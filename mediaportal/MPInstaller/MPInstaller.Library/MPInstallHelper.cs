using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal;

namespace MediaPortal.MPInstaller
{
  public class MPInstallHelper
  {
    public ArrayList items = new ArrayList();
    string InstallDir = Config.GetFolder(Config.Dir.Installer);
    public string FileName = "";

    public ArrayList Items
    {
      get { return items; }
      set { items = value; }
    }


    public MPInstallHelper()
    {
      FileName = MpiFileList.LOCAL_LISTING;
      //LoadFromFile();
    }

    public void NormalizeNames()
    {
      string rfile = InstallDir + @"\cleanup.xml";
      MpInstallerNameReplace replecer = new MpInstallerNameReplace();
      if (File.Exists(rfile))
      {
        replecer.Load(rfile);
      }
      foreach (MPpackageStruct pk in this.Items)
      {
        if (replecer.GroupSubstitutions.ContainsKey(pk.InstallerInfo.Group))
        {
          pk.InstallerInfo.Group = replecer.GroupSubstitutions[pk.InstallerInfo.Group];
        }

        foreach (KeyValuePair<string, string> kpv in replecer.NameCleanups)
        {
          Regex replacregex = new Regex(kpv.Key);
          pk.InstallerInfo.Name = replacregex.Replace(pk.InstallerInfo.Name, kpv.Value);
        }
      }
    }

    public void Compare(MPInstallHelper mp)
    {
      foreach (MPpackageStruct pk in this.Items)
      {
        pk.isNew = true;
      }
      foreach (MPpackageStruct pk in mp.Items)
      {
        int idx = this.IndexOf(pk);
        if (idx > -1)
        {
          if (((MPpackageStruct)this.Items[idx]).InstallerInfo.Version.CompareTo(pk.InstallerInfo.Version) > 0)
            ((MPpackageStruct)this.Items[idx]).isUpdated = true;
          ((MPpackageStruct)this.Items[idx]).isNew = false;

        }
      }
    }

