/**
*  MultiMemReader.cpp
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
#include "MultiMemReader.h"
#include <atlbase.h>

MultiMemReader::MultiMemReader(SharedMemory* pSharedMemory) :
	FileReader(pSharedMemory),
	m_TSBufferFile(pSharedMemory),
	m_TSFile(pSharedMemory),
	m_pSharedMemory(pSharedMemory) 
{
	m_startPosition = 0;
	m_endPosition = 0;
	m_currentPosition = 0;
	m_filesAdded = 0;
	m_filesRemoved = 0;
	m_TSFileId = 0;
	m_bReadOnly = 1;
	m_bDelay = 0;
	m_llBufferPointer = 0;	
}

MultiMemReader::~MultiMemReader()
{
	//CloseFile called by ~FileReader
/*	USES_CONVERSION;

	std::vector<MultiMemReaderFile *>::iterator it = m_tsFiles.begin();
	for ( ; it < m_tsFiles.end() ; it++ )
	{
		if((*it)->filename)
		{
			DeleteFile(W2T((*it)->filename));
			delete[] (*it)->filename;
		}

		delete *it;
	};
*/
}

FileReader* MultiMemReader::CreateFileReader()
{
	return (FileReader *)new MultiMemReader(m_pSharedMemory);
}

HRESULT MultiMemReader::GetFileName(LPOLESTR *lpszFileName)
{
	CheckPointer(lpszFileName,E_POINTER);
	return m_TSBufferFile.GetFileName(lpszFileName);
}

HRESULT MultiMemReader::SetFileName(LPCOLESTR pszFileName)
{
	CheckPointer(pszFileName,E_POINTER);
	return m_TSBufferFile.SetFileName(pszFileName);
}

//
// OpenFile
//
HRESULT MultiMemReader::OpenFile()
{
	HRESULT hr = m_TSBufferFile.OpenFile();

	RefreshTSBufferFile();

	m_currentPosition = 0;
	m_llBufferPointer = 0;	

	return hr;
}

//
// CloseFile
//
HRESULT MultiMemReader::CloseFile()
{
	HRESULT hr;
	hr = m_TSBufferFile.CloseFile();
	hr = m_TSFile.CloseFile();
	m_TSFileId = 0;
	m_llBufferPointer = 0;	
	return hr;
}

BOOL MultiMemReader::IsFileInvalid()
{
	return m_TSBufferFile.IsFileInvalid();
}

HRESULT MultiMemReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
//	RefreshTSBufferFile();
	CheckPointer(pStartPosition,E_POINTER);
	CheckPointer(pLength,E_POINTER);
	*pStartPosition = m_startPosition;
	*pLength = (__int64)(m_endPosition - m_startPosition);
	return S_OK;
}

DWORD MultiMemReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
//	RefreshTSBufferFile();

	if (dwMoveMethod == FILE_END)
	{
		m_currentPosition = m_endPosition + llDistanceToMove;
	}
	else if (dwMoveMethod == FILE_CURRENT)
	{
		m_currentPosition += llDistanceToMove;
	}
	else // if (dwMoveMethod == FILE_BEGIN)
	{
		m_currentPosition = llDistanceToMove;
	}

	if (m_currentPosition < m_startPosition)
		m_currentPosition = m_startPosition;

	if (m_currentPosition > m_endPosition)
		m_currentPosition = m_endPosition;

	RefreshTSBufferFile();
	return S_OK;
}

__int64 MultiMemReader::GetFilePointer()
{
//	RefreshTSBufferFile();
	return m_currentPosition;
}

