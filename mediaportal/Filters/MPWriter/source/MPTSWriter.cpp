#include <windows.h>
#include <commdlg.h>
#include <streams.h>
#include <initguid.h>
#include "MPTSWriter.h"

//maxium timeshifting length
#define MAX_FILE_LENGTH (2000LL*1024LL*1024LL)  // 2 gigabyte
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
    &CLSID_MPTSWriter,          // Filter CLSID
    L"MediaPortal TS Writer",   // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};


//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
    L"MediaPortal TS Writer", &CLSID_MPTSWriter, CDump::CreateInstance, NULL, &sudDump
};
int g_cTemplates = 1;


// Constructor

CDumpFilter::CDumpFilter(CDump *pDump,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("WSFileWriter"), pUnk, pLock, CLSID_MPTSWriter),
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
	
	m_pDump->Log(TEXT("graph Stop() called"),true);

    if (m_pDump)
        m_pDump->CloseFile();
    
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
        // GraphEdit calls Pause() before calling Stop() for this filter.
        // If we have encountered a write error (such as disk full),
        // then stopping the graph could cause our log to be deleted
        // (because the current log file handle would be invalid).
        // 
        // To preserve the log, don't open/create the log file on pause
        // if we have previously encountered an error.  The write error
        // flag gets cleared when setting a new log file name or
        // when restarting the graph with Run().
        if (!m_pDump->m_fWriteError)
        {
            m_pDump->OpenFile();
        }
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

    // Clear the global 'write error' flag that would be set
    // if we had encountered a problem writing the previous dump file.
    // (eg. running out of disk space).
    //
    // Since we are restarting the graph, a new file will be created.
    m_pDump->m_fWriteError = FALSE;

    if (m_pDump)
        m_pDump->OpenFile();

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
	ResetPids();

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

