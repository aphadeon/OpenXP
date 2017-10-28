using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class GameIni
    {
        //TODO: preserve any unused data in Game.ini (atm this is very destructive and overwrites the whole file with just known settings)

        public string Library = "RGSS104E.dll";
        public string Scripts = "Data\\Scripts.rxdata";
        public string Title = "Project1";
        public string RTP1 = "Standard";
        public string RTP2 = "";
        public string RTP3 = "";

        public void Deserialize(string ini)
        {
            string[] lines = ini.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach(string s in lines)
            {
                if (s.Contains("="))
                {
                    string[] sy = s.Split(new[] { "=" }, StringSplitOptions.None);
                    if(sy.Length > 1)
                    {
                        if(sy[0].Trim().ToLower() == "library") Library = sy[1].Trim();
                        if(sy[0].Trim().ToLower() == "scripts") Scripts = sy[1].Trim();
                        if(sy[0].Trim().ToLower() == "title") Title = sy[1].Trim();
                        if(sy[0].Trim().ToLower() == "rtp1") RTP1 = sy[1].Trim();
                        if(sy[0].Trim().ToLower() == "rtp2") RTP2 = sy[1].Trim();
                        if(sy[0].Trim().ToLower() == "rtp3") RTP3 = sy[1].Trim();
                    }
                }
            }
        }

        public string Serialize()
        {
            return "[Game]\r\nLibrary=" + Library + "\r\nScripts=" + Scripts + "\r\nTitle=" + Title + "\r\nRTP1=" + RTP1 + "\r\nRTP2=" + "\r\nRTP3=" + RTP3;
        }
    }
}
