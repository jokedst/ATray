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
            this.historyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // btnHistoryOk
            // 
            this.btnHistoryOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistoryOk.Location = new System.Drawing.Point(1096, 833);
            this.btnHistoryOk.Margin = new System.Windows.Forms.Padding(6);
            this.btnHistoryOk.Name = "btnHistoryOk";
            this.btnHistoryOk.Size = new System.Drawing.Size(150, 44);
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
            this.historyPanel.Location = new System.Drawing.Point(0, 62);
            this.historyPanel.Margin = new System.Windows.Forms.Padding(6);
            this.historyPanel.Name = "historyPanel";
            this.historyPanel.Size = new System.Drawing.Size(1270, 760);
            this.historyPanel.TabIndex = 1;
            // 
            // historyPicture
            // 
            this.historyPicture.Location = new System.Drawing.Point(0, 0);
            this.historyPicture.Margin = new System.Windows.Forms.Padding(6);
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
            this.lastMonthButton.Margin = new System.Windows.Forms.Padding(6);
            this.lastMonthButton.Name = "lastMonthButton";
            this.lastMonthButton.Size = new System.Drawing.Size(50, 50);
            this.lastMonthButton.TabIndex = 2;
            this.lastMonthButton.Text = "<";
            this.lastMonthButton.UseVisualStyleBackColor = true;
            this.lastMonthButton.Click += new System.EventHandler(this.lastMonthButton_Click);
            // 
            // monthDropDown
            // 
            this.monthDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.monthDropDown.FormattingEnabled = true;
            this.monthDropDown.Location = new System.Drawing.Point(62, 8);
            this.monthDropDown.Margin = new System.Windows.Forms.Padding(6);
            this.monthDropDown.Name = "monthDropDown";
            this.monthDropDown.Size = new System.Drawing.Size(338, 33);
            this.monthDropDown.TabIndex = 3;
            this.monthDropDown.SelectedIndexChanged += new System.EventHandler(this.monthDropDown_SelectedIndexChanged);
            // 
            // nextMonthButton
            // 
            this.nextMonthButton.Location = new System.Drawing.Point(416, 0);
            this.nextMonthButton.Margin = new System.Windows.Forms.Padding(6);
            this.nextMonthButton.Name = "nextMonthButton";
            this.nextMonthButton.Size = new System.Drawing.Size(50, 50);
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
            this.computerDropDown.Location = new System.Drawing.Point(1058, 8);
            this.computerDropDown.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.computerDropDown.Name = "computerDropDown";
            this.computerDropDown.Size = new System.Drawing.Size(200, 33);
            this.computerDropDown.TabIndex = 5;
            // 
            // showWork
            // 
            this.showWork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showWork.AutoSize = true;
            this.showWork.Location = new System.Drawing.Point(958, 12);
            this.showWork.Name = "showWork";
            this.showWork.Size = new System.Drawing.Size(94, 29);
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
            this.showBlurred.Location = new System.Drawing.Point(870, 12);
            this.showBlurred.Name = "showBlurred";
            this.showBlurred.Size = new System.Drawing.Size(82, 29);
            this.showBlurred.TabIndex = 7;
            this.showBlurred.Text = "Blur";
            this.showBlurred.UseVisualStyleBackColor = false;
            this.showBlurred.CheckedChanged += new System.EventHandler(this.OnBlurCheckboxChange);
            // 
            // ActivityHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1270, 900);
            this.Controls.Add(this.showWork);
            this.Controls.Add(this.computerDropDown);
            this.Controls.Add(this.nextMonthButton);
            this.Controls.Add(this.monthDropDown);
            this.Controls.Add(this.lastMonthButton);
            this.Controls.Add(this.historyPanel);
            this.Controls.Add(this.btnHistoryOk);
            this.Controls.Add(this.showBlurred);
            this.Icon = global::ATray.Properties.Resources.main_icon;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "ActivityHistoryForm";
            this.Text = "Activity History";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ActivityHistoryForm_Paint);
            this.Resize += new System.EventHandler(this.ActivityHistoryForm_Resize);
            this.historyPanel.ResumeLayout(false);
            this.historyPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyPicture)).EndInit();
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
    }
}