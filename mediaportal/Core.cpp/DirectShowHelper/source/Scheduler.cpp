/* 
 *      Copyright (C) 2005-2009 Team MediaPortal
 *      http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include <streams.h>
#include <stdio.h>
#include <atlbase.h>
#include <string.h>
#include <atlconv.h>
#include <mmsystem.h>
#include <d3d9.h>
#include <mfidl.h>

#include "dshowhelper.h"
#include "evrcustompresenter.h"
#include "timesource.h"

void CALLBACK TimerCallback(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
  SchedulerParams *p = (SchedulerParams*)dwUser;
  Log("Callback %d", uTimerID);
  TIME_LOCK(&p->csLock, 30000, "TimeCallback");
  if (p->bDone)
  {
    Log("The end is near");
  }
  p->eHasWork.Set();
}


UINT CALLBACK WorkerThread(void* param)
{
  timeBeginPeriod(1);
  SchedulerParams *p = (SchedulerParams*)param;
  while (true)
  {
    p->csLock.Lock();
    if (p->bDone)
    {
      Log("Worker done.");
      p->csLock.Unlock();
      //AvRevertMmThreadCharacteristics(hMmThread);
      return 0;
    }

    if (!p->pPresenter->CheckForInput()) {}
    p->csLock.Unlock();
    LOG_TRACE("Worker sleeping.");
    while (!p->eHasWork.Wait());
    LOG_TRACE("Worker woken up");
  }
  return -1;
}


UINT CALLBACK SchedulerThread(void* param)
{
  timeBeginPeriod(1);
  SchedulerParams *p = (SchedulerParams*)param;
  LONGLONG hnsSampleTime = 0;
  MMRESULT lastTimerId = 0;
  LONGLONG delay = 0;
  REFERENCE_TIME timePerFrame;
  //HANDLE hMmThread;
  //hMmThread = AvSetMmThreadCharacteristics("Playback", &dwTaskIndex);
  //AvSetMmThreadPriority(hMmThread, AVRT_PRIORITY_HIGH);
  DWORD dwUser = 0;
  TIMECAPS tc;
  DWORD dwResolution;
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 0), tc.wPeriodMax);
  dwUser = timeBeginPeriod(dwResolution);

  while (true)
  {
    if (lastTimerId > 0)
    {
      timeKillEvent(lastTimerId);
      lastTimerId = 0;
    }
    LONGLONG now = GetCurrentTimestamp();
    p->csLock.Lock();
    LOG_TRACE("Scheduler got lock");
    LONGLONG diff = GetCurrentTimestamp()-now;
    if (diff > 100000)
    {
      Log("High lock latency in SchedulerThread: %I64d ms", diff/10000);
    }
    if (p->bDone)
    {
      Log("Scheduler done.");
      if (lastTimerId > 0)
      {
        timeKillEvent(lastTimerId);
      }
      p->csLock.Unlock();
      //AvRevertMmThreadCharacteristics(hMmThread);
      return 0;
    }

    p->pPresenter->CheckForScheduledSample(&hnsSampleTime, delay);
    LOG_TRACE("Got scheduling time: %I64d", hnsSampleTime);
    timePerFrame = p->pPresenter->GetFrameDuration();
    if (hnsSampleTime > 0)
    {
      // wait for 3/4 sample time or 3/4 of frame duration depending on what is smaller
      delay = min((hnsSampleTime/100*75), (timePerFrame/100*75));
      // make sure thread wake up 5% of frame duration earlier than presentation time
      // delay = delay - (timePerFrame/20);
    }
    else 
    {
      // backup check to avoid starvation (and work around unknown bugs)
      delay = timePerFrame/100*75;
    }
    if (delay > 10000)
    {
      LOG_TRACE("Setting Timer");
      lastTimerId = timeSetEvent(DWORD(delay/10000), 
                                 0,
                                 (LPTIMECALLBACK)(HANDLE)p->eHasWork,
                                 0, 
                                 TIME_ONESHOT|TIME_KILL_SYNCHRONOUS|TIME_CALLBACK_EVENT_SET);
    }
    else 
    {
      p->eHasWork.Set();
    }
    p->csLock.Unlock();
    while (!p->eHasWork.Wait());
    LOG_TRACE("Scheduler woken up");
  }
  return -1;
}