HRESULT MultiMemReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	HRESULT hr;

	// If the file has already been closed, don't continue
	if (m_TSBufferFile.IsFileInvalid())
		return S_FALSE;

	RefreshTSBufferFile();

	if (m_currentPosition < m_startPosition)
		m_currentPosition = m_startPosition;

	// Find out which file the currentPosition is in.
	MultiMemReaderFile *file = NULL;
	std::vector<MultiMemReaderFile *>::iterator it = m_tsFiles.begin();
	for ( ; it < m_tsFiles.end() ; it++ )
	{
		file = *it;
		if (m_currentPosition < (file->startPosition + file->length))
			break;
	};

	if(!file)
		return S_FALSE;

	if (m_currentPosition < (file->startPosition + file->length))
	{
		if (m_TSFileId != file->filePositionId)
		{
			m_TSFile.CloseFile();
			m_TSFile.SetFileName(file->filename);
			m_TSFile.OpenFile();

			m_TSFileId = file->filePositionId;

			if (m_bDebugOutput)
			{
				USES_CONVERSION;
				TCHAR sz[MAX_PATH+128];
				wsprintf(sz, TEXT("Current File Changed to %s\n"), W2T(file->filename));
				::OutputDebugString(sz);
			}
		}

		__int64 seekPosition = m_currentPosition - file->startPosition;
		m_TSFile.SetFilePointer(seekPosition, FILE_BEGIN);

		ULONG bytesRead = 0;

		__int64 bytesToRead = file->length - seekPosition;
		if (lDataLength > bytesToRead)
		{
			hr = m_TSFile.Read(pbData, (ULONG)bytesToRead, &bytesRead);
			m_currentPosition += bytesToRead;

			hr = this->Read(pbData + bytesToRead, lDataLength - (ULONG)bytesToRead, dwReadBytes);
			*dwReadBytes += bytesRead;
		}
		else
		{
			hr = m_TSFile.Read(pbData, lDataLength, dwReadBytes);
			m_currentPosition += lDataLength;
		}
	}
	else
	{
		// The current position is past the end of the last file
		*dwReadBytes = 0;
	}

	return S_OK;
}

HRESULT MultiMemReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//If end method then we want llDistanceToMove to be the end of the buffer that we read.
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}

HRESULT MultiMemReader::get_ReadOnly(WORD *ReadOnly)
{
	CheckPointer(ReadOnly, E_POINTER);

	if (!m_TSBufferFile.IsFileInvalid())
		return m_TSBufferFile.get_ReadOnly(ReadOnly);

	*ReadOnly = m_bReadOnly;
	return S_OK;
}
        //ensures that there's always a back slash at the end
//        wPathName[wcslen(wPathName)] = char(92*(int)(wPathName[wcslen(wPathName)-1]!=char(92)));

