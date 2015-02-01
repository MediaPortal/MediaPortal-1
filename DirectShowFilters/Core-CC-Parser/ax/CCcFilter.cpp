#include "StdAfx.h"

#include "CCPGUIDs.h"
#include "CCCP.h"
#include "mediaformats.h"

const GUID CCcFilter::m_guidPassThroughMediaMajor   = MEDIATYPE_Video;

const WCHAR CCcFilter::m_szInput[]       = L"Input";
const WCHAR CCcFilter::m_szPassThrough[] = L"Pass Through";

const GUID CLine21OutputPin::m_guidMediaMajor   = MEDIATYPE_AUXLine21Data;
const GUID CLine21OutputPin::m_guidMediaSubtype = MEDIASUBTYPE_Line21_BytePair;
const WCHAR CLine21OutputPin::m_szName[]        = L"Line 21";

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	CCcFilterInputPin

CCcFilterInputPin::CCcFilterInputPin( CCcFilter* pFilter )
: CTransInPlaceInputPin( _T("Input pin"), pFilter, NULL, CCcFilter::m_szInput )
{

}

HRESULT CCcFilterInputPin::CheckStreaming()
{
	//TODO:     if (!m_pTransformFilter->m_pOutput->IsConnected())
    //TODO:         return VFW_E_NOT_CONNECTED;
	
	return CBaseInputPin::CheckStreaming(); // Note: great grandparent
}

STDMETHODIMP CCcFilterInputPin::BeginFlush()
{
//TODONOW    CAutoLock lck(&m_pTransformFilter->m_csFilter);
/*TODO    //  Are we actually doing anything?
    ASSERT(m_pTransformFilter->m_pOutput != NULL);
    if (!IsConnected() ||
        !m_pTransformFilter->m_pOutput->IsConnected()) {
        return VFW_E_NOT_CONNECTED;
    }*/
    HRESULT hr = CBaseInputPin::BeginFlush(); // Note: great grandparent
    if (FAILED(hr)) {
    	return hr;
    }

    return m_pTransformFilter->BeginFlush();
}

