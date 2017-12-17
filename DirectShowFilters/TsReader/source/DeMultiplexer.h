/*
 *  Copyright (C) 2005 Team MediaPortal
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
#include "FrameHeaderParser.h"
#include "TsAVRT.h"

using namespace std;
class CTsReaderFilter;

// Used for H.264 video stream demuxing
class Packet : public CAtlArray<BYTE>
{
public:
//	DWORD TrackNumber;
//	BOOL bDiscontinuity; //, bNewRtStart, bSyncPoint, bAppendable;
	static const REFERENCE_TIME INVALID_TIME = _I64_MIN;
	REFERENCE_TIME rtStart, rtPrevStart;
//	AM_MEDIA_TYPE* pmt;
	Packet() {/*pmt = NULL;*/ /*bDiscontinuity = FALSE;*/ /*bNewRtStart = FALSE;*/}
	virtual ~Packet() {/*if(pmt) DeleteMediaType(pmt);*/}
	virtual int GetDataSize() {return GetCount();}
	void SetData(const void* ptr, DWORD len) {SetCount(len); memcpy(GetData(), ptr, len);}
};

class CDeMultiplexer : public CPacketSync, public IPatParserCallback, public TSThread, public TsAVRT
{
public:
  CDeMultiplexer( CTsDuration& duration,CTsReaderFilter& filter);
  virtual ~CDeMultiplexer(void);

  bool       Start(DWORD timeout);
  void       Flush(bool clearAVready, bool isMidStream);
  CBuffer*   GetVideo(bool earlyStall);
  CBuffer*   GetAudio(bool earlyStall, CRefTime rtStartTime);
  CBuffer*   GetSubtitle();
  void       EraseAudioBuff();
  void       EraseVideoBuff();
  void       OnTsPacket(byte* tsPacket, int bufferOffset, int bufferLength);
  void       OnNewChannel(CChannelInfo& info);
  void       SetFileReader(FileReader* reader);
  void       FillSubtitle(CTsHeader& header, byte* tsPacket);
  bool       CheckContinuity(int prevCC, CTsHeader& header);
  void       FillAudio(CTsHeader& header, byte* tsPacket, int bufferOffset, int bufferLength);
  void       FillVideo(CTsHeader& header, byte* tsPacket, int bufferOffset, int bufferLength);
  void       FillVideoH264(CTsHeader& header, byte* tsPacket);
  void       FillVideoMPEG2(CTsHeader& header, byte* tsPacket);
  void       FillVideoHEVC(CTsHeader& header, byte* tsPacket);
  void       FillTeletext(CTsHeader& header, byte* tsPacket);
  void       SetEndOfFile(bool bEndOfFile);
  CPidTable  GetPidTable();
  bool       CheckCompensation(CRefTime rtStartTime);

  int        GetAudioBufferPts(CRefTime& First, CRefTime& Last) ;
  int        GetAudioBufferCnt();
  int        GetVideoBufferPts(CRefTime& First, CRefTime& Last, CRefTime& Zero) ;
  int        GetVideoBufferCnt();
  int        GetVideoBuffCntFt(double* frameTime);
  void       GetBufferCounts(int* ACnt, int* VCnt);
  int        GetRTSPBufferSize();

  bool       SetAudioStream(__int32 stream);
  bool       GetAudioStream(__int32 &stream);

  void       GetAudioStreamInfo(int stream,char* szName);
  bool       GetAudioStreamType(int stream,CMediaType&  pmt, int iPosition);
  bool       GetVideoStreamType(CMediaType &pmt);
  int        GetAudioStreamCount();

