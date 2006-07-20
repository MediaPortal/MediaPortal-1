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
#include "MPFileWriter.h"

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
    &MEDIATYPE_NULL,            // Major type
    &MEDIASUBTYPE_NULL          // Minor type
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
    &CLSID_MPFileWriter,          // Filter CLSID
    L"MediaPortal File Writer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};

void LogDebug(const char *fmt, ...) 
{
#ifdef DEBUG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("MPFileWriter.log","a+");
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
    L"MediaPortal File Writer", &CLSID_MPFileWriter, CDump::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


// Constructor

CDumpFilter::CDumpFilter(CDump *pDump,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("WSFileWriter"), pUnk, pLock, CLSID_MPFileWriter),
    m_pDump(pDump)
{
}


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
	
	LogDebug("CDumpFilter::Stop()");
	//m_pDump->Log(TEXT("graph Stop() called"),true);

    if (m_pDump)
		m_pDump->StopRecord();
    
    return CBaseFilter::Stop();
}


//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CDumpFilter::Pause()
{
	LogDebug("CDumpFilter::Pause()");
    CAutoLock cObjectLock(m_pLock);

    if (m_pDump)
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
STDMETHODIMP CDumpFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CDumpFilter::Run()");
  CAutoLock cObjectLock(m_pLock);


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
    m_pDump(pDump)
{
	LogDebug("CDumpInputPin:ctor");
	
	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CDumpInputPin::CheckMediaType(const CMediaType *)
{
    return S_OK;
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
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}

		m_pDump->Write(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("receive exception");
	}
  return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CDumpInputPin::EndOfStream(void)
{
    CAutoLock lock(m_pReceiveLock);
    return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CDumpInputPin::Reset()
{
		LogDebug("Reset()...");
		m_bIsReceiving=FALSE;
		m_lTickCount=0;
}
BOOL CDumpInputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("lost signal...");
		}
		m_bIsReceiving=FALSE;
	}
	return m_bIsReceiving;
}
//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CDumpInputPin::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
    return S_OK;

} // NewSegment


//
//  CDump class
//
CDump::CDump(LPUNKNOWN pUnk, HRESULT *phr) :
    CUnknown(NAME("CDump"), pUnk),
    m_pFilter(NULL),
    m_pPin(NULL)
{
		LogDebug("CDump::ctor()");

		DeleteFile("MPFileWriter.log");
    m_pRecordFile = NULL;
		m_pTimeShiftFile=NULL;

    m_pFilter = new CDumpFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL) 
		{
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

	strcpy(m_strRecordingFileName,"");
	strcpy(m_strTimeShiftFileName,"");
}




// Destructor

CDump::~CDump()
{

    delete m_pPin;
    delete m_pFilter;

		if (m_pRecordFile!=NULL)
		{
			m_pRecordFile->CloseFile();
			delete m_pRecordFile;
			m_pRecordFile=NULL;
		}
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
		}
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
	if (riid == IID_IMPFileRecord)
	{
		return GetInterface((IMPFileRecord*)this, ppv);
	}
    else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) {
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

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

STDMETHODIMP CDump::SetTimeShiftFileName(char* pszFileName)
{
	strcpy(m_strTimeShiftFileName,pszFileName);
	strcat(m_strTimeShiftFileName,".tsbuffer");
	return S_OK;
}
STDMETHODIMP CDump::StartTimeShifting( )
{
	CAutoLock lock(&m_Lock);
	if (strlen(m_strTimeShiftFileName)==0) return E_FAIL;

	
	::DeleteFile((LPCTSTR) m_strRecordingFileName);
	LogDebug("Start TimeShifting:'%s'",m_strTimeShiftFileName);
	WCHAR wstrFileName[2048];
	MultiByteToWideChar(CP_ACP,0,m_strTimeShiftFileName,-1,wstrFileName,1+strlen(m_strTimeShiftFileName));
	m_pTimeShiftFile = new MultiFileWriter();
	if (FAILED(m_pTimeShiftFile->OpenFile(wstrFileName))) 
	{
		m_pTimeShiftFile->CloseFile();
		delete m_pTimeShiftFile;
		m_pTimeShiftFile=NULL;
		return E_FAIL;
	}
	return S_OK;

}	
STDMETHODIMP CDump::StopTimeShifting( )
{
	CAutoLock lock(&m_Lock);
	if (m_pTimeShiftFile==NULL) return S_OK;

	LogDebug("Stop TimeShifting:'%s'",m_strTimeShiftFileName);
	m_pTimeShiftFile->CloseFile();
	delete m_pTimeShiftFile;
	m_pTimeShiftFile=NULL;
	strcpy(m_strTimeShiftFileName,"");
	return S_OK;
}



STDMETHODIMP CDump::SetRecordingFileName(char* pszFileName)
{
	strcpy(m_strRecordingFileName,pszFileName);
	return S_OK;
}


STDMETHODIMP CDump::StartRecord( )
{
	CAutoLock lock(&m_Lock);
	if (strlen(m_strRecordingFileName)==0) return E_FAIL;

	::DeleteFile((LPCTSTR) m_strRecordingFileName);
	LogDebug("Start Recording:'%s'",m_strRecordingFileName);
	WCHAR wstrFileName[2048];
	MultiByteToWideChar(CP_ACP,0,m_strRecordingFileName,-1,wstrFileName,1+strlen(m_strRecordingFileName));

	m_pRecordFile = new FileWriter();
	m_pRecordFile->SetFileName( wstrFileName);
	if (FAILED(m_pRecordFile->OpenFile())) 
	{
		m_pRecordFile->CloseFile();
		delete m_pRecordFile;
		m_pRecordFile=NULL;
		return E_FAIL;
	}
	return S_OK;

}	
STDMETHODIMP CDump::StopRecord( )
{
	CAutoLock lock(&m_Lock);
	if (m_pRecordFile==NULL) return S_OK;

	LogDebug("Stop Recording:'%s'",m_strRecordingFileName);
	m_pRecordFile->FlushFile();
	m_pRecordFile->CloseFile();
	delete m_pRecordFile;
	m_pRecordFile=NULL;
	strcpy(m_strRecordingFileName,"");
	return S_OK;
}

HRESULT CDump::Write(PBYTE pbData, LONG lDataLength)
{
	CAutoLock lock(&m_Lock);
	DWORD written = 0;
	if (lDataLength<=0) 
	{
		LogDebug("write: datalen=%d", (int)lDataLength);
		return S_OK;
	}
	if (pbData==NULL) 
	{
		LogDebug("write: pbData=NULL");
		return S_OK;
	}

	if (m_pRecordFile!=NULL)
	{
		m_pRecordFile->Write(pbData,lDataLength);
	}
	if (m_pTimeShiftFile!=NULL)
	{
		m_pTimeShiftFile->Write(pbData,lDataLength);
	}
	return S_OK;
}

STDMETHODIMP  CDump::IsReceiving(BOOL* yesNo)
{
	*yesNo = m_pPin->IsReceiving();
	return S_OK;
}
STDMETHODIMP  CDump::Reset()
{
	m_pPin->Reset();
	return S_OK;
}