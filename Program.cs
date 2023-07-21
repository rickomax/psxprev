﻿using System;
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

            public bool IgnoreTMDVersion { get; set; }

            public bool LogToFile { get; set; }
            public bool NoVerbose { get; set; }
            public bool Debug { get; set; }
            public bool ShowErrors { get; set; }
            public bool NoConsoleColor { get; set; }

            public bool DrawAllToVRAM { get; set; }
            public bool NoOffset { get; set; }
            public bool AutoAttachLimbs { get; set; }
            public bool AutoPlayAnimations { get; set; }
            public bool AutoSelect { get; set; }

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

        public static bool Scanning { get; private set; }
        public static List<RootEntity> AllEntities { get; private set; }
        public static List<Texture> AllTextures { get; private set; }
        public static List<Animation> AllAnimations { get; private set; }

        private static long _largestFileLength;
        private static long _largestCurrentFilePosition;
        private static string _path;
        private static string _filter;
        private static ScanOptions _options = new ScanOptions();


        public static bool IgnoreTmdVersion => _options.IgnoreTMDVersion;
        public static bool Debug => _options.Debug;
        public static bool ShowErrors => _options.ShowErrors;
        public static bool LogToFile => _options.LogToFile;
        public static bool NoVerbose => _options.NoVerbose;
        public static bool NoOffset => _options.NoOffset;


        public const string DefaultFilter = "*.*";

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

        public static volatile bool HaltRequested; //Field used to pause/resume scanning


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
                + " [-an] [-bff] [-croc] [-hmd] [-mod] [-pmd] [-psx] [-tim] [-tmd] [-tod] [-vdf]" // scanner formats
                + " [-ignoretmdversion]" // scanner options
                + " [-log] [-noverbose] [-debug] [-error] [-nocolor]" // log options
                + " [-drawvram] [-nooffset] [-attachlimbs] [-autoplay] [-autoselect]" // program options
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
            Console.WriteLine("scanner options: (default: all formats)");
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
            Console.WriteLine("  -ignoretmdversion : reduce strictness when scanning TMD models");
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
            Console.WriteLine("  -nooffset    : only scan files at offset 0");
            Console.WriteLine("  -attachlimbs : enable Auto Attach Limbs by default");
            Console.WriteLine("  -autoplay    : automatically play selected animations");
            Console.WriteLine("  -autoselect  : select animation's model and draw selected model's textures (HMD only)");

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

        private static bool TryParseOption(string arg, ScanOptions options, ref bool help)
        {
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
                case "-ignoretmdversion":
                    options.IgnoreTMDVersion = true;
                    break;

                case "-log":
                    options.LogToFile = true;
                    break;
                case "-noverbose":
                    options.NoVerbose = true;
                    break;
                case "-debug":
                    options.Debug = true;
                    break;
                case "-error":
                    options.ShowErrors = true;
                    break;
                case "-nocolor":
                    options.NoConsoleColor = true;
                    break;

                case "-drawvram":
                    options.DrawAllToVRAM = true;
                    break;
                case "-nooffset":
                    options.NoOffset = true;
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

                default:
                    return false;
            }
            return true;
        }

        public static void Initialize(string[] args)
        {
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
                    Application.EnableVisualStyles();
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

            if (!help)
            {
                // Parse positional arguments PATH and FILTER.
                path = args[0];
                //if (!Directory.Exists(path) && !File.Exists(path))
                //{
                //    Logger = new Logger();
                //    Program.Logger.WriteErrorLine("Directory/File not found");
                //    if (options.Debug)
                //    {
                //        PressAnyKeyToContinue(); // Make it easier to check console output before closing.
                //    }
                //    return;
                //}

                filter = args.Length > 1 ? args[1] : DefaultFilter;
                // If we want, we can make FILTER truly optional by checking TryParseOption, and skipping FILTER if one was found.
                // However, this would prevent the user from specifying a filter that matches a command line option.
                // This is a pretty unlikely scenario, but it's worth considering.
                //filter = DefaultFilter;
                //if (args.Length > 1 && !TryParseOption(args[1], options, ref help))
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
                    if (!TryParseOption(args[a], options, ref help))
                    {
                        // If we want, we can show some warning or error that an unknown option was passed.
                    }
                }
            }

            // Show help and quit.
            if (help)
            {
                PrintHelp(options.NoConsoleColor);
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
            
            Logger = new Logger(options.LogToFile, !options.NoVerbose, !options.NoConsoleColor);

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Program.Logger.WriteErrorLine($"Directory/File not found: {path}");
                if (options.Debug)
                {
                    PressAnyKeyToContinue(); // Make it easier to check console output before closing.
                }
                return;
            }

            _path = path;
            _filter = filter ?? DefaultFilter;
            _options = options.Clone();

            Scanning = true;

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
                Application.EnableVisualStyles();
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

            Scanning = false;
        }

        private static void UpdateProgress(long filePos, string message)
        {
            if (filePos > _largestCurrentFilePosition)
            {
                _largestCurrentFilePosition = filePos;
            }
            var perc = (double)_largestCurrentFilePosition / _largestFileLength;
            PreviewForm.UpdateProgress((int)(perc * 100), 100, false, message);
        }

        private static void AddEntity(RootEntity entity, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (AllEntities)
            {
                AllEntities.Add(entity);
            }
            UpdateProgress(fp, $"Found Model with {entity.ChildCount} objects");
            PreviewForm.ReloadItems();
        }

        private static void AddTexture(Texture texture, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (AllTextures)
            {
                AllTextures.Add(texture);
            }
            UpdateProgress(fp, $"Found Texture {texture.Width}x{texture.Height} {texture.Bpp}bpp");
            PreviewForm.ReloadItems();
        }

        private static void AddAnimation(Animation animation, long fp)
        {
            // Prevent another thread from enumerating or modifying the list while adding to it.
            lock (AllAnimations)
            {
                AllAnimations.Add(animation);
            }
            UpdateProgress(fp, $"Found Animation with {animation.ObjectCount} objects and {animation.FrameCount} frames");
            PreviewForm.ReloadItems();
        }

        private static void WaitWhileHalted()
        {
            while (HaltRequested)
            {
                // todo: could we use Thread.Yield() here?
                //Thread.Yield();
            }
        }

        private static void ScanFiles()
        {
            var parsers = new List<Action<BinaryReader, string>>();

            if (_options.CheckAll || _options.CheckTIM)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParser(AddTexture);
                    //Program.Logger.WriteLine();
                    timParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckMOD)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var modModelReader = new CrocModelReader(AddEntity);
                    //Program.Logger.WriteLine();
                    modModelReader.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckBFF)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var bffModelReader = new BFFModelReader(AddEntity);
                    //Program.Logger.WriteLine();
                    bffModelReader.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckPSX)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var psxParser = new PSXParser(AddEntity);
                    //Program.Logger.WriteLine();
                    psxParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckTMD)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var tmdParser = new TMDParser(AddEntity);
                    //Program.Logger.WriteLine();
                    tmdParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckVDF)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var vdfParser = new VDFParser(AddAnimation);
                    //Program.Logger.WriteLine();
                    vdfParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckAN)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var anParser = new ANParser(AddAnimation);
                    //Program.Logger.WriteLine();
                    anParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckPMD)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var pmdParser = new PMDParser(AddEntity);
                    //Program.Logger.WriteLine();
                    pmdParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckTOD)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var todParser = new TODParser(AddAnimation);
                    //Program.Logger.WriteLine();
                    todParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_options.CheckAll || _options.CheckHMD)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var hmdParser = new HMDParser(AddEntity, AddAnimation, AddTexture);
                    //Program.Logger.WriteLine();
                    hmdParser.ScanFile(binaryReader, fileTitle);
                });
            }

            if (_path.ToLowerInvariant().EndsWith(".iso"))
            {
                using (var isoStream = File.Open(_path, FileMode.Open))
                {
                    var cdReader = new CDReader(isoStream, true);
                    var files = cdReader.GetFiles("", _filter ?? DefaultFilter, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (HasInvalidExtension(file))
                        {
                            continue;
                        }
                        var fileInfo = cdReader.GetFileInfo(file);
                        if (fileInfo.Exists)
                        {
                            foreach (var parser in parsers)
                            {
                                using (var stream = fileInfo.OpenRead())
                                {
                                    ProcessFile(stream, file, parser);
                                }
                            }
                        }
                        WaitWhileHalted();
                    }
                }
            }
            else if (File.Exists(_path))
            {
                Parallel.ForEach(parsers, parser =>
                {
                    using (var fs = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        ProcessFile(fs, _path, parser);
                    }
                    WaitWhileHalted();
                });
            }
            else
            {
                ProcessFiles(_path, _filter, parsers);
            }
        }

        private static void ProcessFiles(string path, string filter, List<Action<BinaryReader, string>> parsers)
        {
            var files = Directory.GetFiles(path, filter);
            foreach (var file in files)
            {
                if (HasInvalidExtension(file))
                {
                    continue;
                }
                Parallel.ForEach(parsers, parser =>
                {
                    using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        ProcessFile(fs, file, parser);
                    }
                    WaitWhileHalted();
                });
            }
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ProcessFiles(directory, filter, parsers);

                WaitWhileHalted();
            }
        }

        private static bool HasInvalidExtension(string file)
        {
            return file.ToLowerInvariant().EndsWith(".str") || file.ToLowerInvariant().EndsWith(".xa") || file.ToLowerInvariant().EndsWith(".vb") || file.ToLowerInvariant().EndsWith(".str;1") || file.ToLowerInvariant().EndsWith(".xa;1") || file.ToLowerInvariant().EndsWith(".vb;1");
        }

        private static void ProcessFile(Stream stream, string file, Action<BinaryReader, string> parser)
        {
            using (var bs = new BufferedStream(stream))
            {
                using (var binaryReader = new BinaryReader(bs, Encoding.BigEndianUnicode))
                {
                    try
                    {
                        if (stream.Length > _largestFileLength)
                        {
                            _largestFileLength = stream.Length;
                        }
                        var fileTitle = file.Substring(file.LastIndexOf('\\') + 1);
                        parser(binaryReader, fileTitle);
                    }
                    catch (Exception exp)
                    {
                        Program.Logger.WriteExceptionLine(exp, "Error processing file");
                    }
                }
            }
        }

    }
}
