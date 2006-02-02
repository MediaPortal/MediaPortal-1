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

namespace MediaPortal.GUI.X10Plugin
{
  /// <summary>
  /// Summary description for LocationForm.
  /// </summary>
  public class ApplianceForm : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPButton buttonSave;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.Label labelName;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private System.Windows.Forms.Label labelURL;
    private System.Windows.Forms.Label label1;
    public System.Windows.Forms.TextBox txtDescription;
    public System.Windows.Forms.TextBox txtCode;
    private System.Windows.Forms.Label label2;
    public System.Windows.Forms.TextBox txtLocation;
    private System.Windows.Forms.Label label3;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public ApplianceForm()
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
      this.buttonSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.txtDescription = new System.Windows.Forms.TextBox();
      this.labelURL = new System.Windows.Forms.Label();
      this.txtCode = new System.Windows.Forms.TextBox();
      this.labelName = new System.Windows.Forms.Label();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.label2 = new System.Windows.Forms.Label();
      this.txtLocation = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonSave
      // 
      this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonSave.Location = new System.Drawing.Point(112, 168);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(72, 24);
      this.buttonSave.TabIndex = 1;
      this.buttonSave.Text = "Save";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.txtLocation);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.txtDescription);
      this.groupBox1.Controls.Add(this.labelURL);
      this.groupBox1.Controls.Add(this.txtCode);
      this.groupBox1.Controls.Add(this.labelName);
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(360, 152);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "X10 Appliance";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 104);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(344, 24);
      this.label1.TabIndex = 4;
      this.label1.Text = "Code = Letter + Number (Housecode and devide number, i.e. A3)";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtDescription
      // 
      this.txtDescription.Location = new System.Drawing.Point(104, 56);
      this.txtDescription.Name = "txtDescription";
      this.txtDescription.Size = new System.Drawing.Size(224, 20);
      this.txtDescription.TabIndex = 3;
      this.txtDescription.Text = "";
      this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
      // 
      // labelURL
      // 
      this.labelURL.Location = new System.Drawing.Point(24, 56);
      this.labelURL.Name = "labelURL";
      this.labelURL.Size = new System.Drawing.Size(72, 24);
      this.labelURL.TabIndex = 2;
      this.labelURL.Text = "Description :";
      this.labelURL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtCode
      // 
      this.txtCode.Location = new System.Drawing.Point(104, 32);
      this.txtCode.Name = "txtCode";
      this.txtCode.Size = new System.Drawing.Size(224, 20);
      this.txtCode.TabIndex = 1;
      this.txtCode.Text = "";
      this.txtCode.TextChanged += new System.EventHandler(this.txtCode_TextChanged);
      // 
      // labelName
      // 
      this.labelName.Location = new System.Drawing.Point(24, 32);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(72, 24);
      this.labelName.TabIndex = 0;
      this.labelName.Text = "Code :";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(192, 168);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(72, 24);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "Cancel";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(24, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 24);
      this.label2.TabIndex = 5;
      this.label2.Text = "Location:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtLocation
      // 
      this.txtLocation.Location = new System.Drawing.Point(104, 80);
      this.txtLocation.Name = "txtLocation";
      this.txtLocation.Size = new System.Drawing.Size(224, 20);
      this.txtLocation.TabIndex = 6;
      this.txtLocation.Text = "";
      this.txtLocation.TextChanged += new System.EventHandler(this.txtLocation_TextChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 128);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(344, 24);
      this.label3.TabIndex = 7;
      this.label3.Text = "Location = room name. Display is grouped by room name.";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ApplianceForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(376, 198);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.buttonSave);
      this.Name = "ApplianceForm";
      this.Text = "Appliance edition - X10 Plugin";
      this.Load += new System.EventHandler(this.ApplianceForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void ApplianceForm_Load(object sender, System.EventArgs e)
    {
      txtCode_TextChanged(sender, e);
    }

    private void txtCode_TextChanged(object sender, System.EventArgs e)
    {
      bool enable = false;
      if (txtCode.Text.Length > 0)
        enable = true;
      txtDescription.Enabled = enable;
      txtDescription_TextChanged(sender, e);
    }

    private void txtDescription_TextChanged(object sender, System.EventArgs e)
    {
      bool enable = false;
      if (txtCode.Text.Length > 0 && txtDescription.Text.Length > 0)
        enable = true;
      txtLocation.Enabled = enable;
    }

    private void txtLocation_TextChanged(object sender, System.EventArgs e)
    {
      bool enable = false;
      if (txtCode.Text.Length > 0 && txtDescription.Text.Length > 0 && txtLocation.Text.Length > 0)
        enable = true;
      buttonSave.Enabled = enable;
    }


  }
}
