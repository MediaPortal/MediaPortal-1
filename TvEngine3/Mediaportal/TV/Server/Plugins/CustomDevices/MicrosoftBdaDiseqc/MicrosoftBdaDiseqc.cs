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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBdaDiseqc
{
  /// <summary>
  /// This class provides a base implementation of DiSEqC support for tuners that support Microsoft
  /// BDA interfaces and de-facto standards.
  /// </summary>
  public class MicrosoftBdaDiseqc : BaseCustomDevice, IDiseqcDevice
  {
    #region constants

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private static readonly int BDA_DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(BdaDiseqcMessage));   // 16
    private const int MAX_DISEQC_MESSAGE_LENGTH = 8;

    #endregion

    #region variables

    private bool _isMicrosoftBdaDiseqc = false;

    private IKsPropertySet _propertySet = null;
    private uint _requestId = 1;                          // Unique request ID for raw DiSEqC commands.
    private IBDA_DeviceControl _deviceControl = null;

    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This implementation should be used in preference to the old frequency filter (put range)
        // interface and when more specialised interfaces are not available.
        return 2;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft BDA DiSEqC";
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
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Microsoft BDA DiSEqC: initialising");

      if (_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: extension already initialised");
        return true;
      }

      if (tunerType != CardType.DvbS)
      {
        this.LogDebug("Microsoft BDA DiSEqC: tuner type not supported");
        return false;
      }

      _deviceControl = context as IBDA_DeviceControl;
      if (_deviceControl == null)
      {
        this.LogDebug("Microsoft BDA DiSEqC: device control interface not supported");
        return false;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Microsoft BDA DiSEqC: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      if (pin == null)
      {
        this.LogError("Microsoft BDA DiSEqC: failed to find filter input pin");
        return false;
      }

      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Microsoft BDA DiSEqC: input pin is not a property set");
        Release.ComObject("Microsoft BDA DiSEqC filter input pin", ref pin);
        return false;
      }

      // IBDA_DiseqCommand was introduced in Windows 7. It is only supported by some tuners. We
      // prefer to use this interface [over IBDA_FrequencyFilter.put_Range()] if it is available
      // because it has capability to support sending and receiving raw messages. If the driver
      // supports it then it can be used even on Windows XP.
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Microsoft BDA DiSEqC: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        Release.ComObject("Microsoft BDA DiSEqC property set", ref _propertySet);
        pin = null;
        return false;
      }

      this.LogInfo("Microsoft BDA DiSEqC: extension supported");
      _isMicrosoftBdaDiseqc = true;
      _paramBuffer = Marshal.AllocCoTaskMem(BDA_DISEQC_MESSAGE_SIZE);
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      return true;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Microsoft BDA DiSEqC: on before tune call back");
      action = TunerAction.Default;

      if (!_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: not initialised or interface not supported");
        return;
      }

      // When tuning a DVB-S channel, we need to translate the modulation value.
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        return;
      }
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

    #endregion

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
      // Not implemented.
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <remarks>
    /// Drivers don't all behave the same. Take care when modifying this method!
    /// In practise, I have observed the following behaviour with the tuners that I have tested:
    ///
    /// Must send commands before tuning
    ///----------------------------------
    /// - Anysee E7 S2
    /// - Pinnacle PCTV 7010ix
    /// - TechniSat SkyStar HD2
    /// - AVerMedia Satellite Trinity
    ///
    /// Must send commands after tuning
    ///---------------------------------
    /// - TBS 5980 CI
    ///
    /// Doesn't matter
    ///----------------
    /// - Hauppauge HVR-4400
    /// - TechniSat SkyStar 2 r2.6d BDA driver
    /// - TeVii S480
    /// - Digital Everywhere FloppyDTV S2
    /// 
    /// Since the list for "before" is longer than the list for "after" (and because we have specific DiSEqC
    /// support for Turbosight but not for AVerMedia and Pinnacle), we send commands before the tune request.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
    {
      this.LogDebug("Microsoft BDA DiSEqC: send DiSEqC command");

      if (!_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogError("Microsoft BDA DiSEqC: command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Microsoft BDA DiSEqC: command too long, length = {0}", command.Length);
        return false;
      }

      // Attempt to translate the raw command back into a DiSEqC 1.0 command. Some drivers don't
      // implement support for raw commands using the IBDA_DiseqCommand interface so we want to use
      // the simpler LNB source property if possible.
      int portNumber = -1;
      if (command.Length == 4 &&
        (command[0] == (byte)DiseqcFrame.CommandFirstTransmissionNoReply ||
        command[0] == (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) &&
        command[1] == (byte)DiseqcAddress.AnySwitch &&
        command[2] == (byte)DiseqcCommand.WriteN0)
      {
        portNumber = (command[3] & 0xc) >> 2;
        this.LogDebug("Microsoft BDA DiSEqC: DiSEqC 1.0 command recognised for port {0}", portNumber);
      }

      // If we get to here, then we're going to attempt to send a command.
      bool success = true;
      int hr = _deviceControl.StartChanges();
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("Microsoft BDA DiSEqC: failed to start device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }

      // This property has to be set for each command sent for some tuners (eg. TBS).
      if (success)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 1);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Enable, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to enable DiSEqC commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      // Disable command repeats for optimal performance. We set this for each command for "safety",
      // assuming that if DiSEqC must be enabled for each command then the same may apply to repeats.
      if (success)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Repeats, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to disable DiSEqC command repeats, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      // Disable tone burst messages - it seems that many drivers don't support them, and setting the correct
      // tone state is inconvenient with the IBDA_DiseqCommand implementation.
      if (success)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.UseToneBurst, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to disable tone burst commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      if (success)
      {
        LNB_Source lnbSource = (LNB_Source)portNumber++;
        if (lnbSource != LNB_Source.NOT_SET)
        {
          Marshal.WriteInt32(_paramBuffer, 0, (int)lnbSource);
          hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogError("Microsoft BDA DiSEqC: failed to set LNB source, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            success = false;
          }
        }
        else
        {
          BdaDiseqcMessage message = new BdaDiseqcMessage();
          message.RequestId = _requestId++;
          message.PacketLength = (uint)command.Length;
          message.PacketData = new byte[MAX_DISEQC_MESSAGE_LENGTH];
          Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
          Marshal.StructureToPtr(message, _paramBuffer, true);
          //Dump.DumpBinary(_paramBuffer, BDA_DISEQC_MESSAGE_SIZE);
          hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Send, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, BDA_DISEQC_MESSAGE_SIZE);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogError("Microsoft BDA DiSEqC: failed to send DiSEqC command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            success = false;
          }
        }
      }

      // Finalise (send) the command.
      if (success)
      {
        hr = _deviceControl.CheckChanges();
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to check device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }
      if (success)
      {
        hr = _deviceControl.CommitChanges();
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to commit device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      this.LogDebug("Microsoft BDA DiSEqC: result = {0}", success);
      return success;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadDiseqcResponse(out byte[] response)
    {
      this.LogDebug("Microsoft BDA DiSEqC: read DiSEqC response");
      response = null;

      if (!_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: not initialised or interface not supported");
        return false;
      }

      for (int i = 0; i < BDA_DISEQC_MESSAGE_SIZE; i++)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Response, _paramBuffer, INSTANCE_SIZE, _paramBuffer, BDA_DISEQC_MESSAGE_SIZE, out returnedByteCount);
      if (hr == (int)HResult.Severity.Success && returnedByteCount == BDA_DISEQC_MESSAGE_SIZE)
      {
        // Copy the response into the return array.
        BdaDiseqcMessage message = (BdaDiseqcMessage)Marshal.PtrToStructure(_paramBuffer, typeof(BdaDiseqcMessage));
        if (message.PacketLength > MAX_DISEQC_MESSAGE_LENGTH)
        {
          this.LogError("Microsoft BDA DiSEqC: response length is out of bounds, response length = {0}", message.PacketLength);
          return false;
        }
        this.LogDebug("Microsoft BDA DiSEqC: result = success");
        response = new byte[message.PacketLength];
        Buffer.BlockCopy(message.PacketData, 0, response, 0, (int)message.PacketLength);
        return true;
      }

      this.LogError("Microsoft BDA DiSEqC: failed to read DiSEqC response, response length = {0}, hr = 0x{1:x} ({2})", returnedByteCount, hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      Release.ComObject("Microsoft BDA DiSEqC property set", ref _propertySet);
      _deviceControl = null;
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
      _isMicrosoftBdaDiseqc = false;
    }

    #endregion
  }
}