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

        public void FloodFill(int column, int row, int layer, int tileId)
        {
            FloodSourceTile = rbMap.data[column, row, layer];
            FloodLayer = layer;
            FloodDestTile = tileId;
            FloodTiles = new List<Point>();
            FloodTiles.Add(new Point(column, row));
            while(FloodTiles.Count > 0)
            {
                UpdateFloodFill();
            }
        }

        private List<Point> FloodTiles;
        private int FloodSourceTile = -1;
        private int FloodLayer = -1;
        private int FloodDestTile = -1;

        private void UpdateFloodFill()
        {
            Point tile = FloodTiles.First();
            FloodTiles.Remove(tile);
            //if this is not a valid target, simply return.
            if(rbMap.data[tile.X, tile.Y, FloodLayer] == FloodSourceTile)
            {
                //update this tile...
                rbMap.data[tile.X, tile.Y, FloodLayer] = FloodDestTile;
                //then add all its neighbors
                FloodTiles.Add(new Point(tile.X - 1, tile.Y)); //left
                FloodTiles.Add(new Point(tile.X + 1, tile.Y)); //right
                FloodTiles.Add(new Point(tile.X, tile.Y - 1)); //up
                FloodTiles.Add(new Point(tile.X, tile.Y + 1)); //down
            }
        }
    }
}
