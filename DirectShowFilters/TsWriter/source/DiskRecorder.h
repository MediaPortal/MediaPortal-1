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
#include <ctime>
#include <map>
#include <sstream>
#include "..\..\shared\BasePmtParser.h"
#include "..\..\shared\PacketSync.h"
#include "..\..\shared\Pcr.h"
#include "..\..\shared\TsHeader.h"
#include "CriticalSection.h"
#include "EnterCriticalSection.h"
#include "IChannelObserver.h"
#include "MultiFileWriter.h"
#include "RecorderMode.h"

using namespace MediaPortal;
using namespace std;


#define MAX_TS_PACKET_QUEUE 4
#define MAX_PES_HEADER_BYTES 19     // The maximum number of bytes required to hold the contents of a PES packet up to and including the last DTS byte.

class CDiskRecorder
{
  public:
    CDiskRecorder(RecorderMode mode);
    ~CDiskRecorder();
  
    HRESULT SetPmt(unsigned char* pmt,
                    unsigned short pmtSize,
                    bool isDynamicPmtChange);
    void SetObserver(IChannelObserver* observer);
    HRESULT SetFileName(wchar_t* fileName);
    HRESULT Start();
    void GetStreamQualityCounters(unsigned long long& countTsPackets,
                                  unsigned long long& countDiscontinuities,
                                  unsigned long long& countDroppedBytes);
    void Stop();

    // time-shifting only
    HRESULT SetTimeShiftingParameters(unsigned long fileCountMinimum,
                                      unsigned long fileCountMaximum,
                                      unsigned long long fileSizeBytes);
    void GetTimeShiftingParameters(unsigned long& fileCountMinimum,
                                    unsigned long& fileCountMaximum,
                                    unsigned long long& fileSizeBytes);
    void GetTimeShiftingFilePosition(unsigned long long& position, unsigned long& bufferId);

    void OnTsPacket(CTsHeader& header, unsigned char* tsPacket);

  private:
    typedef struct
    {
      public:
        unsigned short OriginalPid;
        unsigned short FakePid;
        bool IsStillPresent;
        bool SeenStart;
        unsigned char PrevContinuityCounter;

        // Used to patch and/or generate PCR.
        // The main purpose is to be able to reassemble PES packet headers which
        // are split over multiple TS packets.
        unsigned char TsPacketQueueLength;
        unsigned char TsPacketQueue[MAX_TS_PACKET_QUEUE][TS_PACKET_LEN];
        unsigned char TsPacketPayloadStartQueue[MAX_TS_PACKET_QUEUE];
        unsigned char PesHeader[MAX_PES_HEADER_BYTES];
        unsigned short PesHeaderLength;

        long long PrevPts;
        clock_t PrevPtsTimeStamp;
        long long PrevDts;
        clock_t PrevDtsTimeStamp;
    }PidInfo;

    void WriteLog(const wchar_t* fmt, ...);
    void ClearPids();

    void WritePacket(unsigned char* tsPacket);
    void WritePacketDirect(unsigned char* tsPacket);

    void CreateFakePat();
    void CreateFakePmt(CPidTable& pidTable);
    void AddPidToPmt(BasePid* pid,
                      const string& pidType,
                      unsigned short& nextFakePid,
                      unsigned short& pmtOffset);
    void WriteFakeServiceInfo();

    void InjectPcrFromPts(PidInfo& info);
    void PatchPcr(unsigned char* tsPacket, CTsHeader& header);
    void HandlePcrInstability(CPcr& pcrNew, long long pcrChange);
    void PatchPtsDts(unsigned char* tsPacket, PidInfo& pidInfo);

    static unsigned char GetPesHeader(unsigned char* tsPacket, CTsHeader& header, PidInfo& info);
    static void UpdatePesHeader(PidInfo& info);
    static long long EcPcrTime(long long newTs, long long prevTs);
    static void SetPcrBase(unsigned char* tsPacket, long long pcrBaseValue);

    CCriticalSection m_section;
    RecorderMode m_recorderMode;
    bool m_isRunning;
    bool m_isDropping;
    map<unsigned short, PidInfo*> m_pids;
    IChannelObserver* m_observer;
    clock_t m_videoAudioStartTimeStamp;

    unsigned long long m_tsPacketCount;
    unsigned long long m_discontinuityCount;
    unsigned long long m_droppedByteCount;

    wstringstream m_fileName;
    FileWriter* m_fileRecording;
    MultiFileWriter* m_fileTimeShifting;
    MultiFileWriterParams m_timeShiftingParameters;

    // PIDs.
    unsigned short m_nextFakePidVideo;
    unsigned short m_nextFakePidAudio;
    unsigned short m_nextFakePidSubtitles;
    unsigned short m_nextFakePidTeletext;
    unsigned short m_nextFakePidVbi;

    // PAT and PMT.
    unsigned char m_fakePat[TS_PACKET_LEN];
    unsigned char m_patContinuityCounter;
    unsigned char m_patVersion;
    unsigned char m_fakePmt[MAX_SECTION_LENGTH];
    unsigned char m_pmtContinuityCounter;
    unsigned char m_pmtVersion;
    CBasePmtParser m_pmtParser;
    unsigned char m_serviceInfoPacketCounter;

    // PCR.
    unsigned short m_originalPcrPid;
    unsigned short m_substitutePcrSourcePid;
    unsigned short m_fakePcrPid;
    bool m_waitingForPcr;
    bool m_generatePcrFromPts;                // Used in the [rare] case that the stream does not contain PCR.
    CPcr m_prevPcr;
    clock_t m_prevPcrReceiveTimeStamp;
    double m_averagePcrSpeed;                 // Time average between PCR samples.
    long long m_pcrCompensation;              // Compensation from PCR/PTS/DTS to fake PCR/PTS/DTS (33 bit offset with PCR resoluion).
    unsigned char m_pcrGapConfirmationCount;
    long long m_pcrFutureCompensation;

    // Write buffer.
    unsigned char* m_writeBuffer;
    unsigned long m_writeBufferSize;          // Byte count.
    unsigned long m_writeBufferPosition;      // Byte index in m_writeBuffer.
    unsigned char m_writeBufferThrottleStep;  // Index into WRITE_BUFFER_THROTTLE_STEP_PACKET_COUNTS.
    bool m_isWriteBufferThrottleFullyOpen;
    unsigned char m_writeBufferThrottleFullyOpenStep;

    static const unsigned char WRITE_BUFFER_THROTTLE_STEP_PACKET_COUNTS[];
};