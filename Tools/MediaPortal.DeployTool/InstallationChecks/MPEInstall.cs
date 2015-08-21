using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.DeployTool.InstallationChecks
{
  /// <summary>
  /// Template to easily add MPEI installs to deploy tool.
  /// </summary>
  internal abstract class MPEInstall : IInstallationPackage
  {

    #region variables and properties

    protected static readonly string InstalledMpesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Team MediaPortal\MediaPortal\Installer\V2\InstalledExtensions.xml";

    protected Version OnlineVersion = null;

    protected string MpeId { get; set; }
    protected string MpeURL { get; set; }
    protected string MpeUpdateURL { get; set; }
    protected string MpeUpdateFile { get; set; }
    protected string FileName { get; set; }

    #endregion

    #region IInstallationPackage implementations

    public abstract string GetDisplayName();

    public bool Install()
    {
      if (File.Exists(FileName))
      {
        string mpeExePath = Path.Combine(InstallationProperties.Instance["MPDir"], "MpeInstaller.exe");
        if (File.Exists(mpeExePath))
        {
          Process setup = Process.Start(mpeExePath, String.Format(@"/S ""{0}""", FileName));
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
        Process setup = Process.Start(mpeExePath, String.Format(@"/Uninstall={0}", MpeId));
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

    public abstract CheckResult CheckStatus();

    #endregion

    #region MPE methods

    public bool Download()
    {
      var dlg = new HTTPDownload();
      return dlg.ShowDialog(MpeURL, FileName, Utils.GetUserAgentOsString()) == DialogResult.OK;
    }

    /// <summary>
    /// Gets the MPE version number from an xml MPE file
    /// </summary>
    /// <param name="xmlLocation">Location of MPE xml file</param>
    /// <param name="xPathExpression">xPath expression to get version number from</param>
    /// <returns>Version in xml file</returns>
    protected Version GetMpeVersionFromXml(string xmlLocation,string xPathExpression)
    {
      try
      {
        var xDoc = new XmlDocument();
        xDoc.Load(xmlLocation);
        var versionNode = xDoc.SelectNodes(xPathExpression);
        var versions = new List<Version>();
        if (versionNode == null)
        {
          return null;
        }
        foreach (XmlElement versionNodee in versionNode)
        {
          int iMajor = 0;
          int iMinor = 0;
          int iBuild = 0;
          int iRevision = 0;
          XmlNode xMajor = versionNodee.SelectSingleNode("Major");
          XmlNode xMinor = versionNodee.SelectSingleNode("Minor");
          XmlNode xBuild = versionNodee.SelectSingleNode("Build");
          XmlNode xRevision = versionNodee.SelectSingleNode("Revision");
          var bMajor = xMajor != null && int.TryParse(xMajor.InnerText, out iMajor);
          var bMinor = xMinor != null && int.TryParse(xMinor.InnerText, out iMinor);
          var bBuild = xBuild != null && int.TryParse(xBuild.InnerText, out iBuild);
          var bRevision = xRevision != null && int.TryParse(xRevision.InnerText, out iRevision);

          if (bMajor && bMinor && bBuild && bRevision)
          {
            versions.Add(new Version(iMajor, iMinor, iBuild, iRevision));
          }
        }
        if (versions.Count > 0)
        {
          versions.Sort();
          return versions[versions.Count - 1];
        }

      }
      catch (Exception)
      {
        //problem reading xml file
      }
      return null;
    }

    /// <summary>
    /// Gets the current MPE version installed
    /// </summary>
    /// <returns>Version of installed MPE</returns>
    protected Version GetInstalledMpeVersion()
    {
      if (!File.Exists(InstalledMpesPath))
      {
        return null;
      }
      return GetMpeVersionFromXml(InstalledMpesPath, "//PackageClass/GeneralInfo[Id/text()='" + MpeId + "']/Version");
    } 

    /// <summary>
    /// Checks remote MPE file to check the latest version available
    /// </summary>
    /// <returns>Version of latest MPE available</returns>
    protected Version GetLatestAvailableMpeVersion()
    {
      if (!File.Exists(MpeUpdateFile) || (DateTime.Now - new FileInfo(MpeUpdateFile).LastWriteTime).TotalMinutes > 60)
      {
        bool downloadSuccess = DownloadMpeUpdateXml();
        if (!downloadSuccess && !File.Exists(MpeUpdateFile)) return null;
      }
      return GetMpeVersionFromXml(MpeUpdateFile, "//PackageClass/GeneralInfo/Version");
    }

    /// <summary>
    /// Download the MPE update.xml that contains the information what is the latest version from the MediaPortal homepage and store it locally.
    /// </summary>
    /// <returns>true when the file was successfully downloaded and saved, otherwise false.</returns>
    protected bool DownloadMpeUpdateXml()
    {
      HttpWebResponse response = null;
      try
      {
        var request = HttpWebRequest.Create(MpeUpdateURL) as HttpWebRequest;
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
          var responseEncoding = Encoding.UTF8;
          // try to get the response encoding if one was specified
          if (response.CharacterSet != null && !String.IsNullOrEmpty(response.CharacterSet.Trim())) responseEncoding = Encoding.GetEncoding(response.CharacterSet.Trim(new char[] { ' ', '"' }));
          using (var reader = new StreamReader(responseStream, responseEncoding, true))
          {
            string str = reader.ReadToEnd().Trim();
            File.WriteAllText(MpeUpdateFile, str);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        // error downloading update.xml for MPE
      }
      finally
      {
        if (response != null) ((IDisposable)response).Dispose();
      }
      return false;
    }

    #endregion

  }
}
