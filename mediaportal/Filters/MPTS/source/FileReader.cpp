/*
	MediaPortal TS-SourceFilter by Agree

	this files was taken from OpenSource TSSourceFilter by nate & bear & bisswanger
*/

#include <streams.h>
#include "FileReader.h"
extern void LogDebug(const char *fmt, ...) ;
FileReader::FileReader() :
	m_hFile(INVALID_HANDLE_VALUE),
	m_pFileName(0),
	m_bReadOnly(FALSE),
	m_fileSize(0),
	m_hInfoFile(INVALID_HANDLE_VALUE),
	m_bDelay(FALSE)
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
	CheckPointer(pszFileName,E_POINTER);

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

	wcscpy(m_pFileName,pszFileName);

	return S_OK;
}

//
// OpenFile
//
// Opens the file ready for streaming
//
HRESULT FileReader::OpenFile()
{
	TCHAR *pFileName = NULL;

	// Is the file already opened
	if (m_hFile != INVALID_HANDLE_VALUE) 
	{
		return NOERROR;
	}

	LogDebug("FileReader::OpenFile()");
	// Has a filename been set yet
	if (m_pFileName == NULL) 
	{
		LogDebug("FileReader::OpenFile() invalid filename");
		return ERROR_INVALID_NAME;
	}

	// Convert the UNICODE filename if necessary

#if defined(WIN32) && !defined(UNICODE)
	char convert[MAX_PATH];

	if(!WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,convert,MAX_PATH,0,0))
		return ERROR_INVALID_NAME;

	pFileName = convert;
#else
	pFileName = m_pFileName;
#endif

	m_bReadOnly = FALSE;

	// Try to open the file
	m_hFile = CreateFile((LPCTSTR) pFileName,   // The filename
						 GENERIC_READ,          // File access
						 FILE_SHARE_READ,       // Share access
						 NULL,                  // Security
						 OPEN_EXISTING,         // Open flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template

	if (m_hFile == INVALID_HANDLE_VALUE) {

		DWORD dwErr = GetLastError();
		LogDebug("FileReader::OpenFile() unable to open file:%x, try again",dwErr);
		//Test incase file is being recorded to
		m_hFile = CreateFile((LPCTSTR) pFileName,		// The filename
							GENERIC_READ,				// File access
							FILE_SHARE_READ |
							FILE_SHARE_WRITE,			// Share access
							NULL,						// Security
							OPEN_EXISTING,				// Open flags
							FILE_ATTRIBUTE_NORMAL |
							FILE_FLAG_RANDOM_ACCESS, // More flags
							NULL);						// Template

		if (m_hFile == INVALID_HANDLE_VALUE)
		{
			LogDebug("FileReader::OpenFile() unable to open file:%x",dwErr);
			return HRESULT_FROM_WIN32(dwErr);
		}

		m_bReadOnly = TRUE;
	}

	TCHAR infoName[512];
	strcpy(infoName, pFileName);
	strcat(infoName, ".info");

	m_hInfoFile = CreateFile((LPCTSTR) infoName, // The filename
			GENERIC_READ,    // File access
			FILE_SHARE_READ |
			FILE_SHARE_WRITE,   // Share access
			NULL,      // Security
			OPEN_EXISTING,    // Open flags
			FILE_ATTRIBUTE_NORMAL, // |	FILE_FLAG_RANDOM_ACCESS, // More flags
			NULL);

	LogDebug("FileReader::OpenFile() file opened");

	return S_OK;

}
//
HRESULT FileReader::CloseFile()
{
	if (m_hFile == INVALID_HANDLE_VALUE) {

		return S_OK;
	}

	LogDebug("FileReader::CloseFile()");
	CloseHandle(m_hFile);
	m_hFile = INVALID_HANDLE_VALUE; 

	if (m_hInfoFile != INVALID_HANDLE_VALUE)
		CloseHandle(m_hInfoFile);

	m_hInfoFile = INVALID_HANDLE_VALUE; 

	return NOERROR;

}

