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
using System;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  ///<summary>
  /// The tswriter ca callback
  ///</summary>
  [ComVisible(true), ComImport,
 Guid("38536AB6-7A41-404f-917F-C47DD8B639C7"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ICACallback
  {
    /// <summary>
    /// Called when the Ca has been received.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int OnCaReceived();
  };

  /// <summary>
  /// interface to the pmt grabber com object
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("F9AA3910-7818-452a-94D1-72E039DF50EF"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsCaGrabber
  {
    /// <summary>
    /// Sets the call back to be called when CA is received.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(ICACallback callback);

    /// <summary>
    /// Gets the CA data.
    /// </summary>
    /// <param name="caData">The caData.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetCaData(IntPtr caData);
    /// <summary>
    /// Resets the ca grabber.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();
  }
}
