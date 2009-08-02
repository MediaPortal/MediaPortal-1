#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#pragma warning disable 108

namespace SetupTv.Sections
{
  public partial class Project : SetupTv.SectionSettings
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
      this.linkLabelSourceforge = new System.Windows.Forms.LinkLabel();
      this.labelSourceForge = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelOnlineDocumentation = new System.Windows.Forms.LinkLabel();
      this.labelOnlineDocumentation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelIrcChannelData = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelIrcChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelForums = new System.Windows.Forms.LinkLabel();
      this.labelForums = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelHomepage = new System.Windows.Forms.LinkLabel();
      this.labelHomepage = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelPayPal = new System.Windows.Forms.LinkLabel();
      this.labelMePo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxAbout = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelVersion3 = new System.Windows.Forms.Label();
      this.labelVersion2 = new System.Windows.Forms.Label();
      this.labelVersion1 = new System.Windows.Forms.Label();
      this.groupBoxInfo.SuspendLayout();
      this.groupBoxContact.SuspendLayout();
      this.mpGroupBoxAbout.SuspendLayout();
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
      this.labelInfo2.Text = "Clients like MediaPortal can use the TV-Server to watch Live-TV/recordings/EPG ov" +
          "er the network.";
      // 
      // labelInfo1
      // 
      this.labelInfo1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelInfo1.Location = new System.Drawing.Point(16, 24);
      this.labelInfo1.Name = "labelInfo1";
      this.labelInfo1.Size = new System.Drawing.Size(440, 32);
      this.labelInfo1.TabIndex = 0;
      this.labelInfo1.Text = "The TV-Server is an application which allows you to set up a central server with " +
          "multiple TV cards.";
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
      this.groupBoxContact.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxContact.Location = new System.Drawing.Point(211, 112);
      this.groupBoxContact.Name = "groupBoxContact";
      this.groupBoxContact.Size = new System.Drawing.Size(261, 232);
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
      // linkLabelPayPal
      // 
      this.linkLabelPayPal.ActiveLinkColor = System.Drawing.Color.Transparent;
      this.linkLabelPayPal.DisabledLinkColor = System.Drawing.Color.Transparent;
      this.linkLabelPayPal.Image = global::SetupTv.Properties.Resources.logo_PayPal;
      this.linkLabelPayPal.LinkColor = System.Drawing.Color.Transparent;
      this.linkLabelPayPal.Location = new System.Drawing.Point(133, 314);
      this.linkLabelPayPal.Name = "linkLabelPayPal";
      this.linkLabelPayPal.Size = new System.Drawing.Size(72, 29);
      this.linkLabelPayPal.TabIndex = 2;
      this.linkLabelPayPal.TabStop = true;
      this.linkLabelPayPal.Text = "http://www.team-mediaportal.com/donate.html";
      this.linkLabelPayPal.UseCompatibleTextRendering = true;
      this.linkLabelPayPal.VisitedLinkColor = System.Drawing.Color.Transparent;
      this.linkLabelPayPal.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPayPal_LinkClicked);
      // 
      // labelMePo
      // 
      this.labelMePo.Image = global::SetupTv.Properties.Resources.logo_MePo;
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
      this.mpGroupBoxAbout.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxAbout.Location = new System.Drawing.Point(213, 355);
      this.mpGroupBoxAbout.Name = "mpGroupBoxAbout";
      this.mpGroupBoxAbout.Size = new System.Drawing.Size(258, 52);
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
      this.labelVersion3.Size = new System.Drawing.Size(115, 13);
      this.labelVersion3.TabIndex = 2;
      this.labelVersion3.Text = "(SVN for testing only!!!)";
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
      // Project
      // 
      this.Controls.Add(this.mpGroupBoxAbout);
      this.Controls.Add(this.linkLabelPayPal);
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
      this.ResumeLayout(false);

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
    private MediaPortal.UserInterface.Controls.MPLabel labelSourceForge;
    private System.Windows.Forms.LinkLabel linkLabelSourceforge;
    private MediaPortal.UserInterface.Controls.MPLabel labelInfo2;
    private System.Windows.Forms.LinkLabel linkLabelPayPal;
    private MediaPortal.UserInterface.Controls.MPLabel labelMePo;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxAbout;
    private System.Windows.Forms.Label labelVersion3;
    private System.Windows.Forms.Label labelVersion2;
    private System.Windows.Forms.Label labelVersion1;
  }
}
