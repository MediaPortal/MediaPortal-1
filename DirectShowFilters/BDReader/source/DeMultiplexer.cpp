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
#include <streams.h>
#include <math.h>
#include <wmcodecdsp.h>
#include "demultiplexer.h"
#include "..\..\shared\adaptionfield.h"
#include "bdreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "subtitlePin.h"
#include "..\..\DVBSubtitle3\Source\IDVBSub.h"
#include "mediaFormats.h"
#include "h264nalu.h"
#include "FrameHeaderParser.h"
#include <cassert>
#include <ks.h>
#include <ksmedia.h>

#include <bluray.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define PACKET_GRANULARITY 80000
#define CONVERT_90KHz_DS(x) (REFERENCE_TIME)(x * (1000.0 / 9.0))
#define CONVERT_DS_90KHz(x) (REFERENCE_TIME)(x / (1000.0 / 9.0))

const double mediaChangeTimeout = 5000;

extern void LogDebug(const char *fmt, ...);

CDeMultiplexer::CDeMultiplexer(CBDReaderFilter& filter) : m_filter(filter)
{
  m_filter.lib.SetEventObserver(this);
  m_patParser.SetCallBack(this);
  m_pCurrentVideoBuffer = NULL;
  m_pCurrentAudioBuffer = new Packet();
  m_pCurrentSubtitleBuffer = new Packet();
  m_iAudioStream = 0;
  m_AudioStreamType = SERVICE_TYPE_AUDIO_NOT_INIT;
  m_iSubtitleStream = 0;
  m_audioPid = 0;
  m_currentSubtitlePid = 0;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
  m_bHoldSubtitle = false;
  m_bShuttingDown = false;
  m_iAudioIdx = -1;
  m_iPatVersion = -1;
  m_bSetAudioDiscontinuity = false;
  m_bSetVideoDiscontinuity = false;
  pSubUpdateCallback = NULL;
  m_lastVideoPTS.IsValid = false;
  m_lastAudioPTS.IsValid = false;
  m_bRebuildOnVideoChange = false;
  m_bDoDelayedRebuild = false;
  SetMediaChanging(false);

  m_WaitHeaderPES = -1 ;
  m_mpegPesParser = new CMpegPesParser();

  m_audioStreamsToBeParsed = 0;
  m_bDelayedAudioTypeChange = false;

  m_bReadFailed = false;

  m_fHasAccessUnitDelimiters = false;
  m_fakeAudioVideoSeen = false;
  m_fakeAudioPacketCount = 0;

  m_audioPlSeen = false;
  m_videoPlSeen = false;
  m_playlistManager = new CPlaylistManager();
  m_loopLastSearch=1;

  m_videoServiceType = -1;
}

CDeMultiplexer::~CDeMultiplexer()
{
  m_filter.lib.RemoveEventObserver(this);
  m_bShuttingDown = true;

  delete m_playlistManager;

  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;
  delete m_mpegPesParser;

  ivecVBuffers itv = m_t_vecVideoBuffers.begin();
  while (itv != m_t_vecVideoBuffers.end())
  {
    Packet* videoBuffer = *itv;
    delete videoBuffer;
    itv = m_t_vecVideoBuffers.erase(itv);
  }

  m_subtitleStreams.clear();
  m_audioStreams.clear();
}

void CDeMultiplexer::ResetClipInfo(int pDebugMark)
{
  m_rtNewOffset = 0;
  m_newOffsetPos = 0;
  m_rtVideoOffset = 0;
  m_rtAudioOffset = 0;
  m_rtSubtitleOffset = 0;
  m_bOffsetBackwards = false;

  m_bForceUpdateAudioOffset = false;
  m_bForceUpdateVideoOffset = false;
  m_bForceUpdateSubtitleOffset = false;

  m_nClip = pDebugMark;
  m_nPlaylist = pDebugMark;

  m_nAudioClip = pDebugMark;
  m_nAudioPl = pDebugMark;
  m_nVideoClip = pDebugMark;
  m_nVideoPl = pDebugMark;
  m_nSubtitleClip = pDebugMark;
  m_nSubtitlePl = pDebugMark;
}

int CDeMultiplexer::GetVideoServiceType()
{
  if (m_pids.videoPids.size() > 0)
  {
    return m_pids.videoPids[0].VideoServiceType;
  }
  else
  {
    return SERVICE_TYPE_VIDEO_UNKNOWN;
  }
}

CPidTable CDeMultiplexer::GetPidTable()
{
  return m_pids;
}


/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed
bool CDeMultiplexer::SetAudioStream(int stream)
{
  LogDebug("SetAudioStream : %d", stream);
  if (stream < 0 || stream >= (int)m_audioStreams.size())
    return S_FALSE;

  m_iAudioStream = stream;

  // Get the new audio stream type
  int newAudioStreamType = SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream >= 0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType = m_audioStreams[m_iAudioStream].audioType;
  }

  LogDebug("Old Audio %d, New Audio %d", m_AudioStreamType, newAudioStreamType);

  if ((m_AudioStreamType != SERVICE_TYPE_AUDIO_NOT_INIT) && 
      (m_AudioStreamType != newAudioStreamType))
  {
    m_AudioStreamType = newAudioStreamType;
    if (m_filter.GetAudioPin()->IsConnected())
    {
	    // Here, stream is not parsed yet
      if (!IsMediaChanging())
      {
        LogDebug("SetAudioStream : OnMediaTypeChanged(1)");
        //Flush();

        // TODO - check
        //m_filter.OnMediaTypeChanged(1);
        //SetMediaChanging(true);
        //m_filter.m_bForceSeekOnStop = true;     // Force stream to be resumed after
      }
      else
      {
        LogDebug("SetAudioStream : Media already changing");
      }
    }
  }
  else
  {
    m_filter.GetAudioPin()->SetDiscontinuity(true);
  }

  return S_OK;
}

bool CDeMultiplexer::GetAudioStream(__int32 &audioIndex)
{
  audioIndex = m_iAudioStream;
  return S_OK;
}

void CDeMultiplexer::GetAudioStreamInfo(int stream, char* szName)
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
  if (m_AudioStreamType == SERVICE_TYPE_NO_AUDIO)
  {
    // TODO - return 1?
    return -1; // fake audio stream is present
  }
  else
  {
    return m_audioStreams.size();
  }
}

// TODO - use stream parser to get real channel numbers etc.

