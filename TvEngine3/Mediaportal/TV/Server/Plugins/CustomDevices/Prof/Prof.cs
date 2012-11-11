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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Prof
{
  /// <summary>
  /// A class for handling DiSEqC for Prof devices, including clones from Satrade and Omicom. The interface
  /// was originally a customised Conexant interface created by Turbosight, however Turbosight have
  /// implemented a new unified interface for their products.
  /// </summary>
  public class Prof : BaseCustomDevice, IPowerDevice, IDiseqcDevice
  {


    #region enums

    private enum BdaExtensionProperty
    {
      DiseqcMessage = 0,    // For DiSEqC messaging.
      DiseqcInit,           // For intialising DiSEqC.
      ScanFrequency,        // (Not supported...)
      ChannelChange,        // For changing channel.
      DemodInfo,            // For returning demodulator firmware state and version.
      EffectiveFrequency,   // (Not supported...)
      SignalStatus,         // For retrieving signal quality, strength, BER and other attributes.
      LockStatus,           // For retrieving demodulator lock indicators.
      ErrorControl,         // For controlling error correction and BER window.
      ChannelInfo,          // For retrieving the locked values of frequency, symbol rate etc. after corrections and adjustments.
      NbcParams             // For setting DVB-S2 parameters that could not initially be set through BDA interfaces.
    }

    private enum BdaExtensionCommand : uint
    {
      LnbPower = 0,
      Motor,
      Tone,
      Diseqc
    }

    /// <summary>
    /// Enum listing all possible 22 kHz oscillator states.
    /// </summary>
    protected enum Prof22k : byte
    {
      /// <summary>
      /// Oscillator off.
      /// </summary>
      Off = 0,
      /// <summary>
      /// Oscillator on.
      /// </summary>
      On
    }

    /// <summary>
    /// Enum listing all possible tone burst (simple DiSEqC) messages.
    /// </summary>
    protected enum ProfToneBurst : byte
    {
      /// <summary>
      /// Tone burst (simple A).
      /// </summary>
      ToneBurst = 0,
      /// <summary>
      /// Data burst (simple B).
      /// </summary>
      DataBurst,
      /// <summary>
      /// Off (no message).
      /// </summary>
      Off
    }

    private enum ProfToneModulation : uint
    {
      Undefined = 0,        // (Results in an error - *do not use*!)
      Modulated,
      Unmodulated
    }

    private enum ProfDiseqcReceiveMode : uint
    {
      Interrogation = 0,    // Expecting multiple devices attached.
      QuickReply,           // Expecting one response (receiving is suspended after first response).
      NoReply,              // Expecting no response(s).
    }

    private enum ProfPilot : uint
    {
      Off = 0,
      On,
      Unknown               // (Not used...)
    }

    private enum ProfRollOff : uint
    {
      Undefined = 0xff,
      Twenty = 0,           // 0.2
      TwentyFive,           // 0.25
      ThirtyFive            // 0.35
    }

    private enum ProfDvbsStandard : uint
    {
      Auto = 0,
      Dvbs,
      Dvbs2
    }

    private enum ProfLnbPower : uint
    {
      Off = 0,
      On
    }

    private enum ProfIrProperty
    {
      Keystrokes = 0,
      Command
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcTxMessageLength)]
      public byte[] DiseqcTransmitMessage;
      public byte DiseqcTransmitMessageLength;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcRxMessageLength)]
      public byte[] DiseqcReceiveMessage;
      public byte DiseqcReceiveMessageLength;
      private UInt16 Padding;

      public ProfToneModulation ToneModulation;
      public ProfDiseqcReceiveMode ReceiveMode;

      public BdaExtensionCommand Command;
      public Prof22k Tone22k;
      public ProfToneBurst ToneBurst;
      public byte MicroControllerParityErrors;        // Parity errors: 0 indicates no errors, binary 1 indicates an error.
      public byte MicroControllerReplyErrors;         // 1 in bit i indicates error in byte i.

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;
      public ProfLnbPower LnbPower;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NbcTuningParams
    {
      public ProfRollOff RollOff;
      public ProfPilot Pilot;
      public ProfDvbsStandard DvbsStandard;
      public BinaryConvolutionCodeRate InnerFecRate;
      public ModulationType ModulationType;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    private const int BdaExtensionParamsSize = 188;
    private const int NbcTuningParamsSize = 20;
    private const byte MaxDiseqcTxMessageLength = 151;  // 3 bytes per message * 50 messages
    private const byte MaxDiseqcRxMessageLength = 9;    // reply fifo size, do not increase (hardware limitation)

    #endregion

    #region variables

    /// <summary>
    /// A buffer for general use in synchronised methods in the Prof and ProfUsb classes.
    /// </summary>
    protected IntPtr _generalBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;

    private bool _isProf = false;

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // TeVii, Hauppauge, Geniatech, Turbosight, DVBSky, Prof and possibly others all use or implement the
        // same Conexant property set for DiSEqC support, often adding custom extensions. In order to ensure
        // that the full device functionality is available for all hardware we use the following priority
        // hierarchy:
        // TeVii [75] > Hauppauge, DVBSky, Turbosight [70] > Prof (USB) [65] > Prof (PCI, PCIe) [60] > Geniatech [50] > Conexant [40]
        return 60;
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
      this.LogDebug("Prof: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("Prof: tuner filter is null");
        return false;
      }
      if (_isProf)
      {
        this.LogDebug("Prof: device is already initialised");
        return true;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Prof: pin is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage, out support);
      // The original Conexant interface uses the set method; this interface uses the get method.
      if (hr != 0 || (support & KSPropertySupport.Get) == 0)
      {
        this.LogDebug("Prof: device does not support the Prof property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Prof: supported device detected");
      _isProf = true;
      // Note: this buffer is shared between the Prof and ProfUsb classes. It must be large enough to
      // accomodate the largest struct from either class. At present the largest struct is the
      // BdaExtensionParams struct in the ProfUsb class.
      _generalBuffer = Marshal.AllocCoTaskMem(288);
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
      this.LogDebug("Prof: on before tune callback");
      action = DeviceAction.Default;

      if (!_isProf || _propertySet == null)
      {
        this.LogDebug("Prof: device not initialised or interface not supported");
        return;
      }

      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      NbcTuningParams command = new NbcTuningParams();
      // Default: tuning with "auto" is slower, so avoid it if possible.
      command.DvbsStandard = ProfDvbsStandard.Auto;

      // FEC rate
      command.InnerFecRate = ch.InnerFecRate;
      this.LogDebug("  inner FEC rate = {0}", command.InnerFecRate);

      // Modulation
      if (ch.ModulationType == ModulationType.ModNotSet)
      {
        ch.ModulationType = ModulationType.ModQpsk;
        command.DvbsStandard = ProfDvbsStandard.Dvbs;
      }
      else if (ch.ModulationType == ModulationType.ModQpsk)
      {
        ch.ModulationType = ModulationType.ModNbcQpsk;
        command.DvbsStandard = ProfDvbsStandard.Dvbs2;
      }
      else if (ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.ModNbc8Psk;
        command.DvbsStandard = ProfDvbsStandard.Dvbs2;
      }
      command.ModulationType = ch.ModulationType;
      this.LogDebug("  modulation     = {0}", ch.ModulationType);

      // Pilot
      command.Pilot = ProfPilot.Off;
      if (ch.Pilot == Pilot.On)
      {
        command.Pilot = ProfPilot.On;
      }
      this.LogDebug("  pilot          = {0}", command.Pilot);

      // Roll-off
      if (ch.RollOff == RollOff.Twenty)
      {
        command.RollOff = ProfRollOff.Twenty;
      }
      else if (ch.RollOff == RollOff.TwentyFive)
      {
        command.RollOff = ProfRollOff.TwentyFive;
      }
      else if (ch.RollOff == RollOff.ThirtyFive)
      {
        command.RollOff = ProfRollOff.ThirtyFive;
      }
      else
      {
        command.RollOff = ProfRollOff.Undefined;
      }
      this.LogDebug("  roll-off       = {0}", command.RollOff);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Prof: NBC tuning parameter property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, NbcTuningParamsSize);

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
        _generalBuffer, NbcTuningParamsSize,
        _generalBuffer, NbcTuningParamsSize
      );
      if (hr == 0)
      {
        this.LogDebug("Prof: result = success");
      }
      else
      {
        this.LogDebug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public virtual bool SetPowerState(bool powerOn)
    {
      this.LogDebug("Prof: set power state, on = {0}", powerOn);

      if (!_isProf || _propertySet == null)
      {
        this.LogDebug("Prof: device not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.Command = BdaExtensionCommand.LnbPower;
      command.LnbPower = ProfLnbPower.Off;
      if (powerOn)
      {
        command.LnbPower = ProfLnbPower.On;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
         _generalBuffer, BdaExtensionParamsSize,
         _generalBuffer, BdaExtensionParamsSize,
         out returnedByteCount
      );
      if (hr == 0)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogDebug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public virtual bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Prof: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isProf || _propertySet == null)
      {
        this.LogDebug("Prof: device not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.Command = BdaExtensionCommand.Tone;
      command.ToneBurst = ProfToneBurst.Off;
      command.ToneModulation = ProfToneModulation.Unmodulated;   // Can't use undefined, so use simple A instead.
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        command.ToneBurst = ProfToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        command.ToneBurst = ProfToneBurst.DataBurst;
        command.ToneModulation = ProfToneModulation.Modulated;
      }

      command.Tone22k = Prof22k.Off;
      if (tone22kState == Tone22k.On)
      {
        command.Tone22k = Prof22k.On;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
        _generalBuffer, BdaExtensionParamsSize,
        _generalBuffer, BdaExtensionParamsSize,
        out returnedByteCount
      );
      if (hr == 0)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogDebug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public virtual bool SendCommand(byte[] command)
    {
      this.LogDebug("Prof: send DiSEqC command");

      if (!_isProf || _propertySet == null)
      {
        this.LogDebug("Prof: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("Prof: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcTxMessageLength)
      {
        this.LogDebug("Prof: command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.DiseqcTransmitMessage = new byte[MaxDiseqcTxMessageLength];
      Buffer.BlockCopy(command, 0, propertyParams.DiseqcTransmitMessage, 0, command.Length);
      propertyParams.DiseqcTransmitMessageLength = (byte)command.Length;
      propertyParams.ReceiveMode = ProfDiseqcReceiveMode.NoReply;
      propertyParams.Command = BdaExtensionCommand.Diseqc;
      propertyParams.IsLastMessage = true;
      propertyParams.LnbPower = ProfLnbPower.On;

      Marshal.StructureToPtr(propertyParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
        _generalBuffer, BdaExtensionParamsSize,
        _generalBuffer, BdaExtensionParamsSize,
        out returnedByteCount
      );
      if (hr == 0)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogDebug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      // Not implemented.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (_propertySet != null)
      {
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
      }
      _isProf = false;
    }

    #endregion
  }
}
