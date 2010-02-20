#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.WinampPlayer
{
  /// <summary>
  /// Summary description for Configuration.
  /// </summary>
  public class ConfigurationForm : MPConfigForm
  {
    public const string WINAMPINI = "winamp.ini";

    private string m_enabledExt = "";
    private MPButton btnOK;
    private MPLabel label1;
    private MPTextBox extensionBox;
    private MPButton btnCancel;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public ConfigurationForm()
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
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.extensionBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(186, 50);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 3;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.bnOK_Click);
      // 
      // extensionBox
      // 
      this.extensionBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionBox.BorderColor = System.Drawing.Color.Empty;
      this.extensionBox.Location = new System.Drawing.Point(82, 16);
      this.extensionBox.Name = "extensionBox";
      this.extensionBox.Size = new System.Drawing.Size(260, 20);
      this.extensionBox.TabIndex = 4;
      this.extensionBox.Text = ".cda, .mp3, .mp2, .mp1, .aac, .apl, .wav, .voc, .au, .snd, .aif, .aiff, .wma";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 23);
      this.label1.TabIndex = 5;
      this.label1.Text = "Extensions";
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(267, 50);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 6;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // ConfigurationForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(354, 85);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.extensionBox);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "ConfigurationForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Winamp - Setup";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private void LoadSettings()
    {
      extensionBox.Text = String.Empty;
      using (Settings xmlreader = new MPSettings())
      {
        m_enabledExt = xmlreader.GetValueAsString("winampplugin", "enabledextensions", "");
        m_enabledExt.Replace(":", ","); // in case it was using the old plugin code where the separator was ":"
      }
      if (m_enabledExt != null && m_enabledExt.Length > 0)
      {
        extensionBox.Text = m_enabledExt;
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlWriter = new MPSettings())
      {
        xmlWriter.SetValue("winampplugin", "enabledextensions", extensionBox.Text);
      }
    }

    private void bnOK_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      LoadSettings();
      this.Close();
    }
  }
}