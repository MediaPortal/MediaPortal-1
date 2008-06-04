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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Windows.Forms;

namespace MPLanguageTool
{
  class XmlHandler
  {
    private static string BuildFileName(string languageID)
    {
      string LangFileName = "strings_";
      string LangExtension = ".xml";
      string LangDefaultID = "en";
      if (languageID != null)
      {
        LangDefaultID = languageID;
      }
      return AppDomain.CurrentDomain.BaseDirectory + LangFileName + LangDefaultID + LangExtension;
    }

    public static NameValueCollection Load(string languageID)
    {
      string xml = BuildFileName(languageID);
      if (!File.Exists(xml))
      {
        if (languageID == null)
          return null;
        else
          return new NameValueCollection();
      }
      NameValueCollection translations = new NameValueCollection();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Language/Section/String");
      bool first = true;
      foreach (XmlNode keyNode in nodes)
      {
        if (first)
        {
          translations.Add("-", keyNode.Attributes["prefix"].Value);
          first = false;
        }
        translations.Add(keyNode.Attributes["id"].Value, keyNode.InnerText);
      }
      return translations;
    }

    public static void Save(string languageID, NameValueCollection translations)
    {
      //
      // Need to find a way to add prefix when needed !!!
      //
      // This function need a complete rewrite to handle MP strings xml format !!!
      //
      MessageBox.Show("Save function for strings_*.xml will be available soon.", "Work in progress...");
      return;


      string xml = BuildFileName(languageID);
      StreamWriter writer = new StreamWriter(xml, false, Encoding.UTF8);
      writer.Close();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      XmlNode nRoot = doc.SelectSingleNode("/Language/Section/String");
      foreach (string key in translations.Keys)
      {
        if (translations[key] == null) continue;
        XmlNode nValue = doc.CreateElement("value");
        nValue.InnerText = translations[key];
        XmlNode nKey = doc.CreateElement("data");
        XmlAttribute attr = nKey.OwnerDocument.CreateAttribute("name");
        attr.InnerText = key;
        nKey.Attributes.Append(attr);
        attr = nKey.OwnerDocument.CreateAttribute("xml:space");
        attr.InnerText = "preserve";
        nKey.Attributes.Append(attr);
        nKey.AppendChild(nValue);
        nRoot.AppendChild(nKey);
      }
      doc.Save(xml);

    }
  }
}
