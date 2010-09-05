/* 
 * $Id: MpcAudioRenderer.cpp 1785 2010-04-09 14:12:59Z xhmikosr $
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

#include "stdafx.h"
#include <initguid.h>
#include "moreuuids.h"
#include <ks.h>
#include <ksmedia.h>
#include <propkey.h>
#include <FunctionDiscoveryKeys_devpkey.h>

#include "MpAudioRenderer.h"
#include "DirectSoundRenderer.h"
#include "WASAPIRenderer.h"
#include "FilterApp.h"

#include "alloctracing.h"

CFilterApp theApp;

#define MAX_SAMPLE_TIME_ERROR 10000 // 1.0 ms

extern HRESULT CopyWaveFormatEx(WAVEFORMATEX **dst, const WAVEFORMATEX *src);

CUnknown* WINAPI CMPAudioRenderer::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
  ASSERT(phr);
  CMPAudioRenderer *pNewObject = new CMPAudioRenderer(punk, phr);

  if (!pNewObject)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
  }
  return pNewObject;
}

// for logging
extern void Log(const char *fmt, ...);
extern void LogWaveFormat(const WAVEFORMATEX* pwfx, const char *text);

CMPAudioRenderer::CMPAudioRenderer(LPUNKNOWN punk, HRESULT *phr)
: CBaseRenderer(__uuidof(this), NAME("MediaPortal - Audio Renderer"), punk, phr)
, m_Clock(static_cast<IBaseFilter*>(this), phr, this)
, m_pSoundTouch(NULL)
, m_dRate(1.0)
, m_pReferenceClock(NULL)
, m_pWaveFileFormat(NULL)
, m_dBias(1.0)
, m_dAdjustment(1.0)
, m_dSampleCounter(0)
, m_rtNextSampleTime(0)
, m_rtPrevSampleTime(0)
, m_bDropSamples(false)
, m_bFlushSamples(false)
, m_pVolumeHandler(NULL)
{
  Log("CMPAudioRenderer - instance 0x%x", this);

  if (m_Settings.m_bUseWASAPI)
  {
    m_pRenderDevice = new WASAPIRenderer(this, phr);   
    
    if (*phr != S_OK)
      m_Settings.m_bUseWASAPI = false;
  }
  
  if (!m_Settings.m_bUseWASAPI)
  {
    m_pRenderDevice = new DirectSoundRenderer(this, phr);

    if (FAILED(*phr))
      return;
  }

  m_pSoundTouch = new CMultiSoundTouch(m_Settings.m_bEnableAC3Encoding, m_Settings.m_AC3bitrate);
  
  if (!m_pSoundTouch)
  {
    if(phr)
    {
      *phr = E_OUTOFMEMORY;
      return;
    }
  }

  m_pVolumeHandler = new CVolumeHandler(punk);

  if (m_pVolumeHandler)
  {
    m_pVolumeHandler->AddRef();
  }
  else
  {
    if(phr)
      *phr = E_OUTOFMEMORY;
  }
}


CMPAudioRenderer::~CMPAudioRenderer()
{
  Log("MP Audio Renderer - destructor - instance 0x%x", this);
  
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);
  CAutoLock cInterfaceLock(&m_InterfaceLock);

  Stop();

  if (m_pVolumeHandler)
    m_pVolumeHandler->Release();

  if (m_pSoundTouch)
    m_pSoundTouch->StopResamplingThread();

  // Get rid of the render thread
  if (m_pRenderDevice)
    m_pRenderDevice->StopRendererThread();

  delete m_pSoundTouch;
  delete m_pRenderDevice;

  if (m_pReferenceClock)
  {
    SetSyncSource(NULL);
    SAFE_RELEASE(m_pReferenceClock);
  }

  SAFE_DELETE_WAVEFORMATEX(m_pWaveFileFormat);

  Log("MP Audio Renderer - destructor - instance 0x%x - end", this);
}

WAVEFORMATEX* CMPAudioRenderer::CreateWaveFormatForAC3(int pSamplesPerSec)
{
  WAVEFORMATEX* pwfx = (WAVEFORMATEX*)new BYTE[sizeof(WAVEFORMATEX)];
  if (pwfx)
  {
    // SPDIF uses static 2 channels and 16 bit. 
    // AC3 header contains the real stream information
    pwfx->wFormatTag = WAVE_FORMAT_DOLBY_AC3_SPDIF;
    pwfx->wBitsPerSample = 16;
    pwfx->nBlockAlign = 4;
    pwfx->nChannels = 2;
    pwfx->nSamplesPerSec = pSamplesPerSec;
    pwfx->nAvgBytesPerSec = pwfx->nSamplesPerSec * pwfx->nBlockAlign;
    pwfx->cbSize = 0;
  }
  return pwfx;
}

HRESULT CMPAudioRenderer::CheckInputType(const CMediaType *pmt)
{
  return CheckMediaType(pmt);
}

HRESULT	CMPAudioRenderer::CheckMediaType(const CMediaType *pmt)
{
  HRESULT hr = S_OK;
  
  if (!pmt) 
    return E_INVALIDARG;
  
  Log("CheckMediaType");
  WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->Format();

  if (!pwfx) 
    return VFW_E_TYPE_NOT_ACCEPTED;

  if ((pmt->majortype	!= MEDIATYPE_Audio) ||
      (pmt->formattype != FORMAT_WaveFormatEx))
  {
    Log("CheckMediaType Not supported");
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  LogWaveFormat(pwfx, "CheckMediaType");

  if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
  {
    WAVEFORMATEXTENSIBLE* tmp = (WAVEFORMATEXTENSIBLE*)pwfx;
    
    DWORD channelMask5_1 = m_Settings.m_dwChannelMaskOverride_5_1;
    DWORD channelMask7_1 = m_Settings.m_dwChannelMaskOverride_7_1;

    if (tmp->Format.nChannels == 6 && channelMask5_1 > 0)
    {
      Log("CheckMediaType:: overriding 5.1 channel mask to %d", channelMask5_1);
      tmp->dwChannelMask = channelMask5_1;  
    }

    if (tmp->Format.nChannels == 8 && channelMask7_1 > 0)
    {
      Log("CheckMediaType:: overriding 7.1 channel mask to %d", channelMask7_1);
      tmp->dwChannelMask = channelMask7_1;  
    }
  }

  if (m_Settings.m_bUseTimeStretching)
  {
    hr = m_pSoundTouch->CheckFormat(pwfx);
    if (FAILED(hr))
      return hr;
  }

  if (m_pRenderDevice)
  {
    if (m_Settings.m_bEnableAC3Encoding)
    {
      WAVEFORMATEX *pRenderFormat = CreateWaveFormatForAC3(pwfx->nSamplesPerSec);
      hr = m_pRenderDevice->CheckFormat(pRenderFormat);
      SAFE_DELETE_WAVEFORMATEX(pRenderFormat);
    }
    else
    {
      hr = m_pRenderDevice->CheckFormat(pwfx);
    }
  }

  return hr;
}

void CMPAudioRenderer::AudioClock(UINT64& pTimestamp, UINT64& pQpc)
{
  if (m_pRenderDevice)
    m_pRenderDevice->AudioClock(pTimestamp, pQpc);
  //TRACE(_T("AudioClock query pos: %I64d qpc: %I64d"), pTimestamp, pQpc);
}

void CMPAudioRenderer::OnReceiveFirstSample(IMediaSample *pMediaSample)
{
  if (m_pRenderDevice)
    m_pRenderDevice->OnReceiveFirstSample(pMediaSample);
}

BOOL CMPAudioRenderer::ScheduleSample(IMediaSample *pMediaSample)
{
  REFERENCE_TIME rtSampleTime = 0;
  REFERENCE_TIME rtSampleEndTime = 0;
  REFERENCE_TIME rtTime = 0;

  // Is someone pulling our leg
  if (!pMediaSample) return FALSE;

  if (m_dRate >= 2.0 || m_dRate <= -2.0)
  {
    // Do not render Micey Mouse(tm) audio
    return TRUE;
  }

  m_dSampleCounter++;

  // Get the next sample due up for rendering.  If there aren't any ready
  // then GetNextSampleTimes returns an error.  If there is one to be done
  // then it succeeds and yields the sample times. If it is due now then
  // it returns S_OK other if it's to be done when due it returns S_FALSE
  HRESULT hr = GetSampleTimes(pMediaSample, &rtSampleTime, &rtSampleEndTime);
  if (FAILED(hr)) return FALSE;

  // Get media time
  m_pClock->GetTime(&rtTime);
  rtTime = rtTime - m_tStart;
  rtTime += m_pRenderDevice->Latency();
  
  long sampleLenght = pMediaSample->GetActualDataLength();

  UINT nFrames = sampleLenght / m_pWaveFileFormat->nBlockAlign;
  REFERENCE_TIME rtSampleDuration = nFrames * UNITS / m_pWaveFileFormat->nSamplesPerSec;
  REFERENCE_TIME rtLate = rtTime - rtSampleTime;  

  if (rtSampleTime - m_pRenderDevice->Latency() < 0)
  {
    // Preroll samples are ignored. During this time the device's audio
    // latency will be "eliminated". Preroll amount has been set to match the 
    // device's buffer latency. Unfortunately IsPreroll() is not working on
    // all source filters so we have to use own calculation
    
    //Log("preroll sample - rtTime: %.3f ms rtSampleTime: %.3f ms ", rtTime/10000.0, rtSampleTime/10000.0);
    
    m_rtNextSampleTime = rtSampleTime + rtSampleDuration;

    EXECUTE_ASSERT(SetEvent((HANDLE)m_RenderEvent));
    return TRUE;
  }

  // Try to keep the A/V sync when data has been dropped
  if ((abs(rtSampleTime - m_rtNextSampleTime) > MAX_SAMPLE_TIME_ERROR) && m_dSampleCounter > 1)
  {
    m_bDropSamples = true;
    Log("  Dropped audio data detected: diff: %.3f ms MAX_SAMPLE_TIME_ERROR: %.3f ms", ((double)rtSampleTime - (double)m_rtNextSampleTime) / 10000.0, (double)MAX_SAMPLE_TIME_ERROR / 10000.0);
  }
  else if (rtLate > rtSampleDuration)
  {
    m_bDropSamples = true;
  }
  else if(rtLate  <= rtSampleDuration && m_bDropSamples)
  {
    m_bDropSamples = false;
    Log("  Live stream position after ::Run has been reached");
  }

  m_rtNextSampleTime = rtSampleTime + rtSampleDuration;
  
  if(m_Settings.m_bLogSampleTimes)
    Log("  rtTime: %5.3f ms rtSampleTime: %5.3f ms diff: %5.3f ms size: %d",
      rtTime / 10000.0, rtSampleTime / 10000.0, (rtTime - rtSampleTime) / 10000.0, sampleLenght);

  // The whole timespan of the sample is late
  if( rtLate > rtSampleDuration && m_bDropSamples)
  {
    Log("  dropping whole sample - late: %.3f ms dur: %.3f ms", 
      rtLate / 10000.0, rtSampleDuration / 10000.0);

    pMediaSample->SetActualDataLength(0);

    // Triggers next sample to be scheduled
    EXECUTE_ASSERT(SetEvent((HANDLE)m_RenderEvent));
    return TRUE;
  }
  else if (m_bDropSamples && rtLate > 0)
  {
    long newLenght = m_pWaveFileFormat->nBlockAlign * ((rtSampleDuration - rtLate) * m_pWaveFileFormat->nSamplesPerSec / UNITS);
    long sampleLenght = pMediaSample->GetActualDataLength();
    
    // Just some sanity checks
    if( newLenght < 0 )
    {
      newLenght = 0;
    }
    else
    {
      newLenght = min(newLenght, sampleLenght);
    }

    Log("  dropping part of sample %d / %d bytes", newLenght, sampleLenght);
    pMediaSample->SetActualDataLength(newLenght);

    BYTE* sampleData = NULL;
    pMediaSample->GetPointer(&sampleData);
    if (sampleData)
    {
      // Discard the oldest sample data to match the start timestamp
      memmove(sampleData, sampleData + newLenght, newLenght);
    }
  }

  if (m_bFlushSamples)
  {
    CAutoLock cInterfaceLock(&m_InterfaceLock);
    CAutoLock cRenderThreadLock(&m_RenderThreadLock);

    m_pSoundTouch->BeginFlush();
    m_pSoundTouch->clear();
    m_pSoundTouch->EndFlush();
    m_bDropSamples = false; // stream is continuous from this point on  
    m_bFlushSamples = false;
  }
  
  if (m_dSampleCounter > 1 && !m_bDropSamples)
  {
    EXECUTE_ASSERT(SetEvent((HANDLE) m_RenderEvent));
    m_rtPrevSampleTime = rtSampleTime;
    return TRUE;
  }

  if (m_dRate <= 1.1)
  {
    // Discard all old data in queues
    if (m_bDropSamples)
    {
      m_bFlushSamples = true;
    }

    ASSERT(m_dwAdvise == 0);
    ASSERT(m_pClock);
    WaitForSingleObject((HANDLE)m_RenderEvent, 0);

    hr = m_pClock->AdviseTime((REFERENCE_TIME)m_tStart, rtSampleTime, (HEVENT)(HANDLE)m_RenderEvent, &m_dwAdvise);
    
    if (SUCCEEDED(hr)) return TRUE;
  }
  else
  {
    hr = DoRenderSample(pMediaSample);
  }

  m_rtPrevSampleTime = rtSampleTime;

  // We could not schedule the next sample for rendering despite the fact
  // we have a valid sample here. This is a fair indication that either
  // the system clock is wrong or the time stamp for the sample is duff
  ASSERT(m_dwAdvise == 0);

  return FALSE;
}

HRESULT	CMPAudioRenderer::DoRenderSample(IMediaSample *pMediaSample)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  return m_pRenderDevice->DoRenderSample(pMediaSample, m_dSampleCounter);
}


STDMETHODIMP CMPAudioRenderer::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
  if (riid == IID_IReferenceClock)
  {
    return GetInterface(static_cast<IReferenceClock*>(&m_Clock), ppv);
  }

  if (riid == IID_IAVSyncClock) 
  {
    return GetInterface(static_cast<IAVSyncClock*>(this), ppv);
  }

  if (riid == IID_IMediaSeeking) 
  {
    return GetInterface(static_cast<IMediaSeeking*>(this), ppv);
  }

  if (riid == IID_IBasicAudio)
  {
    return GetInterface(static_cast<IBasicAudio*>(m_pVolumeHandler), ppv);
  }

	return CBaseRenderer::NonDelegatingQueryInterface (riid, ppv);
}

HRESULT CMPAudioRenderer::SetMediaType(const CMediaType *pmt)
{
	if (!pmt) return E_POINTER;
  
  HRESULT hr = S_OK;
  Log("SetMediaType");

  WAVEFORMATEX* pwf = (WAVEFORMATEX*) pmt->Format();
  
  if (m_pRenderDevice)
  {
    if (m_Settings.m_bEnableAC3Encoding)
    {
      WAVEFORMATEX* pRenderFormat = CreateWaveFormatForAC3(pwf->nSamplesPerSec);
      m_pRenderDevice->SetMediaType(pRenderFormat);
      SAFE_DELETE_WAVEFORMATEX(pRenderFormat);
    }
    else
    {
      m_pRenderDevice->SetMediaType(pwf);
    }
  }

  SAFE_DELETE_WAVEFORMATEX(m_pWaveFileFormat);
  
  if (pwf)
  {
    hr = CopyWaveFormatEx(&m_pWaveFileFormat, pwf);
    if (FAILED(hr))
      return hr;

    if (m_pSoundTouch)
    {
      //m_pSoundTouch->setChannels(pwf->nChannels);
      hr = m_pSoundTouch->SetFormat(pwf);
      if (FAILED(hr))
      {
        Log("CMPAudioRenderer::SetMediaType: Format rejected by CMultiSoundTouch (0x%08x)", hr);
        LogWaveFormat(pwf, "SetMediaType");
        return hr;
      }

      m_pSoundTouch->setSampleRate(pwf->nSamplesPerSec);
      m_pSoundTouch->setTempoChange(0);
      m_pSoundTouch->setPitchSemiTones(0);

      // settings from Reclock - watch CPU usage when enabling these!
      /*bool usequickseek = false;
      bool useaafilter = false; //seems clearer without it
      int aafiltertaps = 56; //Def=32 doesnt matter coz its not used
      int seqms = 120; //reclock original is 82
      int seekwinms = 28; //reclock original is 28
      int overlapms = seekwinms; //reduces cutting sound if this is large
      int seqmslfe = 180; //larger value seems to preserve low frequencies better
      int seekwinmslfe = 42; //as percentage of seqms
      int overlapmslfe = seekwinmslfe; //reduces cutting sound if this is large

      m_pSoundTouch->setSetting(SETTING_USE_QUICKSEEK, usequickseek);
      m_pSoundTouch->setSetting(SETTING_USE_AA_FILTER, useaafilter);
      m_pSoundTouch->setSetting(SETTING_AA_FILTER_LENGTH, aafiltertaps);
      m_pSoundTouch->setSetting(SETTING_SEQUENCE_MS, seqms); 
      m_pSoundTouch->setSetting(SETTING_SEEKWINDOW_MS, seekwinms);
      m_pSoundTouch->setSetting(SETTING_OVERLAP_MS, overlapms);
      */
    }
  }

  return CBaseRenderer::SetMediaType (pmt);
}

