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

#pragma once 

#include "stdafx.h"
#include "Settings.h"
#include "queuedaudiosink.h"

#define MPAR_S_NEED_DATA ((HRESULT)0x00040201)

using namespace std;

class CWASAPIRenderFilter :
  public CQueuedAudioSink
{
public:
  CWASAPIRenderFilter(AudioRendererSettings *pSettings);
  virtual ~CWASAPIRenderFilter(void);

// IAudioSink implementation
public:
  // Initialization
  virtual HRESULT Init();
  virtual HRESULT Cleanup();

  // Format negotiation
  virtual HRESULT NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth);

  // Processing
  virtual HRESULT EndOfStream();

protected:
  // Processing
  virtual DWORD ThreadProc();

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

  PTR_AvSetMmThreadCharacteristicsW		pfAvSetMmThreadCharacteristicsW;
  PTR_AvRevertMmThreadCharacteristics		pfAvRevertMmThreadCharacteristics;

  HRESULT GetAudioDevice(IMMDevice **ppMMDevice);
  HRESULT GetAvailableAudioDevices(IMMDeviceCollection **ppMMDevices, bool pLog); // caller must release ppMMDevices!
  HRESULT CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient);
  HRESULT InitAudioClient(const WAVEFORMATEX *pWaveFormatEx, IAudioRenderClient **ppRenderClient);
  HRESULT StartAudioClient(IAudioClient** ppAudioClient);
  HRESULT StopAudioClient(IAudioClient** ppAudioClient);
  void CancelDataEvent();

  HRESULT CheckAudioClient(WAVEFORMATEX *pWaveFormatEx);
  bool    CheckFormatChanged(const WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx);
  HRESULT GetBufferSize(const WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod);

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

  BOOL                m_bIsAudioClientStarted;

  HANDLE              m_hDataEvent;

  RenderState       m_state;

  vector<HANDLE> m_hDataEvents;
  vector<HANDLE> m_hOOBCommandEvents;
  vector<HANDLE> m_hSampleEvents;

  vector<DWORD> m_dwDataWaitObjects;
  vector<DWORD> m_dwOOBCommandWaitObjects;
  vector<DWORD> m_dwSampleWaitObjects;
};
