//PsloglistDialog.cs: Shows Dialog about pslogdir utility missing.
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
 * Date: 16-9-2005
 * Time: 23:21
 * 
 */

using System;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;

namespace MPTestTool
{
	/// <summary>
	/// Shows a dialog stating that psloglist.exe is missing from
	/// the installation directory. An explanation is given what
	/// to do about it... ;)
	/// </summary>
	public class PsloglistDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label msgLabel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.LinkLabel lnkLabel;
		private System.Windows.Forms.Label warningLabel;
		private System.Windows.Forms.Label msg2Label;
		public PsloglistDialog()
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PsloglistDialog));
      this.msg2Label = new System.Windows.Forms.Label();
      this.warningLabel = new System.Windows.Forms.Label();
      this.lnkLabel = new System.Windows.Forms.LinkLabel();
      this.okButton = new System.Windows.Forms.Button();
      this.msgLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // msg2Label
      // 
      this.msg2Label.Location = new System.Drawing.Point(16, 97);
      this.msg2Label.Name = "msg2Label";
      this.msg2Label.Size = new System.Drawing.Size(264, 68);
      this.msg2Label.TabIndex = 3;
      this.msg2Label.Text = resources.GetString("msg2Label.Text");
      // 
      // warningLabel
      // 
      this.warningLabel.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.warningLabel.ForeColor = System.Drawing.Color.Red;
      this.warningLabel.Location = new System.Drawing.Point(24, 6);
      this.warningLabel.Name = "warningLabel";
      this.warningLabel.Size = new System.Drawing.Size(248, 31);
      this.warningLabel.TabIndex = 0;
      this.warningLabel.Text = "Warning!";
      this.warningLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // lnkLabel
      // 
      this.lnkLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lnkLabel.Location = new System.Drawing.Point(16, 76);
      this.lnkLabel.Name = "lnkLabel";
      this.lnkLabel.Size = new System.Drawing.Size(272, 14);
      this.lnkLabel.TabIndex = 2;
      this.lnkLabel.TabStop = true;
      this.lnkLabel.Text = "http://www.sysinternals.com/Utilities/PsTools.html";
      this.lnkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkLabelLinkClicked);
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(104, 179);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(80, 21);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "OK!";
      this.okButton.Click += new System.EventHandler(this.OkButtonClick);
      // 
      // msgLabel
      // 
      this.msgLabel.Location = new System.Drawing.Point(16, 48);
      this.msgLabel.Name = "msgLabel";
      this.msgLabel.Size = new System.Drawing.Size(272, 28);
      this.msgLabel.TabIndex = 1;
      this.msgLabel.Text = "The utility psloglist.exe appears to be missing from your installation! Please do" +
          "wnload this utility from:";
      // 
      // PsloglistDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(292, 227);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.msg2Label);
      this.Controls.Add(this.lnkLabel);
      this.Controls.Add(this.msgLabel);
      this.Controls.Add(this.warningLabel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "PsloglistDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Missing utility";
      this.ResumeLayout(false);

		}
		#endregion

		void OkButtonClick(object sender, System.EventArgs e)
		{
			this.Close();
		}
		
		void LnkLabelLinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Process p = new Process();
            try
            {
                p.StartInfo.FileName = this.lnkLabel.Text; 
                p.StartInfo.CreateNoWindow = true;
                p.Start();
            }
            catch  {}
		}
		
	}
}
