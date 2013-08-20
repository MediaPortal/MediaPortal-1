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

#include "base64.h"
#include "ErrorCodes.h"

static const char *base64_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

static inline bool is_base64(unsigned char c)
{
  return (isalnum(c) || (c == '+') || (c == '/'));
}

HRESULT base64_encode(const unsigned char *input, unsigned int length, char **output)
{
  return base64_encode(input, length, output, NULL);
}

HRESULT base64_encode(const unsigned char *input, unsigned int length, char **output, unsigned int *outputLength)
{
  HRESULT result = S_OK;

  CHECK_POINTER_DEFAULT_HRESULT(result, input);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);

  if (SUCCEEDED(result))
  {
    int i = 0;
    int j = 0;
    unsigned char char_array_3[3];
    unsigned char char_array_4[4];
    int outputPointer = 0;

    *output = ALLOC_MEM_SET(*output, char, (length * 4 + 1), 0);
    CHECK_POINTER_HRESULT(result, *output, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      while (length--)
      {
        char_array_3[i++] = *(input++);
        if (i == 3)
        {
          char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
          char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
          char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
          char_array_4[3] = char_array_3[2] & 0x3f;

          for(i = 0; (i <4) ; i++)
          {
            (*output)[outputPointer++] = base64_chars[char_array_4[i]];
          }
          i = 0;
        }
      }

      if (i)
      {
        for(j = i; j < 3; j++)
        {
          char_array_3[j] = '\0';
        }

        char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
        char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
        char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
        char_array_4[3] = char_array_3[2] & 0x3f;

        for (j = 0; (j < i + 1); j++)
        {
          (*output)[outputPointer++] = base64_chars[char_array_4[j]];
        }

        while((i++ < 3))
        {
          (*output)[outputPointer++] = '=';
        }
      }
    }

    if (FAILED(result))
    {
      FREE_MEM(*output);
    }
  }

  if (SUCCEEDED(result) && (outputLength != NULL))
  {
    *outputLength = strlen(*output);
  }

  return result;
}

HRESULT base64_encode(const unsigned char *input, unsigned int length, wchar_t **output)
{
  return base64_encode(input, length, output, NULL);
}

HRESULT base64_encode(const unsigned char *input, unsigned int length, wchar_t **output, unsigned int *outputLength)
{
  HRESULT result = S_OK;

  CHECK_POINTER_DEFAULT_HRESULT(result, input);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);

  if (SUCCEEDED(result))
  {
    char *temp = NULL;
    unsigned int tempLength = 0;
    result = base64_encode(input, length, &temp, &tempLength);

    if (SUCCEEDED(result))
    {
      *output = ConvertToUnicodeA(temp);
      CHECK_POINTER_HRESULT(result, (*output), result, E_CONVERT_STRING_ERROR);

      if (SUCCEEDED(result) && (outputLength != NULL))
      {
        *outputLength = tempLength;
      }
    }
    FREE_MEM(temp);
  }

  return result;
}

HRESULT base64_decode(const char *input, unsigned char **output, unsigned int *length)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, input);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);
  CHECK_POINTER_DEFAULT_HRESULT(result, length);

  if (SUCCEEDED(result))
  {
    int inputLength = strlen(input);
    int i = 0;
    int j = 0;
    int inputPointer = 0;
    int outputPointer = 0;
    unsigned char char_array_4[4], char_array_3[3];

    *output = ALLOC_MEM_SET(*output, unsigned char, inputLength, 0);
    CHECK_POINTER_HRESULT(result, *output, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      while (SUCCEEDED(result) && inputLength-- && (input[inputPointer] != '=') && is_base64(input[inputPointer]))
      {
        char_array_4[i++] = input[inputPointer];
        inputPointer++;

        if (i == 4)
        {
          for (i = 0; SUCCEEDED(result) && (i < 4); i++)
          {
            const char *val = strchr(base64_chars, char_array_4[i]);
            CHECK_POINTER_HRESULT(result, val, result, E_INVALIDARG);

            if (SUCCEEDED(result))
            {
              char_array_4[i] = (val - base64_chars);
            }
          }

          if (SUCCEEDED(result))
          {
            char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
            char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
            char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

            for (i = 0; (i < 3); i++)
            {
              (*output)[outputPointer++] = char_array_3[i];
            }
            i = 0;
          }
        }
      }

      if (i && SUCCEEDED(result))
      {
        for (j = i; j < 4; j++)
        {
          char_array_4[j] = 0;
        }

        for (j = 0; SUCCEEDED(result) && (j < 4); j++)
        {
          const char *val = strchr(base64_chars, char_array_4[j]);
          CHECK_POINTER_HRESULT(result, val, result, E_INVALIDARG);

          if (SUCCEEDED(result))
          {
            char_array_4[j] = (val - base64_chars);
          }
        }

        if (SUCCEEDED(result))
        {
          char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
          char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
          char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

          for (j = 0; (j < i - 1); j++)
          {
            (*output)[outputPointer++] = char_array_3[j];
          }
        }
      }

      if (SUCCEEDED(result))
      {
        *length = outputPointer;
      }
    }

    if (FAILED(result))
    {
      FREE_MEM(*output);
    }
  }

  return result;
}

HRESULT base64_decode(const wchar_t *input, unsigned char **output, unsigned int *length)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, input);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);
  CHECK_POINTER_DEFAULT_HRESULT(result, length);

  if (SUCCEEDED(result))
  {
    char *temp = ConvertToMultiByteW(input);
    CHECK_POINTER_HRESULT(result, temp, result, E_CONVERT_STRING_ERROR);

    if (SUCCEEDED(result))
    {
      result = base64_decode(temp, output, length);
    }

    FREE_MEM(temp);
  }

  return result;
}
