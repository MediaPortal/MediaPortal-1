/*
 *  Copyright (C) 2005-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <sbe.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "subtitlepin.h"
#include "teletextpin.h"
#include "tsfileSeek.h"
#include "memoryreader.h"
#include <cassert>

static char logFile[MAX_PATH];
static bool logFileParsed = false;

void GetLogFile(char *pLog)
{
  if(!logFileParsed)
  {
    TCHAR folder[MAX_PATH];
    ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    sprintf(logFile,"%s\\Team MediaPortal\\MediaPortal\\Log\\TsReader.log",folder);
    logFileParsed=true;
  }
  strcpy(pLog, &logFile[0]);
}


void LogDebug(const char *fmt, ...)
{
  va_list ap;
  va_start(ap,fmt);

  char buffer[1000];
  int tmp;
  va_start(ap,fmt);
  tmp=vsprintf(buffer, fmt, ap);
  va_end(ap);
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);

//#ifdef DONTLOG
  TCHAR filename[1024];
  GetLogFile(filename);
  FILE* fp = fopen(filename,"a+");

  if (fp!=NULL)
  {
    fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%x]%s\n",
      systemTime.wDay, systemTime.wMonth, systemTime.wYear,
      systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
      systemTime.wMilliseconds,
      GetCurrentThreadId(),
      buffer);
    fclose(fp);
  }
//#endif
  char buf[1000];
  sprintf(buf,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
    buffer);
  ::OutputDebugString(buf);
};


const AMOVIESETUP_MEDIATYPE acceptAudioPinTypes =
{
  &MEDIATYPE_Audio,             // major type
  &MEDIASUBTYPE_MPEG1Audio      // minor type
};
const AMOVIESETUP_MEDIATYPE acceptVideoPinTypes =
{
  &MEDIATYPE_Video,             // major type
  &MEDIASUBTYPE_MPEG2_VIDEO     // minor type
};

const AMOVIESETUP_MEDIATYPE acceptSubtitlePinTypes =
{
  &MEDIATYPE_Stream,            // major type
  &MEDIASUBTYPE_MPEG2_TRANSPORT // minor type
};

const AMOVIESETUP_PIN audioVideoPin[] =
{
  {L"Audio",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptAudioPinTypes},
  {L"Video",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptVideoPinTypes},
  {L"Subtitle",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptSubtitlePinTypes}
};

const AMOVIESETUP_FILTER TSReader =
{
  &CLSID_TSReader,L"MediaPortal File Reader",MERIT_NORMAL+1000,3,audioVideoPin
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
  m_demultiplexer( m_duration, *this),
  m_rtspClient(m_buffer),
  m_pDVBSubtitle(NULL),
  m_pCallback(NULL),
  m_pRequestAudioCallback(NULL)
{
 // use the following line if u r having trouble setting breakpoints
 // #pragma comment( lib, "strmbasd" )
  TCHAR filename[1024];
  GetLogFile(filename);
  ::DeleteFile(filename);
  LogDebug("-------------- v1.0.6 ----------------");

  m_fileReader=NULL;
  m_fileDuration=NULL;
  Compensation=CRefTime(0L);

  LogDebug("CTsReaderFilter::ctor");
  m_pAudioPin = new CAudioPin(GetOwner(), this, phr,&m_section);
  m_pVideoPin = new CVideoPin(GetOwner(), this, phr,&m_section);
  m_pSubtitlePin = new CSubtitlePin(GetOwner(), this, phr,&m_section);

  m_bSeeking=false;

  if (m_pAudioPin == NULL)
  {
    *phr = E_OUTOFMEMORY;
    return;
  }
  wcscpy(m_fileName,L"");
  m_dwGraphRegister=0;
  m_rtspClient.Initialize();
  HKEY key;
  if (ERROR_SUCCESS==RegCreateKey(HKEY_CURRENT_USER, "Software\\MediaPortal\\TsReader",&key))
  {
    RegCloseKey(key);
  }

  // Set default filtering mode (normal), if not overriden externaly (see ITSReader::SetRelaxedMode)
  m_demultiplexer.m_DisableDiscontinuitiesFiltering=false ;
  SetWaitForSeekToEof(false) ;
  m_bLiveTv = false ;
  m_RandomCompensation = 0 ;     
	m_bPauseOnly=false ;
	m_bAnalog=false ;
	m_bStopping=false;
}

CTsReaderFilter::~CTsReaderFilter()
{
  LogDebug("CTsReaderFilter::dtor");
  HRESULT hr=m_pAudioPin->Disconnect();
  delete m_pAudioPin;

  hr=m_pVideoPin->Disconnect();
  delete m_pVideoPin;

  hr=m_pSubtitlePin->Disconnect();
  delete m_pSubtitlePin;

  if (m_pDVBSubtitle)
  {
    m_pDVBSubtitle->Release();
    m_pDVBSubtitle = NULL;
  }

  if (m_fileReader!=NULL)
    delete m_fileReader;
  if (m_fileDuration!=NULL)
    delete m_fileDuration;
}

STDMETHODIMP CTsReaderFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
  if (riid == IID_IStreamBufferConfigure)
  {
    LogDebug("filt:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
    LogDebug("filt:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
    LogDebug("filt:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
    LogDebug("filt:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
    LogDebug("filt:IID_IStreamBufferDataCounters()");
  }
  if (riid == IID_IMediaSeeking)
  {
    LogDebug("filt:IID_IMediaSeeking()");
    if (m_pAudioPin->IsConnected())
      return m_pAudioPin->NonDelegatingQueryInterface(riid, ppv);

    if (m_pVideoPin->IsConnected())
      return m_pVideoPin->NonDelegatingQueryInterface(riid, ppv);
  }
  if (riid == IID_IAMFilterMiscFlags)
  {
    return GetInterface((IAMFilterMiscFlags*)this, ppv);
  }
  if (riid == IID_IFileSourceFilter)
  {
    return GetInterface((IFileSourceFilter*)this, ppv);
  }
  if (riid == IID_IAMStreamSelect)
  {
    return GetInterface((IAMStreamSelect*)this, ppv);
  }
    if (riid == IID_ITeletextSource)
  {
    LogDebug("filt:IID_ITeletextSource()");
    return GetInterface((ITeletextSource*)this, ppv);
  }
  if (riid == IID_ISubtitleStream)
  {
    LogDebug("filt:IID_ISubtitleStream()");
    HRESULT hr =  GetInterface((ISubtitleStream*)this, ppv);
    if(SUCCEEDED(hr)){
      LogDebug("SUCCESS",hr);
    }
    else{
      LogDebug("FAILED",hr);
    }
    return hr;
  }
  if ( riid == IID_ITSReader )
  {
    return GetInterface((ITSReader*)this, ppv);
  }
  if ( riid == IID_IAudioStream )
  {
    return GetInterface((IAudioStream*)this, ppv);
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
  else if (n==2)
  {
    return m_pSubtitlePin;
  }
  return NULL;
}


int CTsReaderFilter::GetPinCount()
{
  return 3;
}

void CTsReaderFilter::OnMediaTypeChanged(int mediaTypes)
{
  if ( m_pCallback ) m_pCallback->OnMediaTypeChanged(mediaTypes);
}

void CTsReaderFilter::OnRequestAudioChange()
{
  if ( m_pRequestAudioCallback ) m_pRequestAudioCallback->OnRequestAudioChange();
}

void CTsReaderFilter::OnVideoFormatChanged(int streamType,int width,int height,int aspectRatioX,int aspectRatioY,int bitrate,int isInterlaced)
{
  if ( m_pCallback )
    m_pCallback->OnVideoFormatChanged(streamType,width,height,aspectRatioX,aspectRatioY,bitrate,isInterlaced);
}

STDMETHODIMP CTsReaderFilter::SetGraphCallback(ITSReaderCallback* pCallback)
{
  LogDebug("CALLBACK SET");
  m_pCallback = pCallback;
  return S_OK;
}

STDMETHODIMP CTsReaderFilter::SetRequestAudioChangeCallback(ITSReaderAudioChange* pCallback)
{
  LogDebug("SetRequestAudioChangeCallback SET");
  m_pRequestAudioCallback = pCallback;
  return S_OK;
}
extern int ShowBuffer ;
STDMETHODIMP CTsReaderFilter::SetRelaxedMode(BOOL relaxedReading)
{
  LogDebug("SetRelaxedMode");
  if (relaxedReading == FALSE)
  {
    LogDebug("Normal discontinuities filtering");
    m_demultiplexer.m_DisableDiscontinuitiesFiltering=false ;
  }
  else
  {
    LogDebug("Relaxed discontinuities filtering");
    m_demultiplexer.m_DisableDiscontinuitiesFiltering=true ;
  }
  return S_OK;
}

void STDMETHODCALLTYPE CTsReaderFilter::OnZapping(int info)
{
  LogDebug("OnZapping %x", info);
	if (info & 0x80)							// Theorically a new PAT ( equal to PAT+1 modulo 16 ) will be issued by TsWriter.
	{
		m_demultiplexer.RequestNewPat() ;
		m_bAnalog=false ;
	}
	else
	{
		if (info <= 0)							// Analog or card assigment failure
		{
			m_demultiplexer.ClearRequestNewPat() ;
			m_bAnalog=true ;
		}
	}

  return;
}

void STDMETHODCALLTYPE CTsReaderFilter::OnGraphRebuild(int info)
{
	LogDebug("CTsReaderFilter::OnGraphRebuild %d",info);
	m_demultiplexer.SetVideoChanging(false) ;
}

STDMETHODIMP CTsReaderFilter::Run(REFERENCE_TIME tStart)
{
  CRefTime runTime=tStart;
  double msec=(double)runTime.Millisecs();
  msec/=1000.0;
  LogDebug("CTsReaderFilter::Run(%05.2f) %d",msec,m_State);

  m_RandomCompensation = 0 ;

  if (m_bPauseOnly)
  {
    m_lastRun += (GetTickCount()-m_lastPause) ;
    SetWaitForSeekToEof(false) ;
  }
  else
  {
    m_lastRun = GetTickCount() ;
    if (m_bStreamCompensated && m_bLiveTv) ;
      LogDebug("Elapsed time from pause to Audio/Video ( total zapping time ) : %d mS",GetTickCount()-m_lastPause);
  }

  ShowBuffer=40 ;

  CAutoLock cObjectLock(m_pLock);

  if(m_pSubtitlePin) m_pSubtitlePin->SetRunningStatus(true);
	
	//are we using RTSP or local file
  if (m_fileDuration==NULL)
  {
    //using RTSP, if its streaming is paused then
    //stop pausing and continue streaming
    if (m_rtspClient.IsPaused())
    {
      LogDebug(" CTsReaderFilter::Run()  -->is paused,continue rtsp");
      m_rtspClient.Continue();
      LogDebug(" CTsReaderFilter::Run()  --> rtsp running");
    }
    m_demultiplexer.SetHoldAudio(false);
    m_demultiplexer.SetHoldVideo(false);
    m_demultiplexer.SetHoldSubtitle(false);
  }

  m_demultiplexer.m_LastDataFromRtsp=GetTickCount() ;
  //Set our StreamTime Reference offset to zero
  HRESULT hr= CSource::Run(tStart);

  FindSubtitleFilter();
  LogDebug("CTsReaderFilter::Run(%05.2f)  -->done",msec);
  return hr;
}

STDMETHODIMP CTsReaderFilter::Stop()
{
  LogDebug("CTsReaderFilter::Stop()");

	m_bPauseOnly=false ;

  //guarantees that audio/video/subtitle pins dont block in the fillbuffer() method
  m_bSeeking=true;
	m_bStopping=true;

  if (m_pSubtitlePin)
  {
    m_pSubtitlePin->SetRunningStatus(false);
  }

  //stop duration thread
  StopThread();

  LogDebug("CTsReaderFilter::Stop()  -stop source");
  //stop filter
  HRESULT hr=CSource::Stop();
  LogDebug("CTsReaderFilter::Stop()  -stop source done");

  //are we using rtsp?
  if (m_fileDuration==NULL)
  {
    //yep then stop streaming
    LogDebug("CTsReaderFilter::Stop()   -- stop rtsp");
    m_buffer.Run(false);
    m_rtspClient.Stop();
  }

  //reset vaues
  m_bSeeking=false;
	m_bStopping=false;
  LogDebug("CTsReaderFilter::Stop() done %d");
  return hr;
}
bool CTsReaderFilter::IsTimeShifting()
{
  return m_bTimeShifting;
}
extern int ShowBuffer ;
STDMETHODIMP CTsReaderFilter::Pause()
{
  ShowBuffer=100 ;
  LogDebug("CTsReaderFilter::Pause() %d %d", IsTimeShifting(), m_State);
  CAutoLock cObjectLock(m_pLock);

	if (GetTickCount()-m_lastRun > 200)
	{
		if (m_State == State_Running)
		{
			m_bPauseOnly = true ;
			m_lastPause = GetTickCount() ;
			m_RandomCompensation = 0 ;
			SetWaitForSeekToEof(true) ; // && IsTimeShifting()) ;
		}
//
//		if ((m_State == State_Running) && IsTimeShifting())
//			SetWaitForNewPat(true) ;
	}
	// else .... Immediate Pause after Run with EVR that freezs MP due to SetWaitForSeekToEof(true)...Another way should be found!!

    //pause filter
  HRESULT hr=CSource::Pause();

  LogDebug("Clock : %d",GetTickCount() - m_lastRun) ;

  //are we using rtsp?
  if (m_fileDuration==NULL)
  {
    //yes, are we busy seeking?
    if (!m_bSeeking)
    {
      //not seeking, is rtsp streaming at the moment?
      if (!m_rtspClient.IsRunning())
      {
        //not streaming atm
        double startTime=m_seekTime.Millisecs();
        startTime/=1000.0f;

        //clear buffers
        LogDebug("  -- Pause()  ->start rtsp from %f",startTime);
        m_buffer.Clear();

        //start streaming
        m_buffer.Run(true);
        m_rtspClient.Play(startTime);
        m_tickCount=GetTickCount();
        LogDebug("  -- Pause()  ->rtsp started");

        //get duration of the stream
        double duration=m_rtspClient.Duration()/1000.0f;
        CPcr pcrstart,pcrEnd,pcrMax;
        pcrstart=m_duration.StartPcr();
        duration+=pcrstart.ToClock();
        pcrEnd.FromClock(duration);
        m_duration.Set( pcrstart, pcrEnd, pcrMax);

        //allow audio/video/subtitle pins to block in fillbuffer
        LogDebug("  -- Pause()-  >duration:%f",(m_rtspClient.Duration()/1000.0f));
      }
      else
      {
        //we are streaming at the moment.
        //pause the streaming
        LogDebug("  -- Pause()  ->pause rtsp");
        m_rtspClient.Pause();
      }
    }
  else //we are seeking
  {
    IMediaSeeking * ptrMediaPos;

        if (SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking , (void**)&ptrMediaPos) ) )
        {
          LONGLONG currentPos;
          ptrMediaPos->GetCurrentPosition(&currentPos);
          ptrMediaPos->Release();
          double clock =currentPos;clock /=10000000.0;
          float clockEnd=m_duration.EndPcr().ToClock() ;
          if (clock>=clockEnd && clockEnd>0 )
          {
            LogDebug("End of rtsp stream...");
            m_demultiplexer.SetEndOfFile(true);
          }
        }
  }
  }
  m_demultiplexer.m_LastDataFromRtsp=GetTickCount() ;

  //is the duration update thread running?
  if (!IsThreadRunning())
  {
    //nop? then start it
    //LogDebug("  CTsReaderFilter::Pause()->start duration thread");
    StartThread();
  }
  return hr;
}

STDMETHODIMP CTsReaderFilter::GetDuration(REFERENCE_TIME *dur)
{
  if(!dur)
    return E_INVALIDARG;

  CAutoLock lock (&m_CritSecDuration);
  *dur = (REFERENCE_TIME)m_duration.Duration();

  return NOERROR;
}

STDMETHODIMP CTsReaderFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
  LogDebug("CTsReaderFilter::Load()");
  //clean up any old file readers
  if (m_fileReader!=NULL)
    delete m_fileReader;
  if (m_fileDuration!=NULL)
    delete m_fileDuration;
  m_fileReader=NULL;
  m_fileDuration=NULL;
  m_seekTime=CRefTime(0L);
  m_bSeeking=false;

  wcscpy(m_fileName,pszFileName);
  char url[MAX_PATH];
  WideCharToMultiByte(CP_ACP,0,m_fileName,-1,url,MAX_PATH,0,0);
  //strcpy(url,"rtsp://192.168.1.102/stream1.0");
  //strcpy(url,"rtsp://192.168.1.58/F8C66B49");
  //check file type
  int length=strlen(url);
  if ((length > 5) && (_strcmpi(&url[length-4], ".tsp") == 0))
  {
    // .tsp file
    m_bTimeShifting=true;
    m_bLiveTv=true ;

    FILE* fd=fopen(url,"rb");
    if (fd==NULL) return E_FAIL;
    fread(url,1,100,fd);
    int bytesRead=fread(url,1,sizeof(url),fd);
    if (bytesRead>=0) url[bytesRead]=0;
    fclose(fd);

    LogDebug("open %s", url);
    if ( !m_rtspClient.OpenStream(url)) return E_FAIL;

    m_buffer.Clear();
    m_buffer.Run(true);
    m_rtspClient.Play(0.0f);
    m_tickCount=GetTickCount();
    m_fileReader = new CMemoryReader(m_buffer);
    m_demultiplexer.SetFileReader(m_fileReader);
    m_demultiplexer.Start();
    m_buffer.Run(false);
    m_tickCount=GetTickCount();

    LogDebug("close rtsp:%s", url);
    m_rtspClient.Stop();
    double duration=m_rtspClient.Duration()/1000.0f;
    CPcr pcrstart,pcrEnd,pcrMax;
    pcrstart=m_duration.StartPcr();
    duration+=pcrstart.ToClock();
    pcrEnd.FromClock(duration);
    m_duration.Set( pcrstart, pcrEnd,pcrMax);
  }
  else if ((length > 7) && (strnicmp(url, "rtsp://",7) == 0))
  {
    //rtsp:// stream
    //open stream
    LogDebug("open rtsp:%s", url);
    if ( !m_rtspClient.OpenStream(url)) return E_FAIL;

    m_bTimeShifting=true;
    m_bLiveTv=true ;

    //are we playing a recording via RTSP
    if (strstr(url,"/stream")==NULL)
    {
      //yes, then we're not timeshifting
      m_bTimeShifting=false;
      m_bLiveTv=false ;
    }

    //play
    m_buffer.Clear();
    m_buffer.Run(true);
    m_rtspClient.Play(0.0f);
    m_tickCount=GetTickCount();
    m_fileReader = new CMemoryReader(m_buffer);

    //get audio /video pids
    m_demultiplexer.SetFileReader(m_fileReader);
    m_demultiplexer.Start();
    m_buffer.Run(false);
    m_tickCount=GetTickCount();

    // stop streaming
    LogDebug("close rtsp:%s", url);
    m_rtspClient.Stop();

    //get the duration of the stream
    double duration=m_rtspClient.Duration()/1000.0f;
    CPcr pcrstart,pcrEnd,pcrMax;
    pcrstart=m_duration.StartPcr();
    duration+=pcrstart.ToClock();
    pcrEnd.FromClock(duration);
    m_duration.Set( pcrstart, pcrEnd,pcrMax);
  }
  else
  {
    if ((length < 9) || (_strcmpi(&url[length-9], ".tsbuffer") != 0))
    {
      //local .ts file
      m_bTimeShifting=false;
      m_bLiveTv = false ;
      m_fileReader = new FileReader();
      m_fileDuration = new FileReader();
    }
    else
    {
      //local timeshift buffer file file
      m_bTimeShifting=true;
      m_bLiveTv = true ;
      m_fileReader = new MultiFileReader();
      m_fileDuration = new MultiFileReader();
    }

    //open file
    m_fileReader->SetFileName(m_fileName);
    m_fileReader->OpenFile();

    m_fileDuration->SetFileName(m_fileName);
    m_fileDuration->OpenFile();

    //detect audio/video pids
    m_demultiplexer.SetFileReader(m_fileReader);
    m_demultiplexer.Start();

    //get file duration
    m_duration.SetFileReader(m_fileDuration);
    m_duration.UpdateDuration();

    float milli=m_duration.Duration().Millisecs();
    milli/=1000.0;
    LogDebug("start:%x end:%x %f",
        (DWORD)m_duration.StartPcr().PcrReferenceBase,(DWORD) m_duration.EndPcr().PcrReferenceBase,
        milli);
    m_fileReader->SetFilePointer(0LL,FILE_BEGIN);
  }

  //AddGraphToRot(GetFilterGraph());
  SetDuration();

  return S_OK;
}


STDMETHODIMP CTsReaderFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)//
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
  return 0;
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
  CAutoLock lock (&m_CritSecDuration);
  return 0;
}

///Seeks to the specified seekTime
void CTsReaderFilter::Seek(CRefTime& seekTime, bool seekInfile)
{
  //dont seek to the same location as last time
  //if (m_seekTime==seekTime) return;
  bool doSeek=true ;

  LogDebug("CTsReaderFilter::Seek-- LiveTv : %d, TimeShifting : %d",m_bLiveTv,m_bTimeShifting);

  SetWaitForSeekToEof(true) ;
	m_bPauseOnly=false;

  m_seekTime=seekTime;
  if (seekInfile==false) return;

  if (m_bTimeShifting)
  {
//    if (GetTickCount()-m_lastPause < 200)
//      SetWaitForNewPat(false) ;   // This is not a channel change, it's too fast !!.

    doSeek = false ;
    double startTime=m_seekTime.Millisecs();
    double duration=m_duration.Duration().Millisecs();
    if ((startTime+200 < duration) || m_bAnalog)   // Seek to end of file ?
    {
      doSeek = true ;       // No, exec seek !
      m_bLiveTv = false ;   // => No LiveTv.
//      SetWaitForNewPat(false) ;
    }
    else
    {
      if (!m_bLiveTv)       // Is reading Live data ? ( Live data means reading at end of file )
      {
        doSeek=true ;       // No, seek to end.
        m_bLiveTv=true ;    // Will be live after seek if it's timeshifting.
      }
    }
    LogDebug("Zap to File Seek : %d mS ( %f / %f ) LiveTv : %d, Seek : %d",GetTickCount()-m_lastPause, startTime, duration, m_bLiveTv, doSeek);
  }

  m_bSeeking=true;

  //are we playing a rtsp:// stream?
  if (m_fileDuration!=NULL)
  {
    //no, do a seek in the local file
    double startTime=m_seekTime.Millisecs();
    double duration=m_duration.Duration().Millisecs();

    startTime/=1000.0f;
    duration/=1000.0f;

    LogDebug("CTsReaderFilter::  Seek-> %f/%f (%d)",startTime,duration, doSeek);
    //if (seekTime >= m_duration.Duration())
    //  seekTime=m_duration.Duration();
    if (doSeek)
    {
      CTsFileSeek seek(m_duration);
      seek.SetFileReader(m_fileReader);
      seek.Seek(seekTime);
    }
  }
  else
  {
    //yes, we're playing a RTSP stream
    //stop the RTSP steam
    LogDebug("CTsReaderFilter::  Seek->stop rtsp");
    m_rtspClient.Stop();
    double startTime=m_seekTime.Millisecs();
    startTime/=1000.0f;
    float milli=m_duration.Duration().Millisecs();
    milli/=1000.0;

		if (m_bLiveTv) startTime+=10.0f ; // If liveTv, it's a seek to end, force end of buffer.

    LogDebug("CTsReaderFilter::  Seek->start client from %f/ %f",startTime,milli);
    //clear the buffers
    m_demultiplexer.Flush();
    m_buffer.Clear();
    m_buffer.Run(true);
    //start rtsp stream from the seek-time
    m_rtspClient.Play(startTime);

    int loop=0;
    while (m_buffer.Size() == 0 && loop++ <= 50 ) // lets exit the loop if no data received for 5 secs.
    {
      LogDebug("CTsReaderFilter:: Seek-->buffer empty, sleep(100ms)");
      Sleep(100);
    }

    m_tickCount=GetTickCount();

    //update the duration of the stream
    double duration=m_rtspClient.Duration()/1000.0f;
CPcr pcrstart,pcrEnd,pcrMax;
    pcrstart=m_duration.StartPcr();
    duration+=pcrstart.ToClock();
    pcrEnd.FromClock(duration);
    m_duration.Set( pcrstart, pcrEnd,pcrMax);
    LogDebug("CTsReaderFilter::  Seek->start client done duration:%2.2f",duration);
  }
}

bool CTsReaderFilter::IsFilterRunning()
{
  if (m_fileDuration!=NULL) return true;
  return m_buffer.IsRunning();
}


/// Returns true if one of the output pins is currently seeking
bool CTsReaderFilter::IsSeeking()
{
  return m_bSeeking;
}

/// Called by one of the output pins to indicate it has finished seeking
/// in response to a IMediaSeeking.SetPositions()
void CTsReaderFilter::SeekDone(CRefTime& rtSeek)
{
  if (m_fileDuration!=NULL)
  {
    if (rtSeek >= m_duration.Duration())
      rtSeek=m_duration.Duration();
  }
  LogDebug("CTsReaderFilter::--SeekDone()");
  //m_demultiplexer.Flush();
  m_bSeeking=false;

  m_demultiplexer.CallTeletextEventCallback(TELETEXT_EVENT_SEEK_END,TELETEXT_EVENTVALUE_NONE);

  if (m_pDVBSubtitle)
  {
    m_pDVBSubtitle->SetFirstPcr(m_duration.FirstStartPcr().PcrReferenceBase);
    m_pDVBSubtitle->SeekDone(rtSeek);
  }
}

// When a IMediaSeeking.SetPositions() is done on one of the output pins the output pin will do:
//  SeekStart() ->indicates to any other output pins we're busy seeking
//  Seek()      ->Does the seeking
//  SeekDone()  ->indicates that seeking has finished
// This prevents the situation where multiple outputpins are seeking in the file at the same time
void CTsReaderFilter::SeekStart()
{
  LogDebug("CTsReaderFilter::--SeekStart()--");
  m_demultiplexer.CallTeletextEventCallback(TELETEXT_EVENT_SEEK_START,TELETEXT_EVENTVALUE_NONE);
  m_bSeeking=true;
}

///Returns the audio output pin
CAudioPin* CTsReaderFilter::GetAudioPin()
{
  return m_pAudioPin;
}
///Returns the video output pin
CVideoPin* CTsReaderFilter::GetVideoPin()
{
  return m_pVideoPin;
}
///Returns the subtitle output pin
CSubtitlePin* CTsReaderFilter::GetSubtitlePin()
{
  return m_pSubtitlePin;
}

IDVBSubtitle* CTsReaderFilter::GetSubtitleFilter()
{
  /*if( !m_pDVBSubtitle )
  {
    FindSubtitleFilter(); // THIS CAUSED A DEADLOCK WITH STOP() ! NOW ONLY EXECUTED IN RUN()
  }*/
  return m_pDVBSubtitle;
}

