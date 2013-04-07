/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 *  Major part of this file's content is based on MPC-HC's source code 
 *  http://mpc-hc.sourceforge.net/
 */

#pragma once

#include "stdafx.h"
#include <streams.h>
#include <mmsystem.h>
#include <d3d9.h>

class MPEVRCustomPresenter;

class COuterEVR : public CUnknown, public IBaseFilter//, public IMediaSeeking
{
public:
  COuterEVR(const TCHAR* pName, LPUNKNOWN pUnk, HRESULT& hr, MPEVRCustomPresenter *pAllocatorPresenter);
  ~COuterEVR();

  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv); 

  // IBaseFilter
  STDMETHODIMP EnumPins(__out  IEnumPins **ppEnum);
  STDMETHODIMP FindPin(LPCWSTR Id, __out  IPin **ppPin);
  STDMETHODIMP QueryFilterInfo(__out  FILTER_INFO *pInfo);
  STDMETHODIMP JoinFilterGraph(__in_opt  IFilterGraph *pGraph, __in_opt  LPCWSTR pName);
  STDMETHODIMP QueryVendorInfo(__out  LPWSTR *pVendorInfo);
  STDMETHODIMP Stop();
  STDMETHODIMP Pause();
  STDMETHODIMP Run(REFERENCE_TIME tStart);
  STDMETHODIMP GetState( DWORD dwMilliSecsTimeout, __out  FILTER_STATE *State);
  STDMETHODIMP SetSyncSource(__in_opt  IReferenceClock *pClock);
  STDMETHODIMP GetSyncSource(__deref_out_opt  IReferenceClock **pClock);
  STDMETHODIMP GetClassID(__RPC__out CLSID *pClassID);

  // IMediaSeeking
/*  STDMETHODIMP IsFormatSupported(const GUID* pFormat);
  STDMETHODIMP QueryPreferredFormat(GUID* pFormat);
  STDMETHODIMP SetTimeFormat(const GUID* pFormat);
  STDMETHODIMP IsUsingTimeFormat(const GUID* pFormat);
  STDMETHODIMP GetTimeFormat(GUID* pFormat);
  STDMETHODIMP GetDuration(LONGLONG* pDuration);
  STDMETHODIMP GetStopPosition(LONGLONG* pStop);
  STDMETHODIMP GetCurrentPosition(LONGLONG* pCurrent);
  STDMETHODIMP CheckCapabilities(DWORD* pCapabilities);
  STDMETHODIMP GetCapabilities(DWORD* pCapabilities);
  STDMETHODIMP ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat);
  STDMETHODIMP SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG * pStop, DWORD StopFlags);
  STDMETHODIMP GetPositions(LONGLONG* pCurrent, LONGLONG* pStop);
  STDMETHODIMP GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest);
  STDMETHODIMP SetRate(double dRate);
  STDMETHODIMP GetRate(double* pdRate);
  STDMETHODIMP GetPreroll(LONGLONG *pPreroll);
*/
  // IUnknown
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject);
  ULONG STDMETHODCALLTYPE AddRef();
  ULONG STDMETHODCALLTYPE Release();
  ULONG STDMETHODCALLTYPE NonDelegatingAddRef();
  ULONG STDMETHODCALLTYPE NonDelegatingRelease();

private:
  IUnknown* m_pEVR;
  MPEVRCustomPresenter* m_pAllocatorPresenter;

  long m_refCount;
};
