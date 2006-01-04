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
  /// <summary>
  /// This class is a singleton which implements the
  /// -task scheduler to schedule, (start,stop) all tv recordings on time
  /// -a front end to other classes to control the tv capture cardsd
  /// </summary>
  public class Recorder
  {
    #region recorder commands
    //The GUI can communicate with the process thread by sending Recorder Commands
    //todo this, create a RecorderCommand object and add it to _listCommands.
    //The process thread will execute it in the background
    enum RecorderCommandType
    {
      Paused,
      StopAll,        // stop all activity on all cards
      StopAllViewing, // stop any card which is currently viewing tv
      StopViewing,    // stop viewing tv on current selected card
      StartViewing,   // start viewing tv
      StartRadio,     // start listening radio
      StopRadio,      // stop listening radio
      StopRecording,  // stop a recording
      HandleRecording
    }

    class RecorderCommand
    {
      RecorderCommandType _type;
      string _channel;
      bool _timeShift;
      TVRecording _recording;

      public RecorderCommand(RecorderCommandType type)
      {
        _type = type;
      }
      public RecorderCommand(RecorderCommandType type, string channel, bool timeShift)
      {
        _type = type;
        _channel = channel;
        _timeShift = timeShift;
      }
      public RecorderCommand(RecorderCommandType type, TVRecording rec)
      {
        _type = type;
        _recording = rec;
      }
      public RecorderCommandType CommandType
      {
        get { return _type; }
        set { _type = value; }
      }
      public string Channel
      {
        get { return _channel; }
        set { _channel = value; }
      }
      public bool TimeShifting
      {
        get { return _timeShift; }
        set { _timeShift = value; }
      }
      public TVRecording Recording
      {
        get { return _recording; }
        set { _recording = value; }
      }
    }
    #endregion

    #region variables
    enum State
    {
      None,           //recorder is not initialized yet
      Initializing,   //recorder is busy initializing
      Initialized,    //recorder is initialized 
      Deinitializing  //recorder is de-initializing
    }

    // flag indicating that recordings have been added/changed/removed
    static bool _recordingsListChanged = false;  
    // flag indicating that notifies have been added/changed/removed
    static bool _notifiesListChanged = false;  
    // number of minutes we should start recording before the program starts
    static int _preRecordInterval = 0;  
    // number of minutes we keeprecording after the program starts
    static int _postRecordInterval = 0; 

    // current selected TV channel name
    static string _tvChannel = String.Empty; 

    // recorder state
    static State _state = State.None; 
    // list of all tv cards installed
    static List<TVCaptureDevice> _tvcards = new List<TVCaptureDevice>();

    //list of all tv channels present in tv database
    static List<TVChannel> _tvChannelsList = new List<TVChannel>();

    //list of all scheduled recordings
    static List<TVRecording> _recordingsList = new List<TVRecording>();

    //list of all notifies (alert me 2 minutes before program starts)
    static List<TVNotify> _notifiesList = new List<TVNotify>();

    static DateTime _progressBarTimer = DateTime.Now;

    //current selected tv card used for watching tv
    static int _currentCardIndex = -1;

    //specifies the number of minutes the notify should be send before a program starts
    static int _preRecordingWarningTime = 2;

    // vmr9 osd class 
    static VMR9OSD _vmr9Osd = new VMR9OSD();
    static bool _useVmr9Zap = false;

    // last duration of timeshifting buffer
    static double _duration = 0;

    // last position in timeshifting buffer
    static double _lastPosition = 0;

    // timer which is used to stop timeshifting on a card when player has stopped
    static DateTime _killTimeshiftingTimer;

    // list of all recorder commands which the processthread should process
    static List<RecorderCommand> _listCommands = new List<RecorderCommand>();

    //handle to the processing thread
    static BackgroundWorker _processThread;
    static bool _isPaused = false;
    static bool _autoGrabEpg = false;
    #endregion

    #region delegates and events
    public delegate void OnTvViewHandler(int card, TVCaptureDevice device);
    public delegate void OnTvChannelChangeHandler(string tvChannelName);
    public delegate void OnTvRecordingChangedHandler();
    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);

    //event which gets called when the tv channel changes (due to zapping for example)
    static public event OnTvChannelChangeHandler OnTvChannelChanged = null;

    //event which happens when the state of a recording changes (like started or stopped)
    static public event OnTvRecordingChangedHandler OnTvRecordingChanged = null;


    //event which happens when a recording about to be recorded
    static public event OnTvRecordingHandler OnTvRecordingStarted = null;

    //event which happens when a recording has ended
    static public event OnTvRecordingHandler OnTvRecordingEnded = null;

    //event which happens when TV viewing is started
    static public event OnTvViewHandler OnTvViewingStarted = null;

    //event which happens when TV viewing is stopped
    static public event OnTvViewHandler OnTvViewingStopped = null;
    #endregion

    #region initialisation
    /// <summary>
    /// singleton. Dont allow any instance of this class so make the constructor private
    /// </summary>
    private Recorder()
    {
    }

    static Recorder()
    {
    }

    /// <summary>
    /// This method will Start the recorder. It
    ///   -Loads the capture cards from capturecards.xml (made by the setup)
    ///   -Loads the recordings (programs scheduled to record) from the tvdatabase
    ///   -Loads the TVchannels from the tvdatabase
    ///   -starts the thread which handles all the tv cards
    /// </summary>
    static public void Start()
    {
      if (_state != State.None) return;//if we are initialized already then no need todo anything

      _state = State.Initializing;
      RecorderProperties.Init();
      _recordingsListChanged = false;

      //load the definitions for each tv capture card
      Log.WriteFile(Log.LogType.Recorder, "Recorder: Loading capture cards from capturecards.xml");
      _tvcards.Clear();
      try
      {
        using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
        {
          SoapFormatter c = new SoapFormatter();
          ArrayList cards = (ArrayList)c.Deserialize(r);
          foreach (TVCaptureDevice dev in cards)
          {
            _tvcards.Add(dev);
          }
          r.Close();
        }
      }
      catch (Exception)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder: invalid capturecards.xml found! please delete it");
      }
      if (_tvcards.Count == 0)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder: no capture cards found. Use file->setup to setup tvcapture!");
      }

      //subscribe to the recording events of each card
      for (int i = 0; i < _tvcards.Count; i++)
      {
        TVCaptureDevice card = _tvcards[i];
        card.ID = (i + 1);
        card.OnTvRecordingEnded += new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingEnded);
        card.OnTvRecordingStarted += new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingStarted);
        Log.WriteFile(Log.LogType.Recorder, "Recorder:    card:{0} video device:{1} TV:{2}  record:{3} priority:{4}",
                              card.ID, card.VideoDevice, card.UseForTV, card.UseForRecording, card.Priority);
      }

      //load the TV settings
      _preRecordInterval = 0;
      _postRecordInterval = 0;
      //m_bAlwaysTimeshift=false;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        _autoGrabEpg = xmlreader.GetValueAsBool("xmltv", "epgdvb", true);
        _preRecordInterval = xmlreader.GetValueAsInt("capture", "prerecord", 5);
        _postRecordInterval = xmlreader.GetValueAsInt("capture", "postrecord", 5);
        //m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
        TVChannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
        _useVmr9Zap = xmlreader.GetValueAsBool("general", "useVMR9ZapOSD", false);
        _preRecordingWarningTime = xmlreader.GetValueAsInt("mytv", "recordwarningtime", 2);
      }

      //clean up any old leftover timeshifting files from the last time
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        try
        {
          TVCaptureDevice dev = _tvcards[i];
          string dir = String.Format(@"{0}\card{1}", dev.RecordingPath, i + 1);
          System.IO.Directory.CreateDirectory(dir);
          DiskManagement.DeleteOldTimeShiftFiles(dir);
        }
        catch (Exception) { }
      }

      //import any recordings which are on disk, but not in the tv database
      RecordingImporterWorker.ImportDvrMsFiles();

      //get all tv channels
      _tvChannelsList.Clear();
      TVDatabase.GetChannels(ref _tvChannelsList);

      // get all the scheduled recordings
      _recordingsList.Clear();
      TVDatabase.GetRecordings(ref _recordingsList);
      TVDatabase.OnRecordingsChanged += new TVDatabase.OnRecordingChangedHandler(Recorder.OnRecordingsChanged);

      //get all the notifies
      _notifiesList.Clear();
      TVDatabase.GetNotifies(_notifiesList, true);
      TVDatabase.OnNotifiesChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(Recorder.OnNotifiesChanged);

      //subscribe to window messages.
      GUIWindowManager.Receivers += new SendMessageHandler(Recorder.OnMessage);
      _state = State.Initialized;

      
      _vmr9Osd.Mute = false;
      GUIWindow win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
      if (win != null)
        win.SetObject(_vmr9Osd);
      win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
      if (win != null)
        win.SetObject(_vmr9Osd);
      TeletextGrabber.TeletextCache.ClearBuffer();

      if (GUIGraphicsContext.DX9Device == null)
      {
        _isPaused = true;
      }
      //start the processing thread
      _processThread = new BackgroundWorker();
      _processThread.DoWork += new DoWorkEventHandler(Recorder.ProcessThread);
      _processThread.RunWorkerAsync();

      //subscribe to window change notifications
      GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);

    }

    /// <summary>
    /// This callback gets called by the window manager when user switches to another window (plugin)
    /// When the users enters the My TV plugin we want to enable Direct3d exclusive mode to prevent
    /// any tearing. And when the users leaves the My Tv plugin we want to disable Direct3d exclusive mode 
    /// again
    /// </summary>
    /// <param name="windowId">id of the window which is about to be activated</param>
    static void GUIWindowManager_OnActivateWindow(int windowId)
    {
      //Note, because of a direct3d limitation we cannot switch between
      //normal / exclusive mode when a file is playing
      if (g_Player.Playing) return;
      if (GUIGraphicsContext.IsTvWindow(windowId))
      {
        // we enter my tv, enable exclusive mode
        Log.Write("Recorder:enable dx9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      else
      {
        Log.Write("Recorder:disable dx9 exclusive mode");
        // we leave my tv, disable exclusive mode
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }//static public void GUIWindowManager_OnActivateWindow()

    /// <summary>
    /// Stops the scheduler. It will cleanup all resources allocated and free
    /// the capture cards
    /// </summary>
    static public void Stop()
    {

      if (_state != State.Initialized) return;
      //unsubscribe from events
      TVDatabase.OnRecordingsChanged -= new TVDatabase.OnRecordingChangedHandler(Recorder.OnRecordingsChanged);
      GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
      GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);
      RecorderProperties.Clean();
      _recordingsListChanged = false;

      //tell process thread to stop
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopAll);
        _listCommands.Add(cmd);
      }

      //wait until the process thread has stopped.
      _isPaused = false;
      while (_state != State.None)
      {
        GUIWindowManager.Process();
        System.Threading.Thread.Sleep(10);
      }
    }//static public void Stop()

    #endregion

    #region recording
    /// <summary>
    /// This method handles the StopAll recorder command
    /// it will simply stop all activity on each tv card
    /// and set the recorder state to None
    /// </summary>
    static void HandleStopAll()
    {
      foreach (TVCaptureDevice card in _tvcards)
      {
        card.Stop();
      }
      _state = State.None;
    }
    /// <summary>
    /// This method handles the StopAll recorder command
    /// it will simply stop all activity on each tv card
    /// and set the recorder state to None
    /// </summary>
    static void HandlePaused()
    {
      foreach (TVCaptureDevice card in _tvcards)
      {
        card.Stop();
      }
    }
    /// <summary>
    /// Checks if a recording should be started and if so starts the recording
    /// This function gets called on a regular basis by the process thread. It will
    /// look if any of the recordings needs to be started. Ifso it will
    /// find a free tvcapture card and start the recording
    /// 
    /// It will also check if any notifications (alert me just before program starts) needs to be send
    /// and ifso will send a message to the GUI so it can show the popup to the user
    /// </summary>
    static void HandleRecordings()
    {
      if (_state != State.Initialized) return;

      DateTime dtCurrentTime = DateTime.Now;
    
      // no TV cards? then we cannot record anything, so just return
      if (_tvcards.Count == 0) return;

      // If the scheduled recordings have been changed,deleted or added since last time
      // then we need to re-load the recordings from the database
      if (_recordingsListChanged)
      {
        // then get (refresh) all recordings from the database
        List<TVRecording> oldRecs = new List<TVRecording>();
        oldRecs = _recordingsList;
        _recordingsList = new List<TVRecording>();
        _tvChannelsList.Clear();
        TVDatabase.GetRecordings(ref _recordingsList);
        TVDatabase.GetChannels(ref _tvChannelsList);
        _recordingsListChanged = false;

        //remember if we already send a notification for this recording
        foreach (TVRecording recording in _recordingsList)
        {
          foreach (TVRecording oldrec in oldRecs)
          {
            if (oldrec.ID == recording.ID)
            {
              recording.IsAnnouncementSend = oldrec.IsAnnouncementSend;
              break;
            }
          }

          //if any card is busy recording a changed recording, then let the card know
          //about the changes
          for (int i = 0; i < _tvcards.Count; ++i)
          {
            TVCaptureDevice dev = _tvcards[i];
            if (dev.IsRecording)
            {
              if (dev.CurrentTVRecording.ID == recording.ID)
              {
                dev.CurrentTVRecording = recording;
              }//if (dev.CurrentTVRecording.ID==recording.ID)
            }//if (dev.IsRecording)
          }//for (int i=0; i < _tvcards.Count;++i)
        }//foreach (TVRecording recording in _recordingsList)
        oldRecs = null;
      }//if (_recordingsListChanged)

      //for each tv channel
      int card;
      for (int i = 0; i < _tvChannelsList.Count; ++i)
      {
        TVChannel chan = _tvChannelsList[i];

        // get all programs running for this TV channel
        // between  (now-4 hours) - (now+iPostRecordInterval+3 hours)
        DateTime dtStart = dtCurrentTime.AddHours(-4);
        DateTime dtEnd = dtCurrentTime.AddMinutes(_postRecordInterval + 3 * 60);
        long iStartTime = Utils.datetolong(dtStart);
        long iEndTime = Utils.datetolong(dtEnd);

        // for each TV recording scheduled
        for (int j = 0; j < _recordingsList.Count; ++j)
        {
          TVRecording rec = _recordingsList[j];
          //if recording is not canceled
          if (rec.Canceled > 0) continue;

          //and recording has not finished yet (already recorded)
          if (rec.IsDone()) continue;

          //is this the correct channel for the recording
          if (rec.RecType == TVRecording.RecordingType.EveryTimeOnEveryChannel || chan.Name == rec.Channel)
          {
            //Are we already recording this recording?
            if (!IsRecordingSchedule(rec, out card))
            {
              //no, then check if its time to record it
              int paddingFront = rec.PaddingFront;
              int paddingEnd = rec.PaddingEnd;
              if (paddingFront < 0) paddingFront = _preRecordInterval;
              if (paddingEnd < 0) paddingEnd = _postRecordInterval;

              // check which program is current running on this channel
              TVProgram prog = chan.GetProgramAt(dtCurrentTime.AddMinutes(1 + paddingFront));

              // if the recording should record the tv program
              if (rec.IsRecordingProgramAtTime(dtCurrentTime, prog, paddingFront, paddingEnd))
              {
                // yes, then record it
                if (Record(dtCurrentTime, rec, prog, paddingFront, paddingEnd))
                {
                  break;
                }
              }
              else
              {
                //not necessary to record, but maybe its time to send a notification
                //first check if we didnt already send it
                if (!rec.IsAnnouncementSend)
                {
                  //no, then check if a notification needs to be send
                  DateTime dtTime = DateTime.Now.AddMinutes(_preRecordingWarningTime);
                  TVProgram prog2Min = chan.GetProgramAt(dtTime.AddMinutes(1 + paddingFront));

                  // if the recording should record the tv program
                  if (rec.IsRecordingProgramAtTime(dtTime, prog2Min, paddingFront, paddingEnd))
                  {
                    //then send the announcement that we are about to record this recording in 2 minutes from now
                    Log.WriteFile(Log.LogType.Recorder, "Recorder: Send announcement for recording:{0}", rec.ToString());
                    rec.IsAnnouncementSend = true;
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING, 0, 0, 0, 0, 0, null);
                    msg.Object = rec;
                    msg.Object2 = prog2Min;
                    GUIGraphicsContext.SendMessage(msg);
                  }
                }
              }
            }//if (!IsRecordingSchedule(rec, out card)) 
          }//if (rec.RecType==TVRecording.RecordingType.EveryTimeOnEveryChannel || chan.Name==rec.Channel)
        } //for (int j=0; j < _recordingsList.Count;++j)
      }//for (int i=0; i < _tvChannelsList.Count;++i)


      //check if any manually added recording should be recorded
      for (int j = 0; j < _recordingsList.Count; ++j)
      {
        TVRecording rec = _recordingsList[j];
        //if recording has been canceled then skip it
        if (rec.Canceled > 0) continue;

        //if recording has been recorded already, then skip it
        if (rec.IsDone()) continue;

        int paddingFront = rec.PaddingFront;
        int paddingEnd = rec.PaddingEnd;
        if (paddingFront < 0) paddingFront = _preRecordInterval;
        if (paddingEnd < 0) paddingEnd = _postRecordInterval;

        // 1st check if the recording itself should b recorded
        if (rec.IsRecordingProgramAtTime(DateTime.Now, null, paddingFront, paddingEnd))
        {
          //yes, time to record it. Are we already recording it?
          if (!IsRecordingSchedule(rec, out card))
          {
            // no, then start recording it now
            if (Record(dtCurrentTime, rec, null, paddingFront, paddingEnd))
            {
              // recording it
            }
          }
        }
        else
        {
          //no time yet to record this recording, 
          //if we are going to record it within 2 minutes from now, then send an annoucement

          //is announcement already sent?
          if (!rec.IsAnnouncementSend)
          {
            //no, is the recording going to start within 2 mins?
            DateTime dtTime = DateTime.Now.AddMinutes(_preRecordingWarningTime);
            // if the recording should record the tv program
            if (rec.IsRecordingProgramAtTime(dtTime, null, paddingFront, paddingEnd))
            {
              //yes, then send the announcement
              rec.IsAnnouncementSend = true;
              Log.WriteFile(Log.LogType.Recorder, "Recorder: Send announcement for recording:{0}", rec.ToString());
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING, 0, 0, 0, 0, 0, null);
              msg.Object = rec;
              msg.Object2 = null;
              GUIGraphicsContext.SendMessage(msg);
            }
          }
        }
      }//for (int j=0; j < _recordingsList.Count;++j)
    }//static void HandleRecordings()


    /// <summary>
    /// NeedChannelSwitchForRecording()
    /// This method determines if we need to switch to tv channel A to tv channel B 
    /// if we want to record the TVRecording specified in rec
    /// </summary>
    /// <param name="rec">TVRecording to record</param>
    /// <returns>
    /// true : we need to switch channels
    /// false: we dont need to switch channels
    /// </returns>
    static public bool NeedChannelSwitchForRecording(TVRecording rec)
    {
      //are we viewing the channel requested, then there is no need to switch
      if (IsViewing() && TVChannelName == rec.Channel) return false;

      //no, check if there's another card which is free. ifso we can use that one and
      //there is no need to switch channels
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i] as TVCaptureDevice;
        //is the card free?
        if (!dev.IsRecording && !dev.IsTimeShifting && !dev.View)
        {
          //yes and can it receive the tv channel as well?
          if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || _tvcards.Count == 1)
          {
            //yes, then we need dont need to switch
            return false;
          }
        }
      }
      //we need to switch channels if we are going to record this recording
      return true;
    }

    /// <summary>
    /// Starts recording the specified tv channel immediately using a reference recording
    /// When called this method starts an reference recording on the channel specified
    /// It will record the next 2 hours or if the epg guide contains data about the program
    /// end/stop times, then we'll just record the current program
    /// </summary>
    /// <param name="strChannel">TVchannel to record</param>
    static public void RecordNow(string strChannel, bool manualStop)
    {
      if (strChannel == null) return;
      if (strChannel == String.Empty) return;
      if (_state != State.Initialized) return;

      // create a new recording which records the next 2 hours...
      TVRecording tmpRec = new TVRecording();

      tmpRec.Channel = strChannel;
      tmpRec.RecType = TVRecording.RecordingType.Once;

      TVProgram program = null;
      for (int i = 0; i < _tvChannelsList.Count; ++i)
      {
        TVChannel chan = _tvChannelsList[i];
        if (String.Compare(chan.Name,strChannel,false)==0)
        {
          program = chan.CurrentProgram;
          break;
        }
      }

      if (program != null && !manualStop)
      {
        //record current playing program
        tmpRec.Start = program.Start;
        tmpRec.End = program.End;
        tmpRec.Title = program.Title;
        tmpRec.IsContentRecording = false;//make a reference recording! (record from timeshift buffer)
        Log.WriteFile(Log.LogType.Recorder, "Recorder:record now:{0} program:{1}", strChannel, program.Title);
      }
      else
      {
        //no tvguide data, just record the next 2 hours
        Log.WriteFile(Log.LogType.Recorder, "Recorder:record now:{0} for next 4 hours", strChannel);
        tmpRec.Start = Utils.datetolong(DateTime.Now);
        tmpRec.End = Utils.datetolong(DateTime.Now.AddHours(4));
        tmpRec.Title = GUILocalizeStrings.Get(413);
        if (program != null)
          tmpRec.Title = program.Title;
        tmpRec.IsContentRecording = true;//make a content recording! (record from now)
      }

      Log.WriteFile(Log.LogType.Recorder, "Recorder:   start: {0} {1}", tmpRec.StartTime.ToShortDateString(), tmpRec.StartTime.ToShortTimeString());
      Log.WriteFile(Log.LogType.Recorder, "Recorder:   end  : {0} {1}", tmpRec.EndTime.ToShortDateString(), tmpRec.EndTime.ToShortTimeString());

      AddRecording(ref tmpRec);
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.HandleRecording);
        _listCommands.Add(cmd);
      }
      
    }//static public void RecordNow(string strChannel)

    /// <summary>
    /// Adds a new recording to the tv database and arranges the priority 
    /// </summary>
    /// <param name="rec">new recording to add to the database</param>
    /// <returns></returns>
    static public int AddRecording(ref TVRecording rec)
    {
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      recs.Sort(new TVRecording.PriorityComparer(true));
      int prio = Int32.MaxValue;
      foreach (TVRecording recording in recs)
      {
        if (prio != recording.Priority)
        {
          recording.Priority = prio;
          TVDatabase.SetRecordingPriority(recording);
        }
        prio--;
      }
      rec.Priority = prio;
      return TVDatabase.AddRecording(ref rec);
    }

    /// <summary>
    /// Find a capture card we can use to start a new recording
    /// </summary>
    /// <param name="recordingChannel">Channel we need to record</param>
    /// <param name="terminatePostRecording">
    /// false: use algoritm 1 ( see below)
    /// true: use algoritm 2 ( see below)
    /// </param>
    /// <returns>
    /// -1 : no card found
    /// else card which can be usfed for recording</returns>
    /// <remarks>
    /// MP will first use the following algorithm to find a card to use for the recording:
    ///		- card must be able to record the selected channel
    ///		- card must be free (or viewing the channel we need to record)
    ///		- of all cards found it will use the one with the highest priority
    ///		
    ///	if no card is found then MP will try to use the following algorithm:
    ///		- card must be able to record the selected channel
    ///		- card must be free  (or viewing the channel we need to record) or postrecording on any channel !!!
    ///		- of all cards found it will use the one with the highest priority
    ///	
    ///	Note. If the determined card is in use and the user is currently watching different channel on it
    ///	then the one we need to record then MP will look if there are other cards available with maybe have a
    ///	lower priority. reason for this is that we want to prevent the situation where the user
    ///	is watching channel A, and then when the recording starts on channel B the user suddenly 
    ///	sees channel B
    /// </remarks>
    static private int FindFreeCardForRecording(string recordingChannel, bool stopRecordingsWithLowerPriority, int recordingPrio)
    {
      // if we are viewing a tv channel, and we want to record the program on this channel
      // then just use the same card 
      if (_currentCardIndex >= 0 && _currentCardIndex < _tvcards.Count)
      {
        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        //is card viewing?
        if (dev.View && !dev.IsRecording)
        {
          // is it viewing the channel we want to record?
          if (dev.TVChannel == recordingChannel)
          {
            //then just use the current selected card
            if (dev.UseForRecording) return _currentCardIndex;
          }
        }
      }

      //no, then find another card which is free
      int cardNo = 0;
      int highestPrio = -1;
      int highestCard = -1;
      for (int loop = 0; loop <= 1; loop++)
      {
        highestPrio = -1;
        highestCard = -1;
        cardNo = 0;
        foreach (TVCaptureDevice dev in _tvcards)
        {
          //if we may use the  card for recording tv?
          if (dev.UseForRecording)
          {
            // and is it not recording already?
            // or recording a show which has lower priority then the one we need to record?
            if (!dev.IsRecording || (dev.IsRecording && stopRecordingsWithLowerPriority && dev.CurrentTVRecording.Priority < recordingPrio))
            {
              //and can it receive the channel we want to record?
              if (TVDatabase.CanCardViewTVChannel(recordingChannel, dev.ID) || _tvcards.Count == 1)
              {
                // does this card has the highest priority?
                if (dev.Priority > highestPrio)
                {
                  //yes then we use this card
                  //but do we want to use it?
                  //if the user is using this card to watch tv on another channel
                  //then we prefer to use another tuner for the recording
                  bool preferCard = false;
                  if (_currentCardIndex == cardNo)
                  {
                    //user is watching tv on this tuner
                    if (loop >= 1)
                    {
                      //first loop didnt find any other free card,
                      //so no other choice then to use this one.
                      preferCard = true;
                    }
                    else
                    {
                      //is user watching same channel as we wanna record?
                      if (dev.IsTimeShifting && dev.TVChannel == recordingChannel)
                      {
                        //yes, then he wont notice anything, so we can use the card
                        preferCard = true;
                      }
                    }
                  }
                  else
                  {
                    //user is not using this tuner, so we can use this card
                    preferCard = true;
                  }

                  if (preferCard)
                  {
                    highestPrio = dev.Priority;
                    highestCard = cardNo;
                  }
                }//if (dev.Priority>highestPrio)

                //if this card has the same priority and is already watching this channel
                //then we use this card
                if (dev.Priority == highestPrio)
                {
                  if ((dev.IsTimeShifting || dev.View == true) && dev.TVChannel == recordingChannel)
                  {
                    highestPrio = dev.Priority;
                    highestCard = cardNo;
                  }
                }
              }//if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || _tvcards.Count==1 )
            }//if (!dev.IsRecording)
          }//if (dev.UseForRecording)
          cardNo++;
        }//foreach (TVCaptureDevice dev in _tvcards)
        if (highestCard >= 0)
        {
          return highestCard;
        }
      }//for (int loop=0; loop <= 1; loop++)
      return -1;
    }

    /// <summary>
    /// Start recording a new program
    /// </summary>
    /// <param name="currentTime"></param>
    /// <param name="rec">TVRecording to record <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <param name="currentProgram">TVprogram to record <seealso cref="MediaPortal.TV.Database.TVProgram"/> (can be null)</param>
    /// <param name="iPreRecordInterval">Pre record interval in minutes</param>
    /// <param name="iPostRecordInterval">Post record interval in minutes</param>
    /// <returns>true if recording has been started</returns>
    static bool Record(DateTime currentTime, TVRecording rec, TVProgram currentProgram, int iPreRecordInterval, int iPostRecordInterval)
    {
      if (rec == null) return false;
      if (_state != State.Initialized) return false;
      if (iPreRecordInterval < 0) iPreRecordInterval = 0;
      if (iPostRecordInterval < 0) iPostRecordInterval = 0;

      // Check if we're already recording this...
      foreach (TVCaptureDevice dev in _tvcards)
      {
        if (dev.IsRecording)
        {
          if (dev.CurrentTVRecording.ID == rec.ID)
          {
            //we are alreay recording this schedule, so we can return
            return false;
          }
        }
      }

      // not recording this yet
      Log.WriteFile(Log.LogType.Recorder, "Recorder: time to record '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6} {7}", rec.Title, rec.Channel, rec.StartTime.ToLongTimeString(), rec.EndTime.ToLongTimeString(), rec.ID, rec.Priority, rec.Quality.ToString(), rec.RecType.ToString());
      if (currentProgram != null)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder: program:{0}-{1}", currentProgram.StartTime.ToLongTimeString() , currentProgram.EndTime.ToLongTimeString());
      }
      LogTvStatistics();

      // find free card we can use for recording
      int cardNo = FindFreeCardForRecording(rec.Channel, false, rec.Priority);
      if (cardNo < 0)
      {
        // no card found. 
        //check if this recording has a higher priority then any recordings currently busy
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  No card found, check if a card is recording a show which has a lower priority then priority:{0}", rec.Priority);
        cardNo = FindFreeCardForRecording(rec.Channel, true, rec.Priority);
        if (cardNo < 0)
        {
          //no, other recordings have higher priority...
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  no recordings have a lower priority then priority:{0}", rec.Priority);
          return false;
        }
      }

      //did we find a free tv card for this recording?
      if (cardNo < 0)
      {
        //no then show a recording conflict and let the user choose
        //which recording to stop
        GUIDialogMenuBottomRight pDlgOK = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT) as GUIDialogMenuBottomRight;
        pDlgOK.Reset();
        pDlgOK.SetHeading(879);//Recording Conflict
        pDlgOK.SetHeadingRow2(GUILocalizeStrings.Get(880) + " " + rec.Channel);
        pDlgOK.SetHeadingRow3(881);
        int cardWithLowestPriority = -1;
        int lowestPriority = TVRecording.HighestPriority;
        int count = 0;
        for (int i = 0; i < _tvcards.Count; i++)
        {
          TVCaptureDevice dev = _tvcards[i];
          if (!dev.IsRecording) continue;
          if (dev.CurrentTVRecording.Channel == rec.Channel)
          {
            if (dev.IsPostRecording) return false;
          }

          GUIListItem item = new GUIListItem();
          item.Label = dev.CurrentTVRecording.Title;
          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, dev.CurrentTVRecording.Channel);
          if (System.IO.File.Exists(strLogo))
          {
            item.IconImage = strLogo;
          }
          pDlgOK.Add(item);
          int prio = dev.CurrentTVRecording.Priority;
          if (prio < lowestPriority)
          {
            cardWithLowestPriority = i;
            lowestPriority = prio;
          }
          count++;
        }

        if (count > 0)
        {
          pDlgOK.TimeOut = 60;
          pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          if (pDlgOK.TimedOut)
          {
            cardNo = cardWithLowestPriority;
            TVCaptureDevice dev = _tvcards[cardNo];
            Log.WriteFile(Log.LogType.Recorder, "Recorder: Canceled recording:{0} priority:{1} on card:{2}",
                           dev.CurrentTVRecording.ToString(),
                           dev.CurrentTVRecording.Priority,
                           dev.ID);
            HandleStopTvRecording(dev.CurrentTVRecording);
          }
          else
          {
            int selectedIndex = pDlgOK.SelectedLabel;
            if (selectedIndex >= 0)
            {
              for (int i = 0; i < _tvcards.Count; ++i)
              {
                TVCaptureDevice dev = _tvcards[i];
                if (dev.IsRecording)
                {
                  if (count == selectedIndex)
                  {
                    cardNo = i;
                    Log.WriteFile(Log.LogType.Recorder, "Recorder: User canceled recording:{0} priority:{1} on card:{2}",
                      dev.CurrentTVRecording.ToString(),
                      dev.CurrentTVRecording.Priority,
                      dev.ID);
                    HandleStopTvRecording(dev.CurrentTVRecording);
                    break;
                  }
                  count++;
                }
              }
            }
          }
        }
        if (cardNo < 0)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  no card available for recording");
          return false;//no card free
        }
      }

      //now we have a free card
      TVCaptureDevice card = _tvcards[cardNo];
      Log.WriteFile(Log.LogType.Recorder, "Recorder:  using card:{0} prio:{1}", card.ID, card.Priority);
      if (card.IsRecording)
      {
        //if its recording, we cancel it 
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  card:{0} was recording. Now use it for recording new program", card.ID);
        Log.WriteFile(Log.LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", card.ID, card.TVChannel);
        if (card.CurrentTVRecording.RecType == TVRecording.RecordingType.Once)
        {
          card.CurrentTVRecording.Canceled = Utils.datetolong(DateTime.Now);
        }
        else
        {
          long datetime = Utils.datetolong(DateTime.Now);
          TVProgram prog = card.CurrentProgramRecording;
          if (prog != null) datetime = Utils.datetolong(prog.StartTime);
          card.CurrentTVRecording.CanceledSeries.Add(datetime);
        }
        //and stop the recording
        TVDatabase.UpdateRecording(card.CurrentTVRecording, TVDatabase.RecordingChange.Canceled);
        card.StopRecording();
      }

      //finally start recording...
      TuneExternalChannel(rec.Channel, false);
      card.Record(rec, currentProgram, iPostRecordInterval, iPostRecordInterval);

      //if the user was using this card to watch tv, then start watching it also
      if (_currentCardIndex == cardNo)
      {
        TVChannelName = rec.Channel;
        HandleViewCommand(rec.Channel, true, true);
      }
      return true;
    }//static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)


    /// <summary>
    /// Tell the recorder to stop recording the schedule specified in rec
    /// </summary>
    /// <param name="rec">recording to stop</param>
    static public void StopRecording(TVRecording rec)
    {
      //add a new RecorderCommand which holds 'rec'
      //and tell the process thread to handle it
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRecording({0})", rec.Title);
      if (_state != State.Initialized) return;
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopRecording, rec);
        _listCommands.Add(cmd);
      }
    }
    /// <summary>
    /// This method is called by the process thread when it received a StopRecording() command
    /// it will check which card is currently recording the specified recording
    /// and tell it to stop it.
    /// </summary>
    /// <param name="rec">recording to cancel</param>
    static void HandleStopTvRecording(TVRecording rec)
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:HandleStopTvRecording()");
      if (rec == null) return;
      //find card which currently records the 'rec'
      for (int card = 0; card < _tvcards.Count; ++card)
      {
        TVCaptureDevice dev = _tvcards[card];
        //is this card recording
        if (dev.IsRecording)
        {
          //yes, is it recording the 'rec' ?
          if (dev.CurrentTVRecording.ID == rec.ID)
          {
            //yep then cancel the recording
            if (rec.RecType == TVRecording.RecordingType.Once)
            {
              Log.WriteFile(Log.LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", dev.ID, dev.TVChannel);
              rec.Canceled = Utils.datetolong(DateTime.Now);
            }
            else
            {
              Log.WriteFile(Log.LogType.Recorder, "Recorder: Stop serie of recording card:{0} channel:{1}", dev.ID, dev.TVChannel);
              long datetime = Utils.datetolong(DateTime.Now);
              TVProgram prog = dev.CurrentProgramRecording;
              if (prog != null) datetime = Utils.datetolong(prog.StartTime);
              rec.CanceledSeries.Add(datetime);
              rec.Canceled = 0;
            }
            //and tell the card to stop recording
            TVDatabase.UpdateRecording(rec, TVDatabase.RecordingChange.Canceled);
            dev.StopRecording();

            //if we're not viewing this card
            if (_currentCardIndex != card)
            {
              //then stop card
              dev.Stop();
            }
          }
        }
      }
    }//StopRecording

    /// <summary>
    /// Stops all recording on the current channel. 
    /// </summary>
    /// <remarks>
    /// Only stops recording. timeshifting wont be stopped so user can continue to watch the channel
    /// </remarks>
    static public void StopRecording()
    {
      //add a new RecorderCommand to tell the process thread
      //that it must stop recording
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRecording()");
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopRecording);
        _listCommands.Add(cmd);
      }
    }
    /// <summary>
    /// this methods will be called by the process thread when it receives a stoprecording command
    /// It will stop any recording on the currently selected card
    /// </summary>
    static void HandleStopRecording()
    {
      if (_state != State.Initialized) return;
      Log.WriteFile(Log.LogType.Recorder, "Recorder:HandleStopRecording()");

      //get the current selected card
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];

      //is it recording?
      if (dev.IsRecording)
      {
        //yes. then cancel the recording
        Log.WriteFile(Log.LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", dev.ID, dev.TVChannel);
        int ID = dev.CurrentTVRecording.ID;

        if (dev.CurrentTVRecording.RecType == TVRecording.RecordingType.Once)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel recording");
          dev.CurrentTVRecording.Canceled = Utils.datetolong(DateTime.Now);
        }
        else
        {
          long datetime = Utils.datetolong(DateTime.Now);
          TVProgram prog = dev.CurrentProgramRecording;
          Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel {0}", prog);

          if (prog != null)
          {
            datetime = Utils.datetolong(prog.StartTime);
            Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel serie {0} {1} {2}", prog.Title,prog.StartTime.ToLongDateString(), prog.StartTime.ToLongTimeString());
          }
          else
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder: cancel series");
          }
          dev.CurrentTVRecording.CanceledSeries.Add(datetime);
        }
        TVDatabase.UpdateRecording(dev.CurrentTVRecording, TVDatabase.RecordingChange.Canceled);

        //and tell the card to stop the recording
        dev.StopRecording();
        _recordingsListChanged = true;
      }
    }//static public void StopRecording()

    #endregion

    #region Properties
    static public bool Paused
    {
      get { return _isPaused; }
      set
      {
        if (_isPaused == value) return;
        if (value)
        {
          lock (_listCommands)
          {
            RecorderCommand cmd = new RecorderCommand(RecorderCommandType.Paused);
            _listCommands.Add(cmd);
          }
          while (_listCommands.Count > 0)
            System.Threading.Thread.Sleep(100);
        }
        _isPaused = value; 

      }
    }
    /// <summary>
    /// Property which returns if any tvcard is recording
    /// </summary>
    static public bool IsAnyCardRecording()
    {
      foreach (TVCaptureDevice dev in _tvcards)
      {
        if (dev.IsRecording) return true;
      }
      return false;
    }//static public bool IsAnyCardRecording()

    /// <summary>
    /// Property which returns if any card is recording the specified tv channel
    /// </summary>
    static public bool IsRecordingChannel(string channel)
    {
      if (_state != State.Initialized) return false;

      foreach (TVCaptureDevice dev in _tvcards)
      {
        if (dev.IsRecording && dev.CurrentTVRecording.Channel == channel) return true;
      }
      return false;
    }//static public bool IsRecordingChannel(string channel)


    /// <summary>
    /// Property which returns if current selected card is recording
    /// </summary>
    static public bool IsRecording()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;

      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.IsRecording;
    }//static public bool IsRecording()

    /// <summary>
    /// Property which returns if current tv channel has teletext or not
    /// </summary>
    static public bool HasTeletext()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.HasTeletext;
    }

    /// <summary>
    /// Property which returns if current selected card supports timeshifting
    /// </summary>
    static public bool DoesSupportTimeshifting()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;

      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.SupportsTimeShifting;
    }//static public bool DoesSupportTimeshifting()

    /// <summary>
    /// Property which returns the friendly name for a card
    /// </summary>
    /// <param name="card">tv card index</param>
    /// <returns>string which contains the friendly name</returns>
    static public string GetFriendlyNameForCard(int card)
    {
      if (_state != State.Initialized) return String.Empty;
      if (card < 0 || card >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[card];
      return dev.FriendlyName;
    }//static public string GetFriendlyNameForCard(int card)

    /// <summary>
    /// Returns the tv hannel name of the channel we're currently watching
    /// </summary>
    /// <returns>
    /// Returns the Channel name of the channel we're currently watching
    /// </returns>
    static public string GetTVChannelName()
    {
      if (_state != State.Initialized) return String.Empty;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return String.Empty;

      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.TVChannel;
    }//static public string GetTVChannelName()

    /// <summary>
    /// Returns the TV Recording we're currently recording
    /// </summary>
    /// <returns>
    /// </returns>
    static public TVRecording GetTVRecording()
    {
      if (_state != State.Initialized) return null;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return null;

      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      if (dev.IsRecording) return dev.CurrentTVRecording;
      return null;
    }//static public TVRecording GetTVRecording()

    /// <summary>
    /// Checks if a tv card is recording the TVRecording specified in 'rec
    /// </summary>
    /// <param name="rec">TVRecording <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <param name="card">the card index which is currently recording the 'rec'></param>
    /// <returns>true if a card is recording the specified TVRecording, else false</returns>
    static public bool IsRecordingSchedule(TVRecording rec, out int card)
    {
      card = -1;
      if (rec == null) return false;
      if (_state != State.Initialized) return false;
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

    /// <summary>
    /// Property which returns the current program being recorded. 
    /// If no programs are being recorded at the moment
    /// it will return null;
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    static public TVProgram ProgramRecording
    {
      get
      {
        if (_state != State.Initialized) return null;
        if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return null;
        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        if (dev.IsRecording) return dev.CurrentProgramRecording;
        return null;
      }
    }//static public TVProgram ProgramRecording

    /// <summary>
    /// Property which returns the current TVRecording being recorded. 
    /// If no recordings are being recorded at the moment
    /// it will return null;
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Database.TVRecording"/>
    static public TVRecording CurrentTVRecording
    {
      get
      {
        if (_state != State.Initialized) return null;
        if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return null;
        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        if (dev.IsRecording) return dev.CurrentTVRecording;
        return null;
      }
    }//static public TVRecording CurrentTVRecording


    /// <summary>
    /// Returns true if we're timeshifting
    /// </summary>
    /// <returns></returns>
    static public bool IsTimeShifting()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      if (dev.IsTimeShifting) return true;
      return false;
    }//static public bool IsTimeShifting()

    /// <summary>
    /// Returns true if we're watching live tv
    /// </summary>
    /// <returns></returns>
    static public bool IsViewing()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      if (dev.View) return true;
      if (dev.IsTimeShifting)
      {
        string fileName = GetTimeShiftFileName(_currentCardIndex);
        if (g_Player.Playing && g_Player.CurrentFile == fileName)
          return true;
        if (System.IO.File.Exists(fileName))
          return true;
      }
      return false;
    }//static public bool IsViewing()

    /// <summary>
    /// Property which returns true if the card specified by the 'cardId' is currently used
    /// for viewing tv
    /// </summary>
    /// <param name="cardId">id of tv card to check</param>
    /// <returns></returns>
    static public bool IsCardViewing(int cardId)
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      if (dev.ID != cardId) return false;
      if (dev.View) return true;
      if (dev.IsTimeShifting)
      {
        if (g_Player.Playing && g_Player.CurrentFile == GetTimeShiftFileName(_currentCardIndex))
          return true;
      }
      return false;
    }//static public bool IsViewing()

    /// <summary>
    /// Property which get TV Viewing mode.
    /// if TV Viewing  mode is turned on then live tv will be shown
    /// </summary>
    static public bool View
    {
      get
      {
        //return if we're viewing tv or not.
        //if we're playing a timeshift file, then we're watching tv so return true
        if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording) return true;

        //if any card is in 'View' mode then return true also
        for (int i = 0; i < _tvcards.Count; ++i)
        {
          TVCaptureDevice dev = _tvcards[i];
          if (dev.View) return true;
        }

        //no card is used for viewing tv
        return false;
      }
    }//static public bool View

    /// <summary>
    /// property which returns the date&time the current recording was started
    /// </summary>
    static public DateTime TimeRecordingStarted
    {
      get
      {
        if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return DateTime.Now;
        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        if (dev.IsRecording)
        {
          return dev.TimeRecordingStarted;
        }
        return DateTime.Now;
      }
    }

    /// <summary>
    /// property which returns the date&time that timeshifting  was started
    /// </summary>
    static public DateTime TimeTimeshiftingStarted
    {
      get
      {
        if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return DateTime.Now;
        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        if (!dev.IsRecording && dev.IsTimeShifting)
        {
          return dev.TimeShiftingStarted;
        }
        return DateTime.Now;
      }
    }

    /// <summary>
    /// Returns the number of tv cards configured
    /// </summary>
    static public int Count
    {
      get { return _tvcards.Count; }
    }

    /// <summary>
    /// Indexer which returns the TVCaptureDevice object for a given card
    /// </summary>
    /// <param name="index">card number (0-Count)</param>
    /// <returns>TVCaptureDevice object</returns>
    static public TVCaptureDevice Get(int index)
    {
      if (index < 0 || index >= _tvcards.Count) return null;
      return _tvcards[index] as TVCaptureDevice;
    }

    /// <summary>
    /// returns the name of the current tv channel we're watching
    /// </summary>
    static public string TVChannelName
    {
      get { return _tvChannel; }
      set
      {
        if (value != _tvChannel)
        {
          _tvChannel = value;
          if (OnTvChannelChanged != null)
            OnTvChannelChanged(_tvChannel);
          //SetZapOSDData(_tvChannel);
        }
      }
    }

    // this sets the channel to render the osd
    static void SetZapOSDData(string channelName)
    {
      if (_state != State.Initialized) return;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];

      TVChannel channel = null;
      foreach (TVChannel chan in _tvChannelsList)
      {
        if (chan.Name == channelName)
        {
          channel = chan;
          break;
        }
      }
      if (channel == null) return;

      if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
        return;
      if (_vmr9Osd != null && channel != null && _useVmr9Zap == true)
      {
        int level = dev.SignalStrength;
        int quality = dev.SignalQuality;
        _vmr9Osd.RenderZapOSD(channel, quality, level);
      }
    }
    /// <summary>
    /// Property which returns the Signal Strength of the current tv card used
    /// </summary>
    static public int SignalStrength
    {
      get
      {
        if (_state != State.Initialized) return 0;
        if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return 0;

        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        return dev.SignalStrength;
      }
    }

    /// <summary>
    /// Property which returns the signal quality of the current tv card used
    /// </summary>
    static public int SignalQuality
    {
      get
      {
        if (_state != State.Initialized) return 0;
        if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return 0;

        TVCaptureDevice dev = _tvcards[_currentCardIndex];
        return dev.SignalQuality;
      }
    }

    /// <summary>
    /// Property to get the recording filename for a given tv recording
    /// </summary>
    /// <param name="rec">recording</param>
    /// <returns>filename for the recording</returns>
    static public string GetRecordingFileName(TVRecording rec)
    {
      int card;
      if (!IsRecordingSchedule(rec, out card)) return String.Empty;
      TVCaptureDevice dev = _tvcards[card] as TVCaptureDevice;

      return dev.RecordingFileName;
    }

    /// <summary>
    /// Property which returns the timeshifting filename for the current selected card
    /// </summary>
    /// <returns>filename of the timeshifting file</returns>
    static public string GetTimeShiftFileName()
    {
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      if (!dev.IsTimeShifting) return String.Empty;

      string FileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, _currentCardIndex + 1, dev.TimeShiftFileName);
      return FileName;
    }

    /// <summary>
    /// Property which returns the timeshifting filename for a specific card
    /// </summary>
    /// <param name="card">card index</param>
    /// <returns>filename of the timeshifting file</returns>
    static public string GetTimeShiftFileName(int card)
    {
      if (card < 0 || card >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[card];
      string FileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, card + 1, dev.TimeShiftFileName);
      return FileName;
    }

    /// <summary>
    /// Property which returns the timeshifting filename for a specific card id
    /// </summary>
    /// <param name="card">card id</param>
    /// <returns>filename of the timeshifting file</returns>
    static public string GetTimeShiftFileNameByCardId(int cardId)
    {
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        if (dev.ID == cardId)
        {
          string FileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, i + 1, dev.TimeShiftFileName);
          return FileName;
        }
      }
      return String.Empty;
    }




    #endregion

    #region Radio
    /// <summary>
    /// Property which returns true if we currently listening to a radio station
    /// </summary>
    /// <returns>true if listening to radio</returns>
    static public bool IsRadio()
    {
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        if (dev.IsRadio)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// When listening to radio, this property returns the name of the current radio stations
    /// </summary>
    /// <returns>station name when listening to radio, else emty string</returns>
    static public string RadioStationName()
    {
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        if (dev.IsRadio)
        {
          return dev.RadioStation;
        }
      }
      return string.Empty;
    }

    /// <summary>
    /// Stop listening to radio
    /// </summary>
    static public void StopRadio()
    {
      //Send command to process thread to stop listening to radio
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopRadio);
        _listCommands.Add(cmd);
      }
    }
    /// <summary>
    /// this method will be called by the process thread when it received a StopRadio command.
    /// It will simply stop radio on any card
    /// </summary>
    static void HandleStopRadio()
    {
      Log.Write("playing{0} radio:{1}", g_Player.Playing, g_Player.IsRadio);
      if (g_Player.Playing && g_Player.IsRadio)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }
      foreach (TVCaptureDevice dev in _tvcards)
      {
        if (dev.IsRadio)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRadio() stop radio on card:{0}", dev.ID);
          dev.Stop();
        }
      }
    }//stopRadio()

    /// <summary>
    /// Start listening to a radio channel
    /// </summary>
    /// <param name="radioStationName"></param>
    static public void StartRadio(string radioStationName)
    {
      //Send command to process thread to start listening to radio
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StartRadio);
        cmd.Channel = radioStationName;
        _listCommands.Add(cmd);
      }
    }
    /// <summary>
    /// this method will be called by the process thread when it received a StartRadio command.
    /// It will find a card which can receive this radio station and when found tell the
    /// card to tune to the radio station and play it
    /// </summary>
    static void HandleStartRadio(string radioStationName)
    {
      if (radioStationName == null)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio() listening radioStation=null?");
        return;
      }
      if (radioStationName == String.Empty)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio() listening radioStation=empty");
      }
      if (_state != State.Initialized)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio() but recorder is not initalised");
        return;
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msg);
      HandleStopAllViewing();
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio():{0}", radioStationName);
      RadioStation radiostation;
      if (!RadioDatabase.GetStation(radioStationName, out radiostation))
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio()  unknown station:{0}", radioStationName);
        return;
      }

      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice tvcard = _tvcards[i];
        if (!tvcard.IsRecording)
        {
          if (RadioDatabase.CanCardTuneToStation(radioStationName, tvcard.ID) || _tvcards.Count == 1)
          {
            for (int x = 0; x < _tvcards.Count; ++x)
            {
              TVCaptureDevice dev = _tvcards[x];
              if (i != x)
              {
                if (dev.IsRadio)
                {
                  dev.Stop();
                }
              }
            }
            _currentCardIndex = i;
            Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio()  start on card:{0} station:{1}", tvcard.ID, radioStationName);
            tvcard.StartRadio(radiostation);
            /*if (tvcard.IsTimeShifting)
            {
              string strTimeShiftFileName=GetTimeShiftFileNameByCardId(tvcard.ID);

              Log.WriteFile(Log.LogType.Recorder,"Recorder:  currentfile:{0} newfile:{1}", g_Player.CurrentFile,strTimeShiftFileName);
              g_Player.Play(strTimeShiftFileName);
            }*/
            return;
          }
        }
      }
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StartRadio()  no free card which can listen to radio channel:{0}", radioStationName);
    }//StartRadio

    #endregion

    #region TV watching
    /// <summary>
    /// Stop viewing on all cards
    /// </summary>
    static bool reEntrantStopViewing = false;
    static public void StopViewing()
    {
      if (reEntrantStopViewing) return;
      try
      {
        if (_isPaused) return;
        reEntrantStopViewing = true;

        //send Recorder command to process thread to stop viewing
        Log.WriteFile(Log.LogType.Recorder, "Recorder:Stopviewing()");
        lock (_listCommands)
        {
          RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopAllViewing);
          _listCommands.Add(cmd);
        }
        //wait till thread finished this command
        while (_listCommands.Count > 0)
        {
          GUIWindowManager.Process();
          System.Threading.Thread.Sleep(10);
        }
      }
      finally
      {
        reEntrantStopViewing = false;
      }
    }
    /// <summary>
    /// This method gets called by the process thread when it receives a StopAllViewing command.
    /// It will enumerate all tv cards and stop any card which is viewing tv
    /// </summary>
    static void HandleStopAllViewing()
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopAllViewing()");
      LogTvStatistics();

      // stop playback..
      if (g_Player.Playing && g_Player.IsTV)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }

      // stop any card viewing..
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev;
        dev = _tvcards[i];
        if (!dev.IsRecording)
        {
          bool stopped = false;
          if (dev.IsTimeShifting)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  stop timeshifting card {0} channel:{1}", dev.ID, dev.TVChannel);
            dev.StopTimeShifting();
            stopped = true;
          }
          if (dev.View)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  stop viewing card {0} channel:{1}", dev.ID, dev.TVChannel);
            dev.View = false;
            stopped = true;
          }
          dev.DeleteGraph();
          if (stopped && OnTvViewingStopped != null)
            OnTvViewingStopped(i, dev);
        }
      }
      _currentCardIndex = -1;
    }//static public void StopViewing()


    /// <summary>
    /// Turns of watching TV /radio on all capture cards
    /// </summary>
    /// <param name="exceptCard">
    /// index in _tvcards so 0<= exceptCard< _tvcards.Count
    /// if exceptCard==-1 then tv/radio is turned on all cards
    /// else this tells which card should be ignored and not turned off 
    /// </param>
    /// <remarks>
    /// Only viewing is stopped. If a card is recording then this wont be stopped
    /// </remarks>
    static private void TurnTvOff(int exceptCard)
    {
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        if (i == exceptCard) continue;

        bool stopped = false;
        TVCaptureDevice dev = _tvcards[i];
        string strTimeShiftFileName = GetTimeShiftFileName(i);
        if (dev.SupportsTimeShifting)
        {
          if (g_Player.CurrentFile == strTimeShiftFileName)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  stop playing timeshifting file for card:{0}", dev.ID);

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
            stopped = true;
          }
        }

        //if card is not recording, then stop the card
        if (!dev.IsRecording)
        {
          if (dev.IsTimeShifting || dev.View || dev.IsRadio)
          {
            stopped = (dev.View);
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  stop card:{0}", dev.ID);
            dev.Stop();
          }
        }
        if (stopped && OnTvViewingStopped != null)
          OnTvViewingStopped(i, dev);
      }
    }//TurnTvOff(int exceptCard)

    /// <summary>
    /// Start watching TV.
    /// </summary>
    /// <param name="channel">name of the tv channel</param>
    /// <param name="TVOnOff">
    /// true : turn tv on (start watching)
    /// false: turn tv off (stop watching)
    /// </param>
    /// <param name="timeshift">
    /// true: use timeshifting if possible
    /// false: dont use timeshifting
    /// </param>
    /// <remarks>
    /// The following algorithm is used to determine which tuner card will be used:
    /// 1. if a card is already recording the channel requested then that card will be used for viewing
    ///    by just starting to play the timeshift buffer of the card
    /// 2. if a card is already timeshifting (on same or other channel) and it can also view
    ///    the channel requested, then that card will be used for viewing
    /// else MP will determine which card:
    ///   - is free
    ///   - has the highest priority
    ///   - and can view the selected tv channel
    /// if it finds a card matching these criteria it will start viewing on the card found
    /// </remarks>
    static bool reEntrantStartViewing = false;
    static public void StartViewing(string channel, bool TVOnOff, bool timeshift)
    {
      if (reEntrantStartViewing) return;
      try
      {

        if (_isPaused) return;
        reEntrantStartViewing = true;
        if (TVOnOff)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:StartViewing on:{0} {1} {2}", channel, TVOnOff, timeshift);
          lock (_listCommands)
          {
            RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StartViewing, channel, timeshift);
            _listCommands.Add(cmd);
          }
        }
        else
        {

          Log.WriteFile(Log.LogType.Recorder, "Recorder:StartViewing off:{0} {1} {2}", channel, TVOnOff, timeshift);
          lock (_listCommands)
          {
            RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopViewing, channel, timeshift);
            _listCommands.Add(cmd);
          }
        }
        //wait till thread finished this command
        while (_listCommands.Count > 0)
        {
          GUIWindowManager.Process();
          System.Threading.Thread.Sleep(10);
        }
        Log.WriteFile(Log.LogType.Recorder, "Recorder:StartViewing off:{0} {1} {2} done", channel, TVOnOff, timeshift);
      }
      finally
      {
        reEntrantStartViewing = false;
      }
    }

    
    static public bool IsBusyProcessingCommands
    {
      get
      {
        return (_listCommands.Count > 0);
      }
    }

    static void HandleViewCommand(string channel, bool TVOnOff, bool timeshift)
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:HandleView {0} {1} {2}", channel, TVOnOff, timeshift);
      // checks
      if (channel == null)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:Start TV viewing channel=null?");
        return;
      }
      if (channel == String.Empty)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:Start TV viewing channel=empty");
        return;
      }
      if (_state != State.Initialized)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:Start TV viewing but recorder is not initalised");
        Recorder.Start();
        return;
      }

      Log.WriteFile(Log.LogType.Recorder, "Recorder:StartViewing() channel:{0} tvon:{1} timeshift:{2} vmr9:{3}",
                    channel, TVOnOff, timeshift, GUIGraphicsContext.Vmr9Active);
      TVCaptureDevice dev;
      LogTvStatistics();

      string strTimeShiftFileName;
      if (TVOnOff == false)
      {
        TurnTvOff(-1);
        TVChannelName = String.Empty;
        _currentCardIndex = -1;
        return;
      }

      Log.WriteFile(Log.LogType.Recorder, "Recorder:  Turn tv on channel:{0}", channel);

      int cardNo = -1;
      // tv should be turned on
      // check if any card is already tuned to this channel...
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        dev = _tvcards[i];
        //is card already viewing ?
        if (dev.IsTimeShifting || dev.View)
        {
          //can card view the new channel we want?
          if (TVDatabase.CanCardViewTVChannel(channel, dev.ID) || _tvcards.Count == 1)
          {
            // is it not recording ? or is it recording the channel we want to watch ?
            if (!dev.IsRecording || (dev.IsRecording && dev.TVChannel == channel))
            {
              if (dev.IsRecording)
              {
                cardNo = i;
                break;
              }
              cardNo = i;
            }
          }
        }
      }//for (int i=0; i < _tvcards.Count;++i)

      if (cardNo >= 0)
      {
        dev = _tvcards[cardNo];
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  Found card:{0}", dev.ID);

        //stop viewing on any other card
        TurnTvOff(cardNo);

        _currentCardIndex = cardNo;
        TVChannelName = channel;

        // do we want timeshifting?
        if (timeshift || dev.IsRecording)
        {
          //yes
          strTimeShiftFileName = GetTimeShiftFileName(_currentCardIndex);
          if (g_Player.CurrentFile != strTimeShiftFileName)
          {

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
          if (dev.TVChannel != channel)
          {
            TuneExternalChannel(channel, true);
            dev.TVChannel = channel;
          }
          if (!dev.IsRecording && !dev.IsTimeShifting && dev.SupportsTimeShifting)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  start timeshifting on card:{0}", dev.ID);
            dev.StartTimeShifting();
          }

          //yes, check if we're already playing/watching it
          strTimeShiftFileName = GetTimeShiftFileName(_currentCardIndex);
          if (g_Player.CurrentFile != strTimeShiftFileName)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  start viewing timeshift file of card {0}", dev.ID);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
            msg.Label = strTimeShiftFileName;
            GUIGraphicsContext.SendMessage(msg);
          }
          if (OnTvViewingStarted != null)
            OnTvViewingStarted(_currentCardIndex, dev);
          _killTimeshiftingTimer = DateTime.Now;
          return;
        }//if  (timeshift || dev.IsRecording)
        else
        {
          //we dont want timeshifting
          strTimeShiftFileName = GetTimeShiftFileName(_currentCardIndex);
          if (g_Player.CurrentFile == strTimeShiftFileName)
          {

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
          if (dev.IsTimeShifting)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  stop timeshifting on card:{0}", dev.ID);
            dev.StopTimeShifting();
          }
          if (dev.TVChannel != channel)
          {
            TuneExternalChannel(channel, true);
            dev.TVChannel = channel;
          }
          dev.View = true;
          if (OnTvViewingStarted != null)
            OnTvViewingStarted(_currentCardIndex, dev);
          _killTimeshiftingTimer = DateTime.Now;
          return;
        }
      }//if (cardNo>=0)

      Log.WriteFile(Log.LogType.Recorder, "Recorder:  find free card");

      TurnTvOff(-1);

      // no cards are timeshifting the channel we want.
      // Find a card which can view the channel
      int card = -1;
      int prio = -1;
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        dev = _tvcards[i];
        if (!dev.IsRecording)
        {
          if (TVDatabase.CanCardViewTVChannel(channel, dev.ID) || _tvcards.Count == 1)
          {
            if (dev.Priority > prio)
            {
              card = i;
              prio = dev.Priority;
            }
          }
        }
      }

      if (card < 0)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  No free card which can receive channel [{0}]", channel);
        return; // no card available
      }

      _currentCardIndex = card;
      TVChannelName = channel;
      dev = _tvcards[_currentCardIndex];

      Log.WriteFile(Log.LogType.Recorder, "Recorder:  found free card {0} prio:{1} name:{2}", dev.ID, dev.Priority, dev.CommercialName);

      //do we want to use timeshifting ?
      if (timeshift)
      {
        // yep, then turn timeshifting on
        strTimeShiftFileName = GetTimeShiftFileName(_currentCardIndex);
        if (g_Player.CurrentFile != strTimeShiftFileName)
        {

          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
        // yes, does card support it?
        if (dev.SupportsTimeShifting)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  start timeshifting card {0} channel:{1}", dev.ID, channel);
          TuneExternalChannel(channel, true);
          dev.TVChannel = channel;
          dev.StartTimeShifting();
          TVChannelName = channel;

          // and play the timeshift file (if its not already playing it)
          strTimeShiftFileName = GetTimeShiftFileName(_currentCardIndex);
          if (g_Player.CurrentFile != strTimeShiftFileName)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:  currentfile:{0} newfile:{1}", g_Player.CurrentFile, strTimeShiftFileName);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
            msg.Label = strTimeShiftFileName;
            GUIGraphicsContext.SendMessage(msg);
          }
          if (OnTvViewingStarted != null)
            OnTvViewingStarted(_currentCardIndex, dev);
          _killTimeshiftingTimer = DateTime.Now;
          return;
        }//if (dev.SupportsTimeShifting)
      }//if (timeshift)

      //tv should be turned on without timeshifting
      //just present the overlay tv view
      // now start watching on our card
      Log.WriteFile(Log.LogType.Recorder, "Recorder:  start watching on card:{0} channel:{1}", dev.ID, channel);
      TuneExternalChannel(channel, true);
      dev.TVChannel = channel;
      dev.View = true;
      TVChannelName = channel;
      if (OnTvViewingStarted != null)
        OnTvViewingStarted(_currentCardIndex, dev);
      _killTimeshiftingTimer = DateTime.Now;
    }//static public void StartViewing(string channel, bool TVOnOff, bool timeshift)

    #endregion

    #region Process and properties
    #region auto epg grabber
    static DateTime _epgTimer = DateTime.MinValue;
    
    static void ProcessEpg()
    {
      TimeSpan ts = DateTime.Now - _epgTimer;
      if (ts.TotalMilliseconds < 30000) return;

      //Log.WriteFile(Log.LogType.EPG, "epg grabber process");
      bool isGrabbing = false;
      foreach (TVCaptureDevice card in _tvcards)
      {
        //card is empty
        if (card.Network == NetworkType.Analog) continue;
        if (card.IsRadio || card.IsRecording || card.IsTimeShifting || card.View) continue;

        if (card.IsEpgGrabbing)
        {
          isGrabbing = true;
          card.Process();
          if (card.IsEpgFinished)
          {
            card.DeleteGraph();
            //reload tv channels...
            _tvChannelsList.Clear();
            TVDatabase.GetChannels(ref _tvChannelsList);
          }
        }
      }
      if (isGrabbing) return;
      foreach (TVCaptureDevice card in _tvcards)
      {
        //card is empty
        if (card.Network == NetworkType.Analog) continue;
        if (card.IsRadio || card.IsRecording || card.IsTimeShifting || card.View) continue;
//      Log.WriteFile(Log.LogType.EPG, "card :{0} idle", card.ID);
        foreach (TVChannel chan in _tvChannelsList)
        {
          if (_listCommands.Count > 0) break;
          if (!chan.AutoGrabEpg) continue;
          if (_tvcards.Count !=1)
          {
            if (TVDatabase.CanCardViewTVChannel(chan.Name, card.ID) == false) continue;
          }

          //Log.WriteFile(Log.LogType.EPG, "  card:{0} ch:{1} epg hrs:{2} last:{3} {4} hrs:{5}", card.ID,chan.Name,chan.EpgHours, chan.LastDateTimeEpgGrabbed.ToShortDateString(), chan.LastDateTimeEpgGrabbed.ToLongTimeString(),ts.TotalHours);
          TVProgram prog = TVDatabase.GetLastProgramForChannel(chan);
          //Log.WriteFile(Log.LogType.EPG, "last prog in tvguide:{0} {1}", prog.EndTime.ToShortDateString(), prog.EndTime.ToLongTimeString());
          if (prog.EndTime < DateTime.Now.AddHours(chan.EpgHours))
          {
            ts = DateTime.Now - chan.LastDateTimeEpgGrabbed;
            if (ts.TotalHours > 2  )
            {
                //grab the epg
                Log.WriteFile(Log.LogType.EPG, "auto-epg: card:{0} grab epg for channel:{1} expected:{2} hours, last event in tv guide:{3} {4}, last grab :{5} {6}",
                            card.ID,
                            chan.Name, chan.EpgHours, prog.EndTime.ToShortDateString(), prog.EndTime.ToLongTimeString(),
                             chan.LastDateTimeEpgGrabbed.Date.ToShortDateString(),chan.LastDateTimeEpgGrabbed.Date.ToLongTimeString() );
              card.GrabEpg(chan);
              chan.LastDateTimeEpgGrabbed = DateTime.Now;
              TVDatabase.UpdateChannel(chan, chan.Sort);
              _epgTimer = DateTime.Now;
              return;
            }
          }
        }
      }
      _epgTimer = DateTime.Now;
    }
    #endregion

    /// <summary>
    /// All things needed to handle the tv cards is done is this thread. We use a seperate thread
    /// since some actions are time-consuming like starting/stopping a card. By using a seperate
    /// thread the main GUI can continue doing rendering work
    /// Things done in this thread:
    ///   - auto epg grabber. 
    ///       The auto epg grabber will grab the EPG from DVB cards when they are idle and update
    ///       the epg database
    ///   - disk management.
    ///       Will delete any old recordings when space is needed and/or max. number of episodes for
    ///       a recording has been reached
    ///   - recordings
    ///       it will check periodicaly if a new recording should be started (or stopped)
    ///   - notifies
    ///       it will send notifications to the GUI when a new program is about to start
    ///   - recorder commands
    ///       The gui can submit commands by adding them to _listCommands to for example
    ///       start viewing tv, stop viewing tv, zap channels etc.
    ///       These commands will be processed and handled by this thread
    ///     
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void ProcessThread(object sender, DoWorkEventArgs e)
    {
      System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
      DateTime recTimer = DateTime.Now;
      while (_state == State.Initialized)
      {
        try
        {
          System.Threading.Thread.Sleep(500);

          if (_isPaused) continue;

          //process auto epg grabber
          if (_autoGrabEpg)
            ProcessEpg();

          //process all cards
          ProcessCards();

          //handle the recordings every 10 secs
          TimeSpan ts = DateTime.Now - recTimer;
          if (ts.TotalSeconds >= 10 || _recordingsListChanged)
          {
            Recorder.HandleRecordings();
            recTimer = DateTime.Now;
          }

          DiskManagement.DeleteOldRecordings();
          DiskManagement.CheckFreeDiskSpace();

          //handle the notifies
          Recorder.HandleNotifies();

          //process any recorder commands from the GUI
          lock (_listCommands)
          {
            foreach (RecorderCommand cmd in _listCommands)
            {
              switch (cmd.CommandType)
              {
                case RecorderCommandType.Paused:
                  HandlePaused();
                  break;
                case RecorderCommandType.StopAll:
                  // stop all activity on all cards (used when MP stops)
                  HandleStopAll();
                  break;
                case RecorderCommandType.StartViewing:
                  //start viewing a tv channel
                  HandleViewCommand(cmd.Channel, true, cmd.TimeShifting);
                  _epgTimer = DateTime.MinValue;
                  break;
                case RecorderCommandType.StopViewing:
                  //stop viewing a tv channel
                  HandleViewCommand(cmd.Channel, false, cmd.TimeShifting);
                  break;
                case RecorderCommandType.StopRecording:
                  // stop recording a program
                  if (cmd.Recording == null)
                    HandleStopRecording();
                  else
                    HandleStopTvRecording(cmd.Recording);
                  HandleRecordings();
                  break;
                case RecorderCommandType.StopAllViewing:
                  //stop viewing on any card
                  HandleStopAllViewing();
                  break;
                case RecorderCommandType.StopRadio:
                  //stop listening to radio
                  HandleStopRadio();
                  break;
                case RecorderCommandType.StartRadio:
                  //start listening to radio
                  HandleStartRadio(cmd.Channel);
                  break;
                case RecorderCommandType.HandleRecording:
                  HandleRecordings();
                  break;
              }
            }
            _listCommands.Clear();
          }
        }
        catch (Exception ex)
        {
          Log.WriteFile(Log.LogType.Recorder, true, "exception in process() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }
      }
    }


    /// <summary>
    /// ProcessCards()
    /// This method gets called regulary and will stop timeshifting on any card
    /// which is 
    ///     - not recording 
    ///     - timeshifting
    ///     - not being watched
    ///  Also it will start playback when the card is timeshifting but the
    ///  timeshift player has not been started yet
    /// </summary>
    static void ProcessCards()
    {
      // if a card is timeshifting, and the timeshift file exists
      // and the player is not currently playing, then start playing the timeshift file
      
      //are we timeshifting and/or recording
      if (Recorder.IsTimeShifting() || Recorder.IsRecording())
      {
        // and player is not playing?
        if (!g_Player.Playing)
        {
          //are we in My TV 
          int windowId = GUIWindowManager.ActiveWindow;
          if (GUIGraphicsContext.IsTvWindow(windowId) || Recorder.IsRadio())
          {
            //check if timeshift file exists
            string fileName = Recorder.GetTimeShiftFileName();
            try
            {
              if (System.IO.File.Exists(fileName))
              {
                using (FileStream f = new FileStream(fileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                  //yes, if filesize > 10KByte
                  if (f.Length > 1024 * 10)
                  {
                    //then we start playing the timeshift file
                    Log.Write("Recorder:filesize:{0} play timeshifting", f.Length);
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
                    msg.Label = fileName;
                    GUIGraphicsContext.SendMessage(msg);
                  }
                }
              }
            }
            catch (Exception)
            {
            }
          }
        }
      }


      //process all cards
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        dev.Process();
        //if card is timeshifting, but player has stopped, then stop the card also
        if (dev.IsTimeShifting && !dev.IsRecording && !dev.IsRadio)
        {
          if (_currentCardIndex == i)
          {
            //player not playing?
            if (!g_Player.Playing)
            {
              // for more then 10 secs?
              TimeSpan ts = DateTime.Now - _killTimeshiftingTimer;
              if (ts.TotalSeconds > 10)
              {
                //then stop the card
                dev.Stop();
                _currentCardIndex = -1;
                if (OnTvViewingStopped != null)
                  OnTvViewingStopped(i, dev);
              }
            }
            else
            {
              _killTimeshiftingTimer = DateTime.Now;
            }
          }
          else
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder:Stop card:{0}", _currentCardIndex);
            dev.Stop();
          }
        }
      }
    }//static void ProcessCards()

    /// <summary>
    /// Scheduler main loop. This function needs to get called on a regular basis.
    /// It will handle all scheduler tasks
    /// </summary>
    static public void Process()
    {
      if (_state != State.Initialized)
      {
        Recorder.Start();
        return;
      }
      if (Paused) return;
      if (GUIGraphicsContext.InVmr9Render) return;
      TimeSpan ts = DateTime.Now - _progressBarTimer;
      if (g_Player.Playing && (Math.Abs(g_Player.Duration - _duration) >= 1 || Math.Abs(g_Player.CurrentPosition - _lastPosition) >= 1))
      {
        RecorderProperties.UpdateRecordingProperties();
        _progressBarTimer = DateTime.Now;
      }
      else if (ts.TotalMilliseconds > 10000)
      {
        RecorderProperties.UpdateRecordingProperties();
        _progressBarTimer = DateTime.Now;
      }
      _duration = g_Player.Duration;
      _lastPosition = g_Player.CurrentPosition;

    }//static public void Process()



    #endregion

    #region Helper functions
    /// <summary>
    /// This function gets called by the TVDatabase when a recording has been
    /// added,changed or deleted. It forces the Scheduler to get update its list of
    /// recordings.
    /// </summary>
    static private void OnRecordingsChanged(TVDatabase.RecordingChange change)
    {
      if (_state != State.Initialized) return;
      _recordingsListChanged = true;
    }

    /// <summary>
    /// Handles incoming messages from other modules
    /// </summary>
    /// <param name="message">message received</param>
    /// <remarks>
    /// Supports the following messages:
    ///  GUI_MSG_RECORDER_ALLOC_CARD 
    ///  When received the scheduler will release/free all resources for the
    ///  card specified so other assemblies can use it
    ///  
    ///  GUI_MSG_RECORDER_FREE_CARD
    ///  When received the scheduler will alloc the resources for the
    ///  card specified. Its send when other assemblies dont need the card anymore
    ///  
    ///  GUI_MSG_RECORDER_STOP_TIMESHIFT
    ///  When received the scheduler will stop timeshifting.
    ///  
    ///  GUI_MSG_RECORDER_STOP_TV
    ///  When received the scheduler will stop viewing tv on any card.
    ///  
    ///  GUI_MSG_RECORDER_STOP_RADIO
    ///  When received the scheduler will stop listening radio on any card.
    /// </remarks>
    static public void OnMessage(GUIMessage message)
    {
      if (message == null) return;
      switch (message.Message)
      {

        case GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED:
          _progressBarTimer = DateTime.MinValue;
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TV:
          StopViewing();
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO:
          StopRadio();
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO:
          StartRadio(message.Label);
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_ALLOC_CARD:
          {
            // somebody wants to allocate a capture card
            // if possible, lets release it
            //TODO
            int i = 0;
            foreach (TVCaptureDevice card in _tvcards)
            {
              if (card.VideoDevice.Equals(message.Label))
              {
                if (!card.IsRecording)
                {
                  bool stopped = false;
                  if (IsCardViewing(card.ID))
                    stopped = true;
                  card.Stop();
                  card.Allocated = true;
                  if (stopped && OnTvViewingStopped != null)
                    OnTvViewingStopped(i, card);
                  return;
                }
              }
              ++i;
            }
          }
          break;


        case GUIMessage.MessageType.GUI_MSG_RECORDER_FREE_CARD:
          // somebody wants to allocate a capture card
          // if possible, lets release it
          foreach (TVCaptureDevice card in _tvcards)
          {
            if (card.VideoDevice.Equals(message.Label))
            {
              if (card.Allocated)
              {
                card.Allocated = false;
                return;
              }
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT:
          foreach (TVCaptureDevice card in _tvcards)
          {
            if (!card.IsRecording)
            {
              if (card.IsTimeShifting)
              {
                Log.WriteFile(Log.LogType.Recorder, "Recorder: stop timeshifting on card:{0} channel:{1}",
                                  card.ID, card.TVChannel);
                card.Stop();
              }
            }
          }
          break;
      }//switch(message.Message)
    }//static public void OnMessage(GUIMessage message)


    /// <summary>
    /// Shows in the log file which cards are in use and what they are doing
    /// Also logs which file is currently being played
    /// </summary>
    static private void LogTvStatistics()
    {
      TVCaptureDevice dev;
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        dev = _tvcards[i];
        if (!dev.IsRecording)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} viewing:{1} recording:{2} timeshifting:{3} channel:{4}",
            dev.ID, dev.View, dev.IsRecording, dev.IsTimeShifting, dev.TVChannel);
        }
        else
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  Card:{0} viewing:{1} recording:{2} timeshifting:{3} channel:{4} id:{5}",
            dev.ID, dev.View, dev.IsRecording, dev.IsTimeShifting, dev.TVChannel, dev.CurrentTVRecording.ID);
        }
      }
      if (g_Player.Playing)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  currently playing:{0}", g_Player.CurrentFile);
      }
    }


    /// <summary>
    /// This method will send a message to all 'external tuner control' plugins like USBUIRT
    /// to switch channel on the remote device
    /// </summary>
    /// <param name="strChannelName">name of channel</param>
    static void TuneExternalChannel(string strChannelName, bool isViewing)
    {
      if (strChannelName == null) return;
      if (strChannelName == String.Empty) return;
      foreach (TVChannel chan in _tvChannelsList)
      {
        if (chan.Name.Equals(strChannelName))
        {
          if (chan.External)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL, 0, 0, 0, 0, 0, null);
            msg.Label = chan.ExternalTunerChannel;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;
        }
      }
      if (isViewing)
        SetZapOSDData(strChannelName);
    }//static void TuneExternalChannel(string strChannelName)

    #endregion


    #region audiostream selection
    static public int GetAudioLanguage()
    {
      if (_state != State.Initialized) return -1;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return -1;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];

      return dev.GetAudioLanguage();
    }

    static public void SetAudioLanguage(int audioPid)
    {
      if (_state != State.Initialized) return;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];

      dev.SetAudioLanguage(audioPid);
    }

    static public ArrayList GetAudioLanguageList()
    {
      if (_state != State.Initialized) return null;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return null;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];

      return dev.GetAudioLanguageList();
    }
    #endregion

    private static void card_OnTvRecordingEnded(string recordingFileName, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder: recording ended '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6}", recording.Title, recording.Channel, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString(), recording.ID, recording.Priority, recording.Quality.ToString());
      if (OnTvRecordingEnded != null)
        OnTvRecordingEnded(recordingFileName, recording, program);
      if (OnTvRecordingChanged != null)
        OnTvRecordingChanged();
    }

    private static void card_OnTvRecordingStarted(string recordingFileName, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder: recording started '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6}", recording.Title, recording.Channel, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString(), recording.ID, recording.Priority, recording.Quality.ToString());
      if (OnTvRecordingStarted != null)
        OnTvRecordingStarted(recordingFileName, recording, program);
      if (OnTvRecordingChanged != null)
        OnTvRecordingChanged();
    }

    #region notification handling
    private static void OnNotifiesChanged()
    {
      _notifiesListChanged = true;
    }

    static void HandleNotifies()
    {
      if (_notifiesListChanged)
      {
        _notifiesList.Clear();
        TVDatabase.GetNotifies(_notifiesList, true);
        _notifiesListChanged = false;
      }
      DateTime dt5Mins = DateTime.Now.AddMinutes(5);
      for (int i = 0; i < _notifiesList.Count; ++i)
      {
        TVNotify notify = _notifiesList[i];
        if (dt5Mins > notify.Program.StartTime)
        {
          TVDatabase.DeleteNotify(notify);
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
          msg.Object = notify.Program;
          GUIGraphicsContext.SendMessage(msg);
          msg = null;
        }
      }
    }
    #endregion
  }//public class Recorder
}//namespace MediaPortal.TV.Recording
