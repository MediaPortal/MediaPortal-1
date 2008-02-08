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
using DirectShowLib;

namespace TvLibrary.Interfaces.Analyzer
{
  [ComVisible(true), ComImport,
Guid("5EB9F392-E7FD-4071-8E44-3590E5E767BA"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsFilter
  {
    [PreserveSig] int AddChannel(ref int handle);
    [PreserveSig] int DeleteChannel(int handle);
    [PreserveSig] int DeleteAllChannels();

    [PreserveSig] int AnalyzerSetVideoPid(int handle, int videoPid);
    [PreserveSig] int AnalyzerGetVideoPid(int handle, out int  videoPid);
    [PreserveSig] int AnalyzerSetAudioPid(int handle, int audioPid);
    [PreserveSig] int AnalyzerGetAudioPid(int handle, out int audioPid);
    [PreserveSig] int AnalyzerIsVideoEncrypted(int handle, out int yesNo);
    [PreserveSig] int AnalyzerIsAudioEncrypted(int handle, out int yesNo);
    [PreserveSig] int AnalyzerReset(int handle);

    [PreserveSig] int PmtSetPmtPid(int handle, int pmtPid, long serviceId);
    [PreserveSig] int PmtSetCallBack(int handle,  IPMTCallback callback);
    [PreserveSig] int PmtGetPMTData(int handle, IntPtr pmtData);


    [PreserveSig] int RecordSetRecordingFileName(int handle, [In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig] int RecordStartRecord(int handle);
    [PreserveSig] int RecordStopRecord(int handle);
    [PreserveSig] int RecordGetMode(int handle, out TimeShiftingMode mode);
    [PreserveSig] int RecordSetMode(int handle, TimeShiftingMode mode);
    [PreserveSig] int RecordSetPmtPid(int handle, int pmtPid, int serviceId,[In, MarshalAs(UnmanagedType.LPArray)] byte[] pmtData,int pmtLength);

    [PreserveSig] int TimeShiftSetTimeShiftingFileName(int handle, [In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig] int TimeShiftStart(int handle);
    [PreserveSig] int TimeShiftStop(int handle);
    [PreserveSig] int TimeShiftReset(int handle);
    [PreserveSig] int TimeShiftGetBufferSize(int handle, out long size);
    [PreserveSig] int TimeShiftSetMode(int handle, TimeShiftingMode  mode);
    [PreserveSig] int TimeShiftGetMode(int handle, out TimeShiftingMode mode);
    [PreserveSig] int TimeShiftSetPmtPid(int handle, int pmtPid, int serviceId,[In, MarshalAs(UnmanagedType.LPArray)]  byte[] pmtData,int pmtLength);
    [PreserveSig] int TimeShiftPause(int handle, byte onOff);
    [PreserveSig] int TimeShiftSetParams(int handle, int minFiles, int maxFiles, UInt32 chunkSize);
    [PreserveSig] int SetVideoAudioObserver(int handle, IVideoAudioObserver observer);

    [PreserveSig] int TTxStart(int handle);
    [PreserveSig] int TTxStop(int handle);
    [PreserveSig] int TTxSetTeletextPid(int handle, int teletextPid);
    [PreserveSig] int TTxSetCallBack(int handle, ITeletextCallBack callback);
    [PreserveSig] int CaSetCallBack(int handle, ICACallback callback);
    [PreserveSig] int CaGetCaData(int handle, IntPtr caData);
    [PreserveSig] int CaReset(int handle);

  }
}
