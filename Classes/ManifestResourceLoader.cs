using System.IO;
using System.Reflection;

namespace PSXPrev.Classes
{
    public static class ManifestResourceLoader
    {
        public static string BaseNamespace = "PSXPrev";

        private static string ConvertPath(string fileName)
        {
            // Replace path slashes with dots.
            var resourceName = fileName.Replace('\\', '.').Replace('/', '.');
            // Prefix path with base namespace.
            return $"{BaseNamespace}.{resourceName}";
        }

        public static Stream Open(string fileName, bool checkFileSystem = true)
        {
            if (checkFileSystem && File.Exists(fileName))
            {
                return File.OpenRead(fileName);
            }
            else
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ConvertPath(fileName));
                if (stream == null)
                {
                    throw new FileNotFoundException("Could not open resource file", fileName);
                }
                return stream;
            }
        }

        public static string LoadTextFile(string fileName, bool checkFileSystem = true)
        {
            using (var stream = Open(fileName, checkFileSystem))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
