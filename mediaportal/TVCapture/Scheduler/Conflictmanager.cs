/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.TV.Recording
{
  public class ConflictManager
  {
    static int[] cards;
    static List<TVRecording> _recordings;
    static TVUtil _util = null;
    static List<TVRecording> _conflictingRecordings = null;

    public delegate void OnConflictsUpdatedHandler();
    static public event OnConflictsUpdatedHandler OnConflictsUpdated = null;
    static ConflictManager()
    {
      TVDatabase.OnRecordingsChanged += new MediaPortal.TV.Database.TVDatabase.OnRecordingChangedHandler(TVDatabase_OnRecordingsChanged);
    }
    static bool AllocateCard(string ChannelName)
    {
      int cardNo = -1;
      int minRecs = Int32.MaxValue;
      for (int i = 0; i < cards.Length; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (!dev.UseForRecording) continue;
        if (cards.Length > 1)
        {
          if (!TVDatabase.CanCardViewTVChannel(ChannelName, dev.ID)) continue;
        }
        if (cards[i] == 0)
        {
          cardNo = i;
          break;
        }
        if (cards[i] < minRecs)
        {
          minRecs = cards[i];
          cardNo = i;
        }
      }
      if (cardNo >= 0)
      {
        cards[cardNo]++;
        //_log.Info("  card:{0} {1} {2}",cardNo,cards[cardNo], ChannelName);
        if (cards[cardNo] > 1) return true;
      }
      return false;
    }

    static void FreeCards()
    {
      for (int i = 0; i < cards.Length; ++i)
        cards[i] = 0;
    }

    static void Initialize()
    {
      _util = new TVUtil(14);
      _recordings = new List<TVRecording>();
      _conflictingRecordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref _recordings);
      Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));
      WorkerThread.Start();
    }

    static void WorkerThreadFunction()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
      DateTime dtStart = DateTime.Now;
      foreach (TVRecording rec in _recordings)
      {
        DetermineIsConflict(rec);
      }
      TimeSpan ts = DateTime.Now - dtStart;
      if (OnConflictsUpdated != null)
        OnConflictsUpdated();
    }

    static public bool IsConflict(TVRecording rec)
    {
      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      foreach (TVRecording conflict in _conflictingRecordings)
      {
        if (conflict.ID == rec.ID) return true;
      }
      return false;
    }
    static bool DetermineIsConflict(TVRecording rec)
    {
      if (Recorder.Count <= 0)
        return false;
      if (rec.Canceled > 0 || rec.IsDone()) return false;

      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      cards = new int[Recorder.Count];
      if (_recordings.Count == 0) return false;
      List<TVRecording> episodes = _util.GetRecordingTimes(rec);
      foreach (TVRecording episode in episodes)
      {
        if (episode.Canceled != 0) continue;
        FreeCards();
        AllocateCard(episode.Channel);
        foreach (TVRecording otherRecording in _recordings)
        {
          List<TVRecording> otherEpisodes = _util.GetRecordingTimes(otherRecording);
          foreach (TVRecording otherEpisode in otherEpisodes)
          {
            if (otherEpisode.Canceled != 0) continue;
            if (otherEpisode.ID == episode.ID &&
              otherEpisode.Start == episode.Start &&
              otherEpisode.End == episode.End) continue;
            // episode        s------------------------e
            // other    ---------s-----------------------------
            // other ------------------e
            if ((otherEpisode.Start >= episode.Start && otherEpisode.Start < episode.End) ||
                 (otherEpisode.Start <= episode.Start && otherEpisode.End >= episode.End) ||
              (otherEpisode.End > episode.Start && otherEpisode.End <= episode.End))
            {
              if (AllocateCard(otherEpisode.Channel))
              {
                _conflictingRecordings.Add(rec);
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    static public void GetConflictingSeries(TVRecording rec, List<TVRecording> recSeries)
    {
      recSeries.Clear();
      if (Recorder.Count <= 0) return;

      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      cards = new int[Recorder.Count];
      if (_recordings.Count == 0) return;

      List<TVRecording> episodes = _util.GetRecordingTimes(rec);
      foreach (TVRecording episode in episodes)
      {
        if (episode.Canceled != 0) continue;

        bool epsiodeConflict = false;
        FreeCards();
        AllocateCard(episode.Channel);
        foreach (TVRecording otherRecording in _recordings)
        {
          List<TVRecording> otherEpisodes = _util.GetRecordingTimes(otherRecording);
          foreach (TVRecording otherEpisode in otherEpisodes)
          {
            if (otherEpisode.Canceled != 0) continue;
            if (otherEpisode.ID == episode.ID &&
              otherEpisode.Start == episode.Start &&
              otherEpisode.End == episode.End) continue;
            // episode        s------------------------e
            // other    ---------s-----------------------------
            // other ------------------e
            if ((otherEpisode.Start >= episode.Start && otherEpisode.Start < episode.End) ||
              (otherEpisode.Start <= episode.Start && otherEpisode.End >= episode.End) ||
              (otherEpisode.End > episode.Start && otherEpisode.End <= episode.End))
            {
              if (AllocateCard(otherEpisode.Channel))
              {
                epsiodeConflict = true;
                break;
              }
            }
          }
          if (epsiodeConflict) break;
        }
        if (epsiodeConflict)
        {
          recSeries.Add(episode);
        }
      }
    }

    static public TVRecording[] GetConflictingRecordings(TVRecording episode)
    {
      if (Recorder.Count <= 0) return null;

      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      cards = new int[Recorder.Count];
      if (_recordings.Count == 0) return null;

      List<TVRecording> conflicts = new List<TVRecording>();
      if (episode.Canceled != 0) return null;

      foreach (TVRecording otherRecording in _recordings)
      {
        List<TVRecording> otherEpisodes = _util.GetRecordingTimes(otherRecording);
        foreach (TVRecording otherEpisode in otherEpisodes)
        {
          if (otherEpisode.Canceled != 0) continue;
          if (otherEpisode.ID == episode.ID &&
            otherEpisode.Start == episode.Start &&
            otherEpisode.End == episode.End) continue;
          // episode        s------------------------e
          // other    ---------s-----------------------------
          // other ------------------e
          if ((otherEpisode.Start >= episode.Start && otherEpisode.Start < episode.End) ||
            (otherEpisode.Start <= episode.Start && otherEpisode.End >= episode.End) ||
            (otherEpisode.End > episode.Start && otherEpisode.End <= episode.End))
          {
            conflicts.Add(otherEpisode);
          }
        }
      }
      TVRecording[] conflictingRecordings = new TVRecording[conflicts.Count];
      for (int i = 0; i < conflicts.Count; ++i)
        conflictingRecordings[i] = (TVRecording)conflicts[i];
      return conflictingRecordings;
    }

    static public TVUtil Util
    {
      get
      {
        if (_util == null)
          Initialize();
        return _util;
      }
    }

    static private void TVDatabase_OnRecordingsChanged(TVDatabase.RecordingChange change)
    {
      if (change == TVDatabase.RecordingChange.Added ||
        change == TVDatabase.RecordingChange.CanceledSerie ||
        change == TVDatabase.RecordingChange.Canceled ||
        change == TVDatabase.RecordingChange.Deleted ||
        change == TVDatabase.RecordingChange.Modified)
      {
        Initialize();
      }
    }
  }
}