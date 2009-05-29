#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Runtime.InteropServices;

namespace DShowNET.MPSA
{
  [ComVisible(true), ComImport,
   Guid("6301D1B8-6C92-4c6e-8CC2-CD1B05C6B545"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEPGGrabber
  {
    [PreserveSig]
    int GrabEPG();

    [PreserveSig]
    int IsEPGReady(out bool yesNo);

    [PreserveSig]
    int GetEPGChannelCount([Out] out uint channelCount);

    [PreserveSig]
    int GetEPGEventCount([In] uint channel, [Out] out uint eventCount);

    [PreserveSig]
    int GetEPGChannel([In] uint channel, [In, Out] ref UInt16 networkId, [In, Out] ref UInt16 transportid,
                      [In, Out] ref UInt16 service_id);

    [PreserveSig]
    int GetEPGEvent([In] uint channel, [In] uint eventid, [Out] out uint languageCount, [Out] out uint date,
                    [Out] out uint time, [Out] out uint duration, out IntPtr genre);

    [PreserveSig]
    int GetEPGLanguage([In] uint channel, [In] uint eventid, [In] uint languageIndex, [Out] out uint language,
                       [Out] out IntPtr eventText, [Out] out IntPtr eventDescription);
  }
}