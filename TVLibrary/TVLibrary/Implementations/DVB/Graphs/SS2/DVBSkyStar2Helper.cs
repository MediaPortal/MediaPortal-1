#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Runtime.InteropServices;
using System.Collections;
using DirectShowLib;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// All things related to the skyStar2 specific things go in here
  /// </summary>
  public class DVBSkyStar2Helper
  {
    #region technisat guids
    public static Guid IID_IB2C2AVCTRL2 = new Guid(0x9c0563ce, 0x2ef7, 0x4568, 0xa2, 0x97, 0x88, 0xc7, 0xbb, 0x82, 0x40, 0x75);
    public static Guid CLSID_B2C2Adapter = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x0, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);
    public static Guid CLSID_StreamBufferSink = new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
    public static Guid CLSID_Mpeg2VideoStreamAnalyzer = new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
    public static Guid CLSID_StreamBufferConfig = new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);
    public static Guid CLSID_Mpeg2Data = new Guid(0xC666E115, 0xBB62, 0x4027, 0xA1, 0x13, 0x82, 0xD6, 0x43, 0xFE, 0x2D, 0x99);
    public static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
    public static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid(0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
    #endregion
    // interfaces
    #region AVControl
    [ComVisible(true), ComImport,
      Guid("9C0563CE-2EF7-4568-A297-88C7BB824075"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2AVCtrl
    {
      // Argument 1: Audio PID
      // Argument 2: Video PID

      [PreserveSig]
      int SetAudioVideoPIDs(
        int pida,
        int pidb
        );
    };
    #endregion
    
    #region AVControl2
    // setup interfaces

    [ComVisible(true), ComImport,
      Guid("295950B0-696D-4a04-9EE3-C031A0BFBEDE"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2AVCtrl2 : IB2C2MPEG2AVCtrl
    {
      [PreserveSig]
      int SetCallbackForVideoMode(
        [MarshalAs(UnmanagedType.FunctionPtr)] Delegate vInfo
        );

      [PreserveSig]
      int DeleteAudioVideoPIDs(
        int pida,
        int pidv
        );
      [PreserveSig]
      int GetAudioVideoState(
        [Out] out int a,
        [Out] out int b,
        [Out] out int c,
        [Out] out int d,
        [Out] out int e,
        [Out] out int f
        );
    };
    #endregion

    #region DataControl
    [ComVisible(true), ComImport,
      Guid("7F35C560-08B9-11d5-A469-00D0D7B2C2D7"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2DataCtrl
    {


      // Transport Stream methods
      [PreserveSig]
      int GetMaxPIDCount(
        [Out] out int pidCount
        );

      //this function is obselete, please use IB2C2MPEG2DataCtrl2's AddPIDsToPin function
      [PreserveSig]
      int AddPIDs(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray)] int[] pidArray
        );

      //this function is obselete, please use IB2C2MPEG2DataCtrl2's DeletePIDsFromPin function
      [PreserveSig]
      int DeletePIDs(
        int count,
        [In] ref int[] pidArray
        );

      // IP methods
      [PreserveSig]
      int GetMaxIpPIDCount(
        [Out] out int maxIpPidCount
        );

      [PreserveSig]
      int AddIpPIDs(
        int count,
        [In] ref int[] ipPids
        );

      [PreserveSig]
      int DeleteIpPIDs(
        int count,
        [In] ref int[] ipPids
        );

      [PreserveSig]
      int GetIpPIDs(
        [Out] out int count,
        [Out] out int[] ipPids
        );

      // All protocols

      [PreserveSig]
      int PurgeGlobalPIDs();

      [PreserveSig]
      int GetMaxGlobalPIDCount();

      [PreserveSig]
      int GetGlobalPIDs(
        [Out] out int count,
        [Out] out int[] globalPids
        );


      [PreserveSig]
      int ResetDataReceptionStats();

      [PreserveSig]
      int GetDataReceptionStats(
        [Out] out int ipQuality,
        [Out] out int tsQuality
        );

    };
    #endregion // do NOT use data control interface !!!
    
    #region DataControl2
    [ComVisible(true), ComImport,
      Guid("B0666B7C-8C7D-4c20-BB9B-4A7FE0F313A8"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2DataCtrl2 : IB2C2MPEG2DataCtrl
    {
      [PreserveSig]
      int AddPIDsToPin(
        ref int count,
        [In, MarshalAs(UnmanagedType.LPArray)] int[] pidsArray,
        int dataPin
        );

      [PreserveSig]
      int DeletePIDsFromPin(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 39)] int[] pidsArray,
        int dataPin
        );
    };
    #endregion// do NOT use data control interface !!!
    
    #region DataControl3
    [ComVisible(true), ComImport,
      Guid("E2857B5B-84E7-48b7-B842-4EF5E175F315"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2DataCtrl3 : IB2C2MPEG2DataCtrl2
    {
      [PreserveSig]
      int AddTsPIDs(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pids
        );
      [PreserveSig]
      int DeleteTsPIDs(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pids
        );

      [PreserveSig]
      int GetTsState(
        ref Int32 plOpen,
        ref Int32 plRunning,
        ref Int32 plCount,
        ref Int32[] plPIDArray
        );

      [PreserveSig]
      int GetIpState(
        [Out] out int plOpen,
        [Out] out int plRunning,
        [Out] out int plCount,
        [Out] out int[] plPIDArray
        );

      [PreserveSig]
      int GetReceivedDataIp(
        IntPtr ptrA, IntPtr ptrB
        );

      [PreserveSig]
      int AddMulticastMacAddress(
        IntPtr pMacAddrList
        );

      [PreserveSig]
      int GetMulticastMacAddressList(
        IntPtr pMacAddrList
        );

      [PreserveSig]
      int DeleteMulticastMacAddress(
        IntPtr pMacAddrList
        );

      [PreserveSig]
      int SetUnicastMacAddress(
        IntPtr pMacAddr
        );

      [PreserveSig]
      int GetUnicastMacAddress(
        IntPtr pMacAddr
        );

      [PreserveSig]
      int RestoreUnicastMacAddress();
    };
    #endregion// do NOT use data control interface !!!

    #region TunerControl
    //
    // tuner follows
    //
    [ComVisible(true), ComImport,
      Guid("D875D4A9-0749-4fe8-ADB9-CC13F9B3DD45"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl
    {
      // Satellite, Cable, Terrestrial (ATSC and DVB)

      [PreserveSig]
      int SetFrequency(
        int frequency
        );

      // Satellite, Cable

      [PreserveSig]
      int SetSymbolRate(
        int symbolRate
        );

      // Satellite only

      [PreserveSig]
      int SetLnbFrequency(
        int lnbFrequency
        );

      [PreserveSig]
      int SetFec(
        int fec
        );

      [PreserveSig]
      int SetPolarity(
        int polarity
        );

      [PreserveSig]
      int SetLnbKHz(
        int lnbKHZ
        );

      [PreserveSig]
      int SetDiseqc(
        int diseqc
        );

      // Cable only

      [PreserveSig]
      int SetModulation(
        int modulation
        );

      // All tuners

      [PreserveSig]
      int Initialize();

      [PreserveSig]
      int SetTunerStatus();

      [PreserveSig]
      int CheckLock();

      [PreserveSig]
      int GetTunerCapabilities(
        IntPtr tunerCaps,
        ref int count
        );

      // Terrestrial (ATSC)

      [PreserveSig]
      int GetFrequency(
        [Out] out int freq
        );

      [PreserveSig]
      int GetSymbolRate(
        [Out] out int symbRate
        );

      [PreserveSig]
      int GetModulation(
        [Out] out int modulation
        );

      [PreserveSig]
      int GetSignalStrength(
        [Out] out int signalStrength
        );

      [PreserveSig]
      int GetSignalLevel(
        [Out] out float signalLevel
        );

      [PreserveSig]
      int GetSNR(
        [Out] out float SNR
        );

      [PreserveSig]
      int GetPreErrorCorrectionBER(
        [Out] out float ber,
        bool flag
        );

      [PreserveSig]
      int GetUncorrectedBlocks(
        [Out] out int uncorrectedBlocks
        );

      [PreserveSig]
      int GetTotalBlocks(
        [Out] out int correctedBlocks
        );

      [PreserveSig]
      int GetChannel(
        [Out] out int channel
        );

      [PreserveSig]
      int SetChannel(
        int channel
        );
    };
    #endregion

    #region TunerControl2
    [ComVisible(true), ComImport,
      Guid("CD900832-50DF-4f8f-882D-1C358F90B3F2"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl2 : IB2C2MPEG2TunerCtrl
    {
      int SetTunerStatusEx(
        int count
        );

      int SetFrequencyKHz(
        long freqKHZ
        );

      // Terrestrial DVB only

      int SetGuardInterval(
        int interval
        );

      int GetGuardInterval(
        [Out] out int interval
        );

      int GetFec(
        [Out] out int plFec
        );

      int GetPolarity(

        [Out] out int plPolarity
        );

      int GetDiseqc(

        [Out] out int plDiseqc
        );

      int GetLnbKHz(
        [Out] out int plLnbKHz
        );

      int GetLnbFrequency(
        [Out] out int plFrequencyMHz
        );

      int GetCorrectedBlocks(
        [Out] out int plCorrectedBlocks
        );

      int GetSignalQuality(
        [Out] out int pdwSignalQuality
        );
    };
    #endregion

    #region TunerControl3
    [ComVisible(true), ComImport,
     Guid("4B39EB78-D3CD-4223-B682-46AE66968118"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl3 : IB2C2MPEG2TunerCtrl2
    {
      int SetBandwidth(
        int bandwidth
        );

      int GetBandwidth(
        [Out] out int bandwidth
        );
    };
    #endregion

    #region TunerControl4
    [ComVisible(true), ComImport,
     Guid("61A9051F-04C4-435e-8742-9EDD2C543CE9"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl4 : IB2C2MPEG2TunerCtrl3
    {
      int SendDiSEqCCommand(
        int length, IntPtr disEqcCommand
        );
    };
    #endregion
  }// class
}//namespace
