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
#include "DiskRecorder.h"
#include "..\..\shared\DvbUtil.h"
#include "..\..\shared\Section.h"
#include "EnterCriticalSection.h"
using namespace Mediaportal;

#define THROTTLE_MAX_RADIO_PACKET_COUNT   10
#define THROTTLE_MAX_TV_PACKET_COUNT      172
#define SERVICE_INFO_INJECT_RATE          100   // the number of input packets between the injected SI (PAT, PMT etc.) packets

#define PID_NOT_SET                       0
#define PID_PAT                           0
#define PID_PMT                           0x20
#define PID_PCR                           0x21
#define PID_VIDEO_FIRST                   0x30
#define PID_AUDIO_FIRST                   0x40
#define PID_SUBTITLES_FIRST               0x50
#define PID_TELETEXT_FIRST                0x60
#define TABLE_ID_PAT                      0
#define TABLE_ID_PMT                      0x2
#define TRANSPORT_STREAM_ID               0x4
#define PROGRAM_NUMBER                    0x89

// TS header flags
#define DISCONTINUITY_FLAG_BIT            0x80  // bitmask for the DISCONTINUITY flag
#define ADAPTATION_ONLY                   0x20

// adaptation field flags
#define ADAPTATION_FIELD_FLAG_OFFSET      5     // byte offset from the start of a TS packet to the adaption field flags byte
#define PCR_FLAG_BIT                      0x10  // bitmask for the PCR flag
#define PCR_OFFSET                        6     // byte offset from the start of a TS packet to the start of the PCR field
#define PCR_LENGTH                        6     // the length of the PCR field in bytes

// PES header flags
#define PES_HEADER_ATTRIBUTES_OFFSET      6     // byte offset from the start of a PES packet to the attributes byte
#define PES_HEADER_FLAG_OFFSET            7     // byte offset from the start of a PES packet to the flags byte
#define PTS_FLAG_BIT                      0x80  // bitmask for the PTS flag
#define DTS_FLAG_BIT                      0x40  // bitmask for the DTS flag
#define PTS_OFFSET                        9     // byte offset from the start of a PES packet to the start of the PTS field
#define PTS_OR_DTS_LENGTH                 5     // the length of the PTS (or DTS) field in bytes
#define PTS_AND_DTS_LENGTH                10    // the length of the combined PTS/DTS field in bytes


//#define ERROR_FILE_TOO_LARGE 223  - already defined in winerror.h
#define RECORD_BUFFER_SIZE 256000




extern void LogDebug(const char* fmt, ...);
extern void LogDebug(const wchar_t *fmt, ...) ;

int GetPesHeader(byte* tsPacket, CTsHeader& header, PidInfo& info)
{
  // Update/populate the PES header in info.PesHeader.

  // Does this TS packet start a new PES packet?
  if (header.PayloadUnitStart)
  {
    if (info.TsPacketQueueLength != 0)
    {
      LogDebug("PID %d has non-empty TS packet queue at start of new PES packet, %d TS packet(s) will be lost", header.Pid, info.TsPacketQueueLength);
      info.TsPacketQueueLength = 0;
    }
    info.PesHeaderLength = 0;
  }
  else if (info.TsPacketQueueLength >= MAX_TS_PACKET_QUEUE) 
  {
    LogDebug("PID %d PES header starts after or is split over more than %d TS packets", header.Pid, MAX_TS_PACKET_QUEUE);
    return 0; // Flush the TS packet queue to disk.
  }

  // Copy this TS packet onto the end of the queue.
  memcpy(&info.TsPacketQueue[info.TsPacketQueueLength][0], tsPacket, TS_PACKET_LEN);
  if (header.HasPayload)
  {
    info.TsPacketPayloadStartQueue[info.TsPacketQueueLength++] = header.PayLoadStart;
  }
  else
  {
    info.TsPacketPayloadStartQueue[info.TsPacketQueueLength++] = 0xff;
    // If the packet doesn't have any payload then no point in continuing.
    return 2;
  }

  // Add any PES packet bytes to our PES header buffer. Be careful not to
  // overflow the buffer.
  int tsPacketPesByteCount = TS_PACKET_LEN - header.PayLoadStart;
  if (info.PesHeaderLength + tsPacketPesByteCount > MAX_PES_HEADER_BYTES)
  {
    tsPacketPesByteCount = MAX_PES_HEADER_BYTES - info.PesHeaderLength;
  }
  memcpy(&info.PesHeader[info.PesHeaderLength], &tsPacket[header.PayLoadStart], tsPacketPesByteCount);
  info.PesHeaderLength += tsPacketPesByteCount;

  // Do we have enough of the PES header to determine whether PTS and/or DTS
  // might be present?
  if (info.PesHeaderLength < PES_HEADER_ATTRIBUTES_OFFSET + 1)
  {
    return 2; // No, not yet.
  }
  // Might the header contain PTS and/or DTS?
  if ((info.PesHeader[PES_HEADER_ATTRIBUTES_OFFSET] & 0xc0) != 0x80)
  {
    return 0; // No, flush the packet queue to disk.
  }

  // Do we have enough of the PES header to determine whether PTS and/or DTS
  // are present?
  if (info.PesHeaderLength < PES_HEADER_FLAG_OFFSET + 1)
  {
    // No, not yet.
    return 2;
  }

  // Yes... so what do we have?
  switch (info.PesHeader[PES_HEADER_FLAG_OFFSET] & (PTS_FLAG_BIT | DTS_FLAG_BIT))
  {
    // PTS OR DTS
    case PTS_FLAG_BIT:
    case DTS_FLAG_BIT:
      if (info.PesHeaderLength >= PTS_OFFSET + PTS_OR_DTS_LENGTH)
      {
        return 1;  // Ready!
      }
      return 2; // We can tell that PTS or DTS are present, but we don't have enough PTS/DTS bytes yet.
    // PTS AND DTS
    case (PTS_FLAG_BIT | DTS_FLAG_BIT):
      if (info.PesHeaderLength >= PTS_OFFSET + PTS_AND_DTS_LENGTH)
      {
        return 1; // Ready!
      }
      return 2; // We can tell that PTS and DTS are present, but we don't have enough PTS/DTS bytes yet.
    // neither
    default:
      return 0; // Nothing interesting in this packet, flush the packet queue to disk.
  }
}

void UpdatePesHeader(PidInfo& info)
{
  // Overwrite the PTS and/or DTS in the queued packets with patched PTS and/or DTS.
  int i = 0;
  int pesUpdatedByteCount = 0;
  do
  {
    int tsPacketPayloadStart = info.TsPacketPayloadStartQueue[i];
    if (tsPacketPayloadStart != 0xff)
    {
      int tsPacketPesByteCount = TS_PACKET_LEN - tsPacketPayloadStart;
      if (tsPacketPesByteCount + pesUpdatedByteCount > info.PesHeaderLength)
      {
        tsPacketPesByteCount = info.PesHeaderLength - pesUpdatedByteCount;
      }
      if (tsPacketPesByteCount > 0)
      {
        memcpy(&info.TsPacketQueue[i][tsPacketPayloadStart], &info.PesHeader[pesUpdatedByteCount], tsPacketPesByteCount);
      }
      pesUpdatedByteCount += tsPacketPesByteCount;
    }
    i++;
  }
  while (i < info.TsPacketQueueLength);
}

__int64 EcPcrTime(__int64 newTs, __int64 prevTs)
{
  // Compute a signed difference between the new and previous timestamps.
  __int64 dt = newTs - prevTs;
  if (dt & 0x100000000)
  {
    dt |= 0xffffffff00000000LL; // negative
  }
  else
  {
    dt &= 0x00000000ffffffffLL; // positive
  }
  return dt;
}

