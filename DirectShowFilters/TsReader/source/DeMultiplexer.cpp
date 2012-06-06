/*
 *  Copyright (C) 2005-2009 Team MediaPortal
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
#include <afx.h>
#include <afxwin.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <wmcodecdsp.h>
#include "demultiplexer.h"
#include "buffer.h"
#include "..\..\shared\adaptionfield.h"
#include "tsreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "subtitlePin.h"
#include "..\..\DVBSubtitle3\Source\IDVBSub.h"
#include "mediaFormats.h"
#include "h264nalu.h"
#include <cassert>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_AUD_BUF_SIZE 1024
#define MAX_VID_BUF_SIZE 640
#define MAX_SUB_BUF_SIZE 640
#define BUFFER_LENGTH 0x1000
#define READ_SIZE (65536)
#define MIN_READ_SIZE (READ_SIZE/8)
#define MIN_READ_SIZE_UNC (READ_SIZE/4)
#define INITIAL_READ_SIZE (READ_SIZE * 512)

//Macro borrowed from MPC-HC/LAV splitter...
#define MOVE_TO_H264_START_CODE(b, e) while(b <= e-4 && !((*(DWORD *)b == 0x01000000) || ((*(DWORD *)b & 0x00FFFFFF) == 0x00010000))) b++; if((b <= e-4) && *(DWORD *)b == 0x01000000) b++;

// uncomment the //LogDebug to enable extra logging
#define LOG_SAMPLES //LogDebug
#define LOG_OUTSAMPLES //LogDebug

extern void LogDebug(const char *fmt, ...);
extern DWORD m_tGTStartTime;

// *** UNCOMMENT THE NEXT LINE TO ENABLE DYNAMIC VIDEO PIN HANDLING!!!! ******
#define USE_DYNAMIC_PINS


CDeMultiplexer::CDeMultiplexer(CTsDuration& duration,CTsReaderFilter& filter)
:m_duration(duration)
,m_filter(filter)
{
  //Initialise m_tGTStartTime for GET_TIME_NOW() macro.
  //The macro is used to avoid having to handle timeGetTime()
  //rollover issues in the body of the code
  m_tGTStartTime = (timeGetTime() - 0x40000000); 

  m_patParser.SetCallBack(this);
  m_pCurrentAudioBuffer = new CBuffer();
  m_pCurrentSubtitleBuffer = new CBuffer();
  m_iAudioStream = 0;
  m_AudioStreamType = SERVICE_TYPE_AUDIO_UNKNOWN;
  m_iSubtitleStream = 0;
  m_audioPid = 0;
  m_currentSubtitlePid = 0;
  m_bEndOfFile = false;
  m_bShuttingDown = false;
  m_iAudioIdx = -1;
  m_iPatVersion = -1;
  m_ReqPatVersion = -1;
  m_bPatParsed = false;
  m_bWaitGoodPat = false;
  m_receivedPackets = 0;
  m_bSetAudioDiscontinuity = false;
  m_bSetVideoDiscontinuity = false;
  m_reader = NULL;
  pTeletextEventCallback = NULL;
  pSubUpdateCallback = NULL;
  pTeletextPacketCallback = NULL;
  pTeletextServiceInfoCallback = NULL;
  m_iAudioReadCount = 0;
  m_lastVideoPTS.IsValid = false;
  m_lastAudioPTS.IsValid = false;
  m_bFlushDelegated = false;
  m_bFlushDelgNow = false;
  m_bFlushRunning = false; 
  m_bStarting=false;
  m_bReadAheadFromFile = false;
  m_mpegParserTriggerFormatChange = false;
  m_mpegParserReset = true;
  m_videoChanged=false;
  m_audioChanged=false;
  m_DisableDiscontinuitiesFiltering = false;

  m_AudioPrevCC = -1;
  m_FirstAudioSample = 0x7FFFFFFF00000000LL;
  m_LastAudioSample = 0;

  m_WaitHeaderPES=-1 ;
  m_VideoPrevCC = -1;
  m_bFirstGopFound = false;
  m_bSecondGopFound = false;
  m_bFrame0Found = false;
  m_bFirstGopParsed = false;
  m_lastVidResX=-1 ;
  m_lastVidResY=-1 ;
  m_FirstVideoSample = 0x7FFFFFFF00000000LL;
  m_LastVideoSample = 0;
  
  m_LastDataFromRtsp = GET_TIME_NOW();
  m_targetAVready = m_LastDataFromRtsp;
  m_tWaitForMediaChange=m_LastDataFromRtsp ;
  m_tWaitForAudioSelection=m_LastDataFromRtsp;
  m_bWaitForMediaChange=false;
  m_bWaitForAudioSelection=false;
  m_bSubtitleCompensationSet=false;

  m_mpegPesParser = new CMpegPesParser();
  
  LogDebug("demux: Start file read thread");
  StartThread();
}

CDeMultiplexer::~CDeMultiplexer()
{
  LogDebug("CDeMultiplexer::dtor");
  m_bShuttingDown = true;
  //stop file read thread
  StopThread();
  Flush(true);
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;
  delete m_mpegPesParser;

  m_subtitleStreams.clear();
  m_audioStreams.clear();
  LogDebug("CDeMultiplexer::dtor - finished");
}

int CDeMultiplexer::GetVideoServiceType()
{
  if(m_pids.videoPids.size() > 0)
  {
    return m_pids.videoPids[0].VideoServiceType;
  }
  else
  {
    return SERVICE_TYPE_VIDEO_UNKNOWN;
  }
}

void CDeMultiplexer::SetFileReader(FileReader* reader)
{
  m_reader = reader;
}

CPidTable CDeMultiplexer::GetPidTable()
{
  return m_pids;
}


/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed
bool CDeMultiplexer::SetAudioStream(int stream)
{
  CAutoLock lock (&m_sectionSetAudioStream);
  LogDebug("SetAudioStream : %d",stream);
  //is stream index valid?
  if (stream < 0 || stream >= m_audioStreams.size())
    return S_FALSE;

  //set index
  m_iAudioStream = stream;

  //get the new audio stream type
  int newAudioStreamType = SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream >= 0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType = m_audioStreams[m_iAudioStream].audioType;
  }

  LogDebug("Old Audio %d, New Audio %d", m_AudioStreamType, newAudioStreamType);
  //did it change?
  if ((m_AudioStreamType == SERVICE_TYPE_AUDIO_UNKNOWN) || (m_AudioStreamType != newAudioStreamType))
  {
    m_AudioStreamType = newAudioStreamType;
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {
	  // here, stream is not parsed yet
      if (!IsMediaChanging())             
      {
        LogDebug("SetAudioStream : OnMediaTypeChanged(AUDIO_CHANGE)");
        //Flush(true) ;                   
        //Flushing is delegated to CDeMultiplexer::ThreadProc()
        if (!m_bFlushDelgNow && !m_bFlushRunning) //Flush already pending
        {
          m_bFlushDelgNow = true;
          WakeThread(); 
        }
        m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
        SetMediaChanging(true);
        m_filter.m_bForceSeekOnStop=true;     // Force stream to be resumed after
      }
      else   // Mpeg parser info is required or audio graph is already rebuilding.
      {
        LogDebug("SetAudioStream : Media already changing");   // just wait 1st GOP
        m_audioChanged = true;
      }
    }
  }
  else
  {
    m_filter.GetAudioPin()->SetAddPMT();
    m_filter.GetAudioPin()->SetDiscontinuity(true);
  }

  SetAudioChanging(false);
  return S_OK;
}

bool CDeMultiplexer::GetAudioStream(__int32 &audioIndex)
{
  audioIndex = m_iAudioStream;
  return S_OK;
}

void CDeMultiplexer::GetAudioStreamInfo(int stream,char* szName)
{
  if (stream < 0 || stream>=m_audioStreams.size())
  {
    szName[0] = szName[1] = szName[2] = 0;
    return;
  }
  szName[0] = m_audioStreams[stream].language[0];
  szName[1] = m_audioStreams[stream].language[1];
  szName[2] = m_audioStreams[stream].language[2];
  szName[3] = m_audioStreams[stream].language[3];
  szName[4] = m_audioStreams[stream].language[4];
  szName[5] = m_audioStreams[stream].language[5];
  szName[6] = m_audioStreams[stream].language[6];  
}
int CDeMultiplexer::GetAudioStreamCount()
{
  return m_audioStreams.size();
}

void CDeMultiplexer::GetAudioStreamType(int stream,CMediaType& pmt)
{
  if (m_iAudioStream< 0 || stream >= m_audioStreams.size())
  {
    pmt.InitMediaType();
    pmt.SetType      (& MEDIATYPE_Audio);
    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
    pmt.SetSampleSize(1);
    pmt.SetTemporalCompression(FALSE);
    pmt.SetVariableSize();
    pmt.SetFormatType(&FORMAT_WaveFormatEx);
    pmt.SetFormat(MPEG2AudioFormat,sizeof(MPEG2AudioFormat));
    return;
  }

  switch (m_audioStreams[stream].audioType)
  {
    // MPEG1 shouldn't be mapped to MPEG2 audio as it will break Cyberlink audio codec
    // (and MPA is not working with the MPEG1 to MPEG2 mapping...)
    case SERVICE_TYPE_AUDIO_MPEG1:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_MPEG1Payload);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
      break;
  case SERVICE_TYPE_AUDIO_MPEG2:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG2AudioFormat,sizeof(MPEG2AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AAC:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AACAudioFormat,sizeof(AACAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_LATM_AAC:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_LATM_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AACAudioFormat,sizeof(AACAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AC3:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_DOLBY_AC3);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AC3AudioFormat,sizeof(AC3AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_DD_PLUS:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_DOLBY_DDPLUS);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AC3AudioFormat,sizeof(AC3AudioFormat));
      break;
  }
}
// This methods selects the subtitle stream specified
bool CDeMultiplexer::SetSubtitleStream(__int32 stream)
{
  //is stream index valid?
  if (stream < 0 || stream >= m_subtitleStreams.size())
    return S_FALSE;

  //set index
  m_iSubtitleStream=stream;
  return S_OK;
}

bool CDeMultiplexer::GetCurrentSubtitleStream(__int32 &stream)
{

  stream = m_iSubtitleStream;
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamLanguage(__int32 stream,char* szLanguage)
{
  if (stream <0 || stream >= m_subtitleStreams.size())
  {
    szLanguage[0] = szLanguage[1] = szLanguage[2] = 0;
    return S_FALSE;
  }
  szLanguage[0] = m_subtitleStreams[stream].language[0];
  szLanguage[1] = m_subtitleStreams[stream].language[1];
  szLanguage[2] = m_subtitleStreams[stream].language[2];
  szLanguage[3] = m_subtitleStreams[stream].language[3];

  return S_OK;
}
bool CDeMultiplexer::GetSubtitleStreamCount(__int32 &count)
{
  count = m_subtitleStreams.size();
  return S_OK;
}

bool CDeMultiplexer::SetSubtitleResetCallback(int(CALLBACK *cb)(int, void*, int*))
{
  pSubUpdateCallback = cb;
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamType(__int32 stream, __int32 &type)
{
  if (m_iSubtitleStream< 0 || m_iSubtitleStream >= m_subtitleStreams.size())
  {
    // invalid stream number
    return S_FALSE;
  }

  type = m_subtitleStreams[m_iSubtitleStream].subtitleType;
  return S_OK;
}

bool CDeMultiplexer::GetVideoStreamType(CMediaType &pmt)
{
  if( m_pids.videoPids.size() != 0 && m_mpegPesParser != NULL && m_bFirstGopParsed)
  {
    CAutoLock lock (&m_mpegPesParser->m_sectionVideoPmt);
    pmt = m_mpegPesParser->pmt;
    return true;
  }
  return false;
}

void CDeMultiplexer::FlushVideo()
{
  LogDebug("demux:flush video");
  CAutoLock flock (&m_sectionFlushVideo);
  CAutoLock lock (&m_sectionVideo);
  ivecBuffers it = m_vecVideoBuffers.begin();
  while (it != m_vecVideoBuffers.end())
  {
    CBuffer* videoBuffer = *it;
    delete videoBuffer;
    it = m_vecVideoBuffers.erase(it);
  }
    
  m_p.Free();
  m_lastStart = 0;
  m_pl.RemoveAll();
  m_fHasAccessUnitDelimiters = false;

  m_VideoPrevCC = -1;
  m_bFirstGopFound = false;
  m_bSecondGopFound = false;
  m_bFrame0Found = false;
  m_mpegParserReset = true;
  m_FirstVideoSample = 0x7FFFFFFF00000000LL;
  m_LastVideoSample = 0;
  m_lastVideoPTS.IsValid = false;
  m_VideoValidPES = false;
  m_mVideoValidPES = false;
  m_WaitHeaderPES=-1 ;
  m_bVideoAtEof=false;
  m_MinVideoDelta = 10.0 ;
  _InterlockedAnd(&m_AVDataLowCount, 0) ;
  m_filter.m_bRenderingClockTooFast=false ;
  m_bSetVideoDiscontinuity=true;
  m_bVideoSampleLate=false;
  
  Reset();  // PacketSync reset.
}

void CDeMultiplexer::FlushAudio()
{
  LogDebug("demux:flush audio");
  CAutoLock flock (&m_sectionFlushAudio);
  CAutoLock lock (&m_sectionAudio);
  delete m_pCurrentAudioBuffer;
  ivecBuffers it = m_vecAudioBuffers.begin();
  while (it != m_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer = *it;
    delete AudioBuffer;
    it = m_vecAudioBuffers.erase(it);
  }
  // Clear PES temporary queue.
  it = m_t_vecAudioBuffers.begin();
  while (it != m_t_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer=*it;
    delete AudioBuffer;
    it=m_t_vecAudioBuffers.erase(it);
  }

  m_AudioPrevCC = -1;
  m_FirstAudioSample = 0x7FFFFFFF00000000LL;
  m_LastAudioSample = 0;
  m_lastAudioPTS.IsValid = false;
  m_AudioValidPES = false;
  m_pCurrentAudioBuffer = new CBuffer();
  m_bAudioAtEof = false;
  m_MinAudioDelta = 10.0;
  _InterlockedAnd(&m_AVDataLowCount, 0);
  m_filter.m_bRenderingClockTooFast=false;
  m_bSetAudioDiscontinuity=true;
  m_bAudioSampleLate=false;
  
  Reset();  // PacketSync reset.
}

void CDeMultiplexer::FlushSubtitle()
{
  LogDebug("demux:flush subtitle");
  CAutoLock flock (&m_sectionFlushSubtitle);
  CAutoLock lock (&m_sectionSubtitle);
  delete m_pCurrentSubtitleBuffer;
  ivecBuffers it = m_vecSubtitleBuffers.begin();
  while (it != m_vecSubtitleBuffers.end())
  {
    CBuffer* subtitleBuffer = *it;
    delete subtitleBuffer;
    it = m_vecSubtitleBuffers.erase(it);
  }
  m_pCurrentSubtitleBuffer = new CBuffer();
}

/// Flushes all buffers
void CDeMultiplexer::Flush(bool clearAVready)
{
  if (m_bFlushRunning) return;
    
  LogDebug("demux:flushing");

  m_bFlushRunning = true; //Stall GetVideo()/GetAudio()/GetSubtitle() calls from pins 

  if (!m_bShuttingDown)
  {
    //Wait for output pin data sample delivery to stop - timeout after 100 loop iterations in case pin delivery threads are stalled
    int i = 0;
    while ((i < 100) && (m_filter.GetAudioPin()->IsInFillBuffer() || m_filter.GetVideoPin()->IsInFillBuffer() || m_filter.GetSubtitlePin()->IsInFillBuffer()) )
    {
      Sleep(5);
      i++;
    }
    if (i >= 100)
    {
      LogDebug("demux: Flush: InFillBuffer() wait timeout, %d %d %d", m_filter.GetAudioPin()->IsInFillBuffer(), m_filter.GetVideoPin()->IsInFillBuffer(), m_filter.GetSubtitlePin()->IsInFillBuffer());
    }
  }

  m_iAudioReadCount = 0;
  m_LastDataFromRtsp = GET_TIME_NOW();
  FlushAudio();
  FlushVideo();
  FlushSubtitle();
  m_bFlushDelegated = false;
  m_bReadAheadFromFile = false;  
  
  if (clearAVready)
  {
    m_filter.m_bStreamCompensated=false ;
  }
  
  m_bFlushRunning = false;
}

///
///Returns the next subtitle packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetSubtitle()
{
  if (m_bFlushDelgNow || m_bFlushRunning || m_bStarting) return NULL; //Flush pending or Start() active
  if (m_filter.GetSubtitlePin()->IsConnected() && (m_iAudioStream == -1) || IsAudioChanging()) return NULL;

  if ((m_pids.subtitlePids.size() > 0 && m_pids.subtitlePids[0].Pid==0) || IsMediaChanging())
  {
    if (CheckPrefetchState(false, true))
    {
      //Read some data
      m_bReadAheadFromFile = true;
      WakeThread();
    }
    return NULL;
  }
  //if there is no subtitle pid, then simply return NULL
  if (m_currentSubtitlePid==0) return NULL;
  if (m_bEndOfFile) return NULL;
  
  CAutoLock lock (&m_sectionSubtitle);
  
  //are there subtitle packets in the buffer?
  if (m_vecSubtitleBuffers.size()!=0 )
  {
    //yup, then return the next one
    ivecBuffers it =m_vecSubtitleBuffers.begin();
    CBuffer* subtitleBuffer=*it;
    m_vecSubtitleBuffers.erase(it);
    return subtitleBuffer;
  }
  //no subtitle packets available
  return NULL;
}

///
///Returns the next video packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetVideo(bool earlyStall)
{
  //CAutoLock flock (&m_sectionFlushVideo);
  if (m_bFlushDelgNow || m_bFlushRunning || m_bStarting) return NULL; //Flush pending or Start() active 
  if (m_filter.GetVideoPin()->IsConnected() && (m_iAudioStream == -1) || IsAudioChanging()) return NULL;

  //if there is no video pid, then simply return NULL
  if ((m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid==0) || IsMediaChanging())
  {
    if (CheckPrefetchState(false, true))
    {
      //Read some data
      m_bReadAheadFromFile = true;
      WakeThread();
    }
    return NULL;
  }

  if (CheckPrefetchState(!earlyStall, false))
  {
    //Read some data
    m_bReadAheadFromFile = true;
    WakeThread();
  }

  //We should have a video packet available
  CAutoLock lock (&m_sectionVideo);

  if (m_vecVideoBuffers.size() > 0)
  {
    ivecBuffers it = m_vecVideoBuffers.begin();
    CBuffer* videoBuffer = *it;
    return videoBuffer;
  }

  //no video packets available
  return NULL;
}

//Free a video buffer after use
void CDeMultiplexer::EraseVideoBuff()
{
  CAutoLock lock (&m_sectionVideo);
  
  if (m_vecVideoBuffers.size() > 0)
  {
    ivecBuffers it = m_vecVideoBuffers.begin();
    m_vecVideoBuffers.erase(it);
  }
}

///
///Returns the next audio packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetAudio(bool earlyStall, CRefTime rtStartTime)
{
  if (m_bFlushDelgNow || m_bFlushRunning || m_bStarting) return NULL; //Flush pending or Start() active
  if ((m_iAudioStream == -1) || IsAudioChanging()) return NULL;

  // if there is no audio pid, then simply return NULL
  if ((m_audioPid==0) || IsMediaChanging())
  {
    if (CheckPrefetchState(false, true))
    {
      //Read some data
      m_bReadAheadFromFile = true;
      WakeThread();
    }
    return NULL;
  }

  if (CheckPrefetchState(!earlyStall, false))
  {
    //Read some data
    m_bReadAheadFromFile = true;
    WakeThread();
  }

  if (!CheckCompensation(rtStartTime)) 
  {
    return NULL; //Not enough audio/video to start
  }

  //Return the next buffer
  CAutoLock lock (&m_sectionAudio);

  if (m_vecAudioBuffers.size() > 0)
  {
    ivecBuffers it = m_vecAudioBuffers.begin();
    CBuffer* audiobuffer = *it;
    return audiobuffer;
  }
  
  return NULL;
}

//Free an audio buffer after use
void CDeMultiplexer::EraseAudioBuff()
{
  CAutoLock lock (&m_sectionAudio);
  
  if (m_vecAudioBuffers.size() > 0)
  {
    ivecBuffers it = m_vecAudioBuffers.begin();
    m_vecAudioBuffers.erase(it);
  }
}

bool CDeMultiplexer::CheckCompensation(CRefTime rtStartTime)
{
  if (!m_filter.m_bStreamCompensated && (m_vecAudioBuffers.size()>0))
  {
    int cntA,cntV ;
    CRefTime firstAudio, lastAudio;
    CRefTime firstVideo, lastVideo;
    cntA = GetAudioBufferPts(firstAudio, lastAudio); // this one...
    cntV = GetVideoBufferPts(firstVideo, lastVideo);
    
    // Goal is to start with at least 500mS audio and 400mS video ahead. ( LiveTv and RTSP as TsReader cannot go ahead by itself)
    if (lastAudio.Millisecs() - firstAudio.Millisecs() < (310+INITIAL_BUFF_DELAY)) return false ;       // Not enough audio to start.

    if (m_filter.GetVideoPin()->IsConnected())
    {
      if (!m_bFrame0Found) return NULL ;
        
      //if (lastVideo.Millisecs() - firstVideo.Millisecs() < 310) return NULL ;   // Not enough video to start.
      if (lastVideo.Millisecs() - firstVideo.Millisecs() < (210+INITIAL_BUFF_DELAY)) return false ;   // Not enough video to start.
      
      if (!m_filter.m_EnableSlowMotionOnZapping)
      {
        if (lastAudio.Millisecs() - firstVideo.Millisecs() < 10) return false ;   // Not enough simultaneous audio & video to start.
      }       
    }

    // Ambass : Find out the best compensation to apply in order to have fast Audio/Video delivery
    CRefTime BestCompensation ;
    CRefTime AddVideoCompensation ;
    LogDebug("Audio Samples : %d, First : %03.3f, Last : %03.3f",cntA, (float)firstAudio.Millisecs()/1000.0f,(float)lastAudio.Millisecs()/1000.0f);
    LogDebug("Video Samples : %d, First : %03.3f, Last : %03.3f",cntV, (float)firstVideo.Millisecs()/1000.0f,(float)lastVideo.Millisecs()/1000.0f);
              
    if (m_filter.GetVideoPin()->IsConnected())
    {
      if (firstAudio.Millisecs() < firstVideo.Millisecs())
      {
        CRefTime targFirstAudio = ((firstVideo - firstAudio) > (500*10000)) ? (firstVideo - (500*10000)) : firstAudio; //Limit to 500ms difference
          
        if (targFirstAudio > (lastAudio-(100*10000))) //Make sure there is an audio sample available at the start
        {
          targFirstAudio = lastAudio-(100*10000);
        }
        
        BestCompensation = targFirstAudio - m_filter.m_RandomCompensation - rtStartTime ;
        AddVideoCompensation = firstVideo - targFirstAudio;
        AddVideoCompensation = (AddVideoCompensation > (2000*10000)) ? (2000*10000) : AddVideoCompensation; //Limit to 2.0 seconds
        LogDebug("Compensation : ( Rnd : %d mS ) Audio pts ahead Video pts . Add %03.3f sec of extra video comp to start now !...",(DWORD)m_filter.m_RandomCompensation/10000,(float)AddVideoCompensation.Millisecs()/1000.0f) ;       
      }
      else
      {
        BestCompensation = firstAudio-rtStartTime ;
        AddVideoCompensation = 0 ;
        LogDebug("Compensation : Audio pts behind Video Pts ( Recover skipping Video ) ....") ;
      }
      m_filter.m_RandomCompensation += 500000 ;   // Stupid feature required to have FFRW working with DVXA ( at least ATI.. ) to avoid frozen picture. ( it just moves the sample time a bit !! )
      m_filter.m_RandomCompensation = m_filter.m_RandomCompensation % 1000000 ;
    }
    else
    {
      BestCompensation = firstAudio-rtStartTime;
      AddVideoCompensation = 0 ;
    }

    // Here, clock filter should be running, but : EVR  is generally running and need to be compensated accordingly
    //                                             with current elapsed time from "RUN" to avoid beeing late.
    //                                             VMR9 is generally waiting I-frame + 1 or 2 frames to start clock and "RUN"
    // Apply a margin of 200 mS seems safe to avoid being late.
    if (m_filter.State() == State_Running)
    {
      REFERENCE_TIME RefClock = 0;
      m_filter.GetMediaPosition(&RefClock) ;

      m_filter.m_ClockOnStart = RefClock - rtStartTime.m_time ;
      if (m_filter.m_bLiveTv)
      {
        LogDebug("CheckCompensation() - Elapsed time from pause to Audio/Video ( total zapping time ) : %d mS",GET_TIME_NOW()-m_filter.m_lastPause);
      }
    }
    else
    {
      m_filter.m_ClockOnStart=0 ;
    }

    // set the current compensation
    CRefTime compTemp;
    compTemp.m_time=(BestCompensation.m_time - m_filter.m_ClockOnStart.m_time) - PRESENT_DELAY ;
    m_filter.SetCompensation(compTemp);
    m_filter.AddVideoComp = (AddVideoCompensation < 0) ? 0 : AddVideoCompensation;

    LogDebug("demux:Compensation:%03.3f, Clock on start %03.3f m_rtStart:%d ",(float)m_filter.Compensation.Millisecs()/1000.0f, m_filter.m_ClockOnStart.Millisecs()/1000.0f, rtStartTime.Millisecs());

    //set flag to false so we dont keep compensating
    m_filter.m_bStreamCompensated = true;
    m_bSubtitleCompensationSet = false;

    m_targetAVready = GET_TIME_NOW() + AV_READY_DELAY;
  }

  // Subtitle filter is "found" only after Run() has been completed
  if(!m_bSubtitleCompensationSet)
  {
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if(pDVBSubtitleFilter)
    {
      LogDebug("demux:pDVBSubtitleFilter->SetTimeCompensation");
      pDVBSubtitleFilter->SetTimeCompensation(m_filter.GetCompensation());
      m_bSubtitleCompensationSet=true;
    }
  }
  return true;
}

/// Starts the demuxer
/// This method will read the file until we found the pat/sdt
/// with all the audio/video pids
void CDeMultiplexer::Start()
{
  //reset some values
  m_bStarting=true ;
  m_receivedPackets=0;
  m_mpegParserTriggerFormatChange=false;
  m_mpegParserReset = true;  
  m_bFirstGopParsed = false; 
  m_mpegPesParser->basicVideoInfo.isValid = false;
  m_videoChanged=false;
  m_audioChanged=false;
  m_bEndOfFile=false;
  m_iPatVersion=-1;
  m_ReqPatVersion=-1;
  m_bPatParsed = false;
  m_bWaitGoodPat = false;
  m_bSetAudioDiscontinuity=false;
  m_bSetVideoDiscontinuity=false;
  m_bReadAheadFromFile = false;
  m_filter.m_bStreamCompensated=false ;
  int dwBytesProcessed=0;
  DWORD m_Time = GET_TIME_NOW();
  while((GET_TIME_NOW() - m_Time) < 10000)
  {
    m_bEndOfFile = false;  //reset eof every time through to ignore a false eof due to slow rtsp startup
    int BytesRead = ReadFromFile(false,false);    
    if (BytesRead <= 0)
    {
      BytesRead = 0;
      Sleep(10);
    }      
	  // LogDebug("demux:Start() BytesRead:%d, BytesProcessed:%d", BytesRead, dwBytesProcessed);
    if (dwBytesProcessed>INITIAL_READ_SIZE || GetAudioStreamCount()>0) //Wait for first PAT to be found
    {
      #ifdef USE_DYNAMIC_PINS
      if ((m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid > 1) &&       //There is a video stream.....
           (!m_mpegPesParser->basicVideoInfo.isValid || !m_bFirstGopParsed) &&  //and the first GOP header is not parsed....
           dwBytesProcessed<INITIAL_READ_SIZE)                                  //and we have not reached the data limit
      {
        //We are waiting for the first video GOP header to be parsed
        //so that OnVideoFormatChanged() can be triggered if necessary
        dwBytesProcessed+=BytesRead;
        continue;
      }
      #endif
      m_reader->SetFilePointer(0,FILE_BEGIN);
      //Flush(true);
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      if (!m_bFlushDelgNow && !m_bFlushRunning) //Flush already pending
      {
        m_bFlushDelgNow = true;
        WakeThread(); 
      }
      m_streamPcr.Reset();
      m_bStarting=false;
	    LogDebug("demux:Start() end1 BytesProcessed:%d", dwBytesProcessed);
      return;
    }
    dwBytesProcessed+=BytesRead;
  }
  m_streamPcr.Reset();
  m_iAudioReadCount=0;
  m_bStarting=false;
	LogDebug("demux:Start() end2 BytesProcessed:%d", dwBytesProcessed);
}

void CDeMultiplexer::SetEndOfFile(bool bEndOfFile)
{
  m_bEndOfFile=bEndOfFile;
}
/// Returns true if we reached the end of the file
bool CDeMultiplexer::EndOfFile()
{
  return m_bEndOfFile;
}

int CDeMultiplexer::ReadAheadFromFile()
{
  //if filter is stopped or
  //end of file has been reached or
  //demuxer should stop getting video packets
  //then return an error
  if (!m_filter.IsFilterRunning() || m_filter.m_bStopping || m_bEndOfFile) return -1;
  
	//LogDebug("demux:ReadAheadFromFile");
  int SizeRead = ReadFromFile(false,false) ;
  
  if (m_filter.State() != State_Running)
  {
    _InterlockedAnd(&m_AVDataLowCount, 0);
    m_bVideoSampleLate=false;
    m_bAudioSampleLate=false;
  }
  else if (m_filter.m_bStreamCompensated 
           && (SizeRead >= 0) 
           && (SizeRead < (m_filter.IsUNCfile() ? MIN_READ_SIZE_UNC : MIN_READ_SIZE)))
  {
    if ((m_vecAudioBuffers.size()==0) && m_bAudioSampleLate)
    {
      // No buffer and nothing to read....Running very low on data
      _InterlockedIncrement(&m_AVDataLowCount);   
    }
    if ((m_vecVideoBuffers.size()==0) && m_bVideoSampleLate)
    {
      // No buffer and nothing to read....Running very low on data
      _InterlockedIncrement(&m_AVDataLowCount);   
    }
  }

  return SizeRead;
}

/// This method reads the next READ_SIZE bytes from the file
/// and processes the raw data
/// When a TS packet has been discovered, OnTsPacket(byte* tsPacket) gets called
//  which in its turn deals with the packet
int CDeMultiplexer::ReadFromFile(bool isAudio, bool isVideo)
{
   // Don't read if flush pending/running or no reader....
  if (m_filter.IsSeeking() || m_bFlushDelgNow || m_bFlushRunning || (m_reader==NULL)) return -1;
    
  CAutoLock lock (&m_sectionRead);
  byte buffer[READ_SIZE];
  int dwReadBytes=0;
  bool result=false;
  //if we are playing a RTSP stream
  if (m_reader->IsBuffer())
  {
    // and, the current buffer holds data
    int nBytesToRead = m_reader->HasData();
    if (nBytesToRead > sizeof(buffer))
    {
        nBytesToRead=sizeof(buffer);
    }
    else
    {
        m_bAudioAtEof = true ;
        m_bVideoAtEof = true ;
    }
    if (nBytesToRead)
    {
      //then read raw data from the buffer
      m_reader->Read(buffer, nBytesToRead, (DWORD*)&dwReadBytes);
      if (dwReadBytes > 0)
      {
        //yes, then process the raw data
        result=true;
        OnRawData2(buffer,(int)dwReadBytes);
        m_LastDataFromRtsp = GET_TIME_NOW();
      }
    }
    else
    {
      if (!m_filter.IsTimeShifting())
      {
        //LogDebug("demux:endoffile...%d",GET_TIME_NOW()-m_LastDataFromRtsp );
        //set EOF flag and return
        if (((GET_TIME_NOW()-m_LastDataFromRtsp) > 2000) && (m_filter.State() != State_Paused) ) // A bit crappy, but no better idea...
        {
          LogDebug("demux:endoffile");
          m_bEndOfFile=true;
          return -1;
        }
      }
    }
    return dwReadBytes;
  }
  else
  {
    //playing a local file.
    //read raw data from the file
    if (SUCCEEDED(m_reader->Read(buffer,sizeof(buffer), (DWORD*)&dwReadBytes)))
    {
      if ((m_filter.IsTimeShifting()) && (dwReadBytes < sizeof(buffer)))
      {
        m_bAudioAtEof = true;
        m_bVideoAtEof = true;
      }

      if (dwReadBytes > 0)
      {
        //succeeded, process data
        OnRawData2(buffer,(int)dwReadBytes);
      }
      else
      {
        if (!m_filter.IsTimeShifting())
        {
          //set EOF flag and return
          LogDebug("demux:endoffile");
          m_bEndOfFile=true;
          return -1;
        }
      }

      //and return
      return dwReadBytes;
    }
    else
    {
      int x=123;
      LogDebug("Read failed...");
    }
  }
  //Failed to read any data
  return 0;
}
/// This method gets called via ReadFile() when a new TS packet has been received
/// if will :
///  - decode any new pat/pmt/sdt
///  - decode any audio/video packets and put the PES packets in the appropiate buffers
void CDeMultiplexer::OnTsPacket(byte* tsPacket)
{
  //LogDebug("OnTsPacket() start");
  CTsHeader header(tsPacket);

  m_patParser.OnTsPacket(tsPacket);

  if ((m_iPatVersion==-1) || m_bWaitGoodPat)
  {
    // First PAT not found or waiting for correct PAT
    return;
  }

  // Wait for new PAT if required.
  if ((m_iPatVersion & 0x0F) != (m_ReqPatVersion & 0x0F)) //No PAT yet, or PAT version doesn't match requested e.g. PAT data from old channel
  {
    if (m_ReqPatVersion==-1)                    
    {                                     // Now, unless channel change, 
       m_ReqPatVersion = m_iPatVersion;    // Initialize Pat Request.
       m_WaitNewPatTmo = GET_TIME_NOW();   // Now, unless channel change request,timeout will be always true. 
    }
    if (GET_TIME_NOW() < m_WaitNewPatTmo) 
    {
      // Timeout not reached.
      return;
    }
  }

  //if we have no PCR pid (yet) then there's nothing to decode, so return
  if (m_pids.PcrPid==0) return;

  if (header.Pid==0) return;
    
  // 'TScrambling' check commented out - headers are never scrambled, 
  // so it's safe to detect scrambled payload at PES level (in FillVideo()/FillAudio())
  
  //if (header.TScrambling) return;

  //skip any packets with errors in it
  if (header.TransportError) return;

  if( m_pids.TeletextPid > 0 && m_pids.TeletextPid != m_currentTeletextPid )
  {
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if( pTeletextServiceInfoCallback )
      {
      std::vector<TeletextServiceInfo>::iterator vit = m_pids.TeletextInfo.begin();
      while(vit != m_pids.TeletextInfo.end())
      {
        TeletextServiceInfo& info = *vit;
        LogDebug("Calling Teletext Service info callback");
        (*pTeletextServiceInfoCallback)(info.page, info.type, (byte)info.lang[0],(byte)info.lang[1],(byte)info.lang[2]);
        vit++;
      }
      m_currentTeletextPid = m_pids.TeletextPid;
    }
  }

  //is this the PCR pid ?
  if (header.Pid==m_pids.PcrPid)
  {
    //yep, does it have a PCR timestamp?
    CAdaptionField field;
    field.Decode(header,tsPacket);
    if (field.Pcr.IsValid)
    {
      //then update our stream pcr which holds the current playback timestamp
      m_streamPcr=field.Pcr;
    }
  }

  //as long as we dont have a stream pcr timestamp we return
  if (m_streamPcr.IsValid==false)
  {
    return;
  }
  
  //Buffers about to be flushed
  if (m_bFlushDelgNow || m_bFlushRunning || m_bShuttingDown)
  {
  	return;
  }

  //process the ts packet further
  FillVideo(header,tsPacket);
  FillAudio(header,tsPacket);
  FillSubtitle(header,tsPacket);
  FillTeletext(header,tsPacket);
}

/// Validate TS packet discontinuity 
bool CDeMultiplexer::CheckContinuity(int prevCC, CTsHeader& header)
{
  if ((prevCC !=-1 ) && (prevCC != ((header.ContinuityCounter - 1) & 0x0F)))
  {
    return false;
  }
  return true;
}

/// This method will check if the tspacket is an audio packet
/// ifso, it decodes the PES audio packet and stores it in the audio buffers
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  //LogDebug("FillAudio - audio PID %d", m_audioPid );
  CAutoLock flock (&m_sectionFlushAudio);

  if (IsAudioChanging() || m_iAudioStream<0 || m_iAudioStream>=m_audioStreams.size()) return;
  m_audioPid=m_audioStreams[m_iAudioStream].pid;
  if (m_audioPid==0 || m_audioPid != header.Pid) return;
  if (m_filter.GetAudioPin()->IsConnected()==false) return;
  if (header.AdaptionFieldOnly())return;

  if(!CheckContinuity(m_AudioPrevCC, header))
  {
    LogDebug("Audio Continuity error... %x ( prev %x )", header.ContinuityCounter, m_AudioPrevCC);
    if (!m_DisableDiscontinuitiesFiltering) 
    {
      m_AudioValidPES=false;  
    }
  }

  m_AudioPrevCC = header.ContinuityCounter;

  //CAutoLock lock (&m_sectionAudio);
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    //Sanity check PES header
    int posn=header.PayLoadStart;
    if (
           ((tsPacket[posn+0]!=0) || (tsPacket[posn+1]!=0) || (tsPacket[posn+2]!=1)) //Invalid start code
        || ((tsPacket[posn+3] & 0x80)==0)     //Invalid stream ID
        || ((tsPacket[posn+6] & 0xC0)!=0x80)  //Invalid marker bits
        || ((tsPacket[posn+6] & 0x20)==0x20)  //Payload scrambled
       )
    {
      //Discard this new/current PES packet
      m_AudioValidPES=false;  
      m_bSetAudioDiscontinuity=true;
      LogDebug("PES audio 0-0-1 fail");
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      m_bFlushDelegated = true;
      WakeThread();  
      return;
    }
    
    //yes, packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentAudioBuffer->Length() > 0)
    {
      m_t_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      m_pCurrentAudioBuffer = new CBuffer();
    }

    if (m_t_vecAudioBuffers.size()) //Process the previous PES packet
    {
      CBuffer *Cbuf=*m_t_vecAudioBuffers.begin();
      byte *p = Cbuf->Data() ;

      if (m_AudioValidPES)
      {
        //get pts/dts from pes header
        CPcr pts;
        CPcr dts;
        
        if (CPcr::DecodeFromPesHeader(p,0,pts,dts))
        {
          double diff;
          if (!m_lastAudioPTS.IsValid)
            m_lastAudioPTS=pts;
          if (m_lastAudioPTS>pts)
            diff=m_lastAudioPTS.ToClock()-pts.ToClock();
          else
            diff=pts.ToClock()-m_lastAudioPTS.ToClock();
          if (diff>2.0)
          {
            //Large PTS jump - flush the world...
            LogDebug("DeMultiplexer::FillAudio pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastAudioPTS.ToClock());
            m_lastAudioPTS.IsValid=false;
            m_lastVideoPTS.IsValid=false;
            m_bSetAudioDiscontinuity=true;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            m_bFlushDelegated = true;
            WakeThread();            
          }
          else
          {
            m_lastAudioPTS=pts;
          }

          Cbuf->SetPts(pts);
          
          REFERENCE_TIME MediaTime;
          m_filter.GetMediaPosition(&MediaTime);
          if (m_filter.m_bStreamCompensated && m_bAudioAtEof && !m_filter.m_bRenderingClockTooFast)
          {
            float Delta = pts.ToClock()-(float)((double)(m_filter.Compensation.m_time+MediaTime)/10000000.0) ;
            if (Delta < m_MinAudioDelta)
            {
              m_MinAudioDelta=Delta;
              if (Delta < -2.0)
              {
                //Large negative delta - flush the world...
                LogDebug("Demux : Audio to render too late= %03.3f Sec, flushing", Delta) ;
                m_MinAudioDelta+=1.0;
                m_MinVideoDelta+=1.0;                
                //Flushing is delegated to CDeMultiplexer::ThreadProc()
                m_bFlushDelegated = true;
                WakeThread();            
              }
              else if (Delta < 0.1)
              {
                LogDebug("Demux : Audio to render too late= %03.3f Sec", Delta) ;
                //  m_filter.m_bRenderingClockTooFast=true;
                m_MinAudioDelta+=1.0;
                m_MinVideoDelta+=1.0;                
              }
              else
              {
                LogDebug("Demux : Audio to render %03.3f Sec", Delta);
              }
            }
          }
        }
        //skip pes header
        int headerLen=9+p[8] ;
        int len = Cbuf->Length()-headerLen;
        if (len > 0)
        {
          byte *ps = p+headerLen;
          Cbuf->SetLength(len);
          while(len--) *p++ = *ps++;   // memcpy could be not safe.
        }
        else
        {
          LogDebug(" No data");
          m_AudioValidPES=false; //stop further processing
        }
      }

      { //Scoped for CAutoLock
        CAutoLock lock (&m_sectionAudio);
        if (m_AudioValidPES)
        {
          if (m_bSetAudioDiscontinuity)
          {
            m_bSetAudioDiscontinuity=false;
            Cbuf->SetDiscontinuity();
          }
  
          Cbuf->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
  
          //yes, then move the full PES in main queue.
          while (m_t_vecAudioBuffers.size())
          {
            ivecBuffers it;
            // Check if queue is not abnormally long..
            if (m_vecAudioBuffers.size()>MAX_AUD_BUF_SIZE)
            {
              ivecBuffers it = m_vecAudioBuffers.begin();
              delete *it;
              m_vecAudioBuffers.erase(it);
              
              //Something is going wrong...
              //LogDebug("DeMultiplexer: Audio buffer overrun, A/V buffers = %d/%d", m_vecAudioBuffers.size(), m_vecVideoBuffers.size());
              //m_filter.SetErrorAbort();
            }
            it = m_t_vecAudioBuffers.begin();
  
            CRefTime Ref;
            if((*it)->MediaTime(Ref))
            {
              if (Ref < m_FirstAudioSample) m_FirstAudioSample = Ref;
              if (Ref > m_LastAudioSample) m_LastAudioSample = Ref;
            }
            
            m_vecAudioBuffers.push_back(*it);
            m_t_vecAudioBuffers.erase(it);
          }
        }
        else
        {
          while (m_t_vecAudioBuffers.size())
          {
            ivecBuffers it;
            it = m_t_vecAudioBuffers.begin();
            delete *it;
            m_t_vecAudioBuffers.erase(it);
          }
          m_bSetAudioDiscontinuity = true; //Next good packet will be discontinuous
        }
      }
    }
    m_AudioValidPES = true;     
    m_bAudioAtEof = false;
  }

  if (m_AudioValidPES)
  {
    int pos=header.PayLoadStart;
    //packet contains rest of a pes packet
    //does the entire data in this tspacket fit in the current buffer ?
    if (m_pCurrentAudioBuffer->Length()+(188-pos)>=0x2000)
    {
      //no, then determine how many bytes do fit
      int copyLen=0x2000-m_pCurrentAudioBuffer->Length();
      //copy those bytes
      m_pCurrentAudioBuffer->Add(&tsPacket[pos],copyLen);
      pos+=copyLen;

      m_t_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      //and create a new one
      m_pCurrentAudioBuffer = new CBuffer();
    }
    //copy (rest) data in current buffer
    if (pos>0 && pos < 188)
    {
      m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos);
    }
  }
}


/// This method will check if the tspacket is an video packet
void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{                  
  CAutoLock lock (&m_sectionFlushVideo);
  
  if (m_pids.videoPids.size() == 0 || m_pids.videoPids[0].Pid==0) return;
  if (header.Pid!=m_pids.videoPids[0].Pid) return;

  if (header.AdaptionFieldOnly()) return;

  if (!CheckContinuity(m_VideoPrevCC, header))
  {
    LogDebug("Video Continuity error... %x ( prev %x )", header.ContinuityCounter, m_VideoPrevCC);
    if (!m_DisableDiscontinuitiesFiltering)
    {
      m_VideoValidPES = false;  
    }
  }

  m_VideoPrevCC = header.ContinuityCounter;

  if (m_pids.videoPids[0].VideoServiceType == SERVICE_TYPE_VIDEO_MPEG1 ||
      m_pids.videoPids[0].VideoServiceType == SERVICE_TYPE_VIDEO_MPEG2)
  {
    FillVideoMPEG2(header, tsPacket);
  }
  else
  {
    FillVideoH264(header, tsPacket);
  }
}

void CDeMultiplexer::FillVideoH264(CTsHeader& header, byte* tsPacket)
{
  int headerlen = header.PayLoadStart;

  if(!m_p)
  {
    m_p.Attach(new Packet());
    m_p->rtStart = Packet::INVALID_TIME;
    m_p->rtPrevStart = Packet::INVALID_TIME; 
    m_lastStart = 0;
    m_isNewNALUTimestamp = false;
    m_minVideoPTSdiff = DBL_MAX;
    m_bVideoPTSroff = false;
    //LogDebug("DeMultiplexer::FillVideoH264 New m_p");
  }

  if (header.PayloadUnitStart)
  {
    m_WaitHeaderPES = m_p->GetCount();
    m_mVideoValidPES = m_VideoValidPES;
    LOG_SAMPLES("DeMultiplexer::FillVideoH264 PayLoad Unit Start");
  }
  
  CAutoPtr<Packet> p(new Packet());

  if (headerlen < 188)
  {            
    int dataLen = 188-headerlen;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen],dataLen);

    m_p->Append(*p);

    if (m_p->GetCount() > 4194303) //Sanity check
    {
      //Let's start again...
      m_p.Free();
      m_pl.RemoveAll();
      m_bSetVideoDiscontinuity = true;
      m_mpegParserReset = true;
      m_VideoValidPES = false;
      m_mVideoValidPES = false;
      m_WaitHeaderPES = -1;
      LogDebug("DeMux: H264 PES size out-of-bounds");
      return;
    }
  }
  else
  {
    return;
  }

  if (m_WaitHeaderPES >= 0)
  {
    int AvailablePESlength = m_p->GetCount()-m_WaitHeaderPES ;
    BYTE* start = m_p->GetData() + m_WaitHeaderPES;
    
    if (AvailablePESlength < 9)
    {
      LogDebug("demux:vid Incomplete PES ( Avail %d )", AvailablePESlength);    
      return;
    }

    if (
           ((start[0]!=0) || (start[1]!=0) || (start[2]!=1)) //Invalid start code
        || ((start[3] & 0x80)==0)    //Invalid stream ID
      	|| ((start[6] & 0xC0)!=0x80) //Invalid marker bits
        || ((start[6] & 0x20)==0x20) //Payload scrambled
       )
    {
      LogDebug("PES H264 0-0-1 fail");
      //LogDebug("PES H264 0-0-1 fail, %x-%x-%x-%x-%x-%x-%x-%x", start[0], start[1], start[2], start[3], start[4], start[5], start[6], start[7]);
      m_VideoValidPES=false;
      m_mVideoValidPES = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_p->rtPrevStart = Packet::INVALID_TIME; 
      m_WaitHeaderPES = -1;
      m_bSetVideoDiscontinuity=true;
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      m_bFlushDelegated = true;
      WakeThread();
      return;
    }
    else
    {
      if (AvailablePESlength < 9+start[8])
      {
        LogDebug("demux:vid Incomplete PES ( Avail %d/%d )", AvailablePESlength, AvailablePESlength+9+start[8]) ;    
        return ;
      }
      else
      { // full PES header is available.
        CPcr pts;
        CPcr dts;
        bool isSamePTS = false;

        m_VideoValidPES=true ;
        if (CPcr::DecodeFromPesHeader(start,0,pts,dts))
        {
          double diff;
          if (!m_lastVideoPTS.IsValid)
            m_lastVideoPTS=pts;
          if (m_lastVideoPTS>pts)
            diff=m_lastVideoPTS.ToClock()-pts.ToClock();
          else
            diff=pts.ToClock()-m_lastVideoPTS.ToClock();
          if (diff>2.0)
          {
            //Large PTS jump - flush the world...
            LogDebug("DeMultiplexer::FillVideoH264 pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_lastAudioPTS.IsValid=false;
            m_lastVideoPTS.IsValid=false;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            m_bFlushDelegated = true;
            WakeThread();
          }
          else
          {
            m_lastVideoPTS=pts;
            if ((diff < m_minVideoPTSdiff) && (diff > 0.005))
            {
              m_minVideoPTSdiff = diff;
            }
            if ((diff < 0.002) && (pts.IsValid) && !m_fHasAccessUnitDelimiters)
            {
              LOG_SAMPLES("DeMultiplexer::FillVideoH264 - PTS is same, diff %f, pts %f ", (float)diff, (float)pts.ToClock());
              double d = pts.ToClock();
              if (m_minVideoPTSdiff < 0.05) //We've seen a few PES timestamps/video frames
              {
                d += m_minVideoPTSdiff;
              }
              else //Guess - add 36.6 ms
              {
                d += 0.0366 ;
              }
              d += (m_bVideoPTSroff ? 0.0015 : 0.0025); //Ensure PTS always changes
              m_bVideoPTSroff = !m_bVideoPTSroff;
              pts.FromClock(d);
              pts.IsValid=true;
              isSamePTS = true;
            }            
            LOG_SAMPLES("DeMultiplexer::FillVideoH264 pts: %f, dts: %f, diff %f, minDiff %f, rtStart : %d ", (float)pts.ToClock(), (float)dts.ToClock(), (float) diff, (float)m_minVideoPTSdiff, pts.PcrReferenceBase);
          }
        }
        m_lastStart -= 9+start[8];
        m_p->RemoveAt(m_WaitHeaderPES, 9+start[8]);
                
        if (pts.IsValid)
        {
          if (!isSamePTS) 
          {
            m_p->rtPrevStart = m_p->rtStart;
          }
          m_p->rtStart = (pts.PcrReferenceBase);
        }
        m_WaitHeaderPES = -1;
        //LogDebug("m_p->rtStart: %d, m_p->rtPrevStart: %d",(int)m_p->rtStart, (int)m_p->rtPrevStart);
      }
    }
  }

  if (m_p->GetCount())
  {
    BYTE* start = m_p->GetData();
    BYTE* end = start + m_p->GetCount();

    MOVE_TO_H264_START_CODE(start, end);

    while(start <= end-4)
    {
      BYTE* next = start+1;
      if (next < m_p->GetData() + m_lastStart)
      {
        next = m_p->GetData() + m_lastStart;
      }

      MOVE_TO_H264_START_CODE(next, end);

      if(next >= end-4)
      {
        m_lastStart = next - m_p->GetData();
        break;
      }
        
      CAutoPtr<Packet> p2(new Packet());
      p2->rtStart = Packet::INVALID_TIME;
      bool isNewTimestamp = false;

      int size = next - start;
      
      //Copy complete NALU into p2 buffer
          
      size -= 3; //Adjust to allow for start code
      
      if ((size <= 0) || (size > 4194303)) //Sanity check
      {
        //Let's start again...
        m_p.Free();
        m_pl.RemoveAll();
        m_bSetVideoDiscontinuity = true;
        m_mpegParserReset = true;
        m_VideoValidPES = false;
        m_mVideoValidPES = false;
        m_WaitHeaderPES = -1;
        LogDebug("DeMux: H264 NALU size out-of-bounds %d", size);
        return;
      }
      
      DWORD dwNalLength = 
        ((size >> 24) & 0x000000ff) |
        ((size >>  8) & 0x0000ff00) |
        ((size <<  8) & 0x00ff0000) |
        ((size << 24) & 0xff000000);

      //LogDebug("DeMux: NALU size %d", size);

      p2->SetCount (size+sizeof(dwNalLength));
      
      memcpy (p2->GetData(), &dwNalLength, sizeof(dwNalLength));
      memcpy (p2->GetData()+sizeof(dwNalLength), (start+3), size);
      LOG_SAMPLES("Input p2 NALU Type: %d (%d), m_p->rtStart: %d, m_p->rtPrevStart: %d", (*(p2->GetData()+4)&0x1f), p2->GetCount(), (int)m_p->rtStart, (int)m_p->rtPrevStart);
      
      if ((m_p->rtStart != m_p->rtPrevStart) && (m_p->rtPrevStart != Packet::INVALID_TIME))
      {
        // new rtStart/PES packet transition - use previous rtStart value as this NALU started in the previous PES packet.
        // This is important for streams without Access Unit Delimiters e.g. some IPTV streams
		    p2->rtStart = m_p->rtPrevStart;
		    m_p->rtPrevStart = m_p->rtStart; 
		    isNewTimestamp = true;
      }
      else
      {
		    p2->rtStart = m_p->rtStart;
      }

      //Decide if we should transfer NALU packets to output sample
      
      if((*(p2->GetData()+4)&0x1f) == 0x09) 
      {
        m_fHasAccessUnitDelimiters = true;
      }
        
      if(((*(p2->GetData()+4)&0x1f) == 0x09) || (!m_fHasAccessUnitDelimiters && m_isNewNALUTimestamp))
      {
        m_isNewNALUTimestamp = false;
        if ((m_pl.GetCount()>0) && m_mVideoValidPES)
        {
          //Transfer NALU packets to output sample
          bool Gop = false;
          char nalID = 0;
          
          //Copy available NALUs into new packet 'p' (for the next video buffer)
          CAutoPtr<Packet> p(new Packet());
          p->rtStart = Packet::INVALID_TIME;

          if (!m_fHasAccessUnitDelimiters)
          {
            //Add fake AUD....
            DWORD size = 2;
            WORD data9 = 0xF009;
            DWORD dwNalLength = 
              ((size >> 24) & 0x000000ff) |
              ((size >>  8) & 0x0000ff00) |
              ((size <<  8) & 0x00ff0000) |
              ((size << 24) & 0xff000000);
              
            p->SetCount (size+sizeof(dwNalLength));
            
            memcpy (p->GetData(), &dwNalLength, sizeof(dwNalLength));
            memcpy (p->GetData()+sizeof(dwNalLength), &data9, size);
            //LogDebug("Fake AUD: %x %x %x %x %x %x",  p->GetAt(0), p->GetAt(1), p->GetAt(2), p->GetAt(3), p->GetAt(4), p->GetAt(5));
          }
          
          while(m_pl.GetCount()>0)
          {
            CAutoPtr<Packet> p4(new Packet());
            p4 = m_pl.RemoveHead();
            
            //if (!iFrameScanner.SeenEnough())
            //  iFrameScanner.ProcessNALU(p2);
            LOG_OUTSAMPLES("Output p4 NALU Type: %d (%d), rtStart: %d", p4->GetAt(4)&0x1f, p4->GetCount(), (int)p->rtStart);
            
            nalID = p4->GetAt(4);
            if ((((nalID & 0x9f) == 0x07) || ((nalID & 0x9f) == 0x08)) && ((nalID & 0x60) != 0)) //Process SPS & PPS data
            {
              Gop = m_mpegPesParser->OnTsPacket(p4->GetData(), p4->GetCount(), false, m_mpegParserReset);
              m_mpegParserReset = false;
            }
                                        
            if (p->rtStart == Packet::INVALID_TIME)
            {
              p->rtStart = p4->rtStart;
              //LogDebug("Fake AUD2: %x %x %x %x %x %x",  p4->GetAt(0), p4->GetAt(1), p4->GetAt(2), p4->GetAt(3), p4->GetAt(4), p4->GetAt(5));
            }
            if (p->GetCount()>0)
            {
              p->Append(*p4);
            }
            else
            {
              p = p4;
            }
          }

          if (Gop)
          {
            m_mpegParserReset = true; //Reset next time around (so that it always searches for a full 'Gop' header)
            if (!m_bFirstGopParsed)
            {
              m_bFirstGopParsed = true;
              LogDebug("DeMultiplexer: First Gop after new PAT, %dx%d @ %d:%d, %.3fHz %s",m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps, m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
            }
          }

          CPcr timestamp;
          if(p->rtStart != Packet::INVALID_TIME )
          {
            timestamp.PcrReferenceBase = p->rtStart;
            timestamp.IsValid=true;
          }
          //LogDebug("NALU Type: %d (%d) %d, p->timestamp %f, p->rtStart %d",  p->GetAt(4)&0x1f, p->GetCount(), timestamp.ToClock(), (int)p->rtStart);


          if ((Gop || m_bFirstGopFound) && m_filter.GetVideoPin()->IsConnected())
          {
            CRefTime Ref;
            CBuffer *pCurrentVideoBuffer = new CBuffer(p->GetCount());
            pCurrentVideoBuffer->Add(p->GetData(), p->GetCount());
            pCurrentVideoBuffer->SetPts(timestamp);   
            pCurrentVideoBuffer->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
            pCurrentVideoBuffer->MediaTime(Ref);
            LOG_OUTSAMPLES("...> Store NALU type (length) = %d (%d), p->rtStart = %d, timestamp %f", (*(p->GetData()+4) & 0x1F), p->GetCount(), (int)p->rtStart, timestamp.ToClock()) ;
            // Must use p->rtStart as CPcr is UINT64 and INVALID_TIME is LONGLONG
            // Too risky to change CPcr implementation at this time 
            if(p->rtStart != Packet::INVALID_TIME)
            {
              if (Gop && m_bFirstGopFound && !m_bSecondGopFound)
              {
                m_bSecondGopFound=true;
                LogDebug("  H.264 2nd GOP found %f ", Ref.Millisecs()/1000.0f);
              }
              if (Gop && !m_bFirstGopFound)
              {
                m_bFirstGopFound=true;
                LogDebug("  H.264 I-FRAME found %f ", Ref.Millisecs()/1000.0f);
                m_LastValidFrameCount=0;
              }
              if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
              if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
              if (m_bFirstGopFound && !m_bFrame0Found && m_LastValidFrameCount>=5 /*(frame_count==0)*/)
              {
                LogDebug("  H.264 First supposed '0' frame found. %f ", m_FirstVideoSample.Millisecs()/1000.0f);
                m_bFrame0Found = true;
              }
              m_LastValidFrameCount++;
            }

            pCurrentVideoBuffer->SetFrameType(Gop? 'I':'?');
            pCurrentVideoBuffer->SetFrameCount(0);
            pCurrentVideoBuffer->SetVideoServiceType(m_pids.videoPids[0].VideoServiceType);
            if (m_bSetVideoDiscontinuity)
            {
              m_bSetVideoDiscontinuity=false;
              pCurrentVideoBuffer->SetDiscontinuity();
            }
            
            REFERENCE_TIME MediaTime;
            m_filter.GetMediaPosition(&MediaTime);
            if (m_filter.m_bStreamCompensated && m_bVideoAtEof && !m_filter.m_bRenderingClockTooFast)
            {
              float Delta = (float)((double)Ref.Millisecs()/1000.0)-(float)((double)(m_filter.Compensation.m_time+MediaTime)/10000000.0) ;
              if (Delta < m_MinVideoDelta)
              {
                m_MinVideoDelta=Delta;
                if (Delta < -2.0)
                {
                  //Large negative delta - flush the world...
                  LogDebug("Demux : Video to render too late= %03.3f Sec, flushing", Delta) ;
                  m_MinAudioDelta+=1.0;
                  m_MinVideoDelta+=1.0;                
                  //Flushing is delegated to CDeMultiplexer::ThreadProc()
                  m_bFlushDelegated = true;
                  WakeThread();            
                }
                else if (Delta < 0.2)
                {
                  LogDebug("Demux : Video to render too late= %03.3f Sec", Delta) ;
                  //  m_filter.m_bRenderingClockTooFast=true;
                  m_MinAudioDelta+=1.0;
                  m_MinVideoDelta+=1.0;                
                }
                else
                {
                  LogDebug("Demux : Video to render %03.3f Sec", Delta);
                }
              }
            }
            m_bVideoAtEof = false;

            { //Scoped for CAutoLock
              CAutoLock lock (&m_sectionVideo);
              if (m_vecVideoBuffers.size()<=MAX_VID_BUF_SIZE)
              {
                if (m_bFirstGopFound)
                {
                  // ownership is transfered to vector
                  m_vecVideoBuffers.push_back(pCurrentVideoBuffer);
                }
                else
                {
                  delete pCurrentVideoBuffer;
                  pCurrentVideoBuffer = NULL;
                  m_bSetVideoDiscontinuity = true;            
                  //LogDebug("DeMultiplexer: Delete video buffer");
                }
              }
              else
              {
                delete pCurrentVideoBuffer;
                pCurrentVideoBuffer = NULL;
                m_bSetVideoDiscontinuity = true;            
                //Something is going wrong...
                //LogDebug("DeMultiplexer: Video buffer overrun, A/V buffers = %d/%d", m_vecAudioBuffers.size(), m_vecVideoBuffers.size());
                //m_filter.SetErrorAbort();  
              }
            }
          }

          if (Gop)
          {
            if (m_lastVidResX!=m_mpegPesParser->basicVideoInfo.width || m_lastVidResY!=m_mpegPesParser->basicVideoInfo.height
                || (m_mpegParserTriggerFormatChange && m_videoChanged && !IsAudioChanging()))
            {
              LogDebug("DeMultiplexer: triggering OnVideoFormatChanged");
              m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,15000000,m_mpegPesParser->basicVideoInfo.isInterlaced);
              m_filter.GetVideoPin()->SetAddPMT();
            }
            
            if ((m_lastVidResX!=m_mpegPesParser->basicVideoInfo.width || m_lastVidResY!=m_mpegPesParser->basicVideoInfo.height) && !m_filter.m_bDisableVidSizeRebuildH264)
            {
              LogDebug("DeMultiplexer: %x video format changed, %dx%d @ %d:%d, %.3fHz %s",header.Pid,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps,m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
              if (m_mpegParserTriggerFormatChange && !IsAudioChanging())
              {
                LogDebug("DeMultiplexer: OnMediaFormatChange triggered by H264Parser, aud %d, vid 1", m_audioChanged);
                SetMediaChanging(true);
                if (m_audioChanged)
                  m_filter.OnMediaTypeChanged(VIDEO_CHANGE | AUDIO_CHANGE); //Video and audio
                else
                  m_filter.OnMediaTypeChanged(VIDEO_CHANGE); //Video only
                m_mpegParserTriggerFormatChange=false;
              }
              else if (m_mpegParserTriggerFormatChange)
              {
                m_videoChanged = true;
              }
            }
            else //video resolution is unchanged, but there may be other format changes
            {
              if (m_mpegParserTriggerFormatChange && !IsAudioChanging())
              {
                LogDebug("DeMultiplexer: Got GOP after channel change detected, format change, aud %d, vid %d", m_audioChanged, m_videoChanged);
                if (m_audioChanged || m_videoChanged)
                {
                  SetMediaChanging(true);
                  if (m_audioChanged && m_videoChanged)
                    m_filter.OnMediaTypeChanged(VIDEO_CHANGE | AUDIO_CHANGE);
                  else if (m_audioChanged)
                    m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
                  else
                    m_filter.OnMediaTypeChanged(VIDEO_CHANGE);
                }
                else
                {
                  SetMediaChanging(false);
                }
                m_mpegParserTriggerFormatChange=false;
              }
            }
            m_lastVidResX=m_mpegPesParser->basicVideoInfo.width;
            m_lastVidResY=m_mpegPesParser->basicVideoInfo.height;
          }
        }
        else
        {
          m_bSetVideoDiscontinuity = !m_mVideoValidPES;
        }
        
        m_pl.RemoveAll();
          
        //p2->rtStart = m_p->rtStart; 
        //m_p->rtStart = Packet::INVALID_TIME;
      }

      //LogDebug(".......> Store NALU type (length) = %d (%d), p2->rtStart = %d", (*(p2->GetData()+4) & 0x1F), p2->GetCount(), (int)p2->rtStart) ;
      m_pl.AddTail(p2);
      m_isNewNALUTimestamp = isNewTimestamp;
      isNewTimestamp = false;

      start = next;
      m_lastStart = start - m_p->GetData() + 1;
    }

    if(start > m_p->GetData())
    {
      m_lastStart -= (start - m_p->GetData());
      m_p->RemoveAt(0, start - m_p->GetData());
    }
  }
  return;
}


