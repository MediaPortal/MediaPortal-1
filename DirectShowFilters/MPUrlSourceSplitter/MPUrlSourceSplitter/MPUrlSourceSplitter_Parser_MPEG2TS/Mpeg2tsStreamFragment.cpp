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

#include "Mpeg2tsStreamFragment.h"
#include "FastSearchItemCollection.h"

CMpeg2tsStreamFragment::CMpeg2tsStreamFragment(HRESULT *result)
  : CStreamFragment(result)
{
  this->fragmentOriginalStartPosition = STREAM_FRAGMENT_START_POSITION_NOT_SET;
}

CMpeg2tsStreamFragment::~CMpeg2tsStreamFragment(void)
{
}

/* get methods */

int64_t CMpeg2tsStreamFragment::GetFragmentOriginalStartPosition(void)
{
  return this->fragmentOriginalStartPosition;
}

/* set methods */

void CMpeg2tsStreamFragment::SetFragmentOriginalStartPosition(int64_t fragmentOriginalStartPosition)
{
  this->fragmentOriginalStartPosition = fragmentOriginalStartPosition;
}

void CMpeg2tsStreamFragment::SetReadyForAlign(bool readyForAlign, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN;
  this->flags |= (readyForAlign) ? MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetAligned(bool aligned, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED;
  this->flags |= (aligned) ? MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetPartiallyProcessed(bool partiallyProcessed, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_PARTIALLY_PROCESSED;
  this->flags |= (partiallyProcessed) ? MPEG2TS_STREAM_FRAGMENT_FLAG_PARTIALLY_PROCESSED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

/* other methods */

bool CMpeg2tsStreamFragment::IsSetFragmentOriginalStartPosition(void)
{
  return (this->fragmentOriginalStartPosition != STREAM_FRAGMENT_START_POSITION_NOT_SET);
}

bool CMpeg2tsStreamFragment::IsReadyForAlign(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN);
}

bool CMpeg2tsStreamFragment::IsAligned(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED);
}

bool CMpeg2tsStreamFragment::IsPartiallyProcessed(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_PARTIALLY_PROCESSED);
}

/* protected methods */

CFastSearchItem *CMpeg2tsStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CMpeg2tsStreamFragment *item = new CMpeg2tsStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CMpeg2tsStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CMpeg2tsStreamFragment *fragment = dynamic_cast<CMpeg2tsStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentOriginalStartPosition = this->fragmentOriginalStartPosition;
    }
  }

  return result;
}