void CDeMultiplexer::GetAudioStreamType(int stream, CMediaType& pmt)
{
  if (m_AudioStreamType <= SERVICE_TYPE_NO_AUDIO)
  {
    pmt.InitMediaType();
    pmt.SetType(&MEDIATYPE_Audio);
    pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
    pmt.SetSampleSize(1);
    pmt.SetTemporalCompression(FALSE);
    pmt.SetVariableSize();
    pmt.SetFormatType(&FORMAT_WaveFormatEx);
    pmt.SetFormat(AC3AudioFormat, sizeof(AC3AudioFormat));

    return;
  }

  if (m_iAudioStream < 0 || stream >= (int)m_audioStreams.size())
  {
    pmt.InitMediaType();
    return;
  }

  switch (m_audioStreams[stream].audioType)
  {
    // MPEG1 shouldn't be mapped to MPEG2 audio as it will break Cyberlink audio codec
    // (and MPA is not working with the MPEG1 to MPEG2 mapping...)
    case SERVICE_TYPE_AUDIO_MPEG1:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_MPEG1Payload);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG1AudioFormat, sizeof(MPEG1AudioFormat));
      break;
  case SERVICE_TYPE_AUDIO_MPEG2:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_MPEG2_AUDIO);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG2AudioFormat, sizeof(MPEG2AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AAC:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AACAudioFormat, sizeof(AACAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_LATM_AAC:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_LATM_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AACAudioFormat, sizeof(AACAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AC3:
    case SERVICE_TYPE_AUDIO_DD_PLUS:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AC3AudioFormat, sizeof(AC3AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_MLP:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);//MEDIASUBTYPE_ARCSOFT_MLP);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AC3AudioFormat, sizeof(AC3AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_DTS:
    case SERVICE_TYPE_AUDIO_DTS_HD:
    case SERVICE_TYPE_AUDIO_TS_HD_XLL:
      pmt.InitMediaType();
      pmt.SetType(&MEDIATYPE_Audio);
      pmt.SetSubtype(&MEDIASUBTYPE_DTS);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(DTSAudioFormat, sizeof(DTSAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_LPCM:
      CAutoLock lockVid(&m_sectionVideo);
      CAutoLock lockAud(&m_sectionAudio);
      CAutoLock lockSub(&m_sectionSubtitle);
      
      if ((int)m_pids.audioPids.size() >= stream)
      {
        pmt = m_mAudioMediaTypes[m_pids.audioPids[stream].Pid];
      }
  }
}
// This methods selects the subtitle stream specified
bool CDeMultiplexer::SetSubtitleStream(__int32 stream)
{
  if (stream < 0 || stream >= (__int32)m_subtitleStreams.size())
    return S_FALSE;

  m_iSubtitleStream = stream;

  return S_OK;
}

bool CDeMultiplexer::GetCurrentSubtitleStream(__int32 &stream)
{
  stream = m_iSubtitleStream;
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamLanguage(__int32 stream, char* szLanguage)
{
  if (stream <0 || stream >= (__int32)m_subtitleStreams.size())
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
  count =(__int32) m_subtitleStreams.size();
  return S_OK;
}

bool CDeMultiplexer::SetSubtitleResetCallback(int(CALLBACK *cb)(int, void*, int*))
{
  pSubUpdateCallback = cb;
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamType(__int32 stream, __int32 &type)
{
  if (m_iSubtitleStream < 0 || m_iSubtitleStream >= m_subtitleStreams.size())
  {
    // Invalid stream number
    return S_FALSE;
  }

  type = m_subtitleStreams[m_iSubtitleStream].subtitleType;
  return S_OK;
}

void CDeMultiplexer::GetVideoStreamType(CMediaType &pmt)
{
  if (m_pids.videoPids.size() != 0 && m_mpegPesParser != NULL)
  {
    pmt = m_mpegPesParser->pmt;
  }
}

void CDeMultiplexer::FlushVideo()
{
  CVideoPin* videoPin = m_filter.GetVideoPin();

  if (videoPin && videoPin->IsConnected())
  {
    videoPin->DeliverBeginFlush();
  }

  LogDebug("demux:flush video");
  CAutoLock lock (&m_sectionVideo);

  // Clear PES temporary queue.
  ivecVBuffers it = m_t_vecVideoBuffers.begin();
  while (it != m_t_vecVideoBuffers.end())
  {
    Packet* videoBuffer = *it;
    {
      delete videoBuffer;
      it = m_t_vecVideoBuffers.erase(it);
      LogDebug("Flush Video - sample was removed clip: %d:%d pl: %d:%d start: %03.5f", 
        videoBuffer->nClipNumber, m_nVideoClip, videoBuffer->nPlaylist, m_nVideoPl, videoBuffer->rtStart / 10000000.0);
    }
  }


  m_lastVideoPTS.FromClock(0);
  m_lastVideoPTS.IsValid = false;

  delete m_pCurrentVideoBuffer;
  m_pCurrentVideoBuffer = NULL;

  m_p.Free();
  m_pBuild.Free();
  m_lastStart = 0;
  m_loopLastSearch = 1;
  m_pl.RemoveAll();
  m_fHasAccessUnitDelimiters = false;
  m_rtPrev = Packet::INVALID_TIME;

  m_VideoValidPES = true;
  m_mVideoValidPES = false;  
  m_WaitHeaderPES = -1;
  m_rtOffset = 0;

  if (videoPin && videoPin->IsConnected())
  {
    videoPin->DeliverEndFlush();
  }
}

void CDeMultiplexer::FlushAudio()
{
  CAudioPin* audioPin = m_filter.GetAudioPin();

  if (audioPin && audioPin->IsConnected())
  {
    audioPin->DeliverBeginFlush();
  }

  LogDebug("demux:flush audio");
  CAutoLock lock (&m_sectionAudio);

  m_lastAudioPTS.FromClock(0);
  m_lastAudioPTS.IsValid = false;

  m_AudioValidPES = false;

  delete m_pCurrentAudioBuffer;
  m_pCurrentAudioBuffer = new Packet();

  if (audioPin && audioPin->IsConnected())
  {
    audioPin->DeliverEndFlush();
  }
}

void CDeMultiplexer::FlushSubtitle()
{
  LogDebug("demux:flush subtitle");
  CAutoLock lock (&m_sectionSubtitle);
  delete m_pCurrentSubtitleBuffer;
  ivecSBuffers it = m_vecSubtitleBuffers.begin();
  
  while (it != m_vecSubtitleBuffers.end())
  {
    Packet* subtitleBuffer = *it;

    delete subtitleBuffer;
    it = m_vecSubtitleBuffers.erase(it);
    LogDebug("Flush Subtitle - sample was removed clip: %d:%d pl: %d:%d start: %03.5f", 
      subtitleBuffer->nClipNumber, m_nSubtitleClip, subtitleBuffer->nPlaylist, m_nPlaylist, subtitleBuffer->rtStart / 10000000.0);
  }

  m_pCurrentSubtitleBuffer = new Packet();
  
  m_pCurrentSubtitleBuffer->nPlaylist = -1;
  m_pCurrentSubtitleBuffer->nClipNumber = -1;
}

void CDeMultiplexer::Flush()
{
  LogDebug("demux:flushing");

  CAutoLock lockVid(&m_sectionVideo);
  CAutoLock lockAud(&m_sectionAudio);
  CAutoLock lockSub(&m_sectionSubtitle);

  SetHoldAudio(true);
  SetHoldVideo(true);
  SetHoldSubtitle(true);

  FlushAudio();
  FlushVideo();
  FlushSubtitle();

  Reset(); // PacketSync reset. 

  m_playlistManager->ClearAllButCurrentClip(true);
  //delete m_playlistManager;
  //m_playlistManager = new CPlaylistManager();

  //ResetClipInfo(-12);
  m_filter.m_bStreamCompensated = false;

  SetHoldAudio(false);
  SetHoldVideo(false);
  SetHoldSubtitle(false);
}

Packet* CDeMultiplexer::GetSubtitle()
{
  if (m_currentSubtitlePid == 0) return NULL;
  if (m_bEndOfFile) return NULL;
  if (m_bHoldSubtitle) return NULL;

  if (m_vecSubtitleBuffers.size() != 0)
  {
    CAutoLock lock (&m_sectionSubtitle);
    ivecSBuffers it = m_vecSubtitleBuffers.begin();
    Packet* subtitleBuffer = *it;
    m_vecSubtitleBuffers.erase(it);
    return subtitleBuffer;
  }

  return NULL;
}

Packet* CDeMultiplexer::GetVideo()
{
  if (HoldVideo())
  {
    return NULL;
  }

  while (!m_playlistManager->HasVideo())
  {
    if (m_filter.IsStopping() || m_bEndOfFile || m_filter.IsSeeking())
    {
      return NULL;
    }
    ReadFromFile(false, true);
  }
  Packet * ret = m_playlistManager->GetNextVideoPacket();
  if (ret==NULL)
  {
    LogDebug("No Video Data");
  }
  return ret;
  //if (m_filter.GetVideoPin()->IsConnected() && m_iAudioStream == -1) return NULL;

  //// If there is no video pid, then simply return NULL
  //if ((m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid==0) || 
  //  IsMediaChanging() || m_audioStreamsToBeParsed > 0 || !m_filter.m_bStreamCompensated)
  //{
  //  ReadFromFile(false, true);
  //  return NULL;
  //}

  //// when there are no video packets at the moment
  //// then try to read some from the current file
  //while ((m_vecVideoBuffers.size() == 0) || (m_FirstVideoSample >= m_LastAudioSample))
  //{
  //  //if filter is stopped or
  //  //end of file has been reached or
  //  //demuxer should stop getting video packets
  //  //then return NULL
  //  if (m_filter.m_bStopping || m_bEndOfFile)
  //  {
  //    return NULL;
  //  }
  //  
  //  if (ReadFromFile(false, true) < READ_SIZE) 
  //  {
  //    break;
  //  }
  //}

  ////are there video packets in the buffer?
  //if (m_vecVideoBuffers.size() != 0 && !IsMediaChanging() || m_audioStreamsToBeParsed > 0)
  //{
  //  CAutoLock lock (&m_sectionVideo);
  //  //yup, then return the next one
  //  ivecVBuffers it = m_vecVideoBuffers.begin();
  //  if (it != m_vecVideoBuffers.end())
  //  {
  //    Packet* videoBuffer = *it;
  //    //m_FirstVideoSample = videoBuffer->rtStart;
  //    m_vecVideoBuffers.erase(it);
  //    return videoBuffer;
  //  }
  //}

  //return NULL;
}


Packet* CDeMultiplexer::GetAudio(int playlist, int clip)
{
  if (HoldAudio())
  {
    return NULL;
  }

  Packet * ret = m_playlistManager->GetNextAudioPacket(playlist, clip);
  return ret;
}

///
///Returns the next audio packet
// or NULL if there is none available
Packet* CDeMultiplexer::GetAudio()
{
  if (HoldAudio())
  {
    return NULL;
  }

  while (!m_playlistManager->HasAudio())
  {
    if (m_filter.IsStopping() || m_bEndOfFile || m_filter.IsSeeking())
    {
      return NULL;
    }
    ReadFromFile(true, false);
  }
  Packet * ret = m_playlistManager->GetNextAudioPacket();
  if (ret==NULL)
  {
    LogDebug("No Audio Data");
  }
  return ret;
  //int readSize = 0;
  //if (m_iAudioStream < 0) return NULL;
  ////remove packets which aren't as expected before going any further
  //bool bChangeFound=false;
  //if (m_vecAudioBuffers.size()>0)
  //{
  //  ivecABuffers it = m_vecAudioBuffers.begin();
  //  
  //  Packet* firstAudiobuffer = *it;
  //  if (m_iAudioPlaylist!=firstAudiobuffer->nPlaylist || m_iAudioClip!=firstAudiobuffer->nClipNumber)
  //  {
  //    m_iAudioPlaylist=firstAudiobuffer->nPlaylist;
  //    m_iAudioClip=firstAudiobuffer->nClipNumber;
  //  }

  //  while (it != m_vecAudioBuffers.end())
  //  {
  //    Packet* audiobuffer = *it;
  //    if (!IsValidPacket(audiobuffer)) 
  //    {
  //      if (audiobuffer) 
  //      {
  //        LogDebug("Droping Audio Packet: playlist %d clip %d",audiobuffer->nPlaylist, audiobuffer->nClipNumber);
  //        delete audiobuffer;
  //      }
  //      m_vecAudioBuffers.erase(it);
  //    }
  //    else if (m_iAudioPlaylist!=audiobuffer->nPlaylist || m_iAudioClip!=audiobuffer->nClipNumber)
  //    {
  //      //clip change in buffer - just check out the first
  //      if (!bChangeFound && AudioMissingBetweenClips(m_iAudioPlaylist,audiobuffer->nPlaylist,m_iAudioClip,audiobuffer->nClipNumber))
  //      {
  //        bChangeFound=true;
  //        GenerateFakeAudio(m_iAudioPlaylist,audiobuffer->nPlaylist,m_iAudioClip,audiobuffer->nClipNumber);
  //      }
  //      ++it;
  //    }
  //    else
  //    {
  //      ++it;
  //    }
  //  }
  //}

  //if (m_AudioStreamType == SERVICE_TYPE_NO_AUDIO)
  //{
  //  ReadFromFile(true, false);
  //  // Clear PES temporary queue.
  //  delete m_pCurrentAudioBuffer;
  //  m_pCurrentAudioBuffer = new Packet();
  //  ivecABuffers it = m_vecAudioBuffers.begin();
  //  while (it != m_vecAudioBuffers.end())
  //  {
  //    Packet* audioBuffer = *it;
  //    {
  //      delete audioBuffer;
  //      it = m_vecAudioBuffers.erase(it);
  //      LogDebug("Flush Audio (for faking) - sample was removed clip: %d:%d pl: %d:%d start: %03.5f", 
  //        audioBuffer->nClipNumber, m_nAudioClip, audioBuffer->nPlaylist, m_nAudioPl, audioBuffer->rtStart / 10000000.0);
  //    }
  //  }

  //  PlaylistInfo * playlistInfo = GetCurrentVideoPlaylist();
  //  GenerateFakeAudio(m_iAudioPlaylist,playlistInfo->playlist, m_iAudioClip, playlistInfo->clip);
  //  m_AudioStreamType = SERVICE_TYPE_NO_AUDIO_GENERATED;
  //}

  //// if there is no audio pid, then simply return NULL
  //if (((m_audioPid == 0) || IsMediaChanging() || m_audioStreamsToBeParsed > 0) && m_AudioStreamType > SERVICE_TYPE_NO_AUDIO)
  //{
  //  ReadFromFile(true, false);
  //  return NULL;
  //}

  //// when there are no audio packets at the moment
  //// then try to read some from the current file
  //while (m_vecAudioBuffers.size() == 0 || !m_bAudioVideoReady)
  //{
  //  if (m_filter.m_bStopping || m_bEndOfFile) 
  //  {
  //    return NULL;
  //  }

  //  readSize = ReadFromFile(true, false);

  //  if ((m_vecAudioBuffers.size() == 0 && readSize < READ_SIZE) || IsMediaChanging() || m_audioStreamsToBeParsed > 0)
  //  {
  //    return NULL;
  //  }

  //  m_bAudioVideoReady = true;
  //}

  //CAutoLock lock(&m_sectionAudio);

  //if (m_vecAudioBuffers.size() > 0)
  //{
  //  ivecABuffers it = m_vecAudioBuffers.begin();
  //  Packet* audiobuffer = *it;
  //  //m_FirstAudioSample = audiobuffer->rtStart;
  //  m_vecAudioBuffers.erase(it);
  //  return audiobuffer;
  //}
  //else
  //{
  //  return NULL;
  //}
}


/// Starts the demuxer
/// This method will read the file until we found the pat/sdt
/// with all the audio/video pids
HRESULT CDeMultiplexer::Start()
{
  m_bReadFailed = false;
  m_bStarting = true;
  m_bRebuildOnVideoChange = false;
  m_bDoDelayedRebuild = false;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
  m_iPatVersion = -1;
  m_bSetAudioDiscontinuity = false;
  m_bSetVideoDiscontinuity = false;
  DWORD dwBytesProcessed = 0;
  DWORD m_Time = GetTickCount();

#ifdef DEBUG
  const DWORD readTimeout = 50000;  // give more time when debugging 
#else
  const DWORD readTimeout = 5000;
#endif

  while((GetTickCount() - m_Time) < readTimeout && !m_bReadFailed)
  {
    int BytesRead = ReadFromFile(false, false);
    if (BytesRead == 0) Sleep(10);
    if (dwBytesProcessed > INITIAL_READ_SIZE || GetAudioStreamCount() > 0)
    {
      if ((!m_mpegPesParser->basicVideoInfo.isValid &&  m_pids.videoPids.size() > 0 && 
        m_pids.videoPids[0].Pid > 1) && dwBytesProcessed < INITIAL_READ_SIZE)
      {
        dwBytesProcessed += BytesRead;
        continue;
      }
      // Seek to start - reset the libbluray reading position
      m_filter.lib.Seek(0);
      Flush();
      m_streamPcr.Reset();
      m_bStarting = false;

      CMediaType pmt;
      GetAudioStreamType(m_iAudioStream, pmt);
      m_filter.GetAudioPin()->SetInitialMediaType(&pmt);

      GetVideoStreamType(pmt);
      m_filter.GetVideoPin()->SetInitialMediaType(&pmt);

      return S_OK;
    }
    dwBytesProcessed += BytesRead;
  }
  
  m_streamPcr.Reset();
  m_bStarting = false;

  Flush();
  // Seek to start - reset the libbluray reading position
  m_filter.lib.Seek(0);

  return m_bReadFailed ? S_FALSE : S_OK;
}

void CDeMultiplexer::SetEndOfFile(bool bEndOfFile)
{
  m_bEndOfFile = bEndOfFile;
}

bool CDeMultiplexer::EndOfFile()
{
  return m_bEndOfFile;
}

/// This method reads the next READ_SIZE bytes from the file
/// and processes the raw data
/// When a TS packet has been discovered, OnTsPacket(byte* tsPacket) gets called
//  which in its turn deals with the packet
int CDeMultiplexer::ReadFromFile(bool isAudio, bool isVideo)
{
  if (m_filter.IsSeeking() || m_filter.IsStopping()) return 0;
  CAutoLock lock (&m_sectionRead);
  int dwReadBytes = 0;
  bool pause = false;


  dwReadBytes = m_filter.lib.Read(m_readBuffer, sizeof(m_readBuffer), pause, m_bStarting);
//  m_nAudioClip=clip;
//  m_nVideoClip=clip;
//  m_nAudioPl=pl;
//  m_nVideoPl=pl;
//  LogDebug("TEST: PL %d, CLIP %d", pl, clip);

  if (dwReadBytes != sizeof(m_readBuffer))
    LogDebug("got less bytes than expected");

  if (dwReadBytes > 0)
  {
    OnRawData(m_readBuffer, (int)dwReadBytes);
    return dwReadBytes;
  }
  else if (dwReadBytes == -1)
  {
    LogDebug("Read failed...failure on libbluray side");
    m_bReadFailed = true;
  }
  else if (!pause)
  {
    LogDebug("Read failed...EOF");
    m_bEndOfFile = true;
  }

  return 0;
}

void CDeMultiplexer::ResetStream()
{
  m_bSetVideoDiscontinuity = true;
  m_bSetAudioDiscontinuity = true;
}

void CDeMultiplexer::HandleBDEvent(BD_EVENT& pEv, UINT64 /*pPos*/)
{
  switch (pEv.event)
  {
    case BD_EVENT_SEEK:
      // TODO: check this
      //ResetStream();
      break;

    case BD_EVENT_STILL_TIME:
      break;

    case BD_EVENT_STILL:
      break;

    case BD_EVENT_TITLE:
      break;

    case BD_EVENT_PLAYLIST:
      //ResetStream();
//      m_nClip = -10; // to make tracking easier
      m_nPlaylist = pEv.param;
      m_nAudioPl = pEv.param;
      m_nVideoPl = pEv.param;
      
      break;

    case BD_EVENT_PLAYITEM:
      LogDebug("New playitem %d",pEv.param);
      UINT64 clipStart = 0, clipIn = 0, bytePos = 0, duration = 0;
      int ret = m_filter.lib.GetClipInfo(pEv.param, &clipStart, &clipIn, &bytePos, &duration);
      if (ret) 
      {
        REFERENCE_TIME rtOldOffset = m_rtNewOffset;

        m_newOffsetPos = bytePos;

        /*
        if (m_nClip == pEv.param)
        {
          LogDebug("demux: clip changed - same clip!");
        }
        else*/
        {
          m_rtNewOffset = clipStart - clipIn;
        }

        rtOldOffset > m_rtNewOffset ? m_bOffsetBackwards = true : m_bOffsetBackwards = false;

        m_nClip = pEv.param;

        BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();

        REFERENCE_TIME clipOffset = m_rtNewOffset * -1;

        //TODO is m_nPlaylist always set?
        bool interrupted = m_playlistManager->CreateNewPlaylistClip(m_nPlaylist, m_nClip, clip->audio_stream_count > 0, CONVERT_90KHz_DS(clipIn), CONVERT_90KHz_DS(clipOffset), CONVERT_90KHz_DS(duration));

        if (interrupted)
        {
          LogDebug("demux: current clip was irterrupted - triggering flush");

          SetHoldVideo(true);
          SetHoldAudio(true);
          SetHoldSubtitle(true);

          m_filter.IssueCommand(FLUSH, 0);
        }

        m_nAudioClip = pEv.param;
        m_nVideoClip = pEv.param;
        m_nSubtitleClip = pEv.param;

        CPcr offset;
        CPcr oldOffset;
        offset.PcrReferenceBase = abs(m_rtNewOffset);
        oldOffset.PcrReferenceBase = abs(rtOldOffset);
        
        m_rtAudioOffset = m_rtNewOffset;
        m_rtVideoOffset = m_rtNewOffset;
        m_rtSubtitleOffset = m_rtNewOffset;              
        
        if (m_rtNewOffset < 0)
          LogDebug("demux: clip changed - offset: %I64d (-%3.3f) oldOffset: %I64d (-%3.3f) bytepos: %I64u backwards: %d dur: %6.3f audio: %d", 
            m_rtNewOffset, offset.ToClock(), rtOldOffset, oldOffset.ToClock(), m_newOffsetPos, m_bOffsetBackwards, 
            CONVERT_90KHz_DS(duration) / 10000000.0, clip->audio_stream_count);
        else
          LogDebug("demux: clip changed - offset: %I64d (%3.3f) oldOffset: %I64d (%3.3f) bytepos: %I64u backwards: %d dur: %6.3f audio: %d", 
            m_rtNewOffset, offset.ToClock(), rtOldOffset, oldOffset.ToClock(), m_newOffsetPos, m_bOffsetBackwards, 
            CONVERT_90KHz_DS(duration) / 10000000.0, clip->audio_stream_count);

/*
        if (rtOldOffset == m_rtNewOffset)
        {
          LogDebug("demux: clip changed - offset is same as previously - force clip & pl info update");
          m_bForceUpdateAudioOffset = true;
          m_bForceUpdateVideoOffset = true;
          m_bForceUpdateSubtitleOffset = true;
        }*/
      }
  } 
}

void CDeMultiplexer::HandleOSDUpdate(OSDTexture& /*pTexture*/)
{ 
}

void CDeMultiplexer::HandleMenuStateChange(bool /*pVisible*/)
{
}

/// This method gets called via ReadFile() when a new TS packet has been received
/// if will :
///  - decode any new pat/pmt/sdt
///  - decode any audio/video packets and put the PES packets in the appropiate buffers
void CDeMultiplexer::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);

  m_patParser.OnTsPacket(tsPacket);

  if (m_iPatVersion == -1)
  {
    // First PAT not found
    return;
  }

  if (m_pids.PcrPid == 0) return;
  if (header.Pid == 0) return;
  if (header.TScrambling) return;
  if (header.TransportError) return;

  if (header.Pid == m_pids.PcrPid)
  {
    CAdaptionField field;
    field.Decode(header,tsPacket);
    if (field.Pcr.IsValid)
    {
      // Then update our stream pcr which holds the current playback timestamp
      m_streamPcr = field.Pcr;
    }
  }

  // As long as we dont have a stream pcr timestamp we return
  if (m_streamPcr.IsValid == false)
  {
    return;
  }

  FillSubtitle(header, tsPacket);
  FillAudio(header, tsPacket);
  FillVideo(header, tsPacket);
}


/// This method will check if the tspacket is an audio packet
/// ifso, it decodes the PES audio packet and stores it in the audio buffers
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  //LogDebug("FillAudio - audio PID %d", m_audioPid );

  ParseAudioHeader(header, tsPacket);

  if (m_iAudioStream < 0 || m_iAudioStream >= m_audioStreams.size()) return;

  m_audioPid = m_audioStreams[m_iAudioStream].pid;
  if (m_audioPid == 0 || m_audioPid != header.Pid) return;
  if (m_filter.GetAudioPin()->IsConnected()==false) return;
  if (header.AdaptionFieldOnly()) return;

  bool packetProcessed = false;

  CAutoLock lock (&m_sectionAudio);
  if (header.PayloadUnitStart)
  {

    byte* p = tsPacket + header.PayLoadStart;
    if ((p[0] == 0) && (p[1] == 0) && (p[2] == 1))
    {
      CPcr pts;
      CPcr dts;



//      if (m_pCurrentAudioBuffer) m_playlistManager->SubmitAudioPacket(m_pCurrentAudioBuffer);
      m_pCurrentAudioBuffer = new Packet();
      CMediaType pmt;
      GetAudioStreamType(m_iAudioStream, pmt);
      if (!m_pCurrentAudioBuffer->pmt) m_pCurrentAudioBuffer->pmt = CreateMediaType(&pmt);

      if (CPcr::DecodeFromPesHeader(p, 0, pts, dts))
      {
        if (m_lastAudioPTS.ToClock() == 0 ||
            ((m_rtAudioOffset != m_rtNewOffset || m_bForceUpdateAudioOffset) && 
            ((m_bOffsetBackwards && pts > m_lastAudioPTS) ||
            (!m_bOffsetBackwards && m_lastAudioPTS > pts))))
        {
//          LogDebug("demux: audio offset changed old: %I64d new: %I64d", m_rtAudioOffset, m_rtNewOffset);
//          m_rtAudioOffset = m_rtNewOffset;
//          m_nAudioClip = m_nClip;
//          m_nAudioPl = m_nPlaylist;
//          m_bForceUpdateAudioOffset = false;
        }

        CPcr correctedPts;
        correctedPts.PcrReferenceBase = pts.PcrReferenceBase + m_rtAudioOffset;
        correctedPts.IsValid = true;

//          LogDebug("demux: aud last pts: %6.3f pts %6.3f corr %6.3f clip: %d playlist: %d", 
//            m_lastAudioPTS.ToClock(), pts.ToClock(), correctedPts.ToClock(), m_nAudioClip, m_nAudioPl);

        m_lastAudioPTS = pts;
        m_pCurrentAudioBuffer->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        m_pCurrentAudioBuffer->rtStop = m_pCurrentAudioBuffer->rtStart + 1;
          
        m_pCurrentAudioBuffer->nClipNumber = m_nAudioClip;
        m_pCurrentAudioBuffer->nPlaylist = m_nAudioPl;

        REFERENCE_TIME Ref = m_pCurrentAudioBuffer->rtStart;

        BYTE pesHeader[256];

        int pos = header.PayLoadStart;
        int pesHeaderLen = tsPacket[pos+8] + 9;
        memcpy(pesHeader, &tsPacket[pos], pesHeaderLen);
        pos += pesHeaderLen;
        
        // Calculate expected payload length
        m_nAudioPesLenght = ( pesHeader[4] << 8 ) + pesHeader[5] - (pesHeaderLen - 6);

        int len = 188 - pesHeaderLen - 4; // 4 for TS packet header
        if (len > 0)
        { 
          byte* ps = p + pesHeaderLen;
          m_pCurrentAudioBuffer->SetCount(len);
          memcpy(m_pCurrentAudioBuffer->GetData(), ps, len);
        }
        else
        {
          LogDebug(" No data");
          m_AudioValidPES = false;
        }
        packetProcessed = true;
      }
    }
    else
    {
      LogDebug("Pes header 0-0-1 fail");
      m_AudioValidPES = false;
    }

    if (m_AudioValidPES)
    {
      if (m_bSetAudioDiscontinuity)
      {
        m_bSetAudioDiscontinuity = false;
        m_pCurrentAudioBuffer->bDiscontinuity = true;
      }
    }
    else
    {
      m_bSetAudioDiscontinuity = true;
    }
    m_AudioValidPES = true;     
  }

  if (m_AudioValidPES && !packetProcessed)
  {
    int pos = header.PayLoadStart;

    if (pos > 0 && pos < 188)
    {
      int dataLenght = 188 - pos;
      m_pCurrentAudioBuffer->SetCount(m_pCurrentAudioBuffer->GetDataSize() + dataLenght);
      memcpy(m_pCurrentAudioBuffer->GetData() + m_pCurrentAudioBuffer->GetDataSize() - dataLenght, &tsPacket[pos], dataLenght);
    }

    if (m_pCurrentAudioBuffer->GetCount() == m_nAudioPesLenght)
    {
//      m_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      m_playlistManager->SubmitAudioPacket(m_pCurrentAudioBuffer);
//      m_pCurrentAudioBuffer = new Packet();
      m_pCurrentAudioBuffer=NULL;
      m_nAudioPesLenght = 0;
    }
//    else if (m_pCurrentAudioBuffer->GetCount() > m_nAudioPesLenght)
//    {
//      delete m_pCurrentAudioBuffer;
//      m_pCurrentAudioBuffer = new Packet();
//      m_nAudioPesLenght = 0;
//      LogDebug("demux: mismatch in audio PES packet lenght! (%d vs. %d)", m_nAudioPesLenght, m_pCurrentAudioBuffer->GetCount());
//    }
  }
}

void CDeMultiplexer::ParseAudioHeader(CTsHeader& header, byte* tsPacket)
{
  if (m_audioStreamsToBeParsed > 0)
  {
    bool isAudioPid = false;

    for (unsigned int i = 0; i < m_pids.audioPids.size() ; i++)
    {
      if (m_pids.audioPids[i].Pid == header.Pid)
      {
        isAudioPid = true;
        break;
      }
    }

    if (isAudioPid)
    {
      CAutoLock lockVid(&m_sectionVideo);
      CAutoLock lockAud(&m_sectionAudio);
      CAutoLock lockSub(&m_sectionSubtitle);
      
      if (header.PayloadUnitStart)
      {
        int pos = header.PayLoadStart;

        if ((tsPacket[pos] == 0) && (tsPacket[pos + 1] == 0) && (tsPacket[pos + 2] == 1))
        {
          // Skip pes header
          int headerLen = 9 + tsPacket[pos + 8];
          int len = 188 - headerLen;
          if (len > 0)
          { 
            byte* dataStart = &tsPacket[headerLen + pos];

            map<int, CMediaType>::iterator it;
            it = m_mAudioMediaTypes.find(header.Pid);

            if (it == m_mAudioMediaTypes.end())
            {
              CMediaType pmt;
              bdlpcmhdr hr;
              m_audioParser.Reset(dataStart, len);
              if (m_audioParser.Read(hr, len, &pmt))
              {
                m_mAudioMediaTypes.insert(pair<int, CMediaType>(header.Pid, pmt));
                m_audioStreamsToBeParsed--;
                
                if (m_audioStreamsToBeParsed == 0 && m_bDelayedAudioTypeChange)
                {
                  LogDebug("DEBUG parse  m_filter.OnMediaTypeChanged(1)");
                  //m_filter.OnMediaTypeChanged(1);
                }
              }
            }
          }
        }
      }
    }
  }
}

/// This method will check if the tspacket is an video packet
void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{
  if (m_pids.videoPids.size() == 0 || m_pids.videoPids[0].Pid == 0) return;
  if (header.Pid != m_pids.videoPids[0].Pid) return;

  if (header.AdaptionFieldOnly()) return;

  CAutoLock lock (&m_sectionVideo);

  if (m_bShuttingDown) return;

  if (header.PayloadUnitStart)
  {
    CPcr pts;
    CPcr dts;
      
    BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();
    m_videoServiceType = clip->video_streams->coding_type;      
      
    BYTE* start = tsPacket + header.PayLoadStart;
  
    if (CPcr::DecodeFromPesHeader(start, 0, pts, dts))
    {
      if (m_lastVideoPTS.ToClock() == 0 ||
          ((m_rtVideoOffset != m_rtNewOffset || m_bForceUpdateVideoOffset) &&
          ((m_bOffsetBackwards && pts > m_lastVideoPTS) ||
          (!m_bOffsetBackwards && m_lastVideoPTS > pts))))
      {
        //LogDebug("demux: video offset changed old: %I64d new: %I64d", m_rtVideoOffset, m_rtNewOffset);
//        m_rtVideoOffset = m_rtNewOffset;

//        m_nVideoClip = m_nClip;
//        m_nVideoPl = m_nPlaylist;
             
//        BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();
//        m_videoServiceType = clip->video_streams->coding_type;

//        m_bForceUpdateVideoOffset = false;
      }
    }
  }

  if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_MPEG1 ||
      m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_MPEG2)
  {
    FillVideoMPEG2(header, tsPacket);
  }
  else
  {
    FillVideoH264(header, tsPacket);
  }
}

void CDeMultiplexer::PacketDelivery(Packet* pIn, CTsHeader header)
{
  Packet* p = new Packet();
  p->SetData(pIn->GetData(), pIn->GetDataSize());
  p->rtStart = pIn->rtStart;
  p->rtStop = pIn->rtStop;
  p->bDiscontinuity = pIn->bDiscontinuity;
  p->nClipNumber = pIn->nClipNumber;
  p->nPlaylist = pIn->nPlaylist;
  CPcr timestamp;
  
  if (p->rtStart != Packet::INVALID_TIME)
  {
    timestamp.PcrReferenceBase = CONVERT_DS_90KHz(p->rtStart);
    timestamp.IsValid=true;
  }
    
  if (m_filter.GetVideoPin()->IsConnected())
  {
    if (p->rtStart != Packet::INVALID_TIME)
    {
      m_LastValidFrameCount++;
    }
    
    //pCurrentVideoBuffer->SetFrameType(Gop? 'I':'?');
    //pCurrentVideoBuffer->SetFrameCount(0);
    //pCurrentVideoBuffer->SetVideoServiceType(m_pids.videoPids[0].VideoServiceType);
    if (m_bSetVideoDiscontinuity)
    {
      m_bSetVideoDiscontinuity = false;
      p->bDiscontinuity = true;
    }
    CheckVideoFormat(p);
    m_playlistManager->SubmitVideoPacket(p);
//    CorrectPacketPlaylist(p, SUPERCEEDED_VIDEO);
//    m_vecVideoBuffers.push_back(p);
  }
  else
  {
    CheckVideoFormat(p);
    delete p;
  }
}

void CDeMultiplexer::CheckVideoFormat(Packet* p)
{
  int lastVidResX = m_mpegPesParser->basicVideoInfo.width;
  int lastVidResY = m_mpegPesParser->basicVideoInfo.height;
  int laststreamType = m_mpegPesParser->basicVideoInfo.streamType;

  m_mpegPesParser->OnTsPacket(p->GetData(), p->GetCount(), m_videoServiceType);
  
  p->pmt = CreateMediaType(&m_mpegPesParser->pmt);

  if ((lastVidResX != 0 && lastVidResY != 0 ) && 
    (lastVidResX != m_mpegPesParser->basicVideoInfo.width || 
    lastVidResY != m_mpegPesParser->basicVideoInfo.height ||
    laststreamType != m_mpegPesParser->basicVideoInfo.streamType))
  {
    CMediaType pmt;
    GetAudioStreamType(m_iAudioStream, pmt);
			  
    if (m_audioStreams.size() == 0 || pmt.formattype != GUID_NULL)
    {                
      LogDebug("demux: video format changed: res = %dx%d aspectRatio = %d:%d fps = %d isInterlaced = %d",
        m_mpegPesParser->basicVideoInfo.width, m_mpegPesParser->basicVideoInfo.height,
        m_mpegPesParser->basicVideoInfo.arx, m_mpegPesParser->basicVideoInfo.ary,
        m_mpegPesParser->basicVideoInfo.fps, m_mpegPesParser->basicVideoInfo.isInterlaced);

      if (m_bRebuildOnVideoChange)
      {
        LogDebug("demux: OnMediaFormatChange triggered by mpeg2Parser");
        //SetMediaChanging(true);
        //m_filter.OnMediaTypeChanged(3);
        m_bRebuildOnVideoChange = false;
      }
      LogDebug("demux: triggering OnVideoFormatChanged");
      m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType, m_mpegPesParser->basicVideoInfo.width,
        m_mpegPesParser->basicVideoInfo.height, m_mpegPesParser->basicVideoInfo.arx,
        m_mpegPesParser->basicVideoInfo.ary, 15000000, m_mpegPesParser->basicVideoInfo.isInterlaced);
    }
    else
    {
      LogDebug("demux: audio is not ready yet - m_bDoDelayedRebuild = true");
      m_bDoDelayedRebuild = true;
    }
  }
  else
  {
    if (m_bRebuildOnVideoChange && m_bDoDelayedRebuild)
    {
      CMediaType pmt;
      GetAudioStreamType(m_iAudioStream, pmt);
			  
      if (m_audioStreams.size() == 0 || pmt.formattype != GUID_NULL)
      {                
        LogDebug("demux: triggering a delayed rebuild");
        //SetMediaChanging(true);
        //m_filter.OnMediaTypeChanged(3);
        m_bRebuildOnVideoChange = false;
        m_bDoDelayedRebuild = false;
      }
    }
  }
}

