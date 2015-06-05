/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#pragma warning(disable : 4995)
#pragma warning(disable : 4996)
#include <process.h>
#include <windows.h>
#include <commdlg.h>
#include <time.h>
#include <streams.h>
#include <Ks.h>
#include <KsMedia.h>
#include <bdatypes.h>
#include <bdamedia.h>
#include <initguid.h>
#include <shlobj.h>
#include <tchar.h>
#include <queue>
#include "version.h"
#include "TsWriter.h"
#include "..\..\shared\tsheader.h"
#include "..\..\shared\DebugSettings.h"


// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
	&MEDIATYPE_Stream,							// Major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT   // Minor type
};

const AMOVIESETUP_PIN sudPins[] =
{
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
  },
  {
    L"OOB SI",                  // Pin string name
    FALSE,                      // Is it rendered
    FALSE,                      // Is it an output
    FALSE,                      // Allowed none
    FALSE,                      // Likewise many
    &CLSID_NULL,                // Connects to filter
    L"Output",                  // Connects to pin
    1,                          // Number of types
    &sudPinTypes                // Pin information
  }
};

const AMOVIESETUP_FILTER sudDump =
{
    &CLSID_MpTsFilter,          // Filter CLSID
    L"MediaPortal Ts Writer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    2,                          // Number pins
    sudPins,                    // Pin details
    CLSID_LegacyAmFilterCategory// Filter category
};

void DumpTs(byte* tspacket)
{
	FILE* fp=fopen("dump.ts", "ab+");
	fwrite(tspacket,1,188,fp);
	fclose(fp);
}

DEFINE_TVE_DEBUG_SETTING(DisableCRCCheck)
DEFINE_TVE_DEBUG_SETTING(DumpRawTS)

//-------------------- Async logging methods start -------------------------------------------------

WORD logFileParsed = -1;
WORD logFileDate = -1;
CMpTs* instanceID = 0;

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
  _stprintf(dest, _T("%s\\Team MediaPortal\\MediaPortal TV Server\\log\\TsWriter.%s"), folder, name);
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
  swprintf_s(msg, 5000,L"[%04.4d-%02.2d-%02.2d %02.2d:%02.2d:%02.2d,%03.3d] [%x] [%x] - %s\n",
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

//--------------- Async logging methods end ---------------------------------

//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
    L"MediaPortal Ts Writer", &CLSID_MpTsFilter, CMpTs::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


//================================================================================================
//================================================================================================
// Definition of CMpTsFilter
CMpTsFilter::CMpTsFilter(CMpTs* pDump, LPUNKNOWN pUnk, CCritSec* pFilterLock, CCritSec* pReceiveLock, HRESULT* pHr)
  : CBaseFilter(NAME("TsWriter"), pUnk, pFilterLock, CLSID_MpTsFilter),
    m_pWriterFilter(pDump), m_pReceiveLock(pReceiveLock)
{
}

CBasePin * CMpTsFilter::GetPin(int n)
{
  if (n == 0) 
	{
    return m_pWriterFilter->m_pPin;
  }
  else if (n == 1)
  {
    return m_pWriterFilter->m_pOobSiPin;
  }
	else 
	{
    return NULL;
  }
}

int CMpTsFilter::GetPinCount()
{
  return 2;
}

STDMETHODIMP CMpTsFilter::Stop()
{
	LogDebug("CMpTsFilter::Stop()");
    CAutoLock filterLock(m_pLock);
    LogDebug("Stop streaming...");
    CAutoLock receiveLock(m_pReceiveLock);
    LogDebug("Stop filter...");
    int hr = CBaseFilter::Stop();
    LogDebug("HRESULT = 0x%x", hr);
    return hr;
}

STDMETHODIMP CMpTsFilter::Pause()
{
	LogDebug("CMpTsFilter::Pause()");
    CAutoLock filterLock(m_pLock);
    LogDebug("Pause filter...");
    int hr = CBaseFilter::Pause();
    LogDebug("HRESULT = 0x%x", hr);
    return hr;
}

STDMETHODIMP CMpTsFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CMpTsFilter::Run()");
    CAutoLock filterLock(m_pLock);
    LogDebug("Run filter...");
    int hr = CBaseFilter::Run(tStart);
    LogDebug("HRESULT = 0x%x", hr);
    return hr;
}


