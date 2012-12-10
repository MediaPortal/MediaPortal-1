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
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MediaPortal.DeployTool
{
  public class SimpleCultureInfo
  {
    public string nativeName;
    public string name;

    public SimpleCultureInfo(string name, string nativeName)
    {
      this.name = name;
      this.nativeName = nativeName;
    }

    public override string ToString()
    {
      return nativeName;
    }
  }

  public sealed class Localizer
  {
    #region Singleton implementation

    private static readonly Localizer _instance = new Localizer();

    private Localizer()
    {
      _rscMan = new ResourceManager("MediaPortal.DeployTool.MediaPortal.DeployTool",
                                    System.Reflection.Assembly.GetExecutingAssembly());
    }

    public static Localizer Instance
    {
      get { return _instance; }
    }

    #endregion

    #region Variables

    private readonly ResourceManager _rscMan;

    #endregion

    public string GetString(string id)
    {
      return _rscMan.GetString(id);
    }

    public string GetDefaultString(string id)
    {
      return _rscMan.GetString(id, new CultureInfo("en-US"));
    }

    public static void SwitchCulture(string cultureId)
    {
      System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureId);
    }

    public static string GetBestTranslation(string ID)
    {
      string _translation = Instance.GetString(ID);
      return _translation.Length > 0 ? _translation : Instance.GetDefaultString(ID);
    }
  }

  internal class Utils
  {
    #region DialogHelper

    public static void ErrorDlg(string msg)
    {
      MessageBox.Show(msg, "MediaPortal Deploy Tool -- Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Environment.Exit(-1);
    }

    #endregion

    #region Download

    public static string GetDownloadString(string session_id, string node_id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";
      const string XmlUrl = "http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml";

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        dlg.ShowDialog(XmlUrl, XmlFile, GetUserAgentOsString());
      }
      try
      {
        doc.Load(XmlFile);
        XmlNode node = doc.SelectSingleNode("/Applications/" + session_id + "/" + node_id);
        return node.InnerText;
      }
      catch
      {
        MessageBox.Show(Localizer.GetBestTranslation("DownloadSettings_failed"), XmlUrl, MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
        File.Delete(XmlFile);
        Environment.Exit(-2);
      }
      return String.Empty;
    }

    public static DialogResult DownloadFile(string FileName, string prg)
    {
      DialogResult result;

      if (GetDownloadString(prg, "TYPE") == "Manual")
      {
        ManualDownload dlg = new ManualDownload();
        result = dlg.ShowDialog(GetDownloadString(prg, "URL"), Path.GetFileName(FileName),
                                Application.StartupPath + "\\deploy");
      }
      else
      {
        HTTPDownload dlg = new HTTPDownload();
        result = dlg.ShowDialog(GetDownloadString(prg, "URL"), FileName, GetUserAgentOsString());
      }
      return result;
    }

    public static DialogResult RetryDownloadFile(string FileName, string prg)
    {
      DialogResult result = DialogResult.Cancel;
      FileInfo FileInfo = new FileInfo(FileName);

      for (int i = 0; i < 5; i++)
      {
        if (File.Exists(FileName))
        {
          FileInfo.Refresh();
          if (FileInfo.Length > 10000)
          {
            break;
          }
          result = DownloadFile(FileName, prg);
        }
        else
          result = DownloadFile(FileName, prg);
        if (result == DialogResult.Cancel) break;
      }
      return result;
    }

    public static string LocalizeDownloadFile(string filename, string downloadtype, string prg)
    {
      // LangCode = "ITA"
      string LangCode = InstallationProperties.Instance["DownloadThreeLetterWindowsLanguageName"];
      if (String.IsNullOrEmpty(LangCode))
      {
        LangCode = CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName;
      }
      // LangCodeExt = "it-IT"
      string LangCodeExt = InstallationProperties.Instance["DownloadLanguageName"];
      if (String.IsNullOrEmpty(LangCodeExt))
      {
        LangCodeExt = CultureInfo.InstalledUICulture.Name;
      }
      string NewFileName = filename;

      // WMP11 native language download
      if (prg == InstallationChecks.WindowsMediaPlayerChecker.prg)
      {
        string arch = InstallationProperties.Instance["DownloadArch"];
        if (arch == "64")
        {
          NewFileName = filename.Replace("x86", "x64").Split('.')[0] + "-ENU.exe";
        }
        else
        {
          string suffix = LangCode == "ENU" ? LangCode : LangCodeExt;
          NewFileName = filename.Split('.')[0] + "-" + suffix + ".exe";
        }
      }
      return NewFileName;
    }

    internal static string GetUserAgentOsString()
    {
      return "Windows NT " + OSInfo.OSInfo.OSMajorVersion + "." + OSInfo.OSInfo.OSMinorVersion;
    }

    #endregion

    #region Uninstall

    public static void UninstallNSIS(string RegistryFullPathName)
    {
      string FileName = Path.GetFileName(RegistryFullPathName);
      string Directory = Path.GetDirectoryName(RegistryFullPathName);
      string TempFullPathName = Environment.GetEnvironmentVariable("TEMP") + "\\" + FileName;
      File.Copy(RegistryFullPathName, TempFullPathName, true);
      Process setup = Process.Start(TempFullPathName, " /S _?=" + Directory);
      if (setup != null)
      {
        setup.WaitForExit();
      }
      File.Delete(TempFullPathName);
    }

    public static void UninstallMSI(string clsid)
    {
      Process setup = Process.Start("msiexec.exe", "/x " + clsid + " /qn");
      if (setup != null)
      {
        setup.WaitForExit();
      }
      CheckUninstallString(clsid, true);
    }

    public static string CheckUninstallString(string clsid, bool delete)
    {
      string keyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + clsid;
      RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
      if (key != null)
      {
        string strUninstall = key.GetValue("UninstallString").ToString();
        if (File.Exists(strUninstall))
        {
          key.Close();
          return strUninstall;
        }
        if (delete)
        {
          key.DeleteSubKeyTree(keyPath);
        }
        key.Close();
      }
      return null;
    }

    public static CheckResult CheckNSISUninstallString(string RegistryPath, string MementoSection)
    {
      CheckResult result = new CheckResult();
      result.state = CheckState.NOT_INSTALLED;

      RegistryKey key =
        Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + RegistryPath);
      if (key != null)
      {
        int _IsInstalled = (int)key.GetValue(MementoSection, 0);
        int major = (int)key.GetValue("VersionMajor", 0);
        int minor = (int)key.GetValue("VersionMinor", 0);
        int revision = (int)key.GetValue("VersionRevision", 0);
        key.Close();

        Version ver = new Version(major, minor, revision);

#if DEBUG
        MessageBox.Show("Registry version = <" + ver + ">, IsUpdatable= " + IsPackageUpdatabled(ver) + ", IsInstalled=" + _IsInstalled,
          "Debug information: " +  MementoSection, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif

        if (_IsInstalled == 1)
        {
          result.state = IsPackageUpdatabled(ver) ? CheckState.VERSION_MISMATCH : CheckState.INSTALLED;
        }
        else
        {
          result.state = CheckState.NOT_INSTALLED;
        }
      }
      return result;
    }

    #endregion

    #region Misc checks

    public static bool CheckTargetDir(string dir)
    {
      if (dir == "")
        return false;
      if (Directory.Exists(dir))
        return true;
      try
      {
        Directory.CreateDirectory(dir);
      }
      catch
      {
        return false;
      }
      return true;
    }

    public static bool CheckFileVersion(string aFilePath, string aMinimumVersion, out Version aCurrentVersion)
    {
      aCurrentVersion = new Version(0, 0, 0, 0);
      try
      {
        Version desiredVersion = new Version(aMinimumVersion);
        FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(aFilePath);
        if (!string.IsNullOrEmpty(fileVersion.ProductVersion))
        {
          // Replace "," with "." because of versioning localization issues
          aCurrentVersion = new Version(fileVersion.ProductVersion.Replace(',', '.'));
          return aCurrentVersion >= desiredVersion;
        }
        return false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public static bool CheckStartupPath()
    {
      try
      {
        if (Directory.GetCurrentDirectory().StartsWith("\\"))
        {
          MessageBox.Show("Please start installation from a local drive.", Application.StartupPath, MessageBoxButtons.OK,
                          MessageBoxIcon.Stop);
          return false;
        }
        FileInfo file = new FileInfo(Application.ExecutablePath);
        DirectoryInfo dir = file.Directory;
        if (dir != null)
        {
          if ((dir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
          {
            MessageBox.Show("Need write access to startup directory.", Application.StartupPath, MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
            return false;
          }
        }
        return true;
      }
      catch
      {
        MessageBox.Show("Unable to determine startup path. Please try running from a local drive with write access.",
                        Application.StartupPath, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        return false;
      }
    }

    #endregion

    #region NotifyReboot

    public static bool AutoRunApplication(string action)
    {
      const string desc = "MediaPortal Installation";
      RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
      if (key != null)
      {
        if (action == "set")
        {
          key.SetValue(desc, Application.ExecutablePath);
        }
        else
        {
          try
          {
            key.DeleteValue(desc, true);
          }
          catch
          {
            return false;
          }
        }
        key.Close();
      }
      return true;
    }

    public static void NotifyReboot(string DisplayName)
    {
      // Write Run registry key
      Utils.AutoRunApplication("set");

      // Notify about the needed reboot
      MessageBox.Show(Localizer.GetBestTranslation("Reboot_Required"), DisplayName, MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);

      //Close DeployTool
      Environment.Exit(-3);

    }

    #endregion

    #region Operation System Version Check

    public static void CheckPrerequisites()
    {
      DialogResult res;

      switch (OSInfo.OSInfo.GetOSSupported())
      {
        case OSInfo.OSInfo.OsSupport.Blocked:
          MessageBox.Show(Localizer.GetBestTranslation("OS_Support"), OSInfo.OSInfo.GetOSDisplayVersion(),
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
          Application.Exit();
          break;
        case OSInfo.OSInfo.OsSupport.NotSupported:
          res = MessageBox.Show(Localizer.GetBestTranslation("OS_Warning"), OSInfo.OSInfo.GetOSDisplayVersion(),
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
          if (res == DialogResult.Cancel) Application.Exit();
          break;
      }
      if (OSInfo.OSInfo.OSServicePackMinor != 0)
      {
        res = MessageBox.Show(Localizer.GetBestTranslation("OS_Beta"), OSInfo.OSInfo.GetOSDisplayVersion(),
                              MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
        if (res == DialogResult.Cancel) Application.Exit();
      }
    }

    #endregion

    #region IsWow64 check

    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process(
      [In] IntPtr hProcess,
      [Out] out bool lpSystemInfo
      );

    public static bool Check64bit()
    {
      //IsWow64Process is not supported under Windows2000
      if (!OSInfo.OSInfo.XpOrLater())
      {
        return false;
      }

      Process p = Process.GetCurrentProcess();
      IntPtr handle = p.Handle;
      bool isWow64;
      bool success = IsWow64Process(handle, out isWow64);
      if (!success)
      {
        throw new System.ComponentModel.Win32Exception();
      }
      return isWow64;
    }

    #endregion

    #region Aero check

    [DllImport("dwmapi.dll")]
    public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

    public static bool IsAeroEnabled()
    {
      int enabled = 0;
      try
      {
        DwmIsCompositionEnabled(ref enabled);
      }
      catch (DllNotFoundException)
      {
        return false;
      }
      return (enabled == 1);
    }

    #endregion

    #region Logging

    private static readonly string _filePath = Application.StartupPath + "\\deploy\\DeployTool-" +
                                               DateTime.Now.ToString("dd-MM-yyyy HH_mm") + ".log";

    public void Log(string message)
    {
      DateTime dt = DateTime.Now;
      if (!File.Exists(_filePath))
      {
        FileStream fs = File.Create(_filePath);
        fs.Close();
      }
      try
      {
        StreamWriter sw = File.AppendText(_filePath);
        sw.WriteLine(dt.ToString("hh:mm:ss") + " " + message);
        sw.Flush();
        sw.Close();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    #endregion

    #region Version & Registry checks

    public static bool IsOfficialBuild(string build)
    {
      //
      // All official releases has "0" as build number
      // but 1.1.0 RC2 that was 25546
      //
      return (build == "0" || build == "25546");
    }

    public static Version GetPackageVersion(string type)
    {
      int major = 0;
      int minor = 0;
      int revision = 0;

      switch (type)
      {
        case "min":
          major = 1;
          minor = 0;
          revision = 5; // 1.0.5 = 1.1.0 RC1
          break;
        case "max":
          major = 1;
          minor = 2;
          revision = 100;
          break;
      }
      Version ver = new Version(major, minor, revision);
      return ver;
    }

    public static bool IsPackageUpdatabled(Version pkgVer)
    {
      if (pkgVer.CompareTo(GetPackageVersion("min")) >= 0 &&
          pkgVer.CompareTo(GetPackageVersion("max")) <= 0)
      {
        return true;
      }
      return false;
    }

    public static string PathFromRegistry(string regkey)
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey(regkey);
      string Tv3Path = null;

      if (key != null)
      {
        Tv3Path = (string)key.GetValue("UninstallString");
        key.Close();

      }
      return Tv3Path;
    }

    public static Version VersionFromRegistry(string regkey)
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey(regkey);
      int major = 0;
      int minor = 0;
      int revision = 0;

      if (key != null)
      {
        major = (int)key.GetValue("VersionMajor", 0);
        minor = (int)key.GetValue("VersionMinor", 0);
        revision = (int)key.GetValue("VersionRevision", 0);
        key.Close();
      }
      return new Version(major, minor, revision);
    }

    public static string GetDisplayVersion()
    {
      return "1.3.0 Beta";
    }

    /// <summary>
    /// Adds a record to deploy.xml (will be created if it does not exist).
    /// This will then be picked up by MP itself and applied to mediaportal.xml
    /// </summary>
    /// <param name="section">name attribute of section element in mediaportal.xml</param>
    /// <param name="entry">name attribute of entry element in mediaportal.xml</param>
    /// <param name="value">Value to be set for this section/entry</param>
    public static void SetDeployXml(string section, string entry, string value)
    {
      var commonAppsDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                          @"\Team MediaPortal\MediaPortal\";
      if (!Directory.Exists(commonAppsDir))
      {
        Directory.CreateDirectory(commonAppsDir);
      }

      var deployXmlLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              @"\Team MediaPortal\MediaPortal\deploy.xml";
      var deployXml = File.Exists(deployXmlLocation) ? XDocument.Load(deployXmlLocation) : new XDocument();
      var root = deployXml.Elements("deploySettings").FirstOrDefault();
      if (root == null)
      {
        deployXml.Add(new XElement("deploySettings"));
        root = deployXml.Elements("deploySettings").First();
      }

      var existingNode =
        (from x in deployXml.Elements("deploySettings").Elements()
         where (string) x.Attribute("section") == section &&
               (string) x.Attribute("entry") == entry
         select x).FirstOrDefault();

      if (existingNode == null)
      {
        root.Add(new XElement("deploySetting", new XAttribute("section", section), new XAttribute("entry", entry),
                              value));
      }
      else
      {
        existingNode.Value = value;
      }

      deployXml.Save(deployXmlLocation);
    }

    #endregion
  }
}