BOOL FileReader::IsFileInvalid()
{
	return m_hFile == INVALID_HANDLE_VALUE?true:false;
}

HRESULT FileReader::GetFileSize(HANDLE handle,__int64 *lpllsize)
{
	LARGE_INTEGER li;
	li.QuadPart=0;
	li.LowPart= ::GetFileSize(handle, (LPDWORD)&li.HighPart);
	if ((li.LowPart == 0xFFFFFFFF) && (GetLastError() != NO_ERROR ))
	{
		LogDebug("FileReader:GetFileSize failed with %x", GetLastError() );

		return E_FAIL;
	}

	*lpllsize = li.QuadPart;
	return S_OK;
}

HRESULT FileReader::GetFileSize(__int64 *lpllsize)
{
	LARGE_INTEGER li;
	li.QuadPart=0;
	li.LowPart= ::GetFileSize(m_hFile, (LPDWORD)&li.HighPart);
	if ((li.LowPart == 0xFFFFFFFF) && (GetLastError() != NO_ERROR ))
	{
		LogDebug("FileReader:GetFileSize failed with %x", GetLastError() );

		return E_FAIL;
	}

	m_fileSize = li.QuadPart;
	*lpllsize = li.QuadPart;
	return S_OK;
}

__int64 FileReader::get_FileSize(void)
{
	return m_fileSize;
}


DWORD FileReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	LARGE_INTEGER li;


	if (dwMoveMethod == FILE_END)
	{
		__int64 length = 0;
		GetFileSize(&length);
		length  = (__int64)((__int64)length + (__int64)llDistanceToMove);
		li.QuadPart = length;
		dwMoveMethod = FILE_BEGIN;
		LogDebug("FileReader:SetFilePointer %x %x (%x)", li.HighPart,li.LowPart, dwMoveMethod);
	}
	else
	{
		li.QuadPart = llDistanceToMove;
		LogDebug("FileReader:SetFilePointer %x %x (%x)", li.HighPart,li.LowPart, dwMoveMethod);
	}

	DWORD dwErr=::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, dwMoveMethod);
	if (dwErr==INVALID_SET_FILE_POINTER)
	{
		DWORD dwErr=GetLastError();
		if (dwErr!=NO_ERROR)
		{
			LogDebug("FileReader:SetFilePointer failed:%d", dwErr);
		}
	}
	return dwErr;
}

__int64 FileReader::GetFilePointer()
{
	LARGE_INTEGER li;
	li.QuadPart = 0LL;
	li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);
	if (li.LowPart==INVALID_SET_FILE_POINTER)
	{
		DWORD dwErr=GetLastError();
		if (dwErr!=NO_ERROR)
		{
			LogDebug("FileReader:GetFilePointer failed:%d", dwErr);
		}
	}
	return li.QuadPart;
}

HRESULT FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	BOOL hr;

	if (m_hFile == INVALID_HANDLE_VALUE)
	{
		LogDebug("FileReader:Read file is not open");
		return S_FALSE;
	}
		
	//LogDebug("FileReader:Read %x ", lDataLength);
	__int64 m_filecurrent = GetFilePointer();

	__int64 length = 0;
	GetFileSize(&length);
	
	if (length < (__int64)(m_filecurrent + (__int64)lDataLength))
		hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)(length - m_filecurrent), dwReadBytes, NULL);
	else
		hr = ::ReadFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, dwReadBytes, NULL);

	if (hr==FALSE)
	{
		DWORD dwErr=GetLastError();
		LogDebug("FileReader:ReadFile failed with %d", dwErr);
		return E_FAIL;
	}

	//LogDebug("FileReader:ReadFile read %x/%x bytes size:%x pos:%x", (*dwReadBytes),lDataLength, length,m_filecurrent);
	if (*dwReadBytes < (ULONG)lDataLength)
	{
		return S_OK;
	}
	return S_OK;
}

HRESULT FileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	LogDebug("FileReader read from pos:%x move method:%x", llDistanceToMove,dwMoveMethod);
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}