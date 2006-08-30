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
  Guid("6F78D59C-1066-4e1b-8258-717F33C51F67"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMHWGrabber
  {
    [PreserveSig]
    int GrabMHW();

    [PreserveSig]
    int IsMHWReady(out bool yesNo);

    [PreserveSig]
    int GetMHWTitleCount(out Int16 count);

    [PreserveSig]
    int GetMHWTitle(Int16 program, ref Int16 id, ref Int16 transportId, ref Int16 networkId, ref Int16 channelId, ref Int16 programId, ref Int16 themeId, ref Int16 PPV, ref byte Summaries, ref Int16 duration, ref uint dateStart, ref uint timeStart, out IntPtr title, out IntPtr programName);

    [PreserveSig]
    int GetMHWChannel(Int16 channelNr, ref Int16 channelId, ref Int16 networkId, ref Int16 transportId, out IntPtr channelName);

    [PreserveSig]
    int GetMHWSummary(Int16 programId, out IntPtr summary);

    [PreserveSig]
    int GetMHWTheme(Int16 themeId, out IntPtr theme);
  };
}
