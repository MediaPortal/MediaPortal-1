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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.Topbar
{
  /// <summary>
  /// Summary description for TopBarSetupForm.
  /// </summary>
  public class TopBarSetupForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MediaPortal.UserInterface.Controls.MPCheckBox chkAutoHide;
    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox textTimeOut;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkOverrideSkinAutoHide;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public TopBarSetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      LoadSettings();
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.chkAutoHide = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textTimeOut = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.chkOverrideSkinAutoHide = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // chkAutoHide
      // 
      this.chkAutoHide.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.chkAutoHide.AutoSize = true;
      this.chkAutoHide.Enabled = false;
      this.chkAutoHide.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkAutoHide.Location = new System.Drawing.Point(16, 40);
      this.chkAutoHide.Name = "chkAutoHide";
      this.chkAutoHide.Size = new System.Drawing.Size(66, 17);
      this.chkAutoHide.TabIndex = 1;
      this.chkAutoHide.Text = "Autohide";
      this.chkAutoHide.UseVisualStyleBackColor = true;
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonOk.Location = new System.Drawing.Point(73, 124);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 3;
      this.buttonOk.Text = "&OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(137, 76);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(57, 24);
      this.label2.TabIndex = 3;
      this.label2.Text = "sec.";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(13, 76);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 24);
      this.label1.TabIndex = 2;
      this.label1.Text = "Timeout:";
      // 
      // textTimeOut
      // 
      this.textTimeOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textTimeOut.BorderColor = System.Drawing.Color.Empty;
      this.textTimeOut.Location = new System.Drawing.Point(83, 73);
      this.textTimeOut.Name = "textTimeOut";
      this.textTimeOut.Size = new System.Drawing.Size(48, 20);
      this.textTimeOut.TabIndex = 2;
      this.textTimeOut.Text = "15";
      // 
      // chkOverrideSkinAutoHide
      // 
      this.chkOverrideSkinAutoHide.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.chkOverrideSkinAutoHide.AutoSize = true;
      this.chkOverrideSkinAutoHide.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkOverrideSkinAutoHide.Location = new System.Drawing.Point(16, 16);
      this.chkOverrideSkinAutoHide.Name = "chkOverrideSkinAutoHide";
      this.chkOverrideSkinAutoHide.Size = new System.Drawing.Size(177, 17);
      this.chkOverrideSkinAutoHide.TabIndex = 0;
      this.chkOverrideSkinAutoHide.Text = "Override skin \"AutoHide\" setting";
      this.chkOverrideSkinAutoHide.UseVisualStyleBackColor = true;
      this.chkOverrideSkinAutoHide.CheckedChanged += new System.EventHandler(this.chkOverrideSkinAutoHide_CheckedChanged);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(154, 124);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 4;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // TopBarSetupForm
      // 
      this.AcceptButton = this.buttonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(241, 159);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.chkOverrideSkinAutoHide);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textTimeOut);
      this.Controls.Add(this.chkAutoHide);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "TopBarSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Topbar - Setup";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        textTimeOut.Text = xmlreader.GetValueAsString("TopBar", "autohidetimeout", "3");

        chkAutoHide.Checked = false;
        if (xmlreader.GetValueAsInt("TopBar", "autohide", 0) == 1) chkAutoHide.Checked = true;
        chkOverrideSkinAutoHide.Checked = false;
        if (xmlreader.GetValueAsInt("TopBar", "overrideskinautohide", 0) == 1) chkOverrideSkinAutoHide.Checked = true;
        chkAutoHide.Enabled = chkOverrideSkinAutoHide.Checked;
      }
    }

    private void buttonOk_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlWriter.SetValue("TopBar", "autohidetimeout", textTimeOut.Text);

        int iAutoHide = 0;
        if (chkAutoHide.Checked) iAutoHide = 1;
        xmlWriter.SetValue("TopBar", "autohide", iAutoHide.ToString());

        int iBoolean = 0;
        if (chkOverrideSkinAutoHide.Checked) iBoolean = 1;
        xmlWriter.SetValue("TopBar", "overrideskinautohide", iBoolean.ToString());
      }
    }

    private void chkOverrideSkinAutoHide_CheckedChanged(object sender, System.EventArgs e)
    {
      chkAutoHide.Enabled = chkOverrideSkinAutoHide.Checked;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      LoadSettings();
      this.Close();
    }
  }
}
