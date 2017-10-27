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
    public partial class DialogChangeGameTitle : Form
    {
        public DialogChangeGameTitle()
        {
            InitializeComponent();
            textBoxGameTitle.Text = Editor.Project.Ini.Title;
            FormClosed += DialogChangeGameTitle_FormClosed;
        }

        public void Cancel()
        {
            Close();
        }

        private void DialogChangeGameTitle_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cancel();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            //todo: sanitize
            Editor.Project.Ini.Title = textBoxGameTitle.Text;
            Editor.Touch();
            Close();
        }
    }
}
