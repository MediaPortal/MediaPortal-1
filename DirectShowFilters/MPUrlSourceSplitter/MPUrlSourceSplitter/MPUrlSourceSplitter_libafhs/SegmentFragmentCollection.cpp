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

#include "SegmentFragmentCollection.h"

CSegmentFragmentCollection::CSegmentFragmentCollection(void)
  : CKeyedCollection()
{
  this->defaultBaseUrl = NULL;
  this->segmentFragmentUrlExtraParameters = NULL;
}

CSegmentFragmentCollection::~CSegmentFragmentCollection(void)
{
  FREE_MEM(this->defaultBaseUrl);
  FREE_MEM(this->segmentFragmentUrlExtraParameters);
}

int CSegmentFragmentCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
{
  bool invariant = (*(bool *)context);

  if (invariant)
  {
    return _wcsicmp(firstKey, secondKey);
  }
  else
  {
    return wcscmp(firstKey, secondKey);
  }
}

const wchar_t *CSegmentFragmentCollection::GetKey(CSegmentFragment *item)
{
  return L"";
}

CSegmentFragment *CSegmentFragmentCollection::Clone(CSegmentFragment *item)
{
  return item->Clone();
}

CSegmentFragment *CSegmentFragmentCollection::GetSegmentFragment(const wchar_t *url, bool invariant)
{
  return this->GetItem(url, (void *)&invariant);
}

unsigned int CSegmentFragmentCollection::GetFirstNotDownloadedSegmentFragment(unsigned int start)
{
  unsigned int result = UINT_MAX;

  for (unsigned int i = start; i < this->Count(); i++)
  {
    if (!this->GetItem(i)->IsDownloaded())
    {
      result = i;
      break;
    }
  }

  return result;
}

unsigned int CSegmentFragmentCollection::GetFirstNotProcessedSegmentFragment(unsigned int start)
{
  unsigned int result = UINT_MAX;

  for (unsigned int i = start; i < this->Count(); i++)
  {
    if (this->GetItem(i)->IsDownloaded() && (!this->GetItem(i)->IsProcessed()))
    {
      result = i;
      break;
    }
  }

  return result;
}

wchar_t *CSegmentFragmentCollection::GetSegmentFragmentUrl(CSegmentFragment *segmentFragment)
{
  return FormatString(L"%sSeg%d-Frag%d%s", this->defaultBaseUrl, segmentFragment->GetSegment(), segmentFragment->GetFragment(), (this->segmentFragmentUrlExtraParameters == NULL) ? L"" : this->segmentFragmentUrlExtraParameters);
}

const wchar_t *CSegmentFragmentCollection::GetBaseUrl(void)
{
  return this->defaultBaseUrl;
}

bool CSegmentFragmentCollection::SetBaseUrl(const wchar_t *baseUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->defaultBaseUrl, baseUrl);
}

const wchar_t *CSegmentFragmentCollection::GetSegmentFragmentUrlExtraParameters(void)
{
  return this->segmentFragmentUrlExtraParameters;
}

bool CSegmentFragmentCollection::SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters)
{
  SET_STRING_RETURN_WITH_NULL(this->segmentFragmentUrlExtraParameters, segmentFragmentUrlExtraParameters);
}