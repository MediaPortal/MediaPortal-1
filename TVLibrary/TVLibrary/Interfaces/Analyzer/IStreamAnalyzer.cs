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
  /// Interface to the PMT callback 
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("37A1C1E3-4760-49fe-AB59-6688ADA54923"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IPMTCallback
  {
    /// <summary>
    /// Called when the PMT has been received.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int OnPMTReceived();
  };

  /// <summary>
  /// Interface for h/w pid filtering 
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("1F4566CD-61A1-4bf9-9544-9D4C4D120DB6"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IHardwarePidFiltering
  {
    /// <summary>
    /// Sets the pids to filter.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="pids">The pids.</param>
    /// <returns></returns>
    [PreserveSig]
    int FilterPids(short count, IntPtr pids);
  };


}
