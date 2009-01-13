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

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DirectShowLib.BDA
{

  #region Declarations

  /// <summary>
  /// Define possible values for a running_status field according to ETSI EN 300 468
  /// This enum doesn't exist in the c++ headers
  /// </summary>
  public enum RunningStatus : byte
  {
    Undefined = 0,
    NotRunning = 1,
    StartInAFewSeconds = 2,
    Pausing = 3,
    Running = 4,
    Reserved1 = 5,
    Reserved2 = 6,
    Reserved3 = 7
  }

  #endregion

  #region Interfaces

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("B758A7BD-14DC-449d-B828-35909ACB3B1E"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbSiParser
  {
    [PreserveSig]
    int Initialize([In] IMpeg2Data punkMpeg2Data);

    [PreserveSig]
    int GetPAT([Out] out IPAT ppPAT);

    [PreserveSig]
    int GetCAT(
      [In] int dwTimeout,
      [Out] out ICAT ppCAT
      );

    [PreserveSig]
    int GetPMT(
      [In] short pid,
      [In] DsShort pwProgramNumber,
      [Out] out IPMT ppPMT
      );

    [PreserveSig]
    int GetTSDT([Out] out ITSDT ppTSDT);

    [PreserveSig]
    int GetNIT(
      [In] byte tableId,
      [In] DsShort pwNetworkId,
      [Out] out IDVB_NIT ppNIT
      );

    [PreserveSig]
    int GetSDT(
      [In] byte tableId,
      [In] DsShort pwTransportStreamId,
      [Out] out IDVB_SDT ppSDT
      );

    [PreserveSig]
    int GetEIT(
      [In] byte tableId,
      [In] DsShort pwServiceId,
      [Out] out IDVB_EIT ppEIT
      );

    [PreserveSig]
    int GetBAT(
      [In] DsShort pwBouquetId,
      [Out] out IDVB_BAT ppBAT
      );

    [PreserveSig]
    int GetRST(
      [In] int dwTimeout,
      [Out] out IDVB_RST ppRST
      );

    [PreserveSig]
    int GetST(
      [In] short pid,
      [In] int dwTimeout,
      [Out] out IDVB_ST ppST
      );

    [PreserveSig]
    int GetTDT([Out] out IDVB_TDT ppTDT);

    [PreserveSig]
    int GetTOT([Out] out IDVB_TOT ppTOT);

    [PreserveSig]
    int GetDIT(
      [In] int dwTimeout,
      [Out] out IDVB_DIT ppDIT
      );

    [PreserveSig]
    int GetSIT(
      [In] int dwTimeout,
      [Out] out IDVB_SIT ppSIT
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("F47DCD04-1E23-4fb7-9F96-B40EEAD10B2B"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_RST
  {
    [PreserveSig]
    int Initialize([In] ISectionList pSectionList);

    [PreserveSig]
    int GetCountOfRecords([Out] out int pdwVal);

    [PreserveSig]
    int GetRecordTransportStreamId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordOriginalNetworkId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordServiceId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordEventId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordRunningStatus(
      [In] int dwRecordIndex,
      [Out] out RunningStatus pbVal
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("4D5B9F23-2A02-45de-BCDA-5D5DBFBFBE62"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_ST
  {
    [PreserveSig]
    int Initialize([In] ISectionList pSectionList);

    [PreserveSig]
    int GetDataLength([Out] out short pwVal);

    [PreserveSig]
    int GetData([Out] out IntPtr ppData);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("0780DC7D-D55C-4aef-97E6-6B75906E2796"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_TDT
  {
    [PreserveSig]
    int Initialize([In] ISectionList pSectionList);

    [PreserveSig]
    int GetUTCTime([Out] out MpegDateAndTime pmdtVal);
  }


  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("91BFFDF9-9432-410f-86EF-1C228ED0AD70"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_DIT
  {
    [PreserveSig]
    int Initialize([In] ISectionList pSectionList);

    [PreserveSig]
    int GetTransitionFlag([Out, MarshalAs(UnmanagedType.Bool)] out bool pfVal);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("68CDCE53-8BEA-45c2-9D9D-ACF575A089B5"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_SIT
  {
    [PreserveSig]
    int Initialize(
      [In] ISectionList pSectionList,
      [In] IMpeg2Data pMPEGData
      );

    [PreserveSig]
    int GetVersionNumber([Out] out byte pbVal);

    [PreserveSig]
    int GetCountOfTableDescriptors([Out] out int pdwVal);

    [PreserveSig]
    int GetTableDescriptorByIndex(
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetTableDescriptorByTag(
      [In] byte bTag,
      [In] IntPtr pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetCountOfRecords([Out] out int pdwVal);

    [PreserveSig]
    int GetRecordServiceId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordRunningStatus(
      [In] int dwRecordIndex,
      [Out] out RunningStatus pbVal
      );

    [PreserveSig]
    int GetRecordCountOfDescriptors(
      [In] int dwRecordIndex,
      [Out] out int pdwVal
      );

    [PreserveSig]
    int GetRecordDescriptorByIndex(
      [In] int dwRecordIndex,
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetRecordDescriptorByTag(
      [In] int dwRecordIndex,
      [In] byte bTag,
      [In, Out] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int RegisterForNextTable([In] IntPtr hNextTableAvailable);

    [PreserveSig]
    int GetNextTable(
      [In] int dwTimeout,
      [Out] out IDVB_SIT ppSIT
      );

    [PreserveSig]
    int RegisterForWhenCurrent([In] IntPtr hNextTableIsCurrent);

    [PreserveSig]
    int ConvertNextToCurrent();
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("DFB98E36-9E1A-4862-9946-993A4E59017B"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbCableDeliverySystemDescriptor
  {
    [PreserveSig]
    int GetTag([Out] out byte pbVal);

    [PreserveSig]
    int GetLength([Out] out byte pbVal);

    [PreserveSig]
    int GetFrequency([Out] out int pdwVal);

    [PreserveSig]
    int GetFECOuter([Out] out byte pbVal);

    [PreserveSig]
    int GetModulation([Out] out byte pbVal);

    [PreserveSig]
    int GetSymbolRate([Out] out int pdwVal);

    [PreserveSig]
    int GetFECInner([Out] out byte pbVal);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("1CADB613-E1DD-4512-AFA8-BB7A007EF8B1"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbFrequencyListDescriptor
  {
    [PreserveSig]
    int GetTag([Out] out byte pbVal);

    [PreserveSig]
    int GetLength([Out] out byte pbVal);

    [PreserveSig]
    int GetCodingType([Out] out byte pbVal);

    [PreserveSig]
    int GetCountOfRecords([Out] out byte pbVal);

    [PreserveSig]
    int GetRecordCentreFrequency(
      [In] byte bRecordIndex,
      [Out] out int pdwVal
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("F9C7FBCF-E2D6-464d-B32D-2EF526E49290"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbServiceDescriptor
  {
    [PreserveSig]
    int GetTag([Out] out byte pbVal);

    [PreserveSig]
    int GetLength([Out] out byte pbVal);

    [PreserveSig]
    int GetServiceType([Out] out byte pbVal);

    [PreserveSig]
    int GetServiceProviderName([Out, MarshalAs(UnmanagedType.LPStr)] out string pszName);

    [PreserveSig]
    int GetServiceProviderNameW([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrName);

    [PreserveSig]
    int GetServiceName([Out, MarshalAs(UnmanagedType.LPStr)] out string pszName);

    [PreserveSig]
    int GetProcessedServiceName([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrName);

    [PreserveSig]
    int GetServiceNameEmphasized([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrName);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("C64935F4-29E4-4e22-911A-63F7F55CB097"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_NIT
  {
    [PreserveSig]
    int Initialize(
      [In] ISectionList pSectionList,
      [In] IMpeg2Data pMPEGData
      );

    [PreserveSig]
    int GetVersionNumber([Out] out byte pbVal);

    [PreserveSig]
    int GetNetworkId([Out] out short pwVal);

    [PreserveSig]
    int GetCountOfTableDescriptors([Out] out int pdwVal);

    [PreserveSig]
    int GetTableDescriptorByIndex(
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetTableDescriptorByTag(
      [In] byte bTag,
      [In] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetCountOfRecords([Out] out int pdwVal);

    [PreserveSig]
    int GetRecordTransportStreamId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordOriginalNetworkId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordCountOfDescriptors(
      [In] int dwRecordIndex,
      [Out] out int pdwVal
      );

    [PreserveSig]
    int GetRecordDescriptorByIndex(
      [In] int dwRecordIndex,
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetRecordDescriptorByTag(
      [In] int dwRecordIndex,
      [In] byte bTag,
      [In, Out] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int RegisterForNextTable([In] IntPtr hNextTableAvailable);

    [PreserveSig]
    int GetNextTable([Out] out IDVB_NIT ppNIT);

    [PreserveSig]
    int RegisterForWhenCurrent([In] IntPtr hNextTableIsCurrent);

    [PreserveSig]
    int ConvertNextToCurrent();

    [PreserveSig]
    int GetVersionHash([Out] out int pdwVersionHash);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("ECE9BB0C-43B6-4558-A0EC-1812C34CD6CA"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_BAT
  {
    [PreserveSig]
    int Initialize(
      [In] ISectionList pSectionList,
      [In] IMpeg2Data pMPEGData
      );

    [PreserveSig]
    int GetVersionNumber([Out] out byte pbVal);

    [PreserveSig]
    int GetBouquetId([Out] out short pwVal);

    [PreserveSig]
    int GetCountOfTableDescriptors([Out] out int pdwVal);

    [PreserveSig]
    int GetTableDescriptorByIndex(
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetTableDescriptorByTag(
      [In] byte bTag,
      [In] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetCountOfRecords([Out] out int pdwVal);

    [PreserveSig]
    int GetRecordTransportStreamId(
      [In] int dwRecordIndex,
      [Out] out short pwVal);

    [PreserveSig]
    int GetRecordOriginalNetworkId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordCountOfDescriptors(
      [In] int dwRecordIndex,
      [Out] out int pdwVal
      );

    [PreserveSig]
    int GetRecordDescriptorByIndex(
      [In] int dwRecordIndex,
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetRecordDescriptorByTag(
      [In] int dwRecordIndex,
      [In] byte bTag,
      [In, Out] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int RegisterForNextTable([In] IntPtr hNextTableAvailable);

    [PreserveSig]
    int GetNextTable([Out] out IDVB_BAT ppBAT);

    [PreserveSig]
    int RegisterForWhenCurrent([In] IntPtr hNextTableIsCurrent);

    [PreserveSig]
    int ConvertNextToCurrent();
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("ED7E1B91-D12E-420c-B41D-A49D84FE1823"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbTerrestrialDeliverySystemDescriptor
  {
    [PreserveSig]
    int GetTag([Out] out byte pbVal);

    [PreserveSig]
    int GetLength([Out] out byte pbVal);

    [PreserveSig]
    int GetCentreFrequency([Out] out int pdwVal);

    [PreserveSig]
    int GetBandwidth([Out] out byte pbVal);

    [PreserveSig]
    int GetConstellation([Out] out byte pbVal);

    [PreserveSig]
    int GetHierarchyInformation([Out] out byte pbVal);

    [PreserveSig]
    int GetCodeRateHPStream([Out] out byte pbVal);

    [PreserveSig]
    int GetCodeRateLPStream([Out] out byte pbVal);

    [PreserveSig]
    int GetGuardInterval([Out] out byte pbVal);

    [PreserveSig]
    int GetTransmissionMode([Out] out byte pbVal);

    [PreserveSig]
    int GetOtherFrequencyFlag([Out] out byte pbVal);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("442DB029-02CB-4495-8B92-1C13375BCE99"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_EIT
  {
    [PreserveSig]
    int Initialize(
      [In] ISectionList pSectionList,
      [In] IMpeg2Data pMPEGData
      );

    [PreserveSig]
    int GetVersionNumber([Out] out byte pbVal);

    [PreserveSig]
    int GetServiceId([Out] out short pwVal);

    [PreserveSig]
    int GetTransportStreamId([Out] out short pwVal);

    [PreserveSig]
    int GetOriginalNetworkId([Out] out short pwVal);

    [PreserveSig]
    int GetSegmentLastSectionNumber([Out] out byte pbVal);

    [PreserveSig]
    int GetLastTableId([Out] out byte pbVal);

    [PreserveSig]
    int GetCountOfRecords([Out] out int pdwVal);

    [PreserveSig]
    int GetRecordEventId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordStartTime(
      [In] int dwRecordIndex,
      [Out] out MpegDateAndTime pmdtVal
      );

    [PreserveSig]
    int GetRecordDuration(
      [In] int dwRecordIndex,
      [Out] out MpegDuration pmdVal
      );

    [PreserveSig]
    int GetRecordRunningStatus(
      [In] int dwRecordIndex,
      [Out] out RunningStatus pbVal
      );

    [PreserveSig]
    int GetRecordFreeCAMode(
      [In] int dwRecordIndex,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pfVal
      );

    [PreserveSig]
    int GetRecordCountOfDescriptors(
      [In] int dwRecordIndex,
      [Out] out int pdwVal
      );

    [PreserveSig]
    int GetRecordDescriptorByIndex(
      [In] int dwRecordIndex,
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetRecordDescriptorByTag(
      [In] int dwRecordIndex,
      [In] byte bTag,
      [In, Out] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int RegisterForNextTable([In] IntPtr hNextTableAvailable);

    [PreserveSig]
    int GetNextTable([Out] out IDVB_EIT ppEIT);

    [PreserveSig]
    int RegisterForWhenCurrent([In] IntPtr hNextTableIsCurrent);

    [PreserveSig]
    int ConvertNextToCurrent();

    [PreserveSig]
    int GetVersionHash([Out] out int pdwVersionHash);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("02F2225A-805B-4ec5-A9A6-F9B5913CD470"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbSatelliteDeliverySystemDescriptor
  {
    [PreserveSig]
    int GetTag([Out] out byte pbVal);

    [PreserveSig]
    int GetLength([Out] out byte pbVal);

    [PreserveSig]
    int GetFrequency([Out] out int pdwVal);

    [PreserveSig]
    int GetOrbitalPosition([Out] out short pwVal);

    [PreserveSig]
    int GetWestEastFlag([Out] out byte pbVal);

    [PreserveSig]
    int GetPolarization([Out] out byte pbVal);

    [PreserveSig]
    int GetModulation([Out] out byte pbVal);

    [PreserveSig]
    int GetSymbolRate([Out] out int pdwVal);

    [PreserveSig]
    int GetFECInner([Out] out byte pbVal);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("02CAD8D3-FE43-48e2-90BD-450ED9A8A5FD"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_SDT
  {
    [PreserveSig]
    int Initialize(
      [In] ISectionList pSectionList,
      [In] IMpeg2Data pMPEGData
      );

    [PreserveSig]
    int GetVersionNumber([Out] out byte pbVal);

    [PreserveSig]
    int GetTransportStreamId([Out] out short pwVal);

    [PreserveSig]
    int GetOriginalNetworkId([Out] out short pwVal);

    [PreserveSig]
    int GetCountOfRecords([Out] out int pdwVal);

    [PreserveSig]
    int GetRecordServiceId(
      [In] int dwRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordEITScheduleFlag(
      [In] int dwRecordIndex,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pfVal
      );

    [PreserveSig]
    int GetRecordEITPresentFollowingFlag(
      [In] int dwRecordIndex,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pfVal
      );

    [PreserveSig]
    int GetRecordRunningStatus(
      [In] int dwRecordIndex,
      [Out] out RunningStatus pbVal
      );

    [PreserveSig]
    int GetRecordFreeCAMode(
      [In] int dwRecordIndex,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pfVal
      );

    [PreserveSig]
    int GetRecordCountOfDescriptors(
      [In] int dwRecordIndex,
      [Out] out int pdwVal
      );

    [PreserveSig]
    int GetRecordDescriptorByIndex(
      [In] int dwRecordIndex,
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetRecordDescriptorByTag(
      [In] int dwRecordIndex,
      [In] byte bTag,
      [In, Out] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int RegisterForNextTable([In] IntPtr hNextTableAvailable);

    [PreserveSig]
    int GetNextTable([Out] out IDVB_SDT ppSDT);

    [PreserveSig]
    int RegisterForWhenCurrent([In] IntPtr hNextTableIsCurrent);

    [PreserveSig]
    int ConvertNextToCurrent();

    [PreserveSig]
    int GetVersionHash([Out] out int pdwVersionHash);
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("CF1EDAFF-3FFD-4cf7-8201-35756ACBF85F"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbLogicalChannelDescriptor
  {
    [PreserveSig]
    int GetTag([Out] out byte pbVal);

    [PreserveSig]
    int GetLength([Out] out byte pbVal);

    [PreserveSig]
    int GetCountOfRecords([Out] out byte pbVal);

    [PreserveSig]
    int GetRecordServiceId(
      [In] byte bRecordIndex,
      [Out] out short pwVal
      );

    [PreserveSig]
    int GetRecordLogicalChannelNumber(
      [In] byte bRecordIndex,
      [Out] out short pwVal
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("83295D6A-FABA-4ee1-9B15-8067696910AE"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVB_TOT
  {
    [PreserveSig]
    int Initialize([In] ISectionList pSectionList);

    [PreserveSig]
    int GetUTCTime([Out] out MpegDateAndTime pmdtVal);

    [PreserveSig]
    int GetCountOfTableDescriptors([Out] out int pdwVal);

    [PreserveSig]
    int GetTableDescriptorByIndex(
      [In] int dwIndex,
      [Out] out IGenericDescriptor ppDescriptor
      );

    [PreserveSig]
    int GetTableDescriptorByTag(
      [In] byte bTag,
      [In, Out] DsInt pdwCookie,
      [Out] out IGenericDescriptor ppDescriptor
      );
  }

  #endregion
}