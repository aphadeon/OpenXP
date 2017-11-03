using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenXP
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] parameters)
        {
            if (parameters.Length > 0)
            {
                foreach(string s in parameters)
                {
                    if (s.EndsWith(".rxproj"))
                    {
                        Editor.PreloadProject = s;
                    }
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OpenXP());
        }

        public static string GetEditorDirectory()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"/";
        }
    }
}
