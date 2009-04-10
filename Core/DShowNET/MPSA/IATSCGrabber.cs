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
   Guid("3921427B-72AC-4e4d-AF4F-518AFE1D0780"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IATSCGrabber
  {
    [PreserveSig]
    int GrabATSC();

    [PreserveSig]
    int IsATSCReady(out bool yesNo);

    [PreserveSig]
    int GetATSCTitleCount([Out] out UInt16 channelCount);

    [PreserveSig]
    int GetATSCTitle(Int16 no, out Int16 source_id, out uint starttime, out Int16 length_in_mins, out IntPtr title,
                     out IntPtr description);
  }
}