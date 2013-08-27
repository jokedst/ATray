namespace ATray
{
    using System;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        /// <summary>If user is away this long, a brake has been taken</summary>
        private const uint MinBrake = 60000; // 1 minute

        /// <summary>How long the user may keep working without taking a break</summary>
        private const uint MaxWorkTime = 20 * 60 * 1000; // 20 mins

        /// <summary>How long the user may keep working without taking a break</summary>
        private const uint SaveInterval = 60; // every minute

        private uint workingtime;

        private DateTime startTime = DateTime.Now;

        private bool inWarnState;

        private bool reallyClose;

        private DateTime lastSave = DateTime.MinValue;

        private ActivityHistoryForm historyForm;

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
#if DEBUG
            this.Icon = new Icon(GetType(), "debug.ico");
            this.notifyIcon1.Icon = this.Icon;

            // DEBUG! Show history on boot
            menuHistory_Click(null, null);
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

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.inWarnState) this.ShowMe();
            else if (FormWindowState.Minimized == WindowState) Hide();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.ShowMe();
        }

        private void ShowMe()
        {
            Show();
            WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            this.reallyClose = true;
            this.Close();
            ////Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var idle = UserInputChecker.GetIdleTime();
            ////lblSmall.Text = ((double)UserInputChecker.GetIdleTime() / 1000d).ToString("0");
            lblSmall.Text = this.MilisecondsToString(idle);

            // Only call Now once to avoid annoying bugs
            var now = DateTime.Now;

            if (idle > MinBrake)
            {
                this.BackColor = Color.Green;
                lblInfo.Text = "You can start working now";
                this.workingtime = 0;
                this.startTime = now;
                this.TopMost = false;
                this.inWarnState = false;
            }
            else
            {
                this.workingtime += (uint)now.Subtract(this.startTime).TotalMilliseconds;
                this.startTime = now;

                if (this.workingtime > MaxWorkTime && !this.inWarnState)
                {
                    this.BackColor = Color.Red;
                    lblInfo.Text = "Take a break!";
                    this.ShowMe();
                    this.TopMost = true;
                    this.inWarnState = true;
                    ////this.BringToFront();
                }
            }

            if (now.Subtract(lastSave).TotalSeconds > SaveInterval)
            {
                // Time to save
                var wasActive = idle < SaveInterval * 1000;
                ActivityManager.SaveActivity(now, SaveInterval, wasActive);
                lastSave = now;
            }

            ////lblWork.Text = (workingtime / 1000d).ToString("0");
            this.lblWork.Text = this.MilisecondsToString(this.workingtime);
            lblDebug.Text = Pinvoke.GetForegroundWindowText();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.inWarnState)
                this.Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Don't close the program, just minimize it (unless they used the menu)
            if (e.CloseReason == CloseReason.UserClosing && !this.reallyClose)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
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
    }
}
