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

#include "Playlist.h"

extern void LogDebug(const char *fmt, ...);

CPlaylist::CPlaylist(int playlistNumber, REFERENCE_TIME firstPacketTime)
{
  Reset(playlistNumber, firstPacketTime);
}

CPlaylist::~CPlaylist(void)
{
  if (m_vecClips.size()>0)
  {
    ivecClip it = m_vecClips.begin();
    while (it!=m_vecClips.end())
    {
      CClip * clip=*it;
      it=m_vecClips.erase(it);
      delete clip;
    }
  }
}

Packet* CPlaylist::ReturnNextAudioPacket()
{
  firstPacketRead=true;
  firstAudioPESPacketSeen=true;
  if (m_currentAudioPlayBackClip==NULL)
  {
    LogDebug("m_currentAudioPlayBackClip is NULL");
    m_currentAudioPlayBackClip=*m_vecClips.begin();
  }
  Packet * ret = m_currentAudioPlayBackClip->ReturnNextAudioPacket(playlistFirstPacketTime);
  if (ret)
  {
    ret->nPlaylist=nPlaylist;
    CorrectTimeStamp(m_currentAudioPlayBackClip,ret);
  }
  else
  {
    CClip* nextAudioClip = GetNextAudioClip(m_currentAudioPlayBackClip, SUPERCEEDED_AUDIO_RETURN);
    if (m_currentAudioPlayBackClip!=nextAudioClip)
    {
      LogDebug("Moving to %d,%d",this->nPlaylist,nextAudioClip->nClip);
      m_currentAudioPlayBackClip->Superceed(SUPERCEEDED_AUDIO_RETURN);
      m_currentAudioPlayBackClip=nextAudioClip;
      ret=ReturnNextAudioPacket();
    }
  }
  return ret;
}

Packet* CPlaylist::ReturnNextAudioPacket(int clip)
{
  Packet* ret=NULL;
  if (m_currentAudioPlayBackClip==NULL)
  {
    LogDebug("m_currentAudioPlayBackClip is NULL");
    m_currentAudioPlayBackClip=*m_vecClips.begin();
  }
  if (m_currentAudioPlayBackClip->nClip==clip)
  {
    ret = m_currentAudioPlayBackClip->ReturnNextAudioPacket(playlistFirstPacketTime);
    if (ret!=NULL)
    {
      ret->nPlaylist=nPlaylist;
      CorrectTimeStamp(m_currentAudioPlayBackClip,ret);
    }
  }
  if (ret) firstPacketRead=true;
  return ret;
}

Packet* CPlaylist::ReturnNextVideoPacket()
{
  firstPacketRead=true;
  Packet * ret = m_currentVideoPlayBackClip->ReturnNextVideoPacket(playlistFirstPacketTime);
  if (ret)
  {
    ret->nPlaylist=nPlaylist;
    CorrectTimeStamp(m_currentVideoPlayBackClip,ret);
  }
  else
  {
    CClip* nextVideoClip = GetNextVideoClip(m_currentVideoPlayBackClip,SUPERCEEDED_VIDEO_RETURN);
    if (m_currentVideoPlayBackClip!=nextVideoClip)
    {
      m_currentVideoPlayBackClip->Superceed(SUPERCEEDED_VIDEO_RETURN);
      m_currentVideoPlayBackClip=nextVideoClip;
      ret=ReturnNextVideoPacket();
    }
  }
  return ret;
}

int CPlaylist::CurrentAudioSubmissionClip()
{
  if (m_currentAudioSubmissionClip==NULL) return -1;
  return m_currentAudioSubmissionClip->nClip;
}

int CPlaylist::CurrentVideoSubmissionClip()
{
  if (m_currentVideoSubmissionClip==NULL) return -1;
  return m_currentVideoSubmissionClip->nClip;
}

