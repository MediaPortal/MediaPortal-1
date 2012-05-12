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

#include "stdafx.h"
#include "IRenderFilter.h"
#include "Settings.h"
#include "queuedaudiosink.h"
#include "SyncClock.h"

#define CLOCK_DATA_SIZE 10

using namespace std;

class CWASAPIRenderFilter : public CQueuedAudioSink, public IRenderFilter
{
public:
  CWASAPIRenderFilter(AudioRendererSettings* pSettings, CSyncClock* pClock);
  virtual ~CWASAPIRenderFilter();

  // IAudioSink implementation
  HRESULT Init();
  HRESULT Cleanup();
  HRESULT NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder);
  HRESULT EndOfStream();
  HRESULT Run(REFERENCE_TIME rtStart);
  HRESULT Pause();
  HRESULT BeginStop();

  // IRenderFilter implementation
  HRESULT AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc);
  REFERENCE_TIME Latency();
  void ReleaseDevice();
  REFERENCE_TIME BufferredDataDuration();

protected:
  // Processing
  DWORD ThreadProc();
  HRESULT PutSample(IMediaSample* pSample);
  HRESULT BeginFlush();
  HRESULT EndFlush();

// Internal implementation
private:

  enum RenderState
  {
    StateStopped,
    StateRunning,
    StatePaused
  };

  // AVRT.dll (Vista or greater)
  typedef HANDLE (__stdcall *PTR_AvSetMmThreadCharacteristicsW)(LPCWSTR TaskName, LPDWORD TaskIndex);
  typedef BOOL (__stdcall *PTR_AvRevertMmThreadCharacteristics)(HANDLE AvrtHandle);

  HRESULT EnableMMCSS();
  HRESULT RevertMMCSS();

  void StopRenderThread();

  PTR_AvSetMmThreadCharacteristicsW	pfAvSetMmThreadCharacteristicsW;
  PTR_AvRevertMmThreadCharacteristics	pfAvRevertMmThreadCharacteristics;

  HMODULE m_hLibAVRT;

  HRESULT CreateAudioClient(bool init = false);
  HRESULT InitAudioClient();
  HRESULT StartAudioClient();
  HRESULT StopAudioClient();
  void CancelDataEvent();
  void ReleaseResources();

  void ResetClockData();
  void UpdateAudioClock();

  HRESULT IsFormatSupported(const WAVEFORMATEXTENSIBLE* pwfx, WAVEFORMATEXTENSIBLE** pwfxAccepted);
  HRESULT CheckSample(IMediaSample* pSample, UINT32 framesToFlush);
  HRESULT CheckStreamTimeline(IMediaSample* pSample, REFERENCE_TIME* pDueTime, UINT32 sampleOffset);
  HRESULT GetBufferSize(const WAVEFORMATEX* pWaveFormatEx, REFERENCE_TIME* pHnsBufferPeriod);
  void CalculateSilence(REFERENCE_TIME* pDueTime, LONGLONG* pBytesOfSilence);
  HRESULT GetWASAPIBuffer(UINT32& bufferSize, UINT32& currentPadding, UINT32& bufferSizeInBytes, BYTE** pData);
  void RenderAudio(BYTE* pTarget, UINT32 bufferSizeInBytes, UINT32 &dataLeftInSample, UINT32 &sampleOffset, IMediaSample* pSample, UINT32 &bytesFilled);
  void RenderSilence(BYTE* pTarget, UINT32 bufferSizeInBytes, LONGLONG &writeSilence, UINT32 &bytesFilled);
  void HandleFlush();

  AudioRendererSettings* m_pSettings;
  IMMDevice*          m_pMMDevice;
  IAudioClient*       m_pAudioClient;
  IAudioRenderClient* m_pRenderClient;
  UINT32              m_nFramesInBuffer;
  HANDLE              m_hTask;
  UINT32              m_nBufferSize;
  IAudioClock*        m_pAudioClock;
  UINT64              m_nHWfreq;
  DWORD               m_dwStreamFlags;

  bool                m_bIsAudioClientStarted;
  bool                m_bDeviceInitialized;

  HANDLE              m_hDataEvent;
  HANDLE              m_hTimerEvent;

  DWORD_PTR           m_dwAdvise;

  RenderState         m_state;
  FILTER_STATE        m_filterState;

  CSyncClock*         m_pClock;

  REFERENCE_TIME      m_rtNextSampleTime;
  REFERENCE_TIME      m_rtHwStart;
  REFERENCE_TIME      m_rtHwPauseTime;
  REFERENCE_TIME      m_rtPauseTime;

  bool                m_bResyncHwClock;

  // Audio HW clock data
  CCritSec            m_csClockLock;
  UINT64              m_ullHwClock[CLOCK_DATA_SIZE];
  UINT64              m_ullHwQpc[CLOCK_DATA_SIZE];
  UINT64              m_dClockDataCollectionCount;
  UINT64              m_ullPrevPos;
  UINT64              m_ullPrevQpc;
  INT64               m_llPosError;
  int                 m_dClockPosIn;
  int                 m_dClockPosOut;

  vector<HANDLE> m_hDataEvents;
  vector<HANDLE> m_hSampleEvents;

  vector<DWORD> m_dwDataWaitObjects;
  vector<DWORD> m_dwSampleWaitObjects;

  UINT32 m_nSampleOffset;
  UINT32 m_nDataLeftInSample;
};
