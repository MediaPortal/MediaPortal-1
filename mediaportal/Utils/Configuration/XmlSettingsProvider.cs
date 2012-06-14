#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MediaPortal.Utils;

namespace MediaPortal.Profile
{
  public class XmlSettingsProvider : ISettingsProvider, ISettingsPrefetchable
  {
    private XmlDocument document = null;
    private bool modified = false;
    private string filename = null;

    public XmlSettingsProvider(string xmlFileName)
    {
      filename = xmlFileName;
      document = new XmlDocument();
      //bool docLoaded = false;
      try
      {
        document.Load(filename);
        if (document.DocumentElement == null)
        {
          document = null;
        }
      }
      catch (Exception)
      {
        if (File.Exists(filename + ".bak"))
        {
          document.Load(filename + ".bak");
        }
        if (File.Exists(filename + ".xml"))
        {
          File.Delete(filename + ".xml");
        }
        if (document.DocumentElement == null)
        {
          document = null;
        }
      }
      if (document == null)
      {
        document = new XmlDocument();
      }
    }

    public string FileName
    {
      get { return filename; }
    }

    public object GetValue(string section, string entry)
    {
      if (document == null)
      {
        return null;
      }

      XmlElement root = document.DocumentElement;
      if (root == null)
      {
        return null;
      }
      //XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));      
      XmlNode entryNode = SelectSingleNodeFast(root, GetSectionsPath(section) + "/" + GetEntryPath(entry));
      if (entryNode == null)
      {
        return null;
      }
      return entryNode.InnerText;
    }

    /// <summary>
    /// Returns true if the specified settings section exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="section"></param>
    /// <returns></returns>
    public bool HasSection<T>(string section)
    {
      if (document == null)
      {
        return false;
      }

      XmlElement root = document.DocumentElement;
      if (root == null)
      {
        return false;
      }

      //  Select the section node.
      XmlNode node = root.SelectSingleNode(GetSectionsPath(section));

      return (node != null);
    }

    /// <summary>
    /// Get all of the settings entries in the specified section.
    /// </summary>
    /// <param name="section">Specified the name of the section to retrieve.</param>
    /// <returns>A dictionary of all entries; key is the the "name" attribute.</returns>
    public IDictionary<string,T> GetSection<T>(string section)
    {
      if (document == null)
      {
        return null;
      }

      XmlElement root = document.DocumentElement;
      if (root == null)
      {
        return null;
      }

      //  Select all of the entry nodes in the section.
      XmlNodeList nodes = root.SelectNodes(GetSectionsPath(section) + "/entry");

      if (nodes == null)
        return null;

      IDictionary<string, T> entries = new Dictionary<string, T>();
      IEnumerator enumerator = nodes.GetEnumerator();
      if (enumerator != null)
      {
        while (enumerator.MoveNext())
        {
          string key = ((XmlNode)enumerator.Current).Attributes["name"].Value;
          T value = (T)Convert.ChangeType(((XmlNode)enumerator.Current).InnerText, typeof(T));
          entries.Add(key, value);
        }
      }
      return entries;
    }

    public void Save()
    {
      if (!modified)
      {
        return;
      }
      if (document == null)
      {
        return;
      }
      if (document.DocumentElement == null)
      {
        return;
      }
      if (document.ChildNodes.Count == 0)
      {
        return;
      }
      if (document.DocumentElement.ChildNodes == null)
      {
        return;
      }
      try
      {
        if (File.Exists(filename + ".bak"))
        {
          File.Delete(filename + ".bak");
        }
        if (File.Exists(filename))
        {
          File.Move(filename, filename + ".bak");
        }
      }
      catch (Exception) {}

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
      //XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
      XmlNode sectionNode = SelectSingleNodeFast(root, GetSectionsPath(section));
      if (sectionNode == null)
      {
        XmlElement element = document.CreateElement("section");
        XmlAttribute attribute = document.CreateAttribute("name");
        attribute.Value = section;
        element.Attributes.Append(attribute);
        sectionNode = root.AppendChild(element);
      }

      // Get the entry element and add it if it's not there
      //XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
      XmlNode entryNode = SelectSingleNodeFast(sectionNode, GetEntryPath(entry));
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
      if (document == null)
      {
        return;
      }
      // -- Unecessary : if (document.DocumentElement == null) return;
      // Get the entry's node, if it exists
      // -- Unecessary : XmlElement root = document.DocumentElement;
      string xpathString = GetSectionsPath(section) + "/" + GetEntryPath(entry);

      XmlNode entryNode = document.DocumentElement.SelectSingleNode(xpathString);
      //entryNode = document.DocumentElement.SelectSingleNode(GetSectionsPath(section));

      if (entryNode == null)
      {
        return;
      }
      entryNode.ParentNode.RemoveChild(entryNode);
      modified = true;
    }

    /// <summary>
    /// Moves an entry from the 'from' section to the 'to' section
    /// </summary>
    /// <param name="fromSection"></param>
    /// <param name="toSection"></param>
    /// <param name="entry"></param>
    public void MoveEntry(string fromSection, string toSection, string entry)
    {
      object value = GetValue(fromSection, entry);

      if (value == null)
        return;

      SetValue(toSection, entry, value);

      RemoveEntry(fromSection, entry);
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
      {
        return;
      }

      using (XmlReader reader = XmlReader.Create(filename, GetReaderSettings()))
      {
        try
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
              value = reader.ReadString();
              function.Invoke(section, entry, value);
            }
          }
        }
        catch
        {
          return;
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

    private static XmlNode SelectSingleNodeFast(XmlNode node, string xpath)
    {
      //NOTE: this is code-duplication from Util.cs, but I don't want to introduce new dependencies
      // XmlNode.SelectSingleNode finds all occurances as oppossed to a single one, this causes huge perf issues (about 50% of control creation according to dotTrace)
      XmlNodeList nodes = node.SelectNodes(xpath);

      if (nodes == null)
        return null;

      IEnumerator enumerator = nodes.GetEnumerator();
      if (enumerator != null && enumerator.MoveNext())
      {
        return (XmlNode)enumerator.Current;
      }
      return null; //nothing found
    }
  }
}