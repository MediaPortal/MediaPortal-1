/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include <streams.h>
#include "OutputPin.h"
#include "demux.h"
#include <atlbase.h>
 
#define BUFFER_SIZE (1316*30)
extern void Log(const char *fmt, ...) ;
COutputPin::COutputPin(LPUNKNOWN pUnk, CRtspSourceFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinOut"), phr, pFilter, L"Out"),
  CSourceSeeking(NAME("pinOut"),pUnk,phr,section),
	m_pFilter(pFilter),
	m_section(section)
{
	m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute	|
	AM_SEEKING_CanSeekForwards	|
	AM_SEEKING_CanSeekBackwards	|
	AM_SEEKING_CanGetCurrentPos	|
	AM_SEEKING_CanGetStopPos	|
	AM_SEEKING_CanGetDuration	|
	AM_SEEKING_Source;
	m_rtDuration=CRefTime(7200L*1000L);
	m_bSeeking = false;
  m_DemuxLock=false;
  m_biMpegDemux=false;
  m_bIsTimeShifting=true;
  m_mpegDemuxerFilter=NULL;
}

COutputPin::~COutputPin(void)
{
}

void COutputPin::IsTimeShifting(bool onOff)
{
  m_bIsTimeShifting=onOff;
  if (m_bIsTimeShifting) 
    Log("timeshifting file");
  else
    Log("normal file");
}

STDMETHODIMP COutputPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	if (riid == IID_IAsyncReader)
  {
		int x=1;
	}
  if (riid == IID_IMediaSeeking)
  {
    return CSourceSeeking::NonDelegatingQueryInterface(riid, ppv);
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT COutputPin::GetMediaType(CMediaType *pmt)
{

	pmt->InitMediaType();
  pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);
  pmt->SetFormatType(&FORMAT_None);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();

	return S_OK;
}
HRESULT COutputPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;


	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers==0)
	{
		pRequest->cBuffers = 30;
	}

	pRequest->cbBuffer = BUFFER_SIZE;


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

HRESULT COutputPin::BreakConnect()
{	
	Log("COutputPin::BreakConnect()");
	HRESULT hr = CBaseOutputPin::BreakConnect();
	if (FAILED(hr))
		return hr;
	DisconnectDemux();
	m_bSeeking = false;
	m_biMpegDemux = false;
	m_DemuxLock = false;
  return S_OK;
}
HRESULT COutputPin::CheckConnect(IPin *pReceivePin)
{
	Log("COutputPin::CheckConnect()");
	if(!pReceivePin)
		return E_INVALIDARG;

	m_biMpegDemux = true;
    CAutoLock cAutoLock(m_pFilter->pStateLock());
	HRESULT hr = CBaseOutputPin::CheckConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		PIN_INFO pInfo;
		if (SUCCEEDED(pReceivePin->QueryPinInfo(&pInfo)))
		{
			// Get an instance of the Demux control interface
			CComPtr<IMpeg2Demultiplexer> muxInterface;
			if(SUCCEEDED(pInfo.pFilter->QueryInterface (&muxInterface)))
			{
				// Create out new pin to make sure the interface is real
				CComPtr<IPin>pIPin = NULL;
				AM_MEDIA_TYPE pintype;
				ZeroMemory(&pintype, sizeof(AM_MEDIA_TYPE));
				hr = muxInterface->CreateOutputPin(&pintype, L"MS" ,&pIPin);
				if (SUCCEEDED(hr) && pIPin != NULL)
				{
          m_mpegDemuxerFilter=pInfo.pFilter;
					//pInfo.pFilter->Release();
					hr = muxInterface->DeleteOutputPin(L"MS");
					if SUCCEEDED(hr)
						m_biMpegDemux = true;
					return S_OK;
				}
			}

			//Test for a filter with "MPEG-2" on input pin label
			if (wcsstr(pInfo.achName, L"MPEG-2") != NULL)
			{
        m_mpegDemuxerFilter=pInfo.pFilter;
				//pInfo.pFilter->Release();
				m_biMpegDemux = true;
				return S_OK;
			}
		}
	}
	return S_OK;
}
HRESULT COutputPin::CompleteConnect(IPin *pReceivePin)
{
	Log("COutputPin::CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
    m_pFilter->OnConnect();
	}
	else
	{
	}
	return hr;
}

