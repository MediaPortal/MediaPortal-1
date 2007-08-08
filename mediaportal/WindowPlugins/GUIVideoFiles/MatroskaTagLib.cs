using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace MediaPortal.GUI.Video
{
  class MatroskaTagInfo
  {
    public string title;
    public string description;
    public string genre;
  }
  class MatroskaTagHandler
  {
    #region Private members
    private static XmlNode AddSimpleTag(string tagName, string value, XmlDocument doc)
    {
      XmlNode rootNode = doc.CreateElement("SimpleTag");
      XmlNode nameNode = doc.CreateElement("name");
      nameNode.InnerText = tagName;
      XmlNode valueNode = doc.CreateElement("value");
      valueNode.InnerText = value;
      rootNode.AppendChild(nameNode);
      rootNode.AppendChild(valueNode);
      return rootNode;
    }
    #endregion

    #region Public members
    public static MatroskaTagInfo Fetch(string filename)
    {
      if (!File.Exists(filename))
        return null;
      MatroskaTagInfo info = new MatroskaTagInfo();
      XmlDocument doc = new XmlDocument();
      doc.Load(filename);
      XmlNodeList simpleTags = doc.SelectNodes("/tags/tag/SimpleTag");
      foreach (XmlNode simpleTag in simpleTags)
      {
        string tagName = simpleTag.ChildNodes[0].InnerText;
        switch (tagName)
        {
          case "TITLE":
            info.title = simpleTag.ChildNodes[1].InnerText;
            break;
          case "COMMENT":
            info.description = simpleTag.ChildNodes[1].InnerText;
            break;
          case "GENRE":
            info.genre = simpleTag.ChildNodes[1].InnerText;
            break;
        }
      }
      return info;
    }
    public static void Persist(string filename, MatroskaTagInfo taginfo)
    {
      if (!Directory.Exists(Path.GetDirectoryName(filename)))
        Directory.CreateDirectory(Path.GetDirectoryName(filename));
      XmlDocument doc = new XmlDocument();
      XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      XmlNode tagsNode = doc.CreateElement("tags");
      XmlNode tagNode = doc.CreateElement("tag");
      tagNode.AppendChild(AddSimpleTag("TITLE", taginfo.title, doc));
      tagNode.AppendChild(AddSimpleTag("COMMENT", taginfo.description, doc));
      tagNode.AppendChild(AddSimpleTag("GENRE", taginfo.genre, doc));
      tagsNode.AppendChild(tagNode);
      doc.AppendChild(tagsNode);
      doc.InsertBefore(xmldecl, tagsNode);
      doc.Save(filename);
    }
    #endregion
  }
}
