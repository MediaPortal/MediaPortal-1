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
  if (m_currentAudioPlayBackClip==NULL)
  {
    LogDebug("m_currentAudioPlayBackClip is NULL");
    m_currentAudioPlayBackClip=*m_vecClips.begin();
  }
  Packet * ret = m_currentAudioPlayBackClip->ReturnNextAudioPacket(playlistFirstPacketTime);
  if (ret!=NULL)
  {
    ret->nPlaylist=nPlaylist;
    CorrectTimeStamp(m_currentAudioPlayBackClip,ret);
  }
  else
  {
    CClip* nextAudioClip = GetNextAudioClip(m_currentAudioPlayBackClip);
    if (m_currentAudioPlayBackClip!=nextAudioClip)
    {
      m_currentAudioPlayBackClip->Superceed(SUPERCEEDED_AUDIO);
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
    Packet * ret = m_currentAudioPlayBackClip->ReturnNextAudioPacket(playlistFirstPacketTime);
    if (ret!=NULL)
    {
      ret->nPlaylist=nPlaylist;
      CorrectTimeStamp(m_currentAudioPlayBackClip,ret);
    }
  }
  return ret;
}


Packet* CPlaylist::ReturnNextVideoPacket()
{
  Packet * ret = m_currentVideoPlayBackClip->ReturnNextVideoPacket(playlistFirstPacketTime);
  if (ret!=NULL)
  {
    ret->nPlaylist=nPlaylist;
    CorrectTimeStamp(m_currentVideoPlayBackClip,ret);
  }
  else
  {
    CClip* nextVideoClip = GetNextVideoClip(m_currentVideoPlayBackClip);
    if (m_currentVideoPlayBackClip!=nextVideoClip)
    {
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

bool CPlaylist::AcceptAudioPacket(Packet*  packet, bool forced)
{
  bool ret = true;
  if (m_currentAudioSubmissionClip==NULL) return false;
  else if (!m_currentAudioSubmissionClip->AcceptAudioPacket(packet, forced))
  {
    m_currentAudioSubmissionClip->Superceed(SUPERCEEDED_AUDIO);
    bool complete=false;
    while (!complete)
    {
      CClip* nextSubmissionClip = GetNextAudioClip(m_currentAudioSubmissionClip);
      if (m_currentAudioSubmissionClip==nextSubmissionClip)
      {
        return false;
      }
      else if (!nextSubmissionClip->noAudio)
      {
        m_currentAudioSubmissionClip=nextSubmissionClip;
        complete=true;
      }
      else
      {
        m_currentAudioSubmissionClip=nextSubmissionClip;
      }
    }
    ret=AcceptAudioPacket(packet, forced);
  }
  if (!firstPESPacketSeen && ret)
  {
    firstPESPacketSeen=true;
    firstPESTimeStamp= m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart;
  }
  return ret;
}

bool CPlaylist::AcceptVideoPacket(Packet*  packet, bool firstPacket, bool forced)
{
  bool ret = true;
  if (m_currentVideoSubmissionClip==NULL) 
  {
    LogDebug("m_currentVideoSubmissionClip is NULL");
    ret=false;
  }
  //this needs well verified...
  else if (!firstPacket && packet->rtStart == playlistFirstPacketTime)
  {
    //next clip has arrived
    m_currentVideoSubmissionClip->Superceed(SUPERCEEDED_VIDEO);
    CClip * nextVideoClip = GetNextVideoClip(m_currentVideoSubmissionClip);
    if (nextVideoClip==m_currentVideoSubmissionClip) return false; //this playlist is finished
    m_currentVideoSubmissionClip = nextVideoClip;
    ret=AcceptVideoPacket(packet,true, forced);
  }
  else if (!m_currentVideoSubmissionClip->AcceptVideoPacket(packet, forced))
  {
    CClip * nextVideoClip = GetNextVideoClip(m_currentVideoSubmissionClip);
    m_currentVideoSubmissionClip->Superceed(SUPERCEEDED_VIDEO);
    if (nextVideoClip != m_currentVideoSubmissionClip)
    {
      m_currentVideoSubmissionClip = nextVideoClip;
      ret=AcceptVideoPacket(packet,true, forced);
    }
  }
  if (!firstPESPacketSeen && ret && packet->rtStart!=Packet::INVALID_TIME)
  {
    firstPESPacketSeen=true;
    firstPESTimeStamp= m_currentVideoSubmissionClip->clipPlaylistOffset - packet->rtStart;
  }
  return ret;
}

CClip * CPlaylist::GetNextAudioClip(CClip * currentClip)
{
  CClip * ret = currentClip;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    if (!clip->IsSuperceeded(SUPERCEEDED_AUDIO) && currentClip->nClip !=clip->nClip)
    {
//      LogDebug("New Audio Clip %d",clip->nClip);
      return clip;
    }
    ++it;
  }
  return ret;
}

CClip * CPlaylist::GetNextVideoClip(CClip * currentClip)
{
  CClip * ret = currentClip;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    if (!clip->IsSuperceeded(SUPERCEEDED_VIDEO) && currentClip->nClip !=clip->nClip)
    {
//      LogDebug("New Video Clip %d",clip->nClip);
      return clip;
    }
    ++it;
  }
  return ret;
}

bool CPlaylist::CreateNewClip(int clipNumber, REFERENCE_TIME clipStart, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration)
{
  bool ret = true;
  m_vecClips.push_back(new CClip(clipNumber, clipStart, clipOffset, audioPresent, duration));
  if (m_currentAudioPlayBackClip==NULL)
  {
    // initialise
    m_currentAudioPlayBackClip=m_currentVideoPlayBackClip=m_currentAudioSubmissionClip=m_currentVideoSubmissionClip=*m_vecClips.begin();
  }
  if (m_currentAudioSubmissionClip==NULL)// current clip finish before new pl/clip (fake audio)
  {
    m_currentAudioSubmissionClip=*m_vecClips.begin();
  }
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
  return packetClip->clipPlaylistOffset - firstPESTimeStamp;
}

Packet * CPlaylist::CorrectTimeStamp(CClip * packetClip, Packet* packet)
{
  Packet* ret=packet;
  if (packet->rtStart!=Packet::INVALID_TIME)
  {
    ret->rtStart -= GetPacketTimeStampCorrection(packetClip);
    ret->rtStop -= GetPacketTimeStampCorrection(packetClip);
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
  CClip * nextClip = GetNextAudioClip(m_currentAudioPlayBackClip);
  return nextClip->FakeAudioAvailable();
}

bool CPlaylist::IsFakingAudio()
{
  if (m_currentAudioPlayBackClip==NULL) return false;
  CClip * nextClip = GetNextAudioClip(m_currentAudioPlayBackClip);
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

int CPlaylist::AudioPacketCount()
{
  int totalAudioPackets=0;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    totalAudioPackets+=clip->AudioPacketCount();
    ++it;
  }
  return totalAudioPackets;
}

int CPlaylist::VideoPacketCount()
{
  int totalVideoPackets=0;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    totalVideoPackets+=clip->VideoPacketCount();
    ++it;
  }
  return totalVideoPackets;
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

  firstPESPacketSeen=false;
  firstPESTimeStamp=0LL;
}

bool CPlaylist::HasAudio()
{
  if (m_currentAudioPlayBackClip==NULL) return false;
  if (m_currentAudioPlayBackClip->HasAudio()) return true;
  CClip* nextClip = GetNextAudioClip(m_currentAudioPlayBackClip);
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
  CClip* nextClip = GetNextVideoClip(m_currentVideoPlayBackClip);
  if (nextClip!=m_currentVideoPlayBackClip)
  {
    return nextClip->HasVideo();
  }
  return false;
}
