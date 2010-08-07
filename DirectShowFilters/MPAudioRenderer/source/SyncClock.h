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

class CMPAudioRenderer;

class CSyncClock: public CBaseReferenceClock
{

public:
  CSyncClock(LPUNKNOWN pUnk, HRESULT *phr, CMPAudioRenderer* pRenderer);

	REFERENCE_TIME GetPrivateTime();

  void SetBias(double pBias);
  void SetAdjustment(double pAdjustment);

  double Bias();
  double Adjustment();

  void SetDiff(DWORD diff);

private:
	DOUBLE m_dAdjustment;
  DOUBLE m_dBias;

  REFERENCE_TIME    m_rtPrivateTime;
  DWORD             m_dwPrevSystemTime;
  CCritSec          m_csClock;

  // Not owned
  IReferenceClock*  m_pCurrentRefClock;
  IReferenceClock*  m_pPrevRefClock;
  CMPAudioRenderer* m_pAudioRenderer;
};
