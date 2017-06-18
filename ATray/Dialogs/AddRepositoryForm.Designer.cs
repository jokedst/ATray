namespace ATray
{
    partial class AddRepositoryForm
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
            this.textboxPath = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonSelectPath = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.scheduleTrackBar = new System.Windows.Forms.TrackBar();
            this.scheduleLabel = new System.Windows.Forms.Label();
            this.validationResultLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.scheduleTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // textboxPath
            // 
            this.textboxPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxPath.Location = new System.Drawing.Point(12, 29);
            this.textboxPath.Name = "textboxPath";
            this.textboxPath.Size = new System.Drawing.Size(229, 20);
            this.textboxPath.TabIndex = 0;
            this.textboxPath.TextChanged += new System.EventHandler(this.textboxPath_TextChanged);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectPath.Location = new System.Drawing.Point(247, 29);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(25, 20);
            this.buttonSelectPath.TabIndex = 1;
            this.buttonSelectPath.Text = "...";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.buttonSelectPath_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Repository location";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(197, 199);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "&Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button2.Location = new System.Drawing.Point(116, 199);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "&Add";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // buttonTest
            // 
            this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTest.Enabled = false;
            this.buttonTest.Location = new System.Drawing.Point(13, 199);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 5;
            this.buttonTest.Text = "&Validate...";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.OnClickValidate);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Update Schedule";
            // 
            // scheduleTrackBar
            // 
            this.scheduleTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scheduleTrackBar.Location = new System.Drawing.Point(13, 68);
            this.scheduleTrackBar.Maximum = 4;
            this.scheduleTrackBar.Name = "scheduleTrackBar";
            this.scheduleTrackBar.Size = new System.Drawing.Size(260, 45);
            this.scheduleTrackBar.TabIndex = 7;
            this.scheduleTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.scheduleTrackBar.Scroll += new System.EventHandler(this.scheduleTrackBar_Scroll);
            // 
            // scheduleLabel
            // 
            this.scheduleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scheduleLabel.Location = new System.Drawing.Point(12, 116);
            this.scheduleLabel.Name = "scheduleLabel";
            this.scheduleLabel.Size = new System.Drawing.Size(260, 23);
            this.scheduleLabel.TabIndex = 8;
            this.scheduleLabel.Text = "Never";
            this.scheduleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // validationResultLabel
            // 
            this.validationResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.validationResultLabel.AutoSize = true;
            this.validationResultLabel.Location = new System.Drawing.Point(13, 180);
            this.validationResultLabel.Name = "validationResultLabel";
            this.validationResultLabel.Size = new System.Drawing.Size(0, 13);
            this.validationResultLabel.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Name";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NameTextBox.Location = new System.Drawing.Point(12, 143);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(260, 20);
            this.NameTextBox.TabIndex = 11;
            // 
            // AddRepositoryForm
            // 
            this.AcceptButton = this.button2;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(284, 234);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.validationResultLabel);
            this.Controls.Add(this.scheduleLabel);
            this.Controls.Add(this.scheduleTrackBar);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonSelectPath);
            this.Controls.Add(this.textboxPath);
            this.Name = "AddRepositoryForm";
            this.Text = "Add Repository";
            ((System.ComponentModel.ISupportInitialize)(this.scheduleTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button buttonSelectPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttonTest;
        public System.Windows.Forms.TextBox textboxPath;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TrackBar scheduleTrackBar;
        private System.Windows.Forms.Label scheduleLabel;
        private System.Windows.Forms.Label validationResultLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NameTextBox;
    }
}