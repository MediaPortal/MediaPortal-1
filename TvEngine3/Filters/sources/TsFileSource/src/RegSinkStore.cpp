/**
*  RegSinkStore.ccp
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2003  Shaun Faulds
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
*  bear can be reached on the forums at
*    http://forums.dvbowners.com/
*/

// RegSinkStore.cpp: implementation of the RegSinkStore class.
//
//////////////////////////////////////////////////////////////////////

#include "RegSinkStore.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CRegSinkStore::CRegSinkStore(LPCSTR lpSubKey)
{
	LONG resp = 0;
	DWORD action_result = 0;

	resp = RegCreateKeyEx(	HKEY_CURRENT_USER, //HKEY_LOCAL_MACHINE,
							lpSubKey,
							NULL,
							NULL,
							REG_OPTION_NON_VOLATILE,
							KEY_ALL_ACCESS,
							NULL,
							&rootkey,
							&action_result);

	removeOld();

}

BOOL CRegSinkStore::getSettingsInfo(CSettingsSinkStore *setStore)
{
	std::string keyName = "settings\\" + setStore->getName();

	LONG resp = 0;
	DWORD action_result = 0;
	HKEY settingsKey;

	resp = RegCreateKeyEx(	rootkey,
							keyName.c_str(),
							NULL,
							NULL,
							REG_OPTION_NON_VOLATILE,
							KEY_ALL_ACCESS,
							NULL,
							&settingsKey,
							&action_result);

	if(REG_CREATED_NEW_KEY == action_result)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	DWORD datalen;
	DWORD type;

	TCHAR szFileName[MAX_PATH] ="MyBufferFile";
	datalen = MAX_PATH;
	type = REG_SZ;

	resp = RegQueryValueEx(settingsKey, "DefaultName", NULL, &type, (BYTE*)szFileName, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setRegFileNameReg(szFileName);

	long lMinFiles(6);
	datalen = 4;
	type = REG_NONE;

	resp = RegQueryValueEx(settingsKey, "MinNumbFiles", NULL, &type, (BYTE*)&lMinFiles, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setMinTSFilesReg(lMinFiles);

	long lMaxFiles(60);
	datalen = 4;
	type = REG_NONE;

	resp = RegQueryValueEx(settingsKey, "MaxNumbFiles", NULL, &type, (BYTE*)&lMaxFiles, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setMaxTSFilesReg(lMaxFiles);

	__int64 llMaxSize((__int64) ((__int64)1048576 *(__int64)250)); //250MB
	datalen = 8;
	type = REG_NONE;

	resp = RegQueryValueEx(settingsKey, "MaxFileSize", NULL, &type, (BYTE*)&llMaxSize, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setMaxTSFileSizeReg(llMaxSize);

	__int64 llChunkSize((__int64) ((__int64)1048576 *(__int64)250)); //250MB
	datalen = 8;
	type = REG_NONE;

	resp = RegQueryValueEx(settingsKey, "ChunkSize", NULL, &type, (BYTE*)&llChunkSize, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setChunkReserveReg(llChunkSize);

	RegCloseKey(settingsKey);

	return true;
}

BOOL CRegSinkStore::setSettingsInfo(CSettingsSinkStore *setStore)
{
	std::string keyName = "settings\\" + setStore->getName();

	LONG resp = 0;
	DWORD action_result = 0;
	HKEY settingsKey;

	resp = RegCreateKeyEx(	rootkey,
							keyName.c_str(),
							NULL,
							NULL,
							REG_OPTION_NON_VOLATILE,
							KEY_ALL_ACCESS,
							NULL,
							&settingsKey,
							&action_result);

	std::string szFileName = setStore->getRegFileNameReg();
	resp = RegSetValueEx(settingsKey, "DefaultName", NULL, REG_SZ, (BYTE*)szFileName.c_str(), MAX_PATH);

	long lMinFiles = setStore->getMinTSFilesReg();
	resp = RegSetValueEx(settingsKey, "MinNumbFiles", NULL, REG_BINARY, (BYTE*)&lMinFiles, 4);

	long lMaxFiles = setStore->getMaxTSFilesReg();
	resp = RegSetValueEx(settingsKey, "MaxNumbFiles", NULL, REG_BINARY, (BYTE*)&lMaxFiles, 4);

	__int64 llMaxSize = setStore->getMaxTSFileSizeReg();
	resp = RegSetValueEx(settingsKey, "MaxFileSize", NULL, REG_BINARY, (BYTE*)&llMaxSize, 8);

	__int64 llChunkSize = setStore->getChunkReserveReg();
	resp = RegSetValueEx(settingsKey, "ChunkSize", NULL, REG_BINARY, (BYTE*)&llChunkSize, 8);

	RegCloseKey(settingsKey);

	return true;
}

BOOL CRegSinkStore::setInt(char *name, int val)
{
	LONG result = RegSetValueEx(rootkey, name, NULL, REG_DWORD, (BYTE*)&val, 4);
  
	return TRUE;
}

int CRegSinkStore::getInt(char *name, int def)
{
	int val = 0;
	DWORD datalen = 4;
	DWORD type = 0;

	LONG resp = RegQueryValueEx(rootkey, name, NULL, &type, (BYTE*)&val, &datalen);

	if(resp == 2)
	{
		val = def;
		RegSetValueEx(rootkey, name, NULL, REG_DWORD, (BYTE*)&val, 4);
	}

	return val;
}

int CRegSinkStore::getString(char *name, char *buff, int len)
{
	DWORD datalen = len-1;
	DWORD type = 0;

	LONG resp = RegQueryValueEx(rootkey, name, NULL, &type, (BYTE*)buff, &datalen);

	if(resp == 2)
	{
		char *val = 0;
		RegSetValueEx(rootkey, name, NULL, REG_SZ, (BYTE*)&val, 1);
	}

	return datalen;
}

CRegSinkStore::~CRegSinkStore()
{
	RegCloseKey(rootkey);
}

BOOL CRegSinkStore::removeOld()
{
	LONG resp = 0;
	DWORD action_result = 0;
	HKEY settingsKey;

	resp = RegCreateKeyEx(	rootkey,
							"settings",
							NULL,
							NULL,
							REG_OPTION_NON_VOLATILE,
							KEY_ALL_ACCESS,
							NULL,
							&settingsKey,
							&action_result);

	if(resp != ERROR_SUCCESS)
	{
		return false;
	}

	std::vector <LPTSTR> toRemove;

	TCHAR buff[256];
	int index = 0;

	while(RegEnumKey(settingsKey, index++, buff, 256) == ERROR_SUCCESS)
	{
		HKEY settingsData;
		resp = RegCreateKeyEx(	settingsKey,
								buff,
								NULL,
								NULL,
								REG_OPTION_NON_VOLATILE,
								KEY_ALL_ACCESS,
								NULL,
								&settingsData,
								&action_result);

		if(resp == ERROR_SUCCESS)
		{
			DWORD datalen = 8;
			DWORD type = 0;
			__int64 lastUsed = 0;
			resp = RegQueryValueEx(settingsData, "lastUsed", NULL, &type, (BYTE*)&lastUsed, &datalen);
			if(resp == ERROR_SUCCESS)
			{
				__int64 now = time(NULL);
				if(lastUsed < (now - (3600 * 24 * 30)))
				{
					LPTSTR pStr = new TCHAR[256];
					lstrcpy(pStr, buff);
                    toRemove.push_back(pStr);
				}
			}

			RegCloseKey(settingsData);
		}
	}

	//
	// Now remove old items
	//
	LPTSTR pName = "";
	for(int x = 0; x < (int)toRemove.size(); x++)
	{
		pName = toRemove.at(x);
		RegDeleteKey(settingsKey, pName);
		delete[] pName;
	}

	RegCloseKey(settingsKey);

	return TRUE;
}
