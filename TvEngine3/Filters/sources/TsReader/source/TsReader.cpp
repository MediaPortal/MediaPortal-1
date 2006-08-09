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
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"

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

	FILE* fp = fopen("tsreader.log","a+");
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


const AMOVIESETUP_MEDIATYPE acceptAudioPinTypes =
{
	&MEDIATYPE_Audio,                  // major type
	&MEDIASUBTYPE_MPEG2_AUDIO      // minor type
};
const AMOVIESETUP_MEDIATYPE acceptVideoPinTypes =
{
	&MEDIATYPE_Audio,                  // major type
	&MEDIASUBTYPE_MPEG2_VIDEO      // minor type
};

const AMOVIESETUP_PIN audioVideoPin[] =
{
	{L"Audio",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptAudioPinTypes},
	{L"Video",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptVideoPinTypes}
};

const AMOVIESETUP_FILTER TSReader =
{
	&CLSID_TSReader,L"MediaPortal File Reader",MERIT_DO_NOT_USE,2,audioVideoPin
};

CFactoryTemplate g_Templates[] =
{
	{L"MediaPortal File Reader",&CLSID_TSReader,CTsReaderFilter::CreateInstance,NULL,&TSReader},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);





CUnknown * WINAPI CTsReaderFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);
	CTsReaderFilter *pNewObject = new CTsReaderFilter(punk, phr);

	if (pNewObject == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
	}
	return pNewObject;
}


// Constructor
CTsReaderFilter::CTsReaderFilter(IUnknown *pUnk, HRESULT *phr) :
	CSource(NAME("CTsReaderFilter"), pUnk, CLSID_TSReader),
	m_pAudioPin(NULL),
	m_pcrDecoder(m_fileReader),
	m_demultiplexer(m_fileReader,&m_section)
{

	m_pAudioPin = new CAudioPin(GetOwner(), this, phr,&m_section);
	m_pVideoPin = new CVideoPin(GetOwner(), this, phr,&m_section);

	if (m_pAudioPin == NULL) 
	{
		*phr = E_OUTOFMEMORY;
		return;
	}
	wcscpy(m_fileName,L"");
}

CTsReaderFilter::~CTsReaderFilter()
{
	HRESULT hr=m_pAudioPin->Disconnect();
	delete m_pAudioPin;
}

STDMETHODIMP CTsReaderFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	if (riid == IID_IAMFilterMiscFlags)
	{
		return GetInterface((IAMFilterMiscFlags*)this, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)this, ppv);
	}
	if ((riid == IID_IMediaPosition || riid == IID_IMediaSeeking))
	{
		return m_pAudioPin->NonDelegatingQueryInterface(riid, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);

}

CBasePin * CTsReaderFilter::GetPin(int n)
{
    if (n == 0) 
		{
			return m_pAudioPin;
    } 
		else  if (n==1)
		{
        return m_pVideoPin;
    } 
		else 
		{
        return NULL;
    }
}


int CTsReaderFilter::GetPinCount()
{
    return 2;
}

STDMETHODIMP CTsReaderFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CTsReaderFilter::Run()");
  CAutoLock cObjectLock(m_pLock);
	return CSource::Run(tStart);
}

STDMETHODIMP CTsReaderFilter::Stop()
{
		CAutoLock cObjectLock(m_pLock);

		LogDebug("CTsReaderFilter::Stop()");
		m_fileReader.CloseFile();
		return CSource::Stop();
}

STDMETHODIMP CTsReaderFilter::Pause()
{
	LogDebug("CTsReaderFilter::Pause()");
  CAutoLock cObjectLock(m_pLock);


  return CSource::Pause();
}

STDMETHODIMP CTsReaderFilter::GetDuration(REFERENCE_TIME *dur)
{
	if(!dur)
		return E_INVALIDARG;

	double dmilliSeconds=m_endTime-m_startTime;
	__int64 milliSeconds=(__int64)dmilliSeconds;
	*dur = MILLISECONDS_TO_100NS_UNITS(milliSeconds);

	return NOERROR;
}
STDMETHODIMP CTsReaderFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
	wcscpy(m_fileName,pszFileName);
	m_fileReader.SetFileName(m_fileName);
	m_fileReader.OpenFile();
	UpdateDuration();
	m_fileReader.SetFilePointer(0LL,FILE_BEGIN);
	return S_OK;
}


STDMETHODIMP CTsReaderFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
{
	CheckPointer(ppszFileName, E_POINTER);
	*ppszFileName = NULL;

	if (lstrlenW(m_fileName)>0)
	{
		*ppszFileName = (LPOLESTR)QzTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(m_fileName)));
		wcscpy(*ppszFileName,m_fileName);
	}
	if(pmt)
	{
		ZeroMemory(pmt, sizeof(*pmt));
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG2_PROGRAM;
	}
	return S_OK;
}


double CTsReaderFilter::UpdateDuration()
{
	// determine first PCR...
	m_fileReader.SetFilePointer(0LL,FILE_BEGIN);
	m_startTime=m_pcrDecoder.GetPcr();

	m_fileReader.SetFilePointer(-8192LL,FILE_END);
	m_endTime=m_pcrDecoder.GetPcr(true);

	m_pVideoPin->SetDuration();
	m_pAudioPin->SetDuration();
	return (m_endTime- m_startTime);
}

// IAMFilterMiscFlags

ULONG CTsReaderFilter::GetMiscFlags()
{
	return AM_FILTER_MISC_FLAGS_IS_SOURCE;
}


CDeMultiplexer& CTsReaderFilter::GetDemultiplexer()
{
	return m_demultiplexer;
}

double CTsReaderFilter::GetStartTime()
{
	return m_startTime;
}

void CTsReaderFilter::Seek(CRefTime& seekTime)
{
	double duration=(m_endTime-m_startTime);
	double seektime=(double)seekTime.Millisecs();
	double percent=seektime/duration;
	__int64 start;
	__int64 end;
	m_fileReader.GetFileSize(&start,&end);
	__int64 fileDuration=end-start;
	percent *= ((double)fileDuration);
	m_fileReader.setFilePointer((__int64)percent,FILE_BEGIN);
	m_demultiplexer.Reset();
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
