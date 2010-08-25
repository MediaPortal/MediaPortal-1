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

#include "alloctracing.h"

CSyncClock::CSyncClock(LPUNKNOWN pUnk, HRESULT *phr, CMPAudioRenderer* pRenderer)
  : CBaseReferenceClock(NAME("SyncClock"), pUnk, phr),
  m_pCurrentRefClock(0),
  m_pPrevRefClock(0),
  m_dAdjustment(1.0),
  m_dBias(1.0),
  m_pAudioRenderer(pRenderer)
{
  m_dwPrevSystemTime = timeGetTime();
  //m_dwPrevSystemTime = 0;
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

REFERENCE_TIME CSyncClock::GetPrivateTime()
{
  CAutoLock cObjectLock(this);

  DWORD dwTime = timeGetTime();
  
  /*  
  UINT64 timestmap(0);
  UINT64 qpc(0);
  m_pAudioRenderer->AudioClock(timestmap, qpc);
  
  DWORD dwTime2 = timestmap / 10000;
  */

  REFERENCE_TIME delta = REFERENCE_TIME(dwTime) - REFERENCE_TIME(m_dwPrevSystemTime);
  if(dwTime < m_dwPrevSystemTime)
  {
    delta +=  REFERENCE_TIME(UINT_MAX) + 1;
  }

  m_dwPrevSystemTime = dwTime;

  delta = (REFERENCE_TIME)(delta * (UNITS / MILLISECONDS) * m_dAdjustment * m_dBias);
  m_rtPrivateTime = m_rtPrivateTime + delta;
  
  return m_rtPrivateTime;
}
