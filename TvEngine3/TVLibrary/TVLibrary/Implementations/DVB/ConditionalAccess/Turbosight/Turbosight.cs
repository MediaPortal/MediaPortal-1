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
  /// A class for handling conditional access and DiSEqC for Turbosight tuners. Note
  /// that Turbosight drivers seem to still support the original Conexant, NXP and
  /// Cyprus interfaces/structures. However, it is simpler and probably more
  /// future-proof to stick with the information in the published SDK.
  /// </summary>
  public class Turbosight : IDiSEqCController, ICiMenuActions, IDisposable
  {
    #region enums

    // PCIe/PCI only.
    private enum BdaExtensionProperty
    {
      Reserved = 0,
      NbcParams = 10,     // Property for setting DVB-S2 parameters that could not initially be set through BDA interfaces.
      BlindScan = 11,     // Property for accessing and controlling the hardware blind scan capabilities.
      TbsAccess = 21      // TBS property for enabling control of the common properties in the BdaExtensionCommand enum.
    }

    // USB (QBOX) only.
    private enum UsbBdaExtensionProperty
    {
      Reserved = 0,
      Ir = 1,             // Property for retrieving IR codes from the device's IR receiver.
      BlindScan = 9,      // Property for accessing and controlling the hardware blind scan capabilities.
      TbsAccess = 18      // TBS property for enabling control of the common properties in the TbsAccessMode enum.
    }

    // Common properties that can be controlled on all TBS products.
    private enum TbsAccessMode : uint
    {
      LnbPower = 0,       // Control the LNB power supply.
      Diseqc,             // Send and receive DiSEqC messages.
      Tone                // Control the 22 kHz oscillator state.
    }

    private enum TbsLnbPower : uint
    {
      Off = 0,
      High,               // 18 V - linear horizontal, circular left.
      Low,                // 13 V - linear vertical, circular right.
      On                  // Power on using the previous voltage.
    }

    private enum TbsTone : uint
    {
      Off = 0,
      On,                 // Continuous tone on.
      BurstUnmodulated,   // Simple DiSEqC port A (tone burst).
      BurstModulated      // Simple DiSEqC port B (data burst).
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

    private enum TbsMmiMessage : byte
    {
      Null = 0,
      ApplicationInfo = 0x01,     // PC <-->
      CaInfo = 0x02,              // PC <-->
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

    #region IR remote

    // PCIe/PCI only.
    private enum TbsIrProperty
    {
      Codes = 0,
      ReceiverCommand
    }

    private enum TbsIrCode : byte
    {
      Recall = 0x80,
      Up1 = 0x81,
      Right1 = 0x82,
      Record = 0x83,
      Power = 0x84,
      Three = 0x85,
      Two = 0x86,
      One = 0x87,
      Down1 = 0x88,
      Six = 0x89,
      Five = 0x8a,
      Four = 0x8b,
      Left1 = 0x8c,
      Nine = 0x8d,
      Eight = 0x8e,
      Seven = 0x8f,
      Left2 = 0x90,
      Up2 = 0x91,
      Zero = 0x92,
      Right2 = 0x93,
      Mute = 0x94,
      Tab = 0x95,
      Down2 = 0x96,
      Epg = 0x97,
      Pause = 0x98,
      Ok = 0x99,
      Snapshot = 0x9a,
      Info = 0x9c,
      Play = 0x9b,
      FullScreen = 0x9d,
      Menu = 0x9e,
      Exit = 0x9f
    }

    // PCIe/PCI only.
    private enum TbsIrReceiverCommand : byte
    {
      Start = 1,
      Stop,
      Flush
    }

    #endregion

    #endregion

    #region DLL imports

    /// <summary>
    /// Open the CI interface. This function can only be called once after the graph is built. The graph must be destroyed
    /// and rebuilt if you want to reset the CI interface.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="deviceName">The corresponding DsDevice name.</param>
    /// <returns>a handle that the DLL can use to identify this device for future function calls</returns>
    [DllImport("TbsCIapi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private static extern IntPtr On_Start_CI(IBaseFilter tunerFilter, [MarshalAs(UnmanagedType.LPWStr)] String deviceName);

    /// <summary>
    /// Check whether a CAM is present in the CI slot.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    [DllImport("TbsCIapi.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Camavailable(IntPtr handle);

    /// <summary>
    /// Exchange MMI messages with the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    /// <param name="command">The MMI command.</param>
    /// <param name="response">The MMI response.</param>
    [DllImport("TbsCIapi.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void TBS_ci_MMI_Process(IntPtr handle, IntPtr command, IntPtr response);

    /// <summary>
    /// Send PMT to the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    /// <param name="pmt">The PMT command.</param>
    /// <param name="pmtLength">The length of the PMT.</param>
    [DllImport("TbsCIapi.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void TBS_ci_SendPmt(IntPtr handle, IntPtr pmt, UInt16 pmtLength);

    /// <summary>
    /// Close the CI interface.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    [DllImport("TbsCIapi.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void On_Exit_CI(IntPtr handle);

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct TbsAccessParams
    {
      public TbsAccessMode AccessMode;
      public TbsTone Tone;
      private UInt32 Reserved1;
      public TbsLnbPower LnbPower;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] DiseqcTransmitMessage;
      public UInt32 DiseqcTransmitMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] DiseqcReceiveMessage;
      public UInt32 DiseqcReceiveMessageLength;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] Reserved2;
    }
    
    // Used to improve tuning speeds for older Conexant-based tuners.
    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct NbcTuningParams
    {
      public TbsRollOff RollOff;
      public TbsPilot Pilot;
      public TbsDvbsStandard DvbsStandard;
      public BinaryConvolutionCodeRate InnerFecRate;
      public ModulationType ModulationType;
    }

    // PCIe/PCI only.
    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct IrCommand
    {
      public UInt32 Address;
      public UInt32 Command;
    }

    // USB (QBOX) only.
    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct UsbIrCommand
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      private byte[] Reserved1;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
      public byte[] Codes;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 244)]
      private byte[] Reserved2;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);
    private static readonly Guid UsbBdaExtensionPropertySet = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);
    private static readonly Guid IrPropertySet = new Guid(0xb51c4994, 0x0054, 0x4749, 0x82, 0x43, 0x02, 0x9a, 0x66, 0x86, 0x36, 0x36);

    private const int TbsAccessParamsSize = 536;
    private const int NbcTuningParamsSize = 20;
    private const int MaxDiseqcMessageLength = 128;
    private const int MaxPmtLength = 1024;

    private const int MmiMessageBufferSize = 512;
    private const int MmiResponseBufferSize = 2048;

    private static readonly string[] TunersWithCiSlots = new string[]
    {
      "TBS 5980 CI Tuner",
      "TBS DVBC Tuner",
      "TBS 6991 DVBS/S2 Tuner A",
      "TBS 6991 DVBS/S2 Tuner B",
      "TBS 6992 DVBS/S2 Tuner A",
      "TBS 6992 DVBS/S2 Tuner B",
      "TBS 6928 DVBS/S2 Tuner",
      "TBS 5880 DVB-T/T2 Tuner",
      "TBS 6618 BDA DVBC Tuner",
      "TBS 5880 DVBC Tuner"
    };

    #endregion

    #region variables

    // Buffers for use in conditional access related functions.
    private IntPtr _mmiMessageBuffer = IntPtr.Zero;
    private IntPtr _mmiResponseBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    /// A buffer for general use in synchronised methods in the
    /// Turbosight and TurbosightUsb classes.
    private IntPtr _generalBuffer = IntPtr.Zero;

    private IntPtr _ciHandle = IntPtr.Zero;

    private IBaseFilter _tunerFilter = null;
    private String _tunerFilterName = null;

    private Guid _propertySetGuid = Guid.Empty;
    private IKsPropertySet _propertySet = null;
    private int _tbsAccessProperty = 0;

    private bool _isTurbosight = false;
    private bool _isUsb = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;

    private bool _stopMmiHandlerThread = false;
    private Thread _mmiHandlerThread = null;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    // This is a first-in-first-out queue of messages that are
    // ready to be passed to the CAM. Each message is preceded by
    // a length byte which specifies the complete length of the
    // message including the message code and any parameters.
    // The length byte is followed by a message code (TbsMmiMessage).
    // The final part of the message is zero or more bytes
    // containing parameter data specific to the message type.
    private List<byte> _mmiMessageQueue = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Turbosight"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Turbosight(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        Log.Log.Debug("Turbosight: tuner filter is null");
        return;
      }

      // Check the tuner filter name first. Other manufacturers that do not support these interfaces
      // use the same GUIDs which makes things a little tricky.
      _tunerFilterName = FilterGraphTools.GetFilterName(tunerFilter);
      if (_tunerFilterName == null || (!_tunerFilterName.StartsWith("TBS") && !_tunerFilterName.StartsWith("QBOX")))
      {
        Log.Log.Debug("Turbosight: tuner filter name does not match");
        return;
      }

      // Now check for the USB interface first as per TBS SDK recommendations.
      KSPropertySupport support;
      int hr;
      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet != null)
      {
        hr = _propertySet.QuerySupported(UsbBdaExtensionPropertySet, (int)UsbBdaExtensionProperty.TbsAccess, out support);
        if (hr == 0)
        {
          // Okay, we've got a USB tuner here.
          Log.Log.Debug("Turbosight: supported tuner detected (USB interface)");
          _isTurbosight = true;
          _isUsb = true;
          _propertySetGuid = UsbBdaExtensionPropertySet;
          _tbsAccessProperty = (int)UsbBdaExtensionProperty.TbsAccess;
        }
      }

      // If the tuner doesn't support the USB interface then check for the PCIe/PCI interface.
      if (!_isTurbosight)
      {
        IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
        _propertySet = pin as IKsPropertySet;
        if (_propertySet != null)
        {
          hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.TbsAccess, out support);
          if (hr == 0)
          {
            // Okay, we've got a PCIe or PCI tuner here.
            Log.Log.Debug("Turbosight: supported tuner detected (PCIe/PCI interface)");
            _isTurbosight = true;
            _isUsb = false;
            _propertySetGuid = BdaExtensionPropertySet;
            _tbsAccessProperty = (int)BdaExtensionProperty.TbsAccess;
          }
        }
        if (pin != null && !_isTurbosight)
        {
          Release.ComObject(pin);
        }
      }

      if (!_isTurbosight)
      {
        return;
      }

      _tunerFilter = tunerFilter;
      _generalBuffer = Marshal.AllocCoTaskMem(TbsAccessParamsSize);
      OpenCi();
      SetPowerState(true);
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
    /// Turn the LNB or aerial power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      Log.Log.Debug("Turbosight: set power state, on = {0}", powerOn);

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.LnbPower;
      if (powerOn)
      {
        accessParams.LnbPower = TbsLnbPower.On;
      }
      else
      {
        accessParams.LnbPower = TbsLnbPower.Off;
      }

      Marshal.StructureToPtr(accessParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

      int hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
        _generalBuffer, TbsAccessParamsSize,
        _generalBuffer, TbsAccessParamsSize
      );
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
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Turbosight: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);
      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Tone;
      bool success = true;
      int hr;

      // Send the burst command first.
      if (toneBurstState != ToneBurst.Off)
      {
        accessParams.Tone = TbsTone.BurstUnmodulated;
        if (toneBurstState == ToneBurst.DataBurst)
        {
          accessParams.Tone = TbsTone.BurstModulated;
        }

        Marshal.StructureToPtr(accessParams, _generalBuffer, true);
        //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

        hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
          _generalBuffer, TbsAccessParamsSize,
          _generalBuffer, TbsAccessParamsSize
        );
        if (hr == 0)
        {
          Log.Log.Debug("Turbosight: burst result = success");
        }
        else
        {
          Log.Log.Debug("Turbosight: burst result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      // Now set the 22 kHz tone state.
      accessParams.Tone = TbsTone.Off;
      if (tone22kState == Tone22k.On)
      {
        accessParams.Tone = TbsTone.On;
      }

      Marshal.StructureToPtr(accessParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

      hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
        _generalBuffer, TbsAccessParamsSize,
        _generalBuffer, TbsAccessParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: 22 kHz result = success");
      }
      else
      {
        Log.Log.Debug("Turbosight: 22 kHz result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }

      return success;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Turbosight: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      NbcTuningParams command = new NbcTuningParams();
      // Default: tuning with "auto" is slower, so avoid it if possible.
      command.DvbsStandard = TbsDvbsStandard.Auto;
      // FEC rate
      command.InnerFecRate = ch.InnerFecRate;
      Log.Log.Debug("  inner FEC rate = {0}", command.InnerFecRate);

      // Modulation
      if (ch.ModulationType != ModulationType.ModNotDefined && ch.ModulationType != ModulationType.ModNotSet)
      {
          if (ch.ModulationType == ModulationType.ModNbcQpsk || ch.ModulationType == ModulationType.ModNbc8Psk)
          {
              command.DvbsStandard = TbsDvbsStandard.Dvbs2;
          }
          else
          {
              command.DvbsStandard = TbsDvbsStandard.Dvbs;
          }
      }
      command.ModulationType = ch.ModulationType;
      Log.Log.Debug("  modulation     = {0}", ch.ModulationType);

      // Pilot
      if (ch.Pilot == Pilot.On)
      {
        command.Pilot = TbsPilot.On;
      }
      else
      {
        command.Pilot = TbsPilot.Off;
      }
      Log.Log.Debug("  pilot          = {0}", command.Pilot);

      // Roll-off
      if (ch.Rolloff == RollOff.Twenty)
      {
        command.RollOff = TbsRollOff.Twenty;
      }
      else if (ch.Rolloff == RollOff.TwentyFive)
      {
        command.RollOff = TbsRollOff.TwentyFive;
      }
      else if (ch.Rolloff == RollOff.ThirtyFive)
      {
        command.RollOff = TbsRollOff.ThirtyFive;
      }
      else
      {
        command.RollOff = TbsRollOff.Undefined;
      }
      Log.Log.Debug("  roll-off       = {0}", command.RollOff);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return ch as DVBBaseChannel;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Turbosight: NBC tuning parameter property not supported");
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
        Log.Log.Debug("Turbosight: result = success");
      }
      else
      {
        Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return ch as DVBBaseChannel;
    }

    #region conditional access

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenCi()
    {
      _isCiSlotPresent = IsCiSlotPresent();
      if (!_isCiSlotPresent)
      {
        return false;
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
        return false;
      }
      Log.Log.Debug("Turbosight: interface opened successfully");

      _mmiMessageBuffer = Marshal.AllocCoTaskMem(MmiMessageBufferSize);
      _mmiResponseBuffer = Marshal.AllocCoTaskMem(MmiResponseBufferSize);
      _pmtBuffer = Marshal.AllocCoTaskMem(1026);  // MaxPmtSize + 2

      _isCamPresent = IsCamPresent();
      _isCamReady = IsCamReady();
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseCi()
    {
      if (_ciHandle == IntPtr.Zero)
      {
        return true;
      }

      Log.Log.Debug("Turbosight: close conditional access interface");
      try
      {
        On_Exit_CI(_ciHandle);
      }
      catch (Exception ex)
      {
        // On_Exit_CI() can throw an access violation exception.
      }
      _ciHandle = IntPtr.Zero;
      Marshal.FreeCoTaskMem(_mmiMessageBuffer);
      Marshal.FreeCoTaskMem(_mmiResponseBuffer);
      Marshal.FreeCoTaskMem(_pmtBuffer);
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph"><c>True</c> if the DirectShow filter graph should be rebuilt after calling this function.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetCi(out bool rebuildGraph)
    {
      // TBS have confirmed that it is not currently possible to call On_Start_CI() multiple
      // times on a filter instance *even if On_Exit_CI() is called*. The graph must be rebuilt
      // to reset the CI.
      /*CloseCi();
      OpenCi();*/
      rebuildGraph = true;
      return true;
    }

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("Turbosight: is CI slot present");

      // It does not seem to be possible determine whether a CI slot is actually
      // present. All that we can check is whether the corresponding product has
      // a CI slot.
      String tunerFilterName = FilterGraphTools.GetFilterName(_tunerFilter);
      bool ciPresent = false;
      for (int i = 0; i < TunersWithCiSlots.Length; i++)
      {
        if (tunerFilterName.Equals(TunersWithCiSlots[i]))
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
        Log.Log.Debug("Turbosight: buffer capacity too small, length = {0}", length);
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

      //DVB_MMI.DumpBinary(_pmtBuffer, 0, 2 + length);

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
        // Clear the message queue and buffers in preparation for [first] use.
        _mmiMessageQueue = new List<byte>();
        for (int i = 0; i < MmiMessageBufferSize; i++)
        {
          Marshal.WriteByte(_mmiMessageBuffer, i, 0);
        }
        for (int i = 0; i < MmiResponseBufferSize; i++)
        {
          Marshal.WriteByte(_mmiResponseBuffer, i, 0);
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
      ushort sendCount = 0;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          // Check for CAM state changes.
          bool newState;
          lock (this)
          {
            newState = Camavailable(_ciHandle);
          }
          if (newState != _isCamPresent)
          {
            _isCamPresent = newState;
            Log.Log.Debug("Turbosight: CAM state change, CAM present = {0}", _isCamPresent);
            // If a CAM has just been inserted then clear the message queue - we consider
            // any old messages as invalid now.
            if (_isCamPresent)
            {
              lock (this)
              {
                _mmiMessageQueue = new List<byte>();
              }
              message = TbsMmiMessage.Null;
            }
          }

          // If there is no CAM then we can't send or receive messages.
          if (!_isCamPresent)
          {
            Thread.Sleep(2000);
            continue;
          }

          // Are we still trying to get a response?
          if (message == TbsMmiMessage.Null)
          {
            // No -> do we have a message to send?
            lock (this)
            {
              // Yes -> load it into the message buffer.
              if (_mmiMessageQueue.Count > 0)
              {
                ushort messageLength = _mmiMessageQueue[0];
                message = (TbsMmiMessage)_mmiMessageQueue[1];
                Log.Log.Debug("Turbosight: sending message {0}", message);
                for (ushort i = 0; i < messageLength; i++)
                {
                  Marshal.WriteByte(_mmiMessageBuffer, i, _mmiMessageQueue[i + 1]);
                }
                sendCount = 0;
              }
              // No -> poll for unrequested messages from the CAM.
              else
              {
                Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessage.GetMmi);
              }
            }
          }

          // Send/resend the message.
          lock (this)
          {
            TBS_ci_MMI_Process(_ciHandle, _mmiMessageBuffer, _mmiResponseBuffer);
          }

          // Do we expect a response to this message?
          if (message == TbsMmiMessage.EnterMenu || message == TbsMmiMessage.MenuAnswer || message == TbsMmiMessage.Answer || message == TbsMmiMessage.CloseMmi)
          {
            // No -> remove this message from the queue and move on.
            lock (this)
            {
              ushort messageLength = _mmiMessageQueue[0];
              _mmiMessageQueue.RemoveRange(0, messageLength + 1);
            }
            message = TbsMmiMessage.Null;
            if (_mmiMessageQueue.Count == 0)
            {
              Log.Log.Debug("Turbosight: resuming polling...");
            }
            continue;
          }

          // Yes -> check for a response.
          TbsMmiMessage response = TbsMmiMessage.Null;
          response = (TbsMmiMessage)Marshal.ReadByte(_mmiResponseBuffer, 4);
          if (response == TbsMmiMessage.Null)
          {
            // Responses don't always arrive quickly so give the CAM time to respond if
            // the response isn't ready yet.
            Thread.Sleep(2000);

            // If we are waiting for a response to a message that we sent
            // directly and we haven't received a response after 10 requests
            // then give up and move on.
            if (message != TbsMmiMessage.Null)
            {
              sendCount++;
              if (sendCount >= 10)
              {
                lock (this)
                {
                  ushort messageLength = _mmiMessageQueue[0];
                  _mmiMessageQueue.RemoveRange(0, messageLength + 1);
                }
                Log.Log.Debug("Turbosight: giving up on message {0}", message);
                message = TbsMmiMessage.Null;
                if (_mmiMessageQueue.Count == 0)
                {
                  Log.Log.Debug("Turbosight: resuming polling...");
                }
              }
            }
            continue;
          }

          Log.Log.Debug("Turbosight: received MMI response {0} to message {1}", response, message);
          #region response handling

          // Get the response bytes.
          byte lsb = Marshal.ReadByte(_mmiResponseBuffer, 5);
          byte msb = Marshal.ReadByte(_mmiResponseBuffer, 6);
          int length = (256 * msb) + lsb;
          if (length > MmiResponseBufferSize - 7)
          {
            Log.Log.Debug("Turbosight: response too long, length = {0}", length);
            // We know we haven't got the complete response (DLL internal buffer overflow),
            // so wipe the message and response buffers and give up on this message.
            for (int i = 0; i < MmiResponseBufferSize; i++)
            {
              Marshal.WriteByte(_mmiResponseBuffer, i, 0);
            }
            // If we requested this response directly then remove the request
            // message from the queue.
            if (message != TbsMmiMessage.Null)
            {
              lock (this)
              {
                ushort messageLength = _mmiMessageQueue[0];
                _mmiMessageQueue.RemoveRange(0, messageLength + 1);
              }
              message = TbsMmiMessage.Null;
              if (_mmiMessageQueue.Count == 0)
              {
                Log.Log.Debug("Turbosight: resuming polling...");
              }
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
          }
          else if (response == TbsMmiMessage.CaInfo)
          {
            HandleCaInformation(responseBytes, length);
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
            Log.Log.Debug("Turbosight: unhandled response message {0}", response);
            DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
          }

          // A message has been handled and now we move on to handling the
          // next message or revert to polling for messages from the CAM.
          for (int i = 0; i < MmiResponseBufferSize; i++)
          {
            Marshal.WriteByte(_mmiResponseBuffer, i, 0);
          }
          // If we requested this response directly then remove the request
          // message from the queue.
          if (message != TbsMmiMessage.Null)
          {
            lock (this)
            {
              ushort messageLength = _mmiMessageQueue[0];
              _mmiMessageQueue.RemoveRange(0, messageLength + 1);
            }
            message = TbsMmiMessage.Null;
            if (_mmiMessageQueue.Count == 0)
            {
              Log.Log.Debug("Turbosight: resuming polling...");
            }
          }
          #endregion
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Turbosight: exception in MMI handler thread\r\n{0}", ex.ToString());
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
      String title = DVB_MMI.BytesToString(content, 5, length - 5);
      Log.Log.Debug("  type         = {0}", type);
      Log.Log.Debug("  manufacturer = 0x{0:x}", manufacturer);
      Log.Log.Debug("  code         = 0x{0:x}", code);
      Log.Log.Debug("  menu title   = {0}", title);
    }

    private void HandleCaInformation(byte[] content, int length)
    {
      Log.Log.Debug("Turbosight: conditional access information");
      if (length == 0)
      {
        Log.Log.Debug("Turbosight: error, response too short");
        return;
      }
      int numCasIds = content[0];
      Log.Log.Debug("  # CAS IDs = {0}", numCasIds);
      int i = 1;
      int l = 1;
      while (l + 2 <= length)
      {
        Log.Log.Debug("  {0,-2}        = 0x{1:x2}{2:x2}", i, content[l + 1], content[l]);
        l += 2;
        i++;
      }
      if (length != ((numCasIds * 2) + 1))
      {
        Log.Log.Debug("Turbosight: error, unexpected numCasIds");
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
      int numEntries = content[0];

      // Read all the entries into a list. Entries are NULL terminated.
      List<String> entries = new List<String>();
      String entry = String.Empty;
      int entryCount = 0;
      for (int i = 1; i < length; i++)
      {
        if (content[i] == 0)
        {
          entries.Add(entry);
          entryCount++;
          entry = String.Empty;
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
        Log.Log.Debug("Turbosight: error, not enough menu entries");
        DVB_MMI.DumpBinary(content, 0, length);
        return;
      }

      Log.Log.Debug("  title     = {0}", entries[0]);
      Log.Log.Debug("  sub-title = {0}", entries[1]);
      Log.Log.Debug("  footer    = {0}", entries[2]);
      Log.Log.Debug("  # entries = {0}", numEntries);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiMenu(entries[0], entries[1], entries[2], entryCount);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Turbosight: menu header callback exception\r\n{0}", ex.ToString());
        }
      }
      for (int i = 0; i < entryCount; i++)
      {
        Log.Log.Debug("  entry {0,-2}  = {1}", i + 1, entries[i + 3]);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiMenuChoice(i, entries[i + 3]);
          }
          catch (Exception ex)
          {
            Log.Log.Debug("Turbosight: menu entry callback exception\r\n{0}", ex.ToString());
          }
        }
      }

      if (entryCount != numEntries)
      {
        Log.Log.Debug("Turbosight: error, numEntries != entryCount");
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
        try
        {
          _ciMenuCallbacks.OnCiRequest(blind, answerLength, text);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Turbosight: CAM request callback exception\r\n{0}", ex.ToString());
        }
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
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: enter menu");

      lock (this)
      {
        // Close any existing sessions otherwise the CAM gets confused.
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.CloseMmi);
        // We send an "application info" message because attempting to enter the menu will fail
        // if you don't get the application information first.
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.ApplicationInfo);
        // The CA information is just for information purposes.
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.CaInfo);
        // The main message.
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.EnterMenu);
        // We have to request a response.
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.GetMmi);
      }
      return true;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: close menu");
      lock (this)
      {
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.CloseMmi);
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
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: select menu entry, choice = {0}", choice);
      lock (this)
      {
        _mmiMessageQueue.Add(4);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.MenuAnswer);
        _mmiMessageQueue.Add(0);
        _mmiMessageQueue.Add(0);
        _mmiMessageQueue.Add(choice);
        // Don't explicitly request a response for a "back" request as that
        // could choke the message queue with a message that the CAM
        // never answers.
        if (choice != 0)
        {
          _mmiMessageQueue.Add(1);
          _mmiMessageQueue.Add((byte)TbsMmiMessage.GetMmi);
        }
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
      if (!_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("Turbosight: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      // The message queue requires that we can specify the entire length of the
      // message with only one byte => answer size limit of 251.
      if (answer.Length > 251)
      {
        Log.Log.Debug("Turbosight: answer too long, length = {0}", answer.Length);
        return false;
      }

      byte responseType = (byte)DVB_MMI.ResponseType.Answer;
      if (cancel)
      {
        responseType = (byte)DVB_MMI.ResponseType.Cancel;
      }
      lock (this)
      {
        _mmiMessageQueue.Add((byte)(answer.Length + 4));
        _mmiMessageQueue.Add((byte)TbsMmiMessage.Answer);
        _mmiMessageQueue.Add((byte)(answer.Length + 1));
        _mmiMessageQueue.Add(0);
        _mmiMessageQueue.Add(responseType);
        for (int i = 0; i < answer.Length; i++)
        {
          _mmiMessageQueue.Add((byte)answer[i]);
        }
        // We have to request a response.
        _mmiMessageQueue.Add(1);
        _mmiMessageQueue.Add((byte)TbsMmiMessage.GetMmi);
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

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Turbosight: command too long, length = {0}", command.Length);
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Diseqc;
      accessParams.DiseqcTransmitMessageLength = (uint)command.Length;
      accessParams.DiseqcTransmitMessage = new byte[MaxDiseqcMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        accessParams.DiseqcTransmitMessage[i] = command[i];
      }

      Marshal.StructureToPtr(accessParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

      int hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
        _generalBuffer, TbsAccessParamsSize,
        _generalBuffer, TbsAccessParamsSize
      );
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
      Log.Log.Debug("Turbosight: read DiSEqC command");
      reply = null;

      for (int i = 0; i < TbsAccessParamsSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Diseqc;
      int returnedByteCount;
      int hr = _propertySet.Get(_propertySetGuid, _tbsAccessProperty,
        _generalBuffer, TbsAccessParamsSize,
        _generalBuffer, TbsAccessParamsSize,
        out returnedByteCount
      );
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);

      if (returnedByteCount != TbsAccessParamsSize)
      {
        Log.Log.Debug("Turbosight: result = failure, unexpected number of bytes ({0}) returned", returnedByteCount);
        return false;
      }

      accessParams = (TbsAccessParams)Marshal.PtrToStructure(_generalBuffer, typeof(TbsAccessParams));
      if (accessParams.DiseqcReceiveMessageLength > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Turbosight: result = failure, unexpected number of message bytes ({0}) returned", accessParams.DiseqcReceiveMessageLength);
        return false;
      }
      reply = new byte[accessParams.DiseqcReceiveMessageLength];
      for (int i = 0; i < accessParams.DiseqcReceiveMessageLength; i++)
      {
        reply[i] = accessParams.DiseqcReceiveMessage[i];
      }
      Log.Log.Debug("Turbosight: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close the conditional access interface and free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (!_isTurbosight)
      {
        return;
      }

      SetPowerState(false);
      if (_mmiHandlerThread != null)
      {
        _stopMmiHandlerThread = true;
        Thread.Sleep(3000);
      }
      CloseCi();
      Marshal.FreeCoTaskMem(_generalBuffer);
      if (_isUsb)
      {
        Release.ComObject(_propertySet);
      }
    }

    #endregion
  }
}
