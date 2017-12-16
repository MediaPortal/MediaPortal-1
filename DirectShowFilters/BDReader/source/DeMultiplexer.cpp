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
#include <afx.h>
#include <streams.h>
#include <wmcodecdsp.h>
#include "demultiplexer.h"
#include <bluray.h>
#include "..\..\shared\adaptionfield.h"
#include "bdreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "mediaFormats.h"
#include "h264nalu.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define NO_STREAM -1
#define PACKET_GRANULARITY 80000

extern void LogDebug(const char *fmt, ...);

CDeMultiplexer::CDeMultiplexer(CBDReaderFilter& filter) : m_filter(filter)
{
  m_filter.lib.SetEventObserver(this);
  m_pCurrentVideoBuffer = NULL;
  m_pCurrentAudioBuffer = new Packet();
  m_iAudioStream = 0;
  m_AudioStreamType = NO_STREAM;
  m_audioPid = 0;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
  m_bShuttingDown = false;
  m_iAudioIdx = -1;
  m_bRebuildOngoing = false;
  SetMediaChanging(false);

  m_WaitHeaderPES = -1 ;
  m_videoParser = new StreamParser();
  m_audioParser = new StreamParser();

  m_bReadFailed = false;
  m_bFlushBuffersOnPause = false;

  m_fHasAccessUnitDelimiters = false;

  m_eAudioClipSeen = new CAMEvent(true);
  m_bVideoClipSeen = false;
  m_bAudioRequiresRebuild = false;
  m_bVideoRequiresRebuild = false;
  m_playlistManager = new CPlaylistManager();
  m_loopLastSearch = 1;
  m_bStreamPaused = false;
  m_bAudioWaitForSeek = false;
  m_bVideoFormatParsed = false;
  m_bAudioFormatParsed = false;

  m_bAC3Substream = false;
  m_AudioValidPES = false;
  m_VideoValidPES = false;

  m_videoServiceType = NO_STREAM;
  m_nVideoPid = -1;

  m_bLibRequestedFlush = false;

  m_nClip = -1;
  m_nTitle = -1;
  m_nPlaylist = -1;

  m_nMPEG2LastPlaylist = -1;
  m_nMPEG2LastClip = -1;
  m_nMPEG2LastTitleDuration = -1;

  m_rtOffset = _I64_MAX;
  m_rtTitleDuration = 0;

  m_rtStallTime = 0;
  m_bTitleChanged = false;

  m_bAudioResetStreamPosition = false;
}

CDeMultiplexer::~CDeMultiplexer()
{
  m_filter.lib.RemoveEventObserver(this);
  m_bShuttingDown = true;

  delete m_eAudioClipSeen;

  delete m_playlistManager;

  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_videoParser;
  delete m_audioParser;

  m_pl.RemoveAll();

  m_audioStreams.clear();
}

int CDeMultiplexer::GetVideoServiceType()
{
  return m_videoServiceType;
}

/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed
bool CDeMultiplexer::SetAudioStream(int stream)
{
  CAutoLock lock (&m_sectionAudio);

  if (stream == m_iAudioStream)
  {
    LogDebug("demux: SetAudioStream : %d - no change", stream);
    return true;
  }

  if (stream < 0 || stream >= (int)m_audioStreams.size())
  {
    LogDebug("demux: SetAudioStream : %d - fail (size %d)", stream, m_audioStreams.size());
    return false;
  }

  // Get the new audio stream type
  int newAudioStreamType =  m_audioStreams[stream].audioType;   
  m_iAudioStream = stream;
  m_iAudioIdx = stream;

  if (m_AudioStreamType != newAudioStreamType)
  {
    LogDebug("demux: old %s new audio %s", StreamFormatAsString(m_AudioStreamType), StreamFormatAsString(newAudioStreamType));
		m_AudioStreamType = newAudioStreamType;
  }
  else
  {
    LogDebug("demux: no change in audio type (%s)", StreamFormatAsString(m_AudioStreamType));      
  }

  delete m_pCurrentAudioBuffer;
  m_pCurrentAudioBuffer = NULL;
  m_nAudioPesLenght = 0;

  m_bAudioFormatParsed = false;

  return true;
}

bool CDeMultiplexer::GetAudioStream(__int32 &audioIndex)
{
  audioIndex = m_iAudioStream;
  return true;
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
    return 0; // only fake audio stream is present (no need to tell it to player)
  }
  else
  {
    return m_audioStreams.size();
  }
}

void CDeMultiplexer::GetAudioStreamPMT(CMediaType& pmt)
{
  // Fake audio in use
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
    pmt = m_audioParser->pmt;
}

int CDeMultiplexer::GetAudioStreamType(int stream)
{
  if (stream < 0 || stream >= (int)m_audioStreams.size())
    return 0;
  else
    return m_audioStreams[stream].audioType;
}

int CDeMultiplexer::GetAudioChannelCount(int stream)
{
  if (stream < 0 || stream >= (int)m_audioStreams.size())
    return 0;
  else
    return m_audioStreams[stream].audioChannelCount;
}