void CDeMultiplexer::FillVideoMPEG2(CTsHeader& header, byte* tsPacket)
{
  static const double frame_rate[16]={1.0/25.0,       1001.0/24000.0, 1.0/24.0, 1.0/25.0,
                                    1001.0/30000.0, 1.0/30.0,       1.0/50.0, 1001.0/60000.0,
                                    1.0/60.0,       1.0/25.0,       1.0/25.0, 1.0/25.0,
                                    1.0/25.0,       1.0/25.0,       1.0/25.0, 1.0/25.0 };
  static const char tc[]="XIPBXXXX";

  int headerlen = header.PayLoadStart;

  if(!m_p)
  {
    m_p.Attach(new Packet());
    m_p->rtStart = Packet::INVALID_TIME;
    m_lastStart = 0;
    m_bInBlock=false;
  }

  if (header.PayloadUnitStart)
  {
    m_WaitHeaderPES = m_p->GetCount();
    m_mVideoValidPES = m_VideoValidPES;
//    LogDebug("DeMultiplexer::FillVideo PayLoad Unit Start");
  }

  CAutoPtr<Packet> p(new Packet());
  
  if (headerlen < 188)
  {
    int dataLen = 188-headerlen;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen],dataLen);

    m_p->Append(*p);

    if (m_p->GetCount() > 4194303) //Sanity check
    {
      //Let's start again...
      m_p.Free();
      m_pl.RemoveAll();
      m_bSetVideoDiscontinuity = true;
      m_mpegParserReset = true;
      m_VideoValidPES = false;
      m_mVideoValidPES = false;
      m_WaitHeaderPES = -1;
      LogDebug("DeMux: MPEG2 PES size out-of-bounds");
      return;
    }
  }
  else
    return;

  if (m_WaitHeaderPES >= 0)
  {
    int AvailablePESlength = m_p->GetCount()-m_WaitHeaderPES;
    BYTE* start = m_p->GetData() + m_WaitHeaderPES;

    if (AvailablePESlength < 9)
    {
      LogDebug("demux:vid Incomplete PES ( Avail %d )", AvailablePESlength);    
      return;
    }

    if (
           ((start[0]!=0) || (start[1]!=0) || (start[2]!=1)) //Invalid start code
        || ((start[3] & 0x80)==0)    //Invalid stream ID
      	|| ((start[6] & 0xC0)!=0x80) //Invalid marker bits
        || ((start[6] & 0x20)==0x20) //Payload scrambled
       )
    {
      LogDebug("PES MPEG2 0-0-1 fail");
      //LogDebug("Pes MPEG2 0-0-1 fail, %x-%x-%x-%x-%x-%x-%x-%x", start[0], start[1], start[2], start[3], start[4], start[5], start[6], start[7]);
      m_VideoValidPES = false;
      m_mVideoValidPES = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_WaitHeaderPES = -1;
      m_bSetVideoDiscontinuity=true;
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      m_bFlushDelegated = true;
      WakeThread();    
      return;        
    }
    else
    {
      if (AvailablePESlength < 9+start[8])
      {
        LogDebug("demux:vid Incomplete PES ( Avail %d/%d )", AvailablePESlength, AvailablePESlength+9+start[8]) ;    
        return;
      }
      else
      { // full PES header is available.
        CPcr pts;
        CPcr dts;

        m_VideoValidPES=true ;
        if (CPcr::DecodeFromPesHeader(start,0,pts,dts))
        {
          double diff;
          if (!m_lastVideoPTS.IsValid)
            m_lastVideoPTS=pts;
          if (m_lastVideoPTS>pts)
            diff=m_lastVideoPTS.ToClock()-pts.ToClock();
          else
            diff=pts.ToClock()-m_lastVideoPTS.ToClock();
          if (diff>2.0)
          {
            //Large PTS jump - flush the world...
            LogDebug("DeMultiplexer::FillVideoMPEG2 pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_lastAudioPTS.IsValid=false;
            m_lastVideoPTS.IsValid=false;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            m_bFlushDelegated = true;
            WakeThread();
          }
          else
          {
//            LogDebug("DeMultiplexer::FillVideo pts : %f ", (float)pts.ToClock());
            m_lastVideoPTS=pts;
          }
          m_VideoPts = pts;
        }
        m_lastStart -= 9+start[8];
        m_p->RemoveAt(m_WaitHeaderPES, 9+start[8]);

        m_WaitHeaderPES = -1;
      }
    }
  }

  if (m_p->GetCount())
  {
    BYTE* start = m_p->GetData();
    BYTE* end = start + m_p->GetCount();
    // 000001B3 sequence_header_code
    // 00000100 picture_start_code

    while(start <= end-4)
    {
      if (((*(DWORD*)start & 0xFFFFFFFF) == 0xb3010000) || ((*(DWORD*)start & 0xFFFFFFFF) == 0x00010000))
      {
        if(!m_bInBlock)
        {
          if (m_VideoPts.IsValid) m_CurrentVideoPts=m_VideoPts;
          m_VideoPts.IsValid=false;
          m_bInBlock=true;
        }
        break;
      }
      start++;
    }

    if(start <= end-4)
    {
      BYTE* next = start+1;
      if (next < m_p->GetData() + m_lastStart)
      {
        next = m_p->GetData() + m_lastStart;
      }

      while(next <= end-4 && ((*(DWORD*)next & 0xFFFFFFFF) != 0xb3010000) && ((*(DWORD*)next & 0xFFFFFFFF) != 0x00010000)) next++;

      if(next >= end-4)
      {
        m_lastStart = next - m_p->GetData();
      }
      else
      {
        m_bInBlock=false ;
        int size = next - start;

        CAutoPtr<Packet> p2(new Packet());		
        p2->SetCount(size);
        memcpy (p2->GetData(), m_p->GetData(), size);

        if (*(DWORD*)p2->GetData() == 0x00010000)     // picture_start_code ?
        {
          BYTE *p = p2->GetData() ; 
          char frame_type = tc[((p[5]>>3)&7)];                     // Extract frame type (IBP). Just info.
          int frame_count = (p[5]>>6)+(p[4]<<2);                   // Extract temporal frame count to rebuild timestamp ( if required )

          // TODO: try to drop non I-Frames when > 2.0x playback speed
          //if (frame_type != 'I')

            //double rate = 0.0;
            //m_filter.GetVideoPin()->GetRate(&rate);

          m_pl.AddTail(p2);
//          LogDebug("DeMultiplexer::FillVideo Frame length : %d %x %x", size, *(DWORD*)start, *(DWORD*)next);

          if (m_mVideoValidPES)
          {
            CAutoPtr<Packet> p(new Packet());
            p = m_pl.RemoveHead();
//            LogDebug("Output Type: %x %d", *(DWORD*)p->GetData(),p->GetCount());

            while(m_pl.GetCount())
            {
              CAutoPtr<Packet> p2 = m_pl.RemoveHead();
//              LogDebug("Output Type: %x %d", *(DWORD*)p2->GetData(),p2->GetCount());
              p->Append(*p2);
            }

//            LogDebug("frame len %d decoded PTS %f (framerate %f), %c(%d)", p->GetCount(), m_CurrentVideoPts.IsValid ? (float)m_CurrentVideoPts.ToClock() : 0.0f,(float)m_curFrameRate,frame_type,frame_count);

            bool Gop = m_mpegPesParser->OnTsPacket(p->GetData(), p->GetCount(), true, m_mpegParserReset);
            if (Gop)
            {
              m_mpegParserReset = true; //Reset next time around (so that it always searches for a full 'Gop' header)
              if (!m_bFirstGopParsed)
              {
                m_bFirstGopParsed = true;
                LogDebug("DeMultiplexer: First Gop after new PAT, %dx%d @ %d:%d, %.3fHz %s",m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps, m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
              }
            }
            else
            {
              m_mpegParserReset = false;
            }

            if (Gop) m_LastValidFrameCount=-1;

            if ((Gop || m_bFirstGopFound) && m_filter.GetVideoPin()->IsConnected())
            {
              CRefTime Ref;
              CBuffer *pCurrentVideoBuffer = new CBuffer(p->GetCount());
              pCurrentVideoBuffer->Add(p->GetData(), p->GetCount());
              if (m_CurrentVideoPts.IsValid)
              {                                                     // Timestamp Ok.
                m_LastValidFrameCount=frame_count;
                m_LastValidFramePts=m_CurrentVideoPts;
              }
              else
              {                    
                if (m_LastValidFrameCount>=0)                       // No timestamp, but we've latest GOP timestamp.
                {
                  double d = m_LastValidFramePts.ToClock() + (frame_count-m_LastValidFrameCount) * m_curFrameRate ;
                  m_CurrentVideoPts.FromClock(d);                   // Rebuild it from 1st frame in GOP timestamp.
                  m_CurrentVideoPts.IsValid=true;
                }
              }
              pCurrentVideoBuffer->SetPts(m_CurrentVideoPts);   
              pCurrentVideoBuffer->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
              pCurrentVideoBuffer->MediaTime(Ref);

              if(m_CurrentVideoPts.IsValid)
              {
                if (Gop && !m_bFirstGopFound)
                {
                  m_bFirstGopFound=true ;
                  LogDebug("  MPEG I-FRAME found %f ", Ref.Millisecs()/1000.0f);
                }
                if (m_bFirstGopFound && !m_bFrame0Found && (frame_count==0))
                {
                  LogDebug("  MPEG First '0' frame found. %f ", Ref.Millisecs()/1000.0f);
                  m_bFrame0Found = true;
                }
                if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
                if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
              }

              pCurrentVideoBuffer->SetFrameType(frame_type);
              pCurrentVideoBuffer->SetFrameCount(frame_count);
              pCurrentVideoBuffer->SetVideoServiceType(m_pids.videoPids[0].VideoServiceType);
              if (m_bSetVideoDiscontinuity)
              {
                m_bSetVideoDiscontinuity=false;
                pCurrentVideoBuffer->SetDiscontinuity();
              }

              REFERENCE_TIME MediaTime;
              m_filter.GetMediaPosition(&MediaTime);
              if (m_filter.m_bStreamCompensated && m_bVideoAtEof && !m_filter.m_bRenderingClockTooFast)
              {
                float Delta = (float)((double)Ref.Millisecs()/1000.0)-(float)((double)(m_filter.Compensation.m_time+MediaTime)/10000000.0) ;
                if (Delta < m_MinVideoDelta)
                {
                  m_MinVideoDelta=Delta;
                  if (Delta < -2.0)
                  {
                    //Large negative delta - flush the world...
                    LogDebug("Demux : Video to render too late= %03.3f Sec, flushing", Delta) ;
                    m_MinAudioDelta+=1.0;
                    m_MinVideoDelta+=1.0;                
                    //Flushing is delegated to CDeMultiplexer::ThreadProc()
                    m_bFlushDelegated = true;
                    WakeThread();            
                  }
                  else if (Delta < 0.2)
                  {
                    //_InterlockedIncrement(&m_VideoDataLowCount);              
                    LogDebug("Demux : Video to render too late= %03.3f Sec", Delta) ;
                    //  m_filter.m_bRenderingClockTooFast=true;
                    m_MinAudioDelta+=1.0;
                    m_MinVideoDelta+=1.0;                
                  }
                  else
                  {
                    LogDebug("Demux : Video to render %03.3f Sec", Delta);
                  }
                }
              }
              m_bVideoAtEof = false ;

              { //Scoped for CAutoLock
                CAutoLock lock (&m_sectionVideo);
                if (m_vecVideoBuffers.size()<=MAX_VID_BUF_SIZE)
                {
                  if (m_bFirstGopFound)
                  {
                    // ownership is transfered to vector
                    m_vecVideoBuffers.push_back(pCurrentVideoBuffer);
                  }
                  else
                  {
                    delete pCurrentVideoBuffer;
                    pCurrentVideoBuffer = NULL;
                    m_bSetVideoDiscontinuity = true;            
                    //LogDebug("DeMultiplexer: Delete video buffer");
                  }
                }
                else
                {
                  delete pCurrentVideoBuffer;
                  pCurrentVideoBuffer = NULL;
                  m_bSetVideoDiscontinuity = true;
                  //Something is going wrong...
                  //LogDebug("DeMultiplexer: Video buffer overrun, A/V buffers = %d/%d", m_vecAudioBuffers.size(), m_vecVideoBuffers.size());
                  //m_filter.SetErrorAbort();  
                }
              }
              
            }
            m_CurrentVideoPts.IsValid=false ;   
            
            if (Gop)
            {
              if (m_lastVidResX!=m_mpegPesParser->basicVideoInfo.width || m_lastVidResY!=m_mpegPesParser->basicVideoInfo.height
                  || (m_mpegParserTriggerFormatChange && m_videoChanged && !IsAudioChanging()))
              {
                LogDebug("DeMultiplexer: triggering OnVideoFormatChanged");
                m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,15000000,m_mpegPesParser->basicVideoInfo.isInterlaced);
                m_filter.GetVideoPin()->SetAddPMT();
              }
              
              if ((m_lastVidResX!=m_mpegPesParser->basicVideoInfo.width || m_lastVidResY!=m_mpegPesParser->basicVideoInfo.height) && !m_filter.m_bDisableVidSizeRebuildMPEG2)
              {
                LogDebug("DeMultiplexer: %x video format changed, %dx%d @ %d:%d, %.3fHz %s",header.Pid,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps,m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
                if (m_mpegParserTriggerFormatChange && !IsAudioChanging())
                {
                  LogDebug("DeMultiplexer: OnMediaFormatChange triggered by MPEG2 parser, aud %d, vid 1", m_audioChanged);
                  SetMediaChanging(true);
                  if (m_audioChanged)
                    m_filter.OnMediaTypeChanged(VIDEO_CHANGE | AUDIO_CHANGE); //Video and audio
                  else
                    m_filter.OnMediaTypeChanged(VIDEO_CHANGE); //Video only
                  m_mpegParserTriggerFormatChange=false;
                }
                else if (m_mpegParserTriggerFormatChange)
                {
                  m_videoChanged = true;
                }
              }
              else //video resolution is unchanged, but there may be other format changes
              {
                if (m_mpegParserTriggerFormatChange && !IsAudioChanging())
                {
                  LogDebug("DeMultiplexer: Got GOP after channel change detected, format change, aud %d, vid %d", m_audioChanged, m_videoChanged);
                  if (m_audioChanged || m_videoChanged)
                  {
                    SetMediaChanging(true);
                    if (m_audioChanged && m_videoChanged)
                      m_filter.OnMediaTypeChanged(VIDEO_CHANGE | AUDIO_CHANGE);
                    else if (m_audioChanged)
                      m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
                    else
                      m_filter.OnMediaTypeChanged(VIDEO_CHANGE);
                  }
                  else
                  {
                    SetMediaChanging(false);
                  }
                  m_mpegParserTriggerFormatChange=false;
                }
              }
              m_lastVidResX=m_mpegPesParser->basicVideoInfo.width;
              m_lastVidResY=m_mpegPesParser->basicVideoInfo.height;
            }
          }
          else
          {
            m_bSetVideoDiscontinuity = !m_mVideoValidPES;
          }
          //m_VideoValidPES=true ;                                    // We've just completed a frame, set flag until problem clears it 
          m_pl.RemoveAll() ;                                        
        }
        else                                                        // sequence_header_code
        {
          m_curFrameRate = frame_rate[*(p2->GetData()+7) & 0x0F] ;  // Extract frame rate in seconds.
   	      m_pl.AddTail(p2);                                         // Add sequence header.
   	    }

        start = next;
        m_lastStart = start - m_p->GetData() + 1;
      }
      if(start > m_p->GetData())
      {
        m_lastStart -= (start - m_p->GetData());
        m_p->RemoveAt(0, start - m_p->GetData());
      }
    }
  }
}


/// This method will check if the tspacket is an subtitle packet
/// if so store it in the subtitle buffers
void CDeMultiplexer::FillSubtitle(CTsHeader& header, byte* tsPacket)
{
  CAutoLock flock (&m_sectionFlushSubtitle);

  if (header.TScrambling) return;
  if (m_filter.GetSubtitlePin()->IsConnected()==false) return;
  if (m_iSubtitleStream<0 || m_iSubtitleStream>=m_subtitleStreams.size()) return;

  // If current subtitle PID has changed notify the DVB sub filter
  if( m_subtitleStreams[m_iSubtitleStream].pid > 0 &&
    m_subtitleStreams[m_iSubtitleStream].pid != m_currentSubtitlePid )
  {
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if( pDVBSubtitleFilter )
    {
      LogDebug("Calling SetSubtitlePid");
      pDVBSubtitleFilter->SetSubtitlePid(m_subtitleStreams[m_iSubtitleStream].pid);
      LogDebug(" done - SetSubtitlePid");
      LogDebug("Calling SetFirstPcr");
      pDVBSubtitleFilter->SetFirstPcr(m_duration.FirstStartPcr().PcrReferenceBase);
      LogDebug(" done - SetFirstPcr");
      m_currentSubtitlePid = m_subtitleStreams[m_iSubtitleStream].pid;
      pDVBSubtitleFilter->SetHDMV(false);
    }
  }

  if (m_currentSubtitlePid==0 || m_currentSubtitlePid != header.Pid) return;
  if ( header.AdaptionFieldOnly() ) return;

  CAutoLock lock (&m_sectionSubtitle);
  if ( false==header.AdaptionFieldOnly() )
  {
    if (header.PayloadUnitStart)
    {
      m_subtitlePcr = m_streamPcr;
      //LogDebug("FillSubtitle: PayloadUnitStart -- %lld", m_streamPcr.PcrReferenceBase );
    }
    if (m_vecSubtitleBuffers.size()>MAX_SUB_BUF_SIZE)
    {
      ivecBuffers it = m_vecSubtitleBuffers.begin() ;
      CBuffer* subtitleBuffer=*it;
      delete subtitleBuffer ;
      m_vecSubtitleBuffers.erase(it);
    }

    m_pCurrentSubtitleBuffer->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
    m_pCurrentSubtitleBuffer->SetPts(m_subtitlePcr);
    m_pCurrentSubtitleBuffer->Add(tsPacket,188);

    m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);

    m_pCurrentSubtitleBuffer = new CBuffer();
  }
}

