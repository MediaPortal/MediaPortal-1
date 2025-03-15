#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class Project : MediaPortal.Configuration.SectionSettings
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
      this.groupBoxInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelInfo2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelInfo1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxContact = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelGithub = new System.Windows.Forms.LinkLabel();
      this.labelGithub = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelOnlineDocumentation = new System.Windows.Forms.LinkLabel();
      this.labelOnlineDocumentation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelIrcChannelData = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelIrcChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelForums = new System.Windows.Forms.LinkLabel();
      this.labelForums = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelHomepage = new System.Windows.Forms.LinkLabel();
      this.labelHomepage = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelMePo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxAbout = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelVersion2 = new System.Windows.Forms.Label();
      this.labelVersion1 = new System.Windows.Forms.Label();
      this.paypalPictureBox = new System.Windows.Forms.PictureBox();
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
      this.groupBoxInfo.Controls.Add(this.labelInfo2);
      this.groupBoxInfo.Controls.Add(this.labelInfo1);
      this.groupBoxInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxInfo.Location = new System.Drawing.Point(0, 0);
      this.groupBoxInfo.Name = "groupBoxInfo";
      this.groupBoxInfo.Size = new System.Drawing.Size(472, 104);
      this.groupBoxInfo.TabIndex = 0;
      this.groupBoxInfo.TabStop = false;
      // 
      // labelInfo2
      // 
      this.labelInfo2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelInfo2.Location = new System.Drawing.Point(16, 56);
      this.labelInfo2.Name = "labelInfo2";
      this.labelInfo2.Size = new System.Drawing.Size(440, 30);
      this.labelInfo2.TabIndex = 1;
      this.labelInfo2.Text = "It allows you to listen to your favorite music and radio, watch your videos and D" +
    "VDs, view, schedule and record live TV and much more!";
      // 
      // labelInfo1
      // 
      this.labelInfo1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelInfo1.Location = new System.Drawing.Point(16, 24);
      this.labelInfo1.Name = "labelInfo1";
      this.labelInfo1.Size = new System.Drawing.Size(440, 32);
      this.labelInfo1.TabIndex = 0;
      this.labelInfo1.Text = "MediaPortal is an open source project, hosted at Github, that will turn your" +
    " home computer into a fully fledged multi media center (HTPC).";
      // 
      // groupBoxContact
      // 
      this.groupBoxContact.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxContact.Controls.Add(this.linkLabelGithub);
      this.groupBoxContact.Controls.Add(this.labelGithub);
      this.groupBoxContact.Controls.Add(this.linkLabelOnlineDocumentation);
      this.groupBoxContact.Controls.Add(this.labelOnlineDocumentation);
      this.groupBoxContact.Controls.Add(this.labelIrcChannelData);
      this.groupBoxContact.Controls.Add(this.labelIrcChannel);
      this.groupBoxContact.Controls.Add(this.linkLabelForums);
      this.groupBoxContact.Controls.Add(this.labelForums);
      this.groupBoxContact.Controls.Add(this.linkLabelHomepage);
      this.groupBoxContact.Controls.Add(this.labelHomepage);
      this.groupBoxContact.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxContact.Location = new System.Drawing.Point(211, 112);
      this.groupBoxContact.Name = "groupBoxContact";
      this.groupBoxContact.Size = new System.Drawing.Size(261, 232);
      this.groupBoxContact.TabIndex = 1;
      this.groupBoxContact.TabStop = false;
      this.groupBoxContact.Text = "Contact";
      // 
      // linkLabelGithub
      // 
      this.linkLabelGithub.AutoSize = true;
      this.linkLabelGithub.Location = new System.Drawing.Point(18, 160);
      this.linkLabelGithub.Name = "linkLabelGithub";
      this.linkLabelGithub.Size = new System.Drawing.Size(223, 13);
      this.linkLabelGithub.TabIndex = 7;
      this.linkLabelGithub.TabStop = true;
      this.linkLabelGithub.Text = "https://github.com/MediaPortal/MediaPortal-1";
      this.linkLabelGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
      // 
      // labelGithub
      // 
      this.labelGithub.AutoSize = true;
      this.labelGithub.Location = new System.Drawing.Point(10, 144);
      this.labelGithub.Name = "labelGithub";
      this.labelGithub.Size = new System.Drawing.Size(135, 13);
      this.labelGithub.TabIndex = 6;
      this.labelGithub.Text = "Github Project Page:";
      // 
      // linkLabelOnlineDocumentation
      // 
      this.linkLabelOnlineDocumentation.AutoSize = true;
      this.linkLabelOnlineDocumentation.Location = new System.Drawing.Point(18, 120);
      this.linkLabelOnlineDocumentation.Name = "linkLabelOnlineDocumentation";
      this.linkLabelOnlineDocumentation.Size = new System.Drawing.Size(201, 13);
      this.linkLabelOnlineDocumentation.TabIndex = 5;
      this.linkLabelOnlineDocumentation.TabStop = true;
      this.linkLabelOnlineDocumentation.Text = "https://www.team-mediaportal.com/wiki/";
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
      this.linkLabelForums.Size = new System.Drawing.Size(175, 13);
      this.linkLabelForums.TabIndex = 3;
      this.linkLabelForums.TabStop = true;
      this.linkLabelForums.Text = "https://forum.team-mediaportal.com";
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
      this.linkLabelHomepage.Size = new System.Drawing.Size(173, 13);
      this.linkLabelHomepage.TabIndex = 1;
      this.linkLabelHomepage.TabStop = true;
      this.linkLabelHomepage.Text = "https://www.team-mediaportal.com";
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
      this.labelMePo.Image = global::MediaPortal.Configuration.Properties.Resources.mepo_donation;
      this.labelMePo.Location = new System.Drawing.Point(-3, 112);
      this.labelMePo.Name = "labelMePo";
      this.labelMePo.Size = new System.Drawing.Size(215, 256);
      this.labelMePo.TabIndex = 9;
      // 
      // mpGroupBoxAbout
      // 
      this.mpGroupBoxAbout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBoxAbout.Controls.Add(this.labelVersion2);
      this.mpGroupBoxAbout.Controls.Add(this.labelVersion1);
      this.mpGroupBoxAbout.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxAbout.Location = new System.Drawing.Point(210, 355);
      this.mpGroupBoxAbout.Name = "mpGroupBoxAbout";
      this.mpGroupBoxAbout.Size = new System.Drawing.Size(261, 52);
      this.mpGroupBoxAbout.TabIndex = 10;
      this.mpGroupBoxAbout.TabStop = false;
      this.mpGroupBoxAbout.Text = "About";
      // 
      // labelVersion2
      // 
      this.labelVersion2.AutoSize = true;
      this.labelVersion2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.labelVersion2.Location = new System.Drawing.Point(59, 20);
      this.labelVersion2.Name = "labelVersion2";
      this.labelVersion2.Size = new System.Drawing.Size(75, 13);
      this.labelVersion2.TabIndex = 1;
      this.labelVersion2.Text = "1.0.3.23400";
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
      this.paypalPictureBox.Image = global::MediaPortal.Configuration.Properties.Resources.logo_PayPal;
      this.paypalPictureBox.Location = new System.Drawing.Point(64, 359);
      this.paypalPictureBox.Name = "paypalPictureBox";
      this.paypalPictureBox.Size = new System.Drawing.Size(72, 29);
      this.paypalPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.paypalPictureBox.TabIndex = 10;
      this.paypalPictureBox.TabStop = false;
      this.paypalPictureBox.Click += new System.EventHandler(this.paypalPictureBox_Click);
      // 
      // Project
      // 
      this.Controls.Add(this.paypalPictureBox);
      this.Controls.Add(this.mpGroupBoxAbout);
      this.Controls.Add(this.groupBoxContact);
      this.Controls.Add(this.groupBoxInfo);
      this.Controls.Add(this.labelMePo);
      this.Name = "Project";
      this.Size = new System.Drawing.Size(472, 408);
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

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxInfo;
    private MediaPortal.UserInterface.Controls.MPLabel labelInfo1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxContact;
    private MediaPortal.UserInterface.Controls.MPLabel labelIrcChannelData;
    private MediaPortal.UserInterface.Controls.MPLabel labelIrcChannel;
    private System.Windows.Forms.LinkLabel linkLabelForums;
    private MediaPortal.UserInterface.Controls.MPLabel labelForums;
    private System.Windows.Forms.LinkLabel linkLabelHomepage;
    private MediaPortal.UserInterface.Controls.MPLabel labelHomepage;
    private MediaPortal.UserInterface.Controls.MPLabel labelOnlineDocumentation;
    private System.Windows.Forms.LinkLabel linkLabelOnlineDocumentation;
    private MediaPortal.UserInterface.Controls.MPLabel labelGithub;
    private System.Windows.Forms.LinkLabel linkLabelGithub;
    private MediaPortal.UserInterface.Controls.MPLabel labelInfo2;
    private MediaPortal.UserInterface.Controls.MPLabel labelMePo;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxAbout;
    private System.Windows.Forms.Label labelVersion2;
    private System.Windows.Forms.Label labelVersion1;
    private System.Windows.Forms.PictureBox paypalPictureBox;
  }
}
