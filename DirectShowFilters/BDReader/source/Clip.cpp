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

#include <afx.h>

#include "Clip.h"
#include <streams.h>
#include "mediaformats.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);

#define HALF_SECOND 5000000LL

CClip::CClip(int clipNumber, int playlistNumber, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffset, REFERENCE_TIME totalStreamOffset, bool audioPresent, REFERENCE_TIME duration, REFERENCE_TIME streamStartOffset, bool seekTarget, bool interrupted)
{
  nClip = clipNumber;
  nPlaylist = playlistNumber;

  playlistFirstPacketTime = firstPacketTime;

  lastVideoPosition = playlistFirstPacketTime;
  lastAudioPosition = playlistFirstPacketTime;
  audioPlaybackPosition = playlistFirstPacketTime;
  videoPlaybackPosition = playlistFirstPacketTime;

  earliestPacketAccepted = _I64_MAX;
  firstVideoPosition = _I64_MAX;
  firstAudioPosition = _I64_MAX;

  clipDuration = duration;
  clipPlaylistOffset = totalStreamOffset;
  m_rtStreamStartOffset = streamStartOffset;

  m_playlistOffset = clipOffset;
  m_rtClipVideoStartingOffset = 0LL;
  m_rtClipAudioStartingOffset = 0LL;

  m_bCalculateAudioOffset = true;
  m_bCalculateVideoOffset = true;

  noAudio =! audioPresent;

  bSeekTarget = seekTarget;
  clipInterrupted = interrupted;

  superseded = NO_SUPERSEDE;

  m_videoPmt = NULL;

  firstAudio = true;
  firstVideo = true;
  firstPacketAccepted = false;
  firstPacketReturned = false;

  clipReset = false;
  //LogDebug("CClip:: New Clip (%d,%d) stream Offset %I64d", nPlaylist, nClip, totalStreamOffset);
}

CClip::~CClip(void)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  CAutoLock vectorALock(&m_sectionVectorAudio);
  FlushAudio();
  FlushVideo();
  DeleteMediaType(m_videoPmt);
}

