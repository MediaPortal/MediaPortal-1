#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
    public static event OnConflictsUpdatedHandler OnConflictsUpdated = null;
    static ConflictManager()
    {
      TVDatabase.OnRecordingsChanged += new MediaPortal.TV.Database.TVDatabase.OnRecordingChangedHandler(TVDatabase_OnRecordingsChanged);
    }

    static void Initialize()
    {
      Log.Info("ConflictManager.Initialize()");
      _util = new TVUtil(14);
      _recordings = new List<TVRecording>();
      _conflictingRecordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref _recordings);
      Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));
      WorkerThread.Name = "ConflictManager";
      WorkerThread.IsBackground = true;
      WorkerThread.Start();
    }

    static void WorkerThreadFunction()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
      // We'll use a clone of _recordings instead of _recordings itself
      // to avoid it to be modified while enumerating and using it (raised an exception for at least one user)
      List<TVRecording> _temprecordings = new List<TVRecording>(_recordings); 
      foreach (TVRecording rec in _temprecordings)
      {
        DetermineIsConflict(rec);
      }

      if (OnConflictsUpdated != null)
        OnConflictsUpdated();
    }

    public static bool IsConflict(TVRecording rec)
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
      //Log.Info("ConflictManager.DetermineisConflict");
      if (Recorder.Count <= 0)
        return false;
      if (rec.Canceled > 0 || rec.IsDone()) return false;

      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      List<TVRecording>[] _cardrecordings = new List<TVRecording>[Recorder.Count];
      for (int i = 0; i < Recorder.Count; i++) _cardrecordings[i] = new List<TVRecording>();
      if (_recordings.Count == 0) return false;
      List<TVRecording> episodes = _util.GetRecordingTimes(rec);
      foreach (TVRecording episode in episodes)
      {
        if (episode.Canceled != 0) continue;
        freeCardsRecordings(_cardrecordings);
        //Log.Info("Trying to assign Rec {0} - {1} on channel {2}", episode.Start.ToString(), episode.End.ToString(), episode.Channel);
        AssignRecToCard(episode, _cardrecordings);
        foreach (TVRecording otherRecording in _recordings)
        {
          List<TVRecording> otherEpisodes = _util.GetRecordingTimes(otherRecording);
          foreach (TVRecording otherEpisode in otherEpisodes)
          {
            if (otherEpisode.Canceled != 0) continue;
            if (isSameRecordings(episode, otherEpisode)) continue;

            if (IsOverlap(episode, otherEpisode))
            {
              //Log.Info("Overlapping : Trying to assign Rec {0} - {1} on channel {2}", episode.Start.ToString(), episode.End.ToString(), episode.Channel);
              if (!AssignRecToCard(otherEpisode, _cardrecordings))
              {
                //Log.Info("Added conflicting recording  {0} - {1} on channel {2}", episode.Start.ToString(), episode.End.ToString(), episode.Channel);
                _conflictingRecordings.Add(rec);
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    public static void GetConflictingSeries(TVRecording rec, List<TVRecording> recSeries)
    {
      recSeries.Clear();
      if (Recorder.Count <= 0) return;

      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      List<TVRecording>[] _cardrecordings = new List<TVRecording>[Recorder.Count];
      for (int i = 0; i < Recorder.Count; i++) _cardrecordings[i] = new List<TVRecording>();
      if (_recordings.Count == 0) return;

      List<TVRecording> episodes = _util.GetRecordingTimes(rec);
      foreach (TVRecording episode in episodes)
      {
        if (episode.Canceled != 0) continue;

        bool epsiodeConflict = false;
        freeCardsRecordings(_cardrecordings);
        AssignRecToCard(episode, _cardrecordings);
        foreach (TVRecording otherRecording in _recordings)
        {
          List<TVRecording> otherEpisodes = _util.GetRecordingTimes(otherRecording);
          foreach (TVRecording otherEpisode in otherEpisodes)
          {
            if (otherEpisode.Canceled != 0) continue;
            if (isSameRecordings(episode, otherEpisode)) continue;

            if (IsOverlap(episode, otherEpisode))
            {
              if (!AssignRecToCard(otherEpisode, _cardrecordings))
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

    public static void GetConflictingSeries2(TVRecording rec, List<TVRecording> recSeries)
    {
      recSeries.Clear();
      if (Recorder.Count <= 0) return;

      if (_recordings == null || _util == null)
      {
        Initialize();
      }
      List<TVRecording>[] _cardrecordings = new List<TVRecording>[Recorder.Count];
      for (int i = 0; i < Recorder.Count; i++) _cardrecordings[i] = new List<TVRecording>();
      if (_recordings.Count == 0) return;

      List<TVRecording> episodes = _util.GetRecordingTimes(rec);
      foreach (TVRecording episode in episodes)
      {
        if (episode.Canceled != 0) continue;

        freeCardsRecordings(_cardrecordings);
        AssignRecToCard(episode, _cardrecordings);
        foreach (TVRecording otherRecording in _recordings)
        {
          List<TVRecording> otherEpisodes = _util.GetRecordingTimes(otherRecording);
          foreach (TVRecording otherEpisode in otherEpisodes)
          {
            if (otherEpisode.Canceled != 0) continue;
            if (isSameRecordings(episode, otherEpisode)) continue;

            if (IsOverlap(episode, otherEpisode))
            {
              if (!AssignRecToCard(otherEpisode, _cardrecordings))
              {
                recSeries.Add(otherEpisode);
              }
            }
          }
        }
      }
    }


    public static TVRecording[] GetConflictingRecordings(TVRecording episode)
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
          if (isSameRecordings(episode, otherEpisode)) continue;

          if (IsOverlap(episode, otherEpisode))
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

    public static TVUtil Util
    {
      get
      {
        if (_util == null)
          Initialize();
        return _util;
      }
    }

    private static void TVDatabase_OnRecordingsChanged(TVDatabase.RecordingChange change)
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
    /// <summary>
    /// Tests if two recordings are overlapping
    /// </summary>
    /// <param name="record_1">A recording...</param>
    /// <param name="record_1">Another recording</param>
    /// <returns>true : overlapping, false : no overlapping</returns>
    private static bool IsOverlap(TVRecording record_1, TVRecording record_2)
    {
      DateTime Start1, Start2, End1, End2;

      Start1 = record_1.StartTime.AddMinutes(-record_1.PreRecord);
      Start2 = record_2.StartTime.AddMinutes(-record_2.PreRecord);
      End1 = record_1.EndTime.AddMinutes(record_1.PostRecord);
      End2 = record_2.EndTime.AddMinutes(record_2.PostRecord);

      // rec_1        s------------------------e
      // rec_2    ---------s-----------------------------
      // rec_2  ------------------e
      if ((Start2 >= Start1 && Start2 < End1) ||
          (Start2 <= Start1 && End2 >= End1) ||
          (End2 > Start1 && End2 <= End1)) return true;
      return false;
    }
    /// <summary>Tries to assign a recording to a card</summary>
    /// <param name="arec">The recording you wan't to try to assign</param>
    /// <param name="cardrec">An array of Recordings lists (one list for each card)</param>
    /// <returns>True if succeed, False either</returns>
    private static bool AssignRecToCard(TVRecording arec, List<TVRecording>[] cardrec)
    {
      int _cardscount = Recorder.Count;
      //Log.Info("Found {0} cards", _cardscount);
      if (_cardscount == 0) return false;
      int _currentcardNo = 0;
      for (_currentcardNo = 0; _currentcardNo < _cardscount; _currentcardNo++)
      {
        //Log.Info("Current Card", _currentcardNo);
        TVCaptureDevice dev = Recorder.Get(_currentcardNo);
        if (!dev.UseForRecording) continue;
        //Log.Info("Card {} is used for recording", _currentcardNo);
        if (!TVDatabase.CanCardViewTVChannel(arec.Channel, dev.ID))
        {
          //Log.Info("Card {0} cannot view {1}",_currentcardNo.ToString(),arec.Channel);
          continue;
        }
        bool _free = true;
        //Log.Info("Trying to assign : {0} - {1} on channel {3}  to  card {4}", arec.Start.ToString(), arec.End.ToString(), arec.Channel, _currentcardNo.ToString());
        foreach (TVRecording _inrec in cardrec[_currentcardNo])
        {
          if (IsOverlap(_inrec, arec) && !isSameRecordings(_inrec, arec)) _free = false;
          //Log.Info("IsOverloap: {0} ,  isSameRecording : {0}", IsOverlap(_inrec, arec), isSameRecordings(_inrec, arec));
        }
        if (_free)
        {
          cardrec[_currentcardNo].Add(arec);
          //Log.Info("Recording : {0} - {1} on channel {3}  allocated to card {4}", arec.Start.ToString(), arec.End.ToString(), arec.Channel, _currentcardNo.ToString());
          return true;
        }
      }
      //Log.Info("Could Not assign...");
      return false;
    }
    /// <summary>Clears an array of Recordings lists</summary>
    /// <param name="cardrec">An array of Recordings lists (one list for each card)</param>
    /// <returns>Nothing</returns>
    static void freeCardsRecordings(List<TVRecording>[] cardrec)
    {
      for (int i = 0; i < cardrec.Length; i++)
      {
        cardrec[i].Clear();
      }
    }
    /// <summary>Checks if two recording are the same</summary>
    /// <param name="record_1">First recording to test</param>
    /// <param name="record_2">Second recording to test</param>
    /// <returns>Nothing</returns>
    static bool isSameRecordings(TVRecording record_1, TVRecording record_2)
    {
      if (record_2.ID == record_1.ID &&
        record_2.Start == record_1.Start &&
        record_2.End == record_1.End) return true;
      return false;
    }

  }
}