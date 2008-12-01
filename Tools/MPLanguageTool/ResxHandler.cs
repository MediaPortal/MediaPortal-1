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

using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.IO;

namespace MPLanguageTool
{
  class ResxHandler
  {
    private static string BuildFileName(string languageID)
    {
      if (languageID == null)
      {
        return frmMain.languagePath + "\\MediaPortal.DeployTool.resx";
      }
      return frmMain.languagePath + "\\MediaPortal.DeployTool." + languageID + ".resx";
    }

    public static NameValueCollection Load(string languageID)
    {
      string xml = BuildFileName(languageID);
      if (!File.Exists(xml))
      {
        if (languageID == null)
        {
          return null;
        }
        return new NameValueCollection();
      }
      NameValueCollection translations = new NameValueCollection();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      XmlNodeList nodes = doc.SelectNodes("/root/data");
      if (nodes != null)
      {
        foreach (XmlNode keyNode in nodes)
          translations.Add(keyNode.Attributes["name"].Value, keyNode.SelectSingleNode("value").InnerText);
      }
      return translations;
    }

    public static void Save(string languageID, NameValueCollection translations)
    {
      string stub = Resource1.ResourceManager.GetString("ResxTemplate");
      string xml = BuildFileName(languageID);
      StreamWriter writer = new StreamWriter(xml, false, Encoding.UTF8);
      writer.Write(stub);
      writer.Close();
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);
      XmlNode nRoot = doc.SelectSingleNode("/root");
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
