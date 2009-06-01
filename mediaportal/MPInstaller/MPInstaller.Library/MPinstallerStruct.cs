#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Ionic.Zip;
using MediaPortal.Configuration;
using MediaPortal.Util;
using MediaPortal.MPInstaller;


namespace MediaPortal.MPInstaller
{
  /// <summary>
  ///  Base mpi entity class informations stored in instaler.xmp
  /// </summary>
  public class MPinstallerStruct
  {
    public const string DEFAULT_UPDATE_SITE = "http://www.team-mediaportal.com";
    // for testing only 
    //public const string DEFAULT_UPDATE_SITE = "http://testbase001.team-mediaportal.com";

    public static string[] CategoryListing ={ "Audio/Radio", "Automation", "EPG/TV", "Games", "Input", "Others", "PIM", "Skins", "Utilities", "Video/Movies", "Web", "TV Logos" };


    public const string PLUGIN_TYPE = "Plugin";
    public const string PLUGIN_WINDOW_SUBTYPE = "Window";
    public const string PLUGIN_PROCESS_SUBTYPE = "Process";
    public const string PLUGIN_SUBTITLE_SUBTYPE = "Subtitle";
    public const string PLUGIN_TAGREADER_SUBTYPE = "TagReader";
    public const string PLUGIN_PLAYER_SUBTYPE = "External Player";
    public const string SKIN_TYPE = "Skin";
    public const string SKIN_MEDIA_TYPE = "Media";
    public const string SKIN_SOUNDS_TYPE = "Sounds";
    public const string SKIN_ANIMATIONS_TYPE = "Animations";
    public const string SKIN_TETRIS_TYPE = "Tetris";
    public const string SKIN_XML_SUBTYPE = "Genskin";
    public const string SKIN_SYSTEMFONT_TYPE = "System_Font";
    public const string TEXT_TYPE = "Text";
    public const string TEXT_EULA_TYPE = "EULA";
    public const string TEXT_LOG_TYPE = "Log";
    public const string TEXT_README_TYPE = "Ream_me";
    public const string THUMBS_TYPE = "Thumbs";
    public const string OTHER_TYPE = "Other";
    public const string INTERNAL_TYPE = "Internal";
    public const string INTERNAL_PLUGIN_SUBTYPE = "Plugin";

    string _builFileName = string.Empty;
    string _proiectFileName = string.Empty;
    string _author = string.Empty;
    string _update = DEFAULT_UPDATE_SITE;
    string _name = string.Empty;
    string _version = string.Empty;
    string _description = string.Empty;
    string _group = string.Empty;
    string _release = string.Empty;
    private string script = string.Empty;

    Image _image = null;
    public ArrayList Uninstall = new ArrayList();
    public ArrayList FileList = new ArrayList();
    public List<LanguageString> Language;
    public List<ActionInfo> Actions;
    public List<GroupString> SetupGroups;
    public List<GroupStringMapping> SetupGroupsMappig;
    
    public MPinstallerStruct()
    {
      Language = new List<LanguageString>();
      Actions = new List<ActionInfo>();
      ProjectProperties = new ProjectPropertiesClass();
      SetupGroups = new List<GroupString>();
      SetupGroupsMappig = new List<GroupStringMapping>();
      
    }

    private ProjectPropertiesClass projectPropertise;

    public ProjectPropertiesClass ProjectProperties
    {
      get { return projectPropertise; }
      set { projectPropertise = value; }
    }


    public string UpdateURL
    {
      get { return _update; }
      set
      {
        if (String.IsNullOrEmpty(value)) _update = DEFAULT_UPDATE_SITE; else _update = value.Trim();
      }
    }

    public string Script
    {
      get { return script; }
      set { script = value; }
    }


    public string Version
    {
      get { return _version; }
      set
      {
        if (value == null) _version = ""; else _version = value;
      }
    }

    public string Author
    {
      get { return _author; }
      set { _author = value; }
    }

    public string Release
    {
      get { return _release; }
      set { _release = value; }
    }

    public Image Logo
    {
      get { return _image; }
      set { _image = value; }
    }

