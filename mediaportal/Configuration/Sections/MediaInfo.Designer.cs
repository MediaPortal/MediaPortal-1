namespace MediaPortal.Configuration.Sections
{
  partial class MediaInfo
  {
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.mpGroupBoxDB = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpCheckBoxEnAudioCD = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpCheckBoxEnImage = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpCheckBoxEnPicture = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpCheckBoxEnAudio = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpCheckBoxEnVideo = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpCheckBoxEnDVD = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpCheckBoxEnBluray = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.mpButtonClear = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpGroupBoxDbMaintenance = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpCheckBoxDeleteOlder = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.nudDays = new System.Windows.Forms.NumericUpDown();
            this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpGroupBoxDB.SuspendLayout();
            this.mpGroupBoxDbMaintenance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDays)).BeginInit();
            this.SuspendLayout();
            // 
            // mpGroupBoxDB
            // 
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnAudioCD);
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnImage);
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnPicture);
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnAudio);
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnVideo);
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnDVD);
            this.mpGroupBoxDB.Controls.Add(this.mpCheckBoxEnBluray);
            this.mpGroupBoxDB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBoxDB.Location = new System.Drawing.Point(6, 0);
            this.mpGroupBoxDB.Name = "mpGroupBoxDB";
            this.mpGroupBoxDB.Size = new System.Drawing.Size(462, 77);
            this.mpGroupBoxDB.TabIndex = 0;
            this.mpGroupBoxDB.TabStop = false;
            this.mpGroupBoxDB.Text = "Caching";
            // 
            // mpCheckBoxEnAudioCD
            // 
            this.mpCheckBoxEnAudioCD.AutoSize = true;
            this.mpCheckBoxEnAudioCD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnAudioCD.Location = new System.Drawing.Point(251, 24);
            this.mpCheckBoxEnAudioCD.Name = "mpCheckBoxEnAudioCD";
            this.mpCheckBoxEnAudioCD.Size = new System.Drawing.Size(69, 17);
            this.mpCheckBoxEnAudioCD.TabIndex = 9;
            this.mpCheckBoxEnAudioCD.Text = "Audio CD";
            this.mpCheckBoxEnAudioCD.UseVisualStyleBackColor = true;
            // 
            // mpCheckBoxEnImage
            // 
            this.mpCheckBoxEnImage.AutoSize = true;
            this.mpCheckBoxEnImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnImage.Location = new System.Drawing.Point(172, 47);
            this.mpCheckBoxEnImage.Name = "mpCheckBoxEnImage";
            this.mpCheckBoxEnImage.Size = new System.Drawing.Size(53, 17);
            this.mpCheckBoxEnImage.TabIndex = 8;
            this.mpCheckBoxEnImage.Text = "Image";
            this.mpCheckBoxEnImage.UseVisualStyleBackColor = true;
            // 
            // mpCheckBoxEnPicture
            // 
            this.mpCheckBoxEnPicture.AutoSize = true;
            this.mpCheckBoxEnPicture.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnPicture.Location = new System.Drawing.Point(172, 24);
            this.mpCheckBoxEnPicture.Name = "mpCheckBoxEnPicture";
            this.mpCheckBoxEnPicture.Size = new System.Drawing.Size(57, 17);
            this.mpCheckBoxEnPicture.TabIndex = 7;
            this.mpCheckBoxEnPicture.Text = "Picture";
            this.mpCheckBoxEnPicture.UseVisualStyleBackColor = true;
            // 
            // mpCheckBoxEnAudio
            // 
            this.mpCheckBoxEnAudio.AutoSize = true;
            this.mpCheckBoxEnAudio.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnAudio.Location = new System.Drawing.Point(96, 47);
            this.mpCheckBoxEnAudio.Name = "mpCheckBoxEnAudio";
            this.mpCheckBoxEnAudio.Size = new System.Drawing.Size(51, 17);
            this.mpCheckBoxEnAudio.TabIndex = 6;
            this.mpCheckBoxEnAudio.Text = "Audio";
            this.mpCheckBoxEnAudio.UseVisualStyleBackColor = true;
            // 
            // mpCheckBoxEnVideo
            // 
            this.mpCheckBoxEnVideo.AutoSize = true;
            this.mpCheckBoxEnVideo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnVideo.Location = new System.Drawing.Point(96, 24);
            this.mpCheckBoxEnVideo.Name = "mpCheckBoxEnVideo";
            this.mpCheckBoxEnVideo.Size = new System.Drawing.Size(51, 17);
            this.mpCheckBoxEnVideo.TabIndex = 5;
            this.mpCheckBoxEnVideo.Text = "Video";
            this.mpCheckBoxEnVideo.UseVisualStyleBackColor = true;
            // 
            // mpCheckBoxEnDVD
            // 
            this.mpCheckBoxEnDVD.AutoSize = true;
            this.mpCheckBoxEnDVD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnDVD.Location = new System.Drawing.Point(18, 47);
            this.mpCheckBoxEnDVD.Name = "mpCheckBoxEnDVD";
            this.mpCheckBoxEnDVD.Size = new System.Drawing.Size(47, 17);
            this.mpCheckBoxEnDVD.TabIndex = 4;
            this.mpCheckBoxEnDVD.Text = "DVD";
            this.mpCheckBoxEnDVD.UseVisualStyleBackColor = true;
            // 
            // mpCheckBoxEnBluray
            // 
            this.mpCheckBoxEnBluray.AutoSize = true;
            this.mpCheckBoxEnBluray.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxEnBluray.Location = new System.Drawing.Point(18, 24);
            this.mpCheckBoxEnBluray.Name = "mpCheckBoxEnBluray";
            this.mpCheckBoxEnBluray.Size = new System.Drawing.Size(53, 17);
            this.mpCheckBoxEnBluray.TabIndex = 3;
            this.mpCheckBoxEnBluray.Text = "Bluray";
            this.mpCheckBoxEnBluray.UseVisualStyleBackColor = true;
            // 
            // mpButtonClear
            // 
            this.mpButtonClear.Location = new System.Drawing.Point(108, 67);
            this.mpButtonClear.Name = "mpButtonClear";
            this.mpButtonClear.Size = new System.Drawing.Size(258, 23);
            this.mpButtonClear.TabIndex = 2;
            this.mpButtonClear.Text = "Clear MediaInfo Database";
            this.mpButtonClear.UseVisualStyleBackColor = true;
            this.mpButtonClear.Click += new System.EventHandler(this.mpButtonClear_Click);
            // 
            // mpGroupBoxDbMaintenance
            // 
            this.mpGroupBoxDbMaintenance.Controls.Add(this.mpCheckBoxDeleteOlder);
            this.mpGroupBoxDbMaintenance.Controls.Add(this.nudDays);
            this.mpGroupBoxDbMaintenance.Controls.Add(this.mpLabel1);
            this.mpGroupBoxDbMaintenance.Controls.Add(this.mpButtonClear);
            this.mpGroupBoxDbMaintenance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBoxDbMaintenance.Location = new System.Drawing.Point(6, 83);
            this.mpGroupBoxDbMaintenance.Name = "mpGroupBoxDbMaintenance";
            this.mpGroupBoxDbMaintenance.Size = new System.Drawing.Size(462, 111);
            this.mpGroupBoxDbMaintenance.TabIndex = 1;
            this.mpGroupBoxDbMaintenance.TabStop = false;
            this.mpGroupBoxDbMaintenance.Text = "Database maintenance";
            // 
            // mpCheckBoxDeleteOlder
            // 
            this.mpCheckBoxDeleteOlder.AutoSize = true;
            this.mpCheckBoxDeleteOlder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpCheckBoxDeleteOlder.Location = new System.Drawing.Point(18, 29);
            this.mpCheckBoxDeleteOlder.Name = "mpCheckBoxDeleteOlder";
            this.mpCheckBoxDeleteOlder.Size = new System.Drawing.Size(206, 17);
            this.mpCheckBoxDeleteOlder.TabIndex = 10;
            this.mpCheckBoxDeleteOlder.Text = "Automatically delete records older than";
            this.mpCheckBoxDeleteOlder.UseVisualStyleBackColor = true;
            this.mpCheckBoxDeleteOlder.CheckedChanged += new System.EventHandler(this.mpCheckBoxDeleteOlder_CheckedChanged);
            // 
            // nudDays
            // 
            this.nudDays.Enabled = false;
            this.nudDays.Location = new System.Drawing.Point(235, 27);
            this.nudDays.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDays.Name = "nudDays";
            this.nudDays.Size = new System.Drawing.Size(52, 20);
            this.nudDays.TabIndex = 4;
            this.nudDays.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // mpLabel1
            // 
            this.mpLabel1.AutoSize = true;
            this.mpLabel1.Location = new System.Drawing.Point(293, 31);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new System.Drawing.Size(29, 13);
            this.mpLabel1.TabIndex = 3;
            this.mpLabel1.Text = "days";
            // 
            // MediaInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mpGroupBoxDbMaintenance);
            this.Controls.Add(this.mpGroupBoxDB);
            this.Name = "MediaInfo";
            this.Size = new System.Drawing.Size(472, 408);
            this.mpGroupBoxDB.ResumeLayout(false);
            this.mpGroupBoxDB.PerformLayout();
            this.mpGroupBoxDbMaintenance.ResumeLayout(false);
            this.mpGroupBoxDbMaintenance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDays)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxDB;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClear;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnBluray;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnImage;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnPicture;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnAudio;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnVideo;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnDVD;
    private UserInterface.Controls.MPGroupBox mpGroupBoxDbMaintenance;
    private UserInterface.Controls.MPCheckBox mpCheckBoxEnAudioCD;
    private UserInterface.Controls.MPCheckBox mpCheckBoxDeleteOlder;
    private System.Windows.Forms.NumericUpDown nudDays;
    private UserInterface.Controls.MPLabel mpLabel1;
  }
}