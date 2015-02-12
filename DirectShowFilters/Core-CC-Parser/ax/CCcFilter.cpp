
#pragma warning(disable:4996)

#include "StdAfx.h"

#include "CCPGUIDs.h"
#include "CCCP.h"
#include "mediaformats.h"
#include <dvdmedia.h>
#include <shlobj.h>
#include <queue>

// uncomment the //LogDebug to enable extra logging
#define LOG_DETAIL //LogDebug

//These are global variables, and can be shared between multiple CCcFilter instances !
long m_instanceCount = 0;
CCritSec m_instanceLock;

//-------------------- Async logging methods -------------------------------------------------

//These are global variables, and can be shared between multiple CCcFilter instances !
WORD logFileParsed = -1;
WORD logFileDate = -1;

CCcFilter* instanceID = 0;

CCritSec m_qLock;
CCritSec m_logLock;
CCritSec m_logFileLock;
std::queue<std::wstring> m_logQueue;
BOOL m_bLoggerRunning = false;
HANDLE m_hLogger = NULL;
CAMEvent m_EndLoggingEvent;

void LogPath(TCHAR* dest, TCHAR* name)
{
  TCHAR folder[MAX_PATH];
  SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  _stprintf(dest, _T("%s\\Team Mediaportal\\MediaPortal\\log\\CoreCC.%s"), folder, name);
}


void LogRotate()
{   
  CAutoLock lock(&m_logFileLock);
    
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));
  
  try
  {
    // Get the last file write date
    WIN32_FILE_ATTRIBUTE_DATA fileInformation; 
    if (GetFileAttributesEx(fileName, GetFileExInfoStandard, &fileInformation))
    {  
      // Convert the write time to local time.
      SYSTEMTIME stUTC, fileTime;
      if (FileTimeToSystemTime(&fileInformation.ftLastWriteTime, &stUTC))
      {
        if (SystemTimeToTzSpecificLocalTime(NULL, &stUTC, &fileTime))
        {
          logFileDate = fileTime.wDay;
        
          SYSTEMTIME systemTime;
          GetLocalTime(&systemTime);
          
          if(fileTime.wDay == systemTime.wDay)
          {
            //file date is today - no rotation needed
            return;
          }
        } 
      }   
    }
  }  
  catch (...) {}
  
  TCHAR bakFileName[MAX_PATH];
  LogPath(bakFileName, _T("bak"));
  _tremove(bakFileName);
  _trename(fileName, bakFileName);
}


wstring GetLogLine()
{
  CAutoLock lock(&m_qLock);
  if ( m_logQueue.size() == 0 )
  {
    return L"";
  }
  wstring ret = m_logQueue.front();
  m_logQueue.pop();
  return ret;
}


UINT CALLBACK LogThread(void* param)
{
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));
  while ( m_bLoggerRunning || (m_logQueue.size() > 0) ) 
  {
    if ( m_logQueue.size() > 0 ) 
    {
      SYSTEMTIME systemTime;
      GetLocalTime(&systemTime);
      if(logFileParsed != systemTime.wDay)
      {
        LogRotate();
        logFileParsed=systemTime.wDay;
        LogPath(fileName, _T("log"));
      }
      
      CAutoLock lock(&m_logFileLock);
      FILE* fp = _tfopen(fileName, _T("a+"));
      if (fp!=NULL)
      {
        SYSTEMTIME systemTime;
        GetLocalTime(&systemTime);
        wstring line = GetLogLine();
        while (!line.empty())
        {
          fwprintf_s(fp, L"%s", line.c_str());
          line = GetLogLine();
        }
        fclose(fp);
      }
      else //discard data
      {
        wstring line = GetLogLine();
        while (!line.empty())
        {
          line = GetLogLine();
        }
      }
    }
    if (m_bLoggerRunning)
    {
      m_EndLoggingEvent.Wait(1000); //Sleep for 1000ms, unless thread is ending
    }
    else
    {
      Sleep(1);
    }
  }
  return 0;
}


