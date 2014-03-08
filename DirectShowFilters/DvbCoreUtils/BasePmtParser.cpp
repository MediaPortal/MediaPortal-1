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


void LogDebug(const char* fmt, ...);
extern bool DisableCRCCheck();

CBasePmtParser::CBasePmtParser()
{
  m_isFound = false;
  m_programNumber = -1;
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
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

void CBasePmtParser::SetFilter(int pid, int programNumber)
{
  SetPid(pid);
  m_programNumber = programNumber;
}

void CBasePmtParser::GetFilter(int& pid, int& programNumber)
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

bool CBasePmtParser::DecodePmtSection(CSection& sections)
{   
  // 0x02 = standard PMT table ID
  if (sections.table_id != 0x02)
  {
    return false;
  }
  byte* section = sections.Data;

  try
  {
    int sectionSyntaxIndicator = section[1] & 0x80;
    int sectionLength = ((section[1] & 0xf) << 8) + section[2];
    if (sectionLength > 1021 || sectionLength < 13)
    {
      LogDebug("PMT: invalid section length = %d", sectionLength);
      return false;
    }
    int programNumber = (section[3] << 8) + section[4];
    int versionNumber = (section[5] >> 1) & 0x1f;
    int currentNextIndicator = section[5] & 1;
    if (currentNextIndicator == 0)
    {
      // Details do not apply yet...
      return false;
    }
    int sectionNumber = section[6];
    int lastSectionNumber = section[7];
    int pcrPid = ((section[8] & 0x1f) << 8) + section[9];
    int programInfoLength = ((section[10] & 0xf) << 8) + section[11];

    int pointer = 12;
    int endOfProgramInfo = pointer + programInfoLength;
    int endOfSection = sectionLength - 1;
    //LogDebug("PMT: program number = %d, section number = %d, version number = %d, last section number = %d, program info length = %d, end of program info = %d, section length = %d, end of section = %d",
    //          programNumber, sectionNumber, versionNumber, lastSectionNumber, programInfoLength, endOfProgramInfo, sectionLength, endOfSection);
    if (endOfProgramInfo > endOfSection)
    {
      LogDebug("PMT: invalid program info length = %d, pointer = %d, end of program info = %d, end of section = %d, section length = %d", programInfoLength, pointer, endOfProgramInfo, endOfSection, sectionLength);
      return false;
    }
    while (pointer + 1 < endOfProgramInfo)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      int endOfDescriptor = pointer + length;
      //LogDebug("PMT: program descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
      if (endOfDescriptor > endOfProgramInfo)
      {
        LogDebug("PMT: invalid program descriptor length = %d, pointer = %d, end of descriptor = %d, end of program info = %d, section length = %d", length, pointer, endOfDescriptor, endOfProgramInfo, sectionLength);
        return false;
      }
      if (tag == DESCRIPTOR_CONDITIONAL_ACCESS)
      {
        m_pidInfo.ConditionalAccessDescriptorCount++;
      }
      pointer += length;
    }
    pointer = endOfProgramInfo;

    m_pidInfo.Reset();
    m_pidInfo.PmtPid = GetPid();
    m_pidInfo.ServiceId = programNumber;
    m_pidInfo.PcrPid = pcrPid;
    bool seenMpegVideoEs = false;
    while (pointer + 4 < endOfSection)
    {
      int streamType = section[pointer++];
      int elementaryPid = ((section[pointer] & 0x1f) << 8) + section[pointer + 1];
      pointer += 2;
      int esInfoLength = ((section[pointer] & 0xf) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfEsInfo = pointer + esInfoLength;
      //LogDebug("PMT: stream type = 0x%x, elementary PID = %d, elementary stream info length = %d, end of elementary stream info = %d", streamType, elementaryPid, esInfoLength, endOfEsInfo);
      if (endOfEsInfo > endOfSection)
      {
        LogDebug("PMT: invalid elementary stream info length = %d, pointer = %d, end of elementary stream info = %d, end of section = %d, section length = %d", esInfoLength, pointer, endOfEsInfo, endOfSection, sectionLength);
        return false;
      }

      if (streamType == STREAM_TYPE_VIDEO_MPEG1 ||
        streamType == STREAM_TYPE_VIDEO_MPEG2 ||
        streamType == STREAM_TYPE_VIDEO_MPEG4 ||
        streamType == STREAM_TYPE_VIDEO_H264 ||
        (streamType == STREAM_TYPE_VIDEO_MPEG2_DCII && !seenMpegVideoEs))
      {
        VideoPid pid;
        pid.Pid = elementaryPid;
        pid.StreamType = streamType;
        if (streamType == STREAM_TYPE_VIDEO_MPEG2_DCII)
        {
          pid.StreamType = STREAM_TYPE_VIDEO_MPEG2;
        }
        else if (!seenMpegVideoEs)
        {
          // Attempt to avoid false-positive DC II detection. Only allow DC II
          // detection in the case where ISO video stream types are not seen.
          // This may fail for audio-only programs.
          m_pidInfo.VideoPids.clear();
          seenMpegVideoEs = true;
        }
        m_pidInfo.VideoPids.push_back(pid);
        //LogDebug("PMT:  video PID %d, stream type = 0x%x", elementaryPid, streamType);
      }
      else if (streamType == STREAM_TYPE_AUDIO_MPEG1 ||
        streamType == STREAM_TYPE_AUDIO_MPEG2 ||
        streamType == STREAM_TYPE_AUDIO_AAC ||
        streamType == STREAM_TYPE_AUDIO_LATM_AAC ||
        streamType == STREAM_TYPE_AUDIO_AC3 ||
        streamType == STREAM_TYPE_AUDIO_E_AC3 ||
        streamType == STREAM_TYPE_AUDIO_E_AC3_ATSC)
      {          
        AudioPid pid;
        pid.Pid = elementaryPid;
        if (streamType == STREAM_TYPE_AUDIO_E_AC3_ATSC)
        {
          pid.StreamType = STREAM_TYPE_AUDIO_E_AC3;
        }
        else
        {
          pid.StreamType = streamType;
        }
        m_pidInfo.AudioPids.push_back(pid);
        //LogDebug("PMT:  audio PID %d, stream type = 0x%x", elementaryPid, streamType);
      }

      char lang[7];
      while (pointer + 1 < endOfEsInfo)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        ZeroMemory(lang, sizeof(lang));
        int endOfDescriptor = pointer + length;
        //LogDebug("PMT: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
        if (endOfDescriptor > endOfEsInfo)
        {
          LogDebug("PMT: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of elementary stream info = %d, section length = %d", length, pointer, endOfDescriptor, endOfEsInfo, sectionLength);
          return false;
        }

        if (tag == DESCRIPTOR_AC3_DVB || tag == DESCRIPTOR_E_AC3_DVB)
        {                
          AudioPid pid;
          pid.Pid = elementaryPid;
          if (tag == DESCRIPTOR_AC3_DVB)
          {
            pid.StreamType = STREAM_TYPE_AUDIO_AC3;
            //LogDebug("PMT:  AC-3 audio PID %d, stream type = 0x%x", elementaryPid, streamType);
          }
          else
          {
            pid.StreamType = STREAM_TYPE_AUDIO_E_AC3;
            //LogDebug("PMT:  enhanced AC-3 audio PID %d, stream type = 0x%x", elementaryPid, streamType);
          }
          if (lang[0] != 0)
          {
            memcpy(pid.Lang, lang, sizeof(pid.Lang));
          }

          m_pidInfo.AudioPids.push_back(pid);
        }
        // audio or subtitle language
        else if (tag == DESCRIPTOR_ISO_639_LANG)
        {          
          BYTE* pidLang = NULL;
          int pidLangLength = 6;
          for (unsigned int i = 0; i < m_pidInfo.AudioPids.size(); i++)
          {
            if (m_pidInfo.AudioPids[i].Pid == elementaryPid)
            {
              pidLang = (BYTE*)&m_pidInfo.AudioPids[i].Lang;
              break;
            }
          }
          if (pidLang == NULL)
          {
            for (unsigned int i = 0; i < m_pidInfo.SubtitlePids.size(); i++)
            {
              if (m_pidInfo.SubtitlePids[i].Pid == elementaryPid)
              {
                pidLang = (BYTE*)&m_pidInfo.SubtitlePids[i].Lang;
                pidLangLength = 3;  // we only support one language per subtitle stream
                break;
              }
            }
          }
          if (pidLang == NULL)
          {
            pidLang = (BYTE*)&lang;
          }
          int pidLangOffset = 0;
          while (pointer + 3 < endOfDescriptor && pidLangLength >= 3)
          {
            memcpy(&pidLang[pidLangOffset], &section[pointer], 3);
            pidLangOffset += 3;
            pidLang[pidLangOffset] = 0;   // NULL terminate
            pointer += 4;
          }
        }
        else if (tag == DESCRIPTOR_TELETEXT_DVB /*&& m_pidInfo.TeletextPid == 0*/)
        {
          m_pidInfo.TeletextPid = elementaryPid;
          while (pointer + 4 < endOfDescriptor)
          {
            TeletextServiceInfo info;
            info.Lang[0] = section[pointer++];
            info.Lang[1] = section[pointer++];
            info.Lang[2] = section[pointer++];
            info.Lang[3] = 0;
            info.Type = section[pointer] >> 3;
            if (info.Type == 2 || info.Type == 5) // subtitle page or subtitle page for hearing impaired
            {
              info.Page = (section[pointer++] & 0x7) * 100;
              info.Page += (section[pointer] >> 4) * 10;
              info.Page += (section[pointer++] & 0xf);
              if (!m_pidInfo.HasTeletextPageInfo(info.Page))
              {
                m_pidInfo.TeletextInfo.push_back(info);
              }
            }
            else
            {
              pointer += 2;
            }
          }
          //LogDebug("PMT:  teletext PID %d, stream type = 0x%x", elementaryPid, streamType);
        }
        else if (streamType == STREAM_TYPE_SUBTITLES_SCTE || (tag == DESCRIPTOR_SUBTITLING_DVB && streamType == STREAM_TYPE_PES_PRIVATE_DATA))
        {
          SubtitlePid pid;
          pid.Pid = elementaryPid;
          pid.StreamType = streamType;
          if (tag == DESCRIPTOR_SUBTITLING_DVB)
          {
            if (length > 3)
            {
              memcpy(pid.Lang, &section[pointer], 3);
              pid.Lang[3] = 0;  // NULL terminate
            }
          }
          else if (lang[0] != 0)
          {
            memcpy(pid.Lang, lang, sizeof(pid.Lang));
          }
          m_pidInfo.SubtitlePids.push_back(pid);
          //LogDebug("PMT:  subtitle PID %d, stream type = 0x%x", elementaryPid, streamType);
        }
        else if (tag == DESCRIPTOR_CONDITIONAL_ACCESS)
        {
          m_pidInfo.ConditionalAccessDescriptorCount++;
        }

        pointer = endOfDescriptor;
      }
      if (pointer != endOfEsInfo)
      {
        LogDebug("PMT: section parsing error 1");
        return false;
      }
    }
    if (pointer != endOfSection)
    {
      LogDebug("PMT: section parsing error 2");
      return false;
    }
  } 
  catch (...) 
  { 
    LogDebug("PMT: unhandled exception in DecodePmtSection()");
    return false;
  }
  return true;
}

CPidTable& CBasePmtParser::GetPidInfo()
{
  return m_pidInfo;
}

void CBasePmtParser::OnNewSection(CSection& sections)
{ 
  // Can be overriden by derived classes.
  DecodePmtSection(sections);
}