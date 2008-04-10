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
	m_pAudioPin(NULL),
	m_pVideoPin(NULL)
{

	ASSERT(phr);

	LogDebug("--------");
	m_pFileReader = new FileReader();
	m_pSections = new Sections(m_pFileReader);
	m_pDemux = new SplitterSetup(m_pSections);

	m_pVideoPin = new CFilterVideoPin(GetOwner(), this, m_pFileReader, m_pSections, phr);
	if (m_pVideoPin == NULL) {
		*phr = E_OUTOFMEMORY;
		return;
	}

	m_pAudioPin = new CFilterAudioPin(GetOwner(), this, m_pSections, phr);
	if (m_pAudioPin == NULL) {
		*phr = E_OUTOFMEMORY;
		return;
	}



}

CMPTSFilter::~CMPTSFilter()
{
	//HRESULT hr=m_pPin->Disconnect();

	//delete m_pPin;
	delete m_pAudioPin;
	delete m_pVideoPin;
	delete m_pDemux;
	delete m_pSections;
	delete m_pFileReader;
	

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
		return m_pVideoPin->NonDelegatingQueryInterface(riid, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

CBasePin * CMPTSFilter::GetPin(int n)
{
	//if (n == 0) 
	//	return m_pPin;
	if (n == 0) 
		return (CBasePin*)m_pAudioPin;
	if (n == 1) 
		return (CBasePin*)m_pVideoPin;

	return NULL;
}

int CMPTSFilter::GetPinCount()
{
	return 2;
}

STDMETHODIMP CMPTSFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("filter:Run(%x)", (DWORD)tStart);
	CAutoLock cObjectLock(m_pLock);
	HRESULT hr;
	hr=CSource::Run(tStart);
	return hr;
}

REFERENCE_TIME CMPTSFilter::StreamStartTime()
{
	return m_tStart;
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
		position=(fileSize/36560522000)*seek;
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
		return S_FALSE;
	}

	if(position<1)
		position=0;

	if(position>0) position=(position/188)*188;

	LogDebug("filter: Seek to pos:%x",position);
	m_pVideoPin->ResetBuffers(position);
	return S_OK;
}

HRESULT CMPTSFilter::Pause()
{
	LogDebug("Filter: Pause()");
	CAutoLock cObjectLock(m_pLock);
	HRESULT hr= CSource::Pause();
	return S_OK;

}

STDMETHODIMP CMPTSFilter::Stop()
{
	LogDebug("Filter: Stop()");
	CAutoLock cObjectLock(m_pLock);
	CAutoLock lock(&m_Lock);
	//m_pPin->AboutToStop();
	return CSource::Stop();
}

