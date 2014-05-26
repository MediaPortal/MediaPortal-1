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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using B2c2PidFilterMode = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum.PidFilterMode;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat tuners
  /// with B2C2 chipsets and WDM drivers.
  /// </summary>
  internal abstract class TunerB2c2Base : TunerDirectShowBase, IMpeg2PidFilter
  {
    #region constants

    private static readonly int TUNER_CAPABILITIES_SIZE = Marshal.SizeOf(typeof(TunerCapabilities));  // 56

    #endregion

    #region variables

    private IBaseFilter _filterB2c2Adapter = null;
    private IMpeg2DataCtrl6 _interfaceData = null;
    /// <summary>
    /// The main tuning control interface.
    /// </summary>
    protected IMpeg2TunerCtrl4 _interfaceTuner = null;

    private IBaseFilter _filterInfiniteTee = null;

    private DeviceInfo _deviceInfo;
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

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Base"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    /// <param name="type">The tuner type.</param>
    public TunerB2c2Base(DeviceInfo info, CardType type)
      : base(info.ProductName, "B2C2 tuner " + info.DeviceId, type)
    {
      _deviceInfo = info;
    }

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    public override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (_interfaceTuner == null)
      {
        _isSignalLocked = false;
        _isSignalPresent = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      _isSignalLocked = (_interfaceTuner.CheckLock() == 0);
      _isSignalPresent = _isSignalLocked;
      if (!onlyUpdateLock)
      {
        _interfaceTuner.GetSignalStrength(out _signalLevel);
        _interfaceTuner.GetSignalQuality(out _signalQuality);
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 base: apply tuning parameters");
      HResult.ThrowException(_interfaceTuner.SetTunerStatus(), "Failed to apply tuning parameters.");
    }

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    public override void PerformLoading()
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
      HResult.ThrowException(hr, "Failed to initialise tuner interface.");

      // This line is a remnant from old code. I don't know if/why it is necessary, but no harm
      // in leaving it...
      _interfaceTuner.CheckLock();

      // The source filter has multiple output pins, and connecting to the right one is critical.
      // Extensions can't handle this automatically, so we add an extra infinite tee in between the
      // source filter and any extension filters.
      _filterInfiniteTee = (IBaseFilter)new InfTee();
      FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterInfiniteTee, "Infinite Tee", _filterB2c2Adapter, 2, 0);

      // Load and open extensions.
      IBaseFilter lastFilter = _filterInfiniteTee;
      LoadExtensions(_filterB2c2Adapter, ref lastFilter);

      // This class implements the extension interface and should be treated as the main extension.
      _extensions.Add(this);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      ReadTunerInfo();
      ReadPidFilterInfo();
      if (_pidFilterPids.Contains((int)B2c2PidFilterMode.AllExcludingNull) || _pidFilterPids.Contains((int)B2c2PidFilterMode.AllIncludingNull))
      {
        _isPidFilterDisabled = false;   // make sure the next call actually disables the filter
        DisableFilter();
      }
      else
      {
        _isPidFilterDisabled = true;
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public override void PerformUnloading()
    {
      this.LogDebug("B2C2 base: perform unloading");
      _interfaceData = null;
      _interfaceTuner = null;

      if (_graph != null)
      {
        _graph.RemoveFilter(_filterInfiniteTee);
        _graph.RemoveFilter(_filterB2c2Adapter);
      }
      Release.ComObject("B2C2 infinite tee", ref _filterInfiniteTee);
      Release.ComObject("B2C2 source filter", ref _filterB2c2Adapter);

      CleanUpGraph();
    }

    #endregion

    /// <summary>
    /// Attempt to read the tuner information.
    /// </summary>
    private void ReadTunerInfo()
    {
      this.LogDebug("B2C2 base: read tuner information");

      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("B2C2 base: failed to select device, hr = 0x{0:x}", hr);
        return;
      }

      IntPtr buffer = Marshal.AllocCoTaskMem(TUNER_CAPABILITIES_SIZE);
      try
      {
        for (int i = 0; i < TUNER_CAPABILITIES_SIZE; i++)
        {
          Marshal.WriteByte(buffer, i, 0);
        }
        int returnedByteCount = TUNER_CAPABILITIES_SIZE;
        hr = _interfaceTuner.GetTunerCapabilities(buffer, ref returnedByteCount);
        if (hr != (int)HResult.Severity.Success || returnedByteCount != TUNER_CAPABILITIES_SIZE)
        {
          this.LogWarn("B2C2 base: failed to get tuner capabilities, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        }
        else
        {
          //Dump.DumpBinary(_generalBuffer, returnedByteCount);
          _capabilities = (TunerCapabilities)Marshal.PtrToStructure(buffer, typeof(TunerCapabilities));
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
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }
    }

    private void ReadPidFilterInfo()
    {
      this.LogDebug("B2C2 base: read PID filter information");

      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("B2C2 base: failed to select device, hr = 0x{0:x}", hr);
        return;
      }

      int count = 0;
      hr = _interfaceData.GetMaxGlobalPIDCount(out count);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  global max PID count = {0}", count);
      }
      else
      {
        this.LogWarn("B2C2 base: failed to get max global PID count, hr = 0x{0:x}", hr);
      }
      hr = _interfaceData.GetMaxIpPIDCount(out count);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  IP max PID count     = {0}", count);
      }
      else
      {
        this.LogWarn("B2C2 base: failed to get max IP PID count, hr = 0x{0:x}", hr);
      }
      hr = _interfaceData.GetMaxPIDCount(out count);
      if (hr == (int)HResult.Severity.Success)
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
      hr = _interfaceData.GetTsState(out openPidCount, out runningPidCount, ref totalPidCount, currentPids);
      if (hr != (int)HResult.Severity.Success)
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

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITVCard tuner, out TunerAction action)
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
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref TunerAction action)
    {
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter member

    /// <summary>
    /// Should the filter be enabled for the current multiplex.
    /// </summary>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    public bool ShouldEnableFilter(IChannel tuningDetail)
    {
      if (_deviceInfo.BusInterface == BusType.Usb1)
      {
        return true;
      }
      if (TunerType != CardType.DvbS && TunerType != CardType.DvbC)
      {
        return false;
      }

      // It is not ideal to have to enable PID filtering because doing so can limit
      // the number of channels that can be viewed/recorded simultaneously. However,
      // it does seem that there is a need for filtering on satellite transponders
      // with high bit rates. Problems have been observed with transponders on Thor
      // 5/6, Intelsat 10-02 (0.8W) if the filter is not enabled:
      //   Symbol Rate: 27500, Modulation: 8 PSK, FEC rate: 5/6, Pilot: On, Roll-Off: 0.35
      //   Symbol Rate: 30000, Modulation: 8 PSK, FEC rate: 3/4, Pilot: On, Roll-Off: 0.35
      int bitRate = 0;
      DVBSChannel satelliteTuningDetail = tuningDetail as DVBSChannel;
      if (satelliteTuningDetail != null)
      {
        int bitsPerSymbol = 2;
        if (satelliteTuningDetail.ModulationType == ModulationType.ModBpsk)
        {
          bitsPerSymbol = 1;
        }
        else if (satelliteTuningDetail.ModulationType == ModulationType.Mod8Psk ||
          satelliteTuningDetail.ModulationType == ModulationType.ModNbc8Psk)
        {
          bitsPerSymbol = 3;
        }
        else if (satelliteTuningDetail.ModulationType == ModulationType.Mod16Apsk)
        {
          bitsPerSymbol = 4;
        }
        else if (satelliteTuningDetail.ModulationType == ModulationType.Mod32Apsk)
        {
          bitsPerSymbol = 5;
        }
        bitRate = bitsPerSymbol * satelliteTuningDetail.SymbolRate; // kb/s
        if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate1_2)
        {
          bitRate /= 2;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate1_3)
        {
          bitRate /= 3;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate1_4)
        {
          bitRate /= 4;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate2_3)
        {
          bitRate = bitRate * 2 / 3;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate2_5)
        {
          bitRate = bitRate * 2 / 5;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate3_4)
        {
          bitRate = bitRate * 3 / 4;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate3_5)
        {
          bitRate = bitRate * 3 / 5;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate4_5)
        {
          bitRate = bitRate * 4 / 5;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate5_6)
        {
          bitRate = bitRate * 5 / 6;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate5_11)
        {
          bitRate = bitRate * 5 / 11;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate6_7)
        {
          bitRate = bitRate * 6 / 7;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate7_8)
        {
          bitRate = bitRate * 7 / 8;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9)
        {
          bitRate = bitRate * 8 / 9;
        }
        else if (satelliteTuningDetail.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
        {
          bitRate = bitRate * 9 / 10;
        }
      }
      else
      {
        DVBCChannel cableTuningDetail = tuningDetail as DVBCChannel;
        double bitsPerSymbol = 6;  // 64 QAM
        if (cableTuningDetail.ModulationType == ModulationType.Mod80Qam)
        {
          bitsPerSymbol = 6.25;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod96Qam)
        {
          bitsPerSymbol = 6.5;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod112Qam)
        {
          bitsPerSymbol = 6.75;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod128Qam)
        {
          bitsPerSymbol = 7;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod160Qam)
        {
          bitsPerSymbol = 7.25;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod192Qam)
        {
          bitsPerSymbol = 7.5;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod224Qam)
        {
          bitsPerSymbol = 7.75;
        }
        else if (cableTuningDetail.ModulationType == ModulationType.Mod256Qam)
        {
          bitsPerSymbol = 8;
        }
        bitRate = (int)bitsPerSymbol * cableTuningDetail.SymbolRate;  // kb/s
      }

      // Rough approximation: enable PID filtering when bit rate is over 40 Mb/s.
      this.LogDebug("B2C2 base: multiplex bit rate = {0} kb/s", bitRate);
      return bitRate >= 60000;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    public bool DisableFilter()
    {
      if (_isPidFilterDisabled)
      {
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogDebug("B2C2 base: disable PID filter");
      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("B2C2 base: failed to select device, hr = 0x{0:x}", hr);
        return false;
      }

      // Remove all current PIDs.
      if (_pidFilterPids.Count > 0)
      {
        this.LogDebug("  delete {0} current PID(s)...", _pidFilterPids.Count);
        int[] currentPids = new int[_pidFilterPids.Count];
        _pidFilterPids.CopyTo(currentPids, 0, _pidFilterPids.Count);
        hr = _interfaceData.DeletePIDsFromPin(_pidFilterPids.Count, currentPids, 0);
        if (hr != (int)HResult.Severity.Success)
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
      if (hr == (int)HResult.Severity.Success)
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
    public int MaximumPidCount
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
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
      return true;
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
      return true;
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    public bool ApplyFilter()
    {
      if (_pidFilterPidsToAdd.Count == 0 && _pidFilterPidsToRemove.Count == 0)
      {
        return true;
      }

      this.LogDebug("B2C2 base: apply PID filter");
      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("B2C2 base: failed to select device, hr = 0x{0:x}", hr);
        return false;
      }

      if (_isPidFilterDisabled)
      {
        this.LogDebug("  delete all excluding NULL...");
        hr = _interfaceData.DeletePIDsFromPin(1, new int[1] { (int)B2c2PidFilterMode.AllExcludingNull }, 0);
        if (hr != (int)HResult.Severity.Success)
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
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("B2C2 base: failed to delete current PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        foreach (ushort pid in _pidFilterPidsToRemove)
        {
          _pidFilterPids.Remove(pid);
        }
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
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("B2C2 base: failed to add new PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        foreach (ushort pid in _pidFilterPidsToAdd)
        {
          _pidFilterPids.Add(pid);
        }
        _pidFilterPidsToAdd.Clear();
      }

      this.LogDebug("B2C2 base: result = success");
      return true;
    }

    #endregion
  }
}