//================================================================================================
//================================================================================================
//  Definition of CMpTsFilterPin
CMpTsFilterPin::CMpTsFilterPin(CMpTs* pDump, LPUNKNOWN pUnk, CBaseFilter* pFilter, CCritSec* pFilterLock, CCritSec *pReceiveLock,HRESULT *pHr)
	: CRenderedInputPin(NAME("CMpTsFilterPin"), pFilter, pFilterLock, pHr, L"TS Input"),
	m_pWriterFilter(pDump), m_pReceiveLock(pReceiveLock)
{
	LogDebug("CMpTsFilterPin::ctor");
    m_pRawPacketWriter = NULL;
}


STDMETHODIMP CMpTsFilterPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("CMpTsFilterPin: received NULL sample");
            return E_POINTER;
		}
		
		CAutoLock lock(m_pReceiveLock);
    
		if (IsStopped())
        {
	      LogDebug("CMpTsFilterPin: reject sample, wrong state");
          return VFW_E_WRONG_STATE;
        }

        long sampleLength = pSample->GetActualDataLength();
        if (sampleLength <= 0)		
		{
			return S_OK;
		}
		
        PBYTE pbData = NULL;
		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
          LogDebug("CMpTsFilterPin: failed to get sample pointer");
          return E_POINTER;
		}
		if (m_pRawPacketWriter!=NULL)
    {
			if (!m_pRawPacketWriter->IsFileInvalid())
      {
        m_pRawPacketWriter->Write(pbData,sampleLength);
      }
    }
		OnRawData(pbData, sampleLength);
	}
	catch(...)
	{
		LogDebug("CMpTsFilterPin: exception in Receive()");
	}
  return S_OK;
}

STDMETHODIMP CMpTsFilterPin::EndOfStream(void)
{
    LogDebug("CMpTsFilterPin::EndOfStream()");
    return CRenderedInputPin::EndOfStream();
}

STDMETHODIMP CMpTsFilterPin::ReceiveCanBlock()
{
	return S_FALSE;
}

HRESULT CMpTsFilterPin::CheckMediaType(const CMediaType* pMediaType)
{
    return S_OK;
}

HRESULT CMpTsFilterPin::BreakConnect()
{
	return CRenderedInputPin::BreakConnect();
}

void CMpTsFilterPin::Reset()
{
  LogDebug("CMpTsFilterPin::Reset()");
}

STDMETHODIMP CMpTsFilterPin::NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  // Seeking is not supported.
   return S_OK;
}

void CMpTsFilterPin::OnTsPacket(byte* tsPacket)
{
  m_pWriterFilter->AnalyzeTsPacket(tsPacket);
 }

void CMpTsFilterPin::AssignRawPacketWriter(FileWriter* pRawPacketWriter)
{
  m_pRawPacketWriter = pRawPacketWriter;
}

//================================================================================================
//================================================================================================
// Definition of CMpOobSiFilterPin
CMpOobSiFilterPin::CMpOobSiFilterPin(CMpTs* pDump, LPUNKNOWN pUnk, CBaseFilter* pFilter, CCritSec* pFilterLock, CCritSec* pReceiveLock, HRESULT* pHr)
  : CRenderedInputPin(NAME("CMpOobSiFilterPin"), pFilter, pFilterLock, pHr, L"OOB SI Input"),
    m_pWriterFilter(pDump), m_pReceiveLock(pReceiveLock)
{
  LogDebug("CMpOobSiFilterPin::ctor");
  m_pRawSectionWriter = NULL;
}

