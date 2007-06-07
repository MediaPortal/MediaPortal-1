/**
*  RegStore.ccp
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

// RegStore.cpp: implementation of the CRegStore class.
//
//////////////////////////////////////////////////////////////////////

#include "RegStore.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CRegStore::CRegStore(LPCSTR lpSubKey)
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

BOOL CRegStore::getSettingsInfo(CSettingsStore *setStore)
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

	if ((strcmp(setStore->getName().c_str(), "user")!=0) && (strcmp(setStore->getName().c_str(), "default")!=0))
	{
		int regSID = 0;
		datalen = 4;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "ProgramSID", NULL, &type, (BYTE*)&regSID, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setProgramSIDReg(regSID);
	}
	else
	{
		BOOL regAuto(TRUE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableAuto", NULL, &type, (BYTE*)&regAuto, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setAutoModeReg(regAuto);

		BOOL regNPControl(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableNPControl", NULL, &type, (BYTE*)&regNPControl, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setNPControlReg(regNPControl);

		BOOL regNPSlave(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableNPSlave", NULL, &type, (BYTE*)&regNPSlave, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setNPSlaveReg(regNPSlave);

		BOOL regMP2(TRUE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableMP2", NULL, &type, (BYTE*)&regMP2, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setMP2ModeReg(regMP2);

		BOOL regFixedAR(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableFixedAR", NULL, &type, (BYTE*)&regFixedAR, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setFixedAspectRatioReg(regFixedAR);

		BOOL regTSPin(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableTSPin", NULL, &type, (BYTE*)&regTSPin, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setCreateTSPinOnDemuxReg(regTSPin);

		BOOL regDelay(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableDelay", NULL, &type, (BYTE*)&regDelay, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setDelayModeReg(regDelay);

		BOOL regShared(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableSharedMode", NULL, &type, (BYTE*)&regShared, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setSharedModeReg(regShared);

		BOOL regInject(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableInjectMode", NULL, &type, (BYTE*)&regInject, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setInjectModeReg(regInject);

		BOOL regRate(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableRateControl", NULL, &type, (BYTE*)&regRate, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setRateControlModeReg(regRate);

		int regSID = 0;
		datalen = 4;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "ProgramSID", NULL, &type, (BYTE*)&regSID, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setProgramSIDReg(regSID);

		BOOL regROT(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableROT", NULL, &type, (BYTE*)&regROT, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setROTModeReg(regROT);

		int regClock = 1; //TSFileSource clock
		datalen = 4;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "clockType", NULL, &type, (BYTE*)&regClock, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setClockModeReg(regClock);

		BOOL regTxtPin(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableTxtPin", NULL, &type, (BYTE*)&regTxtPin, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setCreateTxtPinOnDemuxReg(regTxtPin);

		BOOL regSubPin(FALSE);
		datalen = 1;
		type = 0;

		resp = RegQueryValueEx(settingsKey, "enableSubPin", NULL, &type, (BYTE*)&regSubPin, &datalen);
		if(resp != ERROR_SUCCESS)
		{
			RegCloseKey(settingsKey);
			return FALSE;
		}
		setStore->setCreateSubPinOnDemuxReg(regSubPin);
	}
	
	BOOL regAC3(FALSE);//(TRUE)
	datalen = 1;
	type = 0;

	resp = RegQueryValueEx(settingsKey, "enableAC3", NULL, &type, (BYTE*)&regAC3, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setAC3ModeReg(regAC3);

	BOOL regAudio2(FALSE);
	datalen = 1;
	type = 0;

	resp = RegQueryValueEx(settingsKey, "enableAudio2", NULL, &type, (BYTE*)&regAudio2, &datalen);
	if(resp != ERROR_SUCCESS)
	{
		RegCloseKey(settingsKey);
		return FALSE;
	}
	setStore->setAudio2ModeReg(regAudio2);

	RegCloseKey(settingsKey);

	return true;
}

BOOL CRegStore::setSettingsInfo(CSettingsStore *setStore)
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


	if ((strcmp(setStore->getName().c_str(), "user")!=0) && (strcmp(setStore->getName().c_str(), "default")!=0))
	{
		int regSID = setStore->getProgramSIDReg();
		resp = RegSetValueEx(settingsKey, "ProgramSID", NULL, REG_BINARY, (BYTE*)&regSID, 4);

	}
	else
	{
		BOOL regAuto = setStore->getAutoModeReg();
		resp = RegSetValueEx(settingsKey, "enableAuto", NULL, REG_BINARY, (BYTE*)&regAuto, 1);

		BOOL regNPControl = setStore->getNPControlReg();
		resp = RegSetValueEx(settingsKey, "enableNPControl", NULL, REG_BINARY, (BYTE*)&regNPControl, 1);

		BOOL regNPSlave = setStore->getNPSlaveReg();
		resp = RegSetValueEx(settingsKey, "enableNPSlave", NULL, REG_BINARY, (BYTE*)&regNPSlave, 1);

		BOOL regMP2 = setStore->getMP2ModeReg();
		resp = RegSetValueEx(settingsKey, "enableMP2", NULL, REG_BINARY, (BYTE*)&regMP2, 1);

		BOOL regFixedAR = setStore->getFixedAspectRatioReg();
		resp = RegSetValueEx(settingsKey, "enableFixedAR", NULL, REG_BINARY, (BYTE*)&regFixedAR, 1);

		BOOL regTSPin = setStore->getCreateTSPinOnDemuxReg();
		resp = RegSetValueEx(settingsKey, "enableTSPin", NULL, REG_BINARY, (BYTE*)&regTSPin, 1);

		BOOL regDelay = setStore->getDelayModeReg();
		resp = RegSetValueEx(settingsKey, "enableDelay", NULL, REG_BINARY, (BYTE*)&regDelay, 1);

		BOOL regShared = setStore->getSharedModeReg();
		resp = RegSetValueEx(settingsKey, "enableSharedMode", NULL, REG_BINARY, (BYTE*)&regShared, 1);

		BOOL regInject = setStore->getInjectModeReg();
		resp = RegSetValueEx(settingsKey, "enableInjectMode", NULL, REG_BINARY, (BYTE*)&regInject, 1);

		BOOL regRate = setStore->getRateControlModeReg();
		resp = RegSetValueEx(settingsKey, "enableRateControl", NULL, REG_BINARY, (BYTE*)&regRate, 1);

		int regSID = setStore->getProgramSIDReg();
		resp = RegSetValueEx(settingsKey, "ProgramSID", NULL, REG_BINARY, (BYTE*)&regSID, 4);

		BOOL regROT = setStore->getROTModeReg();
		resp = RegSetValueEx(settingsKey, "enableROT", NULL, REG_BINARY, (BYTE*)&regROT, 1);

		int regClock = setStore->getClockModeReg();
		resp = RegSetValueEx(settingsKey, "clockType", NULL, REG_BINARY, (BYTE*)&regClock, 4);

		BOOL regTxtPin = setStore->getCreateTxtPinOnDemuxReg();
		resp = RegSetValueEx(settingsKey, "enableTxtPin", NULL, REG_BINARY, (BYTE*)&regTxtPin, 1);

		BOOL regSubPin = setStore->getCreateSubPinOnDemuxReg();
		resp = RegSetValueEx(settingsKey, "enableSubPin", NULL, REG_BINARY, (BYTE*)&regSubPin, 1);
	}
	BOOL regAudio2 = setStore->getAudio2ModeReg();
	resp = RegSetValueEx(settingsKey, "enableAudio2", NULL, REG_BINARY, (BYTE*)&regAudio2, 1);

	BOOL regAC3 = setStore->getAC3ModeReg();
	resp = RegSetValueEx(settingsKey, "enableAC3", NULL, REG_BINARY, (BYTE*)&regAC3, 1);

	RegCloseKey(settingsKey);

	return true;
}

BOOL CRegStore::setInt(char *name, int val)
{
	LONG result = RegSetValueEx(rootkey, name, NULL, REG_DWORD, (BYTE*)&val, 4);
  
	return TRUE;
}

int CRegStore::getInt(char *name, int def)
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

int CRegStore::getString(char *name, char *buff, int len)
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

CRegStore::~CRegStore()
{
	RegCloseKey(rootkey);
}

BOOL CRegStore::removeOld()
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
