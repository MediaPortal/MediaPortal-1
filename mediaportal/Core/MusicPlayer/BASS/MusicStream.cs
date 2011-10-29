#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Player.DSP;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.AddOn.Vst;
using Un4seen.Bass.AddOn.WaDsp;
using Un4seen.Bass.Misc;

namespace MediaPortal.MusicPlayer.BASS
{
  /// <summary>
  /// This Class Handles a Music Stream to be used by the BASS Player
  /// </summary>
  public class MusicStream : IDisposable
  {

    #region Delegates

    private SYNCPROC PlaybackFadeOutProcDelegate = null;
    private SYNCPROC PlaybackEndProcDelegate = null;
    private SYNCPROC CueTrackEndProcDelegate = null;
    private SYNCPROC MetaTagSyncProcDelegate = null;
    private SYNCPROC PlayBackSlideEndDelegate = null;

    public delegate void MusicStreamMessageHandler(object sender, StreamAction action);
    public event MusicStreamMessageHandler MusicStreamMessage;

    #endregion

    #region Enum

    public enum StreamAction
    {
      Ended,
      CrossFade,
      InternetStreamChanged,
      Freed,
    }

    #endregion

    #region Structs

    public struct FileType
    {
      public FileMainType FileMainType;
      public FileSubType FileSubType;
    }

    #endregion

    #region Variables

    private int _stream = 0;
    private FileType _fileType;
    private BASS_CHANNELINFO _channelInfo;
    private string _filePath;

    private List<int> _streamEventSyncHandles = new List<int>();

    private TAG_INFO _tagInfo;
    private bool _crossFading = false;

    // DSP related Variables
    private static DSP_Gain _gain = null;
    private static BASS_BFX_DAMP _damp = null;
    private static BASS_BFX_COMPRESSOR _comp = null;
    private static int _dampPrio = 3;
    private static int _compPrio = 2;

    #endregion

    #region Properties

    public int BassStream
    {
      get { return _stream; }
    }

    public string FilePath
    {
      get { return _filePath; }
    }

    public BASS_CHANNELINFO ChannelInfo
    {
      get { return _channelInfo; }
    }

    public bool IsPlaying
    {
      get { return Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PLAYING; }
    }

    public bool IsCrossFading
    {
      get { return _crossFading; }
    }

    #endregion

    #region Constructor

    public MusicStream(string filePath)
    {
      _fileType.FileMainType = FileMainType.Unknown;
      _channelInfo = new BASS_CHANNELINFO();
      _filePath = filePath;

      CreateStream();
    }

    #endregion

    #region Private Methods

    private void CreateStream()
    {
      // Enable Later, when everything is changed
      //BASSFlag streamFlags = BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT;
      BASSFlag streamFlags = BASSFlag.BASS_SAMPLE_FLOAT;

      _fileType = Utils.GetFileType(_filePath);

      switch (_fileType.FileMainType)
      {
        case FileMainType.Unknown:
          return;

        case FileMainType.AudioFile:
        case FileMainType.MidiFile:
          _stream = Bass.BASS_StreamCreateFile(_filePath, 0, 0, streamFlags);
          break;

        case FileMainType.CDTrack:
          // StreamCreateFile causes problems with Multisession disks, so use StreamCreate with driveindex and track index
          int driveindex = Config.CdDriveLetters.IndexOf(_filePath.Substring(0, 1));
          int tracknum = Convert.ToInt16(_filePath.Substring(_filePath.IndexOf(".cda") - 2, 2));
          _stream = BassCd.BASS_CD_StreamCreate(driveindex, tracknum - 1, streamFlags);
          break;

        case FileMainType.MODFile:
          _stream = Bass.BASS_MusicLoad(_filePath, 0, 0,
                             BASSFlag.BASS_SAMPLE_SOFTWARE | BASSFlag.BASS_SAMPLE_FLOAT |
                             BASSFlag.BASS_MUSIC_AUTOFREE | BASSFlag.BASS_MUSIC_PRESCAN |
                             BASSFlag.BASS_MUSIC_RAMP, 0);
          break;

        case FileMainType.WebStream:
          // Turn on parsing of ASX files
          Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 2);
          _stream = Bass.BASS_StreamCreateURL(_filePath, 0, streamFlags, null, IntPtr.Zero);
          if (_stream != 0)
          {
            // Get the Tags and set the Meta Tag SyncProc
            _tagInfo = new TAG_INFO(_filePath);
            SetStreamTags(_stream);

            if (BassTags.BASS_TAG_GetFromURL(_stream, _tagInfo))
            {
              GetMetaTags();
            }

            Bass.BASS_ChannelSetSync(_stream, BASSSync.BASS_SYNC_META, 0, MetaTagSyncProcDelegate, IntPtr.Zero);
          }
          Log.Debug("BASS: Webstream found - fetching stream {0}", Convert.ToString(_stream));
          break;
      }

