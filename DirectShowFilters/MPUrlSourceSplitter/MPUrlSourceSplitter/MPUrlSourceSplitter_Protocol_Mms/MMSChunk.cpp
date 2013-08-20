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

#include "MMSChunk.h"


MMSChunk::MMSChunk(void)
{
  this->chunkData = NULL;
  this->extraHeaderData = NULL;

  this->Clear();
}

MMSChunk::MMSChunk(MMSChunk *mmsChunk)
{
  this->chunkData = NULL;
  this->extraHeaderData = NULL;

  this->Clear();

  if (mmsChunk != NULL)
  {
    bool result = true;

    result &= this->SetChunkDataLength(mmsChunk->GetChunkDataLength());
    result &= this->SetExtraHeaderDataLength(mmsChunk->GetExtraHeaderDataLength());

    if (result)
    {
      this->SetChunkType(mmsChunk->GetChunkType());
      memcpy(this->chunkData, mmsChunk->GetChunkData(), this->chunkDataLength);
      memcpy(this->extraHeaderData, mmsChunk->GetExtraHeaderData(), this->extraHeaderLength);
    }
    else
    {
      this->Clear();
    }
  }
}

MMSChunk::~MMSChunk(void)
{
  this->Clear();
}

void MMSChunk::Clear(void)
{
  FREE_MEM(this->chunkData);
  FREE_MEM(this->extraHeaderData);
  this->chunkDataLength = 0;
  this->extraHeaderLength = 0;
  this->chunkType = 0;
}

bool MMSChunk::SetChunkDataLength(unsigned int chunkDataLength)
{
  FREE_MEM(this->chunkData);
  this->chunkData = ALLOC_MEM_SET(this->chunkData, unsigned char, chunkDataLength, 0);
  if (this->chunkData != NULL)
  {
    this->chunkDataLength = chunkDataLength;
    return true;
  }

  this->chunkDataLength = 0;
  return false;
}

unsigned int MMSChunk::GetChunkDataLength(void)
{
  return this->chunkDataLength;
}

bool MMSChunk::SetExtraHeaderDataLength(unsigned int extraHeaderLength)
{
  FREE_MEM(this->extraHeaderData);
  this->extraHeaderData = ALLOC_MEM_SET(this->extraHeaderData, unsigned char, extraHeaderLength, 0);
  if (this->extraHeaderData != NULL)
  {
    this->extraHeaderLength = extraHeaderLength;
    return true;
  }

  this->extraHeaderLength = 0;
  return false;
}

unsigned int MMSChunk::GetExtraHeaderDataLength(void)
{
  return this->extraHeaderLength;
}

void MMSChunk::SetChunkType(unsigned int chunkType)
{
  this->chunkType = chunkType;
}

unsigned int MMSChunk::GetChunkType(void)
{
  return this->chunkType;
}

const unsigned char *MMSChunk::GetChunkData(void)
{
  return this->chunkData;
}

const unsigned char *MMSChunk::GetExtraHeaderData(void)
{
  return this->extraHeaderData;
}

bool MMSChunk::IsCleared(void)
{
  return ((this->chunkData == NULL) && (this->extraHeaderData == NULL) && (this->chunkDataLength == 0) && (this->extraHeaderLength == 0) && (this->chunkType == 0));
}

bool MMSChunk::SetChunkData(const unsigned char *chunkData, unsigned int length)
{
  bool result = this->SetChunkDataLength(length);

  if (result)
  {
    memcpy(this->chunkData, chunkData, length);
  }

  return result;
}

bool MMSChunk::SetExtraHeaderData(const unsigned char *extraHeaderData, unsigned int length)
{
  bool result = this->SetExtraHeaderDataLength(length);

  if (result)
  {
    memcpy(this->extraHeaderData, extraHeaderData, length);
  }

  return result;
}