HRESULT CMPAudioRenderer::CompleteConnect(IPin *pReceivePin)
{
  Log("CompleteConnect");

  HRESULT hr = S_OK;
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  
  hr = pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter == NULL) return E_FAIL;
  hr = pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  filterInfo.pGraph->Release();
  pinInfo.pFilter->Release();

  if (FAILED(hr)) 
    return E_FAIL;
  
  Log("CompleteConnect - audio decoder: %S", &filterInfo.achName);

  if (!m_pRenderDevice) return E_FAIL;

  if (SUCCEEDED(hr)) hr = CBaseRenderer::CompleteConnect(pReceivePin);
  if (SUCCEEDED(hr)) hr = m_pRenderDevice->CompleteConnect(pReceivePin);

  if (SUCCEEDED(hr)) Log("CompleteConnect Success");

  return hr;
}

STDMETHODIMP CMPAudioRenderer::Run(REFERENCE_TIME tStart)
{
  Log("Run");

  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  HRESULT	hr;

  if (m_State == State_Running) return NOERROR;

  m_pRenderDevice->Run(tStart);

  if (m_dRate >= 1.0 && m_pSoundTouch)
  {
    m_pSoundTouch->setRateChange((float)(m_dRate-1.0)*100);
  }
     
  return CBaseRenderer::Run(tStart);
}

