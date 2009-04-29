/**
*  FileWriter.cpp
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
#include "FileWriter.h"
#include <atlbase.h>

FileWriter::FileWriter() :
	m_hFile(INVALID_HANDLE_VALUE),
	m_pFileName(0),
	m_bChunkReserve(FALSE),
	m_chunkReserveSize(2000000),
	m_chunkReserveFileSize(0),
	m_maxFileSize(0)
{
}

FileWriter::~FileWriter()
{
	CloseFile();
	if (m_pFileName)
		delete m_pFileName;
}

HRESULT FileWriter::GetFileName(LPWSTR *lpszFileName)
{
	*lpszFileName = m_pFileName;
	return S_OK;
}

HRESULT FileWriter::SetFileName(LPCWSTR pszFileName)
{
	// Is this a valid filename supplied
	CheckPointer(pszFileName,E_POINTER);

	long length = wcslen(pszFileName);

	if(length > MAX_PATH)
		return ERROR_FILENAME_EXCED_RANGE;

	// Take a copy of the filename

	if (m_pFileName)
	{
		delete[] m_pFileName;
		m_pFileName = NULL;
	}

	m_pFileName = new wchar_t[length+1];
	if (m_pFileName == NULL)
		return E_OUTOFMEMORY;

	wcscpy(m_pFileName,pszFileName);

	return S_OK;
}

//
// OpenFile
//
HRESULT FileWriter::OpenFile()
{
	USES_CONVERSION;

	TCHAR *pFileName = NULL;

	// Is the file already opened
	if (m_hFile != INVALID_HANDLE_VALUE)
	{
		return NOERROR;
	}

	// Has a filename been set yet
	if (m_pFileName == NULL)
	{
		return ERROR_INVALID_NAME;
	}

	// See the the file is being read.
	m_hFile = CreateFile(W2T(m_pFileName),      // The filename
						 (DWORD) GENERIC_WRITE,         // File access
						 (DWORD) NULL,                  // Share access
						 NULL,                  // Security
						 (DWORD) OPEN_ALWAYS,           // Open flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template
	if (m_hFile == INVALID_HANDLE_VALUE)
	{
		DWORD dwErr = GetLastError();
		if (dwErr == ERROR_SHARING_VIOLATION)
		{
			return HRESULT_FROM_WIN32(dwErr);
		}
		return HRESULT_FROM_WIN32(dwErr);
	}
	CloseHandle(m_hFile);

	// Try to open the file
	m_hFile = CreateFile(W2T(m_pFileName),      // The filename
						 (DWORD) GENERIC_WRITE,         // File access
						 (DWORD) FILE_SHARE_READ,       // Share access
						 NULL,                  // Security
						 (DWORD) OPEN_ALWAYS,           // Open flags
//						 (DWORD) FILE_FLAG_RANDOM_ACCESS,
//						 (DWORD) FILE_FLAG_WRITE_THROUGH,             // More flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template

	if (m_hFile == INVALID_HANDLE_VALUE)
	{
		DWORD dwErr = GetLastError();
		return HRESULT_FROM_WIN32(dwErr);
	}

	SetFilePointer(0, FILE_END);
	m_chunkReserveFileSize = GetFilePointer();
	SetFilePointer(0, FILE_BEGIN);

	return S_OK;
}

//
// CloseFile
//
HRESULT FileWriter::CloseFile()
{
	if (m_hFile == INVALID_HANDLE_VALUE)
	{
		return S_OK;
	}

	if (m_bChunkReserve)
	{
		__int64 currentPosition = GetFilePointer();
		SetFilePointer(currentPosition, FILE_BEGIN);
		SetEndOfFile(m_hFile);
	}

	CloseHandle(m_hFile);
	m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file

	return S_OK;

}

BOOL FileWriter::IsFileInvalid()
{
	return (m_hFile == INVALID_HANDLE_VALUE);
}

DWORD FileWriter::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	LARGE_INTEGER li;
	li.QuadPart = llDistanceToMove;
	return ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, dwMoveMethod);
}

__int64 FileWriter::GetFilePointer()
{
	LARGE_INTEGER li;
	li.QuadPart = 0;
	li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);
	return li.QuadPart;
}

HRESULT FileWriter::Write(PBYTE pbData, ULONG lDataLength)
{
	HRESULT hr;

	// If the file has already been closed, don't continue
	if (m_hFile == INVALID_HANDLE_VALUE)
		return S_FALSE;

	if (m_bChunkReserve)
	{
		__int64 currentPosition = GetFilePointer();
		if ((currentPosition + lDataLength > m_chunkReserveFileSize) &&
			(m_chunkReserveFileSize < m_maxFileSize))
		{
			while (currentPosition + lDataLength > m_chunkReserveFileSize)
				m_chunkReserveFileSize += m_chunkReserveSize;

			if (m_chunkReserveFileSize > m_maxFileSize)
				m_chunkReserveFileSize = m_maxFileSize;

			SetFilePointer(m_chunkReserveFileSize, FILE_BEGIN);
			SetEndOfFile(m_hFile);
			SetFilePointer(currentPosition, FILE_BEGIN);
		}
	}

	DWORD written = 0;
	hr = WriteFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, &written, NULL);

	if (FAILED(hr))
		return hr;
	if (written < (ULONG)lDataLength)
		return S_FALSE;

	return S_OK;
}

void FileWriter::SetChunkReserve(BOOL bEnable, __int64 chunkReserveSize, __int64 maxFileSize)
{
	m_bChunkReserve = bEnable;
	if (m_bChunkReserve)
	{
		m_chunkReserveSize = chunkReserveSize;
		m_chunkReserveFileSize = 0;
		m_maxFileSize = maxFileSize;
	}
}

