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

#include "IAVSyncClock.h"

class CMPAudioRenderer;

class CSyncClock: public CBaseReferenceClock
{

public:
  CSyncClock(LPUNKNOWN pUnk, HRESULT *phr, CMPAudioRenderer* pRenderer, bool pUseHWRefClock);

  REFERENCE_TIME GetPrivateTime();

  void SetBias(double pBias);
  void SetAdjustment(double pAdjustment);

  double Bias();
  double Adjustment();
  void GetClockData(CLOCKDATA *pClockData);
  void ProvideAdjustmentDrift(double pDrift);
  double AdjustmentDrift();

private:
  double m_dAdjustment;
  double m_dAdjustmentDrift;
  double m_dBias;

  UINT64 m_ullPrivateTime;
  UINT64 m_ullPrevSystemTime;

  UINT64 m_ullStartQpcHW;
  UINT64 m_ullStartTimeHW;
  UINT64 m_ullStartTimeSystem;

  INT64 m_llDurationHW;
  INT64 m_llDurationSystem;

  UINT64 m_ullPrevTimeHW;
  UINT64 m_ullPrevQpcHW;
  UINT64 m_dwPrevSystemTime;

  double m_dDeltaError;
  INT32 m_lOverallCorrection;

  double  m_dSystemClockMultiplier;

  bool m_bHWBasedRefClock;

  // Not owned
  IReferenceClock*  m_pCurrentRefClock;
  IReferenceClock*  m_pPrevRefClock;
  CMPAudioRenderer* m_pAudioRenderer;
};
