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

CClip::CClip(int clipNumber, int playlistNumber, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffset, REFERENCE_TIME playlistOffset, bool audioPresent, REFERENCE_TIME duration, bool seekNeeded)
{
  playlistFirstPacketTime=firstPacketTime;
  lastVideoPosition=playlistFirstPacketTime;
  nClip=clipNumber;
  nPlaylist = playlistNumber;
  lastAudioPosition = playlistFirstPacketTime;
  audioPlaybackPosition = playlistFirstPacketTime;
  clipDuration=duration;
  noAudio=!audioPresent;
  superceeded=0;
  audioPlaybackpoint = firstPacketTime;
  clipPlaylistOffset = firstPacketTime - playlistOffset;
  firstAudio=true;
  firstVideo=true;
  bSeekNeededVideo = false;
  bSeekNeededAudio = false;
  m_videoPmt=NULL;
  m_playlistOffset = clipOffset;
  firstAudioPosition = playlistFirstPacketTime;
  firstVideoPosition = playlistFirstPacketTime;
  m_rtClipStartingOffset = 0;
  firstPacketAccepted = false;
}

CClip::~CClip(void)
{
  FlushAudio();
  FlushVideo();
  DeleteMediaType(m_videoPmt);
}

Packet* CClip::ReturnNextAudioPacket(REFERENCE_TIME playlistOffset)
{
  Packet* ret=NULL;

  // Disabled for now - causes issues with clip changes on rare situatios.
  // we should't need this since audio and video pins are syncronized for the
  // playlist boundaries. Also Seamless BDs could have issues when clip
  // joints are having extra data.

  /*if (!noAudio && !firstAudio && m_vecClipAudioPackets.size()==0)
  {
    LogDebug("Swithching to fake Audio for clip %d",nClip);
    firstAudio = true;
    noAudio = true;
  }*/

  if (noAudio)
  {
    ret=GenerateFakeAudio(audioPlaybackpoint);
  }
  else
  {
    if (m_vecClipAudioPackets.size()>0)
    {
      ivecAudioBuffers it = m_vecClipAudioPackets.begin();
      ret=*it;
      audioPlaybackpoint=ret->rtStart+FAKE_AUDIO_DURATION;//FAKE_AUDIO_DURATION is a proxy for the real duration (currently)
      it=m_vecClipAudioPackets.erase(it);
    }
  }
  if (firstAudio && ret && ret->rtStart!=Packet::INVALID_TIME)
  {
    ret->bDiscontinuity=true;
    firstAudio=false;
    firstAudioPosition = ret->rtStart;
  }
  
  if (ret && ret->rtStart!=Packet::INVALID_TIME)
  {
    ret->rtPlaylistTime = ret->rtStart - m_playlistOffset;
    ret->rtClipTime = m_rtClipStartingOffset;
    audioPlaybackPosition = ret->rtStart;
  }
  if (bSeekNeededAudio && ret->rtStart!=Packet::INVALID_TIME)
  {
    LogDebug("Setting bSeekRequired Audio on (%d,%d)", nPlaylist, nClip);
    ret->bSeekRequired = true;
    bSeekNeededAudio = false;
  }

//  LogDebug("Clip: aud: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

Packet* CClip::ReturnNextVideoPacket(REFERENCE_TIME playlistOffset)
{
  Packet* ret=NULL;
  if (m_vecClipVideoPackets.size()>0 && m_videoPmt)
  {
    ivecVideoBuffers it = m_vecClipVideoPackets.begin();
    ret=*it;
    it=m_vecClipVideoPackets.erase(it);
  }
  if (firstVideo && ret && ret->rtStart!=Packet::INVALID_TIME)
  {
    ret->bDiscontinuity=true;
    firstVideo=false;
    firstVideoPosition = ret->rtStart;
  }

  if (ret) ret->pmt = CreateMediaType(m_videoPmt);
  
  if (ret && ret->rtStart!=Packet::INVALID_TIME)
  {
    ret->rtPlaylistTime = ret->rtStart - m_playlistOffset;
    ret->rtClipTime = m_rtClipStartingOffset;
    if (ret->rtStart > videoPlaybackPosition) videoPlaybackPosition = ret->rtStart;
  }

  if (bSeekNeededVideo && ret->rtStart!=Packet::INVALID_TIME)
  {
    LogDebug("Setting bSeekRequired Video on (%d,%d)", nPlaylist, nClip);
    ret->bSeekRequired = true;
    bSeekNeededVideo = false;
  }

//  LogDebug("Clip: vid: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

bool CClip::FakeAudioAvailable()
{
  return audioPlaybackpoint <= playlistFirstPacketTime + clipDuration;
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
  audioPlaybackpoint+=FAKE_AUDIO_DURATION;
  lastAudioPosition+=FAKE_AUDIO_DURATION;
  return packet;
}

bool CClip::AcceptAudioPacket(Packet* packet)
{
  if (nPlaylist != packet->nPlaylist) return false;
  if (packet->nClipNumber != nClip)
  {
    // Oh dear, not for this clip so throw it away
    delete packet;
  }
  else
  {
    if (!firstPacketAccepted)
    {
      if (abs(packet->rtStart - playlistFirstPacketTime - m_rtClipStartingOffset) > 10000000)
      {
        firstPacketAccepted = true;
        m_rtClipStartingOffset = packet->rtStart - playlistFirstPacketTime;
      }
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
  if (packet->nClipNumber != nClip)
  {
    LogDebug("Packet %64d (%d,%d) deleted in (%d,%d)",packet->rtStart, packet->nPlaylist, packet->nClipNumber, nPlaylist, nClip);
    // Oh dear, not for this clip so throw it away
    delete packet;
  }
  else
  {
    if (!firstPacketAccepted && packet->rtStart != Packet::INVALID_TIME)
    {
      if (abs(packet->rtStart - playlistFirstPacketTime - m_rtClipStartingOffset) > 10000000)
      {
        firstPacketAccepted = true;
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
  LogDebug("Superceed clip %d,%d = %4X", nPlaylist, nClip, superceeded);
  if ((superceedType == SUPERCEEDED_AUDIO_FILL) && firstAudio) 
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

REFERENCE_TIME CClip::Reset(REFERENCE_TIME rtClipStartPoint)
{
  LogDebug("Reseting (%d,%d)", nPlaylist, nClip);
  REFERENCE_TIME ret = PlayedDuration();
  clipPlaylistOffset -= ret;
  FlushAudio();
  FlushVideo();
  lastVideoPosition = playlistFirstPacketTime;
  lastAudioPosition = playlistFirstPacketTime;
  superceeded=0;
  audioPlaybackpoint = playlistFirstPacketTime;
  clipPlaylistOffset = playlistFirstPacketTime;
  audioPlaybackPosition = rtClipStartPoint;
  firstAudio=true;
  firstVideo=true;
//  bSeekNeededAudio=true;
//  bSeekNeededVideo=true;
  firstAudioPosition = _I64_MAX;
  firstVideoPosition = _I64_MAX;
  m_rtClipStartingOffset = rtClipStartPoint;
  firstPacketAccepted = false;
  return ret;
}

bool CClip::HasAudio()
{
  if (!m_videoPmt) return false;
  if (m_vecClipAudioPackets.size()>0) return true;
  if (noAudio) 
  {
    if (FakeAudioAvailable()) return true;
    else if (!IsSuperceeded(SUPERCEEDED_AUDIO_RETURN)) Superceed(SUPERCEEDED_AUDIO_RETURN);
  }
  return false;
}

bool CClip::HasVideo()
{
//  if (!noAudio  && firstAudio ) return false;
  if (!m_videoPmt) return false;
  if (m_vecClipVideoPackets.size()>0) return true;
  return false;
}

REFERENCE_TIME CClip::Incomplete()
{
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
  REFERENCE_TIME
    start=playlistFirstPacketTime - m_rtClipStartingOffset, 
    finish=audioPlaybackPosition,
    playDuration;
  if (audioPlaybackPosition < videoPlaybackPosition)
  {
    finish = videoPlaybackPosition;
  }
  playDuration = finish - playlistFirstPacketTime;
  if (abs(clipDuration - playDuration) < 5000000LL) 
  {
//    LogDebug("Clip::Duration 1 %I64d %I64d %I64d ", clipDuration - m_rtClipStartingOffset, clipDuration, m_rtClipStartingOffset);
    return clipDuration - m_rtClipStartingOffset;
  }
//  LogDebug("Clip::Duration 2 %I64d%I64d %I64d ", finish - start, finish, start);
  return finish - start;
}


void CClip::SetVideoPMT(AM_MEDIA_TYPE *pmt, bool changingMediaType)
{
  bSeekNeededVideo |= changingMediaType;
  bSeekNeededAudio |= changingMediaType;
  if (m_videoPmt) DeleteMediaType(m_videoPmt);
  if (changingMediaType)
  {
    LogDebug("Clip::Changing video pmt causing stream Reset");
    bSeekNeededVideo = true;
    bSeekNeededAudio = true;
    clipPlaylistOffset = playlistFirstPacketTime;
  }
  m_videoPmt = CreateMediaType(pmt);
}

