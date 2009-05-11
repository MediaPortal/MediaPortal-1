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
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
using MediaPortal.Configuration;
using MediaPortal.TV.Scanning;

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
    static bool automaticbacktoback;
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
		static Object thisLock = new Object();
    #endregion

    #region events
    public delegate void OnTvViewHandler(int card, TVCaptureDevice device);
    public delegate void OnTvChannelChangeHandler(string tvChannelName);

    public delegate void OnTvRecordingChangedHandler();
    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);

    //event which happens when the state of a recording changes (like started or stopped)
    public static event OnTvRecordingChangedHandler OnTvRecordingChanged = null;
    //event which happens when a recording about to be recorded
    public static event OnTvRecordingHandler OnTvRecordingStarted = null;
    //event which happens when a recording has ended
    public static event OnTvRecordingHandler OnTvRecordingEnded = null;
    //event which happens when TV viewing is started
    public static event OnTvViewHandler OnTvViewingStarted = null;
    //event which happens when TV viewing is stopped
    public static event OnTvViewHandler OnTvViewingStopped = null;
    public static event OnTvChannelChangeHandler OnTvChannelChanged = null;
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

    public static bool Running
    {
      get
      {
        if (_commandProcessor == null) _state = State.None;
        return (_state != State.None);
      }
    }
    /// <summary>
    /// This method will Start the recorder. It
    ///   -Loads the capture cards from capturecards.xml (made by the setup)
    ///   -Loads the recordings (programs scheduled to record) from the tvdatabase
    ///   -Loads the TVchannels from the tvdatabase
    ///   -starts the thread which handles all the tv cards
    /// </summary>
    public static void Start()
    {
			lock (thisLock)
			{
				if (Running) return;//if we are initialized already then no need todo anything
				CommandProcessor processor = new CommandProcessor();
				processor.Start();
				Start(processor);
			}
    }

    public static void Start(CommandProcessor processor)
    {
      if (Running) return;//if we are initialized already then no need todo anything
      Log.Info("Recorder: start");
      _state = State.Initializing;
      _commandProcessor = processor;
      RecorderProperties.Init();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        automaticbacktoback = xmlreader.GetValueAsBool("mytv", "automaticbacktoback", false);
        if (_commandProcessor != null) _commandProcessor.TVChannelName = xmlreader.GetValueAsString("mytv", "channel", string.Empty);
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
        if (_commandProcessor != null) _commandProcessor.Paused = true;
      }

      GUIWindowManager.Receivers += new SendMessageHandler(Recorder.OnMessage);
      _state = State.Initialized;

      if (_commandProcessor != null)
      {
        _commandProcessor.OnTvChannelChanged += new MultiCardBase.OnTvChannelChangeHandler(_commandProcessor_OnTvChannelChanged);
        _commandProcessor.OnTvViewingStarted += new MultiCardBase.OnTvViewHandler(_commandProcessor_OnTvViewingStarted);
        _commandProcessor.OnTvViewingStopped += new MultiCardBase.OnTvViewHandler(_commandProcessor_OnTvViewingStopped);

        _commandProcessor.TVCards.OnTvRecordingChanged += new TvCardCollection.OnTvRecordingChangedHandler(TVCards_OnTvRecordingChanged);
        _commandProcessor.TVCards.OnTvRecordingEnded += new TvCardCollection.OnTvRecordingHandler(TVCards_OnTvRecordingEnded);
        _commandProcessor.TVCards.OnTvRecordingStarted += new TvCardCollection.OnTvRecordingHandler(TVCards_OnTvRecordingStarted);
      }
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
    public static void Stop()
    {
      if (!Running) return;
      Log.Info("Recorder: stop");
      //unsubscribe from events
      GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);
      RecorderProperties.Clean();


      //wait until the process thread has stopped.
      _state = State.None;
      if (_commandProcessor != null) _commandProcessor.Dispose();
      _commandProcessor = null;
    }//public static void Stop()

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
    /// Controls toggling between regular & exclusive DirectX mode.
    /// </summary>
    /// <param name="enable">indicator if we want to enable or disable exclusive DirectX</param>
    static void SwitchDXExclusive(bool enable)
    {
      //Note, because of a direct3d limitation we cannot switch between
      //normal / exclusive mode when a file is playing
      if (!Running) return;
      if (g_Player.Playing) return;
      if (enable)
      {
        // we enter my tv, enable exclusive mode
        Log.WriteFile(LogType.Recorder, "Recorder: Enabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      else
      {
        // we leave my tv, disable exclusive mode
        Log.WriteFile(LogType.Recorder, "Recorder: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }


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
    public static bool NeedChannelSwitchForRecording(TVRecording rec)
    {
      if (!Running) return false;
      //are we viewing the channel requested, then there is no need to switch
      if (IsViewing() && _commandProcessor.TVChannelName == rec.Channel) return false;

      //no, check if there's another card which is free. ifso we can use that one and
      //there is no need to switch channels
      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i] as TVCaptureDevice;

        Log.Info("NeedChannelSwitchForRecording rec1={0} ts={1} view={2} rec2={3}", dev.IsRecordingAt(PrePostRecord.Instance.PreRecordingWarningTime), dev.IsTimeShifting, dev.View, dev.IsRecording);
        //is the card free?
        if (!dev.IsRecordingAt(PrePostRecord.Instance.PreRecordingWarningTime) && !(dev.IsTimeShifting && !dev.IsRecording) && !dev.View)
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
    public static void RecordNow(string channelName, bool manualStop)
    {
      if (channelName == null) return;
      if (channelName == string.Empty) return;

      if (!Running) return;

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
        if (String.Compare(chan.Name, channelName, false) == 0)
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
        Log.WriteFile(LogType.Recorder, "Recorder:record now:{0} program:{1}", channelName, program.Title);
      }
      else
      {
        if (program != null && manualStop)
        {
          //record current playing program for the next 4 hours
          tmpRec.Start = program.Start;
          tmpRec.End = MediaPortal.Util.Utils.datetolong(DateTime.Now.AddHours(4));
          tmpRec.Title = program.Title;
          tmpRec.IsContentRecording = false;//make a reference recording! (record from timeshift buffer)
          Log.WriteFile(LogType.Recorder, "Recorder:record now:{0} program:{1}", channelName, program.Title);
        }
        else
        {
          //no tvguide data, just record the next 4 hours
          Log.WriteFile(LogType.Recorder, "Recorder:record now:{0} for next 4 hours", channelName);
          tmpRec.Start = MediaPortal.Util.Utils.datetolong(DateTime.Now);
          tmpRec.End = MediaPortal.Util.Utils.datetolong(DateTime.Now.AddHours(4));
          tmpRec.Title = GUILocalizeStrings.Get(413);
          if (program != null)
            tmpRec.Title = program.Title;
          tmpRec.IsContentRecording = true;//make a content recording! (record from now)
        }
      }

      Log.WriteFile(LogType.Recorder, "Recorder:   start: {0} {1}", tmpRec.StartTime.ToShortDateString(), tmpRec.StartTime.ToShortTimeString());
      Log.WriteFile(LogType.Recorder, "Recorder:   end  : {0} {1}", tmpRec.EndTime.ToShortDateString(), tmpRec.EndTime.ToShortTimeString());

      AddRecording(ref tmpRec);
      CheckRecordingsCommand cmd = new CheckRecordingsCommand();
      _commandProcessor.AddCommand(cmd);
    }//public static void RecordNow(string channelName)

    /// <summary>
    /// Adds a new recording to the tv database and arranges the priority 
    /// </summary>
    /// <param name="rec">new recording to add to the database</param>
    /// <returns></returns>
    public static int AddRecording(ref TVRecording rec)
    {
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      
      Recorder.AddNoPrePost(ref rec);

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
    public static void StopRecording(TVRecording rec)
    {
      RemoveNoPrePost(ref rec);
      if (!Running) return;
      //add a new RecorderCommand which holds 'rec'
      //and tell the process thread to handle it
      Log.WriteFile(LogType.Recorder, "Recorder:StopRecording({0})", rec.Title);
      CancelRecordingCommand cmd = new CancelRecordingCommand(rec);
      _commandProcessor.AddCommand(cmd);
    }
    /// <summary>
    /// Stops all recording on the current channel. 
    /// </summary>
    /// <remarks>
    /// Only stops recording. timeshifting wont be stopped so user can continue to watch the channel
    /// </remarks>
    public static void StopRecording()
    {
      if (!Running) return;
      //add a new RecorderCommand to tell the process thread
      //that it must stop recording
      Log.WriteFile(LogType.Recorder, "Recorder:StopRecording()");
      StopRecordingCommand cmd = new StopRecordingCommand();
      _commandProcessor.AddCommand(cmd);
    }

    /// <summary>
    /// Property which returns if any tvcard is recording
    /// </summary>
    public static bool IsAnyCardRecording()
    {
      if (!Running) return false;

      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
        if (dev.IsRecording) return true;
      }
      return false;
    }//public static bool IsAnyCardRecording()

    /// <summary>
    /// Property which returns if any card is recording the specified tv channel
    /// </summary>
    public static bool IsRecordingChannel(string channel)
    {
      if (!Running) return false;

      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
        if (dev.IsRecording && dev.CurrentTVRecording.Channel == channel) return true;
      }
      return false;
    }//public static bool IsRecordingChannel(string channel)


    /// <summary>
    /// Property which returns if current selected card is recording
    /// </summary>
    public static bool IsRecording()
    {
      if (!Running) return false;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;

      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      return dev.IsRecording;
    }//public static bool IsRecording()

    /// <summary>
    /// Property which returns if current tv channel has teletext or not
    /// </summary>
    public static bool HasTeletext()
    {
      if (!Running) return false;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      return dev.HasTeletext;
    }

    /// <summary>
    /// Property which returns if current selected card supports timeshifting
    /// </summary>
    public static bool DoesSupportTimeshifting()
    {
      if (!Running) return false;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;

      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      return dev.SupportsTimeShifting;
    }//public static bool DoesSupportTimeshifting()

    /// <summary>
    /// Property which returns the friendly name for a card
    /// </summary>
    /// <param name="card">tv card index</param>
    /// <returns>string which contains the friendly name</returns>
    public static string GetFriendlyNameForCard(int card)
    {
      if (!Running) return string.Empty;
      if (card < 0 || card >= _commandProcessor.TVCards.Count) return string.Empty;
      TVCaptureDevice dev = _commandProcessor.TVCards[card];
      return dev.FriendlyName;
    }//public static string GetFriendlyNameForCard(int card)

    /// <summary>
    /// Returns the tv hannel name of the channel we're currently watching
    /// </summary>
    /// <returns>
    /// Returns the Channel name of the channel we're currently watching
    /// </returns>
    public static string GetTVChannelName()
    {
      if (!Running) return string.Empty;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return string.Empty;
      return _commandProcessor.TVChannelName;
    }//public static string GetTVChannelName()

    /// <summary>
    /// Returns the TV Recording we're currently recording
    /// </summary>
    /// <returns>
    /// </returns>
    public static TVRecording GetTVRecording()
    {
      if (!Running) return null;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;

      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      if (dev.IsRecording) return dev.CurrentTVRecording;
      return null;
    }//public static TVRecording GetTVRecording()


    /// <summary>
    /// Property which returns the current program being recorded. 
    /// If no programs are being recorded at the moment
    /// it will return null;
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public static TVProgram ProgramRecording
    {
      get
      {
        if (!Running) return null;
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;
        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
        if (dev.IsRecording) return dev.CurrentProgramRecording;
        return null;
      }
    }//public static TVProgram ProgramRecording

    /// <summary>
    /// Property which returns the current TVRecording being recorded. 
    /// If no recordings are being recorded at the moment
    /// it will return null;
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Database.TVRecording"/>
    public static TVRecording CurrentTVRecording
    {
      get
      {
        if (!Running) return null;
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;
        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
        if (dev.IsRecording) return dev.CurrentTVRecording;
        return null;
      }
    }//public static TVRecording CurrentTVRecording


    /// <summary>
    /// Returns true if we're timeshifting
    /// </summary>
    /// <returns></returns>
    public static bool IsTimeShifting()
    {
      if (!Running) return false;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return false;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
      if (dev.IsTimeShifting) return true;
      return false;
    }//public static bool IsTimeShifting()

    /// <summary>
    /// Returns true if we're watching live tv
    /// </summary>
    /// <returns></returns>
    public static bool IsViewing()
    {
      if (!Running) return false;
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
    }//public static bool IsViewing()

    /// <summary>
    /// Property which returns true if the card specified by the 'cardId' is currently used
    /// for viewing tv
    /// </summary>
    /// <param name="cardId">id of tv card to check</param>
    /// <returns></returns>
    public static bool IsCardViewing(int cardId)
    {
      if (!Running) return false;
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
    }//public static bool IsViewing()

    /// <summary>
    /// Property which get TV Viewing mode.
    /// if TV Viewing  mode is turned on then live tv will be shown
    /// </summary>
    public static bool View
    {
      get
      {
        if (!Running) return false;
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
    }//public static bool View

    /// <summary>
    /// property which returns the date&time the current recording was started
    /// </summary>
    public static DateTime TimeRecordingStarted
    {
      get
      {
        if (!Running) return DateTime.MinValue;
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
    public static DateTime TimeTimeshiftingStarted
    {
      get
      {
        if (!Running) return DateTime.MinValue;
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
    public static int Count
    {
      get
      {
        if (!Running) return 0;
        return _commandProcessor.TVCards.Count;
      }
    }

    /// <summary>
    /// Indexer which returns the TVCaptureDevice object for a given card
    /// </summary>
    /// <param name="index">card number (0-Count)</param>
    /// <returns>TVCaptureDevice object</returns>
    public static TVCaptureDevice Get(int index)
    {
      if (!Running) return null;
      if (index < 0 || index >= _commandProcessor.TVCards.Count) return null;
      return _commandProcessor.TVCards[index] as TVCaptureDevice;
    }
    /// <summary>
    /// Property which returns the Signal Strength of the current tv card used
    /// </summary>
    public static int SignalStrength
    {
      get
      {
        if (!Running) return 0;
        if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return 0;

        TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];
        return dev.SignalStrength;
      }
    }

    /// <summary>
    /// Property which returns the signal quality of the current tv card used
    /// </summary>
    public static int SignalQuality
    {
      get
      {


        if (!Running) return 0;
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
    public static string GetRecordingFileName(TVRecording rec)
    {

      if (!Running) return string.Empty;
      int card;
      if (!_commandProcessor.IsRecordingSchedule(rec, out card)) return string.Empty;
      TVCaptureDevice dev = _commandProcessor.TVCards[card] as TVCaptureDevice;

      return dev.RecordingFileName;
    }


    /// <summary>
    /// Property which returns the timeshifting filename for a specific card
    /// </summary>
    /// <param name="card">card index</param>
    /// <returns>filename of the timeshifting file</returns>
    public static string GetTimeShiftFileName(int card)
    {

      if (!Running) return string.Empty;
      if (card < 0 || card >= _commandProcessor.TVCards.Count) return string.Empty;
      TVCaptureDevice dev = _commandProcessor.TVCards[card];
      string FileName = dev.TimeShiftFullFileName;
      return FileName;
    }

    /// <summary>
    /// Property which returns the timeshifting filename for a specific card id
    /// </summary>
    /// <param name="card">card id</param>
    /// <returns>filename of the timeshifting file</returns>
    public static string GetTimeShiftFileNameByCardId(int cardId)
    {

      if (!Running) return string.Empty;
      for (int i = 0; i < _commandProcessor.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = _commandProcessor.TVCards[i];
        if (dev.ID == cardId)
        {
          string FileName = dev.TimeShiftFullFileName;
          return FileName;
        }
      }
      return string.Empty;
    }




    /// <summary>
    /// Property which returns true if we currently listening to a radio station
    /// </summary>
    /// <returns>true if listening to radio</returns>
    public static bool IsRadio()
    {

      if (!Running) return false;
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
    public static string RadioStationName()
    {

      if (!Running) return string.Empty;
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
    public static void StopRadio()
    {

      if (!Running) return;
      //Send command to process thread to stop listening to radio

      StopRadioCommand cmd = new StopRadioCommand();
      _commandProcessor.AddCommand(cmd);
    }

    /// <summary>
    /// Start listening to a radio channel
    /// </summary>
    /// <param name="radioStationName"></param>
    public static void StartRadio(string radioStationName)
    {

      if (!Running) return;
      //Send command to process thread to start listening to radio
      StartRadioCommand cmd = new StartRadioCommand(radioStationName);
      _commandProcessor.Execute(cmd);
      if (!cmd.Succeeded)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, GUIWindowManager.ActiveWindow, 0, 0, 665, 757, null);
        GUIWindowManager.SendThreadMessage(msg);
      }
    }


    /// <summary>
    /// Stop viewing on all cards
    /// </summary>
    static bool reEntrantStopViewing = false;
    public static void StopViewing()
    {
      if (!Running) return;
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
        _commandProcessor.Execute(cmd);
        SwitchDXExclusive(false);
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
    public static bool StartViewing(string channel, bool TVOnOff, bool timeshift, bool wait, out string errorMessage)
    {
      try
      {
        if (CommandProcessor != null) CommandProcessor.ControlTimeShifting = false;
        if (!Running)
        {
          errorMessage = "Recorder is not started";
          return false;
        }
        errorMessage = string.Empty;
        if (reEntrantStartViewing)
        {
          errorMessage = GUILocalizeStrings.Get(763);// "Recorder is busy";
          Log.WriteFile(LogType.Recorder, true, "Recorder:StartViewing() reentrant");
          return false;
        }
        if (TVOnOff)
        {
          if (IsViewing() && IsTimeShifting() == timeshift && TVChannelName == channel) return true;
          g_Player.Stop();
          SwitchDXExclusive(true);
        }
        else
        {
          g_Player.Stop();
        }
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
          }
          else
          {
            cmd = new ViewTvCommand(channel);
          }
        }
        else
        {
          cmd = new StopTvCommand();
        }
        //wait till thread finished this command
        if (wait || !TVOnOff)
        {
          _commandProcessor.Execute(cmd);
          if (!TVOnOff)
          {
            SwitchDXExclusive(false);
          }
          if (cmd.Succeeded) return true;
          errorMessage = cmd.ErrorMessage;
          return false;
        }
        else
        {
          _commandProcessor.AddCommand(cmd);
        }
      }
      finally
      {
        if (CommandProcessor != null) CommandProcessor.ControlTimeShifting = true;
        reEntrantStartViewing = false;
      }
      return true;
    }

    public static void AddNoPrePost(ref TVRecording rec)
    {
      if (!automaticbacktoback)
        return;
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      foreach (TVRecording recording in recs)
      {
        if (rec.Channel == recording.Channel && rec.EndTime == recording.StartTime)
        {
          Log.Debug("blockrecording add {0} before {1}", rec.Title, recording.Title);
          Log.Debug("blockrecording update {0}", recording.Title);
          rec.PaddingEnd = -2;
          recording.PaddingFront = -2;
          TVDatabase.UpdateRecording(recording, TVDatabase.RecordingChange.Modified);
        }
        if (rec.Channel == recording.Channel && rec.StartTime == recording.EndTime)
        {
          Log.Debug("blockrecording add {0} after {1}", rec.Title, recording.Title);
          Log.Debug("blockrecording update {0}", recording.Title);
          rec.PaddingFront = -2;
          recording.PaddingEnd = -2;
          TVDatabase.UpdateRecording(recording, TVDatabase.RecordingChange.Modified);
        }
      }
    }

    public static void RemoveNoPrePost(ref TVRecording rec)
    {
      if (!automaticbacktoback)
        return;
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      foreach (TVRecording recording in recs)
      {
        if (rec.Channel == recording.Channel && rec.EndTime == recording.StartTime)
        {
          Log.Debug("blockrecording remove {0} before {1}", rec.Title, recording.Title);
          Log.Debug("blockrecording update {0}", recording.Title);
          rec.PaddingEnd = -1;
          recording.PaddingFront = -1;
          TVDatabase.UpdateRecording(recording, TVDatabase.RecordingChange.Modified);
        }
        if (rec.Channel == recording.Channel && rec.StartTime == recording.EndTime)
        {
          Log.Debug("blockrecording remove {0} after {1}", rec.Title, recording.Title);
          Log.Debug("blockrecording update {0}", recording.Title);
          rec.PaddingFront = -1;
          recording.PaddingEnd = -1;
          TVDatabase.UpdateRecording(recording, TVDatabase.RecordingChange.Modified);
        }
      }
    }


    public static CommandProcessor CommandProcessor
    {
      get
      {
        return (_commandProcessor);
      }
    }



    /// <summary>
    /// Scheduler main loop. This function needs to get called on a regular basis.
    /// It will handle all scheduler tasks
    /// </summary>
    public static void Process()
    {

      if (!Running) return;
      if (_commandProcessor.Paused) return;
      if (GUIGraphicsContext.InVmr9Render) return;
      TimeSpan ts = DateTime.Now - _progressBarTimer;
      if (g_Player.Playing && (Math.Abs(g_Player.Duration - _duration) >= 1 || Math.Abs(g_Player.CurrentPosition - _lastPosition) >= 1))
      {
        RecorderProperties.UpdateRecordingProperties();
        _progressBarTimer = DateTime.Now;
        _duration = g_Player.Duration;
        _lastPosition = g_Player.CurrentPosition;
      }
      else if (ts.TotalMilliseconds > 10000)
      {
        RecorderProperties.UpdateRecordingProperties();
        _progressBarTimer = DateTime.Now;
        _duration = g_Player.Duration;
        _lastPosition = g_Player.CurrentPosition;
      }

    }//public static void Process()





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
    public static void OnMessage(GUIMessage message)
    {

      if (!Running) return;
      if (message == null) return;
      switch (message.Message)
      {

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP:
          Stop();
          break;
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
            _commandProcessor.Execute(cmd);
          }

          break;
      }//switch(message.Message)
    }//public static void OnMessage(GUIMessage message)



    public static int GetAudioLanguage()
    {
      if (!Running) return -1;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return -1;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];

      return dev.GetAudioLanguage();
    }

    public static void SetAudioLanguage(int audioPid)
    {
      if (!Running) return;
      SetAudioLanguageCommand cmd = new SetAudioLanguageCommand(audioPid);
      _commandProcessor.AddCommand(cmd);
    }

    public static ArrayList GetAudioLanguageList()
    {

      if (!Running) return null;
      if (_commandProcessor.CurrentCardIndex < 0 || _commandProcessor.CurrentCardIndex >= _commandProcessor.TVCards.Count) return null;
      TVCaptureDevice dev = _commandProcessor.TVCards[_commandProcessor.CurrentCardIndex];

      return dev.GetAudioLanguageList();
    }

    public static void DeleteRecording(TVRecorded rec)
    {
      MediaPortal.Util.Utils.DeleteRecording(rec.FileName);
      TVDatabase.RemoveRecordedTV(rec);
      VideoDatabase.DeleteMovie(rec.FileName);

    }

    public static bool IsRecordingSchedule(TVRecording rec, out int card)
    {
      card = -1;
      if (!Running) return false;
      return _commandProcessor.IsRecordingSchedule(rec, out  card);
    }
    public static bool Paused
    {
      get
      {

        if (!Running) return true;
        return _commandProcessor.Paused;
      }
      set { _commandProcessor.Paused = value; }
    }
    public static string TVChannelName
    {
      get
      {
        if (!Running) return string.Empty;
        return _commandProcessor.TVChannelName;
      }
    }

    #region AutoTune
    public static void StartAutoTune(NetworkType networkType, int card, AutoTuneCallback autoTuneCallback)
    {
      AutoTuneCommand autoTuneCommand = new AutoTuneCommand(networkType, card, autoTuneCallback);
      _commandProcessor.AddCommand(autoTuneCommand);
    }
        
    #endregion


  }//public class Recorder
}//namespace MediaPortal.TV.Recording
