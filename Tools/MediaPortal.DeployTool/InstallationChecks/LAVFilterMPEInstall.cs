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
  internal class LAVFilterMPEInstall : MPEInstall
  {

    public LAVFilterMPEInstall()
    {
      MpeId = "b7738156-b6ec-4f0f-b1a8-b5010349d8b1";
      MpeURL ="http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link_id=162&cf_id=24";
      MpeUpdateURL= "http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link_id=162&cf_id=52";
      MpeUpdateFile = Application.StartupPath + "\\deploy\\" + "LAVFilters.xml";
      FileName = Application.StartupPath + "\\deploy\\" + "LAVFilters.mpe1";
    }

    public override string GetDisplayName()
    {
      return "LAV Filters" + (OnlineVersion != null ? " " + OnlineVersion.ToString() : "");
    }

    public override CheckResult CheckStatus()
    {
      CheckResult result = default(CheckResult);

      // check if the user does not want LAV installed
      if (InstallationProperties.Instance["ConfigureMediaPortalLAV"] == "0")
      {
        result.state = CheckState.SKIPPED;
        return result;
      }

      // check if mpe package is installed and also check if LAV is actually installed
      Version vMpeInstalled = GetInstalledMpeVersion();
      Version vLavInstalled = GetInstalledLAVVersion();
      if (vLavInstalled != null || vMpeInstalled != null)
      {
        OnlineVersion = GetLatestAvailableMpeVersion();
        if (OnlineVersion != null)
        {
          if (vMpeInstalled >= OnlineVersion || vLavInstalled >= OnlineVersion)
          {
            result.state = CheckState.INSTALLED;
          }
          else
          {
            result.needsDownload = !File.Exists(FileName);
          }
        }
        else
        {
          result.state = CheckState.VERSION_LOOKUP_FAILED;
        }
      }
      else
      {
        result.needsDownload = !File.Exists(FileName);
      }

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
      }

      return result;
    }

    static Version GetInstalledLAVVersion()
    {
      RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{B98D13E7-55DB-4385-A33D-09FD1BA26338}\InprocServer32");
      if (key != null)
      {
        string ax = key.GetValue(null).ToString();
        if (File.Exists(ax))
        {
          FileVersionInfo info = FileVersionInfo.GetVersionInfo(ax);
          return new Version(info.ProductMajorPart, info.ProductMinorPart, info.ProductBuildPart, info.ProductPrivatePart);
        }
      }
      return null;
    }

  }
}
