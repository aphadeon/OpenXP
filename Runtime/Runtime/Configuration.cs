using System.IO;

namespace OpenXP.Runtime
{
    class Configuration
    {
        public bool LogEnabled = true;
        public LogLevel LogLevel = LogLevel.Verbose;
        public int LogCount = 3;
        public string LogPath = Program.Directory;
        public string GameTitle = "OpenXP Runtime";

        public Configuration()
        {
            //locate our INI file, if we have one
            string file = Path.GetFileNameWithoutExtension(Program.Filename) + ".ini";
            if (!File.Exists(file) && File.Exists("Game.ini")) file = "Game.ini";

            //If the INI exists, read our values, otherwise leave them default
            if (File.Exists(file))
            {

            }
        }
    }
}
