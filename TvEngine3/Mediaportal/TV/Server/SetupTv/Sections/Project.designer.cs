using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

#pragma warning disable 108

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Project : SectionSettings
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Project));
      this.groupBoxInfo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelInfo1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxContact = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.linkLabelSourceforge = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.labelSourceForge = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.linkLabelOnlineDocumentation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.labelOnlineDocumentation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelIrcChannelData = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelIrcChannel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.linkLabelForums = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.labelForums = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.linkLabelHomepage = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.labelHomepage = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelMePo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpGroupBoxAbout = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelVersion3 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelVersion2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelVersion1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.paypalPictureBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      this.groupBoxInfo.SuspendLayout();
      this.groupBoxContact.SuspendLayout();
      this.mpGroupBoxAbout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.paypalPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxInfo
      // 
      this.groupBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxInfo.Controls.Add(this.labelInfo1);
      this.groupBoxInfo.Location = new System.Drawing.Point(0, 0);
      this.groupBoxInfo.Name = "groupBoxInfo";
      this.groupBoxInfo.Size = new System.Drawing.Size(480, 104);
      this.groupBoxInfo.TabIndex = 0;
      this.groupBoxInfo.TabStop = false;
      // 
      // labelInfo1
      // 
      this.labelInfo1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelInfo1.Location = new System.Drawing.Point(16, 24);
      this.labelInfo1.Name = "labelInfo1";
      this.labelInfo1.Size = new System.Drawing.Size(448, 69);
      this.labelInfo1.TabIndex = 0;
      this.labelInfo1.Text = resources.GetString("labelInfo1.Text");
      // 
      // groupBoxContact
      // 
      this.groupBoxContact.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxContact.Controls.Add(this.linkLabelSourceforge);
      this.groupBoxContact.Controls.Add(this.labelSourceForge);
      this.groupBoxContact.Controls.Add(this.linkLabelOnlineDocumentation);
      this.groupBoxContact.Controls.Add(this.labelOnlineDocumentation);
      this.groupBoxContact.Controls.Add(this.labelIrcChannelData);
      this.groupBoxContact.Controls.Add(this.labelIrcChannel);
      this.groupBoxContact.Controls.Add(this.linkLabelForums);
      this.groupBoxContact.Controls.Add(this.labelForums);
      this.groupBoxContact.Controls.Add(this.linkLabelHomepage);
      this.groupBoxContact.Controls.Add(this.labelHomepage);
      this.groupBoxContact.Location = new System.Drawing.Point(211, 112);
      this.groupBoxContact.Name = "groupBoxContact";
      this.groupBoxContact.Size = new System.Drawing.Size(269, 232);
      this.groupBoxContact.TabIndex = 1;
      this.groupBoxContact.TabStop = false;
      this.groupBoxContact.Text = "Contact";
      // 
      // linkLabelSourceforge
      // 
      this.linkLabelSourceforge.AutoSize = true;
      this.linkLabelSourceforge.Location = new System.Drawing.Point(18, 160);
      this.linkLabelSourceforge.Name = "linkLabelSourceforge";
      this.linkLabelSourceforge.Size = new System.Drawing.Size(213, 13);
      this.linkLabelSourceforge.TabIndex = 7;
      this.linkLabelSourceforge.TabStop = true;
      this.linkLabelSourceforge.Text = "http://sourceforge.net/projects/mediaportal";
      this.linkLabelSourceforge.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
      // 
      // labelSourceForge
      // 
      this.labelSourceForge.AutoSize = true;
      this.labelSourceForge.Location = new System.Drawing.Point(10, 144);
      this.labelSourceForge.Name = "labelSourceForge";
      this.labelSourceForge.Size = new System.Drawing.Size(135, 13);
      this.labelSourceForge.TabIndex = 6;
      this.labelSourceForge.Text = "SourceForge Project Page:";
      // 
      // linkLabelOnlineDocumentation
      // 
      this.linkLabelOnlineDocumentation.AutoSize = true;
      this.linkLabelOnlineDocumentation.Location = new System.Drawing.Point(18, 120);
      this.linkLabelOnlineDocumentation.Name = "linkLabelOnlineDocumentation";
      this.linkLabelOnlineDocumentation.Size = new System.Drawing.Size(162, 13);
      this.linkLabelOnlineDocumentation.TabIndex = 5;
      this.linkLabelOnlineDocumentation.TabStop = true;
      this.linkLabelOnlineDocumentation.Text = "http://wiki.team-mediaportal.com";
      this.linkLabelOnlineDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
      // 
      // labelOnlineDocumentation
      // 
      this.labelOnlineDocumentation.AutoSize = true;
      this.labelOnlineDocumentation.Location = new System.Drawing.Point(10, 105);
      this.labelOnlineDocumentation.Name = "labelOnlineDocumentation";
      this.labelOnlineDocumentation.Size = new System.Drawing.Size(115, 13);
      this.labelOnlineDocumentation.TabIndex = 4;
      this.labelOnlineDocumentation.Text = "Online Documentation:";
      // 
      // labelIrcChannelData
      // 
      this.labelIrcChannelData.AutoSize = true;
      this.labelIrcChannelData.Location = new System.Drawing.Point(18, 200);
      this.labelIrcChannelData.Name = "labelIrcChannelData";
      this.labelIrcChannelData.Size = new System.Drawing.Size(232, 13);
      this.labelIrcChannelData.TabIndex = 9;
      this.labelIrcChannelData.Text = "IRC network: freenode / channel: #MediaPortal";
      // 
      // labelIrcChannel
      // 
      this.labelIrcChannel.AutoSize = true;
      this.labelIrcChannel.Location = new System.Drawing.Point(10, 184);
      this.labelIrcChannel.Name = "labelIrcChannel";
      this.labelIrcChannel.Size = new System.Drawing.Size(105, 13);
      this.labelIrcChannel.TabIndex = 8;
      this.labelIrcChannel.Text = "Official IRC Channel:";
      // 
      // linkLabelForums
      // 
      this.linkLabelForums.AutoSize = true;
      this.linkLabelForums.Location = new System.Drawing.Point(18, 80);
      this.linkLabelForums.Name = "linkLabelForums";
      this.linkLabelForums.Size = new System.Drawing.Size(170, 13);
      this.linkLabelForums.TabIndex = 3;
      this.linkLabelForums.TabStop = true;
      this.linkLabelForums.Text = "http://forum.team-mediaportal.com";
      this.linkLabelForums.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
      // 
      // labelForums
      // 
      this.labelForums.AutoSize = true;
      this.labelForums.Location = new System.Drawing.Point(10, 64);
      this.labelForums.Name = "labelForums";
      this.labelForums.Size = new System.Drawing.Size(44, 13);
      this.labelForums.TabIndex = 2;
      this.labelForums.Text = "Forums:";
      // 
      // linkLabelHomepage
      // 
      this.linkLabelHomepage.AutoSize = true;
      this.linkLabelHomepage.Location = new System.Drawing.Point(18, 40);
      this.linkLabelHomepage.Name = "linkLabelHomepage";
      this.linkLabelHomepage.Size = new System.Drawing.Size(168, 13);
      this.linkLabelHomepage.TabIndex = 1;
      this.linkLabelHomepage.TabStop = true;
      this.linkLabelHomepage.Text = "http://www.team-mediaportal.com";
      this.linkLabelHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // labelHomepage
      // 
      this.labelHomepage.AutoSize = true;
      this.labelHomepage.Location = new System.Drawing.Point(10, 24);
      this.labelHomepage.Name = "labelHomepage";
      this.labelHomepage.Size = new System.Drawing.Size(62, 13);
      this.labelHomepage.TabIndex = 0;
      this.labelHomepage.Text = "Homepage:";
      // 
      // labelMePo
      // 
      this.labelMePo.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.logo_MePo;
      this.labelMePo.Location = new System.Drawing.Point(16, 112);
      this.labelMePo.Name = "labelMePo";
      this.labelMePo.Size = new System.Drawing.Size(164, 239);
      this.labelMePo.TabIndex = 9;
      // 
      // mpGroupBoxAbout
      // 
      this.mpGroupBoxAbout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBoxAbout.Controls.Add(this.labelVersion3);
      this.mpGroupBoxAbout.Controls.Add(this.labelVersion2);
      this.mpGroupBoxAbout.Controls.Add(this.labelVersion1);
      this.mpGroupBoxAbout.Location = new System.Drawing.Point(213, 355);
      this.mpGroupBoxAbout.Name = "mpGroupBoxAbout";
      this.mpGroupBoxAbout.Size = new System.Drawing.Size(266, 52);
      this.mpGroupBoxAbout.TabIndex = 10;
      this.mpGroupBoxAbout.TabStop = false;
      this.mpGroupBoxAbout.Text = "About";
      // 
      // labelVersion3
      // 
      this.labelVersion3.AutoSize = true;
      this.labelVersion3.ForeColor = System.Drawing.Color.Red;
      this.labelVersion3.Location = new System.Drawing.Point(122, 20);
      this.labelVersion3.Name = "labelVersion3";
      this.labelVersion3.Size = new System.Drawing.Size(164, 13);
      this.labelVersion3.TabIndex = 2;
      this.labelVersion3.Text = "(Snapshot-Build for testing only!!!)";
      // 
      // labelVersion2
      // 
      this.labelVersion2.AutoSize = true;
      this.labelVersion2.Location = new System.Drawing.Point(59, 20);
      this.labelVersion2.Name = "labelVersion2";
      this.labelVersion2.Size = new System.Drawing.Size(62, 13);
      this.labelVersion2.TabIndex = 1;
      this.labelVersion2.Text = "1.0.3.XXXX";
      // 
      // labelVersion1
      // 
      this.labelVersion1.AutoSize = true;
      this.labelVersion1.Location = new System.Drawing.Point(15, 20);
      this.labelVersion1.Name = "labelVersion1";
      this.labelVersion1.Size = new System.Drawing.Size(45, 13);
      this.labelVersion1.TabIndex = 0;
      this.labelVersion1.Text = "Version:";
      // 
      // paypalPictureBox
      // 
      this.paypalPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
      this.paypalPictureBox.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.logo_PayPal;
      this.paypalPictureBox.Location = new System.Drawing.Point(133, 315);
      this.paypalPictureBox.Name = "paypalPictureBox";
      this.paypalPictureBox.Size = new System.Drawing.Size(72, 29);
      this.paypalPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.paypalPictureBox.TabIndex = 11;
      this.paypalPictureBox.TabStop = false;
      this.paypalPictureBox.Click += new System.EventHandler(this.paypalPictureBox_Click);
      // 
      // Project
      // 
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.paypalPictureBox);
      this.Controls.Add(this.mpGroupBoxAbout);
      this.Controls.Add(this.groupBoxContact);
      this.Controls.Add(this.groupBoxInfo);
      this.Controls.Add(this.labelMePo);
      this.Name = "Project";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxInfo.ResumeLayout(false);
      this.groupBoxContact.ResumeLayout(false);
      this.groupBoxContact.PerformLayout();
      this.mpGroupBoxAbout.ResumeLayout(false);
      this.mpGroupBoxAbout.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.paypalPictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPGroupBox groupBoxInfo;
    private MPLabel labelInfo1;
    private MPGroupBox groupBoxContact;
    private MPLabel labelIrcChannelData;
    private MPLabel labelIrcChannel;
    private MPLinkLabel linkLabelForums;
    private MPLabel labelForums;
    private MPLinkLabel linkLabelHomepage;
    private MPLabel labelHomepage;
    private MPLabel labelOnlineDocumentation;
    private MPLinkLabel linkLabelOnlineDocumentation;
    private MPLabel labelSourceForge;
    private MPLinkLabel linkLabelSourceforge;
    private MPLabel labelMePo;
    private MPGroupBox mpGroupBoxAbout;
    private MPLabel labelVersion3;
    private MPLabel labelVersion2;
    private MPLabel labelVersion1;
    private MPPictureBox paypalPictureBox;
  }
}
