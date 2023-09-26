using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscUtils;
using DiscUtils.Iso9660;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Parsers;
using PSXPrev.Common.Utils;
using PSXPrev.Forms;

namespace PSXPrev
{
    public class Program
    {
        public static string Name = "PSXPrev";
        public static string RootNamespace = "PSXPrev";

        public static readonly Logger Logger = new Logger();
        // For use with non-scanners
        public static readonly Logger ConsoleLogger = new Logger();

        private static PreviewForm PreviewForm;

        private static readonly List<RootEntity> _allEntities = new List<RootEntity>();
        private static readonly List<Texture> _allTextures = new List<Texture>();
        private static readonly List<Animation> _allAnimations = new List<Animation>();
        private static int _scannedEntityCount;
        private static int _scannedTextureCount;
        private static int _scannedAnimationCount;

        // Lock for _currentFileLength and _largestCurrentFilePosition, because longs can't be volatile.
        private static readonly object _fileProgressLock = new object();
        private static readonly Dictionary<FileOffsetScanner, long> _currentParserPositions = new Dictionary<FileOffsetScanner, long>();
        private static long _currentFilePosition; // Farthest position of all the active parsers
        private static long _currentFileLength;
        private static int _currentFileIndex;
        private static int _totalFiles;
        private static int _lastUpdateFileIndex;
        private static ScanOptions _options = new ScanOptions();
        private static ScanOptions _commandLineOptions;
        private static Action<ScanProgressReport> _progressCallback;

        private static volatile bool _scanning;        // Is a scan currently running?
        private static volatile bool _pauseRequested;  // Is the scan currently paused? Reset when _scanning is false.
        private static volatile bool _cancelRequested; // Is the scan being canceled? Reset when _scanning is false.

        public static bool IsScanning => _scanning;
        public static bool IsScanPaused => _pauseRequested;
        public static bool IsScanCanceling => _cancelRequested;

        public static ScanOptions CommandLineOptions => _commandLineOptions;

        public static bool HasCommandLineArguments => _commandLineOptions != null;
        public static bool HasEntityResults
        {
            get { lock (_allEntities) return _allEntities.Count > 0; }
        }
        public static bool HasTextureResults
        {
            get { lock (_allTextures) return _allTextures.Count > 0; }
        }
        public static bool HasAnimationResults
        {
            get { lock (_allAnimations) return _allAnimations.Count > 0; }
        }

        public static bool FixUVAlignment => _options.FixUVAlignment;

        public static bool Debug => _options.DebugLogging;
        public static bool ShowErrors => _options.ErrorLogging;


        // The ";1" is appended to file names in raw PS1 CDs. Extractors may include them.
        private const string BINPostfix = ";1";

        // Ignore movies, audio, and (unsure).
        private static readonly string[] IgnoreFileExtensions =
        {
            ".str", ".str"+BINPostfix, ".xa", ".xa"+BINPostfix, ".vb", ".vb"+BINPostfix,
        };
        private static readonly string[] ISOFileExtensions = { ".iso" };
        private static readonly string[] BINFileExtensions = { ".bin" };


        // This attribute is necessary since PreviewForm now runs on the main thread.
        [STAThread]
        private static void Main(string[] args)
        {
            Initialize(args);
        }

        public static void PrintUsage(bool noColor = false)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ResetColor();
            }

            Console.WriteLine($"usage: PSXPrev <PATH> [FILTER=\"{ScanOptions.DefaultFilter}\"] [-help] [...options]");

