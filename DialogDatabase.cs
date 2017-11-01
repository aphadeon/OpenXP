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
        private static string[] dataTabs = { "actors", "classes", "skills", "items", "weapons", "armors", "enemies", "troops", "states", "animations", "tilesets", "commonevents" };

        private Dictionary<string, dynamic> TempData;
        private Dictionary<string, ListBox> DataListBoxes;
        private Dictionary<string, TextBox> DataNameTextBoxes;
        private Dictionary<string, Button> DataChangeMaxButtons;
        private Dictionary<string, int> DataSelections;

        public DialogDatabase()
        {
            InitializeComponent();

            //create temp (cancelable) database
            TempData = new Dictionary<string, dynamic>();
            TempData["actors"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Actors);
            TempData["animations"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Animations);
            TempData["armors"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Armors);
            TempData["classes"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Classes);
            TempData["commonevents"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.CommonEvents);
            TempData["enemies"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Enemies);
            TempData["items"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Items);
            TempData["skills"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Skills);
            TempData["states"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.States);
            TempData["system"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.System);
            TempData["tilesets"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Tilesets);
            TempData["troops"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Troops);
            TempData["weapons"] = Editor.Project.Ruby.RubyDeepCopy(Editor.Project.Database.Weapons);

            //setup common data management for non-system tabs
            DataListBoxes = new Dictionary<string, ListBox>();
            DataNameTextBoxes = new Dictionary<string, TextBox>();
            DataChangeMaxButtons = new Dictionary<string, Button>();
            DataSelections = new Dictionary<string, int>();
            foreach (string s in dataTabs)
            {
                //control reference caching
                Control[] matchingControls;

                matchingControls = this.Controls.Find("listBox" + Capitalize(s), true);
                if (matchingControls.Length <= 0) Console.WriteLine("Failed to locate: listBox" + Capitalize(s));
                DataListBoxes.Add(s, (ListBox) matchingControls[0]);

                matchingControls = this.Controls.Find("textBoxName" + Capitalize(s), true);
                if (matchingControls.Length <= 0) Console.WriteLine("Failed to locate: textBoxName" + Capitalize(s));
                DataNameTextBoxes.Add(s, (TextBox)matchingControls[0]);

                matchingControls = this.Controls.Find("buttonChangeMax" + Capitalize(s), true);
                if (matchingControls.Length <= 0) Console.WriteLine("Failed to locate: buttonChangeMax" + Capitalize(s));
                DataChangeMaxButtons.Add(s, (Button)matchingControls[0]);


                //populate data listbox
                PopulateDataList(s);

                //setup data selection
                DataSelections[s] = 0;
                DataListBoxes[s].SelectedItem = DataListBoxes[s].Items[0];

                //read tab datas
                Read(s, 1);

                //event hooks
                DataListBoxes[s].SelectedIndexChanged += ListBox_SelectedIndexChanged;
                DataNameTextBoxes[s].TextChanged += TextBoxName_TextChanged;
                DataChangeMaxButtons[s].Click += ButtonChangeMax_Click;
            }
        }
        
        private string Capitalize(string lower)
        {
            return char.ToUpper(lower[0]) + lower.Substring(1);
        }

        private dynamic GetNewDataInstance(string tab)
        {
            switch (tab)
            {
                case "actors":
                    return Ruby.CreateRubyInstance("RPG::Actor");
                case "classes":
                    return Ruby.CreateRubyInstance("RPG::Class");
                case "skills":
                    return Ruby.CreateRubyInstance("RPG::Skill");
                case "items":
                    return Ruby.CreateRubyInstance("RPG::Item");
                case "weapons":
                    return Ruby.CreateRubyInstance("RPG::Weapon");
                case "armors":
                    return Ruby.CreateRubyInstance("RPG::Armor");
                case "enemies":
                    return Ruby.CreateRubyInstance("RPG::Enemy");
                case "troops":
                    return Ruby.CreateRubyInstance("RPG::Troop");
                case "states":
                    return Ruby.CreateRubyInstance("RPG::State");
                case "animations":
                    return Ruby.CreateRubyInstance("RPG::Animation");
                case "tilesets":
                    return Ruby.CreateRubyInstance("RPG::Tileset");
                case "commonevents":
                    return Ruby.CreateRubyInstance("RPG::CommonEvent");
            }
            Console.WriteLine("New instance for unknown tab!");
            return null;
        }

        private void ButtonChangeMax_Click(object sender, EventArgs e)
        {
            Button bSender = (Button)sender;
            string tab = bSender.Name.Substring(15).ToLower();
            ListBox listBox = DataListBoxes[tab];

            using (DialogChangeMaximum dcm = new DialogChangeMaximum(listBox.Items.Count))
            {
                dcm.ShowDialog();
                if(dcm.DialogResult == DialogResult.OK)
                {
                    int oldCount = listBox.Items.Count;
                    int newCount = dcm.NewValue;
                    if(newCount < oldCount)
                    {
                        DialogResult dr = System.Windows.Forms.MessageBox.Show(this, "You are reducing the number of items.\nData loss will occur. Continue?", "Confirmation", MessageBoxButtons.OKCancel);
                        if(dr == DialogResult.OK)
                        {
                            while(oldCount > newCount)
                            {
                                TempData[tab].RemoveAt(oldCount);
                                oldCount -= 1;
                            }
                            PopulateDataList(tab);
                        }
                    } else
                    {
                        while(oldCount < newCount)
                        {
                            dynamic actor = GetNewDataInstance(tab);
                            actor.id = oldCount + 1;
                            TempData[tab].Add(actor);
                            oldCount++;
                        }
                        PopulateDataList(tab);
                    }
                }
            }
        }

        private void TextBoxName_TextChanged(object sender, EventArgs e)
        {
            TextBox bSender = (TextBox)sender;
            string tab = bSender.Name.Substring(11).ToLower();
            DataListBoxes[tab].Items[DataListBoxes[tab].SelectedIndex] = (DataListBoxes[tab].SelectedIndex + 1).ToString("D4") + ": " + bSender.Text;
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox bSender = (ListBox)sender;
            string tab = bSender.Name.Substring(7).ToLower();
            int id = DataListBoxes[tab].SelectedIndex + 1;
            if (id < 1) return;

            Write(tab, DataSelections[tab]);
            DataSelections[tab] = id;
            Read(tab, id);
        }

        private void PopulateDataList(string db)
        {
            dynamic rxdata = TempData[db];
            DataListBoxes[db].Items.Clear();
            IronRuby.Builtins.RubyArray ra = (IronRuby.Builtins.RubyArray)rxdata;
            for (int i = 1; i < ra.Count; i++) //its a one-based index
            {
                if (rxdata[i] != null) //safety
                {
                    int id = rxdata[i].id;
                    string name = rxdata[i].name.ToString();
                    DataListBoxes[db].Items.Add(id.ToString("D4") + ": " + name);
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
            Editor.Project.Database.Actors = TempData["actors"];
            Editor.Project.Database.Animations = TempData["animations"];
            Editor.Project.Database.Armors = TempData["armors"];
            Editor.Project.Database.Classes = TempData["classes"];
            Editor.Project.Database.CommonEvents = TempData["commonevents"];
            Editor.Project.Database.Enemies = TempData["enemies"];
            Editor.Project.Database.Items = TempData["items"];
            Editor.Project.Database.Skills = TempData["skills"];
            Editor.Project.Database.States = TempData["states"];
            Editor.Project.Database.System = TempData["system"];
            Editor.Project.Database.Tilesets = TempData["tilesets"];
            Editor.Project.Database.Troops = TempData["troops"];
            Editor.Project.Database.Weapons = TempData["weapons"];

            Editor.Touch();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Read(string tab, int id)
        {
            if (TempData[tab][id] == null) return; //queries while repopulating try to say 0 is selected
            //common: name
            DataNameTextBoxes[tab].Text = TempData[tab][id].name.ToString();
            //switch on tab
            switch (tab)
            {
                case "actors": ReadActor(id); break;
                case "classes": ReadClass (id); break;
                case "skills": ReadSkill(id); break;
                case "items": ReadItem(id); break;
                case "weapons": ReadWeapon(id); break;
                case "enemies": ReadEnemy(id); break;
                case "troops": ReadTroop(id); break;
                case "states": ReadState(id); break;
                case "animations": ReadAnimation(id); break;
                case "tilesets": ReadTileset(id); break;
                case "commonevents": ReadCommonEvent(id); break;
            }
        }

        private void Write(string tab, int id)
        {
            if (TempData[tab][id] == null) return; //queries while repopulating try to say 0 is selected
            //common: name
            TempData[tab][id].name = new IronRuby.Builtins.MutableString().Append(DataNameTextBoxes[tab].Text);
            //switch on tab
            switch (tab)
            {
                case "actors": WriteActor(id); break;
                case "classes": WriteClass(id); break;
                case "skills": WriteSkill(id); break;
                case "items": WriteItem(id); break;
                case "weapons": WriteWeapon(id); break;
                case "enemies": WriteEnemy(id); break;
                case "troops": WriteTroop(id); break;
                case "states": WriteState(id); break;
                case "animations": WriteAnimation(id); break;
                case "tilesets": WriteTileset(id); break;
                case "commonevents": WriteCommonEvent(id); break;
            }
        }

        //ACTORS ==========================================================================
        private void ReadActor(int id)
        {

        }

        private void WriteActor(int id)
        {

        }
        //CLASSES ==========================================================================
        private void ReadClass(int id)
        {

        }

        private void WriteClass(int id)
        {

        }
        //SKILLS ==========================================================================
        private void ReadSkill(int id)
        {

        }

        private void WriteSkill(int id)
        {

        }
        //ITEMS ==========================================================================
        private void ReadItem(int id)
        {

        }

        private void WriteItem(int id)
        {

        }
        //WEAPONS ==========================================================================
        private void ReadWeapon(int id)
        {

        }

        private void WriteWeapon(int id)
        {

        }
        //ENEMIES ==========================================================================
        private void ReadEnemy(int id)
        {

        }

        private void WriteEnemy(int id)
        {

        }
        //TROOPS ==========================================================================
        private void ReadTroop(int id)
        {

        }

        private void WriteTroop(int id)
        {

        }
        //STATES ==========================================================================
        private void ReadState(int id)
        {

        }

        private void WriteState(int id)
        {

        }
        //ANIMATIONS ==========================================================================
        private void ReadAnimation(int id)
        {

        }

        private void WriteAnimation(int id)
        {

        }
        //ACTORS ==========================================================================
        private void ReadTileset(int id)
        {

        }

        private void WriteTileset(int id)
        {

        }
        //COMMON EVENTS ==========================================================================
        private void ReadCommonEvent(int id)
        {

        }

        private void WriteCommonEvent(int id)
        {

        }

        //END TABS ========================================================================
    }
}
