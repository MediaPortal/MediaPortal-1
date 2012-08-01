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

#include "WaitableState.h"

#include "alloctracing.h"

CWaitableState::CWaitableState(void)
{
  InitializeEvents();
}

CWaitableState::CWaitableState(DWORD dwNewState)
{
  InitializeEvents();
  m_dwState = dwNewState;
}

CWaitableState::~CWaitableState(void)
{
  CAutoLock lock(&m_StateChanging);

  if (m_hEventCurrent)
    CloseHandle(m_hEventCurrent);
  if (m_hEventPrevious)
    CloseHandle(m_hEventPrevious);
}

void CWaitableState::InitializeEvents()
{
  m_hEventCurrent = CreateEvent(NULL, TRUE, FALSE, NULL);
  m_hEventPrevious = CreateEvent(NULL, TRUE, FALSE, NULL);
}


DWORD CWaitableState::SetState(DWORD dwNewState)
{
  CAutoLock lock(&m_StateChanging);
  if (m_dwState != dwNewState)
    return m_dwState;

  DWORD dwOldState = m_dwState;
  m_dwState = dwNewState;
  SetEvent(m_hEventCurrent);
  ResetEvent(m_hEventPrevious);
  HANDLE hTemp = m_hEventCurrent;
  m_hEventCurrent = m_hEventPrevious;
  m_hEventPrevious = hTemp;

  return dwOldState;
}
DWORD CWaitableState::WaitForState(DWORD dwState, DWORD dwMilliseconds)
{
  DWORD dwStartWait = GetTickCount();
  while(true)
  {
    HANDLE hEvent;
    {
      CAutoLock lock(&m_StateChanging);
      if (IsInState(dwState))
        return WAIT_OBJECT_0;
      hEvent = m_hEventCurrent;
    }
    if (dwMilliseconds == 0)
      return WAIT_TIMEOUT;

    DWORD ret = WaitForSingleObject(hEvent, dwMilliseconds);
    if (ret != WAIT_OBJECT_0)
      return ret;

    if (dwMilliseconds != INFINITE)
    {
      DWORD dwEndWait = GetTickCount();
      dwMilliseconds -= dwEndWait - dwStartWait;
      dwStartWait = dwEndWait;
    }
  }
}