  // TsReader::ISubtitleStream uses these
  bool       SetSubtitleStream(__int32 stream);
  bool       GetSubtitleStreamType(__int32 stream, __int32& count);
  bool       GetSubtitleStreamCount(__int32 &count);
  bool       GetCurrentSubtitleStream(__int32 &stream);
  bool       GetSubtitleStreamLanguage(__int32 stream, char* szLanguage);
  bool       SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int c, void* opts, int* select));

  bool       EndOfFile();
  void       ThreadProc();
  void       FlushVideo(bool isMidStream);
  void       FlushAudio();
  void       FlushCurrentAudio();
  void       FlushSubtitle();
  void       FlushTeletext();
  int        GetVideoServiceType();

  void SetTeletextEventCallback(int (CALLBACK *pTeletextResetCallback)(int,DWORD64));
  void SetTeletextPacketCallback(int (CALLBACK *pTeletextPacketCallback)(byte*, int));
  void SetTeletextServiceInfoCallback(int (CALLBACK *pTeletextServiceInfoCallback)(int, byte,byte,byte,byte));

  void CallTeletextEventCallback(int eventCode,unsigned long int eventValue);

  void SetMediaChanging(bool onOff);
  bool IsMediaChanging();
  void RequestNewPat(void);
  void ClearRequestNewPat(void);
  void ResetPatInfo(void);
  bool IsNewPatReady(void);
  void SetAudioChanging(bool onOff);
  bool IsAudioChanging(void);
  
  bool AudPidGood(void);
  bool VidPidGood(void);
  bool SubPidGood(void);
  bool PatParsed(void);
  void CheckMediaChange(unsigned int Pid, bool isVideo);

  int  ReadAheadFromFile(ULONG lDataLength);
  bool CheckPrefetchState(bool isNormal, bool isForced);

  void DelegatedFlush(bool forceNow, bool waitForFlush);
  void PrefetchData();
  
  DWORD GetMaxFileReadLatency();
  float GetAveFileReadLatency();

  bool m_DisableDiscontinuitiesFiltering;
  DWORD m_LastDataFromRtsp;
  bool m_bFlushDelegated;
  bool m_bFlushDelgNow;
  bool m_bFlushRunning;
  bool m_bReadAheadFromFile;

  float m_sampleTime;
  float m_sampleTimePrev;
  unsigned long m_byteRead;
  float m_bitRate;

  bool m_bAudioSampleLate;
  long m_AVDataLowCount;
  long m_AudioDataLowPauseTime;
  DWORD m_targetAVready;
  bool  m_bSubtitleCompensationSet;
  bool m_bShuttingDown;
  double m_dVidPTSJumpLimit;
  double m_dfAudSampleDuration;
  
  DWORD  m_lastFlushTime;
  
