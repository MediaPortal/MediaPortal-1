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
  [ComVisible(true), ComImport,
  Guid("540EA3F3-C2E0-4a96-9FC2-071875962911"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITeletextCallBack
  {
    [PreserveSig]
    int OnTeletextReceived(IntPtr data, short packetCount);
  };

  [ComVisible(true), ComImport,
 Guid("9A9E7592-A178-4a63-A210-910FD7FFEC8C"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTeletextGrabber
  {
    [PreserveSig]
    int Start();

    [PreserveSig]
    int Stop();

    [PreserveSig]
    int SetTeletextPid(short teletextPid);

    [PreserveSig]
    int SetCallBack(ITeletextCallBack callback);

  }
}
