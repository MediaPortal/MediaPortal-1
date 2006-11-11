namespace ProcessPlugins.DreamboxDirectEPG
{
    partial class DreamboxDirectEPGSettings
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
            this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpButton2 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpNumericUpDown1 = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
            this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpNumericUpDown2 = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
            this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtDreamboxIP = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.edtUserName = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.edtPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // mpGroupBox1
            // 
            this.mpGroupBox1.Controls.Add(this.edtPassword);
            this.mpGroupBox1.Controls.Add(this.mpLabel5);
            this.mpGroupBox1.Controls.Add(this.edtUserName);
            this.mpGroupBox1.Controls.Add(this.mpLabel4);
            this.mpGroupBox1.Controls.Add(this.edtDreamboxIP);
            this.mpGroupBox1.Controls.Add(this.mpLabel3);
            this.mpGroupBox1.Controls.Add(this.mpLabel2);
            this.mpGroupBox1.Controls.Add(this.mpNumericUpDown2);
            this.mpGroupBox1.Controls.Add(this.mpLabel1);
            this.mpGroupBox1.Controls.Add(this.mpNumericUpDown1);
            this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox1.Location = new System.Drawing.Point(13, 13);
            this.mpGroupBox1.Name = "mpGroupBox1";
            this.mpGroupBox1.Size = new System.Drawing.Size(308, 144);
            this.mpGroupBox1.TabIndex = 0;
            this.mpGroupBox1.TabStop = false;
            this.mpGroupBox1.Text = "Dreambox EPG schedule: ";
            // 
            // mpButton1
            // 
            this.mpButton1.Location = new System.Drawing.Point(246, 163);
            this.mpButton1.Name = "mpButton1";
            this.mpButton1.Size = new System.Drawing.Size(75, 23);
            this.mpButton1.TabIndex = 1;
            this.mpButton1.Text = "Cancel";
            this.mpButton1.UseVisualStyleBackColor = true;
            this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
            // 
            // mpButton2
            // 
            this.mpButton2.Location = new System.Drawing.Point(165, 163);
            this.mpButton2.Name = "mpButton2";
            this.mpButton2.Size = new System.Drawing.Size(75, 23);
            this.mpButton2.TabIndex = 2;
            this.mpButton2.Text = "Save";
            this.mpButton2.UseVisualStyleBackColor = true;
            this.mpButton2.Click += new System.EventHandler(this.mpButton2_Click);
            // 
            // mpNumericUpDown1
            // 
            this.mpNumericUpDown1.Location = new System.Drawing.Point(127, 118);
            this.mpNumericUpDown1.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.mpNumericUpDown1.Name = "mpNumericUpDown1";
            this.mpNumericUpDown1.Size = new System.Drawing.Size(39, 20);
            this.mpNumericUpDown1.TabIndex = 0;
            this.mpNumericUpDown1.Tag = "";
            this.mpNumericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // mpLabel1
            // 
            this.mpLabel1.AutoSize = true;
            this.mpLabel1.Location = new System.Drawing.Point(172, 120);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new System.Drawing.Size(10, 13);
            this.mpLabel1.TabIndex = 1;
            this.mpLabel1.Text = ":";
            // 
            // mpNumericUpDown2
            // 
            this.mpNumericUpDown2.Location = new System.Drawing.Point(188, 118);
            this.mpNumericUpDown2.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.mpNumericUpDown2.Name = "mpNumericUpDown2";
            this.mpNumericUpDown2.Size = new System.Drawing.Size(39, 20);
            this.mpNumericUpDown2.TabIndex = 2;
            // 
            // mpLabel2
            // 
            this.mpLabel2.AutoSize = true;
            this.mpLabel2.Location = new System.Drawing.Point(88, 120);
            this.mpLabel2.Name = "mpLabel2";
            this.mpLabel2.Size = new System.Drawing.Size(33, 13);
            this.mpLabel2.TabIndex = 3;
            this.mpLabel2.Text = "Time:";
            // 
            // mpLabel3
            // 
            this.mpLabel3.AutoSize = true;
            this.mpLabel3.Location = new System.Drawing.Point(7, 31);
            this.mpLabel3.Name = "mpLabel3";
            this.mpLabel3.Size = new System.Drawing.Size(71, 13);
            this.mpLabel3.TabIndex = 4;
            this.mpLabel3.Text = "Dreambox IP:";
            // 
            // edtDreamboxIP
            // 
            this.edtDreamboxIP.BorderColor = System.Drawing.Color.Empty;
            this.edtDreamboxIP.Location = new System.Drawing.Point(84, 28);
            this.edtDreamboxIP.Name = "edtDreamboxIP";
            this.edtDreamboxIP.Size = new System.Drawing.Size(143, 20);
            this.edtDreamboxIP.TabIndex = 5;
            // 
            // edtUserName
            // 
            this.edtUserName.BorderColor = System.Drawing.Color.Empty;
            this.edtUserName.Location = new System.Drawing.Point(84, 54);
            this.edtUserName.Name = "edtUserName";
            this.edtUserName.Size = new System.Drawing.Size(143, 20);
            this.edtUserName.TabIndex = 7;
            // 
            // mpLabel4
            // 
            this.mpLabel4.AutoSize = true;
            this.mpLabel4.Location = new System.Drawing.Point(7, 57);
            this.mpLabel4.Name = "mpLabel4";
            this.mpLabel4.Size = new System.Drawing.Size(61, 13);
            this.mpLabel4.TabIndex = 6;
            this.mpLabel4.Text = "User name:";
            // 
            // edtPassword
            // 
            this.edtPassword.BorderColor = System.Drawing.Color.Empty;
            this.edtPassword.Location = new System.Drawing.Point(84, 80);
            this.edtPassword.Name = "edtPassword";
            this.edtPassword.Size = new System.Drawing.Size(143, 20);
            this.edtPassword.TabIndex = 9;
            // 
            // mpLabel5
            // 
            this.mpLabel5.AutoSize = true;
            this.mpLabel5.Location = new System.Drawing.Point(7, 83);
            this.mpLabel5.Name = "mpLabel5";
            this.mpLabel5.Size = new System.Drawing.Size(56, 13);
            this.mpLabel5.TabIndex = 8;
            this.mpLabel5.Text = "Password:";
            // 
            // DreamboxDirectEPGSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(333, 198);
            this.ControlBox = false;
            this.Controls.Add(this.mpButton2);
            this.Controls.Add(this.mpButton1);
            this.Controls.Add(this.mpGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DreamboxDirectEPGSettings";
            this.Text = "Dreambox Direct EPG";
            this.Load += new System.EventHandler(this.DreamboxDirectEPGSettings_Load);
            this.mpGroupBox1.ResumeLayout(false);
            this.mpGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDown2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
        private MediaPortal.UserInterface.Controls.MPButton mpButton1;
        private MediaPortal.UserInterface.Controls.MPButton mpButton2;
        private MediaPortal.UserInterface.Controls.MPNumericUpDown mpNumericUpDown1;
        private MediaPortal.UserInterface.Controls.MPNumericUpDown mpNumericUpDown2;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
        private MediaPortal.UserInterface.Controls.MPTextBox edtPassword;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
        private MediaPortal.UserInterface.Controls.MPTextBox edtUserName;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
        private MediaPortal.UserInterface.Controls.MPTextBox edtDreamboxIP;
        private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    }
}