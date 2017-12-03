using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenXP.Runtime
{
    class Program
    {

        public static string Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + System.IO.Path.DirectorySeparatorChar;
        public static string Filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
        public static Configuration Configuration;
        public static Parameters Parameters;

        static void Main(string[] args)
        {
            //hook exit handler
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            //handle commandline parameters
            Parameters = new Parameters(new List<string>(args));

            //set the current working directory to the assembly directory
            System.IO.Directory.SetCurrentDirectory(Directory);

            //create the configuration instance
            Configuration = new Configuration();

            Log.Verbose("Starting up...");

            //todo: remove pause when no longer needed
            Console.WriteLine("\nExecution finished. Press any key to exit.");
            Console.ReadKey();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Log.SaveLog();
        }
    }
}
