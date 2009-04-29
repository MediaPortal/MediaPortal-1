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

#region usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;

#endregion

namespace MediaPortal.TV.Recording
{
  public class CommandProcessor : MultiCardBase, IDisposable
  {
    #region variables

    // recorder state
    private BackgroundWorker _processThread;


    // list of all recorder commands which the processthread should process
    private List<CardCommand> _listCommands = new List<CardCommand>();
    private EPGProcessor _epgProcessor;
    private Scheduler _scheduler;
    private bool _isRunning;
    private bool _isStopped;
    private bool _isPaused;
    private DateTime _startTimeShiftTimer = DateTime.MinValue;
    private TvCardCollection _tvcards;
    private AutoResetEvent _waitMutex;
    private bool _controlTimeShifting = true;

    #endregion

    #region ctor

    public CommandProcessor()
      : base()
    {
      //load tv cards
      _tvcards = new TvCardCollection();

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
      AddCommand(command);
      while (!command.Finished)
      {
        GUIWindowManager.Process();// DispatchThreadMessages();
        Thread.Sleep(100);
      }
    }

    public void AddCommand(CardCommand command)
    {
      lock (_listCommands)
      {
        _listCommands.Add(command);
        Log.Debug("CommandProcessor: add cmd: {0} #{1}", command.ToString(), _listCommands.Count);
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
      if (card < 0 || card >= _tvcards.Count)
      {
        return string.Empty;
      }
      TVCaptureDevice dev = _tvcards[card];
      string fileName = dev.TimeShiftFullFileName;
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
      if (rec == null)
      {
        return false;
      }
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
          if (rec.StartTime.AddMinutes(-rec.PreRecord) <= DateTime.Now && rec.EndTime >= rec.StartTime)
          {
            // do everytime on ... specific stuff
            if (rec.StartTime < DateTime.Now && DateTime.Now < rec.EndTime
                // make sure we will get the right next program
                && (rec.RecType == TVRecording.RecordingType.EveryTimeOnEveryChannel // only needed
                    || rec.RecType == TVRecording.RecordingType.EveryTimeOnThisChannel)) // for "every..." types
            {
              // we need to know if we want to record 2 back 2 back  programs with same title 
              // eg : everytimeonthischannel : 12:45-13:30 Smallville / 13:30-14:15 SmallVille
              TVChannel recChannel = new TVChannel(rec.Channel);
              TVProgram nextProg = recChannel.GetProgramAt(rec.EndTime.AddMinutes(1));
              bool isRecordingNextProgram = false;
              if (nextProg != null)
              {
                isRecordingNextProgram = rec.IsRecordingProgramAtTime(nextProg.StartTime.AddMinutes(1), nextProg,
                                                                      rec.PreRecord, rec.PostRecord);
              }
              if (isRecordingNextProgram)
              {
                // clone the currentrec and set its start end to next program ones
                TVRecording nextrec = new TVRecording(rec);
                nextrec.StartTime = nextProg.StartTime;
                nextrec.Start = nextProg.Start;
                nextrec.EndTime = nextProg.EndTime;
                nextrec.End = nextProg.End;
                nextrec.Channel = nextProg.Channel;
                nextrec.ID = -1; // a new schedule will be created in db
                TVDatabase.AddRecording(ref nextrec);
                // set current rec type to once
                dev.CurrentTVRecording.RecType = TVRecording.RecordingType.Once;
                TVDatabase.UpdateRecording(dev.CurrentTVRecording, TVDatabase.RecordingChange.Modified);
              }
            }
            //yep, we're recording this episode, so return true
            card = i;
            return true;
          }
        }
      }
      return false;
    } //public static bool IsRecordingSchedule(TVRecording rec, out int card)

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
        Thread.Sleep(100);
      }
    }

    public bool Paused
    {
      get { return _isPaused; }
      set
      {
        if (_isPaused == value)
        {
          return;
        }
        _isPaused = value;
      }
    }

    public Scheduler scheduler
    {
      get { return _scheduler; }
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
          Log.Info("Commandprocessor: Card:{0} recording tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.IsRadio)
        {
          Log.Info("Commandprocessor: Card:{0} radio station:{1}", dev.CommercialName, dev.RadioStation);
        }
        else if (dev.IsEpgGrabbing)
        {
          Log.Info("Commandprocessor: Card:{0} grab epg tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.View)
        {
          Log.Info("Commandprocessor: Card:{0} view tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else if (dev.IsTimeShifting)
        {
          Log.Info("Commandprocessor: Card:{0} timeshift tv channel:{1}", dev.CommercialName, dev.TVChannel);
        }
        else
        {
          Log.Info("Commandprocessor: Card:{0} idle", dev.CommercialName);
        }
      }

      if (g_Player.Playing)
      {
        Log.Info("Commandprocessor: Currently playing: {0} pos: {0}/{1}", g_Player.CurrentFile, g_Player.CurrentPosition,
                 g_Player.Duration);
      }
    }

    #endregion

    #region private members

    private void ProcessThread(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "CommandProcessor";
      Log.Info("Commandprocessor: Starting");
      if (GUIGraphicsContext.DX9Device != null)
      {
        // hwahrmann: not needed here. caused sometimes hangs, when starting a VS debug session
        //AvailableFilters af = AvailableFilters.Instance;
        //Log.Debug("Commandprocessor: Starting tv cards");

        for (int i = 0; i < TVCards.Count; ++i)
        {
          Log.Debug("Commandprocessor: Start card:{0}", TVCards[i].CommercialName);
          TVCards[i].CreateGraph();
        }
      }
      Log.Debug("Commandprocessor: Running");

      Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

      while (_isRunning)
      {
        try
        {
          _waitMutex.WaitOne(500, true);
          if (_isPaused)
          {
            continue;
          }

          ProcessCommands();
          ProcessCards();
          ProcessScheduler();
          _epgProcessor.Process(this);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }
      StopAllCards();
      _isStopped = true;
      Log.Info("Commandprocessor: Stopped");
    }

    public bool ControlTimeShifting
    {
      get { return _controlTimeShifting; }
      set { _controlTimeShifting = value; }
    }

    public void ProcessScheduler()
    {
      _scheduler.Process(this);
    }

    public void ProcessCommands()
    {
      while (true)
      {
        if (!IsBusy)
        {
          return;
        }
        CardCommand cmd;
        lock (_listCommands)
        {
          cmd = _listCommands[0];
        }

        DateTime dtStart = DateTime.Now;
        try
        {
          Thread.CurrentThread.Priority = ThreadPriority.Highest;
          cmd.Execute(this);
        }
        catch (Exception ex)
        {
          Log.Warn("Commandprocessor: {0} failed", cmd.ToString());
          Log.Error(ex);
        }
        finally
        {
          Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
          TimeSpan ts = DateTime.Now - dtStart;
          if (cmd.Succeeded == false)
          {
            Log.Info("Commandprocessor: {0} failed reason: {1} time: {2} msec", cmd.ToString(), cmd.ErrorMessage,
                     ts.TotalMilliseconds);
          }
          else
          {
            Log.Debug("Commandprocessor: {0} time: {1} msec", cmd.ToString(), ts.TotalMilliseconds);
          }
          cmd.Finished = true;
          lock (_listCommands)
          {
            _listCommands.RemoveAt(0);
          }
        }
        LogTunerStatus();
      }
    }

    private void StopAllCards()
    {
      Log.Info("Commandprocessor: Stopping all tuners");
      for (int i = 0; i < TVCards.Count; ++i)
      {
        Log.Debug("Commandprocessor: Stop card:{0}", TVCards[i].CommercialName);
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

        if (ControlTimeShifting)
        {
          if (CurrentCardIndex == i)
          {
            if (GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow) || GUIGraphicsContext.DX9Device == null)
            {
              if (dev.IsTimeShifting || dev.IsRecording)
              {
                if (!g_Player.Playing && !g_Player.Starting)
                {
                  if (dev.CanViewTimeShiftFile())
                  {
                    TimeSpan ts = DateTime.Now - _startTimeShiftTimer;
                    if (ts.TotalSeconds > 7)
                    {
                      //yes, check if we're already playing/watching it
                      string timeShiftFileName = GetTimeShiftFileName(CurrentCardIndex);
                      Log.Debug("Commandprocessor: Start viewing timeshift file of card {0}", dev.CommercialName);
                      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
                      msg.Label = timeShiftFileName;
                      GUIGraphicsContext.SendMessage(msg);
                      ResetTimeshiftTimer();
                      _startTimeShiftTimer = DateTime.Now;
                    }
                    //else Log.Info(LogType.Recorder, "Recorder:wait {0}...", ts.TotalSeconds);
                  }
                  else
                  {
                    _startTimeShiftTimer = DateTime.MinValue;
                  }
                  return;
                }
                else
                {
                  _startTimeShiftTimer = DateTime.MinValue;
                }
              }
              else
              {
                _startTimeShiftTimer = DateTime.MinValue;
              }
            }
          }
          //if card is timeshifting, but player has stopped, then stop the card also
          if (dev.IsTimeShifting && !dev.IsRecording && !dev.IsRadio)
          {
            if (CurrentCardIndex == i)
            {
              //player not playing?
              if (!g_Player.Playing && !g_Player.Starting)
              {
                // for more then 10 secs?
                TimeSpan ts = DateTime.Now - _killTimeshiftingTimer;
                if (ts.TotalSeconds > 10)
                {
                  //then stop the card
                  Log.Debug("Commandprocessor: Stop card: {0}", dev.CommercialName);
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
              Log.Debug("Commandprocessor: Stop card:{0}", dev.CommercialName);
              dev.StopTimeShifting();
            }
          }
        }
      }
    } //void ProcessCards()

    #endregion

    #region IDisposable

    public void Dispose()
    {
      if (_processThread != null)
      {
        _isRunning = false;
        while (_isStopped == false)
        {
          Thread.Sleep(100);
        }
        _processThread.Dispose();
        _processThread = null;
      }
    }

    #endregion
  }
}