int CDeMultiplexer::GetCurrentAudioStreamType()
{
  if (m_iAudioStream >= m_audioStreams.size())
    return 0;
  else
    return m_audioStreams[m_iAudioStream].audioType;
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

void CDeMultiplexer::GetSubtitleStreamPMT(CMediaType& pmt)
{
  pmt.InitMediaType();
  pmt.SetType(&MEDIATYPE_Text);
  pmt.SetSubtype(&MEDIATYPE_Subtitle);
}

void CDeMultiplexer::GetVideoStreamPMT(CMediaType &pmt)
{
  if (m_videoParser)
    pmt = m_videoParser->pmt;
}

REFERENCE_TIME CDeMultiplexer::TitleDuration()
{
  return m_rtTitleDuration;
}

void CDeMultiplexer::FlushVideo()
{
  LogDebug("demux:flush video");
  CAutoLock lock (&m_sectionVideo);

  delete m_pCurrentVideoBuffer;
  m_pCurrentVideoBuffer = NULL;

  m_p.Free();
  m_pBuild.Free();
  m_lastStart = 0;
  m_loopLastSearch = 1;
  m_pl.RemoveAll();
  m_fHasAccessUnitDelimiters = false;

  m_VideoValidPES = true;
  m_WaitHeaderPES = -1;
}

void CDeMultiplexer::FlushAudio()
{
  LogDebug("demux:flush audio");
  CAutoLock lock (&m_sectionAudio);

  m_AudioValidPES = false;
  m_bAC3Substream = false;

  delete m_pCurrentAudioBuffer;
  m_pCurrentAudioBuffer = new Packet();
}

void CDeMultiplexer::Flush(bool bClearclips)
{
  LogDebug("demux:flushing");

  SetHoldAudio(true);
  SetHoldVideo(true);

  FlushPESBuffers(true, false);

  if (bClearclips)
    m_playlistManager->ClearClips();

  FlushAudio();
  FlushVideo();

  SetHoldAudio(false);
  SetHoldVideo(false);
}

Packet* CDeMultiplexer::GetVideo()
{
  if (HoldVideo())
    return NULL;

  bool allowBuffering = m_playlistManager->AllowBuffering();

  while (!m_playlistManager->HasVideo() && allowBuffering)
  {
    if (m_filter.IsStopping() || m_bEndOfFile || ReadFromFile() <= 0)
      return NULL;
  }

  ReadFromFile(true);

  return m_playlistManager->GetNextVideoPacket();
}

///
///Returns the next audio packet
// or NULL if there is none available
Packet* CDeMultiplexer::GetAudio()
{
  if (HoldAudio())
    return NULL;

  bool allowBuffering = m_playlistManager->AllowBuffering();

  while (!m_playlistManager->HasAudio() && allowBuffering)
  {
    if (m_filter.IsStopping() || m_bEndOfFile || ReadFromFile() <= 0)
      return NULL;
  }

  ReadFromFile(true);

  Packet* packet = m_playlistManager->GetNextAudioPacket();
  if (packet && packet->rtTitleDuration == 0)
    packet->rtTitleDuration = m_rtTitleDuration; // for fake audio

  return packet;
}

/// Starts the demuxer
/// This method will read the file until we found the pat/sdt
/// with all the audio/video pids
HRESULT CDeMultiplexer::Start()
{
  m_bReadFailed = false;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
  DWORD dwBytesProcessed = 0;
  DWORD m_Time = GetTickCount();

  const DWORD readTimeout = 25000;

  if (m_playlistManager)
    m_playlistManager->ClearClips(false);

  while ((GetTickCount() - m_Time) < readTimeout && !m_bReadFailed)
  {
    int BytesRead = ReadFromFile();

    if (dwBytesProcessed > INITIAL_READ_SIZE)
    {
      m_bReadFailed = true;
      break;
    }

    if ((!m_videoParser->basicVideoInfo.isValid ||
        m_videoParser->pmt.formattype == GUID_NULL ||
        (m_AudioStreamType > NO_STREAM && m_audioParser->pmt.formattype == GUID_NULL)) &&
        !m_bStreamPaused)
    {
      dwBytesProcessed += BytesRead;
      continue;
    }

    CMediaType pmt;
    GetAudioStreamPMT(pmt);
    m_filter.GetAudioPin()->SetInitialMediaType(&pmt);

    GetVideoStreamPMT(pmt);
    m_filter.GetVideoPin()->SetInitialMediaType(&pmt);

    return S_OK;
  }

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
int CDeMultiplexer::ReadFromFile(bool pollEvents)
{
  if (m_filter.IsStopping()) return 0;
  CAutoLock lock (&m_sectionRead);
  int dwReadBytes = 0;
  bool pause = false;

  int readSize = sizeof(m_readBuffer);
  if (pollEvents)
    readSize = 0;

  dwReadBytes = m_filter.lib.Read(m_readBuffer, readSize, pause, false);

  if (dwReadBytes > 0)
  {
    int pos = 4;
    while (pos < dwReadBytes)
    {
      OnTsPacket(&m_readBuffer[pos]);
      pos += 192;
    }
  
    m_iReadErrors = 0;
    if (dwReadBytes < sizeof(m_readBuffer))
      FlushPESBuffers(false, true);

    return dwReadBytes;
  }
  else if (dwReadBytes == -1)
  {
    LogDebug("Read failed...failure on libbluray side");
    m_bReadFailed = true;
    m_iReadErrors++;

    if (m_iReadErrors > MAX_CONSECUTIVE_READ_ERRORS)
    {
      LogDebug("Read failed too many times... EOF or broken disc?");
      m_bEndOfFile = true;

      m_filter.NotifyEvent(EC_ERRORABORT, 0, 0);
    }
  }
  /*else if (!pause)
  {
    LogDebug("Read failed...EOF");
    m_bEndOfFile = true;

    m_filter.NotifyEvent(EC_ERRORABORT, 0, 0);
  }*/
  else if (pause && m_bFlushBuffersOnPause)
  {
    m_bFlushBuffersOnPause = false;
    FlushPESBuffers(false, true);
  }

  return 0;
}

void CDeMultiplexer::HandleBDEvent(BD_EVENT& pEv)
{
  switch (pEv.event)
  {
    case BD_EVENT_ERROR:
    case BD_EVENT_READ_ERROR:
      m_bEndOfFile = true;
      m_filter.NotifyEvent(EC_ERRORABORT, 0, 0);
      break;

    case BD_EVENT_ENCRYPTED:
      m_bEndOfFile = true;
      m_filter.NotifyEvent(EC_ERRORABORT, STG_E_STATUS_COPY_PROTECTION_FAILURE, 0);
      break;

    case BD_EVENT_PLAYLIST_STOP:
      Flush(true);
      m_bLibRequestedFlush = true;
      break;

    case BD_EVENT_SEEK:
      Flush(true);
      break;

    case BD_EVENT_STILL_TIME:
      m_bStreamPaused = true;
      break;

    case BD_EVENT_STILL:
      if (pEv.param == 1)
        m_bStreamPaused = true;
      break;

    case BD_EVENT_TITLE:
      m_bTitleChanged = true;
      m_nTitle = pEv.param;
      m_filter.GetTime(&m_rtTitleChangeStarted);
      break;

    case BD_EVENT_PLAYLIST:
      m_nPlaylist = pEv.param;
      break;

    case BD_EVENT_AUDIO_STREAM:
      if (!m_filter.lib.ForceTitleBasedPlayback() && pEv.param < 0xff)
      {
        m_iAudioIdx = pEv.param - 1;
        ParseAudioStreams(m_filter.lib.CurrentClipInfo());
        m_bAudioFormatParsed = false;
      }
      break;

    case BD_EVENT_PG_TEXTST_STREAM:
      break;

    case BD_EVENT_PLAYITEM:
      LogDebug("demux: New playitem %d", pEv.param);
      
      m_bFlushBuffersOnPause = true;
      
      UINT64 clipStart = 0, clipIn = 0, bytePos = 0, duration = 0;
      int ret = m_filter.lib.GetClipInfo(pEv.param, &clipStart, &clipIn, &bytePos, &duration);
      if (ret) 
      {
        REFERENCE_TIME rtOldOffset = m_rtOffset;

        m_rtOffset = clipStart - clipIn;
        m_nClip = pEv.param;

        BLURAY_CLIP_INFO* clip = m_filter.lib.CurrentClipInfo();
        if (!clip)
        {
          LogDebug("demux: HandleBDEvent - failed to get clip info!");
          return;
        }

        UINT64 position = 0;
        m_filter.lib.CurrentPosition(position, (UINT64&)m_rtTitleDuration);
        m_rtTitleDuration = CONVERT_90KHz_DS(m_rtTitleDuration);

        //if (!m_bStarting)
        {
          REFERENCE_TIME clipOffset = m_rtOffset * -1;

          FlushPESBuffers(false, false);

          m_playlistManager->CreateNewPlaylistClip(m_nPlaylist, m_nClip, AudioStreamsAvailable(clip),
            CONVERT_90KHz_DS(clipIn), CONVERT_90KHz_DS(clipOffset), CONVERT_90KHz_DS(duration), CONVERT_90KHz_DS(position), m_bLibRequestedFlush);

          if (m_bLibRequestedFlush)
          {
            m_playlistManager->ClearClips();
            CVideoPin* pVideoPin = m_filter.GetVideoPin();
            if (pVideoPin)
              pVideoPin->SyncClipBoundary();
          }

          m_bLibRequestedFlush = false;
        }

        m_bVideoFormatParsed = false;
        m_bAudioFormatParsed = false;

        ParseVideoStream(clip);
        ParseAudioStreams(clip);
        ParseSubtitleStreams(clip);
      }
  } 
}

void CDeMultiplexer::HandleOSDUpdate(OSDTexture& /*pTexture*/)
{ 
}

void CDeMultiplexer::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);

  if (header.Pid == 0) return;
  if (header.TScrambling) return;
  if (header.TransportError) return;

  FillAudio(header, tsPacket);
  FillVideo(header, tsPacket);
}