void CDeMultiplexer::FillTeletext(CTsHeader& header, byte* tsPacket)
{
  if (header.TScrambling) return;
  if (m_pids.TeletextPid==0) return;
  if (header.Pid!=m_pids.TeletextPid) return;
  if ( header.AdaptionFieldOnly() ) return;

  if(pTeletextEventCallback != NULL)
  {
    (*pTeletextEventCallback)(TELETEXT_EVENT_PACKET_PCR_UPDATE,m_streamPcr.PcrReferenceBase - m_duration.FirstStartPcr().PcrReferenceBase - (m_filter.Compensation.Millisecs() * 90 ));
  }
  if(pTeletextPacketCallback != NULL)
  {
    (*pTeletextPacketCallback)(tsPacket,188);
  }
}

int CDeMultiplexer::GetAudioBufferCnt()
{
  return m_vecAudioBuffers.size();
}

int CDeMultiplexer::GetVideoBufferCnt()
{
  return m_vecVideoBuffers.size();
}

int CDeMultiplexer::GetVideoBuffCntFt(double* frameTime)
{
  double fps = m_mpegPesParser->basicVideoInfo.fps;
  if ((fps > 22.0) && (fps < 130.0))
  {
    *frameTime = (1000.0/fps);
  }
  else
  {
    *frameTime = 10.0;
  }
  return m_vecVideoBuffers.size();
}

