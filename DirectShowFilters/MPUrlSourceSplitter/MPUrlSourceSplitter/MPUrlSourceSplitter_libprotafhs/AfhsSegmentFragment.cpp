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
#include "FastSearchItemCollection.h"

CAfhsSegmentFragment::CAfhsSegmentFragment(HRESULT *result, unsigned int segment, unsigned int fragment)
  : CStreamFragment(result)
{
  this->segment = segment;
  this->fragment = fragment;
  this->fragmentTimestamp = 0;
}

CAfhsSegmentFragment::CAfhsSegmentFragment(HRESULT *result, unsigned int segment, unsigned int fragment, int64_t fragmentTimestamp)
  : CStreamFragment(result)
{
  this->segment = segment;
  this->fragment = fragment;
  this->fragmentTimestamp = fragmentTimestamp;
}

CAfhsSegmentFragment::~CAfhsSegmentFragment(void)
{
}

/* get methods */

int64_t CAfhsSegmentFragment::GetFragmentTimestamp(void)
{
  return this->fragmentTimestamp;
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

void CAfhsSegmentFragment::SetDecrypted(bool decrypted, unsigned int segmentFragmentItemIndex)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED;
  this->flags |= decrypted ? AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED : AFHS_SEGMENT_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (segmentFragmentItemIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(segmentFragmentItemIndex);
  }
}

void CAfhsSegmentFragment::SetEncrypted(bool encrypted, unsigned int segmentFragmentItemIndex)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_ENCRYPTED;
  this->flags |= encrypted ? AFHS_SEGMENT_FRAGMENT_FLAG_ENCRYPTED : AFHS_SEGMENT_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (segmentFragmentItemIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(segmentFragmentItemIndex);
  }
}

void CAfhsSegmentFragment::SetContainsHeaderOrMetaPacket(bool containsHeaderOrMetaPacket)
{
  this->flags &= ~AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET;
  this->flags |= containsHeaderOrMetaPacket ? AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET : AFHS_SEGMENT_FRAGMENT_FLAG_NONE;
}

/* other methods */

bool CAfhsSegmentFragment::IsDecrypted(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED);
}

bool CAfhsSegmentFragment::IsEncrypted(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_ENCRYPTED);
}

bool CAfhsSegmentFragment::ContainsHeaderOrMetaPacket(void)
{
  return this->IsSetFlags(AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET);
}

/* protected methods */

CFastSearchItem *CAfhsSegmentFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CAfhsSegmentFragment *fragment = new CAfhsSegmentFragment(&result, this->segment, this->fragment);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CAfhsSegmentFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CAfhsSegmentFragment *fragment = dynamic_cast<CAfhsSegmentFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->fragmentTimestamp = this->fragmentTimestamp;
    }
  }

  return result;
}