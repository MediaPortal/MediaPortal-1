/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
#include <streams.h>
#include <string.h>
#include <winnt.h>
#include "bdaiface.h"
#include "ks.h"
#include "ksmedia.h"
#include "bdamedia.h"
#include "MPTSFilter.h"

extern void LogDebug(const char *fmt, ...) ;

CUnknown * WINAPI CMPTSFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{

	ASSERT(phr);
	CMPTSFilter *pNewObject = new CMPTSFilter(punk, phr);

	if (pNewObject == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
	}

	return pNewObject;
}

// Constructor
CMPTSFilter::CMPTSFilter(IUnknown *pUnk, HRESULT *phr) :
	CSource(NAME("CMPTSFilter"), pUnk, CLSID_MPTSFilter),
	m_pPin(NULL),m_logFileHandle(NULL)
{

	ASSERT(phr);

	m_pFileReader = new FileReader();
	m_pSections = new Sections(m_pFileReader);
	m_pDemux = new SplitterSetup(m_pSections);

	m_pPin = new CFilterOutPin(GetOwner(), this, m_pFileReader, m_pSections, phr);

	if (m_pPin == NULL) {
		*phr = E_OUTOFMEMORY;
		return;
	}

}

CMPTSFilter::~CMPTSFilter()
{
	HRESULT hr=m_pPin->Disconnect();

	delete m_pPin;
	delete m_pDemux;
	delete m_pSections;
	delete m_pFileReader;
	
	if(m_logFileHandle!=INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_logFileHandle);
	}

}

