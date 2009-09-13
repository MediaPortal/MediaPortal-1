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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "MPFileWriter.h"
#include "liveMedia.hh"
#include "ChannelScan.h"

const AMOVIESETUP_MEDIATYPE mpeg2InputPinTypes[] =
{
	{&MEDIATYPE_Stream,&MEDIASUBTYPE_MPEG1System},
	{&MEDIATYPE_Stream,&MEDIASUBTYPE_MPEG2_PROGRAM}
};

// Setup data
const AMOVIESETUP_MEDIATYPE teletextInputPinType =
{
	&MEDIATYPE_VBI,         // Major type
	&MEDIASUBTYPE_TELETEXT	// Minor type
};

const AMOVIESETUP_PIN sudPins[] =
{
	{
		L"MPEG2 Input",             // Pin string name
			FALSE,                  // Is it rendered
			FALSE,                  // Is it an output
			FALSE,                  // Allowed none
			FALSE,                  // Likewise many
			&CLSID_NULL,            // Connects to filter
			L"Output",              // Connects to pin
			2,                      // Number of types
			mpeg2InputPinTypes      // Pin information
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
		}
};

const AMOVIESETUP_FILTER sudDump =
{
	&CLSID_MPFileWriter,        // Filter CLSID
	L"MediaPortal File Writer",	// String name
	MERIT_DO_NOT_USE,           // Filter merit
	2,                          // Number pins
	sudPins						// Pin details
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
	sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\MPFileWriter.Log",folder);

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
	L"MediaPortal File Writer", &CLSID_MPFileWriter, CMPFileWriter::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


// Constructor

CMPFileWriterFilter::CMPFileWriterFilter(CMPFileWriter *pMPFileWriter,
										 LPUNKNOWN pUnk,
										 CCritSec *pLock,
										 HRESULT *phr) :
CBaseFilter(NAME("MPFileWriter"), pUnk, pLock, CLSID_MPFileWriter),
m_pMPFileWriter(pMPFileWriter)
{
}


//
// GetPin
//
CBasePin * CMPFileWriterFilter::GetPin(int n)
{
	if (n == 0) {
		return m_pMPFileWriter->m_pMPEG2InputPin;
	}else if (n == 1) {
		return m_pMPFileWriter->m_pTeletextInputPin;
	} else {
		return NULL;
	}
}


//
// GetPinCount
//
int CMPFileWriterFilter::GetPinCount()
{
	return 2;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CMPFileWriterFilter::Stop()
{
	CAutoLock cObjectLock(m_pLock);

	LogDebug("CMPFileWriterFilter::Stop()");
	//m_pDump->Log(TEXT("graph Stop() called"),true);

	if (m_pMPFileWriter)
		m_pMPFileWriter->DeleteAllChannels();

	HRESULT result =  CBaseFilter::Stop();
	LogDebug("CMPFileWriterFilter::Stop() completed");
	return result;
}


//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CMPFileWriterFilter::Pause()
{
	LogDebug("CMPFileWriterFilter::Pause()");
	CAutoLock cObjectLock(m_pLock);

	if (m_pMPFileWriter)
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
STDMETHODIMP CMPFileWriterFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CMPFileWriterFilter::Run()");
	CAutoLock cObjectLock(m_pLock);


	return CBaseFilter::Run(tStart);
}


//
//  Definition of CMPFileWriterMPEG2InputPin
//
CMPFileWriterMPEG2InputPin::CMPFileWriterMPEG2InputPin(CMPFileWriter *pMPFileWriter,
													   LPUNKNOWN pUnk,
													   CBaseFilter *pFilter,
													   CCritSec *pLock,
													   CCritSec *pReceiveLock,
													   HRESULT *phr) :

CRenderedInputPin(NAME("CMPFileWriterMPEG2InputPin"),
				  pFilter,                   // Filter
				  pLock,                     // Locking
				  phr,                       // Return code
				  L"MPEG2 Input"),           // Pin name
				  m_pReceiveLock(pReceiveLock),
				  m_pMPFileWriter(pMPFileWriter)
{
	LogDebug("CMPFileWriterMPEG2InputPin:ctor");

	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CMPFileWriterMPEG2InputPin::CheckMediaType(const CMediaType *pType)
{
	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG1System == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG2_PROGRAM == pType->subtype){
		return S_OK;
	}
	return S_FALSE;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CMPFileWriterMPEG2InputPin::BreakConnect()
{

	return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CMPFileWriterMPEG2InputPin::ReceiveCanBlock()
{
	return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMPFileWriterMPEG2InputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("MPEG2: receive sample=null");
			return S_OK;
		}

		//		CheckPointer(pSample,E_POINTER);
		//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{

			LogDebug("MPEG2: receive samplelen:%d",sampleLen);
			return S_OK;
		}

		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("MPEG2: receive cannot get samplepointer");
			return S_OK;
		}
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("MPEG2: got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}
		m_pMPFileWriter->Write(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("MPEG2: receive exception");
	}
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CMPFileWriterMPEG2InputPin::EndOfStream(void)
{
	CAutoLock lock(m_pReceiveLock);
	return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CMPFileWriterMPEG2InputPin::Reset()
{
	LogDebug("MPEG2: Reset()...");
	m_bIsReceiving=FALSE;
	m_lTickCount=0;
}
BOOL CMPFileWriterMPEG2InputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("MPEG2: lost signal...");
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
STDMETHODIMP CMPFileWriterMPEG2InputPin::NewSegment(REFERENCE_TIME tStart,
													REFERENCE_TIME tStop,
													double dRate)
{
	return S_OK;

} // NewSegment



//
//  CMPFileWriter class
//
CMPFileWriter::CMPFileWriter(LPUNKNOWN pUnk, HRESULT *phr) :
CUnknown(NAME("CMPFileWriter"), pUnk),
m_pFilter(NULL),
m_pMPEG2InputPin(NULL),
m_pTeletextInputPin(NULL)
{
	LogDebug("CMPFileWriter::ctor()");

	DeleteFile("MPFileWriter.log");
	m_id=0;
	m_pFilter = new CMPFileWriterFilter(this, GetOwner(), &m_Lock, phr);
	if (m_pFilter == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pMPEG2InputPin = new CMPFileWriterMPEG2InputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pMPEG2InputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pTeletextInputPin = new CMPFileWriterTeletextInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pTeletextInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pChannelScan = new CChannelScan(GetOwner(),phr);

}




// Destructor

CMPFileWriter::~CMPFileWriter()
{
	LogDebug("CMPFilerWriter::dtor()");


	LogDebug("CMPFileWriter::dtor() - Stopping all subchannels");

	for (int i=0; i < (int)m_vecChannels.size();++i)
	{
		delete m_vecChannels[i];
	}
	m_vecChannels.clear();

	delete m_pMPEG2InputPin;
	m_pMPEG2InputPin = NULL;

	LogDebug("CMPFileWriterMPEG2InputPin::dtor() completed");

	delete m_pTeletextInputPin;
	m_pTeletextInputPin = NULL;

	LogDebug("CMPFileWriterTeletextInputPin::dtor() completed");

	delete m_pFilter;
	m_pFilter = NULL;
	LogDebug("CMPFileWriterFilter::dtor() completed");

	delete m_pChannelScan;
	m_pChannelScan = NULL;

	LogDebug("CChannelScan::dtor() completed");

}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CMPFileWriter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);

	CMPFileWriter *pNewObject = new CMPFileWriter(punk, phr);
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
STDMETHODIMP CMPFileWriter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	CheckPointer(ppv,E_POINTER);
	CAutoLock lock(&m_Lock);

	// Do we have this interface
	if (riid == IID_IMPFileRecord)
	{
		return GetInterface((IMPFileRecord*)this, ppv);
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

STDMETHODIMP CMPFileWriter::SetTimeShiftFileName(int subChannelId, char* pszFileName)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->SetTimeShiftFileName(pszFileName);
}

STDMETHODIMP CMPFileWriter::SetTimeShiftParams(int subChannelId, int minFiles, int maxFiles, ULONG maxFileSize)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->SetTimeShiftParams(minFiles, maxFiles,maxFileSize);
}
STDMETHODIMP CMPFileWriter::StartTimeShifting(int subChannelId)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->StartTimeShifting();

}	
STDMETHODIMP CMPFileWriter::StopTimeShifting(int subChannelId)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->StopTimeShifting();
}

STDMETHODIMP CMPFileWriter::PauseTimeShifting(int subChannelId, int onOff)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->PauseTimeShifting(onOff);
}


