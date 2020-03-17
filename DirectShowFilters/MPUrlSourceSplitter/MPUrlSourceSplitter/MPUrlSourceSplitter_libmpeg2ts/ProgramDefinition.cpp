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

#include "ProgramDefinition.h"

CProgramDefinition::CProgramDefinition(HRESULT *result)
{
  this->streamType = 0;
  this->elementaryPID = 0;
  this->esInfoSize = 0;
  this->esInfoDescriptor = NULL;
}

CProgramDefinition::~CProgramDefinition(void)
{
  FREE_MEM(this->esInfoDescriptor);
}

/* get methods */

unsigned int CProgramDefinition::GetStreamType(void)
{
  return this->streamType;
}

unsigned int CProgramDefinition::GetElementaryPID(void)
{
  return this->elementaryPID;
}

unsigned int CProgramDefinition::GetEsInfoSize(void)
{
  return this->esInfoSize;
}

const uint8_t *CProgramDefinition::GetEsInfoDescriptor(void)
{
  return this->esInfoDescriptor;
}

/* set methods */

void CProgramDefinition::SetStreamType(unsigned int streamType)
{
  this->streamType = (uint8_t)streamType;
}

void CProgramDefinition::SetElementaryPID(unsigned int elementaryPID)
{
  this->elementaryPID = (uint16_t)elementaryPID;
}

bool CProgramDefinition::SetEsInfoDescriptor(const uint8_t *descriptor, unsigned int size)
{
  HRESULT result = S_OK;
  FREE_MEM(this->esInfoDescriptor);

  this->esInfoSize = (uint16_t)size;
  if (this->esInfoSize != 0)
  {
    CHECK_POINTER_DEFAULT_HRESULT(result, descriptor);
    
    if (SUCCEEDED(result))
    {
      this->esInfoDescriptor = ALLOC_MEM_SET(this->esInfoDescriptor, uint8_t, this->esInfoSize, 0);
      CHECK_POINTER_HRESULT(result, this->esInfoDescriptor, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), memcpy(this->esInfoDescriptor, descriptor, this->esInfoSize));
    }
  }

  return SUCCEEDED(result);
}

/* other methods */

CProgramDefinition *CProgramDefinition::Clone(void)
{
  HRESULT result = S_OK;
  CProgramDefinition *definition = new CProgramDefinition(&result);
  CHECK_POINTER_HRESULT(result, definition, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    definition->streamType = this->streamType;
    definition->elementaryPID = this->elementaryPID;
    definition->esInfoSize = this->esInfoSize;

    if (definition->esInfoSize != 0)
    {
      definition->esInfoDescriptor = ALLOC_MEM_SET(definition->esInfoDescriptor, uint8_t, definition->esInfoSize, 0);
      CHECK_POINTER_HRESULT(result, definition->esInfoDescriptor, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), memcpy(definition->esInfoDescriptor, this->esInfoDescriptor, definition->esInfoSize));
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(definition));
  return definition;
}

/* protected methods */