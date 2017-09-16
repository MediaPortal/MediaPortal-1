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
#include <string>
#include <WinError.h>   // HRESULT
#include "..\..\shared\BasePmtParser.h"
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\FileWriter.h"
#include "..\..\shared\PacketSync.h"
#include "..\..\shared\Pcr.h"
#include "..\..\shared\TsHeader.h"
#include "IChannelObserver.h"
#include "MultiFileWriter.h"
#include "RecorderMode.h"

using namespace MediaPortal;
using namespace std;


#define MAX_TS_PACKET_QUEUE         4
#define MAX_PES_HEADER_BYTE_COUNT   19    // The maximum number of bytes required to hold the contents of a PES packet up to and including the last DTS byte.

class CDiskRecorder
{
  public:
    CDiskRecorder(RecorderMode mode);
    ~CDiskRecorder();

    HRESULT SetPmt(const unsigned char* pmt,
                    unsigned short pmtSize,
                    bool isDynamicPmtChange);
    void SetObserver(IChannelObserver* observer);
    HRESULT SetFileName(const wchar_t* fileName);
    HRESULT Start();
    void Pause(bool isPause);
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

    void OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);

  private:
    typedef struct
    {
      public:
        unsigned short OriginalPid;
        unsigned short FakePid;
        unsigned char StreamType;
        bool IsSeen;
        bool IsStartSeen;
        unsigned char PrevContinuityCounter;

        // Used to patch PTS and/or DTS.
        // 1. A PES packet header is built up in PesHeader from the TS packets
        // in TsPacketQueue. Refer to GetPesHeader().
        // 2. Any PTS and DTS in the PES packet header is patched based on PCR
        // information. Refer to PatchPtsDts().
        // 3. The PES packet header is written back into the TS packets in
        // TsPacketQueue. Refer to UpdatePesHeader().
        // 4. Finally, the TS packets in TsPacketQueue are written to file.
        // This process may seem overly convoluted. Unfortunately the
        // complication is necessary. It's necessary because PES packets may be
        // split over TS packets due to the presence of adaptation fields.
        unsigned char TsPacketQueueLength;
        unsigned char TsPacketQueue[MAX_TS_PACKET_QUEUE][TS_PACKET_LEN];
        unsigned char TsPacketPayloadStartQueue[MAX_TS_PACKET_QUEUE];
        unsigned char PesHeader[MAX_PES_HEADER_BYTE_COUNT];
        unsigned short PesHeaderByteCount;

        long long PrevPts;
        clock_t PrevPtsTimeStamp;
        long long PrevDts;
        clock_t PrevDtsTimeStamp;
    } PidInfo;

    bool CheckDiscontinuityFlag(const CTsHeader& header, const unsigned char* tsPacket);
    bool IsVideoOrAudioSeen(const CTsHeader& header);
    bool ConfirmAudioStreams(PidInfo* pidInfo);
    void CheckContinuityCounter(const CTsHeader& header,
                                PidInfo& pidInfo,
                                bool isExpectingJump);
    bool StartStream(const CTsHeader& header, PidInfo& pidInfo);

    void WriteLog(const wchar_t* fmt, ...);

    void WritePacket(const unsigned char* tsPacket);
    void WritePacketDirect(const unsigned char* tsPacket);

    void CreateFakePat();
    void CreateFakePmt();
    void AddPidToPmt(const BasePid* pid, unsigned short fakePid, unsigned short& pmtOffset);
    void WriteFakeServiceInfo();

    void UpdatePids(const CPidTable& pidTable);
    void ClearPids();
    bool AddPid(const BasePid* pid,
                const string& pidType,
                unsigned short& nextFakePid);

    bool ReadParameters();
    void WriteParameters();

    bool HandlePcr(const CTsHeader& header, unsigned short fakePid, unsigned char* tsPacket);
    void InjectPcrFromPts(const unsigned char* pesHeader);
    void PatchPcr(const CPcr& pcrNew, unsigned char* tsPacket);
    void HandlePcrInstability(const CPcr& pcrNew, long long pcrChange);
    void PatchPtsDts(PidInfo& pidInfo);

    static void GetPesHeader(const unsigned char* tsPacket,
                              const CTsHeader& header,
                              PidInfo& pidInfo,
                              bool& isMoreDataNeeded,
                              bool& isPtsOrDtsPresent);
    static void UpdatePesHeader(PidInfo& pidInfo);
    static long long PcrDifference(long long newTs, long long prevTs);
    static void SetPcrBase(unsigned char* tsPacket, long long pcrBaseValue);

    CCriticalSection m_section;
    RecorderMode m_recorderMode;
    bool m_isRunning;
    bool m_isDropping;
    map<unsigned short, PidInfo*> m_pids;
    IChannelObserver* m_observer;
    clock_t m_videoAudioStartTimeStamp;
    bool m_confirmAudioStreams;
    bool m_isAudioConfirmed;

    unsigned long long m_tsPacketCount;
    unsigned long long m_discontinuityCount;
    unsigned long long m_droppedByteCount;

    wstring m_fileName;
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
    unsigned short m_originalPcrPid;          // The PID in the input transport stream that contains PCR.
    unsigned short m_substitutePcrSourcePid;  // The audio PID in the input transport stream from which PCR is generated when PCR is generated from PTS.
    unsigned short m_fakePcrPid;              // The PID in the output transport stream that contains PCR.
    unsigned short m_fakePcrOriginalPid;      // The PID in the input transport stream associated with the fake PCR PID. This is usually but not always the same as m_originalPcrPid.
    bool m_waitingForPcr;
    bool m_generatePcrFromPts;                // Used in the [rare] case that the input transport stream does not contain PCR.
    CPcr m_prevPcr;
    clock_t m_prevPcrReceiveTimeStamp;
    double m_averagePcrIncrement;             // Average difference between PCR values. PCR frequency/timing is assumed to be regular.
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