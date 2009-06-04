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
    private static string Prefix = string.Empty;

    public static string BuildFileName(string languageID, bool ReturnFullPath)
    {
      string LangDefaultID = "en";
      string LangPrefix = string.Empty;
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MovingPictures:
        case frmMain.StringsType.TvSeries:
          LangDefaultID = "en-US";
          break;
        case frmMain.StringsType.MediaPortal_1:
        case frmMain.StringsType.MpTagThat:
          LangPrefix = "strings_";
          break;
      }
      if (languageID != null)
      {
        LangDefaultID = languageID;
      }
      return ReturnFullPath
               ? frmMain.languagePath + "\\" + LangPrefix + LangDefaultID + ".xml"
               : LangPrefix + LangDefaultID + ".xml";
    }

    public static void InitializeXmlValues()
    {
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MediaPortal_1:
          MainNodeSelection = "/Language/Section";
          SingleNodeSelection = "String";
          Field = "id";
          Prefix = "prefix";
          break;
        case frmMain.StringsType.MovingPictures:
          MainNodeSelection = "/strings";
          SingleNodeSelection = "string";
          Field = "Field";
          break;
        case frmMain.StringsType.TvSeries:
          MainNodeSelection = "/strings";
          SingleNodeSelection = "string";
          Field = "Field";
          Prefix = "Original";
          break;
      }
    }

    public static void InitializeXmlValues(string Section)
    {
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MpTagThat:
        case frmMain.StringsType.MediaPortal_II:
          MainNodeSelection = "/Language/Section[@name='" + Section + "']";
          SingleNodeSelection = "String";
          Field = "id";
          break;
      }
    }

    // Load Original Label to Translate
    public static DataTable Load(string languageID, out Dictionary<string, DataRow> originalMapping)
    {
      originalMapping = new Dictionary<string, DataRow>();
      string xml = BuildFileName(languageID, true);
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
              prefixValue = keyNode.Attributes[Prefix].Value;
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
      string xml = BuildFileName(languageID, true);
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
              prefixValue = keyNode.Attributes[Prefix].Value;
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

    // Get list of sections
    public static List<string> ListSections(string strSection, string attribute)
    {
      string xml = BuildFileName(null, true);
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);

      List<string> Sections = new List<string>();

      if (doc.DocumentElement != null)
      {
        XmlNodeList nodes = doc.DocumentElement.SelectNodes(strSection);

        if (nodes != null)
        {
          foreach (XmlNode keyNode in nodes)
          {
            if (keyNode.Attributes.Count > 0)
            {
              Sections.Add(keyNode.Attributes[attribute].Value);
            }
          }
        }
      }
      return Sections;
    }

    // Save file
    public static void Save(string languageID, string LanguageNAME, DataTable translations)
    {
      string xml = BuildFileName(languageID, true);
      StreamWriter writer = new StreamWriter(xml, false, Encoding.UTF8);
      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MediaPortal_1:
          writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
          writer.Write("<Language name=\"" + LanguageNAME + "\" characters=\"255\">\n");
          writer.Write("  <Section name=\"unmapped\">\n");
          writer.Write("  </Section>\n");
          writer.Write("</Language>\n");
          break;
        case frmMain.StringsType.MpTagThat:
          writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
          writer.Write("<!-- MpTagThat translation file -->\n");
          writer.Write("<Section name=\"main\">");
          writer.Write("</Section>\n");
          break;
        case frmMain.StringsType.MovingPictures:
          writer.Write("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n");
          writer.Write("<!-- MP-TV-Series translation file -->\n");
          writer.Write("<!-- " + LanguageNAME + " -->\n");
          writer.Write("<!-- Note: English is the fallback for any strings not found in other languages -->\n");
          writer.Write("  <strings>\n");
          writer.Write("  </strings>\n");
          break;
        case frmMain.StringsType.TvSeries:
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
          attr = nValue.OwnerDocument.CreateAttribute(Prefix);
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