void StartLogger()
{
  UINT id;
  m_hLogger = (HANDLE)_beginthreadex(NULL, 0, LogThread, 0, 0, &id);
  SetThreadPriority(m_hLogger, THREAD_PRIORITY_BELOW_NORMAL);
}


void StopLogger()
{
  CAutoLock logLock(&m_logLock);
  if (m_hLogger)
  {
    m_bLoggerRunning = FALSE;
    m_EndLoggingEvent.Set();
    WaitForSingleObject(m_hLogger, INFINITE);	
    m_EndLoggingEvent.Reset();
    m_hLogger = NULL;
    logFileParsed = -1;
    logFileDate = -1;
    instanceID = 0;
  }
}


void LogDebug(const wchar_t *fmt, ...) 
{
  CAutoLock logLock(&m_logLock);
  
  if (!m_hLogger) {
    m_bLoggerRunning = true;
    StartLogger();
  }

  wchar_t buffer[2000]; 
  int tmp;
  va_list ap;
  va_start(ap,fmt);
  tmp = vswprintf_s(buffer, fmt, ap);
  va_end(ap); 

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  wchar_t msg[5000];
  swprintf_s(msg, 5000,L"[%04.4d-%02.2d-%02.2d %02.2d:%02.2d:%02.2d,%03.3d] [%8x] [%4x] - %s\n",
    systemTime.wYear, systemTime.wMonth, systemTime.wDay,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds,
    instanceID,
    GetCurrentThreadId(),
    buffer);
  CAutoLock l(&m_qLock);
  if (m_logQueue.size() < 2000) 
  {
    m_logQueue.push((wstring)msg);
  }
};

void LogDebug(const char *fmt, ...)
{
  char logbuffer[2000]; 
  wchar_t logbufferw[2000];

	va_list ap;
	va_start(ap,fmt);
	vsprintf_s(logbuffer, fmt, ap);
	va_end(ap); 

	MultiByteToWideChar(CP_ACP, 0, logbuffer, -1,logbufferw, sizeof(logbuffer)/sizeof(wchar_t));
	LogDebug(L"%s", logbufferw);
};

