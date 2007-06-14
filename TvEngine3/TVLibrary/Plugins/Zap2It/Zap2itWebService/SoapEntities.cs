#region Copyright (C) 2005-2007 Team MediaPortal
/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
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
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Zap2it.SoapEntities
{
  /// <summary>
  /// Full schema definition available at 
  /// http://docs.tms.tribune.com/tech/tmsdatadirect/zap2it/XTVDSchemaDefinition.pdf
  /// </summary>
  #region XTVD
  [XmlRoot(ElementName = "xtvd", Namespace = "urn:TMSWebServices")]
  public class XTVD
  {
    [XmlAttribute("from")]
    public DateTime From;
    [XmlAttribute("to")]
    public DateTime To;
    [XmlAttribute("schemaVersion")]
    public string SchemaVersion;
    [XmlElement("stations")]
    public TVStations Stations;
    [XmlElement("lineups")]
    public TVLineups Lineups = new TVLineups();
    [XmlElement("schedules")]
    public TVSchedules Schedules = new TVSchedules();
    [XmlElement("programs")]
    public TVPrograms Programs = new TVPrograms();
    [XmlElement("productionCrew")]
    public TVCrews Crews = new TVCrews();
    [XmlElement("genres")]
    public TVGenres Genres = new TVGenres();

    /// <summary>
    /// Writes serialized object to given XML writer
    /// </summary>
    /// <param name="xmlWriter">The XML writer.</param>
    public void WriteTo(System.Xml.XmlWriter xmlWriter)
    {
      XmlSerializer serializer = new XmlSerializer(this.GetType());
      serializer.Serialize(xmlWriter, this);
    }
  }
  #endregion

  #region TVStations
  public class TVStations
  {
    [XmlElement("station")]
    public List<TVStation> List = new List<TVStation>(0);

    #region StationIndex
    [NonSerialized]
    private Dictionary<string, TVStation> _mapIndex;

    public TVStation StationById(string stationId)
    {
      TVStation result;
      if (_mapIndex == null)
      {
        _mapIndex = new Dictionary<string, TVStation>(this.List.Count);
        foreach (TVStation item in this.List)
        {
          _mapIndex.Add(item.ID, item);
        }
      }
      _mapIndex.TryGetValue(stationId, out result);
      return result;
    }
    #endregion
  }
  #endregion

  #region TVStation
  public class TVStation : IComparable<TVStation>
  {
    [XmlAttribute("id")]
    public string ID = string.Empty;
    [XmlElement("callSign")]
    public string CallSign = string.Empty;
    [XmlElement("name")]
    public string Name = string.Empty;
    [XmlElement("affiliate")]
    public string Affiliate = string.Empty;
    [XmlElement("fccChannelNumber")]
    public int BroadcastChannelNumber;

    /// <summary>
    /// Gets a value indicating whether this TVStation instance is digital. All OTA
    /// digital stations are designated by the use of "DTn" where n is the subchannel
    /// (if n is not presented, it is assumed to be 0)
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this TVStation instance is digital; otherwise, <c>false</c>.
    /// </value>
    public bool IsDigitalTerrestrial
    {
      get { return BroadcastChannelNumber > 0 && Regex.Match(this.CallSign, @"DT\d?$").Success; }
    }

    /// <summary>
    /// Gets a value indicating the Digital Terrestrial SubChannel aka Minor Channel. All OTA
    /// digital stations are designated by the use of "DTn" where n is the subchannel
    /// (if n is not presented, it is assumed to be 1)
    /// </summary>
    /// <param name="subChannel">The Digital Terrestrial SubChannel aka Minor Channel.</param>
    /// <returns>True if this TVStation instance is digital and subchannel was found; otherwise, false</returns>
    public bool DigitalTerrestrialSubChannel(out int subChannel)
    {
      subChannel = 0;

      if (!IsDigitalTerrestrial)
        return false;

      try
      {
        string rx = @"DT(?<dtSubChannel>\d?$)";
        Regex reg = new Regex(rx);
        Match rmatch = reg.Match(this.CallSign);
        string sch = rmatch.Groups["dtSubChannel"].Value;
        if (!String.IsNullOrEmpty(sch))
          subChannel = Int32.Parse(sch);
        else
          subChannel = 1;
        return true;
      }
      catch
      {
        subChannel = 1;
        return true;
      }

      return false;
    }

    #region IComparable<TVStation> Members
    public int CompareTo(TVStation other)
    {
      return ID.CompareTo(other.ID);
    }
    #endregion
  }
  #endregion

  #region TVLineups
  public class TVLineups
  {
    [XmlElement("lineup")]
    public List<TVLineup> List = new List<TVLineup>(0);

    #region LineupIndex
    [NonSerialized]
    private Dictionary<string, TVLineup> _mapIndex;

    public TVLineup LineupById(string lineupId)
    {
      TVLineup result;
      if (_mapIndex == null)
      {
        _mapIndex = new Dictionary<string, TVLineup>(this.List.Count);
        foreach (TVLineup item in this.List)
        {
          _mapIndex.Add(item.ID, item);
        }
      }
      _mapIndex.TryGetValue(lineupId, out result);
      return result;
    }
    #endregion
  }
  #endregion

  #region TVLineup
  public class TVLineup
  {
    [XmlAttribute("id")]
    public string ID = string.Empty;
    [XmlAttribute("name")]
    public string Name = string.Empty;
    [XmlAttribute("location")]
    public string Location = string.Empty;
    [XmlAttribute("type")]
    public LineupTypes Type;
    [XmlAttribute("device")]
    public string Device = string.Empty;
    [XmlAttribute("postalCode")]
    public string PostalCode = string.Empty;
    [XmlElement("map")]
    public List<TVStationMap> StationMap = new List<TVStationMap>(0);

    /// <summary>
    /// Determines whether this lineup is analogue.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance is analogue; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnalogue()
    {
      if (Name.Contains("DIRECTV")) return false;
      if (Type == LineupTypes.Satellite) return false;
      if (Type == LineupTypes.CableDigital) return false;
      return true;
    }

    /// <summary>
    /// Determines whether this lineup is Local Broadcast.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance is Local Broadcast; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocalBroadcast()
    {
      if (Name.Contains("DIRECTV")) return false;
      if (Type == LineupTypes.LocalBroadcast) return true;
      //if (Type == LineupTypes.Satellite) return false;
      //if (Type == LineupTypes.CableDigital) return false;
      //if (Type == LineupTypes.Cable) return false;
      return false;
    }


    #region StationMapIndex
    [NonSerialized]
    private Dictionary<string, TVStationMap> _mapIndex;

    public TVStationMap StationMapById(string stationId)
    {
      TVStationMap result;
      if (_mapIndex == null)
      {
        _mapIndex = new Dictionary<string, TVStationMap>(this.StationMap.Count);
        foreach (TVStationMap item in this.StationMap)
        {
          _mapIndex.Add(item.StationId, item);
        }
      }
      _mapIndex.TryGetValue(stationId, out result);
      return result;
    }
    #endregion
  }
  #endregion

  #region TVStationMap
  public class TVStationMap
  {
    [XmlAttribute("station")]
    public string StationId = string.Empty;
    [XmlAttribute("channel")]
    public string Zap2ItChannel; //public int ChannelMajor;
    [XmlAttribute("channelMinor")]
    public int ChannelMinor;
    [XmlAttribute("from")]
    public string OnAirFrom = string.Empty;

    private int _channelMajor = -1;

    /// <summary>
    /// Gets the full channel (major-minor format if there is a minor channel specified)
    /// </summary>
    /// <value>The channel.</value>
    public string Channel
    {
      get { return ChannelMajor + (ChannelMinor > 0 ? "-" + ChannelMinor : string.Empty); }
    }

    /// <summary>
    /// Gets the int channel (major)
    /// </summary>
    /// <value>The channel.</value>
    public int ChannelMajor
    {
      get
      {
        if (_channelMajor > -1)
          return _channelMajor;

        try
        {
          _channelMajor = Int32.Parse(Zap2ItChannel);
          return _channelMajor;
        }
        catch
        {
          return -1;
        }
      }
      set { _channelMajor = value; }
    }

  }
  #endregion

  #region TVSchedules
  public class TVSchedules
  {
    [XmlElement("schedule")]
    public List<TVSchedule> List = new List<TVSchedule>(0);
  }
  #endregion

  #region TVSchedule
  public class TVSchedule
  {
    [XmlAttribute("program")]
    public string ProgramId = string.Empty;
    [XmlAttribute("station")]
    public string Station = string.Empty;
    [XmlAttribute("time")]
    public string StartTimeStr = string.Empty;
    [XmlAttribute("duration")]
    public string DurationStr = string.Empty;
    [XmlAttribute("repeat")]
    public bool Repeat;
    [XmlAttribute("stereo")]
    public bool Stereo;
    [XmlAttribute("subtitled")]
    public bool SubTitled;
    [XmlAttribute("hdtv")]
    public bool HDTV;
    [XmlAttribute("closeCaptioned")]
    public bool CloseCaptioned;
    [XmlAttribute("tvRating")]
    public string TVRating = string.Empty;
    [XmlAttribute("dolby")]
    public string Dolby = string.Empty;
    [XmlElement("part")]
    public TVPart Part = new TVPart();

    /// <summary>
    /// Gets the start date and time in UTC.
    /// </summary>
    /// <value>The start date and time in UTC.</value>
    public DateTime StartTimeUtc
    {
      get
      {
        return DateTime.ParseExact(this.StartTimeStr, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InstalledUICulture, DateTimeStyles.AdjustToUniversal);
      }
    }

    /// <summary>
    /// Gets the program duration.
    /// </summary>
    /// <value>The duration timespan</value>
    public TimeSpan Duration
    {
      get
      {
        string str = this.DurationStr.Substring(2, 2) + ":" + this.DurationStr.Substring(5, 2);
        return TimeSpan.Parse(str);
      }
    }
  }

  public class TVPart
  {
    [XmlAttribute("number")]
    public int Number;
    [XmlAttribute("total")]
    public int Total;
  }
  #endregion

  #region TVPrograms
  public class TVPrograms
  {
    [XmlElement("program")]
    public List<TVProgram> List = new List<TVProgram>(0);

    #region ProgramIndex
    [NonSerialized]
    private Dictionary<string, TVProgram> _mapIndex;

    public TVProgram ProgramById(string programId)
    {
      TVProgram result;
      if (_mapIndex == null)
      {
        _mapIndex = new Dictionary<string, TVProgram>(this.List.Count);
        foreach (TVProgram item in this.List)
        {
          _mapIndex.Add(item.ID, item);
        }
      }
      _mapIndex.TryGetValue(programId, out result);
      return result;
    }
    #endregion
  }
  #endregion

  #region TVProgram
  public class TVProgram
  {
    [XmlAttribute("id")]
    public string ID = string.Empty;
    [XmlElement("series")]
    public string Series = string.Empty;
    [XmlElement("title")]
    public string Title = string.Empty;
    [XmlElement("subtitle")]
    public string Subtitle = string.Empty;
    [XmlElement("description")]
    public string Description = string.Empty;
    [XmlElement("mpaaRating")]
    public string MPAARating = string.Empty;
    [XmlElement("starRating")]
    public string StarRating = string.Empty;
    [XmlElement("runTime")]
    public string RunTime = string.Empty;
    [XmlElement("year")]
    public int Year;
    [XmlElement("showType")]
    public string ShowType = string.Empty;
    [XmlElement("colorCode")]
    public string ColorCode = string.Empty;
    [XmlElement("originalAirDate")]
    public string OriginalAirDateStr = string.Empty;
    [XmlElement("syndicatedEpisodeNumber")]
    public string SyndicatedEpisodeNumber = string.Empty;
    [XmlElement("advisories")]
    public TVAdvisory Advisories = new TVAdvisory();

    /// <summary>
    /// Determines whether the specified tv schedule item is repeat based on the logic provided
    /// by Zap2it at http://bb.labs.zap2it.com/viewtopic.php?t=48 and http://bb.labs.zap2it.com/viewtopic.php?t=66
    /// </summary>
    /// <param name="tvScheduleItem">The tv schedule item.</param>
    /// <returns>
    /// 	<c>true</c> if the specified tv schedule item is repeat; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRepeat(TVSchedule tvScheduleItem)
    {
      // If the tvScheduleItem says it is a repeat, we believe it
      if (tvScheduleItem.Repeat) return true;

      // If this is an episode and the original episode air date isn't the same as the schedule date, it is a repeat
      if (this.ID.StartsWith("EP") && !string.IsNullOrEmpty(this.OriginalAirDateStr) &&
          tvScheduleItem.StartTimeUtc.Subtract(this.OriginalAirDate).TotalDays > 60) return true;

      // If this is a show with specified show type and the original episode air date doesn't match the schedule date, it is a repeat
      if (this.ID.StartsWith("SH") && !string.IsNullOrEmpty(this.OriginalAirDateStr) &&
           tvScheduleItem.StartTimeUtc.Subtract(this.OriginalAirDate).TotalDays > 60 &&
          (this.ShowType == "Short Film" || this.ShowType == "Special")) return true;

      return false;
    }

    /// <summary>
    /// Gets the original air date as a DateTime.
    /// </summary>
    /// <value>The original air date.</value>
    public DateTime OriginalAirDate
    {
      get
      {
        DateTime oad;
        DateTime.TryParse(OriginalAirDateStr, out oad);
        return oad;
      }
    }

    /// <summary>
    /// Gets the star rating as a whole number out of 8.
    /// </summary>
    /// <value>The star rating number.</value>
    public int StarRatingNum
    {
      get
      {
        if (this.StarRating != null)
        {
          int count = 2 * (this.StarRating.LastIndexOf('*') + 1);
          count += (this.StarRating.EndsWith("+")) ? 1 : 0;
          return count;
        }
        else
        {
          return 0;
        }
      }
    }
  }
  #endregion

  #region TVAdvisory
  public class TVAdvisory
  {
    [XmlElement("advisory")]
    public List<string> Advisory = new List<string>(0);
  }
  #endregion

  #region TVCrews
  public class TVCrews
  {
    [XmlElement("crew")]
    public List<TVCrew> List = new List<TVCrew>(0);
  }
  #endregion

  #region TVCrew
  public class TVCrew
  {
    [XmlAttribute("program")]
    public string Program = string.Empty;
    [XmlElement("member")]
    public List<TVMember> Member = new List<TVMember>(0);
  }
  #endregion

  #region TVMember
  public class TVMember
  {
    [XmlElement("role")]
    public string Role = string.Empty;
    [XmlElement("givenname")]
    public string GivenName = string.Empty;
    [XmlElement("surname")]
    public string Surname = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    /// <value>The full name.</value>
    public string FullName
    {
      get { return (GivenName + " " + Surname).Trim(); }
    }
  }
  #endregion

  #region TVGenres
  public class TVGenres
  {
    [XmlElement("programGenre")]
    public List<ProgramGenre> List = new List<ProgramGenre>(0);

    #region ProgramGenreIndex
    [NonSerialized]
    private Dictionary<string, ProgramGenre> _mapIndex;

    public ProgramGenre ProgramGenreById(string programId)
    {
      ProgramGenre result;
      if (_mapIndex == null)
      {
        _mapIndex = new Dictionary<string, ProgramGenre>(this.List.Count);
        foreach (ProgramGenre item in this.List)
        {
          _mapIndex.Add(item.ProgramId, item);
        }
      }
      _mapIndex.TryGetValue(programId, out result);
      return result;
    }
    #endregion
  }
  #endregion

  #region ProgramGenre
  public class ProgramGenre
  {
    [XmlAttribute("program")]
    public string ProgramId = string.Empty;
    [XmlElement("genre")]
    public List<TVGenre> List = new List<TVGenre>(0);
  }
  #endregion

  #region TVGenre
  public class TVGenre
  {
    [XmlElement("class")]
    public string GenreClass = string.Empty;
    [XmlElement("relevance")]
    public string Relevence = string.Empty;
  }
  #endregion

  #region LineupTypes
  public enum LineupTypes
  {
    Cable,
    CableDigital,
    Satellite,
    LocalBroadcast
  }
  #endregion

  #region DownloadResults
  /// <summary>
  /// Returned by Download method, contains the XTVD object,
  /// date of subscription expiration and a list of
  /// messages returned by Zap2it.
  /// </summary>
  public class DownloadResults
  {
    internal DateTime _subscriptionExpiration;
    internal List<string> _messages = new List<string>(0);
    internal Zap2it.SoapEntities.XTVD _data = new Zap2it.SoapEntities.XTVD();

    /// <summary>
    /// Gets the list of messages returned.
    /// </summary>
    /// <value>The messages.</value>
    public List<string> Messages
    {
      get { return _messages; }
    }

    /// <summary>
    /// Gets the television data returned.
    /// </summary>
    /// <value>The data.</value>
    public Zap2it.SoapEntities.XTVD Data
    {
      get { return _data; }
    }

    /// <summary>
    /// Gets the subscription expiration date.
    /// </summary>
    /// <value>The subscription expiration.</value>
    public DateTime SubscriptionExpiration
    {
      get { return _subscriptionExpiration; }
    }
  }
  #endregion
}