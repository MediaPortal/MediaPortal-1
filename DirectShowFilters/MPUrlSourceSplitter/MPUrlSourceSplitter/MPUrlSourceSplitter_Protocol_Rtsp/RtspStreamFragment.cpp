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

CRtspStreamFragment::CRtspStreamFragment(HRESULT *result)
  : CCacheFileItem(result)
{
  this->fragmentRtpTimestamp = 0;
  this->fragmentStartPosition = RTSP_STREAM_FRAGMENT_START_POSITION_NOT_SET;
}

CRtspStreamFragment::CRtspStreamFragment(HRESULT *result, int64_t fragmentRtpTimestamp, bool setRtpTimestampFlag)
  : CCacheFileItem(result)
{
  this->flags = (setRtpTimestampFlag) ? RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP : RTSP_STREAM_FRAGMENT_FLAG_NONE;
  this->fragmentRtpTimestamp = fragmentRtpTimestamp;
  this->fragmentStartPosition = RTSP_STREAM_FRAGMENT_START_POSITION_NOT_SET;
}

CRtspStreamFragment::~CRtspStreamFragment(void)
{
}

/* get methods */

int64_t CRtspStreamFragment::GetFragmentRtpTimestamp(void)
{
  return this->fragmentRtpTimestamp;
}

int64_t CRtspStreamFragment::GetFragmentStartPosition(void)
{
  return this->fragmentStartPosition;
}

/* set methods */

void CRtspStreamFragment::SetDownloaded(bool downloaded)
{
  this->flags &= ~RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED;

  if (downloaded)
  {
    this->flags |= RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED;
    this->SetLoadedToMemoryTime(GetTickCount());
  }
  else
  {
    this->SetLoadedToMemoryTime(CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET);
  }
}

void CRtspStreamFragment::SetFragmentRtpTimestamp(int64_t fragmentRtpTimestamp)
{
  this->SetFragmentRtpTimestamp(fragmentRtpTimestamp, true);
}

void CRtspStreamFragment::SetFragmentRtpTimestamp(int64_t fragmentRtpTimestamp, bool setRtpTimestampFlag)
{
  this->flags &= ~RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP;
  this->fragmentRtpTimestamp = fragmentRtpTimestamp;
  this->flags |= setRtpTimestampFlag ? RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP : RTSP_STREAM_FRAGMENT_FLAG_NONE;
}

void CRtspStreamFragment::SetFragmentStartPosition(int64_t fragmentStartPosition)
{
  this->fragmentStartPosition = fragmentStartPosition;
}

void CRtspStreamFragment::SetDiscontinuity(bool discontinuity)
{
  this->flags &= ~RTSP_STREAM_FRAGMENT_FLAG_DISCONTINUITY;
  this->flags |= discontinuity ? RTSP_STREAM_FRAGMENT_FLAG_DISCONTINUITY : RTSP_STREAM_FRAGMENT_FLAG_NONE;
}

/* other methods */

bool CRtspStreamFragment::IsDiscontinuity(void)
{
  return this->IsSetFlags(RTSP_STREAM_FRAGMENT_FLAG_DISCONTINUITY);
}

bool CRtspStreamFragment::IsDownloaded(void)
{
  return this->IsSetFlags(RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED);
}

bool CRtspStreamFragment::IsSetFragmentRtpTimestamp(void)
{
  return this->IsSetFlags(RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP);
}

bool CRtspStreamFragment::IsSetFragmentStartPosition(void)
{
  return (this->fragmentStartPosition != RTSP_STREAM_FRAGMENT_START_POSITION_NOT_SET);
}

/* protected methods */

CCacheFileItem *CRtspStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CRtspStreamFragment *fragment = new CRtspStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CRtspStreamFragment::InternalClone(CCacheFileItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CRtspStreamFragment *fragment = dynamic_cast<CRtspStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentRtpTimestamp = this->fragmentRtpTimestamp;
      fragment->fragmentStartPosition = this->fragmentStartPosition;
    }
  }

  return result;
}