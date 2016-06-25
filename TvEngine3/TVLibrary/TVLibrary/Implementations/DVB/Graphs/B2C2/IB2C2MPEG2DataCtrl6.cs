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

namespace TvLibrary.Implementations.DVB
{

  /// <summary>
  /// A channel typically contains multiple program streams of packeted information identified by PID (Program Identifier). An IP data stream is a stream carrying IP packets. A Raw TS stream is any stream (regardless of content) arbitrarily selected by its PID. After a channel is locked onto by a tuner, the desired program is selected by setting its PID. 
  /// IB2C2MPEG2DataCtrl3 supports all tuner types. IB2C2MPEG2DataCtrl3 methods enable an application to configure PID parameters for receiving IP data and Raw Transport Stream (Raw TS) data. Additionally, IB2C2MPEG2DataCtrl6 methods can set the broadband device's unicast MAC address and multicast groups and monitor IP data reception statistics.
  /// </summary>
  [ComVisible(true), ComImport,
   Guid("a12a4531-72d2-40fc-b17d-8f9b0004444f"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IB2C2MPEG2DataCtrl6
  {
    /// <summary>
    /// Retrieves count of maximum simultaneous raw transport stream PIDs allowed. This value is currently 39
    /// </summary>
    /// <param name="pidCount">Pointer to long variable created by the caller. Variables will hold the value for maximum simultaneous PIDs</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetMaxPIDCount(
      out int pidCount
    );

    /// <summary>
    /// Sets raw transport stream PID values of interest to the application
    /// The AddPIDs function is obsolete and is implemented for backwards compatibility only. Current version uses AddPIDsToPin or AddTsPIDs method
    /// </summary>
    /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount</param>
    /// <param name="pidArray">Array of length lCount, where each element is set to a PID of interest</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int AddPIDs(
      int count,
      [MarshalAs(UnmanagedType.LPArray)] int[] pidArray
      );

    /// <summary>
    /// Deletes raw transport stream PID values that the application no longer wants processed
    /// The DeletePIDs function is obsolete and is implemented for backwards compatibility only. Current version uses DeletePIDsFromPin or DeleteTsPIDs method
    /// </summary>
    /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount</param>
    /// <param name="pidArray">Array of length lCount, where each element is set to a PID that no longer needs to be processed</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int DeletePIDs(
      int count,
      [MarshalAs(UnmanagedType.LPArray)] int[] pidArray
      );

