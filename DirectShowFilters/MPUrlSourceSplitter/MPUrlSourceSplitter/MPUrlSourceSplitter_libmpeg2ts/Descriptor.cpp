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

#include "Descriptor.h"
#include "BufferHelper.h"

CDescriptor::CDescriptor(HRESULT *result)
  : CFlags()
{
  this->tag = 0;
  this->payloadSize = 0;
  this->payload = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    /*this->packet = ALLOC_MEM_SET(this->packet, uint8_t, TS_PACKET_SIZE, 0);

    CHECK_POINTER_HRESULT(*result, this->packet, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      unsigned int header = ((TS_PACKET_SYNC_BYTE & TS_PACKET_HEADER_SYNC_BYTE_MASK) << TS_PACKET_HEADER_SYNC_BYTE_SHIFT);

      WBE32(this->packet, 0, header);
    }*/
  }
}

CDescriptor::~CDescriptor(void)
{
  FREE_MEM(this->payload);
}

/* get methods */

uint8_t CDescriptor::GetTag(void)
{
  return this->tag;
}

uint8_t CDescriptor::GetPayloadSize(void)
{
  return this->payloadSize;
}

const uint8_t *CDescriptor::GetPayload(void)
{
  return this->payload;
}

uint8_t CDescriptor::GetDescriptorSize(void)
{
  return (2 + this->GetPayloadSize());
}

/* set methods */

bool CDescriptor::SetPayload(const uint8_t *payload, unsigned int payloadSize)
{
  HRESULT result = S_OK;
  FREE_MEM(this->payload);
  this->payloadSize = 0;

  if (payloadSize != 0)
  {
    CHECK_POINTER_DEFAULT_HRESULT(result, payload);

    if (SUCCEEDED(result))
    {
      this->payload = ALLOC_MEM_SET(this->payload, uint8_t, payloadSize, 0);
      CHECK_POINTER_HRESULT(result, this->payload, result, E_OUTOFMEMORY);
    
      if (SUCCEEDED(result))
      {
        memcpy((uint8_t *)this->GetPayload(), payload, payloadSize);
        this->payloadSize = payloadSize;
      }
    }
  }

  return SUCCEEDED(result);
}

/* other methods */

bool CDescriptor::IsParsed(void)
{
  return this->IsSetFlags(DESCRIPTOR_FLAG_PARSED);
}

bool CDescriptor::Parse(const unsigned char *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, false);
}

CDescriptor *CDescriptor::Clone(void)
{
  CDescriptor *result = this->CreateDescriptor();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

/* protected methods */

bool CDescriptor::ParseInternal(const unsigned char *buffer, uint32_t length, bool onlyHeader)
{
  this->flags &= ~DESCRIPTOR_FLAG_PARSED;

  if ((buffer != NULL) && (length >= DESCRIPTOR_HEADER_LENGTH))
  {
    unsigned int position = 0;

    RBE8INC(buffer, position, this->tag);
    RBE8INC(buffer, position, this->payloadSize);

    bool result = true;
    if ((this->payloadSize > 0) && (!onlyHeader))
    {
      result &= ((length - position) >= this->payloadSize);

      this->payload = ALLOC_MEM_SET(this->payload, uint8_t, this->payloadSize, 0);
      result &= (this->payload != NULL);

      if (result)
      {
        memcpy(this->payload, buffer + position, this->payloadSize);
      }
    }

    this->flags |= (result) ? DESCRIPTOR_FLAG_PARSED : DESCRIPTOR_FLAG_NONE;
  }

  return this->IsSetFlags(DESCRIPTOR_FLAG_PARSED);
}

CDescriptor *CDescriptor::CreateDescriptor(void)
{
  HRESULT result = S_OK;
  CDescriptor *descriptor = new CDescriptor(&result);
  CHECK_POINTER_HRESULT(result, descriptor, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(descriptor));
  return descriptor;
}

bool CDescriptor::InternalClone(CDescriptor *descriptor)
{
  bool result = (descriptor != NULL);

  if (result)
  {
    descriptor->flags = this->flags;

    if (this->payloadSize != 0)
    {
      descriptor->payload = ALLOC_MEM_SET(descriptor->payload, uint8_t, this->payloadSize, 0);
      result &= (descriptor->payload != NULL);

      if (result)
      {
        descriptor->payloadSize = this->payloadSize;
        memcpy(descriptor->payload, this->payload, this->payloadSize);
      }
    }
  }

  return result;
}
