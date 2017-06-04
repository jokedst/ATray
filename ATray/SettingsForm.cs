namespace ATray
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using System.Diagnostics;
    using RepositoryManager;

    public partial class SettingsForm : Form
    {
        //private List<ISourceRepository> _updatedRepoList = Program.Repositories;

        public SettingsForm()
        {
            InitializeComponent();
#if DEBUG
            Icon = new Icon(GetType(), "debug.ico");
#endif
            propertyGrid.SelectedObject = Program.Configuration.Clone();
            Program.Repositories.RepositoryUpdated += OnRepositoryUpdated;
        }

        private void OnRepositoryUpdated(object sender, RepositoryEventArgs repositoryEventArgs)
        {
            if(Visible)
                this.UIThread(UpdateRepoList);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                Program.Repositories.RepositoryUpdated -= OnRepositoryUpdated;
            }
            base.Dispose(disposing);
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
                _clickedRepository = hit.Item;
                editRepoMenu.Show(Cursor.Position);
            }
        }

        private ListViewItem _clickedRepository;

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Since the repo list actually modifies the live list, on cancel we simply restore the last list
            Program.Repositories.ReloadFromFile();
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Save settings 
            Program.Configuration = (Configuration) propertyGrid.SelectedObject;
            Program.Configuration.SaveToIniFile();
            
            // Save new repo list
            Program.Repositories.Save();
            Close();
        }

        private void ClickAddRepository(object sender, EventArgs e)
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
            if (!string.IsNullOrWhiteSpace(dialog.RepoName))
                repo.Name = dialog.RepoName;
            Program.Repositories.Add(repo);
            UpdateRepoList();
        }

        private void UpdateRepoList()
        {
            repoList.Items.Clear();

            foreach (var repository in Program.Repositories)
            {
                repoList.Items.Add(RepoToRow(repository));
            }
            repoList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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
            if (_clickedRepository == null) return;
            var location = _clickedRepository.SubItems[columnPath.Index].Text;
            var repo = Program.Repositories.Single(x => x.Location == location);

            var dialog = new AddRepositoryForm();
            dialog.Text = "Edit Repository";
            dialog.OkButtonText = "&Save";
            dialog.textboxPath.Text = location;
            dialog.SetSchedule((int)repo.UpdateSchedule);
            dialog.RepoName = repo.Name;
            if (dialog.ShowDialog() != DialogResult.OK) return;

            Trace.TraceInformation($"About to edit repo {dialog.textboxPath.Text}");
            repo.Location = dialog.textboxPath.Text;
            repo.UpdateSchedule = dialog.ChosenSchedule;
            repo.Name = dialog.RepoName;
            UpdateRepoList();
        }

        private void OnClickUpdateRepo(object sender, EventArgs e)
        {
            if (_clickedRepository == null) return;
            var location = _clickedRepository.SubItems[columnPath.Index].Text;
            var repo = Program.Repositories.Single(x => x.Location == location);

            repo.UpdateStatus();
            UpdateRepoList();
        }
    }
}
