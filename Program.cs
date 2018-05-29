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
        public static long LargestFileLength = 0;
        public static long LargestCurrentFilePosition = 0;
        private static bool checkAll;
        private static string path;
        private static bool checkTmd;
        private static bool checkTmdAlt;
        private static bool checkTim;
        private static bool checkTimAlt;
        private static bool checkPmd;
        private static bool checkTod;
        private static bool checkHmdModels;
        private static bool log;
        private static bool noVerbose;
        private static bool debug;
        private static string filter;
        private static List<RootEntity> allEntities;
        private static List<Texture> allTextures;
        private static List<Animation> allAnimations;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage PSXPrev folder filter(optional) -tmd(optional) -tmdAlt(optional) -pmd(optional) -tim(optional) -timAlt(optional) -tod(optional) -hmdmodels(optional) -log(optional) -noverbose(optional)");
                return;
            }

            path = args[0];
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Logger.WriteLine("Directory/File not found");
                return;
            }

           checkTmd = false;
           checkTmdAlt = false;
           checkTim = false;
           checkTimAlt = false;
           checkPmd = false;
           checkTod = false;
           checkHmdModels = false;
           log = false;
           noVerbose = false;
           debug = false;

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

            Logger = new Logger(log, noVerbose);

            checkAll = !(checkTmd || checkTmdAlt || checkTim || checkTimAlt || checkPmd || checkTod || checkHmdModels);

            filter = args.Length > 1 ? args[1] : "*.*";

            allEntities = new List<RootEntity>();
            allTextures = new List<Texture>();
            allAnimations = new List<Animation>();
            PreviewForm = new PreviewForm((form) =>
            {
                form.UpdateAnimations(allAnimations);
                form.UpdateRootEntities(allEntities);
                form.UpdateTextures(allTextures);
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
                Logger.WriteLine("Found {0} Models", allEntities.Count);
                Logger.WriteLine("Found {0} Textures", allTextures.Count);
                Logger.WriteLine("Found {0} Animations", allAnimations.Count);

                PreviewForm.UpdateProgress(0, 0, true, $"{allEntities.Count} Models, {allTextures.Count} Textures, {allAnimations.Count} Animations Found");
            }
            catch (Exception exp)
            {
                Logger.WriteLine(exp);
            }
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

            if (checkAll || checkTim)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParser((tmdEntity, fp) =>
                    {
                        allTextures.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Texture {tmdEntity.Width}x{tmdEntity.Height} {tmdEntity.Bpp}bpp");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TIM Images at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
                });
            }

            if (checkTimAlt)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParserAlternative((tmdEntity, fp) =>
                    {
                        allTextures.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Texture {tmdEntity.Width}x{tmdEntity.Height} {tmdEntity.Bpp}bpp");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TIM Images (alt) at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
                });
            }

            if (checkAll || checkTmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var tmdParser = new TMDParser((tmdEntity, fp) =>
                    {
                        allEntities.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Model with {tmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TMD Models at file {0}", fileTitle);
                    tmdParser.LookForTmd(binaryReader, fileTitle);
                });
            }

            if (checkTmdAlt)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var tmdParser = new TMDParserAlternative((tmdEntity, fp) =>
                    {
                        allEntities.Add(tmdEntity);
                        UpdateProgress(fp, $"Found Model with {tmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TMD Models (alt) at file {0}", fileTitle);
                    tmdParser.LookForTmd(binaryReader, fileTitle);
                });
            }

            if (checkAll || checkPmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var pmdParser = new PMDParser((pmdEntity, fp) =>
                    {
                        allEntities.Add(pmdEntity);
                        UpdateProgress(fp, $"Found Model with {pmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for PMD Models at file {0}", fileTitle);
                    pmdParser.LookForPMD(binaryReader, fileTitle);
                });
            }

            if (checkAll || checkTod)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var todParser = new TODParser((todEntity, fp) =>
                    {
                        allAnimations.Add(todEntity);
                        UpdateProgress(fp, $"Found Animation with {todEntity.ObjectCount} objects and {todEntity.FrameCount} frames");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TOD Animations at file {0}", fileTitle);
                    todParser.LookForTOD(binaryReader, fileTitle);
                });
            }

            if (checkHmdModels)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var hmdParser = new HMDParser((hmdEntity, fp) =>
                    {
                        allEntities.Add(hmdEntity);
                        UpdateProgress(fp, $"Found Model with {hmdEntity.ChildCount} objects");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for HMD Models at file {0}", fileTitle);
                    hmdParser.LookForHMDEntities(binaryReader, fileTitle);
                });
            }

            if (path.ToLowerInvariant().EndsWith(".iso"))
            {
                using (var isoStream = File.Open(path, FileMode.Open))
                {
                    var cdReader = new CDReader(isoStream, true);
                    var files = cdReader.GetFiles("", filter ?? "*.*", SearchOption.AllDirectories);
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
                ProcessFiles(path, filter, parsers);
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
