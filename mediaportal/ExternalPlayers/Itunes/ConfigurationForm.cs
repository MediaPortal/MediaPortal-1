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
using System.Text;
using System.IO;
using Microsoft.Win32;
using MediaPortal.GUI.Library;

namespace MediaPortal.ITunesPlayer 
{
	/// <summary>
	/// Summary description for Configuration.
	/// </summary>
	public class ConfigurationForm : System.Windows.Forms.Form
	{

    private string m_enabledExt = "";
    private System.Windows.Forms.Button buttonEnable;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox extensionBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConfigurationForm()
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
			this.buttonEnable = new System.Windows.Forms.Button();
			this.extensionBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonEnable
			// 
			this.buttonEnable.Location = new System.Drawing.Point(256, 48);
			this.buttonEnable.Name = "buttonEnable";
			this.buttonEnable.TabIndex = 3;
			this.buttonEnable.Text = "Save";
			this.buttonEnable.Click += new System.EventHandler(this.buttonEnable_Click);
			// 
			// extensionBox
			// 
			this.extensionBox.Location = new System.Drawing.Point(72, 16);
			this.extensionBox.Name = "extensionBox";
			this.extensionBox.Size = new System.Drawing.Size(256, 20);
			this.extensionBox.TabIndex = 4;
			this.extensionBox.Text = ".mp3, .m4a, .m4p, .m4b";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 5;
			this.label1.Text = "Extensions";
			// 
			// ConfigurationForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(344, 85);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.extensionBox);
			this.Controls.Add(this.buttonEnable);
			this.Name = "ConfigurationForm";
			this.Text = "Configuration";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ConfigurationForm_Closing);
			this.Load += new System.EventHandler(this.ConfigurationForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

    private void ConfigurationForm_Load(object sender, System.EventArgs e)
    {
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        m_enabledExt = xmlreader.GetValueAsString("itunesplugin", "enabledextensions","");
        m_enabledExt.Replace(":", ","); // in case it was using the old plugin code where the separator was ":"
      }
      if(m_enabledExt != null && m_enabledExt.Length > 0)
        extensionBox.Text = m_enabledExt;
    }

    private void ConfigurationForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      using (MediaPortal.Profile.Xml   xmlWriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlWriter.SetValue("itunesplugin", "enabledextensions", extensionBox.Text);
      }
    }

    private void buttonEnable_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Xml   xmlWriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlWriter.SetValue("itunesplugin", "enabledextensions", extensionBox.Text);
      }
			this.Close();
    }
    
	}
}
