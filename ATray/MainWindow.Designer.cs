namespace ATray
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mainTimer = new System.Windows.Forms.Timer(this.components);
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblSmall = new System.Windows.Forms.Label();
            this.lblWork = new System.Windows.Forms.Label();
            this.lblDebug = new System.Windows.Forms.Label();
            this.diskUsageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayMenu;
            this.trayIcon.Text = "Jocke tray";
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += new System.EventHandler(this.OnTrayIconDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diskUsageToolStripMenuItem,
            this.menuSettings,
            this.menuHistory,
            this.menuExit});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(245, 192);
            // 
            // menuSettings
            // 
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(244, 36);
            this.menuSettings.Text = "&Settings";
            this.menuSettings.Click += new System.EventHandler(this.OnMenuClickSettings);
            // 
            // menuHistory
            // 
            this.menuHistory.Name = "menuHistory";
            this.menuHistory.Size = new System.Drawing.Size(244, 36);
            this.menuHistory.Text = "Show &History";
            this.menuHistory.Click += new System.EventHandler(this.OnMenuClickHistory);
            // 
            // menuExit
            // 
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(244, 36);
            this.menuExit.Text = "&Exit";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // mainTimer
            // 
            this.mainTimer.Interval = 1000;
            this.mainTimer.Tick += new System.EventHandler(this.OnMainTimerTick);
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(24, 167);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(684, 60);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "Info";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSmall
            // 
            this.lblSmall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSmall.Location = new System.Drawing.Point(400, 348);
            this.lblSmall.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSmall.Name = "lblSmall";
            this.lblSmall.Size = new System.Drawing.Size(308, 44);
            this.lblSmall.TabIndex = 2;
            this.lblSmall.Text = "---";
            this.lblSmall.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // lblWork
            // 
            this.lblWork.AutoSize = true;
            this.lblWork.Location = new System.Drawing.Point(26, 365);
            this.lblWork.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblWork.Name = "lblWork";
            this.lblWork.Size = new System.Drawing.Size(33, 25);
            this.lblWork.TabIndex = 3;
            this.lblWork.Text = "---";
            // 
            // lblDebug
            // 
            this.lblDebug.AutoSize = true;
            this.lblDebug.Location = new System.Drawing.Point(24, 227);
            this.lblDebug.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDebug.Name = "lblDebug";
            this.lblDebug.Size = new System.Drawing.Size(0, 25);
            this.lblDebug.TabIndex = 4;
            // 
            // diskUsageToolStripMenuItem
            // 
            this.diskUsageToolStripMenuItem.Name = "diskUsageToolStripMenuItem";
            this.diskUsageToolStripMenuItem.Size = new System.Drawing.Size(244, 36);
            this.diskUsageToolStripMenuItem.Text = "Disk usage";
            this.diskUsageToolStripMenuItem.Click += new System.EventHandler(this.diskUsageToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 410);
            this.Controls.Add(this.lblDebug);
            this.Controls.Add(this.lblWork);
            this.Controls.Add(this.lblSmall);
            this.Controls.Add(this.lblInfo);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "MainWindow";
            this.Text = "A tray application";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.Load += new System.EventHandler(this.OnMainWindowLoad);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.Resize += new System.EventHandler(this.OnResize);
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.Timer mainTimer;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblSmall;
        private System.Windows.Forms.Label lblWork;
        private System.Windows.Forms.Label lblDebug;
        private System.Windows.Forms.ToolStripMenuItem menuHistory;
        private System.Windows.Forms.ToolStripMenuItem menuSettings;
        private System.Windows.Forms.ToolStripMenuItem diskUsageToolStripMenuItem;
    }
}

