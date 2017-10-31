using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenXP
{
    class EditorIni
    {
        public string LastProjectDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private string filepath;

        public EditorIni()
        {
            filepath = System.IO.Path.Combine(Program.GetEditorDirectory(), "OpenXP.ini");
            try
            {
                string ini = System.IO.File.ReadAllText(filepath);
                string[] lines = ini.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string s in lines)
                {
                    if (s.Contains("="))
                    {
                        string[] sy = s.Split(new[] { "=" }, StringSplitOptions.None);
                        if (sy.Length > 1)
                        {
                            if (sy[0].Trim().ToLower() == "lastproject") LastProjectDirectory = sy[1].Trim();
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        public void Save()
        {
            string file = "[Editor]\r\nLastProject=" + LastProjectDirectory;
            System.IO.File.WriteAllText(filepath, file);
        }
    }
}
