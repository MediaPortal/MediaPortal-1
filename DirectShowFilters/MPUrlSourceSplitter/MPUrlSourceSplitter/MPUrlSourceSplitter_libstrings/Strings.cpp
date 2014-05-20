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

char *ConvertGuidToStringA(const GUID guid)
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

wchar_t *ConvertGuidToStringW(const GUID guid)
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

GUID ConvertStringToGuidA(const char *guid)
{
  GUID result = GUID_NULL;

  if (!IsNullOrEmptyOrWhitespaceA(guid))
  {
    wchar_t *wideGuid = ConvertToUnicodeA(guid);
    if (wideGuid != NULL)
    {
      result = ConvertStringToGuidW(wideGuid);
    }
    FREE_MEM(wideGuid);
  }
  return result;
}

GUID ConvertStringToGuidW(const wchar_t *guid)
{
  GUID result = GUID_NULL;

  if (!IsNullOrEmptyOrWhitespaceW(guid))
  {
    if (IIDFromString(guid, &result) != S_OK)
    {
      result = GUID_NULL;
    }
  }

  return result;
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

bool IsNullOrEmptyA(const char *string)
{
  bool result = true;
  if (string != NULL)
  {
    result = (strlen(string) == 0);
  }

  return result;
}

bool IsNullOrEmptyW(const wchar_t *string)
{
  bool result = true;
  if (string != NULL)
  {
    result = (wcslen(string) == 0);
  }

  return result;
}

bool IsNullOrEmptyOrWhitespaceA(const char *string)
{
  bool result = IsNullOrEmptyA(string);

  if (!result)
  {
    const char *temp = SkipBlanksA(string);
    result = (strlen(temp) == 0);
  }

  return result;
}

bool IsNullOrEmptyOrWhitespaceW(const wchar_t *string)
{
  bool result = IsNullOrEmptyW(string);

  if (!result)
  {
    const wchar_t *temp = SkipBlanksW(string);
    result = (wcslen(temp) == 0);
  }

  return result;
}


char *FormatStringA(const char *format, ...)
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

wchar_t *FormatStringW(const wchar_t *format, ...)
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

bool IsBlankA(const char *input)
{
  if (input == NULL)
  {
    return false;
  }

  switch(*input)
  {
  case ' ':
  case '\t':
  case '\r':
  case '\n':
    return true;
  default:
    return false;
  }
}

bool IsBlankW(const wchar_t *input)
{
  if (input == NULL)
  {
    return false;
  }

  switch(*input)
  {
  case L' ':
  case L'\t':
  case L'\r':
  case L'\n':
    return true;
  default:
    return false;
  }
}

const char *SkipBlanksA(const char *str)
{
  if (str == NULL)
  {
    return NULL;
  }

  unsigned int length = strlen(str);

  while(length > 0)
  {
    if (IsBlankA(str))
    {
      --length;
      ++str;
    }
    else
    {
      length = 0;
    }
  }

  return str;
} 

const wchar_t *SkipBlanksW(const wchar_t *str)
{
  if (str == NULL)
  {
    return NULL;
  }

  unsigned int length = wcslen(str);

  while(length > 0)
  {
    if (IsBlankW(str))
    {
      --length;
      ++str;
    }
    else
    {
      length = 0;
    }
  }

  return str;
}

static bool IsUnreservedA(char in)
{
  switch (in)
  {
    case '0': case '1': case '2': case '3': case '4':
    case '5': case '6': case '7': case '8': case '9':
    case 'a': case 'b': case 'c': case 'd': case 'e':
    case 'f': case 'g': case 'h': case 'i': case 'j':
    case 'k': case 'l': case 'm': case 'n': case 'o':
    case 'p': case 'q': case 'r': case 's': case 't':
    case 'u': case 'v': case 'w': case 'x': case 'y': case 'z':
    case 'A': case 'B': case 'C': case 'D': case 'E':
    case 'F': case 'G': case 'H': case 'I': case 'J':
    case 'K': case 'L': case 'M': case 'N': case 'O':
    case 'P': case 'Q': case 'R': case 'S': case 'T':
    case 'U': case 'V': case 'W': case 'X': case 'Y': case 'Z':
    case '-': case '.': case '_': case '~':
      return true;
    default:
      return false;
  }
}

static bool IsUnreservedW(wchar_t in)
{
  switch (in)
  {
    case L'0': case L'1': case L'2': case L'3': case L'4':
    case L'5': case L'6': case L'7': case L'8': case L'9':
    case L'a': case L'b': case L'c': case L'd': case L'e':
    case L'f': case L'g': case L'h': case L'i': case L'j':
    case L'k': case L'l': case L'm': case L'n': case L'o':
    case L'p': case L'q': case L'r': case L's': case L't':
    case L'u': case L'v': case L'w': case L'x': case L'y': case L'z':
    case L'A': case L'B': case L'C': case L'D': case L'E':
    case L'F': case L'G': case L'H': case L'I': case L'J':
    case L'K': case L'L': case L'M': case L'N': case L'O':
    case L'P': case L'Q': case L'R': case L'S': case L'T':
    case L'U': case L'V': case L'W': case L'X': case L'Y': case L'Z':
    case L'-': case L'.': case L'_': case L'~':
      return true;
    default:
      return false;
  }
}


char *EscapeA(const char *input)
{
  char *result = NULL;
  
  if (input != NULL)
  {
    size_t length = strlen(input);
    size_t inputIndex = 0;
    unsigned char in; /* we need to treat the characters unsigned */
    size_t outputLength = length;
    size_t outputIndex = 0;

    result = ALLOC_MEM_SET(result, char, (length + 1), 0);
    while ((result != NULL) && (inputIndex < length))
    {
      in = *(input + inputIndex);

      if (IsUnreservedA(in))
      {
        /* just copy this */
        result[outputIndex++] = in;
      }
      else 
      {
        // encode it
        // the size grows with two, since this'll become a %XX
        ALLOC_MEM_DEFINE_SET(tempResult, char, (outputLength + 2), 0);
        if (tempResult != NULL)
        {
          memcpy(tempResult, result, outputLength);
          FREE_MEM(result);
          result = tempResult;
          outputLength += 2;

          _snprintf_s(&result[outputIndex], outputLength - outputIndex, 4, "%%%02X", in);
          outputIndex += 3;
        }
        else
        {
          // error occured, free result memory and return
          FREE_MEM(result);
        }
      }

      inputIndex++;
    }
  }

  return result;
}

wchar_t *EscapeW(const wchar_t *input)
{
  wchar_t *result = NULL;
  
  if (input != NULL)
  {
    size_t length = wcslen(input);
    size_t inputIndex = 0;
    wchar_t in;
    size_t outputLength = length;
    size_t outputIndex = 0;

    result = ALLOC_MEM_SET(result, wchar_t, (length + 1), 0);
    while ((result != NULL) && (inputIndex < length))
    {
      in = *(input + inputIndex);

      if (IsUnreservedW(in))
      {
        /* just copy this */
        result[outputIndex++] = in;
      }
      else 
      {
        // encode it
        // the size grows with two, since this'll become a %XX
        ALLOC_MEM_DEFINE_SET(tempResult, wchar_t, (outputLength + 2), 0);
        if (tempResult != NULL)
        {
          memcpy(tempResult, result, outputLength * sizeof(wchar_t));
          FREE_MEM(result);
          result = tempResult;
          outputLength += 2;

          _snwprintf_s(&result[outputIndex], outputLength - outputIndex, 4, L"%%%02X", in);
          outputIndex += 3;
        }
        else
        {
          // error occured, free result memory and return
          FREE_MEM(result);
        }
      }

      inputIndex++;
    }
  }

  return result;
}

char *UnescapeA(const char *input)
{
  char *result = NULL;

  if (input != NULL)
  {
    size_t length = strlen(input);
    size_t inputIndex = 0;
    unsigned char in; /* we need to treat the characters unsigned */
    size_t outputLength = length;
    size_t outputIndex = 0;

    result = ALLOC_MEM_SET(result, char, (length + 1), 0);
    while ((result != NULL) && (inputIndex < length))
    {
      in = *(input + inputIndex);

      if (in == '%')
      {
        // check length and check next two characters
        if ((inputIndex + 2) < length)
        {
          if (isxdigit(*(input + inputIndex + 1)) && isxdigit(*(input + inputIndex + 2)))
          {
            // this is two hexadecimal digits following a '%'
            ALLOC_MEM_DEFINE_SET(hexString, char, 3, 0);
            if (hexString != NULL)
            {
              char *endPtr = NULL;
              memcpy(hexString, input + inputIndex + 1, 2);
              in = (unsigned char)strtoul(hexString, &endPtr, 16);
              inputIndex += 2;
            }
            FREE_MEM(hexString);
          }
        }
      }

      result[outputIndex++] = in;
      inputIndex++;      
    }
  }

  return result;
}

wchar_t *UnescapeW(const wchar_t *input)
{
  wchar_t *result = NULL;

  if (input != NULL)
  {
    size_t length = wcslen(input);
    size_t inputIndex = 0;
    wchar_t in;
    size_t outputLength = length;
    size_t outputIndex = 0;

    result = ALLOC_MEM_SET(result, wchar_t, (length + 1), 0);
    while ((result != NULL) && (inputIndex < length))
    {
      in = *(input + inputIndex);

      if (in == L'%')
      {
        // check length and check next two characters
        if ((inputIndex + 2) < length)
        {
          if (iswxdigit(*(input + inputIndex + 1)) && iswxdigit(*(input + inputIndex + 2)))
          {
            // this is two hexadecimal digits following a '%'
            ALLOC_MEM_DEFINE_SET(hexString, wchar_t, 3, 0);
            if (hexString != NULL)
            {
              wchar_t *endPtr = NULL;
              memcpy(hexString, input + inputIndex + 1, 2 * sizeof(wchar_t));
              in = (wchar_t)wcstoul(hexString, &endPtr, 16);
              inputIndex += 2;
            }
            FREE_MEM(hexString);
          }
        }
      }

      result[outputIndex++] = in;
      inputIndex++;      
    }
  }

  return result;
}

wchar_t *ConvertUtf8ToUnicode(const char *utf8String)
{
  wchar_t *result = NULL;

  if (utf8String != NULL)
  {
    int length = MultiByteToWideChar(CP_UTF8, 0, utf8String, -1, NULL, 0); // including null character

    result = ALLOC_MEM_SET(result, wchar_t, length, 0);
    if ((result != NULL) && (length > 1))
    {
      if (MultiByteToWideChar(CP_UTF8, 0, utf8String, -1, result, length) == 0)
      {
        // error occured
        FREE_MEM(result);
      }
    }
  }

  return result;
}

char *ConvertUnicodeToUtf8(const wchar_t *unicodeString)
{
  char *result = NULL;

  if (unicodeString != NULL)
  {
    int length = WideCharToMultiByte(CP_UTF8, 0, unicodeString, -1, NULL, 0, NULL, NULL); // including null character
    result = ALLOC_MEM_SET(result, char, length, 0);
    if ((result != NULL) && (length > 1))
    {
      if (WideCharToMultiByte(CP_UTF8, 0, unicodeString, -1, result, length, NULL, NULL) == 0)
      {
        // error occured
        FREE_MEM(result);
      }
    }
  }

  return result;
}

char *TrimLeftA(const char *input)
{
  return DuplicateA(SkipBlanksA(input));
}

wchar_t *TrimLeftW(const wchar_t *input)
{
  return DuplicateW(SkipBlanksW(input));
}

char *TrimRightA(const char *input)
{
  char *reversed = ReverseA(input);
  char *trimmed = TrimLeftA(reversed);
  char *result = ReverseA(trimmed);

  FREE_MEM(reversed);
  FREE_MEM(trimmed);

  return result;
}

wchar_t *TrimRightW(const wchar_t *input)
{
  wchar_t *reversed = ReverseW(input);
  wchar_t *trimmed = TrimLeftW(reversed);
  wchar_t *result = ReverseW(trimmed);

  FREE_MEM(reversed);
  FREE_MEM(trimmed);

  return result;
}

char *TrimA(const char *input)
{
  char *trimmed = TrimLeftA(input);
  char *result = TrimRightA(trimmed);
  FREE_MEM(trimmed);

  return result;
}

wchar_t *TrimW(const wchar_t *input)
{
  wchar_t *trimmed = TrimLeftW(input);
  wchar_t *result = TrimRightW(trimmed);
  FREE_MEM(trimmed);

  return result;
}

char *ReverseA(const char *input)
{
  char *result = DuplicateA(input);
  
  if (input != NULL)
  {
    _strrev(result);
  }

  return result;
}

wchar_t *ReverseW(const wchar_t *input)
{
  wchar_t *result = DuplicateW(input);
  
  if (input != NULL)
  {
    _wcsrev(result);
  }

  return result;
}

bool EndsWithA(const char *string, const char c)
{
  bool result = false;

  if (!IsNullOrEmptyA(string))
  {
    unsigned int length = strlen(string);
    result = (string[length - 1] == c);
  }

  return result;
}

bool EndsWithW(const wchar_t *string, const wchar_t c)
{
  bool result = false;

  if (!IsNullOrEmptyW(string))
  {
    unsigned int length = wcslen(string);
    result = (string[length - 1] == c);
  }

  return result;
}

int CompareWithNullA(const char *str1, const char *str2)
{
  int result = 0;

  if ((str1 != NULL) && (str2 != NULL))
  {
    result = strcmp(str1, str2);
  }
  else if (str1 != NULL)
  {
    result = -1;
  }
  else if (str2 != NULL)
  {
    result = 1;
  }

  return result;
}

int CompareWithNullW(const wchar_t *str1, const wchar_t *str2)
{
  int result = 0;

  if ((str1 != NULL) && (str2 != NULL))
  {
    result = wcscmp(str1, str2);
  }
  else if (str1 != NULL)
  {
    result = -1;
  }
  else if (str2 != NULL)
  {
    result = 1;
  }

  return result;
}

int CompareWithNullInvariantA(const char *str1, const char *str2)
{
  int result = 0;

  if ((str1 != NULL) && (str2 != NULL))
  {
    result = _stricmp(str1, str2);
  }
  else if (str1 != NULL)
  {
    result = -1;
  }
  else if (str2 != NULL)
  {
    result = 1;
  }

  return result;
}

int CompareWithNullInvariantW(const wchar_t *str1, const wchar_t *str2)
{
  int result = 0;

  if ((str1 != NULL) && (str2 != NULL))
  {
    result = _wcsicmp(str1, str2);
  }
  else if (str1 != NULL)
  {
    result = -1;
  }
  else if (str2 != NULL)
  {
    result = 1;
  }

  return result;
}

int IndexOfA(const char *string, const char *searchString)
{
  return IndexOfA(string, (string == NULL) ? 0 : strlen(string), searchString, (searchString == NULL) ? 0 : strlen(searchString));
}

int IndexOfA(const char *string, unsigned int stringLength, const char *searchString, unsigned int searchStringLength)
{
  if ((string == NULL) || (searchString == NULL))
  {
    return -1;
  }

  int result = -1;

  for (unsigned int i = 0; i < stringLength; i++)
  {
    if (strncmp(string + i, searchString, searchStringLength) == 0)
    {
      // we found search string in string
      result = i;
      break;
    }
  }

  return result;
}

int IndexOfW(const wchar_t *string, const wchar_t *searchString)
{
  return IndexOfW(string, (string == NULL) ? 0 : wcslen(string), searchString, (searchString == NULL) ? 0 : wcslen(searchString));
}

int IndexOfW(const wchar_t *string, unsigned int stringLength, const wchar_t *searchString, unsigned int searchStringLength)
{
  if ((string == NULL) || (searchString == NULL))
  {
    return -1;
  }

  int result = -1;

  for (unsigned int i = 0; i < stringLength; i++)
  {
    if (wcsncmp(string + i, searchString, searchStringLength) == 0)
    {
      // we found search string in string
      result = i;
      break;
    }
  }

  return result;
}

char *SubstringA(const char *string, unsigned int position)
{
  return SubstringA(string, position, UINT_MAX);
}

char *SubstringA(const char *string, unsigned int position, unsigned int length)
{
  char *result = NULL;
  unsigned int inputStringLength = (string != NULL) ? strlen(string) : 0;

  if ((position < inputStringLength) && (length > 0))
  {
    unsigned int resultLength = min(inputStringLength - position, length) + 1;
    result = ALLOC_MEM_SET(result, char, resultLength, 0);
    if (result != NULL)
    {
      strncpy_s(result, resultLength, string + position, resultLength - 1);
    }
  }

  return result;
}

wchar_t *SubstringW(const wchar_t *string, unsigned int position)
{
  return SubstringW(string, position, UINT_MAX);
}

wchar_t *SubstringW(const wchar_t *string, unsigned int position, unsigned int length)
{
  wchar_t *result = NULL;
  unsigned int inputStringLength = (string != NULL) ? wcslen(string) : 0;

  if ((position < inputStringLength) && (length > 0))
  {
    unsigned int resultLength = min(inputStringLength - position, length) + 1;
    result = ALLOC_MEM_SET(result, wchar_t, resultLength, 0);
    if (result != NULL)
    {
      wcsncpy_s(result, resultLength, string + position, resultLength - 1);
    }
  }

  return result;
}

LineEnding GetEndOfLineA(const char *buffer, unsigned int length)
{
  return GetEndOfLineA(buffer, length, 0);
}

LineEnding GetEndOfLineA(const char *buffer, unsigned int length, unsigned int start)
{
  LineEnding result;
  result.position = -1;
  result.size = 0;

  for (unsigned int i = start; (i < length); i++)
  {
    if ((buffer[i] == '\r') || (buffer[i] == '\n'))
    {
      result.position = i;
      result.size = 1;

      if ((i + 1) < length)
      {
        if ((buffer[i] != buffer[i + 1]) && ((buffer[i + 1] == '\r') || (buffer[i + 1] == '\n')))
        {
          result.size = 2;
        }
      }

      break;
    }
  }

  return result;
}

LineEnding GetEndOfLineW(const wchar_t *buffer, unsigned int length)
{
  return GetEndOfLineW(buffer, length, 0);
}

LineEnding GetEndOfLineW(const wchar_t *buffer, unsigned int length, unsigned int start)
{
  LineEnding result;
  result.position = -1;
  result.size = 0;

  for (unsigned int i = start; (i < length); i++)
  {
    if ((buffer[i] == L'\r') || (buffer[i] == L'\n'))
    {
      result.position = i;
      result.size = 1;

      if ((i + 1) < length)
      {
        if ((buffer[i] != buffer[i + 1]) && ((buffer[i + 1] == L'\r') || (buffer[i + 1] == L'\n')))
        {
          result.size = 2;
        }
      }

      break;
    }
  }

  return result;
}

char *ToLowerA(const char *string)
{
  return ToLowerA(string, (string == NULL) ? 0 : strlen(string));
}

char *ToLowerA(const char *string, unsigned int length)
{
  char *result = DuplicateA(string);

  if (result != NULL)
  {
    if (_strlwr_s(result, length + 1) != 0)
    {
      FREE_MEM(result);
    }
  }

  return result;
}


wchar_t *ToLowerW(const wchar_t *string)
{
  return ToLowerW(string, (string == NULL) ? 0 : wcslen(string));
}

wchar_t *ToLowerW(const wchar_t *string, unsigned int length)
{
  wchar_t *result = DuplicateW(string);

  if (result != NULL)
  {
    if (_wcslwr_s(result, length + 1) != 0)
    {
      FREE_MEM(result);
    }
  }

  return result;
}

char *ToUpperA(const char *string)
{
  return ToUpperA(string, (string == NULL) ? 0 : strlen(string));
}

char *ToUpperA(const char *string, unsigned int length)
{
  char *result = DuplicateA(string);

  if (result != NULL)
  {
    if (_strupr_s(result, length + 1) != 0)
    {
      FREE_MEM(result);
    }
  }

  return result;
}

wchar_t *ToUpperW(const wchar_t *string)
{
  return ToUpperW(string, (string == NULL) ? 0 : wcslen(string));
}

wchar_t *ToUpperW(const wchar_t *string, unsigned int length)
{
  wchar_t *result = DuplicateW(string);

  if (result != NULL)
  {
    if (_wcsupr_s(result, length + 1) != 0)
    {
      FREE_MEM(result);
    }
  }

  return result;
}

char *AppendStringA(char *string1, const char *string2)
{
  char *result = NULL;
  if ((string1 != NULL) && (string2 != NULL))
  {
    result = FormatStringA("%s%s", string1, string2);
    FREE_MEM(string1);
  }
  return result;
}

wchar_t *AppendStringW(wchar_t *string1, const wchar_t *string2)
{
  wchar_t *result = NULL;
  if ((string1 != NULL) && (string2 != NULL))
  {
    result = FormatStringW(L"%s%s", string1, string2);
    FREE_MEM(string1);
  }
  return result;
}
