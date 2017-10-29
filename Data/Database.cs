using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXP
{
    public class Database
    {
        //these should all be IronRuby.Builtins.RubyArray
        //but we are leaving them dynamic so we can call against the objects directly
        public dynamic Actors;
        public dynamic Animations;
        public dynamic Armors;
        public dynamic Classes;
        public dynamic CommonEvents;
        public dynamic Enemies;
        public dynamic Items;
        public dynamic Skills;
        public dynamic States;
        public dynamic System;
        public dynamic Tilesets;
        public dynamic Troops;
        public dynamic Weapons;
    }
}
