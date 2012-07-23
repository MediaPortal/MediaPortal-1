// Copyright (C) 2005-2010 Team MediaPortal
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

#include <streams.h>
#include <stdio.h>
#include <atlbase.h>
#include <string.h>
#include <atlconv.h>
#include <mmsystem.h>
#include <d3d9.h>
#include <mfidl.h>
#include <math.h>

#include "dshowhelper.h"
#include "evrcustompresenter.h"
#include "timesource.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

void CALLBACK TimerCallback(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
  SchedulerParams *p = (SchedulerParams*)dwUser;
  Log("Callback %d", uTimerID);
  TIME_LOCK(&p->csLock, 30000, "TimeCallback");
  if (p->bDone)
  {
    Log("The end is near");
  }
  p->eTimerEnd.Set();
}


UINT CALLBACK TimerThread(void* param)
{  
  SchedulerParams *p = (SchedulerParams*)param;
  DWORD dwUser = 0;
  TIMECAPS tc;
  DWORD dwResolution;
  HANDLE hAvrt;
  DWORD dwTaskIndex = 0;
  LONGLONG now = 0;
  LONGLONG diff = 0;
  LONGLONG timeout = 0;
  HANDLE hEvts[] = {p->eHasWork, p->eHasWorkLP};
  DWORD dwObject;

    // Tell Vista Multimedia Class Scheduler (MMCS) we are doing threaded playback
  if (m_pAvSetMmThreadCharacteristicsW) 
  {
    hAvrt = m_pAvSetMmThreadCharacteristicsW(L"Playback", &dwTaskIndex);
  }
  if (m_pAvSetMmThreadPriority) 
  {
    if (m_pAvSetMmThreadPriority(hAvrt, AVRT_PRIORITY_LOW))
    {
      Log("Timer set AvSetMmThreadPriority succeeded");
    }
  }

    // Set timer resolution (must be after MMCS setup, since timer res can be changed by MMCS)
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  while (!p->bDone)
  {    
    LOG_TRACE("Timer sleeping.");
    if (p->iPause <= 0)
    {
      dwObject = WaitForMultipleObjects (2, hEvts, FALSE, INFINITE);
    }

    if (p->iPause > 0)
    {
      p->bPauseAck = TRUE;
      while (p->iPause > 0)
      {
        Sleep(1);
      }
      p->bPauseAck = FALSE;
      dwObject = (WAIT_FAILED - 1); // Make sure we fall through the switch
      p->eHasWork.Reset();
      p->eHasWorkLP.Reset();
      p->eTimerEnd.Reset();
    }

    switch (dwObject)
    {
      case WAIT_OBJECT_0 :     //eHasWork
        p->eHasWork.Reset();
        break;
      case WAIT_OBJECT_0 + 1 : //eHasWorkLP
        p->eHasWorkLP.Reset();
        now = GetCurrentTimestamp();
        timeout = now + 1000000;
        while ( (now < p->llTime) && (now < timeout) && !p->bDone && (p->llTime > 0))
        {    
          now = GetCurrentTimestamp(); //poll until we reach the target time
          Sleep(2); //CPU load is too high without a Sleep()
        }
        if ((p->llTime > 0) && !p->bDone)
        {
          p->pPresenter->NotifySchedulerTimer(); //wake up scheduler thread
        }   
        
        if (LOG_DELAYS)
        {
          diff = GetCurrentTimestamp() - p->llTime;
          if ((diff > 100000) && (p->llTime > 0))
            Log("High latency in TimerThread: %.2f ms", (double)diff/10000);
        }
        
        break;
    }

    LOG_TRACE("Timer woken up");
  }
  
  // quit
  p->eHasWork.Reset();
  p->eHasWorkLP.Reset();
  p->eTimerEnd.Reset();
  timeEndPeriod(dwResolution);
  if (m_pAvRevertMmThreadCharacteristics) 
  {
    m_pAvRevertMmThreadCharacteristics(hAvrt);
  }
  Log("Timer done.");
  return 0;
}




