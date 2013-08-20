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

#include "LockMutex.h"

CLockMutex::CLockMutex(HANDLE lockMutex, DWORD milliseconds)
{
  this->lockMutex = lockMutex;
  this->error = NOERROR;
  this->result = NOERROR;

  this->Lock(milliseconds);
}

CLockMutex::~CLockMutex(void)
{
  this->Unlock();
}

bool CLockMutex::IsAbandoned(void)
{
  return (this->result == WAIT_ABANDONED);
}

bool CLockMutex::IsLocked(void)
{
  return (this->result == WAIT_OBJECT_0);
}

bool CLockMutex::IsTimeout(void)
{
  return (this->result == WAIT_TIMEOUT);
}

bool CLockMutex::IsFailed(void)
{
  return (this->result == WAIT_FAILED);
}

DWORD CLockMutex::GetErrorCode(void)
{
  return this->error;
}

bool CLockMutex::Lock(DWORD milliseconds)
{
  if (this->lockMutex != NULL)
  {
    this->result = WaitForSingleObject(this->lockMutex, milliseconds);
    if (this->IsFailed())
    {
      this->error = GetLastError();
    }
  }

  return (this->IsLocked());
}

void CLockMutex::Unlock(void)
{
  if (this->IsLocked())
  {
    ReleaseMutex(this->lockMutex);
    this->result = NOERROR;
  }
}