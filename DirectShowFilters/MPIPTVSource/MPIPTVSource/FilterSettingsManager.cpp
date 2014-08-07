/*
*  Copyright (C) 2005-2013 Team MediaPortal
*  http://www.team-mediaportal.com
*
*  This Program is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2, or (at your option)
*  any later version.
*
*  This Program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with GNU Make; see the file COPYING.  If not, write to
*  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
*  http://www.gnu.org/copyleft/gpl.html
*
*/
#include "StdAfx.h"
#include "FilterSettingsmanager.h"

using namespace std;

TCHAR CFilterSettingsManager::logPath[MAX_PATH] = { 0 };
TCHAR CFilterSettingsManager::dllSearchPath[MAX_PATH] = { 0 };
TCHAR CFilterSettingsManager::configPath[MAX_PATH] = { 0 };

TCHAR* CFilterSettingsManager::GetLogPath()
{
	// don't read the ini file everytime
	if (!logPath || !strcmp(logPath, ""))
	{

		GetPrivateProfileString("FilterSettings", "logPath", 0, logPath, MAX_PATH, GetIniFilePath());

		// replace special folders
		string logPathString(logPath);
		ReplaceSpecialFolders(logPathString);
		sprintf_s(logPath, logPathString.c_str());


		// set default path if not defined inside the ini file
		if (!logPath || !strcmp(logPath, ""))
		{
			sprintf_s(logPath, "%s\\Team MediaPortal\\MediaPortal TV Server\\log", GetProgramDataFolderPath());
		}
	}
	return logPath;
};

TCHAR* CFilterSettingsManager::GetDllSearchPath()
{
	// don't read the ini file everytime
	if (!dllSearchPath || !strcmp(dllSearchPath, ""))
	{
		GetPrivateProfileString("FilterSettings", "dllSearchPath", 0, dllSearchPath, MAX_PATH, GetIniFilePath());

		// replace special folders
		string dllSearchPathString(dllSearchPath);
		ReplaceSpecialFolders(dllSearchPathString);
		sprintf_s(dllSearchPath, dllSearchPathString.c_str());


		// set default path if not defined inside the ini file
		if (!dllSearchPath || !strcmp(dllSearchPath, ""))
		{
			GetModuleFileName(NULL, dllSearchPath, _MAX_PATH);
			PathRemoveFileSpec(dllSearchPath);
		}
	}
	return dllSearchPath;
};
	
TCHAR* CFilterSettingsManager::GetConfigPath()
{
	// don't read the ini file everytime
	if (!configPath || !strcmp(configPath, ""))
	{
		GetPrivateProfileString("FilterSettings", "configPath", 0, configPath, MAX_PATH, GetIniFilePath());

		// replace special folders
		string configPathString(configPath);
		ReplaceSpecialFolders(configPathString);
		sprintf_s(configPath, configPathString.c_str());


		// set default path if not defined inside the ini file
		if (!configPath || !strcmp(configPath, ""))
		{
			sprintf_s(configPath, "%s\\Team MediaPortal\\MediaPortal TV Server", GetProgramDataFolderPath());
		}
	}
	return configPath;
};

bool CFilterSettingsManager::StringReplace(std::string& str, const std::string& from, const std::string& to)
{
	size_t start_pos = str.find(from);
	if (start_pos == std::string::npos)
		return false;
	str.replace(start_pos, from.length(), to);
	return true;
}

void CFilterSettingsManager::ReplaceSpecialFolders(std::string& path)
{
	StringReplace(path, "%ProgramData%", GetProgramDataFolderPath());
	StringReplace(path, "%FilterLocation%", GetFilterLocation());
}

TCHAR* CFilterSettingsManager::GetProgramDataFolderPath()
{
	static TCHAR temp[MAX_PATH];
	::SHGetSpecialFolderPath(NULL, temp, CSIDL_COMMON_APPDATA, FALSE);
	return temp;
}

TCHAR* CFilterSettingsManager::GetFilterLocation()
{
	TCHAR DllPath[MAX_PATH] = { 0 };
	static TCHAR returnValue[MAX_PATH] = { 0 };

	// get dll path and remove the dll name from the path
	GetModuleFileName((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
	string DllBasePath(DllPath);
	DllBasePath = DllBasePath.substr(0, DllBasePath.find_last_of("\\/"));

	sprintf_s(returnValue, DllBasePath.c_str());

	return returnValue;
}

TCHAR* CFilterSettingsManager::GetIniFilePath()
{
	static TCHAR iniPath[MAX_PATH] = { 0 };

	sprintf_s(iniPath, "%s\\%s", GetFilterLocation(), FILTERSETTINGSMANAGER_INI_FILE_NAME);

	return iniPath;
}