STDMETHODIMP CMPFileWriter::SetRecordingFileName(int subChannelId, char* pszFileName)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->SetRecordingFileName(pszFileName);
}


STDMETHODIMP CMPFileWriter::StartRecord(int subChannelId)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->StartRecord();
}	
STDMETHODIMP CMPFileWriter::StopRecord(int subChannelId)
{
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->StopRecord();
}

HRESULT CMPFileWriter::Write(PBYTE pbData, LONG lDataLength)
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
	for (int i=0; i < (int)m_vecChannels.size();++i)
	{
		m_vecChannels[i]->Write(pbData,lDataLength);
	}
	return S_OK;
}

HRESULT CMPFileWriter::WriteTeletext(PBYTE pbData, LONG lDataLength){
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
	for (int i=0; i < (int)m_vecChannels.size();++i)
	{
		m_vecChannels[i]->WriteTeletext(pbData,lDataLength);
	}
	if(m_pChannelScan!=NULL){
		m_pChannelScan->OnTeletextData(pbData,lDataLength);
	}
	return S_OK;
}


STDMETHODIMP  CMPFileWriter::IsReceiving(BOOL* yesNo)
{
	*yesNo = m_pMPEG2InputPin->IsReceiving();
	return S_OK;
}

STDMETHODIMP CMPFileWriter::TTxSetCallBack(int subChannelId, IAnalogTeletextCallBack* callback){
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->TTxSetCallBack(callback);
}

