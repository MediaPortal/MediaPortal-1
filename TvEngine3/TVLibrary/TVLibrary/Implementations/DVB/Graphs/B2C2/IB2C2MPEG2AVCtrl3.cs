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
  /// IB2C2MPEG2AVCtrl2 methods allow access to MPEG2 Audio and Video elementary streams by setting or deleting their PIDs. For Video in Windows, a Video callback structure can be configured to pass Video window size, aspect ratio, and frame rate when instructed by the application.
  /// </summary>
  [ComVisible(true), ComImport,
   Guid("3ca933bb-4378-4e03-8abd-02450169aa5e"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IB2C2MPEG2AVCtrl3
  {
    /// <summary>
    /// Sets the Audio and Video PIDs of interest to the application.
    /// </summary>
    /// <param name="pida">Audio PID of interest to the application.</param>
    /// <param name="pidb">Video PID of interest to the application.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int SetAudioVideoPIDs(
      int pida,
      int pidb
      );
    /// <summary>
    /// Sets Callback for Video mode of operation, which allows Video aspect ratio to be reported back to the user application when the user application passes a parameter.
    /// </summary>
    /// <param name="vInfo">Pointer to a callback function with the format: UINT __stdcall (MPEG2_VIDEO_INFO *). </param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int SetCallbackForVideoMode(
      [MarshalAs(UnmanagedType.FunctionPtr)] Delegate vInfo
      );

    /// <summary>
    /// Deletes Audio and Video PIDs. 
    /// </summary>
    /// <param name="pida">A nonzero value indicates that the current Audio PID shall be deleted.</param>
    /// <param name="pidv">A nonzero value indicates that the current Video PID shall be deleted.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
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
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetAudioVideoState(
      out int a,
      out int b,
      out int c,
      out int d,
      out int e,
      out int f
      );

    /// <summary>
    /// Reads IR data received by the SkyStar IR receiver.
    /// </summary>
    /// <param name="plIrBuffer">Pointer to a buffer created by the caller. The buffer will hold the IR data returned by GetIRData(). </param>
    /// <param name="plIrBufferLength">Pointer to a long variable created by the caller. (In) The variable contains the number of IR data entries that the buffer can hold. As each IR data entry is 2 bytes, the buffer length must be at least the number of entries * 2 bytes. The number of entries must be greater than 0 and less than or equal to MAX_IR_DATA (4). (Out) The variable contains the number of two byte IR data entries in the buffer.</param>
    /// <returns>Returns an HRESULT value. Use SUCCEEDED and FAILED macros to interpret the return value.</returns>
    [PreserveSig]
    int GetIRData(
      out int plIrBuffer,
      ref int plIrBufferLength
    );
  } ;

}

