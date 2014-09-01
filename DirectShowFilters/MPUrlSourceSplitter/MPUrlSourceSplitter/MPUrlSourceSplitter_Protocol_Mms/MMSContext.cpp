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

#include "MMSContext.h"

MMSContext::MMSContext(HRESULT *result)
{
  this->buffer = NULL;
  this->streams = NULL;
  this->headerParsed = false;
  this->requestSequenceNumber = 1;
  this->chunkSequence = 0;
  this->finishTime = 0;

  this->asfHeader = NULL;
  this->asfHeaderLength = 0;
  this->asfPacketLength = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->buffer = new CLinearBuffer(result, HEADER_BUFFER_SIZE);
    this->streams = new MMSStreamCollection(result);

    CHECK_POINTER_HRESULT(*result, this->buffer, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streams, *result, E_OUTOFMEMORY);
  }
}

MMSContext::~MMSContext()
{
  FREE_MEM_CLASS(this->buffer);
  FREE_MEM_CLASS(this->streams);
  FREE_MEM(this->asfHeader);
}

/* get methods */

CLinearBuffer *MMSContext::GetBuffer(void)
{
  return this->buffer;
}

MMSStreamCollection *MMSContext::GetStreams(void)
{
  return this->streams;
}

bool MMSContext::GetHeaderParsed(void)
{
  return this->headerParsed;
}

unsigned int MMSContext::GetChunkSequence(void)
{
  return this->chunkSequence;
}

const unsigned char *MMSContext::GetAsfHeader(void)
{
  return this->asfHeader;
}

unsigned int MMSContext::GetAsfHeaderLength(void)
{
  return this->asfHeaderLength;
}

unsigned int MMSContext::GetAsfPacketLength(void)
{
  return this->asfPacketLength;
}

unsigned int MMSContext::GetFinishTime(void)
{
  return this->finishTime;
}

/* set methods */

void MMSContext::SetHeaderParsed(bool headerParsed)
{
  this->headerParsed = headerParsed;
}

void MMSContext::SetChunkSequence(unsigned int chunkSequence)
{
  this->chunkSequence = chunkSequence;
}

bool MMSContext::SetAsfHeader(const unsigned char *asfHeader, unsigned int length)
{
  bool result = false;
  this->ClearAsfHeader();

  if (this->InitializeAsfHeader(length))
  {
    memcpy(this->asfHeader, asfHeader, length);
    result = true;
  }

  return result;
}

void MMSContext::SetAsfPacketLength(unsigned int asfPacketLength)
{
  this->asfPacketLength = asfPacketLength;
}

void MMSContext::SetFinishTime(unsigned int finishTime)
{
  this->finishTime = finishTime;
}

/* other methods */

bool MMSContext::IsValid(void)
{
  return ((this->buffer != NULL) && (this->streams != NULL) && (this->buffer->GetBufferSize() != 0));
}

bool MMSContext::InitializeAsfHeader(unsigned int asfHeaderLength)
{
  bool result = false;

  unsigned char *header = ALLOC_MEM_SET(header, unsigned char, asfHeaderLength, 0);
  if (header != NULL)
  {
    FREE_MEM(this->asfHeader);
    this->asfHeader = header;
    this->asfHeaderLength = asfHeaderLength;
    result = true;
  }

  return result;
}

void MMSContext::ClearAsfHeader(void)
{
  FREE_MEM(this->asfHeader);
  this->asfHeaderLength = 0;
}