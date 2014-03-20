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
#include <Mmsystem.h>
#include <comdef.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...) ;

FileReader::FileReader() :
	m_hFile(INVALID_HANDLE_VALUE),
	m_pFileName(0),
  m_bUseDummyWrites(FALSE),
  m_bIsStopping(FALSE)
{
  //LogDebug("FileReader::ctor");
}

FileReader::~FileReader()
{
  m_bIsStopping = true;
	if (m_hFile != INVALID_HANDLE_VALUE) 
  {
	  CancelIoEx(m_hFile, NULL); //Cancel all pending IO operations
    CloseFile();
  }
  
  if (m_pFileName)
    delete m_pFileName;
}


HRESULT FileReader::GetFileName(LPOLESTR *lpszFileName)
{
  CAutoLock rLock (&m_accessLock);
  *lpszFileName = m_pFileName;
  return S_OK;
}

HRESULT FileReader::SetFileName(LPCOLESTR pszFileName)
{
  CAutoLock rLock (&m_accessLock);
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
  CAutoLock rLock (&m_accessLock);
	WCHAR *pFileName = NULL;
	DWORD Tmo=14 ;
  HANDLE hFileUnbuff = INVALID_HANDLE_VALUE;
  
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

  //LogDebug("FileReader::OpenFile(), Filename: %ws.", pFileName);

	do
	{
	  if (m_bIsStopping)
	    return E_FAIL;
	    
	  if (m_bUseDummyWrites)  //enable SMB2/SMB3 file existence cache workaround
	  {
  		if ((wcsstr(pFileName, L".ts.tsbuffer") != NULL)) //timeshift file only
  		{  	  
    	  CString tempFileName = pFileName;
    	  
    	  int replCount = tempFileName.Replace(L".ts.tsbuffer", randomStrGen(12));
  
        if (replCount > 0)
        {
    	    //LogDebug("FileReader::OpenFile(), try to write dummy file to update SMB2 cache - %ws", tempFileName);
      		hFileUnbuff = ::CreateFileW(tempFileName,		// The filename
      							(DWORD) (GENERIC_READ | GENERIC_WRITE),				// File access
      							(DWORD) (FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE), // Share access
      							NULL,						            // Security
      							(DWORD) CREATE_ALWAYS,		  // Open flags
      							(DWORD) (FILE_ATTRIBUTE_NORMAL | FILE_FLAG_DELETE_ON_CLOSE), // | FILE_FLAG_WRITE_THROUGH),	// More flags
      							NULL);						          // Template
      
      		if (hFileUnbuff != INVALID_HANDLE_VALUE)
      		{
      		  char tempData[16] = {0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF};
      		  DWORD bytesWritten;
            ::WriteFile(hFileUnbuff, tempData, 16, &bytesWritten, NULL);  
          	::CloseHandle(hFileUnbuff); //File is deleted on CloseHandle since FILE_FLAG_DELETE_ON_CLOSE was used
          	hFileUnbuff = INVALID_HANDLE_VALUE; // Invalidate the file
    	      //LogDebug("FileReader::OpenFile(), dummy file write %d bytes to %ws", bytesWritten, tempFileName);
      		}
      	}
      }
    }
    
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

		if (m_hFile != INVALID_HANDLE_VALUE) break ;

		if ((wcsstr(pFileName, L".ts.tsbuffer") != NULL) && (Tmo<10)) //timeshift file only
		{
  	  if (m_bUseDummyWrites)  //enable SMB2/SMB3 file existence cache workaround
  	  {
  		  //Not succeeded in opening file yet, try WRITE_THROUGH dummy file write
    	  CString tempFileName = pFileName;
    	  int replCount = tempFileName.Replace(L".ts.tsbuffer", randomStrGen(12));
  
        if (replCount > 0)
        {
    	    // LogDebug("FileReader::OpenFile(), try to write dummy file to update SMB2 cache - %ws", tempFileName);
      		hFileUnbuff = ::CreateFileW(tempFileName,		// The filename
      							(DWORD) (GENERIC_READ | GENERIC_WRITE),				// File access
      							(DWORD) (FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE), // Share access
      							NULL,						            // Security
      							(DWORD) CREATE_ALWAYS,		  // Open flags
      							(DWORD) (FILE_ATTRIBUTE_NORMAL | FILE_FLAG_DELETE_ON_CLOSE | FILE_FLAG_WRITE_THROUGH),	// More flags
      							NULL);						          // Template
      
      		if (hFileUnbuff != INVALID_HANDLE_VALUE)
      		{
      		  char tempData[16] = {0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF};
      		  DWORD bytesWritten;
            ::WriteFile(hFileUnbuff, tempData, 16, &bytesWritten, NULL);  
            Sleep(50);   		  
          	::CloseHandle(hFileUnbuff); //File is deleted on CloseHandle since FILE_FLAG_DELETE_ON_CLOSE was used
          	hFileUnbuff = INVALID_HANDLE_VALUE; // Invalidate the file
    	      LogDebug("FileReader::OpenFile(), dummy file write %d bytes to %ws", bytesWritten, tempFileName);
      		}
      	}
      }

  		//No luck yet, so try unbuffered open and close (to flush SMB2 cache?),
  		//then go round loop again to open it properly (hopefully....)
  		hFileUnbuff = ::CreateFileW(pFileName,		// The filename
  							(DWORD) GENERIC_READ,				// File access
  							(DWORD) (FILE_SHARE_READ | FILE_SHARE_WRITE), // Share access
  							NULL,						            // Security
  							(DWORD) OPEN_EXISTING,		  // Open flags
  							(DWORD) (FILE_ATTRIBUTE_NORMAL | FILE_FLAG_NO_BUFFERING),	// More flags
  							NULL);						          // Template
  
  		if (hFileUnbuff != INVALID_HANDLE_VALUE)
  		{
      	::CloseHandle(hFileUnbuff);
      	hFileUnbuff = INVALID_HANDLE_VALUE; // Invalidate the file
  		}
  	  LogDebug("FileReader::OpenFile() unbuff, %d tries to open %ws", 15-Tmo, pFileName);  	  
    }

		Sleep(min((20*(15-Tmo)),250)) ; //wait longer between retries as loop iterations increase
	}
	while(--Tmo) ;
	
	if (Tmo)
	{
    if (Tmo<13) // 1 failed + 1 succeded is quasi-normal, more is a bit suspicious ( disk drive too slow or problem ? )
  			LogDebug("FileReader::OpenFile(), %d tries to succeed opening %ws.", 15-Tmo, pFileName);
	}
	else
	{
	  HRESULT lastErr = HRESULT_FROM_WIN32(GetLastError());	  
		LogDebug("FileReader::OpenFile(), open file failed. Error 0x%x, %ws, filename = %ws", lastErr, HresultToCString(lastErr), pFileName);    
		return E_FAIL;
	}

  //LogDebug("FileReader::OpenFile() handle %i %ws", m_hFile, pFileName );

	LARGE_INTEGER li;
	li.QuadPart = 0;
	::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, FILE_BEGIN);

	return S_OK;

} // Open