bool CPlaylist::AcceptAudioPacket(Packet*  packet, bool seeking)
{
  bool ret = true;
  if (!m_currentAudioSubmissionClip) return false;
  REFERENCE_TIME prevAudioPosition = m_currentAudioSubmissionClip->lastAudioPosition;
  if (m_currentAudioSubmissionClip->nClip == packet->nClipNumber)
  {
    m_currentAudioSubmissionClip->AcceptAudioPacket(packet);
  }
  else 
  {
    bool complete=false;
    while (!complete)
    {
      CClip* nextSubmissionClip = GetNextAudioClip(m_currentAudioSubmissionClip, SUPERCEEDED_AUDIO_FILL);
      if (nextSubmissionClip->nClip==packet->nClipNumber)
      {
        complete=true;
      }
      else if (m_currentAudioSubmissionClip==nextSubmissionClip)
      {
        LogDebug("Playlist::Audio Clip %d not found in playlist %d",packet->nClipNumber, this->nPlaylist); 
        return false;
      }
      m_currentAudioSubmissionClip=nextSubmissionClip;
    }
    ret=AcceptAudioPacket(packet, seeking);
  }

  bool discontinuity = false;

  if (packet->rtStart != Packet::INVALID_TIME && prevAudioPosition != Packet::INVALID_TIME && abs(prevAudioPosition - packet->rtStart) > 10000000)
  {
    LogDebug("clip: audio stream's discontinuity detected: old: %I64d new: %I64d", prevAudioPosition, packet->rtStart);
    firstAudioPESPacketSeen=false;
    discontinuity=true;
  }

  if (!firstAudioPESPacketSeen && ret && packet->rtStart!=Packet::INVALID_TIME)
  {
    REFERENCE_TIME oldPEStime = firstAudioPESTimeStamp;

    firstAudioPESPacketSeen=true;
    firstAudioPESTimeStamp= m_currentAudioSubmissionClip->clipPlaylistOffset - packet->rtStart;
    
    packet->rtOffset = (0 - firstAudioPESTimeStamp) > 10000000 || !discontinuity ? 0 - firstAudioPESTimeStamp : 0;
    if (packet->rtOffset != 0)
      packet->bSeekRequired=!seeking;

    m_currentAudioSubmissionClip->FlushAudio(packet);
    LogDebug("clip: first Packet (aud) %I64d old: %I64d new: %I64d seekRequired %d", packet->rtStart, oldPEStime, firstAudioPESTimeStamp, packet->bSeekRequired);
  }
  if (!firstPacketRead && ret && packet->rtStart!=Packet::INVALID_TIME && firstAudioPESTimeStamp > m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart)
  {
    firstAudioPESTimeStamp=m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart;
  } 

  return ret;
}

bool CPlaylist::AcceptVideoPacket(Packet* packet, bool firstPacket, bool seeking)
{
  bool ret = true;
  REFERENCE_TIME prevVideoPosition = 0;
  if (!m_currentVideoSubmissionClip) 
  {
    LogDebug("m_currentVideoSubmissionClip is NULL");
    ret=false;
  }
  else
  {
    if (m_currentVideoSubmissionClip->nClip==packet->nClipNumber)
    {
      prevVideoPosition = m_currentVideoSubmissionClip->lastVideoPosition; 
      ret=m_currentVideoSubmissionClip->AcceptVideoPacket(packet);
    }
    else
    {
      while (m_currentVideoSubmissionClip->nClip != packet->nClipNumber)
      {
        m_currentVideoSubmissionClip->Superceed(SUPERCEEDED_VIDEO_FILL);
        CClip * nextVideoClip = GetNextVideoClip(m_currentVideoSubmissionClip, SUPERCEEDED_VIDEO_FILL);
        if (nextVideoClip==m_currentVideoSubmissionClip)
        {
          LogDebug("Playlist::Video Clip %d not found in playlist %d",packet->nClipNumber, this->nPlaylist); 
          return false; //this playlist is finished
        }
        m_currentVideoSubmissionClip = nextVideoClip;
      }
      prevVideoPosition = m_currentVideoSubmissionClip->lastVideoPosition;
      ret=m_currentVideoSubmissionClip->AcceptVideoPacket(packet);
    }
  }

  bool discontinuity=false;

  if (packet->rtStart != Packet::INVALID_TIME && prevVideoPosition != Packet::INVALID_TIME && abs(prevVideoPosition - packet->rtStart) > 20000000)
  {
    LogDebug("clip: video stream's discontinuity detected: old: %I64d new: %I64d", prevVideoPosition, packet->rtStart);
    firstVideoPESPacketSeen=false;
    discontinuity=true;
  }

  if (!firstVideoPESPacketSeen && ret && packet->rtStart!=Packet::INVALID_TIME)
  {
    REFERENCE_TIME oldPEStime = firstVideoPESTimeStamp;

    firstVideoPESPacketSeen=true;
    firstVideoPESTimeStamp= m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart;
    
    packet->rtOffset = (0 - firstVideoPESTimeStamp) > 10000000 || !discontinuity ? 0 - firstVideoPESTimeStamp : 0;
    if (packet->rtOffset != 0)
      packet->bSeekRequired=!seeking;

    m_currentAudioSubmissionClip->FlushVideo(packet);
    LogDebug("clip: first Packet (vid) %I64d old: %I64d new: %I64d seekRequired %d", packet->rtStart, oldPEStime, firstVideoPESTimeStamp, packet->bSeekRequired);
  }
  if (!firstPacketRead && ret && packet->rtStart!=Packet::INVALID_TIME && firstVideoPESTimeStamp > m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart)
  {
    firstVideoPESTimeStamp=m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart;
  } 
  return ret;
}

