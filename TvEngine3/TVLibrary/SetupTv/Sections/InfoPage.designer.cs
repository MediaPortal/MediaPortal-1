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

#pragma warning disable 108

namespace SetupTv.Sections
{
  public partial class InfoPage : SetupTv.SectionSettings
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
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
      this.groupBoxInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblInfoText = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxInfo.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxInfo
      // 
      this.groupBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxInfo.Controls.Add(this.lblInfoText);
      this.groupBoxInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxInfo.Location = new System.Drawing.Point(0, 0);
      this.groupBoxInfo.Name = "groupBoxInfo";
      this.groupBoxInfo.Size = new System.Drawing.Size(472, 405);
      this.groupBoxInfo.TabIndex = 0;
      this.groupBoxInfo.TabStop = false;
      // 
      // lblInfoText
      // 
      this.lblInfoText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblInfoText.Location = new System.Drawing.Point(16, 24);
      this.lblInfoText.Name = "lblInfoText";
      this.lblInfoText.Size = new System.Drawing.Size(440, 367);
      this.lblInfoText.TabIndex = 0;
      this.lblInfoText.Text = "No information available";
      // 
      // Project
      // 
      this.Controls.Add(this.groupBoxInfo);
      this.Name = "Project";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxInfo.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxInfo;
    private MediaPortal.UserInterface.Controls.MPLabel lblInfoText;
  }
}
