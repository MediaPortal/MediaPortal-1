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
#define ONE_SECOND  10000000LL
#define TWO_SECONDS 20000000LL

CClip::CClip(int clipNumber, int playlistNumber, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffset, REFERENCE_TIME totalStreamOffset, bool audioPresent, REFERENCE_TIME duration)
{
  nClip=clipNumber;
  nPlaylist = playlistNumber;

  playlistFirstPacketTime=firstPacketTime;

  lastVideoPosition=playlistFirstPacketTime;
  lastAudioPosition = playlistFirstPacketTime;
  audioPlaybackPosition = playlistFirstPacketTime;
  videoPlaybackPosition = playlistFirstPacketTime;

  earliestPacketAccepted = _I64_MAX;

  clipDuration=duration;
  clipPlaylistOffset = totalStreamOffset;

  m_playlistOffset = clipOffset;
  m_rtClipStartingOffset = 0LL;

  noAudio=!audioPresent;

  superceeded=0;

  m_videoPmt=NULL;

  firstAudio=true;
  firstVideo=true;
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
  Packet* ret=NULL;

  if (!firstPacketReturned)
  {
//    CAutoLock lock (&m_sectionRead);
    firstPacketReturned=true;
    if (abs(earliestPacketAccepted - playlistFirstPacketTime - m_rtClipStartingOffset) > ONE_SECOND)
    {
      m_rtClipStartingOffset = earliestPacketAccepted - playlistFirstPacketTime;
    }
    return ReturnNextAudioPacket(playlistOffset);
  }

  if (noAudio)
  {
    ret=GenerateFakeAudio(audioPlaybackPosition);
    if (earliestPacketAccepted == INT64_MAX)
      earliestPacketAccepted = audioPlaybackPosition;
  }
  else
  {
    if (m_vecClipAudioPackets.size()>0)
    {
      ivecAudioBuffers it = m_vecClipAudioPackets.begin();
      ret=*it;
      audioPlaybackPosition=ret->rtStart;
      it=m_vecClipAudioPackets.erase(it);
    }
  }
  if (ret && ret->rtStart!=Packet::INVALID_TIME)
  {
    if (firstAudio)
    {
      ret->bDiscontinuity=true;
      firstAudio=false;
      ret->bNewClip = true;
    }
  
    ret->rtPlaylistTime = ret->rtStart - m_playlistOffset;
    ret->rtClipStartTime = ret->rtStart -  earliestPacketAccepted;
    ret->rtStart += clipPlaylistOffset - earliestPacketAccepted;
    ret->rtStop += clipPlaylistOffset - earliestPacketAccepted;
  }

//  LogDebug("Clip: aud: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

Packet* CClip::ReturnNextVideoPacket(REFERENCE_TIME playlistOffset)
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
  if (!firstPacketReturned)
  {
//    CAutoLock lock (&m_sectionRead);
    firstPacketReturned=true;
    return ReturnNextVideoPacket(playlistOffset);
  }
  Packet* ret=NULL;
  if (m_vecClipVideoPackets.size()>0 && m_videoPmt)
  {
    ivecVideoBuffers it = m_vecClipVideoPackets.begin();
    ret=*it;
    it=m_vecClipVideoPackets.erase(it);
  }
  if (ret)
  {
    if (ret->rtStart!=Packet::INVALID_TIME)
    {
      if (firstVideo)
      {
        ret->bDiscontinuity=true;
        ret->bNewClip = true;
        firstVideo=false;
        ret->pmt = CreateMediaType(m_videoPmt);
      }

      if (ret->rtStart > videoPlaybackPosition) 
      {
        videoPlaybackPosition = ret->rtStart;
        //LogDebug("Videoplayback position (%d,%d) %I64d", nPlaylist, nClip, videoPlaybackPosition);
      }
  
      ret->rtPlaylistTime = ret->rtStart - m_playlistOffset;
      ret->rtClipStartTime = ret->rtStart -  earliestPacketAccepted;
      ret->rtStart += clipPlaylistOffset - earliestPacketAccepted;
      ret->rtStop += clipPlaylistOffset  - earliestPacketAccepted;
    }
  }
//  LogDebug("Clip: vid: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

bool CClip::FakeAudioAvailable()
{
  return audioPlaybackPosition <= playlistFirstPacketTime + clipDuration;
}

Packet* CClip::GenerateFakeAudio(REFERENCE_TIME rtStart)
{
  if (rtStart>playlistFirstPacketTime+clipDuration)superceeded|=SUPERCEEDED_AUDIO_RETURN;
  if (superceeded&SUPERCEEDED_AUDIO_RETURN) return NULL;
  if (!FakeAudioAvailable()) return NULL;

  Packet* packet = new Packet();
  packet->nClipNumber = nClip;
    
  packet->SetCount(AC3_FRAME_LENGTH);
  packet->SetData(ac3_sample, AC3_FRAME_LENGTH);
  packet->rtStart = rtStart;
  packet->rtStop = packet->rtStart + 1;

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
  wfe->Format.nChannels=6;
  wfe->Format.nSamplesPerSec=48000;
  wfe->Format.wFormatTag = WAVE_FORMAT_DOLBY_AC3;

  packet->pmt = CreateMediaType(&pmt);
  audioPlaybackPosition+=FAKE_AUDIO_DURATION;
  lastAudioPosition+=FAKE_AUDIO_DURATION;
  return packet;
}

bool CClip::AcceptAudioPacket(Packet* packet)
{
  CAutoLock vectorALock(&m_sectionVectorAudio);
  if (nPlaylist != packet->nPlaylist) return false;
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
    packet->nClipNumber=nClip;
    m_vecClipAudioPackets.push_back(packet);
    
    if (packet->rtStart != Packet::INVALID_TIME)
      lastAudioPosition=packet->rtStart;
    
    noAudio=false;
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
    if (!firstPacketReturned && packet->rtStart != Packet::INVALID_TIME)
    {
      if (earliestPacketAccepted > packet->rtStart) earliestPacketAccepted = packet->rtStart;
    }
    if (!firstPacketAccepted && packet->rtStart != Packet::INVALID_TIME)
    {
      firstPacketAccepted = true;
      if (abs(packet->rtStart - playlistFirstPacketTime - m_rtClipStartingOffset) > TWO_SECONDS)
      {
        m_rtClipStartingOffset = packet->rtStart - playlistFirstPacketTime;
      }
    }
    m_vecClipVideoPackets.push_back(packet);
    if (packet->rtStart != Packet::INVALID_TIME)
      lastVideoPosition=packet->rtStart;
  }
  return true;
}

