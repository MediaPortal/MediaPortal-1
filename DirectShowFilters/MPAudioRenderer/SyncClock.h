#pragma once

#include <streams.h>

class CSyncClock: public CBaseReferenceClock
{

public:
  CSyncClock(LPUNKNOWN pUnk, HRESULT *phr);

	REFERENCE_TIME GetPrivateTime();

  void SetBias(double pBias);
  void SetAdjustment(double pAdjustment);

  double Bias();
  double Adjustment();

private:
	DOUBLE m_dAdjustment;
  DOUBLE m_dBias;

  REFERENCE_TIME    m_rtPrivateTime;
  DWORD             m_dwPrevSystemTime;
  CCritSec          m_csClock;
  IReferenceClock*  m_pCurrentRefClock;
  IReferenceClock*  m_pPrevRefClock;
};
