/*
	MediaPortal TS-SourceFilter by Agree
	Some parts taken from OpenSource TSSourceFilter.ax by nate,bear and bisswanger
	
*/


#include <streams.h>
#include "MPTSFilter.h"
//#include "Mmsystem.h"
class CFilterOutPin;
extern void LogDebug(const char *fmt, ...) ;

CFilterOutPin::CFilterOutPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr) :
	CSourceStream(NAME("PinObject"), phr, pFilter, L"Out"),
	CSourceSeeking(NAME("MediaSeekingObject"), pUnk, phr, &m_cSharedState),
	m_pMPTSFilter(pFilter),
	m_pFileReader(pFileReader),
	m_pSections(pSections),m_bDiscontinuity(FALSE), m_bFlushing(FALSE)
{
	CAutoLock cAutoLock(&m_cSharedState);
	m_dwSeekingCaps =	
						AM_SEEKING_CanSeekForwards  | AM_SEEKING_CanSeekBackwards |
						AM_SEEKING_CanGetStopPos    | AM_SEEKING_CanGetDuration   |
						AM_SEEKING_CanSeekAbsolute;

	__int64 size;
	m_pFileReader->GetFileSize(&size);
	m_rtDuration = m_rtStop = m_pSections->pids.Duration;
	m_lTSPacketDeliverySize = 188000;
	m_pBuffers = new CBuffers(m_pFileReader, &m_pSections->pids);
	m_dTimeInc=0;
	m_timeStart=0;
}

CFilterOutPin::~CFilterOutPin()
{
	CAutoLock cAutoLock(&m_cSharedState);
	delete m_pBuffers;
}
STDMETHODIMP CFilterOutPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
    if (riid == IID_IMediaSeeking)
    {
        return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
    }
    return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CFilterOutPin::GetMediaType(CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pmt, E_POINTER);

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);

    return S_OK;
}

HRESULT CFilterOutPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
    HRESULT hr;

	CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(pRequest, E_POINTER);

    if (pRequest->cBuffers == 0)
    {
        pRequest->cBuffers = 2;
    }

	pRequest->cbBuffer = m_lTSPacketDeliverySize;
	

    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(pRequest, &Actual);
    if (FAILED(hr))
    {
        return hr;
    }

    if (Actual.cbBuffer < pRequest->cbBuffer)
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CFilterOutPin::CompleteConnect(IPin *pReceivePin)
{
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		m_pMPTSFilter->OnConnect();
		m_rtDuration = m_pSections->pids.Duration;
		m_rtStop = m_rtDuration;
	}
	return S_OK;
}
STDMETHODIMP CFilterOutPin::SetPositions(LONGLONG *pCurrent,DWORD CurrentFlags,LONGLONG *pStop,DWORD StopFlags)
{
	return CSourceSeeking::SetPositions(pCurrent,CurrentFlags,pStop,StopFlags);
}
HRESULT CFilterOutPin::FillBuffer(IMediaSample *pSample)
{
	//LogDebug("FillBuffer");
	CheckPointer(pSample, E_POINTER);
	m_pMPTSFilter->UpdatePids();
	{
		CAutoLock cAutoLockShared(&m_cSharedState);

		PBYTE pData;
		LONG lDataLength;
		HRESULT hr = pSample->GetPointer(&pData);
		if (FAILED(hr))
		{
			LogDebug("GetPointer() failed:%x",hr);
			m_pMPTSFilter->Log((char*)"pin: FillBuffer() error on getting pointer for sample",true);
			return hr;
		}
		lDataLength = pSample->GetActualDataLength();


		hr = m_pBuffers->Require(lDataLength);
		if (FAILED(hr))
		{
			m_pMPTSFilter->Log((char*)"pin: FillBuffer() error in fillbuffer",true);
			m_pMPTSFilter->Refresh();
			//return S_FALSE; // cant read = end of stream
		}

		m_pBuffers->DequeFromBuffer(pData, lDataLength);

		ULONGLONG pts=0;
		Sections::PTSTime time;
		int stream;
		for(int i=0;i<18800;i+=188)
		{
			if(m_pSections->CurrentPTS(pData+i,&pts,&stream)==S_OK)
			{
					// correct our clock
				ULONGLONG millis = pts / 90; // Systemclock (27MHz) / 300
				m_dwStartTime = (DWORD)(timeGetTime() - millis);
				break; // first pts
			}

		}
		CRefTime rt((LONG)(timeGetTime() - m_dwStartTime));
		REFERENCE_TIME rtStart = static_cast<REFERENCE_TIME>(rt / m_dRateSeeking);
		REFERENCE_TIME rtStop  = static_cast<REFERENCE_TIME>(m_rtStop / m_dRateSeeking);
		pSample->SetTime(&rtStart, &rtStop); 
		pSample->SetSyncPoint(TRUE);

		if(m_bDiscontinuity) 
		{
			m_pMPTSFilter->Log((char*)"pin: FillBuffer() SetDiscontinuity",true);
			pSample->SetDiscontinuity(TRUE);
			m_bDiscontinuity = FALSE;
		}
	}
	return NOERROR;
}
STDMETHODIMP CFilterOutPin::GetDuration(LONGLONG *pDuration)
{
	HRESULT hr = CSourceSeeking::GetDuration(pDuration);
	return S_OK;
}

