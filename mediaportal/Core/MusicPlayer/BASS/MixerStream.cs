using System;
using System.Runtime.InteropServices;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.BassAsio;
using Un4seen.BassWasapi;

namespace MediaPortal.MusicPlayer.BASS
{
  /// <summary>
  /// This class handles the Mixer Strem, which is used by the BASS Player
  /// </summary>
  public class MixerStream : IDisposable
  {
    #region Variables

    private BassAudioEngine _bassPlayer;
    private int _mixer = 0;
    private float[,] _mixingMatrix = null;
    private bool _upmixing = false;

    private ASIOPROC _asioProc = null;
    private WASAPIPROC _wasapiProc = null;

    private bool _wasapiShared = false;
    private int _wasapiMixedChans = 0;
    private int _wasapiMixedFreq = 0;

    private bool _disposedMixerStream = false;

    private SYNCPROC _playbackEndProcDelegate = null;
    private int _syncProc = 0;

    #endregion

    #region Properties

    /// <summary>
    /// Returns the Mixer Handle
    /// </summary>
    public int BassStream
    {
      get { return _mixer; }
    }

    public bool WasApiShared
    {
      get { return _wasapiShared; }
    }

    public int WasApiMixedChans
    {
      get { return _wasapiMixedChans; }
    }

    public int WasApiMixedFreq
    {
      get { return _wasapiMixedFreq; }
    }

    public bool UpMixing
    {
      get { return _upmixing; }
    }

    #endregion

    #region Constructor

