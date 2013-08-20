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

#include "hex.h"
#include "ErrorCodes.h"

static inline bool is_hex(unsigned char c)
{
  return (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'F')) || ((c >= 'a') && (c <= 'f')));
}

static inline unsigned char get_numberA(char c)
{
  unsigned char result = 0xFF;

  if ((c >= '0') && (c <= '9'))
  {
    result = c - '0';
  }
  else if ((c >= 'A') && (c <= 'F'))
  {
    result = c - 'A';
  }
  else if ((c >= 'a') && (c <= 'f'))
  {
    result = c - 'a';
  }

  return result;
}

static inline unsigned char get_numberW(wchar_t c)
{
  unsigned char result = 0xFF;

  if ((c >= L'0') && (c <= L'9'))
  {
    result = c - L'0';
  }
  else if ((c >= L'A') && (c <= L'F'))
  {
    result = c - L'A' + 0x0A;
  }
  else if ((c >= L'a') && (c <= L'f'))
  {
    result = c - L'a' + 0x0A;
  }

  return result;
}

char get_charA(unsigned char c)
{
  char result = (char)0x00;

  if (c < 0x10)
  {
    result = (c < 0x0A) ? (c + '0') : (c - 0x0A + 'A');
  }

  return result;
}

wchar_t get_charW(unsigned char c)
{
  wchar_t result = (wchar_t)0x00;

  if (c < 0x10)
  {
    result = (c < 0x0A) ? (c + L'0') : (c - 0x0A + L'A');
  }

  return result;
}

HRESULT hex_decode(const char *input, unsigned char **output, unsigned int *outputLength)
{
  return hex_decode(input, (input != NULL) ? strlen(input) : 0, output, outputLength);
}

HRESULT hex_decode(const char *input, unsigned int inputLength, unsigned char **output, unsigned int *outputLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, input);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);
  CHECK_POINTER_DEFAULT_HRESULT(result, outputLength);
  CHECK_CONDITION_HRESULT(result, ((inputLength % 2) == 0), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    *outputLength = inputLength / 2;
    *output = ALLOC_MEM_SET(*output, unsigned char, ((*outputLength) + 1), 0);
    CHECK_POINTER_HRESULT(result, *output, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      for (unsigned int i = 0; ((SUCCEEDED(result)) && (i < inputLength)); i += 2)
      {
        unsigned char temp1 = get_numberA(input[i]);
        unsigned char temp2 = get_numberA(input[i + 1]);
        CHECK_CONDITION_HRESULT(result, temp1 != 0xFF, result, E_INVALIDARG);
        CHECK_CONDITION_HRESULT(result, temp2 != 0xFF, result, E_INVALIDARG);

        (*output)[i / 2] = (temp1 << 4) | temp2;
      }
    }

    if (FAILED(result))
    {
      FREE_MEM(*output);
      *outputLength = 0;
    }
  }

  return result;
}

HRESULT hex_decode(const wchar_t *input, unsigned char **output, unsigned int *outputLength)
{
  return hex_decode(input, (input != NULL) ? wcslen(input) : 0, output, outputLength);
}

HRESULT hex_decode(const wchar_t *input, unsigned int inputLength, unsigned char **output, unsigned int *outputLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, input);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);
  CHECK_POINTER_DEFAULT_HRESULT(result, outputLength);
  CHECK_CONDITION_HRESULT(result, ((inputLength % 2) == 0), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    *outputLength = inputLength / 2;
    *output = ALLOC_MEM_SET(*output, unsigned char, ((*outputLength) + 1), 0);
    CHECK_POINTER_HRESULT(result, *output, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      for (unsigned int i = 0; ((SUCCEEDED(result)) && (i < inputLength)); i += 2)
      {
        unsigned char temp1 = get_numberW(input[i]);
        unsigned char temp2 = get_numberW(input[i + 1]);
        CHECK_CONDITION_HRESULT(result, temp1 != 0xFF, result, E_INVALIDARG);
        CHECK_CONDITION_HRESULT(result, temp2 != 0xFF, result, E_INVALIDARG);

        (*output)[i / 2] = (temp1 << 4) | temp2;
      }
    }

    if (FAILED(result))
    {
      FREE_MEM(*output);
      *outputLength = 0;
    }
  }

  return result;
}