CClip * CPlaylist::GetNextAudioClip(CClip * currentClip, int superceedType)
{
  CClip * ret = currentClip;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    if (!clip->IsSuperceeded(superceedType) && currentClip->nClip !=clip->nClip)
    {
      return clip;
    }
    ++it;
  }
  return ret;
}

CClip * CPlaylist::GetNextVideoClip(CClip * currentClip, int superceedType)
{
  CClip * ret = currentClip;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    if (!clip->IsSuperceeded(superceedType) && currentClip->nClip !=clip->nClip)
    {
      //LogDebug("New Video Clip %d",clip->nClip);
      return clip;
    }
    ++it;
  }
  return ret;
}

bool CPlaylist::CreateNewClip(int clipNumber, REFERENCE_TIME clipStart, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, bool discontinuousClip)
{
  bool ret = true;
  if (m_vecClips.size() && m_vecClips.back()->nClip == clipNumber) return false;
  m_vecClips.push_back(new CClip(clipNumber, clipStart, clipOffset, audioPresent, duration, discontinuousClip));
  if (m_currentAudioPlayBackClip==NULL)
  {
    // initialise
    m_currentAudioPlayBackClip=m_currentVideoPlayBackClip=m_currentAudioSubmissionClip=m_currentVideoSubmissionClip=*m_vecClips.begin();
  }
  else
  {
    m_VideoPacketsUntilLatestClip=1;
  }
  m_currentAudioSubmissionClip=m_vecClips.back();
  return ret;
}

bool CPlaylist::IsEmptiedAudio()
{
  return playlistEmptiedAudio;
}

bool CPlaylist::IsEmptiedVideo()
{
  return playlistEmptiedVideo;
}

void CPlaylist::SetEmptiedVideo()
{
  playlistEmptiedVideo=true;
}

void CPlaylist::SetEmptiedAudio()
{
  playlistEmptiedAudio=true;
}

bool CPlaylist::IsFilledAudio()
{
  return playlistFilledAudio;
}

bool CPlaylist::IsFilledVideo()
{
  return playlistFilledVideo;
}

void CPlaylist::SetFilledVideo()
{
  playlistFilledVideo=true;
}

void CPlaylist::SetFilledAudio()
{
  playlistFilledAudio=true;
}

REFERENCE_TIME CPlaylist::GetPacketTimeStampCorrection(CClip * packetClip)
{
//  LogDebug("Correcting timestamp by %I64d - %I64d",packetClip->clipPlaylistOffset, firstPESTimeStamp);
  return packetClip->clipPlaylistOffset;
}

Packet * CPlaylist::CorrectTimeStamp(CClip * packetClip, Packet* packet)
{
  Packet* ret=packet;
  if (packet->rtStart!=Packet::INVALID_TIME)
  {
    ret->rtStart -= GetPacketTimeStampCorrection(packetClip);
    ret->rtStop -= GetPacketTimeStampCorrection(packetClip);
    ret->rtOffset = (0 - firstAudioPESTimeStamp) > 10000000 ? 0 - firstAudioPESTimeStamp : 0; // use only audio offset
  }
  return ret;
}

void CPlaylist::FlushAudio()
{
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    clip->FlushAudio();
    ++it;
  }
}
void CPlaylist::FlushVideo()
{
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    clip->FlushVideo();
    ++it;
  }
}

