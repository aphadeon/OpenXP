using ScintillaNET;
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
    public partial class DialogScriptEditor : Form
    {
        public ScriptHive Scripts;
        private List<string> ScriptNames;
        private int lastSelectedIndex = 0;

        public DialogScriptEditor()
        {
            InitializeComponent();
            //setup scintilla
            scintilla.Lexer = ScintillaNET.Lexer.Ruby;

            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.Styles[ScintillaNET.Style.Ruby.Default].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Ruby.CommentLine].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Ruby.Default].Size = 10;
            scintilla.Styles[ScintillaNET.Style.Ruby.CommentLine].ForeColor = Color.ForestGreen;
            scintilla.Styles[ScintillaNET.Style.Ruby.Pod].ForeColor = Color.ForestGreen;
            scintilla.Styles[ScintillaNET.Style.Ruby.Number].ForeColor = Color.DarkRed;
            scintilla.Styles[ScintillaNET.Style.Ruby.Word].ForeColor = Color.Navy;
            scintilla.Styles[ScintillaNET.Style.Ruby.WordDemoted].ForeColor = Color.Violet;
            scintilla.Styles[ScintillaNET.Style.Ruby.String].ForeColor = Color.Purple;
            scintilla.Styles[ScintillaNET.Style.Ruby.Character].ForeColor = Color.Indigo;
            scintilla.Styles[ScintillaNET.Style.Ruby.Operator].ForeColor = Color.SteelBlue;
            scintilla.Styles[ScintillaNET.Style.Ruby.Regex].ForeColor = Color.OrangeRed;
            scintilla.Styles[ScintillaNET.Style.Ruby.Global].ForeColor = Color.DarkSlateGray;
            scintilla.Styles[ScintillaNET.Style.Ruby.Symbol].ForeColor = Color.Olive;
            scintilla.SetKeywords(0, @"BEGIN END __ENCODING__ __END__ __FILE__ __LINE__ alias and begin break case class def defined? do else elsif end ensure false for if in module next nil not or redo rescue return self super then true undef unless until when while yield");
            //scintilla.SetKeywords(1, @"Audio Graphics Input RPG Bitmap Color Font Plane Rect Sprite Table Tilemap Tone Viewport Window RGSSError");
            
            scintilla.EdgeColumn = 80;
            scintilla.EdgeMode = ScintillaNET.EdgeMode.Line;
            scintilla.EdgeColor = Color.Gray;
            scintilla.Margins[0].Width = 16;
            scintilla.TextChanged += scintilla_TextChanged;

            FormClosed += DialogScriptEditor_FormClosed;
            listBoxScripts.SelectedIndexChanged += ListBoxScripts_SelectedIndexChanged;
            Scripts = Editor.Project.CreateTemporaryScriptHive();
            //if the script hive is empty, let's add a blank script so that there is a starting point
            if(Scripts.Scripts.Count == 0)
            {
                Scripts.AddScript(new Script(0, "", ""));
            }
            
            //quickload first script
            textBoxScriptName.Text = Scripts.Scripts[0].Name;
            scintilla.Text = Scripts.Scripts[0].Contents;
            scintilla.Focus();
            scintilla.CurrentPosition = Scripts.Scripts[0].lastPosition;

            RefreshScriptList();
            listBoxScripts.SelectedIndex = 0;

            textBoxScriptName.TextChanged += TextBoxScriptName_TextChanged;
        }

        private int maxLineNumberCharLength;
        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var maxLineNumberCharLength = scintilla.Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == this.maxLineNumberCharLength)
                return;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int padding = 2;
            scintilla.Margins[0].Width = scintilla.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
            this.maxLineNumberCharLength = maxLineNumberCharLength;
        }

        private void TextBoxScriptName_TextChanged(object sender, EventArgs e)
        {
            //update script name
            Scripts.Scripts[lastSelectedIndex].Name = textBoxScriptName.Text;
            RefreshScriptList();
        }

        private void ListBoxScripts_SelectedIndexChanged(object sender, EventArgs e)
        {
            //apply old script
            Scripts.Scripts[lastSelectedIndex].Contents = scintilla.Text;
            Scripts.Scripts[lastSelectedIndex].lastPosition = scintilla.CurrentPosition;

            //load new script
            scintilla.Text = Scripts.Scripts[listBoxScripts.SelectedIndex].Contents;
            scintilla.Focus();
            scintilla.CurrentPosition = Scripts.Scripts[listBoxScripts.SelectedIndex].lastPosition;

            //update last id
            lastSelectedIndex = listBoxScripts.SelectedIndex;

            //load new title without triggering update
            textBoxScriptName.TextChanged -= TextBoxScriptName_TextChanged;
            textBoxScriptName.Text = Scripts.Scripts[listBoxScripts.SelectedIndex].Name;
            textBoxScriptName.TextChanged += TextBoxScriptName_TextChanged;
        }

        private void RefreshScriptList()
        {
            listBoxScripts.SelectedIndexChanged -= ListBoxScripts_SelectedIndexChanged;
            int index = listBoxScripts.SelectedIndex;
            ScriptNames = new List<string>();
            foreach(Script s in Scripts.Scripts)
            {
                ScriptNames.Add(s.Name);
            }
            listBoxScripts.DataSource = ScriptNames;
            listBoxScripts.SelectedIndex = index;
            listBoxScripts.SelectedIndexChanged += ListBoxScripts_SelectedIndexChanged;
        }

        private void DialogScriptEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            TryClose();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            //apply current script, in case of changes since last id change
            Scripts.Scripts[lastSelectedIndex].Contents = scintilla.Text;
            Scripts.Scripts[lastSelectedIndex].lastPosition = scintilla.CurrentPosition;
            //pass changed hive to project data and touch the editor
            Editor.Project.Scripts = Scripts;
            Editor.Touch();
        }
        
        private void TryClose()
        {
            //todo: scripts-local dirty flag check and save/discard prompt
            Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            TryClose();
        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //attempt to insert before the current selected script
            Scripts.Scripts.Insert(lastSelectedIndex, new Script(0, "", ""));
            RefreshScriptList();
            //update the selection to the new script, which has the old id, but we still need to apply changes to the new id for the old script
            Scripts.Scripts[lastSelectedIndex + 1].Contents = scintilla.Text;
            scintilla.Text = "";
            //load new/blank title without triggering update
            textBoxScriptName.TextChanged -= TextBoxScriptName_TextChanged;
            textBoxScriptName.Text = "";
            textBoxScriptName.TextChanged += TextBoxScriptName_TextChanged;
        }
    }
}
