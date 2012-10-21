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

void LogDebug(const char* fmt, ...);
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
  CleanUp();
}

void CSdtParser::CleanUp()
{
  map<int, char*>::iterator it = m_mBouquetNames.begin();
  while (it != m_mBouquetNames.end())
  {
    char* name = it->second;
    delete[] name;
    name = NULL;
    it++;
  }
  m_mBouquetNames.clear();
}

void CSdtParser::Reset(bool parseSdtOther)
{
  LogDebug("SdtParser: reset, parse SDT other = %d", parseSdtOther);
  CleanUp();
  m_iNextBouquetId = 0x10000;   // normal bouquet IDs have a maximum length of 16 bits
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
      // Details do not apply yet...
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
      info.OriginalNetworkId = original_network_id;
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

        if (tag == 0x47)  // bouquet name descriptor
        {
          char* name = NULL;
          DecodeBouquetNameDescriptor(&section[pointer], length, &name);
          if (name != NULL)
          {
            m_mBouquetNames[m_iNextBouquetId] = name;
            info.BouquetIds.push_back(m_iNextBouquetId++);
          }
        }
        else if (tag == 0x48) // service descriptor
        {
          DecodeServiceDescriptor(&section[pointer], length, &(info.ServiceType), (char**)&(info.ProviderName), (char**)&(info.ServiceName));
          //LogDebug("SdtParser: service type = 0x%x, service name = %s, provider name = %s", info.ServiceType, info.ServiceName, info.ProviderName);
          if (info.ServiceType == 0x19 || info.ServiceType == 0x1a || info.ServiceType == 0x1b)
          {
            info.IsHighDefinition = true;
          }
        }
        else if (tag == 0x49) // country availability descriptor
        {
          DecodeCountryAvailabilityDescriptor(&section[pointer], length, &info.AvailableInCountries, &info.UnavailableInCountries);
        }
        else if (tag == 0x50) // component descriptor
        {
          bool isVideo;
          bool isAudio;
          bool isHighDefinition;
          unsigned int language;
          if (DecodeComponentDescriptor(&section[pointer], length, &isVideo, &isAudio, &isHighDefinition, &language))
          {
            if (isVideo)
            {
              info.VideoStreamCount++;
            }
            else if (isAudio)
            {
              info.AudioStreamCount++;
            }
            info.IsHighDefinition |= isHighDefinition;
            if (language != 0)
            {
              info.Languages.push_back(language);
            }
          }
        }
        else if (tag == 0x72) // service availability descriptor
        {
          DecodeServiceAvailabilityDescriptor(&section[pointer], length, &info.AvailableInCells, &info.UnavailableInCells);
        }
        // Extended descriptors...
        else if (tag == 0x7f)
        {
          if (length < 1)
          {
            LogDebug("SdtParser: invalid extended descriptor length = %d, pointer = %d, end of descriptor loop = %d, end of section = %d, section length = %d", length, pointer, endOfDescriptorLoop, endOfSection, section_length);
            return;
          }

          int tag_extension = section[pointer];
          if (tag_extension == 0x09)  // target region descriptor
          {
            DecodeTargetRegionDescriptor(&section[pointer], length, &info.TargetRegions);
          }
          else if (tag_extension == 0x0b) // service relocated descriptor
          {
            DecodeServiceRelocatedDescriptor(&section[pointer], length, &info.PreviousOriginalNetworkId, &info.PreviousTransportStreamId, &info.PreviousServiceId);
          }
        }

        pointer = endOfDescriptor;
      }

      pointer = endOfDescriptorLoop;

      if (m_pCallBack != NULL)
      {
        m_pCallBack->OnSdtReceived(info);
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug("SdtParser: section parsing error");
    }
    else
    {
      m_mSeenSections[key] = true;
    }
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in OnNewSection()");
  }
}

char* CSdtParser::GetBouquetName(int bouquetId)
{
  map<int, char*>::iterator it = m_mBouquetNames.find(bouquetId);
  if (it == m_mBouquetNames.end())
  {
    return NULL;
  }
  return it->second;
}

void CSdtParser::DecodeServiceDescriptor(byte* b, int length, int* serviceType, char** providerName, char** serviceName)
{
  if (length < 3)
  {
    LogDebug("SdtParser: invalid service descriptor length = %d", length);
    return;
  }
  try
  {
    *serviceType = b[0];
    int service_provider_name_length = b[1];
    int pointer = 2;
    if (pointer + service_provider_name_length + 1 > length)
    {
      LogDebug("SdtParser: invalid service provider name length = %d, pointer = %d, descriptor length = %d", service_provider_name_length, pointer, length);
      return;
    }
    if (service_provider_name_length > 0)
    {
      *providerName = new char[service_provider_name_length + 1];
      if (*providerName == NULL)
      {
        LogDebug("SdtParser: failed to allocate %d bytes for the provider name in DecodeServiceDescriptor()", service_provider_name_length + 1);
      }
      else
      {
        getString468A(&b[pointer], service_provider_name_length, *providerName, service_provider_name_length + 1);
      }
      pointer += service_provider_name_length;
    }

    int service_name_length = b[pointer++];
    if (pointer + service_name_length > length)
    {
      LogDebug("SdtParser: invalid service name length = %d, pointer = %d, descriptor length = %d", service_name_length, pointer, length);
      return;
    }
    if (service_name_length > 0)
    {
      *serviceName = new char[service_name_length + 1];
      if (*serviceName == NULL)
      {
        LogDebug("SdtParser: failed to allocate %d bytes for the service name in DecodeServiceDescriptor()", service_name_length + 1);
      }
      else
      {
        getString468A(&b[pointer], service_name_length, *serviceName, service_name_length + 1);
      }
    }
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in DecodeServiceDescriptor()");
  }
}

