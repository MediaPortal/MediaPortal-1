namespace TvPlugin
{
  partial class TvSetupForm
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
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTextBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpTextBoxPreferredLanguages = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpButtonSelectLanguages = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpButtonSelectLanguages);
      this.mpGroupBox1.Controls.Add(this.mpTextBoxPreferredLanguages);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(15, 81);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(290, 96);
      this.mpGroupBox1.TabIndex = 4;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio stream preference";
      // 
      // mpCheckBoxPrefAC3
      // 
      this.mpCheckBoxPrefAC3.AutoSize = true;
      this.mpCheckBoxPrefAC3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAC3.Location = new System.Drawing.Point(124, 64);
      this.mpCheckBoxPrefAC3.Name = "mpCheckBoxPrefAC3";
      this.mpCheckBoxPrefAC3.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefAC3.TabIndex = 7;
      this.mpCheckBoxPrefAC3.UseVisualStyleBackColor = false;
      this.mpCheckBoxPrefAC3.CheckedChanged += new System.EventHandler(this.mpCheckBoxPrefAC3_CheckedChanged);
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(17, 64);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(61, 13);
      this.mpLabel2.TabIndex = 6;
      this.mpLabel2.Text = "Prefer AC3:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(17, 29);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(105, 13);
      this.mpLabel1.TabIndex = 4;
      this.mpLabel1.Text = "Preferred languages:";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(13, 12);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(292, 63);
      this.mpGroupBox2.TabIndex = 5;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "TvServer";
      // 
      // mpTextBoxHostname
      // 
      this.mpTextBoxHostname.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxHostname.Location = new System.Drawing.Point(126, 25);
      this.mpTextBoxHostname.Name = "mpTextBoxHostname";
      this.mpTextBoxHostname.Size = new System.Drawing.Size(161, 20);
      this.mpTextBoxHostname.TabIndex = 6;
      this.mpTextBoxHostname.TextChanged += new System.EventHandler(this.mpTextBoxHostname_TextChanged);
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(19, 25);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(58, 13);
      this.mpLabel3.TabIndex = 5;
      this.mpLabel3.Text = "Hostname:";
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.mpButtonOk.Location = new System.Drawing.Point(35, 197);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 6;
      this.mpButtonOk.Text = "Ok";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(165, 197);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 7;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      // 
      // mpTextBoxPreferredLanguages
      // 
      this.mpTextBoxPreferredLanguages.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxPreferredLanguages.Location = new System.Drawing.Point(124, 22);
      this.mpTextBoxPreferredLanguages.Name = "mpTextBoxPreferredLanguages";
      this.mpTextBoxPreferredLanguages.ReadOnly = true;
      this.mpTextBoxPreferredLanguages.Size = new System.Drawing.Size(133, 20);
      this.mpTextBoxPreferredLanguages.TabIndex = 8;
      // 
      // mpButtonSelectLanguages
      // 
      this.mpButtonSelectLanguages.Location = new System.Drawing.Point(261, 20);
      this.mpButtonSelectLanguages.Name = "mpButtonSelectLanguages";
      this.mpButtonSelectLanguages.Size = new System.Drawing.Size(24, 23);
      this.mpButtonSelectLanguages.TabIndex = 9;
      this.mpButtonSelectLanguages.Text = "...";
      this.mpButtonSelectLanguages.UseVisualStyleBackColor = true;
      this.mpButtonSelectLanguages.Click += new System.EventHandler(this.mpButtonSelectLanguages_Click);
      // 
      // TvSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(317, 239);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "TvSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MyTV Setup";
      this.Load += new System.EventHandler(this.TvSetupForm_Load);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefAC3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxHostname;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOk;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonSelectLanguages;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxPreferredLanguages;


  }
}