//Decide if we need to prefetch more data
bool CDeMultiplexer::CheckPrefetchState(bool isNormal, bool isForced)
{  
  //Check for near-overflow conditions first
  if (m_filter.GetAudioPin()->IsConnected() && (m_vecAudioBuffers.size() > (MAX_AUD_BUF_SIZE - 50)))
  {
    return false;
  }
  if (m_filter.GetVideoPin()->IsConnected() && (m_vecVideoBuffers.size() > (MAX_VID_BUF_SIZE - 20)))
  {
    return false;
  }

  //Start-of-play situation
  if (!m_filter.m_bStreamCompensated || isForced)
  {
    return true;
  }

  //Normal play
  if (isNormal)
  {
    if (m_filter.GetAudioPin()->IsConnected() && (m_vecAudioBuffers.size() < 3))
    {
      return true;
    }
    if (m_filter.GetVideoPin()->IsConnected() && (m_vecVideoBuffers.size() < 12))
    {
      return true;
    }
  }

  return false;
}

void CDeMultiplexer::GetBufferCounts(int* ACnt, int* VCnt)
{
  *ACnt = m_vecAudioBuffers.size();
  *VCnt = m_vecVideoBuffers.size();
}

int CDeMultiplexer::GetVideoBufferPts(CRefTime& First, CRefTime& Last)
{
  First = m_FirstVideoSample;
  Last = m_LastVideoSample;
  return m_vecVideoBuffers.size();
}

