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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "TsWriter.h"
#include "tsheader.h"

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
	&MEDIATYPE_Stream,							// Major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT   // Minor type
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
    1,                          // Number of types
    &sudPinTypes                // Pin information
};

const AMOVIESETUP_FILTER sudDump =
{
    &CLSID_MpTsFilter,          // Filter CLSID
    L"MediaPortal Ts Writer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};

void DumpTs(byte* tspacket)
{
	FILE* fp=fopen("dump.ts", "ab+");
	fwrite(tspacket,1,188,fp);
	fclose(fp);
}
void LogDebug(const char *fmt, ...) 
{
#ifndef DEBUG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("TsWriter.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
#endif
};

//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
    L"MediaPortal Ts Writer", &CLSID_MpTsFilter, CMpTs::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


// Constructor

CMpTsFilter::CMpTsFilter(CMpTs *pDump,LPUNKNOWN pUnk,CCritSec *pLock,HRESULT *phr) :
    CBaseFilter(NAME("TsWriter"), pUnk, pLock, CLSID_MpTsFilter),
    m_pWriterFilter(pDump)
{
}


//
// GetPin
//
CBasePin * CMpTsFilter::GetPin(int n)
{
  if (n == 0) 
	{
      return m_pWriterFilter->m_pPin;
  } 
	else 
	{
      return NULL;
  }
}


//
// GetPinCount
//
int CMpTsFilter::GetPinCount()
{
    return 1;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CMpTsFilter::Stop()
{
  CAutoLock cObjectLock(m_pLock);
	LogDebug("CMpTsFilter::Stop()");
  return CBaseFilter::Stop();
}


//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CMpTsFilter::Pause()
{
	LogDebug("CMpTsFilter::Pause()");
  CAutoLock cObjectLock(m_pLock);

  if (m_pWriterFilter)
  {
      // GraphEdit calls Pause() before calling Stop() for this filter.
      // If we have encountered a write error (such as disk full),
      // then stopping the graph could cause our log to be deleted
      // (because the current log file handle would be invalid).
      // 
      // To preserve the log, don't open/create the log file on pause
      // if we have previously encountered an error.  The write error
      // flag gets cleared when setting a new log file name or
      // when restarting the graph with Run().
  }

  return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CMpTsFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CMpTsFilter::Run()");
  CAutoLock cObjectLock(m_pLock);


  return CBaseFilter::Run(tStart);
}


//
//  Definition of CMpTsFilterPin
//
CMpTsFilterPin::CMpTsFilterPin(CMpTs *pDump,LPUNKNOWN pUnk,CBaseFilter *pFilter,CCritSec *pLock,CCritSec *pReceiveLock,HRESULT *phr) 
:CRenderedInputPin(NAME("CMpTsFilterPin"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"Input"),                 // Pin name
    m_pReceiveLock(pReceiveLock),
    m_pWriterFilter(pDump)
{
	LogDebug("CMpTsFilterPin:ctor");
}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CMpTsFilterPin::CheckMediaType(const CMediaType *)
{
    return S_OK;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CMpTsFilterPin::BreakConnect()
{
    return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CMpTsFilterPin::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMpTsFilterPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("receive sample=null");
			return S_OK;
		}
		
//		CheckPointer(pSample,E_POINTER);
//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{
			LogDebug("receive samplelen:%d",sampleLen);
			return S_OK;
		}
		
		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("receive cannot get samplepointer");
			return S_OK;
		}
		OnRawData(pbData, sampleLen);
	}
	catch(...)
	{
		LogDebug("receive exception");
	}
  return S_OK;
}

void CMpTsFilterPin::OnTsPacket(byte* tsPacket)
{
	m_pWriterFilter->AnalyzeTsPacket(tsPacket);
}

