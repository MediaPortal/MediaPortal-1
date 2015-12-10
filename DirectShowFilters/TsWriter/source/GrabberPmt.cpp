/*
 *  Copyright (C) 2006-2010 Team MediaPortal
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
#include "GrabberPmt.h"
#include <algorithm>
#include <cstddef>    // NULL
#include <cstring>    // memcpy()
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"
#include "Utils.h"


#define VERSION_NOT_SET 0xff


extern void LogDebug(const wchar_t* fmt, ...);

CGrabberPmt::CGrabberPmt(IEncryptionAnalyser* analyser)
{
  m_isReady = false;
  m_version = VERSION_NOT_SET;
  m_lastSeenTime = 0;
  m_programNumber = 0;
  m_encryptionAnalyser = analyser;
  SetCallBack(NULL);
}

CGrabberPmt::~CGrabberPmt(void)
{
  m_encryptionAnalyser = NULL;
  SetCallBack(NULL);
}

void CGrabberPmt::Reset()
{
  LogDebug(L"PMT %d %hu: reset", GetPid(), m_programNumber);
  CEnterCriticalSection lock(m_section);
  CSectionDecoder::Reset();
  m_isReady = false;
  m_version = VERSION_NOT_SET;
  m_lastSeenTime = 0;
  m_pmtSection.Reset();
  LogDebug(L"PMT %d %hu: reset done", GetPid(), m_programNumber);
}

void CGrabberPmt::SetFilter(unsigned short pid, unsigned short programNumber)
{
  CEnterCriticalSection lock(m_section);
  SetPid(pid);
  m_programNumber = programNumber;
}

void CGrabberPmt::SetCallBack(ICallBackPmt* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CGrabberPmt::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  if (m_lastSeenTime != 0 && CTimeUtils::ElapsedMillis(m_lastSeenTime) >= 30000)
  {
    LogDebug(L"PMT %d %hu: removed", GetPid(), m_programNumber);
    CEnterCriticalSection lock(m_section);
    if (m_callBack != NULL)
    {
      m_callBack->OnPmtRemoved(m_programNumber, GetPid());
    }
    Reset();
  }

  CSectionDecoder::OnTsPacket(header, tsPacket);
}

void CGrabberPmt::OnNewSection(CSection& section)
{
  try
  {
    if (
      section.table_id != TABLE_ID_PMT ||
      !section.SectionSyntaxIndicator ||
      section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }

    CEnterCriticalSection lock(m_section);
    if (section.table_id_extension != m_programNumber)
    {
      return;
    }
    if (section.version_number == m_version)
    {
      m_lastSeenTime = clock();
      return;
    }
    if (section.SectionNumber != 0 || section.LastSectionNumber != 0)
    {
      // According to ISO/IEC 13818-1 PMT should only have one section per program.
      LogDebug(L"PMT %d %hu: unsupported multi-section table, version number = %d, section number = %d, last section number = %d",
                GetPid(), m_programNumber, section.version_number,
                section.SectionNumber, section.LastSectionNumber);
      return;
    }
    if (section.section_length > 1021 || section.section_length < 13)
    {
      LogDebug(L"PMT %d %hu: invalid section, length = %d",
                GetPid(), m_programNumber, section.section_length);
      return;
    }

    //LogDebug(L"PMT %d %hu: version number = %d, section length = %d",
    //          GetPid(), m_programNumber, section.version_number,
    //          section.section_length);

    m_isReady = true;
    if (m_version != VERSION_NOT_SET)
    {
      LogDebug(L"PMT %d %hu: changed, version number = %d, prev. version number = %hhu",
                GetPid(), m_programNumber, section.version_number, m_version);
      if (m_callBack != NULL)
      {
        m_callBack->OnPmtChanged(m_programNumber,
                                  GetPid(),
                                  section.Data,
                                  section.section_length + 3);  // + 3 for table ID and section length bytes
      }
      m_version = section.version_number;
      m_pmtSection = section;
    }
    else
    {
      LogDebug(L"PMT %d %hu: received, version number = %d",
                GetPid(), m_programNumber, section.version_number);
      m_version = section.version_number;
      m_pmtSection = section;   // must be before call back due to trigger of Freesat PID check
      if (m_callBack != NULL)
      {
        m_callBack->OnPmtReceived(m_programNumber,
                                  GetPid(),
                                  section.Data,
                                  section.section_length + 3);  // + 3 for table ID and section length bytes
      }
    }
    m_lastSeenTime = clock();
  }
  catch (...)
  {
    LogDebug(L"PMT %d %hu: unhandled exception in OnNewSection()",
              GetPid(), m_programNumber);
  }
}

bool CGrabberPmt::IsReady()
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

void CGrabberPmt::GetFilter(unsigned short& pid, unsigned short& programNumber) const
{
  CEnterCriticalSection lock(m_section);
  pid = GetPid();
  programNumber = m_programNumber;
}

bool CGrabberPmt::GetProgramInformation(unsigned short& pid,
                                        unsigned short& programNumber,
                                        bool& isPmtReceived,
                                        unsigned short& streamCountVideo,
                                        unsigned short& streamCountAudio,
                                        bool& isEncrypted,
                                        bool& isEncryptionDetectionAccurate,
                                        bool& isThreeDimensional,
                                        unsigned long* audioLanguages,
                                        unsigned char& audioLanguageCount,
                                        unsigned long* subtitlesLanguages,
                                        unsigned char& subtitlesLanguageCount)
{
  CEnterCriticalSection lock(m_section);
  pid = GetPid();
  programNumber = m_programNumber;
  isPmtReceived = m_isReady;
  if (!m_isReady || !m_parser.DecodePmtSection(m_pmtSection))
  {
    if (m_isReady)
    {
      LogDebug(L"PMT %d %hu: invalid PMT, decode failed",
                GetPid(), m_programNumber);
    }
    streamCountVideo = 0;
    streamCountAudio = 0;
    isEncrypted = false;
    isEncryptionDetectionAccurate = false;
    isThreeDimensional = false;
    audioLanguageCount = 0;
    subtitlesLanguageCount = 0;
    return !m_isReady;
  }

  CPidTable& pidTable = m_parser.GetPidInfo();
  streamCountVideo = pidTable.VideoPids.size();
  streamCountAudio = pidTable.AudioPids.size();
  LogDebug(L"PMT %d %hu: video stream count = %hu, audio stream count = %hu",
            GetPid(), m_programNumber, streamCountVideo, streamCountAudio);

  isEncrypted = false;    // Set true if any PID is encrypted.
  isEncryptionDetectionAccurate = false;
  vector<unsigned long> vectorLanguagesAudio;
  vector<unsigned long> vectorLanguagesSubtitles;

  vector<VideoPid*>::const_iterator videoPidIt = pidTable.VideoPids.begin();
  for ( ; videoPidIt != pidTable.VideoPids.end(); videoPidIt++)
  {
    VideoPid* pid = *videoPidIt;
    if (pid != NULL)
    {
      bool pidHasCaDescriptor;
      bool pidIsThreeDimensional;
      vector<unsigned long> pidLanguages;
      CheckDescriptorsPidVideo(pid->Descriptors,
                                pid->DescriptorsLength,
                                pidHasCaDescriptor,
                                pidIsThreeDimensional,
                                pidLanguages);
      isThreeDimensional |= pidIsThreeDimensional;

      EncryptionState state = EncryptionStateNotSet;
      if (m_encryptionAnalyser != NULL)
      {
        state = m_encryptionAnalyser->GetPidState(pid->Pid);
      }
      if (state == Encrypted || (pidHasCaDescriptor && state == EncryptionStateNotSet))
      {
        isEncrypted = true;
      }
      if (state != EncryptionStateNotSet)
      {
        isEncryptionDetectionAccurate = true;
      }

      vector<unsigned long>::const_iterator langIt = pidLanguages.begin();
      for ( ; langIt != pidLanguages.end(); langIt++)
      {
        vector<unsigned long>::const_iterator it = find(vectorLanguagesSubtitles.begin(),
                                                        vectorLanguagesSubtitles.end(),
                                                        *langIt);
        if (it == vectorLanguagesSubtitles.end())
        {
          vectorLanguagesSubtitles.push_back(*langIt);
        }
      }

      LogDebug(L"  video PID %hu, encryption state = %d, has CA descriptor = %d, language count = %llu, is 3D = %d",
                pid->Pid, state, pidHasCaDescriptor,
                (unsigned long long)pidLanguages.size(),
                pidIsThreeDimensional);
      CUtils::DebugVector(pidLanguages, L"  language(s)", true);
    }
  }

  vector<AudioPid*>::const_iterator audioPidIt = pidTable.AudioPids.begin();
  for ( ; audioPidIt != pidTable.AudioPids.end(); audioPidIt++)
  {
    AudioPid* pid = *audioPidIt;
    if (pid != NULL)
    {
      bool pidHasCaDescriptor;
      CheckDescriptorsPidAudio(pid->Descriptors, pid->DescriptorsLength, pidHasCaDescriptor);

      EncryptionState state = EncryptionStateNotSet;
      if (m_encryptionAnalyser != NULL)
      {
        state = m_encryptionAnalyser->GetPidState(pid->Pid);
      }
      if (state == Encrypted || (pidHasCaDescriptor && state == EncryptionStateNotSet))
      {
        isEncrypted = true;
      }
      if (state != EncryptionStateNotSet)
      {
        isEncryptionDetectionAccurate = true;
      }

      unsigned char pointer = 0;
      while (true)
      {
        unsigned long language = pid->Lang[pointer++];
        if (language == 0)
        {
          break;
        }
        language |= ((pid->Lang[pointer] << 8) | (pid->Lang[pointer + 1] << 16));
        pointer += 2;
        vector<unsigned long>::const_iterator it = find(vectorLanguagesAudio.begin(),
                                                        vectorLanguagesAudio.end(),
                                                        language);
        if (it == vectorLanguagesAudio.end())
        {
          vectorLanguagesAudio.push_back(language);
        }
      }

      LogDebug(L"  audio PID %hu, encryption state = %d, has CA descriptor = %d, language(s) = %S",
                pid->Pid, state, pidHasCaDescriptor, pid->Lang);
    }
  }

  vector<SubtitlePid*>::const_iterator subtitlePidIt = pidTable.SubtitlePids.begin();
  for ( ; subtitlePidIt != pidTable.SubtitlePids.end(); subtitlePidIt++)
  {
    SubtitlePid* pid = *subtitlePidIt;
    if (pid != NULL)
    {
      bool pidHasCaDescriptor;
      CheckDescriptorsPidAudio(pid->Descriptors, pid->DescriptorsLength, pidHasCaDescriptor);

      EncryptionState state = EncryptionStateNotSet;
      if (m_encryptionAnalyser != NULL)
      {
        state = m_encryptionAnalyser->GetPidState(pid->Pid);
      }
      if (state == Encrypted || (pidHasCaDescriptor && state == EncryptionStateNotSet))
      {
        isEncrypted = true;
      }
      if (state != EncryptionStateNotSet)
      {
        isEncryptionDetectionAccurate = true;
      }

      unsigned char pointer = 0;
      while (true)
      {
        unsigned long language = pid->Lang[pointer++];
        if (language == 0)
        {
          break;
        }
        language |= ((pid->Lang[pointer] << 8) | (pid->Lang[pointer + 1] << 16));
        pointer += 2;
        vector<unsigned long>::const_iterator it = find(vectorLanguagesSubtitles.begin(),
                                                        vectorLanguagesSubtitles.end(),
                                                        language);
        if (it == vectorLanguagesSubtitles.end())
        {
          vectorLanguagesSubtitles.push_back(language);
        }
      }

      LogDebug(L"  subtitles PID %hu, encryption state = %d, has CA descriptor = %d, language = %S",
                pid->Pid, state, pidHasCaDescriptor, pid->Lang);
    }
  }

  vector<TeletextPid*>::const_iterator teletextPidIt = pidTable.TeletextPids.begin();
  for ( ; teletextPidIt != pidTable.TeletextPids.end(); teletextPidIt++)
  {
    TeletextPid* pid = *teletextPidIt;
    if (pid != NULL)
    {
      vector<unsigned long> pidLanguages;
      CheckDescriptorsPidTeletext(pid->Descriptors, pid->DescriptorsLength, pidLanguages);

      vector<unsigned long>::const_iterator langIt = pidLanguages.begin();
      for ( ; langIt != pidLanguages.end(); langIt++)
      {
        vector<unsigned long>::const_iterator it = find(vectorLanguagesSubtitles.begin(),
                                                        vectorLanguagesSubtitles.end(),
                                                        *langIt);
        if (it == vectorLanguagesSubtitles.end())
        {
          vectorLanguagesSubtitles.push_back(*langIt);
        }
      }

      LogDebug(L"  teletext PID %hu, language count = %llu",
                pid->Pid, (unsigned long long)pidLanguages.size());
      CUtils::DebugVector(pidLanguages, L"  language(s)", true);
    }
  }

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(vectorLanguagesAudio,
                                  audioLanguages,
                                  audioLanguageCount,
                                  requiredCount) && audioLanguages != NULL)
  {
    LogDebug(L"PMT %d %hu: insufficient audio language array size, required size = %hhu, actual size = %hhu",
              GetPid(), m_programNumber, requiredCount, audioLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(vectorLanguagesSubtitles,
                                  subtitlesLanguages,
                                  subtitlesLanguageCount,
                                  requiredCount) && subtitlesLanguages != NULL)
  {
    LogDebug(L"PMT %d %hu: insufficient subtitles language array size, required size = %hhu, actual size = %hhu",
              GetPid(), m_programNumber, requiredCount, subtitlesLanguageCount);
  }
  return true;
}

bool CGrabberPmt::GetFreesatPids(bool& isFreesatProgram,
                                  unsigned short& pidEitSchedule,
                                  unsigned short& pidEitPresentFollowing,
                                  unsigned short& pidSdt,
                                  unsigned short& pidBat,
                                  unsigned short& pidNit)
{
  CEnterCriticalSection lock(m_section);
  if (!m_isReady || !m_parser.DecodePmtSection(m_pmtSection))
  {
    if (m_isReady)
    {
      LogDebug(L"PMT %d %hu: invalid PMT, decode failed",
                GetPid(), m_programNumber);
    }
    return false;
  }

  isFreesatProgram = false;
  CPidTable& pidTable = m_parser.GetPidInfo();
  vector<OtherPid*>::const_iterator it = pidTable.OtherPids.begin();
  for ( ; it != pidTable.OtherPids.end(); it++)
  {
    OtherPid* pid = *it;
    if (pid == NULL || pid->Descriptors == NULL)
    {
      continue;
    }

    unsigned char* descriptors = pid->Descriptors;
    unsigned short offset = 0;
    while (offset + 1 < pid->DescriptorsLength)
    {
      unsigned char tag = descriptors[offset++];
      unsigned char length = descriptors[offset++];
      if (tag == 0x5f)  // DVB private data specifier descriptor
      {
        if (
          length == 4 &&
          descriptors[offset] == 0x46 &&
          descriptors[offset + 1] == 0x53 &&
          descriptors[offset + 2] == 0x41 &&
          descriptors[offset + 3] == 0x54
        )
        {
          isFreesatProgram = true;
        }
        else
        {
          isFreesatProgram = false;
        }
      }
      else if (isFreesatProgram && tag == 0xd1)
      {
        for (unsigned char i = 0; i < length; i++)
        {
          unsigned char pidType = descriptors[offset + i];
          switch (pidType)
          {
            case 1:
              pidEitSchedule = pid->Pid;
              break;
            case 2:
              pidEitPresentFollowing = pid->Pid;
              break;
            case 3:   // SDT or BAT - carried on same PID -> not possible to distinguish => guess
              pidSdt = pid->Pid;
              break;
            case 4:   // SDT or BAT - carried on same PID -> not possible to distinguish => guess
              pidBat = pid->Pid;
              break;
            // 5, 6 = TDT, TOT
            case 7:
              pidNit = pid->Pid;
              break;
          }
        }
        break;
      }

      offset += length;
    }
  }

  return true;
}

bool CGrabberPmt::GetOpenTvEpgPids(bool& isOpenTvEpgProgram,
                                    vector<unsigned short>& pidsEvent,
                                    vector<unsigned short>& pidsDescription)
{
  CEnterCriticalSection lock(m_section);
  if (!m_isReady || !m_parser.DecodePmtSection(m_pmtSection))
  {
    if (m_isReady)
    {
      LogDebug(L"PMT %d %hu: invalid PMT, decode failed",
                GetPid(), m_programNumber);
    }
    return false;
  }

  isOpenTvEpgProgram = false;
  CPidTable& pidTable = m_parser.GetPidInfo();
  vector<OtherPid*>::const_iterator it = pidTable.OtherPids.begin();
  for ( ; it != pidTable.OtherPids.end(); it++)
  {
    OtherPid* pid = *it;
    if (pid == NULL || pid->Descriptors == NULL)
    {
      continue;
    }

    unsigned char* descriptors = pid->Descriptors;
    unsigned short offset = 0;
    while (offset + 1 < pid->DescriptorsLength)
    {
      unsigned char tag = descriptors[offset++];
      unsigned char length = descriptors[offset++];
      if (tag == 0x5f)  // DVB private data specifier descriptor
      {
        if (
          length == 4 &&
          descriptors[offset] == 0 &&
          descriptors[offset + 1] == 0 &&
          descriptors[offset + 2] == 0 &&
          descriptors[offset + 3] == 2
        )
        {
          isOpenTvEpgProgram = true;
        }
        else
        {
          isOpenTvEpgProgram = false;
        }
      }
      else if (isOpenTvEpgProgram && tag == 0xb0 && length == 1)
      {
        unsigned char pidType = descriptors[offset];
        if (pidType < 0x10)
        {
          pidsEvent.push_back(pid->Pid);
        }
        else if (pidType >= 0x20 && pidType < 0x30)
        {
          pidsDescription.push_back(pid->Pid);
        }
        break;
      }

      offset += length;
    }
  }

  return true;
}

bool CGrabberPmt::GetTable(unsigned char* table, unsigned short& tableBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  if (!m_isReady)
  {
    LogDebug(L"PMT %d %hu: not yet received", GetPid(), m_programNumber);
    tableBufferSize = 0;
    return false;
  }
  unsigned short requiredBufferSize = m_pmtSection.section_length + 3;  // + 3 for table ID and section length bytes
  if (table == NULL || tableBufferSize < requiredBufferSize)
  {
    LogDebug(L"PMT %d %hu: insufficient buffer size, required = %d, actual = %hu",
              GetPid(), m_programNumber, requiredBufferSize, tableBufferSize);
    return false;
  }
  memcpy(table, m_pmtSection.Data, requiredBufferSize);
  tableBufferSize = requiredBufferSize;
  return true;
}

void CGrabberPmt::CheckDescriptorsPidVideo(unsigned char* descriptors,
                                            unsigned short descriptorsLength,
                                            bool& hasCaDescriptor,
                                            bool& isThreeDimensionalVideo,
                                            vector<unsigned long>& captionsLanguages)
{
  hasCaDescriptor = false;
  isThreeDimensionalVideo = false;

  if (descriptors == NULL)
  {
    return;
  }

  unsigned short offset = 0;
  while (offset + 1 < descriptorsLength)
  {
    unsigned char tag = descriptors[offset++];
    unsigned char length = descriptors[offset++];
    if (tag == DESCRIPTOR_VIDEO_STREAM)   // ISO/IEC 13818 part 1
    {
      if (length >= 2 && offset + 1 < descriptorsLength && (descriptors[offset] & 0x04) == 0)
      {
        unsigned char profileAndLevelIndication = descriptors[offset + 1];
        if (
          profileAndLevelIndication == 0x8e ||
          profileAndLevelIndication == 0x8d ||
          profileAndLevelIndication == 0x8b ||
          profileAndLevelIndication == 0x8a
        )
        {
          isThreeDimensionalVideo = true;   // MPEG 2 video multi-view profiles
        }
      }
    }
    else if (tag == DESCRIPTOR_CONDITIONAL_ACCESS)
    {
      hasCaDescriptor = true;
    }
    else if (tag == DESCRIPTOR_AVC_VIDEO)   // ISO/IEC 13818 part 1
    {
      if (length >= 4 && offset + 3 < descriptorsLength)
      {
        // frame_packing_SEI_not_present_flag
        isThreeDimensionalVideo |= ((descriptors[offset + 3] & 0x20) == 0);   // frame compatible
        // Technically the stream could be non-3D if the frame packing
        // arrangement (FPA) supplemental enhancement information (SEI) message
        // frame_packing_arrangement_cancel_flag is set. We have no access to
        // that information.
      }
    }
    else if (tag == DESCRIPTOR_MVC_EXTENSION)   // ISO/IEC 13818 part 1
    {
      if (length >= 5 && offset + 4 < descriptorsLength)
      {
        // view_association_not_present
        isThreeDimensionalVideo |= ((descriptors[offset + 4] & 0x80) == 0);   // service compatible
      }
    }
    else if (tag == DESCRIPTOR_MPEG2_STEREOSCOPIC_VIDEO_FORMAT)   // ISO/IEC 13818 part 1
    {
      if (length >= 1 && offset < descriptorsLength)
      {
        // stereo_video_arrangement_type_present
        isThreeDimensionalVideo |= ((descriptors[offset] & 0x80) != 0);   // frame compatible
      }
    }
    else if (tag == DESCRIPTOR_STEREOSCOPIC_PROGRAM_INFO)   // ISO/IEC 13818 part 1
    {
      if (length >= 1 && offset < descriptorsLength)
      {
        unsigned char stereoscopicServiceType = descriptors[offset] & 0x7;
        if (stereoscopicServiceType == 2 || stereoscopicServiceType == 3)
        {
          isThreeDimensionalVideo = true;   // frame or service compatible
        }
      }
    }
    else if (tag == DESCRIPTOR_STEREOSCOPIC_VIDEO_INFO)   // ISO/IEC 13818 part 1
    {
      isThreeDimensionalVideo = true;   // service compatible
    }
    else if (tag == DESCRIPTOR_HEVC_VIDEO)    // ISO/IEC 13818 part 1
    {
      if (length >= 6 && offset + 5 < descriptorsLength)
      {
        // non_packed_constraint_flag
        isThreeDimensionalVideo |= ((descriptors[offset + 5] & 0x20) == 0);   // frame compatible
        // Technically the stream could be non-3D if the frame packing
        // arrangement (FPA) supplemental enhancement information (SEI) message
        // frame_packing_arrangement_cancel_flag is set. We have no access to
        // that information.
      }
    }
    else if (tag == DESCRIPTOR_ATSC_CAPTION_SERVICE)  // ATSC A/65 section 6.9.2
    {
      if (length > 1)
      {
        unsigned short pointer = offset;
        unsigned char numberOfServices = descriptors[pointer++] & 0x1f;
        for (unsigned char i = 0; i < numberOfServices && pointer + 5 < offset + length; i++)
        {
          unsigned long language = descriptors[pointer] | (descriptors[pointer + 1] << 8) | (descriptors[pointer + 2] << 16);
          pointer += 6;   // skip the bytes we don't care about

          if (language != 0)
          {
            vector<unsigned long>::const_iterator it = find(captionsLanguages.begin(),
                                                            captionsLanguages.end(),
                                                            language);
            if (it == captionsLanguages.end())
            {
              captionsLanguages.push_back(language);
            }
          }
        }
      }
    }
    else if (tag == DESCRIPTOR_SCTE_3D_MPEG2)   // SCTE 187 part 2 section 8.3
    {
      if (length >= 1 && offset < descriptorsLength)
      {
        // 3d_frame_packing_data_present
        isThreeDimensionalVideo |= ((descriptors[offset] & 0x80) != 0);   // frame compatible
      }
    }

    offset += length;
  }
}

void CGrabberPmt::CheckDescriptorsPidAudio(unsigned char* descriptors,
                                            unsigned short descriptorsLength,
                                            bool& hasCaDescriptor)
{
  hasCaDescriptor = false;
  if (descriptors == NULL)
  {
    return;
  }

  unsigned short offset = 0;
  while (offset + 1 < descriptorsLength)
  {
    unsigned char tag = descriptors[offset++];
    unsigned char length = descriptors[offset++];
    if (tag == DESCRIPTOR_CONDITIONAL_ACCESS)
    {
      hasCaDescriptor = true;
      break;
    }

    offset += length;
  }
}

void CGrabberPmt::CheckDescriptorsPidTeletext(unsigned char* descriptors,
                                              unsigned short descriptorsLength,
                                              vector<unsigned long>& subtitlesLanguages)
{
  if (descriptors == NULL)
  {
    return;
  }

  unsigned short offset = 0;
  while (offset + 1 < descriptorsLength)
  {
    unsigned char tag = descriptors[offset++];
    unsigned char length = descriptors[offset++];
    if (tag == DESCRIPTOR_DVB_TELETEXT || DESCRIPTOR_DVB_VBI_TELETEXT)
    {
      unsigned short pointer = offset;
      while (pointer + 4 < offset + length)
      {
        unsigned long language = descriptors[pointer] | (descriptors[pointer + 1] << 8) | (descriptors[pointer + 2] << 16);
        pointer += 3;
        unsigned char type = descriptors[pointer] >> 3;
        pointer += 2;

        if ((type == 2 || type == 5) && language != 0)  // teletext subtitles
        {
          vector<unsigned long>::const_iterator it = find(subtitlesLanguages.begin(),
                                                          subtitlesLanguages.end(),
                                                          language);
          if (it == subtitlesLanguages.end())
          {
            subtitlesLanguages.push_back(language);
          }
        }
      }
    }

    offset += length;
  }
}