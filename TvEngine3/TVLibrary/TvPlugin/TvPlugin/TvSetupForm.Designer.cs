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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvSetupForm));
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonSelectLanguages = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpTextBoxPreferredLanguages = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTextBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxPrefRebuildGraphAudioChanged = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxavoidSeekingonChannelChange = new System.Windows.Forms.CheckBox();
      this.mpLabel6 = new System.Windows.Forms.Label();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpCheckBoxPrefRebuildGraphVideoChanged = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.toolTipChannelChangeVideoChanged = new System.Windows.Forms.ToolTip(this.components);
      this.toolTipChannelChangeAudioChanged = new System.Windows.Forms.ToolTip(this.components);
      this.toolSeeking = new System.Windows.Forms.ToolTip(this.components);
      this.label1 = new System.Windows.Forms.Label();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
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
      this.mpGroupBox1.Visible = false;
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
      // mpTextBoxPreferredLanguages
      // 
      this.mpTextBoxPreferredLanguages.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxPreferredLanguages.Location = new System.Drawing.Point(124, 22);
      this.mpTextBoxPreferredLanguages.Name = "mpTextBoxPreferredLanguages";
      this.mpTextBoxPreferredLanguages.ReadOnly = true;
      this.mpTextBoxPreferredLanguages.Size = new System.Drawing.Size(133, 20);
      this.mpTextBoxPreferredLanguages.TabIndex = 8;
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
      this.mpGroupBox2.Visible = false;
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
      this.mpButtonOk.Location = new System.Drawing.Point(32, 323);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 6;
      this.mpButtonOk.Text = "Ok";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Visible = false;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(173, 323);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 7;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Visible = false;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.mpCheckBoxPrefRebuildGraphAudioChanged);
      this.mpGroupBox3.Controls.Add(this.mpCheckBoxavoidSeekingonChannelChange);
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel5);
      this.mpGroupBox3.Controls.Add(this.mpCheckBoxPrefRebuildGraphVideoChanged);
      this.mpGroupBox3.Controls.Add(this.mpLabel4);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(13, 183);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(290, 125);
      this.mpGroupBox3.TabIndex = 8;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Channel change preference";
      this.mpGroupBox3.Visible = false;
      // 
      // mpCheckBoxPrefRebuildGraphAudioChanged
      // 
      this.mpCheckBoxPrefRebuildGraphAudioChanged.AutoSize = true;
      this.mpCheckBoxPrefRebuildGraphAudioChanged.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefRebuildGraphAudioChanged.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefRebuildGraphAudioChanged.Location = new System.Drawing.Point(263, 59);
      this.mpCheckBoxPrefRebuildGraphAudioChanged.Name = "mpCheckBoxPrefRebuildGraphAudioChanged";
      this.mpCheckBoxPrefRebuildGraphAudioChanged.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefRebuildGraphAudioChanged.TabIndex = 9;
      this.mpCheckBoxPrefRebuildGraphAudioChanged.UseVisualStyleBackColor = false;
      this.mpCheckBoxPrefRebuildGraphAudioChanged.CheckedChanged += new System.EventHandler(this.mpCheckBoxPrefRebuildGraphOnNewAVSpecs_CheckedChanged);
      // 
      // mpCheckBoxavoidSeekingonChannelChange
      // 
      this.mpCheckBoxavoidSeekingonChannelChange.AutoSize = true;
      this.mpCheckBoxavoidSeekingonChannelChange.Location = new System.Drawing.Point(263, 90);
      this.mpCheckBoxavoidSeekingonChannelChange.Name = "mpCheckBoxavoidSeekingonChannelChange";
      this.mpCheckBoxavoidSeekingonChannelChange.Size = new System.Drawing.Size(15, 14);
      this.mpCheckBoxavoidSeekingonChannelChange.TabIndex = 11;
      this.mpCheckBoxavoidSeekingonChannelChange.UseVisualStyleBackColor = true;
      this.mpCheckBoxavoidSeekingonChannelChange.CheckedChanged += new System.EventHandler(this.mpCheckBoxavoidSeekingonChannelChange_CheckedChanged);
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(16, 90);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(217, 13);
      this.mpLabel6.TabIndex = 10;
      this.mpLabel6.Text = "Try avoiding seeking during channel change";
      this.toolSeeking.SetToolTip(this.mpLabel6, resources.GetString("mpLabel6.ToolTip"));
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(16, 59);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(204, 13);
      this.mpLabel5.TabIndex = 8;
      this.mpLabel5.Text = "Rebuild graph when audio specs change:";
      // 
      // mpCheckBoxPrefRebuildGraphVideoChanged
      // 
      this.mpCheckBoxPrefRebuildGraphVideoChanged.AutoSize = true;
      this.mpCheckBoxPrefRebuildGraphVideoChanged.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefRebuildGraphVideoChanged.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefRebuildGraphVideoChanged.Location = new System.Drawing.Point(263, 24);
      this.mpCheckBoxPrefRebuildGraphVideoChanged.Name = "mpCheckBoxPrefRebuildGraphVideoChanged";
      this.mpCheckBoxPrefRebuildGraphVideoChanged.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefRebuildGraphVideoChanged.TabIndex = 7;
      this.mpCheckBoxPrefRebuildGraphVideoChanged.UseVisualStyleBackColor = false;
      this.mpCheckBoxPrefRebuildGraphVideoChanged.CheckedChanged += new System.EventHandler(this.mpCheckBoxPrefRebuildGraphVideoChanged_CheckedChanged);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(17, 24);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(204, 13);
      this.mpLabel4.TabIndex = 6;
      this.mpLabel4.Text = "Rebuild graph when video specs change:";
      // 
      // toolTipChannelChangeVideoChanged
      // 
      this.toolTipChannelChangeVideoChanged.AutomaticDelay = 0;
      this.toolTipChannelChangeVideoChanged.AutoPopDelay = 20000;
      this.toolTipChannelChangeVideoChanged.InitialDelay = 500;
      this.toolTipChannelChangeVideoChanged.ReshowDelay = 100;
      this.toolTipChannelChangeVideoChanged.ShowAlways = true;
      // 
      // toolTipChannelChangeAudioChanged
      // 
      this.toolTipChannelChangeAudioChanged.AutomaticDelay = 0;
      this.toolTipChannelChangeAudioChanged.AutoPopDelay = 20000;
      this.toolTipChannelChangeAudioChanged.InitialDelay = 500;
      this.toolTipChannelChangeAudioChanged.ReshowDelay = 100;
      this.toolTipChannelChangeAudioChanged.ShowAlways = true;
      // 
      // toolSeeking
      // 
      this.toolSeeking.ToolTipTitle = "Avoid Seeking";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(27, 349);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(262, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Please find the settings under \'Television->TV Client\'...";
      // 
      // TvSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(317, 369);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.mpGroupBox3);
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
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

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
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefRebuildGraphVideoChanged;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.ToolTip toolTipChannelChangeVideoChanged;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefRebuildGraphAudioChanged;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private System.Windows.Forms.ToolTip toolTipChannelChangeAudioChanged;
    private System.Windows.Forms.CheckBox mpCheckBoxavoidSeekingonChannelChange;
    private System.Windows.Forms.Label mpLabel6;
    private System.Windows.Forms.ToolTip toolSeeking;
    private System.Windows.Forms.Label label1;


  }
}