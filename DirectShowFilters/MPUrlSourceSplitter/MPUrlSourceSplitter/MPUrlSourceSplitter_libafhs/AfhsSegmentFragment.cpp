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

#include "AfhsSegmentFragment.h"

CAfhsSegmentFragment::CAfhsSegmentFragment(HRESULT *result, unsigned int segment, unsigned int fragment)
  : CCacheFileItem(result)
{
  this->segment = segment;
  this->fragment = fragment;
  this->fragmentTimestamp = 0;
  this->fragmentStartPosition = AFHS_SEGMENT_FRAGMENT_START_POSITION_NOT_SET;
}

CAfhsSegmentFragment::CAfhsSegmentFragment(HRESULT *result, unsigned int segment, unsigned int fragment, int64_t fragmentTimestamp)
  : CCacheFileItem(result)
{
  this->segment = segment;
  this->fragment = fragment;
  this->fragmentTimestamp = fragmentTimestamp;
  this->fragmentStartPosition = AFHS_SEGMENT_FRAGMENT_START_POSITION_NOT_SET;
}

CAfhsSegmentFragment::~CAfhsSegmentFragment(void)
{
}

/* get methods */

int64_t CAfhsSegmentFragment::GetFragmentTimestamp(void)
{
  return this->fragmentTimestamp;
}

int64_t CAfhsSegmentFragment::GetFragmentStartPosition(void)
{
  return this->fragmentStartPosition;
}

unsigned int CAfhsSegmentFragment::GetSegment(void)
{
  return this->segment;
}

unsigned int CAfhsSegmentFragment::GetFragment(void)
{
  return this->fragment;
}

/* set methods */

void CAfhsSegmentFragment::SetDownloaded(bool downloaded)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_DOWNLOADED;

  if (downloaded)
  {
    this->flags |= AFHS_SEGMENT_FRAGMENT_FLAG_DOWNLOADED;
    this->SetLoadedToMemoryTime(GetTickCount());
  }
  else
  {
    this->SetLoadedToMemoryTime(CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET);
  }
}

void CAfhsSegmentFragment::SetDecrypted(bool decrypted)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED;
  this->flags |= decrypted ? AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED : AFHS_SEGMENT_FRAGMENT_FLAG_NONE;
}

void CAfhsSegmentFragment::SetFragmentStartPosition(int64_t fragmentStartPosition)
{
  this->fragmentStartPosition = fragmentStartPosition;
}

void CAfhsSegmentFragment::SetDiscontinuity(bool discontinuity)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_DISCONTINUITY;
  this->flags |= discontinuity ? AFHS_SEGMENT_FRAGMENT_FLAG_DISCONTINUITY : AFHS_SEGMENT_FRAGMENT_FLAG_NONE;
}

void CAfhsSegmentFragment::SetContainsHeaderOrMetaPacket(bool containsHeaderOrMetaPacket)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET;
  this->flags |= containsHeaderOrMetaPacket ? AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET : AFHS_SEGMENT_FRAGMENT_FLAG_NONE;
}

/* other methods */

bool CAfhsSegmentFragment::IsDiscontinuity(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_DISCONTINUITY);
}

bool CAfhsSegmentFragment::IsDownloaded(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_DOWNLOADED);
}

bool CAfhsSegmentFragment::IsDecrypted(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED);
}

bool CAfhsSegmentFragment::IsSetFragmentStartPosition(void)
{
  return (this->fragmentStartPosition != AFHS_SEGMENT_FRAGMENT_START_POSITION_NOT_SET);
}

bool CAfhsSegmentFragment::ContainsHeaderOrMetaPacket(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET);
}

/* protected methods */

CCacheFileItem *CAfhsSegmentFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CAfhsSegmentFragment *fragment = new CAfhsSegmentFragment(&result, this->segment, this->fragment);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CAfhsSegmentFragment::InternalClone(CCacheFileItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CAfhsSegmentFragment *fragment = dynamic_cast<CAfhsSegmentFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentTimestamp = this->fragmentTimestamp;
      fragment->fragmentStartPosition = this->fragmentStartPosition;
    }
  }

  return result;
}