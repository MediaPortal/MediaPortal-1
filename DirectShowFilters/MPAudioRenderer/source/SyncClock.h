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

#include "Settings.h"
#include "IAVSyncClock.h"
#include "SynchCorrection.h"
#include "Logger.h"

class CMPAudioRenderer;

class CSyncClock: public CBaseReferenceClock
{
public:
  CSyncClock(LPUNKNOWN pUnk, HRESULT* phr, CMPAudioRenderer* pRenderer, AudioRendererSettings* pSettings, Logger* pLogger);

  void Flush();

  REFERENCE_TIME GetPrivateTime();
  HRESULT GetHWTime(REFERENCE_TIME* rtTime, REFERENCE_TIME* rtHwTimme);

  void SetBias(double pBias);
  void SetAdjustment(double pAdjustment);
  void SetAudioDelay(INT64 pAudioDelay);
  void SetEVRDelay(double pDelay);

  double Bias();
  double Adjustment();
  HRESULT Reset();
  HRESULT Reset(REFERENCE_TIME tStart);
  void GetClockData(CLOCKDATA *pClockData);
  void UpdateClockData(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime);

  void AddSample(INT64 rtOriginalStart, INT64 rtAdjustedStart, INT64 rtOriginalEnd, INT64 rtAdjustedEnd);
  double SuggestedAudioMultiplier(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime, double bias, double adjustment);
  double GetBias();

private:

  enum CLOCK_SOURCE
  {
    NOT_SET,
    HW,
    SYSTEM
  };

  double m_dAdjustment;
  double m_dSuggestedAudioMultiplier;
    
  double m_dEVRDelay;
  double m_dBias;

  UINT64 m_ullHWPrivateTime;
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

  bool m_bDiscontinuity;

  SynchCorrection m_SynchCorrection;

  CLOCK_SOURCE m_clockSource;

  // Not owned
  IReferenceClock*  m_pCurrentRefClock;
  IReferenceClock*  m_pPrevRefClock;
  CMPAudioRenderer* m_pAudioRenderer;
  AudioRendererSettings* m_pSettings;

  // Debug data for the video presenter
  double m_dCurrentDrift;

  Logger* m_pLogger;
};
