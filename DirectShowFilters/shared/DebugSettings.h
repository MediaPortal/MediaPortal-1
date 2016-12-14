/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#pragma once
#include <cstddef>    // NULL
#include <shlobj.h>   // SHGetSpecialFolderPathW()
#include <sstream>
#include <Windows.h>  // CloseHandle(), CreateFileW(), MAX_PATH

using namespace std;


// Available debug/advanced settings
//   TsWriter:
//     TsWriterDisableCrcCheck
//     TsWriterDisableTsBufferReservation
//     TsWriterDumpInput
//   TsReader:
//     DoNotAllowSlowMotionDuringZapping
//   TsMuxer:
//     TsMuxerDumpInput
//     TsMuxerDumpOutput


#define DECLARE_DEBUG_SETTING(setting) \
  bool setting();

#define DEFINE_DEBUG_SETTING(setting, package) \
  bool setting() \
  { \
    wchar_t folder[MAX_PATH]; \
    ::SHGetSpecialFolderPathW(NULL, folder, CSIDL_COMMON_APPDATA, FALSE); \
    std::wstringstream fileName; \
    fileName << folder << L"\\Team MediaPortal\\" << package << "\\debug\\" << L#setting << L".txt"; \
    std::wstring tempFileName(fileName.str()); \
    HANDLE fileHandle = CreateFileW(tempFileName.c_str(), GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL); \
    if (fileHandle == INVALID_HANDLE_VALUE) \
    { \
      return false; \
    } \
    CloseHandle(fileHandle); \
    return true; \
  } \

#define DEFINE_MP_DEBUG_SETTING(setting)    DEFINE_DEBUG_SETTING(setting, L"MediaPortal")
#define DEFINE_TVE_DEBUG_SETTING(setting)   DEFINE_DEBUG_SETTING(setting, L"MediaPortal TV Server")
