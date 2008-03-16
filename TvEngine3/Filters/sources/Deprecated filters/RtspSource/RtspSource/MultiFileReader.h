/**
*  MultiFileReader.h
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

#ifndef MULTIFILEREADER
#define MULTIFILEREADER

#include "FileReader.h"
#include <vector>

class MultiFileReaderFile
{
public:
	char filename[1024];
	__int64 startPosition;
	__int64 length;
	long filePositionId;
};

class MultiFileReader : public FileReader
{
public:

	MultiFileReader();
	virtual ~MultiFileReader();

	virtual FileReader* CreateFileReader();

	virtual int GetFileName(char *lpszFileName);
	virtual int SetFileName(char *pszFileName);
	virtual int OpenFile();
	virtual int CloseFile();
	virtual int Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes);
	virtual int Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual int get_ReadOnly(WORD *ReadOnly);
	virtual int set_DelayMode(WORD DelayMode);
	virtual int get_DelayMode(WORD *DelayMode);
	virtual int get_ReaderMode(WORD *ReaderMode);
	virtual DWORD setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 getFilePointer();

	//TODO: GetFileSize should go since get_FileSize should do the same thing.
	virtual int GetFileSize(__int64 *pStartPosition, __int64 *pLength);

	virtual BOOL IsFileInvalid();

	virtual DWORD SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 GetFilePointer();
	virtual __int64 GetFileSize();

protected:
	int RefreshTSBufferFile();
	int GetFileLength(char* pFilename, __int64 &length);
  void RefreshFileSize();

	FileReader m_TSBufferFile;
	__int64 m_startPosition;
	__int64 m_endPosition;
	__int64 m_currentPosition;
	long m_filesAdded;
	long m_filesRemoved;

	std::vector<MultiFileReaderFile *> m_tsFiles;

	FileReader m_TSFile;
	long	 m_TSFileId;
	BOOL     m_bReadOnly;
	BOOL     m_bDelay;
  __int64  m_cachedFileSize;

};

#endif