void CDeMultiplexer::FillVideoH264PESPacket(CTsHeader& header, CAutoPtr<Packet> p)
{
  //LogDebug("Recieved Packet %d %I64d", p->GetDataSize(), p->rtStart);

  if (!m_p) 
  {
    //LogDebug("FillVideoH264PESPacket %I64d %d %d", p->rtStart, p->nPlaylist, p->nClipNumber);

    m_p.Attach(new Packet());
    m_p->SetCount(0, PACKET_GRANULARITY);
		m_p->bDiscontinuity = p->bDiscontinuity;
		p->bDiscontinuity = FALSE;

		m_p->bSyncPoint = p->bSyncPoint;
		p->bSyncPoint = FALSE;

		m_p->rtStart = p->rtStart;
		p->rtStart = Packet::INVALID_TIME;

		m_p->rtStop = p->rtStop;
		p->rtStop = Packet::INVALID_TIME;

    m_p->nClipNumber = p->nClipNumber;
    m_p->nPlaylist = p->nPlaylist;
	}

	m_p->Append(*p);

	BYTE* start = m_p->GetData();
	BYTE* end = start + m_p->GetCount();

	while (start <= end - 4 && *(DWORD*)start != 0x01000000) start++;
	while (start <= end - 4) 
  {
		BYTE* next =  start + (m_loopLastSearch==0 ? 1 : m_loopLastSearch);
		while(next <= end - 4 && *(DWORD*)next != 0x01000000) next++;
    m_loopLastSearch = next - start;
    if(next >= end - 4) break;
		int size = next - start;
		CH264Nalu	nalu;
    nalu.SetBuffer(start, size, 0);
		CAutoPtr<Packet> p2(new Packet());
    p2->SetCount(0, PACKET_GRANULARITY);
		while (nalu.ReadNext()) 
    {
			DWORD	dwNalLength =
				((nalu.GetDataLength() >> 24) & 0x000000ff) |
				((nalu.GetDataLength() >>  8) & 0x0000ff00) |
				((nalu.GetDataLength() <<  8) & 0x00ff0000) |
				((nalu.GetDataLength() << 24) & 0xff000000);

			CAutoPtr<Packet> p3(new Packet());

			p3->SetCount (nalu.GetDataLength() + sizeof(dwNalLength));
			memcpy(p3->GetData(), &dwNalLength, sizeof(dwNalLength));
			memcpy(p3->GetData() + sizeof(dwNalLength), nalu.GetDataBuffer(), nalu.GetDataLength());

      //LogDebug("Input Nalu type %d (%d)",Nalu.GetType(),Nalu.GetDataLength());

			if (p2 == NULL) p2 = p3;
      else p2->Append(*p3);
		}

		p2->bDiscontinuity = m_p->bDiscontinuity;
		m_p->bDiscontinuity = FALSE;
		p2->bSyncPoint = m_p->bSyncPoint;
		m_p->bSyncPoint = FALSE;
		p2->rtStart = m_p->rtStart;
		m_p->rtStart = Packet::INVALID_TIME;
		p2->rtStop = m_p->rtStop;
		m_p->rtStop = Packet::INVALID_TIME;
    p2->nClipNumber = m_p->nClipNumber;
    m_p->nClipNumber = -2; // to easen tracking
    p2->nPlaylist = m_p->nPlaylist;
    m_p->nClipNumber = -2; // to easen tracking
		
    m_pl.AddTail(p2);

		if (p->rtStart != Packet::INVALID_TIME) 
    {
			m_p->rtStart = p->rtStart;
			m_p->rtStop = p->rtStop;
			p->rtStart = Packet::INVALID_TIME;
		}
		if (p->bDiscontinuity) 
    {
			m_p->bDiscontinuity = p->bDiscontinuity;
			p->bDiscontinuity = FALSE;
		}
		if (p->bSyncPoint) 
    {
			m_p->bSyncPoint = p->bSyncPoint;
			p->bSyncPoint = FALSE;
		}

    m_p->nClipNumber = p->nClipNumber;
    m_p->nPlaylist = p->nPlaylist;

    start = next;
	}
	if (start > m_p->GetData()) 
  {
	  m_loopLastSearch = 1;
		m_p->RemoveAt(0, start - m_p->GetData());
	}

	for(POSITION pos = m_pl.GetHeadPosition(); pos; m_pl.GetNext(pos)) 
  {
		if (pos == m_pl.GetHeadPosition()) 
    {
			continue;
		}

		Packet* pPacket = m_pl.GetAt(pos);
		BYTE* pData = pPacket->GetData();

    if (!pData)
      continue;

		if ((pData[4]&0x1f) == 0x09) 
    {
			m_fHasAccessUnitDelimiters = true;
		}

		if ((pData[4]&0x1f) == 0x09 || !m_fHasAccessUnitDelimiters && pPacket->rtStart != Packet::INVALID_TIME) 
    {
			p = m_pl.RemoveHead();

			while (pos != m_pl.GetHeadPosition()) 
      {
				CAutoPtr<Packet> p2 = m_pl.RemoveHead();
				p->Append(*p2);
			}
      //LogDebug("PacketDelivery - %6.3f %d %d", pIn->rtStart / 10000000.0, pIn->nPlaylist, pIn->nClipNumber);
			PacketDelivery(p, header);
		}
	}
}

