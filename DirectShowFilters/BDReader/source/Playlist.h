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

#include "Packet.h"
#include "Clip.h"
#include <vector>
#include <mtype.h>

using namespace std;

class CPlaylist
{
public:
  CPlaylist(int playlistNumber, REFERENCE_TIME firstPacketTime);
  ~CPlaylist(void);
  Packet* ReturnNextAudioPacket();
  Packet* ReturnNextAudioPacket(int clip);
  Packet* ReturnNextVideoPacket();
  bool CreateNewClip(int clipNumber, REFERENCE_TIME clipStart, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, REFERENCE_TIME playlistClipOffset, bool discontinuousClip);
  bool RemoveRedundantClips(); // returns true if no clips left;
  bool AcceptAudioPacket(Packet*  packet, bool seeking);
  bool AcceptVideoPacket(Packet*  packet, bool firstPacket, bool seeking);
  void FlushAudio();
  void FlushVideo();
  bool IsEmptiedAudio();
  bool IsEmptiedVideo();
  void SetEmptiedVideo();
  void SetEmptiedAudio();
  bool IsFilledAudio();
  bool IsFilledVideo();
  void SetFilledVideo();
  void SetFilledAudio();
  int  nPlaylist;
  int  CurrentAudioSubmissionClip();
  int  CurrentVideoSubmissionClip();
  bool IsFakingAudio();
  bool IsFakeAudioAvailable();
  REFERENCE_TIME playlistFirstPacketTime;
  REFERENCE_TIME ClearAllButCurrentClip(bool resetClip);
  bool HasAudio();
  bool HasVideo();
  REFERENCE_TIME Incomplete();
  REFERENCE_TIME PlayedDuration();
  void SetVideoPMT(AM_MEDIA_TYPE * pmt, int nClip, bool changingMediaType);
  vector<CClip*> Superceed();

protected:
  typedef vector<CClip*>::iterator ivecClip;

  CClip * GetNextAudioClip(CClip * currentClip, int superceedType);
  CClip * GetNextVideoClip(CClip * currentClip, int superceedType);
  CClip * GetClip(int nClip);

  REFERENCE_TIME GetPacketTimeStampCorrection(CClip * packetClip);
  Packet * CorrectTimeStamp(CClip * packetClip, Packet* packet);
  void Reset(int playlistNumber, REFERENCE_TIME firstPacketTime);

  vector<CClip*> m_vecClips;
  CClip * m_currentAudioPlayBackClip;
  CClip * m_currentVideoPlayBackClip;
  CClip * m_currentAudioSubmissionClip;
  CClip * m_currentVideoSubmissionClip;

  bool playlistFilledAudio;
  bool playlistFilledVideo;
  bool playlistEmptiedVideo;
  bool playlistEmptiedAudio;

  int m_VideoPacketsUntilLatestClip;

  bool firstAudioPESPacketSeen;
  bool firstVideoPESPacketSeen;
  REFERENCE_TIME firstAudioPESTimeStamp;
  REFERENCE_TIME firstVideoPESTimeStamp;

  bool firstPacketRead;
};

