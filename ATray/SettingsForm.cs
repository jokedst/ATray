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
    using RepositoryManager;

    public partial class SettingsForm : Form
    {
        //private List<ISourceRepository> _updatedRepoList = Program.Repositories;

        public SettingsForm()
        {
            InitializeComponent();
#if DEBUG
            this.Icon = new Icon(GetType(), "debug.ico");
#endif
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            UpdateRepoList();
            tabControl.SelectedIndex = 1;
        }

        private void repoList_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = repoList.HitTest(e.Location);
            if (e.Button != MouseButtons.Right) return;
            if (hit.Location == ListViewHitTestLocations.None)
            {
                repoListMenu.Show(Cursor.Position);
            }
            else
            {
                _repoToEdit = hit.Item;
                editRepoMenu.Show(Cursor.Position);
            }
        }

        private ListViewItem _repoToEdit = null;

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAddDialog();

            //var dialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
            //var result = dialog.ShowDialog();

            //if (result == DialogResult.OK)
            //{
            //    repoList.Items.Add(dialog.SelectedPath).SubItems.AddRange(new[] { "s1", "s2", "s3" });
            //}
            

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
            // Since the repo list actually modifies the live list, on cancel we simply restore the last list
            Program.repositories.ReloadFromFile();
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // TODO: Save settings if I ever get any?
            
            // Save new repo list
            //Program.Repositories = _updatedRepoList;
            //Program.SaveRepoList();
            Program.repositories.Save();
            Close();
        }

        private void buttonAddRepository_Click(object sender, EventArgs e)
        {
            ShowAddDialog();
        }

        private void ShowAddDialog()
        {
            var dialog = new AddRepositoryForm();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            Trace.TraceInformation($"About to add repo {dialog.textboxPath.Text}");

            var repo = new GitRepository(dialog.textboxPath.Text);
            if (!repo.Valid())
            {
                MessageBox.Show("Invalid directory! This is not a supported repository path.", "Invalid repository path");
                return;
            }

            repo.UpdateSchedule = dialog.ChosenSchedule;
            //_updatedRepoList.Add(repo);
            Program.repositories.Add(repo);
            UpdateRepoList();
        }

        private void UpdateRepoList()
        {
            repoList.Items.Clear();

            foreach (var repository in Program.repositories)
            {
                repoList.Items.Add(RepoToRow(repository));
            }
        }

        private ListViewItem RepoToRow(ISourceRepository repo)
        {
            var ret = new ListViewItem(new string[5]);
            ret.SubItems[columnName.Index].Text = repo.Name;
            ret.SubItems[columnStatus.Index].Text = repo.LastStatus.ToString();
            ret.SubItems[columnPath.Index].Text = repo.Location;
            ret.SubItems[columnLastUpdated.Index].Text = repo.LastStatusAt.ToString();
            ret.SubItems[columnSchedule.Index].Text = repo.UpdateSchedule.ToString();
            return ret;
        }

        private void OnClickEdit(object sender, EventArgs e)
        {
            if (_repoToEdit == null) return;
            var location = _repoToEdit.SubItems[columnPath.Index].Text;
            var repo = Program.repositories.Single(x => x.Location == location);

            var dialog = new AddRepositoryForm();
            dialog.Text = "Edit Repository";
            dialog.textboxPath.Text = location;
            dialog.SetSchedule((int)repo.UpdateSchedule);
            if (dialog.ShowDialog() != DialogResult.OK) return;

            Trace.TraceInformation($"About to edit repo {dialog.textboxPath.Text}");
            repo.Location = dialog.textboxPath.Text;
            repo.UpdateSchedule = dialog.ChosenSchedule;
        }
    }
}
