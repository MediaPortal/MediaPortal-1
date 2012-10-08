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
#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include "BasePmtParser.h"
#include <cassert>

void LogDebug(const char *fmt, ...) ; 
extern bool DisableCRCCheck();

CBasePmtParser::CBasePmtParser()
{
  m_bIsFound = false;
  m_iServiceId = -1;
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
  m_bIsFound = false;
  m_pidInfo.Reset();
  CSectionDecoder::Reset();
}

void CBasePmtParser::SetFilter(int pid, int serviceId)
{
  SetPid(pid);
  m_iServiceId = serviceId;
}

void CBasePmtParser::GetFilter(int &pid, int &serviceId)
{
  pid = GetPid();
  serviceId = m_iServiceId;
}

bool CBasePmtParser::IsReady()
{
  return m_bIsFound;
}

void CBasePmtParser::OnTsPacket(byte* tsPacket)
{
  if (m_bIsFound)
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
    int section_syntax_indicator = section[1] & 0x80;
    int section_length = ((section[1] & 0xf) << 8) + section[2];
    if (section_length > 1021 || section_length < 13)
    {
      LogDebug("PmtParser: invalid section length = %d", section_length);
      return false;
    }
    int program_number = (section[3] << 8) + section[4];
    int version_number = (section[5] >> 1) & 0x1f;
    int current_next_indicator = section[5] & 1;
    if (current_next_indicator == 0)
    {
      // Details do not apply yet...
      return false;
    }
    int section_number = section[6];
    int last_section_number = section[7];
    int pcr_pid = ((section[8] & 0x1f) << 8) + section[9];
    int program_info_length = ((section[10] & 0xf) << 8) + section[11];

    int pointer = 12;
    int endOfProgramInfo = pointer + program_info_length;
    int endOfSection = section_length - 1;
    //LogDebug("PmtParser: program number = 0x%x, section number = %d, version number = %d, last section number = %d, program info length = %d, end of program info = %d, section length = %d, end of section = %d",
    //          program_number, section_number, version_number, last_section_number, program_info_length, endOfProgramInfo, section_length, endOfSection);
    if (endOfProgramInfo > endOfSection)
    {
      LogDebug("PmtParser: invalid program info length = %d, pointer = %d, end of program info = %d, end of section = %d, section length = %d", program_info_length, pointer, endOfProgramInfo, endOfSection, section_length);
      return false;
    }
    while (pointer + 1 < endOfProgramInfo)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      int endOfDescriptor = pointer + length;
      //LogDebug("PmtParser: program descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
      if (endOfDescriptor > endOfProgramInfo)
      {
        LogDebug("PmtParser: invalid program descriptor length = %d, pointer = %d, end of descriptor = %d, end of program info = %d, section length = %d", length, pointer, endOfDescriptor, endOfProgramInfo, section_length);
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
    m_pidInfo.ServiceId = program_number;
    m_pidInfo.PcrPid = pcr_pid;
    bool seenMpegVideoEs = false;
    while (pointer + 4 < endOfSection)
    {
      int stream_type = section[pointer++];
      int elementary_pid = ((section[pointer] & 0x1f) << 8) + section[pointer + 1];
      pointer += 2;
      int es_info_length = ((section[pointer] & 0xf) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfEsInfo = pointer + es_info_length;
      //LogDebug("PmtParser: stream type = 0x%x, elementary PID = 0x%x, elementary stream info length = %d, end of elementary stream info = %d", stream_type, elementary_pid, es_info_length, endOfEsInfo);
      if (endOfEsInfo > endOfSection)
      {
        LogDebug("PmtParser: invalid elementary stream info length = %d, pointer = %d, end of elementary stream info = %d, end of section = %d, section length = %d", es_info_length, pointer, endOfEsInfo, endOfSection, section_length);
        return false;
      }

      if (stream_type == STREAM_TYPE_VIDEO_MPEG1 ||
        stream_type == STREAM_TYPE_VIDEO_MPEG2 ||
        stream_type == STREAM_TYPE_VIDEO_MPEG4 ||
        stream_type == STREAM_TYPE_VIDEO_H264 ||
        (stream_type == STREAM_TYPE_VIDEO_MPEG2_DCII && !seenMpegVideoEs))
      {
        VideoPid pid;
        pid.Pid = elementary_pid;
        pid.StreamType = stream_type;
        if (stream_type == STREAM_TYPE_VIDEO_MPEG2_DCII)
        {
          pid.StreamType = STREAM_TYPE_VIDEO_MPEG2;
        }
        else if (!seenMpegVideoEs)
        {
          m_pidInfo.videoPids.clear();
          seenMpegVideoEs = true;
        }
        m_pidInfo.videoPids.push_back(pid);
        //LogDebug("PmtParser:  video PID 0x%x, stream type = 0x%x", elementary_pid, stream_type);
      }
      else if (stream_type == STREAM_TYPE_AUDIO_MPEG1 ||
        stream_type == STREAM_TYPE_AUDIO_MPEG2 ||
        stream_type == STREAM_TYPE_AUDIO_AAC ||
        stream_type == STREAM_TYPE_AUDIO_LATM_AAC ||
        stream_type == STREAM_TYPE_AUDIO_AC3 ||
        stream_type == STREAM_TYPE_AUDIO_E_AC3)
      {          
        AudioPid pid;
        pid.Pid = elementary_pid;
        pid.StreamType = stream_type;
        m_pidInfo.audioPids.push_back(pid);
        //LogDebug("PmtParser:  audio PID 0x%x, stream type = 0x%x", elementary_pid, stream_type);
      }

      char lang[7];
      while (pointer + 1 < endOfEsInfo)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        lang[0] = 0;
        int endOfDescriptor = pointer + length;
        //LogDebug("PmtParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
        if (endOfDescriptor > endOfEsInfo)
        {
          LogDebug("PmtParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of elementary stream info = %d, section length = %d", length, pointer, endOfDescriptor, endOfEsInfo, section_length);
          return false;
        }

        if (tag == DESCRIPTOR_AC3_DVB || tag == DESCRIPTOR_E_AC3_DVB)
        {                
          AudioPid pid;
          pid.Pid = elementary_pid;
          if (tag == DESCRIPTOR_AC3_DVB)
          {
            pid.StreamType = STREAM_TYPE_AUDIO_AC3;
            //LogDebug("PmtParser:  AC-3 audio PID 0x%x, stream type = 0x%x", elementary_pid, stream_type);
          }
          else
          {
            pid.StreamType = STREAM_TYPE_AUDIO_E_AC3;
            //LogDebug("PmtParser:  enhanced AC-3 audio PID 0x%x, stream type = 0x%x", elementary_pid, stream_type);
          }
          if (lang[0] != 0)
          {
            memcpy(pid.Lang, lang, sizeof(pid.Lang));
          }

          m_pidInfo.audioPids.push_back(pid);
        }
        // audio or subtitle language
        else if (tag == DESCRIPTOR_ISO_639_LANG)
        {          
          bool pidFound = false;
          int count = length == 8 ? 6 : 3;
          for (unsigned int i = 0; i < m_pidInfo.audioPids.size(); i++)
          {
            if (m_pidInfo.audioPids[i].Pid == elementary_pid)
            {
              memcpy(m_pidInfo.audioPids[i].Lang, &section[pointer], count);
              m_pidInfo.audioPids[i].Lang[count] = 0; // NULL terminate
              pidFound = true;
              break;
            }
          }
          if (!pidFound)
          {
            for (unsigned int i = 0; i < m_pidInfo.subtitlePids.size(); i++)
            {
              if (m_pidInfo.subtitlePids[i].Pid == elementary_pid)
              {
                memcpy(m_pidInfo.audioPids[i].Lang, &section[pointer], 3);
                m_pidInfo.subtitlePids[i].Lang[3] = 0;  // NULL terminate
                pidFound = true;
                break;
              }
            }
          }
          if (!pidFound)
          {
            memcpy(lang, &section[pointer], count);
          }
        }
        else if (tag == DESCRIPTOR_TELETEXT_DVB /*&& m_pidInfo.TeletextPid == 0*/)
        {
          m_pidInfo.TeletextPid = elementary_pid;
          while (pointer + 4 < endOfDescriptor)
          {
            TeletextServiceInfo info;
            info.lang[0] = section[pointer++];
            info.lang[1] = section[pointer++];
            info.lang[2] = section[pointer++];
            info.type = section[pointer] >> 3;
            if (info.type == 2 || info.type == 5) // subtitle page or subtitle page for hearing impaired
            {
              info.page = (section[pointer++] & 0x7) * 100;
              info.page += (section[pointer] >> 4) * 10;
              info.page += (section[pointer++] & 0xf);
              if (!m_pidInfo.HasTeletextPageInfo(info.page))
              {
                m_pidInfo.TeletextInfo.push_back(info);
              }
            }
            else
            {
              pointer += 2;
            }
          }
          //LogDebug("PmtParser:  teletext PID 0x%x, stream type = 0x%x", elementary_pid, stream_type);
        }
        else if (stream_type == STREAM_TYPE_SUBTITLES_SCTE || (tag == DESCRIPTOR_SUBTITLING_DVB && stream_type == STREAM_TYPE_SUBTITLES))
        {
          SubtitlePid pid;
          pid.Pid = elementary_pid;
          pid.StreamType = STREAM_TYPE_SUBTITLES;
          if (tag == DESCRIPTOR_SUBTITLING_DVB)
          {
            memcpy(pid.Lang, &section[pointer], 3);
          }
          else if (lang[0] != 0)
          {
            memcpy(pid.Lang, lang, 3);
          }
          pid.Lang[3] = 0;  // NULL terminate
          m_pidInfo.subtitlePids.push_back(pid);
          //LogDebug("PmtParser:  subtitle PID 0x%x, stream type = 0x%x", elementary_pid, stream_type);
        }
        else if (tag == DESCRIPTOR_CONDITIONAL_ACCESS)
        {
          m_pidInfo.ConditionalAccessDescriptorCount++;
        }

        pointer = endOfDescriptor;
      }
      if (pointer != endOfEsInfo)
      {
        LogDebug("PmtParser: section parsing error 1");
        return false;
      }
    }
    if (pointer != endOfSection)
    {
      LogDebug("PmtParser: section parsing error 2");
      return false;
    }
  } 
  catch (...) 
  { 
    LogDebug("PmtParser: unhandled exception in DecodePmtSection()");
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
  // Implemented in derived classes.
}
