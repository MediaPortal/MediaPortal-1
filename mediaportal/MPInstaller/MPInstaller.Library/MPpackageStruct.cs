using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Drawing;
using System.Globalization;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Util;
using MediaPortal;


namespace MediaPortal.MPInstaller
{
    public class MPpackageStruct
    {
        public MPinstalerStruct _intalerStruct= new MPinstalerStruct();
        public string FileName = String.Empty;
        public string txt_EULA = String.Empty;
        public string txt_log = String.Empty;
        public string txt_readme = String.Empty;
        public bool isNew = false;
        public bool isUpdated = false;
        public bool isLocal = false;
        public bool isInstalled = false;
        public bool containsSkin = false;
        public bool isValid = false;
        public List<string> SkinList;
        public List<string> InstallableSkinList;
        public MPpackageStruct()
        {
            txt_EULA = String.Empty;
            txt_log = String.Empty;
            txt_readme = String.Empty;
            containsSkin = false;
            isValid = false;
            isNew = false;
            isUpdated = false;
            isLocal = false;
            SkinList = new List<string>();
            InstallableSkinList = new List<string>();

        }
        
        public void instal_file(ProgressBar pb,ListBox lb, MPIFileList fl)
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
                        //MessageBox.Show(entry.Name);
                        if (test_file(fl,entry))
                        { 
                            string tpf =Path.GetFullPath(MPinstalerStruct.GetDirEntry(fl)) ;
                            if (fl.Type == MPinstalerStruct.SKIN_TYPE || fl.Type == MPinstalerStruct.SKIN_MEDIA_TYPE || fl.Type == MPinstalerStruct.SKIN_SOUNDS_TYPE || fl.Type == MPinstalerStruct.SKIN_ANIMATIONS_TYPE || fl.Type == MPinstalerStruct.SKIN_TETRIS_TYPE)
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(tpf)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(tpf));
                            }
                            //MessageBox.Show(tpf);
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
                            this._intalerStruct.Uninstall.Add(new UninstallInfo(tpf));
                            if (lb != null)
                            {
                                lb.Items.Add(tpf);
                                lb.Refresh();
                                lb.Update();
                            }
                        }
                    }
                    s.Close();
                    load();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool test_file(MPIFileList fl, ZipEntry ze)
        {
          if (_intalerStruct.FindFileInGroupState(fl))
          {
            if (fl.Type == MPinstalerStruct.SKIN_TYPE || fl.Type == MPinstalerStruct.SKIN_MEDIA_TYPE)
            {
              if (Path.GetFileName(ze.Name) == Path.GetFileName(fl.FileName)
                  && ze.Name.Contains(@"\" + fl.SubType + @"\") && InstallableSkinList.Contains(fl.SubType))
              {
                return true;
              }
              else return false;
            }
            else
            {
              if (Path.GetFileName(ze.Name) == Path.GetFileName(fl.FileName))
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
            if (this._intalerStruct.Language.Count > 0)
            {
                LanguageString firstLs = this._intalerStruct.Language[0];
                mpih.Load(firstLs.language);
                if (lb != null)
                    lb.Items.Add("Adding language :" + firstLs.language);
                foreach (LanguageString ls in this._intalerStruct.Language)
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
                            if (Path.GetFileName(entry.Name) == Path.GetFileName(_intalerStruct.FindList(MPinstalerStruct.TEXT_TYPE,MPinstalerStruct.TEXT_EULA_TYPE).FileName))
                            {
                                txt_EULA = String.Empty;
                                while ((nb = s.Read(data, 0, data.Length)) > 0)
                                {
                                     txt_EULA += new ASCIIEncoding().GetString(data, 0, data.Length);
                                }
                            }
                            if (Path.GetFileName(entry.Name) == Path.GetFileName(_intalerStruct.FindList(MPinstalerStruct.TEXT_TYPE, MPinstalerStruct.TEXT_LOG_TYPE).FileName))
                            {
                                txt_log = String.Empty;
                                while ((nb = s.Read(data, 0, data.Length)) > 0)
                                {
                                    txt_log += new ASCIIEncoding().GetString(data, 0, data.Length);
                                }
                            }
                            if (Path.GetFileName(entry.Name) == Path.GetFileName(_intalerStruct.FindList(MPinstalerStruct.TEXT_TYPE, MPinstalerStruct.TEXT_README_TYPE).FileName))
                            {
                                txt_readme = String.Empty;
                                while ((nb = s.Read(data, 0, data.Length)) > 0)
                                {
                                    txt_readme += new ASCIIEncoding().GetString(data, 0, data.Length);
                                }
                            }

                        }
                        s.Close();
                    }
                }
                catch (Exception)
                {
                }
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
                            string tpf =Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP"))+@"\" +"instaler.xmp" ;
                            isValid = true;
                            FileStream fs = new FileStream(tpf, FileMode.Create);
                            while ((nb = s.Read(data, 0, data.Length)) > 0)
                            {
                                fs.Write(data, 0, nb);
                            }
                            fs.Close();
                            _intalerStruct.LoadFromFile(tpf);
                        }
                    }
                    s.Close();
                    load();
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message+ex.StackTrace);
                isValid = false;
            }
            if (isValid)
            {
                SkinList.Clear();
                InstallableSkinList.Clear();
                foreach (MPIFileList fl in _intalerStruct.FileList)
                {
                    if (fl.Type == MPinstalerStruct.SKIN_TYPE || fl.Type == MPinstalerStruct.SKIN_MEDIA_TYPE)
                    {
                        if (!SkinList.Contains(fl.SubType))
                            SkinList.Add(fl.SubType);
                        containsSkin = true;
                    }
                }
            }
        }
    }

    public class MPInstallHelper
    {
        public ArrayList lst = new ArrayList();
        string InstalDir = Config.GetFolder(Config.Dir.Base) + @"\" + "Installer";
        public string FileName = "";
        public MPInstallHelper()
        {
            FileName = InstalDir + @"\" + "config.xml";
            //LoadFromFile();
        }
        
        public void Compare(MPInstallHelper mp)
        {
            foreach (MPpackageStruct pk in this.lst)
            {
                pk.isNew = true;
            }
            foreach (MPpackageStruct pk in mp.lst)
            {
                int idx=this.IndexOf(pk);
                if (idx > -1)
                {
                    if (((MPpackageStruct)this.lst[idx])._intalerStruct.Version.CompareTo(pk._intalerStruct.Version)>0)
                        ((MPpackageStruct)this.lst[idx]).isUpdated = true;
                    ((MPpackageStruct)this.lst[idx]).isNew = false;

                }
            }
        }
        
        public void Add(MPpackageStruct pk)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (((MPpackageStruct)lst[i])._intalerStruct.Name.Trim() == pk._intalerStruct.Name.Trim())
                    lst.RemoveAt(i);
            }
            lst.Add(pk);
            if (!Directory.Exists(InstalDir))
            {
                Directory.CreateDirectory(InstalDir);
            }
            if (Path.GetFullPath(pk.FileName) != Path.GetFullPath(InstalDir + @"\" + Path.GetFileName(pk.FileName)))
              if (File.Exists(Path.GetFullPath(pk.FileName)))
                File.Copy(pk.FileName, InstalDir + @"\" + Path.GetFileName(pk.FileName), true);
        }
      
        public void AddRange(MPInstallHelper mpih)
        {
          foreach (MPpackageStruct pk in mpih.lst)
          {
            this.Add(pk);
          }
        }

        public int IndexOf(MPpackageStruct pk)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (((MPpackageStruct)lst[i])._intalerStruct.Name.Trim() == pk._intalerStruct.Name.Trim())
                    return i;
            }
            return -1;
        }

        public MPpackageStruct Find(string name)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (((MPpackageStruct)lst[i])._intalerStruct.Name.Trim() ==name.Trim())
                    return (MPpackageStruct)lst[i];
            }
            return null;
        }
        public void SaveToFile()
        {
            if (!Directory.Exists(InstalDir))
            {
                Directory.CreateDirectory(InstalDir);
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
                    for (int i = 0; i < this.lst.Count; i++)
                    {
                        MPpackageStruct it = (MPpackageStruct)this.lst[i];
                        writer.WriteStartElement("Extension");
                        writer.WriteElementString("FileName", Path.GetFileName(it.FileName));
                        writer.WriteElementString("Name", it._intalerStruct.Name);
                        writer.WriteElementString("URL", it._intalerStruct.UpdateURL);
                        writer.WriteElementString("Version", it._intalerStruct.Version);
                        writer.WriteElementString("Author", it._intalerStruct.Author);
                        writer.WriteElementString("Description", it._intalerStruct.Description);
                        writer.WriteElementString("Group", it._intalerStruct.Group);
                        it._intalerStruct.WriteLogoElement(writer);
                        writer.WriteStartElement("Properties");
                        it._intalerStruct.ProiectProperties.Save(writer);
                        writer.WriteEndElement();
                        writer.WriteStartElement("Uninstall");
                        for (int j = 0; j < it._intalerStruct.Uninstall.Count; j++)
                        {
                            writer.WriteStartElement("FileInfo");
                            writer.WriteElementString("FileName", ((UninstallInfo)it._intalerStruct.Uninstall[j]).Path);
                            writer.WriteElementString("Date", Path.GetFileName(((UninstallInfo)it._intalerStruct.Uninstall[j]).Date.ToFileTime().ToString()));
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
                    client.DownloadFile(url,fil );
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

        public void LoadFromFile()
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(FileName))
            {
                doc.Load(FileName);
                lst.Clear();
                XmlNode ver = doc.DocumentElement.SelectSingleNode("/MPinstalerS");
                XmlNodeList fileList = ver.SelectNodes("ExtensionList/Extension");
                foreach (XmlNode nodefile in fileList)
                {
                    MPpackageStruct pkg = new MPpackageStruct();
                    pkg.FileName = nodefile.SelectSingleNode("FileName").InnerText;
                    pkg._intalerStruct.Name = nodefile.SelectSingleNode("Name").InnerText;
                    pkg._intalerStruct.Author = nodefile.SelectSingleNode("Author").InnerText;
                    pkg._intalerStruct.Version = nodefile.SelectSingleNode("Version").InnerText;
                    pkg._intalerStruct.UpdateURL = nodefile.SelectSingleNode("URL").InnerText;
                    XmlNode grup_node = nodefile.SelectSingleNode("Group");
                    if (grup_node != null)
                        pkg._intalerStruct.Group = grup_node.InnerText;
                    XmlNode node_logo = nodefile.SelectSingleNode("Logo");
                    if (node_logo != null)
                    {
                        byte[] buffer = Convert.FromBase64String(node_logo.InnerText);
                        string t = Path.GetTempFileName();
                        FileStream fs = new FileStream(t, FileMode.Create);
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Close();
                        pkg._intalerStruct.Logo = Image.FromFile(t, true);
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
                        pkg._intalerStruct.Uninstall.Add(new UninstallInfo(un.SelectSingleNode("FileName").InnerText, un.SelectSingleNode("Date").InnerText));
                    }
                    if (node_des != null)
                        pkg._intalerStruct.Description = node_des.InnerText;
                    XmlNode nodeproperties = nodefile.SelectSingleNode("Properties");
                    pkg._intalerStruct.ProiectProperties.Load(nodeproperties);

                    this.lst.Add(pkg);
                }
                //XmlNode nodeoption = ver.SelectSingleNode("Option");
                //this.BuildFileName = nodeoption.SelectSingleNode("BuildFileName").InnerText;
            }
        }
 
    }

    public class MPLanguageHelper
    {
        public List<LanguageString> Language;
        public string iChars;
        public string iName; 
        public string fileName=string.Empty;
        public bool isLoaded=false;
        public bool oldFormat = false;
        private Dictionary<String, String> _availableLanguages;
        Encoding docencoding = null ;
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
            if (strFileName == String.Empty) return false;
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
            if (strFileName == String.Empty) return false;
            try
            {
                this.Language.Sort(new LanguageStringComparer());
                XmlTextWriter writer = null;
                writer = new XmlTextWriter(strFileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.IndentChar=' ';
                writer.WriteStartDocument();
                writer.WriteStartElement("Language");
                if (!String.IsNullOrEmpty(this.iName))
                    writer.WriteAttributeString("name", this.iName.Trim());
                if(!String.IsNullOrEmpty(this.iChars))
                    writer.WriteAttributeString("characters", this.iChars.Trim());
                writer.WriteStartElement("Section");
                writer.WriteAttributeString("name", "unmapped");
                foreach (LanguageString ls in this.Language)
                {
                    writer.WriteStartElement("String");
                    writer.WriteAttributeString("id", ls.dwCode);
                    if (!String.IsNullOrEmpty(ls.prefix))
                        writer.WriteAttributeString("prefix",ls.prefix);
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
                MessageBox.Show("Language writer error : "+ex.Message);
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
                if (strFileName == String.Empty) return false;
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
                        else ls.prefix = String.Empty;
                        ls.mapSting = node.SelectSingleNode("value").InnerText;
                        XmlAttribute suffix = node.Attributes["Suffix"];
                        if (suffix != null)
                            ls.sufix = suffix.Value;
                        else
                            ls.sufix = String.Empty;
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
            if (strFileName == String.Empty) return false;
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
                    else ls.prefix = String.Empty;
                    ls.mapSting=node.InnerText;
                    XmlAttribute suffix = node.Attributes["suffix"];
                    if (suffix != null)
                        ls.sufix = suffix.Value;
                    else
                        ls.sufix = String.Empty;
                    this.Language.Add(ls);
                }
                reader.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(strFileName+"\nLanguage reader error : " + ex.Message+ ex.Source+ex.StackTrace);
                return false;
            }
        }
    }
}
