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

using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface
{
  /// <summary>
  /// Called by the timeshifting interface when the interface needs to notify the controlling application
  /// about critical playback/recording status changes.
  /// </summary>
  /// <param name="state">The current timeshifting interface state.</param>
  /// <returns>???</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate uint OnTimeShiftState(PvrCallBackState state);

  [Guid("a306af1c-51d9-496d-9e7a-1cfe28f51fda"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMpeg2TimeShiftCtrl
  {
    /// <summary>
    /// Set the name of the file to use for timeshifting. Default is C:\Timeshift.ts.
    /// </summary>
    /// <param name="fileName">The name of the timeshifting file.</param>
    /// <returns>an HRESULT indicating whether the file name was successfully set</returns>
    [PreserveSig]
    int SetFilename([MarshalAs(UnmanagedType.LPWStr)] string fileName);

    /// <summary>
    /// Get the name of the file configured for use for timeshifting.
    /// </summary>
    /// <param name="fileName">The name of the timeshifting file.</param>
    /// <returns>an HRESULT indicating whether the file name was successfully retrieved</returns>
    [PreserveSig]
    int GetFilename([Out, MarshalAs(UnmanagedType.LPWStr)] out string fileName);

    /// <summary>
    /// Start recording during live streaming. Recording is usually only started when live streaming is
    /// paused.
    /// </summary>
    /// <returns>an HRESULT indicating whether recording was successfully started</returns>
    [PreserveSig]
    int StartRecord();

    /// <summary>
    /// Stop recording immediately. Recording is usually stopped when timeshifting catches up with the
    /// live position.
    /// </summary>
    /// <returns>an HRESULT indicating whether recording was successfully stopped</returns>
    [PreserveSig]
    int StopRecord();

    /// <summary>
    /// Enable the timeshifting capability.
    /// </summary>
    /// <returns>an HRESULT indicating whether the timeshifting capability was successfully enabled</returns>
    [PreserveSig]
    int Enable();

    /// <summary>
    /// Disable the timeshifting capability.
    /// </summary>
    /// <returns>an HRESULT indicating whether the timeshifting capability was successfully disabled</returns>
    [PreserveSig]
    int Disable();

    /// <summary>
    /// Set the value of one of the timeshifting interface options.
    /// </summary>
    /// <param name="option">The option to set.</param>
    /// <param name="value">The option value to set.</param>
    /// <returns>an HRESULT indicating whether the option value was successfully set</returns>
    [PreserveSig]
    int SetOption(PvrOption option, int value);

    /// <summary>
    /// Get the value of one of the timeshifting interface options.
    /// </summary>
    /// <param name="option">The option to get.</param>
    /// <param name="value">The option value.</param>
    /// <returns>an HRESULT indicating whether the option value was successfully retrieved</returns>
    [PreserveSig]
    int GetOption(PvrOption option, [Out] out int value);

    /// <summary>
    /// Set the playback marker position within the timeshifting file.
    /// </summary>
    /// <param name="filePosition">The playback marker position.</param>
    /// <returns>an HRESULT indicating whether the marker position was successfully set</returns>
    [PreserveSig]
    int SetFilePosition(long filePosition);

    /// <summary>
    /// Get the current size of the file configured for use for timeshifting.
    /// </summary>
    /// <param name="fileSize">The size of the timeshifting file.</param>
    /// <returns>an HRESULT indicating whether the file size was successfully retrieved</returns>
    [PreserveSig]
    int GetFileSize([Out] out long fileSize);

    /// <summary>
    /// Get the playback marker position within the timeshifting file.
    /// </summary>
    /// <param name="filePosition">The playback marker position.</param>
    /// <returns>an HRESULT indicating whether the marker position was successfully retrieved</returns>
    [PreserveSig]
    int GetFilePosition([Out] out long filePosition);

    /// <summary>
    /// Register a call back delegate that the interface can use to notify the application about critical
    /// playback/recording state changes.
    /// </summary>
    /// <param name="callBack">A pointer to the call back delegate.</param>
    /// <returns>an HRESULT indicating whether the call back delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallback(OnTimeShiftState callBack);
  }
}