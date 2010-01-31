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

#include "StdAfx.h"

#include <streams.h>
#include <atlbase.h>
#include <d3dx9.h>
#include <dvdmedia.h>
#include <mfapi.h>
#include <mferror.h>
#include <afxtempl.h> // CMap
#include <dwmapi.h>

#include "dshowhelper.h"
#include "evrcustompresenter.h"
#include "scheduler.h"
#include "timesource.h"
#include "statsrenderer.h"
#include "autoint.h"

void LogIID(REFIID riid)
{
  LPOLESTR str;
  LPSTR astr;
  StringFromIID(riid, &str);
  UnicodeToAnsi(str, &astr);
  Log("riid: %s", astr);
  CoTaskMemFree(str);
}


void LogGUID(REFGUID guid)
{
  LPOLESTR str;
  LPSTR astr;
  str = (LPOLESTR)CoTaskMemAlloc(200);
  StringFromGUID2(guid, str, 200);
  UnicodeToAnsi(str, &astr);
  Log("guid: %s", astr);
  CoTaskMemFree(str);
}

MPEVRCustomPresenter::MPEVRCustomPresenter(IVMR9Callback* pCallback, IDirect3DDevice9* direct3dDevice, HMONITOR monitor, IBaseFilter* EVRFilter, BOOL pIsWin7):
  m_refCount(1), 
  m_qScheduledSamples(NUM_SURFACES),
  m_EVRFilter(EVRFilter),
  m_bIsWin7(pIsWin7)
{
  timeBeginPeriod(1);
  if (m_pMFCreateVideoSampleFromSurface != NULL)
  {
    HRESULT hr;
    LogRotate();
    Log("----------v0.6---------------------------");
    m_hMonitor = monitor;
    m_pD3DDev = direct3dDevice;
    hr = m_pDXVA2CreateDirect3DDeviceManager9(&m_iResetToken, &m_pDeviceManager);
    if (FAILED(hr))
    {
      Log("Could not create DXVA2 Device Manager");
    }
    else 
    {
      m_pDeviceManager->ResetDevice(direct3dDevice, m_iResetToken);
    }
    m_pCallback                = pCallback;
    m_bEndStreaming            = FALSE;
    m_state                    = MP_RENDER_STATE_SHUTDOWN;
    m_bSchedulerRunning        = FALSE;
    m_fRate                    = 1.0f;
    m_iFreeSamples             = 0;
    m_nNextJitter              = 0;
    m_llLastPerf               = 0;
    m_fAvrFps                  = 0.0;
    m_rtTimePerFrame           = 0;
    m_llLastWorkerNotification = 0;
    m_bFrameSkipping           = true;
    m_bDVDMenu                 = false;
    m_bScrubbing               = false;
    m_fSeekRate                = m_fRate;
    memset(m_pllJitter,           0, sizeof(m_pllJitter));
    memset(m_pllSyncOffset,       0, sizeof(m_pllSyncOffset));
    memset(m_pllRasterSyncOffset, 0, sizeof(m_pllRasterSyncOffset));

    m_nNextSyncOffset       = 0;
    m_fJitterStdDev		      = 0.0;
    m_fSyncOffsetStdDev     = 0.0;
    m_fSyncOffsetAvr	      = 0.0;
    m_dD3DRefreshRate       = 0.0;
    m_dD3DRefreshCycle      = 0.0;
    m_dOptimumDisplayCycle  = 0.0;
    m_dCycleDifference      = 0.0;
    m_uSyncGlitches         = 0;
    m_rasterSyncOffset      = 0;
    m_dDetectedScanlineTime = 0;

    // sample time correction variables
    m_LastScheduledUncorrectedSampleTime  = -1;
    m_LastScheduledSampleTimeFP           = 0;
    m_DetectedFrameTimePos                = 0;
    m_DetectedFrameRate                   = 0;
    m_DetectedFrameTime                   = -1;
    m_DetectedFrameTimeStdDev             = 0;
    m_bCorrectedFrameTime                 = 0;    
    m_DetectedLock                        = false;

    m_pD3DDev->GetDisplayMode(0, &m_displayMode);

    m_bDrawStats = false;
  }
  
  if (m_pDwmEnableMMCSS)
  {
    HRESULT hr = m_pDwmEnableMMCSS(true);
    if (SUCCEEDED(hr)) 
    {
      Log("Enabling the Multimedia Class Schedule Service for DWM succeed");
    }
    else
    {
      Log("Enabling the Multimedia Class Schedule Servicer for DWM failed");
    }
  }
  m_pStatsRenderer = new StatsRenderer(this, m_pD3DDev);
}

void MPEVRCustomPresenter::SetFrameSkipping(bool onOff)
{
  Log("Evr Enable frame skipping:%d",onOff);
  m_bFrameSkipping = onOff;
}


void MPEVRCustomPresenter::EnableDrawStats(bool enable)
{
  // Reset stats when hiding them. This will easen up the troubleshooting / debugging
  if (m_bDrawStats && !enable)
  {
    ResetEVRStatCounters();
  }
  m_bDrawStats = enable;
}


void MPEVRCustomPresenter::ResetEVRStatCounters()
{
  m_bResetStats = true;
}


