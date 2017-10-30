using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class Ruby
    {
        private static ScriptRuntime runtime;
        private static ScriptEngine engine;
        private static ScriptScope scope;
        public static dynamic rbhelper;

        public Ruby()
        {
            //Setup the script engine runtime
            var setup = new ScriptRuntimeSetup();
            setup.LanguageSetups.Add(
                new LanguageSetup(
                    "IronRuby.Runtime.RubyContext, IronRuby",
                    "IronRuby 1.0",
                    new[] { "IronRuby", "Ruby", "rb" },
                    new[] { ".rb" }));
            setup.DebugMode = true;

            //Create the runtime, engine, and scope
            runtime = ScriptRuntime.CreateRemote(AppDomain.CurrentDomain, setup);
            engine = runtime.GetEngine("Ruby");            
            scope = engine.CreateScope();

            //Initialize system scripts
            try
            {
                engine.Execute(@"$GAME_DIRECTORY = '" + Editor.Project.Directory.Replace(@"\", @"\\") + @"'", scope);
                engine.Execute(@"$RGSS_SCRIPTS_PATH = '" + Editor.Project.Ini.Scripts.Replace(@"\", @"\\") + @"'");
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            string script = System.Text.Encoding.UTF8.GetString(Properties.Resources.System);
            script = script.Substring(1);  //fix for a weird character that shouldn't be there o.O
            Eval(script);

            //Create our datahelper instance
            rbhelper = engine.Execute(@"RxDataHelper.new", scope);
        }

        public dynamic RubyDeepCopy(dynamic obj)
        {
            return rbhelper.deep_copy(obj);
        }

        public void PopulateMapInfos(MapInfos infos)
        {
            try
            {
                //read a List from MapInfos.rxdata
                List<dynamic> mapInfoList = ToList(rbhelper.load_map_infos());
                foreach(dynamic d in mapInfoList)
                {
                    List<dynamic> entry = ToList(d);
                    infos.AddMap((int) entry[0], entry[1].ToString(), (int) entry[2], (int) entry[3], (bool) entry[4], (int) entry[5], (int)entry[6], entry[7]);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void WriteMapInfos(MapInfos infos)
        {
            List<IronRuby.Builtins.RubyArray> maps = new List<IronRuby.Builtins.RubyArray>();
            foreach(MapInfo info in infos.Maps)
            {
                IronRuby.Builtins.RubyArray ra = new IronRuby.Builtins.RubyArray();
                ra.Add(info.Id);
                ra.Add(info.Name);
                ra.Add(info.ParentId);
                ra.Add(info.Order);
                ra.Add(info.Expanded);
                ra.Add(info.ScrollX);
                ra.Add(info.ScrollY);
                ra.Add(info.Map.rbMap); //ta-da, found a way
                maps.Add(ra);
            }
            rbhelper.save_map_infos(maps);
        }

        public void PopulateDatabase(Database db)
        {
            rbhelper.load_database(db);
        }

        public void WriteDatabase(Database db)
        {
            rbhelper.save_database(db);
        }

        public void PopulateScriptHive(ScriptHive hive)
        {
            try
            {
                List<dynamic> scrArray = ToList(rbhelper.load_scripts());
                foreach(dynamic scr in scrArray)
                {
                    hive.AddScript(new Script((int)scr[0], scr[1].ToString(), GameData.DataHelper.Inflate(((IronRuby.Builtins.MutableString)scr[2]).ConvertToBytes())));
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void WriteScriptHive(ScriptHive hive)
        {
            try
            {
                IronRuby.Builtins.RubyArray ra = new IronRuby.Builtins.RubyArray();
                foreach(Script s in hive.Scripts)
                {
                    IronRuby.Builtins.RubyArray ra_entry = new IronRuby.Builtins.RubyArray();
                    ra_entry.Add(s.MagicNumber);
                    ra_entry.Add(s.Name);
                    var ms = new IronRuby.Builtins.MutableString();
                    ms = ms.ChangeEncoding(IronRuby.Builtins.RubyEncoding.Binary, true);
                    ms.Append(GameData.DataHelper.Deflate(s.Contents));
                    ra_entry.Add(ms);
                    ra.Add(ra_entry);
                }
                rbhelper.save_scripts(ra);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void Eval(string str)
        {
            try
            {
                var source = engine.CreateScriptSourceFromString(str);
                source.Compile(new ReportingErrorListener());
                source.Execute(scope);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void Dispose()
        {
            scope = null;
            engine.Runtime.Shutdown();
            engine = null;
        }

        //language interops
        public static dynamic CreateRubyInstance(string className)
        {
            return engine.Execute(className + @".new", scope);
        }
        
        public static IronRuby.Builtins.MutableString RString(byte[] item)
        {
            IronRuby.Builtins.MutableString s = new IronRuby.Builtins.MutableString();
            s.Append(item);
            return s;
        }

        public static List<dynamic> ToList(dynamic source)
        {
            if (IsArray(source))
            {
                var found = new List<dynamic>();
                for (int i = 0; i < source.Count; i++)
                {
                    object next = source[i]; found.Add(next);
                }

                return found;
            } else
            {
                Console.WriteLine("Error: Tried to create a list from an invalid source!");
                return new List<dynamic>();
            }
        }

        public static bool IsArray(dynamic source)
        {
            return source.GetType().Name == "RubyArray";
        }
    }


    public class ReportingErrorListener : ErrorListener
    {
        public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity)
        {
            System.Windows.Forms.MessageBox.Show(message);
        }
    }
}
