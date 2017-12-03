using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP.Runtime
{
    class Log
    {
        public static void FatalE(object message, Exception e)
        {
            Fatal(message.ToString() + "\n" + e.Message + "\n" + e.StackTrace);
        }

        public static void Fatal(object message)
        {
            Enter(LogLevel.Fatal, message.ToString());
        }

        public static void Error(object message)
        {
            Enter(LogLevel.Error, message.ToString());
        }

        public static void Warn(object message)
        {
            Enter(LogLevel.Warn, message.ToString());
        }

        public static void Info(object message)
        {
            Enter(LogLevel.Info, message.ToString());
        }

        public static void Verbose(object message)
        {
            Enter(LogLevel.Verbose, message.ToString());
        }

        private static void Enter(LogLevel level, string message)
        {
            if ((int)level < (int) Program.Configuration.LogLevel) return;
            LogEntry entry = new LogEntry(level, message);
            if (!Program.Configuration.LogEnabled) return;
            entries.Add(entry);
        }

        public static void SaveLog()
        {
            if (!Program.Configuration.LogEnabled || Program.Configuration.LogCount <= 0) return;
            string logFileContent = "";
            string debug = "";
            #if DEBUG
                        debug = " (Debug)";
            #endif
            string header = Program.Configuration.GameTitle + debug + " Logfile :: ";
            header += String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.UtcNow) + "\r\n\r\n";

            int itemCount = 0;
            foreach (LogEntry entry in entries)
            {
                string loglevelname = Enum.GetName(typeof(LogLevel), entry.level);
                string message = entry.message;
                string timestamp = entry.datetime;
                logFileContent += timestamp + " [" + loglevelname + "]  " + message + "\r\n";
                itemCount++;
            }
            //add quote to header
            string[] quotes = {
                "\"Anyone who has never made a mistake has never tried anything new.\" - Albert Einstein",
                "\"Freedom is not worth having if it does not include the freedom to make mistakes.\" - Mahatma Gandhi",
                "\"Never interrupt your enemy when he is making a mistake.\" - Napoléon Bonaparte",
                "\"Have no fear of perfection - you'll never reach it.\" - Salvador Dalí",
                "\"To err is human, to forgive, divine.\" - Alexander Pope",
                "\"We learn from failure, not from success!\" - Bram Stoker",
                "\"Mistakes are the growing pains of wisdom.\" - William Jordan",
                "\"In life, there are no mistakes, only lessons.\" - Vic Johnson",
                "\"I think I could be a really great potato.\" - aphadeon"
            };
            string quote = Util.Choose(quotes);
            quote = "\r\n\r\n" + quote + "\r\n\r\n";

            //todo: add runtime information to the header

            //assemble final file contents
            logFileContent = header + quote + logFileContent;

            Directory.CreateDirectory(Program.Configuration.LogPath);
            string fileExt = ".txt";

            //rename old logs
            for (int fileNumber = Program.Configuration.LogCount - 1; fileNumber >= 0; fileNumber--)
            {
                string tempFilename = "log-" + fileNumber.ToString() + fileExt;
                if (File.Exists(tempFilename))
                {
                    if (fileNumber == Program.Configuration.LogCount - 1)
                    {
                        File.Delete(tempFilename);
                    }
                    else
                    {
                        string tempFilename2 = "log-" + (fileNumber + 1).ToString() + fileExt;
                        File.Move(tempFilename, tempFilename2);
                    }
                }
            }

            string filename = "log-0" + fileExt;
            File.WriteAllText(Program.Configuration.LogPath + filename, logFileContent, Encoding.UTF8);
        }


        private static List<LogEntry> entries = new List<LogEntry>();
        private class LogEntry
        {
            public string datetime = "";
            public string message = "";
            public LogLevel level = LogLevel.Info;
            public LogEntry(LogLevel level, string message)
            {
                this.level = level;
                this.message = message;
                datetime = String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.UtcNow) + " ";
                ConsoleColor backupColor = Console.ForegroundColor;
                ConsoleColor backupColor2 = Console.BackgroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(datetime);
                //set console colors
                switch (level)
                {
                    case LogLevel.Verbose:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Red;
                        break;
                }
                Console.WriteLine("[" + Enum.GetName(typeof(LogLevel), level) + "] " + message);
                Console.ForegroundColor = backupColor;
                Console.BackgroundColor = backupColor2;
            }
        }
    }
}

