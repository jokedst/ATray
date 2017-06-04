namespace ATray
{
    using System;
    using System.Windows.Forms;
    using Dialogs;
    using RepositoryManager;

    public partial class AddRepositoryForm : Form
    {
        public AddRepositoryForm()
        {
            InitializeComponent();
            this.Icon = Program.MainIcon;
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFolderDialog())
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                textboxPath.Text = frm.Folder;
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    var repo = new GitRepository(textboxPath.Text);
                    NameTextBox.Text = repo.Name;
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

        public string RepoName
        {
            get => NameTextBox.Text;
            set => NameTextBox.Text = value;
        }

        public string OkButtonText
        {
            set => button2.Text = value;
        }

        private void OnClickValidate(object sender, EventArgs e)
        {
            var repo = new GitRepository(textboxPath.Text);
            validationResultLabel.Text = repo.Valid() ? "Valid!" : "Directory is not a valid repository!";
        }
    }
}
