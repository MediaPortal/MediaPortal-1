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
#include <atlbase.h>
#include <atlcoll.h>
#include <vector>
#include <map>
#include <bluray.h>
#include "pcrdecoder.h"
#include "..\..\shared\packetSync.h"
#include "..\..\shared\tsheader.h"
#include "..\..\shared\Pcr.h"
#include "StreamParser.h"
#include "BDEventObserver.h"
#include "Packet.h"
#include "PlaylistManager.h"

using namespace std;
class CBDReaderFilter;

#define MAX_BUF_SIZE 200000
#define READ_SIZE (1344 * 60)
#define INITIAL_READ_SIZE (READ_SIZE * 1024)
#define MAX_CONSECUTIVE_READ_ERRORS 5 

class CDeMultiplexer : public BDEventObserver
{
public:
  CDeMultiplexer(CBDReaderFilter& filter);
  virtual ~CDeMultiplexer(void);

  // TODO - not all of these should be puclic!

  HRESULT    Start();
  void       Flush(bool bClearclips);
  Packet*    GetVideo();
  Packet*    GetAudio();
  void       OnTsPacket(byte* tsPacket);

  void       FillAudio(CTsHeader& header, byte* tsPacket);
  void       FillVideo(CTsHeader& header, byte* tsPacket);
  void       FillVideoH264PESPacket(CTsHeader* header, CAutoPtr<Packet> p, bool pFlushBuffers = false);
  void       FillVideoVC1PESPacket(CTsHeader* header, CAutoPtr<Packet> p, bool pFlushBuffers = false);
  void       FillVideoH264(CTsHeader* header, byte* tsPacket);
  void       FillVideoMPEG2(CTsHeader* header, byte* tsPacket, bool pFlushBuffers = false);

  bool       ParseVideoFormat(Packet* p);
  bool       ParseAudioFormat(Packet* p);
  void       ParseVideoStream(BLURAY_CLIP_INFO* clip);
  void       ParseAudioStreams(BLURAY_CLIP_INFO* clip);
  void       ParseSubtitleStreams(BLURAY_CLIP_INFO* clip);

  void       SetEndOfFile(bool bEndOfFile);

  bool       SetAudioStream(__int32 stream);
  bool       GetAudioStream(__int32 &stream);

  void       GetAudioStreamInfo(int stream, char* szName);
  int        GetAudioStreamType(int stream);
  int        GetAudioChannelCount(int stream);
  int        GetCurrentAudioStreamType();
  void       GetAudioStreamPMT(CMediaType& pmt);
  void       GetVideoStreamPMT(CMediaType &pmt);
  void       GetSubtitleStreamPMT(CMediaType& pmt);
  int        GetAudioStreamCount();

  REFERENCE_TIME TitleDuration();

  bool       GetSubtitleStreamCount(__int32 &count);
  bool       GetSubtitleStreamLanguage(__int32 stream, char* szLanguage);
  
  bool       EndOfFile();
  bool       HoldAudio();
  void       SetHoldAudio(bool onOff);
  void       SetHoldVideo(bool onOff);
  bool       HoldVideo();
  void       ThreadProc();
  void       FlushVideo();
  void       FlushAudio();
  int        GetVideoServiceType();

  void SetMediaChanging(bool onOff);
  bool IsMediaChanging();

  // From BDEventObserver
  void HandleBDEvent(BD_EVENT& pEv);
  void HandleOSDUpdate(OSDTexture& pTexture);

  CAMEvent* m_eAudioClipSeen;
  bool m_bVideoClipSeen;
  bool m_bAudioRequiresRebuild;
  bool m_bVideoRequiresRebuild;
  bool m_bRebuildOngoing;
  bool m_bStreamPaused;
  bool m_bAudioWaitForSeek;
  bool m_bAudioResetStreamPosition;

  bool m_bTitleChanged;
  REFERENCE_TIME m_rtStallTime;
  REFERENCE_TIME m_rtTitleChangeStarted;

  CCritSec m_sectionRead;

    REFERENCE_TIME m_rtOffset;

private:
  void PacketDelivery(CAutoPtr<Packet> p);

  bool AudioStreamsAvailable(BLURAY_CLIP_INFO* pClip);
  char* StreamFormatAsString(int pStreamType);
  LPCTSTR StreamAudioFormatAsString(int pStreamAudioChannel);

  struct stAudioStream
  {
    int pid;
    int audioType;
    char language[7];
    int audioChannelCount;
  };

  struct stSubtitleStream
  {
    int pid;
    int subtitleType;  // 0=DVB, 1=teletext <-- needs enum
    char language[4];
  };

  vector<struct stAudioStream> m_audioStreams;
  vector<struct stSubtitleStream> m_subtitleStreams;

  int m_nVideoPid;

  int ReadFromFile();
  bool m_bEndOfFile;
  
  CCritSec m_sectionAudio;
  CCritSec m_sectionVideo;
  CCritSec m_sectionMediaChanging;

  StreamParser* m_videoParser;
  StreamParser* m_audioParser;

  UINT32 m_nAudioPesLenght;
  
  typedef vector<Packet*>::iterator ivecVBuffers;
  typedef vector<Packet*>::iterator ivecABuffers;
  typedef vector<Packet*>::iterator ivecSBuffers;
  
  bool m_bAC3Substream;
  bool m_AudioValidPES;
  bool m_VideoValidPES;
  int  m_WaitHeaderPES;

  Packet* m_pCurrentVideoBuffer;
  Packet* m_pCurrentAudioBuffer;

  CBDReaderFilter& m_filter;
  unsigned int m_iAudioStream;

  int m_AudioStreamType;
  int m_videoServiceType;

  unsigned int m_audioPid;

  void FlushPESBuffers(bool bDiscardData, bool bSetCurrentClipFilled);

  bool m_bHoldAudio;
  bool m_bHoldVideo;
  int m_iAudioIdx;

  int m_loopLastSearch;

  bool m_bWaitForMediaChange;
  bool m_bReadFailed;

  // Used only for H.264 stream demuxing
  CAutoPtr<Packet> m_p;
  CAutoPtr<Packet> m_pBuild;
  CAutoPtrList<Packet> m_pl;
  bool m_fHasAccessUnitDelimiters;
  DWORD m_lastStart;
  CPcr m_VideoPts;
  CPcr m_CurrentVideoPts;
  bool m_bInBlock;
  double m_curFrameRate;
  int m_LastValidFrameCount;
  CPcr m_LastValidFramePts;

  bool m_bShuttingDown;
  bool m_bFlushBuffersOnPause;

  byte m_readBuffer[READ_SIZE];

  INT32 m_nClip;
  INT32 m_nTitle;
  INT32 m_nPlaylist;
  INT32 m_nMPEG2LastClip;
  INT32 m_nMPEG2LastPlaylist;
  bool m_bVideoFormatParsed;
  bool m_bAudioFormatParsed;
  
  REFERENCE_TIME m_rtTitleDuration;
  REFERENCE_TIME m_nMPEG2LastTitleDuration;

  bool m_bLibRequestedFlush;

  unsigned int m_iReadErrors;
  // Used for playlist/clip tracking
  CPlaylistManager* m_playlistManager;
};
