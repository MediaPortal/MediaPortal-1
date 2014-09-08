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

#include "MshsStreamFragment.h"
#include "FastSearchItemCollection.h"

CMshsStreamFragment::CMshsStreamFragment(HRESULT *result, int64_t fragmentTimestamp)
  : CCacheFileItem(result)
{
  this->fragmentTimestamp = fragmentTimestamp;
  this->fragmentStartPosition = MSHS_STREAM_FRAGMENT_START_POSITION_NOT_SET;
  this->url = NULL;
}

CMshsStreamFragment::CMshsStreamFragment(HRESULT *result, int64_t fragmentTimestamp, const wchar_t *url, unsigned int flags)
  : CCacheFileItem(result)
{
  this->fragmentTimestamp = fragmentTimestamp;
  this->fragmentStartPosition = MSHS_STREAM_FRAGMENT_START_POSITION_NOT_SET;
  this->flags |= flags;
  this->url = NULL;
  
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, url);

    if (SUCCEEDED(*result))
    {
      this->url = Duplicate(url);

      CHECK_POINTER_HRESULT(*result, this->url, *result, E_OUTOFMEMORY);
    }
  }
}

CMshsStreamFragment::~CMshsStreamFragment(void)
{
  FREE_MEM(this->url);
}

/* get methods */

int64_t CMshsStreamFragment::GetFragmentTimestamp(void)
{
  return this->fragmentTimestamp;
}

int64_t CMshsStreamFragment::GetFragmentStartPosition(void)
{
  return this->fragmentStartPosition;
}

const wchar_t *CMshsStreamFragment::GetUrl(void)
{
  return this->url;
}

/* set methods */

void CMshsStreamFragment::SetFragmentStartPosition(int64_t fragmentStartPosition)
{
  this->fragmentStartPosition = fragmentStartPosition;
}

void CMshsStreamFragment::SetReadyForProcessing(bool readyForProcessing, unsigned int streamFragmentItemIndex)
{
  this->flags &= ~MSHS_STREAM_FRAGMENT_FLAG_READY_FOR_PROCESSING;
  this->flags |= (readyForProcessing) ? MSHS_STREAM_FRAGMENT_FLAG_READY_FOR_PROCESSING : MSHS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentItemIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentItemIndex);
  }
}

/* other methods */

bool CMshsStreamFragment::IsSetFragmentStartPosition(void)
{
  return (this->fragmentStartPosition != MSHS_STREAM_FRAGMENT_START_POSITION_NOT_SET);
}

bool CMshsStreamFragment::IsReadyForProcessing(void)
{
  return this->IsSetFlags(MSHS_STREAM_FRAGMENT_FLAG_READY_FOR_PROCESSING);
}

bool CMshsStreamFragment::IsVideo(void)
{
  return this->IsSetFlags(MSHS_STREAM_FRAGMENT_FLAG_VIDEO);
}

bool CMshsStreamFragment::IsAudio(void)
{
  return this->IsSetFlags(MSHS_STREAM_FRAGMENT_FLAG_AUDIO);
}

bool CMshsStreamFragment::ContainsReconstructedHeader(void)
{
  return this->IsSetFlags(MSHS_STREAM_FRAGMENT_FLAG_CONTAINS_RECONSTRUCTED_HEADER);
}

/* protected methods */

CFastSearchItem *CMshsStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CMshsStreamFragment *fragment = new CMshsStreamFragment(&result, this->fragmentTimestamp, this->url, this->flags);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CMshsStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CMshsStreamFragment *fragment = dynamic_cast<CMshsStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentStartPosition = this->fragmentStartPosition;
    }
  }

  return result;
}