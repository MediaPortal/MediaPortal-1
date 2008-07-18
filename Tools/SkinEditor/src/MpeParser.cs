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
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Mpe.Controls;
using Mpe.Controls.Properties;

namespace Mpe
{
  /// <summary>
  /// This class can be used to load all of the resources and screen
  /// definitions for a particular directory.
  /// </summary>
  public class MpeParser
  {
    #region Variables

    private DirectoryInfo homeDir;
    private DirectoryInfo mediaPortalDir;

    private SortedList fonts;
    private FileInfo fontFile;
    private DirectoryInfo fontDir;

    private SortedList imageFiles;
    private Hashtable imageThumbnails;

    private SortedList screens;

    private Hashtable languageFiles;
    private Hashtable stringTables;

    private Hashtable controls;
    private FileInfo controlFile;

    #endregion

    #region Constructors

    public MpeParser(DirectoryInfo skinDir, DirectoryInfo mpDir)
    {
      if (skinDir == null || skinDir.Exists == false)
      {
        throw new MpeParserException("Invalid skin directory");
      }
      mediaPortalDir = mpDir;
      homeDir = skinDir;
    }

    #endregion

    #region Properties

    /// <summary>
    /// This method will return the home directory of the skin.
    /// </summary>
    public DirectoryInfo HomeDir
    {
      get { return homeDir; }
    }

    /// <summary>
    /// This method will return the name of the skin.
    /// </summary>
    public string SkinName
    {
      get { return homeDir.Name; }
    }

    /// <summary>
    /// This method will return a list of fonts used by the skin.
    /// </summary>
    public MpeFont[] Fonts
    {
      get
      {
        if (fonts == null || fontFile == null)
        {
          fontFile = new FileInfo(homeDir.FullName + "\\fonts.xml");
          if (fontFile.Exists == false)
          {
            throw new MpeParserException("Could not locate fonts");
          }
          fontDir = new DirectoryInfo(homeDir.FullName + "\\fonts");
          if (fontDir.Exists == false)
          {
            fontDir.Create();
          }
          try
          {
            XPathDocument doc = new XPathDocument(fontFile.FullName);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator iterator = nav.Select("/fonts/font");
            fonts = new SortedList();
            while (iterator.MoveNext())
            {
              MpeFont font = new MpeFont();
              font.Load(iterator, this);
              fonts.Add(font.Name, font);
              MpeLog.Debug("Created MpeFont [" + font.Name + "]");
            }
          }
          catch (Exception e)
          {
            throw new MpeParserException(e.Message);
          }
        }
        MpeFont[] result = new MpeFont[fonts.Count];
        for (int i = 0; i < result.Length; i++)
        {
          result[i] = (MpeFont) fonts.GetByIndex(i);
        }
        return result;
      }
    }

    /// <summary>
    /// This method returns a reference to the file which defines the fonts used
    /// by this skin.
    /// </summary>
    public FileInfo FontFile
    {
      get { return fontFile; }
    }

    /// <summary>
    /// 
    /// </summary>
    public string[] FontNames
    {
      get
      {
        string[] names = new string[fonts.Count];
        for (int i = 0; i < names.Length; i++)
        {
          names[i] = ((MpeFont) fonts.GetByIndex(i)).Name;
        }
        return names;
      }
    }

    public DirectoryInfo FontDir
    {
      get { return fontDir; }
    }

    /// <summary>
    /// This method returns an array of all the images used by this skin.
    /// </summary>
    public FileInfo[] ImageFiles
    {
      get
      {
        if (imageFiles == null || imageFiles.Count == 0)
        {
          imageFiles = new SortedList();
          imageThumbnails = new Hashtable();
          DirectoryInfo dir = new DirectoryInfo(homeDir.FullName + "\\Media");
          if (dir == null || !dir.Exists)
          {
            throw new MpeParserException("Could not locate image directory");
          }
          FileInfo[] temp = dir.GetFiles();
          for (int i = 0; i < temp.Length; i++)
          {
            string type = temp[i].Extension.ToUpper();
            if (type.Equals(".JPG") || type.Equals(".GIF") || type.Equals(".PNG") || type.Equals(".BMP"))
            {
              imageFiles.Add(temp[i].Name, temp[i]);
              imageThumbnails.Add(temp[i].Name, CreateThumbnail(temp[i]));
              MpeLog.Debug("Adding Image [" + temp[i].FullName + "]");
            }
          }
        }
        FileInfo[] result = new FileInfo[imageFiles.Count];
        for (int i = 0; i < result.Length; i++)
        {
          result[i] = (FileInfo) imageFiles.GetByIndex(i);
        }
        return result;
      }
    }

    public DirectoryInfo ImageFolder
    {
      get { return new DirectoryInfo(homeDir.FullName + "\\Media"); }
    }

