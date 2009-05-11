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

// AboutForm.cs: Shows "About" information for MPWatchDog.
// Copyright (C) 2005-2006  Michel Otte
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

/*
 * Created by SharpDevelop.
 * User: Michel
 * Date: 13-9-2005
 * Time: 11:18
 * 
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace WatchDog
{
  /// <summary>
  /// Description of AboutForm.
  /// </summary>
  public class AboutForm : MPForm
  {
    private System.Windows.Forms.Label aboutLicense;
    private System.Windows.Forms.Label aboutHeader;
    private System.Windows.Forms.Label thanks2Label;
    private System.Windows.Forms.Label aboutAuthor;
    private System.Windows.Forms.Button closeButton;
    private System.Windows.Forms.Label thanksLabel;
    private System.Windows.Forms.Label aboutForum;
    public AboutForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
    }
		
    #region Windows Forms Designer generated code
    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent() {
      this.aboutForum = new System.Windows.Forms.Label();
      this.thanksLabel = new System.Windows.Forms.Label();
      this.closeButton = new System.Windows.Forms.Button();
      this.aboutAuthor = new System.Windows.Forms.Label();
      this.thanks2Label = new System.Windows.Forms.Label();
      this.aboutHeader = new System.Windows.Forms.Label();
      this.aboutLicense = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // aboutForum
      // 
      this.aboutForum.ForeColor = System.Drawing.SystemColors.ControlText;
      this.aboutForum.Location = new System.Drawing.Point(16, 74);
      this.aboutForum.Name = "aboutForum";
      this.aboutForum.Size = new System.Drawing.Size(104, 15);
      this.aboutForum.TabIndex = 4;
      this.aboutForum.Text = "Forum nick: gemx";
      // 
      // thanksLabel
      // 
      this.thanksLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.thanksLabel.ForeColor = System.Drawing.SystemColors.ControlText;
      this.thanksLabel.Location = new System.Drawing.Point(16, 126);
      this.thanksLabel.Name = "thanksLabel";
      this.thanksLabel.Size = new System.Drawing.Size(88, 15);
      this.thanksLabel.TabIndex = 8;
      this.thanksLabel.Text = "Thanks to:";
      // 
      // closeButton
      // 
      this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
      this.closeButton.Location = new System.Drawing.Point(80, 178);
      this.closeButton.Name = "closeButton";
      this.closeButton.Size = new System.Drawing.Size(80, 23);
      this.closeButton.TabIndex = 2;
      this.closeButton.Text = "Close";
      this.closeButton.Click += new System.EventHandler(this.CloseButtonClick);
      // 
      // aboutAuthor
      // 
      this.aboutAuthor.ForeColor = System.Drawing.SystemColors.ControlText;
      this.aboutAuthor.Location = new System.Drawing.Point(16, 59);
      this.aboutAuthor.Name = "aboutAuthor";
      this.aboutAuthor.Size = new System.Drawing.Size(214, 15);
      this.aboutAuthor.TabIndex = 3;
      this.aboutAuthor.Text = "Andreas Kwasnik (originally by Michel Otte)";
      // 
      // thanks2Label
      // 
      this.thanks2Label.ForeColor = System.Drawing.SystemColors.ControlText;
      this.thanks2Label.Location = new System.Drawing.Point(16, 141);
      this.thanks2Label.Name = "thanks2Label";
      this.thanks2Label.Size = new System.Drawing.Size(184, 34);
      this.thanks2Label.TabIndex = 7;
      this.thanks2Label.Text = " infinityloop, scoop (reworked his first version completely)";
      // 
      // aboutHeader
      // 
      this.aboutHeader.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.aboutHeader.ForeColor = System.Drawing.SystemColors.ControlText;
      this.aboutHeader.Location = new System.Drawing.Point(16, 15);
      this.aboutHeader.Name = "aboutHeader";
      this.aboutHeader.Size = new System.Drawing.Size(184, 15);
      this.aboutHeader.TabIndex = 0;
      this.aboutHeader.Text = "MediaPortal test tool is written by:";
      // 
      // aboutLicense
      // 
      this.aboutLicense.ForeColor = System.Drawing.SystemColors.ControlText;
      this.aboutLicense.Location = new System.Drawing.Point(16, 89);
      this.aboutLicense.Name = "aboutLicense";
      this.aboutLicense.Size = new System.Drawing.Size(96, 15);
      this.aboutLicense.TabIndex = 6;
      this.aboutLicense.Text = "License: GPL";
      // 
      // AboutForm
      // 
      this.AcceptButton = this.closeButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(242, 218);
      this.ControlBox = false;
      this.Controls.Add(this.thanksLabel);
      this.Controls.Add(this.thanks2Label);
      this.Controls.Add(this.aboutLicense);
      this.Controls.Add(this.aboutForum);
      this.Controls.Add(this.aboutAuthor);
      this.Controls.Add(this.closeButton);
      this.Controls.Add(this.aboutHeader);
      this.ForeColor = System.Drawing.SystemColors.Control;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutForm";
      this.Opacity = 0.92;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "About";
      this.ResumeLayout(false);

    }
    #endregion
		
    void CloseButtonClick(object sender, System.EventArgs e)
    {
      this.Close();
    }
		
  }
}