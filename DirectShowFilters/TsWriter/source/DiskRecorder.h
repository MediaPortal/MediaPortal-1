/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#include "multiplexer.h"
#include "multifilewriter.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "..\..\shared\TsHeader.h"
#include "..\..\shared\adaptionfield.h"
#include "..\..\shared\pcr.h"
#include "videoaudioobserver.h"
#include "PmtParser.h"
#include <vector>
#include <map>
using namespace std;
using namespace Mediaportal;


//* enum which specified the timeshifting mode 
enum RecordingMode
{
	TimeShift=0,
	Recording=1
};

enum StreamMode
{
  ProgramStream=0,
  TransportStream=1
};
//* enum which specified the pid type 
enum PidType
{
  Video=0,
  Audio=1,
  Other=2
};

typedef struct stLastPtsDtsRecord
{
  __int64 m_prevPts ;
  __int64 m_prevDts ;
  DWORD   m_prevPtsTimeStamp ;
  DWORD   m_prevDtsTimeStamp ;
  __int64 m_PtsCompensation ;
  __int64 m_DtsCompensation ;
  int m_Pid ;
} LastPtsDtsRecord;

class CDiskRecorder: public IFileWriter
{
public:
	CDiskRecorder(RecordingMode mode);
	~CDiskRecorder(void);
	
	void SetFileName(char* pszFileName);
	bool Start();
	void Stop();
	void Pause( BYTE onOff) ;
	void Reset();

	void GetRecordingMode(int *mode) ;
	void SetStreamMode(int mode) ;
	void GetStreamMode(int *mode) ;
	void SetPmtPid(int pmtPid,int serviceId,byte* pmtData,int pmtLength);

	// Only needed for timeshifting
	void SetVideoAudioObserver (IVideoAudioObserver* callback);
	void GetBufferSize( long * size) ;
	void GetNumbFilesAdded( WORD *numbAdd) ;
	void GetNumbFilesRemoved( WORD *numbRem) ;
	void GetCurrentFileId( WORD *fileID) ;
	void GetMinTSFiles( WORD *minFiles) ;
	void SetMinTSFiles( WORD minFiles) ;
	void GetMaxTSFiles( WORD *maxFiles) ;
	void SetMaxTSFiles( WORD maxFiles) ;
	void GetMaxTSFileSize( __int64 *maxSize) ;
	void SetMaxTSFileSize( __int64 maxSize) ;
	void GetChunkReserve( __int64 *chunkSize) ;
	void SetChunkReserve( __int64 chunkSize) ;
	void GetFileBufferSize( __int64 *lpllsize) ;

	void OnTsPacket(byte* tsPacket);
	void Write(byte* buffer, int len);


private:  
	void WriteToRecording(byte* buffer, int len);
	void WriteToTimeshiftFile(byte* buffer, int len);
	void WriteLog(const char *fmt, ...);
	void SetPcrPid(int pcrPid);
	bool IsStreamWanted(int stream_type);
	void AddStream(PidInfo2 pidInfo);
  void Flush();
	void WriteTs(byte* tsPacket);
  void WriteFakePAT();  
  void WriteFakePMT();

  __int64 EcPcrTime(__int64 New, __int64 Prev) ;
  void PatchPcr(byte* tsPacket,CTsHeader& header);
  void PatchPtsDts(byte* tsPacket,CTsHeader& header,PidInfo2& pidInfo);

	CMultiplexer     m_multiPlexer;
	MultiFileWriterParam m_params;
  RecordingMode    m_recordingMode;
	StreamMode			 m_streamMode;
	CPmtParser*					 m_pPmtParser;
	bool				         m_bRunning;
	char				         m_szFileName[2048];
	MultiFileWriter*     m_pTimeShiftFile;
	HANDLE							 m_hFile;
	CCriticalSection     m_section;
  int                  m_iPmtPid;
  int                  m_pcrPid;
	int									 m_iServiceId;
	vector<PidInfo2>		 m_vecPids;
//	bool								 m_bSeenAudioStart;
	bool								 m_AudioOrVideoSeen;
	int									 m_iPmtContinuityCounter;
	int									 m_iPatContinuityCounter;
  
  BOOL            m_bPaused;
  bool            m_bDetermineNewStartPcr;
	bool		        m_bStartPcrFound;
  int             m_iPacketCounter;
	int			        m_iPatVersion;
	int			        m_iPmtVersion;
	int              m_iPart;
  byte*           m_pWriteBuffer;
  int             m_iWriteBufferPos;
  CTsHeader       m_tsHeader;
  CAdaptionField  m_adaptionField;
  CPcr            m_prevPcr;

	int             m_JumpInProgress ;              // Jump detected, wait confirmation
	float           m_PcrSpeed ;                    // Time average between PCR samples
	__int64         m_PcrCompensation ;             // Compensation from PCR/PTS/DTS to fake PCR/PTS/DTS ( 33 bits offset with PCR resoluion )
	__int64         m_PcrFutureCompensation ;       // Future compensation computed during jump detection.
	DWORD           m_prevTimeStamp ;               // TimeStamp of last PCR patching.

  bool            m_bClearTsQueue;
  unsigned long   m_TsPacketCount;

  vector <LastPtsDtsRecord> m_mapLastPtsDts;
  typedef vector<LastPtsDtsRecord>::iterator imapLastPtsDts;

	IVideoAudioObserver *m_pVideoAudioObserver;
};
