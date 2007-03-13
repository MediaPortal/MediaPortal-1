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

#define BUFFER_BEFORE_PLAY_SIZE (1024L*100) //100Kb

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
	m_bPaused=false;
	m_bReconfigureDemux=false;
  m_bSeek=false;
	StartThread();
}

CRtspSourceFilter::~CRtspSourceFilter(void)
{
	StopThread(100);
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

void CRtspSourceFilter::ThreadProc()
{
	long m_tickUpdateCount=GetTickCount();
	while (IsRunning())
	{
		if (m_bReconfigureDemux)
		{
			Log("Reconfigure mux");
			m_pDemux->AOnConnect();
			m_pDemux->SetRefClock();
			m_patParser.SetCallBack(this);
			m_buffer.Clear();
			m_bReconfigureDemux=false;
		}
		long ticks=GetTickCount()-m_tickUpdateCount;
    if (ticks>1000 && IsClientRunning())
	  {
      if (m_State == State_Running)
      {
		    ticks=GetTickCount()-m_tickCount;
			  CRefTime duration= CRefTime(m_client.Duration());
        CRefTime refAdd(ticks);
        duration+=refAdd;
			  m_pOutputPin->SetDuration( duration);
        NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
        m_tickUpdateCount=GetTickCount();
      }
	  }
		Sleep(10);
	}
}
void CRtspSourceFilter::OnNewChannel(CChannelInfo& info)
{
	Log("Filter::  OnNewChannel()");
  CPidTable pids=info.PidTable;
	if (  m_pids.aud==pids.AudioPid1 &&
				m_pids.aud2==pids.AudioPid2 &&
				m_pids.ac3==pids.AC3Pid &&
				m_pids.pcr==pids.PcrPid &&
				m_pids.pmt==pids.PmtPid &&
				m_pids.sub==pids.SubtitlePid)
	{
		if ( pids.videoServiceType==0x1b && m_pids.h264==pids.VideoPid) return;
		if ( pids.videoServiceType==0x10 && m_pids.mpeg4==pids.VideoPid) return;
		if ( m_pids.vid==pids.VideoPid) return;
	}


  m_pids.Clear();
  m_pids.aud=pids.AudioPid1;
  m_pids.aud2=pids.AudioPid2;
  m_pids.ac3=pids.AC3Pid;
  m_pids.pcr=pids.PcrPid;
  m_pids.pmt=pids.PmtPid;
	m_pids.sub=pids.SubtitlePid;
	if ( pids.videoServiceType==0x1b)
	{
		Log("Filter:  OnNewChannel, audio1:%x audio2:%x ac3:%x h264 video:%x pcr:%x pmt:%x sub:%x",
			pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid,pids.SubtitlePid);
		m_pids.h264=pids.VideoPid;
	}
	else if (pids.videoServiceType==0x10)
	{
	  Log("Filter:  OnNewChannel, audio1:%x audio2:%x ac3:%x mpeg4 video:%x pcr:%x pmt:%x sub:%x",
				  pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid,pids.SubtitlePid);
		m_pids.mpeg4=pids.VideoPid;
	}
	else
	{
	  Log("Filter:  OnNewChannel, audio1:%x audio2:%x ac3:%x mpeg2 video:%x pcr:%x pmt:%x",
				  pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid);
		m_pids.vid=pids.VideoPid;
	}
	m_bReconfigureDemux=true;
}

HRESULT CRtspSourceFilter::OnConnect()
{	
	m_bReconfigureDemux=false;
	Log("Filter:  OnConnect, find pat/pmt...");
  m_buffer.SetCallback(this);
  m_buffer.Run(true);
  m_patParser.SkipPacketsAtStart(0);
  m_patParser.Reset();
	m_patParser.SetCallBack(NULL);
  float milliSecs=0;

  if (m_client.Play(milliSecs))
  {
		Log("Filter:  OnConnect, wait for pat/pmt...");
    DWORD tickStart=GetTickCount();
    while (m_patParser.Count()==0)
    {
      Sleep(10);
      DWORD elapsed=GetTickCount()-tickStart;
      if (elapsed>10000)
      {
		    Log("Filter:  OnConnect, no pat/pmt received in 10 secs...");
        m_client.Stop();
        return E_FAIL;
      }
    }
		Log("Filter:  OnConnect, got pat/pmt...");
    CChannelInfo info;
    for (int i=0; i < m_patParser.Count();++i)
    {
      if (m_patParser.GetChannel(i,info))
      {
        break;
      }
    }
		
    CPidTable pids=info.PidTable;
    m_pids.Clear();
    m_pids.aud=pids.AudioPid1;
    m_pids.aud2=pids.AudioPid2;
    m_pids.ac3=pids.AC3Pid;
    m_pids.pcr=pids.PcrPid;
    m_pids.pmt=pids.PmtPid;
		m_pids.sub=pids.SubtitlePid;
		if ( pids.videoServiceType==0x1b)
		{
			Log("Filter:  OnConnect, audio1:%x audio2:%x ac3:%x h264 video:%x pcr:%x pmt:%x sub:%x",
				pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid,pids.SubtitlePid);
			m_pids.h264=pids.VideoPid;
		}
		else if (pids.videoServiceType==0x10)
		{
		  Log("Filter:  OnConnect, audio1:%x audio2:%x ac3:%x mpeg4 video:%x pcr:%x pmt:%x sub:%x",
					  pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid,pids.SubtitlePid);
			m_pids.mpeg4=pids.VideoPid;
		}
		else
		{
		  Log("Filter:  OnConnect, audio1:%x audio2:%x ac3:%x mpeg2 video:%x pcr:%x pmt:%x",
					  pids.AudioPid1,pids.AudioPid2,pids.AC3Pid,pids.VideoPid,pids.PcrPid,pids.PmtPid);
			m_pids.vid=pids.VideoPid;
		}
  }
  else
  {
		Log("Filter:  OnConnect, failed to play stream...");
    return E_FAIL;
  }
	
	Log("Filter:  setup pause client...");
  m_buffer.Run(false);
  m_client.Stop();
	Log("Filter:  setup demuxer...");
  m_bPaused=false;
  m_pDemux->set_ClockMode(3);
  m_pDemux->set_Auto(TRUE);
  m_pDemux->set_FixedAspectRatio(TRUE);
	m_pDemux->set_CreateSubPinOnDemux(m_pids.sub!=0);
  m_pDemux->set_MPEG2Audio2Mode(FALSE);
	m_pDemux->set_MPEG2AudioMediaType(FALSE);
	m_pDemux->set_AC3Mode(FALSE);
  m_pDemux->AOnConnect();
  m_pDemux->SetRefClock();
	m_patParser.SetCallBack(this);
	Log("Filter:  connect done...");
  return S_OK;
}

STDMETHODIMP CRtspSourceFilter::Run(REFERENCE_TIME tStart)
{
	Log("Filter:run()");
  if (m_bSeek)
  {
    m_bPaused=false;
    m_buffer.Run(false);
    m_client.Stop();
    m_bSeek=false;
  }
	if (m_bPaused)
	{
		Log("Filter:  client continue()");
		m_client.Continue();
		m_bPaused=false;
		return CSource::Run(tStart);
	}

	Log("Filter:  client start()");
	m_pDemux->SetRefClock();
	if (m_bReconfigureDemux==false)
	{
		float milliSecs=m_rtStartFrom.Millisecs();
		milliSecs/=1000.0f;

		m_buffer.Clear();
		m_buffer.Run(true);
		Log("Filter:  play stream() from %f",milliSecs);
		if (m_client.Play(milliSecs))
		{
			Log("Filter:  buffer...");
			CRefTime reftime(m_client.Duration());
			m_pOutputPin->SetDuration(reftime);
			m_tickCount=GetTickCount();
			//m_client.FillBuffer( BUFFER_BEFORE_PLAY_SIZE);
			Log("Filter:  playing...");
		}
		else 
		{	
			Log("Filter:  failed to play stream()");
			m_buffer.Run(false);
			return E_FAIL;
		}
		m_client.Run();
	}
	
	return CSource::Run(tStart);
}

STDMETHODIMP CRtspSourceFilter::Stop()
{
	Log("Filter:stop()");
	if (m_bReconfigureDemux==false)
	{
		m_buffer.Run(false);
		Log("Filter:  stop client...");
		m_client.Stop();
	}
	Log("Filter:  stop playing...");
	HRESULT hr=CSource::Stop();
	if (m_bReconfigureDemux==false)
	{
		Log("Filter:  clear buffer...");
		m_buffer.Clear();
		Log("Filter:  stop done...%x",hr);
	}
	m_bPaused=false;
	return hr;
}

STDMETHODIMP CRtspSourceFilter::Pause()
{
  
	Log("Filter:pause playing...%d", m_State);
  FILTER_STATE state=m_State;
	HRESULT hr=CSource::Pause();
	Log("Filter:pause   source pause done");
  if (m_State==State_Paused && state==State_Running)
	{
    if (m_client.IsRunning())
    {
      if (!m_bPaused)
      {
		    Log("Filter:  client pause()");
		    m_client.Pause();
        Log("Filter:pause   client paused");
		    m_bPaused=true;
      }
    }
	}
  Log("Filter:pause   playing.. done %d", m_State);
  return hr;
}

BOOL CRtspSourceFilter::IsClientRunning(void)
{
  return m_client.IsRunning();
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
	Log("CRtspSourceFilter::Seek()");
	m_rtStartFrom=start;
  m_bSeek=true;
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
	
	m_bPaused=false;
	Log("------------------");
	wcscpy(m_fileName,pszFileName);
	if (wcsstr(m_fileName,L"rtsp://")==NULL)
	{
		Log("Filter:using defailt filename");
		wcscpy(m_fileName,L"rtsp://192.168.1.102/stream5.0");
	}
  if (wcsstr(m_fileName,L"stream")!=NULL)
  {
    m_pOutputPin->IsTimeShifting(true); 
  }
  else
  { 
    m_pOutputPin->IsTimeShifting(false);
  }

	Log("Filter:  Initialize");
  if (m_client.Initialize())
  {
	  char url[MAX_PATH];
	  WideCharToMultiByte(CP_ACP,0,m_fileName,-1,url,MAX_PATH,0,0);
		Log("Filter:  open stream:%s",url);
    if (m_client.OpenStream(url))
    {
			Log("Filter:  stream length :%d msec",m_client.Duration());
			CRefTime reftime(m_client.Duration());
			m_pOutputPin->SetDuration(reftime);
			m_tickCount=GetTickCount();
    }
		else 
		{
			Log("Filter:  failed to open stream");
			return E_FAIL;
		}
  }
	else 
	{
		Log("Filter:  failed to open initialize client");
		return E_FAIL;
	}
	Log("Filter:  load done");
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
	if (m_bReconfigureDemux) return 0;
	if (!m_client.IsRunning()) return 0;
  DWORD bytesRead= m_buffer.ReadFromBuffer(pData, size);
  return bytesRead;
}
CMemoryBuffer& CRtspSourceFilter::Buffer()
{
  return m_buffer;
}

void CRtspSourceFilter::OnTsPacket(byte* tsPacket)
{
  m_patParser.OnTsPacket(tsPacket);
}

void CRtspSourceFilter::OnRawDataReceived(BYTE *pbData, long lDataLength)
{
 // if (m_State == State_Running) return;
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
