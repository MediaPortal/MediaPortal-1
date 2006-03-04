/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
using System;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion

namespace MediaPortal.TV.Recording
{

  public class CommandProcessor : MultiCardBase, IDisposable
  {
    #region variables
    // recorder state
    BackgroundWorker _processThread;


    // list of all recorder commands which the processthread should process
    List<CardCommand> _listCommands = new List<CardCommand>();
    EPGProcessor _epgProcessor;
    Scheduler _scheduler;
    bool _isRunning;
    bool _isStopped;
    bool _isPaused;
    DateTime _startTimeShiftTimer=DateTime.MinValue;
    TvCardCollection _tvcards;
    AutoResetEvent _waitMutex;
    #endregion

    #region ctor
    public CommandProcessor()
      : base()
    {
      //load tv cards
      _tvcards = new TvCardCollection(this);

      _epgProcessor = new EPGProcessor();
      _scheduler = new Scheduler();
      _waitMutex = new AutoResetEvent(true);

    }

    //start the processing thread
    public void Start()
    {
      _isRunning = true;
      _isStopped = false;
      _processThread = new BackgroundWorker();
      _processThread.DoWork += new DoWorkEventHandler(ProcessThread);
      _processThread.RunWorkerAsync();

    }

    #endregion

    #region public members
    public void Execute(CardCommand command)
    {
      AutoResetEvent waitEvent = new AutoResetEvent(false);
      try
      {
        command.Event = waitEvent;
        AddCommand(command);
        command.WaitOne();
      }
      finally
      {
        waitEvent.Close();
      }
    }

    public void AddCommand(CardCommand command)
    {
      lock (_listCommands)
      {
        _listCommands.Add(command);
        Log.WriteFile(Log.LogType.Recorder, "add cmd:{0} #{1}", command.ToString(),_listCommands.Count);
        _waitMutex.Set();
      }
    }

    /// <summary>
    /// Property which returns the timeshifting filename for a specific card
    /// </summary>
    /// <param name="card">card index</param>
    /// <returns>filename of the timeshifting file</returns>
    public string GetTimeShiftFileName(int card)
    {
      if (card < 0 || card >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[card];
      string fileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, card + 1, dev.TimeShiftFileName);
      return fileName;
    }

    /// <summary>
    /// Checks if a tv card is recording the TVRecording specified in 'rec
    /// </summary>
    /// <param name="rec">TVRecording <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <param name="card">the card index which is currently recording the 'rec'></param>
    /// <returns>true if a card is recording the specified TVRecording, else false</returns>
    public bool IsRecordingSchedule(TVRecording rec, out int card)
    {
      card = -1;
      if (rec == null) return false;
      //check all cards
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        // us it recording the schedule specified in 'rec'?
        if (dev.IsRecording && dev.CurrentTVRecording != null && dev.CurrentTVRecording.ID == rec.ID)
        {
          //seems so, is the recording a series
          if (rec.Series == false)
          {
            //no, then we now for sure its recording it
            card = i;
            return true;
          }

          //its a series, so we need to check start/end times of the current episode
          if (rec.StartTime <= DateTime.Now && rec.EndTime >= rec.StartTime)
          {
            //yep, we're recording this episode, so return true
            card = i;
            return true;
          }
        }
      }
      return false;
    }//static public bool IsRecordingSchedule(TVRecording rec, out int card)

    public bool IsBusy
    {
      get
      {
        lock (_listCommands)
        {
          return (_listCommands.Count > 0);
        }
      }
    }
    public virtual void WaitTillFinished()
    {
      while (IsBusy)
      {
        GUIWindowManager.Process();
        System.Threading.Thread.Sleep(100);
      }
    }

    public bool Paused
    {
      get { return _isPaused; }
      set
      {
        if (_isPaused == value) return;
        _isPaused = value;

      }
    }

    public Scheduler scheduler
    {
      get
      {
        return _scheduler;
      }
    }
    public TvCardCollection TVCards
    {
      get { return _tvcards; }
    }

