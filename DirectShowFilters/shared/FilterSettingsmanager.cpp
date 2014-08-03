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

#include "FilterSettingsmanager.h"

wchar_t FilterSettingsmanager_logPath[MAX_PATH] = { 0 };

static wchar_t* FilterSettingsmanager_getLogPath() {
	// don't read the ini file everytime
	if (!FilterSettingsmanager_logPath || !wcscmp(FilterSettingsmanager_logPath, L"")) {
		wchar_t DllPath[MAX_PATH] = { 0 };
		wchar_t iniPath[MAX_PATH] = { 0 };

		// get dll path and remove the dll name from the path
		GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
		wstring DllBasePath(DllPath);
		DllBasePath = DllBasePath.substr(0, DllBasePath.find_last_of(L"\\/"));

		swprintf_s(iniPath, L"%s\\%s", DllBasePath.c_str(), FILTERSETTINGSMANAGER_INI_FILE_NAME);
		GetPrivateProfileString(L"FilterSettings", L"logPath", 0, FilterSettingsmanager_logPath, MAX_PATH, iniPath);

		// set default path if not defined inside the ini file
		if (!FilterSettingsmanager_logPath || !wcscmp(FilterSettingsmanager_logPath, L"")) {
			wchar_t temp[MAX_PATH];
			::SHGetSpecialFolderPathW(NULL, temp, CSIDL_COMMON_APPDATA, FALSE);
			swprintf_s(FilterSettingsmanager_logPath, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log", temp);
		}
	}
	return FilterSettingsmanager_logPath;
};

static wchar_t* FilterSettingsmanager_getDllSearchPath() {
	
};
	
static wchar_t* FilterSettingsmanager_getConfigPath() {

};