using System;

namespace RobloxToSourceEngine
{
    partial class Rbx
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rbx));
            this.gameList = new System.Windows.Forms.ComboBox();
            this.gameConfigTitle = new System.Windows.Forms.Label();
            this.configButton = new System.Windows.Forms.Button();
            this.compileCharacter = new System.Windows.Forms.Label();
            this.inputAssetID = new System.Windows.Forms.TextBox();
            this.inputUsername = new System.Windows.Forms.TextBox();
            this.assetDisplay = new System.Windows.Forms.PictureBox();
            this.toggleAssetId = new System.Windows.Forms.Button();
            this.toggleUserId = new System.Windows.Forms.Button();
            this.debugMenuStrip = new System.Windows.Forms.MenuStrip();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileAsset = new System.Windows.Forms.Label();
            this.enterAssetId = new System.Windows.Forms.Button();
            this.enterUsername = new System.Windows.Forms.Button();
            this.version = new System.Windows.Forms.Label();
            this.compile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.assetDisplay)).BeginInit();
            this.debugMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // gameList
            // 
            this.gameList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gameList.Enabled = false;
            this.gameList.FormattingEnabled = true;
            this.gameList.Items.AddRange(new object[] {
            "No Games Added!"});
            this.gameList.Location = new System.Drawing.Point(6, 174);
            this.gameList.Name = "gameList";
            this.gameList.Size = new System.Drawing.Size(109, 21);
            this.gameList.TabIndex = 2;
            // 
            // gameConfigTitle
            // 
            this.gameConfigTitle.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.gameConfigTitle.AutoSize = true;
            this.gameConfigTitle.BackColor = System.Drawing.Color.Transparent;
            this.gameConfigTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameConfigTitle.ForeColor = System.Drawing.Color.White;
            this.gameConfigTitle.Location = new System.Drawing.Point(3, 158);
            this.gameConfigTitle.Name = "gameConfigTitle";
            this.gameConfigTitle.Size = new System.Drawing.Size(145, 13);
            this.gameConfigTitle.TabIndex = 3;
            this.gameConfigTitle.Text = "Source Game to Compile For:";
            // 
            // configButton
            // 
            this.configButton.Location = new System.Drawing.Point(121, 174);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(63, 21);
            this.configButton.TabIndex = 4;
            this.configButton.Text = "Edit";
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.configButton_Click);
            // 
            // compileCharacter
            // 
            this.compileCharacter.AutoSize = true;
            this.compileCharacter.BackColor = System.Drawing.Color.Transparent;
            this.compileCharacter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compileCharacter.ForeColor = System.Drawing.Color.White;
            this.compileCharacter.Location = new System.Drawing.Point(3, 238);
            this.compileCharacter.Name = "compileCharacter";
            this.compileCharacter.Size = new System.Drawing.Size(132, 13);
            this.compileCharacter.TabIndex = 7;
            this.compileCharacter.Text = "Compile Roblox Character:";
            // 
            // inputAssetID
            // 
            this.inputAssetID.Enabled = false;
            this.inputAssetID.Location = new System.Drawing.Point(6, 215);
            this.inputAssetID.Name = "inputAssetID";
            this.inputAssetID.Size = new System.Drawing.Size(84, 20);
            this.inputAssetID.TabIndex = 8;
            this.inputAssetID.Text = "19027209";
            this.inputAssetID.TextChanged += new System.EventHandler(this.inputAssetID_TextChanged);
            // 
            // inputUsername
            // 
            this.inputUsername.Enabled = false;
            this.inputUsername.Location = new System.Drawing.Point(6, 254);
            this.inputUsername.Name = "inputUsername";
            this.inputUsername.Size = new System.Drawing.Size(84, 20);
            this.inputUsername.TabIndex = 9;
            this.inputUsername.Text = "Shedletsky";
            // 
            // assetDisplay
            // 
            this.assetDisplay.BackColor = System.Drawing.Color.Transparent;
            this.assetDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.assetDisplay.ErrorImage = ((System.Drawing.Image)(resources.GetObject("assetDisplay.ErrorImage")));
            this.assetDisplay.ImageLocation = "http://www.roblox.com/Game/Tools/ThumbnailAsset.ashx?aid=19027209&fmt=png&wd=420&" +
    "ht=420";
            this.assetDisplay.Location = new System.Drawing.Point(190, 158);
            this.assetDisplay.Name = "assetDisplay";
            this.assetDisplay.Size = new System.Drawing.Size(91, 93);
            this.assetDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.assetDisplay.TabIndex = 10;
            this.assetDisplay.TabStop = false;
            // 
            // toggleAssetId
            // 
            this.toggleAssetId.Enabled = false;
            this.toggleAssetId.Location = new System.Drawing.Point(121, 215);
            this.toggleAssetId.Name = "toggleAssetId";
            this.toggleAssetId.Size = new System.Drawing.Size(63, 20);
            this.toggleAssetId.TabIndex = 11;
            this.toggleAssetId.Text = "Use";
            this.toggleAssetId.UseVisualStyleBackColor = true;
            this.toggleAssetId.Click += new System.EventHandler(this.toggleAssetId_Click);
            // 
            // toggleUserId
            // 
            this.toggleUserId.Enabled = false;
            this.toggleUserId.Location = new System.Drawing.Point(121, 254);
            this.toggleUserId.Name = "toggleUserId";
            this.toggleUserId.Size = new System.Drawing.Size(63, 20);
            this.toggleUserId.TabIndex = 12;
            this.toggleUserId.Text = "Use";
            this.toggleUserId.UseVisualStyleBackColor = true;
            this.toggleUserId.Click += new System.EventHandler(this.toggleUserId_Click);
            // 
            // debugMenuStrip
            // 
            this.debugMenuStrip.BackColor = System.Drawing.Color.Transparent;
            this.debugMenuStrip.Dock = System.Windows.Forms.DockStyle.Right;
            this.debugMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugToolStripMenuItem});
            this.debugMenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.debugMenuStrip.Location = new System.Drawing.Point(70, 0);
            this.debugMenuStrip.Name = "debugMenuStrip";
            this.debugMenuStrip.Size = new System.Drawing.Size(223, 313);
            this.debugMenuStrip.TabIndex = 14;
            this.debugMenuStrip.Text = "debugMenuStrip";
            this.debugMenuStrip.Visible = false;
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.debugToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(125, 19);
            this.debugToolStripMenuItem.Text = "DEBUG RESET DATA";
            this.debugToolStripMenuItem.Click += new System.EventHandler(this.debugToolStripMenuItem_Click);
            // 
            // compileAsset
            // 
            this.compileAsset.AutoSize = true;
            this.compileAsset.BackColor = System.Drawing.Color.Transparent;
            this.compileAsset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compileAsset.ForeColor = System.Drawing.Color.White;
            this.compileAsset.Location = new System.Drawing.Point(3, 198);
            this.compileAsset.Name = "compileAsset";
            this.compileAsset.Size = new System.Drawing.Size(95, 13);
            this.compileAsset.TabIndex = 6;
            this.compileAsset.Text = "Compile Hat/Gear:";
            // 
            // enterAssetId
            // 
            this.enterAssetId.Enabled = false;
            this.enterAssetId.Location = new System.Drawing.Point(96, 215);
            this.enterAssetId.Name = "enterAssetId";
            this.enterAssetId.Size = new System.Drawing.Size(19, 20);
            this.enterAssetId.TabIndex = 15;
            this.enterAssetId.Text = ">";
            this.enterAssetId.UseVisualStyleBackColor = true;
            this.enterAssetId.Click += new System.EventHandler(this.enterAssetId_Click);
            // 
            // enterUsername
            // 
            this.enterUsername.Enabled = false;
            this.enterUsername.Location = new System.Drawing.Point(96, 253);
            this.enterUsername.Name = "enterUsername";
            this.enterUsername.Size = new System.Drawing.Size(19, 20);
            this.enterUsername.TabIndex = 15;
            this.enterUsername.Text = ">";
            this.enterUsername.UseVisualStyleBackColor = true;
            this.enterUsername.Click += new System.EventHandler(this.enterUsername_Click);
            // 
            // version
            // 
            this.version.AutoSize = true;
            this.version.BackColor = System.Drawing.Color.Transparent;
            this.version.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.version.ForeColor = System.Drawing.Color.White;
            this.version.Location = new System.Drawing.Point(93, 278);
            this.version.Name = "version";
            this.version.Size = new System.Drawing.Size(195, 26);
            this.version.TabIndex = 16;
            this.version.Text = "Version 1.1\r\n@ CloneTrooper1019, 2014-2015";
            this.version.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // compile
            // 
            this.compile.Enabled = false;
            this.compile.Location = new System.Drawing.Point(190, 254);
            this.compile.Name = "compile";
            this.compile.Size = new System.Drawing.Size(91, 20);
            this.compile.TabIndex = 17;
            this.compile.Text = "Compile";
            this.compile.UseVisualStyleBackColor = true;
            this.compile.Click += new System.EventHandler(this.compile_Click);
            // 
            // Rbx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(293, 313);
            this.Controls.Add(this.compile);
            this.Controls.Add(this.version);
            this.Controls.Add(this.enterUsername);
            this.Controls.Add(this.enterAssetId);
            this.Controls.Add(this.toggleUserId);
            this.Controls.Add(this.toggleAssetId);
            this.Controls.Add(this.assetDisplay);
            this.Controls.Add(this.inputUsername);
            this.Controls.Add(this.inputAssetID);
            this.Controls.Add(this.compileCharacter);
            this.Controls.Add(this.compileAsset);
            this.Controls.Add(this.configButton);
            this.Controls.Add(this.gameConfigTitle);
            this.Controls.Add(this.gameList);
            this.Controls.Add(this.debugMenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.debugMenuStrip;
            this.MaximizeBox = false;
            this.Name = "Rbx";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rbx to Src Converter Tool";
            ((System.ComponentModel.ISupportInitialize)(this.assetDisplay)).EndInit();
            this.debugMenuStrip.ResumeLayout(false);
            this.debugMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox gameList;
        private System.Windows.Forms.Label gameConfigTitle;
        private System.Windows.Forms.Button configButton;
        private System.Windows.Forms.Label compileCharacter;
        private System.Windows.Forms.TextBox inputAssetID;
        private System.Windows.Forms.TextBox inputUsername;
        private System.Windows.Forms.PictureBox assetDisplay;
        private System.Windows.Forms.Button toggleAssetId;
        private System.Windows.Forms.Button toggleUserId;
        private System.Windows.Forms.MenuStrip debugMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.Label compileAsset;
        private System.Windows.Forms.Button enterAssetId;
        private System.Windows.Forms.Button enterUsername;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Button compile;
    }
}

