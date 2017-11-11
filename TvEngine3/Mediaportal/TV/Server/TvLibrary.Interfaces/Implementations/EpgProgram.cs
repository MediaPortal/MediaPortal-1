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
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations
{
  /// <summary>
  /// Class which contains a single epg-program
  /// </summary>
  public class EpgProgram : IComparable<EpgProgram>
  {
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public IDictionary<string, string> Titles { get; set; }
    public IDictionary<string, string> Descriptions { get; set; }
    public IDictionary<string, string> EpisodeNames { get; set; }
    public string SeriesId { get; set; }
    public int? SeasonNumber { get; set; }
    public string EpisodeId { get; set; }
    public int? EpisodeNumber { get; set; }
    public int? EpisodePartNumber { get; set; }
    public bool? IsPreviouslyShown { get; set; }
    public IList<string> Categories { get; set; }
    public IDictionary<string, string> Classifications { get; set; }
    public ContentAdvisory Advisories { get; set; }
    public bool? IsHighDefinition { get; set; }
    public bool? IsThreeDimensional { get; set; }
    public IList<string> AudioLanguages { get; set; }
    public IList<string> SubtitlesLanguages { get; set; }
    public bool? IsLive { get; set; }
    public ushort? ProductionYear { get; set; }
    public IList<string> ProductionCountries { get; set; }
    public decimal? StarRating { get; set; }
    public decimal? StarRatingMaximum { get; set; }
    public IList<string> Actors { get; set; }
    public IList<string> Directors { get; set; }
    public IList<string> Guests { get; set; }
    public IList<string> Presenters { get; set; }
    public IList<string> Writers { get; set; }
    public IDictionary<string, string> OtherPeople { get; set; }

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgProgram"/> class.
    /// </summary>
    /// <param name="startTime">The program's start time.</param>
    /// <param name="endTime">The program's end time.</param>
    public EpgProgram(DateTime startTime, DateTime endTime)
    {
      StartTime = startTime;
      EndTime = endTime;
      Titles = new Dictionary<string, string>();
      Descriptions = new Dictionary<string, string>();
      EpisodeNames = new Dictionary<string, string>();
      SeriesId = null;
      SeasonNumber = null;
      EpisodeId = null;
      EpisodeNumber = null;
      EpisodePartNumber = null;
      IsPreviouslyShown = null;
      Categories = new List<string>();
      Classifications = new Dictionary<string, string>();
      Advisories = ContentAdvisory.None;
      IsHighDefinition = null;
      IsThreeDimensional = null;
      AudioLanguages = new List<string>();
      SubtitlesLanguages = new List<string>();
      IsLive = null;
      ProductionYear = null;
      ProductionCountries = new List<string>();
      StarRating = null;
      StarRatingMaximum = null;
      Actors = new List<string>();
      Directors = new List<string>();
      Guests = new List<string>();
      Presenters = new List<string>();
      Writers = new List<string>();
      OtherPeople = new Dictionary<string, string>();
    }

    #region IComparable member

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
    /// </returns>
    public int CompareTo(EpgProgram other)
    {
      if (other.StartTime > StartTime)
      {
        return -1;
      }
      if (other.StartTime < StartTime)
      {
        return 1;
      }
      return 0;
    }

    #endregion
  }
}