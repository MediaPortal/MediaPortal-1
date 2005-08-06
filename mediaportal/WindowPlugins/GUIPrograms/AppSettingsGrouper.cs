/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  public class AppSettingsGrouper: AppSettings
  {
    private Label lblImageFile;
    private Button buttonImagefile;
    private TextBox txtImageFile;
    private CheckBox chkbEnabled;
    private TextBox txtTitle;
    private Label lblTitle;
    private Label LabelTitle;
    private Label LabelHint;
    private Label LblPinCode;
    private TextBox txtPinCode;
    private IContainer components = null;

    public AppSettingsGrouper()
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      this.txtPinCode.PasswordChar = (char)0x25CF;
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppSettingsGrouper));
      this.LabelTitle = new System.Windows.Forms.Label();
      this.LabelHint = new System.Windows.Forms.Label();
      this.lblImageFile = new System.Windows.Forms.Label();
      this.buttonImagefile = new System.Windows.Forms.Button();
      this.txtImageFile = new System.Windows.Forms.TextBox();
      this.chkbEnabled = new System.Windows.Forms.CheckBox();
      this.txtTitle = new System.Windows.Forms.TextBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.LblPinCode = new System.Windows.Forms.Label();
      this.txtPinCode = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // LabelTitle
      // 
      this.LabelTitle.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.LabelTitle.Location = new System.Drawing.Point(0, 0);
      this.LabelTitle.Name = "LabelTitle";
      this.LabelTitle.Size = new System.Drawing.Size(184, 32);
      this.LabelTitle.TabIndex = 0;
      this.LabelTitle.Text = "Grouper";
      // 
      // LabelHint
      // 
      this.LabelHint.Location = new System.Drawing.Point(120, 124);
      this.LabelHint.Name = "LabelHint";
      this.LabelHint.Size = new System.Drawing.Size(248, 40);
      this.LabelHint.TabIndex = 8;
      this.LabelHint.Text = "This item can hold subitems. Use drag and drop in the treeview to add / move node" +
        "s.";
      // 
      // lblImageFile
      // 
      this.lblImageFile.Location = new System.Drawing.Point(0, 92);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(80, 20);
      this.lblImageFile.TabIndex = 5;
      this.lblImageFile.Text = "Imagefile:";
      // 
      // buttonImagefile
      // 
      this.buttonImagefile.Image = ((System.Drawing.Image)(resources.GetObject("buttonImagefile.Image")));
      this.buttonImagefile.Location = new System.Drawing.Point(376, 92);
      this.buttonImagefile.Name = "buttonImagefile";
      this.buttonImagefile.Size = new System.Drawing.Size(20, 20);
      this.buttonImagefile.TabIndex = 7;
      this.buttonImagefile.Click += new System.EventHandler(this.buttonImagefile_Click);
      // 
      // txtImageFile
      // 
      this.txtImageFile.Location = new System.Drawing.Point(120, 92);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(250, 20);
      this.txtImageFile.TabIndex = 6;
      this.txtImageFile.Text = "";
      // 
      // chkbEnabled
      // 
      this.chkbEnabled.Checked = true;
      this.chkbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.chkbEnabled.Location = new System.Drawing.Point(320, 8);
      this.chkbEnabled.Name = "chkbEnabled";
      this.chkbEnabled.Size = new System.Drawing.Size(72, 24);
      this.chkbEnabled.TabIndex = 9;
      this.chkbEnabled.Text = "Enabled";
      this.chkbEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // txtTitle
      // 
      this.txtTitle.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtTitle.Location = new System.Drawing.Point(120, 40);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(250, 20);
      this.txtTitle.TabIndex = 2;
      this.txtTitle.Text = "";
      // 
      // lblTitle
      // 
      this.lblTitle.Location = new System.Drawing.Point(0, 40);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(100, 16);
      this.lblTitle.TabIndex = 1;
      this.lblTitle.Text = "Title:";
      // 
      // LblPinCode
      // 
      this.LblPinCode.Location = new System.Drawing.Point(0, 64);
      this.LblPinCode.Name = "LblPinCode";
      this.LblPinCode.Size = new System.Drawing.Size(96, 16);
      this.LblPinCode.TabIndex = 3;
      this.LblPinCode.Text = "Pin-Code";
      // 
      // txtPinCode
      // 
      this.txtPinCode.Location = new System.Drawing.Point(120, 64);
      this.txtPinCode.MaxLength = 4;
      this.txtPinCode.Name = "txtPinCode";
      this.txtPinCode.Size = new System.Drawing.Size(64, 20);
      this.txtPinCode.TabIndex = 4;
      this.txtPinCode.Text = "";
      this.txtPinCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPinCode_KeyPress);
      // 
      // AppSettingsGrouper
      // 
      this.Controls.Add(this.LblPinCode);
      this.Controls.Add(this.txtPinCode);
      this.Controls.Add(this.lblImageFile);
      this.Controls.Add(this.buttonImagefile);
      this.Controls.Add(this.txtImageFile);
      this.Controls.Add(this.chkbEnabled);
      this.Controls.Add(this.txtTitle);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.LabelHint);
      this.Controls.Add(this.LabelTitle);
      this.Name = "AppSettingsGrouper";
      this.Size = new System.Drawing.Size(400, 320);
      this.ResumeLayout(false);

    }
    #endregion 

    public override bool AppObj2Form(AppItem curApp)
    {
      base.AppObj2Form(curApp);
      this.chkbEnabled.Checked = curApp.Enabled;
      this.txtTitle.Text = curApp.Title;
      this.txtImageFile.Text = curApp.Imagefile;
      if (curApp.Pincode > 0)
      {
        this.txtPinCode.Text = String.Format("{0}", curApp.Pincode);
      }
      else
      {
        this.txtPinCode.Text = "";
      }
      return true;
    }

    public override void Form2AppObj(AppItem curApp)
    {
      base.Form2AppObj(curApp);
      curApp.Enabled = this.chkbEnabled.Checked;
      curApp.Title = this.txtTitle.Text;
      curApp.SourceType = myProgSourceType.GROUPER;
      curApp.Imagefile = this.txtImageFile.Text;
      curApp.Pincode = ProgramUtils.StrToIntDef(this.txtPinCode.Text,  - 1);
    }

    public override bool EntriesOK(AppItem curApp)
    {
      m_Checker.Clear();
      m_Checker.DoCheck(txtTitle.Text != "", "No title entered!");
      if (!m_Checker.IsOk)
      {
        string strHeader = "The following entries are invalid: \r\n\r\n";
        string strFooter = "\r\n\r\n(Click DELETE to remove this item)";
        MessageBox.Show(strHeader + m_Checker.Problems + strFooter, "Invalid Entries");
      }
      else
      {}
        return m_Checker.IsOk;
    }



    private void buttonImagefile_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = txtImageFile.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        txtImageFile.Text = dialogFile.FileName;
      }
    }

    private void txtPinCode_KeyPress(object sender, KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }


  }
}
