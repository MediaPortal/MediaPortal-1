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
using System.Linq;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Empia
{
  /// <summary>
  /// A class for handling DiSEqC, PID filtering, clear QAM and DVB-C support
  /// for tuners based on eMPIA EM28** chipsets.
  /// </summary>
  public class Empia : BaseTunerExtension, ICustomTuner, IDiseqcDevice, IDisposable, IMpeg2PidFilter, IStreamSelector
  {
    #region enums

    private enum BdaExtensionProperty
    {
      DiseqcSendMessage = 0,
      DiseqcLock,
      DiseqcUnlock,

      QamSetMode = 0x200,
      QamSetFrequency,

      EnablePidFilter = 0x300,

      DvbT2SetPlp = 0x700,
      DvbT2GetPlp
    }

    private enum DiseqcVersion : int
    {
      Undefined = 0,
      Version1,
      Version2
    }

    private enum DiseqcMode : int
    {
      Undefined = 0,
      NoReply,
      QuickReply
    }

    private enum QamMode : int
    {
      Vsb = 0,
      Qam64,
      Qam256
    }

    private enum PlpMode : byte
    {
      Auto = 0,   // First PLP automatically selected.
      Manual
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessageParams  // KSPROPERTY_DISEQC_SEND_MSG_S
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_TX_MESSAGE_LENGTH)]
      public byte[] MessageTransmit;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_RX_MESSAGE_LENGTH)]
      public byte[] MessageReceive;
      public int MessageTransmitLength;
      public int MessageReceiveLength;
      public DiseqcVersion Version;
      public DiseqcMode Mode;
      public int DelayAfterTransmit;    // unit = ms, value -1 = driver default
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsToneBurstModulated;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;
      private uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PidFilterParams  // KSPROPERTY_ENABLE_PID_FILTER_S
    {
      public int PidCount;      // 0 to enable all
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PID_FILTER_PID_COUNT)]
      public ushort[] Pids;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlpSelection   // DVBT2_SET_PLP_S
    {
      public PlpMode Mode;
      public byte PlpId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlpInfo    // DVBT2_GET_PLP_S
    {
      public byte PlpCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PLP_ID_COUNT)]
      public byte[] PlpIdList;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xb88700a5, 0x3a0d, 0x475e, 0x92, 0xa1, 0xf2, 0x01, 0x42, 0x12, 0x55, 0xcc);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.

    private static readonly int DISEQC_MESSAGE_PARAMS_SIZE = Marshal.SizeOf(typeof(DiseqcMessageParams));   // 48
    private static readonly int PID_FILTER_PARAMS_SIZE = Marshal.SizeOf(typeof(PidFilterParams));           // 34
    private static readonly int PLP_SELECTION_SIZE = Marshal.SizeOf(typeof(PlpSelection));                  // 2
    private static readonly int PLP_INFO_SIZE = Marshal.SizeOf(typeof(PlpInfo));                            // 257

    private static readonly int PARAM_BUFFER_SIZE = new int[] { DISEQC_MESSAGE_PARAMS_SIZE, PID_FILTER_PARAMS_SIZE, PLP_SELECTION_SIZE, PLP_INFO_SIZE }.Max();

    private const int MAX_DISEQC_TX_MESSAGE_LENGTH = 8;
    private const int MAX_DISEQC_RX_MESSAGE_LENGTH = 8;
    private const int MAX_PID_FILTER_PID_COUNT = 15;
    private const int MAX_PLP_ID_COUNT = 256;

    #endregion

    #region variables

    private bool _isEmpia = false;
    private bool _releasePropertySet = true;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    private bool _isAtscQamModeSupported = false;
    private bool _isDvbT2PlpSelectionSupported = false;

    private bool _isPidFilterSupported = false;
    private HashSet<ushort> _pidFilterPids = new HashSet<ushort>();

    #endregion

    private IKsPropertySet FindPropertySet(object obj, string objType)
    {
      IKsPropertySet ps = obj as IKsPropertySet;
      if (ps == null)
      {
        this.LogDebug("eMPIA: {0} is not a property set", objType);
        return null;
      }

      // Not sure which property/properties the tuner will support, so check
      // them all.
      int hr = (int)NativeMethods.HResult.E_FAIL;
      KSPropertySupport support = 0;
      foreach (Enum e in Enum.GetValues(typeof(BdaExtensionProperty)))
      {
        hr = ps.QuerySupported(BDA_EXTENSION_PROPERTY_SET, Convert.ToInt32(e), out support);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          return ps;
        }
      }
      this.LogDebug("eMPIA: {0} does not support property set, hr = 0x{1:x}, support = {2}", objType, hr, support);
      return null;
    }

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This is quite a generic interface, so prefer other extensions.
        return 40;
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      if (_isEmpia)
      {
        this.LogWarn("eMPIA: extension already initialised");
        return true;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("eMPIA: context is not a filter");
        return false;
      }

      // Find the property set. We expect to find it on the tuner filter output
      // pin, but we check the tuner filter and input pin as well just in case.
      IPin pin = null;
      try
      {
        _releasePropertySet = true;
        _propertySet = FindPropertySet(filter, "filter");
        if (_propertySet != null)
        {
          _releasePropertySet = false;
          return true;
        }

        pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
        if (pin == null)
        {
          this.LogError("eMPIA: failed to find filter input pin");
          return false;
        }

        _propertySet = FindPropertySet(pin, "input pin");
        if (_propertySet != null)
        {
          return true;
        }

        Release.ComObject("eMPIA filter input pin", ref pin);
        pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
        if (pin == null)
        {
          this.LogError("eMPIA: failed to find filter output pin");
          return false;
        }

        IPin connectedPin;
        int hr = pin.ConnectedTo(out connectedPin);
        if (hr == (int)NativeMethods.HResult.S_OK && connectedPin != null)
        {
          Release.ComObject("eMPIA filter connected pin", ref connectedPin);
          _propertySet = FindPropertySet(pin, "output pin");
          return _propertySet != null;
        }

        // Some drivers will not report whether a property set is supported
        // unless the pin is connected. It is okay when we're checking a tuner
        // filter which has a capture filter connected, but if the tuner filter
        // is also the capture filter then the output pin(s) won't be connected
        // yet.
        FilterInfo filterInfo;
        hr = filter.QueryFilterInfo(out filterInfo);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("eMPIA: failed to get filter info, hr = 0x{0:x}", hr);
          return false;
        }
        IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
        if (graph == null)
        {
          this.LogDebug("eMPIA: filter info graph is null");
          return false;
        }

        // Add an infinite tee.
        IBaseFilter infTee = (IBaseFilter)new InfTee();
        IPin infTeeInputPin = null;
        try
        {
          hr = graph.AddFilter(infTee, "Temp Infinite Tee");
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("eMPIA: failed to add infinite tee to graph, hr = 0x{0:x}", hr);
            return false;
          }

          // Connect the infinite tee to the filter.
          infTeeInputPin = DsFindPin.ByDirection(infTee, PinDirection.Input, 0);
          if (infTeeInputPin == null)
          {
            this.LogError("eMPIA: failed to find the infinite tee input pin, hr = 0x{0:x}", hr);
            return false;
          }
          hr = graph.ConnectDirect(pin, infTeeInputPin, null);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("eMPIA: failed to connect infinite tee, hr = 0x{0:x}", hr);
            return false;
          }

          _propertySet = FindPropertySet(pin, "output pin");
          return _propertySet != null;
        }
        finally
        {
          pin.Disconnect();
          Release.ComObject("eMPIA infinite tee input pin", ref infTeeInputPin);
          graph.RemoveFilter(infTee);
          Release.ComObject("eMPIA infinite tee", ref infTee);
          Release.FilterInfo(ref filterInfo);
          graph = null;
        }
      }
      finally
      {
        if (_propertySet != null)
        {
          this.LogInfo("eMPIA: extension supported");
          _isEmpia = true;
          _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
          _paramBuffer = Marshal.AllocCoTaskMem(PARAM_BUFFER_SIZE);

          KSPropertySupport support;
          int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.EnablePidFilter, out support);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("eMPIA: PID filtering supported");
            _isPidFilterSupported = true;
          }
          hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.QamSetMode, out support);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("eMPIA: ATSC/QAM mode supported");
            _isAtscQamModeSupported = true;
          }
          hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DvbT2SetPlp, out support);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("eMPIA: stream selection supported");
            _isDvbT2PlpSelectionSupported = true;
          }
        }
        else
        {
          Release.ComObject("eMPIA filter output pin", ref pin);
        }
      }
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("eMPIA: on before tune call back");
      action = TunerAction.Default;

      if (!_isEmpia)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return;
      }

      ChannelDvbC dvbcChannel = channel as ChannelDvbC;
      if (dvbcChannel != null)
      {
        // Required by at least PCTV Systems 291/292e "tripleStick", with
        // driver from January 2014 (@device:pnp:\\?\usb#vid_2013&pid_025f).
        // Refer to http://forum.team-mediaportal.com/threads/pctv-292e-dvb-c-problem.128117/
        dvbcChannel.SymbolRate *= 1000;
        this.LogDebug("  symbol rate = {0} s/s", dvbcChannel.SymbolRate);
      }
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
      // Assume it is necessary/desirable to enable the filter.
      return _isPidFilterSupported;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      this.LogDebug("eMPIA: disable PID filter");
      if (!_isEmpia)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }

      _pidFilterPids.Clear();
      int hr = ConfigurePidFilter();
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("eMPIA: result = success");
        return true;
      }

      this.LogError("eMPIA: failed to diable PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return MAX_PID_FILTER_PID_COUNT;
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPids.UnionWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPids.ExceptWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      this.LogDebug("eMPIA: apply PID filter configuration");

      int hr = ConfigurePidFilter();
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("eMPIA: result = success");
        return true;
      }

      this.LogError("eMPIA: failed to apply PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    private int ConfigurePidFilter()
    {
      PidFilterParams filter = new PidFilterParams();
      filter.PidCount = Math.Min(_pidFilterPids.Count, MAX_PID_FILTER_PID_COUNT);
      filter.Pids = new ushort[MAX_PID_FILTER_PID_COUNT];
      if (filter.PidCount > 0)
      {
        _pidFilterPids.CopyTo(filter.Pids, 0, filter.PidCount);
      }
      Marshal.StructureToPtr(filter, _paramBuffer, false);

      //Dump.DumpBinary(_paramBuffer, PID_FILTER_PARAMS_SIZE);

      return _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.EnablePidFilter,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, PID_FILTER_PARAMS_SIZE
      );
    }

    #endregion

    #region IStreamSelector members

    /// <summary>
    /// Get the identifiers for the available streams.
    /// </summary>
    /// <param name="streamIds">The stream identifiers.</param>
    /// <returns><c>true</c> if the stream identifiers are retrieved successfully, otherwise <c>false</c></returns>
    public bool GetAvailableStreamIds(out ICollection<int> streamIds)
    {
      this.LogDebug("eMPIA: get available stream IDs");
      streamIds = null;

      if (!_isEmpia || !_isDvbT2PlpSelectionSupported)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DvbT2GetPlp,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, PLP_INFO_SIZE,
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == PLP_INFO_SIZE)
      {
        this.LogDebug("eMPIA: result = success");
        PlpInfo info = (PlpInfo)Marshal.PtrToStructure(_paramBuffer, typeof(PlpInfo));
        streamIds = new List<int>(info.PlpCount);
        for (int i = 0; i < info.PlpCount; i++)
        {
          streamIds.Add(info.PlpIdList[i]);
        }
        return true;
      }

      this.LogError("eMPIA: failed to get available stream IDs, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    /// <summary>
    /// Select a stream.
    /// </summary>
    /// <param name="streamId">The identifier of the stream to select.</param>
    /// <returns><c>true</c> if the stream is selected successfully, otherwise <c>false</c></returns>
    public bool SelectStream(int streamId)
    {
      this.LogDebug("eMPIA: select stream, stream ID = {0}", streamId);

      if (!_isEmpia || !_isDvbT2PlpSelectionSupported)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }
      if (streamId > 255)
      {
        this.LogError("eMPIA: stream ID is too large, stream ID = {0}", streamId);
        return false;
      }

      PlpSelection command = new PlpSelection();
      if (streamId < 0)
      {
        command.Mode = PlpMode.Auto;
        command.PlpId = 0;
      }
      else
      {
        command.Mode = PlpMode.Manual;
        command.PlpId = (byte)streamId;
      }
      Marshal.StructureToPtr(command, _paramBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DvbT2SetPlp,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, PLP_SELECTION_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("eMPIA: result = success");
        return true;
      }

      this.LogError("eMPIA: failed to select stream, hr = 0x{0:x}, stream ID = {1}", hr, streamId);
      return false;
    }

    #endregion

    #region ICustomTuner methods

    /// <summary>
    /// Check if the extension implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the extension supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      return (channel is ChannelAtsc || channel is ChannelScte) && _isAtscQamModeSupported;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("eMPIA: tune to channel");

      if (!_isEmpia)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }

      QamMode mode;
      int frequency;
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        if (atscChannel.ModulationScheme == ModulationSchemeVsb.Vsb8)
        {
          mode = QamMode.Vsb;
          frequency = atscChannel.Frequency;
        }
        else
        {
          this.LogError("eMPIA: ATSC tune request uses unsupported modulation scheme {0}", atscChannel.ModulationScheme);
          return false;
        }
      }
      else
      {
        ChannelScte scteChannel = channel as ChannelScte;
        if (channel == null)
        {
          this.LogError("eMPIA: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
          return false;
        }

        switch (scteChannel.ModulationScheme)
        {
          case ModulationSchemeQam.Qam64:
            mode = QamMode.Qam64;
            break;
          case ModulationSchemeQam.Qam256:
            mode = QamMode.Qam256;
            break;
          default:
            this.LogError("eMPIA: SCTE tune request uses unsupported modulation scheme {0}", scteChannel.ModulationScheme);
            return false;
        }
        frequency = scteChannel.Frequency;
      }

      Marshal.WriteInt32(_paramBuffer, 0, (int)mode);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.QamSetMode,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("eMPIA: failed to set ATSC/QAM mode, hr = 0x{0:x}, mode = {1}", hr, mode);
        return false;
      }

      Marshal.WriteInt32(_paramBuffer, 0, frequency);
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.QamSetFrequency,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("eMPIA: failed to set frequency, hr = 0x{0:x}, frequency = {1} kHz", hr, frequency);
        return false;
      }

      this.LogDebug("eMPIA: result = success");
      return true;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      this.LogDebug("eMPIA: send DiSEqC command");

      if (!_isEmpia)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("eMPIA: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_TX_MESSAGE_LENGTH)
      {
        this.LogError("eMPIA: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.MessageTransmit = new byte[MAX_DISEQC_TX_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.MessageTransmit, 0, command.Length);
      message.MessageTransmitLength = command.Length;
      message.Version = DiseqcVersion.Version1;
      message.Mode = DiseqcMode.NoReply;
      message.IsLastMessage = false;    // Don't send a tone burst command.
      message.DelayAfterTransmit = 0;   // The caller handles delays.

      // Choose a sensible tone burst command in case the understanding of
      // IsLastMessage is incorrect.
      // If this is a switch command for port A then choose tone burst ("simple
      // A", unmodulated), otherwise choose data burst ("simple B", modulated).
      if (
        command.Length == 4 &&
        (
          (command[2] == (byte)DiseqcCommand.WriteN0 && (command[3] | 0x0c) == 0) ||
          (command[2] == (byte)DiseqcCommand.WriteN1 && (command[3] | 0x0f) == 0)
        )
      )
      {
        message.IsToneBurstModulated = false;
      }
      else
      {
        message.IsToneBurstModulated = true;
      }

      Marshal.StructureToPtr(message, _paramBuffer, false);
      //Dump.DumpBinary(_paramBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcSendMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("eMPIA: result = success");
        return true;
      }

      this.LogError("eMPIA: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <remarks>
    /// Don't know whether the driver will send a tone burst command without a
    /// DiSEqC command.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("eMPIA: send tone burst command, command = {0}", command);

      if (!_isEmpia)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.MessageTransmitLength = 0;
      message.Version = DiseqcVersion.Version1;
      message.Mode = DiseqcMode.NoReply;
      message.IsLastMessage = true;     // Send a tone burst command.
      message.DelayAfterTransmit = 0;   // The caller handles delays.
      if (command == ToneBurst.ToneBurst)
      {
        message.IsToneBurstModulated = false;
      }
      else if (command == ToneBurst.DataBurst)
      {
        message.IsToneBurstModulated = true;
      }

      Marshal.StructureToPtr(message, _paramBuffer, false);
      //Dump.DumpBinary(_paramBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcSendMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("eMPIA: result = success");
        return true;
      }

      this.LogError("eMPIA: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      // Set by tune request LNB frequency parameters.
      return true;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      this.LogDebug("eMPIA: read DiSEqC response");
      response = null;

      if (!_isEmpia)
      {
        this.LogWarn("eMPIA: not initialised or interface not supported");
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.MessageTransmitLength = 0;
      message.Version = DiseqcVersion.Version2;
      message.Mode = DiseqcMode.QuickReply;
      Marshal.StructureToPtr(message, _paramBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcSendMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("eMPIA: failed to read DiSEqC response, hr = 0x{0:x}", hr);
        return false;
      }

      Dump.DumpBinary(_paramBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      message = (DiseqcMessageParams)Marshal.PtrToStructure(_paramBuffer, typeof(DiseqcMessageParams));
      if (message.MessageReceiveLength > MAX_DISEQC_RX_MESSAGE_LENGTH)
      {
        this.LogError("eMPIA: unexpected number of DiSEqC response message bytes ({0}) returned", message.MessageReceiveLength);
        return false;
      }
      response = new byte[message.MessageReceiveLength];
      Buffer.BlockCopy(message.MessageReceive, 0, response, 0, (int)message.MessageReceiveLength);
      this.LogDebug("eMPIA: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Empia()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        if (_isEmpia && _isDvbT2PlpSelectionSupported)
        {
          // Switch to automatic mode to avoid problems for other software that
          // doesn't support PLP selection.
          SelectStream(-1);
        }
        if (_releasePropertySet)
        {
          Release.ComObject("eMPIA property set", ref _propertySet);
        }
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_paramBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_paramBuffer);
        _paramBuffer = IntPtr.Zero;
      }
      _isEmpia = false;
    }

    #endregion
  }
}