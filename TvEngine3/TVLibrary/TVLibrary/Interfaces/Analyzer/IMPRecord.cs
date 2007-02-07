/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
    /// Sets the name of the recording file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetRecordingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int StartRecord();
    /// <summary>
    /// Stops the recording.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int StopRecord();
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
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTimeShiftFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    /// <summary>
    /// Starts  time shifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int StartTimeShifting();
    /// <summary>
    /// Stops  time shifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int StopTimeShifting();
    /// <summary>
    /// Pauses/Continues timeshifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int PauseTimeShifting(short onOff);
  }
}