bool CDeMultiplexer::AudioStreamsAvailable(BLURAY_CLIP_INFO* pClip)
{
  bool hasAudio = false;

  for (int i = 0; i < pClip->audio_stream_count; i++)
  {
    switch (pClip->audio_streams[i].coding_type)
    {
      case BLURAY_STREAM_TYPE_AUDIO_MPEG1:
      case BLURAY_STREAM_TYPE_AUDIO_MPEG2:
      case BLURAY_STREAM_TYPE_AUDIO_LPCM:
      case BLURAY_STREAM_TYPE_AUDIO_AC3:
      case BLURAY_STREAM_TYPE_AUDIO_DTS:
      case BLURAY_STREAM_TYPE_AUDIO_TRUHD:
      case BLURAY_STREAM_TYPE_AUDIO_AC3PLUS:
      case BLURAY_STREAM_TYPE_AUDIO_DTSHD:
      case BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER:
        hasAudio = true;
        break;
      default:
        break;
    }

    if (hasAudio)
      break;
  }

  return hasAudio;
}

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
#ifdef LOG_DEMUXER_AUDIO_SAMPLES
        LogDebug("demux: aud pts: %6.3f clip: %d playlist: %d", pts.ToClock(), m_nClip, m_nPlaylist);
#endif
        m_bAC3Substream = false;

        m_pCurrentAudioBuffer->rtStart = pts.IsValid ? CONVERT_90KHz_DS(pts.PcrReferenceBase) : Packet::INVALID_TIME;
        
        WAVEFORMATEX* wfe = (WAVEFORMATEX*)m_audioParser->pmt.pbFormat;
        if (wfe)
        {
          REFERENCE_TIME duration = (wfe->nBlockAlign * 10000000) / wfe->nAvgBytesPerSec;
          m_pCurrentAudioBuffer->rtStop = m_pCurrentAudioBuffer->rtStart + duration;
        }
        else
          m_pCurrentAudioBuffer->rtStop = m_pCurrentAudioBuffer->rtStart + 1;  

        m_pCurrentAudioBuffer->nClipNumber = m_nClip;
        m_pCurrentAudioBuffer->nPlaylist = m_nPlaylist;
        m_pCurrentAudioBuffer->rtTitleDuration = m_rtTitleDuration;

        UINT32 pesHeaderLen = p[8] + 9;
        m_nAudioPesLenght = (p[4] << 8) + p[5] - (pesHeaderLen - 6);

        unsigned int flags = p[7];
        const BYTE* pos = p + 9;

        if ((flags & 0xc0) == 0x80) 
          pos += 5; // PTS
        else if ((flags & 0xc0) == 0xc0) 
          pos += 10; // PTS & DTS

        if (flags & 0x01) // PES extension
        {
          unsigned int pes_ext = *pos++;
          // Skip PES private data, program packet sequence counter and P-STD buffer
          unsigned int skip = (pes_ext >> 4) & 0xb;
          skip += skip & 0x9;
          pos += skip;
          
          if ((pes_ext & 0x41) == 0x01 && (pos + 2) <= (p + pesHeaderLen)) 
          {  
            // PES extension 2
            if ((pos[0] & 0x7f) > 0 && (pos[1] & 0x80) == 0)
            {
              if (pos[1] == 0x76)
                m_bAC3Substream = true; // this stream will get discarded
            }
          }
        }

        UINT32 len = 188 - pesHeaderLen - 4; // 4 for TS packet header
        if (len > 0 && !m_bAC3Substream)
        { 
          byte* ps = p + pesHeaderLen;

          if (len < m_nAudioPesLenght)
          {
            m_pCurrentAudioBuffer->SetCount(len, m_nAudioPesLenght);
            memcpy(m_pCurrentAudioBuffer->GetData(), ps, len);
            packetProcessed = true;
          }
          else
          {
            m_pCurrentAudioBuffer->SetCount(m_nAudioPesLenght);
            memcpy(m_pCurrentAudioBuffer->GetData(), ps, m_nAudioPesLenght);
          }
        }
        else
        {
          if (!m_bAC3Substream)
            LogDebug(" No data");

          m_AudioValidPES = false;
        }
      }
    }
    else
    {
      LogDebug("Pes header 0-0-1 fail");
      m_AudioValidPES = false;
    }

    m_AudioValidPES = true;     
  }

  if (m_AudioValidPES && !packetProcessed && m_pCurrentAudioBuffer && !m_bAC3Substream)
  {
    if (m_pCurrentAudioBuffer->GetCount() != m_nAudioPesLenght)
    {
      int pos = header.PayLoadStart;

      if (pos > 0 && pos < 188)
      {
        int dataLenght = 188 - pos;
        m_pCurrentAudioBuffer->SetCount(m_pCurrentAudioBuffer->GetDataSize() + dataLenght);
        memcpy(m_pCurrentAudioBuffer->GetData() + m_pCurrentAudioBuffer->GetDataSize() - dataLenght, &tsPacket[pos], dataLenght);
      }
    }
    
    if (m_pCurrentAudioBuffer->GetCount() == m_nAudioPesLenght)
    {
      if (ParseAudioFormat(m_pCurrentAudioBuffer))
        m_playlistManager->SubmitAudioPacket(m_pCurrentAudioBuffer);
      else
        delete m_pCurrentAudioBuffer;

      m_pCurrentAudioBuffer = NULL;
      m_nAudioPesLenght = 0;
    }
  }
}

