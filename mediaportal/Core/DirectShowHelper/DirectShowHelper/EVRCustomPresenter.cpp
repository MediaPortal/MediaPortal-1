/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#define TIME_LOCK(obj, crit, name)  \
ULONGLONG then = GetTickCount64(); \
CAutoLock lock(obj); \
	ULONGLONG diff = GetTickCount64() - then; \
	if ( diff >= crit ) { \
	  Log("Critical lock time for %s was %d ms", name, diff ); \
	}

void Log(const char *fmt, ...) ;
HRESULT __fastcall UnicodeToAnsi(LPCOLESTR pszW, LPSTR* ppszA);





void LogIID( REFIID riid ) {
	LPOLESTR str;
	LPSTR astr;
	StringFromIID(riid, &str); 
	UnicodeToAnsi(str, &astr);
	Log("riid: %s", astr);
	CoTaskMemFree(str);
}

void LogGUID( REFGUID guid ) {
	LPOLESTR str;
	LPSTR astr;
	str = (LPOLESTR)CoTaskMemAlloc(200);
	StringFromGUID2(guid, str, 200); 
	UnicodeToAnsi(str, &astr);
	Log("guid: %s", astr);
	CoTaskMemFree(str);
}

//avoid dependency into MFGetService, aparently only availabe on vista
HRESULT MyGetService(IUnknown* punkObject, REFGUID guidService,
    REFIID riid, LPVOID* ppvObject ) 
{
	if ( ppvObject == NULL ) return E_POINTER;
	HRESULT hr;
	IMFGetService* pGetService;
	hr = punkObject->QueryInterface(__uuidof(IMFGetService),
		(void**)&pGetService);
	if ( SUCCEEDED(hr) ) {
		hr = pGetService->GetService(guidService, riid, ppvObject);
		SAFE_RELEASE(pGetService);
	}
	return hr;
}

void CALLBACK TimerCallback(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
	SchedulerParams *p = (SchedulerParams*)dwUser;
	p->eHasWork.Set();
}

UINT CALLBACK SchedulerThread(void* param)
{
	SchedulerParams *p = (SchedulerParams*)param;
	LONGLONG hnsSampleTime;
	while ( true ) 
	{
		//Log("Scheduler callback");
		p->csLock.Lock();
		if ( p->bDone )//&& !p->bWorkScheduled ) 
		{
			Log("Scheduler done, deleting data");
			delete p;
			p->csLock.Unlock();
			return 0;
		}
		
		p->bWorkScheduled = FALSE;
		p->pPresenter->CheckForScheduledSample(&hnsSampleTime);
		//Log("Got scheduling time: %I64d", hnsSampleTime);
		if ( hnsSampleTime > 0 ) { 
			p->bWorkScheduled = TRUE;
			//Sleep(hnsSampleTime/10000);
			timeSetEvent(hnsSampleTime/10000,1,
				TimerCallback, (DWORD)param, TIME_ONESHOT);
			/*Log( "Next event: %d hns", hnsSampleTime );*/
		}
		else
		{
			p->bWorkScheduled = FALSE;
		} 
		p->csLock.Unlock();
		while ( !p->eHasWork.Wait() );
	}
	return -1;
}



EVRCustomPresenter::EVRCustomPresenter( IVMR9Callback* pCallback, IDirect3DDevice9* direct3dDevice, HMONITOR monitor)
: m_refCount(1)
{
    if (m_pMFCreateVideoSampleFromSurface!=NULL)
    {
        Log("----------v0.37---------------------------");
        m_hMonitor=monitor;
        m_pD3DDev=direct3dDevice;
        HRESULT hr = m_pDXVA2CreateDirect3DDeviceManager9(
            &m_iResetToken, &m_pDeviceManager);
        if ( FAILED(hr) ) {
            Log( "Could not create DXVA2 Device Manager" );
        } else {
            m_pDeviceManager->ResetDevice(direct3dDevice, m_iResetToken);
        }
        m_pCallback=pCallback;
        m_surfaceCount=0;
		m_bInputAvailable = FALSE;
		m_bendStreaming = FALSE;
        //m_UseOffScreenSurface=false;
        m_fRate = 1.0f;
        //TODO: use ZeroMemory
        /*for ( int i=0; i<NUM_SURFACES; i++ ) {
            chains[i] = NULL;
            surfaces[i] = NULL;
            //samples[i] = NULL;
        }*/
    }
}

