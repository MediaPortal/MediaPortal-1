/*
 *  Copyright (C) 2005-2011 Team MediaPortal
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
#include "StdAfx.h"

#include <commdlg.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include <tchar.h>
#include <keys.h>
#include "bdreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "subtitlepin.h"
#include "..\..\shared\DebugSettings.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void SetThreadName(DWORD dwThreadID, char* threadName);
extern void LogDebug(const char *fmt, ...);
extern void GetLogFile(char *pLog);

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

const AMOVIESETUP_PIN pins[] =
{
  {L"Audio", FALSE, TRUE, FALSE, FALSE, &CLSID_NULL, NULL, 1, &acceptAudioPinTypes},
  {L"Video", FALSE, TRUE, FALSE, FALSE, &CLSID_NULL, NULL, 1, &acceptVideoPinTypes},
  {L"Subtitle", FALSE, TRUE, FALSE, FALSE, &CLSID_NULL, NULL, 1, &acceptSubtitlePinTypes}
};

const AMOVIESETUP_FILTER BDReader =
{
  &CLSID_BDReader, L"MediaPortal BD Reader", MERIT_NORMAL + 1000, 3, pins
};

CFactoryTemplate g_Templates[] =
{
  {L"MediaPortal BD Reader", &CLSID_BDReader, CBDReaderFilter::CreateInstance, NULL, &BDReader},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);

CUnknown * WINAPI CBDReaderFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
  ASSERT(phr);
  CBDReaderFilter *pNewObject = new CBDReaderFilter(punk, phr);

  if (!pNewObject)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
  }
  return pNewObject;
}

// Constructor
CBDReaderFilter::CBDReaderFilter(IUnknown *pUnk, HRESULT *phr):
  CSource(NAME("CBDReaderFilter"), pUnk, CLSID_BDReader),
  m_pAudioPin(NULL),
  m_demultiplexer(*this),
  m_pDVBSubtitle(NULL),
  m_pCallback(NULL),
  m_pRequestAudioCallback(NULL),
  m_hCommandThread(NULL),
  m_hCommandEvent(NULL),
  m_hStopCommandThreadEvent(NULL),
  m_bUpdateStreamPositionOnly(false),
  m_dwThreadId(0),
  m_pMediaSeeking(NULL),
  m_rtPlaybackOffset(_I64_MIN),
  m_rtSeekPosition(0),
  m_rtTitleDuration(0),
  m_rtCurrentTime(0),
  m_rtStart(0),
  m_rtStop(0),
  m_rtCurrent(0),
  m_rtNewStart(0),
  m_rtNewStop(0),
  m_dRate(1.0),
  m_rtLastStart(0),
  m_rtLastStop(0),
  m_bFlushing(false),
  m_bRebuildOngoing(false),
  m_bChapterChangeRequested(false),
  m_bFirstSeek(true)
{
  // use the following line if you are having trouble setting breakpoints
  // #pragma comment( lib, "strmbasd" )
  TCHAR filename[1024];
  GetLogFile(filename);
  ::DeleteFile(filename);
  LogDebug("--------- bluray ---------------------");
  LogDebug("-------------- v0.63 -----------------");

  LogDebug("CBDReaderFilter::ctor");
  m_pAudioPin = new CAudioPin(GetOwner(), this, phr, &m_section, m_demultiplexer);
  m_pVideoPin = new CVideoPin(GetOwner(), this, phr, &m_section, m_demultiplexer);
  m_pSubtitlePin = new CSubtitlePin(GetOwner(), this, phr, &m_section);

  if (!m_pAudioPin || !m_pVideoPin || !m_pSubtitlePin)
  {
    *phr = E_OUTOFMEMORY;
    return;
  }
  
  wcscpy(m_fileName, L"");

  LogDebug("Wait for seeking to eof - false - constructor");
  m_bStopping = false;
  m_MPmainThreadID = GetCurrentThreadId();

  lib.Initialize();
  lib.SetEventObserver(this);

  // Manual reset to allow easier command queue handling
  m_hCommandEvent = CreateEvent(NULL, true, false, NULL); 
  m_hStopCommandThreadEvent = CreateEvent(NULL, false, false, NULL);

  Create(); // CAMThread
}

CBDReaderFilter::~CBDReaderFilter()
{
  LogDebug("CBDReaderFilter::dtor");

  CAutoLock cAutoLock(this);

  m_eRebuild.Set(); // Release the command thead if graph rebuild was interrupted

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  if (m_hStopCommandThreadEvent)
  {
    SetEvent(m_hStopCommandThreadEvent);
    WaitForSingleObject(m_hCommandThread, INFINITE);
    CloseHandle(m_hStopCommandThreadEvent);
  }

  if (m_hCommandEvent)
    CloseHandle(m_hCommandEvent);

  lib.RemoveEventObserver(this);
  if (m_pAudioPin)
  {
    m_pAudioPin->Disconnect();
    delete m_pAudioPin;
  }

  if (m_pVideoPin)
  {
    m_pVideoPin->Disconnect();
    delete m_pVideoPin;
  }

  if (m_pSubtitlePin)
  {
    m_pSubtitlePin->Disconnect();
    delete m_pSubtitlePin;
  }

  if (m_pDVBSubtitle)
  {
    m_pDVBSubtitle->Release();
    m_pDVBSubtitle = NULL;
  }
}

STDMETHODIMP CBDReaderFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
  if (riid == IID_IMediaSeeking)
  {
    if (m_pAudioPin->IsConnected())
      return m_pAudioPin->NonDelegatingQueryInterface(riid, ppv);

    if (m_pVideoPin->IsConnected())
      return m_pVideoPin->NonDelegatingQueryInterface(riid, ppv);
  }
  if (riid == IID_IAMFilterMiscFlags)
    return GetInterface((IAMFilterMiscFlags*)this, ppv);
  if (riid == IID_IFileSourceFilter)
    return GetInterface((IFileSourceFilter*)this, ppv);
  if (riid == IID_IAMStreamSelect)
    return GetInterface((IAMStreamSelect*)this, ppv);
  if (riid == IID_ISubtitleStream)
    return GetInterface((ISubtitleStream*)this, ppv);
  if (riid == IID_IBDReader)
    return GetInterface((IBDReader*)this, ppv);
  if (riid == IID_IAudioStream)
    return GetInterface((IAudioStream*)this, ppv);

  return CSource::NonDelegatingQueryInterface(riid, ppv);
}

CBasePin* CBDReaderFilter::GetPin(int n)
{
  if (n == 0)
    return m_pAudioPin;
  else  if (n == 1)
    return m_pVideoPin;
  else if (n == 2)
    return m_pSubtitlePin;
  return NULL;
}

int CBDReaderFilter::GetPinCount()
{
  return 3;
}

void CBDReaderFilter::IssueCommand(DS_CMD_ID pCommand, REFERENCE_TIME pTime)
{
  {
    CAutoLock lock(&m_csCommandQueue);

    if (!m_hCommandThread)
      m_hCommandThread = CreateThread(NULL, 0, CBDReaderFilter::CommandThreadEntryPoint, (LPVOID)this, 0, &m_dwThreadId);

    DS_CMD cmd;
    cmd.id = pCommand;
    cmd.refTime = pTime;
    m_commandQueue.push_back(cmd);
  }
  
  SetEvent(m_hCommandEvent);
}

void CBDReaderFilter::TriggerOnMediaChanged()
{
  if (m_pCallback)
  {		
    m_demultiplexer.SetMediaChanging(true);
    m_bRebuildOngoing = true;

    int videoRate = 0;
    int audioType = m_demultiplexer.GetCurrentAudioStreamType();
    int videoType = m_demultiplexer.GetVideoServiceType();
    
    BLURAY_CLIP_INFO* clip = lib.CurrentClipInfo();
    if (clip)
      videoRate = clip->video_streams->rate;

    HRESULT hr = m_pCallback->OnMediaTypeChanged(videoRate, videoType, audioType);

    if (hr == S_FALSE)
    {
      // There was no need for the graph rebuilding
      m_demultiplexer.SetMediaChanging(false);
      m_eRebuild.Set();
      m_bRebuildOngoing = false;
    }
  }
  else
    LogDebug("CBDReaderFilter: TriggerOnMediaChanged - no callback set!");
}

void CBDReaderFilter::OnPlaybackPositionChange()
{
  if (m_pClock)
  {
    CAutoLock lock(&m_csClock);

    REFERENCE_TIME time = 0;
    m_pClock->GetTime(&time);

    if (m_rtPlaybackOffset == _I64_MIN)
      m_rtPlaybackOffset = time;

    if (m_State == State_Running && m_pCallback)
    {
      m_rtCurrentTime = time - m_rtPlaybackOffset + m_rtSeekPosition - m_rtRun + m_rtRunOffset;

      m_pCallback->OnClockChange(m_rtTitleDuration, m_rtCurrentTime);
      //LogDebug("dur: %6.3f pos: %6.3f run delay: %6.3f", m_rtTitleDuration / 10000000.0, m_rtCurrentTime / 10000000.0, (m_rtRun - m_rtRunOffset) / 10000000.0);
    }
  }
}

void CBDReaderFilter::SetTitleDuration(REFERENCE_TIME pTitleDuration)
{
  LogDebug("CBDReaderFilter: SetTitleDuration duration: %6.3f", pTitleDuration / 10000000.0);
  
  CAutoLock lock(&m_csClock);
  m_bFirstSeek = true;
  m_rtTitleDuration = pTitleDuration;
}

void CBDReaderFilter::ResetPlaybackOffset(REFERENCE_TIME pSeekPosition)
{
  LogDebug("CBDReaderFilter: ResetPlaybackOffset seek position: %6.3f", pSeekPosition / 10000000.0);
  
  CAutoLock lock(&m_csClock);
  m_rtSeekPosition = pSeekPosition;
  m_rtPlaybackOffset = _I64_MIN;

  if (m_pClock)
    m_pClock->GetTime(&m_rtRunOffset);

  m_rtLastStart = 0;
}

STDMETHODIMP CBDReaderFilter::SetGraphCallback(IBDReaderCallback* pCallback)
{
  CheckPointer(pCallback, E_INVALIDARG);
  LogDebug("callback set");
  m_pCallback = pCallback;

  return S_OK;
}

STDMETHODIMP CBDReaderFilter::SetVideoDecoder(int format, GUID* decoder)
{
  if (format != BLURAY_STREAM_TYPE_VIDEO_H264 &&
      format != BLURAY_STREAM_TYPE_VIDEO_VC1 &&
      format != BLURAY_STREAM_TYPE_VIDEO_MPEG2)
      return E_INVALIDARG;

  CheckPointer(decoder, E_INVALIDARG);

  if (m_pVideoPin)
  {
    m_pVideoPin->SetVideoDecoder(format, decoder);
    return S_OK;
  }

  return S_FALSE;
}

STDMETHODIMP CBDReaderFilter::SetVC1Override(GUID* subtype)
{
  CheckPointer(subtype, E_INVALIDARG);

  if (m_pVideoPin)
  {
    m_pVideoPin->SetVC1Override(subtype);
    return S_OK;
  }

  return S_FALSE;
}

STDMETHODIMP CBDReaderFilter::Action(int key)
{
  lib.LogAction(key);

  INT64 pos = -1;

  if (m_pMediaSeeking)
    HRESULT hr = m_pMediaSeeking->GetCurrentPosition(&pos);
  
  switch (key)
  {  
    case BD_VK_0:
    case BD_VK_1:
    case BD_VK_2:
    case BD_VK_3:
    case BD_VK_4:
    case BD_VK_5:
    case BD_VK_6:
    case BD_VK_7:
    case BD_VK_8:
    case BD_VK_9:
    case BD_VK_UP:
    case BD_VK_DOWN:
    case BD_VK_LEFT:
    case BD_VK_RIGHT:
    case BD_VK_ENTER:
    case BD_VK_POPUP:
    case BD_VK_ROOT_MENU:
    case BD_VK_MOUSE_ACTIVATE:
      return lib.ProvideUserInput(CONVERT_DS_90KHz(pos), (UINT32)key) == true ? S_OK : S_FALSE;
    break;
    default:  
      return S_FALSE;
  }
  return S_FALSE;
}

STDMETHODIMP CBDReaderFilter::SetAngle(UINT8 angle)
{
  return lib.SetAngle(angle) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::SetChapter(UINT32 chapter)
{
  m_bChapterChangeRequested = true;

  HRESULT hr = S_FALSE;

  UINT32 current = 0;
  if (lib.GetChapter(&current) && current != chapter)
  {
    hr = m_demultiplexer.FlushToChapter(chapter);
  }

  return hr;
}

STDMETHODIMP CBDReaderFilter::GetAngle(UINT8* angle)
{
  CheckPointer(angle, E_INVALIDARG);

  return lib.GetAngle(angle) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::GetChapter(UINT32* chapter)
{
  CheckPointer(chapter, E_INVALIDARG);

  return lib.GetChapter(chapter) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::GetTitleCount(UINT32* count)
{
  CheckPointer(count, E_INVALIDARG);

  (*count) = lib.GetTitles(TITLES_ALL);
  return S_OK;
}

STDMETHODIMP CBDReaderFilter::MouseMove(UINT16 x, UINT16 y)
{
  INT64 pos = -1;

  if (m_pMediaSeeking)
  {
    HRESULT hr = m_pMediaSeeking->GetCurrentPosition(&pos);
    if (SUCCEEDED(hr))
      pos = CONVERT_DS_90KHz(pos);
  }

  lib.MouseMove(CONVERT_DS_90KHz(pos), x, y);
  return S_OK;
}

BLURAY_TITLE_INFO* STDMETHODCALLTYPE CBDReaderFilter::GetTitleInfo(UINT32 pIndex)
{
  return lib.GetTitleInfo(pIndex);
}

STDMETHODIMP CBDReaderFilter::GetCurrentClipStreamInfo(BLURAY_STREAM_INFO* stream)
{
  CheckPointer(stream, E_INVALIDARG);

  (*stream) = lib.CurrentClipInfo()->video_streams[0];
 
  return S_OK;
}

STDMETHODIMP CBDReaderFilter::FreeTitleInfo(BLURAY_TITLE_INFO* info)
{
  CheckPointer(info, E_INVALIDARG);

  lib.FreeTitleInfo(info);
  return S_OK;
}

void STDMETHODCALLTYPE CBDReaderFilter::OnGraphRebuild(int info)
{
  LogDebug("CBDReaderFilter::OnGraphRebuild %d", info);
  m_bRebuildOngoing = false;
  m_demultiplexer.m_bRebuildOngoing = false;
  m_eRebuild.Set();
}

DWORD WINAPI CBDReaderFilter::CommandThreadEntryPoint(LPVOID lpParameter)
{
  return ((CBDReaderFilter*)lpParameter)->CommandThread();
}

DWORD WINAPI CBDReaderFilter::CommandThread()
{
  SetThreadName(-1, "BDReader_COMMAND");
  
  IFilterGraph* pGraph = NULL;

  pGraph = GetFilterGraph();

  if (pGraph)
  {
    pGraph->QueryInterface(&m_pMediaSeeking);
    pGraph->Release();
  }

  HANDLE handles[2];
  handles[0] = m_hStopCommandThreadEvent;
  handles[1] = m_hCommandEvent;

  if (m_pMediaSeeking)
  {
    while(1)
    {
      //DWORD result = WaitForMultipleObjects(2, handles, false, 40);
	    DWORD result = WaitForMultipleObjects(2, handles, false, INFINITE);
      if (result == WAIT_OBJECT_0) // exit event
      {
        LogDebug("CBDReaderFilter::Command thread: closing down");
        return 0;
      }
      /*
      else if (result == WAIT_TIMEOUT)
      {
        LONGLONG pos = 0;
        HRESULT hr = m_pMediaSeeking->GetCurrentPosition(&pos);
        if (SUCCEEDED(hr))
        {
          lib.ProvideUserInput(CONVERT_DS_90KHz(pos), BD_VK_NONE);
        }
      }
      */
      else if (result == WAIT_OBJECT_0 + 1) // command in queue
      {
        LONGLONG posEnd = 0;
        LONGLONG zeroPos = 0;

        m_pMediaSeeking->GetDuration(&posEnd);

        ivecCommandQueue it;
        DS_CMD cmd;

        { // just fetch the command and release the lock
          CAutoLock lock(&m_csCommandQueue);
          it = m_commandQueue.begin();
          cmd = (*it);
          m_commandQueue.erase(it);
          if (m_commandQueue.empty())
          {
            ResetEvent(m_hCommandEvent);
          }
        }

        switch (cmd.id)
        {
        case REBUILD:
          {
            LogDebug("CBDReaderFilter::Command thread: issue rebuild!");
          
            LONGLONG pos = 0;
            if (cmd.refTime.m_time < 0)
            {
              CAutoLock lock(&m_csClock);
              pos = m_rtCurrentTime;
            }
            else
              pos = cmd.refTime.m_time;

            m_eRebuild.Reset();
            TriggerOnMediaChanged();
            m_eRebuild.Wait();

            if (m_bRebuildOngoing)
            {
              LogDebug("CBDReaderFilter::Command thread: graph rebuild has failed?");
              return 0;
            }

            m_bUpdateStreamPositionOnly = true;

            LogDebug("CBDReaderFilter::Command thread: seek - pos: %06.3f (rebuild)", cmd.refTime.Millisecs() / 1000.0);
            m_pMediaSeeking->SetPositions(&pos, AM_SEEKING_AbsolutePositioning | AM_SEEKING_FakeSeek, &posEnd, AM_SEEKING_NoPositioning);

            m_eSeekDone.Wait();
            m_eSeekDone.Reset();

            m_demultiplexer.SetMediaChanging(false);
            m_demultiplexer.m_bRebuildOngoing = false;

            break;
		      }

        case SEEK:
          m_bUpdateStreamPositionOnly = true;
          
          LogDebug("CBDReaderFilter::Command thread: seek requested - pos: %06.3f", cmd.refTime.Millisecs() / 1000.0);
          HRESULT hr = m_pMediaSeeking->SetPositions((LONGLONG*)&cmd.refTime.m_time, AM_SEEKING_AbsolutePositioning | AM_SEEKING_FakeSeek, &posEnd, AM_SEEKING_NoPositioning);

          m_eSeekDone.Wait();
          m_eSeekDone.Reset();

          break;
        }
      }
      else
      {
        DWORD error = GetLastError();
        LogDebug("CBDReaderFilter::Command thread: WaitForMultipleObjects failed: %d", error);
      }
    }
  }

  return 0;
}

