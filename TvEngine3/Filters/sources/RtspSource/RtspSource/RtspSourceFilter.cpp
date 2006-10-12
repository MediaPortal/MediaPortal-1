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
#include "RtspSourceFilter.h"
#include "outputpin.h"
#include "ChannelInfo.h"
extern void Log(const char *fmt, ...) ;

#define BUFFER_BEFORE_PLAY_SIZE (1024L*200) //200Kb

const AMOVIESETUP_MEDIATYPE acceptOutputPinTypes =
{
	&MEDIATYPE_Stream,                  // major type
	&MEDIASUBTYPE_MPEG2_PROGRAM      // minor type
};

const AMOVIESETUP_PIN outputPin[] =
{
	{L"Out",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptOutputPinTypes},
};

const AMOVIESETUP_FILTER RtspReader =
{
	&CLSID_RtspSource,L"MediaPortal RTSP Source filter",MERIT_DO_NOT_USE,1,outputPin
};

CFactoryTemplate g_Templates[] =
{
	{L"MediaPortal RTSP Source filter",&CLSID_RtspSource,CRtspSourceFilter::CreateInstance,NULL,&RtspReader},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);




CUnknown * WINAPI CRtspSourceFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);
	CRtspSourceFilter *pNewObject = new CRtspSourceFilter(punk, phr);

	if (pNewObject == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
	}
	return pNewObject;
}

CRtspSourceFilter::CRtspSourceFilter(IUnknown *pUnk, HRESULT *phr) 
:	CSource(NAME("CRtspSource"), pUnk, CLSID_RtspSource)
,m_client(m_buffer)
,m_FilterRefList(NAME("MyFilterRefList"))
{
	wcscpy(m_fileName,L"");
  m_pOutputPin = new COutputPin(GetOwner(), this, phr, &m_section);
	m_pDemux = new Demux(&m_pids, this, &m_FilterRefList);
	m_rtStartFrom=0;
}

CRtspSourceFilter::~CRtspSourceFilter(void)
{
	m_pOutputPin->Disconnect();
	delete m_pOutputPin;
  delete m_pDemux;
}

STDMETHODIMP CRtspSourceFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
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
		return m_pOutputPin->NonDelegatingQueryInterface(riid, ppv);
	}

	return CSource::NonDelegatingQueryInterface(riid, ppv);

}

CBasePin * CRtspSourceFilter::GetPin(int n)
{
    if (n == 0) 
		{
			return m_pOutputPin;
    } 
		else 
		{
        return NULL;
    }
}


int CRtspSourceFilter::GetPinCount()
{
    return 1;
}

void CRtspSourceFilter::ResetStreamTime()
{
	CRefTime cTime;
	StreamTime(cTime);
	m_tStart = REFERENCE_TIME(m_tStart) + REFERENCE_TIME(cTime);
}

HRESULT CRtspSourceFilter::OnConnect()
{	
	Log("Filter:OnConnect, find pat/pmt...");
  m_buffer.SetCallback(this);
  m_patParser.SkipPacketsAtStart(500);
  m_patParser.Reset();
  if (m_client.Play(0.0f))
  {
		Log("Filter:OnConnect, wait for pat/pmt...");
    while (m_patParser.Count()==0)
    {
      Sleep(10);
    }
		Log("Filter:OnConnect, got pat/pmt...");
    CChannelInfo info;
    m_patParser.GetChannel(0,info);
		
    CPidTable pids=info.PidTable;
    m_pids.aud=pids.AudioPid1;
    m_pids.aud2=pids.AudioPid2;
    m_pids.ac3=pids.AC3Pid;
		if ( pids.videoServiceType==0x1b)
		{
			m_pids.h264=pids.VideoPid;
		}
		else if (pids.videoServiceType==0x10)
		{
			m_pids.mpeg4=pids.VideoPid;
		}
		else
		{
			m_pids.vid=pids.VideoPid;
		}
    m_pids.pcr=pids.PcrPid;
    m_pids.pmt=pids.PmtPid;
		Log("Filter:OnConnect, audio1:%x audio2:%x ac3:%x video:%x pcr:%x pmt:%x",
					pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid);
  }
  else
  {
		Log("Filter:OnConnect, failed to play stream...");
    return E_FAIL;
  }
	
	Log("Filter:setup demuxer...");
  m_client.Stop();
  m_pDemux->set_ClockMode(1);
  m_pDemux->set_Auto(TRUE);
  m_pDemux->set_FixedAspectRatio(TRUE);
  m_pDemux->set_MPEG2Audio2Mode(TRUE);
  m_pDemux->AOnConnect();
  m_pDemux->SetRefClock();
	Log("Filter:connect done...");
  return S_OK;
}