EVRCustomPresenter::~EVRCustomPresenter()
{
	if (m_pCallback!=NULL)
		m_pCallback->PresentImage(0,0,0,0,0);
	StopScheduler();
	ReleaseSurfaces();
	m_pMediaType.Release();
	HRESULT hr;
	m_pDeviceManager =  NULL;
	Log("Done");
	m_vFreeSamples.clear();
}	

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::GetParameters( 
    /* [out] */ __RPC__out DWORD *pdwFlags,
    /* [out] */ __RPC__out DWORD *pdwQueue)
{
	Log("GetParameters");
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::Invoke( 
    /* [in] */ __RPC__in_opt IMFAsyncResult *pAsyncResult)
{
	Log("Invoke");
	return S_OK;
}


// IUnknown
HRESULT EVRCustomPresenter::QueryInterface( 
        REFIID riid,
        void** ppvObject)
{
    HRESULT hr = E_NOINTERFACE;
Log( "QueryInterface"  );
LogIID( riid );
    if( ppvObject == NULL ) {
        hr = E_POINTER;
    } 
	else if( riid == IID_IMFVideoDeviceID) {
		*ppvObject = static_cast<IMFVideoDeviceID*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFTopologyServiceLookupClient) {
		*ppvObject = static_cast<IMFTopologyServiceLookupClient*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFVideoPresenter) {
		*ppvObject = static_cast<IMFVideoPresenter*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFGetService) {
		*ppvObject = static_cast<IMFGetService*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IQualProp) {
		*ppvObject = static_cast<IQualProp*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFRateSupport) {
		*ppvObject = static_cast<IMFRateSupport*>( this );
        AddRef();
        hr = S_OK;
    } 
    else if( riid == IID_IUnknown ) {
        *ppvObject = 
            static_cast<IUnknown*>( 
			static_cast<IMFVideoDeviceID*>( this ) );
        AddRef();
        hr = S_OK;    
    }
	if ( FAILED(hr) ) {
		Log( "QueryInterface failed" );
	}
    return hr;
}

ULONG EVRCustomPresenter::AddRef()
{
    Log("EVRCustomPresenter::AddRef()");
    return InterlockedIncrement(& m_refCount);
}

ULONG EVRCustomPresenter::Release()
{
    Log("EVRCustomPresenter::Release()");
    ULONG ret = InterlockedDecrement(& m_refCount);
    if( ret == 0 )
    {
        Log("EVRCustomPresenter::Cleanup()");
        delete this;
    }

    return ret;
}

void EVRCustomPresenter::ResetStatistics()
{
			m_bfirstFrame = true;
			m_bfirstInput = true;
			m_iFramesDrawn = 0;
			m_iFramesDropped = 0;
			m_hnsLastFrameTime = 0;
			m_iJitter = 0;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::GetSlowestRate( 
    /* [in] */ MFRATE_DIRECTION eDirection,
    /* [in] */ BOOL fThin,
    /* [out] */ __RPC__out float *pflRate)
{
	Log("GetSlowestRate");
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::GetFastestRate( 
    /* [in] */ MFRATE_DIRECTION eDirection,
    /* [in] */ BOOL fThin,
    /* [out] */ __RPC__out float *pflRate)
{
	Log("GetFastestRate");
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::IsRateSupported( 
    /* [in] */ BOOL fThin,
    /* [in] */ float flRate,
    /* [unique][out][in] */ __RPC__inout_opt float *pflNearestSupportedRate)
{
	Log("IsRateSupported");
	return S_OK;
}



HRESULT EVRCustomPresenter::GetDeviceID(IID* pDeviceID)
{
	Log("GetDeviceID");
    if (pDeviceID == NULL)
    {
        return E_POINTER;
    }
    *pDeviceID = __uuidof(IDirect3DDevice9);
    return S_OK;
}

HRESULT EVRCustomPresenter::InitServicePointers(IMFTopologyServiceLookup *pLookup)
{
	Log("InitServicePointers");
	HRESULT hr = S_OK;
    DWORD   cCount = 0;

    // Ask for the mixer
    cCount = 1;
    hr = pLookup->LookupService(      
        MF_SERVICE_LOOKUP_GLOBAL,   // Not used
        0,                          // Reserved
        MR_VIDEO_MIXER_SERVICE,    // Service to look up
		__uuidof(IMFTransform),         // Interface to look up
        (void**)&m_pMixer,          // Receives the pointer.
        &cCount                     // Number of pointers
        );

	if ( FAILED(hr) ) {
		Log( "ERR: Could not get IMFTransform interface" );
	} else {
		// If there is no clock, cCount is zero.
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
        &cCount                     // Number of pointers
        );

	if ( FAILED(hr) ) {
		Log( "ERR: Could not get IMFClock interface" );
	} else {
		// If there is no clock, cCount is zero.
		Log( "Found clock: %d", cCount );
		ASSERT(cCount == 0 || cCount == 1);
	}

    // Ask for the event-sink
    cCount = 1;
    hr = pLookup->LookupService(      
        MF_SERVICE_LOOKUP_GLOBAL,   // Not used
        0,                          // Reserved
        MR_VIDEO_RENDER_SERVICE,    // Service to look up
		__uuidof(IMediaEventSink),         // Interface to look up
        (void**)&m_pEventSink,          // Receives the pointer.
        &cCount                     // Number of pointers
        );

	if ( FAILED(hr) ) {
		Log( "ERR: Could not get IMediaEventSink interface" );
	} else {
		// If there is no clock, cCount is zero.
		Log( "Found event sink: %d", cCount );
		ASSERT(cCount == 0 || cCount == 1);
	}

	// TODO: Get other interfaces.
    /* ... */

   return S_OK;
}

HRESULT EVRCustomPresenter::ReleaseServicePointers() 
{
	Log("ReleaseServicePointers");
	m_pMediaType.Release();
	m_pMixer.Release();
	m_pClock.Release();
	m_pEventSink.Release();
	return S_OK;
}

HRESULT EVRCustomPresenter::GetCurrentMediaType(IMFVideoMediaType** ppMediaType)
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

done:

	Log( "GetCurrentMediaType done" );
    return hr;
}

HRESULT EVRCustomPresenter::TrackSample(IMFSample *pSample)
{
    HRESULT hr = S_OK;
    IMFTrackedSample *pTracked = NULL;

    CHECK_HR(hr = pSample->QueryInterface(__uuidof(IMFTrackedSample), (void**)&pTracked), "Cannot get Interface IMFTrackedSample");
    CHECK_HR(hr = pTracked->SetAllocator(this, NULL), "SetAllocator failed"); 

done:
    SAFE_RELEASE(pTracked);
    return hr;
}

HRESULT EVRCustomPresenter::GetTimeToSchedule(CComPtr<IMFSample> pSample, LONGLONG *phnsDelta) 
{
	LONGLONG hnsPresentationTime = 0; // Target presentation time
	LONGLONG hnsTimeNow = 0;          // Current presentation time
	MFTIME   hnsSystemTime = 0;       // System time
	LONGLONG hnsDelta = 0;
	HRESULT  hr;

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
		hr = m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime);
	}
	else
	{
		Log("Could not get sample time from %p!", pSample);
		return hr;
	}


	// Calculate the amount of time until the sample's presentation
	// time. A negative value means the sample is late.
	hnsDelta = hnsPresentationTime - hnsTimeNow;
	if (hnsDelta > 10000000 )
	{
		Log("dangerous and unlikely time to schedule [%p]: %I64d. scheduled time: %I64d, now: %I64d",
			pSample, hnsDelta, hnsPresentationTime, hnsTimeNow);
	}
	//Log("Calculated delta: %I64d (rate: %f)", hnsDelta, m_fRate);
	if ( m_fRate != 1.0f && m_fRate != 0.0f )
		*phnsDelta = ((float)hnsDelta) / m_fRate;
	else
		*phnsDelta = hnsDelta;
	return hr;
}

HRESULT EVRCustomPresenter::SetMediaType(CComPtr<IMFMediaType> pType)
{
	if (pType == NULL) 
	{
		m_pMediaType.Release();
		return S_OK;
	}

	HRESULT hr = S_OK;


	LARGE_INTEGER u64;
	UINT32 u32;
	
	CHECK_HR(pType->GetUINT64(MF_MT_FRAME_SIZE, (UINT64*)&u64), "Getting Framesize failed!");
	m_iVideoWidth = u64.HighPart;
	m_iVideoHeight = u64.LowPart;
	//use video size as default value for aspect ratios
	m_iARX = m_iVideoWidth;
	m_iARY = m_iVideoHeight;
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
				m_iARX = 16;
				m_iARY = 9;
				break;
			case MFVideoSrcContentHintFlag_235_1:
				Log("Source is 2.35:1 within 16:9 or 4:3");
				m_iARX = 47;
				m_iARY = 20;
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
			m_iARX = vheader->dwPictAspectRatioX;
			m_iARY = vheader->dwPictAspectRatioY;
			pType->FreeRepresentation(FORMAT_VideoInfo2, (void*)pAMMediaType);
		}
	}

	Log( "New format: %dx%d, Ratio: %d:%d",
		m_iVideoWidth, m_iVideoHeight, m_iARX, m_iARY );

	GUID subtype;
	CHECK_HR(pType->GetGUID(MF_MT_SUBTYPE, &subtype), "Could not get subtype");
	LogGUID( subtype );
	m_pMediaType = pType;
	return hr;
}

