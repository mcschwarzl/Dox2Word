﻿using System;

namespace Dox2Word.Logging
{
    public class Logger
    {
        public static Logger Instance { get; } = new Logger();

        private Logger() { }

        public bool HasErrors { get; private set; }
        public bool HasWarnings { get; private set; }

        public bool Verbose { get; set; }

        public void Debug(string text)
        {
            if (!this.Verbose)
                return;

            WriteLevel(null, "DEBUG");
            Console.WriteLine(text);
        }

        public void Info(string text)
        {
            WriteLevel(ConsoleColor.Green, "INFO");
            Console.WriteLine(text);
        }

        public void Warning(string text)
        {
            this.HasWarnings = true;

            WriteLevel(ConsoleColor.Yellow, "WARNING");
            Console.WriteLine(text);
        }

        public void Unsupported(string text)
        {
            WriteLevel(ConsoleColor.Cyan, "UNSUPPORTED");
            Console.WriteLine(text);
        }

        public void Error(Exception e)
        {
            this.HasErrors = true;

            WriteLevel(ConsoleColor.Red, "ERROR");
            Console.WriteLine(e.Message);

            Console.WriteLine(e.ToString());
        }

        private static void WriteLevel(ConsoleColor? color, string text)
        {
            color ??= Console.ForegroundColor;
            Console.Write("[");
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color.Value;
            Console.Write(text);
            Console.ForegroundColor = oldColor;
            Console.Write("] ");
        }
    }
}
