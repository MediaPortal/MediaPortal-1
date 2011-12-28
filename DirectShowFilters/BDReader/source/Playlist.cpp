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

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);

CPlaylist::CPlaylist(int playlistNumber, REFERENCE_TIME firstPacketTime)
{
  Reset(playlistNumber, firstPacketTime);
}

CPlaylist::~CPlaylist(void)
{
  CAutoLock vectorLock(&m_sectionVector);
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
  CAutoLock vectorLock(&m_sectionVector);
  firstPacketRead=true;
  firstAudioPESPacketSeen=true;
  Packet * ret = (*m_itCurrentAudioPlayBackClip)->ReturnNextAudioPacket(playlistFirstPacketTime);
  if (ret)
  {
    ret->nPlaylist=nPlaylist;
  }
  else
  {
    if (m_itCurrentAudioPlayBackClip++ == m_vecClips.end()) 
    {
      m_itCurrentAudioPlayBackClip--;
      SetEmptiedAudio();
    }
    else
    {
      m_itCurrentAudioPlayBackClip--;
      (*m_itCurrentAudioPlayBackClip)->Superceed(SUPERCEEDED_AUDIO_RETURN);
      m_itCurrentAudioPlayBackClip++;
      ret=ReturnNextAudioPacket();
    }
  }
  return ret;
}

Packet* CPlaylist::ReturnNextAudioPacket(int clip)
{
  CAutoLock vectorLock(&m_sectionVector);
  Packet* ret=NULL;
  if ((*m_itCurrentAudioPlayBackClip)->nClip==clip)
  {
    ret = (*m_itCurrentAudioPlayBackClip)->ReturnNextAudioPacket(playlistFirstPacketTime);
    if (ret!=NULL)
    {
      ret->nPlaylist=nPlaylist;
    }
  }
  if (ret) firstPacketRead=true;
  return ret;
}

Packet* CPlaylist::ReturnNextVideoPacket()
{
  CAutoLock vectorLock(&m_sectionVector);
  firstPacketRead=true;
  Packet * ret = (*m_itCurrentVideoPlayBackClip)->ReturnNextVideoPacket(playlistFirstPacketTime);
  if (ret)
  {
    ret->nPlaylist=nPlaylist;
  }
  else
  {
    if (m_itCurrentVideoPlayBackClip++ == m_vecClips.end()) 
    {
      m_itCurrentVideoPlayBackClip--;
      SetEmptiedVideo();
    }
    else
    {
      (*(m_itCurrentVideoPlayBackClip--))->Superceed(SUPERCEEDED_VIDEO_RETURN);
      m_itCurrentVideoPlayBackClip++;
      ret=ReturnNextVideoPacket();
    }
  }
  return ret;
}

bool CPlaylist::AcceptAudioPacket(Packet*  packet)
{
  CAutoLock vectorLock(&m_sectionVector);
  bool ret = true;
  if (!m_vecClips.size()) return false;
  if ((*m_itCurrentAudioSubmissionClip)->nClip == packet->nClipNumber)
  {
    ret = (*m_itCurrentAudioSubmissionClip)->AcceptAudioPacket(packet);
  }
  else 
  {
    LogDebug("CPlaylist Panic in Accept Audio Packet");
  }

  if (!firstAudioPESPacketSeen && ret && packet->rtStart!=Packet::INVALID_TIME)
  {
    REFERENCE_TIME oldPEStime = firstAudioPESTimeStamp;

    firstAudioPESPacketSeen=true;
    firstAudioPESTimeStamp= (*m_itCurrentAudioSubmissionClip)->clipPlaylistOffset - packet->rtStart;
  }

  return ret;
}

bool CPlaylist::AcceptVideoPacket(Packet* packet)
{
  CAutoLock vectorLock(&m_sectionVector);
  bool ret = true;
  REFERENCE_TIME prevVideoPosition = 0;
  if (nPlaylist != packet->nPlaylist)
  {
    (*m_itCurrentVideoSubmissionClip)->Superceed(SUPERCEEDED_VIDEO_FILL);
    return false;
  }
  if ((*m_itCurrentVideoSubmissionClip)->nClip==packet->nClipNumber)
  {
    prevVideoPosition = (*m_itCurrentVideoSubmissionClip)->lastVideoPosition; 
    ret=(*m_itCurrentVideoSubmissionClip)->AcceptVideoPacket(packet);
  }

  if (!firstVideoPESPacketSeen && ret && packet->rtStart!=Packet::INVALID_TIME)
  {
    REFERENCE_TIME oldPEStime = firstVideoPESTimeStamp;

    firstVideoPESPacketSeen=true;
    firstVideoPESTimeStamp= (*m_itCurrentVideoSubmissionClip)->clipPlaylistOffset - packet->rtStart;
  }
 
  return ret;
}