      if (_stream == 0)
      {
        Log.Error("BASS: Unable to create Stream for {0}.  Reason: {1}.", _filePath,
                      Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
        return;
      }


      _channelInfo = Bass.BASS_ChannelGetInfo(_stream);
      if (Bass.BASS_ErrorGetCode() != BASSError.BASS_OK)
      {
        Log.Error("BASS: Unable to get information for stream {0}.  Reason: {1}.", _filePath,
                      Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
        return;
      }

      Log.Info("BASS: Channel Information for file: {0}", _filePath);
      Log.Info("BASS: ---------------------------------------------");
      Log.Info("BASS: Type of Channels: {0}", _channelInfo.ctype);
      Log.Info("BASS: Number of Channels: {0}", _channelInfo.chans);
      Log.Info("BASS: Channel Frequency: {0}", _channelInfo.freq);
      Log.Info("BASS: ---------------------------------------------");

      Log.Debug("BASS: Registering Playback Events");

      PlaybackFadeOutProcDelegate = new SYNCPROC(PlaybackFadeOutProc);
      PlaybackEndProcDelegate = new SYNCPROC(PlaybackEndProc);
      CueTrackEndProcDelegate = new SYNCPROC(CueTrackEndProc);
      MetaTagSyncProcDelegate = new SYNCPROC(MetaTagSyncProc);

      RegisterPlaybackEvents();

    }

    /*
    private void AttachDspToStream()
    {
      // Attach active DSP effects to the Stream
      if (Config.DSPActive)
      {
        // BASS effects
        if (_gain != null)
        {
          _gain.ChannelHandle = _stream;
          _gain.Start();
        }
        if (_damp != null)
        {
          int dampHandle = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_BFX_DAMP, _dampPrio);
          Bass.BASS_FXSetParameters(dampHandle, _damp);
        }
        if (_comp != null)
        {
          int compHandle = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_BFX_COMPRESSOR, _compPrio);
          Bass.BASS_FXSetParameters(compHandle, _comp);
        }

        // VST Plugins
        foreach (string plugin in _VSTPlugins)
        {
          int vstHandle = BassVst.BASS_VST_ChannelSetDSP(_stream, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
          // Copy the parameters of the plugin as loaded on from the settings
          int vstParm = _vstHandles[plugin];
          BassVst.BASS_VST_SetParamCopyParams(vstParm, vstHandle);
        }

        // Init Winamp DSP only if we got a winamp plugin actiavtes
        int waDspPlugin = 0;
        if (Player.DSP.Settings.Instance.WinAmpPlugins.Count > 0 && !_waDspInitialised)
        {
          BassWaDsp.BASS_WADSP_Init(GUIGraphicsContext.ActiveForm);
          _waDspInitialised = true;
          foreach (WinAmpPlugin plugins in Player.DSP.Settings.Instance.WinAmpPlugins)
          {
            waDspPlugin = BassWaDsp.BASS_WADSP_Load(plugins.PluginDll, 5, 5, 100, 100, null);
            if (waDspPlugin > 0)
            {
              _waDspPlugins[plugins.PluginDll] = waDspPlugin;
              BassWaDsp.BASS_WADSP_Start(waDspPlugin, 0, 0);
            }
            else
            {
              Log.Debug("Couldn't load WinAmp Plugin {0}. Error code: {1}", plugins.PluginDll,
                        Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
            }
          }
        }

        foreach (int waPluginHandle in _waDspPlugins.Values)
        {
          BassWaDsp.BASS_WADSP_ChannelSetDSP(waPluginHandle, stream, 1);
        }
      }
    }
    */

    #endregion

    #region BASS SyncProcs

    /// <summary>
    /// Register the various Playback Events
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private void RegisterPlaybackEvents()
    {
      _streamEventSyncHandles.Add(RegisterPlaybackFadeOutEvent(_stream, Config.CrossFadeIntervalMs));
      _streamEventSyncHandles.Add(RegisterPlaybackEndEvent(_stream));
    }

    /// <summary>
    /// Register the Fade out Event
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fadeOutMS"></param>
    /// <returns></returns>
    private int RegisterPlaybackFadeOutEvent(int stream, int fadeOutMS)
    {
      int syncHandle = 0;
      long len = Bass.BASS_ChannelGetLength(stream); // length in bytes
      double totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len); // the total time length
      double fadeOutSeconds = 0;

      if (fadeOutMS > 0)
        fadeOutSeconds = fadeOutMS / 1000.0;

      long bytePos = Bass.BASS_ChannelSeconds2Bytes(stream, totaltime - fadeOutSeconds);

      syncHandle = Bass.BASS_ChannelSetSync(stream,
                                            BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_POS,
                                            bytePos, PlaybackFadeOutProcDelegate,
                                            IntPtr.Zero);

      if (syncHandle == 0)
      {
        Log.Debug("BASS: RegisterPlaybackFadeOutEvent of stream {0} failed with error {1}", stream,
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
      }

      return syncHandle;
    }

    /// <summary>
    /// Register the Playback end Event
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private int RegisterPlaybackEndEvent(int stream)
    {
      int syncHandle = 0;

      syncHandle = Bass.BASS_ChannelSetSync(stream,
                                            BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_END,
                                            0, PlaybackEndProcDelegate,
                                            IntPtr.Zero);

      if (syncHandle == 0)
      {
        Log.Debug("BASS: RegisterPlaybackEndEvent of stream {0} failed with error {1}", stream,
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
      }

      return syncHandle;
    }

    /// <summary>
    /// Register the CUE file Track End Event
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    private int RegisterCueTrackEndEvent(int stream, long endPos)
    {
      int syncHandle = 0;

      syncHandle = Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_POS, endPos,
                                            CueTrackEndProcDelegate, IntPtr.Zero);

      if (syncHandle == 0)
      {
        Log.Debug("BASS: RegisterPlaybackCueTrackEndEvent of stream {0} failed with error {1}", stream,
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
      }

      return syncHandle;
    }


    /// <summary>
    /// Unregister the Playback Events
    /// </summary>
    /// <returns></returns>
    public bool UnregisterPlaybackEvents()
    {
      try
      {
        foreach (int syncHandle in _streamEventSyncHandles)
        {
          if (syncHandle != 0)
          {
            Bass.BASS_ChannelRemoveSync(_stream, syncHandle);
          }
        }
      }

      catch
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Fade Out  Procedure
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackFadeOutProc(int handle, int stream, int data, IntPtr userData)
    {
      Log.Debug("BASS: Fading out stream {0}", _filePath);

      if (Config.CrossFadeIntervalMs > 0)
      {
        // Only sent GUI_MSG_PLAYBACK_CROSSFADING when gapless/crossfading mode is used
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_CROSSFADING, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendThreadMessage(msg);
      }

      // We want to get informed, when Crossfading has ended
      PlayBackSlideEndDelegate = new SYNCPROC(SlideEndedProc);
      Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_SLIDE, 0, PlayBackSlideEndDelegate, IntPtr.Zero);

      _crossFading = true;
      Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, -1, Config.CrossFadeIntervalMs);
    }

    /// <summary>
    /// This Callback Procedure is called by BASS, once a Slide Ended.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="channel"></param>
    /// <param name="data"></param>
    /// <param name="user"></param>
    private void SlideEndedProc(int handle, int channel, int data, IntPtr user)
    {
      _crossFading = false;
      Log.Debug("BASS: Fading of stream finished.");
    }


    /// <summary>
    /// Playback end Procedure
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackEndProc(int handle, int stream, int data, IntPtr userData)
    {
      Log.Debug("BASS: End of stream {0}", _filePath);

      if (MusicStreamMessage != null)
      {
        MusicStreamMessage(this, StreamAction.Ended);
      }
    }

    /// <summary>
    /// CUE Track End Procedure
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void CueTrackEndProc(int handle, int stream, int data, IntPtr userData)
    {
      Log.Debug("BASS: CueTrackEndProc of stream {0}", stream);

      if (Config.CrossFadeIntervalMs > 0)
      {
        // Only sent GUI_MSG_PLAYBACK_CROSSFADING when gapless/crossfading mode is used
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_CROSSFADING, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendThreadMessage(msg);
      }

      if (MusicStreamMessage != null)
      {
        MusicStreamMessage(this, StreamAction.CrossFade);
      }

      bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
      if (removed)
      {
        Log.Debug("BassAudio: *** BASS_ChannelRemoveSync in CueTrackEndProc");
      }
    }

    /// <summary>
    /// Gets the tags from the Internet Stream.
    /// </summary>
    /// <param name="stream"></param>
    private void SetStreamTags(int stream)
    {
      string[] tags = Bass.BASS_ChannelGetTagsICY(stream);
      if (tags != null)
      {
        foreach (string item in tags)
        {
          if (item.ToLower().StartsWith("icy-name:"))
          {
            GUIPropertyManager.SetProperty("#Play.Current.Album", item.Substring(9));
          }

          if (item.ToLower().StartsWith("icy-genre:"))
          {
            GUIPropertyManager.SetProperty("#Play.Current.Genre", item.Substring(10));
          }

          Log.Info("BASS: Connection Information: {0}", item);
        }
      }
      else
      {
        tags = Bass.BASS_ChannelGetTagsHTTP(stream);
        if (tags != null)
        {
          foreach (string item in tags)
          {
            Log.Info("BASS: Connection Information: {0}", item);
          }
        }
      }
    }

    /// <summary>
    /// This Callback Procedure is called by BASS, once a song changes.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="channel"></param>
    /// <param name="data"></param>
    /// <param name="user"></param>
    private void MetaTagSyncProc(int handle, int channel, int data, IntPtr user)
    {
      // BASS_SYNC_META is triggered on meta changes of SHOUTcast streams
      if (_tagInfo.UpdateFromMETA(Bass.BASS_ChannelGetTags(channel, BASSTag.BASS_TAG_META), false, false))
      {
        GetMetaTags();
      }
    }

    /// <summary>
    /// Set the Properties out of the Tags
    /// </summary>
    private void GetMetaTags()
    {
      // There seems to be an issue with setting correctly the title via taginfo
      // So let's filter it out ourself
      string title = _tagInfo.title;
      int streamUrlIndex = title.IndexOf("';StreamUrl=");
      if (streamUrlIndex > -1)
      {
        title = _tagInfo.title.Substring(0, streamUrlIndex);
      }

      Log.Info("BASS: Internet Stream. New Song: {0} - {1}", _tagInfo.artist, title);
      // and display what we get
      GUIPropertyManager.SetProperty("#Play.Current.Album", _tagInfo.album);
      GUIPropertyManager.SetProperty("#Play.Current.Artist", _tagInfo.artist);
      GUIPropertyManager.SetProperty("#Play.Current.Title", title);
      GUIPropertyManager.SetProperty("#Play.Current.Comment", _tagInfo.comment);
      GUIPropertyManager.SetProperty("#Play.Current.Genre", _tagInfo.genre);
      GUIPropertyManager.SetProperty("#Play.Current.Year", _tagInfo.year);

      if (MusicStreamMessage != null)
      {
        MusicStreamMessage(this, StreamAction.InternetStreamChanged);
      }
    }

    #endregion



    #region IDisposable Members

    public void Dispose()
    {
      Log.Debug("BASS: Disposing Music Stream {0}", _filePath);
      UnregisterPlaybackEvents();
      Bass.BASS_StreamFree(_stream);
    }

    #endregion
  }
}