    public string Group
    {
      get { return _group; }
      set { _group = value; }
    }
    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
    public string BuildFileName
    {
      get { return _builFileName; }
      set { _builFileName = value; }
    }
    public string ProiectFileName
    {
      get { return _proiectFileName; }
      set { _proiectFileName = value; }
    }

    public void AddFileList(ListView lst)
    {
      FileList.Clear();
      for (int i = 0; i < lst.Items.Count; i++)
      {
        FileList.Add(new MPIFileList(lst.Items[i].SubItems[3].Text, lst.Items[i].SubItems[1].Text, lst.Items[i].SubItems[2].Text, lst.Items[i].SubItems[4].Text, lst.Items[i].SubItems[5].Text));
      }
    }

    /// <summary>
    /// Finds a action.
    /// 
    /// </summary>
    /// <param name="p">Name of action to find ex.: "POSTSETUP"</param>
    /// <returns></returns>
    public ActionInfo FindAction(string p)
    {
      int idx = -1;
      for (int i = 0; i < Actions.Count; i++)
      {
        if (Actions[i].Place == p)
        {
          idx = i;
          break;
        }
      }
      if (idx > -1)
        return Actions[idx];
      else return null;
    }

    /// <summary>
    /// Adds a new action action.
    /// </summary>
    /// <param name="a"> Action descriptor </param>
    public void AddAction(ActionInfo a)
    {
      int idx = -1;
      for (int i = 0; i < Actions.Count; i++)
      {
        if (Actions[i].Equals(a))
        {
          idx = i;
          break;
        }
      }
      if (idx > -1)
      {
        Actions[idx].Place = a.Place;
        Actions[idx].Id = a.Id;
        Actions[idx].Command = a.Command;
      }
      else
      {
        Actions.Add(a);
      }
    }

    public void AddSetupGroupMapping(GroupStringMapping a)
    {
      int idx = -1;
      for (int i = 0; i < SetupGroupsMappig.Count; i++)
      {
        if (SetupGroupsMappig[i].Id == a.Id && SetupGroupsMappig[i].FileName == a.FileName)
        {
          idx = i;
          break;
        }
      }
      if (idx > -1)
      {
        SetupGroupsMappig[idx].Id = a.Id;
        SetupGroupsMappig[idx].FileName = a.FileName;
      }
      else
      {
        SetupGroupsMappig.Add(a);
      }
    }

    /// <summary>
    /// Finds the state of the file in group.
    /// </summary>
    /// <param name="fl">The fl.</param>
    /// <returns></returns>
    public bool FindFileInGroupState(MPIFileList fl)
    {
      if (SetupGroups.Count < 1)
        return true;
      for (int i = 0; i < SetupGroups.Count; i++)
      {
        if (FindFileInGroup(SetupGroups[i].Id, fl.FileName) && SetupGroups[i].Checked)
          return true;
      }
      return false;
    }

    public void Clear()
    {
      FileList.Clear();
      Language.Clear();
      Actions.Clear();
      SetupGroups.Clear();
      SetupGroupsMappig.Clear();
      ProjectProperties.Clear();
      BuildFileName = string.Empty;
      ProiectFileName = string.Empty;
      Author = string.Empty;
      Name = string.Empty;
      Version = string.Empty;
      UpdateURL = string.Empty;
      Group = string.Empty;
      Release = string.Empty;
      Script = string.Empty;
      Logo = null;
    }