STDMETHODIMP CMPAudioRenderer::Stop() 
{
  Log("Stop");

  CAutoLock cInterfaceLock(&m_InterfaceLock);

  if (m_pSoundTouch)
  {
    m_pSoundTouch->GetNextSample(NULL, true);
    m_pSoundTouch->BeginFlush();
    m_pSoundTouch->EndFlush();  
  }
  
  m_pRenderDevice->Stop(GetRealState());

  return CBaseRenderer::Stop(); 
};


STDMETHODIMP CMPAudioRenderer::Pause()
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);

  Log("Pause");

  FILTER_STATE state = GetRealState();

  m_pRenderDevice->Pause(state);

  m_dSampleCounter = 0;
  m_rtNextSampleTime = 0;
  m_rtPrevSampleTime = 0;

  return CBaseRenderer::Pause(); 
};


HRESULT CMPAudioRenderer::GetReferenceClockInterface(REFIID riid, void **ppv)
{
  HRESULT hr = S_OK;

  if (m_pReferenceClock)
  {
    return m_pReferenceClock->NonDelegatingQueryInterface(riid, ppv);
  }

  m_pReferenceClock = new CBaseReferenceClock (NAME("MP Audio Clock"), NULL, &hr);
	
  if (!m_pReferenceClock)
    return E_OUTOFMEMORY;

  m_pReferenceClock->AddRef();

  hr = SetSyncSource(m_pReferenceClock);
  if (FAILED(hr)) 
  {
    SetSyncSource(NULL);
    return hr;
  }

  return GetReferenceClockInterface(riid, ppv);
}

