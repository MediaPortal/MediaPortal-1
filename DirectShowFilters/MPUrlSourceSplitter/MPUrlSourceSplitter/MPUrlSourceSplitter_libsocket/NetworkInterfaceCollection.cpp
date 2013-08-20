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

#include "NetworkInterfaceCollection.h"

CNetworkInterfaceCollection::CNetworkInterfaceCollection(void)
  : CKeyedCollection()
{
}

CNetworkInterfaceCollection::~CNetworkInterfaceCollection(void)
{
}

int CNetworkInterfaceCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
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

const wchar_t *CNetworkInterfaceCollection::GetKey(CNetworkInterface *item)
{
  return item->GetFriendlyName();
}

CNetworkInterface *CNetworkInterfaceCollection::Clone(CNetworkInterface *item)
{
  return item->Clone();
}