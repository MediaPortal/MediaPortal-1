/**
*  MultiFileWriter.cpp
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

#pragma warning(disable : 4995)
#pragma warning(disable : 4996)
#include <streams.h>
#include "MultiFileWriter.h"
#include <atlbase.h>
#include <windows.h>
#include <stdio.h>
extern void LogDebug(const char *fmt, ...) ;

MultiFileWriter::MultiFileWriter(MultiFileWriterParam *pWriterParams) :
	m_hTSBufferFile(INVALID_HANDLE_VALUE),
	m_pTSBufferFileName(NULL),
	m_pTSRegFileName(NULL),
	m_pCurrentTSFile(NULL),
	m_filesAdded(0),
	m_filesRemoved(0),
	m_currentFilenameId(0),
	m_currentFileId(0),
	m_minTSFiles(pWriterParams->minFiles),
	m_maxTSFiles(pWriterParams->maxFiles),
	m_maxTSFileSize(pWriterParams->maxSize),	
	m_chunkReserve(pWriterParams->chunkSize) 
{
	m_pCurrentTSFile = new FileWriter();
	m_pCurrentTSFile->SetChunkReserve(TRUE, m_chunkReserve, m_maxTSFileSize);
}

MultiFileWriter::~MultiFileWriter()
{
	CloseFile();
	if (m_pTSBufferFileName)
		delete[] m_pTSBufferFileName;

	if (m_pTSRegFileName)
		delete[] m_pTSRegFileName;

	if (m_pCurrentTSFile)
		delete[] m_pCurrentTSFile;
}

HRESULT MultiFileWriter::GetFileName(LPOLESTR *lpszFileName)
{
	*lpszFileName = m_pTSBufferFileName;
	return S_OK;
}

HRESULT MultiFileWriter::OpenFile(LPCWSTR pszFileName)
{
	USES_CONVERSION;

	// Is the file already opened
	if (m_hTSBufferFile != INVALID_HANDLE_VALUE)
	{
		return E_FAIL;
	}

	// Is this a valid filename supplied
	CheckPointer(pszFileName,E_POINTER);

	if(wcslen(pszFileName) > MAX_PATH)
  {
    LogDebug("MultiFileWriter: filename too long");
		return ERROR_FILENAME_EXCED_RANGE;
  }
	// Take a copy of the filename
	if (m_pTSBufferFileName)
	{
		delete[] m_pTSBufferFileName;
		m_pTSBufferFileName = NULL;
	}
	m_pTSBufferFileName = new WCHAR[1+lstrlenW(pszFileName)];
	if (m_pTSBufferFileName == NULL)
		return E_OUTOFMEMORY;
	wcscpy(m_pTSBufferFileName, pszFileName);
	
	//check disk space first
	__int64 llDiskSpaceAvailable = 0;
	if (SUCCEEDED(GetAvailableDiskSpace(&llDiskSpaceAvailable)) && (__int64)llDiskSpaceAvailable < (__int64)(m_maxTSFileSize*2))
  {
    LogDebug("MultiFileWriter: not enough free diskspace");
		return E_FAIL;
  }
	TCHAR *pFileName = NULL;

	// Try to open the file
	m_hTSBufferFile = CreateFile(W2T(m_pTSBufferFileName),  // The filename
								 (DWORD) GENERIC_WRITE,             // File access
								 (DWORD) FILE_SHARE_READ,           // Share access
								 NULL,                      // Security
								 (DWORD) CREATE_ALWAYS,             // Open flags
								 (DWORD) 0,                 // More flags
								 NULL);                     // Template

	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
	{
        LogDebug("MultiFileWriter: fail to create buffer file");
        DWORD dwErr = GetLastError();
        return HRESULT_FROM_WIN32(dwErr);
	}

	return S_OK;

}

//
// CloseFile
//
HRESULT MultiFileWriter::CloseFile()
{
	CAutoLock lock(&m_Lock);

	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
	{
		return S_OK;
	}

	CloseHandle(m_hTSBufferFile);
	m_hTSBufferFile = INVALID_HANDLE_VALUE;

	m_pCurrentTSFile->CloseFile();

	CleanupFiles();

	return S_OK;
}

HRESULT MultiFileWriter::GetFileSize(__int64 *lpllsize)
{
	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
		*lpllsize = 0;
	else
		*lpllsize = max(0, (__int64)(((__int64)(m_filesAdded - m_filesRemoved - 1) * m_maxTSFileSize) + m_pCurrentTSFile->GetFilePointer()));

	return S_OK;
}

HRESULT MultiFileWriter::Write(PBYTE pbData, ULONG lDataLength)
{
	HRESULT hr;

	CheckPointer(pbData,E_POINTER);
	if (lDataLength == 0)
		return S_OK;

	// If the file has already been closed, don't continue
	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
		return S_FALSE;

	if (m_pCurrentTSFile->IsFileInvalid())
	{
		//::LogDebug("Creating first file");
		if FAILED(hr = PrepareTSFile())
			return hr;
	}

	//Get File Position
	__int64 filePosition = m_pCurrentTSFile->GetFilePointer();

	// See if we will need to create more ts files.
	if (filePosition + lDataLength > m_maxTSFileSize)
	{
		__int64 dataToWrite = m_maxTSFileSize - filePosition;

		// Write some data to the current file if it's not full
		if (dataToWrite > 0)
		{
			m_pCurrentTSFile->Write(pbData, (ULONG)dataToWrite);
		}

		// Try to create a new file
		if FAILED(hr = PrepareTSFile())
		{
			// Buffer is probably full and the oldest file is still locked.
			// We'll just start dropping data
			return hr;
		}

		// Try writing the remaining data now that a new file has been created.
		pbData += dataToWrite;
		lDataLength -= dataToWrite;
		return Write(pbData, lDataLength);
	}
	else
	{
		m_pCurrentTSFile->Write(pbData, lDataLength);
	}

	WriteTSBufferFile();
	
	return S_OK;
}

HRESULT MultiFileWriter::PrepareTSFile()
{
	USES_CONVERSION;
	HRESULT hr;

	//LogDebug("PrepareTSFile()");

//	m_pCurrentTSFile->FlushFile())

	// Make sure the old file is closed
	m_pCurrentTSFile->CloseFile();


	//TODO: disk space stuff
	/*
	if (m_diskSpaceLimit > 0)
	{
		diskSpaceAvailable = WhateverFunctionDoesThat();
		if (diskSpaceAvailable < m_diskSpaceLimit)
		{
			hr = ReuseTSFile();
		}
		else
		{
			hr = CreateNewTSFile();
		}
	}
	else */

	__int64 llDiskSpaceAvailable = 0;
	if (SUCCEEDED(GetAvailableDiskSpace(&llDiskSpaceAvailable)) && (__int64)llDiskSpaceAvailable < (__int64)(m_maxTSFileSize*2))
	{
		hr = ReuseTSFile();
	}
	else
	{
		if (m_tsFileNames.size() >= (UINT)m_minTSFiles) 
		{
			if FAILED(hr = ReuseTSFile())
			{
				if (m_tsFileNames.size() < (UINT)m_maxTSFiles)
				{
					if (hr != 0x80070020) // ERROR_SHARING_VIOLATION
						LogDebug("Failed to reopen old file. Unexpected reason. Trying to create a new file.");

					hr = CreateNewTSFile();
				}
				else
				{
					if (hr != 0x80070020) // ERROR_SHARING_VIOLATION
						LogDebug("Failed to reopen old file. Unexpected reason. Dropping data!");
					else
						LogDebug("Failed to reopen old file. It's currently in use. Dropping data!");

					Sleep(500);
				}
			}
		}	
		else
		{
			hr = CreateNewTSFile();
		}
	}

	return hr;
}

