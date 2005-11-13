/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
    enum RecorderCommandType
    {
      StopAll,        // stop all activity on all cards
      StopAllViewing, // stop any card which is currently viewing
      StopViewing,    
      StartViewing,
      StartRadio,
      StopRadio,
      StopRecording,
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
      None,
      Initializing,
      Initialized,
      Deinitializing
    }

    static bool _recordingsListChanged = false;  // flag indicating that recordings have been added/changed/removed
    static bool _notifiesListChanged = false;  // flag indicating that notifies have been added/changed/removed
    static int _preRecordInterval = 0;
    static int _postRecordInterval = 0;

    static string _tvChannel = String.Empty;

    static State _state = State.None;
    static List<TVCaptureDevice> _tvcards = new List<TVCaptureDevice>();
    static List<TVChannel> _tvChannelsList = new List<TVChannel>();
    static List<TVRecording> _recordingsList = new List<TVRecording>();
    static List<TVNotify> _notifiesList = new List<TVNotify>();

    static DateTime _startTimer = DateTime.Now;
    static DateTime _progressBarTimer = DateTime.Now;
    static int _currentCardIndex = -1;
    static int _preRecordingWarningTime = 2;
    static VMR9OSD _vmr9Osd = new VMR9OSD();
    static bool _useVmr9Zap = false;
    static double _duration = 0;
    static double _lastPosition = 0;
    static DateTime _killTimeshiftingTimer;
    static List<RecorderCommand> _listCommands = new List<RecorderCommand>();
    static BackgroundWorker _epgWorker;
    #endregion

    #region delegates and events
    public delegate void OnTvViewHandler(int card, TVCaptureDevice device);
    public delegate void OnTvChannelChangeHandler(string tvChannelName);
    public delegate void OnTvRecordingChangedHandler();
    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);
    static public event OnTvChannelChangeHandler OnTvChannelChanged = null;
    static public event OnTvRecordingChangedHandler OnTvRecordingChanged = null;
    static public event OnTvRecordingHandler OnTvRecordingStarted = null;
    static public event OnTvRecordingHandler OnTvRecordingEnded = null;
    static public event OnTvViewHandler OnTvViewingStarted = null;
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
    /// This method will Start the scheduler. It
    /// Loads the capture cards from capturecards.xml (made by the setup)
    /// Loads the recordings (programs scheduled to record) from the tvdatabase
    /// Loads the TVchannels from the tvdatabase
    /// </summary>
    static public void Start()
    {
      if (_state != State.None) return;
      _state = State.Initializing;
      RecorderProperties.Init();
      _recordingsListChanged = false;

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
      for (int i = 0; i < _tvcards.Count; i++)
      {
        TVCaptureDevice card = _tvcards[i];
        card.ID = (i + 1);
        card.OnTvRecordingEnded += new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingEnded);
        card.OnTvRecordingStarted += new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingStarted);
        Log.WriteFile(Log.LogType.Recorder, "Recorder:    card:{0} video device:{1} TV:{2}  record:{3} priority:{4}",
                              card.ID, card.VideoDevice, card.UseForTV, card.UseForRecording, card.Priority);
      }

      _preRecordInterval = 0;
      _postRecordInterval = 0;
      //m_bAlwaysTimeshift=false;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        _preRecordInterval = xmlreader.GetValueAsInt("capture", "prerecord", 5);
        _postRecordInterval = xmlreader.GetValueAsInt("capture", "postrecord", 5);
        //m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
        TVChannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
        _useVmr9Zap = xmlreader.GetValueAsBool("general", "useVMR9ZapOSD", false);
        _preRecordingWarningTime = xmlreader.GetValueAsInt("mytv", "recordwarningtime", 2);
      }

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

      DiskManagement.ImportDvrMsFiles();

      _tvChannelsList.Clear();
      TVDatabase.GetChannels(ref _tvChannelsList);

      _recordingsList.Clear();
      _notifiesList.Clear();
      TVDatabase.GetRecordings(ref _recordingsList);
      TVDatabase.GetNotifies(_notifiesList, true);

      TVDatabase.OnRecordingsChanged += new TVDatabase.OnRecordingChangedHandler(Recorder.OnRecordingsChanged);
      TVDatabase.OnNotifiesChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(Recorder.OnNotifiesChanged);

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

      _epgWorker = new BackgroundWorker();
      _epgWorker.DoWork += new DoWorkEventHandler(Recorder.ProcessThread);
      _epgWorker.RunWorkerAsync();
      GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);

    }

    
    static void GUIWindowManager_OnActivateWindow(int windowId)
    {
      if (g_Player.Playing) return;
      if (GUIGraphicsContext.IsTvWindow(windowId))
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      else
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }//static public void Start()

    /// <summary>
    /// Stops the scheduler. It will cleanup all resources allocated and free
    /// the capture cards
    /// </summary>
    static public void Stop()
    {
      //todo
      if (_state != State.Initialized) return;
      TVDatabase.OnRecordingsChanged -= new TVDatabase.OnRecordingChangedHandler(Recorder.OnRecordingsChanged);
      GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);
      RecorderProperties.Clean();
      _recordingsListChanged = false;

      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopAll);
        _listCommands.Add(cmd);
      }
      GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);

      while (_state != State.None) System.Threading.Thread.Sleep(100);
    }//static public void Stop()

    #endregion

    #region recording
    static void HandleStopAll()
    {
      foreach (TVCaptureDevice card in _tvcards)
      {
        card.Stop();
      }
      _state = State.None;

    }
    /// <summary>
    /// Checks if a recording should be started and if so starts the recording
    /// This function gets called on a regular basis by the scheduler. It will
    /// look if any of the recordings needs to be started. Ifso it will
    /// find a free tvcapture card and start the recording
    /// </summary>
    static void HandleRecordings()
    {
      if (_state != State.Initialized) return;

      DateTime dtCurrentTime = DateTime.Now;
      // no TV cards? then we cannot record anything, so just return
      if (_tvcards.Count == 0) return;

      // If the recording schedules have been changed since last time
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
          if (rec.Canceled > 0) continue;
          if (rec.IsDone()) continue;
          if (rec.RecType == TVRecording.RecordingType.EveryTimeOnEveryChannel || chan.Name == rec.Channel)
          {
            if (!IsRecordingSchedule(rec, out card))
            {
              int paddingFront = rec.PaddingFront;
              int paddingEnd = rec.PaddingEnd;
              if (paddingFront < 0) paddingFront = _preRecordInterval;
              if (paddingEnd < 0) paddingEnd = _postRecordInterval;

              // check which program is running 
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
                if (!rec.IsAnnouncementSend)
                {
                  DateTime dtTime = DateTime.Now.AddMinutes(_preRecordingWarningTime);
                  TVProgram prog2Min = chan.GetProgramAt(dtTime.AddMinutes(1 + paddingFront));

                  // if the recording should record the tv program
                  if (rec.IsRecordingProgramAtTime(dtTime, prog2Min, paddingFront, paddingEnd))
                  {
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


      for (int j = 0; j < _recordingsList.Count; ++j)
      {
        TVRecording rec = _recordingsList[j];
        if (rec.Canceled > 0) continue;
        if (rec.IsDone()) continue;

        int paddingFront = rec.PaddingFront;
        int paddingEnd = rec.PaddingEnd;
        if (paddingFront < 0) paddingFront = _preRecordInterval;
        if (paddingEnd < 0) paddingEnd = _postRecordInterval;

        // 1st check if the recording itself should b recorded
        if (rec.IsRecordingProgramAtTime(DateTime.Now, null, paddingFront, paddingEnd))
        {
          if (!IsRecordingSchedule(rec, out card))
          {
            // yes, then record it
            if (Record(dtCurrentTime, rec, null, paddingFront, paddingEnd))
            {
              // recording it
            }
          }
        }
        else
        {
          if (!rec.IsAnnouncementSend)
          {
            DateTime dtTime = DateTime.Now.AddMinutes(_preRecordingWarningTime);
            // if the recording should record the tv program
            if (rec.IsRecordingProgramAtTime(dtTime, null, paddingFront, paddingEnd))
            {
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
    /// This method determines if we need to switch to another tv channel if 
    /// we want to record the TVRecording specified in rec
    /// </summary>
    /// <param name="rec">TVRecording to record</param>
    /// <returns>
    /// true : we need to switch channels
    /// false: we dont need to switch channels
    /// </returns>
    static public bool NeedChannelSwitchForRecording(TVRecording rec)
    {
      if (IsViewing() && TVChannelName == rec.Channel) return false;

      //check if there's another card which is free
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i] as TVCaptureDevice;
        if (!dev.IsRecording && !dev.IsTimeShifting && !dev.View)
        {
          if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || _tvcards.Count == 1)
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Starts recording the specified tv channel immediately using a reference recording
    /// When called this method starts an erference  recording on the channel specified
    /// It will record the next 2 hours.
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
        if (chan.Name.Equals(strChannel))
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
        tmpRec.End = Utils.datetolong(DateTime.Now.AddMinutes(4 * 60));
        tmpRec.Title = GUILocalizeStrings.Get(413);
        if (program != null)
          tmpRec.Title = program.Title;
        tmpRec.IsContentRecording = true;//make a content recording! (record from now)
      }

      Log.WriteFile(Log.LogType.Recorder, "Recorder:   start: {0} {1}", tmpRec.StartTime.ToShortDateString(), tmpRec.StartTime.ToShortTimeString());
      Log.WriteFile(Log.LogType.Recorder, "Recorder:   end  : {0} {1}", tmpRec.EndTime.ToShortDateString(), tmpRec.EndTime.ToShortTimeString());

      AddRecording(ref tmpRec);
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
      HandleRecordings();
    }//static public void RecordNow(string strChannel)

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
        if (dev.View && !dev.IsRecording)
        {
          if (dev.TVChannel == recordingChannel)
          {
            if (dev.UseForRecording) return _currentCardIndex;
          }
        }
      }

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
          if (dev.CurrentTVRecording.ID == rec.ID) return false;
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
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  No card found, check if a card is recording a show which has a lower priority then priority:{0}", rec.Priority);
        cardNo = FindFreeCardForRecording(rec.Channel, true, rec.Priority);
        if (cardNo < 0)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  no recordings have a lower priority then priority:{0}", rec.Priority);
          return false;
        }
      }

      if (cardNo < 0)
      {
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
      TVCaptureDevice card = _tvcards[cardNo];
      Log.WriteFile(Log.LogType.Recorder, "Recorder:  using card:{0} prio:{1}", card.ID, card.Priority);
      if (card.IsRecording)
      {
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
        TVDatabase.UpdateRecording(card.CurrentTVRecording, TVDatabase.RecordingChange.Canceled);
        card.StopRecording();
      }

      TuneExternalChannel(rec.Channel, false);
      card.Record(rec, currentProgram, iPostRecordInterval, iPostRecordInterval);

      if (_currentCardIndex == cardNo)
      {
        TVChannelName = rec.Channel;
        HandleViewCommand(rec.Channel, true, true);
      }
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
      return true;
    }//static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)


    static public void StopRecording(TVRecording rec)
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRecording({0})", rec.Title);
      if (_state != State.Initialized) return;
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopRecording, rec);
        _listCommands.Add(cmd);
      }
    }
    static void HandleStopTvRecording(TVRecording rec)
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:HandleStopTvRecording()");
      if (rec == null) return;
      for (int card = 0; card < _tvcards.Count; ++card)
      {
        TVCaptureDevice dev = _tvcards[card];
        if (dev.IsRecording)
        {
          if (dev.CurrentTVRecording.ID == rec.ID)
          {
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
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
    }//StopRecording

    /// <summary>
    /// Stops all recording on the current channel. 
    /// </summary>
    /// <remarks>
    /// Only stops recording. timeshifting wont be stopped so user can continue to watch the channel
    /// </remarks>
    static public void StopRecording()
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRecording()");
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopRecording);
        _listCommands.Add(cmd);
      }
    }
    static void HandleStopRecording()
    {
      if (_state != State.Initialized) return;
      Log.WriteFile(Log.LogType.Recorder, "Recorder:HandleStopRecording()");

      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];

      if (dev.IsRecording)
      {
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
        dev.StopRecording();
        _recordingsListChanged = true;
      }
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
    }//static public void StopRecording()

    #endregion

    #region Properties
    /// <summary>
    /// Property which returns if any card is recording
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
    /// Property which returns if any card is recording the specified channel
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
    /// Property which returns if current card is recording
    /// </summary>
    static public bool IsRecording()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;

      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.IsRecording;
    }//static public bool IsRecording()

    /// <summary>
    /// Property which returns if current channel has teletext or not
    /// </summary>
    static public bool HasTeletext()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.HasTeletext;
    }

    /// <summary>
    /// Property which returns if current card supports timeshifting
    /// </summary>
    static public bool DoesSupportTimeshifting()
    {
      if (_state != State.Initialized) return false;
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return false;

      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      return dev.SupportsTimeShifting;
    }//static public bool DoesSupportTimeshifting()

    static public string GetFriendlyNameForCard(int card)
    {
      if (_state != State.Initialized) return String.Empty;
      if (card < 0 || card >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[card];
      return dev.FriendlyName;
    }//static public string GetFriendlyNameForCard(int card)

    /// <summary>
    /// Returns the Channel name of the channel we're currently watching
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
    /// Checks if a tvcapture card is recording the TVRecording specified
    /// </summary>
    /// <param name="rec">TVRecording <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <returns>true if a card is recording the specified TVRecording, else false</returns>
    static public bool IsRecordingSchedule(TVRecording rec, out int card)
    {
      card = -1;
      if (rec == null) return false;
      if (_state != State.Initialized) return false;
      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        if (dev.IsRecording && dev.CurrentTVRecording != null && dev.CurrentTVRecording.ID == rec.ID)
        {
          if (rec.Series == false)
          {
            card = i;
            return true;
          }

          //check start/end times
          if (rec.StartTime <= DateTime.Now && rec.EndTime >= rec.StartTime)
          {
            card = i;
            return true;
          }
        }
      }
      return false;
    }//static public bool IsRecordingSchedule(TVRecording rec, out int card)

    /// <summary>
    /// Property which returns the current program being recorded. If no programs are being recorded at the moment
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
        if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording) return true;
        for (int i = 0; i < _tvcards.Count; ++i)
        {
          TVCaptureDevice dev = _tvcards[i];
          if (dev.View) return true;
        }
        return false;
      }
    }//static public bool View

    /// <summary>
    /// property which returns the date&time the recording was started
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

    static public string GetRecordingFileName(TVRecording rec)
    {
      int card;
      if (!IsRecordingSchedule(rec, out card)) return String.Empty;
      TVCaptureDevice dev = _tvcards[card] as TVCaptureDevice;

      return dev.RecordingFileName;
    }

    /// <summary>
    /// Property which returns the timeshifting file for the current channel
    /// </summary>
    static public string GetTimeShiftFileName()
    {
      if (_currentCardIndex < 0 || _currentCardIndex >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[_currentCardIndex];
      if (!dev.IsTimeShifting) return String.Empty;

      string FileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, _currentCardIndex + 1, dev.TimeShiftFileName);
      return FileName;
    }

    static public string GetTimeShiftFileName(int card)
    {
      if (card < 0 || card >= _tvcards.Count) return String.Empty;
      TVCaptureDevice dev = _tvcards[card];
      string FileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, card + 1, dev.TimeShiftFileName);
      return FileName;
    }
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

    static public void StopRadio()
    {
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopRadio);
        _listCommands.Add(cmd);
      }
    }
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
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
    }//stopRadio()

    static public void StartRadio(string radioStationName)
    {
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StartRadio);
        cmd.Channel = radioStationName;
        _listCommands.Add(cmd);
      }
    }
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
            _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0); ;
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
    static public void StopViewing()
    {
      Log.WriteFile(Log.LogType.Recorder, "Recorder:Stopviewing()");
      lock (_listCommands)
      {
        RecorderCommand cmd = new RecorderCommand(RecorderCommandType.StopAllViewing);
        _listCommands.Add(cmd);
      }
      while (_listCommands.Count > 0) System.Threading.Thread.Sleep(10);
    }
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
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
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
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
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
    static public void StartViewing(string channel, bool TVOnOff, bool timeshift)
    {
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
          _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
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
          _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
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
          _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
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
      _startTimer = new DateTime(1971, 6, 11, 0, 0, 0, 0);
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
      if (ts.TotalMilliseconds < 1000) return;

      foreach (TVCaptureDevice card in _tvcards)
      {
        //card is empty
        if (card.Network == NetworkType.Analog) continue;
        if (card.IsEpgGrabbing)
        {
          card.Process();
          continue;
        }
        if (card.IsRadio || card.IsRecording || card.IsTimeShifting || card.View) continue;
        if (!card.IsEpgGrabbing)
        {
          foreach (TVChannel chan in _tvChannelsList)
          {
            if (_listCommands.Count > 0) break;
            if (!chan.AutoGrabEpg) continue;
            ts = DateTime.Now - chan.LastDateTimeEpgGrabbed;
            if (ts.TotalHours > 2)
            {
              if (TVDatabase.CanCardViewTVChannel(chan.Name, card.ID) || _tvcards.Count == 1)
              {
                TVProgram prog = TVDatabase.GetLastProgramForChannel(chan);
                if (prog.EndTime < DateTime.Now.AddHours(12))
                {
                  //grab the epg
                  card.GrabEpg(chan);
                  chan.LastDateTimeEpgGrabbed = DateTime.Now;
                  break;
                }
              }
            }
          }
        }
      }
      _epgTimer = DateTime.Now;
    }
    #endregion

    static void ProcessThread(object sender, DoWorkEventArgs e)
    {
      System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
      while (_state == State.Initialized)
      {
        try
        {
          System.Threading.Thread.Sleep(500);
          ProcessEpg();
          ProcessCards();
          Recorder.HandleRecordings();
          DiskManagement.CheckRecordingDiskSpace();
          Recorder.HandleNotifies();
          DiskManagement.Process();
          lock (_listCommands)
          {
            foreach (RecorderCommand cmd in _listCommands)
            {
              switch (cmd.CommandType)
              {
                case RecorderCommandType.StopAll:
                  HandleStopAll();
                  break;
                case RecorderCommandType.StartViewing:
                  HandleViewCommand(cmd.Channel, true, cmd.TimeShifting);
                  break;
                case RecorderCommandType.StopViewing:
                  HandleViewCommand(cmd.Channel, false, cmd.TimeShifting);
                  break;
                case RecorderCommandType.StopRecording:
                  if (cmd.Recording == null)
                    HandleStopRecording();
                  else
                    HandleStopTvRecording(cmd.Recording);
                  break;
                case RecorderCommandType.StopAllViewing:
                  HandleStopAllViewing();
                  break;
                case RecorderCommandType.StopRadio:
                  HandleStopRadio();
                  break;
                case RecorderCommandType.StartRadio:
                  HandleStartRadio(cmd.Channel);
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
    /// This method gets called regulary and will terminate all cards
    /// which are 
    /// - not recording 
    /// - timeshifting
    /// - not being watched
    /// </summary>
    static void ProcessCards()
    {
      //if we're not playing the timeshifting file, then start playing it...
      if (Recorder.IsTimeShifting() || Recorder.IsRecording())
      {
        if (!g_Player.Playing)
        {
          int windowId = GUIWindowManager.ActiveWindow;
          if (GUIGraphicsContext.IsTvWindow(windowId) || Recorder.IsRadio())
          {

            //then try to start it
            string fileName = Recorder.GetTimeShiftFileName();
            try
            {
              if (System.IO.File.Exists(fileName))
              {
                using (FileStream f = new FileStream(fileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                  if (f.Length > 1024 * 10)
                  {
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


      for (int i = 0; i < _tvcards.Count; ++i)
      {
        TVCaptureDevice dev = _tvcards[i];
        dev.Process();
        if (dev.IsTimeShifting && !dev.IsRecording && !dev.IsRadio)
        {
          if (_currentCardIndex == i)
          {
            if (!g_Player.Playing)
            {
              TimeSpan ts = DateTime.Now - _killTimeshiftingTimer;
              if (ts.TotalSeconds > 10)
              {
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
      _startTimer = new DateTime(1971, 11, 6, 20, 0, 0, 0);
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
