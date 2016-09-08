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
#include <algorithm>    // min()
#include <cstddef>      // NULL
#include <cstring>      // memcpy(), memset()
#include <vector>
#include "..\..\shared\AdaptionField.h"
#include "..\..\shared\DvbUtil.h"
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\TimeUtils.h"
#include "FileReader.h"
#include "FileUtils.h"


#define WRITE_BUFFER_THROTTLE_FULLY_OPEN_STEP_RADIO   7
#define WRITE_BUFFER_THROTTLE_FULLY_OPEN_STEP_TV      19

#define SERVICE_INFO_INJECT_RATE          100     // the number of input packets between the injected SI (PAT, PMT etc.) packets

#define PID_NOT_SET                       0
#define PID_PAT                           0
#define PID_PMT                           0x20
#define PID_PCR                           0x21
#define PID_VIDEO_FIRST                   0x30
#define PID_AUDIO_FIRST                   0x40
#define PID_SUBTITLES_FIRST               0x50
#define PID_TELETEXT_FIRST                0x60
#define PID_VBI_FIRST                     0x70
#define TABLE_ID_PAT                      0
#define TABLE_ID_PMT                      0x2
#define TRANSPORT_STREAM_ID               0x4
#define PROGRAM_NUMBER                    0x89

#define CONTINUITY_COUNTER_NOT_SET        0xff

// TS header flags
#define DISCONTINUITY_FLAG_BIT            0x80    // bitmask for the DISCONTINUITY flag
#define ADAPTATION_ONLY                   0x20

// adaptation field flags
#define ADAPTATION_FIELD_FLAG_OFFSET      5       // byte offset from the start of a TS packet to the adaption field flags byte
#define PCR_FLAG_BIT                      0x10    // bitmask for the PCR flag
#define PCR_OFFSET                        6       // byte offset from the start of a TS packet to the start of the PCR field
#define PCR_LENGTH                        6       // the length of the PCR field in bytes

// PES header flags
#define PES_HEADER_ATTRIBUTES_OFFSET      6       // byte offset from the start of a PES packet to the attributes byte
#define PES_HEADER_FLAG_OFFSET            7       // byte offset from the start of a PES packet to the flags byte
#define PTS_FLAG_BIT                      0x80    // bitmask for the PTS flag
#define DTS_FLAG_BIT                      0x40    // bitmask for the DTS flag
#define PTS_OFFSET                        9       // byte offset from the start of a PES packet to the start of the PTS field
#define PTS_OR_DTS_LENGTH                 5       // the length of the PTS (or DTS) field in bytes
#define PTS_AND_DTS_LENGTH                10      // the length of the combined PTS/DTS field in bytes

#define RECORD_BUFFER_SIZE                255680  // needs to be divisible by TS_PACKET_LEN
#define PARAM_BUFFER_SIZE                 69      // (sizeof(unsigned char) * 5) + (sizeof(unsigned long) * 2) + (sizeof(unsigned long long) * 5) + (sizeof(long long) * 2)


extern void LogDebug(const wchar_t* fmt, ...);

const unsigned char CDiskRecorder::WRITE_BUFFER_THROTTLE_STEP_PACKET_COUNTS[] =
{
  2,    // 2 (running total)
  3,    // 5
  5,    // 10
  5,    // 15
  5,    // 20
  5,    // 25
  5,    // 30
  10,   // 40
  10,   // 50
  10,   // 60
  10,   // 70
  10,   // 80
  20,   // 100
  20,   // 120
  20,   // 140
  32,   // 172  *sync with streaming server
  40,   // 212
  50,   // 262
  82,   // 344  *sync with streaming server
  172   // 516  *sync with streaming server
};

CDiskRecorder::CDiskRecorder(RecorderMode mode) 
{
  m_recorderMode = mode;
  m_isRunning = false;
  m_isDropping = false;

  m_videoAudioStartTimeStamp = -1;
  m_tsPacketCount = 0;
  m_discontinuityCount = 0;
  m_droppedByteCount = 0;

  m_nextFakePidVideo = PID_VIDEO_FIRST;
  m_nextFakePidAudio = PID_AUDIO_FIRST;
  m_nextFakePidSubtitles = PID_SUBTITLES_FIRST;
  m_nextFakePidTeletext = PID_TELETEXT_FIRST;
  m_nextFakePidVbi = PID_VBI_FIRST;

  m_patContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
  m_patVersion = 0;
  m_pmtContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
  m_pmtVersion = 0;
  m_serviceInfoPacketCounter = 0;

  m_originalPcrPid = 0;
  m_substitutePcrSourcePid = PID_NOT_SET;
  m_fakePcrPid = PID_PCR;
  m_waitingForPcr = true;
  m_generatePcrFromPts = false;
  m_prevPcrReceiveTimeStamp = 0;
  m_averagePcrIncrement = 0;
  m_pcrCompensation = 0;
  m_pcrGapConfirmationCount = 0;
  m_pcrFutureCompensation = 0;

  m_writeBuffer = NULL;
  m_writeBufferSize = 0;
  m_writeBufferPosition = 0;
  m_writeBufferThrottleStep = 0;
  m_isWriteBufferThrottleFullyOpen = false;

  m_fileRecording = NULL;
  m_fileTimeShifting = NULL;
  m_timeShiftingParameters.FileCountMinimum = 6;
  m_timeShiftingParameters.FileCountMaximum = 20;
  m_timeShiftingParameters.MaximumFileSize = 255999976;   // ~256 MB, divisible by TS_PACKET_LEN so that buffer files start and end on packet boundaries
  m_timeShiftingParameters.ReservationChunkSize = m_timeShiftingParameters.MaximumFileSize;

  m_pmtParser.Reset();
  m_observer = NULL;
}

CDiskRecorder::~CDiskRecorder()
{
  CEnterCriticalSection lock(m_section);
  Stop();
  if (m_writeBuffer != NULL)
  {
    delete[] m_writeBuffer;
    m_writeBuffer = NULL;
  }
  ClearPids();
}