STDMETHODIMP CBDReaderFilter::Run(REFERENCE_TIME tStart)
{
  LogDebug("CBDReaderFilter::Run(%05.2f) state %d", tStart / 10000000.0, m_State);
  
  m_rtRun = tStart;

  CAutoLock cObjectLock(m_pLock);
  lib.SetState(State_Running);  
  
  if (m_pSubtitlePin) 
    m_pSubtitlePin->SetRunningStatus(true);
	
  HRESULT hr = CSource::Run(tStart);

  FindSubtitleFilter();
  LogDebug("CBDReaderFilter::Run(%05.2f) state %d -->done", tStart / 10000000.0, m_State);

  if (!m_hCommandThread)
    m_hCommandThread = CreateThread(NULL, 0, CBDReaderFilter::CommandThreadEntryPoint, (LPVOID)this, 0, &m_dwThreadId);

  return hr;
}

STDMETHODIMP CBDReaderFilter::Stop()
{
  CAutoLock cObjectLock(m_pLock);
  m_bStopping = true;

  LogDebug("CBDReaderFilter::Stop()");

  lib.SetState(State_Stopped);

  if (!m_bRebuildOngoing)
  {
    if (m_pSubtitlePin)
      m_pSubtitlePin->SetRunningStatus(false);

    m_demultiplexer.m_bVideoRequiresRebuild = false;
    m_demultiplexer.m_bAudioRequiresRebuild = false;
    m_demultiplexer.m_bVideoClipSeen = false;

    if (m_pVideoPin)
      m_pVideoPin->StopWait();

    if (m_demultiplexer.m_eAudioClipSeen)
      m_demultiplexer.m_eAudioClipSeen->Set();
  }
  
  LogDebug("CBDReaderFilter::Stop()  -stop source");
  HRESULT hr = CSource::Stop();
  LogDebug("CBDReaderFilter::Stop()  -stop source done");

  m_bStopping = false;

  LogDebug("CBDReaderFilter::Stop() done");

  return hr;
}