STDMETHODIMP CRtspSourceFilter::Run(REFERENCE_TIME tStart)
{
	Log("Filter:run()");
	float milliSecs=m_rtStartFrom.Millisecs();
	milliSecs/=1000.0f;

	m_buffer.Clear();
	Log("Filter:play stream() from %f",milliSecs);
  if (m_client.Play(milliSecs))
	{
		Log("Filter:buffer...");
		m_pOutputPin->UpdateStopStart();
		m_client.FillBuffer( BUFFER_BEFORE_PLAY_SIZE);
		Log("Filter:playing...");
	}
	else 
	{	
		Log("Filter:failed to play stream()");
		return E_FAIL;
	}
	m_client.Run();
	return CSource::Run(tStart);
}

STDMETHODIMP CRtspSourceFilter::Stop()
{
	Log("Filter:stop playing...");
	HRESULT hr=CSource::Stop();
  m_client.Stop();
  m_buffer.Clear();
	return hr;
}

STDMETHODIMP CRtspSourceFilter::Pause()
{
	Log("Filter:pause playing...");
	m_client.Pause();
  return CSource::Pause();
}

BOOL CRtspSourceFilter::is_Active(void)
{
	return ((m_State == State_Paused) || (m_State == State_Running));
}

void CRtspSourceFilter::GetStartStop(CRefTime &m_rtStart,CRefTime  &m_rtStop)
{
	m_rtStop= CRefTime(m_client.Duration());
}

void CRtspSourceFilter::Seek(CRefTime start)
{
	m_rtStartFrom=start;
}

STDMETHODIMP CRtspSourceFilter::GetDuration(REFERENCE_TIME *dur)
{
	CRefTime reftime(m_client.Duration());
	if(!dur)
		return E_INVALIDARG;
	return NOERROR;
}

STDMETHODIMP CRtspSourceFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
	Log("------------------");
	wcscpy(m_fileName,pszFileName);
	if (wcsstr(m_fileName,L"rtsp://")==NULL)
	{
		Log("Filter:using defailt filename");
		wcscpy(m_fileName,L"rtsp://pcebeckers/stream1");
	}

	Log("Filter:Initialize");
  if (m_client.Initialize())
  {
	  char url[MAX_PATH];
	  WideCharToMultiByte(CP_ACP,0,m_fileName,-1,url,MAX_PATH,0,0);
		Log("Filter:open stream:%s",url);
    if (m_client.OpenStream(url))
    {
			Log("Filter:stream length :%d msec",m_client.Duration());
			m_pOutputPin->UpdateStopStart();

    }
		else 
		{
			Log("Filter:failed to open stream");
			return E_FAIL;
		}
  }
	else 
	{
		Log("Filter:failed to open initialize client");
		return E_FAIL;
	}
	Log("Filter:load done");
	return S_OK;
}
STDMETHODIMP CRtspSourceFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
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
    pmt->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;
	}
	return S_OK;
}
ULONG CRtspSourceFilter::GetMiscFlags()
{
	return AM_FILTER_MISC_FLAGS_IS_SOURCE;
}

LONG CRtspSourceFilter::GetData(BYTE* pData, long size)
{
	if (!m_client.IsRunning()) return 0;
  DWORD bytesRead= m_buffer.ReadFromBuffer(pData, size, 0);
  return bytesRead;
}


void CRtspSourceFilter::OnTsPacket(byte* tsPacket)
{
  m_patParser.OnTsPacket(tsPacket);
}

void CRtspSourceFilter::OnRawDataReceived(BYTE *pbData, long lDataLength)
{
  if (m_State == State_Running) return;
	OnRawData(pbData, lDataLength);
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

