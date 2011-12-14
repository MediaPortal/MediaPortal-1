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

#include "PlaylistManager.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);

CPlaylistManager::CPlaylistManager(void)
{
  LogDebug("Playlist Manager Created");
  m_currentAudioPlayBackPlaylist=NULL;
  m_currentVideoPlayBackPlaylist=NULL;
  m_currentAudioSubmissionPlaylist=NULL;
  m_currentVideoSubmissionPlaylist=NULL;
  m_VideoPacketsUntilLatestplaylist=0;
  m_bIgnoreAudioSeeking=false;
  m_bIgnoreVideoSeeking=false;
  m_rtPlaylistOffset = 0LL;
  m_vPmt = NULL;
  m_aPmt = NULL;
}


CPlaylistManager::~CPlaylistManager(void)
{
  CAutoLock lock (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);
  LogDebug("Playlist Manager Closing");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    it=m_vecPlaylists.erase(it);
    delete playlist;
  }
}

bool CPlaylistManager::CreateNewPlaylistClip(int nPlaylist, int nClip, bool audioPresent, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffsetTime, REFERENCE_TIME duration, bool discontinuousClip)
{
  CAutoLock lock (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);
  bool ret;
  // remove old playlists
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (playlist->RemoveRedundantClips())
    {
      it=m_vecPlaylists.erase(it);
      delete playlist;
    }
    else ++it;
  }

  LogDebug("Playlist Manager new Playlist %d clip %d start %6.3f clipOffset %6.3f Audio %d duration %6.3f",nPlaylist, nClip, firstPacketTime/10000000.0, clipOffsetTime/10000000.0, audioPresent, duration/10000000.0);

  REFERENCE_TIME remainingClipTime = Incomplete();
  REFERENCE_TIME playedDuration = ClipPlayTime();
  ret = remainingClipTime>5000000LL;
  if (ret)
  {
    LogDebug("Playlist Manager::CreateNewPlaylistClip TimeStamp Correction changed from %I64d to %I64d",m_rtPlaylistOffset,m_rtPlaylistOffset-remainingClipTime);
  }
  LogDebug("Playlist Manager::TimeStamp Correction changed to %I64d adding %I64d",m_rtPlaylistOffset + playedDuration, playedDuration);

  m_rtPlaylistOffset += playedDuration;

  if (m_vecPlaylists.size()==0)
  {
    //first playlist
    CPlaylist * firstPlaylist = new CPlaylist(nPlaylist,firstPacketTime);
    firstPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, m_rtPlaylistOffset, false);
    m_vecPlaylists.push_back(firstPlaylist);
    m_currentAudioPlayBackPlaylist=m_currentVideoPlayBackPlaylist=m_currentAudioSubmissionPlaylist=m_currentVideoSubmissionPlaylist=firstPlaylist;
  }
  else if (m_vecPlaylists.back()->nPlaylist == nPlaylist)
  {
    //new clip in existing playlist
    CPlaylist * existingPlaylist = m_vecPlaylists.back();
    existingPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, m_rtPlaylistOffset, false /*discontinuousClip*/);
  }
  else
  {
    //completely new playlist
//    m_bIgnoreAudioSeeking=false;
//    m_bIgnoreVideoSeeking=false;

//    m_rtPlaylistOffset = 0LL;
    CPlaylist * existingPlaylist = m_vecPlaylists.back();
    vector<CClip*> audioLess = existingPlaylist->Superceed();
    if (audioLess.size())
    {
      ivecClip it = audioLess.begin();
      while (it!=audioLess.end())
      {
        CClip * clip=*it;
        m_vecNonFilledClips.push_back(clip);
        ++it;
      }
    }

    CPlaylist * newPlaylist = new CPlaylist(nPlaylist,firstPacketTime);
    newPlaylist->CreateNewClip(nClip,firstPacketTime, clipOffsetTime, audioPresent, duration, m_rtPlaylistOffset, false);
    m_vecPlaylists.push_back(newPlaylist);
  }
  return ret; // was current clip interrupted?
}