    /// <summary>
    /// Retrieves count of maximum simultaneous IP PIDs allowed. This value is currently 33
    /// </summary>
    /// <param name="maxIpPidCount">Pointer to long variable created by the caller. Variable will hold the value for maximum simultaneous IP PIDs</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetMaxIpPIDCount(
      out int maxIpPidCount
      );

    /// <summary>
    /// Sets IP PID values of interest to the application
    /// </summary>
    /// <param name="count">Long variable that passes the size of the array specified in the second argument. Must not be larger than value returned by GetMaxIpPIDCount</param>
    /// <param name="ipPids">Pointer to an array created by the caller. In the array each element specifies a PID to be added. Array must not be smaller than the value passed by the first argument.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int AddIpPIDs(
      int count,
      int[] ipPids
      );

    /// <summary>
    /// Deletes IP PID values that the application no longer wants processed
    /// </summary>
    /// <param name="count">Long variable used to pass the size of the array specified in the second argument. Must not be larger than value returned by GetMaxIpPIDCount</param>
    /// <param name="ipPids">Pointer to an array created by the caller. In the array each element specifies a PID to be deleted</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int DeleteIpPIDs(
      int count,
      int[] ipPids
      );

    /// <summary>
    /// Gets a list of all IP PIDs that have been added that have not been subsequently deleted or purged
    /// </summary>
    /// <param name="count">Pointer to a long variable created by the caller. Variable will hold the size of the array specified in the second argument</param>
    /// <param name="ipPids">Pointer to an array created by the caller. In the array each element will specify a currently active PID. Must not be smaller than the value returned by GetMaxIpPIDCount</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetIpPIDs(
      out int count,
      int[] ipPids
      );

    /// <summary>
    /// Deletes all IP PIDs currently active in the tuner. The current value is 33
    /// </summary>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int PurgeGlobalPIDs();

    /// <summary>
    /// Shows the maximum number of available PIDs
    /// </summary>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetMaxGlobalPIDCount();

    /// <summary>
    /// Retrieves list of currently set PIDs
    /// </summary>
    /// <param name="count">Pointer to long variable created by caller. Variable will hold count of how many PIDs are used in this array</param>
    /// <param name="globalPids">Pointer to array created by the caller. In the array each element specifies a PID. Array must not be smaller than the value returned by GetMaxGlobalPIDCount</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetGlobalPIDs(
      out int count,
      int[] globalPids
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
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetDataReceptionStats(
      out int ipQuality,
      out int tsQuality
      );

    /// <summary>
    /// Sets raw transport stream PID values of interest to the application and associates these PIDs with B2C2MPEG2Filter output pins
    /// </summary>
    /// <param name="count">Pointer to a variable created by the caller. (In) Variable holds the number of PIDs to be added given by the second argument.(Out) Variable holds the number of PIDs added</param>
    /// <param name="pidsArray">Pointer to array created by the caller. Array holds the PIDs to be added. Must not be smaller than the value passed by the first argument</param>
    /// <param name="dataPin">The number of the B2C2MPEG2Filter output pin to which you want to add the PIDs of the second argument</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int AddPIDsToPin(
      ref int count,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pidsArray,
      int dataPin
      );

    /// <summary>
    /// Deletes raw transport stream PID values that the application no longer wants processed and dissociates the PIDs from their B2C2MPEG2Filter output pins
    /// </summary>
    /// <param name="count">variable showing number of PIDs to be deleted. PIDs are defined in the second argument</param>
    /// <param name="pidsArray">Pointer to array created by the caller. Array holds the PIDs to be deleted. Array must not be smaller than the number given by the first argument</param>
    /// <param name="dataPin">The number of the B2C2MPEG2Filter output pin from which you want to delete the PIDs of the second argument</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int DeletePIDsFromPin(
      int count,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pidsArray,
      int dataPin
      );

    /// <summary>
    /// Sets raw transport stream PID values of interest to the application
    /// </summary>
    /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount</param>
    /// <param name="pids">Pointer to an array created by the caller. Array holds each PID of interest. Must not be smaller than the value passed by the first argument</param>
    /// <returns></returns>
    [PreserveSig]
    int AddTsPIDs(
      int count,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pids
      );

    /// <summary>
    /// Deletes raw transport stream PID values that the application no longer wants processed
    /// </summary>
    /// <param name="count">Number of PID(s) being passed in using the array (2nd argument). Must not be larger than value returned by GetMaxPIDCount.</param>
    /// <param name="pids">Pointer to an array created by the caller. Array holds each PID that no longer needs to be processed. Size must not be smaller than value passed by the first argument</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int DeleteTsPIDs(
      int count,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] pids
      );

    /// <summary>
    /// Returns current transport stream settings in terms of raw transport stream PIDs open or running, where "open" means PID set, and "running" means started, not currently stopped
    /// </summary>
    /// <param name="plOpen">Pointer to long variable created by the caller. Variable will hold the count of currently open TS streams. A value of 0 indicates that no TS streams are open. Pass NULL for this argument if no return is desired</param>
    /// <param name="plRunning">Pointer to long variable created by the caller. Variable will hold the count of currently running TS streams. A value of 0 indicates that no TS streams are running. Pass NULL for this argument if no return is desired.</param>
    /// <param name="plCount">Pointer to a long variable created by the caller. (Input) variable holds the size of the PID array. (Output) variable will hold the number of PID(s) being used in the second argument. Pass NULL for this argument and plPIDArray if no return is desired</param>
    /// <param name="plPidArray">Pointer to an array created by the caller. In the array each element will be set to a PID of interest. Must not be smaller than the value passed by the third argument. Use GetMaxPIDCount to determine the maximum possible number of PIDs. Pass NULL for this argument and plCount if no return is desired</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetTsState(
      out int plOpen,
      out int plRunning,
      ref int plCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 39)] int[] plPidArray
      );

    /// <summary>
    /// Returns current IP settings in terms of IP PID streams open or running, , where "open" means PID set, and "running" means started, not currently stopped
    /// </summary>
    /// <param name="plOpen">Pointer to a long variable created by the caller. Variable will hold the count of currently open IP streams. A value of 0 indicates that no IP streams are open. Pass NULL for this argument if no return is desired</param>
    /// <param name="plRunning">Pointer to a long variable created by the caller. Variable will hold the count of currently running IP streams. A value of 0 indicates that no IP streams are running. Pass NULL for this argument if no return is desired</param>
    /// <param name="plCount">Pointer to a long variable created by the caller. (Input) Variable holds the size of the PID array. (Output) Variable will hold the number of PID(s) being used in the fourth argument. Pass NULL for this argument and plPIDArray if no return is desired</param>
    /// <param name="plPidArray">Pointer to an array created by the caller. In the array each element will be set to a PID of interest. Must not be smaller than the value passed by the third argument. Use GetMaxIpPIDCount to determine the maximum possible number of PIDs. Pass NULL for this argument and plCount if no return is desired</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetIpState(
      out int plOpen,
      out int plRunning,
      out int plCount,
      out int[] plPidArray
      );

    /// <summary>
    /// Returns count of bytes and IP packets received
    /// </summary>
    /// <param name="ptrA">Pointer to 64-bit variable where the byte count for total IP data received will be stored</param>
    /// <param name="ptrB">Pointer to 64-bit variable where the packet count for total IP packets received will be stored</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetReceivedDataIp(
      IntPtr ptrA, IntPtr ptrB
      );

    /// <summary>
    /// Adds the given multicast MAC addresses
    /// </summary>
    /// <param name="pMacAddrList">Pointer to the structure tMacAddressList created by the caller. (tMacAddressList is defined in header file b2c2_defs.h delivered as part of the SDK.) The structure member lCount holds the number of MAC addresses to delete; the first 1Count members of the aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE] array will be added. The maximum number of MAC addresses held by the aabtMacAddr array is defined at B2C2_SDK_MAC_ADDR_LIST_MAX; therefore, lCount must not be larger than B2C2_SDK_MAC_ADDR_LIST_MAX. The number of bytes per MAC address in the array is defined at B2C2_SDK_MAC_ADDR_SIZE</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int AddMulticastMacAddress(
      IntPtr pMacAddrList
      );

    /// <summary>
    /// Gets the list of currently set MAC addresses
    /// </summary>
    /// <param name="pMacAddrList">Pointer to the structure tMacAddressList created by the caller. (tMacAddressList is defined in header file b2c2_defs.h delivered as part of the SDK.) The first 1Count members of the aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE] array will be listed. In this case, the member 1Count holds the maximum possible number of MAC addresses defined at B2C2_SDK_MAC_ADDR_LIST_MAX and returns the number of MAC addresses set at the aabtMacAddr array. The number of bytes per MAC address in the array is defined at B2C2_SDK_MAC_ADDR_SIZE, which specifies the size of an array member</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetMulticastMacAddressList(
      IntPtr pMacAddrList
      );

    /// <summary>
    /// Deletes the given multicast MAC addresses
    /// </summary>
    /// <param name="pMacAddrList">Pointer to the structure tMacAddressList created by the caller. (tMacAddressList is defined in header file b2c2_defs.h delivered as part of the SDK.) The member lCount holds the number of MAC addresses to delete; the first 1Count members of the aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE] array will be deleted. The maximum number of MAC addresses held by the aabtMacAddr array is defined at B2C2_SDK_MAC_ADDR_LIST_MAX; therefore, lCount must not be larger than B2C2_SDK_MAC_ADDR_LIST_MAX. The number of bytes per MAC address in the array is defined at B2C2_SDK_MAC_ADDR_SIZE, which specifies the size of an array member.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int DeleteMulticastMacAddress(
      IntPtr pMacAddrList
      );

    /// <summary>
    /// Set the given unicast MAC addresses
    /// </summary>
    /// <param name="pMacAddr">Pointer to the array of size B2C2_SDK_MAC_ADDR_SIZE (6 bytes) created by the caller. pMacAddr holds the new unicast MAC address to be set for the device; the currently set unicast MAC address will be overwritten</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int SetUnicastMacAddress(
      IntPtr pMacAddr
      );

    /// <summary>
    /// Set the given unicast MAC addresses
    /// </summary>
    /// <param name="pMacAddr">Pointer to the array of size B2C2_SDK_MAC_ADDR_SIZE (6 bytes) created by the caller. pMacAddr will hold the current unicast MAC address set at the device</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetUnicastMacAddress(
      IntPtr pMacAddr
      );

    /// <summary>
    /// Restores the unicast MAC address to the device default
    /// </summary>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int RestoreUnicastMacAddress();

    /// <summary>
    /// Gets the Hardware MAC Address of broadband device. 
    /// </summary>
    /// <param name="pHwMacAddr">Pointer to an array of six bytes created by the caller. Array contains the hardware MAC Address for the broadband device.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetHardwareMacAddress(
      IntPtr pHwMacAddr
      );

    /// <summary>
    /// SetTableId
    /// </summary>
    /// <param name="lTableId">lTableId</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int SetTableId(
      int lTableId
      );

    /// <summary>
    /// GetTableId
    /// </summary>
    /// <param name="plTableId">plTableId</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetTableId(
      out int plTableId
      );

    /// <summary>
    /// GetKeyCount
    /// </summary>
    /// <param name="plTotal">plTotal</param>
    /// <param name="plPidTscKeys">plPidTscKeys</param>
    /// <param name="plPidKeys">plPidKeys</param>
    /// <param name="plGlobalKey">plGlobalKey</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetKeyCount(
      out int plTotal,
      out int plPidTscKeys,
      out int plPidKeys,
      out int plGlobalKey
      );

    /// <summary>
    /// GetKeysInUse
    /// </summary>
    /// <param name="plCount">plCount</param>
    /// <param name="plTypeArray">plTypeArray</param>
    /// <param name="plPidArray">plPidArray</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetKeysInUse(
          ref int plCount,
          out int plTypeArray,
          out int plPidArray
          );

    /// <summary>
    /// AddKey
    /// </summary>
    /// <param name="lType">lType</param>
    /// <param name="lPid">lPid</param>
    /// <param name="pKey">pKey</param>
    /// <param name="lKeyLength">lKeyLength</param>
    /// <returns></returns>
    [PreserveSig]
    int AddKey(
          int lType,
          int lPid,
          IntPtr pKey,
          int lKeyLength
          );

    /// <summary>
    /// DeleteKey
    /// </summary>
    /// <param name="lType">lType</param>
    /// <param name="lPid">lPid</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int DeleteKey(
      int lType,
      int lPid
      );

    /// <summary>
    /// PurgeKeys
    /// </summary>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int PurgeKeys();

    /// <summary>
    /// Sets a callback function for receipt of Transport Stream packets, which allows an application to receive the raw transport stream without the need to use a Direct Show filter pin
    /// </summary>
    /// <param name="pvCallBack">Pointer to a callback function with the format: UINT __stdcall (WORD wPID, unsigned char* pucTransportPacket). </param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    int SetCallbackForTransportStream(
      [MarshalAs(UnmanagedType.FunctionPtr)] Delegate pvCallBack
      );

    /// <summary>
    /// Retrieves a list of the installed devices with detailed information about each device to allow user selection of a specific device.
    /// </summary>
    /// <param name="pListOfDevices">Pointer to an array of device structures as defined in the header b2c2_defs.h which will receive information about all installed devices. The caller provides the structure.</param>
    /// <param name="lSize">Pointer to a long variable created by the caller. Variable will hold the size of the array of structure (bytes) on entry.</param>
    /// <param name="pdwDeviceCount">Pointer to a unsigned long variable created by the caller. Variable must hold the maximum number of devices supported by the application on entry and will hold the actual number of devices on return.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    int GetDeviceList(
      IntPtr pListOfDevices,
      ref int lSize,
      ref uint pdwDeviceCount
     );

    /// <summary>
    /// Selects a specific device based on its Device ID.
    /// </summary>
    /// <param name="dwDeviceId">An unsigned long variable that contains the unique Device Identifier for the specific device to open.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    int SelectDevice(
      uint dwDeviceId
      );
  } ;


}

