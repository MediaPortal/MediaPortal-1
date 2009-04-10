/* 
*	Copyright (C) 2005-2008 Team MediaPortal
*  Author: Frodo
*	http://www.team-mediaportal.com
*
*  This Program is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2, or (at your option)
*  any later version.
*   
*  This Program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
*  GNU General Public License for more details.
*   
*  You should have received a copy of the GNU General Public License
*  along with GNU Make; see the file COPYING.  If not, write to
*  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
*  http://www.gnu.org/copyleft/gpl.html
*
*/

// Windows Header Files:
#include <windows.h>
#include <streams.h>
#include <stdio.h>
#include <atlbase.h>
#include <string.h>
#include <atlconv.h>
#include <mmsystem.h>
#include <d3d9.h>
#include <d3dx9.h>
#include <d3d9types.h>
#include <strsafe.h>
#include <dshow.h>
#include <vmr9.h>
#include <sbe.h>
#include <dxva.h>
#include <dvdmedia.h>
#include <evr.h>
#include <mfapi.h>
#include <mfidl.h>
#include <mferror.h>
#include <objbase.h>
#include <dxva2api.h>
#include "dshowhelper.h"
#include "evrcustompresenter.h"
#include <ddraw.h>
#include <process.h>

#pragma warning( disable : 4244 )

void Log(const char *fmt, ...);
void LogRotate();
HRESULT __fastcall UnicodeToAnsi(LPCOLESTR pszW, LPSTR* ppszA);

static DWORD lastWorkerNotification = 0;

//maximum time to run ahead of actual presentation time to avoid vsync-locks
#define MAX_PRERUN 10
#define MAX_PRERUN_HNS ((MAX_PRERUN)*10000)

static BOOL           g_bTimerInitializer = false;
static BOOL           g_bQPCAvail;
static LARGE_INTEGER  g_liQPCFreq;
static DWORD          g_dLastTickCount;

static DWORD GetCurrentTimestamp()
{
  DWORD ms;

  if( !g_bTimerInitializer ) 
  {
    g_bQPCAvail = QueryPerformanceFrequency( &g_liQPCFreq );
    Log("GetCurrentTimestamp(): Performance timer available: %d", g_bQPCAvail);
    g_bTimerInitializer = true;
  }

  if( g_bQPCAvail ) 
  {
    LARGE_INTEGER tics;
    QueryPerformanceFrequency( &g_liQPCFreq );
    QueryPerformanceCounter( &tics );
    ms = (((double)tics.QuadPart) / ((double)g_liQPCFreq.QuadPart)) * 1000.0; // to milliseconds
  }
  else 
  {
    ms = GetTickCount();
  }

  return ms;
}

// Macro for locking 
#define TIME_LOCK(obj, crit, name)  \
  DWORD then = GetCurrentTimestamp(); \
  CAutoLock lock(obj); \
  DWORD diff = GetCurrentTimestamp() - then; \
  if ( diff >= crit ) { \
  Log("Critical lock time for %s was %d ms", name, diff ); \
  }
//#define TIME_LOCK(obj, crit, name) CAutoLock lock(obj);

// uncomment the //Log to enable extra logging
#define LOG_TRACE //Log

void LogIID( REFIID riid ) 
{
  LPOLESTR str;
  LPSTR astr;
  StringFromIID(riid, &str); 
  UnicodeToAnsi(str, &astr);
  Log("riid: %s", astr);
  CoTaskMemFree(str);
}

void LogGUID( REFGUID guid ) 
{
  LPOLESTR str;
  LPSTR astr;
  str = (LPOLESTR)CoTaskMemAlloc(200);
  StringFromGUID2(guid, str, 200); 
  UnicodeToAnsi(str, &astr);
  Log("guid: %s", astr);
  CoTaskMemFree(str);
}


void CALLBACK TimerCallback(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
  SchedulerParams *p = (SchedulerParams*)dwUser;
  Log("Callback %d", uTimerID);
  TIME_LOCK(&p->csLock, 3, "TimeCallback");
  if ( p->bDone ) 
  {
    Log("The end is near");
  }
  p->eHasWork.Set();
}

#define MIN(x,y) ((x)<(y))?(x):(y)
//wait for a maximum of 20 ms (less than one frame when frame rate is 50fps)
#define MAX_WAIT (20)
//if we have at least 10 ms spare time to next frame, get new sample
#define MIN_TIME_TO_PROCESS (10000*10)

UINT CALLBACK WorkerThread(void* param)
{
  timeBeginPeriod(1);
  SchedulerParams *p = (SchedulerParams*)param;
  while ( true ) 
  {
    p->csLock.Lock();
    if ( p->bDone ) 
    {
      Log("Worker done.");
      p->csLock.Unlock();
      //AvRevertMmThreadCharacteristics(hMmThread);
      return 0;
    }

    if ( !p->pPresenter->CheckForInput() ) {}
    p->csLock.Unlock();
    LOG_TRACE("Worker sleeping.");
    while ( !p->eHasWork.Wait() );
    LOG_TRACE( "Worker woken up" );
  }
  return -1;
}


UINT CALLBACK SchedulerThread(void* param)
{
  timeBeginPeriod(1);
  SchedulerParams *p = (SchedulerParams*)param;
  LONGLONG hnsSampleTime = 0;
  MMRESULT lastTimerId = 0;
  DWORD delay = 0;
  /*HANDLE hMmThread;
  hMmThread = AvSetMmThreadCharacteristics("Playback", &dwTaskIndex);
  AvSetMmThreadPriority(hMmThread, AVRT_PRIORITY_HIGH);*/
  DWORD				dwUser = 0;
  TIMECAPS tc;
  DWORD				dwResolution;
  timeGetDevCaps(&tc, sizeof(TIMECAPS));
  dwResolution = min(max(tc.wPeriodMin, 0), tc.wPeriodMax);
  dwUser		= timeBeginPeriod(dwResolution);

  while ( true ) 
  {
    if ( lastTimerId > 0 ) 
    {
      timeKillEvent(lastTimerId);
      lastTimerId = 0;
    }
    //Log("Scheduler callback");
    DWORD now = GetCurrentTimestamp();
    p->csLock.Lock();
    LOG_TRACE("Scheduler got lock");
    DWORD diff = GetCurrentTimestamp()-now;
    if ( diff > 10 ) Log("High lock latency in SchedulerThread: %d ms", diff);
    //if ( p->bDone ) Log("Trying to end things, waiting for timers : %d", p->iTimerSet);
    if ( p->bDone ) 
    {
      Log("Scheduler done.");
      if ( lastTimerId > 0 ) timeKillEvent(lastTimerId);
      p->csLock.Unlock();
      //AvRevertMmThreadCharacteristics(hMmThread);
      return 0;
    }

    p->pPresenter->CheckForScheduledSample(&hnsSampleTime, delay);
    LOG_TRACE("Got scheduling time: %I64d", hnsSampleTime);
    if ( hnsSampleTime > 0) 
    { 
      //Sleep(hnsSampleTime/10000);
      //wait for a maximum of 20 ms!
      //we try to be 3ms early and let vsync do the rest :) --> TODO better real estimation of next vblank!
      delay = MIN( hnsSampleTime/10000, MAX_WAIT );
      if( delay > MAX_PRERUN )
      {
        delay -= MAX_PRERUN;
      }
    }
    else
    {
      // backup check to avoid starvation (and work around unknown bugs)
      delay = MAX_WAIT;
    } 
    if ( delay > 0 ) 
    {
      LOG_TRACE("Setting Timer");
      lastTimerId = timeSetEvent(delay,0, (LPTIMECALLBACK)(HANDLE)p->eHasWork, 0, TIME_ONESHOT|TIME_KILL_SYNCHRONOUS|TIME_CALLBACK_EVENT_SET);
    }
    else 
    {
      p->eHasWork.Set();
    }
    p->csLock.Unlock();
    while ( !p->eHasWork.Wait() );
    LOG_TRACE( "Scheduler woken up" );
  }
  return -1;
}


