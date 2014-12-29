namespace RobloxToSourceEngine
{
    partial class Compiler
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Compiler));
            this.ConsoleDisp = new System.Windows.Forms.ListBox();
            this.goToModel = new System.Windows.Forms.Button();
            this.goToViewer = new System.Windows.Forms.Button();
            this.assetDisplay = new System.Windows.Forms.PictureBox();
            this.returnToMenu = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.assetDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // ConsoleDisp
            // 
            this.ConsoleDisp.BackColor = System.Drawing.SystemColors.Window;
            this.ConsoleDisp.FormattingEnabled = true;
            this.ConsoleDisp.Location = new System.Drawing.Point(12, 12);
            this.ConsoleDisp.Name = "ConsoleDisp";
            this.ConsoleDisp.Size = new System.Drawing.Size(359, 251);
            this.ConsoleDisp.TabIndex = 1;
            this.ConsoleDisp.SelectedIndexChanged += new System.EventHandler(this.ConsoleDisp_SelectedIndexChanged);
            // 
            // goToModel
            // 
            this.goToModel.Enabled = false;
            this.goToModel.Location = new System.Drawing.Point(377, 143);
            this.goToModel.Name = "goToModel";
            this.goToModel.Size = new System.Drawing.Size(125, 36);
            this.goToModel.TabIndex = 4;
            this.goToModel.Text = "View Compiled Model in Model Viewer";
            this.goToModel.UseVisualStyleBackColor = true;
            this.goToModel.Click += new System.EventHandler(this.goToViewer_Click);
            // 
            // goToViewer
            // 
            this.goToViewer.Enabled = false;
            this.goToViewer.Location = new System.Drawing.Point(377, 185);
            this.goToViewer.Name = "goToViewer";
            this.goToViewer.Size = new System.Drawing.Size(125, 36);
            this.goToViewer.TabIndex = 5;
            this.goToViewer.Text = "Go to Compiled Model File";
            this.goToViewer.UseVisualStyleBackColor = true;
            this.goToViewer.Click += new System.EventHandler(this.goToModel_Click);
            // 
            // assetDisplay
            // 
            this.assetDisplay.BackColor = System.Drawing.Color.Transparent;
            this.assetDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.assetDisplay.ErrorImage = ((System.Drawing.Image)(resources.GetObject("assetDisplay.ErrorImage")));
            this.assetDisplay.ImageLocation = "http://www.roblox.com/Game/Tools/ThumbnailAsset.ashx?aid=1563352&fmt=png&wd=420&h" +
    "t=420";
            this.assetDisplay.Location = new System.Drawing.Point(377, 12);
            this.assetDisplay.Name = "assetDisplay";
            this.assetDisplay.Size = new System.Drawing.Size(125, 125);
            this.assetDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.assetDisplay.TabIndex = 11;
            this.assetDisplay.TabStop = false;
            // 
            // returnToMenu
            // 
            this.returnToMenu.Enabled = false;
            this.returnToMenu.Location = new System.Drawing.Point(377, 227);
            this.returnToMenu.Name = "returnToMenu";
            this.returnToMenu.Size = new System.Drawing.Size(125, 36);
            this.returnToMenu.TabIndex = 12;
            this.returnToMenu.Text = "Return to the Application Menu";
            this.returnToMenu.UseVisualStyleBackColor = true;
            this.returnToMenu.Click += new System.EventHandler(this.returnToMenu_Click);
            // 
            // Compiler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 275);
            this.ControlBox = false;
            this.Controls.Add(this.returnToMenu);
            this.Controls.Add(this.assetDisplay);
            this.Controls.Add(this.goToViewer);
            this.Controls.Add(this.goToModel);
            this.Controls.Add(this.ConsoleDisp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Compiler";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Roblox to Source Engine Compiler";
            this.Load += new System.EventHandler(this.Compiler_Load);
            ((System.ComponentModel.ISupportInitialize)(this.assetDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ConsoleDisp;
        private System.Windows.Forms.Button goToModel;
        private System.Windows.Forms.Button goToViewer;
        private System.Windows.Forms.PictureBox assetDisplay;
        private System.Windows.Forms.Button returnToMenu;
    }
}