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

namespace DShowNET.TsFileSink
{

  [ComVisible(true), ComImport,
 Guid("0d2620cd-a57a-4458-b96f-76442b70e9c7"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITSFileSink
  {
    [PreserveSig]
    int GetBufferSize(ref int size);
    [PreserveSig]
    int SetRegSettings();
    [PreserveSig]
    int GetRegSettings();
    [PreserveSig]
    int GetRegFileName([In, Out, MarshalAs(UnmanagedType.AnsiBStr)] ref string fileName);
    [PreserveSig]
    int SetRegFileName([In, MarshalAs(UnmanagedType.AnsiBStr)] ref string fileName);
    [PreserveSig]
    int GetBufferFileName([In,Out, MarshalAs(UnmanagedType.LPWStr)] ref StringBuilder fileName);
    [PreserveSig]
    int SetBufferFileName([In, MarshalAs(UnmanagedType.AnsiBStr)] string fileName);
    //	int GetCurrentTSFile(FileWriter* fileWriter) ;
    [PreserveSig]
    int GetNumbFilesAdded(ref ushort numbAdd);
    [PreserveSig]
    int GetNumbFilesRemoved(ref ushort numbRem);
    [PreserveSig]
    int GetCurrentFileId(ref ushort fileID);
    [PreserveSig]
    int GetMinTSFiles(ref ushort minFiles);
    [PreserveSig]
    int SetMinTSFiles(ushort minFiles);
    [PreserveSig]
    int GetMaxTSFiles(ref ushort maxFiles);
    [PreserveSig]
    int SetMaxTSFiles(ushort maxFiles);
    [PreserveSig]
    int GetMaxTSFileSize(ref long maxSize);
    [PreserveSig]
    int SetMaxTSFileSize(long maxSize);
    [PreserveSig]
    int GetChunkReserve(ref long chunkSize);
    [PreserveSig]
    int SetChunkReserve(long chunkSize);
    [PreserveSig]
    int GetFileBufferSize(ref long lpllsize);
  }
}