void CClip::Superceed(int superceedType)
{
  superceeded|=superceedType;
//  LogDebug("Superceed clip %d,%d = %4X", nPlaylist, nClip, superceeded);
  if ((superceedType == SUPERCEEDED_AUDIO_FILL) && firstAudio && !firstVideo) 
  {
    LogDebug("Setting Fake Audio for clip %d", nClip);
    noAudio = true;
  }
}

bool CClip::IsSuperceeded(int superceedType)
{
  return ((superceeded&superceedType)==superceedType);
}

void CClip::FlushAudio(Packet* pPacketToKeep)
{
  CAutoLock vectorALock(&m_sectionVectorAudio);
  ivecAudioBuffers ita = m_vecClipAudioPackets.begin();
  while (ita!=m_vecClipAudioPackets.end())
  {
    Packet * packet=*ita;
    if (packet!=pPacketToKeep)
    {
      ita=m_vecClipAudioPackets.erase(ita);
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
  while (itv!=m_vecClipVideoPackets.end())
  {
    Packet * packet=*itv;
    if (packet!=pPacketToKeep)
    {
      itv=m_vecClipVideoPackets.erase(itv);
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
  clipPlaylistOffset = totalStreamOffset;
  audioPlaybackPosition = playlistFirstPacketTime;
  videoPlaybackPosition = playlistFirstPacketTime;
  m_rtClipStartingOffset = 0LL;

  earliestPacketAccepted = INT64_MAX;

  superceeded=0;

  firstAudio=true;
  firstVideo=true;
  firstPacketAccepted = false;
  firstPacketReturned = false;
  clipReset = true;
}

bool CClip::HasAudio()
{
  CAutoLock vectorALock(&m_sectionVectorAudio);
  if (!m_videoPmt) return false;
  if (m_vecClipAudioPackets.size()>0) return true;
  if (noAudio) 
  {
    if (FakeAudioAvailable()) return true;
    else if (!IsSuperceeded(SUPERCEEDED_AUDIO_RETURN)) Superceed(SUPERCEEDED_AUDIO_RETURN);
  }
  return false;
}

//
bool CClip::HasVideo()
{
  CAutoLock vectorVLock(&m_sectionVectorVideo);
//  if (!noAudio  && firstAudio ) return false;
  if (!m_videoPmt) return false;
  if (firstVideo && !IsSuperceeded(SUPERCEEDED_VIDEO_FILL) && m_vecClipVideoPackets.size() < 1) return false;
  if (m_vecClipVideoPackets.size()>0) return true;
  return false;
}

REFERENCE_TIME CClip::Incomplete()
{
  // clip not played so not incomplete
  if (!firstPacketReturned || !firstPacketAccepted) return 0LL;

  REFERENCE_TIME ret = playlistFirstPacketTime + clipDuration - videoPlaybackPosition;
  if (playlistFirstPacketTime + clipDuration - audioPlaybackPosition < ret) ret = playlistFirstPacketTime + clipDuration - audioPlaybackPosition; 
  if (ret > 5000000LL)
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
  REFERENCE_TIME playDuration=0LL;
  LogDebug("CClip::earliestPacketAccepted %I64d audioPlaybackPosition %I64d videoPlaybackPosition %I64d %d %d", earliestPacketAccepted, audioPlaybackPosition, videoPlaybackPosition, firstAudio, firstVideo);
  if (!firstPacketReturned || !firstPacketAccepted) 
  {
    LogDebug("CClip::PlayedDuration 0 - clip unplayed");
    return 0LL;
  }
  if (audioPlaybackPosition < videoPlaybackPosition)
  {
    finish = videoPlaybackPosition;
  }
  playDuration = finish - playlistFirstPacketTime;
  if (abs(clipDuration - playDuration) < HALF_SECOND) 
  {
    LogDebug("CClip::PlayedDuration %I64d - clip played to end", clipDuration - earliestPacketAccepted + playlistFirstPacketTime);
    return clipDuration - earliestPacketAccepted + playlistFirstPacketTime;
  }
  if (earliestPacketAccepted>finish)
  {
    return 0LL;
  }
  LogDebug("CClip::PlayedDuration %I64d - clip (%d,%d) partially played finish %I64d start %I64d", finish - earliestPacketAccepted, nPlaylist, nClip, finish, earliestPacketAccepted);
  return finish - earliestPacketAccepted;
}


void CClip::SetVideoPMT(AM_MEDIA_TYPE *pmt)
{
  if (m_videoPmt) DeleteMediaType(m_videoPmt);
  m_videoPmt = CreateMediaType(pmt);
}