HRESULT COutputPin::FillBuffer(IMediaSample *pSample)
{
	CAutoLock lock(&m_FillLock);
  if (!m_pFilter->IsClientRunning()) 
	{
    Log("fillbuffer: client not running");
    m_bDisContinuity=true;
    pSample->SetDiscontinuity(TRUE);
		pSample->SetActualDataLength(0);
		Sleep(50);
		return S_OK;
	}

  BYTE* pBuffer;
  pSample->GetPointer(&pBuffer);
	long lDataLength = BUFFER_SIZE;//pSample->GetActualDataLength();
  DWORD bytesRead=m_pFilter->GetData(pBuffer,lDataLength);
	if (bytesRead==0)
	{
    Log("fillbuffer: 0 bytes read");
		Sleep(50);
    m_bDisContinuity=true;
    pSample->SetDiscontinuity(TRUE);
		return S_OK;
	}
  pSample->SetActualDataLength(bytesRead);
  if (m_bDisContinuity) 
  {
    pSample->SetDiscontinuity(TRUE);
    m_bDisContinuity=false;

  }
  return S_OK;
}
HRESULT COutputPin::ChangeStart()
{
	Log("COutputPin::ChangeStart()");
	m_bSeeking = false;
  return S_OK;
}

HRESULT COutputPin::ChangeStop()
{
	Log("COutputPin::ChangeStop()");
	m_bSeeking = false;
	return S_OK;
}

HRESULT COutputPin::ChangeRate()
{
	Log("COutputPin::ChangeRate()");
	return S_OK;
}

HRESULT COutputPin::Run(REFERENCE_TIME tStart)
{
	Log("COutputPin::Run()");
	//CAutoLock fillLock(&m_FillLock);
	//CAutoLock seekLock(&m_SeekLock);
  m_pFilter->ResetStreamTime();
  m_bDisContinuity=true;
	if (!m_bSeeking && !m_DemuxLock)
	{	
//		CComPtr<IReferenceClock> pClock;
//		Demux::GetReferenceClock(m_pFilter, &pClock);
//		SetDemuxClock(pClock);
	}
	HRESULT hr= CBaseOutputPin::Run(tStart);
	Log("COutputPin::Run() done");
  return hr;
}
HRESULT COutputPin::GetDuration(LONGLONG *pDuration)
{
  //Log("COutputPin::GetDuration(%d)",(m_rtDuration.Millisecs()/1000));
    CheckPointer(pDuration, E_POINTER);
//    CAutoLock lock(m_pLock);
    *pDuration = m_rtDuration;
    return S_OK;
}

