namespace ATray
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.IO.Pipes;
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
        private ActivityHistoryForm historyForm;
        private SettingsForm settingsForm;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Icon = trayIcon.Icon = Program.MainIcon;
            Program.Repositories.RepositoryStatusChanged += OnRepositoryStatusChanged;
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
#if DEBUG
            // DEBUG! Show dialog on boot for convinience
         //   OnMenuClickSettings(null, null);
            OnMenuClickHistory(null, null);
          //  new DiskUsageForm().Show();
#endif
            var first = true;
            foreach (var repo in Program.Repositories)
            {
                if (first)
                {
                    trayMenu.Items.Insert(0, new ToolStripSeparator());
                    first = false;
                }
                var repoLocation = repo.Location; // for clojures
                var submenu = new ToolStripMenuItem($"{repo.Name}: {Environment.NewLine}   {repo.LastStatus}");
                submenu.Name = repoLocation;
                if (repo is GitRepository && Program.TortoiseGitLocation != null)
                {
                    submenu.DropDownItems.Add(new ToolStripMenuItem("Log", null, (sender, args) => Process.Start(TortoiseGit.LogCommand(repoLocation))));
                    if (repo.LastStatus.HasFlag(RepoStatus.Dirty))
                        submenu.DropDownItems.Add(new ToolStripMenuItem("Commit", null,(s,a)=> TortoiseGit.RunCommit(repoLocation)));
                }
                if(repo is GitRepository && Program.GitBashLocation != null)
                    submenu.DropDownItems.Add(new ToolStripMenuItem("Git Bash", null, (sender, args) => Process.Start(new ProcessStartInfo(Program.GitBashLocation) { WorkingDirectory = repoLocation })));

                submenu.DropDownItems.Add(new ToolStripMenuItem("Open in explorer", null, (sender, args) => Process.Start(repoLocation)));
                submenu.DropDownItems.Add(new ToolStripMenuItem("Update", null, (sender, args) => Program.Repositories.UpdateRepo(repoLocation)));
                var pullOption = new ToolStripMenuItem("Pull Changes", null,
                    (sender, args) => Program.Repositories.UpdateRepo(repoLocation));
                if (repo.LastStatus != RepoStatus.Behind) pullOption.Visible = false;
                submenu.DropDownItems.Add(pullOption);
                trayMenu.Items.Insert(0, submenu);
            }

            var animTray = new IconAnimator(trayIcon, Properties.Resources.anim1);
            animTray.StartAnimation();
        }

        private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
        {
            // When logging in, unlocking and a few other events we want to update immediatly
            switch (sessionSwitchEventArgs.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    Program.Repositories.TriggerUpdate(r => r.UpdateSchedule != Schedule.Never);
                    break;
            }

            Trace.TraceInformation("Session changed? ({0})", sessionSwitchEventArgs.Reason);
        }

        private void OnRepositoryStatusChanged(object sender, RepositoryEventArgs e)
        {
            // Notify
            trayIcon.ShowBalloonTip(500, "Repo changed", $"Repository {e.Name} has changed from status {e.OldStatus} to {e.NewStatus}", ToolTipIcon.Info);

            // Update menu
            foreach (var menuRow in trayMenu.Items.Find(e.Location, false))
            {
                this.UIThread(()=>menuRow.Text = e.Name + ": \n" + e.NewStatus);
            }
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
            else if (FormWindowState.Minimized == WindowState) Hide();
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
            var idle = WindowsInternals.GetIdleTime();
            lblSmall.Text = MillisecondsToString(idle);

            // Only call "Now" once to avoid annoying bugs
            var now = DateTime.Now;

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

            WindowsInternals.GetForegroundProcessInfo(out string foregroundApp, out string foregroundTitle);

            if (now.Subtract(lastSave).TotalSeconds > Program.Configuration.SaveInterval)
            {
                // Time to save
                var wasActive = idle < Program.Configuration.SaveInterval * 1000;
                ActivityManager.SaveActivity(now, (uint) Program.Configuration.SaveInterval, wasActive, foregroundApp, foregroundTitle);
                lastSave = now;
            }
            
            lblWork.Text = MillisecondsToString(workingtime);
            lblDebug.Text = foregroundApp + " : " + foregroundTitle;
        }

        private void OnMainWindowLoad(object sender, EventArgs e)
        {
            mainTimer.Start();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // If the window is not showing a warning, minimize when moving the mouse over it
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
        }

        private void OnMenuClickSettings(object sender, EventArgs e)
        {
            if (settingsForm == null || settingsForm.IsDisposed)
                settingsForm = new SettingsForm();
            settingsForm.Show();
        }

        protected override void WndProc(ref Message m)
        {
            // Detect closing/opening of lid
            if (m.Msg == WindowsInternals.WM_POWERBROADCAST && m.WParam.ToInt32() == WindowsInternals.PBT_POWERSETTINGCHANGE)
            {
                var ps = (WindowsInternals.POWERBROADCAST_SETTING) Marshal.PtrToStructure(m.LParam, typeof(WindowsInternals.POWERBROADCAST_SETTING));
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
            const int errorCancelled = 1223; //The operation was canceled by the user.

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
                    if (ex.NativeErrorCode != errorCancelled) throw;
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
