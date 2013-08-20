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

#include "RtmpStreamFragment.h"

CRtmpStreamFragment::CRtmpStreamFragment(void)
{
  this->fragmentStartTimestamp = 0;
  this->fragmentEndTimestamp = 0;
  this->downloaded = false;
  this->storeFilePosition = -1;
  this->length = 0;
  this->buffer = new CLinearBuffer();
  this->setStartTimestamp = false;
  this->seeked = false;
  this->hasIncorrectTimestamps = false;
  this->packetCorrection = 0;
}

CRtmpStreamFragment::~CRtmpStreamFragment(void)
{
  FREE_MEM_CLASS(this->buffer);
}

/* get methods */

uint64_t CRtmpStreamFragment::GetFragmentStartTimestamp(void)
{
  return this->fragmentStartTimestamp;
}

uint64_t CRtmpStreamFragment::GetFragmentEndTimestamp(void)
{
  return this->fragmentEndTimestamp;
}

int64_t CRtmpStreamFragment::GetStoreFilePosition(void)
{
  return this->storeFilePosition;
}

unsigned int CRtmpStreamFragment::GetLength(void)
{
  return (this->buffer != NULL) ? this->buffer->GetBufferOccupiedSpace() : this->length;
}

CLinearBuffer *CRtmpStreamFragment::GetBuffer()
{
  return this->buffer;
}

int CRtmpStreamFragment::GetPacketCorrection(void)
{
  return this->packetCorrection;
}

/* set methods */

void CRtmpStreamFragment::SetFragmentStartTimestamp(uint64_t fragmentStartTimestamp, bool setStartTimestamp)
{
  this->fragmentStartTimestamp = fragmentStartTimestamp;
  this->setStartTimestamp = setStartTimestamp;
}

void CRtmpStreamFragment::SetFragmentEndTimestamp(uint64_t fragmentEndTimestamp)
{
  this->fragmentEndTimestamp = fragmentEndTimestamp;
}

void CRtmpStreamFragment::SetDownloaded(bool downloaded)
{
  this->downloaded = downloaded;
}

void CRtmpStreamFragment::SetStoredToFile(int64_t position)
{
  this->storeFilePosition = position;
  if (this->storeFilePosition != (-1))
  {
    if (this->buffer != NULL)
    {
      this->length = this->buffer->GetBufferOccupiedSpace();
    }

    FREE_MEM_CLASS(this->buffer);
  }
  else
  {
    if (this->buffer == NULL)
    {
      this->buffer = new CLinearBuffer();
    }
  }
}

void CRtmpStreamFragment::SetSeeked(bool seeked)
{
  this->seeked = seeked;
}

void CRtmpStreamFragment::SetIncorrectTimestamps(bool hasIncorrectTimestamps)
{
  this->hasIncorrectTimestamps = hasIncorrectTimestamps;
}

void CRtmpStreamFragment::SetPacketCorrection(int packetCorrection)
{
  this->packetCorrection = packetCorrection;
}

/* other methods */

bool CRtmpStreamFragment::IsStoredToFile(void)
{
  return (this->storeFilePosition != (-1));
}

bool CRtmpStreamFragment::IsDownloaded(void)
{
  return this->downloaded;
}

bool CRtmpStreamFragment::IsStartTimestampSet(void)
{
  return this->setStartTimestamp;
}

bool CRtmpStreamFragment::IsSeeked(void)
{
  return this->seeked;
}

bool CRtmpStreamFragment::HasIncorrectTimestamps(void)
{
  return this->hasIncorrectTimestamps;
}