HRESULT CMPTSFilter::OnConnect()
{
	LogDebug("filter::OnConnect");
	HRESULT hr=m_pDemux->SetDemuxPins(GetFilterGraph());
	if (SUCCEEDED(hr))
		LogDebug("filter::OnConnect ok");
	else
		LogDebug("filter::OnConnect failed:%x",hr);
	//SetSyncClock();
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


	if (m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
	{
		LogDebug("using .info file");
		__int64	fileSize = 0;
		DWORD count=0;


		while(true)
		{
			m_pFileReader->GetFileSize(&fileSize);
			if(fileSize>0||count>=50)
				break;
			Sleep(100);
			count++;
		}
		count=0;
		while(true)
		{
			m_pFileReader->GetFileSize(m_pFileReader->m_hInfoFile,&fileSize);
			if(fileSize>8)
			{
				RefreshPids();
				if (m_pSections->pids.StartPTS>0 && m_pSections->pids.EndPTS>0)
					break;
			}
			Sleep(100);
			count++;
			if (count >50)
			{
				LogDebug("failed:unable to get start/end pts from live.info");
				break;
			}
		}
		m_pFileReader->GetFileSize(&fileSize);
		LogDebug("filesize:%d", (DWORD)fileSize);
	}
	//If this a file start then return null.
	RefreshPids();

	RefreshDuration();
	Sections::PTSTime time;
	m_pSections->PTSToPTSTime(m_pSections->pids.DurTime,&time);
	LogDebug("pids ac3:%x audio1:%x audio2:%x audio3:%x video:%x pmt:%x pcr:%x",
		m_pSections->pids.AC3,m_pSections->pids.AudioPid1,m_pSections->pids.AudioPid2,m_pSections->pids.AudioPid3,m_pSections->pids.VideoPid,m_pSections->pids.PMTPid,m_pSections->pids.PCRPid);
	LogDebug("pes start:%x pes end:%x duration:%02.2d:%02.2d:%02.2d writepos:%x",
				(DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,
				time.h,time.m,time.s, (DWORD)m_pSections->pids.fileStartPosition);
	CAutoLock lock(&m_Lock);
	m_pFileReader->CloseFile();

	return hr;
}


bool CMPTSFilter::UpdatePids()
{
	if (m_pFileReader==NULL) 
	{
		LogDebug("UpdatePids() filereader=null");
		return false;
	}
	if (m_pSections==NULL) 
	{
		LogDebug("UpdatePids() sections=null");
		return false;
	}
	if (m_pFileReader->m_hInfoFile==INVALID_HANDLE_VALUE) 
	{
		LogDebug("UpdatePids() info file not opened");
		return false;
	}
	DWORD dwReadBytes;
	LARGE_INTEGER li,writepos;
	li.QuadPart = 0;
	int ttxPid,subtitlePid,videopid,audiopid,audiopid2,audiopid3=0,ac3pid,pmtpid,pcrpid;
	DWORD dwPos=::SetFilePointer(m_pFileReader->m_hInfoFile, li.LowPart, &li.HighPart, FILE_BEGIN);
	if (dwPos != 0)
	{
		LogDebug("UpdatePids:SetFilePointer failed:%d", GetLastError());
		return false;
	}

	ULONGLONG ptsStart;
	ULONGLONG ptsNow;
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&writepos, 8, &dwReadBytes, NULL)) return false;
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ptsStart, sizeof(ptsStart), &dwReadBytes, NULL)) return false;
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ptsNow, sizeof(ptsNow), &dwReadBytes, NULL)) return false;

	if (ptsStart != m_pSections->pids.StartPTS)
	{
		//LogDebug("start pts changed from %x->%x to new start pts:%x", (DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,(DWORD)ptsStart);
	}
	if (ptsNow < m_pSections->pids.EndPTS)
	{
		//LogDebug("start pts wrapped from %x->%x to new pts:%x - %x", 
//			(DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,(DWORD)ptsStart, (DWORD)ptsNow);
	}

	if (ptsStart>ptsNow) ptsStart=1;
	m_pSections->pids.StartPTS=(__int64)ptsStart;
	m_pSections->pids.EndPTS=(__int64)ptsNow;
	Sections::PTSTime time;
	m_pSections->pids.Duration=(ptsNow-ptsStart);
	m_pSections->pids.DurTime=m_pSections->pids.Duration;
	m_pSections->PTSToPTSTime(m_pSections->pids.Duration,&time);
	m_pSections->pids.Duration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);

	if (ptsNow==ptsStart)
		m_pSections->pids.Duration=10000000;
	m_writePos=writepos.QuadPart;
	m_pSections->pids.fileStartPosition=writepos.QuadPart;


	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ac3pid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&audiopid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&audiopid2, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&videopid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ttxPid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pmtpid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&subtitlePid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (!::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pcrpid, sizeof(int), &dwReadBytes, NULL)) return false;
	if (dwReadBytes!=sizeof(int))
	{
		LogDebug("UpdatePids:readfile failed:%d", GetLastError());
		return false;
	}
	if (pcrpid==0) pcrpid=videopid;
	if (ptsStart==0) return false;

	if (ac3pid	 !=m_pSections->pids.AC3 ||
		audiopid !=m_pSections->pids.AudioPid1 ||
		audiopid2!=m_pSections->pids.AudioPid2 ||
		audiopid3!=m_pSections->pids.AudioPid3 ||
		videopid !=m_pSections->pids.VideoPid ||
		pmtpid   !=m_pSections->pids.PMTPid ||
		pcrpid   !=m_pSections->pids.PCRPid)
	{
		LogDebug("filter: PIDS changed");
		LogDebug("got pids ac3:%x audio:%x audio2:%x audio3:%x video:%x pmt:%x pcr:%x ptso:%x-%x",
			ac3pid,audiopid,audiopid2,audiopid3,videopid,pmtpid,pcrpid, (DWORD)ptsStart,(DWORD)ptsNow);
		m_pSections->pids.AC3=ac3pid;
		m_pSections->pids.AudioPid1=audiopid;
		m_pSections->pids.AudioPid2=audiopid2;
		m_pSections->pids.AudioPid3=audiopid3;
		m_pSections->pids.VideoPid=videopid;
		m_pSections->pids.PMTPid=pmtpid;
		m_pSections->pids.PCRPid=pcrpid;


		LogDebug("filter: reset buffers");
		m_pVideoPin->ResetBuffers(0);
		//setup demuxer?
		
		LogDebug("filter: setup demuxer");
		m_pDemux->SetupPids();
		LogDebug("filter: reconfigured");
		return true;
	}
	return false;
}

HRESULT CMPTSFilter::RefreshPids()
{
	CAutoLock lock(&m_Lock);
	m_pSections->ParseFromFile();
	return S_OK;
}

HRESULT CMPTSFilter::RefreshDuration()
{
	return S_OK;//return m_pPin->SetDuration(m_pSections->pids.Duration);
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


//audio stream selection
STDMETHODIMP CMPTSFilter::SetCurrentAudioPid(int audioPid)
{
	if (m_pSections->pids.CurrentAudioPid==audioPid) return S_OK;
	m_pSections->pids.CurrentAudioPid=audioPid;
	m_pDemux->SetupPids();
	return S_OK;
}

STDMETHODIMP CMPTSFilter::GetCurrentAudioPid(int* audioPid)
{
	*audioPid=m_pSections->pids.CurrentAudioPid;
	return S_OK;
}

STDMETHODIMP CMPTSFilter::GetAudioPid(int index,int* audioPid, BOOL* isAC3, char** language)
{
	*language=0;
	*isAC3=FALSE;
	*audioPid=0;
	*language="";
	switch (index)
	{
		case 0:
			*audioPid=m_pSections->pids.AudioPid1;
			*language=(char*)m_pSections->pids.AudioLanguage1.c_str();
		break;
		case 1:
			*audioPid=m_pSections->pids.AudioPid2;
			*language=(char*)m_pSections->pids.AudioLanguage2.c_str();
		break;
		case 2:
			*audioPid=m_pSections->pids.AudioPid3;
			*language=(char*)m_pSections->pids.AudioLanguage3.c_str();
		break;
		case 3:
			*audioPid=m_pSections->pids.AC3;
			*language=(char*)m_pSections->pids.AC3Language.c_str();
			*isAC3=TRUE;
		break;
	}
	return S_OK;
}

