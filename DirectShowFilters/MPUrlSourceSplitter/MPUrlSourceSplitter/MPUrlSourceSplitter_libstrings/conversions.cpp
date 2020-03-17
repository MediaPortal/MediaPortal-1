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

#include "conversions.h"

unsigned int GetValueUnsignedIntA(const char *input, unsigned int defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceA(input))
  {
    char *end = NULL;
    long valueLong = strtoul(input, &end, 10);
    if ((valueLong == 0) && (input == end))
    {
      // error while converting
      valueLong = defaultValue;
    }

    return (unsigned int)valueLong;
  }
  else
  {
    return defaultValue;
  }
}

unsigned int GetValueUnsignedIntW(const wchar_t *input, unsigned int defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceW(input))
  {
    wchar_t *end = NULL;
    long valueLong = wcstoul(input, &end, 10);
    if ((valueLong == 0) && (input == end))
    {
      // error while converting
      valueLong = defaultValue;
    }

    return (unsigned int)valueLong;
  }
  else
  {
    return defaultValue;
  }
}

unsigned int GetHexValueUnsignedIntA(const char *input, unsigned int defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceA(input))
  {
    char *end = NULL;
    long valueLong = strtoul(input, &end, 16);
    if ((valueLong == 0) && (input == end))
    {
      // error while converting
      valueLong = defaultValue;
    }

    return (unsigned int)valueLong;
  }
  else
  {
    return defaultValue;
  }
}

unsigned int GetHexValueUnsignedIntW(const wchar_t *input, unsigned int defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceW(input))
  {
    wchar_t *end = NULL;
    long valueLong = wcstoul(input, &end, 16);
    if ((valueLong == 0) && (input == end))
    {
      // error while converting
      valueLong = defaultValue;
    }

    return (unsigned int)valueLong;
  }
  else
  {
    return defaultValue;
  }
}

uint64_t GetValueUnsignedInt64A(const char *input, uint64_t defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceA(input))
  {
    char *end = NULL;
    uint64_t valueLong = _strtoui64(input, &end, 10);
    if ((valueLong == 0) && (input == end))
    {
      // error while converting
      valueLong = defaultValue;
    }

    return valueLong;
  }
  else
  {
    return defaultValue;
  }
}

uint64_t GetValueUnsignedInt64W(const wchar_t *input, uint64_t defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceW(input))
  {
    wchar_t *end = NULL;
    uint64_t valueLong = _wcstoui64(input, &end, 10);
    if ((valueLong == 0) && (input == end))
    {
      // error while converting
      valueLong = defaultValue;
    }

    return valueLong;
  }
  else
  {
    return defaultValue;
  }
}

uint8_t HexToDecA(const char c)
{
  switch(c)
  {
  case '0':
  case '1':
  case '2':
  case '3':
  case '4':
  case '5':
  case '6':
  case '7':
  case '8':
  case '9':
    return (c - '0');
  case 'a':
  case 'b':
  case 'c':
  case 'd':
  case 'e':
  case 'f':
    return (c - 'a' + 10);
  case 'A':
  case 'B':
  case 'C':
  case 'D':
  case 'E':
  case 'F':
    return (c - 'A' + 10);
  default:
    return UINT8_MAX;
  }
}

uint8_t HexToDecW(const wchar_t c)
{
  switch(c)
  {
  case L'0':
  case L'1':
  case L'2':
  case L'3':
  case L'4':
  case L'5':
  case L'6':
  case L'7':
  case L'8':
  case L'9':
    return (c - L'0');
  case L'a':
  case L'b':
  case L'c':
  case L'd':
  case L'e':
  case L'f':
    return (c - L'a' + 10);
  case L'A':
  case L'B':
  case L'C':
  case L'D':
  case L'E':
  case L'F':
    return (c - L'A' + 10);
  default:
    return UINT8_MAX;
  }
}

uint8_t *HexToDecA(const char *input)
{
  uint8_t *result = NULL;

  if (input != NULL)
  {
    unsigned int length = strlen(input);
    result = ALLOC_MEM_SET(result, uint8_t, (length / 2), 0);
    
    if (result != NULL)
    {
      for (unsigned int i = 0; ((result != NULL) && (i < length)); i+= 2)
      {
        uint8_t high = HexToDecA(input[i]);
        uint8_t low = HexToDecA(input[i + 1]);

        if ((high != UINT8_MAX) && (low != UINT8_MAX))
        {
          result[i / 2] = (uint8_t)((high << 4) + low);
        }
        else
        {
          // error while converting
          FREE_MEM(result);
        }
      }
    }
  }

  return result;
}

uint8_t *HexToDecW(const wchar_t *input)
{
  uint8_t *result = NULL;

  if (input != NULL)
  {
    unsigned int length = wcslen(input);
    result = ALLOC_MEM_SET(result, uint8_t, (length / 2), 0);
    
    if (result != NULL)
    {
      for (unsigned int i = 0; ((result != NULL) && (i < length)); i+= 2)
      {
        uint8_t high = HexToDecW(input[i]);
        uint8_t low = HexToDecW(input[i + 1]);

        if ((high != UINT8_MAX) && (low != UINT8_MAX))
        {
          result[i / 2] = (uint8_t)((high << 4) + low);
        }
        else
        {
          // error while converting
          FREE_MEM(result);
        }
      }
    }
  }

  return result;
}

double GetValueDoubleA(const char *input, double defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceA(input))
  {
    char *end = NULL;
    double valueDouble = strtod(input, &end);
    if ((valueDouble == 0) && (input == end))
    {
      // error while converting
      valueDouble = defaultValue;
    }

    return valueDouble;
  }
  else
  {
    return defaultValue;
  }
}

double GetValueDoubleW(const wchar_t *input, double defaultValue)
{
  if (!IsNullOrEmptyOrWhitespaceW(input))
  {
    wchar_t *end = NULL;
    double valueDouble = wcstod(input, &end);
    if ((valueDouble == 0) && (input == end))
    {
      // error while converting
      valueDouble = defaultValue;
    }

    return valueDouble;
  }
  else
  {
    return defaultValue;
  }
}