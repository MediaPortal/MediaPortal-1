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

#include "stdafx.h"

#include "SectionMultiplexerCollection.h"

CSectionMultiplexerCollection::CSectionMultiplexerCollection(HRESULT *result)
  : CCollection(result)
{
  this->pidMap = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->pidMap = ALLOC_MEM_SET(this->pidMap, uint8_t, TS_PACKET_PID_COUNT, INDEX_NOT_SET);

    CHECK_POINTER_HRESULT(*result, this->pidMap, *result, E_OUTOFMEMORY);
  }
}

CSectionMultiplexerCollection::~CSectionMultiplexerCollection()
{
  FREE_MEM(this->pidMap);
}

/* get methods */

unsigned int CSectionMultiplexerCollection::GetMultiplexerIdByPID(unsigned int pid)
{
  unsigned int multiplexerId = MPEG2TS_MULTIPEXER_NOT_EXISTS;

  if (pid < TS_PACKET_PID_COUNT)
  {
    uint8_t index = this->pidMap[pid];

    if (index != INDEX_NOT_SET)
    {
      multiplexerId = (unsigned int)index;
    }
  }

  return multiplexerId;

}

CSectionMultiplexer *CSectionMultiplexerCollection::GetMultiplexerByPID(unsigned int pid)
{
  unsigned int multiplexerId = this->GetMultiplexerIdByPID(pid);
  CSectionMultiplexer *multiplexer = NULL;

  if (multiplexerId != MPEG2TS_MULTIPEXER_NOT_EXISTS)
  {
    multiplexer = this->GetItem(multiplexerId);
  }

  return multiplexer;
}

/* set methods */

/* other methods */

bool CSectionMultiplexerCollection::Add(CSectionMultiplexer *multiplexer)
{
  bool result = __super::Add(multiplexer);

  if (result)
  {
    this->pidMap[multiplexer->GetPID()] = (uint8_t)(this->Count() - 1);
  }

  return result;
}

bool CSectionMultiplexerCollection::Insert(unsigned int position, CSectionMultiplexer *multiplexer)
{
  return false;
}

void CSectionMultiplexerCollection::Clear(void)
{
  __super::Clear();

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->pidMap, memset(this->pidMap, INDEX_NOT_SET, TS_PACKET_PID_COUNT));
}

bool CSectionMultiplexerCollection::Remove(unsigned int index, unsigned int count)
{
  for (unsigned int i = index; i < (index + count); i++)
  {
    CSectionMultiplexer *multiplexer = this->GetItem(i);

    this->pidMap[multiplexer->GetPID()] = INDEX_NOT_SET;
  }

  if (__super::Remove(index, count))
  {
    for (unsigned int i = index; i < this->Count(); i++)
    {
      CSectionMultiplexer *multiplexer = this->GetItem(i);

      this->pidMap[multiplexer->GetPID()] = (uint8_t)i;
    }

    return true;
  }
  else
  {
    for (unsigned int i = index; i < (index + count); i++)
    {
      CSectionMultiplexer *multiplexer = this->GetItem(i);

      this->pidMap[multiplexer->GetPID()] = (uint8_t)i;
    }

    return false;
  }
}

/* protected methods */

CSectionMultiplexer *CSectionMultiplexerCollection::Clone(CSectionMultiplexer *item)
{
  return NULL;
}
