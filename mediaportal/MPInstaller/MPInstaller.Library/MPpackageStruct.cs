using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Globalization;

using CSScriptLibrary;
using ICSharpCode.SharpZipLib.Zip;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal;


namespace MediaPortal.MPInstaller
{
  public class MPpackageStruct
  {
    public string FileName = string.Empty;
    public string txt_EULA = string.Empty;
    public string txt_log = string.Empty;
    public string txt_readme = string.Empty;
    public bool isNew = false;
    public bool isUpdated = false;
    public bool isLocal = false;
    public bool isInstalled = false;
    public bool containsSkin = false;
    public bool containsPlugin = false;
    public IMPIInternalPlugin InstallPlugin;
    public IMPInstallerScript InstallerScript;
    public bool isValid = false;
    public List<string> SkinList;
    public List<string> InstallableSkinList;
    public List<string> InstalledSkinList;

    [DllImport("gdi32")]
    public static extern int AddFontResource(string lpFileName);
    [DllImport("gdi32")]
    public static extern int RemoveFontResource(string lpFileName); 

    public MPpackageStruct()
    {
      txt_EULA = string.Empty;
      txt_log = string.Empty;
      txt_readme = string.Empty;
      containsSkin = false;
      containsPlugin = false;
      isValid = false;
      isNew = false;
      isUpdated = false;
      isLocal = false;
      SkinList = new List<string>();
      InstallableSkinList = new List<string>();
      InstalledSkinList = new List<string>();
      InstallPlugin = null;
      InstallerScript = new MPInstallerScript();
    }

    private MPinstallerStruct installerInfo = new MPinstallerStruct();
    /// <summary>
    /// Gets or sets the installer info.
    /// </summary>
    /// <value>The installer info.</value>
    public MPinstallerStruct InstallerInfo
    {
      get { return installerInfo; }
      set { installerInfo = value; }
    }


    /// <summary>
    /// Gets a value indicating whether this instance is skin package.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is skin package; otherwise, <c>false</c>.
    /// </value>
    public bool IsSkinPackage
    {
      get
      {
        if (InstallerInfo.Group == "Skins")
          return true;
        else
          return false;
      }
    }

    /// <summary>
    /// Get the localy stored filename.
    /// </summary>
    /// <returns></returns>
    public string GetLocalFilename()
    {
      return InstallerInfo.Name + " " + InstallerInfo.Version + ".mpe1";
    }
    /// <summary>
    /// Installs the current package.
    /// </summary>
    /// <param name="pb">ProgressBar for overall progress (can bee null) </param>
    /// <param name="pb1">ProgressBar for current copied file (can bee null)</param>
    /// <param name="lb">Listbox for file listing(can bee null) </param>
    public void InstallPackage(ProgressBar pb, ProgressBar pb1, ListBox lb)
    {
      string fil = FileName;
      byte[] data = new byte[2048];
      int nb = data.Length;
      ZipEntry entry;
      try
      {
        if (File.Exists(fil))
        {
          ZipInputStream s = new ZipInputStream(File.OpenRead(fil));
          while ((entry = s.GetNextEntry()) != null)
          {
            MPIFileList fl = InstallerInfo.FindFileFromZipEntry(entry.Name);
            //MessageBox.Show(entry.Name+fl.FileName);
            if ((fl!=null)&&test_file(fl, entry))
            {
              string tpf;
              if (InstallableSkinList.Contains(fl.SubType) || !fl.SkinType)
              {
                tpf = Path.GetFullPath(MPinstallerStruct.GetDirEntry(fl));
              }
              else
              {
                tpf = Path.GetTempFileName();
              }

             

              if (!Directory.Exists(Path.GetDirectoryName(tpf)))
                Directory.CreateDirectory(Path.GetDirectoryName(tpf));
  
              FileStream fs = new FileStream(tpf, FileMode.Create);
              if (pb != null)
              {
                pb.Minimum = 0;
                pb.Maximum = (int)entry.Size;
                pb.Value = 0;
              }
              while ((nb = s.Read(data, 0, data.Length)) > 0)
              {
                if (pb != null)
                {
                  //MessageBox.Show(String.Format("{0} {1} {2}",pb.Minimum,pb.Value,pb.Maximum));
                  pb.Value += nb;
                  pb.Refresh();
                  pb.Update();
                }
                fs.Write(data, 0, nb);
              }
              fs.Close();
              if (fl.SkinType && fl.FileProperties.DefaultFile)
              {
                foreach (string sd in this.InstallableSkinList)
                  if (!this.SkinList.Contains(sd))
                  {
                    string newtpf = Path.GetFullPath(MPinstallerStruct.GetSkinDirEntry(fl, sd));
                    if (!Directory.Exists(Path.GetDirectoryName(newtpf)))
                      Directory.CreateDirectory(Path.GetDirectoryName(newtpf));
                    File.Copy(tpf, newtpf, true);
                    this.InstallerInfo.Uninstall.Add(new UninstallInfo(newtpf));
                    if (lb != null)
                    {
                      lb.Items.Add(newtpf);
                      lb.SelectedIndex = lb.Items.Count - 1;
                      lb.Update();
                      lb.Refresh();
                    }

                  }
              }

              if (!InstallableSkinList.Contains(fl.SubType) && fl.SkinType)
              {
                File.Delete(tpf);
              }
              else
              {
                if (fl.Type == MPinstallerStruct.SKIN_SYSTEMFONT_TYPE)
                {
                  AddFontResource(tpf);
                }
                this.InstallerInfo.Uninstall.Add(new UninstallInfo(tpf));
                if (lb != null)
                {
                  lb.Items.Add(tpf);
                  lb.Refresh();
                  lb.Update();
                }
              }
            }

            this.InstallerScript.OnInstallFileProcesed(fl);

            if (pb1 != null && pb1.Minimum > pb1.Value)
            {
              pb1.Value++;
              pb1.Refresh();
              pb1.Update();
              pb1.Parent.Refresh();
              pb1.Parent.Update();
            }
          }
          s.Close();
          load();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message+"\n"+ex.StackTrace);
      }
    }