    public MpeScreenInfo[] Screens
    {
      get
      {
        if (screens == null || screens.Count == 0)
        {
          FileInfo[] temp = homeDir.GetFiles("*.xml");
          screens = new SortedList();
          for (int i = 0; i < temp.Length; i++)
          {
            try
            {
              MpeScreenInfo info = GenerateScreenInfo(temp[i]);
              if (info != null)
              {
                screens.Add(temp[i].Name, info);
                MpeLog.Debug("Added " + info.Type.ToString() + " [" + info.Name + "]");
              }
              else
              {
                MpeLog.Warn("Skipping " + temp[i].Name + " - Invalid screen file.");
              }
            }
            catch (Exception e)
            {
              MpeLog.Debug(e);
              MpeLog.Warn("Skipping " + temp[i].Name + " - Invalid screen file.");
            }
          }
        }
        MpeScreenInfo[] result = new MpeScreenInfo[screens.Count];
        for (int i = 0; i < result.Length; i++)
        {
          result[i] = (MpeScreenInfo) screens.GetByIndex(i);
        }
        return result;
      }
    }

    public DirectoryInfo ScreenFolder
    {
      get { return homeDir; }
    }

    public Hashtable LanguageFiles
    {
      get
      {
        if (languageFiles == null)
        {
          //-------
          languageFiles = new Hashtable();
          DirectoryInfo dir = new DirectoryInfo(mediaPortalDir.FullName+@"\language");
          foreach (FileInfo file in dir.GetFiles("strings_*.xml"))
          {
            int pos = file.Name.IndexOf('_') + 1;
            string cultName = file.Name.Substring(pos, file.Name.Length - file.Extension.Length - pos);

            try
            {
              CultureInfo cultInfo = new CultureInfo(cultName);
//              _availableLanguages.Add(cultInfo.EnglishName, cultName);
              languageFiles.Add(cultInfo.EnglishName, file);
            }
            catch (ArgumentException)
            {
            }

          }        
        }
        return languageFiles;
      }
    }

    public Hashtable StringTables
    {
      get
      {
        if (stringTables == null)
        {
          stringTables = new Hashtable();
          IEnumerator e = LanguageFiles.Keys.GetEnumerator();
          while (e.MoveNext())
          {
            string language = (string) e.Current;
            FileInfo file = (FileInfo) LanguageFiles[language];
            try
            {
              MpeLog.Debug("Adding StringTable [" + language + "]");
              stringTables.Add(language, CreateStringTable(language, file));
            }
            catch (Exception ex)
            {
              MpeLog.Debug(ex);
              MpeLog.Error(ex);
            }
          }
        }
        return stringTables;
      }
    }

    public FileInfo ControlFile
    {
      get { return controlFile; }
    }

    public MpeControlType[] ControlKeys
    {
      get
      {
        if (controls == null)
        {
          try
          {
            controlFile = new FileInfo(homeDir.FullName + "\\references.xml");
          }
          catch (Exception ee)
          {
            MpeLog.Debug(ee);
            throw new MpeParserException("Could not locate or read control reference file");
          }

          try
          {
            controls = new Hashtable();
            XPathDocument doc = new XPathDocument(controlFile.FullName);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator iterator = null;
            // Create and Load all of the Known controls
            for (int i = 0; i < MpeControlType.KnownControlTypes.Length; i++)
            {
              try
              {
                MpeControlType type = MpeControlType.KnownControlTypes[i];
                iterator = nav.Select("//controls/control[type='" + type.ToString() + "']");
                MpeLog.Debug("Creating control [" + type.ToString() + "]");
                if (iterator.MoveNext())
                {
                  controls.Add(type, CreateControl(type, iterator));
                  MpeLog.Info("Created control [" + type.DisplayName + "]");
                }
              }
              catch (Exception ee)
              {
                MpeLog.Debug(ee);
                MpeLog.Warn(ee);
              }
            }
            // Create and Load Screen
            iterator = nav.Select("//controls");
            if (iterator.MoveNext())
            {
              MpeLog.Debug("Creating [Screen]");
              controls.Add(MpeControlType.Screen, CreateControl(MpeControlType.Screen, iterator));
              MpeLog.Debug("Created [Screen]");
            }
            // Create and Load Group
            if (controls[MpeControlType.Group] == null)
            {
              MpeLog.Debug("Creating control [" + MpeControlType.Group.DisplayName + "]");
              MpeGroup group = new MpeGroup();
              group.Parser = this;
              controls.Add(MpeControlType.Group, group);
              MpeLog.Info("Created control [" + MpeControlType.Group.DisplayName + "]");
            }
            // Create and Load Custom Controls
            iterator = nav.Select("//control");
            while (iterator.MoveNext())
            {
              XPathNodeIterator it = iterator.Current.SelectChildren("type", "");
              if (it.MoveNext())
              {
                MpeControlType type = MpeControlType.Create(it.Current.Value);
                MpeLog.Debug("Creating custom control [" + type.ToString() + "]");
                if (controls[type] == null)
                {
                  controls.Add(type, CreateControl(type, iterator));
                  MpeLog.Info("Created custom control [" + type.DisplayName + "]");
                }
              }
            }
          }
          catch (Exception ee)
          {
            MpeLog.Debug(ee);
            throw new MpeParserException(ee.Message);
          }
        }

        IEnumerator e = controls.Keys.GetEnumerator();
        MpeControlType[] result = new MpeControlType[controls.Keys.Count];
        int n = 0;
        while (e.MoveNext())
        {
          result[n++] = (MpeControlType) e.Current;
        }
        return result;
      }
    }

