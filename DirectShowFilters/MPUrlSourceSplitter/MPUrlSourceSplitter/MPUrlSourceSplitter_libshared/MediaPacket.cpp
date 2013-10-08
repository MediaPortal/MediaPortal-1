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

#include "MediaPacket.h"

CMediaPacket::CMediaPacket(void)
{
  this->buffer = new CLinearBuffer();
  this->buffer->DeleteBuffer();

  this->start = 0;
  this->end = 0;
  this->storeFilePosition = -1;
}

CMediaPacket::~CMediaPacket(void)
{
  FREE_MEM_CLASS(this->buffer);
}

CLinearBuffer *CMediaPacket::GetBuffer()
{
  return this->buffer;
}

CMediaPacket *CMediaPacket::Clone(void)
{
  CMediaPacket *clone = new CMediaPacket();
  clone->start = this->start;
  clone->end = this->end;
  clone->storeFilePosition = this->storeFilePosition;

  // because in clone is created linear buffer we need to delete clone buffer
  FREE_MEM_CLASS(clone->buffer);

  if (!this->IsStoredToFile())
  {
    // media packet is not stored in file
    // we need to copy buffer in that case
    clone->buffer = this->buffer->Clone();

    if (clone->buffer == NULL)
    {
      // error occured while cloning current instance
      FREE_MEM_CLASS(clone);
    }
  }

  return clone;
}

CMediaPacket *CMediaPacket::CreateMediaPacketBasedOnPacket(int64_t start, int64_t end)
{
  CMediaPacket *mediaPacket = new CMediaPacket();
  unsigned char *buffer = NULL;
  bool success = ((start >= this->start) && (end >= start) && (this->GetBuffer() != NULL));

  if (success)
  {
    // initialize new media packet start, end and length
    unsigned int length = (unsigned int)(end - start + 1);

    // initialize new media packet data
    mediaPacket->SetStart(start);
    mediaPacket->SetEnd(end);
    if (success)
    {
      success = (mediaPacket->GetBuffer()->InitializeBuffer(length));
    }

    if (success)
    {
      // create temporary buffer and copy data from unprocessed media packet
      buffer = ALLOC_MEM_SET(buffer, unsigned char, length, 0);
      success = (buffer != NULL);
    }

    if (success)
    {
      success = (this->GetBuffer()->CopyFromBuffer(buffer, length, (unsigned int)(start - this->start)) == length);
    }

    if (success)
    {
      // add data from temporary buffer to first part media packet
      success = (mediaPacket->GetBuffer()->AddToBuffer(buffer, length) == length);

      // remove temporary buffer
      FREE_MEM(buffer);
    }
  }

  if (!success)
  {
    FREE_MEM_CLASS(mediaPacket);
  }

  return mediaPacket;
}

int64_t CMediaPacket::GetStart(void)
{
  return this->start;
}

int64_t CMediaPacket::GetEnd(void)
{
  return this->end;
}


void CMediaPacket::SetStart(int64_t position)
{
  this->start = position;
}

void CMediaPacket::SetEnd(int64_t position)
{
  this->end = position;
}

bool CMediaPacket::IsStoredToFile(void)
{
  return (this->storeFilePosition != (-1));
}

void CMediaPacket::SetStoredToFile(LONGLONG position)
{
  this->storeFilePosition = position;
  if (this->storeFilePosition != (-1))
  {
    if (this->buffer != NULL)
    {
      delete this->buffer;
    }
    this->buffer = NULL;
  }
  else
  {
    if (this->buffer == NULL)
    {
      this->buffer = new CLinearBuffer();
      this->buffer->DeleteBuffer();
    }
  }
}

LONGLONG CMediaPacket::GetStoreFilePosition(void)
{
  return this->storeFilePosition;
}