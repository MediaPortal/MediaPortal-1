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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Microsoft
{
  /// <summary>
  /// This class provides a base implementation of PID filtering, DiSEqC and clear QAM tuning support
  /// for devices that support Microsoft BDA interfaces and de-facto standards.
  /// </summary>
  public class Microsoft : BaseCustomDevice, IPidFilterController, IDiseqcDevice
  {
    #region constants

    private const int InstanceSize = 32;    // The size of a property instance (KSP_NODE) parameter.
    private const int ParamSize = 4;        // The size of a demodulator property value, usually ULONG.
    private const int BdaDiseqcMessageSize = 16;
    private const int MaxDiseqcMessageLength = 8;

    #endregion

    #region variables

    private bool _isMicrosoft = false;

    // DiSEqC
    private IKsPropertySet _diseqcPropertySet = null;           // IBDA_DiseqCommand
    private uint _requestId = 1;                                // Unique request ID for raw DiSEqC commands.
    private IBDA_FrequencyFilter _oldDiseqcInterface = null;    // IBDA_FrequencyFilter
    private IBDA_DeviceControl _deviceControl = null;
    private List<byte[]> _commands = new List<byte[]>();        // A cache of commands.
    private bool _useToneBurst = false;

    // Annex C QAM (North American cable)
    private IKsPropertySet _qamPropertySet = null;

    // PID filter
    private IMPEG2PIDMap _pidFilterInterface = null;
    private HashSet<UInt16> _currentPids = new HashSet<UInt16>();

    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// The class or property set that provides access to the tuner modulation parameter.
    /// </summary>
    protected virtual Guid ModulationPropertyClass
    {
      get
      {
        return typeof(IBDA_DigitalDemodulator).GUID;
      }
    }

    /// <summary>
    /// Determine if a filter supports the IBDA_DiseqCommand interface.
    /// </summary>
    /// <remarks>
    /// The IBDA_DiseqCommand was introduced in Windows 7. It is only supported by some tuners. We prefer to use
    /// this interface [over IBDA_FrequencyFilter.put_Range()] if it is available because it has capability to
    /// support sending and receiving raw messages.
    /// </remarks>
    /// <param name="filter">The filter to check.</param>
    /// <returns>a property set that supports the IBDA_DiseqCommand interface if successful, otherwise <c>null</c></returns>
    private IKsPropertySet CheckBdaDiseqcSupport(IBaseFilter filter)
    {
      this.LogDebug("Microsoft: check for IBDA_DiseqCommand DiSEqC support");

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      if (pin == null)
      {
        this.LogDebug("Microsoft: failed to find input pin");
        return null;
      }

      IKsPropertySet ps = pin as IKsPropertySet;
      if (ps == null)
      {
        this.LogDebug("Microsoft: input pin is not a property set");
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return null;
      }

      KSPropertySupport support;
      int hr = ps.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Microsoft: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return null;
      }

      return ps;
    }

    /// <summary>
    /// Determine if a filter supports the IBDA_FrequencyFilter interface.
    /// </summary>
    /// <remarks>
    /// The IBDA_FrequencyFilter.put_Range() function was the de-facto "BDA" standard for DiSEqC 1.0 prior
    /// to the introduction of IBDA_DiseqCommand in Windows 7.
    /// </remarks>
    /// <param name="filter">The filter to check.</param>
    /// <returns>a control node that supports the IBDA_FrequencyFilter interface if successful, otherwise <c>null</c></returns>
    private IBDA_FrequencyFilter CheckPutRangeDiseqcSupport(IBaseFilter filter)
    {
      this.LogDebug("Microsoft: check for IBDA_FrequencyFilter.put_Range() DiSEqC 1.0 support");

      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        this.LogDebug("Microsoft: filter is not a topology");
        return null;
      }

      object controlNode;
      int hr = topology.GetControlNode(0, 1, 0, out controlNode);
      IBDA_FrequencyFilter frequencyFilterInterface = controlNode as IBDA_FrequencyFilter;
      if (hr == 0 && frequencyFilterInterface != null)
      {
        return frequencyFilterInterface;
      }

      this.LogDebug("Microsoft: failed to get the control interface, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      if (controlNode != null)
      {
        DsUtils.ReleaseComObject(controlNode);
        controlNode = null;
      }
      return null;
    }

    /// <summary>
    /// Determine if a filter supports tuning annex C QAM (North American cable). This requires the ability to
    /// manually set the modulation type for the demodulator.
    /// </summary>
    /// <remarks>
    /// We need to be able to set the modulation manually to support QAM tuning on [at least] Windows XP.
    /// </remarks>
    /// <param name="filter">The filter to check.</param>
    /// <returns>a property set that supports a modulation property if successful, otherwise <c>null</c></returns>
    private IKsPropertySet CheckQamTuningSupport(IBaseFilter filter)
    {
      this.LogDebug("Microsoft: check for QAM tuning support");

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
      if (pin == null)
      {
        this.LogDebug("Microsoft: failed to find output pin");
        return null;
      }

      IKsPropertySet ps = pin as IKsPropertySet;
      if (ps == null)
      {
        this.LogDebug("Microsoft: output pin is not a property set");
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return null;
      }
      // Note: the below code could be problematic for single tuner/capture filter implementations. Some drivers
      // will not report whether a property set is supported unless the pin is connected. It is okay when we're
      // checking a tuner filter which has a capture filter connected, but if the tuner filter is also the capture
      // filter then the output pin(s) won't be connected yet.
      KSPropertySupport support;
      int hr = ps.QuerySupported(ModulationPropertyClass, (int)BdaDemodulatorProperty.ModulationType, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Microsoft: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return null;
      }

      return ps;
    }

    /// <summary>
    /// Determine if a filter supports PID filtering.
    /// </summary>
    /// <param name="filter">The filter to check.</param>
    /// <returns>an implementation of the IMPEG2PIDMap interace if successful, otherwise <c>null</c></returns>
    private IMPEG2PIDMap CheckBdaPidFilterSupport(IBaseFilter filter)
    {
      this.LogDebug("Microsoft: check for IMPEG2PIDMap PID filtering support");

      IMPEG2PIDMap pidFilterInterface = filter as IMPEG2PIDMap;
      if (pidFilterInterface != null)
      {
        return pidFilterInterface;
      }

      this.LogDebug("Microsoft: tuner does not implement the interface");
      return null;
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This is the most generic ICustomDevice implementation. It should only be used as a last resort
        // when more specialised interfaces are not suitable.
        return 1;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        if (_diseqcPropertySet != null)
        {
          return "Microsoft (BDA DiSEqC)";
        }
        if (_oldDiseqcInterface != null)
        {
          return "Microsoft (generic DiSEqC)";
        }
        if (_pidFilterInterface != null)
        {
          return "Microsoft (PID filter)";
        }
        if (_qamPropertySet != null)
        {
          return "Microsoft (generic ATSC/QAM)";
        }
        return base.Name;
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      this.LogDebug("Microsoft: initialising device");

      if (_isMicrosoft)
      {
        this.LogDebug("Microsoft: device is already initialised");
        return true;
      }

      // First, checks for DVB-S tuners: does the tuner support sending DiSEqC commands?
      if (tunerType == CardType.DvbS)
      {
        // We prefer the IBDA_DiseqCommand interface because it has the potential to support raw commands.
        _diseqcPropertySet = CheckBdaDiseqcSupport(tunerFilter);
        if (_diseqcPropertySet != null)
        {
          this.LogDebug("Microsoft: supported device detected (IBDA_DiseqCommand DiSEqC)");
          _isMicrosoft = true;
        }
        else
        {
          // Fallback to IBDA_FrequencyFilter.put_Range().
          _oldDiseqcInterface = (IBDA_FrequencyFilter)CheckPutRangeDiseqcSupport(tunerFilter);
          if (_oldDiseqcInterface != null)
          {
            this.LogDebug("Microsoft: supported device detected (IBDA_FrequencyFilter.put_Range() DiSEqC)");
            _isMicrosoft = true;
          }
        }
      }
      // For ATSC tuners: check if clear QAM tuning is supported.
      else if (tunerType == CardType.Atsc)
      {
        _qamPropertySet = CheckQamTuningSupport(tunerFilter);
        if (_qamPropertySet != null)
        {
          this.LogDebug("Microsoft: supported device detected (QAM tuning)");
          _isMicrosoft = true;
        }
      }

      // Any type of tuner can support PID filtering.
      _pidFilterInterface = CheckBdaPidFilterSupport(tunerFilter);
      if (_pidFilterInterface != null)
      {
        this.LogDebug("Microsoft: supported device detected (PID filtering)");
        _isMicrosoft = true;
      }

      if (!_isMicrosoft)
      {
        this.LogDebug("Microsoft: no interfaces supported");
        return false;
      }

      _deviceControl = tunerFilter as IBDA_DeviceControl;
      _paramBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      this.LogDebug("Microsoft: on before tune callback");
      action = DeviceAction.Default;

      if (!_isMicrosoft)
      {
        this.LogDebug("Microsoft: device not initialised or interface not supported");
        return;
      }

      // When tuning a DVB-S channel, we need to translate the modulation value.
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel != null)
      {
        if (dvbsChannel.ModulationType == ModulationType.ModQpsk)
        {
          dvbsChannel.ModulationType = ModulationType.ModNbcQpsk;
        }
        else if (dvbsChannel.ModulationType == ModulationType.Mod8Psk)
        {
          dvbsChannel.ModulationType = ModulationType.ModNbc8Psk;
        }
        else if (dvbsChannel.ModulationType == ModulationType.ModNotSet)
        {
          dvbsChannel.ModulationType = ModulationType.ModQpsk;
        }
        this.LogDebug("  modulation = {0}", dvbsChannel.ModulationType);
      }

      // When tuning a clear QAM channel, we need to set the modulation directly for compatibility with Windows XP.
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel != null && _qamPropertySet != null)
      {
        if (atscChannel.ModulationType == ModulationType.Mod64Qam || atscChannel.ModulationType == ModulationType.Mod256Qam)
        {
          Marshal.WriteInt32(_paramBuffer, (Int32)atscChannel.ModulationType);
          int hr = _qamPropertySet.Set(ModulationPropertyClass, (int)BdaDemodulatorProperty.ModulationType, _instanceBuffer, InstanceSize, _paramBuffer, ParamSize);
          if (hr != 0)
          {
            this.LogDebug("Microsoft: failed to set QAM modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          }
          else
          {
            this.LogDebug("  modulation = {0}", atscChannel.ModulationType);
          }
        }
      }
    }

    #endregion

    #endregion

    #region IPidFilterController member

    /// <summary>
    /// Configure the PID filter.
    /// </summary>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <param name="modulation">The current multiplex/transponder modulation scheme.</param>
    /// <param name="forceEnable">Set this parameter to <c>true</c> to force the filter to be enabled.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    public bool SetFilterPids(HashSet<UInt16> pids, ModulationType modulation, bool forceEnable)
    {
      this.LogDebug("Microsoft: set PID filter PIDs, modulation = {0}, force enable = {1}", modulation, forceEnable);

      if (!_isMicrosoft || _pidFilterInterface == null)
      {
        this.LogDebug("Microsoft: device not initialised or interface not supported");
        return false;
      }

      if (pids == null || pids.Count == 0)
      {
        this.LogDebug("Microsoft: disabling PID filter");
        // As far as I am aware, the PID filter is disabled by default after each retune. Assuming
        // that is the case, there is nothing to do here. Even if that assumption is wrong, there
        // is no obvious way to disable the PID filter and we have no PIDs we can't do anything.
        return true;
      }

      // If we get to here then we enable the filter by mapping the PIDs. The hardware probably has
      // a limit to the number of PIDs that it can handle, however we have no way to get that information
      // so we proceed as if there is no limit. Note also that attempting to call EnumPIDMap to
      // get the current list of PIDs from the hardware will fail... so we just assume that we can
      // add and remove PIDs on top of whatever PIDs are already mapped.
      bool success = true;
      this.LogDebug("Microsoft: current PID details (before update)");
      this.LogDebug("  count = {0}", _currentPids.Count);
      IEnumerator<UInt16> en = _currentPids.GetEnumerator();
      HashSet<UInt16> toRemove = new HashSet<UInt16>();
      byte i = 1;
      while (en.MoveNext())
      {
        this.LogDebug("  {0,-2}    = {1} (0x{1:x})", i, en.Current);
        i++;
        if (!pids.Contains(en.Current))
        {
          toRemove.Add(en.Current);
        }
      }

      // Remove the PIDs that are no longer needed.
      int hr;
      if (toRemove.Count > 0)
      {
        this.LogDebug("Microsoft: removing...");
        en = toRemove.GetEnumerator();
        i = 1;
        while (en.MoveNext())
        {
          hr = _pidFilterInterface.UnmapPID(1, new int[1] { en.Current });
          if (hr != 0)
          {
            this.LogDebug("Microsoft: failed to remove PID {0} (0x{0:x}), hr = 0x{1:x} ({2})", en.Current, hr, HResult.GetDXErrorString(hr));
            success = false;
          }
          else
          {
            this.LogDebug("  {0,-2}    = {1} (0x{1:x})", i, en.Current);
            _currentPids.Remove(en.Current);
          }
          i++;
        }
      }

      // Add the new PIDs.
      this.LogDebug("Microsoft: adding...");
      en = pids.GetEnumerator();
      i = 1;
      while (en.MoveNext())
      {
        if (_currentPids.Contains(en.Current))
        {
          continue;
        }
        hr = _pidFilterInterface.MapPID(1, new int[1] { en.Current }, MediaSampleContent.ElementaryStream);
        if (hr != 0)
        {
          this.LogDebug("Microsoft: failed to add PID {0} (0x{0:x}), hr = 0x{1:x} ({2})", en.Current, hr, HResult.GetDXErrorString(hr));
          success = false;
        }
        else
        {
          this.LogDebug("  {0,-2}    = {1} (0x{1:x})", i, en.Current);
        }
        i++;
      }

      if (success)
      {
        this.LogDebug("Microsoft: updates complete, result = success");
      }

      return success;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The Microsoft interface does not support directly setting the 22 kHz tone state. The tuning
    /// request LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Microsoft: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isMicrosoft)
      {
        this.LogDebug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (_diseqcPropertySet == null)
      {
        this.LogDebug("Microsoft: the interface does not support setting the tone state");
        return false;
      }

      _useToneBurst = toneBurstState != ToneBurst.None;
      this.LogDebug("Microsoft: result = success");
      return true;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <remarks>
    /// Drivers don't all behave the same. There are notes about MS network providers messing up the put_Range()
    /// method when attempting to send commands before the tune request (http://www.dvbdream.org/forum/viewtopic.php?f=1&t=608&start=15).
    /// In practise, I have observed the following behaviour with the tuners that I have tested:
    ///
    /// Must send commands before tuning
    ///----------------------------------
    /// - Anysee (E7 S2 - IBDA_DiseqCommand)
    /// - Pinnacle (PCTV 7010ix - IBDA_DiseqCommand)
    /// - TechniSat SkyStar HD2 (IBDA_DiseqCommand)
    /// - AVerMedia (Satellite Trinity - IBDA_DiseqCommand)
    ///
    /// Must send commands after tuning
    ///---------------------------------
    /// - TBS (5980 CI - IBDA_DiseqCommand)
    ///
    /// Doesn't matter
    ///----------------
    /// - Hauppauge (HVR-4400 - IBDA_DiseqCommand)
    /// - TechniSat SkyStar 2 r2.6d BDA driver (IBDA_DiseqCommand)
    /// - TeVii (S480 - IBDA_DiseqCommand)
    /// - Digital Everywhere (FloppyDTV S2 - IBDA_DiseqCommand)
    /// - TechnoTrend (Budget S2-3200 - IBDA_FrequencyFilter)
    /// 
    /// Since the list for "before" is longer than the list for "after" (and because we have specific DiSEqC
    /// support for Turbosight but not for AVerMedia and Pinnacle), we send commands before the tune request.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("Microsoft: send DiSEqC command");

      if (!_isMicrosoft || _deviceControl == null || (_diseqcPropertySet == null && _oldDiseqcInterface == null))
      {
        this.LogDebug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("Microsoft: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        this.LogDebug("Microsoft: command too long, length = {0}", command.Length);
        return false;
      }

      // Attempt to translate the raw command back into a DiSEqC 1.0 command. The old interface only supports
      // DiSEqC 1.0 switch commands, and some drivers don't implement support for raw commands using the
      // IBDA_DiseqCommand interface (so we want to use the simpler LNB source property if possible).
      int portNumber = -1;
      if (command.Length == 4 &&
        (command[0] == (byte)DiseqcFrame.CommandFirstTransmissionNoReply ||
        command[0] == (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) &&
        command[1] == (byte)DiseqcAddress.AnySwitch &&
        command[2] == (byte)DiseqcCommand.WriteN0)
      {
        portNumber = (command[3] & 0xc) >> 2;
        this.LogDebug("Microsoft: DiSEqC 1.0 command recognised for port {0}", portNumber);
      }
      if (_oldDiseqcInterface != null && portNumber == -1)
      {
        this.LogDebug("Microsoft: command not supported");
        return false;
      }

      // If we get to here, then we're going to attempt to send a command.
      bool success = true;
      int hr = _deviceControl.StartChanges();
      if (hr != 0)
      {
        this.LogDebug("Microsoft: failed to start device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }

      // IBDA_DiseqCommand interface
      if (_diseqcPropertySet != null)
      {
        // This property has to be set for each command sent for some tuners (eg. TBS).
        Marshal.WriteInt32(_paramBuffer, 0, 1);
        hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Enable, _instanceBuffer, InstanceSize, _paramBuffer, 4);
        if (hr != 0)
        {
          this.LogDebug("Microsoft: failed to enable DiSEqC commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }

        // Disable command repeats for optimal performance. We set this for each command for "safety",
        // assuming that if DiSEqC must be enabled for each command then the same may apply to repeats.
        Marshal.WriteInt32(_paramBuffer, 0, 0);
        hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Repeats, _instanceBuffer, InstanceSize, _paramBuffer, 4);
        if (hr != 0)
        {
          this.LogDebug("Microsoft: failed to disable DiSEqC command repeats, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }

        // Disable tone burst messages - it seems that many drivers don't support them, and setting the correct
        // tone state is inconvenient with the IBDA_DiseqCommand implementation.
        Marshal.WriteInt32(_paramBuffer, 0, 0);
        hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.UseToneBurst, _instanceBuffer, InstanceSize, _paramBuffer, 4);
        if (hr != 0)
        {
          this.LogDebug("Microsoft: failed to disable tone burst commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }

        portNumber++;   // Range needs to be 1..4, not 0..3 - Microsoft documentation has been ignored... again!
        if (portNumber > 0)
        {
          Marshal.WriteInt32(_paramBuffer, 0, portNumber);
          hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, _instanceBuffer, InstanceSize, _paramBuffer, 4);
          if (hr != 0)
          {
            this.LogDebug("Microsoft: failed to set LNB source, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            success = false;
          }
        }
        else
        {
          BdaDiseqcMessage message = new BdaDiseqcMessage();
          message.RequestId = _requestId++;
          message.PacketLength = (uint)command.Length;
          message.PacketData = new byte[MaxDiseqcMessageLength];
          Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
          Marshal.StructureToPtr(message, _paramBuffer, true);
          //DVB_MMI.DumpBinary(_paramBuffer, 0, BdaDiseqcMessageSize);
          hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Send, _instanceBuffer, InstanceSize, _paramBuffer, BdaDiseqcMessageSize);
          if (hr != 0)
          {
            this.LogDebug("Microsoft: failed to send command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            success = false;
          }
        }
      }
      // IBDA_FrequencyFilter interface
      else if (_oldDiseqcInterface != null)
      {
        // The two rightmost bytes encode option and position respectively.
        if (portNumber > 1)
        {
          portNumber -= 2;
          portNumber |= 0x100;
        }
        this.LogDebug("Microsoft: range = 0x{0:x4}", portNumber);
        hr = _oldDiseqcInterface.put_Range((ulong)portNumber);
        if (hr != 0)
        {
          this.LogDebug("Microsoft: failed to put range, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      // Finalise (send) the command.
      hr = _deviceControl.CheckChanges();
      if (hr != 0)
      {
        this.LogDebug("Microsoft: failed to check device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }
      hr = _deviceControl.CommitChanges();
      if (hr != 0)
      {
        this.LogDebug("Microsoft: failed to commit device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }

      this.LogDebug("Microsoft: result = {0}", success);
      return success;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      this.LogDebug("Microsoft: read DiSEqC response");
      response = null;

      if (!_isMicrosoft)
      {
        this.LogDebug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (_diseqcPropertySet == null)
      {
        this.LogDebug("Microsoft: the interface does not support reading DiSEqC responses");
        return false;
      }

      for (int i = 0; i < BdaDiseqcMessageSize; i++)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
      }
      int returnedByteCount;
      int hr = _diseqcPropertySet.Get(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Response, _paramBuffer, InstanceSize, _paramBuffer, BdaDiseqcMessageSize, out returnedByteCount);
      if (hr == 0 && returnedByteCount == BdaDiseqcMessageSize)
      {
        // Copy the response into the return array.
        BdaDiseqcMessage message = (BdaDiseqcMessage)Marshal.PtrToStructure(_paramBuffer, typeof(BdaDiseqcMessage));
        if (message.PacketLength > MaxDiseqcMessageLength)
        {
          this.LogDebug("Microsoft: response length is out of bounds, response length = {0}", message.PacketLength);
          return false;
        }
        this.LogDebug("Microsoft: result = success");
        response = new byte[message.PacketLength];
        Buffer.BlockCopy(message.PacketData, 0, response, 0, (int)message.PacketLength);
        return true;
      }

      this.LogDebug("Microsoft: result = failure, response length = {0}, hr = 0x{1:x} ({2})", returnedByteCount, hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      if (_diseqcPropertySet != null)
      {
        DsUtils.ReleaseComObject(_diseqcPropertySet);
        _diseqcPropertySet = null;
      }
      if (_oldDiseqcInterface != null)
      {
        DsUtils.ReleaseComObject(_oldDiseqcInterface);
        _oldDiseqcInterface = null;
      }
      _deviceControl = null;
      if (_qamPropertySet != null)
      {
        DsUtils.ReleaseComObject(_qamPropertySet);
        _qamPropertySet = null;
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
      _isMicrosoft = false;
    }

    #endregion
  }
}