//**************************************************************************************************************
/// This method is running in its own thread
/// Every second it will check the stream or local file and determine the total duration of the file/stream
/// The duration can/will grow if we are playing a timeshifting buffer/stream
//  If the duration has changed it will update m_duration and send a EC_LENGTH_CHANGED event
//  to the graph
void CTsReaderFilter::ThreadProc()
{
  LogDebug("CTsReaderFilter::ThreadProc start()");

  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_BELOW_NORMAL);
do
{
    //if demuxer reached the end of the file, we can stop the thread
    //since we're no longer playing
    if (m_demultiplexer.EndOfFile())
      break;
    //are we playing an RTSP stream?
    if (m_fileDuration!=NULL)
    {
      //no, then get the duration from the local file
      CTsDuration duration;
      duration.SetFileReader(m_fileDuration);
      duration.SetVideoPid(m_duration.GetPid());
      duration.UpdateDuration();

      //did we find a duration?
      if (duration.Duration().Millisecs()>0)
      {
        //yes, is it different then the one we determined last time?
        if (duration.StartPcr().PcrReferenceBase!=m_duration.StartPcr().PcrReferenceBase ||
            duration.EndPcr().PcrReferenceBase!=m_duration.EndPcr().PcrReferenceBase)
        {
          //yes, then update it
          m_duration.Set(duration.StartPcr(), duration.EndPcr(), duration.MaxPcr());

          // Is graph running?
          if (m_State == State_Running||m_State==State_Paused)
          {
            //yes, then send a EC_LENGTH_CHANGED event to the graph
            NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
            SetDuration();
          }
        }
      }
    }
    else
    {
      // we are not playing a local file
      // are we playing a (RTSP) stream?
      if (m_rtspClient.IsRunning())
      {
        if (m_bTimeShifting)
        {
          //yes.
          //Then update the duration. Since we cannot 'read' the duration continously from the stream
          //we take the duration we got when we started playing this stream
          //and add the time-passed since then to it which should give a indication of the current
          //duration
          DWORD ticks=(GetTickCount()-m_tickCount)/1000;
          double duration=m_rtspClient.Duration()/1000.0f;
          duration+=ticks;
          CPcr pcrstart,pcrEnd,pcrMax;
          pcrstart=m_duration.StartPcr();
          duration+=pcrstart.ToClock();
          pcrEnd.FromClock(duration);

          CTsDuration newDuration;
          newDuration.Set( pcrstart, pcrEnd,pcrMax);

          //did we find a duration?
          if (newDuration.Duration().Millisecs()>0)
          {
            //did the duration change?
            if (newDuration.Duration() != m_duration.Duration())
            {
              //set the duration
              m_duration.Set( pcrstart, pcrEnd,pcrMax);

              // Is graph running?
              if (m_State == State_Running)
              {

                //yes, then send a EC_LENGTH_CHANGED event to the graph
                NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
                SetDuration();
              }
            }
          }
        }
      }
    }
  }
  while (!ThreadIsStopping(1000)) ;
  LogDebug("CTsReaderFilter::ThreadProc stopped()");
}

