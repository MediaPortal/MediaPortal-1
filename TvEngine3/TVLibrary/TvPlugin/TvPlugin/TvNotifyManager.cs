#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using Timer = System.Windows.Forms.Timer;

namespace TvPlugin
{
  public class TvNotifyManager
  {
    private Timer _timer;
    // flag indicating that notifies have been added/changed/removed
    private static bool _notifiesListChanged;
    private static bool _enableRecNotification;
    private static bool _busy;
    private int _preNotifyConfig;

    //list of all notifies (alert me n minutes before program starts)
    private IList<Program> _notifiesList;
    private IList _notifiedRecordings;

    private static IList<Recording> _actualRecordings;

    private User _dummyuser;

    public TvNotifyManager()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _enableRecNotification = xmlreader.GetValueAsBool("mytv", "enableRecNotifier", false);
        _preNotifyConfig = xmlreader.GetValueAsInt("mytv", "notifyTVBefore", 300);
      }

      _busy = false;
      _timer = new Timer();
      _timer.Stop();
      // check every 15 seconds for notifies
      _dummyuser = new User();
      _dummyuser.IsAdmin = false;
      _dummyuser.Name = "Free channel checker";
      _timer.Interval = 15000;
      _timer.Enabled = true;
      // Execute TvNotifyManager in a separate thread, so that it doesn't block the Main UI Render thread when Tvservice connection died
      new Thread(() =>
                   {
                     _timer.Tick += new EventHandler(_timer_Tick);

                   }
        ) {Name = "TvNotifyManager"}.Start();
      _notifiedRecordings = new ArrayList();
    }

    public void Start()
    {
      Log.Info("TvNotify: start");
      _timer.Start();
    }

    public void Stop()
    {
      Log.Info("TvNotify: stop");
      _timer.Stop();
    }

    public static bool RecordingNotificationEnabled
    {
      get { return _enableRecNotification; }
    }

    public static void ForceUpdate()
    {
      if (!_enableRecNotification)
      {
        return;
      }
      AddActiveRecordings();
      CheckForStoppedRecordings();
    }

    public static void OnNotifiesChanged()
    {
      Log.Info("TvNotify:OnNotifiesChanged");
      _notifiesListChanged = true;
    }

    private static Recording CheckForStoppedRecordings()
    {
      Recording stoppedRec = null;
      IList<Recording> recordings = Recording.ListAllActive();
      bool found = false;
      foreach (Recording actRec in _actualRecordings)
      {
        foreach (Recording rec in recordings)
        {
          if (rec.IdRecording == actRec.IdRecording)
          {
            found = true;
            break;
          }
        }
        if (found)
        {
          break;
        }
        else
        {          
          stoppedRec = Recording.Refresh(actRec.IdRecording);
        }
      }

      if (stoppedRec != null)
      {
        UpdateActiveRecordings();
      }
      return stoppedRec;
    }

    private static Recording AddActiveRecordings()
    {
      Recording newRecAdded = null;
      IList<Recording> recordings = Recording.ListAllActive();
      if (_actualRecordings == null)
      {
        _actualRecordings = new List<Recording>();
      }
      foreach (Recording rec in recordings)
      {
        if (!_actualRecordings.Contains(rec))
        {
          _actualRecordings.Add(rec);
          newRecAdded = rec;
        }
      }
      return newRecAdded;
    }

    private static void UpdateActiveRecordings()
    {
      _actualRecordings = Recording.ListAllActive();
    }

    private void LoadNotifies()
    {
      try
      {
        Log.Info("TvNotify:LoadNotifies");
        _notifiesList = Program.RetrieveAllNotifications();
        _notifiedRecordings.Clear();

        if (_notifiesList != null)
        {
          Log.Info("TvNotify: {0} notifies", _notifiesList.Count);
        }

        UpdateActiveRecordings();
      }
      catch (Exception e)
      {
        Log.Error("TvNotify:LoadNotifies exception : {0}", e.Message);
      }
    }

    private bool Notify(string heading, string mainMsg, Channel channel)
    {
      Log.Info("send rec notify");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_REC, 0, 0, 0, 0, 0, null);
      msg.Label = heading;
      msg.Label2 = mainMsg;
      msg.Object = channel;
      GUIGraphicsContext.SendMessage(msg);
      msg = null;
      Log.Info("send rec notify done");
      return true;
    }

    private void ProcessNotifies(DateTime preNotifySecs)
    {
      if (_notifiesListChanged)
      {
        LoadNotifies();
        _notifiesListChanged = false;
      }
      if (_notifiesList != null && _notifiesList.Count > 0)
      {
        foreach (Program program in _notifiesList)
        {
          if (System.DateTime.Now > program.EndTime)
          {
            Log.Debug("Notify auto cancel old program {0} on {1} ended {2}", program.Title, program.ReferencedChannel().DisplayName,
                     program.EndTime);
            program.Notify = false;
            program.Persist();

            _notifiesList.Remove(program);

            return;
          }

          if (preNotifySecs > program.StartTime)
          {
            Log.Info("Notify {0} on {1} start {2}", program.Title, program.ReferencedChannel().DisplayName,
                     program.StartTime);
            program.Notify = false;
            program.Persist();
            TVProgramDescription tvProg = new TVProgramDescription();
            tvProg.Channel = program.ReferencedChannel();
            tvProg.Title = program.Title;
            tvProg.Description = program.Description;
            tvProg.Genre = program.Genre;
            tvProg.StartTime = program.StartTime;
            tvProg.EndTime = program.EndTime;

            _notifiesList.Remove(program);
            Log.Info("send notify");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
            msg.Object = tvProg;
            GUIGraphicsContext.SendMessage(msg);
            msg = null;
            Log.Info("send notify done");
            return;
          }
        }
      }
    }

    private void ProcessRecordings(DateTime preNotifySecs)
    {
      //Log.Debug("TVPlugIn: Notifier checking for recording to start at {0}", preNotifySecs);
      //if (g_Player.IsTV && TVHome.Card.IsTimeShifting && g_Player.Playing)
      IList<Schedule> schedulesList = null;
      //if (_enableRecNotification && g_Player.Playing)
      if (_enableRecNotification)
      {
        if (TVHome.TvServer.IsTimeToRecord(preNotifySecs))
        {
          try
          {
            schedulesList = Schedule.ListAll();
            foreach (Schedule rec in schedulesList)
            {
              bool bContinue = false;
              //Check if alerady notified user
              foreach (Schedule notifiedRec in _notifiedRecordings)
              {
                if (rec == notifiedRec)
                {
                  bContinue = true;
                  break;
                }
              }
              if (bContinue)
                continue;

              //Check if timing it's time 
              Log.Debug("TVPlugIn: Notifier checking program {0}", rec.ProgramName);
              if (TVHome.TvServer.IsTimeToRecord(preNotifySecs, rec.IdSchedule))
              {
                //check if freecard is available. 
                //Log.Debug("TVPlugIn: Notify verified program {0} about to start recording. {1} / {2}", rec.ProgramName, rec.StartTime, preNotifySecs);
                if (TVHome.Navigator.Channel.IdChannel != rec.IdChannel &&
                    (int)TVHome.TvServer.GetChannelState(rec.IdChannel, _dummyuser) == 0) //not tunnable
                {
                  Log.Debug("TVPlugIn: No free card available for {0}. Notifying user.", rec.ProgramName);

                  Notify(GUILocalizeStrings.Get(1004),
                         String.Format("{0}. {1}", rec.ProgramName, GUILocalizeStrings.Get(200055)),
                         TVHome.Navigator.Channel);

                  _notifiedRecordings.Add(rec);
                  return;
                }
              }
            }
          }
          catch (Exception ex)
          {
            Log.Debug("Tv NotifyManager: Exception at recording notification {0}", ex.ToString());
          }
        }

        //check if rec. has started
        if (!_enableRecNotification)
        {
          return;
        }

        Recording newRecording = AddActiveRecordings();

        if (newRecording != null && newRecording.IsRecording)
        {
          Schedule parentSchedule = newRecording.ReferencedSchedule();

          if (parentSchedule != null && parentSchedule.IdSchedule > 0)
          {
            string endTime = string.Empty;
            if (parentSchedule != null)
            {
              endTime = parentSchedule.EndTime.AddMinutes(parentSchedule.PostRecordInterval).ToString("t",
                                                                                                      CultureInfo.
                                                                                                        CurrentCulture.
                                                                                                        DateTimeFormat);
            }
            string text = String.Format("{0} {1}-{2}",
                                        newRecording.Title,
                                        newRecording.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                        endTime);
            //Recording started            

            TimeSpan tsStart = DateTime.Now - newRecording.StartTime;
            // do not show rec. that have been started a while ago.
            if (tsStart.TotalSeconds < 60)
            {
              Notify(GUILocalizeStrings.Get(1446), text, newRecording.ReferencedChannel());
            }
            return;
          }
        }
        //check if rec. has ended.                
        Recording stoppedRec = CheckForStoppedRecordings();
        if (stoppedRec == null)
        {
          return;
        }

        TimeSpan tsEnd = DateTime.Now - stoppedRec.EndTime; // do not show rec. that have been stopped a while ago.
        if (tsEnd.TotalSeconds > 60)
        {
          _actualRecordings.Remove(stoppedRec);
          return;
        }

        string textPrg = "";
        IList<Program> prgs = Program.RetrieveByTitleAndTimesInterval(stoppedRec.Title, stoppedRec.StartTime,
                                                                      stoppedRec.EndTime);

        Program prg = null;
        if (prgs != null && prgs.Count > 0)
        {
          prg = prgs[0];
        }
        if (prg != null)
        {
          textPrg = String.Format("{0} {1}-{2}",
                                  prg.Title,
                                  prg.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  prg.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        }
        else
        {
          textPrg = String.Format("{0} {1}-{2}",
                                  stoppedRec.Title,
                                  stoppedRec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  DateTime.Now.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        }
        //Recording stopped:                            
        Notify(GUILocalizeStrings.Get(1447), textPrg, stoppedRec.ReferencedChannel());
        _actualRecordings.Remove(stoppedRec);

        return; //we do not want to show any more notifications.        
      }
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
      try
      {
        if (!TVHome.Connected)
        {
          return;
        }
        
        if (_busy)
        {
          return;
        }

        _busy = true;

        if (_actualRecordings == null)
        {
          AddActiveRecordings();
        }

        if (!TVHome.Connected)
        {
          return;
        }

        DateTime preNotifySecs = DateTime.Now.AddSeconds(_preNotifyConfig);
        ProcessNotifies(preNotifySecs);
        ProcessRecordings(preNotifySecs);
      }
      catch (Exception ex)
      {        
        Log.Error("Tv NotifyManager: Exception at timer_tick {0} st : {1}", ex.ToString(), Environment.StackTrace);
      }
      finally
      {
        _busy = false;
      }
    }
  }
}