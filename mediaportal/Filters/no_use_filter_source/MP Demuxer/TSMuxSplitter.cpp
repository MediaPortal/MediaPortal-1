// 
//  This File is based on CDCLs Sample Code.
//	Changes Copyright by Agree / Team MediaPortal 2005
//
// Copyright (c) GDCL 2004. All Rights Reserved. 
// You are free to re-use this as the basis for your own filter development,
// provided you retain this copyright notice in the source.
// http://www.gdcl.co.uk
//
//////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "TSMuxSplitter.h"


// filter registration tables
//static 
const AMOVIESETUP_MEDIATYPE 
TSMuxSplitter::m_sudType[] = 
{
    {
        &MEDIATYPE_Stream,
        &MEDIASUBTYPE_NULL
    },
    {
        &MEDIATYPE_Video,
        &MEDIASUBTYPE_NULL
    },
    {
        &MEDIATYPE_Audio,
        &MEDIASUBTYPE_NULL
    }
};

//static 
const AMOVIESETUP_PIN 
TSMuxSplitter::m_sudPin[] = 
{
    {
        L"Input",          // pin name
        FALSE,              // is rendered?    
        FALSE,              // is output?
        FALSE,              // zero instances allowed?
        FALSE,              // many instances allowed?
        &CLSID_NULL,        // connects to filter (for bridge pins)
        NULL,               // connects to pin (for bridge pins)
        1,                  // count of registered media types
        &m_sudType[0]       // list of registered media types    
    },
    {
        L"Video Output",    // pin name
        FALSE,              // is rendered?    
        TRUE,               // is output?
        FALSE,              // zero instances allowed?
        FALSE,              // many instances allowed?
        &CLSID_NULL,        // connects to filter (for bridge pins)
        NULL,               // connects to pin (for bridge pins)
        1,                  // count of registered media types
        &m_sudType[1]       // list of registered media types    
    },
    {
        L"Audio Output",    // pin name
        FALSE,              // is rendered?    
        TRUE,               // is output?
        FALSE,              // zero instances allowed?
        FALSE,              // many instances allowed?
        &CLSID_NULL,        // connects to filter (for bridge pins)
        NULL,               // connects to pin (for bridge pins)
        1,                  // count of registered media types
        &m_sudType[2]       // list of registered media types    
    },
};

//static 
const AMOVIESETUP_FILTER 
TSMuxSplitter::m_sudFilter =
{
    &__uuidof(TSMuxSplitter),         // filter clsid
    L"MediaPortal TS-Mux Splitter",   // filter name
    MERIT_NORMAL,               // MERIT_NORMAL
    3,                              // count of registered pins
    m_sudPin                        // list of pins to register
};

//static 
CUnknown* WINAPI TSMuxSplitter::CreateInstance(LPUNKNOWN pUnk, HRESULT* phr)
{
    return new TSMuxSplitter(pUnk, phr);
}

TSMuxSplitter::TSMuxSplitter(LPUNKNOWN pUnk, HRESULT* phr)
: m_pVideo(NULL),
  m_pAudio(NULL),
  m_pInput(NULL),
  m_pSeekingPin(NULL),
  m_tStart(0),
  m_tStop(0x7ffffffffffffff),       // less than MAX_TIME so we can add one second to it
  m_dRate(1.0),
  m_bLocating(false),
  m_nTries(0),
  m_llFileSize(0),
  CBaseFilter(NAME("TSMuxSplitter"), pUnk, &m_csFilter, *m_sudFilter.clsID)
{
    // create the pins: to simplify the sample, we expect
    // exactly one video stream, and one audio stream, and we
    // create an output pin each
    if (SUCCEEDED(*phr))
    {
        m_pVideo = new SplitterOutputPin(this, &m_csFilter, phr, L"Video Out");
    }
    if (SUCCEEDED(*phr))
    {
        m_pAudio = new SplitterOutputPin(this, &m_csFilter, phr, L"Audio Out");
    }
    if (SUCCEEDED(*phr))
    {
        m_pInput = new StreamInputPin(this, &m_csFilter, phr);
    }
}


TSMuxSplitter::~TSMuxSplitter()
{
	delete m_pVideo;
    delete m_pAudio;
    delete m_pInput;
}

