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

        private int lastSelectedActorIndex = 1;
        private int lastSelectedAnimation = 1;
        private int lastSelectedArmor = 1;
        private int lastSelectedClass = 1;
        private int lastSelectedCommonEvent = 1;
        private int lastSelectedEnemy = 1;
        private int lastSelectedItem = 1;
        private int lastSelectedSkill = 1;
        private int lastSelectedState = 1;
        private int lastSelectedTileset = 1;
        private int lastSelectedTroop = 1;
        private int lastSelectedWeapon = 1;

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

            //select initial entries
            listBoxActors.SelectedItem = listBoxActors.Items[0];
            listBoxClasses.SelectedItem = listBoxClasses.Items[0];
            listBoxSkills.SelectedItem = listBoxSkills.Items[0];
            listBoxItems.SelectedItem = listBoxItems.Items[0];
            listBoxWeapons.SelectedItem = listBoxWeapons.Items[0];
            listBoxArmors.SelectedItem = listBoxArmors.Items[0];
            listBoxEnemies.SelectedItem = listBoxEnemies.Items[0];
            listBoxTroops.SelectedItem = listBoxTroops.Items[0];
            listBoxStates.SelectedItem = listBoxStates.Items[0];
            listBoxAnimations.SelectedItem = listBoxAnimations.Items[0];
            listBoxTilesets.SelectedItem = listBoxTilesets.Items[0];
            listBoxCommonEvents.SelectedItem = listBoxCommonEvents.Items[0];

            //initial data reads
            ReadActor(1);

            //Event hooks
            listBoxActors.SelectedIndexChanged += ListBoxActors_SelectedIndexChanged;
            textBoxNameActors.TextChanged += TextBoxNameActors_TextChanged;
            buttonChangeMaxActors.Click += ButtonChangeMaxActors_Click;
        }

        //ACTORS TAB ========================================================================

        private void ButtonChangeMaxActors_Click(object sender, EventArgs e)
        {
            using (DialogChangeMaximum dcm = new DialogChangeMaximum(listBoxActors.Items.Count))
            {
                dcm.ShowDialog();
                if(dcm.DialogResult == DialogResult.OK)
                {
                    int oldCount = listBoxActors.Items.Count;
                    int newCount = dcm.NewValue;
                    if(newCount < oldCount)
                    {
                        DialogResult dr = System.Windows.Forms.MessageBox.Show(this, "You are reducing the number of items.\nData loss will occur. Continue?", "Confirmation", MessageBoxButtons.OKCancel);
                        if(dr == DialogResult.OK)
                        {
                            while(oldCount > newCount)
                            {
                                TempActors.RemoveAt(oldCount);
                                oldCount -= 1;
                            }
                            PopulateDataList(listBoxActors.Items, TempActors);
                        }
                    } else
                    {
                        while(oldCount < newCount)
                        {
                            dynamic actor = Ruby.CreateRubyInstance("RPG::Actor");
                            actor.id = oldCount + 1;
                            TempActors.Add(actor);
                            oldCount++;
                        }
                        PopulateDataList(listBoxActors.Items, TempActors);
                    }
                }
            }
        }

        private void TextBoxNameActors_TextChanged(object sender, EventArgs e)
        {
            listBoxActors.Items[listBoxActors.SelectedIndex] = (listBoxActors.SelectedIndex + 1).ToString("D4") + ": " + textBoxNameActors.Text;
        }

        private void ListBoxActors_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = listBoxActors.SelectedIndex + 1;
            if (id < 1) return;
            WriteActor(lastSelectedActorIndex);
            lastSelectedActorIndex = id;
            ReadActor(id);
        }

        private void ReadActor(int id)
        {
            if (TempActors[id] == null) return;
            textBoxNameActors.Text = TempActors[id].name.ToString();
        }

        private void WriteActor(int id)
        {
            TempActors[id].name = new IronRuby.Builtins.MutableString().Append(textBoxNameActors.Text);
        }

        //END TABS ========================================================================

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
            //todo: write all
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