HRESULT CFilterOutPin::OnThreadCreate( )
{
    CAutoLock cAutoLockShared(&m_cSharedState);
	if(m_pFileReader->IsFileInvalid()==true)
	{
		m_pFileReader->OpenFile();
	}
    return S_OK;
}

HRESULT CFilterOutPin::OnThreadStartPlay( )
{
   m_bDiscontinuity = TRUE;
   return DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
}

HRESULT CFilterOutPin::Run(REFERENCE_TIME tStart)
{
	return CBaseOutputPin::Run(tStart);
}


HRESULT CFilterOutPin::ChangeStart()
{
	m_pMPTSFilter->SetFilePosition(m_rtStart);
    UpdateFromSeek();
    return S_OK;
}

HRESULT CFilterOutPin::ChangeStop()
{
    UpdateFromSeek();
    return S_OK;
}

HRESULT CFilterOutPin::ChangeRate()
{
    {   // Scope for critical section lock.
        CAutoLock cAutoLockSeeking(CSourceSeeking::m_pLock);
        if( m_dRateSeeking <= 0 ) {
            m_dRateSeeking = 1.0;  // Reset to a reasonable value.
            return E_FAIL;
        }
    }
    UpdateFromSeek();
	return S_OK;
}

void CFilterOutPin::UpdateFromSeek(void)
{
	if (ThreadExists())
	{
		m_pMPTSFilter->Log((char*)"pin: UpdateFromSeek()",true);
		DeliverBeginFlush();
		Stop();
		DeliverEndFlush();
		Pause();
	}
}

HRESULT CFilterOutPin::SetDuration(REFERENCE_TIME duration)
{
	
	CAutoLock lock(CSourceSeeking::m_pLock);
	m_rtDuration = duration;
	m_rtStop = m_rtDuration;
    return S_OK;
}

HRESULT CFilterOutPin::GetReferenceClock(IReferenceClock **pClock)
{
	HRESULT hr;
	FILTER_INFO		filterInfo;
	hr = m_pFilter->QueryFilterInfo(&filterInfo);

	if (filterInfo.pGraph)
	{
		// Get IMediaFilter interface
		IMediaFilter* pMediaFilter = NULL;
		hr = filterInfo.pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
		filterInfo.pGraph->Release();

		if (pMediaFilter)
		{
			// Get IReferenceClock interface
			hr = pMediaFilter->GetSyncSource(pClock);
			pMediaFilter->Release();
			return S_OK;
		}
	}
	return E_FAIL;
}
void CFilterOutPin::ResetBuffers()
{
	if (m_pBuffers==NULL) return;
	m_pBuffers->Clear();
	m_rtDuration = m_rtStop = m_pSections->pids.Duration;
	m_bDiscontinuity=true;
}