UINT CALLBACK WorkerThread(void* param)
{
  SchedulerParams *p = (SchedulerParams*)param;
  HANDLE hAvrt;
  DWORD dwTaskIndex = 0;
  DWORD dwUser = 0;
  TIMECAPS tc;
  DWORD dwResolution;
  LONGLONG now = 0;
  LONGLONG diff = 0;
  HANDLE hEvts[] = {p->eHasWork, p->eHasWorkLP};
  DWORD dwObject;

    // Tell Vista Multimedia Class Scheduler (MMCS) we are doing threaded playback
  if (m_pAvSetMmThreadCharacteristicsW) 
  {
    hAvrt = m_pAvSetMmThreadCharacteristicsW(L"Playback", &dwTaskIndex);
  }
  if (m_pAvSetMmThreadPriority) 
  {
    if (m_pAvSetMmThreadPriority(hAvrt, AVRT_PRIORITY_NORMAL))
    {
      Log("Worker set AvSetMmThreadPriority succeeded");
    }
  }

    // Set timer resolution (must be after MMCS setup, since timer res can be changed by MMCS)
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  while (!p->bDone)
  {    
    LOG_TRACE("Worker sleeping.");
    if (p->iPause <= 0)
    {
//      dwObject = WaitForMultipleObjects (2, hEvts, FALSE, INFINITE);
      if(p->pPresenter->m_bScrubbing)
        dwObject = WaitForMultipleObjects (2, hEvts, FALSE, 5);
      else
        dwObject = WaitForMultipleObjects (2, hEvts, FALSE, 50);
    }

    if (p->iPause > 0)
    {
      p->bPauseAck = TRUE;
      while (p->iPause > 0)
      {
        Sleep(1);
      }
      p->bPauseAck = FALSE;
      dwObject = (WAIT_FAILED - 1); // Make sure we fall through the switch by default
      p->eHasWork.Reset();
      p->eHasWorkLP.Reset();
      p->eTimerEnd.Reset();
    }

    if (LOG_DELAYS)
      now = GetCurrentTimestamp();
    
    switch (dwObject)
    {
      case WAIT_OBJECT_0 :     //eHasWork
        p->eHasWork.Reset();
        p->pPresenter->CheckForInput(true);
        break;
      case WAIT_OBJECT_0 + 1 : //eHasWorkLP
        p->eHasWorkLP.Reset();
        p->pPresenter->CheckForInput(false);
        break;
      case WAIT_TIMEOUT :
        p->pPresenter->CheckForInput(false);
        break;
    }

    if (LOG_DELAYS)
    {
      diff = GetCurrentTimestamp()-now;
      if (diff > 1000000)
        Log("High CheckForInput() latency in WorkerThread: %.2f ms", (double)diff/10000);
    }
    
    LOG_TRACE("Worker woken up");
  }
  
  // quit
  p->eHasWork.Reset();
  p->eHasWorkLP.Reset();
  p->eTimerEnd.Reset();
  timeEndPeriod(dwResolution);
  if (m_pAvRevertMmThreadCharacteristics) 
  {
    m_pAvRevertMmThreadCharacteristics(hAvrt);
  }
  Log("Worker done.");
  return 0;
}