bool CPlaylistManager::SubmitAudioPacket(Packet * packet)
{
  CAutoLock lock (&m_sectionAudio);
  bool ret = false;
  if (m_currentAudioSubmissionPlaylist==NULL) 
  {
    LogDebug("m_currentAudioSubmissionPlaylist is NULL!!!");
    return false;
  }
  if (m_vecNonFilledClips.size())
  {
    ivecClip it = m_vecNonFilledClips.begin();
    while (it!=m_vecNonFilledClips.end())
    {
      CClip * clip=*it;
      if (!((clip->nClip == packet->nClipNumber) && (clip->nPlaylist == packet->nPlaylist)))
      {
        clip->Superceed(SUPERCEEDED_AUDIO_FILL);
        it = m_vecNonFilledClips.erase(it);
      }
      else
      {
        ++it;
      }
    }
  }


  ret=m_currentAudioSubmissionPlaylist->AcceptAudioPacket(packet, m_bIgnoreAudioSeeking);
  if (ret) 
  {
    m_bIgnoreAudioSeeking = false;

#ifdef LOG_AUDIO_PACKETS
    LogDebug("Audio Packet %I64d Accepted in %d %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber);
#endif
  }
  if (!ret)
  {
    CPlaylist* nextPlaylist = GetNextAudioSubmissionPlaylist(m_currentAudioSubmissionPlaylist);
    if (nextPlaylist==m_currentAudioSubmissionPlaylist)
    {
      if (packet->nPlaylist == m_currentAudioSubmissionPlaylist->nPlaylist)
      {
        ret=m_currentAudioSubmissionPlaylist->AcceptAudioPacket(packet, m_bIgnoreAudioSeeking);
        if (ret)
          m_bIgnoreAudioSeeking = false;
      }
      else
      {
        //this playlist has no home, delete it
        delete packet;
        ret = true;
      }
    }
    else
    {
      m_currentAudioSubmissionPlaylist->SetFilledAudio();
      m_currentAudioSubmissionPlaylist = nextPlaylist;
      ret=SubmitAudioPacket(packet);
    }
  }

  return ret;
}

bool CPlaylistManager::SubmitVideoPacket(Packet * packet)
{
  CAutoLock lock (&m_sectionVideo);
  bool ret=false;
  if (m_currentVideoSubmissionPlaylist==NULL)
  {
    LogDebug("m_currentVideoSubmissionPlaylist is NULL!!!");
    return false;
  }
  ret=m_currentVideoSubmissionPlaylist->AcceptVideoPacket(packet,false,m_bIgnoreVideoSeeking);
  if (ret) 
  {
    m_bIgnoreVideoSeeking = false;

#ifdef LOG_VIDEO_PACKETS
    LogDebug("Video Packet %I64d Accepted in %d %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber);
#endif
  }
  if (!ret)
  {
#ifdef LOG_VIDEO_PACKETS
     LogDebug("Video Packet %I64d %d %d rejected from %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber, m_currentVideoSubmissionPlaylist->nPlaylist);
#endif

    CPlaylist* nextPlaylist = GetNextVideoSubmissionPlaylist(m_currentVideoSubmissionPlaylist);
    if (nextPlaylist == m_currentVideoSubmissionPlaylist)
    {
//      LogDebug("Failed to find video submission playlist");
      if (packet->nPlaylist == m_currentVideoSubmissionPlaylist->nPlaylist)
      {
        ret=m_currentVideoSubmissionPlaylist->AcceptVideoPacket(packet,false,m_bIgnoreVideoSeeking);
        if (ret)
        {
#ifdef LOG_VIDEO_PACKETS
    LogDebug("Video Packet %I64d Accepted in %d %d", packet->rtStart, packet->nPlaylist, packet->nClipNumber);
#endif
          m_bIgnoreVideoSeeking = false;
        }
      }
      else
      {
        //this packet has no home, delete it
        delete packet;
      }
    }
    else
    {
      m_currentVideoSubmissionPlaylist->SetFilledVideo();
      m_currentVideoSubmissionPlaylist = nextPlaylist;
      if (m_currentVideoSubmissionPlaylist->nPlaylist == packet->nPlaylist)
      {
        ret=m_currentVideoSubmissionPlaylist->AcceptVideoPacket(packet,true,m_bIgnoreVideoSeeking);
        if (ret)
          m_bIgnoreVideoSeeking = false;
      }
      else
      {
        //this packet has no home, delete it
        delete packet;
      }
    }
  }

  return ret;
}

Packet* CPlaylistManager::GetNextAudioPacket()
{
  CAutoLock lock (&m_sectionAudio);
  Packet* ret=m_currentAudioPlayBackPlaylist->ReturnNextAudioPacket();
  if (ret==NULL)
  {
    LogDebug("playlistManager: checking for audio playback playlist after %d",m_currentAudioPlayBackPlaylist->nPlaylist);
    CPlaylist* nextPlaylist = GetNextAudioPlaylist(m_currentAudioPlayBackPlaylist);
    if (m_currentAudioPlayBackPlaylist!=nextPlaylist)
    {
      LogDebug("playlistManager: setting audio playback playlist to %d",nextPlaylist->nPlaylist);
      m_currentAudioPlayBackPlaylist->SetEmptiedAudio();
      m_currentAudioPlayBackPlaylist = nextPlaylist;
      ret=m_currentAudioPlayBackPlaylist->ReturnNextAudioPacket();
    }
  }
  return ret;
}

Packet* CPlaylistManager::GetNextAudioPacket(int playlist, int clip)
{
  Packet* ret=NULL;
  if (m_currentAudioPlayBackPlaylist->nPlaylist==playlist)
  {
    ret=m_currentAudioPlayBackPlaylist->ReturnNextAudioPacket(clip);
  }
  return ret;
}


Packet* CPlaylistManager::GetNextVideoPacket()
{
  CAutoLock lock (&m_sectionVideo);
  Packet* ret=m_currentVideoPlayBackPlaylist->ReturnNextVideoPacket();
  if (ret==NULL)
  {
    CPlaylist* nextPlaylist = GetNextVideoPlaylist(m_currentVideoPlayBackPlaylist);

    if (m_currentVideoPlayBackPlaylist!=nextPlaylist)
    {
      m_currentVideoPlayBackPlaylist->SetEmptiedVideo();
      m_currentVideoPlayBackPlaylist=nextPlaylist;
      ret=m_currentVideoPlayBackPlaylist->ReturnNextVideoPacket();
    }
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetNextAudioPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionAudio);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.end();
  while (it!=m_vecPlaylists.begin())
  {
    --it;
    CPlaylist * playlist=*it;
    if (!playlist->IsEmptiedAudio() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
//      LogDebug("Next Audio Playlist %d HasAudio %d",playlist->nPlaylist,playlist->IsFakingAudio());
      return playlist;
    }
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetPlaylist(int nPlaylist)
{
  CAutoLock lock (&m_sectionAudio);
  CPlaylist * ret = NULL;
  ivecPlaylists it = m_vecPlaylists.end();
  while (it!=m_vecPlaylists.begin())
  {
    --it;
    CPlaylist * playlist=*it;
    if (playlist->nPlaylist == nPlaylist)
    {
//      LogDebug("Next Audio Playlist %d HasAudio %d",playlist->nPlaylist,playlist->IsFakingAudio());
      return playlist;
    }
  }
  LogDebug("Playlist %d not found!",nPlaylist);
  return ret;
}

CPlaylist * CPlaylistManager::GetNextVideoPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionVideo);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (!playlist->IsEmptiedVideo() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
//      LogDebug("Next Video Playlist %d",playlist->nPlaylist);
      return playlist;
    }
    ++it;
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetNextAudioSubmissionPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionAudio);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (!playlist->IsFilledAudio() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
      LogDebug("Playlist Manager New Audio Submission Playlist %d",playlist->nPlaylist);
      return playlist;
    }
    ++it;
  }
  return ret;
}

CPlaylist * CPlaylistManager::GetNextVideoSubmissionPlaylist(CPlaylist* currentPlaylist)
{
  CAutoLock lock (&m_sectionVideo);
  CPlaylist * ret = currentPlaylist;
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (!playlist->IsFilledVideo() && playlist->nPlaylist !=currentPlaylist->nPlaylist)
    {
      LogDebug("Playlist Manager New Video Submission Playlist %d",playlist->nPlaylist);
      return playlist;
    }
    ++it;
  }
  return ret;
}