void CDeMultiplexer::FillVideoVC1PESPacket(CTsHeader& header, CAutoPtr<Packet> p)
{
  if (!m_p) 
  {
		m_p.Attach(new Packet());
    m_p->SetCount(0, PACKET_GRANULARITY);
		m_p->bDiscontinuity = p->bDiscontinuity;
		p->bDiscontinuity = FALSE;

		m_p->bSyncPoint = p->bSyncPoint;
		p->bSyncPoint = FALSE;

		m_p->rtStart = p->rtStart;
		p->rtStart = Packet::INVALID_TIME;

		m_p->rtStop = p->rtStop;
		p->rtStop = Packet::INVALID_TIME;

    m_p->nClipNumber = p->nClipNumber;
    p->nClipNumber = -3; // to easen tracking

    m_p->nPlaylist = p->nPlaylist;
    p->nClipNumber = -3; // to easen tracking
	}

	m_p->Append(*p);

  /*
    * 0x0A: end of sequence
    * 0x0B: slice
    * 0x0C: field
    * 0x0D: frame header
    * 0x0E: entry point header
    * 0x0F: sequence header
    * 0x1B: user-defined slice
    * 0x1C: user-defined field
    * 0x1D: user-defined frame header
    * 0x1E: user-defined entry point header
    * 0x1F: user-defined sequence header 
    */

	BYTE* start = m_p->GetData();
	BYTE* end = start + m_p->GetCount();

  start = m_p->GetData();

	bool bSeqFound = false;
	while(start <= end-4) {
		if (*(DWORD*)start == 0x0D010000) {
			bSeqFound = true;
			break;
		} else if (*(DWORD*)start == 0x0F010000) {
			break;
		}
		start++;
	}

	while(start <= end-4) {
		BYTE* next = start + (m_loopLastSearch==0 ? 1 : m_loopLastSearch);

		while(next <= end-4) {
			if (*(DWORD*)next == 0x0D010000) {
				if (bSeqFound) {
					break;
				}
				bSeqFound = true;
			} else if (*(DWORD*)next == 0x0F010000) {
				break;
			}
			next++;
		}
    m_loopLastSearch = next - start;
		if(next >= end-4) {
			break;
		}

		int size = next - start - 4;
		UNUSED_ALWAYS(size);

		CAutoPtr<Packet> p2(new Packet());
		p2->bDiscontinuity = m_p->bDiscontinuity;
		m_p->bDiscontinuity = FALSE;

		p2->bSyncPoint = m_p->bSyncPoint;
		m_p->bSyncPoint = FALSE;

		p2->rtStart = m_p->rtStart;
		m_p->rtStart = Packet::INVALID_TIME;

		p2->rtStop = m_p->rtStop;
		m_p->rtStop = Packet::INVALID_TIME;

		p2->pmt = m_p->pmt;
		m_p->pmt = NULL;

    p2->nClipNumber = m_p->nClipNumber;
    p2->nPlaylist = m_p->nPlaylist;

		p2->SetData(start, next - start);

    PacketDelivery(p2, header);
   
		if (p->rtStart != Packet::INVALID_TIME) 
    {
			m_p->rtStart = p->rtStop; //p->rtStart; //Sebastiii for enable VC1 decoding in FFDshow (no more shutter)
			m_p->rtStop = p->rtStop;
			p->rtStart = Packet::INVALID_TIME;
		}
		if (p->bDiscontinuity) 
    {
			m_p->bDiscontinuity = p->bDiscontinuity;
			p->bDiscontinuity = FALSE;
		}

		if (p->bSyncPoint) 
    {
			m_p->bSyncPoint = p->bSyncPoint;
			p->bSyncPoint = FALSE;
		}

		if (m_p->pmt) 
    {
			DeleteMediaType(m_p->pmt);
		}

    m_p->nClipNumber = p->nClipNumber;
    m_p->nPlaylist = p->nPlaylist;

		m_p->pmt = p->pmt;
		p->pmt = NULL;

		start = next;
		bSeqFound = (*(DWORD*)start == 0x0D010000);
    m_loopLastSearch=1;
    m_p->RemoveAt(0, start - m_p->GetData());
	}
}

