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

#include "OutputPinPacket.h"

COutputPinPacket::COutputPinPacket(void)
{
  this->buffer = NULL;
  this->flags = FLAG_NONE;
  this->startTime = COutputPinPacket::INVALID_TIME;
  this->endTime = COutputPinPacket::INVALID_TIME;
  this->mediaType = NULL;
  this->streamPid = STREAM_PID_UNSPECIFIED;
}

COutputPinPacket::~COutputPinPacket(void)
{
  FREE_MEM_CLASS(this->buffer);
  DeleteMediaType(this->mediaType);
  this->mediaType = NULL;
}

/* get methods */

CLinearBuffer *COutputPinPacket::GetBuffer(void)
{
  return this->buffer;
}

REFERENCE_TIME COutputPinPacket::GetStartTime(void)
{
  return this->startTime;
}

REFERENCE_TIME COutputPinPacket::GetEndTime(void)
{
  return this->endTime;
}

AM_MEDIA_TYPE *COutputPinPacket::GetMediaType(void)
{
  return this->mediaType;
}

unsigned int COutputPinPacket::GetStreamPid(void)
{
  return this->streamPid;
}

unsigned int COutputPinPacket::GetFlags(void)
{
  return this->flags;
}

/* set methods */

void COutputPinPacket::SetDiscontinuity(bool discontinuity)
{
  this->flags &= ~FLAG_DISCONTINUITY;
  this->flags |= (discontinuity) ? FLAG_DISCONTINUITY : FLAG_NONE;
}

void COutputPinPacket::SetSyncPoint(bool syncPoint)
{
  this->flags &= ~FLAG_SYNC_POINT;
  this->flags |= (syncPoint) ? FLAG_SYNC_POINT : FLAG_NONE;
}

void COutputPinPacket::SetEndOfStream(bool endOfStream)
{
  this->flags &= ~FLAG_END_OF_STREAM;
  this->flags |= (endOfStream) ? FLAG_END_OF_STREAM : FLAG_NONE;
}

void COutputPinPacket::SetFlags(unsigned int flags)
{
  this->flags = flags;
}

void COutputPinPacket::SetStartTime(REFERENCE_TIME startTime)
{
  this->startTime = startTime;
}

void COutputPinPacket::SetEndTime(REFERENCE_TIME endTime)
{
  this->endTime = endTime;
}

void COutputPinPacket::SetMediaType(AM_MEDIA_TYPE *mediaType)
{
  this->mediaType = mediaType;
}

void COutputPinPacket::SetStreamPid(unsigned int streamPid)
{
  this->streamPid = streamPid;
}

/* other methods */

bool COutputPinPacket::IsDiscontinuity(void)
{
  return this->IsSetFlags(FLAG_DISCONTINUITY);
}

bool COutputPinPacket::IsSyncPoint(void)
{
  return this->IsSetFlags(FLAG_SYNC_POINT);
}

bool COutputPinPacket::IsEndOfStream(void)
{
  return this->IsSetFlags(FLAG_END_OF_STREAM);
}

bool COutputPinPacket::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

bool COutputPinPacket::CreateBuffer(unsigned int size)
{
  FREE_MEM_CLASS(this->buffer);

  this->buffer = new CLinearBuffer(size);

  return (this->buffer != NULL);
}