    bool test_file(MPIFileList fl, ZipEntry ze)
    {
      if ((fl.Type != MPinstallerStruct.INTERNAL_TYPE) && (InstallerInfo.FindFileInGroupState(fl)))
      {
        if (fl.SkinType)
        {
          if (ze.Name.Contains(MPinstallerStruct.GetZipEntry(fl)))
          {
            return true;
          }
          else return false;
        }
        else
        {
          if (ze.Name.Contains(MPinstallerStruct.GetZipEntry(fl)))
            return true;
          else return false;
        }
      }
      else
        return false;
    }

    public void installLanguage(ListBox lb)
    {
      MPLanguageHelper mpih = new MPLanguageHelper();
      if (this.InstallerInfo.Language.Count > 0)
      {
        LanguageString firstLs = this.InstallerInfo.Language[0];
        mpih.Load(firstLs.language);
        if (lb != null)
          lb.Items.Add("Adding language :" + firstLs.language);
        foreach (LanguageString ls in this.InstallerInfo.Language)
        {
          if (firstLs.language != ls.language)
          {
            mpih.Language.Sort(new LanguageStringComparer());
            if (lb != null)
              lb.Items.Add("Adding language :" + ls.language);
            mpih.Save();
            mpih.Load(ls.language);
          }
          mpih.Add(ls);
        }
        mpih.Save();
      }
    }

    public void load()
    {
      if (isValid)
      {
        // script suport
        this.InstallerScript = new MPInstallerScript();
        try
        {
          if (!string.IsNullOrEmpty(this.InstallerInfo.Script))
          {
            Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
            AsmHelper script = new AsmHelper(CSScriptLibrary.CSScript.LoadCode(this.InstallerInfo.Script, Path.GetTempFileName(), true));
            //MessageBox.Show(script.CreateObject("InstallScript").ToString());
            this.InstallerScript = (MPInstallerScript)script.CreateObject("InstallScript");
          }
          else
          {
            this.InstallerScript = new MPInstallerScript();
          }
        }
        catch (Exception )
        {
          //MessageBox.Show("Script loading error " + ex.Message + ex.StackTrace);
          this.InstallerScript = new MPInstallerScript();
        }

        this.InstallerScript.CurrentPackage = this;


        byte[] data = new byte[2048];
        int nb = data.Length;
        ZipEntry entry;
        try
        {
          if (File.Exists(FileName))
          {
            ZipInputStream s = new ZipInputStream(File.OpenRead(FileName));
            while ((entry = s.GetNextEntry()) != null)
            {
              MPIFileList fl = InstallerInfo.FindFileFromZipEntry(entry.Name);
              if (fl != null)
              {
                if (fl.Type==MPinstallerStruct.TEXT_TYPE && fl.SubType==MPinstallerStruct.TEXT_EULA_TYPE)
                {
                  txt_EULA = string.Empty;
                  while ((nb = s.Read(data, 0, data.Length)) > 0)
                  {
                    txt_EULA += new ASCIIEncoding().GetString(data, 0, data.Length);
                  }
                }
                if (fl.Type == MPinstallerStruct.TEXT_TYPE && fl.SubType == MPinstallerStruct.TEXT_LOG_TYPE)
                {
                  txt_log = string.Empty;
                  while ((nb = s.Read(data, 0, data.Length)) > 0)
                  {
                    txt_log += new ASCIIEncoding().GetString(data, 0, data.Length);
                  }
                }
                if (fl.Type == MPinstallerStruct.TEXT_TYPE && fl.SubType == MPinstallerStruct.TEXT_README_TYPE)
                {
                  txt_readme = string.Empty;
                  while ((nb = s.Read(data, 0, data.Length)) > 0)
                  {
                    txt_readme += new ASCIIEncoding().GetString(data, 0, data.Length);
                  }
                }
              }
            }
            s.Close();
          }
        }
        catch (Exception)
        {
          
        }
        this.InstallerScript.Init();
      }
    }


