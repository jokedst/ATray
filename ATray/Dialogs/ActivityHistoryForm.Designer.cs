namespace ATray
{
    partial class ActivityHistoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivityHistoryForm));
            this.btnHistoryOk = new System.Windows.Forms.Button();
            this.historyPanel = new System.Windows.Forms.Panel();
            this.historyPicture = new System.Windows.Forms.PictureBox();
            this.lastMonthButton = new System.Windows.Forms.Button();
            this.monthDropDown = new System.Windows.Forms.ComboBox();
            this.nextMonthButton = new System.Windows.Forms.Button();
            this.historyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // btnHistoryOk
            // 
            this.btnHistoryOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistoryOk.Location = new System.Drawing.Point(548, 433);
            this.btnHistoryOk.Name = "btnHistoryOk";
            this.btnHistoryOk.Size = new System.Drawing.Size(75, 23);
            this.btnHistoryOk.TabIndex = 0;
            this.btnHistoryOk.Text = "&Ok";
            this.btnHistoryOk.UseVisualStyleBackColor = true;
            this.btnHistoryOk.Click += new System.EventHandler(this.btnHistoryOk_Click);
            // 
            // historyPanel
            // 
            this.historyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.historyPanel.AutoScroll = true;
            this.historyPanel.Controls.Add(this.historyPicture);
            this.historyPanel.Location = new System.Drawing.Point(0, 32);
            this.historyPanel.Name = "historyPanel";
            this.historyPanel.Size = new System.Drawing.Size(635, 395);
            this.historyPanel.TabIndex = 1;
            // 
            // historyPicture
            // 
            this.historyPicture.Location = new System.Drawing.Point(0, 0);
            this.historyPicture.Name = "historyPicture";
            this.historyPicture.Size = new System.Drawing.Size(100, 50);
            this.historyPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.historyPicture.TabIndex = 0;
            this.historyPicture.TabStop = false;
            this.historyPicture.MouseEnter += new System.EventHandler(this.historyPicture_MouseEnter);
            // 
            // lastMonthButton
            // 
            this.lastMonthButton.Location = new System.Drawing.Point(0, 0);
            this.lastMonthButton.Name = "lastMonthButton";
            this.lastMonthButton.Size = new System.Drawing.Size(25, 26);
            this.lastMonthButton.TabIndex = 2;
            this.lastMonthButton.Text = "<";
            this.lastMonthButton.UseVisualStyleBackColor = true;
            this.lastMonthButton.Click += new System.EventHandler(this.lastMonthButton_Click);
            // 
            // monthDropDown
            // 
            this.monthDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.monthDropDown.FormattingEnabled = true;
            this.monthDropDown.Location = new System.Drawing.Point(31, 4);
            this.monthDropDown.Name = "monthDropDown";
            this.monthDropDown.Size = new System.Drawing.Size(171, 21);
            this.monthDropDown.TabIndex = 3;
            this.monthDropDown.SelectedIndexChanged += new System.EventHandler(this.monthDropDown_SelectedIndexChanged);
            // 
            // nextMonthButton
            // 
            this.nextMonthButton.Location = new System.Drawing.Point(208, 0);
            this.nextMonthButton.Name = "nextMonthButton";
            this.nextMonthButton.Size = new System.Drawing.Size(25, 26);
            this.nextMonthButton.TabIndex = 4;
            this.nextMonthButton.Text = ">";
            this.nextMonthButton.UseVisualStyleBackColor = true;
            this.nextMonthButton.Click += new System.EventHandler(this.nextMonthButton_Click);
            // 
            // ActivityHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 468);
            this.Controls.Add(this.nextMonthButton);
            this.Controls.Add(this.monthDropDown);
            this.Controls.Add(this.lastMonthButton);
            this.Controls.Add(this.historyPanel);
            this.Controls.Add(this.btnHistoryOk);
            this.Icon = ATray.Properties.Resources.main_icon;
            this.Name = "ActivityHistoryForm";
            this.Text = "Activity History";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ActivityHistoryForm_Paint);
            this.Resize += new System.EventHandler(this.ActivityHistoryForm_Resize);
            this.historyPanel.ResumeLayout(false);
            this.historyPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnHistoryOk;
        private System.Windows.Forms.Panel historyPanel;
        private System.Windows.Forms.PictureBox historyPicture;
        private System.Windows.Forms.Button lastMonthButton;
        private System.Windows.Forms.ComboBox monthDropDown;
        private System.Windows.Forms.Button nextMonthButton;
    }
}