int TSMuxSplitter::GetPinCount()
{
    return 3;
}
STDMETHODIMP TSMuxSplitter::GetPages(CAUUID *pPages) 
{
    CheckPointer(pPages,E_POINTER);

    pPages->cElems = 1;
    pPages->pElems = (GUID *) CoTaskMemAlloc(sizeof(GUID));

    if (pPages->pElems == NULL)
        return E_OUTOFMEMORY;

    *(pPages->pElems) = __uuidof(MPDSTProperties);

    return NOERROR;

} // GetPages
STDMETHODIMP TSMuxSplitter::NonDelegatingQueryInterface(REFIID riid,void **ppv)
{
	if (riid == IID_IBaseFilter) 
	{
        return GetInterface((IBaseFilter *) this, ppv);
	}

	if (riid == IID_ISpecifyPropertyPages) 
	{
        return GetInterface((ISpecifyPropertyPages *) this, ppv);
	}
	return CUnknown::NonDelegatingQueryInterface(riid,ppv);
}
CBasePin *TSMuxSplitter::GetPin(int n)
{
    if (n == 0)
    {
        return m_pInput;
    } else if (n == 1)
    {
        return m_pVideo;
    } else if (n == 2)
    {
        return m_pAudio;
    }
    return NULL;
}

HRESULT TSMuxSplitter::BeginFlush()
{
    // flush each output pin
    m_pVideo->DeliverBeginFlush();
    m_pAudio->DeliverBeginFlush();
    return S_OK;
}

HRESULT TSMuxSplitter::EndFlush()
{
    // empty parse buffer
    m_demuxer.Empty();

    // pass to all output pins
    m_pVideo->DeliverEndFlush();
    m_pAudio->DeliverEndFlush();
    
    return S_OK;
}

HRESULT TSMuxSplitter::EndOfStream()
{
    m_pVideo->DeliverEndOfStream();
    m_pAudio->DeliverEndOfStream();
    return S_OK;
}

STDMETHODIMP TSMuxSplitter::Pause()
{
    FILTER_STATE stateFrom = m_State;

    HRESULT hr = CBaseFilter::Pause();

    if (stateFrom == State_Stopped)
    {
        // reset parser buffer
        m_demuxer.Empty();

        // now the filter is properly active, we
        // can start the pulling worker. Doing in the
        // pin's Active method could result in data
        // arriving before we have changed state.

        Seek(m_tStart, m_tStop, m_dRate);
    }

    return S_OK;
}

HRESULT TSMuxSplitter::Receive(IMediaSample* pSample)
{
	BYTE *pData;
	HRESULT hr=S_OK;
    pSample->GetPointer(&pData);
	CheckPointer(pData,E_POINTER);

    long cBytes = pSample->GetActualDataLength();
	hr=m_demuxer.Process(pData,cBytes,this);
	
	m_pVideo->SendAnyway();
	m_pAudio->SendAnyway();

    return hr;
}
DWORD TSMuxSplitter::GetAudioPID()
{
	return m_demuxer.GetAudioPID();
}
DWORD TSMuxSplitter::GetVideoPID()
{
	return m_demuxer.GetVideoPID();
}

HRESULT TSMuxSplitter::ProcessPacket(BYTE *data,DWORD len,LONGLONG pts,bool audioVideo)
{
   SplitterOutputPin* pPin =(audioVideo==true?m_pAudio:m_pVideo);

   bool bEnd = false;
   REFERENCE_TIME tStart;
   REFERENCE_TIME* ptStart = NULL;

   // convert pts time into DirectShow stream time
   tStart =(pts-m_firstPTS)*1000/9;              
        
   if (tStart > (m_tStop + UNITS))
   {
	   bEnd = true;
   }
                    
   // convert time to be relative to seek segment
                    
   tStart -= m_tStart;
   tStart = REFERENCE_TIME(tStart / m_dRate);
   ptStart=&tStart;
   pPin->DeliverData(data,len,ptStart);

   if (bEnd)
   {
	   EndOfStream();
	   m_pVideo->SendAnyway();
	   m_pAudio->SendAnyway();
	   return S_FALSE;
   }

	return S_OK;
}