STDMETHODIMP CMPTSFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	CheckPointer(ppv,E_POINTER);
	CAutoLock lock(&m_Lock);

	// Do we have this interface

	if (riid == IID_IMPTSControl)
	{
		return GetInterface((IMPTSControl*)this, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)this, ppv);
	}
	if ((riid == IID_IMediaPosition || riid == IID_IMediaSeeking))
	{
		return m_pPin->NonDelegatingQueryInterface(riid, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

CBasePin * CMPTSFilter::GetPin(int n)
{
	if (n == 0) {
		return m_pPin;
	} else {
		return NULL;
	}
}

int CMPTSFilter::GetPinCount()
{
	return 1;
}

STDMETHODIMP CMPTSFilter::SetSyncClock(void)
{
	//Log(TEXT("filter: SetSyncClock()"),true);

	HRESULT hr;
	IFilterGraph *pGraph=GetFilterGraph();
	IBaseFilter *pFilter=NULL;
	IMediaFilter *pMF=NULL;
	hr=pGraph->QueryInterface(IID_IMediaFilter,(void**)&pMF);
	hr=pGraph->FindFilterByName(L"Default DirectSound Device",&pFilter);
	if(pFilter==NULL)
	{
		Log(TEXT("filter: DSoundRender not found, try to add..."),true);
		hr=CoCreateInstance(CLSID_DSoundRender, NULL,CLSCTX_INPROC_SERVER,IID_IBaseFilter,(void**)&pFilter);
		if(SUCCEEDED(hr))
		{
			Log(TEXT("filter: Create successfull, add to graph..."),true);
			hr=pGraph->AddFilter(pFilter,L"Default DirectSound Device");
			if(SUCCEEDED(hr))
					Log(TEXT("filter: DSoundRender add ok!"),true);

		}
	}
	if(SUCCEEDED(hr) && pMF!=NULL)
	{
		Log(TEXT("filter: DSoundRender found/added ok"),true);

		if(pFilter!=NULL)
		{
			IReferenceClock *pClock=NULL;
			hr=pFilter->QueryInterface(IID_IReferenceClock,(void**)&pClock);
			if(SUCCEEDED(hr))
			{
				if(pClock!=NULL) 
				{
					Log((char*)" setting pClock = ",false);
					hr=pMF->SetSyncSource(pClock);
					Log(hr,true);
					pClock->Release();
				}
			}
		}
	}
	if(pFilter!=NULL)
		pFilter->Release();
	
	if(pMF!=NULL)
		pMF->Release();
	
	return S_OK;
}


STDMETHODIMP CMPTSFilter::Run(REFERENCE_TIME tStart)
{
	CAutoLock cObjectLock(m_pLock);
	HRESULT hr;
	//Log((char*)"filter: Run() tStart= ",false);
	//Log(tStart,true);
	if(m_pFileReader->IsFileInvalid()==true)
	{
		Log((char*)"filter: Run() invalid file handle ",true);
	}
	hr=CSource::Run(tStart);
	return hr;
}
HRESULT CMPTSFilter::SetFilePosition(REFERENCE_TIME seek)
{
	if(m_pFileReader->IsFileInvalid()==true)
		m_pFileReader->OpenFile();
	__int64 fileSize;
	m_pFileReader->GetFileSize(&fileSize);
	if(m_pSections->pids.Duration<1)
		return S_FALSE;

	__int64 position=0;
	if (seek>0)
	{
		Sections::PTSTime time;

		ULONGLONG duration=m_pSections->pids.EndPTS-m_pSections->pids.StartPTS;
		m_pSections->PTSToPTSTime(duration,&time);
		duration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);

		position=(fileSize/100LL)* ( (seek*100LL)/ duration);
	}
	
	if (m_pFileReader->m_hInfoFile != INVALID_HANDLE_VALUE)
	{
		if (fileSize>=MAX_FILE_LENGTH)
			position += m_writePos;
		if(position>fileSize)
			position -= fileSize;
	}

	if(position>fileSize || position<0)
	{
		Log((char*)"SetFilePosition() error",false);
		return S_FALSE;
	}

	if(position<1)
		position=0;

	if(position>0) position=(position/188)*188;

	LogDebug("filter: Seek to pos:%x",position);
	m_pPin->ResetBuffers(position);
	return S_OK;
}
HRESULT CMPTSFilter::Pause()
{
	LogDebug("Filter: Pause()");
	CAutoLock cObjectLock(m_pLock);
	m_setPosition=true;
	return CSource::Pause();
}

STDMETHODIMP CMPTSFilter::Stop()
{
	LogDebug("Filter: Stop()");
	m_pPin->AboutToStop();
	CAutoLock cObjectLock(m_pLock);
	CAutoLock lock(&m_Lock);
	return CSource::Stop();
}

HRESULT CMPTSFilter::OnConnect()
{
	//Log((char*)"filter: Connecting pins",true);
	HRESULT hr=m_pDemux->SetDemuxPins(GetFilterGraph());
	SetSyncClock();// try to select the clock on the audio-renderer
	return S_OK;
}

STDMETHODIMP CMPTSFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
	LogDebug("--- load file");
	HRESULT hr;
	hr = m_pFileReader->SetFileName(pszFileName);
	if (FAILED(hr))
		return hr;

	hr = m_pFileReader->OpenFile();
	if (FAILED(hr))
		return VFW_E_INVALIDMEDIATYPE;

	// log file

	TCHAR *fileName;

	#if defined(WIN32) && !defined(UNICODE)
    char convert[MAX_PATH];

    if(!WideCharToMultiByte(CP_ACP,0,pszFileName,-1,convert,MAX_PATH,0,0))
        return ERROR_INVALID_NAME;

    fileName = convert;
#else
    fileName = pszFileName;
#endif
	strcat(fileName,".log");

#ifdef DEBUG
	m_logFileHandle=CreateFile((LPCTSTR)fileName,GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS,
		NULL);
#endif
	logFilePos=0;

	if (m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
	{
		Log(TEXT("using .info:"),true);
		__int64	fileSize = 0;
		DWORD count=0;
		while(true)
		{
			m_pFileReader->GetFileSize(m_pFileReader->m_hInfoFile,&fileSize);
			if(fileSize>8)
				break;
			Sleep(80);
			count++;
		}

		while(true)
		{
			m_pFileReader->GetFileSize(&fileSize);
			if(fileSize>=200000 ||count>=50)
				break;
			Sleep(100);
			count++;
		}
	}
	//If this a file start then return null.
	RefreshPids();

	RefreshDuration();
	Log(TEXT("found audio-pid 1: "),false);
	Log((__int64)m_pSections->pids.AudioPid,true);
	Log(TEXT("found audio-pid 2: "),false);
	Log((__int64)m_pSections->pids.AudioPid2,true);
	Log(TEXT("found video-pid  : "),false);
	Log((__int64)m_pSections->pids.VideoPid,true);
	Log(TEXT("found pmt pid    : "),false);
	Log((__int64)m_pSections->pids.PMTPid,true);
	Log(TEXT("found pcr pid    : "),false);
	Log((__int64)m_pSections->pids.PCRPid,true);
	Log(TEXT("found ac3 pid    : "),false);
	Log((__int64)m_pSections->pids.AC3,true);
	Log(TEXT("program number   : "),false);
	Log((__int64)m_pSections->pids.ProgramNumber,true);
	Log(TEXT("file duration    : "),false);
	Sections::PTSTime time;
	m_pSections->PTSToPTSTime(m_pSections->pids.DurTime,&time);
	Log(time.h,false);
	Log(TEXT(":"),false);
	Log(time.m,false);
	Log(TEXT(":"),false);
	Log(time.s,false);
	Log(TEXT("."),false);
	Log(time.u,true);
	LogDebug("pids ac3:%x audio:%x audio2:%x video:%x pmt:%x pcr:%x",
		m_pSections->pids.AC3,m_pSections->pids.AudioPid,m_pSections->pids.AudioPid2,m_pSections->pids.VideoPid,m_pSections->pids.PMTPid,m_pSections->pids.PCRPid);
	LogDebug("pes start:%x pes end:%x duration:%02.2d:%02.2d:%02.2d writepos:%x",
				(DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,
				time.h,time.m,time.s, (DWORD)m_pSections->pids.fileStartPosition);
	CAutoLock lock(&m_Lock);
	m_pFileReader->CloseFile();

	return hr;
}


void CMPTSFilter::UpdatePids()
{
	if (m_pFileReader==NULL) 
	{
		LogDebug("UpdatePids() filereader=null");
		return;
	}
	if (m_pSections==NULL) 
	{
		LogDebug("UpdatePids() sections=null");
		return;
	}
	if (m_pFileReader->m_hInfoFile==INVALID_HANDLE_VALUE) 
	{
		//LogDebug("UpdatePids() info file not opened");
		return;
	}
	DWORD dwReadBytes;
	LARGE_INTEGER li,writepos;
	li.QuadPart = 0;
	int ttxPid,subtitlePid,videopid,audiopid,audiopid2,ac3pid,pmtpid,pcrpid;
	DWORD dwPos=::SetFilePointer(m_pFileReader->m_hInfoFile, li.LowPart, &li.HighPart, FILE_BEGIN);
	if (dwPos != 0)
	{
		LogDebug("UpdatePids:SetFilePointer failed:%d", GetLastError());
		return;
	}

	ULONGLONG ptsStart;
	ULONGLONG ptsNow;
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&writepos, 8, &dwReadBytes, NULL)) return;
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ptsStart, sizeof(ptsStart), &dwReadBytes, NULL)) return;
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ptsNow, sizeof(ptsNow), &dwReadBytes, NULL)) return;

	m_pSections->pids.StartPTS=(__int64)ptsStart;
	m_pSections->pids.EndPTS=(__int64)ptsNow;
	Sections::PTSTime time;
	m_pSections->pids.Duration=(ptsNow-ptsStart);
	m_pSections->pids.DurTime=m_pSections->pids.Duration;
	m_pSections->PTSToPTSTime(m_pSections->pids.Duration,&time);
	m_pSections->pids.Duration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);

	m_writePos=writepos.QuadPart;
	m_pSections->pids.fileStartPosition=writepos.QuadPart;


	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ac3pid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&audiopid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&audiopid2, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&videopid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ttxPid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pmtpid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&subtitlePid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pcrpid, sizeof(int), &dwReadBytes, NULL)) return;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return;
	}
	if (pcrpid==0) pcrpid=videopid;
	if (ac3pid	 !=m_pSections->pids.AC3 ||
		audiopid !=m_pSections->pids.AudioPid ||
		audiopid2!=m_pSections->pids.AudioPid2 ||
		videopid !=m_pSections->pids.VideoPid ||
		pmtpid   !=m_pSections->pids.PMTPid ||
		pcrpid   !=m_pSections->pids.PCRPid)
	{
		LogDebug("filter: PIDS changed");
		LogDebug("got pids ac3:%x audio:%x audio2:%x video:%x pmt:%x pcr:%x",
					ac3pid,audiopid,audiopid2,videopid,pmtpid,pcrpid);
		m_pSections->pids.AC3=ac3pid;
		m_pSections->pids.AudioPid=audiopid;
		m_pSections->pids.AudioPid2=audiopid2;
		m_pSections->pids.VideoPid=videopid;
		m_pSections->pids.PMTPid=pmtpid;
		m_pSections->pids.PCRPid=pcrpid;


		LogDebug("filter: reset buffers");
		m_pPin->ResetBuffers(0);
		//setup demuxer?
		
		LogDebug("filter: setup demuxer");
		m_pDemux->SetupPids();
		LogDebug("filter: reconfigured");
	}
}