int CDeMultiplexer::GetAudioBufferPts(CRefTime& First, CRefTime& Last)
{
  First = m_FirstAudioSample;
  Last = m_LastAudioSample;
  return m_vecAudioBuffers.size();
}

/// This method gets called-back from the pat parser when a new PAT/PMT/SDT has been received
/// In this method we check if any audio/video/subtitle pid or format has changed
/// If not, we simply return
/// If something has changed we ask the MP to rebuild the graph
void CDeMultiplexer::OnNewChannel(CChannelInfo& info)
{
  //CAutoLock lock (&m_section);
  CPidTable pids=info.PidTable;
  
  //LogDebug("OnNewChannel callback, pat version:%d->%d",m_iPatVersion, info.PatVersion);

  if ((info.PatVersion != m_iPatVersion) || m_bWaitGoodPat)
  {
    if (!m_bWaitGoodPat)
    {
      LogDebug("OnNewChannel: PAT change detected: %d->%d",m_iPatVersion, info.PatVersion);
    }
    
    if (m_filter.IsTimeShifting() && (m_iPatVersion!=-1)) //TimeShifting TV channel change only
    {
      DWORD timeTemp = GetTickCount();
      int PatReqDiff = (info.PatVersion & 0x0F) - (m_ReqPatVersion & 0x0F);
      int PatIDiff = (info.PatVersion & 0x0F) - (m_iPatVersion & 0x0F);
      
      if (PatIDiff < 0) //Rollover
      {
        PatIDiff += 16;
      }
      
      if ((PatIDiff > 7) && (PatReqDiff != 0)) //PAT version change too big/negative and it's not the requested PAT)
      {      
        //Skipped back in timeshift file or possible RTSP seek accuracy problem ?
        if (!m_bWaitGoodPat)
        {
          m_bWaitGoodPat = true;
          m_WaitGoodPatTmo = timeTemp + ((m_filter.IsRTSP() && m_filter.IsLiveTV()) ? 2500 : 1000);   // Set timeout to 1 sec (2.5 sec for live RTSP)
          LogDebug("OnNewChannel: wait for good PAT, IDiff:%d, ReqDiff:%d ", PatIDiff, PatReqDiff);
          return; // wait a while for correct PAT version to arrive
        }
        else if (timeTemp < m_WaitGoodPatTmo)
        {
          return; // wait for correct PAT version
        }
        LogDebug("OnNewChannel: 'Wait for good PAT' timeout, allow PAT update: %d->%d",m_iPatVersion, info.PatVersion);
      }
      
      m_ReqPatVersion = info.PatVersion ;
      LogDebug("OnNewChannel: found good PAT: %d", info.PatVersion);
    }
    
    m_bWaitGoodPat = false;
    
    if ((info.PatVersion == m_iPatVersion) && (m_pids == pids)) 
    {
      //Fix for RTSP 'infinite loop' channel change with multiple audio streams problem 
      LogDebug("OnNewChannel: PAT change already processed: %d", info.PatVersion);
      return;
    }

    m_iPatVersion=info.PatVersion;
    m_bSetAudioDiscontinuity=true;
    m_bSetVideoDiscontinuity=true;
    //Flushing is delegated to CDeMultiplexer::ThreadProc()
    if (!m_bFlushDelgNow && !m_bFlushRunning) //Flush already pending
    {
      m_bFlushDelgNow = true;
      WakeThread(); 
    }
  }
  else
  {
    // No audio streams or channel info was not changed
    if (pids.audioPids.size()==0 || m_pids == pids )
    { 
      return; // no
    }
  }

  //remember the old audio & video formats
  int oldVideoServiceType(-1);
  if(m_pids.videoPids.size()>0)
  {
    oldVideoServiceType=m_pids.videoPids[0].VideoServiceType;
  }

  m_pids=pids;
  LogDebug("New channel found (PAT/PMT/SDT changed)");
  m_pids.LogPIDs();

  if(pTeletextEventCallback != NULL)
  {
    (*pTeletextEventCallback)(TELETEXT_EVENT_RESET,TELETEXT_EVENTVALUE_NONE);
  }

  IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
  if( pDVBSubtitleFilter )
  {
    // Make sure that subtitle cache is reset ( in filter & MP )
    pDVBSubtitleFilter->NotifyChannelChange();
  }

  //update audio streams etc..
  if (m_pids.PcrPid>0x1)
  {
    m_duration.SetVideoPid(m_pids.PcrPid);
  }
  else if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>0x1)
  {
    m_duration.SetVideoPid(m_pids.videoPids[0].Pid);
  }
  m_audioStreams.clear();

  for(int i(0) ; i < m_pids.audioPids.size() ; i++)
  {
    struct stAudioStream audio;
    audio.pid=m_pids.audioPids[i].Pid;
    audio.language[0]=m_pids.audioPids[i].Lang[0];
    audio.language[1]=m_pids.audioPids[i].Lang[1];
    audio.language[2]=m_pids.audioPids[i].Lang[2];
    audio.language[3]=m_pids.audioPids[i].Lang[3];
    audio.language[4]=m_pids.audioPids[i].Lang[4];
    audio.language[5]=m_pids.audioPids[i].Lang[5];
    audio.language[6]=0;
    audio.audioType = m_pids.audioPids[i].AudioServiceType;
    m_audioStreams.push_back(audio);
  }

  m_subtitleStreams.clear();
  
  for(int i(0) ; i < m_pids.subtitlePids.size() ; i++)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.subtitlePids[i].Pid;
    subtitle.language[0]=m_pids.subtitlePids[i].Lang[0];
    subtitle.language[1]=m_pids.subtitlePids[i].Lang[1];
    subtitle.language[2]=m_pids.subtitlePids[i].Lang[2];
    subtitle.language[3]=0;
    m_subtitleStreams.push_back(subtitle);
  }

  bool changed=false;
  m_videoChanged=false;
  m_audioChanged=false;

  #ifdef USE_DYNAMIC_PINS
  //Is the video pin connected?
  if ((m_filter.GetVideoPin()->IsConnected()) && (m_pids.videoPids.size() > 0))
  {
    changed=true; //force a check in the mpeg parser
    if (oldVideoServiceType != m_pids.videoPids[0].VideoServiceType)
    {
      m_videoChanged=true;
    }
  }
  #else
  //did the video format change?
  if (m_pids.videoPids.size() > 0 && oldVideoServiceType != m_pids.videoPids[0].VideoServiceType)
  {
    //yes, is the video pin connected?
    if (m_filter.GetVideoPin()->IsConnected())
    {
      changed=true;
      m_videoChanged=true;
    }
  }
  #endif

  //Lock the audio stream variables
  CAutoLock lock (&m_sectionSetAudioStream);
    
  m_iAudioStream = 0;

  LogDebug ("Setting initial audio index to : %i", m_iAudioStream);

  //get the new audio format
  int newAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }

  //did the audio format change?
  if (m_AudioStreamType != newAudioStreamType )
  {
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {
      changed=true;
      m_audioChanged=true;
    }
  }

  //did audio/video format change?
  if (changed)
  {
    #ifdef USE_DYNAMIC_PINS
    // if we have a video stream, let the mpeg parser trigger the OnMediaTypeChanged
    if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>0x1)  
    {
      LogDebug("DeMultiplexer: We have a video stream, so we let the mpegParser check/trigger format changes");
      m_receivedPackets=0;
      SetMediaChanging(true);
      if (m_audioStreams.size() == 1)
      {
        if ((m_AudioStreamType == SERVICE_TYPE_AUDIO_UNKNOWN) || (m_AudioStreamType != newAudioStreamType))
        {
          m_AudioStreamType = newAudioStreamType ;
          m_audioChanged=true;
          LogDebug("DeMultiplexer: Audio media types changed");
        }
      }
      m_mpegParserTriggerFormatChange=true;
      m_mpegParserReset = true;
      m_bFirstGopParsed = false;
      m_mpegPesParser->basicVideoInfo.isValid = false; 
    }
    else
    {
      if (m_audioStreams.size() == 1)
      {
        if ((m_AudioStreamType == SERVICE_TYPE_AUDIO_UNKNOWN) || (m_AudioStreamType != newAudioStreamType))
        {
          m_AudioStreamType = newAudioStreamType ;
          // notify the ITSReaderCallback. MP will then rebuild the graph
          LogDebug("DeMultiplexer: Audio media types changed. Trigger OnMediaTypeChanged()...");
          m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
          SetMediaChanging(true); 
        }
      }
    }
    #else
    if (m_audioChanged && m_videoChanged)
      m_filter.OnMediaTypeChanged(VIDEO_CHANGE | AUDIO_CHANGE);
    else
      if (m_audioChanged)
        m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
      else
        m_filter.OnMediaTypeChanged(VIDEO_CHANGE);
    #endif
  }

  //if we have more than 1 audio track available, tell host application that we are ready
  //to receive an audio track change.
  if ((m_audioStreams.size() >= 1) && m_filter.CheckAudioCallback())
  {
    LogDebug("OnNewChannel: OnRequestAudioChange()");
    SetAudioChanging(true);
    m_filter.OnRequestAudioChange();
  }
  else
    m_AudioStreamType = newAudioStreamType;

  LogDebug("OnNewChannel: New Audio %d", m_AudioStreamType);

  if( pSubUpdateCallback != NULL)
  {
    int bitmap_index = -1;
    (*pSubUpdateCallback)(m_subtitleStreams.size(),(m_subtitleStreams.size() > 0 ? &m_subtitleStreams[0] : NULL),&bitmap_index);
    if(bitmap_index >= 0)
    {
      LogDebug("Calling SetSubtitleStream from OnNewChannel:  %i", bitmap_index);
      SetSubtitleStream(bitmap_index);
    }
  }
  
  m_bPatParsed = true;
}


