using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace MediaPortal.Configuration.Sections
{
  public class TVClient : MediaPortal.Configuration.SectionSettings
  {
    #region variables
    private string _serverHostName;
    private bool _preferAC3;
    private bool _avoidSeeking;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private CheckBox mpCheckBoxavoidSeekingonChannelChange;
    private Label mpLabel6;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxHostname;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefAC3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private int pluginVersion;
    #endregion

    public TVClient()
      : this("TV Client")
    {
    }

    public TVClient(string name)
      : base(name)
    {
      InitializeComponent();
    }


    private void TVClient_Load(object sender, EventArgs e)
    {
      LoadSettings();
      mpTextBoxHostname.Text = _serverHostName;
      mpCheckBoxPrefAC3.Checked = _preferAC3;
      mpCheckBoxavoidSeekingonChannelChange.Checked = _avoidSeeking;
    }

    public override void LoadSettings()
    {
      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
        this.Enabled = true;
      else this.Enabled = false;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _serverHostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        _avoidSeeking = xmlreader.GetValueAsBool("tvservice", "avoidSeeking", false);
      }
    }
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("tvservice", "hostname", _serverHostName);
        xmlreader.SetValueAsBool("tvservice", "preferac3", _preferAC3);            
        xmlreader.SetValueAsBool("tvservice", "avoidSeeking", _avoidSeeking );        
      }
    }

    private void InitializeComponent()
    {
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxavoidSeekingonChannelChange = new System.Windows.Forms.CheckBox();
      this.mpLabel6 = new System.Windows.Forms.Label();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTextBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox3.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.mpCheckBoxavoidSeekingonChannelChange);
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(10, 158);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(290, 67);
      this.mpGroupBox3.TabIndex = 11;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Channel change preference";
      // 
      // mpCheckBoxavoidSeekingonChannelChange
      // 
      this.mpCheckBoxavoidSeekingonChannelChange.AutoSize = true;
      this.mpCheckBoxavoidSeekingonChannelChange.Location = new System.Drawing.Point(264, 27);
      this.mpCheckBoxavoidSeekingonChannelChange.Name = "mpCheckBoxavoidSeekingonChannelChange";
      this.mpCheckBoxavoidSeekingonChannelChange.Size = new System.Drawing.Size(15, 14);
      this.mpCheckBoxavoidSeekingonChannelChange.TabIndex = 11;
      this.mpCheckBoxavoidSeekingonChannelChange.UseVisualStyleBackColor = true;
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(17, 27);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(217, 13);
      this.mpLabel6.TabIndex = 10;
      this.mpLabel6.Text = "Try avoiding seeking during channel change";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(8, 12);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(292, 63);
      this.mpGroupBox2.TabIndex = 10;
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
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(10, 81);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(290, 60);
      this.mpGroupBox1.TabIndex = 9;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio stream preference";
      // 
      // mpCheckBoxPrefAC3
      // 
      this.mpCheckBoxPrefAC3.AutoSize = true;
      this.mpCheckBoxPrefAC3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAC3.Location = new System.Drawing.Point(124, 29);
      this.mpCheckBoxPrefAC3.Name = "mpCheckBoxPrefAC3";
      this.mpCheckBoxPrefAC3.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefAC3.TabIndex = 7;
      this.mpCheckBoxPrefAC3.UseVisualStyleBackColor = false;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(17, 29);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(61, 13);
      this.mpLabel2.TabIndex = 6;
      this.mpLabel2.Text = "Prefer AC3:";
      // 
      // TVClient
      // 
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "TVClient";
      this.Size = new System.Drawing.Size(314, 244);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
  }
}
