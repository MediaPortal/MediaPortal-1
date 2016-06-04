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
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  [DataContract(Name = "Recording", Namespace = "")]
  internal class ArgusMetaInfo
  {
    private const string EXTENSION = ".arg";

    // https://github.com/ARGUS-TV/ARGUS-TV/blob/master/ArgusTV.DataContracts/GuideProgramFlags.cs
    [Flags]
    public enum ArgusGuideProgramFlags
    {
      StandardAspectRatio = 0x1,    // 4:3
      WidescreenAspectRatio = 0x2,  // 16:9
      HighDefinition = 0x4
    }

    // https://github.com/ARGUS-TV/ARGUS-TV/blob/master/ArgusTV.DataContracts/ChannelType.cs
    public enum ArgusChannelType
    {
      Television = 0,
      Radio
    }

    // https://github.com/ARGUS-TV/ARGUS-TV/blob/master/ArgusTV.DataContracts/SchedulePriority.cs
    public enum ArgusSchedulePriority
    {
      VeryLow = -2,
      Low,
      Normal,
      High,
      VeryHigh
    }

    // https://github.com/ARGUS-TV/ARGUS-TV/blob/master/ArgusTV.DataContracts/KeepUntilMode.cs
    public enum ArgusKeepUntilMode
    {
      UntilSpaceIsNeeded = 0,
      Forever,
      NumberOfDays,
      NumberOfEpisodes,
      NumberOfWatchedEpisodes
    }

    // https://github.com/ARGUS-TV/ARGUS-TV/blob/master/ArgusTV.DataContracts/Recording.cs
    [DataMember(Order = 0)]
    public Guid RecordingId;
    [DataMember(Order = 1)]
    public int Id;
    [DataMember(Order = 2)]
    public Guid ScheduleId;
    [DataMember(Order = 3)]
    public string ScheduleName;
    [DataMember(Order = 4)]
    public ArgusSchedulePriority SchedulePriority;
    [DataMember(Order = 5)]
    public bool IsPartOfSeries;
    [DataMember(Order = 6)]
    public ArgusKeepUntilMode KeepUntilMode;
    [DataMember(Order = 7)]
    public int? KeepUntilValue;
    [DataMember(Order = 8)]
    public DateTime? LastWatchedTime;
    [DataMember(Order = 9)]
    public int? LastWatchedPosition;
    [DataMember(Order = 10)]
    public bool IsFullyWatched;
    [DataMember(Order = 11)]
    public int FullyWatchedCount;
    [DataMember(Order = 12)]
    public Guid ChannelId;
    [DataMember(Order = 13)]
    public string ChannelDisplayName;
    [DataMember(Order = 14)]
    public ArgusChannelType ChannelType;
    [DataMember(Order = 15)]
    public DateTime RecordingStartTime;
    [DataMember(Order = 16)]
    public DateTime? RecordingStopTime;
    [DataMember(Order = 17)]
    public DateTime RecordingStartTimeUtc;
    [DataMember(Order = 18)]
    public DateTime? RecordingStopTimeUtc;
    [DataMember(Order = 19)]
    public bool IsPartialRecording;
    [DataMember(Order = 20)]
    public DateTime ProgramStartTime;
    [DataMember(Order = 21)]
    public DateTime ProgramStopTime;
    [DataMember(Order = 22)]
    public DateTime ProgramStartTimeUtc;
    [DataMember(Order = 23)]
    public DateTime ProgramStopTimeUtc;   // Note: sample showed ProgramStopTimeUtc before ProgramStartTimeUtc; class code was the other way around as at June 2016.
    [DataMember(Order = 24)]
    public string Title;
    [DataMember(Order = 25)]
    public string SubTitle;
    [DataMember(Order = 26)]
    public string Description;
    [DataMember(Order = 27)]
    public string Category;
    [DataMember(Order = 28)]
    public bool IsRepeat;
    [DataMember(Order = 29)]
    public bool IsPremiere;
    [DataMember(Order = 30)]
    public ArgusGuideProgramFlags Flags;
    [DataMember(Order = 31)]
    public int? SeriesNumber;
    [DataMember(Order = 32)]
    public string EpisodeNumberDisplay;
    [DataMember(Order = 33)]
    public int? EpisodeNumber;
    [DataMember(Order = 34)]
    public int? EpisodeNumberTotal;
    [DataMember(Order = 35)]
    public int? EpisodePart;
    [DataMember(Order = 36)]
    public int? EpisodePartTotal;
    [DataMember(Order = 37)]
    public string Rating;
    [DataMember(Order = 38)]
    public double? StarRating;
    [DataMember(Order = 39, Name = "Director")]
    private string CsvDirector;
    [DataMember(Order = 40, Name = "Actors")]
    private string CsvActors;

    public IList<string> Directors = new List<string>();
    public IList<string> Actors = new List<string>();

    /// <summary>
    /// Read an ARGUS-TV info file.
    /// </summary>
    /// <param name="fileName">The full name and path to the recording file.</param>
    /// <returns>an ARGUS meta info object</returns>
    public static ArgusMetaInfo Read(string fileName)
    {
      fileName = Path.ChangeExtension(fileName, EXTENSION);
      if (!File.Exists(fileName))
      {
        return null;
      }
      try
      {
        using (XmlReader xmlReader = XmlReader.Create(fileName))
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(ArgusMetaInfo));
          ArgusMetaInfo info = (ArgusMetaInfo)serializer.ReadObject(xmlReader);
          if (!string.IsNullOrWhiteSpace(info.CsvDirector))
          {
            info.Directors = info.CsvDirector.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
          }
          if (!string.IsNullOrWhiteSpace(info.CsvActors))
          {
            info.Actors = info.CsvActors.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
          }
          xmlReader.Close();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ARGUS meta-info: failed to read file, file name = {0}", fileName);
      }
      return null;
    }
  }
}