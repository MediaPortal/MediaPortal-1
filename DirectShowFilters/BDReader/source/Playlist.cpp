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
  Packet * ret = (*m_itCurrentAudioPlayBackClip)->ReturnNextAudioPacket(playlistFirstPacketTime);
  if (ret)
  {
    ret->nPlaylist=nPlaylist;
    CorrectTimeStamp(*m_itCurrentAudioPlayBackClip,ret);
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
  Packet* ret=NULL;
  if ((*m_itCurrentAudioPlayBackClip)->nClip==clip)
  {
    ret = (*m_itCurrentAudioPlayBackClip)->ReturnNextAudioPacket(playlistFirstPacketTime);
    if (ret!=NULL)
    {
      ret->nPlaylist=nPlaylist;
      CorrectTimeStamp((*m_itCurrentAudioPlayBackClip),ret);
    }
  }
  if (ret) firstPacketRead=true;
  return ret;
}

Packet* CPlaylist::ReturnNextVideoPacket()
{
  firstPacketRead=true;
  Packet * ret = (*m_itCurrentVideoPlayBackClip)->ReturnNextVideoPacket(playlistFirstPacketTime);
  if (ret)
  {
    ret->nPlaylist=nPlaylist;
    CorrectTimeStamp((*m_itCurrentVideoPlayBackClip),ret);
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

bool CPlaylist::CreateNewClip(int clipNumber, REFERENCE_TIME clipStart, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, REFERENCE_TIME playlistClipOffset)
{
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

REFERENCE_TIME CPlaylist::ClearAllButCurrentClip(bool resetClip, REFERENCE_TIME rtClipStartPoint)
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
    m_itCurrentAudioPlayBackClip=m_itCurrentVideoPlayBackClip=m_itCurrentAudioSubmissionClip=m_itCurrentVideoSubmissionClip=m_vecClips.begin();
    if (resetClip) 
    {
      (*m_itCurrentAudioPlayBackClip)->Reset(rtClipStartPoint);
    }
    return (*m_itCurrentAudioPlayBackClip)->clipDuration;
  }
  return 0LL;
}

void CPlaylist::Reset(int playlistNumber, REFERENCE_TIME firstPacketTime)
{
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
  if (m_vecClips.size()>0)
  {
    return m_vecClips.back()->Incomplete();
  }

  return 0LL;
}

REFERENCE_TIME CPlaylist::PlayedDuration()
{
  if (m_vecClips.size()>0)
  {
    return m_vecClips.back()->PlayedDuration();
  }

  return 0LL;
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

vector<CClip*> CPlaylist::Superceed()
{
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
