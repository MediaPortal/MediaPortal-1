#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using Gentle.Framework;
using MediaPortal.WebEPG.Parser;
using TvDatabase;
using System.Threading;
using TvLibrary.Log;
using MediaPortal.Utils.Time;

namespace MediaPortal.WebEPG
{
  class DatabaseEPGDataSink
    : IEpgDataSink
  {
    #region Types
    
    struct DateTimeRange
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

    bool _deleteOverlapping = false;
    Dictionary<string, List<Channel>> _channels;
    List<Channel> _currentChannels;
    ProgramList _channelPrograms;
    TimeRange _timeWindow;

    TvBusinessLayer layer = new TvBusinessLayer();

    #endregion

    #region ctor

    public DatabaseEPGDataSink()
    {
    }

    public DatabaseEPGDataSink(bool deleteOverlapping)
    {
      _deleteOverlapping = deleteOverlapping;
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
        DateTime windowEnd = new DateTime(prog.StartTime.Year, prog.StartTime.Month, prog.StartTime.Day, _timeWindow.End.Hour, _timeWindow.End.Minute, 0);
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
        //DateTimeRange fullRange = new DateTimeRange(_channelPrograms[0].StartTime,
        //                                            _channelPrograms[_channelPrograms.Count - 1].EndTime);
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
      _channels = new Dictionary<string, List<Channel>>();
    }

    void IEpgDataSink.Close()
    {
    }

    void IEpgDataSink.WriteChannel(string id, string name)
    {
      string channelKey = name + "-" + id;
      if (!_channels.ContainsKey(channelKey))
      {
        try
        {
          Key dbKey = new Key(false, "displayName", name);
          //Channel dbChannel = Broker.TryRetrieveInstance<Channel>(dbKey);
          List<Channel> dbChannels = (List<Channel>)Broker.RetrieveList<Channel>(dbKey);
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
        _channelPrograms = new ProgramList();
        _channelPrograms.AlreadySorted = false;
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
        foreach(Channel chan in _currentChannels)
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

      if (_deleteOverlapping)
      {
        // Remove programs from DB ending after the first imported program
        SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
        sb.AddConstraint(Operator.In, "idChannel", _currentChannels, "IdChannel");
        sb.AddConstraint(string.Format("((endTime > {0}rangeStart{1} AND startTime < {0}rangeEnd{1}) OR (startTime = endTime AND startTime BETWEEN {0}rangeStart{1} AND {0}rangeEnd{1}))", sb.ParameterPrefix, sb.ParameterSuffix));
        sb.AddParameter("rangeStart", typeof(DateTime));
        sb.AddParameter("rangeEnd", typeof(DateTime));
        SqlStatement stmt = sb.GetStatement(true);
        List<DateTimeRange> ranges = GetGrabbedDateTimeRanges();
        foreach(var range in ranges)
        {
          Log.Info("Removing programs between {0} and {1}", range.Start, range.End);
          stmt.SetParameter("rangeStart", range.Start);
          stmt.SetParameter("rangeEnd", range.End);
          stmt.Execute();  
        }
      }
      else
      {
        // Remove programs overlapping ones in DB:
        // First retrieve all programs for current channels 
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
        sb.AddConstraint(Operator.In, "idChannel", _currentChannels, "IdChannel");
        //sb.AddConstraint(Operator.Equals, "idChannel", _currentChannels.IdChannel);
        //sb.AddOrderByField(false, "starttime");
        SqlStatement stmt = sb.GetStatement(true);
        ProgramList dbPrograms = new ProgramList(ObjectFactory.GetCollection<Program>(stmt.Execute()));

        _channelPrograms.RemoveOverlappingPrograms(dbPrograms);
      }
      foreach (Channel chan in _currentChannels)
      {
        layer.RemoveOldPrograms(chan.IdChannel);
      }

      layer.InsertPrograms(_channelPrograms, ThreadPriority.BelowNormal);
      
      //_channelPrograms.Clear();
      _channelPrograms = null;
      _currentChannels = null;
    }

    #endregion
  }
}