void CPlaylistManager::FlushAudio(void)
{
  CAutoLock lock (&m_sectionAudio);
  LogDebug("Playlist Manager Flush Audio");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    playlist->FlushAudio();
    ++it;
  }
  m_currentAudioPlayBackPlaylist=m_currentAudioSubmissionPlaylist=*m_vecPlaylists.begin();

}

void CPlaylistManager::FlushVideo(void)
{
  CAutoLock lock (&m_sectionVideo);
  LogDebug("Playlist Manager Flush Video");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    playlist->FlushVideo();
    ++it;
  }
}

bool CPlaylistManager::HasAudio()
{
  if (m_currentAudioPlayBackPlaylist==NULL) return false;
  if (m_currentAudioPlayBackPlaylist->HasAudio()) return true;
  CPlaylist * nextPlaylist = GetNextAudioPlaylist(m_currentAudioPlayBackPlaylist);
  if (nextPlaylist!=m_currentAudioPlayBackPlaylist)
  {
    LogDebug("playlistManager: testing audio playback for playlist %d - %d",nextPlaylist->nPlaylist, nextPlaylist->HasAudio());
    return nextPlaylist->HasAudio();
  }
  return false;
}
bool CPlaylistManager::HasVideo()
{
  if (m_currentVideoPlayBackPlaylist==NULL) return false;
  if (m_currentVideoPlayBackPlaylist->HasVideo()) return true;
  CPlaylist * nextPlaylist = GetNextVideoPlaylist(m_currentVideoPlayBackPlaylist);
  if (nextPlaylist!=m_currentVideoPlayBackPlaylist)
  {
//    LogDebug("playlistManager: testing video playback for playlist %d - %d",nextPlaylist->nPlaylist, nextPlaylist->HasVideo());
    return nextPlaylist->HasVideo();
  }
  return false;
}

