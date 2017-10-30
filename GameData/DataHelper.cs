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
            //Vanilla RMXP uses a different configuration for
            //Deflate, with version header and adler32 footer.

            //we need interoperability, so we're going to manually
            //tweak what Ionic.Zlib produces to match.
            List<byte> result = new List<byte>();

            //RMXP-compatible format header bytes
            result.Add(120);
            result.Add(156);

            //actual compressed string
            result.AddRange(DeflateStream.CompressString(s));

            //adler32, last 4 bytes
            result.AddRange(adler32(Encoding.UTF8.GetBytes(s)));

            return result.ToArray();
        }

        private static byte[] adler32(byte[] data)
        {
            UInt32 a = 1, b = 0;
            int index;
            for (index = 0; index < data.Length; ++index)
            {
                a = (a + data[index]) % 65521;
                b = (b + a) % 65521;
            }

            byte[] result = BitConverter.GetBytes((b << 16) | a);
            if (BitConverter.IsLittleEndian)
            {
                result = result.Reverse().ToArray();
            }
            return result;
        }
    }
}
