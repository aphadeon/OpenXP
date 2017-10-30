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
    public partial class DialogChangeMaximum : Form
    {
        public int OldValue = 0;
        public int NewValue = 0;
        public DialogResult DialogResult = DialogResult.Cancel;

        public DialogChangeMaximum(int oldValue)
        {
            InitializeComponent();
            OldValue = oldValue;
            numericUpDown.Value = oldValue;
            NewValue = oldValue;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            int value = (int) numericUpDown.Value;
            if(value < 1 || value > 9999)
            {
                System.Windows.Forms.MessageBox.Show("Please enter a value between 1 and 9999.");
                numericUpDown.Value = OldValue;
            } else
            {
                NewValue = value;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