MPEVRCustomPresenter::MPEVRCustomPresenter( IVMR9Callback* pCallback, IDirect3DDevice9* direct3dDevice, HMONITOR monitor)
: m_refCount(1), m_qScheduledSamples(NUM_SURFACES), m_didSkip(false)
{
  timeBeginPeriod(1);
  m_enableFrameSkipping = true;
  if (m_pMFCreateVideoSampleFromSurface!=NULL)
  {
    LogRotate();
    Log("----------v0.37---------------------------");
    m_hMonitor=monitor;
    m_pD3DDev=direct3dDevice;
    HRESULT hr = m_pDXVA2CreateDirect3DDeviceManager9( &m_iResetToken, &m_pDeviceManager );
    if ( FAILED(hr) ) 
    {
      Log( "Could not create DXVA2 Device Manager" );
    } 
    else 
    {
      m_pDeviceManager->ResetDevice(direct3dDevice, m_iResetToken);
    }
    m_pCallback = pCallback;
    m_bendStreaming = FALSE;
    m_state = MP_RENDER_STATE_SHUTDOWN;
    m_bSchedulerRunning = FALSE;
    m_bReallocSurfaces = FALSE;
    m_fRate = 1.0f;
    m_iFreeSamples = 0;
    m_dwLastStatLogTime = 0;
    //TODO: use ZeroMemory
    /*for ( int i=0; i<NUM_SURFACES; i++ ) {
    chains[i] = NULL;
    surfaces[i] = NULL;
    //samples[i] = NULL;
    }*/
  }
}
void MPEVRCustomPresenter::EnableFrameSkipping(bool onOff)
{
  Log("Evr Enable frame skipping:%d",onOff);
  m_enableFrameSkipping = onOff;
}