void EVRCustomPresenter::ReAllocSurfaces()
{
	Log("ReallocSurfaces");
	//TIME_LOCK(this, 20, "ReAllocSurfaces")
	ReleaseSurfaces();

	// set the presentation parameters
	D3DPRESENT_PARAMETERS d3dpp;
	ZeroMemory(&d3dpp, sizeof(d3dpp));
	d3dpp.BackBufferWidth = m_iVideoWidth;
	d3dpp.BackBufferHeight = m_iVideoHeight;
	d3dpp.BackBufferCount = 1;
	//TODO check media type for correct format!
	d3dpp.BackBufferFormat = D3DFMT_A8R8G8B8;
	d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
	d3dpp.Windowed = true;
	d3dpp.EnableAutoDepthStencil = false;
	d3dpp.AutoDepthStencilFormat = D3DFMT_R8G8B8;
	d3dpp.FullScreen_RefreshRateInHz = D3DPRESENT_RATE_DEFAULT;
	d3dpp.PresentationInterval = D3DPRESENT_INTERVAL_DEFAULT;


	HANDLE hDevice;
	IDirect3DDevice9* pDevice;
	CHECK_HR(m_pDeviceManager->OpenDeviceHandle(&hDevice), "Cannot open device handle");
	CHECK_HR(m_pDeviceManager->LockDevice(hDevice, &pDevice, TRUE), "Cannot lock device");
	HRESULT hr;
	for ( int i=0; i<NUM_SURFACES; i++ ) {
		//Log( "Creating chain %d...", i );
		hr = pDevice->CreateAdditionalSwapChain(&d3dpp, &chains[i]);
		if (FAILED(hr)) {
			Log("Chain creation failed with 0x%x", hr);
			return;
		}
		hr = chains[i]->GetBackBuffer(0, D3DBACKBUFFER_TYPE_MONO, &surfaces[i]);
		if (FAILED(hr)) {
			Log("Could not get back buffer: 0x%x", hr);
			return;
		}

		hr = m_pMFCreateVideoSampleFromSurface(surfaces[i],
			&samples[i]);
		if (FAILED(hr)) {
			Log("CreateVideoSampleFromSurface failed: 0x%x", hr);
			return;
		}
		//Log("Adding sample: 0x%x", samples[i]);
		m_vFreeSamples.push_back(samples[i]);
		//Log("Chain created");
	} 
	CHECK_HR(m_pDeviceManager->UnlockDevice(hDevice, FALSE), "failed: Unlock device");
	Log("Releasing device: %d", pDevice->Release());
	CHECK_HR(m_pDeviceManager->CloseDeviceHandle(hDevice), "failed: CloseDeviceHandle");
	Log("ReallocSurfaces done");
}


