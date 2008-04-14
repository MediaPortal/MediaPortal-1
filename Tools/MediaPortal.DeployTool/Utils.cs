using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Xml;
using Microsoft.Win32;
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

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile, GetUserAgentOsString());
      }
      doc.Load(XmlFile);
      XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/URL");
      return node.InnerText;
    }

    public static string GetDownloadFile(string id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile, GetUserAgentOsString());
      }
      doc.Load(XmlFile);
      XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/FILE");
      return node.InnerText;
    }

    public static string GetDownloadType(string id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile, GetUserAgentOsString());
      }
      doc.Load(XmlFile);
      XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/TYPE");
      return node.InnerText;
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
        if (OsVersion == "Windows Vista")
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
      OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
      OperatingSystem osInfo = Environment.OSVersion;

      osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));  

      bool OsSupport = false;
      string OsDesc = "";

      switch (osInfo.Version.Major)
      {
        case 4:                         // 4.x = Win95,98,ME and NT 
          OsDesc = "Windows 95/98/ME/NT";
          OsSupport = false;
          break;

        case 5:
            if (osInfo.Version.Minor == 0)   // 5.0 = Windows2000
          {
            OsDesc = "Windows 2000";
            OsSupport = false;
          }
          if (osInfo.Version.Minor == 1)   // 5.1 = WindowsXP
          {
            if (osVersionInfo.wServicePackMajor < 2)
            {
              OsDesc = "Windows XP ServicePack 1";
              OsSupport = false;
            }
            else if (Check64bit())
            {
              OsDesc = "Windows XP 64bit";
              OsSupport = false;
            }
            else
            {
              OsSupport = true;
              OsDesc = "Windows XP";
            }
          }
          if (osInfo.Version.Major == 2)   // 5.2 = Windows2003
          {
            OsSupport = true;
            OsDesc = "Windows 2003 Server";
            DialogResult btn = MessageBox.Show(Localizer.Instance.GetString("OS_Warning"), OsDesc, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (btn == DialogResult.Cancel) Environment.Exit(-1);
          }
          break;

        case 6:                       
            if (osVersionInfo.wProductType != 3)       // Windows Vista
            {
                OsSupport = true;
                OsDesc = "Windows Vista";
            }
            else                                       // Windows 2008
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

  public static bool Check64bit()
    {
        try
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows");
            if (key == null)
            {
                InstallationProperties.Instance.Set("RegistryKeyAdd", "");
                InstallationProperties.Instance.Set("Sql2005Download", "32");
                return false;
            }
            else
            {
                key.Close();
                InstallationProperties.Instance.Set("RegistryKeyAdd", "Wow6432Node\\");
                InstallationProperties.Instance.Set("Sql2005Download", "64");
                return true;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show("DEBUG: Check64bit() - Exception: " + e.Message + "( " + e.StackTrace + " )");
        }
        return false;
    }

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
  }
}