void SetPcrBase(byte* tsPacket, __int64 pcrBaseValue)
{
  tsPacket[6] = (byte)((pcrBaseValue >> 25) & 0xff);
  tsPacket[7] = (byte)((pcrBaseValue >> 17) & 0xff);
  tsPacket[8] = (byte)((pcrBaseValue >> 9) & 0xff);
  tsPacket[9] = (byte)((pcrBaseValue >> 1) & 0xff);
  tsPacket[10] = (byte)(((pcrBaseValue & 0x1) << 7) + (tsPacket[10] & 0x7e));
}


//*******************************************************************
//* ctor
//*******************************************************************
CDiskRecorder::CDiskRecorder(RecordingMode mode) 
{
  m_recordingMode = mode;
  m_hFile = INVALID_HANDLE_VALUE;
  m_params.chunkSize = 268435424;
  m_params.maxFiles = 20;
  m_params.maxSize = 268435424;
  m_params.minFiles = 6;

  m_isRunning = false;
  m_pTimeShiftFile = NULL;
  m_pmtParser.Reset();

  if (m_recordingMode == TimeShift)
  {
    //  Set the buffer to the maximum it can throttle to
    m_iWriteBufferSize = THROTTLE_MAX_TV_PACKET_COUNT * TS_PACKET_LEN;
  }
  else
  {
    m_iWriteBufferSize = RECORD_BUFFER_SIZE;
  }

  m_pWriteBuffer = new byte[m_iWriteBufferSize];

  m_iWriteBufferPos = 0;
  m_bClearTsQueue = false;
  m_pVideoAudioObserver = NULL;

  // Populate the throttle.         (Total)
  m_iThrottleBufferSizes[0] = 2;    // 2
  m_iThrottleBufferSizes[1] = 3;    // 5
  m_iThrottleBufferSizes[2] = 5;    // 10
  m_iThrottleBufferSizes[3] = 5;    // 15
  m_iThrottleBufferSizes[4] = 5;    // 20
  m_iThrottleBufferSizes[5] = 5;    // 25
  m_iThrottleBufferSizes[6] = 5;    // 30
  m_iThrottleBufferSizes[7] = 10;   // 40
  m_iThrottleBufferSizes[8] = 10;   // 50
  m_iThrottleBufferSizes[9] = 10;   // 60
  m_iThrottleBufferSizes[10] = 10;  // 70
  m_iThrottleBufferSizes[11] = 10;  // 80
  m_iThrottleBufferSizes[12] = 20;  // 100
  m_iThrottleBufferSizes[13] = 20;  // 120
  m_iThrottleBufferSizes[14] = 20;  // 140
  m_iThrottleBufferSizes[15] = 32;  // 172  *sync with streamingserver
  m_iThrottleBufferSizes[16] = 40;  // 212
  m_iThrottleBufferSizes[17] = 50;  // 262
  m_iThrottleBufferSizes[18] = 82;  // 344  *sync with streamingserver
  m_iThrottleBufferSizes[19] = 172; // 516  *sync with streamingserver
}
//*******************************************************************
//* dtor
//*******************************************************************
CDiskRecorder::~CDiskRecorder(void)
{
  CEnterCriticalSection enter(m_section);
  if (m_hFile != INVALID_HANDLE_VALUE)
  {
    CloseHandle(m_hFile);
    m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file
  }
  delete[] m_pWriteBuffer;
  ClearPids();
}

void CDiskRecorder::ClearPids()
{
  map<int, PidInfo*>::iterator it = m_pids.begin();
  while (it != m_pids.end())
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
    it++;
  }
  m_pids.clear();
}

void CDiskRecorder::SetFileName(wchar_t* fileName)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    wcscpy(m_fileName, fileName);
    if (m_recordingMode == TimeShift)
    {
      wcscat(m_fileName, L".tsbuffer");
    }
    WriteLog(L"set filename:%s", m_fileName);
  }
  catch (...)
  {
    WriteLog(L"SetFilename exception");
  }
}

HRESULT CDiskRecorder::Start()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    // Check buffer is initialized
    if (m_pWriteBuffer == NULL)
    {
      WriteLog("Error, tried to start recording with uninitialized buffer");
      return E_POINTER;
    }

    if (wcslen(m_fileName) == 0)
    {
      return E_INVALIDARG;
    }
    ::DeleteFileW((LPCWSTR) m_fileName);
    m_iPart=2;
    if (m_recordingMode == TimeShift)
    {
      m_pTimeShiftFile = new MultiFileWriter(&m_params);
      HRESULT hr = m_pTimeShiftFile->OpenFile(m_fileName);
      if (FAILED(hr)) 
      {
        WriteLog(L"failed to open filename:%s %d", m_fileName, GetLastError());
        m_pTimeShiftFile->CloseFile();
        delete m_pTimeShiftFile;
        m_pTimeShiftFile = NULL;
        return hr;
      }
    }
    else
    {
      if (m_hFile!=INVALID_HANDLE_VALUE)
      {
        CloseHandle(m_hFile);
        m_hFile=INVALID_HANDLE_VALUE;
      }
      m_hFile = CreateFileW(m_fileName,            // The filename
                  (DWORD) GENERIC_WRITE,        // File access
                  (DWORD) FILE_SHARE_READ,      // Share access
                  NULL,                // Security
                  (DWORD) OPEN_ALWAYS,        // Open flags
//                  (DWORD) FILE_FLAG_RANDOM_ACCESS,
//                  (DWORD) FILE_FLAG_WRITE_THROUGH,  // More flags
                  (DWORD) 0,              // More flags
                  NULL);                // Template
      if (m_hFile == INVALID_HANDLE_VALUE)
      {
        LogDebug(L"Recorder:unable to create file:'%s' %d", m_fileName, GetLastError());
        return E_HANDLE;
      }
    }
    m_iWriteBufferPos = 0;
    WriteLog(L"Start '%s'", m_fileName);
    m_isRunning = true;
  }
  catch (...)
  {
    WriteLog("Start exception");
  }
  return S_OK;
}

void CDiskRecorder::Stop()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    WriteLog(L"Stop '%s'",m_fileName);
    m_isRunning = false;
    if (m_pTimeShiftFile!=NULL)
    {
      m_pTimeShiftFile->CloseFile();
      delete m_pTimeShiftFile;
      m_pTimeShiftFile=NULL;
    }
    if (m_hFile!=INVALID_HANDLE_VALUE)
    {
      if (m_iWriteBufferPos>0)
      {
        DWORD written = 0;
        WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
        m_iWriteBufferPos=0;
      }
      CloseHandle(m_hFile);
      m_hFile=INVALID_HANDLE_VALUE;
    }
  }
  catch (...)
  {
    WriteLog("Stop  exception");
  }
}

void CDiskRecorder::GetRecordingMode(int *mode) 
{
  *mode = (int)m_recordingMode;
}

