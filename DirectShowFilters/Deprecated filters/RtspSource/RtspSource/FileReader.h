/**
*  FileReader.h
*  Copyright (C) 2005      nate
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
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef FILEREADER
#define FILEREADER

//#include "PidInfo.h"
#include <windows.h>

class FileReader
{
public:

	FileReader();
	virtual ~FileReader();

	virtual FileReader* CreateFileReader();

	// Open and write to the file
	virtual int GetFileName(char *lpszFileName);
	virtual int SetFileName(char* pszFileName);
	virtual int OpenFile();
	virtual int CloseFile();
	virtual int Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes);
	virtual int Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual int get_ReadOnly(WORD *ReadOnly);
	virtual int get_DelayMode(WORD *DelayMode);
	virtual int set_DelayMode(WORD DelayMode);
	virtual int get_ReaderMode(WORD *ReaderMode);
	virtual __int64 GetFileSize();
	virtual int GetFileSize(__int64 *pStartPosition, __int64 *pLength);
	int GetInfoFileSize(__int64 *lpllsize);
	int GetStartPosition(__int64 *lpllpos);
	virtual BOOL IsFileInvalid();
	virtual DWORD SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 GetFilePointer();
	virtual DWORD setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 getFilePointer();

	void SetDebugOutput(BOOL bDebugOutput);

protected:
	HANDLE   m_hFile; 				// Handle to file for streaming
	HANDLE   m_hInfoFile;           // Handle to Infofile for filesize from FileWriter
	char    m_fileName[1024];           // The filename where we stream
	BOOL     m_bReadOnly;
	BOOL     m_bDelay;
	__int64 m_fileSize;
	__int64 m_infoFileSize;
	__int64 m_fileStartPos;

	BOOL     m_bDebugOutput;
};

#endif
