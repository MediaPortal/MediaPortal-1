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
#include <mferror.h>
#include <objbase.h>
#include <dxva2api.h>
#include "evrcustompresenter.h"

void Log(const char *fmt, ...) ;
HRESULT __fastcall UnicodeToAnsi(LPCOLESTR pszW, LPSTR* ppszA)
{

    ULONG cbAnsi, cCharacters;
    DWORD dwError;

    // If input is null then just return the same.
    if (pszW == NULL)
    {
        *ppszA = NULL;
        return NOERROR;
    }

    cCharacters = wcslen(pszW)+1;
    // Determine number of bytes to be allocated for ANSI string. An
    // ANSI string can have at most 2 bytes per character (for Double
    // Byte Character Strings.)
    cbAnsi = cCharacters*2;

    // Use of the OLE allocator is not required because the resultant
    // ANSI  string will never be passed to another COM component. You
    // can use your own allocator.
    *ppszA = (LPSTR) CoTaskMemAlloc(cbAnsi);
    if (NULL == *ppszA)
        return E_OUTOFMEMORY;

    // Convert to ANSI.
    if (0 == WideCharToMultiByte(CP_ACP, 0, pszW, cCharacters, *ppszA,
                  cbAnsi, NULL, NULL))
    {
        dwError = GetLastError();
        CoTaskMemFree(*ppszA);
        *ppszA = NULL;
        return HRESULT_FROM_WIN32(dwError);
    }
    return NOERROR;

}



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


unsigned __stdcall SchedulerThread(void *ArgList) {
	SchedulerParams *p = (SchedulerParams*)ArgList;
	EVRCustomPresenter* pPresenter = p->pPresenter;
	Log( "EVRCustomPresenter: SchedulerThread started" );
	while ( true ) 
	{
		HRESULT hr;
		LONGLONG nextSampleTime;
		p->csLock.Lock();
		if ( p->bDone ) {
			p->csLock.Unlock();
			break;
		}
		hr = pPresenter->CheckForScheduledSample(&nextSampleTime);
		p->csLock.Unlock();
		if ( nextSampleTime <= 0 ) { //Wait for notification
			//Log( "Waiting for notification...");
			while ( !p->eHasWork.Wait() );
			//Log( "Done waiting." );
		} else if ( nextSampleTime >= 10000 ) {
			//Log( "Sleeping: %I64d", nextSampleTime );
			Sleep(nextSampleTime / 10000);
		}
	}
	Log( "EVRCustomPresenter: SchedulerThread stopped" );
	delete p;
	return 0;
}

EVRCustomPresenter::EVRCustomPresenter( IVMR9Callback* pCallback, IDirect3DDevice9* direct3dDevice, HMONITOR monitor)
: m_refCount(1)
{
  char systemFolder[MAX_PATH];
  char mfDLLFileName[MAX_PATH];
  GetSystemDirectory(systemFolder,sizeof(systemFolder));
  sprintf(mfDLLFileName,"%s\\mf.dll", systemFolder);
  m_hModuleMF=LoadLibrary(mfDLLFileName);
  if (m_hModuleMF!=NULL)
  {
    m_pMFGetService=(TMFGetService*)GetProcAddress(m_hModuleMF,"MFGetService");
    if (m_pMFGetService!=NULL)
    {
	    Log("----------v0.37---------------------------");
	    m_hMonitor=monitor;
	    m_pD3DDev=direct3dDevice;
	    HRESULT hr = DXVA2CreateDirect3DDeviceManager9(
		    &m_iResetToken, &m_pDeviceManager);
	    if ( FAILED(hr) ) {
		    Log( "Could not create DXVA2 Device Manager" );
	    } else {
		    m_pDeviceManager->ResetDevice(direct3dDevice, m_iResetToken);
	    }
	    m_pCallback=pCallback;
	    m_surfaceCount=0;
	    //m_UseOffScreenSurface=false;
	    m_fRate = 1.0f;
	    //TODO: use ZeroMemory
	    for ( int i=0; i<NUM_SURFACES; i++ ) {
		    chains[i] = NULL;
		    surfaces[i] = NULL;
		    samples[i] = NULL;
	    }
    }
  }
}

