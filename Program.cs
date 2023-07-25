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
using PSXPrev.Classes;
using PSXPrev.Forms;

namespace PSXPrev
{
    public class Program
    {
        public class ScanOptions
        {
            // Scanner formats:
            public bool CheckAll => !CheckAN && !CheckBFF && !CheckHMD && !CheckMOD && !CheckPMD && !CheckPSX && !CheckTIM && !CheckTMD && !CheckTOD && !CheckVDF;

            public bool CheckAN { get; set; }
            public bool CheckBFF { get; set; }
            public bool CheckHMD { get; set; }
            public bool CheckMOD { get; set; } // Previously called Croc
            public bool CheckPMD { get; set; }
            public bool CheckPSX { get; set; }
            public bool CheckTIM { get; set; }
            public bool CheckTMD { get; set; }
            public bool CheckTOD { get; set; }
            public bool CheckVDF { get; set; }

            // Scanner options:
            public bool IgnoreHMDVersion { get; set; }
            public bool IgnoreTIMVersion { get; set; }
            public bool IgnoreTMDVersion { get; set; }

            public long? StartOffset { get; set; }
            public long? StopOffset { get; set; }
            public bool NextOffset { get; set; }

            public bool DepthFirstFileSearch { get; set; } = true; // AKA top-down
            public bool AsyncFileScan { get; set; } = true;

            // Log options:
            public bool LogToFile { get; set; }
            public bool LogToConsole { get; set; } = true;
            public bool Debug { get; set; }
            public bool ShowErrors { get; set; }
            public bool ConsoleColor { get; set; } = true;

            // Program options:
            public bool DrawAllToVRAM { get; set; }
            public bool AutoAttachLimbs { get; set; }
            public bool AutoPlayAnimations { get; set; }
            public bool AutoSelect { get; set; }
            public bool FixUVAlignment { get; set; } = true;

            public ScanOptions Clone()
            {
                return (ScanOptions)MemberwiseClone();
            }
        }

        public static Logger Logger;
        // Volatile because these are assigned and accessed in different threads.
        private static volatile PreviewForm PreviewForm;
        private static volatile LauncherForm LauncherForm;
        // Wait handles to make sure that forms can be constructed and assigned in their respective thread before continuing execution.
        private static AutoResetEvent _waitForPreviewForm = new AutoResetEvent(false);
        private static AutoResetEvent _waitForLauncherForm = new AutoResetEvent(false);

        public static List<RootEntity> AllEntities { get; private set; }
        public static List<Texture> AllTextures { get; private set; }
        public static List<Animation> AllAnimations { get; private set; }

        // Lock for _currentFileLength and _largestCurrentFilePosition, because longs can't be volatile.
        private static readonly object _fileProgressLock = new object();
        private static long _currentFileLength;
        private static long _largestCurrentFilePosition;
        private static string _path;
        private static string _filter;
        private static ScanOptions _options = new ScanOptions();

        private static volatile bool _scanning;        // Is a scan currently running?
        private static volatile bool _pauseRequested;  // Is the scan currently paused? Reset when _scanning is false.
        private static volatile bool _cancelRequested; // Is the scan being canceled? Reset when _scanning is false.

        public static bool IsScanning => _scanning;
        public static bool IsScanPaused => _pauseRequested;
        public static bool IsScanCanceling => _cancelRequested;

        public static bool IgnoreHmdVersion => _options.IgnoreHMDVersion;
        public static bool IgnoreTimVersion => _options.IgnoreTIMVersion;
        public static bool IgnoreTmdVersion => _options.IgnoreTMDVersion;
        public static bool FixUVAlignment => _options.FixUVAlignment;

        public static bool Debug => _options.Debug;
        public static bool ShowErrors => _options.ShowErrors;


        public const string DefaultFilter = "*.*";

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
        public static ulong MaxHMDMIMEeDiffs = 100;
        public static ulong MaxHMDMIMEeOriginals = 100;
        public static ulong MaxHMDVertices = 5000;
        public static ulong MaxMODModels = 1000;
        public static ulong MaxMODVertices = 10000;
        public static ulong MaxMODFaces = 10000;
        public static uint MaxANJoints = 512;
        public static uint MaxANFrames = 5000;


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