HRESULT MultiMemReader::RefreshTSBufferFile()
{
	if (m_TSBufferFile.IsFileInvalid())
		return S_FALSE;

	ULONG bytesRead;
	MultiMemReaderFile *file;

	m_TSBufferFile.SetFilePointer(0, FILE_END);
	__int64 fileLength = m_TSBufferFile.GetFilePointer();
	if (fileLength <= (sizeof(__int64) + sizeof(long) + sizeof(long) + sizeof(wchar_t)))
		return S_FALSE;

	m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
	
	__int64 currentPosition;
	m_TSBufferFile.Read((LPBYTE)&currentPosition, sizeof(currentPosition), &bytesRead);

	long filesAdded, filesRemoved;
	m_TSBufferFile.Read((LPBYTE)&filesAdded, sizeof(filesAdded), &bytesRead);
	m_TSBufferFile.Read((LPBYTE)&filesRemoved, sizeof(filesRemoved), &bytesRead);

	if ((m_filesAdded != filesAdded) || (m_filesRemoved != filesRemoved))
	{
		long filesToRemove = filesRemoved - m_filesRemoved;
		long filesToAdd = filesAdded - m_filesAdded;
		long fileID = filesRemoved;
		__int64 nextStartPosition = 0;

		if (m_bDebugOutput)
		{
			TCHAR sz[128];
			wsprintf(sz, TEXT("Files Added %i, Removed %i\n"), filesToAdd, filesToRemove);
			::OutputDebugString(sz);
		}

		// Removed files that aren't present anymore.
		while ((filesToRemove > 0) && (m_tsFiles.size() > 0))
		{
			MultiMemReaderFile *file = m_tsFiles.at(0);

			if (m_bDebugOutput)
			{
				USES_CONVERSION;
				TCHAR sz[MAX_PATH+128];
				wsprintf(sz, TEXT("Removing file %s\n"), W2T(file->filename));
				::OutputDebugString(sz);
			}
			
			delete file;
			m_tsFiles.erase(m_tsFiles.begin());

			filesToRemove--;
		}


		// Figure out what the start position of the next new file will be
		if (m_tsFiles.size() > 0)
		{
			file = m_tsFiles.back();

			if (filesToAdd > 0)
			{
				// If we're adding files the changes are the one at the back has a partial length
				// so we need update it.
				if (m_bDebugOutput)
					GetFileLength(file->filename, file->length);
				else
					GetFileLength(file->filename, file->length);
			}

			nextStartPosition = file->startPosition + file->length;
		}
		__int64 remainingLength = fileLength - sizeof(__int64) - sizeof(long) - sizeof(long);

///////////////////////////////////////
//Bug Report: Possible issue, seems to get a large value of fileLength greater than the BYTE Array can produce.
		if (remainingLength > 4000000)
			return S_FALSE; //exit false until fixed
//////////////////////////////////////

		LPWSTR pBuffer = (LPWSTR)new BYTE[(UINT)remainingLength];
		m_TSBufferFile.Read((LPBYTE)pBuffer, (ULONG)remainingLength, &bytesRead);
		if (bytesRead < remainingLength)
		{
			delete[] pBuffer;
			::OutputDebugString(TEXT("What's going on?\n"));
			return E_FAIL;
		}

		//randomly park the file pointer to help minimise HDD clogging
		if(currentPosition&1)
			m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
		else
			m_TSBufferFile.SetFilePointer(0, FILE_END);

		//Get the real path of the buffer file
		LPWSTR wfilename;
		m_TSBufferFile.GetFileName(&wfilename);
		LPWSTR path = NULL;
		LPWSTR name = wcsrchr(wfilename, 92);
		if (name)
		{
			name++;
			long len = name - wfilename;
			path = new wchar_t[len+1];
			lstrcpynW(path, wfilename, len+1);
		}

		// Create a list of files in the .tsbuffer file.
		std::vector<LPWSTR> filenames;

		LPWSTR pCurr = pBuffer;
		long length = wcslen(pCurr);
		while (length > 0)
		{
			//modify filename path here to include the real path
			LPWSTR pFilename;
			LPWSTR temp = wcsrchr(pCurr, 92);
			if (path && temp)
			{
				temp++;
				pFilename = new wchar_t[wcslen(path)+wcslen(temp)+1];
				wcscpy(pFilename, path);
				wcscat(pFilename, temp);
			}
			else
			{
				pFilename = new wchar_t[length+1];
				wcscpy(pFilename, pCurr);
			}

//			LPWSTR pFilename = new wchar_t[length+1];
//			wcscpy(pFilename, pCurr);
			filenames.push_back(pFilename);

			pCurr += (length + 1);
			length = wcslen(pCurr);
		}

		if (path)
			delete[] path;

		delete[] pBuffer;

		// Go through files
		std::vector<MultiMemReaderFile *>::iterator itFiles = m_tsFiles.begin();
		std::vector<LPWSTR>::iterator itFilenames = filenames.begin();

		while (itFiles < m_tsFiles.end())
		{
			file = *itFiles;

			itFiles++;
			fileID++;

			if (itFilenames < filenames.end())
			{
				// TODO: Check that the filenames match
				itFilenames++;
			}
			else
			{
				::OutputDebugString(TEXT("Missing files!!\n"));
			}
		}

		while (itFilenames < filenames.end())
		{
			LPWSTR pFilename = *itFilenames;

			if (m_bDebugOutput)
			{
				USES_CONVERSION;
				TCHAR sz[MAX_PATH+128];
				int nextStPos = (int)nextStartPosition;
				wsprintf(sz, TEXT("Adding file %s (%i)\n"), W2T(pFilename), nextStPos);
				::OutputDebugString(sz);
			}

			file = new MultiMemReaderFile();
			file->filename = pFilename;
			file->startPosition = nextStartPosition;

			fileID++;
			file->filePositionId = fileID;

			GetFileLength(pFilename, file->length);

			m_tsFiles.push_back(file);

			nextStartPosition = file->startPosition + file->length;

			itFilenames++;
		}

		m_filesAdded = filesAdded;
		m_filesRemoved = filesRemoved;
	}

	if (m_tsFiles.size() > 0)
	{
		file = m_tsFiles.front();
		m_startPosition = file->startPosition;

		file = m_tsFiles.back();
		file->length = currentPosition;
		m_endPosition = file->startPosition + currentPosition;

	
		/*if (m_bDebugOutput)
		{
			TCHAR sz[128];
			int stPos = m_startPosition;
			int endPos = m_endPosition;
			int curPos = m_currentPosition;
			wsprintf(sz, TEXT("StartPosition %i, EndPosition %i, CurrentPosition %i\n"), stPos, endPos, curPos);
			::OutputDebugString(sz);
		}*/
	}
	else
	{
		m_startPosition = 0;
		m_endPosition = 0;
	}

	return S_OK;
}

