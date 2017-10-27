using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP.GameData
{
    //serves as a custom implementation of Ruby 1.8.1's Zlib and Marshal, directly here in C#.
    //this could come in handy for porting later.
    class DataHelper
    {
        //maybe todo for the future: this is marshal as it was at 1.8.1.
        //https://github.com/ruby/ruby/blob/bf7bf7efeaf8f05926af673898a09d04f03599a1/marshal.c

        public static string Inflate(byte[] s)
        {
            //sanity check for empty or illegal strings
            if (s.Length <= 6) return "";
            //sub out header and adler bytes
            byte[] sub = new byte[s.Length - 6];
            Array.Copy(s, 2, sub, 0, s.Length - 6);
            //uncompress and return
            return DeflateStream.UncompressString(sub);
        }

        public static byte[] Deflate(string s)
        {
            List<byte> result = new List<byte>();
            //RMXP-compatible format header bytes
            result.Add(120);
            result.Add(156);

            //actual compressed string
            result.AddRange(DeflateStream.CompressString(s));

            //adler32, last 4 bytes
            result.AddRange(GenerateAdlerFrame(s));

            return result.ToArray();
        }

        //returns a 4 byte frame
        private static byte[] GenerateAdlerFrame(string data)
        {
            long s1 = 1;
            long s2 = 0;
            byte[] raw = Encoding.UTF8.GetBytes(data);
            foreach(byte b in raw)
            {
                s1 += b;
                s2 += s1;
            }
            s1 %= 65536;
            s2 %= 65536;
            int r = (int)(s2 * 65536 + s1);
            byte[] result = BitConverter.GetBytes(r);
            if (BitConverter.IsLittleEndian)
            {
                result = result.Reverse().ToArray();
            }
            return result;
        }
    }

    public class MarshalContainer
    {
        public List<dynamic> objects = new List<dynamic>();
    }
}