HRESULT CDiskRecorder::SetPmt(byte* pmt, int pmtLength)
{
  CEnterCriticalSection enter(m_section);
  WriteLog("set PMT");
  CSection section;
  memcpy(section.Data, pmt, pmtLength);
  section.BufferPos = pmtLength;
  section.DecodeHeader();

  CPidTable& pidTable = m_pmtParser.GetPidInfo();
  int currentProgramNumber = pidTable.ProgramNumber;
  int currentPmtPid = pidTable.PmtPid;

  m_pmtParser.Reset();
  if (!m_pmtParser.DecodePmtSection(section))
  {
    WriteLog("invalid PMT, DecodePmtSection() failed");
    return E_FAIL;
  }

  pidTable = m_pmtParser.GetPidInfo();
  if (pidTable.VideoPids.size() > 0)
  {
    m_throttleMaxPacketCount = THROTTLE_MAX_TV_PACKET_COUNT;
  }
  else
  {
    m_throttleMaxPacketCount = THROTTLE_MAX_RADIO_PACKET_COUNT;
  }
  m_throttleAtMax = false;
  m_writeBufferThrottle = 0;

  if (m_isRunning)
  {
    if (pidTable.ProgramNumber == currentProgramNumber && pidTable.PmtPid == currentPmtPid)
    {
      WriteLog("dynamic PMT change, program number = %d, PMT PID = %d, PMT version = %d, PCR PID = %d", pidTable.ProgramNumber, pidTable.PmtPid, pidTable.PmtVersion, pidTable.PcrPid);
      if (m_originalPcrPid != pidTable.PcrPid)
      {
        // If the PCR PID changed, treat this like a channel change.
        m_originalPcrPid = pidTable.PcrPid;
        m_substitutePcrSourcePid = PID_NOT_SET;
        m_fakePcrPid = PID_PCR;
        m_generatePcrFromPts = (pidTable.PcrPid == 0x1fff);
        m_waitingForPcr = true;
      }
    }
    else
    {
      WriteLog("program change, program number = %d, PMT PID = %d, PMT version = %d, PCR PID = %d", pidTable.ProgramNumber, pidTable.PmtPid, pidTable.PmtVersion, pidTable.PcrPid);
      m_seenVideoOrAudio = -1;
      ClearPids();
      //m_tsPacketCount = 0;          want to keep stats running
      //m_discontinuityCount = 0;

      m_nextFakePidVideo = PID_VIDEO_FIRST;
      m_nextFakePidAudio = PID_AUDIO_FIRST;
      m_nextFakePidSubtitles = PID_SUBTITLES_FIRST;
      m_nextFakePidTeletext = PID_TELETEXT_FIRST;

      m_patVersion = (m_patVersion + 1) & 0xf;
      CreateFakePat();

      m_originalPcrPid = pidTable.PcrPid;
      m_substitutePcrSourcePid = PID_NOT_SET;
      m_fakePcrPid = PID_PCR;
      m_generatePcrFromPts = (m_originalPcrPid == 0x1fff);
      m_waitingForPcr = true;
    }
    m_pmtVersion = (m_pmtVersion + 1) & 0xf;
  }
  else
  {
    WriteLog("program selection, program number = %d, PMT PID = %d, PMT version = %d, PCR PID = %d", pidTable.ProgramNumber, pidTable.PmtPid, pidTable.PmtVersion, pidTable.PcrPid);
    m_seenVideoOrAudio = -1;
    ClearPids();
    m_tsPacketCount = 0;
    m_discontinuityCount = 0;

    m_nextFakePidVideo = PID_VIDEO_FIRST;
    m_nextFakePidAudio = PID_AUDIO_FIRST;
    m_nextFakePidSubtitles = PID_SUBTITLES_FIRST;
    m_nextFakePidTeletext = PID_TELETEXT_FIRST;

    m_patContinuityCounter = -1;
    m_patVersion = 0;
    CreateFakePat();
    m_pmtContinuityCounter = -1;
    m_pmtVersion = 0;

    m_originalPcrPid = pidTable.PcrPid;
    m_substitutePcrSourcePid = PID_NOT_SET;
    m_fakePcrPid = PID_PCR;
    m_generatePcrFromPts = (m_originalPcrPid == 0x1fff);
    m_waitingForPcr = true;
    m_pcrCompensation = 0;
  }

  //TODO check
  m_bClearTsQueue = true;

  CreateFakePmt(pidTable);
  return S_OK;
}

void CDiskRecorder::SetVideoAudioObserver(IVideoAudioObserver* callBack)
{
  if (callBack)
  {
    WriteLog("SetVideoAudioObserver observer ok");
    m_pVideoAudioObserver = callBack;
  }
  else
  {
    WriteLog("SetVideoAudioObserver observer was null");  
    return;
  }
}

void CDiskRecorder::SetMinTsFiles(WORD minFiles)
{
  m_params.minFiles = (long)minFiles;
}

void CDiskRecorder::SetMaxTsFiles(WORD maxFiles)
{
  m_params.maxFiles = (long)maxFiles;
}

void CDiskRecorder::SetMaxTsFileSize(__int64 maxSize)
{
  m_params.maxSize = maxSize;
}

void CDiskRecorder::SetChunkReserve(__int64 chunkSize)
{
  m_params.chunkSize = chunkSize;
}

void CDiskRecorder::GetBufferSize(__int64* bufferSize)
{
  if (m_pTimeShiftFile != NULL)
  {
    m_pTimeShiftFile->GetFileSize(bufferSize);
  }
  else
  {
    *bufferSize = 0;
  }
}

void CDiskRecorder::GetTimeShiftPosition(__int64* position, long* bufferId)
{
  if (m_pTimeShiftFile != NULL)
  {
    m_pTimeShiftFile->GetPosition(position);
    *bufferId = m_pTimeShiftFile->getCurrentFileId();
  }
  else
  {
    *position = 0;
    *bufferId = 0;
  }
}

void CDiskRecorder::GetDiscontinuityCount(int* count)
{
  if (m_isRunning)
  {
    *count = m_discontinuityCount;
  }
  else
  {
    *count = 0;
  }
}

void CDiskRecorder::GetProcessedPacketCount(int* count)
{
  if (m_isRunning)
  {
    *count = m_tsPacketCount;
  }
  else
  {
    *count = 0;
  }
}

