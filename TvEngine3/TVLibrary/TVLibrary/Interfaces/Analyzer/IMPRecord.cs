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
  /// <summary>
  /// Interface to the File recorder com object
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("d5ff805e-a98b-4d56-bede-3f1b8ef72533"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPRecord
  {
    /// <summary>
    /// Sets the mode of the recording
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [PreserveSig]
    int SetRecordingMode(int subChannelId, TimeShiftingMode mode);
    /// <summary>
    /// Sets the name of the recording file.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetRecordingFileName(int subChannelId, [In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int StartRecord(int subChannelId);
    /// <summary>
    /// Stops the recording.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int StopRecord(int subChannelId);
    /// <summary>
    /// Determines whether the we are receiving audio and video.
    /// </summary>
    /// <param name="yesNo">if we are receiving audio and video then true<c>true</c></param>
    /// <returns></returns>
    [PreserveSig]
    int IsReceiving(out bool yesNo);

    /// <summary>
    /// Resets this recorder.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();
    /// <summary>
    /// Sets the name of the time shift file.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTimeShiftFileName(int subChannelId, [In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    /// <summary>
    /// Starts  time shifting.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int StartTimeShifting(int subChannelId);
    /// <summary>
    /// Stops  time shifting.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int StopTimeShifting(int subChannelId);
    /// <summary>
    /// Pauses/Continues timeshifting.
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int PauseTimeShifting(int subChannelId, short onOff);

    /// <summary>
    /// Sets the timeshift parameters
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <param name="minFiles">Number of minimum timeshifting files</param>
    /// <param name="maxFiles">Number of maximum timeshifting files</param>
    /// <param name="maxFileSize">Maximum file size for each timeshifting file</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTimeShiftParams(int subChannelId, int minFiles, int maxFiles, UInt32 maxFileSize);

    /// <summary>
    /// Sets the callback for teletext packets
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <param name="callback">Callback to set</param>
    /// <returns></returns>
    [PreserveSig]
    int TTxSetCallback(int subChannelId,IAnalogTeletextCallBack callback);

    /// <summary>
    /// Sets the callback for the video/audio observer
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <param name="observer">Video/audio observer</param>
    /// <returns></returns>
    [PreserveSig]
    int SetVideoAudioObserver(int subChannelId, IAnalogVideoAudioObserver observer);

    /// <summary>
    /// Adds a new subchannel
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int AddChannel(ref int subChannelId);

    /// <summary>
    /// Deletes the subchannel with the given id
    /// </summary>
    /// <param name="subChannelId">SubChannel id</param>
    /// <returns></returns>
    [PreserveSig]
    int DeleteChannel(int subChannelId);

    /// <summary>
    /// Deletes all subchannels
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int DeleteAllChannels();

  }

  /// <summary>
  /// Interface to the Teletext callback 
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("14639355-4BA4-471c-BA91-8B4AF51F3A0D"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAnalogTeletextCallBack {
    /// <summary>
    /// Called when teletext has been received.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="packetCount">The packet count.</param>
    /// <returns></returns>
    [PreserveSig]
    int OnTeletextReceived(IntPtr data, short packetCount);
  };

  /// <summary>
  /// Interface to the analog channel scan interface
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("D44ABA24-57B2-44de-8D56-7B95CBF8527A"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAnalogChanelScan {
    /// <summary>
    /// Starts scanning the current transponder.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Start();

    /// <summary>
    /// Stops scanning.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Stop();

    /// <summary>
    /// Determines whether scanner is finished or not.
    /// </summary>
    /// <param name="yesNo">true when scanner is finished otherwise false</param>
    /// <returns></returns>
    [PreserveSig]
    int IsReady(out bool yesNo);

    /// <summary>
    /// Gets the name of a channel.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChannel(out IntPtr serviceName);

    /// <summary>
    /// Sets the callback, which gets called when the channel scan is completed
    /// </summary>
    /// <param name="callback">Callback to set</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IAnalogChannelScanCallback callback);

  };

    /// <summary>
  /// Interface to the analog channel callback
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("9C9B9E27-A9EA-4ac9-B2FB-FC9FCACECA82"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAnalogChannelScanCallback {
    /// <summary>
    /// Gets called when the scanning is done
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int OnScannerDone();
  }

}
