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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// This file holds the TV Server interface customisations of the
/// DirectShow.NET library. We keep customisations separate from the original
/// library code wherever possible to reduce maintenance and merging issues
/// when it comes to upgrading the DirectShow.NET library.
/// </summary>

  #region AxCore.cs

namespace DirectShowLib
{
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEnumFilters
  {
    [PreserveSig]
    int Next(
      [In] int cFilters,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IBaseFilter[] ppFilter,
      [Out] out int pcFetched   // *** Changed from "[In] IntPtr" to "[Out] out int". ***
      );

    [PreserveSig]
    int Skip([In] int cFilters);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    int Clone([Out] out IEnumFilters ppEnum);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEnumPins
  {
    [PreserveSig]
    int Next(
      [In] int cPins,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IPin[] ppPins,
      [Out] out int pcFetched   // *** Changed from "[In] IntPtr" to "[Out] out int". ***
      );

    [PreserveSig]
    int Skip([In] int cPins);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    int Clone([Out] out IEnumPins ppEnum);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("89c31040-846b-11ce-97d3-00aa0055595a"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEnumMediaTypes
  {
    [PreserveSig]
    int Next(
      [In] int cMediaTypes,
      [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EMTMarshaler), SizeParamIndex = 0)] AMMediaType[] ppMediaTypes,
      [Out] out int pcFetched   // *** Changed from "[In] IntPtr" to "[Out] out int". ***
      );

    [PreserveSig]
    int Skip([In] int cMediaTypes);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    int Clone([Out] out IEnumMediaTypes ppEnum);
  }
}

  #endregion

  #region AXExtend.cs

namespace DirectShowLib
{
  /// <summary>
  /// From KSPROPERTY_SUPPORT_* defines
  /// </summary>
  [Flags]     // Added this attribute.
  public enum KSPropertySupport
  {
    Get = 1,
    Set = 2
  }

  /// <summary>
  /// From TVAudioMode
  /// </summary>
  [Flags]
  public enum TVAudioMode
  {
    None = 0,
    Mono = 0x0001,
    Stereo = 0x0002,
    [Description("language A")]
    LangA = 0x0010,
    [Description("language B (SAP)")]
    LangB = 0x0020,
    [Description("language C")]
    LangC = 0x0040,

    // Added...
    [Description("preset stereo")]
    PresetStereo = 0x0200,
    [Description("preset language A")]
    PresetLangA = 0x1000,
    [Description("preset language B")]
    PresetLangB = 0x2000,
    [Description("preset language C")]
    PresetLangC = 0x4000
  }

  /// <summary>
  /// From VideoProcAmpProperty
  /// </summary>
  public enum VideoProcAmpProperty
  {
    Brightness = 0,
    Contrast,
    Hue,
    Saturation,
    Sharpness,
    Gamma,
    ColorEnable,
    WhiteBalance,
    BacklightCompensation,
    Gain,
    // Properties in KsMedia.h, missing in AXExtend.cs added here.
    DigitalMultiplier,
    DigitalMultiplierLimit,
    WhiteBalanceComponent,
    PowerLineFrequency
  }

