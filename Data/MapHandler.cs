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

        public MapInfo Info;
        public dynamic rbMap;
        public dynamic rbTileset;
        public string Tileset;
        public Bitmap TilesetBitmap; //TODO: seriously don't load a separate copy for every map
        public Bitmap RenderLayer1;
        public Bitmap RenderLayer1d;
        public Bitmap RenderLayer2;
        public Bitmap RenderLayer2d;
        public Bitmap RenderLayer3;
        public Bitmap RenderLayer3d;
        public Bitmap GrayLayer;
        public Bitmap GridLayer;
        private bool firstDrawn = false;

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

        public Bitmap CreateFadedBitmap(Image image, float opacity)
        {
            var colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = opacity;
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            var output = new Bitmap(image.Width, image.Height);
            using (var gfx = Graphics.FromImage(output))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                gfx.DrawImage(
                    image,
                    new Rectangle(0, 0, image.Width, image.Height),
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
            return output;
        }

        public void PaintEditor(System.Windows.Forms.Panel p, System.Windows.Forms.PaintEventArgs e)
        {
            if (!firstDrawn) return;
            int widthInTiles = (int)rbMap.width;
            int heightInTiles = (int)rbMap.height; 
            p.Width = widthInTiles * tileSize;
            p.Height = heightInTiles * tileSize;
            p.BackColor = Color.AliceBlue;

            //draw the map bitmaps
            bool dim = Editor.DimOtherLayers;
            bool viewAll = Editor.ViewAllLayers;
            int activeLayer = 0;
            switch (Editor.ActiveLayer)
            {
                case LayerType.LAYER2: activeLayer = 1; break;
                case LayerType.LAYER3: activeLayer = 2; break;
                case LayerType.EVENTS: activeLayer = 3; break;
            }
            if (activeLayer < 3)
            {
                //tile layer 1
                e.Graphics.DrawImage(RenderLayer1, Point.Empty);
                if (activeLayer < 1 && !viewAll) return;
                //tile layer 2
                if (activeLayer == 1 && dim) e.Graphics.DrawImage(GrayLayer, Point.Empty);
                if (activeLayer < 1 && dim) e.Graphics.DrawImage(RenderLayer2d, Point.Empty);
                else e.Graphics.DrawImage(RenderLayer2, Point.Empty);
                //tile layer 3
                if (activeLayer == 2 && dim) e.Graphics.DrawImage(GrayLayer, Point.Empty);
                if (activeLayer < 2 && !viewAll) return;
                if (activeLayer < 2 && dim) e.Graphics.DrawImage(RenderLayer3d, Point.Empty);
                else e.Graphics.DrawImage(RenderLayer3, Point.Empty);
            } else
            {
                e.Graphics.DrawImage(RenderLayer1, Point.Empty);
                e.Graphics.DrawImage(RenderLayer2, Point.Empty);
                e.Graphics.DrawImage(RenderLayer3, Point.Empty);
                e.Graphics.DrawImage(GridLayer, Point.Empty);
            }
        }

        public void DrawLayer(int dataLayer)
        {
            int widthInTiles = (int)rbMap.width;
            int heightInTiles = (int)rbMap.height;
            Bitmap layer = null;
            switch (dataLayer)
            {
                case 0:
                    if (RenderLayer1 != null) RenderLayer1.Dispose();
                    RenderLayer1 = new Bitmap(tileSize * (int)rbMap.width, tileSize * (int)rbMap.height);
                    layer = RenderLayer1;
                    break;
                case 1:
                    if (RenderLayer2 != null) RenderLayer2.Dispose();
                    RenderLayer2 = new Bitmap(tileSize * (int)rbMap.width, tileSize * (int)rbMap.height);
                    layer = RenderLayer2;
                    break;
                case 2:
                    if (RenderLayer3 != null) RenderLayer3.Dispose();
                    RenderLayer3 = new Bitmap(tileSize * (int)rbMap.width, tileSize * (int)rbMap.height);
                    layer = RenderLayer3;
                    break;
            }
            using (Graphics g = Graphics.FromImage(layer))
            {
                for (int x = 0; x < widthInTiles; x++)
                {
                    for (int y = 0; y < heightInTiles; y++)
                    {
                        int tileId = (int)rbMap.data[x, y, dataLayer];
                        //first tile in the main tileset image is tile 384
                        if (tileId < 384)
                        {
                            //autotile
                            //TODO
                        }
                        else
                        {
                            //get an image-local index
                            int localIndex = tileId - 384;
                            //get an image row and column
                            int row = localIndex % 8;
                            int column = localIndex / 8;
                            Rectangle destination = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                            Rectangle source = new Rectangle(row * tileSize, column * tileSize, tileSize, tileSize);
                            g.DrawImage(TilesetBitmap, destination, source, GraphicsUnit.Pixel);
                        }
                    }
                }
            }
            //create dimmed (transparent) bitmap
            switch (dataLayer)
            {
                case 0:
                    if (RenderLayer1d != null) RenderLayer1d.Dispose();
                    RenderLayer1d = CreateFadedBitmap(RenderLayer1, 0.25f);
                    break;
                case 1:
                    if (RenderLayer2d != null) RenderLayer2d.Dispose();
                    RenderLayer2d = CreateFadedBitmap(RenderLayer2, 0.25f);
                    break;
                case 2:
                    if (RenderLayer3d != null) RenderLayer3d.Dispose();
                    RenderLayer3d = CreateFadedBitmap(RenderLayer3, 0.25f);
                    break;
            }
        }

        public void FirstDraw()
        {
            //setup gray layer
            if (GrayLayer != null) GrayLayer.Dispose();
            GrayLayer = new Bitmap(tileSize * (int)rbMap.width, tileSize * (int)rbMap.height);
            using (Graphics g = Graphics.FromImage(GrayLayer))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(64, 0, 0, 0)))
                {
                    g.FillRectangle(brush, 0, 0, tileSize * (int)rbMap.width, tileSize * (int)rbMap.height);
                }
            }

            //setup event grid layer
            if (GridLayer != null) GridLayer.Dispose();
            GridLayer = new Bitmap(tileSize * (int)rbMap.width, tileSize * (int)rbMap.height);
            using (Graphics g = Graphics.FromImage(GridLayer))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(64, 0, 0, 0)))
                {
                    int widthInTiles = (int)rbMap.width;
                    int heightInTiles = (int)rbMap.height;
                    //draw vertical lines
                    for(int i = 1; i < widthInTiles; i++)
                    {
                        g.FillRectangle(brush, (tileSize * i) - 1, 0, 2, tileSize * (int)rbMap.height);
                    }
                    //draw horizontal lines
                    for (int i = 1; i < heightInTiles; i++)
                    {
                        g.FillRectangle(brush, 0, (tileSize * i) - 1, tileSize * (int)rbMap.width, 2);
                    }
                }
            }

            //draw map layers
            DrawLayer(0);
            DrawLayer(1);
            DrawLayer(2);
            firstDrawn = true;
        }
    }
}
