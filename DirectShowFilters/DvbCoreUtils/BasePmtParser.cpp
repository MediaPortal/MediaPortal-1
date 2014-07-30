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


extern void LogDebug(const wchar_t* fmt, ...);
extern bool IsVideoStream(byte streamType);
extern bool IsAudioStream(byte streamType);


//-----------------------------------------------------------------------------
// UTILITY FUNCTIONS
//-----------------------------------------------------------------------------
bool IsValidDigiCipher2Stream(byte streamType)
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

void CBasePmtParser::OnTsPacket(byte* tsPacket)
{
  if (m_isFound)
  {
    return;
  }
  CSectionDecoder::OnTsPacket(tsPacket);
}

bool CBasePmtParser::DecodePmtSection(CSection& section)
{   
  // 0x02 = standard PMT table ID
  if (section.table_id != 0x02)
  {
    return false;
  }

  try
  {
    if (section.section_length > 1021 || section.section_length < 13)
    {
      LogDebug(L"PMT: invalid section length = %d", section.section_length);
      return false;
    }
    if (section.current_next_indicator == 0)
    {
      // Details do not apply yet...
      return false;
    }

    byte* data = section.Data;
    unsigned short pcrPid = ((data[8] & 0x1f) << 8) + data[9];
    unsigned short programInfoLength = ((data[10] & 0xf) << 8) + data[11];

    unsigned short pointer = 12;
    unsigned short endOfProgramInfo = pointer + programInfoLength;
    unsigned short endOfSection = section.section_length - 1;
    //LogDebug(L"PMT: program number = %d, section number = %d, version number = %d, last section number = %d, PCR PID = %d, program info length = %d, end of program info = %d, section length = %d, end of section = %d",
    //          section.table_id_extension, section.section_number, section.version_number, section.last_section_number, pcrPid, programInfoLength, endOfProgramInfo, section.section_length, endOfSection);
    if (endOfProgramInfo > endOfSection)
    {
      LogDebug(L"PMT: invalid program info length = %d, pointer = %d, end of program info = %d, end of section = %d, section length = %d", programInfoLength, pointer, endOfProgramInfo, endOfSection, section.section_length);
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
      m_pidInfo.Descriptors = new byte[programInfoLength];
      if (m_pidInfo.Descriptors == NULL)
      {
        LogDebug(L"PMT: failed to allocate %d bytes for program descriptors", programInfoLength);
        return false;
      }
      memcpy(m_pidInfo.Descriptors, &data[pointer], programInfoLength);
    }

    bool isScteTs = false;
    while (pointer + 1 < endOfProgramInfo)
    {
      byte tag = data[pointer++];
      byte length = data[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"PMT: program descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
      if (endOfDescriptor > endOfProgramInfo)
      {
        LogDebug(L"PMT: invalid program descriptor length = %d, pointer = %d, end of descriptor = %d, end of program info = %d, section length = %d", length, pointer, endOfDescriptor, endOfProgramInfo, section.section_length);
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
      byte streamType = data[pointer++];
      unsigned short elementaryPid = ((data[pointer] & 0x1f) << 8) + data[pointer + 1];
      pointer += 2;
      unsigned short esInfoLength = ((data[pointer] & 0xf) << 8) + data[pointer + 1];
      pointer += 2;
      unsigned short endOfEsInfo = pointer + esInfoLength;
      //LogDebug(L"PMT: stream type = 0x%x, elementary PID = %d, elementary stream info length = %d, end of elementary stream info = %d", streamType, elementaryPid, esInfoLength, endOfEsInfo);
      if (endOfEsInfo > endOfSection)
      {
        LogDebug(L"PMT: invalid elementary stream info length = %d, pointer = %d, end of elementary stream info = %d, end of section = %d, section length = %d", esInfoLength, pointer, endOfEsInfo, endOfSection, section.section_length);
        return false;
      }

      BasePid* basePid = NULL;
      if (IsVideoStream(streamType) || (streamType == STREAM_TYPE_VIDEO_MPEG2_DCII && !isNotDc2Ts))
      {
        VideoPid* pid = new VideoPid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate video PID");
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
        //LogDebug(L"PMT:  video PID %d, stream type = 0x%x", elementaryPid, streamType);
      }
      else if (IsAudioStream(streamType))
      {          
        AudioPid* pid = new AudioPid(elementaryPid, streamType);
        if (pid == NULL)
        {
          LogDebug(L"PMT: failed to allocate audio PID");
          return false;
        }
        pid->LogicalStreamType = streamType;
        m_pidInfo.AudioPids.push_back(pid);
        basePid = pid;
        //LogDebug(L"PMT:  audio PID %d, stream type = 0x%x", elementaryPid, streamType);
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
              m_pidInfo.VideoPids.erase(vPidIt++);
              continue;
            }
            vPidIt++;
          }
        }
      }

      char lang[7];
      ZeroMemory(lang, sizeof(lang));
      int startOfEsInfo = pointer;
      while (pointer + 1 < endOfEsInfo)
      {
        int tag = data[pointer++];
        int length = data[pointer++];
        int endOfDescriptor = pointer + length;
        //LogDebug(L"PMT: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
        if (endOfDescriptor > endOfEsInfo)
        {
          LogDebug(L"PMT: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of elementary stream info = %d, section length = %d", length, pointer, endOfDescriptor, endOfEsInfo, section.section_length);
          return false;
        }

        if (tag == DESCRIPTOR_REGISTRATION)
        {
          if (length >= 4 &&
            data[pointer] == 'V' &&
            data[pointer + 1] == 'C' &&
            data[pointer + 2] == '-' &&
            data[pointer + 3] == '1')
          {
            // This is a VC-1 video stream in a DVB TS.
            VideoPid* pid = new VideoPid(elementaryPid, streamType);
            if (pid == NULL)
            {
              LogDebug(L"PMT: failed to allocate video PID");
              return false;
            }
            pid->LogicalStreamType = STREAM_TYPE_VIDEO_VC1;
            m_pidInfo.VideoPids.push_back(pid);
            basePid = pid;
            //LogDebug(L"PMT:  video PID %d, stream type = 0x%x", elementaryPid, streamType);
          }
        }
        else if (tag == DESCRIPTOR_DVB_AC3 ||
          tag == DESCRIPTOR_DVB_E_AC3 ||
          (tag == DESCRIPTOR_DVB_DTS && !isScteTs) ||
          (tag == DESCRIPTOR_DVB_EXTENSION && length >= 1 && data[pointer] == DESCRIPTOR_DVB_X_DTS_HD) ||
          (tag == DESCRIPTOR_SCTE_DTS_HD && isScteTs))
        {
          AudioPid* pid = new AudioPid(elementaryPid, streamType);
          if (pid == NULL)
          {
            LogDebug(L"PMT: failed to allocate audio PID");
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
            memcpy(pid->Lang, lang, sizeof(pid->Lang));
          }

          m_pidInfo.AudioPids.push_back(pid);
          basePid = pid;
          //LogDebug(L"PMT:  audio PID %d, stream type = 0x%x", elementaryPid, streamType);
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
          BYTE* pidLang = NULL;
          for (unsigned short i = 0; i < m_pidInfo.AudioPids.size(); i++)
          {
            if (m_pidInfo.AudioPids[i]->Pid == elementaryPid)
            {
              pidLang = (BYTE*)&m_pidInfo.AudioPids[i]->Lang;
              break;
            }
          }
          if (pidLang == NULL)
          {
            for (unsigned short i = 0; i < m_pidInfo.SubtitlePids.size(); i++)
            {
              if (m_pidInfo.SubtitlePids[i]->Pid == elementaryPid)
              {
                pidLang = (BYTE*)&m_pidInfo.SubtitlePids[i]->Lang;
                break;
              }
            }
          }
          if (pidLang == NULL)
          {
            pidLang = (BYTE*)&lang;
          }
          int pidLangLength = sizeof(pidLang);

          // There may be more than one language. This makes sense for dual
          // mono audio streams.
          byte pidLangOffset = 0;
          while (pointer + 3 < endOfDescriptor && pidLangOffset + 3 < pidLangLength)
          {
            memcpy(&pidLang[pidLangOffset], &data[pointer], 3);
            pidLangOffset += 3;
            pidLang[pidLangOffset] = 0;   // NULL terminate
            pointer += 4;
          }
        }
        else if (tag == DESCRIPTOR_DVB_TELETEXT)
        {
          TeletextPid* pid = new TeletextPid(elementaryPid, streamType);
          if (pid == NULL)
          {
            LogDebug(L"PMT: failed to allocate teletext PID");
            return false;
          }
          // Store the page details.
          while (pointer + 4 < endOfDescriptor)
          {
            TeletextServiceInfo info;
            info.Lang[0] = data[pointer++];
            info.Lang[1] = data[pointer++];
            info.Lang[2] = data[pointer++];
            info.Lang[3] = 0;
            info.Type = data[pointer] >> 3;
            if (info.Type == 2 || info.Type == 5) // subtitle page or subtitle page for hearing impaired
            {
              info.Page = (data[pointer++] & 0x7) * 100;
              info.Page += (data[pointer] >> 4) * 10;
              info.Page += (data[pointer++] & 0xf);
              if (!pid->HasTeletextPageInfo(info.Page))
              {
                pid->Services.push_back(info);
              }
            }
            else
            {
              pointer += 2;
            }
          }
          m_pidInfo.TeletextPids.push_back(pid);
          basePid = pid;
          //LogDebug(L"PMT:  teletext PID %d, stream type = 0x%x", elementaryPid, streamType);
        }
        else if (streamType == STREAM_TYPE_SUBTITLES_SCTE ||
          (streamType == STREAM_TYPE_PES_PRIVATE_DATA && tag == DESCRIPTOR_DVB_SUBTITLING) ||
          streamType == STREAM_TYPE_TEXT_MPEG4)
        {
          SubtitlePid* pid = new SubtitlePid(elementaryPid, streamType);
          if (pid == NULL)
          {
            LogDebug(L"PMT: failed to allocate subtitle PID");
            return false;
          }
          // Populate the PID language if we can.
          if (tag == DESCRIPTOR_DVB_SUBTITLING && length > 3)
          {
            memcpy(pid->Lang, &data[pointer], 3);
            pid->Lang[3] = 0;  // NULL terminate
          }
          else if (lang[0] != 0)
          {
            memcpy(pid->Lang, lang, sizeof(pid->Lang));
          }
          m_pidInfo.SubtitlePids.push_back(pid);
          basePid = pid;
          //LogDebug(L"PMT:  subtitle PID %d, stream type = 0x%x", elementaryPid, streamType);
        }

        pointer = endOfDescriptor;
      }
      if (pointer != endOfEsInfo)
      {
        LogDebug(L"PMT: section parsing error 1");
        return false;
      }

      // Common code for all PIDs.
      if (basePid != NULL && esInfoLength > 0)
      {
        basePid->DescriptorsLength = esInfoLength;
        basePid->Descriptors = new byte[esInfoLength];
        if (basePid->Descriptors == NULL)
        {
          LogDebug(L"PMT: failed to allocate %d bytes for elementary stream descriptors", esInfoLength);
          return false;
        }
        memcpy(basePid->Descriptors, &data[startOfEsInfo], esInfoLength);
      }
    }
    if (pointer != endOfSection)
    {
      LogDebug(L"PMT: section parsing error 2");
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