void CDeMultiplexer::FillVideoH264(CTsHeader& header, byte* tsPacket)
{
  int headerlen = header.PayLoadStart;

  CPcr pts;
  CPcr dts;

  if (!m_pBuild)
  {
    m_pBuild.Attach(new Packet());
    m_pBuild->bDiscontinuity = false;
    m_pBuild->rtStart = Packet::INVALID_TIME;
    m_pBuild->nClipNumber = m_nVideoClip;
    m_pBuild->nPlaylist = m_nVideoPl;

    m_lastStart = 0;
  }
  
  if (header.PayloadUnitStart)
  {
    //LogDebug("demux::FillVideoH264 PayLoad Unit Start");
    m_WaitHeaderPES = m_pBuild->GetCount();
    m_mVideoValidPES = m_VideoValidPES;

    if (m_pids.videoPids[0].VideoServiceType == SERVICE_TYPE_VIDEO_VC1 && m_pBuild->GetCount() > 0)
    {
      FillVideoVC1PESPacket(header, m_pBuild);
      m_pBuild.Free();

      m_pBuild.Attach(new Packet());
      m_pBuild->bDiscontinuity = false;
      m_pBuild->rtStart = Packet::INVALID_TIME;
      m_pBuild->nClipNumber = m_nVideoClip;
      m_pBuild->nPlaylist = m_nVideoPl;
      m_lastStart = 0;
      m_WaitHeaderPES = 0;
    }
  }
  
  CAutoPtr<Packet> p(new Packet());
  
  if (headerlen < 188)
  {            
    int dataLen = 188-headerlen;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen],dataLen);

    m_pBuild->Append(*p);
  }
  else
    return;

  if (m_WaitHeaderPES >= 0)
  {
    int AvailablePESlength = m_pBuild->GetCount() - m_WaitHeaderPES;
    BYTE* start = m_pBuild->GetData() + m_WaitHeaderPES;
    
    if (AvailablePESlength < 9)
    {
      LogDebug("demux:vid Incomplete PES ( Avail %d )", AvailablePESlength);    
      return;
    }  
    
    if ((start[0]!=0) || (start[1]!=0) || (start[2]!=1))
    {
      LogDebug("Pes 0-0-1 fail");
      m_VideoValidPES=false;
      m_pBuild->rtStart = Packet::INVALID_TIME;
      m_pBuild->nClipNumber = -21;
      m_pBuild->nPlaylist = -21;

      m_WaitHeaderPES = -1;
    }   
    else
    {
      if (AvailablePESlength < 9 + start[8])
      {
        LogDebug("demux:vid Incomplete PES ( Avail %d/%d )", AvailablePESlength, AvailablePESlength+9+start[8]);    
        return;
      }
      else
      { // full PES header is available.
        CPcr pts;
        CPcr dts;
      
        m_VideoValidPES = true;
        if (CPcr::DecodeFromPesHeader(start, 0, pts, dts))
        {
          CPcr correctedPts;
          correctedPts.PcrReferenceBase = pts.PcrReferenceBase + m_rtVideoOffset;
          correctedPts.IsValid = true;

//          LogDebug("demux: vid last pts: %6.3f pts %6.3f corr %6.3f clip: %d playlist: %d", m_lastVideoPTS.ToClock(), pts.ToClock(), correctedPts.ToClock(), m_nVideoClip, m_nVideoPl);

          m_lastVideoPTS = pts;
          //pts = correctedPts;
        }

        m_lastStart -= 9 + start[8];
        m_pBuild->RemoveAt(m_WaitHeaderPES, 9 + start[8]);
        
        m_pBuild->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        m_pBuild->rtStop = m_pBuild->rtStart + 1;
        
        m_pBuild->nClipNumber = m_nVideoClip;
        m_pBuild->nPlaylist = m_nVideoPl;
        LogDebug("time %I64d (%d,%d)",m_pBuild->rtStart, m_nVideoPl, m_nVideoClip);

        m_WaitHeaderPES = -1;
      }
    }
  }//waitPESheader

  if (m_pBuild->GetCount() && m_pids.videoPids[0].VideoServiceType != SERVICE_TYPE_VIDEO_VC1)
  {
    FillVideoH264PESPacket(header, m_pBuild);
    m_pBuild.Free();
  }
}

