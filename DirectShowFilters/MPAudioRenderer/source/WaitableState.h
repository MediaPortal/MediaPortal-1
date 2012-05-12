// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "stdafx.h"

#pragma once

// A class that represents a state variable.
// Any thread can change the state while
// many threads can concurently wait for the
// object to enter a requested state
class CWaitableState
{
public:
  CWaitableState(void);
  CWaitableState(DWORD dwNewState);
  virtual ~CWaitableState(void);

  virtual bool IsInState(DWORD dwState)
  { return dwState == m_dwState; };

  DWORD SetState(DWORD dwNewState);
  DWORD WaitForState(DWORD dwState, DWORD dwMilliseconds);

protected:
  void InitializeEvents();

  CCritSec m_StateChanging;

  DWORD m_dwState;
  HANDLE m_hEventCurrent;
  HANDLE m_hEventPrevious;
};

// Same as a CWaitableState, but states are bitmapped
// Each bit in the DWORD represents a state and it is
// possible to wait for the object to enter any one of 
// a number of states
class CWaitableBitmappedState : public CWaitableState
{
public:
  virtual bool IsInState(DWORD dwState)
  { return (dwState & m_dwState) != 0; };
};
