#region Copyright (C) 2005-2009 Team MediaPortal

/*  Copyright (C) 2005-2009 Team MediaPortal
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
using System.Xml;

namespace MediaPortal.GUI.Video
{
  internal class MatroskaTagInfo
  {
    public string title;
    public string description;
    public string genre;
    public string channelName;
  }

  internal class MatroskaTagHandler
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
      MatroskaTagInfo info = new MatroskaTagInfo();
      try
      {
        if (!File.Exists(filename))
        {
          return null;
        }

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
            case "CHANNEL_NAME":
              info.channelName = simpleTag.ChildNodes[1].InnerText;
              break;
          }
        }
      }
      catch (Exception)
      {
      } // loading the XML doc could fail
      return info;
    }

    public static void Persist(string filename, MatroskaTagInfo taginfo)
    {
      try
      {
        if (!Directory.Exists(Path.GetDirectoryName(filename)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(filename));
        }

        XmlDocument doc = new XmlDocument();
        XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        XmlNode tagsNode = doc.CreateElement("tags");
        XmlNode tagNode = doc.CreateElement("tag");
        tagNode.AppendChild(AddSimpleTag("TITLE", taginfo.title, doc));
        tagNode.AppendChild(AddSimpleTag("COMMENT", taginfo.description, doc));
        tagNode.AppendChild(AddSimpleTag("GENRE", taginfo.genre, doc));
        tagNode.AppendChild(AddSimpleTag("CHANNEL_NAME", taginfo.channelName, doc));
        tagsNode.AppendChild(tagNode);
        doc.AppendChild(tagsNode);
        doc.InsertBefore(xmldecl, tagsNode);
        doc.Save(filename);
      }
      catch (Exception)
      {
      }
    }

    #endregion
  }
}