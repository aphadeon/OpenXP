using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP.Runtime
{
    class Parameters
    {
        public bool Debug = false;

        public Parameters(List<string> arguments)
        {
            while(arguments.Count > 0)
            {
                //load and remove the first element of the array
                string arg = arguments[0];
                arguments.RemoveAt(0);

                //switch on argument
                switch (arg)
                {
                    //-game : sets the game directory for the game with the following parameter
                    //engine will use this directory for everything but the .exe (including .ini)
                    case "-game":
                        if (arguments.Count > 0)
                        {
                            string dir = arguments[0];
                            if (Directory.Exists(dir))
                            {
                                //set the program's directory
                                Program.Directory = arguments[0];
                                //remove the directory argument from the list
                                arguments.RemoveAt(0);
                            }
                        }
                        break;
                    case "debug":
                    case "test":
                        EnableDebug();
                        break;
                }
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void EnableDebug()
        {
            Debug = true;
        }
    }
}
