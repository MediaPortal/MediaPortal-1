#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// All things related to the skyStar2 specific things go in here
  /// </summary>
  public class DVBSkyStar2Helper
  {
    #region technisat guids
    /// <summary>
    /// AVCTRL2 GUID
    /// </summary>
    public static Guid IID_IB2C2AVCTRL2 = new Guid(0x9c0563ce, 0x2ef7, 0x4568, 0xa2, 0x97, 0x88, 0xc7, 0xbb, 0x82, 0x40, 0x75);
    /// <summary>
    /// B2C2 Adapter GUID
    /// </summary>
    public static Guid CLSID_B2C2Adapter = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x0, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);
    /// <summary>
    /// Stream Buffer Sink GUID
    /// </summary>
    public static Guid CLSID_StreamBufferSink = new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
    /// <summary>
    /// Mpeg2VideoStreamAnalyzer GUID
    /// </summary>
    public static Guid CLSID_Mpeg2VideoStreamAnalyzer = new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
    /// <summary>
    /// StreamBufferConfig GUID
    /// </summary>
    public static Guid CLSID_StreamBufferConfig = new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);
    /// <summary>
    /// Mpeg2Data GUID
    /// </summary>
    public static Guid CLSID_Mpeg2Data = new Guid(0xC666E115, 0xBB62, 0x4027, 0xA1, 0x13, 0x82, 0xD6, 0x43, 0xFE, 0x2D, 0x99);
    /// <summary>
    /// Mediatype MPEG2 Sections GUID
    /// </summary>
    public static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
    /// <summary>
    /// Mediasubtype MPEG2 Data GUID
    /// </summary>
    public static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid(0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
    #endregion

    #region AVControl
    /// <summary>
    /// IB2C2MPEG2AVCtrl methods allow access to MPEG2 Audio and Video elementary streams by setting or deleting their PIDs. For Video in Windows, a Video callback structure can be configured to pass Video window size, aspect ratio, and frame rate when instructed by the application.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("9C0563CE-2EF7-4568-A297-88C7BB824075"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2AVCtrl
    {
      /// <summary>
      /// Sets the Audio and Video PIDs of interest to the application.
      /// </summary>
      /// <param name="pida">Audio PID</param>
      /// <param name="pidb">Video PID</param>
      /// <returns></returns>
      [PreserveSig]
      int SetAudioVideoPIDs(
        int pida,
        int pidb
        );
    };
    #endregion

    #region AVControl2
    // setup interfaces

    /// <summary>
    /// IB2C2MPEG2AVCtrl2 methods allow access to MPEG2 Audio and Video elementary streams by setting or deleting their PIDs. For Video in Windows, a Video callback structure can be configured to pass Video window size, aspect ratio, and frame rate when instructed by the application.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("295950B0-696D-4a04-9EE3-C031A0BFBEDE"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2AVCtrl2 : IB2C2MPEG2AVCtrl
    {
      /// <summary>
      /// Sets Callback for Video mode of operation, which allows Video aspect ratio to be reported back to the user application when the user application passes a parameter.
      /// </summary>
      /// <param name="vInfo">Pointer to a callback function with the format: UINT __stdcall (MPEG2_VIDEO_INFO *). </param>
      /// <returns></returns>
      [PreserveSig]
      int SetCallbackForVideoMode(
        [MarshalAs(UnmanagedType.FunctionPtr)] Delegate vInfo
        );
      /// <summary>
      /// Deletes Audio and Video PIDs. 
      /// </summary>
      /// <param name="pida">Audio PID</param>
      /// <param name="pidv">Video PID</param>
      /// <returns></returns>
      [PreserveSig]
      int DeleteAudioVideoPIDs(
        int pida,
        int pidv
        );
      /// <summary>
      /// Returns current Audio and Video settings in terms of which streams are open or running and how many.
      /// </summary>
      /// <param name="a">Pointer to long variable created by the caller. Variable will hold the count of currently open Audio streams. A value of 0 indicates that no Audio stream is open. Pass NULL for this argument if no return is desired.</param>
      /// <param name="b">Pointer to long variable created by the caller. Variable will hold the count of currently open Video streams. A value of 0 indicates that no Video stream is open. Pass NULL for this argument if no return is desired.</param>
      /// <param name="c">Pointer to long variable created by the caller. Variable will hold the count of currently running Audio streams. A value of 0 indicates that no Audio stream is running. Pass NULL for this argument if no return is desired.</param>
      /// <param name="d">Pointer to long variable created by the caller. Variable will hold the count of currently running Video streams. A value of 0 indicates that no Video stream is running. Pass NULL for this argument if no return is desired.</param>
      /// <param name="e">Pointer to long variable created by the caller. Variable will hold the value of the current Audio PID. A value of 0 indicates that no Audio PID is set. Pass NULL for this argument if no return is desired. </param>
      /// <param name="f">Pointer to long variable created by the caller. Variable will hold the value of the current Video PID. A value of 0 indicates that no Video PID is set. Pass NULL for this argument if no return is desired.</param>
      /// <returns></returns>
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
    /// <summary>
    /// A channel typically contains multiple program streams of packeted information identified by PID (Program Identifier). An IP data stream is a stream carrying IP packets. A Raw TS stream is any stream (regardless of content) arbitrarily selected by its PID. After a channel is locked onto by a tuner, the desired program is selected by setting its PID. 
    /// IB2C2MPEG2DataCtrl supports all tuner types. IB2C2MPEG2DataCtrl methods enable an application to configure PID parameters for receiving IP data and Raw Transport Stream (Raw TS) data. Additionally, IB2C2MPEG2DataCtrl6 methods can set the broadband device's unicast MAC address and multicast groups and monitor IP data reception statistics.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("7F35C560-08B9-11d5-A469-00D0D7B2C2D7"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2DataCtrl
    {

      /// <summary>
      /// Retrieves count of maximum simultaneous raw transport stream PIDs allowed. This value is currently 39
      /// </summary>
      /// <param name="pidCount">Pointer to long variable created by the caller. Variables will hold the value for maximum simultaneous PIDs</param>
      /// <returns></returns>
      [PreserveSig]
      int GetMaxPIDCount(
        [Out] out int pidCount
        );

      /// <summary>
      /// Sets raw transport stream PID values of interest to the application
      /// The AddPIDs function is obsolete and is implemented for backwards compatibility only. Current version uses AddPIDsToPin or AddTsPIDs method
      /// </summary>
      /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount</param>
      /// <param name="pidArray">Array of length lCount, where each element is set to a PID of interest</param>
      /// <returns></returns>
      [PreserveSig]
      int AddPIDs(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray)] int[] pidArray
        );

      /// <summary>
      /// Deletes raw transport stream PID values that the application no longer wants processed
      /// The DeletePIDs function is obsolete and is implemented for backwards compatibility only. Current version uses DeletePIDsFromPin or DeleteTsPIDs method
      /// </summary>
      /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount</param>
      /// <param name="pidArray">Array of length lCount, where each element is set to a PID that no longer needs to be processed</param>
      /// <returns></returns>
      [PreserveSig]
      int DeletePIDs(
        int count,
        [In] ref int[] pidArray
        );

      /// <summary>
      /// Retrieves count of maximum simultaneous IP PIDs allowed. This value is currently 33
      /// </summary>
      /// <param name="maxIpPidCount">Pointer to long variable created by the caller. Variable will hold the value for maximum simultaneous IP PIDs</param>
      /// <returns></returns>
      [PreserveSig]
      int GetMaxIpPIDCount(
        [Out] out int maxIpPidCount
        );

      /// <summary>
      /// Sets IP PID values of interest to the application
      /// </summary>
      /// <param name="count">Long variable that passes the size of the array specified in the second argument. Must not be larger than value returned by GetMaxIpPIDCount</param>
      /// <param name="ipPids">Pointer to an array created by the caller. In the array each element specifies a PID to be added. Array must not be smaller than the value passed by the first argument.</param>
      /// <returns></returns>
      [PreserveSig]
      int AddIpPIDs(
        int count,
        [In] ref int[] ipPids
        );

      /// <summary>
      /// Deletes IP PID values that the application no longer wants processed
      /// </summary>
      /// <param name="count">Long variable used to pass the size of the array specified in the second argument. Must not be larger than value returned by GetMaxIpPIDCount</param>
      /// <param name="ipPids">Pointer to an array created by the caller. In the array each element specifies a PID to be deleted</param>
      /// <returns></returns>
      [PreserveSig]
      int DeleteIpPIDs(
        int count,
        [In] ref int[] ipPids
        );

      /// <summary>
      /// Gets a list of all IP PIDs that have been added that have not been subsequently deleted or purged
      /// </summary>
      /// <param name="count">Pointer to a long variable created by the caller. Variable will hold the size of the array specified in the second argument</param>
      /// <param name="ipPids">Pointer to an array created by the caller. In the array each element will specify a currently active PID. Must not be smaller than the value returned by GetMaxIpPIDCount</param>
      /// <returns></returns>
      [PreserveSig]
      int GetIpPIDs(
        [Out] out int count,
        [Out] out int[] ipPids
        );

      /// <summary>
      /// Deletes all IP PIDs currently active in the tuner. The current value is 33
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int PurgeGlobalPIDs();

      /// <summary>
      /// Shows the maximum number of available PIDs
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int GetMaxGlobalPIDCount();

      /// <summary>
      /// Retrieves list of currently set PIDs
      /// </summary>
      /// <param name="count">Pointer to long variable created by caller. Variable will hold count of how many PIDs are used in this array</param>
      /// <param name="globalPids">Pointer to array created by the caller. In the array each element specifies a PID. Array must not be smaller than the value returned by GetMaxGlobalPIDCount</param>
      /// <returns></returns>
      [PreserveSig]
      int GetGlobalPIDs(
        [Out] out int count,
        [Out] out int[] globalPids
        );

      /// <summary>
      /// Defines start time for GetDataReceptionStats calculation
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int ResetDataReceptionStats();

      /// <summary>
      /// Retrieves Data Reception Statistics
      /// </summary>
      /// <param name="ipQuality">Pointer to a long variable created by the caller. Variable will hold the ratio of correctly recovered IP packets to total IP packets measured since last call to this function or to ResetDataReceptionStats</param>
      /// <param name="tsQuality">Pointer to a long variable created by the caller. Variable will hold the ratio of correctly recovered TS (Transport Stream) packets to total TS packets measured since last call to this function or to ResetDataReceptionStats</param>
      /// <returns></returns>
      [PreserveSig]
      int GetDataReceptionStats(
        [Out] out int ipQuality,
        [Out] out int tsQuality
        );

    };
    #endregion // do NOT use data control interface !!!

    #region DataControl2
    /// <summary>
    /// A channel typically contains multiple program streams of packeted information identified by PID (Program Identifier). An IP data stream is a stream carrying IP packets. A Raw TS stream is any stream (regardless of content) arbitrarily selected by its PID. After a channel is locked onto by a tuner, the desired program is selected by setting its PID. 
    /// IB2C2MPEG2DataCtrl2 supports all tuner types. IB2C2MPEG2DataCtrl2 methods enable an application to configure PID parameters for receiving IP data and Raw Transport Stream (Raw TS) data. Additionally, IB2C2MPEG2DataCtrl6 methods can set the broadband device's unicast MAC address and multicast groups and monitor IP data reception statistics.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("B0666B7C-8C7D-4c20-BB9B-4A7FE0F313A8"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2DataCtrl2 : IB2C2MPEG2DataCtrl
    {
      /// <summary>
      /// Sets raw transport stream PID values of interest to the application and associates these PIDs with B2C2MPEG2Filter output pins
      /// </summary>
      /// <param name="count">Pointer to a variable created by the caller. (In) Variable holds the number of PIDs to be added given by the second argument.(Out) Variable holds the number of PIDs added</param>
      /// <param name="pidsArray">Pointer to array created by the caller. Array holds the PIDs to be added. Must not be smaller than the value passed by the first argument</param>
      /// <param name="dataPin">The number of the B2C2MPEG2Filter output pin to which you want to add the PIDs of the second argument</param>
      /// <returns></returns>
      [PreserveSig]
      int AddPIDsToPin(
        ref int count,
        [In, MarshalAs(UnmanagedType.LPArray)] int[] pidsArray,
        int dataPin
        );

      /// <summary>
      /// Deletes raw transport stream PID values that the application no longer wants processed and dissociates the PIDs from their B2C2MPEG2Filter output pins
      /// </summary>
      /// <param name="count">variable showing number of PIDs to be deleted. PIDs are defined in the second argument</param>
      /// <param name="pidsArray">Pointer to array created by the caller. Array holds the PIDs to be deleted. Array must not be smaller than the number given by the first argument</param>
      /// <param name="dataPin">The number of the B2C2MPEG2Filter output pin from which you want to delete the PIDs of the second argument</param>
      /// <returns></returns>
      [PreserveSig]
      int DeletePIDsFromPin(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 39)] int[] pidsArray,
        int dataPin
        );
    };
    #endregion// do NOT use data control interface !!!

    #region DataControl3
    /// <summary>
    /// A channel typically contains multiple program streams of packeted information identified by PID (Program Identifier). An IP data stream is a stream carrying IP packets. A Raw TS stream is any stream (regardless of content) arbitrarily selected by its PID. After a channel is locked onto by a tuner, the desired program is selected by setting its PID. 
    /// IB2C2MPEG2DataCtrl3 supports all tuner types. IB2C2MPEG2DataCtrl3 methods enable an application to configure PID parameters for receiving IP data and Raw Transport Stream (Raw TS) data. Additionally, IB2C2MPEG2DataCtrl6 methods can set the broadband device's unicast MAC address and multicast groups and monitor IP data reception statistics.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("E2857B5B-84E7-48b7-B842-4EF5E175F315"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2DataCtrl3 : IB2C2MPEG2DataCtrl2
    {
      /// <summary>
      /// Sets raw transport stream PID values of interest to the application
      /// </summary>
      /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount</param>
      /// <param name="pids">Pointer to an array created by the caller. Array holds each PID of interest. Must not be smaller than the value passed by the first argument</param>
      /// <returns></returns>
      [PreserveSig]
      int AddTsPIDs(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pids
        );

      /// <summary>
      /// Deletes raw transport stream PID values that the application no longer wants processed
      /// </summary>
      /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount.</param>
      /// <param name="pids">Pointer to an array created by the caller. Array holds each PID that no longer needs to be processed. Size must not be smaller than value passed by the first argument</param>
      /// <returns></returns>
      [PreserveSig]
      int DeleteTsPIDs(
        int count,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pids
        );

      /// <summary>
      /// Returns current transport stream settings in terms of raw transport stream PIDs open or running, where "open" means PID set, and "running" means started, not currently stopped
      /// </summary>
      /// <param name="plOpen">Pointer to long variable created by the caller. Variable will hold the count of currently open TS streams. A value of 0 indicates that no TS streams are open. Pass NULL for this argument if no return is desired</param>
      /// <param name="plRunning">Pointer to long variable created by the caller. Variable will hold the count of currently running TS streams. A value of 0 indicates that no TS streams are running. Pass NULL for this argument if no return is desired.</param>
      /// <param name="plCount">Pointer to a long variable created by the caller. (Input) variable holds the size of the PID array. (Output) variable will hold the number of PID(s) being used in the second argument. Pass NULL for this argument and plPIDArray if no return is desired</param>
      /// <param name="plPIDArray">Pointer to an array created by the caller. In the array each element will be set to a PID of interest. Must not be smaller than the value passed by the third argument. Use GetMaxPIDCount to determine the maximum possible number of PIDs. Pass NULL for this argument and plCount if no return is desired</param>
      /// <returns></returns>
      [PreserveSig]
      int GetTsState(
        ref Int32 plOpen,
        ref Int32 plRunning,
        ref Int32 plCount,
        ref Int32[] plPIDArray
        );

      /// <summary>
      /// Returns current IP settings in terms of IP PID streams open or running, , where "open" means PID set, and "running" means started, not currently stopped
      /// </summary>
      /// <param name="plOpen">Pointer to a long variable created by the caller. Variable will hold the count of currently open IP streams. A value of 0 indicates that no IP streams are open. Pass NULL for this argument if no return is desired</param>
      /// <param name="plRunning">Pointer to a long variable created by the caller. Variable will hold the count of currently running IP streams. A value of 0 indicates that no IP streams are running. Pass NULL for this argument if no return is desired</param>
      /// <param name="plCount">Pointer to a long variable created by the caller. (Input) Variable holds the size of the PID array. (Output) Variable will hold the number of PID(s) being used in the fourth argument. Pass NULL for this argument and plPIDArray if no return is desired</param>
      /// <param name="plPIDArray">Pointer to an array created by the caller. In the array each element will be set to a PID of interest. Must not be smaller than the value passed by the third argument. Use GetMaxIpPIDCount to determine the maximum possible number of PIDs. Pass NULL for this argument and plCount if no return is desired</param>
      /// <returns></returns>
      [PreserveSig]
      int GetIpState(
        [Out] out int plOpen,
        [Out] out int plRunning,
        [Out] out int plCount,
        [Out] out int[] plPIDArray
        );

      /// <summary>
      /// Returns count of bytes and IP packets received
      /// </summary>
      /// <param name="ptrA">Pointer to 64-bit variable where the byte count for total IP data received will be stored</param>
      /// <param name="ptrB">Pointer to 64-bit variable where the packet count for total IP packets received will be stored</param>
      /// <returns></returns>
      [PreserveSig]
      int GetReceivedDataIp(
        IntPtr ptrA, IntPtr ptrB
        );

      /// <summary>
      /// Adds the given multicast MAC addresses
      /// </summary>
      /// <param name="pMacAddrList">Pointer to the structure tMacAddressList created by the caller. (tMacAddressList is defined in header file b2c2_defs.h delivered as part of the SDK.) The structure member lCount holds the number of MAC addresses to delete; the first 1Count members of the aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE] array will be added. The maximum number of MAC addresses held by the aabtMacAddr array is defined at B2C2_SDK_MAC_ADDR_LIST_MAX; therefore, lCount must not be larger than B2C2_SDK_MAC_ADDR_LIST_MAX. The number of bytes per MAC address in the array is defined at B2C2_SDK_MAC_ADDR_SIZE</param>
      /// <returns></returns>
      [PreserveSig]
      int AddMulticastMacAddress(
        IntPtr pMacAddrList
        );

      /// <summary>
      /// Gets the list of currently set MAC addresses
      /// </summary>
      /// <param name="pMacAddrList">Pointer to the structure tMacAddressList created by the caller. (tMacAddressList is defined in header file b2c2_defs.h delivered as part of the SDK.) The first 1Count members of the aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE] array will be listed. In this case, the member 1Count holds the maximum possible number of MAC addresses defined at B2C2_SDK_MAC_ADDR_LIST_MAX and returns the number of MAC addresses set at the aabtMacAddr array. The number of bytes per MAC address in the array is defined at B2C2_SDK_MAC_ADDR_SIZE, which specifies the size of an array member</param>
      /// <returns></returns>
      [PreserveSig]
      int GetMulticastMacAddressList(
        IntPtr pMacAddrList
        );

      /// <summary>
      /// Deletes the given multicast MAC addresses
      /// </summary>
      /// <param name="pMacAddrList">Pointer to the structure tMacAddressList created by the caller. (tMacAddressList is defined in header file b2c2_defs.h delivered as part of the SDK.) The member lCount holds the number of MAC addresses to delete; the first 1Count members of the aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE] array will be deleted. The maximum number of MAC addresses held by the aabtMacAddr array is defined at B2C2_SDK_MAC_ADDR_LIST_MAX; therefore, lCount must not be larger than B2C2_SDK_MAC_ADDR_LIST_MAX. The number of bytes per MAC address in the array is defined at B2C2_SDK_MAC_ADDR_SIZE, which specifies the size of an array member.</param>
      /// <returns></returns>
      [PreserveSig]
      int DeleteMulticastMacAddress(
        IntPtr pMacAddrList
        );

      /// <summary>
      /// Set the given unicast MAC addresses
      /// </summary>
      /// <param name="pMacAddr">Pointer to the array of size B2C2_SDK_MAC_ADDR_SIZE (6 bytes) created by the caller. pMacAddr holds the new unicast MAC address to be set for the device; the currently set unicast MAC address will be overwritten</param>
      /// <returns></returns>
      [PreserveSig]
      int SetUnicastMacAddress(
        IntPtr pMacAddr
        );

      /// <summary>
      /// Set the given unicast MAC addresses
      /// </summary>
      /// <param name="pMacAddr">Pointer to the array of size B2C2_SDK_MAC_ADDR_SIZE (6 bytes) created by the caller. pMacAddr will hold the current unicast MAC address set at the device</param>
      /// <returns></returns>
      [PreserveSig]
      int GetUnicastMacAddress(
        IntPtr pMacAddr
        );

      /// <summary>
      /// Restores the unicast MAC address to the device default
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int RestoreUnicastMacAddress();
    };
    #endregion// do NOT use data control interface !!!

    #region TunerControl
    /// <summary>
    /// In order to receive programs, the broadband receiver must first lock onto a channel. This is accomplished by controlling the tuner. IB2C2MPEG2TunerCtrl3 supports satellite, cable, and terrestrial DVB and terrestrial ATSC tuners. IB2C2MPEG2TunerCtrl4 methods allow software to lock onto a channel, including the monitoring of receiver (tuner module) performance statistics such as BER and Uncorrected Blocks.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("D875D4A9-0749-4fe8-ADB9-CC13F9B3DD45"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl
    {
      /// <summary>
      /// Sets Transponder Frequency value in MHz
      /// </summary>
      /// <param name="frequency">Transponder Frequency in MHz. Must be greater than or equal to zero. The upper limit is tuner dependent</param>
      /// <returns></returns>
      [PreserveSig]
      int SetFrequency(
        int frequency
        );

      /// <summary>
      /// Sets Symbol Rate value
      /// </summary>
      /// <param name="symbolRate">Symbol Rate in KS/s. Must be greater than or equal to zero. The upper limit is tuner dependent and can be queried by GetTunerCapabilities</param>
      /// <returns></returns>
      [PreserveSig]
      int SetSymbolRate(
        int symbolRate
        );

      /// <summary>
      /// Sets LNB Frequency value
      /// </summary>
      /// <param name="lnbFrequency">LNB Frequency in MHz. Must be greater than or equal to zero and less than Transponder Frequency set by IB2C2MPEG2TunerCtrl2::SetFrequency or by IB2C2MPEG2TunerCtrl2::SetFrequencyKHz</param>
      /// <returns></returns>
      [PreserveSig]
      int SetLnbFrequency(
        int lnbFrequency
        );

      /// <summary>
      /// Sets FEC value
      /// </summary>
      /// <param name="fec">FEC value. Use eFEC enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// FEC_1_2
      /// FEC_2_3
      /// FEC_3_4 
      /// FEC_5_6 
      /// FEC_7_8 
      /// FEC_AUTO</param>
      /// <returns></returns>
      [PreserveSig]
      int SetFec(
        int fec
        );

      /// <summary>
      /// Sets Polarity value
      /// </summary>
      /// <param name="polarity">Polarity value. Use ePolarity enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// POLARITY_HORIZONTAL 
      /// POLARITY_VERTICAL</param>
      /// <returns></returns>
      [PreserveSig]
      int SetPolarity(
        int polarity
        );

      /// <summary>
      /// Sets LNB kHz selection value
      /// </summary>
      /// <param name="lnbKHZ">LNB kHz Selection value. Use eLNBSelection enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// LNB_SELECTION_0 
      /// LNB_SELECTION_22 
      /// LNB_SELECTION_33 
      /// LNB_SELECTION_44</param>
      /// <returns></returns>
      [PreserveSig]
      int SetLnbKHz(
        int lnbKHZ
        );

      /// <summary>
      /// Sets DiSEqC value
      /// </summary>
      /// <param name="diseqc">DiSEqC value. Use eDiseqc enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// DISEQC_NONE 
      /// DISEQC_SIMPLE_A 
      /// DISEQC_SIMPLE_B 
      /// DISEQC_LEVEL_1_A_A 
      /// DISEQC_LEVEL_1_B_A 
      /// DISEQC_LEVEL_1_A_B 
      /// DISEQC_LEVEL_1_B_B</param>
      /// <returns></returns>
      [PreserveSig]
      int SetDiseqc(
        int diseqc
        );

      /// <summary>
      /// Sets Modulation value
      /// </summary>
      /// <param name="modulation"></param>
      /// <returns></returns>
      [PreserveSig]
      int SetModulation(
        int modulation
        );

      /// <summary>
      /// Creates connection to Tuner Control interface of B2C2MPEG2Filter. (See sample applications for more information.)
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int Initialize();

      /// <summary>
      /// Sends tuner parameter values to the tuner and waits until the tuner gets in lock or times out. The time-out value depends on the tuner type
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int SetTunerStatus();

      /// <summary>
      /// Checks lock status of tuner
      /// </summary>
      /// <returns></returns>
      [PreserveSig]
      int CheckLock();

      /// <summary>
      /// Identifies capabilities of particular tuner
      /// </summary>
      /// <param name="tunerCaps">Pointer to a structure defined in the header file b2c2_defs.h. The caller provides the structure</param>
      /// <param name="count">Pointer to a long variable created by the caller. Variable will hold the size of the structure (bytes) returned</param>
      /// <returns></returns>
      [PreserveSig]
      int GetTunerCapabilities(
        IntPtr tunerCaps,
        ref int count
        );

      /// <summary>
      /// Gets current Transponder Frequency in MHz
      /// </summary>
      /// <param name="freq">Pointer to a long variable created by the caller. Variable will hold the Transponder Frequency in MHz</param>
      /// <returns></returns>
      [PreserveSig]
      int GetFrequency(
        [Out] out int freq
        );

      /// <summary>
      /// Gets current Symbol Rate in MS/s
      /// </summary>
      /// <param name="symbRate">Pointer to a variable created by the caller. Variable will hold the Symbol Rate in KS/s</param>
      /// <returns></returns>
      [PreserveSig]
      int GetSymbolRate(
        [Out] out int symbRate
        );

      /// <summary>
      /// Gets current Modulation value
      /// </summary>
      /// <param name="modulation">Pointer to a long variable created by the caller. Variable will hold the Modulation value. (Use eModulation enumerated type defined in header file b2c2_defs.h delivered as part of the SDK.) Possible values are: 
      /// QAM_4 
      /// QAM_16 
      /// QAM_32 
      /// QAM_64 
      /// QAM_128
      /// QAM_256</param>
      /// <returns></returns>
      [PreserveSig]
      int GetModulation(
        [Out] out int modulation
        );

      /// <summary>
      /// Gets current Signal Strength value in %
      /// </summary>
      /// <param name="signalStrength">Pointer to a variable created by the caller. Variable will hold Signal Strength in %</param>
      /// <returns></returns>
      [PreserveSig]
      int GetSignalStrength(
        [Out] out int signalStrength
        );

      /// <summary>
      /// The GetSignalLevel function is obsolete and is implemented for backwards compatibility only. Current version uses GetSignalStrength or GetSignalQuality method, depending on the tuner type
      /// </summary>
      /// <param name="signalLevel">Pointer to a variable created by the caller. Variable will hold Signal Level in dBm</param>
      /// <returns></returns>
      [PreserveSig]
      int GetSignalLevel(
        [Out] out float signalLevel
        );

      /// <summary>
      /// Gets current Signal to Noise Ratio (SNR) value
      /// </summary>
      /// <param name="SNR">Pointer to a variable created by the caller. Variable will hold Signal to Noise Ratio</param>
      /// <returns></returns>
      [PreserveSig]
      int GetSNR(
        [Out] out float SNR
        );

      /// <summary>
      /// Gets current pre-error-correction Bit Error Rate (BER) value
      /// </summary>
      /// <param name="ber">Pointer to a variable created by the caller. Variable will hold Bit Error Rate</param>
      /// <param name="flag">(Not used.)</param>
      /// <returns></returns>
      [PreserveSig]
      int GetPreErrorCorrectionBER(
        [Out] out float ber,
        bool flag
        );

      /// <summary>
      /// Gets current count of Uncorrected Blocks
      /// </summary>
      /// <param name="uncorrectedBlocks">Pointer to a variable created by the caller. Variable will hold count of Uncorrected Blocks</param>
      /// <returns></returns>
      [PreserveSig]
      int GetUncorrectedBlocks(
        [Out] out int uncorrectedBlocks
        );

      /// <summary>
      /// Gets current count of Total Blocks
      /// </summary>
      /// <param name="correctedBlocks">Pointer to a variable created by the caller. Variable will hold count of Total Blocks</param>
      /// <returns></returns>
      [PreserveSig]
      int GetTotalBlocks(
        [Out] out int correctedBlocks
        );

      /// <summary>
      /// Gets current Channel value
      /// </summary>
      /// <param name="channel">Pointer to a variable created by the caller. Variable will hold Channel number</param>
      /// <returns></returns>
      [PreserveSig]
      int GetChannel(
        [Out] out int channel
        );

      /// <summary>
      /// Sets Channel value
      /// </summary>
      /// <param name="channel">Channel number</param>
      /// <returns></returns>
      [PreserveSig]
      int SetChannel(
        int channel
        );
    };
    #endregion

    #region TunerControl2
    /// <summary>
    /// In order to receive programs, the broadband receiver must first lock onto a channel. This is accomplished by controlling the tuner. IB2C2MPEG2TunerCtrl3 supports satellite, cable, and terrestrial DVB and terrestrial ATSC tuners. IB2C2MPEG2TunerCtrl4 methods allow software to lock onto a channel, including the monitoring of receiver (tuner module) performance statistics such as BER and Uncorrected Blocks.
    /// </summary>
    [ComVisible(true), ComImport,
      Guid("CD900832-50DF-4f8f-882D-1C358F90B3F2"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl2 : IB2C2MPEG2TunerCtrl
    {
      /// <summary>
      /// Sends values to tuner for tuning with optional argument for defining how many times the SDK should check whether the tuner is in lock. The wait time between each check is 50 ms
      /// </summary>
      /// <param name="count">Number of times the SDK should check whether the tuner is in lock.</param>
      /// <returns></returns>
      int SetTunerStatusEx(
        int count
        );

      /// <summary>
      /// Sets Transponder Frequency value in kHz.
      /// </summary>
      /// <param name="freqKHZ">Transponder Frequency in kHz. Must be greater than or equal to zero. The upper limit is tuner dependent</param>
      /// <returns></returns>
      int SetFrequencyKHz(
        long freqKHZ
        );

      /// <summary>
      /// Sets Guard Interval value
      /// </summary>
      /// <param name="interval">Guard Interval value. Use eGuardInterval enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// GUARD_INTERVAL_1_32
      /// GUARD_INTERVAL_1_16
      /// GUARD_INTERVAL_1_8
      /// GUARD_INTERVAL_1_4
      /// GUARD_INTERVAL_AUTO </param>
      /// <returns></returns>
      int SetGuardInterval(
        int interval
        );

      /// <summary>
      /// Gets current Guard Interval value
      /// </summary>
      /// <param name="interval">Pointer to long variable where value for Guard Interval will be stored. eGuardInterval enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// GUARD_INTERVAL_1_32
      /// GUARD_INTERVAL_1_16
      /// GUARD_INTERVAL_1_8
      /// GUARD_INTERVAL_1_4
      /// GUARD_INTERVAL_AUTO </param>
      /// <returns></returns>
      int GetGuardInterval(
        [Out] out int interval
        );

      /// <summary>
      /// Gets current FEC value
      /// </summary>
      /// <param name="plFec">Pointer to a long variable created by the user where the FEC value will be stored. eFEC enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are:
      /// FEC_1_2
      /// FEC_2_3
      /// FEC_3_4 
      /// FEC_5_6 
      /// FEC_7_8 
      /// FEC_AUTO</param>
      /// <returns></returns>
      int GetFec(
        [Out] out int plFec
        );

      /// <summary>
      /// Gets current Polarity value
      /// </summary>
      /// <param name="plPolarity">Pointer to a long variable created by the user where the Polarity value will be stored. ePolarity enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are:
      /// POLARITY_HORIZONTAL 
      /// POLARITY_VERTICAL </param>
      /// <returns></returns>
      int GetPolarity(

        [Out] out int plPolarity
        );

      /// <summary>
      /// Gets current DiSEqC value
      /// </summary>
      /// <param name="plDiseqc">Pointer to a long variable created by the user where the DiSEqC value will be stored. eDiseqc enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
      /// DISEQC_NONE 
      /// DISEQC_SIMPLE_A 
      /// DISEQC_SIMPLE_B 
      /// DISEQC_LEVEL_1_A_A 
      /// DISEQC_LEVEL_1_B_A 
      /// DISEQC_LEVEL_1_A_B 
      /// DISEQC_LEVEL_1_B_B</param>
      /// <returns></returns>
      int GetDiseqc(

        [Out] out int plDiseqc
        );

      /// <summary>
      /// Gets current LNB kHz selection value
      /// </summary>
      /// <param name="plLnbKHz">Pointer to a long variable created by the user where the LNB kHz Selection value will be stored. eLNBSelection enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are:
      /// LNB_SELECTION_0 
      /// LNB_SELECTION_22 
      /// LNB_SELECTION_33 
      /// LNB_SELECTION_44</param>
      /// <returns></returns>
      int GetLnbKHz(
        [Out] out int plLnbKHz
        );

      /// <summary>
      /// Gets current LNB Frequency value
      /// </summary>
      /// <param name="plFrequencyMHz">Pointer to a long variable created by the user where the LNB Frequency value in MHz will be stored</param>
      /// <returns></returns>
      int GetLnbFrequency(
        [Out] out int plFrequencyMHz
        );

      /// <summary>
      /// Gets current count of Corrected Blocks
      /// </summary>
      /// <param name="plCorrectedBlocks">Pointer to a variable created by the caller. Variable will hold count of Corrected Blocks</param>
      /// <returns></returns>
      int GetCorrectedBlocks(
        [Out] out int plCorrectedBlocks
        );

      /// <summary>
      /// Gets current Signal Quality value in %
      /// </summary>
      /// <param name="pdwSignalQuality">Pointer to a variable created by the caller. Variable will hold Signal Quality in %</param>
      /// <returns></returns>
      int GetSignalQuality(
        [Out] out int pdwSignalQuality
        );
    };
    #endregion

    #region TunerControl3
    /// <summary>
    /// In order to receive programs, the broadband receiver must first lock onto a channel. This is accomplished by controlling the tuner. IB2C2MPEG2TunerCtrl3 supports satellite, cable, and terrestrial DVB and terrestrial ATSC tuners. IB2C2MPEG2TunerCtrl4 methods allow software to lock onto a channel, including the monitoring of receiver (tuner module) performance statistics such as BER and Uncorrected Blocks.
    /// </summary>
    [ComVisible(true), ComImport,
     Guid("4B39EB78-D3CD-4223-B682-46AE66968118"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl3 : IB2C2MPEG2TunerCtrl2
    {
      /// <summary>
      /// Sets the channel bandwidth.
      /// </summary>
      /// <param name="bandwidth">A long variable created by the user where the Bandwidth is stored. Possible values are:
      /// 6 MHZ
      /// 7 MHZ
      /// 8 MHZ</param>
      /// <returns></returns>
      int SetBandwidth(
        int bandwidth
        );

      /// <summary>
      /// Gets the channel bandwidth
      /// </summary>
      /// <param name="bandwidth">Pointer to a long variable created by the user where the Bandwidth value will be stored. Possible values are:
      /// 6 MHZ
      /// 7 MHZ
      /// 8 MHZ</param>
      /// <returns></returns>
      int GetBandwidth(
        [Out] out int bandwidth
        );
    };
    #endregion

    #region TunerControl4
    /// <summary>
    /// In order to receive programs, the broadband receiver must first lock onto a channel. This is accomplished by controlling the tuner. IB2C2MPEG2TunerCtrl3 supports satellite, cable, and terrestrial DVB and terrestrial ATSC tuners. IB2C2MPEG2TunerCtrl4 methods allow software to lock onto a channel, including the monitoring of receiver (tuner module) performance statistics such as BER and Uncorrected Blocks.
    /// </summary>
    [ComVisible(true), ComImport,
     Guid("61A9051F-04C4-435e-8742-9EDD2C543CE9"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IB2C2MPEG2TunerCtrl4 : IB2C2MPEG2TunerCtrl3
    {
      /// <summary>
      /// Sends a DiSEqC command to a DiSEqC compatible device connected to the card
      /// </summary>
      /// <param name="length">A integer variable that contains the number of bytes in the DiSEqC message</param>
      /// <param name="disEqcCommand">A pointer to a sequence of bytes which is the actual DiSEqC bytes to be sent according to the length specified</param>
      /// <returns></returns>
      int SendDiSEqCCommand(
        int length, IntPtr disEqcCommand
        );
    };
    #endregion
  }// class
}//namespace
