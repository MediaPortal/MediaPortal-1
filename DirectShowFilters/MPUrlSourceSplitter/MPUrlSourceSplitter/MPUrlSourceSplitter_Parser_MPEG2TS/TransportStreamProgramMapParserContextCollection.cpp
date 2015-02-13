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

#include "TransportStreamProgramMapParserContextCollection.h"

CTransportStreamProgramMapParserContextCollection::CTransportStreamProgramMapParserContextCollection(HRESULT *result)
  : CCollection(result)
{
  this->pidMap = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->pidMap = ALLOC_MEM_SET(this->pidMap, uint8_t, TS_PACKET_PID_COUNT, INDEX_NOT_SET);

    CHECK_POINTER_HRESULT(*result, this->pidMap, *result, E_OUTOFMEMORY);
  }
}

CTransportStreamProgramMapParserContextCollection::~CTransportStreamProgramMapParserContextCollection(void)
{
  FREE_MEM(this->pidMap);
}

/* get methods */

CTransportStreamProgramMapParserContext *CTransportStreamProgramMapParserContextCollection::GetItem(unsigned int index)
{
  return (CTransportStreamProgramMapParserContext *)__super::GetItem(index);
}

unsigned int CTransportStreamProgramMapParserContextCollection::GetParserContextIdByPID(unsigned int pid)
{
  unsigned int contextId = TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_NOT_EXISTS;

  if (pid < TS_PACKET_PID_COUNT)
  {
    uint8_t index = this->pidMap[pid];

    if (index != INDEX_NOT_SET)
    {
      contextId = (unsigned int)index;
    }
  }

  return contextId;
}

CTransportStreamProgramMapParserContext *CTransportStreamProgramMapParserContextCollection::GetParserContextByPID(unsigned int pid)
{
  unsigned int contextId = this->GetParserContextIdByPID(pid);
  CTransportStreamProgramMapParserContext *context = NULL;

  if (contextId != TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_NOT_EXISTS)
  {
    context = this->GetItem(contextId);
  }

  return context;
}

/* set methods */

/* other methods */

bool CTransportStreamProgramMapParserContextCollection::Add(CTransportStreamProgramMapParserContext *context)
{
  bool result = __super::Add(context);

  if (result)
  {
    this->pidMap[context->GetParser()->GetTransportStreamProgramMapSectionPID()] = (uint8_t)(this->Count() - 1);
  }

  return result;
}

bool CTransportStreamProgramMapParserContextCollection::Insert(unsigned int position, CTransportStreamProgramMapParserContext *context)
{
  return false;
}

void CTransportStreamProgramMapParserContextCollection::Clear(void)
{
  __super::Clear();

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->pidMap, memset(this->pidMap, INDEX_NOT_SET, TS_PACKET_PID_COUNT));
}

bool CTransportStreamProgramMapParserContextCollection::Remove(unsigned int index, unsigned int count)
{
  for (unsigned int i = index; i < (index + count); i++)
  {
    CTransportStreamProgramMapParserContext *context = this->GetItem(i);

    if (context->GetParser()->GetTransportStreamProgramMapSectionPID() != TRANSPORT_STREAM_PROGRAM_MAP_PARSER_PID_NOT_DEFINED)
    {
      this->pidMap[context->GetParser()->GetTransportStreamProgramMapSectionPID()] = INDEX_NOT_SET;
    }
  }

  if (__super::Remove(index, count))
  {
    for (unsigned int i = index; i < this->Count(); i++)
    {
      CTransportStreamProgramMapParserContext *context = this->GetItem(i);

      if (context->GetParser()->GetTransportStreamProgramMapSectionPID() != TRANSPORT_STREAM_PROGRAM_MAP_PARSER_PID_NOT_DEFINED)
      {
        this->pidMap[context->GetParser()->GetTransportStreamProgramMapSectionPID()] = (uint8_t)i;
      }
    }

    return true;
  }
  else
  {
    for (unsigned int i = index; i < (index + count); i++)
    {
      CTransportStreamProgramMapParserContext *context = this->GetItem(i);

      if (context->GetParser()->GetTransportStreamProgramMapSectionPID() != TRANSPORT_STREAM_PROGRAM_MAP_PARSER_PID_NOT_DEFINED)
      {
        this->pidMap[context->GetParser()->GetTransportStreamProgramMapSectionPID()] = (uint8_t)i;
      }
    }

    return false;
  }
}

/* protected methods */

CTransportStreamProgramMapParserContext *CTransportStreamProgramMapParserContextCollection::Clone(CTransportStreamProgramMapParserContext *item)
{
  return NULL;
}
