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

CRtmpStreamFragment::CRtmpStreamFragment(HRESULT *result)
  : CStreamFragment(result)
{
  this->fragmentStartTimestamp = 0;
}

CRtmpStreamFragment::CRtmpStreamFragment(HRESULT *result, int64_t fragmentStartTimestamp, bool setStartTimestampFlag)
  : CStreamFragment(result)
{
  this->flags = (setStartTimestampFlag) ? RTMP_STREAM_FRAGMENT_FLAG_SET_TIMESTAMP : RTMP_STREAM_FRAGMENT_FLAG_NONE;
  this->fragmentStartTimestamp = fragmentStartTimestamp;
}

CRtmpStreamFragment::~CRtmpStreamFragment(void)
{
}

/* get methods */

int64_t CRtmpStreamFragment::GetFragmentStartTimestamp(void)
{
  return this->fragmentStartTimestamp;
}

/* set methods */

void CRtmpStreamFragment::SetFragmentStartTimestamp(int64_t fragmentStartTimestamp)
{
  this->SetFragmentStartTimestamp(fragmentStartTimestamp, true);
}

void CRtmpStreamFragment::SetFragmentStartTimestamp(int64_t fragmentStartTimestamp, bool setStartTimestampFlag)
{
  this->flags &= ~RTMP_STREAM_FRAGMENT_FLAG_SET_TIMESTAMP;
  this->fragmentStartTimestamp = fragmentStartTimestamp;
  this->flags |= setStartTimestampFlag ? RTMP_STREAM_FRAGMENT_FLAG_SET_TIMESTAMP : RTMP_STREAM_FRAGMENT_FLAG_NONE;
}

void CRtmpStreamFragment::SetContainsHeaderOrMetaPacket(bool containsHeaderOrMetaPacket)
{
  this->flags &= ~RTMP_STREAM_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET;
  this->flags |= containsHeaderOrMetaPacket ? RTMP_STREAM_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET : RTMP_STREAM_FRAGMENT_FLAG_NONE;
}

/* other methods */

bool CRtmpStreamFragment::IsSetFragmentStartTimestamp(void)
{
  return this->IsSetFlags(RTMP_STREAM_FRAGMENT_FLAG_SET_TIMESTAMP);
}

bool CRtmpStreamFragment::ContainsHeaderOrMetaPacket(void)
{
  return this->IsSetFlags(RTMP_STREAM_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET);
}

/* protected methods */

CFastSearchItem *CRtmpStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CRtmpStreamFragment *fragment = new CRtmpStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CRtmpStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CRtmpStreamFragment *fragment = dynamic_cast<CRtmpStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentStartPosition = this->fragmentStartPosition;
    }
  }

  return result;
}