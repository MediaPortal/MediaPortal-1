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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.Topbar
{
  /// <summary>
  /// Summary description for TopBarSetupForm.
  /// </summary>
  public class TopBarSetupForm : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPCheckBox chkAutoHide;
    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox textTimeOut;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkOverrideSkinAutoHide;
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

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
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
      this.SuspendLayout();
      // 
      // chkAutoHide
      // 
      this.chkAutoHide.Enabled = false;
      this.chkAutoHide.Location = new System.Drawing.Point(16, 40);
      this.chkAutoHide.Name = "chkAutoHide";
      this.chkAutoHide.TabIndex = 1;
      this.chkAutoHide.Text = "Autohide";
      // 
      // buttonOk
      // 
      this.buttonOk.Location = new System.Drawing.Point(200, 224);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.TabIndex = 3;
      this.buttonOk.Text = "Ok";
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(128, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 24);
      this.label2.TabIndex = 3;
      this.label2.Text = "sec.";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 80);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 24);
      this.label1.TabIndex = 2;
      this.label1.Text = "Timeout:";
      // 
      // textTimeOut
      // 
      this.textTimeOut.Location = new System.Drawing.Point(80, 80);
      this.textTimeOut.Name = "textTimeOut";
      this.textTimeOut.Size = new System.Drawing.Size(48, 20);
      this.textTimeOut.TabIndex = 2;
      this.textTimeOut.Text = "15";
      // 
      // chkOverrideSkinAutoHide
      // 
      this.chkOverrideSkinAutoHide.Location = new System.Drawing.Point(16, 16);
      this.chkOverrideSkinAutoHide.Name = "chkOverrideSkinAutoHide";
      this.chkOverrideSkinAutoHide.Size = new System.Drawing.Size(200, 24);
      this.chkOverrideSkinAutoHide.TabIndex = 0;
      this.chkOverrideSkinAutoHide.Text = "Override skin \"AutoHide\" setting";
      this.chkOverrideSkinAutoHide.CheckedChanged += new System.EventHandler(this.chkOverrideSkinAutoHide_CheckedChanged);
      // 
      // TopBarSetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(292, 266);
      this.Controls.Add(this.chkOverrideSkinAutoHide);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textTimeOut);
      this.Controls.Add(this.chkAutoHide);
      this.Name = "TopBarSetupForm";
      this.Text = "TopBarSetupForm";
      this.Load += new System.EventHandler(this.TopBarSetup_Load);
      this.ResumeLayout(false);

    }
    #endregion

    private void TopBarSetup_Load(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        textTimeOut.Text = xmlreader.GetValueAsString("TopBar", "autohidetimeout", "15");

        chkAutoHide.Checked = false;
        if (xmlreader.GetValueAsInt("TopBar", "autohide", 0) == 1) chkAutoHide.Checked = true;
        chkOverrideSkinAutoHide.Checked = false;
        if (xmlreader.GetValueAsInt("TopBar", "overrideskinautohide", 0) == 1) chkOverrideSkinAutoHide.Checked = true;
        chkAutoHide.Enabled = chkOverrideSkinAutoHide.Checked;
      }
    }

    private void buttonOk_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlWriter.SetValue("TopBar", "autohidetimeout", textTimeOut.Text);

        int iAutoHide = 0;
        if (chkAutoHide.Checked) iAutoHide = 1;
        xmlWriter.SetValue("TopBar", "autohide", iAutoHide.ToString());

        int iBoolean = 0;
        if (chkOverrideSkinAutoHide.Checked) iBoolean = 1;
        xmlWriter.SetValue("TopBar", "overrideskinautohide", iBoolean.ToString());
      }
      this.Close();
    }

    private void chkOverrideSkinAutoHide_CheckedChanged(object sender, System.EventArgs e)
    {
      chkAutoHide.Enabled = chkOverrideSkinAutoHide.Checked;
    }
  }
}
