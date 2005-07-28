#include <windows.h>
#include <commdlg.h>
#include <streams.h>
#include <initguid.h>
#include "Section.h"
#include "MPSA.h"
#include "SplitterSetup.h"
#include "proppage.h"

// Setup data

extern void Log(const char *fmt, ...) ;
const AMOVIESETUP_MEDIATYPE sudPinTypes[2] =
{
	{&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_DVB_SI},         // Minor type
	{&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_ATSC_SI}         // Minor type
};

const AMOVIESETUP_PIN sudPins =
{
    L"Input",                   // Pin string name
    FALSE,                      // Is it rendered
    FALSE,                      // Is it an output
    FALSE,                      // Allowed none
    FALSE,                      // Likewise many
    &CLSID_NULL,                // Connects to filter
    L"Output",                  // Connects to pin
    2,                          // Number of types
    &sudPinTypes[0]             // Pin information
};

const AMOVIESETUP_FILTER sudDump =
{
    &CLSID_MPDSA,          // Filter CLSID
    L"MediaPortal Stream Analyzer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};


//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
	{L"MediaPortal Stream Analyzer", &CLSID_MPDSA, CDump::CreateInstance, NULL, &sudDump},
	{ L"MP StreamAnalyzer PropPage",&CLSID_StreamAnalyzerPropPage, MPDSTProperties::CreateInstance }};
int g_cTemplates = 2;


// Constructor

CDumpFilter::CDumpFilter(CDump *pDump,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("Stream Analyzer MP"), pUnk, pLock, CLSID_MPDSA),
    m_pDump(pDump)
{
}

STDMETHODIMP CDump::GetPages(CAUUID *pPages) 
{
    CheckPointer(pPages,E_POINTER);

    pPages->cElems = 1;
    pPages->pElems = (GUID *) CoTaskMemAlloc(sizeof(GUID));

    if (pPages->pElems == NULL)
        return E_OUTOFMEMORY;

    *(pPages->pElems) = CLSID_StreamAnalyzerPropPage;

    return NOERROR;

} // GetPages

//
// GetPin
//
CBasePin * CDumpFilter::GetPin(int n)
{
    if (n == 0) {
        return m_pDump->m_pPin;
    } else {
        return NULL;
    }
}


