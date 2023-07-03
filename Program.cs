using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using PSXPrev.Forms;
using System.Threading.Tasks;
using System.Text;
using DiscUtils;
using DiscUtils.Iso9660;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class Program
    {
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
        private static bool _checkAll;
        private static string _path;
        private static bool _checkTmd;
        private static bool _checkVdf;
        private static bool _checkTim;
        private static bool _checkPmd;
        private static bool _checkTod;
        private static bool _checkHmd;
        private static bool _checkCroc;
        private static bool _checkPsx;
        private static bool _checkAn;
        private static bool _checkBff;

        public static bool IgnoreTmdVersion;
        public static bool Debug;
        public static bool Log;
        public static bool NoVerbose;
        public static string Filter;

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
        public static ulong MaxHMDTypeCount = 1024;
        public static ulong MaxHMDDataSize = 5000;
        public static ulong MaxHMDMimeDiffs = 100;
        public static ulong MaxHMDVertCount = 5000;
        public static uint MaxANJoints = 512;
        public static uint MaxANFrames = 5000;

        private static void Main(string[] args)
        {
            Initialize(args);
        }

        public static void Initialize(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Usage PSXPrev folder filter(optional) -tmd(optional) -vdf(optional) -pmd(optional) -tim(optional) -tod(optional) -an(optional) -hmd(optional) -croc(optional) -psx(optional) -log(optional) -noverbose(optional) -debug(optional) -ignoretmdversion(optional) -bff(optional)");

                var thread = new Thread(new ThreadStart(delegate
                {
                    LauncherForm = new LauncherForm();
                    _waitForLauncherForm.Set(); // LauncherForm has been assigned, let the main thread continue.
                    Application.EnableVisualStyles();
                    Application.Run(LauncherForm);
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                _waitForLauncherForm.WaitOne(); // Wait for LauncherForm to be assigned before continuing.
                return;
            }

            var path = args[0];
            //if (!Directory.Exists(path) && !File.Exists(path))
            //{
            //    Program.Logger.WriteErrorLine("Directory/File not found");
            //    return;
            //}

            var filter = args.Length > 1 ? args[1] : "*.*";

            var checkTmd = false;
            var checkVdf = false;
            var checkTim = false;
            var checkPmd = false;
            var checkTod = false;
            var checkAn = false;
            var checkHmdModels = false;
            var checkCrocModels = false;
            var checkPsx = false;
            var checkBff = false;
            var log = false;
            var noVerbose = false;
            var debug = false;
            var ignoreTmdVersion = false;

            for (var a = 2; a < args.Length; a++)
            {
                switch (args[a])
                {
                    case "-tmd":
                        checkTmd = true;
                        break;
                    case "-vdf":
                        checkVdf = true;
                        break;
                    case "-pmd":
                        checkPmd = true;
                        break;
                    case "-tim":
                        checkTim = true;
                        break;
                    case "-tod":
                        checkTod = true;
                        break;
                    case "-an":
                        checkAn = true;
                        break;
                    case "-hmd":
                        checkHmdModels = true;
                        break;
                    case "-log":
                        log = true;
                        break;
                    case "-noverbose":
                        noVerbose = true;
                        break;
                    case "-debug":
                        debug = true;
                        break;
                    case "-croc":
                        checkCrocModels = true;
                        break;
                    case "-psx":
                        checkPsx = true;
                        break;
                    case "-ignoretmdversion":
                        ignoreTmdVersion = true;
                        break;
                    case "-bff":
                        checkBff = true;
                        break;
                }
            }
            DoScan(path, filter, checkTmd, checkVdf, checkTim, checkPmd, checkTod, checkHmdModels, log, noVerbose, debug, checkCrocModels, checkPsx, checkAn, ignoreTmdVersion, checkBff);
        }

        internal static void DoScan(string path, string filter, bool checkTmd, bool checkVdf, bool checkTim, bool checkPmd, bool checkTod, bool checkHmd, bool log, bool noVerbose, bool debug, bool checkCroc, bool checkPsx, bool checkAn, bool ignoreTmdVersion, bool checkBff)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Logger.WriteErrorLine("Directory/File not found");
                return;
            }

            Scanning = true;

            Logger = new Logger(log, noVerbose);

            _checkAll = !(checkTmd || checkVdf || checkTim || checkPmd || checkTod || checkHmd || checkCroc || checkPsx || checkAn || checkBff);
            _path = path;
            Filter = filter;
            _checkTmd = checkTmd;
            _checkVdf = checkVdf;
            _checkTim = checkTim;
            //_checkTimAlt = checkTimAlt;
            _checkPmd = checkPmd;
            _checkTod = checkTod;
            _checkHmd = checkHmd;
            _checkCroc = checkCroc;
            _checkPsx = checkPsx;
            _checkAn = checkAn;
            _checkBff = checkBff;

            Log = log;
            NoVerbose = noVerbose;

            IgnoreTmdVersion = ignoreTmdVersion;
            Debug = debug;

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
                _waitForPreviewForm.Set(); // PreviewForm has been assigned, let the main thread continue.
                Application.EnableVisualStyles();
                Application.Run(PreviewForm);
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _waitForPreviewForm.WaitOne(); // Wait for PreviewForm to be assigned before continuing.

            try
            {
                //Program.Logger.WriteLine("");
                Program.Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                ScanFiles();

                //Program.Logger.WriteLine("");
                Program.Logger.WriteLine("Scan End {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Program.Logger.WritePositiveLine("Found {0} Models", AllEntities.Count);
                Program.Logger.WritePositiveLine("Found {0} Textures", AllTextures.Count);
                Program.Logger.WritePositiveLine("Found {0} Animations", AllAnimations.Count);

                PreviewForm.UpdateProgress(0, 0, true, $"{AllEntities.Count} Models, {AllTextures.Count} Textures, {AllAnimations.Count} Animations Found");
            }
            catch (Exception exp)
            {
                Program.Logger.WriteErrorLine(exp);
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

        private static void ScanFiles()
        {
            var parsers = new List<Action<BinaryReader, string>>();

            if (_checkAll || _checkTim)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParser(AddTexture);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for TIM at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkCroc)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var crocModelReader = new CrocModelReader(AddEntity);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for Croc at file {0}", fileTitle);
                    crocModelReader.LookForCrocModel(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkBff)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var bffModelReader = new BFFModelReader(AddEntity);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for BFF at file {0}", fileTitle);
                    bffModelReader.LookForBFF(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkPsx)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var psxParser = new PSXParser(AddEntity);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for PSX at file {0}", fileTitle);
                    psxParser.LookForPSX(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkTmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var tmdParser = new TMDParser(AddEntity);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for TMD at file {0}", fileTitle);
                    tmdParser.LookForTmd(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkVdf)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var vdfParser = new VDFParser(AddAnimation);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for VDF at file {0}", fileTitle);
                    vdfParser.LookForVDF(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkAn)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var anParser = new ANParser(AddAnimation);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for AN at file {0}", fileTitle);
                    anParser.LookForAN(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkPmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var pmdParser = new PMDParser(AddEntity);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for PMD at file {0}", fileTitle);
                    pmdParser.LookForPMD(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkTod)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var todParser = new TODParser(AddAnimation);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for TOD at file {0}", fileTitle);
                    todParser.LookForTOD(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkHmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var hmdParser = new HMDParser(AddEntity, AddAnimation, AddTexture);
                    //Program.Logger.WriteLine("");
                    Program.Logger.WriteLine("Scanning for HMD at file {0}", fileTitle);
                    hmdParser.LookForHMDEntities(binaryReader, fileTitle);
                });
            }

            if (_path.ToLowerInvariant().EndsWith(".iso"))
            {
                using (var isoStream = File.Open(_path, FileMode.Open))
                {
                    var cdReader = new CDReader(isoStream, true);
                    var files = cdReader.GetFiles("", Filter ?? "*.*", SearchOption.AllDirectories);
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
                });
            }
            else
            {
                ProcessFiles(_path, Filter, parsers);
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
                });
            }
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ProcessFiles(directory, filter, parsers);
            }
        }

        private static bool HasInvalidExtension(string file)
        {
            return file.ToLowerInvariant().EndsWith(".str") || file.ToLowerInvariant().EndsWith(".xa") || file.ToLowerInvariant().EndsWith(".vb");
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
                        Program.Logger.WriteErrorLine(exp);
                    }
                }
            }
        }

    }
}
