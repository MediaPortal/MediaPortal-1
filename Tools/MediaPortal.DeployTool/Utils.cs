using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Xml;

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
      MessageBox.Show(msg,"MediaPortal Deploy Tool -- Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
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
      if(!File.Exists(XmlFile))
      {
          DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile);
      }
      doc.Load(XmlFile);
      XmlNode node=doc.SelectSingleNode("/Applications/"+id+"/URL");
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
          DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile);
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
              DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile);
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
        if (prg == "MSSQLExpress")
            FileName = LocalizeDownloadFile(Utils.GetDownloadFile(prg));
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
            result = dlg.ShowDialog(Utils.GetDownloadURL(prg), Application.StartupPath + "\\deploy\\" + FileName);
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


    public static string LocalizeDownloadFile(string filename)
    {
        string LangCode = System.Globalization.CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName;
        string NewFileName = "";
        if (LangCode == "ENU")
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
      DirectoryInfo info=null;
      try
      {
        info=Directory.CreateDirectory(dir);
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

    public static void CheckOSRequirement()
    {
        Version OsVersion = Environment.OSVersion.Version;
        bool OsSupport = false;
        string OsDesc = "";

        switch (OsVersion.Major)
        {
            case 4:                         // 4.x = Win95,98,ME and NT 
                OsDesc = "Windows 95/98/ME/NT";
                OsSupport = false;
                break;
            case 5:
                if (OsVersion.Minor == 0)   // 5.0 = Windows2000
                {
                    OsDesc = "Windows 2000";
                    OsSupport = false;
                }
                if (OsVersion.Minor == 1)   // 5.1 = WindowsXP
                {
                    if (int.Parse(Environment.OSVersion.ServicePack.Substring("Service Pack ".Length, 1)) < 2)
                    {
                        OsDesc = "Windows XP ServicePack 1";
                        OsSupport = false;
                    }
                    else if (IntPtr.Size == 8)
                    {
                        OsDesc = "Windows XP 64bit";
                        OsSupport = false;
                    }
                    else
                        OsSupport = true;
                }
                if (OsVersion.Major == 2)   // 5.2 = Windows2003
                    OsSupport = true;
                break;
            case 6:                         // 6.0 = WindowsVista, Windows2008
                OsSupport = true;
                break;
        }
        if (!OsSupport)
        {
            MessageBox.Show(Localizer.Instance.GetString("OS_Support"), OsDesc, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            Environment.Exit(-1);
        }
    }

    public static void Check64bit()
    {
        if (IntPtr.Size == 8)
            InstallationProperties.Instance.Set("RegistryKeyAdd", "Wow6432Node\\");
        else
            InstallationProperties.Instance.Set("RegistryKeyAdd", "");
    }

    public static bool CheckStartupPath()
    {
        if (Application.StartupPath.StartsWith("\\"))
        {
            MessageBox.Show(Localizer.Instance.GetString("Startup_UNC"), Application.StartupPath, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return false;
        }
        FileInfo file = new FileInfo(Application.ExecutablePath);
        DirectoryInfo dir = file.Directory;
        if((dir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        {
            MessageBox.Show(Localizer.Instance.GetString("Startup_Readonly"), Application.StartupPath, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return false;
        }
        return true;
    }
  }
}