void CDiskRecorder::OnTsPacket(byte* tsPacket)
{
  if (!m_isRunning)
  {
    return;
  }

  CTsHeader header(tsPacket);
  if (header.Pid == 0x1fff)
  {
    return;
  }
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, PidInfo*>::iterator it = m_pids.find(header.Pid);
    if (it != m_pids.end())
    {
      if (header.TransportError)
      {
        WriteLog("PID %d transport flag set, signal quality problem?", header.Pid);
        m_discontinuityCount++;
        return;
      }
      bool expectingPcrOrContinuityCounterJump = false;
      if (header.AdaptionFieldLength > 0 && (tsPacket[ADAPTATION_FIELD_FLAG_OFFSET] & DISCONTINUITY_FLAG_BIT) != 0)
      {
        expectingPcrOrContinuityCounterJump = true;
        if (header.Pid == m_originalPcrPid)
        {
          WriteLog("PID %d discontinuity flag set, expecting PCR and/or continuity counter jump", header.Pid);
        }
        else
        {
          WriteLog("PID %d discontinuity flag set, expecting continuity counter jump", header.Pid);
        }
      }

      // First requirement is that we see unencrypted video or audio. Note any
      // packet that reaches us is not encrypted.
      PidInfo& info = *(it->second);
      if (m_seenVideoOrAudio == -1)
      {
        if (header.HasPayload && ((info.FakePid & 0xff0) == PID_VIDEO_FIRST || (info.FakePid & 0xff0) == PID_AUDIO_FIRST))
        {
          WriteLog("start of video and/or audio detected, wait for PCR");
          m_seenVideoOrAudio = GetTickCount();
        }
        else
        {
          return;
        }
      }

      // Second and last requirement is that we see PCR.
      // ISO/IEC 13818-1 says the maximum gap between PCR timestamps is 100 ms.
      // If we've seen video/audio but then haven't seen PCR after 100 ms
      // assume that we're going to have to generate PCR from PTS.
      if (m_waitingForPcr && !m_generatePcrFromPts && GetTickCount() - m_seenVideoOrAudio > 100)
      {
        WriteLog("timeout waiting for PCR, generate PCR from PTS");
        m_generatePcrFromPts = true;
        m_fakePcrPid = PID_PCR;
      }

      // Check the continuity counter is as expected. Do this here because
      // below this clause we may block packets leading to false positive
      // discontinuity detections.
      if (info.PrevContinuityCounter != 0xff)
      {
        byte expectedContinuityCounter = info.PrevContinuityCounter;
        if (header.HasPayload)
        {
          expectedContinuityCounter = (info.PrevContinuityCounter + 1) & 0x0f;
        }
        if (header.ContinuityCounter != expectedContinuityCounter)
        {
          if (expectingPcrOrContinuityCounterJump)
          {
            WriteLog("PID %d signaled continuity jump, value = %d, previous = %d", header.Pid, header.ContinuityCounter, info.PrevContinuityCounter);
          }
          else
          {
            m_discontinuityCount++;
            WriteLog("PID %d unsignaled discontinuity, value = %d, previous = %d, expected = %d, count = %d, signal quality, descrambling, or HDD load problem?", header.Pid, header.ContinuityCounter, info.PrevContinuityCounter, expectedContinuityCounter, m_discontinuityCount);
          }
        }
      }
      info.PrevContinuityCounter = header.ContinuityCounter;

      // Should we take a closer look at this packet?
      if (!info.SeenStart)
      {
        // When waiting for PCR, don't let the packet through unless it might
        // contain the PCR (or PTS) that we're waiting for.
        // Otherwise, don't let the packet through unless it contains the start
        // of a payload. We make an exception for the PCR stream because
        // stopping those packets might create discontinuities (there is no
        // gurantee that the first PCR was in a TS packet containing the start
        // of a PES packet).
        if (
          (!m_waitingForPcr && !header.PayloadUnitStart && (
            (!m_generatePcrFromPts && header.Pid != m_originalPcrPid) ||
            (m_generatePcrFromPts && header.Pid != m_substitutePcrSourcePid)
          )) ||
          (m_waitingForPcr && (
            (!m_generatePcrFromPts && header.Pid != m_originalPcrPid) ||
            (m_generatePcrFromPts && (info.FakePid & 0xff0) != PID_AUDIO_FIRST)
          ))
        )
        {
          return;
        }

        if (!m_waitingForPcr && header.PayloadUnitStart)
        {
          info.SeenStart = true;
          CPidTable& pidTable = m_pmtParser.GetPidInfo();
          bool foundPid = false;
          for (vector<VideoPid*>::iterator it2 = pidTable.VideoPids.begin(); it2 != pidTable.VideoPids.end(); it2++)
          {
            if ((*it2)->Pid == info.OriginalPid)
            {
              foundPid = true;
              WriteLog("PID %d start of video detected", info.OriginalPid);
              if (m_pVideoAudioObserver)
              {
                m_pVideoAudioObserver->OnNotify(Video);
              }
              break;
            }
          }
          if (!foundPid)
          {
            for (vector<AudioPid*>::iterator it2 = pidTable.AudioPids.begin(); it2 != pidTable.AudioPids.end(); it2++)
            {
              if ((*it2)->Pid == info.OriginalPid)
              {
                WriteLog("PID %d start of audio detected", info.OriginalPid);
                if (m_pVideoAudioObserver)
                {
                  m_pVideoAudioObserver->OnNotify(Audio);
                }
                break;
              }
            }
          }
        }
      }

      //-----------------------------------------------------------------------
      // MAIN PCR HANDLING
      // Normal PCR case: PCR timestamps carried in a main stream, and not
      // generating PCR from PTS.
      byte localTsPacket[TS_PACKET_LEN];
      memcpy(localTsPacket, tsPacket, TS_PACKET_LEN);
      if (!m_generatePcrFromPts && header.Pid == m_originalPcrPid)
      {
        // Overwrite the PCR timestamps.
        PatchPcr(localTsPacket, header);
      }
      // Corner PCR case: undesirable PCR timestamps.
      else if (info.FakePid == m_fakePcrPid && header.HasAdaptionField && header.AdaptionFieldLength >= 1 + PCR_LENGTH && (localTsPacket[ADAPTATION_FIELD_FLAG_OFFSET] & PCR_FLAG_BIT) != 0)
      {
        // If we get to here then we've found a PCR timestamp in a packet that
        // is not expected to contain PCR timestamps. That isn't a problem...
        // unless this packet comes from the stream that we are *inserting* PCR
        // timestamps into. In that case a "rogue" PCR would create a random
        // PCR discontinuity and so break TsReader's ability to skip,
        // fast-forward and rewind. The solution is simple: remove this PCR!

        // Clear the PCR flag.
        localTsPacket[ADAPTATION_FIELD_FLAG_OFFSET] &= (~PCR_FLAG_BIT);
        // Overwrite the PCR with the rest of the adaption field. The -1 is for the adaption field flag byte.
        memcpy(&localTsPacket[PCR_OFFSET], &tsPacket[PCR_OFFSET + PCR_LENGTH], header.AdaptionFieldLength - PCR_LENGTH - 1);
        // The end of the adaption field is now stuffing.
        memset(&localTsPacket[PCR_OFFSET + header.AdaptionFieldLength - PCR_LENGTH - 1], 0xff, PCR_LENGTH);
      }
      //-----------------------------------------------------------------------

      // Should we process this packet?
      if (m_waitingForPcr && !m_generatePcrFromPts) 
      {
        return; // No, still waiting for PCR.
      }

      // Yes. Overwrite the PID.
      localTsPacket[1] = (localTsPacket[1] & 0xe0) + ((info.FakePid >> 8) & 0x1f);
      localTsPacket[2] = info.FakePid & 0xff;

      if (!header.PayloadUnitStart && info.TsPacketQueueLength == 0)
      {
        WritePacket(localTsPacket);
        return;
      }

      int result = GetPesHeader(localTsPacket, header, info);
      if (result == 2)
      {
        return; // Need more bytes to complete the PES header.
      }

      if (result == 1)
      {
        // Found PTS and maybe DTS.
        if (m_generatePcrFromPts)
        {
          InjectPcrFromPts(info);
        }
        if (!m_waitingForPcr)
        {
          PatchPtsDts(info.PesHeader, info);
          UpdatePesHeader(info);
        }
      }
      int i = 0;
      do
      {
        WritePacket(&info.TsPacketQueue[i++][0]);
      }
      while (--info.TsPacketQueueLength);
      return;
    }

    //-------------------------------------------------------------------------
    // ALTERNATE PCR HANDLING
    // Handling for PCR timestamps when they are *not* carried in one of the
    // main streams.
    if (header.Pid == m_originalPcrPid && !m_generatePcrFromPts && m_seenVideoOrAudio && header.HasAdaptionField && header.AdaptionFieldLength >= 1 + PCR_LENGTH && (tsPacket[ADAPTATION_FIELD_FLAG_OFFSET] & PCR_FLAG_BIT) != 0)
    {
      // Patch the PCR.
      byte localTsPacket[TS_PACKET_LEN];
      memcpy(localTsPacket, tsPacket, TS_PACKET_LEN);
      PatchPcr(localTsPacket, header);

      if (!m_waitingForPcr)
      {
        // Overwrite the PID.
        localTsPacket[1] = (m_fakePcrPid >> 8) & 0x1f;
        localTsPacket[2] = m_fakePcrPid & 0xff;

        // Hard-code the continuity counter and ensure any payload is ignored.
        // Adaptation field control == adaptation field only, no payload.
        localTsPacket[3] = ADAPTATION_ONLY;
        localTsPacket[4] = TS_PACKET_LEN - 5;   // -4 for header, -1 for adaptation field length byte

        WritePacket(localTsPacket);
      }
    }
    //-------------------------------------------------------------------------
  }
  catch (...)
  {
    WriteLog("Exception in OnTsPacket()");
  }
}

