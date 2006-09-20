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
using MediaPortal.TV.Database;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditGroupForm.
  /// </summary>
  public class EditGroupForm : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxName;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPincode;
    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPButton btnCancel;
    TVGroup group = new TVGroup();
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public EditGroupForm()
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
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxPincode = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Group name:";
      // 
      // textBoxName
      // 
      this.textBoxName.Location = new System.Drawing.Point(32, 48);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(208, 20);
      this.textBoxName.TabIndex = 1;
      this.textBoxName.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 88);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Pincode:";
      // 
      // textBoxPincode
      // 
      this.textBoxPincode.Location = new System.Drawing.Point(32, 112);
      this.textBoxPincode.MaxLength = 4;
      this.textBoxPincode.Name = "textBoxPincode";
      this.textBoxPincode.PasswordChar = '*';
      this.textBoxPincode.TabIndex = 3;
      this.textBoxPincode.Text = "";
      // 
      // buttonOk
      // 
      this.buttonOk.Location = new System.Drawing.Point(136, 152);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(32, 23);
      this.buttonOk.TabIndex = 4;
      this.buttonOk.Text = "Ok";
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(184, 152);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(56, 23);
      this.btnCancel.TabIndex = 5;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // EditGroupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(264, 198);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.textBoxPincode);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.label1);
      this.Name = "EditGroupForm";
      this.Text = "Edit Group";
      this.ResumeLayout(false);

    }
    #endregion

    private void buttonOk_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      group.GroupName = textBoxName.Text;
      try
      {
        group.Pincode = Int32.Parse(textBoxPincode.Text);
      }
      catch (Exception)
      {
        group.Pincode = 0;
      }
      this.Close();
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    public TVGroup Group
    {
      get
      {
        return group;
      }
      set
      {
        group = value;
        textBoxName.Text = group.GroupName;
        textBoxPincode.Text = "";
        if (group.Pincode >= 1000)
        {
          textBoxPincode.Text = group.Pincode.ToString();
        }
      }
    }
  }
}
