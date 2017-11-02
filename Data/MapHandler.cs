using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class MapHandler
    {
        public static int tileSize = 32;
        public static int startMapId = -1;
        public static int startMapX = -1;
        public static int startMapY = -1;

        public MapInfo Info;
        public dynamic rbMap;
        public dynamic rbTileset;
        public string Tileset;

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
            rbTileset = Editor.Project.Database.Tilesets[tilesetId];
            Tileset = ((IronRuby.Builtins.MutableString)rbTileset.tileset_name).ToString();
        }

        public string GetEventTooltip(int x, int y)
        {
            //check starting position first
            if(startMapId == Info.Id)
            {
                if (x == startMapX && y == startMapY) return "Player";
            }
            //check for events
            if (rbMap.events.Count > 0)
            {
                Dictionary<object, object> events = rbMap.events;
                foreach (dynamic o in events)
                {
                    if(o.Value.x == x && o.Value.y == y)
                    {
                        return o.Value.id.ToString("d3") + ": " + o.Value.name.ToString();
                    }
                }
            }
            //nothing here
            return "";
        }

        public void SetTile(int column, int row, int layer, int tileId)
        {
            rbMap.data[column, row, layer] = tileId;
        }
    }
}
