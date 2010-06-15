#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TvLibrary.Log;
using System.Data.SqlTypes;

namespace TvDatabase
{
  public class MatroskaTagInfo
  {
    public string title;
    public string description;
    public string genre;
    public string channelName;
    public string episodeName = "";
    public string seriesNum = "";
    public string episodeNum = "";
    public string episodePart = "";
    public DateTime startTime = SqlDateTime.MinValue.Value;
    public DateTime endTime = SqlDateTime.MinValue.Value;
  }

  /// <summary>
  /// Contains basic read and write methods to handle Matroska tags for recordings
  /// </summary>
  public class MatroskaTagHandler
  {
    #region Event delegates

    public delegate void TagLookupSuccessful(Dictionary<string, MatroskaTagInfo> FoundTags);

    public static event TagLookupSuccessful OnTagLookupCompleted;

    #endregion

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

    /// <summary>
    /// Searches a given path and its subdirectories for XML files and loads them into corresponding Matroska tags
    /// </summary>
    /// <param name="aLookupDirString">The parent folder (of recordings)</param>
    public static void GetAllMatroskaTags(object aLookupDirString)
    {
      string aDirectory = aLookupDirString.ToString();
      Dictionary<string, MatroskaTagInfo> fileRecordings = GetTagInfoForDirectory(aDirectory);

      if (OnTagLookupCompleted != null)
      {
        OnTagLookupCompleted(fileRecordings);
      }
    }

    private static Dictionary<string, MatroskaTagInfo> GetTagInfoForDirectory(string aDirectory)
    {
      Dictionary<string, MatroskaTagInfo> foundTagInfo = new Dictionary<string, MatroskaTagInfo>();
      try
      {
        string[] importDirs = new string[] {};
        // get all subdirectories
        try
        {
          importDirs = Directory.GetDirectories(aDirectory, "*", SearchOption.TopDirectoryOnly);
        }
        catch (Exception ex)
        {
          Log.Info("Error while reading subdirectories of {0}: {1}", aDirectory, ex);
        }
        List<string> searchDirs = new List<string>(importDirs);
        foreach (string subDir in searchDirs)
        {
          Dictionary<string, MatroskaTagInfo> foundTagsInSubDir = GetTagInfoForDirectory(subDir);
          foreach (KeyValuePair<string, MatroskaTagInfo> kvp in foundTagsInSubDir)
          {
            foundTagInfo.Add(kvp.Key, kvp.Value);
          }
        }
        try
        {
            string[] importFiles = Directory.GetFiles(aDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string xmlFile in importFiles)
            {
              try
              {
                MatroskaTagInfo importTag = ReadTag(xmlFile);
                foundTagInfo[xmlFile] = importTag;
              }
              catch (Exception ex)
              {
                Log.Info("Error while reading matroska informations in file {0}: {1}",xmlFile, ex);
              }
            }
          }
          catch (Exception ex)
          {
            Log.Info("Error while reading matroska informations in directory {0}: {1}", aDirectory, ex);
          }

      }
      catch (Exception ex)
      {
        Log.Info("Error while reading all matroska informations : ", ex);
      }
      return foundTagInfo;
    }


    /// <summary>
    /// Reads Matroska tag files
    /// </summary>
    /// <param name="filename">Path to an XML file containing recording infos</param>
    /// <returns>The Matroska tag object</returns>
    public static MatroskaTagInfo ReadTag(string filename)
    {
      if (!File.Exists(filename))
      {
        return null;
      }
      MatroskaTagInfo info = new MatroskaTagInfo();
      XmlDocument doc = new XmlDocument();
      doc.Load(filename);
      XmlNodeList simpleTags = doc.SelectNodes("/tags/tag/SimpleTag");
      if (simpleTags != null)
      {
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
            case "SERIESNUM":
              info.seriesNum = simpleTag.ChildNodes[1].InnerText;
              break;
            case "EPISODENUM":
              info.episodeNum = simpleTag.ChildNodes[1].InnerText;
              break;
            case "EPISODEPART":
              info.episodePart = simpleTag.ChildNodes[1].InnerText;
              break;
            case "EPISODENAME":
              info.episodeName = simpleTag.ChildNodes[1].InnerText;
              break;
            case "STARTTIME":
              try
              {
                info.startTime = DateTime.ParseExact(simpleTag.ChildNodes[1].InnerText, "yyyy-MM-dd HH:mm", null);
              }
              catch (Exception)
              {
                info.startTime = SqlDateTime.MinValue.Value;
              }
              break;
            case "ENDTIME":
              try
              {
                info.endTime = DateTime.ParseExact(simpleTag.ChildNodes[1].InnerText, "yyyy-MM-dd HH:mm", null);
              }
              catch (Exception)
              {
                info.endTime = SqlDateTime.MinValue.Value;
              }
              break;
          }
        }
      }
      return info;
    }

    /// <summary>
    /// Saves the given MatroskaTagInfo into an XML file
    /// </summary>
    /// <param name="filename">Filename of the XML file</param>
    /// <param name="taginfo">the recording information the xml file should contain</param>
    public static void WriteTag(string filename, MatroskaTagInfo taginfo)
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
      tagNode.AppendChild(AddSimpleTag("EPISODENAME", taginfo.episodeName, doc));
      tagNode.AppendChild(AddSimpleTag("SERIESNUM", taginfo.seriesNum, doc));
      tagNode.AppendChild(AddSimpleTag("EPISODENUM", taginfo.episodeNum, doc));
      tagNode.AppendChild(AddSimpleTag("EPISODEPART", taginfo.episodePart, doc));
      tagNode.AppendChild(AddSimpleTag("STARTTIME", taginfo.startTime.ToString("yyyy-MM-dd HH:mm"), doc));
      tagNode.AppendChild(AddSimpleTag("ENDTIME", taginfo.endTime.ToString("yyyy-MM-dd HH:mm"), doc));
      tagsNode.AppendChild(tagNode);
      doc.AppendChild(tagsNode);
      doc.InsertBefore(xmldecl, tagsNode);
      doc.Save(filename);
    }

    #endregion
  }
}