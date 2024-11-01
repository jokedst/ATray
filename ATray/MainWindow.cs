﻿namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Activity;
    using Diskusage;
    using Newtonsoft.Json;
    using RepositoryManager;
    using RepositoryManager.Git;
    using Tools;

    public partial class MainWindow : Form, IShowNotifications
    {
        private bool _inWarnState;
        private bool _reallyExitProgram;
        private ActivityHistoryForm _historyForm;
        private OverallStatusType _overallStatus;

        private readonly IRepositoryCollection _repositoryCollection;
        private readonly IFactory<ISettingsDialog> _settingsDialogFactory;
        private readonly IActivityMonitor _activityMonitor;
        private ISettingsDialog _settingsDialog;

        public MainWindow(IRepositoryCollection repositoryCollection, 
            IFactory<ISettingsDialog> settingsDialogFactory,
            IActivityMonitor activityMonitor)
        {
            _repositoryCollection = repositoryCollection;
            _settingsDialogFactory = settingsDialogFactory;
            _activityMonitor = activityMonitor;

            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Icon = trayIcon.Icon = Program.GreyIcon;
            _repositoryCollection.RepositoryStatusChanged += OnRepositoryStatusChanged;
            _repositoryCollection.RepositoryListChanged += OnRepositoyListChanged;
            activityMonitor.UserWorkedTooLong += (sender, e) =>
            {
                BackColor = Color.Red;
                lblInfo.Text = "Take a break!";
                ShowMe();
                TopMost = true;
                _inWarnState = true;
            };
            activityMonitor.UserHasTakenBreak += (sender, e) =>
            {
                BackColor = Color.Green;
                lblInfo.Text = "You can start working now";
                TopMost = false;
                _inWarnState = false;
            };
            activityMonitor.UserIsBackFromAbsense += (sender, e) =>
            {
                _repositoryCollection.TriggerUpdate(r => r.UpdateSchedule != Schedule.Never);
            };

            lblSmall.DataBindings.Add(new Binding(nameof(Label.Text), activityMonitor, nameof(ActivityMonitor.IdleTime)));
            lblWork.DataBindings.Add(new Binding(nameof(Label.Text), activityMonitor, nameof(ActivityMonitor.WorkingTime)));
            lblDebug.DataBindings.Add(new Binding(nameof(Label.Text), activityMonitor, nameof(ActivityMonitor.CurrentlyActiveWindow)));
#if DEBUG
            // DEBUG! Show dialog on boot for convinience
           OnMenuClickSettings(null, null);
             //OnMenuClickHistory(null, null);
            //  new DiskUsageForm().Show();
#endif
            CreateRepositoryMenyEntries();
            //var animTray = new IconAnimator(trayIcon, Properties.Resources.anim1);
            //animTray.StartAnimation();
            NativeMethods.RegisterForPowerNotifications(this.Handle);
        }

        private void OnRepositoyListChanged(object sender, RepositoryEventArgs e)
        {
            CreateRepositoryMenyEntries();
        }

        public void CreateRepositoryMenyEntries()
        {
            if(!trayMenu.Items.ContainsKey("RepositorySeparator"))
                trayMenu.Items.Insert(0, new ToolStripSeparator() { Name = "RepositorySeparator" });
            else
            {
                var toRemove = new List<string>();
                for (int i = 0; i < trayMenu.Items.Count; i++)
                {
                    var name = trayMenu.Items[i].Name;
                    if (name == "RepositorySeparator") break;
                    if (!_repositoryCollection.ContainsName(name))
                        toRemove.Add(name);
                }

                foreach (var itemName in toRemove)
                {
                    trayMenu.Items.RemoveByKey(itemName);
                }
            }
            
            foreach (var repo in _repositoryCollection)
            {
                if (trayMenu.Items.ContainsKey(repo.Location))
                    continue;
                var repoLocation = repo.Location; // for clojures
                var submenu = new ToolStripMenuItem {Name = repoLocation};

                if (repo is GitRepository && Program.TortoiseGitLocation != null)
                {
                    submenu.DropDownItems.Add(new ToolStripMenuItem("Log", null, (sender, args) => Process.Start(TortoiseGit.LogCommand(repoLocation))));
                    if (repo.LastStatus.HasFlag(RepoStatus.Dirty))
                        submenu.DropDownItems.Add(new ToolStripMenuItem("Commit", null, (s, a) => TortoiseGit.RunCommit(repoLocation)));
                }
                if (repo is GitRepository && Program.GitBashLocation != null)
                    submenu.DropDownItems.Add(new ToolStripMenuItem("Git Bash", null, (sender, args) => Process.Start(new ProcessStartInfo(Program.GitBashLocation) { WorkingDirectory = repoLocation })));

                submenu.DropDownItems.Add(new ToolStripMenuItem("Open in explorer", null, (sender, args) => Process.Start(repoLocation)));
                submenu.DropDownItems.Add(new ToolStripMenuItem("Force Update", null, (sender, args) =>
                {
                    this.UIThread(()=>{submenu.Text = $"{repo.Name}: {Environment.NewLine}...updating"; });
                    _repositoryCollection.TriggerUpdate(repoLocation, true).ContinueWith(t => UpdateRepoMenu(submenu, repo.Name, repo.LastStatus));
                }));
                var pullOption = new ToolStripMenuItem("Pull Changes", null,
                    (sender, args) => _repositoryCollection.TriggerUpdate(repoLocation, false));
                if (repo.LastStatus != RepoStatus.Behind) pullOption.Visible = false;
                submenu.DropDownItems.Add(pullOption);
                UpdateRepoMenu(submenu, repo.Name, repo.LastStatus);
                trayMenu.Items.Insert(0, submenu);
            }

            UpdateIcon();
        }

        private void UpdateIcon()
        {
            var worstStatus = _repositoryCollection.WorstStatus().ToOverallStatus();
            if (worstStatus == _overallStatus) return;
            _overallStatus = worstStatus;
            switch (_overallStatus)
            {
                case OverallStatusType.Ok:
                    this.trayIcon.Icon = Program.GreyIcon;
                    break;
                case OverallStatusType.WarnAhead:
                case OverallStatusType.WarnBehind:
                    this.trayIcon.Icon = Program.YellowIcon;
                    break;
                case OverallStatusType.CodeRed:
                    this.trayIcon.Icon = Program.MainIcon;
                    break;
                default:
                    this.trayIcon.Icon = Program.MainIcon;
                    break;
            }
        }

        private void UpdateRepoMenu(ToolStripItem menuItem, string repoName, RepoStatus status)
        {
            menuItem.Text = $"{repoName}: {Environment.NewLine}   {status}";

            if (status == RepoStatus.Conflict)
                menuItem.BackColor = Color.FromArgb(0x80, Color.Red);
            else if (status == RepoStatus.Behind)
                menuItem.BackColor = Color.FromArgb(0x80, Color.Yellow);
            else if (status == RepoStatus.Dirty)
                menuItem.BackColor = Color.FromArgb(0x80, 0xB1, 0xB1, 0xFF);
            else menuItem.ResetBackColor();
        }

        public void ShowNotification(string text)
        {
            this.trayIcon.ShowBalloonTip(500, "ATray notification", text, ToolTipIcon.None);
        }

        private void OnRepositoryStatusChanged(object sender, RepositoryEventArgs e)
        {
            // Notify
            trayIcon.ShowBalloonTip(500, "Repo changed", $"Repository {e.Name} has changed from status {e.OldStatus} to {e.NewStatus}", ToolTipIcon.Info);

            // Update menu
            var menuItems = trayMenu.Items.Find(e.Location, false);
            if (menuItems.Length == 0) return;
            this.UIThread(() =>
            {
                foreach (var affectedMenuItem in menuItems)
                    UpdateRepoMenu(affectedMenuItem, e.Name, e.NewStatus);
                UpdateIcon();
            });
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (_inWarnState) ShowMe();
            else if (WindowState == FormWindowState.Minimized) Hide();
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            ShowMe();
        }

        private void ShowMe()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        /// <summary>
        /// Actually exit the program for real
        /// </summary>
        private void OnMenuClickExit(object sender, EventArgs e)
        {
            _reallyExitProgram = true;
            Close();
        }

        /// <summary>
        /// Minimize the window when moving the mouse over it (unless a warning is shown)
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_inWarnState)
                Hide();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Don't close the program, just minimize it (unless they used the menu)
            if (e.CloseReason == CloseReason.UserClosing && !_reallyExitProgram)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                _historyForm?.Close();
            }
        }

        private void OnMenuClickHistory(object sender, EventArgs e)
        {
            if (_historyForm == null || _historyForm.IsDisposed)
                _historyForm = new ActivityHistoryForm();
            if (_historyForm.IsDisposed) return;
            _historyForm.Show();
            _historyForm.Focus();
        }

        private void OnMenuClickSettings(object sender, EventArgs e)
        {
            if (_settingsDialog == null || _settingsDialog.IsDisposed)
                _settingsDialog = _settingsDialogFactory.Build();
            if(!_settingsDialog.Visible)
                _settingsDialog.Show(this);
            _settingsDialog.Focus();
        }

        private void OnMenuClickDiskUsage(object sender, EventArgs e)
        {
            // Use a named pipe to tlk to diskusage.exe, since it will run as admin.
            var pipeName = Guid.NewGuid().ToString("N");
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In))
            {
                var info = new ProcessStartInfo(@"diskusage\diskusage.exe")
                {
                    Arguments = "-c -N " + pipeName,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process duProcess;
                try
                {
                    duProcess = Process.Start(info);
                }
                catch (Win32Exception ex)
                {
                    // 1223 = The operation was canceled by the user
                    if (ex.NativeErrorCode != 1223) throw;
                    MessageBox.Show("Operation aborted", "ATray DiskUsage");
                    return;
                }

                pipeServer.WaitForConnection();
                DiskNode rootNode;

                using (StreamReader sr = new StreamReader(pipeServer))
                using (JsonTextReader jsonReader = new JsonTextReader(sr))
                {
                    JsonSerializer ser = new JsonSerializer();
                    rootNode = ser.Deserialize<DiskNode>(jsonReader);
                }

                duProcess.WaitForExit(1000);
            }
        }

        private void OnMenuClickCheckForUpdates(object sender, EventArgs e)
        {
            Trace.TraceInformation("Update check requested");
            if (!Program.UpdateTask.IsCompleted)
            {
                Trace.TraceInformation("Update check aborted: already running");
                MessageBox.Show("Update check is already running", "ATray Update");
                return;
            }
            Program.UpdateTask = Task.Run(() => Program.UpdateApp(true));
            Trace.TraceInformation("Update check task started");
        }

        protected override void WndProc(ref Message m)
        {
            _activityMonitor.HandleWindowsMessage(m);
            base.WndProc(ref m);
        }

        private void OnMenuClickResetIcon(object sender, EventArgs e)
        {
            Program.LoadIcons();
            this.UpdateIcon();
        }
    }
}