STDMETHODIMP CMpOobSiFilterPin::Receive(IMediaSample *pSample)
{
  try
  {
    if (pSample == NULL) 
    {
      LogDebug("CMpOobSiFilterPin: received NULL sample");
      return E_POINTER;
    }

    CAutoLock lock(m_pReceiveLock);
    if (IsStopped())
    {
      return VFW_E_WRONG_STATE;
    }
    long sampleLength = pSample->GetActualDataLength();
    if (sampleLength <= 1)
    {
      return S_OK;
    }

    PBYTE pbData = NULL;
    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) 
    {
      LogDebug("CMpOobSiFilterPin: failed to get sample pointer");
      return E_POINTER;
    }

    CSection s;
    s.table_id = pbData[0];
    s.section_length = sampleLength - 3;  // 1 for the table_id, 2 for the section_length bytes
    memcpy(s.Data, pbData, sampleLength);

    if (m_pRawSectionWriter != NULL)
    {
      if (!m_pRawSectionWriter->IsFileInvalid())
      {
        m_pRawSectionWriter->Write((BYTE*)&sampleLength, 4);
        m_pRawSectionWriter->Write(pbData, sampleLength);
      }
    }
    m_pWriterFilter->AnalyzeOobSiSection(s);
  }
  catch (...)
  {
    LogDebug("CMpOobSiFilterPin: exception in Receive()");
  }
  return S_OK;
}

STDMETHODIMP CMpOobSiFilterPin::EndOfStream(void)
{
  LogDebug("CMpOobSiFilterPin::EndOfStream()");
  return CRenderedInputPin::EndOfStream();
}

STDMETHODIMP CMpOobSiFilterPin::ReceiveCanBlock()
{
  return S_FALSE;
}

HRESULT CMpOobSiFilterPin::CheckMediaType(const CMediaType* pMediaType)
{
  if (pMediaType == NULL)
  {
    return S_FALSE;
  }
  // Only allow connection to pins that pass samples with the format that we support.
  if (IsEqualGUID(pMediaType->majortype, KSDATAFORMAT_TYPE_MPEG2_SECTIONS) &&
    IsEqualGUID(pMediaType->subtype, KSDATAFORMAT_SUBTYPE_BDA_OPENCABLE_OOB_PSIP))
  {
    return S_OK;
  }
  return S_FALSE;
}

HRESULT CMpOobSiFilterPin::BreakConnect()
{
  return CRenderedInputPin::BreakConnect();
}

STDMETHODIMP CMpOobSiFilterPin::NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  // Seeking is not supported.
  return S_OK;
}

void CMpOobSiFilterPin::AssignRawSectionWriter(FileWriter* pRawSectionWriter)
{
  m_pRawSectionWriter = pRawSectionWriter;
}
//================================================================================================
//================================================================================================


//
//  CMpTs class
//
CMpTs::CMpTs(LPUNKNOWN pUnk, HRESULT *pHr) 
:CUnknown(NAME("CMpTs"), pUnk),m_pFilter(NULL),m_pPin(NULL),m_pOobSiPin(NULL)
{
  m_id=0;
  instanceID = this;
  
  LogDebug(" ");
  LogDebug("CMpTs::ctor() ");
  LogDebug("================ New TsWriter filter instance =====================");
  LogDebug("  Logging format: [Date Time] [InstanceID] [ThreadID] Message....  ");
  LogDebug("===================================================================");
  LogDebug("---------------------- v%d.%d.%d.0 --------------------------------", TSWRITER_MAJOR_VERSION,TSWRITER_MID_VERSION,TSWRITER_VERSION);
  LogDebug("-- async logging, v4 deadlock, 0x46 scan, packetSync and PCR_hunt_v3 mods --");
  LogDebug(" ");  
		
  b_dumpRawPackets = false;
  m_pFilter = new CMpTsFilter(this, GetOwner(), &m_filterLock, &m_receiveLock, pHr);
  if (m_pFilter == NULL) 
  {
    *pHr = E_OUTOFMEMORY;
    return;
  }
  
  m_pPin = new CMpTsFilterPin(this, GetOwner(), m_pFilter, &m_filterLock, &m_receiveLock, pHr);
  if (m_pPin == NULL) 
  {
    *pHr = E_OUTOFMEMORY;
    return;
  }
  
  m_pOobSiPin = new CMpOobSiFilterPin(this, GetOwner(), m_pFilter, &m_filterLock, &m_receiveLock, pHr);
  if (m_pOobSiPin == NULL) 
  {
     *pHr = E_OUTOFMEMORY;
     return;
  }
    
	m_pChannelScanner= new CChannelScan(GetOwner(), pHr, m_pFilter);
  m_pEpgScanner = new CEpgScanner(GetOwner(), pHr);
  m_pChannelLinkageScanner = new CChannelLinkageScanner(GetOwner(), pHr);
  m_pRawPacketWriter = new FileWriter();
  m_pPin->AssignRawPacketWriter(m_pRawPacketWriter);
  //m_pOobSiPin->AssignRawSectionWriter(m_pRawPacketWriter);

}