void CDiskRecorder::InjectPcrFromPts(PidInfo& info)
{
  // We only use PTS from audio PIDs because it is guaranteed to be sequential.
  if (m_substitutePcrSourcePid == PID_NOT_SET && (info.FakePid & 0xff0) == PID_AUDIO_FIRST)
  {
    WriteLog("PID %d found first PTS", info.OriginalPid);
    m_substitutePcrSourcePid = info.OriginalPid;

    // Update the PMT - PCR PID and CRC.
    m_fakePcrPid = PID_PCR;
    m_fakePmt[9] = ((m_fakePcrPid >> 8) & 0x1f) | 0xe0;
    m_fakePmt[10] = m_fakePcrPid & 0xff;

    int sectionLength = ((m_fakePmt[2] & 0xf) << 8) + m_fakePmt[3];
    DWORD crc = crc32((char*)&m_fakePmt[1], sectionLength - 1);   // + 3 for the table ID and section length bytes - 4 for the CRC bytes
    m_fakePmt[sectionLength++] = (crc >> 24) & 0xff;
    m_fakePmt[sectionLength++] = (crc >> 16) & 0xff;
    m_fakePmt[sectionLength++] = (crc >> 8) & 0xff;
    m_fakePmt[sectionLength++] = crc & 0xff;

    m_serviceInfoPacketCounter = SERVICE_INFO_INJECT_RATE;
  }
  if (info.OriginalPid == m_substitutePcrSourcePid)
  {
    CPcr pts;
    CPcr dts;
    if (!CPcr::DecodeFromPesHeader(info.PesHeader, 0, pts, dts))
    {
      WriteLog("failed to decode PTS for PCR generation");
    }
    else
    {
      __int64 adjustedPts = (pts.PcrReferenceBase - 27000) & MAX_PCR_BASE;  // offset PCR from PTS by 300 ms to give time for demuxing and decoding
      byte pcrPacket[TS_PACKET_LEN];
      pcrPacket[0] = TS_PACKET_SYNC;
      pcrPacket[1] = (m_fakePcrPid >> 8) & 0x1f;
      pcrPacket[2] = m_fakePcrPid & 0xff;
      pcrPacket[3] = ADAPTATION_ONLY;
      pcrPacket[4] = TS_PACKET_LEN - 5;   // -4 for header, -1 for adaptation field length byte
      pcrPacket[5] = PCR_FLAG_BIT;
      memset(&pcrPacket[12], 0xff, TS_PACKET_LEN - 12);   // stuffing

      // Set the PCR extension. PTS won't give us this value.
      pcrPacket[10] = 0x7e;
      pcrPacket[11] = 0;
      SetPcrBase(pcrPacket, adjustedPts);

      CTsHeader header(pcrPacket);
      PatchPcr(pcrPacket, header);
      if (!m_waitingForPcr)
      {
        Write(pcrPacket, TS_PACKET_LEN);
      }
    }
  }
}

void CDiskRecorder::WritePacket(byte* tsPacket)
{
  if (m_waitingForPcr)
  {
    return;
  }
  if (m_serviceInfoPacketCounter >= SERVICE_INFO_INJECT_RATE)
  {
    WriteFakeServiceInfo();
    m_serviceInfoPacketCounter = 0;
  }
  Write(tsPacket, TS_PACKET_LEN);
  m_tsPacketCount++;
  m_serviceInfoPacketCounter++;
}

void CDiskRecorder::Write(byte* buffer, int len)
{
  CEnterCriticalSection enter(m_section);
  if (m_recordingMode==TimeShift)
    WriteToTimeshiftFile(buffer,len);
  else
    WriteToRecording(buffer,len);
}

void CDiskRecorder::WriteToRecording(byte* buffer, int len)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (!m_isRunning || buffer == NULL)
    {
      return;
    }
    if (m_bClearTsQueue) 
    {
      try
      {
        WriteLog("clear TS packet queue"); 
        m_bClearTsQueue = false;
        ZeroMemory(m_pWriteBuffer, m_iWriteBufferSize);
        m_iWriteBufferPos = 0;
    
        // Reset the write buffer throttle
        LogDebug("CDiskRecorder::WriteToRecording() - Reset write buffer throttle");
        m_writeBufferThrottle = 0;
        m_throttleAtMax = false;
      }
      catch (...)
      {
        WriteLog("Write exception - 1");
      }
      return;
    }
    if (len <= 0)
    {
      return;
    }
    if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)
    {
      try
      {
        if (m_iWriteBufferPos > 0)
        {
          if (m_hFile != INVALID_HANDLE_VALUE)
          {
            DWORD written = 0;
            if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
            {
              //On fat16/fat32 we can only create files of max. 2gb/4gb
              if (ERROR_FILE_TOO_LARGE == GetLastError())
              {
                LogDebug(L"Recorder:Maximum filesize reached for file:'%s' %d", m_fileName);
                // close the file...
                CloseHandle(m_hFile);
                m_hFile = INVALID_HANDLE_VALUE;

                // create a new file
                wchar_t ext[MAX_PATH];
                wchar_t fileName[MAX_PATH];
                wchar_t part[100];
                int len = wcslen(m_fileName) - 1;
                int pos = len-1;
                while (pos > 0)
                {
                  if (m_fileName[pos] == L'.')
                  {
                    break;
                  }
                  pos--;
                }
                wcscpy(ext, &m_fileName[pos]);
                wcsncpy(fileName, m_fileName, pos);
                fileName[pos] = 0;
                swprintf_s(part, L"_p%d", m_iPart);
                wchar_t newFileName[MAX_PATH];
                swprintf_s(newFileName, L"%s%s%s", fileName, part, ext);
                LogDebug(L"Recorder:Create new  file:'%s' %d", newFileName);
                m_hFile = CreateFileW(newFileName,              // The filename
                            (DWORD) GENERIC_WRITE,              // File access
                            (DWORD) FILE_SHARE_READ,            // Share access
                            NULL,                               // Security
                            (DWORD) OPEN_ALWAYS,                // Open flags
                            (DWORD) 0,                          // More flags
                            NULL);                              // Template
                if (m_hFile == INVALID_HANDLE_VALUE)
                {
                  LogDebug(L"Recorder:unable to create file:'%s' %d", newFileName, GetLastError());
                  m_iWriteBufferPos = 0;
                  return ;
                }
                m_iPart++;
                WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
              }//of if (ERROR_FILE_TOO_LARGE == GetLastError())
              else
              {
                LogDebug(L"Recorder:unable to write file:'%s' %d %d %x", m_fileName, GetLastError(), m_iWriteBufferPos, m_hFile);
              }
            }//of if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
          }//if (m_hFile!=INVALID_HANDLE_VALUE)
        }//if (m_iWriteBufferPos>0)
        m_iWriteBufferPos=0;
      }
      catch (...)
      {
        LogDebug("Recorder:Write exception");
      }
    }// of if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)

    if ((m_iWriteBufferPos + len) < RECORD_BUFFER_SIZE && len > 0)
    {
      memcpy(&m_pWriteBuffer[m_iWriteBufferPos], buffer, len);
      m_iWriteBufferPos+=len;
    }
  }
  catch (...)
  {
    WriteLog("Exception in writetorecording");
  }
}

void CDiskRecorder::WriteToTimeshiftFile(byte* buffer, int len)
{
  if (!m_isRunning || buffer == NULL || len != TS_PACKET_LEN || m_pWriteBuffer == NULL)
  {
    return;
  }
  if (m_bClearTsQueue) 
  {
    try
    {
      WriteLog("clear TS packet queue"); 
      m_bClearTsQueue = false;
      ZeroMemory(m_pWriteBuffer, m_iWriteBufferSize);
      m_iWriteBufferPos = 0;
    
      // Reset the write buffer throttle
      LogDebug("CDiskRecorder::WriteToTimeshiftFile() - Reset write buffer throttle");
      m_writeBufferThrottle = 0;
      m_throttleAtMax = false;
    }
    catch (...)
    {
      WriteLog("Write exception - 1");
    }
  }
  CEnterCriticalSection enter(m_section);

  try
  {
    // Copy first TS packet from the queue to the I/O buffer
    if (m_iWriteBufferPos >= 0 && m_iWriteBufferPos + TS_PACKET_LEN <= m_iWriteBufferSize)
    {
      memcpy(&m_pWriteBuffer[m_iWriteBufferPos], buffer, TS_PACKET_LEN);
      m_iWriteBufferPos += TS_PACKET_LEN;
    }
    else
    {
      WriteLog("Write m_iWriteBufferPos overflow!");
      m_iWriteBufferPos = 0;
    }
  }
  catch (...)
  {
    WriteLog("Write exception - 3");
    return;
  }

  if (m_writeBufferThrottle < 0)
  {
    m_writeBufferThrottle = 0;
  }
  else if (m_writeBufferThrottle >= NUMBER_THROTTLE_BUFFER_SIZES)
  {
    m_writeBufferThrottle = NUMBER_THROTTLE_BUFFER_SIZES - 1;
  }

  int currentThrottlePackets = m_iThrottleBufferSizes[m_writeBufferThrottle];
  int currentThrottleBufferSize = currentThrottlePackets * TS_PACKET_LEN;

  if (m_iWriteBufferPos >= currentThrottleBufferSize)
  {
    //  Throttle up if we are not at maximum
    if (currentThrottlePackets < m_throttleMaxPacketCount)
    {  
      if (m_writeBufferThrottle < (NUMBER_THROTTLE_BUFFER_SIZES - 1))
      {
        LogDebug("CDiskRecorder::Flush() - Throttle to %d bytes", m_iWriteBufferPos);
      }

      m_writeBufferThrottle++;
    }
    else if (currentThrottlePackets == m_throttleMaxPacketCount && !m_throttleAtMax)
    {
      m_throttleAtMax = true;
      LogDebug("CDiskRecorder::Flush() - Throttle to %d bytes (max)", m_iWriteBufferPos);
    }

    Flush();
  }
}

