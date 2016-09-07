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
#include <Windows.h>    // DeleteFileW(), GetDiskFreeSpaceEx(), GetFileAttributesW(), GetVolumePathName(), INVALID_FILE_ATTRIBUTES, MAX_PATH
#include <WinError.h>   // HRESULT, HRESULT_FROM_WIN32()

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

class CFileUtils
{
  public:
    static void DeleteFile(const wchar_t* fileName)
    {
      ::DeleteFileW(fileName);
    }

    static bool Exists(const wchar_t* fileName)
    {
      return GetFileAttributesW(fileName) != INVALID_FILE_ATTRIBUTES;
    }

    static HRESULT GetAvailableDiskSpace(const wchar_t* path,
                                          unsigned long long& availableDiskSpace)
    {
      availableDiskSpace = 0;
      if (path == NULL || wcslen(path) < 2)
      {
        LogDebug(L"file utils: failed to get available disk space, path not supplied");
        return E_INVALIDARG;
      }

      wchar_t volumePathName[MAX_PATH + 1];
      BOOL result = GetVolumePathNameW(path, volumePathName, MAX_PATH);
      if (result == FALSE)
      {
        DWORD errorCode = GetLastError();
        HRESULT hr = HRESULT_FROM_WIN32(errorCode);
        LogDebug(L"file utils: failed to get volume path name for checking available disk space, error = %lu, hr = 0x%x",
                  errorCode, hr);
        return hr;
      }

      ULARGE_INTEGER diskSpaceAvailable;
      diskSpaceAvailable.QuadPart = 0;
      ULARGE_INTEGER diskSpaceTotal;
      diskSpaceTotal.QuadPart = 0;
      result = GetDiskFreeSpaceExW(&volumePathName[0], &diskSpaceAvailable, &diskSpaceTotal, NULL);
      if (result == FALSE)
      {
        DWORD errorCode = GetLastError();
        HRESULT hr = HRESULT_FROM_WIN32(errorCode);
        LogDebug(L"file utils: failed to get available disk space, error = %lu, hr = 0x%x",
                  errorCode, hr);
        return hr;
      }

      LogDebug(L"file utils: disk space, free = %llu bytes, total = %llu bytes",
                diskSpaceAvailable.QuadPart, diskSpaceTotal.QuadPart);
      availableDiskSpace = diskSpaceAvailable.QuadPart;
      return S_OK;
    }
};