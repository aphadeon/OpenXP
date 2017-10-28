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
        public int Validated = 0;
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
                            if (sy[0].Trim().ToLower() == "validated") Validated = int.Parse(sy[1].Trim());
                            if (sy[0].Trim().ToLower() == "lastproject") LastProjectDirectory = sy[1].Trim();
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            Console.WriteLine("Validation token: " + Validated);
            if(Validated == 0)
            {
                Console.WriteLine("Attempting validation...");
                DialogResult result1 = MessageBox.Show("Thank you for your interest in OpenXP!\n\nIn order to use this utility, you need to own RPG Maker XP.\nMay we check with Steam to see if you own the game?\n\nYou would require an internet connection, Steam to be installed\non this machine, and you would need to own the Steam version\nof RPG Maker XP.", "RPG Maker XP", MessageBoxButtons.OKCancel);
                if (result1 == DialogResult.OK)
                {
                    Validate();
                }
            } else
            {
                Console.WriteLine("Attempting revalidation...");
                Revalidate();
            }
        }

        public void Revalidate()
        {
            bool success = XPV.ValidationServices.Revalidate(Validated);
            if (success)
            {
                Editor.Form.MarkedForExit = false;
            }
            else
            {
                Validate();
            }
        }

        public void Validate()
        {
            //We need to validate presence of original editor.
            int valid = XPV.ValidationServices.Validate();
            if (valid != 0)
            {
                Validated = valid;
                //force an initial save
                Save();
                Editor.Form.MarkedForExit = false;
                System.Windows.Forms.MessageBox.Show("Your ownership of RPG Maker XP was verified via Steam.\n\nWelcome to OpenXP!");
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("We could not verify ownership of RPG Maker XP.\n\nThis works via Steam, and unfortunately we cannot check private profiles.\nPlease ensure that you have the Steam client installed, that you do own\nRPG Maker XP, and that your profile is not set to private (just momentarily \nwhile we verify things).  You do not need to have the program installed.\n\nThis program will now exit.");
                Editor.Form.MarkedForExit = true;
            }
        }

        public void Save()
        {
            string file = "[Editor]\r\nLastProject=" + LastProjectDirectory + "\r\nValidated=" + Validated.ToString();
            System.IO.File.WriteAllText(filepath, file);
        }

        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }
}