void CDiskRecorder::WriteLog(const char* fmt,...)
{
  char logbuffer[2000]; 
  va_list ap;
  va_start(ap,fmt);

  int tmp;
  va_start(ap,fmt);
  tmp=vsprintf(logbuffer, fmt, ap);
  va_end(ap); 

  if (m_recordingMode == TimeShift)
    LogDebug("Recorder: TIMESHIFT %s",logbuffer);
  else
    LogDebug("Recorder: RECORD    %s",logbuffer);
}

void CDiskRecorder::WriteLog(const wchar_t* fmt,...)
{
  wchar_t logbuffer[2000]; 
  va_list ap;
  va_start(ap,fmt);

  int tmp;
  va_start(ap,fmt);
  tmp=vswprintf_s(logbuffer, fmt, ap);
  va_end(ap); 

  if (m_recordingMode == TimeShift)
    LogDebug(L"Recorder: TIMESHIFT %s",logbuffer);
  else
    LogDebug(L"Recorder: RECORD    %s",logbuffer);
}

void CDiskRecorder::Flush()
{
  try
  {
    if (m_iWriteBufferPos > 0)
    {
      if (m_pTimeShiftFile != NULL)
      {
        m_pTimeShiftFile->Write(m_pWriteBuffer, m_iWriteBufferPos);
        m_iWriteBufferPos=0;
      }
    }
  }
  catch (...)
  {
    WriteLog("Flush exception");
  }
}

void CDiskRecorder::CreateFakePat()
{
  m_fakePat[0] = 0;   // pointer
  m_fakePat[1] = TABLE_ID_PAT;
  m_fakePat[2] = 0xb0;
  m_fakePat[3] = 13;  // section length
  m_fakePat[4] = (TRANSPORT_STREAM_ID >> 8) & 0xff;
  m_fakePat[5] = TRANSPORT_STREAM_ID & 0xff;
  m_fakePat[6] = ((m_patVersion & 0x1f) << 1) | 0xc1;
  m_fakePat[7] = 0; // section number - standard for PAT
  m_fakePat[8] = 0; // last section number - standard for PAT
  m_fakePat[9] = (PROGRAM_NUMBER >> 8) & 0xff;
  m_fakePat[10] = PROGRAM_NUMBER & 0xff;
  m_fakePat[11] = ((PID_PMT >> 8) & 0x1f) | 0xe0;
  m_fakePat[12] = PID_PMT & 0xff;

  // The CRC covers everything we've written so far, except the one pointer byte.
  DWORD crc = crc32((char*)&m_fakePat[1], 12);
  m_fakePat[13] = (crc >> 24) & 0xff;
  m_fakePat[14] = (crc >> 16) & 0xff;
  m_fakePat[15] = (crc >> 8) & 0xff;
  m_fakePat[16] = crc & 0xff;

  m_serviceInfoPacketCounter = SERVICE_INFO_INJECT_RATE;
}

void CDiskRecorder::CreateFakePmt(CPidTable& pidTable)
{
  m_fakePmt[0] = 0; // pointer
  m_fakePmt[1] = TABLE_ID_PMT;
  m_fakePmt[2] = 0; // section syntax indicator and section length - we fill these once we know what the section length should be
  m_fakePmt[3] = 0;
  m_fakePmt[4] = (PROGRAM_NUMBER >> 8) & 0xff;
  m_fakePmt[5] = PROGRAM_NUMBER & 0xff;
  m_fakePmt[6] = ((m_pmtVersion & 0x1f) << 1) | 0xc1;
  m_fakePmt[7] = 0; // section number - standard for PMT
  m_fakePmt[8] = 0; // last section number - standard for PMT
  m_fakePmt[9] = 0; // PCR PID - we fill this once we know what the fake PID is
  m_fakePmt[10] = 0;
  m_fakePmt[11] = 0xf0; // program info length - we won't include program descriptors
  m_fakePmt[12] = 0;
  int fakePmtOffset = 13;

  // Mark all PIDs as not present so we can identify the PIDs that really
  // aren't present after the loop.
  map<int, PidInfo*>::iterator infoIt = m_pids.begin();
  while (infoIt != m_pids.end())
  {
    (infoIt->second)->IsStillPresent = false;
    infoIt++;
  }

  // Write the elementary stream section of the PMT.
  vector<VideoPid*>::iterator vPidIt = pidTable.VideoPids.begin();
  while (vPidIt != pidTable.VideoPids.end())
  {
    AddPidToPmt(*vPidIt, "video", m_nextFakePidVideo, fakePmtOffset);
    vPidIt++;
  }
  vector<AudioPid*>::iterator aPidIt = pidTable.AudioPids.begin();
  while (aPidIt != pidTable.AudioPids.end())
  {
    AddPidToPmt(*aPidIt, "audio", m_nextFakePidAudio, fakePmtOffset);
    aPidIt++;
  }
  vector<SubtitlePid*>::iterator sPidIt = pidTable.SubtitlePids.begin();
  while (sPidIt != pidTable.SubtitlePids.end())
  {
    AddPidToPmt(*sPidIt, "subtitles", m_nextFakePidSubtitles, fakePmtOffset);
    sPidIt++;
  }
  vector<TeletextPid*>::iterator tPidIt = pidTable.TeletextPids.begin();
  while (tPidIt != pidTable.TeletextPids.end())
  {
    AddPidToPmt(*tPidIt, "teletext", m_nextFakePidTeletext, fakePmtOffset);
    tPidIt++;
  }

  // All PIDs that are still marked as not present should be removed.
  PidInfo* info = NULL;
  infoIt = m_pids.begin();
  while (infoIt != m_pids.end())
  {
    info = infoIt->second;
    if (!info->IsStillPresent)
    {
      WriteLog("remove stream, PID = %d, fake PID = %d", info->OriginalPid, info->FakePid);

      // We lost our PTS source for PCR conversion.
      if (m_generatePcrFromPts && info->OriginalPid == m_substitutePcrSourcePid)
      {
        WriteLog("unset PTS source PID");
        m_substitutePcrSourcePid = PID_NOT_SET;
      }
      m_pids.erase(infoIt++);
    }
    else
    {
      infoIt++;
    }
  }

  // Fill in the section length. At this point, fakePmtOffset is the length of what we've created.
  // The section length should be that value, minus the pointer byte, table ID and section length
  // bytes, plus the four CRC bytes that we haven't yet written.
  m_fakePmt[2] = ((fakePmtOffset >> 8) & 0xf) | 0xb0;
  m_fakePmt[3] = fakePmtOffset & 0xff;

  // Fill in the PCR PID.
  m_fakePmt[9] = ((m_fakePcrPid >> 8) & 0x1f) | 0xe0;
  m_fakePmt[10] = m_fakePcrPid & 0xff;

  // The CRC covers everything we've written so far, except the one pointer byte.
  DWORD crc = crc32((char*)&m_fakePmt[1], fakePmtOffset - 1);
  m_fakePmt[fakePmtOffset++] = (crc >> 24) & 0xff;
  m_fakePmt[fakePmtOffset++] = (crc >> 16) & 0xff;
  m_fakePmt[fakePmtOffset++] = (crc >> 8) & 0xff;
  m_fakePmt[fakePmtOffset++] = crc & 0xff;

  m_serviceInfoPacketCounter = SERVICE_INFO_INJECT_RATE;
}

