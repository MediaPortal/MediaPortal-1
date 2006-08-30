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
Guid("89459BF6-D00E-4d28-928E-9DA8F76B6D3A"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTimeShift
  {
    [PreserveSig]
    int SetPcrPid(short pcrPid);
    [PreserveSig]
    int AddPesStream(short pid, bool isAudio, bool isVideo);
    [PreserveSig]
    int RemovePesStream(short pid);
    [PreserveSig]
    int SetTimeShiftingFileName([In, MarshalAs(UnmanagedType.LPStr)]			string fileName);
    [PreserveSig]
    int Start();
    [PreserveSig]
    int Stop();
    [PreserveSig]
    int Reset();
    [PreserveSig]
    int GetBufferSize(out uint size);
    [PreserveSig]
    int GetNumbFilesAdded(out ushort numbAdd);
    [PreserveSig]
    int GetNumbFilesRemoved(out ushort numbRem);
    [PreserveSig]
    int GetCurrentFileId(out ushort fileID);
    [PreserveSig]
    int GetMinTSFiles(out ushort minFiles);
    [PreserveSig]
    int SetMinTSFiles(ushort minFiles);
    [PreserveSig]
    int GetMaxTSFiles(out ushort maxFiles);
    [PreserveSig]
    int SetMaxTSFiles(ushort maxFiles);
    [PreserveSig]
    int GetMaxTSFileSize(out long maxSize);
    [PreserveSig]
    int SetMaxTSFileSize(long maxSize);
    [PreserveSig]
    int GetChunkReserve(out long chunkSize);
    [PreserveSig]
    int SetChunkReserve(long chunkSize);
    [PreserveSig]
    int GetFileBufferSize(out long lpllsize);
  }
}
