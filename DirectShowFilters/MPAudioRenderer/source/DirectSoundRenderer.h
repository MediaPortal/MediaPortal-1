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

// parts of the code are based on MPC-HC audio renderer source code

#pragma once 

#include "MpAudioRenderer.h"
#include "IRenderDevice.h"

// REFERENCE_TIME time units per second and per millisecond
#define REFTIMES_PER_SEC  10000000
#define REFTIMES_PER_MILLISEC  10000

class DirectSoundRenderer : public IRenderDevice
{
public:
  DirectSoundRenderer(CMPAudioRenderer* pRenderer, HRESULT *phr);
  ~DirectSoundRenderer();

  HRESULT CheckFormat(WAVEFORMATEX* pwfx);
  HRESULT SetMediaType(WAVEFORMATEX* pwfx);
  HRESULT CompleteConnect(IPin *pReceivePin);

  HRESULT DoRenderSample(IMediaSample *pMediaSample, LONGLONG pSampleCounter);
  void    OnReceiveFirstSample(IMediaSample *pMediaSample);

  HRESULT EndOfStream();
  HRESULT BeginFlush();
  HRESULT EndFlush();

  HRESULT Run(REFERENCE_TIME tStart);
  HRESULT Stop(FILTER_STATE pState);
  HRESULT Pause(FILTER_STATE pState);
  HRESULT SetRate(double dRate);

  // DirectSound renderer doesn't use a separate rendering thread currently
  HRESULT StopRendererThread(){ return S_OK; }

  HRESULT AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc){ return E_NOTIMPL; }

  // TODO implement this
  REFERENCE_TIME Latency() { return 0; }

private:

  HRESULT InitCoopLevel();
  HRESULT CreateDSBuffer();
  HRESULT WriteSampleToDSBuffer(IMediaSample *pMediaSample, bool *looped);
  HRESULT ClearBuffer();

  LPDIRECTSOUND8        m_pDS;
  LPDIRECTSOUNDBUFFER   m_pDSBuffer;
  DWORD                 m_dwDSWriteOff;
  int                   m_nDSBufSize;
  REFERENCE_TIME        m_hnsActualDuration;
  DWORD                 m_dwLastBufferTime;
  double                m_dRate;
  CMPAudioRenderer*     m_pRenderer;
};
