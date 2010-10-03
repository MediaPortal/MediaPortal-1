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
  m_dDurationCorrected(0),
  m_llDeltaError(0.0),
  m_llOverallCorrection(0),
  m_dwPrevSystemTime(0),
  m_rtPrivateTime(0)
{
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
  pClockData->driftHWvsSystem = (m_dDurationHW - m_dDurationSystem) / 10000;
  pClockData->driftHWvsCorrected = m_llOverallCorrection / 10000;
}

REFERENCE_TIME CSyncClock::GetPrivateTime()
{
  CAutoLock cObjectLock(this);

  UINT64 qpcNow = GetCurrentTimestamp();

  UINT64 hwClock(0);
  UINT64 hwQpc(0);

  UINT64 hwClockEnd(0);
  UINT64 hwQpcEnd(0);

  HRESULT hr = m_pAudioRenderer->AudioClock(hwClock, hwQpc);

  if (m_dStartTimeSystem == 0)
    m_dStartTimeSystem = qpcNow;

  if (m_dwPrevSystemTime == 0)
    m_dwPrevSystemTime = qpcNow;

  if (hwClock > 10000000 && hr == S_OK)
  {
    if (m_dStartQpcHW == 0)
      m_dStartQpcHW = hwQpc;

    if (m_dStartTimeHW == 0)
      m_dStartTimeHW = hwClock;

    if (m_dStartTimeCorrected == 0)
      m_dStartTimeCorrected = m_rtPrivateTime;

    m_dDurationHW = (hwClock - m_dStartTimeHW);
    m_dDurationSystem = (qpcNow - m_dStartTimeSystem); 
    m_dDurationCorrected = (m_rtPrivateTime - m_dStartTimeCorrected);

    if (m_dPrevTimeHW > hwClock)
    {
      m_dStartTimeHW = m_dPrevTimeHW = hwClock;
      m_dStartQpcHW = m_dPrevQpcHW = hwQpc;
      m_dStartTimeSystem = qpcNow;
      m_dStartTimeCorrected = m_rtPrivateTime;
      m_llDeltaError = 0;
      m_llOverallCorrection = 0;
    }
    else
    {
      double clockDiff = hwClock - m_dStartTimeHW;
      double qpcDiff = hwQpc - m_dStartQpcHW;

      double prevMultiplier = m_dSystemClockMultiplier;

      if (clockDiff > 0 && qpcDiff > 0)
        m_dSystemClockMultiplier = clockDiff / qpcDiff;

      if (m_dSystemClockMultiplier < 0.95 || m_dSystemClockMultiplier > 1.05)
        m_dSystemClockMultiplier = prevMultiplier;

      m_dPrevTimeHW = hwClock;
      m_dPrevQpcHW = hwQpc;
    }
  }
  else if (hr != S_OK)
  {
    Log("AudioClock() returned error (0x%08x)");
  }
  
  INT64 delta = qpcNow - m_dwPrevSystemTime;
  INT64 deltaOrig = delta;

  if (qpcNow < m_dwPrevSystemTime)
  {
    delta += REFERENCE_TIME(ULLONG_MAX) + 1;
  }

  m_dwPrevSystemTime = qpcNow;
  delta = (REFERENCE_TIME)(delta * (UNITS / MILLISECONDS));
  double dAdjustment;
  if (m_bHWBasedRefClock)
  {
    dAdjustment = m_dAdjustment * m_dBias * m_dSystemClockMultiplier;
  }
  else
  {
    dAdjustment = m_dAdjustment * m_dBias;
  }
  double ddelta= ((double) delta) * dAdjustment;
  delta = (REFERENCE_TIME) ddelta;
  m_llDeltaError += ddelta - delta;
  
  const double deltaErrorLimit = 1.0;
  
  if (m_llDeltaError > deltaErrorLimit)
  {
    delta += deltaErrorLimit * 10000;
    m_llDeltaError -= deltaErrorLimit;
    m_llOverallCorrection += deltaErrorLimit;
  }
  else if (m_llDeltaError < -deltaErrorLimit)
  {
    delta -= deltaErrorLimit * 10000;
    m_llDeltaError += deltaErrorLimit;
    m_llOverallCorrection -= deltaErrorLimit;
  }
  
  //if (hwQpc - m_dStartQpcHW > 600000000)
  {
    //Log("mul: %.10f de: %8I64d de.orig: %8I64d de.err: %.10f bias: %.10f adj: %.10f hwQpc: %I64d hwClock: %I64d", 
      //m_dSystemClockMultiplier, delta / 10000, deltaOrig, m_ddeltaError, m_dBias, m_dAdjustment, hwQpc, hwClock);
  }

  m_rtPrivateTime = m_rtPrivateTime + delta / 10000;
  return m_rtPrivateTime;
}
