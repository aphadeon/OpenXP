using System;
using System.Collections.Generic;
using System.IO;

namespace OpenXP.Runtime
{
    //handles the Game.ini
    class Configuration
    {
        //public configuration variables
        //

        // - Logging
        public bool LogEnabled = true;
        public LogLevel LogLevel = LogLevel.Verbose;
        public int LogCount = 3;
        public string LogPath = Program.Directory;
        
        // - Game
        public string GameTitle = "OpenXP Runtime";
        public string ScriptsPath = "Data" + Path.DirectorySeparatorChar + "Scripts.rxdata";
        public string RTP1 = "Standard";
        public string RTP2 = "";
        public string RTP3 = "";

        // - OXP
        public bool UseAppData = false;
        public string AppDataFolder = "OpenXP";

        // - Graphics
        public int GraphicsWidth = 640;
        public int GraphicsHeight = 480;

        //private management stuff
        private bool Dirty = false;
        private List<IniLine> Contents = new List<IniLine>();
        private string filepath = Path.GetFileNameWithoutExtension(Program.Filename) + ".ini";

        public void Load()
        {
            ReadFile(FindInputIniFile());
            //check if we need to reload from AppData
            //UseAppData and AppDataFolder only read from the local program directory and are ignored in appdata
            UseAppData = Get<bool>("OXP", "UseAppData", false, true);
            GameTitle = Get<string>("Game", "Title", "OpenXP Runtime", false);
            AppDataFolder = Get<string>("OXP", "AppDataFolder", GameTitle, false);
            if (UseAppData) //check if there is an INI in the AppData folder
            {
                string appdatapath = Program.AppData + AppDataFolder + Path.DirectorySeparatorChar;
                //update the write location
                filepath = appdatapath + Path.GetFileNameWithoutExtension(Program.Filename) + ".ini";
                Directory.CreateDirectory(appdatapath);
                //check for existing files
                if (File.Exists(appdatapath + Path.GetFileNameWithoutExtension(Program.Filename) + ".ini"))
                {
                    Contents.Clear();
                    ReadFile(appdatapath + Path.GetFileNameWithoutExtension(Program.Filename) + ".ini");
                    //re-read game title
                    GameTitle = Get<string>("Game", "Title", "OpenXP Runtime", false);
                } else if(File.Exists(appdatapath + "Game.ini"))
                {
                    Contents.Clear();
                    ReadFile(appdatapath + "Game.ini");
                    //re-read game title
                    GameTitle = Get<string>("Game", "Title", "OpenXP Runtime", false);
                }
            }

            //load remaining variables

            //Logging
            LogEnabled = Get<bool>("Logging", "LoggingEnabled", true, true);
            LogLevel = (LogLevel) Get<int>("Logging", "LogLevel", 0, true);
            LogCount = Get<int>("Logging", "LogCount", 3, true);
            LogPath = Get<string>("Logging", "LogPath", Program.Directory, false);

            //Game
            ScriptsPath = Get<string>("Game", "Scripts", "Data" + Path.DirectorySeparatorChar + "Scripts.rxdata", false);
            RTP1 = Get<string>("Game", "RTP1", "Standard", false);
            RTP2 = Get<string>("Game", "RTP2", "", false);
            RTP3 = Get<string>("Game", "RTP3", "", false);

            //Graphics
            GraphicsWidth = Get<int>("Graphics", "Width", 640, true);
            GraphicsHeight = Get<int>("Graphics", "Height", 480, true);
        }

        //supports bool, int, and string datatypes
        public T Get<T>(string group, string item, T defaultValue, bool addIfMissing)
        {
            string v = GetValue(group, item);
            if (v == null)
            {
                if (addIfMissing)
                {
                    SetValue(group, item, defaultValue.ToString());
                }
                return defaultValue;
            }
            if (defaultValue is bool) return (T)(object)TryParse(v, (bool)(object)defaultValue);
            else if (defaultValue is string) return (T)(object)TryParse(v, (string)(object)defaultValue);
            else if (defaultValue is int) return (T)(object)TryParse(v, (int)(object)defaultValue);
            else return defaultValue;
        }

        public T TryParse<T>(string input, T defaultValue)
        {
            dynamic r;
            if (defaultValue is bool) if (bool.TryParse(input, out bool result)) return r = result;
            if (defaultValue is string) return r = input;
            if (defaultValue is int) if (int.TryParse(input, out int result)) return r = result;
            return defaultValue;
        }

        public string FindInputIniFile()
        {
            //locate our INI file, if we have one
            string file = Path.GetFileNameWithoutExtension(Program.Filename) + ".ini";
            if (!File.Exists(file) && File.Exists("Game.ini")) file = "Game.ini";
            return file;
        }

        public string GetValue(string group, string item)
        {
            foreach(IniLine line in Contents)
            {
                if(line is IniLineKV){
                    IniLineKV ilkv = line as IniLineKV;
                    if(ilkv.GroupName.ToLower() == group.ToLower())
                    {
                        if(ilkv.ItemName.ToLower() == item.ToLower())
                        {
                            return ilkv.ItemValue;
                        }
                    }
                }
            }
            return null;
        }

