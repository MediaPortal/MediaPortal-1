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
	TCHAR memID[MAX_PATH];
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

	TCHAR *name;
	HANDLE hFile;
	DWORD shareMode;
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

	BOOL IsSame(LPCSTR lpName1, LPCSTR lpName2);
	int FindHandleCount(LPCSTR lpFileName);
	BOOL UpdateHandleCount(LPCSTR lpFileName, int method);
	int UpdateViewCount(LPVOID pShared_Memory, int method);
	SharedMemParam* GetSharedMemParam(LPVOID pShared_Memory);
	void PutSharedMemParam(SharedMemParam* pMemParm, LPVOID pShared_Memory);
	TCHAR* GetSharedFileName(LPCSTR lpFileName);

	HANDLE openFileMapping(DWORD dwDesiredAccess, BOOL bInheritHandle, LPCSTR lpName);
	HANDLE createFileMapping(HANDLE hFile,
							LPSECURITY_ATTRIBUTES lpFileMappingAttributes,
							DWORD flProtect,
							DWORD dwMaximumSizeHigh,
							DWORD dwMaximumSizeLow,
							LPCSTR lpName);

	HANDLE SharedMemory::OpenExistingFile(
							LPCSTR lpFileName,
							DWORD dwDesiredAccess,
							DWORD dwShareMode,
							LPSECURITY_ATTRIBUTES lpSecurityAttributes,
							DWORD dwCreationDisposition,
							DWORD dwFlagsAndAttributes,
							HANDLE hTemplateFile
							);

	//WIN32 API for shared memory
	HANDLE CreateFile(
				LPCSTR lpFileName,
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
	HANDLE FindFirstFile(LPCSTR lpFileName, LPWIN32_FIND_DATAA lpFindFileData);
	BOOL DeleteFile(LPCSTR lpFileName);
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