    /// <summary>
    /// Shows in the log file which cards are in use and what they are doing
    /// Also logs which file is currently being played
    /// </summary>
    public void LogTunerStatus()
    {
      TVCaptureDevice dev;
      for (int i = 0; i < TVCards.Count; ++i)
      {
        dev = TVCards[i];
        if (dev.IsRecording)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} recording tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.IsEpgGrabbing)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} grab epg tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.View)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} view tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.IsTimeShifting)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} timeshift tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.IsRadio)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} radio station:{1}", dev.CommercialName, dev.RadioStation);
        }
        else
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} idle", dev.CommercialName);
        }
      }

      if (g_Player.Playing)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  currently playing:{0} pos:{0}/{1}", g_Player.CurrentFile, g_Player.CurrentPosition, g_Player.Duration);
      }
    }

    #endregion

    #region private members
    void ProcessThread(object sender, DoWorkEventArgs e)
    {
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
        while (_isRunning)
        {
          try
          {
            _waitMutex.WaitOne(500,true);
            if (_isPaused) continue;

            ProcessCommands();
            ProcessCards();
            ProcessScheduler();

            _epgProcessor.Process(this);
          }
          catch (Exception ex)
          {
            Log.Write(ex);
          }
      }
      StopAllCards();
      _isStopped = true;
      Log.WriteFile(Log.LogType.Recorder, "Commandprocessor stopped");
    }

    public void ProcessScheduler()
    {
      _scheduler.Process(this);
    }
    public void ProcessCommands()
    {
      while (true)
      {
        if (!IsBusy) return;
        CardCommand cmd;
        lock (_listCommands)
        {
          cmd = _listCommands[0];
        }

        DateTime dtStart = DateTime.Now;
        try
        {
          cmd.Execute(this);
        }
        catch (Exception ex)
        {
          Log.WriteFile(Log.LogType.Recorder, true, "Command:{0} failed",cmd.ToString());
          Log.Write(ex);
        }
        TimeSpan ts=DateTime.Now-dtStart;
        if (cmd.Succeeded == false)
        {
          Log.WriteFile(Log.LogType.Recorder, true, "Command:{0} failed reason:{1} time:{2} msec",cmd.ToString(), cmd.ErrorMessage, ts.TotalMilliseconds);
        }
        else
        {
          Log.WriteFile(Log.LogType.Recorder, false, "Command:{0} time:{1} msec",cmd.ToString(), ts.TotalMilliseconds);
        }
        cmd.Finished=true;
        lock (_listCommands)
        {
          _listCommands.RemoveAt(0);
        }
        LogTunerStatus();
      }
    }

    void StopAllCards()
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:Stop all tuners");
      for (int i = 0; i < TVCards.Count; ++i)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:Stop card:{0}", TVCards[i].CommercialName);
        TVCards[i].Stop();
      }
    }
    
    public override void ResetTimeshiftTimer()
    {
      _killTimeshiftingTimer = DateTime.Now;
      _startTimeShiftTimer = DateTime.MinValue;
    }
    public void ProcessCards()
    {
      //process all cards
      for (int i = 0; i < TVCards.Count; ++i)
      {
        TVCaptureDevice dev = TVCards[i];
        dev.Process();

        if (CurrentCardIndex == i)
        {
          if (GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow) || GUIGraphicsContext.DX9Device==null)
          {
            if (dev.IsTimeShifting || dev.IsRecording)
            {
              if (!g_Player.Playing)
              {
                if (dev.CanViewTimeShiftFile())
                {
                  TimeSpan ts=DateTime.Now-_startTimeShiftTimer;
                  if (ts.TotalSeconds>5)
                  {
                    //yes, check if we're already playing/watching it
                    string timeShiftFileName = GetTimeShiftFileName(CurrentCardIndex);
                    Log.WriteFile(Log.LogType.Recorder, "Recorder:  start viewing timeshift file of card {0}", dev.CommercialName);
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
                    msg.Label = timeShiftFileName;
                    GUIGraphicsContext.SendMessage(msg);
                    ResetTimeshiftTimer();
                    _startTimeShiftTimer=DateTime.Now;
                  }
                }
                else
                {
                  _startTimeShiftTimer=DateTime.MinValue;
                }
                return;
              }
              else
              {
                _startTimeShiftTimer=DateTime.MinValue;
              }
            }
            else
            {
              _startTimeShiftTimer=DateTime.MinValue;
            }
          }
        }
        //if card is timeshifting, but player has stopped, then stop the card also
        if (dev.IsTimeShifting && !dev.IsRecording && !dev.IsRadio)
        {
          if (CurrentCardIndex == i)
          {

            //player not playing?
            if (!g_Player.Playing)
            {
              // for more then 10 secs?
              TimeSpan ts = DateTime.Now - _killTimeshiftingTimer;
              if (ts.TotalSeconds > 10)
              {
                //then stop the card
                Log.WriteFile(Log.LogType.Recorder, "Recorder:Stop card:{0}", dev.CommercialName);
                dev.StopTimeShifting();
                CurrentCardIndex = -1;
                OnTvStopped(i, dev);
              }
            }
            else
            {
              _killTimeshiftingTimer = DateTime.Now;
            }

          }
          else
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:Stop card:{0}", dev.CommercialName);
            dev.StopTimeShifting();
          }
        }
      }
    }//void ProcessCards()
    #endregion

    #region IDisposable
    public void Dispose()
    {
      if (_processThread != null)
      {
        _isRunning = false;
        while (_isStopped == false)
        {
          System.Threading.Thread.Sleep(100);
        }
        _processThread.Dispose();
        _processThread = null;
      }
    }
    #endregion
  }
}