    public void Add(MPpackageStruct pk)
    {
      for (int i = 0; i < Items.Count; i++)
      {
        if (((MPpackageStruct)Items[i]).InstallerInfo.Name.Trim().ToUpper() == pk.InstallerInfo.Name.Trim().ToUpper())
          Items.RemoveAt(i);
      }
      Items.Add(pk);
      if (!Directory.Exists(InstallDir))
      {
        Directory.CreateDirectory(InstallDir);
      }
      if (Path.GetFullPath(pk.FileName) != Path.GetFullPath(InstallDir + @"\" + pk.GetLocalFilename()))
        if (File.Exists(Path.GetFullPath(pk.FileName)))
          File.Copy(pk.FileName, InstallDir + @"\" + pk.GetLocalFilename(), true);
    }

    public void AddRange(MPInstallHelper mpih)
    {
      foreach (MPpackageStruct pk in mpih.Items)
      {
        this.Add(pk);
      }
    }

    public int IndexOf(MPpackageStruct pk)
    {
      for (int i = 0; i < Items.Count; i++)
      {
        if (((MPpackageStruct)Items[i]).InstallerInfo.Name.Trim() == pk.InstallerInfo.Name.Trim())
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Finds the specified named package.
    /// If not found return null
    /// </summary>
    /// <param name="name">The name of package</param>
    /// <returns></returns>
    public MPpackageStruct Find(string name)
    {
      for (int i = 0; i < Items.Count; i++)
      {
        if (((MPpackageStruct)Items[i]).InstallerInfo.Name.Trim().ToUpper() == name.Trim().ToUpper())
          return (MPpackageStruct)Items[i];
      }
      return null;
    }

    public void SaveToFile()
    {
      if (!Directory.Exists(InstallDir))
      {
        Directory.CreateDirectory(InstallDir);
      }
      Stream myStream;
      if ((myStream = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.None)) != null)
      {
        // Code to write the stream goes here.
        XmlDocument doc = new XmlDocument();
        XmlWriter writer = null;
        try
        {
          // Create an XmlWriterSettings object with the correct options. 
          XmlWriterSettings settings = new XmlWriterSettings();
          string st = string.Empty;
          settings.Indent = true;
          settings.IndentChars = ("\t");
          settings.OmitXmlDeclaration = true;
          // Create the XmlWriter object and write some content.
          writer = XmlWriter.Create(myStream, settings);
          writer.WriteStartElement("MPinstalerS");
          writer.WriteElementString("ver", "1.00.000");
          writer.WriteStartElement("ExtensionList");
          for (int i = 0; i < this.Items.Count; i++)
          {
            MPpackageStruct it = (MPpackageStruct)this.Items[i];
            writer.WriteStartElement("Extension");
            writer.WriteElementString("FileName", Path.GetFileName(it.FileName));
            writer.WriteElementString("Name", it.InstallerInfo.Name);
            writer.WriteElementString("URL", it.InstallerInfo.UpdateURL);
            writer.WriteElementString("Version", it.InstallerInfo.Version);
            writer.WriteElementString("Author", it.InstallerInfo.Author);
            writer.WriteElementString("Description", it.InstallerInfo.Description);
            writer.WriteElementString("Group", it.InstallerInfo.Group);
            it.InstallerInfo.WriteLogoElement(writer);
            writer.WriteStartElement("Properties");
            it.InstallerInfo.ProjectProperties.Save(writer);
            writer.WriteEndElement();
            writer.WriteStartElement("Uninstall");
            for (int j = 0; j < it.InstallerInfo.Uninstall.Count; j++)
            {
              writer.WriteStartElement("FileInfo");
              writer.WriteElementString("FileName", ((UninstallInfo)it.InstallerInfo.Uninstall[j]).Path);
              writer.WriteElementString("Date", Path.GetFileName(((UninstallInfo)it.InstallerInfo.Uninstall[j]).Date.ToFileTime().ToString()));
              writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.WriteStartElement("Option");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.Flush();
        }
        finally
        {
          if (writer != null)
            writer.Close();
        }
        myStream.Close();
      }
    }

    public static bool Download(string url, string fil)
    {
      int code = 0;

      if (!Win32API.IsConnectedToInternet(ref code))
      {
        return false;
      }
      using (WebClient client = new WebClient())
      {
        try
        {
          client.DownloadFile(url, fil);
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    public void LoadFromFile(string fil)
    {
      FileName = fil;
      LoadFromFile();
    }

    /// <summary>
    /// Loads from FileName specified file.
    /// By default from <b>InstallDir + @"\" + "config.xml</b>"
    /// </summary>
    public void LoadFromFile()
    {
      XmlDocument doc = new XmlDocument();
      if (File.Exists(FileName))
      {
        try
        {
          doc.Load(FileName);
          Items.Clear();
          XmlNode ver = doc.DocumentElement.SelectSingleNode("/MPinstalerS");
          XmlNodeList fileList = ver.SelectNodes("ExtensionList/Extension");
          foreach (XmlNode nodefile in fileList)
          {
            MPpackageStruct pkg = new MPpackageStruct();
            pkg.FileName = nodefile.SelectSingleNode("FileName").InnerText;
            pkg.InstallerInfo.Name = nodefile.SelectSingleNode("Name").InnerText;
            pkg.InstallerInfo.Author = nodefile.SelectSingleNode("Author").InnerText;
            pkg.InstallerInfo.Version = nodefile.SelectSingleNode("Version").InnerText;
            pkg.InstallerInfo.UpdateURL = nodefile.SelectSingleNode("URL").InnerText;
            pkg.DownloadCount = GetIntValueFromNode(nodefile, "Downloads", 0);
            pkg.VoteCount = GetIntValueFromNode(nodefile, "Vote/Count", 0);
            pkg.VoteValue = GetIntValueFromNode(nodefile, "Vote/Value", 0);
            pkg.ScreenUrl = GetStringValueFromNode(nodefile, "Screenurl", string.Empty);
            string updateDate = GetStringValueFromNode(nodefile, "Submitdate", string.Empty);
            XmlNode grup_node = nodefile.SelectSingleNode("Group");
            if (grup_node != null)
              pkg.InstallerInfo.Group = grup_node.InnerText;
            XmlNode node_logo = nodefile.SelectSingleNode("Logo");
            if (node_logo != null)
            {
              byte[] buffer = Convert.FromBase64String(node_logo.InnerText);
              string t = Path.GetTempFileName();
              FileStream fs = new FileStream(t, FileMode.Create);
              fs.Write(buffer, 0, buffer.Length);
              fs.Close();
              pkg.InstallerInfo.Logo = Image.FromFile(t, true);
              try
              {
                File.Delete(t);
              }
              catch (Exception)
              {

              }
            }
            XmlNode node_des = nodefile.SelectSingleNode("Description");
            XmlNodeList uninstallList = nodefile.SelectNodes("Uninstall/FileInfo");
            foreach (XmlNode un in uninstallList)
            {
              pkg.InstallerInfo.Uninstall.Add(new UninstallInfo(un.SelectSingleNode("FileName").InnerText, un.SelectSingleNode("Date").InnerText));
            }
            if (node_des != null)
              pkg.InstallerInfo.Description = node_des.InnerText;
            XmlNode nodeproperties = nodefile.SelectSingleNode("Properties");
            pkg.InstallerInfo.ProjectProperties.Load(nodeproperties);

            if (!string.IsNullOrEmpty(updateDate))
            {
              try
              {
                pkg.InstallerInfo.ProjectProperties.CreationDate = DateTime.ParseExact(updateDate.Substring(0, 10), "yyyy-MM-dd", null);
              }
              catch (Exception)
              {
              }
            }
            this.Items.Add(pkg);
          }
          //XmlNode nodeoption = ver.SelectSingleNode("Option");
          //this.BuildFileName = nodeoption.SelectSingleNode("BuildFileName").InnerText;
          NormalizeNames();
        }
        catch (Exception)
        {
          MessageBox.Show("List loading error", "Error");
        }
      }
    }

    /// <summary>
    /// Gets the int value from node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="name">The name of node.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    private int GetIntValueFromNode(XmlNode node, string name, int defaultValue)
    {
      try
      {
        int i = defaultValue;
        if (int.TryParse(node.SelectSingleNode(name).InnerText, out i))
        {
          return i;
        }
        else
        {
          return defaultValue;
        }
      }
      catch (Exception)
      {
        return defaultValue;
      }
    }

    private string GetStringValueFromNode(XmlNode node, string name, string defaultValue)
    {
      try
      {
        return node.SelectSingleNode(name).InnerText;
      }
      catch (Exception)
      {
        return defaultValue;
      }
    }
  }
}
