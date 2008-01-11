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
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Reflection;
using MediaPortal.Services;

namespace MediaPortal.Webepg.Profile
{
  public class Xml : IDisposable
  {
    // Fields
    //private static string		_rootName = "profile";
    //private static Encoding _encoding = Encoding.UTF8;
    //static XmlDocument			_doc=null;
    //static string						_strFileName="";
    //static bool							_bChanged=false;

    private string _rootName = "profile";
    private Encoding _encoding = Encoding.UTF8;
    XmlDocument _doc = null;
    string _strFileName = "";
    bool _bChanged = false;
    ILog _log;

    /// <summary>
    ///   Initializes a new instance of the Xml class by setting the <see cref="Profile.Name" /> to <see cref="Profile.DefaultName" />. </summary>
    public Xml()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    /// <summary>
    ///   Initializes a new instance of the Xml class by setting the <see cref="Profile.Name" /> to the given file name. </summary>
    /// <param name="fileName">
    ///   The name of the XML file to initialize the <see cref="Profile.Name" /> property with. </param>
    public Xml(string fileName)
    {
      if (_strFileName != fileName)
      {
        Save();
        _doc = null;
      }
      _strFileName = fileName;
    }

    public void Clear()
    {
      _doc = null;
      _bChanged = false;
    }

