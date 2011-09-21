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

extern void LogDebug(const char *fmt, ...);

CClip::CClip(int clipNumber, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, bool seekNeeded)
{
  playlistFirstPacketTime=firstPacketTime;
  lastVideoPosition=playlistFirstPacketTime;
  nClip=clipNumber;
  lastAudioPosition = playlistFirstPacketTime;
  clipDuration=duration;
  noAudio=!audioPresent;
  superceeded=0;
  clipFilled=false;
  clipEmptied=false;
  audioPlaybackpoint=firstPacketTime;
  clipPlaylistOffset=clipOffset;
  firstAudio=true;
  firstVideo=true;
  bSeekNeededVideo = seekNeeded;
  bSeekNeededAudio = seekNeeded;
  if (seekNeeded)
  {
    clipPlaylistOffset = firstPacketTime;
  }
  m_videoPmt=NULL;
  m_audioPmt=NULL;
}


CClip::~CClip(void)
{
  FlushAudio();
  FlushVideo();
  DeleteMediaType(m_videoPmt);
  DeleteMediaType(m_audioPmt);
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
    if (ret && bSeekNeededAudio)
    {
      LogDebug("Setting bSeekRequired for fake Audio");
      ret->bSeekRequired = true;
      bSeekNeededAudio = false;
    }
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
  if (firstAudio && ret)
  {
    ret->bDiscontinuity=true;
    firstAudio=false;
  }
  
  if (ret && !noAudio) ret->pmt = CreateMediaType(m_audioPmt);
  
  //LogDebug("Clip: aud: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

Packet* CClip::ReturnNextVideoPacket(REFERENCE_TIME playlistOffset)
{
  Packet* ret=NULL;
  if (m_vecClipVideoPackets.size()>0)
  {
    ivecVideoBuffers it = m_vecClipVideoPackets.begin();
    ret=*it;
    it=m_vecClipVideoPackets.erase(it);
  }
  if (firstVideo && ret)
  {
    ret->bDiscontinuity=true;
    firstVideo=false;
  }

  if (ret) ret->pmt = CreateMediaType(m_videoPmt);
  
  //LogDebug("Clip: vid: return Packet rtStart: %I64d offset: %I64d seekRequired %d",ret->rtStart, ret->rtOffset,ret->bSeekRequired);
  return ret;
}

bool CClip::FakeAudioAvailable()
{
  return (audioPlaybackpoint<lastVideoPosition);
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
  //check if this clip is looping (Fixes some menus which repeat a clip)
  if (!firstAudio && packet->rtStart == playlistFirstPacketTime) bSeekNeededAudio = true;
  if (packet->nClipNumber != nClip)
  {
    // Oh dear, not for this clip so throw it away
    delete packet;
  }
  else
  {
    packet->nClipNumber=nClip;
    if (packet->rtStart!=Packet::INVALID_TIME)
    {
      if (bSeekNeededAudio) 
        LogDebug("Setting bSeekRequired for audio");
      packet->bSeekRequired = bSeekNeededAudio;
      bSeekNeededAudio = false;
    }
    m_vecClipAudioPackets.push_back(packet);
    lastAudioPosition=packet->rtStart;
    noAudio=false;
  }
  return true;
}

bool CClip::AcceptVideoPacket(Packet*  packet)
{
  //check if this clip is looping (Fixes some menus which repeat a clip)
  if (!firstVideo && packet->rtStart == playlistFirstPacketTime) bSeekNeededVideo = true;
  if (packet->nClipNumber != nClip)
  {
    // Oh dear, not for this clip so throw it away
    delete packet;
  }
  else
  {
    packet->nClipNumber=nClip;
    if (packet->rtStart!=Packet::INVALID_TIME)
    {
      if (bSeekNeededVideo) 
        LogDebug("Setting bSeekRequired for video");
      packet->bSeekRequired = bSeekNeededVideo;
      bSeekNeededVideo = false;
    }
    m_vecClipVideoPackets.push_back(packet);
    lastVideoPosition=packet->rtStart;
  }
  return true;
}

void CClip::Superceed(int superceedType)
{
  superceeded|=superceedType;
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

void CClip::Reset()
{
  FlushAudio();
  FlushVideo();
  lastVideoPosition=playlistFirstPacketTime;
  lastAudioPosition = playlistFirstPacketTime;
  superceeded=0;
  clipFilled=false;
  clipEmptied=false;
  audioPlaybackpoint=playlistFirstPacketTime;
  firstAudio=true;
  firstVideo=true;
  bSeekNeededAudio=true;
  bSeekNeededVideo=true;
}

bool CClip::HasAudio()
{
  if (m_vecClipAudioPackets.size()>0 && m_audioPmt) return true;
  if (noAudio && FakeAudioAvailable()) return true;
  return false;
}

bool CClip::HasVideo()
{
  if (firstVideo && m_vecClipVideoPackets.size()<3) return false;
  if (!m_videoPmt) return false;
  if (m_vecClipVideoPackets.size()>0) return true;
  return false;
}

bool CClip::Incomplete()
{
  bool ret = false;
  if (lastAudioPosition < (playlistFirstPacketTime + clipDuration - 5000000LL))
  {
    ret = true;
    
    LogDebug("clip: Incomplete - nClip: %d lastAudioPosition: %I64d first: %I64d duration: %I64d", 
      nClip, lastAudioPosition, playlistFirstPacketTime, clipDuration);
  }
  return ret;
}

void CClip::SetVideoPMT(AM_MEDIA_TYPE *pmt)
{
  if (m_videoPmt)DeleteMediaType(m_videoPmt);
  m_videoPmt = CreateMediaType(pmt);
}

void CClip::SetAudioPMT(AM_MEDIA_TYPE *pmt)
{
  if (m_audioPmt)DeleteMediaType(m_audioPmt);
  m_audioPmt = CreateMediaType(pmt);
}