void CTsReaderFilter::SetDuration()
{
  return;
  DWORD secs=m_duration.Duration().Millisecs();
  HKEY key;
  if (ERROR_SUCCESS==RegOpenKey(HKEY_CURRENT_USER, "Software\\MediaPortal\\TsReader",&key))
  {
    RegSetValueEx(key, "duration",0,REG_DWORD,(const BYTE*)&secs,sizeof(DWORD));
    RegCloseKey(key);
  }
}

HRESULT CTsReaderFilter::AddGraphToRot(IUnknown *pUnkGraph)
{
  CComPtr <IMoniker>              pMoniker;
  CComPtr <IRunningObjectTable>   pROT;
  WCHAR wsz[128];
  HRESULT hr;

  if (m_dwGraphRegister!=0) return S_OK;
  if (FAILED(GetRunningObjectTable(0, &pROT)))
      return E_FAIL;

  swprintf(wsz, L"FilterGraph %08x pid %08x\0", (DWORD_PTR) pUnkGraph, GetCurrentProcessId());
  hr = CreateItemMoniker(L"!", wsz, &pMoniker);
  if (SUCCEEDED(hr))
  {
    hr = pROT->Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, pUnkGraph, pMoniker, &m_dwGraphRegister);
  }
  return hr;
}


// Removes a filter graph from the Running Object Table
void CTsReaderFilter::RemoveGraphFromRot()
{
  if (m_dwGraphRegister==0) return;
  CComPtr <IRunningObjectTable> pROT;

  if (SUCCEEDED(GetRunningObjectTable(0, &pROT)))
      pROT->Revoke(m_dwGraphRegister);
}

