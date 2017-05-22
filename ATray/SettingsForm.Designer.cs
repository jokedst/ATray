namespace ATray
{
    partial class SettingsForm
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
            this.repoList = new System.Windows.Forms.ListView();
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnLastUpdated = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSchedule = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.repoListMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tabRepoMan = new System.Windows.Forms.TabPage();
            this.buttonAddRepository = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.editRepoMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repoListMenu.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabRepoMan.SuspendLayout();
            this.editRepoMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // repoList
            // 
            this.repoList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.repoList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columnStatus,
            this.columnLastUpdated,
            this.columnSchedule,
            this.columnPath});
            this.repoList.FullRowSelect = true;
            this.repoList.Location = new System.Drawing.Point(6, 23);
            this.repoList.Name = "repoList";
            this.repoList.Size = new System.Drawing.Size(410, 227);
            this.repoList.TabIndex = 0;
            this.repoList.UseCompatibleStateImageBehavior = false;
            this.repoList.View = System.Windows.Forms.View.Details;
            this.repoList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.repoList_MouseDown);
            // 
            // columnName
            // 
            this.columnName.Text = "Name";
            this.columnName.Width = 127;
            // 
            // columnStatus
            // 
            this.columnStatus.Text = "Status";
            // 
            // columnLastUpdated
            // 
            this.columnLastUpdated.DisplayIndex = 3;
            this.columnLastUpdated.Text = "Last Updated";
            this.columnLastUpdated.Width = 79;
            // 
            // columnSchedule
            // 
            this.columnSchedule.DisplayIndex = 4;
            this.columnSchedule.Text = "Schedule";
            // 
            // columnPath
            // 
            this.columnPath.DisplayIndex = 2;
            this.columnPath.Text = "Path";
            // 
            // repoListMenu
            // 
            this.repoListMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem});
            this.repoListMenu.Name = "repoListMenu";
            this.repoListMenu.Size = new System.Drawing.Size(97, 26);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.addToolStripMenuItem.Text = "&Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabRepoMan);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(430, 311);
            this.tabControl.TabIndex = 1;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(422, 285);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // tabRepoMan
            // 
            this.tabRepoMan.Controls.Add(this.buttonAddRepository);
            this.tabRepoMan.Controls.Add(this.label1);
            this.tabRepoMan.Controls.Add(this.repoList);
            this.tabRepoMan.Location = new System.Drawing.Point(4, 22);
            this.tabRepoMan.Name = "tabRepoMan";
            this.tabRepoMan.Padding = new System.Windows.Forms.Padding(3);
            this.tabRepoMan.Size = new System.Drawing.Size(422, 285);
            this.tabRepoMan.TabIndex = 1;
            this.tabRepoMan.Text = "RepoMan";
            this.tabRepoMan.UseVisualStyleBackColor = true;
            // 
            // buttonAddRepository
            // 
            this.buttonAddRepository.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddRepository.Location = new System.Drawing.Point(4, 256);
            this.buttonAddRepository.Name = "buttonAddRepository";
            this.buttonAddRepository.Size = new System.Drawing.Size(75, 23);
            this.buttonAddRepository.TabIndex = 2;
            this.buttonAddRepository.Text = "&Add";
            this.buttonAddRepository.UseVisualStyleBackColor = true;
            this.buttonAddRepository.Click += new System.EventHandler(this.buttonAddRepository_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Repositories";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(286, 329);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "&Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(367, 329);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // editRepoMenu
            // 
            this.editRepoMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateNowToolStripMenuItem,
            this.editToolStripMenuItem});
            this.editRepoMenu.Name = "editRepoMenu";
            this.editRepoMenu.Size = new System.Drawing.Size(153, 70);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.editToolStripMenuItem.Text = "&Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.OnClickEdit);
            // 
            // updateNowToolStripMenuItem
            // 
            this.updateNowToolStripMenuItem.Name = "updateNowToolStripMenuItem";
            this.updateNowToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.updateNowToolStripMenuItem.Text = "&Update now";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 364);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tabControl);
            this.Icon = global::ATray.Properties.Resources.main_icon;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.repoListMenu.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabRepoMan.ResumeLayout(false);
            this.tabRepoMan.PerformLayout();
            this.editRepoMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView repoList;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabRepoMan;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnStatus;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenuStrip repoListMenu;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip editRepoMenu;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnPath;
        private System.Windows.Forms.Button buttonAddRepository;
        private System.Windows.Forms.ColumnHeader columnLastUpdated;
        private System.Windows.Forms.ColumnHeader columnSchedule;
        private System.Windows.Forms.ToolStripMenuItem updateNowToolStripMenuItem;
    }
}