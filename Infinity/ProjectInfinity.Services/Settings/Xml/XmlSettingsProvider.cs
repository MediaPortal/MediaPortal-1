
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.IO.IsolatedStorage;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Settings
{
  public class XmlSettingsProvider
  {
    XmlDocument document = null;
    bool modified = false;
    string filename = null;

    public XmlSettingsProvider(string xmlfilename)
    {
      filename = xmlfilename;
      string fullFileName = String.Format(@"{0}\MediaPortal Infinity\Config\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),xmlfilename);;
      document = new XmlDocument();
      if (File.Exists(fullFileName))
      {
        document.Load(fullFileName);
        if (document.DocumentElement == null) document = null;
      }
      else if (File.Exists(fullFileName + ".bak"))
      {
        document.Load(fullFileName + ".bak");
        if (document.DocumentElement == null) document = null;
      }
      if (document == null)
        document = new XmlDocument();

    }

    public string FileName
    {
      get { return filename; }
    }

    public string GetValue(string section, string entry, SettingScope scope)
    {
      if (document == null) return null;

      XmlElement root = document.DocumentElement;
      if (root == null) return null;
      XmlNode entryNode;
      if (scope == SettingScope.User) entryNode = root.SelectSingleNode(GetSectionPath(section) + "/" + GetScopePath(scope.ToString()) + "/" + GetUserPath(Environment.UserName) + "/" + GetEntryPath(entry));
      else entryNode = root.SelectSingleNode(GetSectionPath(section) + "/" + GetScopePath(scope.ToString()) + "/" + GetEntryPath(entry));
      if (entryNode == null) return null;
      return entryNode.InnerText;
    }

    public void Save()
    {
      ILogger log = ServiceScope.Get<ILogger>();
      log.Debug("Saving({0},{1})", filename, modified.ToString());
      if (!modified) return;
      Directory.CreateDirectory(String.Format(@"{0}\MediaPortal Infinity\Config\", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))) ;
      string fullFilename = String.Format(@"{0}\MediaPortal Infinity\Config\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),filename);
      log.Debug("Saving {0}", fullFilename);
      if (document == null) return;
      if (document.DocumentElement == null) return;
      if (document.ChildNodes.Count == 0) return;
      if (document.DocumentElement.ChildNodes == null) return;
      try
      {
        if (File.Exists(fullFilename + ".bak")) File.Delete(fullFilename + ".bak");
        if (File.Exists(fullFilename)) File.Move(fullFilename, fullFilename + ".bak");
      }
      
      catch (Exception) { }

      using (StreamWriter stream = new StreamWriter(fullFilename, false))
      {
        document.Save(stream);
        stream.Flush();
        stream.Close();
      }
      modified = false;
    }

    public void SetValue(string section, string entry, string value, SettingScope scope)
    {
      // If the value is null, remove the entry
      if (value == null)
      {
        RemoveEntry(section, entry);
        return;
      }

      string valueString = value.ToString();

      if (document.DocumentElement == null)
      {
        XmlElement node = document.CreateElement("Configuration");
        document.AppendChild(node);
      }
      XmlElement root = document.DocumentElement;
      // Get the section element and add it if it's not there
      XmlNode sectionNode = root.SelectSingleNode("Section[@name=\"" + section + "\"]");
      if (sectionNode == null)
      {
        XmlElement element = document.CreateElement("Section");
        XmlAttribute attribute = document.CreateAttribute("name");
        attribute.Value = section;
        element.Attributes.Append(attribute);
        sectionNode = root.AppendChild(element);
      }
      // Get the section element and add it if it's not there
      XmlNode scopeSectionNode = sectionNode.SelectSingleNode("Scope[@value=\"" + scope.ToString() + "\"]");
      if (scopeSectionNode == null)
      {
        XmlElement element = document.CreateElement("Scope");
        XmlAttribute attribute = document.CreateAttribute("value");
        attribute.Value = scope.ToString();
        element.Attributes.Append(attribute);
        scopeSectionNode = sectionNode.AppendChild(element);
      }
      if (scope == SettingScope.User)
      {
        XmlNode userNode = scopeSectionNode.SelectSingleNode("User[@name=\"" + Environment.UserName + "\"]");
        if (userNode == null)
        {
          XmlElement element = document.CreateElement("User");
          XmlAttribute attribute = document.CreateAttribute("name");
          attribute.Value = Environment.UserName;
          element.Attributes.Append(attribute);
          userNode = scopeSectionNode.AppendChild(element);
        }
      }
      // Get the entry element and add it if it's not there
      XmlNode entryNode;
      if (scope == SettingScope.User)
      {
        XmlNode userNode = scopeSectionNode.SelectSingleNode("User[@name=\"" + Environment.UserName + "\"]");
        entryNode = userNode.SelectSingleNode("Setting[@name=\"" + entry + "\"]");
      }
      else entryNode = scopeSectionNode.SelectSingleNode("Setting[@name=\"" + entry + "\"]");

      if (entryNode == null)
      {
        XmlElement element = document.CreateElement("Setting");
        XmlAttribute attribute = document.CreateAttribute("name");
        attribute.Value = entry;
        element.Attributes.Append(attribute);
        if (scope == SettingScope.Global) entryNode = scopeSectionNode.AppendChild(element);
        else
        {
          XmlNode userNode = scopeSectionNode.SelectSingleNode("User[@name=\"" + Environment.UserName + "\"]");
          entryNode = userNode.AppendChild(element);
        }
      }
      entryNode.InnerText = valueString;
      modified = true;
    }

    public void RemoveEntry(string section, string entry)
    {
      //todo
    }

    private string GetSectionPath(string section)
    {
      return "Section[@name=\"" + section + "\"]";
    }
    private string GetEntryPath(string entry)
    {
      return "Setting[@name=\"" + entry + "\"]";
    }
    private string GetScopePath(string scope)
    {
      return "Scope[@value=\"" + scope + "\"]";
    }
    private string GetUserPath(string user)
    {
      return "User[@name=\"" + Environment.UserName + "\"]";
    }
  }
}