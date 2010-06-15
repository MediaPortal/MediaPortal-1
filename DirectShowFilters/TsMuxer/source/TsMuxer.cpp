/* 
*	Copyright (C) 2005-2008 Team MediaPortal
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

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "TsMuxer.h"
#include "liveMedia.hh"
#include "ChannelScan.h"
#include "TsOutputPin.h"

const AMOVIESETUP_MEDIATYPE tsPinType =
{
	&MEDIATYPE_Stream,				// Major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT	// Minor type
};

const AMOVIESETUP_PIN tsMuxerPins[] =
{
	{
		L"TS Input",				// Pin string name
			FALSE,                  // Is it rendered
			FALSE,                  // Is it an output
			FALSE,                  // Allowed none
			FALSE,                  // Likewise many
			&CLSID_NULL,            // Connects to filter
			L"Output",              // Connects to pin
			1,                      // Number of types
			&tsPinType				// Pin information
	},
	{
		L"MPEG2 Input",             // Pin string name
			FALSE,                  // Is it rendered
			FALSE,                  // Is it an output
			FALSE,                  // Allowed none
			FALSE,                  // Likewise many
			&CLSID_NULL,            // Connects to filter
			L"Output",              // Connects to pin
			2,                      // Number of types
			mpegInputPinTypes		// Pin information
		},
		{
			L"Video Input",             // Pin string name
				FALSE,                  // Is it rendered
				FALSE,                  // Is it an output
				FALSE,                  // Allowed none
				FALSE,                  // Likewise many
				&CLSID_NULL,            // Connects to filter
				L"Output",              // Connects to pin
				4,                      // Number of types
				videoInputPinTypes      // Pin information
		},
		{
			L"Audio Input",             // Pin string name
				FALSE,                  // Is it rendered
				FALSE,                  // Is it an output
				FALSE,                  // Allowed none
				FALSE,                  // Likewise many
				&CLSID_NULL,            // Connects to filter
				L"Output",              // Connects to pin
				6,                      // Number of types
				audioInputPinTypes      // Pin information
			},
			{
				L"Teletext Input",          // Pin string name
					FALSE,                  // Is it rendered
					FALSE,                  // Is it an output
					FALSE,                  // Allowed none
					FALSE,                  // Likewise many
					&CLSID_NULL,            // Connects to filter
					L"Output",              // Connects to pin
					1,						// Number of types
					&teletextInputPinType	// Pin information
			},
			{
				L"TS Output",			// Pin string name
					FALSE,              // Is it rendered
					TRUE,               // Is it an output
					FALSE,              // Allowed none
					FALSE,              // Likewise many
					&CLSID_NULL,        // Connects to filter
					L"Output",          // Connects to pin
					1,					// Number of types
					&tsPinType			// Pin information
				}
};

const AMOVIESETUP_FILTER tsMuxer =
{
	&CLSID_TsMuxer,				// Filter CLSID
	L"MediaPortal Ts Muxer",		// String name
	MERIT_DO_NOT_USE,           // Filter merit
	6,                          // Number pins
	tsMuxerPins					// Pin details
};

static char logbuffer[2000]; 

void LogDebug(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	TCHAR folder[MAX_PATH];
	TCHAR fileName[MAX_PATH];
	::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
	sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\TsMuxer.Log",folder);

	FILE* fp = fopen(fileName,"a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,systemTime.wMilliseconds,
			logbuffer);
		fclose(fp);
	}
};

//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
	L"MediaPortal Ts Muxer", &CLSID_TsMuxer, CTsMuxer::CreateInstance, NULL, &tsMuxer
};
int g_cTemplates = 1;


// Constructor

CTsMuxerFilter::CTsMuxerFilter(CTsMuxer *pTsMuxer,
							   LPUNKNOWN pUnk,
							   CCritSec *pLock,
							   HRESULT *phr) :
CBaseFilter(NAME("Ts Muxer"), pUnk, pLock, CLSID_TsMuxer),
m_pTsMuxer(pTsMuxer)
{
}


//
// GetPin
//
CBasePin * CTsMuxerFilter::GetPin(int n)
{
	if (n == 0) {
		return m_pTsMuxer->m_pMPEGInputPin;
	}else if (n == 1) {
		return m_pTsMuxer->m_pVideoInputPin;
	}else if (n == 2) {
		return m_pTsMuxer->m_pAudioInputPin;
	}else if (n == 3) {
		return m_pTsMuxer->m_pTeletextInputPin;
	}else if (n == 4) {
		return m_pTsMuxer->m_pTsInputPin;
	}else if (n == 5) {
		return m_pTsMuxer->m_pTsOutputPin;
	} else {
		return NULL;
	}
}


//
// GetPinCount
//
int CTsMuxerFilter::GetPinCount()
{
	return 6;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CTsMuxerFilter::Stop()
{
	CAutoLock cObjectLock(m_pLock);

	LogDebug("CTsMuxerFilter::Stop()");
	//m_pDump->Log(TEXT("graph Stop() called"),true);

	HRESULT result =  CBaseFilter::Stop();
	LogDebug("CTsMuxerFilter::Stop() completed");
	return result;
}


//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CTsMuxerFilter::Pause()
{
	LogDebug("CTsMuxerFilter::Pause()");
	CAutoLock cObjectLock(m_pLock);

	if (m_pTsMuxer != NULL)
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
		m_pTsMuxer->m_pProgramTsWriter->Close();
		m_pTsMuxer->m_pElementaryTsWriter->Close();
	}
	LogDebug("CTsMuxerFilter::Pause() finished");
	return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CTsMuxerFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CTsMuxerFilter::Run()");
	CAutoLock cObjectLock(m_pLock);

	m_pTsMuxer->m_pProgramTsWriter->Initialize(m_pTsMuxer->m_pTsOutputPin);
	m_pTsMuxer->m_pElementaryTsWriter->Initialize(m_pTsMuxer->m_pTsOutputPin);
	return CBaseFilter::Run(tStart);
}


//
//  CTsMuxer class
//
CTsMuxer::CTsMuxer(LPUNKNOWN pUnk, HRESULT *phr) :
CUnknown(NAME("CTsMuxer"), pUnk),
m_pFilter(NULL),
m_pMPEGInputPin(NULL),
m_pVideoInputPin(NULL),
m_pAudioInputPin(NULL),
m_pTeletextInputPin(NULL),
m_pTsInputPin(NULL),
m_pTsOutputPin(NULL)
{
	LogDebug("CTsMuxer::ctor()");

	DeleteFile("TsMuxer.log");
	m_pFilter = new CTsMuxerFilter(this, GetOwner(), &m_Lock, phr);
	if (m_pFilter == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pMPEGInputPin = new CTsMuxerMPEGInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pMPEGInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pVideoInputPin = new CTsMuxerVideoInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pVideoInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pAudioInputPin = new CTsMuxerAudioInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pAudioInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pTeletextInputPin = new CTsMuxerTeletextInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pTeletextInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pTsInputPin = new CTsMuxerTsInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pTsInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pTsOutputPin = new CTsMuxerTsOutputPin(GetOwner(),
		m_pFilter,
		&m_Lock,
		phr);
	if (m_pTsOutputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pChannelScan = new CChannelScan(GetOwner(),phr);
	m_pTeletextGrabber = new CTeletextGrabber();
	m_pProgramTsWriter = new CProgramToTransportStream();
	m_pElementaryTsWriter = new CElementaryToTransportStream();
}




// Destructor

CTsMuxer::~CTsMuxer()
{
	LogDebug("CMPFilerWriter::dtor()");


	delete m_pMPEGInputPin;
	m_pMPEGInputPin = NULL;

	LogDebug("CTsMuxerMPEGInputPin::dtor() completed");

	delete m_pVideoInputPin;
	m_pVideoInputPin = NULL;

	LogDebug("CTsMuxerVideoInputPin::dtor() completed");

	delete m_pAudioInputPin;
	m_pAudioInputPin = NULL;

	LogDebug("CTsMuxerAudioInputPin::dtor() completed");

	delete m_pTeletextInputPin;
	m_pTeletextInputPin = NULL;

	LogDebug("CTsMuxerTeletextInputPin::dtor() completed");

	delete m_pTsInputPin;
	m_pTsInputPin = NULL;

	LogDebug("CTsMuxerTsInputPin::dtor() completed");

	delete m_pTsOutputPin;
	m_pTsOutputPin = NULL;

	LogDebug("CTsMuxerTsOutputPin::dtor() completed");

	delete m_pFilter;
	m_pFilter = NULL;
	LogDebug("CTsMuxerFilter::dtor() completed");

	delete m_pChannelScan;
	m_pChannelScan = NULL;

	LogDebug("CChannelScan::dtor() completed");

	delete m_pTeletextGrabber;
	m_pTeletextGrabber = NULL;

	LogDebug("CTeletextGrabber::dtor() completed");

	delete m_pProgramTsWriter;
	m_pProgramTsWriter = NULL;

	LogDebug("CProgramToTransportStream::dtor() completed");

	delete m_pElementaryTsWriter;
	m_pElementaryTsWriter = NULL;

	LogDebug("CElementaryToTransportStream::dtor() completed");
}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CTsMuxer::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);

	CTsMuxer *pNewObject = new CTsMuxer(punk, phr);
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
STDMETHODIMP CTsMuxer::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	CheckPointer(ppv,E_POINTER);
	CAutoLock lock(&m_Lock);

	// Do we have this interface
	if (riid == IID_ITsMuxer)
	{
		return GetInterface((ITsMuxer*)this, ppv);
	}else if(riid == IID_IAnalogChanelScan){
		return GetInterface((IAnalogChanelScan*)m_pChannelScan,ppv);
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

STDMETHODIMP CTsMuxer::GetPMTPid(int pmtPid)
{
	pmtPid = 0x20;
	return S_OK;
}

STDMETHODIMP CTsMuxer::GetServiceId(int serviceId)
{
	serviceId = 1;
	return S_OK;
}
HRESULT CTsMuxer::WriteProgram(PBYTE pbData, LONG lDataLength)
{
	CAutoLock lock(&m_Lock);
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
	m_pProgramTsWriter->Write(pbData,lDataLength);
	return S_OK;
}

HRESULT CTsMuxer::WriteVideo(PBYTE pbData, LONG lDataLength)
{
	CAutoLock lock(&m_Lock);
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
	m_pElementaryTsWriter->WriteVideo(pbData,lDataLength);
	return S_OK;
}

HRESULT CTsMuxer::WriteAudio(PBYTE pbData, LONG lDataLength)
{
	CAutoLock lock(&m_Lock);
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
	m_pElementaryTsWriter->WriteAudio(pbData,lDataLength);
	return S_OK;
}

HRESULT CTsMuxer::WriteTeletext(PBYTE pbData, LONG lDataLength){
	if (lDataLength<=0) 
	{
		LogDebug("teletext write: datalen=%d", (int)lDataLength);
		return S_OK;
	}
	if (pbData==NULL) 
	{
		LogDebug("teletext write: pbData=NULL");
		return S_OK;
	}
	if(m_pTeletextGrabber!=NULL){
		m_pTeletextGrabber->OnSampleReceived(pbData,lDataLength);
	}

	if(m_pChannelScan!=NULL){
		m_pChannelScan->OnTeletextData(pbData,lDataLength);
	}
	return S_OK;
}


STDMETHODIMP  CTsMuxer::IsReceiving(BOOL* yesNo)
{
	*yesNo = m_pMPEGInputPin->IsReceiving();
	return S_OK;
}

STDMETHODIMP CTsMuxer::TTxSetCallBack(IAnalogTeletextCallBack* callback){
	m_pTeletextGrabber->SetCallBack(callback);
	return S_OK;
}

STDMETHODIMP  CTsMuxer::Reset()
{
	m_pMPEGInputPin->Reset();
	m_pTeletextInputPin->Reset();
	return S_OK;
}