void CDeMultiplexer::FillVideoMPEG2(CTsHeader& header, byte* tsPacket)
{                                                
  static const double frame_rate[16]={1.0/25.0,       1001.0/24000.0, 1.0/24.0, 1.0/25.0,
                                    1001.0/30000.0, 1.0/30.0,       1.0/50.0, 1001.0/60000.0,
                                    1.0/60.0,       1.0/25.0,       1.0/25.0, 1.0/25.0,
                                    1.0/25.0,       1.0/25.0,       1.0/25.0, 1.0/25.0 };
  static const char tc[]="XIPBXXXX";
    
  int headerlen = header.PayLoadStart;

  CPcr pts;
  CPcr dts;

  if(!m_p)
  {
    m_p.Attach(new Packet());
    m_p->bDiscontinuity = false;
    m_p->rtStart = Packet::INVALID_TIME;
    m_lastStart = 0;
    m_bInBlock=false;
  }
  
  if (header.PayloadUnitStart)
  {
    m_WaitHeaderPES = m_p->GetCount();
    m_mVideoValidPES = m_VideoValidPES;
    //LogDebug("demux::FillVideo PayLoad Unit Start");
  }
  
  CAutoPtr<Packet> p(new Packet());
  
  if (headerlen < 188)
  {            
    int dataLen = 188-headerlen;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen],dataLen);

    m_p->Append(*p);
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
    
    if ((start[0]!=0) || (start[1]!=0) || (start[2]!=1))
    {
      LogDebug("Pes 0-0-1 fail");
      m_VideoValidPES=false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_WaitHeaderPES = -1;
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
      
//      m_VideoValidPES=true ;
        if (CPcr::DecodeFromPesHeader(start, 0, pts, dts))
        {
          CPcr correctedPts;
          correctedPts.PcrReferenceBase = pts.PcrReferenceBase + m_rtVideoOffset;
          correctedPts.IsValid = true;

//          LogDebug("demux: vid last pts: %6.3f pts %6.3f corr %6.3f clip: %d playlist: %d", 
//            m_lastVideoPTS.ToClock(), pts.ToClock(), correctedPts.ToClock(), m_nVideoClip, m_nVideoPl);

          m_lastVideoPTS = pts;
          //pts = correctedPts;
          m_VideoPts = pts;
        }

        m_lastStart -= 9 + start[8];
        m_p->RemoveAt(m_WaitHeaderPES, 9 + start[8]);

        m_WaitHeaderPES = -1;
		
        m_p->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        m_p->rtStop = m_p->rtStart + 1;

        m_p->nClipNumber = m_nVideoClip;
        m_p->nPlaylist = m_nVideoPl;
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
          if (m_VideoPts.IsValid) m_CurrentVideoPts = m_VideoPts;
          m_VideoPts.IsValid = false;
          m_bInBlock = true;
        }
        break;
      }
      start++;
    }

    if(start <= end - 4)
    {
      BYTE* next = start + 1;
      if (next < m_p->GetData() + m_lastStart)
      {
        next = m_p->GetData() + m_lastStart;
      }

      while(next <= end - 4 && ((*(DWORD*)next & 0xFFFFFFFF) != 0xb3010000) && ((*(DWORD*)next & 0xFFFFFFFF) != 0x00010000)) next++;

      if(next >= end - 4)
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
//          LogDebug("demux::FillVideo Frame length : %d %x %x", size, *(DWORD*)start, *(DWORD*)next);
        
          if (m_VideoValidPES)
          {
            CAutoPtr<Packet> packet = m_pl.RemoveHead();
            Packet* p = packet.Detach();
//            LogDebug("Output Type: %x %d", *(DWORD*)p->GetData(),p->GetCount());
    
            while(m_pl.GetCount())
            {
              CAutoPtr<Packet> p2 = m_pl.RemoveHead();
//              LogDebug("Output Type: %x %d", *(DWORD*)p2->GetData(),p2->GetCount());
              p->Append(*p2);
            }

//            LogDebug("frame len %d decoded PTS %f (framerate %f), %c(%d)", p->GetCount(), m_CurrentVideoPts.IsValid ? (float)m_CurrentVideoPts.ToClock() : 0.0f,(float)m_curFrameRate,frame_type,frame_count);
    
            if (m_filter.GetVideoPin()->IsConnected())
            {
              if (m_CurrentVideoPts.IsValid)
              {                                                     // Timestamp Ok.
                m_LastValidFrameCount = frame_count;
                m_LastValidFramePts=m_CurrentVideoPts;
              }
              else
              {                    
                if (m_LastValidFrameCount >= 0)                       // No timestamp, but we've latest GOP timestamp.
                {
                  double d = m_LastValidFramePts.ToClock() + (frame_count-m_LastValidFrameCount) * m_curFrameRate ;
                  m_CurrentVideoPts.FromClock(d);                   // Rebuild it from 1st frame in GOP timestamp.
                  m_CurrentVideoPts.IsValid = true;
                }
              }
              p->rtStart = CONVERT_90KHz_DS(m_CurrentVideoPts.PcrReferenceBase);
              p->rtStop = p->rtStart + 1;
              p->nClipNumber = m_p->nClipNumber;
              p->nPlaylist = m_p->nPlaylist;

              if (m_bSetVideoDiscontinuity)
              {
                m_bSetVideoDiscontinuity=false;
                p->bDiscontinuity = true;
              }
              
              // ownership is transfered to vector
              //CorrectPacketPlaylist(p, SUPERCEEDED_VIDEO);
              //m_vecVideoBuffers.push_back(p);
              m_playlistManager->SubmitVideoPacket(p);
            }
            m_CurrentVideoPts.IsValid = false;

            CheckVideoFormat(p);
          }  
          m_VideoValidPES = true;                                    // We've just completed a frame, set flag until problem clears it 
          m_pl.RemoveAll();                                        
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
  if (!m_filter.GetSubtitlePin()->IsConnected()) return;
  if (m_iSubtitleStream < 0 || m_iSubtitleStream >= m_subtitleStreams.size()) return;

  IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
  if (pDVBSubtitleFilter)
  {
    // If current subtitle PID has changed notify the DVB sub filter
    if (m_subtitleStreams[m_iSubtitleStream].pid > 0 &&
        m_subtitleStreams[m_iSubtitleStream].pid != m_currentSubtitlePid)
    {
      pDVBSubtitleFilter->SetSubtitlePid(m_subtitleStreams[m_iSubtitleStream].pid);
      pDVBSubtitleFilter->SetFirstPcr(0);
      m_currentSubtitlePid = m_subtitleStreams[m_iSubtitleStream].pid;

      // TODO - this should be set based on the selected subtitle stream, but its not likely that
      // same TS / MT2S file would contain both DVB and Blu-ray subtitles
      if( m_pids.subtitlePids.size() > 0 && m_pids.subtitlePids[0].SubtitleServiceType == 0x90)
      {
        pDVBSubtitleFilter->SetHDMV(true);
      }
      else
      {
        pDVBSubtitleFilter->SetHDMV(false);
      }
    }

    if ((m_rtSubtitleOffset != m_rtNewOffset || m_bForceUpdateSubtitleOffset) && m_filter.m_bStreamCompensated)
    {
      m_bForceUpdateSubtitleOffset = false;

      m_rtSubtitleOffset = m_rtNewOffset;
      CRefTime refTime = -m_rtSubtitleOffset * 1000 / 9;

      refTime -= m_filter.m_rtCompensation.m_time;
        
      LogDebug("demux: Set subtitle compensation %03.3f (overal comp: %03.3f)", 
        refTime.Millisecs() / 1000.0f, m_filter.m_rtCompensation.m_time / 10000000.0f);
      pDVBSubtitleFilter->SetTimeCompensation(refTime);
    }    
  }

  if (m_currentSubtitlePid == 0 || m_currentSubtitlePid != header.Pid) return;
  if (header.AdaptionFieldOnly()) return;

  CAutoLock lock (&m_sectionSubtitle);
  if (!header.AdaptionFieldOnly())
  {
    if (header.PayloadUnitStart)
    {
      m_subtitlePcr = m_streamPcr;
    }
    if (m_vecSubtitleBuffers.size() > MAX_BUF_SIZE)
    {
      ivecSBuffers it = m_vecSubtitleBuffers.begin();
      Packet* subtitleBuffer = *it;
      delete subtitleBuffer;
      m_vecSubtitleBuffers.erase(it);
    }

    m_pCurrentSubtitleBuffer->rtStart = CONVERT_90KHz_DS(m_subtitlePcr.PcrReferenceBase);
    m_pCurrentSubtitleBuffer->SetCount(m_pCurrentSubtitleBuffer->GetDataSize() + 188);
    memcpy(m_pCurrentSubtitleBuffer->GetData() + m_pCurrentSubtitleBuffer->GetDataSize() - 188, tsPacket, 188);

    m_pCurrentSubtitleBuffer->nClipNumber = m_nSubtitleClip;
    m_pCurrentSubtitleBuffer->nPlaylist = m_nSubtitlePl;

    m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);

    m_pCurrentSubtitleBuffer = new Packet();
  }
}


