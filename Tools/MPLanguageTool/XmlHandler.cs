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
      string node_id;
      foreach (XmlNode keyNode in nodes)
      {
        if (first)
        {
          translations.Add("**", keyNode.Attributes["prefix"].Value);
          first = false;
        }
        if (keyNode.Attributes.Count == 2)
          node_id = "**";
        else
          node_id = "";
        translations.Add(keyNode.Attributes["id"].Value + node_id, keyNode.InnerText);
      }
      return translations;
    }

    public static void Save(string languageID, string LanguageNAME, NameValueCollection translations)
    {
      string xml = BuildFileName(languageID);
      StreamWriter writer = new StreamWriter(xml, false, Encoding.UTF8);
      writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
      writer.Write("<Language name=\"" + LanguageNAME + "\" characters=\"255\">\n");
      writer.Write("  <Section name=\"unmapped\">\n");
      writer.Write("  </Section>\n");
      writer.Write("</Language>\n");
      writer.Close();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      XmlNode nRoot = doc.SelectSingleNode("/Language/Section");
      bool isfirst = true;
      string prefix = "";
      foreach (string key in translations.Keys)
      {
        if (isfirst)
        {
          prefix = translations[key];
          isfirst = false;
          continue;
        }
        if (translations[key] == null) continue;
        XmlNode nValue = doc.CreateElement("value");
        nValue.InnerText = translations[key];
        XmlAttribute attr = nValue.OwnerDocument.CreateAttribute("id");
        if (key.EndsWith("**"))
        {
          attr.InnerText = key.Substring(0, key.Length - 2);
          nValue.Attributes.Append(attr);
          attr = nValue.OwnerDocument.CreateAttribute("prefix");
          attr.InnerText = prefix;
          nValue.Attributes.Append(attr);
        }
        else
        {
          attr.InnerText = key;
          nValue.Attributes.Append(attr);
        }
        nRoot.AppendChild(nValue);
      }
      doc.Save(xml);

    }
  }
}
