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

#include "Strings.h"

#include <stdio.h>

MPIPTVSOURCE_API char *ConvertGuidToStringA(const GUID guid)
{
  char *result = NULL;
  wchar_t *wideGuid = ConvertGuidToStringW(guid);
  if (wideGuid != NULL)
  {
    size_t length = wcslen(wideGuid) + 1;

    ALLOC_MEM_SET(result, char, length, 0);
    result = ConvertToMultiByteW(wideGuid);
  }
  FREE_MEM(wideGuid);

  return result;
}

MPIPTVSOURCE_API wchar_t *ConvertGuidToStringW(const GUID guid)
{
  ALLOC_MEM_DEFINE_SET(wideGuid, OLECHAR, 256, 0);
  if (wideGuid != NULL)
  {
    if (StringFromGUID2(guid, wideGuid, 256) == 0)
    {
      // error occured
      FREE_MEM(wideGuid);
    }
  }

  return wideGuid;
}

char *ConvertToMultiByteA(const char *string)
{
  char *result = NULL;

  if (string != NULL)
  {
    size_t length = strlen(string) + 1;
    result = ALLOC_MEM_SET(result, char, length, 0);
    if (result != NULL)
    {
      strcpy_s(result, length, string);
    }
  }

  return result;
}

char *ConvertToMultiByteW(const wchar_t *string)
{
  char *result = NULL;

  if (string != NULL)
  {
    size_t length = 0;
    if (wcstombs_s(&length, NULL, 0, string, wcslen(string)) == 0)
    {
      result = ALLOC_MEM_SET(result, char, length, 0);
      if (result != NULL)
      {
        if (wcstombs_s(&length, result, length, string, wcslen(string)) != 0)
        {
          // error occurred but buffer is created
          FREE_MEM(result);
        }
      }
    }
  }

  return result;
}

wchar_t *ConvertToUnicodeA(const char *string)
{
  wchar_t *result = NULL;

  if (string != NULL)
  {
    size_t length = 0;

    if (mbstowcs_s(&length, NULL, 0, string, strlen(string)) == 0)
    {
      result = ALLOC_MEM_SET(result, wchar_t, length, 0);
      if (result != NULL)
      {
        if (mbstowcs_s(&length, result, length, string, strlen(string)) != 0)
        {
          // error occurred but buffer is created
          FREE_MEM(result);
        }
      }
    }
  }

  return result;
}

wchar_t *ConvertToUnicodeW(const wchar_t *string)
{
  wchar_t *result = NULL;

  if (string != NULL)
  {
    size_t length = wcslen(string) + 1;
    result = ALLOC_MEM_SET(result, wchar_t, length, 0);
    if (result != NULL)
    {
      wcscpy_s(result, length, string);
    }
  }

  return result;
}

char *DuplicateA(const char *string)
{
  return ConvertToMultiByteA(string);
}

wchar_t *DuplicateW(const wchar_t *string)
{
  return ConvertToUnicodeW(string);
}

MPIPTVSOURCE_API BOOL IsNullOrEmptyA(const char *string)
{
  BOOL result = TRUE;
  if (string != NULL)
  {
    result = (strlen(string) == 0);
  }

  return result;
}

MPIPTVSOURCE_API BOOL IsNullOrEmptyW(const wchar_t *string)
{
  BOOL result = TRUE;
  if (string != NULL)
  {
    result = (wcslen(string) == 0);
  }

  return result;
}

MPIPTVSOURCE_API char *FormatStringA(const char *format, ...)
{
  va_list ap;
  va_start(ap, format);

  int length = _vscprintf(format, ap) + 1;
  ALLOC_MEM_DEFINE_SET(result, char, length, 0);
  if (result != NULL)
  {
    vsprintf_s(result, length, format, ap);
  }
  va_end(ap);

  return result;
}

MPIPTVSOURCE_API wchar_t *FormatStringW(const wchar_t *format, ...)
{
  va_list ap;
  va_start(ap, format);

  int length = _vscwprintf(format, ap) + 1;
  ALLOC_MEM_DEFINE_SET(result, wchar_t, length, 0);
  if (result != NULL)
  {
    vswprintf_s(result, length, format, ap);
  }
  va_end(ap);

  return result;
}

char *ReplaceStringA(const char *string, const char *searchString, const char *replaceString)
{
  if ((string == NULL) || (searchString == NULL) || (replaceString == NULL))
  {
    return NULL;
  }

  unsigned int resultLength = 0;
  char *resultString = NULL;

  unsigned int stringLength = strlen(string);
  unsigned int searchStringLength = strlen(searchString);
  unsigned int replaceStringLength = strlen(replaceString);

  for (unsigned int i = 0; i < stringLength; i++)
  {
    if (strncmp(string + i, searchString, searchStringLength) == 0)
    {
      // we found search string in string
      // increase result length by lenght of replace string
      resultLength += replaceStringLength;

      // skip remaining characters of search string
      i += searchStringLength - 1;
    }
    else
    {
      // we not found search string
      // current character goes to result
      // increase result length
      resultLength++;
    }
  }

  // we got result length
  // increase it by one (null character)
  resultLength++;
  // allocate memory and start copying and replacing
  resultString = ALLOC_MEM_SET(resultString, char, resultLength, 0);
  if (resultString != NULL)
  {
    unsigned int j = 0;
    for (unsigned int i = 0; i < stringLength; i++)
    {
      if (strncmp(string + i, searchString, searchStringLength) == 0)
      {
        // we found search string in string
        memcpy(resultString + j, replaceString, replaceStringLength * sizeof(char));
        j += replaceStringLength;

        // skip remaining characters of search string
        i += searchStringLength - 1;
      }
      else
      {
        // we not found search string
        // just copy character to output
        resultString[j++] = string[i];
      }
    }
  }

  return resultString;
}

wchar_t *ReplaceStringW(const wchar_t *string, const wchar_t *searchString, const wchar_t *replaceString)
{
  if ((string == NULL) || (searchString == NULL) || (replaceString == NULL))
  {
    return NULL;
  }

  unsigned int resultLength = 0;
  wchar_t *resultString = NULL;

  unsigned int stringLength = wcslen(string);
  unsigned int searchStringLength = wcslen(searchString);
  unsigned int replaceStringLength = wcslen(replaceString);

  for (unsigned int i = 0; i < stringLength; i++)
  {
    if (wcsncmp(string + i, searchString, searchStringLength) == 0)
    {
      // we found search string in string
      // increase result length by lenght of replace string
      resultLength += replaceStringLength;

      // skip remaining characters of search string
      i += searchStringLength - 1;
    }
    else
    {
      // we not found search string
      // current character goes to result
      // increase result length
      resultLength++;
    }
  }

  // we got result length
  // increase it by one (null character)
  resultLength++;
  // allocate memory and start copying and replacing
  resultString = ALLOC_MEM_SET(resultString, wchar_t, resultLength, 0);
  if (resultString != NULL)
  {
    unsigned int j = 0;
    for (unsigned int i = 0; i < stringLength; i++)
    {
      if (wcsncmp(string + i, searchString, searchStringLength) == 0)
      {
        // we found search string in string
        memcpy(resultString + j, replaceString, replaceStringLength * sizeof(wchar_t));
        j += replaceStringLength;

        // skip remaining characters of search string
        i += searchStringLength - 1;
      }
      else
      {
        // we not found search string
        // just copy character to output
        resultString[j++] = string[i];
      }
    }
  }

  return resultString;
}