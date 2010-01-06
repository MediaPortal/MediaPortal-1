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
  ///<summary>
  /// Channel scanning callback
  ///</summary>
  [ComVisible(true), ComImport,
   Guid("CE141670-1840-4188-8A40-618BA3A5A1C3"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IChannelScanCallback
  {
    /// <summary>
    /// Called when the channel scanning is done
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int OnScannerDone();
  }

  /// <summary>
  /// interface to the channel scan com object
  /// </summary>
  [ComVisible(true), ComImport,
   Guid("1663DC42-D169-41da-BCE2-EEEC482CB9FB"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsChannelScan
  {
    /// <summary>
    /// Starts scanning the current transponder.
    /// </summary>
    /// <param name="waitForVCT">Tells the analyzer to wait for a vct section in addition to just the pmt</param>
    /// <returns></returns>
    [PreserveSig]
    int Start(bool waitForVCT);

    /// <summary>
    /// Stops scanning.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Stop();

    /// <summary>
    /// Returns the number of channels found.
    /// </summary>
    /// <param name="channelCount">The channel count.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetCount(out short channelCount);

    /// <summary>
    /// Determines whether scanner is finished or not.
    /// </summary>
    /// <param name="yesNo">true when scanner is finished else false</param>
    /// <returns></returns>
    [PreserveSig]
    int IsReady(out bool yesNo);

    /// <summary>
    /// Gets the details for a channel.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="networkId">The network id.</param>
    /// <param name="transportId">The transport id.</param>
    /// <param name="serviceId">The service id.</param>
    /// <param name="majorChannel">The major channel.</param>
    /// <param name="minorChannel">The minor channel.</param>
    /// <param name="frequency">The frequency.</param>
    /// <param name="lcn">The LCN.</param>
    /// <param name="freeCAMode">The free CA mode.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="modulation">The modulation.</param>
    /// <param name="providerName">Name of the provider.</param>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="pmtPid">The PMT pid.</param>
    /// <param name="hasVideo">Whether the channel has Video.</param>
    /// <param name="hasAudio">Whether the channel has Audio.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChannel(short index,
                   out int networkId,
                   out int transportId,
                   out int serviceId,
                   out short majorChannel,
                   out short minorChannel,
                   out short frequency,
                   out short lcn,
                   out short freeCAMode,
                   out short serviceType,
                   out short modulation,
                   out IntPtr providerName,
                   out IntPtr serviceName,
                   out short pmtPid,
                   out short hasVideo,
                   out short hasAudio);

    /// <summary>
    /// Sets the channel scan callback
    /// </summary>
    /// <param name="callback">The callback</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IChannelScanCallback callback);


    /// <summary>
    /// Start the nit scan
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int ScanNIT();

    /// <summary>
    /// Stops the nit scan
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int StopNIT();

    /// <summary>
    /// Gets the number of nit transponders
    /// </summary>
    /// <param name="transponderCount">Number of transponders</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNITCount(out int transponderCount);

    /// <summary>
    /// Gets the nit channel
    /// </summary>
    /// <param name="channel">The channel id</param>
    /// <param name="chType">The channel type</param>
    /// <param name="frequency">The frequence</param>
    /// <param name="polarisation">The polarisation</param>
    /// <param name="modulation">The modulation</param>
    /// <param name="symbolrate">The symbolrate</param>
    /// <param name="bandwidth">The bandwith</param>
    /// <param name="fecInner">The fec inner</param>
    /// <param name="networkName">The network names</param>
    /// <param name="rollOff">rolloff (S2 specific)</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNITChannel(int channel, out int chType, out int frequency, out int polarisation, out int modulation,
                      out int symbolrate, out int bandwidth, out int fecInner, out int rollOff, out IntPtr networkName);
  }
}