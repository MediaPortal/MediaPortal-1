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

// This code is based on Arto Järvinen's work - http://www.ostrogothia.com/video/

#pragma once

#include "stdafx.h"
#include "MpAudioRenderer.h"
#include "SyncClock.h"
#include "TimeSource.h"

#include "alloctracing.h"

extern void Log(const char* fmt, ...);

CSyncClock::CSyncClock(LPUNKNOWN pUnk, HRESULT* phr, CMPAudioRenderer* pRenderer, bool pUseHWRefClock)
  : CBaseReferenceClock(NAME("SyncClock"), pUnk, phr),
  m_pCurrentRefClock(0),
  m_pPrevRefClock(0),
  m_dAdjustment(1.0),
  m_dBias(1.0),
  m_dEVRDelay(0.0),
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
  m_dSuggestedAudioMultiplier(1.0),
  m_ullPrevSystemTime(0),
  m_ullPrivateTime(0),
  m_ullHWPrivateTime(0),
  m_bDiscontinuity(false)
{
}

void CSyncClock::SetEVRDelay(double pDelay)
{
  m_SynchCorrection.SetPresenterInducedAudioDelay(pDelay);
  m_dEVRDelay = pDelay;
}

void CSyncClock::SetBias(double pBias)
{
  m_SynchCorrection.SetBias(pBias);
  m_dBias = pBias;
}

void CSyncClock::SetAdjustment(double pAdjustment)
{
  m_SynchCorrection.SetAdjustment(pAdjustment);
  m_dAdjustment = pAdjustment;
}

void CSyncClock::SetAudioDelay(INT64 pAudioDelay)
{
  m_SynchCorrection.SetAudioDelay(pAudioDelay);
}

double CSyncClock::Bias()
{
  return m_dBias;
}

double CSyncClock::Adjustment()
{
  return m_dAdjustment;
}

HRESULT CSyncClock::Reset()
{
  CAutoLock cObjectLock(this);
  m_SynchCorrection.Reset(m_dBias);
  m_SynchCorrection.SetPresenterInducedAudioDelay(m_dEVRDelay);
  m_bDiscontinuity = true;
  return S_OK;
}

void CSyncClock::GetClockData(CLOCKDATA *pClockData)
{
  // pClockData pointer is validated already in CMPAudioRenderer
  pClockData->driftMultiplier = m_SynchCorrection.GetAVMult();
  pClockData->driftHWvsSystem = (m_llDurationHW - m_llDurationSystem) / 10000.0;
  pClockData->currentDrift = m_SynchCorrection.GetCurrentDrift() / 10000.0;
  pClockData->resamplingAdjustment = m_dSuggestedAudioMultiplier;
}

void CSyncClock::AudioResampled(double sourceLength, double resampleLength, double bias, double adjustment, double driftMultiplier)
{
  CAutoLock cObjectLock(this);
  m_SynchCorrection.AudioResampled(sourceLength, resampleLength, bias, adjustment, driftMultiplier); 
}

char* CSyncClock::DebugData()
{
  return m_SynchCorrection.DebugData(); 
}

double CSyncClock::SuggestedAudioMultiplier(UINT64 sampleLength, double bias, double adjustment)
{
  CAutoLock cObjectLock(this);
  
  // store for EVR stats renderer
  m_dSuggestedAudioMultiplier = m_SynchCorrection.SuggestedAudioMultiplier(sampleLength, bias, adjustment);
  return m_dSuggestedAudioMultiplier;
}

double CSyncClock::GetBias()
{
  CAutoLock cObjectLock(this);
  return m_SynchCorrection.GetBias();
}

REFERENCE_TIME CSyncClock::GetPrivateTime()
{
  CAutoLock cObjectLock(this);

  UINT64 qpcNow = GetCurrentTimestamp();

  UINT64 hwClock(0);
  UINT64 hwQpc(0);

  HRESULT hr = m_pAudioRenderer->AudioClock(hwClock, hwQpc);

  if (hr == S_OK)
  {
    if (m_ullStartQpcHW == 0)
    {
      m_ullStartQpcHW = hwQpc;
      m_ullStartTimeSystem = qpcNow;
      m_ullPrevSystemTime = qpcNow;
    }

    if (m_ullStartTimeHW == 0)
      m_ullStartTimeHW = hwClock;

    m_llDurationHW = (hwClock - m_ullStartTimeHW);
    m_llDurationSystem = (qpcNow - m_ullStartTimeSystem); 

    if (m_bDiscontinuity)
    {
      m_ullStartTimeHW = m_ullPrevTimeHW = hwClock;
      m_ullStartQpcHW = m_ullPrevQpcHW = hwQpc;
      m_ullStartTimeSystem = qpcNow;
      m_bDiscontinuity = false;
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

      if (m_bHWBasedRefClock)
        m_SynchCorrection.SetAVMult(m_dSystemClockMultiplier);

      m_ullPrevTimeHW = hwClock;
      m_ullPrevQpcHW = hwQpc;
    }
  }
  else
  {
    //Log("AudioClock() returned error (0x%08x)");
    if (m_ullStartTimeSystem == 0)
      m_ullStartTimeSystem = qpcNow;

    if (m_ullPrevSystemTime == 0)
      m_ullPrevSystemTime = qpcNow;
  }

  INT64 delta = qpcNow - m_ullPrevSystemTime;

  if (qpcNow < m_ullPrevSystemTime)
    delta += REFERENCE_TIME(ULLONG_MAX) + 1;

  m_ullPrevSystemTime = qpcNow;

  INT64 synchCorrectedDelta = m_SynchCorrection.GetCorrectedTimeDelta(delta);

  //Log("diff %I64d delta: %I64d synchCorrectedDelta: %I64d", delta - synchCorrectedDelta, delta, synchCorrectedDelta);

  m_ullHWPrivateTime = m_ullHWPrivateTime + delta;
  m_ullPrivateTime = m_ullPrivateTime + synchCorrectedDelta;

  return m_ullPrivateTime;
}

HRESULT CSyncClock::GetHWTime(REFERENCE_TIME* rtTime, REFERENCE_TIME* rtHwTime)
{
  HRESULT hr = S_FALSE;
  REFERENCE_TIME* rtDsTime = 0;
  REFERENCE_TIME rtTmp = 0;

  if (rtTime)
    rtDsTime = rtTime;
  else
    rtDsTime = &rtTmp;

  CAutoLock cObjectLock(this);

  // Update the HW clock information
  hr = CBaseReferenceClock::GetTime(rtDsTime);
  if (FAILED(hr))
    return hr;

  // Log("rtTime: %6.3f m_ullRealPrivateTime: %6.3f", rtTime / 10000000.0, m_ullRealPrivateTime/ 10000000.0);

  *rtHwTime = m_ullHWPrivateTime;

  return S_OK;
}