MPEVRCustomPresenter::~MPEVRCustomPresenter()
{
  if (m_pCallback != NULL)
  {
    m_pCallback->PresentImage(0,0,0,0,0,0);
  }
  StopWorkers();
  ReleaseSurfaces();
  m_pMediaType.Release();
  m_pDeviceManager =  NULL;
  for ( int i=0 ; i<NUM_SURFACES ; i++ ) 
  {
    m_vFreeSamples[i] = 0;
  }
  Log("Done");
}	

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetParameters( 
  /* [out] */ __RPC__out DWORD *pdwFlags,
  /* [out] */ __RPC__out DWORD *pdwQueue)
{
  Log("GetParameters");
  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::Invoke( 
  /* [in] */ __RPC__in_opt IMFAsyncResult *pAsyncResult)
{
  Log("Invoke");
  return S_OK;
}


// IUnknown
HRESULT MPEVRCustomPresenter::QueryInterface( 
  REFIID riid,
  void** ppvObject)
{
  HRESULT hr = E_NOINTERFACE;
  Log( "QueryInterface"  );
  LogIID( riid );
  if( ppvObject == NULL ) 
  {
    hr = E_POINTER;
  } 
  else if( riid == IID_IMFVideoDeviceID) 
  {
    *ppvObject = static_cast<IMFVideoDeviceID*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFTopologyServiceLookupClient) 
  {
    *ppvObject = static_cast<IMFTopologyServiceLookupClient*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFVideoPresenter) 
  {
    *ppvObject = static_cast<IMFVideoPresenter*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFGetService) 
  {
    *ppvObject = static_cast<IMFGetService*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IQualProp) 
  {
    *ppvObject = static_cast<IQualProp*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFRateSupport) 
  {
    *ppvObject = static_cast<IMFRateSupport*>( this );
    AddRef();
    hr = S_OK;
  }
  else if( riid == IID_IMFVideoDisplayControl ) 
  {
    *ppvObject = static_cast<IMFVideoDisplayControl*>( this );
    AddRef();
    Log( "QueryInterface:IID_IMFVideoDisplayControl:%x",(*ppvObject) );
    hr = S_OK;
  } 
  else if( riid == IID_IEVRTrustedVideoPlugin ) 
  {
    *ppvObject = static_cast<IEVRTrustedVideoPlugin*>( this );
    AddRef();
    Log( "QueryInterface:IID_IEVRTrustedVideoPlugin:%x",(*ppvObject) );
    hr = S_OK;
  } 
  else if( riid == IID_IMFVideoPositionMapper ) 
  {
    *ppvObject = static_cast<IMFVideoPositionMapper*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IUnknown ) 
  {
    *ppvObject = static_cast<IUnknown*>( static_cast<IMFVideoDeviceID*>( this ) );
    AddRef();
    hr = S_OK;    
  }
  else
  {
    LogIID( riid );
    *ppvObject=NULL;
    hr=E_NOINTERFACE;
  }
  if ( FAILED(hr) ) 
  {
    Log( "QueryInterface failed" );
  }
  return hr;
}

ULONG MPEVRCustomPresenter::AddRef()
{
  Log("MPEVRCustomPresenter::AddRef()");
  return InterlockedIncrement(& m_refCount);
}

ULONG MPEVRCustomPresenter::Release()
{
  Log("MPEVRCustomPresenter::Release()");
  ULONG ret = InterlockedDecrement(& m_refCount);
  if( ret == 0 )
  {
    Log("MPEVRCustomPresenter::Cleanup()");
    delete this;
  }
  return ret;
}

void MPEVRCustomPresenter::ResetStatistics()
{
  m_bfirstFrame = true;
  m_bfirstInput = true;
  m_iFramesDrawn = 0;
  m_iFramesDropped = 0;
  m_hnsLastFrameTime = 0;
  m_iFramesForStats = 0;
  m_iExpectedFrameDuration = 0;
  m_iMinFrameTimeDiff = MAXINT;
  m_iMaxFrameTimeDiff = 0;
  m_dwVariance = 0;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetSlowestRate( 
  /* [in] */ MFRATE_DIRECTION eDirection,
  /* [in] */ BOOL fThin,
  /* [out] */ __RPC__out float *pflRate)
{
  Log("GetSlowestRate");
  // There is no minimum playback rate, so the minimum is zero.
  *pflRate = 0; 
  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetFastestRate( 
  /* [in] */ MFRATE_DIRECTION eDirection,
  /* [in] */ BOOL fThin,
  /* [out] */ __RPC__out float *pflRate)
{
  Log("GetFastestRate");
  float   fMaxRate = 0.0f;

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

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::IsRateSupported( 
  /* [in] */ BOOL fThin,
  /* [in] */ float flRate,
  /* [unique][out][in] */ __RPC__inout_opt float *pflNearestSupportedRate)
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
  DWORD   cCount = 0;

  //just to make sure....
  ReleaseServicePointers();

  // Ask for the mixer
  cCount = 1;
  hr = pLookup->LookupService(      
    MF_SERVICE_LOOKUP_GLOBAL,   // Not used
    0,                          // Reserved
    MR_VIDEO_MIXER_SERVICE,     // Service to look up
    __uuidof(IMFTransform),     // Interface to look up
    (void**)&m_pMixer,          // Receives the pointer.
    &cCount );                  // Number of pointers

  if ( FAILED(hr) ) 
  {
    Log( "ERR: Could not get IMFTransform interface" );
  } 
  else 
  {
    Log( "Found mixers: %d", cCount );
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
    &cCount );                  // Number of pointers


  if ( FAILED(hr) ) 
  {
    Log( "ERR: Could not get IMFClock interface" );
  } 
  else 
  {
    Log( "Found clock: %d", cCount );
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
    &cCount );                  // Number of pointers

  if ( FAILED(hr) ) 
  {
    Log( "ERR: Could not get IMediaEventSink interface" );
  } 
  else 
  {
    Log( "Found event sink: %d", cCount );
    ASSERT(cCount == 0 || cCount == 1);
  }

  return S_OK;
}

HRESULT MPEVRCustomPresenter::ReleaseServicePointers() 
{
  Log("ReleaseServicePointers");
  //on some channel changes it may happen that ReleaseServicePointers is called only after InitServicePointers is called
  //to avoid this race condition, we only release when not in state begin_streamingi
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
  //AutoLock lock(m_ObjectLock);  // Hold the critical section.

  if (ppMediaType == NULL)
  {
    return E_POINTER;
  }

  //CHECK_HR(hr = CheckShutdown());

  if (m_pMediaType == NULL)
  {
    CHECK_HR(hr = MF_E_NOT_INITIALIZED, "MediaType is NULL");
  }

  CHECK_HR(hr = m_pMediaType->QueryInterface(
    __uuidof(IMFVideoMediaType), (void**)ppMediaType),
    "Query interface failed in GetCurrentMediaType");

  Log( "GetCurrentMediaType done" );
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

  if ( m_pClock == NULL ) 
  {
    *phnsDelta = -1;
    return S_OK;
  }

  // Get the sample's time stamp.
  hr = pSample->GetSampleTime(&hnsPresentationTime);
  // Get the current presentation time.
  // If there is no time stamp, there is no reason to get the clock time.
  if (SUCCEEDED(hr))
  {
    if ( hnsPresentationTime == 0 )
    {
      //immediate presentation
      *phnsDelta = -1;
      return S_OK;
    }
    // This method also returns the system time, which is not used
    // in this example.
    CHECK_HR(hr=m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime), "Could not get correlated time!");
  }
  else
  {
    Log("Could not get sample time from %p!", pSample);
    return hr;
  }

  // Calculate the amount of time until the sample's presentation
  // time. A negative value means the sample is late.
  hnsDelta = hnsPresentationTime - hnsTimeNow;
  //if off more than a second
  if (hnsDelta > 100000000 )
  {
    Log("dangerous and unlikely time to schedule [%p]: %I64d. scheduled time: %I64d, now: %I64d",
      pSample, hnsDelta, hnsPresentationTime, hnsTimeNow);
  }
  LOG_TRACE("Due: %I64d, Calculated delta: %I64d (rate: %f)", hnsPresentationTime/10000, hnsDelta, m_fRate);
  if ( m_fRate != 1.0f && m_fRate != 0.0f )
  {
    *phnsDelta = ((float)hnsDelta) / m_fRate;
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
  if ( SUCCEEDED(pType->GetUINT32(MF_MT_SOURCE_CONTENT_HINT, &u32) ) )
  {
    Log( "Getting aspect ratio 'MediaFoundation style'");
    switch ( u32 )
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
    }
  }
  else
  {
    //Try old DirectShow-Header, if above does not work
    Log( "Getting aspect ratio 'DirectShow style'");
    AM_MEDIA_TYPE* pAMMediaType;
    CHECK_HR(
      hr = pType->GetRepresentation(FORMAT_VideoInfo2, (void**)&pAMMediaType),
      "Getting DirectShow Video Info failed");
    if ( SUCCEEDED(hr) ) 
    {
      VIDEOINFOHEADER2* vheader = (VIDEOINFOHEADER2*)pAMMediaType->pbFormat;
      *piARX = vheader->dwPictAspectRatioX;
      *piARY = vheader->dwPictAspectRatioY;
      pType->FreeRepresentation(FORMAT_VideoInfo2, (void*)pAMMediaType);
    }
    else
    {
      Log( "Could not get directshow representation.");
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

  hr = pType->GetUINT64(MF_MT_FRAME_RATE, (UINT64*)&u64);
  if ( SUCCEEDED(hr) ) 
  {
    Log("Media frame rate: %d / %d", u64.HighPart, u64.LowPart);
    m_iExpectedFrameDuration = (1000*u64.LowPart) / u64.HighPart;
  } 
  else 
  {
    m_iExpectedFrameDuration = 0;
  }

  CHECK_HR(pType->GetUINT64(MF_MT_FRAME_SIZE, (UINT64*)&u64), "Getting Framesize failed!");

  MFVideoArea Area;
  UINT32 rSize;
  CHECK_HR(pType->GetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&Area, sizeof(Area), &rSize), "Failed to get MF_MT_GEOMETRIC_APERTURE");
  m_iVideoWidth = u64.HighPart; //Area.Area.cx; //u64.HighPart;
  m_iVideoHeight = u64.LowPart; //Area.Area.cy; //u64.LowPart;
  //use video size as default value for aspect ratios
  m_iARX = m_iVideoWidth;
  m_iARY = m_iVideoHeight;
  CHECK_HR(GetAspectRatio(pType, &m_iARX, &m_iARY), "Failed to get aspect ratio");
  Log( "New format: %dx%d, Ratio: %d:%d",	m_iVideoWidth, m_iVideoHeight, m_iARX, m_iARY );

  GUID subtype;
  CHECK_HR(pType->GetGUID(MF_MT_SUBTYPE, &subtype), "Could not get subtype");
  LogGUID( subtype );
  if ( m_pMediaType == NULL )
  {
    *pbHasChanged = TRUE;
  }
  else
  {
    BOOL doMatch;
    hr = m_pMediaType->Compare(pType, MF_ATTRIBUTES_MATCH_ALL_ITEMS, &doMatch);
    if ( SUCCEEDED(hr) )
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
  if ( !*pbHasChanged )
  {
    Log("Detected same media type as last one.");
  }
  return S_OK;
}

void MPEVRCustomPresenter::ReAllocSurfaces()
{
  Log("ReallocSurfaces");
  //TIME_LOCK(this, 20, "ReAllocSurfaces")
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
  //TODO check media type for correct format!
  d3dpp.BackBufferFormat = D3DFMT_X8R8G8B8;
  d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
  d3dpp.Windowed = true;
  d3dpp.EnableAutoDepthStencil = false;
  d3dpp.AutoDepthStencilFormat = D3DFMT_X8R8G8B8;
  d3dpp.FullScreen_RefreshRateInHz = D3DPRESENT_RATE_DEFAULT;
  d3dpp.PresentationInterval = D3DPRESENT_INTERVAL_ONE; //D3DPRESENT_INTERVAL_DEFAULT;

  HANDLE hDevice;
  IDirect3DDevice9* pDevice;
  CHECK_HR(m_pDeviceManager->OpenDeviceHandle(&hDevice), "Cannot open device handle");
  CHECK_HR(m_pDeviceManager->LockDevice(hDevice, &pDevice, TRUE), "Cannot lock device");
  HRESULT hr;
  Log("Textures will be %dx%d", m_iVideoWidth, m_iVideoHeight);
  for ( int i=0; i<NUM_SURFACES; i++ ) 
  {
    hr = pDevice->CreateTexture(m_iVideoWidth, m_iVideoHeight, 1,
      D3DUSAGE_RENDERTARGET, D3DFMT_X8R8G8B8, D3DPOOL_DEFAULT,
      &textures[i], NULL);
    //Log( "Creating chain %d...", i );
    if ( FAILED(hr) )
    {
      Log("Could not create offscreen surface. Error 0x%x", hr);
    }
    CHECK_HR( textures[i]->GetSurfaceLevel(0, &surfaces[i]), "Could not get surface from texture");
    /*hr = pDevice->CreateAdditionalSwapChain(&d3dpp, &chains[i]);
    if (FAILED(hr)) {
    Log("Chain creation failed with 0x%x", hr);
    return;
    }
    hr = chains[i]->GetBackBuffer(0, D3DBACKBUFFER_TYPE_MONO, &surfaces[i]);
    if (FAILED(hr)) {
    Log("Could not get back buffer: 0x%x", hr);
    return;
    }*/

    hr = m_pMFCreateVideoSampleFromSurface(surfaces[i],
      &samples[i]);
    if (FAILED(hr)) 
    {
      Log("CreateVideoSampleFromSurface failed: 0x%x", hr);
      return;
    }
    Log("Adding sample: 0x%x", samples[i]);
    m_vFreeSamples[i] = samples[i];
    //Log("Chain created");
  } 
  m_iFreeSamples = NUM_SURFACES;
  CHECK_HR(m_pDeviceManager->UnlockDevice(hDevice, FALSE), "failed: Unlock device");
  Log("Releasing device: %d", pDevice->Release());
  CHECK_HR(m_pDeviceManager->CloseDeviceHandle(hDevice), "failed: CloseDeviceHandle");


  m_pVideoTexture = NULL;
  m_pVideoSurface = NULL;



  if(FAILED(hr = m_pD3DDev->CreateTexture(
    m_iVideoWidth, m_iVideoHeight, 1, 
    D3DUSAGE_RENDERTARGET, /*D3DFMT_X8R8G8B8*/D3DFMT_A8R8G8B8, 
    D3DPOOL_DEFAULT, &m_pVideoTexture, NULL)))
    return;

  if(FAILED(hr = m_pVideoTexture->GetSurfaceLevel(0, &m_pVideoSurface)))
    return;


  hr = m_pD3DDev->ColorFill(m_pVideoSurface, NULL, 0);

  Log("ReallocSurfaces done");
}


HRESULT MPEVRCustomPresenter::CreateProposedOutputType(IMFMediaType* pMixerType, IMFMediaType** pType)
{
  HRESULT hr;
  LARGE_INTEGER i64Size;

  hr = m_pMFCreateMediaType(pType);
  if (SUCCEEDED (hr))
  {
    CHECK_HR(hr=pMixerType->CopyAllItems(*pType), "failed: CopyAllItems. Could not clone media type" );
    if ( SUCCEEDED(hr) )
    {
      Log("Successfully cloned media type");
    }
    (*pType)->SetUINT32 (MF_MT_PAN_SCAN_ENABLED, 0);

    i64Size.HighPart = 800;
    i64Size.LowPart	 = 600;
    //(*pType)->SetUINT64 (MF_MT_FRAME_SIZE, i64Size.QuadPart);

    i64Size.HighPart = 1;
    i64Size.LowPart  = 1;
    //(*pType)->SetUINT64 (MF_MT_PIXEL_ASPECT_RATIO, i64Size.QuadPart);

    CHECK_HR((*pType)->GetUINT64(MF_MT_FRAME_SIZE, (UINT64*)&i64Size.QuadPart), "Failed to get MF_MT_FRAME_SIZE");
    Log("Frame size: %dx%d",i64Size.HighPart, i64Size.LowPart); 

    MFVideoArea Area;
    UINT32 rSize;
    /*Log("Would set aperture: %dx%d", VideoFormat->videoInfo.dwWidth,
    VideoFormat->videoInfo.dwHeight);*/
    ZeroMemory( &Area, sizeof(MFVideoArea) );
    //TODO get the real screen size, and calculate area
    //corresponding to the given aspect ratio
    Area.Area.cx = MIN(800, i64Size.HighPart);
    Area.Area.cy = MIN(450, i64Size.LowPart);
    //for hardware scaling, use the following line:
    //(*pType)->SetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&Area, sizeof(MFVideoArea));
    CHECK_HR((*pType)->GetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&Area, sizeof(Area), &rSize), "Failed to get MF_MT_GEOMETRIC_APERTURE");
    Log("Aperture size: %x:%x, %dx%d", Area.OffsetX.value, Area.OffsetY.value, Area.Area.cx, Area.Area.cy); 
  }
  return hr;
}  

HRESULT MPEVRCustomPresenter::LogOutputTypes()
{
  Log("--Dumping output types----");
  //CAutoLock lock(this);
  HRESULT hr = S_OK;
  BOOL fFoundMediaType = FALSE;

  CComPtr<IMFMediaType> pMixerType;
  CComPtr<IMFMediaType> pType;

  if (!m_pMixer)
  {
    return MF_E_INVALIDREQUEST;
  }

  //LogMediaTypes(m_pMixer);
  // Loop through all of the mixer's proposed output types.
  DWORD iTypeIndex = 0;
  while (!fFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
  {
    pMixerType.Release();
    pType.Release();
    Log(  "Testing media type..." );
    // Step 1. Get the next media type supported by mixer.
    hr = m_pMixer->GetOutputAvailableType(0, iTypeIndex++, &pMixerType);
    if (FAILED(hr))
    {
      if ( hr != MF_E_NO_MORE_TYPES )
      {
        Log("stopping, hr=0x%x!", hr );
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
    LogGUID( subtype );
  }
  Log("--Dumping output types done----");
  return S_OK;
}

HRESULT MPEVRCustomPresenter::RenegotiateMediaOutputType()
{
  CAutoLock wLock(&m_workerParams.csLock);
  CAutoLock sLock(&m_schedulerParams.csLock);
  Log("RenegotiateMediaOutputType");
  //LogOutputTypes();
  HRESULT hr = S_OK;
  BOOL fFoundMediaType = FALSE;

  CComPtr<IMFMediaType> pMixerType;
  CComPtr<IMFMediaType> pType;

  if (!m_pMixer)
  {
    return MF_E_INVALIDREQUEST;
  }

  //LogMediaTypes(m_pMixer);
  // Loop through all of the mixer's proposed output types.
  DWORD iTypeIndex = 0;
  while (!fFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
  {
    pMixerType.Release();
    pType.Release();
    Log(  "Testing media type..." );
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
      //Create a clone of the suggested outputtype
      hr = CreateProposedOutputType(pMixerType, &pType);
      //pType = pMixerType;
    }

    // Step 4. Check if the mixer will accept this media type.
    if (SUCCEEDED(hr))
    {
      hr = m_pMixer->SetOutputType(0, pType, MFT_SET_TYPE_TEST_ONLY);
    }

    // Step 5. Try to set the media type on ourselves.
    if (SUCCEEDED(hr))
    {
      Log( "New media type successfully negotiated!" );
      BOOL bHasChanged;
      hr = SetMediaType(pType, &bHasChanged);
      //m_pMediaType = pType;
      if (SUCCEEDED(hr))
      {
        if ( bHasChanged )
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
        Log( "Could not set output type: 0x%x", hr );
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

static int fscount=0;
HRESULT MPEVRCustomPresenter::GetFreeSample(IMFSample** ppSample) 
{
  TIME_LOCK(&m_lockSamples,5,"GetFreeSample");
  //TODO hold lock?
  LOG_TRACE( "Trying to get free sample, size: %d", m_iFreeSamples);
  if ( m_iFreeSamples == 0 ) return E_FAIL;
  m_iFreeSamples--;
  *ppSample = m_vFreeSamples[m_iFreeSamples];
  m_vFreeSamples[m_iFreeSamples] = NULL;

  return S_OK;
}

void MPEVRCustomPresenter::Flush()
{
  CAutoLock sLock(&m_lockSamples);
  CAutoLock ssLock(&m_lockScheduledSamples);
  Log( "Flushing: size=%d", m_qScheduledSamples.Count() );
  while ( m_qScheduledSamples.Count()>0 )
  {
    IMFSample* pSample = PeekSample();
    if ( pSample != NULL ) 
    {
      PopSample();
      ReturnSample(pSample, FALSE);
    }
  }
  m_bFlush = FALSE;
}

void MPEVRCustomPresenter::ReturnSample(IMFSample* pSample, BOOL tryNotify)
{
  //CAutoLock lock(this);
  TIME_LOCK(&m_lockSamples, 5, "ReturnSample")
    LOG_TRACE( "Sample returned: now having %d samples", m_iFreeSamples+1);
  m_vFreeSamples[m_iFreeSamples++] = pSample;
  if ( m_qScheduledSamples.Count() == 0 ) 
  {
    LOG_TRACE("No scheduled samples, queue was empty -> todo, CheckForEndOfStream()");
    CheckForEndOfStream();
    if( m_pEventSink ) 
    {
      // Is this needed?
      m_pEventSink->Notify(EC_SAMPLE_NEEDED, 0, 0);
    }

  }
  if ( tryNotify && m_iFreeSamples == 1 && m_bInputAvailable ) NotifyWorker();
}

HRESULT MPEVRCustomPresenter::PresentSample(IMFSample* pSample)
{
  HRESULT hr = S_OK;
  IMFMediaBuffer* pBuffer = NULL;
  IDirect3DSurface9* pSurface = NULL;
  //IDirect3DSwapChain9* pSwapChain = NULL;
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
    // Get the swap chain from the surface.
    /*CHECK_HR(hr = pSurface->GetContainer(
    __uuidof(IDirect3DSwapChain9),
    (void**)&pSwapChain),
    "failed: GetContainer");*/

    // Calculate offset to scheduled time
    m_iFramesDrawn++;
    if ( m_pClock != NULL ) {
      LONGLONG hnsTimeNow, hnsSystemTime, hnsTimeScheduled;
      m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime);

      pSample->GetSampleTime(&hnsTimeScheduled);
      if ( hnsTimeScheduled > 0 )
      {
        LONGLONG deviation = hnsTimeNow - hnsTimeScheduled;
        if ( deviation < 0 ) deviation = -deviation;
        m_hnsTotalDiff += deviation;
      }
      if ( m_hnsLastFrameTime != 0  && m_iExpectedFrameDuration > 0 )
      {
        m_iFramesForStats ++;
        if ( m_iFramesForStats > 1000 )
        {
          m_iFramesForStats = 1;
          m_dwVariance = 0;
        }
        LONGLONG hnsDiff = hnsTimeNow - m_hnsLastFrameTime;
        int duration = (hnsDiff/10000);
        int dev = m_iExpectedFrameDuration - duration;
        m_dwVariance += dev*dev;
        if (duration < m_iMinFrameTimeDiff) m_iMinFrameTimeDiff = duration;
        if (duration > m_iMaxFrameTimeDiff) m_iMaxFrameTimeDiff = duration;
      }
      m_hnsLastFrameTime = hnsTimeNow;
    }
    // Present the swap surface
    m_didSkip = false;
    LOG_TRACE("Painting");
    DWORD then = GetCurrentTimestamp();
    CHECK_HR(hr = Paint(pSurface), "failed: Paint");
    DWORD diff = GetCurrentTimestamp() - then;
    LOG_TRACE("Paint() latency: %d ms", diff);
  }

  SAFE_RELEASE(pBuffer);
  SAFE_RELEASE(pSurface);
  //SAFE_RELEASE(pSwapChain);
  if (hr == D3DERR_DEVICELOST || hr == D3DERR_DEVICENOTRESET)
  {
    // Failed because the device was lost.
    Log("D3DDevice was lost!");
    //hr = S_OK;
    /*HRESULT hrTmp = TestCooperativeLevel();
    if (hrTmp == D3DERR_DEVICENOTRESET)
    {
    Log("Lost device!");
    //HandleLostDevice();
    }*/
  }

  //Log ( "Presented sample, returning %d\n", hr );
  return hr;
}

BOOL MPEVRCustomPresenter::CheckForInput()
{
  int counter;
  ProcessInputNotify(&counter);
  //if ( counter == 0 ) Log("Unneccessary call to ProcessInputNotify");
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

bool MPEVRCustomPresenter::IsNextAlreadyDue() {
  if (m_qScheduledSamples.IsEmpty()) {
    return false;
  }
  IMFSample* pSample = PeekSample();
  LONGLONG delta;
  GetTimeToSchedule(pSample, &delta);
  if ( delta < 0 ) {
    LOG_TRACE("Next is due too.");
    return true;
  }
  return false;
}

HRESULT MPEVRCustomPresenter::CheckForScheduledSample(LONGLONG *pNextSampleTime, DWORD msLastSleepTime)
{
  HRESULT hr = S_OK;
  int samplesProcessed=0;
  LogStats();
  LOG_TRACE("Checking for scheduled sample (size: %d)", m_qScheduledSamples.Count());
  *pNextSampleTime = 0;
  if ( m_bFlush )
  {
    Flush();
    return S_OK;
  }
  while ( m_qScheduledSamples.Count() > 0 ) 
  {
    IMFSample* pSample = PeekSample();
    if ( pSample == NULL ) break;
    if ( m_state == MP_RENDER_STATE_STARTED ) 
    {
      CHECK_HR(hr=GetTimeToSchedule(pSample, pNextSampleTime), "Couldn't get time to schedule!");
      if ( FAILED(hr) ) *pNextSampleTime = 1;
    }
    else if ( m_bfirstFrame )
    {
      *pNextSampleTime = -1; //immediate
    }
    else
    {
      *pNextSampleTime = 0; //not now!
      break;
    }
    LOG_TRACE( "Time to schedule: %I64d", *pNextSampleTime );
    //if we are ahead only 10 ms, present this sample anyway, as the vsync will be waited for anyway
    if ( *pNextSampleTime > MAX_PRERUN_HNS ) 
    {
      break;
    }
    PopSample();
    samplesProcessed++;
    //skip only if we have a newer sample available; IsNextAlreadyDue() checks if the next one should be rendered already
    //this gives us smooth fast-forward and stuff :)
    bool nextDue = IsNextAlreadyDue();
    if ( *pNextSampleTime < -250000 || nextDue  ) 
    {
      if (  m_qScheduledSamples.Count() > 0 ) //BREAKS DVD NAVIGATION: || *pNextSampleTime < -1500000 ) 
      {
        //skip!

        //skip only every second frame max. (if not older than 250ms)
        if (!m_enableFrameSkipping)// || (m_didSkip && *pNextSampleTime > -2500000)  )
        {
          Log("Not skipping frame (disabled or skip-smoothing mode engaged)");
          CHECK_HR(PresentSample(pSample), "PresentSample failed");
        }
        else
        {
          m_iFramesDropped++;
          //nextDue means that we are most likely fast forwarding. don't report as dropped
          if ( !nextDue ) {
            m_didSkip = true;
            Log( "skipping frame, behind %I64d ms, last sleep time %d ms.", -*pNextSampleTime/10000, msLastSleepTime );
          }
        }
      }
      else
      {
        //too late, but present anyway
        Log("frame is too late for %I64d ms, last sleep time %d ms.", -*pNextSampleTime/10000, msLastSleepTime );
        CHECK_HR(PresentSample(pSample), "PresentSample failed");
      }
    } 
    else 
    {
      CHECK_HR(PresentSample(pSample), "PresentSample failed");
    }
    *pNextSampleTime = 0;
    ReturnSample(pSample, TRUE);
    //EXPERIMENTAL: give other threads some time to breath
    Sleep(3);
  }
  //if ( samplesProcessed == 0 ) Log("Useless call to CheckForScheduledSamples");
  return hr;
} 

void MPEVRCustomPresenter::StartWorkers()
{
  CAutoLock lock(this);
  if ( m_bSchedulerRunning ) return;
  StartThread(&m_hScheduler, &m_schedulerParams, SchedulerThread, &m_uSchedulerThreadId, THREAD_PRIORITY_TIME_CRITICAL);
  StartThread(&m_hWorker, &m_workerParams, WorkerThread, &m_uWorkerThreadId, THREAD_PRIORITY_BELOW_NORMAL);
  m_bSchedulerRunning = TRUE;
}

void MPEVRCustomPresenter::StopWorkers()
{
  Log("Stopping workers...");
  CAutoLock lock(this);
  Log("Threads running : %s", m_bSchedulerRunning?"TRUE":"FALSE");
  if ( !m_bSchedulerRunning ) return;
  EndThread(m_hScheduler, &m_schedulerParams);
  EndThread(m_hWorker, &m_workerParams);
  m_bSchedulerRunning = FALSE;
}

void MPEVRCustomPresenter::StartThread(PHANDLE handle, SchedulerParams* pParams,
                                       UINT  (CALLBACK *ThreadProc)(void*), UINT* threadId, int priority)
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
  if ( m_bSchedulerRunning )
  {
    params->eHasWork.Set();
  } 
  else 
  {
    Log("Scheduler is already shut down");
  }
  /*if ( !m_bSchedulerRunning ) {
  Log("ERROR: Scheduler not running!");
  return;
  } 
  m_schedulerParams->eHasWork.Set();*/
}

void MPEVRCustomPresenter::NotifyScheduler()
{
  LOG_TRACE( "NotifyScheduler()" );
  NotifyThread(&m_schedulerParams);
}

void MPEVRCustomPresenter::NotifyWorker()
{
  LOG_TRACE( "NotifyWorker()" );
  lastWorkerNotification = GetCurrentTimestamp();
  NotifyThread(&m_workerParams);
}

BOOL MPEVRCustomPresenter::PopSample()
{
  CAutoLock lock(&m_lockScheduledSamples);
  LOG_TRACE("Removing scheduled sample, size: %d", m_qScheduledSamples.Count());
  if ( m_qScheduledSamples.Count() > 0 )
  {
    m_qScheduledSamples.Get();
    return TRUE;
  }
  return FALSE;
}

IMFSample* MPEVRCustomPresenter::PeekSample()
{
  CAutoLock lock(&m_lockScheduledSamples);
  if ( m_qScheduledSamples.Count() == 0 )
  {
    Log("ERR: PeekSample: empty queue!");
    return NULL;
  }
  return m_qScheduledSamples.Peek();
}

void MPEVRCustomPresenter::ScheduleSample(IMFSample* pSample)
{
  CAutoLock lock(&m_lockScheduledSamples);
  LOG_TRACE( "Scheduling Sample, size: %d", m_qScheduledSamples.Count() );
  DWORD hr;
  LONGLONG nextSampleTime;
  CHECK_HR(hr=GetTimeToSchedule(pSample, &nextSampleTime), "Couldn't get time to schedule!");
  if ( SUCCEEDED(hr) ) 
  {
    //consider 5 ms "just-in-time" for log-length's sake
    if ( nextSampleTime < -50000 ) 
    {
      Log("Scheduling sample from the past (%I64d ms, last call to NotifyWorker: %d ms)", 
        -nextSampleTime/10000, GetCurrentTimestamp()-lastWorkerNotification);
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
  //CAutoLock lock(this);
  if ( !m_bendStreaming )
  {
    return FALSE;
  }
  //samples pending
  if ( m_qScheduledSamples.Count() > 0 )
  {
    return FALSE;
  }
  if ( m_pEventSink ) 
  {
    Log("Sending completion message");
    m_pEventSink->Notify(EC_COMPLETE, (LONG_PTR)S_OK, 0);
  }
  m_bendStreaming = FALSE;
  return TRUE;
}


HRESULT MPEVRCustomPresenter::ProcessInputNotify(int* samplesProcessed)
{
  //TIME_LOCK(this, 1, "ProcessInputNotify");
  //TIME_LOCK(&m_lockSamples, 5, "ProcessInputNotify")
  LOG_TRACE("ProcessInputNotify");
  HRESULT hr=S_OK;
  *samplesProcessed = 0;
  if ( m_pClock != NULL ) 
  {
    MFCLOCK_STATE state;
    m_pClock->GetState(0, &state);
    if (state == MFCLOCK_STATE_PAUSED && !m_bfirstInput) 
    {
      Log( "Should not be processing data in pause mode");
      m_bInputAvailable = FALSE;
      return S_OK;
    }
  } else {
    return S_OK;
  }
  //try to process as many samples as possible:
  BOOL bhasMoreSamples = true;
  m_bInputAvailable = FALSE;
  do {
    IMFSample* sample;
    hr = GetFreeSample(&sample);
    if ( FAILED(hr) ) 
    {
      //LOG_TRACE( "No free sample available" );
      m_bInputAvailable = TRUE;
      //double-checked locking, in case someone freed a sample between the above 2 steps and we would miss notification
      hr = GetFreeSample(&sample);
      if ( FAILED(hr) ) 
      {
        LOG_TRACE("Still more input available");
        return S_OK;
      }
      m_bInputAvailable = FALSE;
    }

    LONGLONG timeBeforeMixer, systemTime;
    m_pClock->GetCorrelatedTime(0, &timeBeforeMixer, &systemTime);

    if ( m_pMixer == NULL )
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
    if ( SUCCEEDED( hr ) ) 
    {
      LONGLONG sampleTime;
      LONGLONG timeAfterMixer;
      sample->GetSampleTime(&sampleTime);

      m_bfirstInput = false;
      *samplesProcessed++;

      m_pClock->GetCorrelatedTime(0, &timeAfterMixer, &systemTime);

      LONGLONG mixerLatency = timeAfterMixer - timeBeforeMixer;
      LONGLONG sampleLatency = sampleTime-timeAfterMixer ;

      if( m_pEventSink ) 
      {
        m_pEventSink->Notify(EC_PROCESSING_LATENCY, (LONG_PTR)&mixerLatency, 0);
        //m_pEventSink->Notify(EC_SAMPLE_LATENCY, (LONG_PTR)&sampleLatency, 0);
      }

      ScheduleSample(sample);

      // TODO - following code would allow much smooter seeking with seek steps, but unfortunately
      // it will break 1) DVD navigation 2) REW/FW. 

      // schedule only samples that are in the future
      /*if( sampleLatency <= 0 )
      {
      //Log("sending EC_PROCESSING_LATENCY == %I64d -- EC_SAMPLE_LATENCY == %I64d", mixerLatency/10000, sampleLatency/10000);
      ScheduleSample(sample);
      }
      else
      {
      Log("Dropping sample - EC_PROCESSING_LATENCY == %I64d -- EC_SAMPLE_LATENCY == %I64d", mixerLatency/10000, sampleLatency/10000);
      ReturnSample(sample, FALSE);
      }*/
    } 
    else 
    {
      ReturnSample(sample, FALSE);
      switch ( hr ) 
      {
      case MF_E_TRANSFORM_NEED_MORE_INPUT:
        //we are done for now
        hr = S_OK;
        bhasMoreSamples = false;
        LOG_TRACE("Need more input...");
        //m_bInputAvailable = FALSE;
        CheckForEndOfStream();
        break;
      case MF_E_TRANSFORM_STREAM_CHANGE:
        Log( "Unhandled: transform_stream_change");
        break;
      case MF_E_TRANSFORM_TYPE_NOT_SET:
        //no errors, just infos why it didn't succeed
        Log( "ProcessOutput: change of type" );
        bhasMoreSamples = FALSE;
        //hr = S_OK;
        hr = RenegotiateMediaOutputType();
        break;
      default:
        Log( "ProcessOutput failed: 0x%x", hr );
      }
      return hr;
    }
  } while ( bhasMoreSamples );
  return hr;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::ProcessMessage( 
  MFVP_MESSAGE_TYPE eMessage,
  ULONG_PTR ulParam)
{
  HRESULT hr = S_OK;
  LOG_TRACE( "Processmessage: %d, %p", eMessage, ulParam );
  switch ( eMessage ) 
  {
  case MFVP_MESSAGE_INVALIDATEMEDIATYPE:
    Log( "Negotiate Media type" );
    //The mixer's output media type is invalid. The presenter should negotiate a new media type with the mixer. See Negotiating Formats.
    hr = RenegotiateMediaOutputType();
    break;

  case MFVP_MESSAGE_BEGINSTREAMING:
    //Streaming has started. No particular action is required by this message, but you can use it to allocate resources.
    Log("ProcessMessage %x", eMessage);
    m_bendStreaming = FALSE;
    m_state = MP_RENDER_STATE_STARTED;
    ResetStatistics();
    StartWorkers();
    break;

  case MFVP_MESSAGE_ENDSTREAMING:
    //Streaming has ended. Release any resources that you allocated in response to the MFVP_MESSAGE_BEGINSTREAMING message.
    Log("ProcessMessage %x", eMessage);
    //m_bendStreaming = TRUE;
    m_state = MP_RENDER_STATE_STOPPED;
    break;

  case MFVP_MESSAGE_PROCESSINPUTNOTIFY:
    //The mixer has received a new input sample and might be able to generate a new output frame. The presenter should call IMFTransform::ProcessOutput on the mixer. See Processing Output.
    //Log("Message 2: %d", m_lInputAvailable);
    //InterlockedIncrement(&m_lInputAvailable);
    //      NotifyWorker();
    if ( !ImmediateCheckForInput() ) {
      NotifyWorker();
    }
    break;

  case MFVP_MESSAGE_ENDOFSTREAM:
    //m_pEventSink->Notify(EC_COMPLETE, (LONG_PTR)S_OK,
    //0);
    //The presentation has ended. See End of Stream.
    Log("ProcessMessage %x", eMessage);
    m_bendStreaming = TRUE;
    CheckForEndOfStream();
    break;

  case MFVP_MESSAGE_FLUSH:
    //The EVR is flushing the data in its rendering pipeline. The presenter should discard any video frames that are scheduled for presentation.
    LOG_TRACE("ProcessMessage %x", eMessage);
    //delegate to avoid a weird deadlock with application-idle handler Flush();
    m_bFlush = TRUE;
    NotifyScheduler();
    break;

  case MFVP_MESSAGE_STEP:
    //Requests the presenter to step forward N frames. The presenter should discard the next N-1 frames and display the Nth frame. See Frame Stepping.
    Log("ProcessMessage %x", eMessage);
    break;

  case MFVP_MESSAGE_CANCELSTEP:
    //Cancels frame stepping.
    Log("ProcessMessage %x", eMessage);
    break;
  default:
    Log( "ProcessMessage: Unknown: %d", eMessage );
    break;
  }
  if ( FAILED(hr) ) 
  {
    Log( "ProcessMessage failed with 0x%x", hr );
  }
  LOG_TRACE("ProcessMessage done");
  return hr;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockStart( 
  /* [in] */ MFTIME hnsSystemTime,
  /* [in] */ LONGLONG llClockStartOffset)
{
  Log("OnClockStart");
  m_state = MP_RENDER_STATE_STARTED;
  Flush();
  //	m_bInputAvailable = TRUE;
  NotifyWorker();
  NotifyScheduler();
  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockStop( 
  /* [in] */ MFTIME hnsSystemTime)
{
  Log("OnClockStop");
  m_state = MP_RENDER_STATE_STOPPED;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockPause( 
  /* [in] */ MFTIME hnsSystemTime)
{
  Log("OnClockPause");
  m_bfirstFrame = TRUE;
  m_state = MP_RENDER_STATE_PAUSED;
  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockRestart( 
  /* [in] */ MFTIME hnsSystemTime)
{
  Log("OnClockRestart");
  m_state = MP_RENDER_STATE_STARTED;
  //m_bInputAvailable = TRUE;
  NotifyScheduler();
  NotifyWorker();
  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::OnClockSetRate( 
  /* [in] */ MFTIME hnsSystemTime,
  /* [in] */ float flRate)
{
  Log("OnClockSetRate: %f", flRate);
  m_fRate = flRate;
  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetService( 
  /* [in] */ REFGUID guidService,
  /* [in] */  REFIID riid,
  /* [iid_is][out] */ LPVOID *ppvObject)
{
  Log( "GetService" );
  LogGUID(guidService);
  LogIID(riid);
  HRESULT hr = MF_E_UNSUPPORTED_SERVICE;
  if( ppvObject == NULL ) 
  {
    hr = E_POINTER;
  } 

  else if( riid == __uuidof(IDirect3DDeviceManager9) ) 
  {
    hr = m_pDeviceManager->QueryInterface(riid, (void**)ppvObject);
  }
  else if( riid == IID_IMFVideoDeviceID) 
  {
    *ppvObject = static_cast<IMFVideoDeviceID*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFClockStateSink) 
  {
    *ppvObject = static_cast<IMFClockStateSink*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFTopologyServiceLookupClient) 
  {
    *ppvObject = static_cast<IMFTopologyServiceLookupClient*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFVideoPresenter ) 
  {
    *ppvObject = static_cast<IMFVideoPresenter*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFGetService ) 
  {
    *ppvObject = static_cast<IMFGetService*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFRateSupport ) 
  {
    *ppvObject = static_cast<IMFRateSupport*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFVideoDisplayControl ) 
  {
    *ppvObject = static_cast<IMFVideoDisplayControl*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IEVRTrustedVideoPlugin ) 
  {
    *ppvObject = static_cast<IEVRTrustedVideoPlugin*>( this );
    AddRef();
    hr = S_OK;
  } 
  else if( riid == IID_IMFVideoPositionMapper ) 
  {
    *ppvObject = static_cast<IMFVideoPositionMapper*>( this );
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
  if ( FAILED(hr) || (*ppvObject)==NULL) 
  {
    Log("GetService failed" );
  }
  return hr;
}


void MPEVRCustomPresenter::ReleaseCallBack()
{
  m_pCallback = NULL;
}
void MPEVRCustomPresenter::ReleaseSurfaces()
{
  Log("ReleaseSurfaces()");
  CAutoLock lock(this);
  HANDLE hDevice;
  CHECK_HR(m_pDeviceManager->OpenDeviceHandle(&hDevice), "failed opendevicehandle");
  IDirect3DDevice9* pDevice;
  CHECK_HR(m_pDeviceManager->LockDevice(hDevice, &pDevice, TRUE), "failed: lockdevice");
  //make sure that the surface is not in use anymore before we delete it.
  if ( m_pCallback != NULL )
  {
    m_pCallback->PresentImage(0,0,0,0,0,0);
  }
  Flush();
  m_iFreeSamples = 0;
  for ( int i=0; i<NUM_SURFACES; i++ ) 
  {
    //Log("Delete: %d, 0x%x", i, chains[i]);
    samples[i] = NULL;
    surfaces[i] = NULL;
    chains[i] = NULL;
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
  try
  {
    HRESULT hr;

    if (m_pCallback==NULL || pSurface==NULL)
      return E_FAIL;

    m_pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, FALSE);
    if(FAILED(hr = m_pD3DDev->StretchRect(pSurface, NULL, m_pVideoSurface, NULL, D3DTEXF_NONE)))
    {
      Log("vmr9:Paint: StretchRect failed %u\n",hr);
    }
    m_pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
    hr = m_pCallback->PresentImage(m_iVideoWidth, m_iVideoHeight, m_iARX,m_iARY, (DWORD)(IDirect3DTexture9*)m_pVideoTexture, (DWORD)(IDirect3DSurface9*)pSurface);
    return hr;
  }
  catch(...)
  {
    Log("Paint() exception");
  }
  return E_FAIL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_FramesDroppedInRenderer(int *pcFrames)
{
  if ( pcFrames == NULL ) return E_POINTER;
  //Log("evr:get_FramesDropped: %d", m_iFramesDropped);
  *pcFrames = m_iFramesDropped;
  return S_OK;
}
HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_FramesDrawn(int *pcFramesDrawn)
{
  if ( pcFramesDrawn == NULL ) return E_POINTER;
  //Log("evr:get_FramesDrawn: %d", m_iFramesDrawn);
  *pcFramesDrawn = m_iFramesDrawn;
  return S_OK;
}
HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_AvgFrameRate(int *piAvgFrameRate)
{
  //Log("evr:get_AvgFrameRate");
  return S_OK;
}
HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_Jitter(int *iJitter)
{
  /*Log("evr:get_Jitter: %d, deviation: %d", m_iJitter,
  (int)(m_hnsTotalDiff / m_iFramesDrawn) );*/
  if ( m_dwVariance != 0 && m_iFramesForStats > 0 ) 
  {
    *iJitter = sqrt((double)m_dwVariance/m_iFramesForStats);
  }
  else
  {
    *iJitter = 0;
  }
  return S_OK;
}
HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_AvgSyncOffset(int *piAvg)
{
  //Log("evr:get_AvgSyncOffset");
  return S_OK;
}
HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::get_DevSyncOffset(int *piDev)
{
  //Log("evr:get_DevSyncOffset");
  return S_OK;
}

STDMETHODIMP MPEVRCustomPresenter::GetNativeVideoSize( 
  /* [unique][out][in] */  SIZE *pszVideo,
  /* [unique][out][in] */  SIZE *pszARVideo) 
{
  Log("IMFVideoDisplayControl.GetNativeVideoSize()");
  pszVideo->cx=m_iVideoWidth;
  pszVideo->cy=m_iVideoHeight;
  pszARVideo->cx=m_iARX;
  pszARVideo->cy=m_iARY;

  return S_OK;
}

HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetIdealVideoSize( 
  /* [unique][out][in] */  SIZE *pszMin,
  /* [unique][out][in] */  SIZE *pszMax) 
{
  Log("IMFVideoDisplayControl.GetIdealVideoSize()");
  pszMin->cx = m_iVideoWidth;
  pszMin->cy = m_iVideoHeight;
  pszMax->cx = m_iVideoWidth;
  pszMax->cy = m_iVideoHeight;
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetVideoPosition( 
  /* [unique][in] */  const MFVideoNormalizedRect *pnrcSource,
  /* [unique][in] */  const LPRECT prcDest) 
{
  Log("IMFVideoDisplayControl.SetVideoPosition()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetVideoPosition( 
  /* [out] */  MFVideoNormalizedRect *pnrcSource,
  /* [out] */  LPRECT prcDest) 
{
  //Log("IMFVideoDisplayControl.GetVideoPosition()");
  pnrcSource->left = 0;
  pnrcSource->top = 0;
  pnrcSource->right = m_iVideoWidth;
  pnrcSource->bottom = m_iVideoHeight;

  prcDest->left = 0;
  prcDest->top = 0;
  prcDest->right = m_iVideoWidth;
  prcDest->bottom = m_iVideoHeight;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetAspectRatioMode( 
  /* [in] */ DWORD dwAspectRatioMode) 
{
  Log("IMFVideoDisplayControl.SetAspectRatioMode()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetAspectRatioMode( 
  /* [out] */  DWORD *pdwAspectRatioMode) 
{
  Log("IMFVideoDisplayControl.GetAspectRatioMode()");
  *pdwAspectRatioMode = VMR_ARMODE_NONE;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetVideoWindow( 
  /* [in] */  HWND hwndVideo) 
{
  Log("IMFVideoDisplayControl.SetVideoWindow()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetVideoWindow( 
  /* [out] */  HWND *phwndVideo) 
{
  Log("IMFVideoDisplayControl.GetVideoWindow()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::RepaintVideo( void) 
{
  Log("IMFVideoDisplayControl.RepaintVideo()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetCurrentImage( 
  /* [out][in] */  BITMAPINFOHEADER *pBih,
  /* [size_is][size_is][out] */ BYTE **pDib,
  /* [out] */  DWORD *pcbDib,
  /* [unique][out][in] */  LONGLONG *pTimeStamp) 
{
  Log("IMFVideoDisplayControl.GetCurrentImage()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetBorderColor( 
  /* [in] */ COLORREF Clr)
{
  Log("IMFVideoDisplayControl.SetBorderColor()");
  return E_NOTIMPL;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetBorderColor( 
  /* [out] */  COLORREF *pClr) 
{
  Log("IMFVideoDisplayControl.GetBorderColor()");
  if(pClr) *pClr = 0;
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetRenderingPrefs( 
  /* [in] */ DWORD dwRenderFlags) 
{
  Log("IMFVideoDisplayControl.SetRenderingPrefs()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetRenderingPrefs( 
  /* [out] */  DWORD *pdwRenderFlags) 
{
  Log("IMFVideoDisplayControl.GetRenderingPrefs()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::SetFullscreen( 
  /* [in] */ BOOL fFullscreen) 
{
  Log("IMFVideoDisplayControl.SetFullscreen()");
  return S_OK;
}


HRESULT STDMETHODCALLTYPE MPEVRCustomPresenter::GetFullscreen( 
  /* [out] */  BOOL *pfFullscreen) 
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
  //Log("IMFVideoPositionMapper.MapOutputCoordinateToInputStream(%f,%f)",xOut,yOut);
  *pxIn=xOut;
  *pyIn=yOut;
  return S_OK;
}