UINT CALLBACK SchedulerThread(void* param)
{
  SchedulerParams *p = (SchedulerParams*)param;
//  HANDLE hAvrt;
  DWORD dwTaskIndex = 0;
  LONGLONG hnsTargetTime = 0;
  MMRESULT lastTimerId = 0;
  BOOL idleWait = false;
  LONGLONG delay = 0;
  DWORD dwUser = 0;
  TIMECAPS tc;
  DWORD dwResolution;
  LONGLONG now = 0;
  LONGLONG diff = 0;
  LONGLONG delErr = 0;
  DWORD timDel = 0;
  DWORD dwObject;
  HANDLE hEvts2[] = {p->eHasWork, p->eTimerEnd};
  HANDLE hEvts3[] = {p->eHasWork, p->eTimerEnd, p->eHasWorkLP};


  // SetThreadAffinityMask(GetCurrentThread(), 1); //Force onto CPU 0 - ATi flickering GUI experiment
  
    // Tell Vista Multimedia Class Scheduler (MMCS) we are doing threaded playback (increase priority)
//  if (m_pAvSetMmThreadCharacteristicsW) 
//  {
//    hAvrt = m_pAvSetMmThreadCharacteristicsW(L"Playback", &dwTaskIndex);
//  }
//  if (m_pAvSetMmThreadPriority) 
//  {
//    if (m_pAvSetMmThreadPriority(hAvrt, AVRT_PRIORITY_HIGH))
//    {
//      Log("Scheduler set AvSetMmThreadPriority succeeded");
//    }
//  }
 
    // Set timer resolution (must be after MMCS setup, since timer res can be changed by MMCS)
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  while (!p->bDone)
  {   
    // delErr = 0;
    delay  = 0;
    dwObject = (WAIT_FAILED - 1); // Make sure we fall through the switch by default
    
    now = GetCurrentTimestamp();
    delay = hnsTargetTime - now; 
    if ((delay < 0) || (hnsTargetTime < 0))
    {
      delay = 0;
      hnsTargetTime = 0;
    }
    delay = min(1000000, delay); //limit max sleep time to 100ms

    if ( lastTimerId > 0 ) 
    {
      timeKillEvent(lastTimerId);
      lastTimerId = 0;
    }
    p->eTimerEnd.Reset();
    p->pPresenter->NotifyTimer(0); //Disable Timer thread
    
    if (p->iPause <= 0)
    {
      if (idleWait)
      {     
        delay = 100000;
        timDel = (DWORD)(delay/10000);
        LOG_TRACE("Setting Scheduler Timer to %d ms idle time", timDel);
        dwObject = WaitForMultipleObjects (3, hEvts3, FALSE, timDel );
      }
      else if (delay >= 10000) // set timer if hnsTargetTime is at least 1 ms in the future
      {     
        timDel = (DWORD)(delay/10000);
        LOG_TRACE("Setting Scheduler Timer to %d ms video delay", timDel);
  
        p->pPresenter->NotifyTimer(hnsTargetTime); //Wake up Timer thread
  
        dwObject = WaitForMultipleObjects (2, hEvts2, FALSE, timDel + 1);
      }
    }

    if (p->iPause > 0)
    {
      p->bPauseAck = TRUE;
      while (p->iPause > 0)
      {
        Sleep(1);
      }
      p->bPauseAck = FALSE;
      hnsTargetTime = 0;   
      delay = 0;
      dwObject = (WAIT_FAILED - 1); // Make sure we fall through the switch
      p->eHasWork.Reset();
      p->eHasWorkLP.Reset();
      p->eTimerEnd.Reset();
    }
      
    switch (dwObject)
    {
      case WAIT_OBJECT_0 :     //eHasWork
        p->eHasWork.Reset();
        break;
      case WAIT_OBJECT_0 + 1 : //eTimerEnd
        p->eTimerEnd.Reset();
        if (LOG_DELAYS)
        {
          delErr = GetCurrentTimestamp() - hnsTargetTime;
          if (idleWait) 
            delErr = 0;
          if (delErr > 50000 )
            Log("High timer latency in SchedulerThread: %.2f ms, target: %.2f ms", ((double)delErr)/10000.0, ((double)delay)/10000.0);
        }
        break;
      case WAIT_OBJECT_0 + 2 : //eHasWorkLP
        p->eHasWorkLP.Reset();
        break;
      case WAIT_TIMEOUT :
        if (LOG_DELAYS)
        {
          delErr = GetCurrentTimestamp() - hnsTargetTime;
          if (idleWait) 
            delErr = 0;
          if (delErr > 50000 )
            Log("High WFMO timeout latency in SchedulerThread: %.2f ms, target: %.2f ms", ((double)delErr)/10000.0, ((double)delay)/10000.0);
        }
        break;
    }

    idleWait = true;
    
    if (LOG_DELAYS)
      now = GetCurrentTimestamp();
      
    p->pPresenter->CheckForScheduledSample(&hnsTargetTime, delay, &idleWait);
    
    LOG_TRACE("Got scheduling time: %I64d", hnsTargetTime);
    
    if (LOG_DELAYS)
      diff = GetCurrentTimestamp()-now;
      
    LOG_TRACE("Scheduler Timer woken up");
    
    if (LOG_DELAYS && (diff > (500000 + delay)))
      Log("High CheckForScheduledSample() latency in SchedulerThread: %.2f ms", ((double)diff)/10000.0);
    
  }
  
  // quit
  if ( lastTimerId > 0 ) 
  {
    timeKillEvent(lastTimerId);
    lastTimerId = 0;
  }
  p->eHasWork.Reset();
  p->eHasWorkLP.Reset();
  p->eTimerEnd.Reset();
  
  timeEndPeriod(dwResolution);
//  if (m_pAvRevertMmThreadCharacteristics) 
//  {
//    m_pAvRevertMmThreadCharacteristics(hAvrt);
//  }
  Log("Scheduler done.");
  return 0;
}