// Destructor
CMpTs::~CMpTs()
{
  LogDebug("CMpTs::dtor() ");
  delete m_pPin;
  delete m_pOobSiPin;
  delete m_pFilter;
  delete m_pChannelScanner;
  delete m_pEpgScanner;
  delete m_pChannelLinkageScanner;
  delete m_pRawPacketWriter;
  DeleteAllChannels();
  StopLogger();
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
}


//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CMpTs::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
  CheckPointer(ppv,E_POINTER);

  // Do we have this interface
	if (riid == IID_ITSChannelScan)
	{
		//LogDebug("CMpTs:NonDelegatingQueryInterface IID_ITSChannelScan");
		return GetInterface((ITSChannelScan*)m_pChannelScanner, ppv);
	}
	else if (riid == IID_ITsEpgScanner)
	{
		//LogDebug("CMpTs:NonDelegatingQueryInterface IID_ITsEpgScanner");
		return GetInterface((ITsEpgScanner*)m_pEpgScanner, ppv);
	}
	else if (riid == IID_TSFilter)
	{
		//LogDebug("CMpTs:NonDelegatingQueryInterface IID_TSFilter");
		return GetInterface((ITSFilter*)this, ppv);
	}
	else if (riid == IID_ITsChannelLinkageScanner)
	{
		//LogDebug("CMpTs:NonDelegatingQueryInterface IID_ITsChannelLinkageScanner");
		return GetInterface((ITsChannelLinkageScanner*)m_pChannelLinkageScanner, ppv);
	}
  else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) 
	{
		//LogDebug("CMpTs:NonDelegatingQueryInterface other");
    return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
  } 
 
  return CUnknown::NonDelegatingQueryInterface(riid, ppv);
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
}

//
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2( FALSE );
}

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
	try
	{ 
    CAutoLock lock(&m_channelLock);
    for (int i=0; i < (int)m_vecChannels.size();++i)
    {
      m_vecChannels[i]->OnTsPacket(tsPacket);
    }
		m_pChannelScanner->OnTsPacket(tsPacket);
		m_pEpgScanner->OnTsPacket(tsPacket);
		m_pChannelLinkageScanner->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in AnalyzeTsPacket");
	}
}

void CMpTs::AnalyzeOobSiSection(CSection& section)
{
	try
	{
		m_pChannelScanner->OnOobSiSection(section);
	}
	catch(...)
	{
		LogDebug("exception in AnalyzeOobSiSection");
	}
}

STDMETHODIMP CMpTs::AddChannel( int* handle)
{
  LogDebug("debug: AddChannel()");
  CAutoLock lock(&m_channelLock);	
  HRESULT hr;
  CTsChannel* channel = new CTsChannel(GetOwner(), &hr,m_id); 
	*handle=m_id;
	m_id++;
  m_vecChannels.push_back(channel);
  LogDebug("debug: done AddChannel()");
  return S_OK;
}

STDMETHODIMP CMpTs::DeleteChannel( int handle)
{
    LogDebug("debug: DeleteChannel()");
    CAutoLock lock(&m_channelLock);
	try
	{
		ivecChannels it = m_vecChannels.begin();
		while (it != m_vecChannels.end())
		{
			if ((*it)->Handle()==handle)
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
  LogDebug("debug: done DeleteChannel()");
  return S_OK;
}

CTsChannel* CMpTs::GetTsChannel(int handle)
{
    CAutoLock lock(&m_channelLock);
	ivecChannels it = m_vecChannels.begin();
	while (it != m_vecChannels.end())
	{
		if ((*it)->Handle()==handle)
		{
			return *it;
		}
		++it;
	}
	return NULL;
}

STDMETHODIMP CMpTs::DeleteAllChannels()
{
  CAutoLock lock(&m_channelLock);
  LogDebug("--delete all channels");
  for (int i=0; i < (int)m_vecChannels.size();++i)
  {
    delete m_vecChannels[i];
  }
  m_vecChannels.clear();
	m_id=0;
  return S_OK;
}

STDMETHODIMP CMpTs::AnalyzerSetVideoPid(int handle, int videoPid)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->SetVideoPid(  videoPid);
}

STDMETHODIMP CMpTs::AnalyzerGetVideoPid(int handle,  int* videoPid)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->GetVideoPid(  videoPid);
}

