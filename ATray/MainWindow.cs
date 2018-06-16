using ATray.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using Activity;
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using RepositoryManager;
    using RepositoryManager.Git;

    public partial class MainWindow : Form
    {
        private uint workingtime;
        private DateTime startTime = DateTime.Now;
        private bool inWarnState;
        private bool reallyClose;
        private DateTime lastSave = DateTime.MinValue;
        private DateTime lastTimerEvent = DateTime.MinValue;
        private ActivityHistoryForm historyForm;
        private SettingsForm settingsForm;
        private OverallStatusType OverallStatus;
        private WebServer webServer;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Icon = trayIcon.Icon = Program.GreyIcon;
            Program.Repositories.RepositoryStatusChanged += OnRepositoryStatusChanged;
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
#if DEBUG
            // DEBUG! Show dialog on boot for convinience
            OnMenuClickSettings(null, null);
            //OnMenuClickHistory(null, null);
            //  new DiskUsageForm().Show();
#endif
            CreateRepositoryMenyEntries();
            //var animTray = new IconAnimator(trayIcon, Properties.Resources.anim1);
            //animTray.StartAnimation();

            this.webServer = new WebServer(this, "http://localhost:14754");
            this.webServer.Run();
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
                    if (Program.Repositories.All(x => x.Location != name))
                        toRemove.Add(name);
                }

                foreach (var itemName in toRemove)
                {
                    trayMenu.Items.RemoveByKey(itemName);
                }
            }
            
            foreach (var repo in Program.Repositories)
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
                submenu.DropDownItems.Add(new ToolStripMenuItem("Update", null, (sender, args) => Program.Repositories.UpdateRepo(repoLocation)));
                var pullOption = new ToolStripMenuItem("Pull Changes", null,
                    (sender, args) => Program.Repositories.UpdateRepo(repoLocation));
                if (repo.LastStatus != RepoStatus.Behind) pullOption.Visible = false;
                submenu.DropDownItems.Add(pullOption);
                UpdateRepoMenu(submenu, repo.Name, repo.LastStatus);
                trayMenu.Items.Insert(0, submenu);
            }

            UpdateIcon();
        }

        private void UpdateIcon()
        {
            var worstStatus = Program.Repositories.Select(x => x.LastStatus.ToOverallStatus()).OrderBy(x=>x).LastOrDefault();
            if (worstStatus == OverallStatus) return;
            OverallStatus = worstStatus;
            switch (OverallStatus)
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
            foreach (var menuRow in trayMenu.Items.Find(e.Location, false))
            {
                //this.UIThread(() => menuRow.Text = e.Name + ": \n" + e.NewStatus);
                this.UIThread(() =>
                {
                    UpdateRepoMenu(menuRow, e.Name, e.NewStatus);
                    UpdateIcon();
                });
            }
        }

        private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
        {
            // When logging in or unlocking we want to update immediatly
            if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionLogon ||
                sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionUnlock)
                Program.Repositories.TriggerUpdate(r => r.UpdateSchedule != Schedule.Never);

            Trace.TraceInformation("Session changed? ({0})", sessionSwitchEventArgs.Reason);
        }

        private static string MillisecondsToString(uint ms)
        {
            var totsec = (uint)Math.Round(ms / 1000d);
            var totmin = totsec / 60;
            var tothour = totmin / 60;
            var sec = totsec % 60;
            var min = totmin % 60;
            var sb = new StringBuilder();
            if (tothour > 0) sb.AppendFormat("{0}h", tothour);
            if (totmin > 0) sb.AppendFormat("{0}m", min);
            sb.AppendFormat("{0}s", sec);
            return sb.ToString();
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (inWarnState) ShowMe();
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

        private void menuExit_Click(object sender, EventArgs e)
        {
            reallyClose = true;
            Close();
        }

        private void OnMainTimerTick(object sender, EventArgs e)
        {
            var idle = NativeMethods.GetIdleTime();
            // Only call "Now" once to avoid annoying bugs
            var now = DateTime.Now;

            var unpoweredSeconds = (uint) Math.Min(now.Subtract(lastTimerEvent).TotalSeconds, uint.MaxValue);
            if (unpoweredSeconds > 100)
            {
                // This is supposed to fire every second. Now it hasn't -> most likely boot or sleep or something
                idle = Math.Max(idle, unpoweredSeconds - 2);
            }

            lblSmall.Text = MillisecondsToString(idle);


            if (idle > Program.Configuration.MinimumBrakeLength * 1000)
            {
                BackColor = Color.Green;
                lblInfo.Text = "You can start working now";
                workingtime = 0;
                startTime = now;
                TopMost = false;
                inWarnState = false;
            }
            else
            {
                workingtime += (uint)now.Subtract(startTime).TotalMilliseconds;
                startTime = now;

                if (workingtime > Program.Configuration.MaximumWorkTime * 1000 && !inWarnState)
                {
                    BackColor = Color.Red;
                    lblInfo.Text = "Take a break!";
                    ShowMe();
                    TopMost = true;
                    inWarnState = true;
                }
            }

            NativeMethods.GetForegroundProcessInfo(out string foregroundApp, out string foregroundTitle);

            if (now.Subtract(lastSave).TotalSeconds > Program.Configuration.SaveInterval)
            {
                // Time to save
                var wasActive = idle < Program.Configuration.SaveInterval * 1000;
                ActivityManager.SaveActivity(now, (uint) Program.Configuration.SaveInterval, wasActive, foregroundApp, foregroundTitle);
                lastSave = now;
            }
            
            lblWork.Text = MillisecondsToString(workingtime);
            lblDebug.Text = foregroundApp + " : " + foregroundTitle;
            lastTimerEvent = now;
        }

        private void OnMainWindowLoad(object sender, EventArgs e) 
            => mainTimer.Start();

        /// <summary>
        /// Minimize the window when moving the mouse over it (unless a warning is shown)
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!inWarnState)
                Hide();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Don't close the program, just minimize it (unless they used the menu)
            if (e.CloseReason == CloseReason.UserClosing && !reallyClose)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                historyForm?.Close();
            }
        }

        private void OnMenuClickHistory(object sender, EventArgs e)
        {
            if (historyForm == null || historyForm.IsDisposed)
                historyForm = new ActivityHistoryForm();
            if (historyForm.IsDisposed) return;
            historyForm.Show();
            historyForm.Focus();
        }

        private void OnMenuClickSettings(object sender, EventArgs e)
        {
            ISettingsDialog settingsDialog = Program.ServiceProvider.GetService<ISettingsDialog>();
            //settingsForm;
            //if (settingsDialog == null || settingsForm.IsDisposed)
            //    settingsDialog = new SettingsForm();
            settingsDialog.Show(this);
            settingsDialog.Focus();
        }

        protected override void WndProc(ref Message m)
        {
            // Detect closing/opening of lid
            if (m.Msg == NativeMethods.WM_POWERBROADCAST && m.WParam.ToInt32() == NativeMethods.PBT_POWERSETTINGCHANGE)
            {
                var ps = (NativeMethods.POWERBROADCAST_SETTING) Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.POWERBROADCAST_SETTING));
                IntPtr pData = (IntPtr) (m.LParam.ToInt32() + Marshal.SizeOf(ps));
                int iData = (int) Marshal.PtrToStructure(pData, typeof(int));
                string monitorState;
                switch (iData)
                {
                    case 0: monitorState = "off"; break;
                    case 1: monitorState = "on"; break;
                    case 2: monitorState = "dimmed"; break;
                    default: monitorState = "unknown"; break;
                }
                Trace.TraceInformation("Monitor changed to " + monitorState);
            }
            base.WndProc(ref m);
        }

        private void diskUsageToolStripMenuItem_Click(object sender, EventArgs e)
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
                    MessageBox.Show("Operation aborted");
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
    }
}
