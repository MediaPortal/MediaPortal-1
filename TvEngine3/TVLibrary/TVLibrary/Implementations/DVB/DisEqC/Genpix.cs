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
using TvLibrary.Hardware;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling DiSEqC for Genpix tuners using the standard BDA driver.
  /// </summary>
  public class Genpix : ICustomTuning, IDiSEqCController, IHardwareProvider, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty : int
    {
      Tune = 0,               // For custom tuning implementation.
      Diseqc,                 // For DiSEqC messaging.
      SignalStatus,           // For retrieving signal quality, strength, lock status and the actual frequency.
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

      // Tone burst (simple DiSEqc)
      ToneBurst,
      DataBurst,

      //------------------------------
      // Legacy Dish Network switches
      //------------------------------
      // SW21 - a 2-in-1 out switch.
      Sw21PortA,
      Sw21PortB,

      // SW42 - a 2 x 2-in-1 out switch with slightly different
      // switching commands to the SW21.
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

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct BdaExtensionParams
    {
      public UInt32 Frequency;              // unit = MHz
      public UInt32 LnbLowBandLof;          // unit = MHz
      public UInt32 LnbHighBandLof;         // unit = MHz
      public UInt32 LnbSwitchFrequency;     // unit = MHz
      public UInt32 SymbolRate;             // unit = ksps
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

    private const int InstanceSize = 32;

    private const int BdaExtensionParamsSize = 68;
    private const int MaxDiseqcMessageLength = 8;

    #endregion

    #region variables

    private readonly bool _isGenpix = false;
    private readonly IntPtr _generalBuffer = IntPtr.Zero;
    private readonly IntPtr _instanceBuffer = IntPtr.Zero;
    private readonly IKsPropertySet _propertySet = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Genpix"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Genpix(IBaseFilter tunerFilter)
    {
      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      KSPropertySupport support;
      _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc, out support);
      if ((support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Genpix: supported tuner detected");
        _isGenpix = true;
        _generalBuffer = Marshal.AllocCoTaskMem(BdaExtensionParamsSize);
        _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Genpix tuner using the standard BDA driver.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Genpix tuner using the standard BDA driver, otherwise <c>false</c></value>
    public bool IsGenpix
    {
      get
      {
        return _isGenpix;
      }
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Genpix: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (toneBurstState == ToneBurst.Off)
      {
        // The Genpix driver doesn't have any explicit way to control the
        // legacy tone state - the state is set based on the 3 LNB parameters.
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
        Log.Log.Debug("Genpix: result = success");
        return true;
      }

      Log.Log.Debug("Genpix: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Genpix: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      // Genpix tuners support modulation types that many other
      // tuners do not. The DSS packet format is not handled by
      // TsWriter or TsReader. I'm unsure about DC II.
      // QPSK    => DVB-S QPSK
      // 16 QAM  => Turbo FEC QPSK
      // 8 PSK   => Turbo FEC 8 PSK
      // DirecTV => DSS QPSK
      // 32 QAM  => DC II Combo
      // 64 QAM  => DC II Split (I)
      // 80 QAM  => DC II Split (Q)
      // 96 QAM  => DC II OQPSK
      if (ch.ModulationType == ModulationType.ModNotSet)
      {
        ch.ModulationType = ModulationType.ModQpsk;
      }
      Log.Log.Debug("  modulation = {0}", ch.ModulationType);
      return ch as DVBBaseChannel;
    }

    #region ICustomTuning members

    /// <summary>
    /// Check if the custom tune method supports tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the custom tune method supports tuning the channel, otherwise <c>false</c></returns>
    public bool SupportsTuningForChannel(IChannel channel)
    {
      // Tuning is only supported for DVB-S channels.
      if (channel is DVBSChannel)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a channel using the custom tune method.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scan parameters.</param>
    /// <returns><c>true</c> if tuning is successful, otherwise <c>false</c></returns>
    public bool CustomTune(IChannel channel, ScanParameters parameters)
    {
      Log.Log.Debug("Genpix: tune to channel");
      if (!SupportsTuningForChannel(channel))
      {
        Log.Log.Debug("Genpix: custom tuning not supported for this channel");
        return false;
      }

      DVBSChannel ch = channel as DVBSChannel;
      int lnbLowLof;
      int lnbHighLof;
      int lnbSwitchFrequency;
      BandTypeConverter.GetDefaultLnbSetup(parameters, ch.BandType, out lnbLowLof, out lnbHighLof, out lnbSwitchFrequency);
      BdaExtensionParams command = new BdaExtensionParams();
      command.Frequency = (uint)ch.Frequency / 1000;
      command.LnbLowBandLof = (uint)lnbLowLof;
      command.LnbHighBandLof = (uint)lnbHighLof;
      command.LnbSwitchFrequency = (uint)lnbSwitchFrequency;
      command.SymbolRate = (uint)ch.SymbolRate;
      command.Polarisation = ch.Polarisation;
      command.Modulation = ch.ModulationType;
      command.InnerFecRate = ch.InnerFecRate;
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
        Log.Log.Debug("Genpix: result = success");
        return true;
      }

      Log.Log.Debug("Genpix: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool successDiseqc = true;
      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
      if (channel.DisEqc == DisEqcType.SimpleA)
      {
        toneBurst = ToneBurst.ToneBurst;
      }
      else if (channel.DisEqc == DisEqcType.SimpleB)
      {
        toneBurst = ToneBurst.DataBurst;
      }
      else if (channel.DisEqc != DisEqcType.None)
      {
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        successDiseqc = SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }

      Tone22k tone22k = Tone22k.Off;
      if (isHighBand)
      {
        tone22k = Tone22k.On;
      }
      bool successTone = SetToneState(toneBurst, tone22k);

      return (successDiseqc && successTone);
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Genpix: send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Genpix: command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams message = new BdaExtensionParams();
      message.DiseqcMessageLength = 0;
      message.DiseqcRepeats = 0;
      message.DiseqcForceHighVoltage = false;
      message.DiseqcMessage = new byte[MaxDiseqcMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        message.DiseqcMessage[i] = command[i];
      }

      Marshal.StructureToPtr(message, _generalBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, InstanceSize,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Genpix: result = success");
        return true;
      }

      Log.Log.Debug("Genpix: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command. This function is untested.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("Genpix: read DiSEqC command");
      // Not supported...
      reply = null;
      return false;
    }

    #endregion

    #region IHardwareProvider members

    /// <summary>
    /// Initialise the hardware provider.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public void Init(IBaseFilter tunerFilter)
    {
      // Not implemented.
    }

    /// <summary>
    /// Get or set a custom device index. Not applicable for Genpix tuners.
    /// </summary>
    public int DeviceIndex
    {
      get
      {
        return 0;
      }
      set
      {
      }
    }

    /// <summary>
    /// Get or set the tuner device path. Not applicable for Genpix tuners.
    /// </summary>
    public String DevicePath
    {
      get
      {
        return "";
      }
      set
      {
      }
    }

    /// <summary>
    /// Get the provider loading priority.
    /// </summary>
    public int Priority
    {
      get
      {
        return 10;
      }
    }

    /// <summary>
    /// Checks if hardware is supported and open the device.
    /// </summary>
    public void CheckAndOpen()
    {
      // Not implemented.
    }

    /// <summary>
    /// Returns the name of the provider.
    /// </summary>
    public String Provider
    {
      get
      {
        return "Genpix";
      }
    }

    /// <summary>
    /// Returns the result of detection. If false the provider should be disposed.
    /// </summary>
    public bool IsSupported
    {
      get
      {
        return _isGenpix;
      }
    }

    /// <summary>
    /// Returns the provider capabilities.
    /// </summary>
    public CapabilitiesType Capabilities
    {
      get
      {
        return CapabilitiesType.None;
      }
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (_isGenpix)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        Marshal.FreeCoTaskMem(_instanceBuffer);
      }
    }

    #endregion
  }
}