void CDeMultiplexer::SetMediaChanging(bool onOff)
{
  CAutoLock lock (&m_sectionMediaChanging);
  LogDebug("demux:Wait for media format change:%d", onOff);
  m_bWaitForMediaChange=onOff;
  m_tWaitForMediaChange=GET_TIME_NOW() ;
}

bool CDeMultiplexer::IsMediaChanging(void)
{
  CAutoLock lock (&m_sectionMediaChanging);
  if (!m_bWaitForMediaChange)
  { 
    return false ;
  }
  else if (!m_filter.CheckCallback() && (GET_TIME_NOW()-m_tWaitForMediaChange > 200))
  {
    m_bWaitForMediaChange=false;
    LogDebug("demux: No callback - Wait for Media change cancelled");
    return false;
  }
  else
  {
    if (GET_TIME_NOW()-m_tWaitForMediaChange > 5000)
    {
      m_bWaitForMediaChange=false;
      LogDebug("demux: Alert: Wait for Media change cancelled on 5 secs timeout");
      return false;
    }
  }
  return true;
}

void CDeMultiplexer::SetAudioChanging(bool onOff)
{
  CAutoLock lock (&m_sectionAudioChanging);
  LogDebug("demux:Wait for Audio stream selection :%d", onOff);
  m_bWaitForAudioSelection=onOff;
  m_tWaitForAudioSelection=GET_TIME_NOW();
}