HRESULT CDumpInputPin::SetVideoPid(int videoPid)
{
	m_videoPid=videoPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetAudioPid(int audioPid)
{
	m_audio1Pid=audioPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetAudioPid2(int audioPid)
{
	m_audio2Pid=audioPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetAC3Pid(int ac3Pid)
{
	m_ac3Pid=ac3Pid;
	return S_OK;
}
HRESULT CDumpInputPin::SetTeletextPid(int ttxtPid)
{
	m_ttxtPid=ttxtPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetSubtitlePid(int subtitlePid)
{
	m_subtitlePid=subtitlePid;
	return S_OK;
}
HRESULT CDumpInputPin::SetPMTPid(int pmtPid)
{
	m_pmtPid=pmtPid;
	return S_OK;
}
HRESULT CDumpInputPin::SetPCRPid(int pcrPid)
{
	m_pcrPid=pcrPid;
	return S_OK;
}

int CDumpInputPin::GetVideoPid()
{
	return m_videoPid;
}
int CDumpInputPin::GetAudioPid()
{
	return m_audio1Pid;
}
int CDumpInputPin::GetAudioPid2()
{
	return m_audio2Pid;
}
int CDumpInputPin::GetAC3Pid()
{
	return m_ac3Pid;
}
int CDumpInputPin::GetTeletextPid()
{
	return m_ttxtPid;
}
int CDumpInputPin::GetSubtitlePid()
{
	return m_subtitlePid;
}
int CDumpInputPin::GetPMTPid()
{
	return m_pmtPid;
}
int CDumpInputPin::GetPCRPid()
{
	return m_pcrPid;
}

//
// BreakConnect
//
// Break a connection
//
HRESULT CDumpInputPin::BreakConnect()
{
    if (m_pDump->m_pPosition != NULL) {
        m_pDump->m_pPosition->ForceRefresh();
    }

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
    CheckPointer(pSample,E_POINTER);

    CAutoLock lock(m_pReceiveLock);
    PBYTE pbData;

    // Has the filter been stopped yet?
    if (m_pDump->m_hFile == INVALID_HANDLE_VALUE) {
        return NOERROR;
    }

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    m_tLast = tStart;

    // Copy the data to the file

    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) {
        return hr;
    }

	for(DWORD t=0;t<(DWORD)pSample->GetActualDataLength();t+=188)
	{
		if(pbData[t]==0x47)
		{
			int pid=((pbData[t+1] & 0x1F) <<8)+pbData[t+2];
			m_pDump->Log(TEXT("Receive: t="),false);
			m_pDump->Log((__int64)t,false);
			m_pDump->Log(TEXT(", pid="),false);
			m_pDump->Log((__int64)pid,true);
			if(IsPidValid(pid)==true)
			{
				hr=m_pDump->Write(pbData+t,188);
				m_pDump->Log(TEXT("Receive: hr="),false);
				m_pDump->Log((__int64)hr,true);
				if(FAILED(hr))
					break;
			}
			else
				m_pDump->Log(TEXT("Receive: ignored pid"),true);

		}
	}
	m_pDump->UpdateInfoFile();

    return NOERROR;
}
void CDumpInputPin::ResetPids()
{
	m_videoPid=m_audio1Pid=m_audio2Pid=m_ac3Pid=m_ttxtPid=m_subtitlePid=m_pmtPid=m_pcrPid=-1;
}
bool CDumpInputPin::IsPidValid(int pid)
{
	if(pid==0 || pid==1 || pid==0x11||pid==m_videoPid || pid==m_audio1Pid ||
		pid==m_audio2Pid || pid==m_ac3Pid || pid==m_ttxtPid || pid==m_subtitlePid || 
		pid==m_pmtPid|| pid==m_pcrPid)
		return true;
	return false;
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
    m_pPin(NULL),
    m_pPosition(NULL),
    m_hFile(INVALID_HANDLE_VALUE),
    m_pFileName(0),
    m_fWriteError(0),
	currentPosition(0),
	currentFileLength(0)
{
    ASSERT(phr);
	m_logFileHandle=INVALID_HANDLE_VALUE;
	m_hInfoFile=INVALID_HANDLE_VALUE;
    
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


STDMETHODIMP CDump::SetFileName(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
    // Is this a valid filename supplied

    CheckPointer(pszFileName,E_POINTER);
    if(wcslen(pszFileName) > MAX_PATH)
        return ERROR_FILENAME_EXCED_RANGE;

    // Take a copy of the filename

    m_pFileName = new WCHAR[1+lstrlenW(pszFileName)];
    if (m_pFileName == 0)
        return E_OUTOFMEMORY;

    wcscpy(m_pFileName,pszFileName);

    // Clear the global 'write error' flag that would be set
    // if we had encountered a problem writing the previous dump file.
    // (eg. running out of disk space).
    m_fWriteError = FALSE;

    // Create the file then close it

    HRESULT hr = OpenFile();
    CloseFile();

    return hr;

} // SetFileName


//
// GetCurFile
//
// Implemented for IFileSinkFilter support
//
STDMETHODIMP CDump::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
{
    CheckPointer(ppszFileName, E_POINTER);
    *ppszFileName = NULL;

    if (m_pFileName != NULL) 
    {
        *ppszFileName = (LPOLESTR)
        QzTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(m_pFileName)));

        if (*ppszFileName != NULL) 
        {
            wcscpy(*ppszFileName, m_pFileName);
        }
    }

    if(pmt) 
    {
        ZeroMemory(pmt, sizeof(*pmt));
        pmt->majortype = MEDIATYPE_NULL;
        pmt->subtype = MEDIASUBTYPE_NULL;
    }

    return S_OK;

} // GetCurFile


// Destructor

CDump::~CDump()
{
    CloseFile();

    delete m_pPin;
    delete m_pFilter;
    delete m_pPosition;
    delete m_pFileName;
	
	if(m_logFileHandle!=INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_logFileHandle);
		m_logFileHandle=INVALID_HANDLE_VALUE;
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
	if (riid == IID_IMPTSWriter)
	{
		return GetInterface((IMPTSWriter*)this, ppv);
	}

    if (riid == IID_IFileSinkFilter) {
        return GetInterface((IFileSinkFilter *) this, ppv);
    } 
    else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) {
        return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
    } 
    else if (riid == IID_IMediaPosition || riid == IID_IMediaSeeking) {
        if (m_pPosition == NULL) 
        {

            HRESULT hr = S_OK;
            m_pPosition = new CPosPassThru(NAME("Dump Pass Through"),
                                           (IUnknown *) GetOwner(),
                                           (HRESULT *) &hr, m_pPin);
            if (m_pPosition == NULL) 
                return E_OUTOFMEMORY;

            if (FAILED(hr)) 
            {
                delete m_pPosition;
                m_pPosition = NULL;
                return hr;
            }
        }

        return m_pPosition->NonDelegatingQueryInterface(riid, ppv);
    } 

    return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface
STDMETHODIMP CDump::SetVideoPid(int pid)
{
	Log(TEXT("SetVideoPid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetVideoPid(pid);
}
STDMETHODIMP CDump::SetAudioPid(int pid)
{
	Log(TEXT("SetAudioPid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetAudioPid(pid);
}
STDMETHODIMP CDump::SetAudioPid2(int pid)
{
	Log(TEXT("SetAudioPid2 ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetAudioPid2(pid);
}
STDMETHODIMP CDump::SetAC3Pid(int pid)
{
	Log(TEXT("SetAC3Pid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetAC3Pid(pid);
}
STDMETHODIMP CDump::SetTeletextPid(int pid)
{
	Log(TEXT("SetTeletextPid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetTeletextPid(pid);
}
STDMETHODIMP CDump::SetSubtitlePid(int pid)
{
	Log(TEXT("SetSubtitlePid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetSubtitlePid(pid);
}
STDMETHODIMP CDump::SetPMTPid(int pid)
{
	Log(TEXT("SetPMTPid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetPMTPid(pid);
}
STDMETHODIMP CDump::SetPCRPid(int pid)
{
	Log(TEXT("SetPCRPid ="),false);
	Log((__int64)pid,true);
	return m_pPin->SetPCRPid(pid);
}

STDMETHODIMP CDump::ResetPids()
{
	Log(TEXT("Reset Pids"),true);
	m_pFilter->Stop();
	LONG val;
	m_pPin->ResetPids();
	currentPosition=0;
	SetFilePointer(m_hFile,0,&val,FILE_BEGIN);
	SetEndOfFile(m_hFile);
	OpenFile();
	m_pFilter->Run(0);
	return S_OK;
}
//
// OpenFile
//
// Opens the file ready for dumping
//
HRESULT CDump::OpenFile()
{
    TCHAR *pFileName = NULL;

    // Is the file already opened
    if (m_hFile != INVALID_HANDLE_VALUE) {
        return NOERROR;
    }

    // Has a filename been set yet
    if (m_pFileName == NULL) {
        return ERROR_INVALID_NAME;
    }

    // Convert the UNICODE filename if necessary

#if defined(WIN32) && !defined(UNICODE)
    char convert[MAX_PATH];

    if(!WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,convert,MAX_PATH,0,0))
        return ERROR_INVALID_NAME;

    pFileName = convert;
#else
    pFileName = m_pFileName;
#endif

    // Try to open the file
    m_hFile = CreateFile((LPCTSTR) pFileName,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);

    if (m_hFile == INVALID_HANDLE_VALUE) 
    {
        DWORD dwErr = GetLastError();
        return HRESULT_FROM_WIN32(dwErr);
    }
	
	TCHAR logFile[512];
	strcpy(logFile, pFileName);
	strcat(logFile,".log");
#if DEBUG
	m_logFileHandle=CreateFile((LPCTSTR)logFile,GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);
	logFilePos=0;
#endif

	TCHAR infoFile[512];
	strcpy(infoFile, pFileName);
	strcat(infoFile, ".info");

    m_hInfoFile = CreateFile((LPCTSTR) infoFile,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);

	DWORD written = 0;
	currentPosition=0;
	LARGE_INTEGER li;
	li.QuadPart = 0;
	LockFile(m_hFile,0,0,8,0);
	SetFilePointer(m_hInfoFile,li.LowPart,&li.HighPart,FILE_BEGIN);
	WriteFile(m_hInfoFile, &currentPosition, sizeof(currentPosition), &written, NULL);
	UnlockFile(m_hInfoFile,0,0,8,0);
    return S_OK;

} // Open


//
// CloseFile
//
// Closes any dump file we have opened
//
HRESULT CDump::CloseFile()
{
    // Must lock this section to prevent problems related to
    // closing the file while still receiving data in Receive()
    CAutoLock lock(&m_Lock);

    if (m_hFile == INVALID_HANDLE_VALUE)
	{
        return NOERROR;
    }

	Log(TEXT("CloseFile called"),true);
	LARGE_INTEGER li;
	li.QuadPart = currentPosition;

	SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);

	SetEndOfFile(m_hFile);

	currentPosition = 0;
	currentFileLength = 0;
    CloseHandle(m_hFile);

    m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file 

    if (m_hInfoFile != INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_hInfoFile);
		m_hInfoFile = INVALID_HANDLE_VALUE;
	}


	TCHAR *pFileName = NULL;
#if defined(WIN32) && !defined(UNICODE)
    char convert[MAX_PATH];

    if(!WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,convert,MAX_PATH,0,0))
        return ERROR_INVALID_NAME;

    pFileName = convert;
#else
    pFileName = m_pFileName;
#endif

	TCHAR infoFile[512];
	strcpy(infoFile, pFileName);
	strcat(infoFile, ".info");

	DeleteFile(infoFile);

    return NOERROR;

} // Open

// Write
//
// Write raw data to the file
//
HRESULT CDump::Write(PBYTE pbData, LONG lDataLength)
{
    // If the file has already been closed, don't continue
	Log(TEXT("write pointer="),false);
	Log((__int64)pbData,false);
	Log(TEXT(", len="),false);
	Log(lDataLength,false);
	Log(TEXT(", current position="),false);
	Log(currentPosition,true);

    if (m_hFile == INVALID_HANDLE_VALUE)
	{
        Log(TEXT("Write: m_hFile is invalid"),true);
		return S_FALSE;
    }
	HRESULT hr = S_OK;
	DWORD written = 0;
	LARGE_INTEGER li,listart;
	li.QuadPart = currentPosition;
	listart.QuadPart = currentPosition;

	LockFile(m_hFile,listart.LowPart,listart.HighPart,188,0);
	SetFilePointer(m_hFile,li.LowPart,&li.HighPart,FILE_BEGIN);
	WriteFile(m_hFile, pbData, lDataLength, &written, NULL);
	UnlockFile(m_hFile,listart.LowPart,listart.HighPart,188,0);
	currentPosition+=written;

	if (currentPosition > MAX_FILE_LENGTH)
	{
		currentPosition=0;
	}

	return S_OK;
}

HRESULT CDump::UpdateInfoFile()
{
	if (m_hInfoFile==INVALID_HANDLE_VALUE) return S_OK;
	//update the info file
	LARGE_INTEGER li;
	DWORD written = 0;
	li.QuadPart = 0;
	//LockFile(m_hInfoFile,0,0,8+8*sizeof(int),0);
	SetFilePointer(m_hInfoFile,li.LowPart,&li.HighPart,FILE_BEGIN);
	WriteFile(m_hInfoFile, &currentPosition, sizeof(currentPosition), &written, NULL);
	int pid=m_pPin->GetAC3Pid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetAudioPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetAudioPid2();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetVideoPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetTeletextPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetPMTPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetSubtitlePid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	pid=m_pPin->GetPCRPid();WriteFile(m_hInfoFile, &pid, sizeof(pid), &written, NULL);
	
	//UnlockFile(m_hInfoFile,0,0,8+8*sizeof(int),0);
	return S_OK;
}


HRESULT CDump::HandleWriteFailure(void)
{
    DWORD dwErr = GetLastError();

    if (dwErr == ERROR_DISK_FULL)
    {
        // Close the dump file and stop the filter, 
        // which will prevent further write attempts
        Log(TEXT("error_disk_full happend"),true);
		m_pFilter->Stop();

        // Set a global flag to prevent accidental deletion of the dump file
        m_fWriteError = TRUE;

        // Display a message box to inform the developer of the write failure
    }

    return HRESULT_FROM_WIN32(dwErr);
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
STDMETHODIMP CDump::Log(__int64 value,bool crlf)
{
	char buffer[100];
	return Log(_i64toa(value,buffer,10),crlf);
}
STDMETHODIMP CDump::Log(char* text,bool crlf)
{
	CAutoLock lock(&m_Lock);
	if(m_logFileHandle==INVALID_HANDLE_VALUE)
		return S_FALSE;

	char _crlf[2];
	_crlf[0]=(char)13;
	_crlf[1]=(char)10;

	DWORD written=0;
	DWORD len=strlen(text);
	LARGE_INTEGER li;
	li.QuadPart = (LONGLONG)logFilePos;
	SetFilePointer(m_logFileHandle,li.LowPart,&li.HighPart,FILE_BEGIN);
	WriteFile(m_logFileHandle, text, len, &written, NULL);
	logFilePos+=(__int64)written;
	if(crlf)
	{
		written=0;
		WriteFile(m_logFileHandle, _crlf, 2, &written, NULL);
		logFilePos+=(__int64)written;
	}
	return S_OK;
}