HRESULT COutputPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
	Log("COutputPin::SetPositions()");
	if(!m_rtDuration)
		return E_FAIL;

	if (pCurrent)
	{
		REFERENCE_TIME rtCurrent = *pCurrent;
		if (CurrentFlags & AM_SEEKING_RelativePositioning)
		{
			CAutoLock lock(&m_SeekLock);
			rtCurrent += m_rtStart;
			CurrentFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
			CurrentFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
		}

		if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
		{
			CAutoLock lock(&m_SeekLock);
			m_rtStart = rtCurrent;
		}

		if (!(CurrentFlags & AM_SEEKING_NoFlush) && (CurrentFlags & AM_SEEKING_PositioningBitsMask))
		{
				m_bSeeking = true;
        if (m_mpegDemuxerFilter!=NULL) m_mpegDemuxerFilter->SetSyncSource(NULL);
        m_pFilter->Buffer().Run(false);
        Log("COutputPin::SetPositions()#1");
//				if(m_pFilter->is_Active() && !m_DemuxLock)
//				  SetDemuxClock(NULL);
        m_bDisContinuity=true;
        Log("COutputPin::SetPositions()#2");
				DeliverBeginFlush();
        Log("COutputPin::SetPositions()#3");
				CSourceStream::Stop();
        Log("COutputPin::SetPositions()#4");
				SetAccuratePos(rtCurrent);
        Log("COutputPin::SetPositions()#5");
				if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
				{
					CAutoLock lock(&m_SeekLock);
					m_rtStart = rtCurrent;
				}
				//m_rtLastSeekStart = rtCurrent;
				m_bSeeking = false;
				DeliverEndFlush();
        Log("COutputPin::SetPositions()#6");

        //takes 5 secs?
				CSourceStream::Run();
        Log("COutputPin::SetPositions()#7");
				if (CurrentFlags & AM_SEEKING_ReturnTime)
					*pCurrent  = rtCurrent;

        Log("COutputPin::SetPositions()#8");
				CAutoLock lock(&m_SeekLock);
        Log("COutputPin::SetPositions()#9");
				HRESULT hr=CSourceSeeking::SetPositions(&rtCurrent, CurrentFlags, pStop, StopFlags);
        
        Log("COutputPin::SetPositions()#10");
        
        //takes 5 secs?
        m_pFilter->Buffer().Run(true);
        m_bDisContinuity=true;
        Log("COutputPin::SetPositions()#11");

        if (m_mpegDemuxerFilter!=NULL) 
        {
          CComPtr<IReferenceClock> pClock;
          Demux::GetReferenceClock(m_pFilter, &pClock);
          m_mpegDemuxerFilter->SetSyncSource(pClock);
        }
        return hr;
//			}
		}
		if (CurrentFlags & AM_SEEKING_ReturnTime)
			*pCurrent  = rtCurrent;

		return CSourceSeeking::SetPositions(&rtCurrent, CurrentFlags, pStop, StopFlags);
	}
	return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop, StopFlags);
}

HRESULT COutputPin::SetAccuratePos(REFERENCE_TIME seektime)
{
  Log("COutputPin::SetAccuratePos()#1");
	m_pFilter->ResetStreamTime();
  Log("COutputPin::SetAccuratePos()#2");
	m_pFilter->Seek(m_rtStart);
  Log("COutputPin::SetAccuratePos()#3");
  m_bDisContinuity=true;
	return S_OK;
}
HRESULT COutputPin::OnThreadStartPlay(void) 
{
	CAutoLock fillLock(&m_FillLock);
	CAutoLock lock(&m_SeekLock);
	//Log("COutputPin::OnThreadStartPlay()");
  m_bDisContinuity=true;
  DeliverNewSegment(m_rtStart, m_rtStop, 1.0 );
	return CSourceStream::OnThreadStartPlay( );
}


void COutputPin::SetDuration(CRefTime& duration)
{
//  Log("COutputPin::SetDuration(%d)",(duration.Millisecs()/1000));
	m_rtDuration=duration;
}