MPEVRCustomPresenter::~MPEVRCustomPresenter()
{
  if (m_pCallback != NULL)
  {
    m_pCallback->PresentImage(0, 0, 0, 0, 0, 0);
  }

  StopWorkers();
  ReleaseSurfaces();
  m_pMediaType.Release();
  m_pDeviceManager =  NULL;
  for (int i=0; i < NUM_SURFACES; i++)
  {
    m_vFreeSamples[i] = 0;
  }
  Log("Done");
}	


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetParameters(__RPC__out DWORD *pdwFlags, __RPC__out DWORD *pdwQueue)
{
  Log("GetParameters");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::Invoke(__RPC__in_opt IMFAsyncResult *pAsyncResult)
{
  Log("Invoke");
  return S_OK;
}


// IUnknown
HRESULT MPEVRCustomPresenter::QueryInterface(REFIID riid, void** ppvObject)
{
  HRESULT hr = E_NOINTERFACE;
  if (ppvObject == NULL)
  {
    hr = E_POINTER;
  }
  else if (riid == IID_IMFVideoDeviceID)
  {
    *ppvObject = static_cast<IMFVideoDeviceID*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFTopologyServiceLookupClient)
  {
    *ppvObject = static_cast<IMFTopologyServiceLookupClient*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFVideoPresenter)
  {
    *ppvObject = static_cast<IMFVideoPresenter*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFGetService)
  {
    *ppvObject = static_cast<IMFGetService*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IQualProp)
  {
    *ppvObject = static_cast<IQualProp*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFRateSupport)
  {
    *ppvObject = static_cast<IMFRateSupport*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFVideoDisplayControl)
  {
    *ppvObject = static_cast<IMFVideoDisplayControl*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IEVRTrustedVideoPlugin)
  {
    *ppvObject = static_cast<IEVRTrustedVideoPlugin*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFVideoPositionMapper)
  {
    *ppvObject = static_cast<IMFVideoPositionMapper*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IUnknown)
  {
    *ppvObject = static_cast<IUnknown*>(static_cast<IMFVideoDeviceID*>(this));
    AddRef();
    hr = S_OK;
  }
  else
  {
    LogIID(riid);
    *ppvObject = NULL;
    hr = E_NOINTERFACE;
  }
  CHECK_HR(hr, "QueryInterface failed")
  return hr;
}


ULONG MPEVRCustomPresenter::AddRef()
{
  return InterlockedIncrement(&m_refCount);
}


ULONG MPEVRCustomPresenter::Release()
{
  ULONG ret = InterlockedDecrement(&m_refCount);
  if (ret == 0)
  {
    Log("MPEVRCustomPresenter::Cleanup()");
    delete this;
  }
  return ret;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetSlowestRate(MFRATE_DIRECTION eDirection, BOOL fThin, __RPC__out float *pflRate)
{
  Log("GetSlowestRate");
  // There is no minimum playback rate, so the minimum is zero.
  *pflRate = 0;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetFastestRate(MFRATE_DIRECTION eDirection, BOOL fThin, __RPC__out float *pflRate)
{
  Log("GetFastestRate");
  float fMaxRate = 0.0f;

  // Get the maximum *forward* rate.
  fMaxRate = FLT_MAX;

  // For reverse playback, it's the negative of fMaxRate.
  if (eDirection == MFRATE_REVERSE)
  {
    fMaxRate = -fMaxRate;
  }

  *pflRate = fMaxRate;

  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::IsRateSupported(BOOL fThin, float flRate, __RPC__inout_opt float *pflNearestSupportedRate)
{
  Log("IsRateSupported");
  if (pflNearestSupportedRate != NULL)
  {
    *pflNearestSupportedRate = flRate;
  }
  return S_OK;
}


HRESULT MPEVRCustomPresenter::GetDeviceID(IID* pDeviceID)
{
  Log("GetDeviceID");
  if (pDeviceID == NULL)
  {
    return E_POINTER;
  }
  *pDeviceID = __uuidof(IDirect3DDevice9);
  return S_OK;
}


HRESULT MPEVRCustomPresenter::InitServicePointers(IMFTopologyServiceLookup *pLookup)
{
  Log("InitServicePointers");
  HRESULT hr = S_OK;
  DWORD cCount = 0;

  // just to make sure....
  ReleaseServicePointers();

  // Ask for the mixer
  cCount = 1;
  hr = pLookup->LookupService(
    MF_SERVICE_LOOKUP_GLOBAL,   // Not used
    0,                          // Reserved
    MR_VIDEO_MIXER_SERVICE,     // Service to look up
    __uuidof(IMFTransform),     // Interface to look up
    (void**)&m_pMixer,          // Receives the pointer.
    &cCount);                   // Number of pointers

  if (FAILED(hr))
  {
    Log("ERR: Could not get IMFTransform interface");
  }
  else 
  {
    Log("Found mixers: %d", cCount);
    ASSERT(cCount == 0 || cCount == 1);
  }

  // Ask for the clock
  cCount = 1;
  hr = pLookup->LookupService(
    MF_SERVICE_LOOKUP_GLOBAL,   // Not used
    0,                          // Reserved
    MR_VIDEO_RENDER_SERVICE,    // Service to look up
    __uuidof(IMFClock),         // Interface to look up
    (void**)&m_pClock,          // Receives the pointer.
    &cCount);                   // Number of pointers

  if (FAILED(hr))
  {
    Log("ERR: Could not get IMFClock interface");
  }
  else 
  {
    Log("Found clock: %d", cCount);
    ASSERT(cCount == 0 || cCount == 1);
  }

  // Ask for the event-sink
  cCount = 1;
  hr = pLookup->LookupService(
    MF_SERVICE_LOOKUP_GLOBAL,   // Not used
    0,                          // Reserved
    MR_VIDEO_RENDER_SERVICE,    // Service to look up
    __uuidof(IMediaEventSink),  // Interface to look up
    (void**)&m_pEventSink,      // Receives the pointer.
    &cCount);                   // Number of pointers

  if (FAILED(hr))
  {
    Log("ERR: Could not get IMediaEventSink interface");
  }
  else 
  {
    Log("Found event sink: %d", cCount);
    ASSERT(cCount == 0 || cCount == 1);
  }

  return S_OK;
}


HRESULT MPEVRCustomPresenter::ReleaseServicePointers()
{
  Log("ReleaseServicePointers");
  // on some channel changes it may happen that ReleaseServicePointers is called only after InitServicePointers 
  // is called to avoid this rare condition, we only release when not in state begin_streaming
  m_pMediaType.Release();
  m_pMixer.Release();
  m_pClock.Release();
  m_pEventSink.Release();
  return S_OK;
}


HRESULT MPEVRCustomPresenter::GetCurrentMediaType(IMFVideoMediaType** ppMediaType)
{
  Log("GetCurrentMediaType");
  HRESULT hr = S_OK;

  if (ppMediaType == NULL)
  {
    return E_POINTER;
  }

  if (m_pMediaType == NULL)
  {
    CHECK_HR(hr = MF_E_NOT_INITIALIZED, "MediaType is NULL");
  }

  CHECK_HR(hr = m_pMediaType->QueryInterface(__uuidof(IMFVideoMediaType), (void**)ppMediaType), "Query interface failed in GetCurrentMediaType");

  Log("GetCurrentMediaType done");
  return hr;
}


HRESULT MPEVRCustomPresenter::TrackSample(IMFSample *pSample)
{
  HRESULT hr = S_OK;
  IMFTrackedSample *pTracked = NULL;

  CHECK_HR(hr = pSample->QueryInterface(__uuidof(IMFTrackedSample), (void**)&pTracked), "Cannot get Interface IMFTrackedSample");
  CHECK_HR(hr = pTracked->SetAllocator(this, NULL), "SetAllocator failed");

  SAFE_RELEASE(pTracked);
  return hr;
}

HRESULT MPEVRCustomPresenter::GetTimeToSchedule(IMFSample* pSample, LONGLONG *phnsDelta)
{
  LONGLONG hnsPresentationTime = 0; // Target presentation time
  LONGLONG hnsTimeNow = 0;          // Current presentation time
  MFTIME   hnsSystemTime = 0;       // System time
  LONGLONG hnsDelta = 0;
  HRESULT  hr;

  if (m_pClock == NULL)
  {
    *phnsDelta = 0;
    return S_OK;
  }

  hr = pSample->GetSampleTime(&hnsPresentationTime);
  if (SUCCEEDED(hr))
  {
    if (hnsPresentationTime == 0)
    {
      // immediate presentation
      *phnsDelta = 0;
      return S_OK;
    }
    CHECK_HR(hr = m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime), "Could not get correlated time!");
  }
  else
  {
    Log("Could not get sample time from %p!", pSample);
    return hr;
  }

  // Calculate the amount of time until the sample's presentation time. A negative value means the sample is late.
  hnsDelta = hnsPresentationTime - hnsTimeNow;
 
  // if off more than a second and not scrubbing and not DVD Menu
  if (hnsDelta > 100000000 && !m_bScrubbing && !m_bDVDMenu)
  {
    Log("dangerous and unlikely time to schedule [%p]: %I64d. scheduled time: %I64d, now: %I64d",
      pSample, hnsDelta, hnsPresentationTime, hnsTimeNow);
  }
  LOG_TRACE("Due: %I64d, Calculated delta: %I64d (rate: %f)", hnsPresentationTime, hnsDelta, m_fRate);

  if (m_fRate != 1.0f && m_fRate != 0.0f)
  {
    *phnsDelta = (LONGLONG)((float)hnsDelta / m_fRate);
  }
  else
  {
    *phnsDelta = hnsDelta;
  }
  return hr;
}


HRESULT MPEVRCustomPresenter::GetAspectRatio(CComPtr<IMFMediaType> pType, int* piARX, int* piARY)
{
  HRESULT hr;
  UINT32 u32;
  if (SUCCEEDED(pType->GetUINT32(MF_MT_SOURCE_CONTENT_HINT, &u32)))
  {
    Log("Getting aspect ratio 'MediaFoundation style'");
    switch (u32)
    {
    case MFVideoSrcContentHintFlag_None:
      Log("Aspect ratio unknown");
    break;
    case MFVideoSrcContentHintFlag_16x9:
      Log("Source is 16:9 within 4:3!");
      *piARX = 16;
      *piARY = 9;
    break;
    case MFVideoSrcContentHintFlag_235_1:
      Log("Source is 2.35:1 within 16:9 or 4:3");
      *piARX = 47;
      *piARY = 20;
    break;
    default:
      Log("Unkown aspect ratio flag: %d", u32);
    break;
    }
  }
  else
  {
    // Try old DirectShow-Header, if above does not work
    Log("Getting aspect ratio 'DirectShow style'");
    AM_MEDIA_TYPE* pAMMediaType;
    CHECK_HR(
      hr = pType->GetRepresentation(FORMAT_VideoInfo2, (void**)&pAMMediaType),
      "Getting DirectShow Video Info failed");
    if (SUCCEEDED(hr))
    {
      VIDEOINFOHEADER2* vheader = (VIDEOINFOHEADER2*)pAMMediaType->pbFormat;
      *piARX = vheader->dwPictAspectRatioX;
      *piARY = vheader->dwPictAspectRatioY;
      pType->FreeRepresentation(FORMAT_VideoInfo2, (void*)pAMMediaType);
    }
    else
    {
      Log("Could not get directshow representation.");
    }
  }
  return hr;
}


HRESULT MPEVRCustomPresenter::SetMediaType(CComPtr<IMFMediaType> pType, BOOL* pbHasChanged)
{
  if (pType == NULL)
  {
    m_pMediaType.Release();
    return S_OK;
  }

  HRESULT hr = S_OK;
  LARGE_INTEGER u64;

  CHECK_HR(pType->GetUINT64(MF_MT_FRAME_SIZE, (UINT64*)&u64), "Getting Framesize failed!");

  MFVideoArea Area;
  UINT32 rSize;
  CHECK_HR(pType->GetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&Area, sizeof(Area), &rSize), "Failed to get MF_MT_GEOMETRIC_APERTURE");
  m_iVideoWidth = u64.HighPart;
  m_iVideoHeight = u64.LowPart;
  // use video size as default value for aspect ratios
  m_iARX = m_iVideoWidth;
  m_iARY = m_iVideoHeight;
  CHECK_HR(GetAspectRatio(pType, &m_iARX, &m_iARY), "Failed to get aspect ratio");
  Log("New format: %dx%d, Ratio: %d:%d",	m_iVideoWidth, m_iVideoHeight, m_iARX, m_iARY);

  GUID subtype;
  CHECK_HR(pType->GetGUID(MF_MT_SUBTYPE, &subtype), "Could not get subtype");
  LogGUID(subtype);
  if (m_pMediaType == NULL)
  {
    *pbHasChanged = TRUE;
  }
  else
  {
    BOOL doMatch;
    hr = m_pMediaType->Compare(pType, MF_ATTRIBUTES_MATCH_ALL_ITEMS, &doMatch);
    if (SUCCEEDED(hr))
    {
      *pbHasChanged = !doMatch;
    }
    else
    {
      hr = S_OK;
      Log("Could not compare media type to old media type. assuming a change (0x%x)", hr);
      *pbHasChanged = TRUE;
    }
  }
  m_pMediaType = pType;
  if (!*pbHasChanged)
  {
    Log("Detected same media type as last one.");
  }
  return S_OK;
}


void MPEVRCustomPresenter::ReAllocSurfaces()
{
  Log("ReallocSurfaces");
  //make sure both threads are paused
  CAutoLock wLock(&m_workerParams.csLock);
  CAutoLock sLock(&m_schedulerParams.csLock);
  ReleaseSurfaces();

  // set the presentation parameters
  D3DPRESENT_PARAMETERS d3dpp;
  ZeroMemory(&d3dpp, sizeof(d3dpp));
  d3dpp.BackBufferWidth = m_iVideoWidth;
  d3dpp.BackBufferHeight = m_iVideoHeight;
  d3dpp.BackBufferCount = 1;
  // TODO check media type for correct format!
  d3dpp.BackBufferFormat = D3DFMT_X8R8G8B8;
  d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
  d3dpp.Windowed = true;
  d3dpp.EnableAutoDepthStencil = false;
  d3dpp.AutoDepthStencilFormat = D3DFMT_X8R8G8B8;
  d3dpp.FullScreen_RefreshRateInHz = D3DPRESENT_RATE_DEFAULT;
  d3dpp.PresentationInterval = D3DPRESENT_INTERVAL_ONE;

  HANDLE hDevice;
  IDirect3DDevice9* pDevice;
  CHECK_HR(m_pDeviceManager->OpenDeviceHandle(&hDevice), "Cannot open device handle");
  CHECK_HR(m_pDeviceManager->LockDevice(hDevice, &pDevice, TRUE), "Cannot lock device");
  HRESULT hr;
  Log("Textures will be %dx%d", m_iVideoWidth, m_iVideoHeight);
  for (int i = 0; i < NUM_SURFACES; i++)
  {
    hr = pDevice->CreateTexture(m_iVideoWidth, m_iVideoHeight, 1,
      D3DUSAGE_RENDERTARGET, D3DFMT_X8R8G8B8, D3DPOOL_DEFAULT,
      &textures[i], NULL);
    if (FAILED(hr))
    {
      Log("Could not create offscreen surface. Error 0x%x", hr);
    }
    CHECK_HR(textures[i]->GetSurfaceLevel(0, &surfaces[i]), "Could not get surface from texture");

    hr = m_pMFCreateVideoSampleFromSurface(surfaces[i], &samples[i]);
    if (FAILED(hr))
    {
      Log("CreateVideoSampleFromSurface failed: 0x%x", hr);
      return;
    }
    Log("Adding sample: 0x%x", samples[i]);
    m_vFreeSamples[i] = samples[i];
  }
  m_iFreeSamples = NUM_SURFACES;
  CHECK_HR(m_pDeviceManager->UnlockDevice(hDevice, FALSE), "failed: Unlock device");
  Log("Releasing device: %d", pDevice->Release());
  CHECK_HR(m_pDeviceManager->CloseDeviceHandle(hDevice), "failed: CloseDeviceHandle");

  m_pVideoTexture = NULL;
  m_pVideoSurface = NULL;

  hr = m_pD3DDev->CreateTexture(m_iVideoWidth, m_iVideoHeight, 1, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pVideoTexture, NULL);
  if (FAILED(hr))
  {
    return;
  }

  hr = m_pVideoTexture->GetSurfaceLevel(0, &m_pVideoSurface);
  if (FAILED(hr))
  {
    return;
  }

  hr = m_pD3DDev->ColorFill(m_pVideoSurface, NULL, 0);
  
  m_pStatsRenderer->VideSizeChanged();

  Log("ReallocSurfaces done");
}


HRESULT MPEVRCustomPresenter::CreateProposedOutputType(IMFMediaType* pMixerType, IMFMediaType** pType)
{
  HRESULT hr;
  LARGE_INTEGER i64Size;

  hr = m_pMFCreateMediaType(pType);
  if (SUCCEEDED(hr))
  {
    CHECK_HR(hr = pMixerType->CopyAllItems(*pType), "failed: CopyAllItems. Could not clone media type");
    if (SUCCEEDED(hr))
    {
      Log("Successfully cloned media type");
    }
    (*pType)->SetUINT32(MF_MT_PAN_SCAN_ENABLED, 0);

    i64Size.HighPart = 800;
    i64Size.LowPart	 = 600;

    i64Size.HighPart = 1;
    i64Size.LowPart  = 1;

    CComPtr<IMFVideoMediaType> pVideoMediaType;

    AM_MEDIA_TYPE *pAMMedia = NULL;
    MFVIDEOFORMAT *videoFormat = NULL;

    CHECK_HR(pMixerType->GetRepresentation(FORMAT_MFVideoFormat, (void**)&pAMMedia), "pMixerType->GetRepresentation failed!");
    videoFormat = (MFVIDEOFORMAT*)pAMMedia->pbFormat;
    hr = m_pMFCreateVideoMediaType(videoFormat, &pVideoMediaType);

    if (hr == 0 && videoFormat->videoInfo.FramesPerSecond.Numerator != 0)
    {
      m_rtTimePerFrame = (10000000I64*videoFormat->videoInfo.FramesPerSecond.Denominator)/videoFormat->videoInfo.FramesPerSecond.Numerator;
      Log("Time Per Frame: %I64d", m_rtTimePerFrame);
      // HD
      if (videoFormat->videoInfo.dwHeight >= 720 || videoFormat->videoInfo.dwWidth >= 1280)
      {
        Log("Setting MFVideoTransferMatrix_BT709");
        (*pType)->SetUINT32(MF_MT_YUV_MATRIX, MFVideoTransferMatrix_BT709);
      }
      else // SD
      {
        Log("Setting MFVideoTransferMatrix_BT601");
        (*pType)->SetUINT32(MF_MT_YUV_MATRIX, MFVideoTransferMatrix_BT601);
      }
    }
    else
    {
      Log("Setting MFVideoTransferMatrix_BT709 (m_pMFCreateVideoMediaType failed, assuming HD)");
      (*pType)->SetUINT32(MF_MT_YUV_MATRIX, MFVideoTransferMatrix_BT709);
    }

    if (m_rtTimePerFrame == 0)
    {
      // if fps information is not provided use default (workaround for possible bugs)
      Log("No time per frame available usinf default: %d", DEFAULT_FRAME_TIME);
      m_rtTimePerFrame = DEFAULT_FRAME_TIME;
    }

    CHECK_HR((*pType)->GetUINT64(MF_MT_FRAME_SIZE, (UINT64*)&i64Size.QuadPart), "Failed to get MF_MT_FRAME_SIZE");
    Log("Frame size: %dx%d", i64Size.HighPart, i64Size.LowPart);

    MFVideoArea Area;
    UINT32 rSize;
    ZeroMemory(&Area, sizeof(MFVideoArea));
    // TODO get the real screen size, and calculate area corresponding to the given aspect ratio
    Area.Area.cx = min(800, i64Size.HighPart);
    Area.Area.cy = min(450, i64Size.LowPart);
    // for hardware scaling, use the following line:
    //(*pType)->SetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&Area, sizeof(MFVideoArea));
    CHECK_HR((*pType)->GetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&Area, sizeof(Area), &rSize), "Failed to get MF_MT_GEOMETRIC_APERTURE");
    Log("Aperture size: %x:%x, %dx%d", Area.OffsetX.value, Area.OffsetY.value, Area.Area.cx, Area.Area.cy);

    (*pType)->SetUINT32(MF_MT_VIDEO_NOMINAL_RANGE, MFNominalRange_0_255);
  }
  return hr;
}


HRESULT MPEVRCustomPresenter::LogOutputTypes()
{
  Log("--Dumping output types----");
  HRESULT hr = S_OK;
  BOOL fFoundMediaType = FALSE;

  CComPtr<IMFMediaType> pMixerType;
  CComPtr<IMFMediaType> pType;

  if (!m_pMixer)
  {
    return MF_E_INVALIDREQUEST;
  }

  // Loop through all of the mixer's proposed output types.
  DWORD iTypeIndex = 0;
  while (!fFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
  {
    pMixerType.Release();
    pType.Release();
    Log("Testing media type...");

    // Step 1. Get the next media type supported by mixer.
    hr = m_pMixer->GetOutputAvailableType(0, iTypeIndex++, &pMixerType);
    if (FAILED(hr))
    {
      if (hr != MF_E_NO_MORE_TYPES)
      {
        Log("stopping, hr=0x%x!", hr);
        break;
      }
    }
    int arx, ary;
    GetAspectRatio(pMixerType, &arx, &ary);
    Log("Aspect ratio: %d:%d", arx, ary);
    UINT32 interlaceMode;
    pMixerType->GetUINT32(MF_MT_INTERLACE_MODE, &interlaceMode);

    Log("Interlace mode: %d", interlaceMode);
    GUID subtype;
    CHECK_HR(pMixerType->GetGUID(MF_MT_SUBTYPE, &subtype), "Could not get subtype");
    LogGUID(subtype);
  }
  Log("--Dumping output types done----");
  return S_OK;
}


HRESULT MPEVRCustomPresenter::RenegotiateMediaOutputType()
{
  CAutoLock wLock(&m_workerParams.csLock);
  CAutoLock sLock(&m_schedulerParams.csLock);
  Log("RenegotiateMediaOutputType");
  HRESULT hr = S_OK;
  BOOL fFoundMediaType = FALSE;

  CComPtr<IMFMediaType> pMixerType;
  CComPtr<IMFMediaType> pType;

  if (!m_pMixer)
  {
    return MF_E_INVALIDREQUEST;
  }

  // Loop through all of the mixer's proposed output types.
  DWORD iTypeIndex = 0;
  while (!fFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
  {
    pMixerType.Release();
    pType.Release();
    Log("Testing media type...");

    // Step 1. Get the next media type supported by mixer.
    hr = m_pMixer->GetOutputAvailableType(0, iTypeIndex++, &pMixerType);
    if (FAILED(hr))
    {
      Log("ERR: Cannot find usable media type!");
      break;
    }

    // Step 2. Check if we support this media type.
    if (SUCCEEDED(hr))
    {
      hr = S_OK; //IsMediaTypeSupported(pMixerType);
    }

    // Step 3. Adjust the mixer's type to match our requirements.
    if (SUCCEEDED(hr))
    {
      // Create a clone of the suggested outputtype
      hr = CreateProposedOutputType(pMixerType, &pType);
    }

    // Step 4. Check if the mixer will accept this media type.
    if (SUCCEEDED(hr))
    {
      hr = m_pMixer->SetOutputType(0, pType, MFT_SET_TYPE_TEST_ONLY);
    }

    // Step 5. Try to set the media type on ourselves.
    if (SUCCEEDED(hr))
    {
      Log("New media type successfully negotiated!");
      BOOL bHasChanged;
      hr = SetMediaType(pType, &bHasChanged);
      if (SUCCEEDED(hr))
      {
        if (bHasChanged)
        {
          ReAllocSurfaces();
        }
      }
      else
      {
        Log("ERR: Could not set media type on self: 0x%x!", hr);
      }
    }

    // Step 6. Set output media type on mixer.
    if (SUCCEEDED(hr))
    {
      Log("Setting media type on mixer");
      hr = m_pMixer->SetOutputType(0, pType, 0);

      // If something went wrong, clear the media type.
      if (FAILED(hr))
      {
        Log("Could not set output type: 0x%x", hr);
        SetMediaType(NULL, NULL);
      }
    }

    if (SUCCEEDED(hr))
    {
      fFoundMediaType = TRUE;
    }
  }

  return hr;
}


HRESULT MPEVRCustomPresenter::GetFreeSample(IMFSample** ppSample)
{
  TIME_LOCK(&m_lockSamples, 50000, "GetFreeSample");
  LOG_TRACE("Trying to get free sample, size: %d", m_iFreeSamples);
  if (m_iFreeSamples == 0)
  {
    return E_FAIL;
  }
  m_iFreeSamples--;
  *ppSample = m_vFreeSamples[m_iFreeSamples];
  m_vFreeSamples[m_iFreeSamples] = NULL;

  return S_OK;
}


void MPEVRCustomPresenter::Flush()
{
  CAutoLock sLock(&m_lockSamples);
  CAutoLock ssLock(&m_lockScheduledSamples);
  if (m_qScheduledSamples.Count() > 0 && !m_bDVDMenu && !m_bScrubbing)
  {
    Log("Flushing: size=%d", m_qScheduledSamples.Count());
    while (m_qScheduledSamples.Count() > 0)
    {
      IMFSample* pSample = PeekSample();
      if (pSample != NULL)
      {
        PopSample();
        ReturnSample(pSample, FALSE);
      }
    }
  }

  m_LastScheduledUncorrectedSampleTime = -1;
  m_bFlush = FALSE;
}


void MPEVRCustomPresenter::ReturnSample(IMFSample* pSample, BOOL tryNotify)
{
  TIME_LOCK(&m_lockSamples, 50000, "ReturnSample")
  LOG_TRACE("Sample returned: now having %d samples", m_iFreeSamples+1);
  m_vFreeSamples[m_iFreeSamples++] = pSample;
  if (m_qScheduledSamples.Count() == 0)
  {
    LOG_TRACE("No scheduled samples, queue was empty -> todo, CheckForEndOfStream()");
    CheckForEndOfStream();
  }
  if (tryNotify && m_iFreeSamples == 1 && m_bInputAvailable)
  {
    NotifyWorker();
  }
}


HRESULT MPEVRCustomPresenter::PresentSample(IMFSample* pSample)
{
  HRESULT hr = S_OK;
  IMFMediaBuffer* pBuffer = NULL;
  IDirect3DSurface9* pSurface = NULL;
  LOG_TRACE("Presenting sample");
  // Get the buffer from the sample.
  CHECK_HR(hr = pSample->GetBufferByIndex(0, &pBuffer), "failed: GetBufferByIndex");

  CHECK_HR(hr = MyGetService(
    pBuffer, 
    MR_BUFFER_SERVICE, 
    __uuidof(IDirect3DSurface9), 
    (void**)&pSurface),
    "failed: MyGetService");

  if (pSurface)
  {
    // Calculate offset to scheduled time
    m_iFramesDrawn++;
    if (m_pClock != NULL)
    {
      LONGLONG hnsTimeNow, hnsSystemTime, hnsTimeScheduled;
      m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime);

      pSample->GetSampleTime(&hnsTimeScheduled);
      if (hnsTimeScheduled > 0)
      {
			  m_pCallback->SetSampleTime(hnsTimeScheduled);
      }
    }
    // Present the swap surface
    LOG_TRACE("Painting");
    LONGLONG then = GetCurrentTimestamp();
    CHECK_HR(hr = Paint(pSurface), "failed: Paint");
    LONGLONG diff = GetCurrentTimestamp() - then;
    LOG_TRACE("Paint() latency: %.2f ms", (double)diff/10000);
  }

  SAFE_RELEASE(pBuffer);
  SAFE_RELEASE(pSurface);
  if (hr == D3DERR_DEVICELOST || hr == D3DERR_DEVICENOTRESET)
  {
    // Failed because the device was lost.
    Log("D3DDevice was lost!");
  }
  return hr;
}


BOOL MPEVRCustomPresenter::CheckForInput()
{
  int counter;
  ProcessInputNotify(&counter);
  return counter != 0;
}


bool MPEVRCustomPresenter::ImmediateCheckForInput()
{
  int counter;
  CAutoLock lock(&m_workerParams.csLock);
  ProcessInputNotify(&counter);
  return counter != 0;
}


void MPEVRCustomPresenter::LogStats()
{
}

HRESULT MPEVRCustomPresenter::CheckForScheduledSample(REFERENCE_TIME *pNextSampleTime, REFERENCE_TIME hnsLastSleepTime)
{
  HRESULT hr = S_OK;
  LogStats();
  LOG_TRACE("Checking for scheduled sample (size: %d)", m_qScheduledSamples.Count());
  *pNextSampleTime = 0;
  if (m_bFlush)
  {
    Flush();
    return S_OK;
  }

  while (m_qScheduledSamples.Count() > 0)
  {
    // don't process frame in paused mode during normal playbck
    if (m_state == MP_RENDER_STATE_PAUSED && !m_bScrubbing && !m_bDVDMenu) 
    {
      break;
    }

    IMFSample* pSample = PeekSample();
    if (pSample == NULL)
    {
      break;
    }
  
    // get scheduled time, if none is available the sample will be presented immediately
    CHECK_HR(hr = GetTimeToSchedule(pSample, pNextSampleTime), "Couldn't get time to schedule!");
    if (FAILED(hr))
    {
      *pNextSampleTime = 0;
    }
    LOG_TRACE("Time to schedule: %I64d", *pNextSampleTime);

    // If we are ahead up to 1/4 of a frame present sample as the vsync will be waited for anyway
    if (*pNextSampleTime > (m_rtTimePerFrame/4))
    {
      break;
    }
    PopSample();
    
    // Drop late frames when frame skipping is enabled during normal playback
    if (*pNextSampleTime < 0 && m_bFrameSkipping && !m_bDVDMenu && !m_bScrubbing)
    {
      m_iFramesDropped++;
      Log("Dropping frame, behind %.2f ms, last sleep time %.2f ms.", (double)-*pNextSampleTime/10000, (double)hnsLastSleepTime/10000);
      // Notify EVR of late sample
      if( m_pEventSink )
      {
        LONGLONG sampleLatency = -*pNextSampleTime;
        m_pEventSink->Notify(EC_SAMPLE_LATENCY, (LONG_PTR)&sampleLatency, 0);
        LOG_TRACE("Sample Latency: %I64d", sampleLatency);
      }
    }
    else 
    {
      CHECK_HR(PresentSample(pSample), "PresentSample failed");
    }
    *pNextSampleTime = 0;
    ReturnSample(pSample, TRUE);
  }
  return hr;
}


void MPEVRCustomPresenter::StartWorkers()
{
  CAutoLock lock(this);
  if (m_bSchedulerRunning)
  {
    return;
  }
  StartThread(&m_hScheduler, &m_schedulerParams, SchedulerThread, &m_uSchedulerThreadId, THREAD_PRIORITY_TIME_CRITICAL);
  StartThread(&m_hWorker, &m_workerParams, WorkerThread, &m_uWorkerThreadId, THREAD_PRIORITY_ABOVE_NORMAL);
  m_bSchedulerRunning = TRUE;
}


void MPEVRCustomPresenter::StopWorkers()
{
  Log("Stopping workers...");
  CAutoLock lock(this);
  Log("Threads running : %s", m_bSchedulerRunning?"TRUE":"FALSE");
  if (!m_bSchedulerRunning)
  {
    return;
  }
  EndThread(m_hScheduler, &m_schedulerParams);
  EndThread(m_hWorker, &m_workerParams);
  m_bSchedulerRunning = FALSE;
}


void MPEVRCustomPresenter::StartThread(PHANDLE handle, SchedulerParams* pParams, UINT(CALLBACK *ThreadProc)(void*), UINT* threadId, int priority)
{
  Log("Starting thread!");
  pParams->pPresenter = this;
  pParams->bDone = FALSE;

  *handle = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, pParams, 0, threadId);
  Log("Started thread. id: 0x%x (%d), handle: 0x%x", *threadId, *threadId, *handle);
  SetThreadPriority(*handle, priority);
}


void MPEVRCustomPresenter::EndThread(HANDLE hThread, SchedulerParams* params)
{
  Log("Ending thread 0x%x, 0x%x", hThread, params);
  params->csLock.Lock();
  Log("Got lock.");
  params->bDone = TRUE;
  Log("Notifying thread...");
  params->eHasWork.Set();
  Log("Set done.");
  params->csLock.Unlock();
  Log("Waiting for thread to end...");
  WaitForSingleObject(hThread, INFINITE);
  Log("Waiting done");
  CloseHandle(hThread);
}


void MPEVRCustomPresenter::NotifyThread(SchedulerParams* params)
{
  if (m_bSchedulerRunning)
  {
    params->eHasWork.Set();
  }
  else 
  {
    Log("Scheduler is already shut down");
  }
}


void MPEVRCustomPresenter::NotifyScheduler()
{
  LOG_TRACE("NotifyScheduler()");
  NotifyThread(&m_schedulerParams);
}


void MPEVRCustomPresenter::NotifyWorker()
{
  LOG_TRACE("NotifyWorker()");
  m_llLastWorkerNotification = GetCurrentTimestamp();
  NotifyThread(&m_workerParams);
}


BOOL MPEVRCustomPresenter::PopSample()
{
  CAutoLock lock(&m_lockScheduledSamples);
  LOG_TRACE("Removing scheduled sample, size: %d", m_qScheduledSamples.Count());
  if (m_qScheduledSamples.Count() > 0)
  {
    m_qScheduledSamples.Get();
    return TRUE;
  }
  return FALSE;
}


IMFSample* MPEVRCustomPresenter::PeekSample()
{
  CAutoLock lock(&m_lockScheduledSamples);
  if (m_qScheduledSamples.Count() == 0)
  {
    Log("ERR: PeekSample: empty queue!");
    return NULL;
  }
  return m_qScheduledSamples.Peek();
}


void MPEVRCustomPresenter::ScheduleSample(IMFSample* pSample)
{
  CAutoLock lock(&m_lockScheduledSamples);
  LOG_TRACE("Scheduling Sample, size: %d", m_qScheduledSamples.Count());

  CorrectSampleTime(pSample);

  DWORD hr;
  LONGLONG nextSampleTime;
  CHECK_HR(hr = GetTimeToSchedule(pSample, &nextSampleTime), "Couldn't get time to schedule!");
  if (SUCCEEDED(hr))
  {
    // consider 5 ms "just-in-time" for log-length's sake
    if (nextSampleTime < -50000 && !m_bDVDMenu && !m_bScrubbing)
    {
      Log("Scheduling sample from the past (%.2f ms, last call to NotifyWorker: %.2f ms)", 
        (double)-nextSampleTime/10000, (GetCurrentTimestamp()-(double)m_llLastWorkerNotification)/10000);
    }
  }

  m_qScheduledSamples.Put(pSample);
  if (m_qScheduledSamples.Count() >= 1)
  {
    NotifyScheduler();
  }
}


BOOL MPEVRCustomPresenter::CheckForEndOfStream()
{
  if (!m_bEndStreaming)
  {
    return FALSE;
  }
  // samples pending
  if (m_qScheduledSamples.Count() > 0)
  {
    return FALSE;
  }
  if (m_pEventSink)
  {
    Log("Sending completion message");
    m_pEventSink->Notify(EC_COMPLETE, (LONG_PTR)S_OK, 0);
  }
  m_bEndStreaming = FALSE;
  return TRUE;
}


HRESULT MPEVRCustomPresenter::ProcessInputNotify(int* samplesProcessed)
{
  LOG_TRACE("ProcessInputNotify");
  HRESULT hr = S_OK;
  *samplesProcessed = 0;
  if (m_pClock != NULL)
  {
    MFCLOCK_STATE state;
    m_pClock->GetState(0, &state);
    if (state == MFCLOCK_STATE_PAUSED && !m_bScrubbing)
    {
      Log("Should not be processing data in pause mode");
      m_bInputAvailable = FALSE;
      return S_OK;
    }
  }
  else 
  {
    return S_OK;
  }
  // try to process as many samples as possible:
  BOOL bhasMoreSamples = true;
  m_bInputAvailable = FALSE;
  do {
    IMFSample* sample;
    hr = GetFreeSample(&sample);
    if (FAILED(hr))
    {
      m_bInputAvailable = TRUE;
      // double-checked locking, in case someone freed a sample between the above 2 steps and we would miss notification
      hr = GetFreeSample(&sample);
      if (FAILED(hr))
      {
        LOG_TRACE("Still more input available");
        return S_OK;
      }
      m_bInputAvailable = FALSE;
    }

    LONGLONG timeBeforeMixer;
    LONGLONG systemTime;
    m_pClock->GetCorrelatedTime(0, &timeBeforeMixer, &systemTime);

    if (m_pMixer == NULL)
    {
      return E_POINTER;
    }
    DWORD dwStatus;
    MFT_OUTPUT_DATA_BUFFER outputSamples[1];
    outputSamples[0].dwStreamID = 0;
    outputSamples[0].dwStatus = 0;
    outputSamples[0].pSample = sample;
    outputSamples[0].pEvents = NULL;
    hr = m_pMixer->ProcessOutput(0, 1, outputSamples, &dwStatus);
    SAFE_RELEASE(outputSamples[0].pEvents);
    if (SUCCEEDED(hr))
    {
      LONGLONG sampleTime;
      LONGLONG timeAfterMixer;
      sample->GetSampleTime(&sampleTime);

      *samplesProcessed++;

      m_pClock->GetCorrelatedTime(0, &timeAfterMixer, &systemTime);

      LONGLONG mixerLatency = timeAfterMixer - timeBeforeMixer;
      if (m_pEventSink)
      {
        m_pEventSink->Notify(EC_PROCESSING_LATENCY, (LONG_PTR)&mixerLatency, 0);
        LOG_TRACE("Mixer Latency: %I64d", mixerLatency);
      }
      ScheduleSample(sample);
    }
    else 
    {
      ReturnSample(sample, FALSE);
      switch (hr)
      {
      case MF_E_TRANSFORM_NEED_MORE_INPUT:
        // we are done for now
        hr = S_OK;
        bhasMoreSamples = false;
        LOG_TRACE("Need more input...");
        CheckForEndOfStream();
      break;

      case MF_E_TRANSFORM_STREAM_CHANGE:
        Log("Unhandled: transform_stream_change");
      break;

      case MF_E_TRANSFORM_TYPE_NOT_SET:
        // no errors, just infos why it didn't succeed
        Log("ProcessOutput: change of type");
        bhasMoreSamples = FALSE;
        hr = RenegotiateMediaOutputType();
      break;

      default:
        Log("ProcessOutput failed: 0x%x", hr);
        break;
      }
      return hr;
    }
  } while (bhasMoreSamples);
  return hr;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::ProcessMessage(MFVP_MESSAGE_TYPE eMessage, ULONG_PTR ulParam)
{
  HRESULT hr = S_OK;
  LOG_TRACE("Processmessage: %d, %p", eMessage, ulParam);

  switch (eMessage)
  {
    case MFVP_MESSAGE_FLUSH:
      // The presenter should discard any pending samples.
      LOG_TRACE("ProcessMessage MFVP_MESSAGE_FLUSH");
      // Delegate to avoid a weird deadlock with application-idle handler Flush();
      m_bFlush = TRUE;
      NotifyScheduler();
    break;

    case MFVP_MESSAGE_INVALIDATEMEDIATYPE:
      // The mixer's output format has changed. The EVR will initiate format negotiation.
      Log("ProcessMessage MFVP_MESSAGE_INVALIDATEMEDIATYPE");
      hr = RenegotiateMediaOutputType();
    break;

    case MFVP_MESSAGE_PROCESSINPUTNOTIFY:
      // One input stream on the mixer has received a new sample.
      LOG_TRACE("ProcessMessage MFVP_MESSAGE_PROCESSINPUTNOTIFY");
      if (!ImmediateCheckForInput())
      {
        NotifyWorker();
      }
    break;

    case MFVP_MESSAGE_BEGINSTREAMING:
      // The EVR switched from stopped to paused. The presenter should allocate resources.
      Log("ProcessMessage MFVP_MESSAGE_BEGINSTREAMING");
      m_bEndStreaming = FALSE;
      m_state = MP_RENDER_STATE_STARTED;
      StartWorkers();

      // TODO add 2nd monitor support
      ResetTraceStats();
      EstimateRefreshTimings();
    break;

    case MFVP_MESSAGE_ENDSTREAMING:
      // The EVR switched from running or paused to stopped. The presenter should free resources.
      Log("ProcessMessage MFVP_MESSAGE_ENDSTREAMING");
      m_state = MP_RENDER_STATE_STOPPED;
    break;

    case MFVP_MESSAGE_ENDOFSTREAM:
      // All streams have ended. The ulParam parameter is not used and should be zero.
      Log("ProcessMessage MFVP_MESSAGE_ENDOFSTREAM");
      m_bEndStreaming = TRUE;
      CheckForEndOfStream();
    break;

    case MFVP_MESSAGE_STEP:
      // Requests a frame step. The lower DWORD of the ulParam parameter contains the number of frames to step. 
      // If the value is N, the presenter should skip N –1 frames and display the N th frame. When that frame 
      // has been displayed, the presenter should send an EC_STEP_COMPLETE event to the EVR. If the presenter 
      // is not paused when it receives this message, it should return MF_E_INVALIDREQUEST.
      Log("ProcessMessage MFVP_MESSAGE_STEP");
    break;

    case MFVP_MESSAGE_CANCELSTEP:
      // Cancels a frame step.
      Log("ProcessMessage MFVP_MESSAGE_CANCELSTEP");
    break;

    default:
      Log("ProcessMessage Unknown: %d", eMessage);
    break;
  }

  if (FAILED(hr))
  {
    Log("ProcessMessage failed with 0x%x", hr);
  }

  LOG_TRACE("ProcessMessage done");
  return hr;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockStart(MFTIME hnsSystemTime, LONGLONG llClockStartOffset)
{
  Log("OnClockStart");
  m_state = MP_RENDER_STATE_STARTED;
  ResetTraceStats();
  Flush();
  NotifyWorker();
  NotifyScheduler();
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockStop(MFTIME hnsSystemTime)
{
  Log("OnClockStop");
  m_state = MP_RENDER_STATE_STOPPED;
  Flush();
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockPause(MFTIME hnsSystemTime)
{
  Log("OnClockPause");
  m_state = MP_RENDER_STATE_PAUSED;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockRestart(MFTIME hnsSystemTime)
{
  Log("OnClockRestart");
  m_state = MP_RENDER_STATE_STARTED;
  NotifyScheduler();
  NotifyWorker();
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockSetRate(MFTIME hnsSystemTime, float flRate)
{
  Log("OnClockSetRate: %f", flRate);
  m_fRate = flRate;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetService(REFGUID guidService, REFIID riid, LPVOID *ppvObject)
{
  Log("GetService");
  LogGUID(guidService);
  LogIID(riid);
  HRESULT hr = MF_E_UNSUPPORTED_SERVICE;
  if (ppvObject == NULL)
  {
    hr = E_POINTER;
  }
  else if (riid == __uuidof(IDirect3DDeviceManager9))
  {
    hr = m_pDeviceManager->QueryInterface(riid, (void**)ppvObject);
  }
  else if (riid == IID_IMFVideoDeviceID)
  {
    *ppvObject = static_cast<IMFVideoDeviceID*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFClockStateSink)
  {
    *ppvObject = static_cast<IMFClockStateSink*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFTopologyServiceLookupClient)
  {
    *ppvObject = static_cast<IMFTopologyServiceLookupClient*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFVideoPresenter)
  {
    *ppvObject = static_cast<IMFVideoPresenter*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFGetService)
  {
    *ppvObject = static_cast<IMFGetService*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFRateSupport)
  {
    *ppvObject = static_cast<IMFRateSupport*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFVideoDisplayControl)
  {
    *ppvObject = static_cast<IMFVideoDisplayControl*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IEVRTrustedVideoPlugin)
  {
    *ppvObject = static_cast<IEVRTrustedVideoPlugin*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == IID_IMFVideoPositionMapper)
  {
    *ppvObject = static_cast<IMFVideoPositionMapper*>(this);
    AddRef();
    hr = S_OK;
  }
  else
  {
    LogGUID(guidService);
    LogIID(riid);
    *ppvObject=NULL;
    hr = E_NOINTERFACE;
  }
  if (FAILED(hr) || (*ppvObject)==NULL)
  {
    Log("GetService failed");
  }
  return hr;
}


void MPEVRCustomPresenter::ReleaseSurfaces()
{
  Log("ReleaseSurfaces()");
  CAutoLock lock(this);
  HANDLE hDevice;
  CHECK_HR(m_pDeviceManager->OpenDeviceHandle(&hDevice), "failed opendevicehandle");
  IDirect3DDevice9* pDevice;
  CHECK_HR(m_pDeviceManager->LockDevice(hDevice, &pDevice, TRUE), "failed: lockdevice");
  // make sure that the surface is not in use anymore before we delete it.
  if (m_pCallback != NULL)
  {
    m_pCallback->PresentImage(0, 0, 0, 0, 0, 0);
  }
  Flush();
  m_iFreeSamples = 0;
  for (int i = 0; i < NUM_SURFACES; i++)
  {
    samples[i] = NULL;
    surfaces[i] = NULL;
    textures[i] = NULL;
    m_vFreeSamples[i] = NULL;
  }

  m_pDeviceManager->UnlockDevice(hDevice, FALSE);
  Log("Releasing device");
  pDevice->Release();
  m_pDeviceManager->CloseDeviceHandle(hDevice);
  Log("ReleaseSurfaces() done");
}


HRESULT MPEVRCustomPresenter::Paint(CComPtr<IDirect3DSurface9> pSurface)
{
  // Old current surface is saved in case the device is lost
  // and we need to restore it 
  IDirect3DSurface9* pOldSurface = NULL;

  try
  {
    if (m_pCallback == NULL || pSurface == NULL)
    {
      return E_FAIL;
    }

    // Presenter is flushing samples, do not render! (not considered as failure)
    // This should solve the random video frame freeze issue when stopping the playback
    if (m_bFlush && !m_bScrubbing)
    {
      return S_OK;
    }

    HRESULT hr;

    if (FAILED(hr = m_pD3DDev->GetRenderTarget(0, &pOldSurface)))
    {
      Log("EVR:Paint: Failed to get current render target: %u\n", hr);
    }

    int priority = GetThreadPriority(GetCurrentThread());
    if (priority != THREAD_PRIORITY_ERROR_RETURN)
    {
      SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
    }

    if (m_dD3DRefreshRate == 0)
    {
      GetRealRefreshRate();
    }

	  // Raster offset target
    double limitLow = 0.40;

    REFERENCE_TIME timePerFrame = m_rtTimePerFrame;
    if (m_DetectedFrameTime * 10000000.0 > 0) 
    {
      timePerFrame = (LONGLONG)(m_DetectedFrameTime * 10000000.0);
    }

    // Every second frame matching to display device refresh rate
    if (fabs(m_dD3DRefreshCycle - timePerFrame/20000) < 0.0015)
    {
      double limitLow = 0.75;
    }

    D3DRASTER_STATUS rasterStatus;
    LONGLONG prev = GetCurrentTimestamp();

    // Correct raster offset - It Will Come
    while (SUCCEEDED(m_pD3DDev->GetRasterStatus(0, &rasterStatus)))
    {
      if (!rasterStatus.InVBlank && 
        (rasterStatus.ScanLine >= limitLow * m_displayMode.Height )) //&&
        //(rasterStatus.ScanLine <= limitHigh * m_displayMode.Height))
      {
        break;
      }

      if ((GetCurrentTimestamp() - prev) > 800000) break;

      Sleep(1);	
    }

    m_rasterSyncOffset = (m_displayMode.Height - rasterStatus.ScanLine) * m_dDetectedScanlineTime;
    if (m_rasterSyncOffset > 1000)
    {
      // Correct invalid values, scanline can be bigger than screen resolution	
      m_rasterSyncOffset = m_dDetectedScanlineTime * m_displayMode.Height;
    }

    // Restore thread priority
    if (priority != THREAD_PRIORITY_ERROR_RETURN)
    {
      SetThreadPriority(GetCurrentThread(), priority);
    }

    LONGLONG startPaint = GetCurrentTimestamp();

    m_pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, FALSE);
    if (FAILED(hr = m_pD3DDev->StretchRect(pSurface, NULL, m_pVideoSurface, NULL, D3DTEXF_NONE)))
    {
      Log("EVR:Paint: StretchRect failed %u\n",hr);
    }
    m_pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
    m_pD3DDev->SetRenderTarget(0, m_pVideoSurface);

    if (m_bDrawStats)
    {
      m_pStatsRenderer->DrawStats();
      m_pStatsRenderer->DrawTearingTest();
    }

    m_pD3DDev->SetRenderTarget(0, pOldSurface);

    hr = m_pCallback->PresentImage(m_iVideoWidth, m_iVideoHeight, m_iARX,m_iARY, (DWORD)(IDirect3DTexture9*)m_pVideoTexture, (DWORD)(IDirect3DSurface9*)pSurface);

    m_PaintTime = GetCurrentTimestamp() - startPaint;
    m_PaintTimeMin = min(m_PaintTimeMin, m_PaintTime);
    m_PaintTimeMax = max(m_PaintTimeMax, m_PaintTime);

    OnVBlankFinished(true, startPaint, GetCurrentTimestamp());

    CalculateJitter(startPaint);

    if (m_bResetStats)
    {
      ResetTraceStats();
    }

    return hr;
  }
  catch(...)
  {
    if (pOldSurface)
    {
      Log("Paint() exception - restoring old render target");
      m_pD3DDev->SetRenderTarget(0, pOldSurface);
    }
    else
    {
      Log("Paint() exception");
    }
  }
  return E_FAIL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_FramesDroppedInRenderer(int *pcFrames)
{
  if (pcFrames == NULL)
  {
    return E_POINTER;
  }
  *pcFrames = m_iFramesDropped;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_FramesDrawn(int *pcFramesDrawn)
{
  if (pcFramesDrawn == NULL)
  {
    return E_POINTER;
  }
  *pcFramesDrawn = m_iFramesDrawn;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_AvgFrameRate(int *piAvgFrameRate)
{
  *piAvgFrameRate = (int)(m_fAvrFps*100);
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_Jitter(int *iJitter)
{
  *iJitter = (int)((m_fJitterStdDev/10000.0) + 0.5);
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_AvgSyncOffset(int *piAvg)
{
  *piAvg = (int)((m_fSyncOffsetAvr/10000.0) + 0.5);
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_DevSyncOffset(int *piDev)
{
  *piDev = (int)((m_fSyncOffsetStdDev/10000.0) + 0.5);
  return S_OK;
}


STDMETHODIMP MPEVRCustomPresenter::GetNativeVideoSize(SIZE *pszVideo, SIZE *pszARVideo)
{
  Log("IMFVideoDisplayControl.GetNativeVideoSize()");
  pszVideo->cx   = m_iVideoWidth;
  pszVideo->cy   = m_iVideoHeight;
  pszARVideo->cx = m_iARX;
  pszARVideo->cy = m_iARY;

  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetIdealVideoSize(SIZE *pszMin, SIZE *pszMax)
{
  Log("IMFVideoDisplayControl.GetIdealVideoSize()");
  pszMin->cx = m_iVideoWidth;
  pszMin->cy = m_iVideoHeight;
  pszMax->cx = m_iVideoWidth;
  pszMax->cy = m_iVideoHeight;
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetVideoPosition(const MFVideoNormalizedRect *pnrcSource, const LPRECT prcDest)
{
  Log("IMFVideoDisplayControl.SetVideoPosition()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetVideoPosition(MFVideoNormalizedRect *pnrcSource, LPRECT prcDest)
{
  pnrcSource->left = 0;
  pnrcSource->top = 0;
  pnrcSource->right = (float)m_iVideoWidth;
  pnrcSource->bottom = (float)m_iVideoHeight;

  prcDest->left = 0;
  prcDest->top = 0;
  prcDest->right = m_iVideoWidth;
  prcDest->bottom = m_iVideoHeight;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetAspectRatioMode(DWORD dwAspectRatioMode)
{
  Log("IMFVideoDisplayControl.SetAspectRatioMode()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetAspectRatioMode(DWORD *pdwAspectRatioMode)
{
  Log("IMFVideoDisplayControl.GetAspectRatioMode()");
  *pdwAspectRatioMode = VMR_ARMODE_NONE;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetVideoWindow(HWND hwndVideo)
{
  Log("IMFVideoDisplayControl.SetVideoWindow()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetVideoWindow(HWND *phwndVideo)
{
  Log("IMFVideoDisplayControl.GetVideoWindow()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::RepaintVideo(void)
{
  Log("IMFVideoDisplayControl.RepaintVideo()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetCurrentImage(BITMAPINFOHEADER *pBih, BYTE **pDib, DWORD *pcbDib, LONGLONG *pTimeStamp)
{
  Log("IMFVideoDisplayControl.GetCurrentImage()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetBorderColor(COLORREF Clr)
{
  Log("IMFVideoDisplayControl.SetBorderColor()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetBorderColor(COLORREF *pClr)
{
  Log("IMFVideoDisplayControl.GetBorderColor()");
  if (pClr)
  {
    *pClr = 0;
  }
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetRenderingPrefs(DWORD dwRenderFlags)
{
  Log("IMFVideoDisplayControl.SetRenderingPrefs()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetRenderingPrefs(DWORD *pdwRenderFlags)
{
  Log("IMFVideoDisplayControl.GetRenderingPrefs()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetFullscreen(BOOL fFullscreen)
{
  Log("IMFVideoDisplayControl.SetFullscreen()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetFullscreen(BOOL *pfFullscreen)
{
  Log("GetFullscreen()");
  *pfFullscreen=NULL;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::IsInTrustedVideoMode (BOOL *pYes)
{
  Log("IEVRTrustedVideoPlugin.IsInTrustedVideoMode()");
  *pYes=TRUE;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::CanConstrict (BOOL *pYes)
{
  *pYes=TRUE;
  Log("IEVRTrustedVideoPlugin.CanConstrict()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetConstriction(DWORD dwKPix)
{
  Log("IEVRTrustedVideoPlugin.SetConstriction(%d)",dwKPix);
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::DisableImageExport(BOOL bDisable)
{
  Log("IEVRTrustedVideoPlugin.DisableImageExport(%d)",bDisable);
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::MapOutputCoordinateToInputStream(float xOut,float yOut,DWORD dwOutputStreamIndex,DWORD dwInputStreamIndex,float* pxIn,float* pyIn)
{
  *pxIn = xOut;
  *pyIn = yOut;
  return S_OK;
}


void MPEVRCustomPresenter::EstimateRefreshTimings()
{
  if (m_pD3DDev)
  {
    D3DRASTER_STATUS rasterStatus;
    m_pD3DDev->GetRasterStatus(0, &rasterStatus);
    while (rasterStatus.ScanLine != 0)
    {
      m_pD3DDev->GetRasterStatus(0, &rasterStatus);
    }
    while (rasterStatus.ScanLine == 0)
    {
      m_pD3DDev->GetRasterStatus(0, &rasterStatus);
    }
    m_pD3DDev->GetRasterStatus(0, &rasterStatus);
    LONGLONG startTime = GetCurrentTimestamp();
    UINT startLine = rasterStatus.ScanLine;
    LONGLONG endTime = 0;
    LONGLONG time = 0;
    UINT endLine = 0;
    UINT line = 0;

    bool done = false;
    while (!done) // Estimate time for one scan line
    {
      m_pD3DDev->GetRasterStatus(0, &rasterStatus);
      line = rasterStatus.ScanLine;
      time = GetCurrentTimestamp();
      if (line > 0)
      {
        endLine = line;
        endTime = time;
      }
      else
      {
        done = true;
      }
    }

    m_dDetectedScanlineTime = (double)(endTime - startTime) / (double)((endLine - startLine) * 10000.0);

    m_dEstRefreshCycle = m_dD3DRefreshCycle;
  }
}


// Update the array m_pllJitter with a new vsync period. Calculate min, max and stddev.
void MPEVRCustomPresenter::CalculateJitter(LONGLONG PerfCounter)
{
  m_nNextJitter = (m_nNextJitter+1) % NB_JITTER;
	m_pllJitter[m_nNextJitter] = PerfCounter - m_llLastPerf;
  
  m_pllRasterSyncOffset[m_nNextJitter] = m_rasterSyncOffset;

	double syncDeviation = ((double)m_pllJitter[m_nNextJitter] - m_fJitterMean) / 10000.0;
	
  if (abs(syncDeviation) > (GetDisplayCycle() / 2))
  {
    // ignore glitches until enough data has been collected
    if (m_iFramesDrawn > NB_JITTER)
    {
      m_uSyncGlitches++;
    }
  }

	LONGLONG llJitterSum = 0;
	LONGLONG llJitterSumAvg = 0;
	for (int i = 0; i < NB_JITTER; i++)
	{
		LONGLONG Jitter = m_pllJitter[i];
		llJitterSum += Jitter;
		llJitterSumAvg += Jitter;
	}
	m_fJitterMean = double(llJitterSumAvg) / NB_JITTER;
	double DeviationSum = 0;

	for (int i = 0; i < NB_JITTER; i++)
	{
		LONGLONG DevInt = m_pllJitter[i] - (LONGLONG)m_fJitterMean;
		double Deviation = (double)DevInt;

		DeviationSum += Deviation*Deviation;
		
    if (m_iFramesDrawn > NB_JITTER)
    {
      m_MaxJitter = max(m_MaxJitter, DevInt);
		  m_MinJitter = min(m_MinJitter, DevInt);
    }
	}

	m_fJitterStdDev = sqrt(DeviationSum/NB_JITTER);
	m_fAvrFps = 10000000.0/(double(llJitterSum)/NB_JITTER);
	m_llLastPerf = PerfCounter;
}


// Collect the difference between periodEnd and periodStart in an array, calculate mean and stddev.
void MPEVRCustomPresenter::OnVBlankFinished(bool fAll, LONGLONG periodStart, LONGLONG periodEnd)
{
	m_nNextSyncOffset = (m_nNextSyncOffset+1) % NB_JITTER;
	m_pllSyncOffset[m_nNextSyncOffset] = periodStart - periodEnd;

	LONGLONG AvrageSum = 0;
	for (int i = 0; i < NB_JITTER; i++)
	{
		LONGLONG Offset = m_pllSyncOffset[i];
		AvrageSum += Offset;
		m_MaxSyncOffset = max(m_MaxSyncOffset, Offset);
		m_MinSyncOffset = min(m_MinSyncOffset, Offset);
	}
	double MeanOffset = double(AvrageSum)/NB_JITTER;
	double DeviationSum = 0;
	for (int i = 0; i < NB_JITTER; i++)
	{
		double Deviation = double(m_pllSyncOffset[i]) - MeanOffset;
		DeviationSum += Deviation*Deviation;
	}
	double StdDev = sqrt(DeviationSum/NB_JITTER);

	m_fSyncOffsetAvr = MeanOffset;
	m_fSyncOffsetStdDev = StdDev;
}


void MPEVRCustomPresenter::ResetTraceStats()
{
  m_uSyncGlitches   = 0;
  m_PaintTimeMin    = MAXLONG64;
  m_PaintTimeMax    = MINLONG64;
  m_MinJitter       = MAXLONG64;
  m_MaxJitter       = MINLONG64;
  m_MinSyncOffset   = MAXLONG64;
  m_MaxSyncOffset   = MINLONG64;
  m_iFramesDrawn    = 0;
  m_iFramesDropped  = 0;
  m_bResetStats     = false;
}


REFERENCE_TIME MPEVRCustomPresenter::GetFrameDuration()
{
  // TODO find a better place for this? Multi monitor support?
  if(m_dCycleDifference == 0.0 && m_rtTimePerFrame)
  {
    m_dCycleDifference = GetCycleDifference();
  }

  return m_rtTimePerFrame;
}


// Get the best estimate of the display refresh rate in Hz
double MPEVRCustomPresenter::GetRefreshRate()
{
  return m_dD3DRefreshRate;
}


// Get the best estimate of the display cycle time in milliseconds
double MPEVRCustomPresenter::GetDisplayCycle()
{
  return m_dD3DRefreshCycle;
}

// Get detected frame duration in milliseconds
double MPEVRCustomPresenter::GetDetectedFrameTime()
{
  return m_DetectedFrameTime;
}

// Get the difference in video and display cycle times.
double MPEVRCustomPresenter::GetCycleDifference()
{
	double dBaseDisplayCycle = GetDisplayCycle();
	UINT i;
	double minDiff = 1.0;
	if (dBaseDisplayCycle == 0.0 || m_dFrameCycle == 0.0)
  {
    return 1.0;
  }
  else
	{
		for (i = 1; i <= 8; i++) // Try a lot of multiples of the display frequency
		{
			double dDisplayCycle = i * dBaseDisplayCycle;
			double diff = (dDisplayCycle - m_rtTimePerFrame / 10000) / m_rtTimePerFrame / 10000;
			if (abs(diff) < abs(minDiff))
			{
				minDiff = diff;
				m_dOptimumDisplayCycle = dDisplayCycle;
			}
		}
	}
	return minDiff;
}


void MPEVRCustomPresenter::NotifyRateChange(double pRate)
{
  if (pRate != m_fSeekRate)
  {
    Log("NotifyRateChange: %f", pRate);
    m_fSeekRate = pRate;
    if (m_fSeekRate != 1.0 && m_fSeekRate != 0.0)
    {
      m_bScrubbing = true;
    }
    else
    {
      m_bScrubbing = false;
    }
  }
}


void MPEVRCustomPresenter::NotifyDVDMenuState(bool pIsInMenu)
{
  if (pIsInMenu != m_bDVDMenu)
  {
    Log("NotifyDVDMenu: %d", pIsInMenu);
    m_bDVDMenu = pIsInMenu;
  }
}

void MPEVRCustomPresenter::CorrectSampleTime(IMFSample* pSample)
{
  double ForceFPS = 0.0;
  //double ForceFPS = 59.94;
  //double ForceFPS = 23.976;
  if (ForceFPS != 0.0)
  {
    m_rtTimePerFrame = (LONGLONG)(10000000.0 / ForceFPS);
  }

  LONGLONG Duration = m_rtTimePerFrame;
  LONGLONG PrevTime = m_LastScheduledUncorrectedSampleTime;
  LONGLONG Time;
  LONGLONG SetDuration;
  pSample->GetSampleDuration(&SetDuration);
  pSample->GetSampleTime(&Time);
  m_LastScheduledUncorrectedSampleTime = Time;

  m_bCorrectedFrameTime = false;
  double LastFP = m_LastScheduledSampleTimeFP;

  LONGLONG Diff2 = (LONGLONG)(PrevTime - m_LastScheduledSampleTimeFP*10000000.0);
  LONGLONG Diff = Time - PrevTime;
  if (PrevTime == -1)
    Diff = 0;
  if (Diff < 0)
    Diff = -Diff;
  if (Diff2 < 0)
    Diff2 = -Diff2;
  if (Diff < m_rtTimePerFrame*8 && m_rtTimePerFrame && 
      Diff2 < m_rtTimePerFrame*8 && m_fRate == 1.0f &&
      !m_bDVDMenu && !m_bScrubbing) 
  {
    int iPos = (m_DetectedFrameTimePos++) % 30;
    LONGLONG Diff = Time - PrevTime;
    if (PrevTime == -1)
    Diff = 0;
    m_DetectedFrameTimeHistory[iPos] = Diff;

    if (m_DetectedFrameTimePos >= 10)
    {
      int nFrames = min(m_DetectedFrameTimePos, 30);
      LONGLONG DectedSum = 0;
      for (int i = 0; i < nFrames; ++i)
      {
        DectedSum += m_DetectedFrameTimeHistory[i];
      }

      double Average = double(DectedSum) / double(nFrames);
      double DeviationSum = 0.0;
      for (int i = 0; i < nFrames; ++i)
      {
        double Deviation = m_DetectedFrameTimeHistory[i] - Average;
        DeviationSum += Deviation*Deviation;
      }

      double StdDev = sqrt(DeviationSum/double(nFrames));

      m_DetectedFrameTimeStdDev = StdDev;

      double DetectedRate = 1.0/ (double(DectedSum) / (nFrames * 10000000.0) );

      double AllowedError = 0.0003;

      static double AllowedValues[] = {60.0, 59.94, 50.0, 48.0, 47.952, 30.0, 29.97, 25.0, 24.0, 23.976};

      int nAllowed = sizeof(AllowedValues) / sizeof(AllowedValues[0]);
      for (int i = 0; i < nAllowed; ++i)
      {
        if (fabs(1.0 - DetectedRate / AllowedValues[i]) < AllowedError)
        {
          DetectedRate = AllowedValues[i];
          break;
        }
      }

      m_DetectedFrameTimeHistoryHistory[m_DetectedFrameTimePos % 100] = DetectedRate;

      CMap<double, double, CAutoInt, CAutoInt> Map;

      for (int i = 0; i < 100; ++i)
      {
        ++Map[m_DetectedFrameTimeHistoryHistory[i]];
      }

      POSITION Pos = Map.GetStartPosition();
      double BestVal = 0.0;
      int BestNum = 5;
      while (Pos)
      {
        double Key;
        CAutoInt Value;
        Map.GetNextAssoc(Pos, Key, Value);
        if (Value.m_Int > BestNum && Key != 0.0)
        {
          BestNum = Value.m_Int;
          BestVal = Key;
        }
      }

      m_DetectedLock = false;
      for (int i = 0; i < nAllowed; ++i)
      {
        if (BestVal == AllowedValues[i])
        {
          m_DetectedLock = true;
          break;
        }
      }
      if (BestVal != 0.0)
      {
        m_DetectedFrameRate = BestVal;
        m_DetectedFrameTime = 1.0 / BestVal;
      }
    }

    LONGLONG PredictedNext = PrevTime + m_rtTimePerFrame;
    LONGLONG PredictedDiff = PredictedNext - Time;
    if (PredictedDiff < 0)
      PredictedDiff = -PredictedDiff;

    if (m_DetectedFrameTime != 0.0 && m_DetectedLock )
    {
      double CurrentTime = Time / 10000000.0;
      double LastTime = m_LastScheduledSampleTimeFP;
      double PredictedTime = LastTime + m_DetectedFrameTime;
      if (fabs(PredictedTime - CurrentTime) > 0.0015) // 1.5 ms wrong, lets correct
      {
        CurrentTime = PredictedTime;
        Time = (LONGLONG)(CurrentTime * 10000000.0);
        // Not needed?
        //pSample->SetSampleTime(Time);
        pSample->SetSampleDuration((LONGLONG)(m_DetectedFrameTime * 10000000.0));
        m_bCorrectedFrameTime = true;
      }
      m_LastScheduledSampleTimeFP = CurrentTime;;
    }
    else
    {
      m_LastScheduledSampleTimeFP = Time / 10000000.0;
    }
  }
  else
  {
    m_LastScheduledSampleTimeFP = Time / 10000000.0;
    if (Diff > m_rtTimePerFrame*8)
    {
      // Seek
      m_DetectedFrameTimePos = 0;
      m_DetectedLock = false;
    }
  }
  LOG_TRACE("EVR: Time: %f %f %f\n", Time / 10000000.0, SetDuration / 10000000.0, m_DetectedFrameRate);
}


// get driver refresh rate
void MPEVRCustomPresenter::GetRealRefreshRate()
{
  // Win7
  if (m_bIsWin7 && m_pW7GetRefreshRate)
  {
    m_dD3DRefreshRate = m_pW7GetRefreshRate();

    if (m_dD3DRefreshRate == -1)
    {
      m_dD3DRefreshRate = (double)m_displayMode.RefreshRate;
    }
  }
  else // XP or Vista
  {
	  m_dD3DRefreshRate = (double)m_displayMode.RefreshRate;
  }
  m_dD3DRefreshCycle = 1000.0 / m_dD3DRefreshRate; // in ms
}