HRESULT CDiskRecorder::SetPmt(unsigned char* pmt,
                              unsigned short pmtSize,
                              bool isDynamicPmtChange)
{
  try
  {
    if (pmt == NULL)
    {
      WriteLog(L"invalid PMT, not provided");
      return E_FAIL;
    }

    CSection section;
    section.AppendData(pmt, min(sizeof(section.Data), pmtSize));
    CEnterCriticalSection lock(m_section);
    m_pmtParser.Reset();
    if (!section.IsComplete() || !m_pmtParser.DecodePmtSection(section))
    {
      WriteLog(L"invalid PMT, incomplete or invalid section");
      return E_FAIL;
    }

    CPidTable& pidTable = m_pmtParser.GetPidInfo();
    if (pidTable.VideoPids.size() > 0)
    {
      m_writeBufferThrottleFullyOpenStep = WRITE_BUFFER_THROTTLE_FULLY_OPEN_STEP_TV;
    }
    else
    {
      m_writeBufferThrottleFullyOpenStep = WRITE_BUFFER_THROTTLE_FULLY_OPEN_STEP_RADIO;
    }
    m_isWriteBufferThrottleFullyOpen = false;
    m_writeBufferThrottleStep = 0;

    if (m_isRunning)
    {
      if (isDynamicPmtChange)
      {
        WriteLog(L"dynamic PMT change, program number = %hu, version = %hhu, PCR PID = %hu",
                  pidTable.ProgramNumber, pidTable.PmtVersion,
                  pidTable.PcrPid);
        if (m_originalPcrPid != pidTable.PcrPid)
        {
          // If the PCR PID changed, treat this like a channel change.
          m_videoAudioStartTimeStamp = clock();

          m_originalPcrPid = pidTable.PcrPid;
          m_substitutePcrSourcePid = PID_NOT_SET;
          m_fakePcrPid = PID_PCR;
          m_generatePcrFromPts = pidTable.PcrPid == 0x1fff;
          m_waitingForPcr = true;
        }
      }
      else
      {
        WriteLog(L"program change, program number = %hu, version = %hhu, PCR PID = %hu",
                  pidTable.ProgramNumber, pidTable.PmtVersion,
                  pidTable.PcrPid);
        m_videoAudioStartTimeStamp = -1;
        ClearPids();
        // Keep statistics.
        //m_tsPacketCount = 0;
        //m_discontinuityCount = 0;
        //m_droppedByteCount = 0;

        m_nextFakePidVideo = PID_VIDEO_FIRST;
        m_nextFakePidAudio = PID_AUDIO_FIRST;
        m_nextFakePidSubtitles = PID_SUBTITLES_FIRST;
        m_nextFakePidTeletext = PID_TELETEXT_FIRST;
        m_nextFakePidVbi = PID_VBI_FIRST;

        m_patVersion = (m_patVersion + 1) & 0xf;
        CreateFakePat();

        m_originalPcrPid = pidTable.PcrPid;
        m_substitutePcrSourcePid = PID_NOT_SET;
        m_fakePcrPid = PID_PCR;
        m_generatePcrFromPts = m_originalPcrPid == 0x1fff;
        m_waitingForPcr = true;
      }
      m_pmtVersion = (m_pmtVersion + 1) & 0xf;
    }
    else
    {
      WriteLog(L"program selection, program number = %hu, version = %hhu, PCR PID = %hu",
                pidTable.ProgramNumber, pidTable.PmtVersion, pidTable.PcrPid);
      m_videoAudioStartTimeStamp = -1;
      ClearPids();
      m_tsPacketCount = 0;
      m_discontinuityCount = 0;
      m_droppedByteCount = 0;

      m_nextFakePidVideo = PID_VIDEO_FIRST;
      m_nextFakePidAudio = PID_AUDIO_FIRST;
      m_nextFakePidSubtitles = PID_SUBTITLES_FIRST;
      m_nextFakePidTeletext = PID_TELETEXT_FIRST;
      m_nextFakePidVbi = PID_VBI_FIRST;

      m_patContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
      m_patVersion = 0;
      CreateFakePat();
      m_pmtContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
      m_pmtVersion = 0;

      m_originalPcrPid = pidTable.PcrPid;
      m_substitutePcrSourcePid = PID_NOT_SET;
      m_fakePcrPid = PID_PCR;
      m_generatePcrFromPts = m_originalPcrPid == 0x1fff;
      m_waitingForPcr = true;
      m_pcrCompensation = 0;
    }

    //TODO should we really clear the write buffer in all cases?
    m_writeBufferPosition = 0;
    m_writeBufferThrottleStep = 0;
    m_isWriteBufferThrottleFullyOpen = false;

    CreateFakePmt(pidTable);
    return S_OK;
  }
  catch (...)
  {
    WriteLog(L"unhandled exception in SetPmt()");
  }
  return E_FAIL;
}

void CDiskRecorder::SetObserver(IChannelObserver* observer)
{
  CEnterCriticalSection lock(m_section);
  m_observer = observer;
}

HRESULT CDiskRecorder::SetFileName(wchar_t* fileName)
{
  try
  {
    CEnterCriticalSection lock(m_section);
    if (fileName == NULL)
    {
      WriteLog(L"set file name, file name is NULL");
      return E_INVALIDARG;
    }

    m_fileName.str(fileName);
    return S_OK;
  }
  catch (...)
  {
    WriteLog(L"unhandled exception in SetFileName()");
  }
  return E_FAIL;
}

HRESULT CDiskRecorder::Start()
{
  try
  {
    CEnterCriticalSection lock(m_section);
    wstring fileName = m_fileName.str();
    if (fileName.length() == 0)
    {
      WriteLog(L"failed to start, file name not set");
      return E_INVALIDARG;
    }

    // Check that the write buffer is initialised.
    if (m_writeBuffer == NULL)
    {
      if (m_recorderMode == TimeShift)
      {
        // Use the maximum buffer size.
        m_writeBufferSize = WRITE_BUFFER_THROTTLE_STEP_PACKET_COUNTS[WRITE_BUFFER_THROTTLE_FULLY_OPEN_STEP_TV] * TS_PACKET_LEN;
      }
      else
      {
        m_writeBufferSize = RECORD_BUFFER_SIZE;
      }

      m_writeBuffer = new unsigned char[m_writeBufferSize];
      if (m_writeBuffer == NULL)
      {
        WriteLog(L"failed to allocate %lu bytes for the write buffer",
                  m_writeBufferSize);
        return E_OUTOFMEMORY;
      }
    }

    if (m_recorderMode == TimeShift)
    {
      m_fileTimeShifting = new MultiFileWriter();
      if (m_fileTimeShifting == NULL)
      {
        WriteLog(L"failed to allocate multi file writer");
        return E_OUTOFMEMORY;
      }

      bool resume = ReadParameters();
      m_fileTimeShifting->SetConfiguration(m_timeShiftingParameters);
      HRESULT hr = m_fileTimeShifting->OpenFile(fileName.c_str(), resume);
      if (FAILED(hr)) 
      {
        WriteLog(L"failed to open file, hr = 0x%x, file = %s", hr, fileName);
        delete m_fileTimeShifting;
        m_fileTimeShifting = NULL;
        return hr;
      }

      if (resume)
      {
        m_fakePat[6] = ((m_patVersion & 0x1f) << 1) | (m_fakePat[6] & 0xc1);
        m_fakePmt[6] = ((m_pmtVersion & 0x1f) << 1) | (m_fakePmt[6] & 0xc1);
      }
      else
      {
        m_patContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
        m_patVersion = 0;
        m_pmtContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
        m_pmtVersion = 0;
        m_pcrCompensation = 0;
      }
    }
    else
    {
      m_fileRecording = new FileWriter();
      if (m_fileRecording == NULL)
      {
        WriteLog(L"failed to allocate file writer");
        return E_OUTOFMEMORY;
      }

      HRESULT hr = m_fileRecording->OpenFile(fileName.c_str());
      if (FAILED(hr))
      {
        WriteLog(L"failed to open file, hr = 0x%x, file = %s", hr, fileName);
        delete m_fileRecording;
        m_fileRecording = NULL;
        return hr;
      }
    }

    m_writeBufferPosition = 0;
    m_isRunning = true;
    m_isDropping = false;
    return S_OK;
  }
  catch (...)
  {
    WriteLog(L"unhandled exception in Start()");
  }
  return E_FAIL;
}

void CDiskRecorder::GetStreamQualityCounters(unsigned long long& countTsPackets,
                                              unsigned long long& countDiscontinuities,
                                              unsigned long long& countDroppedBytes)
{
  if (m_isRunning)
  {
    countTsPackets = m_tsPacketCount;
    countDiscontinuities = m_discontinuityCount;
    countDroppedBytes = m_droppedByteCount;
    return;
  }

  countTsPackets = 0;
  countDiscontinuities = 0;
  countDroppedBytes = 0;
}

