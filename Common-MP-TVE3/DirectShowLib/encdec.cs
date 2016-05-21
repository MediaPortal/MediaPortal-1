#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2007
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System.Runtime.InteropServices;
using System.Security;

namespace DirectShowLib.BDA
{

  #region Declarations

  /// <summary>
  /// From ProtType
  /// </summary>
  public enum ProtType
  {
    None = 0,
    Free = 1,
    Once = 2,
    Never = 3,
    NeverReally = 4,
    NoMore = 5,
    FreeCit = 6,
    BF = 7,
    CnRecordingStop = 8,
    FreeSecure = 9,
    Invalid = 50
  }

  #endregion

  #region Interfaces

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("FAF37694-909C-49cd-886F-C7382E5DB596"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDTFilterBlockedOverlay
  {
    [PreserveSig]
    int SetOverlay(
      int dwOverlayCause
      );

    [PreserveSig]
    int ClearOverlay(
      int dwOverlayCause
      );

    [PreserveSig]
    int GetOverlay(
      out int pdwOverlayCause
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4C2-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IDTFilterEvents
  {
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4C1-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IETFilterEvents
  {
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4C3-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IXDSCodecEvents
  {
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4B1-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IETFilter
  {
    [PreserveSig]
    int get_EvalRatObjOK(
      out int pHrCoCreateRetVal
      );

    [PreserveSig]
    int GetCurrRating(
      out EnTvRat_System pEnSystem,
      out EnTvRat_GenericLevel pEnRating,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    int GetCurrLicenseExpDate(
      ProtType protType,
      out int lpDateTime
      );

    [PreserveSig]
    int GetLastErrorCode();

    [PreserveSig]
    int SetRecordingOn(
      [MarshalAs(UnmanagedType.Bool)] bool fRecState
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4B3-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IXDSCodec
  {
    [PreserveSig]
    int get_XDSToRatObjOK(
      out int pHrCoCreateRetVal
      );

    [PreserveSig]
    int put_CCSubstreamService(
      int SubstreamMask
      );

    [PreserveSig]
    int get_CCSubstreamService(
      out int pSubstreamMask
      );

    [PreserveSig]
    int GetContentAdvisoryRating(
      out int pRat,
      out int pPktSeqID,
      out int pCallSeqID,
      out long pTimeStart,
      out long pTimeEnd
      );

    [PreserveSig]
    int GetXDSPacket(
      out int pXDSClassPkt,
      out int pXDSTypePkt,
      [MarshalAs(UnmanagedType.BStr)] out string pBstrXDSPkt,
      out int pPktSeqID,
      out int pCallSeqID,
      out long pTimeStart,
      out long pTimeEnd
      );

    [PreserveSig]
    int GetCurrLicenseExpDate(
      ProtType protType,
      out int lpDateTime
      );

    [PreserveSig]
    int GetLastErrorCode();
  }


  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4D3-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IXDSCodecConfig
  {
    [PreserveSig]
    int GetSecureChannelObject(
      [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkDRMSecureChannel
      );

    [PreserveSig]
    int SetPauseBufferTime(
      int dwPauseBufferTime
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4B2-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDTFilter
  {
    [PreserveSig]
    int get_EvalRatObjOK(
      out int pHrCoCreateRetVal
      );

    [PreserveSig]
    int GetCurrRating(
      out EnTvRat_System pEnSystem,
      out EnTvRat_GenericLevel pEnRating,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    int get_BlockedRatingAttributes(
      EnTvRat_System enSystem,
      EnTvRat_GenericLevel enLevel,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    int put_BlockedRatingAttributes(
      EnTvRat_System enSystem,
      EnTvRat_GenericLevel enLevel,
      BfEnTvRat_GenericAttributes lbfAttrs
      );

    [PreserveSig]
    int get_BlockUnRated(
      [MarshalAs(UnmanagedType.Bool)] out bool pfBlockUnRatedShows
      );

    [PreserveSig]
    int put_BlockUnRated(
      [MarshalAs(UnmanagedType.Bool)] bool fBlockUnRatedShows
      );

    [PreserveSig]
    int get_BlockUnRatedDelay(
      out int pmsecsDelayBeforeBlock
      );

    [PreserveSig]
    int put_BlockUnRatedDelay(
      int msecsDelayBeforeBlock
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4B4-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDTFilter2 : IDTFilter
  {
    #region IDTFilter methods

    [PreserveSig]
    new int get_EvalRatObjOK(
      out int pHrCoCreateRetVal
      );

    [PreserveSig]
    new int GetCurrRating(
      out EnTvRat_System pEnSystem,
      out EnTvRat_GenericLevel pEnRating,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    new int get_BlockedRatingAttributes(
      EnTvRat_System enSystem,
      EnTvRat_GenericLevel enLevel,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    new int put_BlockedRatingAttributes(
      EnTvRat_System enSystem,
      EnTvRat_GenericLevel enLevel,
      BfEnTvRat_GenericAttributes lbfAttrs
      );

    [PreserveSig]
    new int get_BlockUnRated(
      [MarshalAs(UnmanagedType.Bool)] out bool pfBlockUnRatedShows
      );

    [PreserveSig]
    new int put_BlockUnRated(
      [MarshalAs(UnmanagedType.Bool)] bool fBlockUnRatedShows
      );

    [PreserveSig]
    new int get_BlockUnRatedDelay(
      out int pmsecsDelayBeforeBlock
      );

    [PreserveSig]
    new int put_BlockUnRatedDelay(
      int msecsDelayBeforeBlock
      );

    #endregion

    [PreserveSig]
    int get_ChallengeUrl(
      [MarshalAs(UnmanagedType.BStr)] out string pbstrChallengeUrl
      );

    [PreserveSig]
    int GetCurrLicenseExpDate(
      ProtType protType,
      out int lpDateTime
      );

    [PreserveSig]
    int GetLastErrorCode();
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4D1-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IETFilterConfig
  {
    [PreserveSig]
    int InitLicense(
      int LicenseId
      );

    [PreserveSig]
    int GetSecureChannelObject(
      [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkDRMSecureChannel
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C4C4C4D2-0049-4E2B-98FB-9537F6CE516D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDTFilterConfig
  {
    [PreserveSig]
    int GetSecureChannelObject(
      [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkDRMSecureChannel
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("513998cc-e929-4cdf-9fbd-bad1e0314866"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDTFilter3 : IDTFilter2
  {
    #region IDTFilter methods

    [PreserveSig]
    new int get_EvalRatObjOK(
      out int pHrCoCreateRetVal
      );

    [PreserveSig]
    new int GetCurrRating(
      out EnTvRat_System pEnSystem,
      out EnTvRat_GenericLevel pEnRating,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    new int get_BlockedRatingAttributes(
      EnTvRat_System enSystem,
      EnTvRat_GenericLevel enLevel,
      out BfEnTvRat_GenericAttributes plbfEnAttr
      );

    [PreserveSig]
    new int put_BlockedRatingAttributes(
      EnTvRat_System enSystem,
      EnTvRat_GenericLevel enLevel,
      BfEnTvRat_GenericAttributes lbfAttrs
      );

    [PreserveSig]
    new int get_BlockUnRated(
      [MarshalAs(UnmanagedType.Bool)] out bool pfBlockUnRatedShows
      );

    [PreserveSig]
    new int put_BlockUnRated(
      [MarshalAs(UnmanagedType.Bool)] bool fBlockUnRatedShows
      );

    [PreserveSig]
    new int get_BlockUnRatedDelay(
      out int pmsecsDelayBeforeBlock
      );

    [PreserveSig]
    new int put_BlockUnRatedDelay(
      int msecsDelayBeforeBlock
      );

    #endregion

    #region IDTFilter2 methods

    [PreserveSig]
    new int get_ChallengeUrl(
      [MarshalAs(UnmanagedType.BStr)] out string pbstrChallengeUrl
      );

    [PreserveSig]
    new int GetCurrLicenseExpDate(
      ProtType protType,
      out int lpDateTime
      );

    [PreserveSig]
    new int GetLastErrorCode();

    #endregion

    [PreserveSig]
    int GetProtectionType(
      out ProtType pProtectionType
      );

    [PreserveSig]
    int LicenseHasExpirationDate(
      [MarshalAs(UnmanagedType.Bool)] out bool pfLicenseHasExpirationDate
      );

    [PreserveSig]
    int SetRights(
      [MarshalAs(UnmanagedType.BStr)] string bstrRights
      );
  }

  #endregion
}