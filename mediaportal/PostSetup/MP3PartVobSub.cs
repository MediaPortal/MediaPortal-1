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
