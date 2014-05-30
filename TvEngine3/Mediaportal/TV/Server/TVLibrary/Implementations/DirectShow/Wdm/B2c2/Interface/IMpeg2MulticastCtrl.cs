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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface
{
  [Guid("0b5a8a87-7133-4a37-846e-77f568a52155"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMpeg2MulticastCtrl
  {
    /// <summary>
    /// Enable the video/audio multicasting capability. Any streams that were previously active will be
    /// re-enabled.
    /// </summary>
    /// <returns>an HRESULT indicating whether the multicasting capability was successfully enabled</returns>
    [PreserveSig]
    int Enable();

    /// <summary>
    /// Disable the video/audio multicasting capability. All active streams will be disabled.
    /// </summary>
    /// <returns>an HRESULT indicating whether the multicasting capability was successfully disabled</returns>
    [PreserveSig]
    int Disable();

    /// <summary>
    /// Start multicasting the content from a specific set of PIDs on a specific network interface.
    /// </summary>
    /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1) to multicast on.</param>
    /// <param name="port">The network interface port to multicast from.</param>
    /// <param name="pidCount">The number of PIDs in the list of PIDs to multicast.</param>
    /// <param name="pidList">A list of PIDs to multicast.</param>
    /// <returns>an HRESULT indicating whether the multicast was successfully started</returns>
    [PreserveSig]
    int StartMulticast([MarshalAs(UnmanagedType.LPStr)] string address, ushort port, int pidCount, int[] pidList);

    /// <summary>
    /// Stop multicasting on a specific network interface.
    /// </summary>
    /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1) to stop multicasting on.</param>
    /// <param name="port">The network interface port to stop multicasting from.</param>
    /// <returns>an HRESULT indicating whether the multicast was successfully stopped</returns>
    [PreserveSig]
    int StopMulticast([MarshalAs(UnmanagedType.LPStr)] string address, ushort port);

    /// <summary>
    /// Set the network interface to use for multicast operations.
    /// </summary>
    /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
    /// <returns>an HRESULT indicating whether the address was successfully set</returns>
    [PreserveSig]
    int SetNetworkInterface([MarshalAs(UnmanagedType.LPStr)] string address);

    /// <summary>
    /// Get the network interface configured for use for multicast operations.
    /// </summary>
    /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
    /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
    [PreserveSig]
    int GetNetworkInterface([Out, MarshalAs(UnmanagedType.LPStr)] out string address);
  }
}