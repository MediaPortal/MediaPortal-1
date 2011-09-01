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
#include <afx.h>
#include <streams.h>
#include "demultiplexer.h"
#include <cassert>
#include <bluray.h>
#include "..\..\shared\adaptionfield.h"
#include "bdreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "subtitlePin.h"
#include "..\..\DVBSubtitle3\Source\IDVBSub.h"
#include "mediaFormats.h"
#include "h264nalu.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define NO_STREAM -1
#define PACKET_GRANULARITY 80000
#define CONVERT_90KHz_DS(x) (REFERENCE_TIME)(x * (1000.0 / 9.0))
#define CONVERT_DS_90KHz(x) (REFERENCE_TIME)(x / (1000.0 / 9.0))

const double mediaChangeTimeout = 5000;

extern void LogDebug(const char *fmt, ...);

CDeMultiplexer::CDeMultiplexer(CBDReaderFilter& filter) : m_filter(filter)
{
  m_filter.lib.SetEventObserver(this);
  m_pCurrentVideoBuffer = NULL;
  m_pCurrentAudioBuffer = new Packet();
  m_pCurrentSubtitleBuffer = new Packet();
  m_iAudioStream = 0;
  m_AudioStreamType = -1;
  m_iSubtitleStream = 0;
  m_audioPid = 0;
  m_currentSubtitlePid = 0;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
  m_bHoldSubtitle = false;
  m_bShuttingDown = false;
  m_iAudioIdx = -1;
  m_bSetAudioDiscontinuity = false;
  m_bSetVideoDiscontinuity = false;
  m_pSubUpdateCallback = NULL;
  m_lastVideoPTS.IsValid = false;
  m_lastAudioPTS.IsValid = false;
  SetMediaChanging(false);

  m_WaitHeaderPES = -1 ;
  m_videoParser = new StreamParser();
  m_audioParser = new StreamParser();

  m_bReadFailed = false;

  m_bUpdateSubtitleOffset = false;

  m_fHasAccessUnitDelimiters = false;

  m_bAudioPlSeen = false;
  m_bVideoPlSeen = false;
  m_bAudioRequiresRebuild = false;
  m_bVideoRequiresRebuild = false;
  m_nActiveAudioPlaylist = -1;
  m_playlistManager = new CPlaylistManager();
  m_loopLastSearch = 1;
  m_bDiscontinuousClip = false;
  m_bVideoFormatParsed = false;

  m_videoServiceType = NO_STREAM;
  m_nVideoPid = -1;

  m_nMPEG2LastPlaylist = -1;
  m_nMPEG2LastClip = -1;
}

CDeMultiplexer::~CDeMultiplexer()
{
  m_filter.lib.RemoveEventObserver(this);
  m_bShuttingDown = true;

  delete m_playlistManager;

  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;
  delete m_videoParser;
  delete m_audioParser;

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
  m_rtOffset = 0;

  m_bUpdateSubtitleOffset = false;

  m_nClip = pDebugMark;
  m_nPlaylist = pDebugMark;
}

int CDeMultiplexer::GetVideoServiceType()
{
  return m_videoServiceType;
}

