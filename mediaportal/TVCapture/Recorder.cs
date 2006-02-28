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


namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// This class is a singleton which implements the
  /// -task scheduler to schedule, (start,stop) all tv recordings on time
  /// -a front end to other classes to control the tv capture cardsd
  /// </summary>
  public class Recorder
  {

    #region enum
    enum State
    {
      None,           //recorder is not initialized yet
      Initializing,   //recorder is busy initializing
      Initialized,    //recorder is initialized 
      Deinitializing  //recorder is de-initializing
    }
    #endregion

    #region variables
    // recorder state
    static State _state = State.None; 
    static DateTime _progressBarTimer = DateTime.Now;
    // vmr9 osd class 
    static VMR9OSD _vmr9Osd = new VMR9OSD();
    //static bool _useVmr9Zap = false;
    // last duration of timeshifting buffer
    static double _duration = 0;
    // last position in timeshifting buffer
    static double _lastPosition = 0;
    static CommandProcessor _commandProcessor;
    #endregion

    #region events
    public delegate void OnTvViewHandler(int card, TVCaptureDevice device);
    public delegate void OnTvChannelChangeHandler(string tvChannelName);

    public delegate void OnTvRecordingChangedHandler();
    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);

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
    static public event OnTvChannelChangeHandler OnTvChannelChanged = null;
    #endregion

    #region ctor
    /// <summary>
    /// singleton. Dont allow any instance of this class so make the constructor private
    /// </summary>
    private Recorder()
    {
    }

    static Recorder()
    {
    }
    #endregion

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
      CommandProcessor processor = new CommandProcessor();
      processor.Start();
      Start(processor);
    }

    static public void Start(CommandProcessor processor)
    {
      if (_state != State.None) return;//if we are initialized already then no need todo anything
      _state = State.Initializing;
      _commandProcessor = processor;
      RecorderProperties.Init();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _commandProcessor.TVChannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      }

      //import any recordings which are on disk, but not in the tv database
      RecordingImporterWorker.ImportDvrMsFiles();

      //subscribe to window messages.

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
        _commandProcessor.Paused= true;
      }


      //subscribe to window change notifications
      GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
      GUIWindowManager.Receivers += new SendMessageHandler(Recorder.OnMessage);
      _state = State.Initialized;

      _commandProcessor.OnTvChannelChanged += new MultiCardBase.OnTvChannelChangeHandler(_commandProcessor_OnTvChannelChanged);
      _commandProcessor.OnTvViewingStarted += new MultiCardBase.OnTvViewHandler(_commandProcessor_OnTvViewingStarted);
      _commandProcessor.OnTvViewingStopped += new MultiCardBase.OnTvViewHandler(_commandProcessor_OnTvViewingStopped);

      _commandProcessor.TVCards.OnTvRecordingChanged += new TvCardCollection.OnTvRecordingChangedHandler(TVCards_OnTvRecordingChanged);
      _commandProcessor.TVCards.OnTvRecordingEnded += new TvCardCollection.OnTvRecordingHandler(TVCards_OnTvRecordingEnded);
      _commandProcessor.TVCards.OnTvRecordingStarted += new TvCardCollection.OnTvRecordingHandler(TVCards_OnTvRecordingStarted);
    }

    #region recording event callbacks
    static void TVCards_OnTvRecordingStarted(string recordingFilename, TVRecording recording, TVProgram program)
    {
      if (OnTvRecordingStarted != null)
        OnTvRecordingStarted(recordingFilename, recording, program);
    }

    static void TVCards_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      if (OnTvRecordingEnded != null)
        OnTvRecordingEnded(recordingFilename, recording, program);
    }

    static void TVCards_OnTvRecordingChanged()
    {
      if (OnTvRecordingChanged != null)
        OnTvRecordingChanged();
    }
    #endregion

    /// <summary>
    /// Stops the scheduler. It will cleanup all resources allocated and free
    /// the capture cards
    /// </summary>
    static public void Stop()
    {

      if (_state != State.Initialized) return;
      //unsubscribe from events
      GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
      GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);
      RecorderProperties.Clean();


      //wait until the process thread has stopped.
      _state = State.None;
      _commandProcessor.Dispose();
      _commandProcessor = null;
    }//static public void Stop()

    #region tv viewing event callbacks
    static void _commandProcessor_OnTvViewingStopped(int card, TVCaptureDevice device)
    {
      if (OnTvViewingStopped != null)
        OnTvViewingStopped(card, device);
    }

    static void _commandProcessor_OnTvViewingStarted(int card, TVCaptureDevice device)
    {
      if (OnTvViewingStarted != null)
        OnTvViewingStarted(card, device);
    }

    static void _commandProcessor_OnTvChannelChanged(string tvChannelName)
    {
      if (OnTvChannelChanged != null)
        OnTvChannelChanged(tvChannelName);
    }
    #endregion

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
        Log.WriteFile(Log.LogType.Recorder,"Recorder:enable dx9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      else
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder:disable dx9 exclusive mode");
        // we leave my tv, disable exclusive mode
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }//static public void GUIWindowManager_OnActivateWindow()

    

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
      if (_commandProcessor==null) return false;
      //are we viewing the channel requested, then there is no need to switch
      if (IsViewing() && _commandProcessor.TVChannelName == rec.Channel) return false;

      //no, check if there's another card which is free. ifso we can use that one and
      //there is no need to switch channels
      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i] as TVCaptureDevice;
        //is the card free?
        if (!dev.IsRecording && !dev.IsTimeShifting && !dev.View)
        {
          //yes and can it receive the tv channel as well?
          if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || _commandProcessor.TVCards.Count == 1)
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
    /// <param name="channelName">TVchannel to record</param>
    static public void RecordNow(string channelName, bool manualStop)
    {
      if (channelName == null) return;
      if (channelName == String.Empty) return;
      if (_state != State.Initialized) return;

      // create a new recording which records the next 2 hours...
      TVRecording tmpRec = new TVRecording();

      tmpRec.Channel = channelName;
      tmpRec.RecType = TVRecording.RecordingType.Once;

      List<TVChannel> tvChannelsList = new List<TVChannel>();
      TVDatabase.GetChannels(ref tvChannelsList);
      TVProgram program = null;
      for (int i = 0; i < tvChannelsList.Count; ++i)
      {
        TVChannel chan = tvChannelsList[i];
        if (String.Compare(chan.Name,channelName,false)==0)
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
        Log.WriteFile(Log.LogType.Recorder, "Recorder:record now:{0} program:{1}", channelName, program.Title);
      }
      else
      {
        //no tvguide data, just record the next 2 hours
        Log.WriteFile(Log.LogType.Recorder, "Recorder:record now:{0} for next 4 hours", channelName);
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
      CheckRecordingsCommand cmd = new CheckRecordingsCommand();
      _commandProcessor.AddCommand(cmd);
    }//static public void RecordNow(string channelName)

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
    /// Tell the recorder to stop recording the schedule specified in rec
    /// </summary>
    /// <param name="rec">recording to stop</param>
    static public void StopRecording(TVRecording rec)
    {
      if (_state != State.Initialized) return;
      //add a new RecorderCommand which holds 'rec'
      //and tell the process thread to handle it
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRecording({0})", rec.Title);
      CancelRecordingCommand cmd = new CancelRecordingCommand(rec);
      _commandProcessor.AddCommand(cmd);
    }
    /// <summary>
    /// Stops all recording on the current channel. 
    /// </summary>
    /// <remarks>
    /// Only stops recording. timeshifting wont be stopped so user can continue to watch the channel
    /// </remarks>
    static public void StopRecording()
    {
      if (_state != State.Initialized) return;
      //add a new RecorderCommand to tell the process thread
      //that it must stop recording
      Log.WriteFile(Log.LogType.Recorder, "Recorder:StopRecording()");
      StopRecordingCommand cmd = new StopRecordingCommand();
      _commandProcessor.AddCommand(cmd);
    }

    /// <summary>
    /// Property which returns if any tvcard is recording
    /// </summary>
    static public bool IsAnyCardRecording()
    { 
      if (_state != State.Initialized) return false;

      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
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

      for (int i=0; i < _commandProcessor.TVCards.Count;++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
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
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;

      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      return dev.IsRecording;
    }//static public bool IsRecording()

    /// <summary>
    /// Property which returns if current tv channel has teletext or not
    /// </summary>
    static public bool HasTeletext()
    {
      if (_state != State.Initialized) return false;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      return dev.HasTeletext;
    }

    /// <summary>
    /// Property which returns if current selected card supports timeshifting
    /// </summary>
    static public bool DoesSupportTimeshifting()
    {
      if (_state != State.Initialized) return false;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;

      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
      if (card < 0 || card >= _commandProcessor.TVCards.Count) return String.Empty;
      TVCaptureDevice dev = _commandProcessor.TVCards[card];
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
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return String.Empty;
      return _commandProcessor.TVChannelName;
    }//static public string GetTVChannelName()

    /// <summary>
    /// Returns the TV Recording we're currently recording
    /// </summary>
    /// <returns>
    /// </returns>
    static public TVRecording GetTVRecording()
    {
      if (_state != State.Initialized) return null;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;

      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      if (dev.IsRecording) return dev.CurrentTVRecording;
      return null;
    }//static public TVRecording GetTVRecording()


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
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;
        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;
        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      if (dev.View) return true;
      if (dev.IsTimeShifting)
      {
        string fileName = _commandProcessor.GetTimeShiftFileName(_commandProcessor.CurrentCardIndex);
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
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      if (dev.ID != cardId) return false;
      if (dev.View) return true;
      if (dev.IsTimeShifting)
      {
        if (g_Player.Playing && g_Player.CurrentFile == _commandProcessor.GetTimeShiftFileName(_commandProcessor.CurrentCardIndex))
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
        for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
        {
          TVCaptureDevice dev = _commandProcessor.TVCards[i];
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
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return DateTime.Now;
        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return DateTime.Now;
        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
      get { return _commandProcessor.TVCards.Count; }
    }

    /// <summary>
    /// Indexer which returns the TVCaptureDevice object for a given card
    /// </summary>
    /// <param name="index">card number (0-Count)</param>
    /// <returns>TVCaptureDevice object</returns>
    static public TVCaptureDevice Get(int index)
    {
      if (index < 0 || index >= _commandProcessor.TVCards.Count) return null;
      return _commandProcessor.TVCards[index] as TVCaptureDevice;
    }
    /// <summary>
    /// Property which returns the Signal Strength of the current tv card used
    /// </summary>
    static public int SignalStrength
    {
      get
      {
        if (_state != State.Initialized) return 0;
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return 0;

        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return 0;

        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
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
      if (!_commandProcessor.IsRecordingSchedule(rec, out card)) return String.Empty;
      TVCaptureDevice dev = _commandProcessor.TVCards[card] as TVCaptureDevice;

      return dev.RecordingFileName;
    }


    /// <summary>
    /// Property which returns the timeshifting filename for a specific card
    /// </summary>
    /// <param name="card">card index</param>
    /// <returns>filename of the timeshifting file</returns>
    static public string GetTimeShiftFileName(int card)
    {
      if (card < 0 || card >= _commandProcessor.TVCards.Count) return String.Empty;
      TVCaptureDevice dev = _commandProcessor.TVCards[card];
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
      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
        if (dev.ID == cardId)
        {
          string FileName = String.Format(@"{0}\card{1}\{2}", dev.RecordingPath, i + 1, dev.TimeShiftFileName);
          return FileName;
        }
      }
      return String.Empty;
    }



    
    /// <summary>
    /// Property which returns true if we currently listening to a radio station
    /// </summary>
    /// <returns>true if listening to radio</returns>
    static public bool IsRadio()
    {
      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
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
      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
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

      StopRadioCommand cmd = new StopRadioCommand();
      _commandProcessor.AddCommand(cmd);
    }

    /// <summary>
    /// Start listening to a radio channel
    /// </summary>
    /// <param name="radioStationName"></param>
    static public void StartRadio(string radioStationName)
    {
      //Send command to process thread to start listening to radio
      StartRadioCommand cmd = new StartRadioCommand(radioStationName);
      _commandProcessor.AddCommand(cmd);
    }

    
    /// <summary>
    /// Stop viewing on all cards
    /// </summary>
    static bool reEntrantStopViewing = false;
    static public void StopViewing()
    {
      if (reEntrantStopViewing) return;
      try
      {
        // Return if there is no _commandProcess; this should only happen if
        // Stop() has been called
        if (_commandProcessor == null) return;

        if (_commandProcessor.Paused) return;
        reEntrantStopViewing = true;

        //send Recorder command to process thread to stop viewing
        StopTvCommand cmd = new StopTvCommand();
        _commandProcessor.AddCommand(cmd);

        //wait till thread finished this command
        _commandProcessor.WaitTillFinished();
      }
      finally
      {
        reEntrantStopViewing = false;
      }
    }
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
    static public bool StartViewing(string channel, bool TVOnOff, bool timeshift, bool wait, out string errorMessage)
    {
      errorMessage = String.Empty;
      if (reEntrantStartViewing)
      {
        errorMessage = GUILocalizeStrings.Get(763);// "Recorder is busy";
        Log.WriteFile(Log.LogType.Recorder, true, "Recorder:StartViewing() reentrant");
        return false;
      }
      try
      {
        if (_commandProcessor.Paused)
        {
          errorMessage = GUILocalizeStrings.Get(764);//"Recorder is paused";
          return false;
        }
        if (_commandProcessor.IsBusy)
        {
          errorMessage = GUILocalizeStrings.Get(763);//"Recorder is busy";
          return false;
        }
        reEntrantStartViewing = true;
        CardCommand cmd;
        if (TVOnOff)
        {
          if (timeshift)
          {
            cmd = new TimeShiftTvCommand(channel);
            _commandProcessor.AddCommand(cmd);
          }
          else
          {
            cmd = new ViewTvCommand(channel);
            _commandProcessor.AddCommand(cmd);
          }
        }
        else
        {
          cmd = new StopTvCommand();
          _commandProcessor.AddCommand(cmd);
        }
        //wait till thread finished this command
        if (wait)
        {
          _commandProcessor.WaitTillFinished();
          if (cmd.Succeeded) return true;
          errorMessage = cmd.ErrorMessage;
          return false;
        }
      }
      finally
      {
        reEntrantStartViewing = false;
      }
      return true;
    }

    
    static public bool IsBusyProcessingCommands
    {
      get
      {
        if (_commandProcessor == null) return false;
        return (_commandProcessor.IsBusy );
      }
    }


    
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
      if (_commandProcessor.Paused) return;
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
/*
        case GUIMessage.MessageType.GUI_MSG_RECORDER_ALLOC_CARD:
          {
            // somebody wants to allocate a capture card
            // if possible, lets release it
            //TODO
            int i = 0;
            foreach (TVCaptureDevice card in _commandProcessor.TVCards)
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
          foreach (TVCaptureDevice card in _commandProcessor.TVCards)
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
*/
        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT:
          {
            if (!IsTimeShifting()) return;
            if (_commandProcessor == null) return;
            StopTimeShiftingCommand cmd = new StopTimeShiftingCommand();
            _commandProcessor.AddCommand(cmd);
            _commandProcessor.WaitTillFinished();
          }

          break;
      }//switch(message.Message)
    }//static public void OnMessage(GUIMessage message)


    
    static public int GetAudioLanguage()
    {
      if (_state != State.Initialized) return -1;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return -1;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];

      return dev.GetAudioLanguage();
    }

    static public void SetAudioLanguage(int audioPid)
    {
      if (_state != State.Initialized) return;
      SetAudioLanguageCommand cmd = new SetAudioLanguageCommand(audioPid);
      _commandProcessor.AddCommand(cmd);
    }

    static public ArrayList GetAudioLanguageList()
    {
      if (_state != State.Initialized) return null;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];

      return dev.GetAudioLanguageList();
    }
    
    static public void DeleteRecording(TVRecorded rec)
    {
      Utils.DeleteRecording(rec.FileName);
      TVDatabase.RemoveRecordedTV(rec);
      VideoDatabase.DeleteMovie(rec.FileName);
      VideoDatabase.DeleteMovieInfo(rec.FileName);
    }

    static public bool IsRecordingSchedule(TVRecording rec, out int card)
    {
      return _commandProcessor.IsRecordingSchedule(rec,out  card);
    }
    static public bool Paused
    {
      get { return _commandProcessor.Paused; }
      set { _commandProcessor.Paused = value; }
    }
    static public string TVChannelName
    {
      get { return _commandProcessor.TVChannelName; }
    }

  }//public class Recorder
}//namespace MediaPortal.TV.Recording
