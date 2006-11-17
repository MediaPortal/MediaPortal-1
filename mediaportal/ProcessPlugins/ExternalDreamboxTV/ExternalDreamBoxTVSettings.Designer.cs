namespace ProcessPlugins.ExternalDreamboxTV
{
    partial class ExternalDreamBoxTVSettings
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
            this.mpTabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnSaveSettings = new MediaPortal.UserInterface.Controls.MPButton();
            this.edtDreamPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtDreamUserName = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtDreamIP = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.progLabel = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lblProgressText = new System.Windows.Forms.Label();
            this.btnLoadChannels = new System.Windows.Forms.Button();
            this.progChannels = new System.Windows.Forms.ProgressBar();
            this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtChannel = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtTotalChannels = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtBoutique = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtTotalBoutiques = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.btnEPG = new MediaPortal.UserInterface.Controls.MPButton();
            this.btnClose = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.edtSyncHours = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
            this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtSaveEPGSyncSettings = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.mpGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.mpGroupBox1.SuspendLayout();
            this.mpGroupBox2.SuspendLayout();
            this.mpGroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.edtSyncHours)).BeginInit();
            this.SuspendLayout();
            // 
            // mpTabControl1
            // 
            this.mpTabControl1.Controls.Add(this.tabPage1);
            this.mpTabControl1.Controls.Add(this.tabPage2);
            this.mpTabControl1.Location = new System.Drawing.Point(13, 4);
            this.mpTabControl1.Name = "mpTabControl1";
            this.mpTabControl1.SelectedIndex = 0;
            this.mpTabControl1.Size = new System.Drawing.Size(527, 333);
            this.mpTabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.mpGroupBox3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(519, 307);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Dreambox Settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // mpGroupBox3
            // 
            this.mpGroupBox3.Controls.Add(this.pictureBox1);
            this.mpGroupBox3.Controls.Add(this.btnSaveSettings);
            this.mpGroupBox3.Controls.Add(this.edtDreamPassword);
            this.mpGroupBox3.Controls.Add(this.mpLabel7);
            this.mpGroupBox3.Controls.Add(this.edtDreamUserName);
            this.mpGroupBox3.Controls.Add(this.mpLabel6);
            this.mpGroupBox3.Controls.Add(this.edtDreamIP);
            this.mpGroupBox3.Controls.Add(this.mpLabel5);
            this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox3.Location = new System.Drawing.Point(52, 43);
            this.mpGroupBox3.Name = "mpGroupBox3";
            this.mpGroupBox3.Size = new System.Drawing.Size(386, 182);
            this.mpGroupBox3.TabIndex = 0;
            this.mpGroupBox3.TabStop = false;
            this.mpGroupBox3.Text = "Dreambox";
            // 
            // pictureBox1
            // 
            this.pictureBox1.ErrorImage = null;
            this.pictureBox1.Image = global::ProcessPlugins.Properties.Resources.dream_20multimedia;
            this.pictureBox1.Location = new System.Drawing.Point(6, 117);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(157, 59);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(252, 137);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(100, 23);
            this.btnSaveSettings.TabIndex = 6;
            this.btnSaveSettings.Text = "Save settings";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // edtDreamPassword
            // 
            this.edtDreamPassword.BorderColor = System.Drawing.Color.Empty;
            this.edtDreamPassword.Location = new System.Drawing.Point(142, 91);
            this.edtDreamPassword.Name = "edtDreamPassword";
            this.edtDreamPassword.Size = new System.Drawing.Size(211, 20);
            this.edtDreamPassword.TabIndex = 5;
            // 
            // mpLabel7
            // 
            this.mpLabel7.AutoSize = true;
            this.mpLabel7.Location = new System.Drawing.Point(23, 94);
            this.mpLabel7.Name = "mpLabel7";
            this.mpLabel7.Size = new System.Drawing.Size(56, 13);
            this.mpLabel7.TabIndex = 4;
            this.mpLabel7.Text = "Password:";
            // 
            // edtDreamUserName
            // 
            this.edtDreamUserName.BorderColor = System.Drawing.Color.Empty;
            this.edtDreamUserName.Location = new System.Drawing.Point(142, 65);
            this.edtDreamUserName.Name = "edtDreamUserName";
            this.edtDreamUserName.Size = new System.Drawing.Size(211, 20);
            this.edtDreamUserName.TabIndex = 3;
            // 
            // mpLabel6
            // 
            this.mpLabel6.AutoSize = true;
            this.mpLabel6.Location = new System.Drawing.Point(23, 68);
            this.mpLabel6.Name = "mpLabel6";
            this.mpLabel6.Size = new System.Drawing.Size(58, 13);
            this.mpLabel6.TabIndex = 2;
            this.mpLabel6.Text = "Username:";
            // 
            // edtDreamIP
            // 
            this.edtDreamIP.BorderColor = System.Drawing.Color.Empty;
            this.edtDreamIP.Location = new System.Drawing.Point(142, 39);
            this.edtDreamIP.Name = "edtDreamIP";
            this.edtDreamIP.Size = new System.Drawing.Size(211, 20);
            this.edtDreamIP.TabIndex = 1;
            // 
            // mpLabel5
            // 
            this.mpLabel5.AutoSize = true;
            this.mpLabel5.Location = new System.Drawing.Point(23, 42);
            this.mpLabel5.Name = "mpLabel5";
            this.mpLabel5.Size = new System.Drawing.Size(113, 13);
            this.mpLabel5.TabIndex = 0;
            this.mpLabel5.Text = "IP address/Hostname:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.mpGroupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(519, 307);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Import Channels & EPG";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // mpGroupBox1
            // 
            this.mpGroupBox1.Controls.Add(this.mpGroupBox4);
            this.mpGroupBox1.Controls.Add(this.progLabel);
            this.mpGroupBox1.Controls.Add(this.lblProgressText);
            this.mpGroupBox1.Controls.Add(this.btnLoadChannels);
            this.mpGroupBox1.Controls.Add(this.progChannels);
            this.mpGroupBox1.Controls.Add(this.mpGroupBox2);
            this.mpGroupBox1.Controls.Add(this.btnEPG);
            this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox1.Location = new System.Drawing.Point(7, 7);
            this.mpGroupBox1.Name = "mpGroupBox1";
            this.mpGroupBox1.Size = new System.Drawing.Size(506, 284);
            this.mpGroupBox1.TabIndex = 0;
            this.mpGroupBox1.TabStop = false;
            this.mpGroupBox1.Text = "Import Channels and EPG";
            // 
            // progLabel
            // 
            this.progLabel.AutoSize = true;
            this.progLabel.Location = new System.Drawing.Point(33, 78);
            this.progLabel.Name = "progLabel";
            this.progLabel.Size = new System.Drawing.Size(0, 13);
            this.progLabel.TabIndex = 6;
            // 
            // lblProgressText
            // 
            this.lblProgressText.AutoSize = true;
            this.lblProgressText.Location = new System.Drawing.Point(33, 78);
            this.lblProgressText.Name = "lblProgressText";
            this.lblProgressText.Size = new System.Drawing.Size(0, 13);
            this.lblProgressText.TabIndex = 5;
            // 
            // btnLoadChannels
            // 
            this.btnLoadChannels.Location = new System.Drawing.Point(33, 241);
            this.btnLoadChannels.Name = "btnLoadChannels";
            this.btnLoadChannels.Size = new System.Drawing.Size(107, 23);
            this.btnLoadChannels.TabIndex = 4;
            this.btnLoadChannels.Text = "Load Channels";
            this.btnLoadChannels.UseVisualStyleBackColor = true;
            this.btnLoadChannels.Click += new System.EventHandler(this.btnLoadChannels_Click);
            // 
            // progChannels
            // 
            this.progChannels.Location = new System.Drawing.Point(33, 48);
            this.progChannels.Name = "progChannels";
            this.progChannels.Size = new System.Drawing.Size(452, 23);
            this.progChannels.TabIndex = 3;
            // 
            // mpGroupBox2
            // 
            this.mpGroupBox2.Controls.Add(this.mpLabel3);
            this.mpGroupBox2.Controls.Add(this.edtChannel);
            this.mpGroupBox2.Controls.Add(this.mpLabel4);
            this.mpGroupBox2.Controls.Add(this.edtTotalChannels);
            this.mpGroupBox2.Controls.Add(this.mpLabel2);
            this.mpGroupBox2.Controls.Add(this.edtBoutique);
            this.mpGroupBox2.Controls.Add(this.mpLabel1);
            this.mpGroupBox2.Controls.Add(this.edtTotalBoutiques);
            this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox2.Location = new System.Drawing.Point(167, 107);
            this.mpGroupBox2.Name = "mpGroupBox2";
            this.mpGroupBox2.Size = new System.Drawing.Size(318, 161);
            this.mpGroupBox2.TabIndex = 2;
            this.mpGroupBox2.TabStop = false;
            this.mpGroupBox2.Text = "Import Statisctics";
            // 
            // mpLabel3
            // 
            this.mpLabel3.AutoSize = true;
            this.mpLabel3.Location = new System.Drawing.Point(7, 114);
            this.mpLabel3.Name = "mpLabel3";
            this.mpLabel3.Size = new System.Drawing.Size(72, 13);
            this.mpLabel3.TabIndex = 7;
            this.mpLabel3.Text = "This Channel:";
            // 
            // edtChannel
            // 
            this.edtChannel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.edtChannel.BorderColor = System.Drawing.Color.Empty;
            this.edtChannel.Enabled = false;
            this.edtChannel.Location = new System.Drawing.Point(97, 111);
            this.edtChannel.Name = "edtChannel";
            this.edtChannel.ReadOnly = true;
            this.edtChannel.Size = new System.Drawing.Size(215, 20);
            this.edtChannel.TabIndex = 6;
            // 
            // mpLabel4
            // 
            this.mpLabel4.AutoSize = true;
            this.mpLabel4.Location = new System.Drawing.Point(7, 88);
            this.mpLabel4.Name = "mpLabel4";
            this.mpLabel4.Size = new System.Drawing.Size(81, 13);
            this.mpLabel4.TabIndex = 5;
            this.mpLabel4.Text = "Total Channels:";
            // 
            // edtTotalChannels
            // 
            this.edtTotalChannels.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.edtTotalChannels.BorderColor = System.Drawing.Color.Empty;
            this.edtTotalChannels.Enabled = false;
            this.edtTotalChannels.Location = new System.Drawing.Point(97, 85);
            this.edtTotalChannels.Name = "edtTotalChannels";
            this.edtTotalChannels.ReadOnly = true;
            this.edtTotalChannels.Size = new System.Drawing.Size(67, 20);
            this.edtTotalChannels.TabIndex = 4;
            // 
            // mpLabel2
            // 
            this.mpLabel2.AutoSize = true;
            this.mpLabel2.Location = new System.Drawing.Point(7, 59);
            this.mpLabel2.Name = "mpLabel2";
            this.mpLabel2.Size = new System.Drawing.Size(75, 13);
            this.mpLabel2.TabIndex = 3;
            this.mpLabel2.Text = "This Boutique:";
            // 
            // edtBoutique
            // 
            this.edtBoutique.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.edtBoutique.BorderColor = System.Drawing.Color.Empty;
            this.edtBoutique.Enabled = false;
            this.edtBoutique.Location = new System.Drawing.Point(97, 56);
            this.edtBoutique.Name = "edtBoutique";
            this.edtBoutique.ReadOnly = true;
            this.edtBoutique.Size = new System.Drawing.Size(215, 20);
            this.edtBoutique.TabIndex = 2;
            // 
            // mpLabel1
            // 
            this.mpLabel1.AutoSize = true;
            this.mpLabel1.Location = new System.Drawing.Point(7, 33);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new System.Drawing.Size(84, 13);
            this.mpLabel1.TabIndex = 1;
            this.mpLabel1.Text = "Total Boutiques:";
            // 
            // edtTotalBoutiques
            // 
            this.edtTotalBoutiques.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.edtTotalBoutiques.BorderColor = System.Drawing.Color.Empty;
            this.edtTotalBoutiques.Enabled = false;
            this.edtTotalBoutiques.Location = new System.Drawing.Point(97, 30);
            this.edtTotalBoutiques.Name = "edtTotalBoutiques";
            this.edtTotalBoutiques.ReadOnly = true;
            this.edtTotalBoutiques.Size = new System.Drawing.Size(67, 20);
            this.edtTotalBoutiques.TabIndex = 0;
            // 
            // btnEPG
            // 
            this.btnEPG.Location = new System.Drawing.Point(33, 211);
            this.btnEPG.Name = "btnEPG";
            this.btnEPG.Size = new System.Drawing.Size(107, 23);
            this.btnEPG.TabIndex = 0;
            this.btnEPG.Text = "Load EPG";
            this.btnEPG.UseVisualStyleBackColor = true;
            this.btnEPG.Click += new System.EventHandler(this.btnEPG_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(460, 344);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Cancel";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // mpGroupBox4
            // 
            this.mpGroupBox4.Controls.Add(this.edtSaveEPGSyncSettings);
            this.mpGroupBox4.Controls.Add(this.mpLabel8);
            this.mpGroupBox4.Controls.Add(this.edtSyncHours);
            this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox4.Location = new System.Drawing.Point(33, 107);
            this.mpGroupBox4.Name = "mpGroupBox4";
            this.mpGroupBox4.Size = new System.Drawing.Size(107, 98);
            this.mpGroupBox4.TabIndex = 7;
            this.mpGroupBox4.TabStop = false;
            this.mpGroupBox4.Text = "Sync EPG every:";
            // 
            // edtSyncHours
            // 
            this.edtSyncHours.Location = new System.Drawing.Point(14, 26);
            this.edtSyncHours.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.edtSyncHours.Name = "edtSyncHours";
            this.edtSyncHours.Size = new System.Drawing.Size(48, 20);
            this.edtSyncHours.TabIndex = 0;
            // 
            // mpLabel8
            // 
            this.mpLabel8.AutoSize = true;
            this.mpLabel8.Location = new System.Drawing.Point(68, 30);
            this.mpLabel8.Name = "mpLabel8";
            this.mpLabel8.Size = new System.Drawing.Size(33, 13);
            this.mpLabel8.TabIndex = 1;
            this.mpLabel8.Text = "hours";
            // 
            // edtSaveEPGSyncSettings
            // 
            this.edtSaveEPGSyncSettings.Location = new System.Drawing.Point(7, 59);
            this.edtSaveEPGSyncSettings.Name = "edtSaveEPGSyncSettings";
            this.edtSaveEPGSyncSettings.Size = new System.Drawing.Size(94, 23);
            this.edtSaveEPGSyncSettings.TabIndex = 2;
            this.edtSaveEPGSyncSettings.Text = "Save";
            this.edtSaveEPGSyncSettings.UseVisualStyleBackColor = true;
            this.edtSaveEPGSyncSettings.Click += new System.EventHandler(this.edtSaveEPGSyncSettings_Click);
            // 
            // ExternalDreamBoxTVSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 386);
            this.ControlBox = false;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.mpTabControl1);
            this.Name = "ExternalDreamBoxTVSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "External DreamBox TV Settings";
            this.Load += new System.EventHandler(this.ExternalDreamBoxTVSettings_Load);
            this.mpTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.mpGroupBox3.ResumeLayout(false);
            this.mpGroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.mpGroupBox1.ResumeLayout(false);
            this.mpGroupBox1.PerformLayout();
            this.mpGroupBox2.ResumeLayout(false);
            this.mpGroupBox2.PerformLayout();
            this.mpGroupBox4.ResumeLayout(false);
            this.mpGroupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.edtSyncHours)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
        private MediaPortal.UserInterface.Controls.MPButton btnEPG;
        private System.Windows.Forms.ProgressBar progChannels;
        private MediaPortal.UserInterface.Controls.MPTextBox edtTotalBoutiques;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
        private MediaPortal.UserInterface.Controls.MPTextBox edtBoutique;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
        private MediaPortal.UserInterface.Controls.MPTextBox edtChannel;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
        private MediaPortal.UserInterface.Controls.MPTextBox edtTotalChannels;
        private System.Windows.Forms.Button btnLoadChannels;
        private System.Windows.Forms.Label lblProgressText;
        private MediaPortal.UserInterface.Controls.MPLabel progLabel;
        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
        private MediaPortal.UserInterface.Controls.MPTextBox edtDreamPassword;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel7;
        private MediaPortal.UserInterface.Controls.MPTextBox edtDreamUserName;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel6;
        private MediaPortal.UserInterface.Controls.MPTextBox edtDreamIP;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
        private MediaPortal.UserInterface.Controls.MPButton btnSaveSettings;
        private MediaPortal.UserInterface.Controls.MPButton btnClose;
        private System.Windows.Forms.PictureBox pictureBox1;
        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox4;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel8;
        private MediaPortal.UserInterface.Controls.MPNumericUpDown edtSyncHours;
        private MediaPortal.UserInterface.Controls.MPButton edtSaveEPGSyncSettings;
    }
}