namespace ATray
{
    using System;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Activity;

    public partial class MainWindow : Form
    {
        /// <summary> If user is away this long, a brake has been taken</summary>
        private const uint MinBrake = 1 * Minutes;

        /// <summary> How long the user may keep working without taking a break</summary>
        private const uint MaxWorkTime = 20 * Minutes;

        /// <summary> How often we flush activity data to disk </summary>
        private const uint SaveInterval = 1 * Minutes;


        private const uint Minutes = 60 * 1000; // Just to make above readable

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
            Icon = notifyIcon1.Icon = Program.MainIcon;
#if DEBUG
            //this.Icon = new Icon(GetType(), "debug.ico");
            //this.notifyIcon1.Icon = this.Icon;

            // DEBUG! Show settings on boot
            menuSettings_Click(null, null);
#endif
        }

        private string MilisecondsToString(uint ms)
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

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            var idle = UserInputChecker.GetIdleTime();
            ////lblSmall.Text = ((double)UserInputChecker.GetIdleTime() / 1000d).ToString("0");
            lblSmall.Text = MilisecondsToString(idle);

            // Only call Now once to avoid annoying bugs
            var now = DateTime.Now;

            if (idle > MinBrake)
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

                if (workingtime > MaxWorkTime && !inWarnState)
                {
                    BackColor = Color.Red;
                    lblInfo.Text = "Take a break!";
                    ShowMe();
                    TopMost = true;
                    inWarnState = true;
                    ////this.BringToFront();
                }
            }

            var foregroundApp = Pinvoke.GetForegroundAppName();
            var foregroundTitle = Pinvoke.GetForegroundWindowText();

            if (now.Subtract(lastSave).TotalSeconds > SaveInterval)
            {
                // Time to save
                var wasActive = idle < SaveInterval * 1000;
                ActivityManager.SaveActivity(now, SaveInterval, wasActive, foregroundApp, foregroundTitle);
                lastSave = now;
            }

            ////lblWork.Text = (workingtime / 1000d).ToString("0");
            lblWork.Text = MilisecondsToString(workingtime);
            lblDebug.Text = foregroundApp + " : " + foregroundTitle;
        }

        private void OnMainWindowLoad(object sender, EventArgs e)
        {
            timer1.Start();
            notifyIcon1.ShowBalloonTip(500, "Started", "ATray has started", ToolTipIcon.Info);
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
                if (historyForm != null)
                    historyForm.Close();
            }
        }

        private void menuHistory_Click(object sender, EventArgs e)
        {
            if (historyForm == null || historyForm.IsDisposed)
                historyForm = new ActivityHistoryForm();
            historyForm.Show();
        }

        private void menuSettings_Click(object sender, EventArgs e)
        {
            if (settingsForm == null || settingsForm.IsDisposed)
                settingsForm = new SettingsForm();
            settingsForm.Show();
        }
    }
}
