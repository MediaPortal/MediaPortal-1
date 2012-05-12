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

#include <dsound.h>

#include <MMReg.h>  //must be before other Wasapi headers
#include <strsafe.h>
#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>
#include <Endpointvolume.h>

#include "IAVSyncClock.h"
#include "IAudioSink.h"
#include "IRenderFilter.h"
#include "ITimeStretch.h"

#include "WASAPIRenderFilter.h"
#include "BitDepthAdapter.h"
#include "AC3EncoderFilter.h"
#include "TimeStretchFilter.h"
#include "SampleRateConverterFilter.h"
#include "StreamSanitizerFilter.h"
#include "ChannelMixer.h"

#include "../SoundTouch/Include/SoundTouch.h"
#include "SyncClock.h"
#include "Settings.h"
#include "VolumeHandler.h"

// if you get a compilation error on AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED,
// uncomment the #define below
#ifndef AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
#define AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED      AUDCLNT_ERR(0x019)
#endif

[uuid("EC9ED6FC-7B03-4cb6-8C01-4EABE109F26B")]
class CMPAudioRenderer : public CBaseRenderer, IMediaSeeking, IAVSyncClock, IAMFilterMiscFlags
{
public:
  CMPAudioRenderer(LPUNKNOWN punk, HRESULT *phr);
  ~CMPAudioRenderer();

  static const AMOVIESETUP_FILTER sudASFilter;

  HRESULT Receive(IMediaSample* pSample);
  HRESULT CheckInputType(const CMediaType* mtIn);
  HRESULT CheckMediaType(const CMediaType* pmt);
  HRESULT DoRenderSample(IMediaSample* pMediaSample);
  HRESULT SetMediaType(const CMediaType* pmt);
  HRESULT CompleteConnect(IPin* pReceivePin);
  HRESULT EndOfStream();
  HRESULT BeginFlush();
  HRESULT EndFlush();

  DECLARE_IUNKNOWN
  static CUnknown* WINAPI CreateInstance(LPUNKNOWN punk, HRESULT* phr);
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // === IMediaFilter
  STDMETHOD(Run)(REFERENCE_TIME tStart);
  STDMETHOD(Stop)();
  STDMETHOD(Pause)();
  STDMETHOD(GetState)(DWORD dwMSecs, FILTER_STATE* State);

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

  // === IAMFilterMiscFlags
  ULONG STDMETHODCALLTYPE GetMiscFlags();

  // === IAVSyncClock
  STDMETHOD(AdjustClock)(DOUBLE adjustment);
  STDMETHOD(SetBias)(DOUBLE bias);
  STDMETHOD(GetBias)(DOUBLE* bias);
  STDMETHOD(GetMaxBias)(DOUBLE* pMaxBias);
  STDMETHOD(GetMinBias)(DOUBLE* pMinBias);
  STDMETHOD(GetClockData)(CLOCKDATA* pClockData);
  STDMETHOD(SetEVRPresentationDelay)(DOUBLE pEVRDelay);

  HRESULT AudioClock(UINT64& pTimestamp, UINT64& pQpc);

  // CMpcAudioRenderer
private:

  bool DeliverSample(IMediaSample* pSample);
  HRESULT SetupFilterPipeline();
  HRESULT GetReferenceClockInterface(REFIID riid, void** ppv);
   
private:
  CBaseReferenceClock*	m_pReferenceClock;
  double					      m_dRate;

  UINT64          m_lastSampleArrivalTime;

  REFERENCE_TIME  m_rtNextSample;

  CSyncClock*     m_pClock;
  CVolumeHandler* m_pVolumeHandler;
  double          m_dBias;
  double          m_dAdjustment;

  AudioRendererSettings m_Settings;

  IAudioSink* m_pPipeline; // entry point for the audio filter pipeline
  CWASAPIRenderFilter*  m_pWASAPIRenderer;
  CAC3EncoderFilter*    m_pAC3Encoder;
  CBitDepthAdapter*     m_pInBitDepthAdapter;
  CBitDepthAdapter*     m_pOutBitDepthAdapter;
  CTimeStretchFilter*   m_pTimestretchFilter;
  CSampleRateConverter* m_pSampleRateConverter;
  CStreamSanitizer*     m_pStreamSanitizer;
  CChannelMixer*        m_pChannelMixer;

  IRenderFilter* m_pRenderer;
  ITimeStretch* m_pTimeStretch;
  
  AM_MEDIA_TYPE* m_pMediaType;
};