/// method which implements IAMStreamSelect.Count
/// returns the number of audio streams available
STDMETHODIMP CTsReaderFilter::Count(DWORD* streamCount)
{
  *streamCount=m_demultiplexer.GetAudioStreamCount();
  return S_OK;
}

/// method which implements IAMStreamSelect.Enable
/// Sets the current audio stream to use
STDMETHODIMP CTsReaderFilter::Enable(long index, DWORD flags)
{
  bool res = m_demultiplexer.SetAudioStream((int)index);
  return S_OK;
}

/// method which implements IAMStreamSelect.Info
/// returns an array of all audio streams available
STDMETHODIMP CTsReaderFilter::Info( long lIndex,AM_MEDIA_TYPE **ppmt,DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk)
{
  if (pdwFlags)
  {
  int audioIndex = 0;
  m_demultiplexer.GetAudioStream(audioIndex);

    //if (m_demultiplexer.GetAudioStream()==(int)lIndex)
  if (audioIndex==(int)lIndex)
      *pdwFlags=AMSTREAMSELECTINFO_EXCLUSIVE;
    else
      *pdwFlags=0;
  }
  if (plcid) *plcid=0;
  if (pdwGroup) *pdwGroup=0;
  if (ppObject) *ppObject=NULL;
  if (ppUnk) *ppUnk=NULL;
  if (ppszName)
  {
    char szName[20];
    m_demultiplexer.GetAudioStreamInfo((int)lIndex,szName);
    *ppszName = (WCHAR *)CoTaskMemAlloc(20);
    MultiByteToWideChar(CP_ACP,0,szName,-1,*ppszName,20);
  }
  if (ppmt)
  {
    CMediaType mediaType;
    m_demultiplexer.GetAudioStreamType((int)lIndex,mediaType);
    AM_MEDIA_TYPE* mType=(AM_MEDIA_TYPE*)(&mediaType);
    *ppmt=(AM_MEDIA_TYPE*)CoTaskMemAlloc(sizeof(AM_MEDIA_TYPE));
    memcpy(*ppmt, mType,sizeof(AM_MEDIA_TYPE));

    (*ppmt)->pbFormat=(BYTE*)CoTaskMemAlloc(mediaType.FormatLength());
    memcpy((*ppmt)->pbFormat,mType->pbFormat,mediaType.FormatLength());
  }
  return S_OK;
}