    /// <summary>
    ///   Retrieves an XMLDocument object based on the XML file (Name). </summary>
    /// <returns>
    ///   The return value is the XMLDocument object based on the file, 
    ///   or null if the file does not exist. </returns>
    private XmlDocument GetXmlDocument()
    {

      if (!File.Exists(_strFileName))
      {
        if (File.Exists(_strFileName + ".bak"))
        {
          XmlDocument docBak = new XmlDocument();
          docBak.Load(_strFileName + ".bak");
          return docBak;
        }
        return null;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(_strFileName);
      if (doc != null && doc.DocumentElement != null && doc.DocumentElement.ChildNodes != null) return doc;
      if (File.Exists(_strFileName + ".bak"))
      {
        doc = new XmlDocument();
        doc.Load(_strFileName + ".bak");
      }
      return doc;
    }

    /// <summary>
    ///   Retrieves the XPath string used for retrieving a section from the XML file. </summary>
    /// <returns>
    ///   An XPath string. </returns>
    /// <seealso cref="GetEntryPath" />
    private string GetSectionsPath(string section)
    {
      return "section[@name=\"" + section + "\"]";
    }

    /// <summary>
    ///   Retrieves the XPath string used for retrieving an entry from the XML file. </summary>
    /// <returns>
    ///   An XPath string. </returns>
    /// <seealso cref="GetSectionsPath" />
    private string GetEntryPath(string entry)
    {
      return "entry[@name=\"" + entry + "\"]";
    }

    /// <summary>
    ///   Sets the value for an entry inside a section. </summary>
    /// <param name="section">
    ///   The name of the section that holds the entry. </param>
    /// <param name="entry">
    ///   The name of the entry where the value will be set. </param>
    /// <param name="value">
    ///   The value to set. If it's null, the entry is removed. </param>
    /// <exception cref="InvalidOperationException"><see cref="Profile.ReadOnly" /> is true. </exception>
    /// <exception cref="InvalidOperationException"><see cref="Profile.Name" /> is null or empty. </exception>
    /// <exception cref="ArgumentNullException">Either section or entry is null. </exception>
    /// <remarks>
    ///   If the XML file does not exist, it is created.
    ///   The <see cref="Profile.Changing" /> event is raised before setting the value.  
    ///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
    ///   returns immediately without setting the value.  After the value has been set, 
    ///   the <see cref="Profile.Changed" /> event is raised. </remarks>
    /// <seealso cref="GetValue" />
    public void Save()
    {
      if (_bChanged)
      {
        lock (typeof(MediaPortal.Webepg.Profile.Xml))
        {
          if (_doc == null) return;
          if (_doc.DocumentElement == null) return;
          if (_doc.ChildNodes.Count == 0) return;
          if (_doc.DocumentElement.ChildNodes == null) return;
          if (!_bChanged) return;
          try
          {
            try
            {
              System.IO.File.Delete(_strFileName + ".bak");
              System.IO.File.Move(_strFileName, _strFileName + ".bak");
            }
            catch (Exception) { }

            using (StreamWriter stream = new StreamWriter(_strFileName, false))
            {
              _doc.Save(stream);
              _doc = null;
              stream.Flush();
              stream.Close();
            }
            _bChanged = false;
          }
          catch (Exception ex)
          {
            _log.Error("Unable to save {0} {1}", ex.Message);
          }
          _doc = null;
        }
      }
    }

    public void SetValue(string section, string entry, object value)
    {
      lock (typeof(Xml))
      {
        // If the value is null, remove the entry
        if (value == null)
        {
          RemoveEntry(section, entry);
          return;
        }

        string valueString = value.ToString();

        // If the file does not exist, use the writer to quickly create it
        if (!File.Exists(_strFileName))
        {
          XmlTextWriter writer = new XmlTextWriter(_strFileName, _encoding);
          writer.Formatting = Formatting.Indented;

          writer.WriteStartDocument();

          writer.WriteStartElement(_rootName);
          writer.WriteStartElement("section");
          writer.WriteAttributeString("name", null, section);
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("name", null, entry);
          if (valueString != "")
            writer.WriteString(valueString);
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.Close();
          _doc = null;
          return;
        }


        if (_doc == null)
        {
          _doc = GetXmlDocument();
        }
        if (_doc == null) return;

        XmlElement root = _doc.DocumentElement;

        // Get the section element and add it if it's not there
        XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
        if (sectionNode == null)
        {
          XmlElement element = _doc.CreateElement("section");
          XmlAttribute attribute = _doc.CreateAttribute("name");
          attribute.Value = section;
          element.Attributes.Append(attribute);
          sectionNode = root.AppendChild(element);
        }

        // Get the entry element and add it if it's not there
        XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
        if (entryNode == null)
        {
          XmlElement element = _doc.CreateElement("entry");
          XmlAttribute attribute = _doc.CreateAttribute("name");
          attribute.Value = entry;
          element.Attributes.Append(attribute);
          entryNode = sectionNode.AppendChild(element);
        }

        // Add the value and save the file
        //if (valueString != "")
        entryNode.InnerText = valueString;
        _bChanged = true;
      }
    }

    public string GetValueAsString(string section, string entry, string strDefault)
    {
      string strValue = (string)GetValue(section, entry);
      if (strValue == null) return strDefault;
      if (strValue.Length == 0) return strDefault;
      return strValue;
    }

    public bool GetValueAsBool(string section, string entry, bool bDefault)
    {
      string strValue = (string)GetValue(section, entry);
      if (strValue == null) return bDefault;
      if (strValue.Length == 0) return bDefault;
      if (strValue == "yes") return true;
      return false;
    }
    public int GetValueAsInt(string section, string entry, int iDefault)
    {
      string strValue = (string)GetValue(section, entry);
      if (strValue == null) return iDefault;
      if (strValue.Length == 0) return iDefault;
      try
      {
        int iRet = System.Int32.Parse(strValue);
        return iRet;
      }
      catch (Exception)
      {
      }
      return iDefault;
    }

    public float GetValueAsFloat(string section, string entry, float fDefault)
    {
      string strValue = (string)GetValue(section, entry);
      if (strValue == null) return fDefault;
      if (strValue.Length == 0) return fDefault;
      try
      {
        float fRet = (float)System.Double.Parse(strValue);
        return fRet;
      }
      catch (Exception)
      {
      }
      return fDefault;
    }
    public void SetValueAsBool(string section, string entry, bool bValue)
    {
      string strValue = "yes";
      if (!bValue) strValue = "no";
      SetValue(section, entry, strValue);
    }
    /// <summary>
    ///   Retrieves the value of an entry inside a section. </summary>
    /// <param name="section">
    ///   The name of the section that holds the entry with the value. </param>
    /// <param name="entry">
    ///   The name of the entry where the value is stored. </param>
    /// <returns>
    ///   The return value is the entry's value, or null if the entry does not exist. </returns>
    /// <exception cref="ArgumentNullException">Either section or entry is null. </exception>
    /// <seealso cref="SetValue" />
    /// <seealso cref="Profile.HasEntry" />

    public object GetValue(string section, string entry)
    {
      lock (typeof(Xml))
      {

        try
        {
          if (_doc == null)
            _doc = GetXmlDocument();
          if (_doc == null) return null;

          XmlElement root = _doc.DocumentElement;

          XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
          if (entryNode == null) return null;
          return entryNode.InnerText;
        }
        catch
        {
          return null;
        }
      }
    }

    /// <summary>
    ///   Removes an entry from a section. </summary>
    /// <param name="section">
    ///   The name of the section that holds the entry. </param>
    /// <param name="entry">
    ///   The name of the entry to remove. </param>
    /// <exception cref="InvalidOperationException"><see cref="Profile.ReadOnly" /> is true. </exception>
    /// <exception cref="ArgumentNullException">Either section or entry is null. </exception>
    /// <remarks>
    ///   The <see cref="Profile.Changing" /> event is raised before removing the entry.  
    ///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
    ///   returns immediately without removing the entry.  After the entry has been removed, 
    ///   the <see cref="Profile.Changed" /> event is raised. </remarks>
    /// <seealso cref="RemoveSection" />
    public void RemoveEntry(string section, string entry)
    {

      // Verify the file exists
      if (_doc == null)
      {
        _doc = GetXmlDocument();
        if (_doc == null) return;
      }

      // Get the entry's node, if it exists
      XmlElement root = _doc.DocumentElement;
      XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
      if (entryNode == null)
        return;


      entryNode.ParentNode.RemoveChild(entryNode);
      _bChanged = true;
    }

    /// <summary>
    ///   Removes a section. </summary>
    /// <param name="section">
    ///   The name of the section to remove. </param>
    /// <exception cref="InvalidOperationException"><see cref="Profile.ReadOnly" /> is true. </exception>
    /// <exception cref="ArgumentNullException">section is null. </exception>
    /// <remarks>
    ///   The <see cref="Profile.Changing" /> event is raised before removing the section.  
    ///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
    ///   returns immediately without removing the section.  After the section has been removed, 
    ///   the <see cref="Profile.Changed" /> event is raised. </remarks>
    /// <seealso cref="RemoveEntry" />
    public void RemoveSection(string section)
    {

      // Verify the file exists
      if (_doc == null)
      {
        _doc = GetXmlDocument();
        if (_doc == null)
          return;
      }
      // Get the section's node, if it exists
      XmlElement root = _doc.DocumentElement;
      XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
      if (sectionNode == null)
        return;

      root.RemoveChild(sectionNode);
      _bChanged = true;
    }
    #region IDisposable Members

    public void Dispose()
    {
      Save();
    }

    #endregion
  }
}
