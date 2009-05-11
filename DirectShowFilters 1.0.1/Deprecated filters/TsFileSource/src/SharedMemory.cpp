/**
*  SharedMemory.cpp
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

#include <streams.h>
#include "SharedMemory.h"
#include <atlbase.h>
#include <Math.h>

//////////////////////////////////////////////////////////////////////
// SharedMemoryItem
//////////////////////////////////////////////////////////////////////

SharedMemoryItem::SharedMemoryItem()
{
	name = NULL;
	hFile = INVALID_HANDLE_VALUE;
	pShared_Memory = NULL;
	sharedFilePosition = 0;
	shareMode = 0;
	desiredAccess = 0;
}

SharedMemoryItem::~SharedMemoryItem()
{
	if (name)
		delete[] name;

	if (pShared_Memory)
		::UnmapViewOfFile(pShared_Memory);

	if (hFile && hFile != INVALID_HANDLE_VALUE)
		::CloseHandle(hFile);
}

//////////////////////////////////////////////////////////////////////
// SharedMemory
//////////////////////////////////////////////////////////////////////

SharedMemory::SharedMemory(__int64 maxFileSize) :
	m_bSharedMemory(TRUE),
	m_maxFileSize(maxFileSize+sizeof(SharedMemParam)),
	debugcount(0),
	m_bBuffOvld(0)
{
}

SharedMemory::~SharedMemory()
{
	Destroy();
}

HRESULT SharedMemory::Destroy()
{
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		if (*it)
		{
			ViewClearAccess((*it)->pShared_Memory, (*it)->desiredAccess);
			UpdateViewCount((*it)->pShared_Memory, -1);
			delete *it;
		}
	}
	m_ViewList.clear();

	std::vector<SharedMemoryItem *>::iterator iT = m_CreateList.begin();
	for ( ; iT < m_CreateList.end() ; iT++ )
	{
		if (*iT)
		{
			delete *iT;
		}
	}
	m_CreateList.clear();
	
	return S_OK;
}

void SharedMemory::SetBuffOvld(BOOL bBuffOvld)
{
	//if shared memory is disabled
	if (!m_bSharedMemory)
		return;

	m_bBuffOvld = bBuffOvld;
}

BOOL SharedMemory::GetBuffOvld()
{
	//if shared memory is disabled
	if (!m_bSharedMemory)
		return FALSE;

	return m_bBuffOvld;
}

void SharedMemory::SetShareSize(__int64 maxFileSize)
{
	m_maxFileSize = maxFileSize+sizeof(SharedMemParam);
}

BOOL SharedMemory::GetShareMode()
{
	return m_bSharedMemory;
}

void SharedMemory::SetShareMode(BOOL bShareMode)
{
	if (!bShareMode)
		Destroy();

	m_bSharedMemory = bShareMode;
	return;
}

BOOL IsSameName(LPCWSTR lpName1, LPCWSTR lpName2)
{
	if (lpName1 && lpName2)
		if (!wcscmp(lpName1, lpName2))
			return TRUE;

	return FALSE;
}

SharedMemParam* SharedMemory::GetSharedMemParam(LPVOID pShared_Memory)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	if (!pShared_Memory)
	{
		::SetLastError(E_POINTER);
		PrintError(TEXT("SharedMemory::GetSharedMemParam()::Pointer Error: "));
		return NULL;
	}

	CAutoLock memLock(&m_MemoryLock);
	SharedMemParam *pMemParm = new SharedMemParam();

	SharedMemParam *pMemParmLocal = (SharedMemParam *)pShared_Memory;

	int count = 0;

	while (pMemParmLocal->lock || pMemParmLocal->version != (__int64)2280)
	{
		Sleep(1);
		if (count > 1000)
		{
			if (pMemParmLocal->lock)
				::SetLastError(ERROR_OPEN_FILES);
			else
				::SetLastError(ERROR_BAD_FORMAT);

			delete pMemParm;
			return NULL;
		}
		count++;
	}

	//Lock the file
	pMemParmLocal->lock = TRUE;
	memcpy(pMemParm, pShared_Memory, sizeof(SharedMemParam));

	return pMemParm;
}

void SharedMemory::PutSharedMemParam(SharedMemParam* pMemParm, LPVOID pShared_Memory)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	if (!pShared_Memory || !pMemParm)
	{
		::SetLastError(E_POINTER);
		PrintError(TEXT("SharedMemory::PutSharedMemParam()::Pointer Error: "));
		return;
	}

CAutoLock memLock(&m_MemoryLock);
	memcpy(pShared_Memory, pMemParm, sizeof(SharedMemParam));

	pMemParm->lock = FALSE;

	//free the file
	SharedMemParam *pMemParmLocal = (SharedMemParam *)pShared_Memory;
	pMemParmLocal->lock = FALSE;
}

int SharedMemory::FindHandleCount(LPCWSTR lpFileName)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	WCHAR *name = GetSharedFileName(lpFileName);
	if (!name)
		return -1;

	CAutoLock memLock(&m_MemoryLock);
	HANDLE hFile = ::OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, name);
	delete[] name;

	if (hFile == NULL)
	{
		PrintError(TEXT("SharedMemory::FindHandleCount()::OpenFileMapping Error: "));
		return -1;
	}

	LPVOID pShared_Memory = ::MapViewOfFile(hFile, FILE_MAP_ALL_ACCESS, 0, 0, (ULONG)0);
	if (!pShared_Memory)
	{
		PrintError(TEXT("SharedMemory::FindHandleCount()::MapViewOfFile Error: "));
		::CloseHandle(hFile);
		return -1;
	}

	SharedMemParam* pMemParm = GetSharedMemParam(pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::FindHandleCount()::GetSharedMemParam Error: "));
		if (pShared_Memory)
			::UnmapViewOfFile(pShared_Memory);

		::CloseHandle(hFile);
		return -1;
	}

	int count = pMemParm->handleCount;

	//free the file
	PutSharedMemParam(pMemParm, pShared_Memory);
	delete pMemParm;

	if (pShared_Memory)
		::UnmapViewOfFile(pShared_Memory);

	::CloseHandle(hFile);

	return count;
}

BOOL SharedMemory::UpdateHandleCount(LPCWSTR lpFileName, int method)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	WCHAR *name = GetSharedFileName(lpFileName);
	if (!name)
	{
		::SetLastError(ERROR_BAD_FORMAT);
		return FALSE;
	}

	CAutoLock memLock(&m_MemoryLock);
	HANDLE hFile = ::OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, name);
	delete[] name;

	if (hFile == NULL)
	{
		PrintError(TEXT("SharedMemory::UpdateHandleCount()::OpenFileMapping Error: "));
		return FALSE;
	}

	LPVOID pShared_Memory = ::MapViewOfFile(hFile, FILE_MAP_ALL_ACCESS, 0, 0, (ULONG)0);
	if (!pShared_Memory)
	{
		PrintError(TEXT("SharedMemory::UpdateHandleCount()::MapViewOfFile Error: "));
		::CloseHandle(hFile);
		return FALSE;
	}

	SharedMemParam* pMemParm = GetSharedMemParam(pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::UpdateHandleCount()::GetSharedMemParam Error: "));
		::CloseHandle(hFile);
		return FALSE;
	}

	pMemParm->handleCount = pMemParm->handleCount + method;
	if (pMemParm->handleCount < 1)
	{
		//change the file param to null
		pMemParm->dwShareMode = 0;
		pMemParm->dwDesiredAccess = 0;
		pMemParm->dwFlagsAndAttributes = 0;
		PrintError(TEXT("SharedMemory::UpdateHandleCount()::File Is Free: "));
		pMemParm->handleCount = 0;
	}

	PutSharedMemParam(pMemParm, pShared_Memory);
	delete pMemParm;

	if (pShared_Memory)
		::UnmapViewOfFile(pShared_Memory);

	::CloseHandle(hFile);

	return TRUE;
}

int SharedMemory::UpdateViewCount(LPVOID pShared_Memory, int method)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	if (!pShared_Memory)
	{
		::SetLastError(E_POINTER);
		PrintError(TEXT("SharedMemory::UpdateViewCount()::Pointer Error: "));
		return -1;
	}

	int handleCount = -1;
CAutoLock memLock(&m_MemoryLock);

	SharedMemParam* pMemParm = GetSharedMemParam(pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::UpdateViewCount()::GetSharedMemParam Error: "));
		return -1;
	}

	pMemParm->handleCount = pMemParm->handleCount + method;
	if (pMemParm->handleCount < 1)
	{
		//change the file param to null
		pMemParm->dwShareMode = 0;
		pMemParm->dwDesiredAccess = 0;
		pMemParm->dwFlagsAndAttributes = 0;

		pMemParm->handleCount = 0;
		PrintError(TEXT("SharedMemory::UpdateViewCount()::File Is Free: "));
	}

	handleCount = pMemParm->handleCount;
	PutSharedMemParam(pMemParm, pShared_Memory);
	delete pMemParm;

	return handleCount;
}

DWORD SharedMemory::ViewClearAccess(LPVOID pShared_Memory, int accessMode)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	if (!pShared_Memory)
	{
		::SetLastError(E_POINTER);
		PrintError(TEXT("SharedMemory::ViewClearAccess()::Pointer Error: "));
		return -1;
	}

	DWORD desiredAccess = 0;
CAutoLock memLock(&m_MemoryLock);

	SharedMemParam* pMemParm = GetSharedMemParam(pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::ViewClearAccess()::GetSharedMemParam Error: "));
		return -1;
	}

	if (accessMode && (pMemParm->dwDesiredAccess&accessMode))
	{
		//change the file param to null
		pMemParm->dwDesiredAccess -= accessMode;
		PrintError(TEXT("SharedMemory::UpdateViewCount()::File Is Write Free: "));
	}

	desiredAccess = pMemParm->dwDesiredAccess;
	PutSharedMemParam(pMemParm, pShared_Memory);
	delete pMemParm;

	return desiredAccess;
}

WCHAR* SharedMemory::GetSharedFileName(LPCWSTR lpFileName)
{
	WCHAR *filename = new WCHAR[wcslen(lpFileName)+1];
	for (ULONG i = 0; i < wcslen(lpFileName)+1; i++)
	{
		if (lpFileName[i] == '\\')//92)
			filename[i] = '*';
		else
			filename[i] = lpFileName[i];
	}
	return _wcslwr(filename);
}

HANDLE  SharedMemory::openFileMapping(DWORD dwDesiredAccess, BOOL bInheritHandle, LPCWSTR lpName)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	if (!lpName)
	{
		::SetLastError(E_POINTER);
		return INVALID_HANDLE_VALUE;
	}

	//convert the file name to a format that would be allowed in sharing
	WCHAR *name = GetSharedFileName(lpName);
	if (!name)
	{
		::SetLastError(ERROR_BAD_FORMAT);
		return INVALID_HANDLE_VALUE;
	}

CAutoLock memLock(&m_MemoryLock);
	HANDLE hFile = ::OpenFileMappingW(dwDesiredAccess, bInheritHandle, name);
	delete[] name;

	if (!hFile)
	{
		PrintError(TEXT("SharedMemory::openFileMapping()::OpenFileMapping Error: "));
		::SetLastError(ERROR_OPEN_FILES);
		return INVALID_HANDLE_VALUE;
	}

	return hFile;
}

HANDLE SharedMemory::createFileMapping(HANDLE hFile,
						LPSECURITY_ATTRIBUTES lpFileMappingAttributes,
						DWORD flProtect,
						DWORD dwMaximumSizeHigh,
						DWORD dwMaximumSizeLow,
						LPCWSTR lpName)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	if (!lpName)
	{
		::SetLastError(E_POINTER);
		return INVALID_HANDLE_VALUE;
	}

	//convert the file name to a format that would be allowed in sharing
	WCHAR *name = GetSharedFileName(lpName);
	if (!name)
	{
		::SetLastError(ERROR_BAD_FORMAT);
		return INVALID_HANDLE_VALUE;
	}

CAutoLock memLock(&m_MemoryLock);
	HANDLE hfile = ::CreateFileMappingW(hFile, lpFileMappingAttributes, flProtect, dwMaximumSizeHigh, dwMaximumSizeLow, name);
	delete[] name;

	if (!hfile)
	{
		PrintError(TEXT("SharedMemory::createFileMapping()::CreateFileMapping Error: "));
		::SetLastError(ERROR_OPEN_FILES);
		return INVALID_HANDLE_VALUE;
	}

	return hfile;
}

HANDLE SharedMemory::OpenExistingFile(
						LPCWSTR lpFileName,
						DWORD dwDesiredAccess,
						DWORD dwShareMode,
						LPSECURITY_ATTRIBUTES lpSecurityAttributes,
						DWORD dwCreationDisposition,
						DWORD dwFlagsAndAttributes,
						HANDLE hTemplateFile
						)
{
	//clear any previous errors
	::SetLastError(NO_ERROR);

	//check if the file already exists
	HANDLE hFile = openFileMapping(FILE_MAP_ALL_ACCESS, FALSE, lpFileName);
	DWORD dwErr = ::GetLastError();
	if (hFile == INVALID_HANDLE_VALUE && dwErr)
	{
		if (dwErr != ERROR_OPEN_FILES)
			return hFile;
	}

	CAutoLock memLock(&m_MemoryLock);
	//if the file exists then compare the share mode
	if (hFile != INVALID_HANDLE_VALUE || !dwErr)
	{
		//open a view of the file
		LPVOID pShared_Memory = ::MapViewOfFile(hFile, FILE_MAP_ALL_ACCESS, 0, 0, (ULONG)0);
		if (pShared_Memory)
		{
			//get the file params
			SharedMemParam* pMemParm = GetSharedMemParam(pShared_Memory);
			if (pMemParm == NULL)
			{
				PrintError(TEXT("SharedMemory::OpenExistingFile()::GetSharedMemParam Error: "));
				::UnmapViewOfFile(pShared_Memory);
				::CloseHandle(hFile);
				return INVALID_HANDLE_VALUE;
			}
			else if ((pMemParm->dwShareMode == dwShareMode) ||
				((pMemParm->dwDesiredAccess == (DWORD)GENERIC_WRITE) &&
				(dwShareMode&FILE_SHARE_WRITE)))
			{
				//
				//update the file params
				//

				//create our storage item
				SharedMemoryItem *item = new SharedMemoryItem();
				item->name = GetSharedFileName(pMemParm->memID);
				item->hFile = hFile;
				item->pShared_Memory = pShared_Memory;
				item->desiredAccess = dwDesiredAccess;
				pMemParm->handleCount = pMemParm->handleCount + 1;
				PutSharedMemParam(pMemParm, pShared_Memory);
				delete pMemParm;

				CAutoLock memLock(&m_MemoryLock);
				m_ViewList.push_back(item);
				return item->hFile;
			}
			else if (dwDesiredAccess == (DWORD)GENERIC_WRITE)
			{
				//Wait for the file to become free
				int handleCount = pMemParm->handleCount;

				if (m_bBuffOvld && TRUE)
				{
					int loop = 0;
					while (handleCount > 0 && loop < 100)
					{
						loop++;
						Sleep(10);
						SharedMemParam* pMemParm2 = (SharedMemParam*)pShared_Memory;
						handleCount = pMemParm2->handleCount;
					}
					pMemParm->handleCount = 0;//handleCount; Got to get this working sometime ????????
//					pMemParm->handleCount = handleCount;
				}

				// now check if not already open
				if (pMemParm->handleCount == 0)
				{
					//
					//update the file params
					//

					//create our storage item
					SharedMemoryItem *item = new SharedMemoryItem();
					item->hFile = hFile;
					item->desiredAccess = dwDesiredAccess;
					item->pShared_Memory = pShared_Memory;
					item->name = GetSharedFileName(pMemParm->memID);
					pMemParm->handleCount = pMemParm->handleCount + 1;
					pMemParm->dwShareMode = dwShareMode;
					pMemParm->dwCreationDisposition = dwCreationDisposition;
					pMemParm->dwDesiredAccess = dwDesiredAccess;
					pMemParm->dwFlagsAndAttributes = dwFlagsAndAttributes;
					PutSharedMemParam(pMemParm, pShared_Memory);
					delete pMemParm;
					CAutoLock memLock(&m_MemoryLock);
					m_ViewList.push_back(item);
					return item->hFile;
				}
			}
			else if (dwDesiredAccess == (DWORD)GENERIC_READ)
			{
				//Wait for the file to become free
				int handleCount = pMemParm->handleCount;

				if (m_bBuffOvld && TRUE)
				{
					int loop = 0;
					while (handleCount > 0 && loop < 100)
					{
						loop++;
						Sleep(10);
						SharedMemParam* pMemParm2 = (SharedMemParam*)pShared_Memory;
						handleCount = pMemParm2->handleCount;
					}
					pMemParm->handleCount = 0;//handleCount; Got this working.
//					pMemParm->handleCount = handleCount; 
				}

				// now check if not already open
				if (pMemParm->handleCount == 0)
				{
					//
					//update the file params
					//

					//create our storage item
					SharedMemoryItem *item = new SharedMemoryItem();
					item->hFile = hFile;
					item->pShared_Memory = pShared_Memory;
					item->name = GetSharedFileName(pMemParm->memID);
					pMemParm->handleCount = pMemParm->handleCount + 1;
					pMemParm->dwShareMode = dwShareMode;
					pMemParm->dwCreationDisposition = dwCreationDisposition;
					pMemParm->dwDesiredAccess = dwDesiredAccess;
					pMemParm->dwFlagsAndAttributes = dwFlagsAndAttributes;
					PutSharedMemParam(pMemParm, pShared_Memory);
					delete pMemParm;
					CAutoLock memLock(&m_MemoryLock);
					m_ViewList.push_back(item);
					return item->hFile;
				}

			}

			PrintError(TEXT("SharedMemory::OpenExistingFile()::ShareMode Error: "));
			PutSharedMemParam(pMemParm, pShared_Memory);
			delete pMemParm;
			::UnmapViewOfFile(pShared_Memory);
		}
		PrintError(TEXT("SharedMemory::OpenExistingFile()::MapViewOfFile Error: "));
		::CloseHandle(hFile);
		::SetLastError(ERROR_SHARING_VIOLATION);
		return INVALID_HANDLE_VALUE;
	}
	return hFile;
}

HANDLE SharedMemory::CreateFile(LPCWSTR lpFileName,
								DWORD dwDesiredAccess,
								DWORD dwShareMode,
								LPSECURITY_ATTRIBUTES lpSecurityAttributes,
								DWORD dwCreationDisposition,
								DWORD dwFlagsAndAttributes,
								HANDLE hTemplateFile
								)
{
	//if shared memory is disabled
	if (!m_bSharedMemory)
		return ::CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,                  
								dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
	
	//clear any previous errors
	::SetLastError(NO_ERROR);
	DWORD dwErr = NO_ERROR;
	HANDLE hFile = INVALID_HANDLE_VALUE;

CAutoLock memLock(&m_MemoryLock);//6/1/07
	if (dwCreationDisposition == OPEN_EXISTING || dwCreationDisposition == OPEN_ALWAYS)
	{
		//return fail now if were just after a read as these are always writing
		if (dwDesiredAccess == (DWORD)GENERIC_READ && dwShareMode == (DWORD)FILE_SHARE_READ)
		{
			::SetLastError(ERROR_SHARING_VIOLATION);
			return INVALID_HANDLE_VALUE;
		}

		//check if the file already exists
		HANDLE hFile = OpenExistingFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
										dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

		//get the error and return if abnormal fail
		DWORD dwErr = ::GetLastError();
		if (hFile && !dwErr)
		{
			//check if we hold a reference in the create list
			WCHAR *sz = GetSharedFileName(lpFileName);
			SharedMemoryItem *item = NULL;
			CAutoLock memLock(&m_MemoryLock);
			std::vector<SharedMemoryItem *>::iterator it = m_CreateList.begin();
			for ( ; it < m_CreateList.end() ; it++ )
			{
				item = *it;
				if (IsSameName(item->name, sz))
				{
					delete[] sz;
					return hFile;
				}
				else
					item = NULL;
			}

			//
			//If no reference is held then create our own so we can keep playing if the source closes.
			//

			//setup security if enabled
			SECURITY_ATTRIBUTES SA_ShMem;
			PSECURITY_DESCRIPTOR pSD_ShMem;

			if (lpSecurityAttributes == NULL)
			{
				//create null security descriptor

				pSD_ShMem = (PSECURITY_DESCRIPTOR)LocalAlloc(LPTR, SECURITY_DESCRIPTOR_MIN_LENGTH);
				if (pSD_ShMem == NULL)
				{
					::SetLastError(ERROR_INVALID_HANDLE);
					return INVALID_HANDLE_VALUE;
				}
				if (!InitializeSecurityDescriptor(pSD_ShMem, SECURITY_DESCRIPTOR_REVISION))
				{
					::SetLastError(ERROR_INVALID_HANDLE);
					return INVALID_HANDLE_VALUE;
				}
				if (!SetSecurityDescriptorDacl(pSD_ShMem, TRUE, (PACL)NULL, FALSE))
				{
					::SetLastError(ERROR_INVALID_HANDLE);
					return INVALID_HANDLE_VALUE;
				}
				SA_ShMem.nLength = sizeof(SA_ShMem);
				SA_ShMem.lpSecurityDescriptor = pSD_ShMem;
				SA_ShMem.bInheritHandle = TRUE;
			}

			//create a storage item
			SharedMemoryItem *Item = new SharedMemoryItem();

			//convert the file name to a format that would be allowed in sharing
			Item->name = GetSharedFileName(lpFileName);

			if (lpSecurityAttributes == NULL)//it's in just in case
				Item->hFile = createFileMapping(INVALID_HANDLE_VALUE, &SA_ShMem, PAGE_READWRITE, 0, (ULONG)m_maxFileSize, Item->name);
			else
				Item->hFile = createFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, (ULONG)m_maxFileSize, Item->name);
			
			dwErr = ::GetLastError();
			if (Item->hFile == INVALID_HANDLE_VALUE && dwErr)
			{
				if (dwErr != ERROR_OPEN_FILES)
				{
					delete item;
					return INVALID_HANDLE_VALUE;
				}
			}

			//save our share mode
			Item->shareMode = dwShareMode;
			//store in storage array
			m_CreateList.push_back(Item);

			return hFile;
		}
		//have we have opened the file ok
		if (hFile == INVALID_HANDLE_VALUE && dwErr)
		{
			if (dwErr != ERROR_OPEN_FILES || dwDesiredAccess != GENERIC_WRITE)
				return INVALID_HANDLE_VALUE;
		}
	}

	//
	//Check if file already exists for writing return handle if already created by this source.
	//

	//return fail now if were just after a read as these are always writing
	if (dwDesiredAccess == (DWORD)GENERIC_WRITE && dwShareMode == (DWORD)(FILE_SHARE_READ | FILE_SHARE_WRITE))
	{
		//check if we hold a reference in the create list
		WCHAR *sz = GetSharedFileName(lpFileName);
		SharedMemoryItem *item = NULL;
		CAutoLock memLock(&m_MemoryLock);
		std::vector<SharedMemoryItem *>::iterator it = m_CreateList.begin();
		for ( ; it < m_CreateList.end() ; it++ )
		{
			item = *it;
			if (IsSameName(item->name, sz))
			{
				//check if the file already exists
				HANDLE hFile = OpenExistingFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
												dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

				//get the error and return if abnormal fail
				DWORD dwErr = ::GetLastError();
				if (hFile && !dwErr)
				{
					delete[] sz;
					return hFile;
				}
				else
					item = NULL;
			}
		}
	}













	//
	//Check if file already exists for writing, return fail if already open by another source.
	//

	//return fail now if were just after a read as these are always writing
	if (dwDesiredAccess == (DWORD)GENERIC_WRITE && dwShareMode == (DWORD)(FILE_SHARE_READ | FILE_SHARE_WRITE))
	{
		//check if the file already exists
		HANDLE hFile = OpenExistingFile(lpFileName, (DWORD)GENERIC_READ, dwShareMode, lpSecurityAttributes,
										dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

		//get the error and return if abnormal fail
		DWORD dwErr = ::GetLastError();
		if (hFile && !dwErr)
		{
			//check if we hold a reference in the create list
			WCHAR *sz = GetSharedFileName(lpFileName);
			SharedMemoryItem *item = NULL;
			CAutoLock memLock(&m_MemoryLock);
			std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
			for ( ; it < m_ViewList.end() ; it++ )
			{
				item = *it;
				if (IsSameName(item->name, sz))
				{
					DWORD writeflag = 0;
					delete[] sz;
					SharedMemParam* pMemParm = GetSharedMemParam(item->pShared_Memory);
					if (pMemParm == NULL)
					{
						PrintError(TEXT("SharedMemory::CreateFile()::GetSharedMemParam Error: "));
					}
					else
					{
						writeflag = pMemParm->dwDesiredAccess & (DWORD)GENERIC_WRITE;
						PutSharedMemParam(pMemParm, item->pShared_Memory);
						delete pMemParm;
					}
					hFile = INVALID_HANDLE_VALUE;
					delete item;
					m_ViewList.erase(it);
					//Is the file already open for writing
					if (writeflag)
					{
						PrintError(TEXT("SharedMemory::CreateFile()::File already open for Writing.: "));
						::SetLastError(ERROR_OPEN_FILES);
						return INVALID_HANDLE_VALUE;
					}
					break;
				}
				item = NULL;
			};
		}
	}

























	//
	//if the file does not already exist then make a new file
	//

	//setup security if enabled
	SECURITY_ATTRIBUTES SA_ShMem;
	PSECURITY_DESCRIPTOR pSD_ShMem;

	if (lpSecurityAttributes == NULL)
	{
		//create null security descriptor

		pSD_ShMem = (PSECURITY_DESCRIPTOR)LocalAlloc(LPTR, SECURITY_DESCRIPTOR_MIN_LENGTH);
		if (pSD_ShMem == NULL)
		{
			::SetLastError(ERROR_INVALID_HANDLE);
			return INVALID_HANDLE_VALUE;
		}
		if (!InitializeSecurityDescriptor(pSD_ShMem, SECURITY_DESCRIPTOR_REVISION))
		{
			::SetLastError(ERROR_INVALID_HANDLE);
			return INVALID_HANDLE_VALUE;
		}
		if (!SetSecurityDescriptorDacl(pSD_ShMem, TRUE, (PACL)NULL, FALSE))
		{
			::SetLastError(ERROR_INVALID_HANDLE);
			return INVALID_HANDLE_VALUE;
		}
		SA_ShMem.nLength = sizeof(SA_ShMem);
		SA_ShMem.lpSecurityDescriptor = pSD_ShMem;
		SA_ShMem.bInheritHandle = TRUE;
	}

	//create a storage item
	SharedMemoryItem *item = new SharedMemoryItem();

	//create a shared file header.
	SharedMemParam memParm;

	//convert the file name to a format that would be allowed in sharing
	item->name = GetSharedFileName(lpFileName);
	item->desiredAccess = dwDesiredAccess;

	//populate a file header with the file creation info.
	memParm.memStartOffset = sizeof(SharedMemParam);
	memParm.version = (__int64)2280;
	memParm.lock = FALSE;
	memParm.handleCount = 0;
	//sprintf(memParm.memID,"%s", lpFileName);
	wcscpy(memParm.memID, lpFileName);
	memParm.dwShareMode = dwShareMode;
	memParm.dwCreationDisposition = dwCreationDisposition;
	memParm.dwDesiredAccess = dwDesiredAccess;
	memParm.dwFlagsAndAttributes = dwFlagsAndAttributes;
	memParm.memMaxSize = m_maxFileSize - sizeof(SharedMemParam);
	memParm.memSize = 0;

	if (lpSecurityAttributes == NULL)//it's in just in case
		item->hFile = createFileMapping(INVALID_HANDLE_VALUE, &SA_ShMem, PAGE_READWRITE, 0, (ULONG)m_maxFileSize, item->name);
	else
		item->hFile = createFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, (ULONG)m_maxFileSize, item->name);
	
	dwErr = ::GetLastError();
	if (item->hFile == INVALID_HANDLE_VALUE && dwErr)
	{
		if (dwErr != ERROR_OPEN_FILES)
		{
			delete item;
			return INVALID_HANDLE_VALUE;
		}
	}

	//
	//if the file now exist then make a new map view
	//

	//open a view of the file
	item->pShared_Memory = ::MapViewOfFile(item->hFile, FILE_MAP_ALL_ACCESS, 0, 0, (ULONG)0);
	if (item->pShared_Memory)
	{
		//load our file param and close mapping
		PutSharedMemParam(&memParm, item->pShared_Memory);
		if (item->pShared_Memory)
			::UnmapViewOfFile(item->pShared_Memory);

		item->pShared_Memory = NULL;

		//save our share mode
		item->shareMode = dwShareMode;
		item->desiredAccess = dwDesiredAccess;

		//store in storage array
		CAutoLock memLock(&m_MemoryLock);
		m_CreateList.push_back(item);
	}
	else
	{
		PrintError(TEXT("SharedMemory::CreateFile()::MapViewOfFile Error: "));
		//this may occur if we run out of memory
		delete item; //closes the handle
		return INVALID_HANDLE_VALUE;
	}

	//open a new viewable file and save it in the view list
	hFile = OpenExistingFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
									dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

	//get the error and return if abnormal fail
	dwErr = ::GetLastError();
	if (hFile == INVALID_HANDLE_VALUE && dwErr)
	{
		delete item; //closes the handle
		PrintError(TEXT("SharedMemory::CreateFile()::OpenExistingFile Error: "));
		if (dwErr != ERROR_OPEN_FILES)
			return INVALID_HANDLE_VALUE;
	}

	//success
	return hFile;
}

DWORD SharedMemory::SetFilePointer(HANDLE hFile,
										LONG lDistanceToMove,
										PLONG lpDistanceToMoveHigh,
										DWORD dwMoveMethod
										)
{
	if (!m_bSharedMemory)
		return ::SetFilePointer(hFile, lDistanceToMove, lpDistanceToMoveHigh, dwMoveMethod);

	::SetLastError(NO_ERROR);

	if (lpDistanceToMoveHigh)
		*lpDistanceToMoveHigh = 0;

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hFile) 
			break;
		else
			item = NULL;
	}

	if (!item || item->hFile == INVALID_HANDLE_VALUE)
	{
		PrintError(TEXT("SharedMemory::SetFilePointer()::hFile Error: "));
		::SetLastError(ERROR_INVALID_HANDLE);
		return (DWORD)0xFFFFFFFF;
	}

	__int64 position = item->sharedFilePosition;

	SharedMemParam* pMemParm = GetSharedMemParam(item->pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::SetFilePointer()::GetSharedMemParam Error: "));
		return (DWORD)0xFFFFFFFF;
	}

	__int64 memSize = pMemParm->memSize;
	__int64 memMaxSize = pMemParm->memMaxSize;
	PutSharedMemParam(pMemParm, item->pShared_Memory);
	delete pMemParm;

	LARGE_INTEGER li;
	if (lpDistanceToMoveHigh)
	{
		li.LowPart = (ULONG)lDistanceToMove;
		li.HighPart = (ULONG)*lpDistanceToMoveHigh;
	}
	else
		li.QuadPart = (__int64)lDistanceToMove;

	if (dwMoveMethod == FILE_BEGIN)
		position = (__int64)li.QuadPart;
	else if (dwMoveMethod == FILE_CURRENT)
		position += (__int64)li.QuadPart;
	else if (dwMoveMethod == FILE_END)
		position = memSize + (__int64)li.QuadPart;

	if (position < 0)
	{
		PrintError(TEXT("SharedMemory::SetFilePointer()::NEGATIVE SEEK Error: "));
		::SetLastError(ERROR_NEGATIVE_SEEK);
		return (DWORD)0xFFFFFFFF;
	}
	else if (position > (DWORD)0xFFFFFFFF)
	{
		PrintError(TEXT("SharedMemory::SetFilePointer()::SEEK Error: > 0xFFFFFFF "));
	}

	item->sharedFilePosition = position;
	li.QuadPart = position;

	if (lpDistanceToMoveHigh)
	{
		*lpDistanceToMoveHigh = (ULONG)li.HighPart;
		return (DWORD)li.LowPart;
	}

	return (DWORD)position;
}

BOOL SharedMemory::SetEndOfFile(HANDLE hFile)
{
	if (!m_bSharedMemory)
		return ::SetEndOfFile(hFile);

	::SetLastError(NO_ERROR);

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hFile) 
			break;
		else
			item = NULL;
	}

	if (!item || item->hFile == NULL)
	{
		PrintError(TEXT("SharedMemory::SetEndOfFile()::hFile Error: "));
		::SetLastError(ERROR_INVALID_HANDLE);
		return FALSE;
	}

	SharedMemParam* pMemParm = GetSharedMemParam(item->pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::SetEndOfFile()::GetSharedMemParam Error: "));
		return FALSE;
	}

	if (item->sharedFilePosition > pMemParm->memMaxSize)
	{
		pMemParm->memSize = item->sharedFilePosition;
		item->sharedFilePosition = pMemParm->memMaxSize;
		PutSharedMemParam(pMemParm, item->pShared_Memory);
		delete pMemParm;
		::SetLastError(ERROR_SEEK_ON_DEVICE);
		PrintError(TEXT("SharedMemory::SetEndOfFile()::SEEK Error: "));
		return FALSE;
	}

	pMemParm->memSize = item->sharedFilePosition;
	PutSharedMemParam(pMemParm, item->pShared_Memory);
	delete pMemParm;
	return TRUE;
}

BOOL SharedMemory::WriteFile(HANDLE hFile,
								LPCVOID lpBuffer,
								DWORD nNumberOfBytesToWrite,
								LPDWORD lpNumberOfBytesWritten,
								LPOVERLAPPED lpOverlapped
								)
{
	if (!m_bSharedMemory)
		return ::WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);

	::SetLastError(NO_ERROR);

	if (lpBuffer == NULL)
	{
		PrintError(TEXT("SharedMemory::WriteFile()::Pointer Error: "));
		::SetLastError(E_POINTER);
		return FALSE;
	}

	if (lpNumberOfBytesWritten)
		*lpNumberOfBytesWritten = 0;

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hFile) 
			break;
		else
			item = NULL;
	}

	if (!item || item->hFile == NULL)
	{
		PrintError(TEXT("SharedMemory::WriteFile()::SEEK Error: "));
		::SetLastError(ERROR_INVALID_HANDLE);
		return FALSE;
	}

	if (!nNumberOfBytesToWrite)
		return TRUE;
	
	SharedMemParam* pMemParm = GetSharedMemParam(item->pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::WriteFile()::GetSharedMemParam Error: "));
		return FALSE;
	}

	if (item->sharedFilePosition > pMemParm->memMaxSize)
	{
		PutSharedMemParam(pMemParm, item->pShared_Memory);
		::SetLastError(ERROR_ALLOTTED_SPACE_EXCEEDED);
		PrintError(TEXT("SharedMemory::WriteFile()::File Size Exceeded Error: "));
		*lpNumberOfBytesWritten = 0;
		delete pMemParm;
		return FALSE;
	}

	if(item->sharedFilePosition + nNumberOfBytesToWrite > pMemParm->memMaxSize)
		*lpNumberOfBytesWritten = (ULONG)max((long)0, (long)(pMemParm->memMaxSize - item->sharedFilePosition));
	else
		*lpNumberOfBytesWritten = nNumberOfBytesToWrite;

	PBYTE mem = (BYTE*) item->pShared_Memory;
	mem += pMemParm->memStartOffset;
	mem += item->sharedFilePosition;
	memcpy(mem, lpBuffer, (UINT)(*lpNumberOfBytesWritten));

	item->sharedFilePosition += (__int64)(*lpNumberOfBytesWritten);

	if (item->sharedFilePosition > pMemParm->memSize)
		pMemParm->memSize = item->sharedFilePosition;

	PutSharedMemParam(pMemParm, item->pShared_Memory);
	delete pMemParm;
	return TRUE;

}

BOOL SharedMemory::ReadFile(HANDLE hFile,
							LPVOID lpBuffer,
							DWORD nNumberOfBytesToRead,
							LPDWORD lpNumberOfBytesRead,
							LPOVERLAPPED lpOverlapped
							)
{
	if (!m_bSharedMemory)
		return ::ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);

	::SetLastError(NO_ERROR);

	if (lpBuffer == NULL)
	{
		PrintError(TEXT("SharedMemory::ReadFile()::Pointer Error: "));
		::SetLastError(E_POINTER);
		return FALSE;
	}

	if (lpNumberOfBytesRead)
		*lpNumberOfBytesRead = 0;

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hFile) 
			break;
		else
			item = NULL;
	}

	if (!item || item->hFile == NULL)
	{
		PrintError(TEXT("SharedMemory::ReadFile()::hFile Error: "));
		::SetLastError(ERROR_INVALID_HANDLE);
		return FALSE;
	}

	if (!nNumberOfBytesToRead)
		return TRUE;
	
	SharedMemParam* pMemParm = GetSharedMemParam(item->pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::ReadFile()::GetSharedMemParam Error: "));
		return FALSE;
	}

	if (item->sharedFilePosition > pMemParm->memMaxSize)
	{
		PutSharedMemParam(pMemParm, item->pShared_Memory);
		*lpNumberOfBytesRead = 0;
		delete pMemParm;
		return TRUE;
	}

	if(item->sharedFilePosition + nNumberOfBytesToRead > pMemParm->memSize)
		*lpNumberOfBytesRead = (ULONG)max((long)0, (long)(pMemParm->memSize - item->sharedFilePosition));
	else
		*lpNumberOfBytesRead = nNumberOfBytesToRead;

	PBYTE mem = (BYTE*) item->pShared_Memory;
	mem += pMemParm->memStartOffset;
	mem += item->sharedFilePosition;
	memmove(lpBuffer, mem, (UINT)(*lpNumberOfBytesRead));

	item->sharedFilePosition += (__int64)(*lpNumberOfBytesRead);

	if (item->sharedFilePosition > (DWORD)0xFFFFFFFF)
		item->sharedFilePosition = (DWORD)0xFFFFFFFF;

	PutSharedMemParam(pMemParm, item->pShared_Memory);
	delete pMemParm;

	return TRUE;
}

DWORD SharedMemory::GetFileSize(HANDLE hFile, LPDWORD lpFileSizeHigh)
{
	if (!m_bSharedMemory)
		return ::GetFileSize(hFile, lpFileSizeHigh);

	::SetLastError(NO_ERROR);

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hFile) 
			break;
		else
			item = NULL;
	}

	if (!item || item->hFile == NULL)
	{
		PrintError(TEXT("SharedMemory::GetFileSize()::hFile Error: "));
		::SetLastError(ERROR_INVALID_HANDLE);
		return -1;
	}

	LARGE_INTEGER li;
	SharedMemParam* pMemParm = GetSharedMemParam(item->pShared_Memory);
	if (pMemParm == NULL)
	{
		PrintError(TEXT("SharedMemory::GetFileSize()::GetSharedMemParam Error: "));
		return -1;
	}

	li.QuadPart = pMemParm->memSize;
	PutSharedMemParam(pMemParm, item->pShared_Memory);
	delete pMemParm;

	if (lpFileSizeHigh)
	{
		*lpFileSizeHigh = li.HighPart;
		return (DWORD)li.LowPart;
	}
	return (DWORD) li.QuadPart;
}

BOOL SharedMemory::CloseHandle(HANDLE hObject)
{
	if (!m_bSharedMemory)
		return ::CloseHandle(hObject);

	::SetLastError(NO_ERROR);

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hObject)
		{
			ViewClearAccess((*it)->pShared_Memory, (*it)->desiredAccess);
			UpdateViewCount(item->pShared_Memory, -1);
			PrintError(TEXT("SharedMemory::CloseHandle()::Removing File From View List: "));
			delete item;
			m_ViewList.erase(it);
			return TRUE;
		}
		else
			item = NULL;
	}

	PrintError(TEXT("SharedMemory::CloseHandle()::File Not Found in View List: "));
	::SetLastError(ERROR_INVALID_HANDLE);
	return FALSE;
}

BOOL SharedMemory::FindClose(HANDLE hFindFile)
{
	if (!m_bSharedMemory)
		return ::FindClose(hFindFile);

	::SetLastError(NO_ERROR);

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_CreateList.begin();
	for ( ; it < m_CreateList.end() ; it++ )
	{
		item = *it;
		if (item->hFile == hFindFile)
			return TRUE;
		else
			item = NULL;
	}

	PrintError(TEXT("SharedMemory::FindClose()::File Not Found in View List: "));
	::SetLastError(ERROR_INVALID_HANDLE);
	return FALSE;
}

HANDLE SharedMemory::FindFirstFile(LPCWSTR lpFileName, LPWIN32_FIND_DATAW lpFindFileData)
{
	if (!m_bSharedMemory)
		return ::FindFirstFileW(lpFileName, lpFindFileData);

	::SetLastError(NO_ERROR);

	WCHAR *sz = GetSharedFileName(lpFileName);

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_CreateList.begin();
	for ( ; it < m_CreateList.end() ; it++ )
	{
		item = *it;
		if (IsSameName(item->name, sz))
		{
			PrintError(TEXT("SharedMemory::FindFirstFile()::File Found in View List: "));
			delete[] sz;
			return item->hFile;
		}
		else
			item = NULL;
	}

	// no file found in the store
	PrintError(TEXT("SharedMemory::FindFirstFile()::File Not Found in View List: "));
	delete[] sz;
	::SetLastError(ERROR_FILE_NOT_FOUND);
	return INVALID_HANDLE_VALUE;

}

BOOL SharedMemory::DeleteFile(LPCWSTR lpFileName)
{
	if (!m_bSharedMemory)
		return ::DeleteFileW(lpFileName);

	::SetLastError(NO_ERROR);

	WCHAR *sz = GetSharedFileName(lpFileName);

	SharedMemoryItem *item = NULL;
	CAutoLock memLock(&m_MemoryLock);
	std::vector<SharedMemoryItem *>::iterator it = m_ViewList.begin();
	for ( ; it < m_ViewList.end() ; it++ )
	{
		item = *it;
		if (IsSameName(item->name, sz))
		{
			//We have viewmaps still open so exit
			::SetLastError(ERROR_ACCESS_DENIED);
			PrintError(TEXT("SharedMemory::DeleteFile()::File Still Open In View List: "));
			delete[] sz;
			return FALSE;
		}
		else
			item = NULL;
	}

	//if where here then we have no open view handles so we can now delete the creation
	std::vector<SharedMemoryItem *>::iterator iT = m_CreateList.begin();
	for ( ; iT < m_CreateList.end() ; iT++ )
	{
		item = *iT;
		if (IsSameName(item->name, sz))
		{
			PrintError(TEXT("SharedMemory::DeleteFile()::File Deleted: "));
			delete[] sz;
			delete item;
			m_CreateList.erase(iT);
			return TRUE;
		}
		else
			item = NULL;
	}

	PrintError(TEXT("SharedMemory::DeleteFile()::File Not Found In Lists: "));
	//if we are here then the created map doesn't exist 
	delete[] sz;
	::SetLastError(ERROR_FILE_NOT_FOUND);
	return FALSE;
}

BOOL SharedMemory::FlushFileBuffers(HANDLE hFile)
{
	if (!m_bSharedMemory)
		return ::FlushFileBuffers(hFile);

	return TRUE;
}

BOOL SharedMemory::GetDiskFreeSpaceEx(LPCSTR lpDirectoryName,
						PULARGE_INTEGER lpFreeBytesAvailableToCaller,
						PULARGE_INTEGER lpTotalNumberOfBytes,
						PULARGE_INTEGER lpTotalNumberOfFreeBytes
						)
{
	if (!m_bSharedMemory)
		return ::GetDiskFreeSpaceEx(lpDirectoryName, lpFreeBytesAvailableToCaller,
						lpTotalNumberOfBytes, lpTotalNumberOfFreeBytes);

	if (!lpFreeBytesAvailableToCaller)
		return FALSE;

	ULARGE_INTEGER uliSharedSpaceAvailable;
	uliSharedSpaceAvailable.QuadPart = 0;
	*lpFreeBytesAvailableToCaller = uliSharedSpaceAvailable;

	::SetLastError(NO_ERROR);

	MEMORYSTATUS memStatus;  
 	memStatus.dwLength=sizeof(MEMORYSTATUS);  
	memStatus.dwTotalPhys = 0;
	GlobalMemoryStatus(&memStatus);

	//check if failed to populate 
	if (!memStatus.dwTotalPhys)
		return FALSE;
	
	//convert to 64MB Blocks
	__int64 size = (__int64)((__int64)memStatus.dwAvailPhys/(__int64)(1048576*64));

	//convert back to bytes
	size = (__int64)(size*(__int64)(1048576*64));

	uliSharedSpaceAvailable.QuadPart = size;  
	*lpFreeBytesAvailableToCaller = uliSharedSpaceAvailable;  
	PrintLongLong(NAME("SharedMemory::GetDiskFreeSpaceEx returned:"),size);
	return TRUE;
}

void SharedMemory::PrintError(LPCTSTR lstring)
{
	if (!lstring)
		return;

	CHAR sz[MAX_PATH];
	LPVOID pMsg;
	FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | 
					FORMAT_MESSAGE_FROM_SYSTEM | 
					FORMAT_MESSAGE_IGNORE_INSERTS,
					NULL,
					::GetLastError(),
					MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), 
					(LPTSTR) &pMsg,
					0,
					NULL);
	if (!pMsg)
		return;

	sprintf(sz, TEXT("%05i - %s %s\n"), debugcount, lstring, pMsg);
	::OutputDebugString(sz);
	LocalFree(pMsg);
}

void SharedMemory::PrintLongLong(LPCTSTR lstring, __int64 value)
{
	CHAR sz[100];
	double dVal = (double)value;
	double len = log10(dVal);
	int pos = (int)len;
	sz[pos+1] = '\0';
	while (pos >= 0)
	{
		int val = (int)(value % 10);
		sz[pos] = '0' + val;
		value /= 10;
		pos--;
	}
	CHAR szout[100];
	wsprintf(szout, TEXT("%05i - %s %s\n"), debugcount, lstring, sz);
	::OutputDebugString(szout);
	debugcount++;
}


