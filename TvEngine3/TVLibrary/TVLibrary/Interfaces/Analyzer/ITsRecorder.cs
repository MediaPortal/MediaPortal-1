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
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// interface to the file recorder comobject
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("B45662E3-2749-4a34-993A-0C1659E86E83"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsRecorder
  {
    /// <summary>
    /// Sets the filename to record the stream to.
    /// </summary>
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
    /// Sets the pmt pid
    /// </summary>
    /// <param name="pmtPid">The PMT pid</param>
    /// <returns></returns>
    [PreserveSig]
    int SetPmtPid(short pmtPid);
  }
}
