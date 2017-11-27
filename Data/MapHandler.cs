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

        public void SetRectangle(int x1, int x2, int y1, int y2, int layer, int tileId)
        {
            //sanitize coordinates
            x1 = x1 < 0 ? 0 : x1 > rbMap.width ? rbMap.width : x1;
            x2 = x2 < 0 ? 0 : x2 > rbMap.width ? rbMap.width : x2;
            y1 = y1 < 0 ? 0 : y1 > rbMap.height ? rbMap.height : y1;
            y2 = y2 < 0 ? 0 : y2 > rbMap.height ? rbMap.height : y2;
            if(x2 < x1)
            {
                int vx = x2;
                x2 = x1;
                x1 = vx;
            }
            if (y2 < y1)
            {
                int vy = y2;
                y2 = y1;
                y1 = vy;
            }
            for (int ix = x1; ix <= x2; ix++)
            {
                for(int iy = y1; iy <= y2; iy++)
                {
                    rbMap.data[ix, iy, layer] = tileId;
                }
            }
            //Console.WriteLine("Drawing rectangle: " + x1 + ", " + x2 + ", " + y1 + ", " + y2);
        }

        public void SetEllipse(int x1, int x2, int y1, int y2, int layer, int tileId)
        {
            //sanitize coordinates
            x1 = x1 < 0 ? 0 : x1 > rbMap.width ? rbMap.width : x1;
            x2 = x2 < 0 ? 0 : x2 > rbMap.width ? rbMap.width : x2;
            y1 = y1 < 0 ? 0 : y1 > rbMap.height ? rbMap.height : y1;
            y2 = y2 < 0 ? 0 : y2 > rbMap.height ? rbMap.height : y2;
            if (x2 < x1)
            {
                int vx = x2;
                x2 = x1;
                x1 = vx;
            }
            if (y2 < y1)
            {
                int vy = y2;
                y2 = y1;
                y1 = vy;
            }
            int width = (x2 - x1);
            int height = (y2 - y1);
            if(width <= 1 || height <= 1)
            {
                //just fill it as a rectangle
                SetRectangle(x1, x2, y1, y2, layer, tileId);
                return;
            }
            int halfw = width / 2;
            int halfh = height / 2;
            bool wodd = (x2 - x1) % 2 == 1;
            bool hodd = (y2 - y1) % 2 == 1;
            FillEllipse(x1 + halfw, y1 + halfh, halfw, halfh, wodd, hodd, layer, tileId);
            Console.WriteLine("Drawing ellipse: " + x1 + ", " + x2 + ", " + y1 + ", " + y2);
        }

        void FillEllipse(int CenterX, int CenterY, int EllipseWidth, int EllipseHeight, bool WidthOdd, bool HeightOdd, int Layer, int TileId)
        {
            int a2 = EllipseWidth * EllipseWidth;
            int b2 = EllipseHeight * EllipseHeight;
            int fa2 = 4 * a2, fb2 = 4 * b2;
            int x, y, sigma;

            /* first half */
            for (x = 0, y = EllipseHeight, sigma = 2 * b2 + a2 * (1 - 2 * EllipseHeight); b2 * x <= a2 * y; x++)
            {
                SetRectangle(CenterX - x, CenterX + x + (WidthOdd ? 1 : 0), CenterY + y, CenterY + y, Layer, TileId);
                SetRectangle(CenterX - x, CenterX + x + (WidthOdd ? 1 : 0), CenterY - y, CenterY - y, Layer, TileId);
                if (sigma >= 0)
                {
                    sigma += fa2 * (1 - y);
                    y--;
                }
                sigma += b2 * ((4 * x) + 6);
            }
            /* second half */
            for (x = EllipseWidth, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * EllipseWidth); a2 * y <= b2 * x; y++)
            {
                SetRectangle(CenterX - x, CenterX + x + (WidthOdd ? 1 : 0), CenterY + y, CenterY + y, Layer, TileId);
                SetRectangle(CenterX - x, CenterX + x + (WidthOdd ? 1 : 0), CenterY - y, CenterY - y, Layer, TileId);
                //SetTile(CenterX + x, CenterY + y, Layer, TileId);
                //SetTile(CenterX - x, CenterY + y, Layer, TileId);
                //SetTile(CenterX + x, CenterY - y, Layer, TileId);
                //SetTile(CenterX - x, CenterY - y, Layer, TileId);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
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
