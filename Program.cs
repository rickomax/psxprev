using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        // Results to be added to PreviewForm on next refresh
        private static readonly List<RootEntity> _addedEntities = new List<RootEntity>();
        private static readonly List<Texture> _addedTextures = new List<Texture>();
        private static readonly List<Animation> _addedAnimations = new List<Animation>();

        private static readonly List<RootEntity> _allEntities = new List<RootEntity>();
        private static readonly List<Texture> _allTextures = new List<Texture>();
        private static readonly List<Animation> _allAnimations = new List<Animation>();

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

        private static volatile bool _scanning;        // Is a scan currently running?
        private static volatile bool _pauseRequested;  // Is the scan currently paused? Reset when _scanning is false.
        private static volatile bool _cancelRequested; // Is the scan being canceled? Reset when _scanning is false.

        public static bool IsScanning => _scanning;
        public static bool IsScanPaused => _pauseRequested;
        public static bool IsScanCanceling => _cancelRequested;

        public static bool HasCommandLineArguments => _commandLineOptions != null;
        public static bool HasEntityResults => _allEntities.Count > 0;
        public static bool HasTextureResults => _allTextures.Count > 0;
        public static bool HasAnimationResults => _allAnimations.Count > 0;

        public static bool FixUVAlignment => _options.FixUVAlignment;

        public static bool Debug => _options.DebugLogging;
        public static bool ShowErrors => _options.ErrorLogging;


        private static readonly string[] InvalidFileExtensions = { ".str", ".str;1", ".xa", ".xa;1", ".vb", ".vb;1" };
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
            Console.WriteLine("scanner formats: (default: all formats)");
            Console.WriteLine("  -an        : scan for AN animations");
            Console.WriteLine("  -bff       : scan for BFF models");
            Console.WriteLine("  -hmd       : scan for HMD models, textures, and animations");
            Console.WriteLine("  -mod/-croc : scan for MOD (Croc) models");
            Console.WriteLine("  -pmd       : scan for PMD models");
            Console.WriteLine("  -psx       : scan for PSX models (just another format)");
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
            Console.WriteLine("  -depthlast  : scan files at lower folder depths first");
            Console.WriteLine("  -syncscan   : disable multi-threaded scanning per format");
            Console.WriteLine("  -scaniso    : scan contents of .iso files");
            Console.WriteLine("  -scanbin    : scan contents of raw PS1 .bin files (experimental)");
            Console.WriteLine("  -binalign   : scan .bin file offsets at sector size increments");
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
                    options.ReadBINContents = true;
                    break;
                case "-binalign":
                    options.BINAlignToSector = true;
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

            // Still parse when -help to check for -debug
            //if (!help)
            {
                // Parse positional arguments PATH and FILTER.
                options.Path = args[0];

                options.Filter = args.Length > 1 ? args[1] : ScanOptions.DefaultFilter;
                // If we want, we can make FILTER truly optional by checking TryParseOption, and skipping FILTER if one was found.
                // However, this would prevent the user from specifying a filter that matches a command line option.
                // This is a pretty unlikely scenario, but it's worth considering.
                //options.Filter = ScanOptions.DefaultFilter;
                //if (args.Length > 1 && !TryParseOption(args, 1, options, ref help, out _, out _))
                //{
                //    options.Filter = args[1];
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
                            options.Filter = args[a];
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

            Settings.Load();
            Logger.UseConsoleColor = Settings.Instance.ScanOptions.UseConsoleColor;
            Logger.ReadSettings(Settings.Instance);
            ConsoleLogger.ReadSettings(Settings.Instance);


            if (ParseCommandLineOptions(args))
            {
                return; // Help command was used, close the program.
            }

            PreviewForm = new PreviewForm(RefreshPreviewForm);
            Application.Run(PreviewForm);
        }

        private static void RefreshPreviewForm(PreviewForm form)
        {
            // Prevent another thread from adding to the lists while enumerating them.
            lock (_addedEntities)
            {
                form.AddRootEntities(_addedEntities);
                _allEntities.AddRange(_addedEntities);
                _addedEntities.Clear();
            }
            lock (_addedTextures)
            {
                form.AddTextures(_addedTextures);
                _allTextures.AddRange(_addedTextures);
                _addedTextures.Clear();
            }
            lock (_addedAnimations)
            {
                form.AddAnimations(_addedAnimations);
                _allAnimations.AddRange(_addedAnimations);
                _addedAnimations.Clear();
            }
        }

        public static RootEntity[] GetEntityResults()
        {
            lock (_addedEntities)
            {
                return _allEntities.ToArray();
            }
        }

        public static Texture[] GetTextureResults()
        {
            lock (_addedTextures)
            {
                return _allTextures.ToArray();
            }
        }

        public static Animation[] GetAnimationResults()
        {
            lock (_addedAnimations)
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
        internal static bool ScanCommandLineAsync()
        {
            var options = _commandLineOptions;
            _commandLineOptions = null; // Clear so that HasCommandLineOptions returns false
            return ScanInternal(options, true);
        }

        // Returns false if the path was not found.
        internal static bool ScanAsync(ScanOptions options = null)
        {
            return ScanInternal(options, true);
        }

        // Returns false if the path was not found.
        private static bool ScanInternal(ScanOptions options, bool @async)
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

            // Assign parser settings that are no longer stored in Program
            Limits.IgnoreHMDVersion = options.IgnoreHMDVersion;
            Limits.IgnoreTIMVersion = options.IgnoreTIMVersion;
            Limits.IgnoreTMDVersion = options.IgnoreTMDVersion;

            _options = options;
            _scanning = true;
            _pauseRequested = false;
            _cancelRequested = false;
            _currentParserPositions.Clear();
            _currentFilePosition = 0;
            _currentFileLength = 0;
            _currentFileIndex = 0;
            _lastUpdateFileIndex = 0;
            _totalFiles = 0;

            _addedEntities.Clear();
            _addedTextures.Clear();
            _addedAnimations.Clear();

            if (@async)
            {
                var thread = new Thread(new ThreadStart(ScanThread));
                thread.SetApartmentState(ApartmentState.STA);
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
                PreviewForm?.ScanStarted();

                //Program.Logger.WriteLine();
                Program.Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                var watch = Stopwatch.StartNew();

                ScanFiles();

                watch.Stop();
                var hours = (int)watch.Elapsed.TotalHours;
                var minutes = watch.Elapsed.Minutes;
                var seconds = watch.Elapsed.Seconds;
                //Program.Logger.WriteLine();
                Program.Logger.WriteLine("Scan end {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Program.Logger.WriteLine("Scan took {0} hours {1} minutes {2} seconds", hours, minutes, seconds);
                Program.Logger.WritePositiveLine("Found {0} Models", _allEntities.Count);
                Program.Logger.WritePositiveLine("Found {0} Textures", _allTextures.Count);
                Program.Logger.WritePositiveLine("Found {0} Animations", _allAnimations.Count);

                // Scan finished, perform end-of-scan actions specified by the user.
                PreviewForm?.ScanFinished(_options.DrawAllToVRAM);
            }
            catch (Exception exp)
            {
                Program.Logger.WriteExceptionLine(exp, "Error scanning files");
            }

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
                    // Update progress if 20 files or 10% of files scanned since last update
                    // todo: These numbers may need some tweaking...
                    if (filesIncrease >= 20 || percentIncrease >= 0.10f)
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

        private static void UpdateFileProgress(FileOffsetScanner scanner, long fp, string message)
        {
            var reloadItems = message != null; // ReloadItems in the same invoke call as ScanUpdated
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
                    if (maxfp != _currentFilePosition)
                    {
                        _currentFilePosition = maxfp;
                    }
                    else if (!reloadItems && message == null)
                    {
                        return; // Position hasn't changed and we didn't find a file, don't update progress
                    }
                }

                // We need to store these as local variables before leaving the lock.
                currentPosition = _currentFilePosition;
                currentLength = _currentFileLength;
                fileIndex = _currentFileIndex;
                totalFiles = _totalFiles;
            }
            PreviewForm?.ScanUpdated(currentPosition, currentLength, fileIndex, totalFiles, message, reloadItems);
        }

        private static void AddEntity(FileOffsetScanner scanner, RootEntity entity, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_addedEntities)
            {
                _addedEntities.Add(entity);
            }
            UpdateFileProgress(scanner, fp, $"Found {scanner.FormatName} Model with {entity.ChildCount} objects");
        }

        private static void AddTexture(FileOffsetScanner scanner, Texture texture, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_addedTextures)
            {
                _addedTextures.Add(texture);
            }
            UpdateFileProgress(scanner, fp, $"Found {scanner.FormatName} Texture {texture.Width}x{texture.Height} {texture.Bpp}bpp");
        }

        private static void AddAnimation(FileOffsetScanner scanner, Animation animation, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_addedAnimations)
            {
                _addedAnimations.Add(animation);
            }
            UpdateFileProgress(scanner, fp, $"Found {scanner.FormatName} Animation with {animation.ObjectCount} objects and {animation.FrameCount} frames");
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

        private static bool ShouldIncludeFile(string file)
        {
            return !HasFileExtension(file, InvalidFileExtensions);
        }

        private static bool ShouldProcessISOContents(string file)
        {
            return _options.ReadISOContents && HasFileExtension(file, ISOFileExtensions);
        }

        private static bool ShouldProcessBINContents(string file)
        {
            return _options.ReadBINContents && HasFileExtension(file, BINFileExtensions) &&
                BinCDStream.IsBINFile(file);
        }

        private static void ScanFiles()
        {
            var parsers = new List<Func<FileOffsetScanner>>();

            if (_options.CheckAll || _options.CheckAN)
            {
                parsers.Add(() => new ANParser(AddAnimation));
            }
            if (_options.CheckAll || _options.CheckBFF)
            {
                parsers.Add(() => new BFFParser(AddEntity));
            }
            if (_options.CheckAll || _options.CheckHMD)
            {
                parsers.Add(() => new HMDParser(AddEntity, AddTexture, AddAnimation));
            }
            if (_options.CheckAll || _options.CheckMOD)
            {
                parsers.Add(() => new MODParser(AddEntity));
            }
            if (_options.CheckAll || _options.CheckPMD)
            {
                parsers.Add(() => new PMDParser(AddEntity));
            }
            if (_options.CheckAll || _options.CheckPSX)
            {
                parsers.Add(() => new PSXParser(AddEntity));
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
            if (_options.CheckAll || _options.CheckVDF)
            {
                parsers.Add(() => new VDFParser(AddAnimation));
            }

            if (File.Exists(_options.Path))
            {
                _totalFiles = 1;
                if (!ProcessFileOrContents(_options.Path, _options.Filter, parsers))
                {
                    _currentFileIndex++;
                }
            }
            else
            {
                ProcessDirectoryContents(_options.Path, _options.Filter, parsers);
            }
        }

        private static bool ProcessFileOrContents(string file, string filter, List<Func<FileOffsetScanner>> parsers)
        {
            if (ShouldProcessISOContents(file))
            {
                return ProcessISOContents(file, filter, parsers);
            }
            else if (ShouldProcessBINContents(file))
            {
                return ProcessBINContents(file, filter, parsers);
            }
            else
            {
                return ProcessFile(file, parsers);
            }
        }

        private static bool ProcessISOContents(string isoPath, string filter, List<Func<FileOffsetScanner>> parsers)
        {
            Stream OpenDiscFileInfo(DiscFileInfo fileInfo)
            {
                return fileInfo.OpenRead();
            }

            using (var isoStream = File.OpenRead(isoPath))
            using (var cdReader = new CDReader(isoStream, true))
            {
                var files = cdReader.GetFiles("", filter, SearchOption.AllDirectories);
                var fileInfoList = new List<DiscFileInfo>();
                foreach (var file in files)
                {
                    var fileInfo = cdReader.GetFileInfo(file);
                    // fileInfo.Exists is here for a reason (unsure what that reason was)
                    if (ShouldIncludeFile(file) && fileInfo.Exists)
                    {
                        fileInfoList.Add(fileInfo);
                    }
                }

                _totalFiles += fileInfoList.Count; // Use += in-case this isn't the only selected path
                foreach (var fileInfo in fileInfoList)
                {
                    // False to disable async processing, since one underlying stream is being used to read the ISO.
                    if (ProcessFile(fileInfo, fileInfo.FullName, parsers, false, true, false, OpenDiscFileInfo))
                    {
                        return true; // Canceled
                    }
                    _currentFileIndex++;
                }

                // Implementation to support depth-last file search in ISO files.
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

                    foreach (var fileInfo in directoryInfo.GetFiles(filter))
                    {
                        // fileInfo.Exists is here for a reason (unsure what that reason was)
                        if (ShouldIncludeFile(fileInfo.FullName) && fileInfo.Exists)
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
                    // False to disable async processing, since one underlying stream is being used to read the ISO.
                    if (ProcessFile(fileInfo.FullName, parsers, false, fileInfo, OpenDiscFileInfo))
                    {
                        return true; // Canceled
                    }
                    _currentFileIndex++;
                }
#endif
            }
            return false;
        }

        private static bool ProcessBINContents(string binPath, string filter, List<Func<FileOffsetScanner>> parsers)
        {
            Stream OpenBINFile(string file)
            {
                var firstIndex = BinCDStream.SectorsFirstIndex;
                var rawSize   = BinCDStream.SectorRawSize;
                var userStart = _options.BINSectorUserStart;
                var userSize  = _options.BINSectorUserSize;
                return new BinCDStream(File.OpenRead(file), firstIndex, rawSize, userStart, userSize);
            }

            // Not sure how to read BIN file index yet, so just read the entire thing as one file...
            return ProcessFile(binPath, binPath, parsers, true, true, true, OpenBINFile);
        }

        private static bool ProcessDirectoryContents(string basePath, string filter, List<Func<FileOffsetScanner>> parsers)
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

                foreach (var file in Directory.GetFiles(path, filter))
                {
                    if (ShouldIncludeFile(file))
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
                if (ProcessFileOrContents(file, filter, parsers))
                {
                    return true; // Canceled
                }
                _currentFileIndex++;
            }
            return false;
        }

        private static bool ProcessFile(string file, List<Func<FileOffsetScanner>> parsers)
        {
            return ProcessFile(file, file, parsers, true, true, false, File.OpenRead);
        }

        private static bool ProcessFile<TFile>(TFile fileInfo, string file, List<Func<FileOffsetScanner>> parsers, bool @async, bool buffered, bool isBin, Func<TFile, Stream> openFile)
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
                    using (var bs = buffered ? new BufferedStream(fs) : null)
                    {
                        var stream = bs ?? fs;
                        ScanFile(stream, file, isBin, parser);
                    }
                });
            }
            else
            {
                // Re-use one stream if we're doing synchronous scanning
                using (var fs = openFile(fileInfo))
                using (var bs = buffered ? new BufferedStream(fs) : null)
                {
                    var stream = bs ?? fs;
                    foreach (var parser in parsers)
                    {
                        ResetFileProgress(true); // Start of next parser
                        if (WaitOnScanState())
                        {
                            return true; // Canceled
                        }
                        stream.Seek(0, SeekOrigin.Begin);
                        ScanFile(stream, file, isBin, parser);
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
                    using (var bs = buffered ? new BufferedStream(fs) : null)
                    {
                        var stream = bs ?? fs;
                        ScanFile(stream, file, isBin, parser);
                    }
                }*/
            }
            return false;
        }

        private static void ScanFile(Stream stream, string file, bool isBin, Func<FileOffsetScanner> parser)
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
                    if (isBin && _options.BINAlignToSector)
                    {
                        scanner.Alignment = _options.BINSectorUserSize;
                    }

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

    }
}
