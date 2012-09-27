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
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TvLibrary.Utils.Time;
using System.Threading;
using WebEPG.Parser;
using WebEPG.Utils;

namespace Mediaportal.TV.Server.Plugins.WebEPGImport
{
  internal class DatabaseEPGDataSink
    : IEpgDataSink
  {
    #region Types

    private struct DateTimeRange
    {
      public DateTime Start;
      public DateTime End;

      public DateTimeRange(DateTime start, DateTime end)
      {
        Start = start;
        End = end;
      }
    }

    #endregion

    #region Variables

    private bool _deleteExisting;
    private Dictionary<string, IList<Channel>> _channels;
    private IList<Channel> _currentChannels;
    private ProgramList _channelPrograms;
    private TimeRange _timeWindow;

    private ProgramManagement _programManagement = new ProgramManagement();

    #endregion

    #region ctor

    public DatabaseEPGDataSink() {}

    public DatabaseEPGDataSink(bool deleteExisting)
    {
      _deleteExisting = deleteExisting;
    }

    #endregion

    #region Public properties

    public ProgramList ChannelPrograms
    {
      get { return _channelPrograms; }
      set { _channelPrograms = value; }
    }

    #endregion

    #region Private members

    private void ClipProgramsToWindow()
    {
      if (_timeWindow == null)
        return;
      _channelPrograms.SortIfNeeded();
      for (int i = 0; i < _channelPrograms.Count; ++i)
      {
        Program prog = _channelPrograms[i];
        DateTime windowEnd = new DateTime(prog.StartTime.Year, prog.StartTime.Month, prog.StartTime.Day,
                                          _timeWindow.End.Hour, _timeWindow.End.Minute, 0);
        if (windowEnd < prog.StartTime)
        {
          windowEnd = windowEnd.AddDays(1);
        }
        if (prog.EndTime > windowEnd)
        {
          prog.EndTime = windowEnd;
        }
      }
    }

    private List<DateTimeRange> GetGrabbedDateTimeRanges()
    {
      List<DateTimeRange> ranges = new List<DateTimeRange>();
      if (_channelPrograms.Count != 0)
      {
        _channelPrograms.SortIfNeeded();
        // First implementation: scan programs
        DateTimeRange range = new DateTimeRange(_channelPrograms[0].StartTime, _channelPrograms[0].EndTime);
        for (int i = 1; i < _channelPrograms.Count; i++)
        {
          Program currProg = _channelPrograms[i];
          if (range.End.Equals(currProg.StartTime))
          {
            range.End = currProg.EndTime;
          }
          else
          {
            ranges.Add(range);
            range = new DateTimeRange(currProg.StartTime, currProg.EndTime);
          }
        }
        ranges.Add(range);
        // Alternate implementation: use time window 
        //DateTimeRange fullRange = new DateTimeRange(_channelPrograms[0].startTime,
        //                                            _channelPrograms[_channelPrograms.Count - 1].endTime);
        //if (_timeWindow == null)
        //{
        //  ranges.Add(fullRange);
        //}
        //else
        //{

        //  range =
        //    new DateTimeRange(
        //      new DateTime(fullRange.Start.Year, fullRange.Start.Month, fullRange.Start.Day, _timeWindow.Start.Hour,
        //                   _timeWindow.Start.Minute, 0),
        //      new DateTime(fullRange.Start.Year, fullRange.Start.Month, fullRange.Start.Day, _timeWindow.End.Hour,
        //                   _timeWindow.End.Minute, 0));
        //  if (range.End<range.Start)
        //  {
        //    range.End.AddDays(1);
        //  }
        //  while(range.Start < fullRange.End)
        //  {
        //    ranges.Add(range);
        //    range.Start.AddDays(1);
        //    range.End.AddDays(1);
        //  }
        //}
      }
      return ranges;
    }

    #endregion

    #region IEPGDataSink implementation

    void IEpgDataSink.Open()
    {
      _channels = new Dictionary<string, IList<Channel>>();
    }

    void IEpgDataSink.Close()
    {
      Log.Info("WebEPG: Waiting for database to be updated...");
      ProgramManagement.InitiateInsertPrograms();
      Log.Info("WebEPG: Database update finished.");
    }

    void IEpgDataSink.WriteChannel(string id, string name)
    {
      string channelKey = name + "-" + id;
      if (!_channels.ContainsKey(channelKey))
      {
        try
        {
          IList<Channel> dbChannels = ChannelManagement.GetChannelsByName(name);          
          if (dbChannels.Count > 0)
          {
            _channels.Add(channelKey, dbChannels);
          }
        }
        catch (Exception ex)
        {
          Log.Error("WebEPG: failed to retrieve channels with display name '{0}':", name);
          Log.Write(ex);
        }
      }
    }

    bool IEpgDataSink.StartChannelPrograms(string id, string name)
    {
      string channelKey = name + "-" + id;

      _timeWindow = null;

      if (_channels.TryGetValue(channelKey, out _currentChannels))
      {
        _channelPrograms = new ProgramList {AlreadySorted = false};
        return true;
      }
      else
      {
        _currentChannels = null;
        Log.Info("WebEPG: Unknown channel (display name = {0}, channel id = {1}) - skipping...", name, id);
        return false;
      }
    }

    void IEpgDataSink.SetTimeWindow(TimeRange window)
    {
      _timeWindow = window;
    }

    void IEpgDataSink.WriteProgram(ProgramData programData, bool merged)
    {
      if (_currentChannels != null)
      {
        foreach (Channel chan in _currentChannels)
        {
          _channelPrograms.Add(programData.ToTvProgram(chan.IdChannel));
        }
      }
    }

    void IEpgDataSink.EndChannelPrograms(string id, string name)
    {
      if (_currentChannels == null) return;
      if (_channelPrograms.Count == 0) return;

      // Sort programs
      _channelPrograms.SortIfNeeded();
      _channelPrograms.AlreadySorted = true;

      // Fix end times
      _channelPrograms.FixEndTimes();
      ClipProgramsToWindow();

      // Remove overlapping programs
      _channelPrograms.RemoveOverlappingPrograms();

      if (!_deleteExisting)
      {
        // Remove programs overlapping ones in DB:
        // First retrieve all programs for current channels                         
        var dbPrograms = new ProgramList(ProgramManagement.GetProgramsForAllChannels(_currentChannels));
        _channelPrograms.RemoveOverlappingPrograms(dbPrograms);
      }
      foreach (Channel chan in _currentChannels)
      {        
        ProgramManagement.DeleteOldPrograms(chan.IdChannel);
      }

      DeleteBeforeImportOption programsToDelete = _deleteExisting
                                                    ? DeleteBeforeImportOption.OverlappingPrograms
                                                    : DeleteBeforeImportOption.None;

      var prgManagement = new ProgramManagement();
      prgManagement.InsertPrograms(_channelPrograms, programsToDelete, ThreadPriority.BelowNormal);

      _channelPrograms = null;
      _currentChannels = null;
    }

    #endregion
  }
}