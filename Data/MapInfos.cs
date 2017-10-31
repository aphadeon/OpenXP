using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenXP
{
    public class MapInfos
    {
        public List<MapInfo> Maps;
        public System.Windows.Forms.TreeNode TreeNode;

        private bool finishedLoading = false;

        public MapInfos()
        {
            Maps = new List<MapInfo>();
            TreeNode = new System.Windows.Forms.TreeNode();
            TreeNode.Text = Editor.Project.Ini.Title;
            TreeNode.ImageKey = "globe";
            TreeNode.SelectedImageKey = "globe";
        }

        public void AddMap(int id, string name, int parent, int order, bool expanded, int scrollx, int scrolly, dynamic map)
        {
            MapInfo info = new MapInfo();
            info.Id = id;
            info.Name = name;
            info.ParentId = parent;
            info.Order = order;
            info.Expanded = expanded;
            info.ScrollX = scrollx;
            info.ScrollY = scrolly;
            info.Map = new MapHandler(info, map);
            info.TreeNode = new TreeNode();
            info.TreeNode.Name = "tn" + info.Id.ToString(); //so that we can get the id later
            info.TreeNode.Text = info.Name; //temp, make order variable visible
            info.TreeNode.ImageKey = "map";
            info.TreeNode.SelectedImageKey = "map";
            if (expanded) info.TreeNode.Expand();
            else info.TreeNode.Collapse();
            Maps.Add(info);
            if (finishedLoading) RebuildTreeNode(); //don't rebuild until all maps are initially loaded, or we'll have parenting issues
        }

        public MapInfo GetMapByTreeNode(TreeNode node)
        {
            if (node == null || node == TreeNode) return null;
            int id = int.Parse(node.Name.Substring(2));
            return GetMapById(id);
        }

        public void FinishedLoading()
        {
            //rebuilds the tree view initially
            finishedLoading = true;
            RebuildTreeNode();
        }

        //not super-duper efficient but whatever. it works for now
        public bool MapIdExists(int id)
        {
            foreach(MapInfo info in Maps)
            {
                if (info.Id == id) return true;
            }
            return false;
        }

        public MapInfo GetMapById(int id)
        {
            foreach (MapInfo info in Maps)
            {
                if (info.Id == id) return info;
            }
            Console.WriteLine("Bad map linkage detected!");
            return null;
        }

        private void ReparentLostMapsToZero()
        {
            foreach(MapInfo info in Maps)
            {
                if (!MapIdExists(info.ParentId)) info.ParentId = 0;
            }
        }

        public void RebuildTreeNode()
        {
            //we have to construct this in a tree search fashion.

            //first step, let's save any stragglers with missing parents.
            //really, this should never happen. but we don't want the maps to become inaccessible but
            //still existant if it does.
            ReparentLostMapsToZero();

            //scan for all maps with a parent of zero, then do a scan for each one recursively to create the tree
            foreach(MapInfo info in Maps)
            {
                if(info.ParentId == 0)
                {
                    //link it to the root node
                    TreeNode.Nodes.Add(info.TreeNode);
                } else
                {
                    //link it to its parent's node
                    GetMapById(info.ParentId).TreeNode.Nodes.Add(info.TreeNode);
                }
            }

            //always expand the root node on load/change
            TreeNode.Expand();
        }
    }
    public class MapInfo
    {
        public int Id;
        public string Name;
        public int ParentId;
        public int Order;
        public bool Expanded;
        public int ScrollX;
        public int ScrollY;

        //special case for serialization
        public MapHandler Map;

        //not serialized, used for map browser
        public TreeNode TreeNode;
    }

    public class NodeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = (TreeNode)x;
            TreeNode ty = (TreeNode)y;

            if (tx == null || ty == null) return 0;

            MapInfo info1 = Editor.Project.Maps.GetMapByTreeNode(tx);
            if (info1 == null) return -1;
            MapInfo info2 = Editor.Project.Maps.GetMapByTreeNode(ty);
            if (info2 == null) return 1;


            if (info1.Order < info2.Order)
            {
                return -1;
            }

            if (info1.Order > info2.Order)
            {
                return 1;
            }

            return 0;
        }
    }
}