void CPlaylistManager::IgnoreNextDiscontinuity()
{
  LogDebug("CPlaylistManager::IgnoreNextDiscontinuity point in stream");
//  m_bIgnoreAudioSeeking=true;
//  m_bIgnoreVideoSeeking=true;
}

void CPlaylistManager::ClearAllButCurrentClip(bool resetClip, REFERENCE_TIME rtClipStartPoint)
{
  CAutoLock locka (&m_sectionAudio);
  CAutoLock lockv (&m_sectionVideo);

  LogDebug("CPlaylistManager::ClearAllButCurrentClip");
  ivecPlaylists it = m_vecPlaylists.begin();
  while (it!=m_vecPlaylists.end())
  {
    CPlaylist * playlist=*it;
    if (playlist==m_vecPlaylists.back())
    {
      ++it;
    }
    else
    {
      it=m_vecPlaylists.erase(it);
      delete playlist;
    }
  }
  if (m_vecPlaylists.size()>0)
  {
    m_currentAudioPlayBackPlaylist=m_currentVideoPlayBackPlaylist=m_currentAudioSubmissionPlaylist=m_currentVideoSubmissionPlaylist=m_vecPlaylists.back();
//    m_rtPlaylistOffset += m_vecPlaylists.back()->ClearAllButCurrentClip(resetClip, rtClipStartPoint);
  }
  else
  {
    //m_rtPlaylistOffset=0;
  }
  //if (resetClip) m_rtPlaylistOffset=0;
}

REFERENCE_TIME CPlaylistManager::Incomplete()
{
  REFERENCE_TIME ret = 0LL;
  if (!m_vecPlaylists.empty())
  {
    ret = m_vecPlaylists.back()->Incomplete();
  }
    
  return ret;
}

REFERENCE_TIME CPlaylistManager::ClipPlayTime()
{
  REFERENCE_TIME ret = 0LL;
  if (!m_vecPlaylists.empty())
  {
    ret = m_vecPlaylists.back()->PlayedDuration();
  }
    
  return ret;
}

void CPlaylistManager::SetVideoPMT(AM_MEDIA_TYPE *pmt, int nPlaylist, int nClip)
{
  if (pmt)
  {
    bool seekRequired = false;
    if (!m_vPmt)
    {
      m_vPmt = pmt;
    }
    else if (pmt->subtype != m_vPmt->subtype)  //  TODO check if extra code needed for Cyberlink etc
    {
      seekRequired = true;
      m_vPmt = pmt;
    }
    
    LogDebug("CPlaylistManager: Setting video PMT {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X} for (%d, %d)",
	  pmt->subtype.Data1, pmt->subtype.Data2, pmt->subtype.Data3,
      pmt->subtype.Data4[0], pmt->subtype.Data4[1], pmt->subtype.Data4[2],
      pmt->subtype.Data4[3], pmt->subtype.Data4[4], pmt->subtype.Data4[5], 
      pmt->subtype.Data4[6], pmt->subtype.Data4[7], nPlaylist, nClip);
    CPlaylist* pl=GetPlaylist(nPlaylist);
    if (pl)
    {
      pl->SetVideoPMT(pmt, nClip, seekRequired);
      if (seekRequired) m_rtPlaylistOffset = 0;
    }  
  }
}
