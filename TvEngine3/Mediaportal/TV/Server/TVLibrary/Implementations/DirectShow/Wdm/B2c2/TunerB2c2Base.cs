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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using B2c2PidFilterMode = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum.PidFilterMode;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat tuners
  /// with B2C2 chipsets and WDM drivers.
  /// </summary>
  internal abstract class TunerB2c2Base : TunerDirectShowBase, IMpeg2PidFilter, IRemoteControlListener
  {
    #region constants

    private static readonly int TUNER_CAPABILITIES_SIZE = Marshal.SizeOf(typeof(TunerCapabilities));  // 56
    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    private IBaseFilter _filterB2c2Adapter = null;

    /// <summary>
    /// The data/streaming control interface.
    /// </summary>
    protected IMpeg2DataCtrl6 _interfaceData = null;
    /// <summary>
    /// The tuning control interface.
    /// </summary>
    protected IMpeg2TunerCtrl4 _interfaceTuner = null;

    /// <summary>
    /// A lock used to avoid simultaneous tuner interaction.
    /// </summary>
    protected static object _tunerAccessLock = new object();

    /// <summary>
    /// An infinite tee filter, necessary because downstream filters assume
    /// they should connect to the first output pin (the B2C2 source filter
    /// streams from the second output pin).
    /// </summary>
    private IBaseFilter _filterInfiniteTee = null;

    /// <summary>
    /// B2C2-specific tuner identify information.
    /// </summary>
    protected DeviceInfo _deviceInfo;
    /// <summary>
    /// B2C2-specific tuner hardware capability information.
    /// </summary>
    protected TunerCapabilities _capabilities;

    // PID filter variables - especially important for DVB-S2 tuners.
    private int _maxPidCount = 0;
    private HashSet<int> _pidFilterPids = new HashSet<int>();
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();
    private bool _isPidFilterDisabled = true;

    // remote control variables
    private bool _isRemoteControlInterfaceOpen = false;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Base"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    public TunerB2c2Base(DeviceInfo info, BroadcastStandard supportedBroadcastStandards)
      : base(info.ProductName, "B2C2 tuner " + info.DeviceId, null, null, supportedBroadcastStandards)
    {
      _deviceInfo = info;
    }

    ~TunerB2c2Base()
    {
      Dispose(false);
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      int hr = _interfaceTuner.SetTunerStatusEx(1);
      if (hr != (int)Error.NotLockedOnSignal)
      {
        TvExceptionDirectShowError.Throw(hr, "Failed to apply tuning parameters.");
      }
    }

    /// <summary>
    /// Attempt to read the tuner information.
    /// </summary>
    private void ReadTunerInfo()
    {
      this.LogDebug("B2C2 base: read tuner information");

      int returnedByteCount = TUNER_CAPABILITIES_SIZE;
      int hr = (int)NativeMethods.HResult.S_OK;
      lock (_tunerAccessLock)
      {
        hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("B2C2 base: failed to select device to read tuner information, hr = 0x{0:x}", hr);
          return;
        }

        hr = _interfaceTuner.GetTunerCapabilities(out _capabilities, out returnedByteCount);
      }
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != TUNER_CAPABILITIES_SIZE)
      {
        this.LogWarn("B2C2 base: failed to get tuner capabilities, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      this.LogDebug("  tuner type                = {0}", _capabilities.TunerType);
      this.LogDebug("  set constellation?        = {0}", _capabilities.ConstellationSupported);
      this.LogDebug("  set FEC rate?             = {0}", _capabilities.FecSupported);
      this.LogDebug("  min transponder frequency = {0} kHz", _capabilities.MinTransponderFrequency);
      this.LogDebug("  max transponder frequency = {0} kHz", _capabilities.MaxTransponderFrequency);
      this.LogDebug("  min tuner frequency       = {0} kHz", _capabilities.MinTunerFrequency);
      this.LogDebug("  max tuner frequency       = {0} kHz", _capabilities.MaxTunerFrequency);
      this.LogDebug("  min symbol rate           = {0} baud", _capabilities.MinSymbolRate);
      this.LogDebug("  max symbol rate           = {0} baud", _capabilities.MaxSymbolRate);
      this.LogDebug("  performance monitoring    = {0}", _capabilities.PerformanceMonitoringCapabilities);
      this.LogDebug("  lock time                 = {0} ms", _capabilities.LockTime);
      this.LogDebug("  kernel lock time          = {0} ms", _capabilities.KernelLockTime);
      this.LogDebug("  acquisition capabilities  = {0}", _capabilities.AcquisitionCapabilities);
    }

    private void ReadPidFilterInfo()
    {
      this.LogDebug("B2C2 base: read PID filter information");

      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("B2C2 base: failed to select device to read PID filter information, hr = 0x{0:x}", hr);
        return;
      }

      int count = 0;
      hr = _interfaceData.GetMaxGlobalPIDCount(out count);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("  global max PID count = {0}", count);
      }
      else
      {
        this.LogWarn("B2C2 base: failed to get max global PID count, hr = 0x{0:x}", hr);
      }
      hr = _interfaceData.GetMaxIpPIDCount(out count);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("  IP max PID count     = {0}", count);
      }
      else
      {
        this.LogWarn("B2C2 base: failed to get max IP PID count, hr = 0x{0:x}", hr);
      }
      hr = _interfaceData.GetMaxPIDCount(out count);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("  TS max PID count     = {0}", count);
        _maxPidCount = count;
      }
      else
      {
        this.LogWarn("B2C2 base: failed to get max PID count, hr = 0x{0:x}", hr);
      }

      int openPidCount;
      int runningPidCount;
      int totalPidCount = 0;
      int[] currentPids = new int[_maxPidCount];
      hr = _interfaceData.GetTsState(out openPidCount, out runningPidCount, ref totalPidCount, ref currentPids);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogWarn("B2C2 base: failed to get transport stream state, hr = 0x{0:x}", hr);
      }
      else
      {
        this.LogDebug("  open count           = {0}", openPidCount);
        this.LogDebug("  running count        = {0}", runningPidCount);
        this.LogDebug("  returned count       = {0}", totalPidCount);
        _pidFilterPids.Clear();
        _pidFilterPids.UnionWith(currentPids);
      }
    }

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isRemoteControlInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_remoteControlListenerThread != null && !_remoteControlListenerThread.IsAlive)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlListenerThread == null)
      {
        this.LogDebug("B2C2 base: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "B2C2 remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("B2C2 base: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("B2C2 base: failed to join remote control listener thread, aborting thread");
            _remoteControlListenerThread.Abort();
          }
        }
        _remoteControlListenerThread = null;
        if (_remoteControlListenerThreadStopEvent != null)
        {
          _remoteControlListenerThreadStopEvent.Close();
          _remoteControlListenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("B2C2 base: remote control listener thread start polling");
      int hr;

      IMpeg2AvCtrl3 interfaceAudioVideo = _filterB2c2Adapter as IMpeg2AvCtrl3;
      long codes;
      int codeCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          lock (_tunerAccessLock)
          {
            hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogError("B2C2 base: failed to select device to update signal status, hr = 0x{0:x}", hr);
              return;
            }
            codeCount = 1;
            hr = interfaceAudioVideo.GetIRData(out codes, ref codeCount);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogError("B2C2 base: failed to read remote code, hr = 0x{0:x}", hr);
            }
            else if (codeCount == 1)
            {
              this.LogDebug("B2C2 base: remote control key press, code = {0}", codes);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "B2C2 base: remote control listener thread exception");
        return;
      }
      this.LogDebug("B2C2 base: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerInternal members

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading()
    {
      this.LogDebug("B2C2 base: perform loading");
      InitialiseGraph();

      // Create, add and initialise the B2C2 source filter.
      _filterB2c2Adapter = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, Constants.B2C2_ADAPTER_CLSID, "B2C2 Source");
      _interfaceData = _filterB2c2Adapter as IMpeg2DataCtrl6;
      _interfaceTuner = _filterB2c2Adapter as IMpeg2TunerCtrl4;
      if (_interfaceTuner == null || _interfaceData == null)
      {
        throw new TvException("Failed to find interfaces on source filter.");
      }
      int hr = _interfaceTuner.Initialize();
      TvExceptionDirectShowError.Throw(hr, "Failed to initialise tuner interface.");

      // The source filter has multiple output pins, and connecting to the right one is critical.
      // Extensions can't handle this automatically, so we add an extra infinite tee in between the
      // source filter and any extension filters.
      _filterInfiniteTee = (IBaseFilter)new InfTee();
      FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterInfiniteTee, "Infinite Tee", _filterB2c2Adapter, 2, 0);

      // Load and open extensions.
      IBaseFilter lastFilter = _filterInfiniteTee;
      IList<ITunerExtension> extensions = LoadExtensions(_deviceInfo, ref lastFilter);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      ReadTunerInfo();
      ReadPidFilterInfo();
      if (_pidFilterPids.Count == 1 && _pidFilterPids.Contains((int)B2c2PidFilterMode.AllExcludingNull))
      {
        _isPidFilterDisabled = true;
      }
      else
      {
        _isPidFilterDisabled = false;   // make sure the next call actually disables the filter
        (this as IMpeg2PidFilter).Disable();
      }
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("B2C2 base: perform unloading");

      if (!isFinalising)
      {
        _interfaceData = null;
        _interfaceTuner = null;

        if (_graph != null)
        {
          _graph.RemoveFilter(_filterInfiniteTee);
          _graph.RemoveFilter(_filterB2c2Adapter);
        }
        Release.ComObject("B2C2 infinite tee", ref _filterInfiniteTee);
        Release.ComObject("B2C2 source filter", ref _filterB2c2Adapter);
      }

      CleanUpGraph(isFinalising);
    }

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
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      if (_interfaceTuner == null)
      {
        return;
      }

      lock (_tunerAccessLock)
      {
        int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("B2C2 base: failed to select device to update signal status, hr = 0x{0:x}", hr);
          return;
        }
        isLocked = (_interfaceTuner.CheckLock() == 0);
        isPresent = isLocked;
        if (!onlyGetLock)
        {
          _interfaceTuner.GetSignalStrength(out strength);
          _interfaceTuner.GetSignalQuality(out quality);
        }
      }
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    byte ITunerExtension.Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    public bool ControlsTunerHardware
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITuner tuner, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before
    /// the tuner is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITuner tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITuner tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITuner tuner, ref TunerAction action)
    {
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      if (_deviceInfo.BusInterface == BusType.Usb1)
      {
        return true;
      }
      ChannelDvbC dvbcTuningDetail = tuningDetail as ChannelDvbC;
      IChannelSatellite satelliteTuningDetail = tuningDetail as IChannelSatellite;
      if (dvbcTuningDetail == null && satelliteTuningDetail == null)
      {
        return false;
      }

      // It is not ideal to have to enable PID filtering because doing so can
      // limit the number of channels that can be viewed/recorded
      // simultaneously. However, it does seem that there is a need for
      // filtering on satellite and cable transmitters with high bit rates.
      int bitRate = 0;
      if (satelliteTuningDetail != null)
      {
        int bitsPerSymbol = 2;  // QPSK
        switch (satelliteTuningDetail.ModulationScheme)
        {
          case ModulationSchemePsk.Psk2:
          case ModulationSchemePsk.Psk4SplitI:
          case ModulationSchemePsk.Psk4SplitQ:
            bitsPerSymbol = 1;
            break;
          case ModulationSchemePsk.Psk4:
          case ModulationSchemePsk.Psk4Offset:
            bitsPerSymbol = 2;
            break;
          case ModulationSchemePsk.Psk8:
            bitsPerSymbol = 3;
            break;

          // Not supported by the hardware.
          case ModulationSchemePsk.Psk16:
            bitsPerSymbol = 4;
            break;
          case ModulationSchemePsk.Psk32:
            bitsPerSymbol = 5;
            break;
          case ModulationSchemePsk.Psk64:
            bitsPerSymbol = 6;
            break;
          case ModulationSchemePsk.Psk128:
            bitsPerSymbol = 7;
            break;
          case ModulationSchemePsk.Psk256:
            bitsPerSymbol = 8;
            break;
        }

        // Other FEC code rates not supported by the hardware.
        bitRate = bitsPerSymbol * satelliteTuningDetail.SymbolRate; // kb/s
        switch (satelliteTuningDetail.FecCodeRate)
        {
          case FecCodeRate.Rate1_2:
            bitRate /= 2;
            break;
          case FecCodeRate.Rate1_3:
            bitRate /= 3;
            break;
          case FecCodeRate.Rate1_4:
            bitRate /= 4;
            break;
          case FecCodeRate.Rate2_3:
            bitRate = bitRate * 2 / 3;
            break;
          case FecCodeRate.Rate2_5:
            bitRate = bitRate * 2 / 5;
            break;
          case FecCodeRate.Rate3_4:
            bitRate = bitRate * 3 / 4;
            break;
          case FecCodeRate.Rate3_5:
            bitRate = bitRate * 3 / 5;
            break;
          case FecCodeRate.Rate4_5:
            bitRate = bitRate * 4 / 5;
            break;
          case FecCodeRate.Rate5_11:
            bitRate = bitRate * 5 / 11;
            break;
          case FecCodeRate.Rate5_6:
            bitRate = bitRate * 5 / 6;
            break;
          case FecCodeRate.Rate6_7:
            bitRate = bitRate * 6 / 7;
            break;
          case FecCodeRate.Rate7_8:
            bitRate = bitRate * 7 / 8;
            break;
          case FecCodeRate.Rate8_9:
            bitRate = bitRate * 8 / 9;
            break;
          case FecCodeRate.Rate9_10:
            bitRate = bitRate * 9 / 10;
            break;
        }
      }
      else if (dvbcTuningDetail != null)
      {
        int bitsPerSymbol = 6;  // 64 QAM
        switch (dvbcTuningDetail.ModulationScheme)
        {
          case ModulationSchemeQam.Qam16:
            bitsPerSymbol = 4;
            break;
          case ModulationSchemeQam.Qam32:
            bitsPerSymbol = 5;
            break;
          case ModulationSchemeQam.Qam64:
            bitsPerSymbol = 6;
            break;
          case ModulationSchemeQam.Qam128:
            bitsPerSymbol = 7;
            break;
          case ModulationSchemeQam.Qam256:
            bitsPerSymbol = 8;
            break;

          // Not supported by the hardware.
          case ModulationSchemeQam.Qam512:
            bitsPerSymbol = 9;
            break;
          case ModulationSchemeQam.Qam1024:
            bitsPerSymbol = 10;
            break;
          case ModulationSchemeQam.Qam2048:
            bitsPerSymbol = 11;
            break;
          case ModulationSchemeQam.Qam4096:
            bitsPerSymbol = 12;
            break;
        }
        bitRate = bitsPerSymbol * dvbcTuningDetail.SymbolRate;  // kb/s
      }

      // Rough approximation: enable PID filtering when bit rate is over 40 Mb/s.
      bool enableFilter = (bitRate >= 40000);
      this.LogDebug("B2C2 base: transport stream bit rate = {0} kb/s, need PID filter = {1}", bitRate, enableFilter);
      return enableFilter;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      if (_isPidFilterDisabled)
      {
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogDebug("B2C2 base: disable PID filter");
      int hr = (int)NativeMethods.HResult.S_OK;
      lock (_tunerAccessLock)
      {
        hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("B2C2 base: failed to select device to disable PID filter, hr = 0x{0:x}", hr);
          return false;
        }

        // Remove all current PIDs.
        if (_pidFilterPids.Count > 0)
        {
          this.LogDebug("  delete {0} current PID(s)...", _pidFilterPids.Count);
          int[] currentPids = new int[_pidFilterPids.Count];
          _pidFilterPids.CopyTo(currentPids, 0, _pidFilterPids.Count);
          hr = _interfaceData.DeletePIDsFromPin(_pidFilterPids.Count, currentPids, 0);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("B2C2 base: failed to delete current PIDs, hr = 0x{0:x}", hr);
            return false;
          }
          _pidFilterPids.Clear();
        }

        // Allow all PIDs.
        this.LogDebug("  add all excluding NULL...");
        int changingPidCount = 1;
        hr = _interfaceData.AddPIDsToPin(ref changingPidCount, new int[1] { (int)B2c2PidFilterMode.AllExcludingNull }, 0);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        _isPidFilterDisabled = true;
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        this.LogDebug("B2C2 base: result = success");
        return true;
      }

      this.LogError("B2C2 base: failed to add all PIDs, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return _maxPidCount;
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      if (_pidFilterPidsToAdd.Count == 0 && _pidFilterPidsToRemove.Count == 0)
      {
        return true;
      }

      this.LogDebug("B2C2 base: apply PID filter configuration");
      lock (_tunerAccessLock)
      {
        int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("B2C2 base: failed to select device to apply PID filter configuration, hr = 0x{0:x}", hr);
          return false;
        }

        if (_isPidFilterDisabled)
        {
          this.LogDebug("  delete all excluding NULL...");
          hr = _interfaceData.DeletePIDsFromPin(1, new int[1] { (int)B2c2PidFilterMode.AllExcludingNull }, 0);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("B2C2 base: failed to delete all PIDs, hr = 0x{0:x}", hr);
            return false;
          }
          _isPidFilterDisabled = false;
        }

        int changingPidCount = 0;
        if (_pidFilterPidsToRemove.Count > 0)
        {
          changingPidCount = _pidFilterPidsToRemove.Count;
          this.LogDebug("  delete {0} current PID(s)...", changingPidCount);
          int[] newPids = new int[changingPidCount];
          int i = 0;
          foreach (ushort pid in _pidFilterPidsToRemove)
          {
            newPids[i++] = pid;
          }
          hr = _interfaceData.DeletePIDsFromPin(changingPidCount, newPids, 0);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("B2C2 base: failed to delete current PID(s), hr = 0x{0:x}", hr);
            return false;
          }
          _pidFilterPids.ExceptWith(newPids);
          _pidFilterPidsToRemove.Clear();
        }

        if (_pidFilterPidsToAdd.Count > 0)
        {
          changingPidCount = _pidFilterPidsToAdd.Count;
          this.LogDebug("  add {0} new PID(s)...", changingPidCount);
          int[] newPids = new int[changingPidCount];
          int i = 0;
          foreach (ushort pid in _pidFilterPidsToAdd)
          {
            newPids[i++] = pid;
          }
          hr = _interfaceData.AddPIDsToPin(ref changingPidCount, newPids, 0);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("B2C2 base: failed to add new PID(s), hr = 0x{0:x}", hr);
            return false;
          }
          _pidFilterPids.UnionWith(newPids);
          _pidFilterPidsToAdd.Clear();
        }
      }

      this.LogDebug("B2C2 base: result = success");
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("B2C2 base: open remote control interface");

      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("B2C2 base: remote control interface is already open");
        return true;
      }
      if (!_capabilities.AcquisitionCapabilities.HasFlag(AcquisitionCapability.IrInput))
      {
        this.LogDebug("B2C2 base: remote control capability not supported");
        return false;
      }
      if (!(_filterB2c2Adapter is IMpeg2AvCtrl3))
      {
        this.LogWarn("B2C2 base: remote control capability advertised but interface not supported");
        return false;
      }

      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();
      this.LogDebug("B2C2 base: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Close()
    {
      return CloseRemoteControlListenerInterface(true);
    }

    private bool CloseRemoteControlListenerInterface(bool isDisposing)
    {
      this.LogDebug("B2C2 base: close remote control interface");

      if (isDisposing)
      {
        StopRemoteControlListenerThread();
      }
      _isRemoteControlInterfaceOpen = false;

      this.LogDebug("B2C2 base: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the tuner is being disposed.</param>
    protected override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      CloseRemoteControlListenerInterface(isDisposing);
    }

    #endregion
  }
}