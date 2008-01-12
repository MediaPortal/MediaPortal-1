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
  /// interface to the pmt grabber com object
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("6E714740-803D-4175-BEF6-67246BDF1855"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsPmtGrabber
  {
    /// <summary>
    /// Sets the PMT pid.
    /// </summary>
    /// <param name="pmtPid">The PMT pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetPmtPid(short pmtPid, int serviceId);

    /// <summary>
    /// Sets the call back to be called when PMT is received.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(IPMTCallback callback);

    /// <summary>
    /// Gets the PMT data.
    /// </summary>
    /// <param name="pmt">The PMT.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetPMTData(IntPtr pmt);
  }
}
