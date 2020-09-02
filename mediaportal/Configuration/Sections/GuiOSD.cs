#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GuiOSD : SectionSettings
  {
    private MPGroupBox groupBoxZapOSD;
    private MPTextBox textBoxZapTimeout;
    private MPLabel labelZapTimeOut;
    private MPTextBox textBoxZapDelay;
    private MPLabel labelZapDelay;
    private MPGroupBox groupBoxOSD;
    private MPTextBox textBoxDisplayTimeout;
    private MPLabel labelDisplayTimeout;
    private MPLabel labelZapKeyTimeOut;
    private MPTextBox textBoxZapKeyTimeout;
    private IContainer components = null;

    public GuiOSD()
      : this("On-Screen Display") {}

    public GuiOSD(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        textBoxDisplayTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 0));
        textBoxZapDelay.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2));
        textBoxZapTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zaptimeout", 5));
        textBoxZapKeyTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zapKeyTimeout", 1));
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "osdtimeout", textBoxDisplayTimeout.Text);
        xmlwriter.SetValue("movieplayer", "zapdelay", textBoxZapDelay.Text);
        xmlwriter.SetValue("movieplayer", "zaptimeout", textBoxZapTimeout.Text);
        xmlwriter.SetValue("movieplayer", "zapKeyTimeout", textBoxZapKeyTimeout.Text);
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
            this.groupBoxZapOSD = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.textBoxZapTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.labelZapTimeOut = new MediaPortal.UserInterface.Controls.MPLabel();
            this.textBoxZapDelay = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.labelZapDelay = new MediaPortal.UserInterface.Controls.MPLabel();
            this.groupBoxOSD = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.textBoxDisplayTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.labelDisplayTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
            this.textBoxZapKeyTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.labelZapKeyTimeOut = new MediaPortal.UserInterface.Controls.MPLabel();
            this.groupBoxZapOSD.SuspendLayout();
            this.groupBoxOSD.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxZapOSD
            // 
            this.groupBoxZapOSD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxZapOSD.Controls.Add(this.labelZapKeyTimeOut);
            this.groupBoxZapOSD.Controls.Add(this.textBoxZapKeyTimeout);
            this.groupBoxZapOSD.Controls.Add(this.textBoxZapTimeout);
            this.groupBoxZapOSD.Controls.Add(this.labelZapTimeOut);
            this.groupBoxZapOSD.Controls.Add(this.textBoxZapDelay);
            this.groupBoxZapOSD.Controls.Add(this.labelZapDelay);
            this.groupBoxZapOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBoxZapOSD.Location = new System.Drawing.Point(6, 56);
            this.groupBoxZapOSD.Name = "groupBoxZapOSD";
            this.groupBoxZapOSD.Size = new System.Drawing.Size(462, 99);
            this.groupBoxZapOSD.TabIndex = 3;
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
            this.textBoxZapTimeout.Size = new System.Drawing.Size(286, 20);
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
            this.textBoxZapDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxZapDelay.BorderColor = System.Drawing.Color.Empty;
            this.textBoxZapDelay.Location = new System.Drawing.Point(160, 20);
            this.textBoxZapDelay.Name = "textBoxZapDelay";
            this.textBoxZapDelay.Size = new System.Drawing.Size(286, 20);
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
            this.groupBoxOSD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOSD.Controls.Add(this.textBoxDisplayTimeout);
            this.groupBoxOSD.Controls.Add(this.labelDisplayTimeout);
            this.groupBoxOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBoxOSD.Location = new System.Drawing.Point(6, 0);
            this.groupBoxOSD.Name = "groupBoxOSD";
            this.groupBoxOSD.Size = new System.Drawing.Size(462, 50);
            this.groupBoxOSD.TabIndex = 2;
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
            this.textBoxDisplayTimeout.Size = new System.Drawing.Size(286, 20);
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
            // textBoxZapKeyTimeout
            // 
            this.textBoxZapKeyTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxZapKeyTimeout.BorderColor = System.Drawing.Color.Empty;
            this.textBoxZapKeyTimeout.Location = new System.Drawing.Point(160, 68);
            this.textBoxZapKeyTimeout.Name = "textBoxZapKeyTimeout";
            this.textBoxZapKeyTimeout.Size = new System.Drawing.Size(286, 20);
            this.textBoxZapKeyTimeout.TabIndex = 5;
            // 
            // labelZapKeyTimeOut
            // 
            this.labelZapKeyTimeOut.AutoSize = true;
            this.labelZapKeyTimeOut.Location = new System.Drawing.Point(16, 72);
            this.labelZapKeyTimeOut.Name = "labelZapKeyTimeOut";
            this.labelZapKeyTimeOut.Size = new System.Drawing.Size(115, 13);
            this.labelZapKeyTimeOut.TabIndex = 4;
            this.labelZapKeyTimeOut.Text = "Zap key timeout (sec.):";
            // 
            // GuiOSD
            // 
            this.Controls.Add(this.groupBoxZapOSD);
            this.Controls.Add(this.groupBoxOSD);
            this.Name = "GuiOSD";
            this.Size = new System.Drawing.Size(472, 408);
            this.groupBoxZapOSD.ResumeLayout(false);
            this.groupBoxZapOSD.PerformLayout();
            this.groupBoxOSD.ResumeLayout(false);
            this.groupBoxOSD.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion
  }
}