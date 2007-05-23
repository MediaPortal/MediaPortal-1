#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for AppSettingsPrePost.
  /// </summary>
  public class AppSettingsPrePost : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPButton btnOk;
    private MediaPortal.UserInterface.Controls.MPButton btnCancel;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbData;
    private MediaPortal.UserInterface.Controls.MPLabel lblTitle;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPre;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPost;
    private MediaPortal.UserInterface.Controls.MPLabel lblPre;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel lblTitleData;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public AppSettingsPrePost()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    public string Title
    {
      get { return lblTitleData.Text; }
      set { lblTitleData.Text = value; }
    }

    public string PreLaunch
    {
      get { return tbPre.Text; }
      set { tbPre.Text = value; }
    }

    public string PostLaunch
    {
      get { return tbPost.Text; }
      set { tbPost.Text = value; }
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
      this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbData = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblPre = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbPost = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbPre = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblTitle = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblTitleData = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbData.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(248, 374);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(80, 25);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(336, 374);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 25);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // gbData
      // 
      this.gbData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbData.Controls.Add(this.label1);
      this.gbData.Controls.Add(this.lblPre);
      this.gbData.Controls.Add(this.tbPost);
      this.gbData.Controls.Add(this.tbPre);
      this.gbData.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbData.Location = new System.Drawing.Point(8, 52);
      this.gbData.Name = "gbData";
      this.gbData.Size = new System.Drawing.Size(408, 305);
      this.gbData.TabIndex = 0;
      this.gbData.TabStop = false;
      this.gbData.Text = "Commands to execute before / after launching";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(8, 151);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(112, 17);
      this.label1.TabIndex = 3;
      this.label1.Text = "Post-Execute";
      // 
      // lblPre
      // 
      this.lblPre.Location = new System.Drawing.Point(8, 26);
      this.lblPre.Name = "lblPre";
      this.lblPre.Size = new System.Drawing.Size(100, 17);
      this.lblPre.TabIndex = 2;
      this.lblPre.Text = "Pre-Execute";
      // 
      // tbPost
      // 
      this.tbPost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPost.BorderColor = System.Drawing.Color.Empty;
      this.tbPost.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbPost.Location = new System.Drawing.Point(8, 176);
      this.tbPost.Multiline = true;
      this.tbPost.Name = "tbPost";
      this.tbPost.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbPost.Size = new System.Drawing.Size(392, 120);
      this.tbPost.TabIndex = 1;
      this.tbPost.Text = "tbPost";
      // 
      // tbPre
      // 
      this.tbPre.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPre.BorderColor = System.Drawing.Color.Empty;
      this.tbPre.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbPre.Location = new System.Drawing.Point(8, 52);
      this.tbPre.Multiline = true;
      this.tbPre.Name = "tbPre";
      this.tbPre.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbPre.Size = new System.Drawing.Size(392, 81);
      this.tbPre.TabIndex = 0;
      this.tbPre.Text = "tbPre";
      // 
      // lblTitle
      // 
      this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTitle.Location = new System.Drawing.Point(16, 17);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(72, 26);
      this.lblTitle.TabIndex = 6;
      this.lblTitle.Text = "Application: ";
      // 
      // lblTitleData
      // 
      this.lblTitleData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblTitleData.Location = new System.Drawing.Point(91, 17);
      this.lblTitleData.Name = "lblTitleData";
      this.lblTitleData.Size = new System.Drawing.Size(309, 17);
      this.lblTitleData.TabIndex = 7;
      this.lblTitleData.Text = "Title";
      // 
      // AppSettingsPrePost
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(424, 406);
      this.Controls.Add(this.lblTitleData);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.gbData);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnCancel);
      this.Name = "AppSettingsPrePost";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Pre-Launch / Post-Launch Options";
      this.gbData.ResumeLayout(false);
      this.gbData.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion
  }
}
