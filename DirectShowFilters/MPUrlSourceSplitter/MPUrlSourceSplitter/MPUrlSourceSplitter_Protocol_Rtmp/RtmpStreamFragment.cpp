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
  : CCacheFileItem()
{
  this->fragmentStartTimestamp = 0;
  this->fragmentEndTimestamp = 0;
  this->packetCorrection = 0;
}

CRtmpStreamFragment::~CRtmpStreamFragment(void)
{
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

int CRtmpStreamFragment::GetPacketCorrection(void)
{
  return this->packetCorrection;
}

/* set methods */

void CRtmpStreamFragment::SetFragmentStartTimestamp(uint64_t fragmentStartTimestamp, bool setStartTimestamp)
{
  this->fragmentStartTimestamp = fragmentStartTimestamp;
  
  this->flags &= ~RTMP_STREAM_FRAGMENT_FLAG_SET_START_TIMESTAMP;
  this->flags |= (setStartTimestamp) ? RTMP_STREAM_FRAGMENT_FLAG_SET_START_TIMESTAMP : RTMP_STREAM_FRAGMENT_FLAG_NONE;
}

void CRtmpStreamFragment::SetFragmentEndTimestamp(uint64_t fragmentEndTimestamp)
{
  this->fragmentEndTimestamp = fragmentEndTimestamp;
}

void CRtmpStreamFragment::SetDownloaded(bool downloaded)
{
  this->flags &= ~RTMP_STREAM_FRAGMENT_FLAG_DOWNLOADED;

  if (downloaded)
  {
    this->flags |= RTMP_STREAM_FRAGMENT_FLAG_DOWNLOADED;
    this->SetLoadedToMemoryTime(GetTickCount());
  }
  else
  {
    this->SetLoadedToMemoryTime(CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET);
  }
}

void CRtmpStreamFragment::SetSeeked(bool seeked)
{
  this->flags &= ~RTMP_STREAM_FRAGMENT_FLAG_SEEKED;
  this->flags |= (seeked) ? RTMP_STREAM_FRAGMENT_FLAG_SEEKED : RTMP_STREAM_FRAGMENT_FLAG_NONE;
}

void CRtmpStreamFragment::SetIncorrectTimestamps(bool hasIncorrectTimestamps)
{
  this->flags &= ~RTMP_STREAM_FRAGMENT_FLAG_HAS_INCORRECT_TIMESTAMPS;
  this->flags |= (hasIncorrectTimestamps) ? RTMP_STREAM_FRAGMENT_FLAG_HAS_INCORRECT_TIMESTAMPS : RTMP_STREAM_FRAGMENT_FLAG_NONE;
}

void CRtmpStreamFragment::SetPacketCorrection(int packetCorrection)
{
  this->packetCorrection = packetCorrection;
}

/* other methods */

bool CRtmpStreamFragment::IsDownloaded(void)
{
  return this->IsSetFlags(RTMP_STREAM_FRAGMENT_FLAG_DOWNLOADED);
}

bool CRtmpStreamFragment::IsStartTimestampSet(void)
{
  return this->IsSetFlags(RTMP_STREAM_FRAGMENT_FLAG_SET_START_TIMESTAMP);
}

bool CRtmpStreamFragment::IsSeeked(void)
{
  return this->IsSetFlags(RTMP_STREAM_FRAGMENT_FLAG_SEEKED);
}

bool CRtmpStreamFragment::HasIncorrectTimestamps(void)
{
  return this->IsSetFlags(RTMP_STREAM_FRAGMENT_FLAG_HAS_INCORRECT_TIMESTAMPS);
}

/* protected methods */

CCacheFileItem *CRtmpStreamFragment::CreateItem(void)
{
  return new CRtmpStreamFragment();
}

bool CRtmpStreamFragment::InternalClone(CCacheFileItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CRtmpStreamFragment *fragment = dynamic_cast<CRtmpStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentStartTimestamp = this->fragmentStartTimestamp;
      fragment->fragmentEndTimestamp = this->fragmentEndTimestamp;
      fragment->packetCorrection = this->packetCorrection;
    }
  }

  return result;
}