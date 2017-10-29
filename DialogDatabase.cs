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
        public DialogDatabase()
        {
            InitializeComponent();
            //todo: load data here
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
            //todo: write data back to ruby (not to file) here
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Save();
        }
    }
}
