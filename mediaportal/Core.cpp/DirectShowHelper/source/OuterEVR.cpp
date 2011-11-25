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

#include "stdafx.h"
#include "OuterEVR.h"
#include "EVRCustomPresenter.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

DEFINE_GUID(CLSID_EVRAllocatorPresenter,
			0x7612b889, 0xe070, 0x4bcc, 0xb8, 0x8, 0x91, 0xcb, 0x79, 0x41, 0x74, 0xab);

COuterEVR::COuterEVR(const TCHAR* pName, LPUNKNOWN pUnk, HRESULT& hr, MPEVRCustomPresenter *pAllocatorPresenter) 
  : CUnknown(pName, pUnk) 
{
  ASSERT(m_pAllocatorPresenter);

  m_pAllocatorPresenter = pAllocatorPresenter;
  hr = CoCreateInstance(CLSID_EnhancedVideoRenderer, this, CLSCTX_INPROC_SERVER, IID_IUnknown, (void **)&m_pEVR);
  m_refCount = 0;
}

COuterEVR::~COuterEVR()
{
  if (m_pAllocatorPresenter)
  {
    delete m_pAllocatorPresenter;
  }
}

STDMETHODIMP COuterEVR::NonDelegatingQueryInterface(REFIID riid, void** ppv) 
{
  if (riid == __uuidof(IMediaFilter)) 
  {
    return GetInterface((IMediaFilter*)this, ppv);
  }
  else if (riid == __uuidof(IPersist)) 
  {
    return GetInterface((IPersist*)this, ppv);
  }
  else if (riid == __uuidof(IBaseFilter)) 
  {
    return GetInterface((IBaseFilter*)this, ppv);
  }

  HRESULT hr = m_pEVR ? m_pEVR->QueryInterface(riid, ppv) : E_NOINTERFACE;
  return SUCCEEDED(hr) ? hr : __super::NonDelegatingQueryInterface(riid, ppv);
}

STDMETHODIMP COuterEVR::EnumPins(__out  IEnumPins **ppEnum) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->EnumPins(ppEnum);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::FindPin(LPCWSTR Id, __out  IPin **ppPin) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->FindPin(Id, ppPin);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::QueryFilterInfo(__out  FILTER_INFO *pInfo) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->QueryFilterInfo(pInfo);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::JoinFilterGraph(__in_opt  IFilterGraph *pGraph, __in_opt  LPCWSTR pName) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->JoinFilterGraph(pGraph, pName);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::QueryVendorInfo(__out  LPWSTR *pVendorInfo) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->QueryVendorInfo(pVendorInfo);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::Stop() 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->Stop();
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::Pause() 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->Pause();
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::Run(REFERENCE_TIME tStart) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->Run(tStart);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::GetState(DWORD dwMilliSecsTimeout, __out  FILTER_STATE *State)
{
  HRESULT ReturnValue;
  if (m_pAllocatorPresenter->GetState(dwMilliSecsTimeout, State, ReturnValue)) 
  {
    return ReturnValue;
  }
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->GetState(dwMilliSecsTimeout, State);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::SetSyncSource(__in_opt  IReferenceClock *pClock) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->SetSyncSource(pClock);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::GetSyncSource(__deref_out_opt  IReferenceClock **pClock) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->GetSyncSource(pClock);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::GetClassID(__RPC__out CLSID *pClassID) 
{
  CComPtr<IBaseFilter> pEVRBase;
  if (m_pEVR) 
  {
    m_pEVR->QueryInterface(&pEVRBase);
  }
  if (pEVRBase) 
  {
    return pEVRBase->GetClassID(pClassID);
  }
  return E_NOTIMPL;
}

STDMETHODIMP COuterEVR::QueryInterface(REFIID riid, __deref_out void **ppv) 
{
  return GetOwner()->QueryInterface(riid,ppv);
}       

ULONG COuterEVR::AddRef()
{
  return NonDelegatingAddRef();
}

ULONG COuterEVR::Release()
{
  return NonDelegatingRelease();
}

ULONG COuterEVR::NonDelegatingAddRef()
{
  return InterlockedIncrement(&m_refCount);
}

ULONG COuterEVR::NonDelegatingRelease()
{
  LONG ret = InterlockedDecrement(&m_refCount);
  ASSERT(ret >= 0);
  if (ret == 0)
  {
    delete this;
  }
  
  return ret;
}