void LogMediaTypes(CComPtr<IMFTransform> pMixer)
{
	HRESULT hr=S_OK;
    DWORD iTypeIndex = 0;
	CComPtr<IMFMediaType> pType=NULL;
	CComPtr<IMFVideoMediaType> pVideo=NULL;
    while ((hr != MF_E_NO_MORE_TYPES))
    {
        hr = pMixer->GetOutputAvailableType(0, iTypeIndex++, &pType);
        if (FAILED(hr))
        {
            break;
        }

		CHECK_HR(hr = pType->QueryInterface(
			__uuidof(IMFVideoMediaType), (void**)&pVideo),
			"Query interface failed in LogMediaType");

		const MFVIDEOFORMAT *f = pVideo->GetVideoFormat();

		Log( "Videotype: %dx%d",f->videoInfo.dwWidth,
			f->videoInfo.dwHeight);

	}
}

HRESULT EVRCustomPresenter::CreateProposedOutputType(IMFMediaType* pMixerType, IMFMediaType** pType)
{
   	HRESULT				hr;
   	AM_MEDIA_TYPE*		pAMMedia = NULL;
   	LARGE_INTEGER		i64Size;
   	MFVIDEOFORMAT*		VideoFormat;
	IMFVideoMediaType* pMediaType;
   
	CHECK_HR (pMixerType->GetRepresentation  (FORMAT_MFVideoFormat, (void**)&pAMMedia), "failed: GetRepresentation");
   	VideoFormat = (MFVIDEOFORMAT*)pAMMedia->pbFormat;
   	hr = m_pMFCreateVideoMediaType  (VideoFormat, &pMediaType);
   
   	if (SUCCEEDED (hr))
   	{
   		i64Size.HighPart = VideoFormat->videoInfo.dwWidth;
   		i64Size.LowPart	 = VideoFormat->videoInfo.dwHeight;
   		pMediaType->SetUINT64 (MF_MT_FRAME_SIZE, i64Size.QuadPart);
   
	    pMediaType->SetUINT32 (MF_MT_PAN_SCAN_ENABLED, 0);
	  
	  i64Size.HighPart = 1;
	  i64Size.LowPart  = 1;
	  pMediaType->SetUINT64 (MF_MT_PIXEL_ASPECT_RATIO, i64Size.QuadPart);
	  
    }
	pMixerType->FreeRepresentation (FORMAT_MFVideoFormat, (void*)pAMMedia);
   	pMediaType->QueryInterface (__uuidof(IMFMediaType), (void**) pType);
	pMediaType->Release();
   	return hr;
  }  

