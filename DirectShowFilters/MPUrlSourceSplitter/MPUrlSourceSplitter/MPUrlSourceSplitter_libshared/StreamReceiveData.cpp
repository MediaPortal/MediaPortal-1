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

#include "StreamReceiveData.h"

CStreamReceiveData::CStreamReceiveData(void)
{
  this->endOfStreamReached = new CEndOfStreamReached();
  this->totalLength = new CSetTotalLength();
  this->mediaPackets = new CMediaPacketCollection();
  this->streamInputFormat = NULL;
  this->flags = STREAM_RECEIVE_DATA_FLAG_NONE;
}

CStreamReceiveData::~CStreamReceiveData(void)
{
  FREE_MEM_CLASS(this->endOfStreamReached);
  FREE_MEM_CLASS(this->totalLength);
  FREE_MEM_CLASS(this->mediaPackets);
  FREE_MEM(this->streamInputFormat);
}

/* get methods */

const wchar_t *CStreamReceiveData::GetStreamInputFormat(void)
{
  return this->streamInputFormat;
}

CSetTotalLength *CStreamReceiveData::GetTotalLength(void)
{
  return this->totalLength;
}

CMediaPacketCollection *CStreamReceiveData::GetMediaPacketCollection(void)
{
  return this->mediaPackets;
}

CEndOfStreamReached *CStreamReceiveData::GetEndOfStreamReached(void)
{
  return this->endOfStreamReached;
}

unsigned int CStreamReceiveData::GetFlags(void)
{
  return this->flags;
}

/* set methods */

bool CStreamReceiveData::SetStreamInputFormat(const wchar_t *streamInputFormat)
{
  SET_STRING_RETURN_WITH_NULL(this->streamInputFormat, streamInputFormat);
}

void CStreamReceiveData::SetFlags(unsigned int flags)
{
  this->flags = flags;
}

void CStreamReceiveData::SetContainer(bool container)
{
  this->flags &= ~STREAM_RECEIVE_DATA_FLAG_CONTAINER;
  this->flags |= (container) ? STREAM_RECEIVE_DATA_FLAG_CONTAINER : STREAM_RECEIVE_DATA_FLAG_NONE;
}

void CStreamReceiveData::SetPackets(bool packets)
{
  this->flags &= ~STREAM_RECEIVE_DATA_FLAG_PACKETS;
  this->flags |= (packets) ? STREAM_RECEIVE_DATA_FLAG_PACKETS : STREAM_RECEIVE_DATA_FLAG_NONE;
}

/* other methods */

bool CStreamReceiveData::IsContainer(void)
{
  return this->IsSetFlags(STREAM_RECEIVE_DATA_FLAG_CONTAINER);
}

bool CStreamReceiveData::IsPackets(void)
{
  return this->IsSetFlags(STREAM_RECEIVE_DATA_FLAG_PACKETS);
}

bool CStreamReceiveData::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

void CStreamReceiveData::Clear(void)
{
  this->endOfStreamReached->Clear();
  this->mediaPackets->Clear();
  this->totalLength->Clear();
  FREE_MEM(this->streamInputFormat);
  this->flags = STREAM_RECEIVE_DATA_FLAG_NONE;
}