Packet* CClip::ReturnNextAudioPacket(REFERENCE_TIME playlistOffset)
{
  CAutoLock vectorALock(&m_sectionVectorAudio);
  Packet* ret = NULL;

  if (noAudio)
  {
    ret = GenerateFakeAudio(audioPlaybackPosition);
    if (earliestPacketAccepted == INT64_MAX)
      earliestPacketAccepted = audioPlaybackPosition;
  }
  else
  {
    if (m_vecClipAudioPackets.size()>0)
    {
      ivecAudioBuffers it = m_vecClipAudioPackets.begin();
      ret = *it;
      audioPlaybackPosition=ret->rtStart;
      it = m_vecClipAudioPackets.erase(it);
    }
  }
  if (ret && ret->rtStart!=Packet::INVALID_TIME)
  {
    if (firstAudio)
    {
      firstAudioPosition = ret->rtStart;
      ret->nNewSegment |= NS_STREAM_RESET;
      ret->bDiscontinuity = clipInterrupted | bSeekTarget | clipReset;
      firstAudio = false;

      if (!clipReset)
        ret->nNewSegment |= NS_NEW_CLIP;

      if (clipInterrupted)
        ret->nNewSegment |= NS_INTERRUPTED;

      if (m_bCalculateAudioOffset && (abs(earliestPacketAccepted - ret->rtStart) > 0))
        m_rtClipAudioStartingOffset =  earliestPacketAccepted - ret->rtStart;
      
      m_bCalculateAudioOffset = false;
    }

    if (!firstPacketReturned)
      firstPacketReturned = true;

    ret->rtPlaylistTime = ret->rtStart + m_rtStreamStartOffset;

    if (m_rtStreamStartOffset > 0)
      ret->rtPlaylistTime -= earliestPacketAccepted;
    else
      ret->rtPlaylistTime -= playlistFirstPacketTime;

    ret->rtClipStartTime = ret->rtStart - earliestPacketAccepted + m_rtClipAudioStartingOffset;
    ret->rtStart += clipPlaylistOffset - earliestPacketAccepted + m_rtClipAudioStartingOffset;
    ret->rtStop += clipPlaylistOffset - earliestPacketAccepted + m_rtClipAudioStartingOffset;
  }

//  LogDebug("Clip: aud: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

Packet* CClip::ReturnNextVideoPacket(REFERENCE_TIME playlistOffset)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  Packet* ret = NULL;

  if (m_vecClipVideoPackets.size() > 0 && m_videoPmt)
  {
    ivecVideoBuffers it = m_vecClipVideoPackets.begin();
    ret = *it;
    it = m_vecClipVideoPackets.erase(it);
  }

  if (ret)
  {
    if (ret->rtStart!=Packet::INVALID_TIME)
    {
      if (firstVideo)
      {
        ret->bDiscontinuity = clipInterrupted | bSeekTarget | clipReset;
        ret->nNewSegment |= NS_STREAM_RESET;

        if (!clipReset)
          ret->nNewSegment |= NS_NEW_CLIP;

        if (clipInterrupted)
          ret->nNewSegment |= NS_INTERRUPTED;

        firstVideo = false;
        bSeekTarget = false;
        ret->pmt = CreateMediaType(m_videoPmt);

        if (m_bCalculateVideoOffset && (abs(earliestPacketAccepted - ret->rtStart) > 0))
          m_rtClipVideoStartingOffset = earliestPacketAccepted - ret->rtStart;

        m_bCalculateVideoOffset = false;
      }

      if (ret->rtStart > videoPlaybackPosition) 
      {
        videoPlaybackPosition = ret->rtStart;
        //LogDebug("Videoplayback position (%d,%d) %I64d", nPlaylist, nClip, videoPlaybackPosition);
      }

      if (!firstPacketReturned)
        firstPacketReturned = true;

      ret->rtPlaylistTime = ret->rtStart + m_rtStreamStartOffset;

      if (m_rtStreamStartOffset > 0)
        ret->rtPlaylistTime -= earliestPacketAccepted;
      else
        ret->rtPlaylistTime -= playlistFirstPacketTime;

      ret->rtClipStartTime = ret->rtStart - earliestPacketAccepted + m_rtClipVideoStartingOffset;
      ret->rtStart += clipPlaylistOffset - earliestPacketAccepted + m_rtClipVideoStartingOffset;
      ret->rtStop += clipPlaylistOffset - earliestPacketAccepted + m_rtClipVideoStartingOffset;
    }
  }
//  LogDebug("Clip: vid: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

Packet* CClip::GenerateFakeAudio(REFERENCE_TIME rtStart)
{
  if (!firstAudio && (superseded & AUDIO_RETURN))
      return NULL;

  bool bSetAudioSuperceeded = false;
  
  if (rtStart + FAKE_AUDIO_DURATION - 1 > playlistFirstPacketTime + clipDuration)
  {
    LogDebug("Fake audio SUPERCEEDED_AUDIO_RETURN (%d,%d) clipDuration: %I64d", nPlaylist, nClip, clipDuration);
    bSetAudioSuperceeded = true;
  }

  Packet* packet = new Packet();
  packet->nClipNumber = nClip;
  packet->nPlaylist = nPlaylist;

  packet->SetCount(AC3_FRAME_LENGTH);
  packet->SetData(ac3_sample, AC3_FRAME_LENGTH);
  packet->rtStart = rtStart;
  packet->rtStop = packet->rtStart + FAKE_AUDIO_DURATION;

  if (firstAudio)
  {
    CMediaType pmt;
    pmt.InitMediaType();
    pmt.SetType(&MEDIATYPE_Audio);
    pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
    pmt.SetSampleSize(1);
    pmt.SetTemporalCompression(FALSE);
    pmt.SetVariableSize();
    pmt.SetFormatType(&FORMAT_WaveFormatEx);
    pmt.SetFormat(AC3AudioFormat, sizeof(AC3AudioFormat));
    WAVEFORMATEXTENSIBLE* wfe = (WAVEFORMATEXTENSIBLE*)pmt.pbFormat;
    wfe->Format.nChannels = 6;
    wfe->Format.nSamplesPerSec = 48000;
    wfe->Format.wFormatTag = WAVE_FORMAT_DOLBY_AC3;

    packet->pmt = CreateMediaType(&pmt);
  }
  
  if (earliestPacketAccepted == INT64_MAX)
    earliestPacketAccepted = audioPlaybackPosition;

  firstPacketAccepted = true;

  lastAudioPosition += FAKE_AUDIO_DURATION;
  audioPlaybackPosition += FAKE_AUDIO_DURATION;

  if (bSetAudioSuperceeded)
    superseded |= AUDIO_RETURN | AUDIO_FILL;

  return packet;
}

bool CClip::AcceptAudioPacket(Packet* packet)
{
  CAutoLock vectorALock(&m_sectionVectorAudio);
  if (nPlaylist != packet->nPlaylist) 
    return false;

  if (packet->nClipNumber != nClip)
  {
    LogDebug("Clip::Removing incorrect Audio Packet");
    // Oh dear, not for this clip so throw it away
    delete packet;
  }
  else
  {
    //CAutoLock lock(m_sectionRead);
    if (!firstPacketReturned)
    {
      if (earliestPacketAccepted > packet->rtStart) earliestPacketAccepted = packet->rtStart;
        firstPacketAccepted = true;
    }

    packet->nClipNumber = nClip;

    m_vecClipAudioPackets.push_back(packet);

    if (packet->rtStart != Packet::INVALID_TIME)
      lastAudioPosition = packet->rtStart;
    
    noAudio = false;
  }

  return true;
}

bool CClip::AcceptVideoPacket(Packet*  packet)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  if (packet->nClipNumber != nClip)
  {
    LogDebug("Clip::Removing incorrect Video Packet");
    // Oh dear, not for this clip so throw it away
    delete packet;
  }
  else
  {
    if (!firstPacketReturned && packet->rtStart != Packet::INVALID_TIME && earliestPacketAccepted > packet->rtStart)
        earliestPacketAccepted = packet->rtStart;

    if (!firstPacketAccepted && packet->rtStart != Packet::INVALID_TIME)
    {
      firstPacketAccepted = true;
      lastVideoPosition = packet->rtStart;
    }

    if (packet->rtStart != Packet::INVALID_TIME)
      lastVideoPosition=packet->rtStart;

    m_vecClipVideoPackets.push_back(packet);
  }

  return true;
}