HRESULT CMPAudioRenderer::EndOfStream()
{
  Log("EndOfStream");

  // Do not stop the playback when end of stream is received. Source filter
  // will send the EndOfStream as soon as the file end is reached, but we
  // are still having samples in our queues that we still need to play. 
  // In worst case it could cause almost 10 seconds of the audio to be "eaten"
  // at the end the playback.

  return CBaseRenderer::EndOfStream();
}

HRESULT CMPAudioRenderer::BeginFlush()
{
  Log("BeginFlush");

  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  HRESULT hrBase = CBaseRenderer::BeginFlush(); 

  m_pRenderDevice->BeginFlush();

  if (m_pSoundTouch)
  {
    m_pSoundTouch->BeginFlush();
    m_pSoundTouch->clear();
  }

  return hrBase;
}

HRESULT CMPAudioRenderer::EndFlush()
{
  Log("EndFlush");

  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);
  
  m_dSampleCounter = 0;
  m_rtNextSampleTime = 0;
  m_rtPrevSampleTime = 0;

  if (m_pSoundTouch)
  {
    m_pSoundTouch->EndFlush();
  }

  m_pRenderDevice->EndFlush();

  return CBaseRenderer::EndFlush(); 
}

// TODO - implement TsReader side as well

/*
bool CMPAudioRenderer::CheckForLiveSouce()
{
  FILTER_INFO filterInfo;
  ZeroMemory(&filterInfo, sizeof(filterInfo));
  m_EVRFilter->QueryFilterInfo(&filterInfo); // This addref's the pGraph member

  CComPtr<IBaseFilter> pBaseFilter;

  HRESULT hr = filterInfo.pGraph->FindFilterByName(L"MediaPortal File Reader", &pBaseFilter);
  filterInfo.pGraph->Release();
}*/

