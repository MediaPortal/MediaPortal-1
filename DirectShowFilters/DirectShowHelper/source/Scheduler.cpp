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
  DWORD dwObject;
  DWORD dwObject2;
  HANDLE hWTEvent = NULL;
  LONGLONG delay = 0;
  
  LARGE_INTEGER liDueTime; 
  liDueTime.QuadPart = -1LL;

    // Tell Vista Multimedia Class Scheduler (MMCS) we are doing threaded playback
  if (m_pAvSetMmThreadCharacteristicsW) 
  {
    hAvrt = m_pAvSetMmThreadCharacteristicsW(L"Playback", &dwTaskIndex);
  }
  if (m_pAvSetMmThreadPriority) 
  {
    if (m_pAvSetMmThreadPriority(hAvrt, (AVRT_PRIORITY)(p->pPresenter->m_regTimerMmcssPriority - 1)))
    {
      Log("Timer AvSetMmThreadPriority = %d", (p->pPresenter->m_regTimerMmcssPriority - 1));
    }
  }

    // Set timer resolution (must be after MMCS setup, since timer res can be changed by MMCS)
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  hWTEvent = CreateWaitableTimer(NULL, FALSE, NULL); //Synchronisation timer mode

  HANDLE hEvts[] = {p->eDoHPtask, p->eHasWork, p->eHasWorkLP, hWTEvent};
  HANDLE hEvts2[] = {p->eDoHPtask, hWTEvent};

  if (hWTEvent == NULL)
  {
    Log("Timer thread - Error!! Timer handle is NULL");
    //We can't run correctly, so go to sleep until we get closed down
    while (!p->bDone)
    {
      WaitForSingleObject(p->eHasWork, 100);     
    }
  }

  while (!p->bDone)
  {    
    LOG_TRACE("Timer sleeping.");

    dwObject = WaitForMultipleObjects (4, hEvts, FALSE, 150);

    switch (dwObject)
    {
      case WAIT_OBJECT_0 :     //eDoHPtask
        p->eDoHPtask.Reset();
        p->pPresenter->DwmInit(); //Initialise DWM
        break;
      case WAIT_OBJECT_0 + 1 :     //eHasWork
        p->eHasWork.Reset();
        break;
      case WAIT_OBJECT_0 + 2 : //eHasWorkLP
        p->eHasWorkLP.Reset();
        if (p->llTime > 0)
        {
          now = GetCurrentTimestamp();
          delay = p->llTime - now; 
          if (delay >= 1000) // >= 100us
          {        
            liDueTime.QuadPart = -delay;
      		  SetWaitableTimer(hWTEvent, &liDueTime, 0, NULL, NULL, 0);
            dwObject2 = WaitForMultipleObjects (2, hEvts2, FALSE, 110);
            switch (dwObject2)
            {
              case WAIT_OBJECT_0 :     //eDoHPtask
                p->eDoHPtask.Reset();
                p->pPresenter->DwmInit(); //Initialise DWM
                break;
              case WAIT_OBJECT_0 + 1 :     //hWTEvent
                break;
              case WAIT_TIMEOUT :
                break;
            }
            
            if (!p->bDone)
            {
              CAutoLock sLock(&p->csLock);
              p->pPresenter->NotifySchedulerTimer(); //wake up scheduler thread
            }
                         
            if (LOG_DELAYS)
            {
              diff = GetCurrentTimestamp() - p->llTime;
              if ((diff > 100000) && (p->llTime > 0))
                Log("High latency in TimerThread: %.2f ms", (double)diff/10000);
            }                 
          }
          else if (!p->bDone)
          {
            CAutoLock sLock(&p->csLock);
            p->pPresenter->NotifySchedulerTimer(); //wake up scheduler thread
          }
        }
        CancelWaitableTimer(hWTEvent);
        break;
      case WAIT_OBJECT_0 + 3 :  //hWTEvent - do nothing (just to discard spurious events)
        CancelWaitableTimer(hWTEvent);
        break;
      case WAIT_TIMEOUT :
        break;
    }
      
    LOG_TRACE("Timer woken up");
  }
  
  // quit
  if (hWTEvent)
  {
    CancelWaitableTimer(hWTEvent);
    CloseHandle(hWTEvent);
  }
  p->eHasWork.Reset();
  p->eHasWorkLP.Reset();
  p->eTimerEnd.Reset();
  p->eUnstall.Reset();
  p->eDoHPtask.Reset();  
  p->eStall.Reset();  
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
  HANDLE hEvts[] = {p->eStall, p->eHasWork, p->eHasWorkLP};
  DWORD dwObject;

    // Tell Multimedia Class Scheduler (MMCS) we are doing threaded playback
  if (m_pAvSetMmThreadCharacteristicsW) 
  {
    hAvrt = m_pAvSetMmThreadCharacteristicsW(L"Playback", &dwTaskIndex);
  }
  if (m_pAvSetMmThreadPriority) 
  {
    if (m_pAvSetMmThreadPriority(hAvrt, (AVRT_PRIORITY)(p->pPresenter->m_regWorkerMmcssPriority - 1)))
    {
      Log("Worker AvSetMmThreadPriority = %d", (p->pPresenter->m_regWorkerMmcssPriority - 1));
    }
  }

    // Set timer resolution (must be after MMCS setup, since timer res can be changed by MMCS)
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  while (!p->bDone)
  {    
    LOG_TRACE("Worker sleeping.");

    if(p->pPresenter->m_bScrubbing)
      dwObject = WaitForMultipleObjects (3, hEvts, FALSE, 5);
    else if(p->pPresenter->CheckQueueCount() <= 1)
      dwObject = WaitForMultipleObjects (3, hEvts, FALSE, 10);
    else
      dwObject = WaitForMultipleObjects (3, hEvts, FALSE, 50);

    if (p->pPresenter->IsRunning())
    {	//Context for CAutoLock			
      CAutoLock sLock(&p->csLock);
      
      if (LOG_DELAYS)
        now = GetCurrentTimestamp();
    
      switch (dwObject)
      {
        case WAIT_OBJECT_0 :     //eStall
          //Log("Worker - StallEvent 1a");
          p->eStall.Reset();
          p->pPresenter->m_WorkerStalledEvent.Set();
          if (!p->eUnstall.Wait(1000))
          {
            Log("Worker thread - Unstall Event timeout");
          }
          p->eUnstall.Reset();
          //Log("Worker - StallEvent 1b");
          break;
        case WAIT_OBJECT_0 + 1:     //eHasWork
          p->eHasWork.Reset();
          p->pPresenter->CheckForInput(true);
          break;
        case WAIT_OBJECT_0 + 2 : //eHasWorkLP
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
    else
    {
      switch (dwObject)
      {
        case WAIT_OBJECT_0 :     //eStall
          //Log("Worker - StallEvent 2a");
          p->eStall.Reset();
          p->pPresenter->m_WorkerStalledEvent.Set();
          if (!p->eUnstall.Wait(1000))
          {
            Log("Worker thread - Unstall Event timeout");
          }
          p->eUnstall.Reset();
          //Log("Worker - StallEvent 2b");
          break;
        case WAIT_OBJECT_0 + 1 :     //eHasWork
          p->eHasWork.Reset();
          break;
        case WAIT_OBJECT_0 + 2 : //eHasWorkLP
          p->eHasWorkLP.Reset();
          break;
        case WAIT_TIMEOUT :
          break;
      }      
    }

  }
  
  // quit
  p->eHasWork.Reset();
  p->eHasWorkLP.Reset();
  p->eTimerEnd.Reset();
  p->eUnstall.Reset();
  p->eDoHPtask.Reset();  
  p->eStall.Reset();  
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
  HANDLE hAvrt;
  DWORD dwTaskIndex = 0;
  LONGLONG hnsTargetTime = 0;
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

  
  if (p->pPresenter->m_bSchedulerEnableMMCSS)
  {
    // Tell Multimedia Class Scheduler (MMCS) we are doing threaded playback (increase priority)
    if (m_pAvSetMmThreadCharacteristicsW) 
    {
      hAvrt = m_pAvSetMmThreadCharacteristicsW(L"Playback", &dwTaskIndex);
    }
    if (m_pAvSetMmThreadPriority) 
    {
      if (m_pAvSetMmThreadPriority(hAvrt, (AVRT_PRIORITY)(p->pPresenter->m_regSchedMmcssPriority - 1)))
      {
        Log("Scheduler AvSetMmThreadPriority = %d", (p->pPresenter->m_regSchedMmcssPriority - 1));
      }
    }
  }
 
  // Set timer resolution (must be after MMCS setup, since timer res can be changed by MMCS)
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  HANDLE hEvts4[] = {p->eStall, p->eDoHPtask, p->eHasWork, p->eTimerEnd};
  HANDLE hEvts5[] = {p->eStall, p->eDoHPtask, p->eHasWork, p->eTimerEnd, p->eHasWorkLP};
  
  while (!p->bDone)
  {   
    delay  = 0;   
    dwObject = (WAIT_FAILED - 1); // Make sure we fall through the switch by default
    
    now = GetCurrentTimestamp();
    delay = hnsTargetTime - now; 
    if ((delay < 0) || (hnsTargetTime < 0))
    {
      delay = 0;
      hnsTargetTime = 0;
    }
    
    if (p->pPresenter->m_bLowResTiming)
    {
      if ((delay > 0) && (delay < MIN_VSC_DELAY))
      {
        //Force a minimum delay period to reduce CPU usage
        delay = MIN_VSC_DELAY;
        hnsTargetTime = delay + now;
      }
    }
         
    delay = min(1000000, delay); //limit max sleep time to 100ms

    p->eTimerEnd.Reset();
    p->pPresenter->NotifyTimer(0); //Disable Timer thread

    if (idleWait)
    {     
      delay = 100000;
      timDel = (DWORD)(delay/10000);
      LOG_TRACE("Setting Scheduler Timer to %d ms idle time", timDel);
      p->pPresenter->NotifyTimer(now + delay); //Wake up Timer thread
      dwObject = WaitForMultipleObjects (5, hEvts5, FALSE, timDel);
    }
    else if (delay >= 10000) // set timer if hnsTargetTime is at least 1 ms in the future
    {     
      timDel = (DWORD)(delay/10000);
      LOG_TRACE("Setting Scheduler Timer to %d ms video delay", timDel);
      p->pPresenter->NotifyTimer(hnsTargetTime); //Wake up Timer thread
      dwObject = WaitForMultipleObjects (4, hEvts4, FALSE, timDel);
    }
              
    switch (dwObject)
    {
      case WAIT_OBJECT_0 :     //eStall
        //Log("Sch - StallEvent start");
        p->eStall.Reset();
        p->pPresenter->m_SchedulerStalledEvent.Set();
        if (!p->eUnstall.Wait(1000))
        {
          Log("Scheduler thread - Unstall Event timeout");
        }
        p->eUnstall.Reset();
        //Log("Sch - StallEvent end");
        break;
      case WAIT_OBJECT_0 + 1 :     //eDoHPtask
        // Log("Sch - FlushEvent");
        p->eDoHPtask.Reset();
        p->pPresenter->DelegatedFlush();
        delay = 0;
        hnsTargetTime = 0;
        break;
      case WAIT_OBJECT_0 + 2 :     //eHasWork
        p->eHasWork.Reset();
        break;
      case WAIT_OBJECT_0 + 3 : //eTimerEnd
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
      case WAIT_OBJECT_0 + 4 : //eHasWorkLP
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

    
    if (p->pPresenter->IsRunning())
    { //Context for CAutoLock	
  	  CAutoLock sLock(&p->csLock);	  

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
    else
    {
      idleWait = true;
      delay = 0;
      hnsTargetTime = 0;
    }
    
  }
  
  // quit
  p->eHasWork.Reset();
  p->eHasWorkLP.Reset();
  p->eTimerEnd.Reset();
  p->eUnstall.Reset();
  p->eDoHPtask.Reset();  
  p->eStall.Reset();  
  timeEndPeriod(dwResolution);
  if (p->pPresenter->m_bSchedulerEnableMMCSS)
  {
    if (m_pAvRevertMmThreadCharacteristics) 
    {
      m_pAvRevertMmThreadCharacteristics(hAvrt);
    }
  }
  Log("Scheduler done.");
  return 0;
}