/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed
bool CDeMultiplexer::SetAudioStream(int stream)
{
  LogDebug("SetAudioStream : %d - TODO - not implemented!", stream);
  if (stream < 0 || stream >= (int)m_audioStreams.size())
    return S_FALSE;

  return true; // TODO audio stream switching

  m_iAudioStream = stream;
  /*
  BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();
  if (clip)
  {
    if (clip->audio_stream_count < stream)
    {
      m_AudioStreamType = clip->audio_streams[stream].coding_type;
    }
    else
    {
      LogDebug("demux: SetAudioStream - requested stream > stream count");
    }
  }
  else
  {
    LogDebug("demux: SetAudioStream - failed to get clip info!");
    return;
  }*/

  // Get the new audio stream type
  int newAudioStreamType = BLURAY_STREAM_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream >= 0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType = m_audioStreams[m_iAudioStream].audioType;
  }

  LogDebug("Old Audio %d, New Audio %d", m_AudioStreamType, newAudioStreamType);

  if ((m_AudioStreamType != -1) && 
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
    //m_filter.GetAudioPin()->SetDiscontinuity(true);
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
  if (m_AudioStreamType == NO_STREAM)
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
  if (m_AudioStreamType == NO_STREAM)
  {
    pmt.InitMediaType();
    pmt.SetType(&MEDIATYPE_Audio);
    pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
    pmt.SetSampleSize(1);
    pmt.SetTemporalCompression(FALSE);
    pmt.SetVariableSize();
    pmt.SetFormatType(&FORMAT_WaveFormatEx);
    pmt.SetFormat(AC3AudioFormat, sizeof(AC3AudioFormat));
  }
  else
  {
    pmt = m_audioParser->pmt;
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
  m_pSubUpdateCallback = cb;
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
  if (m_videoParser)
  {
    pmt = m_videoParser->pmt;
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
        videoBuffer->nClipNumber, m_nClip, videoBuffer->nPlaylist, m_nPlaylist, videoBuffer->rtStart / 10000000.0);
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
      subtitleBuffer->nClipNumber, m_nClip, subtitleBuffer->nPlaylist, m_nPlaylist, subtitleBuffer->rtStart / 10000000.0);
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
}


/// Starts the demuxer
/// This method will read the file until we found the pat/sdt
/// with all the audio/video pids
HRESULT CDeMultiplexer::Start()
{
  m_bReadFailed = false;
  m_bStarting = true;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
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

    if (dwBytesProcessed > INITIAL_READ_SIZE)
    {
      m_bReadFailed = true;
      break;
    }

    if (!m_videoParser->basicVideoInfo.isValid ||
        m_videoParser->pmt.formattype == GUID_NULL ||
        (m_AudioStreamType > NO_STREAM && m_audioParser->pmt.formattype == GUID_NULL))
    {
      dwBytesProcessed += BytesRead;
      continue;
    }

    // Seek to start - reset the libbluray reading position
    m_filter.lib.Seek(0);
    Flush();
    m_bStarting = false;

    CMediaType pmt;
    GetAudioStreamType(m_iAudioStream, pmt);
    m_filter.GetAudioPin()->SetInitialMediaType(&pmt);

    GetVideoStreamType(pmt);
    m_filter.GetVideoPin()->SetInitialMediaType(&pmt);

    return S_OK;
  }
  
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

  dwReadBytes = m_filter.lib.Read(m_readBuffer, sizeof(m_readBuffer), pause, false);

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
      m_bDiscontinuousClip = true;
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
      m_nPlaylist = pEv.param;
      break;

    case BD_EVENT_CHAPTER:
    {
      BLURAY_TITLE_INFO* title = m_filter.lib.GetTitleInfo(m_nPlaylist);
      
      if (title)
      {
        UINT64 start = 0;
        if (title->chapter_count > 0 && title->chapter_count >= pEv.param)
        {
          start = title->chapters[pEv.param - 1].start;
        }
        LogDebug("demux: New chapter %d - start: %6.3f", pEv.param, CONVERT_90KHz_DS(start) / 10000000.0);
        
        m_filter.lib.FreeTitleInfo(title);
      }
      else
      {
        LogDebug("demux: New chapter %d - title info N/A", pEv.param); 
      }
      break;
    }

    case BD_EVENT_PLAYITEM:
      LogDebug("demux: New playitem %d", pEv.param);
      
      m_bVideoFormatParsed = false;
      
      UINT64 clipStart = 0, clipIn = 0, bytePos = 0, duration = 0;
      int ret = m_filter.lib.GetClipInfo(pEv.param, &clipStart, &clipIn, &bytePos, &duration);
      if (ret) 
      {
        REFERENCE_TIME rtOldOffset = m_rtOffset;

        m_rtOffset = clipStart - clipIn;
        m_nClip = pEv.param;

        if (m_rtOffset != rtOldOffset)
        {
          m_bUpdateSubtitleOffset = true;
        }

        BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();
        if (clip)
        {
          ParseVideoStream(clip);
          ParseAudioStreams(clip);
          ParseSubtitleStreams(clip);
        }
        else
        {
          LogDebug("demux: HandleBDEvent - failed to get clip info!");
          return;
        }

        if (!m_bStarting)
        {
          REFERENCE_TIME clipOffset = m_rtOffset * -1;

          bool interrupted = m_playlistManager->CreateNewPlaylistClip(m_nPlaylist, m_nClip, clip->audio_stream_count > 0, 
            CONVERT_90KHz_DS(clipIn), CONVERT_90KHz_DS(clipOffset), CONVERT_90KHz_DS(duration), m_bDiscontinuousClip);
          m_bDiscontinuousClip = false;
        
          if (interrupted)
          {
            LogDebug("demux: current clip was interrupted - triggering flush");

            SetHoldVideo(true);
            SetHoldAudio(true);
            SetHoldSubtitle(true);

            m_filter.IssueCommand(FLUSH, 0);
          }
        }
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

  if (header.Pid == 0) return;
  if (header.TScrambling) return;
  if (header.TransportError) return;

  FillSubtitle(header, tsPacket);
  FillAudio(header, tsPacket);
  FillVideo(header, tsPacket);
}