bool EVRCustomPresenter::IsInstalled()
{
  return (m_hModuleMF!=NULL && m_pMFGetService!=NULL);
}
EVRCustomPresenter::~EVRCustomPresenter()
{
	if (m_pCallback!=NULL)
		m_pCallback->PresentImage(0,0,0,0,0);
	DeleteSurfaces();
	StopScheduler();
  if (m_hModuleMF!=NULL)
  {
    FreeLibrary(m_hModuleMF);
  }
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
Log( "QueryInterface" );
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
	else if( riid == IID_IMFGetService) {
		*ppvObject = static_cast<IMFGetService*>( this );
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
		m_pClock = NULL;
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
		m_pClock = NULL;
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
	SAFE_RELEASE( m_pMixer );
	SAFE_RELEASE( m_pClock );
	SAFE_RELEASE( m_pEventSink );
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

HRESULT EVRCustomPresenter::GetTimeToSchedule(IMFSample* pSample, LONGLONG *phnsDelta) 
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
		// This method also returns the system time, which is not used
		// in this example.
		hr = m_pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime);
	}

	// Calculate the amount of time until the sample's presentation
	// time. A negative value means the sample is late.
	hnsDelta = hnsPresentationTime - hnsTimeNow;
	//Log("Calculated delta: %I64d (rate: %f)", hnsDelta, m_fRate);
	if ( m_fRate != 1.0f )
		*phnsDelta = ((float)hnsDelta) / m_fRate;
	else
		*phnsDelta = hnsDelta;
	return hr;
}

void EVRCustomPresenter::ReAllocSurfaces()
{
	DeleteSurfaces();
	IMFVideoMediaType *type = NULL;
	CHECK_HR( GetCurrentMediaType(&type), "GetCurrentMediaType failed" );
	if ( type == NULL ) Log( "Returned NULL-MediaType" );
	const MFVIDEOFORMAT* format = type->GetVideoFormat();

	m_iVideoWidth = format->videoInfo.dwWidth;
	m_iVideoHeight = format->videoInfo.dwHeight;
	m_iARX = format->videoInfo.GeometricAperture.Area.cx;
	m_iARY = format->videoInfo.GeometricAperture.Area.cy;
	Log( "New format: %dx%d, Ratio: %d:%d",
		m_iVideoWidth, m_iVideoHeight, m_iARX, m_iARY );
	LogGUID( format->guidFormat );

	SAFE_RELEASE(type);

	// set the presentation parameters
	D3DPRESENT_PARAMETERS d3dpp;
	ZeroMemory(&d3dpp, sizeof(d3dpp));
	d3dpp.BackBufferWidth = m_iVideoWidth;
	d3dpp.BackBufferHeight = m_iVideoHeight;
	d3dpp.BackBufferCount = 1;
	d3dpp.BackBufferFormat = D3DFMT_X8R8G8B8;
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
	IDirectXVideoDecoderService *pService;
	m_pDeviceManager->GetVideoService(hDevice,
		__uuidof(IDirectXVideoDecoderService), (void**)&pService);
	SAFE_RELEASE(pService);
	HRESULT hr;
	for ( int i=0; i<NUM_SURFACES; i++ ) {
		Log( "Creating chain %d...", i );
		hr = pDevice->CreateAdditionalSwapChain(&d3dpp, &chains[i]);
		if (FAILED(hr)) {
			Log("Chain creation failed with 0x%x", hr);
			return;
		}
		hr = chains[i]->GetBackBuffer(0, D3DBACKBUFFER_TYPE_LEFT, &surfaces[i]);
		if (FAILED(hr)) {
			Log("Could not get back buffer: 0x%x", hr);
			return;
		}
		hr = MFCreateVideoSampleFromSurface(surfaces[i],
			&samples[i]);
		if (FAILED(hr)) {
			Log("CreateVideoSampleFromSurface failed: 0x%x", hr);
			return;
		}
		Log("Adding sample: 0x%x", samples[i]);
		m_vFreeSamples.push_back(samples[i]);
		Log("Chain created");
	}
	m_pDeviceManager->UnlockDevice(hDevice, FALSE);
	m_pDeviceManager->CloseDeviceHandle(hDevice);
}

