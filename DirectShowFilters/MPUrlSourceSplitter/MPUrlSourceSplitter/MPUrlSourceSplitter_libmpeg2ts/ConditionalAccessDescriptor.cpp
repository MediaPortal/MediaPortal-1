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

#include "ConditionalAccessDescriptor.h"
#include "BufferHelper.h"

CConditionalAccessDescriptor::CConditionalAccessDescriptor(HRESULT *result)
  : CDescriptor(result)
{
}

CConditionalAccessDescriptor::~CConditionalAccessDescriptor(void)
{
}

/* get methods */

uint16_t CConditionalAccessDescriptor::GetSystemId(void)
{
  uint16_t systemId = (this->GetPayloadSize() >= CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH) ? RBE16(this->GetPayload(), 0) : 0;

  return ((systemId >> CONDITIONAL_ACCESS_DESCRIPTOR_SYSTEM_ID_SHIFT) & CONDITIONAL_ACCESS_DESCRIPTOR_SYSTEM_ID_MASK);
}

uint16_t CConditionalAccessDescriptor::GetPID(void)
{
  uint16_t pid = (this->GetPayloadSize() >= CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH) ? RBE16(this->GetPayload(), 2) : 0;

  return ((pid >> CONDITIONAL_ACCESS_DESCRIPTOR_PID_SHIFT) & CONDITIONAL_ACCESS_DESCRIPTOR_PID_MASK);
}

uint8_t CConditionalAccessDescriptor::GetPrivateDataSize(void)
{
  return (this->GetPayloadSize() > CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH) ? (this->GetPayloadSize() - CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH) : 0;
}

const uint8_t *CConditionalAccessDescriptor::GetPrivateData(void)
{
  return (this->GetPrivateDataSize() > 0) ? (this->GetPayload() + CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH) : NULL;
}

/* set methods */

/* other methods */

/* protected methods */

bool CConditionalAccessDescriptor::ParseInternal(const unsigned char *buffer, uint32_t length, bool onlyHeader)
{
  bool result = __super::ParseInternal(buffer, length, onlyHeader);
  result &= (this->GetTag() == TAG_CONDITIONAL_ACCESS_DESCRIPTOR);
  result &= (length >= CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH);

  return result;
}

CDescriptor *CConditionalAccessDescriptor::CreateDescriptor(void)
{
  HRESULT result = S_OK;
  CConditionalAccessDescriptor *descriptor = new CConditionalAccessDescriptor(&result);
  CHECK_POINTER_HRESULT(result, descriptor, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(descriptor));
  return descriptor;
}