HRESULT TSMuxSplitter::CompleteConnect(IPin* pPeer)
{
	m_demuxer.Empty();
	IAsyncReaderPtr pRdr = pPeer;
    if (pRdr == NULL)
    {
        return E_NOINTERFACE;
    }

	m_demuxer.CheckTSFile(pRdr);
	m_firstPTS = m_demuxer.m_firstPTS;
	m_lastPTS = m_demuxer.m_lastPTS;
	
	CreateVideoType();
	CreateAudioType();

    // record the file size in bytes -- even if the file changes, our
    // bitrate estimate is based on this many bytes
	LONGLONG total,ava;
	pRdr->Length(&total,&ava);
    m_llFileSize = total;
	m_demuxer.Empty();
	return S_OK;
}

bool TSMuxSplitter::CreateVideoType()
{
    CMediaType mt;
    mt.SetType(&MEDIATYPE_Video);
    mt.SetSubtype(&MEDIASUBTYPE_MPEG2_VIDEO);
    mt.SetFormatType(&FORMAT_MPEG2Video);
	mt.SetFormat(Mpeg2ProgramVideo,sizeof(Mpeg2ProgramVideo));
    m_pVideo->OfferMediaType(&mt);
    return true;
}


bool TSMuxSplitter::CreateAudioType()
{
    CMediaType mt;
	if(m_demuxer.m_isAC3Audio==false)
	{
		mt.SetType(&MEDIATYPE_Audio);
		mt.SetSubtype(&MEDIASUBTYPE_MPEG2_AUDIO);
		mt.SetFormatType(&FORMAT_WaveFormatEx);
		mt.SetFormat(MPEG1AudioFormat, sizeof(MPEG1AudioFormat));
		m_pAudio->OfferMediaType(&mt);
	}
	else
	{
		WAVEFORMATEX formatBlock;
		ZeroMemory(&formatBlock,sizeof(WAVEFORMATEX));
		mt.SetType(&MEDIATYPE_Audio);
		mt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
		mt.SetFormatType(&FORMAT_WaveFormatEx);
		mt.SetFormat(MPEG1AudioFormat, sizeof(MPEG1AudioFormat));
		m_pAudio->OfferMediaType(&mt);
	}
    return true;
}

bool TSMuxSplitter::SelectSeekingPin(SplitterOutputPin* pPin)
{
    CAutoLock lock(&m_csSeeking);
    if (m_pSeekingPin == NULL)
    {
        m_pSeekingPin = pPin;
    }
    return(m_pSeekingPin == pPin);
}

void TSMuxSplitter::DeselectSeekingPin(SplitterOutputPin* pPin)
{
    CAutoLock lock(&m_csSeeking);
    if (pPin == m_pSeekingPin)
    {
        m_pSeekingPin = pPin;
    }
}

REFERENCE_TIME TSMuxSplitter::GetDuration()
{
    return (m_lastPTS - m_firstPTS)*1000/9;
}

void TSMuxSplitter::GetSeekingParams(REFERENCE_TIME* ptStart, REFERENCE_TIME* ptStop, double* pdRate)
{
    if (ptStart != NULL)
    {
        *ptStart = m_tStart;
    }
    if (ptStop != NULL)
    {
        *ptStop = m_tStop;
    }
    if (pdRate != NULL)
    {
        *pdRate = m_dRate;
    }
}

HRESULT TSMuxSplitter::Seek(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
 
    bool bShouldRestart = m_pInput->Suspend();

    {
        CAutoLock lock(&m_csSeeking);
        m_tStart = tStart;
        m_tStop = tStop;
        m_dRate = dRate;

        m_nTries = 0;
        m_posLast = 0;
        m_tsLast = 0;
        

        REFERENCE_TIME target = m_tStart - UNITS;
        if (target > UNITS)
        {
            m_posThis = target * m_llFileSize / GetDuration();
            m_bLocating = false;
        } else
        {
            m_posThis = 0;
            m_bLocating = false;
        }
    }

    // change pull-pin seeking
    m_posThis=m_posThis/188;
	m_posThis-=1;
	m_posThis*=188;
	// now add the offset to first packet to the position
	m_posThis+=m_demuxer.GetFileOffset();

	m_pInput->Seek(m_posThis);

    // can restart suspended worker now
    if (bShouldRestart)
    {
        m_pInput->Resume();
    }

    return S_OK;
}

HRESULT TSMuxSplitter::SetRate(double dRate)
{
    CAutoLock lock(&m_csSeeking);
    m_dRate = dRate;
    return S_OK;
}


