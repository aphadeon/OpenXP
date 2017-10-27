using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class Project
    {
        public string Path;
        public string Directory;
        public bool Dirty = false;
        public bool Extended = false; //does it make use of OpenXP extensions?
        public GameIni Ini;
        public ScriptHive Scripts;
        public MapInfos Maps;
        public Ruby Ruby;

        //call on loading to set up the project
        //return a string message if there is a problem or null if everything is okay
        public string Setup()
        {
            //Game.ini
            Ini = new GameIni();
            if(File.Exists(System.IO.Path.Combine(Directory, "Game.ini")))
            {
                Ini.Deserialize(File.ReadAllText(System.IO.Path.Combine(Directory, "Game.ini")));
            } else
            {
                return "There was an error reading Game.ini";
            }

            //Ruby
            Ruby = new Ruby();

            //Scripts.rxdata
            Scripts = new ScriptHive();
            Ruby.PopulateScriptHive(Scripts);

            //MapInfos.rxdata
            Maps = new MapInfos();
            Ruby.PopulateMapInfos(Maps);

            return null;
        }

        //call to save the project and clear the dirty flag
        public void Save()
        {
            if (Dirty)
            {
                //todo: save stuff here - implement as each file is supported
                //save the .rxproj file
                File.WriteAllText(Path, "RPGXP 1.05");

                //save the Game.ini
                File.WriteAllText(System.IO.Path.Combine(Directory, "Game.ini"), Ini.Serialize());

                //save Scripts.rxdata
                Ruby.WriteScriptHive(Scripts);

                //save MapInfos.rxdata
                Ruby.WriteMapInfos(Maps);

                Dirty = false; //set this last if everything saved okay
            }
        }

        //call this when you change something that needs to be saved
        public void Touch()
        {
            Dirty = true;
        }

        //called by DialogScriptEditor at initialization to get a cancelable script hive
        public ScriptHive CreateTemporaryScriptHive()
        {
            return Scripts.Clone();
        }

        public void Dispose()
        {
            if (Ruby != null) Ruby.Dispose();
        }
    }
}