HRESULT EVRCustomPresenter::RenegotiateMediaInputType()
{
    HRESULT hr = S_OK;
    BOOL fFoundMediaType = FALSE;

    IMFMediaType *pMixerType = NULL;
    IMFMediaType *pType = NULL;

    if (!m_pMixer)
    {
        return MF_E_INVALIDREQUEST;
    }
	if ( SUCCEEDED(m_pMixer->GetInputCurrentType(0, &pMixerType)) ){
		SAFE_RELEASE(pMixerType);
		Log( "Mixer has a valid input type!" );
		return S_OK;
	}
    // Loop through all of the mixer's proposed input types.
    DWORD iTypeIndex = 0;
    while (!fFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
    {
        SAFE_RELEASE(pMixerType);
        SAFE_RELEASE(pType );
Log(  "Testing input media type..." );
        // Step 1. Get the next media type supported by mixer.
        hr = m_pMixer->GetInputAvailableType(0, iTypeIndex++, &pMixerType);
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
            //hr = CreateProposedOutputType(pMixerType, &pType);
        }

        // Step 4. Check if the mixer will accept this media type.
        if (SUCCEEDED(hr))
        {
            //hr = m_pMixer->SetOutputType(0, pType, MFT_SET_TYPE_TEST_ONLY);
			hr = m_pMixer->SetInputType(0, pMixerType, MFT_SET_TYPE_TEST_ONLY);
        }

        // Step 5. Try to set the media type on ourselves.
        if (SUCCEEDED(hr))
        {
            //hr = SetMediaType(pType);
			Log( "New input media type successfully negotiated!" );
			//m_pMediaInputType = pMixerType;
			CHECK_HR( 
				hr = m_pMixer->SetInputType(0, pMixerType, 0),
				"SetInputType failed" );
        }

        // Step 6. Set output media type on mixer.
        if (SUCCEEDED(hr))
        {
            hr = m_pMixer->SetOutputType(0, pType, 0);

            // If something went wrong, clear the media type.
            if (FAILED(hr))
            {
                
				m_pMediaType = NULL;
            }
        }

        if (SUCCEEDED(hr))
        {
            fFoundMediaType = TRUE;
        }
    }

    SAFE_RELEASE(pMixerType);
    SAFE_RELEASE(pType);
    return hr;
}

