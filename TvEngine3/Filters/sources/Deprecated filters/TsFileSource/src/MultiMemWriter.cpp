/**
*  MultiMemWriter.cpp
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
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include "stdafx.h"
#include "MultiMemWriter.h"
#include <atlbase.h>
#include <windows.h>
#include <stdio.h>

MultiMemWriter::MultiMemWriter(SharedMemory *pSharedMemory, MultiMemWriterParam *pWriterParams) :
	m_pSharedMemory(pSharedMemory),
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
	m_pCurrentTSFile = new MemWriter(m_pSharedMemory);
	m_pCurrentTSFile->SetChunkReserve(TRUE, m_chunkReserve, m_maxTSFileSize);
}

MultiMemWriter::~MultiMemWriter()
{
	CloseFile();
	if (m_pTSBufferFileName)
		delete[] m_pTSBufferFileName;

	if (m_pTSRegFileName)
		delete[] m_pTSRegFileName;

	if (m_pCurrentTSFile)
		delete[] m_pCurrentTSFile;
}

HRESULT MultiMemWriter::GetFileName(LPOLESTR *lpszFileName)
{
	*lpszFileName = m_pTSBufferFileName;
	return S_OK;
}

HRESULT MultiMemWriter::OpenFile(LPCWSTR pszFileName)
{
	//USES_CONVERSION;

	// Is the file already opened
	if (m_hTSBufferFile != INVALID_HANDLE_VALUE)
	{
		return E_FAIL;
	}

	// Is this a valid filename supplied
	CheckPointer(pszFileName,E_POINTER);

	if(wcslen(pszFileName) > MAX_PATH)
		return ERROR_FILENAME_EXCED_RANGE;

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
		return E_FAIL;

	TCHAR *pFileName = NULL;

	// Try to open the file
	m_hTSBufferFile = m_pSharedMemory->CreateFile(m_pTSBufferFileName,  // The filename
													 (DWORD) GENERIC_WRITE,             // File access
													 (DWORD) FILE_SHARE_READ,           // Share access
													 NULL,                      // Security
													 (DWORD) CREATE_ALWAYS,             // Open flags
													 (DWORD) 0,                 // More flags
													 NULL);                     // Template

	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
	{
        DWORD dwErr = GetLastError();
        return HRESULT_FROM_WIN32(dwErr);
	}

	return S_OK;

}

//
// CloseFile
//
HRESULT MultiMemWriter::CloseFile()
{
	CAutoLock lock(&m_Lock);

	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
	{
		return S_OK;
	}

	m_pSharedMemory->CloseHandle(m_hTSBufferFile);
	m_hTSBufferFile = INVALID_HANDLE_VALUE;

	m_pCurrentTSFile->CloseFile();

	CleanupFiles();

	return S_OK;
}

HRESULT MultiMemWriter::GetFileSize(__int64 *lpllsize)
{
	if (m_hTSBufferFile == INVALID_HANDLE_VALUE)
		*lpllsize = 0;
	else
		*lpllsize = max(0, (__int64)(((__int64)(m_filesAdded - m_filesRemoved - 1) * m_maxTSFileSize) + m_pCurrentTSFile->GetFilePointer()));

	return S_OK;
}

HRESULT MultiMemWriter::Write(PBYTE pbData, ULONG lDataLength)
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
		::OutputDebugString(TEXT("Creating first file\n"));
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
		lDataLength -= (ULONG)dataToWrite;
		return Write(pbData, lDataLength);
	}
	else
	{
		m_pCurrentTSFile->Write(pbData, lDataLength);
	}

	WriteTSBufferFile();
	
	return S_OK;
}

HRESULT MultiMemWriter::PrepareTSFile()
{
	USES_CONVERSION;
	HRESULT hr;

	::OutputDebugString(TEXT("PrepareTSFile()\n"));

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
		if ((long)m_tsFileNames.size() >= m_minTSFiles) 
		{
			if FAILED(hr = ReuseTSFile())
			{
				if ((long)m_tsFileNames.size() < m_maxTSFiles)
				{
					if (hr != 0x80070020) // ERROR_SHARING_VIOLATION
						::OutputDebugString(TEXT("Failed to reopen old file. Unexpected reason. Trying to create a new file.\n"));

					hr = CreateNewTSFile();
				}
				else
				{
					if (hr != 0x80070020) // ERROR_SHARING_VIOLATION
						::OutputDebugString(TEXT("Failed to reopen old file. Unexpected reason. Dropping data!\n"));
					else
						::OutputDebugString(TEXT("Failed to reopen old file. It's currently in use. Dropping data!\n"));

//					Sleep(500);
					m_pSharedMemory->SetBuffOvld(TRUE);
				}
			}
			else
				m_pSharedMemory->SetBuffOvld(FALSE);
		}	
		else
		{
			hr = CreateNewTSFile();
		}
	}

	return hr;
}

HRESULT MultiMemWriter::CreateNewTSFile()
{
	USES_CONVERSION;
	HRESULT hr;

	LPWSTR pFilename = new wchar_t[MAX_PATH];
	WIN32_FIND_DATAW findData;
	HANDLE handleFound = INVALID_HANDLE_VALUE;

	while (TRUE)
	{
		// Create new filename
		m_currentFilenameId++;
		swprintf(pFilename, L"%s%i.ts", m_pTSBufferFileName, m_currentFilenameId);

		// Check if file already exists
		handleFound = m_pSharedMemory->FindFirstFile(pFilename, &findData);
		if (handleFound == INVALID_HANDLE_VALUE)
			break;

		::OutputDebugString(TEXT("Newly generated filename already exists.\n"));

		// If it exists we loop and try the next number
		m_pSharedMemory->FindClose(handleFound);
	}
	
	if FAILED(hr = m_pCurrentTSFile->SetFileName(pFilename))
	{
		::OutputDebugString(TEXT("Failed to set filename for new file.\n"));
		delete[] pFilename;
		return hr;
	}

	if FAILED(hr = m_pCurrentTSFile->OpenFile())
	{
		::OutputDebugString(TEXT("Failed to open new file\n"));
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

	return S_OK;
}

HRESULT MultiMemWriter::ReuseTSFile()
{
	USES_CONVERSION;
	HRESULT hr;

	LPWSTR pFilename = m_tsFileNames.at(0);

	if FAILED(hr = m_pCurrentTSFile->SetFileName(pFilename))
	{
		::OutputDebugString(TEXT("Failed to set filename to reuse old file\n"));
		return hr;
	}

	int count = 50;
	while (count > 0)
	{
		// Check if file is being read by something.
		if (IsFileLocked(pFilename) != TRUE)
		{
			//TCHAR sz[MAX_PATH];
			//sprintf(sz, "%S", pFilename);
			if (!m_pSharedMemory->DeleteFile(pFilename))
				continue;

			// Check if we are above the minimun buffer size
			if ((long)m_tsFileNames.size() > m_minTSFiles)
			{
				// Check if the next file is being read by something.
				pFilename = m_tsFileNames.at(1);
				if (IsFileLocked(pFilename) != TRUE)
				{
					//If the next file is free then drop the first excess file and set the next to be re-used 
					//sprintf(sz, "%S", pFilename);
					if (m_pSharedMemory->DeleteFile(pFilename))
					{
						// if deleted ok then then delete the first on the list and move to the next filename
						pFilename = m_tsFileNames.at(0);
						//sprintf(sz, "%S", pFilename);
						delete[] pFilename;
						m_tsFileNames.erase(m_tsFileNames.begin());
						m_filesRemoved++;
					}

					// if move to the next filename
					pFilename = m_tsFileNames.at(0);
					if FAILED(hr = m_pCurrentTSFile->SetFileName(pFilename))
					{
						::OutputDebugString(TEXT("Failed to set filename to reuse old file\n"));
						return hr;
					}
				}
				else //restore file name if were only able to re-use the first file in the list
					pFilename = m_tsFileNames.at(0);
			}
			break;
		}
		Sleep(10);
		count--;
	};


/*
	// Check if file is being read by something.
	if (IsFileLocked(pFilename) != TRUE)
	{
		TCHAR sz[MAX_PATH];
		sprintf(sz, "%S", pFilename);
		m_pSharedMemory->DeleteFile(sz);
	}
*/
	if FAILED(hr = m_pCurrentTSFile->OpenFile())
	{
		return hr;
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

	return S_OK;
}