    public MixerStream(BassAudioEngine bassPlayer)
    {
      _bassPlayer = bassPlayer;
      _playbackEndProcDelegate = new SYNCPROC(PlaybackEndProc);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Create a mixer using the stream attributes
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool CreateMixer(MusicStream stream)
    {
      Log.Debug("BASS: ---------------------------------------------");
      Log.Debug("BASS: Creating BASS mixer stream");

      bool result = false;

      BASSFlag mixerFlags = BASSFlag.BASS_MIXER_NONSTOP | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_MIXER_NORAMPIN;

      if (Config.MusicPlayer == AudioPlayer.Asio || Config.MusicPlayer == AudioPlayer.WasApi)
      {
        mixerFlags |= BASSFlag.BASS_STREAM_DECODE;
      }

      int outputChannels = _bassPlayer.DeviceChannels;
      _mixingMatrix = null;

      // See, if we need Upmixing
      if (outputChannels > stream.ChannelInfo.chans)
      {
        Log.Debug("BASS: Found more output channels ({0}) than input channels ({1}). Check for upmixing.", outputChannels,
                 stream.ChannelInfo.chans);
        _mixingMatrix = CreateMixingMatrix(stream.ChannelInfo.chans);
        if (_mixingMatrix != null)
        {
          outputChannels = Math.Min(_mixingMatrix.GetLength(0), outputChannels);
          _upmixing = true;
        }
        else
        {
          outputChannels = stream.ChannelInfo.chans;
        }
      }
      else if (outputChannels < stream.ChannelInfo.chans)
      {
        // Downmix to Stereo
        Log.Debug("BASS: Found more input channels ({0}) than output channels ({1}). Downmix.", stream.ChannelInfo.chans,
                 outputChannels);
        outputChannels = Math.Min(outputChannels, 2);
      }

      Log.Debug("BASS: Creating {0} channel mixer with sample rate of {1}", outputChannels, stream.ChannelInfo.freq);
      _mixer = BassMix.BASS_Mixer_StreamCreate(stream.ChannelInfo.freq, outputChannels, mixerFlags);
      if (_mixer == 0)
      {
        Log.Error("BASS: Unable to create Mixer.  Reason: {0}.",
                  Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
        return false;
      }

      switch (Config.MusicPlayer)
      {
        case AudioPlayer.Bass:
        case AudioPlayer.DShow:

          if (!Bass.BASS_ChannelPlay(_mixer, false))
          {
            Log.Error("BASS: Unable to start Mixer.  Reason: {0}.", Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
            return false;
          }

          result = true;

          break;

        case AudioPlayer.Asio:

          Log.Info("BASS: Initialising ASIO device");

          if (BassAsio.BASS_ASIO_IsStarted() && !BassAsio.BASS_ASIO_Stop())
          {
            Log.Error("BASS: Error stopping Asio Device: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }

          // Disable and Unjoin all the channels
          if (!BassAsio.BASS_ASIO_ChannelReset(false, -1, BASSASIOReset.BASS_ASIO_RESET_ENABLE))
          {
            Log.Error("BASS: Error disabling Asio Channels: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }

          if (!BassAsio.BASS_ASIO_ChannelReset(false, -1, BASSASIOReset.BASS_ASIO_RESET_JOIN))
          {
            Log.Error("BASS: Error unjoining Asio Channels: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }

          _asioProc = new ASIOPROC(AsioCallback);

          BassAsio.BASS_ASIO_ChannelSetVolume(false, -1, (float)Config.StreamVolume / 100f);

          // enable 1st output channel...(0=first)
          Log.Debug("BASS: Joining Asio Channel #{0}", "0");
          BassAsio.BASS_ASIO_ChannelEnable(false, 0, _asioProc, new IntPtr(_mixer));

          // and join the next channels to it
          int numChannels = Math.Max(stream.ChannelInfo.chans, outputChannels);
          for (int i = 1; i < numChannels; i++)
          {
            Log.Debug("BASS: Joining Asio Channel #{0}", i);
            BassAsio.BASS_ASIO_ChannelJoin(false, i, 0);
          }

          // since we joined the channels, the next commands will apply to all channles joined
          // so setting the values to the first channels changes them all automatically
          // set the source format (float, as the decoding channel is)
          if (!BassAsio.BASS_ASIO_ChannelSetFormat(false, 0, BASSASIOFormat.BASS_ASIO_FORMAT_FLOAT))
          {
            Log.Error("BASS: Error setting Asio Sample Format: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }

          // set the source rate
          Log.Debug("BASS: Set sample rate to {0}", stream.ChannelInfo.freq);
          if (!BassAsio.BASS_ASIO_ChannelSetRate(false, 0, (double)stream.ChannelInfo.freq))
          {
            Log.Error("BASS: Error setting Asio Channel Samplerate: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }

          // try to set the device rate too (saves resampling)
          if (!BassAsio.BASS_ASIO_SetRate((double)stream.ChannelInfo.freq))
          {
            Log.Error("BASS: Error setting Asio Samplerate: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }

          // and start playing it...start output using default buffer/latency
          if (!BassAsio.BASS_ASIO_Start(0))
          {
            Log.Error("BASS: Error starting Asio playback: {0}", BassAsio.BASS_ASIO_ErrorGetCode());
          }
          Log.Info("BASS: Finished initialising ASIO device");
          result = true;

          break;

        case AudioPlayer.WasApi:

          Log.Info("BASS: Initialising WASAPI device");

          try
          {
            BassWasapi.BASS_WASAPI_Free();
            Log.Debug("BASS: Freed WASAPI device");
          }
          catch (Exception ex)
          {
            Log.Error("BASS: Exception freeing WASAPI. {0} {1}", ex.Message, ex.StackTrace);
          }

          BASSWASAPIInit initFlags = BASSWASAPIInit.BASS_WASAPI_AUTOFORMAT;

          _wasapiProc = new WASAPIPROC(WasApiCallback);

          bool wasApiExclusiveSupported = true;

          // Check if we have an uneven number of channels
          var chkChannels = outputChannels % 2;
          if (chkChannels == 1)
          {
            Log.Warn("BASS: Found uneven number of channels {0}. increase output channels.", outputChannels);
            outputChannels++; // increase the number of output channels
            wasApiExclusiveSupported = false; // And indicate that we need a new mixer
          }

          // Handle the special case of a 5.0 file being played on a 5.1 or 6.1 device
          if (outputChannels == 5)
          {
            Log.Info("BASS: Found a 5 channel file. Set upmixing with LFE set to silent");
            _mixingMatrix = CreateFiveDotZeroUpMixMatrix();
            wasApiExclusiveSupported = true;
          }

          // If Exclusive mode is used, check, if that would be supported, otherwise init in shared mode
          if (Config.WasApiExclusiveMode)
          {
            initFlags |= BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE;
            _wasapiShared = false;
            _wasapiMixedChans = 0;
            _wasapiMixedFreq = 0;

            BASSWASAPIFormat wasapiFormat = BassWasapi.BASS_WASAPI_CheckFormat(_bassPlayer.DeviceNumber,
                                                                               stream.ChannelInfo.freq,
                                                                               outputChannels,
                                                                               BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE);
            if (wasapiFormat == BASSWASAPIFormat.BASS_WASAPI_FORMAT_UNKNOWN)
            {
              Log.Warn("BASS: WASAPI exclusive mode not directly supported. Let BASS WASAPI choose better mode.");
              wasApiExclusiveSupported = false;
            }
          }
          else
          {
            Log.Debug("BASS: Init WASAPI shared mode with Event driven system enabled.");
            initFlags |= BASSWASAPIInit.BASS_WASAPI_SHARED | BASSWASAPIInit.BASS_WASAPI_EVENT;

            // In case of WASAPI Shared mode we need to setup the mixer to use the same sample rate as set in 
            // the Windows Mixer, otherwise we wioll have increased playback speed
            BASS_WASAPI_DEVICEINFO devInfo = BassWasapi.BASS_WASAPI_GetDeviceInfo(_bassPlayer.DeviceNumber);
            Log.Debug("BASS: Creating {0} channel mixer for frequency {1}", devInfo.mixchans, devInfo.mixfreq);
            _mixer = BassMix.BASS_Mixer_StreamCreate(devInfo.mixfreq, devInfo.mixchans, mixerFlags);
            if (_mixer == 0)
            {
              Log.Error("BASS: Unable to create Mixer.  Reason: {0}.",
                        Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
              return false;
            }
            _wasapiShared = true;
          }

          Log.Debug("BASS: Try to init WASAPI with a Frequency of {0} and {1} channels", stream.ChannelInfo.freq, outputChannels);

          if (BassWasapi.BASS_WASAPI_Init(_bassPlayer.DeviceNumber, stream.ChannelInfo.freq, outputChannels,
                                      initFlags | BASSWASAPIInit.BASS_WASAPI_BUFFER, Convert.ToSingle(Config.BufferingMs / 1000.0), 0f, _wasapiProc, IntPtr.Zero))
          {
            BASS_WASAPI_INFO wasapiInfo = BassWasapi.BASS_WASAPI_GetInfo();

            Log.Debug("BASS: ---------------------------------------------");
            Log.Debug("BASS: Buffer Length: {0}", wasapiInfo.buflen);
            Log.Debug("BASS: Channels: {0}", wasapiInfo.chans);
            Log.Debug("BASS: Frequency: {0}", wasapiInfo.freq);
            Log.Debug("BASS: Format: {0}", wasapiInfo.format.ToString());
            Log.Debug("BASS: InitFlags: {0}", wasapiInfo.initflags.ToString());
            Log.Debug("BASS: Exclusive: {0}", wasapiInfo.IsExclusive.ToString());
            Log.Debug("BASS: ---------------------------------------------");
            Log.Info("BASS: WASAPI Device successfully initialised");

            // Now we need to check, if WASAPI decided to switch to a different mode
            if (Config.WasApiExclusiveMode && !wasApiExclusiveSupported)
            {
              // Recreate Mixer with new value
              Log.Debug("BASS: Creating new {0} channel mixer for frequency {1}", wasapiInfo.chans, wasapiInfo.freq);
              _mixer = BassMix.BASS_Mixer_StreamCreate(wasapiInfo.freq, wasapiInfo.chans, mixerFlags);
              if (_mixer == 0)
              {
                Log.Error("BASS: Unable to create Mixer.  Reason: {0}.",
                          Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
                return false;
              }
            }

            BassWasapi.BASS_WASAPI_SetVolume(BASSWASAPIVolume.BASS_WASAPI_CURVE_DB, (float)Config.StreamVolume / 100f);
            BassWasapi.BASS_WASAPI_Start();
            result = true;
          }
          else
          {
            Log.Error("BASS: Couldn't init WASAPI device. Error: {0}", Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
          }
          break;
      }

      if (result)
      {
        Log.Debug("BASS: Successfully created BASS Mixer stream");
      }
      return result;
    }

    /// <summary>
    /// Attach a stream to the Mixer
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool AttachStream(MusicStream stream)
    {
      Bass.BASS_ChannelLock(_mixer, true);

      // Set SynyPos at end of stream
      SetSyncPos(stream, 0.0);

      bool result = BassMix.BASS_Mixer_StreamAddChannel(_mixer, stream.BassStream,
                                        BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_BUFFER |
                                        BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_MIXER_DOWNMIX |
                                        BASSFlag.BASS_STREAM_AUTOFREE);

      if (!result)
      {
        Log.Error("BASS: Error attaching stream to mixer. {0}", Bass.BASS_ErrorGetCode());
      }

      Bass.BASS_ChannelLock(_mixer, false);

      if (result && _mixingMatrix != null)
      {
        Log.Debug("BASS: Setting mixing matrix...");
        result = BassMix.BASS_Mixer_ChannelSetMatrix(stream.BassStream, _mixingMatrix);
        if (!result)
        {
          Log.Error("BASS: Error attaching Mixing Matrix. {0}", Bass.BASS_ErrorGetCode());
        }
      }

      return result;
    }

    /// <summary>
    /// Sets a SyncPos on the mixer stream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="timePos"></param>
    public void SetSyncPos(MusicStream stream, double timePos)
    {
      double fadeOutSeconds = Config.CrossFadeIntervalMs / 1000.0;
      double totalStreamLen = Bass.BASS_ChannelBytes2Seconds(stream.BassStream, Bass.BASS_ChannelGetLength(stream.BassStream, BASSMode.BASS_POS_BYTES));
      long mixerPos = Bass.BASS_ChannelGetPosition(_mixer, BASSMode.BASS_POS_BYTES | BASSMode.BASS_POS_DECODE);
      long syncPos = mixerPos + Bass.BASS_ChannelSeconds2Bytes(_mixer, totalStreamLen - timePos - fadeOutSeconds);

      if (_syncProc != 0)
      {
        Bass.BASS_ChannelRemoveSync(_mixer, _syncProc);
      }

      GCHandle pFilePath = GCHandle.Alloc(stream);

      _syncProc = Bass.BASS_ChannelSetSync(_mixer,
        BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME,
        syncPos, _playbackEndProcDelegate,
        GCHandle.ToIntPtr(pFilePath));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Callback from Asio to deliver data from Decoding channel
    /// </summary>
    /// <param name="input"></param>
    /// <param name="channel"></param>
    /// <param name="buffer"></param>
    /// <param name="length"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private int AsioCallback(bool input, int channel, IntPtr buffer, int length, IntPtr user)
    {
      try
      {
        return Bass.BASS_ChannelGetData(user.ToInt32(), buffer, length);
      }
      catch (AccessViolationException)
      { }
      return 0;
    }

    /// <summary>
    /// Callback from WasApi to deliver data from Decoding channel
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="length"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private int WasApiCallback(IntPtr buffer, int length, IntPtr user)
    {
      if (_mixer == null || _mixer == 0)
      {
        return 0;
      }
      try
      {
        return Bass.BASS_ChannelGetData(_mixer, buffer, length);
      }
      catch (AccessViolationException)
      {
      }
      catch (Exception)
      {
      }
      return 0;
    }

    /// <summary>
    /// Check, which Mixing Matrix to be used
    /// Thanks to Symphy, author of PureAudio, for this code
    /// </summary>
    /// <param name="inputChannels"></param>
    /// <returns></returns>
    private float[,] CreateMixingMatrix(int inputChannels)
    {
      switch (inputChannels)
      {
        case 1:
          return CreateMonoUpMixMatrix();
        case 2:
          return CreateStereoUpMixMatrix();
        case 4:
          return CreateQuadraphonicUpMixMatrix();
        case 5:
          return CreateFiveDotZeroUpMixMatrix(); // Special case to handle a 5.0 Music File
        case 6:
          return CreateFiveDotOneUpMixMatrix();
        default:
          return null;
      }
    }

    private float[,] CreateMonoUpMixMatrix()
    {
      float[,] mixMatrix = null;

      switch (Config.UpmixMono)
      {
        case MonoUpMix.None:
          Log.Info("BASS: No upmixing of Mono selected");
          break;

        case MonoUpMix.Stereo:
          // Channel 1: left front out = in
          // Channel 2: right front out = in
          mixMatrix = new float[2, 1];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 0] = 1;
          Log.Info("BASS: Using Mono -> Stereo mixing matrix");
          break;

        case MonoUpMix.QuadraphonicPhonic:
          // Channel 1: left front out = in
          // Channel 2: right front out = in
          // Channel 3: left rear out = in
          // Channel 4: right rear out = in
          mixMatrix = new float[4, 1];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 0] = 1;
          mixMatrix[2, 0] = 1;
          mixMatrix[3, 0] = 1;
          Log.Info("BASS: Using Mono -> Quadro mixing matrix");
          break;

        case MonoUpMix.FiveDotOne:
          // Channel 1: left front out = in
          // Channel 2: right front out = in
          // Channel 3: centre out = in
          // Channel 4: LFE out = in
          // Channel 5: left rear/side out = in
          // Channel 6: right rear/side out = in
          mixMatrix = new float[6, 1];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 0] = 1;
          mixMatrix[2, 0] = 1;
          mixMatrix[3, 0] = 1;
          mixMatrix[4, 0] = 1;
          mixMatrix[5, 0] = 1;
          Log.Info("BASS: Using Mono -> 5.1 mixing matrix");
          break;

        case MonoUpMix.SevenDotOne:
          // Channel 1: left front out = in
          // Channel 2: right front out = in
          // Channel 3: centre out = in
          // Channel 4: LFE out = in
          // Channel 5: left rear/side out = in
          // Channel 6: right rear/side out = in
          // Channel 7: left-rear center out = in
          // Channel 8: right-rear center out = in
          mixMatrix = new float[8, 1];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 0] = 1;
          mixMatrix[2, 0] = 1;
          mixMatrix[3, 0] = 1;
          mixMatrix[4, 0] = 1;
          mixMatrix[5, 0] = 1;
          mixMatrix[6, 0] = 1;
          mixMatrix[7, 0] = 1;
          Log.Info("BASS: Using Mono -> 7.1 mixing matrix");
          break;
      }
      return mixMatrix;
    }

    private float[,] CreateStereoUpMixMatrix()
    {
      float[,] mixMatrix = null;

      switch (Config.UpmixStereo)
      {
        case StereoUpMix.None:
          Log.Info("BASS: No upmixing of Stereo selected");
          break;

        case StereoUpMix.QuadraphonicPhonic:
          // Channel 1: left front out = left in
          // Channel 2: right front out = right in
          // Channel 3: left rear out = left in
          // Channel 4: right rear out = right in
          mixMatrix = new float[4, 2];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 1] = 1;
          mixMatrix[2, 0] = 1;
          mixMatrix[3, 1] = 1;
          Log.Info("BASS: Using Stereo -> Quadro mixing matrix");
          break;

        case StereoUpMix.FiveDotOne:
          // Channel 1: left front out = left in
          // Channel 2: right front out = right in
          // Channel 3: centre out = left/right in
          // Channel 4: LFE out = left/right in
          // Channel 5: left rear/side out = left in
          // Channel 6: right rear/side out = right in
          mixMatrix = new float[6, 2];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 1] = 1;
          mixMatrix[2, 0] = 0.5f;
          mixMatrix[2, 1] = 0.5f;
          mixMatrix[3, 0] = 0.5f;
          mixMatrix[3, 1] = 0.5f;
          mixMatrix[4, 0] = 1;
          mixMatrix[5, 1] = 1;
          Log.Info("BASS: Using Stereo -> 5.1 mixing matrix");
          break;

        case StereoUpMix.SevenDotOne:
          // Channel 1: left front out = left in
          // Channel 2: right front out = right in
          // Channel 3: centre out = left/right in
          // Channel 4: LFE out = left/right in
          // Channel 5: left rear/side out = left in
          // Channel 6: right rear/side out = right in
          // Channel 7: left-rear center out = left in
          // Channel 8: right-rear center out = right in
          mixMatrix = new float[8, 2];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 1] = 1;
          mixMatrix[2, 0] = 0.5f;
          mixMatrix[2, 1] = 0.5f;
          mixMatrix[3, 0] = 0.5f;
          mixMatrix[3, 1] = 0.5f;
          mixMatrix[4, 0] = 1;
          mixMatrix[5, 1] = 1;
          mixMatrix[6, 0] = 1;
          mixMatrix[7, 1] = 1;
          Log.Info("BASS: Using Stereo -> 7.1 mixing matrix");
          break;
      }
      return mixMatrix;
    }

    private float[,] CreateQuadraphonicUpMixMatrix()
    {
      float[,] mixMatrix = null;

      switch (Config.UpmixQuadro)
      {
        case QuadraphonicUpMix.None:
          Log.Info("BASS: No upmixing of Quadro selected");
          break;

        case QuadraphonicUpMix.FiveDotOne:
          // Channel 1: left front out = left front in
          // Channel 2: right front out = right front in
          // Channel 3: centre out = left/right front in
          // Channel 4: LFE out = left/right front in
          // Channel 5: left surround out = left surround in
          // Channel 6: right surround out = right surround in
          mixMatrix = new float[6, 4];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 1] = 1;
          mixMatrix[2, 0] = 0.5f;
          mixMatrix[2, 1] = 0.5f;
          mixMatrix[3, 0] = 0.5f;
          mixMatrix[3, 1] = 0.5f;
          mixMatrix[4, 2] = 1;
          mixMatrix[5, 3] = 1;
          Log.Info("BASS: Using Quadro -> 5.1 mixing matrix");
          break;

        case QuadraphonicUpMix.SevenDotOne:
          // Channel 1: left front out = left front in
          // Channel 2: right front out = right front in
          // Channel 3: center out = left/right front in
          // Channel 4: LFE out = left/right front in
          // Channel 5: left surround out = left surround in
          // Channel 6: right surround out = right surround in
          // Channel 7: left back out = left surround in
          // Channel 8: right back out = right surround in
          mixMatrix = new float[8, 4];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 1] = 1;
          mixMatrix[2, 0] = 0.5f;
          mixMatrix[2, 1] = 0.5f;
          mixMatrix[3, 0] = 0.5f;
          mixMatrix[3, 1] = 0.5f;
          mixMatrix[4, 2] = 1;
          mixMatrix[5, 3] = 1;
          mixMatrix[6, 2] = 1;
          mixMatrix[7, 3] = 1;
          Log.Info("BASS: Using Quadro -> 7.1 mixing matrix");
          break;
      }
      return mixMatrix;
    }

    private float[,] CreateFiveDotZeroUpMixMatrix()
    {
      float[,] mixMatrix = null;

      // Handle the Special playback case of a 5.0 music file
      switch (_bassPlayer.DeviceChannels)
      {
        case 6:
           mixMatrix = new float[6, 5] {
          	{1,0,0,0,0}, // left front out = left front in
	          {0,1,0,0,0}, // right front out = right front in
	          {0,0,1,0,0}, // centre out = centre in
	          {0,0,0,0,0}, // LFE out = silent
	          {0,0,0,1,0}, // left rear out = left rear in
	          {0,0,0,0,1}  // right rear out = right rear in
           }; 
          Log.Info("BASS: Upmix 5.0-> 5.1 with LFE empty");
          break;

        case 7:
          mixMatrix = new float[8, 5] {
          	{1,0,0,0,0}, // left front out = left front in
	          {0,1,0,0,0}, // right front out = right front in
	          {0,0,1,0,0}, // centre out = centre in
	          {0,0,0,0,0}, // LFE out = silent
	          {0,0,0,1,0}, // left rear out = left rear in
	          {0,0,0,0,1}, // right rear out = right rear in
            {0,0,0,0,0}, // left back out = silent
            {0,0,0,0,0}  // right back out = silent
           };
          Log.Info("BASS: Upmix 5.0-> 5.1 with LFE empty");
          break;
      }
      return mixMatrix;
    }

    private float[,] CreateFiveDotOneUpMixMatrix()
    {
      float[,] mixMatrix = null;
      switch (Config.UpmixFiveDotOne)
      {
        case FiveDotOneUpMix.None:
          Log.Info("BASS: No upmixing of 5.1 selected");
          break;

        case FiveDotOneUpMix.SevenDotOne:
          mixMatrix = new float[8, 6];
          mixMatrix[0, 0] = 1;
          mixMatrix[1, 1] = 1;
          mixMatrix[2, 2] = 1;
          mixMatrix[3, 3] = 1;
          mixMatrix[4, 4] = 1;
          mixMatrix[5, 5] = 1;
          mixMatrix[6, 4] = 1;
          mixMatrix[7, 5] = 1;
          Log.Info("BASS: Using 5.1 -> 7.1 mixing matrix");
          break;
      }
      return mixMatrix;
    }
    #endregion

    #region SyncProcs

    /// <summary>
    /// End of Playback for a stream has been signaled
    /// Send event to Bass player to start playback of next song
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="userData"></param>
    private void PlaybackEndProc(int handle, int stream, int data, IntPtr userData)
    {
      try
      {
        GCHandle gch = GCHandle.FromIntPtr(userData);
        MusicStream musicstream = (MusicStream)gch.Target;

        Log.Debug("BASS: End of Song {0}", musicstream.FilePath);

        // We need to find out, if the nextsongs sample rate and / or number of channels are different to the one just ended
        // If this is the case we need a new mixer and the OnMusicStreamMessage needs to be invoked in a thread to avoid crashes.
        // In order to have gapless playback, it needs to be invoked in sync.
        MusicStream nextStream = null;
        Playlists.PlayListItem nextSong = Playlists.PlayListPlayer.SingletonPlayer.GetNextItem();
        MusicStream._fileType = Utils.GetFileType(musicstream.FilePath);
        if (nextSong != null && MusicStream._fileType.FileMainType != FileMainType.WebStream)
        {
          nextStream = new MusicStream(nextSong.FileName, true);
        }
        else if (MusicStream._fileType.FileMainType == FileMainType.WebStream)
        {
          _bassPlayer.OnMusicStreamMessage(musicstream, MusicStream.StreamAction.InternetStreamChanged);
        }

        bool newMixerNeeded = false;
        if (nextStream != null && nextStream.BassStream != 0)
        {
          if (_bassPlayer.NewMixerNeeded(nextStream))
          {
            newMixerNeeded = true;
          }
          nextStream.Dispose();
        }

        if (newMixerNeeded)
        {
          if (Config.MusicPlayer == AudioPlayer.WasApi && BassWasapi.BASS_WASAPI_IsStarted())
          {
            BassWasapi.BASS_WASAPI_Stop(true);
          }

          // Unplug the Source channel from the mixer
          Log.Debug("BASS: Unplugging source channel from Mixer.");
          BassMix.BASS_Mixer_ChannelRemove(musicstream.BassStream);

          // invoke a thread because we need a new mixer
          Log.Debug("BASS: Next song needs a new mixer. Invoke a thread.");
          new Thread(() => _bassPlayer.OnMusicStreamMessage(musicstream, MusicStream.StreamAction.Crossfading)) { Name = "BASS" }.Start();
        }
        else
        {
          _bassPlayer.OnMusicStreamMessage(musicstream, MusicStream.StreamAction.Crossfading);
        }
      }
      catch (AccessViolationException)
      {
        Log.Error("BASS: Caught AccessViolationException in Playback End Proc");
      }
    }


    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      if (_disposedMixerStream)
      {
        return;
      }

      lock (this)
      {
        _disposedMixerStream = true;

        Log.Debug("BASS: Disposing Mixer Stream");

        try
        {
          if (!Bass.BASS_StreamFree(_mixer))
          {
            Log.Error("BASS: Error freeing mixer: {0}", Bass.BASS_ErrorGetCode());
          }
          _mixer = 0;
        }
        catch (Exception ex)
        {
          Log.Error("BASS: Exception disposing mixer - {0}. {1}", ex.Message, ex.StackTrace);
        }
      }
    }

    #endregion
  }
}