// IAudioStream methods
/*
STDMETHODIMP CTsReaderFilter::SetAudioStream(__int32 stream)
{
  return m_demultiplexer.SetAudioStream(stream);
}
*/
STDMETHODIMP CTsReaderFilter::GetAudioStream(__int32 &stream)
{
  return m_demultiplexer.GetAudioStream(stream);
}

// ITeletextSource methods
STDMETHODIMP CTsReaderFilter::SetTeletextTSPacketCallBack ( int (CALLBACK *pPacketCallback)(byte*, int))
{
  LogDebug("Setting Teletext TS packet callback");
  m_demultiplexer.SetTeletextPacketCallback(pPacketCallback);
  return S_OK;
}

STDMETHODIMP CTsReaderFilter::SetTeletextServiceInfoCallback( int (CALLBACK *pSICallback)(int,byte,byte,byte,byte) )
{
  LogDebug("Setting Teletext Service Info callback");
  m_demultiplexer.SetTeletextServiceInfoCallback(pSICallback);
  return S_OK;
}

STDMETHODIMP CTsReaderFilter::SetTeletextEventCallback( int (CALLBACK *pEventCallback)(int ecode,DWORD64 ev) )
{
  LogDebug("Setting Teletext Event callback");
  m_demultiplexer.SetTeletextEventCallback(pEventCallback);
  return S_OK;
}