HRESULT TSMuxSplitter::SetStopTime(REFERENCE_TIME tStop)
{
    CAutoLock lock(&m_csSeeking);
    // this does not guarantee that a stop change only, while running,
    // will stop at the right point -- but most filters only
    // implement stop/rate changes when the current position changes
    m_tStop = tStop;
    return S_OK;
}

// --- input pin implementation --------------------------------

StreamInputPin::StreamInputPin(TSMuxSplitter* pFilter, CCritSec* pLock, HRESULT* phr)
: m_pPull(NULL),
  m_pParser(pFilter),
  m_bFlushing(false),
  CBasePin(NAME("StreamInputPin"), pFilter, pLock, phr, L"Input", PINDIR_INPUT)
{
}

StreamInputPin::~StreamInputPin()
{
    // only delete it if it exists!
    // thanks to Paul Hatcher for pointing out this bug.
    if (m_pPull)
    {
        m_pPull->Inactive();
        delete m_pPull;
    }
}

// base pin overrides
HRESULT StreamInputPin::CheckMediaType(const CMediaType* pmt)
{
    if (*pmt->Type() == MEDIATYPE_Stream)
    {
        return S_OK;
    }
    return S_FALSE;
}

HRESULT StreamInputPin::GetMediaType(int iPosition, CMediaType* pmt)
{
    if (iPosition != 0)
    {
        return VFW_S_NO_MORE_ITEMS;
    }
    pmt->InitMediaType();
    pmt->SetType(&MEDIATYPE_Stream);
    pmt->SetSubtype(&MEDIASUBTYPE_NULL);
    return S_OK;
}

STDMETHODIMP StreamInputPin::BeginFlush()
{
    // pass the flush call downstream via the filter
    m_bFlushing = true;
    return m_pParser->BeginFlush();
}

STDMETHODIMP StreamInputPin::EndFlush()
{
    m_bFlushing = false;
    return m_pParser->EndFlush();
}

HRESULT StreamInputPin::CompleteConnect(IPin* pPeer)
{
    HRESULT hr = CBasePin::CompleteConnect(pPeer);

    if (SUCCEEDED(hr))
    {
        // validate input with parser
        hr = m_pParser->CompleteConnect(pPeer);
    }

    if (SUCCEEDED(hr))
    {
        IMemAllocatorPtr pAlloc;
        hr = CreateMemoryAllocator(&pAlloc);
        if (FAILED(hr))
        {
            return hr; 
        }

         m_pPull = new implPullPin(this);
        hr = m_pPull->Connect(pPeer, pAlloc, true);
    }
    return hr;
}

HRESULT StreamInputPin::Seek(REFERENCE_TIME tStart)
{
    HRESULT hr = S_FALSE;
    if (m_pPull)
    {
         hr = m_pPull->Seek(tStart * UNITS, MAX_TIME);
        if (SUCCEEDED(hr))
        {
            if (!m_pPull->ThreadExists())
            {
                hr = m_pPull->Active();
            }
        }
    }
    return hr;
}

bool StreamInputPin::Suspend()
{
    bool bShouldRestart = false;
    if (m_pPull)
    {
        bShouldRestart = m_pPull->Suspend();
    }
    return bShouldRestart;
}

void StreamInputPin::Resume()
{
    if (m_pPull)
    {
        m_pPull->StartThread();
    }
}

HRESULT StreamInputPin::Inactive()
{
    if (m_pPull != NULL)
    {
        m_pPull->Inactive();
    }
    return S_OK;
}

STDMETHODIMP StreamInputPin::Receive(IMediaSample* pSample)
{
    if (IsStopped())
    {
        return VFW_E_WRONG_STATE;
    }
    if (m_bFlushing)
    {
        return S_FALSE;
    }

    return m_pParser->Receive(pSample);
}

STDMETHODIMP StreamInputPin::EndOfStream()
{
    return m_pParser->EndOfStream();
}


StreamInputPin::implPullPin::implPullPin(StreamInputPin* pPin)
: m_pPin(pPin)
{
}

HRESULT StreamInputPin::implPullPin::BeginFlush()
{
    return m_pPin->BeginFlush();
}

HRESULT StreamInputPin::implPullPin::EndFlush()
{
    return m_pPin->EndFlush();
}

