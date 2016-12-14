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
#include "FileReader.h"
#include <string>
#include <Windows.h>  // CloseHandle(), CreateFileW(), GetLastError(), INVALID_HANDLE_VALUE, ReadFile()

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

HRESULT FileReader::Read(const wchar_t* fileName, unsigned char* data, unsigned long& dataLength)
{
  LogDebug(L"file reader: read, name = %s", fileName == NULL ? L"" : fileName);

  wstring tempFileName;
  if (fileName != NULL)
  {
    tempFileName = L"\\\\?\\";
    tempFileName += fileName;
  }

  HRESULT hr = S_OK;
  HANDLE handle = CreateFileW(tempFileName.c_str(),   // file name
                              GENERIC_READ,           // file access
                              FILE_SHARE_READ,        // share access
                              NULL,                   // security
                              OPEN_EXISTING,          // open flags
                              FILE_ATTRIBUTE_NORMAL,  // more flags
                              NULL);                  // template
  if (handle == INVALID_HANDLE_VALUE)
  {
    DWORD errorCode = GetLastError();
    hr = HRESULT_FROM_WIN32(errorCode);
    LogDebug(L"file reader: failed to open file, error = %lu, hr = 0x%x, name = %s",
              errorCode, hr, fileName);
    return hr;
  }

  unsigned long readByteCount;
  BOOL result = ReadFile(handle,
                          (void*)data,
                          dataLength,
                          &readByteCount,
                          NULL);
  dataLength = readByteCount;
  CloseHandle(handle);
  if (result != TRUE)
  {
    DWORD errorCode = GetLastError();
    hr = HRESULT_FROM_WIN32(errorCode);
    LogDebug(L"file reader: failed to read file, error = %lu, hr = 0x%x, name = %s",
              errorCode, hr, fileName);
  }
  return hr;
}