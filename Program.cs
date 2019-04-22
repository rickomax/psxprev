using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using PSXPrev.Forms;
using System.Threading.Tasks;
using System.Text;
using DiscUtils;
using DiscUtils.Iso9660;

namespace PSXPrev
{
    public class Program
    {
        public static Logger Logger;
        public static PreviewForm PreviewForm;
        public static LauncherForm LauncherForm;

        public static bool Scanning { get; private set; }
        public static List<RootEntity> AllEntities { get; private set; }
        public static List<Texture> AllTextures { get; private set; }
        public static List<Animation> AllAnimations { get; private set; }

        public static long LargestFileLength = 0;
        public static long LargestCurrentFilePosition = 0;
        private static bool _checkAll;
        private static string _path;
        private static bool _checkTmd;
        private static bool _checkTmdAlt;
        private static bool _checkTim;
        private static bool _checkTimAlt;
        private static bool _checkPmd;
        private static bool _checkTod;
        private static bool _checkHmdModels;
        private static bool _log;
        private static bool _noVerbose;
        private static bool _debug;
        private static string _filter;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage PSXPrev folder filter(optional) -tmd(optional) -tmdAlt(optional) -pmd(optional) -tim(optional) -timAlt(optional) -tod(optional) -hmdmodels(optional) -log(optional) -noverbose(optional)");
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
            var checkTmdAlt = false;
            var checkTim = false;
            var checkTimAlt = false;
            var checkPmd = false;
            var checkTod = false;
            var checkHmdModels = false;
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
                    case "-tmdAlt":
                        checkTmdAlt = true;
                        break;
                    case "-pmd":
                        checkPmd = true;
                        break;
                    case "-tim":
                        checkTim = true;
                        break;
                    case "-timAlt":
                        checkTimAlt = true;
                        break;
                    case "-tod":
                        checkTod = true;
                        break;
                    case "-hmdmodels":
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
                }
            }

            DoScan(path, filter, checkTmd, checkTmdAlt, checkTim, checkTimAlt, checkPmd, checkTod, checkHmdModels, log, noVerbose, debug);
        }

        internal static void DoScan(string path, string filter, bool checkTmd, bool checkTmdAlt, bool checkTim, bool checkTimAlt, bool checkPmd, bool checkTod, bool checkHmdModels, bool log, bool noVerbose, bool debug)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Logger.WriteLine("Directory/File not found");
                return;
            }

            Scanning = true;

            Logger = new Logger(log, noVerbose);

            _checkAll = !(checkTmd || checkTmdAlt || checkTim || checkTimAlt || checkPmd || checkTod || checkHmdModels);
            _path = path;
            _filter = filter;
            _checkTmd = checkTmd;
            _checkTmdAlt = checkTmdAlt;
            _checkTim = checkTim;
            _checkTimAlt = checkTimAlt;
            _checkPmd = checkPmd;
            _checkTod = checkTod;
            _checkHmdModels = checkHmdModels;
            _log = log;
            _noVerbose = noVerbose;
            _debug = debug;

            AllEntities = new List<RootEntity>();
            AllTextures = new List<Texture>();
            AllAnimations = new List<Animation>();
            PreviewForm = new PreviewForm((form) =>
            {
                form.UpdateAnimations(AllAnimations);
                form.UpdateRootEntities(AllEntities);
                form.UpdateTextures(AllTextures);
            }, debug);

            var t = new Thread(new ThreadStart(delegate
            {
                Application.EnableVisualStyles();
                Application.Run(PreviewForm);
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();

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
            if (filePos > LargestCurrentFilePosition)
            {
                LargestCurrentFilePosition = filePos;
            }
            var perc = (double)LargestCurrentFilePosition / LargestFileLength;
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
                    Logger.WriteLine("Scanning for TIM Images at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
                });
            }

            if (_checkTimAlt)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParserOld((tmdEntity, fp) =>
                    {
                        AllTextures.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Texture {tmdEntity.Width}x{tmdEntity.Height} {tmdEntity.Bpp}bpp");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TIM Images (alt) at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
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
                    Logger.WriteLine("Scanning for TMD Models at file {0}", fileTitle);
                    tmdParser.LookForTmd(binaryReader, fileTitle);
                });
            }

            if (_checkTmdAlt)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var tmdParser = new TMDParserAlternative((tmdEntity, fp) =>
                    {
                        AllEntities.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Model with {tmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TMD Models (alt) at file {0}", fileTitle);
                    tmdParser.LookForTmd(binaryReader, fileTitle);
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
                    Logger.WriteLine("Scanning for PMD Models at file {0}", fileTitle);
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
                    Logger.WriteLine("Scanning for TOD Animations at file {0}", fileTitle);
                    todParser.LookForTOD(binaryReader, fileTitle);
                });
            }

            if (_checkHmdModels)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var hmdParser = new HMDParser((hmdEntity, fp) =>
                    {
                        AllEntities.Add(hmdEntity);
                        UpdateProgress(fp, $"Found Model with {hmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for HMD Models at file {0}", fileTitle);
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
                        if (file.ToLowerInvariant().Contains(".str;"))
                        {
                            continue;
                        }
                        var fileInfo = cdReader.GetFileInfo(file);
                        if (fileInfo.Exists)
                        {
                            foreach (var parser in parsers)
                            {
                                var stream = fileInfo.OpenRead();
                                ProcessFile(stream, file, parser);
                            }
                        }
                    }
                }
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
                if (file.ToLowerInvariant().EndsWith(".str"))
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
                        if (stream.Length > LargestFileLength)
                        {
                            LargestFileLength = stream.Length;
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