STDMETHODIMP CCcFilterInputPin::EndFlush()
{
//TODONOW        CAutoLock lck(&m_pTransformFilter->m_csFilter);
/*TODO    //  Are we actually doing anything?
    ASSERT(m_pTransformFilter->m_pOutput != NULL);
    if (!IsConnected() ||
        !m_pTransformFilter->m_pOutput->IsConnected()) {
        return VFW_E_NOT_CONNECTED;
    }  */

    HRESULT hr = m_pTransformFilter->EndFlush();
    if (FAILED(hr)) {
        return hr;
    }

    return CBaseInputPin::EndFlush();
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	CLine21OutputPin

CLine21OutputPin::CLine21OutputPin( CCcFilter* pFilter )
:	CBaseOutputPin( _T("Line 21 output pin"), pFilter, &pFilter->m_csFilter, NULL, CLine21OutputPin::m_szName )
{

}

HRESULT CLine21OutputPin::GetMediaType( int iPosition, CMediaType* pMediaType )
{
    if (iPosition == 0)
    {
		pMediaType->majortype = m_guidMediaMajor;
		pMediaType->subtype = m_guidMediaSubtype;

        return S_OK;
    }
    else
		return VFW_S_NO_MORE_ITEMS;
}

HRESULT CLine21OutputPin::CheckMediaType( const CMediaType* pmtOut )
{
	if(	pmtOut->majortype != m_guidMediaMajor ||
		pmtOut->subtype   != m_guidMediaSubtype 
	  )
		return VFW_E_TYPE_NOT_ACCEPTED;

    return S_OK;
}
	
HRESULT CLine21OutputPin::DecideBufferSize(IMemAllocator* pAllocator, ALLOCATOR_PROPERTIES* pProp )
{
	pProp->cbAlign = 2;
	
	if( pProp->cBuffers < 20 )   //TODO???
    	pProp->cBuffers = 20;

	pProp->cbBuffer = 256;

    // Set allocator properties.
    ALLOCATOR_PROPERTIES Actual;
    HRESULT hr = pAllocator->SetProperties(pProp, &Actual);
    if (FAILED(hr)) 
        return hr;

	// Even when it succeeds, check the actual result.
    if (pProp->cbBuffer > Actual.cbBuffer) 
        return E_FAIL;

	return S_OK;
}

HRESULT CLine21OutputPin::CompleteConnect( IPin* pReceivePin )
{
	RETURN_FAILED( CBaseOutputPin::CompleteConnect( pReceivePin ));

	VERIFY( SUCCEEDED( UpdateCurrentService( pReceivePin )));

	return S_OK;
}

HRESULT CLine21OutputPin::UpdateCurrentService( IPin* pReceivePin )
{
	AM_LINE21_CCSERVICE idService;
	VERIFY( SUCCEEDED( GetFilter()->get_Service( &idService )));
	
	if( !pReceivePin )
		pReceivePin = GetConnected();

	if( !pReceivePin )
		return S_FALSE;
	
	PIN_INFO infoOut;
	RETURN_FAILED( pReceivePin->QueryPinInfo( &infoOut ));
	if( !infoOut.pFilter )
	{
		ASSERT(0);
		return S_FALSE;
	}

	auto_pif<IAMLine21Decoder> pifLine21Decoder;
	RETURN_FAILED( infoOut.pFilter->QueryInterface( IID_IAMLine21Decoder, (void**)pifLine21Decoder.AcceptHere()));
	infoOut.pFilter->Release();
	
	ASSERT( pifLine21Decoder );

	return pifLine21Decoder->SetCurrentService( idService );
}

STDMETHODIMP CLine21OutputPin::Notify( IBaseFilter* pSender, Quality q )
{
    UNREFERENCED_PARAMETER(pSender);
    ValidateReadPtr(pSender,sizeof(IBaseFilter));

    CCcFilter* pFilter = GetFilter();

    HRESULT hr = pFilter->AlterQuality(q);
    if (hr!=S_FALSE) {
        return hr;        // either S_OK or a failure
    }

    // S_FALSE means we pass the message on.
    // Find the quality sink for our input pin and send it there

    ASSERT(pFilter->m_pInput != NULL);

    return pFilter->m_pInput->PassNotify(q);
}

CCcFilterPassThroughPin::CCcFilterPassThroughPin( CCcFilter* pFilter )
: CTransInPlaceOutputPin( NAME("Pass through output pin"), pFilter, NULL, CCcFilter::m_szPassThrough )
{

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CCcFilter

#pragma warning( push )
#pragma warning( disable : 4355 ) // 'this' : used in base member initializer list

CCcFilter::CCcFilter(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr)
    : CTransInPlaceFilter (tszName, punk, CLSID_CcParser, phr)
    , CPersistStream(punk, phr)
	, m_inpinInput( this )
	, m_outpinPassThrough( this )
	, m_outpinLine21( this )
	, m_pPassThroughQueue(NULL)
	, m_pLine21Queue(NULL)
{
    m_nThisInstance = ++m_nInstanceCount; // Useful for debug, no other purpose

    DbgFunc("CCcFilter");

	m_pInput  = &m_inpinInput;
	m_pOutput = &m_outpinPassThrough;
	
	m_bIsSubtypeAVC1 = false;
	m_guidPassThroughMediaSubtype = GUID_NULL;

	m_proc.put_XformType( ICcParser_CCTYPE_ATSC_A53 ); //TODO: remove
}
#pragma warning( pop )

CCcFilter::~CCcFilter()
{
	ASSERT( NULL == m_pPassThroughQueue );
	ASSERT( NULL == m_pLine21Queue );
	
	ASSERT( m_pOutput == &m_outpinPassThrough );
	m_pOutput = NULL; // To prevent its destruction

	ASSERT( m_pInput == &m_inpinInput );
	m_pInput = NULL; // To prevent its destruction
}

int CCcFilter::GetPinCount()
{
	return 3;
}

CBasePin* CCcFilter::GetPin( int iPin )
{
	if( 2 == iPin )
		return &m_outpinLine21;

	return CTransInPlaceFilter::GetPin( iPin );
}

STDMETHODIMP CCcFilter::FindPin(LPCWSTR Id, IPin **ppPin)
{
    CheckPointer(ppPin,E_POINTER);
    ValidateReadWritePtr(ppPin,sizeof(IPin *));

    if (0==lstrcmpW( Id, CLine21OutputPin::m_szName ))
        *ppPin = &m_outpinLine21;
    else if (0==lstrcmpW( Id, m_szPassThrough ))
        *ppPin = &m_outpinPassThrough;
    else if (0==lstrcmpW( Id, m_szInput ))
        *ppPin = &m_inpinInput;
    else
		return CTransInPlaceFilter::FindPin(Id, ppPin);

    HRESULT hr = NOERROR;
    //  AddRef() returned pointer - but GetPin could fail if memory is low.
    if (*ppPin) {
        (*ppPin)->AddRef();
    } else {
        hr = E_OUTOFMEMORY;  // probably.  There's no pin anyway.
    }
    return hr;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CCcFilter::CheckInputType

HRESULT CCcFilter::CheckInputType(const CMediaType *pmt)
{
    CheckPointer(pmt,E_POINTER);

    DisplayType(TEXT("CheckInputType"), pmt);

    if( m_guidPassThroughMediaMajor != pmt->majortype || GUID_NULL == pmt->subtype)  
	  {
      m_guidPassThroughMediaSubtype = GUID_NULL;
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
    
    //We can only process particular MPEG1/MPEG2/AVC1 stream formats
    if (pmt->subtype != MEDIASUBTYPE_MPEG2_VIDEO && pmt->subtype != MPG4_SubType && pmt->subtype != MEDIASUBTYPE_MPEG1Payload)  
	  {
      m_guidPassThroughMediaSubtype = GUID_NULL;
      m_bIsSubtypeAVC1 = false;
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
    
    m_guidPassThroughMediaSubtype = pmt->subtype;
    m_bIsSubtypeAVC1 = (pmt->subtype == MPG4_SubType);
    
    return NOERROR;

} // CheckInputType

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Transform

HRESULT CCcFilter::InitializeOutputSample( CBaseOutputPin* pPin, IMediaSample *pSample, IMediaSample **ppOutSample )
{
	ASSERT( m_pOutput == &m_outpinPassThrough );
	m_pOutput = static_cast<CTransformOutputPin*>( pPin );
	
	HRESULT hr = CTransformFilter::InitializeOutputSample( pSample, ppOutSample );

	m_pOutput = &m_outpinPassThrough;

	return hr;
}

HRESULT CCcFilter::Receive( IMediaSample* pSourceSample )
{
  CAutoLock lock_it(m_pLock);

	if( !m_pPassThroughQueue && !m_pLine21Queue )
	    return NOERROR;

  //  Check for other streams and pass them on
  AM_SAMPLE2_PROPERTIES * const pProps = m_pInput->SampleProps();
  if( pProps->dwStreamId != AM_STREAM_MEDIA ) 
	{
		if( m_pPassThroughQueue )
		{
			pSourceSample->AddRef();
			m_pPassThroughQueue->Receive( pSourceSample );
		}

		if( m_pLine21Queue )
		{
			pSourceSample->AddRef();
			m_pLine21Queue->Receive( pSourceSample );
		}

		return S_OK;
  }

	int cbData = pSourceSample->GetActualDataLength();
	const BYTE* pSourceData;
	RETURN_FAILED( pSourceSample->GetPointer( (BYTE**)&pSourceData ));

	m_rgCCData.SetCount(0);
	if( !m_pPassThroughQueue )
	{
		m_proc.ProcessData( cbData, pSourceData, NULL, &m_rgCCData, m_bIsSubtypeAVC1 );
	}
	else
	{
		auto_pif<IMediaSample> pifOutSample = 
			UsingDifferentAllocators() ? Copy( pSourceSample ) : pSourceSample;

		if( !pifOutSample )
			return E_UNEXPECTED;

		BYTE* pToTransform;
		RETURN_FAILED( pifOutSample->GetPointer( &pToTransform ));

		m_proc.ProcessData( cbData, pSourceData, pToTransform, &m_rgCCData, m_bIsSubtypeAVC1 );

		pifOutSample->AddRef();
		m_pPassThroughQueue->Receive( pifOutSample );
	}

	if( m_pLine21Queue )
	{
		int cWORDs = m_rgCCData.GetCount();
		if( cWORDs > 0 )
		{
			for( const WORD* pData = m_rgCCData.GetData(); pData < m_rgCCData.GetData() + cWORDs; ++pData )
			{
				auto_pif<IMediaSample> pifOutSample;
				RETURN_FAILED( InitializeOutputSample( &m_outpinLine21, pSourceSample, pifOutSample.AcceptHere()));

				if( !pifOutSample )
					return E_UNEXPECTED;

				BYTE* pBufferOut = NULL;
				RETURN_FAILED( pifOutSample->GetPointer(&pBufferOut));

				enum{ cbBufferOut = 2 };
				ASSERT( sizeof( pData[0]) == cbBufferOut  );

				if( pifOutSample->GetSize() < cbBufferOut )
					return E_UNEXPECTED;
				
				memcpy( pBufferOut, pData, cbBufferOut );
				pifOutSample->SetActualDataLength( cbBufferOut );

				//pifOutSample->SetSyncPoint(TRUE);
				
				pifOutSample->AddRef();
				HRESULT hr = m_pLine21Queue->Receive( pifOutSample );
				m_bSampleSkipped = FALSE;	// last thing no longer dropped

				RETURN_FAILED( hr );
			}

			m_rgCCData.SetCount(0);
        } 
		else 
		{
            m_bSampleSkipped = TRUE;
            
			if (!m_bQualityChanged) 
			{
                NotifyEvent(EC_QUALITY_CHANGE,0,0);
                m_bQualityChanged = TRUE;
            }
		}
	}

    return S_OK;
}

STDMETHODIMP CCcFilter::Stop()
{
    CBaseFilter::Stop(); // Note: great gradnparent
    ASSERT( m_State == State_Stopped );

	delete m_pPassThroughQueue; m_pPassThroughQueue = NULL;
	delete m_pLine21Queue;	    m_pLine21Queue = NULL;

	return NOERROR;
}

static void CreateQueue( COutputQueue** ppQueue, IPin* pPin )
{
    ASSERT( ppQueue );
	
	if( NULL == *ppQueue && pPin )
	{
		HRESULT hr = S_OK;
		COutputQueue* pOutputQueue = new COutputQueue( pPin, &hr, TRUE, FALSE);
		if( pOutputQueue != NULL && SUCCEEDED(hr))
		{
			*ppQueue = pOutputQueue;

			return;
		}

		delete pOutputQueue;
	}
}

STDMETHODIMP CCcFilter::Pause()
{
    CreateQueue( &m_pPassThroughQueue, m_outpinPassThrough.GetConnected());
    CreateQueue( &m_pLine21Queue,	   m_outpinLine21.GetConnected());

	CBaseFilter::Pause();  // Note: great grandparent
    ASSERT( m_State == State_Paused );
//	m_pifIPSample = NULL;

    return NOERROR;
}

HRESULT CCcFilter::EndOfStream()
{
	if( m_pPassThroughQueue )
		m_pPassThroughQueue->EOS();

	if( m_pLine21Queue )
		m_pLine21Queue->EOS();

	return S_OK;
}

HRESULT CCcFilter::BeginFlush()
{
	if( m_pPassThroughQueue )
		m_pPassThroughQueue->BeginFlush();

	if( m_pLine21Queue )
		m_pLine21Queue->BeginFlush();

	return S_OK;
}

HRESULT CCcFilter::EndFlush()
{
	if( m_pPassThroughQueue )
		m_pPassThroughQueue->EndFlush();

	if( m_pLine21Queue )
		m_pLine21Queue->EndFlush();

	return S_OK;
}

HRESULT CCcFilter::NewSegment( REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
	if( m_pPassThroughQueue )
		m_pPassThroughQueue->NewSegment( tStart, tStop, dRate );

	if( m_pLine21Queue )
		m_pLine21Queue->NewSegment( tStart, tStop, dRate );

	return S_OK;
}