void CDeMultiplexer::FlushPESBuffers(bool bDiscardData, bool bSetCurrentClipFilled)
{
  LogDebug("Demux::Flushing PES %d", bDiscardData);

  CAutoLock lockVid(&m_sectionVideo);
  CAutoLock lockAud(&m_sectionAudio);

  if (m_videoServiceType != NO_STREAM && !bDiscardData)
  {
    if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_MPEG1 ||
        m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_MPEG2)
      FillVideoMPEG2(NULL, NULL, true);
    else if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_H264)
      FillVideoH264PESPacket(NULL, m_pBuild, true);
    else if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_VC1)
      FillVideoVC1PESPacket(NULL, m_pBuild, true);
  }

  m_p.Free();
  m_pBuild.Free();
  m_lastStart = 0;
  m_loopLastSearch = 1;
  m_pl.RemoveAll();
  m_nMPEG2LastPlaylist = -1;
  m_nMPEG2LastClip = -1;
  delete m_pCurrentAudioBuffer;
  m_pCurrentAudioBuffer = NULL;
  
  if (bSetCurrentClipFilled)
    m_playlistManager->CurrentClipFilled();
}

void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{
  if ( m_nVideoPid == -1 || header.Pid != m_nVideoPid || header.AdaptionFieldOnly())
    return;
  
  CAutoLock lock (&m_sectionVideo);

  if (m_bShuttingDown) return;

  if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_MPEG1 ||
      m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_MPEG2)
  {
    FillVideoMPEG2(&header, tsPacket);
  }
  else
  {
    FillVideoH264(&header, tsPacket);
  }
}

void CDeMultiplexer::PacketDelivery(CAutoPtr<Packet> p)
{
  if (m_filter.GetVideoPin()->IsConnected())
  {
    if (p->rtStart != Packet::INVALID_TIME)
      m_LastValidFrameCount++;
    
    ParseVideoFormat(p);
    if (m_bVideoFormatParsed)
      m_playlistManager->SubmitVideoPacket(p.Detach());
  }
  else
    ParseVideoFormat(p);
}