void CClip::Supersede(int supersedeType)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  CAutoLock vectorALock(&m_sectionVectorAudio);

  superseded |= supersedeType;
  LogSupersede(superseded);
}

bool CClip::IsSuperseded(int supersedeType)
{
  return (superseded & supersedeType) == supersedeType;
}

void CClip::FlushAudio(Packet* pPacketToKeep)
{
  CAutoLock vectorALock(&m_sectionVectorAudio);
  ivecAudioBuffers ita = m_vecClipAudioPackets.begin();
  while (ita != m_vecClipAudioPackets.end())
  {
    Packet* packet =* ita;
    if (packet!=pPacketToKeep)
    {
      ita = m_vecClipAudioPackets.erase(ita);
      delete packet;
      //LogDebug("clip: FlushAudio -  packet removed");
    }
    else
    {
      //LogDebug("clip: FlushAudio - skipped packet");
      ++ita;
    }
  }
}

void CClip::FlushVideo(Packet* pPacketToKeep)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  ivecVideoBuffers itv = m_vecClipVideoPackets.begin();

  while (itv != m_vecClipVideoPackets.end())
  {
    Packet* packet =* itv;
    if (packet != pPacketToKeep)
    {
      itv = m_vecClipVideoPackets.erase(itv);
      delete packet;
      //LogDebug("clip: FlushVideo -  packet removed");
    }
    else
    {
      //LogDebug("clip: FlushVideo - skipped packet");
      ++itv;
    }
  }
}

