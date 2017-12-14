/*
 *  Copyright (C) 2005-2013 Team MediaPortal
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
//#include "..\..\DVBSubtitle2\Source\IDVBSub.h"
#include "mediaFormats.h"
//#include "h264nalu.h"
#include <cassert>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"


//Macro derived from from MPC-HC/LAV splitter...
#define MOVE_TO_H264_START_CODE(b, e, fb) fb=false; while(b <= e-4 && !((*(DWORD *)b == 0x01000000) || ((*(DWORD *)b & 0x00FFFFFF) == 0x00010000))) b++; if((b <= e-4) && *(DWORD *)b == 0x01000000) {b++; fb=true;}

#define MOVE_TO_HEVC_START_CODE(b, e, fb) fb=false; while(b <= e-4 && !((*(DWORD *)b == 0x01000000) || ((*(DWORD *)b & 0x00FFFFFF) == 0x00010000))) b++; if((b <= e-4) && *(DWORD *)b == 0x01000000) {b++; fb=true;}


// uncomment the //LogDebug to enable extra logging
#define LOG_SAMPLES //LogDebug
#define LOG_OUTSAMPLES //LogDebug
#define LOG_SAMPLES_HEVC //LogDebug
#define LOG_OUTSAMPLES_HEVC //LogDebug
#define LOG_VID_BITRATE //LogDebug

extern void LogDebug(const char *fmt, ...);
extern void LogRotate();
extern void StopLogger();
extern DWORD m_tGTStartTime;
extern long m_instanceCount;
extern CCritSec m_instanceLock;


CDeMultiplexer::CDeMultiplexer(CTsDuration& duration,CTsReaderFilter& filter)
:m_duration(duration)
,m_filter(filter)
{
  { // Scope for CAutoLock
    CAutoLock lock(&m_instanceLock);  
    if (m_instanceCount == 0)
    {
      //Initialise m_tGTStartTime for GET_TIME_NOW() macro.
      //The macro is used to avoid having to handle timeGetTime()
      //rollover issues in the body of the code
      m_tGTStartTime = (timeGetTime() - 0x40000000); 
    }
    m_instanceCount++;
  }

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
  m_bSetAudioDiscontinuity = false;
  m_bSetVideoDiscontinuity = false;
  m_reader = NULL;
  pTeletextEventCallback = NULL;
  pSubUpdateCallback = NULL;
  pTeletextPacketCallback = NULL;
  pTeletextServiceInfoCallback = NULL;
  m_iAudioReadCount = 0;
  m_lastVideoPTS.IsValid = false;
  m_lastVideoDTS.IsValid = false;
  m_lastAudioPTS.IsValid = false;
  m_bLogFPSfromDTSPTS = false;
  m_bUsingGOPtimestamp = false;
  m_bFlushDelegated = false;
  m_bFlushDelgNow = false;
  m_bFlushRunning = false; 
  m_bStarting=false;
  m_bReadAheadFromFile = false;
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
  m_lastARX=-1;
  m_lastARY=-1;
  m_lastStreamType=-1;
  m_FirstVideoSample = 0x7FFFFFFF00000000LL;
  m_LastVideoSample = 0;
  m_ZeroVideoSample = 0;
  
  m_sampleTime = 0;
  m_sampleTimePrev = 0;
  m_byteRead = 0;
  m_bitRate = 5000000.0f; //Nominal value...
  m_LastDataFromRtsp = GET_TIME_NOW();
  m_targetAVready = m_LastDataFromRtsp;
  m_tWaitForMediaChange=m_LastDataFromRtsp ;
  m_tWaitForAudioSelection=m_LastDataFromRtsp;
  m_lastFlushTime=m_LastDataFromRtsp; 
  m_bWaitForMediaChange=false;
  m_bWaitForAudioSelection=false;
  m_bSubtitleCompensationSet=false;
  m_initialAudioSamples = 3;
  m_initialVideoSamples = 12;
  m_prefetchLoopDelay = PF_LOOP_DELAY_MIN;

  m_mpegPesParser = new CMpegPesParser();

  m_pFileReadBuffer = NULL;
  m_pFileReadBuffer = new byte[READ_SIZE]; //~130ms of data @ 8Mbit/s
  
  m_dVidPTSJumpLimit = 2.0; //Maximum allowed time in seconds for video PTS jumps
  m_dfAudSampleDuration = -1.0;
  m_currentAudHeader = 0;
  m_lastAudHeader = 0;
  m_audHeaderCount = 0;
  m_audioBytesRead = 0;
  m_hadPESfail = 0;
  m_fileReadLatency = 0;
  m_maxFileReadLatency = 0;
  m_fileReadLatSum = 0;
  m_fileReadLatCount = 0;
  
  LogDebug(" ");
  LogDebug("=================== New filter instance =========================================");
  LogDebug("  Logging format: [Date Time] [InstanceID-instanceCount] [ThreadID] Message....  ");
  LogDebug("=================================================================================");
}

CDeMultiplexer::~CDeMultiplexer()
{
  LogDebug("CDeMultiplexer::dtor");
  m_bShuttingDown = true;
  //stop file read thread
  StopThread(5000);
  Flush(true, false);
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;
  delete m_mpegPesParser;
  // delete m_CcParserH264;


  m_subtitleStreams.clear();
  m_audioStreams.clear();
  if (m_pFileReadBuffer)
  {
    delete [] m_pFileReadBuffer;
    m_pFileReadBuffer = NULL;
  }
  else
  {
    LogDebug("CDeMultiplexer::dtor - ERROR m_pFileReadBuffer is NULL !!");
  }
  
  { // Scope for CAutoLock
    CAutoLock lock(&m_instanceLock); 
    if (m_instanceCount > 0) 
    {
      m_instanceCount--;
    }
  }
  LogDebug("CDeMultiplexer::dtor - finished, instanceCount:%d", m_instanceCount);
  StopLogger();
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
  if (stream < 0 || stream >= (int)m_audioStreams.size())
    return S_FALSE;

  //set index
  m_iAudioStream = (unsigned int)stream;

  //get the new audio stream type
  int newAudioStreamType = m_audioStreams[m_iAudioStream].audioType;

  LogDebug("Old Audio %d, New Audio %d", m_AudioStreamType, newAudioStreamType);
  //did it change?
  if ((m_AudioStreamType == SERVICE_TYPE_AUDIO_UNKNOWN) || (m_AudioStreamType != newAudioStreamType))
  {
    m_AudioStreamType = newAudioStreamType;
    m_mpegPesParser->AudioReset(); 
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {
	    // here, stream is not parsed yet
      m_audioChanged = true;
      if (!IsMediaChanging())             
      {
        LogDebug("SetAudioStream : SetMediaChanging(true)");
        //Flushing is delegated to CDeMultiplexer::ThreadProc()
        DelegatedFlush(true, false);
        SetMediaChanging(true);
        m_filter.m_bForceSeekOnStop=true;     // Force stream to be resumed after
      }
      else   // Mpeg parser info is required or audio graph is already rebuilding.
      {
        LogDebug("SetAudioStream : Media already changing");   // just wait 1st GOP
      }
    }
  }
  else
  {
    //Reset the audio parser (but do not flush the audio sample queue)
    FlushCurrentAudio();
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
  if (stream < 0 || stream >= (int)m_audioStreams.size())
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

bool CDeMultiplexer::GetAudioStreamType(int stream,CMediaType& pmt, int iPosition)
{
  //LogDebug("GetAudioStreamType() : Stream %d, iPosition %d, audioIsValid %d", stream, iPosition, m_mpegPesParser->basicAudioInfo.isValid);

  if (stream < 0 || stream >= (int)m_audioStreams.size() || m_mpegPesParser == NULL )
  {
    pmt.InitMediaType();
    pmt.SetType      (& MEDIATYPE_Audio);
    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
    pmt.SetSampleSize(1);
    pmt.SetTemporalCompression(FALSE);
    pmt.SetVariableSize();
    pmt.SetFormatType(&FORMAT_WaveFormatEx);
    pmt.SetFormat(MPEG2AudioFormat,sizeof(MPEG2AudioFormat));
    return false;
  }

  CAutoLock lock (&m_mpegPesParser->m_sectionAudioPmt);

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
      iPosition ? pmt.SetSubtype(& MEDIASUBTYPE_AAC) : pmt.SetSubtype(& MEDIASUBTYPE_MPEG_ADTS_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);      
      pmt.SetFormat(AACRawAudioFormat2,sizeof(AACRawAudioFormat2));
      break;
    case SERVICE_TYPE_AUDIO_LATM_AAC:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      iPosition ? pmt.SetSubtype(& MEDIASUBTYPE_MPEG_LOAS) : pmt.SetSubtype(& MEDIASUBTYPE_LATM_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      iPosition ? pmt.SetFormat(AACLoasAudioFormat,sizeof(AACLoasAudioFormat)) : pmt.SetFormat(AACLatmAudioFormat,sizeof(AACLatmAudioFormat));
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
    case SERVICE_TYPE_AUDIO_DD_PLUS: //ATSC E-AC3 (DD plus)
    case SERVICE_TYPE_AUDIO_E_AC3:   //ATSC E-AC3 (DD plus)
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_DOLBY_DDPLUS);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AC3AudioFormat,sizeof(AC3AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_DTS:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_DTS2);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(DTSAudioFormat,sizeof(DTSAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_DTS_HD:
    case SERVICE_TYPE_AUDIO_DTS_HDMA:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_DTS_HD);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(DTSHDAudioFormat,sizeof(DTSHDAudioFormat));
      break;
    case SERVICE_TYPE_DCII_OR_LPCM: //HDMV/BD format LPCM audio
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_BD_LPCM_AUDIO);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(LPCMAudioFormat,sizeof(LPCMAudioFormat));
      break;
  }
  
  //Modify with generated WaveFormatEx, correct channel count and sampling rate if available
  if (m_mpegPesParser->basicAudioInfo.isValid && m_mpegPesParser->basicAudioInfo.streamIndex==stream)
  {
    if (m_mpegPesParser->basicAudioInfo.pmtValid)
    {
      pmt.SetFormat(m_mpegPesParser->audPmt.Format(), m_mpegPesParser->audPmt.FormatLength());
    }
    
    WAVEFORMATEX* wfe = (WAVEFORMATEX*)pmt.Format();
    wfe->nChannels = m_mpegPesParser->basicAudioInfo.channels;
    wfe->nSamplesPerSec = m_mpegPesParser->basicAudioInfo.sampleRate;
  }
  
  
  
  return m_mpegPesParser->basicAudioInfo.isValid;
}

// This methods selects the subtitle stream specified
bool CDeMultiplexer::SetSubtitleStream(__int32 stream)
{
  //is stream index valid?
  if (stream < 0 || stream >= (int)m_subtitleStreams.size())
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
  if (stream <0 || stream >= (int)m_subtitleStreams.size())
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
  if( m_pids.videoPids.size() != 0 && m_mpegPesParser != NULL)
  {
    if (!m_mpegPesParser->basicVideoInfo.isValid)
    {
      return false;
    }
    
    CAutoLock lock (&m_mpegPesParser->m_sectionVideoPmt);
    pmt = m_mpegPesParser->pmt;

    if (m_filter.m_bUseFPSfromDTSPTS)  //Use FPS derived from DTS/PTS timestamps if available, instead of from header data.
    {              
      double minVidDiff = 0.0;
      double AllowedError = 0.015; //Allow 1.5% tolerance
      double BestVal = 0.0;

      if ((m_minVideoDTSdiff < 0.043) && (m_minVideoDTSdiff > 0.0163) && (m_minVideoDTSdiff < m_minVideoPTSdiff)) //Sanity check, 23.25Hz -> 61.35Hz
      {
        minVidDiff = m_minVideoDTSdiff;
      }
      else if ((m_minVideoPTSdiff < 0.043) && (m_minVideoPTSdiff > 0.0163)) //Sanity check, 23.25Hz -> 61.35Hz
      {
        minVidDiff = m_minVideoPTSdiff;
      }  
      else if ((m_curFramePeriod < 0.043) && (m_curFramePeriod > 0.0163)) //Sanity check, 23.25Hz -> 61.35Hz
      {
        minVidDiff = m_curFramePeriod;
      }                
      
      if (minVidDiff > 0.0)
      {
        static double AllowedValues[] = {1000.0/60000.0, 1001.0/60000.0,
                                         1000.0/50000.0, 
                                         1000.0/30000.0, 1001.0/30000.0, 
                                         1000.0/25000.0, 
                                         1000.0/24000.0, 1001.0/24000.0};
  	
        double currError = AllowedError;
        int nAllowed = sizeof(AllowedValues) / sizeof(AllowedValues[0]);
  	      
        // Find best match with allowed frame periods
        for (int i = 0; i < nAllowed; ++i)
        {
          currError = fabs(1.0 - (minVidDiff / AllowedValues[i]));
            // if (!m_bLogFPSfromDTSPTS)
            // {
            //   LogDebug("demux:GetVideoStreamType(), currError = %f, minDiff = %f, AE = %f", (float)currError, (float)minVidDiff, (float)AllowedValues[i]);  
            // }
          if (currError < AllowedError)
          {
            AllowedError = currError;
            BestVal = AllowedValues[i];
          }
        }
        
        if (BestVal > 0.0)
        {
          //Sanity check - get timestamp FPS into the same ballpark as the header FPS....
          if ((m_mpegPesParser->basicVideoInfo.fps < 40.0) && (m_mpegPesParser->basicVideoInfo.fps > 23.25) && (BestVal < 0.025))
          {
            //header FPS is < 40 and timestamp FPS is > 40, divide timestamp FPS by 2
            BestVal *= 2.0;
          }
          else if ((m_mpegPesParser->basicVideoInfo.fps > 40.0) && (m_mpegPesParser->basicVideoInfo.fps < 61.35) && (BestVal > 0.025))
          {
            //header FPS is > 40 and timestamp FPS is < 40, so multiply timestamp FPS by 2
            BestVal /= 2.0;
          }
          
          //Update PMT with new AvgTimePerFrame value          
          if (pmt.formattype==FORMAT_VideoInfo)
            ((VIDEOINFOHEADER*)pmt.pbFormat)->AvgTimePerFrame = (REFERENCE_TIME)(BestVal * 10000000.0);
          else if (pmt.formattype==FORMAT_VideoInfo2)
            ((VIDEOINFOHEADER2*)pmt.pbFormat)->AvgTimePerFrame = (REFERENCE_TIME)(BestVal * 10000000.0);
          else if (pmt.formattype==FORMAT_MPEGVideo)
            ((MPEG1VIDEOINFO*)pmt.pbFormat)->hdr.AvgTimePerFrame = (REFERENCE_TIME)(BestVal * 10000000.0);
          else if (pmt.formattype==FORMAT_MPEG2Video)
            ((MPEG2VIDEOINFO*)pmt.pbFormat)->hdr.AvgTimePerFrame = (REFERENCE_TIME)(BestVal * 10000000.0);

          if (!m_bLogFPSfromDTSPTS) 
          {
            LogDebug("demux:GetVideoStreamType(), FPS from DTS/PTS = %f, DTSdiff/PTSdiff/MPEG2 = %f/%f/%f, DTS/PTS count = %d/%d", (float)(1.0/BestVal), (float)m_minVideoDTSdiff, (float)m_minVideoPTSdiff, (float)m_curFramePeriod, m_vidDTScount, m_vidPTScount);  
          } 
          m_bLogFPSfromDTSPTS = true;
        } 
        else if (!m_bLogFPSfromDTSPTS) 
        {
          m_bLogFPSfromDTSPTS = true;
          LogDebug("demux:GetVideoStreamType(), FPS from DTS/PTS best match failed, DTSdiff/PTSdiff/MPEG2 = %f/%f/%f, DTS/PTS count = %d/%d", (float)m_minVideoDTSdiff, (float)m_minVideoPTSdiff, (float)m_curFramePeriod, m_vidDTScount, m_vidPTScount);  
        }
      }
      else if (!m_bLogFPSfromDTSPTS)
      {
        m_bLogFPSfromDTSPTS = true;
        LogDebug("demux:GetVideoStreamType(), FPS from DTS/PTS failed, DTSdiff/PTSdiff/MPEG2 = %f/%f/%f, DTS/PTS count = %d/%d", (float)m_minVideoDTSdiff, (float)m_minVideoPTSdiff, (float)m_curFramePeriod, m_vidDTScount, m_vidPTScount);  
      }
     
    }
    return true;
  }
  return false;
}

void CDeMultiplexer::FlushVideo(bool isMidStream)
{
  //LogDebug("demux:flush video");
  CAutoLock flock (&m_sectionFlushVideo);
  CAutoLock lock (&m_sectionVideo);
  
  if (m_vecVideoBuffers.size()>0)
  {
    ivecBuffers it = m_vecVideoBuffers.begin();
    for ( ; it != m_vecVideoBuffers.end() ; it++ )
    {
      CBuffer* videoBuffer = *it;
      delete videoBuffer;
    }
    m_vecVideoBuffers.clear();
  }
    
  m_p.Free();
  m_lastStart = 0;
  m_pl.RemoveAll();

  m_FirstVideoSample = 0x7FFFFFFF00000000LL;
  m_LastVideoSample = 0;
  m_ZeroVideoSample = 0;
  m_lastVideoPTS.IsValid = false;
  m_lastVideoDTS.IsValid = false;
  m_bLogFPSfromDTSPTS = false;
  m_bUsingGOPtimestamp = false;
  m_VideoValidPES = false;
  m_mVideoValidPES = false;
  m_WaitHeaderPES=-1 ;
  m_bVideoAtEof=false;
  m_MinVideoDelta = 10.0 ;
  _InterlockedAnd(&m_AVDataLowCount, 0) ;
  _InterlockedAnd(&m_AudioDataLowPauseTime, 0) ;
  if (!m_bShuttingDown)
  {
    m_filter.m_bRenderingClockTooFast=false;
  }
  m_bSetVideoDiscontinuity=true;
	m_VideoPrevCC = -1;

	m_bFrame0Found = false;

  if ((!m_filter.IsSeeking() && !isMidStream) || m_filter.IsTimeShifting() || !m_bFirstGopParsed)
  {
  	//Don't reset these when seeking in non-timeshift files, after we have parsed the first Gop
    m_mpegParserReset = true;
	  m_fHasAccessUnitDelimiters = false;	
	  m_bFirstGopFound = false;
	  m_bSecondGopFound = false;
  }
  if (m_filter.IsSeeking() && m_filter.IsTimeShifting() && m_filter.GetVideoPin()->IsConnected() && !IsMediaChanging())
  {
    m_mpegPesParser->VideoValidReset(); 
  }
  
  Reset();  // PacketSync reset.
}

void CDeMultiplexer::FlushAudio()
{
  //LogDebug("demux:flush audio");
  CAutoLock flock (&m_sectionFlushAudio);
  CAutoLock lock (&m_sectionAudio);
  delete m_pCurrentAudioBuffer;
  
  if (m_vecAudioBuffers.size()>0)
  {
    ivecBuffers it = m_vecAudioBuffers.begin();
    for ( ; it != m_vecAudioBuffers.end() ; it++ )
    {
      CBuffer* AudioBuffer = *it;
      delete AudioBuffer;
    }
    m_vecAudioBuffers.clear();
  }
  
  // Clear PES temporary queue.
  if (m_t_vecAudioBuffers.size()>0)
  {
    ivecBuffers it = m_t_vecAudioBuffers.begin();
    for ( ; it != m_t_vecAudioBuffers.end() ; it++ )
    {
      CBuffer* AudioBuffer=*it;
      delete AudioBuffer;
    }
    m_t_vecAudioBuffers.clear();
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
  _InterlockedAnd(&m_AudioDataLowPauseTime, 0) ;
  if (!m_bShuttingDown)
  {
    m_filter.m_bRenderingClockTooFast=false;
  }
  m_bSetAudioDiscontinuity=true;
  m_bAudioSampleLate=false;
  m_currentAudHeader = 0;
  m_lastAudHeader = 0;
  m_audHeaderCount = 0;
  m_audioBytesRead = 0;

  if (m_filter.IsSeeking() && m_filter.GetAudioPin()->IsConnected() && !IsMediaChanging())
  {
    m_mpegPesParser->AudioValidReset(); 
  }
  
  Reset();  // PacketSync reset.
}

void CDeMultiplexer::FlushCurrentAudio()
{
  //LogDebug("demux:flush current audio");
  CAutoLock flock (&m_sectionFlushAudio);
  CAutoLock lock (&m_sectionAudio);

  // Clear PES temporary queue.
  delete m_pCurrentAudioBuffer;
  
  if (m_t_vecAudioBuffers.size()>0)
  {
    ivecBuffers it = m_t_vecAudioBuffers.begin();
    for ( ; it != m_t_vecAudioBuffers.end() ; it++ )
    {
      CBuffer* AudioBuffer=*it;
      delete AudioBuffer;
    }
    m_t_vecAudioBuffers.clear();
  }

  m_pCurrentAudioBuffer = new CBuffer();

  m_AudioValidPES = false;
  m_bSetAudioDiscontinuity=true;
  m_currentAudHeader = 0;
  m_lastAudHeader = 0;
  m_audHeaderCount = 0;
  m_audioBytesRead = 0;

  m_mpegPesParser->AudioValidReset();   
}

void CDeMultiplexer::FlushSubtitle()
{
  //LogDebug("demux:flush subtitle");
  CAutoLock flock (&m_sectionFlushSubtitle);
  CAutoLock lock (&m_sectionSubtitle);
  delete m_pCurrentSubtitleBuffer;
  
  if (m_vecSubtitleBuffers.size()>0)
  {
    ivecBuffers it = m_vecSubtitleBuffers.begin();
    for ( ; it != m_vecSubtitleBuffers.end() ; it++ )
    {
      CBuffer* subtitleBuffer = *it;
      delete subtitleBuffer;
    }
    m_vecSubtitleBuffers.clear();
  }

  m_pCurrentSubtitleBuffer = new CBuffer();
}

/// Flushes all buffers
void CDeMultiplexer::Flush(bool clearAVready, bool isMidStream)
{
  if (m_bFlushRunning) return;
    
  LogDebug("demux:Flush(), clearAVready = %d, isMidStream = %d", clearAVready, isMidStream);

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
  FlushVideo(isMidStream);
  FlushSubtitle();
  m_bFlushDelegated = false;
  m_bReadAheadFromFile = false;  
  m_fileReadLatency = 0;
  m_maxFileReadLatency = 0;
  m_fileReadLatSum = 0;
  m_fileReadLatCount = 0;
  
  if (clearAVready)
  {
    m_filter.m_bStreamCompensated=false ;
    m_initialAudioSamples = 3;
    m_initialVideoSamples = 12;
    m_prefetchLoopDelay = PF_LOOP_DELAY_MIN;
    m_filter.m_audioReady = false;
  }
  
  m_bFlushRunning = false;
}

///
///Returns the next subtitle packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetSubtitle()
{
  if (m_bFlushDelgNow || m_bFlushRunning || m_bStarting) return NULL; //Flush pending or Start() active
  if (IsAudioChanging()) return NULL; //Waiting for MP player to do something....

  if ((m_pids.subtitlePids.size() > 0 && m_pids.subtitlePids[0].Pid==0) || IsMediaChanging())
  {
    return NULL;
  }
  //if there is no subtitle pid, then simply return NULL
  if (m_currentSubtitlePid==0) return NULL;
  if (m_bEndOfFile) return NULL;
  
  CAutoLock lock (&m_sectionSubtitle);
  
  //are there subtitle packets in the buffer?
  if (m_vecSubtitleBuffers.size()>0 )
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
  if (IsAudioChanging()) return NULL; //Waiting for MP player to do something....

  //if there is no video pid, then simply return NULL
  if ((m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid==0) || IsMediaChanging())
  {
    return NULL;
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
  if (IsAudioChanging()) return NULL; //Waiting for MP player to do something....

  // if there is no audio pid, then simply return NULL
  if ((m_audioPid==0) || IsMediaChanging())
  {
    return NULL;
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
    CRefTime firstVideo, lastVideo, zeroVideo;
    cntA = GetAudioBufferPts(firstAudio, lastAudio); // this one...
    cntV = GetVideoBufferPts(firstVideo, lastVideo, zeroVideo);
    
    // Goal is to start with at least 500mS audio and 400mS video ahead. ( LiveTv and RTSP as TsReader cannot go ahead by itself)
    if (lastAudio.Millisecs() - firstAudio.Millisecs() < (MIN_AUD_BUFF_TIME + m_filter.m_regInitialBuffDelay)) return false ;       // Not enough audio to start.

    int vidSampDurPrefetch = PF_LOOP_DELAY_MAX;
    double fvidSampleDuration = 0;
    if (m_filter.GetVideoPin()->IsConnected())
    {
      if (!m_bFrame0Found) return NULL ;
        
      if (lastVideo.Millisecs() - zeroVideo.Millisecs() < (MIN_VID_BUFF_TIME + m_filter.m_regInitialBuffDelay)) return false ;   // Not enough video to start.
      
      if (!m_filter.m_EnableSlowMotionOnZapping)
      {
        if (lastAudio.Millisecs() - zeroVideo.Millisecs() < 100) return false ;   // Not enough simultaneous audio & video to start.
      }  
           
      //Set video prefetch threshold
      fvidSampleDuration = ((double)(lastVideo.Millisecs() - firstVideo.Millisecs())/(double)cntV);
      m_initialVideoSamples = (int)(((double)(MIN_VID_BUFF_TIME + m_filter.m_regInitialBuffDelay))/fvidSampleDuration);    
      m_initialVideoSamples = max(12, m_initialVideoSamples);
      vidSampDurPrefetch = min(PF_LOOP_DEL_VID_MAX, max(PF_LOOP_DELAY_MIN,(int)fvidSampleDuration));
    }

    //Set audio prefetch threshold
    double faudSampleDuration = ((double)(lastAudio.Millisecs() - firstAudio.Millisecs())/(double)cntA);
    m_initialAudioSamples = (int)(((double)(MIN_AUD_BUFF_TIME + m_filter.m_regInitialBuffDelay))/faudSampleDuration);
    m_initialAudioSamples = max(3, m_initialAudioSamples);
    int audSampDurPrefetch = max(PF_LOOP_DELAY_MIN,(int)faudSampleDuration);
    m_prefetchLoopDelay = (DWORD)(min(PF_LOOP_DELAY_MAX, min(vidSampDurPrefetch, audSampDurPrefetch)));
    m_dfAudSampleDuration = faudSampleDuration/1000.0;

    LogDebug("demux:CheckCompensation(): Audio Samples : %d, First : %03.3f, Last : %03.3f, buffThresh : %d, pfLoopDel : %d, SampDur : %03.1f ms",cntA, (float)firstAudio.Millisecs()/1000.0f,(float)lastAudio.Millisecs()/1000.0f, m_initialAudioSamples, m_prefetchLoopDelay, (float)faudSampleDuration);
    LogDebug("demux:CheckCompensation(): Video Samples : %d, First : %03.3f, Last : %03.3f, Zero : %03.3f, buffThresh : %d, SampDur : %03.1f ms",cntV, (float)firstVideo.Millisecs()/1000.0f,(float)lastVideo.Millisecs()/1000.0f, (float)zeroVideo.Millisecs()/1000.0f, m_initialVideoSamples, (float)fvidSampleDuration);

    // Ambass : Find out the best compensation to apply in order to have fast Audio/Video delivery
    CRefTime BestCompensation;
    CRefTime AddVideoCompensation;   
              
    if (m_filter.GetVideoPin()->IsConnected())
    {        
      if (firstAudio < zeroVideo)
      {
        //Make sure there is a minimum amount of audio available at the start
        CRefTime targFirstAudio = lastAudio - (REFERENCE_TIME)(fmax((double)(m_filter.m_regInitialBuffDelay + MIN_AUD_BUFF_TIME), faudSampleDuration*1.5) * 10000);
        if (targFirstAudio < firstAudio)
        {
          //Use the timestamp of the earliest audio sample we have
          targFirstAudio = firstAudio;
        }
        
        if (zeroVideo < targFirstAudio)
        {
          //Align audio start with video start
          targFirstAudio = zeroVideo;
        }
        
        BestCompensation = (targFirstAudio - rtStartTime) - m_filter.m_RandomCompensation ;
        AddVideoCompensation = zeroVideo - targFirstAudio;
        AddVideoCompensation = (AddVideoCompensation > (5000*10000)) ? (5000*10000) : AddVideoCompensation; //Limit to 5.0 seconds
        LogDebug("demux:CheckCompensation(): (AudBackBuff : %03.3f) Audio pts < Video pts. Add %03.3f sec of extra video comp", (float)(lastAudio.Millisecs()-targFirstAudio.Millisecs())/1000.0f,(float)AddVideoCompensation.Millisecs()/1000.0f) ;       
      }
      else
      {
        BestCompensation = (firstAudio-rtStartTime) - m_filter.m_RandomCompensation  ;
        AddVideoCompensation = 0 ;
        LogDebug("demux:CheckCompensation() : Audio pts > Video Pts (Recover skipping Video)...") ;
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
        LogDebug("demux:CheckCompensation(): Elapsed time from pause to Audio/Video ( total zapping time ) : %d mS",GET_TIME_NOW()-m_filter.m_lastPauseRun);
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

    LogDebug("demux:CheckCompensation(): Compensation = %03.3f, Clock on start %03.3f rtStartTime:%d ",(float)m_filter.Compensation.Millisecs()/1000.0f, m_filter.m_ClockOnStart.Millisecs()/1000.0f, rtStartTime.Millisecs());

    m_targetAVready = GET_TIME_NOW() + AV_READY_DELAY;
    //set flag so we dont keep compensating
    m_filter.m_bStreamCompensated = true;
    m_bSubtitleCompensationSet = false;
  }

  // Subtitle filter is "found" only after Run() has been completed
  if(!m_bSubtitleCompensationSet)
  {
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if(pDVBSubtitleFilter)
    {
      LogDebug("demux:CheckCompensation(): pDVBSubtitleFilter->SetTimeCompensation");
      pDVBSubtitleFilter->SetTimeCompensation(m_filter.GetCompensation());
      m_bSubtitleCompensationSet=true;
    }
  }
  return true;
}

/// Starts the demuxer
/// This method will read the file until we found the pat/sdt
/// with all the audio/video pids
bool CDeMultiplexer::Start(DWORD timeout)
{
  //reset some values
  m_bStarting=true ;
  m_mpegParserReset = true;  
  m_bFirstGopParsed = false; 
  m_mpegPesParser->VideoReset(); 
  m_mpegPesParser->AudioReset(); 
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
  m_filter.m_audioReady = false;
  m_initialAudioSamples = 3;
  m_initialVideoSamples = 12;
  m_prefetchLoopDelay = PF_LOOP_DELAY_MIN;
  m_vidPTScount = 0;
  m_vidDTScount = 0;
  m_bUsingGOPtimestamp = false;
  int dwBytesProcessed=0;
  m_reader->SetStopping(true); //Stop outstanding IO etc 
  m_reader->SetStopping(false);    
  CAutoLock lock (&m_filter.m_ReadAheadLock);
  DWORD m_Time = GET_TIME_NOW();
  m_hadPESfail = 0;
  
  while (dwBytesProcessed < INITIAL_READ_SIZE && (GET_TIME_NOW() - m_Time) < timeout)
  {
    m_bEndOfFile = false;  //reset eof every time through to ignore a false eof due to slow rtsp startup
    int BytesRead = ReadFromFile(READ_SIZE);    
    if (BytesRead <= 0)
    {
      BytesRead = 0;
      Sleep(10);
    }      
    dwBytesProcessed+=BytesRead;

    if (m_hadPESfail > 64)
    {
      //Probably initial decryption problems so allow more time....
      timeout = 60000;
    }
	  
    if (GetAudioStreamCount()>0) //Wait for first PAT to be found
    {
      if (!m_mpegPesParser->basicAudioInfo.isValid) continue; //The audio hasn't been parsed...

      //Wait for the first video GOP header to be parsed (if there is a video stream)
      //so that OnVideoFormatChanged() can be triggered if necessary. 
      if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid > 1) //There is a video stream
      {
        if (!m_mpegPesParser->basicVideoInfo.isValid) continue;  //The first GOP header is not parsed...
        if (m_filter.m_bUseFPSfromDTSPTS && !m_bUsingGOPtimestamp && m_vidPTScount < 6 && m_vidDTScount < 6) continue;  //We havent seen enough PTS/DTS timestamps....
      }
            
      //Success !!
      //Move back to beginning of file (or RTSP memory buffer)
      m_filter.SetSeeking(true); //Treat this as a 'seek' operation.
      m_reader->SetFilePointer(0,FILE_BEGIN);
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      DelegatedFlush(true, true);
      m_filter.SetSeeking(false);
      m_streamPcr.Reset();
      m_bStarting=false;
	    LogDebug("demux:Start() Succeeded : BytesProcessed:%d, DTS/PTS count = %d/%d, GOPts = %d", dwBytesProcessed+BytesRead, m_vidDTScount, m_vidPTScount, m_bUsingGOPtimestamp);
      return true;
    }
    Sleep(1);
  }
  
  m_streamPcr.Reset();
  m_iAudioReadCount=0;
  m_bStarting=false;
	LogDebug("demux:Start() Failed due to timeout : BytesProcessed:%d, DTS/PTS count = %d/%d, BVI=%d, BAI=%d", dwBytesProcessed, m_vidDTScount, m_vidPTScount, m_mpegPesParser->basicVideoInfo.isValid, m_mpegPesParser->basicAudioInfo.isValid);
  return false;
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

int CDeMultiplexer::ReadAheadFromFile(ULONG lDataLength)
{  
  CAutoLock lock (&m_filter.m_ReadAheadLock);

  //if filter is stopped or
  //end of file has been reached or
  //demuxer should stop getting video packets
  //then return an error
  if ((m_filter.State() == State_Stopped) || !m_filter.IsFilterRunning() || 
       m_filter.IsStopping() || m_filter.IsSeeking() || m_bEndOfFile || m_bStarting) 
  {
    return -1;
  }
  
	//LogDebug("demux:ReadAheadFromFile");
  int SizeRead = ReadFromFile(lDataLength) ;
  
  if (m_filter.State() != State_Running)
  {
    _InterlockedAnd(&m_AVDataLowCount, 0);
    _InterlockedAnd(&m_AudioDataLowPauseTime, 0) ;
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
  }

  return SizeRead;
}


/// This method reads the next lDataLength bytes from the file
/// and processes the raw data
/// When a TS packet has been discovered, OnTsPacket(byte* tsPacket) gets called
//  which in its turn deals with the packet
int CDeMultiplexer::ReadFromFile(ULONG lDataLength)
{
   // Don't read if flush pending/running or no reader....
  if (m_filter.IsSeeking() || m_bFlushDelgNow || m_bFlushRunning || (m_reader==NULL)) 
  {
    return -1;
  }

  if (!m_pFileReadBuffer || (lDataLength > READ_SIZE))
  {
    LogDebug("CDeMultiplexer::ReadFromFile() - Buffer ERROR !!");
    return -1;
  }
    
  CAutoLock lock (&m_sectionRead);
  int dwReadBytes=0;
  //if we are playing a RTSP stream
  if (m_reader->IsBuffer())
  {    
    if (m_reader->HasData() < 0)
    {
      //Buffer not running
      return -1;
    }      
    //Read raw data from the buffer
    DWORD readFileTime = GET_TIME_NOW();
    m_reader->Read(m_pFileReadBuffer, lDataLength, (DWORD*)&dwReadBytes);
    m_fileReadLatency = GET_TIME_NOW() - readFileTime; 
    m_fileReadLatSum += m_fileReadLatency;
    m_fileReadLatCount++;  
    if (m_fileReadLatency > m_maxFileReadLatency)
    {
      m_maxFileReadLatency = m_fileReadLatency;
    }
    if (dwReadBytes < (int)lDataLength)
    {
      m_bAudioAtEof = true;
      m_bVideoAtEof = true;
    }
    if (dwReadBytes > 0)
    {
      //yes, then process the raw data
      if (OnRawData2(m_pFileReadBuffer,(int)dwReadBytes))
      {
        Sleep(200); //Not enough data to initially sync or re-sync to stream
      }
      m_LastDataFromRtsp = GET_TIME_NOW();
    }
    else
    {
      if (!m_filter.IsTimeShifting())
      {
        //LogDebug("demux:endoffile...%d",GET_TIME_NOW()-m_LastDataFromRtsp );
        //set EOF flag and return
        if (((GET_TIME_NOW()-m_LastDataFromRtsp) > RTSP_EOF_TIMEOUT) && (m_filter.State() != State_Paused) ) // A bit crappy, but no better idea...
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
    //playing a local file or using UNC path
    //read raw data from the file
    DWORD readFileTime = GET_TIME_NOW();
    __int64 filePointer = m_reader->GetFilePointer(); //store current pointer for re-reads if required for errors
    HRESULT readResult = m_reader->Read(m_pFileReadBuffer, lDataLength, (DWORD*)&dwReadBytes);
    m_fileReadLatency = GET_TIME_NOW() - readFileTime;    
    m_fileReadLatSum += m_fileReadLatency;
    m_fileReadLatCount++;  
    if (m_fileReadLatency > m_maxFileReadLatency)
    {
      m_maxFileReadLatency = m_fileReadLatency;
    }

    //check data integrity
    if (m_filter.m_bEnableBufferLogging && (SUCCEEDED(readResult)) && (dwReadBytes > 0))
    {
      int syncErrors = OnRawDataCheck(m_pFileReadBuffer,(int)dwReadBytes);
      if (syncErrors != 0)
      {
        LogDebug("demux:ReadFromFile() syncErrors: %d, bufferSize: %d, filePointer: %d", syncErrors, dwReadBytes, filePointer);
      }
    }

    if (SUCCEEDED(readResult))
    {
      if ((m_filter.IsTimeShifting()) && (dwReadBytes < (int)lDataLength))
      {
        m_bAudioAtEof = true;
        m_bVideoAtEof = true;
      }

      if (dwReadBytes > 0)
      {        
        //process data
        if (OnRawData2(m_pFileReadBuffer,(int)dwReadBytes))       
        {
          //Not enough data to initially sync ro re-sync to stream, so stall for a while
          Sleep(200);
        }
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
      //LogDebug("CDeMultiplexer::ReadFromFile() - Read failed, HRESULT = 0x%x", readResult);
      return -2;      
    }
  }
  //Read failure/error
  LogDebug("CDeMultiplexer::ReadFromFile() - Read failed...");
  return -2;
  // return 0;
}



/// This method gets called via ReadFile() when a new TS packet has been received
/// if will :
///  - decode any new pat/pmt/sdt
///  - decode any audio/video packets and put the PES packets in the appropiate buffers
void CDeMultiplexer::OnTsPacket(byte* tsPacket, int bufferOffset, int bufferLength)
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
        (*pTeletextServiceInfoCallback)(info.page, (byte)info.type, (byte)info.lang[0],(byte)info.lang[1],(byte)info.lang[2]);
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
  FillVideo(header,tsPacket, bufferOffset, bufferLength);
  FillAudio(header,tsPacket, bufferOffset, bufferLength);
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

// This method will check if the tspacket is an audio packet
// ifso, it decodes the PES audio packet and stores it in the audio buffers
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket, int bufferOffset, int bufferLength)
{
  //LogDebug("FillAudio - audio PID %d", m_audioPid );
  CAutoLock flock (&m_sectionFlushAudio);

  if (IsAudioChanging() || m_iAudioStream>=m_audioStreams.size()) return;
    
  m_audioPid=m_audioStreams[m_iAudioStream].pid;
  
  if (m_audioPid==0 || m_audioPid != header.Pid) return;
  if (header.AdaptionFieldOnly())return;

  if(!CheckContinuity(m_AudioPrevCC, header))
  {
    LogDebug("Audio Continuity error... %x ( prev %x ), bufferOffset=%d, bufferLength=%d", header.ContinuityCounter, m_AudioPrevCC, bufferOffset, bufferLength);
    if (!m_DisableDiscontinuitiesFiltering) 
    {
      m_AudioValidPES=false;  
    }
  }

  m_AudioPrevCC = header.ContinuityCounter;

  //LogDebug("FillAudio() process TS packet");

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
      if (m_hadPESfail < 256)
      {
        m_hadPESfail++;
      }
      //LogDebug("PES audio 0-0-1 fail");
      LogDebug("PES audio 0-0-1 fail, PES hdr = %x-%x-%x-%x-%x-%x-%x-%x, TS hdr = %x-%x-%x-%x-%x-%x-%x-%x-%x-%x", 
                                                                          tsPacket[posn+0], tsPacket[posn+1], tsPacket[posn+2], tsPacket[posn+3],                                                                            
                                                                          tsPacket[posn+4], tsPacket[posn+5], tsPacket[posn+6], tsPacket[posn+7],                                                                           
                                                                          tsPacket[0], tsPacket[1], tsPacket[2], tsPacket[3], tsPacket[4],
                                                                          tsPacket[5], tsPacket[6], tsPacket[7], tsPacket[8], tsPacket[9]);
      //header.LogHeader();
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      DelegatedFlush(false, false);
      return;
    }
    
    //yes, packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentAudioBuffer->Length() > 0)
    {
      m_t_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      m_pCurrentAudioBuffer = new CBuffer();
    }

    //write in pes header data (only)
    int headerLen=9+tsPacket[posn+8] ;
    if (headerLen>0 && headerLen < 188)
    {
      m_pCurrentAudioBuffer->Add(&tsPacket[posn],headerLen);
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
            m_lastVideoDTS.IsValid=false;
            m_bSetAudioDiscontinuity=true;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            DelegatedFlush(false, false);
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
            float Delta = (float)(pts.ToClock()-((double)(m_filter.Compensation.m_time+MediaTime)/10000000.0)) ;
            if (Delta < m_MinAudioDelta)
            {
              m_MinAudioDelta=Delta;
              if (Delta < -2.0)
              {
                //Large negative delta - flush the world...
                LogDebug("Demux : Audio to render too late= %03.3f Sec, FileReadLatency: %d ms, flushing", Delta, m_fileReadLatency) ;
                m_MinAudioDelta+=1.0;
                m_MinVideoDelta+=1.0;                
                //Flushing is delegated to CDeMultiplexer::ThreadProc()
                DelegatedFlush(false, false);
              }
              else if (Delta < 0.1)
              {
                LogDebug("Demux : Audio to render too late= %03.3f Sec, FileReadLatency: %d ms", Delta, m_fileReadLatency) ;
                _InterlockedIncrement(&m_AVDataLowCount);   
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
        int lastADTSheaderPosn = 0;
        if (len > 0)
        {
          //Check if we need to try a different AAC packetisation format (workaround for incorrectly described stream type in PMT)
          if (!m_mpegPesParser->basicAudioInfo.isValid)
          {
            m_audioBytesRead += len;
            if (m_audioBytesRead > 32768 && m_audHeaderCount < 8) //Failed to find audio header in a reasonable time
            {
              if (m_AudioStreamType == SERVICE_TYPE_AUDIO_AAC)
              {
                LogDebug("demux: FillAudio() AAC, Swap from ADTS to LATM, bytesRead = %d, headerCount = %d", m_audioBytesRead, m_audHeaderCount);              
                m_AudioStreamType = SERVICE_TYPE_AUDIO_LATM_AAC;
                m_audioStreams[m_iAudioStream].audioType = SERVICE_TYPE_AUDIO_LATM_AAC;
                m_currentAudHeader = 0;
                m_lastAudHeader = 0;
                m_audHeaderCount = 0;
                m_audioBytesRead = 0;
              }
              else if (m_AudioStreamType == SERVICE_TYPE_AUDIO_LATM_AAC)
              {
                LogDebug("demux: FillAudio() AAC, Swap from LATM to ADTS, bytesRead = %d, headerCount = %d", m_audioBytesRead, m_audHeaderCount);              
                m_AudioStreamType = SERVICE_TYPE_AUDIO_AAC;
                m_audioStreams[m_iAudioStream].audioType = SERVICE_TYPE_AUDIO_AAC;
                m_currentAudHeader = 0;
                m_lastAudHeader = 0;
                m_audHeaderCount = 0;
                m_audioBytesRead = 0;
              }
            }
          }

          byte *ps = p+headerLen;
          int length = len;
          bool foundAudHeader = false;
          int copyLen = 0;
          
          if (m_AudioStreamType == SERVICE_TYPE_AUDIO_AAC) //ADTS AAC audio stream - requires data frame re-alignment 
          {
            // LogDebug("ADTS start PES = %d", len);
            while(len) 
            {
              //Find correct ADTS frame header sync sequence by 'learning' the most frequent 28 bit header start pattern
              if (((*(INT16 *)ps & 0xF6FF) == 0xF0FF) && len > 6) //Syncword bits==111111111111 and Layer bits==00
              {     
                //byte hObjectType = ((*(ps+2) & 0xC0)>>6);
                byte hFreq     = ((*(ps+2) & 0x3C)>>2);
                byte hChannels = ((*(ps+2) & 0x01)<<2) | ((*(ps+3) & 0xC0)>>6);                      
                byte hRDBs     = *(ps+6) & 0x03; //Raw data blocks per frame

                if (hFreq>2 && hFreq<9 && hChannels<7 && hChannels>0 && hRDBs==0) //Sanity checks...
                {
                  if (m_audHeaderCount<16)  // Learning/training state
                  {
                    if (m_currentAudHeader == (*(INT32 *)ps & 0x30FEFFFF)) //compare first 28 bits only
                    {
                      m_audHeaderCount+=4;
                    }  
                    else if (m_lastAudHeader != (*(INT32 *)ps & 0x30FEFFFF))
                    {
                      m_lastAudHeader = *(INT32 *)ps & 0x30FEFFFF;
                      if (m_audHeaderCount==0) 
                      {
                        m_currentAudHeader = m_lastAudHeader;
                      } 
                    }  
                    else 
                    {  
                      m_currentAudHeader = (*(INT32 *)ps & 0x30FEFFFF); //only first 28 bits are relevant, and channel count is excluded
                      LogDebug("demux: ADTS AAC resync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                      if (m_audHeaderCount>0) 
                      {
                        m_audHeaderCount--;
                      } 
                      m_mpegPesParser->basicAudioInfo.isValid = false;     
                    }       
                    // LogDebug("ADTS find sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                  }
                  else // m_audHeaderCount>=16, 'locked' state
                  {
                    if (m_lastAudHeader != (*(INT32 *)ps & 0x30FEFFFF))
                    {
                      m_lastAudHeader = *(INT32 *)ps & 0x30FEFFFF;
                      if (m_currentAudHeader == m_lastAudHeader)
                      {
                        //Good header
                        foundAudHeader = true;
                        lastADTSheaderPosn = len;
                      }  
                    }  
                    else if (m_currentAudHeader != (*(INT32 *)ps & 0x30FEFFFF))  //compare first 28 bits only
                    {
                      m_audHeaderCount--; //invalid (or changing) header sequence
                    }  
                    else //good header sequence
                    {
                      foundAudHeader = true;
                      lastADTSheaderPosn = len;
                      if (!m_mpegPesParser->basicAudioInfo.isValid && len > 8)
                      {
                        m_mpegPesParser->OnAudioPacket(ps, len, m_AudioStreamType, m_iAudioStream, true);
                        m_bSetAudioDiscontinuity=true;
                        //LogDebug("demux: AAC ADTS parsedChannels = %d", hChannels);
  						          LogDebug("demux: AAC ADTS header: sampleRate = %d, channels = %d, bitrate = %d, objectType = %d, bytesRead = %d", m_mpegPesParser->basicAudioInfo.sampleRate, m_mpegPesParser->basicAudioInfo.channels, m_mpegPesParser->basicAudioInfo.bitrate, m_mpegPesParser->basicAudioInfo.aacObjectType, m_audioBytesRead);
                      }
                      else
                      {
                        if (hChannels != m_mpegPesParser->basicAudioInfo.channels)
                        {
      				            LogDebug("demux: AAC ADTS channels = %d -> %d, header = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", m_mpegPesParser->basicAudioInfo.channels, hChannels, *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                          CAutoLock plock (&m_mpegPesParser->m_sectionAudioPmt);
                          m_mpegPesParser->basicAudioInfo.channels=hChannels;
                          Cbuf->SetForcePMT();
                        }
                      }
                      if (m_audHeaderCount<32)
                      {
                        if (m_audHeaderCount>27 && m_audHeaderCount<31)
                        {
                          LogDebug("demux: AAC ADTS good sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                        }
                        m_audHeaderCount += min(4, 32-m_audHeaderCount);
                      }
                    }
                  }     
                }             
              }
              
              if (foundAudHeader)
              {
                copyLen++;
                *p++ = *ps;   // memcpy could be not safe.
              }
              ps++;
              len--;            
            } 
                                     
            Cbuf->SetLength(copyLen - lastADTSheaderPosn); //Adjust length to discard 'remainder' incomplete ADTS frame data
            
            if (lastADTSheaderPosn > 0) //Add the 'remainder' incomplete ADTS frame data to the next (current incoming) PES buffer - the PES header data is already there.
            {
              m_pCurrentAudioBuffer->Add((byte *)(p-lastADTSheaderPosn),lastADTSheaderPosn);
            }
          }
          else if (m_AudioStreamType == SERVICE_TYPE_AUDIO_LATM_AAC)
          {
            //LogDebug("LATM start PES = %d", len);
            while(len) 
            {
              //Find correct LATM/LAOS frame header sync sequence by 'learning' the correct header start pattern
              if ((*(INT16 *)ps & 0xE0FF) == 0xE056 && len > 6) //Syncword bits==0x2B7 (first 11 bits)
              {     
                //LogDebug("demux: LATM AAC syncword found = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d, frame len = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount, ((*(ps+1) & 0x1f) << 8) + *(ps+2));
                //if ((*(ps+3) & 0xC) == 0x0) //Preamble to AudioSpecificConfig() data
                if (*(INT16 *)(ps+3) == 0x0020) //Preamble to AudioSpecificConfig() data
                {     
                  //LogDebug("demux: LATM AAC preamble found = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                  byte hObjectType = ((*(ps+5) & 0xF8)>>3);
                  byte hFreq = ((*(ps+5) & 0x07) <<1) | ((*(ps+6) & 0x80)>>7);
                  byte hChannels = ((*(ps+6) & 0x78)>>3);                      
                  if (hFreq>2 && hFreq<9 && hChannels<7 && hChannels>0 && (hObjectType==2 || hObjectType==5)) //Sanity checks...
                  {
                    //Found a possible good header....
                    if (m_audHeaderCount<8)  // Learning/training state
                    {
                      if (m_currentAudHeader == (*(INT16 *)(ps+5) & 0x87FF)) //AudioSpecificConfig(), channel count is excluded
                      {
                        m_audHeaderCount+=4;
                      }  
                      else if (m_lastAudHeader != (*(INT16 *)(ps+5) & 0x87FF))
                      {
                        m_lastAudHeader = *(INT16 *)(ps+5) & 0x87FF;
                        if (m_audHeaderCount==0) 
                        {
                          m_currentAudHeader = m_lastAudHeader;
                        } 
                      }  
                      else 
                      {  
                        m_currentAudHeader = (*(INT16 *)(ps+5) & 0x87FF); //AudioSpecificConfig(), channel count is excluded
                        LogDebug("demux: LATM AAC resync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                        if (m_audHeaderCount>0) 
                        {
                          m_audHeaderCount--;
                        }
                      }       
                    }
                    else // 'locked' state
                    {
                      if (m_lastAudHeader != (*(INT16 *)(ps+5) & 0x87FF))
                      {
                        m_lastAudHeader = *(INT16 *)(ps+5) & 0x87FF;
                      }  
                      else if (m_currentAudHeader != (*(INT16 *)(ps+5) & 0x87FF))  //AudioSpecificConfig(), channel count is excluded
                      {
                        m_audHeaderCount--; //invalid (or changing) header sequence
                      }  
                      else if (m_audHeaderCount<16) //good header sequence
                      {
                        if (m_audHeaderCount>11 && m_audHeaderCount<15)
                        {
                          LogDebug("demux: AAC LATM good sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                        }
                        m_audHeaderCount += min(4, 16-m_audHeaderCount);
                      }
                    }                  

                    //LATM find sync = 56 e3 52 20 0 11 b0, byteCount = 0, headerCount = 0
                    if (!m_mpegPesParser->basicAudioInfo.isValid)
                    {                    
                      CAutoLock plock (&m_mpegPesParser->m_sectionAudioPmt);
                      m_mpegPesParser->OnAudioPacket(0, 0, m_AudioStreamType, m_iAudioStream, true); //Generate default info
                      //Modify with parsed info
                      static int freq[] = {96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11025, 8000, 7350};                     
                      m_mpegPesParser->basicAudioInfo.sampleRate = freq[hFreq];                      
                      m_mpegPesParser->basicAudioInfo.channels = hChannels;
                      m_mpegPesParser->basicAudioInfo.aacObjectType = hObjectType;                      
  				            LogDebug("demux: AAC LATM header: sampleRate = %d, channels = %d, bitrate = %d, objectType = %d, bytesRead = %d", m_mpegPesParser->basicAudioInfo.sampleRate, m_mpegPesParser->basicAudioInfo.channels, m_mpegPesParser->basicAudioInfo.bitrate, m_mpegPesParser->basicAudioInfo.aacObjectType, m_audioBytesRead);
                      //Update the PMT on the output pin
                      m_bSetAudioDiscontinuity=true;
                    }
                    else if (m_audHeaderCount==16 && m_currentAudHeader==(*(INT16 *)(ps+5) & 0x87FF))
                    {
                      if (hChannels != m_mpegPesParser->basicAudioInfo.channels)
                      {                        
      				          LogDebug("demux: AAC LATM channels = %d -> %d, header = %x %x %x %x %x %x %x", m_mpegPesParser->basicAudioInfo.channels, hChannels, *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6));
                        CAutoLock plock (&m_mpegPesParser->m_sectionAudioPmt);
                        m_mpegPesParser->basicAudioInfo.channels=hChannels;
                        Cbuf->SetForcePMT();
                      }
                    }   
                  }           
                }
              }
              
              copyLen++;
              *p++ = *ps++;   // memcpy could be not safe.
              len--;            
            }
            
            Cbuf->SetLength(copyLen);            
          }         
          else if (m_AudioStreamType == SERVICE_TYPE_AUDIO_AC3)
          {
            // LogDebug("AC3 start PES = %d", len);
            while(len) 
            {
              //Find correct AC3 frame header sync sequence by 'learning' the current header pattern
              if (((*(INT16 *)ps & 0xFFFF) == 0x770b) && len > 6) //Syncword bits==0x0b77
              {     
                //LogDebug("demux: AC3 all sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);

                if (m_audHeaderCount<16)  // Learning/training state
                {
                  if (m_currentAudHeader == *(INT16 *)(ps+4)) //fscod, frmsizcod, bsid, bsmod fields
                  {
                    m_audHeaderCount+=4;
                  }  
                  else if (m_lastAudHeader != *(INT16 *)(ps+4))
                  {
                    m_lastAudHeader = *(INT16 *)(ps+4);
                    if (m_audHeaderCount==0) 
                    {
                      m_currentAudHeader = m_lastAudHeader;
                    } 
                  }  
                  else 
                  {  
                    m_currentAudHeader = *(INT16 *)(ps+4); //fscod, frmsizcod, bsid, bsmod fields
                    LogDebug("demux: AC3 resync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                    if (m_audHeaderCount>0) 
                    {
                      m_audHeaderCount--;
                    } 
                  }       
                }
                else // 'locked' state
                {
                  if (m_lastAudHeader != *(INT16 *)(ps+4))
                  {
                    m_lastAudHeader = *(INT16 *)(ps+4);
                  }  
                  else if (m_currentAudHeader != *(INT16 *)(ps+4)) //fscod, frmsizcod, bsid, bsmod fields
                  {
                    m_audHeaderCount--; //invalid (or changing) header sequence
                    //LogDebug("demux: AC3 lkd bad sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                  }  
                  else //good header sequence
                  {
                    foundAudHeader = true;
                    if (!m_mpegPesParser->basicAudioInfo.isValid)
                    {
                      m_mpegPesParser->OnAudioPacket(ps, len, m_AudioStreamType, m_iAudioStream, true);
                      m_bSetAudioDiscontinuity=true;
                      
                      //Parse the channel count
                      byte bsi = *(ps+6);
                      byte acmod = (bsi & 0xe0)>>5;
                      
                      //Get the 'lfeon' bit in the correct position
                    	if((acmod & 1) && acmod != 1) bsi<<=2;
                    	if(acmod & 4) bsi<<=2;
                    	if(acmod == 2) bsi<<=2;

                    	static int channels[] = {2, 1, 2, 3, 3, 4, 4, 5};
                      byte parsedChannels = channels[acmod] + ((bsi & 0x10)>>4); //Add one channel for 'lfeon'
						          LogDebug("demux: AC3 header: sampleRate = %d, channels = %d, bitrate = %d, parsedChannels = %d, bytesRead = %d", m_mpegPesParser->basicAudioInfo.sampleRate, m_mpegPesParser->basicAudioInfo.channels, m_mpegPesParser->basicAudioInfo.bitrate, parsedChannels, m_audioBytesRead);
                    }
                    else
                    {
                      //Parse the channel count
                      byte bsi = *(ps+6);
                      byte acmod = (bsi & 0xe0)>>5;
                      
                      //Get the 'lfeon' bit in the correct position - note the maximum total left shift is 4 bits
                    	if((acmod & 1) && acmod != 1) bsi<<=2;
                    	if(acmod & 4) bsi<<=2;
                    	if(acmod == 2) bsi<<=2;

                    	static int channels[] = {2, 1, 2, 3, 3, 4, 4, 5};
                      byte parsedChannels = channels[acmod] + ((bsi & 0x10)>>4); //Add one channel for 'lfeon'
                      if (parsedChannels<7 && parsedChannels>0 && (parsedChannels != m_mpegPesParser->basicAudioInfo.channels))
                      {
    				            LogDebug("demux: AC3 channels = %d -> %d", m_mpegPesParser->basicAudioInfo.channels, parsedChannels);
                        CAutoLock plock (&m_mpegPesParser->m_sectionAudioPmt);
                        m_mpegPesParser->basicAudioInfo.channels=parsedChannels;
                        Cbuf->SetForcePMT();
                      }
                    }
                    if (m_audHeaderCount<32)
                    {
                      if (m_audHeaderCount>27 && m_audHeaderCount<31)
                      {
                        LogDebug("demux: AC3 good sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                      }
                      m_audHeaderCount += min(4, 32-m_audHeaderCount);
                    }
                  }
                }                  
              }
              
              copyLen++;
              *p++ = *ps++;   // memcpy could be not safe.
              len--;            
            }
            
            Cbuf->SetLength(copyLen);            
          }         
          else if (m_AudioStreamType == SERVICE_TYPE_AUDIO_DD_PLUS ||
                   m_AudioStreamType == SERVICE_TYPE_AUDIO_E_AC3)
          {
            // LogDebug("E-AC3 start PES = %d", len);
            while(len) 
            {
              //Find correct AC3 frame header sync sequence by 'learning' the current header pattern
              if (((*(INT16 *)ps & 0xFFFF) == 0x770b) && len > 6) //Syncword bits==0x0b77
              {     
                //LogDebug("demux: E-AC3 all sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);

                if (m_audHeaderCount<16)  // Learning/training state
                {
                  if (m_currentAudHeader == (*(INT16 *)(ps+4) & 0xFFF0)) //fscod, fscod2, bsid, bsmod fields
                  {
                    m_audHeaderCount+=4;
                  }  
                  else if (m_lastAudHeader != (*(INT16 *)(ps+4) & 0xFFF0))
                  {
                    m_lastAudHeader = *(INT16 *)(ps+4) & 0xFFF0;
                    if (m_audHeaderCount==0) 
                    {
                      m_currentAudHeader = m_lastAudHeader;
                    } 
                  }  
                  else 
                  {  
                    m_currentAudHeader = (*(INT16 *)(ps+4) & 0xFFF0); //fscod, fscod2, bsid, bsmod fields
                    LogDebug("demux: E-AC3 resync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                    if (m_audHeaderCount>0) 
                    {
                      m_audHeaderCount--;
                    } 
                  }       
                }
                else // 'locked' state
                {
                  if (m_lastAudHeader != (*(INT16 *)(ps+4) & 0xFFF0))
                  {
                    m_lastAudHeader = *(INT16 *)(ps+4) & 0xFFF0;
                  }  
                  else if (m_currentAudHeader != (*(INT16 *)(ps+4) & 0xFFF0)) //fscod, fscod2, bsid, bsmod fields
                  {
                    m_audHeaderCount--; //invalid (or changing) header sequence
                    //LogDebug("demux: E-AC3 lkd bad sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                  }  
                  else //good header sequence
                  {
                    foundAudHeader = true;
                    if (!m_mpegPesParser->basicAudioInfo.isValid)
                    {
                      m_mpegPesParser->OnAudioPacket(ps, len, m_AudioStreamType, m_iAudioStream, true);
                      m_bSetAudioDiscontinuity=true;
                      
                      //Parse the channel count
                      byte chan = *(ps+4);
                      byte acmod = (chan & 0x0E)>>1;
                      
                    	static int channels[] = {2, 1, 2, 3, 3, 4, 4, 5};
                      byte parsedChannels = channels[acmod] + (chan & 0x01); //Add one channel for 'lfeon'
						          LogDebug("demux: E-AC3 header: sampleRate = %d, channels = %d, bitrate = %d, parsedChannels = %d, bytesRead = %d", m_mpegPesParser->basicAudioInfo.sampleRate, m_mpegPesParser->basicAudioInfo.channels, m_mpegPesParser->basicAudioInfo.bitrate, parsedChannels, m_audioBytesRead);
                    }
                    else
                    {
                      //Parse the channel count
                      byte chan = *(ps+4);
                      byte acmod = (chan & 0x0E)>>1;
                      
                    	static int channels[] = {2, 1, 2, 3, 3, 4, 4, 5};
                      byte parsedChannels = channels[acmod] + (chan & 0x01); //Add one channel for 'lfeon'
                      if (parsedChannels<7 && parsedChannels>0 && (parsedChannels != m_mpegPesParser->basicAudioInfo.channels))
                      {
    				            LogDebug("demux: E-AC3 channels = %d -> %d", m_mpegPesParser->basicAudioInfo.channels, parsedChannels);
                        CAutoLock plock (&m_mpegPesParser->m_sectionAudioPmt);
                        m_mpegPesParser->basicAudioInfo.channels=parsedChannels;
                        Cbuf->SetForcePMT();
                      }
                    }
                    if (m_audHeaderCount<32)
                    {
                      if (m_audHeaderCount>27 && m_audHeaderCount<31)
                      {
                        LogDebug("demux: E-AC3 good sync = %x %x %x %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), *(ps+4), *(ps+5), *(ps+6), length-len, m_audHeaderCount);
                      }
                      m_audHeaderCount += min(4, 32-m_audHeaderCount);
                    }
                  }
                }                  
              }
              
              copyLen++;
              *p++ = *ps++;   // memcpy could be not safe.
              len--;            
            }
            
            Cbuf->SetLength(copyLen);            
          }         
          else if (m_AudioStreamType == SERVICE_TYPE_AUDIO_MPEG1 ||
                   m_AudioStreamType == SERVICE_TYPE_AUDIO_MPEG2)
          {
            // LogDebug("MPA start PES = %d", len);
            while(len) 
            {
              //Find correct MPA frame header sync sequence by 'learning' the current header pattern
              if (((*(INT16 *)ps & 0xE0FF) == 0xE0FF) && len > 3) //Syncword bits==0xE0FF - first 11 bits set
              {     
                //LogDebug("demux: MPA all sync = %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), length-len, m_audHeaderCount);
                if ( ((*(ps+1) & 0x18) != 0x08) && //version check
                     ((*(ps+1) & 0x06) != 0x00) && //layer check
                     ((*(ps+2) & 0xF0) != 0xF0) && //bitrate check
                     ((*(ps+2) & 0x0C) != 0x0C) )  //sampling freq check 
                {                  
                  if (m_audHeaderCount<16)  // Learning/training state
                  {
                    if (m_currentAudHeader == (*(INT32 *)(ps+0) & 0x0FFCFFFF))
                    {
                      m_audHeaderCount+=4; 
                    }  
                    else if (m_lastAudHeader != (*(INT32 *)(ps+0) & 0x0FFCFFFF))
                    {
                      m_lastAudHeader = *(INT32 *)(ps+0) & 0x0FFCFFFF;
                      if (m_audHeaderCount==0) 
                      {
                        m_currentAudHeader = m_lastAudHeader;
                      } 
                    }  
                    else
                    {  
                      m_currentAudHeader = (*(INT32 *)(ps+0) & 0x0FFCFFFF);
                      LogDebug("demux: MPA resync = %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), length-len, m_audHeaderCount);
                      if (m_audHeaderCount>0) 
                      {
                        m_audHeaderCount--;
                      } 
                    }       
                  }
                  else // 'locked' state
                  {
                    if (m_lastAudHeader != (*(INT32 *)(ps+0) & 0x0FFCFFFF))
                    {
                      m_lastAudHeader = *(INT32 *)(ps+0) & 0x0FFCFFFF;
                    }  
                    else if (m_currentAudHeader != (*(INT32 *)(ps+0) & 0x0FFCFFFF))
                    {
                      //LogDebug("demux: MPA lkd bad sync = %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), length-len, m_audHeaderCount);
                      m_audHeaderCount--; //invalid (or changing) header sequence
                    }  
                    else //good header sequence
                    {
                      foundAudHeader = true;
                      if (!m_mpegPesParser->basicAudioInfo.isValid)
                      {
                        m_mpegPesParser->OnAudioPacket(ps, len, m_AudioStreamType, m_iAudioStream, true);
                        m_bSetAudioDiscontinuity=true;
                        
                        //Parse the channel count
                        byte parsedChannels = ((*(ps+3) & 0xC0) == 0xC0) ? 1 : 2;
  						          LogDebug("demux: MPA header: sampleRate = %d, channels = %d, bitrate = %d, parsedChannels = %d, bytesRead = %d", m_mpegPesParser->basicAudioInfo.sampleRate, m_mpegPesParser->basicAudioInfo.channels, m_mpegPesParser->basicAudioInfo.bitrate, parsedChannels, m_audioBytesRead);
                      }
                      else
                      {
                        //Parse the channel count
                        byte parsedChannels = ((*(ps+3) & 0xC0) == 0xC0) ? 1 : 2;
                        if (parsedChannels != m_mpegPesParser->basicAudioInfo.channels)
                        {
      				            LogDebug("demux: MPA channels = %d -> %d", m_mpegPesParser->basicAudioInfo.channels, parsedChannels);
                          CAutoLock plock (&m_mpegPesParser->m_sectionAudioPmt);
                          m_mpegPesParser->basicAudioInfo.channels=parsedChannels;
                          Cbuf->SetForcePMT();
                        }
                      }
                      if (m_audHeaderCount<32)
                      {
                        if (m_audHeaderCount>27 && m_audHeaderCount<31)
                        {
                          LogDebug("demux: MPA good sync = %x %x %x %x, byteCount = %d, headerCount = %d", *ps, *(ps+1), *(ps+2), *(ps+3), length-len, m_audHeaderCount);
                        }
                        m_audHeaderCount += min(4, 32-m_audHeaderCount);
                      }
                    }
                  }  
                }                
              }
              
              copyLen++;
              *p++ = *ps++;   // memcpy could be not safe.
              len--;            
            }
            
            Cbuf->SetLength(copyLen);            
          }         
          else //other audio types
          {
            if (!m_mpegPesParser->basicAudioInfo.isValid)
            {
              m_mpegPesParser->OnAudioPacket(0, 0, m_AudioStreamType, m_iAudioStream, true); //Generate default info
              m_bSetAudioDiscontinuity=true;
            }
            while(len--) 
            {
              copyLen++;
              *p++ = *ps++;   // memcpy could be not safe.
            }
            
            Cbuf->SetLength(copyLen);
          }         
        }
        else
        {
          LogDebug(" No data");
          m_AudioValidPES=false; //stop further processing
        }
      }

      { //Scoped for CAutoLock
        CAutoLock lock (&m_sectionAudio);
        
        if (m_AudioValidPES && m_mpegPesParser->basicAudioInfo.isValid)
        {
          CheckMediaChange(header.Pid, false);   
        }

        if ((m_AudioValidPES && m_filter.GetAudioPin()->IsConnected() && m_mpegPesParser->basicAudioInfo.isValid) && 
            (!m_filter.GetVideoPin()->IsConnected() || 
             (m_filter.GetVideoPin()->IsConnected() && m_bFrame0Found))) //Prevent video and audio getting too far out of step at start of play
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
              CBuffer* AudioBuffer=*it;
              delete AudioBuffer;
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
          // Clear PES temporary queue.
          if (m_t_vecAudioBuffers.size()>0)
          {
            ivecBuffers it = m_t_vecAudioBuffers.begin();
            for ( ; it != m_t_vecAudioBuffers.end() ; it++ )
            {
              CBuffer* AudioBuffer=*it;
              delete AudioBuffer;
            }
            m_t_vecAudioBuffers.clear();
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
    if (header.PayloadUnitStart)
    {
      //skip over pes header data (already written into buffer)
      int headerLen=9+tsPacket[pos+8] ;
      pos += headerLen;
    }
    
    //packet contains rest of a pes packet
    //does the entire data in this tspacket fit in the current buffer ?    
    if (m_pCurrentAudioBuffer->Length()+(188-pos)>=MAX_BUFFER_SIZE)
    {
      //Discard this new/current PES packet due to overflow
      m_AudioValidPES=false;  
      m_bSetAudioDiscontinuity=true;
      m_pCurrentAudioBuffer->SetLength(0);
      LogDebug("PES audio buffer overflow error");
      return;
    }

    //copy the data into the current buffer
    if (pos>0 && pos < 188)
    {
      m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos);
    }
  }
  
  //LogDebug("FillAudio() end");
}


/// This method will check if the tspacket is an video packet
void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket, int bufferOffset, int bufferLength)
{                  
  CAutoLock lock (&m_sectionFlushVideo);
  
  if (m_pids.videoPids.size() == 0 || m_pids.videoPids[0].Pid==0) return;
  if (header.Pid!=m_pids.videoPids[0].Pid) return;

  if (header.AdaptionFieldOnly()) return;

  if (!CheckContinuity(m_VideoPrevCC, header))
  {
    LogDebug("Video Continuity error... %x ( prev %x ), bufferOffset=%d, bufferLength=%d", header.ContinuityCounter, m_VideoPrevCC, bufferOffset, bufferLength);
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
  else if (m_pids.videoPids[0].VideoServiceType == SERVICE_TYPE_VIDEO_H264)
  {
    FillVideoH264(header, tsPacket);
  }
  else if (m_pids.videoPids[0].VideoServiceType == SERVICE_TYPE_VIDEO_HEVC)
  {
    //LogDebug("HEVC ts packet found, VideoServiceType = %x", m_pids.videoPids[0].VideoServiceType);
    FillVideoHEVC(header, tsPacket);
  }
}

void CDeMultiplexer::FillVideoHEVC(CTsHeader& header, byte* tsPacket)
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
    m_minVideoDTSdiff = DBL_MAX;
    m_vidPTScount = 0;
    m_vidDTScount = 0;
    m_bVideoPTSroff = false;
    m_bLogFPSfromDTSPTS = false;
    m_bUsingGOPtimestamp = false;
    m_mVideoValidPES = false; 
    m_VideoValidPES = false;
    m_WaitHeaderPES = -1;
    m_curFramePeriod = 0.0;
    LOG_SAMPLES_HEVC("DeMultiplexer::FillVideoHEVC New m_p");
  }

  if (header.PayloadUnitStart)
  {
    m_WaitHeaderPES = m_p->GetCount();
    m_mVideoValidPES = m_VideoValidPES;
    LOG_SAMPLES_HEVC("DeMultiplexer::FillVideoHEVC PayLoad Unit Start");
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
      LogDebug("DeMux: HEVC PES size out-of-bounds");
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
      if (m_hadPESfail < 256)
      {
        m_hadPESfail++;
      }
      //LogDebug("PES HEVC 0-0-1 fail");
      LogDebug("PES HEVC 0-0-1 fail, PES hdr = %x-%x-%x-%x-%x-%x-%x-%x, TS hdr = %x-%x-%x-%x-%x-%x-%x-%x-%x-%x", 
                                                                          start[0], start[1], start[2], start[3], start[4], start[5], start[6], start[7],
                                                                          tsPacket[0], tsPacket[1], tsPacket[2], tsPacket[3], tsPacket[4],
                                                                          tsPacket[5], tsPacket[6], tsPacket[7], tsPacket[8], tsPacket[9]);
      //header.LogHeader();
      m_VideoValidPES=false;
      m_mVideoValidPES = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_p->rtPrevStart = Packet::INVALID_TIME; 
      m_WaitHeaderPES = -1;
      m_bSetVideoDiscontinuity=true;
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      DelegatedFlush(false, false);
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
          double diff = 0.0;
          if (pts.IsValid)
          {
            if (!m_lastVideoPTS.IsValid)
              m_lastVideoPTS=pts;
            if (m_lastVideoPTS>pts)
              diff=m_lastVideoPTS.ToClock()-pts.ToClock();
            else
              diff=pts.ToClock()-m_lastVideoPTS.ToClock();
              
            if (diff > 0.005)
              m_vidPTScount++;
          }

          double diffDTS = 0.0;
          if (dts.IsValid)
          {
            if (!m_lastVideoDTS.IsValid)
              m_lastVideoDTS=dts;
            if (m_lastVideoDTS>dts)
              diffDTS=m_lastVideoDTS.ToClock()-dts.ToClock();
            else
              diffDTS=dts.ToClock()-m_lastVideoDTS.ToClock();
   
            if (diffDTS > 0.005)
              m_vidDTScount++;    
          }        

          if (diff > m_dVidPTSJumpLimit)
          {
            //Large PTS jump - flush the world...
            LogDebug("DeMultiplexer::FillVideoHEVC pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_lastAudioPTS.IsValid=false;
            m_lastVideoPTS.IsValid=false;
            m_lastVideoDTS.IsValid=false;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            DelegatedFlush(false, false);
          }
          else
          {
            if (pts.IsValid)
            {
              m_lastVideoPTS=pts;
            }             
            if ((diff < m_minVideoPTSdiff) && (diff > 0.005))
            {
              m_minVideoPTSdiff = diff;
            }
            
            if (dts.IsValid)
            {
              m_lastVideoDTS=dts;
            }              
            if ((diffDTS < m_minVideoDTSdiff) && (diffDTS > 0.005))
            {
              m_minVideoDTSdiff = diffDTS;
            }

            if ((diff < 0.002) && (pts.IsValid) && !m_fHasAccessUnitDelimiters)
            {
              LOG_SAMPLES_HEVC("DeMultiplexer::FillVideoHEVC - PTS is same, diff %f, pts %f ", (float)diff, (float)pts.ToClock());
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
            LOG_SAMPLES_HEVC("DeMultiplexer::FillVideoHEVC pts: %f, dts: %f, diff %f, minDiff %f, rtStart : %d ", (float)pts.ToClock(), (float)dts.ToClock(), (float) diff, (float)m_minVideoPTSdiff, pts.PcrReferenceBase);
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
    bool fourByte;

    MOVE_TO_HEVC_START_CODE(start, end, fourByte);

 	
    while(start <= end-4)
    {
      BYTE* next = start+1;
      if (next < m_p->GetData() + m_lastStart)
      {
        next = m_p->GetData() + m_lastStart;
      }

      MOVE_TO_HEVC_START_CODE(next, end, fourByte);

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
          
      size -= (fourByte ? 4 : 3); //Adjust to allow for start code
      
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
        LogDebug("DeMux: HEVC NALU size out-of-bounds %d", size);
        return;
      }
              
      
      DWORD dwNalStart = 0x01000000;  //NAL start code 

      //LogDebug("DeMux: NALU size %d", size);

      p2->SetCount (size+sizeof(dwNalStart));
      
      memcpy (p2->GetData(), &dwNalStart, sizeof(dwNalStart)); //Insert NAL start code
      memcpy (p2->GetData()+sizeof(dwNalStart), (start+3), size);

      //Get the NAL ID
      char nalIDp2 = ((*(p2->GetData()+4) & 0xfe) >> 1); //Note the 'forbidden_zero_bit' is included i.e. it must be zero for a valid NAL ID
      
      LOG_SAMPLES_HEVC("HEVC: Input p2 NALU Type: %d (%d), m_p->rtStart: %d, m_p->rtPrevStart: %d", nalIDp2, p2->GetCount(), (int)m_p->rtStart, (int)m_p->rtPrevStart);
      
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
      if(nalIDp2 == HEVC_NAL_AUD) //Check for AUD
      {
        m_fHasAccessUnitDelimiters = true;
      }
        
      if((nalIDp2 == HEVC_NAL_AUD) || (!m_fHasAccessUnitDelimiters && m_isNewNALUTimestamp)) //Check for AUD
      {
        m_isNewNALUTimestamp = false;
        if ((m_pl.GetCount()>0) && m_mVideoValidPES)
        {
          //Transfer NALU packets to output sample
          bool Gop = false;
          bool foundIRAP = false;
          
          //Copy available NALUs into new packet 'p' (for the next video buffer)
          CAutoPtr<Packet> p(new Packet());
          p->rtStart = Packet::INVALID_TIME;

          LOG_OUTSAMPLES_HEVC("HEVC: Transfer to p, p->len = %d, m_pl.len = %d, hasAUD = %d", p->GetCount(), m_pl.GetCount(), m_fHasAccessUnitDelimiters);

          if (!m_fHasAccessUnitDelimiters)
          {
            //Add fake AUD....
            DWORD dwNalStart = 0x01000000;  //Insert NAL start code 
            DWORD dwFakeAUD  = 0x00500146;  //AUD with IPB pic_type        
              
            p->SetCount (sizeof(dwNalStart)+sizeof(dwFakeAUD));
            
            memcpy (p->GetData(), &dwNalStart, sizeof(dwNalStart));  //Insert NAL start code
            memcpy (p->GetData()+sizeof(dwNalStart), &dwFakeAUD, sizeof(dwFakeAUD));            
            //LogDebug("HEVC: Insert Fake AUD: %x %x %x %x %x %x %x %x",  p->GetAt(0), p->GetAt(1), p->GetAt(2), p->GetAt(3), p->GetAt(4), p->GetAt(5), p->GetAt(6), p->GetAt(7));
          }
          
          while(m_pl.GetCount()>0)
          {
            CAutoPtr<Packet> p4(new Packet());
            p4 = m_pl.RemoveHead();
            
            //if (!iFrameScanner.SeenEnough())
            //  iFrameScanner.ProcessNALU(p2);
            LOG_OUTSAMPLES_HEVC("HEVC: Output p4 NALU Type: %d (%d), rtStart: %d", (p4->GetAt(4) & 0xfe)>>1, p4->GetCount(), (int)p->rtStart);
            
            char nalIDp4 = ((p4->GetAt(4) & 0xfe) >> 1);
            //LogDebug("HEVC: All NAL, type = %d", nalIDp4);
            if ((nalIDp4 == HEVC_NAL_VPS) || (nalIDp4 == HEVC_NAL_SPS) || (nalIDp4 == HEVC_NAL_PPS)) //Process VPS, SPS & PPS data
            {
              //LogDebug("HEVC: VPS/SPS/PPS NAL, type = %d", nalIDp4);
              Gop = m_mpegPesParser->OnTsPacket(p4->GetData(), p4->GetCount(), VIDEO_STREAM_TYPE_HEVC, m_mpegParserReset);
              m_mpegParserReset = false;
            }
            
            //Check for random-access entry points in the stream
            if ((nalIDp4 >= HEVC_NAL_BLA_W_LP) && (nalIDp4 <= HEVC_NAL_CRA_NUT))
            {
              foundIRAP = true;
              
              if (!m_bFrame0Found)  //First random access point after stream start or seek - add SPS/PPS/VPS
              {
                LogDebug("HEVC: Random access point, insert SPS(%I64d), PPS(%I64d), VPS(%I64d)", m_mpegPesParser->basicVideoInfo.spslen, m_mpegPesParser->basicVideoInfo.ppslen, m_mpegPesParser->basicVideoInfo.vpslen);
                if (m_mpegPesParser->basicVideoInfo.spslen > 0)
                {
                  //Insert SPS NAL
                  size_t currCount = p->GetCount();
                  p->SetCount (currCount + (size_t)m_mpegPesParser->basicVideoInfo.spslen);
                  memcpy (p->GetData() + currCount, m_mpegPesParser->basicVideoInfo.sps, (size_t)m_mpegPesParser->basicVideoInfo.spslen);
                }
                if (m_mpegPesParser->basicVideoInfo.ppslen > 0)
                {
                  //Insert PPS NAL
                  size_t currCount = p->GetCount();
                  p->SetCount (currCount + (size_t)m_mpegPesParser->basicVideoInfo.ppslen);
                  memcpy (p->GetData() + currCount, m_mpegPesParser->basicVideoInfo.pps, (size_t)m_mpegPesParser->basicVideoInfo.ppslen);
                }
                if (m_mpegPesParser->basicVideoInfo.vpslen > 0)
                {
                  //Insert VPS NAL
                  size_t currCount = p->GetCount();
                  p->SetCount (currCount + (size_t)m_mpegPesParser->basicVideoInfo.vpslen);
                  memcpy (p->GetData() + currCount, m_mpegPesParser->basicVideoInfo.vps, (size_t)m_mpegPesParser->basicVideoInfo.vpslen);
                }
              }
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
            // m_mpegParserReset = true; //Reset next time around (so that it always searches for a full 'Gop' header)
            if (!m_bFirstGopParsed)
            {
              m_bFirstGopParsed = true;
              LogDebug("DeMultiplexer: HEVC: First Gop after new PAT, %dx%d @ %d:%d, %.3fHz %s",m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps, m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
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
            //Bitrate info calculation for MP player
            m_byteRead = m_byteRead + p->GetCount();
          	m_sampleTime = (float)timestamp.ToClock();
          	float elapsedTime = m_sampleTime - m_sampleTimePrev;
          	          
          	if (elapsedTime > 5.0f)
          	{
              m_bitRate = ((float)m_byteRead*8.0f)/elapsedTime;
          	  m_filter.OnBitRateChanged((int)m_bitRate);
          	  m_sampleTimePrev = m_sampleTime;
          	  m_byteRead = 0;
          	  LOG_VID_BITRATE("HEVC: Rolling bitrate = %f", m_bitRate/1000000.0f);
            }
          	else if (elapsedTime < -0.5) 
          	{
          	  m_sampleTimePrev = m_sampleTime;
            	m_byteRead = 0;
          	}

            CRefTime Ref;
            CBuffer *pCurrentVideoBuffer = new CBuffer(p->GetCount());
            pCurrentVideoBuffer->Add(p->GetData(), p->GetCount());
            pCurrentVideoBuffer->SetPts(timestamp);   
            pCurrentVideoBuffer->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
            pCurrentVideoBuffer->MediaTime(Ref);
            LOG_OUTSAMPLES_HEVC("...> HEVC: Store NALU type (length) = %d (%d), p->rtStart = %d, timestamp %f, IRAP = %d", (*(p->GetData()+4) & 0x1F), p->GetCount(), (int)p->rtStart, timestamp.ToClock(), foundIRAP) ;
            // Must use p->rtStart as CPcr is UINT64 and INVALID_TIME is LONGLONG
            // Too risky to change CPcr implementation at this time 
            if(p->rtStart != Packet::INVALID_TIME)
            {
              if (foundIRAP && m_bFirstGopFound && !m_bSecondGopFound)
              {
                m_bSecondGopFound=true;
                LogDebug("  HEVC: 2nd random access frame found %f ", Ref.Millisecs()/1000.0f);
              }
              if (Gop && !m_bFirstGopFound)
              {
                m_bFirstGopFound=true;
                LogDebug("  HEVC: SPS/PPS/VPS found %f ", Ref.Millisecs()/1000.0f);
                m_LastValidFrameCount=0;
              }
              if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
              if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
              if (m_bFirstGopFound && foundIRAP && !m_bFrame0Found)
              {
                LogDebug("  HEVC: First random access frame found. RefFVS = %f, Ref = %f, IRAP = %d ", m_FirstVideoSample.Millisecs()/1000.0f, Ref.Millisecs()/1000.0f, foundIRAP);
                m_ZeroVideoSample = Ref; //We start filling the main sample buffer at this point
                m_LastVideoSample = Ref;
                m_FirstVideoSample = Ref;
                m_bFrame0Found = true;
              }
              m_LastValidFrameCount++;
            }

            pCurrentVideoBuffer->SetFrameType(foundIRAP ? 'I':'?');
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
                  LogDebug("Demux : Video to render too late= %03.3f Sec, FileReadLatency: %d ms, flushing", Delta, m_fileReadLatency) ;
                  m_MinAudioDelta+=1.0;
                  m_MinVideoDelta+=1.0;                
                  //Flushing is delegated to CDeMultiplexer::ThreadProc()
                  DelegatedFlush(false, false);
                }
                else if (Delta < 0.2)
                {
                  LogDebug("Demux : Video to render too late= %03.3f Sec, FileReadLatency: %d ms", Delta, m_fileReadLatency) ;
                  _InterlockedIncrement(&m_AVDataLowCount);   
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
                if (m_bFrame0Found) //if (m_bFirstGopFound)
                {                  
                  // ownership is transfered to vector
                  m_vecVideoBuffers.push_back(pCurrentVideoBuffer);
                  // Parse the sample buffer for Closed Caption data (testing...)
                  //m_CcParserHEVC->parseAVC1sample(pCurrentVideoBuffer->Data(), pCurrentVideoBuffer->Length(), 4);
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
            CheckMediaChange(header.Pid, true);
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

      LOG_OUTSAMPLES_HEVC(".......> HEVC: Store NALU type (length) = %d (%d), p2->rtStart = %d, newtimestamp = %d", (*(p2->GetData()+4) & 0x7e)>>1, p2->GetCount(), (int)p2->rtStart, isNewTimestamp) ;
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
    m_minVideoDTSdiff = DBL_MAX;
    m_vidPTScount = 0;
    m_vidDTScount = 0;
    m_bVideoPTSroff = false;
    m_bLogFPSfromDTSPTS = false;
    m_bUsingGOPtimestamp = false;
    m_mVideoValidPES = false; 
    m_VideoValidPES = false;
    m_WaitHeaderPES = -1;
    m_curFramePeriod = 0.0;
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
      if (m_hadPESfail < 256)
      {
        m_hadPESfail++;
      }
      //LogDebug("PES H264 0-0-1 fail");
      LogDebug("PES H264 0-0-1 fail, PES hdr = %x-%x-%x-%x-%x-%x-%x-%x, TS hdr = %x-%x-%x-%x-%x-%x-%x-%x-%x-%x", 
                                                                          start[0], start[1], start[2], start[3], start[4], start[5], start[6], start[7],
                                                                          tsPacket[0], tsPacket[1], tsPacket[2], tsPacket[3], tsPacket[4],
                                                                          tsPacket[5], tsPacket[6], tsPacket[7], tsPacket[8], tsPacket[9]);
      //header.LogHeader();
      m_VideoValidPES=false;
      m_mVideoValidPES = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_p->rtPrevStart = Packet::INVALID_TIME; 
      m_WaitHeaderPES = -1;
      m_bSetVideoDiscontinuity=true;
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      DelegatedFlush(false, false);
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
          double diff = 0.0;
          if (pts.IsValid)
          {
            if (!m_lastVideoPTS.IsValid)
              m_lastVideoPTS=pts;
            if (m_lastVideoPTS>pts)
              diff=m_lastVideoPTS.ToClock()-pts.ToClock();
            else
              diff=pts.ToClock()-m_lastVideoPTS.ToClock();
              
            if (diff > 0.005)
              m_vidPTScount++;
          }

          double diffDTS = 0.0;
          if (dts.IsValid)
          {
            if (!m_lastVideoDTS.IsValid)
              m_lastVideoDTS=dts;
            if (m_lastVideoDTS>dts)
              diffDTS=m_lastVideoDTS.ToClock()-dts.ToClock();
            else
              diffDTS=dts.ToClock()-m_lastVideoDTS.ToClock();
   
            if (diffDTS > 0.005)
              m_vidDTScount++;    
          }        

          if (diff > m_dVidPTSJumpLimit)
          {
            //Large PTS jump - flush the world...
            LogDebug("DeMultiplexer::FillVideoH264 pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_lastAudioPTS.IsValid=false;
            m_lastVideoPTS.IsValid=false;
            m_lastVideoDTS.IsValid=false;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            DelegatedFlush(false, false);
          }
          else
          {
            if (pts.IsValid)
            {
              m_lastVideoPTS=pts;
            }             
            if ((diff < m_minVideoPTSdiff) && (diff > 0.005))
            {
              m_minVideoPTSdiff = diff;
            }
            
            if (dts.IsValid)
            {
              m_lastVideoDTS=dts;
            }              
            if ((diffDTS < m_minVideoDTSdiff) && (diffDTS > 0.005))
            {
              m_minVideoDTSdiff = diffDTS;
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
    bool fourByte;

    MOVE_TO_H264_START_CODE(start, end, fourByte);

 	
    while(start <= end-4)
    {
      BYTE* next = start+1;
      if (next < m_p->GetData() + m_lastStart)
      {
        next = m_p->GetData() + m_lastStart;
      }

      MOVE_TO_H264_START_CODE(next, end, fourByte);

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
          
      size -= (fourByte ? 4 : 3); //Adjust to allow for start code
      
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
              
      DWORD dwNalLength = _byteswap_ulong(size);  //dwNalLength is big-endian format

      //LogDebug("DeMux: NALU size %d", size);

      p2->SetCount (size+sizeof(dwNalLength));
      
      memcpy (p2->GetData(), &dwNalLength, sizeof(dwNalLength));
      memcpy (p2->GetData()+sizeof(dwNalLength), (start+3), size);
      
      //Get the NAL ID
      char nalIDp2 = (*(p2->GetData()+4)&0x9f); //Note the 'forbidden_zero_bit' is included i.e. it must be zero for a valid NAL ID

      LOG_SAMPLES("Input p2 NALU Type: %d (%d), m_p->rtStart: %d, m_p->rtPrevStart: %d", nalIDp2, p2->GetCount(), (int)m_p->rtStart, (int)m_p->rtPrevStart);
      
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
      
      if(nalIDp2 == H264_NAL_AUD) 
      {
        m_fHasAccessUnitDelimiters = true;
      }
        
      if((nalIDp2 == H264_NAL_AUD) || (!m_fHasAccessUnitDelimiters && m_isNewNALUTimestamp))
      {
        m_isNewNALUTimestamp = false;
        if ((m_pl.GetCount()>0) && m_mVideoValidPES)
        {
          //Transfer NALU packets to output sample
          bool Gop = false;
          bool foundIRAP = false;
          
          //Copy available NALUs into new packet 'p' (for the next video buffer)
          CAutoPtr<Packet> p(new Packet());
          p->rtStart = Packet::INVALID_TIME;

          if (!m_fHasAccessUnitDelimiters)
          {
            //Add fake AUD....
            DWORD size = 2;
            WORD data9 = 0xF009;            
            DWORD dwNalLength = _byteswap_ulong(size);  //dwNalLength is big-endian format
              
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
            
            char nalIDp4     = p4->GetAt(4) & 0x9f; //Note the 'forbidden_zero_bit' is included i.e. it must be zero for a valid NAL ID
            char nalRefIdcp4 = (p4->GetAt(4) & 0x60) >> 5;
            //LogDebug("H264: All NAL, type = %d, nalRefIDC = %d, nextByte = 0x%x", nalIDp4, nalRefIdcp4, p4->GetAt(5));
            
            if (((nalIDp4 == H264_NAL_SPS) || (nalIDp4 == H264_NAL_PPS)) && (nalRefIdcp4 != 0)) //Process SPS & PPS data
            {
              Gop = m_mpegPesParser->OnTsPacket(p4->GetData(), p4->GetCount(), VIDEO_STREAM_TYPE_H264, m_mpegParserReset);
              m_mpegParserReset = false;
              
              if (Gop && !m_bFirstGopParsed)
              {
                m_bFirstGopParsed = true;
                LogDebug("DeMultiplexer: H264: First Gop after new PAT, %dx%d @ %d:%d, %.3fHz %s",m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps, m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
              }
            }

            //if(nalIDp4 == H264_NAL_SEI && p4->GetAt(5) == 0x06) //recovery_point SEI
            //{
            //  LogDebug("H264: recovery_point SEI");
            //}
            //
            //if(nalIDp4 == H264_NAL_SEI && p4->GetAt(5) == 0x04) //closed-caption SEI
            //{
            //  LogDebug("demux: p2 H264 SEI CC");
            //}

            //Find random-access entry points in the stream - check nalID, nal_ref_idc, first_mb_in_slice and slice-type values
            if (((nalIDp4 == H264_NAL_IDR) || (nalIDp4 == H264_NAL_SLICE)) && (nalRefIdcp4 != 0) && 
              ((p4->GetAt(5) & 0xF0) == 0xB0 ||  // first_mb_in_slice=0 and  I-slice type 2
               (p4->GetAt(5) & 0xFC) == 0x94 ||  // first_mb_in_slice=0 and SI-slice type 4
               (p4->GetAt(5) & 0xFF) == 0x88 ||  // first_mb_in_slice=0 and  I-slice type 7
               (p4->GetAt(5) & 0xFF) == 0x8A )   // first_mb_in_slice=0 and SI-slice type 9
             )
            {
              foundIRAP = true;
              //LogDebug("H264: Random access point, nalID = %d, nalRefIdc = %d, slice_header = 0x%x", nalIDp4, nalRefIdcp4, p4->GetAt(5));
              
              if (!m_bFrame0Found && m_bFirstGopFound)  //First random access point after stream start or seek - add SPS/PPS
              {
                LogDebug("H264: Random access point, nalID = %d, nalRefIdc = %d, slice_header = 0x%x, SPS(%I64d), PPS(%I64d)", nalIDp4, nalRefIdcp4, p4->GetAt(5), m_mpegPesParser->basicVideoInfo.spslen, m_mpegPesParser->basicVideoInfo.ppslen);
                if (m_mpegPesParser->basicVideoInfo.spslen > 0)
                {
                  //Insert SPS NAL
                  size_t currCount = p->GetCount();                  
                  DWORD dwNalLength = _byteswap_ulong((unsigned long)m_mpegPesParser->basicVideoInfo.spslen);  //dwNalLength is big-endian format
                  p->SetCount (currCount + (size_t)m_mpegPesParser->basicVideoInfo.spslen + sizeof(dwNalLength));
                  memcpy (p->GetData() + currCount, &dwNalLength, sizeof(dwNalLength)); //Insert NAL length                  
                  memcpy (p->GetData() + currCount + sizeof(dwNalLength), m_mpegPesParser->basicVideoInfo.sps, (size_t)m_mpegPesParser->basicVideoInfo.spslen);
                }
                if (m_mpegPesParser->basicVideoInfo.ppslen > 0)
                {
                  //Insert PPS NAL
                  size_t currCount = p->GetCount();                  
                  DWORD dwNalLength = _byteswap_ulong((unsigned long)m_mpegPesParser->basicVideoInfo.ppslen);  //dwNalLength is big-endian format
                  p->SetCount (currCount + (size_t)m_mpegPesParser->basicVideoInfo.ppslen + sizeof(dwNalLength));
                  memcpy (p->GetData() + currCount, &dwNalLength, sizeof(dwNalLength)); //Insert NAL length                  
                  memcpy (p->GetData() + currCount + sizeof(dwNalLength), m_mpegPesParser->basicVideoInfo.pps, (size_t)m_mpegPesParser->basicVideoInfo.ppslen);
                }
              }              
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

          CPcr timestamp;
          if(p->rtStart != Packet::INVALID_TIME )
          {
            timestamp.PcrReferenceBase = p->rtStart;
            timestamp.IsValid=true;
          }
          //LogDebug("NALU Type: %d (%d) %d, p->timestamp %f, p->rtStart %d",  p->GetAt(4)&0x1f, p->GetCount(), timestamp.ToClock(), (int)p->rtStart);


          if ((Gop || m_bFirstGopFound) && m_filter.GetVideoPin()->IsConnected())
          {
            //Bitrate info calculation for MP player
            m_byteRead = m_byteRead + p->GetCount();
          	m_sampleTime = (float)timestamp.ToClock();
          	float elapsedTime = m_sampleTime - m_sampleTimePrev;
          
          	if (elapsedTime > 5.0f)
          	{
              m_bitRate = ((float)m_byteRead*8.0f)/elapsedTime;
          	  m_filter.OnBitRateChanged((int)m_bitRate);
          	  m_sampleTimePrev = m_sampleTime;
          	  m_byteRead = 0;
          	  LOG_VID_BITRATE("H264: Rolling bitrate = %f", m_bitRate/1000000.0f);
            }
          	else if (elapsedTime < -0.5) 
          	{
          	  m_sampleTimePrev = m_sampleTime;
            	m_byteRead = 0;
          	}

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
              if (foundIRAP && m_bFirstGopFound && !m_bSecondGopFound)
              {
                m_bSecondGopFound=true;
                LogDebug("  H264: 2nd random access frame found %f", Ref.Millisecs()/1000.0f);
              }
              if (Gop && !m_bFirstGopFound)
              {
                m_bFirstGopFound=true;
                LogDebug("  H264: SPS/PPS found %f", Ref.Millisecs()/1000.0f);
                m_LastValidFrameCount=0;
              }
              if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
              if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
              if (m_bFirstGopFound && foundIRAP && !m_bFrame0Found)  /*&& m_LastValidFrameCount>=5*/ /*(frame_count==0)*/
              {
                LogDebug("  H264: First random access frame found. RefFVS = %f, Ref = %f", m_FirstVideoSample.Millisecs()/1000.0f, Ref.Millisecs()/1000.0f);
                m_ZeroVideoSample = Ref; //We start filling the main sample buffer at this point
                m_LastVideoSample = Ref;
                m_FirstVideoSample = Ref;
                m_bFrame0Found = true;
              }
              m_LastValidFrameCount++;
            }

            pCurrentVideoBuffer->SetFrameType(foundIRAP ? 'I':'?');
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
                  LogDebug("Demux : Video to render too late= %03.3f Sec, FileReadLatency: %d ms, flushing", Delta, m_fileReadLatency) ;
                  m_MinAudioDelta+=1.0;
                  m_MinVideoDelta+=1.0;                
                  //Flushing is delegated to CDeMultiplexer::ThreadProc()
                  DelegatedFlush(false, false);
                }
                else if (Delta < 0.2)
                {
                  LogDebug("Demux : Video to render too late= %03.3f Sec, FileReadLatency: %d ms", Delta, m_fileReadLatency) ;
                  _InterlockedIncrement(&m_AVDataLowCount);   
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
                if (m_bFrame0Found) // (m_bFirstGopFound)
                {                  
                  // ownership is transfered to vector
                  m_vecVideoBuffers.push_back(pCurrentVideoBuffer);
                  // Parse the sample buffer for Closed Caption data (testing...)
                  //m_CcParserH264->parseAVC1sample(pCurrentVideoBuffer->Data(), pCurrentVideoBuffer->Length(), 4);
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
            CheckMediaChange(header.Pid, true);
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
    m_minVideoPTSdiff = DBL_MAX;
    m_minVideoDTSdiff = DBL_MAX;
    m_vidPTScount = 0;
    m_vidDTScount = 0;
    m_bLogFPSfromDTSPTS = false;
    m_bUsingGOPtimestamp = false;
    m_LastValidFrameCount=-1;
    m_VideoPts.IsValid=false;
    m_CurrentVideoPts.IsValid=false;
    m_LastValidFramePts.IsValid=false; 
    m_mVideoValidPES = false; 
    m_VideoValidPES = false;
    m_WaitHeaderPES = -1;
    m_curFramePeriod = 0.0;
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
      if (m_hadPESfail < 256)
      {
        m_hadPESfail++;
      }
      //LogDebug("PES MPEG2 0-0-1 fail");
      LogDebug("PES MPEG2 0-0-1 fail, PES hdr = %x-%x-%x-%x-%x-%x-%x-%x, TS hdr = %x-%x-%x-%x-%x-%x-%x-%x-%x-%x", 
                                                                          start[0], start[1], start[2], start[3], start[4], start[5], start[6], start[7],
                                                                          tsPacket[0], tsPacket[1], tsPacket[2], tsPacket[3], tsPacket[4],
                                                                          tsPacket[5], tsPacket[6], tsPacket[7], tsPacket[8], tsPacket[9]);
      //header.LogHeader();
      m_VideoValidPES = false;
      m_mVideoValidPES = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_WaitHeaderPES = -1;
      m_bSetVideoDiscontinuity=true;
      //Flushing is delegated to CDeMultiplexer::ThreadProc()
      DelegatedFlush(false, false);
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
          double diff = 0.0;
          if (pts.IsValid)
          {
            if (!m_lastVideoPTS.IsValid)
              m_lastVideoPTS=pts;
            if (m_lastVideoPTS>pts)
              diff=m_lastVideoPTS.ToClock()-pts.ToClock();
            else
              diff=pts.ToClock()-m_lastVideoPTS.ToClock();
              
            if (diff > 0.005)
              m_vidPTScount++;
          }

          double diffDTS = 0.0;
          if (dts.IsValid)
          {
            if (!m_lastVideoDTS.IsValid)
              m_lastVideoDTS=dts;
            if (m_lastVideoDTS>dts)
              diffDTS=m_lastVideoDTS.ToClock()-dts.ToClock();
            else
              diffDTS=dts.ToClock()-m_lastVideoDTS.ToClock();
   
            if (diffDTS > 0.005)
              m_vidDTScount++;    
          }        

          if (diff > m_dVidPTSJumpLimit)
          {
            //Large PTS jump - flush the world...
            LogDebug("DeMultiplexer::FillVideoMPEG2 pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_lastAudioPTS.IsValid=false;
            m_lastVideoPTS.IsValid=false;
            m_lastVideoDTS.IsValid=false;
            //Flushing is delegated to CDeMultiplexer::ThreadProc()
            DelegatedFlush(false, false);
          }
          else
          {
//            LogDebug("DeMultiplexer::FillVideo pts : %f ", (float)pts.ToClock());
            if (pts.IsValid)
            {
              m_lastVideoPTS=pts;
            }
            if ((diff < m_minVideoPTSdiff) && (diff > 0.005))
            {
              m_minVideoPTSdiff = diff;
            }
            
            if (dts.IsValid)
            {
              m_lastVideoDTS=dts;
            }
            if ((diffDTS < m_minVideoDTSdiff) && (diffDTS > 0.005))
            {
              m_minVideoDTSdiff = diffDTS;
            }
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

            // LogDebug("frame len %d decoded PTS %f (framerate %f), %c(%d)", p->GetCount(), m_CurrentVideoPts.IsValid ? (float)m_CurrentVideoPts.ToClock() : 0.0f,(float)m_curFramePeriod,frame_type,frame_count);

            bool Gop = m_mpegPesParser->OnTsPacket(p->GetData(), p->GetCount(), VIDEO_STREAM_TYPE_MPEG2, m_mpegParserReset);
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
              
            if (!m_CurrentVideoPts.IsValid)
            {
              //PES packet with no PTS
              m_bUsingGOPtimestamp = true;
            }

            if ((Gop || m_bFirstGopFound) && m_filter.GetVideoPin()->IsConnected())
            {
              if (m_CurrentVideoPts.IsValid)
              {                                                     // Timestamp Ok.
                m_LastValidFrameCount=frame_count;
                m_LastValidFramePts=m_CurrentVideoPts;
              }
              else
              {                    
                if (m_LastValidFrameCount>=0)                       // No timestamp, but we've latest GOP timestamp.
                {
                  double d = m_LastValidFramePts.ToClock() + (frame_count-m_LastValidFrameCount) * m_curFramePeriod ;
                  m_CurrentVideoPts.FromClock(d);                   // Rebuild it from 1st frame in GOP timestamp.
                  m_CurrentVideoPts.IsValid=true;
                }
              }

              //Bitrate info calculation for MP player
              m_byteRead = m_byteRead + p->GetCount();
              m_sampleTime = (float)m_CurrentVideoPts.ToClock();
              float elapsedTime = m_sampleTime - m_sampleTimePrev;
            
              if (elapsedTime > 5.0f)
              {
                m_bitRate = ((float)m_byteRead*8.0f)/elapsedTime;
            	  m_filter.OnBitRateChanged((int)m_bitRate);
            	  m_sampleTimePrev = m_sampleTime;
            	  m_byteRead = 0;
          	    LOG_VID_BITRATE("MPEG2: Rolling bitrate = %f", m_bitRate/1000000.0f);
              }
              else if (elapsedTime < -0.5) 
              {
                m_sampleTimePrev = m_sampleTime;
                m_byteRead = 0;
              }

              CRefTime Ref;
              CBuffer *pCurrentVideoBuffer = new CBuffer(p->GetCount());
              pCurrentVideoBuffer->Add(p->GetData(), p->GetCount());
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
                if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
                if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
                if (m_bFirstGopFound && !m_bFrame0Found && (frame_count==0))
                {
                  LogDebug("  MPEG: First random access frame found. RefFVS = %f, Ref = %f", m_FirstVideoSample.Millisecs()/1000.0f, Ref.Millisecs()/1000.0f);
                  m_ZeroVideoSample = Ref; //We start filling the main sample buffer at this point
	                m_LastVideoSample = Ref;
	                m_FirstVideoSample = Ref;
                  m_bFrame0Found = true;
                }
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
                    LogDebug("Demux : Video to render too late= %03.3f Sec, FileReadLatency: %d ms, flushing", Delta, m_fileReadLatency) ;
                    m_MinAudioDelta+=1.0;
                    m_MinVideoDelta+=1.0;                
                    //Flushing is delegated to CDeMultiplexer::ThreadProc()
                    DelegatedFlush(false, false);
                  }
                  else if (Delta < 0.2)
                  {
                    LogDebug("Demux : Video to render too late= %03.3f Sec, FileReadLatency: %d ms", Delta, m_fileReadLatency) ;
                    _InterlockedIncrement(&m_AVDataLowCount);   
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
                  if (m_bFrame0Found) // (m_bFirstGopFound)
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
              CheckMediaChange(header.Pid, true);              
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
          m_curFramePeriod = frame_rate[*(p2->GetData()+7) & 0x0F] ;  // Extract frame period in seconds.
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
      //LogDebug("Calling SetSubtitlePid");
      pDVBSubtitleFilter->SetSubtitlePid(m_subtitleStreams[m_iSubtitleStream].pid);
      LogDebug(" done - DVBSub - SetSubtitlePid");
      //LogDebug("Calling SetFirstPcr");
      pDVBSubtitleFilter->SetFirstPcr(m_duration.FirstStartPcr().PcrReferenceBase);
      LogDebug(" done - DVBSub - SetFirstPcr");
      m_currentSubtitlePid = m_subtitleStreams[m_iSubtitleStream].pid;
    }
  }

  if (m_currentSubtitlePid==0 || m_currentSubtitlePid != header.Pid) return;
  if ( header.AdaptionFieldOnly() ) return;

  //We have a packet with valid payload
  CAutoLock lock (&m_sectionSubtitle);
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
  if (m_filter.GetAudioPin()->IsConnected() && (m_vecAudioBuffers.size() > AUD_BUF_SIZE_PREFETCH_LIM))
  {
    return false;
  }
  if (m_filter.GetVideoPin()->IsConnected() && (m_vecVideoBuffers.size() > VID_BUF_SIZE_PREFETCH_LIM))
  {
    return false;
  }

  //Start-of-play situation
  if (isForced)
  {
    return true;
  }

  //Normal play
  if (isNormal)
  {
    if (m_filter.GetAudioPin()->IsConnected() && ((int)m_vecAudioBuffers.size() < m_initialAudioSamples))
    {
      return true;
    }
    if (m_filter.GetVideoPin()->IsConnected() && ((int)m_vecVideoBuffers.size() < m_initialVideoSamples))
    {
      return true;
    }
  }

  return false;
}

int CDeMultiplexer::GetRTSPBufferSize()
{
  if (m_reader)
  {
    if (m_reader->IsBuffer()) //RTSP mode
    {
      return (m_reader->HasData());
    }
  }
  return -1;
}

void CDeMultiplexer::GetBufferCounts(int* ACnt, int* VCnt)
{
  *ACnt = m_vecAudioBuffers.size();
  *VCnt = m_vecVideoBuffers.size();
}

int CDeMultiplexer::GetVideoBufferPts(CRefTime& First, CRefTime& Last, CRefTime& Zero)
{
  First = m_FirstVideoSample;
  Last = m_LastVideoSample;
  Zero = m_ZeroVideoSample;
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
  CPidTable pids=info.PidTable;

  // No audio streams or PCR/video with PCR
  if (pids.audioPids.size()<1 || (pids.PcrPid<1 && m_pids.videoPids.size()<1))
  { 
    return;
  }
  
  //pids.LogPIDs();
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
    DelegatedFlush(true, true);
  }
  else
  {
    if ((m_pids.PmtPid>1 && (m_pids.PmtPid != pids.PmtPid)) || m_pids == pids)
    {
      // This is not the correct PMT (if there are multiple PMTs), or the current PMT content is unchanged 
      return; 
    }
  }

  //remember the old audio & video formats
  int oldVideoServiceType(-1);
  if(m_pids.videoPids.size()>0)
  {
    oldVideoServiceType=m_pids.videoPids[0].VideoServiceType;
  }

  m_pids=pids;
  LogDebug("OnNewChannel: New channel found (PAT/PMT/SDT changed)");
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

  //update PCR pid..
  if (m_pids.PcrPid>0x1)
  {
    m_duration.SetVideoPid(m_pids.PcrPid);
  }
  else if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>0x1)
  {
    m_duration.SetVideoPid(m_pids.videoPids[0].Pid);
  }

  if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>0x1)
  {
    //Adjust PTS jump detection limits for still image streams
    if (m_pids.videoPids[0].DescriptorData & 0x01) //Still image flag
    {
      m_dVidPTSJumpLimit = 70.0;
    }
    else
    {
      m_dVidPTSJumpLimit = 2.0;
    }
  }

  //update audio streams etc..
  m_audioStreams.clear();

  for(int i(0) ; i < (int)m_pids.audioPids.size() ; i++)
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
  
  for(int i(0) ; i < (int)m_pids.subtitlePids.size() ; i++)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.subtitlePids[i].Pid;
    subtitle.language[0]=m_pids.subtitlePids[i].Lang[0];
    subtitle.language[1]=m_pids.subtitlePids[i].Lang[1];
    subtitle.language[2]=m_pids.subtitlePids[i].Lang[2];
    subtitle.language[3]=0;
    m_subtitleStreams.push_back(subtitle);
  }

  //bool changed=false;
  m_videoChanged=false;
  m_audioChanged=false;

  //Is the video pin connected?
  if (m_filter.GetVideoPin()->IsConnected()) 
  {
    if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>0x1)
    {
     //force a check in the mpeg parser
      //changed=true;
      LogDebug("OnNewChannel: Video media type changed. SetMediaChanging(true)...");
      m_mpegParserReset = true;
      m_bFirstGopParsed = false;
      m_videoChanged=true;
      m_mpegPesParser->VideoReset();  
      SetMediaChanging(true);
    }
  }

  //Lock the audio stream variables
  CAutoLock lock (&m_sectionSetAudioStream);
    
  m_iAudioStream = 0;

  //get the new audio format
  int newAudioStreamType=SERVICE_TYPE_AUDIO_UNKNOWN;
  if (m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }
 
  //did the audio format change?
  if (m_AudioStreamType != newAudioStreamType )
  {
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {
      //changed=true;
      if (m_audioStreams.size() == 1)
      {
        // notify the ITSReaderCallback. MP will then rebuild the graph
        LogDebug("OnNewChannel: Audio media types changed. SetMediaChanging(true)...");
        //m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
        m_AudioStreamType = newAudioStreamType ;
        m_audioChanged=true;
        m_mpegPesParser->AudioReset(); 
        SetMediaChanging(true); 
      }
    }
  }

  //if we have more than 1 audio track available, tell host application that we are ready
  //to receive an audio track change.
  if ((m_audioStreams.size() > 1) && m_filter.CheckAudioCallback())
  {
    LogDebug("OnNewChannel: OnRequestAudioChange()");
    SetAudioChanging(true);
    m_mpegPesParser->AudioValidReset(); 
    m_filter.OnRequestAudioChange();
  }
  else
  {
    m_AudioStreamType = newAudioStreamType;
    m_mpegPesParser->AudioReset(); 
  }

  LogDebug("OnNewChannel: New Audio stream type = 0x%x", m_AudioStreamType);

  if( pSubUpdateCallback != NULL)
  {
    int bitmap_index = -1;
    (*pSubUpdateCallback)(m_subtitleStreams.size(),(m_subtitleStreams.size() > 0 ? &m_subtitleStreams[0] : NULL),&bitmap_index);
    if(bitmap_index >= 0)
    {
      LogDebug("OnNewChannel: Calling SetSubtitleStream:  %i", bitmap_index);
      SetSubtitleStream(bitmap_index);
    }
  }
  
  m_bPatParsed = true;
}


