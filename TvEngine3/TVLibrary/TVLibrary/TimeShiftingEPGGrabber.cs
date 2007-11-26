/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvDatabase;
using System.Collections.Specialized;

namespace TvLibrary
{
  class TimeShiftingEPGGrabber: BaseEpgGrabber
  {
    #region Variables
    private ITVCard _card;
    private System.Timers.Timer _epgTimer = new System.Timers.Timer();
    DateTime _grabStartTime;
    private List<EpgChannel> _epg;
    private bool _updateThreadRunning;
    private string _epgLanguages;
    private bool _storeOnlySelectedChannels;
    private string _titleTemplate;
    private string _descriptionTemplate;
    #endregion

    public TimeShiftingEPGGrabber(ITVCard card)
    {
      _card = card;
      TvBusinessLayer layer = new TvBusinessLayer();
      _epgLanguages = layer.GetSetting("epgLanguages").Value;
      _storeOnlySelectedChannels = (layer.GetSetting("epgStoreOnlySelected","no").Value=="yes");
      _grabStartTime = DateTime.Now;
      _epgTimer = new System.Timers.Timer();
      double timeout;
      if (!double.TryParse(layer.GetSetting("timeshiftingEpgGrabberTimeout", "2").Value, out timeout))
        timeout = 2;
      _epgTimer.Interval = timeout*60000;
      _epgTimer.Elapsed += new System.Timers.ElapsedEventHandler(_epgTimer_Elapsed);
      _updateThreadRunning = false;
      _titleTemplate = layer.GetSetting("epgTitleTemplate", "%TITLE%").Value;
      _descriptionTemplate = layer.GetSetting("epgDescriptionTemplate", "%DESCRIPTION%").Value;
    }
    public bool StartGrab()
    {
      if (_updateThreadRunning)
      {
        Log.Log.Info("Timeshifting epg grabber not started because the db update thread is still running.");
        return false;
      }
      else
      {
        Log.Log.Info("Timeshifting epg grabber started.");
        _epgTimer.Enabled = true;
        return true;
      }
    }
    void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      TimeSpan ts = DateTime.Now - _grabStartTime;
      if (ts.TotalMinutes > 2)
      {
        Log.Log.Epg("TimeshiftingEPG: timeout after {1} mins", ts.TotalMinutes);
        _epgTimer.Enabled = false;
        _card.AbortGrabbing();
      }
    }

    #region BaseEpgGrabber implementation
    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public void OnEpgCancelled()
    {
      Log.Log.Info("Timeshifting epg grabber stopped.");
      _card.IsEpgGrabbing = false;
      _epgTimer.Enabled = false;
    }
    /// <summary>
    /// Gets called when epg has been received
    /// Should be overriden by the class
    /// </summary>
    /// <returns></returns>
    public override int OnEpgReceived()
    {
      List<EpgChannel> grabbedEpg = null;
      try
      {
        grabbedEpg = _card.Epg;
      }
      catch (Exception)
      {
      }
      if (grabbedEpg == null)
      {
        Log.Log.Epg("TimeshiftingEPG: No epg received.");
        return 0;
      }
      _epg =new List<EpgChannel>(grabbedEpg);
      Log.Log.Epg("TimeshiftingEPG: OnEPGReceived got {0} channels",_epg.Count);
      if (_epg.Count == 0)
        Log.Log.Epg("TimeshiftingEPG: No epg received.");
      else
      {
        Thread workerThread = new Thread(new ThreadStart(UpdateDatabaseThread));
        workerThread.IsBackground = true;
        workerThread.Name = "EPG Update thread";
        workerThread.Start();
      }
      _epgTimer.Enabled = false;
      return 0;
    }
    #endregion

