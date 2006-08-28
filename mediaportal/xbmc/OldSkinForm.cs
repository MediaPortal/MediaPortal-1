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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Util;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for OldSkinForm.
	/// </summary>
	public class OldSkinForm : System.Windows.Forms.Form
	{
		private MediaPortal.UserInterface.Controls.MPLabel label1;
		private MediaPortal.UserInterface.Controls.MPCheckBox checkBox1;
		private MediaPortal.UserInterface.Controls.MPButton button1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OldSkinForm()
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
			this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
			this.checkBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(272, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "The current skin is not up-2-date. This can cause problems when using MP.";
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(16, 96);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(120, 40);
			this.checkBox1.TabIndex = 0;
			this.checkBox1.Text = "Dont show this message again";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(216, 104);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// OldSkinForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(304, 141);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.label1);
			this.Name = "OldSkinForm";
			this.Text = "Warning! Old skin in use";
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
			{
				xmlreader.SetValueAsBool("general", "dontshowskinversion", checkBox1.Checked);
			}
			this.Close();
		}
		
		public bool CheckSkinVersion(string skin)
		{
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
			{
				bool ignoreErrors=false;
				ignoreErrors=xmlreader.GetValueAsBool("general", "dontshowskinversion", false);
				if (ignoreErrors) return true;
			}

			string versionBlueTwoSkin="";
			string versionSkin="";
			string filename= Config.Get(Config.Dir.Skin) + @"BlueTwo\references.xml";
			if(File.Exists(filename))
			{	
				XmlDocument doc=new XmlDocument();
				doc.Load(filename);
				XmlNode node=doc.SelectSingleNode("/controls/skin/version");
				if (node!=null && node.InnerText!=null)
					versionBlueTwoSkin=node.InnerText;
			}
      filename = String.Format(Config.Get(Config.Dir.Skin) + @"{0}\references.xml", skin);
			if(File.Exists(filename))
			{	
				XmlDocument doc=new XmlDocument();
				doc.Load(filename);
				XmlNode node=doc.SelectSingleNode("/controls/skin/version");
				if (node!=null && node.InnerText!=null)
					versionSkin=node.InnerText;
			}
			if (versionBlueTwoSkin==versionSkin) return true;
			return false;
		}
	}
}
