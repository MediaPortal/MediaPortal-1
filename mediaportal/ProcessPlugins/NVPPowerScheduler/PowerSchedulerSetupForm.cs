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

namespace MediaPortal.PowerScheduler
{
	/// <summary>
	/// Summary description for PowerSchedulerSetupForm.
	/// </summary>
	public class PowerSchedulerSetupForm : System.Windows.Forms.Form
	{
		private MediaPortal.UserInterface.Controls.MPButton cb_ok;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cobx_shutdown;
		private System.Windows.Forms.NumericUpDown nud_wakeup;
		private System.Windows.Forms.NumericUpDown nud_shutdown;
		private MediaPortal.UserInterface.Controls.MPCheckBox cbxExtensive;
		private MediaPortal.UserInterface.Controls.MPCheckBox cbxForced;
		private MediaPortal.UserInterface.Controls.MPCheckBox cbxTVoff;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PowerSchedulerSetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			LoadSettings();

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

		void LoadSettings()
		{
			using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				nud_wakeup.Value = xmlreader.GetValueAsInt("powerscheduler","wakeupinterval",1);
				nud_shutdown.Value = xmlreader.GetValueAsInt("powerscheduler","shutdowninterval",3);
				cobx_shutdown.Text = xmlreader.GetValueAsString("powerscheduler","shutdownmode","Suspend");
				cbxExtensive.Checked = xmlreader.GetValueAsBool("powerscheduler","extensivelogging",false);
				cbxForced.Checked = xmlreader.GetValueAsBool("powerscheduler","forcedshutdown",false);
				cbxTVoff.Checked = xmlreader.GetValueAsBool("powerscheduler","disabletv",true);
		
			}
		}

		bool SaveSettings()
		{
			using(MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValue("powerscheduler","wakeupinterval",nud_wakeup.Value);
				xmlwriter.SetValue("powerscheduler","shutdowninterval",nud_shutdown.Value);
				xmlwriter.SetValue("powerscheduler","shutdownmode",cobx_shutdown.Text);
				xmlwriter.SetValueAsBool("powerscheduler","extensivelogging",cbxExtensive.Checked);
				xmlwriter.SetValueAsBool("powerscheduler","forcedshutdown",cbxForced.Checked);
				xmlwriter.SetValueAsBool("powerscheduler","disabletv",cbxTVoff.Checked);
			}
			return true;
		}



		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cb_ok = new MediaPortal.UserInterface.Controls.MPButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cobx_shutdown = new System.Windows.Forms.ComboBox();
			this.nud_wakeup = new System.Windows.Forms.NumericUpDown();
			this.nud_shutdown = new System.Windows.Forms.NumericUpDown();
			this.cbxExtensive = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.cbxForced = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.cbxTVoff = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.nud_wakeup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nud_shutdown)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cb_ok
			// 
			this.cb_ok.Location = new System.Drawing.Point(280, 248);
			this.cb_ok.Name = "cb_ok";
			this.cb_ok.TabIndex = 0;
			this.cb_ok.Text = "OK";
			this.cb_ok.Click += new System.EventHandler(this.cb_ok_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(296, 32);
			this.label2.TabIndex = 0;
			this.label2.Text = "Time in minutes to resume system before recording starts. Zero (0) minutes disabl" +
				"es wakeup. (1 min recommended)";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(304, 40);
			this.label3.TabIndex = 0;
			this.label3.Text = "Idle time in minutes (MP HOME) before system shuts down. Zero (0) minutes disable" +
				"s shutdown. (at least 3 min recommended)";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Shutdown mode";
			// 
			// cobx_shutdown
			// 
			this.cobx_shutdown.Items.AddRange(new object[] {
															   "Hibernate",
															   "Suspend",
															   "Shutdown (no wakeup)",
															   "None (or windows inbuilt)"});
			this.cobx_shutdown.Location = new System.Drawing.Point(200, 88);
			this.cobx_shutdown.Name = "cobx_shutdown";
			this.cobx_shutdown.Size = new System.Drawing.Size(160, 21);
			this.cobx_shutdown.TabIndex = 14;
			// 
			// nud_wakeup
			// 
			this.nud_wakeup.Location = new System.Drawing.Point(320, 8);
			this.nud_wakeup.Maximum = new System.Decimal(new int[] {
																	   60,
																	   0,
																	   0,
																	   0});
			this.nud_wakeup.Name = "nud_wakeup";
			this.nud_wakeup.Size = new System.Drawing.Size(40, 20);
			this.nud_wakeup.TabIndex = 12;
			// 
			// nud_shutdown
			// 
			this.nud_shutdown.Location = new System.Drawing.Point(320, 48);
			this.nud_shutdown.Maximum = new System.Decimal(new int[] {
																		 60,
																		 0,
																		 0,
																		 0});
			this.nud_shutdown.Name = "nud_shutdown";
			this.nud_shutdown.Size = new System.Drawing.Size(40, 20);
			this.nud_shutdown.TabIndex = 13;
			// 
			// cbxExtensive
			// 
			this.cbxExtensive.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbxExtensive.Location = new System.Drawing.Point(8, 80);
			this.cbxExtensive.Name = "cbxExtensive";
			this.cbxExtensive.Size = new System.Drawing.Size(200, 24);
			this.cbxExtensive.TabIndex = 17;
			this.cbxExtensive.Text = "Extensive logging";
			// 
			// cbxForced
			// 
			this.cbxForced.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbxForced.Location = new System.Drawing.Point(8, 16);
			this.cbxForced.Name = "cbxForced";
			this.cbxForced.Size = new System.Drawing.Size(200, 24);
			this.cbxForced.TabIndex = 15;
			this.cbxForced.Text = "Forced shutdown";
			// 
			// cbxTVoff
			// 
			this.cbxTVoff.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbxTVoff.Location = new System.Drawing.Point(8, 40);
			this.cbxTVoff.Name = "cbxTVoff";
			this.cbxTVoff.Size = new System.Drawing.Size(200, 24);
			this.cbxTVoff.TabIndex = 16;
			this.cbxTVoff.Text = "Disable TVcards before shutdown";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.cbxForced);
			this.groupBox1.Controls.Add(this.cbxTVoff);
			this.groupBox1.Controls.Add(this.cbxExtensive);
			this.groupBox1.Location = new System.Drawing.Point(8, 128);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(352, 112);
			this.groupBox1.TabIndex = 18;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Advanced options";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(216, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 16);
			this.label4.TabIndex = 18;
			this.label4.Text = "Highly recommended";
			// 
			// PowerSchedulerSetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(368, 277);
			this.Controls.Add(this.nud_shutdown);
			this.Controls.Add(this.nud_wakeup);
			this.Controls.Add(this.cobx_shutdown);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cb_ok);
			this.Controls.Add(this.groupBox1);
			this.Name = "PowerSchedulerSetupForm";
			this.Text = "Power Scheduler configuration 0.3 ";
			((System.ComponentModel.ISupportInitialize)(this.nud_wakeup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nud_shutdown)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		

		

		private void cb_ok_Click(object sender, System.EventArgs e)
		{
			if(SaveSettings())
				this.Close();
		}

	

		
	}
}
