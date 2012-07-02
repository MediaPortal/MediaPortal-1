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
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;
using TvLibrary.Log;
using System.Collections.Generic;

namespace TvEngine
{
  /// <summary>
  /// This class provides a base implementation of DiSEqC and clear QAM tuning support for devices that
  /// support Microsoft BDA interfaces and de-facto standards.
  /// </summary>
  public class Microsoft : BaseCustomDevice, IDiseqcDevice
  {
    #region enums

    private enum BdaDiseqcProperty
    {
      Enable = 0,
      LnbSource,
      UseToneBurst,
      Repeats,
      Send,
      Response
    }

    private enum BdaDemodulatorProperty
    {
      ModulationType = 0,
      InnerFecType,
      InnerFecRate,
      OuterFecType,
      OuterFecRate,
      SymbolRate,
      SpectralInversion,
      TransmissionMode,
      RollOff,
      Pilot,
      SignalTimeouts,
      PlpNumber               // physical layer pipe - for DVB-S2 and DVB-T2
    }

    #endregion

    #region structs

    private struct BdaDiseqcMessage
    {
      public UInt32 RequestId;
      public UInt32 PacketLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] PacketData;
    }

    #endregion

    #region IBDA_DiseqCommand interface

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
      Guid("f84e2ab0-3c6b-45e3-a0fc-8669d4b81f11"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IBDA_DiseqCommand
    {
      [PreserveSig]
      int put_EnableDiseqCommands([In, MarshalAs(UnmanagedType.I1)] bool bEnable);

      [PreserveSig]
      int put_DiseqLNBSource([In] UInt32 ulLNBSource);

      [PreserveSig]
      int put_DiseqUseToneBurst([In, MarshalAs(UnmanagedType.I1)] bool bUseToneBurst);

      [PreserveSig]
      int put_DiseqRepeats([In] UInt32 ulRepeats);

      [PreserveSig]
      int put_DiseqSendCommand([In] UInt32 ulRequestId, [In] UInt32 ulcbCommandLen, [In] ref byte pbCommand);

      [PreserveSig]
      int get_DiseqResponse([In] UInt32 ulRequestId, [In, Out] ref int pulcbResponseLen, [In, Out] ref byte pbResponse);
    }

    #endregion

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
      Log.Debug("Microsoft: check for IBDA_DiseqCommand DiSEqC support");

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      if (pin == null)
      {
        Log.Debug("Microsoft: failed to find input pin");
        return null;
      }

      IKsPropertySet ps = pin as IKsPropertySet;
      if (ps == null)
      {
        Log.Debug("Microsoft: input pin is not a property set");
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return null;
      }

      KSPropertySupport support;
      int hr = ps.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Microsoft: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
    private object CheckPutRangeDiseqcSupport(IBaseFilter filter)
    {
      Log.Debug("Microsoft: check for IBDA_FrequencyFilter.put_Range() DiSEqC 1.0 support");

      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        Log.Debug("Microsoft: filter is not a topology");
        return null;
      }

      object controlNode;
      int hr = topology.GetControlNode(0, 1, 0, out controlNode);
      if (hr == 0 && controlNode is IBDA_FrequencyFilter)
      {
        Log.Debug("Microsoft: found node that implements the interface");
        return controlNode;
      }

      Log.Debug("Microsoft: failed to get the control interface, hr = 0x{1:x} ({2})", hr, HResult.GetDXErrorString(hr));
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
      Log.Debug("Microsoft: check for QAM tuning support");

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
      if (pin == null)
      {
        Log.Debug("Microsoft: failed to find output pin");
        return null;
      }

      IKsPropertySet ps = pin as IKsPropertySet;
      if (ps == null)
      {
        Log.Debug("Microsoft: output pin is not a property set");
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
        Log.Debug("Microsoft: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return null;
      }

      return ps;
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
      Log.Debug("Microsoft: initialising device");
      if (tunerType != CardType.DvbS && tunerType != CardType.Atsc)
      {
        Log.Debug("Microsoft: tuner type {0} is not supported", tunerType);
        return false;
      }
      if (_isMicrosoft)
      {
        Log.Debug("Microsoft: device is already initialised");
        return true;
      }

      // First, checks for DVB-S tuners: does the tuner support sending DiSEqC commands?
      if (tunerType == CardType.DvbS)
      {
        // We prefer the IBDA_DiseqCommand interface.
        _diseqcPropertySet = CheckBdaDiseqcSupport(tunerFilter);
        if (_diseqcPropertySet == null)
        {
          // Fallback to IBDA_FrequencyFilter.put_Range().
          _oldDiseqcInterface = (IBDA_FrequencyFilter)CheckPutRangeDiseqcSupport(tunerFilter);
          if (_oldDiseqcInterface == null)
          {
            return false;
          }
          Log.Debug("Microsoft: supported device detected (IBDA_FrequencyFilter.put_Range() DiSEqC)");
        }
      }
      else
      {
        _qamPropertySet = CheckQamTuningSupport(tunerFilter);
        if (_qamPropertySet == null)
        {
          return false;
        }
        Log.Debug("Microsoft: supported device detected (QAM tuning)");
      }

      _isMicrosoft = true;
      _deviceControl = tunerFilter as IBDA_DeviceControl;
      _paramBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      if (_diseqcPropertySet == null)
      {
        return true;
      }

      // For the IBDA_DiseqCommand interface: disable automatic command repetition for optimal performance.
      Log.Debug("Microsoft: supported device detected (IBDA_DiseqCommand DiSEqC)");
      int hr = _deviceControl.StartChanges();
      if (hr != 0)
      {
        Log.Debug("Microsoft: failed to start device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      Marshal.WriteInt32(_paramBuffer, 0, 0);
      hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Repeats, _instanceBuffer, InstanceSize, _paramBuffer, 4);
      if (hr != 0)
      {
        Log.Debug("Microsoft: failed to disable DiSEqC command repeats, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      hr = _deviceControl.CheckChanges();
      if (hr != 0)
      {
        Log.Debug("Microsoft: failed to check device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      hr = _deviceControl.CommitChanges();
      if (hr != 0)
      {
        Log.Debug("Microsoft: failed to commit device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return true;
    }

    #region graph state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="forceGraphStart">Ensure that the tuner's BDA graph is running when the tune request is submitted.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out bool forceGraphStart)
    {
      Log.Debug("Microsoft: on before tune callback");
      forceGraphStart = false;

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
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
        Log.Debug("  modulation = {0}", dvbsChannel.ModulationType);
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
            Log.Debug("Microsoft: failed to set QAM modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          }
          else
          {
            Log.Debug("  modulation = {0}", atscChannel.ModulationType);
          }
        }
      }
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted but before the device's BDA graph is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public override void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
      SendDeferredCommands();
    }

    #endregion

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>The Microsoft interface does not support directly setting the 22 kHz tone state. The tuning
    /// request LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Debug("Microsoft: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (_diseqcPropertySet == null)
      {
        Log.Debug("Microsoft: the interface does not support setting the tone state");
        return false;
      }

      _useToneBurst = toneBurstState != ToneBurst.None;
      Log.Debug("Microsoft: result = success");
      return true;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      Log.Debug("Microsoft: send DiSEqC command");

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Microsoft: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Debug("Microsoft: command too long, length = {0}", command.Length);
        return false;
      }

      // Most tuners can only successfully send commands after the tune request is submitted and/or the graph is
      // running. Here we only save the command for sending later.
      byte[] commandCopy = new byte[command.Length];
      Buffer.BlockCopy(command, 0, commandCopy, 0, command.Length);
      _commands.Add(commandCopy);
      Log.Debug("Microsoft: result = success");
      return true;
    }

    private void SendDeferredCommands()
    {
      if (_commands.Count == 0)
      {
        return;
      }
      Log.Debug("Microsoft: send {0} deferred DiSEqC command(s)", _commands.Count);

      bool isFirstCommand = true;
      for (byte i = 0; i < _commands.Count; i++)
      {
        byte[] command = _commands[i];
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
          Log.Debug("Microsoft: DiSEqC 1.0 command recognised for port {0}", portNumber);
        }
        if (_oldDiseqcInterface != null && portNumber == -1)
        {
          Log.Debug("Microsoft: command not supported");
          continue;
        }

        // If we get to here, then we're going to attempt to send a command.
        int hr = _deviceControl.StartChanges();
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to start device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }

        // IBDA_DiseqCommand interface
        if (_diseqcPropertySet != null)
        {
          // This property has to be set for each command sent for some tuners (eg. TBS).
          Marshal.WriteInt32(_paramBuffer, 0, 1);
          hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Enable, _instanceBuffer, InstanceSize, _paramBuffer, 4);
          if (hr != 0)
          {
            Log.Debug("Microsoft: failed to enable DiSEqC commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          }

          // Enable or disable tone burst messages for this set of commands.
          if (isFirstCommand)
          {
            Log.Debug("Microsoft: use tone burst = {0}", _useToneBurst);
            Marshal.WriteInt32(_paramBuffer, 0, _useToneBurst ? 1 : 0);
            hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.UseToneBurst, _instanceBuffer, InstanceSize, _paramBuffer, 4);
            if (hr != 0)
            {
              Log.Debug("Microsoft: failed to enable/disable tone burst commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            }
            isFirstCommand = false;
          }

          portNumber++;   // range needs to be 1..4, not 0..3
          if (portNumber > 0)
          {
            Marshal.WriteInt32(_paramBuffer, 0, portNumber);
            hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, _instanceBuffer, InstanceSize, _paramBuffer, 4);
            if (hr != 0)
            {
              Log.Debug("Microsoft: failed to set LNB source, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            }
          }
          else
          {
            BdaDiseqcMessage message = new BdaDiseqcMessage();
            message.RequestId = _requestId;
            message.PacketLength = (uint)command.Length;
            message.PacketData = new byte[MaxDiseqcMessageLength];
            Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
            Marshal.StructureToPtr(message, _paramBuffer, true);
            //DVB_MMI.DumpBinary(_paramBuffer, 0, BdaDiseqcMessageSize);
            hr = _diseqcPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Send, _instanceBuffer, InstanceSize, _paramBuffer, BdaDiseqcMessageSize);
            if (hr != 0)
            {
              Log.Debug("Microsoft: failed to send command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
          Log.Debug("Microsoft: range = 0x{0:x4}", portNumber);
          hr = _oldDiseqcInterface.put_Range((ulong)portNumber);
          if (hr != 0)
          {
            Log.Debug("Microsoft: failed to put range, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          }
        }

        // Finalise (send) the command.
        hr = _deviceControl.CheckChanges();
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to check device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }
        hr = _deviceControl.CommitChanges();
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to commit device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }
      }

      // Don't forget - now that we've sent the commands we need to clear the command cache.
      _commands.Clear();
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      Log.Debug("Microsoft: read DiSEqC response");
      response = null;

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (_diseqcPropertySet == null)
      {
        Log.Debug("Microsoft: the interface does not support reading DiSEqC responses");
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
          Log.Debug("Microsoft: response length is out of bounds, response length = {0}", message.PacketLength);
          return false;
        }
        Log.Debug("Microsoft: result = success");
        response = new byte[message.PacketLength];
        Buffer.BlockCopy(message.PacketData, 0, response, 0, (int)message.PacketLength);
        return true;
      }

      Log.Debug("Microsoft: result = failure, response length = {0}, hr = 0x{1:x} ({2})", returnedByteCount, hr, HResult.GetDXErrorString(hr));
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
