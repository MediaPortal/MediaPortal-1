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
 */#pragma once

#include "Packet.h"
#include <map>
#include <vector>
#include <dshow.h>
//#include <mtype.h>

using namespace std;

// TODO - enum
#define SUPERCEEDED_AUDIO    1
#define SUPERCEEDED_VIDEO    2
#define SUPERCEEDED_SUBTITLE 4


class CClip
{
public:
  CClip(int clipNumber, REFERENCE_TIME playlistFirstPacketTime, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, bool seekNeeded);
  ~CClip(void);
  Packet* ReturnNextAudioPacket(REFERENCE_TIME playlistOffset);
  Packet* ReturnNextVideoPacket(REFERENCE_TIME playlistOffset);
  bool AcceptAudioPacket(Packet*  packet, bool forced);
  bool AcceptVideoPacket(Packet*  packet, bool forced);
  void FlushAudio(void);
  void FlushVideo(void);
  int  nClip;
  bool noAudio;
  bool clipFilled;
  bool clipEmptied;
  void Superceed(int superceedType);
  bool IsSuperceeded(int superceedType);
  REFERENCE_TIME playlistFirstPacketTime;
  REFERENCE_TIME clipPlaylistOffset;
  int AudioPacketCount();
  int VideoPacketCount();
  void Reset();
  bool FakeAudioAvailable();
  bool HasAudio();
  bool HasVideo();
  bool Incomplete();
  void SetPMT(AM_MEDIA_TYPE *pmt);

protected:
  typedef vector<Packet*>::iterator ivecVideoBuffers;
  typedef vector<Packet*>::iterator ivecAudioBuffers;
  vector<Packet*> m_vecClipAudioPackets;
  vector<Packet*> m_vecClipVideoPackets;
  AM_MEDIA_TYPE *m_pmt;
  REFERENCE_TIME clipDuration;
  REFERENCE_TIME audioPlaybackpoint;
  REFERENCE_TIME lastAudioPosition;
  REFERENCE_TIME lastVideoPosition;
  int superceeded;

  bool firstAudio;
  bool firstVideo;
  bool bSeekNeeded;

  Packet* GenerateFakeAudio(REFERENCE_TIME rtStart);
};

