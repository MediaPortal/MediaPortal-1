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

#include "RtspStreamFragment.h"

CRtspStreamFragment::CRtspStreamFragment(uint64_t fragmentStartTimestamp)
{
  this->flags = RTSP_STREAM_FRAGMENT_FLAG_NONE;
  this->fragmentStartTimestamp = fragmentStartTimestamp;
  this->fragmentEndTimestamp = UINT64_MAX;
  this->length = 0;
  this->storeFilePosition = -1;
  this->receivedData = new CLinearBuffer();
}

CRtspStreamFragment::~CRtspStreamFragment(void)
{
  FREE_MEM_CLASS(this->receivedData);
}

/* get methods */

uint64_t CRtspStreamFragment::GetFragmentStartTimestamp(void)
{
  return this->fragmentStartTimestamp;
}

uint64_t CRtspStreamFragment::GetFragmentEndTimestamp(void)
{
  return this->fragmentEndTimestamp;
}

int64_t CRtspStreamFragment::GetStoreFilePosition(void)
{
  return this->storeFilePosition;
}

unsigned int CRtspStreamFragment::GetLength(void)
{
  return (this->length == 0) ? this->receivedData->GetBufferOccupiedSpace() : this->length;
}

CLinearBuffer *CRtspStreamFragment::GetReceivedData(void)
{
  return this->receivedData;
}

/* set methods */

void CRtspStreamFragment::SetDownloaded(bool downloaded)
{
  this->flags &= ~RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED;
  this->flags |= (downloaded ? RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED : RTSP_STREAM_FRAGMENT_FLAG_NONE);
}

void CRtspStreamFragment::SetStoredToFile(int64_t position)
{
  this->flags &= ~RTSP_STREAM_FRAGMENT_FLAG_STORED_TO_FILE;
  this->storeFilePosition = position;

  if (this->storeFilePosition != (-1))
  {
    this->flags |= RTSP_STREAM_FRAGMENT_FLAG_STORED_TO_FILE;
    this->length = this->receivedData->GetBufferOccupiedSpace();
    this->receivedData->DeleteBuffer();
  }
}

void CRtspStreamFragment::SetFragmentStartTimestamp(uint64_t fragmentStartTimestamp)
{
  this->fragmentStartTimestamp = fragmentStartTimestamp;
}

void CRtspStreamFragment::SetFragmentEndTimestamp(uint64_t fragmentEndTimestamp)
{
  this->fragmentEndTimestamp = fragmentEndTimestamp;
}

/* other methods */

bool CRtspStreamFragment::IsStoredToFile(void)
{
  return this->IsFlags(RTSP_STREAM_FRAGMENT_FLAG_STORED_TO_FILE);
}

bool CRtspStreamFragment::IsDownloaded(void)
{
  return this->IsFlags(RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED);
}

bool CRtspStreamFragment::IsFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

CRtspStreamFragment *CRtspStreamFragment::Clone(void)
{
  CRtspStreamFragment *clone = new CRtspStreamFragment(this->fragmentStartTimestamp);
  bool result = (clone != NULL);

  if (result)
  {
    clone->flags = this->flags;
    clone->length = this->length;
    clone->storeFilePosition = this->storeFilePosition;
    clone->fragmentEndTimestamp = this->fragmentEndTimestamp;

    if (this->receivedData->GetBufferSize() != 0)
    {
      // some data allocated, must be cloned
      FREE_MEM_CLASS(clone->receivedData);
      clone->receivedData = this->receivedData->Clone();
      result &= (clone->receivedData != NULL);
    }
  }

  CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(clone));
  return clone;
}