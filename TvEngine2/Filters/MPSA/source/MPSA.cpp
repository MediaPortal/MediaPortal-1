/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: Agree
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

#pragma warning(disable: 4786)
#include <windows.h>
#include <commdlg.h>
#include <streams.h>
#include <initguid.h>
#include "Section.h"
#include "MPSA.h"
#include "SplitterSetup.h"
#include "proppage.h"
#include "mhwinputpin1.h"
#include "mhwinputpin2.h"
#include "epginputpin.h"
#include "atscparser.h"
// Setup data

extern void Log(const char *fmt, ...) ;
extern void Dump(const char *fmt, ...) ;
const AMOVIESETUP_MEDIATYPE sudPinTypes[2] =
{
	{&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_DVB_SI},         // Minor type
	{&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_ATSC_SI}         // Minor type
};
const AMOVIESETUP_MEDIATYPE sudPinTypesMHW =
{
	&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_DVB_SI,         // Minor type
};

const AMOVIESETUP_MEDIATYPE sudPinTypesEPG =
{
	&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_DVB_SI 
};

const AMOVIESETUP_PIN sudPins[4] =
{
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
	},
		
	{
		L"MHWd2",                   // Pin string name
		FALSE,                      // Is it rendered
		FALSE,                      // Is it an output
		FALSE,                      // Allowed none
		FALSE,                      // Likewise many
		&CLSID_NULL,                // Connects to filter
		L"Output",                  // Connects to pin
		1,                          // Number of types
		&sudPinTypesMHW             // Pin information
	},
		
	{
		L"MHWd3",                   // Pin string name
		FALSE,                      // Is it rendered
		FALSE,                      // Is it an output
		FALSE,                      // Allowed none
		FALSE,                      // Likewise many
		&CLSID_NULL,                // Connects to filter
		L"Output",                  // Connects to pin
		1,                          // Number of types
		&sudPinTypesMHW             // Pin information
	},
			
	{
		L"EPG",                   // Pin string name
		FALSE,                      // Is it rendered
		FALSE,                      // Is it an output
		FALSE,                      // Allowed none
		FALSE,                      // Likewise many
		&CLSID_NULL,                // Connects to filter
		L"EPG",                  // Connects to pin
		1,                          // Number of types
		&sudPinTypesEPG             // Pin information
	},
};

const AMOVIESETUP_FILTER sudDump =
{
    &CLSID_MPDSA,          // Filter CLSID
    L"MediaPortal Stream Analyzer",   // String name
		MERIT_DO_NOT_USE,           // Filter merit
    4,                          // Number pins
    sudPins                    // Pin details
};


//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
	{L"MediaPortal Stream Analyzer", &CLSID_MPDSA, CStreamAnalyzer::CreateInstance, NULL, &sudDump},
	{ L"MP StreamAnalyzer PropPage",&CLSID_StreamAnalyzerPropPage, MPDSTProperties::CreateInstance }};
int g_cTemplates = 2;


// Constructor

CStreamAnalyzerFilter::CStreamAnalyzerFilter(CStreamAnalyzer *pDump,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("Stream Analyzer MP"), pUnk, pLock, CLSID_MPDSA),
    m_pDump(pDump)
{
}

STDMETHODIMP CStreamAnalyzer::GetPages(CAUUID *pPages) 
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
CBasePin * CStreamAnalyzerFilter::GetPin(int n)
{
    if (n ==0) 
	    return m_pDump->m_pPin;
    else if (n ==1) 
		return m_pDump->m_pMHWPin1;
    else if (n ==2) 
		return m_pDump->m_pMHWPin2;
	else if (n ==3)
		return m_pDump->m_pEPGPin;
	else {
        return NULL;
    }
}