bool CPlaylist::IsFakeAudioAvailable()
{
  if (m_currentAudioPlayBackClip==NULL) return false;
  CClip * nextClip = GetNextAudioClip(m_currentAudioPlayBackClip, SUPERCEEDED_AUDIO_RETURN);
  return nextClip->FakeAudioAvailable();
}

bool CPlaylist::IsFakingAudio()
{
  if (m_currentAudioPlayBackClip==NULL) return false;
  CClip * nextClip = GetNextAudioClip(m_currentAudioPlayBackClip, SUPERCEEDED_AUDIO_RETURN);
  return nextClip->noAudio;
}

void CPlaylist::ClearAllButCurrentClip(bool resetClip)
{
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    if (clip==m_vecClips.back())
    {
      ++it;
    }
    else
    {
      it=m_vecClips.erase(it);
      delete clip;
    }
  }
  if (m_vecClips.size()>0)
  {
    if (resetClip) 
    {
      Reset(nPlaylist, playlistFirstPacketTime);
    }
    m_currentAudioPlayBackClip=m_currentVideoPlayBackClip=m_currentAudioSubmissionClip=m_currentVideoSubmissionClip=m_vecClips.back();
    if (resetClip) 
    {
      m_currentAudioPlayBackClip->Reset();
    }
  }
}

void CPlaylist::Reset(int playlistNumber, REFERENCE_TIME firstPacketTime)
{
  nPlaylist=playlistNumber;
  playlistFirstPacketTime=firstPacketTime;
  m_currentAudioPlayBackClip=NULL;
  m_currentVideoPlayBackClip=NULL;
  m_currentAudioSubmissionClip=NULL;
  m_currentVideoSubmissionClip=NULL;
  playlistFilledAudio=false;
  playlistFilledVideo=false;
  playlistEmptiedVideo=false;
  playlistEmptiedAudio=false;

  m_VideoPacketsUntilLatestClip=0;

  firstAudioPESPacketSeen=false;
  firstVideoPESPacketSeen=false;
  firstAudioPESTimeStamp=0LL;
  firstVideoPESTimeStamp=0LL;

  firstPacketRead=false;
}

bool CPlaylist::HasAudio()
{
  if (m_currentAudioPlayBackClip==NULL) return false;
  if (m_currentAudioPlayBackClip->HasAudio()) return true;
  CClip* nextClip = GetNextAudioClip(m_currentAudioPlayBackClip, SUPERCEEDED_AUDIO_RETURN);
  if (nextClip!=m_currentAudioPlayBackClip)
  {
    return nextClip->HasAudio();
  }
  return false;
}

bool CPlaylist::HasVideo()
{
  if (m_currentVideoPlayBackClip==NULL) return false;
  if (m_currentVideoPlayBackClip->HasVideo()) return true;
  CClip* nextClip = GetNextVideoClip(m_currentVideoPlayBackClip, SUPERCEEDED_VIDEO_RETURN);
  if (nextClip!=m_currentVideoPlayBackClip)
  {
    return nextClip->HasVideo();
  }
  return false;
}

bool CPlaylist::Incomplete()
{
  if (m_currentAudioPlayBackClip)
  {
    return m_currentAudioPlayBackClip->Incomplete();
  }

  return false;
}

CClip * CPlaylist::GetClip(int nClip)
{
  CClip *ret=NULL;
  ivecClip it = m_vecClips.end();
  while (it!=m_vecClips.begin())
  {
    --it;
    CClip * clip=*it;
    if (clip->nClip==nClip) return clip;
  }
  return ret;
}

void CPlaylist::SetVideoPMT(AM_MEDIA_TYPE * pmt, int nClip)
{
  CClip * clip = GetClip(nClip);
  if (clip)
  {
    clip->SetVideoPMT(pmt);
  }
}

bool CPlaylist::RemoveRedundantClips()
{
  ivecClip it = m_vecClips.end();
  if (it != m_vecClips.begin()) --it;
  while (it != m_vecClips.begin())
  {
    CClip * clip=*it;
    if (clip->IsSuperceeded(SUPERCEEDED_AUDIO_RETURN|SUPERCEEDED_VIDEO_RETURN|SUPERCEEDED_AUDIO_FILL|SUPERCEEDED_VIDEO_FILL)) 
    {
      it=m_vecClips.erase(it);
      delete clip;
    }
    else
    {
      --it;
    }
  }
  if (m_vecClips.size()==0) return true;
  return false;
}