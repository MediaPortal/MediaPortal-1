/**
*  FileReader.cpp
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
#include "StdAfx.h"

#include "FileReader.h"
//#include "global.h"
extern void LogDebug(const char *fmt, ...) ;
FileReader::FileReader() :
	m_hFile(INVALID_HANDLE_VALUE),
	m_pFileName(0),
	m_bReadOnly(FALSE),
	m_fileSize(0),
	m_infoFileSize(0),
	m_fileStartPos(0),
	m_hInfoFile(INVALID_HANDLE_VALUE),
	m_bDelay(FALSE),
	m_llBufferPointer(0),	
	m_bDebugOutput(FALSE)
{
}

FileReader::~FileReader()
{
  CloseFile();
  if (m_pFileName)
    delete m_pFileName;
}


HRESULT FileReader::GetFileName(LPOLESTR *lpszFileName)
{
  *lpszFileName = m_pFileName;
  return S_OK;
}

HRESULT FileReader::SetFileName(LPCOLESTR pszFileName)
{
  // Is this a valid filename supplied
  //CheckPointer(pszFileName,E_POINTER);

  if(wcslen(pszFileName) > MAX_PATH)
    return ERROR_FILENAME_EXCED_RANGE;

  // Take a copy of the filename

  if (m_pFileName)
  {
    delete[] m_pFileName;
    m_pFileName = NULL;
  }
  m_pFileName = new WCHAR[1+lstrlenW(pszFileName)];
  if (m_pFileName == NULL)
    return E_OUTOFMEMORY;

  wcscpy(m_pFileName, pszFileName);

  return S_OK;
}

//
// OpenFile
//
// Opens the file ready for streaming
//
HRESULT FileReader::OpenFile()
{
	WCHAR *pFileName = NULL;
	int Tmo=5 ;
	// Is the file already opened
	if (m_hFile != INVALID_HANDLE_VALUE) 
  {
    LogDebug("FileReader::OpenFile() file already open");
		return NOERROR;
	}

	// Has a filename been set yet
	if (m_pFileName == NULL) 
  {
    LogDebug("FileReader::OpenFile() no filename");
		return ERROR_INVALID_NAME;
	}

//	BoostThread Boost;

	// Convert the UNICODE filename if necessary

//#if defined(WIN32) && !defined(UNICODE)
//	char convert[MAX_PATH];
//
//	if(!WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,convert,MAX_PATH,0,0))
//		return ERROR_INVALID_NAME;
//
//	pFileName = convert;
//#else

	pFileName = m_pFileName;
//#endif
	do
	{
		// do not try to open a tsbuffer file without SHARE_WRITE so skip this try if we have a buffer file
		if (wcsstr(pFileName, L".ts.tsbuffer") == NULL) 
		{
			// Try to open the file
			m_hFile = ::CreateFileW(pFileName,      // The filename
						 (DWORD) GENERIC_READ,        // File access
						 (DWORD) FILE_SHARE_READ,     // Share access
						 NULL,                        // Security
						 (DWORD) OPEN_EXISTING,       // Open flags
						 (DWORD) 0,                   // More flags
						 NULL);                       // Template

			m_bReadOnly = FALSE;
			if (m_hFile != INVALID_HANDLE_VALUE) break ;
		}

		//Test incase file is being recorded to
		m_hFile = ::CreateFileW(pFileName,		// The filename
							(DWORD) GENERIC_READ,				// File access
							(DWORD) (FILE_SHARE_READ |
							FILE_SHARE_WRITE),          // Share access
							NULL,						            // Security
							(DWORD) OPEN_EXISTING,		  // Open flags
//							(DWORD) 0,
							(DWORD) FILE_ATTRIBUTE_NORMAL,		// More flags
//							FILE_ATTRIBUTE_NORMAL |
//							FILE_FLAG_RANDOM_ACCESS,	      // More flags
//							FILE_FLAG_SEQUENTIAL_SCAN,	    // More flags
							NULL);						                // Template

		m_bReadOnly = TRUE;
		if (m_hFile != INVALID_HANDLE_VALUE) break ;

		Sleep(20) ;
	}
	while(--Tmo) ;
	if (Tmo)
	{
    if (Tmo<4) // 1 failed + 1 succeded is quasi-normal, more is a bit suspicious ( disk drive too slow or problem ? )
  			LogDebug("FileReader::OpenFile(), %d tries to succeed opening %ws.", 6-Tmo, pFileName);
	}
	else
	{
		LogDebug("FileReader::OpenFile(), open file %ws failed.", pFileName);
		DWORD dwErr = GetLastError();
		return HRESULT_FROM_WIN32(dwErr);
	}



  //LogDebug("FileReader::OpenFile() handle %i %ws", m_hFile, pFileName );

	WCHAR infoName[512];
	wcscpy(infoName, pFileName);
	wcscat(infoName, L".info");

	m_hInfoFile = ::CreateFileW(infoName,   // The filename
			(DWORD) GENERIC_READ,               // File access
			(DWORD) (FILE_SHARE_READ |
			FILE_SHARE_WRITE),                  // Share access
			NULL,                               // Security
			(DWORD) OPEN_EXISTING,              // Open flags
//			(DWORD) 0,
			(DWORD) FILE_ATTRIBUTE_NORMAL,      // More flags
//			FILE_FLAG_SEQUENTIAL_SCAN,	      // More flags
//			FILE_ATTRIBUTE_NORMAL |
//			FILE_FLAG_RANDOM_ACCESS,	        // More flags
			NULL);

  //LogDebug("FileReader::OpenFile() info file handle %i", m_hInfoFile);

	SetFilePointer(0, FILE_BEGIN);
	m_llBufferPointer = 0;	

	return S_OK;

} // Open

//
// CloseFile
//
// Closes any dump file we have opened
//
HRESULT FileReader::CloseFile()
{
	// Must lock this section to prevent problems related to
	// closing the file while still receiving data in Receive()

	if (m_hFile == INVALID_HANDLE_VALUE) 
  {
    LogDebug("FileReader::CloseFile() no open file");
		return S_OK;
	}

  //LogDebug("FileReader::CloseFile() handle %i %ws", m_hFile, m_pFileName);
  //LogDebug("FileReader::CloseFile() info file handle %i", m_hInfoFile);

//	BoostThread Boost;

	::CloseHandle(m_hFile);
	m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file

	if (m_hInfoFile != INVALID_HANDLE_VALUE)
		::CloseHandle(m_hInfoFile);

	m_hInfoFile = INVALID_HANDLE_VALUE; // Invalidate the file

	m_llBufferPointer = 0;	
	return NOERROR;

} // CloseFile

BOOL FileReader::IsFileInvalid()
{
	return (m_hFile == INVALID_HANDLE_VALUE);
}

HRESULT FileReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
	//CheckPointer(pStartPosition,E_POINTER);
	//CheckPointer(pLength,E_POINTER);
	
//	BoostThread Boost;

	GetStartPosition(pStartPosition);

	//Do not get file size if static file or first time 
	if (m_bReadOnly || !m_fileSize) {
		
		if (m_hInfoFile != INVALID_HANDLE_VALUE)
		{
			__int64 length = -1;
			DWORD read = 0;
			LARGE_INTEGER li;
			li.QuadPart = 0;
			::SetFilePointer(m_hInfoFile, li.LowPart, &li.HighPart, FILE_BEGIN);
			::ReadFile(m_hInfoFile, (PVOID)&length, (DWORD)sizeof(__int64), &read, NULL);

			if(length > -1)
			{
				m_fileSize = length;
				*pLength = length;
				return S_OK;
			}
		}

		DWORD dwSizeLow;
		DWORD dwSizeHigh;

		dwSizeLow = ::GetFileSize(m_hFile, &dwSizeHigh);
		if ((dwSizeLow == 0xFFFFFFFF) && (GetLastError() != NO_ERROR ))
		{
			return E_FAIL;
		}

		LARGE_INTEGER li;
		li.LowPart = dwSizeLow;
		li.HighPart = dwSizeHigh;
		m_fileSize = li.QuadPart;
	}
	*pLength = m_fileSize;
	return S_OK;
}

HRESULT FileReader::GetInfoFileSize(__int64 *lpllsize)
{
	//Do not get file size if static file or first time 
	if (m_bReadOnly || !m_infoFileSize) {
		
		DWORD dwSizeLow;
		DWORD dwSizeHigh;

//		BoostThread Boost;

		dwSizeLow = ::GetFileSize(m_hInfoFile, &dwSizeHigh);
		if ((dwSizeLow == 0xFFFFFFFF) && (GetLastError() != NO_ERROR ))
		{
			return E_FAIL;
		}

		LARGE_INTEGER li;
		li.LowPart = dwSizeLow;
		li.HighPart = dwSizeHigh;
		m_infoFileSize = li.QuadPart;
	}
	*lpllsize = m_infoFileSize;
	return S_OK;
}

HRESULT FileReader::GetStartPosition(__int64 *lpllpos)
{
	//Do not get file size if static file unless first time 
	if (m_bReadOnly || !m_fileStartPos) {
		
		if (m_hInfoFile != INVALID_HANDLE_VALUE)
		{
//			BoostThread Boost;
	
			__int64 size = 0;
			GetInfoFileSize(&size);
			//Check if timeshift info file
			if (size > sizeof(__int64))
			{
				//Get the file start pointer
				__int64 length = -1;
				DWORD read = 0;
				LARGE_INTEGER li;
				li.QuadPart = sizeof(__int64);
				::SetFilePointer(m_hInfoFile, li.LowPart, &li.HighPart, FILE_BEGIN);
				::ReadFile(m_hInfoFile, (PVOID)&length, (DWORD)sizeof(__int64), &read, NULL);

				if(length > -1)
				{
					m_fileStartPos = length;
					*lpllpos =  length;
					return S_OK;
				}
			}
		}
		m_fileStartPos = 0;
	}
	*lpllpos = m_fileStartPos;
	return S_OK;
}

DWORD FileReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
//	BoostThread Boost;

	LARGE_INTEGER li;

	if (dwMoveMethod == FILE_END && m_hInfoFile != INVALID_HANDLE_VALUE)
	{
		__int64 startPos = 0;
		GetStartPosition(&startPos);

		if (startPos > 0)
		{
			__int64 start;
			__int64 fileSize = 0;
			GetFileSize(&start, &fileSize);

			__int64 filePos  = (__int64)((__int64)fileSize + (__int64)llDistanceToMove + (__int64)startPos);

			if (filePos >= fileSize)
				li.QuadPart = (__int64)((__int64)startPos + (__int64)llDistanceToMove);
			else
				li.QuadPart = filePos;

			return ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, FILE_BEGIN);
		}

		__int64 start = 0;
		__int64 length = 0;
		GetFileSize(&start, &length);

		length  = (__int64)((__int64)length + (__int64)llDistanceToMove);

		li.QuadPart = length;

		dwMoveMethod = FILE_BEGIN;
	}
	else
	{
		__int64 startPos = 0;
		GetStartPosition(&startPos);

		if (startPos > 0)
		{
			__int64 start;
			__int64 fileSize = 0;
			GetFileSize(&start, &fileSize);

			__int64 filePos  = (__int64)((__int64)startPos + (__int64)llDistanceToMove);

			if (filePos >= fileSize)
				li.QuadPart = (__int64)((__int64)filePos - (__int64)fileSize);
			else
				li.QuadPart = filePos;

			return ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, dwMoveMethod);
		}
		li.QuadPart = llDistanceToMove;
	}

	return ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, dwMoveMethod);
}

__int64 FileReader::GetFilePointer()
{
//	BoostThread Boost;

	LARGE_INTEGER li;
	li.QuadPart = 0;
	li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);

	__int64 start;
	__int64 length = 0;
	GetFileSize(&start, &length);

	__int64 startPos = 0;
	GetStartPosition(&startPos);

	if (startPos > 0)
	{
		if(startPos > (__int64)li.QuadPart)
			li.QuadPart = (__int64)(length - startPos + (__int64)li.QuadPart);
		else
			li.QuadPart = (__int64)((__int64)li.QuadPart - startPos);
	}

	return li.QuadPart;
}

HRESULT FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	HRESULT hr;

	// If the file has already been closed, don't continue
	if (m_hFile == INVALID_HANDLE_VALUE)
  {
    LogDebug("FileReader::Read() no open file");
		return E_FAIL;
  }
//	BoostThread Boost;

	//Get File Position
	LARGE_INTEGER li;
	li.QuadPart = 0;
	li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);
	DWORD dwErr = ::GetLastError();
	if ((DWORD)li.LowPart == (DWORD)0xFFFFFFFF && dwErr)
	{
    LogDebug("FileReader::Read() seek failed");
		return E_FAIL;
	}
	__int64 m_filecurrent = li.QuadPart;

	if (m_hInfoFile != INVALID_HANDLE_VALUE)
	{
		__int64 startPos = 0;
		GetStartPosition(&startPos);

		if (startPos > 0)
		{
			__int64 start;
			__int64 length = 0;
			GetFileSize(&start, &length);

			if (length < (__int64)(m_filecurrent + (__int64)lDataLength) && m_filecurrent > startPos)
			{
				hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)max(0,(length - m_filecurrent)), dwReadBytes, NULL);
				if (!hr)
					return E_FAIL;

				LARGE_INTEGER li;
				li.QuadPart = 0;
				hr = ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, FILE_BEGIN);
				DWORD dwErr = ::GetLastError();
				if ((DWORD)hr == (DWORD)0xFFFFFFFF && dwErr)
				{
					return E_FAIL;
				}

				ULONG dwRead = 0;

				hr = ::ReadFile(m_hFile,
					(PVOID)(pbData + (DWORD)max(0,(length - m_filecurrent))),
					(DWORD)max(0,((__int64)lDataLength -(__int64)(length - m_filecurrent))),
					&dwRead,
					NULL);

				*dwReadBytes = *dwReadBytes + dwRead;

			}
			else if (startPos < (__int64)(m_filecurrent + (__int64)lDataLength) && m_filecurrent < startPos)
				hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)max(0,(startPos - m_filecurrent)), dwReadBytes, NULL);

			else
				hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);

			if (!hr)
				return S_FALSE;

			if (*dwReadBytes < (ULONG)lDataLength)
			{
				return E_FAIL;
			}

			return S_OK;
		}

		__int64 start = 0;
		__int64 length = 0;
		GetFileSize(&start, &length);
		if (length < (__int64)(m_filecurrent + (__int64)lDataLength))
			hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)max(0,(length - m_filecurrent)), dwReadBytes, NULL);
		else
			hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);
	}
	else
		hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);//Read file data into buffer

	if (!hr)
	{
    LogDebug("FileReader::Read() read failed - error = %d",  HRESULT_FROM_WIN32(GetLastError()));
		return E_FAIL;
	}

	if (*dwReadBytes < (ULONG)lDataLength)
  {
    LogDebug("FileReader::Read() read to less bytes");
		return S_FALSE;
  }
	return S_OK;
}

HRESULT FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
//	BoostThread Boost;

	//If end method then we want llDistanceToMove to be the end of the buffer that we read.
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}

HRESULT FileReader::get_ReadOnly(WORD *ReadOnly)
{
	*ReadOnly = m_bReadOnly;
	return S_OK;
}

HRESULT FileReader::get_DelayMode(WORD *DelayMode)
{
	*DelayMode = m_bDelay;
	return S_OK;
}

HRESULT FileReader::set_DelayMode(WORD DelayMode)
{
	m_bDelay = DelayMode;
	return S_OK;
}

HRESULT FileReader::get_ReaderMode(WORD *ReaderMode)
{
	*ReaderMode = FALSE;
	return S_OK;
}

void FileReader::SetDebugOutput(BOOL bDebugOutput)
{
	m_bDebugOutput = bDebugOutput;
}

DWORD FileReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//Get the file information
	__int64 fileStart, fileEnd, fileLength;
	GetFileSize(&fileStart, &fileLength);
	fileEnd = fileLength;
	if (dwMoveMethod == FILE_BEGIN)
		return SetFilePointer((__int64)min(fileEnd, llDistanceToMove), FILE_BEGIN);
	else
		return SetFilePointer((__int64)max((__int64)-fileLength, llDistanceToMove), FILE_END);
}

__int64 FileReader::getFilePointer()
{
	return GetFilePointer();
}

__int64 FileReader::getBufferPointer()
{
	return 	m_llBufferPointer;	
}

void FileReader::setBufferPointer()
{
	m_llBufferPointer = GetFilePointer();	
}

__int64 FileReader::GetFileSize()
{
  __int64 pStartPosition =0;
  __int64 pLength=0;
  GetFileSize(&pStartPosition, &pLength);
  return pLength;
}