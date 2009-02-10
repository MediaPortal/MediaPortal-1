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
using System;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// Interface to the channel linkage scanner com object
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("26DF395C-3D13-4f3e-9EC1-453FAAFFB13E"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsChannelLinkageScanner
  {
    /// <summary>
    /// Start scanning for channel links.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Start();

    /// <summary>
    /// Resets the scanner. Also can be used to abort scanning.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();

    /// <summary>
    /// Gets the nummer of channels for which linked channels have been received
    /// </summary>
    /// <param name="channelCount">The channel count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChannelCount([Out] out uint channelCount);

    /// <summary>
    /// Gets the channel details.
    /// </summary>
    /// <param name="channelIndex">The channel.</param>
    /// <param name="network_id">The network id.</param>
    /// <param name="transport_id">The transportid.</param>
    /// <param name="service_id">The service_id.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChannel([In] uint channelIndex, [In, Out] ref UInt16 network_id, [In, Out] ref UInt16 transport_id, [In, Out] ref UInt16 service_id);

    /// <summary>
    /// Gets the number of linked channels for a channel
    /// </summary>
    /// <param name="channelIndex">The channel.</param>
    /// <param name="linkedChannelsCount">The link count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetLinkedChannelsCount([In] uint channelIndex, [Out] out uint linkedChannelsCount);

    /// <summary>
    /// Gets the linked channel details.
    /// </summary>
    /// <param name="channelIndex">The channel.</param>
    /// <param name="linkIndex">The linked channel id.</param>
    /// <param name="network_id">The network id.</param>
    /// <param name="transport_id">The transportid.</param>
    /// <param name="service_id">The service_id.</param>
    /// <param name="name">The channel name</param>
    /// <returns></returns>
    [PreserveSig]
    int GetLinkedChannel([In] uint channelIndex, [In] uint linkIndex, [In, Out] ref UInt16 network_id, [In, Out] ref UInt16 transport_id, [In, Out] ref UInt16 service_id, out IntPtr name);


    /// <summary>
    /// Sets the call back which will be called when links have been received
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IChannelLinkageCallback callback);
  }
}
