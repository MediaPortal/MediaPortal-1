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

CSyncClock::CSyncClock(LPUNKNOWN pUnk, HRESULT* phr, CMPAudioRenderer* pRenderer, AudioRendererSettings* pSettings)
  : CBaseReferenceClock(NAME("SyncClock"), pUnk, phr),
  m_SynchCorrection(pSettings),
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
  m_llDurationHW(0),
  m_llDurationSystem(0),
  m_dSuggestedAudioMultiplier(1.0),
  m_ullPrevSystemTime(0),
  m_ullPrivateTime(0),
  m_ullHWPrivateTime(0),
  m_bDiscontinuity(false),
  m_clockSource(NOT_SET),
  m_pSettings(pSettings)
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

HRESULT CSyncClock::Reset(REFERENCE_TIME tStart)
{
  CAutoLock cObjectLock(this);
  m_SynchCorrection.Reset(m_dBias, tStart);
  m_SynchCorrection.SetPresenterInducedAudioDelay(m_dEVRDelay);
  m_bDiscontinuity = true;

  return S_OK;
}

void CSyncClock::GetClockData(CLOCKDATA* pClockData)
{
  // pClockData has been checked in the MpAudioRenderer class

  // We don't want 100% exact results - it won't matter if one video frame displays a bit wrong
  // debug details as no person can read those in any case with sich great detail

  INT64 llDurationSystem = m_llDurationSystem;
  INT64 llDurationHW = m_llDurationHW;

  pClockData->driftMultiplier = llDurationSystem > 0 ? (double)llDurationHW / (double)llDurationSystem : 0;
  pClockData->driftHWvsSystem = (double)(llDurationHW - llDurationSystem) / 10000.0;
  pClockData->currentDrift = m_dCurrentDrift;
  pClockData->resamplingAdjustment = m_dSuggestedAudioMultiplier;;
}

void CSyncClock::UpdateClockData(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime)
{
  m_dCurrentDrift = m_SynchCorrection.GetCurrentDrift(rtAHwTime, rtRCTime) / 10000.0;
}

double CSyncClock::SuggestedAudioMultiplier(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime, double bias, double adjustment)
{
  // Use a local copy so we wont need locking
  double dSuggestedAudioMultiplier = m_SynchCorrection.SuggestedAudioMultiplier(rtAHwTime, rtRCTime, bias, adjustment);
  
  // store for EVR stats renderer - it is ok for other threads to change the stats
  m_dSuggestedAudioMultiplier = dSuggestedAudioMultiplier;

  return dSuggestedAudioMultiplier;
}

double CSyncClock::GetBias()
{
  return m_SynchCorrection.GetBias();
}

REFERENCE_TIME CSyncClock::GetPrivateTime()
{
  UINT64 qpcNow = GetCurrentTimestamp();

  UINT64 hwClock = 0;
  UINT64 hwQpc = 0;
  INT64 delta = 0;
  INT64 qpcDelta = 0;

  HRESULT hr = S_FALSE;

  //UINT64 start1 = GetCurrentTimestamp();

  if (m_pSettings->GetHWBasedRefClock())
    hr = m_pAudioRenderer->AudioClock(hwClock, hwQpc, qpcNow);

  //UINT64 end1 = GetCurrentTimestamp();

  if (hr == S_OK)
  {
    if (m_clockSource != HW)
    {
      Log("Using HW clock");
      m_clockSource = HW;
    }
    
    if (m_ullStartQpcHW == 0)
    {
      m_ullStartQpcHW = hwQpc;
      m_ullStartTimeSystem = qpcNow;
    }

    if (m_ullStartTimeHW == 0)
    {
      m_ullStartTimeHW = hwClock;
      m_ullPrevSystemTime = qpcNow;
    }

    qpcDelta = qpcNow - m_ullPrevSystemTime;

    m_llDurationHW = (hwClock - m_ullStartTimeHW);
    m_llDurationSystem = (qpcNow - m_ullStartTimeSystem); 

    if (m_bDiscontinuity)
    {
      m_ullStartTimeHW = m_ullPrevTimeHW = hwClock;
      m_ullStartQpcHW = m_ullPrevQpcHW = hwQpc;
      m_ullStartTimeSystem = qpcNow;
      delta = qpcNow - m_ullPrevSystemTime;

      m_bDiscontinuity = false;
    }
    else
    {
      delta = hwClock - m_ullPrevTimeHW;
	  
      m_ullPrevTimeHW = hwClock;
      m_ullPrevQpcHW = hwQpc;
    }
  }
  else
  {
    if (m_clockSource != SYSTEM)
    {
      Log("Using SYSTEM clock");
      m_clockSource = SYSTEM;
    }

    if (m_ullPrevSystemTime == 0)
    {
      m_ullPrevSystemTime = qpcNow;
      m_ullStartTimeSystem = qpcNow;
    }

    qpcDelta = delta = qpcNow - m_ullPrevSystemTime;
    
    if (m_ullStartTimeSystem == 0)
      m_ullStartTimeSystem = qpcNow;

    if (m_ullPrevSystemTime == 0)
      m_ullPrevSystemTime = qpcNow;
  }

  if (qpcNow < m_ullPrevSystemTime)
    delta += REFERENCE_TIME(ULLONG_MAX) + 1;

  m_ullPrevSystemTime = qpcNow;

  //UINT64 start2 = GetCurrentTimestamp();
  INT64 synchCorrectedDelta = m_SynchCorrection.GetCorrectedTimeDelta(delta, m_ullHWPrivateTime, m_ullPrivateTime);
  //UINT64 end2 = GetCurrentTimestamp();

  //Log("diff %I64d delta: %I64d synchCorrectedDelta: %I64d qpc based delta: %I64d", delta - synchCorrectedDelta, delta, synchCorrectedDelta, qpcDelta);

  m_ullHWPrivateTime = m_ullHWPrivateTime + delta;
  m_ullPrivateTime = m_ullPrivateTime + synchCorrectedDelta;

  //UINT64 qpcEnd = GetCurrentTimestamp();

  /*if (qpcEnd - qpcNow > 2000)
    Log("DUR: %I64d first: %I64d second: %I64d", qpcEnd - qpcNow, end1 - start1, end2 - start2);
  */

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

  // Update the HW clock information
  hr = CBaseReferenceClock::GetTime(rtDsTime);
  if (FAILED(hr))
    return hr;

  // Log("rtTime: %6.3f m_ullRealPrivateTime: %6.3f", rtTime / 10000000.0, m_ullRealPrivateTime/ 10000000.0);

  *rtHwTime = m_ullHWPrivateTime;

  //UINT64 start3 = GetCurrentTimestamp();
  if (rtHwTime && rtDsTime)
    UpdateClockData(*rtHwTime, *rtDsTime);

  //UINT64 end3 = GetCurrentTimestamp();

  /*if (start3 - end3 > 50)
    Log("DUR3: first: %I64d ", end3 - start3);
  */
  
  //Log("CWASAPIRenderFilter::GetHWTime Clocks: Hw Clock: rtHwTime: %10.8f rtTime: %10.8f",
  //  *rtHwTime / 10000000.0, *rtDsTime / 10000000.0);

  return S_OK;
}

void CSyncClock::AddSample(INT64 rtOriginalStart, INT64 rtAdjustedStart, INT64 rtOriginalEnd, INT64 rtAdjustedEnd)
{
  m_SynchCorrection.AddSample(rtOriginalStart, rtAdjustedStart, rtOriginalEnd, rtAdjustedEnd);
}

void CSyncClock::Flush()
{
  m_SynchCorrection.Flush();
}
