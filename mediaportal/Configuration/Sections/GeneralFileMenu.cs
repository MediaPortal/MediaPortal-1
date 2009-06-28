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

using System;
using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralFileMenu : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPCheckBox chbEnabled;
    private MPLabel label1;
    private MPTextBox textPinCodeBox;
    private MPLabel label3;
    private MPTextBox textTrashcanFolder;
    private IContainer components = null;

    public GeneralFileMenu()
      : this("File Menu")
    {
    }

    public GeneralFileMenu(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
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


    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        chbEnabled.Checked = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        textPinCodeBox.Text = Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", ""));
        textTrashcanFolder.Text = xmlreader.GetValueAsString("filemenu", "trashcan", "");
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("filemenu", "enabled", chbEnabled.Checked);
        xmlwriter.SetValue("filemenu", "pincode", Util.Utils.EncryptPin(textPinCodeBox.Text));
        xmlwriter.SetValue("filemenu", "trashcan", textTrashcanFolder.Text);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textPinCodeBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.chbEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textTrashcanFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.textPinCodeBox);
      this.groupBox1.Controls.Add(this.chbEnabled);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 80);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Pincode:";
      // 
      // textPinCodeBox
      // 
      this.textPinCodeBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textPinCodeBox.Location = new System.Drawing.Point(168, 44);
      this.textPinCodeBox.Name = "textPinCodeBox";
      this.textPinCodeBox.Size = new System.Drawing.Size(288, 20);
      this.textPinCodeBox.TabIndex = 2;
      // 
      // chbEnabled
      // 
      this.chbEnabled.AutoSize = true;
      this.chbEnabled.Location = new System.Drawing.Point(168, 20);
      this.chbEnabled.Name = "chbEnabled";
      this.chbEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.chbEnabled.Size = new System.Drawing.Size(104, 17);
      this.chbEnabled.TabIndex = 0;
      this.chbEnabled.Text = "Enable file menu";
      this.chbEnabled.CheckedChanged += new System.EventHandler(this.chbEnabled_CheckedChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 104);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 16);
      this.label3.TabIndex = 1;
      this.label3.Text = "Trashcan folder:";
      this.label3.Visible = false;
      // 
      // textTrashcanFolder
      // 
      this.textTrashcanFolder.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textTrashcanFolder.Location = new System.Drawing.Point(168, 100);
      this.textTrashcanFolder.Name = "textTrashcanFolder";
      this.textTrashcanFolder.Size = new System.Drawing.Size(288, 20);
      this.textTrashcanFolder.TabIndex = 2;
      this.textTrashcanFolder.Visible = false;
      // 
      // FileMenu
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.textTrashcanFolder);
      this.Name = "FileMenu";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private void chbEnabled_CheckedChanged(object sender, EventArgs e)
    {
      textPinCodeBox.Enabled = chbEnabled.Checked;
      textTrashcanFolder.Enabled = chbEnabled.Checked;
    }
  }
}