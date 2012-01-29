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
#include "TimeStretchFilter.h"

#include "alloctracing.h"

extern HRESULT CopyWaveFormatEx(WAVEFORMATEX** dst, const WAVEFORMATEX* src);

extern void Log(const char* fmt, ...);
extern void LogWaveFormat(const WAVEFORMATEX* pwfx, const char* text);

CTimeStretchFilter::CTimeStretchFilter(AudioRendererSettings* pSettings) :
  m_pSettings(pSettings)
{
}

CTimeStretchFilter::~CTimeStretchFilter(void)
{
  Log("CTimeStretchFilter - destructor - instance 0x%x", this);
  

  Log("CTimeStretchFilter - destructor - instance 0x%x - end", this);
}

//Initialization
HRESULT CTimeStretchFilter::Init()
{
  m_hSampleEvents.push_back(m_hInputAvailableEvent);
  m_hSampleEvents.push_back(m_hStopThreadEvent);

  m_dwSampleWaitObjects.push_back(S_OK);
  m_dwSampleWaitObjects.push_back(MPAR_S_THREAD_STOPPING);

  return CQueuedAudioSink::Init();
}

HRESULT CTimeStretchFilter::Cleanup()
{
  HRESULT hr = CQueuedAudioSink::Cleanup();
  return hr;
}

/*
// Format negotiation
HRESULT CTimeStretchFilter::NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // check always from the renderer device?
  if (FormatsEqual(pwfx, m_pInputFormat))
    return S_OK;

  bool bApplyChanges = nApplyChangesDepth != 0;

  if (!bApplyChanges)
    return S_OK;

  Log("CTimeStretchFilter::NegotiateFormat");
  LogWaveFormat(pwfx, "CTimeStretchFilter::NegotiateFormat");

  HRESULT hr = VFW_E_CANNOT_CONNECT;

  
  // TODO


  return hr;
}*/

HRESULT CTimeStretchFilter::EndOfStream()
{
  // Queue an EOS marker so that it gets processed in 
  // the same thread as the audio data.
  PutSample(NULL);
  // wait until input queue is empty
  //if(m_hInputQueueEmptyEvent)
  //  WaitForSingleObject(m_hInputQueueEmptyEvent, END_OF_STREAM_FLUSH_TIMEOUT); // TODO make this depend on the amount of data in the queue
  return S_OK;
}

// Processing
DWORD CTimeStretchFilter::ThreadProc()
{
  Log("CTimeStretchFilter::timestretch thread - starting up - thread ID: %d", m_ThreadId);
  
  AudioSinkCommand command;
  IMediaSample* sample = NULL;

  while(true)
  {
    HRESULT hr = GetNextSampleOrCommand(&command, &sample, INFINITE, &m_hSampleEvents, &m_dwSampleWaitObjects);

    if (hr == MPAR_S_THREAD_STOPPING)
    {
      Log("CTimeStretchFilter::timestretch threa - closing down - thread ID: %d", m_ThreadId);
      return 0;
    }
    else
    {
      if (command == ASC_Pause && sample)
      {
        sample->Release();
        sample = NULL;
      }
      else
      {
        m_pNextOutSample = sample;
        OutputNextSample();
      }
    }
  }

  return 0;
}

