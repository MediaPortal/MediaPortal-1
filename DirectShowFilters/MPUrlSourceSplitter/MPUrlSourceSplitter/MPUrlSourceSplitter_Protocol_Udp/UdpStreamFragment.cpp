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

#include "UdpStreamFragment.h"

CUdpStreamFragment::CUdpStreamFragment(HRESULT *result)
  : CCacheFileItem(result)
{
  this->start = 0;
}

CUdpStreamFragment::~CUdpStreamFragment(void)
{
}

/* get methods */

int64_t CUdpStreamFragment::GetStart(void)
{
  return this->start;
}

/* set methods */

void CUdpStreamFragment::SetStart(int64_t position)
{
  this->start = position;
}

/* other methods */

/* protected methods */

CFastSearchItem *CUdpStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CUdpStreamFragment *item = new CUdpStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CUdpStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CUdpStreamFragment *fragment = dynamic_cast<CUdpStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->start = this->start;
    }
  }

  return result;
}