/// This method will check if the tspacket is an audio packet
/// ifso, it decodes the PES audio packet and stores it in the audio buffers
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  //LogDebug("FillAudio - audio PID %d", m_audioPid );

  if (m_iAudioStream < 0 || m_iAudioStream >= m_audioStreams.size()) return;

  m_audioPid = m_audioStreams[m_iAudioStream].pid;
  if (m_audioPid == 0 || m_audioPid != header.Pid) return;
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

      if (m_pCurrentAudioBuffer)
      {
        delete m_pCurrentAudioBuffer;
        m_pCurrentAudioBuffer = NULL;
      }

      m_pCurrentAudioBuffer = new Packet();

      if (CPcr::DecodeFromPesHeader(p, 0, pts, dts))
      {
        CPcr correctedPts;
        correctedPts.PcrReferenceBase = pts.PcrReferenceBase + m_rtOffset;
        correctedPts.IsValid = true;

#ifdef LOG_DEMUXER_AUDIO_SAMPLES
          LogDebug("demux: aud last pts: %6.3f pts %6.3f corr %6.3f clip: %d playlist: %d", 
            m_lastAudioPTS.ToClock(), pts.ToClock(), correctedPts.ToClock(), m_nClip, m_nPlaylist);
#endif

        m_lastAudioPTS = pts;
        m_pCurrentAudioBuffer->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        m_pCurrentAudioBuffer->rtStop = m_pCurrentAudioBuffer->rtStart + 1;
          
        m_pCurrentAudioBuffer->nClipNumber = m_nClip;
        m_pCurrentAudioBuffer->nPlaylist = m_nPlaylist;

        REFERENCE_TIME Ref = m_pCurrentAudioBuffer->rtStart;

        BYTE pesHeader[256];

        int pos = header.PayLoadStart;
        int pesHeaderLen = tsPacket[pos+8] + 9;
        memcpy(pesHeader, &tsPacket[pos], pesHeaderLen);
        pos += pesHeaderLen;
        
        // Calculate expected payload length
        m_nAudioPesLenght = (pesHeader[4] << 8 ) + pesHeader[5] - (pesHeaderLen - 6);

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

  if (m_AudioValidPES && !packetProcessed && m_pCurrentAudioBuffer)
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
      ParseAudioFormat(m_pCurrentAudioBuffer);

      if (!m_bStarting)
      {
        m_playlistManager->SubmitAudioPacket(m_pCurrentAudioBuffer);
      }
      else
      {
        delete m_pCurrentAudioBuffer;
      }
      m_pCurrentAudioBuffer = NULL;
      m_nAudioPesLenght = 0;
    }
  }
}

