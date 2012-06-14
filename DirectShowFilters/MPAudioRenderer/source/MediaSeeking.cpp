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

#include "stdafx.h"
#include <ks.h>
#include <ksmedia.h>

#include "MpAudioRenderer.h"

#include "alloctracing.h"

extern void Log(const char* fmt, ...);

// IMediaSeeking interface implementation

STDMETHODIMP CMPAudioRenderer::IsFormatSupported(const GUID* pFormat)
{
  return m_pPosition->IsFormatSupported(pFormat);
}

STDMETHODIMP CMPAudioRenderer::QueryPreferredFormat(GUID* pFormat)
{
  return m_pPosition->QueryPreferredFormat(pFormat);
}

STDMETHODIMP CMPAudioRenderer::SetTimeFormat(const GUID* pFormat)
{
  return m_pPosition->SetTimeFormat(pFormat);
}

STDMETHODIMP CMPAudioRenderer::IsUsingTimeFormat(const GUID* pFormat)
{
  return m_pPosition->IsUsingTimeFormat(pFormat);
}

STDMETHODIMP CMPAudioRenderer::GetTimeFormat(GUID* pFormat)
{
  return m_pPosition->GetTimeFormat(pFormat);
}

STDMETHODIMP CMPAudioRenderer::GetDuration(LONGLONG* pDuration)
{
  return m_pPosition->GetDuration(pDuration);
}

STDMETHODIMP CMPAudioRenderer::GetStopPosition(LONGLONG* pStop)
{
  return m_pPosition->GetStopPosition(pStop);
}

STDMETHODIMP CMPAudioRenderer::GetCurrentPosition(LONGLONG* pCurrent)
{
  return m_pPosition->GetCurrentPosition(pCurrent);
}

STDMETHODIMP CMPAudioRenderer::GetCapabilities(DWORD* pCapabilities)
{
  return m_pPosition->GetCapabilities(pCapabilities);
}

STDMETHODIMP CMPAudioRenderer::CheckCapabilities(DWORD* pCapabilities)
{
  return m_pPosition->CheckCapabilities(pCapabilities);
}

STDMETHODIMP CMPAudioRenderer::ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat)
{
  return m_pPosition->ConvertTimeFormat(pTarget, pTargetFormat, Source, pSourceFormat);
}

STDMETHODIMP CMPAudioRenderer::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags)
{
  return m_pPosition->SetPositions(pCurrent, CurrentFlags, pStop, StopFlags);
}

STDMETHODIMP CMPAudioRenderer::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  return m_pPosition->GetPositions(pCurrent, pStop);
}

STDMETHODIMP CMPAudioRenderer::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  return m_pPosition->GetAvailable(pEarliest, pLatest);
}

STDMETHODIMP CMPAudioRenderer::SetRate(double dRate)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);

  if (dRate < 0.1)
    return VFW_E_UNSUPPORTED_AUDIO;

  if (m_pTimeStretch)
    m_pTimeStretch->setRate(dRate);

  m_dRate = dRate;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::GetRate(double* pdRate)
{
  return m_pPosition->GetRate(pdRate);
}

STDMETHODIMP CMPAudioRenderer::GetPreroll(LONGLONG* pPreroll)
{
  CheckPointer(pPreroll, E_POINTER);
  
  if (m_pRenderer)
    (*pPreroll) = m_pRenderer->Latency() * 2;
  else
    (*pPreroll) = 0;

  return S_OK;
}