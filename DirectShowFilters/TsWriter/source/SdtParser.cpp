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
#pragma warning(disable : 4995)
#include <windows.h>
#include "SdtParser.h"

void LogDebug(const char *fmt, ...) ;
extern bool DisableCRCCheck();

CSdtParser::CSdtParser(void)
{
  SetPid(PID_SDT);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset(false);
  m_pCallBack = NULL;
}

CSdtParser::~CSdtParser(void)
{
}

void CSdtParser::Reset(bool parseSdtOther)
{
  LogDebug("SdtParser: reset, parse SDT other = %d", parseSdtOther);
  CSectionDecoder::Reset();
  m_mSeenSections.clear();
  m_bIsReady = false;
  m_bParseSdtOther = parseSdtOther;
  LogDebug("SdtParser: reset done");
}

void CSdtParser::SetCallBack(ISdtCallBack* callBack)
{
  m_pCallBack = callBack;
}

bool CSdtParser::IsReady()
{
  return m_bIsReady;
}

void CSdtParser::OnNewSection(CSection& sections)
{
  // 0x42 = actual (current) transport stream
  // 0x46 = other transport streams
  if (sections.table_id != 0x42 && (sections.table_id != 0x46 || !m_bParseSdtOther))
  {
    return;
  }
  if (m_pCallBack == NULL)
  {
    return;
  }
  byte* section = sections.Data;

  try
  {
    int section_syntax_indicator = section[1] & 0x80;
    int section_length = ((section[1] & 0xf) << 8) + section[2];
    if (section_length > 1021 || section_length < 12)
    {
      LogDebug("SdtParser: invalid section length = %d", section_length);
      return;
    }
    int transport_stream_id = (section[3] << 8) + section[4];
    int version_number = (section[5] >> 1) & 0x1f;
    int current_next_indicator = section[5] & 1;
    if (current_next_indicator == 0)
    {
      // Details do not yet apply...
      return;
    }
    int section_number = section[6];
    int last_section_number = section[7];
    int original_network_id = (section[8] << 8) + section[9];

    int endOfSection = section_length - 1;
    //LogDebug("SdtParser: TSID = 0x%x, ONID = 0x%x, table ID = 0x%x, section number = %d, version number = %d, last section number = %d, section length = %d, end of section = %d",
    //          transport_stream_id, original_network_id, sections.table_id, section_number, version_number, last_section_number, section_length, endOfSection);

    unsigned __int64 key = ((unsigned __int64)sections.table_id << 40) + ((unsigned __int64)original_network_id << 24) + (transport_stream_id << 8) + section_number;
    map<unsigned __int64, bool>::iterator it = m_mSeenSections.find(key);
    if (it != m_mSeenSections.end())
    {
      // We know about this key. Have we seen it before?
      if (it->second)
      {
        // We've seen this section before. Have we seen all the sections that we're expecting to see?
        //LogDebug("SdtParser: previously seen section %x", key);
        if (!m_bIsReady)
        {
          bool ready = true;
          for (it = m_mSeenSections.begin(); it != m_mSeenSections.end(); it++)
          {
            if (!it->second)
            {
              //LogDebug("SdtParser: not yet seen %x", it->first);
              ready = false;
              break;
            }
          }
          m_bIsReady = ready;
          if (ready)
          {
            LogDebug("SdtParser: ready, sections parsed = %d", m_mSeenSections.size());
          }
        }
        return;
      }
    }
    else
    {
      //LogDebug("SdtParser: new section %x", key);
      m_bIsReady = false;
      unsigned __int64 k = ((unsigned __int64)sections.table_id << 40) + ((unsigned __int64)original_network_id << 24) + (transport_stream_id << 8);
      while ((k & 0xff) <= last_section_number)
      {
        if (m_mSeenSections.find(k) == m_mSeenSections.end())
        {
          //LogDebug("SdtParser: add section %x", k);
          m_mSeenSections[k] = false;
        }
        k++;
      }
    }

    bool error = false;

    int pointer = 11; // points to the first byte in the service loop
    while (pointer + 4 < endOfSection)
    {
      int service_id = (section[pointer] << 8) + section[pointer + 1];
      pointer += 2;
      int eit_schedule_flag = (section[pointer] >> 1) & 1;
      int eit_present_following_flag = section[pointer] & 1;
      pointer++;
      int running_status = (section[pointer] >> 5) & 7;
      int free_ca_mode = (section[pointer] >> 4) & 1;

      CChannelInfo info;
      info.NetworkId = original_network_id;
      info.TransportStreamId = transport_stream_id;
      info.ServiceId = service_id;
      info.IsOtherMux = (sections.table_id == 0x46);
      info.IsRunning = (running_status == 2 || running_status == 4);    // running or starting in a few seconds
      info.IsEncrypted = (free_ca_mode == 1);
      //LogDebug("SdtParser: service ID = 0x%x, EIT schedule flag = %d, EIT present following flag = %d, running status = %d, free CA mode = %d",
      //          service_id, eit_schedule_flag, eit_present_following_flag, running_status, free_ca_mode);

      int descriptors_loop_length = ((section[pointer] & 0xf) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfDescriptorLoop = pointer + descriptors_loop_length;
      //LogDebug("SdtParser: pointer = %d, descriptor loop length = %d, end of descriptor loop = %d", pointer, descriptors_loop_length, endOfDescriptorLoop);
      if (endOfDescriptorLoop > endOfSection)
      {
        LogDebug("SdtParser: invalid descriptor loop length = %d, pointer = %d, end of descriptor loop = %d, end of section = %d, section length = %d", descriptors_loop_length, pointer, endOfDescriptorLoop, endOfSection, section_length);
        error = true;
        if (m_pCallBack != NULL)
        {
          m_pCallBack->OnSdtReceived(info);
        }
        return;
      }

      while (pointer + 1 < endOfDescriptorLoop)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        int endOfDescriptor = pointer + length;
        //LogDebug("SdtParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, endOfDescriptor);
        if (endOfDescriptor > endOfDescriptorLoop)
        {
          LogDebug("SdtParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptor loop = %d, section length = %d", length, pointer, endOfDescriptor, endOfDescriptorLoop, section_length);
          return;
        }
        if (tag == 0x48)  // service descriptor
        {
          info.ServiceType = section[pointer++];
          int service_provider_name_length = section[pointer++];
          if (pointer + service_provider_name_length + 1 <= endOfDescriptor)
          {
            getString468A(&section[pointer], service_provider_name_length, info.ProviderName, CHANNEL_INFO_MAX_STRING_LENGTH);
            pointer += service_provider_name_length;
            int service_name_length = section[pointer++];
            if (pointer + service_name_length <= endOfDescriptor)
            {
              getString468A(&section[pointer], service_name_length, info.ServiceName, CHANNEL_INFO_MAX_STRING_LENGTH);
              pointer += service_name_length;
              //LogDebug("SdtParser: service type = 0x%x, service name = %s, provider name = %s", info.ServiceType, info.ServiceName, info.ProviderName);
            }
            else
            {
              error = true;
            }
          }
          else
          {
            error = true;
          }
        }
        pointer = endOfDescriptor;
      }
      pointer = endOfDescriptorLoop;
      if (m_pCallBack != NULL && !error)
      {
        m_pCallBack->OnSdtReceived(info);
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug("SdtParser: section parsing error");
      error = true;
    }
    else if (!error)
    {
      m_mSeenSections[key] = true;
    }
  }
  catch(...)
  {
    LogDebug("SdtParser: unhandled exception in OnNewSection()");
  }
}