bool CDeMultiplexer::IsAudioChanging(void)
{
  CAutoLock lock (&m_sectionAudioChanging);
  if (!m_bWaitForAudioSelection) return false;
  else
  {
    if (GET_TIME_NOW()-m_tWaitForAudioSelection > 5000)
    {
      m_bWaitForAudioSelection=false;
      LogDebug("demux: Alert: Wait for Audio stream selection cancelled on 5 secs timeout");
      return false;
    }
  }
  return true;
}

void CDeMultiplexer::RequestNewPat(void)
{
  m_ReqPatVersion++;
  m_ReqPatVersion &= 0x0F;
  LogDebug("Request new PAT = %d", m_ReqPatVersion);
  m_WaitNewPatTmo=GET_TIME_NOW()+10000;
  
  if (!m_bFlushDelgNow && !m_bFlushRunning && (m_filter.State() == State_Paused)) //Flush already pending
  {
    //Flush buffers to speed up zapping
    m_bFlushDelgNow = true;
    WakeThread(); 
  }
}

void CDeMultiplexer::ClearRequestNewPat(void)
{
  m_ReqPatVersion=m_iPatVersion; // Used for AnalogTv or channel change fail.
}

bool CDeMultiplexer::IsNewPatReady(void)
{
  return ((m_ReqPatVersion & 0x0F) == (m_iPatVersion & 0x0F)) ? true : false;
}

