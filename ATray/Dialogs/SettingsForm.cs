namespace ATray
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Reflection;
    using RepositoryManager;
    using Tools;

    public interface ISettingsDialog : IDisposable
    {
        bool Focus();
        void Show(IWin32Window owner);
        bool IsDisposed { get; }
        bool Visible { get; }
    }

    public partial class SettingsForm : Form, ISettingsDialog
    {
        private readonly IAddRepositoryDialog _addRepositoryDialog;
        private readonly IRepositoryCollection _repositories;
        private readonly IHaveLogs _logSource;

        public SettingsForm(IAddRepositoryDialog addRepositoryDialog, IRepositoryCollection repositories, IHaveLogs logSource)
        {
            _addRepositoryDialog = addRepositoryDialog;
            _repositories = repositories;
            _logSource = logSource;

            InitializeComponent();
#if DEBUG
            Icon = new Icon(GetType(), "debug.ico");
#endif
            propertyGrid.SelectedObject = Program.Configuration.Clone();
            _repositories.RepositoryUpdated += OnRepositoryUpdated;

            var v = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = $"v{v.Major}.{v.Minor}.{v.Build}";

            logTextbox.Text = logSource.RegisterCallback(this,
                s => this.UIThread(() => logTextbox.AppendText(s + Environment.NewLine)));
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
                _logSource.Unregister(this);
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
            Trace.TraceInformation("Setting dialog was cancelled");
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
                MessageBox.Show("Invalid configuration for some reason", "ATray Configuration error");
                return;
            }
            Program.Configuration = newConfig;
            Program.Configuration.SaveToIniFile();

            // Save new repo list
            _repositories.Save();
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
        }

        private void OnClickUpdateRepo(object sender, EventArgs e)
        {
            if (_clickedRepository == null) return;
            var location = _clickedRepository.SubItems[columnPath.Index].Text;
            _repositories.TriggerUpdate(location, true);
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