void CDiskRecorder::AddPidToPmt(BasePid* pid, const string& pidType, int& nextFakePid, int& pmtOffset)
{
  PidInfo* info = NULL;
  map<int, PidInfo*>::iterator infoIt = m_pids.find(pid->Pid);
  if (infoIt == m_pids.end())
  {
    info = new PidInfo();
    if (info == NULL)
    {
      WriteLog("failed to allocate PidInfo in AddPidToPmt()");
      return;
    }

    info->OriginalPid = pid->Pid;
    info->FakePid = nextFakePid++;
    info->SeenStart = false;
    info->PrevContinuityCounter = 0xff;
    info->TsPacketQueueLength = 0;
    info->PesHeaderLength = 0;
    info->PrevPts = -1;
    info->PrevPtsTimeStamp = 0;
    info->PrevDts = -1;
    info->PrevDtsTimeStamp = 0;
    m_pids[pid->Pid] = info;
    WriteLog("add %s stream, PID = %d, fake PID = %d, stream type = 0x%x, logical stream type = 0x%x", pidType.c_str(), pid->Pid, info->FakePid, pid->StreamType, pid->LogicalStreamType);
  }
  else
  {
    info = infoIt->second;
    WriteLog("update %s stream, PID = %d, fake PID = %d, stream type = 0x%x, logical stream type = 0x%x", pidType.c_str(), pid->Pid, info->FakePid, pid->StreamType, pid->LogicalStreamType);
  }

  info->IsStillPresent = true;
  m_fakePmt[pmtOffset++] = pid->StreamType;
  m_fakePmt[pmtOffset++] = ((info->FakePid >> 8) & 0x1f) | 0xe0;
  m_fakePmt[pmtOffset++] = info->FakePid & 0xff;
  m_fakePmt[pmtOffset++] = ((pid->DescriptorsLength >> 8) & 0xf) | 0xf0;
  m_fakePmt[pmtOffset++] = pid->DescriptorsLength & 0xff;
  if (pid->DescriptorsLength != 0)
  {
    memcpy(&m_fakePmt[pmtOffset], pid->Descriptors, pid->DescriptorsLength);
    pmtOffset += pid->DescriptorsLength;
  }

  if (pid->Pid == m_originalPcrPid)
  {
    WriteLog("  set fake PCR PID");
    m_fakePcrPid = info->FakePid;
  }
}

void CDiskRecorder::WriteFakeServiceInfo()
{
  // PAT
  byte packet[TS_PACKET_LEN];
  packet[0] = TS_PACKET_SYNC;
  packet[1] = ((PID_PAT >> 8) & 0x1f) | 0x40;
  packet[2] = PID_PAT & 0xff;
  m_patContinuityCounter++;
  if (m_patContinuityCounter > 0xf)
  {
    m_patContinuityCounter = 0;
  }
  packet[3] = 0x10 | m_patContinuityCounter;
  int fakePatLength = 17;
  memcpy(&packet[4], &m_fakePat[0], fakePatLength);
  memset(&packet[4 + fakePatLength], 0xff, TS_PACKET_LEN - 4 - fakePatLength);
  Write(packet, TS_PACKET_LEN);

  // PMT
  int fakePmtLength = ((m_fakePmt[2] & 0xf) << 8) + m_fakePmt[3] + 4; // includes pointer byte, table ID, section length bytes, PMT and CRC
  int pointer = 0;
  packet[1] = ((PID_PMT >> 8) & 0x1f) | 0x40;
  packet[2] = PID_PMT & 0xff;
  while (fakePmtLength > 0)
  {
    m_pmtContinuityCounter++;
    if (m_pmtContinuityCounter > 0xf)
    {
      m_pmtContinuityCounter = 0;
    }
    packet[3] = 0x10 | m_pmtContinuityCounter;
    if (fakePmtLength >= 184)
    {
      memcpy(&packet[4], &m_fakePmt[pointer], 184);
      fakePmtLength -= 184;
    }
    else
    {
      memcpy(&packet[4], &m_fakePmt[pointer], fakePmtLength);
      memset(&packet[4 + fakePmtLength], 0xff, TS_PACKET_LEN - 4 - fakePmtLength);
      fakePmtLength = 0;
    }
    Write(packet, TS_PACKET_LEN);
    pointer += TS_PACKET_LEN - 4;

    packet[1] &= 0x1f;  // unset payload unit start indicator
  }
}

void CDiskRecorder::PatchPcr(byte* tsPacket, CTsHeader& header)
{
  bool wr = false;
  bool wjump = false;
  bool verbose = false;

  if (header.PayLoadOnly())
  {
    return;
  }
  CAdaptionField adaptationField;
  adaptationField.Decode(header, tsPacket);
  if (!adaptationField.PcrFlag)
  {
    return;
  }
  CPcr pcrNew = adaptationField.Pcr;
  DWORD timeStamp = GetTickCount();
  
  if (m_waitingForPcr)
  {
    m_waitingForPcr = false;
    m_serviceInfoPacketCounter = SERVICE_INFO_INJECT_RATE;
    m_pcrGapConfirmations = 0;
    if (m_pcrCompensation == 0)
    {
      // First estimate of PCR speed is based on ISO/IEC 13818-1:
      // They have a resolution of one part in 27 000 000 per second, and occur at intervals up to
      // 100 ms in Transport Streams, or up to 700 ms in Program Streams.
      // ... and TR 101 290:
      // In DVB a repetition period of not more than 40 ms is recommended.
      m_averagePcrSpeed = 3600;  // 40 ms
      // PCR compensation is set in such a way as to ensure PCR starts at zero.
      m_pcrCompensation = (0 - pcrNew.PcrReferenceBase) & MAX_PCR_BASE;
    }
    else
    {
      // Adjust the PCR compensation in such a way that we get a smooth transition without jumps.
      UINT64 predictedNextPcrOldStream = (m_prevPcr.PcrReferenceBase + (UINT64)m_averagePcrSpeed) & MAX_PCR_BASE;
      m_pcrCompensation = (m_pcrCompensation + predictedNextPcrOldStream - pcrNew.PcrReferenceBase) & MAX_PCR_BASE;
    }
    m_prevPcr = pcrNew;

    CPcr nextPcr;
    nextPcr.PcrReferenceBase = MAX_PCR_BASE - pcrNew.PcrReferenceBase;
    nextPcr.PcrReferenceExtension = MAX_PCR_EXTENSION - pcrNew.PcrReferenceExtension;
    WriteLog("PCR found, value = %I64d, compensation = %I64d, next broadcast program clock reference rollover in %s", pcrNew.PcrReferenceBase, m_pcrCompensation, nextPcr.ToString());
  }
  else
  {
    if (pcrNew.PcrReferenceBase < 0x10000 && m_prevPcr.PcrReferenceBase > 0x1ffff0000)
    {
      WriteLog("normal broadcast program clock reference rollover passed");
    }

    // Calculate the difference between the new and previous PCR value. The unit is 90 kHz ticks.
    __int64 dt = EcPcrTime(pcrNew.PcrReferenceBase, m_prevPcr.PcrReferenceBase);

    // Any negative PCR change is unexpected.
    if (dt < 0)
    {
      HandlePcrInstability(pcrNew, dt);
    }
    else
    {
      // Is this postive PCR change unexpectedly large? 8x (800%) normal is an arbitrary threshold.
      if (dt > 8 * m_averagePcrSpeed)
      {
        // Yes. Is it also coherent/consistent with the local system clock change?
        __int64 dt2 = (timeStamp - m_prevPcrReceiveTimeStamp) * 90;   // # 90 kHz local system clock ticks since previous PCR seen
        __int64 dt3 = dt2 - dt;
        if (dt3 < 15000 && dt3 > -15000)  // +/- 166 ms
        {
          // Yes. Assume signal was lost or the stream stopped for awhile and do nothing.
          WriteLog("PCR jump %I64d detected with coherent local system clock jump %I64d, signal quality or HDD load problem?", dt, dt2);
          m_prevPcr = pcrNew;
        }
        else
        {
          // No, so this really is unexpected.
          HandlePcrInstability(pcrNew, dt);
        }
      }
      else
      {
        // Normal situation - stable/regular/timely PCR.
        // We assume that PCR is delivered at a regular interval which is approximated by
        // m_averagePcrSpeed. Improve the PCR speed estimate by comparison with the current speed (ie. the
        // time since the previous PCR was seen). Use a 10% adjustment factor.
        m_averagePcrSpeed += ((float)dt - m_averagePcrSpeed) * 0.1;
        if (m_pcrGapConfirmations > 0)
        {
          WriteLog("PCR restabilised after %d confirmation(s), speed = %I64d", m_pcrGapConfirmations, (__int64)m_averagePcrSpeed);
        }
        m_pcrGapConfirmations = 0;
        m_prevPcr = pcrNew;
      }
    }
  }

  __int64 adjustedPcr = (m_prevPcr.PcrReferenceBase + m_pcrCompensation) & MAX_PCR_BASE;
  SetPcrBase(tsPacket, adjustedPcr);

  m_prevPcrReceiveTimeStamp = timeStamp;
}

