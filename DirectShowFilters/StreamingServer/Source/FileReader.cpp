/**
*  FileReader.cpp
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

//#include <streams.h>
#include "FileReader.h"
#include <stdio.h>
extern void LogDebug(const char *fmt, ...) ;
FileReader::FileReader() :
	m_hFile(INVALID_HANDLE_VALUE),
	m_bReadOnly(FALSE),
	m_fileSize(0),
	m_infoFileSize(0),
	m_fileStartPos(0),
	m_hInfoFile(INVALID_HANDLE_VALUE),
	m_bDelay(FALSE),
	m_bDebugOutput(FALSE)
{
}

FileReader::~FileReader()
{
	CloseFile();
	
}

FileReader* FileReader::CreateFileReader()
{
	return new FileReader();
}

int FileReader::GetFileName(char *lpszFileName)
{
  strcpy(lpszFileName,m_fileName);
	return S_OK;
}

int FileReader::SetFileName(char* pszFileName)
{
	// Is this a valid filename supplied
	//CheckPointer(pszFileName,E_POINTER);

	strcpy(m_fileName,pszFileName);

	return S_OK;
}

//
// OpenFile
//
// Opens the file ready for streaming
//
int FileReader::OpenFile()
{
	int Tmo=5 ;

    //printf("FileReader::OpenFile(%s)\n",m_fileName);
	// Is the file already opened
	if (m_hFile != INVALID_HANDLE_VALUE) 
  {
    LogDebug("FileReader::OpenFile() already opened");
		return NOERROR;
	}

	do
	{
		// Try to open the file
		m_hFile = CreateFileA((LPCSTR)m_fileName,   // The filename
						 GENERIC_READ,          // File access
						 FILE_SHARE_READ,       // Share access
						 NULL,                  // Security
						 OPEN_EXISTING,         // Open flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template

		m_bReadOnly = FALSE;
		if (m_hFile != INVALID_HANDLE_VALUE) break ;

		//Test incase file is being recorded to
		m_hFile = CreateFileA((LPCSTR)m_fileName,		// The filename
							GENERIC_READ,				// File access
							FILE_SHARE_READ |
							FILE_SHARE_WRITE,   // Share access
							NULL,						// Security
							OPEN_EXISTING,				// Open flags
							FILE_ATTRIBUTE_NORMAL,		// More flags
//							FILE_ATTRIBUTE_NORMAL |
//							FILE_FLAG_RANDOM_ACCESS,	// More flags
//							FILE_FLAG_SEQUENTIAL_SCAN,	// More flags
							NULL);						// Template

		m_bReadOnly = TRUE;
		if (m_hFile != INVALID_HANDLE_VALUE) break ;

		Sleep(20) ;
	}
	while(--Tmo) ;
	if (Tmo)
	{
    if (Tmo<4) // 1 failed + 1 succeded is quasi-normal, more is a bit suspicious ( disk drive too slow or problem ? )
  			LogDebug("FileReader::OpenFile(%s), %d tries to succeed opening %ws.",(LPCSTR)m_fileName, 6-Tmo);
	}
	else
	{
		DWORD dwErr = GetLastError();
    LogDebug("FileReader::OpenFile(%s) failed:%d",(LPCSTR)m_fileName,dwErr);
		return (int)dwErr;
	}
/*
	char infoName[512];
	strcpy(infoName, m_fileName);
	strcat(infoName, ".info");

	m_hInfoFile = CreateFile(m_fileName, // The filename
			GENERIC_READ,    // File access
			FILE_SHARE_READ |
			FILE_SHARE_WRITE,   // Share access
			NULL,      // Security
			OPEN_EXISTING,    // Open flags
			FILE_ATTRIBUTE_NORMAL, // More flags
//			FILE_FLAG_SEQUENTIAL_SCAN,	// More flags
//			FILE_ATTRIBUTE_NORMAL |
//			FILE_FLAG_RANDOM_ACCESS,	// More flags
			NULL);
*/
	SetFilePointer(0, FILE_BEGIN);

	return S_OK;

} // Open

//
// CloseFile
//
// Closes any dump file we have opened
//
int FileReader::CloseFile()
{
	// Must lock this section to prevent problems related to
	// closing the file while still receiving data in Receive()


//    printf("FileReader::CloseFile()\n");
	if (m_hFile == INVALID_HANDLE_VALUE) {

		return S_OK;
	}

	CloseHandle(m_hFile);
	m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file

	if (m_hInfoFile != INVALID_HANDLE_VALUE)
		CloseHandle(m_hInfoFile);

	m_hInfoFile = INVALID_HANDLE_VALUE; // Invalidate the file

	return NOERROR;

} // CloseFile