            Console.WriteLine("usage: PSXPrev <PATH> [FILTER=\"" + DefaultFilter + "\"] [-help]"  // general
                + " [...options]" // Just use -help to see the rest
                //+ " [-an] [-bff] [-croc] [-hmd] [-mod] [-pmd] [-psx] [-tim] [-tmd] [-tod] [-vdf]" // scanner formats
                //+ " [-ignorehmdversion] [-ignoretimversion] [-ignoretmdversion]" // scanner options
                //+ " [-start <OFFSET>] [-stop <OFFSET>] [-range [START],[STOP]] [-nooffset] [-nextoffset] [-syncscan]"
                //+ " [-log] [-noverbose] [-debug] [-error] [-nocolor]" // log options
                //+ " [-drawvram] [-nooffset] [-attachlimbs] [-autoplay] [-autoselect]" // program options
                );

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
            Console.WriteLine("  FILTER : wildcard filter for files to include (default: \"" + DefaultFilter + "\")");
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
            Console.WriteLine("  -drawvram    : draw all loaded textures to VRAM (not advised when scanning many files)");
            Console.WriteLine("  -attachlimbs : enable Auto Attach Limbs by default");
            Console.WriteLine("  -autoplay    : automatically play selected animations");
            Console.WriteLine("  -autoselect  : select animation's model and draw selected model's textures (HMD only)");
            //Console.WriteLine("  -fixuv       : fix UV alignment to closely match that on the PlayStation");
            Console.WriteLine("  -olduv       : use old UV alignment that less-accurately matches the PlayStation");

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
                    options.ConsoleColor = false;
                    break;

                // Program options:
                case "-drawvram":
                    options.DrawAllToVRAM = true;
                    break;
                case "-attachlimbs":
                    options.AutoAttachLimbs = true;
                    break;
                case "-autoplay":
                    options.AutoPlayAnimations = true;
                    break;
                case "-autoselect":
                    options.AutoSelect = true;
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

