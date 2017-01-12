using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PSXPrev.Classes.Entities;
using PSXPrev.Classes.FileUtils;
using PSXPrev.Classes.Parsers;
using PSXPrev.Classes.Texture;
using PSXPrev.Classes.Utils;

namespace PSXPrev
{
    public class Program
    {
        public static Logger Logger;
        public static List<RootEntity> AllEntities = new List<RootEntity>();
        public static List<Texture> AllTextures = new List<Texture>();
        public static bool Debug;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage PSXPrev folder filter(optional) -tmd(optional) -pmd(optional) -tim(optional) -retim(optional) -log(optional) -noverbose(optional)");
                return;
            }

            var checkTmd = false;
            var checkTim = false;
            var checkPmd = false;
            var retim = false;
            var log = false;
            var noVerbose = false;
            Debug = false;

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
                    case "-log":
                        log = true;
                        break;
                    case "-noverbose":
                        noVerbose = true;
                        break;
                    case "-debug":
                        Debug = true;
                        break;
                }
            }

            Logger = new Logger(log, noVerbose);

            var checkAll = !(checkTmd || checkTim || checkPmd || retim);

            var path = args[0];
            if (!Directory.Exists(path))
            {
                Logger.WriteLine("Directory not found");
                return;
            }

            var filter = args.Length > 1 ? args[1] : "*.*";

            try
            {
                Logger.WriteLine("");
                Logger.WriteLine("Scan begin {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                ScanFiles(path, filter, checkTmd, checkPmd, checkTim, checkAll, retim);

                Logger.WriteLine("");
                Logger.WriteLine("Scan End {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Logger.WriteLine("Found {0} Models", AllEntities.Count);
                Logger.WriteLine("Found {0} Textures", AllTextures.Count);
            }
            catch (Exception exp)
            {
                Logger.WriteLine(exp);
            }

            if (AllEntities.Count > 0 || AllTextures.Count > 0)
            {
                var t = new Thread(new ThreadStart(delegate
                {
                    Application.EnableVisualStyles();
                    Application.Run(new PreviewForm());
                }));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }

        private static void ScanFiles(string path, string filter, bool checkTmd, bool checkPmd, bool checkTim, bool checkAll, bool reTIM)
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
                            AllTextures.AddRange(timTextures);
                        }

                        if (checkAll || reTIM)
                        {
                            var reTimParser = new RETIMParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for TIM Images at file {0}", file);
                            var timTextures = reTimParser.LookForTim(fileReader.Reader, fileTitle);
                           AllTextures.AddRange(timTextures);
                        }

                        if (checkAll || checkTmd)
                        {
                            var tmdParser = new TMDParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for TMD Models at file {0}", file);
                            var tmdEntities = tmdParser.LookForTmd(fileReader.Reader, fileTitle);
                            AllEntities.AddRange(tmdEntities);
                        }

                        if (checkAll || checkPmd)
                        {
                            var pmdParser = new PMDParser();
                            Logger.WriteLine("");
                            Logger.WriteLine("Scanning for PMD Models at file {0}", file);
                            var pmdEntities = pmdParser.LookForPMD(fileReader.Reader, fileTitle);
                            AllEntities.AddRange(pmdEntities);
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
                ScanFiles(directory, filter, checkTmd, checkPmd, checkTim, checkAll, reTIM);
            }
        }
    }
}
