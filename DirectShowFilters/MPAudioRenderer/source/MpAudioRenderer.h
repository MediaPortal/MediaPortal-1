/* 
 * $Id: MpcAudioRenderer.h 1785 2010-04-09 14:12:59Z xhmikosr $
 *
 * (C) 2006-2010 see AUTHORS
 *
 * This file is part of mplayerc.
 *
 * Mplayerc is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * Mplayerc is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#include <streams.h>
#include <dsound.h>

#include <MMReg.h>  //must be before other Wasapi headers
#include <strsafe.h>
#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>
#include <Endpointvolume.h>

#include "../SoundTouch/Include/SoundTouch.h"
#include "MultiSoundTouch.h"
#include "SyncClock.h"
#include "IAVSyncClock.h"

// REFERENCE_TIME time units per second and per millisecond
#define REFTIMES_PER_SEC  10000000
#define REFTIMES_PER_MILLISEC  10000

// if you get a compilation error on AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED,
// uncomment the #define below
#ifndef AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
#define AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED      AUDCLNT_ERR(0x019)
#endif

[uuid("EC9ED6FC-7B03-4cb6-8C01-4EABE109F26B")]
class CMPAudioRenderer : public CBaseRenderer, IMediaSeeking, IAVSyncClock
{
public:
	CMPAudioRenderer(LPUNKNOWN punk, HRESULT *phr);
	~CMPAudioRenderer();

  static const AMOVIESETUP_FILTER sudASFilter;

	HRESULT CheckInputType(const CMediaType* mtIn);
	HRESULT CheckMediaType(const CMediaType *pmt);
	HRESULT DoRenderSample(IMediaSample *pMediaSample);
	void    OnReceiveFirstSample(IMediaSample *pMediaSample);
	BOOL    ScheduleSample(IMediaSample *pMediaSample);
	HRESULT SetMediaType(const CMediaType *pmt);
  HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT EndOfStream(void);
  HRESULT BeginFlush();
  HRESULT EndFlush();

  DECLARE_IUNKNOWN
  static CUnknown* WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void **ppv);

	// === IMediaFilter
	STDMETHOD(Run)(REFERENCE_TIME tStart);
	STDMETHOD(Stop)();
	STDMETHOD(Pause)();

  // === IMediaSeeking
  STDMETHODIMP IsFormatSupported(const GUID* pFormat);
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

  // === IAVSyncClock
	STDMETHOD(AdjustClock)(DOUBLE adjustment);
	STDMETHOD(SetBias)(DOUBLE bias);
	STDMETHOD(GetBias)(DOUBLE *bias);

  void AudioClock(UINT64& pTimestamp, UINT64& pQpc);

  // CMpcAudioRenderer
private:

  // For accessing the registry
  void            LoadSettingsFromRegistry();
  void            ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void            WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void            WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data);
  
  HRESULT					DoRenderSampleDirectSound(IMediaSample *pMediaSample);

  HRESULT					InitCoopLevel();
  HRESULT					ClearBuffer();
  HRESULT					CreateDSBuffer();
  HRESULT					GetReferenceClockInterface(REFIID riid, void **ppv);
  HRESULT					WriteSampleToDSBuffer(IMediaSample *pMediaSample, bool *looped);

  LPDIRECTSOUND8        m_pDS;
  LPDIRECTSOUNDBUFFER   m_pDSBuffer;
  DWORD                 m_dwDSWriteOff;
  WAVEFORMATEX*         m_pWaveFileFormat;
  int						        m_nDSBufSize;
  CBaseReferenceClock*	m_pReferenceClock;
  double					      m_dRate;

  CMultiSoundTouch*	    m_pSoundTouch;

  // CMpcAudioRenderer WASAPI methods
  HRESULT     GetDefaultAudioDevice(IMMDevice **ppMMDevice);
  HRESULT     CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient);
  HRESULT     InitAudioClient(WAVEFORMATEX *pWaveFormatEx, IAudioClient *pAudioClient, IAudioRenderClient **ppRenderClient);
  HRESULT     CheckAudioClient(WAVEFORMATEX *pWaveFormatEx);
  bool        CheckFormatChanged(WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx);
  HRESULT     DoRenderSampleWasapi(IMediaSample *pMediaSample);
  HRESULT     GetBufferSize(WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod);
   
  // WASAPI variables
  bool                m_bUseWASAPI;
  IMMDevice*          m_pMMDevice;
  IAudioClient*       m_pAudioClient;
  IAudioRenderClient* m_pRenderClient;
  UINT32              m_nFramesInBuffer;
  REFERENCE_TIME      m_hnsPeriod;
  REFERENCE_TIME      m_hnsActualDuration;
  HANDLE              m_hTask;
  CCritSec            m_csCheck;
  UINT32              m_nBufferSize;
  bool                m_bIsAudioClientStarted;
  DWORD               m_dwLastBufferTime;
  AUDCLNT_SHAREMODE   m_WASAPIShareMode;

  // AVRT.dll (Vista or greater)
  typedef HANDLE (__stdcall *PTR_AvSetMmThreadCharacteristicsW)(LPCWSTR TaskName, LPDWORD TaskIndex);
  typedef BOOL (__stdcall *PTR_AvRevertMmThreadCharacteristics)(HANDLE AvrtHandle);

  PTR_AvSetMmThreadCharacteristicsW		pfAvSetMmThreadCharacteristicsW;
  PTR_AvRevertMmThreadCharacteristics		pfAvRevertMmThreadCharacteristics;

private:
  CSyncClock  m_Clock;
  double      m_dBias;
  double      m_dAdjustment;
  CCritSec    m_csResampleLock;
  bool        m_bUseTimeStretching;

  DWORD       m_dwTimeStart;
  bool        m_bFirstAudioSample;

  IAudioClock*  m_pAudioClock;
  UINT64        m_nHWfreq;

  bool        m_bUseThreads;
};