    #endregion

    #region Methods

    public void Load()
    {
      MpeLog.Info("Loading Skin [" + homeDir.FullName + "]");
      MpeLog.Progress(0, 6);
      MpeLog.Info("Parsing Languages", 1);
      MpeLog.Info("Loaded " + LanguageFiles.Count + " Languages", 1);
      MpeLog.Info("Parsing Strings", 2);
      MpeLog.Info("Loaded " + StringTables.Count + " String Tables", 2);
      MpeLog.Info("Parsing Fonts", 3);
      MpeLog.Info("Loaded " + Fonts.Length + " Fonts", 3);
      MpeLog.Info("Parsing Images", 4);
      MpeLog.Info("Loaded " + ImageFiles.Length + " Images", 4);
      MpeLog.Info("Parsing Screens", 5);
      MpeLog.Info("Loaded " + Screens.Length + " Screens", 5);
      MpeLog.Info("Parsing Controls", 6);
      MpeLog.Info("Loaded " + ControlKeys.Length + " Controls", 6);
      MpeLog.Info("Loaded Skin [" + homeDir.FullName + "]", 0);
    }

    public void Destroy()
    {
      // Controls
      if (controls != null)
      {
        MpeControlType[] keys = ControlKeys;
        for (int i = 0; i < keys.Length; i++)
        {
          MpeControl c = GetControl(keys[i]);
          c.Destroy();
        }
        controls = null;
        controlFile = null;
      }
      // Images
      if (imageFiles != null)
      {
        imageFiles.Clear();
        imageFiles = null;
      }
      if (imageThumbnails != null)
      {
        IEnumerator e = imageThumbnails.Values.GetEnumerator();
        while (e.MoveNext())
        {
          Bitmap b = (Bitmap) e.Current;
        }
        imageThumbnails.Clear();
        imageThumbnails = null;
      }
      // Screens
      if (screens != null)
      {
        screens.Clear();
        screens = null;
      }
      // Fonts
      if (fonts != null)
      {
        for (int i = 0; i < fonts.Count; i++)
        {
          MpeFont f = (MpeFont) fonts.GetByIndex(i);
          f.Destroy();
        }
        fonts.Clear();
        fonts = null;
        fontFile = null;
        fontDir = null;
      }
      // Languages and Strings
      if (languageFiles != null)
      {
        languageFiles.Clear();
        languageFiles = null;
      }
      if (stringTables != null)
      {
        stringTables.Clear();
        stringTables = null;
      }
      // Folders
      homeDir = null;
      mediaPortalDir = null;

      // Invoke the Garbage Collector to finish cleaning up resources
      GC.Collect();
    }

    #endregion

    #region Methods: Parsing

    /// <summary>
    /// A helper method make adding elements to a node simplier. If the element
    /// exists, then it is replaced with the new value.
    /// </summary>
    /// <param name="doc">The xml document being added to</param>
    /// <param name="node">The node in the document being added to</param>
    /// <param name="name">The tag name of the new element</param>
    /// <param name="value">The value of the element</param>
    public void SetValue(XmlDocument doc, XmlNode node, string name, string value)
    {
      // First check to see if the element already exists, and if so, remove it
      if (value == null || value.Length == 0)
      {
        value = "-";
      }
      XmlNode n = node.SelectSingleNode(name);
      if (n != null)
      {
        n.RemoveAll();
        n.AppendChild(doc.CreateTextNode(value));
        return;
      }
      XmlElement e = doc.CreateElement(name);
      e.AppendChild(doc.CreateTextNode(value));
      node.AppendChild(e);
    }

    public void SetInt(XmlDocument doc, XmlNode node, string name, int value)
    {
      SetValue(doc, node, name, value.ToString());
    }

    public void SetColor(XmlDocument doc, XmlNode node, string name, Color value)
    {
      SetValue(doc, node, name, string.Format("{0:x}", value.ToArgb()));
    }

    public void SetPadding(XmlDocument doc, XmlNode node, string name, MpeControlPadding value)
    {
      // First check to see if the element already exists, and if so, remove it
      XmlNode n = node.SelectSingleNode(name);
      if (n != null)
      {
        node.RemoveChild(n);
      }
      XmlElement padding = doc.CreateElement(name);
      SetInt(doc, padding, "top", value.Top);
      SetInt(doc, padding, "right", value.Right);
      SetInt(doc, padding, "bottom", value.Bottom);
      SetInt(doc, padding, "left", value.Left);

      node.AppendChild(padding);
    }

