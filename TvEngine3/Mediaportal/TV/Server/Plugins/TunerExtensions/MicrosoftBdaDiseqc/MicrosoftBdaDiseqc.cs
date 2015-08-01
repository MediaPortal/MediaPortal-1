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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBdaDiseqc
{
  /// <summary>
  /// This class provides a base implementation of DiSEqC support for tuners that support Microsoft
  /// BDA interfaces and de-facto standards.
  /// </summary>
  public class MicrosoftBdaDiseqc : BaseTunerExtension, IDiseqcDevice, IDisposable
  {
    #region constants

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private static readonly int BDA_DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(BdaDiseqcMessage));   // 16
    private const int MAX_DISEQC_MESSAGE_LENGTH = 8;

    #endregion

    #region variables

    private bool _isMicrosoftBdaDiseqc = false;

    private IKsPropertySet _propertySet = null;
    private IBDA_DeviceControl _deviceControl = null;

    // settings
    private bool _useSettingAutoDetection = true;
    private bool _setEnable = true;             // Not sure if it is necessary to set for every command, but it is probably more compatible to do so.
    private bool _enable = true;                // Most implementations seem to need this to be set true.
    private bool _setRepeatCount = true;        // Repeats may not be implemented.
    private int _repeatCount = 0;               // Zero should mean send once.
    private bool _isToneBurstSupported = false; // Probably not implemented anyway.
    private bool _preferSend = false;           // Most implementations only support LNB source.

    private int _requestId = 1;             // Unique request ID for raw DiSEqC commands.

    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    #region constructors

    /// <summary>
    /// Constructor for <see cref="MicrosoftBdaDiseqc"/> instances.
    /// </summary>
    public MicrosoftBdaDiseqc()
    {
      _useSettingAutoDetection = true;
    }

    /// <summary>
    /// Constructor for non-inherited types (eg. <see cref="DigitalDevices"/>).
    /// </summary>
    public MicrosoftBdaDiseqc(bool setEnable = true, bool enable = true, bool setRepeatCount = true, int repeatCount = 0, bool isToneBurstSupported = false, bool preferSend = false)
    {
      _useSettingAutoDetection = false;
      _setEnable = setEnable;
      _enable = enable;
      _setRepeatCount = setRepeatCount;
      _repeatCount = repeatCount;
      _isToneBurstSupported = isToneBurstSupported;
      _preferSend = preferSend;
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
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
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft BDA DiSEqC";
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
      this.LogDebug("Microsoft BDA DiSEqC: initialising");

      if (_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Microsoft BDA DiSEqC: tuner type not supported");
        return false;
      }

      _deviceControl = context as IBDA_DeviceControl;
      if (_deviceControl == null)
      {
        this.LogDebug("Microsoft BDA DiSEqC: context is not a device control");
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
        this.LogDebug("Microsoft BDA DiSEqC: pin is not a property set");
        Release.ComObject("Microsoft BDA DiSEqC filter input pin", ref pin);
        return false;
      }

      // IBDA_DiseqCommand was introduced in Windows 7. It is only supported by some tuners. We
      // prefer to use this interface [over IBDA_FrequencyFilter.put_Range()] if it is available
      // because it has capability to support sending and receiving raw messages. If the driver
      // supports it then it can be used even on Windows XP.
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, out support);
      if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Set))
      {
        this.LogInfo("Microsoft BDA DiSEqC: extension supported, LNB source property");
      }
      else
      {
        hr = _propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Send, out support);
        if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Set))
        {
          this.LogInfo("Microsoft BDA DiSEqC: extension supported, send property");
          _preferSend = true;
        }
        else
        {
          this.LogDebug("Microsoft BDA DiSEqC: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
          Release.ComObject("Microsoft BDA DiSEqC property set", ref _propertySet);
          pin = null;
          return false;
        }
      }

      if (_useSettingAutoDetection)
      {
        hr = _propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Enable, out support);
        if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
        {
          _setEnable = false;
        }
        hr = _propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Repeats, out support);
        if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
        {
          _setRepeatCount = false;
        }
        hr = _propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.UseToneBurst, out support);
        if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Set))
        {
          _isToneBurstSupported = true;
        }
        this.LogDebug("Microsoft BDA DiSEqc: auto-detected settings, enable = {0} [{1}], repeat count = {2} [{3}], tone burst = {4}", _setEnable, _enable, _setRepeatCount, _repeatCount, _isToneBurstSupported);
      }
      else
      {
        this.LogDebug("Microsoft BDA DiSEqc: custom settings, enable = {0} [{1}], repeat count = {2} [{3}], tone burst = {4}", _setEnable, _enable, _setRepeatCount, _repeatCount, _isToneBurstSupported);
      }

      _isMicrosoftBdaDiseqc = true;
      _paramBuffer = Marshal.AllocCoTaskMem(BDA_DISEQC_MESSAGE_SIZE);
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      return true;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <remarks>
    /// Drivers don't all behave the same. Take care when modifying this
    /// method! In practise, I have observed the following behaviour with the
    /// tuners that I have tested:
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
    /// Since the list for "before" is longer than the list for "after" (and
    /// because we have specific DiSEqC support for Turbosight but not for
    /// AVerMedia and Pinnacle), we send commands before the tune request.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      this.LogDebug("Microsoft BDA DiSEqC: send DiSEqC command");

      if (!_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Microsoft BDA DiSEqC: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Microsoft BDA DiSEqC: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      // Attempt to translate the raw command back into a DiSEqC 1.0 command. Some drivers don't
      // implement support for raw commands using the IBDA_DiseqCommand interface so we want to use
      // the simpler LNB source property if possible.
      LNB_Source lnbSource = LNB_Source.NOT_SET;
      if (command.Length == 4 &&
        (command[0] == (byte)DiseqcFrame.CommandFirstTransmissionNoReply ||
        command[0] == (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) &&
        command[1] == (byte)DiseqcAddress.AnySwitch &&
        command[2] == (byte)DiseqcCommand.WriteN0)
      {
        lnbSource = (LNB_Source)(((command[3] & 0xc) >> 2) + 1);
        this.LogDebug("Microsoft BDA DiSEqC: DiSEqC 1.0 command recognised for port {0}", lnbSource);
      }

      // If we get to here, then we're going to attempt to send a command.
      bool success = true;
      int hr = _deviceControl.StartChanges();
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Microsoft BDA DiSEqC: failed to start DiSEqC command device control changes, hr = 0x{0:x}", hr);
        success = false;
      }

      // This property has to be set for each command sent for some tuners (eg.
      // TBS).
      if (success && _setEnable)
      {
        if (_enable)
        {
          Marshal.WriteInt32(_paramBuffer, 0, 1);
        }
        else
        {
          Marshal.WriteInt32(_paramBuffer, 0, 0);
        }
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Enable, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to enable/disable DiSEqC commands, hr = 0x{0:x}, enable = {1}", hr, _enable);
          success = false;
        }
      }

      // Disable command repeats for optimal performance. We set this for each
      // command for "safety",/ assuming that if DiSEqC must be enabled for
      // each command then the same may apply to repeats.
      if (success && _setRepeatCount)
      {
        Marshal.WriteInt32(_paramBuffer, 0, _repeatCount);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Repeats, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to set DiSEqC command repeat count, hr = 0x{0:x}, count = {1}", hr, _repeatCount);
          success = false;
        }
      }

      // We're sending a normal command here, not a burst command.
      if (success && _isToneBurstSupported)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.UseToneBurst, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to disable tone burst commands, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      if (success)
      {
        if (lnbSource != LNB_Source.NOT_SET && !_preferSend)
        {
          Marshal.WriteInt32(_paramBuffer, 0, (int)lnbSource);
          hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Microsoft BDA DiSEqC: failed to set DiSEqC command LNB source, hr = 0x{0:x}", hr);
            success = false;
          }
        }
        else
        {
          BdaDiseqcMessage message = new BdaDiseqcMessage();
          message.RequestId = _requestId++;
          message.PacketLength = command.Length;
          message.PacketData = new byte[MAX_DISEQC_MESSAGE_LENGTH];
          Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
          Marshal.StructureToPtr(message, _paramBuffer, false);
          //Dump.DumpBinary(_paramBuffer, BDA_DISEQC_MESSAGE_SIZE);
          hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Send, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, BDA_DISEQC_MESSAGE_SIZE);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Microsoft BDA DiSEqC: failed to send DiSEqC command, hr = 0x{0:x}", hr);
            success = false;
          }
        }
      }

      // Finalise (send) the command.
      if (success)
      {
        hr = _deviceControl.CheckChanges();
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to check DiSEqC command device control changes, hr = 0x{0:x}", hr);
          success = false;
        }
      }
      if (success)
      {
        hr = _deviceControl.CommitChanges();
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to commit DiSEqC command device control changes, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      this.LogDebug("Microsoft BDA DiSEqC: result = {0}", success);
      return success;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Microsoft BDA DiSEqC: send tone burst command, command = {0}", command);

      if (!_isMicrosoftBdaDiseqc)
      {
        this.LogWarn("Microsoft BDA DiSEqC: not initialised or interface not supported");
        return false;
      }
      if (!_isToneBurstSupported)
      {
        this.LogWarn("Microsoft BDA DiSEqC: tone burst commands not supported");
        return false;
      }

      bool success = true;
      int hr = _deviceControl.StartChanges();
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Microsoft BDA DiSEqC: failed to start tone burst command device control changes, hr = 0x{0:x}", hr);
        success = false;
      }

      // This property has to be set for each command sent for some tuners (eg. TBS).
      if (success && _setEnable)
      {
        if (_enable)
        {
          Marshal.WriteInt32(_paramBuffer, 0, 1);
        }
        else
        {
          Marshal.WriteInt32(_paramBuffer, 0, 0);
        }
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Enable, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to enable/disable tone burst commands, hr = 0x{0:x}, enable = {1}", hr, _enable);
          success = false;
        }
      }

      // Disable command repeats for optimal performance. We set this for each
      // command for "safety", assuming that if DiSEqC must be enabled for each
      // command then the same may apply to repeats.
      if (success && _setRepeatCount)
      {
        Marshal.WriteInt32(_paramBuffer, 0, _repeatCount);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.Repeats, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to set tone burst command repeat count, hr = 0x{0:x}, count = {1}", hr, _repeatCount);
          success = false;
        }
      }

      // Enable tone burst messages because we want to send a tone burst
      // instead of a regular command.
      if (success)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.UseToneBurst, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to enable tone burst commands, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      if (success)
      {
        LNB_Source lnbSource = LNB_Source.NOT_SET;
        if (command == ToneBurst.ToneBurst)
        {
          lnbSource = LNB_Source.A;
        }
        else if (command == ToneBurst.DataBurst)
        {
          lnbSource = LNB_Source.B;
        }

        Marshal.WriteInt32(_paramBuffer, 0, (int)lnbSource);
        hr = _propertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)BdaDiseqcProperty.LnbSource, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to set tone burst command LNB source, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      // Finalise (send) the command.
      if (success)
      {
        hr = _deviceControl.CheckChanges();
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to check tone burst command device control changes, hr = 0x{0:x}", hr);
          success = false;
        }
      }
      if (success)
      {
        hr = _deviceControl.CommitChanges();
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft BDA DiSEqC: failed to commit tone burst command device control changes, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      this.LogDebug("Microsoft BDA DiSEqC: result = {0}", success);
      return success;
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
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == BDA_DISEQC_MESSAGE_SIZE)
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
        Buffer.BlockCopy(message.PacketData, 0, response, 0, message.PacketLength);
        return true;
      }

      this.LogError("Microsoft BDA DiSEqC: failed to read DiSEqC response, response length = {0}, hr = 0x{1:x}", returnedByteCount, hr);
      return false;
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

    ~MicrosoftBdaDiseqc()
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
        Release.ComObject("Microsoft BDA DiSEqC property set", ref _propertySet);
        _deviceControl = null;
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
      _isMicrosoftBdaDiseqc = false;
    }

    #endregion
  }
}