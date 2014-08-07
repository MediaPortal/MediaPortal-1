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

wchar_t CFilterSettingsManager::logPath[MAX_PATH] = { 0 };
wchar_t CFilterSettingsManager::dllSearchPath[MAX_PATH] = { 0 };
wchar_t CFilterSettingsManager::configPath[MAX_PATH] = { 0 };

wchar_t* CFilterSettingsManager::GetLogPath()
{
	// don't read the ini file everytime
	if (!logPath || !wcscmp(logPath, L""))
	{

		GetPrivateProfileString(L"FilterSettings", L"logPath", 0, logPath, MAX_PATH, GetIniFilePath());

		// replace special folders
		wstring logPathString(logPath);
		ReplaceSpecialFolders(logPathString);
		swprintf_s(logPath, logPathString.c_str());


		// set default path if not defined inside the ini file
		if (!logPath || !wcscmp(logPath, L""))
		{
			swprintf_s(logPath, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log", GetProgramDataFolderPath());
		}
	}
	return logPath;
};

wchar_t* CFilterSettingsManager::GetDllSearchPath()
{
	// don't read the ini file everytime
	if (!dllSearchPath || !wcscmp(dllSearchPath, L""))
	{
		GetPrivateProfileString(L"FilterSettings", L"dllSearchPath", 0, dllSearchPath, MAX_PATH, GetIniFilePath());

		// replace special folders
		wstring dllSearchPathString(dllSearchPath);
		ReplaceSpecialFolders(dllSearchPathString);
		swprintf_s(dllSearchPath, dllSearchPathString.c_str());


		// set default path if not defined inside the ini file
		if (!dllSearchPath || !wcscmp(dllSearchPath, L""))
		{
			GetModuleFileName(NULL, dllSearchPath, _MAX_PATH);
			PathRemoveFileSpec(dllSearchPath);
		}
	}
	return dllSearchPath;
};
	
wchar_t* CFilterSettingsManager::GetConfigPath()
{
	// don't read the ini file everytime
	if (!configPath || !wcscmp(configPath, L""))
	{
		GetPrivateProfileString(L"FilterSettings", L"configPath", 0, configPath, MAX_PATH, GetIniFilePath());

		// replace special folders
		wstring configPathString(configPath);
		ReplaceSpecialFolders(configPathString);
		swprintf_s(configPath, configPathString.c_str());


		// set default path if not defined inside the ini file
		if (!configPath || !wcscmp(configPath, L""))
		{
			swprintf_s(configPath, L"%s\\Team MediaPortal\\MediaPortal TV Server", GetProgramDataFolderPath());
		}
	}
	return configPath;
};

bool CFilterSettingsManager::StringReplace(std::wstring& str, const std::wstring& from, const std::wstring& to)
{
	size_t start_pos = str.find(from);
	if (start_pos == std::wstring::npos)
		return false;
	str.replace(start_pos, from.length(), to);
	return true;
}

void CFilterSettingsManager::ReplaceSpecialFolders(std::wstring& path)
{
	StringReplace(path, L"%ProgramData%", GetProgramDataFolderPath());
	StringReplace(path, L"%FilterLocation%", GetFilterLocation());
}

wchar_t* CFilterSettingsManager::GetProgramDataFolderPath()
{
	static wchar_t temp[MAX_PATH];
	::SHGetSpecialFolderPathW(NULL, temp, CSIDL_COMMON_APPDATA, FALSE);
	return temp;
}

wchar_t* CFilterSettingsManager::GetFilterLocation()
{
	wchar_t DllPath[MAX_PATH] = { 0 };
	static wchar_t returnValue[MAX_PATH] = { 0 };

	// get dll path and remove the dll name from the path
	GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
	wstring DllBasePath(DllPath);
	DllBasePath = DllBasePath.substr(0, DllBasePath.find_last_of(L"\\/"));

	swprintf_s(returnValue, DllBasePath.c_str());

	return returnValue;
}

wchar_t* CFilterSettingsManager::GetIniFilePath()
{
	static wchar_t iniPath[MAX_PATH] = { 0 };

	swprintf_s(iniPath, L"%s\\%s", GetFilterLocation(), FILTERSETTINGSMANAGER_INI_FILE_NAME);

	return iniPath;
}