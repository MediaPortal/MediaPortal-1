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
using System.Text;
using System.IO;

namespace MediaPortal.MPExTuneCmd
{
	/// <summary>
	/// Summary description for MPExTuneCmdForm.
	/// </summary>
	public class MPExTuneCmdForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox MpExTuneCmdLoc;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Button ok_button;
		private System.Windows.Forms.Button cancel_button;
		private System.Windows.Forms.TextBox MPExTuneCmdDelim;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MPExTuneCmdForm()
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

		private void LoadSettings()
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				MpExTuneCmdLoc.Text = xmlreader.GetValueAsString("MPExTuneCmd","commandloc","C:\\dtvcon\\dtvcmd.exe");
				MPExTuneCmdDelim.Text = xmlreader.GetValueAsString("MPExTuneCmd","commanddelim","#");
			}
		}

		private bool SaveSettings()
		{
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("MPExTuneCmd","commandloc",MpExTuneCmdLoc.Text);
				xmlwriter.SetValue("MPExTuneCmd","commanddelim",MPExTuneCmdDelim.Text);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MPExTuneCmdForm));
			this.MpExTuneCmdLoc = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.browseButton = new System.Windows.Forms.Button();
			this.ok_button = new System.Windows.Forms.Button();
			this.cancel_button = new System.Windows.Forms.Button();
			this.MPExTuneCmdDelim = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// MpExTuneCmdLoc
			// 
			this.MpExTuneCmdLoc.Location = new System.Drawing.Point(112, 16);
			this.MpExTuneCmdLoc.Name = "MpExTuneCmdLoc";
			this.MpExTuneCmdLoc.Size = new System.Drawing.Size(192, 20);
			this.MpExTuneCmdLoc.TabIndex = 0;
			this.MpExTuneCmdLoc.Text = "C:\\dtvcon\\dtvcmd.exe";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 30);
			this.label1.TabIndex = 1;
			this.label1.Text = "External Command Executable:";
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(312, 15);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 24);
			this.browseButton.TabIndex = 2;
			this.browseButton.Text = "...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// ok_button
			// 
			this.ok_button.Location = new System.Drawing.Point(208, 120);
			this.ok_button.Name = "ok_button";
			this.ok_button.Size = new System.Drawing.Size(56, 32);
			this.ok_button.TabIndex = 3;
			this.ok_button.Text = "OK";
			this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
			// 
			// cancel_button
			// 
			this.cancel_button.Location = new System.Drawing.Point(280, 120);
			this.cancel_button.Name = "cancel_button";
			this.cancel_button.Size = new System.Drawing.Size(56, 32);
			this.cancel_button.TabIndex = 4;
			this.cancel_button.Text = "Cancel";
			this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
			// 
			// MPExTuneCmdDelim
			// 
			this.MPExTuneCmdDelim.Location = new System.Drawing.Point(112, 56);
			this.MPExTuneCmdDelim.Name = "MPExTuneCmdDelim";
			this.MPExTuneCmdDelim.Size = new System.Drawing.Size(192, 20);
			this.MPExTuneCmdDelim.TabIndex = 5;
			this.MPExTuneCmdDelim.Text = "#";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 32);
			this.label2.TabIndex = 6;
			this.label2.Text = "External Command Delimeter:";
			// 
			// MPExTuneCmdForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(344, 163);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.MPExTuneCmdDelim);
			this.Controls.Add(this.cancel_button);
			this.Controls.Add(this.ok_button);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.MpExTuneCmdLoc);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MPExTuneCmdForm";
			this.Text = "MPExTuneCmd Setup 0.1";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>

		/// This method is called whenever the browse button is click

		/// </summary>

		/// <param name="sender">the sender instance</param>

		/// <param name="e">the event.  In this case click!</param>

		private void browseButton_Click(object sender, System.EventArgs e)

		{

			string curDir = Directory.GetCurrentDirectory();

			// The filter for the dialog window is foobar2000.exe

			OpenFileDialog dlg = new OpenFileDialog();
      dlg.RestoreDirectory = true;
			dlg.AddExtension = true;

			dlg.Filter = "dtvcmd (dtvcmd.exe)|dtvcmd.exe|dtvcl (dtvcl.exe)|dtvcl.exe|All files (*.*)|*.*" ;

			// start in media folder

			//dlg.InitialDirectory = @"C:\";    

			// open dialog

			if(dlg.ShowDialog(this) == DialogResult.OK)

			{

				MpExTuneCmdLoc.Text = dlg.FileName;

			}

			Directory.SetCurrentDirectory(curDir);

		}
		
		private void ok_button_Click(object sender, System.EventArgs e)
		{
			if(SaveSettings())
				this.Close();
		}

		private void cancel_button_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
