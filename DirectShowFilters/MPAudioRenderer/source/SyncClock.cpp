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
  m_ullStartQpcHW(0),
  m_ullStartTimeHW(0),
  m_ullStartTimeSystem(0),
  m_ullPrevTimeHW(0),
  m_ullPrevQpcHW(0),
  m_dSystemClockMultiplier(1.0),
  m_bHWBasedRefClock(pUseHWRefClock),
  m_llDurationHW(0),
  m_llDurationSystem(0),
  m_dDeltaError(0.0),
  m_lOverallCorrection(0),
  m_ullPrevSystemTime(0),
  m_ullPrivateTime(0),
  m_dAdjustmentDrift(0.0)
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
  // pClockData pointer is validated already in CMPAudioRenderer
  pClockData->driftMultiplier = m_dSystemClockMultiplier;
  pClockData->driftHWvsSystem = (m_llDurationHW - m_llDurationSystem) / 10000.0;
  pClockData->driftAdjustment = m_dAdjustmentDrift / 10000.0;
  pClockData->driftHWvsCorrected = m_lOverallCorrection / 10000;
}

void CSyncClock::ProvideAdjustmentDrift(double drift)
{
  CAutoLock cObjectLock(this);
  m_dAdjustmentDrift += drift;
}

double CSyncClock::AdjustmentDrift()
{
  CAutoLock cObjectLock(this);
  return m_dAdjustmentDrift;
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

  if (m_ullStartTimeSystem == 0)
    m_ullStartTimeSystem = qpcNow;

  if (m_ullPrevSystemTime == 0)
    m_ullPrevSystemTime = qpcNow;

  if (hr == S_OK)
  {
    if (m_ullStartQpcHW == 0)
      m_ullStartQpcHW = hwQpc;

    if (m_ullStartTimeHW == 0)
      m_ullStartTimeHW = hwClock;

    m_llDurationHW = (hwClock - m_ullStartTimeHW);
    m_llDurationSystem = (qpcNow - m_ullStartTimeSystem); 

    if (m_ullPrevTimeHW > hwClock)
    {
      m_ullStartTimeHW = m_ullPrevTimeHW = hwClock;
      m_ullStartQpcHW = m_ullPrevQpcHW = hwQpc;
      m_ullStartTimeSystem = qpcNow;
      m_dDeltaError = 0;
      m_lOverallCorrection = 0;
    }
    else
    {
      double clockDiff = hwClock - m_ullStartTimeHW;
      double qpcDiff = hwQpc - m_ullStartQpcHW;

      double prevMultiplier = m_dSystemClockMultiplier;

      if (clockDiff > 0 && qpcDiff > 0)
        m_dSystemClockMultiplier = clockDiff / qpcDiff;

      if (m_dSystemClockMultiplier < 0.95 || m_dSystemClockMultiplier > 1.05)
        m_dSystemClockMultiplier = prevMultiplier;

      m_ullPrevTimeHW = hwClock;
      m_ullPrevQpcHW = hwQpc;
    }
  }
  else if (hr != S_OK)
  {
    //Log("AudioClock() returned error (0x%08x)");
  }
  
  INT64 delta = qpcNow - m_ullPrevSystemTime;
  INT64 deltaOrig = delta;

  if (qpcNow < m_ullPrevSystemTime)
  {
    delta += REFERENCE_TIME(ULLONG_MAX) + 1;
  }

  m_ullPrevSystemTime = qpcNow;
  double dAdjustment = 1.0;

  if (m_bHWBasedRefClock)
  {
    dAdjustment = m_dAdjustment * m_dBias * m_dSystemClockMultiplier;
  }
  else
  {
    dAdjustment = m_dAdjustment * m_dBias;
  }

  if (m_dAdjustment != 1.0)
  {
    m_dAdjustmentDrift -= (m_dAdjustment * delta -  delta);  
  }

  double ddelta= ((double) delta) * dAdjustment;
  delta = (REFERENCE_TIME) ddelta;
  m_dDeltaError += ddelta - delta;
  
  const double deltaErrorLimit = 10000.0;

  if (m_dDeltaError > deltaErrorLimit)
  {
    delta += deltaErrorLimit;
    m_dDeltaError -= deltaErrorLimit;
    m_lOverallCorrection += deltaErrorLimit;
  }
  else if (m_dDeltaError < -deltaErrorLimit)
  {
    delta -= deltaErrorLimit;
    m_dDeltaError += deltaErrorLimit;
    m_lOverallCorrection -= deltaErrorLimit;
  }
  
  //if (hwQpc - m_ullStartQpcHW > 600000000)
  {
    //Log("mul: %.10f de: %8I64d de.orig: %8I64d de.err: %.10f bias: %.10f adj: %.10f hwQpc: %I64d hwClock: %I64d", 
      //m_dSystemClockMultiplier, delta, deltaOrig, m_dDeltaError, m_dBias, m_dAdjustment, hwQpc, hwClock);
  }

  m_ullPrivateTime = m_ullPrivateTime + delta;
  return m_ullPrivateTime;
}
