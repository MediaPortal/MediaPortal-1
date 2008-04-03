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
          try
          {
              DialogResult result = dlg.ShowDialog("http://install.team-mediaportal.com/DeployTool/ApplicationLocations.xml", XmlFile);
          }
          catch(System.UnauthorizedAccessException)
          {
              ErrorDlg("Need write access to: " + Application.StartupPath + " and subdirs.");
              Environment.Exit(-2);
          }
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
  }
}