bool CSdtParser::DecodeComponentDescriptor(byte* b, int length, bool* isVideo, bool* isAudio, bool* isHighDefinition, unsigned int* language)
{
  if (length < 6)
  {
    LogDebug("SdtParser: invalid component descriptor length = %d", length);
    return false;
  }
  try
  {
    *isVideo = false;
    *isAudio = false;
    *isHighDefinition = false;

    int stream_content = b[0] & 0x0f;
    int component_type = b[1];
    int component_tag = b[2];
    int iso_639_language_code = b[3] + (b[4] << 8) + (b[5] << 16);
    // (component description not read)

    *language = iso_639_language_code;

    if (stream_content == 1 || stream_content == 5)
    {
      *isVideo = true;
      if (component_type >= 0x09 && component_type <= 0x10)
      {
        *isHighDefinition = true;
      }
    }
    else if (stream_content == 2 || stream_content == 4 || stream_content == 6 || stream_content == 7)
    {
      *isAudio = true;
    }
    return true;
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in DecodeComponentDescriptor()");
    return false;
  }
}

void CSdtParser::DecodeServiceAvailabilityDescriptor(byte* b, int length, vector<int>* availableInCells, vector<int>* unavailableInCells)
{
  if (length < 1)
  {
    LogDebug("SdtParser: invalid service availability descriptor length = %d", length);
    return;
  }
  try
  {
    int pointer = 0;
    bool availability_flag = (b[pointer++] & 0x80) != 0;
    while (pointer + 1 < length)
    {
      int cell_id = (b[pointer] << 8) + b[pointer + 1];
      pointer += 2;
      if (availability_flag)
      {
        availableInCells->push_back(cell_id);
      }
      else
      {
        unavailableInCells->push_back(cell_id);
      }
    }
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in DecodeServiceAvailabilityDescriptor()");
  }
}

void CSdtParser::DecodeCountryAvailabilityDescriptor(byte* b, int length, vector<unsigned int>* availableInCountries, vector<unsigned int>* unavailableInCountries)
{
  if (length < 1)
  {
    LogDebug("SdtParser: invalid country availability descriptor length = %d", length);
    return;
  }
  try
  {
    int pointer = 0;
    bool country_availability_flag = (b[pointer++] & 0x80) != 0;
    while (pointer + 2 < length)
    {
      unsigned int country_code = b[pointer] + (b[pointer + 1] << 8) + (b[pointer + 2] << 16);
      pointer += 3;
      if (country_availability_flag)
      {
        availableInCountries->push_back(country_code);
      }
      else
      {
        unavailableInCountries->push_back(country_code);
      }
    }
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in DecodeCountryAvailabilityDescriptor()");
  }
}

void CSdtParser::DecodeBouquetNameDescriptor(byte* b, int length, char** name)
{
  if (length <= 0)
  {
    return;
  }
  *name = new char[length + 1];
  if (*name == NULL)
  {
    LogDebug("SdtParser: failed to allocate %d bytes in DecodeBouquetNameDescriptor()", length + 1);
    return;
  }
  getString468A(b, length, *name, length + 1);
}

void CSdtParser::DecodeTargetRegionDescriptor(byte* b, int length, vector<__int64>* targetRegions)
{
  if (length < 4)
  {
    LogDebug("SdtParser: invalid target region descriptor length = %d", length);
    return;
  }
  try
  {
    int country_code = (b[1] << 16) + (b[2] << 8) + b[3];
    if (length == 4)
    {
      targetRegions->push_back((__int64)country_code << 32);
      return;
    }

    int pointer = 4;
    while (pointer < length)
    {
      bool country_code_flag = (b[pointer] & 0x04) != 0;
      int region_depth = b[pointer++] & 0x03;

      // How many bytes are we expecting in this loop?
      int byteCount = 0;
      if (country_code_flag)
      {
        byteCount += 3;
      }
      byteCount += region_depth;
      if (region_depth == 3)
      {
        byteCount++;
      }

      if (pointer + byteCount > length)
      {
        LogDebug("SdtParser: invalid target region descriptor length = %d, pointer = %d, country code flag = %d, region depth = %d", length, pointer, country_code_flag, region_depth);
        return;
      }

      if (country_code_flag)
      {
        country_code = (b[pointer] << 16) + (b[pointer + 1] << 8) + b[pointer + 2];
        pointer += 3;
      }

      __int64 targetRegionId = ((__int64)country_code << 32);
      if (region_depth > 0)
      {
        targetRegionId += (b[pointer++] << 24);
        if (region_depth > 1)
        {
          targetRegionId += (b[pointer++] << 16);
          if (region_depth > 2)
          {
            targetRegionId += (b[pointer] << 8) + b[pointer + 1];
            pointer += 2;
          }
        }
      }

      targetRegions->push_back(targetRegionId);
    }
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in DecodeTargetRegionDescriptor()");
  }
}

void CSdtParser::DecodeServiceRelocatedDescriptor(byte* b, int length, int* previousOriginalNetworkId, int* previousTransportStreamId, int* previousServiceId)
{
  if (length != 7)
  {
    LogDebug("SdtParser: invalid service relocated descriptor length = %d", length);
    return;
  }
  try
  {
    *previousOriginalNetworkId = (b[1] << 8) + b[2];
    *previousTransportStreamId = (b[3] << 8) + b[4];
    *previousServiceId = (b[5] << 8) + b[6];
  }
  catch (...)
  {
    LogDebug("SdtParser: unhandled exception in DecodeServiceRelocatedDescriptor()");
  }
}