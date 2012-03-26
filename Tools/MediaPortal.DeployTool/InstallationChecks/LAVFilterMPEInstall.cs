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
using System.Xml;
using System.Collections.Generic;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class LAVFilterMPEInstall : IInstallationPackage
  {
    static Version cachedLatestOnlineVersion = null;

    const string lavId = "b7738156-b6ec-4f0f-b1a8-b5010349d8b1";
    const string mpeUrl = "http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link_id=162&cf_id=24";
    const string mpeUpdateUrl = "http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link_id=162&cf_id=52";

    readonly string fileName = Application.StartupPath + "\\deploy\\" + "LAVFilters.mpe1";
    readonly string installedMpesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Team MediaPortal\MediaPortal\Installer\V2\InstalledExtensions.xml";

    public string GetDisplayName()
    {
      return "Install LAV Filters";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();
      return dlg.ShowDialog(mpeUrl, fileName, Utils.GetUserAgentOsString()) == DialogResult.OK;
    }

    public bool Install()
    {
      if (File.Exists(fileName))
      {
        string mpeExePath = Path.Combine(InstallationProperties.Instance["MPDir"], "MpeInstaller.exe");
        if (File.Exists(mpeExePath))
        {
          Process setup = Process.Start(mpeExePath, String.Format(@"/S ""{0}""", fileName));
          if (setup != null)
          {
            setup.WaitForExit();
            if (setup.ExitCode == 0)
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    public bool UnInstall()
    {
      string mpeExePath = Path.Combine(InstallationProperties.Instance["MPDir"], "MpeInstaller.exe");
      if (File.Exists(mpeExePath))
      {
        Process setup = Process.Start(mpeExePath, String.Format(@"/Uninstall={0}", lavId));
        if (setup != null)
        {
          setup.WaitForExit();
          if (setup.ExitCode == 0)
          {
            return true;
          }
        }
      }
      return false;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result = default(CheckResult);

      // check if the user does not want LAV installed
      if (InstallationProperties.Instance["ConfigureMediaPortalLAV"] == "0")
      {
        result.state = CheckState.SKIPPED;
        return result;
      }

      // check if mpe package is installed and also check if LAV is actually installed
      Version vMpeInstalled = GetInstalledLAVMpeVersion();
      Version vLavInstalled = GetInstalledLAVVersion();
      if (vLavInstalled != null && vMpeInstalled != null)
      {
        Version vOnline = GetLatestAvailableMPEVersion();
        if (vOnline != null)
        {
          if (vMpeInstalled >= vOnline) result.state = CheckState.INSTALLED;
          else result.needsDownload = !File.Exists(fileName);
        }
        else
        {
          result.state = CheckState.VERSION_LOOKUP_FAILED;
        }
      }
      else
      {
        result.needsDownload = !File.Exists(fileName);
      }
      return result;
    }

    Version GetInstalledLAVVersion()
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

    Version GetInstalledLAVMpeVersion()
    {
      try
      {
        if (File.Exists(installedMpesPath))
        {
          XmlDocument xDoc = new XmlDocument();
          xDoc.Load(installedMpesPath);
          var versionNode = xDoc.SelectNodes("//PackageClass/GeneralInfo[Id/text()='" + lavId + "']/Version");
          List<Version> versions = new List<Version>();
          foreach (XmlElement versionNodee in versionNode)
          {
            versions.Add(new Version(
            int.Parse(versionNodee.SelectSingleNode("Major").InnerText),
              int.Parse(versionNodee.SelectSingleNode("Minor").InnerText),
                int.Parse(versionNodee.SelectSingleNode("Build").InnerText),
                  int.Parse(versionNodee.SelectSingleNode("Revision").InnerText)));
          }
          if (versions.Count > 0)
          {
            versions.Sort();
            return versions[versions.Count - 1];
          }
        }
      }
      catch (Exception ex)
      {
        // error checking for latest installed MPE version
      }
      return null;
    }

    Version GetLatestAvailableMPEVersion()
    {
      if (cachedLatestOnlineVersion == null)
      {
        try
        {
          XmlDocument xDoc = new XmlDocument();
          xDoc.Load(mpeUpdateUrl);
          var versionNode = xDoc.SelectNodes("//PackageClass/GeneralInfo/Version");
          List<Version> versions = new List<Version>();
          foreach (XmlElement versionNodee in versionNode)
          {
            versions.Add(new Version(
            int.Parse(versionNodee.SelectSingleNode("Major").InnerText),
              int.Parse(versionNodee.SelectSingleNode("Minor").InnerText),
                int.Parse(versionNodee.SelectSingleNode("Build").InnerText),
                  int.Parse(versionNodee.SelectSingleNode("Revision").InnerText)));
          }
          if (versions.Count > 0)
          {
            versions.Sort();
            cachedLatestOnlineVersion = versions[versions.Count - 1];
          }
        }
        catch (Exception ex)
        {
          // error checking for latest online version
        }
      }
      return cachedLatestOnlineVersion;
    }
  }
}
