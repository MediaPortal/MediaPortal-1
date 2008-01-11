#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using MediaPortal.Util;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralOSD : MediaPortal.Configuration.SectionSettings
  {
    private System.ComponentModel.IContainer components = null;

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxOSD;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDisplayTimeout;
    private MediaPortal.UserInterface.Controls.MPLabel labelDisplayTimeout;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlGeneralOSD;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageOSD;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxZapOSD;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxZapDelay;
    private MediaPortal.UserInterface.Controls.MPLabel labelZapDelay;
    private MediaPortal.UserInterface.Controls.MPLabel labelZapTimeOut;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxZapTimeout;

    public GeneralOSD()
      : this("On-Screen Display")
    {
    }

    public GeneralOSD(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        textBoxDisplayTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 0));
        textBoxZapDelay.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2));
        textBoxZapTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zaptimeout", 5));
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("movieplayer", "osdtimeout", textBoxDisplayTimeout.Text);
        xmlwriter.SetValue("movieplayer", "zapdelay", textBoxZapDelay.Text);
        xmlwriter.SetValue("movieplayer", "zaptimeout", textBoxZapTimeout.Text);
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tabControlGeneralOSD = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageOSD = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxZapOSD = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBoxZapTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelZapTimeOut = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxZapDelay = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelZapDelay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxOSD = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBoxDisplayTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDisplayTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControlGeneralOSD.SuspendLayout();
      this.tabPageOSD.SuspendLayout();
      this.groupBoxZapOSD.SuspendLayout();
      this.groupBoxOSD.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlGeneralOSD
      // 
      this.tabControlGeneralOSD.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlGeneralOSD.Controls.Add(this.tabPageOSD);
      this.tabControlGeneralOSD.Location = new System.Drawing.Point(0, 0);
      this.tabControlGeneralOSD.Name = "tabControlGeneralOSD";
      this.tabControlGeneralOSD.SelectedIndex = 0;
      this.tabControlGeneralOSD.Size = new System.Drawing.Size(472, 408);
      this.tabControlGeneralOSD.TabIndex = 0;
      // 
      // tabPageOSD
      // 
      this.tabPageOSD.Controls.Add(this.groupBoxZapOSD);
      this.tabPageOSD.Controls.Add(this.groupBoxOSD);
      this.tabPageOSD.Location = new System.Drawing.Point(4, 22);
      this.tabPageOSD.Name = "tabPageOSD";
      this.tabPageOSD.Size = new System.Drawing.Size(464, 382);
      this.tabPageOSD.TabIndex = 3;
      this.tabPageOSD.Text = "On-Screen Display";
      this.tabPageOSD.UseVisualStyleBackColor = true;
      // 
      // groupBoxZapOSD
      // 
      this.groupBoxZapOSD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxZapOSD.Controls.Add(this.textBoxZapTimeout);
      this.groupBoxZapOSD.Controls.Add(this.labelZapTimeOut);
      this.groupBoxZapOSD.Controls.Add(this.textBoxZapDelay);
      this.groupBoxZapOSD.Controls.Add(this.labelZapDelay);
      this.groupBoxZapOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxZapOSD.Location = new System.Drawing.Point(16, 72);
      this.groupBoxZapOSD.Name = "groupBoxZapOSD";
      this.groupBoxZapOSD.Size = new System.Drawing.Size(432, 74);
      this.groupBoxZapOSD.TabIndex = 1;
      this.groupBoxZapOSD.TabStop = false;
      this.groupBoxZapOSD.Text = "Zap On-Screen-Display";
      // 
      // textBoxZapTimeout
      // 
      this.textBoxZapTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxZapTimeout.BorderColor = System.Drawing.Color.Empty;
      this.textBoxZapTimeout.Location = new System.Drawing.Point(160, 44);
      this.textBoxZapTimeout.Name = "textBoxZapTimeout";
      this.textBoxZapTimeout.Size = new System.Drawing.Size(256, 21);
      this.textBoxZapTimeout.TabIndex = 3;
      // 
      // labelZapTimeOut
      // 
      this.labelZapTimeOut.AutoSize = true;
      this.labelZapTimeOut.Location = new System.Drawing.Point(16, 48);
      this.labelZapTimeOut.Name = "labelZapTimeOut";
      this.labelZapTimeOut.Size = new System.Drawing.Size(99, 13);
      this.labelZapTimeOut.TabIndex = 2;
      this.labelZapTimeOut.Text = "Zap timeout (sec.):";
      // 
      // textBoxZapDelay
      // 
      this.textBoxZapDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxZapDelay.BorderColor = System.Drawing.Color.Empty;
      this.textBoxZapDelay.Location = new System.Drawing.Point(160, 20);
      this.textBoxZapDelay.Name = "textBoxZapDelay";
      this.textBoxZapDelay.Size = new System.Drawing.Size(256, 21);
      this.textBoxZapDelay.TabIndex = 1;
      // 
      // labelZapDelay
      // 
      this.labelZapDelay.AutoSize = true;
      this.labelZapDelay.Location = new System.Drawing.Point(16, 24);
      this.labelZapDelay.Name = "labelZapDelay";
      this.labelZapDelay.Size = new System.Drawing.Size(89, 13);
      this.labelZapDelay.TabIndex = 0;
      this.labelZapDelay.Text = "Zap delay (sec.):";
      // 
      // groupBoxOSD
      // 
      this.groupBoxOSD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxOSD.Controls.Add(this.textBoxDisplayTimeout);
      this.groupBoxOSD.Controls.Add(this.labelDisplayTimeout);
      this.groupBoxOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOSD.Location = new System.Drawing.Point(16, 16);
      this.groupBoxOSD.Name = "groupBoxOSD";
      this.groupBoxOSD.Size = new System.Drawing.Size(432, 50);
      this.groupBoxOSD.TabIndex = 0;
      this.groupBoxOSD.TabStop = false;
      this.groupBoxOSD.Text = "On-Screen-Display";
      // 
      // textBoxDisplayTimeout
      // 
      this.textBoxDisplayTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDisplayTimeout.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDisplayTimeout.Location = new System.Drawing.Point(160, 20);
      this.textBoxDisplayTimeout.Name = "textBoxDisplayTimeout";
      this.textBoxDisplayTimeout.Size = new System.Drawing.Size(256, 21);
      this.textBoxDisplayTimeout.TabIndex = 1;
      // 
      // labelDisplayTimeout
      // 
      this.labelDisplayTimeout.AutoSize = true;
      this.labelDisplayTimeout.Location = new System.Drawing.Point(16, 24);
      this.labelDisplayTimeout.Name = "labelDisplayTimeout";
      this.labelDisplayTimeout.Size = new System.Drawing.Size(115, 13);
      this.labelDisplayTimeout.TabIndex = 0;
      this.labelDisplayTimeout.Text = "Display timeout (sec.):";
      // 
      // GeneralOSD
      // 
      this.Controls.Add(this.tabControlGeneralOSD);
      this.Name = "GeneralOSD";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControlGeneralOSD.ResumeLayout(false);
      this.tabPageOSD.ResumeLayout(false);
      this.groupBoxZapOSD.ResumeLayout(false);
      this.groupBoxZapOSD.PerformLayout();
      this.groupBoxOSD.ResumeLayout(false);
      this.groupBoxOSD.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion
  }
}