/// This method gets called-back from the pat parser when a new PAT/PMT/SDT has been received
/// In this method we check if any audio/video/subtitle pid or format has changed
/// If not, we simply return
/// If something has changed we ask the MP to rebuild the graph
void CDeMultiplexer::OnNewChannel(CChannelInfo& info)
{
  CAutoLock lockVid(&m_sectionVideo);
  CAutoLock lockAud(&m_sectionAudio);
  CAutoLock lockSub(&m_sectionSubtitle);

  CPidTable pids = info.PidTable;

  if (info.PatVersion != m_iPatVersion)
  {
    LogDebug("OnNewChannel pat version:%d->%d", m_iPatVersion, info.PatVersion);
    m_iPatVersion = info.PatVersion;
    m_bSetAudioDiscontinuity = true;
    m_bSetVideoDiscontinuity = true;
    //Flush();
  }
  else
  {
    if (m_pids == pids)
    { 
      return;
    }
  }

  CAutoLock lock (&m_sectionMediaChanging);

  // Remember the old audio & video formats
  int oldVideoServiceType(-1);
  if (m_pids.videoPids.size() > 0)
  {
    oldVideoServiceType = m_pids.videoPids[0].VideoServiceType;
  }

  CPidTable tmpPids; // This is only for logging
  tmpPids = m_pids = pids;
  LogDebug("PAT/PMT/SDT changed");

  IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
  if( pDVBSubtitleFilter )
  {
    // Make sure that subtitle cache is reset ( in filter & MP )
    pDVBSubtitleFilter->NotifyChannelChange();
  }

  m_audioStreams.clear();
  m_audioStreamsToBeParsed = 0;
  m_mAudioMediaTypes.clear();

  BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();  

  for (unsigned int i(0); i < m_pids.audioPids.size(); i++)
  {
    struct stAudioStream audio;
    audio.pid = m_pids.audioPids[i].Pid;

    if (clip)
    {
      for (int j(0); j < clip->audio_stream_count; j++)
      {
        if (clip->audio_streams[j].pid == audio.pid)
        {
          tmpPids.audioPids[i].Lang[0] = audio.language[0] = clip->audio_streams[j].lang[0];
          tmpPids.audioPids[i].Lang[1] = audio.language[1] = clip->audio_streams[j].lang[1];
          tmpPids.audioPids[i].Lang[2] = audio.language[2] = clip->audio_streams[j].lang[2];
          tmpPids.audioPids[i].Lang[3] = audio.language[3] = clip->audio_streams[j].lang[3];
        }
      }

      if (clip->audio_stream_count == 0)
      {
        tmpPids.audioPids[i].Lang[0] = audio.language[0] = 'U';
        tmpPids.audioPids[i].Lang[1] = audio.language[1] = 'N';
        tmpPids.audioPids[i].Lang[2] = audio.language[2] = 'K';
        tmpPids.audioPids[i].Lang[3] = audio.language[3] = 0;
      }
    }
    audio.audioType = m_pids.audioPids[i].AudioServiceType;

    if (audio.audioType == SERVICE_TYPE_AUDIO_LPCM)
    {
      m_audioStreamsToBeParsed++;
    }

    m_audioStreams.push_back(audio);
  }

  m_subtitleStreams.clear();
  
  for (unsigned int i(0); i < m_pids.subtitlePids.size(); i++)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid = m_pids.subtitlePids[i].Pid;

    if (clip)
    {
      for (int j(0); j < clip->pg_stream_count; j++)
      {
        if (clip->pg_streams[j].pid == subtitle.pid)
        {
          tmpPids.subtitlePids[i].Lang[0] = subtitle.language[0] = clip->pg_streams[j].lang[0];
          tmpPids.subtitlePids[i].Lang[1] = subtitle.language[1] = clip->pg_streams[j].lang[1];
          tmpPids.subtitlePids[i].Lang[2] = subtitle.language[2] = clip->pg_streams[j].lang[2];
          tmpPids.subtitlePids[i].Lang[3] = subtitle.language[3] = clip->pg_streams[j].lang[3];
        }
      }
    }
    m_subtitleStreams.push_back(subtitle);
  }

  tmpPids.LogPIDs();

  bool changed = false;
  bool videoChanged = false;
  
  // Did the video format change?
  if (m_pids.videoPids.size() > 0 && oldVideoServiceType != m_pids.videoPids[0].VideoServiceType)
  {
    if (m_filter.GetVideoPin()->IsConnected())
    {
      changed = true;
      videoChanged = true;
    }
  }

  m_iAudioStream = 0;

  LogDebug ("Setting initial audio index to : %i", m_iAudioStream);

  // Get the new audio format
  int newAudioStreamType = SERVICE_TYPE_NO_AUDIO;
  if (m_iAudioStream >= 0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType = m_audioStreams[m_iAudioStream].audioType;
  }

  // Did the audio format change?
  if (m_AudioStreamType != newAudioStreamType)
  {
    changed = true;
  }

  // Did audio/video format change?
  if (changed)
  {
    // If we have a video stream and it's format changed, let the mpeg parser trigger the OnMediaTypeChanged
    if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid > 0x1 && videoChanged)  
    {
      LogDebug("demux: Video type changed - m_bRebuildOnVideoChange = true");
      //m_bRebuildOnVideoChange = true;

      //SetMediaChanging(true);
    }
    else
    {
      if ((m_AudioStreamType != SERVICE_TYPE_AUDIO_NOT_INIT) && 
          (m_AudioStreamType != newAudioStreamType))
      {
        LogDebug("demux: Audio media types changed. Trigger OnMediaTypeChanged()");
            
        if (m_audioStreamsToBeParsed > 0)
        {
          LogDebug("demux: m_bDelayedAudioTypeChange = true;");  
          //m_bDelayedAudioTypeChange = true;
        }
        else
        {
          /*
          m_filter.OnMediaTypeChanged(1);
          m_bDelayedAudioTypeChange = false;
          m_AudioStreamType = newAudioStreamType;

          SetMediaChanging(true);
          */
        }
      }
    }
  }

  m_AudioStreamType = newAudioStreamType;

  LogDebug("New Audio %d", m_AudioStreamType);

  if (pSubUpdateCallback)
  {
    int bitmap_index = -1;
    (*pSubUpdateCallback)(m_subtitleStreams.size(), (m_subtitleStreams.size() > 0 ? &m_subtitleStreams[0] : NULL), &bitmap_index);
    if (bitmap_index >= 0)
    {
      SetSubtitleStream(bitmap_index);
    }
  }
}

