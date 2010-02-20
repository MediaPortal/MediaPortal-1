namespace MediaPortal.Configuration.Sections
{
  partial class MusicASIO
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private new System.ComponentModel.IContainer components = null;

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
      this.asioDeviceComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lbBalance = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarBalance = new System.Windows.Forms.HScrollBar();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lbNumberOfChannels = new MediaPortal.UserInterface.Controls.MPLabel();
      this.useASIOCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.btSettings = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // asioDeviceComboBox
      // 
      this.asioDeviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.asioDeviceComboBox.BorderColor = System.Drawing.Color.Empty;
      this.asioDeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.asioDeviceComboBox.Location = new System.Drawing.Point(81, 65);
      this.asioDeviceComboBox.Name = "asioDeviceComboBox";
      this.asioDeviceComboBox.Size = new System.Drawing.Size(262, 21);
      this.asioDeviceComboBox.TabIndex = 5;
      this.asioDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.asioDeviceComboBox_SelectedIndexChanged);
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(5, 68);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(72, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "ASIO Device:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.btSettings);
      this.mpGroupBox1.Controls.Add(this.lbBalance);
      this.mpGroupBox1.Controls.Add(this.hScrollBarBalance);
      this.mpGroupBox1.Controls.Add(this.mpLabel4);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.lbNumberOfChannels);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.asioDeviceComboBox);
      this.mpGroupBox1.Controls.Add(this.useASIOCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 241);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "General settings";
      // 
      // lbBalance
      // 
      this.lbBalance.AutoSize = true;
      this.lbBalance.Location = new System.Drawing.Point(371, 169);
      this.lbBalance.Name = "lbBalance";
      this.lbBalance.Size = new System.Drawing.Size(28, 13);
      this.lbBalance.TabIndex = 12;
      this.lbBalance.Text = "0.00";
      // 
      // hScrollBarBalance
      // 
      this.hScrollBarBalance.Location = new System.Drawing.Point(81, 160);
      this.hScrollBarBalance.Minimum = -100;
      this.hScrollBarBalance.Name = "hScrollBarBalance";
      this.hScrollBarBalance.Size = new System.Drawing.Size(262, 22);
      this.hScrollBarBalance.TabIndex = 11;
      this.hScrollBarBalance.ValueChanged += new System.EventHandler(this.hScrollBarBalance_ValueChanged);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(84, 201);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(282, 26);
      this.mpLabel4.TabIndex = 10;
      this.mpLabel4.Text = "In case of multi-channel (not stereo) the left/right positions \r\nare interleaved " +
          "between the additional channels.";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(19, 169);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(49, 13);
      this.mpLabel1.TabIndex = 7;
      this.mpLabel1.Text = "Balance:";
      // 
      // lbNumberOfChannels
      // 
      this.lbNumberOfChannels.AutoSize = true;
      this.lbNumberOfChannels.Location = new System.Drawing.Point(81, 106);
      this.lbNumberOfChannels.Name = "lbNumberOfChannels";
      this.lbNumberOfChannels.Size = new System.Drawing.Size(22, 13);
      this.lbNumberOfChannels.TabIndex = 6;
      this.lbNumberOfChannels.Text = " __";
      // 
      // useASIOCheckBox
      // 
      this.useASIOCheckBox.AutoSize = true;
      this.useASIOCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useASIOCheckBox.Location = new System.Drawing.Point(87, 24);
      this.useASIOCheckBox.Name = "useASIOCheckBox";
      this.useASIOCheckBox.Size = new System.Drawing.Size(197, 17);
      this.useASIOCheckBox.TabIndex = 2;
      this.useASIOCheckBox.Text = "Use ASIO (available with BASS only)";
      this.useASIOCheckBox.UseVisualStyleBackColor = true;
      this.useASIOCheckBox.CheckedChanged += new System.EventHandler(this.useASIOCheckBox_CheckedChanged);
      // 
      // btSettings
      // 
      this.btSettings.Location = new System.Drawing.Point(351, 63);
      this.btSettings.Name = "btSettings";
      this.btSettings.Size = new System.Drawing.Size(75, 23);
      this.btSettings.TabIndex = 13;
      this.btSettings.Text = "Settings";
      this.btSettings.UseVisualStyleBackColor = true;
      this.btSettings.Click += new System.EventHandler(this.btSettings_Click);
      // 
      // MusicASIO
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "MusicASIO";
      this.Size = new System.Drawing.Size(472, 400);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPComboBox asioDeviceComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox useASIOCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel lbNumberOfChannels;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.HScrollBar hScrollBarBalance;
    private MediaPortal.UserInterface.Controls.MPLabel lbBalance;
    private MediaPortal.UserInterface.Controls.MPButton btSettings;
  }
}