STDMETHODIMP CMpTsFilterPin::EndOfStream(void)
{
    CAutoLock lock(m_pReceiveLock);
    return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CMpTsFilterPin::Reset()
{
		LogDebug("Reset()...");
}

//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CMpTsFilterPin::NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate)
{
    return S_OK;
} // NewSegment


//
//  CMpTs class
//
CMpTs::CMpTs(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown(NAME("CMpTs"), pUnk),m_pFilter(NULL),m_pPin(NULL)
{
		LogDebug("CMpTs::ctor()");
		DeleteFile("TsWriter.log");

    m_pFilter = new CMpTsFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL) 
		{
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

    m_pPin = new CMpTsFilterPin(this,GetOwner(),m_pFilter,&m_Lock,&m_ReceiveLock,phr);
    if (m_pPin == NULL) 
		{
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

		m_pVideoAnalyzer = new CVideoAnalyzer(GetOwner(),phr);
		m_pChannelScanner= new CChannelScan(GetOwner(),phr);
		m_pEpgScanner = new CEpgScanner(GetOwner(),phr);
		m_pPmtGrabber = new CPmtGrabber(GetOwner(),phr);
		m_pRecorder = new CRecorder(GetOwner(),phr);
		m_pTimeShifting= new CTimeShifting(GetOwner(),phr);
		m_pTeletextGrabber= new CTeletextGrabber(GetOwner(),phr);
}




// Destructor

CMpTs::~CMpTs()
{
    delete m_pPin;
    delete m_pFilter;
		delete m_pVideoAnalyzer;
		delete m_pChannelScanner;
		delete m_pEpgScanner;
		delete m_pPmtGrabber;
		delete m_pRecorder;
		delete m_pTimeShifting;
		delete m_pTeletextGrabber;
}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CMpTs::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    CMpTs *pNewObject = new CMpTs(punk, phr);
    if (pNewObject == NULL) 
		{
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return pNewObject;

} // CreateInstance


//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CMpTs::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv,E_POINTER);
    CAutoLock lock(&m_Lock);

    // Do we have this interface
	if (riid == IID_ITSVideoAnalyzer)
	{
		return GetInterface((ITsVideoAnalyzer*)m_pVideoAnalyzer, ppv);
	}
	else if (riid == IID_ITSChannelScan)
	{
		return GetInterface((ITSChannelScan*)m_pChannelScanner, ppv);
	}
	else if (riid == IID_ITsEpgScanner)
	{
		return GetInterface((ITsEpgScanner*)m_pEpgScanner, ppv);
	}
	else if (riid == IID_IPmtGrabber)
	{
		return GetInterface((IPmtGrabber*)m_pPmtGrabber, ppv);
	}
	else if (riid == IID_ITsRecorder)
	{
		return GetInterface((ITsRecorder*)m_pRecorder, ppv);
	}
	else if (riid == IID_ITsTimeshifting)
	{
		return GetInterface((ITsTimeshifting*)m_pTimeShifting, ppv);
	}
	else if (riid == IID_ITeletextGrabber)
	{
		return GetInterface((ITeletextGrabber*)m_pTeletextGrabber, ppv);
	}
  else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) 
	{
      return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
  } 

  return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

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

BOOL APIENTRY DllMain(HANDLE hModule, DWORD  dwReason, LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}


void CMpTs::AnalyzeTsPacket(byte* tsPacket)
{
	/*
	CTsHeader header(tsPacket);
	if (header.Pid==0||header.Pid==0x10||header.Pid==0x11||header.Pid==0x12||header.Pid==0xd2||header.Pid==0xd3)
	{
		DumpTs(tsPacket);
	}*/
	try
	{
		m_pVideoAnalyzer->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in video analyzer");
	}

	try
	{
		m_pChannelScanner->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in channel scanner");
	}

	try
	{
		m_pEpgScanner->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in epg scanner");
	}
	
	try
	{
		m_pPmtGrabber->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in pmt grabber");
	}
	try
	{
		m_pRecorder->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in recorder");
	}
	try
	{
		m_pTimeShifting->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in timeshifter");
	}
	try
	{
		m_pTeletextGrabber->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in teletext grabber");
	}
	
	
}