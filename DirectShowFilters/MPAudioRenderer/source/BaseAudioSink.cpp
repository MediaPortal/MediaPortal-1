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
#include "BaseAudioSink.h"

CBaseAudioSink::CBaseAudioSink(void) 
: m_pNextSink(NULL)
{
}

CBaseAudioSink::~CBaseAudioSink(void)
{

}

// Initialization
HRESULT CBaseAudioSink::ConnectTo(IAudioSink *pSink)
{
  m_pNextSink = pSink;
  return S_OK;
}

HRESULT CBaseAudioSink::Disconnect()
{
  m_pNextSink = NULL;
  return S_OK;
}

HRESULT CBaseAudioSink::DisconnectAll()
{
  if(m_pNextSink)
    return m_pNextSink->DisconnectAll();
  return S_OK;
}


HRESULT CBaseAudioSink::Init()
{
  if(m_pNextSink)
    return m_pNextSink->Init();
  return S_OK;
}

HRESULT CBaseAudioSink::Cleanup()
{
  if(m_pNextSink)
    return m_pNextSink->Cleanup();
  return S_OK;
}

// Control
HRESULT CBaseAudioSink::Start()
{
  if(m_pNextSink)
    return m_pNextSink->Start();
  return S_OK;
}

HRESULT CBaseAudioSink::BeginStop()
{
  if(m_pNextSink)
    return m_pNextSink->BeginStop();
  return S_OK;
}

HRESULT CBaseAudioSink::EndStop()
{
  if(m_pNextSink)
    return m_pNextSink->EndStop();
  return S_OK;
}

// Format negotiation
HRESULT CBaseAudioSink::NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth)
{
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  if(m_pNextSink)
    return m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth);
  return VFW_E_TYPE_NOT_ACCEPTED;
}

// Processing
HRESULT CBaseAudioSink::PutSample(IMediaSample *pSample)
{
  if(m_pNextSink)
    return m_pNextSink->PutSample(pSample);
  return S_OK;
}

HRESULT CBaseAudioSink::EndOfStream()
{
  if(m_pNextSink)
    return m_pNextSink->EndOfStream();
  return S_OK;
}

HRESULT CBaseAudioSink::BeginFlush()
{
  if(m_pNextSink)
    return m_pNextSink->BeginFlush();
  return S_OK;
}

HRESULT CBaseAudioSink::EndFlush()
{
  if(m_pNextSink)
    return m_pNextSink->EndFlush();
  return S_OK;
}
