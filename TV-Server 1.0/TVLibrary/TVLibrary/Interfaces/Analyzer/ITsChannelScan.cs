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
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{

  [ComVisible(true), ComImport,
  Guid("CE141670-1840-4188-8A40-618BA3A5A1C3"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IChannelScanCallback
  {
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
	  int IsReady( out bool yesNo);

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
    
    [PreserveSig]
	  int SetCallBack(IChannelScanCallback callback);


    [PreserveSig]
    int ScanNIT();

    [PreserveSig]
    int StopNIT();

    [PreserveSig]
    int GetNITCount(out int transponderCount);

    [PreserveSig]
    int GetNITChannel(int channel, out int chType, out int frequency, out int polarisation, out int modulation, out int symbolrate, out int bandwidth, out int fecInner, out IntPtr networkName);

  }
}
