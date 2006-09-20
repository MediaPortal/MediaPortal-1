using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace MediaPortal.Profile
{
  public class XmlSettingsProvider : ISettingsProvider, ISettingsPrefetchable
  {
    XmlDocument document = null;
    bool modified = false;
    string filename = null;

    public XmlSettingsProvider(string xmlFileName)
    {
      filename = xmlFileName;
      document = new XmlDocument();
      if (File.Exists(filename))
      {
        document.Load(filename);
        if (document.DocumentElement == null) document = null;
      }
      else if (File.Exists(filename + ".bak"))
      {
        document.Load(filename + ".bak");
        if (document.DocumentElement == null) document = null;
      }
      if (document == null)
        document = new XmlDocument();

    }
    public string FileName
    {
      get { return filename; }
    }

    public object GetValue(string section, string entry)
    {
      if (document == null) return null;

      XmlElement root = document.DocumentElement;
      if (root == null) return null;
      XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
      if (entryNode == null) return null;
      return entryNode.InnerText;
    }

    public void Save()
    {
      if (!modified) return;
      if (document == null) return;
      if (document.DocumentElement == null) return;
      if (document.ChildNodes.Count == 0) return;
      if (document.DocumentElement.ChildNodes == null) return;
      try
      {
        if (File.Exists(filename + ".bak")) File.Delete(filename + ".bak");
        if (File.Exists(filename)) File.Move(filename, filename + ".bak");
      }
      catch (Exception) { }

      using (StreamWriter stream = new StreamWriter(filename, false))
      {
        document.Save(stream);
        stream.Flush();
        stream.Close();
      }
      modified = false;
    }
    public void SetValue(string section, string entry, object value)
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
        XmlElement node = document.CreateElement("profile");
        document.AppendChild(node);
      }
      XmlElement root = document.DocumentElement;
      // Get the section element and add it if it's not there
      XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
      if (sectionNode == null)
      {
        XmlElement element = document.CreateElement("section");
        XmlAttribute attribute = document.CreateAttribute("name");
        attribute.Value = section;
        element.Attributes.Append(attribute);
        sectionNode = root.AppendChild(element);
      }

      // Get the entry element and add it if it's not there
      XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
      if (entryNode == null)
      {
        XmlElement element = document.CreateElement("entry");
        XmlAttribute attribute = document.CreateAttribute("name");
        attribute.Value = entry;
        element.Attributes.Append(attribute);
        entryNode = sectionNode.AppendChild(element);
      }
      entryNode.InnerText = valueString;
      modified = true;
    }

    public void RemoveEntry(string section, string entry)
    {

      // Verify the file exists
      if (document == null) return;
      // -- Unecessary : if (document.DocumentElement == null) return;
      // Get the entry's node, if it exists
      // -- Unecessary : XmlElement root = document.DocumentElement;
      string xpathString = GetSectionsPath(section) + "/" + GetEntryPath(entry);

      XmlNode entryNode = document.DocumentElement.SelectSingleNode(xpathString);
      //entryNode = document.DocumentElement.SelectSingleNode(GetSectionsPath(section));

      if (entryNode == null)
        return;
      entryNode.ParentNode.RemoveChild(entryNode);
      modified = true;
    }
    private string GetSectionsPath(string section)
    {
      return "section[@name=\"" + section + "\"]";
    }
    private string GetEntryPath(string entry)
    {
      return "entry[@name=\"" + entry + "\"]";
    }

    public void Prefetch(RememberDelegate function)
    {
      string section = "", entry = "";
      object value;

      if (!File.Exists(filename))
        return;

      using (XmlReader reader = XmlReader.Create(filename, GetReaderSettings()))
      {
        while (reader.Read())
        {
          if (IsAtSection(reader))
          {
            section = reader.GetAttribute("name");
          }
          else if (IsAtEntry(reader))
          {
            entry = reader.GetAttribute("name");
            value = reader.ReadInnerXml();
            function.Invoke(section, entry, value);
          }
        }
      }
    }

    private static XmlReaderSettings GetReaderSettings()
    {
      XmlReaderSettings settings = new XmlReaderSettings();
      settings.IgnoreComments = true;
      settings.IgnoreWhitespace = true;
      settings.IgnoreProcessingInstructions = true;
      return settings;
    }

    private static bool IsAtEntry(XmlReader reader)
    {
      return reader.LocalName == "entry" && reader.Depth == 2 && reader.NodeType == XmlNodeType.Element;
    }

    private static bool IsAtSection(XmlReader reader)
    {
      return reader.LocalName == "section" && reader.Depth == 1 && reader.NodeType == XmlNodeType.Element;
    }
  }
}
