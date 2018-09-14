using System.Diagnostics;

namespace ATray
{
    using System;
    using System.Windows.Forms;
    using Dialogs;
    using RepositoryManager;
    using RepositoryManager.Git;

    public interface IAddRepositoryDialog
    {
        ISourceRepository AddRepository(IWin32Window owningWindow);
        bool EditRepository(IWin32Window owningWindow, ISourceRepository repository);
    }

    public partial class AddRepositoryForm : Form, IAddRepositoryDialog
    {
        private readonly IRepositoryCollection _repositoryCollection;

        /// <summary> If editing contains the repo to edit </summary>
        private ISourceRepository _repo;

        public AddRepositoryForm(IRepositoryCollection repositoryCollection)
        {
            _repositoryCollection = repositoryCollection;
            InitializeComponent();
            Icon = Program.MainIcon;
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFolderDialog())
            {
                if (frm.ShowDialog(Owner) != DialogResult.OK) return;
                textboxPath.Text = frm.Folder;
                var repo = new GitRepository(textboxPath.Text);
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameTextBox.Text = repo.Name;
                }

                validationResultLabel.Text = repo.Valid() ? "Valid!" : "Directory is not a valid repository!";
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

        public void SetSchedule(int minutes)
        {
            if (minutes <= 0) scheduleTrackBar.Value = 0;
            else if (minutes < 5) scheduleTrackBar.Value = 4;
            else if (minutes < 60) scheduleTrackBar.Value = 3;
            else if (minutes < 24*60) scheduleTrackBar.Value = 2;
            else scheduleTrackBar.Value = 1;

            scheduleTrackBar_Scroll(null, null);
        }

        public Schedule ChosenSchedule
        {
            get
            {
                switch (scheduleTrackBar.Value)
                {
                    case 0: return Schedule.Never; 
                    case 4: return Schedule.EveryMinute;
                    case 3: return Schedule.FifthMinute;
                    case 2: return Schedule.EveryHour;
                    case 1: return Schedule.EveryDay;
                    default: return Schedule.Never;
                }
            }
        }

        private string RepoName
        {
            get => NameTextBox.Text;
            set => NameTextBox.Text = value;
        }

        private void OnClickValidate(object sender, EventArgs e)
        {
            var repo = new GitRepository(textboxPath.Text);
            validationResultLabel.Text = repo.Valid() ? "Valid!" : "Directory is not a valid repository!";
        }

        private void OnClickOk(object sender, EventArgs e)
        {
            var repo = new GitRepository(textboxPath.Text);
            if (!repo.Valid())
            {
                 MessageBox.Show("Invalid directory! This is not a supported repository path", "ATray Repository", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((_repo == null || _repo.Name != RepoName) && _repositoryCollection.ContainsName(RepoName))
            {
                MessageBox.Show($"A repository with name '{RepoName}' exists already", "ATray Repository", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _repo = _repo ?? repo;
            _repo.UpdateSchedule = ChosenSchedule;
            _repo.Name = RepoName;

            DialogResult = DialogResult.OK;
        }

        public ISourceRepository AddRepository(IWin32Window owningWindow)
        {
            Owner = owningWindow as Form;
            _repo = null;
            if (this.ShowDialog() != DialogResult.OK) return null;
            var newRepo = _repo;
            _repo = null;
            return newRepo;
        }

        public bool EditRepository(IWin32Window owningWindow, ISourceRepository repository)
        {
            _repo = repository;
            Owner = owningWindow as Form;
            Text = "Edit Repository";
            button2.Text = "&Save";
            textboxPath.Text = _repo.Location;
            RepoName = _repo.Name;
            SetSchedule((int)_repo.UpdateSchedule);

            if (ShowDialog() != DialogResult.OK) return false;

            Trace.TraceInformation($"About to edit repo {textboxPath.Text}");
            _repo.Location = textboxPath.Text;
            _repo.UpdateSchedule = ChosenSchedule;
            _repo.Name = RepoName;
            _repo = null;
            return true;
        }
    }
}
