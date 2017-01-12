using System;
using System.IO;
using System.Windows.Forms;

namespace PSXPrev.Classes.Utils
{
    public class Logger : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly bool _writeToFile;
        private readonly bool _noVerbose;

        public Logger(bool writeToFile, bool noVerbose)
        {
            if (writeToFile)
            {
                _writer = new StreamWriter(Path.Combine(Application.StartupPath, DateTime.Now.ToFileTime() + ".log"));
            }
            _writeToFile = writeToFile;
            _noVerbose = noVerbose;
        }

        public void WriteLine(string format, params object[] args)
        {
            if (!_noVerbose)
            {
                Console.WriteLine(format, args);
            }
            if (_writeToFile)
            {
                _writer.WriteLine(format, args);
            }
        }

        public void WriteLine(object text)
        {
            if (!_noVerbose)
            {
                Console.WriteLine(text);
            }
            if (_writeToFile)
            {
                _writer.WriteLine(text);
            }
        }

        public void Dispose()
        {
            if (_writeToFile)
            {
                _writer.Close();
            }
        }
    }
}
