#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxNotifyTV;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxNotifyTimeoutVal;
    private MediaPortal.UserInterface.Controls.MPLabel labelNotifyTimeout;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxNotifyPlaySound;
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        textBoxDisplayTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 0));
        textBoxZapDelay.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2));
        textBoxZapTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zaptimeout", 5));
        textBoxNotifyTimeoutVal.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "notifyTVTimeout", 15));
        checkBoxNotifyPlaySound.Checked = xmlreader.GetValueAsBool("movieplayer", "playNotifyBeep", true);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("movieplayer", "osdtimeout", textBoxDisplayTimeout.Text);
        xmlwriter.SetValue("movieplayer", "zapdelay", textBoxZapDelay.Text);
        xmlwriter.SetValue("movieplayer", "zaptimeout", textBoxZapTimeout.Text);
        xmlwriter.SetValue("movieplayer", "notifyTVTimeout", textBoxNotifyTimeoutVal.Text);
        xmlwriter.SetValue("movieplayer", "playNotifyBeep", checkBoxNotifyPlaySound.Checked);
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
      this.groupBoxNotifyTV = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxNotifyPlaySound = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.textBoxNotifyTimeoutVal = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelNotifyTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
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
      this.groupBoxNotifyTV.SuspendLayout();
      this.groupBoxZapOSD.SuspendLayout();
      this.groupBoxOSD.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlGeneralOSD
      // 
      this.tabControlGeneralOSD.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                  | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.tabControlGeneralOSD.Controls.Add(this.tabPageOSD);
      this.tabControlGeneralOSD.Location = new System.Drawing.Point(0, 0);
      this.tabControlGeneralOSD.Name = "tabControlGeneralOSD";
      this.tabControlGeneralOSD.SelectedIndex = 0;
      this.tabControlGeneralOSD.Size = new System.Drawing.Size(472, 408);
      this.tabControlGeneralOSD.TabIndex = 0;
      // 
      // tabPageOSD
      // 
      this.tabPageOSD.Controls.Add(this.groupBoxNotifyTV);
      this.tabPageOSD.Controls.Add(this.groupBoxZapOSD);
      this.tabPageOSD.Controls.Add(this.groupBoxOSD);
      this.tabPageOSD.Location = new System.Drawing.Point(4, 22);
      this.tabPageOSD.Name = "tabPageOSD";
      this.tabPageOSD.Size = new System.Drawing.Size(464, 382);
      this.tabPageOSD.TabIndex = 3;
      this.tabPageOSD.Text = "On-Screen Display";
      this.tabPageOSD.UseVisualStyleBackColor = true;
      // 
      // groupBoxNotifyTV
      // 
      this.groupBoxNotifyTV.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.groupBoxNotifyTV.Controls.Add(this.checkBoxNotifyPlaySound);
      this.groupBoxNotifyTV.Controls.Add(this.textBoxNotifyTimeoutVal);
      this.groupBoxNotifyTV.Controls.Add(this.labelNotifyTimeout);
      this.groupBoxNotifyTV.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxNotifyTV.Location = new System.Drawing.Point(16, 166);
      this.groupBoxNotifyTV.Name = "groupBoxNotifyTV";
      this.groupBoxNotifyTV.Size = new System.Drawing.Size(432, 74);
      this.groupBoxNotifyTV.TabIndex = 2;
      this.groupBoxNotifyTV.TabStop = false;
      this.groupBoxNotifyTV.Text = "Program start notification";
      // 
      // checkBoxNotifyPlaySound
      // 
      this.checkBoxNotifyPlaySound.AutoSize = true;
      this.checkBoxNotifyPlaySound.Checked = true;
      this.checkBoxNotifyPlaySound.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxNotifyPlaySound.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxNotifyPlaySound.Location = new System.Drawing.Point(19, 46);
      this.checkBoxNotifyPlaySound.Name = "checkBoxNotifyPlaySound";
      this.checkBoxNotifyPlaySound.Size = new System.Drawing.Size(105, 17);
      this.checkBoxNotifyPlaySound.TabIndex = 2;
      this.checkBoxNotifyPlaySound.Text = "Play \"notify.wav\"";
      this.checkBoxNotifyPlaySound.UseVisualStyleBackColor = true;
      // 
      // textBoxNotifyTimeoutVal
      // 
      this.textBoxNotifyTimeoutVal.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxNotifyTimeoutVal.BorderColor = System.Drawing.Color.Empty;
      this.textBoxNotifyTimeoutVal.Location = new System.Drawing.Point(160, 20);
      this.textBoxNotifyTimeoutVal.Name = "textBoxNotifyTimeoutVal";
      this.textBoxNotifyTimeoutVal.Size = new System.Drawing.Size(256, 20);
      this.textBoxNotifyTimeoutVal.TabIndex = 1;
      this.textBoxNotifyTimeoutVal.Text = "15";
      // 
      // labelNotifyTimeout
      // 
      this.labelNotifyTimeout.AutoSize = true;
      this.labelNotifyTimeout.Location = new System.Drawing.Point(16, 24);
      this.labelNotifyTimeout.Name = "labelNotifyTimeout";
      this.labelNotifyTimeout.Size = new System.Drawing.Size(127, 13);
      this.labelNotifyTimeout.TabIndex = 0;
      this.labelNotifyTimeout.Text = "notification timeout (sec.):";
      // 
      // groupBoxZapOSD
      // 
      this.groupBoxZapOSD.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.textBoxZapTimeout.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxZapTimeout.BorderColor = System.Drawing.Color.Empty;
      this.textBoxZapTimeout.Location = new System.Drawing.Point(160, 44);
      this.textBoxZapTimeout.Name = "textBoxZapTimeout";
      this.textBoxZapTimeout.Size = new System.Drawing.Size(256, 20);
      this.textBoxZapTimeout.TabIndex = 3;
      // 
      // labelZapTimeOut
      // 
      this.labelZapTimeOut.AutoSize = true;
      this.labelZapTimeOut.Location = new System.Drawing.Point(16, 48);
      this.labelZapTimeOut.Name = "labelZapTimeOut";
      this.labelZapTimeOut.Size = new System.Drawing.Size(95, 13);
      this.labelZapTimeOut.TabIndex = 2;
      this.labelZapTimeOut.Text = "Zap timeout (sec.):";
      // 
      // textBoxZapDelay
      // 
      this.textBoxZapDelay.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxZapDelay.BorderColor = System.Drawing.Color.Empty;
      this.textBoxZapDelay.Location = new System.Drawing.Point(160, 20);
      this.textBoxZapDelay.Name = "textBoxZapDelay";
      this.textBoxZapDelay.Size = new System.Drawing.Size(256, 20);
      this.textBoxZapDelay.TabIndex = 1;
      // 
      // labelZapDelay
      // 
      this.labelZapDelay.AutoSize = true;
      this.labelZapDelay.Location = new System.Drawing.Point(16, 24);
      this.labelZapDelay.Name = "labelZapDelay";
      this.labelZapDelay.Size = new System.Drawing.Size(86, 13);
      this.labelZapDelay.TabIndex = 0;
      this.labelZapDelay.Text = "Zap delay (sec.):";
      // 
      // groupBoxOSD
      // 
      this.groupBoxOSD.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.textBoxDisplayTimeout.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxDisplayTimeout.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDisplayTimeout.Location = new System.Drawing.Point(160, 20);
      this.textBoxDisplayTimeout.Name = "textBoxDisplayTimeout";
      this.textBoxDisplayTimeout.Size = new System.Drawing.Size(256, 20);
      this.textBoxDisplayTimeout.TabIndex = 1;
      // 
      // labelDisplayTimeout
      // 
      this.labelDisplayTimeout.AutoSize = true;
      this.labelDisplayTimeout.Location = new System.Drawing.Point(16, 24);
      this.labelDisplayTimeout.Name = "labelDisplayTimeout";
      this.labelDisplayTimeout.Size = new System.Drawing.Size(110, 13);
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
      this.groupBoxNotifyTV.ResumeLayout(false);
      this.groupBoxNotifyTV.PerformLayout();
      this.groupBoxZapOSD.ResumeLayout(false);
      this.groupBoxZapOSD.PerformLayout();
      this.groupBoxOSD.ResumeLayout(false);
      this.groupBoxOSD.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion
  }
}