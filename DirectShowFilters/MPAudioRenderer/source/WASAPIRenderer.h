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

// parts of the code are based on MPC-HC audio renderer source code

#pragma once 

#include "MpAudioRenderer.h"
#include "IRenderDevice.h"

#define CLOCK_DATA_SIZE 10

class WASAPIRenderer : public IRenderDevice
{
public:
  WASAPIRenderer(CMPAudioRenderer* pRenderer, HRESULT *phr);
  ~WASAPIRenderer();

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

  HRESULT StopRendererThread();

  HRESULT AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc);

  REFERENCE_TIME Latency();

private:

  // AVRT.dll (Vista or greater)
  typedef HANDLE (__stdcall *PTR_AvSetMmThreadCharacteristicsW)(LPCWSTR TaskName, LPDWORD TaskIndex);
  typedef BOOL (__stdcall *PTR_AvRevertMmThreadCharacteristics)(HANDLE AvrtHandle);

  HRESULT EnableMMCSS();
  HRESULT RevertMMCSS();

  void UpdateAudioClock();
  void ResetClockData();

  PTR_AvSetMmThreadCharacteristicsW		pfAvSetMmThreadCharacteristicsW;
  PTR_AvRevertMmThreadCharacteristics		pfAvRevertMmThreadCharacteristics;

  HRESULT GetAudioDevice(IMMDevice **ppMMDevice);
  HRESULT GetAvailableAudioDevices(IMMDeviceCollection **ppMMDevices, bool pLog); // caller must release ppMMDevices!
  HRESULT CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient);
  HRESULT InitAudioClient(const WAVEFORMATEX *pWaveFormatEx, IAudioClient *pAudioClient, IAudioRenderClient **ppRenderClient);
  HRESULT CheckAudioClient(WAVEFORMATEX *pWaveFormatEx);
  bool    CheckFormatChanged(const WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx);
  HRESULT DoRenderSampleWasapi(IMediaSample *pMediaSample);
  HRESULT GetBufferSize(const WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod);
  
  void CancelDataEvent();
  void StartAudioClient(IAudioClient** ppAudioClient);
  void StopAudioClient(IAudioClient** ppAudioClient);

  IMMDevice*          m_pMMDevice;
  IAudioClient*       m_pAudioClient;
  IAudioRenderClient* m_pRenderClient;
  UINT32              m_nFramesInBuffer;
  HANDLE              m_hTask;
  CCritSec            m_csCheck;
  UINT32              m_nBufferSize;
  bool                m_bIsAudioClientStarted;
  bool                m_bReinitAfterStop;
  bool                m_bDiscardCurrentSample;
  double              m_dRate;
  CMPAudioRenderer*   m_pRenderer;
  IAudioClock*        m_pAudioClock;
  UINT64              m_nHWfreq;
  WAVEFORMATEX*       m_pRenderFormat;
  DWORD               m_StreamFlags;

  // Audio HW clock data
  CCritSec            m_csClockLock;
  UINT64              m_ullHwClock[CLOCK_DATA_SIZE];
  UINT64              m_ullHwQpc[CLOCK_DATA_SIZE];
  UINT64              m_dClockDataCollectionCount;
  int                 m_dClockPosIn;
  int                 m_dClockPosOut;
 
  // Rendering thread
  static DWORD WINAPI RenderThreadEntryPoint(LPVOID lpParameter);
  DWORD RenderThread();
  DWORD m_threadId;
 
  bool m_bThreadPaused;

  HRESULT StartRendererThread();
  HRESULT PauseRendererThread();

  HANDLE m_hRenderThread;

  HANDLE m_hDataEvent;
  HANDLE m_hPauseEvent;
  HANDLE m_hResumeEvent;
  HANDLE m_hWaitPauseEvent;
  HANDLE m_hWaitResumeEvent;
  HANDLE m_hStopRenderThreadEvent;
};
