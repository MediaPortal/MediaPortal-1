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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <sbe.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "subtitlepin.h"
#include "tsfileSeek.h"
#include "memoryreader.h"
void LogDebug(const char *fmt, ...) 
{
#ifndef DONTLOG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 
	SYSTEMTIME systemTime;
	GetLocalTime(&systemTime);

  FILE* fp = fopen("c:\\tsreader.log","a+");
	if (fp!=NULL)
	{
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
  }
	char buf[1000];
	sprintf(buf,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
		systemTime.wDay, systemTime.wMonth, systemTime.wYear,
		systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
		buffer);
	::OutputDebugString(buf);
	
#endif
};


const AMOVIESETUP_MEDIATYPE acceptAudioPinTypes =
{
	&MEDIATYPE_Audio,                  // major type
  &MEDIASUBTYPE_MPEG1Audio      // minor type
};
const AMOVIESETUP_MEDIATYPE acceptVideoPinTypes =
{
	&MEDIATYPE_Video,                  // major type
	&MEDIASUBTYPE_MPEG2_VIDEO      // minor type
};

const AMOVIESETUP_MEDIATYPE acceptSubtitlePinTypes =
{
  &MEDIATYPE_Stream,           // major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT      // minor type
};

const AMOVIESETUP_PIN audioVideoPin[] =
{
	{L"Audio",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptAudioPinTypes},
	{L"Video",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptVideoPinTypes},
	{L"Subtitle",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptSubtitlePinTypes}
};

const AMOVIESETUP_FILTER TSReader =
{
  &CLSID_TSReader,L"MediaPortal File Reader",MERIT_NORMAL+1000,2,audioVideoPin
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
  m_rtspClient(m_buffer)
{

  ::DeleteFile("c:\\tsreader.log");
  LogDebug("-------------- v1.0.0.0 ----------------");

  m_fileReader=NULL;
  m_fileDuration=NULL;
  Compensation=CRefTime(0L);

	LogDebug("CTsReaderFilter::ctor");
	m_pAudioPin = new CAudioPin(GetOwner(), this, phr,&m_section);
	m_pVideoPin = new CVideoPin(GetOwner(), this, phr,&m_section);
  m_pSubtitlePin = new CSubtitlePin(GetOwner(), this, phr,&m_section);
  
  // Not used anywhere and currently is causing a leak (2 objects left active in TsReader.ax)
  // - tourettes
  // m_referenceClock= new CBaseReferenceClock("refClock",GetOwner(), phr);
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
		//else  if (n==2)
		//{
    //    return m_pSubtitlePin;
    //}  
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
	CRefTime runTime=tStart;
	double msec=(double)runTime.Millisecs();
	msec/=1000.0;
	LogDebug("CTsReaderFilter::Run(%05.2f)",msec);
  CAutoLock cObjectLock(m_pLock);
		 
  //are we using RTSP or local file
  if (m_fileDuration==NULL)
  {
    //using RTSP, if its streaming is paused then 
    //stop pausing and continue streaming
    if (m_rtspClient.IsPaused())
    {
	    LogDebug(" CTsReaderFilter::Run()  -->is paused,continue rtsp");
      m_buffer.Clear();
      m_buffer.Run(true);
      m_rtspClient.Continue();
	    LogDebug(" CTsReaderFilter::Run()  --> rtsp running");
    }
  }

  //Set our StreamTime Reference offset to zero
	HRESULT hr= CSource::Run(tStart);

  LogDebug("CTsReaderFilter::Run(%05.2f)  -->done",msec);
  return hr;
}

STDMETHODIMP CTsReaderFilter::Stop()
{
	LogDebug("CTsReaderFilter::Stop()");

  //guarantees that audio/video pins dont block in the fillbuffer() method
  m_bSeeking=true;
  m_demultiplexer.SetHoldAudio(true);
  m_demultiplexer.SetHoldVideo(true);
  
  //stop duration thread
  StopThread();

	LogDebug("CTsReaderFilter::Stop()  -stop source");
  //stop filter
	HRESULT hr=CSource::Stop();
  
  //are we using rtsp?
  if (m_fileDuration==NULL)
  { 
    //yep then stop streaming
	  LogDebug("CTsReaderFilter::Stop()   -- stop rtsp");
    m_buffer.Run(false);
    m_rtspClient.Stop();
  }

  //reset vaues
  m_bNeedSeeking=false;
  m_bSeeking=false;
  m_demultiplexer.SetHoldAudio(false);
  m_demultiplexer.SetHoldVideo(false);
  m_demultiplexer.Flush();
  
	LogDebug("CTsReaderFilter::Stop() done");
	return hr;
}
bool CTsReaderFilter::IsTimeShifting()
{
  return m_bTimeShifting;
}
STDMETHODIMP CTsReaderFilter::Pause()
{
	LogDebug("CTsReaderFilter::Pause()");
  CAutoLock cObjectLock(m_pLock);

  //pause filter
  HRESULT hr= CSource::Pause();

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
        //stop audio/video pins from blocking in FillBuffer()
        m_demultiplexer.SetHoldAudio(true);
        m_demultiplexer.SetHoldVideo(true);
        double startTime=m_seekTime.Millisecs();

        //clear buffers
        m_demultiplexer.Flush();
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
        m_duration.Set( pcrstart, pcrEnd,pcrMax);

        //allow audio/video pins to block in fillbuffer(0
        m_demultiplexer.SetHoldAudio(false);
        m_demultiplexer.SetHoldVideo(false);
        LogDebug("  -- Pause()-  >duration:%f",(m_rtspClient.Duration()/1000.0f));
      }
      else
      {
        //we are streaming at the moment.
        //pause the streaming
        LogDebug("  -- Pause()  ->pause rtsp");
        m_buffer.Run(false);
        m_rtspClient.Pause();
      }
    }
  }

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
  m_bNeedSeeking=false;

	wcscpy(m_fileName,pszFileName);
  char url[MAX_PATH];
  WideCharToMultiByte(CP_ACP,0,m_fileName,-1,url,MAX_PATH,0,0);
  //strcpy(url,"rtsp://192.168.1.102/stream1.0");
  //strcpy(url,"rtsp://192.168.1.102/stream2.0");
  //check file type
  int length=strlen(url);	
  if ((length > 5) && (_strcmpi(&url[length-4], ".tsp") == 0))
  {
    // .tsp file
    m_bTimeShifting=true;
    
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
    
    //play
    m_bTimeShifting=true;
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
      m_fileReader = new FileReader();
      m_fileDuration = new FileReader();
    }
    else
    {
      //local timeshift buffer file file
      m_bTimeShifting=true;
      m_fileReader = new MultiFileReader();
      m_fileDuration = new MultiFileReader();
    }

    //open file
	  m_fileReader->SetFileName(url);
	  m_fileReader->OpenFile();

    m_fileDuration->SetFileName(url);
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
   if (m_seekTime==seekTime) return;

  LogDebug("CTsReaderFilter::Seek--");
  m_seekTime=seekTime;
  if (seekInfile==false) return;

  m_bSeeking=true;
  //are we playing a rtsp:// stream?
  if (m_fileDuration!=NULL)
  {
    //no, do a seek in the local file
    double startTime=m_seekTime.Millisecs();
    double duration=m_duration.Duration().Millisecs();
    startTime/=1000.0f;
    duration/=1000.0f;
    LogDebug("CTsReaderFilter::  Seek-> %f/%f",startTime,duration);
    if (seekTime >= m_duration.Duration())
      seekTime=m_duration.Duration();
    CTsFileSeek seek(m_duration);
    seek.SetFileReader(m_fileReader);
    seek.Seek(seekTime);
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
    if (startTime >= milli-40.0f)
    {
      startTime=milli+40.0f;
    }

    LogDebug("CTsReaderFilter::  Seek->start client from %f/ %f",startTime,milli);
    //clear the buffers
    m_demultiplexer.Flush();
    m_buffer.Clear();
    m_buffer.Run(true);
    //start rtsp stream from the seek-time
    m_rtspClient.Play(startTime);
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
  m_demultiplexer.Flush();
  m_bSeeking=false;
  
}