STDMETHODIMP CBDReaderFilter::Pause()
{
  LogDebug("CBDReaderFilter::Pause() - state = %d", m_State);

  CAutoLock cObjectLock(m_pLock);
  lib.SetState(State_Paused);

  HRESULT hr = CSource::Pause();

  LogDebug("CBDReaderFilter::Pause() - END - state = %d", m_State);
  return hr;
}

STDMETHODIMP CBDReaderFilter::GetDuration(REFERENCE_TIME* pDuration)
{
  if (!pDuration)
    return E_INVALIDARG;

  ULONGLONG pos = 0, dur = 0;

  if (lib.CurrentPosition(pos, dur))
    *pDuration = CONVERT_90KHz_DS(dur);
  else
    pDuration = 0;

  return NOERROR;
}

STDMETHODIMP CBDReaderFilter::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt)
{
  LogDebug("CBDReaderFilter::Load()");

  wcscpy(m_fileName, pszFileName);
  char path[4096];
  WideCharToMultiByte(CP_UTF8, 0, m_fileName, -1, path, 4096, 0, 0);

  int pathLen = strlen(path);
  int extLen = strlen("\\BDMV\\index.bdmv");
  
  // We need at least "\BDMV\index.bdmv"
  if (pathLen < extLen)
    return S_FALSE; // cannot be a path to Blu-ray

  if (stricmp(path + pathLen - 16, "\\bdmv\\index.bdmv") != 0)
    return S_FALSE; // not a BD

  strncpy(m_pathToBD, path, pathLen - extLen);
  m_pathToBD[pathLen - extLen] = '\0';
  
  if (!lib.OpenBluray(m_pathToBD))
    return VFW_E_NOT_FOUND;

  UINT32 titleCount = lib.GetTitles(TITLES_ALL);

  for (unsigned int i = 0; i < titleCount; i++)
    lib.LogTitleInfo(i, true);

  // Debugging aid - allow GraphEdit to be used
  if (0)
  {
    // Aviator
    lib.ForceTitleBasedPlayback(true);
    lib.SetTitle(5);
    Start();
  }

  return S_OK;
}

