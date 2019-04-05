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
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Program
  {
    public string GetRecordingFileName(string template, string channelName)
    {
      if (string.IsNullOrEmpty(template))
      {
        template = "%program_title%";
      }
      if (channelName == null)
      {
        channelName = "Unknown Channel";
      }
      else
      {
        channelName = channelName.Trim();
      }

      // Limit length of title and episode name to try to ensure the file name
      // length is kept within limits (eg. MAX_PATH).
      string programTitle = Title;
      if (programTitle.Length > 80)
      {
        programTitle = programTitle.Substring(0, 77) + "...";
      }
      string programEpisodeName = EpisodeName ?? string.Empty;
      if (programEpisodeName.Length > 80)
      {
        programEpisodeName = programEpisodeName.Substring(0, 77) + "...";
      }

      string programCategory = string.Empty;
      if (ProgramCategory != null)
      {
        programCategory = ProgramCategory.Category.Trim();
      }

      Dictionary<string, string> tags = new Dictionary<string, string>
      {
        { "%program_title%", programTitle },
        { "%episode_name%", programEpisodeName },
        { "%series_number%", SeasonNumber.ToString() },
        { "%episode_number%", EpisodeNumber.ToString() },
        { "%episode_part%", EpisodePartNumber.ToString() },
        { "%channel_name%", channelName },
        { "%genre%", programCategory },
        { "%date%", StartTime.ToString("yyyy-MM-dd") },
        { "%start%", StartTime.ToShortTimeString() },
        { "%end%", EndTime.ToShortTimeString() },
        { "%start_year%", StartTime.ToString("yyyy") },
        { "%start_month%", StartTime.ToString("MM") },
        { "%start_day%", StartTime.ToString("dd") },
        { "%start_hour%", StartTime.ToString("HH") },
        { "%start_minute%", StartTime.ToString("mm") },
        { "%end_year%", EndTime.ToString("yyyy") },
        { "%end_month%", EndTime.ToString("MM") },
        { "%end_day%", EndTime.ToString("dd") },
        { "%end_hour%", EndTime.ToString("HH") },
        { "%end_minute%", EndTime.ToString("mm") }
      };

      foreach (var tag in tags)
      {
        template = ReplaceTag(template, tag.Key, tag.Value);
        if (!template.Contains("%"))
        {
          break;
        }
      }

      template = template.Trim();
      if (string.IsNullOrEmpty(template))
      {
        template = string.Format("{0}_{1}_{2}", channelName, programTitle, StartTime.ToString("yyyy-MM-dd_HHmm"));
      }
      return template;
    }

    private static string ReplaceTag(string line, string tag, string value)
    {
      value = value.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');

      // This regex checks for optional sections of the form [*%tag%*].
      // Nesting (eg. "Hello [%tag1% [%tag2% ]]world!") is not supported.
      string regexPattern = string.Format(@"\[[^%]*{0}[^\]]*[\]]", tag.Replace("%", "\\%"));
      Regex r;
      try
      {
        r = new Regex(regexPattern);
      }
      catch
      {
        return line;
      }

      Match match = r.Match(line);
      if (match.Success)  // means there are one or more optional sections
      {
        // Remove the entire optional section. If the tag has a value, reinsert
        // the section without the square braces.
        line = line.Remove(match.Index, match.Length);
        if (!string.IsNullOrEmpty(value))
        {
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }

      // Replace tags.
      return line.Replace(tag, value);
    }

    #region program state

    private void SetFlag(ProgramState flag, bool isSet)
    {
      ProgramState newState = (ProgramState)State;
      if (!isSet && newState.HasFlag(flag))
      {
        newState ^= flag;
        State = (int)newState;
      }
      else if (isSet && !newState.HasFlag(flag))
      {
        newState |= flag;
        State = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column notify
    /// </summary>
    public bool Notify
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.Notify); }
      set { SetFlag(ProgramState.Notify, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingOnce
    /// </summary>
    public bool IsRecordingOnce
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.RecordOnce); }
      set { SetFlag(ProgramState.RecordOnce, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingSeriesPending
    /// </summary>
    public bool IsRecordingSeriesPending
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.RecordSeriesPending); }
      set { SetFlag(ProgramState.RecordSeriesPending, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingSeriesPending
    /// </summary>
    public bool IsPartialRecordingSeriesPending
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.PartialRecordSeriesPending); }
      set { SetFlag(ProgramState.PartialRecordSeriesPending, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingOncePending
    /// </summary>
    public bool IsRecordingOncePending
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.RecordOncePending); }
      set { SetFlag(ProgramState.RecordOncePending, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingManual
    /// </summary>
    public bool IsRecordingManual
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.RecordManual); }
      set { SetFlag(ProgramState.RecordManual, value); }
    }

    /// <summary>
    /// Property relating to database column isRecording
    /// </summary>
    public bool IsRecordingSeries
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.RecordSeries); }
      set { SetFlag(ProgramState.RecordSeries, value); }
    }

    /// <summary>
    /// Property relating to database column conflict
    /// </summary>
    public bool HasConflict
    {
      get { return ((ProgramState)State).HasFlag(ProgramState.Conflict); }
      set { SetFlag(ProgramState.Conflict, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecording
    /// </summary>
    public bool IsRecording
    { // This should reflect whether program is actually recording right now
      get { return (IsRecordingSeries || IsRecordingManual || IsRecordingOnce); }
    }

    public void ClearRecordPendingState()
    {
      State &=
        ~(int)
         (ProgramState.RecordOncePending | ProgramState.RecordSeriesPending | ProgramState.PartialRecordSeriesPending);
    }

    #endregion

    /// <summary>
    /// Checks if the program ended prior to the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program ended prior to tCurTime</returns>
    public bool EndedBefore(DateTime tCurTime)
    {
      return EndTime <= tCurTime;
    }

    /// <summary>
    /// Checks if the program is running between the specified start and end time/dates, i.e. whether the intervals overlap
    /// </summary>
    /// <param name="tStartTime">Start date and time</param>
    /// <param name="tEndTime">End date and time</param>
    /// <returns>true if program is running between tStartTime-tEndTime</returns>
    public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
    {
      // do NOT use <= >= since end-times are non-including
      return tStartTime < EndTime && tEndTime > StartTime;
    }

    /// <summary>
    /// Checks if the program is running at the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program is running at tCurTime</returns>
    public bool IsRunningAt(DateTime tCurTime)
    {
      return tCurTime >= StartTime && tCurTime <= EndTime;
    }
  }
}