HRESULT EVRCustomPresenter::RenegotiateMediaOutputType()
{
	Log("RenegotiateMediaOutputType");
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
			
            hr = SetMediaType(pType);
			//m_pMediaType = pType;
			if (SUCCEEDED(hr))
			{
				ReAllocSurfaces();
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
                SetMediaType(NULL);
            }
        }

        if (SUCCEEDED(hr))
        {
            fFoundMediaType = TRUE;
        }
    }
    return hr;
}

HRESULT EVRCustomPresenter::GetFreeSample(CComPtr<IMFSample> &ppSample) 
{
	CAutoLock lock (&m_lockSamples);
	//TODO hold lock?
	//Log( "Trying to get free sample, size: %d", m_vFreeSamples.size());
	if ( m_vFreeSamples.size() == 0 ) return E_FAIL;
	ppSample = m_vFreeSamples.back();
	m_vFreeSamples.pop_back();
	
	return S_OK;
}

void EVRCustomPresenter::Flush()
{
	//CAutoLock lock(this);
	//TIME_LOCK(&m_lockSamples, 10, "Flush")
	//Log( "Flushing: size=%d", m_vScheduledSamples.size() );
	while ( m_vScheduledSamples.size()>0 )
	{
		CComPtr<IMFSample> pSample = m_vScheduledSamples.front();
		m_vScheduledSamples.pop();
		ReturnSample(pSample, FALSE);
	}
}

void EVRCustomPresenter::ReturnSample(CComPtr<IMFSample> pSample, BOOL bCheckForWork)
{
	//CAutoLock lock(this);
	TIME_LOCK(&m_lockSamples, 5, "ReturnSample")
	//Log( "Sample returned: now having %d samples", m_vFreeSamples.size()+1);
	m_vFreeSamples.push_back(pSample);
	//todo, if queue was empty, do something?
	if ( m_vScheduledSamples.size() == 0 ) CheckForEndOfStream();
	if ( bCheckForWork && m_vFreeSamples.size() == 1  ) {
		//Log("New Sample available; trying to process input..." );
		ProcessInputNotify();
	}
}

HRESULT EVRCustomPresenter::PresentSample(CComPtr<IMFSample> pSample)
{
    HRESULT hr = S_OK;
    IMFMediaBuffer* pBuffer = NULL;
    IDirect3DSurface9* pSurface = NULL;
    //IDirect3DSwapChain9* pSwapChain = NULL;
//Log("Presenting sample");
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

        // Present the swap surface
		Paint(pSurface);

		// Calculate offset to scheduled time
		LONGLONG hnsTimeNow, hnsSystemTime, hnsTimeScheduled;
		m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime);

		pSample->GetSampleTime(&hnsTimeScheduled);
		if ( hnsTimeScheduled > 0 )
		{
			LONGLONG deviation = hnsTimeNow - hnsTimeScheduled;
			if ( deviation < 0 ) deviation = -deviation;
			m_hnsTotalDiff += deviation;
		}
		if ( m_hnsLastFrameTime != 0 )
		{
			LONGLONG hnsDiff = hnsTimeNow - m_hnsLastFrameTime;
			//todo: expected: standard deviation!
			m_iJitter = hnsDiff / 10000;
		}
		m_hnsLastFrameTime = hnsTimeNow;
		m_iFramesDrawn++;
        /*CHECK_HR(hr = pSwapChain->Present(NULL, NULL, NULL, NULL, 0),
			"failed: Present");*/
    }

done:
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


HRESULT EVRCustomPresenter::CheckForScheduledSample(LONGLONG *pNextSampleTime)
{
	HRESULT hr = S_OK;
//	CAutoLock lock(this);
	TIME_LOCK(&m_lockSamples, 1, "CheckForScheduledSample")
	if ( m_vScheduledSamples.size() == 0 ) {
		//Log("Nothing in queue. False alert?");
		//ProcessInputNotify();
	} else {
	//Log("Checking for scheduled sample (size: %d)",
	//	m_vScheduledSamples.size());
	while ( m_vScheduledSamples.size() > 0 ) {
		CComPtr<IMFSample> pSample = m_vScheduledSamples.front();
		CHECK_HR(hr=GetTimeToSchedule(pSample, pNextSampleTime), "Couldn't get time to schedule!");
		//Log( "Time to schedule: %I64d", *pNextSampleTime );
		//if we are ahead only 1 ms, present this sample anyway
		//else sleep for some time
		if ( *pNextSampleTime > 10000 ) {
			return hr;
		}
		m_vScheduledSamples.pop();
		if ( *pNextSampleTime < -400000 ) {
			//skip!
			Log( "skipping frame, behind %I64d hns", -*pNextSampleTime );
			m_iFramesDropped++;
		} else {
			CHECK_HR(PresentSample(pSample), "PresentSample failed");
		}
		ReturnSample(pSample, TRUE);
	}
	}
	*pNextSampleTime = 0;
	return hr;
} 

