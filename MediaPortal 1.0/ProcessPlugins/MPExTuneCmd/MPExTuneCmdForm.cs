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
using System.Text;
using System.IO;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.MPExTuneCmd
{
  /// <summary>
  /// Summary description for MPExTuneCmdForm.
  /// </summary>
  public class MPExTuneCmdForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox MpExTuneCmdLoc;
    private MediaPortal.UserInterface.Controls.MPButton browseButton;
    private MediaPortal.UserInterface.Controls.MPButton ok_button;
    private MediaPortal.UserInterface.Controls.MPButton cancel_button;
    private MediaPortal.UserInterface.Controls.MPTextBox MPExTuneCmdDelim;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public MPExTuneCmdForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      LoadSettings();
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

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        MpExTuneCmdLoc.Text = xmlreader.GetValueAsString("MPExTuneCmd", "commandloc", "C:\\dtvcon\\dtvcmd.exe");
        MPExTuneCmdDelim.Text = xmlreader.GetValueAsString("MPExTuneCmd", "commanddelim", "#");
      }
    }

    private bool SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("MPExTuneCmd", "commandloc", MpExTuneCmdLoc.Text);
        xmlwriter.SetValue("MPExTuneCmd", "commanddelim", MPExTuneCmdDelim.Text);
      }
      return true;
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.MpExTuneCmdLoc = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.browseButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.ok_button = new MediaPortal.UserInterface.Controls.MPButton();
      this.cancel_button = new MediaPortal.UserInterface.Controls.MPButton();
      this.MPExTuneCmdDelim = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SuspendLayout();
      // 
      // MpExTuneCmdLoc
      // 
      this.MpExTuneCmdLoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.MpExTuneCmdLoc.BorderColor = System.Drawing.Color.Empty;
      this.MpExTuneCmdLoc.Location = new System.Drawing.Point(112, 16);
      this.MpExTuneCmdLoc.Name = "MpExTuneCmdLoc";
      this.MpExTuneCmdLoc.Size = new System.Drawing.Size(190, 20);
      this.MpExTuneCmdLoc.TabIndex = 0;
      this.MpExTuneCmdLoc.Text = "C:\\dtvcon\\dtvcmd.exe";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(104, 30);
      this.label1.TabIndex = 1;
      this.label1.Text = "External Command Executable:";
      // 
      // browseButton
      // 
      this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browseButton.Location = new System.Drawing.Point(308, 16);
      this.browseButton.Name = "browseButton";
      this.browseButton.Size = new System.Drawing.Size(24, 24);
      this.browseButton.TabIndex = 2;
      this.browseButton.Text = "...";
      this.browseButton.UseVisualStyleBackColor = true;
      this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
      // 
      // ok_button
      // 
      this.ok_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.ok_button.Location = new System.Drawing.Point(214, 127);
      this.ok_button.Name = "ok_button";
      this.ok_button.Size = new System.Drawing.Size(56, 24);
      this.ok_button.TabIndex = 3;
      this.ok_button.Text = "&OK";
      this.ok_button.UseVisualStyleBackColor = true;
      this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
      // 
      // cancel_button
      // 
      this.cancel_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancel_button.Location = new System.Drawing.Point(276, 127);
      this.cancel_button.Name = "cancel_button";
      this.cancel_button.Size = new System.Drawing.Size(56, 24);
      this.cancel_button.TabIndex = 4;
      this.cancel_button.Text = "&Cancel";
      this.cancel_button.UseVisualStyleBackColor = true;
      this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
      // 
      // MPExTuneCmdDelim
      // 
      this.MPExTuneCmdDelim.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.MPExTuneCmdDelim.BorderColor = System.Drawing.Color.Empty;
      this.MPExTuneCmdDelim.Location = new System.Drawing.Point(112, 56);
      this.MPExTuneCmdDelim.Name = "MPExTuneCmdDelim";
      this.MPExTuneCmdDelim.Size = new System.Drawing.Size(190, 20);
      this.MPExTuneCmdDelim.TabIndex = 5;
      this.MPExTuneCmdDelim.Text = "#";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(104, 32);
      this.label2.TabIndex = 6;
      this.label2.Text = "External Command Delimeter:";
      // 
      // MPExTuneCmdForm
      // 
      this.AcceptButton = this.ok_button;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.cancel_button;
      this.ClientSize = new System.Drawing.Size(344, 163);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.MPExTuneCmdDelim);
      this.Controls.Add(this.cancel_button);
      this.Controls.Add(this.ok_button);
      this.Controls.Add(this.browseButton);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.MpExTuneCmdLoc);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "MPExTuneCmdForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MPExTuneCmd - Setup";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    /// <summary>

    /// This method is called whenever the browse button is click

    /// </summary>

    /// <param name="sender">the sender instance</param>

    /// <param name="e">the event.  In this case click!</param>

    private void browseButton_Click(object sender, System.EventArgs e)
    {

      string curDir = Directory.GetCurrentDirectory();

      // The filter for the dialog window is foobar2000.exe

      OpenFileDialog dlg = new OpenFileDialog();
      dlg.RestoreDirectory = true;
      dlg.AddExtension = true;

      dlg.Filter = "dtvcmd (dtvcmd.exe)|dtvcmd.exe|dtvcl (dtvcl.exe)|dtvcl.exe|All files (*.*)|*.*";

      // start in media folder

      //dlg.InitialDirectory = @"C:\";    

      // open dialog

      if (dlg.ShowDialog(this) == DialogResult.OK)
      {

        MpExTuneCmdLoc.Text = dlg.FileName;

      }

      Directory.SetCurrentDirectory(curDir);

    }

    private void ok_button_Click(object sender, System.EventArgs e)
    {
      if (SaveSettings())
        this.Close();
    }

    private void cancel_button_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }
  }
}
