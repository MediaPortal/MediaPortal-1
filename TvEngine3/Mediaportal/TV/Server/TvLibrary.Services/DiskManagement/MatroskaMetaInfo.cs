#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  public class MatroskaMetaInfo
  {
    public string ChannelName = string.Empty;
    public DateTime StartTime = SqlDateTime.MinValue.Value;
    public DateTime EndTime = SqlDateTime.MinValue.Value;
    public string Title = string.Empty;
    public string Description = string.Empty;
    public string EpisodeName = null;
    public int? SeasonNumber = null;
    public int? EpisodeNumber = null;
    public int? EpisodePartNumber = null;
    public string ProgramCategory = null;

    // Not available in files produced by TVE 3.
    public MediaType MediaType = MediaType.Television;
    public string SeriesId = null;
    public string EpisodeId = null;
    public bool? IsPreviouslyShown = null;
    public DateTime? OriginalAirDate = null;
    public string Classification = null;
    public ContentAdvisory Advisories = ContentAdvisory.None;
    public bool? IsHighDefinition = null;
    public bool? IsThreeDimensional = null;
    public bool? IsLive = null;
    public int? ProductionYear = null;
    public string ProductionCountry = null;
    public decimal? StarRating = null;
    public decimal? StarRatingMaximum = null;
    public int WatchedCount = 0;
    public IDictionary<string, string> Credits = new Dictionary<string, string>();    // name => credit type

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

    /// <summary>
    /// Read a Matroska info file.
    /// </summary>
    /// <param name="fileName">The full name and path to the file.</param>
    /// <returns>a Matroska meta info object</returns>
    public static MatroskaMetaInfo Read(string fileName)
    {
      if (!File.Exists(fileName))
      {
        return null;
      }
      try
      {
        MatroskaMetaInfo info = new MatroskaMetaInfo();
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        XmlNodeList simpleTags = doc.SelectNodes("/tags/tag/SimpleTag");
        if (simpleTags == null)
        {
          return null;
        }
        foreach (XmlNode simpleTag in simpleTags)
        {
          string tagValue = simpleTag.ChildNodes[1].InnerText;
          switch (simpleTag.ChildNodes[0].InnerText)
          {
            case "CHANNEL_NAME":
              info.ChannelName = tagValue;
              break;
            case "STARTTIME":
            case "START_TIME":
              try
              {
                info.StartTime = DateTime.ParseExact(tagValue, "yyyy-MM-dd HH:mm", null);
              }
              catch
              {
                info.StartTime = SqlDateTime.MinValue.Value;
              }
              break;
            case "ENDTIME":
            case "END_TIME":
              try
              {
                info.EndTime = DateTime.ParseExact(tagValue, "yyyy-MM-dd HH:mm", null);
              }
              catch
              {
                info.EndTime = SqlDateTime.MinValue.Value;
              }
              break;
            case "TITLE":
              info.Title = tagValue;
              break;
            case "COMMENT":
            case "DESCRIPTION":
              info.Description = tagValue;
              break;
            case "EPISODENAME":
            case "EPISODE_NAME":
              if (!string.IsNullOrEmpty(tagValue))
              {
                info.EpisodeName = tagValue;
              }
              break;
            case "SERIESNUM":
            case "SEASON_NUMBER":
              int seasonNumber;
              if (int.TryParse(tagValue, out seasonNumber))
              {
                info.SeasonNumber = seasonNumber;
              }
              break;
            case "EPISODENUM":
            case "EPISODE_NUMBER":
              int episodeNumber;
              if (int.TryParse(tagValue, out episodeNumber))
              {
                info.EpisodeNumber = episodeNumber;
              }
              break;
            case "EPISODEPART":
            case "EPISODE_PART_NUMBER":
              int episodePartNumber;
              if (int.TryParse(tagValue, out episodePartNumber))
              {
                info.EpisodePartNumber = episodePartNumber;
              }
              break;
            case "GENRE":
              if (!string.IsNullOrEmpty(tagValue) && !string.Equals(tagValue, "-"))
              {
                info.ProgramCategory = tagValue;
              }
              break;

            // --- new ---
            case "MEDIA_TYPE":
              info.MediaType = (MediaType)Enum.Parse(typeof(MediaType), tagValue);
              break;
            case "SERIES_ID":
              if (!string.IsNullOrEmpty(tagValue))
              {
                info.SeriesId = tagValue;
              }
              break;
            case "EPISODE_ID":
              if (!string.IsNullOrEmpty(tagValue))
              {
                info.EpisodeId = tagValue;
              }
              break;
            case "IS_PREVIOUSLY_SHOWN":
              bool isPreviouslyShown;
              if (bool.TryParse(tagValue, out isPreviouslyShown))
              {
                info.IsPreviouslyShown = isPreviouslyShown;
              }
              break;
            case "ORIGINAL_AIR_DATE":
              DateTime originalAirDate;
              if (DateTime.TryParse(tagValue, out originalAirDate))
              {
                info.OriginalAirDate = originalAirDate;
              }
              break;
            case "CLASSIFICATION":
              if (!string.IsNullOrEmpty(tagValue))
              {
                info.Classification = tagValue;
              }
              break;
            case "ADVISORIES":
              ContentAdvisory advisories;
              if (Enum.TryParse<ContentAdvisory>(tagValue, out advisories))
              {
                info.Advisories = advisories;
              }
              break;
            case "IS_HIGH_DEFINITION":
              bool isHighDefinition;
              if (bool.TryParse(tagValue, out isHighDefinition))
              {
                info.IsHighDefinition = isHighDefinition;
              }
              break;
            case "IS_THREE_DIMENSIONAL":
              bool isThreeDimensional;
              if (bool.TryParse(tagValue, out isThreeDimensional))
              {
                info.IsThreeDimensional = isThreeDimensional;
              }
              break;
            case "IS_LIVE":
              bool isLive;
              if (bool.TryParse(tagValue, out isLive))
              {
                info.IsLive = isLive;
              }
              break;
            case "PRODUCTION_YEAR":
              int productionYear;
              if (int.TryParse(tagValue, out productionYear))
              {
                info.ProductionYear = productionYear;
              }
              break;
            case "PRODUCTION_COUNTRY":
              if (!string.IsNullOrEmpty(tagValue))
              {
                info.ProductionCountry = tagValue;
              }
              break;
            case "STAR_RATING":
              string[] ratingParts = tagValue.Split('/');
              if (ratingParts.Length == 2)
              {
                decimal rating;
                decimal maximumRating;
                if (decimal.TryParse(ratingParts[0], out rating) && decimal.TryParse(ratingParts[1], out maximumRating))
                {
                  info.StarRating = rating;
                  info.StarRatingMaximum = maximumRating;
                }
              }
              break;
            case "WATCHED_COUNT":
              int watchedCount;
              if (int.TryParse(tagValue, out watchedCount))
              {
                info.WatchedCount = watchedCount;
              }
              break;
            case "CREDITS":
              string[] credits = tagValue.Split(',');
              foreach (string credit in credits)
              {
                string[] creditParts = credit.Split(':');
                if (creditParts.Length == 2)
                {
                  info.Credits[creditParts[0]] = creditParts[1];
                }
              }
              break;
          }
        }
        return info;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Matroska meta-info: failed to read file, file name = {0}", fileName);
      }
      return null;
    }

    /// <summary>
    /// Write a Matroska info file.
    /// </summary>
    /// <param name="fileName">The full name and path to the file.</param>
    public void Write(string fileName)
    {
      if (!Directory.Exists(Path.GetDirectoryName(fileName)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      }
      XmlDocument doc = new XmlDocument();
      XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      XmlNode tagsNode = doc.CreateElement("tags");
      XmlNode tagNode = doc.CreateElement("tag");

      tagNode.AppendChild(AddSimpleTag("CHANNEL_NAME", ChannelName, doc));
      tagNode.AppendChild(AddSimpleTag("MEDIA_TYPE", MediaType.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("START_TIME", StartTime.ToString("yyyy-MM-dd HH:mm"), doc));
      tagNode.AppendChild(AddSimpleTag("END_TIME", EndTime.ToString("yyyy-MM-dd HH:mm"), doc));
      tagNode.AppendChild(AddSimpleTag("TITLE", Title, doc));
      tagNode.AppendChild(AddSimpleTag("DESCRIPTION", Description, doc));
      tagNode.AppendChild(AddSimpleTag("EPISODE_NAME", EpisodeName ?? string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("SERIES_ID", SeriesId ?? string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("SEASON_NUMBER", SeasonNumber.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("EPISODE_ID", EpisodeId ?? string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("EPISODE_NUMBER", EpisodeNumber.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("EPISODE_PART_NUMBER", EpisodePartNumber.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("IS_PREVIOUSLY_SHOWN", IsPreviouslyShown.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("ORIGINAL_AIR_DATE", OriginalAirDate.HasValue ? OriginalAirDate.Value.ToString("yyyy-MM-dd") : string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("GENRE", ProgramCategory ?? string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("CLASSIFICATION", Classification ?? string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("ADVISORIES", Advisories.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("IS_HIGH_DEFINITION", IsHighDefinition.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("IS_THREE_DIMENSIONAL", IsThreeDimensional.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("IS_LIVE", IsLive.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("PRODUCTION_YEAR", ProductionYear.ToString(), doc));
      tagNode.AppendChild(AddSimpleTag("PRODUCTION_COUNTRY", ProductionCountry ?? string.Empty, doc));
      tagNode.AppendChild(AddSimpleTag("WATCHED_COUNT", WatchedCount.ToString(), doc));

      string rating = string.Empty;
      if (StarRating.HasValue && StarRatingMaximum.HasValue)
      {
        rating = string.Format("{0}/{1}", StarRating, StarRatingMaximum);
      }
      tagNode.AppendChild(AddSimpleTag("STAR_RATING", rating, doc));

      IList<string> credits = new List<string>(Credits.Count);
      foreach (var pair in Credits)
      {
        credits.Add(string.Format("{0}:{1}", pair.Key, pair.Value));
      }
      tagNode.AppendChild(AddSimpleTag("CREDITS", string.Join(",", credits), doc));

      tagsNode.AppendChild(tagNode);
      doc.AppendChild(tagsNode);
      doc.InsertBefore(xmldecl, tagsNode);
      doc.Save(fileName);
    }
  }
}