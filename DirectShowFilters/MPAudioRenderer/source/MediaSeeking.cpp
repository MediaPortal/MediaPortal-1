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

#include "stdafx.h"
#include <ks.h>
#include <ksmedia.h>

#include "MpAudioRenderer.h"

#include "alloctracing.h"

extern void Log(const char *fmt, ...);

// IMediaSeeking interface implementation

STDMETHODIMP CMPAudioRenderer::IsFormatSupported(const GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  // only seeking in time (REFERENCE_TIME units) is supported
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

STDMETHODIMP CMPAudioRenderer::QueryPreferredFormat(GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  *pFormat = TIME_FORMAT_MEDIA_TIME;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::SetTimeFormat(const GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);

  // nothing to set; just check that it's TIME_FORMAT_TIME
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : E_INVALIDARG;
}

STDMETHODIMP CMPAudioRenderer::IsUsingTimeFormat(const GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

STDMETHODIMP CMPAudioRenderer::GetTimeFormat(GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  *pFormat = TIME_FORMAT_MEDIA_TIME;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::GetDuration(LONGLONG* pDuration)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetStopPosition(LONGLONG* pStop)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetCurrentPosition(LONGLONG* pCurrent)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetCapabilities(DWORD* pCapabilities)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::CheckCapabilities(DWORD* pCapabilities)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG * pStop, DWORD StopFlags)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::SetRate(double dRate)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  if (m_dRate != dRate && dRate == 1.0)
  {
    m_pRenderDevice->SetRate(dRate);

    m_pSoundTouch->BeginFlush();
    m_pSoundTouch->clear();
    m_pSoundTouch->EndFlush();
  }
  
  m_dRate = dRate;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::GetRate(double* pdRate)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetPreroll(LONGLONG *pPreroll)
{
  CheckPointer(pPreroll, E_POINTER);
  (*pPreroll) = m_pRenderDevice->Latency();
  return S_OK;
}