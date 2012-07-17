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
using System.Net;
using System.Text;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class LAVFilterMPEInstall : IInstallationPackage
  {
    const string lavId = "b7738156-b6ec-4f0f-b1a8-b5010349d8b1";
    const string mpeUrl = "http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link_id=162&cf_id=24";
    const string mpeUpdateUrl = "http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link_id=162&cf_id=52";

    readonly string mpeUpdateFile = Application.StartupPath + "\\deploy\\" + "LAVFilters.xml";
    readonly string fileName = Application.StartupPath + "\\deploy\\" + "LAVFilters.mpe1";
    readonly string installedMpesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Team MediaPortal\MediaPortal\Installer\V2\InstalledExtensions.xml";

    static Version onlineVersion = null;

    public string GetDisplayName()
    {
      return "LAV Filters" + (onlineVersion != null ? " " + onlineVersion.ToString() : "");
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
        onlineVersion = GetLatestAvailableMPEVersion();
        if (onlineVersion != null)
        {
          if (vMpeInstalled >= onlineVersion) result.state = CheckState.INSTALLED;
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
      if (!File.Exists(mpeUpdateFile) || (DateTime.Now - new FileInfo(mpeUpdateFile).LastWriteTime).TotalMinutes > 60)
      {
        bool downloadSuccess = DownloadMpeUpdateXml();
        if (!downloadSuccess && !File.Exists(mpeUpdateFile)) return null;
      }
      try
      {
        XmlDocument xDoc = new XmlDocument();
        xDoc.Load(mpeUpdateFile);
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
          return versions[versions.Count - 1];
        }
      }
      catch (Exception ex)
      {
        // error checking for latest online version
      }
      return null;
    }

    /// <summary>
    /// Download the LAV Filter MPE update.xml that contains the information what is the latest version from the MediaPortal homepage and store it locally.
    /// </summary>
    /// <returns>true when the file was successfully downloaded and saved, otherwise false.</returns>
    bool DownloadMpeUpdateXml()
    {
      HttpWebResponse response = null;
      try
      {
        HttpWebRequest request = HttpWebRequest.Create(mpeUpdateUrl) as HttpWebRequest;
        if (request != null)
        {
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
          request.UserAgent = @"Mozilla/4.0 (compatible; MSIE 7.0;" + Utils.GetUserAgentOsString();
          request.UseDefaultCredentials = true;
          request.Timeout = 10000; // don't wait longer than 10 seconds for data to start receiving
          request.Accept = "*/*"; // we accept any content type
          request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate"); // we accept compressed content
          response = request.GetResponse() as HttpWebResponse;
          if (response == null) return false;
          Stream responseStream;
          if (response.ContentEncoding.ToLower().Contains("gzip"))
          {
            responseStream = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
          }
          else if (response.ContentEncoding.ToLower().Contains("deflate"))
          {
            responseStream = new System.IO.Compression.DeflateStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
          }
          else
          {
            responseStream = response.GetResponseStream();
          }
          // UTF8 is the default encoding as fallback
          Encoding responseEncoding = Encoding.UTF8;
          // try to get the response encoding if one was specified
          if (response.CharacterSet != null && !String.IsNullOrEmpty(response.CharacterSet.Trim())) responseEncoding = Encoding.GetEncoding(response.CharacterSet.Trim(new char[] { ' ', '"' }));
          using (StreamReader reader = new StreamReader(responseStream, responseEncoding, true))
          {
            string str = reader.ReadToEnd().Trim();
            File.WriteAllText(mpeUpdateFile, str);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        // error downloading update.xml for LAV Filter MPE
      }
      finally
      {
        if (response != null) ((IDisposable)response).Dispose();
      }
      return false;
    }
  }
}