STDMETHODIMP CMPFileWriter::SetVideoAudioObserver(int subChannelId, IAnalogVideoAudioObserver* callback){
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->SetVideoAudioObserver(callback);
}

STDMETHODIMP CMPFileWriter::SetRecorderVideoAudioObserver(int subChannelId, IAnalogVideoAudioObserver* callback){
	CSubChannel* pSubChannel=GetSubChannel(subChannelId);
	if (pSubChannel==NULL) return S_OK;
	return pSubChannel->SetRecorderVideoAudioObserver(callback);
}

STDMETHODIMP CMPFileWriter::AddChannel(int* subChannelId)
{
	CAutoLock lock(&m_Lock);
	HRESULT hr;
	LogDebug("CMPFileWriter::AddChannel() - ID: %d",m_id);
	CSubChannel* channel = new CSubChannel(GetOwner(),&hr,m_id); 
	*subChannelId=m_id;
	m_id++;
	m_vecChannels.push_back(channel);
	return S_OK;
}

STDMETHODIMP CMPFileWriter::DeleteChannel( int subChannelId)
{
	CAutoLock lock(&m_Lock);
	LogDebug("CMPFileWriter::DeleteChannel() - ID: %d",subChannelId);
	try
	{
		ivecChannels it = m_vecChannels.begin();
		while (it != m_vecChannels.end())
		{
			if ((*it)->Handle()==subChannelId)
			{
				delete *it;
				m_vecChannels.erase(it);
				if (m_vecChannels.size()==0)
				{
					m_id=0;
				}
				return S_OK;
			}
			++it;
		}
	}
	catch(...)
	{
		LogDebug("exception in delete channel");
	}
	return S_OK;
}

CSubChannel* CMPFileWriter::GetSubChannel(int subChannelId)
{
	CAutoLock lock(&m_Lock);
	ivecChannels it = m_vecChannels.begin();
	while (it != m_vecChannels.end())
	{
		if ((*it)->Handle()==subChannelId)
		{
			return *it;
		}
		++it;
	}
	return NULL;
}

STDMETHODIMP CMPFileWriter::DeleteAllChannels()
{
	CAutoLock lock(&m_Lock);
	LogDebug("CMPFileWriter::DeleteAllChannels()");
	for (int i=0; i < (int)m_vecChannels.size();++i)
	{
		delete m_vecChannels[i];
	}
	m_vecChannels.clear();
	m_id=0;
	return S_OK;
}


STDMETHODIMP  CMPFileWriter::Reset()
{
	m_pMPEG2InputPin->Reset();
	m_pTeletextInputPin->Reset();
	return S_OK;
}



//
//  Definition of CMPFileWriterTeletextInputPin
//
CMPFileWriterTeletextInputPin::CMPFileWriterTeletextInputPin(CMPFileWriter *pMPFileWriter,
															 LPUNKNOWN pUnk,
															 CBaseFilter *pFilter,
															 CCritSec *pLock,
															 CCritSec *pReceiveLock,
															 HRESULT *phr) :

CRenderedInputPin(NAME("CMPFileWriterTeletextInputPin"),
				  pFilter,                   // Filter
				  pLock,                     // Locking
				  phr,                       // Return code
				  L"Teletext Input"),        // Pin name
				  m_pReceiveLock(pReceiveLock),
				  m_pMPFileWriter(pMPFileWriter)
{
	LogDebug("CMPFileWriterTeletextInputPin:ctor");

	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CMPFileWriterTeletextInputPin::CheckMediaType(const CMediaType *pType)
{
	if(MEDIATYPE_VBI == pType->majortype && MEDIASUBTYPE_TELETEXT == pType->subtype){
		return S_OK;
	}
	return S_FALSE;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CMPFileWriterTeletextInputPin::BreakConnect()
{

	return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CMPFileWriterTeletextInputPin::ReceiveCanBlock()
{
	return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMPFileWriterTeletextInputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("TELETEXT: receive sample=null");
			return S_OK;
		}

		//		CheckPointer(pSample,E_POINTER);
		//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{
			return S_OK;
		}

		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("TELETEXT: receive cannot get samplepointer");
			return S_OK;
		}
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("TELETEXT: got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}

		m_pMPFileWriter->WriteTeletext(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("TELETEXT: receive exception");
	}
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CMPFileWriterTeletextInputPin::EndOfStream(void)
{
	CAutoLock lock(m_pReceiveLock);
	return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CMPFileWriterTeletextInputPin::Reset()
{
	LogDebug("TELETEXT: Reset()...");
	m_bIsReceiving=FALSE;
	m_lTickCount=0;
}
BOOL CMPFileWriterTeletextInputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("TELETEXT: lost signal...");
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
STDMETHODIMP CMPFileWriterTeletextInputPin::NewSegment(REFERENCE_TIME tStart,
													   REFERENCE_TIME tStop,
													   double dRate)
{
	return S_OK;

} // NewSegment




