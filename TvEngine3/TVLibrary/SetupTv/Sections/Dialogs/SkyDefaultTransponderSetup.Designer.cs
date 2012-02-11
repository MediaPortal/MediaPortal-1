namespace SetupTv.Sections.Dialogs
{
  partial class SkyDefaultTransponderSetup
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
      this.lbFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbFrequency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.cbPolarisation = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tbSymbolRate = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbNetworkId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.cbFEC = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lbPolarisation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lbSymbolRate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lbFEC = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lbNetworkId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.btCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // lbFrequency
      // 
      this.lbFrequency.AutoSize = true;
      this.lbFrequency.Location = new System.Drawing.Point(17, 15);
      this.lbFrequency.Name = "lbFrequency";
      this.lbFrequency.Size = new System.Drawing.Size(83, 13);
      this.lbFrequency.TabIndex = 0;
      this.lbFrequency.Text = "Frequency (khz)";
      // 
      // tbFrequency
      // 
      this.tbFrequency.Location = new System.Drawing.Point(106, 12);
      this.tbFrequency.Name = "tbFrequency";
      this.tbFrequency.Size = new System.Drawing.Size(100, 20);
      this.tbFrequency.TabIndex = 1;
      // 
      // cbPolarisation
      // 
      this.cbPolarisation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPolarisation.FormattingEnabled = true;
      this.cbPolarisation.Items.AddRange(new object[] {
            "Horizontal",
            "Vertical",
            "Circular Left",
            "Circular Right"});
      this.cbPolarisation.Location = new System.Drawing.Point(106, 38);
      this.cbPolarisation.Name = "cbPolarisation";
      this.cbPolarisation.Size = new System.Drawing.Size(100, 21);
      this.cbPolarisation.TabIndex = 2;
      // 
      // tbSymbolRate
      // 
      this.tbSymbolRate.Location = new System.Drawing.Point(106, 65);
      this.tbSymbolRate.Name = "tbSymbolRate";
      this.tbSymbolRate.Size = new System.Drawing.Size(100, 20);
      this.tbSymbolRate.TabIndex = 3;
      // 
      // tbNetworkId
      // 
      this.tbNetworkId.Location = new System.Drawing.Point(106, 118);
      this.tbNetworkId.Name = "tbNetworkId";
      this.tbNetworkId.Size = new System.Drawing.Size(100, 20);
      this.tbNetworkId.TabIndex = 4;
      // 
      // cbFEC
      // 
      this.cbFEC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFEC.FormattingEnabled = true;
      this.cbFEC.Items.AddRange(new object[] {
            "1/2",
            "1/3",
            "1/4",
            "2/3",
            "2/5",
            "3/4",
            "3/5",
            "4/5",
            "5/11",
            "5/6",
            "7/8",
            "8/9",
            "9/10"});
      this.cbFEC.Location = new System.Drawing.Point(106, 91);
      this.cbFEC.Name = "cbFEC";
      this.cbFEC.Size = new System.Drawing.Size(100, 21);
      this.cbFEC.TabIndex = 5;
      // 
      // lbPolarisation
      // 
      this.lbPolarisation.AutoSize = true;
      this.lbPolarisation.Location = new System.Drawing.Point(17, 41);
      this.lbPolarisation.Name = "lbPolarisation";
      this.lbPolarisation.Size = new System.Drawing.Size(61, 13);
      this.lbPolarisation.TabIndex = 6;
      this.lbPolarisation.Text = "Polarisation";
      // 
      // lbSymbolRate
      // 
      this.lbSymbolRate.AutoSize = true;
      this.lbSymbolRate.Location = new System.Drawing.Point(17, 68);
      this.lbSymbolRate.Name = "lbSymbolRate";
      this.lbSymbolRate.Size = new System.Drawing.Size(62, 13);
      this.lbSymbolRate.TabIndex = 7;
      this.lbSymbolRate.Text = "Symbol rate";
      // 
      // lbFEC
      // 
      this.lbFEC.AutoSize = true;
      this.lbFEC.Location = new System.Drawing.Point(17, 94);
      this.lbFEC.Name = "lbFEC";
      this.lbFEC.Size = new System.Drawing.Size(27, 13);
      this.lbFEC.TabIndex = 8;
      this.lbFEC.Text = "FEC";
      // 
      // lbNetworkId
      // 
      this.lbNetworkId.AutoSize = true;
      this.lbNetworkId.Location = new System.Drawing.Point(18, 121);
      this.lbNetworkId.Name = "lbNetworkId";
      this.lbNetworkId.Size = new System.Drawing.Size(81, 13);
      this.lbNetworkId.TabIndex = 9;
      this.lbNetworkId.Text = "Network ID (0x)";
      // 
      // btSave
      // 
      this.btSave.Location = new System.Drawing.Point(37, 163);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(75, 25);
      this.btSave.TabIndex = 10;
      this.btSave.Text = "Save";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new System.EventHandler(this.btSave_Click);
      // 
      // btCancel
      // 
      this.btCancel.Location = new System.Drawing.Point(118, 163);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new System.Drawing.Size(75, 25);
      this.btCancel.TabIndex = 11;
      this.btCancel.Text = "Cancel";
      this.btCancel.UseVisualStyleBackColor = true;
      this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
      // 
      // SkyDefaultTransponderSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(231, 200);
      this.Controls.Add(this.btCancel);
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.lbNetworkId);
      this.Controls.Add(this.lbFEC);
      this.Controls.Add(this.lbSymbolRate);
      this.Controls.Add(this.lbPolarisation);
      this.Controls.Add(this.cbFEC);
      this.Controls.Add(this.tbNetworkId);
      this.Controls.Add(this.tbSymbolRate);
      this.Controls.Add(this.cbPolarisation);
      this.Controls.Add(this.tbFrequency);
      this.Controls.Add(this.lbFrequency);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SkyDefaultTransponderSetup";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Default transponder setup";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel lbFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox tbFrequency;
    private MediaPortal.UserInterface.Controls.MPComboBox cbPolarisation;
    private MediaPortal.UserInterface.Controls.MPTextBox tbSymbolRate;
    private MediaPortal.UserInterface.Controls.MPTextBox tbNetworkId;
    private MediaPortal.UserInterface.Controls.MPComboBox cbFEC;
    private MediaPortal.UserInterface.Controls.MPLabel lbPolarisation;
    private MediaPortal.UserInterface.Controls.MPLabel lbSymbolRate;
    private MediaPortal.UserInterface.Controls.MPLabel lbFEC;
    private MediaPortal.UserInterface.Controls.MPLabel lbNetworkId;
    private MediaPortal.UserInterface.Controls.MPButton btSave;
    private MediaPortal.UserInterface.Controls.MPButton btCancel;
  }
}
