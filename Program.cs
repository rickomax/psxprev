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
        private static PreviewForm PreviewForm;
        private static LauncherForm LauncherForm;

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
        private static bool _log;
        private static bool _noVerbose;
        private static string _filter;

        public static bool Debug;

        public static ulong MaxTODPackets = 100000;
        public static ulong MaxTODFrames = 100000;
        public static ulong MaxTMDPrimitives = 100000;
        public static ulong MaxTMDObjects = 100000;
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
        public static ulong MaxHMDVertCount = 10000;
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
                Console.WriteLine("Usage PSXPrev folder filter(optional) -tmd(optional) -vdf(optional) -pmd(optional) -tim(optional) -tod(optional) -an(optional) -hmd(optional) -croc(optional) -psx(optional) -log(optional) -noverbose(optional) -debug(optional)");
                LauncherForm = new LauncherForm();
                var thread = new Thread(new ThreadStart(delegate
                {
                    Application.EnableVisualStyles();
                    Application.Run(LauncherForm);
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                return;
            }

            var path = args[0];
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Logger.WriteLine("Directory/File not found");
                return;
            }

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
            var log = false;
            var noVerbose = false;
            var debug = false;

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
                }
            }
            DoScan(path, filter, checkTmd, checkVdf, checkTim, checkPmd, checkTod, checkHmdModels, log, noVerbose, debug, checkCrocModels, checkPsx, checkAn);
        }

        internal static void DoScan(string path, string filter, bool checkTmd, bool checkVdf, bool checkTim, bool checkPmd, bool checkTod, bool checkHmd, bool log, bool noVerbose, bool debug, bool checkCroc, bool checkPsx, bool checkAn)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Logger.WriteLine("Directory/File not found");
                return;
            }

            Scanning = true;

            Logger = new Logger(log, noVerbose);

            _checkAll = !(checkTmd || checkVdf || checkTim || checkPmd || checkTod || checkHmd || checkCroc || checkPsx || checkAn);
            _path = path;
            _filter = filter;
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
            _log = log;
            _noVerbose = noVerbose;
            Debug = debug;

            AllEntities = new List<RootEntity>();
            AllTextures = new List<Texture>();
            AllAnimations = new List<Animation>();

            PreviewForm = new PreviewForm((form) =>
            {
                form.UpdateAnimations(AllAnimations);
                form.UpdateRootEntities(AllEntities);
                form.UpdateTextures(AllTextures);
            });

            var thread = new Thread(new ThreadStart(delegate
            {
                Application.EnableVisualStyles();
                Application.Run(PreviewForm);
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            try
            {
                Logger.WriteLine("");
                Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                ScanFiles();

                Logger.WriteLine("");
                Logger.WriteLine("Scan End {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Logger.WriteLine("Found {0} Models", AllEntities.Count);
                Logger.WriteLine("Found {0} Textures", AllTextures.Count);
                Logger.WriteLine("Found {0} Animations", AllAnimations.Count);

                PreviewForm.UpdateProgress(0, 0, true, $"{AllEntities.Count} Models, {AllTextures.Count} Textures, {AllAnimations.Count} Animations Found");
            }
            catch (Exception exp)
            {
                Logger.WriteLine(exp);
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

        private static void ScanFiles()
        {
            var parsers = new List<Action<BinaryReader, string>>();

            if (_checkAll || _checkTim)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParser((tmdEntity, fp) =>
                    {
                        AllTextures.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Texture {tmdEntity.Width}x{tmdEntity.Height} {tmdEntity.Bpp}bpp");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TIM at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkCroc)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var crocModelReader = new CrocModelReader((tmdEntity, fp) =>
                    {
                        AllEntities.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Model with {tmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for Croc at file {0}", fileTitle);
                    crocModelReader.LookForCrocModel(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkPsx)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var psxParser = new PSXParser((tmdEntity, fp) =>
                    {
                        AllEntities.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Model with {tmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for PSX at file {0}", fileTitle);
                    psxParser.LookForPSX(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkTmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var tmdParser = new TMDParser((tmdEntity, fp) =>
                    {
                        AllEntities.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Model with {tmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TMD at file {0}", fileTitle);
                    tmdParser.LookForTmd(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkVdf)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var vdfParser = new VDFParser((vdfEntity, fp) =>
                    {
                        AllAnimations.Add(vdfEntity);
                        UpdateProgress(fp, $"Found Animation with {vdfEntity.ObjectCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for VDF at file {0}", fileTitle);
                    vdfParser.LookForVDF(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkAn)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var anParser = new ANParser((vdfEntity, fp) =>
                    {
                        AllAnimations.Add(vdfEntity);
                        UpdateProgress(fp, $"Found Animation with {vdfEntity.ObjectCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for AN at file {0}", fileTitle);
                    anParser.LookForAN(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkPmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var pmdParser = new PMDParser((pmdEntity, fp) =>
                    {
                        AllEntities.Add(pmdEntity);
                        UpdateProgress(fp, $"Found Model with {pmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for PMD at file {0}", fileTitle);
                    pmdParser.LookForPMD(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkTod)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var todParser = new TODParser((todEntity, fp) =>
                    {
                        AllAnimations.Add(todEntity);
                        UpdateProgress(fp, $"Found Animation with {todEntity.ObjectCount} objects and {todEntity.FrameCount} frames");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TOD at file {0}", fileTitle);
                    todParser.LookForTOD(binaryReader, fileTitle);
                });
            }

            if (_checkAll || _checkHmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var hmdParser = new HMDParser((hmdEntity, fp) =>
                    {
                        AllEntities.Add(hmdEntity);
                        UpdateProgress(fp, $"Found Model with {hmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    }, (hmdAnimation, fp) =>
                    {
                        AllAnimations.Add(hmdAnimation);
                        UpdateProgress(fp, $"Found Animation with {hmdAnimation.ObjectCount} objects and {hmdAnimation.FrameCount} frames");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for HMD at file {0}", fileTitle);
                    hmdParser.LookForHMDEntities(binaryReader, fileTitle);
                });
            }

            if (_path.ToLowerInvariant().EndsWith(".iso"))
            {
                using (var isoStream = File.Open(_path, FileMode.Open))
                {
                    var cdReader = new CDReader(isoStream, true);
                    var files = cdReader.GetFiles("", _filter ?? "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (file.ToLowerInvariant().Contains(".str;") || file.ToLowerInvariant().Contains(".xa"))
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
                ProcessFiles(_path, _filter, parsers);
            }
        }

        private static void ProcessFiles(string path, string filter, List<Action<BinaryReader, string>> parsers)
        {
            var files = Directory.GetFiles(path, filter);
            foreach (var file in files)
            {
                if (file.ToLowerInvariant().EndsWith(".str") || file.ToLowerInvariant().EndsWith(".xa"))
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
                        Logger.WriteLine(exp);
                    }
                }
            }
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("---------------------");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Environment.Exit(1);
        }

    }
}
