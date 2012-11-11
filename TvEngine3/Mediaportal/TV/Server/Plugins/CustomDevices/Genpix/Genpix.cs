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

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Genpix
{
  /// <summary>
  /// A class for handling DiSEqC for Genpix tuners using the standard BDA driver.
  /// </summary>
  public class Genpix : BaseCustomDevice, ICustomTuner, IDiseqcDevice
  {
 
    #region enums

    private enum BdaExtensionProperty : int
    {
      Tune = 0,               // For custom tuning implementation.
      Diseqc,                 // For DiSEqC messaging.
      SignalStatus,           // For retrieving signal quality, strength, lock status and the actual lock frequency.
    }

    private enum GenpixToneBurst : byte
    {
      ToneBurst = 0,
      DataBurst
    }

    private enum GenpixSwitchPort : uint
    {
      None = 0,

      // DiSEqC 1.0
      PortA,
      PortB,
      PortC,
      PortD,

      // Tone burst (simple DiSEqC)
      ToneBurst,
      DataBurst,

      //------------------------------
      // Legacy Dish Network switches
      //------------------------------
      // SW21 - a 2-in-1 out switch.
      Sw21PortA,
      Sw21PortB,

      // SW42 - a 2 x 2-in-1 out (ie. 2 satellites, 2 independent
      // receivers) switch with slightly different switching
      // commands to the SW21.
      Sw42PortA,
      Sw42PortB,

      // SW44???
      SW44PortB,

      // SW64 - a 6-in-4 out switch, usually used for connecting
      // 3 satellites (both polarities) to 4 independent receivers.
      Sw64PortA_Odd,
      Sw64PortA_Even,
      Sw64PortB_Odd,
      Sw64PortB_Even,
      Sw64PortC_Odd,
      Sw64PortC_Even,

      // Twin LNB - a dual head LNB with multiple independent outputs.
      TwinLnbSatA,
      TwinLnbSatB,

      // Quad LNB???
      QuadLnbSatB
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    private struct BdaExtensionParams
    {
      public UInt32 Frequency;              // unit = MHz
      public UInt32 LnbLowBandLof;          // unit = MHz
      public UInt32 LnbHighBandLof;         // unit = MHz
      public UInt32 LnbSwitchFrequency;     // unit = MHz
      public UInt32 SymbolRate;             // unit = ks/s
      public Polarisation Polarisation;
      public ModulationType Modulation;
      public BinaryConvolutionCodeRate InnerFecRate;
      public GenpixSwitchPort SwitchPort;

      public UInt32 DiseqcRepeats;          // Set to zero to send once, one to send twice, two to send three times etc.

      public UInt32 DiseqcMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] DiseqcMessage;
      public bool DiseqcForceHighVoltage;

      public UInt32 SignalStrength;         // range = 0 - 100%
      public UInt32 SignalQuality;          // range = 0 - 100%
      public bool SignalIsLocked;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xdf981009, 0x0d8a, 0x430e, 0xa8, 0x03, 0x17, 0xc5, 0x14, 0xdc, 0x8e, 0xc0);

    private const int InstanceSize = 32;    // The size of a property instance (KSP_NODE) parameter.

    private const int BdaExtensionParamsSize = 68;
    private const int MaxDiseqcMessageLength = 8;

    #endregion

    #region variables

    private bool _isGenpix = false;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    #region ICustomDevice members

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
      this.LogDebug("Genpix: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("Genpix: tuner filter is null");
        return false;
      }
      if (_isGenpix)
      {
        this.LogDebug("Genpix: device is already initialised");
        return true;
      }

      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Genpix: tuner filter is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Genpix: device does not support the Genpix property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Genpix: supported device detected");
      _isGenpix = true;
      _generalBuffer = Marshal.AllocCoTaskMem(BdaExtensionParamsSize);
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
      this.LogDebug("Genpix: on before tune callback");
      action = DeviceAction.Default;

      if (!_isGenpix)
      {
        this.LogDebug("Genpix: device not initialised or interface not supported");
        return;
      }

      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      // Genpix tuners support modulation types that many other tuners do not. Their support of DC II  makes them very attractive
      // for North American people trying to receive Dish TV on HTPCs. Aparently there are some Twinhan and DVB
      // World tuners that also support some of the turbo schemes. However their SDKs don't specify the
      // details. The TeVii SDK does specify modulation mappings for turbo schemes, but I don't know if the
      // hardware actually supports them. The Conexant SDK also specifies support for some DC II modulation and
      // FEC schemes.
      // We don't specifically support turbo or DC II modulation schemes in our tuning details, but we at least
      // try to use a common mapping in our plugin code. We won't enforce mapping conversions in plugins to allow
      // as much user flexibility as possible.

      // Genpix driver mappings are as follows [BDA ModulationType => hardware/driver modulation]:
      // QPSK     => DVB-S QPSK
      // 16 QAM   => turbo FEC QPSK
      // 8 PSK    => turbo FEC 8 PSK
      // DirecTV  => DSS QPSK
      // 32 QAM   => DC II combo
      // 64 QAM   => DC II split (I)
      // 80 QAM   => DC II split (Q)
      // 96 QAM   => DC II offset QPSK

      // MediaPortal TV Server mappings in the context of satellite tuning details are as follows:
      // DVB-S
      //-------
      // not set  => default, DVB-S QPSK

      // DVB-SNG
      //---------
      // BPSK     => DVB-SNG BPSK
      // 16 QAM   => DVB-SNG 16 QAM
      // 32 QAM   => DVB-SNG 8 PSK

      // DVB-S2
      //--------
      // QPSK     => non-backwards compatible DVB-S2 QPSK
      // 8 PSK    => non-backwards compatible DVB-S2 8 PSK
      // 16 APSK  => DVB-S2 16 APSK
      // 32 APSK  => DVB-S2 32 APSK

      // DSS (DirecTV)
      //---------------
      // DirecTV  => DSS QPSK

      // DC II
      //-------
      // 768 QAM  => DC II combo
      // 896 QAM  => DC II split (I)
      // 1024 QAM => DC II split (Q)
      // O-QPSK   => DC II offset QPSK

      // Turbo FEC
      //-----------
      // 64 QAM  => turbo FEC QPSK
      // 80 QAM  => turbo FEC 8 PSK
      // 160 QAM => turbo FEC 16 PSK

      // Note: the DSS packet format used by North American DirecTV uses a packet format which is completely
      // different from ISO MPEG 2. It is not currently supported by TsWriter or TsReader. DC II is more similar
      // to MPEG 2 but I'm unsure if TsWriter and TsReader fully support it.

      if (ch.ModulationType == ModulationType.Mod64Qam)
      {
        ch.ModulationType = ModulationType.Mod16Qam;
      }
      else if (ch.ModulationType == ModulationType.Mod80Qam)
      {
        ch.ModulationType = ModulationType.Mod8Psk;
      }
      else if (ch.ModulationType == ModulationType.Mod768Qam)
      {
        ch.ModulationType = ModulationType.Mod32Qam;
      }
      else if (ch.ModulationType == ModulationType.Mod896Qam)
      {
        ch.ModulationType = ModulationType.Mod64Qam;
      }
      else if (ch.ModulationType == ModulationType.Mod1024Qam)
      {
        ch.ModulationType = ModulationType.Mod80Qam;
      }
      else if (ch.ModulationType == ModulationType.ModOqpsk)
      {
        ch.ModulationType = ModulationType.Mod96Qam;
      }
      this.LogDebug("  modulation = {0}", ch.ModulationType);
    }

    #endregion

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the device implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      // This plugin only supports satellite tuners. As such, tuning is only supported for satellite channels.
      if (channel is DVBSChannel)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("Genpix: tune to channel");

      if (!_isGenpix || _propertySet == null)
      {
        this.LogDebug("Genpix: device not initialised or interface not supported");
        return false;
      }
      if (!CanTuneChannel(channel))
      {
        this.LogDebug("Genpix: tuning is not supported for this channel");
        return false;
      }

      DVBSChannel dvbsChannel = channel as DVBSChannel;
      BdaExtensionParams command = new BdaExtensionParams();
      command.Frequency = (UInt32)dvbsChannel.Frequency / 1000;
      command.LnbLowBandLof = (UInt32)dvbsChannel.LnbType.LowBandFrequency / 1000;
      command.LnbHighBandLof = (UInt32)dvbsChannel.LnbType.HighBandFrequency / 1000;
      command.LnbSwitchFrequency = (UInt32)dvbsChannel.LnbType.SwitchFrequency / 1000;
      command.SymbolRate = (UInt32)dvbsChannel.SymbolRate;
      command.Polarisation = dvbsChannel.Polarisation;
      command.Modulation = dvbsChannel.ModulationType;
      command.InnerFecRate = dvbsChannel.InnerFecRate;
      command.SwitchPort = GenpixSwitchPort.None;
      command.DiseqcRepeats = 0;

      Marshal.StructureToPtr(command, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tune,
        _instanceBuffer, InstanceSize,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      this.LogDebug("Genpix: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <remarks>
    /// The Genpix interface does not support directly setting the 22 kHz tone state. The tuning request
    /// LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Genpix: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isGenpix || _propertySet == null)
      {
        this.LogDebug("Genpix: device not initialised or interface not supported");
        return false;
      }

      if (toneBurstState == ToneBurst.None)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      // The driver interprets sending a DiSEqC message with length zero as
      // a tone burst command.
      BdaExtensionParams command = new BdaExtensionParams();
      command.DiseqcMessageLength = 0;
      command.DiseqcRepeats = 0;
      command.DiseqcForceHighVoltage = false;
      command.DiseqcMessage = new byte[MaxDiseqcMessageLength];
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        command.DiseqcMessage[0] = (byte)GenpixToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        command.DiseqcMessage[0] = (byte)GenpixToneBurst.DataBurst;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, InstanceSize,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      this.LogDebug("Genpix: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("Genpix: send DiSEqC command");

      if (!_isGenpix || _propertySet == null)
      {
        this.LogDebug("Genpix: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("Genpix: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        this.LogDebug("Genpix: command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams message = new BdaExtensionParams();
      message.DiseqcMessageLength = (uint)command.Length;
      message.DiseqcRepeats = 0;
      message.DiseqcForceHighVoltage = true;
      message.DiseqcMessage = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, message.DiseqcMessage, 0, command.Length);

      Marshal.StructureToPtr(message, _generalBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, InstanceSize,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      this.LogDebug("Genpix: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      // Not supported.
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
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      _propertySet = null;
      _isGenpix = false;
    }

    #endregion
  }
}