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
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace ProcessPlugins.ExternalDisplay
{
	/// <summary>
	/// Summary description for DriverMissingForm.
	/// </summary>
	public class DriverMissingForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DriverMissingForm()
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.label1 = new System.Windows.Forms.Label();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.btnOK = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label1.Location = new System.Drawing.Point(8, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(296, 64);
      this.label1.TabIndex = 0;
      this.label1.Text = "The DriverLYNX Port I/O Driver is not detected on your system.  This driver is ne" +
        "eded for most of the supported displays.";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.linkLabel1.Location = new System.Drawing.Point(8, 104);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(303, 18);
      this.linkLabel1.TabIndex = 1;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://www.driverlinx.com/DownLoad/DlPortIO.htm";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(232, 128);
      this.btnOK.Name = "btnOK";
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "&OK";
      // 
      // label2
      // 
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
      this.label2.Location = new System.Drawing.Point(8, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(296, 32);
      this.label2.TabIndex = 3;
      this.label2.Text = "Please download and install this driver from the following link:";
      // 
      // DriverMissingForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(312, 160);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.linkLabel1);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DriverMissingForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "DriverLYNX Driver missing";
      this.ResumeLayout(false);

    }
		#endregion

    private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(linkLabel1.Text);
    }
	}
}
