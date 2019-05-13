using System;
using System.IO;
using System.Text;

namespace PSXPrev
{
    public class FileReader : IDisposable
    {
        public BinaryReader Reader { get; private set; }

        public void Dispose()
        {
            Reader?.Close();
        }

        public void OpenFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            var file = File.Open(filename, FileMode.Open);
            Reader = new BinaryReader(file, Encoding.ASCII);
        }
    }
}