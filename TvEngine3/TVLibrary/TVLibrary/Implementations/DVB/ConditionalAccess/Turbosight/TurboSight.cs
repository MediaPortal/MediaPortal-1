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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Support for TBS Cards
  /// </summary>
  public class Turbosight : IDiSEqCController, ICiMenuActions, IDisposable
  {
    #region Invoke imports

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string dllFilePath);
    [DllImport("kernel32.dll")]
    private extern static bool FreeLibrary(IntPtr dllPointer);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private extern static IntPtr GetProcAddress(IntPtr dllPointer, string functionName);

    #endregion

    #region structures

    // MP internal message holder - purely for convenience.
    private struct MmiMessage
    {
      public TbsMmiMessageType Type;
      public Int32 Length;
      public byte[] Message;

      public MmiMessage(TbsMmiMessageType type, Int32 length)
      {
        Type = type;
        Length = length;
        Message = new byte[length];
      }

      public MmiMessage(TbsMmiMessageType type)
      {
        Type = type;
        Length = 0;
        Message = null;
      }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct NbcTuningParams
    {
      public TbsRollOff RollOff;
      public TbsPilot Pilot;
      public TbsDvbsStandard DvbsStandard;
      public BinaryConvolutionCodeRate InnerFecRate;
      public ModulationType ModulationType;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct TbsAccessParams
    {
      public TbsAccessMode AccessMode;
      public TbsTone Tone;
      private uint Reserved1;
      public TbsLnbPower LnbPower;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] DiseqcTransmitMessage;
      public uint DiseqcTransmitMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] DiseqcReceiveMessage;
      public uint DiseqcReceiveMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] Reserved2;
    }

    #endregion

    #region enums

    // Common properties that can be controlled on all TBS products.
    private enum TbsAccessMode : uint
    {
      LnbPower = 0,           // Control the LNB power supply.
      Diseqc = 1,             // Send and receive DiSEqC messages.
      Tone = 2                // Control the 22 kHz oscillator state.
    }

    private enum TbsDvbsStandard : uint
    {
      Auto = 0,
      Dvbs = 1,
      Dvbs2 = 2
    }

    private enum TbsLnbPower : uint
    {
      Off = 0,
      High = 1,               // 18 V - linear horizontal, circular left.
      Low = 2,                // 13 V - linear vertical, circular right.
      On = 3                  // Power on using the previous voltage.
    }

    private enum TbsMmiMessageType : byte
    {
      Null = 0,
      ApplicationInfo = 0x01,         // PC <-->
      CaInfo = 0x02,                  // PC <-->
      //CaPmt = 0x03,                 // PC -->
      //CaPmtReply = 0x04,            // PC <--
      DateTimeEnquiry = 0x05,         // PC <--
      //DateTime = 0x06,              // PC -->
      Enquiry = 0x07,                 // PC <--
      Answer = 0x08,                  // PC -->
      EnterMenu = 0x09,               // PC -->
      Menu = 0x0a,                    // PC <--
      MenuAnswer = 0x0b,              // PC -->
      List = 0x0c,                    // PC <--
      GetMmi = 0x0d,                  // PC <--
      CloseMmi = 0x0e,                // PC -->
      //DateTimeMode = 0x10,          // PC -->
      //SetDateTime = 0x12            // PC <--
    }

    private enum TbsPilot : uint
    {
      Off = 0,
      On = 1,
      Unknown = 2     // (not used...)
    }

    private enum TbsRollOff : uint
    {
      Undefined = 0xff,
      Twenty = 0,             // 0.2
      TwentyFive = 1,         // 0.25
      ThirtyFive = 2          // 0.35
    }

    private enum TbsTone : uint
    {
      Off = 0,
      On = 1,                 // Continuous tone on.
      BurstUnmodulated = 2,   // Simple DiSEqC port A (tone burst).
      BurstModulated = 3      // Simple DiSEqC port B (data burst).
    }

    // USB (QBOX) only.
    private enum UsbBdaExtensionProperty
    {
      Ir = 1,                 // Property for retrieving IR codes from the IR receiver.
      CiAccess = 8,           // Property for interacting with the CI slot.
      TbsAccess = 18,         // Property for enabling control of the common properties in the TbsAccessMode enum.
      PlpInfo = 19            // Property for setting the DVB-*2 input stream or physical layer pipe identifier.
    }

    // PCIe/PCI only.
    private enum BdaExtensionProperty
    {
      NbcParams = 10,         // Property for setting DVB-S2 parameters that could not initially be set through BDA interfaces.
      CiAccess = 18,          // Property for interacting with the CI slot.
      TbsAccess = 21,         // Property for enabling control of the common properties in the TbsAccessMode enum.
      PlpInfo = 22            // Property for setting the DVB-*2 input stream or physical layer pipe identifier.
    }

    private enum MmiApplicationType : byte
    {
      /// <summary>
      /// Conditional access application.
      /// </summary>
      ConditionalAccess = 1,
      /// <summary>
      /// Electronic programme guide application.
      /// </summary>
      ElectronicProgrammeGuide
    }

    /// <summary>
    /// DVB MMI enquiry answer response types.
    /// </summary>
    private enum MmiResponseType : byte
    {
      /// <summary>
      /// The response is a cancel request.
      /// </summary>
      Cancel = 0,
      /// <summary>
      /// The response contains an answer from the user.
      /// </summary>
      Answer
    }

    /// <summary>
    /// DVB MMI close MMI command types.
    /// </summary>
    private enum MmiCloseType : byte
    {
      /// <summary>
      /// The MMI dialog should be closed immediately.
      /// </summary>
      Immediate = 0,
      /// <summary>
      /// The MMI dialog should be closed after a [short] delay.
      /// </summary>
      Delayed
    }

    /// <summary>
    /// DVB MMI message tags.
    /// </summary>
    private enum MmiTag
    {
      /// <summary>
      /// Unknown tag.
      /// </summary>
      Unknown = 0,

      /// <summary>
      /// Profile enquiry.
      /// </summary>
      ProfileEnquiry = 0x9f8010,
      /// <summary>
      /// Profile.
      /// </summary>
      Profile,
      /// <summary>
      /// Profile change.
      /// </summary>
      ProfileChange,

      /// <summary>
      /// Application information enquiry.
      /// </summary>
      ApplicationInfoEnquiry = 0x9f8020,
      /// <summary>
      /// Application information.
      /// </summary>
      ApplicationInfo,
      /// <summary>
      /// Enter menu.
      /// </summary>
      EnterMenu,

      /// <summary>
      /// Conditional access information enquiry.
      /// </summary>
      ConditionalAccessInfoEnquiry = 0x9f8030,
      /// <summary>
      /// Conditional access information.
      /// </summary>
      ConditionalAccessInfo,
      /// <summary>
      /// Conditional access information programme map table.
      /// </summary>
      ConditionalAccessPmt,
      /// <summary>
      /// Conditional access information programme map table response.
      /// </summary>
      ConditionalAccessPmtResponse,

      /// <summary>
      /// Tune.
      /// </summary>
      Tune = 0x9f8400,
      /// <summary>
      /// Replace.
      /// </summary>
      Replace,
      /// <summary>
      /// Clear replace.
      /// </summary>
      ClearReplace,
      /// <summary>
      /// Ask release.
      /// </summary>
      AskRelease,

      /// <summary>
      /// Date/time enquiry.
      /// </summary>
      DateTimeEnquiry = 0x9f8440,
      /// <summary>
      /// Date/time.
      /// </summary>
      DateTime,

      /// <summary>
      /// Close man-machine interface.
      /// </summary>
      CloseMmi = 0x9f8800,
      /// <summary>
      /// Display control.
      /// </summary>
      DisplayControl,
      /// <summary>
      /// Display reply.
      /// </summary>
      DisplayReply,
      /// <summary>
      /// Text - last.
      /// </summary>
      TextLast,
      /// <summary>
      /// Text - more.
      /// </summary>
      TextMore,
      /// <summary>
      /// Keypad control.
      /// </summary>
      KeypadControl,
      /// <summary>
      /// Key press.
      /// </summary>
      KeyPress,
      /// <summary>
      /// Enquiry.
      /// </summary>
      Enquiry,
      /// <summary>
      /// Answer.
      /// </summary>
      Answer,
      /// <summary>
      /// Menu - last.
      /// </summary>
      MenuLast,
      /// <summary>
      /// Menu - more.
      /// </summary>
      MenuMore,
      /// <summary>
      /// Menu answer.
      /// </summary>
      MenuAnswer,
      /// <summary>
      /// List - last.
      /// </summary>
      ListLast,
      /// <summary>
      /// List - more.
      /// </summary>
      ListMore,
      /// <summary>
      /// Subtitle segment - last.
      /// </summary>
      SubtitleSegmentLast,
      /// <summary>
      /// Subtitle segment - more.
      /// </summary>
      SubtitleSegmentMore,
      /// <summary>
      /// Display message.
      /// </summary>
      DisplayMessage,
      /// <summary>
      /// Scene end mark.
      /// </summary>
      SceneEndMark,
      /// <summary>
      /// Scene done.
      /// </summary>
      SceneDone,
      /// <summary>
      /// Scene control.
      /// </summary>
      SceneControl,
      /// <summary>
      /// Subtitle download - last.
      /// </summary>
      SubtitleDownloadLast,
      /// <summary>
      /// Subtitle download - more.
      /// </summary>
      SubtitleDownloadMore,
      /// <summary>
      /// Flush download.
      /// </summary>
      FlushDownload,
      /// <summary>
      /// Download reply.
      /// </summary>
      DownloadReply,

      /// <summary>
      /// Communication command.
      /// </summary>
      CommsCommand = 0x9f8c00,
      /// <summary>
      /// Connection descriptor.
      /// </summary>
      ConnectionDescriptor,
      /// <summary>
      /// Communication reply.
      /// </summary>
      CommsReply,
      /// <summary>
      /// Communication send - last.
      /// </summary>
      CommsSendLast,
      /// <summary>
      /// Communication send - more.
      /// </summary>
      CommsSendMore,
      /// <summary>
      /// Communication receive - last.
      /// </summary>
      CommsReceiveLast,
      /// <summary>
      /// Communication receive - more.
      /// </summary>
      CommsReceiveMore
    }

    private enum ToneBurst
    {
      None,
      ToneBurst,
      DataBurst
    }

    private enum Tone22k
    {
      Off,
      On,
      Auto
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 110, 0xc9);
    private static readonly Guid UsbBdaExtensionPropertySet = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 170, 0x87, 0xb5, 0xe1, 220, 0x41, 0x13);

    private const int MmiMessageBufferSize = 512;
    private const int MmiResponseBufferSize = 2048;
    private const int MmiHandlerThreadSleepTime = 2000;   // unit = ms

    private const int MaxDiseqcMessageLength = 128;
    private const int MaxPmtLength = 1024;
    private const int NbcTuningParamsSize = 20;
    private const int TbsAccessParamsSize = 536;

    #endregion

    #region variables

    private bool _isTurbosight = false;
    private bool _isUsb = false;

    private IBaseFilter _tunerFilter = null;
    private string _tunerFilterName = string.Empty;

    private IKsPropertySet _propertySet = null;
    private Guid _propertySetGuid = Guid.Empty;
    private int _tbsAccessProperty = -1;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    private static int _ciApiCount = 0;
    private int _ciApiIndex = 0;
    private IntPtr _ciApiLibHandle = IntPtr.Zero;
    private IntPtr _ciApiHandle = IntPtr.Zero;
    private ICiMenuCallbacks _ciMenuCallbacks;

    private DelOn_Start_CI2 _onStartCi = null;
    private DelCamavailable _camAvailable = null;
    private DelTBS_ci_SendPmt _sendPmt = null;
    private DelTBS_ci_MMI_Process _mmiProcess = null;
    private DelOn_Exit_CI _onExitCi = null;

    private bool _isCaInterfaceOpen = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread = false;
    private List<MmiMessage> _mmiMessageQueue;
    private IntPtr _mmiMessageBuffer = IntPtr.Zero;
    private IntPtr _mmiResponseBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// Turbosight constructor
    /// </summary>
    /// <param name="tunerFilter"></param>
    /// <param name="devicePath"></param>
    public Turbosight(IBaseFilter tunerFilter, string devicePath)
    {
      if (tunerFilter == null)
      {
        Log.Log.Debug("Turbosight: tuner filter is null");
        return;
      }

      // Check the tuner filter name first. Other manufacturers that do not support these interfaces
      // use the same property set GUIDs which makes things a little tricky.
      _tunerFilterName = FilterGraphTools.GetFilterName(tunerFilter);
      if ((_tunerFilterName == null) || (!_tunerFilterName.StartsWith("TBS") && !_tunerFilterName.StartsWith("QBOX")))
      {
        Log.Log.Debug("Turbosight: tuner filter name does not match");
        return;
      }

      // Now check for the USB interface first as per TBS SDK recommendations.
      KSPropertySupport support;
      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet != null)
      {
        // For DVB-S/S2 tuners...
        int hr = _propertySet.QuerySupported(UsbBdaExtensionPropertySet, (int)UsbBdaExtensionProperty.TbsAccess, out support);
        if (hr == 0)
        {
          _tbsAccessProperty = (int)UsbBdaExtensionProperty.TbsAccess;
        }
        else
        {
          // For DVB-C/T/T2 tuners with CI slot...
          hr = _propertySet.QuerySupported(UsbBdaExtensionPropertySet, (int)UsbBdaExtensionProperty.CiAccess, out support);
          if (hr != 0)
          {
            // For DVB-T2 tuners without CI slot...
            hr = _propertySet.QuerySupported(UsbBdaExtensionPropertySet, (int)UsbBdaExtensionProperty.PlpInfo, out support);
          }
        }

        if (hr == 0)
        {
          Log.Log.Debug("Turbosight: supported tuner detected (USB interface)");
          _isTurbosight = true;
          _isUsb = true;
          _propertySetGuid = UsbBdaExtensionPropertySet;
        }
      }

      // If the tuner doesn't support the USB interface then check for the PCIe/PCI interface.
      if (!_isTurbosight)
      {
        IPin o = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
        _propertySet = o as IKsPropertySet;
        if (_propertySet != null)
        {
          // For DVB-S/S2 tuners...
          int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.TbsAccess, out support);
          if (hr == 0)
          {
            _tbsAccessProperty = (int)BdaExtensionProperty.TbsAccess;
          }
          else
          {
            // For DVB-C/T/T2 tuners with CI slot...
            hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.CiAccess, out support);
            if (hr != 0)
            {
              // For DVB-T2 tuners without CI slot...
              hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.PlpInfo, out support);
              if (hr != 0)
              {
                // For older Conexant-based DVB-S/S2 tuners [without CI slot]...
                hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
              }
            }
          }

          if (hr == 0)
          {
            Log.Log.Debug("Turbosight: supported tuner detected (PCIe/PCI interface)");
            _isTurbosight = true;
            _isUsb = false;
            _propertySetGuid = BdaExtensionPropertySet;
          }
        }
        if ((o != null) && !_isTurbosight)
        {
          Release.ComObject(o);
        }
      }

      if (_isTurbosight)
      {
        _tunerFilter = tunerFilter;
        _generalBuffer = Marshal.AllocCoTaskMem(TbsAccessParamsSize);
        OpenCi();
        SetPowerState(true);
      }
    }

    public bool IsTurbosight
    {
      get
      {
        return _isTurbosight;
      }
    }

    public void Dispose()
    {
      if (_isTurbosight)
      {
        SetPowerState(false);
        CloseCi();
        Marshal.FreeCoTaskMem(_generalBuffer);
        if (_isUsb)
        {
          Release.ComObject(_propertySet);
        }
      }

      _tunerFilter = null;
      _isTurbosight = false;
    }

    #region CI/CAM support

    #region TBS interface

    /// <summary>
    /// Load a conditional access API instance. This involves obtaining delegate instances for
    /// each of the member functions.
    /// </summary>
    /// <returns><c>true</c> if the instance is successfully loaded, otherwise <c>false</c></returns>
    private bool LoadNewCaApiInstance()
    {
      // Load a new DLL. DLLs should not be reused to avoid issues when resetting interfaces and
      // enable support for multiple tuners with CI slots.
      _ciApiCount++;
      _ciApiIndex = _ciApiCount;
      Log.Log.Debug("Turbosight: loading API, API index = {0}", _ciApiIndex);
      string fileNameSource = Path.Combine("TbsCIapi.dll");
      string fileNameTarget = string.Format("TbsCiApi{0}.dll", _ciApiIndex);
      if (!File.Exists(fileNameTarget))
      {
        try
        {
          File.Copy(fileNameSource, fileNameTarget);
        }
        catch (Exception ex)
        {
          Log.Log.Error("Turbosight: failed to copy TBS CI API DLL\r\n{0}", ex.ToString());
          return false;
        }
      }
      _ciApiLibHandle = LoadLibrary(fileNameTarget);
      if (_ciApiLibHandle == IntPtr.Zero)
      {
        Log.Log.Error("Turbosight: failed to load TBS CI API DLL");
        return false;
      }

      bool foundAllFunctions = false;
      try
      {
        IntPtr function = GetProcAddress(_ciApiLibHandle, "On_Start_CI2");
        if (function == IntPtr.Zero)
        {
          Log.Log.Error("Turbosight: failed to locate the On_Start_CI2 functions");
          return false;
        }
        try
        {
          _onStartCi = (DelOn_Start_CI2)Marshal.GetDelegateForFunctionPointer(function, typeof(DelOn_Start_CI2));
        }
        catch (Exception ex)
        {
          Log.Log.Error("Turbosight: failed to load the On_Start_CI2 function\r\n{0}", ex.ToString());
          return false;
        }

        function = GetProcAddress(_ciApiLibHandle, "Camavailable");
        if (function == IntPtr.Zero)
        {
          Log.Log.Error("Turbosight: failed to locate the Camavailable function");
          return false;
        }
        try
        {
          _camAvailable = (DelCamavailable)Marshal.GetDelegateForFunctionPointer(function, typeof(DelCamavailable));
        }
        catch (Exception ex)
        {
          Log.Log.Error("Turbosight: failed to load the Camavailable function\r\n{0}", ex.ToString());
          return false;
        }

        function = GetProcAddress(_ciApiLibHandle, "TBS_ci_MMI_Process");
        if (function == IntPtr.Zero)
        {
          Log.Log.Error("Turbosight: failed to locate the TBS_ci_MMI_Process function");
          return false;
        }
        try
        {
          _mmiProcess = (DelTBS_ci_MMI_Process)Marshal.GetDelegateForFunctionPointer(function, typeof(DelTBS_ci_MMI_Process));
        }
        catch (Exception ex)
        {
          Log.Log.Error("Turbosight: failed to load the TBS_ci_MMI_Process function\r\n{0}", ex.ToString());
          return false;
        }

        function = GetProcAddress(_ciApiLibHandle, "TBS_ci_SendPmt");
        if (function == IntPtr.Zero)
        {
          Log.Log.Error("Turbosight: failed to locate the TBS_ci_SendPmt function");
          return false;
        }
        try
        {
          _sendPmt = (DelTBS_ci_SendPmt)Marshal.GetDelegateForFunctionPointer(function, typeof(DelTBS_ci_SendPmt));
        }
        catch (Exception ex)
        {
          Log.Log.Error("Turbosight: failed to load the TBS_ci_SendPmt function\r\n{0}", ex.ToString());
          return false;
        }

        function = GetProcAddress(_ciApiLibHandle, "On_Exit_CI");
        if (function == IntPtr.Zero)
        {
          Log.Log.Error("Turbosight: failed to locate the On_Exit_CI function");
          return false;
        }
        try
        {
          _onExitCi = (DelOn_Exit_CI)Marshal.GetDelegateForFunctionPointer(function, typeof(DelOn_Exit_CI));
        }
        catch (Exception ex)
        {
          Log.Log.Error("Turbosight: failed to load the On_Exit_CI function\r\n{0}", ex.ToString());
          return false;
        }

        foundAllFunctions = true;
        return true;
      }
      finally
      {
        if (!foundAllFunctions)
        {
          FreeLibrary(_ciApiLibHandle);
          _ciApiLibHandle = IntPtr.Zero;
        }
      }
    }

    #region delegate definitions

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DelOn_Start_CI2(IBaseFilter tunerFilter, [MarshalAs(UnmanagedType.LPWStr)] string deviceName, int deviceIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelCamavailable(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelTBS_ci_MMI_Process(IntPtr handle, IntPtr command, IntPtr response);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelTBS_ci_SendPmt(IntPtr handle, IntPtr pmt, ushort pmtLength);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelOn_Exit_CI(IntPtr handle);

    #endregion

    #endregion

    #region CI/CAM handling

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    private bool OpenCi()
    {
      Log.Log.Debug("Turbosight: open conditional access interface");

      if (!_isTurbosight)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        return false;
      }

      // Check whether a CI slot is present.
      _isCiSlotPresent = IsCiSlotPresent();
      if (!_isCiSlotPresent)
      {
        return false;
      }
      Log.Log.Debug("Turbosight: open conditional access interface");
      if (!LoadNewCaApiInstance())
      {
        _isCiSlotPresent = false;
        return false;
      }
      _ciApiHandle = _onStartCi(_tunerFilter, FilterGraphTools.GetFilterName(_tunerFilter), _ciApiIndex);
      if (_ciApiHandle == IntPtr.Zero || _ciApiHandle.ToInt64() == -1)
      {
        Log.Log.Debug("Turbosight: interface handle is null");
        _isCiSlotPresent = false;
        FreeLibrary(_ciApiLibHandle);
        _ciApiLibHandle = IntPtr.Zero;
        return false;
      }
      else
      {
        Log.Log.Debug("Turbosight: interface handle {0}", _ciApiHandle);
      }

      Log.Log.Debug("Turbosight: interface opened successfully");
      _isCaInterfaceOpen = true;
      _mmiMessageBuffer = Marshal.AllocCoTaskMem(MmiMessageBufferSize);
      _mmiResponseBuffer = Marshal.AllocCoTaskMem(MmiResponseBufferSize);
      _pmtBuffer = Marshal.AllocCoTaskMem(MaxPmtLength + 2);  // + 2 for TBS PMT header
      _isCamPresent = IsCamPresent();
      _isCamReady = IsCamReady();
      return true;
    }

    private bool IsCiSlotPresent()
    {
      // Check whether a CI slot is present.
      Log.Log.Debug("Turbosight: is CI slot present");
      int ciAccessProperty = (int)BdaExtensionProperty.CiAccess;
      if (_isUsb)
      {
        ciAccessProperty = (int)UsbBdaExtensionProperty.CiAccess;
      }
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(_propertySetGuid, ciAccessProperty, out support);
      if (hr != (int)HResult.Serverity.Success || support == 0)
      {
        Log.Log.Debug("Turbosight: device doesn't have a CI slot");
        return false;
      }
      Log.Log.Debug("Turbosight: device does have a CI slot");
      return true;
    }

    private bool IsCamPresent()
    {
      Log.Log.Debug("Turbosight: is CAM present");
      if (_ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: interface not opened");
        return false;
      }
      bool flag = false;
      lock (this)
      {
        flag = _camAvailable(_ciApiHandle);
      }
      Log.Log.Debug("Turbosight: result = {0}", flag);
      return flag;
    }

    public bool IsCamReady()
    {
      Log.Log.Debug("Turbosight: is CAM ready");
      if (_ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: interface not opened");
        return false;
      }
      // We can only tell whether a CAM is present, not whether it is ready.
      bool camPresent = false;
      lock (this)
      {
        camPresent = _camAvailable(_ciApiHandle);
      }
      Log.Log.Debug("Turbosight: result = {0}", camPresent);
      return camPresent;
    }

    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("Turbosight: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (_ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: interface not opened");
        return false;
      }
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
      Marshal.WriteByte(_pmtBuffer, 0, (byte)listAction);
      Marshal.WriteByte(_pmtBuffer, 1, (byte)command);
      int ofs = 2;
      for (int i = 0; i < length; i++)
      {
        Marshal.WriteByte(_pmtBuffer, ofs, pmt[i]);
        ofs++;
      }
      _sendPmt(_ciApiHandle, _pmtBuffer, (ushort)(length + 2));
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetCi()
    {
      // TBS have confirmed that it is not currently possible to call On_Start_CI() multiple times on a
      // filter instance ***even if On_Exit_CI() is called***. The graph must be rebuilt to reset the CI.
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseCi()
    {
      Log.Log.Debug("Turbosight: close conditional access interface");

      if (_isCaInterfaceOpen && _ciApiHandle != IntPtr.Zero)
      {
        _onExitCi(_ciApiHandle);
        _ciApiHandle = IntPtr.Zero;
      }
      StopMmiHandlerThread();
      _isCiSlotPresent = false;
      _isCamPresent = false;
      _mmiMessageQueue = null;
      if (_mmiMessageBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiMessageBuffer);
        _mmiMessageBuffer = IntPtr.Zero;
      }
      if (_mmiResponseBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiResponseBuffer);
        _mmiResponseBuffer = IntPtr.Zero;
      }
      if (_pmtBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_pmtBuffer);
        _pmtBuffer = IntPtr.Zero;
      }
      if (_ciApiLibHandle != IntPtr.Zero)
      {
        FreeLibrary(_ciApiLibHandle);
        _ciApiLibHandle = IntPtr.Zero;
      }

      Log.Log.Debug("Turbosight: result = true");
      return true;
    }

    #endregion

    #region CAM menu access

    #region ICiMenuActions implementation

    /// <summary>
    /// Set the CAM callback handler functions.
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
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      Log.Log.Debug("Turbosight: enter menu");
      if (!_isTurbosight || _ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        Log.Log.Debug("Turbosight: the CAM is not present");
        return false;
      }
      lock (this)
      {
        // Close any existing sessions otherwise the CAM gets confused.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CloseMmi));
        // We send an "application info" message because attempting to enter the menu will fail
        // if you don't get the application information first.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.ApplicationInfo));
        // The CA information is just for information purposes.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CaInfo));
        // The main message.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.EnterMenu));
        // We have to request a response.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
      }
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Log.Debug("Turbosight: select menu entry, choice = {0}", choice);

      if (!_isTurbosight || _ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        Log.Log.Debug("Turbosight: the CAM is not present");
        return false;
      }

      lock (this)
      {
        MmiMessage selectMessage = new MmiMessage(TbsMmiMessageType.MenuAnswer, 3);
        selectMessage.Message[0] = 0;
        selectMessage.Message[1] = 0;
        selectMessage.Message[2] = choice;
        _mmiMessageQueue.Add(selectMessage);
        // Don't explicitly request a response for a "back" request as that
        // could choke the message queue with a message that the CAM
        // never answers.
        if (choice != 0)
        {
          _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
        }
      }
      return true;
    }

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("Turbosight: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isTurbosight || _ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        Log.Log.Debug("Turbosight: the CAM is not present");
        return false;
      }

      if (answer.Length > 254)
      {
        Log.Log.Debug("Turbosight: answer too long, length = {0}", answer.Length);
        return false;
      }

      byte responseType = (byte)MmiResponseType.Answer;
      if (cancel)
      {
        responseType = (byte)MmiResponseType.Cancel;
      }
      lock (this)
      {
        MmiMessage answerMessage = new MmiMessage(TbsMmiMessageType.Answer, answer.Length + 3);
        answerMessage.Message[0] = (byte)(answer.Length + 1);
        answerMessage.Message[1] = 0;
        answerMessage.Message[2] = responseType;
        int offset = 3;
        for (int i = 0; i < answer.Length; i++)
        {
          answerMessage.Message[offset++] = (byte)answer[i];
        }
        _mmiMessageQueue.Add(answerMessage);
        // We have to request a response.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
      }
      return true;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Log.Debug("Turbosight: close menu");

      if (!_isTurbosight || _ciApiHandle == IntPtr.Zero)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        Log.Log.Debug("Turbosight: the CAM is not present");
        return false;
      }
      lock (this)
      {
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CloseMmi));
      }
      return true;
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if there is no purpose for it.
      if (!_isTurbosight || !_isCiSlotPresent || _ciApiHandle == IntPtr.Zero)
      {
        return;
      }

      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if ((_mmiHandlerThread != null) && !_mmiHandlerThread.IsAlive)
      {
        Log.Log.Debug("Turbosight: aborting old MMI handler thread");
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Log.Debug("Turbosight: starting new MMI handler thread");
        _mmiMessageQueue = new List<MmiMessage>();
        for (int i = 0; i < MmiMessageBufferSize; i++)
        {
          Marshal.WriteByte(_mmiMessageBuffer, i, 0);
        }
        for (int j = 0; j < MmiResponseBufferSize; j++)
        {
          Marshal.WriteByte(_mmiResponseBuffer, j, 0);
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
    /// Stop the thread that handles interaction with the CAM.
    /// </summary>
    private void StopMmiHandlerThread()
    {
      if (_mmiHandlerThread != null)
      {
        if (!_mmiHandlerThread.IsAlive)
        {
          Log.Log.Error("Turbosight: aborting old MMI handler thread");
          _mmiHandlerThread.Abort();
        }
        else
        {
          _stopMmiHandlerThread = true;
          // In the worst case scenario it should take approximately
          // twice the thread sleep time to cleanly stop the thread.
          if (!_mmiHandlerThread.Join(MmiHandlerThreadSleepTime * 2))
          {
            Log.Log.Error("Turbosight: failed to join MMI handler thread, aborting thread");
            _mmiHandlerThread.Abort();
          }
        }
        _mmiHandlerThread = null;
      }
    }

    private void MmiHandler()
    {
      Log.Log.Debug("Turbosight: MMI handler thread start polling");
      TbsMmiMessageType message = TbsMmiMessageType.Null;
      ushort sendCount = 0;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          // Check for CAM state changes.
          bool newState;
          lock (this)
          {
            newState = _camAvailable(_ciApiHandle);
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
                _mmiMessageQueue = new List<MmiMessage>();
              }
              message = TbsMmiMessageType.Null;
            }
          }
          // If there is no CAM then we can't send or receive messages.
          if (!_isCamPresent)
          {
            Thread.Sleep(MmiHandlerThreadSleepTime);
            continue;
          }

          // Are we still trying to get a response?
          if (message == TbsMmiMessageType.Null)
          {
            // No -> do we have a message to send?
            lock (this)
            {
              // Yes -> load it into the message buffer.
              if (_mmiMessageQueue.Count > 0)
              {
                message = _mmiMessageQueue[0].Type;
                Log.Log.Debug("Turbosight: sending message {0}", message);
                Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)message);
                for (ushort i = 0; i < _mmiMessageQueue[0].Length; i++)
                {
                  Marshal.WriteByte(_mmiMessageBuffer, i + 1, _mmiMessageQueue[0].Message[i]);
                }
                sendCount = 0;
              }
              // No -> poll for unrequested messages from the CAM.
              else
              {
                Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessageType.GetMmi);
              }
            }
          }
          // Send/resend the message.
          lock (this)
          {
            _mmiProcess(_ciApiHandle, _mmiMessageBuffer, _mmiResponseBuffer);
          }
          // Do we expect a response to this message?
          if (message == TbsMmiMessageType.EnterMenu || message == TbsMmiMessageType.MenuAnswer || message == TbsMmiMessageType.Answer || message == TbsMmiMessageType.CloseMmi)
          {
            // No -> remove this message from the queue and move on.
            lock (this)
            {
              _mmiMessageQueue.RemoveAt(0);
            }
            message = TbsMmiMessageType.Null;
            if (_mmiMessageQueue.Count == 0)
            {
              Log.Log.Debug("Turbosight: resuming polling...");
            }
            continue;
          }

          // Yes, we expect a response -> check for a response.
          TbsMmiMessageType response = TbsMmiMessageType.Null;
          response = (TbsMmiMessageType)Marshal.ReadByte(_mmiResponseBuffer, 4);
          if (response == TbsMmiMessageType.Null)
          {
            // Responses don't always arrive quickly so give the CAM time to respond if
            // the response isn't ready yet.
            Thread.Sleep(MmiHandlerThreadSleepTime);

            // If we are waiting for a response to a message that we sent
            // directly and we haven't received a response after 10 requests
            // then give up and move on.
            if (message != TbsMmiMessageType.Null)
            {
              sendCount = sendCount++;
              if (sendCount >= 10)
              {
                lock (this)
                {
                  _mmiMessageQueue.RemoveAt(0);
                }
                Log.Log.Debug("Turbosight: giving up on message {0}", message);
                message = TbsMmiMessageType.Null;
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
            if (message != TbsMmiMessageType.Null)
            {
              lock (this)
              {
                _mmiMessageQueue.RemoveAt(0);
              }
              message = TbsMmiMessageType.Null;
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
            responseBytes[i] = Marshal.ReadByte(_mmiResponseBuffer, j++);
          }

          if (response == TbsMmiMessageType.ApplicationInfo)
          {
            HandleApplicationInformation(responseBytes, length);
          }
          else if (response == TbsMmiMessageType.CaInfo)
          {
            HandleCaInformation(responseBytes, length);
          }
          else if (response == TbsMmiMessageType.Menu || response == TbsMmiMessageType.List)
          {
            HandleMenu(responseBytes, length);
          }
          else if (response == TbsMmiMessageType.Enquiry)
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
          if (message != TbsMmiMessageType.Null)
          {
            lock (this)
            {
              _mmiMessageQueue.RemoveAt(0);
            }
            message = TbsMmiMessageType.Null;
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
      catch (Exception exception)
      {
        Log.Log.Debug("Turbosight: error in MMI handler thread\r\n{0}", exception.ToString());
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

      MmiApplicationType type = (MmiApplicationType)content[0];
      //DVB_MMI.ApplicationType type = (DVB_MMI.ApplicationType)content[0];
      String title = System.Text.Encoding.ASCII.GetString(content, 5, length - 5);
      Log.Log.Debug("  type         = {0}", type);
      Log.Log.Debug("  manufacturer = 0x{0:x}{1:x}", content[1], content[2]);
      Log.Log.Debug("  code         = 0x{0:x}{1:x}", content[3], content[4]);
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
      while ((l + 2) <= length)
      {
        Log.Log.Debug("  {0,-2}        = 0x{1:x2}{2:x2}", i, content[l + 1], content[l]);
        l += 2;
        i++;
      }
      if (length != ((numCasIds * 2) + 1))
      {
        Log.Log.Debug("Turbosight: error, unexpected numCasIds");
        DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
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
      String text = System.Text.Encoding.ASCII.GetString(content, 2, length - 2);
      Log.Log.Debug("  text   = {0}", text);
      Log.Log.Debug("  length = {0}", answerLength);
      Log.Log.Debug("  blind  = {0}", blind);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiRequest(blind, answerLength, text);
        }
        catch (Exception exception)
        {
          Log.Log.Debug("Turbosight: CAM request callback exception\r\n{0}", exception.ToString());
        }
      }
      else
      {
        Log.Log.Debug("Turbosight: menu callbacks are not set");
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
      List<string> entries = new List<string>();
      byte[] source = null;
      int entryCount = 0;
      for (int i = 1; i < length; i++)
      {
        if (content[i] == 0)
        {
          IntPtr ptr = Marshal.AllocCoTaskMem(source.Length + 1);
          Marshal.Copy(source, 0, ptr, source.Length);
          Marshal.WriteByte(ptr, source.Length, 0);
          //DVB_MMI.DumpBinary(ptr, 0, source.Length);
          entries.Add(DvbTextConverter.Convert(ptr, null));
          entryCount++;
          source = new byte[] { };
          Marshal.FreeCoTaskMem(ptr);
        }
        else
        {
          if (source == null)
          {
            source = new byte[] { content[i] };
          }
          else
          {
            byte[] array = new byte[source.Length + 1];
            source.CopyTo(array, 0);
            array[array.Length - 1] = content[i];
            source = array;
          }
        }
      }
      IntPtr destination = Marshal.AllocCoTaskMem(source.Length + 1);
      Marshal.Copy(source, 0, destination, source.Length);
      Marshal.WriteByte(destination, source.Length, 0);
      //DVB_MMI.DumpBinary(destination, 0, source.Length);
      entries.Add(DvbTextConverter.Convert(destination, null));
      source = null;
      Marshal.FreeCoTaskMem(destination);
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
        catch (Exception exception)
        {
          Log.Log.Debug("Turbosight: menu header callback exception\r\n{0}", exception.ToString());
        }
      }
      for (int j = 0; j < entryCount; j++)
      {
        Log.Log.Debug("  entry {0,-2}  = {1}", j + 1, entries[j + 3]);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiMenuChoice(j, entries[j + 3]);
          }
          catch (Exception exception2)
          {
            Log.Log.Debug("Turbosight: menu entry callback exception\r\n{0}", exception2.ToString());
          }
        }
      }
      if (entryCount != numEntries)
      {
        Log.Log.Debug("Turbosight: error, numEntries != entryCount");
      }
    }

    #endregion

    #endregion

    #endregion

    #region DiSEqC support

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    private bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Turbosight: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isTurbosight || _propertySet == null || _tbsAccessProperty < 0)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Tone;
      bool success = true;
      int hr;

      // Send the burst command first.
      if (toneBurstState != ToneBurst.None)
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

    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool flag = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst off = ToneBurst.None;
      bool successDiseqc = true;
      if (channel.DisEqc == DisEqcType.SimpleA)
      {
        off = ToneBurst.ToneBurst;
      }
      else if (channel.DisEqc == DisEqcType.SimpleB)
      {
        off = ToneBurst.DataBurst;
      }
      else if (channel.DisEqc != DisEqcType.None)
      {
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool flag3 = (channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL);
        byte num2 = 240;
        num2 = (byte)(num2 | (flag ? ((byte)1) : ((byte)0)));
        num2 = (byte)(num2 | (flag3 ? ((byte)2) : ((byte)0)));
        num2 = (byte)(num2 | ((byte)((antennaNr - 1) << 2)));
        byte[] command = new byte[] { 0xe0, 0x10, 0x38, 0 };
        command[3] = num2;
        successDiseqc = SendDiSEqCCommand(command);
      }
      Tone22k on = Tone22k.Off;
      if (flag)
      {
        on = Tone22k.On;
      }
      bool successTone = SetToneState(off, on);

      SetTuningParameters(channel);

      return (successDiseqc && successTone);
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Turbosight: send DiSEqC command");

      if (!_isTurbosight || _propertySet == null || _tbsAccessProperty < 0)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Log.Debug("Turbosight: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Turbosight: command too long, length = {0}", command.Length);
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Diseqc;
      accessParams.DiseqcTransmitMessageLength = (uint)command.Length;
      accessParams.DiseqcTransmitMessage = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, accessParams.DiseqcTransmitMessage, 0, command.Length);

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
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] response)
    {
      Log.Log.Debug("Turbosight: read DiSEqC response");
      response = null;

      if (!_isTurbosight || _propertySet == null || _tbsAccessProperty < 0)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }

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
      response = new byte[accessParams.DiseqcReceiveMessageLength];
      Buffer.BlockCopy(accessParams.DiseqcReceiveMessage, 0, response, 0, (int)accessParams.DiseqcReceiveMessageLength);
      Log.Log.Debug("Turbosight: result = success");
      return true;
    }

    #endregion

    #region other

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    private bool SetPowerState(bool powerOn)
    {
      Log.Log.Debug("Turbosight: set power state, on = {0}", powerOn);
      if (!_isTurbosight || _propertySet == null || _tbsAccessProperty < 0)
      {
        Log.Log.Debug("Turbosight: device not initialised or interface not supported");
        return false;
      }
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

      int hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty, _generalBuffer, TbsAccessParamsSize, _generalBuffer, TbsAccessParamsSize);
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: result = success");
        return true;
      }
      Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    private void SetTuningParameters(DVBSChannel channel)
    {
      Log.Log.Debug("Turbosight: set tuning parameters");
      if (channel == null)
      {
        return;
      }

      NbcTuningParams structure = new NbcTuningParams
      {
        DvbsStandard = TbsDvbsStandard.Auto,
        InnerFecRate = channel.InnerFecRate
      };
      Log.Log.Debug("  inner FEC rate = {0}", structure.InnerFecRate);

      if (channel.ModulationType == ModulationType.ModNotSet)
      {
        channel.ModulationType = ModulationType.ModQpsk;
        structure.DvbsStandard = TbsDvbsStandard.Dvbs;
      }
      else if (channel.ModulationType == ModulationType.ModQpsk)
      {
        channel.ModulationType = ModulationType.ModNbcQpsk;
        structure.DvbsStandard = TbsDvbsStandard.Dvbs2;
      }
      else if (channel.ModulationType == ModulationType.Mod8Psk)
      {
        channel.ModulationType = ModulationType.ModNbc8Psk;
        structure.DvbsStandard = TbsDvbsStandard.Dvbs2;
      }
      structure.ModulationType = channel.ModulationType;
      Log.Log.Debug("  modulation     = {0}", channel.ModulationType);

      if (channel.Pilot == Pilot.On)
      {
        structure.Pilot = TbsPilot.On;
      }
      else
      {
        structure.Pilot = TbsPilot.Off;
      }
      Log.Log.Debug("  pilot          = {0}", structure.Pilot);

      if (channel.Rolloff == RollOff.Twenty)
      {
        structure.RollOff = TbsRollOff.Twenty;
      }
      else if (channel.Rolloff == RollOff.TwentyFive)
      {
        structure.RollOff = TbsRollOff.TwentyFive;
      }
      else if (channel.Rolloff == RollOff.ThirtyFive)
      {
        structure.RollOff = TbsRollOff.ThirtyFive;
      }
      else
      {
        structure.RollOff = TbsRollOff.Undefined;
      }
      Log.Log.Debug("  roll-off       = {0}", structure.RollOff);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Turbosight: NBC tuning parameter property not supported");
        return;
      }
      Marshal.StructureToPtr(structure, _generalBuffer, true);
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, _generalBuffer, NbcTuningParamsSize, _generalBuffer, NbcTuningParamsSize);
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight: result = success");
        return;
      }
      Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
    }

    #endregion
  }
}