void LogMediaTypes(IMFTransform* pMixer)
{
	HRESULT hr=S_OK;
    DWORD iTypeIndex = 0;
	IMFMediaType* pType=NULL;
	IMFVideoMediaType* pVideo;
    while ((hr != MF_E_NO_MORE_TYPES))
    {
        SAFE_RELEASE(pType );
        // Step 1. Get the next media type supported by mixer.
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

HRESULT EVRCustomPresenter::RenegotiateMediaOutputType()
{
    HRESULT hr = S_OK;
    BOOL fFoundMediaType = FALSE;

    IMFMediaType *pMixerType = NULL;
    IMFMediaType *pType = NULL;

    if (!m_pMixer)
    {
        return MF_E_INVALIDREQUEST;
    }

	LogMediaTypes(m_pMixer);
    // Loop through all of the mixer's proposed output types.
    DWORD iTypeIndex = 0;
    while (!fFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
    {
        SAFE_RELEASE(pMixerType);
        SAFE_RELEASE(pType );
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
            //hr = CreateProposedOutputType(pMixerType, &pType);
        }

        // Step 4. Check if the mixer will accept this media type.
        if (SUCCEEDED(hr))
        {
            //hr = m_pMixer->SetOutputType(0, pType, MFT_SET_TYPE_TEST_ONLY);
			hr = m_pMixer->SetOutputType(0, pMixerType, MFT_SET_TYPE_TEST_ONLY);
        }

        // Step 5. Try to set the media type on ourselves.
        if (SUCCEEDED(hr))
        {
            //hr = SetMediaType(pType);
			Log( "New media type successfully negotiated!" );
			m_pMediaType = pMixerType;
			ReAllocSurfaces();
        }

        // Step 6. Set output media type on mixer.
        if (SUCCEEDED(hr))
        {
            hr = m_pMixer->SetOutputType(0, pMixerType, 0);

            // If something went wrong, clear the media type.
            if (FAILED(hr))
            {
				Log( "Could not set output type: 0x%x", hr );
                //SetMediaType(NULL);
				m_pMediaType = NULL;
            }
        }

        if (SUCCEEDED(hr))
        {
            fFoundMediaType = TRUE;
        }
    }

    SAFE_RELEASE(pMixerType);
    SAFE_RELEASE(pType);
    return hr;
}

HRESULT EVRCustomPresenter::GetFreeSample(IMFSample** ppSample) 
{
	//TODO hold lock?
	//Log( "Trying to get free sample, size: %d", m_vFreeSamples.size());
	if ( m_vFreeSamples.size() == 0 ) return E_FAIL;
	*ppSample = m_vFreeSamples.back();
	m_vFreeSamples.pop_back();
	
	return S_OK;
}

void EVRCustomPresenter::Flush()
{
	CAutoLock lock(this);
	//Log( "Flushing: size=%d", m_vScheduledSamples.size() );
	while ( m_vScheduledSamples.size()>0 )
	{
		m_vFreeSamples.push_back(m_vScheduledSamples.front());
		m_vScheduledSamples.pop();
	}
}

void EVRCustomPresenter::ReturnSample(IMFSample* pSample, BOOL bCheckForWork)
{
	CAutoLock lock(this);
	//Log( "Sample returned: %x", pSample);
	m_vFreeSamples.push_back(pSample);
	//todo, if queue was empty, do something?
	if ( bCheckForWork && m_vFreeSamples.size() == 1 ) {
		//Log("New Sample available; trying to process input..." );
		ProcessInputNotify();
	}
}

HRESULT EVRCustomPresenter::PresentSample(IMFSample* pSample)
{
    HRESULT hr = S_OK;
    IMFMediaBuffer* pBuffer = NULL;
    IDirect3DSurface9* pSurface = NULL;
    IDirect3DSwapChain9* pSwapChain = NULL;
//Log("Presenting sample");
    // Get the buffer from the sample.
	CHECK_HR(hr = pSample->GetBufferByIndex(0, &pBuffer), "failed: GetBufferByIndex");

    CHECK_HR(hr = m_pMFGetService(
        pBuffer, 
        MR_BUFFER_SERVICE, 
        __uuidof(IDirect3DSurface9), 
        (void**)&pSurface),
		"failed: MFGetService");

    if (pSurface)
    {
        // Get the swap chain from the surface.
        CHECK_HR(hr = pSurface->GetContainer(
            __uuidof(IDirect3DSwapChain9),
            (void**)&pSwapChain),
			"failed: GetContainer");

        // Present the swap chain.
		Paint(pSurface);
        /*CHECK_HR(hr = pSwapChain->Present(NULL, NULL, NULL, NULL, 0),
			"failed: Present");*/
    }

done:
    SAFE_RELEASE(pBuffer);
    SAFE_RELEASE(pSurface);
    SAFE_RELEASE(pSwapChain);
    if (hr == D3DERR_DEVICELOST || hr == D3DERR_DEVICENOTRESET)
    {
        // Failed because the device was lost.
        hr = S_OK;
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
	CAutoLock lock(this);
	if ( m_vScheduledSamples.size() == 0 ) {
		//Log("Nothing in queue. False alert?");
	} else {
	//Log("Checking for scheduled sample (size: %d)",
	//	m_vScheduledSamples.size());
	while ( m_vScheduledSamples.size() > 0 ) {
		IMFSample* pSample = m_vScheduledSamples.front();
		GetTimeToSchedule(pSample, pNextSampleTime);
		//Log( "Time to schedule: %I64d", *pNextSampleTime );
		//if we are ahead only 1 ms, present this sample anyway
		//else sleep for some time
		if ( *pNextSampleTime > 10000 ) {
			return hr;
		}
		m_vScheduledSamples.pop();
		if ( *pNextSampleTime < -400000 ) {
			//skip!
			Log( "skipping frame, behind %d hns", -*pNextSampleTime );
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
	CAutoLock lock(this);
	m_schedulerParams = new SchedulerParams();
	m_schedulerParams->pPresenter = this;
	m_schedulerParams->bDone = FALSE;
	m_hScheduler = (HANDLE)_beginthreadex(NULL, 0, SchedulerThread,
		m_schedulerParams, 0, &m_uThreadId);
	m_bSchedulerRunning = TRUE;
}

void EVRCustomPresenter::StopScheduler()
{
	CAutoLock lock(this);
	if ( !m_bSchedulerRunning ) return;
	Log( "Ending Scheduler (getting lock)" );
	CAutoLock slock(&m_schedulerParams->csLock);
	m_schedulerParams->bDone = TRUE;
	Log( "Scheduler should end now" );
	NotifyScheduler();
	m_bSchedulerRunning = FALSE;
}

void EVRCustomPresenter::NotifyScheduler()
{
	//Log( "Notifying Scheduler" );
	if ( !m_bSchedulerRunning ) {
		Log("ERROR: Scheduler not running!");
		return;
	} 
	m_schedulerParams->eHasWork.Set();
}

void EVRCustomPresenter::ScheduleSample(IMFSample* pSample)
{
	CAutoLock lock(this);
//	Log( "Scheduling Sample, size: %d", m_vScheduledSamples.size() );
	m_vScheduledSamples.push(pSample);
	if ( m_vScheduledSamples.size() == 1 )
	{
		NotifyScheduler();
	}
}

HRESULT EVRCustomPresenter::ProcessInputNotify()
{
	CAutoLock lock(this);
	HRESULT hr=S_OK;
	if ( m_pClock ) {
		MFCLOCK_STATE state;
		m_pClock->GetState(0, &state);
		if ( state == MFCLOCK_STATE_PAUSED && !m_bfirstInput) return S_OK;
		m_bfirstInput = false;
	}
	//try to process as many samples as possible:
	BOOL bhasMoreSamples = true;
	do {
		IMFSample* sample;
		hr = GetFreeSample(&sample);
		if ( FAILED(hr) ) {
			//Log( "No free sample available" );
			return S_OK;
		}
		//Log("Found free sample 0x%x", sample);
		DWORD dwStatus;
		MFT_OUTPUT_DATA_BUFFER outputSamples[1];
		outputSamples[0].dwStreamID = 0; 
		outputSamples[0].dwStatus = 0; 
		outputSamples[0].pSample = sample; 
		outputSamples[0].pEvents = NULL;
		hr = m_pMixer->ProcessOutput(0, 1, outputSamples,
			&dwStatus);
		SAFE_RELEASE(outputSamples[0].pEvents);
		if ( SUCCEEDED( hr ) ) {
			//Log("Scheduling sample");
			ScheduleSample(sample);
		} else {
			switch ( hr ) {
				case MF_E_TRANSFORM_NEED_MORE_INPUT:
					//we are done for now
					hr = S_OK;
					bhasMoreSamples = false;
					break;
				case MF_E_TRANSFORM_STREAM_CHANGE:
				case MF_E_TRANSFORM_TYPE_NOT_SET:
					//no errors, just infos why it didn't succeed
					Log( "ProcessOutput: unhandled change of stream or type: %x", hr );
					hr = S_OK;
					break;
				default:
					Log( "ProcessOutput failed: 0x%x", hr );
			}
			ReturnSample(sample, FALSE);
		}
	} while ( bhasMoreSamples );
	return hr;
}

HRESULT STDMETHODCALLTYPE EVRCustomPresenter::ProcessMessage( 
            MFVP_MESSAGE_TYPE eMessage,
            ULONG_PTR ulParam)
{
	HRESULT hr = S_OK;
	switch ( eMessage ) {
		case MFVP_MESSAGE_INVALIDATEMEDIATYPE:
			Log( "Negotiate Media type" );
			//The mixer's output media type is invalid. The presenter should negotiate a new media type with the mixer. See Negotiating Formats.
			hr = RenegotiateMediaInputType();
			if ( SUCCEEDED(hr) ) RenegotiateMediaOutputType();
			break;

		case MFVP_MESSAGE_BEGINSTREAMING:
			//Streaming has started. No particular action is required by this message, but you can use it to allocate resources.
			Log("ProcessMessage %x", eMessage);
			m_bfirstFrame = true;
			m_bfirstInput = true;
			StartScheduler();
			break;

		case MFVP_MESSAGE_ENDSTREAMING:
			//Streaming has ended. Release any resources that you allocated in response to the MFVP_MESSAGE_BEGINSTREAMING message.
			Log("ProcessMessage %x", eMessage);
			StopScheduler();
			break;

		case MFVP_MESSAGE_PROCESSINPUTNOTIFY:
			//The mixer has received a new input sample and might be able to generate a new output frame. The presenter should call IMFTransform::ProcessOutput on the mixer. See Processing Output.
	//Log("ProcessMessage %x", eMessage);
			hr = ProcessInputNotify();
			break;

		case MFVP_MESSAGE_ENDOFSTREAM:
			//The presentation has ended. See End of Stream.
	Log("ProcessMessage %x", eMessage);
	Flush();
	m_pEventSink->Notify(EC_COMPLETE, (LONG_PTR)S_OK,
		0);
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
	Log("OnClockSetRate");
	m_fRate = flRate;
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
		Log("Wanting Manager!");
		hr = m_pDeviceManager->QueryInterface(riid, (void**)ppvObject);
		hr = S_OK;
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
void EVRCustomPresenter::DeleteSurfaces()
{

	Log("vmr9:DeleteSurfaces()");
	m_vFreeSamples.clear();
	for ( int i=0; i<NUM_SURFACES; i++ ) {
		Log("Delete: %d, 0x%x", i, chains[i]);
		SAFE_RELEASE( chains[i] );
	}

	if (m_pCallback!=NULL)
		m_pCallback->PresentImage(0,0,0,0,0);
}

void EVRCustomPresenter::Paint(IDirect3DSurface9* pSurface)
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
				dwPtr=(DWORD)(pSurface);
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
	Log("evr:get_FramesDropped");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_FramesDrawn(int *pcFramesDrawn)
{
	Log("evr:get_FramesDrawn");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_AvgFrameRate(int *piAvgFrameRate)
{
	Log("evr:get_AvgFrameRate");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_Jitter(int *iJitter)
{
	Log("evr:get_Jitter");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_AvgSyncOffset(int *piAvg)
{
	Log("evr:get_AvgSyncOffset");
	return S_OK;
}
HRESULT STDMETHODCALLTYPE EVRCustomPresenter::get_DevSyncOffset(int *piDev)
{
	Log("evr:get_DevSyncOffset");
	return S_OK;
}
