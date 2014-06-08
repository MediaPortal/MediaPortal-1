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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface
{
  /// <summary>
  /// Called by the AV interface when the interface wants to notify the controlling application
  /// about the video stream information.
  /// </summary>
  /// <param name="info">The video stream information.</param>
  /// <returns>???</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate uint OnVideoInfo(ref VideoInfo info);

  [Guid("3ca933bb-4378-4e03-8abd-02450169aa5e"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMpeg2AvCtrl3
  {
    #region IMpeg2AvCtrl

    /// <summary>
    /// Register the audio and/or video PID(s) that are of interest to the application. Packets
    /// marked with these PIDs will be passed on B2C2 filter audio and video output pins.
    /// </summary>
    /// <param name="audioPid">The audio PID (or zero to not register an audio PID).</param>
    /// <param name="videoPid">The video PID (or zero to not register a video PID).</param>
    /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
    [PreserveSig]
    int SetAudioVideoPIDs(int audioPid, int videoPid);

    #endregion

    #region IMpeg2AvCtrl2

    /// <summary>
    /// Register a call back delegate that the interface can use to notify the application about
    /// video stream information.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    /// <returns>an HRESULT indicating whether the call back delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallbackForVideoMode(OnVideoInfo callBack);

    /// <summary>
    /// Deregister the current audio and/or video PID(s) when they are no longer of interest to the
    /// application. Packets marked with the previously registered PIDs will no longer be passed on
    /// the B2C2 filter audio and video output pins.
    /// </summary>
    /// <param name="audioPid">A non-zero value to deregister the current audio PID.</param>
    /// <param name="videoPid">A non-zero value to deregister the current video PID.</param>
    /// <returns>an HRESULT indicating whether the PID(s) were successfully deregistered</returns>
    [PreserveSig]
    int DeleteAudioVideoPIDs(int audioPid, int videoPid);

    /// <summary>
    /// Get the current audio and video settings.
    /// </summary>
    /// <param name="openAudioStreamCount">The number of currently open audio streams.</param>
    /// <param name="openVideoStreamCount">The number of currently open video streams.</param>
    /// <param name="totalAudioStreamCount">The number of audio streams in the full transport stream.</param>
    /// <param name="totalVideoStreamCount">The number of video streams in the full transport stream.</param>
    /// <param name="audioPid">The current audio PID.</param>
    /// <param name="videoPid">The current video PID.</param>
    /// <returns>an HRESULT indicating whether the settings were successfully retrieved</returns>
    [PreserveSig]
    int GetAudioVideoState(out int openAudioStreamCount, out int openVideoStreamCount,
          out int totalAudioStreamCount, out int totalVideoStreamCount,
          out int audioPid, out int videoPid
    );

    #endregion

    #region IMpeg2AvCtrl3

    /// <summary>
    /// Get IR data from the interface. The size of each code is two bytes, and up to 4 codes may
    /// be retrieved in one call.
    /// </summary>
    /// <param name="dataBuffer">A buffer for the interface to populate.</param>
    /// <param name="bufferCapacity">As an input, the number of IR codes that the buffer is able
    ///   to hold; as an output, the number of IR codes in the buffer.</param>
    /// <returns>an HRESULT indicating whether the IR data was successfully retrieved</returns>
    [PreserveSig]
    int GetIRData(out long dataBuffer, ref int bufferCapacity);

    #endregion
  }
}