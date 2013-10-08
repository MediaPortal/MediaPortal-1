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

#include "RtspStreamFragmentCollection.h"

CRtspStreamFragmentCollection::CRtspStreamFragmentCollection(void)
  : CCollection()
{
}

CRtspStreamFragmentCollection::~CRtspStreamFragmentCollection(void)
{
}

CRtspStreamFragment *CRtspStreamFragmentCollection::Clone(CRtspStreamFragment *item)
{
  return item->Clone();
}

unsigned int CRtspStreamFragmentCollection::GetFirstNotDownloadedStreamFragment(unsigned int start)
{
  unsigned int result = UINT_MAX;

  for (unsigned int i = start; i < this->itemCount; i++)
  {
    if (!this->GetItem(i)->IsDownloaded())
    {
      result = i;
      break;
    }
  }

  return result;
}