HRESULT MultiFileWriter::CreateNewTSFile()
{
	USES_CONVERSION;
	HRESULT hr;

	LPWSTR pFilename = new wchar_t[MAX_PATH];
	WIN32_FIND_DATA findData;
	HANDLE handleFound = INVALID_HANDLE_VALUE;

	//LogDebug("CreateNewTSFile.");
	while (TRUE)
	{
		// Create new filename
		m_currentFilenameId++;
		swprintf(pFilename, L"%s%i.ts", m_pTSBufferFileName, m_currentFilenameId);

		// Check if file already exists
		handleFound = FindFirstFile(W2T(pFilename), &findData);
		if (handleFound == INVALID_HANDLE_VALUE)
			break;

		//LogDebug("Newly generated filename already exists.");

		// If it exists we loop and try the next number
		FindClose(handleFound);
	}
	
	if FAILED(hr = m_pCurrentTSFile->SetFileName(pFilename))
	{
		LogDebug("Failed to set filename for new file.");
		delete[] pFilename;
		return hr;
	}

	if FAILED(hr = m_pCurrentTSFile->OpenFile())
	{
		LogDebug("Failed to open new file");
		delete[] pFilename;
		return hr;
	}

	m_tsFileNames.push_back(pFilename);
	m_filesAdded++;

	LPWSTR pos = pFilename + wcslen(m_pTSBufferFileName);
	if (pos)
		m_currentFileId = _wtoi(pos);
	wchar_t msg[MAX_PATH];
	swprintf((LPWSTR)&msg, L"New file created : %s\n", pFilename);
	::OutputDebugString(W2T((LPWSTR)&msg));

	//LogDebug("new file created");
	return S_OK;
}

