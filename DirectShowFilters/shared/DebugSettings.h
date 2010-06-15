/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#pragma once

// Available debug/advanced settings
//   TsWriter:
//     DisableCRCCheck
//     DumpRawTS
//   TsReader:
//     DoNotAllowSlowMotionDuringZapping


#define DECLARE_DEBUG_SETTING(setting)  \
  bool setting();

#define DEFINE_DEBUG_SETTING(setting, package) \
  bool setting() \
  { \
    TCHAR folder[MAX_PATH]; \
    TCHAR fileName[MAX_PATH]; \
    ::SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE); \
    _stprintf(fileName, _T("%s\\Team MediaPortal\\" package  "\\debug\\" #setting ".txt"), folder); \
    HANDLE hTest=CreateFile(fileName,(DWORD) GENERIC_READ,0,0,(DWORD) OPEN_EXISTING,0,NULL); \
    if (hTest==INVALID_HANDLE_VALUE) \
		  return false; \
    CloseHandle(hTest); \
	  return TRUE; \
  } \

#define DEFINE_MP_DEBUG_SETTING(setting)    DEFINE_DEBUG_SETTING(setting, "MediaPortal")
#define DEFINE_TVE_DEBUG_SETTING(setting)   DEFINE_DEBUG_SETTING(setting, "MediaPortal TV Server")