STDMETHODIMP CMpTs::AnalyzerSetAudioPid(int handle,  int audioPid)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->SetAudioPid(  audioPid);
}

STDMETHODIMP CMpTs::AnalyzerGetAudioPid(int handle,  int* audioPid)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->GetAudioPid(  audioPid);
}

STDMETHODIMP CMpTs::AnalyzerIsVideoEncrypted(int handle,  int* yesNo)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->IsVideoEncrypted(  yesNo);
}

STDMETHODIMP CMpTs::AnalyzerIsAudioEncrypted(int handle,  int* yesNo)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->IsAudioEncrypted(  yesNo);
}

STDMETHODIMP CMpTs::AnalyzerReset(int handle )
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pVideoAnalyzer->Reset(  );
}

STDMETHODIMP CMpTs::PmtSetPmtPid(int handle,int pmtPid, long serviceId)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pPmtGrabber->SetPmtPid(pmtPid,serviceId  );
}

STDMETHODIMP CMpTs::PmtSetCallBack(int handle,IPMTCallback* callback)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pPmtGrabber->SetCallBack(callback);
}

STDMETHODIMP CMpTs::PmtGetPMTData (int handle,BYTE *pmtData)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pPmtGrabber->GetPMTData (pmtData);
}

STDMETHODIMP CMpTs::RecordSetRecordingFileNameW( int handle, wchar_t* pwszFileName)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
  pChannel->m_pRecorder->SetFileNameW(pwszFileName);
  return S_OK;
}

STDMETHODIMP CMpTs::RecordStartRecord( int handle)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	if (pChannel->m_pRecorder->Start())
		return S_OK;
	else
		return S_FALSE;
}

STDMETHODIMP CMpTs::RecordStopRecord( int handle)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pRecorder->Stop(  );
	return S_OK;
}

STDMETHODIMP CMpTs::RecordSetPmtPid(int handle,int mtPid, int serviceId,byte* pmtData,int pmtLength )
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pRecorder->SetPmtPid( mtPid, serviceId,pmtData,pmtLength );
	return S_OK;
}

STDMETHODIMP CMpTs::TimeShiftSetTimeShiftingFileNameW( int handle, wchar_t* pwszFileName)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;

  b_dumpRawPackets=false;
  if (DumpRawTS())
  {
    b_dumpRawPackets=true;
    wstring fileName=pwszFileName;
    fileName=fileName.substr(0, fileName.rfind(L"\\"));
    fileName.append(L"\\raw_packet_dump.ts");

    LogDebug(L"Setting name for raw packet dump file to %s", fileName.c_str());
    m_pRawPacketWriter->SetFileName(fileName.c_str());
  }
  pChannel->m_pTimeShifting->SetFileNameW(pwszFileName);
  return S_OK;
}
STDMETHODIMP CMpTs::TimeShiftStart( int handle )
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
  if (b_dumpRawPackets)
  {
	m_pRawPacketWriter->OpenFile();
    LogDebug("Raw packet dump file created. Now dumping raw packets to dump file");
  }
  if (pChannel->m_pTimeShifting->Start())
		return S_OK;
	else
		return S_FALSE;
}

STDMETHODIMP CMpTs::TimeShiftStop( int handle )
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
  if (b_dumpRawPackets)
  {
	  m_pRawPacketWriter->CloseFile();
    LogDebug("Raw packet dump file closed");
  }
	pChannel->m_pTimeShifting->Stop( );
	return S_OK;
}

STDMETHODIMP CMpTs:: TimeShiftReset( int handle )
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
  if (b_dumpRawPackets)
  {
    m_pRawPacketWriter->CloseFile();
    m_pRawPacketWriter->OpenFile();
    LogDebug("Raw packet dump file reset");
  }
	pChannel->m_pTimeShifting->Reset( );
	return S_OK;
}