HRESULT MultiFileWriter::ReuseTSFile()
{
	USES_CONVERSION;
	HRESULT hr;
  DWORD Tmo=5 ;

	LPWSTR pFilename = m_tsFileNames.at(0);

	if FAILED(hr = m_pCurrentTSFile->SetFileName(pFilename))
	{
		LogDebug("Failed to set filename to reuse old file");
		return hr;
	}

	// Check if file is being read by something.
  TCHAR sz[MAX_PATH];
  sprintf(sz, "%S", pFilename);
  // Can be locked temporarily to update duration or definitely (!) if timeshift is paused.
	do
	{
		DeleteFile(sz) ;	// Stupid function, return can be ok and file not deleted ( just tagged for deleting )!!!!
		hr = m_pCurrentTSFile->OpenFile() ;
		if (!FAILED(hr)) break ;
		Sleep(20) ;
	}
	while (--Tmo) ;

  if (Tmo)
  {
    if (Tmo<4) // 1 failed + 1 succeded is quasi-normal, more is a bit suspicious ( disk drive too slow or problem ? )
			LogDebug("MultiFileWriter: %d tries to succeed deleting and re-opening %ws.", 6-Tmo, pFilename);
  }
  else
	{
    LogDebug("MultiFileWriter: failed to create file %ws",pFilename);
		return hr ;
	}

	// if stuff worked then move the filename to the end of the files list
	m_tsFileNames.erase(m_tsFileNames.begin());
	m_filesRemoved++;

	m_tsFileNames.push_back(pFilename);
	m_filesAdded++;

	LPWSTR pos = pFilename + wcslen(m_pTSBufferFileName);
	if (pos)
		m_currentFileId = _wtoi(pos);
	wchar_t msg[MAX_PATH];
	swprintf((LPWSTR)&msg, L"Old file reused : %s\n", pFilename);
	::OutputDebugString(W2T((LPWSTR)&msg));

	//LogDebug("reuse old file");
	return S_OK;
}


HRESULT MultiFileWriter::WriteTSBufferFile()
{
	LARGE_INTEGER li;
	DWORD written = 0;

	// Move to the start of the file
	li.QuadPart = 0;
	SetFilePointer(m_hTSBufferFile, li.LowPart, &li.HighPart, FILE_BEGIN);

	// Write current position of most recent file.
	__int64 currentPointer = m_pCurrentTSFile->GetFilePointer();
	WriteFile(m_hTSBufferFile, &currentPointer, sizeof(currentPointer), &written, NULL);

	// Write filesAdded and filesRemoved values
	WriteFile(m_hTSBufferFile, &m_filesAdded, sizeof(m_filesAdded), &written, NULL);
	WriteFile(m_hTSBufferFile, &m_filesRemoved, sizeof(m_filesRemoved), &written, NULL);

	// Write out all the filenames (null terminated)
	std::vector<LPWSTR>::iterator it = m_tsFileNames.begin();
	for ( ; it < m_tsFileNames.end() ; it++ )
	{
		LPWSTR pFilename = *it;
		long length = wcslen(pFilename)+1;
		length *= sizeof(wchar_t);
		WriteFile(m_hTSBufferFile, pFilename, length, &written, NULL);
	}

	// Finish up with a unicode null character in case we want to put stuff after this in the future.
	wchar_t temp = 0;
	WriteFile(m_hTSBufferFile, &temp, sizeof(temp), &written, NULL);

	// Write again filesAdded and filesRemoved values for integrity check.
	WriteFile(m_hTSBufferFile, &m_filesAdded, sizeof(m_filesAdded), &written, NULL);
	WriteFile(m_hTSBufferFile, &m_filesRemoved, sizeof(m_filesRemoved), &written, NULL);

	//randomly park the file pointer to help minimise HDD clogging
//	if(m_pCurrentTSFile && m_pCurrentTSFile->GetFilePointer()&1)
		SetFilePointer(m_hTSBufferFile, 0, NULL, FILE_END);
//	else
//		SetFilePointer(m_hTSBufferFile, 0, NULL, FILE_BEGIN);

	return S_OK;
}