void CDeMultiplexer::SetMediaChanging(bool onOff)
{
  CAutoLock lock (&m_sectionMediaChanging);
  if (m_bWaitForMediaChange != onOff)
  {
    LogDebug("demux:Wait for media format change:%d", onOff);
    m_bWaitForMediaChange=onOff;
    m_tWaitForMediaChange=GET_TIME_NOW() ;
  }
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
  
  if (m_filter.State() == State_Paused)
  {
    //Flush buffers to speed up zapping
    DelegatedFlush(true, false);
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

void CDeMultiplexer::CheckMediaChange(unsigned int Pid, bool isVideo)
{
  if (m_bStarting) 
	{
    if (isVideo)
    {
      if (m_lastVidResX!=m_mpegPesParser->basicVideoInfo.width || m_lastVidResY!=m_mpegPesParser->basicVideoInfo.height)
      {
        LogDebug("DeMultiplexer: %x new video format, %dx%d @ %d:%d, %.3fHz %s",Pid,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps,m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
        m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(int)m_bitRate,m_mpegPesParser->basicVideoInfo.isInterlaced);
      }

      m_lastVidResX     = m_mpegPesParser->basicVideoInfo.width;
      m_lastVidResY     = m_mpegPesParser->basicVideoInfo.height;
  	  m_lastARX         = m_mpegPesParser->basicVideoInfo.arx;
  	  m_lastARY         = m_mpegPesParser->basicVideoInfo.ary;
  	  m_lastStreamType  = m_mpegPesParser->basicVideoInfo.streamType;
    }
	  return; //do not check for dynamic changes when Start() is active
	}
	
	bool update = false;
		
  if (isVideo)
  {
    if ((m_lastARX != m_mpegPesParser->basicVideoInfo.arx || m_lastARY != m_mpegPesParser->basicVideoInfo.ary)
          && m_lastVidResX==m_mpegPesParser->basicVideoInfo.width && m_lastVidResY==m_mpegPesParser->basicVideoInfo.height)
    {
      LogDebug("DeMultiplexer: Video aspect ratio change to %d:%d", m_mpegPesParser->basicVideoInfo.arx, m_mpegPesParser->basicVideoInfo.ary);
      m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType, m_mpegPesParser->basicVideoInfo.width, m_mpegPesParser->basicVideoInfo.height, m_mpegPesParser->basicVideoInfo.arx, m_mpegPesParser->basicVideoInfo.ary,(int)m_bitRate, m_mpegPesParser->basicVideoInfo.isInterlaced);
    }
        
    if (m_lastVidResX!=m_mpegPesParser->basicVideoInfo.width || m_lastVidResY!=m_mpegPesParser->basicVideoInfo.height || m_lastStreamType != m_mpegPesParser->basicVideoInfo.streamType)
    {
      LogDebug("DeMultiplexer: %x video format changed, %dx%d @ %d:%d, %.3fHz %s",Pid,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(float)m_mpegPesParser->basicVideoInfo.fps,m_mpegPesParser->basicVideoInfo.isInterlaced ? "interlaced":"progressive");
      m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,(int)m_bitRate,m_mpegPesParser->basicVideoInfo.isInterlaced);
      m_videoChanged = true;

      if (m_mpegPesParser->basicAudioInfo.isValid && !IsAudioChanging())
      {
        LogDebug("DeMultiplexer: OnMediaTypeChanged() triggered by video 1, aud %d, vid %d", m_audioChanged, m_videoChanged);
        update = true;                
      }

      m_filter.GetVideoPin()->SetAddPMT();
    }
    else //video resolution is unchanged, but there may be other format changes
    {
      if (m_mpegPesParser->basicAudioInfo.isValid && !IsAudioChanging())
      {
        if (m_audioChanged || m_videoChanged)
        {
          LogDebug("DeMultiplexer: OnMediaTypeChanged() triggered by video 2, aud %d, vid %d", m_audioChanged, m_videoChanged);
          update = true;
            
          if (m_videoChanged)
            m_filter.GetVideoPin()->SetAddPMT();            
        }
      }
    }
  
    m_lastVidResX     = m_mpegPesParser->basicVideoInfo.width;
    m_lastVidResY     = m_mpegPesParser->basicVideoInfo.height;
	  m_lastARX         = m_mpegPesParser->basicVideoInfo.arx;
	  m_lastARY         = m_mpegPesParser->basicVideoInfo.ary;
    m_lastStreamType  = m_mpegPesParser->basicVideoInfo.streamType;
  }
  else  //audio
  {
    if (m_audioChanged && !IsAudioChanging())
    {
      m_filter.GetAudioPin()->SetAddPMT();
      if ((m_videoChanged && m_mpegPesParser->basicVideoInfo.isValid) || !m_videoChanged)
      {
        LogDebug("DeMultiplexer: OnMediaTypeChanged() triggered by audio 1, aud %d, vid %d", m_audioChanged, m_videoChanged);
        update = true;
      }
    }    
  }


  if (update)
  {
    if (m_audioChanged || m_videoChanged)
    {
      SetMediaChanging(true);
      
      if (m_audioChanged && m_videoChanged)
        m_filter.OnMediaTypeChanged(VIDEO_CHANGE | AUDIO_CHANGE);
      else if (m_audioChanged)
        m_filter.OnMediaTypeChanged(AUDIO_CHANGE);
      else
        m_filter.OnMediaTypeChanged(VIDEO_CHANGE);
        
      m_audioChanged = false;
      m_videoChanged = false;
    }
    else
    {
      SetMediaChanging(false);
    }
  }

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

void CDeMultiplexer::DelegatedFlush(bool forceNow, bool waitForFlush)
{
  if (m_bFlushDelgNow || m_bFlushRunning) //Flush already pending or in progress
  {
    return;
  }

  if (forceNow) 
  {
    m_bFlushDelgNow = true;
  }
  else
  {
    m_bFlushDelegated = true;
  }
  
  WakeThread();        

  if (waitForFlush && forceNow)
  {
    for(int i(0) ; ((i < 500) && (m_bFlushDelgNow || m_bFlushRunning)) ; i++)
    {
      Sleep(1);
    }
  }
}    

void CDeMultiplexer::PrefetchData()
{
  m_bReadAheadFromFile = true;
  WakeThread();        
}

DWORD CDeMultiplexer::GetMaxFileReadLatency()
{
  CAutoLock lock (&m_sectionRead); 
  DWORD maxFileReadLat = m_maxFileReadLatency;  
  m_maxFileReadLatency = 0;   
  return maxFileReadLat;
}

float CDeMultiplexer::GetAveFileReadLatency()
{
  CAutoLock lock (&m_sectionRead); 
  float aveFileReadLat = 0;  
  if (m_fileReadLatCount > 0)
  {
    aveFileReadLat = (float)m_fileReadLatSum/m_fileReadLatCount;
  }
  m_fileReadLatSum = 0; 
  m_fileReadLatCount = 0;   
  return aveFileReadLat;
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
  m_lastFlushTime = timeNow;
  DWORD  lastFileReadTime = timeNow;
  DWORD  lastRetryLoopTime = timeNow;
  int sizeRead = 0;
  bool retryRead = false;
  DWORD pfLoopDelay = PF_LOOP_DELAY_MIN;
  CRefTime rtStartTime;

  //Set basic thread priority
  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_NORMAL);
  
  //Tell the Multimedia Class Scheduler (MMCS) we are doing playback
  DWORD dwTaskIndex = 0;
  HANDLE hAvrt = SetMMCSThreadPlayback(&dwTaskIndex, AVRT_PRIORITY_NORMAL);
    
  do
  {

    timeNow = GET_TIME_NOW();
    
    //Flush delegated to this thread
    if (m_bFlushDelegated || m_bFlushDelgNow)
    {
      if (!m_bFlushDelgNow && ((timeNow - 500) < m_lastFlushTime)) 
      { 
        // Too early for next flush
        m_bFlushDelegated = false;
      }
      else
      {
        m_lastFlushTime = timeNow;
        
        if (m_filter.State() == State_Running)
        {
          m_filter.m_lastPauseRun = timeNow;
        }
  
        LogDebug("CDeMultiplexer::ThreadProc - Flush");     
        //Flush the internal data
        Flush(true, m_bFlushDelegated && !m_bFlushDelgNow);
        m_bFlushDelgNow = false;
        
        sizeRead = 0;
        retryRead = false;
      }
    }

    //File read prefetch section...

    if (!IsAudioChanging())
    {
      if (m_filter.GetAudioPin()->IsThreadRunning(&rtStartTime)) //Check the audio pin thread is running
      {
        
        //Forced for initial parsing and buffering, normal mode otherwise
        m_bReadAheadFromFile = CheckPrefetchState(m_filter.m_bStreamCompensated, !m_filter.m_bStreamCompensated);
      }
    }

    if (m_bReadAheadFromFile && (timeNow > (lastFileReadTime + pfLoopDelay - 1)) )
    {
      lastFileReadTime = timeNow; 
      int sizeReadTemp = ReadAheadFromFile((ULONG)(max(READ_SIZE-sizeRead, MIN_READ_SIZE))); 
      sizeRead += sizeReadTemp;
           
      if (
            (sizeReadTemp < 0) ||        //Read aborted or failed
            (sizeRead >= READ_SIZE) ||   //Got all the data we requested
            (retryRead && ((timeNow > (lastRetryLoopTime + MAX_PREFETCH_LOOP_TIME)) || !CheckPrefetchState(true, false)))  //Looping retry mode exit
          )
      {
        m_bReadAheadFromFile = false;
        sizeRead = 0;
        retryRead = false;
      }
      else if (!retryRead) //Enable looping retry mode
      {
        lastRetryLoopTime = timeNow;
        retryRead = true;
      }      
    }

    if (m_filter.GetAudioPin()->IsThreadRunning(&rtStartTime) && !IsMediaChanging() && !m_filter.IsSeeking())
    {
      CheckCompensation(rtStartTime);
    }
    
    pfLoopDelay = retryRead ? (m_filter.IsRTSP() ? 2 : (m_prefetchLoopDelay/2)) : m_prefetchLoopDelay;              
  }
  while (!ThreadIsStopping(pfLoopDelay)) ;

  //Revert MMCS
  RevertMMCSThread(hAvrt); 

  LogDebug("CDeMultiplexer::ThreadProc stopped()");
}

