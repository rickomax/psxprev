using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
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
        private static long _currentFileLength;
        private static long _largestCurrentFilePosition;
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

        public static bool IgnoreHmdVersion => _options.IgnoreHMDVersion;
        public static bool IgnoreTimVersion => _options.IgnoreTIMVersion;
        public static bool IgnoreTmdVersion => _options.IgnoreTMDVersion;
        public static bool FixUVAlignment => _options.FixUVAlignment;

        public static bool Debug => _options.Debug;
        public static bool ShowErrors => _options.ShowErrors;


        private static readonly string[] InvalidFileExtensions = { ".str", ".str;1", ".xa", ".xa;1", ".vb", ".vb;1" };
        private static readonly string[] ISOFileExtensions = { ".iso" };

        // Sanity check values
        public static ulong MaxTODPackets = 10000;
        public static ulong MaxTODFrames = 10000;
        public static ulong MaxTMDPrimitives = 10000;
        public static ulong MaxTMDObjects = 10000;
        public static ulong MaxTIMResolution = 1024;
        public static ulong MinVDFFrames = 3;
        public static ulong MaxVDFFrames = 512;
        public static ulong MaxVDFVertices = 1024;
        public static ulong MaxVDFObjects = 512;
        public static ulong MaxPSXObjectCount = 1024;
        public static ulong MaxHMDBlockCount = 1024;
        public static ulong MaxHMDCoordCount = 1024; // Same as BlockCount, because they're related
        public static ulong MaxHMDTypeCount = 1024;
        public static ulong MaxHMDDataSize = 20000;
        public static ulong MaxHMDDataCount = 5000;
        public static ulong MaxHMDPrimitiveChainLength = 512;
        public static ulong MaxHMDHeaderLength = 100;
        public static ulong MinHMDStripMeshLength = 1;
        public static ulong MaxHMDStripMeshLength = 1024;
        public static ulong MaxHMDAnimSequenceSize = 20000;
        public static ulong MaxHMDAnimSequenceCount = 1024;
        public static ulong MaxHMDAnimInstructions = ushort.MaxValue + 1; // Hard cap
        public static ulong MaxHMDMIMeDiffs = 100;
        public static ulong MaxHMDMIMeOriginals = 100;
        public static ulong MaxHMDVertices = 5000;
        public static ulong MaxMODModels = 1000;
        public static ulong MaxMODVertices = 10000;
        public static ulong MaxMODFaces = 10000;
        public static uint MaxANJoints = 512;
        public static uint MaxANFrames = 5000;


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
            //Console.WriteLine("  -start [OFFSET]       : scan files starting at offset (hex)");
            //Console.WriteLine("  -stop [OFFSET]        : scan files up to offset (hex)");
            Console.WriteLine("  -range [START],[STOP] : scan files between offsets (hex)");
            Console.WriteLine("  -nooffset             : alias for -range 0,1");
            Console.WriteLine("  -nextoffset           : continue scan at end of previous match");
            Console.WriteLine("  -syncscan             : disable multi-threaded scanning");
            Console.WriteLine();
            Console.WriteLine("log options:");
            Console.WriteLine("  -log       : write output to log file");
            Console.WriteLine("  -noverbose : don't write output to console");
            Console.WriteLine("  -debug     : output file format details and other information");
            Console.WriteLine("  -error     : show error (exception) messages when reading files");
            Console.WriteLine("  -nocolor   : disable colored console output");
            Console.WriteLine();
            Console.WriteLine("program options:");
            //Console.WriteLine("  -help        : show this help message"); // It's redundant to display this
            Console.WriteLine("  -drawvram  : draw all loaded textures to VRAM (not advised when scanning many files)");
            Console.WriteLine("  -olduv     : use old UV alignment that less-accurately matches the PlayStation");

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

        // Consumed is number of extra arguments consumed after index.
        private static bool TryParseOption(string[] args, int index, ScanOptions options, ref bool help, out int consumed)
        {
            consumed = 0;
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

                /*case "-start": // -start <OFFSET>
                    if (index + 1 < args.Length)
                    {
                        consumed++;
                        if (TryParseOffset(args[index + 1], out var startOffset))
                        {
                            options.StartOffset = startOffset;
                        }
                        break;
                    }
                    return false;
                case "-stop": // -stop <OFFSET>
                    if (index + 1 < args.Length)
                    {
                        consumed++;
                        if (TryParseOffset(args[index + 1], out var stopOffset))
                        {
                            options.StopOffset = stopOffset;
                        }
                        break;
                    }
                    return false;*/
                case "-range": // -range [START],[STOP]  Shorthand for -start <START> -stop <STOP>
                    if (index + 1 < args.Length)
                    {
                        consumed++;
                        if (TryParseRange(args[index + 1], out var start, out var stop))
                        {
                            options.StartOffset = start;
                            options.StopOffset  = stop;
                        }
                        break;
                    }
                    return false;
                case "-nooffset": // Shorthand for -range 0,1
                    options.StartOffset = 0;
                    options.StopOffset  = 1;
                    break;
                case "-nextoffset":
                    options.NextOffset = true;
                    break;

                case "-syncscan":
                    options.AsyncFileScan = false;
                    break;

                // Log options:
                case "-log":
                    options.LogToFile = true;
                    break;
                case "-noverbose":
                    options.LogToConsole = false;
                    break;
                case "-debug":
                    options.Debug = true;
                    break;
                case "-error":
                    options.ShowErrors = true;
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

        private static bool TryParseOffset(string text, out long offset)
        {
            // This style has a terrible name. "0x" prefix is illegal and the number is always parsed as hex.
            var style = NumberStyles.AllowHexSpecifier; // Only accept offsets as hexadecimal integer
            if (text.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                text = text.Substring(2); // Strip prefix
            }
            else if (text.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
            {
                // Use the decimal prefix observed in OllyDbg, I'm at a loss for what else could be used...
                text = text.Substring(1); // Strip prefix
                style = NumberStyles.None; // Decimal integer
            }
            return long.TryParse(text, style, CultureInfo.InvariantCulture, out offset);
        }

        private static bool TryParseRange(string text, out long? start, out long? stop)
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
                if (!TryParseOffset(param[0], out var offset))
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
                if (!string.IsNullOrEmpty(param[0]))
                {
                    if (!TryParseOffset(param[0], out var startOffset))
                    {
                        return false;
                    }
                    start = startOffset;
                }
                if (!string.IsNullOrEmpty(param[1]))
                {
                    if (!TryParseOffset(param[1], out var stopOffset))
                    {
                        return false;
                    }
                    stop = stopOffset;
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

            var options = new ScanOptions();
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

            if (!help)
            {
                // Parse positional arguments PATH and FILTER.
                options.Path = args[0];

                options.Filter = args.Length > 1 ? args[1] : ScanOptions.DefaultFilter;
                // If we want, we can make FILTER truly optional by checking TryParseOption, and skipping FILTER if one was found.
                // However, this would prevent the user from specifying a filter that matches a command line option.
                // This is a pretty unlikely scenario, but it's worth considering.
                //options.Filter = ScanOptions.DefaultFilter;
                //if (args.Length > 1 && !TryParseOption(args, 1, options, ref help, out _))
                //{
                //    options.Filter = args[1];
                //}


                // Parse all remaining options that aren't PATH or FILTER.
                for (var a = 2; a < args.Length; a++)
                {
                    if (!TryParseOption(args, a, options, ref help, out var consumed))
                    {
                        if (a == 1)
                        {
                            // If we want to make filter optional, then handle it here.
                            options.Filter = args[a];
                        }
                        else
                        {
                            // If we want, we can show some warning or error that an unknown option was passed.
                            Program.Logger.WriteWarningLine($"Unknown or invalid usage of argument: {args[a]}");
                        }
                    }
                    // Skip consumed extra arguments (consumed count does not include the base argument).
                    a += consumed;
                }
            }

            // Show help and quit.
            if (help)
            {
                PrintHelp(!options.UseConsoleColor);
                if (options.Debug)
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
                Program.Logger.WriteErrorLine($"Directory/File not found: {options.Path}");
                return false;
            }

            _options = options;
            _scanning = true;
            _pauseRequested = false;
            _cancelRequested = false;

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
                //Program.Logger.WriteLine();
                Program.Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                var watch = Stopwatch.StartNew();

                ScanFiles();

                //Program.Logger.WriteLine();
                watch.Stop();
                Program.Logger.WriteLine("Scan end {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Program.Logger.WriteLine("Scan took {0} minutes and {1} seconds", (int)watch.Elapsed.TotalMinutes, watch.Elapsed.Seconds);
                Program.Logger.WritePositiveLine("Found {0} Models", _allEntities.Count);
                Program.Logger.WritePositiveLine("Found {0} Textures", _allTextures.Count);
                Program.Logger.WritePositiveLine("Found {0} Animations", _allAnimations.Count);

                PreviewForm.UpdateProgress(0, 0, true, $"{_allEntities.Count} Models, {_allTextures.Count} Textures, {_allAnimations.Count} Animations Found");

                // Scan finished, perform end-of-scan actions specified by the user.
                PreviewForm.ScanFinished(_options.DrawAllToVRAM);
                //PreviewForm.SelectFirstEntity(); // Select something if the user hasn't already done so.
                //if (_options.DrawAllToVRAM)
                //{
                //    PreviewForm.DrawAllTexturesToVRAM();
                //}
            }
            catch (Exception exp)
            {
                Program.Logger.WriteExceptionLine(exp, "Error scanning files");
            }

            _scanning = false;
            _pauseRequested = false;
            _cancelRequested = false;
        }

        private static void ResetFileProgress(bool nextParser = false)
        {
            lock (_fileProgressLock)
            {
                _largestCurrentFilePosition = 0;
                _currentFileLength = 0;
            }
            if (!nextParser)
            {
                //PreviewForm.UpdateProgress(0, 100, false, "File Started");
            }
        }

        private static void UpdateFileProgress(long filePos, string message)
        {
            var percent = 1d;
            lock (_fileProgressLock)
            {
                if (filePos > _largestCurrentFilePosition)
                {
                    _largestCurrentFilePosition = filePos;
                }
                // Avoid divide-by-zero when scanning a file that's 0 bytes in size.
                if (_currentFileLength > 0)
                {
                    percent = (double)_largestCurrentFilePosition / _currentFileLength;
                }
            }
            PreviewForm.UpdateProgress((int)(percent * 100), 100, false, message);
        }

        private static void AddEntity(RootEntity entity, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_addedEntities)
            {
                _addedEntities.Add(entity);
            }
            UpdateFileProgress(fp, $"Found Model with {entity.ChildCount} objects");
            PreviewForm.ReloadItems();
        }

        private static void AddTexture(Texture texture, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_addedTextures)
            {
                _addedTextures.Add(texture);
            }
            UpdateFileProgress(fp, $"Found Texture {texture.Width}x{texture.Height} {texture.Bpp}bpp");
            PreviewForm.ReloadItems();
        }

        private static void AddAnimation(Animation animation, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (_addedAnimations)
            {
                _addedAnimations.Add(animation);
            }
            UpdateFileProgress(fp, $"Found Animation with {animation.ObjectCount} objects and {animation.FrameCount} frames");
            PreviewForm.ReloadItems();
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

        private static bool HasInvalidExtension(string file)
        {
            return HasFileExtension(file, InvalidFileExtensions);
        }

        private static bool HasISOExtension(string file)
        {
            return HasFileExtension(file, ISOFileExtensions);
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

            if (HasISOExtension(_options.Path))
            {
                ProcessISO(_options.Path, _options.Filter, parsers);
            }
            else if (File.Exists(_options.Path))
            {
                ProcessFile(_options.Path, parsers);
            }
            else
            {
                ProcessFiles(_options.Path, _options.Filter, parsers);
            }
        }

        private static bool ProcessISO(string isoPath, string filter, List<Func<FileOffsetScanner>> parsers)
        {
            using (var isoStream = File.OpenRead(isoPath))
            using (var cdReader = new CDReader(isoStream, true))
            {
                var files = cdReader.GetFiles("", filter, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fileInfo = cdReader.GetFileInfo(file);
                    // fileInfo.Exists is here for a reason (unsure what that reason was)
                    if (HasInvalidExtension(file) || !fileInfo.Exists)
                    {
                        continue;
                    }
                    ResetFileProgress();

                    foreach (var parser in parsers)
                    {
                        ResetFileProgress(true);
                        if (WaitOnScanState())
                        {
                            return true; // Canceled
                        }
                        using (var fs = fileInfo.OpenRead())
                        {
                            ScanFile(fs, file, parser);
                        }
                    }
                }

                // Implementation to support depth-last file search in ISO files.
#if false
                // Avoid recursion and just use a stack/queue for directories to process. This will give cleaner stack traces.
                var directoryList = new List<string> { "" };

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
                        var file = fileInfo.FullName;
                        if (HasInvalidExtension(file) || !fileInfo.Exists)
                        {
                            continue;
                        }
                        ResetFileProgress();

                        foreach (var parser in parsers)
                        {
                            ResetFileProgress(true);
                            if (WaitOnScanState())
                            {
                                return true; // Canceled
                            }
                            using (var stream = fileInfo.OpenRead())
                            {
                                ScanFile(stream, file, parser);
                            }
                        }
                    }

                    if (WaitOnScanState())
                    {
                        return true; // Canceled
                    }

                    var directories = directoryInfo.GetDirectories(); // PushRange/EnqueueRange
                    for (var i = 0; i < directories.Length; i++)
                    {
                        if (_options.DepthFirstFileSearch)
                        {
                            directoryList.Insert(i, directories[i].FullName);
                        }
                        else
                        {
                            directoryList.Add(directories[i].FullName);
                        }
                    }
                }
#endif
            }
            return false;
        }

        private static bool ProcessFiles(string basePath, string filter, List<Func<FileOffsetScanner>> parsers)
        {
            // Note: We can also just use SearchOption.AllDirectories as the third argument to GetFiles,
            // but that might be slow if there are A LOT of files to get. And we can't use EnumerateFiles
            // because the enumerator can throw UnauthorizedAccessException for individual files, which is really stupid.

            // Avoid recursion and just use a stack/queue for directories to process.
            // This will give cleaner stack traces, and make it easier to cancel the scan.
            var directoryList = new List<string> { basePath };

            while (directoryList.Count > 0)
            {
                var path = directoryList[0]; // Pop/Dequeue
                directoryList.RemoveAt(0);

                foreach (var file in Directory.GetFiles(path, filter))
                {
                    if (HasInvalidExtension(file))
                    {
                        continue;
                    }
                    // If we want, we could add support to process ISOs in directories.
                    /*if (HasISOExtension(file))
                    {
                        if (ProcessISO(file, filter, parsers))
                        {
                            return true; // Canceled
                        }
                    }
                    else*/ if (ProcessFile(file, parsers))
                    {
                        return true; // Canceled
                    }
                }

                if (WaitOnScanState())
                {
                    return true; // Canceled
                }

                var directories = Directory.GetDirectories(path); // PushRange/EnqueueRange
                if (_options.DepthFirstFileSearch)
                {
                    directoryList.InsertRange(0, directories);
                }
                else
                {
                    directoryList.AddRange(directories);
                }
            }
            return false;
        }

        private static bool ProcessFile(string file, List<Func<FileOffsetScanner>> parsers)
        {
            ResetFileProgress();
            if (_options.AsyncFileScan && parsers.Count > 1)
            {
                if (WaitOnScanState())
                {
                    return true; // Canceled
                }
                Parallel.ForEach(parsers, parser => {
                    using (var fs = File.OpenRead(file))
                    {
                        ScanFile(fs, file, parser);
                    }
                });
            }
            else
            {
                foreach (var parser in parsers)
                {
                    ResetFileProgress(true);
                    if (WaitOnScanState())
                    {
                        return true; // Canceled
                    }
                    using (var fs = File.OpenRead(file))
                    {
                        ScanFile(fs, file, parser);
                    }
                }
            }
            return false;
        }

        private static void ScanFile(Stream stream, string file, Func<FileOffsetScanner> parser)
        {
            using (var bs = new BufferedStream(stream))
            using (var reader = new BinaryReader(bs, Encoding.BigEndianUnicode))
            //using (var fileOffsetStream = new FileOffsetStream(bs))
            //using (var reader = new BinaryReader(fileOffsetStream, Encoding.BigEndianUnicode))
            {
                var scanner = parser();
                try
                {
                    lock (_fileProgressLock)
                    {
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
                    scanner.NextOffset  = _options.NextOffset;

                    scanner.ScanFile(reader, fileTitle);
                }
                catch (Exception exp)
                {
                    Program.Logger.WriteExceptionLine(exp, $"Error processing file for {scanner.FormatName} scanner");
                }
                finally
                {
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
