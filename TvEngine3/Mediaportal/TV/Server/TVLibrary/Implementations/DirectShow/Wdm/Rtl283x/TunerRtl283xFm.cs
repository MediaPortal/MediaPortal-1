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
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Microsoft.Win32;
using Encoder = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component.Encoder;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Rtl283x
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for the proprietary FM radio mode supported by
  /// tuners based on Realtek's RTL283x series of chipsets.
  /// </summary>
  /// <remarks>
  /// The tuner filter can only be instanciated and operate from within a single threaded COM
  /// apartment. Since the TVE environment is a multi-threaded apartment, all interaction with the
  /// graph or tuner must be performed from a separate thread which runs in a STA.
  /// Call-backs are trickier. If you try to interact with the graph or tuner in any way in that
  /// context it will result in dead-lock, because the STA message queue doesn't get pumped until
  /// after the call-back completes. You must allow the call-back to complete before attempting any
  /// interaction.
  /// In short, this code is complex. Please take care and make sure you understand what you're
  /// doing before you make changes.
  /// </remarks>
  internal class TunerRtl283xFm : TunerDirectShowMpeg2TsBase
  {
    #region enums

    private enum GraphJobType
    {
      Load,
      Tune,
      GetSignalStatus,
      SetTunerState,
      Unload,
      TsWriterMethodITsWriter,
      TsWriterMethodIGrabberSiDvb,
      TsWriterMethodIGrabberSiMpeg
    }

    private enum Rtl283xFmResult
    {
      Fail = 0,
      Success,
      RdsDisplayBufferMissing
    }

    private enum Rtl283xFmScanStepSize : int
    {
      Step50kHz = 50,
      Step100kHz = 100,
      Step200kHz = 200
    }

    private enum Rtl283xFmScanDirection
    {
      Decrease = 0,
      Increase
    }

    private enum Rtl283xFmSampleRate : uint
    {
      Sr32000Hz = 32000,
      Sr48000Hz = 48000
    }

    private enum Rtl283xFmDeEmphasisTimeConstant : byte
    {
      FiftyMicroSeconds = 0,
      SeventyFiveMicroSeconds
    }

    [Flags]
    private enum Rtl283xFmSignalControlProperty : uint
    {
      None = 0,
      // Attenuate the audio output level (audio and noise) if signal quality is poor. Default: on.
      SoftMute = 0x01,
      // Blend stereo audio gradually based on the signal quality. Default: on.
      StereoBlend = 0x02,
      // Only go to stereo mode when the channel separation is more than 10 dB. Default: off.
      StereoSwitch = 0x04,
      // Reduce audio bandwidth in low SNR conditions. Default: on.
      HighCutControl = 0x08
    }

    private enum Rtl283xFmAudioChannelConfig : byte
    {
      Mono = 0,
      Stereo
    }

    [Flags]
    private enum Rtl283xFmRdsSignalControlProperty : uint
    {
      None,
      // Decode cyclic redundancy check bits and correct errors. Burst errors of up to 5 bits can be corrected. Default: on.
      CyclicRedundancyChecking = 1,
      // The decoder is only activated when the signal quality is above the threshold. Default: threshold 1 = 0, threshold 2 = 1 => 66%.
      DecoderActivationSignalQualityThreshold1 = 2,
      DecoderActivationSignalQualityThreshold2 = 4,
    }

    private enum Rtl283xFmRdsGroupType : byte
    {
      Group0a = 1,
      Group0b
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct RdsData    // FM_RDS_DISPLAY_STRUCT
    {
      public Rtl283xFmRdsGroupType GroupType;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
      public string ProgramServiceName;

      // DI/PTYI bits
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
      public string Bit3Description;                          // "Static PTY" / "Dynamic PTY"
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
      public string Bit2Description;                          // "Not Compressed" / "Compressed"
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
      public string Bit1Description;                          // "Not Artificial Head" / "Artificial Head"
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
      public string Bit0Description;                          // "Mono" / "Stereo"

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
      public string MusicOrSpeechDescription;                 // "Speech" / "Music"

      /// <remarks>
      /// There are four possible strings, corresponding with the four possible
      /// combinations of the TA/TP bits:
      /// TP = 0, TA = 0: "No traffic message"
      /// TP = 0, TA = 1: "traffic message in EON"
      /// TP = 1, TA = 0: "traffic msg exist not broadcast"
      /// TP = 1, TA = 1: "broadcasting traffic message"
      /// </remarks>
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
      public string TrafficAnnouncementOrProgrammeDescription;

      // unit = kHz
      public float AlternativeFrequencyOne;
      public float AlternativeFrequencyTwo;
    }

    #endregion

    #region COM interfaces

    [Guid("6c433cea-7f9c-40cc-a670-bacae16097b8"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IRtl283xFmSource
    {
      /// <summary>
      /// Debug use only.
      /// </summary>
      /// <param name="mediaType">The media type to set.</param>
      /// <returns>an HRESULT indicating whether the media type was set successfully</returns>
      [PreserveSig]
      int DABSetMediaType(byte mediaType);

      /// <summary>
      /// Set the tuner's audio sample rate.
      /// </summary>
      /// <param name="sampleRate">The audio sample rate for the tuner to use.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetAudioSampleRate(Rtl283xFmSampleRate sampleRate);

      /// <summary>
      /// Set the tuner's frequency.
      /// </summary>
      /// <remarks>
      /// The tuner will switch to direct-tuning mode.
      /// </remarks>
      /// <param name="frequency">The frequency to tune to. The unit is kilo-Hertz (kHz).</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetFrequency(int frequency);

      /// <summary>
      /// Perform an automated scan for the next available station.
      /// </summary>
      /// <remarks>
      /// The tuner will switch to scan-tuning mode.
      /// </remarks>
      /// <param name="startFrequency">The frequency to start scanning from. The unit is kilo-Hertz (kHz).</param>
      /// <param name="stepSize">The scan step size.</param>
      /// <param name="direction">The direction for the scan search relative to the start frequency.</param>
      /// <param name="maxSteps">The maximum number of frequencies to check.</param>
      /// <param name="stationFrequency">The frequency of the station found by the scan.</param>
      /// <param name="stationSignalQuality">The signal quality of the station found by the scan (0 - 100, best is 100).</param>
      /// <returns><c>RtlFmResult.Success</c> if a station was found, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult ScanNextProg(int startFrequency, Rtl283xFmScanStepSize stepSize, Rtl283xFmScanDirection direction, int maxSteps, out int stationFrequency, out int stationSignalQuality);

      /// <summary>
      /// Get the tuner's supported frequency range.
      /// </summary>
      /// <remarks>
      /// Doesn't work. The values returned are always zero.
      /// </remarks>
      /// <param name="lowerLimit">The lower limit of the tuner's range. The unit is kilo-Hertz (kHz).</param>
      /// <param name="upperLimit">The upper limit of the tuner's range. The unit is kilo-Hertz (kHz).</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetTunerRange(out int lowerLimit, out int upperLimit);

      /// <summary>
      /// Check whether the tuner is locked onto a signal.
      /// </summary>
      /// <remarks>
      /// Only available in direct-tuning mode.
      /// </remarks>
      /// <param name="isLocked"><c>True</c> if the tuner is currently locked onto a signal.</param>
      /// <returns><c>RtlFmResult.Success</c> if the lock status is successfully retrieved, otherwise <c>RtlFmResult.Fail</c> if the status is not successfully retrieved or the tuner is not in direct-tuning mode.</returns>
      [PreserveSig]
      Rtl283xFmResult GetSignalLock([MarshalAs(UnmanagedType.Bool)] out bool isLocked);

      /// <summary>
      /// Check the current signal quality.
      /// </summary>
      /// <remarks>
      /// Only available in direct-tuning mode.
      /// </remarks>
      /// <param name="quality">The current quality of the signal that the tuner is tuned to.
      /// 0   = no signal, tuner not locked
      /// 20  = poor, possibly very noisy
      /// 40  = average, tolerable audio quality
      /// 60  = good
      /// >80 = excellent
      /// 100 = maximum
      /// </param>
      /// <returns><c>RtlFmResult.Success</c> if the signal quality is successfully retrieved, otherwise <c>RtlFmResult.Fail</c> if the quality is not successfully retrieved or the tuner is not in direct-tuning mode.</returns>
      [PreserveSig]
      Rtl283xFmResult GetSignalQuality(out int quality);

      /// <summary>
      /// Get the current audio characteristics.
      /// </summary>
      /// <param name="channelCount">The number of audio channels received.</param>
      /// <param name="sampleRate">The audio sample rate. The unit is Hertz (Hz).</param>
      /// <param name="sampleSize">The number of bits per sample.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetPCMInfo(out byte channelCount, out Rtl283xFmSampleRate sampleRate, out uint sampleSize);

      /// <summary>
      /// Set the audio de-emphasis time constant.
      /// </summary>
      /// <param name="timeConstant">The de-emphasis time constant.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetDeemphasisTC(Rtl283xFmDeEmphasisTimeConstant timeConstant);

      /// <summary>
      /// Get the values of the signal control properties.
      /// </summary>
      /// <param name="propertyValues">The current property values.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetSignalQualityCtr(out Rtl283xFmSignalControlProperty propertyValues);

      /// <summary>
      /// Set the values of the signal control properties.
      /// </summary>
      /// <param name="propertyValues">The property values to set.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetSignalQualityCtr(Rtl283xFmSignalControlProperty propertyValues);

      /// <summary>
      /// Set the quality threshold for station identification during automated
      /// scanning.
      /// </summary>
      /// <param name="thresholdQuality">The quality threshold value, which should be between 10 and 100.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetScanStopQuality(uint thresholdQuality);

      /// <summary>
      /// Check whether the radio data system (RDS) decoder is synchronised
      /// (locked).
      /// </summary>
      /// <param name="isSynchronised"><c>True</c> if the RDS decoder is currently synchronised.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetRDSSync([MarshalAs(UnmanagedType.Bool)] out bool isSynchronised);

      /// <summary>
      /// Set the values of the radio data system (RDS) decoder control
      /// properties.
      /// </summary>
      /// <param name="propertyValues">The property values to set.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetRDSCtr(Rtl283xFmRdsSignalControlProperty propertyValues);

      /// <summary>
      /// Get the values of the radio data system (RDS) decoder control
      /// properties.
      /// </summary>
      /// <param name="propertyValues">The property values to set.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetRDSCtr(out Rtl283xFmRdsSignalControlProperty propertyValues);

      /// <summary>
      /// Start processing radio data system (RDS) data.
      /// </summary>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult StartRDS();

      /// <summary>
      /// Stop processing radio data system (RDS) data.
      /// </summary>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult StopRDS();

      /// <summary>
      /// Check the current radio data system (RDS) signal quality.
      /// </summary>
      /// <param name="quality">The current quality of the RDS signal that the tuner is receiving. The value range is from 0 to 100.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetRDSQuality(out int quality);

      /// <summary>
      /// Set the radio data system (RDS) decoder's data buffer.
      /// </summary>
      /// <param name="buffer">The RDS data buffer.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult SetRDSDisplay(IntPtr buffer);

      /// <summary>
      /// Enable the radio data system (RDS) decoder.
      /// </summary>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult EnableRDSDecoder();

      /// <summary>
      /// Disable the radio data system (RDS) decoder.
      /// </summary>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult DisableRDSDecoder();

      /// <summary>
      /// Check the current audio channel configuration.
      /// </summary>
      /// <param name="config">The audio channel configuration.</param>
      /// <returns><c>RtlFmResult.Success</c> if successful, otherwise <c>RtlFmResult.Fail</c></returns>
      [PreserveSig]
      Rtl283xFmResult GetMonoStereo(out Rtl283xFmAudioChannelConfig config);
    }

    #endregion

    private class GraphJob
    {
      public GraphJobType JobType;
      public string MethodName;     // Only used for TsWriterMethod* jobs.
      public object[] Parameters;
      public object ReturnValue;
      public Exception ThrownException;
      public ManualResetEvent WaitEvent;
    }

    #region constants

    private static readonly Guid SOURCE_FILTER_CLSID = new Guid(0x6b368f8c, 0xf383, 0x44d3, 0xb8, 0xc2, 0x3a, 0x15, 0x0b, 0x70, 0xb1, 0xc9);

    private static readonly int RDS_DATA_SIZE = Marshal.SizeOf(typeof(RdsData));    // 200

    #endregion

    #region variables

    private IRtl283xFmSource _fmSource = null;
    private Capture _capture = new Capture();
    private Encoder _encoder = new Encoder();
    private DsDevice _mainTunerDevice = null;
    private bool _mainTunerDeviceInUse = false;
    private TsWriterWrapper _staTsWriter = null;
    private bool _isRdsEnabled = false;

    /// <summary>
    /// The tuner's sub-channel manager.
    /// </summary>
    private ISubChannelManager _subChannelManager = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    private IChannelScannerInternal _channelScanner = null;

    // STA graph thread variables.
    private object _graphThreadLock = new object();
    private Thread _graphThread = null;
    private volatile bool _stopGraphThread = false;
    private AutoResetEvent _graphThreadWaitEvent = null;

    private object _jobQueueLock = new object();
    private Queue<GraphJob> _jobs = new Queue<GraphJob>();

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerRtl283xFm"/> class.
    /// </summary>
    /// <param name="mainTunerDevice">The main BDA tuner device for for the tuner.</param>
    public TunerRtl283xFm(DsDevice mainTunerDevice)
      : base("Realtek RTL283x FM Tuner", mainTunerDevice.DevicePath + "FM", mainTunerDevice.TunerInstanceIdentifier >= 0 ? mainTunerDevice.TunerInstanceIdentifier.ToString() : null, mainTunerDevice.ProductInstanceIdentifier, BroadcastStandard.FmRadio)
    {
      _mainTunerDevice = mainTunerDevice;
    }

    ~TunerRtl283xFm()
    {
      Dispose(false);
    }

    #endregion

    private IBaseFilter LoadSourceFilter()
    {
      this.LogDebug("RTL283x FM: load source filter");

      // Normally the RTL283x driver only supports operation of one tuner in a
      // special mode. The driver selects this tuner by first match on a list
      // of friendly names located in the registry. We manipulate the registry
      // list and tuner name to ensure the driver matches this tuner. In theory
      // this should enable multiple tuners to operate in special modes
      // simultaneously.
      string originalListTunerName = null;
      string originalTunerName = _mainTunerDevice.Name;
      string fakeUniqueTunerName = "MediaPortal FM Tuner " + TunerId;
      List<RegistryView> views = new List<RegistryView>() { RegistryView.Default };
      if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
      {
        views.Add(RegistryView.Registry64);
      }
      foreach (RegistryView view in views)
      {
        using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).CreateSubKey(@"SYSTEM\CurrentControlSet\Services\RTL2832UBDA"))
        {
          try
          {
            if (string.IsNullOrEmpty(originalListTunerName))
            {
              originalListTunerName = (string)key.GetValue("FilterName1");
            }
            key.SetValue("FilterName1", fakeUniqueTunerName);
          }
          finally
          {
            key.Close();
          }
        }
      }

      try
      {
        _mainTunerDevice.SetPropBagValue("FriendlyName", fakeUniqueTunerName);

        try
        {
          return FilterGraphTools.AddFilterFromRegisteredClsid(Graph, SOURCE_FILTER_CLSID, "RTL283x FM Source");
        }
        finally
        {
          _mainTunerDevice.SetPropBagValue("FriendlyName", originalTunerName);
        }
      }

      // After loading the source filter we can revert the registry changes.
      finally
      {
        foreach (RegistryView view in views)
        {
          using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).CreateSubKey(@"SYSTEM\CurrentControlSet\Services\RTL2832UBDA"))
          {
            try
            {
              key.SetValue("FilterName1", originalListTunerName);
            }
            finally
            {
              key.Close();
            }
          }
        }
      }
    }

    #region radio data system (RDS)

    private void EnableRds()
    {
      this.LogDebug("RTL283x FM: enable RDS");

      // We'll be receiving RDS via samples from the filter output pin. This
      // may only be applicable for reception of RDS via the interface, but
      // just in case...
      Rtl283xFmResult result = _fmSource.StartRDS();
      if (result != Rtl283xFmResult.Success)
      {
        this.LogWarn("RTL283x FM: failed to start RDS processing, result = {0}", result);
        return;
      }
      _isRdsEnabled = true;

      // Apply highest possible error correction and threshold to try to
      // minimise reception of spurious data.
      Rtl283xFmRdsSignalControlProperty properties = Rtl283xFmRdsSignalControlProperty.CyclicRedundancyChecking;
      properties |= Rtl283xFmRdsSignalControlProperty.DecoderActivationSignalQualityThreshold1;
      properties |= Rtl283xFmRdsSignalControlProperty.DecoderActivationSignalQualityThreshold2;
      result = _fmSource.SetRDSCtr(properties);
      if (result != Rtl283xFmResult.Success)
      {
        this.LogWarn("RTL283x FM: failed to set RDS control properties, result = {0}", result);
      }
    }

    private void DisableRds()
    {
      if (!_isRdsEnabled)
      {
        return;
      }
      this.LogDebug("RTL283x FM: disable RDS");

      Rtl283xFmResult result = _fmSource.StopRDS();
      if (result != Rtl283xFmResult.Success)
      {
        this.LogWarn("RTL283x FM: failed to stop RDS processing, result = {0}", result);
      }

      _isRdsEnabled = false;
    }

    #endregion

    #region graph thread

    /// <summary>
    /// Do something that requires interaction with the graph or tuner.
    /// </summary>
    private object InvokeGraphJob(GraphJobType jobType, ref object[] parameters, string methodName = null)
    {
      lock (_graphThreadLock)
      {
        // Kill the existing thread if it is in "zombie" state.
        if (_graphThread != null && !_graphThread.IsAlive)
        {
          StopGraphThread();
        }

        if (_graphThread == null)
        {
          this.LogDebug("RTL283x FM: starting new graph thread");
          lock (_jobQueueLock)
          {
            _jobs.Clear();
          }
          _stopGraphThread = false;
          _graphThreadWaitEvent = new AutoResetEvent(false);
          _graphThread = new Thread(GraphThread);
          _graphThread.Name = "RTL283x FM graph";
          _graphThread.SetApartmentState(ApartmentState.STA);
          _graphThread.IsBackground = false;
          _graphThread.Start();
        }

        GraphJob job = new GraphJob();
        job.JobType = jobType;
        job.MethodName = methodName;
        job.Parameters = parameters;
        job.WaitEvent = new ManualResetEvent(false);

        lock (_jobQueueLock)
        {
          _jobs.Enqueue(job);
        }
        _graphThreadWaitEvent.Set();

        // Wait for the job to complete. The time limit is arbitrary. Normally
        // we'd expect a job to complete within a few seconds at most. Mainly
        // we just have to be careful to avoid causing deadlock.
        if (!job.WaitEvent.WaitOne(60000))
        {
          throw new TvException("RTL283x job failed to complete within a reasonable time, job type = {0}, method name = {1}.", jobType, methodName ?? string.Empty);
        }
        job.WaitEvent.Close();
        if (job.ThrownException != null)
        {
          throw job.ThrownException;
        }
        return job.ReturnValue;
      }
    }

    /// <summary>
    /// Stop the thread that is used to interact with the graph.
    /// </summary>
    private void StopGraphThread()
    {
      lock (_graphThreadLock)
      {
        if (_graphThread != null)
        {
          if (!_graphThread.IsAlive)
          {
            this.LogWarn("RTL283x FM: aborting old graph thread");
            _graphThread.Abort();
          }
          else
          {
            _stopGraphThread = true;
            _graphThreadWaitEvent.Set();
            if (!_graphThread.Join(100))
            {
              this.LogWarn("RTL283x FM: failed to join graph thread, aborting thread");
              _graphThread.Abort();
            }
          }
          _graphThread = null;
          if (_graphThreadWaitEvent != null)
          {
            _graphThreadWaitEvent.Close();
            _graphThreadWaitEvent = null;
          }
        }
      }
    }

    private void GraphThread()
    {
      try
      {
        while (_graphThreadWaitEvent.WaitOne())
        {
          if (_stopGraphThread)
          {
            return;
          }

          lock (_jobQueueLock)
          {
            while (_jobs.Count > 0)
            {
              GraphJob job = _jobs.Dequeue();
              try
              {
                switch (job.JobType)
                {
                  case GraphJobType.Load:
                    job.ReturnValue = InternalPerformLoading();
                    break;
                  case GraphJobType.Tune:
                    InternalPerformTuning(job.Parameters[0] as IChannel);
                    break;
                  case GraphJobType.GetSignalStatus:
                    bool isLocked;
                    bool isPresent;
                    int strength;
                    int quality;
                    InternalGetSignalStatus(out isLocked, out isPresent, out strength, out quality, (bool)job.Parameters[4]);
                    job.Parameters[0] = isLocked;
                    job.Parameters[1] = isPresent;
                    job.Parameters[2] = strength;
                    job.Parameters[3] = quality;
                    break;
                  case GraphJobType.SetTunerState:
                    InternalPerformSetTunerState((TunerState)job.Parameters[0], (bool)job.Parameters[1]);
                    break;
                  case GraphJobType.Unload:
                    InternalPerformUnloading((bool)job.Parameters[0]);
                    break;
                  case GraphJobType.TsWriterMethodITsWriter:
                    job.ReturnValue = InternalInvokeTsWriterMethod(typeof(ITsWriter), job.MethodName, ref job.Parameters);
                    break;
                  case GraphJobType.TsWriterMethodIGrabberSiDvb:
                    job.ReturnValue = InternalInvokeTsWriterMethod(typeof(IGrabberSiDvb), job.MethodName, ref job.Parameters);
                    break;
                  case GraphJobType.TsWriterMethodIGrabberSiMpeg:
                    job.ReturnValue = InternalInvokeTsWriterMethod(typeof(IGrabberSiMpeg), job.MethodName, ref job.Parameters);
                    break;
                }
              }
              catch (ThreadAbortException ex)
              {
                job.ThrownException = ex;
                throw;
              }
              catch (Exception ex)
              {
                job.ThrownException = ex;
              }
              finally
              {
                job.WaitEvent.Set();
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RTL283x FM: graph thread exception");
      }
      finally
      {
        this.LogDebug("RTL283x FM: graph thread stopped");
      }
    }

    #endregion

    #region TsWriter wrapping

    private object InvokeTsWriterITsWriterJob(string methodName, ref object[] parameters)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        return InternalInvokeTsWriterMethod(typeof(ITsWriter), methodName, ref parameters);
      }
      return InvokeGraphJob(GraphJobType.TsWriterMethodITsWriter, ref parameters, methodName);
    }

    private object InvokeTsWriterIGrabberSiDvbJob(string methodName, ref object[] parameters)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        return InternalInvokeTsWriterMethod(typeof(IGrabberSiDvb), methodName, ref parameters);
      }
      return InvokeGraphJob(GraphJobType.TsWriterMethodIGrabberSiDvb, ref parameters, methodName);
    }

    private object InvokeTsWriterIGrabberSiMpegJob(string methodName, ref object[] parameters)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        return InternalInvokeTsWriterMethod(typeof(IGrabberSiMpeg), methodName, ref parameters);
      }
      return InvokeGraphJob(GraphJobType.TsWriterMethodIGrabberSiMpeg, ref parameters, methodName);
    }

    private object InternalInvokeTsWriterMethod(Type type, string methodName, ref object[] parameters)
    {
      return type.GetMethod(methodName).Invoke(TsWriter, parameters);
    }

    #endregion

    #region ITunerInternal members

    // Any method that interacts with the tuner or graph has normal and
    // internal/wrapped implementations.

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(TVDatabase.Entities.Tuner configuration)
    {
      this.LogDebug("RTL283x FM: reload configuration");
      base.ReloadConfiguration(configuration);

      if (configuration != null && configuration.AnalogTunerSettings == null)
      {
        AnalogTunerSettings settings = new TVDatabase.Entities.AnalogTunerSettings();
        settings.IdAnalogTunerSettings = TunerId;
        settings.IdVideoEncoder = null;
        settings.IdAudioEncoder = null;
        settings.EncoderBitRateModeTimeShifting = (int)EncodeMode.ConstantBitRate;
        settings.EncoderBitRateTimeShifting = 100;
        settings.EncoderBitRatePeakTimeShifting = 100;
        settings.EncoderBitRateModeRecording = (int)EncodeMode.ConstantBitRate;
        settings.EncoderBitRateRecording = 100;
        settings.EncoderBitRatePeakRecording = 100;
        settings.ExternalTunerProgram = string.Empty;
        settings.ExternalTunerProgramArguments = string.Empty;
        settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.None;
        settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.Tuner;
        settings.SupportedVideoSources = (int)CaptureSourceVideo.None;
        settings.SupportedAudioSources = (int)CaptureSourceAudio.Tuner;
        configuration.AnalogTunerSettings = AnalogTunerSettingsManagement.SaveAnalogTunerSettings(settings);
      }
      if (_encoder != null)
      {
        _encoder.ReloadConfiguration(configuration);
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        return InternalPerformLoading();
      }
      else
      {
        object[] p = new object[1] { streamFormat };
        return (IList<ITunerExtension>)InvokeGraphJob(GraphJobType.Load, ref p);
      }
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    private IList<ITunerExtension> InternalPerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      this.LogDebug("RTL283x FM: perform loading");

      if (!DevicesInUse.Instance.Add(_mainTunerDevice))
      {
        throw new TvException("Tuner is in use.");
      }
      _mainTunerDeviceInUse = true;

      InitialiseGraph();

      IBaseFilter sourceFilter = LoadSourceFilter();
      _capture.PerformLoading(sourceFilter);
      _encoder.PerformLoading(Graph, null, _capture);

      // Check for and load extensions, adding any additional filters to the graph.
      IBaseFilter lastFilter = _encoder.TsMultiplexerFilter;
      IList<ITunerExtension> extensions = LoadExtensions(sourceFilter, ref lastFilter);
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Analog;
      }
      AddAndConnectTsWriterIntoGraph(lastFilter, streamFormat);
      CompleteGraph();

      _fmSource = sourceFilter as IRtl283xFmSource;
      _staTsWriter = new TsWriterWrapper(InvokeTsWriterITsWriterJob, InvokeTsWriterIGrabberSiDvbJob, InvokeTsWriterIGrabberSiMpegJob);

      Rtl283xFmSignalControlProperty properties;
      Rtl283xFmResult result = _fmSource.GetSignalQualityCtr(out properties);
      if (result == Rtl283xFmResult.Success)
      {
        this.LogDebug("RTL283x FM: signal control properties = {0}", properties);
      }
      else
      {
        this.LogWarn("RTL283x FM: failed to get signal control properties, result = {0}", result);
      }

      EnableRds();

      _subChannelManager = new SubChannelManagerAnalog(_staTsWriter);
      _channelScanner = new ChannelScannerAnalog(this, _staTsWriter as IGrabberSiMpeg, _staTsWriter as IGrabberSiDvb);
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        InternalPerformSetTunerState(state, isFinalising);
      }
      else if (!isFinalising)
      {
        object[] p = new object[2] { state, isFinalising };
        InvokeGraphJob(GraphJobType.SetTunerState, ref p);
      }
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    private void InternalPerformSetTunerState(TunerState state, bool isFinalising)
    {
      base.PerformSetTunerState(state, isFinalising);
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        InternalPerformUnloading(isFinalising);
      }
      else if (!isFinalising)
      {
        object[] p = new object[1] { isFinalising };
        InvokeGraphJob(GraphJobType.Unload, ref p);
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    private void InternalPerformUnloading(bool isFinalising)
    {
      this.LogDebug("RTL283x FM: perform unloading");

      if (!isFinalising)
      {
        // This function is called from inside the graph thread, which means we
        // can't force the thread to stop from here. Setting the stop variable
        // and event should be enough to cause the thread to stop immediately
        // after this function finishes executing.
        _stopGraphThread = true;
        _graphThreadWaitEvent.Set();

        DisableRds();

        _fmSource = null;
        _capture.PerformUnloading(Graph);
        _encoder.PerformUnloading(Graph);

        _subChannelManager = null;
        _channelScanner = null;
        _staTsWriter = null;
        RemoveTsWriterFromGraph();

        // Only remove the main tuner device from use when we registered it.
        if (_mainTunerDeviceInUse)
        {
          DevicesInUse.Instance.Remove(_mainTunerDevice);
          _mainTunerDeviceInUse = false;
          // Do NOT Dispose() or set the main tuner device to NULL. We would be
          // unable to reload.
        }
      }

      CleanUpGraph(isFinalising);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        InternalPerformTuning(channel);
      }
      else
      {
        object[] p = new object[1] { channel };
        InvokeGraphJob(GraphJobType.Tune, ref p);
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    private void InternalPerformTuning(IChannel channel)
    {
      this.LogDebug("RTL283x FM: perform tuning");
      ChannelFmRadio fmRadioChannel = channel as ChannelFmRadio;
      if (fmRadioChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }
      Rtl283xFmResult result = _fmSource.SetFrequency(fmRadioChannel.Frequency);
      if (result != Rtl283xFmResult.Success)
      {
        throw new TvException("Failed to set frequency. Result = {0}.", result);
      }
      _encoder.PerformTuning(fmRadioChannel);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      {
        InternalGetSignalStatus(out isLocked, out isPresent, out strength, out quality, onlyGetLock);
      }
      else
      {
        object[] p = new object[5] { false, false, 0, 0, onlyGetLock };
        InvokeGraphJob(GraphJobType.GetSignalStatus, ref p);
        isLocked = (bool)p[0];
        isPresent = (bool)p[1];
        strength = (int)p[2];
        quality = (int)p[3];
      }
    }

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    private void InternalGetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      if (_fmSource == null)
      {
        return;
      }

      Rtl283xFmResult result = _fmSource.GetSignalLock(out isLocked);
      if (result != Rtl283xFmResult.Success)
      {
        this.LogWarn("RTL283x FM: failed to update signal lock status, result = {0}", result);
      }
      if (onlyGetLock)
      {
        return;
      }
      isPresent = isLocked;

      result = _fmSource.GetSignalQuality(out quality);
      if (result != Rtl283xFmResult.Success)
      {
        this.LogWarn("RTL283x FM: failed to update signal quality, result = {0}", result);
      }
      strength = quality;
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's sub-channel manager.
    /// </summary>
    public override ISubChannelManager SubChannelManager
    {
      get
      {
        return _subChannelManager;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the tuner is being disposed.</param>
    protected override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      if (isDisposing && _mainTunerDevice != null)
      {
        _mainTunerDevice.Dispose();
        _mainTunerDevice = null;
      }
    }

    #endregion
  }
}