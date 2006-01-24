/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.HCWBlaster
{
	/// <summary>
	/// Summary description for HCWBlasterSetupForm.
	/// </summary>
	public class HCWBlasterSetupForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.CheckBox chkExtendedLog;
    private System.Windows.Forms.GroupBox groupBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public HCWBlasterSetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			LoadSettings();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		private void LoadSettings()
		{
			using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				this.chkExtendedLog.Checked = xmlreader.GetValueAsBool("HCWBlaster", "ExtendedLogging", false);
			}
		}

		private bool SaveSettings()
		{
			using(MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("HCWBlaster", "ExtendedLogging", this.chkExtendedLog.Checked);
			}
			return true;
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
      this.btnOK = new System.Windows.Forms.Button();
      this.chkExtendedLog = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(224, 29);
      this.label1.TabIndex = 6;
      this.label1.Text = "To configure the IR Blaster, use the original Hauppauge IR configuration software" +
        ".";
      // 
      // btnOK
      // 
      this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnOK.Location = new System.Drawing.Point(184, 120);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(72, 22);
      this.btnOK.TabIndex = 5;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // chkExtendedLog
      // 
      this.chkExtendedLog.Location = new System.Drawing.Point(16, 64);
      this.chkExtendedLog.Name = "chkExtendedLog";
      this.chkExtendedLog.Size = new System.Drawing.Size(224, 21);
      this.chkExtendedLog.TabIndex = 4;
      this.chkExtendedLog.Text = "Enable Extended Logging";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.chkExtendedLog);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(248, 96);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      // 
      // HCWBlasterSetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(266, 152);
      this.ControlBox = false;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "HCWBlasterSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Hauppauge IR Blaster Setup";
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			SaveSettings();
			this.Close();
		}
	}
}
