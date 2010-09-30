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

// This code is based on Arto Järvinen's work - http://www.ostrogothia.com/video/

#pragma once

#include "stdafx.h"
#include "SyncClock.h"
#include "MpAudioRenderer.h"
#include "TimeSource.h"

#include "alloctracing.h"

extern void Log(const char *fmt, ...);

CSyncClock::CSyncClock(LPUNKNOWN pUnk, HRESULT *phr, CMPAudioRenderer* pRenderer, bool pUseHWRefClock)
  : CBaseReferenceClock(NAME("SyncClock"), pUnk, phr),
  m_pCurrentRefClock(0),
  m_pPrevRefClock(0),
  m_dAdjustment(1.0),
  m_dBias(1.0),
  m_pAudioRenderer(pRenderer),
  m_dStartQpcHW(0),
  m_dStartTimeHW(0),
  m_dStartTimeSystem(0),
  m_dStartTimeCorrected(0),
  m_dPrevTimeHW(0),
  m_dPrevQpcHW(0),
  m_dSystemClockMultiplier(1.0),
  m_bHWBasedRefClock(pUseHWRefClock),
  m_dDurationHW(0),
  m_dDurationSystem(0),
  m_dDurationCorrected(0)
{
  m_dwPrevSystemTime = timeGetTime();
  m_rtPrivateTime = (UNITS / MILLISECONDS) * m_dwPrevSystemTime;
}

void CSyncClock::SetDiff(DWORD diff)
{
   m_rtPrivateTime = m_rtPrivateTime + (UNITS / MILLISECONDS) * diff;
}

void CSyncClock::SetBias(double pBias)
{
   m_dBias = pBias;
}

void CSyncClock::SetAdjustment(double pAdjustment)
{
  m_dAdjustment = pAdjustment;
}

double CSyncClock::Bias()
{
  return m_dBias;
}

double CSyncClock::Adjustment()
{
  return m_dAdjustment;
}

void CSyncClock::GetClockData(CLOCKDATA *pClockData)
{
  // pointer is validated already in CMPAudioRenderer
  pClockData->driftMultiplier = m_dSystemClockMultiplier;
  pClockData->driftHWvsSystem = m_dDurationHW - m_dDurationSystem;
  pClockData->driftHWvsCorrected = m_dDurationHW - m_dDurationCorrected / m_dBias;
}

REFERENCE_TIME CSyncClock::GetPrivateTime()
{
  CAutoLock cObjectLock(this);

  LONGLONG qpcNow = GetCurrentTimestamp();

  DWORD dwTime = timeGetTime();

  UINT64 hwClock(0);
  UINT64 hwQpc(0);
  HRESULT hr = m_pAudioRenderer->AudioClock(hwClock, hwQpc);

  if (hr == S_OK)
  {
    if (m_dStartQpcHW == 0)
      m_dStartQpcHW = hwQpc;

    if (m_dStartTimeHW == 0)
      m_dStartTimeHW = hwClock;

    if (m_dStartTimeSystem == 0)
      m_dStartTimeSystem = dwTime;

    if (m_dStartTimeCorrected == 0)
      m_dStartTimeCorrected = m_rtPrivateTime;

    m_dDurationHW = (hwClock - m_dStartTimeHW) / 10000;
    m_dDurationSystem = (dwTime - m_dStartTimeSystem); 
    m_dDurationCorrected = (m_rtPrivateTime - m_dStartTimeCorrected) / 10000;

    //Log("hw: %I64d sys: %I64d cor: %I64d hwsy: %I64d hwcor: %I64d", m_dDurationHW, m_dDurationSystem, m_dDurationCorrected, m_dDurationHW - m_dDurationSystem, m_dDurationHW - m_dDurationCorrected);

    if (m_dPrevTimeHW > hwClock)
    {
      m_dStartTimeHW = m_dPrevTimeHW = hwClock;
      m_dStartQpcHW = m_dPrevQpcHW = hwQpc;
      m_dStartTimeSystem = dwTime;
      m_dStartTimeCorrected = m_rtPrivateTime;
    }
    else
    {
      double clockDiff = hwClock - m_dStartTimeHW;
      double qpcDiff = hwQpc - m_dStartQpcHW;

      if (clockDiff > 0 && qpcDiff > 0)
        m_dSystemClockMultiplier = clockDiff / qpcDiff;

      // TODO is this needed?
      if (m_dSystemClockMultiplier < 0.95 || m_dSystemClockMultiplier > 1.05)
        m_dSystemClockMultiplier = 1.0;

      m_dPrevTimeHW = hwClock;
      m_dPrevQpcHW = hwQpc;
    }
  }
  else
  {
    //Log("AudioClock() returned error (0x%08x)");
  }

  REFERENCE_TIME delta = REFERENCE_TIME(dwTime) - REFERENCE_TIME(m_dwPrevSystemTime);
  if(dwTime < m_dwPrevSystemTime)
  {
    delta += REFERENCE_TIME(UINT_MAX) + 1;
  }

  m_dwPrevSystemTime = dwTime;
  delta = (REFERENCE_TIME)(delta * (UNITS / MILLISECONDS) * m_dAdjustment * m_dBias);

  if (m_bHWBasedRefClock)
    delta /= m_dSystemClockMultiplier;

  Log("mul: %.10f delta: %I64d - hwClock: %I64d hwQpc: %I64d qpc: %I64d clock diff: %I64d qpc diff: %I64d", m_dSystemClockMultiplier, delta, hwClock, hwQpc, qpcNow, hwClock - m_dStartTimeHW, hwQpc - m_dStartQpcHW);

  m_rtPrivateTime = m_rtPrivateTime + delta;

  return m_rtPrivateTime;
}