BOOL FileReader::IsFileInvalid()
{
	return (m_hFile == INVALID_HANDLE_VALUE);
}

__int64 FileReader::GetFileSize()
{
  __int64 pStartPosition =0;
  __int64 pLength=0;
  GetFileSize(&pStartPosition, &pLength);
  return pLength;
}
int FileReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
//	CheckPointer(pStartPosition,E_POINTER);
//	CheckPointer(pLength,E_POINTER);
	
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
			ReadFile(m_hInfoFile, (PVOID)&length, (DWORD)sizeof(__int64), &read, NULL);

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
      LogDebug("FileReader::GetFileSize() failed()");
			return E_FAIL;
		}

		LARGE_INTEGER li;
		li.LowPart = dwSizeLow;
		li.HighPart = dwSizeHigh;
		m_fileSize = li.QuadPart;
	}
  //printf("FileReader::GetFileSize() :%d\n",(int)m_fileSize );
	*pLength = m_fileSize;
	return S_OK;
}

int FileReader::GetInfoFileSize(__int64 *lpllsize)
{
	//Do not get file size if static file or first time 
	if (m_bReadOnly || !m_infoFileSize) {
		
		DWORD dwSizeLow;
		DWORD dwSizeHigh;

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

int FileReader::GetStartPosition(__int64 *lpllpos)
{
	//Do not get file size if static file unless first time 
	if (m_bReadOnly || !m_fileStartPos) {
		
		if (m_hInfoFile != INVALID_HANDLE_VALUE)
		{
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
				ReadFile(m_hInfoFile, (PVOID)&length, (DWORD)sizeof(__int64), &read, NULL);

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

int FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	int hr;
  //printf("FileReader::Read:%d\n", (int)lDataLength);
	// If the file has already been closed, don't continue
	if (m_hFile == INVALID_HANDLE_VALUE)
		return S_FALSE;

	//Get File Position
	LARGE_INTEGER li;
	li.QuadPart = 0;
	li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);
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

				hr = ReadFile(m_hFile, (PVOID)pbData, (DWORD)(length - m_filecurrent), dwReadBytes, NULL);
				if (FAILED(hr))
        {
          //printf(" read failed:%d",hr);
					return hr;
        }
				LARGE_INTEGER li;
				li.QuadPart = 0;
				::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, FILE_BEGIN);

				ULONG dwRead = 0;

				hr = ReadFile(m_hFile,
					(PVOID)(pbData + (DWORD)(length - m_filecurrent)),
					(DWORD)((__int64)lDataLength -(__int64)((__int64)length - (__int64)m_filecurrent)),
					&dwRead,
					NULL);

				*dwReadBytes = *dwReadBytes + dwRead;

			}
			else if (startPos < (__int64)(m_filecurrent + (__int64)lDataLength) && m_filecurrent < startPos)
				hr = ReadFile(m_hFile, (PVOID)pbData, (DWORD)(startPos - m_filecurrent), dwReadBytes, NULL);

			else
				hr = ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);


	    if (FAILED(hr))
      {
        //printf(" read failed:%d",hr);
		    return hr;
      }
	    if (*dwReadBytes < (ULONG)lDataLength)
      {
        //printf(" read failed not enough data :%d/%d",(int)(*dwReadBytes), (int)lDataLength);
		    return S_FALSE;
      }

			return S_OK;
		}

		__int64 start = 0;
		__int64 length = 0;
		GetFileSize(&start, &length);
		if (length < (__int64)(m_filecurrent + (__int64)lDataLength))
			hr = ReadFile(m_hFile, (PVOID)pbData, (DWORD)(length - m_filecurrent), dwReadBytes, NULL);
		else
			hr = ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);
	}
	else
		hr = ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);//Read file data into buffer

	if (FAILED(hr))
  {
    //printf(" read failed:%d",hr);
		return hr;
  }
	if (*dwReadBytes < (ULONG)lDataLength)
  {
    //printf(" read failed not enough data :%d/%d",(int)(*dwReadBytes), (int)lDataLength);
		return S_FALSE;
  }
	return S_OK;
}

int FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//If end method then we want llDistanceToMove to be the end of the buffer that we read.
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}

int FileReader::get_ReadOnly(WORD *ReadOnly)
{
	*ReadOnly = m_bReadOnly;
	return S_OK;
}

int FileReader::get_DelayMode(WORD *DelayMode)
{
	*DelayMode = m_bDelay;
	return S_OK;
}

int FileReader::set_DelayMode(WORD DelayMode)
{
	m_bDelay = DelayMode;
	return S_OK;
}

int FileReader::get_ReaderMode(WORD *ReaderMode)
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