STDMETHODIMP CBDReaderFilter::Start()
{
  m_bFirstPlay = true;
  lib.Play();

  HRESULT hr = m_demultiplexer.Start();
  TriggerOnMediaChanged();
  // Close BD so the HDVM etc. are reset completely after querying the initial data.
  // This is required so we wont lose the initial events.
  lib.CloseBluray();
  m_bFirstPlay = false;
  lib.OpenBluray(m_pathToBD);
  lib.Play();

  return hr;
}

STDMETHODIMP CBDReaderFilter::GetCurFile(LPOLESTR * ppszFileName, AM_MEDIA_TYPE *pmt)
{
  CheckPointer(ppszFileName, E_POINTER);
  *ppszFileName = NULL;

  if (lstrlenW(m_fileName) > 0)
  {
    *ppszFileName = (LPOLESTR)QzTaskMemAlloc(sizeof(WCHAR) * (1 + lstrlenW(m_fileName)));
    wcscpy(*ppszFileName, m_fileName);
  }
  if (pmt)
  {
    ZeroMemory(pmt, sizeof(*pmt));
    pmt->majortype = MEDIATYPE_Stream;
    pmt->subtype = MEDIASUBTYPE_MPEG2_PROGRAM;
  }
  return S_OK;
}

ULONG CBDReaderFilter::GetMiscFlags()
{
  return AM_FILTER_MISC_FLAGS_IS_SOURCE;
}