HRESULT MultiMemWriter::WriteTSBufferFile()
{
	LARGE_INTEGER li;
	DWORD written = 0;

	// Move to the start of the file
	li.QuadPart = 0;
	m_pSharedMemory->SetFilePointer(m_hTSBufferFile, li.LowPart, &li.HighPart, FILE_BEGIN);

	// Write current position of most recent file.
	__int64 currentPointer = m_pCurrentTSFile->GetFilePointer();
	m_pSharedMemory->WriteFile(m_hTSBufferFile, &currentPointer, sizeof(currentPointer), &written, NULL);

	// Write filesAdded and filesRemoved values
	m_pSharedMemory->WriteFile(m_hTSBufferFile, &m_filesAdded, sizeof(m_filesAdded), &written, NULL);
	m_pSharedMemory->WriteFile(m_hTSBufferFile, &m_filesRemoved, sizeof(m_filesRemoved), &written, NULL);

	// Write out all the filenames (null terminated)
	std::vector<LPWSTR>::iterator it = m_tsFileNames.begin();
	for ( ; it < m_tsFileNames.end() ; it++ )
	{
		LPWSTR pFilename = *it;
		long length = wcslen(pFilename)+1;
		length *= sizeof(wchar_t);
		m_pSharedMemory->WriteFile(m_hTSBufferFile, pFilename, length, &written, NULL);
	}

	// Finish up with a unicode null character in case we want to put stuff after this in the future.
	wchar_t temp = 0;
	m_pSharedMemory->WriteFile(m_hTSBufferFile, &temp, sizeof(temp), &written, NULL);

	//randomly park the file pointer to help minimise HDD clogging
//	if(m_pCurrentTSFile && m_pCurrentTSFile->GetFilePointer()&1)
		m_pSharedMemory->SetFilePointer(m_hTSBufferFile, 0, NULL, FILE_END);
//	else
//		m_pSharedMemory->SetFilePointer(m_hTSBufferFile, 0, NULL, FILE_BEGIN);

	return S_OK;
}