HRESULT COutputPin::SetDemuxClock(IReferenceClock *pClock)
{
	Log("COutputPin::SetDemuxClock()");
	// Parse only the existing Mpeg2 Demultiplexer Filter
	// in the filter graph, we do this by looking for filters
	// that implement the IMpeg2Demultiplexer interface while
	// the count is still active.
	CFilterList FList(NAME("MyList"));  // List to hold the downstream peers.
	if (SUCCEEDED(Demux::GetPeerFilters(m_pFilter, PINDIR_OUTPUT, FList)) && FList.GetHeadPosition())
	{
		IBaseFilter* pFilter = NULL;
		POSITION pos = FList.GetHeadPosition();
		pFilter = FList.Get(pos);
		while (SUCCEEDED(Demux::GetPeerFilters(pFilter, PINDIR_OUTPUT, FList)) && pos)
		{
			pFilter = FList.GetNext(pos);
		}

		pos = FList.GetHeadPosition();

		while (pos)
		{
			pFilter = FList.GetNext(pos);
			if(pFilter != NULL)
			{
				//Keep a reference for later destroy, will not add the same object
                Demux::AddFilterUnique(m_pFilter->m_FilterRefList, pFilter);
				// Get an instance of the Demux control interface
				IMpeg2Demultiplexer* muxInterface = NULL;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
//***********************************************************************************************
//Old Capture format Additions
//					if (m_pPidParser->pids.pcr && m_pFilter->get_AutoMode()) // && !m_pPidParser->pids.opcr) 
//***********************************************************************************************
//					if (TRUE && m_biMpegDemux && m_pFilter->get_AutoMode())
					if (TRUE && m_biMpegDemux)
						pFilter->SetSyncSource(pClock);
					muxInterface->Release();
				}
				pFilter = NULL;
			}
		}
	}

	//Clear the filter list;
	POSITION pos = FList.GetHeadPosition();
	while (pos){

		if (FList.Get(pos) != NULL)
				FList.Get(pos)->Release();

		FList.Remove(pos);
		pos = FList.GetHeadPosition();
	}

	return S_OK;
}

HRESULT COutputPin::DisconnectDemux()
{
	Log("COutputPin::DisconnectDemux()");
	// Parse only the existing Mpeg2 Demultiplexer Filter
	// in the filter graph, we do this by looking for filters
	// that implement the IMpeg2Demultiplexer interface while
	// the count is still active.
	CFilterList FList(NAME("MyList"));  // List to hold the downstream peers.
	if (SUCCEEDED(Demux::GetPeerFilters(m_pFilter, PINDIR_OUTPUT, FList)) && FList.GetHeadPosition())
	{
		IBaseFilter* pFilter = NULL;
		POSITION pos = FList.GetHeadPosition();
		pFilter = FList.Get(pos);
		while (SUCCEEDED(Demux::GetPeerFilters(pFilter, PINDIR_OUTPUT, FList)) && pos)
		{
			pFilter = FList.GetNext(pos);
		}

		pos = FList.GetHeadPosition();

		while (pos)
		{
			pFilter = FList.GetNext(pos);
			if(pFilter != NULL)
			{
				//Keep a reference for later destroy, will not add the same object
				Demux::AddFilterUnique(m_pFilter->m_FilterRefList, pFilter);
				// Get an instance of the Demux control interface
				CComPtr<IMpeg2Demultiplexer> muxInterface;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
					DisconnectOutputPins(pFilter);
					muxInterface.Release();
				}

				pFilter = NULL;
			}
		}
	}

	//Clear the filter list;
	POSITION pos = FList.GetHeadPosition();
	while (pos){

		if (FList.Get(pos) != NULL)
				FList.Get(pos)->Release();

		FList.Remove(pos);
		pos = FList.GetHeadPosition();
	}

	return S_OK;
}

HRESULT COutputPin::DisconnectOutputPins(IBaseFilter *pFilter)
{
	Log("COutputPin::DisconnectOutputPins()");
	CComPtr<IPin> pOPin;
	PIN_DIRECTION  direction;
	// Enumerate the Demux pins
	CComPtr<IEnumPins> pIEnumPins;
	if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins)))
	{

		ULONG pinfetch(0);
		while(pIEnumPins->Next(1, &pOPin, &pinfetch) == S_OK)
		{

			pOPin->QueryDirection(&direction);
			if(direction == PINDIR_OUTPUT)
			{
				// Get an instance of the Demux control interface
				CComPtr<IMpeg2Demultiplexer> muxInterface;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
					LPWSTR pinName = L"";
					pOPin->QueryId(&pinName);
					muxInterface->DeleteOutputPin(pinName);
					muxInterface.Release();
				}
				else
				{
					IPin *pIPin = NULL;
					pOPin->ConnectedTo(&pIPin);
					if (pIPin)
					{
						pOPin->Disconnect();
						pIPin->Disconnect();
						pIPin->Release();
					}
				}

			}
			pOPin.Release();
			pOPin = NULL;
		}
	}
	return S_OK;
}