    #region Database update routines
    private int GetEPGOffset(List<EpgProgram> programs, DateTime newestEntry)
    {
      int off = 0;
      for (int i=0;i<programs.Count;i++)
      {
        if (programs[i].StartTime>=newestEntry)
        {
          off = i;
          break;
        }
      }
      return off;
    }
    private bool ProgramExists(IList dbPrograms, DateTime startTime, DateTime endTime)
    {
      foreach (Program prog in dbPrograms)
      {
        if (DateTime.Compare(prog.StartTime, startTime) == 0 && DateTime.Compare(prog.EndTime, endTime) == 0)
        {
          return true;
        }
      }
      return false;
    }
    private void GetEPGLanguage(List<EpgLanguageText> texts,out string title, out string description, out string genre,out int starRating,out string classification, out int parentalRating)
    {
      title = "";
      description = "";
      genre = "";
      starRating = 0;
      classification = "";
      
      parentalRating = -1;

      if (texts.Count != 0)
      {
        int offset = -1;
        for (int i = 0; i < texts.Count; ++i)
        {
          if (texts[0].Language.ToLower() == "all")
          {
            offset = i;
            break;
          }
          if (_epgLanguages.Length == 0 || _epgLanguages.ToLower().IndexOf(texts[i].Language.ToLower()) >= 0)
          {
            offset = i;
            break;
          }
        }
        if (offset != -1)
        {
          title = texts[offset].Title;
          description = texts[offset].Description;
          genre = texts[offset].Genre;
          starRating = texts[offset].StarRating;
          classification = texts[offset].Classification;
          parentalRating = texts[offset].ParentalRating;
        }
        else
        {
          title = texts[0].Title;
          description = texts[0].Description;
          genre = texts[0].Genre;
          starRating = texts[0].StarRating;
          classification = texts[0].Classification;
          parentalRating = texts[0].ParentalRating;
        }
      }

      if (title == null) title = "";
      if (description == null) description = "";
      if (genre == null) genre = "";
      if (classification == null) classification = "";
    }
    private void UpdateDatabaseThread()
    {
      if (_epg == null)
        return;

      _updateThreadRunning = true;
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (EpgChannel epgChannel in _epg)
      {
        int iInserted = 0;
        DVBBaseChannel dvbChannel = (DVBBaseChannel)epgChannel.Channel;
        if (epgChannel.Programs.Count == 0)
        {
          Log.Log.Epg("TimeshiftingEPG: no epg info for channel found with nid={0} tid={1} sid={2}", dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId);
          continue;
        }
        Channel dbChannel = layer.GetChannelByTuningDetail(dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId);
        if (dbChannel == null)
        {
          Log.Log.Epg("TimeshiftingEPG: no channel found for nid={0} tid={1} sid={2}", dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId);
          continue;
        }
        if (_storeOnlySelectedChannels)
        {
          if (!dbChannel.GrabEpg)
          {
            Log.Log.Epg("TimeshiftingEPG: channel {0} is not configured to grab epg.", dbChannel.DisplayName);
            continue;
          }
        }
        DateTime newestEntry = layer.GetNewestProgramForChannel(dbChannel.IdChannel);
        if (epgChannel.Programs[epgChannel.Programs.Count - 1].StartTime <= newestEntry)
        {
          Log.Log.Epg("TimeshiftingEPG: no new epg entries for channel {0}", dbChannel.DisplayName);
          continue;
        }
        int epgOffset = GetEPGOffset(epgChannel.Programs,newestEntry);
        layer.RemoveOldPrograms(dbChannel.IdChannel);
        IList dbPrograms = layer.GetPrograms(dbChannel, newestEntry);
        for (int i=epgOffset;i<epgChannel.Programs.Count;i++)
        {
          EpgProgram epgProgram = epgChannel.Programs[i];
          if (!ProgramExists(dbPrograms, epgProgram.StartTime, epgProgram.EndTime))
          {
            string title; string description; string genre; int starRating; string classification; int parentRating;
            GetEPGLanguage(epgProgram.Text, out title, out description, out genre,out starRating, out classification, out parentRating);
            NameValueCollection values = new NameValueCollection();
            values.Add("%TITLE%", title);
            values.Add("%DESCRIPTION%", description);
            values.Add("%GENRE%", genre);
            values.Add("%STARRATING%", starRating.ToString());
            values.Add("%STARRATING_STR%", GetStarRatingStr(starRating));
            values.Add("%CLASSIFICATION%", classification);
            values.Add("%PARENTALRATING%", parentRating.ToString());
            values.Add("%NEWLINE%", Environment.NewLine);
            Program prog = new Program(dbChannel.IdChannel, epgProgram.StartTime, epgProgram.EndTime,EvalTemplate(_titleTemplate,values) ,EvalTemplate(_descriptionTemplate,values),genre, false, DateTime.MinValue, string.Empty, string.Empty, starRating, classification,parentRating);
            prog.Persist();
            iInserted++;
          }
        }
        dbChannel.LastGrabTime = DateTime.Now;
        dbChannel.Persist();
        Log.Log.Epg("- Inserted {0} epg entries for channel {1}", iInserted, dbChannel.DisplayName);
      }
      Log.Log.Epg("TimeshiftingEPG: Finished updating the database.");
      _epg.Clear();
      _epg = null;
      _card.IsEpgGrabbing = false;
      _updateThreadRunning = false;
    }
    #endregion

    #region Template helper
    private string GetStarRatingStr(int starRating)
    {
      string rating = "";
      switch (starRating)
      {
        case 1:
          rating = "*";
          break;
        case 2:
          rating = "*+";
          break;
        case 3:
          rating = "**";
          break;
        case 4:
          rating = "**+";
          break;
        case 5:
          rating = "***";
          break;
        case 6:
          rating = "***+";
          break;
        case 7:
          rating = "****";
          break;
      }
      return rating;
    }
    private string EvalTemplate(string template, NameValueCollection values)
    {
      for (int i = 0; i < values.Count; i++)
        template = template.Replace(values.Keys[i], values[i]);
      return template;
    }
    #endregion
  }
}