// ISubtitleStream methods
STDMETHODIMP CTsReaderFilter::SetSubtitleStream(__int32 stream)
{
  return m_demultiplexer.SetSubtitleStream(stream);
}

STDMETHODIMP CTsReaderFilter::GetSubtitleStreamLanguage(__int32 stream,char* szLanguage)
{
  return m_demultiplexer.GetSubtitleStreamLanguage( stream, szLanguage );
}

STDMETHODIMP CTsReaderFilter::GetSubtitleStreamType(__int32 stream, int &type)
{
  return m_demultiplexer.GetSubtitleStreamType(stream, type);
}

STDMETHODIMP CTsReaderFilter::GetSubtitleStreamCount(__int32 &count)
{
  return m_demultiplexer.GetSubtitleStreamCount(count);
}

STDMETHODIMP CTsReaderFilter::GetCurrentSubtitleStream(__int32 &stream)
{
  return m_demultiplexer.GetCurrentSubtitleStream(stream);
}

STDMETHODIMP CTsReaderFilter::SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int c, void* opts, int* select)){
  //LogDebug("CTsReaderFilter SetSubtitleResetCallback");
  return m_demultiplexer.SetSubtitleResetCallback( pSubUpdateCallback );
}

//
// FindSubtitleFilter
//
// be careful with this method, can cause deadlock with CSync::Stop, so should only be called in Run()
HRESULT CTsReaderFilter::FindSubtitleFilter()
{
  if( m_pDVBSubtitle )
  {
    return S_OK;
  }
  //LogDebug( "FindSubtitleFilter - start");

  IEnumFilters * piEnumFilters = NULL;
  if (GetFilterGraph() && SUCCEEDED(GetFilterGraph()->EnumFilters(&piEnumFilters)))
  {
    IBaseFilter * pFilter;
    while (piEnumFilters->Next(1, &pFilter, 0) == NOERROR )
    {
      FILTER_INFO filterInfo;
      if (pFilter->QueryFilterInfo(&filterInfo) == S_OK)
      {
        if (!wcsicmp(L"MediaPortal DVBSub2", filterInfo.achName))
        {
          HRESULT fhr = pFilter->QueryInterface( IID_IDVBSubtitle2, ( void**)&m_pDVBSubtitle );
          assert( fhr == S_OK);
          //LogDebug("Testing that DVBSub2 works");
          m_pDVBSubtitle->Test(1);
        }
        filterInfo.pGraph->Release();
      }
      pFilter->Release();
      pFilter = NULL;
    }
    piEnumFilters->Release();
  }
  //LogDebug( "FindSubtitleFilter - End");
  return S_OK;
}

CTsDuration& CTsReaderFilter::GetDuration()
{
  return m_duration;
}

bool CTsReaderFilter::IsStreaming()
{
  return (m_fileDuration==NULL);;
}


void CTsReaderFilter::SetWaitForSeekToEof(bool onOff)
{
  LogDebug("Wait for seeking to eof %d",onOff) ;
  m_WaitForSeekToEof = onOff ;
}

bool CTsReaderFilter::IsSeekingToEof()
{
  return m_WaitForSeekToEof ;
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

