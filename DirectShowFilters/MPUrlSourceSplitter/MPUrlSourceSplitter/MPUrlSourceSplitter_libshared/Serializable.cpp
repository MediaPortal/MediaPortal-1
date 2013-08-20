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

#include "Serializable.h"
#include "BufferHelper.h"

CSerializable::CSerializable(void)
{
}

CSerializable::~CSerializable(void)
{
}

uint32_t CSerializable::GetSerializeSize(void)
{
  return 0;
}

bool CSerializable::Serialize(uint8_t *buffer)
{
  return (buffer != NULL);
}

uint32_t CSerializable::GetSerializeStringSize(const wchar_t *input)
{
  uint32_t required = 4;

  if (input != NULL)
  {
    required += (wcslen(input) * sizeof(wchar_t));
  }

  return required;
}

bool CSerializable::SerializeString(uint8_t *buffer, const wchar_t *input)
{
  bool result = (buffer != NULL);
  uint32_t position = 0;

  if (result)
  {
    if (input == NULL)
    {
      WBE32INC(buffer, position, 0);
    }
    else
    {
      uint32_t count = wcslen(input);
      uint32_t length = (count + 1) * sizeof(wchar_t);
      WBE32INC(buffer, position, length);

      for (uint32_t i = 0; i < count; i++)
      {
        WBE16INC(buffer, position, input[i]);
      }

      WBE16INC(buffer, position, 0);
    }
  }

  return result;
}

bool CSerializable::Deserialize(const uint8_t *buffer)
{
  return (buffer != NULL);
}

bool CSerializable::DeserializeString(const uint8_t *buffer, wchar_t **output)
{
  bool result = ((buffer != NULL) && (output != NULL));
  uint32_t position = 0;

  if (result)
  {
    uint32_t length = 0;
    RBE32INC(buffer, position, length);

    if (length == 0)
    {
      // NULL string serialized
      *output = NULL;
    }
    else
    {
      length /= sizeof(wchar_t);
      *output = ALLOC_MEM_SET(*output, wchar_t, length, 0);
      result = ((*output) != NULL);

      if (result)
      {
        uint32_t count = length - 1;
        for (uint32_t i = 0; i < count; i++)
        {
          RBE16INC(buffer, position, (*output)[i]);
        }
      }
    }
  }

  return result;
}