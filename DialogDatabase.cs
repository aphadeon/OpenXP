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
    public partial class DialogDatabase : Form
    {
        private dynamic TempActors;
        private dynamic TempAnimations;
        private dynamic TempArmors;
        private dynamic TempClasses;
        private dynamic TempCommonEvents;
        private dynamic TempEnemies;
        private dynamic TempItems;
        private dynamic TempSkills;
        private dynamic TempStates;
        private dynamic TempSystem;
        private dynamic TempTilesets;
        private dynamic TempTroops;
        private dynamic TempWeapons;

        public DialogDatabase()
        {
            InitializeComponent();

            //create temp (cancelable) database
            TempActors = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Actors);
            TempAnimations = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Animations);
            TempArmors = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Armors);
            TempClasses = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Classes);
            TempCommonEvents = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.CommonEvents);
            TempEnemies = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Enemies);
            TempItems = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Items);
            TempSkills = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Skills);
            TempStates = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.States);
            TempSystem = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.System);
            TempTilesets = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Tilesets);
            TempTroops = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Troops);
            TempWeapons = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Weapons);

            //populate listboxes
            PopulateDataList(listBoxActors.Items, TempActors);
            PopulateDataList(listBoxClasses.Items, TempClasses);
            PopulateDataList(listBoxSkills.Items, TempSkills);
            PopulateDataList(listBoxItems.Items, TempItems);
            PopulateDataList(listBoxWeapons.Items, TempWeapons);
            PopulateDataList(listBoxArmors.Items, TempArmors);
            PopulateDataList(listBoxEnemies.Items, TempEnemies);
            PopulateDataList(listBoxTroops.Items, TempTroops);
            PopulateDataList(listBoxStates.Items, TempStates);
            PopulateDataList(listBoxAnimations.Items, TempAnimations);
            PopulateDataList(listBoxTilesets.Items, TempTilesets);
            PopulateDataList(listBoxCommonEvents.Items, TempCommonEvents);
        }

        private void PopulateDataList(ListBox.ObjectCollection list, dynamic rxdata)
        {
            list.Clear();
            IronRuby.Builtins.RubyArray ra = (IronRuby.Builtins.RubyArray)rxdata;
            for (int i = 1; i < ra.Count; i++) //its a one-based index
            {
                if (rxdata[i] != null) //safety
                {
                    int id = rxdata[i].id;
                    string name = rxdata[i].name.ToString();
                    list.Add(id.ToString("D4") + ": " + name);
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Save();
            Close();
        }

        public void Save()
        {
            //save temp database
            Editor.Project.Database.Actors = TempActors;
            Editor.Project.Database.Animations = TempAnimations;
            Editor.Project.Database.Armors = TempArmors;
            Editor.Project.Database.Classes = TempClasses;
            Editor.Project.Database.CommonEvents = TempCommonEvents;
            Editor.Project.Database.Enemies = TempEnemies;
            Editor.Project.Database.Items = TempItems;
            Editor.Project.Database.Skills = TempSkills;
            Editor.Project.Database.States = TempStates;
            Editor.Project.Database.System = TempSystem;
            Editor.Project.Database.Tilesets = TempTilesets;
            Editor.Project.Database.Troops = TempTroops;
            Editor.Project.Database.Weapons = TempWeapons;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Save();
        }
    }
}