/// This method will check if the tspacket is an video packet
void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{
  if (m_nVideoPid == -1) return;
  if (header.Pid != m_nVideoPid) return;

  if (header.AdaptionFieldOnly()) return;

  CAutoLock lock (&m_sectionVideo);

  if (m_bShuttingDown) return;

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
    timestamp.IsValid = true;
  }
    
  if (m_filter.GetVideoPin()->IsConnected())
  {
    if (p->rtStart != Packet::INVALID_TIME)
    {
      m_LastValidFrameCount++;
    }
    
    if (m_bSetVideoDiscontinuity)
    {
      m_bSetVideoDiscontinuity = false;
      p->bDiscontinuity = true;
    }
    ParseVideoFormat(p);
    if (!m_bStarting)
    {
      m_playlistManager->SubmitVideoPacket(p);
    }
    else
    {
      delete p;
      p = NULL;
    }
  }
  else
  {
    ParseVideoFormat(p);
    delete p;
    p = NULL;
  }
}

bool CDeMultiplexer::ParseVideoFormat(Packet* p)
{
  if (!m_bVideoFormatParsed)
  {
    m_bVideoFormatParsed = m_videoParser->OnTsPacket(p->GetData(), p->GetCount(), m_videoServiceType);
    if (!m_bVideoFormatParsed)
    {
      LogDebug("demux: ParseVideoFormat - failed to parse video format!");
    }
    else
    {
      LogDebug("demux: ParseVideoFormat - succeeded");
      if (!m_bStarting)
      {
        m_playlistManager->SetPMT(CreateMediaType(&m_videoParser->pmt), m_nPlaylist, m_nClip);
      }
    }
  }

  return m_bVideoFormatParsed;
}

void CDeMultiplexer::ParseAudioFormat(Packet* p)
{
  m_audioParser->OnTsPacket(p->GetData(), p->GetCount(), m_AudioStreamType);
  p->pmt = CreateMediaType(&m_audioParser->pmt);
}

void CDeMultiplexer::FillVideoH264PESPacket(CTsHeader& header, CAutoPtr<Packet> p)
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
      //LogDebug("PacketDelivery - %6.3f %d %d", p->rtStart / 10000000.0, p->nPlaylist, p->nClipNumber);
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
    m_pBuild->nClipNumber = m_nClip;
    m_pBuild->nPlaylist = m_nPlaylist;

    m_lastStart = 0;
  }
  
  if (header.PayloadUnitStart)
  {
#ifdef LOG_DEMUXER_VIDEO_SAMPLES    
    //LogDebug("demux: FillVideoH264 PayLoad Unit Start");
#endif
    
    m_WaitHeaderPES = m_pBuild->GetCount();
    m_mVideoValidPES = m_VideoValidPES;

    if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_VC1 && m_pBuild->GetCount() > 0)
    {
      FillVideoVC1PESPacket(header, m_pBuild);
      m_pBuild.Free();

      m_pBuild.Attach(new Packet());
      m_pBuild->bDiscontinuity = false;
      m_pBuild->rtStart = Packet::INVALID_TIME;
      m_pBuild->nClipNumber = m_nClip;
      m_pBuild->nPlaylist = m_nPlaylist;
      m_lastStart = 0;
      m_WaitHeaderPES = 0;
    }
  }
  
  CAutoPtr<Packet> p(new Packet());
  
  if (headerlen < 188)
  {            
    int dataLen = 188 - headerlen;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen], dataLen);

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
          correctedPts.PcrReferenceBase = pts.PcrReferenceBase + m_rtOffset;
          correctedPts.IsValid = true;

#ifdef LOG_DEMUXER_VIDEO_SAMPLES
          LogDebug("demux: vid last pts: %6.3f pts %6.3f corr %6.3f clip: %d playlist: %d", 
            m_lastVideoPTS.ToClock(), pts.ToClock(), correctedPts.ToClock(), m_nClip, m_nPlaylist);
#endif
          m_lastVideoPTS = pts;
          //pts = correctedPts;
        }

        m_lastStart -= 9 + start[8];
        m_pBuild->RemoveAt(m_WaitHeaderPES, 9 + start[8]);
        
        m_pBuild->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        m_pBuild->rtStop = m_pBuild->rtStart + 1;
        
        m_pBuild->nClipNumber = m_nClip;
        m_pBuild->nPlaylist = m_nPlaylist;

        m_WaitHeaderPES = -1;
      }
    }
  }//waitPESheader

  if (m_pBuild->GetCount() && m_videoServiceType != BLURAY_STREAM_TYPE_VIDEO_VC1)
  {
    FillVideoH264PESPacket(header, m_pBuild);
    m_pBuild.Free();
  }
}

