using System;
using MediaPortal.GUI.Library;
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

    private ASIOPROC _asioProc = null;
    private WASAPIPROC _wasapiProc = null;

    private bool _wasapiSwitchedToShared = false;
    private int _wasapiMixedChans = 0;
    private int _wasapiMixedFreq = 0;

    #endregion

    #region Properties

    /// <summary>
    /// Returns the Mixer Handle
    /// </summary>
    public int BassStream
    {
      get { return _mixer; }
    }

    public bool WasApiSwitchedtoShared
    {
      get { return _wasapiSwitchedToShared; }
    }

    public int WasApiMixedChans
    {
      get { return _wasapiMixedChans; }
    }

    public int WasApiMixedFreq
    {
      get { return _wasapiMixedFreq; }
    }

    #endregion
    
    #region Constructor

    public MixerStream(BassAudioEngine bassPlayer)
    {
      _bassPlayer = bassPlayer;
    }

    #endregion

    #region Public Methods

    public bool CreateMixer(MusicStream stream)
    {
      bool result = false;

      if (Config.MusicPlayer == AudioPlayer.WasApi)
      {
        BassWasapi.BASS_WASAPI_Free();
      }

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
        Log.Info("BASS: Found more output channels than input channels. Check for upmixing.");
        _mixingMatrix = CreateMixingMatrix(stream.ChannelInfo.chans);
        if (_mixingMatrix != null)
        {
          outputChannels = Math.Min(_mixingMatrix.GetLength(0), outputChannels);
        }
        else
        {
          outputChannels = stream.ChannelInfo.chans;
        }
      }
      else if (outputChannels < stream.ChannelInfo.chans)
      {
        // Downmix to Stereo
        Log.Info("BASS: Found more input channels than output channels. Downmix.");
        outputChannels = Math.Min(outputChannels, 2);
      }

      Log.Debug("BASS: Creating {0} channel mixer with sample rate of {1}", outputChannels, stream.ChannelInfo.freq);

      _mixer = BassMix.BASS_Mixer_StreamCreate(stream.ChannelInfo.freq, outputChannels, mixerFlags);

      Bass.BASS_ChannelPlay(_mixer, false);

      switch (Config.MusicPlayer)
      {
        case AudioPlayer.Bass:
        case AudioPlayer.DShow:

          result = true;

          break;

        case AudioPlayer.Asio:

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
          result = true;

          break;

        case AudioPlayer.WasApi:

          BASSWASAPIInit initFlags = BASSWASAPIInit.BASS_WASAPI_AUTOFORMAT;

          _wasapiProc = new WASAPIPROC(WasApiCallback);

          int frequency = stream.ChannelInfo.freq;
          int chans = outputChannels;

          // If Exclusive mode is used, check, if that would be supported, otherwise init in shared mode
          if (Config.WasApiExclusiveMode)
          {
            BASSWASAPIFormat wasapiFormat = BassWasapi.BASS_WASAPI_CheckFormat(_bassPlayer.DeviceNumber,
                                                                               stream.ChannelInfo.freq,
                                                                               stream.ChannelInfo.chans,
                                                                               BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE);

            if (wasapiFormat == BASSWASAPIFormat.BASS_WASAPI_FORMAT_UNKNOWN)
            {
              Log.Warn("BASS: Stream can't be played in WASAPI exclusive mode. Switch to WASAPI shared mode.");

              initFlags |= BASSWASAPIInit.BASS_WASAPI_SHARED | BASSWASAPIInit.BASS_WASAPI_EVENT;

              BASS_WASAPI_DEVICEINFO deviceinfo = BassWasapi.BASS_WASAPI_GetDeviceInfo(_bassPlayer.DeviceNumber);
              frequency = deviceinfo.mixfreq;
              chans = deviceinfo.mixchans;

              // Save the original frequency, so that we don't need to recreate the mixer every time
              _wasapiSwitchedToShared = true;
              _wasapiMixedChans = stream.ChannelInfo.chans;
              _wasapiMixedFreq = stream.ChannelInfo.freq;

              // Recreate Mixer with new value
              Log.Debug("BASS: Creating new {0} channel mixer for frequency {1}", chans, frequency);
              _mixer = BassMix.BASS_Mixer_StreamCreate(frequency, chans, mixerFlags);
              Bass.BASS_ChannelPlay(_mixer, false);
            }
            else
            {
              initFlags |= BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE;
              _wasapiSwitchedToShared = false;
              _wasapiMixedChans = 0;
              _wasapiMixedFreq = 0;
            }
          }
          else
          {
            Log.Debug("BASS: Init WASAPI shared mode with Event driven system enabled.");
            initFlags |= BASSWASAPIInit.BASS_WASAPI_SHARED | BASSWASAPIInit.BASS_WASAPI_EVENT;
          }

          if (BassWasapi.BASS_WASAPI_Init(_bassPlayer.DeviceNumber, frequency, chans,
                                      initFlags, 0f, 0f, _wasapiProc, IntPtr.Zero))
          {
            BASS_WASAPI_INFO wasapiInfo = BassWasapi.BASS_WASAPI_GetInfo();

            Log.Info("BASS: WASAPI Device successfully initialised");
            Log.Info("BASS: ---------------------------------------------");
            Log.Info("BASS: Buffer Length: {0}", wasapiInfo.buflen);
            Log.Info("BASS: Channels: {0}", wasapiInfo.chans);
            Log.Info("BASS: Frequency: {0}", wasapiInfo.freq);
            Log.Info("BASS: Format: {0}", wasapiInfo.format.ToString());
            Log.Info("BASS: InitFlags: {0}", wasapiInfo.initflags.ToString());
            Log.Info("BASS: Exclusive: {0}", wasapiInfo.IsExclusive.ToString());
            Log.Info("BASS: ---------------------------------------------");

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

      return result;
    }

    public bool AttachStream(MusicStream stream)
    {
      Bass.BASS_ChannelLock(_mixer, true);
      bool result = BassMix.BASS_Mixer_StreamAddChannel(_mixer, stream.BassStream,
                                        BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_BUFFER |
                                        BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_MIXER_DOWNMIX);
        
      if (result && _mixingMatrix != null)
      {
        Log.Debug("BASS: Setting mixing matrix...");
        result = BassMix.BASS_Mixer_ChannelSetMatrix(stream.BassStream, _mixingMatrix);
        if (!result)
        {
          Log.Error("BASS: Error attaching Mixing Matrix. {0}", Bass.BASS_ErrorGetCode());
        }
      }
      Bass.BASS_ChannelLock(_mixer, false);
      return result;
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
      if (_mixer == 0)
      {
        return 0;
      }
      try
      {
        return Bass.BASS_ChannelGetData(_mixer, buffer, length);
      }
      catch (AccessViolationException)
      { }
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

    #region IDisposable Members

    public void Dispose()
    {
      Log.Debug("BASS: Disposing Mixer Stream");
      if (!Bass.BASS_ChannelStop(_mixer))
      {
        Log.Error("BASS: Error stopping mixer: {0}", Bass.BASS_ErrorGetCode());
      }
      if (!Bass.BASS_StreamFree(_mixer))
      {
        Log.Error("BASS: Error freeing mixer: {0}", Bass.BASS_ErrorGetCode());
      }
    }

    #endregion
  }
}