            Console.ResetColor();
        }

        public static void PrintHelp(bool noColor = false)
        {
            PrintUsage(noColor);

            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ResetColor();
            }

            Console.WriteLine();
            //Console.WriteLine("positional arguments:");
            Console.WriteLine("arguments:");
            Console.WriteLine("  PATH   : folder or file path to scan");
            Console.WriteLine("  FILTER : wildcard filter for files to include (default: \"" + ScanOptions.DefaultFilter + "\")");
            Console.WriteLine();
            Console.WriteLine("scanner formats: (default: all formats except SPT)");
            Console.WriteLine("  -an        : scan for AN animations");
            Console.WriteLine("  -bff       : scan for BFF models (Blitz Games)");
            Console.WriteLine("  -hmd       : scan for HMD models, textures, and animations");
            Console.WriteLine("  -mod/-croc : scan for MOD (Croc) models");
            Console.WriteLine("  -pmd       : scan for PMD models");
            Console.WriteLine("  -psx       : scan for PSX models and textures (Neversoft)");
            Console.WriteLine("  -spt       : scan for SPT textures (Blitz Games)");
            Console.WriteLine("  -tim       : scan for TIM textures");
            Console.WriteLine("  -tmd       : scan for TMD models");
            Console.WriteLine("  -tod       : scan for TOD animations");
            Console.WriteLine("  -vdf       : scan for VDF animations");
            Console.WriteLine();
            Console.WriteLine("scanner options:");
            Console.WriteLine("  -ignorehmdversion     : less strict scanning of HMD models");
            Console.WriteLine("  -ignoretimversion     : less strict scanning of TIM textures");
            Console.WriteLine("  -ignoretmdversion     : less strict scanning of TMD models");
            Console.WriteLine("  -align <ALIGN>        : scan offsets at specified increments");
            Console.WriteLine("  -start <OFFSET>       : scan files starting at offset (hex)");
            Console.WriteLine("  -stop  <OFFSET>       : scan files up to offset (hex, exclusive)");
            Console.WriteLine("  -range [START],[STOP] : shorthand for [-start <START>] [-stop <STOP>]");
            Console.WriteLine("  -startonly  : shorthand for -stop <START+1>");
            Console.WriteLine("  -nextoffset : continue scan at end of previous match");
            Console.WriteLine("  -regex      : treat FILTER as Regular Expression");
            Console.WriteLine("  -depthlast  : scan files at lower folder depths first");
            Console.WriteLine("  -syncscan   : disable multi-threaded scanning per format");
            Console.WriteLine("  -scaniso    : scan individual files inside .iso files");
            Console.WriteLine("  -scanbin    : scan individual files inside raw PS1 .bin files");
            Console.WriteLine("                not all files may be listed in a .bin file, use -databin as a fallback");
            Console.WriteLine("  -databin    : scan data contents of raw PS1 .bin files");
            Console.WriteLine("  -binsector <START>,<SIZE> : change sector reading of .bin files (default: 24,2048)");
            Console.WriteLine("                              combined values must not exceed " + BinCDStream.SectorRawSize);
            Console.WriteLine();
            Console.WriteLine("log options:");
            Console.WriteLine("  -log       : write output to log file");
            Console.WriteLine("  -debug     : output file format details and other information");
            Console.WriteLine("  -error     : show error (exception) messages when reading files");
            Console.WriteLine("  -nocolor   : disable colored console output");
            Console.WriteLine("  -noverbose/-quiet : don't write output to console");
            Console.WriteLine();
            Console.WriteLine("program options:");
            //Console.WriteLine("  -help        : show this help message"); // It's redundant to display this
            Console.WriteLine("  -drawvram  : draw all loaded textures to VRAM (not advised when scanning many files)");
            Console.WriteLine("  -olduv     : use old UV alignment that less-accurately matches the PlayStation");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("notes:");
            Console.WriteLine("Star Ocean 2 seems to use -binsector 40,2032. However most observed games use the defaut.");

            Console.ResetColor();
        }

        private static void PressAnyKeyToContinue()
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static bool TryParseHelp(string arg)
        {
            switch (arg)
            {
                // Add all -help aliases here
                case "-help":
                    return true;
                default:
                    return false;
            }
        }

        // consumedParameters is number of extra arguments consumed after index.
        private static bool TryParseOption(string[] args, int index, ScanOptions options, ref bool help, out int parameterCount, out bool invalidParameter)
        {
            parameterCount = 0;
            invalidParameter = false;
            var arg = args[index];

            if (options == null)
            {
                // Use dummy options. We're just checking for a valid argument.
                options = new ScanOptions();
            }

            if (TryParseHelp(arg))
            {
                help = true;
                return true;
            }

            switch (arg)
            {
                // Scanner formats:
                case "-an":
                    options.CheckAN = true;
                    break;
                case "-bff":
                    options.CheckBFF = true;
                    break;
                case "-hmd":
                    options.CheckHMD = true;
                    break;
                case "-croc": // Alias for -mod
                case "-mod":  // Previously called -croc
                    options.CheckMOD = true;
                    break;
                case "-pmd":
                    options.CheckPMD = true;
                    break;
                case "-psx":
                    options.CheckPSX = true;
                    break;
                case "-spt":
                    options.CheckSPT = true;
                    break;
                case "-tim":
                    options.CheckTIM = true;
                    break;
                case "-tmd":
                    options.CheckTMD = true;
                    break;
                case "-tod":
                    options.CheckTOD = true;
                    break;
                case "-vdf":
                    options.CheckVDF = true;
                    break;

                // Scanner options:
                case "-ignorehmdversion":
                    options.IgnoreHMDVersion = true;
                    break;
                case "-ignoretimversion":
                    options.IgnoreTIMVersion = true;
                    break;
                case "-ignoretmdversion":
                    options.IgnoreTMDVersion = true;
                    break;

                case "-align":
                    parameterCount++;
                    if (index + 1 < args.Length)
                    {
                        invalidParameter = !TryParseValue(args[index + 1], false, out var align);
                        if (!invalidParameter)
                        {
                            options.Alignment = align;
                            break;
                        }
                    }
                    return false;
                case "-start": // -start <OFFSET>
                    parameterCount++;
                    if (index + 1 < args.Length)
                    {
                        invalidParameter = !TryParseValue(args[index + 1], true, out var startOffset);
                        if (!invalidParameter)
                        {
                            options.StartOffsetHasValue = true;
                            options.StartOffsetValue = startOffset;
                            break;
                        }
                    }
                    return false;
                case "-stop": // -stop <OFFSET>
                    parameterCount++;
                    if (index + 1 < args.Length)
                    {
                        invalidParameter = !TryParseValue(args[index + 1], true, out var stopOffset);
                        if (!invalidParameter)
                        {
                            options.StopOffsetHasValue = true;
                            options.StopOffsetValue = stopOffset;
                            break;
                        }
                    }
                    return false;
                case "-range": // -range [START],[STOP]  Shorthand for -start <START> -stop <STOP>
                    parameterCount++;
                    if (index + 1 < args.Length)
                    {
                        invalidParameter = !TryParseRange(args[index + 1], true, false, out var startRange, out var stopRange);
                        if (!invalidParameter)
                        {
                            options.StartOffsetHasValue = startRange.HasValue;
                            options.StopOffsetHasValue  = stopRange.HasValue;
                            options.StartOffsetValue = startRange ?? options.StartOffsetValue;
                            options.StopOffsetValue  = stopRange  ?? options.StopOffsetValue;
                            break;
                        }
                    }
                    return false;
                case "-startonly": // Shorthand for -stop <START+1>
                    options.StartOffsetOnly = true;
                    break;
                case "-nextoffset":
                    options.NextOffset = true;
                    break;

                case "-regex":
                    options.UseRegex = true;
                    break;
                case "-depthlast":
                    options.TopDownFileSearch = false;
                    break;
                case "-syncscan":
                    options.AsyncFileScan = false;
                    break;
                case "-scaniso":
                    options.ReadISOContents = true;
                    break;
                case "-scanbin":
                    options.ReadBINContents  = true;
                    break;
                case "-databin":
                    options.ReadBINSectorData = true;
                    break;
                case "-binsector":
                    parameterCount++;
                    if (index + 1 < args.Length)
                    {
                        invalidParameter = !TryParseBINSector(args[index + 1], false, out var sectorStart, out var sectorSize);
                        if (!invalidParameter)
                        {
                            options.BINSectorUserStartSizeHasValue = true;
                            options.BINSectorUserStartValue = sectorStart;
                            options.BINSectorUserSizeValue  = sectorSize;
                            break;
                        }
                    }
                    return false;

                // Log options:
                case "-log":
                    options.LogToFile = true;
                    break;
                case "-noverbose":
                case "-quiet":
                    options.LogToConsole = false;
                    break;
                case "-debug":
                    options.DebugLogging = true;
                    break;
                case "-error":
                    options.ErrorLogging = true;
                    break;
                case "-nocolor":
                    options.UseConsoleColor = false;
                    break;

                // Program options:
                case "-drawvram":
                    options.DrawAllToVRAM = true;
                    break;
                case "-olduv":
                    options.FixUVAlignment = false;
                    break;

                default:
                    return false;
            }
            return true;
        }

        private static bool TryParseValue(string text, bool hex, out long offset)
        {
            var style = hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None;
            if (text.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                text = text.Substring(2); // Strip prefix
                // This style has a terrible name. "0x" prefix is illegal and the number is always parsed as hex.
                style = NumberStyles.AllowHexSpecifier; // Hexadecimal integer
            }
            else if (text.StartsWith(".", StringComparison.InvariantCulture))
            {
                // Use the decimal prefix observed in OllyDbg, I'm at a loss for what else could be used...
                text = text.Substring(1); // Strip prefix
                style = NumberStyles.None; // Decimal integer
            }
            return long.TryParse(text, style, CultureInfo.InvariantCulture, out offset);
        }

        private static bool TryParseRange(string text, bool hex, bool require, out long? start, out long? stop)
        {
            //-range BE740       : BE740-BE741
            //-range BE740,      : BE740-end
            //-range ,F580A      : 0-F580A
            //-range BE740,F580A : BE740-F580A
            start = stop = null;
            var param = text.Split(new[] { ',' }, StringSplitOptions.None);
            if (param.Length == 1)
            {
                // Parse a single offset as both the start and stop.
                if (!TryParseValue(param[0], hex, out var offset))
                {
                    return false;
                }
                start = offset;
                stop = offset + 1;
                return true;
            }
            else if (param.Length == 2)
            {
                // Parse a start and/or stop offset.
                // Empty strings are treated as null.
                if (require || !string.IsNullOrEmpty(param[0]))
                {
                    if (!TryParseValue(param[0], hex, out var startOffset))
                    {
                        return false;
                    }
                    start = startOffset;
                }
                if (require || !string.IsNullOrEmpty(param[1]))
                {
                    if (!TryParseValue(param[1], hex, out var stopOffset))
                    {
                        return false;
                    }
                    stop = stopOffset;
                }
                return true;
            }
            return false;
        }

        private static bool TryParseBINSector(string text, bool hex, out int start, out int size)
        {
            //-binsector 24,2048 (default)
            //-binsector 40,2032 (Star Ocean 2)
            start = size = 0;
            var param = text.Split(new[] { ',' }, StringSplitOptions.None);
            if (param.Length == 2)
            {
                // Parse a start and/or stop offset.
                // Empty strings are treated as null.
                if (!TryParseValue(param[0], hex, out var sectorStart))
                {
                    return false;
                }
                start = (int)sectorStart;
                if (!TryParseValue(param[1], hex, out var sectorSize))
                {
                    return false;
                }
                size = (int)sectorSize;
                // Validate sector info so that we don't cause problems parsing the file
                if (sectorStart < 0 || sectorSize <= 0 || sectorStart + sectorSize > BinCDStream.SectorRawSize)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        // Returns true if the program should quit after this.
        private static bool ParseCommandLineOptions(string[] args)
        {
            if (args == null)
            {
                return false; // This was not called from main. Don't do anything or print usage again.
            }
            else if (args.Length == 0)
            {
                // No arguments specified.  Print usage so that the user can either
                // ask for help, or specify what they want without the GUI in the future.
                PrintUsage();
                return false; // No command line arguments
            }

            // Change settings whose defaults differ between the command line and ScannerForm.
            var options = new ScanOptions
            {
                ReadISOContents = false,
            };
            var help = false; // Skip scanning and print the help message.

            // Check if the user is asking for -help in-place of positional arguments.
            for (var a = 0; a < Math.Min(2, args.Length); a++)
            {
                if (TryParseHelp(args[a]))
                {
                    help = true;
                    break;
                }
            }

            string filter = null;
            // Still parse when -help to check for -debug
            //if (!help)
            {
                // Parse positional arguments PATH and FILTER.
                options.Path = args[0];

                if (args.Length > 1)
                {
                    filter = args[1];
                }
                // If we want, we can make FILTER truly optional by checking TryParseOption, and skipping FILTER if one was found.
                // However, this would prevent the user from specifying a filter that matches a command line option.
                // This is a pretty unlikely scenario, but it's worth considering.
                //if (args.Length > 1 && !TryParseOption(args, 1, options, ref help, out _, out _))
                //{
                //    filter = args[1];
                //}


                // Parse all remaining options that aren't PATH or FILTER.
                var startIndex = help ? 0 : 2;
                for (var a = startIndex; a < args.Length; a++)
                {
                    if (!TryParseOption(args, a, options, ref help, out var parameterCount, out var invalidParameter))
                    {
                        if (a + 1 + parameterCount > args.Length)
                        {
                            var missing = (a + 1 + parameterCount) - args.Length;
                            Program.ConsoleLogger.WriteErrorLine($"Missing {missing} parameters for argument: {args[a]}");
                        }
                        else if (invalidParameter)
                        {
                            var paramList = new List<string>();
                            for (var p = 0; p < parameterCount; p++)
                            {
                                paramList.Add(args[a + 1 + p]);
                            }
                            var paramStr = string.Join(" ", paramList);
                            Program.ConsoleLogger.WriteErrorLine($"Invalid parameters for argument: {args[a]} {paramStr}");
                        }
                        else if (a == 1)
                        {
                            // If we want to make filter optional, then handle it here.
                            filter = args[a];
                        }
                        else
                        {
                            // If we want, we can show some warning or error that an unknown option was passed.
                            Program.ConsoleLogger.WriteWarningLine($"Unknown or invalid usage of argument: {args[a]}");
                        }
                    }
                    // Skip consumed extra arguments (parameterCount does not include the base argument).
                    a += parameterCount;
                }
            }

            if (!options.UseRegex)
            {
                options.WildcardFilter = !string.IsNullOrWhiteSpace(filter) ? filter : ScanOptions.EmptyFilter;
            }
            else
            {
                options.RegexPattern = !string.IsNullOrWhiteSpace(filter) ? filter : ScanOptions.DefaultRegexPattern;
            }

            // Show help and quit.
            if (help)
            {
                PrintHelp(!options.UseConsoleColor);
                if (options.DebugLogging)
                {
                    PressAnyKeyToContinue(); // Make it easier to check console output before closing.
                }
                return true; // Quit program after this
            }

            _commandLineOptions = options;
            return false; // Command line arguments, but no help
        }

        public static void Initialize(string[] args)
        {
            Application.EnableVisualStyles();

            Settings.Load(true);
            Logger.UseConsoleColor = Settings.Instance.ScanOptions.UseConsoleColor;
            Logger.ReadSettings(Settings.Instance);
            ConsoleLogger.ReadSettings(Settings.Instance);


            if (ParseCommandLineOptions(args))
            {
                return; // Help command was used, close the program.
            }

            PreviewForm = new PreviewForm();
            Application.Run(PreviewForm);
        }

        public static RootEntity[] GetEntityResults()
        {
            lock (_allEntities)
            {
                return _allEntities.ToArray();
            }
        }

        public static Texture[] GetTextureResults()
        {
            lock (_allTextures)
            {
                return _allTextures.ToArray();
            }
        }

        public static Animation[] GetAnimationResults()
        {
            lock (_allAnimations)
            {
                return _allAnimations.ToArray();
            }
        }

        internal static void ClearResults()
        {
            if (!_scanning)
            {
                _allEntities.Clear();
                _allTextures.Clear();
                _allAnimations.Clear();
            }
        }

        // Returns false if the path argument was not found.
        internal static bool ScanCommandLineAsync(Action<ScanProgressReport> progressCallback = null)
        {
            var options = _commandLineOptions;
            _commandLineOptions = null; // Clear so that HasCommandLineOptions returns false
            return ScanInternal(options, progressCallback, true);
        }

        // Returns false if the path was not found.
        internal static bool ScanAsync(ScanOptions options = null, Action<ScanProgressReport> progressCallback = null)
        {
            return ScanInternal(options, progressCallback, true);
        }

        // Returns false if the path was not found.
        private static bool ScanInternal(ScanOptions options, Action<ScanProgressReport> progressCallback, bool @async)
        {
            if (_scanning)
            {
                return true; // Can't start scan while another is in-progress.
            }

            if (options == null)
            {
                options = new ScanOptions(); // Use default options if none given.
            }

            var oldFixUVAlignment = Program.FixUVAlignment;
            options = options.Clone();
            options.Validate();
            if (HasEntityResults)
            {
                // Force-preserve this option if entities have already been parsed using it.
                options.FixUVAlignment = oldFixUVAlignment;
            }

            Logger.LogToFile = options.LogToFile;
            Logger.LogToConsole = options.LogToConsole;
            Logger.UseConsoleColor = options.UseConsoleColor;
            Logger.ReadSettings(Settings.Instance);

            if (!Directory.Exists(options.Path) && !File.Exists(options.Path))
            {
                Program.ConsoleLogger.WriteErrorLine($"Directory/File not found: {options.Path}");
                return false;
            }
            try
            {
                // Ensure regex pattern is valid
                options.GetRegexFilter(false);
            }
            catch (Exception exp)
            {
                // Message starts as "parsing ...", so prefix with "Error "
                Program.ConsoleLogger.WriteErrorLine($"Invalid filter: Error {exp.Message}");
                return false;
            }

            // Assign parser settings that are no longer stored in Program
            Limits.IgnoreHMDVersion = options.IgnoreHMDVersion;
            Limits.IgnoreTIMVersion = options.IgnoreTIMVersion;
            Limits.IgnoreTMDVersion = options.IgnoreTMDVersion;

            _options = options;
            _progressCallback = progressCallback;
            _scanning = true;
            _pauseRequested = false;
            _cancelRequested = false;
            _currentParserPositions.Clear();
            _currentFilePosition = 0;
            _currentFileLength = 0;
            _currentFileIndex = 0;
            _lastUpdateFileIndex = 0;
            _totalFiles = 0;

            if (@async)
            {
                var thread = new Thread(new ThreadStart(ScanThread));
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
            }
            else
            {
                ScanThread();
            }
            return true;
        }

        private static void ScanThread()
        {
            try
            {
                _progressCallback?.Invoke(new ScanProgressReport
                {
                    State = ScanProgressState.Started,
                });

                //Program.Logger.WriteLine();
                Program.Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                var watch = Stopwatch.StartNew();

                try
                {
                    ScanFiles();
                }
                catch (Exception exp)
                {
                    Program.Logger.WriteExceptionLine(exp, "Error scanning files");
                }

                watch.Stop();
                var hours = (int)watch.Elapsed.TotalHours;
                var minutes = watch.Elapsed.Minutes;
                var seconds = watch.Elapsed.Seconds;
                var milliseconds = watch.Elapsed.Milliseconds;
                //Program.Logger.WriteLine();
                Program.Logger.WriteLine("Scan end {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                var millisecondsStr = string.Empty;
#if DEBUG
                // Always print time taken to console for debug builds, and include milliseconds
                var oldLogToConsole = Program.Logger.LogToConsole;
                Program.Logger.LogToConsole = true;
                millisecondsStr = $" {milliseconds} milliseconds";
#endif
                Program.Logger.WriteLine("Scan took {0} hours {1} minutes {2} seconds{3}", hours, minutes, seconds, millisecondsStr);
#if DEBUG
                Program.Logger.LogToConsole = oldLogToConsole;
#endif
                Program.Logger.WritePositiveLine("Found {0} Models", _scannedEntityCount);
                Program.Logger.WritePositiveLine("Found {0} Textures", _scannedTextureCount);
                Program.Logger.WritePositiveLine("Found {0} Animations", _scannedAnimationCount);

                // Scan finished, perform end-of-scan actions specified by the user.
                _progressCallback?.Invoke(new ScanProgressReport
                {
                    State = ScanProgressState.Finished,
                    CurrentPosition = 0,
                    CurrentLength = 0,
                    CurrentFile = _totalFiles,
                    TotalFiles = _totalFiles,
                });
            }
            catch (Exception exp)
            {
                Program.Logger.WriteExceptionLine(exp, "Error during ScanThread");
            }

            _progressCallback = null; // Nullify to remove references to instanced method object
            _scanning = false;
            _pauseRequested = false;
            _cancelRequested = false;
        }

        private static void ResetFileProgress(bool nextParser)
        {
            var shouldUpdateProgress = false;
            lock (_fileProgressLock)
            {
                if (_currentFilePosition > 1 * 1024 * 1024)
                {
                    // Always update progress if the last scan was over 1MB
                    shouldUpdateProgress = true;
                }

                if (!nextParser)
                {
                    _currentParserPositions.Clear();
                    _currentFilePosition = 0;
                    _currentFileLength = 0;
                    var filesIncrease = _currentFileIndex - _lastUpdateFileIndex;
                    var percentIncrease = (float)filesIncrease / _totalFiles;
                    // Because we've changed how progress is handled, it's now fine to update every file.
                    // Update progress if 20 files or 10% of files scanned since last update
                    // todo: These numbers may need some tweaking...
                    //if (filesIncrease >= 20 || percentIncrease >= 0.10f)
                    {
                        shouldUpdateProgress = true;
                    }
                }
                else
                {
                    _currentFilePosition = 0; // Start of next synchronous parser
                }
            }
            if (shouldUpdateProgress)
            {
                UpdateFileProgress(null, 0, null);
            }
        }

        private static void UpdateFileProgress(FileOffsetScanner scanner, long fp, object result)
        {
            long currentPosition, currentLength;
            int fileIndex, totalFiles;
            lock (_fileProgressLock)
            {
                _lastUpdateFileIndex = _currentFileIndex;

                if (scanner != null)
                {
                    // This is being called by a scanner callback, so update the current file progress.
                    _currentParserPositions[scanner] = fp;
                    var maxfp = _currentParserPositions.Values.Max();
                    if (maxfp != _currentFilePosition || _currentFileIndex != _totalFiles)
                    {
                        _currentFilePosition = maxfp;
                    }
                    else if (result == null)
                    {
                        return; // Position hasn't changed and we didn't find a file, don't update progress
                    }
                }

                // We need to store these as local variables before leaving the lock.
                currentPosition = _currentFilePosition;
                currentLength = Math.Max(1, _currentFileLength); // Max of 1 to prevent showing complete bar before we have a length
                fileIndex = _currentFileIndex;
                totalFiles = _totalFiles;
            }
            _progressCallback?.Invoke(new ScanProgressReport
            {
                State = ScanProgressState.Updated,
                CurrentFile = fileIndex,
                TotalFiles = totalFiles,
                CurrentPosition = currentPosition,
                CurrentLength = currentLength,
                Result = result,
            });
        }

        private static bool AddEntity(FileOffsetScanner scanner, RootEntity entity, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_allEntities)
            {
                _allEntities.Add(entity);
                _scannedEntityCount++;
            }
            UpdateFileProgress(scanner, fp, entity);
            return true;
        }

        private static bool AddTexture(FileOffsetScanner scanner, Texture texture, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_allTextures)
            {
                _allTextures.Add(texture);
                _scannedTextureCount++;
            }
            UpdateFileProgress(scanner, fp, texture);
            return true;
        }

        private static bool AddAnimation(FileOffsetScanner scanner, Animation animation, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_allAnimations)
            {
                _allAnimations.Add(animation);
                _scannedAnimationCount++;
            }
            UpdateFileProgress(scanner, fp, animation);
            return true;
        }

        private static void ProgressCallback(FileOffsetScanner scanner, long fp)
        {
            UpdateFileProgress(scanner, fp, null); // Update progress bar but don't reload items
        }

        internal static bool PauseScan(bool paused)
        {
            if (_scanning)
            {
                if (!paused)
                {
                    _pauseRequested = false;
                }
                else if (!_cancelRequested)
                {
                    _pauseRequested = true; // Cannot pause while scan is canceled.
                }
            }
            return _pauseRequested;
        }

        internal static bool CancelScan()
        {
            if (_scanning)
            {
                _pauseRequested = false; // Prevent waiting in while loop if canceled.
                _cancelRequested = true;
            }
            return _cancelRequested;
        }

        // Returns true if the program has requested to cancel the scan.
        internal static bool WaitOnScanState()
        {
            // Currently the code is written so that _pauseRequested and _cancelRequested
            // will never be true at the same time. Otherwise we could wait in an endless loop,
            // even if we canceled the scan.
            // 
            // If we want to be lazier when managing the state of these two variables then
            // change the while loop condition to: `_pauseRequested && !_cancelRequested`.
            while (_pauseRequested)
            {
                // Give priority to other threads and don't run up the CPU with constant looping.
                // This is fine to use here, because we don't expect _pauseRequested to change frequently.
                // Note that lms will average at least 10ms due to how Sleep works, which is fine.
                Thread.Sleep(1);
            }
            return _cancelRequested;
        }

        private static bool HasFileExtension(string file, string[] extensions)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            return Array.IndexOf(extensions, ext) != -1;
        }

        private static string StripBINPostfix(string file)
        {
            // The ";1" postfix is seen in raw PS1 CD files for all file names.
            // We want to ignore it.
            if (Path.GetExtension(file).EndsWith(BINPostfix))
            {
                return file.Substring(0, file.Length - BINPostfix.Length);
            }
            return file;
        }

        private static bool ShouldIncludeFile(string file, Regex regex)
        {
            if (!HasFileExtension(file, IgnoreFileExtensions))
            {
                return regex?.IsMatch(Path.GetFileName(file)) ?? true;
            }
            return false;
        }

        private static bool ShouldProcessISOContents(string file)
        {
            return _options.ReadISOContents && HasFileExtension(file, ISOFileExtensions);
        }

        private static bool ShouldProcessBINContents(string file)
        {
            return (_options.ReadBINContents || _options.ReadBINSectorData) &&
                HasFileExtension(file, BINFileExtensions) &&
                BinCDStream.IsBINFile(file);
        }

        private static void ScanFiles()
        {
            var parsers = new List<Func<FileOffsetScanner>>();

            // todo: AN produces too many false positives to be enabled by default
            if (_options.CheckAll || _options.CheckAN)
            {
                parsers.Add(() => new ANParser(AddAnimation));
            }
            if (_options.CheckAll || _options.CheckBFF)
            {
                parsers.Add(() => new BFFParser(AddEntity, AddAnimation));
            }
            if (_options.CheckAll || _options.CheckHMD)
            {
                parsers.Add(() => new HMDParser(AddEntity, AddTexture, AddAnimation));
            }
            if (_options.CheckAll || _options.CheckMOD)
            {
                parsers.Add(() => new MODParser(AddEntity));
            }
            if (_options.CheckAll || _options.CheckBFF)
            {
                // For now we're sharing the BFF option, since the same inner format can appear in both (and its the same devs)
                parsers.Add(() => new PILParser(AddEntity, AddAnimation));
            }
            if (_options.CheckAll || _options.CheckPMD)
            {
                parsers.Add(() => new PMDParser(AddEntity));
            }
            if (_options.CheckAll || _options.CheckPSX)
            {
                parsers.Add(() => new PSXParser(AddEntity, AddTexture, AddAnimation));
            }
            // SPT produces too many false positives to be enabled by default
            if (/*_options.CheckAll ||*/ _options.CheckSPT)
            {
                parsers.Add(() => new SPTParser(AddTexture));
            }
            if (_options.CheckAll || _options.CheckTIM)
            {
                parsers.Add(() => new TIMParser(AddTexture));
            }
            if (_options.CheckAll || _options.CheckTMD)
            {
                parsers.Add(() => new TMDParser(AddEntity));
            }
            if (_options.CheckAll || _options.CheckTOD)
            {
                parsers.Add(() => new TODParser(AddAnimation));
            }
            // todo: VDF produces too many false positives
            if (_options.CheckAll || _options.CheckVDF)
            {
                parsers.Add(() => new VDFParser(AddAnimation));
            }

            var regex = _options.GetRegexFilter(true);

            if (File.Exists(_options.Path))
            {
                _totalFiles = 1;
                if (!ProcessFileOrContents(_options.Path, regex, parsers))
                {
                    _currentFileIndex++;
                }
            }
            else
            {
                ProcessDirectoryContents(_options.Path, regex, parsers);
            }
        }

        private static bool ProcessFileOrContents(string file, Regex regex, List<Func<FileOffsetScanner>> parsers)
        {
            try
            {
                if (ShouldProcessISOContents(file))
                {
                    return ProcessISOContents(file, regex, parsers);
                }
                else if (ShouldProcessBINContents(file))
                {
                    return ProcessBINContents(file, regex, parsers);
                }
                else
                {
                    return ProcessFile(file, parsers);
                }
            }
            catch (Exception exp)
            {
                Program.Logger.WriteExceptionLine(exp, $"Error processing file {Path.GetFileName(file)}");
                return false;
            }
        }

        private static bool ProcessISOContents(string isoPath, Regex regex, List<Func<FileOffsetScanner>> parsers)
        {
            using (var isoStream = File.OpenRead(isoPath))
            using (var cdReader = new CDReader(isoStream, true))
            {
                return ProcessCDContents(cdReader, regex, parsers, false);
            }
        }

        private static bool ProcessBINContents(string binPath, Regex regex, List<Func<FileOffsetScanner>> parsers)
        {
            if (_options.ReadBINContents)
            {
                try
                {
                    // Process indexed files in the BIN file.
                    // Although we *can* do this, we can't rely on it, because the only files
                    // required to be indexed are SYSTEM.CNF and the SLUS...whatever file.
                    // For example, Chrono Cross only indexes those two files.
                    var rawSize    = BinCDStream.SectorRawSize;
                    var userStart  = _options.BINSectorUserStart;
                    var userSize   = _options.BINSectorUserSize;
                    using (var binStream = new BinCDStream(File.OpenRead(binPath), 0, rawSize, userStart, userSize))
                    using (var cdReader = new CDReader(binStream, true))
                    {
                        return ProcessCDContents(cdReader, regex, parsers, true);
                    }
                }
                catch
                {
                    if (!_options.ReadBINSectorData)
                    {
                        throw; // Not reading BIN data, throw an error.
                    }
                    // Can't read CD contents, fallback to reading as single file.
                }
            }

            if (_options.ReadBINSectorData)
            {
                Stream OpenBINFile(string file)
                {
                    var firstIndex = BinCDStream.SectorsFirstIndex;
                    var rawSize    = BinCDStream.SectorRawSize;
                    var userStart  = _options.BINSectorUserStart;
                    var userSize   = _options.BINSectorUserSize;
                    // Wrap around BufferedStream here, since WrapBufferedStream also wraps around the position
                    // cache stream. We don't want that, since the returned stream already does the same thing.
                    return new BinCDStream(File.OpenRead(file), firstIndex, rawSize, userStart, userSize);
                }

                // Process all data of the BIN file.
                return ProcessFile(binPath, binPath, parsers, true, OpenBINFile);
            }

            return false;
        }

        private static bool ProcessCDContents(CDReader cdReader, Regex regex, List<Func<FileOffsetScanner>> parsers, bool isBin)
        {
            Stream OpenDiscFileInfo(DiscFileInfo fileInfo)
            {
                // Wrap around BufferedStream here, since WrapBufferedStream also wraps around the position
                // cache stream. We don't want that, since the returned stream already does the same thing.
                return fileInfo.OpenRead();
            }

            var files = cdReader.GetFiles("");
            var fileInfoList = new List<DiscFileInfo>();
            foreach (var file in files)
            {
                var fullName = isBin ? StripBINPostfix(file) : file;

                var fileInfo = cdReader.GetFileInfo(file);
                // fileInfo.Exists is here for a reason (unsure what that reason was)
                if (ShouldIncludeFile(fullName, regex) && fileInfo.Exists)
                {
                    fileInfoList.Add(fileInfo);
                }
            }

            _totalFiles += fileInfoList.Count; // Use += in-case this isn't the only selected path
            foreach (var fileInfo in fileInfoList)
            {
                var fullName = isBin ? StripBINPostfix(fileInfo.FullName) : fileInfo.FullName;

                // False to disable async processing, since one underlying stream is being used to read the CD.
                if (ProcessFile(fileInfo, fullName, parsers, false, OpenDiscFileInfo))
                {
                    return true; // Canceled
                }
                _currentFileIndex++;
            }

            // Implementation to support depth-last file search in CD files.
#if false
            // Avoid recursion and just use a stack/queue for directories to process. This will give cleaner stack traces.
            var directoryList = new List<string> { "" };
            var fileInfoList = new List<DiscFileInfo>();

            while (directoryList.Count > 0)
            {
                var path = directoryList[0]; // Pop/Dequeue
                directoryList.RemoveAt(0);

                var directoryInfo = cdReader.GetDirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    continue;
                }

                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    var fullName = isBin ? StripBINPostfix(fileInfo.FullName) : fileInfo.FullName;

                    // fileInfo.Exists is here for a reason (unsure what that reason was)
                    if (ShouldIncludeFile(fullName, regex) && fileInfo.Exists)
                    {
                        fileInfoList.Add(fileInfo);
                    }
                }

                if (WaitOnScanState())
                {
                    return true; // Canceled
                }

                var directories = directoryInfo.GetDirectories(); // PushRange/EnqueueRange
                for (var i = 0; i < directories.Length; i++)
                {
                    if (_options.TopDownFileSearch)
                    {
                        directoryList.Insert(i, directories[i].FullName);
                    }
                    else
                    {
                        directoryList.Add(directories[i].FullName);
                    }
                }
            }

            _totalFiles += fileInfoList.Count; // Use += in-case this isn't the only selected path
            foreach (var fileInfo in fileInfoList)
            {
                var fullName = isBin ? StripBINPostfix(fileInfo.FullName) : fileInfo.FullName;

                // False to disable async processing, since one underlying stream is being used to read the CD.
                if (ProcessFile(fileInfo, fullName, parsers, false, false, OpenDiscFileInfo))
                {
                    return true; // Canceled
                }
                _currentFileIndex++;
            }
