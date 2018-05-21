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

namespace PSXPrev
{
    public class Program
    {
        public static Logger Logger;
        public static PreviewForm PreviewForm;
        public static long LargestFileLength = 0;
        public static long LargestCurrentFilePosition = 0;

        static void Main(string[] args)
        {
            //var x = new Thread(new ThreadStart(delegate
            //{
            //    Application.EnableVisualStyles();
            //    Application.Run(new StubForm());
            //}));
            //x.SetApartmentState(ApartmentState.STA);
            //x.Start();
            //return;

            //AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage PSXPrev folder filter(optional) -tmd(optional) -pmd(optional) -tim(optional) -retim(optional) -tod(optional) -hmdmodels(optional) -log(optional) -noverbose(optional)");
                return;
            }

            var checkTmd = false;
            var checkTim = false;
            var checkPmd = false;
            var checkTod = false;
            var checkHmdModels = false;
            var retim = false;
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
                    case "-pmd":
                        checkPmd = true;
                        break;
                    case "-tim":
                        checkTim = true;
                        break;
                    case "-retim":
                        retim = true;
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

            var checkAll = !(checkTmd || checkTim || checkPmd || retim || checkTod || checkHmdModels);

            var path = args[0];
            if (!Directory.Exists(path))
            {
                Logger.WriteLine("Directory not found");
                return;
            }

            var filter = args.Length > 1 ? args[1] : "*.*";

            var allEntities = new List<RootEntity>();
            var allTextures = new List<Texture>();
            var allAnimations = new List<Animation>();
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

                ScanFiles(allEntities, allTextures, allAnimations, path, filter, checkTmd, checkPmd, checkTim, checkAll, retim, checkTod, checkHmdModels, PreviewForm);

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
            if(filePos > LargestCurrentFilePosition)
            {
                LargestCurrentFilePosition = filePos;
            }
            var perc = (double)LargestCurrentFilePosition / LargestFileLength;
            PreviewForm.UpdateProgress((int)(perc * 100), 100, false, message);
        }

        private static void ScanFiles(List<RootEntity> allEntities, List<Texture> allTextures, List<Animation> allAnimations, string path, string filter, bool checkTmd, bool checkPmd, bool checkTim, bool checkAll, bool reTIM, bool checkTod, bool checkHmdModels, PreviewForm previewForm)
        {
            var parsers = new List<Action<BinaryReader, string>>();
            if (checkAll || checkTim)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var timParser = new TIMParser((todEntity, fp) =>
                    {
                        allTextures.Add(todEntity);
                        UpdateProgress(fp, $"Found Texture {todEntity.Width}x{todEntity.Height} {todEntity.Bpp}bpp");
                        PreviewForm.ReloadItems();
                    });
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TIM Images at file {0}", fileTitle);
                    timParser.LookForTim(binaryReader, fileTitle);
                });
            }

            if (checkAll || reTIM)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var reTimParser = new RETIMParser();
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for TIM Images at file {0}", fileTitle);
                    var timTextures = reTimParser.LookForTim(binaryReader, fileTitle);
                    allTextures.AddRange(timTextures);
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

            if (checkAll || checkPmd)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var pmdParser = new PMDParser();
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for PMD Models at file {0}", fileTitle);
                    var pmdEntities = pmdParser.LookForPMD(binaryReader, fileTitle);
                    allEntities.AddRange(pmdEntities);
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

            if (checkAll || checkHmdModels)
            {
                parsers.Add((binaryReader, fileTitle) =>
                {
                    var hmdParser = new HMDParser();
                    Logger.WriteLine("");
                    Logger.WriteLine("Scanning for HMD Models at file {0}", fileTitle);
                    var hmdEntities = hmdParser.LookForHMDEntities(binaryReader, fileTitle);
                    allEntities.AddRange(hmdEntities);
                });
            }

            var files = Directory.GetFiles(path, filter);
            foreach (var file in files)
            {
                Parallel.ForEach(parsers, (parser) =>
                {
                    using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BufferedStream bs = new BufferedStream(fs))
                        {
                            using (var binaryReader = new BinaryReader(bs, Encoding.BigEndianUnicode))
                            {
                                try
                                {
                                    if(fs.Length > LargestFileLength)
                                    {
                                        LargestFileLength = fs.Length;
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
                });
            }

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ScanFiles(allEntities, allTextures, allAnimations, directory, filter, checkTmd, checkPmd, checkTim, checkAll, reTIM, checkTod, checkHmdModels, previewForm);
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