HRESULT MultiMemWriter::CleanupFiles()
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
			return S_OK;
		}
	}

	// Now we know we can delete all the files.

	for (it = m_tsFileNames.begin() ; it < m_tsFileNames.end() ; it++ )
	{
		if (m_pSharedMemory->DeleteFile(*it) == FALSE)
		{
			wchar_t msg[MAX_PATH];
			swprintf((LPWSTR)&msg, L"Failed to delete file %s : 0x%x\n", *it, GetLastError());
			::OutputDebugString(W2T((LPWSTR)&msg));
		}
		delete[] *it;
	}
	m_tsFileNames.clear();

	if (m_pSharedMemory->DeleteFile(m_pTSBufferFileName) == FALSE)
	{
		wchar_t msg[MAX_PATH];
		swprintf((LPWSTR)&msg, L"Failed to delete tsbuffer file : 0x%x\n", GetLastError());
		::OutputDebugString(W2T((LPWSTR)&msg));
	}
	m_filesAdded = 0;
	m_filesRemoved = 0;
	m_currentFilenameId = 0;
	m_currentFileId = 0;
	return S_OK;
}

BOOL MultiMemWriter::IsFileLocked(LPWSTR pFilename)
{
	//USES_CONVERSION;

	HANDLE hFile;
	hFile = m_pSharedMemory->CreateFile(pFilename,        // The filename
											   (DWORD) GENERIC_READ,          // File access
											   (DWORD) NULL,                  // Share access
											   NULL,                  // Security
											   (DWORD) OPEN_EXISTING,         // Open flags
											   (DWORD) 0,             // More flags
											   NULL);                 // Template

	if (hFile == INVALID_HANDLE_VALUE)
		return TRUE;

	m_pSharedMemory->CloseHandle(hFile);
	return FALSE;
}

HRESULT MultiMemWriter::GetAvailableDiskSpace(__int64* llAvailableDiskSpace)
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
	hr = m_pSharedMemory->GetDiskFreeSpaceEx(pszDrive, &uliDiskSpaceAvailable, &uliDiskSpaceTotal, &uliDiskSpaceFree);
	if SUCCEEDED(hr)
		*llAvailableDiskSpace = uliDiskSpaceAvailable.QuadPart;
	else
		*llAvailableDiskSpace = 0;

	return hr;
}

LPTSTR MultiMemWriter::getRegFileName(void)
{
	return 	m_pTSRegFileName;
}

void MultiMemWriter::setRegFileName(LPTSTR fileName)
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

LPWSTR MultiMemWriter::getBufferFileName(void)
{
	return 	m_pTSBufferFileName;
}

void MultiMemWriter::setBufferFileName(LPWSTR fileName)
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

MemWriter* MultiMemWriter::getCurrentTSFile(void)
{
	return m_pCurrentTSFile;
}

long MultiMemWriter::getNumbFilesAdded(void)
{
	return m_filesAdded;
}

long MultiMemWriter::getNumbFilesRemoved(void)
{
	return m_filesRemoved;
}

long MultiMemWriter::getCurrentFileId(void)
{
	return m_currentFileId;//m_currentFilenameId;
}

long MultiMemWriter::getMinTSFiles(void)
{
	return m_minTSFiles;
}

void MultiMemWriter::setMinTSFiles(long minFiles)
{
	m_minTSFiles = minFiles;
}

long MultiMemWriter::getMaxTSFiles(void)
{
	return m_maxTSFiles;
}

void MultiMemWriter::setMaxTSFiles(long maxFiles)
{
	m_maxTSFiles = maxFiles;
}

__int64 MultiMemWriter::getMaxTSFileSize(void)
{
	return m_maxTSFileSize;
}

void MultiMemWriter::setMaxTSFileSize(__int64 maxSize)
{
	m_maxTSFileSize = maxSize;
	m_pCurrentTSFile->SetChunkReserve(TRUE, m_chunkReserve, m_maxTSFileSize);
}

__int64 MultiMemWriter::getChunkReserve(void)
{
	return m_chunkReserve;
}

void MultiMemWriter::setChunkReserve(__int64 chunkSize)
{
	m_chunkReserve = chunkSize;
	m_pCurrentTSFile->SetChunkReserve(TRUE, m_chunkReserve, m_maxTSFileSize);
}