void CDiskRecorder::Stop()
{
  CEnterCriticalSection lock(m_section);
  try
  {
    m_isRunning = false;

    if (m_fileTimeShifting != NULL)
    {
      m_fileTimeShifting->CloseFile();
      delete m_fileTimeShifting;
      m_fileTimeShifting = NULL;
      WriteParameters();
    }

    if (m_fileRecording != NULL)
    {
      // Flush buffer to disk.
      if (m_writeBufferPosition > 0)
      {
        HRESULT hr = m_fileRecording->Write(m_writeBuffer, m_writeBufferPosition);
        if (FAILED(hr))
        {
          WriteLog(L"failed to flush to file, hr = 0x%x, byte count = %lu, file = %s",
                    hr, m_writeBufferPosition, m_fileName.str());
        }
        m_writeBufferPosition = 0;
      }

      m_fileRecording->CloseFile();
      delete m_fileRecording;
      m_fileRecording = NULL;
    }
  }
  catch (...)
  {
    WriteLog(L"unhandled exception in Stop()");
  }
}

HRESULT CDiskRecorder::SetTimeShiftingParameters(unsigned long fileCountMinimum,
                                                  unsigned long fileCountMaximum,
                                                  unsigned long long fileSizeBytes)
{
  CEnterCriticalSection lock(m_section);

  HRESULT hr = S_OK;
  if (fileCountMinimum > 1 && fileCountMinimum <= fileCountMaximum)
  {
    m_timeShiftingParameters.FileCountMinimum = fileCountMinimum;
    m_timeShiftingParameters.FileCountMaximum = fileCountMaximum;
  }
  else
  {
    WriteLog(L"invalid file counts, minimum = %lu, maximum = %lu, using minimum = %lu, maximum = %lu",
              fileCountMinimum, fileCountMaximum,
              m_timeShiftingParameters.FileCountMinimum,
              m_timeShiftingParameters.FileCountMaximum);
    hr = E_INVALIDARG;
  }

  if (fileSizeBytes > 50000000) // 50 MB
  {
    // Ensure buffer files always start and finish on a TS packet boundary.
    long long fileSizeAdjustment = fileSizeBytes % TS_PACKET_LEN;
    m_timeShiftingParameters.MaximumFileSize = fileSizeBytes - fileSizeAdjustment;
    if (fileSizeAdjustment != 0)
    {
      WriteLog(L"file size adjusted, adjustment = % bytes, file size = %llu bytes",
                fileSizeAdjustment, m_timeShiftingParameters.MaximumFileSize);
    }
    m_timeShiftingParameters.ReservationChunkSize = m_timeShiftingParameters.MaximumFileSize;
  }
  else
  {
    WriteLog(L"invalid file size, size = %llu bytes, using size = %llu bytes",
              fileSizeBytes, m_timeShiftingParameters.MaximumFileSize);
    hr = E_INVALIDARG;
  }

  if (m_fileTimeShifting != NULL)
  {
    m_fileTimeShifting->SetConfiguration(m_timeShiftingParameters);
  }
  return hr;
}

void CDiskRecorder::GetTimeShiftingParameters(unsigned long& fileCountMinimum,
                                              unsigned long& fileCountMaximum,
                                              unsigned long long& fileSizeBytes)
{
  fileCountMinimum = m_timeShiftingParameters.FileCountMinimum;
  fileCountMaximum = m_timeShiftingParameters.FileCountMaximum;
  fileSizeBytes = m_timeShiftingParameters.MaximumFileSize;
}

void CDiskRecorder::GetTimeShiftingFilePosition(unsigned long long& position,
                                                unsigned long& bufferId)
{
  CEnterCriticalSection lock(m_section);
  if (m_fileTimeShifting != NULL)
  {
    m_fileTimeShifting->GetCurrentFilePosition(bufferId, position);
    return;
  }

  position = 0;
  bufferId = 0;
}