void CDeMultiplexer::FillVideoMPEG2(CTsHeader& header, byte* tsPacket)
{                                                
  int headerlen = header.PayLoadStart;

  if (!m_p)
  {
    m_p.Attach(new Packet());
    m_p->SetCount(0, PACKET_GRANULARITY);
    m_p->bDiscontinuity = false;
    m_p->rtStart = Packet::INVALID_TIME;
    m_p->nPlaylist = m_nPlaylist;
    m_p->nClipNumber = m_nClip;
    m_lastStart = 0;
    m_bInBlock = false;
  }
  
  if (header.PayloadUnitStart)
  {
    m_WaitHeaderPES = m_p->GetCount();
    m_mVideoValidPES = m_VideoValidPES;

#ifdef LOG_DEMUXER_VIDEO_SAMPLES
    //LogDebug("demux FillVideoMPEG2 PayLoad Unit Start");
#endif
  }
  
  CAutoPtr<Packet> p(new Packet());
  
  if (headerlen < 188)
  {            
    int dataLen = 188 - headerlen;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen], dataLen);

    m_p->Append(*p);
  }
  else
    return;

  if (m_WaitHeaderPES >= 0)
  {
    int AvailablePESlength = m_p->GetCount() - m_WaitHeaderPES;
    BYTE* start = m_p->GetData() + m_WaitHeaderPES;
    
    if (AvailablePESlength < 9)
    {
      LogDebug("demux:vid Incomplete PES ( Avail %d )", AvailablePESlength);    
      return;
    }  
    
    if ((start[0]!=0) || (start[1]!=0) || (start[2]!=1))
    {
      LogDebug("Pes 0-0-1 fail");
      m_VideoValidPES = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_WaitHeaderPES = -1;
    }   
    else
    {
      if (AvailablePESlength < 9 + start[8])
      {
        LogDebug("demux:vid Incomplete PES ( Avail %d/%d )", AvailablePESlength, AvailablePESlength+9+start[8]) ;    
        return;
      }
      else
      { // full PES header is available.
        CPcr pts;
        CPcr dts;
      
        if (CPcr::DecodeFromPesHeader(start, 0, pts, dts))
        {
#ifdef LOG_DEMUXER_VIDEO_SAMPLES
          LogDebug("demux: vid last pts: %6.3f pts %6.3f clip: %d playlist: %d", 
            m_lastVideoPTS.ToClock(), pts.ToClock(), m_nClip, m_nPlaylist);
#endif

          m_lastVideoPTS = pts;
          m_VideoPts = pts;
        }

        m_WaitHeaderPES = -1;
		
        m_p->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        m_p->rtStop = m_p->rtStart + 1;

        /*
        if (m_nMPEG2LastPlaylist == -1)
          m_nMPEG2LastPlaylist = m_nPlaylist;

        if (m_nMPEG2LastClip == -1)
          m_nMPEG2LastClip = m_nClip;

        m_p->nPlaylist = m_nMPEG2LastPlaylist;
        m_p->nClipNumber = m_nMPEG2LastClip;
        m_nMPEG2LastPlaylist = m_nPlaylist;
        m_nMPEG2LastClip = m_nClip;
        */

        m_p->nPlaylist = m_nPlaylist;
        m_p->nClipNumber = m_nClip;

        if (m_p->GetCount())
        {
          if (m_VideoPts.IsValid) 
            m_CurrentVideoPts = m_VideoPts;  

          //m_p->rtStart = CONVERT_90KHz_DS(m_CurrentVideoPts.PcrReferenceBase);
          //m_p->rtStop = p->rtStart + 1;

          ParseVideoFormat(m_p);
          
          if (!m_bStarting)
          {
            Packet* p = new Packet();
            p->Append(*m_p);
            p->rtStart = m_p->rtStart;
            p->rtStop = m_p->rtStop;
            p->nClipNumber = m_p->nClipNumber;
            p->nPlaylist = m_p->nPlaylist;
            m_playlistManager->SubmitVideoPacket(p);
          }
        }

        m_p->RemoveAll();
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

      pDVBSubtitleFilter->SetHDMV(true);
    }

    if (m_bUpdateSubtitleOffset)
    {
      m_bUpdateSubtitleOffset = false;

      CRefTime refTime = -m_rtOffset * 1000 / 9;
        
      LogDebug("demux: Set subtitle compensation %03.3f", refTime.Millisecs() / 1000.0f);
      pDVBSubtitleFilter->SetTimeCompensation(refTime);
    }    
  }

  if (m_currentSubtitlePid == 0 || m_currentSubtitlePid != header.Pid) return;
  if (header.AdaptionFieldOnly()) return;

  CAutoLock lock (&m_sectionSubtitle);
  if (!header.AdaptionFieldOnly())
  {
    if (m_vecSubtitleBuffers.size() > MAX_BUF_SIZE)
    {
      ivecSBuffers it = m_vecSubtitleBuffers.begin();
      Packet* subtitleBuffer = *it;
      delete subtitleBuffer;
      m_vecSubtitleBuffers.erase(it);
    }

    m_pCurrentSubtitleBuffer->rtStart = 0;
    m_pCurrentSubtitleBuffer->SetCount(m_pCurrentSubtitleBuffer->GetDataSize() + 188);
    memcpy(m_pCurrentSubtitleBuffer->GetData() + m_pCurrentSubtitleBuffer->GetDataSize() - 188, tsPacket, 188);

    m_pCurrentSubtitleBuffer->nClipNumber = m_nClip;
    m_pCurrentSubtitleBuffer->nPlaylist = m_nPlaylist;

    m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);

    m_pCurrentSubtitleBuffer = new Packet();
  }
}

