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
  bool CreateNewClip(int clipNumber, REFERENCE_TIME clipStart, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, REFERENCE_TIME playlistClipOffset);
  bool RemoveRedundantClips(); // returns true if no clips left;
  bool AcceptAudioPacket(Packet*  packet);
  bool AcceptVideoPacket(Packet*  packet);
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
  REFERENCE_TIME playlistFirstPacketTime;
  REFERENCE_TIME ClearAllButCurrentClip(REFERENCE_TIME totalStreamOffset);
  bool HasAudio();
  bool HasVideo();
  REFERENCE_TIME Incomplete();
  REFERENCE_TIME PlayedDuration();
  void SetVideoPMT(AM_MEDIA_TYPE * pmt, int nClip);
  vector<CClip*> Superceed();

protected:
  typedef vector<CClip*>::iterator ivecClip;

  void Reset(int playlistNumber, REFERENCE_TIME firstPacketTime);

  vector<CClip*> m_vecClips;
  ivecClip m_itCurrentAudioPlayBackClip;
  ivecClip m_itCurrentVideoPlayBackClip;
  ivecClip m_itCurrentAudioSubmissionClip;
  ivecClip m_itCurrentVideoSubmissionClip;
  int m_itCurrentAudioPlayBackClipPos;
  int m_itCurrentVideoPlayBackClipPos;
  int m_itCurrentAudioSubmissionClipPos;
  int m_itCurrentVideoSubmissionClipPos;

  bool playlistFilledAudio;
  bool playlistFilledVideo;
  bool playlistEmptiedVideo;
  bool playlistEmptiedAudio;

  bool firstAudioPESPacketSeen;
  bool firstVideoPESPacketSeen;
  REFERENCE_TIME firstAudioPESTimeStamp;
  REFERENCE_TIME firstVideoPESTimeStamp;

  CCritSec m_sectionVector;

  bool firstPacketRead;

  void PushClips();
  void PopClips();

};