//
// GetPinCount
//
int CStreamAnalyzerFilter::GetPinCount()
{
    return 4;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CStreamAnalyzerFilter::Stop()
{
    CAutoLock cObjectLock(m_pLock);
	
    return CBaseFilter::Stop();
}

//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CStreamAnalyzerFilter::Pause()
{
    CAutoLock cObjectLock(m_pLock);


    return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CStreamAnalyzerFilter::Run(REFERENCE_TIME tStart)
{
    CAutoLock cObjectLock(m_pLock);

	m_pDump->ResetParser();
	return CBaseFilter::Run(tStart);
}


//
//  Definition of CStreamAnalyzerSectionsPin
//
CStreamAnalyzerSectionsPin::CStreamAnalyzerSectionsPin(CStreamAnalyzer *pDump,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CStreamAnalyzerSectionsPin"),
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
HRESULT CStreamAnalyzerSectionsPin::CheckMediaType(const CMediaType *pmt)
{
	if(pmt->majortype==MEDIATYPE_MPEG2_SECTIONS )
		return S_OK;
	return S_FALSE;
}

//
// BreakConnect
//
// Break a connection
//
HRESULT CStreamAnalyzerSectionsPin::BreakConnect()
{
//	Log("mpsa::Section:BreakConnect");
    return CRenderedInputPin::BreakConnect();
}

HRESULT CStreamAnalyzerSectionsPin::CompleteConnect(IPin *pPin)
{
	HRESULT hr=CBasePin::CompleteConnect(pPin);
	return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CStreamAnalyzerSectionsPin::ReceiveCanBlock()
{
    return S_OK;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CStreamAnalyzerSectionsPin::Receive(IMediaSample *pSample)
{
	
    CheckPointer(pSample,E_POINTER);

    //CAutoLock lock(m_pReceiveLock);
    PBYTE pbData=NULL;

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

    return S_OK;
}
void CStreamAnalyzerSectionsPin::ResetPids()
{
	m_pDump->ResetPids();
}

//
// EndOfStream
//
STDMETHODIMP CStreamAnalyzerSectionsPin::EndOfStream(void)
{
    CAutoLock lock(m_pReceiveLock);
//	Log("mpsa::Sections:end of stream");
    return CRenderedInputPin::EndOfStream();

} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CStreamAnalyzerSectionsPin::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
    m_tLast = 0;
    return S_OK;

} // NewSegment


//
//  CStreamAnalyzer class
//
CStreamAnalyzer::CStreamAnalyzer(LPUNKNOWN pUnk, HRESULT *phr) :
    CUnknown(NAME("CStreamAnalyzer"), pUnk),
    m_pFilter(NULL),
    m_pPin(NULL),m_pSections(NULL),m_pDemuxer(NULL),
	m_patChannelsCount(0),m_pmtGrabProgNum(0),
	m_currentPMTLen(0)
{
	m_pPMTCallback=NULL;
	m_bScanning=TRUE;
	//::DeleteFile("mpsa.log");
	Log("----mpsa::Initialize MPSA v1.10 ----");

	m_pCallback=NULL;
	m_bDecodeATSC=false;
	m_bReset=true;
			m_pmtGrabProgNum=0;
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

    m_pFilter = new CStreamAnalyzerFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

    m_pPin = new CStreamAnalyzerSectionsPin(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pPin == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

	m_pMHWPin1 = new CMHWInputPin1(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pMHWPin1 == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

	m_pMHWPin2 = new CMHWInputPin2(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pMHWPin2 == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }
	m_pEPGPin = new CEPGInputPin(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pEPGPin == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }
	m_pAtscParser = new ATSCParser(m_pPin);
	m_pAtscParser ->SetDemuxer(m_pDemuxer);
}



// Destructor

CStreamAnalyzer::~CStreamAnalyzer()
{
//	Log("mpsa::~CStreamAnalyzer()");
	delete m_pDemuxer;
	delete m_pSections;
	delete m_pPin;
	delete m_pMHWPin1;
	delete m_pMHWPin2;
	delete m_pEPGPin;
	delete m_pFilter;
	delete m_pAtscParser ;

}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CStreamAnalyzer::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    CStreamAnalyzer *pNewObject = new CStreamAnalyzer(punk, phr);
    if (pNewObject == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }
    return pNewObject;

} // CreateInstance

STDMETHODIMP CStreamAnalyzer::ResetParser()
{
	try
	{
		Log("mpsa::ResetParser");
		MapSectionPids();
		m_pPin->ResetPids();
		m_pMHWPin1->ResetPids();
		m_pMHWPin2->ResetPids();
		m_pEPGPin->ResetPids();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in ResetParser()");
	}	

	return S_OK;
}
//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CStreamAnalyzer::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv,E_POINTER);
    CAutoLock lock(&m_Lock);
    // Do we have this interface
	if (riid == IID_IStreamAnalyzer)
	{
		Log("mpsa::get IID_IStreamAnalyzer");
		return GetInterface((IStreamAnalyzer*)this, ppv);
	}
	else if (riid == IID_IEPGGrabber)
	{
		Log("mpsa::get IID_IEPGGrabber %x", (*ppv));
		HRESULT hr= GetInterface((IEPGGrabber*)this, ppv);
		Log("mpsa::result:%x %x", hr, (*ppv));
		return hr;
	}
	else if (riid == IID_IMHWGrabber)
	{
		Log("mpsa::get IID_IMHWGrabber");
		return GetInterface((IMHWGrabber*)this, ppv);
	}
	else if (riid == IID_ATSCGrabber)
	{
		Log("mpsa::get IID_ATSCGrabber");
		return GetInterface((IATSCGrabber*)this, ppv);
	}
    else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist)
	{
        return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
    } 
	else if (riid == IID_ISpecifyPropertyPages) 
	{
        return GetInterface((ISpecifyPropertyPages *) this, ppv);
	}
    return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

	
STDMETHODIMP CStreamAnalyzer::ResetPids()
{
	Log("mpsa::resetpids");
	m_bReset=true;
	m_pmtGrabProgNum=0;
	memset(m_pmtGrabData,0,4096);
  return NOERROR;
}
HRESULT CStreamAnalyzer::Process(BYTE *pbData,long len)
{
	if (pbData==NULL) return S_OK;
	if (len <2) return S_OK;
	//Log("mpsa:process");
	try
	{
		if (m_bReset)
		{
			Log("mpsa::process() reset");
			m_bReset=false;
			m_patChannelsCount=0;
			m_pSections->ResetPAT();
			if (m_bDecodeATSC)
			{
				m_pAtscParser->Reset();
			}
		}		
		if (m_bScanning==FALSE) return S_OK;
		//CAutoLock lock(&m_Lock);
/*
		if(pbData[0]==0x00 && pbData[1]==0x00 && pbData[2]==0x01)
		{
			//pes packet
			AudioHeader audio;
			BYTE *d=new BYTE[len];
			m_pSections->GetPES(pbData,len,d);
			if(m_pSections->ParseAudioHeader(d,&audio)==S_OK)
			{
				// we can check audio
				int a=0;
			}
			delete [] d;
			return S_OK;
		}
*/
		
		if (m_bDecodeATSC)
		{
			if (pbData[0]==0xc7)
			{
				try
				{
					m_pAtscParser->ATSCDecodeMasterGuideTable(pbData,len,&m_patChannelsCount);
				}
				catch(...)
				{
					Dump("mpsaa: unhandled exception while decoding ATSC guide table");
				}
			}
			if (pbData[0]==0xc8 || pbData[0]==0xc9)
			{
				try
				{
					if (m_patChannelsCount==0)
					{
						//decode ATSC: Virtual Channel Table (pid 0xc8 / 0xc9)
						m_pAtscParser->ATSCDecodeChannelTable(pbData,m_patTable, &m_patChannelsCount,len);
					}

				}
				catch(...)
				{
					Dump("mpsaa: unhandled exception while decoding ATSC channel table");
				}
			}

			//decode ATSC: EPG
			try
			{
				if (m_patChannelsCount>0)
					m_pAtscParser->ATSCDecodeEPG(pbData,len);
			}
			catch(...)
			{
				Dump("mpsaa: unhandled exception while decoding ATSC epg");
			}
			return S_OK;
		}		
			
		if(pbData[0]==0x02)// pmt
		{
			ULONG prgNumber=(pbData[3]<<8)+pbData[4];
			Log("rcv pmt prog:%x %x", prgNumber,m_pmtGrabProgNum);
			for(int n=0;n<m_patChannelsCount && n < 512;n++)
			{
				try
				{
					if(m_patTable[n].ProgrammNumber==prgNumber )
					{
						if (m_patTable[n].PMTReady==0)
						{
							m_pSections->decodePMT(pbData,&m_patTable[n],len);
							Log("mpsa::decode PMT program number:0x%x 0x%x",prgNumber,m_patTable[n].ProgrammNumber);
							//Log("mpsa::PMT decoded");
							//if(m_patTable[n].Pids.AudioPid1>0)
							//	m_pDemuxer->MapAdditionalPayloadPID(m_patTable[n].Pids.AudioPid1);
							//if(m_patTable[n].Pids.AudioPid2>0)
							//	m_pDemuxer->MapAdditionalPayloadPID(m_patTable[n].Pids.AudioPid2);
							//if(m_patTable[n].Pids.AudioPid3>0)
							//	m_pDemuxer->MapAdditionalPayloadPID(m_patTable[n].Pids.AudioPid3);
						}
					}
				
				}	
				catch(...)
				{
					Dump("mpsa: unhandled exception while decoding PMT");
				}
			}
			
			if(m_pmtGrabProgNum>0 && m_pmtGrabProgNum==prgNumber && len<=4096)
			{
				if (memcmp(m_pmtGrabData,pbData,len)!=0)
				{
					memset(m_pmtGrabData,0,4096);
					memcpy(m_pmtGrabData,pbData,len);// save the pmt in the buffer
					m_currentPMTLen=len;
					Log("mpsa::Received new/modified PMT for program:%d",m_pmtGrabProgNum);
					m_pFilter->NotifyEvent(EC_PROGRAM_CHANGED,0,(LONG_PTR)(IBaseFilter*)m_pFilter);
					if (m_pPMTCallback!=NULL)
					{
						Log("mpsa::notify pmt received");
						m_pPMTCallback->OnPMTReceived();
					}
				}
			}
		}

		
		if(pbData[0]==0x00)// pat
		{
		//	Log("rcv:PAT");
			// we need to check if we received a new PAT
			// reason, after submitting a tune request (zap to another channel) we might still
			// receive the old PAT for a couple of msec until the tuner has
			// finished tuning to the new channel
			if (m_pSections->IsNewPat(pbData,len))
			{
				/*
				if (m_patChannelsCount>0) 
				{
					Log("mpsa::Found new PAT");
					MapSectionPids();//unmap any section pids and map default pids for pat (0x0,0x10,0x11)
					m_pmtGrabProgNum=0;
					m_patChannelsCount=0;
					m_pSections->ResetPAT();//reset PAT...
					if (m_bDecodeATSC)
					{
						m_pAtscParser->Reset();
					}
				}*/
				//Log("mpsa::decode pat");
				try
				{
					m_pSections->decodePAT(pbData,m_patTable,&m_patChannelsCount,len);
					Log("mpsa::PAT decoded and found %d channels, map pids", m_patChannelsCount);
					int pids[512];
					for(int n=0;n<m_patChannelsCount && n < 512;n++)
					{
						pids[n]=m_patTable[n].ProgrammPMTPID;
					}
					MapPMTPids(m_patChannelsCount, pids);
					bool grabMHW=m_pMHWPin1->isGrabbing() || m_pMHWPin2->isGrabbing();
					bool grabEPG=m_pEPGPin->isGrabbing();						
					Log("mpsa::PAT decoded and pids mapped (epg:%d mhw:%d)", grabEPG,grabMHW);
					m_pFilter->NotifyEvent(EC_PROGRAM_CHANGED,0,(LONG_PTR)(IBaseFilter*)m_pFilter);
				}
				catch(...)
				{
					Dump("mpsa: unhandled exception while decoding PAT");
				}
			}
		}
		if(pbData[0]==0x42)// sdt
		{
			try
			{
				//Log("mpsa::decode SDT");
				if (m_patChannelsCount>0)
				{
					m_pSections->decodeSDT(pbData,m_patTable,m_patChannelsCount,len);
				}
			}
			catch(...)
			{
				Dump("mpsa: unhandled exception while decoding SDT");
			}
			//Log("mpsa::SDT decoded");
		}
		if (pbData[0]==0x40) //NIT
		{
		//	Log("mpsa::rcv decode NIT");
			try
			{
				if (m_patChannelsCount>0)
				{
					//m_pSections->decodeNITTable(pbData,m_patTable,m_patChannelsCount);
				}
			}
			catch(...)
			{
				Dump("mpsa: unhandled exception while decoding NIT");
			}
		}
	}
	catch(...)
	{
		Dump("mpsa:--- PROCESS UNHANDLED EXCEPTION ---");
	}
	return S_OK;
}


STDMETHODIMP CStreamAnalyzer::GetLCN(WORD channel, WORD* networkId, WORD* transportId, WORD* serviceID, WORD* LCN)
{
	try
	{
		m_pSections->GetLCN(channel,networkId, transportId, serviceID, LCN);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetLCN()");
	}	
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::SetPidFilterCallback(IHardwarePidFiltering* callback)
{
	Log("mpsa:set callback");
	m_pCallback=callback;
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::SetPMTCallback(IPMTCallback* callback)
{
	Log("mpsa:set pmt callback :%x",callback);
	m_pPMTCallback=callback;
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::Scanning(BOOL yesNo)
{
	m_bScanning=yesNo;
	if (m_bScanning)
		Log("mpsa:set Scanning :on");
	else
		
		Log("mpsa:set Scanning :off");
	return S_OK;
}

STDMETHODIMP CStreamAnalyzer::IsChannelReady(ULONG channel)
{
	try
	{
		Log("mpsa::IsChannelReady:%d channels:%d", channel,m_patChannelsCount);
		if(channel<0 || channel >= (ULONG)m_patChannelsCount)
		{
			Log("mpsa::IsChannelReady:%d channels:%d failed, invalid channel", channel,m_patChannelsCount);
			return S_FALSE;
		}
		if(m_patTable[channel].SDTReady!=0 && m_patTable[channel].PMTReady!=0)
		{
			Log("mpsa::IsChannelReady:%d returns true",channel);
			return S_OK;
		}
		
		Log("mpsa::IsChannelReady:%d returns false SDT:%d PMT:%d", channel, m_patTable[channel].SDTReady,m_patTable[channel].PMTReady);

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in IsChannelReady()");
	}	
	return S_FALSE;
}
STDMETHODIMP CStreamAnalyzer::get_IPin (IPin **ppPin)
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
STDMETHODIMP CStreamAnalyzer::put_MediaType(CMediaType *pmt)
{
     return NOERROR;
} // put_MediaType


//
// get_MediaType
//
// INull method.
// Set *pmt to the current preferred media type.
//
STDMETHODIMP CStreamAnalyzer::get_MediaType(CMediaType **pmt)
{
    return NOERROR;
} // get_MediaType


//
// get_State
//
// INull method
// Set *state to the current state of the filter (State_Stopped etc)
//
STDMETHODIMP CStreamAnalyzer::get_State(FILTER_STATE *pState)
{
    return NOERROR;

} // get_State

STDMETHODIMP CStreamAnalyzer::GetChannelCount(WORD *count)
{
	try
	{
		if (m_bReset) 
			*count=0;
		else
			*count=m_patChannelsCount;
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetChannelCount()");
	}	
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::SetPMTProgramNumber(ULONG prgNum)
{

	Log("mpsa:set program id:%d %d", prgNum,m_pmtGrabProgNum);
	if (m_pmtGrabProgNum!=prgNum)
	{
	}
	m_pmtGrabProgNum=prgNum;
	return S_OK;
}

STDMETHODIMP CStreamAnalyzer::UseATSC(BOOL yesNo)
{
	try
	{
		m_bDecodeATSC=yesNo;
		m_pDemuxer->UseATSC(yesNo);
		if (m_bDecodeATSC)
			Log("mpsa::use ATSC:yes");
		else
			Log("mpsa::use ATSC:no");
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in UseATSC()");
	}	
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::IsATSCUsed(BOOL* yesNo)
{
	try
	{
		*yesNo=m_bDecodeATSC;
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in IsATSCUsed()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GrabEPG()
{
	try
	{
		Log("mpsa::StreamAnalyzer:GrabEPG");
		m_pEPGPin->GrabEPG();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GrabEPG()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::IsEPGReady(BOOL* yesNo)
{
	try
	{
		*yesNo=m_pEPGPin->IsEPGReady();
		if (*yesNo)
		{
			Log("CStreamAnalyzer:IsEPGReady() ->yes");
		}
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in IsEPGReady()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetEPGChannelCount( ULONG* channelCount)
{
	try
	{
		*channelCount=m_pEPGPin->GetEPGChannelCount( );
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetEPGChannelCount()");
	}

	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetEPGEventCount( ULONG channel,  ULONG* eventCount)
{
	try
	{
		*eventCount=m_pEPGPin->GetEPGEventCount( channel);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetEPGEventCount()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	try
	{
		m_pEPGPin->GetEPGChannel( channel,  networkId,  transportid, service_id  );
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetEPGChannel()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    )
{
	try
	{
		m_pEPGPin->GetEPGEvent( channel,  eventid, language,dateMJD, timeUTC, duration, genre    );
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetEPGEvent()");
	}

	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetEPGLanguage(THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )
{
	try
	{
		m_pEPGPin->GetEPGLanguage( channel,  eventid, languageIndex,language,eventText,eventDescription    );
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetEPGLanguage()");
	}
	return S_OK;
}

STDMETHODIMP CStreamAnalyzer::GrabMHW()
{
	try
	{
		Log("MPSA:GrabMhw()");
		m_pMHWPin1->GrabMHW();
		m_pMHWPin2->GrabMHW();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GrabMHW()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::IsMHWReady(BOOL* yesNo)
{
	try
	{
		*yesNo=FALSE;
		if (m_pMHWPin1->IsReady() && m_pMHWPin2->IsReady() )
		{
			*yesNo=TRUE;
			return S_OK;
		}
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in IsMHWReady()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetMHWTitleCount(WORD* count)
{
	try
	{
		*count=m_pMHWPin1->m_MHWParser.GetTitleCount();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetMHWTitleCount()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetMHWTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
{	
	try
	{
		m_pMHWPin1->m_MHWParser.GetTitle(program, id, transportId, networkId, channelId, programId, themeId, PPV, Summaries, duration, dateStart,timeStart,title,programName);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetMHWTitle()");
	}

	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetMHWChannel(WORD channelNr, WORD* channelId,WORD* networkId, WORD* transportId, char** channelName)
{
	try
	{
		m_pMHWPin2->m_MHWParser.GetChannel(channelNr,channelId, networkId, transportId, channelName);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetMHWChannel()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetMHWSummary(WORD programId, char** summary)
{
	try
	{
		m_pMHWPin2->m_MHWParser.GetSummary(programId, summary);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetSummary()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetMHWTheme(WORD themeId, char** theme)
{
	try
	{
		m_pMHWPin2->m_MHWParser.GetTheme(themeId, theme);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetMHWTheme()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GrabATSC()
{
	try
	{
		m_pAtscParser->Reset();

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GrabATSC()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::IsATSCReady (BOOL* yesNo)
{
	try
	{
		*yesNo = m_pAtscParser->IsReady();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in IsATSCReady()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetATSCTitleCount(WORD* count)
{
	try
	{
		*count=m_pAtscParser->GetEPGCount();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetATSCTitleCount()");
	}
	return S_OK;
}
STDMETHODIMP CStreamAnalyzer::GetATSCTitle(WORD no, WORD* source_id, ULONG* starttime, WORD* length_in_secs, char** title, char** description)
{
	try
	{
		m_pAtscParser->GetEPGTitle(no, source_id, starttime, length_in_secs, title, description);
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetATSCTitle()");
	}
	return S_OK;
}


STDMETHODIMP CStreamAnalyzer::GetPMTData(BYTE *data)
{
	BYTE *buf=m_pmtGrabData;
	if(m_currentPMTLen>0 && m_pmtGrabProgNum>0)
	{
		memcpy(data,buf,m_currentPMTLen);
		return m_currentPMTLen;
	}
	return -1;
}

STDMETHODIMP CStreamAnalyzer::GetChannel(WORD channel,BYTE *ch)
{
	try
	{
		Log("mpsa::GetChannel(%d) channels found:%d size:%d", channel, m_patChannelsCount,m_pSections->CISize());

		if(channel>=0 && channel < m_patChannelsCount && m_patChannelsCount>0)
		{
			memcpy(ch,&m_patTable[channel],m_pSections->CISize());
			if(m_patTable[channel].SDTReady==0 || m_patTable[channel].PMTReady==0)
			{
				Log("mpsa::GetChannel(%d) not ready",channel);
				if (m_patTable[channel].SDTReady==0)
					Log("mpsa::GetChannel(%d) SDT not found");
				if (m_patTable[channel].PMTReady==0)
					Log("mpsa::GetChannel(%d) PMT not found");
				return S_FALSE;
			}
		
			Log("mpsa::GetChannel(%d) found ok",channel);
			return S_OK;
		}
		Log("mpsa::GetChannel(%d) invalid channel");
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetChannel()");
	}
	return S_FALSE;
}
STDMETHODIMP CStreamAnalyzer::GetCISize(WORD *size)
{
	try
	{
		*size=m_pSections->CISize();
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in GetCISize()");
	}
	return S_OK;
}
HRESULT CStreamAnalyzer::NotifyFinished(int EVENT)
{
	//Log("NotifyEvent %d", EVENT);
	m_pFilter->NotifyEvent(EVENT,0,(LONG_PTR)(IBaseFilter*)m_pFilter);	

	return S_OK;
}

HRESULT CStreamAnalyzer::MapSectionPids()
{
	try
	{
		Log("mpsa:set MapSectionPids()");
		/*
		int pids[16];
		pids[0]=0;//PAT
		pids[1]=0x10;//NIT
		pids[2]=0x11;//SDT
		if (m_pCallback !=NULL)
		{
			Log("mpsa callback");
			m_pCallback->FilterPids(3,pids);
			Log("mpsa callback done");
		}*/
		//HRESULT hr=m_pDemuxer->SetSectionMapping(m_pPin);

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in MapSectionPids()");
	}
	return S_OK;
}
HRESULT CStreamAnalyzer::MapPMTPids(int count, int* pids)
{
	try
	{
		Log("mpsa:MapPMTPids(%d)",count);
		int hwPids[520];
		memset(hwPids,0,sizeof(hwPids));
		int offset=0;
		if (m_bDecodeATSC)
		{
			hwPids[0]=0x1ffb;
			offset=1;
		}
		else
		{
			hwPids[0]=0;//PAT
			hwPids[1]=0x10;//NIT
			hwPids[2]=0x11;//SDT
			offset=3;
		}
		for (int i=0; i < count; ++i)
		{	
			hwPids[i+offset]=pids[i];
		}

		if (m_pCallback !=NULL)
		{
			m_pCallback->FilterPids(offset+count,hwPids);
		}

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in MapPMTPids()");
	}
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