void CDeMultiplexer::ParseVideoStream(BLURAY_CLIP_INFO* clip)
{
  if (clip)
  {
    m_videoServiceType = clip->video_streams->coding_type;
    m_nVideoPid = clip->video_streams->pid;

    LogDebug("   Video    [%4d]     %s", m_nVideoPid, StreamFormatAsString(m_videoServiceType));
  }
}

void CDeMultiplexer::ParseAudioStreams(BLURAY_CLIP_INFO* clip)
{
  if (clip)
  {
    m_audioStreams.clear();

    // TODO - use correct stream on clip changes
    if (clip->audio_stream_count > m_iAudioStream && m_iAudioStream >= 0)
    {
      m_AudioStreamType = clip->audio_streams[m_iAudioStream].coding_type;
    }
    else if (clip->audio_stream_count > 0)
    {
      m_AudioStreamType = clip->audio_streams[0].coding_type;
      LogDebug("demux: ParseAudioStreams - requested stream > stream count");
    }
    else
    {
      m_AudioStreamType = NO_STREAM;

      stAudioStream audio;

      audio.language[0] = 'F';
      audio.language[1] = 'F';
      audio.language[2] = 'F';
      audio.language[3] = 0;

      audio.audioType = BLURAY_STREAM_TYPE_AUDIO_AC3;
      audio.pid = -1;

      LogDebug("   Audio    [%4d] %s %s (fake)", audio.pid, audio.language, StreamFormatAsString(audio.audioType));

      m_audioStreams.push_back(audio);
    }

    if (m_AudioStreamType != NO_STREAM)
    {
      for (int i(0); i < clip->audio_stream_count; i++)
      {
        stAudioStream audio;

        audio.language[0] = clip->audio_streams[i].lang[0];
        audio.language[1] = clip->audio_streams[i].lang[1];
        audio.language[2] = clip->audio_streams[i].lang[2];
        audio.language[3] = clip->audio_streams[i].lang[3];

        audio.audioType = clip->audio_streams[i].coding_type;
        audio.pid = clip->audio_streams[i].pid;

        LogDebug("   Audio    [%4d] %s %s", audio.pid, audio.language, StreamFormatAsString(audio.audioType));

        m_audioStreams.push_back(audio);
      }
    }
  }
}

