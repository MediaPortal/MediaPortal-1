#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace PostSetup
{
  /// <summary>
  /// Summary description for MP3PartFFDShow.
  /// </summary>
  public class MP3PartFFDShow : MP3PartInstaller
  {
    private RadioButton rdSSE;
    private RadioButton rdSSE2;
    private RadioButton rdStandard;

    public MP3PartFFDShow()
      : base()
    {
      InitializeComponent();
    }

    private void InitializeComponent()
    {
      this.rdStandard = new MPRadioButton();
      this.rdSSE = new MPRadioButton();
      this.rdSSE2 = new MPRadioButton();
      this.panPackage.SuspendLayout();
      // 
      // panPackage
      // 
      this.panPackage.Controls.Add(this.rdSSE2);
      this.panPackage.Controls.Add(this.rdSSE);
      this.panPackage.Controls.Add(this.rdStandard);
      this.panPackage.Name = "panPackage";
      this.panPackage.Controls.SetChildIndex(this.rdStandard, 0);
      this.panPackage.Controls.SetChildIndex(this.rdSSE, 0);
      this.panPackage.Controls.SetChildIndex(this.rdSSE2, 0);
      // 
      // rdStandard
      // 
      this.rdStandard.Checked = true;
      this.rdStandard.Enabled = false;
      this.rdStandard.Location = new Point(96, 120);
      this.rdStandard.Name = "rdStandard";
      this.rdStandard.TabIndex = 18;
      this.rdStandard.TabStop = true;
      this.rdStandard.Text = "Standard";
      this.rdStandard.CheckedChanged += new EventHandler(this.rdStandard_CheckedChanged);
      // 
      // rdSSE
      // 
      this.rdSSE.Enabled = false;
      this.rdSSE.Location = new Point(200, 120);
      this.rdSSE.Name = "rdSSE";
      this.rdSSE.TabIndex = 19;
      this.rdSSE.Text = "SSE";
      this.rdSSE.CheckedChanged += new EventHandler(this.rdSSE_CheckedChanged);
      // 
      // rdSSE2
      // 
      this.rdSSE2.Enabled = false;
      this.rdSSE2.Location = new Point(312, 120);
      this.rdSSE2.Name = "rdSSE2";
      this.rdSSE2.TabIndex = 18;
      this.rdSSE2.Text = "SSE2";
      this.rdSSE2.CheckedChanged += new EventHandler(this.rdSSE2_CheckedChanged);
      // 
      // MP3PartFFDShow
      // 
      this.Name = "MP3PartFFDShow";
      this.panPackage.ResumeLayout(false);
    }

    /// <summary>
    /// called when some one checks the box
    /// </summary>
    public override void Init(string mpTargetDir)
    {
      string programfilesdir = Environment.GetEnvironmentVariable("ProgramFiles");
      // HKEY_LOCAL_MACHINE\SOFTWARE\GNU\ffdshow 
      this.Title = "ffdshow";
      this.Description =
        "ffdshow is a decoding filter which can process several media formats such as DivX, Xvid and MPEG1." +
        "It also includes VobSub, which is a directshow filter which can show subtitles in movies.\n\n" +
        "ffdshow is available in three versions: Standard, SSE and SSE2.\n" +
        "The SSE and SSE2 versions have been compiled to take advantage of special CPU instructions available in later processors.\n" +
        "SSE is compatible with AMD XP, Pentium III, and some older Pentium 4 chips.\n" +
        "The SSE2 version supports AMD64 and the newer Pentium 4 chips.\n\n" +
        "If you are unsure of which version to install, or are running on older hardware, then use the Standard version.\n\n";
      //see rdXXXXXX_CheckedChanged for download urls.
      rdStandard_CheckedChanged(null, null);
      this.UnZipToPath = mpTargetDir + @"\postsetup\ffdshow";
      this.ExecCommand = mpTargetDir + @"\postsetup\ffdshow\ffdshow-20041012[www.free-codecs.com].exe";
      this.ExecCmdArguments = "/S /D=" + programfilesdir + @"\ffdshow\";
      this.MoreInfoUrl = "http://www.maisenbachers.de/dokuw/glossary:ffdshow";
      this.Dock = DockStyle.Top;
      this.Visible = true;
    }

    /// <summary>
    /// called when some one unchecks the box
    /// </summary>
    public override void CheckedToInstall()
    {
      // HKEY_LOCAL_MACHINE\SOFTWARE\GNU\ffdshow
      if (this.RegistryKeyExists(@"SOFTWARE\GNU\ffdshow"))
      {
        DialogResult dr =
          MessageBox.Show(
            "MediaPortal has detected that ffdshow is already installed. \nTo re-install ffdshow, press Cancel and use the Add/Remove applet in Control Panel before trying again. \nTo continue the installation with your existing ffdshow setup press OK",
            "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);


        if (dr.Equals(DialogResult.OK))
        {
          //skip installation, go to next.
          this.DoInstallation = false;
          this.rdStandard.Enabled = false;
          this.rdSSE.Enabled = false;
          this.rdSSE2.Enabled = false;
          this.ButtonAction = BUTTONACTION_NEXT;
        }
        else if (dr.Equals(DialogResult.Cancel))
        {
          //cancel...stop! uninstall first.
          this.DoInstallation = true;
          this.rdStandard.Enabled = true;
          this.rdSSE.Enabled = true;
          this.rdSSE2.Enabled = true;
          // uninstall old version!, popping up controlpanel-add/remove programs.
          CallControlPanelApplet("APPWIZ.CPL");
          this.ButtonAction = BUTTONACTION_INSTALL;
        }
      }
      else
      {
        this.DoInstallation = true;
        this.rdStandard.Enabled = true;
        this.rdSSE.Enabled = true;
        this.rdSSE2.Enabled = true;
        this.ButtonAction = BUTTONACTION_INSTALL;
      }
    }

    public override void UnCheckedToInstall()
    {
      this.rdStandard.Enabled = false;
      this.rdSSE.Enabled = false;
      this.rdSSE2.Enabled = false;
      this.ButtonAction = BUTTONACTION_NEXT;
    }

    private void rdStandard_CheckedChanged(object sender, EventArgs e)
    {
      this.DownloadUrls = new string[] {"http://www.free-codecs.com/download_soft.php?d=372&s=50"};
    }

    private void rdSSE_CheckedChanged(object sender, EventArgs e)
    {
      this.DownloadUrls = new string[] {"http://www.free-codecs.com/download_soft.php?d=374&s=50"};
    }

    private void rdSSE2_CheckedChanged(object sender, EventArgs e)
    {
      this.DownloadUrls = new string[] {"http://www.free-codecs.com/download_soft.php?d=373&s=50"};
    }
  }
}