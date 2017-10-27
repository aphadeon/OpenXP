using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class MapInfos
    {
        public List<MapInfo> Maps;
        public MapInfos()
        {
            Maps = new List<MapInfo>();
        }

        public void AddMap(int id, string name, int parent, int order, bool expanded, int scrollx, int scrolly)
        {
            MapInfo info = new MapInfo();
            info.Id = id;
            info.Name = name;
            info.ParentId = parent;
            info.Order = order;
            info.Expanded = expanded;
            info.ScrollX = scrollx;
            info.ScrollY = scrolly;
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
    }
}
