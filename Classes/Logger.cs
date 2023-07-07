using System;
using System.IO;
using System.Windows.Forms;

namespace PSXPrev.Classes
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

        private void Write(string format, object[] args)
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

        public void WriteLine(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Write(format, args);
        }

        public void WriteLine(object text)
        {
            WriteLine("{0}", new[] { text });
        }

        public void WriteErrorLine(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(format, args);
        }
        public void WriteErrorLine(object text)
        {
            WriteErrorLine("{0}", new[] { text });
        }


        public void WritePositiveLine(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Write(format, args);
        }

        public void WritePositiveLine(object text)
        {
            WritePositiveLine("{0}", new[] { text });
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
