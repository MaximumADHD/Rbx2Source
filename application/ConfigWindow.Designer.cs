namespace RobloxToSourceEngine
{
    partial class ConfigWindow
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
            this.gameList = new System.Windows.Forms.ComboBox();
            this.addButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.inputStudioMDL = new System.Windows.Forms.TextBox();
            this.studiomdlSearch = new System.Windows.Forms.Button();
            this.studiomdltitle = new System.Windows.Forms.Label();
            this.inputGameInfo = new System.Windows.Forms.TextBox();
            this.gameinfotitle = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.doneButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // gameList
            // 
            this.gameList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gameList.Enabled = false;
            this.gameList.FormattingEnabled = true;
            this.gameList.Location = new System.Drawing.Point(12, 14);
            this.gameList.Name = "gameList";
            this.gameList.Size = new System.Drawing.Size(121, 21);
            this.gameList.Sorted = true;
            this.gameList.TabIndex = 0;
            this.gameList.SelectedIndexChanged += new System.EventHandler(this.updateGameList);
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(139, 14);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(46, 23);
            this.addButton.TabIndex = 1;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Enabled = false;
            this.removeButton.Location = new System.Drawing.Point(191, 14);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(55, 23);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // inputStudioMDL
            // 
            this.inputStudioMDL.Location = new System.Drawing.Point(12, 103);
            this.inputStudioMDL.Name = "inputStudioMDL";
            this.inputStudioMDL.ReadOnly = true;
            this.inputStudioMDL.Size = new System.Drawing.Size(173, 20);
            this.inputStudioMDL.TabIndex = 3;
            this.inputStudioMDL.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // studiomdlSearch
            // 
            this.studiomdlSearch.Enabled = false;
            this.studiomdlSearch.Location = new System.Drawing.Point(191, 103);
            this.studiomdlSearch.Name = "studiomdlSearch";
            this.studiomdlSearch.Size = new System.Drawing.Size(30, 20);
            this.studiomdlSearch.TabIndex = 4;
            this.studiomdlSearch.Text = "...";
            this.studiomdlSearch.UseVisualStyleBackColor = true;
            this.studiomdlSearch.Click += new System.EventHandler(this.studiomdlSearch_Click);
            // 
            // studiomdltitle
            // 
            this.studiomdltitle.AutoSize = true;
            this.studiomdltitle.Location = new System.Drawing.Point(12, 87);
            this.studiomdltitle.Name = "studiomdltitle";
            this.studiomdltitle.Size = new System.Drawing.Size(74, 13);
            this.studiomdltitle.TabIndex = 5;
            this.studiomdltitle.Text = "studiomdl.exe:";
            // 
            // inputGameInfo
            // 
            this.inputGameInfo.Location = new System.Drawing.Point(12, 64);
            this.inputGameInfo.Name = "inputGameInfo";
            this.inputGameInfo.ReadOnly = true;
            this.inputGameInfo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.inputGameInfo.Size = new System.Drawing.Size(209, 20);
            this.inputGameInfo.TabIndex = 6;
            this.inputGameInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // gameinfotitle
            // 
            this.gameinfotitle.AutoSize = true;
            this.gameinfotitle.Location = new System.Drawing.Point(12, 48);
            this.gameinfotitle.Name = "gameinfotitle";
            this.gameinfotitle.Size = new System.Drawing.Size(67, 13);
            this.gameinfotitle.TabIndex = 7;
            this.gameinfotitle.Text = "gameinfo.txt:";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "gameinfo.txt|gameinfo.txt";
            // 
            // doneButton
            // 
            this.doneButton.Location = new System.Drawing.Point(12, 129);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(74, 20);
            this.doneButton.TabIndex = 12;
            this.doneButton.Text = "Done";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // ConfigWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(251, 161);
            this.ControlBox = false;
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.gameinfotitle);
            this.Controls.Add(this.inputGameInfo);
            this.Controls.Add(this.studiomdltitle);
            this.Controls.Add(this.studiomdlSearch);
            this.Controls.Add(this.inputStudioMDL);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.gameList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ConfigWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Game Configuration";
            this.VisibleChanged += new System.EventHandler(this.updateGameList);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox gameList;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.TextBox inputStudioMDL;
        private System.Windows.Forms.Button studiomdlSearch;
        private System.Windows.Forms.Label studiomdltitle;
        private System.Windows.Forms.TextBox inputGameInfo;
        private System.Windows.Forms.Label gameinfotitle;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button doneButton;
    }
}