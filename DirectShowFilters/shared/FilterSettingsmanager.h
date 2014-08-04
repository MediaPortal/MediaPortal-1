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

#include <Windows.h>
#include "WinDef.h"
#include "ShlObj.h"
#include <stdio.h>

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

#define FILTERSETTINGSMANAGER_INI_FILE_NAME L"filterConfig.ini"

class CFilterSettingsManager
{
	public:
		static wchar_t* GetLogPath();
		static wchar_t* GetDllSearchPath();
		static wchar_t* GetConfigPath();
	private:
		static wchar_t logPath[MAX_PATH];

		static bool StringReplace(std::wstring& str, const std::wstring& from, const std::wstring& to);
		static wchar_t* CFilterSettingsManager::GetProgramDataFolderPath();
};