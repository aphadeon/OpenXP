using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class ScriptHive
    {
        public List<Script> Scripts;

        public ScriptHive()
        {
            Scripts = new List<Script>();
        }

        public void AddScript(Script s)
        {
            Scripts.Add(s);
        }

        //we need an actual value copy for temporary modification in the script editor
        public ScriptHive Clone()
        {
            ScriptHive clone = new ScriptHive();
            foreach(Script s in Scripts)
            {
                clone.AddScript(s.Clone());
            }
            return clone;
        }
    }

    public class Script
    {
        public string Name;
        public string Contents;
        public int MagicNumber;
        public int lastPosition = 0; //not saved to file, only used for script editor

        public Script(int magic, string name, string contents)
        {
            if (magic == 0)
            {
                //create a new magic number
                MagicNumber = new Random().Next(99999999);
            }
            else MagicNumber = magic;
            Name = name;
            Contents = contents;
        }

        //we need an actual value copy for temporary modification in the script editor
        public Script Clone()
        {
            return new Script(MagicNumber, new string(Name.ToCharArray()), new string(Contents.ToCharArray()));
        }
    }
}
