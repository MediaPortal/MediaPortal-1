#include <streams.h>
#include "SyncClock.h"

CSyncClock::CSyncClock(LPUNKNOWN pUnk, HRESULT *phr)
  : CBaseReferenceClock(NAME("SyncClock"), pUnk, phr),
  m_pCurrentRefClock(0),
  m_pPrevRefClock(0),
  m_dAdjustment(1.0),
  m_dBias(1.0)
{
  m_dwPrevSystemTime = timeGetTime();
  m_rtPrivateTime = (UNITS / MILLISECONDS) * m_dwPrevSystemTime;
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
