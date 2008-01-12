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
  /// Interface to the Teletext callback 
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("540EA3F3-C2E0-4a96-9FC2-071875962911"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITeletextCallBack
  {
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
  /// Interface to the Teletext grabber com object
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("9A9E7592-A178-4a63-A210-910FD7FFEC8C"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTeletextGrabber
  {
    /// <summary>
    /// Starts grabbing teletext.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Start();

    /// <summary>
    /// Stops grabbing teletext.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Stop();

    /// <summary>
    /// Sets the teletext pid.
    /// </summary>
    /// <param name="teletextPid">The teletext pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTeletextPid(short teletextPid);

    /// <summary>
    /// Sets the call back which will be called when teletext has been received.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetCallBack(ITeletextCallBack callback);

  }
}