HRESULT StreamInputPin::implPullPin::Receive(IMediaSample* pSample)
{
    return m_pPin->Receive(pSample);
}

HRESULT StreamInputPin::implPullPin::EndOfStream()
{
    return m_pPin->EndOfStream();
}

void StreamInputPin::implPullPin::OnError(HRESULT hr)
{
}

// --- output pin implementation -----------------------

SplitterOutputPin::SplitterOutputPin(
    TSMuxSplitter* pParser, 
    CCritSec* pLock, 
    HRESULT* phr, 
    LPCWSTR pName)
: m_pParser(pParser),
  m_pOutputQ(NULL),
  m_bDiscont(true),
  CBaseOutputPin(NAME("SplitterOutputPin"), pParser, pLock, phr, pName)
{
}

STDMETHODIMP SplitterOutputPin::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
    if (iid == IID_IMediaSeeking)
    {
        return GetInterface((IMediaSeeking*)this, ppv);
    } else
    {
        return CBaseOutputPin::NonDelegatingQueryInterface(iid, ppv);
    }
}

HRESULT SplitterOutputPin::CheckMediaType(const CMediaType* pmt)
{
    if ((*m_mtPayload.Type() != GUID_NULL) && (*pmt == m_mtPayload))
    {
        return S_OK;
    }
    return S_FALSE;
}

HRESULT SplitterOutputPin::GetMediaType(int iPosition, CMediaType* pmt)
{
    HRESULT hr = S_OK;
    if (iPosition == 0)
    {
        *pmt = m_mtPayload;
    }else
    {
        hr = VFW_S_NO_MORE_ITEMS;
    }
    return hr;
}

HRESULT SplitterOutputPin::SetMediaType(const CMediaType* pmt)
{
   return CBaseOutputPin::SetMediaType(pmt);
}

HRESULT SplitterOutputPin::DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pprop)
{
    pprop->cbBuffer = 64 * 1024; 
    pprop->cBuffers = 30;
    pprop->cbAlign = 1;
    pprop->cbPrefix = 0;
    ALLOCATOR_PROPERTIES propActual;

    return pAlloc->SetProperties(pprop, &propActual);
}

HRESULT SplitterOutputPin::CompleteConnect(IPin *pPin)
{
	HRESULT hr;
	AM_MEDIA_TYPE mt;
	this->ConnectionMediaType(&mt);
	
	if(mt.majortype==MEDIATYPE_Video)
	{
		if(m_pParser->GetVideoPID()==0)
			return S_FALSE;
		else
			hr=CBaseOutputPin::CompleteConnect(pPin);
	}
	
	if(mt.majortype==MEDIATYPE_Audio)
	{
		if(m_pParser->GetAudioPID()==0)
			return S_FALSE;
		else
			hr=CBaseOutputPin::CompleteConnect(pPin);
	}
	return hr;
}

// this group of methods deal with the COutputQueue
HRESULT SplitterOutputPin::Active()
{
    m_bDiscont = true;

    HRESULT hr = S_OK;
    if (IsConnected())
    {
        hr = CBaseOutputPin::Active();
        if (SUCCEEDED(hr))
        {
            m_pOutputQ = new COutputQueue(GetConnected(),&hr,false,true,10,1,10);
            if (m_pOutputQ == NULL)
            {
                hr = E_OUTOFMEMORY;
            } else if (FAILED(hr))
            {
                delete m_pOutputQ;
                m_pOutputQ = NULL;
            }
        }
    }
    return hr;
}

HRESULT SplitterOutputPin::Inactive()
{
    HRESULT hr = CBaseOutputPin::Inactive();

    delete m_pOutputQ;
    m_pOutputQ = NULL;

    return hr;
}

HRESULT SplitterOutputPin::DeliverEndOfStream()
{
    if (m_pOutputQ != NULL)
    {
       		m_pOutputQ->EOS();
    }
    return S_OK;
}

HRESULT SplitterOutputPin::DeliverBeginFlush()
{
    if (m_pOutputQ != NULL)
    {
        m_pOutputQ->BeginFlush();
    }
    return S_OK;
}

HRESULT SplitterOutputPin::DeliverEndFlush()
{
    if (m_pOutputQ != NULL)
    {
        m_pOutputQ->EndFlush();
    }
    m_bDiscont = true;

    return S_OK;
}