#endif
            return false;
        }

        private static bool ProcessDirectoryContents(string basePath, Regex regex, List<Func<FileOffsetScanner>> parsers)
        {
            // Note: We can also just use SearchOption.AllDirectories as the third argument to GetFiles,
            // but that might be slow if there are A LOT of files to get. And we can't use EnumerateFiles
            // because the enumerator can throw UnauthorizedAccessException for individual files, which is really stupid.

            // Avoid recursion and just use a stack/queue for directories to process.
            // This will give cleaner stack traces, and make it easier to cancel the scan.
            var directoryList = new List<string> { basePath };
            var fileList = new List<string>();

            while (directoryList.Count > 0)
            {
                var path = directoryList[0]; // Pop/Dequeue
                directoryList.RemoveAt(0);

                foreach (var file in Directory.GetFiles(path))
                {
                    if (ShouldIncludeFile(file, regex))
                    {
                        fileList.Add(file);
                    }
                }

                if (WaitOnScanState())
                {
                    return true; // Canceled
                }

                var directories = Directory.GetDirectories(path); // PushRange/EnqueueRange
                if (_options.TopDownFileSearch)
                {
                    directoryList.InsertRange(0, directories);
                }
                else
                {
                    directoryList.AddRange(directories);
                }
            }

            _totalFiles += fileList.Count; // Use += in-case this isn't the only selected path
            foreach (var file in fileList)
            {
                if (ProcessFileOrContents(file, regex, parsers))
                {
                    return true; // Canceled
                }
                _currentFileIndex++;
            }
            return false;
        }

        private static bool ProcessFile(string file, List<Func<FileOffsetScanner>> parsers)
        {
            Stream OpenFile(string filePath)
            {
                return new FilePositionCacheStream(File.OpenRead(filePath));
            }

            return ProcessFile(file, file, parsers, true, OpenFile);
        }

        private static bool ProcessFile<TFile>(TFile fileInfo, string file, List<Func<FileOffsetScanner>> parsers, bool @async, Func<TFile, Stream> openFile)
        {
            ResetFileProgress(false); // Start of file
            if (@async && _options.AsyncFileScan && parsers.Count > 1)
            {
                if (WaitOnScanState())
                {
                    return true; // Canceled
                }
                Parallel.ForEach(parsers, parser => {
                    // Open individual streams for asynchronous scanning
                    using (var fs = openFile(fileInfo))
                    using (var stream = new BufferedStream(fs))
                    {
                        ScanFile(stream, file, parser);
                    }
                });
            }
            else
            {
                // Re-use one stream if we're doing synchronous scanning
                using (var fs = openFile(fileInfo))
                using (var stream = new BufferedStream(fs))
                {
                    foreach (var parser in parsers)
                    {
                        ResetFileProgress(true); // Start of next parser
                        if (WaitOnScanState())
                        {
                            return true; // Canceled
                        }
                        stream.Seek(0, SeekOrigin.Begin);
                        ScanFile(stream, file, parser);
                    }
                }
                /*foreach (var parser in parsers)
                {
                    ResetFileProgress(true); // Start of next parser
                    if (WaitOnScanState())
                    {
                        return true; // Canceled
                    }
                    using (var fs = openFile(fileInfo))
                    using (var stream = new BufferedStream(fs))
                    {
                        ScanFile(stream, file, parser);
                    }
                }*/
            }
            return false;
        }

        private static void ScanFile(Stream stream, string file, Func<FileOffsetScanner> parser)
        {
            using (var reader = new BinaryReader(stream, Encoding.BigEndianUnicode, true))
            //using (var fos = new FileOffsetStream(stream, true))
            //using (var reader = new BinaryReader(fos, Encoding.BigEndianUnicode, true))
            {
                var scanner = parser();
                try
                {
                    lock (_fileProgressLock)
                    {
                        // Add this position to the dictionary so its included in the max
                        _currentParserPositions.Add(scanner, 0);
                        if (stream.Length > _currentFileLength)
                        {
                            _currentFileLength = stream.Length;
                        }
                    }

                    var fileTitle = Path.GetFileName(file);

                    // Setup scanner settings
                    // In the future we could move Debug, ShowErrors, and Logger into the scanner.
                    scanner.StartOffset = _options.StartOffset;
                    scanner.StopOffset  = _options.StopOffset;
                    scanner.StopOffset  = _options.StartOffsetOnly ? _options.StartOffset + 1 : _options.StopOffset;
                    scanner.NextOffset  = _options.NextOffset;
                    scanner.Alignment   = _options.Alignment;

                    scanner.ProgressCallback = ProgressCallback;
                    scanner.BytesPerProgress = 1 * 1024 * 1024; // 1MB

                    scanner.ScanFile(reader, fileTitle);
                }
                catch (Exception exp)
                {
                    Program.Logger.WriteExceptionLine(exp, $"Error processing file for {scanner.FormatName} scanner");
                }
                finally
                {
                    lock (_fileProgressLock)
                    {
                        // Remove this position from the dictionary so its not included in the max
                        _currentParserPositions.Remove(scanner);
                    }
                    try
                    {
                        scanner.Dispose();
                    }
                    catch (Exception exp)
                    {
                        // It's pretty bad if we're erroring during disposal, but handle it anyway.
                        if (Program.Debug || Program.ShowErrors)
                        {
                            Program.Logger.WriteExceptionLine(exp, $"Error disposing of {scanner.FormatName} scanner");
                        }
                    }
                }
            }
        }


        public enum ScanProgressState
        {
            Started,
            Updated,
            Finished,
        }

        public class ScanProgressReport
        {
            public ScanProgressState State { get; set; }
            // Updated and Finished state
            public int CurrentFile { get; set; }
            public int TotalFiles { get; set; }
            public long CurrentPosition { get; set; }
            public long CurrentLength { get; set; }
            // Updated state
            public object Result { get; set; } // RootEntity, Texture, Animation, or null
            public RootEntity Entity => Result as RootEntity;
            public Texture Texture => Result as Texture;
            public Animation Animation => Result as Animation;
        }

        // Stream that remembers its position to avoid expensive calls to Position getter and setter.
        // Yes, this really speeds things up by a lot (when wrapped around BufferedStream).
        private sealed class FilePositionCacheStream : Stream
        {
            private Stream _stream;
            private readonly bool _leaveOpen;
            private readonly long _length;
            private long _position; // Position to seek to before reads
            private long _streamPosition; // Last position of underlying stream

            public FilePositionCacheStream(Stream stream, bool leaveOpen = false)
            {
                _stream = stream ?? throw new ArgumentNullException(nameof(stream));
                _leaveOpen = leaveOpen;
                if (!stream.CanRead || !stream.CanSeek)
                {
                    throw new ArgumentException("Stream must be able to Read and Seek", nameof(stream));
                }

                _length = stream.Length;
                _streamPosition = _position = stream.Position;
            }


            public override long Position
            {
                get => _position;
                set => Seek(value, SeekOrigin.Begin);
            }

            public override long Length => _length;

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanWrite => false;


            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Current:
                        offset += _position;
                        break;
                    case SeekOrigin.End:
                        offset += _length;
                        break;
                }
                if (offset < 0)
                {
                    throw new IOException("An attempt was made to move the file pointer before the beginning of the file.");
                }
                _position = offset;
                return offset;
            }

            public override int ReadByte()
            {
                PrepareForRead();
                var value = _stream.ReadByte();
                if (value != -1)
                {
                    _position++;
                    _streamPosition = _position;
                }
                return value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                PrepareForRead();
                var bytesRead = _stream.Read(buffer, offset, count);
                _position += bytesRead;
                _streamPosition = _position;
                return bytesRead;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException("Stream is not writable");
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException("Stream is not writable");
            }

            public override void Flush()
            {
            }


            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing && !_leaveOpen && _stream != null)
                    {
                        _stream.Close();
                    }
                }
                finally
                {
                    _stream = null;
                    base.Dispose(disposing);
                }
            }


            private void PrepareForRead()
            {
                if (_streamPosition != _position)
                {
                    _streamPosition = _position = _stream.Seek(_position, SeekOrigin.Begin);
                }
            }
        }
    }
}
