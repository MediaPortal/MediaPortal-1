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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using DirectShowLib.BDA;
using DirectShowLib.Dvd;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// This file holds the MediaPortal and TV Server interface customisations of
/// the DirectShow.NET library. We keep customisations separate from the
/// original library code wherever possible to reduce maintenance and merging
/// issues when it comes to upgrading the DirectShow.NET library.
/// </summary>

  #region AXExtend.cs

namespace DirectShowLib
{
  /// <summary>
  /// From AnalogVideoStandard
  /// </summary>
  [Flags]
  public enum AnalogVideoStandard
  {
    None = 0x00000000,
    [Description("NTSC M")]
    NTSC_M = 0x00000001,
    [Description("NTSC J")]
    NTSC_M_J = 0x00000002,
    [Description("NTSC 4.43")]
    NTSC_433 = 0x00000004,
    [Description("PAL B")]
    PAL_B = 0x00000010,
    [Description("PAL D")]
    PAL_D = 0x00000020,
    [Description("PAL G")]
    PAL_G = 0x00000040,
    [Description("PAL H")]
    PAL_H = 0x00000080,
    [Description("PAL I")]
    PAL_I = 0x00000100,
    [Description("PAL M")]
    PAL_M = 0x00000200,
    [Description("PAL N")]
    PAL_N = 0x00000400,
    [Description("PAL 60")]
    PAL_60 = 0x00000800,
    [Description("SECAM B")]
    SECAM_B = 0x00001000,
    [Description("SECAM D")]
    SECAM_D = 0x00002000,
    [Description("SECAM G")]
    SECAM_G = 0x00004000,
    [Description("SECAM H")]
    SECAM_H = 0x00008000,
    [Description("SECAM K")]
    SECAM_K = 0x00010000,
    [Description("SECAM K1")]
    SECAM_K1 = 0x00020000,
    [Description("SECAM L")]
    SECAM_L = 0x00040000,
    [Description("SECAM L1")]
    SECAM_L1 = 0x00080000,
    [Description("PAL N Combo")]
    PAL_N_COMBO = 0x00100000,

    /*NTSCMask = 0x00000007,
    PALMask = 0x00100FF0,
    SECAMMask = 0x000FF000*/
  }

  /// <summary>
  /// From VideoProcAmpProperty
  /// </summary>
  public enum VideoProcAmpProperty
  {
    Brightness,
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
      [Out] out IntPtr Values,  // *** Changed from object[] to IntPtr. ***
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
      [Out] out IntPtr Values,  // *** Changed from object[] to IntPtr. ***
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
      [Out] out int pcReceived  // *** Changed from IntPtr to int. ***
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
      [Out] out int pulcApplications,   // *** Changed from in/out ref to out. ***
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

  #region BDATypes.cs

namespace DirectShowLib.BDA
{
  /// <summary>
  /// From RollOff
  /// </summary>
  public enum RollOff
  {
    [Description("Not Set")]
    NotSet = -1,
    [Description("Not Defined")]
    NotDefined = 0,
    [Description("0.20")]
    Twenty = 1,
    [Description("0.25")]
    TwentyFive,
    [Description("0.35")]
    ThirtyFive
  }

  /// <summary>
  /// From Pilot
  /// </summary>
  public enum Pilot
  {
    [Description("Not Set")]
    NotSet = -1,
    [Description("Not Defined")]
    NotDefined = 0,
    Off = 1,
    On
  }

  /// <summary>
  /// From BinaryConvolutionCodeRate
  /// </summary>
  public enum BinaryConvolutionCodeRate
  {
    [Description("Not Set")]
    RateNotSet = -1,
    [Description("Not Defined")]
    RateNotDefined = 0,
    [Description("1/2")]
    Rate1_2 = 1,
    [Description("2/3")]
    Rate2_3,
    [Description("3/4")]
    Rate3_4,
    [Description("3/5")]
    Rate3_5,
    [Description("4/5")]
    Rate4_5,
    [Description("5/6")]
    Rate5_6,
    [Description("5/11")]
    Rate5_11,
    [Description("7/8")]
    Rate7_8,
    [Description("1/4")]
    Rate1_4,
    [Description("1/3")]
    Rate1_3,
    [Description("2/5")]
    Rate2_5,
    [Description("6/7")]
    Rate6_7,
    [Description("8/9")]
    Rate8_9,
    [Description("9/10")]
    Rate9_10
  }