bool CDeMultiplexer::ParseVideoFormat(Packet* p)
{
  if (!m_bVideoFormatParsed)
  {
    m_bVideoFormatParsed = m_videoParser->OnTsPacket(p->GetData(), p->GetCount(), m_videoServiceType);
    if (!m_bVideoFormatParsed)
      LogDebug("demux: ParseVideoFormat - failed to parse video format!");
    else
    {
      LogDebug("demux: ParseVideoFormat - succeeded {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X} on %I64d (%d,%d)",
        m_videoParser->pmt.subtype.Data1, m_videoParser->pmt.subtype.Data2, m_videoParser->pmt.subtype.Data3,
        m_videoParser->pmt.subtype.Data4[0], m_videoParser->pmt.subtype.Data4[1], m_videoParser->pmt.subtype.Data4[2],
        m_videoParser->pmt.subtype.Data4[3], m_videoParser->pmt.subtype.Data4[4], m_videoParser->pmt.subtype.Data4[5], 
        m_videoParser->pmt.subtype.Data4[6], m_videoParser->pmt.subtype.Data4[7], p->rtStart, p->nPlaylist, p->nClipNumber);

      m_playlistManager->SetVideoPMT(CreateMediaType(&m_videoParser->pmt), p->nPlaylist, p->nClipNumber);
    }
  }

  return m_bVideoFormatParsed;
}

bool CDeMultiplexer::ParseAudioFormat(Packet* p)
{
  if (!m_bAudioFormatParsed)
  {
    m_bAudioFormatParsed = m_audioParser->OnTsPacket(p->GetData(), p->GetCount(), m_AudioStreamType);
    if (!m_bAudioFormatParsed)
      LogDebug("demux: ParseAudioFormat - failed to parse audio format!");
    else
    {
      LogDebug("demux: ParseAudioFormat - succeeded");
      p->pmt = CreateMediaType(&m_audioParser->pmt);
    }
  }

  return m_bAudioFormatParsed;
}

void CDeMultiplexer::FillVideoH264PESPacket(CTsHeader* header, CAutoPtr<Packet> p, bool pFlushBuffers)
{
  if (!pFlushBuffers)
  {
    if (!m_p) 
    {
      m_p.Attach(new Packet());
      m_p->SetCount(0, PACKET_GRANULARITY);
      m_p->TransferProperties(*p, false, false);
    }

    m_p->Append(*p);
  }
  else
  {
    if (!m_p) 
      return;

    m_loopLastSearch = 0;
  }

  BYTE* start = m_p->GetData();
  BYTE* end = start + m_p->GetCount();

  while (start <= end - 4 && *(DWORD*)start != 0x01000000) start++;
  while (start <= end - 4) 
  {
    BYTE* next =  start + (m_loopLastSearch==0 ? 1 : m_loopLastSearch);
    while(next <= end - 4 && *(DWORD*)next != 0x01000000) next++;
    m_loopLastSearch = next - start;
    if (next >= end - 4)
    {
      if (pFlushBuffers)
        next = end;
      else 
        break;
    }
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


    p2->TransferProperties(*m_p, false, true);
    m_pl.AddTail(p2);

    if (p)
    {
      m_p->CopyProperties(*p, true);
      p->rtStart = Packet::INVALID_TIME;
    }

    start = next;
  }

  if (start > m_p->GetData()) 
  {
    m_loopLastSearch = 1;
    m_p->RemoveAt(0, start - m_p->GetData());
  }

  for (POSITION pos = m_pl.GetHeadPosition(); pos; m_pl.GetNext(pos)) 
  {
    if (pos == m_pl.GetHeadPosition()) 
      continue;

    Packet* pPacket = m_pl.GetAt(pos);
    BYTE* pData = pPacket->GetData();

    if (!pData)
      continue;

    if ((pData[4]&0x1f) == 0x09) 
      m_fHasAccessUnitDelimiters = true;

    if ((pData[4]&0x1f) == 0x09 || !m_fHasAccessUnitDelimiters && pPacket->rtStart != Packet::INVALID_TIME) 
    {
      p = m_pl.RemoveHead();

      while (pos != m_pl.GetHeadPosition() && !m_pl.IsEmpty())
      {
        CAutoPtr<Packet> p2 = m_pl.RemoveHead();
        p->Append(*p2);
      }
      //LogDebug("PacketDelivery - %6.3f %d %d", p->rtStart / 10000000.0, p->nPlaylist, p->nClipNumber);
      PacketDelivery(p);
    }
  }
  if (pFlushBuffers && m_pl.GetCount() > 0)
  {
    p = m_pl.RemoveHead();
    while (!m_pl.IsEmpty())
    {
      CAutoPtr<Packet> p2 = m_pl.RemoveHead();
      p->Append(*p2);
    }

    PacketDelivery(p);
  }
}

