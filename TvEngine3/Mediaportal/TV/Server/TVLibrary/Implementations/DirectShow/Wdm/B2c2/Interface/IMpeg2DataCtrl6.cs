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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface
{
  /// <summary>
  /// Called by the data interface to pass raw transport stream packet data to the application.
  /// </summary>
  /// <remarks>
  /// The data must be copied out of the buffer and return control to the interface as quickly as possible.
  /// </remarks>
  /// <param name="pid">The PID for the stream that the packet is associated with.</param>
  /// <param name="data">The packet.</param>
  /// <returns>???</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate uint OnTsData(ushort pid, IntPtr data);

  [Guid("a12a4531-72d2-40fc-b17d-8f9b0004444f"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMpeg2DataCtrl6
  {
    #region IMpeg2DataCtrl

    #region transport stream PIDs

    /// <summary>
    /// Get the maximum number of transport stream class PIDs that may be registered at any given
    /// time.
    /// </summary>
    /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
    /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
    [PreserveSig]
    int GetMaxPIDCount(out int maxPidCount);

    /// <summary>
    /// Obsolete. Use AddPIDsToPin() or AddTsPIDs() instead.
    /// </summary>
    /// <param name="pidCount">The number of PIDs to register.</param>
    /// <param name="pids">The PIDs to register.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
    [PreserveSig, Obsolete("Use AddPIDsToPin() or AddTsPIDs() instead.")]
    int AddPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids);

    /// <summary>
    /// Obsolete. Use DeletePIDsFromPin() or DeleteTsPIDs() instead.
    /// </summary>
    /// <param name="pidCount">The number of PIDs to deregister.</param>
    /// <param name="pids">The PIDs to deregister.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
    [PreserveSig, Obsolete("Use DeletePIDsFromPin() or DeleteTsPIDs() instead.")]
    int DeletePIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids);

    #endregion

    #region IP PIDs

    /// <summary>
    /// Get the maximum number of IP class PIDs that may be registered at any given time.
    /// </summary>
    /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
    /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
    [PreserveSig]
    int GetMaxIpPIDCount(out int maxPidCount);

    /// <summary>
    /// Register IP class PID(s) that are of interest to the application. Packets marked with these
    /// PIDs will be passed to the system network stack.
    /// </summary>
    /// <param name="pidCount">The number of PIDs to register.</param>
    /// <param name="pids">The PIDs to register.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
    [PreserveSig]
    int AddIpPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids);

    /// <summary>
    /// Deregister IP class PID(s) that are no longer of interest to the application. Packets
    /// marked with these PIDs will no longer be passed to the system network stack.
    /// </summary>
    /// <remarks>
    /// If a PID has been registered more than once, this function only removes one registration.
    /// </remarks>
    /// <param name="pidCount">The number of PIDs to deregister.</param>
    /// <param name="pids">The PIDs to deregister.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
    [PreserveSig]
    int DeleteIpPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids);

    /// <summary>
    /// Get the list of IP class PIDs that are currently registered with the interface.
    /// </summary>
    /// <param name="pidCount">As an input, the size of the PID array; as an output, the number of
    ///   PIDs registered.</param>
    /// <param name="pids">The registered PIDs.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully retrieved</returns>
    [PreserveSig]
    int GetIpPIDs(ref int pidCount, [MarshalAs(UnmanagedType.LPArray)] ref int[] pids);

    #endregion

    #region all PIDs

    /// <summary>
    /// Deregister all IP class PIDs currently registered with the interface.
    /// </summary>
    /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
    [PreserveSig]
    int PurgeGlobalPIDs();

    /// <summary>
    /// Get the maximum number of IP class PIDs of any class that may be registered at any given
    /// time.
    /// </summary>
    /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
    /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
    [PreserveSig]
    int GetMaxGlobalPIDCount(out int maxPidCount);

    /// <summary>
    /// Get the list of IP class PIDs that are currently registered with the interface.
    /// </summary>
    /// <param name="pidCount">As an input, the size of the PID array; as an output, the number of
    ///   PIDs registered.</param>
    /// <param name="pids">The registered PIDs.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully retrieved</returns>
    [PreserveSig]
    int GetGlobalPIDs(ref int pidCount, [MarshalAs(UnmanagedType.LPArray)] ref int[] pids);

    #endregion

    /// <summary>
    /// Reset the values of the statistics retrieved by GetDataReceptionStats().
    /// </summary>
    /// <returns>an HRESULT indicating whether the statistics were successfully reset</returns>
    [PreserveSig]
    int ResetDataReceptionStats();

    /// <summary>
    /// Get the current values of statistics that can be used for monitoring signal quality. The
    /// statistics are measured since the last call to this function or to
    /// ResetDataReceptionStats().
    /// </summary>
    /// <param name="ipRatio">The ratio of correctly received IP class packets to total IP packets.</param>
    /// <param name="tsRatio">The ratio of correctly received TS class packets to total TS packets.</param>
    /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
    [PreserveSig]
    int GetDataReceptionStats(out int ipRatio, out int tsRatio);

    #endregion

    #region IMpeg2DataCtrl2

    /// <summary>
    /// Register transport stream class PID(s) that are of interest to the application. Packets marked
    /// with these PIDs will be passed on the corrresponding B2C2 filter data output pin.
    /// </summary>
    /// <param name="pidCount">As an input, the number of PIDs to attempt to register; as an output, the
    ///   number of PIDs that were successfully registered.</param>
    /// <param name="pids">The PIDs to register.</param>
    /// <param name="pinIndex">The index (zero-based) of the data output pin to register with.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
    [PreserveSig]
    int AddPIDsToPin(ref int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids, int pinIndex);

    /// <summary>
    /// Deregister transport stream class PID(s) that are no longer of interest to the application.
    /// Packets marked with these PIDs will no longer be passed on the corrresponding B2C2 filter data
    /// output pin.
    /// </summary>
    /// <remarks>
    /// If a PID has been registered more than once, this function only removes one registration.
    /// </remarks>
    /// <param name="pidCount">The number of PIDs to deregister.</param>
    /// <param name="pids">The PIDs to deregister.</param>
    /// <param name="pinIndex">The index (zero-based) of the data output pin to deregister with.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
    [PreserveSig]
    int DeletePIDsFromPin(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids, int pinIndex);

    #endregion

    #region IMpeg2DataCtrl3

    #region transport stream PIDs

    /// <summary>
    /// Register transport stream class PID(s) that are of interest to the application. Packets
    /// marked with these PIDs will be passed to the application via call back.
    /// </summary>
    /// <param name="pidCount">The number of PIDs to register.</param>
    /// <param name="pids">The PIDs to register.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
    [PreserveSig]
    int AddTsPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids);

    /// <summary>
    /// Deregister transport stream class PID(s) when they are no longer of interest to the
    /// application. Packets marked with these PIDs will no longer be passed to the application via
    /// call back.
    /// </summary>
    /// <remarks>
    /// If a PID has been registered more than once, this function only removes one registration.
    /// </remarks>
    /// <param name="pidCount">The number of PIDs to deregister.</param>
    /// <param name="pids">The PIDs to deregister.</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
    [PreserveSig]
    int DeleteTsPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray)] int[] pids);

    /// <summary>
    /// Get the details of transport stream class PIDs that are registered with the interface.
    /// </summary>
    /// <param name="openPidCount">The number of registered PIDs.</param>
    /// <param name="runningPidCount">The number of PIDs that are currently running.</param>
    /// <param name="pidCount">As an input, the number of PIDs to retrieve; as an output, the
    ///   number of PIDs actually retrieved.</param>
    /// <param name="pidList">The list of registered PIDs.</param>
    /// <returns>an HRESULT indicating whether the state information was successfully retrieved</returns>
    [PreserveSig]
    int GetTsState(out int openPidCount, out int runningPidCount, ref int pidCount, [MarshalAs(UnmanagedType.LPArray)] ref int[] pidList);

    #endregion

    #region IP PIDs

    /// <summary>
    /// Get the details of IP class PIDs that are registered with the interface.
    /// </summary>
    /// <param name="openPidCount">The number of registered PIDs.</param>
    /// <param name="runningPidCount">The number of PIDs that are currently running.</param>
    /// <param name="pidCount">As an input, the number of PIDs to retrieve; as an output, the number of
    ///   PIDs actually retrieved.</param>
    /// <param name="pidList">The list of registered PIDs.</param>
    /// <returns>an HRESULT indicating whether the state information was successfully retrieved</returns>
    [PreserveSig]
    int GetIpState(out int openPidCount, out int runningPidCount, ref int pidCount, [MarshalAs(UnmanagedType.LPArray)] ref int[] pidList);

    /// <summary>
    /// Get the number of IP class PID bytes and packets that have been received.
    /// </summary>
    /// <param name="byteCount">The number of bytes received.</param>
    /// <param name="packetCount">The number of packets received.</param>
    /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
    [PreserveSig]
    int GetReceivedDataIp(out long byteCount, out long packetCount);

    #endregion

    #region multicast

    /// <summary>
    /// Register the given multicast MAC addresses with the interface.
    /// </summary>
    /// <remarks>
    /// The maximum number of addresses that may be registered can be retrieved from MAX_MAC_ADDRESS_COUNT.
    /// </remarks>
    /// <param name="addressList">The list of addresses to register.</param>
    /// <returns>an HRESULT indicating whether the MAC addresses were successfully registered</returns>
    [PreserveSig]
    int AddMulticastMacAddress([In] ref MacAddressList addressList);

    /// <summary>
    /// Get the list of multicast MAC addresses that are registered with the interface.
    /// </summary>
    /// <param name="addressList">The list of addresses.</param>
    /// <returns>an HRESULT indicating whether the address list was successfully retrieved</returns>
    [PreserveSig]
    int GetMulticastMacAddressList(out MacAddressList addressList);

    /// <summary>
    /// Deregister the given multicast MAC addresses from the interface.
    /// </summary>
    /// <remarks>
    /// The maximum number of addresses that may be deregistered is set at MAX_MAC_ADDRESS_COUNT.
    /// </remarks>
    /// <param name="addressList">The list of addresses to deregister.</param>
    /// <returns>an HRESULT indicating whether the MAC addresses were successfully deregistered</returns>
    [PreserveSig]
    int DeleteMulticastMacAddress([In] ref MacAddressList addressList);

    #endregion

    #region unicast

    /// <summary>
    /// Set the device's unicast MAC address.
    /// </summary>
    /// <param name="address">The address to set.</param>
    /// <returns>an HRESULT indicating whether the address was successfully set</returns>
    [PreserveSig]
    int SetUnicastMacAddress([In] ref MacAddress address);

    /// <summary>
    /// Get the device's current unicast MAC address.
    /// </summary>
    /// <param name="address">The current address.</param>
    /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
    [PreserveSig]
    int GetUnicastMacAddress(out MacAddress address);

    /// <summary>
    /// Restore the unicast MAC address to the default address for the device.
    /// </summary>
    /// <returns>an HRESULT indicating whether the address was successfully restored</returns>
    [PreserveSig]
    int RestoreUnicastMacAddress();

    #endregion

    #endregion

    #region IMpeg2DataCtrl4

    /// <summary>
    /// Get the device's MAC address.
    /// </summary>
    /// <param name="macAddress">The MAC address.</param>
    /// <returns>an HRESULT indicating whether the MAC address was successfully retrieved</returns>
    [PreserveSig]
    int GetHardwareMacAddress(out MacAddress macAddress);

    [PreserveSig]
    int SetTableId(int tableId);

    [PreserveSig]
    int GetTableId(out int tableId);

    #region decrypt keys

    /// <summary>
    /// Get counts of each of the types of keys that are registered with the interface.
    /// </summary>
    /// <param name="totalKeyCount">The total number of registered keys.</param>
    /// <param name="pidTscKeyCount">The number of PID-specific keys registered.</param>
    /// <param name="pidKeyCount">The number of fallback PID keys registered.</param>
    /// <param name="globalKeyCount">The number of global keys registered.</param>
    /// <returns>an HRESULT indicating whether the key counts were successfully retrieved</returns>
    [PreserveSig]
    int GetKeyCount(out int totalKeyCount, out int pidTscKeyCount, out int pidKeyCount, out int globalKeyCount);

    /// <summary>
    /// Get the details for the keys that are registered with and being used by the interface.
    /// </summary>
    /// <param name="keyCount">As an input, the size of the key and PID arrays; as an output, the number of keys in use.</param>
    /// <param name="keys">The list of keys.</param>
    /// <param name="pids">A list of the PID associated with each key.</param>
    /// <returns>an HRESULT indicating whether the key details were successfully retrieved</returns>
    [PreserveSig]
    int GetKeysInUse(ref int keyCount, [MarshalAs(UnmanagedType.LPArray)] ref long[] keys, [MarshalAs(UnmanagedType.LPArray)] ref int[] pids);

    /// <summary>
    /// Register a decryption key with the interface.
    /// </summary>
    /// <param name="keyType">The key type.</param>
    /// <param name="pid">The PID to use the key to decrypt.</param>
    /// <param name="key">The key.</param>
    /// <param name="keyLength">The length of the key.</param>
    /// <returns>an HRESULT indicating whether the key was successfully registered</returns>
    [PreserveSig]
    int AddKey(KeyType keyType, uint pid, [MarshalAs(UnmanagedType.LPArray)] byte[] key, int keyLength);

    /// <summary>
    /// Deregister a decryption key.
    /// </summary>
    /// <param name="keyType">The key type.</param>
    /// <param name="pid">The PID that the key is associated with.</param>
    /// <returns>an HRESULT indicating whether the key was successfully deregistered</returns>
    [PreserveSig]
    int DeleteKey(KeyType keyType, uint pid);

    /// <summary>
    /// Deregister all decryption keys.
    /// </summary>
    /// <returns>an HRESULT indicating whether the keys were successfully deregistered</returns>
    [PreserveSig]
    int PurgeKeys();

    #endregion

    #endregion

    #region IMpeg2DataCtrl5

    /// <summary>
    /// Register a call back delegate that the interface can use to pass raw transport stream packets
    /// directly to the application. The packets passed correspond with the transport stream class PIDs
    /// registered with the interface.
    /// </summary>
    /// <param name="callBack">A pointer to the call back delegate.</param>
    /// <returns>an HRESULT indicating whether the call back delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallbackForTransportStream(OnTsData callBack);

    #endregion

    #region IMpeg2DataCtrl6

    /// <summary>
    /// Get information about the B2C2 compatible devices installed in the system.
    /// </summary>
    /// <param name="deviceInfo">A pointer to an array of device information.</param>
    /// <param name="infoSize">As an input, the size (in bytes) of the device information array;
    ///   as an output, the number of bytes of the device information which were filled.</param>
    /// <param name="deviceCount">As an input, the size of the device information array; as an
    ///   output, the number of devices installed in the system.</param>
    /// <returns>an HRESULT indicating whether the device information was successfully retrieved</returns>
    [PreserveSig]
    int GetDeviceList(IntPtr deviceInfo, ref int infoSize, ref int deviceCount);

    /// <summary>
    /// Select (activate) a specific B2C2 device.
    /// </summary>
    /// <param name="deviceId">The identifier of the device to select.</param>
    /// <returns>an HRESULT indicating whether the device was successfully selected</returns>
    [PreserveSig]
    int SelectDevice(uint deviceId);

    #endregion
  }
}