HRESULT MultiFileWriter::CleanupFiles()
{
	USES_CONVERSION;

	m_filesAdded = 0;
	m_filesRemoved = 0;
	m_currentFilenameId = 0;
	m_currentFileId = 0;

	// Check if .tsbuffer file is being read by something.
	if (IsFileLocked(m_pTSBufferFileName) == TRUE)
		return S_OK;

	std::vector<LPWSTR>::iterator it;
	for (it = m_tsFileNames.begin() ; it < m_tsFileNames.end() ; it++ )
	{
		if (IsFileLocked(*it) == TRUE)
		{
			// If any of the files are being read then we won't
			// delete any so that the full buffer stays intact.
			wchar_t msg[MAX_PATH];
			swprintf((LPWSTR)&msg, L"CleanupFiles: A file is still locked : %s\n", *it);
			::OutputDebugString(W2T((LPWSTR)&msg));
			LogDebug("CleanupFiles: A file is still locked");
			return S_OK;
		}
	}

	// Now we know we can delete all the files.

	for (it = m_tsFileNames.begin() ; it < m_tsFileNames.end() ; it++ )
	{
		if (DeleteFile(W2T(*it)) == FALSE)
		{
			wchar_t msg[MAX_PATH];
			swprintf((LPWSTR)&msg, L"Failed to delete file %s : 0x%x\n", *it, GetLastError());
			::OutputDebugString(W2T((LPWSTR)&msg));
			LogDebug("CleanupFiles: Failed to delete file");
		}
		delete[] *it;
	}
	m_tsFileNames.clear();

	if (DeleteFile(W2T(m_pTSBufferFileName)) == FALSE)
	{
		wchar_t msg[MAX_PATH];
		swprintf((LPWSTR)&msg, L"Failed to delete tsbuffer file : 0x%x\n", GetLastError());
		::OutputDebugString(W2T((LPWSTR)&msg));
			LogDebug("CleanupFiles: Failed to delete tsbuffer file: 0x%x\n", GetLastError());

	}
	m_filesAdded = 0;
	m_filesRemoved = 0;
	m_currentFilenameId = 0;
	m_currentFileId = 0;
	return S_OK;
}

BOOL MultiFileWriter::IsFileLocked(LPWSTR pFilename)
{
	USES_CONVERSION;

	HANDLE hFile;
	hFile = CreateFile(W2T(pFilename),        // The filename
					   (DWORD) GENERIC_READ,          // File access
					   (DWORD) NULL,                  // Share access
					   NULL,                  // Security
					   (DWORD) OPEN_EXISTING,         // Open flags
					   (DWORD) 0,             // More flags
					   NULL);                 // Template

	if (hFile == INVALID_HANDLE_VALUE)
		return TRUE;

	CloseHandle(hFile);
	return FALSE;
}