//
// CloseFile
//
HRESULT FileReader::CloseFile()
{
  CAutoLock rLock (&m_accessLock);
	// Must lock this section to prevent problems related to
	// closing the file while still receiving data in Receive()

	if (m_hFile == INVALID_HANDLE_VALUE) 
  {
    LogDebug("FileReader::CloseFile() no open file");
		return S_OK;
	}

  //LogDebug("FileReader::CloseFile() handle %i %ws", m_hFile, m_pFileName);
  
	::CloseHandle(m_hFile);
	m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file

	return NOERROR;
} // CloseFile

BOOL FileReader::IsFileInvalid()
{
  CAutoLock rLock (&m_accessLock);
	return (m_hFile == INVALID_HANDLE_VALUE);
}

HRESULT FileReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
	*pStartPosition = 0;
		
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
	
	*pLength = li.QuadPart;
	return S_OK;
}

DWORD FileReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
  CAutoLock rLock (&m_accessLock);

	LARGE_INTEGER li;

	li.QuadPart = llDistanceToMove;

	return ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, dwMoveMethod);
}

__int64 FileReader::GetFilePointer()
{
  CAutoLock rLock (&m_accessLock);

	LARGE_INTEGER li;
	li.QuadPart = 0;
	li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);

	return li.QuadPart;
}

HRESULT FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
  CAutoLock rLock (&m_accessLock);
	HRESULT hr;

	// If the file has already been closed, don't continue
	if (m_hFile == INVALID_HANDLE_VALUE)
  {
    LogDebug("FileReader::Read() no open file");
    *dwReadBytes = 0;
		return E_FAIL;
  }

  hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);//Read file data into buffer

	if (!hr)
	{
	  HRESULT lastErr = HRESULT_FROM_WIN32(GetLastError());	  
    LogDebug("FileReader::Read() read failed, Error = 0x%x, %ws, filename = %ws", lastErr, HresultToCString(lastErr), m_pFileName);
    *dwReadBytes = 0;
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
	//If end method then we want llDistanceToMove to be the end of the buffer that we read.
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}


__int64 FileReader::GetFileSize()
{
  CAutoLock rLock (&m_accessLock);
  __int64 pStartPosition =0;
  __int64 pLength=0;
  GetFileSize(&pStartPosition, &pLength);
  return pLength;
}

CString FileReader::randomStrGen(int length) 
{
    CString charset = ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
    CString result ( 'x', length );

    srand((unsigned int)timeGetTime());
    for (int i = 0; i < length; i++)
        result.SetAt(i, charset[rand() % charset.GetLength()]);

    return result;
}

//Enable dummy file writes to workaround SMB2/SM3 'file existance cache' problems
void FileReader::SetDummyWrites(BOOL useDummyWrites)
{
  CAutoLock rLock (&m_accessLock);
	m_bUseDummyWrites = useDummyWrites;
	//LogDebug("FileReader::SetDummyWrites, useDummyWrites = %d", useDummyWrites);
}

//for MultiFileReader() compatibility only
void FileReader::SetFileNext(BOOL useFileNext)
{
}

void FileReader::SetStopping(BOOL isStopping)
{
	m_bIsStopping = isStopping;
}

CString FileReader::HresultToCString(HRESULT errToConvert)
{
  _com_error error(errToConvert);
  CString errorText = error.ErrorMessage();
  return errorText;
}

void FileReader::CancelPendingIO()
{
	if (m_hFile != INVALID_HANDLE_VALUE) 
  {
    //LogDebug("FileReader::CancelPendingIO()");
	  CancelIoEx(m_hFile, NULL); //Cancel all pending IO operations
	}
}
