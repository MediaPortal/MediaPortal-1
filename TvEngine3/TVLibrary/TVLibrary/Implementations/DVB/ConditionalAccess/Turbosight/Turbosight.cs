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
  /// A class for handling conditional access and DiSEqC for Turbosight tuners, including
  /// clones from Prof, Satrade and Omicom.
  /// </summary>
  public class Turbosight : IDiSEqCController, ICiMenuActions, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty
    {
      DiseqcMessage = 0,    // Custom property for DiSEqC messaging.
      DiseqcInit,           // Custom property for intialising DiSEqC.
      ScanFrequency,        // (Not supported...)
      ChannelChange,        // Custom property for changing channel.
      DemodInfo,            // Custom property for returning demod FW state and version.
      EffectiveFrequency,   // (Not supported...)
      SignalStatus,         // Custom property for retrieving signal quality, strength, BER and other attributes.
      LockStatus,           // Custom property for retrieving demodulator lock indicators.
      ErrorControl,         // Custom property for controlling error correction and BER window.
      ChannelInfo,          // Custom property for retrieving the locked values of frequency, symbol rate etc. after corrections and adjustments.
      NbcParams             // Custom property for setting DVB-S2 parameters that could not initially be set through BDA interfaces.
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
    protected enum Tbs22k : byte
    {
      /// Oscillator off.
      Off = 0,
      /// Oscillator on.
      On
    }

    /// <summary>
    /// Enum listing all possible tone burst (simple DiSEqC) messages.
    /// </summary>
    protected enum TbsToneBurst : byte
    {
      /// Tone burst (simple A).
      ToneBurst = 0,
      /// Data burst (simple B).
      DataBurst,
      /// Off (no message).
      Off
    }

    private enum TbsToneModulation : uint
    {
      Undefined = 0,        // (Results in an error - *do not use*!)
      Modulated,
      Unmodulated
    }

    private enum TbsDiseqcReceiveMode : uint
    {
      Interrogation = 0,    // Expecting multiple devices attached.
      QuickReply,           // Expecting 1 response (receiving is suspended after 1st response).
      NoReply,              // Expecting no response(s).
    }

    private enum TbsPilot : uint
    {
      Off = 0,
      On,
      Unknown               // (Not used...)
    }

    private enum TbsRollOff : uint
    {
      Undefined = 0xff,
      Twenty = 0,           // 0.2
      TwentyFive,           // 0.25
      ThirtyFive            // 0.35
    }

    private enum TbsDvbsStandard : uint
    {
      Auto = 0,
      Dvbs,
      Dvbs2
    }

    private enum TbsLnbPower : uint
    {
      Off = 0,
      On
    }

    private enum TbsIrProperty
    {
      Keystrokes = 0,
      Command
    }

    /// <summary>
    /// Enum listing all possible remote button codes.
    /// </summary>
    protected enum TbsIrCode : byte
    {
      /// Recall
      Recall = 0x80,
      /// Up
      Up1 = 0x81,
      /// Right
      Right1 = 0x82,
      /// Record
      Record = 0x83,
      /// Power
      Power = 0x84,
      /// 3
      Three = 0x85,
      /// 2
      Two = 0x86,
      /// 1
      One = 0x87,
      /// Down
      Down1 = 0x88,
      /// 6
      Six = 0x89,
      /// 5
      Five = 0x8a,
      /// 4
      Four = 0x8b,
      /// Left
      Left1 = 0x8c,
      /// 9
      Nine = 0x8d,
      /// 8
      Eight = 0x8e,
      /// 7
      Seven = 0x8f,
      /// Left
      Left2 = 0x90,
      /// Up
      Up2 = 0x91,
      /// 0
      Zero = 0x92,
      /// Right
      Right2 = 0x93,
      /// Mute
      Mute = 0x94,
      /// Tab
      Tab = 0x95,
      /// Down
      Down2 = 0x96,
      /// EPG
      Epg = 0x97,
      /// Pause
      Pause = 0x98,
      /// Okay
      Ok = 0x99,
      /// Snapshot (screenshot)
      Snapshot = 0x9a,
      /// Information
      Info = 0x9c,
      /// Play
      Play = 0x9b,
      /// Toggle full screen
      FullScreen = 0x9d,
      /// Menu
      Menu = 0x9e,
      /// Exit
      Exit = 0x9f
    }

    private enum TbsMmiMessage : byte
    {
      Null = 0,
      ApplicationInfo = 0x01,     // PC <--
      CaInfo = 0x02,              // PC <--
      //CaPmt = 0x03,               // PC -->
      //CaPmtReply = 0x04,          // PC <--
      DateTimeEnquiry = 0x05,     // PC <--
      //DateTime = 0x06,            // PC -->
      Enquiry = 0x07,             // PC <--
      Answer = 0x08,              // PC -->
      EnterMenu = 0x09,           // PC -->
      Menu = 0x0a,                // PC <--
      MenuAnswer = 0x0b,          // PC -->
      List = 0x0c,                // PC <--
      GetMmi = 0x0d,              // PC <--
      CloseMmi = 0x0e,            // PC -->
      //DateTimeMode = 0x10,        // PC -->
      //SetDateTime = 0x12          // PC <--
    }

    #endregion

    #region DLL imports

    /// <summary>
    /// Open the CI interface.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="deviceName">The corresponding DsDevice name.</param>
    /// <returns>a handle that the DLL can use to identify this device for future function calls</returns>
    [DllImport("TbsCIapi.dll", EntryPoint = "On_Start_CI", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr On_Start_CI(IBaseFilter tunerFilter, String deviceName);

    /// <summary>
    /// Check whether a CAM is present in the CI slot.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    [DllImport("TbsCIapi.dll", EntryPoint = "Camavailable", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Camavailable(IntPtr handle);

    /// <summary>
    /// Exchange MMI messages with the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    /// <param name="command">The MMI command.</param>
    /// <param name="response">The MMI response.</param>
    [DllImport("TbsCIapi.dll", EntryPoint = "TBS_ci_MMI_Process", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    private static extern void TBS_ci_MMI_Process(IntPtr handle, IntPtr command, IntPtr response);

    /// <summary>
    /// Send PMT to the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    /// <param name="pmt">The PMT command.</param>
    /// <param name="pmtLength">The length of the PMT.</param>
    [DllImport("TbsCIapi.dll", EntryPoint = "TBS_ci_SendPmt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    private static extern void TBS_ci_SendPmt(IntPtr handle, IntPtr pmt, ushort pmtLength);

    /// <summary>
    /// Close the CI interface.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    [DllImport("TbsCIapi.dll", EntryPoint = "On_Exit_CI", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    private static extern void On_Exit_CI(IntPtr handle);

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NbcPropertyParams
    {
      public TbsRollOff RollOff;
      public TbsPilot Pilot;
      public TbsDvbsStandard DvbsStandard;
      public BinaryConvolutionCodeRate InnerFecRate;
      public ModulationType ModulationType;
    }

    /// <summary>
    /// For use with new TBS tuners such as the TBS6984, TBS6925... etc.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcPropertyParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
      public byte MessageLength;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcTxMessageLength)]
      public byte[] DiseqcTransmitMessage;
      public byte DiseqcTransmitMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcRxMessageLength)]
      public byte[] DiseqcReceiveMessage;
      public byte DiseqcReceiveMessageLength;
      private byte Padding1;
      private byte Padding2;
      public TbsToneModulation ToneModulation;
      public TbsDiseqcReceiveMode ReceiveMode;
      public BdaExtensionCommand Command;
      public Tbs22k Tone22k;
      public TbsToneBurst ToneBurst;
      public byte MicroControllerParityErrors;        // Parity errors: 0 indicates no errors, binary 1 indicates an error.
      public byte MicroControllerReplyErrors;         // 1 in bit i indicates error in byte i. 
      public Int32 IsLastMessage;
      public TbsLnbPower LnbPower;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);
    private static readonly Guid IrPropertySet = new Guid(0xb51c4994, 0x0054, 0x4749, 0x82, 0x43, 0x02, 0x9a, 0x66, 0x86, 0x36, 0x36);

    private const int BdaExtensionParamsSize = 188;
    private const int NbcPropertyParamsSize = 20;
    private const int DiseqcPropertyParamsSize = 11;
    private const int MaxDiseqcMessageLength = 10;
    private const byte MaxDiseqcTxMessageLength = 151;  // 3 bytes per message * 50 messages
    private const byte MaxDiseqcRxMessageLength = 9;    // reply fifo size, do not increase (hardware limitation)
    private const int MaxPmtLength = 1024;

    private const int MmiMessageBufferSize = 512;
    private const int MmiResponseBufferSize = 2048;

    private static readonly string[] TunersWithCiSlot = new string[]
    {
      "TBS 5980 CI Tuner"
      //"TBS 6928 DVBS/S2 Tuner"
      //"TBS 6992 DVBS/S2 Tuner A"
      //"TBS 6992 DVBS/S2 Tuner B"
    };

    #endregion

    #region variables

    // Buffers for use in conditional access related functions.
    private IntPtr _mmiMessageBuffer = IntPtr.Zero;
    private IntPtr _mmiResponseBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    /// A buffer for general use in synchronised methods in the
    /// Turbosight and TurbosightUsb classes.
    protected IntPtr _generalBuffer = IntPtr.Zero;

    private IntPtr _ciHandle = IntPtr.Zero;

    /// The device's tuner filter.
    protected IBaseFilter _tunerFilter = null;

    private IKsPropertySet _propertySet = null;
    private KSPropertySupport _diseqcSupportMethod = KSPropertySupport.Get;

    private bool _isTurbosight = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;

    private bool _stopMmiHandlerThread = false;
    private Thread _mmiHandlerThread = null;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Turbosight"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Turbosight(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
                                  out _diseqcSupportMethod);
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }
      // Newer tuners (TBS6984, TBS6925... etc.) support properties using the set method
      // while older tuners (TBS6980, TBS8920... etc.) use the get method. If the set
      // method is supported then it is assumed to be the correct method to use even
      // if the get method is also supported.
      // TODO: this is a problem for generic Conexant support.
      if ((_diseqcSupportMethod & KSPropertySupport.Get) != 0)
      {
        Log.Log.Debug("Turbosight: get method supported");
        _isTurbosight = true;
      }
      if ((_diseqcSupportMethod & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Turbosight: set method supported");
        _isTurbosight = true;
      }
      if (_isTurbosight)
      {
        Log.Log.Debug("Turbosight: supported tuner detected");
        _isTurbosight = true;
        _tunerFilter = tunerFilter;
        _generalBuffer = Marshal.AllocCoTaskMem(1024);
        OpenCi();
        SetLnbPowerState(true);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Turbosight-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Turbosight-compatible tuner, otherwise <c>false</c></value>
    public bool IsTurbosight
    {
      get
      {
        return _isTurbosight;
      }
    }

    /// <summary>
    /// Turn the LNB power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public virtual bool SetLnbPowerState(bool powerOn)
    {
      Log.Log.Debug("Turbosight: set LNB power state, on = {0}", powerOn);

      BdaExtensionParams command = new BdaExtensionParams();
      command.Command = BdaExtensionCommand.LnbPower;
      if (powerOn)
      {
        command.LnbPower = TbsLnbPower.On;
      }
      else
      {
        command.LnbPower = TbsLnbPower.Off;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr;
      if ((_diseqcSupportMethod & KSPropertySupport.Get) == 0)
      {
        Log.Log.Debug("Turbosight: using set method");
        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
          _generalBuffer, BdaExtensionParamsSize,
          _generalBuffer, BdaExtensionParamsSize
        );
      }
      else
      {
        Log.Log.Debug("Turbosight: using get method");
        int bytesReturned = 0;
        hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
          _generalBuffer, BdaExtensionParamsSize,
          _generalBuffer, BdaExtensionParamsSize, out bytesReturned
        );
      }
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: result = success");
        return true;
      }

      Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    protected virtual bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Turbosight: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      BdaExtensionParams command = new BdaExtensionParams();
      command.Command = BdaExtensionCommand.Tone;
      command.ToneBurst = TbsToneBurst.Off;
      command.ToneModulation = TbsToneModulation.Unmodulated;   // Can't use undefined, so use simple A instead.
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        command.ToneBurst = TbsToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        command.ToneBurst = TbsToneBurst.DataBurst;
        command.ToneModulation = TbsToneModulation.Modulated;
      }

      command.Tone22k = Tbs22k.Off;
      if (tone22kState == Tone22k.On)
      {
        command.Tone22k = Tbs22k.On;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr;
      if ((_diseqcSupportMethod & KSPropertySupport.Get) == 0)
      {
        Log.Log.Debug("Turbosight: using set method");
        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
          _generalBuffer, BdaExtensionParamsSize,
          _generalBuffer, BdaExtensionParamsSize
        );
      }
      else
      {
        Log.Log.Debug("Turbosight: using get method");
        int bytesReturned = 0;
        hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
          _generalBuffer, BdaExtensionParamsSize,
          _generalBuffer, BdaExtensionParamsSize, out bytesReturned
        );
      }
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: result = success");
        return true;
      }

      Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set DVB-S2 tuning parameters that could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with DVB-S2 parameters set.</returns>
    public DVBSChannel SetTuningParameters(DVBSChannel channel)
    {
      Log.Log.Debug("Turbosight: set tuning parameters");
      NbcPropertyParams command = new NbcPropertyParams();
      switch (channel.InnerFecRate)
      {
        case BinaryConvolutionCodeRate.Rate1_2:
        case BinaryConvolutionCodeRate.Rate2_3:
        case BinaryConvolutionCodeRate.Rate3_4:
        case BinaryConvolutionCodeRate.Rate3_5:
        case BinaryConvolutionCodeRate.Rate4_5:
        case BinaryConvolutionCodeRate.Rate5_6:
        case BinaryConvolutionCodeRate.Rate7_8:
          command.InnerFecRate = channel.InnerFecRate;
          break;
        case BinaryConvolutionCodeRate.Rate8_9:
          command.InnerFecRate = BinaryConvolutionCodeRate.Rate5_11;
          break;
        case BinaryConvolutionCodeRate.Rate9_10:
          command.InnerFecRate = BinaryConvolutionCodeRate.Rate7_8;
          break;
        default:
          command.InnerFecRate = BinaryConvolutionCodeRate.RateNotSet;
          command.DvbsStandard = TbsDvbsStandard.Auto;
          break;
      }
      Log.Log.Debug("  inner FEC rate = {0}", command.InnerFecRate);
      channel.InnerFecRate = command.InnerFecRate;

      if (command.InnerFecRate != BinaryConvolutionCodeRate.RateNotSet)
      {
        if (channel.ModulationType == ModulationType.ModNotSet)
        {
          command.ModulationType = ModulationType.ModQpsk;
          command.DvbsStandard = TbsDvbsStandard.Dvbs;
        }
        else if (channel.ModulationType == ModulationType.ModQpsk)
        {
          command.ModulationType = ModulationType.ModOqpsk;
          command.DvbsStandard = TbsDvbsStandard.Dvbs2;
        }
        else if (channel.ModulationType == ModulationType.Mod8Psk)
        {
          command.ModulationType = ModulationType.ModBpsk;
          command.DvbsStandard = TbsDvbsStandard.Dvbs2;
        }
        else
        {
          command.ModulationType = ModulationType.ModQpsk;
          command.DvbsStandard = TbsDvbsStandard.Auto;
        }
      }
      Log.Log.Debug("  modulation     = {0}", command.ModulationType);
      channel.ModulationType = command.ModulationType;
      Log.Log.Debug("  DVB standard   = {0}", command.DvbsStandard);

      if (channel.Pilot == Pilot.On)
      {
        command.Pilot = TbsPilot.On;
      }
      else
      {
        command.Pilot = TbsPilot.Off;
      }
      Log.Log.Debug("  pilot          = {0}", command.Pilot);

      if (channel.Rolloff == RollOff.Twenty)
      {
        command.RollOff = TbsRollOff.Twenty;
      }
      else if (channel.Rolloff == RollOff.TwentyFive)
      {
        command.RollOff = TbsRollOff.TwentyFive;
      }
      else if (channel.Rolloff == RollOff.ThirtyFive)
      {
        command.RollOff = TbsRollOff.ThirtyFive;
      }
      else
      {
        command.RollOff = TbsRollOff.Undefined;
      }
      Log.Log.Debug("  roll-off       = {0}", command.RollOff);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
                                  out support);
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return channel;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);

      if ((support & KSPropertySupport.Get) == 0)
      {
        Log.Log.Debug("Turbosight: using set method");
        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
          _generalBuffer, NbcPropertyParamsSize,
          _generalBuffer, NbcPropertyParamsSize
        );
      }
      else
      {
        Log.Log.Debug("Turbosight: using get method");
        int bytesReturned = 0;
        hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
          _generalBuffer, NbcPropertyParamsSize,
          _generalBuffer, NbcPropertyParamsSize, out bytesReturned
        );
      }
      DVB_MMI.DumpBinary(_generalBuffer, 0, NbcPropertyParamsSize);

      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: result = success");
      }
      else
      {
        Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return channel;
    }

    #region conditional access

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    public void OpenCi()
    {
      _isCiSlotPresent = IsCiSlotPresent();
      if (!_isCiSlotPresent)
      {
        return;
      }

      if (_ciHandle != IntPtr.Zero)
      {
        CloseCi();
      }

      Log.Log.Debug("Turbosight: open conditional access interface");
      _ciHandle = On_Start_CI(_tunerFilter, FilterGraphTools.GetFilterName(_tunerFilter));
      if (_ciHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: interface handle is null");
        _isCiSlotPresent = false;
        return;
      }
      Log.Log.Debug("Turbosight: interface opened successfully");

      _mmiMessageBuffer = Marshal.AllocCoTaskMem(MmiMessageBufferSize);
      _mmiResponseBuffer = Marshal.AllocCoTaskMem(MmiResponseBufferSize);
      _pmtBuffer = Marshal.AllocCoTaskMem(1026);  // MaxPmtSize + 2

      _isCamPresent = IsCamPresent();
      _isCamReady = IsCamReady();
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    public void CloseCi()
    {
      if (_ciHandle == IntPtr.Zero)
      {
        return;
      }

      Log.Log.Debug("Turbosight: close conditional access interface");
      _ciHandle = IntPtr.Zero;
      On_Exit_CI(_ciHandle);
      Marshal.FreeCoTaskMem(_mmiMessageBuffer);
      Marshal.FreeCoTaskMem(_mmiResponseBuffer);
      Marshal.FreeCoTaskMem(_pmtBuffer);
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    public void ResetCi()
    {
      CloseCi();
      OpenCi();
    }

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("Turbosight: is CI slot present");

      // It does not seem to be possible tell whether a CI slot is actually
      // present. All that we can check is whether the device itself physically
      // has a CI slot.
      String tunerFilterName = FilterGraphTools.GetFilterName(_tunerFilter);
      bool ciPresent = false;
      for (int i = 0; i < TunersWithCiSlot.Length; i++)
      {
        if (tunerFilterName.Equals(TunersWithCiSlot[i]))
        {
          ciPresent = true;
          break;
        }
      }
      Log.Log.Debug("Turbosight: result = {0}", ciPresent);
      return ciPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("Turbosight: is CAM present");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Turbosight: CI slot not present");
        return false;
      }
      if (_ciHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: interface not opened");
        return false;
      }

      bool camPresent = false;
      lock (this)
      {
        camPresent = Camavailable(_ciHandle);
      }
      Log.Log.Debug("Turbosight: result = {0}", camPresent);
      return camPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("Turbosight: is CAM ready");
      if (_ciHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: interface not opened");
        return false;
      }

      bool camReady = false;
      lock (this)
      {
        camReady = Camavailable(_ciHandle);
      }
      Log.Log.Debug("Turbosight: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the service is successfully descrambled, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("Turbosight: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("Turbosight: CAM not available");
        return true;
      }
      if (length > MaxPmtLength)
      {
        Log.Log.Debug("Turbosight: buffer capacity too small");
        return false;
      }

      // TBS have a short header at the start of the PMT.
      Marshal.WriteByte(_pmtBuffer, 0, (byte)listAction);
      Marshal.WriteByte(_pmtBuffer, 1, (byte)command);
      int offset = 2;
      for (int i = 0; i < length; i++)
      {
        Marshal.WriteByte(_pmtBuffer, offset, pmt[i]);
        offset++;
      }

      DVB_MMI.DumpBinary(_pmtBuffer, 0, 2 + length);

      TBS_ci_SendPmt(_ciHandle, _pmtBuffer, (ushort)(length + 2));
      return true;
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Log.Debug("Turbosight: starting new MMI handler thread");
        // Clear the message buffer in preparation for [first] use.
        for (int i = 0; i < MmiMessageBufferSize; i++)
        {
          Marshal.WriteByte(_mmiMessageBuffer, i, 0);
        }
        _stopMmiHandlerThread = false;
        _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
        _mmiHandlerThread.Name = "Turbosight MMI handler";
        _mmiHandlerThread.IsBackground = true;
        _mmiHandlerThread.Priority = ThreadPriority.Lowest;
        _mmiHandlerThread.Start();
      }
    }

    /// <summary>
    /// Thread function for handling message passing to and from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      Log.Log.Debug("Turbosight: MMI handler thread start polling");
      TbsMmiMessage message = TbsMmiMessage.Null;
      TbsMmiMessage prevMessage = TbsMmiMessage.Null;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          bool newState;
          lock (this)
          {
            newState = Camavailable(_ciHandle);
          }
          if (newState != _isCamPresent)
          {
            _isCamPresent = newState;
            Log.Log.Debug("Turbosight: CAM state change, CAM present = {0}", _isCamPresent);
          }

          // Do we have a message to send?
          prevMessage = message;
          message = TbsMmiMessage.Null;
          lock (this)
          {
            message = (TbsMmiMessage)Marshal.ReadByte(_mmiMessageBuffer, 0);
          }

          // No -> sleep and continue.
          if (!_isCamPresent || message == TbsMmiMessage.Null)
          {
            Thread.Sleep(2000);
            continue;
          }

          // Yes -> clear the response buffer if this is a new message and send it.
          if (message != prevMessage)
          {
            for (int i = 0; i < MmiResponseBufferSize; i++)
            {
              Marshal.WriteByte(_mmiResponseBuffer, i, 0);
            }
          }
          lock (this)
          {
            TBS_ci_MMI_Process(_ciHandle, _mmiMessageBuffer, _mmiResponseBuffer);
          }

          // Explicitly request a response if necessary. This is done by sending "get MMI" messages until a
          // response is received.
          if (message == TbsMmiMessage.EnterMenu || message == TbsMmiMessage.MenuAnswer || message == TbsMmiMessage.Answer)
          {
            prevMessage = message;
            message = TbsMmiMessage.GetMmi;
            lock (this)
            {
              Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.GetMmi);
              TBS_ci_MMI_Process(_ciHandle, _mmiMessageBuffer, _mmiResponseBuffer);
            }
          }

          // Check for a response.
          TbsMmiMessage response = TbsMmiMessage.Null;
          response = (TbsMmiMessage)Marshal.ReadByte(_mmiResponseBuffer, 4);
          if (response == TbsMmiMessage.Null)
          {
            // Responses don't always arrive quickly so give the CAM time to respond if
            // the response isn't ready yet.
            Thread.Sleep(2000);
            continue;
          }

          Log.Log.Debug("Turbosight: received MMI response {0} to message {1}", response, message);

          // Get the response bytes.
          byte lsb = Marshal.ReadByte(_mmiResponseBuffer, 5);
          byte msb = Marshal.ReadByte(_mmiResponseBuffer, 6);
          int length = (256 * msb) + lsb;
          if (length > MmiResponseBufferSize - 7)
          {
            Log.Log.Debug("Turbosight: response too long, length = {0}", length);
            // We know we haven't got the complete response (DLL internal buffer overflow),
            // so wipe the message buffer and give up on this message.
            lock (this)
            {
              Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.Null);
            }
            continue;
          }

          Log.Log.Debug("Turbosight: response length = {0}", length);
          byte[] responseBytes = new byte[length];
          int j = 7;
          for (int i = 0; i < length; i++)
          {
            responseBytes[i] = Marshal.ReadByte(_mmiResponseBuffer, j);
            j++;
          }

          if (response == TbsMmiMessage.ApplicationInfo)
          {
            HandleApplicationInformation(responseBytes, length);

            // Special case (see EnterCIMenu()).
            lock (this)
            {
              Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.CaInfo);
            }
            continue;
          }
          else if (response == TbsMmiMessage.CaInfo)
          {
            HandleCaInformation(responseBytes, length);

            // Special case (see EnterCIMenu()).
            lock (this)
            {
              Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.EnterMenu);
            }
            continue;
          }
          else if (response == TbsMmiMessage.Menu)
          {
            HandleMenu(responseBytes, length);
          }
          else if (response == TbsMmiMessage.Enquiry)
          {
            HandleEnquiry(responseBytes, length);
          }
          else
          {
            Log.Log.Debug("Turbosight: unexpected response");
            DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
          }

          // Clear the message buffer since a request has been handled.
          // Intermediate requests will be lost, but there is nothing
          // we can do about that - they are most likely invalid anyway.
          lock (this)
          {
            Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.Null);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Turbosight: error in MMI handler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    private void HandleApplicationInformation(byte[] content, int length)
    {
      Log.Log.Debug("Turbosight: application information");
      if (length < 5)
      {
        Log.Log.Debug("Turbosight: error, response too short");
        DVB_MMI.DumpBinary(content, 0, length);
        return;
      }
      DVB_MMI.ApplicationType type = (DVB_MMI.ApplicationType)content[0];
      int manufacturer = (content[1] << 8) | content[2];
      int code = (content[3] << 8) | content[4];
      String text = DVB_MMI.BytesToString(content, 5, length - 5);
      Log.Log.Debug("  type         = {0}", type);
      Log.Log.Debug("  manufacturer = 0x{0:x}", manufacturer);
      Log.Log.Debug("  code         = 0x{0:x}", code);
      Log.Log.Debug("  information  = {0}", text);
    }

    private void HandleCaInformation(byte[] content, int length)
    {
      Log.Log.Debug("Turbosight: conditional access information");
      if (length == 0)
      {
        Log.Log.Debug("Turbosight: error, response too short");
        return;
      }
      int numCaIds = content[0];
      Log.Log.Debug("  # CASIDs = {0}", numCaIds);
      int i = 0;
      int l = 1;
      while (l + 2 <= length)
      {
        Log.Log.Debug("  {0,-2}       = 0x{1:x2}{2:x2}", i, content[l + 1], content[l]);
        l += 2;
        i++;
      }
      if (length != ((numCaIds * 2) + 1))
      {
        Log.Log.Debug("Turbosight: error, unexpected numCaIds");
        //DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
      }
    }

    private void HandleMenu(byte[] content, int length)
    {
      Log.Log.Debug("Turbosight: menu");
      if (length == 0)
      {
        Log.Log.Debug("Turbosight: error, response too short");
        return;
      }
      int numChoices = content[0];
      List<String> entries = new List<String>();
      String entry = "";
      int entryCount = 0;
      for (int i = 1; i < length; i++)
      {
        if (content[i] == 0)
        {
          entries.Add(entry);
          entryCount++;
          entry = "";
        }
        else
        {
          if (content[i] >= 32 && content[i] <= 126)
          {
            entry += (char)content[i];
          }
          else
          {
            entry += ' ';
          }
        }
      }
      entries.Add(entry);
      entryCount -= 2;
      if (entryCount < 0)
      {
        Log.Log.Debug("Turbosight: error, not enough menu data");
        DVB_MMI.DumpBinary(content, 0, length);
        return;
      }

      Log.Log.Debug("  title     = {0}", entries[0]);
      Log.Log.Debug("  sub-title = {0}", entries[1]);
      Log.Log.Debug("  footer    = {0}", entries[2]);
      Log.Log.Debug("  # choices = {0}", numChoices);
      if (_ciMenuCallbacks != null)
      {
        _ciMenuCallbacks.OnCiMenu(entries[0], entries[1], entries[2], entryCount);
      }
      for (int i = 0; i < entryCount; i++)
      {
        Log.Log.Debug("  choice {0}  = {1}", i, entries[i + 3]);
        if (_ciMenuCallbacks != null)
        {
          _ciMenuCallbacks.OnCiMenuChoice(i, entries[i + 3]);
        }
      }

      // There seems to be a bug in the DLL code - sometimes the response doesn't
      // contain the full set of choices.
      if (entryCount != numChoices)
      {
        Log.Log.Debug("Turbosight: error, numChoices != entryCount");
        //DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
      }
    }

    private void HandleEnquiry(byte[] content, int length)
    {
      Log.Log.Debug("Turbosight: enquiry");
      if (length < 3)
      {
        Log.Log.Debug("Turbosight: error, response too short");
        DVB_MMI.DumpBinary(content, 0, length);
        return;
      }
      bool blind = (content[0] != 0);
      uint answerLength = content[1];
      String text = DVB_MMI.BytesToString(content, 2, length - 2);
      Log.Log.Debug("  text   = {0}", text);
      Log.Log.Debug("  length = {0}", answerLength);
      Log.Log.Debug("  blind  = {0}", blind);
      if (_ciMenuCallbacks != null)
      {
        _ciMenuCallbacks.OnCiRequest(blind, answerLength, text);
      }
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Sets the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        StartMmiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      if (!_isCiSlotPresent || !_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: enter menu");

      // We send an "application info" message here because attempting to enter the menu will fail
      // if you don't get the application information first. On seeing this message the MMI handler
      // thread does the following:
      // 1. Send the application information request.
      // 2. Parse and log the application information response if/when it arrives.
      // 3. Send a CA information request.
      // 4. Parse and log the CA information response if/when it arrives.
      // 5. Send an enter menu request.
      // 6. Parse the response and execute callbacks as necessary.
      lock (this)
      {
        Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.ApplicationInfo);
      }
      return true;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCiSlotPresent || !_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: close menu");
      lock (this)
      {
        Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.CloseMmi);
      }
      return true;
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCiSlotPresent || !_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: select menu entry, choice = {0}", choice);
      lock (this)
      {
        Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.MenuAnswer);
        Marshal.WriteByte(_mmiMessageBuffer, 3, choice);
      }
      return true;
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (!_isCiSlotPresent || !_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = "";
      }
      Log.Log.Debug("Turbosight: send menu answer, answer = {0}, cancel = {1}", answer, cancel);
      byte lsb = (byte)((answer.Length + 1) % 256);
      byte msb = (byte)((answer.Length + 1) / 256);
      byte responseType = (byte)DVB_MMI.ResponseType.Answer;
      if (cancel)
      {
        responseType = (byte)DVB_MMI.ResponseType.Cancel;
      }
      lock (this)
      {
        Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.Answer);
        Marshal.WriteByte(_mmiMessageBuffer, 1, lsb);
        Marshal.WriteByte(_mmiMessageBuffer, 2, msb);
        Marshal.WriteByte(_mmiMessageBuffer, 3, responseType);
        for (int i = 0; i < answer.Length; i++)
        {
          Marshal.WriteByte(_mmiMessageBuffer, 4 + i, (byte)answer[i]);
        }
      }
      return true;
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
      Log.Log.Debug("Turbosight: send DiSEqC command");

      int hr;
      // TBS6984, 6925... etc.
      if ((_diseqcSupportMethod & KSPropertySupport.Get) == 0)
      {
        Log.Log.Debug("Turbosight: using set method");

        if (command.Length > MaxDiseqcMessageLength)
        {
          Log.Log.Debug("Turbosight: command too long, length = {0}", command.Length);
          return false;
        }

        DiseqcPropertyParams propertyParams = new DiseqcPropertyParams();
        propertyParams.Message = new byte[MaxDiseqcMessageLength];
        for (int i = 0; i < command.Length; i++)
        {
          propertyParams.Message[i] = command[i];
        }
        propertyParams.MessageLength = (byte)command.Length;

        Marshal.StructureToPtr(propertyParams, _generalBuffer, true);
        DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcPropertyParamsSize);

        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
          _generalBuffer, DiseqcPropertyParamsSize,
          _generalBuffer, DiseqcPropertyParamsSize
        );
      }
      // TBS6980, 6981, 8920... etc.
      else
      {
        Log.Log.Debug("Turbosight: using get method");

        if (command.Length > MaxDiseqcTxMessageLength)
        {
          Log.Log.Debug("Turbosight: command too long, length = {0}", command.Length);
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
        propertyParams.IsLastMessage = 1;
        propertyParams.LnbPower = TbsLnbPower.On;
        propertyParams.ReceiveMode = TbsDiseqcReceiveMode.NoReply;

        Marshal.StructureToPtr(propertyParams, _generalBuffer, true);
        DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

        int bytesReturned = 0;
        hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
          _generalBuffer, BdaExtensionParamsSize,
          _generalBuffer, BdaExtensionParamsSize, out bytesReturned
        );
      }

      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: result = success");
        return true;
      }

      Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      reply = new byte[1];
      reply[0] = 0;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close the conditional access interface and free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (_isTurbosight)
      {
        SetLnbPowerState(false);
        if (_mmiHandlerThread != null)
        {
          _stopMmiHandlerThread = true;
          Thread.Sleep(3000);
        }
        CloseCi();
        Marshal.FreeCoTaskMem(_generalBuffer);
      }
      Release.ComObject(_propertySet);
    }

    #endregion
  }
}
