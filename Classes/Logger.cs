using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace PSXPrev.Classes
{
    public class Logger : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly bool _writeToFile;
        private readonly bool _noVerbose;

        public ConsoleColor StandardColor { get; set; } = ConsoleColor.White;
        public ConsoleColor PositiveColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor ExceptionPrefixColor { get; set; } = ConsoleColor.DarkGray;

        public Logger(bool writeToFile, bool noVerbose)
        {
            if (writeToFile)
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fffffff", CultureInfo.InvariantCulture);
                _writer = new StreamWriter(Path.Combine(Application.StartupPath, $"{time}.log"));
            }
            _writeToFile = writeToFile;
            _noVerbose = noVerbose;
        }

        private void WriteInternal(ConsoleColor? color, bool newLine, string text)
        {
            if (text == null)
            {
                text = string.Empty;
            }
            if (!_noVerbose)
            {
                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }
                // Write whole message to WriteLine instead of appending WriteLine(), because it'll be faster.
                if (newLine)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    Console.Write(text);
                }
            }
            if (_writeToFile)
            {
                if (newLine)
                {
                    _writer.WriteLine(text);
                }
                else
                {
                    _writer.Write(text);
                }
            }
        }


        public void WriteColor(ConsoleColor color, string format, params object[] args)
        {
            WriteInternal(color, false, string.Format(format, args));
        }
        public void WriteColor(ConsoleColor color, object value)
        {
            WriteInternal(color, false, value?.ToString());
        }
        public void WriteColor(ConsoleColor color, string text)
        {
            WriteInternal(color, false, text);
        }

        public void WriteColorLine(ConsoleColor color, string format, params object[] args)
        {
            WriteInternal(color, true, string.Format(format, args));
        }
        public void WriteColorLine(ConsoleColor color, object value)
        {
            WriteInternal(color, true, value?.ToString());
        }
        public void WriteColorLine(ConsoleColor color, string text)
        {
            WriteInternal(color, true, text);
        }


        public void Write(string format, params object[] args) => WriteColor(StandardColor, format, args);
        public void Write(object value) => WriteColor(StandardColor, value);
        public void Write(string text)  => WriteColor(StandardColor, text);

        public void WriteLine(string format, params object[] args) => WriteColorLine(StandardColor, format, args);
        public void WriteLine(object value) => WriteColorLine(StandardColor, value);
        public void WriteLine(string text)  => WriteColorLine(StandardColor, text);

        public void WriteLine()
        {
            WriteInternal(null, true, string.Empty);
        }


        public void WritePositive(string format, params object[] args) => WriteColor(PositiveColor, format, args);
        public void WritePositive(object value) => WriteColor(PositiveColor, value);
        public void WritePositive(string text)  => WriteColor(PositiveColor, text);

        public void WritePositiveLine(string format, params object[] args) => WriteColorLine(PositiveColor, format, args);
        public void WritePositiveLine(object value) => WriteColorLine(PositiveColor, value);
        public void WritePositiveLine(string text)  => WriteColorLine(PositiveColor, text);


        public void WriteWarning(string format, params object[] args) => WriteColor(WarningColor, format, args);
        public void WriteWarning(object value) => WriteColor(WarningColor, value);
        public void WriteWarning(string text)  => WriteColor(WarningColor, text);

        public void WriteWarningLine(string format, params object[] args) => WriteColorLine(WarningColor, format, args);
        public void WriteWarningLine(object value) => WriteColorLine(WarningColor, value);
        public void WriteWarningLine(string text)  => WriteColorLine(WarningColor, text);


        public void WriteError(string format, params object[] args) => WriteColor(ErrorColor, format, args);
        public void WriteError(object value) => WriteColor(ErrorColor, value);
        public void WriteError(string text)  => WriteColor(ErrorColor, text);

        public void WriteErrorLine(string format, params object[] args) => WriteColorLine(ErrorColor, format, args);
        public void WriteErrorLine(object value) => WriteColorLine(ErrorColor, value);
        public void WriteErrorLine(string text)  => WriteColorLine(ErrorColor, text);


        public void WriteExceptionLine(Exception exp)
        {
            WriteColorLine(ErrorColor, exp);
        }
        public void WriteExceptionLine(Exception exp, string prefixFormat, params object[] args)
        {
            WriteColor(ExceptionPrefixColor, prefixFormat + ": ", args);
            WriteColorLine(ErrorColor, exp);
        }
        public void WriteExceptionLine(Exception exp, object prefixValue)
        {
            WriteExceptionLine(exp, "{0}", prefixValue);
        }
        public void WriteExceptionLine(Exception exp, string prefixText)
        {
            WriteExceptionLine(exp, "{0}", prefixText);
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