void EVRCustomPresenter::StartScheduler()
{
	//CAutoLock lock(this);
	Log("Starting scheduler!");
	m_schedulerParams = new SchedulerParams();
	m_schedulerParams->pPresenter = this;
	m_schedulerParams->bDone = FALSE;
	m_schedulerParams->bWorkScheduled = FALSE;
	m_bSchedulerRunning = TRUE;
	NotifyScheduler();
	m_hScheduler = (HANDLE)_beginthreadex(NULL, 0, SchedulerThread,
		m_schedulerParams, 0, &m_uThreadId);
}

void EVRCustomPresenter::StopScheduler()
{
	{
		//CAutoLock lock(this);
		if ( !m_bSchedulerRunning ) return;
		Log( "Ending Scheduler (getting lock)" );
		CAutoLock slock(&m_schedulerParams->csLock);
		m_schedulerParams->bDone = TRUE;
		NotifyScheduler();
		m_bSchedulerRunning = FALSE;
	}
	Log("Waiting for scheduler to end...");
	WaitForSingleObject(m_hScheduler, INFINITE);
	Log("Waiting done");
	CloseHandle(m_hScheduler);
}

void EVRCustomPresenter::NotifyScheduler()
{
	//Log( "NotifyScheduler()" );
	if ( m_bSchedulerRunning ){
		CAutoLock slock(&m_schedulerParams->csLock);
		if ( !m_schedulerParams->bWorkScheduled ) {
			m_schedulerParams->bWorkScheduled = TRUE;
			m_schedulerParams->eHasWork.Set();
		}
	} else {
		Log("Scheduler is already shut down");
	}
	/*if ( !m_bSchedulerRunning ) {
		Log("ERROR: Scheduler not running!");
		return;
	} 
	m_schedulerParams->eHasWork.Set();*/
}

void EVRCustomPresenter::ScheduleSample(CComPtr<IMFSample> pSample)
{
//	CAutoLock lock(this);
	m_lockSamples.Lock();
	//Log( "Scheduling Sample, size: %d", m_vScheduledSamples.size() );
	m_vScheduledSamples.push(pSample);
	if ( m_vScheduledSamples.size() == 1 )
	{
		m_lockSamples.Unlock();
		NotifyScheduler();
	}
	else
	{
		m_lockSamples.Unlock();
	}
}

BOOL EVRCustomPresenter::CheckForEndOfStream()
{
	//CAutoLock lock(this);
	//Log("CheckForEndOfStream");
	if ( !m_bendStreaming )
	{
		//Log("No message from mixer yet");
		return FALSE;
	}
	//samples pending
	if ( m_vScheduledSamples.size() > 0 )
	{
		//Log("Still having scheduled samples");
		return FALSE;
	}
	if ( m_pEventSink ) 
	{
		Log("Sending completion message");
		m_pEventSink->Notify(EC_COMPLETE, (LONG_PTR)S_OK,
		0);
	}
	m_bendStreaming = FALSE;
	return TRUE;
}

