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

#include "PidCounterCollection.h"
#include "TsPacketConstants.h"

CPidCounterCollection::CPidCounterCollection(HRESULT *result)
  : CCollection(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_CONDITION_HRESULT(*result, __super::EnsureEnoughSpace(TS_PACKET_PID_COUNT), *result, E_OUTOFMEMORY);

    for (unsigned int i = 0; (SUCCEEDED(*result) && (i < TS_PACKET_PID_COUNT)); i++)
    {
      CPidCounter *pidCounter = new CPidCounter(result);
      CHECK_POINTER_HRESULT(*result, pidCounter, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, __super::Add(pidCounter), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(pidCounter));
    }
  }
}

CPidCounterCollection::~CPidCounterCollection(void)
{
}

/* get methods */

CPidCounter *CPidCounterCollection::GetItem(unsigned int index)
{
  return (CPidCounter *)__super::GetItem(index);
}

/* set methods */

/* other methods */

void CPidCounterCollection::Clear(void)
{
  for (unsigned int i = 0; i < TS_PACKET_PID_COUNT; i++)
  {
    CPidCounter *pidCounter = this->GetItem(i);
    
    pidCounter->Clear();
  }
}

/* protected methods */

CPidCounter *CPidCounterCollection::Clone(CPidCounter *item)
{
  return NULL;
}