  /// <summary>
  /// From CameraControlProperty
  /// </summary>
  public enum CameraControlProperty
  {
    Pan = 0,
    Tilt,
    Roll,
    Zoom,
    Exposure,
    Iris,
    Focus,
    // Properties in KsMedia.h, missing in AXExtend.cs added here.
    ScanMode,
    Privacy,
    PanTilt,
    PanRelative,
    TiltRelative,
    RollRelative,
    ZoomRelative,
    ExposureRelative,
    IrisRelative,
    FocusRelative,
    PanTiltRelative,
    FocalLength,
    AutoExposurePriority
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("70423839-6ACC-4b23-B079-21DBF08156A5"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Obsolete("This interface is deprecated and is maintained for backward compatibility only. New applications and drivers should use the ICodecAPI interface.")]
  public interface IEncoderAPI
  {
    [PreserveSig]
    int IsSupported([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

    [PreserveSig]
    int IsAvailable([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

    [PreserveSig]
    int GetParameterRange(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out object ValueMin,
      [Out] out object ValueMax,
      [Out] out object SteppingDelta
      );

    [PreserveSig]
    int GetParameterValues(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out IntPtr Values,  // *** Changed from object[] to IntPtr to manage memory manually and ensure no leaks. ***
      [Out] out int ValuesCount
      );

    [PreserveSig]
    int GetDefaultValue(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out object Value
      );

    [PreserveSig]
    int GetValue(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out object Value
      );

    [PreserveSig]
    int SetValue(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [In] ref object Value     // *** Changed to ref. ***
      );
  }

  // Disable obsolete interface warning. Some implementations of IEncoderAPI
  // that we want to support do not implement ICodecAPI.
  #pragma warning disable 618
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("02997C3B-8E1B-460e-9270-545E0DE9563E"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IVideoEncoder : IEncoderAPI
  {
    // *** Changed all "[In] Guid" to "[In, MarshalAs(UnmanagedType.LPStruct)] Guid". ***
    #region IEncoderAPI Methods

    [PreserveSig]
    new int IsSupported([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

    [PreserveSig]
    new int IsAvailable([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

    [PreserveSig]
    new int GetParameterRange(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out object ValueMin,
      [Out] out object ValueMax,
      [Out] out object SteppingDelta
      );

    [PreserveSig]
    new int GetParameterValues(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out IntPtr Values,  // *** Changed from object[] to IntPtr to manage memory manually and ensure no leaks. ***
      [Out] out int ValuesCount
      );

    [PreserveSig]
    new int GetDefaultValue(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out object Value
      );

    [PreserveSig]
    new int GetValue(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [Out] out object Value
      );

    [PreserveSig]
    new int SetValue(
      [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,
      [In] ref object Value     // *** Changed to ref. ***
      );

    #endregion
  }
  #pragma warning restore 618
}

  #endregion

  #region BDAIface.cs

namespace DirectShowLib.BDA
{
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("afb6c2a2-2c41-11d3-8a60-0000f81e0e4a"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEnumPIDMap
  {
    [PreserveSig]
    int Next(
      [In] int cRequest,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Struct)] PIDMap[] pPIDMap,
      [Out] out int pcReceived  // *** Changed from "[In] IntPtr" to "[Out] out int". ***
      );

    [PreserveSig]
    int Skip([In] int cRecords);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    int Clone([Out] out IEnumPIDMap ppIEnumPIDMap);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("CD51F1E0-7BE9-4123-8482-A2A796C0A6B0"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IBDA_ConditionalAccess
  {
    [PreserveSig]
    int get_SmartCardStatus(
      [Out] out SmartCardStatusType pCardStatus,
      [Out] out SmartCardAssociationType pCardAssociation,
      [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrCardError,
      [Out, MarshalAs(UnmanagedType.VariantBool)] out bool pfOOBLocked
      );

    [PreserveSig]
    int get_SmartCardInfo(
      [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrCardName,
      [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrCardManufacturer,
      [Out, MarshalAs(UnmanagedType.VariantBool)] out bool pfDaylightSavings,
      [Out] out byte pbyRatingRegion,
      [Out] out int plTimeZoneOffsetMinutes,
      [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrLanguage,
      [Out] out EALocationCodeType pEALocationCode
      );

    [PreserveSig]
    int get_SmartCardApplications(
      [Out] out int pulcApplications,   // *** Changed from "[In, Out] ref" to "[Out] out". ***
      [In] int ulcApplicationsMax,
      [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeParamIndex = 1)] SmartCardApplication[] rgApplications  // *** Changed from in/out to out with marshaling parameters. ***
      );

    [PreserveSig]
    int get_Entitlement(
      [In] short usVirtualChannel,
      [Out] out EntitlementType pEntitlement
      );

    [PreserveSig]
    int TuneByChannel([In] short usVirtualChannel);

    [PreserveSig]
    int SetProgram([In] short usProgramNumber);

    [PreserveSig]
    int AddProgram([In] short usProgramNumber);

    [PreserveSig]
    int RemoveProgram([In] short usProgramNumber);

    [PreserveSig]
    int GetModuleUI(
      [In] byte byDialogNumber,
      [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrURL
      );

    [PreserveSig]
    int InformUIClosed(
      [In] byte byDialogNumber,
      [In] UICloseReasonType CloseReason
      );
  }
}

  #endregion

  #region DsUtils.cs

  // The while loops in DsFindPin methods have been modified as follows:
  //
  //  ...
  //  int lFetched;
  //  ...
  //    while ((ppEnum.Next(1, pPins, out lFetched) >= 0) && (lFetched == 1))
  //  ...
  //
  //  Note the replacement of IntPtr.Zero with lFetched.

namespace DirectShowLib
{
  public class DsDevice : IDisposable
  {
#if USING_NET11
    private UCOMIMoniker m_Mon;
#else
    private IMoniker m_Mon;
#endif
    private string m_Name;

#if USING_NET11
    public DsDevice(UCOMIMoniker Mon)
#else
    public DsDevice(IMoniker Mon)
#endif
    {
      m_Mon = Mon;
      m_Name = null;
    }

#if USING_NET11
    public UCOMIMoniker Mon
#else
    public IMoniker Mon
#endif
    {
      get { return m_Mon; }
    }

    public string Name
    {
      get
      {
        if (m_Name == null)
        {
          m_Name = GetPropBagValue("FriendlyName") as string;
        }
        return m_Name;
      }
    }

    /// <summary>
    /// Returns a unique identifier for a device
    /// </summary>
    public string DevicePath
    {
      get
      {
        string s = null;

        try
        {
          m_Mon.GetDisplayName(null, null, out s);
        }
        catch
        {
        }

        return s;
      }
    }

    /// <summary>
    /// Returns the ClassID for a device
    /// </summary>
    public Guid ClassID
    {
      get
      {
        Guid g;

        m_Mon.GetClassID(out g);

        return g;
      }
    }

    // *** START added. ***

    #region cfgmgr32.dll

    /// <summary>
    /// The CM_Get_Device_ID function retrieves the device instance ID for a specified device instance on the local machine.
    /// </summary>
    /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
    /// <param name="Buffer">Address of a buffer to receive a device instance ID string. The required buffer size can be obtained by calling CM_Get_Device_ID_Size, then incrementing the received value to allow room for the string's terminating NULL. </param>
    /// <param name="BufferLen">Caller-supplied length, in characters, of the buffer specified by Buffer.</param>
    /// <param name="ulFlags">Not used, must be zero.</param>
    /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in Cfgmgr32.h.</returns>
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern uint CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, uint BufferLen, uint ulFlags);

    /// <summary>
    /// The CM_Get_Parent function obtains a device instance handle to the parent node of a specified device node (devnode) in the local machine's device tree.
    /// </summary>
    /// <param name="pdnDevInst">Caller-supplied pointer to the device instance handle to the parent node that this function retrieves. The retrieved handle is bound to the local machine.</param>
    /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
    /// <param name="ulFlags">Not used, must be zero.</param>
    /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in Cfgmgr32.h.</returns>
    [DllImport("cfgmgr32.dll")]
    private static extern uint CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

    #endregion

    #region ole32.dll

    /// <summary>
    /// Returns a pointer to an implementation of IBindCtx (a bind context object). This object stores information about a particular moniker-binding operation.
    /// </summary>
    /// <param name="reserved">This parameter is reserved and must be 0.</param>
    /// <param name="ppbc">Address of an IBindCtx* pointer variable that receives the interface pointer to the new bind context object. When the function is successful, the caller is responsible for calling Release on the bind context. A NULL value for the bind context indicates that an error occurred.</param>
    /// <returns>This function can return the standard return values E_OUTOFMEMORY and S_OK.</returns>
    [DllImport("ole32.dll")]
    private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

    /// <summary>
    /// Converts a string into a moniker that identifies the object named by the string.
    /// This function is the inverse of the IMoniker::GetDisplayName operation, which retrieves the display name associated with a moniker.
    /// </summary>
    /// <param name="pbc">A pointer to the IBindCtx interface on the bind context object to be used in this binding operation.</param>
    /// <param name="szUserName">A pointer to the display name to be parsed.</param>
    /// <param name="pchEaten">A pointer to the number of characters of szUserName that were consumed. If the function is successful, *pchEaten is the length of szUserName; otherwise, it is the number of characters successfully parsed.</param>
    /// <param name="ppmk">The address of the IMoniker* pointer variable that receives the interface pointer to the moniker that was built from szUserName. When successful, the function has called AddRef on the moniker and the caller is responsible for calling Release. If an error occurs, the specified interface pointer will contain as much of the moniker that the method was able to create before the error occurred.</param>
    /// <returns>
    /// This function can return the standard return value E_OUTOFMEMORY, as well as the following values.
    /// S_OK: The parse operation was successful and the moniker was created.
    /// MK_E_SYNTAX: Error in the syntax of a file name or an error in the syntax of the resulting composite moniker.
    /// </returns>
    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    private static extern int MkParseDisplayName(IBindCtx pbc, string szUserName, ref int pchEaten, out IMoniker ppmk);

    #endregion

    #region setupapi.dll

    private const uint MAX_DEVICE_ID_LEN = 200;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SP_DEVINFO_DATA
    {
      public uint cbSize;
      public Guid ClassGuid;
      public uint DevInst;
      public IntPtr Reserved;
    }

    [Flags]
    private enum DiGetClassFlags : uint
    {
      /// <summary>
      /// Return only the device that is associated with the system default device interface, if one is set, for the specified device interface classes.
      /// </summary>
      DIGCF_DEFAULT = 0x00000001,  // only valid with DIGCF_DEVICEINTERFACE
      /// <summary>
      /// Return only devices that are currently present in a system.
      /// </summary>
      DIGCF_PRESENT = 0x00000002,
      /// <summary>
      /// Return a list of installed devices for all device setup classes or all device interface classes.
      /// </summary>
      DIGCF_ALLCLASSES = 0x00000004,
      /// <summary>
      /// Return only devices that are a part of the current hardware profile.
      /// </summary>
      DIGCF_PROFILE = 0x00000008,
      /// <summary>
      /// Return devices that support device interfaces for the specified device interface classes. This flag must be set in the Flags parameter if the Enumerator parameter specifies a device instance ID.
      /// </summary>
      DIGCF_DEVICEINTERFACE = 0x00000010,
    }

    /// <summary>
    /// The SetupDiDestroyDeviceInfoList function deletes a device information set and frees all associated memory.
    /// </summary>
    /// <param name="DeviceInfoSet">A handle to the device information set to delete.</param>
    /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved with a call to GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    /// <summary>
    /// The SetupDiEnumDeviceInfo function returns a SP_DEVINFO_DATA structure that specifies a device information element in a device information set.
    /// </summary>
    /// <param name="DeviceInfoSet">A handle to the device information set for which to return an SP_DEVINFO_DATA structure that represents a device information element.</param>
    /// <param name="MemberIndex">A zero-based index of the device information element to retrieve.</param>
    /// <param name="DeviceInfoData">A pointer to an SP_DEVINFO_DATA structure to receive information about an enumerated device information element. The caller must set DeviceInfoData.cbSize to sizeof(SP_DEVINFO_DATA).</param>
    /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved with a call to GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

    /// <summary>
    /// The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local computer.
    /// </summary>
    /// <param name="ClassGuid">A pointer to the GUID for a device setup class or a device interface class. This pointer is optional and can be NULL. For more information about how to set ClassGuid, see the following Remarks section.</param>
    /// <param name="Enumerator">A pointer to a NULL-terminated string that specifies:
    /// - An identifier (ID) of a Plug and Play (PnP) enumerator. This ID can either be the value's globally unique identifier (GUID) or symbolic name. For example, "PCI" can be used to specify the PCI PnP value. Other examples of symbolic names for PnP values include "USB," "PCMCIA," and "SCSI".
    /// - A PnP device instance ID. When specifying a PnP device instance ID, DIGCF_DEVICEINTERFACE must be set in the Flags parameter.
    /// This pointer is optional and can be NULL. If an enumeration value is not used to select devices, set Enumerator to NULL.
    /// For more information about how to set the Enumerator value, see the following Remarks section.</param>
    /// <param name="hwndParent">A handle to the top-level window to be used for a user interface that is associated with installing a device instance in the device information set. This handle is optional and can be NULL.</param>
    /// <param name="Flags">A variable of type DWORD that specifies control options that filter the device information elements that are added to the device information set. This parameter can be a bitwise OR of zero or more of the following flags. For more information about combining these flags, see the following Remarks section.</param>
    /// <returns>If the operation succeeds, SetupDiGetClassDevs returns a handle to a device information set that contains all installed devices that matched the supplied parameters. If the operation fails, the function returns INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, IntPtr hwndParent, DiGetClassFlags Flags);

    /// <summary>
    /// The SetupDiGetDeviceInstanceId function retrieves the device instance ID that is associated with a device information element.
    /// </summary>
    /// <param name="DeviceInfoSet">A handle to the device information set that contains the device information element that represents the device for which to retrieve a device instance ID. </param>
    /// <param name="DeviceInfoData">A pointer to an SP_DEVINFO_DATA structure that specifies the device information element in DeviceInfoSet.</param>
    /// <param name="DeviceInstanceId">A pointer to the character buffer that will receive the NULL-terminated device instance ID for the specified device information element. For information about device instance IDs, see Device Identification Strings.</param>
    /// <param name="DeviceInstanceIdSize">The size, in characters, of the DeviceInstanceId buffer.</param>
    /// <param name="RequiredSize">A pointer to the variable that receives the number of characters required to store the device instance ID.</param>
    /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved by making a call to GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, uint DeviceInstanceIdSize, out uint RequiredSize);

    #endregion

    private static readonly Guid RTL283X_BDA_EXTENSION_PROPERTY_SET = new Guid(0xbb992b31, 0x931d, 0x41b1, 0x85, 0xea, 0xa0, 0x1b, 0x4e, 0x30, 0x6c, 0xa5);
    private const int RTL283X_BDA_EXTENSION_TEST_SUPPORT_PROPERTY = 2;    // get demod supported modes

    private static IDictionary<string, int> _rtl283xTunerInstanceIds = new Dictionary<string, int>();
    private static int _nextRtl283xTunerInstanceId = 1;

    private bool _readProductInstanceId = false;
    private string _productInstanceId = null;
    private bool _readTunerInstanceId = false;
    private int _tunerInstanceId = -1;

    ~DsDevice()
    {
      Dispose(false);
    }

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the device is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        if (Mon != null)
        {
          Marshal.ReleaseComObject(Mon);
          m_Mon = null;
        }

        m_Name = null;
      }
    }

    #endregion

    /// <summary>
    /// Get the product instance identifier.
    /// </summary>
    /// <remarks>
    /// In most cases the product instance identifier is extracted from the device path (IMoniker
    /// display name).
    /// General device path elements:
    /// @device:[type: cm|dmo|pnp|sw]:\\?\[connection: dd_dvb|hdaudio|pci|root|stream|usb]#[product info: vendor, device, product, subsystem, revision]#[product instance identifier]#[category GUID]\[component instance GUID/CLSID]
    /// 
    /// Examples:
    /// @device:cm:{33D9A762-90C8-11D0-BD43-00A0C911CE86}\7162 BDA Audio Capture
    /// @device:pnp:\\?\usb#vid_1b80&pid_d393#5&10ef021e&0&2#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{9d4afc32-0f42-45d9-b590-af9295699871}
    /// @device:pnp:\\?\stream#hcw88bar.cfg92#5&35edf2e&7&0#{a799a801-a46d-11d0-a18c-00a02401dcd4}\global
    /// @device:pnp:\\?\pci#ven_1131&dev_7162&subsys_010111bd&rev_01#4&1215b326&0&0018#{a799a800-a46d-11d0-a18c-00a02401dcd4}\{62b08a3e-335e-4b30-90f9-2ba400000000}
    /// @device:pnp:\\?\root#system#0000#{fd0a5af4-b41d-11d2-9c95-00c04f7971e0}\{03884cb6-e89a-4deb-b69e-8dc621686e6a}&global
    /// @device:sw:{71985F48-1CA1-11D3-9CC8-00C04F7971E0}\Silicondust HDHomeRun Tuner 1000101F-0
    /// @device:pnp:\\?\hdaudio#func_01&ven_10ec&dev_0882&subsys_1043e601&rev_1001#4&225f9914&0&0001#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\rearlineinwave3
    /// @device:pnp:\\?\dd_dvb#ven_dd01&dev_0011&subsys_0040dd01&rev_00&tuner#5&71e513c&0&2#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{8b884e32-fbca-11de-b16f-000000004d56}
    /// 
    /// For more information about [product] instance IDs:
    /// http://msdn.microsoft.com/en-us/library/windows/hardware/ff547656%28v=vs.85%29.aspx
    /// </remarks>
    /// <param name="devicePath">The device path to analyse.</param>
    /// <returns>the product instance identifier if successful, otherwise <c>null</c></returns>
    public string ProductInstanceIdentifier
    {
      get
      {
        if (_readProductInstanceId)
        {
          return _productInstanceId;
        }
        _readProductInstanceId = true;

        //---------------------------
        // Hardware-specific methods.
        //---------------------------
        Match m;
        string name = Name;
        if (name != null)
        {
          // ATI CableCARD tuners...
          if (name.StartsWith("ATI TV Wonder OpenCable Receiver"))
          {
            // Example: ATI TV Wonder OpenCable Receiver (37F0), Unit #1
            m = Regex.Match(name, @"^ATI\sTV\sWonder\sOpenCable\sReceiver\s\(([^\s]+)\),\sUnit\s\#\d+$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              _productInstanceId = m.Groups[1].Captures[0].Value;
              return _productInstanceId;
            }
          }
          // Ceton CableCARD tuners...
          else if (name.Contains("Ceton"))
          {
            // Example: Ceton InfiniTV PCIe (00-80-75-05) Tuner 1 (00-00-22-00-00-80-75-05)
            m = Regex.Match(name, @"\s+\(([^\s]+)\)\s+Tuner\s+\d+", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              _productInstanceId = m.Groups[1].Captures[0].Value;
              return _productInstanceId;
            }
          }
          // SiliconDust HDHomeRun tuners and Hauppauge CableCARD tuners
          // (SiliconDust HDHomeRun PRIME clones)...
          else if (name.Contains("HDHomeRun") || name.StartsWith("Hauppauge OpenCable Receiver"))
          {
            // Examples:
            // Silicondust HDHomeRun Tuner 1316890F-1
            // HDHomeRun Prime Tuner 1316890F-1
            // Hauppauge OpenCable Receiver 201200AA-1
            m = Regex.Match(name, @"\s+([^\s]+)-\d$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              _productInstanceId = m.Groups[1].Captures[0].Value;
              return _productInstanceId;
            }
          }
        }

        //----------------
        // Generic method.
        //----------------
        string devicePath = DevicePath;
        if (devicePath == null)
        {
          return null;
        }

        // Device paths that we can interpret contain 4 sections separated by #.
        string[] sections = devicePath.Split('#');
        if (sections.Length != 4)
        {
          return null;
        }

        #region explanation
        // The first and second sections are identical for all components
        // driven by the same driver. This is perfect for PnP USB and PCI class
        // hardware. Unfortunately hardware with stream class drivers have
        // separate drivers for each component (tuner, crossbar, capture etc.);
        // same applies for Digital Devices hardware (tuner, capture and CI).
        // Therefore we can't use the first and second sections as part of the
        // product instance ID.
        //
        // The third section is the instance ID. There is no specific
        // documentation on the format of the instance ID. All that we know is:
        // - it contains serial number or location information
        // - guaranteed unique among devices of the same type in the same PC
        // - bus-specific format
        // - persistent across system restarts (but not BIOS or slot changes)
        //
        // We're advised not to assume anything about the format across
        // different versions of Windows:
        // http://support.microsoft.com/kb/311272
        // ...but unfortunately we don't really have a choice if we want to
        // handle stream class drivers.
        //
        // In practice instance IDs for PCI hardware contain 4 sections
        // separated by &. USB seems to be the same unless a serial number is
        // available, in which case the serial number is used.
        //
        // The first section seems to be related to "depth" in the bus
        // connection hierarchy. Commonly 3 for hubs/controllers, 4 for main
        // device, and 5 or greater for sub devices. USB often has multiple
        // intermediate hubs/controllers with value >=4 => main device >= 5.
        //
        // The second and third sections' source and meaning are completely
        // unknown.
        //
        // For PCI, the last section encodes device and function (plus some
        // other unknown information).
        //
        // In order to resolve the stream driver issue we lookup the parent
        // device (expected to be a PCI device - enables us to link to the
        // other components), and use part of the instance ID.
        #endregion

        // Default result.
        string productInstanceIdentifier = sections[2];

        string pnpConnection = sections[0];
        bool isPci = false;
        m = Regex.Match(pnpConnection, @"^\@device\:pnp\:\\\\\?\\([a-z_]+)$");
        if (m.Success)
        {
          pnpConnection = m.Groups[1].Captures[0].Value;
          isPci = pnpConnection.Equals("pci");
        }
        if (!isPci && (pnpConnection.Equals("stream") || pnpConnection.Equals("dd_dvb")))
        {
          // Convert the device path to a device ID. For example:
          // @device:pnp:\\?\stream#hcw88bar.cfg92#5&35edf2e&7&0#{a799a801-a46d-11d0-a18c-00a02401dcd4}\global
          // STREAM\HCW88BAR.CFG92\5&35EDF2E&7&0
          // @device:pnp:\\?\dd_dvb#ven_dd01&dev_0003&subsys_0021dd01&rev_00&tuner#5&d34488e&0&2#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{8b884e11-fbca-11de-b16f-000000004d56}
          // DD_DVB\VEN_DD01&DEV_0003&SUBSYS_0021DD01&REV_00&TUNER\5&D34488E&0&2
          string targetDeviceId = string.Format(@"{0}\{1}\{2}", pnpConnection.ToUpperInvariant(), sections[1].ToUpperInvariant(), sections[2].ToUpperInvariant());

          // Enumerate installed and present media devices with stream class drivers.
          Guid classMedia = new Guid(0x4d36e96c, 0xe325, 0x11ce, 0xbf, 0xc1, 0x08, 0x00, 0x2b, 0xe1, 0x03, 0x18);
          IntPtr devInfoSet = SetupDiGetClassDevs(ref classMedia, pnpConnection.ToUpperInvariant(), IntPtr.Zero, DiGetClassFlags.DIGCF_PRESENT);
          if (devInfoSet != IntPtr.Zero && devInfoSet != new IntPtr(-1))
          {
            try
            {
              StringBuilder tempDeviceId = new StringBuilder((int)MAX_DEVICE_ID_LEN);
              StringBuilder parentDeviceId = new StringBuilder((int)MAX_DEVICE_ID_LEN);
              uint index = 0;
              SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
              devInfo.cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
              while (SetupDiEnumDeviceInfo(devInfoSet, index++, ref devInfo))
              {
                // Get the device ID for the media device.
                uint requiredSize;
                if (SetupDiGetDeviceInstanceId(devInfoSet, ref devInfo, tempDeviceId, MAX_DEVICE_ID_LEN, out requiredSize))
                {
                  // Is this the same device as represented by this DsDevice/moniker?
                  if (string.Equals(tempDeviceId.ToString(), targetDeviceId, StringComparison.InvariantCultureIgnoreCase))
                  {
                    // Yes, same device. Does it have a parent device?
                    uint parentDevInst;
                    if (CM_Get_Parent(out parentDevInst, devInfo.DevInst, 0) == 0 && CM_Get_Device_ID(parentDevInst, parentDeviceId, MAX_DEVICE_ID_LEN, 0) == 0)
                    {
                      // Yes. The parent device ID should look something like:
                      // PCI\VEN_14F1&DEV_8800&SUBSYS_92020070&REV_05\4&CF81C54&0&10F0
                      // PCI\VEN_DD01&DEV_0003&SUBSYS_0021DD01&REV_00\4&2BEDC12D&0&00E4
                      string[] parentSections = parentDeviceId.ToString().Split('\\');
                      if (parentSections.Length == 3)
                      {
                        productInstanceIdentifier = parentSections[2].ToLowerInvariant();
                      }
                      isPci = parentSections[0].Equals("PCI");
                    }
                    break;
                  }
                }
              }
            }
            finally
            {
              SetupDiDestroyDeviceInfoList(devInfoSet);
            }
          }
        }

        sections = productInstanceIdentifier.Split('&');
        if (pnpConnection.Equals("usb") && sections.Length == 1)
        {
          // USB serial number format - nothing further to do.
          _productInstanceId = productInstanceIdentifier;
          return _productInstanceId;
        }
        else if (sections.Length == 4)
        {
          if (!isPci)
          {
            _productInstanceId = sections[0] + '&' + sections[1] + '&' + sections[2];
          }
          else
          {
            // Extract the device ID from the last section.
            sections[3] = sections[3].Substring(0, 2);

            byte deviceId = (byte)(Convert.ToByte(sections[3], 16) & 0xf8);
            _productInstanceId = sections[0] + '&' + sections[1] + '&' + sections[2] + '&' + string.Format("{0:x2}", deviceId);
          }
        }
        return _productInstanceId;
      }
    }

    /// <summary>
    /// Get the tuner instance identifier.
    /// </summary>
    public int TunerInstanceIdentifier
    {
      get
      {
        if (_readTunerInstanceId)
        {
          return _tunerInstanceId;
        }
        _readTunerInstanceId = true;

        //---------------------------
        // Hardware-specific methods.
        //---------------------------
        Match m;
        string name = Name;
        if (name != null)
        {
          // ATI CableCARD tuners...
          if (name.StartsWith("ATI TV Wonder OpenCable Receiver"))
          {
            // Example: ATI TV Wonder OpenCable Receiver (37F0), Unit #1
            m = Regex.Match(name, @"^ATI\sTV\sWonder\sOpenCable\sReceiver\s\([^\s]+\),\sUnit\s\#(\d+)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              _tunerInstanceId = int.Parse(m.Groups[1].Captures[0].Value);
              return _tunerInstanceId;
            }
          }
          // Ceton CableCARD tuners...
          else if (name.Contains("Ceton"))
          {
            // Example: Ceton InfiniTV PCIe (00-80-75-05) Tuner 1 (00-00-22-00-00-80-75-05)
            m = Regex.Match(name, @"\s+\([^\s]+\)\s+Tuner\s+(\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              _tunerInstanceId = int.Parse(m.Groups[1].Captures[0].Value);
              return _tunerInstanceId;
            }
          }
          // SiliconDust HDHomeRun tuners and Hauppauge CableCARD tuners
          // (SiliconDust HDHomeRun PRIME clones)...
          else if (name.Contains("HDHomeRun") || name.StartsWith("Hauppauge OpenCable Receiver"))
          {
            // Examples:
            // Silicondust HDHomeRun Tuner 1316890F-1
            // HDHomeRun Prime Tuner 1316890F-1
            // Hauppauge OpenCable Receiver 201200AA-1
            m = Regex.Match(name, @"\s+[^\s]+-(\d)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              _tunerInstanceId = int.Parse(m.Groups[1].Captures[0].Value);
              return _tunerInstanceId;
            }
          }
        }

        string devicePath = DevicePath;
        if (devicePath == null)
        {
          return _tunerInstanceId;
        }

        // Digital Devices tuners...
        // The device path contains two digits that identify the tuner type and
        // instance. The third digit is the zero-indexed tuner identifier.
        m = Regex.Match(devicePath, @"8b884[0-9a-f]{2}([0-9a-f])-fbca-11de-b16f-000000004d56", RegexOptions.IgnoreCase);
        if (m.Success)
        {
          _tunerInstanceId = Convert.ToInt32(m.Groups[1].Captures[0].Value, 16);
          return _tunerInstanceId;
        }

        //----------------
        // Generic method.
        //----------------
        // Drivers may store a TunerInstanceID in the registry as a means to
        // indicate hardware grouping to the OS. It is related to W7 container
        // IDs:
        // http://msdn.microsoft.com/en-us/library/windows/hardware/ff540024%28v=vs.85%29.aspx
        object registryId = GetPropBagValue("TunerInstanceID");
        if (registryId != null)
        {
          try
          {
            return (int)registryId;
          }
          catch
          {
          }
        }

        //-----------------------------------
        // Realtek RTL283x fall-back method.
        //-----------------------------------
        int rtl283xId;
        if (_rtl283xTunerInstanceIds.TryGetValue(devicePath, out rtl283xId))
        {
          return rtl283xId;
        }
        Guid filterClsid = typeof(IBaseFilter).GUID;
        object obj = null;
        try
        {
          m_Mon.BindToObject(null, null, ref filterClsid, out obj);
        }
        catch
        {
          return _tunerInstanceId;
        }

        try
        {
          IKsPropertySet ps = obj as IKsPropertySet;
          if (ps == null)
          {
            return _tunerInstanceId;
          }
          KSPropertySupport support;
          int hr = ps.QuerySupported(RTL283X_BDA_EXTENSION_PROPERTY_SET, RTL283X_BDA_EXTENSION_TEST_SUPPORT_PROPERTY, out support);
          if (hr == 0 && support.HasFlag(KSPropertySupport.Get))
          {
            // New RTL283x-based tuner identified.
            _tunerInstanceId = _nextRtl283xTunerInstanceId++;
            _rtl283xTunerInstanceIds[devicePath] = _tunerInstanceId;
          }
        }
        finally
        {
          Marshal.ReleaseComObject(obj);
        }

        return _tunerInstanceId;
      }
    }

    /// <summary>
    /// Set the value of a property bag property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    public void SetPropBagValue(string propertyName, object value)
    {
      IPropertyBag bag = null;
      object bagObj = null;

      try
      {
        Guid bagId = typeof(IPropertyBag).GUID;
        m_Mon.BindToStorage(null, null, ref bagId, out bagObj);

        bag = (IPropertyBag)bagObj;

        int hr = bag.Write(propertyName, ref value);
        DsError.ThrowExceptionForHR(hr);
      }
      finally
      {
        bag = null;
        if (bagObj != null)
        {
          Marshal.ReleaseComObject(bagObj);
          bagObj = null;
        }
      }
    }

    /// <summary>
    /// Create a <see cref="DsDevice"/> instance from a device path.
    /// </summary>
    /// <param name="devicePath">The device path.</param>
    /// <returns>the instance if successful, otherwise <c>null</c></returns>
    public static DsDevice FromDevicePath(string devicePath)
    {
      // We need a bind context in order to create a moniker.
      IBindCtx bindContext;
      int hr = CreateBindCtx(0, out bindContext);
      if (hr != 0 || bindContext == null)
      {
        return null;
      }

      devicePath = devicePath.ToLowerInvariant();
      try
      {
        // Create the moniker associated with the device path.
        int consumedCharCount = 0;
        IMoniker moniker;
        hr = MkParseDisplayName(bindContext, devicePath, ref consumedCharCount, out moniker);
        if (hr != 0 && !devicePath.StartsWith("@device:pnp:"))
        {
          devicePath.Insert(0, "@device:pnp:");
          hr = MkParseDisplayName(bindContext, devicePath, ref consumedCharCount, out moniker);
        }
        if (hr != 0 || moniker == null)
        {
          return null;
        }

        return new DsDevice(moniker);
      }
      finally
      {
        Marshal.ReleaseComObject(bindContext);
      }
    }

    // *** END added. ***

    /// <summary>
    /// Returns an array of DsDevices of type devcat.
    /// </summary>
    /// <param name="cat">Any one of FilterCategory</param>
    public static DsDevice[] GetDevicesOfCat(Guid FilterCategory)
    {
      int hr;

      // Use arrayList to build the retun list since it is easily resizable
      DsDevice[] devret;
      ArrayList devs = new ArrayList();
#if USING_NET11
      UCOMIEnumMoniker enumMon;
#else
      IEnumMoniker enumMon;
#endif

      ICreateDevEnum enumDev = (ICreateDevEnum)new CreateDevEnum();
      hr = enumDev.CreateClassEnumerator(FilterCategory, out enumMon, 0);
      DsError.ThrowExceptionForHR(hr);

      // CreateClassEnumerator returns null for enumMon if there are no entries
      if (hr != 1)
      {
        try
        {
          try
          {
#if USING_NET11
            UCOMIMoniker[] mon = new UCOMIMoniker[1];
#else
            IMoniker[] mon = new IMoniker[1];
#endif

#if USING_NET11
            int j;
            while ((enumMon.Next(1, mon, out j) == 0))
#else
            while ((enumMon.Next(1, mon, IntPtr.Zero) == 0))
#endif
            {
              try
              {
                // The devs array now owns this object.  Don't
                // release it if we are going to be successfully
                // returning the devret array
                devs.Add(new DsDevice(mon[0]));
              }
              catch
              {
                Marshal.ReleaseComObject(mon[0]);
                throw;
              }
            }
          }
          finally
          {
            Marshal.ReleaseComObject(enumMon);
          }

          // Copy the ArrayList to the DsDevice[]
          devret = new DsDevice[devs.Count];
          devs.CopyTo(devret);
        }
        catch
        {
          foreach (DsDevice d in devs)
          {
            d.Dispose();
          }
          throw;
        }
      }
      else
      {
        devret = new DsDevice[0];
      }

      return devret;
    }

    /// <summary>
    /// Get a specific PropertyBag value from a moniker
    /// </summary>
    /// <param name="sPropName">The name of the value to retrieve</param>
    /// <returns>object or null on error</returns>
    public object GetPropBagValue(string sPropName)   // *** Changed return type from string to object. ***
    {
      IPropertyBag bag = null;
      object ret = null;        // *** Changed type from string to object. ***
      object bagObj = null;
      object val = null;

      try
      {
        Guid bagId = typeof(IPropertyBag).GUID;
        m_Mon.BindToStorage(null, null, ref bagId, out bagObj);

        bag = (IPropertyBag)bagObj;

        int hr = bag.Read(sPropName, out val, null);
        DsError.ThrowExceptionForHR(hr);

        ret = val;    // *** Removed "as string" cast. ***
      }
      catch
      {
        ret = null;
      }
      finally
      {
        bag = null;
        if (bagObj != null)
        {
          Marshal.ReleaseComObject(bagObj);
          bagObj = null;
        }
      }

      return ret;
    }

    /*
     * Re-implemented above for proper dispose/finalise pattern.
     * 
    public void Dispose()
    {
      if (Mon != null)
      {
        Marshal.ReleaseComObject(Mon);
        m_Mon = null;
      }
      m_Name = null;
    }*/
  }
}

  #endregion

  #region Tuner.cs

namespace DirectShowLib.BDA
{
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("8B8EB248-FC2B-11d2-9D8C-00C04F72D980"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEnumTuningSpaces
  {
    int Next(
      [In] int celt,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ITuningSpace[] rgelt,
      [Out] out int pceltFetched    // *** Changed from "[In] IntPtr" to "[Out] out int". ***
      );

    int Skip([In] int celt);

    int Reset();

    int Clone([Out] out IEnumTuningSpaces ppEnum);
  }

  // *** The methods in this interface had been converted to properties. ***
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("6044634A-1733-4F99-B982-5FB12AFCE4F0"),
   InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IDVBSLocator2 : IDVBSLocator
  {
    #region ILocator Methods

    [PreserveSig]
    new int get_CarrierFrequency([Out] out int Frequency);

    [PreserveSig]
    new int put_CarrierFrequency([In] int Frequency);

    [PreserveSig]
    new int get_InnerFEC([Out] out FECMethod FEC);

    [PreserveSig]
    new int put_InnerFEC([In] FECMethod FEC);

    [PreserveSig]
    new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

    [PreserveSig]
    new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

    [PreserveSig]
    new int get_OuterFEC([Out] out FECMethod FEC);

    [PreserveSig]
    new int put_OuterFEC([In] FECMethod FEC);

    [PreserveSig]
    new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

    [PreserveSig]
    new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

    [PreserveSig]
    new int get_Modulation([Out] out ModulationType Modulation);

    [PreserveSig]
    new int put_Modulation([In] ModulationType Modulation);

    [PreserveSig]
    new int get_SymbolRate([Out] out int Rate);

    [PreserveSig]
    new int put_SymbolRate([In] int Rate);

    [PreserveSig]
    new int Clone([Out] out ILocator NewLocator);

    #endregion

    #region IDVBSLocator methods

    [PreserveSig]
    new int get_SignalPolarisation([Out] out Polarisation PolarisationVal);

    [PreserveSig]
    new int put_SignalPolarisation([In] Polarisation PolarisationVal);

    [PreserveSig]
    new int get_WestPosition([Out, MarshalAs(UnmanagedType.VariantBool)] out bool WestLongitude);

    [PreserveSig]
    new int put_WestPosition([In, MarshalAs(UnmanagedType.VariantBool)] bool WestLongitude);

    [PreserveSig]
    new int get_OrbitalPosition([Out] out int longitude);

    [PreserveSig]
    new int put_OrbitalPosition([In] int longitude);

    [PreserveSig]
    new int get_Azimuth([Out] out int Azimuth);

    [PreserveSig]
    new int put_Azimuth([In] int Azimuth);

    [PreserveSig]
    new int get_Elevation([Out] out int Elevation);

    [PreserveSig]
    new int put_Elevation([In] int Elevation);

    #endregion

    [PreserveSig]
    int get_DiseqLNBSource([Out] out LNB_Source DiseqLNBSourceVal);

    [PreserveSig]
    int put_DiseqLNBSource([In] LNB_Source DiseqLNBSourceVal);

    [PreserveSig]
    int get_LocalOscillatorOverrideLow([Out] out int LocalOscillatorOverrideLowVal);

    [PreserveSig]
    int put_LocalOscillatorOverrideLow([In] int LocalOscillatorOverrideLowVal);

    [PreserveSig]
    int get_LocalOscillatorOverrideHigh([Out] out int LocalOscillatorOverrideHighVal);

    [PreserveSig]
    int put_LocalOscillatorOverrideHigh([In] int LocalOscillatorOverrideHighVal);

    [PreserveSig]
    int get_LocalLNBSwitchOverride([Out] out int LocalLNBSwitchOverrideVal);

    [PreserveSig]
    int put_LocalLNBSwitchOverride([In] int LocalLNBSwitchOverrideVal);

    [PreserveSig]
    int get_LocalSpectralInversionOverride([Out] out SpectralInversion LocalSpectralInversionOverrideVal);

    [PreserveSig]
    int put_LocalSpectralInversionOverride([In] SpectralInversion LocalSpectralInversionOverrideVal);

    [PreserveSig]
    int get_SignalRollOff([Out] out RollOff RollOffVal);

    [PreserveSig]
    int put_SignalRollOff([In] RollOff RollOffVal);

    [PreserveSig]
    int get_SignalPilot([Out] out Pilot PilotVal);

    [PreserveSig]
    int put_SignalPilot([In] Pilot PilotVal);
  }

  #region IESEvent* interfaces

  // *** The Get***() methods in these interfaces incorrectly applied the PreserveSig attribute. ***

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("6B80E96F-55E2-45AA-B754-0C23C8E7D5C1"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESCloseMmiEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetDialogNumber([Out] out int pDialogNumber);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("1F0E5357-AF43-44E6-8547-654C645145D2"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESEvent
  {
    [PreserveSig]
    int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("506A09B8-7F86-4E04-AC05-3303BFE8FC49"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESEventFactory
  {
    [PreserveSig]
    int CreateESEvent([In, MarshalAs(UnmanagedType.IUnknown)] object pServiceProvider, [In] int dwEventId, [In] Guid guidEventType, [In] int dwEventDataLength, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] pEventData, [In, MarshalAs(UnmanagedType.BStr)] string bstrBaseUrl, [In, MarshalAs(UnmanagedType.IUnknown)] object pInitContext, [Out, MarshalAs(UnmanagedType.Interface)] out IESEvent ppESEvent);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("BA9EDCB6-4D36-4CFE-8C56-87A6B0CA48E1"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESFileExpiryDateEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetTunerId([Out] out Guid pguidTunerId);

    [PreserveSig]
    int GetExpiryDate([Out] out long pqwExpiryDate);

    [PreserveSig]
    int GetFinalExpiryDate([Out] out long pqwExpiryDate);

    [PreserveSig]
    int GetMaxRenewalCount([Out] out int dwMaxRenewalCount);

    [PreserveSig]
    int IsEntitlementTokenPresent([Out, MarshalAs(UnmanagedType.Bool)] out bool pfEntTokenPresent);

    [PreserveSig]
    int DoesExpireAfterFirstUse([Out, MarshalAs(UnmanagedType.Bool)] out bool pfExpireAfterFirstUse);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("2017CB03-DC0F-4C24-83CA-36307B2CD19F"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESIsdbCasResponseEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetRequestId([Out] out int pRequestId);

    [PreserveSig]
    int GetStatus([Out] out int pStatus);

    [PreserveSig]
    int GetDataLength([Out] out int pRequestLength);

    [PreserveSig]
    int GetResponseData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("D5A48EF5-A81B-4DF0-ACAA-5E35E7EA45D4"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESLicenseRenewalResultEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetCallersId([Out] out int pdwCallersId);

    [PreserveSig]
    int GetFileName([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrFilename);

    [PreserveSig]
    int IsRenewalSuccessful([Out, MarshalAs(UnmanagedType.Bool)] out bool pfRenewalSuccessful);

    [PreserveSig]
    int IsCheckEntitlementCallRequired([Out, MarshalAs(UnmanagedType.Bool)] out bool pfCheckEntTokenCallNeeded);

    [PreserveSig]
    int GetDescrambledStatus([Out] out int pDescrambledStatus);

    [PreserveSig]
    int GetRenewalResultCode([Out] out int pdwRenewalResultCode);

    [PreserveSig]
    int GetCASFailureCode([Out] out int pdwCASFailureCode);

    [PreserveSig]
    int GetRenewalHResult([Out, MarshalAs(UnmanagedType.Error)] out int phr);

    [PreserveSig]
    int GetEntitlementTokenLength([Out] out int pdwLength);

    [PreserveSig]
    int GetEntitlementToken([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    int GetExpiryDate([Out] out long pqwExpiryDate);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("BA4B6526-1A35-4635-8B56-3EC612746A8C"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESOpenMmiEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetDialogNumber([Out] out int pDialogRequest, [Out] out int pDialogNumber);

    [PreserveSig]
    int GetDialogType([Out] out Guid guidDialogType);

    [PreserveSig]
    int GetDialogData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    int GetDialogStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrBaseUrl, [Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("54C7A5E8-C3BB-4F51-AF14-E0E2C0E34C6D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESRequestTunerEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetPriority([Out] out byte pbyPriority);

    [PreserveSig]
    int GetReason([Out] out byte pbyReason);

    [PreserveSig]
    int GetConsequences([Out] out byte pbyConsequences);

    [PreserveSig]
    int GetEstimatedTime([Out] out int pdwEstimatedTime);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("8A24C46E-BB63-4664-8602-5D9C718C146D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IESValueUpdatedEvent : IESEvent
  {
    #region IESEvent Methods

    [PreserveSig]
    new int GetEventId([Out] out int pdwEventId);

    [PreserveSig]
    new int GetEventType([Out] out Guid pguidEventType);

    [PreserveSig]
    new int SetCompletionStatus([In] int dwResult);

    [PreserveSig]
    new int GetData([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] out byte[] pbData);

    [PreserveSig]
    new int GetStringData([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrData);

    #endregion

    [PreserveSig]
    int GetValueNames([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pbstrNames);
  }

  #endregion
}

  #endregion

  #region Uuids.cs

namespace DirectShowLib
{
  /// <summary>
  /// The full collection of node categories (also known as functions) from bdamedia.h.
  /// </summary>
  public static class BDANodeCategory
  {
    /// <summary> KSNODE_BDA_RF_TUNER </summary>
    public static readonly Guid RF_TUNER = new Guid(0x71985f4c, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

    /// <summary> KSNODE_BDA_ANALOG_DEMODULATOR </summary>
    public static readonly Guid ANALOG_DEMODULATOR = new Guid(0x634db199, 0x27dd, 0x46b8, 0xac, 0xfb, 0xec, 0xc9, 0x8e, 0x61, 0xa2, 0xad);
    /// <summary> KSNODE_BDA_QAM_DEMODULATOR </summary>
    public static readonly Guid QAM_DEMODULATOR = new Guid(0x71985f4d, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);
    /// <summary> KSNODE_BDA_QPSK_DEMODULATOR </summary>
    public static readonly Guid QPSK_DEMODULATOR = new Guid(0x6390c905, 0x27c1, 0x4d67, 0xbd, 0xb7, 0x77, 0xc5, 0x0d, 0x07, 0x93, 0x00);
    /// <summary> KSNODE_BDA_8VSB_DEMODULATOR </summary>
    public static readonly Guid VSB_8_DEMODULATOR = new Guid(0x71985f4f, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);
    /// <summary> KSNODE_BDA_COFDM_DEMODULATOR </summary>
    public static readonly Guid COFDM_DEMODULATOR = new Guid(0x2dac6e05, 0xedbe, 0x4b9c, 0xb3, 0x87, 0x1b, 0x6f, 0xad, 0x7d, 0x64, 0x95);
    /// <summary> KSNODE_BDA_8PSK_DEMODULATOR </summary>
    public static readonly Guid PSK_8_DEMODULATOR = new Guid(0xe957a0e7, 0xdd98, 0x4a3c, 0x81, 0x0b, 0x35, 0x25, 0x15, 0x7a, 0xb6, 0x2e);
    /// <summary> KSNODE_BDA_ISDB_T_DEMODULATOR </summary>
    public static readonly Guid ISDB_T_DEMODULATOR = new Guid(0xfcea3ae3, 0x2cb2, 0x464d, 0x8f, 0x5d, 0x30, 0x5c, 0x0b, 0xb7, 0x78, 0xa2);
    /// <summary> KSNODE_BDA_ISDB_S_DEMODULATOR </summary>
    public static readonly Guid ISDB_S_DEMODULATOR = new Guid(0xedde230a, 0x9086, 0x432d, 0xb8, 0xa5, 0x66, 0x70, 0x26, 0x38, 0x07, 0xe9);

    /// <summary> KSNODE_BDA_OPENCABLE_POD </summary>
    public static readonly Guid OPENCABLE_POD = new Guid(0x345812a0, 0xfb7c, 0x4790, 0xaa, 0x7e, 0xb1, 0xdb, 0x88, 0xac, 0x19, 0xc9);
    /// <summary> KSNODE_BDA_COMMON_CA_POD </summary>
    public static readonly Guid COMMON_CA_POD = new Guid(0xd83ef8fc, 0xf3b8, 0x45ab, 0x8b, 0x71, 0xec, 0xf7, 0xc3, 0x39, 0xde, 0xb4);

    /// <summary> KSNODE_BDA_PID_FILTER </summary>
    public static readonly Guid PID_FILTER = new Guid(0xf5412789, 0xb0a0, 0x44e1, 0xae, 0x4f, 0xee, 0x99, 0x9b, 0x1b, 0x7f, 0xbe);
    /// <summary> KSNODE_BDA_IP_SINK </summary>
    public static readonly Guid IP_SINK = new Guid(0x71985f4e, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);
    /// <summary> KSNODE_BDA_VIDEO_ENCODER </summary>
    public static readonly Guid VIDEO_ENCODER = new Guid(0xd98429e3, 0x65c9, 0x4ac4, 0x93, 0xaa, 0x76, 0x67, 0x82, 0x83, 0x3b, 0x7a);

    /// <summary> KSNODE_BDA_PBDA_CAS </summary>
    public static readonly Guid PBDA_CAS = new Guid(0xc026869f, 0x7129, 0x4e71, 0x86, 0x96, 0xec, 0x8f, 0x75, 0x29, 0x9b, 0x77);
    /// <summary> KSNODE_BDA_PBDA_ISDBCAS </summary>
    public static readonly Guid PBDA_ISDB_CAS = new Guid(0xf2cf2ab3, 0x5b9d, 0x40ae, 0xab, 0x7c, 0x4e, 0x7a, 0xd0, 0xbd, 0x1c, 0x52);
    /// <summary> KSNODE_BDA_PBDA_TUNER </summary>
    public static readonly Guid PBDA_TUNER = new Guid(0xaa5e8286, 0x593c, 0x4979, 0x94, 0x94, 0x46, 0xa2, 0xa9, 0xdf, 0xe0, 0x76);
    /// <summary> KSNODE_BDA_PBDA_MUX </summary>
    public static readonly Guid PBDA_MUX = new Guid(0xf88c7787, 0x6678, 0x4f4b, 0xa1, 0x3e, 0xda, 0x9, 0x86, 0x1d, 0x68, 0x2b);

    /// <summary> KSNODE_BDA_PBDA_DRM </summary>
    public static readonly Guid PBDA_DRM = new Guid(0x9eeebd03, 0xeea1, 0x450f, 0x96, 0xae, 0x63, 0x3e, 0x6d, 0xe6, 0x3c, 0xce);
    /// <summary> KSNODE_BDA_DRI_DRM </summary>
    public static readonly Guid DRI_DRM = new Guid(0x4f95ad74, 0xcefb, 0x42d2, 0x94, 0xa9, 0x68, 0xc5, 0xb2, 0xc1, 0xaa, 0xbe);

    /// <summary> KSNODE_BDA_TS_SELECTOR </summary>
    public static readonly Guid TS_SELECTOR = new Guid(0x5eddf185, 0xfed1, 0x4f45, 0x96, 0x85, 0xbb, 0xb7, 0x3c, 0x32, 0x3c, 0xfc);
  }
}

  #endregion

namespace DirectShowLib
{
  /// <summary>
  /// CLSID_EnhancedVideoRenderer
  /// </summary>
  [ComImport, Guid("fa10746c-9b63-4b6c-bc49-fc300ea5f256")]
  public class EnhancedVideoRenderer
  {
  }

  /// <summary>
  /// A collection of GUID definitions not found elsewhere in DirectShow.NET.
  /// </summary>
  public static class TveGuid
  {
    /// <summary> AM_KSCATEGORY_MULTIVBICODEC </summary>
    public static readonly Guid AM_KS_CATEGORY_MULTI_VBI_CODEC = new Guid(0x9c24a977, 0x0951, 0x451a, 0x80, 0x06, 0x0e, 0x49, 0xbd, 0x28, 0xcd, 0x5f);

    /// <summary> KSDATAFORMAT_SUBTYPE_RAW8 </summary>
    public static readonly Guid KS_DATA_FORMAT_SUB_TYPE_RAW_8 = new Guid(0xca20d9a0, 0x3e3e, 0x11d1, 0x9b, 0xf9, 0x00, 0xc0, 0x4f, 0xbb, 0xde, 0xbf);

    /// <summary> KSMEDIUMSETID_Standard </summary>
    public static readonly Guid KS_MEDIUM_SET_ID_STANDARD = new Guid(0x4747b320, 0x62ce, 0x11cf, 0xa5, 0xd6, 0x28, 0xdb, 0x04, 0xc1, 0x00, 0x00);

    /// <summary> MEDIASUBTYPE_MPEG2_UDCR_TRANSPORT </summary>
    public static readonly Guid MEDIA_SUB_TYPE_MPEG2_UDCR_TRANSPORT = new Guid(0x18bec4ea, 0x4676, 0x450e, 0xb4, 0x78, 0x0c, 0xd8, 0x4c, 0x54, 0xb3, 0x27);
  }

  /// <summary>
  /// A partial collection of codec API parameters from codecapi.h.
  /// </summary>
  public static class CodecApiParameter
  {
    /// <summary> AVAudioChannelCount </summary>
    public static readonly Guid AV_AUDIO_CHANNEL_COUNT = new Guid(0x1d3583c4, 0x1583, 0x474e, 0xb7, 0x1a, 0x5e, 0xe4, 0x63, 0xc1, 0x98, 0xe4);
    /// <summary> AVAudioSampleRate </summary>
    public static readonly Guid AV_AUDIO_SAMPLE_RATE = new Guid(0x971d2723, 0x1acb, 0x42e7, 0x85, 0x5c, 0x52, 0x0a, 0x4b, 0x70, 0xa5, 0xf2);

    /// <summary> AVEncAudioMeanBitRate </summary>
    public static readonly Guid AV_ENC_AUDIO_MEAN_BIT_RATE = new Guid(0x921295bb, 0x4fca, 0x4679, 0xaa, 0xb8, 0x9e, 0x2a, 0x1d, 0x75, 0x33, 0x84);
    /// <summary> AVEncCodecType </summary>
    public static readonly Guid AV_ENC_CODEC_TYPE = new Guid(0x08af4ac1, 0xf3f2, 0x4c74, 0x9d, 0xcf, 0x37, 0xf2, 0xec, 0x79, 0xf8, 0x26);

    /// <summary> AVEncCommonFormatConstraint </summary>
    public static readonly Guid AV_ENC_COMMON_FORMAT_CONSTRAINT = new Guid(0x57cbb9b8, 0x116f, 0x4951, 0xb4, 0x0c, 0xc2, 0xa0, 0x35, 0xed, 0x8f, 0x17);
    /// <summary> AVEncCommonMaxBitRate </summary>
    public static readonly Guid AV_ENC_COMMON_MAX_BIT_RATE = new Guid(0x9651eae4, 0x39b9, 0x4ebf, 0x85, 0xef, 0xd7, 0xf4, 0x44, 0xec, 0x74, 0x65);
    /// <summary> AVEncCommonMeanBitRate </summary>
    public static readonly Guid AV_ENC_COMMON_MEAN_BIT_RATE = new Guid(0xf7222374, 0x2144, 0x4815, 0xb5, 0x50, 0xa3, 0x7f, 0x8e, 0x12, 0xee, 0x52);
    /// <summary> AVEncCommonRateControlMode </summary>
    public static readonly Guid AV_ENC_COMMON_RATE_CONTROL_MODE = new Guid(0x1c0608e9, 0x370c, 0x4710, 0x8a, 0x58, 0xcb, 0x61, 0x81, 0xc4, 0x24, 0x23);

    /// <summary> AVEncMPACodingMode </summary>
    public static readonly Guid AV_ENC_MPA_CODING_MODE = new Guid(0xb16ade03, 0x4b93, 0x43d7, 0xa5, 0x50, 0x90, 0xb4, 0xfe, 0x22, 0x45, 0x37);
    /// <summary> AVEncMPAEnableRedundancyProtection </summary>
    public static readonly Guid AV_ENC_MPA_ENABLE_REDUNDANCY_PROTECTION = new Guid(0x5e54b09e, 0xb2e7, 0x4973, 0xa8, 0x9b, 0x0b, 0x36, 0x50, 0xa3, 0xbe, 0xda);
    /// <summary> AVEncMPALayer </summary>
    public static readonly Guid AV_ENC_MPA_LAYER = new Guid(0x9d377230, 0xf91b, 0x453d, 0x9c, 0xe0, 0x78, 0x44, 0x54, 0x14, 0xc2, 0x2d);

    /// <summary> AVEncMPVDefaultBPictureCount </summary>
    public static readonly Guid AV_ENC_MPV_DEFAULT_B_PICTURE_COUNT = new Guid(0x8d390aac, 0xdc5c, 0x4200, 0xb5, 0x7f, 0x81, 0x4d, 0x04, 0xba, 0xba, 0xb2);
    /// <summary> AVEncMPVGOPOpen </summary>
    public static readonly Guid AV_ENC_MPV_GOP_OPEN = new Guid(0xb1d5d4a6, 0x3300, 0x49b1, 0xae, 0x61, 0xa0, 0x99, 0x37, 0xab, 0x0e, 0x49);
    /// <summary> AVEncMPVGOPSize </summary>
    public static readonly Guid AV_ENC_MPV_GOP_SIZE = new Guid(0x95f31b26, 0x95a4, 0x41aa, 0x93, 0x03, 0x24, 0x6a, 0x7f, 0xc6, 0xee, 0xf1);

    /// <summary> AVEncVideoDefaultUpperFieldDominant </summary>
    public static readonly Guid AV_ENC_VIDEO_DEFAULT_UPPER_FIELD_DOMINANT = new Guid(0x810167c4, 0x0bc1, 0x47ca, 0x8f, 0xc2, 0x57, 0x05, 0x5a, 0x14, 0x74, 0xa5);
    /// <summary> AVEncVideoDisplayDimension </summary>
    public static readonly Guid AV_ENC_VIDEO_DISPLAY_DIMENSION = new Guid(0xde053668, 0xf4ec, 0x47a9, 0x86, 0xd0, 0x83, 0x67, 0x70, 0xf0, 0xc1, 0xd5);
    /// <summary> AVEncVideoInverseTelecineEnable </summary>
    public static readonly Guid AV_ENC_VIDEO_INVERSE_TELECINE_ENABLE = new Guid(0x2ea9098b, 0xe76d, 0x4ccd, 0xa0, 0x30, 0xd3, 0xb8, 0x89, 0xc1, 0xb6, 0x4c);
    /// <summary> AVEncVideoMaxKeyframeDistance </summary>
    public static readonly Guid AV_ENC_VIDEO_MAX_KEY_FRAME_DISTANCE = new Guid(0x2987123a, 0xba93, 0x4704, 0xb4, 0x89, 0xec, 0x1e, 0x5f, 0x25, 0x29, 0x2c);
    /// <summary> AVEncVideoOutputFrameRate </summary>
    public static readonly Guid AV_ENC_VIDEO_OUTPUT_FRAME_RATE = new Guid(0xea85e7c3, 0x9567, 0x4d99, 0x87, 0xc4, 0x02, 0xc1, 0xc2, 0x78, 0xca, 0x7c);
    /// <summary> AVEncVideoOutputScanType </summary>
    public static readonly Guid AV_ENC_VIDEO_OUTPUT_SCAN_TYPE = new Guid(0x460b5576, 0x842e, 0x49ab, 0xa6, 0x2d, 0xb3, 0x6f, 0x73, 0x12, 0xc9, 0xdb);
  }

  /// <summary>
  /// A partial collection of codec API AVEncCodecType GUIDs
  /// (CODECAPI_GUID_AVEnc*) from codecapi.h.
  /// </summary>
  public static class CodecApiAvEncCodecType
  {
    /// <summary> CODECAPI_GUID_AVEncDolbyDigitalConsumer </summary>
    public static readonly Guid DOLBY_DIGITAL_CONSUMER = new Guid(0xc1a7bf6c, 0x0059, 0x4bfa, 0x94, 0xef, 0xef, 0x74, 0x7a, 0x76, 0x8d, 0x52);
    /// <summary> CODECAPI_GUID_AVEncMPEG1Audio </summary>
    public static readonly Guid MPEG1_AUDIO = new Guid(0xd4dd1362, 0xcd4a, 0x4cd6, 0x81, 0x38, 0xb9, 0x4d, 0xb4, 0x54, 0x2b, 0x04);
    /// <summary> CODECAPI_GUID_AVEncMPEG2Audio </summary>
    public static readonly Guid MPEG2_AUDIO = new Guid(0xee4cbb1f, 0x9c3f, 0x4770, 0x92, 0xb5, 0xfc, 0xb7, 0xc2, 0xa8, 0xd3, 0x81);
  }

  /// <summary>
  /// Supported values for codec API parameter AVEncCommonRateControlMode.
  /// </summary>
  public enum eAVEncCommonRateControlMode
  {
    CBR = 0,
    PeakConstrainedVBR = 1,
    UnconstrainedVBR = 2,
    Quality = 3,
    LowDelayVBR = 4,
    GlobalVBR = 5,
    GlobalLowDelayVBR = 6
  }

  /// <summary>
  /// Supported values for codec API parameter AVEncVideoOutputScanType.
  /// </summary>
  public enum eAVEncVideoOutputScanType
  {
    Progressive = 0,
    Interlaced = 1,
    SameAsInput = 2,
    Automatic = 3
  }

  #region IKsControl

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("28f54685-06fd-11d2-b27a-00a0c9223196"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IKsControl
  {
    [PreserveSig]
    int KsProperty(
      [In] ref KsProperty Property,
      [In] int PropertyLength,
      [In, Out] IntPtr PropertyData,
      [In] int DataLength,
      [In, Out] ref int BytesReturned
    );

    [PreserveSig]
    int KsMethod(
      [In] ref KsMethod Method,
      [In] int MethodLength,
      [In, Out] IntPtr MethodData,
      [In] int DataLength,
      [In, Out] ref int BytesReturned
    );

    [PreserveSig]
    int KsEvent(
      [In, Optional] ref KsEvent Event,
      [In] int EventLength,
      [In, Out] IntPtr EventData,
      [In] int DataLength,
      [In, Out] ref int BytesReturned
    );
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct KsProperty
  {
    public Guid Set;
    public int Id;
    public KsPropertyFlag Flags;

    public KsProperty(Guid set, int id, KsPropertyFlag flags)
    {
      Set = set;
      Id = id;
      Flags = flags;
    }
  }

  [Flags]
  public enum KsPropertyFlag
  {
    None = 0,
    Get = 1,
    Set = 2,
    SetSupport = 0x0100,
    BasicSupport = 0x0200,
    Relations = 0x0400,
    SerialiseSet = 0x0800,
    UnserialiseSet = 0x1000,
    SerialiseRaw = 0x2000,
    UnserialiseRaw = 0x4000,
    SerialiseSize = 0x8000,
    DefaultValues = 0x10000,
    Topology = 0x10000000
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct KsMethod
  {
    public Guid Set;
    public int Id;
    public KsMethodFlag Flags;

    public KsMethod(Guid set, int id, KsMethodFlag flags)
    {
      Set = set;
      Id = id;
      Flags = flags;
    }
  }

  [Flags]
  public enum KsMethodFlag
  {
    None = 0,
    Send = 0x1,
    Read = Send,
    Write = 0x2,
    Modify = Read | Write,
    Source = 0x4,
    SetSupport = 0x100,
    BasicSupport = 0x200,
    Topology = 0x10000000
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct KsEvent
  {
    public Guid Set;
    public int Id;
    public KsEventFlag Flags;

    public KsEvent(Guid set, int id, KsEventFlag flags)
    {
      Set = set;
      Id = id;
      Flags = flags;
    }
  }

  [Flags]
  public enum KsEventFlag
  {
    None = 0,
    Enable = 1,
    OneShot = 2,
    EnableBuffered = 4,
    SetSupport = 0x100,
    BasicSupport = 0x200,
    QueryBuffer = 0x400,
    Topology = 0x10000000
  }

  public enum KsEventNotificationType
  {
    EventHandle = 1,
    SemaphoreHandle = 2,
    EventObject = 4,
    SemaphoreObject = 8,
    DeferredProcedureCall = 16,
    WorkItem = 32,
    KsWorkItem = 128
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct KsEventData
  {
    public KsEventNotificationType NotificationType;
    public IntPtr EventOrSemaphore;
    public IntPtr Reserved1;
    public IntPtr Reserved2;
  }

  #endregion

  #region IKsObject

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("423c13a2-2070-11d0-9ef7-00aa00a216a1"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IKsObject
  {
    [PreserveSig]
    IntPtr KsGetObjectHandle();
  }

  #endregion

  /// <summary>
  /// Helper class for releasing COM objects.
  /// </summary>
  public class Release
  {
    /// <summary>
    /// Release a PinInfo instance.
    /// </summary>
    /// <param name="pi">The instance to release.</param>
    /// <returns>the number of remaining references to the instance</returns>
    public static int PinInfo(ref PinInfo pi)
    {
      int refCount = 0;
      if (pi.filter != null)
      {
        refCount = Marshal.ReleaseComObject(pi.filter);
        pi.filter = null;
      }
      return refCount;
    }

    /// <summary>
    /// Release a FilterInfo instance.
    /// </summary>
    /// <param name="fi">The instance to release.</param>
    /// <returns>the number of remaining references to the instance</returns>
    public static int FilterInfo(ref FilterInfo fi)
    {
      int refCount = 0;
      if (fi.pGraph != null)
      {
        refCount = Marshal.ReleaseComObject(fi.pGraph);
        fi.pGraph = null;
      }
      return refCount;
    }

    public static void AmMediaType(ref AMMediaType mediaType)
    {
      if (mediaType != null)
      {
        DsUtils.FreeAMMediaType(mediaType);
        mediaType = null;
      }
      /*else
      {
        LogWarn("Release: asked to release null AM media type");
      }*/
    }

    /// <summary>
    /// Release a COM object.
    /// </summary>
    /// <param name="line">A description of the object that can be used for debugging.</param>
    /// <param name="o">The object to release.</param>
    /// <returns>the number of remaining references to the object</returns>
    public static int ComObject<E>(string line, ref E o)
    {
      int refCount = 0;
      if (o != null)
      {
        //LogDebug("Release: releasing \"{0}\"...", line);
        refCount = Marshal.ReleaseComObject(o);
        //LogDebug("  ref count = {0}", refCount);
        o = default(E);
      }
      /*else
      {
        LogWarn("Release: asked to release null COM object with description \"{0}\"", line);
      }*/
      return refCount;
    }

    /// <summary>
    /// Release a COM object repeatedly until the reference count is zero.
    /// </summary>
    /// <param name="line">A description of the object that can be used for debugging.</param>
    /// <param name="o">The object to release.</param>
    public static void ComObjectAllRefs<E>(string line, ref E o)
    {
      if (o != null)
      {
        //LogDebug("Release: releasing all references to \"{0}\"...", line);
        while (Marshal.ReleaseComObject(o) > 0)
        {
        }
        o = default(E);
      }
      /*else
      {
        LogWarn("Release: asked to release null COM object with description \"{0}\"", line);
      }*/
    }
  }
}

namespace DirectShowLib.BDA
{
  #region IBDA_DiseqCommand

  /// <summary>
  /// KS properties for IBDA_DiseqCommand property set. Defined in bdamedia.h.
  /// </summary>
  public enum BdaDiseqcProperty
  {
    Enable = 0,
    LnbSource,
    UseToneBurst,
    Repeats,
    Send,
    Response
  }

  /// <summary>
  /// Struct for IBDA_DiseqCommand.Send property. Defined in bdamedia.h.
  /// </summary>
  public struct BdaDiseqcMessage
  {
    public int RequestId;
    public int PacketLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] PacketData;
  }

  #endregion

  #region events

  /// <summary>
  /// BDA event types. Defined in bdamedia.h.
  /// </summary>
  public static class BdaEventType
  {
    // Windows XP
    public static readonly Guid TUNING_CHANGING = new Guid(0x83183c03, 0xc09e, 0x45c4, 0xa7, 0x19, 0x80, 0x7a, 0x94, 0x95, 0x2b, 0xf9);
    public static readonly Guid TUNING_CHANGED = new Guid(0x9d7e6235, 0x4b7d, 0x425d, 0xa6, 0xd1, 0xd7, 0x17, 0xc3, 0x3b, 0x9c, 0x4c);
    public static readonly Guid CANDIDATE_POST_TUNE_DATA = new Guid(0x9f02d3d0, 0x9f06, 0x4369, 0x9f, 0x1e, 0x3a, 0xd6, 0xca, 0x19, 0x80, 0x7e);
    public static readonly Guid CA_DENIAL_COUNT_CHANGED = new Guid(0x2a65c528, 0x2249, 0x4070, 0xac, 0x16, 0x00, 0x39, 0x0c, 0xdf, 0xb2, 0xdd);
    public static readonly Guid SIGNAL_STATUS_CHANGED = new Guid(0x6d9cfaf2, 0x702d, 0x4b01, 0x8d, 0xff, 0x68, 0x92, 0xad, 0x20, 0xd1, 0x91);
    public static readonly Guid NEW_SIGNAL_ACQUIRED = new Guid(0xc87ec52d, 0xcd18, 0x404a, 0xa0, 0x76, 0xc0, 0x2a, 0x27, 0x3d, 0x3d, 0xe7);

    // Windows Vista
    public static readonly Guid EAS_MESSAGE_RECEIVED = new Guid(0xd10df9d5, 0xc261, 0x4b85, 0x9e, 0x8a, 0x51, 0x7b, 0x32, 0x99, 0xca, 0xb2);
    public static readonly Guid PSI_TABLE = new Guid(0x1b9c3703, 0xd447, 0x4e16, 0x97, 0xbb, 0x01, 0x79, 0x9f, 0xc0, 0x31, 0xed);
    public static readonly Guid SERVICE_TERMINATED = new Guid(0x0a1d591c, 0xe0d2, 0x4f8e, 0x89, 0x60, 0x23, 0x35, 0xbe, 0xf4, 0x5c, 0xcb);
    public static readonly Guid CARD_STATUS_CHANGED = new Guid(0xa265faea, 0xf874, 0x4b38, 0x9f, 0xf7, 0xc5, 0x3d, 0x02, 0x96, 0x99, 0x96);
    public static readonly Guid DRM_PAIRING_STATUS_CHANGED = new Guid(0x000906f5, 0xf0d1, 0x41d6, 0xa7, 0xdf, 0x40, 0x28, 0x69, 0x76, 0x69, 0xf6);
    public static readonly Guid DRM_PAIRING_STEP_COMPLETE = new Guid(0x5b2ebf78, 0xb752, 0x4420, 0xb4, 0x1e, 0xa4, 0x72, 0xdc, 0x95, 0x82, 0x8e);
    public static readonly Guid MMI_MESSAGE = new Guid(0x052c29af, 0x09a4, 0x4b93, 0x89, 0x0f, 0xbd, 0x6a, 0x34, 0x89, 0x68, 0xa4);
    public static readonly Guid ENTITLEMENT_CHANGED = new Guid(0x9071ad5d, 0x2359, 0x4c95, 0x86, 0x94, 0xaf, 0xa8, 0x1d, 0x70, 0xbf, 0xd5);
    public static readonly Guid STB_CHANNEL_NUMBER = new Guid(0x17c4d730, 0xd0f0, 0x413a, 0x8c, 0x99, 0x50, 0x04, 0x69, 0xde, 0x35, 0xad);

    // Windows 7
    public static readonly Guid BDA_EVENTING_SERVICE_PENDING_EVENT = new Guid(0x5ca51711, 0x5ddc, 0x41a6, 0x94, 0x30, 0xe4, 0x1b, 0x8b, 0x3b, 0xbc, 0x5b);
    public static readonly Guid BDA_CONDITIONAL_ACCESS_TAG = new Guid(0xefc3a459, 0xae8b, 0x4b4a, 0x8f, 0xe9, 0x79, 0xa0, 0xd0, 0x97, 0xf3, 0xea);
    public static readonly Guid CAS_FAILURE_SPANNING_EVENT = new Guid(0xead831ae, 0x5529, 0x4d1f, 0xaf, 0xce, 0x0d, 0x8c, 0xd1, 0x25, 0x7d, 0x30);
    public static readonly Guid CHANNEL_CHANGE_SPANNING_EVENT = new Guid(0x9067c5e5, 0x4c5c, 0x4205, 0x86, 0xc8, 0x7a, 0xfe, 0x20, 0xfe, 0x1e, 0xfa);
    public static readonly Guid CHANNEL_TYPE_SPANNING_EVENT = new Guid(0x72ab1d51, 0x87d2, 0x489b, 0xba, 0x11, 0x0e, 0x08, 0xdc, 0x21, 0x02, 0x43);
    public static readonly Guid CHANNEL_INFO_SPANNING_EVENT = new Guid(0x41f36d80, 0x4132, 0x4cc2, 0xb1, 0x21, 0x01, 0xa4, 0x32, 0x19, 0xd8, 0x1b);
    public static readonly Guid RRT_SPANNING_EVENT = new Guid(0xf6cfc8f4, 0xda93, 0x4f2f, 0xbf, 0xf8, 0xba, 0x1e, 0xe6, 0xfc, 0xa3, 0xa2);
    public static readonly Guid CAPTION_SERVICE_DESCRIPTOR_SPANNING_EVENT = new Guid(0xefe779d9, 0x97f0, 0x4786, 0x80, 0x0d, 0x95, 0xcf, 0x50, 0x5d, 0xdc, 0x66);
    public static readonly Guid CONTENT_ADVISORY_DESCRIPTOR_SPANNING_EVENT = new Guid(0x3ab4a2e6, 0x4247, 0x4b34, 0x89, 0x6c, 0x30, 0xaf, 0xa5, 0xd2, 0x1c, 0x24);
    public static readonly Guid DVB_SCRAMBLING_CONTROL_SPANNING_EVENT = new Guid(0x4bd4e1c4, 0x90a1, 0x4109, 0x82, 0x36, 0x27, 0xf0, 0x0e, 0x7d, 0xcc, 0x5b);
    public static readonly Guid SIGNAL_AND_SERVICE_STATUS_SPANNING_EVENT = new Guid(0x8068c5cb, 0x3c04, 0x492b, 0xb4, 0x7d, 0x03, 0x08, 0x82, 0x0d, 0xce, 0x51);
    public static readonly Guid EMM_MESSAGE_SPANNING_EVENT = new Guid(0x6bf00268, 0x4f7e, 0x4294, 0xaa, 0x87, 0xe9, 0xe9, 0x53, 0xe4, 0x3f, 0x14);
    public static readonly Guid AUDIO_TYPE_SPANNING_EVENT = new Guid(0x501cbfbe, 0xb849, 0x42ce, 0x9b, 0xe9, 0x3d, 0xb8, 0x69, 0xfb, 0x82, 0xb3);
    public static readonly Guid STREAM_TYPE_SPANNING_EVENT = new Guid(0x82af2ebc, 0x30a6, 0x4264, 0xa8, 0x0b, 0xad, 0x2e, 0x13, 0x72, 0xac, 0x60);
    public static readonly Guid ARIB_CONTENT_SPANNING_EVENT = new Guid(0x3a954083, 0x93d0, 0x463e, 0x90, 0xb2, 0x07, 0x42, 0xc4, 0x96, 0xed, 0xf0);
    public static readonly Guid LANGUAGE_SPANNING_EVENT = new Guid(0xe292666d, 0x9c02, 0x448d, 0xaa, 0x8d, 0x78, 0x1a, 0x93, 0xfd, 0xc3, 0x95);
    public static readonly Guid DUAL_MONO_SPANNING_EVENT = new Guid(0xa9a29b56, 0xa84b, 0x488c, 0x89, 0xd5, 0x0d, 0x4e, 0x76, 0x57, 0xc8, 0xce);
    public static readonly Guid PID_LIST_SPANNING_EVENT = new Guid(0x47fc8f65, 0xe2bb, 0x4634, 0x9c, 0xef, 0xfd, 0xbf, 0xe6, 0x26, 0x1d, 0x5c);
    public static readonly Guid AUDIO_DESCRIPTOR_SPANNING_EVENT = new Guid(0x107bd41c, 0xa6da, 0x4691, 0x83, 0x69, 0x11, 0xb2, 0xcd, 0xaa, 0x28, 0x8e);
    public static readonly Guid SUBTITLE_SPANNING_EVENT = new Guid(0x5dcec048, 0xd0b9, 0x4163, 0x87, 0x2c, 0x4f, 0x32, 0x22, 0x3b, 0xe8, 0x8a);
    public static readonly Guid TELETEXT_SPANNING_EVENT = new Guid(0x9599d950, 0x5f33, 0x4617, 0xaf, 0x7c, 0x1e, 0x54, 0xb5, 0x10, 0xda, 0xa3);
    public static readonly Guid STREAM_ID_SPANNING_EVENT = new Guid(0xcaf1ab68, 0xe153, 0x4d41, 0xa6, 0xb3, 0xa7, 0xc9, 0x98, 0xdb, 0x75, 0xee);
    public static readonly Guid PBDA_PARENTAL_CONTROL_EVENT = new Guid(0xf947aa85, 0xfb52, 0x48e8, 0xb9, 0xc5, 0xe1, 0xe1, 0xf4, 0x11, 0xa5, 0x1a);
    public static readonly Guid TUNE_FAILURE_EVENT = new Guid(0xd97287b2, 0x2dfd, 0x436a, 0x94, 0x85, 0x99, 0xd7, 0xd4, 0xab, 0x5a, 0x69);
    public static readonly Guid TUNE_FAILURE_SPANNING_EVENT = new Guid(0x6f8aa455, 0x5ee1, 0x48ab, 0xa2, 0x7c, 0x4c, 0x8d, 0x70, 0xb9, 0xae, 0xba);
    public static readonly Guid DVB_PARENTAL_RATING_DESCRIPTOR = new Guid(0x2a67a58d, 0xeca5, 0x4eac, 0xab, 0xcb, 0xe7, 0x34, 0xd3, 0x77, 0x6d, 0x0a);
    public static readonly Guid DFN_WITH_NO_ACTUAL_AV_DATA = new Guid(0xf5689ffe, 0x55f9, 0x4bb3, 0x96, 0xbe, 0xae, 0x97, 0x1c, 0x63, 0xba, 0xe0);
    public static readonly Guid BDA_ISDB_CAS_RESPONSE = new Guid(0xd4cb1966, 0x41bc, 0x4ced, 0x9a, 0x20, 0xfd, 0xce, 0xac, 0x78, 0xf7, 0x0d);
    public static readonly Guid BDA_CAS_REQUEST_TUNER = new Guid(0xcf39a9d8, 0xf5d3, 0x4685, 0xbe, 0x57, 0xed, 0x81, 0xdb, 0xa4, 0x6b, 0x27);
    public static readonly Guid BDA_CAS_RELEASE_TUNER = new Guid(0x20c1a16b, 0x441f, 0x49a5, 0xbb, 0x5c, 0xe9, 0xa0, 0x44, 0x95, 0xc6, 0xc1);
    public static readonly Guid BDA_CAS_OPEN_MMI = new Guid(0x85dac915, 0xe593, 0x410d, 0x84, 0x71, 0xd6, 0x81, 0x21, 0x05, 0xf2, 0x8e);
    public static readonly Guid BDA_CAS_CLOSE_MMI = new Guid(0x5d0f550f, 0xde2e, 0x479d, 0x83, 0x45, 0xec, 0x0e, 0x95, 0x57, 0xe8, 0xa2);
    public static readonly Guid BDA_CAS_BROADCAST_MMI = new Guid(0x676876f0, 0x1132, 0x404c, 0xa7, 0xca, 0xe7, 0x20, 0x69, 0xa9, 0xd5, 0x4f);
    public static readonly Guid BDA_TUNER_SIGNAL_LOCK = new Guid(0x1872e740, 0xf573, 0x429b, 0xa0, 0x0e, 0xd9, 0xc1, 0xe4, 0x08, 0xaf, 0x09);
    public static readonly Guid BDA_TUNER_NO_SIGNAL = new Guid(0xe29b382b, 0x1edd, 0x4930, 0xbc, 0x46, 0x68, 0x2f, 0xd7, 0x2d, 0x2d, 0xfb);
    public static readonly Guid BDA_GPNV_VALUE_UPDATE = new Guid(0xff75c68c, 0xf416, 0x4e7e, 0xbf, 0x17, 0x6d, 0x55, 0xc5, 0xdf, 0x15, 0x75);
    public static readonly Guid BDA_UPDATE_DRM_STATUS = new Guid(0x65a6f681, 0x1462, 0x473b, 0x88, 0xce, 0xcb, 0x73, 0x14, 0x27, 0xbd, 0xb5);
    public static readonly Guid BDA_UPDATE_SCAN_STATE = new Guid(0x55702b50, 0x7b49, 0x42b8, 0xa8, 0x2f, 0x4a, 0xfb, 0x69, 0x1b, 0x06, 0x28);
    public static readonly Guid BDA_GUIDE_DATA_AVAILABLE = new Guid(0x98db717a, 0x478a, 0x4cd4, 0x92, 0xd0, 0x95, 0xf6, 0x6b, 0x89, 0xe5, 0xb1);
    public static readonly Guid BDA_GUIDE_SERVICE_INFORMATION_UPDATED = new Guid(0xa1c3ea2b, 0x175f, 0x4458, 0xb7, 0x35, 0x50, 0x7d, 0x22, 0xdb, 0x23, 0xa6);
    public static readonly Guid BDA_GUIDE_DATA_ERROR = new Guid(0xac33c448, 0x6f73, 0x4fd7, 0xb3, 0x41, 0x59, 0x4c, 0x36, 0x0d, 0x8d, 0x74);
    public static readonly Guid BDA_DISEQC_RESPONSE_AVAILABLE = new Guid(0xefa628f8, 0x1f2c, 0x4b67, 0x9e, 0xa5, 0xac, 0xf6, 0xfa, 0x9a, 0x1f, 0x36);
    public static readonly Guid BDA_LBIGS_OPEN_CONNECTION = new Guid(0x356207b2, 0x6f31, 0x4eb0, 0xa2, 0x71, 0xb3, 0xfa, 0x6b, 0xb7, 0x68, 0x0f);
    public static readonly Guid BDA_LBIGS_SEND_DATA = new Guid(0x1123277b, 0xf1c6, 0x4154, 0x8b, 0x0d, 0x48, 0xe6, 0x15, 0x70, 0x59, 0xaa);
    public static readonly Guid BDA_LBIGS_CLOSE_CONNECTION_HANDLE = new Guid(0xc2f08b99, 0x65ef, 0x4314, 0x96, 0x71, 0xe9, 0x9d, 0x4c, 0xce, 0x0b, 0xae);
    public static readonly Guid BDA_ENCODER_SIGNAL_LOCK = new Guid(0x5ec90eb9, 0x39fa, 0x4cfc, 0xb9, 0x3f, 0x00, 0xbb, 0x11, 0x07, 0x7f, 0x5e);
    public static readonly Guid BDA_FDC_STATUS = new Guid(0x05f25366, 0xd0eb, 0x43d2, 0xbc, 0x3c, 0x68, 0x2b, 0x86, 0x3d, 0xf1, 0x42);
    public static readonly Guid BDA_FDC_TABLE_SECTION = new Guid(0x6a0cd757, 0x4ce3, 0x4e5b, 0x94, 0x44, 0x71, 0x87, 0xb8, 0x71, 0x52, 0xc5);
    public static readonly Guid BDA_TRANSPORT_STREAM_SELECTOR_INFO = new Guid(0xc40f9f85, 0x09d0, 0x489c, 0x9e, 0x9c, 0x0a, 0xbb, 0xb5, 0x69, 0x51, 0xb0);
    public static readonly Guid BDA_RATING_PIN_RESET = new Guid(0xc6e048c0, 0xc574, 0x4c26, 0xbc, 0xda, 0x2f, 0x4d, 0x35, 0xeb, 0x5e, 0x85);
  }

  /// <summary>
  /// KS event set IDs. Defined in bdamedia.h.
  /// </summary>
  public static class BdaMediaEventSet
  {
    // Windows XP
    /// <summary> KSEVENTSETID_BdaCAEvent </summary>
    public static readonly Guid CONDITIONAL_ACCESS = new Guid(0x488c4ccc, 0xb768, 0x4129, 0x8e, 0xb1, 0xb0, 0xa, 0x7, 0x1f, 0x90, 0x68);
    /// <summary> KSEVENTSETID_BdaPinEvent </summary>
    public static readonly Guid PIN = new Guid(0x104781cd, 0x50bd, 0x40d5, 0x95, 0xfb, 0x08, 0x7e, 0xe, 0x86, 0xa5, 0x91);

    // Windows Vista
    /// <summary> KSEVENTSETID_BdaTunerEvent </summary>
    public static readonly Guid TUNER = new Guid(0xaab59e17, 0x1c9, 0x4ebf, 0x93, 0xf2, 0xfc, 0x3b, 0x79, 0xb4, 0x6f, 0x91);

    // Windows 7
    /// <summary> KSEVENTSETID_BdaDiseqCEvent </summary>
    public static readonly Guid DISEQC = new Guid(0x8b19bbf0, 0x4184, 0x43ac, 0xad, 0x3c, 0xc, 0x88, 0x9b, 0xe4, 0xc2, 0x12);
    /// <summary> KSEVENTSETID_BdaEvent </summary>
    public static readonly Guid EVENT = new Guid(0xae7e55b2, 0x96d7, 0x4e29, 0x90, 0x8f, 0x62, 0xf9, 0x5b, 0x2a, 0x16, 0x79);
  }

  /// <summary>
  /// KS event IDs for the KSEVENTSETID_BdaCAEvent event set. Defined in bdamedia.h.
  /// </summary>
  public enum ConditionalAccessEvent
  {
    /// <summary> KSEVENT_BDA_PROGRAM_FLOW_STATUS_CHANGED </summary>
    ProgramFlowStatusChanged = 0,
    /// <summary> KSEVENT_BDA_CA_MODULE_STATUS_CHANGED </summary>
    ModuleStatusChanged,
    /// <summary> KSEVENT_BDA_CA_SMART_CARD_STATUS_CHANGED </summary>
    SmartCardStatusChanged,
    /// <summary> KSEVENT_BDA_CA_MODULE_UI_REQUESTED </summary>
    ModuleUiRequested
  }

  /// <summary>
  /// KS event IDs for the KSEVENTSETID_BdaDiseqCEvent event set. Defined in bdamedia.h.
  /// </summary>
  public enum DiseqcEvent
  {
    /// <summary> KSEVENT_BDA_DISEQC_DATA_RECEIVED </summary>
    DataReceived = 0
  }

  /// <summary>
  /// KS event IDs for the KSEVENTSETID_BdaEvent event set. Defined in bdamedia.h.
  /// </summary>
  public enum EventType
  {
    /// <summary> KSEVENT_BDA_EVENT_PENDINGEVENT </summary>
    PendingEvent = 0
  }

  /// <summary>
  /// KS event IDs for the KSEVENTSETID_BdaPinEvent event set. Defined in bdamedia.h.
  /// </summary>
  public enum PinEvent
  {
    /// <summary> KSEVENT_BDA_PIN_CONNECTED </summary>
    Connected = 0,
    /// <summary> KSEVENT_BDA_PIN_DISCONNECTED </summary>
    Disconnected
  }

  /// <summary>
  /// KS event IDs for the KSEVENTSETID_BdaTunerEvent event set. Defined in bdamedia.h.
  /// </summary>
  public enum TunerEvent
  {
    /// <summary> KSEVENT_BDA_TUNER_SCAN </summary>
    Scan = 0
  }

  #region IESEvent* enums

  public enum RequestTunerEventPriority : byte
  {
    Opportunistic = 0,
    Notify = 1,
    // 2 - 0xfe reserved
    Immediate = 0xff
  }

  public enum RequestTunerEventReason : byte
  {
    Unspecified = 0,
    DownloadInternalUpdate = 1
    // 2 - 0xff reserved
  }

  public enum RequestTunerEventConsequences : byte
  {
    Unspecified = 0,
    RebootRequired = 1,
    // 2 - 0xff reserved
  }

  public enum IsdbCasResponseEventStatus
  {
    Success = 0x9000,
    MemoryError = 0x6400,
    WriteError = 0x6581,
    InterconnectCommandLength = 0x6700,
    UnsupportedClass = 0x6800,
    IncorrectParameter = 0x6a86,
    UnsupportedInstruction = 0x6d00,
    UnsupportedClass2 = 0x6e00
  }

  #endregion

  #endregion

  /// <summary>
  /// KS properties for the IBDA_DigitalDemodulator property set. Defined in bdamedia.h.
  /// </summary>
  public enum BdaDemodulatorProperty
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
    PlpNumber               // physical layer pipe - for DVB-*2
  }

  /// <summary>
  /// The full collection of network types from bdamedia.h.
  /// </summary>
  public static class NetworkType
  {
    /// <summary> DIGITAL_CABLE_NETWORK_TYPE </summary>
    public static readonly Guid DIGITAL_CABLE = new Guid(0x143827ab, 0xf77b, 0x498d, 0x81, 0xca, 0x5a, 0x00, 0x7a, 0xec, 0x28, 0xbf);
    /// <summary> ANALOG_TV_NETWORK_TYPE </summary>
    public static readonly Guid ANALOG_TV = new Guid(0xb820d87e, 0xe0e3, 0x478f, 0x8a, 0x38, 0x4e, 0x13, 0xf7, 0xb3, 0xdf, 0x42);
    /// <summary> ANALOG_AUXIN_NETWORK_TYPE </summary>
    public static readonly Guid ANALOG_AUX_IN = new Guid(0x742ef867, 0x09e1, 0x40a3, 0x82, 0xd3, 0x96, 0x69, 0xba, 0x35, 0x32, 0x5f);
    /// <summary> ANALOG_FM_NETWORK_TYPE </summary>
    public static readonly Guid ANALOG_FM = new Guid(0x7728087b, 0x2bb9, 0x4e30, 0x80, 0x78, 0x44, 0x94, 0x76, 0xe5, 0x9d, 0xbb);
    /// <summary> ISDB_TERRESTRIAL_TV_NETWORK_TYPE </summary>
    public static readonly Guid ISDB_TERRESTRIAL = new Guid(0x95037f6f, 0x3ac7, 0x4452, 0xb6, 0xc4, 0x45, 0xa9, 0xce, 0x92, 0x92, 0xa2);
    /// <summary> ISDB_T_NETWORK_TYPE </summary>
    public static readonly Guid ISDB_T = new Guid(0xfc3855a6, 0xc901, 0x4f2e, 0xab, 0xa8, 0x90, 0x81, 0x5a, 0xfc, 0x6c, 0x83);
    /// <summary> ISDB_SATELLITE_TV_NETWORK_TYPE </summary>
    public static readonly Guid ISDB_SATELLITE = new Guid(0xb0a4e6a0, 0x6a1a, 0x4b83, 0xbb, 0x5b, 0x90, 0x3e, 0x1d, 0x90, 0xe6, 0xb6);
    /// <summary> ISDB_S_NETWORK_TYPE </summary>
    public static readonly Guid ISDB_S = new Guid(0xa1e78202, 0x1459, 0x41b1, 0x9c, 0xa9, 0x2a, 0x92, 0x58, 0x7a, 0x42, 0xcc);
    /// <summary> ISDB_CABLE_TV_NETWORK_TYPE </summary>
    public static readonly Guid ISDB_CABLE = new Guid(0xc974ddb5, 0x41fe, 0x4b25, 0x97, 0x41, 0x92, 0xf0, 0x49, 0xf1, 0xd5, 0xd1);
    /// <summary> DIRECT_TV_SATELLITE_TV_NETWORK_TYPE </summary>
    public static readonly Guid DIRECTV_SATELLITE = new Guid(0x93b66fb5, 0x93d4, 0x4323, 0x92, 0x1c, 0xc1, 0xf5, 0x2d, 0xf6, 0x1d, 0x3f);
    /// <summary> ECHOSTAR_SATELLITE_TV_NETWORK_TYPE </summary>
    public static readonly Guid ECHOSTAR_SATELLITE = new Guid(0xc4f6b31b, 0xc6bf, 0x4759, 0x88, 0x6f, 0xa7, 0x38, 0x6d, 0xca, 0x27, 0xa0);
    /// <summary> ATSC_TERRESTRIAL_TV_NETWORK_TYPE </summary>
    public static readonly Guid ATSC_TERRESTRIAL = new Guid(0x0dad2fdd, 0x5fd7, 0x11d3, 0x8f, 0x50, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe2);
    /// <summary> DVB_TERRESTRIAL_TV_NETWORK_TYPE </summary>
    public static readonly Guid DVB_TERRESTRIAL = new Guid(0x216c62df, 0x6d7f, 0x4e9a, 0x85, 0x71, 0x05, 0xf1, 0x4e, 0xdb, 0x76, 0x6a);
    /// <summary> BSKYB_TERRESTRIAL_TV_NETWORK_TYPE </summary>
    public static readonly Guid BSKYB_TERRESTRIAL = new Guid(0x9e9e46c6, 0x3aba, 0x4f08, 0xad, 0x0e, 0xcc, 0x5a, 0xc8, 0x14, 0x8c, 0x2b);
    /// <summary> DVB_SATELLITE_TV_NETWORK_TYPE </summary>
    public static readonly Guid DVB_SATELLITE = new Guid(0xfa4b375a, 0x45b4, 0x4d45, 0x84, 0x40, 0x26, 0x39, 0x57, 0xb1, 0x16, 0x23);
    /// <summary> DVB_CABLE_TV_NETWORK_TYPE </summary>
    public static readonly Guid DVB_CABLE = new Guid(0xdc0c0fe7, 0x0485, 0x4266, 0xb9, 0x3f, 0x68, 0xfb, 0xf8, 0x0e, 0xd8, 0x34);
  }

  /// <summary>
  /// The full collection of descramble status codes from the PBDA specification.
  /// </summary>
  public enum DescrambleStatus
  {
    FreeToAir = 0,
    DescramblingPossible = 1,
    TechnicalFailOther = unchecked((int)0x80010000),
    FirmwareUpgradeRequired = unchecked((int)0x80010001),
    InternalFailure = unchecked((int)0x80010002),
    InitialisingNotReady = unchecked((int)0x80010003),
    SetupRequired = unchecked((int)0x80010004),
    NoAccessCard = unchecked((int)0x80010005),
    AccessCardFailure = unchecked((int)0x80010006),
    BadAccessCard = unchecked((int)0x80010007),
    WrongAccessCard = unchecked((int)0x80010008),
    ExpiredAccessCard = unchecked((int)0x80010009),
    OutOfResources = unchecked((int)0x8001000a),
    NotInPurchaseWindow = unchecked((int)0x8001000b),
    NotInPurchaseWindowPrior = unchecked((int)0x8001000c),
    NotInPurchaseWindowAfter = unchecked((int)0x8001000d),
    NoEntitlementOther = unchecked((int)0x80020000),
    AccessCardNotAuthorised = unchecked((int)0x80020001),
    ServiceNotAuthorised = unchecked((int)0x80020002),
    ServiceExpired = unchecked((int)0x80020003),
    AccountNotAuthorised = unchecked((int)0x80020004),
    AccountExpired = unchecked((int)0x80020005),
    ServiceBlackedOut = unchecked((int)0x80020006),
    PurchaseRequired = unchecked((int)0x80020007),
    InsufficientCredit = unchecked((int)0x80020008),
    PurchaseCanceled = unchecked((int)0x80020009),
    RenewalEntitlementExpired = unchecked((int)0x8002000a),
    ShowingNotAvailable = unchecked((int)0x8002000b),
    ShowingNext = unchecked((int)0x8002000c)
  }
}