        public void SetValue(string group, string item, string value)
        {
            bool groupExists = false;
            foreach (IniLine line in Contents)
            {
                if (line is IniLineKV)
                {
                    IniLineKV ilkv = line as IniLineKV;
                    if (ilkv.GroupName.ToLower() == group.ToLower())
                    {
                        groupExists = true;
                        if (ilkv.ItemName.ToLower() == item.ToLower())
                        {
                            if (ilkv.ItemValue != value)
                            {
                                //found it!
                                Dirty = true;
                                ilkv.ItemValue = value;
                                ilkv.dirty = true;
                                return;
                            }
                        }
                    }
                }
            }
            //we didn't find it, have to add it.

            //if the group doesn't exist, we can just append this to the end of the file
            if (!groupExists)
            {
                Dirty = true;
                Contents.Add(new IniLine("")); //whitespace
                Contents.Add(new IniLineGroup("[" + group + "]", group));
                Contents.Add(new IniLineKV(item + "=" + value, group));
            } else
            {
                //otherwise we have to do a little legwork
                //scan for the last line of the target group
                int l = 0;
                int ll = 0;
                bool g = false;
                for(; l < Contents.Count; l++)
                {
                    if (!g) //still searching for the appropriate group
                    {
                        if (Contents[l] is IniLineGroup)
                        {
                            if((Contents[l] as IniLineGroup).GroupName == group)
                            {
                                //found the group
                                g = true;
                                continue;
                            }
                        }
                    } else //we have a group, seeking to the end of it
                    {
                        if(Contents[l] is IniLineKV)
                        {
                            if ((Contents[l] as IniLineKV).GroupName != group)
                            {
                                //ll is now our target index. break out!
                                break;
                            }
                                ll = l + 1;
                        }    
                    }
                }
                Dirty = true;
                Contents.Insert(ll, new IniLineKV(item + "=" + value, group));
            }
        }

        public void ReadFile(string file)
        {
            //If the INI exists, read our values, otherwise leave them default
            if (File.Exists(file))
            {
                filepath = file;
                Log.Info("Reading INI file: " + file);
                //quick scan for trailing newline
                string rawFile = File.ReadAllText(file);
                bool trailingNewline = false;
                if(rawFile[rawFile.Length - 1] == '\n' || rawFile[rawFile.Length - 1] == '\r') trailingNewline = true;

                //read lines to an array
                string[] lines = File.ReadAllLines(file);
                string group = null; //track the current grouping
                int lineNumber = 0;

                //iterate the lines for parsing
                foreach (string s in lines)
                {
                    lineNumber += 1;
                    string ss = s.Trim();
                    //strip comments, starting with one of //  ;  #
                    ss = ss.Split(new string[] { "//", ";", "#" }, StringSplitOptions.None)[0];
                    
                    //is this empty or comment?
                    if (string.IsNullOrWhiteSpace(ss))
                    {
                        //add as a raw line, it's either whitespace or a comment
                        Contents.Add(new IniLine(s));
                        continue;
                    }

                    //is this a group identifier?
                    if (ss.StartsWith("[") && ss.EndsWith("]")) {
                        group = ss.Substring(1, ss.Length - 2);
                        Contents.Add(new IniLineGroup(s, group));
                        continue;
                    }

                    //is this a k/v pair?
                    if (ss.Contains("="))
                    {
                        Contents.Add(new IniLineKV(s, group));
                        continue;
                    }

                    //srsly, wtf is this?! let's just... try to ignore it.
                    Log.Warn("Ignoring unparseable INI line " + lineNumber + ": " + s.Trim());
                    Contents.Add(new IniLine(s));
                }

                if (trailingNewline)
                {
                    Contents.Add(new IniLine(""));
                }
            } else
            {
                Log.Warn("INI file could not be located. Proceeding with defaults.");
                //create an artificial file Contents here
            }
        }

        public void Save()
        {
            if (Dirty)
            {
                string output = "";
                for (int line = 0; line < Contents.Count; line++)
                {
                    output += Contents[line].GetForWrite();
                    if (line < Contents.Count - 1) output += "\r\n";
                }
                Log.Info("Writing configuration file: " + filepath);
                File.WriteAllText(filepath, output);
            }
        }

        private class IniLine
        {
            //set this flag to recompile the line, otherwise just restore the raw input
            public bool dirty = false;

            public string Raw = "";
            public IniLine(string raw)
            {
                Raw = raw;
            }
            public virtual string Compile()
            {
                return Raw;
            }

            public string GetForWrite()
            {
                if (dirty) return Compile();
                else return Raw;
            }
        }

        private class IniLineGroup : IniLine
        {
            public string GroupName = null;
            public IniLineGroup(string raw, string proper) : base(raw)
            {
                GroupName = proper;
            }
        }

        private class IniLineKV : IniLine
        {
            public string GroupName = null; //the proper (trimmed) name of the current group
            public string ItemNamePrefix = ""; //everything to the left of ItemName
            public string ItemName = ""; //the trimmed content to the left of "="
            public string ItemNameSuffix = ""; //everything between the ItemName and the "="
            public string ItemValue = ""; //the trimmed content to the right of "="
            public string ItemValuePrefix = ""; //everything between "=" and ItemValue
            public string ItemValueSuffix = ""; //everything beyond ItemValue

            public IniLineKV(string raw, string group) : base(raw)
            {
                GroupName = group;
                string[] kv = raw.Split('=');
                ItemName = kv[0];
                ItemNamePrefix = kv[0].Substring(0, kv[0].IndexOf(ItemName));
                ItemNameSuffix = kv[0].Substring(kv[0].IndexOf(ItemName) + ItemName.Length);
                ItemValue = kv[1].Split(new string[] { "//", ";", "#" }, StringSplitOptions.None)[0].Trim();
                ItemValuePrefix = kv[1].Substring(0, kv[1].IndexOf(ItemValue));
                ItemValueSuffix = kv[1].Substring(kv[1].IndexOf(ItemValue) + ItemValue.Length);
            }

            public override string Compile()
            {
                string result = ItemNamePrefix;
                result += ItemName;
                result += ItemNameSuffix;
                result += "=";
                result += ItemValuePrefix;
                result += ItemValue;
                result += ItemValueSuffix;
                return result;
            }
        }
    }
}
