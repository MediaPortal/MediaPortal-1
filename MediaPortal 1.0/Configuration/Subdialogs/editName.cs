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

namespace MediaPortal
{
  /// <summary>
  /// Summary description for editName.
  /// </summary>
  public class editName : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxName;
    private MediaPortal.UserInterface.Controls.MPButton buttonOK;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public editName()
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
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(200, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Enter a name for this channel";
      // 
      // textBoxName
      // 
      this.textBoxName.BorderColor = System.Drawing.Color.Empty;
      this.textBoxName.Location = new System.Drawing.Point(16, 40);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(256, 20);
      this.textBoxName.TabIndex = 0;
      this.textBoxName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editName_KeyDown);
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(232, 72);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(40, 23);
      this.buttonOK.TabIndex = 1;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // editName
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(292, 102);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "editName";
      this.Text = "Enter a name for this channel";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private void editName_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter && textBoxName.Text != string.Empty)
      {
        this.Close();
      }
    }

    private void btnOk_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }

    public string ChannelName
    {
      get { return textBoxName.Text; }
    }
  }
}
