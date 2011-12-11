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

#pragma once

#include <afx.h>
#include <atlbase.h>
#include <atlcoll.h>
#include <vector>
#include "Playlist.h"
#include "Packet.h"
#include <streams.h>

#define LOG_AUDIO_PACKETS
#define LOG_VIDEO_PACKETS

using namespace std;

class CPlaylistManager
{
public:
  CPlaylistManager(void);
  ~CPlaylistManager(void);
  bool CreateNewPlaylistClip(int nPlaylist, int nClip, bool audioPresent, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffsetTime, REFERENCE_TIME duration, bool discontinuousClip);
  void SetVideoPMT(AM_MEDIA_TYPE *pmt, int nPlaylist, int nClip);

  bool SubmitAudioPacket(Packet * packet);
  bool SubmitVideoPacket(Packet * packet);
  void FlushAudio(void);
  void FlushVideo(void);
  void ClearAllButCurrentClip(bool resetClip);
  void IgnoreNextDiscontinuity();
  Packet* GetNextAudioPacket();
  Packet* GetNextAudioPacket(int playlist, int clip);
  Packet* GetNextVideoPacket();
  bool HasAudio();
  bool HasVideo();

protected:
  typedef vector<CPlaylist*>::iterator ivecPlaylists;
  typedef vector<CClip*>::iterator ivecClip;
  CPlaylist * GetNextAudioPlaylist(CPlaylist* currentPlaylist);
  CPlaylist * GetNextVideoPlaylist(CPlaylist* currentPlaylist);
  CPlaylist * GetNextAudioSubmissionPlaylist(CPlaylist* currentPlaylist);
  CPlaylist * GetNextVideoSubmissionPlaylist(CPlaylist* currentPlaylist);
  CPlaylist * GetPlaylist(int playlist);

  REFERENCE_TIME Incomplete();
  REFERENCE_TIME ClipPlayTime();
  REFERENCE_TIME m_rtPlaylistOffset;

  vector<CPlaylist *> m_vecPlaylists;
  vector<CClip*> m_vecNonFilledClips;

  AM_MEDIA_TYPE *m_vPmt;
  AM_MEDIA_TYPE *m_aPmt;

  CPlaylist * m_currentAudioPlayBackPlaylist;
  CPlaylist * m_currentVideoPlayBackPlaylist;
  CPlaylist * m_currentAudioSubmissionPlaylist;
  CPlaylist * m_currentVideoSubmissionPlaylist;

  int m_VideoPacketsUntilLatestplaylist;

  CCritSec m_sectionAudio;
  CCritSec m_sectionVideo;

  bool m_bIgnoreAudioSeeking;
  bool m_bIgnoreVideoSeeking;
};

