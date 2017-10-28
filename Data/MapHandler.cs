using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class MapHandler
    {
        public MapInfo Info;
        public dynamic rbMap;
        public dynamic rbTileset;
        public string Tileset;
        public Bitmap TilesetBitmap; //TODO: seriously don't load a separate copy for every map

        //constructor for empty, new map data
        public MapHandler(MapInfo info)
        {
            Info = info;
            throw new NotImplementedException(); //TODO
        }

        //constructor for existing map data
        public MapHandler(MapInfo info, dynamic map)
        {
            Info = info;
            rbMap = map;
            //to test, let's print the name of the tileset graphic
            int tilesetId = (int)rbMap.tileset_id;
            rbTileset = Ruby.rbhelper.grab_tileset(tilesetId);
            Tileset = ((IronRuby.Builtins.MutableString)rbTileset.tileset_name).ToString();
            TilesetBitmap = LoadTileset();
        }

        public Bitmap LoadTileset()
        {
            foreach(string path in Editor.Project.ResourcePaths)
            {
                //todo, multiple extension support
                string file = "Graphics\\Tilesets\\" + Tileset + ".png";
                if(System.IO.File.Exists(System.IO.Path.Combine(path, file)))
                {
                    return (Bitmap) Image.FromFile(System.IO.Path.Combine(path, file));
                }
            }
            Console.WriteLine("Could not locate tileset: " + Tileset);
            return null;
        }
    }
}
