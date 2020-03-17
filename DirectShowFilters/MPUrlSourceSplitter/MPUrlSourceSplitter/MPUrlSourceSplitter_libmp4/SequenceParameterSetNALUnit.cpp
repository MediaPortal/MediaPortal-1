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

#include "SequenceParameterSetNALUnit.h"

CSequenceParameterSetNALUnit::CSequenceParameterSetNALUnit(HRESULT *result)
{
  this->length = 0;
  this->buffer = NULL;
}

CSequenceParameterSetNALUnit::~CSequenceParameterSetNALUnit(void)
{
  FREE_MEM(buffer);
}

/* get methods */

uint16_t CSequenceParameterSetNALUnit::GetLength(void)
{
  return this->length;
}

const uint8_t *CSequenceParameterSetNALUnit::GetBuffer(void)
{
  return this->buffer;
}

/* set methods */

bool CSequenceParameterSetNALUnit::SetBuffer(const uint8_t *buffer, uint16_t length)
{
  FREE_MEM(this->buffer);
  this->length = 0;

  if ((length > 0) && (buffer != NULL))
  {
    this->buffer = ALLOC_MEM_SET(this->buffer, uint8_t, length, 0);
    if (buffer != NULL)
    {
      memcpy(this->buffer, buffer, length);
      this->length = length;
    }
  }

  return ((this->buffer != NULL) || ((this->buffer == NULL) && (buffer == NULL)));
}

/* other methods */