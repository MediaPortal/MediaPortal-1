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
#include "..\shared\BasePmtParser.h"
#include <algorithm>
#include <cstddef>    // NULL
#include <cstring>    // memcpy(), memset()

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

//-----------------------------------------------------------------------------
// UTILITY FUNCTIONS
//-----------------------------------------------------------------------------
bool IsValidDigiCipher2Stream(unsigned char streamType)
{
  if (streamType == STREAM_TYPE_VIDEO_MPEG2_DCII ||
    streamType == STREAM_TYPE_AUDIO_AC3 ||
    streamType == STREAM_TYPE_AUDIO_E_AC3 ||
    streamType == STREAM_TYPE_SUBTITLES_SCTE)
  {
    return true;
  }
  return false;
}


//-----------------------------------------------------------------------------
// CLASS
//-----------------------------------------------------------------------------
CBasePmtParser::CBasePmtParser()
{
  Reset();
  m_programNumber = -1;
}

CBasePmtParser::~CBasePmtParser(void)
{
}

void CBasePmtParser::Reset()
{
  m_isFound = false;
  m_pidInfo.Reset();
  CSectionDecoder::Reset();
}

void CBasePmtParser::SetFilter(unsigned short pid, unsigned short programNumber)
{
  SetPid(pid);
  m_programNumber = programNumber;
}

void CBasePmtParser::GetFilter(unsigned short& pid, unsigned short& programNumber)
{
  pid = GetPid();
  programNumber = m_programNumber;
}

bool CBasePmtParser::IsReady()
{
  return m_isFound;
}

void CBasePmtParser::OnTsPacket(unsigned char* tsPacket)
{
  if (m_isFound)
  {
    return;
  }
  CSectionDecoder::OnTsPacket(tsPacket);
}