    public bool SaveToFile(string fil)
    {
      Stream myStream;
      if ((myStream = File.Open(fil, FileMode.Create, FileAccess.Write, FileShare.None)) != null)
      {
        this.ProiectFileName = fil;
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
          writer.WriteStartElement("MPinstaler");
          writer.WriteElementString("ver", "1.00.000");
          writer.WriteStartElement("FileList");
          for (int i = 0; i < this.FileList.Count; i++)
          {
            MPIFileList it = (MPIFileList)this.FileList[i];

            if (projectPropertise.UseRealPathInPackage)
            {
              it.GUID = string.Empty;
            }
            else
            {
              it.SetGuid();
            }

            writer.WriteStartElement("File");
            writer.WriteElementString("FileName", Path.GetFileName(it.FileName));
            writer.WriteElementString("Type", it.Type);
            writer.WriteElementString("SubType", it.SubType);
            writer.WriteElementString("Source", RelativePath(fil,it.FileName));
            writer.WriteElementString("Id", it.ID);
            writer.WriteElementString("Option", it.Option);
            writer.WriteElementString("Guid",it.GUID );
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.WriteStartElement("StringList");
          foreach (LanguageString lg in Language)
          {
            writer.WriteStartElement("string");
            writer.WriteElementString("language", lg.language);
            writer.WriteElementString("id", lg.dwCode);
            writer.WriteElementString("value", lg.mapSting);
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.WriteStartElement("Actions");
          foreach (ActionInfo ai in Actions)
          {
            writer.WriteStartElement("Action");
            writer.WriteAttributeString("Place", ai.Place);
            writer.WriteAttributeString("Id", ai.Id.ToString());
            writer.WriteAttributeString("Command", ai.Command);
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.WriteStartElement("SetupGroups");
          foreach (GroupString gs in SetupGroups)
          {
            writer.WriteStartElement("SetupGroup");
            writer.WriteAttributeString("Id", gs.Id);
            writer.WriteAttributeString("Name", gs.Name);
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.WriteStartElement("SetupGroupMappings");
          foreach (GroupStringMapping gs in SetupGroupsMappig)
          {
            writer.WriteStartElement("SetupGroupMapping");
            writer.WriteAttributeString("Id", gs.Id);
            writer.WriteAttributeString("FileName", RelativePath(fil, gs.FileName));
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.WriteStartElement("Option");
          writer.WriteElementString("BuildFileName", this.BuildFileName);
          writer.WriteElementString("ProiectFileName", Path.GetFullPath(this.ProiectFileName));
          writer.WriteElementString("ProiectName", this.Name);
          writer.WriteElementString("Author", this.Author);
          writer.WriteElementString("UpdateURL", this.UpdateURL);
          writer.WriteElementString("Version", this.Version);
          writer.WriteElementString("Description", this.Description);
          writer.WriteElementString("Group", this.Group);
          writer.WriteElementString("Release", this.Release);
          writer.WriteElementString("Script", this.Script);
          WriteLogoElement(writer);
          writer.WriteEndElement();
          writer.WriteStartElement("Properties");
          ProjectProperties.Save(writer);
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.Flush();

          //-------------------------------------
          //-------------------------------------
        }
        finally
        {
          if (writer != null)
            writer.Close();
        }
        myStream.Close();

      }
      return true;
    }

    /// <summary>
    /// Writes the logo element to xml .
    /// </summary>
    /// <param name="writer">The writer .</param>
    public void WriteLogoElement(XmlWriter writer)
    {
      if (this.Logo != null)
      {
        string t = Path.GetTempFileName();
        this.Logo.Save(t, System.Drawing.Imaging.ImageFormat.Png);
        FileStream fs = File.OpenRead(Path.GetFullPath(t));
        byte[] buffer = new byte[fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        fs.Close();
        File.Delete(t);
        writer.WriteStartElement("Logo");
        writer.WriteBase64(buffer, 0, buffer.Length);
        writer.WriteEndElement();
      }
    }

    public void BuildFile(ListBox ls, ProgressBar pb)
    {
      try
      {
        using (ZipFile zip = new ZipFile())
        {
          StreamReader sr;

          zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BEST_COMPRESSION;
          ls.Items.Clear();
          pb.Value = 0;
          pb.Maximum = FileList.Count;
          ls.Items.Add("Build file :" + _builFileName);

          if (File.Exists(ProiectFileName))
          {
            sr = new StreamReader(Path.GetFullPath(ProiectFileName));
            zip.AddFileStream("instaler.xmp", "", sr.BaseStream);
          }
          else ls.Items.Add("Error : Proiect file not found !");

          foreach (MPIFileList file in FileList)
          {
            ls.Items.Add("Adding file :" + file.FileName);
            pb.Value++;
            ls.Refresh();
            ls.Update();
            if (!string.IsNullOrEmpty(file.FileName) && File.Exists(file.FileName))
            {
              string filename = Path.GetFullPath(file.FileName);
              ZipEntry entry = zip.AddFile(filename);
              entry.FileName = GetZipEntry(file);
              entry.LastModified = File.GetLastWriteTimeUtc(filename);
              ls.Items.Add("Added file :" + GetZipEntry(file));
            }
            else ls.Items.Add("Error : File not found !");
          }
          zip.Save(_builFileName);
        }
      }
      catch (System.Exception ex1)
      {
        ls.Items.Add("exception: " + ex1); // Probably due to file access error
      }
    }
    
    string RelativePath(string refpath, string file)
    {
      return Path.GetFullPath(file).Replace(Path.GetDirectoryName(refpath) + @"\", "");
    }

    string AbsolutePath(string refpath, string file)
    {
      if (!Path.IsPathRooted(file))
      {
        return Path.Combine(Path.GetDirectoryName(refpath), file);
      }
      return file;
    }

    public void LoadFromFile(string fil)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(fil);
      FileList.Clear();
      Language.Clear();
      ProjectProperties.Clear();
      XmlNode ver = doc.DocumentElement.SelectSingleNode("/MPinstaler");
      XmlNodeList fileList = ver.SelectNodes("FileList/File");
      foreach (XmlNode nodefile in fileList)
      {
        string t_path = nodefile.SelectSingleNode("Source").InnerText;
        XmlNode node_guid = nodefile.SelectSingleNode("Guid");
        string str_guid = string.Empty;
        if (node_guid != null)
          str_guid = nodefile.SelectSingleNode("Guid").InnerText;
        this.FileList.Add(new MPIFileList(
               AbsolutePath(fil,t_path),
               nodefile.SelectSingleNode("Type").InnerText,
               nodefile.SelectSingleNode("SubType").InnerText,
               nodefile.SelectSingleNode("Id").InnerText,
               nodefile.SelectSingleNode("Option").InnerText,
               str_guid));
      }
      XmlNodeList langList = ver.SelectNodes("StringList/string");
      foreach (XmlNode langnode in langList)
      {
        Language.Add(new LanguageString(langnode.SelectSingleNode("language").InnerText,
                                        langnode.SelectSingleNode("id").InnerText,
                                        langnode.SelectSingleNode("value").InnerText));
      }
      XmlNodeList actionList = ver.SelectNodes("Actions/Action");
      foreach (XmlNode actionnode in actionList)
      {
        Actions.Add(new ActionInfo(actionnode.Attributes["Place"].Value,
                                   Convert.ToInt32(actionnode.Attributes["Id"].Value),
                                   actionnode.Attributes["Command"].Value));
      }
      XmlNodeList groupList = ver.SelectNodes("SetupGroups/SetupGroup");
      foreach (XmlNode groupnode in groupList)
      {
        SetupGroups.Add(new GroupString(groupnode.Attributes["Id"].Value,
                                        groupnode.Attributes["Name"].Value));
      }
      XmlNodeList groupmapList = ver.SelectNodes("SetupGroupMappings/SetupGroupMapping");
      foreach (XmlNode groupnode in groupmapList)
      {
        SetupGroupsMappig.Add(new GroupStringMapping(groupnode.Attributes["Id"].Value,
                                        AbsolutePath(fil,groupnode.Attributes["FileName"].Value)));
      }
      XmlNode nodeoption = ver.SelectSingleNode("Option");
      this.BuildFileName = nodeoption.SelectSingleNode("BuildFileName").InnerText;
      this.Name = nodeoption.SelectSingleNode("ProiectName").InnerText;
      this.Author = nodeoption.SelectSingleNode("Author").InnerText;
      this.UpdateURL = nodeoption.SelectSingleNode("UpdateURL").InnerText;
      this.Version = nodeoption.SelectSingleNode("Version").InnerText;
      XmlNode node_des = nodeoption.SelectSingleNode("Description");
      if (node_des != null)
        this._description = node_des.InnerText;
      XmlNode node_release = nodeoption.SelectSingleNode("Release");
      if (node_release != null)
        this.Release = node_release.InnerText;
      XmlNode node_scr = nodeoption.SelectSingleNode("Script");
      if (node_scr != null)
        this.Script = node_scr.InnerText;
      XmlNode node_gr = nodeoption.SelectSingleNode("Group");
      if (node_gr != null)
        this.Group = node_gr.InnerText;
      XmlNode node_logo = nodeoption.SelectSingleNode("Logo");
      if (node_logo != null)
      {
        byte[] buffer = Convert.FromBase64String(node_logo.InnerText);
        string t = Path.GetTempFileName();
        FileStream fs = new FileStream(t, FileMode.Create);
        fs.Write(buffer, 0, buffer.Length);
        fs.Close();
        this.Logo = Image.FromFile(t, true);
        try
        {
          File.Delete(t);
        }
        catch (Exception)
        {
          //MessageBox.Show(ex.Message + "\n" + ex.StackTrace); // Probably file access error
        }
      }
      XmlNode nodeproperties = ver.SelectSingleNode("Properties");
      ProjectProperties.Load(nodeproperties);
    }

    public MPIFileList FindList(string typ, string stpy)
    {
      MPIFileList fs = new MPIFileList();
      for (int i = 0; i < FileList.Count; i++)
      {
        if ((((MPIFileList)FileList[i]).Type == typ) && (((MPIFileList)FileList[i]).SubType == stpy))
        {
          fs = (MPIFileList)FileList[i];
        }
      }
      return fs;
    }

    /// <summary>
    /// Finds the file from zip entry.
    /// </summary>
    /// <param name="file">The zip entry name</param>
    /// <returns></returns>
    /// 
    public MPIFileList FindFileFromZipEntry(string file)
    {
      MPIFileList fs = null;
      for (int i = 0; i < FileList.Count; i++)
      {
        if ((GetZipEntry((MPIFileList)FileList[i]) == file))
        {
          fs = (MPIFileList)FileList[i];
          break;
        }
      }
      return fs;
    }


    /// <summary>
    /// Finds the file in list.
    /// </summary>
    /// <param name="file">The file name</param>
    /// <returns></returns>
    public MPIFileList FindFile(string file)
    {
      MPIFileList fs = new MPIFileList();
      for (int i = 0; i < FileList.Count; i++)
      {
        if ((((MPIFileList)FileList[i]).FileNameShort == file))
        {
          fs = (MPIFileList)FileList[i];
          break;
        }
      }
      return fs;
    }

    public bool FindFileInGroup(string group, string file)
    {

      for (int i = 0; i < SetupGroupsMappig.Count; i++)
      {
        if (SetupGroupsMappig[i].FileName == file && SetupGroupsMappig[i].Id == group)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Generate a mpi zip entry 
    /// </summary>
    /// <param name="flst">The file </param>
    /// <returns></returns>
    public static string GetZipEntry(MPIFileList flst)
    {
      string ret = string.Empty;
      if (flst.Type == PLUGIN_TYPE)
      {
        ret = "Release" + @"\" + "Plugins" + @"\";
        switch (flst.SubType)
        {
          case PLUGIN_WINDOW_SUBTYPE:
            ret += "Windows";
            break;
          case PLUGIN_PLAYER_SUBTYPE:
            ret += "ExternalPlayers";
            break;
          case PLUGIN_PROCESS_SUBTYPE:
            ret += "Process";
            break;
          case PLUGIN_SUBTITLE_SUBTYPE:
            ret += "Subtitle";
            break;
          case PLUGIN_TAGREADER_SUBTYPE:
            ret += "TagReaders";
            break;

        }
        ret += @"\" ;
      }

      if (flst.Type == SKIN_TYPE)
      {
        ret = "Release" + @"\" + "Skin" + @"\" + flst.SubType + @"\" ;
      }
      if (flst.Type == SKIN_MEDIA_TYPE)
      {
        ret = "Release" + @"\" + "Skin" + @"\" + flst.SubType + @"\" + "Media" + @"\" ;
      }

      if (flst.Type == SKIN_SOUNDS_TYPE)
      {
        ret = "Release" + @"\" + "Skin" + @"\" + flst.SubType + @"\" + "Sounds" + @"\" ;
      }

      if (flst.Type == SKIN_ANIMATIONS_TYPE)
      {
        ret = "Release" + @"\" + "Skin" + @"\" + flst.SubType + @"\" + "Media" + @"\" + "Animations" + @"\" ;
      }

      if (flst.Type == SKIN_TETRIS_TYPE)
      {
        ret = "Release" + @"\" + "Skin" + @"\" + flst.SubType + @"\" + "Media" + @"\" + "Tetris" + @"\";
      }

      if (flst.Type == TEXT_TYPE)
      {
        ret = "Release" + @"\" + "Text" + @"\" + flst.SubType + @"\" ;
      }

      if (flst.Type == THUMBS_TYPE)
      {
        ret = "Release" + @"\" + "Thumbs" + @"\" + flst.SubType + @"\" ;
      }
      if (flst.Type == OTHER_TYPE)
      {
        ret = "Release" + @"\" + "Other" + @"\" ;
      }

      if (flst.Type == INTERNAL_TYPE)
      {
        ret = "Internal" + @"\" + flst.SubType + @"\";
      }
      
      if (flst.Type == SKIN_SYSTEMFONT_TYPE)
      {
        ret = "System_Font" + @"\" + flst.SubType + @"\";
      }

      if (string.IsNullOrEmpty(flst.FileProperties.OutputFileName))
        ret += Path.GetFileName(flst.FileName);
      else
        ret += flst.FileProperties.OutputFileName;
      if (!string.IsNullOrEmpty(flst.GUID))
        ret = flst.GUID;
      return ret;
    }

    /// <summary>
    /// Gets the skin dir entry.
    /// Build a path for a new skin 
    /// </summary>
    /// <param name="flst">File info </param>
    /// <param name="skindDir">The skin name</param>
    /// <returns></returns>
    public static string GetSkinDirEntry(MPIFileList flst, string skindDir)
    {
      string ret = string.Empty;

      if (flst.Type == SKIN_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + skindDir + @"\";
      }

      if (flst.Type == SKIN_MEDIA_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + skindDir + @"\" + "Media" + @"\";
      }

      if (flst.Type == SKIN_SOUNDS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + skindDir + @"\" + "Sounds" + @"\";
      }

      if (flst.Type == SKIN_ANIMATIONS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + skindDir + @"\" + "Media" + @"\" + "Animations" + @"\";
      }
      if (flst.Type == SKIN_TETRIS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + skindDir + @"\" + "Media" + @"\" + "Tetris" + @"\";
      }
      if (string.IsNullOrEmpty(flst.FileProperties.OutputFileName))
        ret += Path.GetFileName(flst.FileName);
      else
        ret += flst.FileProperties.OutputFileName;
      return ret;
    }

    public static string GetDirEntry(MPIFileList flst)
    {
      string ret = string.Empty;
      if (flst.Type == PLUGIN_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Plugins) + @"\";
        switch (flst.SubType)
        {
          case PLUGIN_WINDOW_SUBTYPE:
            ret += "Windows";
            break;
          case PLUGIN_PLAYER_SUBTYPE:
            ret += "ExternalPlayers";
            break;
          case PLUGIN_PROCESS_SUBTYPE:
            ret += "Process";
            break;
          case PLUGIN_SUBTITLE_SUBTYPE:
            ret += "Subtitle";
            break;
          case PLUGIN_TAGREADER_SUBTYPE:
            ret += "TagReaders";
            break;

        }
        ret += @"\";
      }

      if (flst.Type == SKIN_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + flst.SubType + @"\";
      }

      if (flst.Type == SKIN_MEDIA_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + flst.SubType + @"\" + "Media" + @"\";
      }

      if (flst.Type == SKIN_SOUNDS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + flst.SubType + @"\" + "Sounds" + @"\";
      }

      if (flst.Type == SKIN_ANIMATIONS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + flst.SubType + @"\" + "Media" + @"\" + "Animations" + @"\";
      }
      if (flst.Type == SKIN_TETRIS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Skin) + @"\" + flst.SubType + @"\" + "Media" + @"\" + "Tetris" + @"\";
      }

      if (flst.Type == THUMBS_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Thumbs) + @"\" + flst.SubType + @"\";
      }
      if (flst.Type == OTHER_TYPE)
      {
        if (String.IsNullOrEmpty(flst.SubType.Trim()))
          ret = Config.GetFolder(Config.Dir.Base) + @"\";
        else
          if (flst.SubType.StartsWith("%"))
          {
            ret = flst.SubType;
            foreach (Config.Dir option in Enum.GetValues(typeof(Config.Dir)))
            {
              ret = ret.Replace("%" + option.ToString() + "%", Config.GetFolder(option));
            }
            ret += @"\";
          }
          else
          {
            ret = Config.GetFolder(Config.Dir.Base) + @"\" + flst.SubType + @"\";
          }
      }
      if (flst.Type == TEXT_TYPE)
      {
        ret = Config.GetFolder(Config.Dir.Base) + @"\" + "Docs" + @"\";
      }
      if (flst.Type == SKIN_SYSTEMFONT_TYPE)
      {
        ret = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Fonts") + @"\";
      }

      if (string.IsNullOrEmpty(flst.FileProperties.OutputFileName))
        ret += Path.GetFileName(flst.FileName);
      else
        ret += flst.FileProperties.OutputFileName;
      return ret;
    }

  }

  public class MPIFileList
  {
    string _FileName = string.Empty;
    string _Type = string.Empty;
    string _SubType = string.Empty;
    string _Id = string.Empty;
    string _Op = string.Empty;
    string _Guid = string.Empty;
    public FilePropertiesClass FileProperties = new FilePropertiesClass();

    public MPIFileList()
    {
      FileName = string.Empty; ;
      Type = string.Empty;
      SubType = string.Empty;
      ID = string.Empty;
      Option = string.Empty;
      FilePropertiesClass FileProperties = new FilePropertiesClass();
    }

    public MPIFileList(string fn, string ty, string sty, string i)
    {
      FileName = fn;
      Type = ty;
      SubType = sty;
      ID = i;
      Option = string.Empty;
      FilePropertiesClass FileProperties = new FilePropertiesClass();
    }

    public MPIFileList(string fn, string ty, string sty, string i,string o)
    {
      FileName = fn;
      Type = ty;
      SubType = sty;
      ID = i;
      Option = o;
      FilePropertiesClass FileProperties = new FilePropertiesClass();
    }
    
    public MPIFileList(string fn, string ty, string sty, string i, string o, string g)
    {
      FileName = fn;
      Type = ty;
      SubType = sty;
      ID = i;
      Option = o;
      GUID = g;
      FilePropertiesClass FileProperties = new FilePropertiesClass();
    }
    public string FileName
    {
      get { return _FileName; }
      set { _FileName = value; }
    }

    public string FileNameShort
    {
      get { return Path.GetFileName(_FileName); }
    }

    public string Type
    {
      get { return _Type; }
      set { _Type = value; }
    }

    public bool SkinType
    {
      get { return this.Type == MPinstallerStruct.SKIN_TYPE || this.Type == MPinstallerStruct.SKIN_MEDIA_TYPE || this.Type == MPinstallerStruct.SKIN_SOUNDS_TYPE || this.Type == MPinstallerStruct.SKIN_ANIMATIONS_TYPE || this.Type == MPinstallerStruct.SKIN_TETRIS_TYPE; }
    }

    public string SubType
    {
      get { return _SubType; }
      set { _SubType = value; }
    }
    public string ID
    {
      get { return _Id; }
      set { _Id = value; }
    }

    public string GUID
    {
      get
      {
        return _Guid;
      }
      set { _Guid = value; }
    }

    public string Option
    {
      get { return _Op; }
      set
      {
        _Op = value;
        FileProperties.Parse(_Op);
      }
    }

    public void SetGuid()
    {
      if (Type != MPinstallerStruct.INTERNAL_TYPE)
        GUID = Guid.NewGuid().ToString();
      else
        GUID = string.Empty;
    }

  }

  public class UninstallInfo
  {
    private DateTime _date;
    private string _path;

    public UninstallInfo(string fil)
    {
      Path = System.IO.Path.GetFullPath(fil);
      Date = System.IO.File.GetCreationTime(fil);
    }
    public UninstallInfo(string fil, string dt)
    {
      Path = fil;
      Date = DateTime.FromFileTime(long.Parse(dt));
    }
    public DateTime Date
    {
      get { return _date; }
      set { _date = value; }
    }

    public string Path
    {
      get { return _path; }
      set { _path = value; }
    }

  }

  public class LanguageString
  {
    public string language;
    public string mapSting;
    public string dwCode;
    public string prefix;
    public string sufix;

    public LanguageString(string lang, string dc, string str)
    {
      language = lang;
      dwCode = dc;
      mapSting = str;
      prefix = string.Empty;
      sufix = string.Empty;
    }
    public LanguageString(string lang, string dc, string str, string pre, string su)
    {
      language = lang;
      dwCode = dc;
      mapSting = str;
      prefix = pre;
      sufix = su;
    }
    public LanguageString()
    {
      language = string.Empty;
      dwCode = string.Empty;
      mapSting = string.Empty;
      prefix = string.Empty;
      sufix = string.Empty;
    }
  }

  public class LanguageStringComparer : IComparer<LanguageString>
  {
    public int Compare(LanguageString x, LanguageString y)
    {
      if (x == null)
      {
        if (y == null)
        {
          // If x is null and y is null, they're
          // equal. 
          return 0;
        }
        else
        {
          // If x is null and y is not null, y
          // is greater. 
          return -1;
        }
      }
      else
      {
        // If x is not null...
        //
        if (y == null)
        // ...and y is null, x is greater.
        {
          return 1;
        }
        else
        {
          if (x.language.Trim().ToUpper() == y.language.Trim().ToUpper())
          {
            if (Convert.ToInt32(x.dwCode) == Convert.ToInt32(y.dwCode))
              return 0;
            else if (Convert.ToInt32(x.dwCode) > Convert.ToInt32(y.dwCode))
              return 1;
            else return -1;
          }
          else
          {
            return (x.language.CompareTo(y.language));
          }

        }
      }
    }
  }

  public class ActionInfo : IEquatable<ActionInfo>
  {
    public String Place;
    public int Id;
    public String Command;
    public ActionInfo(string p, int i, string c)
    {
      Place = p;
      Id = i;
      Command = c;
    }
    public bool Equals(ActionInfo ac)
    {
      if (Place == ac.Place && Id == ac.Id)
        return true;
      else return false;
    }

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="xmp">FIle list</param>
    public void ExecuteAction(MPinstallerStruct xmp)
    {
      switch (Place)
      {
        case "POSTSETUP":
          switch (Id)
          {
            case 0:
              if (Command == "MediaPortal.exe" || Command == "Configuration.exe")
              {
                MPIutils.StartApp(Config.GetFile(Config.Dir.Base, Command));
              }
              else
              {
                MPIFileList fs_ = xmp.FindFile(Command);
                MPIutils.StartApp(Config.GetFile(Config.Dir.Base, MPinstallerStruct.GetDirEntry(fs_)));
              }
              break;
            case 1:
              MPIFileList fs = xmp.FindFile(Command);
              MPIutils.LoadPlugins(MPinstallerStruct.GetDirEntry(fs));
              break;
          }
          break;
      }
    }

    override public string ToString()
    {
      string x_ret = string.Empty;
      switch (Place)
      {
        case "POSTSETUP":
          switch (Id)
          {
            case 0:
              x_ret = "Run " + Command;
              break;
            case 1:
              x_ret = "Configure plugin ";
              break;
          }
          break;
      }
      return x_ret;
    }
  }

  public class GroupString
  {
    string _id;
    string _name;
    bool _checked;

    public GroupString()
    {
      Id = string.Empty;
      Name = string.Empty;
      Checked = true;
    }

    public GroupString(string wid, string wname)
    {
      Id = wid;
      Name = wname;
      Checked = false;
    }
    [System.Xml.Serialization.XmlAttribute] 
    public string Id
    {
      get { return _id; }
      set { _id = value; }
    }
    [System.Xml.Serialization.XmlAttribute] 
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
    [System.Xml.Serialization.XmlAttribute] 
    public bool Checked
    {
      get { return _checked; }
      set { _checked = value; }
    }

    override public string ToString()
    {
      return Id + " - " + Name;
    }
  }

  public class GroupStringMapping
  {
    string _id;
    string _filename;
    public GroupStringMapping(string wid, string wname)
    {
      Id = wid;
      FileName = wname;
    }
    public string Id
    {
      get { return _id; }
      set { _id = value; }
    }
    public string FileName
    {
      get { return _filename; }
      set { _filename = value; }
    }

  }
}