//
// GetPinCount
//
int CDumpFilter::GetPinCount()
{
    return 1;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CDumpFilter::Stop()
{
    CAutoLock cObjectLock(m_pLock);
	
    return CBaseFilter::Stop();
}

//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CDumpFilter::Pause()
{
    CAutoLock cObjectLock(m_pLock);

    if (m_pDump)
    {
    }

    return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CDumpFilter::Run(REFERENCE_TIME tStart)
{
    CAutoLock cObjectLock(m_pLock);

	m_pDump->ResetParser();
    return CBaseFilter::Run(tStart);
}


//
//  Definition of CDumpInputPin
//
CDumpInputPin::CDumpInputPin(CDump *pDump,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CDumpInputPin"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"Input"),                 // Pin name
    m_pReceiveLock(pReceiveLock),
    m_pDump(pDump),
    m_tLast(0)
{

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CDumpInputPin::CheckMediaType(const CMediaType *pmt)
{
	if(pmt->majortype==MEDIATYPE_MPEG2_SECTIONS)
		return S_OK;
	return S_FALSE;
}

//
// BreakConnect
//
// Break a connection
//
HRESULT CDumpInputPin::BreakConnect()
{
    return CRenderedInputPin::BreakConnect();
}

HRESULT CDumpInputPin::CompleteConnect(IPin *pPin)
{
	HRESULT hr=CBasePin::CompleteConnect(pPin);
	m_pDump->OnConnect();
	return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CDumpInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CDumpInputPin::Receive(IMediaSample *pSample)
{
    CheckPointer(pSample,E_POINTER);

    CAutoLock lock(m_pReceiveLock);
    PBYTE pbData;

    // Has the filter been stopped yet?

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    m_tLast = tStart;
	long lDataLen=0;

    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) {
        return hr;
    }
	
	lDataLen=pSample->GetActualDataLength();
	// decode
	if(lDataLen>5)
		m_pDump->Process(pbData,lDataLen);

    return NOERROR;
}
void CDumpInputPin::ResetPids()
{
}

//
// EndOfStream
//
STDMETHODIMP CDumpInputPin::EndOfStream(void)
{
    CAutoLock lock(m_pReceiveLock);
    return CRenderedInputPin::EndOfStream();

} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CDumpInputPin::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
    m_tLast = 0;
    return S_OK;

} // NewSegment


//
//  CDump class
//
CDump::CDump(LPUNKNOWN pUnk, HRESULT *phr) :
    CUnknown(NAME("CDump"), pUnk),
    m_pFilter(NULL),
    m_pPin(NULL),m_pSections(NULL),m_pDemuxer(NULL),
	m_patChannelsCount(0),m_pmtGrabProgNum(0),
	m_currentPMTLen(0)
{
	Log("Initialize MPSA");
	m_bDecodeATSC=false;
    ASSERT(phr);
    memset(m_pmtGrabData,0,4096);
	m_pSections=new Sections();
	if(m_pSections==NULL)
	{
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

	m_pDemuxer=new SplitterSetup(m_pSections);
	if(m_pDemuxer==NULL)
	{
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

    m_pFilter = new CDumpFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

    m_pPin = new CDumpInputPin(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pPin == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }


}



// Destructor

CDump::~CDump()
{
	delete m_pDemuxer;
	delete m_pSections;
	delete m_pPin;
	delete m_pFilter;

}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CDump::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    CDump *pNewObject = new CDump(punk, phr);
    if (pNewObject == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }
    return pNewObject;

} // CreateInstance

STDMETHODIMP CDump::ResetParser()
{
	HRESULT hr;
	hr=m_pDemuxer->UnMapAllPIDs();
	m_patChannelsCount=0;
	return hr;
}
//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CDump::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv,E_POINTER);
    CAutoLock lock(&m_Lock);
    // Do we have this interface
	if (riid == IID_IStreamAnalyzer)
	{
		return GetInterface((IStreamAnalyzer*)this, ppv);
	}
    else 
	if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist)
	{
        return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
    } 
	else
	if (riid == IID_ISpecifyPropertyPages) 
	{
        return GetInterface((ISpecifyPropertyPages *) this, ppv);
	}
    return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

HRESULT CDump::OnConnect()
{
	m_pDemuxer->SetDemuxPins(m_pFilter->GetFilterGraph());
	IPin *pin;
	m_pPin->ConnectedTo(&pin);
	if(pin!=NULL)
		m_pDemuxer->SetPin(pin);

	return S_OK;
}
STDMETHODIMP CDump::ResetPids()
{
        return NOERROR;
}
HRESULT CDump::Process(BYTE *pbData,long len)
{
	//CAutoLock lock(&m_Lock);
	bool pesPacket=false;
	if(pbData[0]==0x00 && pbData[1]==0x00 && pbData[2]==0x01)
	{
		Sections::AudioHeader audio;
		pesPacket=true;
		BYTE *d=new BYTE[len];
		m_pSections->GetPES(pbData,len,d);
		if(m_pSections->ParseAudioHeader(d,&audio)==S_OK)
		{
			// we can check audio
			int a=0;
		}
		delete d;
	}
		
	if (pbData[0]==0xc8 || pbData[0]==0xc9)
		Log("got pid:%x",pbData[0]);
	
	if (m_bDecodeATSC)
	{
		if (pbData[0]==0xc8 || pbData[0]==0xc9)
		{
			//decode ATSC: Virtual Channel Table (pid 0xc8 / 0xc9)
			m_pSections->ATSCDecodeChannelTable(pbData,m_patTable, &m_patChannelsCount);
		}
	}
	else
	{
		if(pbData[0]==0x02)// pmt
		{
			ULONG prgNumber=(pbData[3]<<8)+pbData[4];
			for(int n=0;n<m_patChannelsCount;n++)
			{
				if(m_patTable[n].ProgrammNumber==prgNumber && m_patTable[n].PMTReady==false)
				{
					m_pSections->decodePMT(pbData,&m_patTable[n]);
					if(m_patTable[n].Pids.AudioPid1>0)
						m_pDemuxer->MapAdditionalPayloadPID(m_patTable[n].Pids.AudioPid1);
					if(m_patTable[n].Pids.AudioPid2>0)
						m_pDemuxer->MapAdditionalPayloadPID(m_patTable[n].Pids.AudioPid2);
					if(m_patTable[n].Pids.AudioPid3>0)
						m_pDemuxer->MapAdditionalPayloadPID(m_patTable[n].Pids.AudioPid3);
				}
			}
			if(m_pmtGrabProgNum==prgNumber && len<=4096)
			{
				memset(m_pmtGrabData,0,4096);
				memcpy(m_pmtGrabData,pbData,len);// save the pmt in the buffer
				m_currentPMTLen=len;
			}
					
		}
		if(pbData[0]==0x00 && m_patChannelsCount==0 && pesPacket==false)// pat
		{
			m_pDemuxer->UnMapAllPIDs();
			m_pSections->decodePAT(pbData,m_patTable,&m_patChannelsCount);
			for(int n=0;n<m_patChannelsCount;n++)
			{
				m_pDemuxer->MapAdditionalPID(m_patTable[n].ProgrammPMTPID);
			}
		}
		if(pbData[0]==0x42)// sdt
		{
			m_pSections->decodeSDT(pbData,m_patTable,m_patChannelsCount);
		}
	}
	return S_OK;
}
STDMETHODIMP CDump::IsChannelReady(ULONG channel)
{
	if(channel<0 || channel>m_patChannelsCount-1)
		return S_FALSE;

	if(m_patTable[channel].SDTReady==true && m_patTable[channel].PMTReady==true)
		return S_OK;

	return S_FALSE;
}
STDMETHODIMP CDump::get_IPin (IPin **ppPin)
{
    *ppPin = m_pPin->GetConnected ();
	if(*ppPin!=NULL)
		(*ppPin)->AddRef();
    return NOERROR ;
} // get_IPin


//
// put_MediaType
//
// INull method.
//
STDMETHODIMP CDump::put_MediaType(CMediaType *pmt)
{
     return NOERROR;
} // put_MediaType


//
// get_MediaType
//
// INull method.
// Set *pmt to the current preferred media type.
//
STDMETHODIMP CDump::get_MediaType(CMediaType **pmt)
{
    return NOERROR;
} // get_MediaType


//
// get_State
//
// INull method
// Set *state to the current state of the filter (State_Stopped etc)
//
STDMETHODIMP CDump::get_State(FILTER_STATE *pState)
{
    return NOERROR;

} // get_State

STDMETHODIMP CDump::GetChannelCount(WORD *count)
{
	*count=m_patChannelsCount;
	return S_OK;
}
STDMETHODIMP CDump::SetPMTProgramNumber(ULONG prgNum)
{
	m_pmtGrabProgNum=prgNum;
	return S_OK;
}

STDMETHODIMP CDump::UseATSC(BOOL yesNo)
{
	m_bDecodeATSC=yesNo;
	if (m_bDecodeATSC)
		Log("use ATSC:yes");
	else
		Log("use ATSC:no");
	return S_OK;
}

STDMETHODIMP CDump::GetPMTData(BYTE *data)
{
	BYTE *buf=m_pmtGrabData;
	if(m_currentPMTLen>0 && m_pmtGrabProgNum>0)
	{
		memcpy(data,buf,m_currentPMTLen);
		return m_currentPMTLen;
	}
	return -1;
}

STDMETHODIMP CDump::GetChannel(WORD channel,BYTE *ch)
{
	
	if(channel>=0 && channel<=m_patChannelsCount-1 && m_patChannelsCount>0)
	{
		if(m_patTable[channel].SDTReady==false || m_patTable[channel].PMTReady==false)
			return S_FALSE;
		memcpy(ch,&m_patTable[channel],m_pSections->CISize());
		return S_OK;
	}
	return S_FALSE;
}
STDMETHODIMP CDump::GetCISize(WORD *size)
{
	*size=m_pSections->CISize();
	return S_OK;
}
////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration 
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

//
// DllRegisterSever
//
// Handle the registration of this filter
//
STDAPI DllRegisterServer()
{
    return AMovieDllRegisterServer2( TRUE );

} // DllRegisterServer


//
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
    return AMovieDllRegisterServer2( FALSE );

} // DllUnregisterServer


//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