void CDiskRecorder::HandlePcrInstability(CPcr& pcrNew, __int64 pcrChange)
{
  // Estimate the expected PCR value. This estimate should be slower/less than reality to avoid
  // falling into the negative PCR change case and perpetual instability.
  m_prevPcr.PcrReferenceBase += (__int64)(m_averagePcrSpeed / 2);
  m_prevPcr.PcrReferenceBase &= MAX_PCR_BASE;

  __int64 futureComp = m_pcrCompensation - pcrChange + (__int64)m_averagePcrSpeed;
  if (m_pcrGapConfirmations > 0)
  {
    // We previously detected instability. Has that instability settled yet?
    __int64 ecFutureComp = EcPcrTime(m_pcrFutureCompensation, futureComp);
    m_pcrFutureCompensation = futureComp;
    if (ecFutureComp < -8 * m_averagePcrSpeed || ecFutureComp > 8 * m_averagePcrSpeed)
    {
      // No, looks like the instability is continuing.
      WriteLog("ongoing PCR instability, change = %I64d, confirmation count %d", pcrChange, m_pcrGapConfirmations);
      m_pcrGapConfirmations = 1;
    }
    else
    {
      // Yes, it is settling. Shall we go back to normal operation?
      m_pcrGapConfirmations++;
      if (m_pcrGapConfirmations >= 3)
      {
        // Yes. We're back to normal!
        m_pcrGapConfirmations = 0;
        m_prevPcr = pcrNew;
        m_pcrCompensation = m_pcrFutureCompensation;
        WriteLog("PCR stabilised, change = %I64d, compensation = %I64d", pcrChange, m_pcrCompensation);
      }
    }
  }
  else
  {
    // Start of instability.
    m_pcrGapConfirmations = 1;
    m_pcrFutureCompensation = futureComp;
    WriteLog("PCR instability detected, change = %I64d, speed = %I64d, compensation = %I64d", pcrChange, (__int64)m_averagePcrSpeed, m_pcrCompensation);
  }
}

void CDiskRecorder::PatchPtsDts(byte* pesHeader, PidInfo& pidInfo)
{
  DWORD timeStamp = GetTickCount();

  // Sanity check PES header bytes.
  if (pesHeader[0] != 0 || pesHeader[1] != 0 || pesHeader[2] != 1)
  {
    return;
  }

  CPcr pts;
  CPcr dts;
  if (!CPcr::DecodeFromPesHeader(pesHeader, 0, pts, dts))
  {
    WriteLog("no PTS or DTS to decode, should not be detected here!");
    return;
  }

  if (pts.IsValid)
  {
    // Check for PTS jumps.
    /*if (pidInfo.PrevPts != -1)
    {
      __int64 dt = EcPcrTime(pts.PcrReferenceBase, pidInfo.PrevPts);  // # 90 kHz broadcast clock ticks since previous PTS
      __int64 dt2 = (timeStamp - pidInfo.PrevPtsTimeStamp) * 90;      // # 90 kHz local system clock ticks since previous PTS seen
      __int64 dt3 = dt2 - dt;                                         // = PTS drift estimation
      if ((dt3 < -30000LL || dt3 > 30000LL) && ((pidInfo.FakePid & 0xff0) == PID_VIDEO_FIRST || (pidInfo.FakePid & 0xff0) == PID_AUDIO_FIRST))
      {
        WriteLog("PTS jump %I64d detected on PID %d stream ID 0x%x, corresponding local system clock jump = %I64d, clock jump difference = %I64d, prev PTS = %I64d, new PTS = %I64d", dt, pidInfo.OriginalPid, pesHeader[3], dt2, dt3, pidInfo.PrevPts, pts.PcrReferenceBase);
      }
    }*/

    // Patch the PTS with the PCR compensation. This seems to be enough to avoid freezes & crashes.
    __int64 ptsPatched = (pts.PcrReferenceBase + m_pcrCompensation) & MAX_PCR_BASE;
    pesHeader[13] = (byte)((ptsPatched & 0x7f) << 1) + 1;
    ptsPatched >>= 7;
    pesHeader[12] = (byte)(ptsPatched & 0xff);
    ptsPatched >>= 8;
    pesHeader[11] = (byte)((ptsPatched & 0x7f) << 1) + 1;
    ptsPatched >>= 7;
    pesHeader[10] = (byte)(ptsPatched & 0xff);
    ptsPatched >>= 8;
    pesHeader[9] = (byte)((ptsPatched & 7) << 1) + 0x21;

    pidInfo.PrevPts = pts.PcrReferenceBase;
    pidInfo.PrevPtsTimeStamp = timeStamp;

    if (dts.IsValid)
    {
      /*if (pidInfo.PrevDts != -1)
      {
        __int64 dt = EcPcrTime(dts.PcrReferenceBase, pidInfo.PrevDts);  // # 90 kHz broadcast clock ticks since previous DTS
        __int64 dt2 = (timeStamp - pidInfo.PrevPtsTimeStamp) * 90;      // # 90 kHz local system clock ticks since previous DTS seen
        __int64 dt3 = dt2 - dt;                                         // = DTS drift estimation
        if ((dt3 < -30000LL || dt3 > 30000LL) && ((pidInfo.FakePid & 0xff0) == PID_VIDEO_FIRST || (pidInfo.FakePid & 0xff0) == PID_AUDIO_FIRST))
        {
          WriteLog("DTS jump %I64d detected on PID %d stream ID 0x%x, corresponding local system clock jump = %I64d, clock jump difference = %I64d, prev DTS = %I64d, new DTS = %I64d", dt, pidInfo.OriginalPid, pesHeader[3], dt2, dt3, pidInfo.PrevDts, dts.PcrReferenceBase);
        }
      }*/

      __int64 dtsPatched = (dts.PcrReferenceBase + m_pcrCompensation) & MAX_PCR_BASE;
      pesHeader[18] = (byte)((dtsPatched & 0x7f) << 1) + 1;
      dtsPatched >>= 7;
      pesHeader[17] = (byte)(dtsPatched & 0xff);
      dtsPatched >>= 8;
      pesHeader[16] = (byte)((dtsPatched & 0x7f) << 1) + 1;
      dtsPatched >>= 7;
      pesHeader[15] = (byte)((dtsPatched & 0xff));
      dtsPatched >>= 8;
      pesHeader[14] = (byte)((dtsPatched & 7) << 1) + 0x31;

      pidInfo.PrevDts = dts.PcrReferenceBase;
      pidInfo.PrevDtsTimeStamp = timeStamp;
    }
  }
}