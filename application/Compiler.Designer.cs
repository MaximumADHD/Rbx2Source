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
            this.assetDisplay = new System.Windows.Forms.PictureBox();
            this.returnToMenu = new System.Windows.Forms.Button();
            this.goToModel = new System.Windows.Forms.Button();
            this.recompileModel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.assetDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // ConsoleDisp
            // 
            this.ConsoleDisp.BackColor = System.Drawing.SystemColors.Window;
            this.ConsoleDisp.FormattingEnabled = true;
            this.ConsoleDisp.Location = new System.Drawing.Point(12, 12);
            this.ConsoleDisp.Name = "ConsoleDisp";
            this.ConsoleDisp.Size = new System.Drawing.Size(313, 212);
            this.ConsoleDisp.TabIndex = 1;
            this.ConsoleDisp.SelectedIndexChanged += new System.EventHandler(this.ConsoleDisp_SelectedIndexChanged);
            // 
            // assetDisplay
            // 
            this.assetDisplay.BackColor = System.Drawing.Color.Transparent;
            this.assetDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.assetDisplay.ErrorImage = ((System.Drawing.Image)(resources.GetObject("assetDisplay.ErrorImage")));
            this.assetDisplay.ImageLocation = "";
            this.assetDisplay.Location = new System.Drawing.Point(331, 12);
            this.assetDisplay.Name = "assetDisplay";
            this.assetDisplay.Size = new System.Drawing.Size(125, 125);
            this.assetDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.assetDisplay.TabIndex = 11;
            this.assetDisplay.TabStop = false;
            // 
            // returnToMenu
            // 
            this.returnToMenu.Enabled = false;
            this.returnToMenu.Location = new System.Drawing.Point(331, 203);
            this.returnToMenu.Name = "returnToMenu";
            this.returnToMenu.Size = new System.Drawing.Size(125, 24);
            this.returnToMenu.TabIndex = 12;
            this.returnToMenu.Text = "Return to the menu";
            this.returnToMenu.UseVisualStyleBackColor = true;
            this.returnToMenu.Click += new System.EventHandler(this.returnToMenu_Click);
            // 
            // goToModel
            // 
            this.goToModel.Enabled = false;
            this.goToModel.Location = new System.Drawing.Point(331, 143);
            this.goToModel.Name = "goToModel";
            this.goToModel.Size = new System.Drawing.Size(125, 24);
            this.goToModel.TabIndex = 4;
            this.goToModel.Text = "View Compiled Model";
            this.goToModel.UseVisualStyleBackColor = true;
            this.goToModel.Click += new System.EventHandler(this.goToViewer_Click);
            // 
            // recompileModel
            // 
            this.recompileModel.Enabled = false;
            this.recompileModel.Location = new System.Drawing.Point(331, 173);
            this.recompileModel.Name = "recompileModel";
            this.recompileModel.Size = new System.Drawing.Size(125, 24);
            this.recompileModel.TabIndex = 5;
            this.recompileModel.Text = "Recompile Model";
            this.recompileModel.UseVisualStyleBackColor = true;
            this.recompileModel.Click += new System.EventHandler(this.recompileModel_Click);
            // 
            // Compiler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 235);
            this.ControlBox = false;
            this.Controls.Add(this.returnToMenu);
            this.Controls.Add(this.assetDisplay);
            this.Controls.Add(this.recompileModel);
            this.Controls.Add(this.goToModel);
            this.Controls.Add(this.ConsoleDisp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Compiler";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Roblox to Source Engine Compiler";
            this.Load += new System.EventHandler(this.CompileModel);
            ((System.ComponentModel.ISupportInitialize)(this.assetDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ConsoleDisp;
        private System.Windows.Forms.PictureBox assetDisplay;
        private System.Windows.Forms.Button returnToMenu;
        private System.Windows.Forms.Button goToModel;
        private System.Windows.Forms.Button recompileModel;
    }
}