    public void LoadFromFile(string fil)
    {
      FileName = fil;
      byte[] data = new byte[2048];
      int nb = data.Length;
      ZipEntry entry;
      try
      {
        if (File.Exists(fil))
        {
          ZipInputStream s = new ZipInputStream(File.OpenRead(fil));
          while ((entry = s.GetNextEntry()) != null)
          {
            if (entry.Name == "instaler.xmp")
            {
              string tpf = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")) + @"\" + "instaler.xmp";
              isValid = true;
              FileStream fs = new FileStream(tpf, FileMode.Create);
              while ((nb = s.Read(data, 0, data.Length)) > 0)
              {
                fs.Write(data, 0, nb);
              }
              fs.Close();
              InstallerInfo.LoadFromFile(tpf);
            }

            if (entry.Name.Contains(MPinstallerStruct.INTERNAL_TYPE + @"\" + MPinstallerStruct.INTERNAL_PLUGIN_SUBTYPE))
            {
              string tpf = Path.GetTempFileName();
              isValid = true;
              FileStream fs = new FileStream(tpf, FileMode.Create);
              while ((nb = s.Read(data, 0, data.Length)) > 0)
              {
                fs.Write(data, 0, nb);
              }
              fs.Close();
              try
              {
                Assembly pluginAssembly = Assembly.LoadFrom(tpf);
                if (pluginAssembly != null)
                {
                  Type[] exportedTypes = pluginAssembly.GetExportedTypes();
                  foreach (Type type in exportedTypes)
                  {
                    if (type.IsAbstract)
                    {
                      continue;
                    }

                    if (type.GetInterface("MediaPortal.MPInstaller.IMPIInternalPlugin", true) != null)
                    {
                      try
                      {
                        //
                        // Create instance of the current type
                        //
                        object pluginObject = Activator.CreateInstance(type);
                        InstallPlugin = pluginObject as IMPIInternalPlugin;
                      }
                      catch (Exception setupFormException)
                      {
                        MessageBox.Show(string.Format("Exception in plugin SetupForm loading : {0} ", setupFormException.Message));
                      }
                    }
                  }
                }
              }
              catch (Exception)
              {
              }
            }
          }
          s.Close();
          load();
        }
      }
      catch (Exception)
      {
        //MessageBox.Show(ex.Message + ex.StackTrace);
        isValid = false;
      }
      if (isValid)
      {
        SkinList.Clear();
        InstallableSkinList.Clear();
        foreach (MPIFileList fl in InstallerInfo.FileList)
        {
          if (fl.Type == MPinstallerStruct.SKIN_TYPE || fl.Type == MPinstallerStruct.SKIN_MEDIA_TYPE)
          {
            if (!SkinList.Contains(fl.SubType))
              SkinList.Add(fl.SubType);
            containsSkin = true;
          }
          if (fl.Type == MPinstallerStruct.PLUGIN_TYPE)
          {
            containsPlugin = true;
          }
        }
        //-----------------
        string SkinDirectory = Config.GetFolder(Config.Dir.Skin);
        if (Directory.Exists(SkinDirectory))
        {
          string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");

          foreach (string skinFolder in skinFolders)
          {
            bool isInvalidDirectory = false;
            string[] invalidDirectoryNames = new string[] { "cvs" };

            string directoryName = skinFolder.Substring(SkinDirectory.Length + 1);

            if (directoryName != null && directoryName.Length > 0)
            {
              foreach (string invalidDirectory in invalidDirectoryNames)
              {
                if (invalidDirectory.Equals(directoryName.ToLower()))
                {
                  isInvalidDirectory = true;
                  break;
                }
              }

              if (isInvalidDirectory == false)
              {
                string filename = Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
                if (File.Exists(filename))
                {
                  InstalledSkinList.Add(directoryName);
                }
              }
            }
          }
        }
        //-----------------
      }
    }
  }

  public class MPLanguageHelper
  {
    public List<LanguageString> Language;
    public string iChars;
    public string iName;
    public string fileName = string.Empty;
    public bool isLoaded = false;
    public bool oldFormat = false;
    private Dictionary<String, String> _availableLanguages;
    Encoding docencoding = null;
    public MPLanguageHelper()
    {
      Language = new List<LanguageString>();
      if (File.Exists(Config.GetFolder(Config.Dir.Language) + @"\strings_en.xml"))
      {
        Load_Names();
      }
      else
      {
        oldFormat = true;
      }
    }

    public void Load_Names()
    {

      _availableLanguages = new Dictionary<string, string>();

      DirectoryInfo dir = new DirectoryInfo(Config.GetFolder(Config.Dir.Language));
      foreach (FileInfo file in dir.GetFiles("strings_*.xml"))
      {
        int pos = file.Name.IndexOf('_') + 1;
        string cultName = file.Name.Substring(pos, file.Name.Length - file.Extension.Length - pos);

        try
        {
          CultureInfo cultInfo = new CultureInfo(cultName);
          _availableLanguages.Add(cultInfo.EnglishName, cultName);
        }
        catch (ArgumentException)
        {
        }

      }
    }
    public void Add(LanguageString ls)
    {
      this.Language.Sort(new LanguageStringComparer());
      int idx = -1;// this.Language.BinarySearch(ls, new LanguageStringComparer());
      for (int i = 0; i < this.Language.Count; i++)
        if (this.Language[i].dwCode.Trim() == ls.dwCode.Trim())
        {
          idx = i;
          break;
        }
      if (idx < 0)
        this.Language.Add(ls);
      else
      {
        this.Language[idx].language = ls.language;
        this.Language[idx].prefix = ls.prefix;
        this.Language[idx].sufix = ls.sufix;
        this.Language[idx].mapSting = ls.mapSting;
      }

    }
    public void Load(string lg)
    {
      if (oldFormat)
      {
        isLoaded = LoadMap_old(Config.GetFile(Config.Dir.Language, lg + @"\strings.xml"));
        fileName = Config.GetFile(Config.Dir.Language, lg + @"\strings.xml");
      }
      else
      {
        if (_availableLanguages.ContainsKey(lg))
        {
          isLoaded = LoadMap(Config.GetFile(Config.Dir.Language, "strings_" + _availableLanguages[lg] + ".xml"));
          fileName = Config.GetFile(Config.Dir.Language, "strings_" + _availableLanguages[lg] + ".xml");
        }
      }
    }
    public void Save()
    {
      if (isLoaded)
        if (oldFormat)
          SaveMap_old(fileName);
        else
          SaveMap(fileName);
    }

    public bool SaveMap_old(string strFileName)
    {
      if (strFileName == null) return false;
      if (strFileName == string.Empty) return false;
      try
      {
        this.Language.Sort(new LanguageStringComparer());
        XmlTextWriter writer = null;
        writer = new XmlTextWriter(strFileName, docencoding);
        writer.Formatting = Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("strings");


        if (!String.IsNullOrEmpty(this.iChars))
          writer.WriteElementString("characters", this.iChars.Trim());


        foreach (LanguageString ls in this.Language)
        {
          writer.WriteStartElement("string");

          if (!String.IsNullOrEmpty(ls.prefix))
            writer.WriteAttributeString("Prefix", ls.prefix);
          if (!String.IsNullOrEmpty(ls.sufix))
            writer.WriteAttributeString("Suffix", ls.sufix);
          writer.WriteElementString("id", ls.dwCode);
          writer.WriteElementString("value", ls.mapSting);
          writer.WriteEndElement();

        }
        writer.WriteEndElement();

        writer.WriteEndDocument();
        writer.Close();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Language writer error : " + ex.Message);
        return false;
      }
      return true;
    }

    public bool SaveMap(string strFileName)
    {
      if (strFileName == null) return false;
      if (strFileName == string.Empty) return false;
      try
      {
        this.Language.Sort(new LanguageStringComparer());
        XmlTextWriter writer = null;
        writer = new XmlTextWriter(strFileName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.IndentChar = ' ';
        writer.WriteStartDocument();
        writer.WriteStartElement("Language");
        if (!String.IsNullOrEmpty(this.iName))
          writer.WriteAttributeString("name", this.iName.Trim());
        if (!String.IsNullOrEmpty(this.iChars))
          writer.WriteAttributeString("characters", this.iChars.Trim());
        writer.WriteStartElement("Section");
        writer.WriteAttributeString("name", "unmapped");
        foreach (LanguageString ls in this.Language)
        {
          writer.WriteStartElement("String");
          writer.WriteAttributeString("id", ls.dwCode);
          if (!String.IsNullOrEmpty(ls.prefix))
            writer.WriteAttributeString("prefix", ls.prefix);
          if (!String.IsNullOrEmpty(ls.sufix))
            writer.WriteAttributeString("suffix", ls.sufix);
          writer.WriteValue(ls.mapSting);
          writer.WriteEndElement();

        }
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Close();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Language writer error : " + ex.Message);
        return false;
      }
      return true;
    }

    public bool LoadMap_old(string strFileName)
    {
      {
        //            bool isPrefixEnabled = true;
        this.iChars = string.Empty;
        this.Language.Clear();
        if (strFileName == null) return false;
        if (strFileName == string.Empty) return false;
        try
        {
          XmlDocument doc = new XmlDocument();
          XmlTextReader reader = new XmlTextReader(strFileName);
          docencoding = reader.Encoding;
          doc.Load(reader);
          if (doc.DocumentElement == null) return false;
          string strRoot = doc.DocumentElement.Name;
          if (strRoot != "strings") return false;
          XmlNode nodeChars = doc.DocumentElement.SelectSingleNode("/strings/characters");

          if (nodeChars != null)
          {
            iChars = nodeChars.InnerText;

          }
          XmlNodeList list = doc.DocumentElement.SelectNodes("/strings/string");
          foreach (XmlNode node in list)
          {
            //StringBuilder builder = new StringBuilder();
            LanguageString ls = new LanguageString();
            ls.dwCode = node.SelectSingleNode("id").InnerText;

            XmlAttribute prefix = node.Attributes["Prefix"];
            if (prefix != null)
              ls.prefix = prefix.Value;
            else ls.prefix = string.Empty;
            ls.mapSting = node.SelectSingleNode("value").InnerText;
            XmlAttribute suffix = node.Attributes["Suffix"];
            if (suffix != null)
              ls.sufix = suffix.Value;
            else
              ls.sufix = string.Empty;
            this.Language.Add(ls);
          }
          reader.Close();
          return true;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Language reader error : " + ex.Message);
          return false;
        }
      }

    }

    public bool LoadMap(string strFileName)
    {
      //            bool isPrefixEnabled = true;
      this.iChars = string.Empty;
      this.Language.Clear();
      if (strFileName == null) return false;
      if (strFileName == string.Empty) return false;
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlTextReader reader = new XmlTextReader(strFileName);
        docencoding = reader.Encoding;
        doc.Load(reader);
        if (doc.DocumentElement == null) return false;
        string strRoot = doc.DocumentElement.Name;
        if (strRoot != "Language") return false;
        //XmlNode nodeChars = doc.DocumentElement.SelectSingleNode("Language");
        XmlNode nodeChars = doc.DocumentElement;
        if (nodeChars != null)
        {
          iChars = nodeChars.Attributes["characters"].Value;
          iName = nodeChars.Attributes["name"].Value;
        }
        XmlNodeList list = doc.DocumentElement.SelectNodes("Section/String");
        foreach (XmlNode node in list)
        {
          LanguageString ls = new LanguageString();
          ls.dwCode = node.Attributes["id"].Value;
          XmlAttribute prefix = node.Attributes["prefix"];
          if (prefix != null)
            ls.prefix = prefix.Value;
          else ls.prefix = string.Empty;
          ls.mapSting = node.InnerText;
          XmlAttribute suffix = node.Attributes["suffix"];
          if (suffix != null)
            ls.sufix = suffix.Value;
          else
            ls.sufix = string.Empty;
          this.Language.Add(ls);
        }
        reader.Close();
        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(strFileName + "\nLanguage reader error : " + ex.Message + ex.Source + ex.StackTrace);
        return false;
      }
    }
  }
}
