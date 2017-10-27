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
        private static dynamic datahelper;

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
            datahelper = engine.Execute(@"RbDataHelper.new", scope);
        }

        public void PopulateMapInfos(MapInfos infos)
        {
            //Load rxdata
            try
            {
                //read a List from MapInfos.rxdata
                List<dynamic> mapInfoList = ToList(datahelper.load_map_infos());
                foreach(dynamic d in mapInfoList)
                {
                    List<dynamic> entry = ToList(d);
                    infos.AddMap((int) entry[0], entry[1].ToString(), (int) entry[2], (int) entry[3], (bool) entry[4], (int) entry[5], (int)entry[6]);
                    Console.WriteLine("Loaded mapinfo for map id " + entry[0].ToString() + ": " + entry[1].ToString());
                    //todo: load the accompanying map rxdata here
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
                ra.Add(info.ParentId);
                ra.Add(info.Order);
                ra.Add(info.Expanded);
                ra.Add(info.ScrollX);
                ra.Add(info.ScrollY);
                maps.Add(ra);
            }
            datahelper.save_map_infos(infos);
        }

        public void PopulateScriptHive(ScriptHive hive)
        {
            //Load rxdata
            try
            {
                engine.Execute(@"load_scripts", scope);
                int scriptCount = (int)engine.Execute("get_script_count", scope);
                if(scriptCount > 0)
                {
                    for(int i = 0; i < scriptCount; i++)
                    {
                        int scriptMagic = (int)engine.Execute(@"get_script_magic(" + i + ")", scope);
                        string scriptName = (string) engine.Execute(@"get_script_name(" + i + ")", scope);
                        string scriptContents = (string) engine.Execute(@"get_script_contents(" + i + ")", scope);
                        hive.AddScript(new Script(scriptMagic, scriptName, scriptContents));
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void WriteScriptHive(ScriptHive hive)
        {
            //Save rxdata
            try
            {
                engine.Execute(@"save_scripts_start", scope);
                foreach(Script s in hive.Scripts)
                {
                    System.IO.File.WriteAllText(System.IO.Path.Combine(Editor.Project.Directory, "script0.tmp"), s.MagicNumber.ToString());
                    System.IO.File.WriteAllText(System.IO.Path.Combine(Editor.Project.Directory, "script1.tmp"), s.Name);
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(Editor.Project.Directory, "script2.tmp"), GameData.DataHelper.Deflate(s.Contents));
                    engine.Execute(@"install_script", scope);
                    System.IO.File.Delete(System.IO.Path.Combine(Editor.Project.Directory, "script0.tmp"));
                    System.IO.File.Delete(System.IO.Path.Combine(Editor.Project.Directory, "script1.tmp"));
                    System.IO.File.Delete(System.IO.Path.Combine(Editor.Project.Directory, "script2.tmp"));
                }
                engine.Execute(@"save_scripts_end", scope);
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
