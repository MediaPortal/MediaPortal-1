/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "Utilities.h"
#include "Parameters.h"

#include <ObjBase.h>
#include <ShlObj.h>
#include <stdio.h>

wchar_t *GetTvServerFolder(void)
{
  return GetTvServerFilePath(NULL);
}

wchar_t *GetTvServerFilePath(const wchar_t *filePath)
{
  wchar_t *result = NULL;
  ALLOC_MEM_DEFINE_SET(folder, wchar_t, MAX_PATH, 0);
  if (folder != NULL)
  {
    // get common application data folder
    SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);

    if (filePath == NULL)
    {
      result = FormatString(L"%s\\Team MediaPortal\\MediaPortal TV Server\\", folder);
    }
    else
    {
      result = FormatString(L"%s\\Team MediaPortal\\MediaPortal TV Server\\%s", folder, filePath);
    }
  }

  FREE_MEM(folder);

  return result;
}

wchar_t *GetMediaPortalFolder(void)
{
  return GetMediaPortalFilePath(NULL);
}

wchar_t *GetMediaPortalFilePath(const wchar_t *filePath)
{
  wchar_t *result = NULL;
  ALLOC_MEM_DEFINE_SET(folder, wchar_t, MAX_PATH, 0);
  if (folder != NULL)
  {
    // get common application data folder
    SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);

    if (filePath == NULL)
    {
      result = FormatString(L"%s\\Team MediaPortal\\MediaPortal\\", folder);
    }
    else
    {
      result = FormatString(L"%s\\Team MediaPortal\\MediaPortal\\%s", folder, filePath);
    }
  }

  FREE_MEM(folder);

  return result;
}

void ZeroURL(URL_COMPONENTS *url) 
{
  url->lpszScheme = NULL;
  url->lpszExtraInfo = NULL;
  url->lpszHostName = NULL;
  url->lpszPassword = NULL;
  url->lpszUrlPath = NULL;
  url->lpszUserName = NULL;

  url->dwSchemeLength = 1;
  url->dwHostNameLength = 1;
  url->dwUrlPathLength = 1;
  url->dwExtraInfoLength = 1;
  url->dwPasswordLength = 1;
  url->dwUrlPathLength = 1;
  url->dwUserNameLength = 1;
}

wchar_t *GetVersionInfo(const wchar_t *commit, const wchar_t *date)
{
  return FormatString(L"Commit: %s Build date: %s", commit, date);
}

wchar_t *GetStoreFilePath(const wchar_t *folder, CParameterCollection *configuration)
{
  wchar_t *result = NULL;
  ALLOC_MEM_DEFINE_SET(storeFolder, wchar_t, MAX_PATH, 0);
  if (folder != NULL)
  {
    // check if we have path in configuration
    const wchar_t *cacheFolder = configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);
    if (cacheFolder == NULL)
    {
      // get new folder in local app data
      // get common application data folder
      if (SHGetSpecialFolderPath(NULL, storeFolder, CSIDL_COMMON_APPDATA, FALSE))
      {
        wcscat_s(storeFolder, MAX_PATH, L"\\Team MediaPortal\\");
        wcscat_s(storeFolder, MAX_PATH, folder);
        wcscat_s(storeFolder, MAX_PATH, L"\\");
      }
    }
    else
    {
      // copy cache folder to folder
      wcscat_s(storeFolder, MAX_PATH, cacheFolder);
    }

    unsigned int length = wcslen(storeFolder);
    if ((length > 0) && (storeFolder[length - 1] != L'\\'))
    {
      // append last '\' if not already in path
      wcscat_s(storeFolder, MAX_PATH, L"\\");
    }

    length = wcslen(storeFolder);
    if (length > 0)
    {
      // there is something in folder variable
      // create directory path
      int error = SHCreateDirectory(NULL, storeFolder);
      if ((error == ERROR_SUCCESS) || (error == ERROR_FILE_EXISTS) || (error == ERROR_ALREADY_EXISTS))
      {
        // correct, directory exists
        result = storeFolder;
      }
    }
  }

  if (result == NULL)
  {
    // error occured, but folder can be allocated
    FREE_MEM(storeFolder);
  }

  return result;
}

typedef struct tagTHREADNAME_INFO
{
   DWORD dwType;            // must be 0x1000
   LPCSTR szName;           // pointer to name (in user addr space)
   DWORD dwThreadID;        // thread ID (-1=caller thread)
   DWORD dwFlags;           // reserved for future use, must be zero
} THREADNAME_INFO;