void CClip::Reset(REFERENCE_TIME totalStreamOffset)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  CAutoLock vectorALock(&m_sectionVectorAudio);

  LogDebug("CClip:: Clip Reset (%d,%d) stream Offset %I64d", nPlaylist, nClip, totalStreamOffset);

  FlushAudio();
  FlushVideo();
  
  lastVideoPosition = playlistFirstPacketTime;
  lastAudioPosition = playlistFirstPacketTime;
  
  if (clipPlaylistOffset != totalStreamOffset)
  {
    clipReset = true;
    clipPlaylistOffset = totalStreamOffset;
  }

  audioPlaybackPosition = playlistFirstPacketTime;
  videoPlaybackPosition = playlistFirstPacketTime;
  m_rtClipAudioStartingOffset = 0LL;
  m_rtClipVideoStartingOffset = 0LL;

  earliestPacketAccepted = INT64_MAX;

  superseded = NO_SUPERSEDE;

  firstAudio = true;
  firstVideo = true;
  firstPacketAccepted = false;
  firstPacketReturned = false;
}

bool CClip::HasAudio()
{
  CAutoLock vectorALock(&m_sectionVectorAudio);

  if (!m_videoPmt)
    return false;

  if (m_vecClipAudioPackets.size() > 0)
    return true;
  else if (noAudio && !IsSuperseded(AUDIO_RETURN))
      return true;

  return false;
}

bool CClip::HasVideo()
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);

  if (!m_videoPmt)
    return false;

  if (m_vecClipVideoPackets.size() > 0)
    return true;

  return false;
}

REFERENCE_TIME CClip::Incomplete()
{
  // clip not played so not incomplete
  if (!firstPacketReturned || !firstPacketAccepted || firstVideo)
    return 0LL;

  REFERENCE_TIME ret = clipDuration - earliestPacketAccepted + playlistFirstPacketTime - PlayedDuration();
  if (ret > HALF_SECOND)
  {    
    LogDebug("clip: Incomplete - nClip: %d lastAudioPosition: %I64d first: %I64d duration: %I64d", 
      nClip, lastAudioPosition, playlistFirstPacketTime, clipDuration);
  }
  return ret;
}

REFERENCE_TIME CClip::PlayedDuration()
{
  REFERENCE_TIME start = earliestPacketAccepted;
  REFERENCE_TIME finish = audioPlaybackPosition;
  REFERENCE_TIME playDuration = 0LL;

  LogDebug("CClip::(%d,%d) earliestPacketAccepted %I64d audioPlaybackPosition %I64d videoPlaybackPosition %I64d a:%d v:%d",
    nPlaylist, nClip, earliestPacketAccepted, audioPlaybackPosition, videoPlaybackPosition, firstAudio, firstVideo);

  if (!firstPacketReturned || !firstPacketAccepted) 
  {
    LogDebug("CClip::PlayedDuration 0 - clip unplayed");
    return 0LL;
  }


  playDuration = finish - playlistFirstPacketTime;
  if (abs(clipDuration - playDuration) < HALF_SECOND)
  {
    LogDebug("CClip::PlayedDuration %I64d - clip played to end", clipDuration);
    return clipDuration;
  }

  if (earliestPacketAccepted>finish)
    return 0LL;

  LogDebug("CClip::PlayedDuration %I64d - clip (%d,%d) partially played finish %I64d start %I64d", finish - earliestPacketAccepted, nPlaylist, nClip, finish, earliestPacketAccepted);
  return lastAudioPosition - firstAudioPosition;
}

void CClip::SetVideoPMT(AM_MEDIA_TYPE *pmt)
{
  if (m_videoPmt)
    DeleteMediaType(m_videoPmt);

  m_videoPmt = CreateMediaType(pmt);
}

void CClip::LogSupersede(int supersede)
{
  std::string tmp;

  if (supersede & NO_SUPERSEDE)
    tmp.append("NO_SUPERSEDE");
  else
  {
    if (supersede & AUDIO_RETURN)
      tmp.append("AUDIO_RETURN ");

    if (supersede & VIDEO_RETURN)
      tmp.append("VIDEO_RETURN ");

    if (supersede & AUDIO_FILL)
      tmp.append("AUDIO_FILL ");

    if (supersede & VIDEO_FILL)
      tmp.append("VIDEO_FILL");
  }

  LogDebug("Supersede clip %d, %d = %s", nPlaylist, nClip, tmp.c_str());
}

