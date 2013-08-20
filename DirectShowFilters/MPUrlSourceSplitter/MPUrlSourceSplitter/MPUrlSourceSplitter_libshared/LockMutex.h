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

#pragma once

#ifndef __LOCK_MUTEX_DEFINED
#define __LOCK_MUTEX_DEFINED

class CLockMutex
{
public:
  CLockMutex(HANDLE lockMutex, DWORD milliseconds);
  ~CLockMutex(void);

  // tests if result of locking is abandoned mutex
  // @return : true if abandoned, false otherwise
  bool IsAbandoned(void);

  // tests if mutex is locked
  // @return : true if locked, false otherwise
  bool IsLocked(void);

  // tests if timeout occured
  // @return : true if timeout occured, false otherwise
  bool IsTimeout(void);

  // tests if locking operation failed
  // @return : true if operation failed, false otherwise
  bool IsFailed(void);

  // gets error code if locking operation failed
  // @return : error code or NOERROR
  DWORD GetErrorCode(void);

  // locks mutex
  // @param milliseconds : timeout for waiting
  // @return : true if mutex locked, false otherwise
  bool Lock(DWORD milliseconds);

  // unlocks locked mutex
  void Unlock(void);

private:
  // store lock mutex here for releasing purpose
  HANDLE lockMutex;

  // result of lock operation
  DWORD result;
  // error code if failed
  DWORD error;
};

#endif