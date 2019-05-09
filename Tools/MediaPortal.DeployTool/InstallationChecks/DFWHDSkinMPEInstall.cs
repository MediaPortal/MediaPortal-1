#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class DFWHDSkinMPEInstall : MPEInstall
  {
    string MPversion = Convert.ToString(Utils.GetCurrentPackageVersion()) + ".0"; //get MP version being installed

    public DFWHDSkinMPEInstall()
    {
      MpeId = "922354ea-14e1-42dd-92af-496e2ac999db";
      MpeURL = "https://github.com/MediaPortal/MP1-Skin-DefaultWideHD/releases/download/" + MPversion + "/DFWHD_" + MPversion + ".mpe1";
      MpeUpdateURL = "https://raw.githubusercontent.com/MediaPortal/MP1-Skin-DefaultWideHD/master/DFWHDupdate.xml";
      MpeUpdateFile = Application.StartupPath + "\\deploy\\" + "DFWHDupdate.xml";
      FileName = Application.StartupPath + "\\deploy\\" + "DFWHD_" + MPversion + ".mpe1";
    }

    static string DirSkinPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Team MediaPortal\\MediaPortal\\Skin\\DefaultWideHD";

    public static bool SkinFolder(bool v)
    {
      if (Directory.Exists(DirSkinPath))
      {
        return true;
      }
      else
      {
        return false;
      }
    }


    public override string GetDisplayName()
    {
      return "DefaultWideHD skin" + (OnlineVersion != null ? " " + OnlineVersion.ToString() : MPversion + " *");
    }

    public override CheckResult CheckStatus()
    {
      CheckResult result = default(CheckResult);

      // check if the user selected DefaultWideHD as default skin, and install it

      if (InstallationProperties.Instance["ChosenSkin"] == "DefaultWideHD" || InstallationProperties.Instance["InstallAllSkin"] == "1" || SkinFolder(true))
      {
        // check if mpe package is installed
        Version vMpeInstalled = GetInstalledMpeVersion();
        if (vMpeInstalled != null)
        {
          if (File.Exists(FileName))
          {
            result.state = CheckState.NOT_INSTALLED; // always install skin setup, not update
          }
          else
          {
            result.state = CheckState.SKIPPED;
            return result;
          }
        }
        else if (SkinFolder(true) && File.Exists(FileName))
        {
          result.state = CheckState.NOT_INSTALLED;
        }
        else
        {
          result.state = CheckState.SKIPPED;
        }


        #region old logic backup
        // Backup old logic
        //// check if mpe package is installed
        //Version vMpeInstalled = GetInstalledMpeVersion();
        //if (vMpeInstalled != null)
        //{
        //  OnlineVersion = GetLatestAvailableMpeVersion();
        //  if (OnlineVersion != null)
        //  {
        //    if ((vMpeInstalled >= OnlineVersion || vMpeInstalled <= OnlineVersion ) && File.Exists(FileName))
        //    {

        //      result.state = CheckState.NOT_INSTALLED; // always install skin setup
        //    }
        //    else if(SkinFolder(true))
        //      {
        //      result.state = CheckState.NOT_INSTALLED;
        //    }
        //    else
        //    {
        //      result.needsDownload = !File.Exists(FileName);
        //    }
        //  }
        //  else if (File.Exists(FileName))
        //  {
        //    result.state = CheckState.VERSION_MISMATCH;
        //  }
        //  else
        //  {
        //    result.state = CheckState.VERSION_LOOKUP_FAILED; 
        //  }
        //}
        //else
        //{
        //  result.needsDownload = !File.Exists(FileName);
        //}
        #endregion
      }
      else
      {
        result.state = CheckState.SKIPPED;
        return result;
      }


      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      return result;
    }

  }

}
