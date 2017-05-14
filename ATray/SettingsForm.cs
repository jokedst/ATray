using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ATray
{
    using System.Diagnostics;

    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
#if DEBUG
            this.Icon = new Icon(GetType(), "debug.ico");
#endif
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            //repoList.Columns.
            repoList.Items.Add("hej");
            repoList.Items.Add("Column1Text").SubItems.AddRange(new [] { "s1", "s2", "s3" });

            tabControl.SelectedIndex = 1;
        }

        private void repoList_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo HI = repoList.HitTest(e.Location);
            if (e.Button == MouseButtons.Right)
            {
                if (HI.Location == ListViewHitTestLocations.None)
                {
                    repoListMenu.Show(Cursor.Position);
                }
                else
                {
                    RepoToEdit = HI.Item;
                    editRepoMenu.Show(Cursor.Position);
                }
            }
        }

        private ListViewItem RepoToEdit = null;

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                repoList.Items.Add(dialog.SelectedPath).SubItems.AddRange(new[] { "s1", "s2", "s3" });
            }
            

            //var fbd = new FolderBrowserDialog();
            //var newDir = fbd.ShowDialog();

            //// Prepare a dummy string, thos would appear in the dialog
            //string dummyFileName = "Save Here";

            //SaveFileDialog sf = new SaveFileDialog();
            //// Feed the dummy name to the save dialog
            //sf.FileName = dummyFileName;

            //if (sf.ShowDialog() == DialogResult.OK)
            //{
            //    // Now here's our save folder
            //    string savePath = Path.GetDirectoryName(sf.FileName);
            //    // Do whatever
            //}
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // TODO: Reset form or something
            Hide();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // TODO: Commit changes or something
            Hide();
        }

        private void buttonAddRepository_Click(object sender, EventArgs e)
        {
            var dialog = new AddRepositoryForm();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            Trace.TraceInformation($"About to add repo {dialog.textboxPath.Text}");
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RepoToEdit == null) return;

            var dialog = new AddRepositoryForm();
            dialog.textboxPath.Text = RepoToEdit.SubItems[2].Text;
            if (dialog.ShowDialog() != DialogResult.OK) return;

            Trace.TraceInformation($"About to edit repo {dialog.textboxPath.Text}");
        }
    }
}