HRESULT CMPTSFilter::RefreshPids()
{
	CAutoLock lock(&m_Lock);
	m_pSections->ParseFromFile();
	return S_OK;
}

HRESULT CMPTSFilter::RefreshDuration()
{
	return m_pPin->SetDuration(m_pSections->pids.Duration);
}

STDMETHODIMP CMPTSFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
{

	CheckPointer(ppszFileName, E_POINTER);
	*ppszFileName = NULL;

	LPOLESTR pFileName = NULL;
	HRESULT hr = m_pFileReader->GetFileName(&pFileName);
	if (FAILED(hr))
		return hr;

	if (pFileName != NULL)
	{
		*ppszFileName = (LPOLESTR)
		QzTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(pFileName)));

		if (*ppszFileName != NULL)
		{
			wcscpy(*ppszFileName, pFileName);
		}
	}

	if(pmt)
	{
		ZeroMemory(pmt, sizeof(*pmt));
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;
	}

	return S_OK;

} // GetCurFile

STDMETHODIMP CMPTSFilter::GetDuration(REFERENCE_TIME *dur)
{
	if(!dur)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	*dur = m_pSections->pids.Duration;

	return NOERROR;

}

HRESULT CMPTSFilter::GetFileSize(__int64 *pfilesize)
{
	CAutoLock lock(&m_Lock);
	return m_pFileReader->GetFileSize(pfilesize);
}

STDMETHODIMP CMPTSFilter::Refresh(void)
{
	if (m_pFileReader->IsFileInvalid())
		return S_OK;
	__int64	fileSize = 0;
	DWORD count=0;
	while(true)
	{
		m_pFileReader->GetFileSize(&fileSize);
		if(fileSize>=200000 ||count>=50)
			break;
		Sleep(50);
		count++;
	}
	RefreshPids();
	return S_OK;
}
STDMETHODIMP CMPTSFilter::Log(__int64 value,bool crlf)
{
	char buffer[100];
	return Log(_i64toa(value,buffer,10),crlf);
}
STDMETHODIMP CMPTSFilter::Log(char* text,bool crlf)
{
	CAutoLock lock(&m_Lock);
#ifdef DEBUG
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
#endif
	return S_OK;
}