CDeMultiplexer& CBDReaderFilter::GetDemultiplexer()
{
  return m_demultiplexer;
}

void CBDReaderFilter::Seek(REFERENCE_TIME rtAbsSeek)
{
  CAutoLock cObjectLock(m_pLock);

  LogDebug("CBDReaderFilter::--Seek()-- %6.3f, media changing %d", rtAbsSeek / 10000000.0, m_demultiplexer.IsMediaChanging());

  if (!m_bUpdateStreamPositionOnly)
  {
    LogDebug("CBDReaderFilter::Seek - Flush");
    m_demultiplexer.Flush(true, true, rtAbsSeek);
    lib.Seek(CONVERT_DS_90KHz(rtAbsSeek));
  }

  m_bUpdateStreamPositionOnly = false;

  if (m_pDVBSubtitle)
  {
    m_pDVBSubtitle->SetFirstPcr(0); // TODO: check if we need ot set the 1st PTS
    CRefTime refTime(rtAbsSeek);
    m_pDVBSubtitle->SeekDone(refTime);
  }
}

CAudioPin* CBDReaderFilter::GetAudioPin()
{
  return m_pAudioPin;
}

CVideoPin* CBDReaderFilter::GetVideoPin()
{
  return m_pVideoPin;
}

CSubtitlePin* CBDReaderFilter::GetSubtitlePin()
{
  return m_pSubtitlePin;
}