private:
  struct stAudioStream
  {
    int pid;
    int audioType;
    char language[7];
  };
  struct stSubtitleStream
  {
    int pid;
    int subtitleType;  // 0=DVB, 1=teletext <-- needs enum
    char language[4];
  };

  vector<struct stAudioStream> m_audioStreams;
  vector<struct stSubtitleStream> m_subtitleStreams;
  int ReadFromFile(ULONG lDataLength);
  bool m_bEndOfFile;
  HRESULT RenderFilterPin(CBasePin* pin, bool isAudio, bool isVideo);
  CCritSec m_sectionFlushAudio;
  CCritSec m_sectionFlushVideo;
  CCritSec m_sectionFlushSubtitle;
  CCritSec m_sectionAudio;
  CCritSec m_sectionVideo;
  CCritSec m_sectionSubtitle;
  CCritSec m_sectionRead;
  CCritSec m_sectionAudioChanging;
  CCritSec m_sectionMediaChanging;
  CCritSec m_sectionSetAudioStream;
  FileReader* m_reader;
  CPatParser m_patParser;
  CMpegPesParser *m_mpegPesParser;
  CPidTable m_pids;
  vector<CBuffer*> m_vecSubtitleBuffers;
  vector<CBuffer*> m_vecVideoBuffers;
  vector<CBuffer*> m_vecAudioBuffers;
  vector<CBuffer*> m_t_vecAudioBuffers;
  typedef vector<CBuffer*>::iterator ivecBuffers;
  int  m_AudioPrevCC;
  int  m_VideoPrevCC;
  bool m_AudioValidPES;
  bool m_VideoValidPES;
  bool m_mVideoValidPES;
  int  m_WaitHeaderPES;
  CRefTime  m_FirstAudioSample;
  CRefTime  m_LastAudioSample;
  CRefTime  m_FirstVideoSample;
  CRefTime  m_LastVideoSample;
  CRefTime  m_ZeroVideoSample;

  CBuffer* m_pCurrentTeletextBuffer;
  CBuffer* m_pCurrentSubtitleBuffer;
  CBuffer* m_pCurrentAudioBuffer;
  CPcr     m_streamPcr;
  CPcr     m_lastVideoPTS;
  CPcr     m_lastVideoDTS;
  CPcr     m_lastAudioPTS;
  double   m_minVideoPTSdiff;
  double   m_minVideoDTSdiff;
  int      m_vidPTScount;
  int      m_vidDTScount;
  bool     m_bLogFPSfromDTSPTS;
  bool     m_bUsingGOPtimestamp;
  CTsDuration& m_duration;
  CTsReaderFilter& m_filter;
  unsigned int m_iAudioStream;
  int m_AudioStreamType;

  unsigned int m_audioPid;
  unsigned int m_currentSubtitlePid;
  unsigned int m_iSubtitleStream;
  unsigned int m_currentTeletextPid; 

  unsigned int m_iAudioReadCount;

  int m_iAudioIdx;
  int m_iPatVersion;
  int m_ReqPatVersion;
  DWORD m_WaitNewPatTmo;
  DWORD m_WaitGoodPatTmo;
  bool m_bWaitGoodPat;

  bool m_bFirstGopFound;
  bool m_bSecondGopFound;
  bool m_bFrame0Found;

  bool  m_bWaitForMediaChange;
  DWORD m_tWaitForMediaChange;
  bool  m_bWaitForAudioSelection;
  DWORD m_tWaitForAudioSelection;

  bool m_bStarting;

  bool m_videoChanged;
  bool m_audioChanged;
  bool m_bSetAudioDiscontinuity;
  bool m_bSetVideoDiscontinuity;
  CPcr m_subtitlePcr;

  int (CALLBACK *pTeletextServiceInfoCallback)(int, byte,byte,byte,byte);
  int (CALLBACK *pTeletextPacketCallback)(byte*, int);
  int (CALLBACK *pTeletextEventCallback)(int,DWORD64);
  int (CALLBACK *pSubUpdateCallback)(int c, void* opts,int* bi);

  // Used only for H.264 stream demuxing
  CAutoPtr<Packet> m_p;
  CAutoPtrList<Packet> m_pl;
  bool m_fHasAccessUnitDelimiters;
  DWORD m_lastStart;
  CPcr m_VideoPts;
  CPcr m_CurrentVideoPts;
  bool m_bInBlock;
  double m_curFramePeriod;
  int m_LastValidFrameCount;
  CPcr m_LastValidFramePts;

  bool m_bAudioAtEof;
  bool m_bVideoAtEof;

  float m_MinAudioDelta;
  float m_MinVideoDelta;

  
  int m_lastVidResX;
  int m_lastVidResY;

  int m_lastARX;
  int m_lastARY;

  int m_lastStreamType;
  
  bool m_mpegParserReset;
  bool m_bFirstGopParsed;
  bool m_bPatParsed;
  
  bool m_isNewNALUTimestamp;
  bool m_bVideoPTSroff;
  
  int  m_initialAudioSamples;
  int  m_initialVideoSamples;
  DWORD  m_prefetchLoopDelay;
  
  byte* m_pFileReadBuffer;
  
  DWORD m_currentAudHeader;
  DWORD m_lastAudHeader;
  int m_audHeaderCount;
  int m_hadPESfail;
  
  DWORD m_fileReadLatency;
  DWORD m_maxFileReadLatency;
  DWORD m_fileReadLatSum;
  DWORD m_fileReadLatCount;

  int m_audioBytesRead;
    
};
