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
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Prof tuners, including
  /// clones from Satrade and Omicom. The interface was originally a customised
  /// Conexant interface created by Turbosight, however Turbosight now use a new
  /// interface.
  /// </summary>
  public class Prof : IDiSEqCController, IDisposable
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
      /// Oscillator off.
      Off = 0,
      /// Oscillator on.
      On
    }

    /// <summary>
    /// Enum listing all possible tone burst (simple DiSEqC) messages.
    /// </summary>
    protected enum ProfToneBurst : byte
    {
      /// Tone burst (simple A).
      ToneBurst = 0,
      /// Data burst (simple B).
      DataBurst,
      /// Off (no message).
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

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
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
      public bool IsLastMessage;
      public ProfLnbPower LnbPower;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
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

    /// A buffer for general use in synchronised methods in the
    /// Prof and ProfUsb classes.
    protected IntPtr _generalBuffer = IntPtr.Zero;

    /// The device's tuner filter.
    protected IBaseFilter _tunerFilter = null;

    private IKsPropertySet _propertySet = null;

    private bool _isProf = false;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Prof"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Prof(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        Log.Log.Debug("Prof: tuner filter is null");
        return;
      }
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Log.Debug("Prof: property set is null");
        return;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Prof: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      // The original Conexant interface uses the set method; this interface uses
      // the get method.
      if ((support & KSPropertySupport.Get) != 0)
      {
        Log.Log.Debug("Prof: supported tuner detected");
        _isProf = true;
        _tunerFilter = tunerFilter;
        // Note: this buffer is shared between the Prof and ProfUsb classes. It
        // must be large enough to accomodate the largest struct from either
        // class, which at present is the BdaExtensionParams struct in the ProfUsb
        // class.
        _generalBuffer = Marshal.AllocCoTaskMem(288);

        SetLnbPowerState(true);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Prof-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Prof-compatible tuner, otherwise <c>false</c></value>
    public bool IsProf
    {
      get
      {
        return _isProf;
      }
    }

    /// <summary>
    /// Turn the LNB power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public virtual bool SetLnbPowerState(bool powerOn)
    {
      Log.Log.Debug("Prof: set LNB power state, on = {0}", powerOn);

      BdaExtensionParams command = new BdaExtensionParams();
      command.Command = BdaExtensionCommand.LnbPower;
      if (powerOn)
      {
        command.LnbPower = ProfLnbPower.On;
      }
      else
      {
        command.LnbPower = ProfLnbPower.Off;
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
        Log.Log.Debug("Prof: result = success");
        return true;
      }

      Log.Log.Debug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public virtual bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Prof: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

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
        Log.Log.Debug("Prof: result = success");
        return true;
      }

      Log.Log.Debug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Prof: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      NbcTuningParams command = new NbcTuningParams();
      // Default: tuning with "auto" is slower, so avoid it if possible.
      command.DvbsStandard = ProfDvbsStandard.Auto;

      // FEC rate
      command.InnerFecRate = ch.InnerFecRate;
      Log.Log.Debug("  inner FEC rate = {0}", command.InnerFecRate);

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
      Log.Log.Debug("  modulation     = {0}", ch.ModulationType);

      // Pilot
      if (ch.Pilot == Pilot.On)
      {
        command.Pilot = ProfPilot.On;
      }
      else
      {
        command.Pilot = ProfPilot.Off;
      }
      Log.Log.Debug("  pilot          = {0}", command.Pilot);

      // Roll-off
      if (ch.Rolloff == RollOff.Twenty)
      {
        command.RollOff = ProfRollOff.Twenty;
      }
      else if (ch.Rolloff == RollOff.TwentyFive)
      {
        command.RollOff = ProfRollOff.TwentyFive;
      }
      else if (ch.Rolloff == RollOff.ThirtyFive)
      {
        command.RollOff = ProfRollOff.ThirtyFive;
      }
      else
      {
        command.RollOff = ProfRollOff.Undefined;
      }
      Log.Log.Debug("  roll-off       = {0}", command.RollOff);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Prof: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return ch as DVBBaseChannel;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Prof: NBC tuning parameter property not supported");
        return ch as DVBBaseChannel;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, NbcTuningParamsSize);

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
        _generalBuffer, NbcTuningParamsSize,
        _generalBuffer, NbcTuningParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Prof: result = success");
      }
      else
      {
        Log.Log.Debug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return ch as DVBBaseChannel;
    }

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
      bool successDiseqc = true;
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
    public virtual bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Prof: send DiSEqC command");

      if (command.Length > MaxDiseqcTxMessageLength)
      {
        Log.Log.Debug("Prof: command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.DiseqcTransmitMessage = new byte[MaxDiseqcTxMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        propertyParams.DiseqcTransmitMessage[i] = command[i];
      }
      propertyParams.DiseqcTransmitMessageLength = (byte)command.Length;
      propertyParams.Command = BdaExtensionCommand.Diseqc;
      propertyParams.IsLastMessage = true;
      propertyParams.LnbPower = ProfLnbPower.On;
      propertyParams.ReceiveMode = ProfDiseqcReceiveMode.NoReply;

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
        Log.Log.Debug("Prof: result = success");
        return true;
      }

      Log.Log.Debug("Prof: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      // (Not implemented...)
      reply = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release COM objects and free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (_isProf)
      {
        SetLnbPowerState(false);
        Marshal.FreeCoTaskMem(_generalBuffer);
      }
      if (_propertySet != null)
      {
        Release.ComObject(_propertySet);
      }
    }

    #endregion
  }
}
