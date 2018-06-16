namespace ATray
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using System.Diagnostics;
    using System.Reflection;
    using RepositoryManager;
    using RepositoryManager.Git;

    public interface ISettingsDialog:IDisposable
    {
        bool Focus();
        void Show(IWin32Window owner);
    }

    public partial class SettingsForm : Form, ISettingsDialog
    {
        private readonly IAddRepositoryDialog _addRepositoryDialog;
        private readonly IRepositoryCollection _repositories;
        //private List<ISourceRepository> _updatedRepoList = Program.Repositories;

        public SettingsForm(IAddRepositoryDialog addRepositoryDialog, IRepositoryCollection repositories)
        {
            _addRepositoryDialog = addRepositoryDialog;
            _repositories = repositories;

            InitializeComponent();
#if DEBUG
            Icon = new Icon(GetType(), "debug.ico");
#endif
            propertyGrid.SelectedObject = Program.Configuration.Clone();
            _repositories.RepositoryUpdated += OnRepositoryUpdated;

            var v = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = $"v{v.Major}.{v.Minor}.{v.Build}";
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
                _repositories.RepositoryUpdated -= OnRepositoryUpdated;
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
            _repositories.ReloadFromFile();
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Save settings 
            var newConfig = (Configuration) propertyGrid.SelectedObject;
            if (!Program.Configuration.ApplyChanges(newConfig))
            {
                MessageBox.Show("Invalid configuration for some reason", "Configuration error");
                return;
            }
            Program.Configuration = newConfig;
            Program.Configuration.SaveToIniFile();

            // Save new repo list
            _repositories.Save();
            Program.MainWindowInstance.CreateRepositoryMenyEntries();
            Close();
        }

        private void OnClickAddRepository(object sender, EventArgs e)
        {
            var repo = _addRepositoryDialog.AddRepository(this.Owner);
            if (repo == null) return;
            _repositories.Add(repo);
            UpdateRepoList();
        }

        private void UpdateRepoList()
        {
            repoList.Items.Clear();

            foreach (var repository in _repositories)
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
            var repository = _repositories.GetByName(_clickedRepository.SubItems[columnName.Index].Text);
            if (_addRepositoryDialog.EditRepository(this.Owner, repository))
            {
                _repositories.RepositoryModified(repository);
                UpdateRepoList();
            }

            //// Old code
            //var location = _clickedRepository.SubItems[columnPath.Index].Text;
            //var repo = _repositories.Single(x => x.Location == location);

            //var dialog = new AddRepositoryForm(this.Owner ?? this)
            //{
            //    Text = "Edit Repository",
            //    OkButtonText = "&Save",
            //    textboxPath = {Text = location},
            //    RepoName = repo.Name
            //};
            //dialog.SetSchedule((int)repo.UpdateSchedule);
            //if (dialog.ShowDialog() != DialogResult.OK) return;

            //Trace.TraceInformation($"About to edit repo {dialog.textboxPath.Text}");
            //repo.Location = dialog.textboxPath.Text;
            //repo.UpdateSchedule = dialog.ChosenSchedule;
            //repo.Name = dialog.RepoName;
            //UpdateRepoList();
        }

        private void OnClickUpdateRepo(object sender, EventArgs e)
        {
            if (_clickedRepository == null) return;
            var location = _clickedRepository.SubItems[columnPath.Index].Text;
            _repositories.UpdateRepo(location);
        }

        private void OnClickRemoveRepo(object sender, EventArgs e)
        {
            if (_clickedRepository == null) return;
            var location = _clickedRepository.SubItems[columnPath.Index].Text;
            _repositories.Remove(location);
            UpdateRepoList();
        }
    }
}
