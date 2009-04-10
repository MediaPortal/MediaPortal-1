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

using System.IO;
using System.Windows.Forms;

namespace PostSetup
{
  /// <summary>
  /// Summary description for DownloadAndInstall.
  /// </summary>
  public class MP3PartXMLTV : MP3PartInstaller
  {
    /// <summary>
    /// Init this package..
    /// </summary>
    /// <param name="mpTargetDir"></param>
    public override void Init(string mpTargetDir)
    {
      this.Title = "XMLTV";
      this.Description =
        "If you want to download TV listings and use them within MediaPortal's Electronic Programming Guide (EPG) then you may want to install this XMLTV grabber. \nThis grabber can provide listings for Austria, Canada, Denmark, Finland, France, Germany, Hungary, Italy, Japan, Netherlands, Norway, Romania, Spain, Sweden, UK and USA.";
      this.DownloadUrls = new string[]
                            {
                              "http://umn.dl.sourceforge.net/sourceforge/xmltv/xmltv-0.5.37-win32.zip",
                              "http://heanet.dl.sourceforge.net/sourceforge/xmltv/xmltv-0.5.37-win32.zip",
                              "http://jaist.dl.sourceforge.net/sourceforge/xmltv/xmltv-0.5.37-win32.zip"
                            };
      this.UnZipToPath = mpTargetDir + @"\xmltv"; // fix so it dont creates the first dir.
      this.UnzipToExcludeDir = "xmltv-0.5.37-win32";
      this.ExecCommand = "";
      this.ExecCmdArguments = "";
      this.MoreInfoUrl = "http://www.maisenbachers.de/dokuw/glossary:xmltv";
      this.Dock = DockStyle.Top;
      this.Visible = true;
    }

    /// <summary>
    /// called when some one checks the box
    /// </summary>
    public override void CheckedToInstall()
    {
      if (File.Exists(this.UnZipToPath + @"\xmltv.exe"))
      {
        DialogResult dr =
          MessageBox.Show(
            "MediaPortal has detected that XMLTV is already installed. \nTo remove the existing XMLTV, press Yes. \nTo continue the installation with your existing XMLTV setup press No",
            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (dr.Equals(DialogResult.No))
        {
          //skip installation, go to next.
          this.DoInstallation = false;
          this.panPackage.Enabled = false;
          this.ButtonAction = BUTTONACTION_DONTINSTALL;
        }
        else if (dr.Equals(DialogResult.Yes))
        {
          //cancel...stop! remove first.
          this.DoInstallation = true;
          this.panPackage.Enabled = true;
          // remove (delete directory)
          Directory.Delete(this.UnZipToPath, true);
          this.ButtonAction = BUTTONACTION_INSTALL;
        }
      }
      else
      {
        this.DoInstallation = true;
        this.panPackage.Enabled = true;
        this.ButtonAction = BUTTONACTION_INSTALL;
      }
    }

    /// <summary>
    /// calls when some one unchecks the box
    /// </summary>
    public override void UnCheckedToInstall()
    {
      this.ButtonAction = BUTTONACTION_NEXT;
    }
  }
}