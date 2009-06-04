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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace MPLanguageTool
{
  class XmlHandler
  {
    private static string MainNodeSelection = string.Empty;
    private static string SingleNodeSelection = string.Empty;
    private static string Field = string.Empty;

    private static string BuildFileName(string languageID)
    {
      string LangDefaultID = "en";
      string LangPrefix = string.Empty;
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MovingPictures:
          LangDefaultID = "en-US";
          break;
        case frmMain.StringsType.MediaPortal:
          LangPrefix = "strings_";
          break;
      }
      if (languageID != null)
      {
        LangDefaultID = languageID;
      }
      return frmMain.languagePath + "\\" + LangPrefix + LangDefaultID + ".xml";
    }

    public static void InitializeXmlValues()
    {
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MediaPortal:
          MainNodeSelection = "/Language/Section";
          SingleNodeSelection = "String";
          Field = "id";
          break;
        case frmMain.StringsType.MovingPictures:
          MainNodeSelection = "/strings";
          SingleNodeSelection = "string";
          Field = "Field";
          break;
      }
    }

    // Load Original Label to Translate
    public static DataTable Load(string languageID, out Dictionary<string, DataRow> originalMapping)
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
        XmlNodeList nodes = doc.DocumentElement.SelectNodes(MainNodeSelection + "//" + SingleNodeSelection);

        if (nodes != null)
        {
          foreach (XmlNode keyNode in nodes)
          {
            string prefixValue = "";
            string node_id = keyNode.Attributes[Field].Value;

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
    public static DataTable Load_Traslation(string languageID, DataTable originalTranslation, Dictionary<string, DataRow> originalMapping)
    {
      string xml = BuildFileName(languageID);
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

      if (!File.Exists(xml))
      {
        if (languageID == null)
        {
          return null;
        }
        //
        // Create a empty xml file as placeholder
        //
        Save(languageID, "", translations);
      }

      Dictionary<string, DataRow> translationMapping = new Dictionary<string, DataRow>();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      if (doc.DocumentElement != null)
      {
        XmlNodeList nodes = doc.DocumentElement.SelectNodes(MainNodeSelection + "//" + SingleNodeSelection);

        if (nodes != null)
        {
          foreach (XmlNode keyNode in nodes)
          {
            string prefixValue = "";
            string node_id = keyNode.Attributes[Field].Value;

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

            //
            // If strings_XX.xml trow an expcetion, uncomment this line to find the offending line
            //
            //MessageBox.Show("Loading xml strings (node_id = " + node_id + ")");
            //
            translations.Rows.Add(row);
            translationMapping.Add(node_id.Trim(), row);
          }
        }
      }


      // Hope That indexes was syncronized
      foreach (String key in originalMapping.Keys)
      {
        if (originalMapping.ContainsKey(key) && translationMapping.ContainsKey(key))
        {
          originalMapping[key]["PrefixTranslated"] = translationMapping[key]["PrefixOriginal"].ToString();
          originalMapping[key]["Translated"] = translationMapping[key]["Original"].ToString();
        }
      }

      return originalTranslation;
    }

    public static void Save(string languageID, string LanguageNAME, DataTable translations)
    {
      string xml = BuildFileName(languageID);
      StreamWriter writer = new StreamWriter(xml, false, Encoding.UTF8);
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MediaPortal:
          writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
          writer.Write("<Language name=\"" + LanguageNAME + "\" characters=\"255\">\n");
          writer.Write("  <Section name=\"unmapped\">\n");
          writer.Write("  </Section>\n");
          writer.Write("</Language>\n");
          break;
        case frmMain.StringsType.MovingPictures:
          writer.Write("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n");
          writer.Write("<!-- Moving Pictures translation file -->\n");
          writer.Write("<!-- " + LanguageNAME + " -->\n");
          writer.Write("<!-- Note: English is the fallback for any strings not found in other languages -->\n");
          writer.Write("  <strings>\n");
          writer.Write("  </strings>\n");
          break;
      }

      writer.Close();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      XmlNode nRoot = doc.SelectSingleNode(MainNodeSelection);

      foreach (DataRow row in translations.Rows)
      {
        XmlNode nValue = doc.CreateElement(SingleNodeSelection);

        // First place id, must be same key as original
        XmlAttribute attr = nValue.OwnerDocument.CreateAttribute(Field);
        attr.InnerText = row["id"].ToString();
        nValue.Attributes.Append(attr);

        if (!String.IsNullOrEmpty(row["PrefixTranslated"].ToString()))
        {
          attr = nValue.OwnerDocument.CreateAttribute("prefix");
          attr.InnerText = row["PrefixTranslated"].ToString();
        }
        nValue.Attributes.Append(attr);

        if (!String.IsNullOrEmpty(row["Translated"].ToString()))
        {
          nValue.InnerText = row["Translated"].ToString();
          nRoot.AppendChild(nValue);
        }

      }
      doc.Save(xml);
    }
  }
}
