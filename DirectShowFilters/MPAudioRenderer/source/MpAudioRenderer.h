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

#include <dsound.h>

#include <MMReg.h>  //must be before other Wasapi headers
#include <strsafe.h>
#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>
#include <Endpointvolume.h>

#include "IAVSyncClock.h"
#include "IRenderDevice.h"

#include "../SoundTouch/Include/SoundTouch.h"
#include "MultiSoundTouch.h"
#include "SyncClock.h"
#include "Settings.h"
#include "VolumeHandler.h"

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
  HRESULT EndOfStream();
  HRESULT BeginFlush();
  HRESULT EndFlush();

  DECLARE_IUNKNOWN
  static CUnknown* WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void **ppv);

  // === IMediaFilter
  STDMETHOD(Run)(REFERENCE_TIME tStart);
  STDMETHOD(Stop)();
  STDMETHOD(Pause)();

  // === IMediaSeeking - implementation is located in MediaSeeking.cpp
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
  STDMETHOD(GetMaxBias)(DOUBLE *pMaxBias);
  STDMETHOD(GetMinBias)(DOUBLE *pMinBias);

  void AudioClock(UINT64& pTimestamp, UINT64& pQpc);

  // RenderDevice(s) uses these getters
  WAVEFORMATEX* WaveFormat() { return m_pWaveFileFormat; }
  AudioRendererSettings* Settings(){ return &m_Settings; }
  IFilterGraph* Graph(){ return m_pGraph; }
  CMultiSoundTouch* SoundTouch(){ return m_pSoundTouch; }
  CCritSec* ResampleLock() { return &m_csResampleLock; }
  CCritSec* RenderThreadLock() { return &m_RenderThreadLock; }
  CCritSec* InterfaceLock() { return &m_InterfaceLock; }

  // CMpcAudioRenderer
private:

  HRESULT GetReferenceClockInterface(REFIID riid, void **ppv);
  WAVEFORMATEX* CreateWaveFormatForAC3(int pSamplesPerSec);

  WAVEFORMATEX*         m_pWaveFileFormat;
  CBaseReferenceClock*	m_pReferenceClock;
  double					      m_dRate;
  CMultiSoundTouch*	    m_pSoundTouch;
   
private:
  CSyncClock      m_Clock;
  CVolumeHandler* m_pVolumeHandler;
  double          m_dBias;
  double          m_dAdjustment;
  CCritSec        m_csResampleLock;
  CCritSec        m_RenderThreadLock;
  LONGLONG        m_dSampleCounter;

  // Used for detecting dropped data
  REFERENCE_TIME m_rtNextSampleTime;
  REFERENCE_TIME m_rtPrevSampleTime;

  // Stream has discontinuity error(s), data must be dropped if gaps are too wide
  bool m_bDropSamples;

  // Flush old audio samples only after the new A/V sync point is reached
  bool m_bFlushSamples;

  AudioRendererSettings m_Settings;

  IRenderDevice* m_pRenderDevice;

  
};