void CDeMultiplexer::FillVideoVC1PESPacket(CTsHeader* header, CAutoPtr<Packet> p, bool pFlushBuffers) 
{
  if (!pFlushBuffers)
  {
    if (!m_p) 
    {
      m_p.Attach(new Packet());
      m_p->SetCount(0, PACKET_GRANULARITY);
      m_p->TransferProperties(*p, false, false);
    }

    m_p->Append(*p);
  }
  else if (!m_p)
    return;

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

  bool bSeqFound = false;
  while(start <= end - 4)
  {
    if (*(DWORD*)start == 0x0D010000)
    {
      bSeqFound = true;
      break;
    } 
    if (*(DWORD*)start == 0x0F010000)
      break;
    start++;
  }

  while (start <= end - 4)
  {
    BYTE* next = start + (m_loopLastSearch == 0 ? 1 : m_loopLastSearch);

    while (next <= end - 4)
    {
      if (*(DWORD*)next == 0x0D010000) 
      {
        if (bSeqFound) 
          break;
        
        bSeqFound = true;
      } 
      else if (*(DWORD*)next == 0x0F010000) 
        break;

      next++;
    }

    m_loopLastSearch = next - start;

    if (next >= end - 4) 
    {
      if (pFlushBuffers)
        next = end;
      else
        break;
    }

    int size = next - start - 4;
    UNUSED_ALWAYS(size);

    CAutoPtr<Packet> p2(new Packet());
    p2->SetCount(0, PACKET_GRANULARITY);
    p2->CopyProperties(*m_p);
    p2->SetData(start, next - start);

    PacketDelivery(p2);
   
    if (p)
    {
      m_p->CopyProperties(*p, true);
      p->rtStart = Packet::INVALID_TIME;
    }

    start = next;
    if (!pFlushBuffers || start!=end)
      bSeqFound = (*(DWORD*)start == 0x0D010000);
    else
    {
      m_p.Free();
      m_loopLastSearch = 1;
    }
  }

  if (m_p && start > m_p->GetData())
  {
    m_loopLastSearch = 1;
    m_p->RemoveAt(0, start - m_p->GetData());
  }
}

