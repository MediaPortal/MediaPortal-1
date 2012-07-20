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
using System.Security;
using DirectShowLib.BDA;
using DirectShowLib.Dvd;
using System.Drawing;
using System.Text;

/// <summary>
/// This file holds the MediaPortal and TV Server interface customisations
/// of the DirectShow.NET library. We keep customisations separate
/// from the original library code wherever possible to reduce maintenance
/// and merging issues when it comes to upgrading the DirectShow.NET library.
/// </summary>

  #region AXExtend.cs

namespace DirectShowLib
{
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
      [Out] out object[] Values,
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
        [Out] out object[] Values,
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
   Guid("71985F47-1CA1-11d3-9CC8-00C04F7971E0"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IBDA_FrequencyFilter
  {
    [PreserveSig]
    int put_Autotune([In] int ulTransponder);

    [PreserveSig]
    int get_Autotune([Out] out int pulTransponder);

    [PreserveSig]
    int put_Frequency([In] int ulFrequency);

    [PreserveSig]
    int get_Frequency([Out] out int pulFrequency);

    [PreserveSig]
    int put_Polarity([In] Polarisation Polarity);

    [PreserveSig]
    int get_Polarity([Out] out Polarisation pPolarity);

    [PreserveSig]
    int put_Range([In] ulong ulRange);        // *** Changed from int to ulong. ***

    [PreserveSig]
    int get_Range([Out] out ulong pulRange);  // *** Changed from int to ulong. ***

    [PreserveSig]
    int put_Bandwidth([In] int ulBandwidth);

    [PreserveSig]
    int get_Bandwidth([Out] out int pulBandwidth);

    [PreserveSig]
    int put_FrequencyMultiplier([In] int ulMultiplier);

    [PreserveSig]
    int get_FrequencyMultiplier([Out] out int pulMultiplier);
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

  // 1. The definition for DsUtils.ReleaseComObject() was added.
  // 2. References to Marshal.ReleaseComObject() were replaced with ReleaseComObject().
  // 3. The while loops in DsFindPin methods have been modified as follows:
  //
  //  ...
  //  int lFetched;
  //  ...
  //    while ((ppEnum.Next(1, pPins, out lFetched) >= 0) && (lFetched == 1))
  //  ...
  //
  //  Note the replacement of IntPtr.Zero with lFetched.

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
    public static readonly Guid AMKSMULTIVBICodec = new Guid(0x9c24a977, 0x0951, 0x451a, 0x80, 0x06, 0x0e, 0x49, 0xbd, 0x28, 0xcd, 0x5f);

    /// <summary> MEDIATYPE_Subtitle 'subs' </summary>
    public static readonly Guid Subtitle = new Guid(0xE487EB08, 0x6B26, 0x4be9, 0x9D, 0xD3, 0x99, 0x34, 0x34, 0xD3, 0x13, 0xFD);
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

    // 56564D41-0000-0010-8000-00AA00389B71
    public static readonly Guid XVID = new Guid(0x44495658, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 56564D41-0000-0010-8000-00AA00389B71
    public static readonly Guid xvid = new Guid(0x64697678, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 30355844-0000-0010-8000-00aa00389b71
    public static readonly Guid DX50 = new Guid(0x30355844, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 30357864-0000-0010-8000-00AA00389B71
    public static readonly Guid dx50 = new Guid(0x30357864, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 56564D41-0000-0010-8000-00AA00389B71
    public static readonly Guid DIVX = new Guid(0x58564944, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    // 78766964-0000-0010-8000-00AA00389B71
    public static readonly Guid divx = new Guid(0x78766964, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

    /// <summary> MEDIASUBTYPE_LATM_AAC </summary>
    public static readonly Guid LATMAAC = new Guid(0x00001FF, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

    /// <summary> MEDIASUBTYPE_LATM_AAC_LAVF_SPLITTER </summary>
    public static readonly Guid LATMAACLAVF = new Guid(0x53544441, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
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
    public UInt32 RequestId;
    public UInt32 PacketLength;
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
    PlpNumber               // physical layer pipe - for DVB-S2 and DVB-T2
  }
}