void SetThreadName( DWORD dwThreadID, LPCSTR szThreadName)
{
   THREADNAME_INFO info;
   info.dwType = 0x1000;
   info.szName = szThreadName;
   info.dwThreadID = dwThreadID;
   info.dwFlags = 0;

   __try
   {
      RaiseException(0x406D1388, 0, sizeof(info)/sizeof(DWORD), (ULONG_PTR*)&info);
   }
   __except(EXCEPTION_CONTINUE_EXECUTION)
   {
   }
}

unsigned int GreatestCommonDivisor(unsigned int u, unsigned int v)
{
  unsigned int shift;
 
  /* GCD(0,v) == v; GCD(u,0) == u, GCD(0,0) == 0 */
  if (u == 0)
  {
    return v;
  }
  if (v == 0)
  {
    return u;
  }
 
  /* Let shift := lg K, where K is the greatest power of 2
        dividing both u and v. */
  for (shift = 0; ((u | v) & 1) == 0; ++shift)
  {
    u >>= 1;
    v >>= 1;
  }
 
  while ((u & 1) == 0)
  {
    u >>= 1;
  }
 
  /* From here on, u is always odd. */
  do
  {
    /* remove all factors of 2 in v -- they are not common */
    /*   note: v is not zero, so while will terminate */
    while ((v & 1) == 0)  /* Loop X */
    {
      v >>= 1;
    }

    /* Now u and v are both odd. Swap if necessary so u <= v,
    then set v = v - u (which is even). For bignums, the
    swapping is just pointer movement, and the subtraction
    can be done in-place. */
    if (u > v)
    {
      // Swap u and v.
      unsigned int t = v;
      v = u;
      u = t;
    }

    v = v - u;                       // Here v >= u.
  }
  while (v != 0);
 
  /* restore common factors of 2 */
  return u << shift;
}

int64_t GreatestCommonDivisor(int64_t u, int64_t v)
{
  int64_t shift;
 
  /* GCD(0,v) == v; GCD(u,0) == u, GCD(0,0) == 0 */
  if (u == 0)
  {
    return v;
  }
  if (v == 0)
  {
    return u;
  }
 
  /* Let shift := lg K, where K is the greatest power of 2
        dividing both u and v. */
  for (shift = 0; ((u | v) & 1) == 0; ++shift)
  {
    u >>= 1;
    v >>= 1;
  }
 
  while ((u & 1) == 0)
  {
    u >>= 1;
  }
 
  /* From here on, u is always odd. */
  do
  {
    /* remove all factors of 2 in v -- they are not common */
    /*   note: v is not zero, so while will terminate */
    while ((v & 1) == 0)  /* Loop X */
    {
      v >>= 1;
    }

    /* Now u and v are both odd. Swap if necessary so u <= v,
    then set v = v - u (which is even). For bignums, the
    swapping is just pointer movement, and the subtraction
    can be done in-place. */
    if (u > v)
    {
      // Swap u and v.
      int64_t t = v;
      v = u;
      u = t;
    }

    v = v - u;                       // Here v >= u.
  }
  while (v != 0);
 
  /* restore common factors of 2 */
  return u << shift;
}

uint64_t GreatestCommonDivisor(uint64_t u, uint64_t v)
{
  uint64_t shift;
 
  /* GCD(0,v) == v; GCD(u,0) == u, GCD(0,0) == 0 */
  if (u == 0)
  {
    return v;
  }
  if (v == 0)
  {
    return u;
  }
 
  /* Let shift := lg K, where K is the greatest power of 2
        dividing both u and v. */
  for (shift = 0; ((u | v) & 1) == 0; ++shift)
  {
    u >>= 1;
    v >>= 1;
  }
 
  while ((u & 1) == 0)
  {
    u >>= 1;
  }
 
  /* From here on, u is always odd. */
  do
  {
    /* remove all factors of 2 in v -- they are not common */
    /*   note: v is not zero, so while will terminate */
    while ((v & 1) == 0)  /* Loop X */
    {
      v >>= 1;
    }

    /* Now u and v are both odd. Swap if necessary so u <= v,
    then set v = v - u (which is even). For bignums, the
    swapping is just pointer movement, and the subtraction
    can be done in-place. */
    if (u > v)
    {
      // Swap u and v.
      uint64_t t = v;
      v = u;
      u = t;
    }

    v = v - u;                       // Here v >= u.
  }
  while (v != 0);
 
  /* restore common factors of 2 */
  return u << shift;
}