void CDeMultiplexer::FillVideoH264(CTsHeader* header, byte* tsPacket)
{
  int headerlen = header->PayLoadStart;

  CPcr pts;
  CPcr dts;

  if (!m_pBuild)
  {
    m_pBuild.Attach(new Packet());
    m_pBuild->bDiscontinuity = false;
    m_pBuild->rtStart = Packet::INVALID_TIME;
    m_pBuild->nClipNumber = m_nClip;
    m_pBuild->nPlaylist = m_nPlaylist;
    m_pBuild->rtTitleDuration = m_rtTitleDuration;

    m_lastStart = 0;
  }
  
  if (header->PayloadUnitStart)
  {
#ifdef LOG_DEMUXER_VIDEO_SAMPLES    
    //LogDebug("demux: FillVideoH264 PayLoad Unit Start");
#endif
    
    m_WaitHeaderPES = m_pBuild->GetCount();

    if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_VC1 && m_pBuild->GetCount() > 0)
    {
      FillVideoVC1PESPacket(header, m_pBuild);
      m_pBuild.Free();

      m_pBuild.Attach(new Packet());
      m_pBuild->bDiscontinuity = false;
      m_pBuild->rtStart = Packet::INVALID_TIME;
      m_pBuild->nClipNumber = m_nClip;
      m_pBuild->nPlaylist = m_nPlaylist;
      m_pBuild->rtTitleDuration = m_rtTitleDuration;
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
      m_pBuild->rtTitleDuration = -21;

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
        m_VideoValidPES = true;
        if (CPcr::DecodeFromPesHeader(start, 0, pts, dts))
        {
#ifdef LOG_DEMUXER_VIDEO_SAMPLES
          LogDebug("demux: vid pts: %6.3f clip: %d playlist: %d", pts.ToClock(), m_nClip, m_nPlaylist);
#endif
        }

        m_lastStart -= 9 + start[8];
        m_pBuild->RemoveAt(m_WaitHeaderPES, 9 + start[8]);
        
        if (pts.IsValid)
        {
          m_pBuild->rtStart = CONVERT_90KHz_DS(pts.PcrReferenceBase);
        
          if (m_bVideoFormatParsed)
          {
            if (m_videoServiceType == BLURAY_STREAM_TYPE_VIDEO_VC1)
              m_pBuild->rtStop = m_pBuild->rtStart + ((VIDEOINFOHEADER*)m_videoParser->pmt.pbFormat)->AvgTimePerFrame;
            else
              m_pBuild->rtStop = m_pBuild->rtStart + ((MPEG2VIDEOINFO*)m_videoParser->pmt.pbFormat)->hdr.AvgTimePerFrame;
          }
        }
        else
          m_pBuild->rtStart = m_pBuild->rtStop = Packet::INVALID_TIME;

        if (m_bStreamPaused)
        {
          m_pBuild->bResuming = true;
          m_bStreamPaused = false;
        }

        m_pBuild->nClipNumber = m_nClip;
        m_pBuild->nPlaylist = m_nPlaylist;
        m_pBuild->rtTitleDuration = m_rtTitleDuration;

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

void CDeMultiplexer::FillVideoMPEG2(CTsHeader* header, byte* tsPacket, bool pFlushBuffers)
{                                                
  static const double frame_rate[16]={1.0/25.0,       1001.0/24000.0, 1.0/24.0, 1.0/25.0,
                                    1001.0/30000.0, 1.0/30.0,       1.0/50.0, 1001.0/60000.0,
                                    1.0/60.0,       1.0/25.0,       1.0/25.0, 1.0/25.0,
                                    1.0/25.0,       1.0/25.0,       1.0/25.0, 1.0/25.0 };
  static const char tc[]="XIPBXXXX";

  if (!pFlushBuffers)
  {
    if (!header)
      return;

    int headerlen = header->PayLoadStart;

    CPcr pts, dts;

    if (!m_p)
    {
      m_p.Attach(new Packet());
      m_p->SetCount(0, PACKET_GRANULARITY);
      m_p->bDiscontinuity = false;
      m_p->rtStart = Packet::INVALID_TIME;
      m_p->nPlaylist = m_nPlaylist;
      m_p->nClipNumber = m_nClip;
      m_p->rtTitleDuration = m_rtTitleDuration;
      m_lastStart = 0;
      m_bInBlock = false;
    }
  
    if (header->PayloadUnitStart)
    {
      m_WaitHeaderPES = m_p->GetCount();

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
    
      if ((start[0] != 0) || (start[1] != 0) || (start[2] != 1))
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
          if (CPcr::DecodeFromPesHeader(start, 0, pts, dts))
          {
  #ifdef LOG_DEMUXER_VIDEO_SAMPLES
            LogDebug("demux: vid pts: %6.3f clip: %d playlist: %d", pts.ToClock(), m_nClip, m_nPlaylist);
  #endif
            m_VideoPts = pts;
          }
        }

        m_lastStart -= 9 + start[8];
        m_p->RemoveAt(m_WaitHeaderPES, 9 + start[8]);
        m_WaitHeaderPES = -1;
        m_VideoValidPES = true;

        if (pts.IsValid)
        {
          m_p->rtStart = CONVERT_90KHz_DS(pts.PcrReferenceBase);
          
          if (m_bVideoFormatParsed)
            m_p->rtStop = m_p->rtStart + ((MPEG2VIDEOINFO*)m_videoParser->pmt.pbFormat)->hdr.AvgTimePerFrame;
        }
        else
           m_p->rtStart = m_p->rtStop = Packet::INVALID_TIME;
        
        if (m_nMPEG2LastPlaylist == -1)
          m_nMPEG2LastPlaylist = m_nPlaylist;

        if (m_nMPEG2LastClip == -1)
          m_nMPEG2LastClip = m_nClip;

        if (m_nMPEG2LastTitleDuration == -1)
          m_nMPEG2LastTitleDuration = m_rtTitleDuration;

        m_p->nPlaylist = m_nMPEG2LastPlaylist;
        m_p->nClipNumber = m_nMPEG2LastClip;
        m_p->rtTitleDuration = m_nMPEG2LastTitleDuration;
        m_nMPEG2LastPlaylist = m_nPlaylist;
        m_nMPEG2LastClip = m_nClip;
        m_nMPEG2LastTitleDuration = m_rtTitleDuration;
      }
    }
  }
  
  if (m_p && m_p->GetCount())
  {
    BYTE* start = m_p->GetData();
    BYTE* end = start + m_p->GetCount();
    // 000001B3 sequence_header_code
    // 00000100 picture_start_code

    while (start <= end - 4)
    {
      if (((*(DWORD*)start & 0xFFFFFFFF) == 0xb3010000) || ((*(DWORD*)start & 0xFFFFFFFF) == 0x00010000))
      {
        if (!m_bInBlock)
        {
          if (m_VideoPts.IsValid) m_CurrentVideoPts = m_VideoPts;
          m_VideoPts.IsValid = false;
          m_bInBlock = true;
        }
        break;
      }
      start++;
    }

    if (start <= end - 4)
    {
      BYTE* next = start + 1;
      if (next < m_p->GetData() + m_lastStart)
        next = m_p->GetData() + m_lastStart;

      if (!pFlushBuffers)
        while(next <= end - 4 && ((*(DWORD*)next & 0xFFFFFFFF) != 0xb3010000) && ((*(DWORD*)next & 0xFFFFFFFF) != 0x00010000)) next++;
      else
        next = end;

      if ((next >= end - 4) && !pFlushBuffers)
        m_lastStart = next - m_p->GetData();
      else
      {
        m_bInBlock = false;
        int size = next - start;

        CAutoPtr<Packet> p2(new Packet());
        p2->SetCount(size);
        memcpy(p2->GetData(), m_p->GetData(), size);
        
        if (*(DWORD*)p2->GetData() == 0x00010000 || (*(DWORD*)p2->GetData() == 0xb3010000) && pFlushBuffers)
        {
          BYTE* p = p2->GetData();
          char frame_type = tc[((p[5]>>3)&7)];    // Extract frame type (IBP). Just info.
          int frame_count = (p[5]>>6)+(p[4]<<2);  // Extract temporal frame count to rebuild timestamp ( if required )

          m_pl.AddTail(p2);
       
          if (m_VideoValidPES)
          {
            Packet* p = m_pl.RemoveHead().Detach();

            while (m_pl.GetCount())
            {
              Packet* head = m_pl.RemoveHead().Detach();
              p->Append(*head);
              delete head;
            }
    
            if (m_CurrentVideoPts.IsValid)
            {
              m_LastValidFrameCount = frame_count;
              m_LastValidFramePts = m_CurrentVideoPts;
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
			
     		    if (m_bVideoFormatParsed)
              p->rtStop = p->rtStart + ((MPEG2VIDEOINFO*)m_videoParser->pmt.pbFormat)->hdr.AvgTimePerFrame;

            p->nClipNumber = m_p->nClipNumber;
            p->nPlaylist = m_p->nPlaylist;
            p->rtTitleDuration = m_p->rtTitleDuration;

            ParseVideoFormat(p);

            if (m_bStreamPaused && p->rtStart != Packet::INVALID_TIME)
            {
              p->bResuming = true;
              m_bStreamPaused = false;
            }

            if (m_bVideoFormatParsed)
              m_playlistManager->SubmitVideoPacket(p);
            else
            {
              delete p;
              p = NULL;
            }
          }
          m_CurrentVideoPts.IsValid = false;
          m_VideoValidPES = true;                                    // We've just completed a frame, set flag until problem clears it 
          m_pl.RemoveAll();
        }
        else                                                        // sequence_header_code
        {
          m_curFrameRate = frame_rate[*(p2->GetData()+7) & 0x0F];   // Extract frame rate in seconds.
   	      m_pl.AddTail(p2);                                         // Add sequence header.
   	    }
      
        start = next;
        m_lastStart = start - m_p->GetData() + 1;
      }
      if (start > m_p->GetData())
      {
        m_lastStart -= (start - m_p->GetData());
        m_p->RemoveAt(0, start - m_p->GetData());
      }
    }
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
    
    if (clip->audio_stream_count <= 0 || !AudioStreamsAvailable(clip))
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

      return;
    }

    bd_player_settings& settings = m_filter.lib.GetBDPlayerSettings();
    int iAudioIdx_tmp = 0;
    for (int i(0); i < clip->audio_stream_count; i++)
    {
      stAudioStream audio;

      audio.language[0] = clip->audio_streams[i].lang[0];
      audio.language[1] = clip->audio_streams[i].lang[1];
      audio.language[2] = clip->audio_streams[i].lang[2];
      audio.language[3] = clip->audio_streams[i].lang[3];

      audio.audioType = clip->audio_streams[i].coding_type;
      audio.pid = clip->audio_streams[i].pid;
      audio.audioChannelCount = clip->audio_streams[i].format;

      if(m_filter.lib.ForceTitleBasedPlayback())
      {
        if (strncmp(audio.language, settings.audioLang, 3) == 0 && m_iAudioIdx < 0)
        {
          iAudioIdx_tmp = i;
          if (audio.audioType == settings.audioType)
            m_iAudioIdx = i;
        }
      }

      LogDebug("   Audio #%d:[%4d] %s %s %s", i, audio.pid, audio.language, StreamFormatAsString(audio.audioType), StreamAudioFormatAsString(audio.audioChannelCount));

      m_audioStreams.push_back(audio);
    }
		
    if (m_iAudioIdx < 0 || m_iAudioIdx >= clip->audio_stream_count)
      m_iAudioIdx = iAudioIdx_tmp;

    m_iAudioStream = m_iAudioIdx;
    m_AudioStreamType = clip->audio_streams[m_iAudioIdx].coding_type;

    LogDebug("demux: ParseAudioStreams - Audio track #%d selected", m_iAudioIdx);
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

void CDeMultiplexer::AudioStreamMediaType(int stream, CMediaType& pmt)
{
  pmt.InitMediaType();
  pmt.SetType(&MEDIATYPE_Audio);
  pmt.SetSampleSize(1);
  pmt.SetTemporalCompression(FALSE);
  pmt.SetVariableSize();
  pmt.SetFormatType(&FORMAT_WaveFormatEx);

  int type = GetAudioStreamType(stream);

  switch (type)
  {
    case BLURAY_STREAM_TYPE_AUDIO_MPEG1:
      pmt.SetSubtype(&MEDIASUBTYPE_MPEG1Payload);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_MPEG2:
      pmt.SetSubtype(&MEDIASUBTYPE_MPEG2_AUDIO);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_LPCM:
      pmt.SetSubtype(&MEDIASUBTYPE_HDMV_LPCM_AUDIO);
    case BLURAY_STREAM_TYPE_AUDIO_AC3:
      pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_DTS:
      pmt.SetSubtype(&MEDIASUBTYPE_DTS);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_TRUHD:
      pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_TRUEHD);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_AC3PLUS:
      pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_DDPLUS);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_DTSHD:
      pmt.SetSubtype(&MEDIASUBTYPE_DTS_HD);
      break;
    case BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER:
      pmt.SetSubtype(&MEDIASUBTYPE_DTS_HD);
      break;
    default:
      pmt.SetSubtype(&MEDIASUBTYPE_MPEG1Payload);
      break;
  }
}

