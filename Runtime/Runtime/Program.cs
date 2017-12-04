using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenXP.Runtime
{
    //handles interfacing with the operating system and lowest-level workflow
    class Program
    {
        public static string Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
        public static string Filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
        public static string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar;
        public static Configuration Configuration;
        public static Parameters Parameters;

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e)
                => ForwardFatal(e.ExceptionObject);

                //make sure to check OpenTK for an unhandledexception or threadexception hook

                //hook exit handler
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

                //handle commandline parameters
                Parameters = new Parameters(new List<string>(args));

                //set the current working directory to the assembly directory
                System.IO.Directory.SetCurrentDirectory(Directory);

                //create the configuration instance and load the config file
                Configuration = new Configuration();
                Configuration.Load();

                Log.Verbose("Starting up...");
            }
            catch (Exception huh)
            {
                return HandleFatal(huh);
            }

            //todo: remove pause when no longer needed
            Console.WriteLine("\nExecution finished. Press any key to exit.");
            Console.ReadKey();

            //Exit without error
            return 0;
        }

        //on any normal exit
        static void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                Configuration.Save();
                Log.Verbose("Shutting down...");
                Log.SaveLog();
            } catch {}
        }

        //hooks odd exception objects up to a safe HandleFatal call
        static int ForwardFatal(object exceptionObject)
        {
            var huh = exceptionObject as Exception;
            if (huh == null) //wtf did we throw
            {
                huh = new NotSupportedException(
                  "Unhandled exception doesn't derive from System.Exception: "
                   + exceptionObject.ToString()
                );
            }
            return HandleFatal(huh);
        }

        //Handles any unexpected fatal errors
        static int HandleFatal(Exception e)
        {
            //return the desired error code
            return -1;
        }
    }
}