bool CBasePmtParser::DecodePmtSection(const CSection& section)
{
  try
  {
    if (
      section.table_id != 0x02 ||         // 0x02 = standard PMT table ID
      !section.SectionSyntaxIndicator ||
      section.PrivateIndicator ||
      !section.CurrentNextIndicator       // Details do not apply yet...
    )
    {
      return false;
    }

    if (section.section_length > 1021 || section.section_length < 13)
    {
      LogDebug(L"PMT: invalid section, length = %d", section.section_length);
      return false;
    }

    const unsigned char* data = section.Data;
    unsigned short pcrPid = ((data[8] & 0x1f) << 8) | data[9];
    unsigned short programInfoLength = ((data[10] & 0xf) << 8) | data[11];

    unsigned short pointer = 12;
    unsigned short endOfProgramInfo = pointer + programInfoLength;
    unsigned short endOfSection = section.section_length - 1;
    //LogDebug(L"PMT: program number = %d, version number = %d, section number = %d, last section number = %d, PCR PID = %hu, program info length = %hu, section length = %d",
    //          section.table_id_extension, section.version_number,
    //          section.SectionNumber, section.LastSectionNumber, pcrPid,
    //          programInfoLength, section.section_length);
    if (endOfProgramInfo > endOfSection)
    {
      LogDebug(L"PMT: invalid section, program info length = %hu, pointer = %hu, section length = %d",
                programInfoLength, pointer, section.section_length);
      return false;
    }

    m_pidInfo.Reset();
    m_pidInfo.PmtPid = GetPid();
    m_pidInfo.PmtVersion = section.version_number;
    m_pidInfo.ProgramNumber = section.table_id_extension;
    m_pidInfo.PcrPid = pcrPid;
    m_pidInfo.DescriptorsLength = programInfoLength;
    if (programInfoLength != 0)
    {
      m_pidInfo.Descriptors = new unsigned char[programInfoLength];
      if (m_pidInfo.Descriptors == NULL)
      {
        LogDebug(L"PMT: failed to allocate %hu bytes for program descriptors",
                  programInfoLength);
        return false;
      }
      memcpy(m_pidInfo.Descriptors, &data[pointer], programInfoLength);
    }

    bool isScteTs = false;
    while (pointer + 1 < endOfProgramInfo)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"PMT: program descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfProgramInfo)
      {
        LogDebug(L"PMT: invalid section, program descriptor length = %hhu, pointer = %hu, end of program info = %hu, section length = %d",
                  length, pointer, endOfProgramInfo, section.section_length);
        return false;
      }

      if (tag == DESCRIPTOR_REGISTRATION)
      {
        if (length >= 4 &&
          data[pointer] == 'S' &&
          data[pointer + 1] == 'C' &&
          data[pointer + 2] == 'T' &&
          data[pointer + 3] == 'E')
        {
          isScteTs = true;
        }
      }
      pointer += length;
    }
    pointer = endOfProgramInfo;

    bool isNotDc2Ts = false;
    bool foundDc2VideoStream = false;
    while (pointer + 4 < endOfSection)
    {
      unsigned char streamType = data[pointer++];
      unsigned short elementaryPid = ((data[pointer] & 0x1f) << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short esInfoLength = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short endOfEsInfo = pointer + esInfoLength;
      //LogDebug(L"PMT: stream type = 0x%hhx, elementary PID = %hu, elementary stream info length = %hu, end of elementary stream info = %hu",
      //          streamType, elementaryPid, esInfoLength, endOfEsInfo);
      if (endOfEsInfo > endOfSection)
      {
        LogDebug(L"PMT: invalid section, elementary stream info length = %hu, pointer = %hu, section length = %d",
                  esInfoLength, pointer, section.section_length);
        return false;
      }

      BasePid* basePid = NULL;
      if (
        CPidTable::IsVideoStream(streamType) ||
        (streamType == STREAM_TYPE_VIDEO_MPEG2_DCII && !isNotDc2Ts)
      )
      {
        VideoPid* pid = new VideoPid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate video PID %hu", elementaryPid);
          return false;
        }
        // Only allow DigiCipher II video detection until we see a clear
        // indication that this is not a DC II transport stream.
        if (streamType == STREAM_TYPE_VIDEO_MPEG2_DCII)
        {
          pid->LogicalStreamType = STREAM_TYPE_VIDEO_MPEG2;
          foundDc2VideoStream = true;
        }
        else
        {
          pid->LogicalStreamType = streamType;
        }
        m_pidInfo.VideoPids.push_back(pid);
        basePid = pid;
        //LogDebug(L"PMT:  video PID %hu, stream type = 0x%hhx",
        //          elementaryPid, streamType);
      }
      else if (CPidTable::IsAudioStream(streamType))
      {          
        AudioPid* pid = new AudioPid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate audio PID %hu", elementaryPid);
          return false;
        }
        pid->LogicalStreamType = streamType;
        m_pidInfo.AudioPids.push_back(pid);
        basePid = pid;
        //LogDebug(L"PMT:  audio PID %hu, stream type = 0x%hhx",
        //          elementaryPid, streamType);
      }
      else if (streamType == STREAM_TYPE_SUBTITLES_SCTE || streamType == STREAM_TYPE_TEXT_MPEG4)
      {
        SubtitlePid* pid = new SubtitlePid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate subtitle PID %hu", elementaryPid);
          return false;
        }
        m_pidInfo.SubtitlePids.push_back(pid);
        basePid = pid;
        //LogDebug(L"PMT:  subtitle PID %hu, stream type = 0x%hhx",
        //          elementaryPid, streamType);
      }

      if (!isNotDc2Ts && !IsValidDigiCipher2Stream(streamType))
      {
        isNotDc2Ts = true;
        if (foundDc2VideoStream)
        {
          // Fix false positive DC II video detections.
          std::vector<VideoPid*>::iterator vPidIt = m_pidInfo.VideoPids.begin();
          while (vPidIt != m_pidInfo.VideoPids.end())
          {
            if ((*vPidIt)->StreamType == STREAM_TYPE_VIDEO_MPEG2_DCII)
            {
              delete *vPidIt;
              *vPidIt = NULL;
              vPidIt = m_pidInfo.VideoPids.erase(vPidIt);
              continue;
            }
            vPidIt++;
          }
        }
      }

      bool isVbiStream = false;
      bool isTeletextStream = false;
      unsigned char lang[7];
      memset(lang, 0, sizeof(lang));
      unsigned short startOfEsInfo = pointer;
      while (pointer + 1 < endOfEsInfo)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        unsigned short endOfDescriptor = pointer + length;
        //LogDebug(L"PMT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (endOfDescriptor > endOfEsInfo)
        {
          LogDebug(L"PMT: invalid descriptor length = %hhu, pointer = %hu, end of elementary stream info = %hu, section length = %d",
                    length, pointer, endOfEsInfo, section.section_length);
          return false;
        }

        if (tag == DESCRIPTOR_REGISTRATION)
        {
          if (
            length >= 4 &&
            data[pointer] == 'V' &&
            data[pointer + 1] == 'C' &&
            data[pointer + 2] == '-' &&
            data[pointer + 3] == '1'
          )
          {
            // This is a VC-1 video stream in a DVB TS.
            VideoPid* pid = new VideoPid(elementaryPid, streamType);
            if (pid == NULL)
            {
              LogDebug(L"PMT: failed to allocate video PID %hu", elementaryPid);
              return false;
            }
            pid->LogicalStreamType = STREAM_TYPE_VIDEO_VC1;
            m_pidInfo.VideoPids.push_back(pid);
            basePid = pid;
            //LogDebug(L"PMT:  video PID %hu, stream type = 0x%hhx",
            //          elementaryPid, streamType);
          }
        }
        else if (
          tag == DESCRIPTOR_DVB_AC3 ||
          tag == DESCRIPTOR_DVB_E_AC3 ||
          (tag == DESCRIPTOR_DVB_DTS && !isScteTs) ||
          (
            tag == DESCRIPTOR_DVB_EXTENSION &&
            length >= 1 &&
            data[pointer] == DESCRIPTOR_DVB_X_DTS_HD
          ) ||
          (tag == DESCRIPTOR_SCTE_DTS_HD && isScteTs)
        )
        {
          AudioPid* pid = new AudioPid(elementaryPid, streamType);
          if (pid == NULL)
          {
            LogDebug(L"PMT: failed to allocate audio PID %hu", elementaryPid);
            return false;
          }
          if (tag == DESCRIPTOR_DVB_AC3)
          {
            pid->LogicalStreamType = STREAM_TYPE_AUDIO_AC3;
          }
          else if (tag == DESCRIPTOR_DVB_E_AC3)
          {
            pid->LogicalStreamType = STREAM_TYPE_AUDIO_E_AC3;
          }
          else if (tag == DESCRIPTOR_DVB_DTS && !isScteTs)
          {
            pid->LogicalStreamType = STREAM_TYPE_AUDIO_DTS;
          }
          else
          {
            pid->LogicalStreamType = STREAM_TYPE_AUDIO_DTS_HD;
          }
          if (lang[0] != 0)
          {
            memcpy(pid->Lang, lang, min(sizeof(pid->Lang), sizeof(lang)) - 1);
          }

          m_pidInfo.AudioPids.push_back(pid);
          basePid = pid;
          //LogDebug(L"PMT:  audio PID %hu, stream type = 0x%hhx",
          //          elementaryPid, streamType);
        }
        else if (tag == DESCRIPTOR_ATSC_AC3 && basePid != NULL && length >= 10)
        {
          AudioPid* audioPid = dynamic_cast<AudioPid*>(basePid);
          if (audioPid != NULL)
          {
            unsigned char textlenOffset = 5;
            unsigned char languageFlagsOffset = 6;
            unsigned char numChannels = (data[pointer + 2] & 0xf) >> 1;
            if (numChannels == 0)
            {
              textlenOffset++;
              languageFlagsOffset++;
            }
            unsigned char textlen = data[pointer + textlenOffset] >> 1;
            languageFlagsOffset += textlenOffset;
            if (languageFlagsOffset + 3 < length)
            {
              bool languageFlag = (data[pointer + languageFlagsOffset] & 0x80) != 0;
              bool languageFlag2 = (data[pointer + languageFlagsOffset] & 0x40) != 0; // for dual-mono
              if (languageFlag || languageFlag2)
              {
                memcpy(audioPid->Lang,
                        &data[languageFlagsOffset + 1],
                        min((unsigned char)sizeof(audioPid->Lang), (unsigned char)4) - 1);
              }
              if (languageFlag && languageFlag2 && languageFlagsOffset + 6 < length)
              {
                memcpy(audioPid->Lang,
                        &data[languageFlagsOffset + 1],
                        min((unsigned char)sizeof(audioPid->Lang), (unsigned char)7) - 1);
              }
              //LogDebug(L"PMT:    ATSC AC3 language = %S",
              //          (char*)audioPid->Lang);
            }
          }
        }
        else if (tag == DESCRIPTOR_ATSC_E_AC3 && basePid != NULL && length >= 6)
        {
          AudioPid* audioPid = dynamic_cast<AudioPid*>(basePid);
          if (audioPid != NULL)
          {
            bool languageFlag = (data[pointer + 2] & 0x80) != 0;
            bool languageFlag2 = (data[pointer + 2] & 0x40) != 0;
            // Perhaps we could also take the 3 sub-stream languages...
            if (languageFlag || languageFlag2)
            {
              unsigned char languageOffset = 3;
              bool mainIdFlag = (data[pointer] & 0x20) != 0;
              bool asvcFlag = (data[pointer] & 0x10) != 0;
              bool mixInfoExistsFlag = (data[pointer] & 8) != 0;
              bool subStream1Flag = (data[pointer] & 4) != 0;
              bool subStream2Flag = (data[pointer] & 2) != 0;
              bool subStream3Flag = (data[pointer] & 1) != 0;
              if (mainIdFlag)
              {
                languageOffset++;
              }
              if (asvcFlag)
              {
                languageOffset++;
              }
              if (mixInfoExistsFlag)
              {
                languageOffset++;
              }
              if (subStream1Flag)
              {
                languageOffset++;
              }
              if (subStream2Flag)
              {
                languageOffset++;
              }
              if (subStream3Flag)
              {
                languageOffset++;
              }

              if (languageOffset + 2 < length)
              {
                memcpy(audioPid->Lang,
                        &data[pointer + languageOffset],
                        min((unsigned char)sizeof(audioPid->Lang), (unsigned char)4) - 1);
              }
              if (languageFlag && languageFlag2 && languageOffset + 5 < length)
              {
                memcpy(audioPid->Lang,
                        &data[pointer + languageOffset],
                        min((unsigned char)sizeof(audioPid->Lang), (unsigned char)7) - 1);
              }
              //LogDebug(L"PMT:    ATSC E-AC3 language = %S",
              //          (char*)audioPid->Lang);
            }
          }
        }
        // audio or subtitle language
        else if (tag == DESCRIPTOR_ISO_639_LANG)
        {
          // Copy the language to the corresponding audio or subtitle PID. In
          // cases where we rely on a descriptor to determine the stream type
          // we might not yet know the stream type, ergo we might not have a
          // PID yet. In those cases copy the language to our local variable.
          // We'll copy the value from the local variable to the PID if/when we
          // create one.
          unsigned char* pidLang = NULL;
          if (basePid != NULL)
          {
            AudioPid* audioPid = dynamic_cast<AudioPid*>(basePid);
            if (audioPid != NULL)
            {
              pidLang = (unsigned char*)&(audioPid->Lang);
            }
            else
            {
              SubtitlePid* subtitlePid = dynamic_cast<SubtitlePid*>(basePid);
              if (subtitlePid != NULL)
              {
                pidLang = (unsigned char*)&(subtitlePid->Lang);
              }
            }
          }
          if (pidLang == NULL)
          {
            pidLang = (unsigned char*)&lang;
          }
          unsigned char pidLangLength = sizeof(pidLang);

          // There may be more than one language. This makes sense for dual
          // mono audio streams.
          unsigned char pidLangOffset = 0;
          while (pointer + 3 < endOfDescriptor && pidLangOffset + 3 < pidLangLength)
          {
            memcpy(&pidLang[pidLangOffset], &data[pointer], 3);
            pidLangOffset += 3;
            pidLang[pidLangOffset] = 0;   // NULL terminate
            pointer += 4;                 // skips the audio_type field
          }
          //LogDebug(L"PMT:    ISO 639 language = %S", (char*)pidLang);
        }
        else if (streamType == STREAM_TYPE_PES_PRIVATE_DATA && tag == DESCRIPTOR_DVB_VBI_DATA)
        {
          isVbiStream = true;
        }
        else if (
          !isTeletextStream &&
          streamType == STREAM_TYPE_PES_PRIVATE_DATA &&
          (tag == DESCRIPTOR_DVB_TELETEXT || tag == DESCRIPTOR_DVB_VBI_TELETEXT)
        )
        {
          isTeletextStream = true;
          TeletextPid* pid = new TeletextPid(elementaryPid, streamType);
          if (pid == NULL)
          {
            LogDebug(L"PMT: failed to allocate teletext PID %hu", elementaryPid);
            return false;
          }
          m_pidInfo.TeletextPids.push_back(pid);
          basePid = pid;
          //LogDebug(L"PMT:  teletext PID %hu, stream type = 0x%hhx",
          //          elementaryPid, streamType);
        }
        else if (streamType == STREAM_TYPE_PES_PRIVATE_DATA && tag == DESCRIPTOR_DVB_SUBTITLING)
        {
          SubtitlePid* pid = new SubtitlePid(elementaryPid, streamType);
          if (pid == NULL)
          {
            LogDebug(L"PMT: failed to allocate subtitle PID %hu", elementaryPid);
            return false;
          }
          // Populate the PID language if we can.
          if (length >= 3)
          {
            memcpy(pid->Lang, &data[pointer], min((unsigned char)sizeof(pid->Lang), (unsigned char)4) - 1);
          }
          else if (lang[0] != 0)
          {
            memcpy(pid->Lang, lang, min(sizeof(pid->Lang), sizeof(lang)));
          }
          m_pidInfo.SubtitlePids.push_back(pid);
          basePid = pid;
          //LogDebug(L"PMT:  subtitle PID %hu, stream type = 0x%hhx, language = %S",
          //          elementaryPid, streamType, (char*)pid->Lang);
        }

        pointer = endOfDescriptor;
      }
      if (pointer != endOfEsInfo)
      {
        LogDebug(L"PMT: section parsing error, pointer = %hu, end of elementary stream info = %hu, PID = %hu",
                  pointer, endOfEsInfo, elementaryPid);
        return false;
      }

      if (isVbiStream && !isTeletextStream)
      {
        VbiPid* pid = new VbiPid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate VBI PID %hu", elementaryPid);
          return false;
        }
        m_pidInfo.VbiPids.push_back(pid);
        basePid = pid;
        //LogDebug(L"PMT:  VBI PID %hu, stream type = 0x%hhx",
        //          elementaryPid, streamType);
      }

      if (basePid == NULL)
      {
        OtherPid* pid = new OtherPid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate other PID %hu", elementaryPid);
          return false;
        }
        m_pidInfo.OtherPids.push_back(pid);
        basePid = pid;
        //LogDebug(L"PMT:  other PID %hu, stream type = 0x%hhx",
        //          elementaryPid, streamType);
      }

      // Common code for all PIDs.
      if (esInfoLength > 0)
      {
        basePid->DescriptorsLength = esInfoLength;
        basePid->Descriptors = new unsigned char[esInfoLength];
        if (basePid->Descriptors == NULL)
        {
          LogDebug(L"PMT: failed to allocate %hu bytes for PID %hu's elementary stream descriptors",
                    esInfoLength, basePid->Pid);
          return false;
        }
        memcpy(basePid->Descriptors, &data[startOfEsInfo], esInfoLength);
      }
    }
    if (pointer != endOfSection)
    {
      LogDebug(L"PMT: section parsing error, pointer = %hu, end of section = %hu",
                pointer, endOfSection);
      return false;
    }
  } 
  catch (...)
  { 
    LogDebug(L"PMT: unhandled exception in DecodePmtSection()");
    return false;
  }
  return true;
}

CPidTable& CBasePmtParser::GetPidInfo()
{
  return m_pidInfo;
}

void CBasePmtParser::OnNewSection(CSection& section)
{ 
  // Can be overriden by derived classes.
  DecodePmtSection(section);
}