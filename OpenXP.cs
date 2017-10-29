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

        public int TilesetSelectionX = -1;
        public int TilesetSelectionY = -1;
        public int TilesetSelectionId = 0;
        public int MapHoverLocationX = -1;
        public int MapHoverLocationY = -1;

        public bool MarkedForExit = true;

        public OpenXP()
        {
            InitializeComponent();
            Editor.Form = this;

            //changing to a different value triggers initial updates
            Editor.ActiveLayer = LayerType.LAYER1;
            Editor.ChangeLayer(LayerType.EVENTS);
            Editor.ActiveDrawTool = DrawToolType.SELECT;
            Editor.ChangeDrawTool(DrawToolType.PENCIL);

            treeViewMaps.TreeViewNodeSorter = new NodeSorter();
            treeViewMaps.AfterExpand += TreeViewMaps_AfterExpand;
            treeViewMaps.AfterCollapse += TreeViewMaps_AfterCollapse;
            treeViewMaps.AfterSelect += TreeViewMaps_AfterSelect;

            pictureBoxMap.MouseLeave += PictureBoxMap_MouseLeave;
            pictureBoxMap.MouseMove += PictureBoxMap_MouseMove;

            pictureBoxMap.Paint += PanelMap_Paint;
            pictureBoxMap.MouseClick += PictureBoxMap_MouseClick;
            panelTilemapContainer.HorizontalScroll.Enabled = true;
            panelTilemapContainer.VerticalScroll.Enabled = true;

            pictureBoxTileset.MouseClick += PictureBoxTileset_MouseClick;
            pictureBoxTileset.Paint += PictureBoxTileset_Paint;

            FormClosed += OpenXP_FormClosed;

            //load configuration here
            Load += OpenXP_Load;
            Editor.OnStartup();
        }

        private void OpenXP_Load(object sender, EventArgs e)
        {
            if (MarkedForExit) Close();
        }

        private void PictureBoxMap_MouseMove(object sender, MouseEventArgs e)
        {
            int hoverX = e.X / 32;
            int hoverY = e.Y / 32;
            if(hoverX > 0 && hoverY > 0)
            {
                if (MapHoverLocationX != hoverX || MapHoverLocationY != hoverY) RepaintMap();
                MapHoverLocationX = hoverX;
                MapHoverLocationY = hoverY;
            }
        }

        private void PictureBoxMap_MouseLeave(object sender, EventArgs e)
        {
            MapHoverLocationX = -1;
            MapHoverLocationY = -1;
        }

        private void PictureBoxMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (Editor.ActiveLayer == LayerType.EVENTS)
            {

            } else {
                int column = e.X / 32;
                int row = e.Y / 32;
                if (SelectedMap != null)
                {
                    int layer = 0;
                    switch (Editor.ActiveLayer)
                    {
                        case LayerType.LAYER2: layer = 1; break;
                        case LayerType.LAYER3: layer = 2; break;
                    }
                    SelectedMap.Map.SetTile(column, row, layer, TilesetSelectionId);
                    pictureBoxMap.Invalidate();
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
            if(SelectedMap != null) SelectedMap.Map.FirstDraw();
            pictureBoxMap.Invalidate();
        }

        private void PanelMap_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            if (SelectedMap != null) SelectedMap.Map.PaintEditor(this.pictureBoxMap, e);
        }

        private void TreeViewMaps_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MapInfo mapInfo = Editor.Project.Maps.GetMapByTreeNode(e.Node);
            if (mapInfo != null)
            {
                pictureBoxTileset.Image = mapInfo.Map.TilesetBitmap;
                pictureBoxTileset.Visible = true;
                SelectedMap = mapInfo;
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

        private void OpenXP_FormClosed(object sender, FormClosedEventArgs e)
        {
            Editor.Exit();
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
            pictureBoxMap.Invalidate();
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
            treeViewMaps.Nodes.Clear();
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
            MapInfo lastMap = SelectedMap;
            if(id == 0)
            {
                //select the root
                treeViewMaps.SelectedNode = treeViewMaps.Nodes[0];
            }
            MapInfo m = Editor.Project.Maps.GetMapById(id);
            if(m != null)
            {
                treeViewMaps.SelectedNode = m.TreeNode;
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
            Editor.ChangeLayer(LayerType.LAYER1);
        }

        private void toolbarLayer2Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.LAYER2);
        }

        private void toolbarLayer3Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.LAYER3);
        }

        private void toolbarEventsItem_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.EVENTS);
        }

        private void modeMenuLayer1Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.LAYER1);
        }

        private void modeMenuLayer2Item_Click(object sender, EventArgs e)
        {
            Editor.ChangeLayer(LayerType.LAYER2);
        }

        private void modeMenuLayer3Item_Click(object sender, EventArgs e)
        {
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
    }
}
