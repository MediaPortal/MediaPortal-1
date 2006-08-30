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
  Guid("1663DC42-D169-41da-BCE2-EEEC482CB9FB"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsChannelScan
  {
    [PreserveSig]
    int Start();

    [PreserveSig]
    int Stop();

    [PreserveSig]
    int GetCount(out short channelCount);

    [PreserveSig]
    int GetChannel(short index,
                       out short networkId,
                       out short transportId,
                       out short serviceId,
                       out short majorChannel,
                       out short minorChannel,
                       out short frequency,
                       out short lcn,
                       out short EIT_schedule_flag,
                       out short EIT_present_following_flag,
                       out short runningStatus,
                       out short freeCAMode,
                       out short serviceType,
                       out short modulation,
                       out IntPtr providerName,
                       out IntPtr serviceName,
                       out short pcrPid,
                       out short pmtPid,
                       out short videoPid,
                       out short audio1Pid,
                       out short audio2Pid,
                       out short audio3Pid,
                       out short ac3Pid,
                       out IntPtr audioLanguage1,
                       out IntPtr audioLanguage2,
                       out IntPtr audioLanguage3,
                       out short teletextPid,
                       out short subtitlePid);
  }
}