void CDeMultiplexer::ParseSubtitleStreams(BLURAY_CLIP_INFO* clip)
{
  if (clip)
  {
    m_subtitleStreams.clear();

    for (int i(0); i < clip->pg_stream_count; i++)
    {
      stSubtitleStream subtitle;

      subtitle.language[0] = clip->pg_streams[i].lang[0];
      subtitle.language[1] = clip->pg_streams[i].lang[1];
      subtitle.language[2] = clip->pg_streams[i].lang[2];
      subtitle.language[3] = clip->pg_streams[i].lang[3];

      subtitle.subtitleType = clip->pg_streams[i].coding_type;
      subtitle.pid = clip->pg_streams[i].pid;

      LogDebug("   Subtitle [%4d] %s %s", subtitle.pid, subtitle.language, StreamFormatAsString(subtitle.subtitleType));

      m_subtitleStreams.push_back(subtitle);
    }

    // TODO - do not trigger on every clip change
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if (pDVBSubtitleFilter)
    {
      // Make sure that subtitle cache is reset (in filter & MP)
      pDVBSubtitleFilter->NotifyChannelChange();
    }

    if (m_pSubUpdateCallback)
    {
      int bitmap_index = -1;
      (*m_pSubUpdateCallback)(m_subtitleStreams.size(), (m_subtitleStreams.size() > 0 ? &m_subtitleStreams[0] : NULL), &bitmap_index);
      if (bitmap_index >= 0)
      {
        SetSubtitleStream(bitmap_index);
      }
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
  LogDebug("demux: set hold audio:%d", onOff);
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
  LogDebug("demux: set hold video:%d", onOff);
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
  LogDebug("demux: set hold subtitle:%d", onOff);
  m_bHoldSubtitle = onOff;
}

void CDeMultiplexer::SetMediaChanging(bool onOff)
{
  CAutoLock lock (&m_sectionMediaChanging);
  LogDebug("demux: Wait for media format change:%d", onOff);
  m_bWaitForMediaChange = onOff;
}

bool CDeMultiplexer::IsMediaChanging()
{
  CAutoLock lock (&m_sectionMediaChanging);
  return m_bWaitForMediaChange;
}

LPCTSTR CDeMultiplexer::StreamFormatAsString(int pStreamType)
{
	switch (pStreamType)
	{
	case BLURAY_STREAM_TYPE_VIDEO_MPEG1:
		return _T("MPEG1");
	case BLURAY_STREAM_TYPE_VIDEO_MPEG2:
		return _T("MPEG2");
	case BLURAY_STREAM_TYPE_AUDIO_MPEG1:
		return _T("MPEG1");
	case BLURAY_STREAM_TYPE_AUDIO_MPEG2:
		return _T("MPEG2");
	case BLURAY_STREAM_TYPE_VIDEO_H264:
		return _T("H264");
	case BLURAY_STREAM_TYPE_VIDEO_VC1:
		return _T("VC1");
	case BLURAY_STREAM_TYPE_AUDIO_LPCM:
		return _T("LPCM");
	case BLURAY_STREAM_TYPE_AUDIO_AC3:
		return _T("AC3");
	case BLURAY_STREAM_TYPE_AUDIO_DTS:
		return _T("DTS");
	case BLURAY_STREAM_TYPE_AUDIO_TRUHD:
		return _T("TrueHD");
	case BLURAY_STREAM_TYPE_AUDIO_AC3PLUS:
		return _T("AC3+");
	case BLURAY_STREAM_TYPE_AUDIO_DTSHD:
		return _T("DTS-HD");
	case BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER:
		return _T("DTS-HD Master");
  case 0x0f:
		return _T("AAC");
	case 0x11:
		return _T("LATM AAC");
	case BLURAY_STREAM_TYPE_SUB_PG:
		return _T("PGS");
	case BLURAY_STREAM_TYPE_SUB_IG:
		return _T("IG");
	case BLURAY_STREAM_TYPE_SUB_TEXT:
		return _T("Text");
	default:
		return _T("Unknown");
	}
}