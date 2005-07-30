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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Alarm
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class AlarmSetupForm : System.Windows.Forms.Form, ISetupForm
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.TextBox txtAlarmSoundsFolder;
		private System.Windows.Forms.Button btnAlarmSoundsFolder;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AlarmSetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AlarmSetupForm));
			this.label1 = new System.Windows.Forms.Label();
			this.txtAlarmSoundsFolder = new System.Windows.Forms.TextBox();
			this.btnAlarmSoundsFolder = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.label2 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "Alarm sounds folder:";
			// 
			// txtAlarmSoundsFolder
			// 
			this.txtAlarmSoundsFolder.Location = new System.Drawing.Point(112, 16);
			this.txtAlarmSoundsFolder.Name = "txtAlarmSoundsFolder";
			this.txtAlarmSoundsFolder.Size = new System.Drawing.Size(184, 20);
			this.txtAlarmSoundsFolder.TabIndex = 1;
			this.txtAlarmSoundsFolder.Text = "";
			// 
			// btnAlarmSoundsFolder
			// 
			this.btnAlarmSoundsFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnAlarmSoundsFolder.Location = new System.Drawing.Point(304, 16);
			this.btnAlarmSoundsFolder.Name = "btnAlarmSoundsFolder";
			this.btnAlarmSoundsFolder.TabIndex = 2;
			this.btnAlarmSoundsFolder.Text = "Browse";
			this.btnAlarmSoundsFolder.Click += new System.EventHandler(this.btnAlarmSoundsFolder_Click);
			// 
			// btnOk
			// 
			this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOk.Location = new System.Drawing.Point(224, 80);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "&Ok";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(304, 80);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "Snooze Time";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(112, 48);
			this.numericUpDown1.Maximum = new System.Decimal(new int[] {
																		   59,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Minimum = new System.Decimal(new int[] {
																		   1,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(40, 20);
			this.numericUpDown1.TabIndex = 6;
			this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDown1.Value = new System.Decimal(new int[] {
																		 5,
																		 0,
																		 0,
																		 0});
			// 
			// AlarmSetupForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(386, 112);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnAlarmSoundsFolder);
			this.Controls.Add(this.txtAlarmSoundsFolder);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AlarmSetupForm";
			this.Text = "My Alarm Setup";
			this.Load += new System.EventHandler(this.AlarmSetupFrom_Load);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string Description()
		{
			return "An alarm plugin for media portal";
		}

		public bool DefaultEnabled()
		{
			return false;
		}

		public int GetWindowId()
		{
			return 5000;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(850); //My Alarm
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "hover_my alarm.png";
			return true;
		}

		public string Author()
		{
			return "Devo";
		}

		public string PluginName()
		{
			return "My Alarm";
		}

		public void ShowPlugin()
		{
			ShowDialog();
		}
		public bool HasSetup()
		{
			return true;
		}

		#endregion

		private void AlarmSetupFrom_Load(object sender, System.EventArgs e)
		{
			LoadSettings();
		}

		#region Button Events
		/// <summary>
		/// Opens a folder dialog for the alarm sounds
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnAlarmSoundsFolder_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where alarm sounds will be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = txtAlarmSoundsFolder.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					txtAlarmSoundsFolder.Text = folderBrowserDialog.SelectedPath;
				}
			}		
		}

		/// <summary>
		/// Cancels modifing the properties of myalarm
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Saves settings to config file then closes the window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			SaveSettings();
			this.Close();
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Saves my alarm settings to the profile xml.
		/// </summary>
		private void SaveSettings()
		{
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{		
				xmlwriter.SetValue("alarm","alarmSoundsFolder",txtAlarmSoundsFolder.Text); 
				xmlwriter.SetValue("alarm","alarmSnoozeTime",numericUpDown1.Value); 
			}
		}

		/// <summary>
		/// Loads my alarm settings from the profile xml.
		/// </summary>
		private void LoadSettings()
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				txtAlarmSoundsFolder.Text = xmlreader.GetValueAsString("alarm","alarmSoundsFolder",string.Empty);
				numericUpDown1.Value = xmlreader.GetValueAsInt("alarm","alarmSnoozeTime",5);
			}
		}

		#endregion
	
	}
}
