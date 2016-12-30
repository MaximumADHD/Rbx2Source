namespace Rbx2Source
{
    partial class Rbx2Source
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rbx2Source));
            this.About = new System.Windows.Forms.TabPage();
            this.nemsTools = new System.Windows.Forms.PictureBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.vtfCmdTitle = new System.Windows.Forms.Label();
            this.thirdPartyInfoElaboration = new System.Windows.Forms.Label();
            this.thirdPartyInfo = new System.Windows.Forms.Label();
            this.egoMooseContribution = new System.Windows.Forms.Label();
            this.egoMooseLink = new System.Windows.Forms.LinkLabel();
            this.AJContribution = new System.Windows.Forms.Label();
            this.AJLink = new System.Windows.Forms.LinkLabel();
            this.specialThanksTo = new System.Windows.Forms.Label();
            this.twitterLink = new System.Windows.Forms.LinkLabel();
            this.developedBy = new System.Windows.Forms.Label();
            this.egoMooseIcon = new System.Windows.Forms.PictureBox();
            this.AJIcon = new System.Windows.Forms.PictureBox();
            this.TwitterIcon = new System.Windows.Forms.PictureBox();
            this.rbx2SourceLogo = new System.Windows.Forms.PictureBox();
            this.Compiler = new System.Windows.Forms.TabPage();
            this.compilerTypeIcon = new System.Windows.Forms.PictureBox();
            this.gameIcon = new System.Windows.Forms.PictureBox();
            this.compileProgress = new System.Windows.Forms.ProgressBar();
            this.viewCompiledModel = new System.Windows.Forms.Button();
            this.outputHeader = new System.Windows.Forms.Label();
            this.output = new System.Windows.Forms.RichTextBox();
            this.compilerInputField = new System.Windows.Forms.TextBox();
            this.compile = new System.Windows.Forms.Button();
            this.compilerInput = new System.Windows.Forms.Label();
            this.gameSelect = new System.Windows.Forms.ComboBox();
            this.gameSelectLbl = new System.Windows.Forms.Label();
            this.assetPreview = new System.Windows.Forms.PictureBox();
            this.compilerType = new System.Windows.Forms.Label();
            this.compilerTypeSelect = new System.Windows.Forms.ComboBox();
            this.MainTab = new System.Windows.Forms.TabControl();
            this.About.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nemsTools)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.egoMooseIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AJIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TwitterIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rbx2SourceLogo)).BeginInit();
            this.Compiler.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.compilerTypeIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gameIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.assetPreview)).BeginInit();
            this.MainTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // About
            // 
            this.About.AutoScroll = true;
            this.About.Controls.Add(this.nemsTools);
            this.About.Controls.Add(this.textBox3);
            this.About.Controls.Add(this.vtfCmdTitle);
            this.About.Controls.Add(this.thirdPartyInfoElaboration);
            this.About.Controls.Add(this.thirdPartyInfo);
            this.About.Controls.Add(this.egoMooseContribution);
            this.About.Controls.Add(this.egoMooseLink);
            this.About.Controls.Add(this.AJContribution);
            this.About.Controls.Add(this.AJLink);
            this.About.Controls.Add(this.specialThanksTo);
            this.About.Controls.Add(this.twitterLink);
            this.About.Controls.Add(this.developedBy);
            this.About.Controls.Add(this.egoMooseIcon);
            this.About.Controls.Add(this.AJIcon);
            this.About.Controls.Add(this.TwitterIcon);
            this.About.Controls.Add(this.rbx2SourceLogo);
            this.About.Location = new System.Drawing.Point(4, 28);
            this.About.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.About.Name = "About";
            this.About.Size = new System.Drawing.Size(469, 342);
            this.About.TabIndex = 3;
            this.About.Text = "About";
            this.About.UseVisualStyleBackColor = true;
            // 
            // nemsTools
            // 
            this.nemsTools.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nemsTools.ImageLocation = "http://nemesis.thewavelength.net/images/site/title.png";
            this.nemsTools.InitialImage = global::Rbx2Source.Properties.Resources.Loading;
            this.nemsTools.Location = new System.Drawing.Point(321, 377);
            this.nemsTools.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.nemsTools.Name = "nemsTools";
            this.nemsTools.Size = new System.Drawing.Size(123, 104);
            this.nemsTools.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.nemsTools.TabIndex = 17;
            this.nemsTools.TabStop = false;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(12, 377);
            this.textBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(303, 106);
            this.textBox3.TabIndex = 16;
            this.textBox3.Text = "VTFCmd is a command-line based tool that allows you to convert standard image fil" +
    "es into the Valve Texture Format (.VTF)\r\nRbx2Source utilizes VTFCmd to handle mo" +
    "del textures.";
            // 
            // vtfCmdTitle
            // 
            this.vtfCmdTitle.AutoSize = true;
            this.vtfCmdTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.20472F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vtfCmdTitle.Location = new System.Drawing.Point(8, 348);
            this.vtfCmdTitle.Name = "vtfCmdTitle";
            this.vtfCmdTitle.Size = new System.Drawing.Size(88, 22);
            this.vtfCmdTitle.TabIndex = 15;
            this.vtfCmdTitle.Text = "VTFCmd";
            // 
            // thirdPartyInfoElaboration
            // 
            this.thirdPartyInfoElaboration.AutoSize = true;
            this.thirdPartyInfoElaboration.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.236221F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.thirdPartyInfoElaboration.Location = new System.Drawing.Point(8, 308);
            this.thirdPartyInfoElaboration.Name = "thirdPartyInfoElaboration";
            this.thirdPartyInfoElaboration.Size = new System.Drawing.Size(286, 30);
            this.thirdPartyInfoElaboration.TabIndex = 14;
            this.thirdPartyInfoElaboration.Text = "This program utilizes some third party applications. \r\nInformation regarding them" +
    " is listed below:";
            // 
            // thirdPartyInfo
            // 
            this.thirdPartyInfo.AutoSize = true;
            this.thirdPartyInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.77165F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.thirdPartyInfo.Location = new System.Drawing.Point(3, 283);
            this.thirdPartyInfo.Name = "thirdPartyInfo";
            this.thirdPartyInfo.Size = new System.Drawing.Size(198, 24);
            this.thirdPartyInfo.TabIndex = 13;
            this.thirdPartyInfo.Text = "Third-Party Information";
            // 
            // egoMooseContribution
            // 
            this.egoMooseContribution.AutoSize = true;
            this.egoMooseContribution.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.236221F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.egoMooseContribution.Location = new System.Drawing.Point(191, 238);
            this.egoMooseContribution.Name = "egoMooseContribution";
            this.egoMooseContribution.Size = new System.Drawing.Size(195, 45);
            this.egoMooseContribution.TabIndex = 11;
            this.egoMooseContribution.Text = "Rewrote ROBLOX\'s CFrame \r\ndatatype from scratch in C# for me.\r\n\r\n";
            // 
            // egoMooseLink
            // 
            this.egoMooseLink.AutoSize = true;
            this.egoMooseLink.Location = new System.Drawing.Point(191, 222);
            this.egoMooseLink.Name = "egoMooseLink";
            this.egoMooseLink.Size = new System.Drawing.Size(75, 17);
            this.egoMooseLink.TabIndex = 10;
            this.egoMooseLink.TabStop = true;
            this.egoMooseLink.Text = "EgoMoose";
            // 
            // AJContribution
            // 
            this.AJContribution.AutoSize = true;
            this.AJContribution.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.236221F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AJContribution.Location = new System.Drawing.Point(191, 183);
            this.AJContribution.Name = "AJContribution";
            this.AJContribution.Size = new System.Drawing.Size(177, 30);
            this.AJContribution.TabIndex = 8;
            this.AJContribution.Text = "Helped me reverse-engineer \r\nROBLOX\'s binary mesh format.";
            // 
            // AJLink
            // 
            this.AJLink.AutoSize = true;
            this.AJLink.Location = new System.Drawing.Point(191, 166);
            this.AJLink.Name = "AJLink";
            this.AJLink.Size = new System.Drawing.Size(171, 17);
            this.AJLink.TabIndex = 7;
            this.AJLink.TabStop = true;
            this.AJLink.Text = "RedInquisitive (AJ Walter)";
            // 
            // specialThanksTo
            // 
            this.specialThanksTo.AutoSize = true;
            this.specialThanksTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.77165F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.specialThanksTo.Location = new System.Drawing.Point(131, 143);
            this.specialThanksTo.Name = "specialThanksTo";
            this.specialThanksTo.Size = new System.Drawing.Size(156, 24);
            this.specialThanksTo.TabIndex = 5;
            this.specialThanksTo.Text = "Special thanks to:";
            // 
            // twitterLink
            // 
            this.twitterLink.AutoSize = true;
            this.twitterLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.twitterLink.Location = new System.Drawing.Point(9, 255);
            this.twitterLink.Name = "twitterLink";
            this.twitterLink.Size = new System.Drawing.Size(106, 17);
            this.twitterLink.TabIndex = 4;
            this.twitterLink.TabStop = true;
            this.twitterLink.Text = "@MaxGee1019";
            // 
            // developedBy
            // 
            this.developedBy.AutoSize = true;
            this.developedBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.20472F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.developedBy.Location = new System.Drawing.Point(7, 231);
            this.developedBy.Name = "developedBy";
            this.developedBy.Size = new System.Drawing.Size(125, 22);
            this.developedBy.TabIndex = 3;
            this.developedBy.Text = "Developed by:";
            // 
            // egoMooseIcon
            // 
            this.egoMooseIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.egoMooseIcon.ImageLocation = "https://github.com/EgoMoose.png";
            this.egoMooseIcon.InitialImage = global::Rbx2Source.Properties.Resources.Loading;
            this.egoMooseIcon.Location = new System.Drawing.Point(139, 222);
            this.egoMooseIcon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.egoMooseIcon.Name = "egoMooseIcon";
            this.egoMooseIcon.Size = new System.Drawing.Size(49, 50);
            this.egoMooseIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.egoMooseIcon.TabIndex = 9;
            this.egoMooseIcon.TabStop = false;
            // 
            // AJIcon
            // 
            this.AJIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.AJIcon.ImageLocation = "https://github.com/RedInquisitive.png";
            this.AJIcon.InitialImage = global::Rbx2Source.Properties.Resources.Loading;
            this.AJIcon.Location = new System.Drawing.Point(139, 166);
            this.AJIcon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AJIcon.Name = "AJIcon";
            this.AJIcon.Size = new System.Drawing.Size(49, 50);
            this.AJIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AJIcon.TabIndex = 6;
            this.AJIcon.TabStop = false;
            // 
            // TwitterIcon
            // 
            this.TwitterIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TwitterIcon.ImageLocation = "https://avatars.io/twitter/MaxGee1019/large";
            this.TwitterIcon.InitialImage = global::Rbx2Source.Properties.Resources.Loading;
            this.TwitterIcon.Location = new System.Drawing.Point(12, 145);
            this.TwitterIcon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TwitterIcon.Name = "TwitterIcon";
            this.TwitterIcon.Size = new System.Drawing.Size(93, 93);
            this.TwitterIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.TwitterIcon.TabIndex = 1;
            this.TwitterIcon.TabStop = false;
            // 
            // rbx2SourceLogo
            // 
            this.rbx2SourceLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbx2SourceLogo.Image = ((System.Drawing.Image)(resources.GetObject("rbx2SourceLogo.Image")));
            this.rbx2SourceLogo.Location = new System.Drawing.Point(0, 0);
            this.rbx2SourceLogo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbx2SourceLogo.Name = "rbx2SourceLogo";
            this.rbx2SourceLogo.Size = new System.Drawing.Size(447, 134);
            this.rbx2SourceLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.rbx2SourceLogo.TabIndex = 0;
            this.rbx2SourceLogo.TabStop = false;
            // 
            // Compiler
            // 
            this.Compiler.Controls.Add(this.compilerTypeIcon);
            this.Compiler.Controls.Add(this.gameIcon);
            this.Compiler.Controls.Add(this.compileProgress);
            this.Compiler.Controls.Add(this.viewCompiledModel);
            this.Compiler.Controls.Add(this.outputHeader);
            this.Compiler.Controls.Add(this.output);
            this.Compiler.Controls.Add(this.compilerInputField);
            this.Compiler.Controls.Add(this.compile);
            this.Compiler.Controls.Add(this.compilerInput);
            this.Compiler.Controls.Add(this.gameSelect);
            this.Compiler.Controls.Add(this.gameSelectLbl);
            this.Compiler.Controls.Add(this.assetPreview);
            this.Compiler.Controls.Add(this.compilerType);
            this.Compiler.Controls.Add(this.compilerTypeSelect);
            this.Compiler.Location = new System.Drawing.Point(4, 28);
            this.Compiler.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Compiler.Name = "Compiler";
            this.Compiler.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Compiler.Size = new System.Drawing.Size(469, 342);
            this.Compiler.TabIndex = 0;
            this.Compiler.Text = "Compiler";
            this.Compiler.UseVisualStyleBackColor = true;
            // 
            // compilerTypeIcon
            // 
            this.compilerTypeIcon.BackColor = System.Drawing.SystemColors.Control;
            this.compilerTypeIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.compilerTypeIcon.Location = new System.Drawing.Point(11, 70);
            this.compilerTypeIcon.Name = "compilerTypeIcon";
            this.compilerTypeIcon.Size = new System.Drawing.Size(24, 24);
            this.compilerTypeIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.compilerTypeIcon.TabIndex = 15;
            this.compilerTypeIcon.TabStop = false;
            // 
            // gameIcon
            // 
            this.gameIcon.BackColor = System.Drawing.SystemColors.Control;
            this.gameIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.gameIcon.Location = new System.Drawing.Point(11, 23);
            this.gameIcon.Name = "gameIcon";
            this.gameIcon.Size = new System.Drawing.Size(24, 24);
            this.gameIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.gameIcon.TabIndex = 14;
            this.gameIcon.TabStop = false;
            // 
            // compileProgress
            // 
            this.compileProgress.Location = new System.Drawing.Point(11, 289);
            this.compileProgress.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.compileProgress.Name = "compileProgress";
            this.compileProgress.Size = new System.Drawing.Size(289, 31);
            this.compileProgress.TabIndex = 13;
            // 
            // viewCompiledModel
            // 
            this.viewCompiledModel.Enabled = false;
            this.viewCompiledModel.Location = new System.Drawing.Point(306, 289);
            this.viewCompiledModel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.viewCompiledModel.Name = "viewCompiledModel";
            this.viewCompiledModel.Size = new System.Drawing.Size(155, 31);
            this.viewCompiledModel.TabIndex = 11;
            this.viewCompiledModel.Text = "View Compiled Model";
            this.viewCompiledModel.UseVisualStyleBackColor = true;
            this.viewCompiledModel.Click += new System.EventHandler(this.viewCompiledModel_Click);
            // 
            // outputHeader
            // 
            this.outputHeader.AutoSize = true;
            this.outputHeader.Location = new System.Drawing.Point(8, 97);
            this.outputHeader.Name = "outputHeader";
            this.outputHeader.Size = new System.Drawing.Size(114, 17);
            this.outputHeader.TabIndex = 10;
            this.outputHeader.Text = "Compiler Output:";
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(11, 117);
            this.output.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(450, 166);
            this.output.TabIndex = 9;
            this.output.Text = "";
            this.output.WordWrap = false;
            // 
            // compilerInputField
            // 
            this.compilerInputField.Location = new System.Drawing.Point(318, 25);
            this.compilerInputField.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.compilerInputField.Name = "compilerInputField";
            this.compilerInputField.Size = new System.Drawing.Size(143, 22);
            this.compilerInputField.TabIndex = 6;
            this.compilerInputField.Text = "CloneTrooper1019";
            this.compilerInputField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.compilerInputField_KeyDown);
            this.compilerInputField.Leave += new System.EventHandler(this.compilerInputField_Leave);
            // 
            // compile
            // 
            this.compile.Location = new System.Drawing.Point(318, 81);
            this.compile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.compile.Name = "compile";
            this.compile.Size = new System.Drawing.Size(143, 25);
            this.compile.TabIndex = 7;
            this.compile.Text = "Compile";
            this.compile.UseVisualStyleBackColor = true;
            this.compile.Click += new System.EventHandler(this.compile_Click);
            // 
            // compilerInput
            // 
            this.compilerInput.AutoSize = true;
            this.compilerInput.Location = new System.Drawing.Point(315, 2);
            this.compilerInput.Name = "compilerInput";
            this.compilerInput.Size = new System.Drawing.Size(77, 17);
            this.compilerInput.TabIndex = 5;
            this.compilerInput.Text = "Username:";
            // 
            // gameSelect
            // 
            this.gameSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gameSelect.Enabled = false;
            this.gameSelect.FormattingEnabled = true;
            this.gameSelect.Items.AddRange(new object[] {
            "Loading..."});
            this.gameSelect.Location = new System.Drawing.Point(41, 23);
            this.gameSelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gameSelect.Name = "gameSelect";
            this.gameSelect.Size = new System.Drawing.Size(161, 24);
            this.gameSelect.Sorted = true;
            this.gameSelect.TabIndex = 4;
            this.gameSelect.SelectedIndexChanged += new System.EventHandler(this.gameSelect_SelectedIndexChanged);
            // 
            // gameSelectLbl
            // 
            this.gameSelectLbl.AutoSize = true;
            this.gameSelectLbl.Location = new System.Drawing.Point(8, 2);
            this.gameSelectLbl.Name = "gameSelectLbl";
            this.gameSelectLbl.Size = new System.Drawing.Size(145, 17);
            this.gameSelectLbl.TabIndex = 3;
            this.gameSelectLbl.Text = "Game to Compile For:";
            // 
            // assetPreview
            // 
            this.assetPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.assetPreview.ErrorImage = null;
            this.assetPreview.Location = new System.Drawing.Point(208, 2);
            this.assetPreview.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.assetPreview.Name = "assetPreview";
            this.assetPreview.Size = new System.Drawing.Size(104, 104);
            this.assetPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.assetPreview.TabIndex = 2;
            this.assetPreview.TabStop = false;
            // 
            // compilerType
            // 
            this.compilerType.AutoSize = true;
            this.compilerType.Location = new System.Drawing.Point(8, 50);
            this.compilerType.Name = "compilerType";
            this.compilerType.Size = new System.Drawing.Size(103, 17);
            this.compilerType.TabIndex = 1;
            this.compilerType.Text = "Compiler Type:";
            // 
            // compilerTypeSelect
            // 
            this.compilerTypeSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.compilerTypeSelect.FormattingEnabled = true;
            this.compilerTypeSelect.Items.AddRange(new object[] {
            "Accessory/Gear",
            "Avatar"});
            this.compilerTypeSelect.Location = new System.Drawing.Point(41, 70);
            this.compilerTypeSelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.compilerTypeSelect.Name = "compilerTypeSelect";
            this.compilerTypeSelect.Size = new System.Drawing.Size(161, 24);
            this.compilerTypeSelect.Sorted = true;
            this.compilerTypeSelect.TabIndex = 0;
            this.compilerTypeSelect.SelectedIndexChanged += new System.EventHandler(this.compilerTypeSelect_SelectedIndexChanged);
            // 
            // MainTab
            // 
            this.MainTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.MainTab.Controls.Add(this.Compiler);
            this.MainTab.Controls.Add(this.About);
            this.MainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTab.Location = new System.Drawing.Point(0, 0);
            this.MainTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MainTab.Name = "MainTab";
            this.MainTab.SelectedIndex = 0;
            this.MainTab.Size = new System.Drawing.Size(477, 374);
            this.MainTab.TabIndex = 0;
            // 
            // Rbx2Source
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(477, 374);
            this.Controls.Add(this.MainTab);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "Rbx2Source";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rbx2Source v2.00";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Rbx2Source_FormClosed);
            this.Load += new System.EventHandler(this.Rbx2Source_Load);
            this.About.ResumeLayout(false);
            this.About.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nemsTools)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.egoMooseIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AJIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TwitterIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rbx2SourceLogo)).EndInit();
            this.Compiler.ResumeLayout(false);
            this.Compiler.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.compilerTypeIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gameIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.assetPreview)).EndInit();
            this.MainTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage About;
        private System.Windows.Forms.PictureBox nemsTools;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label vtfCmdTitle;
        private System.Windows.Forms.Label thirdPartyInfoElaboration;
        private System.Windows.Forms.Label thirdPartyInfo;
        private System.Windows.Forms.Label egoMooseContribution;
        private System.Windows.Forms.LinkLabel egoMooseLink;
        private System.Windows.Forms.Label AJContribution;
        private System.Windows.Forms.LinkLabel AJLink;
        private System.Windows.Forms.Label specialThanksTo;
        private System.Windows.Forms.LinkLabel twitterLink;
        private System.Windows.Forms.Label developedBy;
        private System.Windows.Forms.PictureBox egoMooseIcon;
        private System.Windows.Forms.PictureBox AJIcon;
        private System.Windows.Forms.PictureBox TwitterIcon;
        private System.Windows.Forms.PictureBox rbx2SourceLogo;
        private System.Windows.Forms.TabPage Compiler;
        private System.Windows.Forms.ProgressBar compileProgress;
        private System.Windows.Forms.Button viewCompiledModel;
        private System.Windows.Forms.Label outputHeader;
        private System.Windows.Forms.RichTextBox output;
        private System.Windows.Forms.TextBox compilerInputField;
        private System.Windows.Forms.Button compile;
        private System.Windows.Forms.Label compilerInput;
        public System.Windows.Forms.ComboBox gameSelect;
        private System.Windows.Forms.Label gameSelectLbl;
        private System.Windows.Forms.PictureBox assetPreview;
        private System.Windows.Forms.Label compilerType;
        public System.Windows.Forms.ComboBox compilerTypeSelect;
        private System.Windows.Forms.TabControl MainTab;
        private System.Windows.Forms.PictureBox gameIcon;
        private System.Windows.Forms.PictureBox compilerTypeIcon;


    }
}