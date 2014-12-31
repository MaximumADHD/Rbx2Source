namespace Rbx2SourceLauncher
{
    partial class Launcher
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

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
        /// 

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            this.loadingStatus = new System.Windows.Forms.Label();
            this.loadingBar = new System.Windows.Forms.ProgressBar();
            this.logDisplay = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // loadingStatus
            // 
            this.loadingStatus.AutoSize = true;
            this.loadingStatus.BackColor = System.Drawing.Color.Transparent;
            this.loadingStatus.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadingStatus.ForeColor = System.Drawing.SystemColors.Control;
            this.loadingStatus.Location = new System.Drawing.Point(55, 123);
            this.loadingStatus.Name = "loadingStatus";
            this.loadingStatus.Size = new System.Drawing.Size(63, 19);
            this.loadingStatus.TabIndex = 0;
            this.loadingStatus.Text = "Loading";
            this.loadingStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // loadingBar
            // 
            this.loadingBar.Location = new System.Drawing.Point(59, 145);
            this.loadingBar.MarqueeAnimationSpeed = 30;
            this.loadingBar.Name = "loadingBar";
            this.loadingBar.Size = new System.Drawing.Size(166, 23);
            this.loadingBar.Step = 1;
            this.loadingBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.loadingBar.TabIndex = 1;
            // 
            // logDisplay
            // 
            this.logDisplay.AutoSize = true;
            this.logDisplay.BackColor = System.Drawing.Color.Transparent;
            this.logDisplay.ForeColor = System.Drawing.Color.Red;
            this.logDisplay.Location = new System.Drawing.Point(0, 266);
            this.logDisplay.Name = "logDisplay";
            this.logDisplay.Size = new System.Drawing.Size(0, 13);
            this.logDisplay.TabIndex = 2;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(284, 282);
            this.Controls.Add(this.logDisplay);
            this.Controls.Add(this.loadingBar);
            this.Controls.Add(this.loadingStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Launcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Launcher_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label loadingStatus;
        private System.Windows.Forms.ProgressBar loadingBar;
        private System.Windows.Forms.Label logDisplay;
    }
}