  /// <summary>
  /// From Polarisation
  /// </summary>
  public enum Polarisation
  {
    [Description("Not Set")]
    NotSet = -1,
    [Description("Not Defined")]
    NotDefined = 0,
    [Description("Linear Horizontal")]
    LinearH = 1,
    [Description("Linear Vertical")]
    LinearV,
    [Description("Circular Left")]
    CircularL,
    [Description("Circular Right")]
    CircularR
  }
}

  #endregion

  #region Control.cs

namespace DirectShowLib
{
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("56a868c0-0ad4-11ce-b03a-0020af0ba770"),
   InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IMediaEventEx : IMediaEvent
  {
    #region IMediaEvent Methods

    [PreserveSig]
    new int GetEventHandle([Out] out IntPtr hEvent); // HEVENT

    [PreserveSig]
    int GetEvent(
      [Out] out EventCode lEventCode,
      [Out] out int lParam1,  // *** Changed from IntPtr to int. ***
      [Out] out int lParam2,  // *** Changed from IntPtr to int. ***
      [In] int msTimeout
      );

    [PreserveSig]
    new int WaitForCompletion(
      [In] int msTimeout,
      [Out] out EventCode pEvCode
      );

    [PreserveSig]
    new int CancelDefaultHandling([In] EventCode lEvCode);

    [PreserveSig]
    new int RestoreDefaultHandling([In] EventCode lEvCode);

    [PreserveSig]
    int FreeEventParams(
      [In] EventCode lEvCode,
      [In] int lParam1,       // *** Changed from IntPtr to int. ***
      [In] int lParam2        // *** Changed from IntPtr to int. ***
      );

    #endregion

    [PreserveSig]
    int SetNotifyWindow(
      [In] IntPtr hwnd, // HWND *
      [In] int lMsg,
      [In] IntPtr lInstanceData // PVOID
      );

    [PreserveSig]
    int SetNotifyFlags([In] NotifyFlags lNoNotifyFlags);

    [PreserveSig]
    int GetNotifyFlags([Out] out NotifyFlags lplNoNotifyFlags);
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

    /// <summary>
    /// Get the product instance identifier.
    /// </summary>
    /// <remarks>
    /// In most cases the product instance identifier is extracted from the device path (IMoniker
    /// display name).
    /// General device path elements:
    /// @device:[type: cm|pnp|sw]:\\?\[connection: hdaudio|pci|root|stream|usb]#[product info: vendor, device, product, subsystem]#[product instance identifier]#[category GUID]\[component instance GUID/CLSID]
    /// 
    /// Examples:
    /// @device:cm:{33D9A762-90C8-11D0-BD43-00A0C911CE86}\7162 BDA Audio Capture
    /// @device:pnp:\\?\usb#vid_1b80&pid_d393#5&10ef021e&0&2#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{9d4afc32-0f42-45d9-b590-af9295699871}
    /// @device:pnp:\\?\stream#hcw88bar.cfg92#5&35edf2e&7&0#{a799a801-a46d-11d0-a18c-00a02401dcd4}\global
    /// @device:pnp:\\?\pci#ven_1131&dev_7162&subsys_010111bd&rev_01#4&1215b326&0&0018#{a799a800-a46d-11d0-a18c-00a02401dcd4}\{62b08a3e-335e-4b30-90f9-2ba400000000}
    /// @device:pnp:\\?\root#system#0000#{fd0a5af4-b41d-11d2-9c95-00c04f7971e0}\{03884cb6-e89a-4deb-b69e-8dc621686e6a}&global
    /// @device:sw:{71985F48-1CA1-11D3-9CC8-00C04F7971E0}\Silicondust HDHomeRun Tuner 1000101F-0
    /// @device:pnp:\\?\hdaudio#func_01&ven_10ec&dev_0882&subsys_1043e601&rev_1001#4&225f9914&0&0001#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\rearlineinwave3
    /// </remarks>
    /// <param name="devicePath">The device path to analyse.</param>
    /// <returns>the product instance identifier if successful, otherwise <c>null</c></returns>
    public string ProductInstanceIdentifier
    {
      get
      {
        //---------------------------
        // Hardware-specific methods.
        //---------------------------
        string name = Name;
        if (name != null)
        {
          if (name.Contains("Ceton"))
          {
            // Example: Ceton InfiniTV PCIe (00-80-75-05) Tuner 1 (00-00-22-00-00-80-75-05)
            Match m = Regex.Match(name, @"\s+\(([^\s]+)\)\s+Tuner\s+\d+", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              return m.Groups[1].Captures[0].Value;
            }
          }
          else if (name.Contains("HDHomeRun") || name.StartsWith("Hauppauge OpenCable Receiver"))
          {
            // Examples: HDHomeRun Prime Tuner 1316890F-1, Hauppauge OpenCable Receiver 201200AA-1
            Match m = Regex.Match(name, @"\s+([^\s]+)-\d$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              return m.Groups[1].Captures[0].Value;
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

        // The third section is the product instance identifier. Note the first
        // and second sections are usually identical for all components (digital
        // and analog) of PnP USB and PCI hardware, but they are ***not***
        // usually identical for hardware with stream class drivers.
        // Hardware component identifiers that we can interpret contain 4
        // sections separated by &. Again, all 4 parts are usually identical for
        // PnP USB and PCI hardware components. For hardware with stream class
        // drivers, the first three parts are identical and the last part is a
        // unique component identifier.
        string productInstanceIdentifier = sections[2];
        sections = productInstanceIdentifier.Split('&');
        if (sections.Length == 1)
        {
          return productInstanceIdentifier;
        }
        if (sections.Length != 4)
        {
          return null;
        }
        return sections[0] + '&' + sections[1] + '&' + sections[2];
      }
    }

    /// <summary>
    /// Get the tuner instance identifier.
    /// </summary>
    public int TunerInstanceIdentifier
    {
      get
      {
        //---------------------------
        // Hardware-specific methods.
        //---------------------------
        Match m;
        string name = Name;
        if (name != null)
        {
          // Ceton CableCARD tuners.
          if (name.Contains("Ceton"))
          {
            // Example: Ceton InfiniTV PCIe (00-80-75-05) Tuner 1 (00-00-22-00-00-80-75-05)
            m = Regex.Match(name, @"\s+\([^\s]+\)\s+Tuner\s+(\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              return Int32.Parse(m.Groups[1].Captures[0].Value);
            }
          }
          // Silicondust HDHomeRun tuners (Hauppauge CableCARD tuners are clones)
          else if (name.Contains("HDHomeRun") || name.StartsWith("Hauppauge OpenCable Receiver"))
          {
            // Examples: HDHomeRun Prime Tuner 1316890F-1, Hauppauge OpenCable Receiver 201200AA-1
            m = Regex.Match(name, @"\s+[^\s]+-(\d)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              return Int32.Parse(m.Groups[1].Captures[0].Value);
            }
          }
        }

        string devicePath = DevicePath;
        if (devicePath == null)
        {
          return -1;
        }

        // Digital Devices tuners.
        // The device path contains two digits that identify the tuner type and
        // instance. The second digit is the zero-indexed tuner identifier.
        m = Regex.Match(devicePath, @"8b884e\d(\d)-fbca-11de-b16f-000000004d56", RegexOptions.IgnoreCase);
        if (m.Success)
        {
          return Int32.Parse(m.Groups[1].Captures[0].Value);
        }

        //----------------
        // Generic method.
        //----------------
        // Drivers may store a TunerInstanceID in the registry as a means to
        // indicate hardware grouping to the OS. It is related to W7 container
        // IDs:
        // http://msdn.microsoft.com/en-us/library/windows/hardware/ff540024%28v=vs.85%29.aspx
        object id = GetPropBagValue("TunerInstanceID");
        if (id != null)
        {
          return (Int32)id;
        }
        return -1;
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

    public void Dispose()
    {
      if (Mon != null)
      {
        Marshal.ReleaseComObject(Mon);
        m_Mon = null;
      }
      m_Name = null;
    }
  }
}

  #endregion

  #region DVDIf.cs

namespace DirectShowLib.Dvd
{
  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("34151510-EEC0-11D2-8201-00A0C9D74842"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvdInfo2
  {
    [PreserveSig]
    int GetCurrentDomain([Out] out DvdDomain pDomain);

    [PreserveSig]
    int GetCurrentLocation([Out] out DvdPlaybackLocation2 pLocation);

    [PreserveSig]
    int GetTotalTitleTime(
      [Out] DvdHMSFTimeCode pTotalTime,
      [Out] out DvdTimeCodeFlags ulTimeCodeFlags
      );

    [PreserveSig]
    int GetCurrentButton(
      [Out] out int pulButtonsAvailable,
      [Out] out int pulCurrentButton
      );

    [PreserveSig]
    int GetCurrentAngle(
      [Out] out int pulAnglesAvailable,
      [Out] out int pulCurrentAngle
      );

    [PreserveSig]
    int GetCurrentAudio(
      [Out] out int pulStreamsAvailable,
      [Out] out int pulCurrentStream
      );

    [PreserveSig]
    int GetCurrentSubpicture(
      [Out] out int pulStreamsAvailable,
      [Out] out int pulCurrentStream,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pbIsDisabled
      );

    [PreserveSig]
    int GetCurrentUOPS([Out] out ValidUOPFlag pulUOPs);

    [PreserveSig]
    int GetAllSPRMs([Out] out SPRMArray pRegisterArray);

    [PreserveSig]
    int GetAllGPRMs([Out] out GPRMArray pRegisterArray);

    [PreserveSig]
    int GetAudioLanguage(
      [In] int ulStream,
      [Out] out int pLanguage
      );

    [PreserveSig]
    int GetSubpictureLanguage(
      [In] int ulStream,
      [Out] out int pLanguage
      );

    [PreserveSig]
    int GetTitleAttributes(
      [In] int ulTitle,
      [Out] out DvdMenuAttributes pMenu,
      [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DTAMarshaler))] DvdTitleAttributes
        pTitle
      );

    [PreserveSig]
    int GetVMGAttributes([Out] out DvdMenuAttributes pATR);

    [PreserveSig]
    int GetCurrentVideoAttributes([Out] out DvdVideoAttributes pATR);

    [PreserveSig]
    int GetAudioAttributes(
      [In] int ulStream,
      [Out] out DvdAudioAttributes pATR
      );

    [PreserveSig]
    int GetKaraokeAttributes(
      [In] int ulStream,
      [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DKAMarshaler))] DvdKaraokeAttributes
        pAttributes
      );

    [PreserveSig]
    int GetSubpictureAttributes(
      [In] int ulStream,
      [Out] out DvdSubpictureAttributes pATR
      );

    [PreserveSig]
    int GetDVDVolumeInfo(
      [Out] out int pulNumOfVolumes,
      [Out] out int pulVolume,
      [Out] out DvdDiscSide pSide,
      [Out] out int pulNumOfTitles
      );

    [PreserveSig]
    int GetDVDTextNumberOfLanguages([Out] out int pulNumOfLangs);

    [PreserveSig]
    int GetDVDTextLanguageInfo(
      [In] int ulLangIndex,
      [Out] out int pulNumOfStrings,
      [Out] out int pLangCode,
      [Out] out DvdTextCharSet pbCharacterSet
      );

    [PreserveSig]
    int GetDVDTextStringAsNative(
      [In] int ulLangIndex,
      [In] int ulStringIndex,
      [MarshalAs(UnmanagedType.LPStr)] StringBuilder pbBuffer,
      [In] int ulMaxBufferSize,
      [Out] out int pulActualSize,
      [Out] out DvdTextStringType pType
      );

    [PreserveSig]
    int GetDVDTextStringAsUnicode(
      [In] int ulLangIndex,
      [In] int ulStringIndex,
      StringBuilder pchwBuffer,
      [In] int ulMaxBufferSize,
      [Out] out int pulActualSize,
      [Out] out DvdTextStringType pType
      );

    [PreserveSig]
    int GetPlayerParentalLevel(
      [Out] out int pulParentalLevel,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] byte[] pbCountryCode
      );

    [PreserveSig]
    int GetNumberOfChapters(
      [In] int ulTitle,
      [Out] out int pulNumOfChapters
      );

    [PreserveSig]
    int GetTitleParentalLevels(
      [In] int ulTitle,
      [Out] out DvdParentalLevel pulParentalLevels
      );

    [PreserveSig]
    int GetDVDDirectory(
      StringBuilder pszwPath,
      [In] int ulMaxSize,
      [Out] out int pulActualSize
      );

    [PreserveSig]
    int IsAudioStreamEnabled(
      [In] int ulStreamNum,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pbEnabled
      );

    [PreserveSig]
    int GetDiscID(
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszwPath,
      [Out] out long pullDiscID
      );

    [PreserveSig]
    int GetState([Out] out IDvdState pStateData);

    [PreserveSig]
    int GetMenuLanguages(
      [MarshalAs(UnmanagedType.LPArray)] int[] pLanguages,
      [In] int ulMaxLanguages,
      [Out] out int pulActualLanguages
      );

    [PreserveSig]
    int GetButtonAtPosition(
      [In] Point point,
      [Out] out int pulButtonIndex
      );

    [PreserveSig]
    int GetCmdFromEvent(
      [In] int lParam1,         // *** Changed from IntPtr to int. ***
      [Out] out IDvdCmd pCmdObj
      );

    [PreserveSig]
    int GetDefaultMenuLanguage([Out] out int pLanguage);

    [PreserveSig]
    int GetDefaultAudioLanguage(
      [Out] out int pLanguage,
      [Out] out DvdAudioLangExt pAudioExtension
      );

    [PreserveSig]
    int GetDefaultSubpictureLanguage(
      [Out] out int pLanguage,
      [Out] out DvdSubPictureLangExt pSubpictureExtension
      );

    [PreserveSig]
    int GetDecoderCaps(ref DvdDecoderCaps pCaps);

    [PreserveSig]
    int GetButtonRect(
      [In] int ulButton,
      [Out] DsRect pRect
      );

    [PreserveSig]
    int IsSubpictureStreamEnabled(
      [In] int ulStreamNum,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pbEnabled
      );
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
      [Out] out int pceltFetched    // *** Changed from IntPtr to int. ***
      );

    int Skip([In] int celt);

    int Reset();

    int Clone([Out] out IEnumTuningSpaces ppEnum);
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
  public static class MediaPortalGuid
  {
    /// <summary> AM_KSCATEGORY_MULTIVBICODEC </summary>
    public static readonly Guid AM_KS_CATEGORY_MULTI_VBI_CODEC = new Guid(0x9c24a977, 0x0951, 0x451a, 0x80, 0x06, 0x0e, 0x49, 0xbd, 0x28, 0xcd, 0x5f);

    /// <summary> KSNODE_BDA_8PSK_DEMODULATOR </summary>
    public static readonly Guid KS_NODE_BDA_8PSK_DEMODULATOR = new Guid(0xe957a0e7, 0xdd98, 0x4a3c, 0x81, 0x0b, 0x35, 0x25, 0x15, 0x7a, 0xb6, 0x2e);
    /// <summary> KSNODE_BDA_ISDB_S_DEMODULATOR </summary>
    public static readonly Guid KS_NODE_BDA_ISDB_S_DEMODULATOR = new Guid(0xedde230a, 0x9086, 0x432d, 0xb8, 0xa5, 0x66, 0x70, 0x26, 0x38, 0x07, 0xe9);
    /// <summary> KSNODE_BDA_ISDB_T_DEMODULATOR </summary>
    public static readonly Guid KS_NODE_BDA_ISDB_T_DEMODULATOR = new Guid(0xfcea3ae3, 0x2cb2, 0x464d, 0x8f, 0x5d, 0x30, 0x5c, 0x0b, 0xb7, 0x78, 0xa2);

    /// <summary> KSMEDIUMSETID_Standard </summary>
    public static readonly Guid KS_MEDIUM_SET_ID_STANDARD = new Guid(0x4747b320, 0x62ce, 0x11cf, 0xa5, 0xd6, 0x28, 0xdb, 0x04, 0xc1, 0x00, 0x00);

    /// <summary> MEDIATYPE_Subtitle 'subs' </summary>
    public static readonly Guid Subtitle = new Guid(0xE487EB08, 0x6B26, 0x4be9, 0x9D, 0xD3, 0x99, 0x34, 0x34, 0xD3, 0x13, 0xFD);
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
    public static readonly Guid ANALOG_AUX_IN = new Guid(0x742EF867, 0x9E1, 0x40A3, 0x82, 0xD3, 0x96, 0x69, 0xBA, 0x35, 0x32, 0x5F);
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
  /// A partial collection of codec API parameters from codecapi.h.
  /// </summary>
  public static class CodecApiParameter
  {
    /// <summary> AVAudioSampleRate </summary>
    public static readonly Guid AV_AUDIO_SAMPLE_RATE = new Guid(0x971d2723, 0x1acb, 0x42e7, 0x85, 0x5c, 0x52, 0x0a, 0x4b, 0x70, 0xa5, 0xf2);
    /// <summary> AVEncAudioMeanBitRate </summary>
    public static readonly Guid AV_ENC_AUDIO_MEAN_BIT_RATE = new Guid(0x921295bb, 0x4fca, 0x4679, 0xaa, 0xb8, 0x9e, 0x2a, 0x1d, 0x75, 0x33, 0x84);
    /// <summary> AVEncCommonMaxBitRate </summary>
    public static readonly Guid AV_ENC_COMMON_MAX_BIT_RATE = new Guid(0x9651eae4, 0x39b9, 0x4ebf, 0x85, 0xef, 0xd7, 0xf4, 0x44, 0xec, 0x74, 0x65);
    /// <summary> AVEncCommonMeanBitRate </summary>
    public static readonly Guid AV_ENC_COMMON_MEAN_BIT_RATE = new Guid(0xf7222374, 0x2144, 0x4815, 0xb5, 0x50, 0xa3, 0x7f, 0x8e, 0x12, 0xee, 0x52);
    /// <summary> AVEncCommonRateControlMode </summary>
    public static readonly Guid AV_ENC_COMMON_RATE_CONTROL_MODE = new Guid(0x1c0608e9, 0x370c, 0x4710, 0x8a, 0x58, 0xcb, 0x61, 0x81, 0xc4, 0x24, 0x23);
  }

  /// <summary>
  /// Supported values for codec API parameter AVEncCommonRateControlMode.
  /// </summary>
  public enum eAVEncCommonRateControlMode
  {
    CBR = 0,
    PeakConstrainedVBR = 1,
    UnconstrainedVBR = 2,
    Quality = 3
  }

  /// <summary>
  /// A collection of MEDIATYPE definitions not found elsewhere in DirectShow.NET.
  /// </summary>
  public static class MpMediaSubType
  {
    public static readonly Guid AAC = new Guid(0x00000FF, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

    public static readonly Guid DDPLUS = new Guid(0xa7fb87af, 0x2d02, 0x42fb,0xa4, 0xd4, 0x05, 0xcd, 0x93, 0x84, 0x3b, 0xdd);

    public static readonly Guid DVD_LPCM_AUDIO = new Guid(0xe06d8032, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x05f, 0x6c, 0xbb, 0xea);

    /// <summary> MEDIASUBTYPE_BDA_MPEG2_TRANSPORT </summary>
    public static readonly Guid BdaMpeg2Transport = new Guid(0xF4AEB342, 0x0329, 0x4fdd, 0xA8, 0xFD, 0x4A, 0xFF, 0x49, 0x26, 0xC9, 0x78);

    /// <summary> MEDIASUBTYPE_VC1 </summary>
    public static readonly Guid VC1 = new Guid(0x31435657, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    /// <summary> MEDIASUBTYPE_VC1_Cyberlink </summary>
    public static readonly Guid CyberlinkVC1 = new Guid(0xD979F77B, 0xDBEA, 0x4BF6, 0x9E, 0x6D, 0x1D, 0x7E, 0x57, 0xFB, 0xAD, 0x53);

    /// <summary> MEDIASUBTYPE_AVC1 </summary>
    public static readonly Guid AVC1 = new Guid(0x31435641, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

    // 44495658-0000-0010-8000-00AA00389B71
    public static readonly Guid XVID1 = new Guid(0x44495658, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 64697678-0000-0010-8000-00AA00389B71
    public static readonly Guid XVID2 = new Guid(0x64697678, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 30355844-0000-0010-8000-00aa00389b71
    public static readonly Guid DX50_1 = new Guid(0x30355844, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 30357864-0000-0010-8000-00AA00389B71
    public static readonly Guid DX50_2 = new Guid(0x30357864, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 58564944-0000-0010-8000-00AA00389B71
    public static readonly Guid DIVX1 = new Guid(0x58564944, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 78766964-0000-0010-8000-00AA00389B71
    public static readonly Guid DIVX2 = new Guid(0x78766964, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    /// <summary> MEDIASUBTYPE_LATM_AAC </summary>
    public static readonly Guid LATMAAC = new Guid(0x00001FF, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

    /// <summary> MEDIASUBTYPE_LATM_AAC_LAVF_SPLITTER </summary>
    public static readonly Guid LATMAACLAVF = new Guid(0x53544441, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
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
      [In] Int32 PropertyLength,
      [In, Out] IntPtr PropertyData,
      [In] Int32 DataLength,
      [In, Out] ref Int32 BytesReturned
    );

    [PreserveSig]
    int KsMethod(
      [In] ref KsMethod Method,
      [In] Int32 MethodLength,
      [In, Out] IntPtr MethodData,
      [In] Int32 DataLength,
      [In, Out] ref Int32 BytesReturned
    );

    [PreserveSig]
    int KsEvent(
      [In, Optional] ref KsEvent Event,
      [In] Int32 EventLength,
      [In, Out] IntPtr EventData,
      [In] Int32 DataLength,
      [In, Out] ref Int32 BytesReturned
    );
  }

  public struct KsProperty
  {
    public Guid Set;
    public Int32 Id;
    public KsPropertyFlag Flags;

    public KsProperty(Guid set, Int32 id, KsPropertyFlag flags)
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

  public struct KsMethod
  {
    public Guid Set;
    public Int32 Id;
    public KsMethodFlag Flags;

    public KsMethod(Guid set, Int32 id, KsMethodFlag flags)
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

  public struct KsEvent
  {
    public Guid Set;
    public Int32 Id;
    public KsEventFlag Flags;

    public KsEvent(Guid set, Int32 id, KsEventFlag flags)
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

  /// <summary>
  /// KS properties for IBDA_DigitalDemodulator property set. Defined in bdamedia.h.
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
}