#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.MPTSWriter
{

  [ComVisible(true), ComImport,
  Guid("236D0A77-D105-43fd-A203-578859AB7948"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPTSWriter
  {
    [PreserveSig]
    int ResetPids();
    [PreserveSig]
    int SetVideoPid(ushort videoPid);
    [PreserveSig]
    int SetAudioPid(ushort audioPid);
    [PreserveSig]
    int SetAudioPid2(ushort audioPid);
    [PreserveSig]
    int SetAC3Pid(ushort ac3Pid);
    [PreserveSig]
    int SetTeletextPid(ushort ttxtPid);
    [PreserveSig]
    int SetSubtitlePid(ushort subtitlePid);
    [PreserveSig]
    int SetPMTPid(ushort pmtPid);
    [PreserveSig]
    int SetPCRPid(ushort pcrPid);
    [PreserveSig]
    int TimeShiftBufferDuration(out long timeInTimeShiftBuffer);
    [PreserveSig]
    int IsStarted(out ushort yesNo);
  }

}
