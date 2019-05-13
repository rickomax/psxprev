using System.Diagnostics;
using System.IO;

namespace PSXPrev
{
    public static class ManifestResourceLoader
    {
        public static string LoadTextFile(string textFileName)
        {
            using (var stream = File.OpenRead(textFileName))
            {
                Debug.Assert(stream != null, "stream != null");
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