void SplitterOutputPin::OfferMediaType(const CMediaType* pmtPayload)
{
    m_mtPayload.InitMediaType();
    m_mtPayload = *pmtPayload;
}

HRESULT SplitterOutputPin::DeliverData(const BYTE* pData, long cBytes, const REFERENCE_TIME* ptStart)
{
    if (!IsConnected() || (m_pOutputQ == NULL))
    {
        return S_FALSE;
    }

    // get an empty buffer
    IMediaSamplePtr pSample;
    HRESULT hr = GetDeliveryBuffer(&pSample, NULL, NULL, 0);
    if (FAILED(hr))
    {
        return hr;
    }
    if (pSample->GetSize() < cBytes)
    {
        return E_INVALIDARG;
    }

    // copy the data into the buffer
    BYTE* pBuffer;
    pSample->GetPointer(&pBuffer);
    CopyMemory(pBuffer, pData, cBytes);
    pSample->SetActualDataLength(cBytes);

    // set the discontinuity if we know there has
    // been a break (eg first sample on active)
    if (m_bDiscont)
    {
        m_bDiscont = false;
        pSample->SetDiscontinuity(true);

        // inform downstream graph of precise segment selection when seeking
        REFERENCE_TIME tSeek, tSeekStop;
        double dRate;
        m_pParser->GetSeekingParams(&tSeek, &tSeekStop, &dRate);
        m_pOutputQ->NewSegment(tSeek, tSeekStop, dRate);
    }

    // set the time stamp. 
    // For decoders and renderers it makes sense to keep the start and stop times
    // of consecutive samples contiguous. But this is done in the decoder -- there is 
    // no benefit to doing it here. We only have a start time, so make the stop time
    // just start+1
    // Remember that we only place a time stamp if the PES packet header had a pts value.
    if (ptStart != NULL)
    {
        REFERENCE_TIME tStart = *ptStart;
        REFERENCE_TIME tStop = tStart + 1;
        pSample->SetTime(&tStart, &tStop);
    }

    // note that we must give a refcount to the output Q
    pSample->AddRef();
    return m_pOutputQ->Receive(pSample);
}

// -- output pin seeking methods -------------------------

