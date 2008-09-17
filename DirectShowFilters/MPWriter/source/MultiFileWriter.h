/**
*  MultiFileWriter.h
*  Copyright (C) 2006-2007      nate
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

#ifndef MULTIFILEWRITER
#define MULTIFILEWRITER

#include "FileWriter.h"
#include <vector>

class MultiFileWriter
{
public:
	MultiFileWriter();
	virtual ~MultiFileWriter();

	HRESULT GetFileName(LPWSTR *lpszFileName);
	HRESULT OpenFile(LPCWSTR pszFileName);
	HRESULT CloseFile();
	HRESULT GetFileSize(__int64 *lpllsize);
	
	HRESULT Write(PBYTE pbData, ULONG lDataLength);
	HRESULT GetAvailableDiskSpace(__int64* llAvailableDiskSpace);

	LPTSTR getRegFileName(void);
	void setRegFileName(LPTSTR fileName);
	LPWSTR getBufferFileName(void);
	void setBufferFileName(LPWSTR fileName);
	FileWriter* getCurrentTSFile(void);
	long getNumbFilesAdded(void);
	long getNumbFilesRemoved(void);
	long getCurrentFileId(void);
	long getMinTSFiles(void);
	void setMinTSFiles(long minFiles);
	long getMaxTSFiles(void);
	void setMaxTSFiles(long maxFiles);
	__int64 getMaxTSFileSize(void);
	void setMaxTSFileSize(__int64 maxSize);
	__int64 getChunkReserve(void);
	void setChunkReserve(__int64 chunkSize);

protected:
	HRESULT PrepareTSFile();
	HRESULT CreateNewTSFile();
	HRESULT ReuseTSFile();

	HRESULT WriteTSBufferFile();
	HRESULT CleanupFiles();
	BOOL IsFileLocked(LPWSTR pFilename);

	HANDLE m_hTSBufferFile;
	LPWSTR m_pTSBufferFileName;
	LPTSTR m_pTSRegFileName;

	CCritSec m_Lock;

	FileWriter *m_pCurrentTSFile;
	std::vector<LPWSTR> m_tsFileNames;
	long m_filesAdded;
	long m_filesRemoved;
	long m_currentFilenameId;
	long m_currentFileId;

	long m_minTSFiles;
	long m_maxTSFiles;
	__int64 m_maxTSFileSize;
	__int64 m_chunkReserve;
};

#endif
