using ATray.Dialogs;

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
            this.btnHistoryOk = new System.Windows.Forms.Button();
            this.historyPanel = new ATray.Dialogs.AutoScrollPanel();
            this.historyPicture = new System.Windows.Forms.PictureBox();
            this.lastMonthButton = new System.Windows.Forms.Button();
            this.monthDropDown = new System.Windows.Forms.ComboBox();
            this.nextMonthButton = new System.Windows.Forms.Button();
            this.computerDropDown = new System.Windows.Forms.ComboBox();
            this.showWork = new System.Windows.Forms.CheckBox();
            this.showBlurred = new System.Windows.Forms.CheckBox();
            this.blurAmount = new System.Windows.Forms.TrackBar();
            this.historyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blurAmount)).BeginInit();
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
            // computerDropDown
            // 
            this.computerDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.computerDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.computerDropDown.FormattingEnabled = true;
            this.computerDropDown.Location = new System.Drawing.Point(529, 4);
            this.computerDropDown.Margin = new System.Windows.Forms.Padding(2, 3, 2, 2);
            this.computerDropDown.Name = "computerDropDown";
            this.computerDropDown.Size = new System.Drawing.Size(102, 21);
            this.computerDropDown.TabIndex = 5;
            // 
            // showWork
            // 
            this.showWork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showWork.AutoSize = true;
            this.showWork.Location = new System.Drawing.Point(474, 6);
            this.showWork.Margin = new System.Windows.Forms.Padding(2);
            this.showWork.Name = "showWork";
            this.showWork.Size = new System.Drawing.Size(52, 17);
            this.showWork.TabIndex = 6;
            this.showWork.Text = "Work";
            this.showWork.UseVisualStyleBackColor = true;
            this.showWork.CheckedChanged += new System.EventHandler(this.OnShowWorkCheckboxChange);
            // 
            // showBlurred
            // 
            this.showBlurred.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showBlurred.AutoSize = true;
            this.showBlurred.BackColor = System.Drawing.Color.Transparent;
            this.showBlurred.Checked = true;
            this.showBlurred.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBlurred.Location = new System.Drawing.Point(432, 6);
            this.showBlurred.Margin = new System.Windows.Forms.Padding(2);
            this.showBlurred.Name = "showBlurred";
            this.showBlurred.Size = new System.Drawing.Size(44, 17);
            this.showBlurred.TabIndex = 7;
            this.showBlurred.Text = "Blur";
            this.showBlurred.UseVisualStyleBackColor = false;
            this.showBlurred.CheckedChanged += new System.EventHandler(this.OnBlurCheckboxChange);
            // 
            // blurAmount
            // 
            this.blurAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.blurAmount.AutoSize = false;
            this.blurAmount.LargeChange = 20;
            this.blurAmount.Location = new System.Drawing.Point(301, 4);
            this.blurAmount.Maximum = 100;
            this.blurAmount.Name = "blurAmount";
            this.blurAmount.Size = new System.Drawing.Size(126, 26);
            this.blurAmount.TabIndex = 8;
            this.blurAmount.TickStyle = System.Windows.Forms.TickStyle.None;
            this.blurAmount.Value = 35;
            // 
            // ActivityHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 468);
            this.Controls.Add(this.blurAmount);
            this.Controls.Add(this.showWork);
            this.Controls.Add(this.computerDropDown);
            this.Controls.Add(this.nextMonthButton);
            this.Controls.Add(this.monthDropDown);
            this.Controls.Add(this.lastMonthButton);
            this.Controls.Add(this.historyPanel);
            this.Controls.Add(this.btnHistoryOk);
            this.Controls.Add(this.showBlurred);
            this.Icon = global::ATray.Properties.Resources.main_icon;
            this.Name = "ActivityHistoryForm";
            this.Text = "Activity History";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ActivityHistoryForm_Paint);
            this.Resize += new System.EventHandler(this.ActivityHistoryForm_Resize);
            this.historyPanel.ResumeLayout(false);
            this.historyPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blurAmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnHistoryOk;
        private AutoScrollPanel historyPanel;
        private System.Windows.Forms.PictureBox historyPicture;
        private System.Windows.Forms.Button lastMonthButton;
        private System.Windows.Forms.ComboBox monthDropDown;
        private System.Windows.Forms.Button nextMonthButton;
        private System.Windows.Forms.ComboBox computerDropDown;
        private System.Windows.Forms.CheckBox showWork;
        private System.Windows.Forms.CheckBox showBlurred;
        private System.Windows.Forms.TrackBar blurAmount;
    }
}