char* CDeMultiplexer::StreamFormatAsString(int pStreamType)
{
  switch (pStreamType)
  {
  case BLURAY_STREAM_TYPE_VIDEO_MPEG1:
    return "MPEG1";
  case BLURAY_STREAM_TYPE_VIDEO_MPEG2:
    return "MPEG2";
  case BLURAY_STREAM_TYPE_AUDIO_MPEG1:
    return "MPEG1";
  case BLURAY_STREAM_TYPE_AUDIO_MPEG2:
    return "MPEG2";
  case BLURAY_STREAM_TYPE_VIDEO_H264:
    return "H264";
  case BLURAY_STREAM_TYPE_VIDEO_VC1:
    return "VC1";
  case BLURAY_STREAM_TYPE_AUDIO_LPCM:
    return "LPCM";
  case BLURAY_STREAM_TYPE_AUDIO_AC3:
    return "AC3";
  case BLURAY_STREAM_TYPE_AUDIO_DTS:
    return "DTS";
  case BLURAY_STREAM_TYPE_AUDIO_TRUHD:
    return "TrueHD";
  case BLURAY_STREAM_TYPE_AUDIO_AC3PLUS:
    return "AC3+";
  case BLURAY_STREAM_TYPE_AUDIO_DTSHD:
    return "DTS-HD";
  case BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER:
    return "DTS-HD Master";
  case 0x0f:
    return "AAC";
  case 0x11:
    return "LATM AAC";
  case BLURAY_STREAM_TYPE_SUB_PG:
    return "PGS";
  case BLURAY_STREAM_TYPE_SUB_IG:
    return "IG";
  case BLURAY_STREAM_TYPE_SUB_TEXT:
    return "Text";
  default:
    return "Unknown";
  }
}

LPCTSTR CDeMultiplexer::StreamAudioFormatAsString(int pStreamAudioChannel)
{
  switch (pStreamAudioChannel)
  {
  case BLURAY_AUDIO_FORMAT_MONO:
    return _T("1.0");
  case BLURAY_AUDIO_FORMAT_STEREO:
    return _T("2.0");
  case BLURAY_AUDIO_FORMAT_MULTI_CHAN:
    return _T("5.1");
  case BLURAY_AUDIO_FORMAT_COMBO:
    return _T("7.1");
  default:
    return _T("Unknown");
  }
}

