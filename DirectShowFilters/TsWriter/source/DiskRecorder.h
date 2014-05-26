/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#include "multifilewriter.h"
#include "CriticalSection.h"
#include "..\..\shared\TsHeader.h"
#include "..\..\shared\adaptionfield.h"
#include "..\..\shared\pcr.h"

#include "..\..\shared\BasePmtParser.h"
#include "..\..\shared\PacketSync.h"


#include "videoaudioobserver.h"
#include <vector>
#include <map>
using namespace std;
using namespace Mediaportal;



//  Incremental buffer sizes
#define NUMBER_THROTTLE_BUFFER_SIZES  20
#define MAX_TS_PACKET_QUEUE 4
#define MAX_PES_HEADER_BYTES 19


//* enum which specified the timeshifting mode 
enum RecordingMode
{
  TimeShift=0,
  Recording=1
};

//* enum which specified the pid type 
enum PidType
{
  Video=0,
  Audio=1,
  Other=2
};

typedef struct
{
  public:
    int OriginalPid;
    int FakePid;
    bool IsStillPresent;
    bool SeenStart;
    byte PrevContinuityCounter;

    // Used to patch and/or generate PCR.
    // The main purpose is to be able to reassemble PES packet headers which
    // are split over multiple TS packets.
    int TsPacketQueueLength;
    byte TsPacketQueue[MAX_TS_PACKET_QUEUE][TS_PACKET_LEN];
    byte TsPacketPayloadStartQueue[MAX_TS_PACKET_QUEUE];
    byte PesHeader[MAX_PES_HEADER_BYTES];
    int PesHeaderLength;

    __int64 PrevPts;
    DWORD PrevPtsTimeStamp;
    __int64 PrevDts;
    DWORD PrevDtsTimeStamp;
}PidInfo;

class CDiskRecorder
{
public:
  CDiskRecorder(RecordingMode mode);
  ~CDiskRecorder(void);
  
  void SetFileName(wchar_t* fileName);
  HRESULT Start();
  void Stop();

  void GetRecordingMode(int* mode);
  HRESULT SetPmt(byte* pmt, int pmtLength);

  // Only needed for timeshifting
  void SetVideoAudioObserver(IVideoAudioObserver* callBack);
  void GetBufferSize(__int64* size);

  void SetMinTsFiles(WORD minFiles);
  void SetMaxTsFiles(WORD maxFiles);
  void SetMaxTsFileSize(__int64 maxSize);
  void SetChunkReserve(__int64 chunkSize);

  void GetTimeShiftPosition(__int64* position, long* bufferId);
  void GetDiscontinuityCount(int* count);
  void GetProcessedPacketCount(int* count);

  void OnTsPacket(byte* tsPacket);
  void Write(byte* buffer, int len);

private:
  void ClearPids();
  void WriteToRecording(byte* buffer, int len);
  void WriteToTimeshiftFile(byte* buffer, int len);
  void WriteLog(const char* fmt, ...);
  void WriteLog(const wchar_t* fmt, ...);
  void Flush();
  void InjectPcrFromPts(PidInfo& info);
  void WritePacket(byte* tsPacket);
  void CreateFakePat();
  void CreateFakePmt(CPidTable& pidTable);
  void AddPidToPmt(BasePid* pid, const string& pidType, int& nextFakePid, int& pmtOffset);
  void WriteFakeServiceInfo();

  void PatchPcr(byte* tsPacket,CTsHeader& header);
  void HandlePcrInstability(CPcr& pcrNew, __int64 pcrChange);
  void PatchPtsDts(byte* tsPacket, PidInfo& pidInfo);

  MultiFileWriterParam m_params;
  RecordingMode m_recordingMode;
  CBasePmtParser m_pmtParser;
  bool m_isRunning;
  wchar_t m_fileName[2048];
  MultiFileWriter* m_pTimeShiftFile;
  HANDLE m_hFile;
  CCriticalSection m_section;


  DWORD m_seenVideoOrAudio;

  map<int, PidInfo*> m_pids;
  unsigned long m_tsPacketCount;
  int m_discontinuityCount;

  int m_nextFakePidVideo;
  int m_nextFakePidAudio;
  int m_nextFakePidSubtitles;
  int m_nextFakePidTeletext;
  
  byte m_fakePat[TS_PACKET_LEN];
  int m_patContinuityCounter;
  int m_patVersion;
  byte m_fakePmt[MAX_SECTION_LENGTH];
  int m_pmtContinuityCounter;
  int m_pmtVersion;
  int m_serviceInfoPacketCounter;

  int m_originalPcrPid;
  int m_substitutePcrSourcePid;
  int m_fakePcrPid;
  bool m_waitingForPcr;
  CPcr m_prevPcr;
  DWORD m_prevPcrReceiveTimeStamp;
  float m_averagePcrSpeed;                    // Time average between PCR samples
  __int64 m_pcrCompensation;             // Compensation from PCR/PTS/DTS to fake PCR/PTS/DTS ( 33 bits offset with PCR resoluion )
  int m_pcrGapConfirmations;
  __int64 m_pcrFutureCompensation;
  bool m_generatePcrFromPts;

  int             m_iPart;
  byte*           m_pWriteBuffer;
  int             m_iWriteBufferPos;
  int        m_iWriteBufferSize;
  int      m_iThrottleBufferSizes[NUMBER_THROTTLE_BUFFER_SIZES];
  int        m_writeBufferThrottle;
  bool        m_throttleAtMax;
  int         m_throttleMaxPacketCount;
  CTsHeader       m_tsHeader;

  bool            m_bClearTsQueue;

  IVideoAudioObserver *m_pVideoAudioObserver;
};