void CDiskRecorder::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  try
  {
    if (!m_isRunning || tsPacket == NULL)
    {
      return;
    }

    if (header.Pid == 0x1fff)
    {
      return;
    }

    CEnterCriticalSection lock(m_section);
    map<unsigned short, PidInfo*>::const_iterator it = m_pids.find(header.Pid);
    if (it != m_pids.end())
    {
      if (header.TransportError)
      {
        WriteLog(L"PID %hu transport error flag set, signal quality problem?",
                  header.Pid);
        m_discontinuityCount++;
        return;
      }
      bool expectingPcrOrContinuityCounterJump = false;
      if (
        header.AdaptionFieldLength > 0 &&
        (tsPacket[ADAPTATION_FIELD_FLAG_OFFSET] & DISCONTINUITY_FLAG_BIT) != 0
      )
      {
        expectingPcrOrContinuityCounterJump = true;
        if (header.Pid == m_originalPcrPid)
        {
          WriteLog(L"PID %hu discontinuity flag set, expecting PCR and/or continuity counter jump",
                    header.Pid);
        }
        else
        {
          WriteLog(L"PID %hu discontinuity flag set, expecting continuity counter jump",
                    header.Pid);
        }
      }

      // First requirement is that we see unencrypted video or audio. Note any
      // packet that reaches us is not encrypted.
      PidInfo& info = *(it->second);
      if (m_videoAudioStartTimeStamp == -1)
      {
        if (
          header.HasPayload &&
          (
            (info.FakePid & 0xff0) == PID_VIDEO_FIRST ||
            (info.FakePid & 0xff0) == PID_AUDIO_FIRST
          )
        )
        {
          WriteLog(L"start of video and/or audio detected, wait for PCR");
          m_videoAudioStartTimeStamp = clock();
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
      if (
        m_waitingForPcr &&
        !m_generatePcrFromPts &&
        CTimeUtils::ElapsedMillis(m_videoAudioStartTimeStamp) > 100
      )
      {
        WriteLog(L"PCR wait time limit reached, generate PCR from PTS");
        m_generatePcrFromPts = true;
        m_fakePcrPid = PID_PCR;
      }

      // Check the continuity counter is as expected. Do this here because
      // below this clause we may block packets leading to false positive
      // discontinuity detections.
      if (info.PrevContinuityCounter != CONTINUITY_COUNTER_NOT_SET)
      {
        unsigned char expectedContinuityCounter = info.PrevContinuityCounter;
        if (header.HasPayload)
        {
          expectedContinuityCounter = (info.PrevContinuityCounter + 1) & 0x0f;
        }
        if (header.ContinuityCounter != expectedContinuityCounter)
        {
          if (expectingPcrOrContinuityCounterJump)
          {
            WriteLog(L"PID %hu signaled discontinuity, value = %hhu, previous = %hhu",
                      header.Pid, header.ContinuityCounter,
                      info.PrevContinuityCounter);
          }
          else
          {
            m_discontinuityCount++;
            WriteLog(L"PID %hu unsignaled discontinuity, value = %hhu, previous = %hhu, expected = %hhu, count = %llu, signal quality, descrambling, or HDD load problem?",
                      header.Pid, header.ContinuityCounter,
                      info.PrevContinuityCounter, expectedContinuityCounter,
                      m_discontinuityCount);
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
          (
            !m_waitingForPcr &&
            !header.PayloadUnitStart &&
            (
              (!m_generatePcrFromPts && header.Pid != m_originalPcrPid) ||
              (m_generatePcrFromPts && header.Pid != m_substitutePcrSourcePid)
            )
          ) ||
          (
            m_waitingForPcr &&
            (
              (!m_generatePcrFromPts && header.Pid != m_originalPcrPid) ||
              (m_generatePcrFromPts && (info.FakePid & 0xff0) != PID_AUDIO_FIRST)
            )
          )
        )
        {
          return;
        }

        if (!m_waitingForPcr && header.PayloadUnitStart)
        {
          info.SeenStart = true;
          CPidTable& pidTable = m_pmtParser.GetPidInfo();
          bool foundPid = false;
          for (vector<VideoPid*>::const_iterator it2 = pidTable.VideoPids.begin(); it2 != pidTable.VideoPids.end(); it2++)
          {
            if ((*it2)->Pid == info.OriginalPid)
            {
              foundPid = true;
              WriteLog(L"PID %hu start of video detected", info.OriginalPid);
              if (m_observer != NULL)
              {
                m_observer->OnSeen(info.OriginalPid, (unsigned long)Video);
              }
              break;
            }
          }
          if (!foundPid)
          {
            for (vector<AudioPid*>::const_iterator it2 = pidTable.AudioPids.begin(); it2 != pidTable.AudioPids.end(); it2++)
            {
              if ((*it2)->Pid == info.OriginalPid)
              {
                WriteLog(L"PID %hu start of audio detected", info.OriginalPid);
                if (m_observer != NULL)
                {
                  m_observer->OnSeen(info.OriginalPid, (unsigned long)Audio);
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
      unsigned char localTsPacket[TS_PACKET_LEN];
      memcpy(localTsPacket, tsPacket, TS_PACKET_LEN);
      if (!m_generatePcrFromPts && header.Pid == m_originalPcrPid)
      {
        // Overwrite the PCR timestamps.
        PatchPcr(localTsPacket, header);
      }
      // Corner PCR case: undesirable PCR timestamps.
      else if (
        info.FakePid == m_fakePcrPid &&
        header.HasAdaptionField &&
        header.AdaptionFieldLength >= 1 + PCR_LENGTH &&
        (localTsPacket[ADAPTATION_FIELD_FLAG_OFFSET] & PCR_FLAG_BIT) != 0
      )
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
        memcpy(&localTsPacket[PCR_OFFSET],
                &tsPacket[PCR_OFFSET + PCR_LENGTH],
                header.AdaptionFieldLength - PCR_LENGTH - 1);
        // The end of the adaption field is now stuffing.
        memset(&localTsPacket[PCR_OFFSET + header.AdaptionFieldLength - PCR_LENGTH - 1],
                0xff,
                PCR_LENGTH);
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

      unsigned char result = GetPesHeader(localTsPacket, header, info);
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
      unsigned char i = 0;
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
    if (
      header.Pid == m_originalPcrPid &&
      !m_generatePcrFromPts &&
      m_videoAudioStartTimeStamp != -1 &&
      header.HasAdaptionField &&
      header.AdaptionFieldLength >= 1 + PCR_LENGTH &&
      (tsPacket[ADAPTATION_FIELD_FLAG_OFFSET] & PCR_FLAG_BIT) != 0
    )
    {
      // Patch the PCR.
      unsigned char localTsPacket[TS_PACKET_LEN];
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
    WriteLog(L"unhandled exception in OnTsPacket()");
  }
}

void CDiskRecorder::WriteLog(const wchar_t* fmt, ...)
{
  wchar_t buffer[2000];
  va_list ap;
  va_start(ap, fmt);
  vswprintf(buffer, sizeof(buffer) / sizeof(buffer[0]), fmt, ap);
  va_end(ap);

  if (m_recorderMode == TimeShift)
  {
    LogDebug(L"time-shifter: %s", buffer);
  }
  else
  {
    LogDebug(L"recorder: %s", buffer);
  }
}

void CDiskRecorder::WritePacket(unsigned char* tsPacket)
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
  WritePacketDirect(tsPacket);
  m_tsPacketCount++;
  m_serviceInfoPacketCounter++;
}

void CDiskRecorder::WritePacketDirect(unsigned char* tsPacket)
{
  try
  {
    if (m_writeBufferPosition + TS_PACKET_LEN > m_writeBufferSize)
    {
      WriteLog(L"write buffer overflow, position = %lu, buffer size = %lu bytes",
                m_writeBufferPosition, m_writeBufferSize);
      m_writeBufferPosition = 0;
      return;
    }

    // Copy the packet into our buffer.
    memcpy(&m_writeBuffer[m_writeBufferPosition], tsPacket, TS_PACKET_LEN);
    m_writeBufferPosition += TS_PACKET_LEN;

    if (m_recorderMode == Record)
    {
      // Should we flush the buffer?
      if (m_fileRecording == NULL)
      {
        m_writeBufferPosition = 0;
        return;
      }
      if (m_writeBufferPosition < RECORD_BUFFER_SIZE)
      {
        return;
      }

      HRESULT hr = m_fileRecording->Write(m_writeBuffer, m_writeBufferPosition, m_isDropping);
      if (FAILED(hr))
      {
        if (!m_isDropping)
        {
          WriteLog(L"failed to write to file, hr = 0x%x, byte count = %lu, file = %s",
                    hr, m_writeBufferPosition, m_fileName.str());
          WriteLog(L"starting to drop data, dropped byte count = %llu",
                    m_droppedByteCount);
          m_isDropping = true;
        }
        m_droppedByteCount += m_writeBufferPosition;
      }
      else if (m_isDropping)
      {
        WriteLog(L"stop dropping data, dropped byte count = %llu",
                  m_droppedByteCount);
        m_isDropping = false;
      }
      m_writeBufferPosition = 0;
      return;
    }

    // Should we flush the buffer?
    if (m_fileTimeShifting == NULL)
    {
      m_writeBufferPosition = 0;
      return;
    }
    unsigned char currentThrottlePacketCount = WRITE_BUFFER_THROTTLE_STEP_PACKET_COUNTS[m_writeBufferThrottleStep];
    unsigned short currentThrottleBufferSize = currentThrottlePacketCount * TS_PACKET_LEN;
    if (m_writeBufferPosition < currentThrottleBufferSize)
    {
      return;
    }

    HRESULT hr = m_fileTimeShifting->Write(m_writeBuffer, m_writeBufferPosition, m_isDropping);
    if (FAILED(hr))
    {
      if (!m_isDropping)
      {
        WriteLog(L"failed to write to file, hr = 0x%x, byte count = %lu, file = %s",
                  hr, m_writeBufferPosition, m_fileName.str());
        WriteLog(L"starting to drop data, dropped byte count = %llu",
                  m_droppedByteCount);
        m_isDropping = true;
        m_isWriteBufferThrottleFullyOpen = false;
        m_writeBufferThrottleStep = 0;
      }
      m_droppedByteCount += m_writeBufferPosition;
      m_writeBufferPosition = 0;
      return;
    }

    if (m_isDropping)
    {
      WriteLog(L"stop dropping data, dropped byte count = %llu",
                m_droppedByteCount);
      m_isDropping = false;
    }
    m_writeBufferPosition = 0;

    // Open the throttle one more step if it isn't already fully open.
    if (
      !m_isWriteBufferThrottleFullyOpen &&
      m_writeBufferThrottleStep < m_writeBufferThrottleFullyOpenStep
    )
    {
      m_writeBufferThrottleStep++;
      currentThrottlePacketCount = WRITE_BUFFER_THROTTLE_STEP_PACKET_COUNTS[m_writeBufferThrottleStep];
      currentThrottleBufferSize = currentThrottlePacketCount * TS_PACKET_LEN;
      if (m_writeBufferThrottleStep == m_writeBufferThrottleFullyOpenStep)
      {
        WriteLog(L"fully open throttle, %hhu packets, %hu bytes",
                  currentThrottlePacketCount, currentThrottleBufferSize);
        m_isWriteBufferThrottleFullyOpen = true;
      }
      else
      {
        WriteLog(L"open throttle, %hhu packets, %hu bytes",
                  currentThrottlePacketCount, currentThrottleBufferSize);
      }
    }
  }
  catch (...)
  {
    WriteLog(L"unhandled exception in WritePacketDirect()");
    m_writeBufferPosition = 0;
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
  unsigned long crc = CalculatCrc32(&m_fakePat[1], 12);
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
  unsigned short fakePmtOffset = 13;

  // Mark all PIDs as not present so we can identify the PIDs that really
  // aren't present after the loop.
  map<unsigned short, PidInfo*>::iterator infoIt = m_pids.begin();
  for ( ; infoIt != m_pids.end(); infoIt++)
  {
    (infoIt->second)->IsStillPresent = false;
  }

  // Write the elementary stream section of the PMT.
  vector<VideoPid*>::const_iterator vPidIt = pidTable.VideoPids.begin();
  for ( ; vPidIt != pidTable.VideoPids.end(); vPidIt++)
  {
    AddPidToPmt(*vPidIt, "video", m_nextFakePidVideo, fakePmtOffset);
  }
  vector<AudioPid*>::const_iterator aPidIt = pidTable.AudioPids.begin();
  for ( ; aPidIt != pidTable.AudioPids.end(); aPidIt++)
  {
    AddPidToPmt(*aPidIt, "audio", m_nextFakePidAudio, fakePmtOffset);
  }
  vector<SubtitlePid*>::const_iterator sPidIt = pidTable.SubtitlePids.begin();
  for ( ; sPidIt != pidTable.SubtitlePids.end(); sPidIt++)
  {
    AddPidToPmt(*sPidIt, "subtitles", m_nextFakePidSubtitles, fakePmtOffset);
  }
  vector<TeletextPid*>::const_iterator tPidIt = pidTable.TeletextPids.begin();
  for ( ; tPidIt != pidTable.TeletextPids.end(); tPidIt++)
  {
    AddPidToPmt(*tPidIt, "teletext", m_nextFakePidTeletext, fakePmtOffset);
  }
  vector<VbiPid*>::const_iterator vbiPidIt = pidTable.VbiPids.begin();
  for ( ; vbiPidIt != pidTable.VbiPids.end(); vbiPidIt++)
  {
    AddPidToPmt(*tPidIt, "VBI", m_nextFakePidVbi, fakePmtOffset);
  }

  // All PIDs that are still marked as not present should be removed.
  PidInfo* info = NULL;
  infoIt = m_pids.begin();
  while (infoIt != m_pids.end())
  {
    info = infoIt->second;
    if (!info->IsStillPresent)
    {
      WriteLog(L"remove stream, PID = %hu, fake PID = %hu",
                info->OriginalPid, info->FakePid);

      // We lost our PTS source for PCR conversion.
      if (m_generatePcrFromPts && info->OriginalPid == m_substitutePcrSourcePid)
      {
        WriteLog(L"unset PTS source PID");
        m_substitutePcrSourcePid = PID_NOT_SET;
      }

      if (infoIt->second != NULL)
      {
        delete infoIt->second;
        infoIt->second = NULL;
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
  unsigned long crc = CalculatCrc32(&m_fakePmt[1], fakePmtOffset - 1);
  m_fakePmt[fakePmtOffset++] = (crc >> 24) & 0xff;
  m_fakePmt[fakePmtOffset++] = (crc >> 16) & 0xff;
  m_fakePmt[fakePmtOffset++] = (crc >> 8) & 0xff;
  m_fakePmt[fakePmtOffset++] = crc & 0xff;

  m_serviceInfoPacketCounter = SERVICE_INFO_INJECT_RATE;
}

void CDiskRecorder::ClearPids()
{
  map<unsigned short, PidInfo*>::iterator it = m_pids.begin();
  for ( ; it != m_pids.end(); it++)
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
  }
  m_pids.clear();
}

void CDiskRecorder::AddPidToPmt(BasePid* pid,
                                const string& pidType,
                                unsigned short& nextFakePid,
                                unsigned short& pmtOffset)
{
  PidInfo* info = NULL;
  map<unsigned short, PidInfo*>::const_iterator infoIt = m_pids.find(pid->Pid);
  if (infoIt == m_pids.end())
  {
    info = new PidInfo();
    if (info == NULL)
    {
      WriteLog(L"failed to allocate PidInfo in AddPidToPmt()");
      return;
    }

    info->OriginalPid = pid->Pid;
    info->FakePid = nextFakePid++;
    info->SeenStart = false;
    info->PrevContinuityCounter = CONTINUITY_COUNTER_NOT_SET;
    info->TsPacketQueueLength = 0;
    info->PesHeaderLength = 0;
    info->PrevPts = -1;
    info->PrevPtsTimeStamp = 0;
    info->PrevDts = -1;
    info->PrevDtsTimeStamp = 0;
    m_pids[pid->Pid] = info;
    WriteLog(L"add %S stream, PID = %hu, fake PID = %hu, stream type = 0x%hhx, logical stream type = 0x%hhx",
              pidType.c_str(), pid->Pid, info->FakePid, pid->StreamType,
              pid->LogicalStreamType);
  }
  else
  {
    info = infoIt->second;
    WriteLog(L"update %S stream, PID = %hu, fake PID = %hu, stream type = 0x%hhx, logical stream type = 0x%hhx",
              pidType.c_str(), pid->Pid, info->FakePid, pid->StreamType,
              pid->LogicalStreamType);
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
    WriteLog(L"  set fake PCR PID");
    m_fakePcrPid = info->FakePid;
  }
}

void CDiskRecorder::WriteFakeServiceInfo()
{
  // PAT
  unsigned char packet[TS_PACKET_LEN];
  packet[0] = TS_PACKET_SYNC;
  packet[1] = ((PID_PAT >> 8) & 0x1f) | 0x40;
  packet[2] = PID_PAT & 0xff;
  m_patContinuityCounter = (m_patContinuityCounter + 1) & 0xf;
  packet[3] = 0x10 | m_patContinuityCounter;
  unsigned short fakePatLength = 17;
  memcpy(&packet[4], &m_fakePat[0], fakePatLength);
  memset(&packet[4 + fakePatLength], 0xff, TS_PACKET_LEN - 4 - fakePatLength);
  WritePacketDirect(packet);

  // PMT
  unsigned short fakePmtLength = ((m_fakePmt[2] & 0xf) << 8) + m_fakePmt[3] + 4; // + 4 to include pointer byte, table ID and section length bytes
  unsigned short pointer = 0;
  packet[1] = ((PID_PMT >> 8) & 0x1f) | 0x40;
  packet[2] = PID_PMT & 0xff;
  while (fakePmtLength > 0)
  {
    m_pmtContinuityCounter = (m_pmtContinuityCounter + 1) & 0xf;
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
    WritePacketDirect(packet);
    pointer += TS_PACKET_LEN - 4;

    packet[1] &= 0x1f;  // unset payload unit start indicator
  }
}

bool CDiskRecorder::ReadParameters()
{
  wchar_t fileName[MAX_PATH];
  swprintf(fileName, L"%s.tvedrpts", MAX_PATH, m_fileName.str().c_str());
  if (!CFileUtils::Exists(fileName))
  {
    return false;
  }

  unsigned char buffer[PARAM_BUFFER_SIZE];
  unsigned long bufferSize = PARAM_BUFFER_SIZE;
  HRESULT hr = FileReader::Read(fileName, buffer, bufferSize);
  CFileUtils::DeleteFile(fileName);
  if (hr != S_OK)
  {
    return false;
  }
  if (bufferSize != PARAM_BUFFER_SIZE || *buffer != 1)
  {
    WriteLog(L"unsupported parameter file format, buffer size = %lu",
              bufferSize);
    return false;
  }

  unsigned char* pointer = &buffer[1];

  m_patContinuityCounter = *pointer;
  pointer += sizeof(unsigned char);

  m_patVersion = *pointer;
  pointer += sizeof(unsigned char);

  m_pmtContinuityCounter = *pointer;
  pointer += sizeof(unsigned char);

  m_pmtVersion = *pointer;
  pointer += sizeof(unsigned char);

  m_prevPcr.PcrReferenceBase = *((unsigned long long*)pointer);
  pointer += sizeof(unsigned long long);

  m_prevPcr.PcrReferenceExtension = *((unsigned long long*)pointer);
  pointer += sizeof(unsigned long long);

  m_prevPcrReceiveTimeStamp = (clock_t)*((unsigned long long*)pointer);
  pointer += sizeof(unsigned long long);

  m_averagePcrIncrement = (double)*((long long*)pointer);
  pointer += sizeof(long long);

  m_pcrCompensation = *((long long*)pointer);
  pointer += sizeof(long long);

  m_timeShiftingParameters.MaximumFileSize = *((unsigned long long*)pointer);
  pointer += sizeof(unsigned long long);

  m_timeShiftingParameters.ReservationChunkSize = *((unsigned long long*)pointer);
  pointer += sizeof(unsigned long long);

  m_timeShiftingParameters.FileCountMinimum = *((unsigned long*)pointer);
  pointer += sizeof(unsigned long);

  m_timeShiftingParameters.FileCountMaximum = *((unsigned long*)pointer);
  return true;
}

void CDiskRecorder::WriteParameters()
{
  unsigned char buffer[PARAM_BUFFER_SIZE];
  unsigned char* pointer = buffer;

  *pointer = 1;    // format/version number
  pointer += sizeof(unsigned char);

  *pointer = m_patContinuityCounter;
  pointer += sizeof(unsigned char);

  *pointer = m_patVersion;
  pointer += sizeof(unsigned char);

  *pointer = m_pmtContinuityCounter;
  pointer += sizeof(unsigned char);

  *pointer = m_pmtVersion;
  pointer += sizeof(unsigned char);

  *((unsigned long long*)pointer) = m_prevPcr.PcrReferenceBase;
  pointer += sizeof(unsigned long long);

  *((unsigned long long*)pointer) = m_prevPcr.PcrReferenceExtension;
  pointer += sizeof(unsigned long long);

  *((unsigned long long*)pointer) = (unsigned long long)m_prevPcrReceiveTimeStamp;
  pointer += sizeof(unsigned long long);

  *((long long*)pointer) = (long long)m_averagePcrIncrement;
  pointer += sizeof(long long);

  *((long long*)pointer) = m_pcrCompensation;
  pointer += sizeof(long long);

  *((unsigned long long*)pointer) = m_timeShiftingParameters.MaximumFileSize;
  pointer += sizeof(unsigned long long);

  *((unsigned long long*)pointer) = m_timeShiftingParameters.ReservationChunkSize;
  pointer += sizeof(unsigned long long);

  *((unsigned long*)pointer) = m_timeShiftingParameters.FileCountMinimum;
  pointer += sizeof(unsigned long);

  *((unsigned long*)pointer) = m_timeShiftingParameters.FileCountMaximum;

  FileWriter writer;
  wchar_t fileName[MAX_PATH];
  swprintf(fileName, L"%s.tvedrpts", MAX_PATH, m_fileName.str().c_str());
  if (writer.OpenFile(fileName) == S_OK)
  {
    writer.Write(buffer, PARAM_BUFFER_SIZE);
    writer.CloseFile();
  }
}

void CDiskRecorder::InjectPcrFromPts(PidInfo& info)
{
  // We only use PTS from audio PIDs because it is guaranteed to be sequential.
  if (m_substitutePcrSourcePid == PID_NOT_SET && (info.FakePid & 0xff0) == PID_AUDIO_FIRST)
  {
    WriteLog(L"PID %hu found first PTS", info.OriginalPid);
    m_substitutePcrSourcePid = info.OriginalPid;

    // Update the PMT - PCR PID and CRC.
    m_fakePcrPid = PID_PCR;
    m_fakePmt[9] = ((m_fakePcrPid >> 8) & 0x1f) | 0xe0;
    m_fakePmt[10] = m_fakePcrPid & 0xff;

    unsigned short sectionLength = ((m_fakePmt[2] & 0xf) << 8) | m_fakePmt[3];
    unsigned long crc = CalculatCrc32(&m_fakePmt[1], sectionLength - 1);      // + 3 for the table ID and section length bytes - 4 for the CRC bytes
    m_fakePmt[sectionLength++] = (crc >> 24) & 0xff;  // Section length is the right offset because it doesn't count the pointer byte, table ID and section length bytes.
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
      WriteLog(L"failed to decode PTS for PCR generation");
    }
    else
    {
      long long adjustedPts = (pts.PcrReferenceBase - 27000) & MAX_PCR_BASE;  // offset PCR from PTS by 300 ms to give time for demuxing and decoding
      unsigned char pcrPacket[TS_PACKET_LEN];
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
        WritePacketDirect(pcrPacket);
      }
    }
  }
}

void CDiskRecorder::PatchPcr(unsigned char* tsPacket, CTsHeader& header)
{
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
  clock_t timeStamp = clock();
  
  if (m_waitingForPcr)
  {
    m_waitingForPcr = false;
    m_serviceInfoPacketCounter = SERVICE_INFO_INJECT_RATE;
    m_pcrGapConfirmationCount = 0;
    if (m_pcrCompensation == 0)
    {
      // First estimate of PCR speed is based on ISO/IEC 13818-1:
      // They have a resolution of one part in 27 000 000 per second, and occur at intervals up to
      // 100 ms in Transport Streams, or up to 700 ms in Program Streams.
      // ... and TR 101 290:
      // In DVB a cycle period of not more than 40 ms is recommended.
      m_averagePcrIncrement = 3600;   // 90 kHz ticks => 40 ms (3600 / 90000 = 0.04)
      // PCR compensation is set in such a way as to ensure PCR starts at zero.
      m_pcrCompensation = (0 - pcrNew.PcrReferenceBase) & MAX_PCR_BASE;
    }
    else
    {
      // Adjust the PCR compensation to try to achieve a smooth transition between streams. The
      // transition won't be perfect because the PCR increment is an estimate and the transition
      // is unlikely to occur at the exact time we were expecting the next PCR.
      unsigned long long predictedNextPcrOldStream = (m_prevPcr.PcrReferenceBase + (unsigned long long)m_averagePcrIncrement) & MAX_PCR_BASE;
      m_pcrCompensation = (m_pcrCompensation + predictedNextPcrOldStream - pcrNew.PcrReferenceBase) & MAX_PCR_BASE;
    }
    m_prevPcr = pcrNew;

    CPcr nextPcrRollOver;
    nextPcrRollOver.PcrReferenceBase = MAX_PCR_BASE - pcrNew.PcrReferenceBase;
    nextPcrRollOver.PcrReferenceExtension = MAX_PCR_EXTENSION - pcrNew.PcrReferenceExtension;
    WriteLog(L"PCR found, value = %llu, compensation = %lld, next broadcast program clock reference roll-over in %S",
              pcrNew.PcrReferenceBase, m_pcrCompensation,
              nextPcrRollOver.ToString());
  }
  else
  {
    if (pcrNew.PcrReferenceBase < 0x10000 && m_prevPcr.PcrReferenceBase > 0x1ffff0000)
    {
      WriteLog(L"normal broadcast program clock reference roll-over passed");
    }

    // Calculate the difference between the new and previous PCR values. The unit is 90 kHz ticks.
    long long pcrDifference = PcrDifference(pcrNew.PcrReferenceBase, m_prevPcr.PcrReferenceBase);

    if (pcrDifference < 0)
    {
      // Normally the PCR should increase. A decrease is unexpected.
      HandlePcrInstability(pcrNew, pcrDifference);
    }
    else
    {
      // Is this PCR increase unexpectedly large? 8x (800%) normal is an arbitrary threshold.
      if (pcrDifference > 8 * m_averagePcrIncrement)
      {
        // Yes. Is it coherent/consistent with the local system clock change?
        long long expectedPcrDifference = (timeStamp - m_prevPcrReceiveTimeStamp) * 90; // # 90 kHz ticks expected based on the local system clock
        long long pcrDrift = expectedPcrDifference - pcrDifference;
        if (expectedPcrDifference > 0 && pcrDrift < 15000 && pcrDrift > -15000)         // 90 kHz ticks => +/- 166 ms (15000 / 90000 = 0.166)
        {
          // Yes. Assume signal was lost or the stream stopped for awhile and do nothing.
          WriteLog(L"coherrent PCR jump %lld detected (expected %lld), signal quality or HDD load problem?",
                    pcrDifference, expectedPcrDifference);
          m_prevPcr = pcrNew;
        }
        else
        {
          // No, so this really is unexpected.
          HandlePcrInstability(pcrNew, pcrDifference);
        }
      }
      else
      {
        // Normal situation - stable/regular/timely PCR.
        // We assume that PCR is delivered at a regular interval with an increment approximated by
        // m_averagePcrIncrement. Improve the PCR increment estimate by comparison with the current
        // increment. Use a 10% adjustment factor.
        m_averagePcrIncrement += ((double)pcrDifference - m_averagePcrIncrement) * 0.1f;
        if (m_pcrGapConfirmationCount > 0)
        {
          WriteLog(L"PCR restabilised after %hhu confirmation(s), average increment = %lld",
                    m_pcrGapConfirmationCount,
                    (long long)m_averagePcrIncrement);
        }
        m_pcrGapConfirmationCount = 0;
        m_prevPcr = pcrNew;
      }
    }
  }

  long long adjustedPcr = (m_prevPcr.PcrReferenceBase + m_pcrCompensation) & MAX_PCR_BASE;
  SetPcrBase(tsPacket, adjustedPcr);

  m_prevPcrReceiveTimeStamp = timeStamp;
}

void CDiskRecorder::HandlePcrInstability(CPcr& pcrNew, long long pcrChange)
{
  // Estimate the expected PCR value. This estimate should be slower/less than reality to avoid
  // falling into the negative PCR change case and perpetual instability.
  m_prevPcr.PcrReferenceBase += (long long)(m_averagePcrIncrement / 2);
  m_prevPcr.PcrReferenceBase &= MAX_PCR_BASE;

  long long futureCompensation = m_pcrCompensation - pcrChange + (long long)m_averagePcrIncrement;
  if (m_pcrGapConfirmationCount > 0)
  {
    // We previously detected instability. Has that instability settled yet?
    long long compensationDifference = PcrDifference(m_pcrFutureCompensation, futureCompensation);
    m_pcrFutureCompensation = futureCompensation;
    if (compensationDifference < -8 * m_averagePcrIncrement || compensationDifference > 8 * m_averagePcrIncrement)
    {
      // No, looks like the instability is continuing.
      WriteLog(L"ongoing PCR instability, change = %lld, confirmation count = %hhu",
                pcrChange, m_pcrGapConfirmationCount);
      m_pcrGapConfirmationCount = 1;
    }
    else
    {
      // Yes, it is settling. Shall we go back to normal operation?
      m_pcrGapConfirmationCount++;
      if (m_pcrGapConfirmationCount >= 3)
      {
        // Yes. We're back to normal!
        m_pcrGapConfirmationCount = 0;
        m_prevPcr = pcrNew;
        m_pcrCompensation = m_pcrFutureCompensation;
        WriteLog(L"PCR stabilised, change = %lld, compensation = %lld",
                  pcrChange, m_pcrCompensation);
      }
    }
  }
  else
  {
    // Start of instability.
    m_pcrGapConfirmationCount = 1;
    m_pcrFutureCompensation = futureCompensation;
    WriteLog(L"PCR instability detected, change = %lld, average increment = %lld, compensation = %lld",
              pcrChange, (long long)m_averagePcrIncrement, m_pcrCompensation);
  }
}

void CDiskRecorder::PatchPtsDts(unsigned char* pesHeader, PidInfo& pidInfo)
{
  clock_t timeStamp = clock();

  // Sanity check PES header bytes.
  if (pesHeader[0] != 0 || pesHeader[1] != 0 || pesHeader[2] != 1)
  {
    return;
  }

  CPcr pts;
  CPcr dts;
  if (!CPcr::DecodeFromPesHeader(pesHeader, 0, pts, dts))
  {
    WriteLog(L"no PTS or DTS to decode, should not be detected here!");
    return;
  }

  if (pts.IsValid)
  {
    // Check for PTS jumps.
    /*if (
      pidInfo.PrevPts != -1 &&
      (
        (pidInfo.FakePid & 0xff0) == PID_VIDEO_FIRST ||
        (pidInfo.FakePid & 0xff0) == PID_AUDIO_FIRST)
      )
    )
    {
      long long ptsDifference = PcrDifference(pts.PcrReferenceBase, pidInfo.PrevPts);   // # 90 kHz broadcast clock ticks since previous PTS
      long long expectedPtsDifference = (timeStamp - pidInfo.PrevPtsTimeStamp) * 90;    // # 90 kHz ticks expected based on the local system clock
      long long ptsDrift = expectedPtsDifference - ptsDifference;
      if (ptsDrift < -30000LL || ptsDrift > 30000LL)                                    // 90 kHz ticks => +/- 333 ms (30000 / 90000 = 0.333)
      {
        WriteLog(L"PTS jump %lld detected for PID %hu stream ID 0x%hhx, expected increment = %lld, drift = %lld, previous PTS = %lld, new PTS = %llu",
                  ptsDifference, pidInfo.OriginalPid, pesHeader[3],
                  expectedPtsDifference, ptsDrift, pidInfo.PrevPts,
                  pts.PcrReferenceBase);
      }
    }*/

    // Patch the PTS with the PCR compensation. This seems to be enough to avoid freezes & crashes.
    long long ptsPatched = (pts.PcrReferenceBase + m_pcrCompensation) & MAX_PCR_BASE;
    pesHeader[13] = (unsigned char)((ptsPatched & 0x7f) << 1) + 1;
    ptsPatched >>= 7;
    pesHeader[12] = (unsigned char)(ptsPatched & 0xff);
    ptsPatched >>= 8;
    pesHeader[11] = (unsigned char)((ptsPatched & 0x7f) << 1) + 1;
    ptsPatched >>= 7;
    pesHeader[10] = (unsigned char)(ptsPatched & 0xff);
    ptsPatched >>= 8;
    pesHeader[9] = (unsigned char)((ptsPatched & 7) << 1) + 0x21;

    pidInfo.PrevPts = pts.PcrReferenceBase;
    pidInfo.PrevPtsTimeStamp = timeStamp;

    if (dts.IsValid)
    {
      /*if (
        pidInfo.PrevDts != -1 &&
        (
          (pidInfo.FakePid & 0xff0) == PID_VIDEO_FIRST ||
          (pidInfo.FakePid & 0xff0) == PID_AUDIO_FIRST)
        )
      )
      {
        long long dtsDifference = PcrDifference(dts.PcrReferenceBase, pidInfo.PrevDts); // # 90 kHz broadcast clock ticks since previous DTS
        long long expectedDtsDifference = (timeStamp - pidInfo.PrevPtsTimeStamp) * 90;  // # 90 kHz ticks expected based on the local system clock
        long long dtsDrift = expectedPtsDifference - dtsDifference;
        if (dtsDrift < -30000LL || dtsDrift > 30000LL)                                  // 90 kHz ticks => +/- 333 ms (30000 / 90000 = 0.333)
        {
          WriteLog(L"DTS jump %lld detected for PID %hu stream ID 0x%hhx, expected increment = %lld, drift = %lld, previous DTS = %lld, new DTS = %llu",
                    dtsDifference, pidInfo.OriginalPid, pesHeader[3],
                    expectedPtsDifference, dtsDrift, pidInfo.PrevDts,
                    dts.PcrReferenceBase);
        }
      }*/

      long long dtsPatched = (dts.PcrReferenceBase + m_pcrCompensation) & MAX_PCR_BASE;
      pesHeader[18] = (unsigned char)((dtsPatched & 0x7f) << 1) + 1;
      dtsPatched >>= 7;
      pesHeader[17] = (unsigned char)(dtsPatched & 0xff);
      dtsPatched >>= 8;
      pesHeader[16] = (unsigned char)((dtsPatched & 0x7f) << 1) + 1;
      dtsPatched >>= 7;
      pesHeader[15] = (unsigned char)((dtsPatched & 0xff));
      dtsPatched >>= 8;
      pesHeader[14] = (unsigned char)((dtsPatched & 7) << 1) + 0x31;

      pidInfo.PrevDts = dts.PcrReferenceBase;
      pidInfo.PrevDtsTimeStamp = timeStamp;
    }
  }
}

unsigned char CDiskRecorder::GetPesHeader(unsigned char* tsPacket,
                                          CTsHeader& header,
                                          PidInfo& info)
{
  // Update/populate the PES header in info.PesHeader.

  // Does this TS packet start a new PES packet?
  if (header.PayloadUnitStart)
  {
    if (info.TsPacketQueueLength != 0)
    {
      LogDebug(L"disk recorder: PID %hu has non-empty TS packet queue at start of new PES packet, %hhu TS packet(s) will be lost",
                header.Pid, info.TsPacketQueueLength);
      info.TsPacketQueueLength = 0;
    }
    info.PesHeaderLength = 0;
  }
  else if (info.TsPacketQueueLength >= MAX_TS_PACKET_QUEUE) 
  {
    LogDebug(L"disk recorder: PID %hu PES header starts after or is split over more than %hhu TS packets",
              header.Pid, MAX_TS_PACKET_QUEUE);
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
  unsigned char tsPacketPesByteCount = TS_PACKET_LEN - header.PayLoadStart;
  if (info.PesHeaderLength + tsPacketPesByteCount > MAX_PES_HEADER_BYTES)
  {
    tsPacketPesByteCount = MAX_PES_HEADER_BYTES - info.PesHeaderLength;
  }
  memcpy(&info.PesHeader[info.PesHeaderLength],
          &tsPacket[header.PayLoadStart],
          tsPacketPesByteCount);
  info.PesHeaderLength += tsPacketPesByteCount;

  // Do we have enough of the PES header to determine whether PTS and/or DTS
  // might be present?
  if (info.PesHeaderLength < PES_HEADER_ATTRIBUTES_OFFSET + 1)
  {
    return 2; // No, not yet.
  }

  // Might the header contain PTS and/or DTS?
  unsigned char pesPacketHeaderAttributes = info.PesHeader[PES_HEADER_ATTRIBUTES_OFFSET] & (PTS_FLAG_BIT | DTS_FLAG_BIT);
  if (pesPacketHeaderAttributes == 0)
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
  switch (pesPacketHeaderAttributes)
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

void CDiskRecorder::UpdatePesHeader(PidInfo& info)
{
  // Overwrite the PTS and/or DTS in the queued packets with patched PTS and/or DTS.
  unsigned char i = 0;
  unsigned short pesUpdatedByteCount = 0;
  do
  {
    unsigned char tsPacketPayloadStart = info.TsPacketPayloadStartQueue[i];
    if (tsPacketPayloadStart != 0xff)
    {
      unsigned char tsPacketPesByteCount = TS_PACKET_LEN - tsPacketPayloadStart;
      if (tsPacketPesByteCount + pesUpdatedByteCount > info.PesHeaderLength)
      {
        tsPacketPesByteCount = info.PesHeaderLength - pesUpdatedByteCount;
      }
      if (info.PesHeaderLength > pesUpdatedByteCount)
      {
        memcpy(&info.TsPacketQueue[i][tsPacketPayloadStart],
                &info.PesHeader[pesUpdatedByteCount],
                tsPacketPesByteCount);
      }
      pesUpdatedByteCount += tsPacketPesByteCount;
    }
    i++;
  }
  while (i < info.TsPacketQueueLength);
}

long long CDiskRecorder::PcrDifference(long long newTs, long long prevTs)
{
  // Compute a signed difference between the new and previous timestamps.
  long long difference = newTs - prevTs;
  if (difference & 0x100000000)
  {
    difference |= 0xffffffff00000000LL; // negative
  }
  else
  {
    difference &= 0x00000000ffffffffLL; // positive
  }
  return difference;
}

void CDiskRecorder::SetPcrBase(unsigned char* tsPacket, long long pcrBaseValue)
{
  tsPacket[6] = (unsigned char)((pcrBaseValue >> 25) & 0xff);
  tsPacket[7] = (unsigned char)((pcrBaseValue >> 17) & 0xff);
  tsPacket[8] = (unsigned char)((pcrBaseValue >> 9) & 0xff);
  tsPacket[9] = (unsigned char)((pcrBaseValue >> 1) & 0xff);
  tsPacket[10] = (unsigned char)(((pcrBaseValue & 0x1) << 7) + (tsPacket[10] & 0x7e));
}