using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using OpenXP;
namespace XPT
{
    public partial class UserControlTilemap : UserControl
    {
        public const int TileSize = 32;
        public MapHandler Map;
        public Bitmap TilesetBitmap; //TODO: seriously don't load a separate copy for every map
        public Bitmap RenderLayer1;
        public Bitmap RenderLayer1d;
        public Bitmap RenderLayer2;
        public Bitmap RenderLayer2d;
        public Bitmap RenderLayer3;
        public Bitmap RenderLayer3d;
        public Bitmap GrayLayer;
        public Bitmap GridLayer;

        public static Bitmap eventOverlay = new Bitmap(OpenXP.Properties.Resources.EventOverlay);
        public static Bitmap startImage = new Bitmap(OpenXP.Properties.Resources.StartPos);
        public static Bitmap startImageFade = CreateFadedBitmap(startImage, 0.25f);

        private bool firstDrawn = false;

        public Panel container;

        public UserControlTilemap()
        {
            InitializeComponent();

            //optimize for drawing
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void SetTile(int column, int row, int layer, int tileId)
        {
            Map.SetTile(column, row, layer, tileId);
            DrawLayer(layer);
            Editor.Touch();
            Redraw();
        }

        public Point DrawPolyPointA;
        public Point DrawPolyPointB;
        public int DrawPolyLayer;
        public int DrawPolyTile;

        public void StartRectangle(int column, int row, int layer, int tileId)
        {
            DrawPolyLayer = layer;
            DrawPolyTile = tileId;
            DrawPolyPointA = new Point(column, row);
            DrawPolyPointB = new Point(column, row);
        }

        public void UpdateRectangle(int column, int row)
        {
            DrawPolyPointB.X = column;
            DrawPolyPointB.Y = row;
        }

        public void EndRectangle()
        {
            int x1 = DrawPolyPointA.X;
            int x2 = DrawPolyPointB.X;
            int y1 = DrawPolyPointA.Y;
            int y2 = DrawPolyPointB.Y;
            //notably these can run out of bounds, so we'll have to sanitize it later
            Map.SetRectangle(x1, x2, y1, y2, DrawPolyLayer, DrawPolyTile);
            DrawLayer(DrawPolyLayer);
            Editor.Touch();
            Redraw();
        }

        public void StartEllipse(int column, int row, int layer, int tileId)
        {
            DrawPolyLayer = layer;
            DrawPolyTile = tileId;
            DrawPolyPointA = new Point(column, row);
            DrawPolyPointB = new Point(column, row);
        }

        public void UpdateEllipse(int column, int row)
        {
            DrawPolyPointB.X = column;
            DrawPolyPointB.Y = row;
        }

        public void EndEllipse()
        {
            int x1 = DrawPolyPointA.X;
            int x2 = DrawPolyPointB.X;
            int y1 = DrawPolyPointA.Y;
            int y2 = DrawPolyPointB.Y;
            //notably these can run out of bounds, so we'll have to sanitize it later
            Map.SetEllipse(x1, x2, y1, y2, DrawPolyLayer, DrawPolyTile);
            DrawLayer(DrawPolyLayer);
            Editor.Touch();
            Redraw();
        }

        public void FloodFill(int column, int row, int layer, int tileId)
        {
            Map.FloodFill(column, row, layer, tileId);
            DrawLayer(layer);
            Editor.Touch();
            Redraw();
        }

        public void Redraw()
        {
            if (container != null && firstDrawn)
            {
                Invalidate();
            }
        }

        public Bitmap LoadTileset()
        {
            foreach (string path in Editor.Project.ResourcePaths)
            {
                //todo, multiple extension support
                string file = "Graphics\\Tilesets\\" + Map.Tileset + ".png";
                if (System.IO.File.Exists(System.IO.Path.Combine(path, file)))
                {
                    return (Bitmap)Image.FromFile(System.IO.Path.Combine(path, file));
                }
            }
            Console.WriteLine("Could not locate tileset: " + Map.Tileset);
            return null;
        }

        public void SetMap(MapHandler map)
        {
            firstDrawn = false;

            //manually dock in parent
            container = Editor.Form.panelTilemapContainer;
            Parent = container;
            container.AutoScroll = true;

            Map = map;
            TilesetBitmap = LoadTileset();

            int zoomDivide = 1;
            if (Editor.ActiveZoomType == ZoomType.HALF) zoomDivide = 2;
            if (Editor.ActiveZoomType == ZoomType.QUARTER) zoomDivide = 4;

            int widthInTiles = (int)Map.rbMap.width;
            int heightInTiles = (int)Map.rbMap.height;

            Size = new Size((widthInTiles * TileSize) / zoomDivide, (heightInTiles * TileSize) / zoomDivide);
            Width = (widthInTiles * TileSize) / zoomDivide;
            //container.AutoScrollMinSize = new Size((widthInTiles * TileSize) / zoomDivide, (heightInTiles * TileSize) / zoomDivide);

            //setup gray layer
            if (GrayLayer != null) GrayLayer.Dispose();
            GrayLayer = new Bitmap(TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height);
            using (Graphics g = Graphics.FromImage(GrayLayer))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                {
                    g.FillRectangle(brush, 0, 0, TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height);
                }
            }

            //setup event grid layer
            if (GridLayer != null) GridLayer.Dispose();
            GridLayer = new Bitmap(TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height);
            using (Graphics g = Graphics.FromImage(GridLayer))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(64, 0, 0, 0)))
                {
                    //draw vertical lines
                    for (int i = 1; i < widthInTiles; i++)
                    {
                        g.FillRectangle(brush, (TileSize * i) - 1, 0, 2, TileSize * (int)Map.rbMap.height);
                    }
                    //draw horizontal lines
                    for (int i = 1; i < heightInTiles; i++)
                    {
                        g.FillRectangle(brush, 0, (TileSize * i) - 1, TileSize * (int)Map.rbMap.width, 2);
                    }
                }
            }

            //draw map layers
            DrawLayer(0);
            DrawLayer(1);
            DrawLayer(2);

            firstDrawn = true;
            Redraw();
        }

        public void DrawLayer(int dataLayer)
        {
            int widthInTiles = (int)Map.rbMap.width;
            int heightInTiles = (int)Map.rbMap.height;
            Bitmap layer = null;
            switch (dataLayer)
            {
                case 0:
                    if (RenderLayer1 != null) RenderLayer1.Dispose();
                    RenderLayer1 = new Bitmap(TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height);
                    layer = RenderLayer1;
                    using (Graphics g = Graphics.FromImage(layer))
                    {
                        g.FillRectangle(Brushes.White, new Rectangle(0, 0, TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height));
                    }
                    break;
                case 1:
                    if (RenderLayer2 != null) RenderLayer2.Dispose();
                    RenderLayer2 = new Bitmap(TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height);
                    layer = RenderLayer2;
                    break;
                case 2:
                    if (RenderLayer3 != null) RenderLayer3.Dispose();
                    RenderLayer3 = new Bitmap(TileSize * (int)Map.rbMap.width, TileSize * (int)Map.rbMap.height);
                    layer = RenderLayer3;
                    break;
            }
            using (Graphics g = Graphics.FromImage(layer))
            {
                for (int x = 0; x < widthInTiles; x++)
                {
                    for (int y = 0; y < heightInTiles; y++)
                    {
                        int tileId = (int)Map.rbMap.data[x, y, dataLayer];
                        //first tile in the main tileset image is tile 384
                        if (tileId < 384)
                        {
                            //todo: autotile support
                        }
                        else
                        {
                            //get an image-local index
                            int localIndex = tileId - 384;
                            //get an image row and column
                            int row = localIndex % 8;
                            int column = localIndex / 8;
                            Rectangle destination = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                            Rectangle source = new Rectangle(row * TileSize, column * TileSize, TileSize, TileSize);
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int zoomDivide = 1;
            if (Editor.ActiveZoomType == ZoomType.HALF) zoomDivide = 2;
            if (Editor.ActiveZoomType == ZoomType.QUARTER) zoomDivide = 4;

            if (!firstDrawn) return;
            int widthInTiles = (int)Map.rbMap.width;
            int heightInTiles = (int)Map.rbMap.height;

            //scrollme
            MinimumSize = new Size((widthInTiles * TileSize) / zoomDivide, (heightInTiles * TileSize) / zoomDivide);
            

            //zoomin'
            e.Graphics.ScaleTransform(1.0f / (float)zoomDivide, 1.0f / (float)zoomDivide);

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

                //draw starting position
                if (Editor.GetSelectedMapId() == MapHandler.startMapId)
                {
                    e.Graphics.DrawImage(startImageFade, new Point((MapHandler.startMapX * TileSize) + 3, (MapHandler.startMapY * TileSize) + 3));
                }
                //draw selection
                using (Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0))))
                {
                    pen.Width = 4;
                    e.Graphics.DrawRectangle(pen, (Editor.Form.MapHoverLocationX * 32), (Editor.Form.MapHoverLocationY * 32), 32, 32);
                }
                using (Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
                {
                    pen.Width = 2;
                    e.Graphics.DrawRectangle(pen, (Editor.Form.MapHoverLocationX * 32), (Editor.Form.MapHoverLocationY * 32), 32, 32);
                }
            }
            else
            {
                e.Graphics.DrawImage(RenderLayer1, Point.Empty);
                e.Graphics.DrawImage(RenderLayer2, Point.Empty);
                e.Graphics.DrawImage(RenderLayer3, Point.Empty);
                e.Graphics.DrawImage(GridLayer, Point.Empty);

                //draw starting position
                if (Editor.GetSelectedMapId() == MapHandler.startMapId)
                {
                    e.Graphics.DrawImage(startImage, new Point((MapHandler.startMapX * TileSize) + 3, (MapHandler.startMapY * TileSize) + 3));
                }

                //draw events
                if (Map.rbMap.events.Count > 0)
                {
                    Dictionary<object, object> events = Map.rbMap.events;
                    foreach (dynamic o in events)
                    {
                        e.Graphics.DrawImage(eventOverlay, new Point((o.Value.x * TileSize) + 2, (o.Value.y * TileSize) + 2));
                        //draw event graphic
                        string filename = o.Value.pages[0].graphic.character_name.ToString();
                        //todo: cache character graphics - urgent, this is a massive memory leak
                        Bitmap eventIcon = new Bitmap(28, 26);
                        Bitmap charset = LoadCharacter(filename);
                        using (Graphics gg = Graphics.FromImage(eventIcon))
                        {
                            gg.DrawImage(charset, new Rectangle(0, 0, 28, 26), new Rectangle(0, 0, charset.Width / 4, charset.Height / 7), GraphicsUnit.Pixel);
                        }
                        e.Graphics.DrawImage(eventIcon, new Point((o.Value.x * TileSize) + 2, (o.Value.y * TileSize) + 2));
                    }
                }

                //draw selection
                using (Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0))))
                {
                    pen.Width = 4;
                    e.Graphics.DrawRectangle(pen, (Editor.Form.MapEventSelectLocationX * 32), (Editor.Form.MapEventSelectLocationY * 32), 32, 32);
                }
                using (Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
                {
                    pen.Width = 2;
                    e.Graphics.DrawRectangle(pen, (Editor.Form.MapEventSelectLocationX * 32), (Editor.Form.MapEventSelectLocationY * 32), 32, 32);
                }
            }
        }

        public Bitmap LoadCharacter(string characterName)
        {
            foreach (string path in Editor.Project.ResourcePaths)
            {
                //todo, multiple extension support
                string file = "Graphics\\Characters\\" + characterName + ".png";
                if (System.IO.File.Exists(System.IO.Path.Combine(path, file)))
                {
                    return (Bitmap)Image.FromFile(System.IO.Path.Combine(path, file));
                }
            }
            Console.WriteLine("Could not locate character: " + characterName);
            return null;
        }

        public static Bitmap CreateFadedBitmap(Image image, float opacity)
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
    }
}