bool CPlaylist::CreateNewClip(int clipNumber, REFERENCE_TIME clipStart, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, REFERENCE_TIME playlistClipOffset)
{
  CAutoLock vectorLock(&m_sectionVector);
  bool ret = true;

  if (m_vecClips.size())
  {
    (*m_itCurrentAudioSubmissionClip)->Superceed(SUPERCEEDED_AUDIO_FILL);
    (*m_itCurrentVideoSubmissionClip)->Superceed(SUPERCEEDED_VIDEO_FILL);
  }


  if (m_vecClips.size()) PushClips();
  m_vecClips.push_back(new CClip(clipNumber, nPlaylist, clipStart, clipOffset, playlistClipOffset, audioPresent, duration));
  if (m_vecClips.size()==1)
  {
    // initialise
    m_itCurrentAudioPlayBackClip=m_itCurrentVideoPlayBackClip=m_itCurrentAudioSubmissionClip=m_itCurrentVideoSubmissionClip=m_vecClips.begin();
  }
  else
  {
    PopClips();
    m_itCurrentAudioSubmissionClip++;
    m_itCurrentVideoSubmissionClip++;
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

void CPlaylist::FlushAudio()
{
  CAutoLock vectorLock(&m_sectionVector);
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
  CAutoLock vectorLock(&m_sectionVector);
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    clip->FlushVideo();
    ++it;
  }
}

REFERENCE_TIME CPlaylist::ClearAllButCurrentClip(REFERENCE_TIME totalStreamOffset)
{
  CAutoLock vectorLock(&m_sectionVector);
  REFERENCE_TIME ret = 0LL;
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
    m_itCurrentAudioPlayBackClip=m_itCurrentVideoPlayBackClip=m_itCurrentAudioSubmissionClip=m_itCurrentVideoSubmissionClip=m_vecClips.begin();
    ret = (*m_itCurrentAudioPlayBackClip)->PlayedDuration();
    Reset(nPlaylist, playlistFirstPacketTime);
    (*m_itCurrentAudioPlayBackClip)->Reset(totalStreamOffset + ret);
    return ret;
  }
  return 0LL;
}

void CPlaylist::Reset(int playlistNumber, REFERENCE_TIME firstPacketTime)
{
  CAutoLock vectorLock(&m_sectionVector);
  nPlaylist=playlistNumber;
  playlistFirstPacketTime=firstPacketTime;
  playlistFilledAudio=false;
  playlistFilledVideo=false;
  playlistEmptiedVideo=false;
  playlistEmptiedAudio=false;

  firstAudioPESPacketSeen=false;
  firstVideoPESPacketSeen=false;
  firstAudioPESTimeStamp=0LL;
  firstVideoPESTimeStamp=0LL;

  firstPacketRead=false;
}

bool CPlaylist::HasAudio()
{
  CAutoLock vectorLock(&m_sectionVector);
  if (!m_vecClips.size()) return false;
  if ((*m_itCurrentAudioPlayBackClip)->HasAudio()) return true;
  if (++m_itCurrentAudioPlayBackClip == m_vecClips.end()) m_itCurrentAudioPlayBackClip--;
  else
  {
    bool ret = (*m_itCurrentAudioPlayBackClip)->HasAudio();
    m_itCurrentAudioPlayBackClip--;
    return ret;
  }
  return false;
}

bool CPlaylist::HasVideo()
{
  CAutoLock vectorLock(&m_sectionVector);
  if (!m_vecClips.size()) return false;
  if ((*m_itCurrentVideoPlayBackClip)->HasVideo()) return true;
  if (++m_itCurrentVideoPlayBackClip == m_vecClips.end()) m_itCurrentVideoPlayBackClip--;
  else
  {
    bool ret = (*m_itCurrentVideoPlayBackClip)->HasVideo();
    m_itCurrentVideoPlayBackClip--;
    return ret;
  }
  return false;
}

REFERENCE_TIME CPlaylist::Incomplete()
{
  CAutoLock vectorLock(&m_sectionVector);
  if (m_vecClips.size()>0)
  {
    return m_vecClips.back()->Incomplete();
  }

  return 0LL;
}

REFERENCE_TIME CPlaylist::PlayedDuration()
{
  CAutoLock vectorLock(&m_sectionVector);
  if (m_vecClips.size()>0)
  {
    return m_vecClips.back()->PlayedDuration();
  }

  return 0LL;
}

void CPlaylist::SetVideoPMT(AM_MEDIA_TYPE * pmt, int nClip)
{
  CAutoLock vectorLock(&m_sectionVector);
  (*m_itCurrentVideoSubmissionClip)->SetVideoPMT(pmt);
}

bool CPlaylist::RemoveRedundantClips()
{
  CAutoLock vectorLock(&m_sectionVector);
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

vector<CClip*> CPlaylist::Superceed()
{
  CAutoLock vectorLock(&m_sectionVector);
  vector<CClip*> ret;
  ivecClip it = m_vecClips.begin();
  while (it!=m_vecClips.end())
  {
    CClip * clip=*it;
    if (clip->noAudio) ret.push_back(clip);
    ++it;
  }
  return ret;
}

void CPlaylist::PushClips()
{
  m_itCurrentAudioPlayBackClipPos = m_itCurrentAudioPlayBackClip-m_vecClips.begin();
  m_itCurrentVideoPlayBackClipPos = m_itCurrentVideoPlayBackClip-m_vecClips.begin();
  m_itCurrentAudioSubmissionClipPos = m_itCurrentAudioSubmissionClip-m_vecClips.begin();
  m_itCurrentVideoSubmissionClipPos = m_itCurrentVideoSubmissionClip-m_vecClips.begin();

}
void CPlaylist::PopClips()
{
  m_itCurrentAudioPlayBackClip = m_vecClips.begin() + m_itCurrentAudioPlayBackClipPos;
  m_itCurrentVideoPlayBackClip = m_vecClips.begin() + m_itCurrentVideoPlayBackClipPos;
  m_itCurrentAudioSubmissionClip = m_vecClips.begin() + m_itCurrentAudioSubmissionClipPos;
  m_itCurrentVideoSubmissionClip = m_vecClips.begin() + m_itCurrentVideoSubmissionClipPos;
}
