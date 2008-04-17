using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MediaPortal.DeployTool
{
  public sealed class Localizer
  {
    #region Singleton implementation
    static readonly Localizer _instance = new Localizer();
    static Localizer()
    {
    }
    Localizer()
    {
      _rscMan = new ResourceManager("MediaPortal.DeployTool.MediaPortal.DeployTool", System.Reflection.Assembly.GetExecutingAssembly());
    }
    public static Localizer Instance
    {
      get
      {
        return _instance;
      }
    }
    #endregion

    #region Variables
    private ResourceManager _rscMan;
    #endregion

    public string GetString(string id)
    {
      return _rscMan.GetString(id);
    }
    public void SwitchCulture(string cultureId)
    {
      System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureId);
    }
  }

  class Utils
  {
    #region DialogHelper
    public static void ErrorDlg(string msg)
    {
      MessageBox.Show(msg, "MediaPortal Deploy Tool -- Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    public static void InfoDlg(string msg)
    {
      MessageBox.Show(msg, "MediaPortal Deploy Tool -- Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    #endregion

    public static string GetDownloadURL(string id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";
      string XmlUrl = GetDownloadSettingsUrl();

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog(XmlUrl, XmlFile, GetUserAgentOsString());
      }
      try
      {
        doc.Load(XmlFile);
        XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/URL");
        return node.InnerText;
      }
      catch
      {
        // TODO: MessageBox.Show(Localizer.Instance.GetString("DownloadSettings_failed"), XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        MessageBox.Show("Download of settings file failed. Please review your InternetExplorer configuration.", XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Environment.Exit(-2);
      }
      return String.Empty;
    }

    public static string GetDownloadFile(string id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";
      string XmlUrl = GetDownloadSettingsUrl();

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog(XmlUrl, XmlFile, GetUserAgentOsString());
      }
      try
      {
        doc.Load(XmlFile);
        XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/FILE");
        return node.InnerText;
      }
      catch
      {
        // TODO: MessageBox.Show(Localizer.Instance.GetString("DownloadSettings_failed"), XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        MessageBox.Show("Download of settings file failed. Please review your InternetExplorer configuration.", XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Environment.Exit(-2);
      }
      return String.Empty;
    }

    public static string GetDownloadType(string id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";
      string XmlUrl = GetDownloadSettingsUrl();

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog(XmlUrl, XmlFile, GetUserAgentOsString());
      }
      try
      {
        doc.Load(XmlFile);
        XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/TYPE");
        return node.InnerText;
      }
      catch
      {
        // TODO: MessageBox.Show(Localizer.Instance.GetString("DownloadSettings_failed"), XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        MessageBox.Show("Download of settings file failed. Please review your InternetExplorer configuration.", XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Environment.Exit(-2);
      }
      return String.Empty;
    }

    public static DialogResult DownloadFile(string prg)
    {
      DialogResult result;
      string FileName;

      //Ack for SQL2005 native language download
      if (prg == "MSSQLExpress" + InstallationProperties.Instance["Sql2005Download"])
        FileName = LocalizeDownloadFile(Utils.GetDownloadFile(prg), Utils.GetDownloadType(prg));
      else
        FileName = Utils.GetDownloadFile(prg);

      if (Utils.GetDownloadType(prg) == "Manual")
      {
        ManualDownload dlg = new ManualDownload();
        result = dlg.ShowDialog(Utils.GetDownloadURL(prg), FileName, Application.StartupPath + "\\deploy");
      }
      else
      {
        HTTPDownload dlg = new HTTPDownload();
        result = dlg.ShowDialog(Utils.GetDownloadURL(prg), Application.StartupPath + "\\deploy\\" + FileName, GetUserAgentOsString());
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
          if (FileInfo.Length > 10000)
            break;
          else
            result = DownloadFile(prg);
        }
        else
          result = DownloadFile(prg);
        if (result == DialogResult.Cancel) break;
      }
      return result;
    }

    public static string LocalizeDownloadFile(string filename, string downloadtype)
    {
      string LangCode = System.Globalization.CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName;
      string NewFileName = "";
      if (LangCode == "ENU" || downloadtype == "Manual")
        NewFileName = filename;
      else
        NewFileName = filename.Split('.')[0] + "_" + LangCode + ".exe";
      return NewFileName;
    }

    public static bool CheckTargetDir(string dir)
    {
      if (dir == "")
        return false;
      if (Directory.Exists(dir))
        return true;
      DirectoryInfo info = null;
      try
      {
        info = Directory.CreateDirectory(dir);
      }
      catch
      {
        return false;
      }
      if (info == null)
        return false;
      else
      {
        Directory.Delete(dir);
        return true;
      }
    }

    private static string GetUserAgentOsString()
    {
      string OsVersion = CheckOSRequirement(false);
      if (!string.IsNullOrEmpty(OsVersion))
      {
        if (OsVersion == "Windows Vista" || OsVersion == "Windows 2008")
          return "Windows NT 6.0";
        else
          if (OsVersion == "Windows 2003 Server")
            return "Windows NT 5.2";
      }
      return "Windows NT 5.1"; // XP
    }

    #region Operation System Version Check
    [DllImport("kernel32.dll")]
    private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct OSVERSIONINFOEX
    {
      public int dwOSVersionInfoSize;
      public int dwMajorVersion;
      public int dwMinorVersion;
      public int dwBuildNumber;
      public int dwPlatformId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string szCSDVersion;
      public short wServicePackMajor;
      public short wServicePackMinor;
      public short wSuiteMask;
      public byte wProductType;
      public byte wReserved;
    }

    public static string CheckOSRequirement(bool NotifyUnsupported)
    {
      bool OsSupport = false;

      OsDetection.OperatingSystemVersion os = new OsDetection.OperatingSystemVersion();
      string OsDesc = os.VersionString;

      switch (os.OSMajorVersion)
      {
        case 4:                                       // 4.x = Win95,98,ME and NT 
          OsSupport = false;
          break;

        case 5:
          switch (os.OSMinorVersion)
          {
            case 0:                                   // 5.0 = Windows2000
              OsSupport = false;
              break;

            case 1:                                   // 5.1 = WindowsXP 32bit

              if (os.OSServicePackMajor < 2)
              {
                OsDesc = "Windows XP ServicePack 1";
                OsSupport = false;
              }
              else
              {
                OsSupport = true;
              }
              break;

            case 2:                                   // 5.2 = Windows2003 e WindowsXP 64bit
              if (os.OSProductType == OsDetection.OSProductType.Workstation)
              {
                OsSupport = true;
                OsDesc = "Windows XP 64bit";
                DialogResult btn = MessageBox.Show(Localizer.Instance.GetString("OS_Warning"), OsDesc, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (btn == DialogResult.Cancel) Environment.Exit(-1);
              }
              else
              {
                OsSupport = true;
                OsDesc = "Windows 2003 Server";
                DialogResult btn = MessageBox.Show(Localizer.Instance.GetString("OS_Warning"), OsDesc, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (btn == DialogResult.Cancel) Environment.Exit(-1);
              }
              break;
          }
          break;

        case 6:                                      // 6.0 = WindowsVista and Windows2008
          if (os.OSProductType == OsDetection.OSProductType.Workstation)
          {
            OsSupport = true;
            OsDesc = "Windows Vista";
          }
          else
          {
            OsSupport = true;
            OsDesc = "Windows 2008";
            DialogResult btn = MessageBox.Show(Localizer.Instance.GetString("OS_Warning"), OsDesc, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (btn == DialogResult.Cancel) Environment.Exit(-1);
          }
          break;

      }
      if (!OsSupport && NotifyUnsupported)
      {
        MessageBox.Show(Localizer.Instance.GetString("OS_Support"), OsDesc, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Environment.Exit(-1);
      }
      return OsDesc;
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
      if (CheckOSRequirement(false) == "Windows 2000") return false;
      Process p = Process.GetCurrentProcess();
      IntPtr handle = p.Handle;
      bool isWow64;
      bool success = IsWow64Process(handle, out isWow64);
      if (!success)
        throw new System.ComponentModel.Win32Exception();
      else
        if (isWow64)
          return true;
        else
          return false;
    }
    #endregion

    public static bool CheckStartupPath()
    {
      try
      {

        if (Directory.GetCurrentDirectory().StartsWith("\\"))
        {
          MessageBox.Show("Please start installation from a local drive.", Application.StartupPath, MessageBoxButtons.OK, MessageBoxIcon.Stop);
          return false;
        }
        FileInfo file = new FileInfo(Application.ExecutablePath);
        DirectoryInfo dir = file.Directory;
        if ((dir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        {
          MessageBox.Show("Need write access to startup directory.", Application.StartupPath, MessageBoxButtons.OK, MessageBoxIcon.Stop);
          return false;
        }
        return true;
      }
      catch
      {
        MessageBox.Show("Unable to determine startup path. Please try running from a local drive with write access.", Application.StartupPath, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        return false;
      }
    }

    public static string GetPackageVersion()
    {
      return "1.0 RC1";
    }

    public static string GetDownloadSettingsUrl()
    {
      return "http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml";
    }

  }
}
