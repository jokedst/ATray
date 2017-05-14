using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATray
{
    using System.IO;
    using Dialogs;

    public partial class AddRepositoryForm : Form
    {
        public AddRepositoryForm()
        {
            InitializeComponent();
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFolderDialog())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    textboxPath.Text = frm.Folder;
                }
            }
        }

        private void textboxPath_TextChanged(object sender, EventArgs e)
        {
            buttonTest.Enabled = !string.IsNullOrEmpty(textboxPath.Text);
        }

        private void scheduleTrackBar_Scroll(object sender, EventArgs e)
        {
            switch (scheduleTrackBar.Value)
            {
                case 0: scheduleLabel.Text = "Never"; break;
                case 4: scheduleLabel.Text = "Every minute"; break;
                case 3: scheduleLabel.Text = "Every 5 minutes"; break;
                case 2: scheduleLabel.Text = "Every hour"; break;
                case 1: scheduleLabel.Text = "Every day"; break;
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            validationResultLabel.Text = "I dunno";
        }
    }
}
