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
using System.Data;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace MPLanguageTool
{
  class XmlHandler
  {
    private static string BuildFileName(string languageID)
    {
      const string LangFileName = "strings_";
      const string LangExtension = ".xml";
      string LangDefaultID = "en";
      if (languageID != null)
      {
        LangDefaultID = languageID;
      }
      return AppDomain.CurrentDomain.BaseDirectory + LangFileName + LangDefaultID + LangExtension;
    }

    // Load Original Label to Translate
    public static DataTable Load(string languageID, out Dictionary<string,DataRow> originalMapping)
    {
      originalMapping = new Dictionary<string, DataRow>();
      string xml = BuildFileName(languageID);
      if (!File.Exists(xml))
      {
        if (languageID == null)
        {
          return null;
        }
        return new DataTable();
      }

      DataTable translations = new DataTable();

      DataColumn col0 = new DataColumn("id", Type.GetType("System.String"));
      DataColumn col1 = new DataColumn("Original", Type.GetType("System.String"));
      DataColumn col2 = new DataColumn("Translated", Type.GetType("System.String"));
      DataColumn col3 = new DataColumn("PrefixOriginal", Type.GetType("System.String"));
      DataColumn col4 = new DataColumn("PrefixTranslated", Type.GetType("System.String"));

      translations.Columns.Add(col0);
      translations.Columns.Add(col1);
      translations.Columns.Add(col2);
      translations.Columns.Add(col3);
      translations.Columns.Add(col4);

      
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      if (doc.DocumentElement != null)
      {
        XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Language/Section/String");

        if (nodes != null)
        {
          foreach (XmlNode keyNode in nodes)
          {
            string prefixValue = "";
            string node_id = keyNode.Attributes["id"].Value;

            if (keyNode.Attributes.Count == 2)
            {
              prefixValue = keyNode.Attributes["prefix"].Value;
            }

            DataRow row = translations.NewRow();
            row[0] = node_id;
            row[1] = keyNode.InnerText;
            row[2] = "";
            row[3] = prefixValue;
            row[4] = "";


            translations.Rows.Add(row);
            originalMapping.Add(node_id, row);

          }
        }
      }
      return translations;
    }

    // Load Translations
    public static DataTable Load_Traslation(string languageID, DataTable originalTranslation, Dictionary<string,DataRow> originalMapping)
    {
      string xml = BuildFileName(languageID);
      if (!File.Exists(xml))
      {
        if (languageID == null)
        {
          return null;
        }
        return new DataTable();
      }

      DataTable translations = new DataTable();

      DataColumn col0 = new DataColumn("id", Type.GetType("System.String"));
      DataColumn col1 = new DataColumn("Original", Type.GetType("System.String"));
      DataColumn col2 = new DataColumn("Translated", Type.GetType("System.String"));
      DataColumn col3 = new DataColumn("PrefixOriginal", Type.GetType("System.String"));
      DataColumn col4 = new DataColumn("PrefixTranslated", Type.GetType("System.String"));

      translations.Columns.Add(col0);
      translations.Columns.Add(col1);
      translations.Columns.Add(col2);
      translations.Columns.Add(col3);
      translations.Columns.Add(col4);

      Dictionary<string, DataRow> translationMapping = new Dictionary<string, DataRow>();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      if (doc.DocumentElement != null)
      {
        XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Language/Section/String");

        if (nodes != null)
        {
          foreach (XmlNode keyNode in nodes)
          {
            string prefixValue = "";
            string node_id = keyNode.Attributes["id"].Value;

            if (keyNode.Attributes.Count == 2)
            {
              prefixValue = keyNode.Attributes["prefix"].Value;
            }

            DataRow row = translations.NewRow();
            row[0] = node_id;
            row[1] = keyNode.InnerText;
            row[2] = "";
            row[3] = prefixValue;
            row[4] = "";

            translations.Rows.Add(row);
            translationMapping.Add(node_id.Trim(), row);
          }
        }
      }


            // Hope That indexes was syncronized
      foreach(String key in originalMapping.Keys){
        if (originalMapping.ContainsKey(key) && translationMapping.ContainsKey(key))
        {
          originalMapping[key]["PrefixTranslated"] = translationMapping[key]["PrefixOriginal"].ToString();
          originalMapping[key]["Translated"] = translationMapping[key]["Original"].ToString();
        }
      }

      return originalTranslation;
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
        XmlNode nValue = doc.CreateElement("String");
        nValue.InnerText = translations[key];
        XmlAttribute attr = nValue.OwnerDocument.CreateAttribute("id");

        if (key.EndsWith(PrefixIdentifier()))
        {
          attr.InnerText = key.Substring(0, key.Length - PrefixIdentifier().Length);
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

    public static void Save(string languageID, string LanguageNAME, DataTable translations)
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

      foreach (DataRow row in translations.Rows)
      {
        XmlNode nValue = doc.CreateElement("String");

        // First place id, must be same key as original
        XmlAttribute attr = nValue.OwnerDocument.CreateAttribute("id");
        attr.InnerText = row["id"].ToString();
        nValue.Attributes.Append(attr);


        //attr.InnerText = key.Substring(0, key.Length - PrefixIdentifier().Length);
        //nValue.Attributes.Append(attr);

        if (row["PrefixTranslated"] != null)
        {
          attr = nValue.OwnerDocument.CreateAttribute("prefix");
          attr.InnerText = row["PrefixTranslated"].ToString().Trim();
        }

        nValue.Attributes.Append(attr);


        if (row["Translated"] != null)
        {
          //attr = nValue.OwnerDocument.CreateAttribute("prefix");
          nValue.InnerText = row["Translated"].ToString().Trim();


          //attr.InnerText = key;
          //nValue.Attributes.Append(attr);

          nRoot.AppendChild(nValue);
        }


      }
      doc.Save(xml);
    }

    private static string PrefixIdentifier()
    {
      return "(*)";
    }
  }
}
