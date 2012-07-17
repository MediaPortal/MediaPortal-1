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
    /// <param name="hasCaDescriptor">Whether the channel has a conditional access descriptor.</param>
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
                   out short hasAudio,
                   out short hasCaDescriptor);

    /// <summary>
    /// Sets the channel scan callback
    /// </summary>
    /// <param name="callback">The callback</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IChannelScanCallback callback);

    #region network information table scanning

    /// <summary>
    /// Start an NIT scan.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int ScanNIT();

    /// <summary>
    /// Stop an NIT scan.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int StopNIT();

    /// <summary>
    /// Get the number of transponder details found by an NIT scan.
    /// </summary>
    /// <param name="transponderCount">The number of transponder details found.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNITCount(out int transponderCount);

    /// <summary>
    /// Get the details for a single transponder found by an NIT scan.
    /// </summary>
    /// <param name="channel">The transponder index (zero based).</param>
    /// <param name="chType">The transponder type.</param>
    /// <param name="frequency">The transponder frequency, in kHz.</param>
    /// <param name="polarisation">The transponder polarisation. Only applicable for DVB-S/2 transponders.</param>
    /// <param name="modulation">The transponder modulation scheme. Only applicable for DVB-S/2 and DVB-C transponders.</param>
    /// <param name="symbolRate">The transponder symbol rate, in ks/s. Only applicable for DVB-S/2 and DVB-C transponders.</param>
    /// <param name="bandwidth">The transponder bandwith, in MHz. Only applicable for DVB-T transponders.</param>
    /// <param name="innerFecRate">The transponder inner FEC rate. Only applicable for DVB-S/2 transponders.</param>
    /// <param name="rollOff">The transponder roll-off parameter. Only applicable for DVB-S2 transponders.</param>
    /// <param name="networkName">The name of the network that the transponder is associated with.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNITChannel(int channel, out int chType, out int frequency, out int polarisation, out int modulation,
                      out int symbolRate, out int bandwidth, out int innerFecRate, out int rollOff, out IntPtr networkName);

    #endregion
  }
}