void CDeMultiplexer::ResetPatInfo(void)
{
  m_pids.Reset();
}

bool CDeMultiplexer::VidPidGood(void)
{
  return (m_pids.videoPids.size() > 0);
}

bool CDeMultiplexer::AudPidGood(void)
{
  return (m_pids.audioPids.size() > 0);
}

bool CDeMultiplexer::SubPidGood(void)
{
  return (m_pids.subtitlePids.size() > 0);
}

bool CDeMultiplexer::PatParsed(void)
{
  return m_bPatParsed;
}

void CDeMultiplexer::SetTeletextEventCallback(int (CALLBACK *pTeletextEventCallback)(int eventcode, DWORD64 eval))
{
  this->pTeletextEventCallback = pTeletextEventCallback;
}

void CDeMultiplexer::SetTeletextPacketCallback(int (CALLBACK *pTeletextPacketCallback)(byte*, int))
{
  this->pTeletextPacketCallback = pTeletextPacketCallback;
}

void CDeMultiplexer::SetTeletextServiceInfoCallback(int (CALLBACK *pTeletextSICallback)(int, byte,byte,byte,byte))
{
  this->pTeletextServiceInfoCallback = pTeletextSICallback;
}

void CDeMultiplexer::CallTeletextEventCallback(int eventCode,unsigned long int eventValue)
{
  if(pTeletextEventCallback != NULL)
  {
    (*pTeletextEventCallback)(eventCode,eventValue);
  }
}


//======================================================================

//**************************************************************************************************************
/// This method is running in its own thread
//  Flushing after large video/audio PTS jump and PES '0-0-1 fail' errors 
//  are delegated to this thread.
void CDeMultiplexer::ThreadProc()
{
  LogDebug("CDeMultiplexer::ThreadProc start(), threadID:0x%x", GetCurrentThreadId());

  DWORD timeNow = GET_TIME_NOW();
  DWORD  lastFlushTime = timeNow;
  DWORD  lastFileReadTime = timeNow;

  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_NORMAL);
  do
  {

    timeNow = GET_TIME_NOW();
    
    //Flush delegated to this thread
    if (m_bFlushDelegated || m_bFlushDelgNow)
    {
      if (!m_bFlushDelgNow && ((timeNow - 500) < lastFlushTime)) 
      { 
        // Too early for next flush
        m_bFlushDelegated = false;
      }
      else
      {
        lastFlushTime = timeNow;
  
        LogDebug("CDeMultiplexer::ThreadProc - Flush");     
        //Flush the internal data
        Flush(true);
        m_bFlushDelgNow = false;
      }
    }

    //File read prefetch
    if (m_bReadAheadFromFile && (timeNow > (lastFileReadTime + (m_filter.IsUNCfile() ? 10 : 5))) )
    {
      lastFileReadTime = timeNow; 
      ReadAheadFromFile();
      m_bReadAheadFromFile = false;
    }
     
     
    Sleep(1);
  }
  while (!ThreadIsStopping(11)) ;
  LogDebug("CDeMultiplexer::ThreadProc stopped()");
}

