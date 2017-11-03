using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenXP
{
    public partial class OpenXP : Form
    {
        public MapInfo SelectedMap { get {return _selectedMap; } set
            {
                var oldmap = _selectedMap;
                 _selectedMap = value;
                if (value != oldmap) OnSelectedMapChange(oldmap);
            }
        }
        public MapInfo _selectedMap = null;

        public int TilesetSelectionX = 0;
        public int TilesetSelectionY = 0;
        public int TilesetSelectionId = 0;
        public int MapHoverLocationX = -1;
        public int MapHoverLocationY = -1;
        public int MapEventSelectLocationX = -1;
        public int MapEventSelectLocationY = -1;

        public OpenXP()
        {
            InitializeComponent();
            Editor.Form = this;

            //changing to a different value triggers initial updates
            Editor.ActiveLayer = LayerType.LAYER1;
            Editor.ChangeLayer(LayerType.EVENTS);
            Editor.ActiveDrawTool = DrawToolType.SELECT;
            Editor.ChangeDrawTool(DrawToolType.PENCIL);

            treeViewMaps.ShowLines = false;
            treeViewMaps.ShowRootLines = false;
            treeViewMaps.ItemHeight = 20;
            treeViewMaps.TreeViewNodeSorter = new NodeSorter();
            treeViewMaps.AfterExpand += TreeViewMaps_AfterExpand;
            treeViewMaps.AfterCollapse += TreeViewMaps_AfterCollapse;
            treeViewMaps.AfterSelect += TreeViewMaps_AfterSelect;
            ImageList il = new ImageList();
            il.Images.Add("globe", Properties.Resources.globe);
            il.Images.Add("map", Properties.Resources.map);
            treeViewMaps.ImageList = il;

            tilemapMap.MouseLeave += tilemapMap_MouseLeave;
            tilemapMap.MouseMove += tilemapMap_MouseMove;
            tilemapMap.MouseClick += tilemapMap_MouseClick;

            panelTilemapContainer.HorizontalScroll.Enabled = true;
            panelTilemapContainer.VerticalScroll.Enabled = true;

            pictureBoxTileset.MouseClick += PictureBoxTileset_MouseClick;
            pictureBoxTileset.Paint += PictureBoxTileset_Paint;

            FormClosing += OpenXP_FormClosed;

            contextMenuStripMap.Opening += ContextMenuStripMap_Opening;

            //setup statusbar
            AddHoverEventToToolStripItems(toolbar.Items);
            AddHoverEventToToolStripItems(mainMenu.Items);

            //project file drop support
            AllowDrop = true;
            DragDrop += OpenXP_DragDrop;
            DragEnter += OpenXP_DragEnter;

            //load configuration here
            Editor.OnStartup();
        }

        //only allow a single file ending in .rxproj
        private void OpenXP_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop", false))
            {
                e.Effect = DragDropEffects.Copy;
                string[] paths = (string[])(e.Data.GetData("FileDrop", false));
                if (paths.Length > 1) e.Effect = DragDropEffects.None;
                foreach (string path in paths)
                {
                    if (!path.EndsWith(".rxproj"))
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
            }
            else e.Effect = DragDropEffects.None;
        }

        private void OpenXP_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop", false))
            {
                string[] paths = (string[])(e.Data.GetData("FileDrop", false));
                foreach (string path in paths)
                {
                    bool shouldContinue = Editor.CloseProject(); //out with the old
                    if (shouldContinue)
                    {
                        try
                        {
                            Editor.LoadProject(path);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                        }
                    }
                }
            }
        }

        //called when closing a project to be ready for a new one
        public void Cleanup()
        {
            tilemapMap.Visible = false;
            Console.Clear();
            _selectedMap = null;
            treeViewMaps.SelectedNode = null;
            treeViewMaps.Nodes.Clear();
            this.ActiveControl = tableLayoutPanel; //forceably unfocus the treeview so it can update to null
            treeViewMaps.Invalidate();
            MapHoverLocationX = -1;
            MapHoverLocationY = -1;
            MapEventSelectLocationX = -1;
            MapEventSelectLocationY = -1;
            TilesetSelectionId = 0;
            TilesetSelectionX = 0;
            TilesetSelectionY = 0;

        }

        private void AddHoverEventToToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem tsi in items)
            {
                tsi.MouseEnter += new EventHandler(this.onStatusBarObjectHoverStart);
                tsi.MouseLeave += new EventHandler(this.onStatusBarObjectHoverEnd);
                if (tsi is ToolStripMenuItem)
                {
                    ToolStripMenuItem mi = tsi as ToolStripMenuItem;
                    AddHoverEventToToolStripItems(mi.DropDownItems);
                }
            }
        }

        private void ContextMenuStripMap_Opening(object sender, CancelEventArgs e)
        {
            if(Editor.ActiveLayer != LayerType.EVENTS) e.Cancel = true;
            else
            {
                //manually emulate the selection change before opening
                int zoomDivide = 1;
                if (Editor.ActiveZoomType == ZoomType.HALF) zoomDivide = 2;
                if (Editor.ActiveZoomType == ZoomType.QUARTER) zoomDivide = 4;
                Point localMouse = tilemapMap.PointToClient(MousePosition);
                int hoverX = (localMouse.X * zoomDivide) / 32;
                int hoverY = (localMouse.Y * zoomDivide) / 32;
                if (hoverX >= 0 && hoverY >= 0)
                {
                    MapEventSelectLocationX = hoverX;
                    MapEventSelectLocationY = hoverY;
                    RepaintMap();
                }
            }
        }

        private void tilemapMap_MouseMove(object sender, MouseEventArgs e)
        {
            int zoomDivide = 1;
            if (Editor.ActiveZoomType == ZoomType.HALF) zoomDivide = 2;
            if (Editor.ActiveZoomType == ZoomType.QUARTER) zoomDivide = 4;

            int hoverX = (e.X * zoomDivide) / 32;
            int hoverY = (e.Y * zoomDivide) / 32;
            if(hoverX >= 0 && hoverY >= 0)
            {
                if (MapHoverLocationX != hoverX || MapHoverLocationY != hoverY) RepaintMap();
                MapHoverLocationX = hoverX;
                MapHoverLocationY = hoverY;
                if (Editor.ActiveLayer != LayerType.EVENTS)
                    toolStripStatusCoord.Text = MapHoverLocationX.ToString("d3") + ", " + MapHoverLocationY.ToString("d3");
            }
        }

        private void tilemapMap_MouseLeave(object sender, EventArgs e)
        {
            MapHoverLocationX = -1;
            MapHoverLocationY = -1;
            if (Editor.ActiveLayer != LayerType.EVENTS)
                toolStripStatusCoord.Text = "";
            tilemapMap.Redraw();
        }

        private void tilemapMap_MouseClick(object sender, MouseEventArgs e)
        {
            int zoomDivide = 1;
            if (Editor.ActiveZoomType == ZoomType.HALF) zoomDivide = 2;
            if (Editor.ActiveZoomType == ZoomType.QUARTER) zoomDivide = 4;

            if (Editor.ActiveLayer == LayerType.EVENTS)
            {
                int column = (e.X * zoomDivide) / 32;
                int row = (e.Y * zoomDivide) / 32;
                MapEventSelectLocationX = column;
                MapEventSelectLocationY = row;
                toolStripStatusCoord.Text = MapEventSelectLocationX.ToString("d3") + ", " + MapEventSelectLocationY.ToString("d3");
                tilemapMap.Redraw();
                if (SelectedMap != null) toolStripStatusEventInfo.Text = SelectedMap.Map.GetEventTooltip(column, row);
            } else {
                int column = (e.X * zoomDivide) / 32;
                int row = (e.Y * zoomDivide) / 32;
                if (SelectedMap != null)
                {
                    int layer = 0;
                    switch (Editor.ActiveLayer)
                    {
                        case LayerType.LAYER2: layer = 1; break;
                        case LayerType.LAYER3: layer = 2; break;
                    }
                    tilemapMap.SetTile(column, row, layer, TilesetSelectionId);
                }
            }
        }

        private void PictureBoxTileset_Paint(object sender, PaintEventArgs e)
        {
            //draw selection indicator
            if (TilesetSelectionX >= 0 && TilesetSelectionY >= 0) { //if we /have/ a selection
                Graphics g = e.Graphics;
                using (Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0))))
                {
                    pen.Width = 4;
                    g.DrawRectangle(pen, TilesetSelectionX * 32, TilesetSelectionY * 32, 32, 32);
                }
                using (Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
                {
                    pen.Width = 2;
                    g.DrawRectangle(pen, (TilesetSelectionX * 32), (TilesetSelectionY * 32), 32, 32);
                }
            }
        }

        private void PictureBoxTileset_MouseClick(object sender, MouseEventArgs e)
        {
            int column = e.X / 32;
            int row = e.Y / 32;
            int index = (row * 8) + column;

            TilesetSelectionX = column;
            TilesetSelectionY = row;
            if (index > 7)
            {
                //todo, test for max id and ignore the click if it was exceeded
                TilesetSelectionId = (index - 8) + 384;
            } else
            {
                TilesetSelectionId = 0; //autotiles aren't yet implemented
            }
            pictureBoxTileset.Invalidate(); //redraw selection
        }

        public void OnSelectedMapChange(MapInfo lastMap)
        {
            if (SelectedMap != null)
            {
                if(!tilemapMap.Visible) tilemapMap.Visible = true;
                tilemapMap.SetMap(SelectedMap.Map);
                toolStripStatusMapInfo.Text = SelectedMap.Id.ToString("D3") + ": " + SelectedMap.Name + "(" + SelectedMap.Map.rbMap.width.ToString() + " x " + SelectedMap.Map.rbMap.height.ToString() + ")";
            } else
            {
                tilemapMap.Visible = false;
                toolStripStatusMapInfo.Text = "";
            }
            MapEventSelectLocationX = -1;
            MapEventSelectLocationY = -1;
            MapHoverLocationX = -1;
            MapHoverLocationY = -1;
            tilemapMap.Redraw();
            toolStripStatusEventInfo.Text = "";
        }

        private void TreeViewMaps_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MapInfo mapInfo = Editor.Project.Maps.GetMapByTreeNode(e.Node);
            if (mapInfo != null)
            {
                SelectedMap = mapInfo;
                pictureBoxTileset.Image = tilemapMap.TilesetBitmap;
                pictureBoxTileset.Visible = true;
            }
            else
            {
                pictureBoxTileset.Visible = false;
                SelectedMap = null;
            }
        }

        private void TreeViewMaps_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            MapInfo map = Editor.Project.Maps.GetMapByTreeNode(e.Node);
            if (map != null) map.Expanded = false;
        }

        private void TreeViewMaps_AfterExpand(object sender, TreeViewEventArgs e)
        {
            MapInfo map = Editor.Project.Maps.GetMapByTreeNode(e.Node);
            if (map != null) map.Expanded = true;
        }

        private void OpenXP_FormClosed(object sender, FormClosingEventArgs e)
        {
            bool shouldExit = Editor.Exit();
            if (!shouldExit) e.Cancel = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Editor.OpenProject();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.OpenProject();
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.NewProject();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Editor.NewProject();
        }

        private void fileMenuCloseProjectItem_Click(object sender, EventArgs e)
        {
            Editor.CloseProject();
        }

        private void fileMenuExitItem_Click(object sender, EventArgs e)
        {
            Editor.Exit();
        }

        private void fileMenuSaveProjectItem_Click(object sender, EventArgs e)
        {
            Editor.SaveProject();
        }

        private void toolbarSaveProjectItem_Click(object sender, EventArgs e)
        {
            Editor.SaveProject();
        }

        public void updateSelectedDrawTool(DrawToolType tool)
        {
            switch (tool)
            {
                case DrawToolType.PENCIL:
                    drawMenuPencilItem.Checked = true;
                    drawMenuRectangleItem.Checked = false;
                    drawMenuEllipseItem.Checked = false;
                    drawMenuFloodFillItem.Checked = false;
                    drawMenuSelectItem.Checked = false;
                    toolbarPencilItem.Checked = true;
                    toolbarRectangleItem.Checked = false;
                    toolbarEllipseItem.Checked = false;
                    toolbarFloodFillItem.Checked = false;
                    toolbarSelectItem.Checked = false;
                    break;
                case DrawToolType.RECTANGLE:
                    drawMenuPencilItem.Checked = false;
                    drawMenuRectangleItem.Checked = true;
                    drawMenuEllipseItem.Checked = false;
                    drawMenuFloodFillItem.Checked = false;
                    drawMenuSelectItem.Checked = false;
                    toolbarPencilItem.Checked = false;
                    toolbarRectangleItem.Checked = true;
                    toolbarEllipseItem.Checked = false;
                    toolbarFloodFillItem.Checked = false;
                    toolbarSelectItem.Checked = false;
                    break;
                case DrawToolType.ELLIPSE:
                    drawMenuPencilItem.Checked = false;
                    drawMenuRectangleItem.Checked = false;
                    drawMenuEllipseItem.Checked = true;
                    drawMenuFloodFillItem.Checked = false;
                    drawMenuSelectItem.Checked = false;
                    toolbarPencilItem.Checked = false;
                    toolbarRectangleItem.Checked = false;
                    toolbarEllipseItem.Checked = true;
                    toolbarFloodFillItem.Checked = false;
                    toolbarSelectItem.Checked = false;
                    break;
                case DrawToolType.FLOODFILL:
                    drawMenuPencilItem.Checked = false;
                    drawMenuRectangleItem.Checked = false;
                    drawMenuEllipseItem.Checked = false;
                    drawMenuFloodFillItem.Checked = true;
                    drawMenuSelectItem.Checked = false;
                    toolbarPencilItem.Checked = false;
                    toolbarRectangleItem.Checked = false;
                    toolbarEllipseItem.Checked = false;
                    toolbarFloodFillItem.Checked = true;
                    toolbarSelectItem.Checked = false;
                    break;
                case DrawToolType.SELECT:
                    drawMenuPencilItem.Checked = false;
                    drawMenuRectangleItem.Checked = false;
                    drawMenuEllipseItem.Checked = false;
                    drawMenuFloodFillItem.Checked = false;
                    drawMenuSelectItem.Checked = true;
                    toolbarPencilItem.Checked = false;
                    toolbarRectangleItem.Checked = false;
                    toolbarEllipseItem.Checked = false;
                    toolbarFloodFillItem.Checked = false;
                    toolbarSelectItem.Checked = true;
                    break;
            }
        }

        internal void RepaintMap()
        {
            tilemapMap.Redraw();
        }

        public void updateSelectedLayer(LayerType layer)
        {
            switch (layer)
            {
                case LayerType.EVENTS:
                    modeMenuEventsItem.Checked = true;
                    modeMenuLayer1Item.Checked = false;
                    modeMenuLayer2Item.Checked = false;
                    modeMenuLayer3Item.Checked = false;
                    toolbarEventsItem.Checked = true;
                    toolbarLayer1Item.Checked = false;
                    toolbarLayer2Item.Checked = false;
                    toolbarLayer3Item.Checked = false;
                    break;
                case LayerType.LAYER1:
                    modeMenuEventsItem.Checked = false;
                    modeMenuLayer1Item.Checked = true;
                    modeMenuLayer2Item.Checked = false;
                    modeMenuLayer3Item.Checked = false;
                    toolbarEventsItem.Checked = false;
                    toolbarLayer1Item.Checked = true;
                    toolbarLayer2Item.Checked = false;
                    toolbarLayer3Item.Checked = false;
                    break;
                case LayerType.LAYER2:
                    modeMenuEventsItem.Checked = false;
                    modeMenuLayer1Item.Checked = false;
                    modeMenuLayer2Item.Checked = true;
                    modeMenuLayer3Item.Checked = false;
                    toolbarEventsItem.Checked = false;
                    toolbarLayer1Item.Checked = false;
                    toolbarLayer2Item.Checked = true;
                    toolbarLayer3Item.Checked = false;
                    break;
                case LayerType.LAYER3:
                    modeMenuEventsItem.Checked = false;
                    modeMenuLayer1Item.Checked = false;
                    modeMenuLayer2Item.Checked = false;
                    modeMenuLayer3Item.Checked = true;
                    toolbarEventsItem.Checked = false;
                    toolbarLayer1Item.Checked = false;
                    toolbarLayer2Item.Checked = false;
                    toolbarLayer3Item.Checked = true;
                    break;
            }
        }

        public void enableDrawTools()
        {
            toolbarPencilItem.Enabled = true;
            toolbarRectangleItem.Enabled = true;
            toolbarEllipseItem.Enabled = true;
            toolbarFloodFillItem.Enabled = true;
            toolbarSelectItem.Enabled = true;
            drawMenuPencilItem.Enabled = true;
            drawMenuRectangleItem.Enabled = true;
            drawMenuEllipseItem.Enabled = true;
            drawMenuFloodFillItem.Enabled = true;
            drawMenuSelectItem.Enabled = true;
        }

        public void disableDrawTools()
        {
            toolbarPencilItem.Enabled = false;
            toolbarRectangleItem.Enabled = false;
            toolbarEllipseItem.Enabled = false;
            toolbarFloodFillItem.Enabled = false;
            toolbarSelectItem.Enabled = false;
            drawMenuPencilItem.Enabled = false;
            drawMenuRectangleItem.Enabled = false;
            drawMenuEllipseItem.Enabled = false;
            drawMenuFloodFillItem.Enabled = false;
            drawMenuSelectItem.Enabled = false;
        }

        public void enableControls()
        {
            treeViewMaps.Nodes.Add(Editor.Project.Maps.TreeNode);
            tableLayoutPanel.Visible = true;
            toolbarSaveProjectItem.Enabled = true;
            toolbarLayer1Item.Enabled = true;
            toolbarLayer2Item.Enabled = true;
            toolbarLayer3Item.Enabled = true;
            toolbarEventsItem.Enabled = true;
            toolbarZoom1Item.Enabled = true;
            toolbarZoom2Item.Enabled = true;
            toolbarZoom4Item.Enabled = true;
            toolbarDatabaseItem.Enabled = true;
            toolbarMaterialsItem.Enabled = true;
            toolbarScriptEditorItem.Enabled = true;
            toolbarSoundTestItem.Enabled = true;
            toolbarPlaytestItem.Enabled = true;
            fileMenuCloseProjectItem.Enabled = true;
            fileMenuSaveProjectItem.Enabled = true;
            viewMenuAllLayersItem.Enabled = true;
            viewMenuCurrentLayerItem.Enabled = true;
            viewMenuDimLayersItem.Enabled = true;
            modeMenuEventsItem.Enabled = true;
            modeMenuLayer1Item.Enabled = true;
            modeMenuLayer2Item.Enabled = true;
            modeMenuLayer3Item.Enabled = true;
            scaleMenuZoom1Item.Enabled = true;
            scaleMenuZoom2Item.Enabled = true;
            scaleMenuZoom4Item.Enabled = true;
            toolsMenuDatabaseItem.Enabled = true;
            toolsMenuMaterialsItem.Enabled = true;
            toolsMenuOptionsItem.Enabled = true;
            toolsMenuScriptEditorItem.Enabled = true;
            toolsMenuSoundTestItem.Enabled = true;
            gameMenuChangeTitleItem.Enabled = true;
            gameMenuOpenFolderItem.Enabled = true;
            gameMenuPlaytestItem.Enabled = true;
            gameMenuRTPItem.Enabled = true;
            treeViewMaps.Select();
            treeViewMaps.Focus();
        }

        public void disableControls()
        {
            treeViewMaps.Nodes.Clear();
            tableLayoutPanel.Visible = false;
            toolbarSaveProjectItem.Enabled = false;
            toolbarLayer1Item.Enabled = false;
            toolbarLayer2Item.Enabled = false;
            toolbarLayer3Item.Enabled = false;
            toolbarEventsItem.Enabled = false;
            toolbarZoom1Item.Enabled = false;
            toolbarZoom2Item.Enabled = false;
            toolbarZoom4Item.Enabled = false;
            toolbarDatabaseItem.Enabled = false;
            toolbarMaterialsItem.Enabled = false;
            toolbarScriptEditorItem.Enabled = false;
            toolbarSoundTestItem.Enabled = false;
            toolbarPlaytestItem.Enabled = false;
            fileMenuCloseProjectItem.Enabled = false;
            fileMenuSaveProjectItem.Enabled = false;
            viewMenuAllLayersItem.Enabled = false;
            viewMenuCurrentLayerItem.Enabled = false;
            viewMenuDimLayersItem.Enabled = false;
            modeMenuEventsItem.Enabled = false;
            modeMenuLayer1Item.Enabled = false;
            modeMenuLayer2Item.Enabled = false;
            modeMenuLayer3Item.Enabled = false;
            scaleMenuZoom1Item.Enabled = false;
            scaleMenuZoom2Item.Enabled = false;
            scaleMenuZoom4Item.Enabled = false;
            toolsMenuDatabaseItem.Enabled = false;
            toolsMenuMaterialsItem.Enabled = false;
            toolsMenuOptionsItem.Enabled = false;
            toolsMenuScriptEditorItem.Enabled = false;
            toolsMenuSoundTestItem.Enabled = false;
            gameMenuChangeTitleItem.Enabled = false;
            gameMenuOpenFolderItem.Enabled = false;
            gameMenuPlaytestItem.Enabled = false;
            gameMenuRTPItem.Enabled = false;
        }

        public void SetDimOtherLayersChecked(bool state)
        {
            viewMenuDimLayersItem.Checked = state;
        }

        public void SelectMap(int id)
        {
            var matches = from TreeNode node in Editor.Project.Maps.TreeNode.Nodes
                          where (int)node.Tag == id
                          select node; //linq match by tag
            if(matches.Count() > 0)
            {
                treeViewMaps.SelectedNode = matches.First<TreeNode>();
            } else
            {
                //select root
                treeViewMaps.SelectedNode = treeViewMaps.Nodes[0];
            }
        }

        public int GetMapId()
        {
            TreeNode tn = treeViewMaps.SelectedNode;
            if(tn != null)
            {
                MapInfo m = Editor.Project.Maps.GetMapByTreeNode(tn);
                if (m != null) return m.Id;
            }
            return 0;
        }

        private void toolbarLayer1Item_Click(object sender, EventArgs e)
        {
            toolStripStatusEventInfo.Text = "";
            Editor.ChangeLayer(LayerType.LAYER1);
        }

        private void toolbarLayer2Item_Click(object sender, EventArgs e)
        {
            toolStripStatusEventInfo.Text = "";
            Editor.ChangeLayer(LayerType.LAYER2);
        }

        private void toolbarLayer3Item_Click(object sender, EventArgs e)
        {
            toolStripStatusEventInfo.Text = "";
            Editor.ChangeLayer(LayerType.LAYER3);
        }

        private void toolbarEventsItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.EVENTS);
        }

        private void modeMenuLayer1Item_Click(object sender, EventArgs e)
        {
            toolStripStatusEventInfo.Text = "";
            Editor.ChangeLayer(LayerType.LAYER1);
        }

        private void modeMenuLayer2Item_Click(object sender, EventArgs e)
        {
            toolStripStatusEventInfo.Text = "";
            Editor.ChangeLayer(LayerType.LAYER2);
        }

        private void modeMenuLayer3Item_Click(object sender, EventArgs e)
        {
            toolStripStatusEventInfo.Text = "";
            Editor.ChangeLayer(LayerType.LAYER3);
        }

        private void modeMenuEventsItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.EVENTS);
        }

        private void toolbarPencilItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.PENCIL);
        }

        private void toolbarRectangleItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.RECTANGLE);
        }

        private void toolbarEllipseItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.ELLIPSE);
        }

        private void toolbarFloodFillItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.FLOODFILL);
        }

        private void toolbarSelectItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.SELECT);
        }

        private void drawMenuPencilItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.PENCIL);
        }

        private void drawMenuRectangleItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.RECTANGLE);
        }

        private void drawMenuEllipseItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.ELLIPSE);
        }

        private void drawMenuFloodFillItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.FLOODFILL);
        }

        private void drawMenuSelectItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeDrawTool(DrawToolType.SELECT);
        }

        private void gameMenuChangeTitleItem_Click(object sender, EventArgs e)
        {
            using (DialogChangeGameTitle dcgt = new DialogChangeGameTitle())
            {
                dcgt.ShowDialog();
            }
        }

        private void toolsMenuScriptEditorItem_Click(object sender, EventArgs e)
        {
            using (DialogScriptEditor dse = new DialogScriptEditor())
            {
                dse.ShowDialog();
            }
        }

        private void toolbarScriptEditorItem_Click(object sender, EventArgs e)
        {
            using (DialogScriptEditor dse = new DialogScriptEditor())
            {
                dse.ShowDialog();
            }
        }

        private void viewMenuDimLayersItem_Click(object sender, EventArgs e)
        {
            Editor.ToggleDimOtherLayers();
            RepaintMap();
        }

        private void viewMenuCurrentLayerItem_Click(object sender, EventArgs e)
        {
            Editor.SetViewAllLayers(false);
            viewMenuCurrentLayerItem.Checked = true;
            viewMenuAllLayersItem.Checked = false;
            RepaintMap();
        }

        private void viewMenuAllLayersItem_Click(object sender, EventArgs e)
        {
            Editor.SetViewAllLayers(true);
            viewMenuCurrentLayerItem.Checked = false;
            viewMenuAllLayersItem.Checked = true;
            RepaintMap();
        }

        private void gameMenuOpenFolderItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = Editor.Project.Directory,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void toolbarPlaytestItem_Click(object sender, EventArgs e)
        {
            Editor.Playtest();
        }

        private void gameMenuPlaytestItem_Click(object sender, EventArgs e)
        {
            Editor.Playtest();
        }

        private void toolbarZoom1Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeZoom(ZoomType.FULL);
            toolbarZoom1Item.Checked = true;
            toolbarZoom2Item.Checked = false;
            toolbarZoom4Item.Checked = false;
            scaleMenuZoom1Item.Checked = true;
            scaleMenuZoom2Item.Checked = false;
            scaleMenuZoom4Item.Checked = false;
        }

        private void toolbarZoom2Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeZoom(ZoomType.HALF);
            toolbarZoom1Item.Checked = false;
            toolbarZoom2Item.Checked = true;
            toolbarZoom4Item.Checked = false;
            scaleMenuZoom1Item.Checked = false;
            scaleMenuZoom2Item.Checked = true;
            scaleMenuZoom4Item.Checked = false;
        }

        private void toolbarZoom4Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeZoom(ZoomType.QUARTER);
            toolbarZoom1Item.Checked = false;
            toolbarZoom2Item.Checked = false;
            toolbarZoom4Item.Checked = true;
            scaleMenuZoom1Item.Checked = false;
            scaleMenuZoom2Item.Checked = false;
            scaleMenuZoom4Item.Checked = true;
        }

        private void scaleMenuZoom1Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeZoom(ZoomType.FULL);
            toolbarZoom1Item.Checked = true;
            toolbarZoom2Item.Checked = false;
            toolbarZoom4Item.Checked = false;
            scaleMenuZoom1Item.Checked = true;
            scaleMenuZoom2Item.Checked = false;
            scaleMenuZoom4Item.Checked = false;
        }

        private void scaleMenuZoom2Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeZoom(ZoomType.HALF);
            toolbarZoom1Item.Checked = false;
            toolbarZoom2Item.Checked = true;
            toolbarZoom4Item.Checked = false;
            scaleMenuZoom1Item.Checked = false;
            scaleMenuZoom2Item.Checked = true;
            scaleMenuZoom4Item.Checked = false;
        }

        private void scaleMenuZoom4Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeZoom(ZoomType.QUARTER);
            toolbarZoom1Item.Checked = false;
            toolbarZoom2Item.Checked = false;
            toolbarZoom4Item.Checked = true;
            scaleMenuZoom1Item.Checked = false;
            scaleMenuZoom2Item.Checked = false;
            scaleMenuZoom4Item.Checked = true;
        }

        private void toolbarDatabaseItem_Click(object sender, EventArgs e)
        {
            using (DialogDatabase ddb = new DialogDatabase())
            {
                ddb.ShowDialog();
            }
        }

        private void toolsMenuDatabaseItem_Click(object sender, EventArgs e)
        {
            using (DialogDatabase ddb = new DialogDatabase())
            {
                ddb.ShowDialog();
            }
        }

        private void playersStartingPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapHandler.startMapId = Editor.GetSelectedMapId();
            MapHandler.startMapX = MapEventSelectLocationX;
            MapHandler.startMapY = MapEventSelectLocationY;
            RepaintMap();
        }

        private void onStatusBarObjectHoverStart(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null && control.Tag != null && control.Tag.ToString().Length > 0)
                this.toolStripStatusLabel.Text = control.Tag.ToString();

            ToolStripItem tsi = sender as ToolStripItem;
            if (tsi != null && tsi.Tag != null && tsi.Tag.ToString().Length > 0)
                this.toolStripStatusLabel.Text = tsi.Tag.ToString();
        }

        private void onStatusBarObjectHoverEnd(object sender, EventArgs e)
        {
            if (this.toolStripStatusLabel.Text.ToString().Length > 0)
                toolStripStatusLabel.Text = "";
        }

        private void helpMenuContentsItem_Click(object sender, EventArgs e)
        {
            string helpfile = System.IO.Path.Combine(Program.GetEditorDirectory(),"Documentation", "index.html");
            var uri = new System.Uri(helpfile);
            var converted = uri.AbsoluteUri;
            System.Diagnostics.Process.Start(DefaultWebBrowser, converted);
        }

        public static string DefaultWebBrowser
        {
            get
            {
                string path = @"\http\shell\open\command";
                using (RegistryKey reg = Registry.ClassesRoot.OpenSubKey(path))
                {
                    if (reg != null)
                    {
                        string webBrowserPath = reg.GetValue(String.Empty) as string;

                        if (!String.IsNullOrEmpty(webBrowserPath))
                        {
                            if (webBrowserPath.First() == '"')
                            {
                                return webBrowserPath.Split('"')[1];
                            }

                            return webBrowserPath.Split(' ')[0];
                        }
                    }
                    return null;
                }
            }
        }
    }
}