HRESULT EVRCustomPresenter::ProcessInputNotify()
{
	//CAutoLock lock(this);
	//TIME_LOCK(this, 1, "ProcessInputNotify")
	HRESULT hr=S_OK;
	if ( !m_bInputAvailable )
	{
		Log("No input available yet!");
		return S_OK;
	}
	if ( m_pClock ) {
		MFCLOCK_STATE state;
		m_pClock->GetState(0, &state);
		if ( state == MFCLOCK_STATE_PAUSED && !m_bfirstInput) 
		{
			Log( "Not processing data in pause mode" );
			return S_OK;
		}
	}
	//try to process as many samples as possible:
	BOOL bhasMoreSamples = true;
	do {
		CComPtr<IMFSample> sample;
		hr = GetFreeSample(sample);
		if ( FAILED(hr) ) {
			//Log( "No free sample available" );
			return S_OK;
		}
		DWORD dwStatus;
		MFT_OUTPUT_DATA_BUFFER outputSamples[1];
		outputSamples[0].dwStreamID = 0; 
		outputSamples[0].dwStatus = 0; 
		outputSamples[0].pSample = sample; 
		outputSamples[0].pEvents = NULL;
		hr = m_pMixer->ProcessOutput(0, 1, outputSamples,
			&dwStatus);
		SAFE_RELEASE(outputSamples[0].pEvents);
		LONGLONG latency;
		if ( SUCCEEDED( hr ) ) {
			//Log("Processoutput succeeded, status: %d", dwStatus);
			//Log("Scheduling sample");
			m_bfirstInput = false;
			ScheduleSample(sample);
		} else {
			ReturnSample(sample, FALSE);
			switch ( hr ) {
				case MF_E_TRANSFORM_NEED_MORE_INPUT:
					//we are done for now
					hr = S_OK;
					bhasMoreSamples = false;
					//Log("Need more input...");
					m_bInputAvailable = FALSE;
					CheckForEndOfStream();
					break;
				case MF_E_TRANSFORM_STREAM_CHANGE:
					Log( "Unhandled: transform_stream_change");
					break;
				case MF_E_TRANSFORM_TYPE_NOT_SET:
					//no errors, just infos why it didn't succeed
					Log( "ProcessOutput: change of type" );
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

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::ProcessMessage( 
            MFVP_MESSAGE_TYPE eMessage,
            ULONG_PTR ulParam)
{
	HRESULT hr = S_OK;
	//Log( "Processmessage: %d, %p", eMessage, ulParam );
	switch ( eMessage ) {
		case MFVP_MESSAGE_INVALIDATEMEDIATYPE:
			Log( "Negotiate Media type" );
			//The mixer's output media type is invalid. The presenter should negotiate a new media type with the mixer. See Negotiating Formats.
			hr = RenegotiateMediaOutputType();
			break;

		case MFVP_MESSAGE_BEGINSTREAMING:
			//Streaming has started. No particular action is required by this message, but you can use it to allocate resources.
			Log("ProcessMessage %x", eMessage);
			m_bendStreaming = FALSE;
			ResetStatistics();
			StartScheduler();
			//ProcessInputNotify();
			break;

		case MFVP_MESSAGE_ENDSTREAMING:
			//Streaming has ended. Release any resources that you allocated in response to the MFVP_MESSAGE_BEGINSTREAMING message.
			Log("ProcessMessage %x", eMessage);
			//m_bendStreaming = TRUE;
			StopScheduler();
			DWORD flags;
			CHECK_HR(m_pMixer->GetOutputStatus(&flags), "nadamixa");
			if ( flags & MFT_OUTPUT_STATUS_SAMPLE_READY )
				Log("Ending with sample ready");
			else
				Log("Ending without samples ready");
			//Flush();
			break;

		case MFVP_MESSAGE_PROCESSINPUTNOTIFY:
			//The mixer has received a new input sample and might be able to generate a new output frame. The presenter should call IMFTransform::ProcessOutput on the mixer. See Processing Output.
			m_bInputAvailable = TRUE;
			hr = ProcessInputNotify();
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
			Log("ProcessMessage %x", eMessage);
			Flush();
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
	if ( FAILED(hr) ) {
		Log( "ProcessMessage failed with 0x%x", hr );
	}
	return hr;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::OnClockStart( 
    /* [in] */ MFTIME hnsSystemTime,
    /* [in] */ LONGLONG llClockStartOffset)
{
	Log("OnClockStart");
	Flush();
	ProcessInputNotify();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::OnClockStop( 
    /* [in] */ MFTIME hnsSystemTime)
{
	Log("OnClockStop");
	return S_OK;
}


HRESULT STDMETHODCALLTYPE EVRCustomPresenter::OnClockPause( 
    /* [in] */ MFTIME hnsSystemTime)
{
	Log("OnClockPause");
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::OnClockRestart( 
    /* [in] */ MFTIME hnsSystemTime)
{
	Log("OnClockRestart");
	ProcessInputNotify();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::OnClockSetRate( 
    /* [in] */ MFTIME hnsSystemTime,
    /* [in] */ float flRate)
{
	Log("OnClockSetRate: %f", flRate);
	//m_fRate = flRate;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::GetService( 
    /* [in] */ REFGUID guidService,
    /* [in] */  REFIID riid,
    /* [iid_is][out] */ LPVOID *ppvObject)
{
	Log( "GetService" );
	LogGUID(guidService);
	LogIID(riid);
	HRESULT hr = MF_E_UNSUPPORTED_SERVICE;
    if( ppvObject == NULL ) {
        hr = E_POINTER;
    } 
	
	else if( riid == __uuidof(IDirect3DDeviceManager9) ) {
		hr = m_pDeviceManager->QueryInterface(riid, (void**)ppvObject);
	}
	else if( riid == IID_IMFVideoDeviceID) {
		*ppvObject = static_cast<IMFVideoDeviceID*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFClockStateSink) {
		*ppvObject = static_cast<IMFClockStateSink*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFTopologyServiceLookupClient) {
		*ppvObject = static_cast<IMFTopologyServiceLookupClient*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFVideoPresenter) {
		*ppvObject = static_cast<IMFVideoPresenter*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFGetService) {
		*ppvObject = static_cast<IMFGetService*>( this );
        AddRef();
        hr = S_OK;
    } 
	else if( riid == IID_IMFRateSupport) {
		*ppvObject = static_cast<IMFRateSupport*>( this );
        AddRef();
        hr = S_OK;
    } 
	if ( FAILED(hr) ) {
		Log("GetService failed" );
	}
	return hr;
}






void EVRCustomPresenter::ReleaseCallBack()
{
	m_pCallback=NULL;
}
void EVRCustomPresenter::ReleaseSurfaces()
{
	//CAutoLock lock(this);
	Log("ReleaseSurfaces()");
	HANDLE hDevice;
	CHECK_HR(m_pDeviceManager->OpenDeviceHandle(&hDevice), "failed opendevicehandle");
	IDirect3DDevice9* pDevice;
	CHECK_HR(m_pDeviceManager->LockDevice(hDevice, &pDevice, TRUE), "failed: lockdevice");
	if ( m_pCallback != NULL )
		m_pCallback->PresentImage(0,0,0,0,0);
	Flush();
	m_vFreeSamples.clear();
	for ( int i=0; i<NUM_SURFACES; i++ ) {
		//Log("Delete: %d, 0x%x", i, chains[i]);
		samples[i] = NULL;
		surfaces[i] = NULL;
		chains[i] = NULL;
	}

	/*if (m_pCallback!=NULL)
		m_pCallback->PresentImage(0,0,0,0,0);*/
	m_pDeviceManager->UnlockDevice(hDevice, FALSE);
	Log("Releasing device");
	pDevice->Release();
	m_pDeviceManager->CloseDeviceHandle(hDevice);
	Log("ReleaseSurfaces() done");
}

void EVRCustomPresenter::Paint(CComPtr<IDirect3DSurface9> pSurface)
{
	try
	{
		if (m_pCallback!=NULL)
		{
			if (pSurface!=NULL)
			{
				DWORD dwPtr;
				void *pContainer = NULL;
				pSurface->GetContainer(IID_IDirect3DTexture9,&pContainer);
				if (pContainer!=NULL)
				{
					LPDIRECT3DTEXTURE9 pTexture=(LPDIRECT3DTEXTURE9)pContainer;

					dwPtr=(DWORD)(pTexture);
					if ( dwPtr == 0 ) Log("WARNING: null-texture-pointer!");
					m_pCallback->PresentImage(m_iVideoWidth, m_iVideoHeight, m_iARX,m_iARY,dwPtr);
					if (m_bfirstFrame)
					{
						m_bfirstFrame=false;
						D3DSURFACE_DESC desc;
						pTexture->GetLevelDesc(0,&desc);
						
					}
					pTexture->Release();
					
					return;
				}
				dwPtr=(DWORD)(IDirect3DSurface9*)pSurface;
				m_pCallback->PresentSurface(
					m_iVideoWidth, m_iVideoHeight, 
					m_iARX,m_iARY,dwPtr);
				if (m_bfirstFrame)
				{
					D3DSURFACE_DESC desc;
					pSurface->GetDesc(&desc);
					m_bfirstFrame=false;
				}
				return;
			}
		}
	}
	catch(...)
	{
		Log("vmr9:Paint() invalid exception");
	}
}


HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_FramesDroppedInRenderer(int *pcFrames)
{
	if ( pcFrames == NULL ) return E_POINTER;
//	Log("evr:get_FramesDropped: %d", m_iFramesDropped);
	*pcFrames = m_iFramesDropped;
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_FramesDrawn(int *pcFramesDrawn)
{
	if ( pcFramesDrawn == NULL ) return E_POINTER;
//	Log("evr:get_FramesDrawn: %d", m_iFramesDrawn);
	*pcFramesDrawn = m_iFramesDrawn;
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_AvgFrameRate(int *piAvgFrameRate)
{
	//Log("evr:get_AvgFrameRate");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_Jitter(int *iJitter)
{
	/*Log("evr:get_Jitter: %d, deviation: %d", m_iJitter,
		(int)(m_hnsTotalDiff / m_iFramesDrawn) );*/
	*iJitter = m_iJitter;
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_AvgSyncOffset(int *piAvg)
{
	//Log("evr:get_AvgSyncOffset");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_DevSyncOffset(int *piDev)
{
	//Log("evr:get_DevSyncOffset");
	return S_OK;
}
