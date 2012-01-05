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

#include "StdAfx.h"

#include <afx.h>
#include <atlbase.h>
#include <atlcoll.h>
#include <vector>
#include "Playlist.h"
#include "Packet.h"
#include <streams.h>

//#define LOG_AUDIO_PACKETS
//#define LOG_VIDEO_PACKETS

using namespace std;

class CPlaylistManager
{
public:
  CPlaylistManager(void);
  ~CPlaylistManager(void);
  bool CreateNewPlaylistClip(int nPlaylist, int nClip, bool audioPresent, REFERENCE_TIME firstPacketTime, REFERENCE_TIME clipOffsetTime, REFERENCE_TIME duration);
  void SetVideoPMT(AM_MEDIA_TYPE *pmt, int nPlaylist, int nClip);

  bool SubmitAudioPacket(Packet * packet);
  bool SubmitVideoPacket(Packet * packet);
  void FlushAudio(void);
  void FlushVideo(void);
  void ClearAllButCurrentClip();
  Packet* GetNextAudioPacket();
  Packet* GetNextAudioPacket(int playlist, int clip);
  Packet* GetNextVideoPacket();
  bool HasAudio();
  bool HasVideo();

protected:
  typedef vector<CPlaylist*>::iterator ivecPlaylists;
  typedef vector<CClip*>::iterator ivecClip;

  void PushPlaylists();
  void PopPlaylists(int difference);

  bool firstVideo, firstAudio;

  REFERENCE_TIME Incomplete();
  REFERENCE_TIME ClipPlayTime();
  REFERENCE_TIME m_rtPlaylistOffset;

  vector<CPlaylist *> m_vecPlaylists;
  vector<CClip*> m_vecNonFilledClips;

  ivecPlaylists m_itCurrentAudioPlayBackPlaylist;
  ivecPlaylists m_itCurrentVideoPlayBackPlaylist;
  ivecPlaylists m_itCurrentAudioSubmissionPlaylist;
  ivecPlaylists m_itCurrentVideoSubmissionPlaylist;
  int m_itCurrentAudioPlayBackPlaylistPos;
  int m_itCurrentVideoPlayBackPlaylistPos;
  int m_itCurrentAudioSubmissionPlaylistPos;
  int m_itCurrentVideoSubmissionPlaylistPos;

  CCritSec m_sectionAudio;
  CCritSec m_sectionVideo;
  CCritSec m_sectionVector;
};

