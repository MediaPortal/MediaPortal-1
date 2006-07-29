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

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
	&MEDIATYPE_Stream,            // Major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT          // Minor type
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
    &CLSID_TsWriter,          // Filter CLSID
    L"MediaPortal Ts Writer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};

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
    L"MediaPortal Ts Writer", &CLSID_TsWriter, CDump::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


// Constructor

CDumpFilter::CDumpFilter(CDump *pDump,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("TsWriter"), pUnk, pLock, CLSID_TsWriter),
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

	m_pDump->ResetAnalyer();

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
		m_pDump->Analyze(pbData,sampleLen);

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

		DeleteFile("TsWriter.log");

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

		ResetAnalyer();
}




// Destructor

CDump::~CDump()
{

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
		return GetInterface((ITsWriter*)this, ppv);
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



STDMETHODIMP CDump::SetVideoPid( int videoPid)
{
	m_videoPid=videoPid;
	LogDebug("videopid:%x",m_videoPid);
	ResetAnalyer();
	return S_OK;
}
STDMETHODIMP CDump::GetVideoPid( int* videoPid)
{
	*videoPid=m_videoPid;
	return S_OK;
}

HRESULT CDump::GetTSHeader(BYTE *data,TSHeader *header)
{
	header->SyncByte=data[0];
	header->TransportError=(data[1] & 0x80)>0?true:false;
	header->PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	header->TransportPriority=(data[1] & 0x20)>0?true:false;
	header->Pid=((data[1] & 0x1F) <<8)+data[2];
	header->TScrambling=data[3] & 0xC0;
	header->AdaptionControl=(data[3]>>4) & 0x3;
	header->ContinuityCounter=data[3] & 0x0F;
	return S_OK;
}
STDMETHODIMP CDump::SetAudioPid( int audioPid)
{
	m_audioPid=audioPid;
	LogDebug("audiopid:%x",audioPid);
	ResetAnalyer();
	return S_OK;
}
STDMETHODIMP CDump::GetAudioPid( int* audioPid)
{
	*audioPid=m_audioPid;
	return S_OK;
}

STDMETHODIMP CDump::IsVideoEncrypted( int* yesNo)
{
	*yesNo = (m_bVideoEncrypted?1:0);
	return S_OK;
}
STDMETHODIMP CDump::IsAudioEncrypted( int* yesNo)
{
	*yesNo = (m_bAudioEncrypted?1:0);
	return S_OK;
}

STDMETHODIMP CDump::ResetAnalyer()
{
		m_bInitAudio=TRUE;
		m_bInitVideo=TRUE;
		m_bVideoEncrypted=TRUE;
		m_bAudioEncrypted=TRUE;
		m_audioTimer=GetTickCount();
		m_videoTimer=GetTickCount();
		return S_OK;
}

int CDump::FindOffset(BYTE* pbData, int nLen)
{
	for (int i=0; i < nLen;i++)
	{
		if (i+5*188 > nLen) break;
		{
			if (pbData[i]==0x47 && pbData[i+188]==0x47 && pbData[i+2*188]==0x47 && pbData[i+3*188]==0x47 && pbData[i+4*188]==0x47)
			{
				return i;
			}
		}
	}
	return -1;
}

void CDump::LogHeader(TSHeader& header)
{
	LogDebug("  SyncByte         :%x", header.SyncByte);
	LogDebug("  TransportError   :%x", header.TransportError);
	LogDebug("  PayloadUnitStart :%d", header.PayloadUnitStart);
	LogDebug("  TransportPriority:%x", header.TransportPriority);
	LogDebug("  Pid              :%x", header.Pid);
	LogDebug("  TScrambling      :%x", header.TScrambling);
	LogDebug("  AdaptionControl  :%x", header.AdaptionControl);
	LogDebug("  ContinuityCounter:%x", header.ContinuityCounter);

}
void CDump::Analyze(BYTE* pbData, int nLen)
{

	int i=FindOffset(pbData,nLen);
	if (i<0) return;

	bool gotVideo=false;
	bool gotAudio=false;
	TSHeader  header;
	int pktCounter=0;
	for (; i < nLen;i+=188)
	{
		pktCounter++;
		GetTSHeader(&pbData[i],&header);
		if (header.SyncByte!=0x47) return;
		if (header.TransportError==true) return;
		if (header.ContinuityCounter!=0)  continue;
		if (gotVideo && gotAudio) return;
		BOOL scrambled= (header.TScrambling!=0);
		if (header.Pid==m_audioPid) 
		{
			gotAudio=true;
			//LogDebug("audio:%x",header.TScrambling);
			if (TRUE==scrambled)
			{
				m_audioTimer=GetTickCount();
			}

			if (scrambled != m_bAudioEncrypted || m_bInitAudio)
			{
				m_bInitAudio=FALSE;
				if (FALSE == scrambled)
				{
					DWORD timeSpan=GetTickCount()-m_audioTimer;
					if (timeSpan > 150)
					{
						LogDebug("audio pid %x unscrambled", m_audioPid);
						m_bAudioEncrypted=scrambled;
						//LogHeader(header);
					}
				}
				else
				{
						LogDebug("audio pid %x scrambled %i", m_audioPid,pktCounter);
					//LogHeader(header);
					m_bAudioEncrypted=scrambled;
				}
			}
		}

		if (header.Pid==m_videoPid) 
		{
			gotVideo=true;
			//LogDebug("video:%x",header.TScrambling);
			if (TRUE==scrambled)
			{
				m_videoTimer=GetTickCount();
			}

			if (scrambled != m_bVideoEncrypted || m_bInitVideo)
			{
				m_bInitVideo=FALSE;
				if (FALSE == scrambled)
				{
					DWORD timeSpan=GetTickCount()-m_videoTimer;
					if (timeSpan > 150)
					{
						LogDebug("video pid %x unscrambled", m_videoPid);
						//LogHeader(header);
						m_bVideoEncrypted=scrambled;
					}
				}
				else
				{
					LogDebug("video pid %x scrambled %i", m_videoPid,pktCounter);
					//LogHeader(header);
					m_bVideoEncrypted=scrambled;
				}
			}
		}
	}
}