// When a IMediaSeeking.SetPositions() is done on one of the output pins the output pin will do:
//  SeekStart() ->indicates to any other output pins we're busy seeking
//  Seek()      ->Does the seeking
//  SeekDone()  ->indicates that seeking has finished
// This prevents the situation where multiple outputpins are seeking in the file at the same time
void CTsReaderFilter::SeekStart()
{
  LogDebug("CTsReaderFilter::--SeekStart()--");
  m_bSeeking=true;
}

///Returns the audio output pin
///
CAudioPin* CTsReaderFilter::GetAudioPin()
{
  return m_pAudioPin;
}
///Returns the video output pin
///
CVideoPin* CTsReaderFilter::GetVideoPin()
{
  return m_pVideoPin;
}
///Returns the subtitle output pin
///
CSubtitlePin* CTsReaderFilter::GetSubtitlePin()
{
  return m_pSubtitlePin;
}


/// This method is running in its own thread
/// Every second it will check the stream or local file and determine the total duration of the file/stream
/// The duration can/will grow if we are playing a timeshifting buffer/stream
//  If the duration has changed it will update m_duration and send a EC_LENGTH_CHANGED event
//  to the graph
void CTsReaderFilter::ThreadProc()
{
  LogDebug("CTsReaderFilter::ThreadProc start()");

  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_BELOW_NORMAL);
  while (!ThreadIsStopping(100))
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
				if (duration.Duration() != m_duration.Duration())
				{
          //yes, then update it
          m_duration.Set(duration.StartPcr(), duration.EndPcr(), duration.MaxPcr());

          // Is graph running?
					if (m_State == State_Running)
					{
            //yes, then send a EC_LENGTH_CHANGED event
						float secs=(float)duration.Duration().Millisecs();
						secs/=1000.0f;
						//LogDebug("notify length change:%f",secs);
						NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
						SetDuration();
					}
				}
			}
    }
    else 
    {
      // we not playing a local file
      // are we playing a (RTSP) stream?
      if (m_rtspClient.IsRunning())
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
            //char sztmp[256];
            //sprintf(sztmp,"%f %d %f\n", (m_rtspClient.Duration()/1000.0f), ticks, (((float)m_duration.Duration().Millisecs())/1000.0f) );
            //::OutputDebugStringA(sztmp);

            // Is graph running?
				    if (m_State == State_Running)
				    {
              //send the event
              NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
              SetDuration();
            }
          }
        }
      }
    }
    Sleep(1000);
  }
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
  m_demultiplexer.SetAudioStream((int)index);
  return S_OK;
}

/// method which implements IAMStreamSelect.Info
/// returns an array of all audio streams available
STDMETHODIMP CTsReaderFilter::Info( long lIndex,AM_MEDIA_TYPE **ppmt,DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk)
{
  if (pdwFlags) 
  {
    if (m_demultiplexer.GetAudioStream()==(int)lIndex)
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

