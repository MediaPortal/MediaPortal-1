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
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.InputDevices;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for IrTrans.
  /// </summary>
  public class IrTrans : SectionSettings
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableIrTrans;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageIrTrans;

    public IrTrans()
      : this("IRTrans")
    {
    }


    public IrTrans(string name)
      : base(name)
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.checkBoxEnableIrTrans = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tabPageIrTrans = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.groupBox1.SuspendLayout();
      this.tabPageIrTrans.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBoxEnableIrTrans
      // 
      this.checkBoxEnableIrTrans.AutoSize = true;
      this.checkBoxEnableIrTrans.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableIrTrans.Location = new System.Drawing.Point(16, 24);
      this.checkBoxEnableIrTrans.Name = "checkBoxEnableIrTrans";
      this.checkBoxEnableIrTrans.Size = new System.Drawing.Size(84, 17);
      this.checkBoxEnableIrTrans.TabIndex = 0;
      this.checkBoxEnableIrTrans.Text = "Use IRTrans";
      this.checkBoxEnableIrTrans.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxEnableIrTrans);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(12, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 56);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // tabPageIrTrans
      // 
      this.tabPageIrTrans.Controls.Add(this.groupBox1);
      this.tabPageIrTrans.Location = new System.Drawing.Point(4, 22);
      this.tabPageIrTrans.Name = "tabPageIrTrans";
      this.tabPageIrTrans.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageIrTrans.Size = new System.Drawing.Size(464, 374);
      this.tabPageIrTrans.TabIndex = 0;
      this.tabPageIrTrans.Text = "IRTrans";
      this.tabPageIrTrans.UseVisualStyleBackColor = true;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPageIrTrans);
      this.tabControl1.Location = new System.Drawing.Point(0, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 400);
      this.tabControl1.TabIndex = 0;
      // 
      // IrTrans
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "IrTrans";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabPageIrTrans.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        checkBoxEnableIrTrans.Checked = xmlreader.GetValueAsBool("remote", "IRTrans", false);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("remote", "IRTrans", checkBoxEnableIrTrans.Checked);
      }
    }
  }
}
