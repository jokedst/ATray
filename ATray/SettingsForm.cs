using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ATray
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            //repoList.Columns.
            repoList.Items.Add("hej");
            repoList.Items.Add("Column1Text").SubItems.AddRange(new [] { "s1", "s2", "s3" });

            tabControl.SelectedIndex = 1;
        }

        private void repoList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (repoList.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    repoListMenu.Show(Cursor.Position);
                }
            }
        }
    }
}
