/**
*  SharedMemory.h
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

#ifndef SHAREDMEMORY_H
#define SHAREDMEMORY_H

#ifndef INVALID_SET_FILE_POINTER
#define INVALID_SET_FILE_POINTER -1
#endif

#include <vector>

typedef struct 
{
	__int64	memStartOffset;
	BOOL lock;
	int handleCount;
	__int64	memSize;
	__int64	memMaxSize;
	DWORD dwDesiredAccess;
	DWORD dwShareMode;
	DWORD dwCreationDisposition;
	DWORD dwFlagsAndAttributes;
	WCHAR memID[MAX_PATH];
	__int64 version;
} SharedMemParam;

//////////////////////////////////////////////////////////////////////
// SharedMemory
//////////////////////////////////////////////////////////////////////

class SharedMemoryItem
{
public:

	SharedMemoryItem();
	virtual ~SharedMemoryItem();

	WCHAR *name;
	HANDLE hFile;
	DWORD shareMode;
	DWORD desiredAccess;
	void * pShared_Memory;
	__int64	sharedFilePosition;
};

//////////////////////////////////////////////////////////////////////
// SharedMemory
//////////////////////////////////////////////////////////////////////

class SharedMemory
{
public:

	SharedMemory(__int64 maxFileSize);
	virtual ~SharedMemory();

	HRESULT Destroy();
	void SetBuffOvld(BOOL bBuffOvld);
	BOOL GetBuffOvld();
	void SetShareSize(__int64 maxFileSize);
	BOOL GetShareMode();
	void SetShareMode(BOOL bShareMode);

	BOOL IsSame(LPCWSTR lpName1, LPCWSTR lpName2);
	int FindHandleCount(LPCWSTR lpFileName);
	BOOL UpdateHandleCount(LPCWSTR lpFileName, int method);
	int UpdateViewCount(LPVOID pShared_Memory, int method);
	DWORD ViewClearAccess(LPVOID pShared_Memory, int accessMode);
	SharedMemParam* GetSharedMemParam(LPVOID pShared_Memory);
	void PutSharedMemParam(SharedMemParam* pMemParm, LPVOID pShared_Memory);
	WCHAR* GetSharedFileName(LPCWSTR lpFileName);

	HANDLE openFileMapping(DWORD dwDesiredAccess, BOOL bInheritHandle, LPCWSTR lpName);
	HANDLE createFileMapping(HANDLE hFile,
							LPSECURITY_ATTRIBUTES lpFileMappingAttributes,
							DWORD flProtect,
							DWORD dwMaximumSizeHigh,
							DWORD dwMaximumSizeLow,
							LPCWSTR lpName);

	HANDLE SharedMemory::OpenExistingFile(
							LPCWSTR lpFileName,
							DWORD dwDesiredAccess,
							DWORD dwShareMode,
							LPSECURITY_ATTRIBUTES lpSecurityAttributes,
							DWORD dwCreationDisposition,
							DWORD dwFlagsAndAttributes,
							HANDLE hTemplateFile
							);

	//WIN32 API for shared memory
	HANDLE CreateFile(
				LPCWSTR lpFileName,
				DWORD dwDesiredAccess,
				DWORD dwShareMode,
				LPSECURITY_ATTRIBUTES lpSecurityAttributes,
				DWORD dwCreationDisposition,
				DWORD dwFlagsAndAttributes,
				HANDLE hTemplateFile
				);

	DWORD SetFilePointer(
				HANDLE hFile,
				LONG lDistanceToMove,
				PLONG lpDistanceToMoveHigh,
				DWORD dwMoveMethod
				);

	BOOL SetEndOfFile(HANDLE hFile);

	BOOL WriteFile(HANDLE hFile,
					LPCVOID lpBuffer,
					DWORD nNumberOfBytesToWrite,
					LPDWORD lpNumberOfBytesWritten,
					LPOVERLAPPED lpOverlapped
					);

	BOOL ReadFile(HANDLE hFile,
					LPVOID lpBuffer,
					DWORD nNumberOfBytesToRead,
					LPDWORD lpNumberOfBytesRead,
					LPOVERLAPPED lpOverlapped
					);

	DWORD GetFileSize(HANDLE hFile, LPDWORD lpFileSizeHigh);
	BOOL CloseHandle(HANDLE hObject);
	BOOL FindClose(HANDLE hFindFile);
	HANDLE FindFirstFile(LPCWSTR lpFileName, LPWIN32_FIND_DATAW lpFindFileData);
	BOOL DeleteFile(LPCWSTR lpFileName);
	BOOL FlushFileBuffers(HANDLE hFile);

	BOOL GetDiskFreeSpaceEx(LPCSTR lpDirectoryName,
							PULARGE_INTEGER lpFreeBytesAvailableToCaller,
							PULARGE_INTEGER lpTotalNumberOfBytes,
							PULARGE_INTEGER lpTotalNumberOfFreeBytes
							);

protected:

	void PrintError(LPCTSTR lstring);
	void PrintLongLong(LPCTSTR lstring, __int64 value);

	int debugcount;
	std::vector<SharedMemoryItem *> m_ViewList;
	std::vector<SharedMemoryItem *> m_CreateList;
	CCritSec m_MemoryLock;


	BOOL m_bSharedMemory;
	__int64 m_maxFileSize;
	BOOL m_bBuffOvld;
};

#endif
