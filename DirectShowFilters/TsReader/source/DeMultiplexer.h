/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include "buffer.h"
#include "MultiFileReader.h"
#include "tsDuration.h"
#include "pcrdecoder.h"
#include "..\..\shared\packetSync.h"
#include "pidtable.h"
#include "..\..\shared\tsheader.h"
#include "patparser.h"
#include "channelInfo.h"
#include "PidTable.h"
#include "TSThread.h"
#include "..\..\shared\Pcr.h"
#include <vector>
#include <map>
#include <dvdmedia.h>
#include "MpegPesParser.h"
using namespace std;
class CTsReaderFilter;
class CDeMultiplexer : public CPacketSync, public IPatParserCallback
{
public:
  CDeMultiplexer( CTsDuration& duration,CTsReaderFilter& filter);
	virtual ~CDeMultiplexer(void);

  void       Start();
  void       Flush();
  CBuffer*   GetVideo();
  CBuffer*   GetAudio();
  CBuffer*   GetSubtitle();
	void       OnTsPacket(byte* tsPacket);
	void       OnNewChannel(CChannelInfo& info);
  void       SetFileReader(FileReader* reader);
  void       FillSubtitle(CTsHeader& header, byte* tsPacket);
  void       FillAudio(CTsHeader& header, byte* tsPacket);
  void       FillVideo(CTsHeader& header, byte* tsPacket);
  void       FillTeletext(CTsHeader& header, byte* tsPacket);
  void			 SetEndOfFile(bool bEndOfFile);
  CPidTable  GetPidTable();

  bool       SetAudioStream(__int32 stream);
  bool       GetAudioStream(__int32 &stream);

  void       GetAudioStreamInfo(int stream,char* szName);
  void       GetAudioStreamType(int stream,CMediaType&  pmt);
  void       GetVideoStreamType(CMediaType& pmt);
  int        GetAudioStreamCount();

  // TsReader::ISubtitleStream uses these 
  bool       SetSubtitleStream(__int32 stream);
  bool       GetSubtitleStreamType(__int32 stream, __int32& count);
  bool       GetSubtitleStreamCount(__int32 &count);
  bool       GetCurrentSubtitleStream(__int32 &stream);
  bool       GetSubtitleStreamLanguage(__int32 stream, char* szLanguage);
  bool			 SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int c, void* opts, int* select));

  bool       EndOfFile();
	bool			 HoldAudio();
	void			 SetHoldAudio(bool onOff);
	bool			 HoldVideo();
	void			 SetHoldVideo(bool onOff);
  bool			 HoldSubtitle();
	void			 SetHoldSubtitle(bool onOff);
  void       ThreadProc();
  void       FlushVideo();
  void       FlushAudio();
  void       FlushSubtitle();
  void       FlushTeletext();
  //void		 SyncTeletext();
  int        GetVideoServiceType();  

  void SetTeletextEventCallback(int (CALLBACK *pTeletextResetCallback)(int,DWORD64));
  void SetTeletextPacketCallback(int (CALLBACK *pTeletextPacketCallback)(byte*, int));
  void SetTeletextServiceInfoCallback(int (CALLBACK *pTeletextServiceInfoCallback)(int, byte,byte,byte,byte));

  void CallTeletextEventCallback(int eventCode,unsigned long int eventValue);

	bool m_DisableDiscontinuitiesFiltering ;

private:
  struct stAudioStream
  {
    int pid;
    int audioType;
    char language[4];
  };
  struct stSubtitleStream
  {
    int pid;
    int subtitleType;  // 0=DVB, 1=teletext <-- needs enum
    char language[4];
  } ;

  vector<struct stAudioStream> m_audioStreams;
  vector<struct stSubtitleStream> m_subtitleStreams;
	void ResetMpeg2VideoInfo();
  void GetVideoMedia(CMediaType *pmt);
  void GetH264Media(CMediaType *pmt);
  void GetMpeg4Media(CMediaType *pmt);
  bool ReadFromFile(bool isAudio, bool isVideo);
  bool m_bEndOfFile;
  HRESULT RenderFilterPin(CBasePin* pin, bool isAudio, bool isVideo);
  HRESULT DoStart();
  HRESULT DoStop();
  HRESULT IsStopped();
  HRESULT IsPlaying();
  CCritSec m_sectionAudio;
  CCritSec m_sectionVideo;
  CCritSec m_sectionSubtitle;
  CCritSec m_sectionRead;
	FileReader* m_reader;
  CPatParser m_patParser;
	CMpegPesParser m_mpegPesParser;
  CPidTable m_pids;
  vector<CBuffer*> m_vecSubtitleBuffers;
  vector<CBuffer*> m_vecVideoBuffers;
  vector<CBuffer*> m_t_vecVideoBuffers;
  vector<CBuffer*> m_vecAudioBuffers;
  vector<CBuffer*> m_t_vecAudioBuffers;
  typedef vector<CBuffer*>::iterator ivecBuffers;
	int  m_AudioPrevCC ;
	int  m_VideoPrevCC ;
	bool m_AudioValidPES ;
	bool m_VideoValidPES ;

  CBuffer* m_pCurrentTeletextBuffer;
  CBuffer* m_pCurrentSubtitleBuffer;
  CBuffer* m_pCurrentVideoBuffer;
  CBuffer* m_pCurrentAudioBuffer;
  CPcr     m_streamPcr;
	CPcr		 m_lastVideoPTS;
  CTsDuration& m_duration;
  CTsReaderFilter& m_filter;
  unsigned int m_iAudioStream;
  unsigned int m_audioPid;
  unsigned int m_currentSubtitlePid;
  unsigned int m_iSubtitleStream;
  unsigned int m_currentTeletextPid; // Ziphnor

  unsigned int m_iAudioReadCount;

  bool m_bScanning;
	bool m_bHoldAudio;
	bool m_bHoldVideo;
  bool m_bHoldSubtitle;
  //bool m_bPreferAC3;
  int m_iAudioIdx;
  int m_iPatVersion;
	int m_receivedPackets;

	MPEG2VIDEOINFO m_mpeg2VideoInfo;
	bool m_mpegParserTriggerFormatChange;
  bool m_bSetAudioDiscontinuity;
  bool m_bSetVideoDiscontinuity;
  CPcr m_subtitlePcr;  
  //void ReadAudioIndexFromRegistry();

  int (CALLBACK *pTeletextServiceInfoCallback)(int, byte,byte,byte,byte);
   int (CALLBACK *pTeletextPacketCallback)(byte*, int);
   int (CALLBACK *pTeletextEventCallback)(int,DWORD64);
   int (CALLBACK *pSubUpdateCallback)(int c, void* opts,int* bi);

    // used to sync teletext packets with video
	//DWORD64 m_inVideoBuffer;
	//DWORD64 m_outVideoBuffer;
};
