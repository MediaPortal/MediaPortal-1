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
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc
{
  /// <summary>
  /// An implementation of <see cref="IEpgGrabber"/> for electronic programme
  /// guide data formats used by ATSC and SCTE broadcasters.
  /// </summary>
  internal class EpgGrabberAtsc : IEpgGrabberInternal
  {
    #region constants

    private static readonly IDictionary<byte, string> MAPPING_ATSC_GENRES = new Dictionary<byte, string>(256)
    {
      { 0x20, "Education" },
      { 0x21, "Entertainment" },
      { 0x22, "Movie" },
      { 0x23, "News" },
      { 0x24, "Religious" },
      { 0x25, "Sports" },
      { 0x26, "Other" },
      { 0x27, "Action" },
      { 0x28, "Advertisement" },
      { 0x29, "Animated" },
      { 0x2a, "Anthology" },
      { 0x2b, "Automobile" },
      { 0x2c, "Awards" },
      { 0x2d, "Baseball" },
      { 0x2e, "Basketball" },
      { 0x2f, "Bulletin" },
      { 0x30, "Business" },
      { 0x31, "Classical" },
      { 0x32, "College" },
      { 0x33, "Combat" },
      { 0x34, "Comedy" },
      { 0x35, "Commentary" },
      { 0x36, "Concert" },
      { 0x37, "Consumer" },
      { 0x38, "Contemporary" },
      { 0x39, "Crime" },
      { 0x3a, "Dance" },
      { 0x3b, "Documentary" },
      { 0x3c, "Drama" },
      { 0x3d, "Elementary" },
      { 0x3e, "Erotica" },
      { 0x3f, "Exercise" },
      { 0x40, "Fantasy" },
      { 0x41, "Farm" },
      { 0x42, "Fashion" },
      { 0x43, "Fiction" },
      { 0x44, "Food" },
      { 0x45, "Football" },
      { 0x46, "Foreign" },
      { 0x47, "Fund Raiser" },
      { 0x48, "Game/Quiz" },
      { 0x49, "Garden" },
      { 0x4a, "Golf" },
      { 0x4b, "Government" },
      { 0x4c, "Health" },
      { 0x4d, "High School" },
      { 0x4e, "History" },
      { 0x4f, "Hobby" },
      { 0x50, "Hockey" },
      { 0x51, "Home" },
      { 0x52, "Horror" },
      { 0x53, "Information" },
      { 0x54, "Instruction" },
      { 0x55, "International" },
      { 0x56, "Interview" },
      { 0x57, "Language" },
      { 0x58, "Legal" },
      { 0x59, "Live" },
      { 0x5a, "Local" },
      { 0x5b, "Math" },
      { 0x5c, "Medical" },
      { 0x5d, "Meeting" },
      { 0x5e, "Military" },
      { 0x5f, "Miniseries" },
      { 0x60, "Music" },
      { 0x61, "Mystery" },
      { 0x62, "National" },
      { 0x63, "Nature" },
      { 0x64, "Police" },
      { 0x65, "Politics" },
      { 0x66, "Premier" },
      { 0x67, "Prerecorded" },
      { 0x68, "Product" },
      { 0x69, "Professional" },
      { 0x6a, "Public" },
      { 0x6b, "Racing" },
      { 0x6c, "Reading" },
      { 0x6d, "Repair" },
      { 0x6e, "Repeat" },
      { 0x6f, "Review" },
      { 0x70, "Romance" },
      { 0x71, "Science" },
      { 0x72, "Series" },
      { 0x73, "Service" },
      { 0x74, "Shopping" },
      { 0x75, "Soap Opera" },
      { 0x76, "Special" },
      { 0x77, "Suspense" },
      { 0x78, "Talk" },
      { 0x79, "Technical" },
      { 0x7a, "Tennis" },
      { 0x7b, "Travel" },
      { 0x7c, "Variety" },
      { 0x7d, "Video" },
      { 0x7e, "Weather" },
      { 0x7f, "Western" },
      { 0x80, "Art" },
      { 0x81, "Auto Racing" },
      { 0x82, "Aviation" },
      { 0x83, "Biography" },
      { 0x84, "Boating" },
      { 0x85, "Bowling" },
      { 0x86, "Boxing" },
      { 0x87, "Cartoon" },
      { 0x88, "Children" },
      { 0x89, "Classic Film" },
      { 0x8a, "Community" },
      { 0x8b, "Computers" },
      { 0x8c, "Country Music" },
      { 0x8d, "Court" },
      { 0x8e, "Extreme Sports" },
      { 0x8f, "Family" },
      { 0x90, "Financial" },
      { 0x91, "Gymnastics" },
      { 0x92, "Headlines" },
      { 0x93, "Horse Racing" },
      { 0x94, "Hunting/Fishing/Outdoors" },
      { 0x95, "Independent" },
      { 0x96, "Jazz" },
      { 0x97, "Magazine" },
      { 0x98, "Motorcycle Racing" },
      { 0x99, "Music/Film/Books" },
      { 0x9a, "News-International" },
      { 0x9b, "News-Local" },
      { 0x9c, "News-National" },
      { 0x9d, "News-Regional" },
      { 0x9e, "Olympics" },
      { 0x9f, "Original" },
      { 0xa0, "Performing Arts" },
      { 0xa1, "Pets/Animals" },
      { 0xa2, "Pop" },
      { 0xa3, "Rock & Roll" },
      { 0xa4, "Sci-Fi" },
      { 0xa5, "Self Improvement" },
      { 0xa6, "Sitcom" },
      { 0xa7, "Skating" },
      { 0xa8, "Skiing" },
      { 0xa9, "Soccer" },
      { 0xaa, "Track/Field" },
      { 0xab, "True" },
      { 0xac, "Volleyball" },
      { 0xad, "Wrestling" }
    };

    #endregion

    private IGrabberEpgAtsc _grabberAtsc = null;
    private IGrabberEpgScte _grabberScte = null;

    /// <summary>
    /// The current transmitter tuning detail.
    /// </summary>
    private IChannel _currentTuningDetail = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgGrabberAtsc"/> class.
    /// </summary>
    /// <param name="grabberAtsc">The ATSC EPG grabber, if available/supported.</param>
    /// <param name="grabberScte">The SCTE EPG grabber, if available/supported.</param>
    public EpgGrabberAtsc(IGrabberEpgAtsc grabberAtsc, IGrabberEpgScte grabberScte)
    {
      _grabberAtsc = grabberAtsc;
      _grabberScte = grabberScte;
    }

    private static string GetAtscGenreDescription(byte genreId)
    {
      string description;
      if (!MAPPING_ATSC_GENRES.TryGetValue(genreId, out description))
      {
        description = string.Format("User Defined {0}", genreId);
      }
      return description;
    }

    #region IEpgGrabberInternal member

    /// <summary>
    /// Reload the grabber's configuration.
    /// </summary>
    /// <param name="configuration">The configuration of the associated tuner.</param>
    public void ReloadConfiguration(Tuner configuration)
    {
      // TODO
    }

    /// <summary>
    /// The tuner implementation invokes this method when it tunes to a
    /// different transmitter.
    /// </summary>
    /// <param name="tuningDetail">The transmitter tuning detail.</param>
    public void OnTune(IChannel tuningDetail)
    {
      // TODO problem - shouldn't grab unless the tuning detail grab flag is set
    }

    #endregion

    #region IEpgGrabber members

    public void ReloadConfiguration()
    {
      throw new System.NotImplementedException();
    }

    public bool IsGrabbing
    {
      get { throw new System.NotImplementedException(); }
    }

    public void GrabEpg(IEpgGrabberCallBack callBack)
    {
      throw new System.NotImplementedException();
    }

    public void AbortGrabbing()
    {
      throw new System.NotImplementedException();
    }

    #endregion

    #region IEpgCallBack member

    public int OnEpgReceived()
    {
      throw new System.NotImplementedException();
    }

    #endregion

    private IDictionary<IChannel, IList<EpgProgram>> CollectData(IGrabberEpgAtsc grabber)
    {
      uint eventCount = _grabberAtsc.GetEventCount();
      this.LogDebug("EPG ATSC: initial event count = {0}", eventCount);
      IDictionary<ushort, List<EpgProgram>> channels = new Dictionary<ushort, List<EpgProgram>>(100);

      const byte ARRAY_SIZE_AUDIO_LANGUAGES = 20;
      const byte ARRAY_SIZE_CAPTIONS_LANGUAGES = 20;
      const byte ARRAY_SIZE_GENRE_IDS = 20;
      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_DESCRIPTION = 1000;
      IntPtr bufferDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_DESCRIPTION);
      try
      {
        ushort sourceId;
        ushort eventId;
        ulong startDateTimeEpoch;
        ushort duration;
        byte textCount;
        Iso639Code[] audioLanguages = new Iso639Code[ARRAY_SIZE_AUDIO_LANGUAGES];
        Iso639Code[] captionsLanguages = new Iso639Code[ARRAY_SIZE_CAPTIONS_LANGUAGES];
        byte[] genreIds = new byte[ARRAY_SIZE_GENRE_IDS];
        byte vchipRating;
        byte mpaaClassification;
        ushort advisories;
        Iso639Code language;
        for (uint i = 0; i < eventCount; i++)
        {
          byte countAudioLanguages = ARRAY_SIZE_AUDIO_LANGUAGES;
          byte countCaptionsLanguages = ARRAY_SIZE_CAPTIONS_LANGUAGES;
          byte countGenreIds = ARRAY_SIZE_GENRE_IDS;
          bool result = grabber.GetEvent(i,
                                          out sourceId,
                                          out eventId,
                                          out startDateTimeEpoch,
                                          out duration,
                                          out textCount,
                                          audioLanguages,
                                          ref countAudioLanguages,
                                          captionsLanguages,
                                          ref countCaptionsLanguages,
                                          genreIds,
                                          ref countGenreIds,
                                          out vchipRating,
                                          out mpaaClassification,
                                          out advisories);
          if (!result)
          {
            this.LogWarn("EPG ATSC: failed to get event, event index = {0}, event count = {1}", i, eventCount);
            continue;
          }

          DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          programStartTime.AddSeconds(startDateTimeEpoch);
          programStartTime = programStartTime.ToLocalTime();
          EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));

          bool isPlaceholderOrDummyEvent = false;
          for (byte j = 0; j < textCount; j++)
          {
            ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
            ushort bufferSizeDescription = BUFFER_SIZE_DESCRIPTION;
            result = grabber.GetEventTextByIndex(i, j,
                                                  out language,
                                                  bufferTitle,
                                                  ref bufferSizeTitle,
                                                  bufferDescription,
                                                  ref bufferSizeDescription);
            if (!result)
            {
              this.LogWarn("EPG ATSC: failed to get event text, event index = {0}, event count = {1}, text index = {2}, text count = {3}", i, eventCount, j, textCount);
              continue;
            }

            string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
            if (string.IsNullOrEmpty(title))
            {
              isPlaceholderOrDummyEvent = true;
              continue;
            }
            program.Titles.Add(language.Code, title);

            string description = TidyString(DvbTextConverter.Convert(bufferDescription, bufferSizeDescription));
            if (!string.IsNullOrEmpty(description))
            {
              program.Descriptions.Add(language.Code, description);
            }
          }

          if (isPlaceholderOrDummyEvent)
          {
            continue;
          }

          for (byte x = 0; x < countAudioLanguages; x++)
          {
            program.AudioLanguages.Add(audioLanguages[x].Code);
          }
          for (byte x = 0; x < countCaptionsLanguages; x++)
          {
            program.SubtitlesLanguages.Add(captionsLanguages[x].Code);
          }
          for (byte x = 0; x < countGenreIds; x++)
          {
            string genreDescription;
            if (MAPPING_ATSC_GENRES.TryGetValue(genreIds[x], out genreDescription))
            {
              program.Categories.Add(genreDescription);
            }
          }
          string mpaaClassificationDescription = GetMpaaClassificationDescription(mpaaClassification);
          if (mpaaClassificationDescription != null)
          {
            program.Classifications.Add("MPAA", mpaaClassificationDescription);
          }
          program.Advisories = GetContentAdvisories(advisories);
          string vchipRatingDescription = GetVchipRatingDescription(vchipRating);
          if (vchipRatingDescription != null)
          {
            program.Classifications.Add("V-Chip", vchipRatingDescription);
          }

          List<EpgProgram> programs;
          if (!channels.TryGetValue(sourceId, out programs))
          {
            programs = new List<EpgProgram>(100);
            channels.Add(sourceId, programs);
          }
          programs.Add(program);
        }

        IDictionary<IChannel, IList<EpgProgram>> epgChannels = new Dictionary<IChannel, IList<EpgProgram>>(channels.Count);
        int validEventCount = 0;
        foreach (var channel in channels)
        {
          IChannel atscScteChannel = _currentTuningDetail.Clone() as IChannel;
          ChannelAtsc atscChannel = atscScteChannel as ChannelAtsc;
          if (atscChannel != null)
          {
            atscChannel.SourceId = channel.Key;
          }
          else
          {
            ChannelScte scteChannel = atscScteChannel as ChannelScte;
            if (scteChannel != null)
            {
              scteChannel.SourceId = channel.Key;
            }
            else
            {
              this.LogWarn("EPG ATSC: the tuned channel is not an ATSC or SCTE channel");
              continue;
            }
          }
          channel.Value.Sort();
          epgChannels.Add(atscScteChannel, channel.Value);
          validEventCount += channel.Value.Count;
        }

        this.LogDebug("EPG ATSC: channel count = {0}, event count = {1}", channels.Count, validEventCount);
        return epgChannels;
      }
      finally
      {
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferDescription);
        }
      }
    }

    private static string TidyString(string s)
    {
      if (s == null)
      {
        return string.Empty;
      }
      return s.Trim();
    }

    private static string GetMpaaClassificationDescription(byte classification)
    {
      // Note: the ATSC/SCTE RRT encoding differs from the Dish/BEV encoding.
      switch (classification)
      {
        case 1:
          return "G";       // general
        case 2:
          return "PG";      // parental guidance
        case 3:
          return "PG-13";   // parental guidance under 13
        case 4:
          return "R";       // restricted
        case 5:
          return "NC-17";   // nobody 17 and under
        case 6:
          return "X";      // not rated
        case 7:
          return "NR";      // not rated
      }
      return null;
    }

    private static ContentAdvisory GetContentAdvisories(ushort advisories)
    {
      ContentAdvisory advisoryFlags = ContentAdvisory.None;
      if ((advisories & 0x01) != 0)
      {
        advisoryFlags |= ContentAdvisory.SexualSituations;
      }
      if ((advisories & 0x02) != 0)
      {
        advisoryFlags |= ContentAdvisory.CourseOrCrudeLanguage;
      }
      if ((advisories & 0x04) != 0)
      {
        advisoryFlags |= ContentAdvisory.MildSensuality;
      }
      if ((advisories & 0x08) != 0)
      {
        advisoryFlags |= ContentAdvisory.FantasyViolence;
      }
      if ((advisories & 0x10) != 0)
      {
        advisoryFlags |= ContentAdvisory.Violence;
      }
      if ((advisories & 0x20) != 0)
      {
        advisoryFlags |= ContentAdvisory.MildPeril;
      }
      if ((advisories & 0x40) != 0)
      {
        advisoryFlags |= ContentAdvisory.Nudity;
      }
      if ((advisories & 0x8000) != 0)
      {
        advisoryFlags |= ContentAdvisory.SuggestiveDialogue;
      }
      return advisoryFlags;
    }

    private static string GetVchipRatingDescription(byte rating)
    {
      switch (rating)
      {
        case 1:
          return "TV-Y";    // all children
        case 2:
          return "TV-Y7";   // children 7 and older
        case 3:
          return "TV-G";    // general audience
        case 4:
          return "TV-PG";   // parental guidance
        case 5:
          return "TV-14";   // adults 14 and older
        case 6:
          return "TV-MA";   // mature audience
      }
      return null;
    }
  }
}