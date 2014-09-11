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
  : CStreamFragment(result)
{
  this->fragmentRtpTimestamp = 0;
}

CRtspStreamFragment::CRtspStreamFragment(HRESULT *result, int64_t fragmentRtpTimestamp, bool setRtpTimestampFlag)
  : CStreamFragment(result)
{
  this->flags = (setRtpTimestampFlag) ? RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP : RTSP_STREAM_FRAGMENT_FLAG_NONE;
  this->fragmentRtpTimestamp = fragmentRtpTimestamp;
}

CRtspStreamFragment::~CRtspStreamFragment(void)
{
}

/* get methods */

int64_t CRtspStreamFragment::GetFragmentRtpTimestamp(void)
{
  return this->fragmentRtpTimestamp;
}

/* set methods */

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

/* other methods */

bool CRtspStreamFragment::IsSetFragmentRtpTimestamp(void)
{
  return this->IsSetFlags(RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP);
}

/* protected methods */

CFastSearchItem *CRtspStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CRtspStreamFragment *fragment = new CRtspStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CRtspStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CRtspStreamFragment *fragment = dynamic_cast<CRtspStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentRtpTimestamp = this->fragmentRtpTimestamp;
    }
  }

  return result;
}