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
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace GUIScript
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form, ISetupForm
	{
		private System.Windows.Forms.ListBox ScriptsFunctions;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			Log.Write("Init MPScript");
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			string scriptdir=System.IO.Directory.GetCurrentDirectory()+"\\"+"scripts";
			Log.Write("{0}",scriptdir);
			if(!Directory.Exists(scriptdir)) // writes some standart scripts
			{
				Directory.CreateDirectory(scriptdir);
				string lines = "#Description: Goes to previous window\n";
				lines =lines + "#Button: 610\n\n";
				lines =lines + "IF (InSubMenu==true)   // Go Back Menu only in Submenus\n";
				lines =lines + "   MP_Action(PREVIOUS_MENU)\n";
				lines =lines + "EndIf\n\n";
				lines =lines + "End\n";
				System.IO.StreamWriter file = new System.IO.StreamWriter(scriptdir+"\\BACK BUTTON.MPS");
				file.WriteLine(lines);
				file.Close();
				
				lines        = "#Description: Ejects CD\n";
				lines =lines + "#Button: 2126\n\n";
				lines =lines + "MP_Action(EJECTCD)\n";
				lines =lines + "End\n";
				file = new System.IO.StreamWriter(scriptdir+"\\CD EJECT.MPS");
				file.WriteLine(lines);
				file.Close();

				lines        = "#Description: Hibernate PC\n";
				lines =lines + "#Button: Hibernate\n\n";
				lines =lines + "MP_Action(hibernate)\n";
				lines =lines + "End\n";
				file = new System.IO.StreamWriter(scriptdir+"\\HIBERNATE.MPS");
				file.WriteLine(lines);
				file.Close();

				lines        = "#Description: Shut down PC\n";
				lines =lines + "#Button: 631\n\n";
				lines =lines + "MP_Action(SHUTDOWN)\n";
				lines =lines + "End\n";
				file = new System.IO.StreamWriter(scriptdir+"\\SHUT DOWN.MPS");
				file.WriteLine(lines);
				file.Close();

				lines        = "#Description: Reboot PC\n";
				lines =lines + "#Button: Reboot\n\n";
				lines =lines + "MP_Action(REBOOT)\n";
				lines =lines + "End\n";
				file = new System.IO.StreamWriter(scriptdir+"\\REBOOT.MPS");
				file.WriteLine(lines);
				file.Close();
			}
			DirectoryInfo scDir = new DirectoryInfo(scriptdir);
			foreach(FileInfo fi in scDir.GetFiles()) 
			{
				if (fi.Extension.ToLower()==".mps") 
				{
					string fl=fi.Name;
					fl=fl.Substring(0,fl.Length-4);
					ScriptsFunctions.Items.Add(fl);
				}
			}
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
			this.ScriptsFunctions = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// ScriptsFunctions
			// 
			this.ScriptsFunctions.Enabled = false;
			this.ScriptsFunctions.Location = new System.Drawing.Point(176, 24);
			this.ScriptsFunctions.Name = "ScriptsFunctions";
			this.ScriptsFunctions.Size = new System.Drawing.Size(392, 381);
			this.ScriptsFunctions.TabIndex = 1;
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 446);
			this.Controls.Add(this.ScriptsFunctions);
			this.Name = "SetupForm";
			this.Text = "SetupForm";
			this.ResumeLayout(false);

		}
		#endregion

		#region plugin vars

		public string PluginName() 
		{
			return "My Script";
		}

		public string Description() 
		{
			return "A script plugin for MediaPortal";
		}

		public string Author() 
		{
			return "Gucky62";
		}

		public void ShowPlugin() 
		{
			ShowDialog();
		}

		public bool DefaultEnabled() 
		{
			return true;
		}

		public bool CanEnable() 
		{
			return true;
		}

		public bool HasSetup() 
		{
			return true;
		}

		public int GetWindowId() 
		{
			return 740;
		}

		/// <summary>
		/// If the plugin should have its own button on the home screen then it
		/// should return true to this method, otherwise if it should not be on home
		/// it should return false
		/// </summary>
		/// <param name="strButtonText">text the button should have</param>
		/// <param name="strButtonImage">image for the button, or empty for default</param>
		/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
		/// <param name="strPictureImage">subpicture for the button or empty for none</param>
		/// <returns>true  : plugin needs its own button on home
		///          false : plugin does not need its own button on home</returns>
		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) 
		{
			strButtonText = "MPScript";
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}
		#endregion

	}
}