HRESULT MultiFileWriter::GetAvailableDiskSpace(__int64* llAvailableDiskSpace)
{
	if (!llAvailableDiskSpace)
		return E_INVALIDARG;

	HRESULT hr;

	char	*pszDrive = NULL;
	char	szDrive[4];
	if (m_pTSBufferFileName[1] == ':')
	{
		szDrive[0] = (char)m_pTSBufferFileName[0];
		szDrive[1] = ':';
		szDrive[2] = '\\';
		szDrive[3] = '\0';
		pszDrive = szDrive;
	}

	ULARGE_INTEGER uliDiskSpaceAvailable;
	ULARGE_INTEGER uliDiskSpaceTotal;
	ULARGE_INTEGER uliDiskSpaceFree;
	uliDiskSpaceAvailable.QuadPart= 0;
	uliDiskSpaceTotal.QuadPart= 0;
	uliDiskSpaceFree.QuadPart= 0;
	hr = GetDiskFreeSpaceEx(pszDrive, &uliDiskSpaceAvailable, &uliDiskSpaceTotal, &uliDiskSpaceFree);
	if SUCCEEDED(hr)
		*llAvailableDiskSpace = uliDiskSpaceAvailable.QuadPart;
	else
		*llAvailableDiskSpace = 0;

	return hr;
}

LPTSTR MultiFileWriter::getRegFileName(void)
{
	return 	m_pTSRegFileName;
}

void MultiFileWriter::setRegFileName(LPTSTR fileName)
{
//	CheckPointer(fileName,E_POINTER);

	if(strlen(fileName) > MAX_PATH)
		return;// ERROR_FILENAME_EXCED_RANGE;

	// Take a copy of the filename
	if (m_pTSRegFileName)
	{
		delete[] m_pTSRegFileName;
		m_pTSRegFileName = NULL;
	}
	m_pTSRegFileName = new TCHAR[1+lstrlen(fileName)];
	if (m_pTSRegFileName == NULL)
		return;// E_OUTOFMEMORY;

	lstrcpy(m_pTSRegFileName, fileName);
}

LPWSTR MultiFileWriter::getBufferFileName(void)
{
	return 	m_pTSBufferFileName;
}

void MultiFileWriter::setBufferFileName(LPWSTR fileName)
{
//	CheckPointer(fileName,E_POINTER);

	if(wcslen(fileName) > MAX_PATH)
		return;// ERROR_FILENAME_EXCED_RANGE;

	// Take a copy of the filename
	if (m_pTSBufferFileName)
	{
		delete[] m_pTSBufferFileName;
		m_pTSBufferFileName = NULL;
	}
	m_pTSBufferFileName = new WCHAR[1+lstrlenW(fileName)];
	if (m_pTSBufferFileName == NULL)
		return;// E_OUTOFMEMORY;

	wcscpy(m_pTSBufferFileName, fileName);
}

FileWriter* MultiFileWriter::getCurrentTSFile(void)
{
	return m_pCurrentTSFile;
}

long MultiFileWriter::getNumbFilesAdded(void)
{
	return m_filesAdded;
}

long MultiFileWriter::getNumbFilesRemoved(void)
{
	return m_filesRemoved;
}

long MultiFileWriter::getCurrentFileId(void)
{
	return m_currentFileId;//m_currentFilenameId;
}

long MultiFileWriter::getMinTSFiles(void)
{
	return m_minTSFiles;
}

void MultiFileWriter::setMinTSFiles(long minFiles)
{
	m_minTSFiles = minFiles;
}

long MultiFileWriter::getMaxTSFiles(void)
{
	return m_maxTSFiles;
}

void MultiFileWriter::setMaxTSFiles(long maxFiles)
{
	m_maxTSFiles = maxFiles;
}

__int64 MultiFileWriter::getMaxTSFileSize(void)
{
	return m_maxTSFileSize;
}

void MultiFileWriter::setMaxTSFileSize(__int64 maxSize)
{
	m_maxTSFileSize = maxSize;
	m_pCurrentTSFile->SetChunkReserve(TRUE, m_chunkReserve, m_maxTSFileSize);
}

__int64 MultiFileWriter::getChunkReserve(void)
{
	return m_chunkReserve;
}

void MultiFileWriter::setChunkReserve(__int64 chunkSize)
{
	m_chunkReserve = chunkSize;
	m_pCurrentTSFile->SetChunkReserve(TRUE, m_chunkReserve, m_maxTSFileSize);
}

void MultiFileWriter::GetPosition(__int64 * position)
{
	if (m_pCurrentTSFile==NULL)
	{
		*position=-1;
		return;
	}
	if (m_pCurrentTSFile->IsFileInvalid())
	{
		*position=-1;
		return;
	}
	*position=m_pCurrentTSFile->GetFilePointer();
}