        public static void Initialize(string[] args)
        {
            Application.EnableVisualStyles();
            if (args == null || args.Length == 0)
            {
                // No arguments specified. Show the launcher window and let the user choose what to do in the GUI.
                // Also print usage so that the user can either ask for help, or specify what they want without the GUI in the future.
                PrintUsage();
                
                var thread = new Thread(new ThreadStart(delegate
                {
                    LauncherForm = new LauncherForm();
                    LauncherForm.HandleCreated += (sender, e) => {
                        // InvokeRequired won't return true unless the form's handle has been created.
                        _waitForLauncherForm.Set(); // LauncherForm has been assigned and is setup, let the main thread continue.
                    };
                    Application.Run(LauncherForm);
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                _waitForLauncherForm.WaitOne(); // Wait for LauncherForm to be assigned before continuing.
                return;
            }

            string path = null;
            string filter = null;
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

            // Use a default logger before starting a scan.
            Logger = new Logger();

            if (!help)
            {
                // Parse positional arguments PATH and FILTER.
                path = args[0];

                filter = args.Length > 1 ? args[1] : DefaultFilter;
                // If we want, we can make FILTER truly optional by checking TryParseOption, and skipping FILTER if one was found.
                // However, this would prevent the user from specifying a filter that matches a command line option.
                // This is a pretty unlikely scenario, but it's worth considering.
                //filter = DefaultFilter;
                //if (args.Length > 1 && !TryParseOption(args, 1, options, ref help, out _))
                //{
                //    filter = args[1];
                //}
                if (string.IsNullOrEmpty(filter))
                {
                    filter = "*"; // When filter is empty, default to matching all files (with or without an extension).
                }


                // Parse all remaining options that aren't PATH or FILTER.
                for (var a = 2; a < args.Length; a++)
                {
                    if (!TryParseOption(args, a, options, ref help, out var consumed))
                    {
                        if (a == 1)
                        {
                            // If we want to make filter optional, then handle it here.
                            filter = args[a];
                            if (string.IsNullOrEmpty(filter))
                            {
                                filter = "*"; // When filter is empty, default to matching all files (with or without an extension).
                            }
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
                PrintHelp(!options.ConsoleColor);
                if (options.Debug)
                {
                    PressAnyKeyToContinue(); // Make it easier to check console output before closing.
                }
                return;
            }

            DoScan(path, filter, options);
        }

        internal static void DoScan(string path, string filter = null, ScanOptions options = null)
        {
            if (options == null)
            {
                options = new ScanOptions(); // Use default options if none given.
            }
            
            Logger = new Logger(options.LogToFile, options.LogToConsole, options.ConsoleColor);

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Program.Logger.WriteErrorLine($"Directory/File not found: {path}");
                if (options.Debug && options.LogToConsole)
                {
                    PressAnyKeyToContinue(); // Make it easier to check console output before closing.
                }
                return;
            }

            _path = path;
            _filter = filter ?? DefaultFilter;
            _options = options.Clone();

            _scanning = true;
            _pauseRequested = false;
            _cancelRequested = false;

            AllEntities = new List<RootEntity>();
            AllTextures = new List<Texture>();
            AllAnimations = new List<Animation>();


            var thread = new Thread(new ThreadStart(delegate
            {
                PreviewForm = new PreviewForm((form) =>
                {
                    // Prevent another thread from adding to the lists while enumerating them.
                    lock (AllAnimations)
                    {
                        form.UpdateAnimations(AllAnimations);
                    }
                    lock (AllEntities)
                    {
                        form.UpdateRootEntities(AllEntities);
                    }
                    lock (AllTextures)
                    {
                        form.UpdateTextures(AllTextures);
                    }
                });
                PreviewForm.HandleCreated += (sender, e) => {
                    // InvokeRequired won't return true unless the form's handle has been created.
                    _waitForPreviewForm.Set(); // PreviewForm has been assigned and is setup, let the main thread continue.
                };
                Application.Run(PreviewForm);
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _waitForPreviewForm.WaitOne(); // Wait for PreviewForm to be assigned before continuing.


            // Assign default preview settings.
            PreviewForm.SetAutoAttachLimbs(_options.AutoAttachLimbs);
            PreviewForm.SetAutoPlayAnimations(_options.AutoPlayAnimations);
            PreviewForm.SetAutoSelectAnimationModel(_options.AutoSelect);
            PreviewForm.SetAutoDrawModelTextures(_options.AutoSelect);

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
                Program.Logger.WritePositiveLine("Found {0} Models", AllEntities.Count);
                Program.Logger.WritePositiveLine("Found {0} Textures", AllTextures.Count);
                Program.Logger.WritePositiveLine("Found {0} Animations", AllAnimations.Count);

                PreviewForm.UpdateProgress(0, 0, true, $"{AllEntities.Count} Models, {AllTextures.Count} Textures, {AllAnimations.Count} Animations Found");

                // Scan finished, perform end-of-scan actions specified by the user.
                PreviewForm.SelectFirstEntity(); // Select something if the user hasn't already done so.
                if (_options.DrawAllToVRAM)
                {
                    PreviewForm.DrawAllTexturesToVRAM();
                }
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
            lock (AllEntities)
            {
                AllEntities.Add(entity);
            }
            UpdateFileProgress(fp, $"Found Model with {entity.ChildCount} objects");
            PreviewForm.ReloadItems();
        }

        private static void AddTexture(Texture texture, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (AllTextures)
            {
                AllTextures.Add(texture);
            }
            UpdateFileProgress(fp, $"Found Texture {texture.Width}x{texture.Height} {texture.Bpp}bpp");
            PreviewForm.ReloadItems();
        }

        private static void AddAnimation(Animation animation, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (AllAnimations)
            {
                AllAnimations.Add(animation);
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
                parsers.Add(() => new BFFModelReader(AddEntity));
            }
            if (_options.CheckAll || _options.CheckHMD)
            {
                parsers.Add(() => new HMDParser(AddEntity, AddAnimation, AddTexture));
            }
            if (_options.CheckAll || _options.CheckMOD)
            {
                parsers.Add(() => new CrocModelReader(AddEntity));
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

            if (HasISOExtension(_path))
            {
                ProcessISO(_path, _filter, parsers);
            }
            else if (File.Exists(_path))
            {
                ProcessFile(_path, parsers);
            }
            else
            {
                ProcessFiles(_path, _filter, parsers);
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
            if (_options.AsyncFileScan)
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
