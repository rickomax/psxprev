using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PSXPrev.Classes;
using PSXPrev.Forms;

namespace PSXPrev
{
    public class Program
    {
        public static Logger Logger;

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
                    case "-hmdnodels":
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

            try
            {
                Logger.WriteLine("");
                Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                ScanFiles(allEntities, allTextures, allAnimations, path, filter, checkTmd, checkPmd, checkTim, checkAll, retim, checkTod, checkHmdModels);

                Logger.WriteLine("");
                Logger.WriteLine("Scan End {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Logger.WriteLine("Found {0} Models", allEntities.Count);
                Logger.WriteLine("Found {0} Textures", allTextures.Count);
                Logger.WriteLine("Found {0} Animations", allAnimations.Count);
            }
            catch (Exception exp)
            {
                Logger.WriteLine(exp);
            }


            if (allEntities.Count > 0 || allTextures.Count > 0 || allAnimations.Count > 0)
            {
                var entities = allEntities.ToArray();
                var textures = allTextures.ToArray();
                var animations = allAnimations.ToArray();

                var t = new Thread(new ThreadStart(delegate
                {
                    Application.EnableVisualStyles();
                    Application.Run(new PreviewForm(entities, textures, animations, debug));
                }));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }

        private static void ScanFiles(List<RootEntity> allEntities, List<Texture> allTextures, List<Animation> allAnimations, string path, string filter, bool checkTmd, bool checkPmd, bool checkTim, bool checkAll, bool reTIM, bool checkTod, bool checkHmdModels)
        {
            var files = Directory.GetFiles(path, filter);
            foreach (var file in files)
            {
                using (var fileReader = new FileReader())
                {
                    try
                    {
                        fileReader.OpenFile(file);
                        var fileTitle = file.Substring(file.LastIndexOf('\\') + 1);
                        if (checkAll || checkTim)
                        {
                            var timParser = new TIMParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for TIM Images at file {0}", file);
                            var timTextures = timParser.LookForTim(fileReader.Reader, fileTitle);
                            allTextures.AddRange(timTextures);
                        }

                        if (checkAll || reTIM)
                        {
                            var reTimParser = new RETIMParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for TIM Images at file {0}", file);
                            var timTextures = reTimParser.LookForTim(fileReader.Reader, fileTitle);
                            allTextures.AddRange(timTextures);
                        }

                        if (checkAll || checkTmd)
                        {
                            var tmdParser = new TMDParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for TMD Models at file {0}", file);
                            var tmdEntities = tmdParser.LookForTmd(fileReader.Reader, fileTitle);
                            allEntities.AddRange(tmdEntities);
                        }

                        if (checkAll || checkPmd)
                        {
                            var pmdParser = new PMDParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for PMD Models at file {0}", file);
                            var pmdEntities = pmdParser.LookForPMD(fileReader.Reader, fileTitle);
                            allEntities.AddRange(pmdEntities);
                        }

                        if (checkAll || checkTod)
                        {
                            var todParser = new TODParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for TOD Animations at file {0}", file);
                            var todAnimations = todParser.LookForTOD(fileReader.Reader, fileTitle);
                            allAnimations.AddRange(todAnimations);
                        }

                        if (checkAll || checkHmdModels)
                        {
                            var hmdParser = new HMDParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for HMD Models at file {0}", file);
                            var hmdEntities = hmdParser.LookForHMDEntities(fileReader.Reader, fileTitle);
                            allEntities.AddRange(hmdEntities);
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.WriteLine(exp);
                    }
                }
            }

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ScanFiles(allEntities, allTextures, allAnimations, directory, filter, checkTmd, checkPmd, checkTim, checkAll, reTIM, checkTod, checkHmdModels);
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