//------------------------------------------------------------------------------------

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
  { // Scope for CAutoLock
    CAutoLock lock(&m_instanceLock);  
    m_instanceCount++;
  }
  m_nThisInstance = ++m_nInstanceCount; // Useful for debug, no other purpose

  DbgFunc("CCcFilter");

	m_pInput  = &m_inpinInput;
	m_pOutput = &m_outpinPassThrough;
	
	m_bIsSubtypeAVC1 = false;
	m_guidPassThroughMediaSubtype = GUID_NULL;
	m_dwFlags = 0;

	m_proc.put_XformType( ICcParser_CCTYPE_ATSC_A53 ); //TODO: remove
	
	instanceID = this;  

  LogDebug(" ");
  LogDebug("=================== New filter instance =========================================");
  LogDebug("  Logging format: [Date Time] [InstanceID-instanceCount] [ThreadID] Message....  ");
  LogDebug("==================================================================================");
  LogDebug("------------- v1.0.0.0 ------------- instanceCount:%d", m_instanceCount);

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
	
  { // Scope for CAutoLock
    CAutoLock lock(&m_instanceLock); 
    if (m_instanceCount > 0) 
    {
      m_instanceCount--;
    }
  }
  LogDebug("CCcFilter::dtor - finished, instanceCount:%d", m_instanceCount);
  StopLogger();
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
    
    m_dwFlags = 0;
    
    if (pmt->formattype == FORMAT_MPEG2Video)
    {
      // Check the buffer size.
      if (pmt->cbFormat >= sizeof(MPEG2VIDEOINFO))
      {
        MPEG2VIDEOINFO *pVih = reinterpret_cast<MPEG2VIDEOINFO*>(pmt->pbFormat);
          /* Access MPEG2VIDEOINFO members through pVih. */
        m_dwFlags = pVih->dwFlags;
      }
    }
    
    if (pmt->subtype == MPG4_SubType)
    {
      m_bIsSubtypeAVC1 = true;
      LogDebug ("CCcFilter: CheckInputType() - Mediasubtype = AVC1, m_dwFlags: %d", m_dwFlags);
    }
    else
    {
      m_bIsSubtypeAVC1 = false;      
      if (pmt->subtype == MEDIASUBTYPE_MPEG2_VIDEO)  
  	  {
        LogDebug ("CCcFilter: CheckInputType() - Mediasubtype = MPEG2, m_dwFlags: %d", m_dwFlags);
  	  }
  	  else
  	  {
        LogDebug ("CCcFilter: CheckInputType() - Mediasubtype = MPEG1, m_dwFlags: %d", m_dwFlags);
  	  }
    }
    
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
	
	REFERENCE_TIME sourceTimeStart = _I64_MAX;
	REFERENCE_TIME sourceTimeEnd;
	HRESULT gthr = pSourceSample->GetTime(&sourceTimeStart, &sourceTimeEnd);	
	if (gthr != S_OK && gthr != VFW_S_NO_STOP_TIME )
	{
	  //No timestamp available, so just tag the samples with the current stream time + offset into future
	  sourceTimeStart = _I64_MAX;
    if (m_pClock)
    {
      m_pClock->GetTime(&sourceTimeStart);
      sourceTimeStart -= m_tStart;
      sourceTimeStart += (500*10000); //add 500ms
    }        
	}
  //LogDebug("CCcFilter: SampleGetTime: %f", (float)sourceTimeStart/10000000.0);

	m_rgCCData.SetCount(0);
	if( !m_pPassThroughQueue )
	{
		m_proc.ProcessData( cbData, pSourceData, NULL, &m_rgCCData, m_bIsSubtypeAVC1, m_dwFlags, sourceTimeStart );
	}
	else
	{
		auto_pif<IMediaSample> pifOutSample = 
			UsingDifferentAllocators() ? Copy( pSourceSample ) : pSourceSample;

		if( !pifOutSample )
			return E_UNEXPECTED;

		BYTE* pToTransform;
		RETURN_FAILED( pifOutSample->GetPointer( &pToTransform ));

		m_proc.ProcessData( cbData, pSourceData, pToTransform, &m_rgCCData, m_bIsSubtypeAVC1, m_dwFlags, sourceTimeStart );

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

			  pifOutSample->SetTime(NULL,NULL); //Remove timestamps
								
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
	LOG_DETAIL("CCcFilter: Stop()");

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
	LOG_DETAIL("CCcFilter: Pause()");

  CreateQueue( &m_pPassThroughQueue, m_outpinPassThrough.GetConnected());
  CreateQueue( &m_pLine21Queue,	   m_outpinLine21.GetConnected());

	CBaseFilter::Pause();  // Note: great grandparent
   ASSERT( m_State == State_Paused );
//	m_pifIPSample = NULL;

    return NOERROR;
}

HRESULT CCcFilter::EndOfStream()
{
	LOG_DETAIL("CCcFilter: EndOfStream()");

	if( m_pPassThroughQueue )
		m_pPassThroughQueue->EOS();

	if( m_pLine21Queue )
		m_pLine21Queue->EOS();


	return S_OK;
}

HRESULT CCcFilter::BeginFlush()
{
	LOG_DETAIL("CCcFilter: BeginFlush()");

	if( m_pPassThroughQueue )
		m_pPassThroughQueue->BeginFlush();

	if( m_pLine21Queue )
		m_pLine21Queue->BeginFlush();

	return S_OK;
}

HRESULT CCcFilter::EndFlush()
{
	LOG_DETAIL("CCcFilter: EndFlush()");

	if( m_pPassThroughQueue )
		m_pPassThroughQueue->EndFlush();

	if( m_pLine21Queue )
		m_pLine21Queue->EndFlush();
		

	return S_OK;
}

HRESULT CCcFilter::NewSegment( REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
	LOG_DETAIL("CCcFilter: NewSegment()");

	if( m_pPassThroughQueue )
		m_pPassThroughQueue->NewSegment( tStart, tStop, dRate );

	if( m_pLine21Queue )
		m_pLine21Queue->NewSegment( tStart, tStop, dRate );

	m_proc.Reset();


	return S_OK;
}