STDMETHODIMP SplitterOutputPin::GetCapabilities(DWORD * pCapabilities)
{

     *pCapabilities =        AM_SEEKING_CanSeekAbsolute |
                            AM_SEEKING_CanSeekForwards |
                            AM_SEEKING_CanSeekBackwards |
                            AM_SEEKING_CanGetDuration |
                            AM_SEEKING_CanGetCurrentPos |
                            AM_SEEKING_CanGetStopPos;
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::CheckCapabilities(DWORD * pCapabilities)
{
    DWORD dwActual;
    GetCapabilities(&dwActual);
    if (*pCapabilities & (~dwActual))
    {
        return S_FALSE;
    }
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::IsFormatSupported(const GUID * pFormat)
{
     if (*pFormat == TIME_FORMAT_MEDIA_TIME)
    {
        return S_OK;
    }
    return S_FALSE;

}
STDMETHODIMP SplitterOutputPin::QueryPreferredFormat(GUID * pFormat)
{
    *pFormat = TIME_FORMAT_MEDIA_TIME;
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::GetTimeFormat(GUID *pFormat)
{
    return QueryPreferredFormat(pFormat);
}

STDMETHODIMP SplitterOutputPin::IsUsingTimeFormat(const GUID * pFormat)
{
    GUID guidActual;
    HRESULT hr = GetTimeFormat(&guidActual);

    if (SUCCEEDED(hr) && (guidActual == *pFormat))
    {
        return S_OK;
    } else
    {
        return S_FALSE;
    }
}

STDMETHODIMP SplitterOutputPin::ConvertTimeFormat(
                                      LONGLONG* pTarget, 
                                      const GUID* pTargetFormat,
                                      LONGLONG Source, 
                                      const GUID* pSourceFormat)
{
     if (pTargetFormat == 0 || *pTargetFormat == TIME_FORMAT_MEDIA_TIME)
    {
        if (pSourceFormat == 0 || *pSourceFormat == TIME_FORMAT_MEDIA_TIME)
        {
            *pTarget = Source;
            return S_OK;
        }
    }

    return E_INVALIDARG;
}

STDMETHODIMP SplitterOutputPin::SetTimeFormat(const GUID * pFormat)
{
    // only one pin can control seeking for the whole filter.
    // This method is used to select the seeker.
    if (*pFormat == TIME_FORMAT_MEDIA_TIME)
    {
        // try to select this pin as seeker (if the first to do so)
        if (m_pParser->SelectSeekingPin(this))
        {
            return S_OK;
        } else
        {
            return E_NOTIMPL;
        }
    } else if (*pFormat == TIME_FORMAT_NONE)
    {
        // deselect ourself, if we were the controlling pin
        m_pParser->DeselectSeekingPin(this);
        return S_OK;
    } else
    {
        // no other formats supported
        return E_NOTIMPL;
    }
}

STDMETHODIMP SplitterOutputPin::GetDuration(LONGLONG *pDuration)
{
    *pDuration = m_pParser->GetDuration();
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::GetStopPosition(LONGLONG *pStop)
{
    REFERENCE_TIME tStart, tStop;
    double dRate;
    m_pParser->GetSeekingParams(&tStart, &tStop, &dRate);
    *pStop = tStop;
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::GetCurrentPosition(LONGLONG *pCurrent)
{
    // this method is not supposed to report the previous start
    // position, but rather where we are now. This is normally
    // implemented by renderers, not parsers
    return E_NOTIMPL;
}

STDMETHODIMP SplitterOutputPin::SetPositions(
                                 LONGLONG * pCurrent, 
                                 DWORD dwCurrentFlags, 
                                 LONGLONG * pStop, 
                                 DWORD dwStopFlags)
{
    // for media player, with the aggregation bug in DShow, it
    // is better to return success and ignore the call if we are
    // not the controlling pin
    if (!m_pParser->SelectSeekingPin(this))
    {
        return S_OK;
    }

    // fetch current properties
    REFERENCE_TIME tStart, tStop;
    double dRate;
    m_pParser->GetSeekingParams(&tStart, &tStop, &dRate);
    dwCurrentFlags &= AM_SEEKING_PositioningBitsMask;

    if (dwCurrentFlags == AM_SEEKING_AbsolutePositioning)
    {
        tStart = *pCurrent;
    } else if (dwCurrentFlags == AM_SEEKING_RelativePositioning)
    {
        tStart += *pCurrent;
    }

    dwStopFlags &= AM_SEEKING_PositioningBitsMask;
    if (dwStopFlags == AM_SEEKING_AbsolutePositioning)
    {
        tStop = *pStop;
    } else if (dwStopFlags == AM_SEEKING_IncrementalPositioning)
    {
        tStop = *pStop + tStart;
    } else
    {
        if (dwStopFlags == AM_SEEKING_RelativePositioning)
        {
            tStop += *pStop;
        }
    }

    if (dwCurrentFlags)
    {
        return m_pParser->Seek(tStart, tStop, dRate);
    } else if (dwStopFlags)
    {
        // stop change only
        return m_pParser->SetStopTime(tStop);
    } else
    {
        // no operation required
        return S_FALSE;
    }

}

STDMETHODIMP SplitterOutputPin::GetPositions(LONGLONG * pCurrent, LONGLONG * pStop)
{
    REFERENCE_TIME tStart, tStop;
    double dRate;
    m_pParser->GetSeekingParams(&tStart, &tStop, &dRate);
    *pCurrent = tStart;
    *pStop = tStop;
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::GetAvailable(LONGLONG * pEarliest, LONGLONG * pLatest)
{
    if (pEarliest != NULL)
    {
        *pEarliest = 0;
    }
    if (pLatest != NULL)
    {
        *pLatest = m_pParser->GetDuration();
    }
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::SetRate(double dRate)
{
    HRESULT hr = S_OK;
    if (m_pParser->SelectSeekingPin(this))
    {
        hr = m_pParser->SetRate(dRate);
    }
    return hr;
}



STDMETHODIMP SplitterOutputPin::GetRate(double * pdRate)
{
    REFERENCE_TIME tStart, tStop;
    double dRate;
    m_pParser->GetSeekingParams(&tStart, &tStop, &dRate);
    *pdRate = dRate;
    return S_OK;
}

STDMETHODIMP SplitterOutputPin::GetPreroll(LONGLONG * pllPreroll)
{
    // don't need to allow any preroll time for us
    *pllPreroll = 0;
    return S_OK;
}

