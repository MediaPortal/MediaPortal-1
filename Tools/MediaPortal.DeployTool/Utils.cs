#region Copyright (C) 2005-2008 Team MediaPortal
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Xml;
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

    public string GetDefaultString(string id)
    {
      return _rscMan.GetString(id, new CultureInfo("en-US"));
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

    public static string GetDownloadString(string session_id, string node_id)
    {
      XmlDocument doc = new XmlDocument();
      HTTPDownload dlg = new HTTPDownload();
      string XmlFile = Application.StartupPath + "\\ApplicationLocations.xml";
      string XmlUrl = "http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml";

      //HTTP update of the xml file with the application download URLs
      if (!File.Exists(XmlFile))
      {
        DialogResult result = dlg.ShowDialog(XmlUrl, XmlFile, GetUserAgentOsString());
      }
      try
      {
        doc.Load(XmlFile);
        XmlNode node = doc.SelectSingleNode("/Applications/" + session_id + "/" + node_id);
        return node.InnerText;
      }
      catch
      {
        // TODO: MessageBox.Show(Utils.GetBestTranslation("DownloadSettings_failed"), XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        MessageBox.Show("Download of settings file failed.\nPlease review your InternetExplorer configuration.\nCorrupted config file, if found, will now be deleted.", XmlUrl, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        File.Delete(XmlFile);
        Environment.Exit(-2);
      }
      return String.Empty;
    }

    public static DialogResult DownloadFile(string FileName, string prg)
    {
      DialogResult result;

      if (Utils.GetDownloadString(prg, "TYPE") == "Manual")
      {
        ManualDownload dlg = new ManualDownload();
        result = dlg.ShowDialog(Utils.GetDownloadString(prg, "URL"), Path.GetFileName(FileName), Application.StartupPath + "\\deploy");
      }
      else
      {
        HTTPDownload dlg = new HTTPDownload();
        result = dlg.ShowDialog(Utils.GetDownloadString(prg, "URL"), FileName, GetUserAgentOsString());
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
      string LangCode = System.Globalization.CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName;
      string LangCodeExt = System.Globalization.CultureInfo.CurrentCulture.Name;
      string NewFileName = filename;

      if (LangCode != "ENU")
      {
        // SQL2005 native language download
        if ((prg == "MSSQLExpress" + InstallationProperties.Instance["Sql2005Download"]) && downloadtype != "Manual")
        {
          NewFileName = filename.Split('.')[0] + "_" + LangCode + ".exe";
        }
        // WMP11 native language download
        if (prg == "WindowsMediaPlayer")
        {
          if (Utils.Check64bit())
          {
            NewFileName = filename.Replace("x86", "x64").Split('.')[0] + "-ENU.exe";
          }
          else
          {
            NewFileName = filename.Split('.')[0] + "-" + LangCodeExt + ".exe";
          }
        }
      }
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
      OsDetection.OperatingSystemVersion os = new OsDetection.OperatingSystemVersion();
      return "Windows NT " + os.OSMajorVersion + "." + os.OSMinorVersion;
    }

    public static void UninstallNSIS(string RegistryFullPathName)
    {
      Process setup;
      string FileName = Path.GetFileName(RegistryFullPathName);
      string Directory = Path.GetDirectoryName(RegistryFullPathName);
      string TempFullPathName = Environment.GetEnvironmentVariable("TEMP") + "\\" + FileName;
      File.Copy(RegistryFullPathName, TempFullPathName);
      setup = Process.Start(TempFullPathName, " /S _?=" + Directory);
      setup.WaitForExit();
      File.Delete(TempFullPathName);
    }

    public static void UninstallMSI(string clsid)
    {
      Process setup = Process.Start("msiexec.exe", "/x " + clsid + " /qn");
      setup.WaitForExit();
      CheckUninstallString(clsid, true);
    }

    public static string CheckUninstallString(string clsid, bool delete)
    {
      string strUninstall;
      string keyPath = "SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + clsid;
      RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
      if (key != null)
      {
        strUninstall = key.GetValue("UninstallString").ToString();
        if (File.Exists(strUninstall))
        {
          key.Close();
          return strUninstall;
        }
        else
        {
          if (delete) key.DeleteSubKeyTree(keyPath);
        }
        key.Close();
      }
      return null;
    }

    public static bool CheckFileVersion(string aFilePath, string aMinimumVersion, out Version aCurrentVersion)
    {
      aCurrentVersion = new Version(0, 0, 0, 0);
      try
      {
        System.Version desiredVersion = new System.Version(aMinimumVersion);
        FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(aFilePath);
        if (!string.IsNullOrEmpty(fileVersion.ProductVersion))
        {
          aCurrentVersion = new System.Version(fileVersion.ProductVersion);
          return aCurrentVersion >= desiredVersion;
        }
        else
          return false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    #region Operation System Version Check
    public static void CheckPrerequisites()
    {
      OsDetection.OSVersionInfo os = new OsDetection.OperatingSystemVersion();
      DialogResult res;

      string ServicePack = "";
      if (!string.IsNullOrEmpty(os.OSCSDVersion))
        ServicePack = " (" + os.OSCSDVersion + ")";
      string MsgOsVersion = os.OSVersionString + ServicePack;

      int ver = (os.OSMajorVersion * 10) + os.OSMinorVersion;

      // Disable OS if < XP
      if (ver < 51)
      {
        MessageBox.Show(Utils.GetBestTranslation("OS_Support"), MsgOsVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Exit();
      }
      switch (ver)
      {
        case 51:
          if (os.OSServicePackMajor < 2)
          {
            MessageBox.Show(Utils.GetBestTranslation("OS_Support"), MsgOsVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
          }
          break;
        case 52:
          if (os.OSProductType == OsDetection.OSProductType.Workstation)
          {
            MsgOsVersion = MsgOsVersion + " [64bit]";
            MessageBox.Show(Utils.GetBestTranslation("OS_Support"), MsgOsVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
          }
          res = MessageBox.Show(Utils.GetBestTranslation("OS_Warning"), MsgOsVersion, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
          if (res == DialogResult.Cancel) Application.Exit();
          break;
        case 60:
          if (os.OSProductType != OsDetection.OSProductType.Workstation || os.OSServicePackMajor < 1)
          {
            res = MessageBox.Show(Utils.GetBestTranslation("OS_Warning"), MsgOsVersion, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.Cancel) Application.Exit();
          }
          break;
      }
      if (os.OSServicePackBuild != 0)
      {
        res = MessageBox.Show(Utils.GetBestTranslation("OS_Beta"), MsgOsVersion, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
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
      OsDetection.OperatingSystemVersion os = new OsDetection.OperatingSystemVersion();
      //IsWow64Process is not supported under Windows2000
      if ((os.OSMajorVersion * 10) + os.OSMinorVersion == 50) return false;
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

    public static string GetBestTranslation(string ID)
    {
      string _translation = Localizer.Instance.GetString(ID);
      if (_translation.Length > 0)
      {
        return _translation;
      }
      else
      {
        return Localizer.Instance.GetDefaultString(ID);
      }
    }

    public static string GetPackageVersion()
    {
      return "1.0 RC3";
    }

  }
}