STDMETHODIMP CMpTs:: TimeShiftGetBufferSize( int handle, long * size) 
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pTimeShifting->GetBufferSize( size);
	return S_OK;
}

STDMETHODIMP CMpTs:: TimeShiftSetPmtPid( int handle, int pmtPid, int serviceId,byte* pmtData,int pmtLength) 
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pTimeShifting->SetPmtPid( pmtPid,serviceId,pmtData,pmtLength);
	return S_OK;
}

STDMETHODIMP CMpTs:: TimeShiftPause( int handle, BYTE onOff) 
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pTimeShifting->Pause( onOff);
	return S_OK;
}

STDMETHODIMP CMpTs::TimeShiftSetParams(int handle, int minFiles, int maxFiles, ULONG chunkSize) 
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
  pChannel->m_pTimeShifting->SetMinTSFiles(minFiles);
  pChannel->m_pTimeShifting->SetMaxTSFiles(maxFiles);
  pChannel->m_pTimeShifting->SetMaxTSFileSize(chunkSize);
  pChannel->m_pTimeShifting->SetChunkReserve(chunkSize);
  return S_OK;
}

STDMETHODIMP CMpTs::TimeShiftGetCurrentFilePosition(int handle,__int64 * position,long * bufferId)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pTimeShifting->GetTimeShiftPosition(position,bufferId);
  return S_OK;
}

STDMETHODIMP CMpTs::SetVideoAudioObserver(int handle, IVideoAudioObserver* callback)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_FALSE;
  pChannel->m_pTimeShifting->SetVideoAudioObserver(callback);
	return S_OK;
}

STDMETHODIMP CMpTs::RecordSetVideoAudioObserver(int handle, IVideoAudioObserver* callback)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_FALSE;
  pChannel->m_pRecorder->SetVideoAudioObserver(callback);
	return S_OK;
}

STDMETHODIMP CMpTs::TTxStart( int handle)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pTeletextGrabber->Start( );
}

STDMETHODIMP CMpTs::TTxStop( int handle )
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pTeletextGrabber->Stop( );
}

STDMETHODIMP CMpTs::TTxSetTeletextPid( int handle,int teletextPid)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pTeletextGrabber->SetTeletextPid(teletextPid );
}

STDMETHODIMP CMpTs::TTxSetCallBack( int handle,ITeletextCallBack* callback)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pTeletextGrabber->SetCallBack(callback );
}

STDMETHODIMP CMpTs::CaSetCallBack(int handle,ICACallback* callback)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pCaGrabber->SetCallBack(callback );
}

STDMETHODIMP CMpTs::CaGetCaData(int handle,BYTE *caData)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pCaGrabber->GetCaData(caData );
}

STDMETHODIMP CMpTs::CaReset(int handle)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	return pChannel->m_pCaGrabber->Reset( );
}

STDMETHODIMP CMpTs::GetStreamQualityCounters(int handle, int* totalTsBytes, int* totalRecordingBytes, 
      int* TsDiscontinuity, int* recordingDiscontinuity)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_FALSE;

  if (pChannel->m_pTimeShifting)
  {
    pChannel->m_pTimeShifting->GetDiscontinuityCounter(TsDiscontinuity);
    pChannel->m_pTimeShifting->GetTotalBytes(totalTsBytes);
  }

  if (pChannel->m_pRecorder)
  {
    pChannel->m_pRecorder->GetDiscontinuityCounter(recordingDiscontinuity);
    pChannel->m_pRecorder->GetTotalBytes(totalRecordingBytes);
  }

  if (pChannel->m_pRecorder || pChannel->m_pTimeShifting)
    return S_OK;
  else
    return S_FALSE;
}


STDMETHODIMP CMpTs::TimeShiftSetChannelType(int handle, int channelType)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* pChannel=GetTsChannel(handle);
  if (pChannel==NULL) return S_OK;
	pChannel->m_pRecorder->SetChannelType(channelType);
	pChannel->m_pTimeShifting->SetChannelType(channelType);
	return S_OK;
}