IDVBSubtitle* CBDReaderFilter::GetSubtitleFilter()
{
  return m_pDVBSubtitle;
}

void CBDReaderFilter::HandleBDEvent(BD_EVENT& pEv, UINT64 pPos)
{
  switch (pEv.event)
  {
    case BD_EVENT_SEEK:
      break;

    case BD_EVENT_STILL_TIME:
      break;

    case BD_EVENT_STILL:
      break;

    case BD_EVENT_TITLE:
      break;

    case BD_EVENT_PLAYLIST:
      break;
  }

  // Send event to the callback - filter out the none events
  if (m_pCallback && pEv.event != BD_EVENT_NONE && !m_bFirstPlay)
    m_pCallback->OnBDEvent(pEv);
}

void CBDReaderFilter::HandleOSDUpdate(OSDTexture& pTexture)
{
  if (m_pCallback)
    m_pCallback->OnOSDUpdate(pTexture);
}

/// method which implements IAMStreamSelect.Count
/// returns the number of audio streams available
STDMETHODIMP CBDReaderFilter::Count(DWORD* streamCount)
{
  *streamCount = m_demultiplexer.GetAudioStreamCount();
  return S_OK;
}

/// method which implements IAMStreamSelect.Enable
/// Sets the current audio stream to use
STDMETHODIMP CBDReaderFilter::Enable(long index, DWORD flags)
{
  return m_demultiplexer.SetAudioStream((int)index) ? S_OK : S_FALSE;
}

/// method which implements IAMStreamSelect.Info
/// returns an array of all audio streams available
STDMETHODIMP CBDReaderFilter::Info(long lIndex, AM_MEDIA_TYPE**ppmt, DWORD* pdwFlags, LCID* plcid, DWORD* pdwGroup, WCHAR** ppszName, IUnknown** ppObject, IUnknown** ppUnk)
{
  if (pdwFlags)
  {
    int audioIndex = 0;
    m_demultiplexer.GetAudioStream(audioIndex);

    //if (m_demultiplexer.GetAudioStream()==(int)lIndex)
    if (audioIndex == (int)lIndex)
      *pdwFlags = AMSTREAMSELECTINFO_EXCLUSIVE;
    else
      *pdwFlags = 0;
  }
  if (plcid) *plcid = 0;
  if (pdwGroup) *pdwGroup = m_demultiplexer.GetAudioStreamType((int)lIndex); //*pdwGroup = 1;
  if (ppObject) *ppObject = NULL;
  if (ppUnk) *ppUnk = NULL;
  if (ppszName)
  {
    char szName[40];
    m_demultiplexer.GetAudioStreamInfo((int)lIndex, szName);
    *ppszName = (WCHAR *)CoTaskMemAlloc(20);
    MultiByteToWideChar(CP_ACP, 0, szName, -1, *ppszName, 20);
  }
  if (ppmt)
  {
    CMediaType mediaType;
    m_demultiplexer.GetAudioStreamPMT(mediaType);
    AM_MEDIA_TYPE* mType = (AM_MEDIA_TYPE*)(&mediaType);
    *ppmt = (AM_MEDIA_TYPE*)CoTaskMemAlloc(sizeof(AM_MEDIA_TYPE));
    if (*ppmt)
    {
      memcpy(*ppmt, mType, sizeof(AM_MEDIA_TYPE));
      (*ppmt)->pbFormat = (BYTE*)CoTaskMemAlloc(mediaType.FormatLength());
      memcpy((*ppmt)->pbFormat, mType->pbFormat, mediaType.FormatLength());
    }
    else
      return S_FALSE;
  }
  return S_OK;
}

// IAudioStream methods
STDMETHODIMP CBDReaderFilter::GetAudioStream(__int32 &stream)
{
  return m_demultiplexer.GetAudioStream(stream) ? S_OK : S_FALSE;
}

