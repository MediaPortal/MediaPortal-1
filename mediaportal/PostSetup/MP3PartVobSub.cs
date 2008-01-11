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

using System;
using System.Windows.Forms;

namespace PostSetup
{
	/// <summary>
	/// Summary description for MP3PartVobSub.
	/// </summary>
	public class MP3PartVobSub : MP3PartInstaller 
	{		
		public override void Init(string mpTargetDir)
		{
			string programfilesdir = Environment.GetEnvironmentVariable("ProgramFiles");
			// HKEY_LOCAL_MACHINE\SOFTWARE\VobSub 
			this.Title = "VobSub";
			this.Description = "VobSub gives MediaPortal the ability to show subtitles during the playback of movies.";
			this.DownloadUrls = new string[] {"http://www.free-codecs.com/download_soft.php?d=242&s=168"};
			this.SaveToPath = mpTargetDir + @"\postsetup\vobsub";
			this.SaveAsFilename = "VobSub_2.23.exe";
			this.ExecCommand = mpTargetDir + @"\postsetup\vobsub\VobSub_2.23.exe";
			this.ExecCmdArguments = "/S /D=" + programfilesdir + @"\Gabest\VobSub\";
			this.MoreInfoUrl = "http://www.doom9.org/index.html?/dvobsub.htm";
			this.Dock=DockStyle.Top;
			this.Visible=true;
		
			
		}
		/// <summary>
		/// called when some one checks the box
		/// </summary>

		public override void CheckedToInstall()
		{
			// HKEY_LOCAL_MACHINE\SOFTWARE\VobSub 
			if(this.RegistryKeyExists(@"SOFTWARE\GNU\VobSub")) 
			{
				DialogResult dr = MessageBox.Show("MediaPortal has detected that vobsub is already installed. \nTo re-install vobsub, press Cancel and use the Add/Remove applet in Control Panel before trying again. \nTo continue the installation with your existing ffdshow setup press OK","Warning",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning);
				

				if(dr.Equals(DialogResult.OK))
				{
					//skip installation, go to next.
					this.DoInstallation=false;
					this.ButtonAction=MP3PartInstaller.BUTTONACTION_NEXT;
					
				} 
				else if(dr.Equals(DialogResult.Cancel))
				{
					//cancel...stop! uninstall first.
					this.DoInstallation=true;
					// uninstall old version!, popping up controlpanel-add/remove programs.
					CallControlPanelApplet("APPWIZ.CPL");
					this.ButtonAction=MP3PartInstaller.BUTTONACTION_INSTALL;
				}

			} 
			else
			{
				this.DoInstallation=true;
				this.ButtonAction=MP3PartInstaller.BUTTONACTION_INSTALL;
			}

		}
		/// <summary>
		/// called when some one unchecks the box
		/// </summary>
		public override void UnCheckedToInstall()
		{
			this.ButtonAction=MP3PartInstaller.BUTTONACTION_NEXT;
		}



	}
}
