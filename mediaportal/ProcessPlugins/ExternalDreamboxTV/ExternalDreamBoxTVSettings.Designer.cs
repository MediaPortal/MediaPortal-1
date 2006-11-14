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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btnEPG = new MediaPortal.UserInterface.Controls.MPButton();
            this.progBoutique = new System.Windows.Forms.ProgressBar();
            this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.progChannels = new System.Windows.Forms.ProgressBar();
            this.edtTotalBoutiques = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtBoutique = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtChannel = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtTotalChannels = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpTabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.mpGroupBox1.SuspendLayout();
            this.mpGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mpTabControl1
            // 
            this.mpTabControl1.Controls.Add(this.tabPage1);
            this.mpTabControl1.Controls.Add(this.tabPage2);
            this.mpTabControl1.Location = new System.Drawing.Point(13, 4);
            this.mpTabControl1.Name = "mpTabControl1";
            this.mpTabControl1.SelectedIndex = 0;
            this.mpTabControl1.Size = new System.Drawing.Size(527, 323);
            this.mpTabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(519, 297);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.mpGroupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(519, 297);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // mpGroupBox1
            // 
            this.mpGroupBox1.Controls.Add(this.progChannels);
            this.mpGroupBox1.Controls.Add(this.mpGroupBox2);
            this.mpGroupBox1.Controls.Add(this.progBoutique);
            this.mpGroupBox1.Controls.Add(this.btnEPG);
            this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox1.Location = new System.Drawing.Point(7, 7);
            this.mpGroupBox1.Name = "mpGroupBox1";
            this.mpGroupBox1.Size = new System.Drawing.Size(506, 284);
            this.mpGroupBox1.TabIndex = 0;
            this.mpGroupBox1.TabStop = false;
            this.mpGroupBox1.Text = "mpGroupBox1";
            // 
            // btnEPG
            // 
            this.btnEPG.Location = new System.Drawing.Point(33, 245);
            this.btnEPG.Name = "btnEPG";
            this.btnEPG.Size = new System.Drawing.Size(75, 23);
            this.btnEPG.TabIndex = 0;
            this.btnEPG.Text = "Load EPG";
            this.btnEPG.UseVisualStyleBackColor = true;
            this.btnEPG.Click += new System.EventHandler(this.btnEPG_Click);
            // 
            // progBoutique
            // 
            this.progBoutique.Location = new System.Drawing.Point(33, 19);
            this.progBoutique.Name = "progBoutique";
            this.progBoutique.Size = new System.Drawing.Size(452, 23);
            this.progBoutique.TabIndex = 1;
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
            this.mpGroupBox2.Text = "mpGroupBox2";
            // 
            // progChannels
            // 
            this.progChannels.Location = new System.Drawing.Point(33, 48);
            this.progChannels.Name = "progChannels";
            this.progChannels.Size = new System.Drawing.Size(452, 23);
            this.progChannels.TabIndex = 3;
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
            // mpLabel1
            // 
            this.mpLabel1.AutoSize = true;
            this.mpLabel1.Location = new System.Drawing.Point(7, 33);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new System.Drawing.Size(84, 13);
            this.mpLabel1.TabIndex = 1;
            this.mpLabel1.Text = "Total Boutiques:";
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
            // ExternalDreamBoxTVSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 349);
            this.Controls.Add(this.mpTabControl1);
            this.Name = "ExternalDreamBoxTVSettings";
            this.Text = "ExternalDreamBoxTVSettings";
            this.mpTabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.mpGroupBox1.ResumeLayout(false);
            this.mpGroupBox2.ResumeLayout(false);
            this.mpGroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
        private System.Windows.Forms.ProgressBar progBoutique;
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
    }
}