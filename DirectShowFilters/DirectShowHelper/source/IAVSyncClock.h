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

#pragma once

#ifndef IAVSYNCCLOCK
#define IAVSYNCCLOCK

struct CLOCKDATA
{
  double driftMultiplier;
  double driftHWvsSystem;
  double currentDrift;
  double resamplingAdjustment;
};

// {91A198BA-1C78-4c31-A50F-0F5C7578F078}
static const GUID IID_IAVSyncClock = { 0x91a198ba, 0x1c78, 0x4c31, { 0xa5, 0xf, 0xf, 0x5c, 0x75, 0x78, 0xf0, 0x78 } };
DEFINE_GUID(CLSID_IAVSyncClock, 0x91a198ba, 0x1c78, 0x4c31, 0xa5, 0xf, 0xf, 0x5c, 0x75, 0x78, 0xf0, 0x78);

MIDL_INTERFACE("91A198BA-1C78-4c31-A50F-0F5C7578F078")
IAVSyncClock: public IUnknown
{
public:
  virtual HRESULT STDMETHODCALLTYPE AdjustClock(DOUBLE adjustment) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetBias(DOUBLE bias) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetBias(DOUBLE *bias) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetMaxBias(DOUBLE *bias) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetMinBias(DOUBLE *bias) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetClockData(CLOCKDATA *clockData) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetEVRPresentationDelay(DOUBLE EVRDelay) = 0;

};
#endif // IAVSYNCCLOCK