HRESULT MultiMemReader::GetFileLength(LPWSTR pFilename, __int64 &length)
{
	USES_CONVERSION;

	length = 0;

	// Try to open the file
	HANDLE hFile = m_pSharedMemory->CreateFile(pFilename,   // The filename
						 (DWORD) GENERIC_READ,          // File access
						 (DWORD) (FILE_SHARE_READ |
						 FILE_SHARE_WRITE),       // Share access
						 NULL,                  // Security
						 (DWORD) OPEN_EXISTING,         // Open flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template
	if (hFile != INVALID_HANDLE_VALUE)
	{
		LARGE_INTEGER li;
		li.QuadPart = 0;
		li.LowPart = m_pSharedMemory->SetFilePointer(hFile, 0, &li.HighPart, FILE_END);
		m_pSharedMemory->CloseHandle(hFile);
		
		length = li.QuadPart;
	}
	else
	{
		wchar_t msg[MAX_PATH];
		DWORD dwErr = GetLastError();
		swprintf((LPWSTR)&msg, L"Failed to open file %s : 0x%x\n", pFilename, dwErr);
		::OutputDebugString(W2T((LPWSTR)&msg));
		return HRESULT_FROM_WIN32(dwErr);
	}
	return S_OK;
}

HRESULT MultiMemReader::get_DelayMode(WORD *DelayMode)
{
	*DelayMode = m_bDelay;
	return S_OK;
}

HRESULT MultiMemReader::set_DelayMode(WORD DelayMode)
{
	m_bDelay = DelayMode;
	return S_OK;
}

HRESULT MultiMemReader::get_ReaderMode(WORD *ReaderMode)
{
	*ReaderMode = TRUE;
	return S_OK;
}

DWORD MultiMemReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//Get the file information
	__int64 fileStart, fileEnd, fileLength;
	GetFileSize(&fileStart, &fileLength);
	fileEnd = (__int64)(fileLength + fileStart);
	if (dwMoveMethod == FILE_BEGIN)
		return SetFilePointer((__int64)min(fileEnd,(__int64)(llDistanceToMove + fileStart)), FILE_BEGIN);
	else
		return SetFilePointer((__int64)max((__int64)-fileLength, llDistanceToMove), FILE_END);
}

__int64 MultiMemReader::getFilePointer()
{
	__int64 fileStart, fileEnd, fileLength;
	GetFileSize(&fileStart, &fileLength);
	fileEnd = fileLength + fileStart;
	return (__int64)(GetFilePointer() - fileStart);
		
}

__int64 MultiMemReader::getBufferPointer()
{
	return 	m_llBufferPointer;	
}

void MultiMemReader::setBufferPointer()
{
	m_llBufferPointer = getFilePointer();	
}