// ISubtitleStream methods
STDMETHODIMP CBDReaderFilter::SetSubtitleStream(__int32 stream)
{
  return m_demultiplexer.SetSubtitleStream(stream) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::GetSubtitleStreamLanguage(__int32 stream, char* szLanguage)
{
  return m_demultiplexer.GetSubtitleStreamLanguage(stream, szLanguage) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::GetSubtitleStreamType(__int32 stream, int &type)
{
  return m_demultiplexer.GetSubtitleStreamType(stream, type) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::GetSubtitleStreamCount(__int32 &count)
{
  return m_demultiplexer.GetSubtitleStreamCount(count) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::GetCurrentSubtitleStream(__int32 &stream)
{
  return m_demultiplexer.GetCurrentSubtitleStream(stream) ? S_OK : S_FALSE;
}

STDMETHODIMP CBDReaderFilter::SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int c, void* opts, int* select))
{
  return m_demultiplexer.SetSubtitleResetCallback(pSubUpdateCallback) ? S_OK : S_FALSE;
}

HRESULT CBDReaderFilter::FindSubtitleFilter()
{
  if (m_pDVBSubtitle)
    return S_OK;

  HRESULT hr = S_FALSE;
  ULONG fetched = 0;

  IEnumFilters * piEnumFilters = NULL;
  if (GetFilterGraph() && SUCCEEDED(GetFilterGraph()->EnumFilters(&piEnumFilters)))
  {
    IBaseFilter * pFilter;
    while (piEnumFilters->Next(1, &pFilter, &fetched) == NOERROR)
    {
      FILTER_INFO filterInfo;
      if (pFilter->QueryFilterInfo(&filterInfo) == S_OK)
      {
        if (!wcsicmp(L"MediaPortal DVBSub3", filterInfo.achName))
          hr = pFilter->QueryInterface(IID_IDVBSubtitle3, (void**)&m_pDVBSubtitle);

        filterInfo.pGraph->Release();
      }
      pFilter->Release();
      pFilter = NULL;
    }
    piEnumFilters->Release();
  }

  return hr;
}

bool CBDReaderFilter::IsStopping()
{
  return m_bStopping;
}

void CBDReaderFilter::ForceTitleBasedPlayback(bool force, UINT32 pTitle)
{
  lib.ForceTitleBasedPlayback(force);
  lib.SetTitle(pTitle);
}

void CBDReaderFilter::SetD3DDevice(IDirect3DDevice9* device)
{
  lib.SetD3DDevice(device);
}

void CBDReaderFilter::SetBDPlayerSettings(bd_player_settings settings)
{	
  lib.SetBDPlayerSettings(settings);
}

STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2(TRUE);
} 

STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2(FALSE);
} 

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, DWORD  dwReason, LPVOID lpReserved)
{
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

STDMETHODIMP CBDReaderFilter::SetPositions(LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags)
{
  return SetPositionsInternal(this, pCurrent, dwCurrentFlags, pStop, dwStopFlags);
}

STDMETHODIMP CBDReaderFilter::SetPositionsInternal(void *caller, LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags)
{
#ifdef LOG_SEEK_INFORMATION
  LogDebug("  ::SetPositions() - seek request - current: %I64d; start: %I64d; stop: %I64d; flags: %ul m_bRebuildOngoing: %d m_bStopping: %d", 
    m_rtCurrent, pCurrent ? *pCurrent : -1, pStop ? *pStop : -1, dwStopFlags, m_bRebuildOngoing, m_bStopping);
#endif
  CAutoLock cAutoLock(this);

  // - Graph rebuild triggers seeking when stopping the graph - ignore, we are going to seek
  // to a correct position after the rebuild has been done
  //
  // - Ignore seek request when playback is stopping
  if (m_bRebuildOngoing || m_bStopping)
  {
    m_eSeekDone.Set();
    return S_OK;
  }

  if (!pCurrent && !pStop
    || (dwCurrentFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning 
    && (dwStopFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning) 
  {
    m_eSeekDone.Set();
    return S_OK;
  }

  REFERENCE_TIME
    rtCurrent = m_rtCurrent,
    rtStop = m_rtStop;


  bool fakeSeek = (dwCurrentFlags & AM_SEEKING_FakeSeek) == AM_SEEKING_FakeSeek;
  bool resetStreamPosition = caller == m_pVideoPin && fakeSeek;

  if (pCurrent) 
  {
    switch(dwCurrentFlags&AM_SEEKING_PositioningBitsMask)
    {
    case AM_SEEKING_NoPositioning: break;
    case AM_SEEKING_AbsolutePositioning: rtCurrent = *pCurrent; break;
    case AM_SEEKING_RelativePositioning: rtCurrent = rtCurrent + *pCurrent; break;
    case AM_SEEKING_IncrementalPositioning: rtCurrent = rtCurrent + *pCurrent; break;
    }
  }

  if (pStop)
  {
    switch(dwStopFlags&AM_SEEKING_PositioningBitsMask)
    {
    case AM_SEEKING_NoPositioning: break;
    case AM_SEEKING_AbsolutePositioning: rtStop = *pStop; break;
    case AM_SEEKING_RelativePositioning: rtStop += *pStop; break;
    case AM_SEEKING_IncrementalPositioning: rtStop = rtCurrent + *pStop; break;
    }
  }

  // Allow consecutive fake seeks to be done to the zero position
  bool alreadySeekedPos = m_rtCurrent == rtCurrent && m_rtStop == rtStop && !resetStreamPosition && !m_bFirstSeek;
  m_bFirstSeek = false;
  
  if (alreadySeekedPos)
  {
#ifdef LOG_SEEK_INFORMATION   
    LogDebug("  ::SetPositions() - already seeked to pos - mark 1");
#endif

    m_eSeekDone.Set();
    return S_OK;
  }

  if (alreadySeekedPos && m_lastSeekers.find(caller) == m_lastSeekers.end()) 
  {
#ifdef LOG_SEEK_INFORMATION   
    LogDebug("  ::SetPositions() - already seeked to pos - mark 2");
#endif

    m_eSeekDone.Set();
    m_lastSeekers.insert(caller);
    return S_OK;
  }

  m_rtLastStart = rtCurrent;
  m_rtLastStop = rtStop;
  m_lastSeekers.clear();
  m_lastSeekers.insert(caller);

  m_rtNewStart = m_rtCurrent = rtCurrent;
  m_rtNewStop = rtStop;

  if (ThreadExists())
  {
    m_eEndNewSegment.Reset();
    
    DeliverBeginFlush();
    CallWorker(CMD_SEEK);
    DeliverEndFlush();

    // Do not allow new seek to happen before the old one is completed 
    m_eEndNewSegment.Wait();
  }
  else
  {
#ifdef LOG_SEEK_INFORMATION   
    LogDebug("  ::SetPositions() - no ThreadExists()");
#endif
  }

  m_eSeekDone.Set();

  return S_OK;
}

void CBDReaderFilter::DeliverBeginFlush()
{
  m_bFlushing = true;

  if (m_pVideoPin && m_pVideoPin->IsConnected())
  {
    m_pVideoPin->DeliverBeginFlush();
    m_pVideoPin->Stop();
  }

  if (m_pAudioPin && m_pAudioPin->IsConnected())
  {
    m_pAudioPin->DeliverBeginFlush();
    m_pVideoPin->Stop();
  }

  if (m_pSubtitlePin && m_pSubtitlePin->IsConnected())
  {
    m_pSubtitlePin->DeliverBeginFlush();
    m_pVideoPin->Stop();
  }
}

void CBDReaderFilter::DeliverEndFlush()
{
  if (m_pVideoPin && m_pVideoPin->IsConnected())
  {
    m_pVideoPin->DeliverEndFlush();
    m_pVideoPin->Pause();
  }

  if (m_pAudioPin && m_pAudioPin->IsConnected())
  {
    m_pAudioPin->DeliverEndFlush();
    m_pAudioPin->Pause();
  }

  if (m_pSubtitlePin && m_pSubtitlePin->IsConnected())
  {
    m_pSubtitlePin->DeliverEndFlush();
    m_pSubtitlePin->Pause();
  }

  m_bFlushing = false;
  m_eEndFlush.Set();
}

DWORD CBDReaderFilter::ThreadProc()
{
  SetThreadName(-1, "BDReader_WORKER");

  m_bFlushing = false;
  m_eEndFlush.Set();

  for (DWORD cmd = (DWORD)-1; ; cmd = GetRequest())
  {
    if (cmd == CMD_EXIT)
    {
      m_hThread = NULL;
      Reply(S_OK);
      return 0;
    }

    SetThreadPriority(m_hThread, THREAD_PRIORITY_NORMAL);

    m_rtStart = m_rtNewStart;
    m_rtStop = m_rtNewStop;

    if (cmd == CMD_SEEK)
      Seek(m_rtStart);

    if (cmd != (DWORD)-1)
      Reply(S_OK);

    // Wait for the end of any flush
    m_eEndFlush.Wait();

    if (cmd == CMD_SEEK)
    {
      m_pVideoPin->DeliverNewSegment(m_rtStart, m_rtStop, m_dRate);
      m_pAudioPin->DeliverNewSegment(m_rtStart, m_rtStop, m_dRate);
      m_pSubtitlePin->DeliverNewSegment(m_rtStart, m_rtStop, m_dRate);

      m_eEndNewSegment.Set();
    }

    while (!CheckRequest(&cmd)) 
      Sleep(5); // TODO remove polling or maybe start to use the worker thread for pre-caching data

    // If we didnt exit by request, deliver end-of-stream
    if (!CheckRequest(&cmd)) 
    {
      m_pAudioPin->EndOfStream();
      m_pVideoPin->EndOfStream();
      m_pSubtitlePin->EndOfStream();
    }
  }
  ASSERT(0); // we should only exit via CMD_EXIT

  m_hThread = NULL;
  return 0;
}