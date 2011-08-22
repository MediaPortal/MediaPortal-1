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

#define BYTES_PER_SEC 192000
#define AUDIO_DATA_32ms 262140
#define FAKE_AUDIO_DURATION 320000LL

#define MAX_VIDEO_DRIFT 5000000LL

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
  bSeekNeeded = seekNeeded;
  if (bSeekNeeded)
  {
    clipPlaylistOffset = firstPacketTime;
  }
}


CClip::~CClip(void)
{
  FlushAudio();
  FlushVideo();
}

Packet* CClip::ReturnNextAudioPacket(REFERENCE_TIME playlistOffset)
{
  Packet* ret=NULL;
  if (!noAudio && !firstAudio && m_vecClipAudioPackets.size()==0)
  {
    LogDebug("Swithching to fake Audio for clip %d",nClip);
    noAudio=true;
  }
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
  if (firstAudio && ret)
  {
    ret->bDiscontinuity=true;
    firstAudio=false;
  }
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
  return ret;
}

bool CClip::FakeAudioAvailable()
{
  return (audioPlaybackpoint<lastVideoPosition);
}


Packet* CClip::GenerateFakeAudio(REFERENCE_TIME rtStart)
{
  if (rtStart>playlistFirstPacketTime+clipDuration) superceeded|=SUPERCEEDED_AUDIO;
  if (superceeded&SUPERCEEDED_AUDIO) return NULL;
  if (!FakeAudioAvailable()) return NULL;

//  if (audioPlaybackpoint>lastVideoPosition) return NULL;

  Packet* packet = new Packet();
    
  packet->nClipNumber = nClip;
  //packet->nPlaylist = nPlaylist; set by the playlist
    
  BYTE* data = (BYTE*)malloc(AUDIO_DATA_32ms);
  memset(data, 0, AUDIO_DATA_32ms);

  data[0] = 0x0B; // sync word
  data[1] = 0x77;
  data[2] = 0xF9; // CRC
  data[3] = 0x02;
  data[4] = 0x16; // params

  packet->SetCount(AUDIO_DATA_32ms);
  packet->SetData(data, AUDIO_DATA_32ms);
  packet->rtStart = rtStart;
  packet->rtStop = packet->rtStart + 1;

  free(data);

  CMediaType pmt;
  pmt.InitMediaType();
  pmt.SetType(&MEDIATYPE_Audio);
  pmt.SetSubtype(&MEDIASUBTYPE_DOLBY_AC3);
  pmt.SetSampleSize(1);
  pmt.SetTemporalCompression(FALSE);
  pmt.SetVariableSize();
  pmt.SetFormatType(&FORMAT_WaveFormatEx);
  pmt.SetFormat(AC3AudioFormat, sizeof(AC3AudioFormat));
  packet->pmt = CreateMediaType(&pmt);
  audioPlaybackpoint+=FAKE_AUDIO_DURATION;
  lastAudioPosition+=FAKE_AUDIO_DURATION;
  return packet;
}

bool CClip::AcceptAudioPacket(Packet*  packet, bool forced)
{
  bool ret=false;
  if (forced)
  {
    if (packet->nClipNumber != nClip)
    {
      // Oh dear, not for this clip so throw it away
      delete packet;
    }
    else
    {
      packet->nClipNumber=nClip;
      m_vecClipAudioPackets.push_back(packet);
      lastAudioPosition=packet->rtStart;
      noAudio=false;
    }
    return true;
  }
  if (noAudio) return false;
  if (!((superceeded&SUPERCEEDED_AUDIO)==SUPERCEEDED_AUDIO))
  {
    //new packet must be after the last for audio
    if (packet->rtStart >=lastAudioPosition)
    {
      //make sure it's in range for this clip
      if (packet->rtStart < playlistFirstPacketTime + clipDuration + 10000000LL/*1 sec for bad mastering*/)
      {
        //belongs to this playlist
        packet->nClipNumber=nClip;
        m_vecClipAudioPackets.push_back(packet);
        lastAudioPosition=packet->rtStart;
        ret=true;
      }
    }
  }
  return ret;
}

bool CClip::AcceptVideoPacket(Packet*  packet, bool forced)
{
  bool ret=false;
  //check if this clip is looping (Fixes some menus which repeat a clip)
  if (!firstVideo && packet->rtStart == playlistFirstPacketTime) bSeekNeeded = true;
  if (forced)
  {
    if (packet->nClipNumber != nClip)
    {
      // Oh dear, not for this clip so throw it away
      delete packet;
    }
    else
    {
      packet->nClipNumber=nClip;
      packet->bSeekRequired = bSeekNeeded;
      bSeekNeeded = false;
      m_vecClipVideoPackets.push_back(packet);
      lastVideoPosition=packet->rtStart;
    }
    return true;
  }
  if (!((superceeded&SUPERCEEDED_VIDEO)==SUPERCEEDED_VIDEO))
  {
    //this will be hard to get right...
    if (packet->rtStart >= playlistFirstPacketTime || packet->rtStart==Packet::INVALID_TIME)
    {
      //make sure it's in range for this clip
      if (packet->rtStart <= playlistFirstPacketTime + clipDuration || packet->rtStart==Packet::INVALID_TIME)
      {
        //belongs to this playlist
        packet->nClipNumber=nClip;
        packet->bSeekRequired = bSeekNeeded;
        bSeekNeeded = false;
        m_vecClipVideoPackets.push_back(packet);
        lastVideoPosition=packet->rtStart;
        ret=true;
      }
    }
  }
  return ret;
}

void CClip::Superceed(int superceedType)
{
  superceeded|=superceedType;
}
bool CClip::IsSuperceeded(int superceedType)
{
  return ((superceeded&superceedType)==superceedType);
}

void CClip::FlushAudio(void)
{
  ivecAudioBuffers ita = m_vecClipAudioPackets.begin();
  while (ita!=m_vecClipAudioPackets.end())
  {
    Packet * packet=*ita;
    ita=m_vecClipAudioPackets.erase(ita);
    delete packet;
  }
}
void CClip::FlushVideo(void)
{
  ivecVideoBuffers itv = m_vecClipVideoPackets.begin();
  while (itv!=m_vecClipVideoPackets.end())
  {
    Packet * packet=*itv;
    itv=m_vecClipVideoPackets.erase(itv);
    delete packet;
  }
}
int CClip::AudioPacketCount()
{
  if (IsSuperceeded(SUPERCEEDED_AUDIO))
  {
    return 0;
  }
  else
  {
    if (noAudio) return 0;
    return m_vecClipAudioPackets.size();
  }
}
int CClip::VideoPacketCount()
{
  if (IsSuperceeded(SUPERCEEDED_VIDEO))
  {
    return 0;
  }
  else
  {
    return m_vecClipVideoPackets.size();
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
}

bool CClip::HasAudio()
{
  if (m_vecClipAudioPackets.size()>0) return true;
  if (noAudio && FakeAudioAvailable()) return true;
  return false;
}

bool CClip::HasVideo()
{
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