///Returns whether the demuxer is allowed to block in GetAudio() or not
bool CDeMultiplexer::HoldAudio()
{
  return m_bHoldAudio;
}

///Sets whether the demuxer may block in GetAudio() or not
void CDeMultiplexer::SetHoldAudio(bool onOff)
{
  LogDebug("demux:set hold audio:%d", onOff);
  m_bHoldAudio = onOff;
}

///Returns whether the demuxer is allowed to block in GetVideo() or not
bool CDeMultiplexer::HoldVideo()
{
  return m_bHoldVideo;
}

///Sets whether the demuxer may block in GetVideo() or not
void CDeMultiplexer::SetHoldVideo(bool onOff)
{
  LogDebug("demux:set hold video:%d", onOff);
  m_bHoldVideo = onOff;
}

///Returns whether the demuxer is allowed to block in GetSubtitle() or not
bool CDeMultiplexer::HoldSubtitle()
{
  return m_bHoldSubtitle;
}

///Sets whether the demuxer may block in GetSubtitle() or not
void CDeMultiplexer::SetHoldSubtitle(bool onOff)
{
  LogDebug("demux:set hold subtitle:%d", onOff);
  m_bHoldSubtitle = onOff;
}

void CDeMultiplexer::SetMediaChanging(bool onOff)
{
  CAutoLock lock (&m_sectionMediaChanging);
  LogDebug("demux:Wait for media format change:%d", onOff);
  m_bWaitForMediaChange = onOff;
  m_tWaitForMediaChange = GetTickCount();
}

bool CDeMultiplexer::IsMediaChanging()
{
  CAutoLock lock (&m_sectionMediaChanging);
  if (!m_bWaitForMediaChange) return false;
  else
  {
    /*if (GetTickCount() - m_tWaitForMediaChange > mediaChangeTimeout)
    {
      m_bWaitForMediaChange = false;
      LogDebug("demux: Alert: Wait for Media change cancelled on %03.2f secs timeout", mediaChangeTimeout / 1000.0);
      return false;
    }*/
  }
  return true;
}