    public void RemoveNode(XmlNode node, string name)
    {
      XmlNode n = node.SelectSingleNode(name);
      if (n != null)
      {
        node.RemoveChild(n);
      }
    }

    public int GetInt(XPathNodeIterator iterator, string tagName, int defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        try
        {
          return int.Parse(i.Current.Value);
        }
        catch
        {
          //
        }
      }
      return defaultValue;
    }

    public bool GetBoolean(XPathNodeIterator iterator, string tagName, bool defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        string s = i.Current.Value != null ? i.Current.Value : "";
        if (s.ToLower().Equals("yes") || s.ToLower().Equals("true"))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      return defaultValue;
    }

    public string GetString(XPathNodeIterator iterator, string tagName, string defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        return i.Current.Value != null ? i.Current.Value : defaultValue;
      }
      return defaultValue;
    }

    public Color GetColor(XPathNodeIterator iterator, string tagName, Color defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        try
        {
          return Color.FromArgb(int.Parse(i.Current.Value, NumberStyles.AllowHexSpecifier));
        }
        catch (Exception e)
        {
          MpeLog.Debug(e);
        }
      }
      return defaultValue;
    }

    public FileInfo GetImageFile(XPathNodeIterator iterator, string tagName, FileInfo defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        try
        {
          string s = i.Current.Value;
          if (s != null && s.Equals("-"))
          {
            return null;
          }
          FileInfo file = GetImageFile(i.Current.Value);
          if (file != null && file.Exists)
          {
            return file;
          }
        }
        catch (Exception e)
        {
          MpeLog.Debug(e);
        }
      }
      return defaultValue;
    }

    public MpeFont GetFont(XPathNodeIterator iterator, string tagName, MpeFont defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        try
        {
          MpeFont font = GetFont(i.Current.Value);
          if (font != null)
          {
            return font;
          }
        }
        catch
        {
          MpeLog.Warn("Could not load the specified font.");
        }
      }
      return defaultValue;
    }

    public MpeLayoutStyle GetLayout(XPathNodeIterator iterator, string tagName, MpeLayoutStyle defaultValue)
    {
      string s = GetString(iterator, "mpe/layout", "");
      if (s.Equals(MpeLayoutStyle.VerticalFlow.ToString()))
      {
        return MpeLayoutStyle.VerticalFlow;
      }
      else if (s.Equals(MpeLayoutStyle.HorizontalFlow.ToString()))
      {
        return MpeLayoutStyle.HorizontalFlow;
      }
      else if (s.Equals(MpeLayoutStyle.Grid.ToString()))
      {
        return MpeLayoutStyle.Grid;
      }
      else
      {
        return defaultValue;
      }
    }

    public MpeControlPadding GetPadding(XPathNodeIterator iterator, string tagName, MpeControlPadding defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        return new MpeControlPadding(
          GetInt(i, "top", 0),
          GetInt(i, "right", 0),
          GetInt(i, "bottom", 0),
          GetInt(i, "left", 0)
          );
      }
      return defaultValue;
    }

    //public MpeAnimationType GetAnimation(XPathNodeIterator iterator, string tagName, MpeAnimationType defaultValue)
    //{
    //  //string s = GetString(iterator, tagName, "");
    //  //if (s.Equals(MpeAnimationType.FlyInFromBottom.ToString()))
    //  //{
    //  //  return MpeAnimationType.FlyInFromBottom;
    //  //}
    //  //else if (s.Equals(MpeAnimationType.FlyInFromLeft.ToString()))
    //  //{
    //  //  return MpeAnimationType.FlyInFromLeft;
    //  //}
    //  //else if (s.Equals(MpeAnimationType.FlyInFromRight.ToString()))
    //  //{
    //  //  return MpeAnimationType.FlyInFromRight;
    //  //}
    //  //else if (s.Equals(MpeAnimationType.FlyInFromTop.ToString()))
    //  //{
    //  //  return MpeAnimationType.FlyInFromTop;
    //  //}
    //  //else if (s.Equals(MpeAnimationType.None.ToString()))
    //  //{
    //  //  return MpeAnimationType.None;
    //  //}
    //  //else
    //  //{
    //  //  return defaultValue;
    //  //}
    //  return new MpeAnimationType();
    //}

    public MpeControlAlignment GetAlignment(XPathNodeIterator iterator, string tagName, MpeControlAlignment defaultValue)
    {
      string s = GetString(iterator, tagName, "");
      if (s.ToLower().Equals("right"))
      {
        return MpeControlAlignment.Right;
      }
      else if (s.ToLower().Equals("left"))
      {
        return MpeControlAlignment.Left;
      }
      else if (s.ToLower().Equals("center"))
      {
        return MpeControlAlignment.Center;
      }
      return defaultValue;
    }

    #endregion

    #region Methods: Strings

    public MpeStringTable GetStringTable(string language)
    {
      return (MpeStringTable) stringTables[language];
    }

    public string GetString(string language, int id)
    {
      MpeStringTable t = GetStringTable(language);
      if (t != null)
      {
        string s = t[id];
        if (s != null)
        {
          return s;
        }
      }
      return id.ToString();
    }

    public FileInfo GetLanguageFile(string language)
    {
      return (FileInfo) languageFiles[language];
    }

    private MpeStringTable CreateStringTable(string language, FileInfo file)
    {
      MpeStringTable table = new MpeStringTable(language);
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlTextReader reader = new XmlTextReader(file.FullName);
        //docencoding = reader.Encoding;
        doc.Load(reader);
        if (doc.DocumentElement == null) return table;
        string strRoot = doc.DocumentElement.Name;
        if (strRoot != "Language") return table;
        XmlNodeList list = doc.DocumentElement.SelectNodes("Section/String");
        foreach (XmlNode node in list)
        {
          table.Add(int.Parse(node.Attributes["id"].Value), node.InnerText);
        }
        reader.Close();
        return table;

        /*
        XPathDocument doc = new XPathDocument(file.FullName);
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iterator = nav.Select("/Language/Section/String");
        XPathNodeIterator i, j;
        MpeStringTable table = new MpeStringTable(language);
        while (iterator.MoveNext())
        {
          //i = iterator.Current.SelectChildren("id", "");
          //j = iterator.Current.SelectChildren("value", "");
          //if (i.MoveNext() && j.MoveNext())
          //{
            try
            {
              System.Windows.Forms.MessageBox.Show(iterator.Current.GetAttribute("Id","/Language/Section/String")+iterator.Current.Value.Trim());
              table.Add(int.Parse(iterator.Current.GetAttribute("Id","")), iterator.Current.Value.Trim());
            }
            catch (Exception ee)
            {
              MpeLog.Warn("Language=[" + language + "] " + ee.Message);
            }
          //}
        }
        return table;
         */ 
      }
      catch (Exception e)
      {
        throw new MpeParserException(e.Message);
      }
    }

    public void SaveStringTable(MpeStringTable stringTable)
    {
      MpeLog.Error("Saveing the stringtable is disabled");
      return;
      if (stringTable == null)
      {
        return;
      }
      MpeLog.Debug("MpeParser.SaveStringTable(" + stringTable.Language + ")");
      XmlTextWriter writer = null;
      try
      {
        FileInfo file = GetLanguageFile(stringTable.Language);
        if (file == null || file.Exists == false)
        {
          throw new MpeParserException("Could not open language file for <" + stringTable.Language + ">");
        }

        XmlDocument doc = new XmlDocument();
        XmlElement strings = doc.CreateElement("strings");
        doc.AppendChild(strings);

        SetInt(doc, strings, "characters", 255);

        int[] keys = stringTable.Keys;
        for (int i = 0; i < keys.Length; i++)
        {
          XmlElement s = doc.CreateElement("string");
          SetInt(doc, s, "id", keys[i]);
          SetValue(doc, s, "value", stringTable[keys[i]]);
          strings.AppendChild(s);
        }

        writer = new XmlTextWriter(file.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        doc.WriteTo(writer);
        writer.Close();
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        throw new MpeParserException("Error saving control: " + e.Message, e);
      }
      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }
    }

    #endregion

    #region Methods: Fonts

    public MpeFont GetFont(string name)
    {
      return (MpeFont) fonts[name];
    }

    public void SaveFont(MpeFont font)
    {
      if (font == null)
      {
        throw new MpeParserException("Invalid Font");
      }
      XmlTextWriter writer = null;
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(fontFile.FullName);
        XmlNode node = doc.SelectSingleNode("/fonts/font[name='" + font.Name + "']");
        if (node == null)
        {
          XmlNode parent = doc.SelectSingleNode("/fonts");
          if (parent == null)
          {
            throw new MpeParserException("Invalid fonts.xml file.");
          }
          node = doc.CreateElement("font");
          parent.AppendChild(node);
        }
        else
        {
          node.RemoveAll();
        }
        // First get the existing font and destroy it
        MpeFont oldFont = GetFont(font.Name);
        if (oldFont != null)
        {
          fonts.Remove(font.Name);
          oldFont.Destroy();
        }
        // Save the new font and store a copy of it
        font.Save(doc, node, this, null);
        fonts.Add(font.Name, new MpeFont(font));
        // Write the document
        writer = new XmlTextWriter(fontFile.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        doc.WriteTo(writer);
      }
      catch (Exception e)
      {
        throw new MpeParserException(e.Message);
      }
      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }
    }

    public string AddFont()
    {
      MpeFont font = null;
      string s = "NewFont";
      int i = 1;
      do
      {
        s = (i == 1) ? "NewFont" : "NewFont" + i.ToString();
        i++;
        font = GetFont(s);
      }
      while (font != null);
      SaveFont(new MpeFont(s, true));
      return s;
    }

    public void RenameFont(string currentName, string newName)
    {
      MpeFont font = GetFont(currentName);
      if (font == null)
      {
        throw new MpeParserException("Unexpected errors occured.");
      }
      MpeFont fontCheck = GetFont(newName);
      if (fontCheck != null)
      {
        throw new MpeParserException("A font with the name [" + newName + "] already exists.");
      }
      XmlTextWriter writer = null;
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(fontFile.FullName);
        XmlNode parent = doc.SelectSingleNode("/fonts");
        if (parent == null)
        {
          throw new MpeParserException("Invalid fonts.xml file.");
        }
        XmlNode node = parent.SelectSingleNode("font[name='" + currentName + "']");
        if (node != null)
        {
          // Remove the font from the collection
          fonts.Remove(currentName);
          // Rename
          font.Name = newName;
          font.Save(doc, node, this, null);
          // Add the modified font to the collection
          fonts.Add(newName, font);
        }
        writer = new XmlTextWriter(fontFile.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        doc.WriteTo(writer);
      }
      catch (Exception e)
      {
        throw new MpeParserException(e.Message);
      }
      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }
    }

    public void DeleteFont(string name)
    {
      MpeFont font = GetFont(name);
      if (font != null)
      {
        XmlTextWriter writer = null;
        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load(fontFile.FullName);
          XmlNode parent = doc.SelectSingleNode("/fonts");
          if (parent == null)
          {
            throw new MpeParserException("Invalid fonts.xml file.");
          }
          XmlNode node = parent.SelectSingleNode("font[name='" + name + "']");
          if (node != null)
          {
            parent.RemoveChild(node);
          }
          writer = new XmlTextWriter(fontFile.FullName, Encoding.UTF8);
          writer.Formatting = Formatting.Indented;
          writer.Indentation = 3;
          doc.WriteTo(writer);

          if (font.TextureDataFile != null)
          {
            font.TextureDataFile.Delete();
          }
          if (font.TextureFile != null)
          {
            font.TextureFile.Delete();
          }
          fonts.Remove(name);
        }
        catch (Exception e)
        {
          throw new MpeParserException(e.Message);
        }
        finally
        {
          if (writer != null)
          {
            writer.Close();
          }
        }
      }
    }

    #endregion

    #region Methods: Images

    protected Image CreateThumbnail(FileInfo f)
    {
      Bitmap b = new Bitmap(f.FullName);
      Image thumb = null;
      if (b.Width > 128 || b.Height > 128)
      {
        if (b.Width >= b.Height)
        {
          int h = 128 * b.Height / b.Width;
          if (h < 1)
            h = 1;
          thumb = new Bitmap(b, 128, h);
        }
        else
        {
          int w = 128 * b.Width / b.Height;
          if (w < 1)
            w = 1;
          thumb = new Bitmap(b, w, 128);
        }
      }
      else
      {
        thumb = new Bitmap(b, b.Width, b.Height);
      }
      b.Dispose();
      return thumb;
    }

    public Image GetImageThumbnail(string name)
    {
      return (Image) imageThumbnails[name];
    }

    public FileInfo GetImageFile(string name)
    {
      return (FileInfo) imageFiles[name];
    }

    public FileInfo AddImageFile(FileInfo newImage)
    {
      FileInfo file = new FileInfo(ImageFolder.FullName + "\\" + newImage.Name);
      if (file.Exists)
      {
        string extension = file.Extension;
        int i = 1;
        string s = newImage.Name.Substring(0, newImage.Name.LastIndexOf("."));
        do
        {
          i++;
          file = new FileInfo(ImageFolder.FullName + "\\" + s + i.ToString() + extension);
        }
        while (file.Exists);
      }
      newImage.CopyTo(file.FullName, false);
      file.Refresh();
      imageFiles.Add(file.Name, file);
      imageThumbnails.Add(file.Name, CreateThumbnail(file));
      return file;
    }

    public FileInfo RenameImageFile(string name, string newName)
    {
      FileInfo f = GetImageFile(name);
      if (f != null)
      {
        f.MoveTo(f.DirectoryName + "\\" + newName);
        f.Refresh();
        imageFiles.Remove(name);
        imageThumbnails.Remove(name);
        imageFiles.Add(newName, f);
        imageThumbnails.Add(newName, f);
        return f;
      }
      throw new MpeParserException("Could not locate original image file.");
    }

    public void DeleteImageFile(string name)
    {
      FileInfo f = GetImageFile(name);
      if (f != null && f.Exists)
      {
        f.Delete();
        f.Refresh();
      }
      imageFiles.Remove(name);
      imageThumbnails.Remove(name);
    }

    #endregion

    #region Methods: Controls

    public MpeControl GetControl(MpeControlType type)
    {
      if (controls[type] != null)
      {
        return (MpeControl) controls[type];
      }
      return null;
      //throw new MpeParserException("Unknown control type [" + type.ToString() + "]");
    }

    public MpeControl CreateControl(MpeControlType type)
    {
      MpeControl c = GetControl(type);
      if (c != null)
      {
        return c.Copy();
      }
      if (type.IsKnown)
      {
        string filter = "Mpe" + type.DisplayName;
        Type t = typeof(MpeControl);
        if (t != null)
        {
          Module mod = t.Module;
          Type[] types = mod.FindTypes(Module.FilterTypeName, filter);
          if (types.Length == 1)
          {
            object o = types[0].InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
            c = (MpeControl) o;
          }
        }
        if (c == null)
        {
          MpeLog.Warn("Could not find implementation of type = [" + type.DisplayName + "]");
          c = new MpeControl();
        }
      }
      else
      {
        c = new MpeControl();
      }
      c.Type = type;
      c.Parser = this;
      return c;
    }

    private MpeControl CreateControl(MpeControlType type, XPathNodeIterator iterator)
    {
      try
      {
        MpeControl c = null;
        if (type.IsKnown)
        {
          string filter = "Mpe" + type.DisplayName;
          Type t = typeof(MpeControl);
          if (t != null)
          {
            Module mod = t.Module;
            Type[] types = mod.FindTypes(Module.FilterTypeName, filter);
            if (types.Length == 1)
            {
              object o = types[0].InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
              c = (MpeControl) o;
            }
          }
          if (c == null)
          {
            MpeLog.Warn("Could not find implementation of type = [" + type.DisplayName + "]");
            c = new MpeControl();
          }
        }
        else
        {
          c = new MpeControl();
        }
        c.Type = type;
        c.Parser = this;
        c.Load(iterator, this);
        return c;
      }
      catch (Exception ee)
      {
        MpeLog.Debug(ee);
        throw new MpeParserException(ee.Message);
      }
    }

    public void SaveControl(MpeControl control)
    {
      MpeLog.Debug("MpeParser.SaveControl(" + control.Type.ToString() + ")");
      XmlTextWriter writer = null;
      try
      {
        XmlDocument d = new XmlDocument();
        d.Load(controlFile.FullName);
        XmlNode node = null;
        if (control.Type != MpeControlType.Screen)
        {
          node = d.SelectSingleNode("/controls/control[type='" + control.Type.ToString() + "']", null);
          if (node == null)
          {
            XmlNode n = d.SelectSingleNode("/controls");
            if (n == null)
            {
              throw new MpeParserException("Invalid reference.xml file. Incorrect root node.");
            }
            node = d.CreateElement("control");
            n.AppendChild(node);
          }
          else
          {
            node.RemoveAll();
          }
        }
        else
        {
          node = d.SelectSingleNode("/controls");
        }
        control.Save(d, node, this, null);
        controls.Remove(control.Type);
        controls.Add(control.Type, control);

        writer = new XmlTextWriter(controlFile.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        d.WriteTo(writer);
        writer.Close();
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        throw new MpeParserException("Error saving control: " + e.Message, e);
      }
      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }
    }

    #endregion

    #region Methods: Screens

    private MpeScreenInfo GenerateScreenInfo(FileInfo screen)
    {
      XPathDocument doc = new XPathDocument(screen.FullName);
      XPathNavigator nav = doc.CreateNavigator();
      XPathNodeIterator iterator = nav.Select("//" + MpeControlType.Screen.ToString());
      if (iterator.MoveNext())
      {
        string s = GetString(iterator, "type", "");
        if (s == MpeScreenType.Dialog.ToString().ToLower())
        {
          return new MpeScreenInfo(screen, MpeScreenType.Dialog);
        }
        else if (s == MpeScreenType.OnScreenDisplay.ToString().ToLower())
        {
          return new MpeScreenInfo(screen, MpeScreenType.OnScreenDisplay);
        }
        else
        {
          return new MpeScreenInfo(screen, MpeScreenType.Window);
        }
      }
      return null;
    }

    public MpeScreenInfo ModifyScreenType(string name, MpeScreenType type)
    {
      MpeScreenInfo screen = GetScreenInfo(name);
      if (screen == null)
      {
        throw new MpeParserException("Invalid screen name.");
      }
      if (screen.File == null)
      {
        throw new MpeParserException("Invalid screen file.");
      }
      XmlTextWriter writer = null;
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(screen.File.FullName);
        XmlNode node = doc.SelectSingleNode(MpeControlType.Screen.ToString());
        if (node == null)
        {
          throw new MpeParserException("Invalid screen file.");
        }
        SetValue(doc, node, "type", type.ToString().ToLower());

        writer = new XmlTextWriter(screen.File.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        doc.WriteTo(writer);
        writer.Close();

        screen.Type = type;
        return screen;
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        MpeLog.Error(e);
        throw new MpeParserException(e.Message);
      }
    }

    public FileInfo GetScreenFile(string name)
    {
      object o = screens[name];
      if (o != null)
      {
        return ((MpeScreenInfo) o).File;
      }
      return null;
    }

    public MpeScreenInfo GetScreenInfo(string name)
    {
      return (MpeScreenInfo) screens[name];
    }

    public FileInfo AddScreenFile(FileInfo newScreen)
    {
      FileInfo file = new FileInfo(ScreenFolder.FullName + "\\" + newScreen.Name);
      if (file.Exists)
      {
        int i = 1;
        string s = newScreen.Name.Substring(0, newScreen.Name.LastIndexOf("."));
        do
        {
          i++;
          file = new FileInfo(ScreenFolder.FullName + "\\" + s + i.ToString() + ".xml");
        }
        while (file.Exists);
      }
      newScreen.CopyTo(file.FullName, false);
      file.Refresh();
      screens.Add(file.Name, GenerateScreenInfo(file));
      return file;
    }

    public FileInfo AddScreenFile()
    {
      FileInfo file = new FileInfo(ScreenFolder.FullName + "\\NewScreen.xml");
      if (file.Exists)
      {
        int i = 1;
        do
        {
          i++;
          file = new FileInfo(ScreenFolder.FullName + "\\NewScreen" + i.ToString() + ".xml");
        }
        while (file.Exists);
      }
      StreamWriter w = file.CreateText();
      w.WriteLine("<!-- Generated By the MediaPortalEditor -->");
      w.Close();
      MpeScreen screen = CreateScreen();
      SaveScreen(screen, file);
      file.Refresh();
      screens.Add(file.Name, GenerateScreenInfo(file));
      return file;
    }

    public MpeScreenInfo RenameScreenFile(string name, string newName)
    {
      FileInfo f = GetScreenFile(name);
      if (f != null)
      {
        f.MoveTo(f.DirectoryName + "\\" + newName);
        f.Refresh();
        screens.Remove(name);
        MpeScreenInfo result = GenerateScreenInfo(f);
        screens.Add(newName, result);
        return result;
      }
      throw new MpeParserException("Could not locate original screen file.");
    }

    public void DeleteScreenFile(string name)
    {
      FileInfo f = GetScreenFile(name);
      if (f != null && f.Exists)
      {
        f.Delete();
        f.Refresh();
      }
      screens.Remove(name);
    }

    public MpeScreen CreateScreen(FileInfo screenFile)
    {
      return CreateScreen(screenFile, 0, 0, MpeScreenSize.PAL);
    }

    public MpeScreen CreateScreen(FileInfo screenFile, int left, int top, MpeScreenSize size)
    {
      if (screenFile == null || screenFile.Exists == false)
      {
        throw new MpeParserException("Invalid screen file.");
      }
      MpeLog.Debug("Loading controls from [" + screenFile.FullName + "]");
      try
      {
        MpeScreen screen = CreateScreen(left, top, size);
        XPathDocument doc = new XPathDocument(screenFile.FullName);
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iterator = nav.Select("/" + MpeControlType.Screen.ToString());
        if (iterator.MoveNext())
        {
          screen.Load(iterator, this);
          screen.Modified = false;
          return screen;
        }
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        throw new MpeParserException(e.Message);
      }
      throw new MpeParserException("Invalid screen file. Could not parse contents.");
    }

    public MpeScreen CreateScreen()
    {
      return (MpeScreen) CreateControl(MpeControlType.Screen);
    }

    public MpeScreen CreateScreen(int left, int top, MpeScreenSize size)
    {
      MpeScreen screen = CreateScreen();
      screen.Left = left;
      screen.Top = top;
      screen.ScreenSize = size;
      return screen;
    }

    public void SaveScreen(MpeScreen screen, FileInfo screenFile)
    {
      if (screen == null)
      {
        throw new MpeParserException("Invalid screen control.");
      }
      if (screenFile == null)
      {
        throw new MpeParserException("Invalid screen file.");
      }
      XmlTextWriter writer = null;
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlNode node = doc.CreateElement("window");
        doc.AppendChild(node);
        screen.Save(doc, node, this, (MpeScreen) GetControl(MpeControlType.Screen));

        writer = new XmlTextWriter(screenFile.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        doc.WriteTo(writer);
        writer.Close();
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        MpeLog.Error(e);
        throw new MpeParserException(e.Message);
      }
    }

    #endregion
  }

  #region Exception Classes

  public class MpeParserException : Exception
  {
    public MpeParserException(string message) : base(message)
    {
      //
    }

    public MpeParserException(string message, Exception inner) : base(message, inner)
    {
      //
    }
  }

  #endregion
}