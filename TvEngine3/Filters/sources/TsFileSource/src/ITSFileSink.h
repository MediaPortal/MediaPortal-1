/**
*  ITSFileSink.h
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  authors can be reached on the forums at
*    http://forums.dvbowners.com/
*/


// {0d2620cd-a57a-4458-b96f-76442b70e9c7}
DEFINE_GUID(IID_ITSFileSink,
0x0d2620cd, 0xa57a, 0x4458, 0xb9, 0x6f, 0x76, 0x44, 0x2b, 0x70, 0xe9, 0xc7);

DECLARE_INTERFACE_(ITSFileSink, IUnknown)
{
	STDMETHOD(GetBufferSize) (THIS_ long * size) PURE;
	STDMETHOD(SetRegSettings) (void) PURE;
	STDMETHOD(GetRegSettings) (void) PURE;
	STDMETHOD(GetRegFileName) (THIS_ LPTSTR fileName) PURE;
	STDMETHOD(SetRegFileName) (THIS_ LPTSTR fileName) PURE;
	STDMETHOD(GetBufferFileName) (THIS_ LPWSTR fileName) PURE;
	STDMETHOD(SetBufferFileName) (THIS_ LPWSTR fileName) PURE;
//	STDMETHOD(GetCurrentTSFile) (THIS_ FileWriter* fileWriter) PURE;
	STDMETHOD(GetNumbFilesAdded) (THIS_ WORD *numbAdd) PURE;
	STDMETHOD(GetNumbFilesRemoved) (THIS_ WORD *numbRem) PURE;
	STDMETHOD(GetCurrentFileId) (THIS_ WORD *fileID) PURE;
	STDMETHOD(GetMinTSFiles) (THIS_ WORD *minFiles) PURE;
	STDMETHOD(SetMinTSFiles) (THIS_ WORD minFiles) PURE;
	STDMETHOD(GetMaxTSFiles) (THIS_ WORD *maxFiles) PURE;
	STDMETHOD(SetMaxTSFiles) (THIS_ WORD maxFiles) PURE;
	STDMETHOD(GetMaxTSFileSize) (THIS_ __int64 *maxSize) PURE;
	STDMETHOD(SetMaxTSFileSize) (THIS_ __int64 maxSize) PURE;
	STDMETHOD(GetChunkReserve) (THIS_ __int64 *chunkSize) PURE;
	STDMETHOD(SetChunkReserve) (THIS_ __int64 chunkSize) PURE;
	STDMETHOD(GetFileBufferSize) (THIS_ __int64 *lpllsize) PURE;
	STDMETHOD(GetNumbErrorPackets) (THIS_ __int64 *lpllErrors) PURE;
	STDMETHOD(SetNumbErrorPackets) (THIS_ __int64 lpllErrors) PURE;
};

