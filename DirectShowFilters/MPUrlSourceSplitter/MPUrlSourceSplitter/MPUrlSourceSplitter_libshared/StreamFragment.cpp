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

#include "StreamFragment.h"
#include "FastSearchItemCollection.h"

CStreamFragment::CStreamFragment(HRESULT *result)
  : CCacheFileItem(result)
{
  this->fragmentStartPosition = STREAM_FRAGMENT_START_POSITION_NOT_SET;
}

CStreamFragment::~CStreamFragment(void)
{
}

/* get methods */

int64_t CStreamFragment::GetFragmentStartPosition(void)
{
  return this->fragmentStartPosition;
}

/* set methods */

bool CStreamFragment::IsSetFragmentStartPosition(void)
{
  return (this->fragmentStartPosition != STREAM_FRAGMENT_START_POSITION_NOT_SET);
}

void CStreamFragment::SetFragmentStartPosition(int64_t fragmentStartPosition)
{
  this->fragmentStartPosition = fragmentStartPosition;
}

void CStreamFragment::SetDownloaded(bool downloaded, unsigned int streamFragmentIndex)
{
  this->flags &= ~STREAM_FRAGMENT_FLAG_DOWNLOADED;
  this->flags |= (downloaded) ? STREAM_FRAGMENT_FLAG_DOWNLOADED : STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CStreamFragment::SetProcessed(bool processed, unsigned int streamFragmentIndex)
{
  this->flags &= ~STREAM_FRAGMENT_FLAG_PROCESSED;
  this->flags |= (processed) ? STREAM_FRAGMENT_FLAG_PROCESSED : STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CStreamFragment::SetDiscontinuity(bool discontinuity, unsigned int streamFragmentIndex)
{
  this->flags &= ~STREAM_FRAGMENT_FLAG_DISCONTINUITY;
  this->flags |= (discontinuity) ? STREAM_FRAGMENT_FLAG_DISCONTINUITY : STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

/* other methods */

bool CStreamFragment::IsDownloaded(void)
{
  return this->IsSetFlags(STREAM_FRAGMENT_FLAG_DOWNLOADED);
}

bool CStreamFragment::IsProcessed(void)
{
  return this->IsSetFlags(STREAM_FRAGMENT_FLAG_PROCESSED);
}

bool CStreamFragment::IsDiscontinuity(void)
{
  return this->IsSetFlags(STREAM_FRAGMENT_FLAG_DISCONTINUITY);
}

/* protected methods */

CFastSearchItem *CStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CStreamFragment *item = new CStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CStreamFragment *fragment = dynamic_cast<CStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentStartPosition = this->fragmentStartPosition;
    }
  }

  return result;
}