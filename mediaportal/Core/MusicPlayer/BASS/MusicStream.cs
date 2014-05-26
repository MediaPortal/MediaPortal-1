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
using System.Globalization;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Player.DSP;
using MediaPortal.TagReader;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Midi;
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

    private SYNCPROC _playbackCrossFadeProcDelegate = null;
    private SYNCPROC _cueTrackEndProcDelegate = null;
    private SYNCPROC _metaTagSyncProcDelegate = null;
    private SYNCPROC _playBackSlideEndDelegate = null;
    private SYNCPROC _streamFreedDelegate = null;

    public delegate void MusicStreamMessageHandler(object sender, StreamAction action);
    public event MusicStreamMessageHandler MusicStreamMessage;

    #endregion

    #region Enum

    public enum StreamAction
    {
      Ended,
      InternetStreamChanged,
      Disposed,
      Crossfading,
      Freed,
    }

    #endregion

    #region Structs

    public struct FileType
    {
      public FileMainType FileMainType;
      public FileSubType FileSubType;
    }

    public struct ReplayGainInfo
    {
      public float? AlbumGain;
      public float? AlbumPeak;
      public float? TrackGain;
      public float? TrackPeak;
    }

    #endregion

    #region Variables

    private int _stream = 0;
    internal static FileType _fileType;
    private BASS_CHANNELINFO _channelInfo;
    private string _filePath;

    private List<int> _streamEventSyncHandles = new List<int>();
    private int _cueTrackEndEventHandler;

    private TAG_INFO _tagInfo;
    private MusicTag _musicTag = null;
    private bool _crossFading = false;

    private ReplayGainInfo _replayGainInfo = new ReplayGainInfo();

    // DSP related Variables
    private DSP_Gain _gain = null;
    private BASS_BFX_DAMP _damp = null;
    private BASS_BFX_COMPRESSOR _comp = null;
    private int _dampPrio = 3;
    private int _compPrio = 2;

    private Dictionary<string, int> _waDspPlugins = new Dictionary<string, int>();

    private bool _disposedMusicStream = false;
    private bool _temporaryStream = false;

    #endregion

    #region Properties

    /// <summary>
    /// Returns the Stream Handle
    /// </summary>
    public int BassStream
    {
      get { return _stream; }
    }

    /// <summary>
    /// Returns the Filepath of the Stream
    /// </summary>
    public string FilePath
    {
      get { return _filePath; }
    }

    /// <summary>
    /// Returns the Channel Info
    /// </summary>
    public BASS_CHANNELINFO ChannelInfo
    {
      get { return _channelInfo; }
    }

    /// <summary>
    /// Indicates that the stream is already disposed.
    /// </summary>
    public bool IsDisposed
    {
      get { return _disposedMusicStream; }
    }

    /// <summary>
    /// Returns the FileType of the Stream
    /// </summary>
    public FileType Filetype
    {
      get { return _fileType; }
    }

    #region Playback Related Properties

    /// <summary>
    /// Returns the Playback status of the stream
    /// </summary>
    public bool IsPlaying
    {
      get { return Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PLAYING; }
    }

    /// <summary>
    /// The stream is Crossfading
    /// </summary>
    public bool IsCrossFading
    {
      get { return _crossFading; }
    }

    /// <summary>
    /// Return Total Seconds of the Stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public double TotalStreamSeconds
    {
      get
      {
        if (_stream == 0)
        {
          return 0;
        }

        return Bass.BASS_ChannelBytes2Seconds(_stream, Bass.BASS_ChannelGetLength(_stream));
      }
    }

    /// <summary>
    /// Retrieve the elapsed time
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public double StreamElapsedTime
    {
      get
      {
        if (_stream == 0)
        {
          return 0;
        }

        // position in bytes
        long pos = Bass.BASS_ChannelGetPosition(_stream);

        // the elapsed time length
        double elapsedtime = Bass.BASS_ChannelBytes2Seconds(_stream, pos);
        return elapsedtime;
      }
    }

    /// <summary>
    /// Returns the Tag Info set from an Internet Stream
    /// </summary>
    public TAG_INFO StreamTags
    {
      get { return _tagInfo; }  
    }

    #endregion
    
    #endregion

    #region Constructor

    /// <summary>
    /// Creates a MusicStream object
    /// </summary>
    /// <param name="filePath">The Path of the song</param>
    public MusicStream(string filePath) : this(filePath, false)
    {
    }

    /// <summary>
    /// Creates a Musicstream object
    /// </summary>
    /// <param name="filePath">The Path of the song</param>
    /// <param name="temporaryStream">Indicates that the stream is just temporary</param>
    public MusicStream(string filePath, bool temporaryStream)
    {
      _temporaryStream = temporaryStream;
      _fileType.FileMainType = FileMainType.Unknown;
      _channelInfo = new BASS_CHANNELINFO();
      _filePath = filePath;

      _playbackCrossFadeProcDelegate = new SYNCPROC(PlaybackCrossFadeProc);
      _cueTrackEndProcDelegate = new SYNCPROC(CueTrackEndProc);
      _metaTagSyncProcDelegate = new SYNCPROC(MetaTagSyncProc);
      _streamFreedDelegate = new SYNCPROC(StreamFreedProc);

      CreateStream();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Create the stream for the file assigned to the Musicstream
    /// </summary>
    private void CreateStream()
    {
      if (!_temporaryStream)
      {
        Log.Info("BASS: ---------------------------------------------");
        Log.Info("BASS: Creating BASS audio stream");
      }

      BASSFlag streamFlags = BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE;

      _fileType = Utils.GetFileType(_filePath);

      switch (_fileType.FileMainType)
      {
        case FileMainType.Unknown:
          return;

        case FileMainType.AudioFile:
        case FileMainType.MidiFile:
          _stream = Bass.BASS_StreamCreateFile(_filePath, 0, 0, streamFlags);

          if (!_temporaryStream)
          {
            // Read the Tag
            _musicTag = TagReader.TagReader.ReadTag(_filePath);
          }
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
          if (_stream != 0 && !_temporaryStream)
          {
            // Get the Tags and set the Meta Tag SyncProc
            _tagInfo = new TAG_INFO(_filePath);
            SetStreamTags(_stream);
            if (BassTags.BASS_TAG_GetFromURL(_stream, _tagInfo))
            {
              GetMetaTags();
            }
            Bass.BASS_ChannelSetSync(_stream, BASSSync.BASS_SYNC_META, 0, _metaTagSyncProcDelegate, IntPtr.Zero);
            Log.Debug("BASS: Webstream found - fetching stream {0}", Convert.ToString(_stream));
          }
          break;
      }

      if (_stream == 0)
      {
        Log.Error("BASS: Unable to create Stream for {0}.  Reason: {1}.", _filePath,
                      Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
        return;
      }

      // When we have a MIDI file, we need to assign the sound banks to the stream
      if (_fileType.FileMainType == FileMainType.MidiFile && Config.SoundFonts != null)
      {
        BassMidi.BASS_MIDI_StreamSetFonts(_stream, Config.SoundFonts, Config.SoundFonts.Length);
      }

      _channelInfo = Bass.BASS_ChannelGetInfo(_stream);
      if (Bass.BASS_ErrorGetCode() != BASSError.BASS_OK)
      {
        Log.Error("BASS: Unable to get information for stream {0}.  Reason: {1}.", _filePath,
                      Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
        return;
      }

      // This stream has just been created to check upfront, the sample rate
      // We don't need further processing of this stream
      if (_temporaryStream)
      {
        return;
      }

      Log.Info("BASS: Stream Information");
      Log.Info("BASS: ---------------------------------------------");
      Log.Info("BASS: File: {0}", _filePath);
      Log.Debug("BASS: Type of Stream: {0}", _channelInfo.ctype);
      Log.Info("BASS: Number of Channels: {0}", _channelInfo.chans);
      Log.Info("BASS: Stream Samplerate: {0}", _channelInfo.freq);
      Log.Debug("BASS: Stream Flags: {0}", _channelInfo.flags);
      Log.Info("BASS: ---------------------------------------------");

      Log.Debug("BASS: Registering stream playback events");
      RegisterPlaybackEvents();

      AttachDspToStream();

      if (Config.EnableReplayGain && _musicTag != null)
      {
        SetReplayGain();
      }
      Log.Info("BASS: Successfully created BASS audio stream");
      Log.Info("BASS: ---------------------------------------------");
    }

    /// <summary>
    /// Sets the ReplayGain Value, which is read from the Tag of the file
    /// </summary>
    private void SetReplayGain()
    {
      _replayGainInfo.AlbumGain = null;
      _replayGainInfo.AlbumPeak = null;
      _replayGainInfo.TrackGain = null;
      _replayGainInfo.TrackPeak = null;

      _replayGainInfo.AlbumGain = ParseReplayGainTagValue(_musicTag.ReplayGainAlbum);
      _replayGainInfo.AlbumPeak = ParseReplayGainTagValue(_musicTag.ReplayGainAlbumPeak);
      _replayGainInfo.TrackGain = ParseReplayGainTagValue(_musicTag.ReplayGainTrack);
      _replayGainInfo.TrackPeak = ParseReplayGainTagValue(_musicTag.ReplayGainTrackPeak);

      if (_replayGainInfo.TrackGain.HasValue || _replayGainInfo.AlbumGain.HasValue)
      {
        Log.Debug("BASS: Replay Gain Data: Track Gain={0}dB, Track Peak={1}, Album Gain={2}dB, Album Peak={3}",
            _replayGainInfo.TrackGain,
            _replayGainInfo.TrackPeak,
            _replayGainInfo.AlbumGain,
            _replayGainInfo.AlbumPeak);
      }
      else
      {
        Log.Debug("BASS: No Replay Gain Information found in stream tags");
      }

      float? gain = null;

      if (Config.EnableAlbumReplayGain && _replayGainInfo.AlbumGain.HasValue)
      {
        gain = _replayGainInfo.AlbumGain;
      }
      else if (_replayGainInfo.TrackGain.HasValue)
      {
        gain = _replayGainInfo.TrackGain;
      }

      if (gain.HasValue)
      {
        Log.Debug("BASS: Setting Replay Gain to {0}dB", gain.Value);
        _gain = new DSP_Gain();
        _gain.ChannelHandle = _stream;
        _gain.Gain_dBV = gain.Value;
        _gain.Start();
      }

    }

    /// <summary>
    /// Removes the "dB" value from the Replaygain tag
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private float? ParseReplayGainTagValue(string s)
    {
      if (s.Length == 0)
      {
        return null;
      }

      // Remove "dB"
      int pos = s.IndexOf(" ");
      if (pos > -1)
        s = s.Substring(0, pos);

      NumberFormatInfo formatInfo = new NumberFormatInfo();
      formatInfo.NumberDecimalSeparator = ".";
      formatInfo.PercentGroupSeparator = ",";
      formatInfo.NegativeSign = "-";

      float f;
      if (float.TryParse(s, NumberStyles.Number, formatInfo, out f))
        return new float?(f);
      else
        return new float?();
    }

    /// <summary>
    /// Attach the various DSP Plugins and effects to the stream
    /// </summary>
    private void AttachDspToStream()
    {
      bool dspActive = Config.DSPActive;

      // BASS DSP/FX
      foreach (BassEffect basseffect in Player.DSP.Settings.Instance.BassEffects)
      {
        dspActive = true;
        foreach (BassEffectParm parameter in basseffect.Parameter)
        {
          setBassDSP(basseffect.EffectName, parameter.Name, parameter.Value);
        }
      }

      // Attach active DSP effects to the Stream
      if (dspActive)
      {
        // BASS effects
        if (_gain != null)
        {
          Log.Debug("BASS: Enabling Gain Effect.");
          _gain.ChannelHandle = _stream;
          _gain.Start();
        }
        if (_damp != null)
        {
          Log.Debug("BASS: Enabling Dynamic Amplifier Effect.");
          int dampHandle = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_BFX_DAMP, _dampPrio);
          Bass.BASS_FXSetParameters(dampHandle, _damp);
        }
        if (_comp != null)
        {
          Log.Debug("BASS: Enabling Compressor Effect.");
          int compHandle = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_BFX_COMPRESSOR, _compPrio);
          Bass.BASS_FXSetParameters(compHandle, _comp);
        }

        // VST Plugins
        foreach (string plugin in Config.VstPlugins)
        {
          Log.Debug("BASS: Enabling VST Plugin: {0}", plugin);
          int vstHandle = BassVst.BASS_VST_ChannelSetDSP(_stream, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
          // Copy the parameters of the plugin as loaded on from the settings
          int vstParm = Config.VstHandles[plugin];
          BassVst.BASS_VST_SetParamCopyParams(vstParm, vstHandle);
        }

        // Init Winamp DSP only if we got a winamp plugin actiavtes
        int waDspPluginHandle = 0;
        if (Player.DSP.Settings.Instance.WinAmpPlugins.Count > 0)
        {
          foreach (WinAmpPlugin plugins in Player.DSP.Settings.Instance.WinAmpPlugins)
          {
            Log.Debug("BASS: Enabling Winamp DSP Plugin: {0}", plugins.PluginDll);
            waDspPluginHandle = BassWaDsp.BASS_WADSP_Load(plugins.PluginDll, 5, 5, 100, 100, null);
            if (waDspPluginHandle > 0)
            {
              _waDspPlugins[plugins.PluginDll] = waDspPluginHandle;
              BassWaDsp.BASS_WADSP_Start(waDspPluginHandle, 0, 0);
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
          BassWaDsp.BASS_WADSP_ChannelSetDSP(waPluginHandle, _stream, 1);
        }
      }
    }

    /// <summary>
    /// Sets the parameter for a given Bass effect
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    private void setBassDSP(string id, string name, string value)
    {
      switch (id)
      {
        case "Gain":
          if (name == "Gain_dbV")
          {
            double gainDB = double.Parse(value);
            if (_gain == null)
            {
              _gain = new DSP_Gain();
            }

            if (gainDB == 0.0)
            {
              _gain.SetBypass(true);
            }
            else
            {
              _gain.SetBypass(false);
              _gain.Gain_dBV = gainDB;
            }
          }
          break;

        case "DynAmp":
          if (name == "Preset")
          {
            if (_damp == null)
            {
              _damp = new BASS_BFX_DAMP();
            }

            switch (Convert.ToInt32(value))
            {
              case 0:
                _damp.Preset_Soft();
                break;
              case 1:
                _damp.Preset_Medium();
                break;
              case 2:
                _damp.Preset_Hard();
                break;
            }
          }
          break;

        case "Compressor":
          if (name == "Threshold")
          {
            if (_comp == null)
            {
              _comp = new BASS_BFX_COMPRESSOR();
            }

            _comp.Preset_Medium();
            _comp.fThreshold = (float)Un4seen.Bass.Utils.DBToLevel(Convert.ToInt32(value) / 10d, 1.0);
          }
          break;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Slide in the Channel over the Defined Crossfade intervall
    /// </summary>
    public void SlideIn()
    {
      if (Config.CrossFadeIntervalMs > 0)
      {
        // Reduce the stream volume to zero so we can fade it in...
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, 0);

        // Fade in from 0 to 1 over the Config.CrossFadeIntervalMs duration
        Bass.BASS_ChannelSlideAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, 1, Config.CrossFadeIntervalMs);
      }
    }

    /// <summary>
    /// Fade out and Stop the Song
    /// </summary>
    /// <param name="stream"></param>
    public void FadeOutStop()
    {
      new Thread(() =>
                   {
                     Log.Debug("BASS: FadeOutStop of stream {0}", _filePath);

                     if (!IsPlaying)
                     {
                       return;
                     }

                     double crossFadeSeconds = 0.0;

                     if (Config.CrossFadeIntervalMs > 0)
                     {
                       crossFadeSeconds = crossFadeSeconds/1000.0;


                       if ((TotalStreamSeconds - (StreamElapsedTime + crossFadeSeconds) > -1))
                       {
                         Bass.BASS_ChannelSlideAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, -1,
                                                         Config.CrossFadeIntervalMs);
                         while (Bass.BASS_ChannelIsSliding(_stream, BASSAttribute.BASS_ATTRIB_VOL))
                         {
                           Thread.Sleep(20);
                         }
                       }
                       else
                       {
                         Bass.BASS_ChannelStop(_stream);
                       }
                     }
                     else
                     {
                       Bass.BASS_ChannelStop(_stream);
                     }
                     Dispose();
                   }
        ) { Name = "BASS FadeOut" }.Start();
    }

    /// <summary>
    /// Set the end position of a song inside a CUE file
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    public void SetCueTrackEndPos(float startPos, float endPos, bool endOnly)
    {
      if (_cueTrackEndEventHandler != 0)
      {
        Bass.BASS_ChannelRemoveSync(_stream, _cueTrackEndEventHandler);
      }

      if (!endOnly)
      {
        Bass.BASS_ChannelSetPosition(_stream, Bass.BASS_ChannelSeconds2Bytes(_stream, startPos));
      }

      if (endPos > startPos)
      {
        _cueTrackEndEventHandler = RegisterCueTrackEndEvent(Bass.BASS_ChannelSeconds2Bytes(_stream, endPos));
      }
    }

    /// <summary>
    /// Resume Playback of a Paused stream
    /// </summary>
    public void ResumePlayback()
    {
      Log.Debug("BASS: Resuming playback of paused stream for {0}", _filePath);
      if (Config.SoftStop)
      {
        Bass.BASS_ChannelSlideAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
      }
      else
      {
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, 1);
      }
    }

    #endregion

    #region BASS SyncProcs

    /// <summary>
    /// Register the various Playback Events
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private void RegisterPlaybackEvents()
    {
      if (Config.CrossFadeIntervalMs > 0)
      {
        _streamEventSyncHandles.Add(RegisterCrossFadeEvent(_stream));
      }
      _streamEventSyncHandles.Add(RegisterStreamFreedEvent(_stream));
    }

    /// <summary>
    /// Register the Fade out Event
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private int RegisterCrossFadeEvent(int stream)
    {
      int syncHandle = 0;
      double fadeOutSeconds = Config.CrossFadeIntervalMs / 1000.0;

      long bytePos = Bass.BASS_ChannelSeconds2Bytes(stream, TotalStreamSeconds - fadeOutSeconds);

      syncHandle = Bass.BASS_ChannelSetSync(stream,
                                            BASSSync.BASS_SYNC_POS,
                                            bytePos, _playbackCrossFadeProcDelegate,
                                            IntPtr.Zero);

      if (syncHandle == 0)
      {
        Log.Debug("BASS: RegisterCrossFadeEvent of stream {0} failed with error {1}", stream,
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
      }
      return syncHandle;
    }

    /// <summary>
    /// Register the CUE file Track End Event
    /// </summary>
    /// <param name="endPos"></param>
    /// <returns></returns>
    private int RegisterCueTrackEndEvent(long endPos)
    {
      int syncHandle = 0;

      syncHandle = Bass.BASS_ChannelSetSync(_stream, BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_POS, endPos,
                                            _cueTrackEndProcDelegate, IntPtr.Zero);

      if (syncHandle == 0)
      {
        Log.Debug("BASS: RegisterPlaybackCueTrackEndEvent of stream {0} failed with error {1}", _stream,
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
      }

      return syncHandle;
    }

    /// <summary>
    /// Register the Stream Freed Event
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private int RegisterStreamFreedEvent(int stream)
    {
      int syncHandle = 0;
      syncHandle = Bass.BASS_ChannelSetSync(stream,
                                            BASSSync.BASS_SYNC_FREE | BASSSync.BASS_SYNC_MIXTIME,
                                            0, _streamFreedDelegate,
                                            IntPtr.Zero);

      if (syncHandle == 0)
      {
        Log.Debug("BASS: RegisterStreamFreedEvent of stream {0} failed with error {1}", stream,
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
      }
      return syncHandle;
    }

    /// <summary>
    /// Fade Out  Procedure
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackCrossFadeProc(int handle, int stream, int data, IntPtr userData)
    {
      new Thread(() =>
                   {
                     try
                     {
                       Log.Debug("BASS: X-Fading out stream {0}", _filePath);

                       // We want to get informed, when Crossfading has ended
                       _playBackSlideEndDelegate = new SYNCPROC(SlideEndedProc);
                       Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_SLIDE, 0, _playBackSlideEndDelegate,
                                                IntPtr.Zero);

                       _crossFading = true;
                       Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 0,
                                                       Config.CrossFadeIntervalMs);
                     }
                     catch (AccessViolationException)
                     {
                       Log.Error("BASS: Caught AccessViolationException in Crossfade Proc");
                     }
                   }
      ) { Name = "BASS X-Fade" }.Start();
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
      new Thread(() =>
                   {
                     _crossFading = false;
                     Log.Debug("BASS: Fading of stream finished.");
                     if (MusicStreamMessage != null)
                     {
                       MusicStreamMessage(this, StreamAction.Ended);
                     }
                   }
      ) { Name = "BASS X-FadeEnded" }.Start();
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
      new Thread(() =>
                   {
                     Log.Debug("BASS: CueTrackEndProc of stream {0}", stream);

                     if (MusicStreamMessage != null)
                     {
                       MusicStreamMessage(this, StreamAction.Crossfading);
                     }

                     bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
                     if (removed)
                     {
                       Log.Debug("BassAudio: *** BASS_ChannelRemoveSync in CueTrackEndProc");
                     }
                   }
       ) { Name = "BASS CueEnd" }.Start();
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
          if (item.ToLowerInvariant().StartsWith("icy-name:"))
          {
            GUIPropertyManager.SetProperty("#Play.Current.Album", item.Substring(9));
          }

          if (item.ToLowerInvariant().StartsWith("icy-genre:"))
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
      new Thread(() =>
                   {
                     // BASS_SYNC_META is triggered on meta changes of SHOUTcast streams
                     if (_tagInfo.UpdateFromMETA(Bass.BASS_ChannelGetTags(channel, BASSTag.BASS_TAG_META), false,
                                                 false))
                     {
                       GetMetaTags();
                     }
                   }
      ) { Name = "BASS MetaSync" }.Start();
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

    /// <summary>
    /// This Callback Procedure is called by BASS when a stream is freed.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="channel"></param>
    /// <param name="data"></param>
    /// <param name="user"></param>
    private void StreamFreedProc(int handle, int channel, int data, IntPtr user)
    {
      {
        if (MusicStreamMessage != null)
        {
          MusicStreamMessage(this, StreamAction.Freed);
        }
      }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      if (_disposedMusicStream)
      {
        return;
      }

      lock (this)
      {
        _disposedMusicStream = true;

        Log.Debug("BASS: Disposing Music Stream {0}", _filePath);

        // Free Winamp resources)
        try
        {
          // Some Winamp dsps might raise an exception when closing
          foreach (int waDspPlugin in _waDspPlugins.Values)
          {
            BassWaDsp.BASS_WADSP_Stop(waDspPlugin);
          }
        }
        catch (Exception) { }
      }
    }

    #endregion
  }
}