// IAVSyncClock interface implementation

HRESULT CMPAudioRenderer::AdjustClock(DOUBLE pAdjustment)
{
  CAutoLock cAutoLock(&m_csResampleLock);
  
  if (m_Settings.m_bUseTimeStretching)
  {
    m_dAdjustment = pAdjustment;
    m_Clock.SetAdjustment(m_dAdjustment);
    if (m_pSoundTouch)
    {
      m_pSoundTouch->setTempo(m_dAdjustment * m_dBias);
    }
    return S_OK;
  }
  else
  {
    return S_FALSE;
  }
}

HRESULT CMPAudioRenderer::SetBias(DOUBLE pBias)
{
  CAutoLock cAutoLock(&m_csResampleLock);

  bool ret = S_FALSE;

  if (m_Settings.m_bUseTimeStretching)
  {
    Log("SetBias: %1.10f", pBias);

    if (pBias < m_Settings.m_dMinBias)
    {
      Log("   bias value too small - using 1.0");
      m_dBias = 1.0;
      ret = S_FALSE; 
    }
    else if(pBias > m_Settings.m_dMaxBias)
    {
      Log("   bias value too big - using 1.0");
      m_dBias = 1.0;
      ret = S_FALSE; 
    }
    else
    {
      m_dBias = pBias;
      ret = S_OK;  
    }
    
    m_Clock.SetBias(m_dBias);
    if (m_pSoundTouch)
    {
      m_pSoundTouch->setTempo(m_dAdjustment * m_dBias);
      Log("SetBias - updated SoundTouch tempo");
      // ret is not set since we want to be able to indicate the too big / small bias value	  
    }
    else
    {
      Log("SetBias - no SoundTouch avaible!");
      ret = S_FALSE;
    }
  }
  else
  {
    Log("SetBias: %1.10f - failed, time stretching is disabled", pBias);
    ret = S_FALSE;  
  }

  return ret;
}

HRESULT CMPAudioRenderer::GetBias(DOUBLE* pBias)
{
  CheckPointer(pBias, E_POINTER);
  *pBias = m_Clock.Bias();
  return S_OK;
}

HRESULT CMPAudioRenderer::GetMaxBias(DOUBLE *pMaxBias)
{
  CheckPointer(pMaxBias, E_POINTER);
  *pMaxBias = m_Settings.m_dMaxBias;
  return S_OK;
}

HRESULT CMPAudioRenderer::GetMinBias(DOUBLE *pMinBias)
{
  CheckPointer(pMinBias, E_POINTER);
  *pMinBias = m_Settings.m_dMinBias;
  return S_OK;
}
