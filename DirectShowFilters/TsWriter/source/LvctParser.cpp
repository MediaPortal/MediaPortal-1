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
#include "..\..\shared\BasePmtParser.h"
#include "LvctParser.h"

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CLvctParser::CLvctParser(void)
{
  SetPid(PID_VCT);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset();
  m_pCallBack = NULL;
}

CLvctParser::~CLvctParser(void)
{
}

void CLvctParser::SetCallBack(ILvctCallBack* callBack)
{
  m_pCallBack = callBack;
}

bool CLvctParser::IsReady()
{
  return m_bIsReady;
}

void CLvctParser::Reset()
{
  LogDebug("LvctParser: reset");
  CSectionDecoder::Reset();
  m_mSeenSections.clear();
  m_bIsReady = false;
  LogDebug("LvctParser: reset done");
}

void CLvctParser::OnNewSection(CSection& sections)
{
  // 0xc8 = terrestrial long form virtual channel table
  // 0xc9 = cable long form virtual channel table
  if (sections.table_id != 0xc8 && sections.table_id != 0xc9)
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
    if ((sections.table_id == 0xc8 && section_length > 1021) || section_length > 4093 || section_length < 13)
    {
      LogDebug("LvctParser: invalid section length = %d", section_length);
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

    int endOfSection = section_length - 1;
    //LogDebug("LvctParser: TSID = 0x%x, table ID = 0x%x, section number = %d, version = %d, last section number = %d, section length = %d, end of section = %d",
    //          transport_stream_id, sections.table_id, section_number, version_number, last_section_number, section_length, endOfSection);

    unsigned int key = (sections.table_id << 24) + (transport_stream_id << 8) + section_number;
    map<unsigned int, bool>::iterator it = m_mSeenSections.find(key);
    if (it != m_mSeenSections.end())
    {
      // We know about this key. Have we seen it before?
      if (it->second)
      {
        // We've seen this section before. Have we seen all the sections that we're expecting to see?
        //LogDebug("%s: previously seen section %x", m_sName, key);
        if (!m_bIsReady)
        {
          bool ready = true;
          for (it = m_mSeenSections.begin(); it != m_mSeenSections.end(); it++)
          {
            if (!it->second)
            {
              //LogDebug("LvctParser: not yet seen %x", it->first);
              ready = false;
              break;
            }
          }
          m_bIsReady = ready;
          if (ready)
          {
            LogDebug("LvctParser: ready, sections parsed = %d", m_mSeenSections.size());
          }
        }
        return;
      }
    }
    else
    {
      //LogDebug("LvctParser: new section %x", key);
      m_bIsReady = false;
      unsigned int k = (sections.table_id << 24) + (transport_stream_id << 8);
      while ((int)(k & 0xff) <= last_section_number)
      {
        if (m_mSeenSections.find(k) == m_mSeenSections.end())
        {
          //LogDebug("LvctParser: add section %x", k);
          m_mSeenSections[k] = false;
        }
        k++;
      }
    }

    int protocol_version = section[8];
    int num_channels_in_section = section[9];
    //LogDebug("LvctParser: protocol version = %d, number of channels = %d", protocol_version, num_channels_in_section);
    int pointer = 10;
    for (int i = 0; i < num_channels_in_section && pointer + 31 < endOfSection; i++)
    {
      // short_name = 7*16 bits (14 bytes), UTF-16 encoding
      char* short_name = new char[8];
      if (short_name == NULL)
      {
        LogDebug("LvctParser: failed to allocate 8 bytes for the short name");
      }
      else
      {
        for (int count = 0; count < 7; count++)
        {
          // Take every second byte as a rough ASCII conversion.
          pointer++;  // skip the UTF-16 high byte
          short_name[count] = section[pointer++];
          short_name[count + 1] = 0; // NULL terminate
        }
      }

      int major_channel = ((section[pointer] & 0xf) << 6) + (section[pointer + 1] >> 6);
      pointer += 2;
      int minor_channel = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
      pointer += 2;

      // Frequency and modulation might be useful as part of an ATSC network
      // scan... except that the standard says that the frequency is deprecated
      // and should be set to zero. There is no other known ATSC NIT standard.
      int modulation_mode = section[pointer++];
      int carrier_frequency = section[pointer++] << 24;
      carrier_frequency += (section[pointer++] << 16);
      carrier_frequency += (section[pointer++] << 8);
      carrier_frequency += section[pointer++];

      int channel_tsid = ((section[pointer]) << 8) + section[pointer + 1];
      pointer += 2;
      int program_number = ((section[pointer]) << 8) + section[pointer + 1];
      pointer += 2;
      int etm_location = (section[pointer] >> 6);
      int access_controlled = ((section[pointer] >> 5) & 0x1);
      int hidden = ((section[pointer] >> 4) & 0x1);

      // cable only
      int path_select = ((section[pointer] >> 3) & 0x1);
      int out_of_band = ((section[pointer] >> 2) & 0x1);

      int hide_guide = ((section[pointer++] >> 1) & 0x1);
      int service_type = (section[pointer++] & 0x3f);
      int source_id = ((section[pointer]) << 8) + section[pointer + 1];
      pointer += 2;
      LogDebug("LvctParser: short name = %s, major channel = %d, minor channel = %d, modulation = %d, carrier frequency = %d, channel TSID = 0x%x, program number = 0x%x, ETM location = %d, access controlled = %d, hidden = %d, path select = %d, out of band = %d, hide guide = %d, service type = 0x%x, source ID = 0x%x",
                  short_name, major_channel, minor_channel, modulation_mode, carrier_frequency, channel_tsid, program_number,
                  etm_location, access_controlled, hidden, path_select, out_of_band, hide_guide, service_type, source_id);

      // descriptors...
      int descriptors_length = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfDescriptors = pointer + descriptors_length;
      LogDebug("LvctParser: pointer = %d, descriptors length = %d, end of descriptors = %d", pointer, descriptors_length, endOfDescriptors);

      vector<char*> extendedNames;
      int videoStreamCount = 0;
      int audioStreamCount = 0;
      vector<unsigned int> languages;
      while (pointer + 1 < endOfDescriptors)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        //LogDebug("LvctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, pointer + length);
        if (pointer + length > endOfDescriptors)
        {
          LogDebug("LvctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptors = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfDescriptors, endOfSection, section_length);
          return;
        }

        if (tag == 0xa0)  // extended channel name descriptor
        {
          DecodeMultipleStrings(&section[pointer], length, &extendedNames);
        }
        else if (tag == 0xa1) // service location descriptor
        {
          DecodeServiceLocationDescriptor(&section[pointer], length, &videoStreamCount, &audioStreamCount, &languages);
        }
        pointer += length;
      }

      if (program_number == 0)
      {
        // The service is inactive. program_number is the equivalent of the DVB service ID. There is
        // no way we can properly handle channels with service ID not set.
        LogDebug("LvctParser: program number is zero, service is inactive");
        continue;
      }
      if (out_of_band == 1)
      {
        // The service is delivered via some out-of-band mechanism that we don't have access to.
        LogDebug("LvctParser: service is carried out of band");
        continue;
      }

      CChannelInfo info;
      info.TransportStreamId = channel_tsid;
      info.ServiceId = program_number;
      if (extendedNames.size() == 0)
      {
        info.ServiceName = short_name;  // copy the pointer address - CChannelInfo now becomes responsible for the memory
      }
      else
      {
        // The name is the short name plus the first extended name.
        bool isFirst = true;
        for (vector<char*>::iterator it = extendedNames.begin(); it != extendedNames.end(); it++)
        {
          if (isFirst)
          {
            isFirst = false;
            int bufferSize = strlen(*it) + 1; // + 1 for NULL termination
            if (short_name != NULL)
            {
              bufferSize += strlen(short_name) + 3; // + 3 for " (" and ")"
            }
            info.ServiceName = new char[bufferSize];
            if (info.ServiceName == NULL)
            {
              LogDebug("LvctParser: failed to allocate %d bytes for the service name", bufferSize);
            }
            else
            {
              if (short_name != NULL)
              {
                strcpy(info.ServiceName, short_name);
                strcat(info.ServiceName, " (");
                strcat(info.ServiceName, *it);
                strcat(info.ServiceName, ")");

                // Don't forget to free the short name buffer.
                delete[] short_name;
                short_name = NULL;
              }
              else
              {
                strcpy(info.ServiceName, *it);
              }
            }
          }

          // Free the extended name buffer(s). We have no need for them anymore.
          delete[] *it;
        }
      }

      info.ProviderName = new char[6];
      if (info.ProviderName == NULL)
      {
        LogDebug("LvctParser: failed to allocate 6 bytes for the provider name");
      }
      else
      {
        if (sections.table_id == 0xc8)
        {
          strcpy(info.ProviderName, "ATSC");
        }
        else
        {
          strcpy(info.ProviderName, "Cable");
        }
      }

      // The LCN can be two part or one part for cable channels. For terrestrial channels
      // the LCN is meant to be two part with major 1..99 and minor 0..999. Cable extends
      // the major channel limit to 1..999.
      info.LogicalChannelNumber = new char[10]; // allow for <4 digits>.<4 digits><NULL>
      if (info.LogicalChannelNumber == NULL)
      {
        LogDebug("LvctParser: failed to allocate 10 bytes for the logical channel number");
      }
      else
      {
        if (((major_channel >> 4) & 0x3f) == 0x3f)
        {
          sprintf(info.LogicalChannelNumber, "%d", ((major_channel & 0xf) << 10) + minor_channel);
        }
        else
        {
          sprintf(info.LogicalChannelNumber, "%d.%d", major_channel, minor_channel);
        }
      }

      info.ServiceType = service_type;
      info.VideoStreamCount = videoStreamCount;
      info.AudioStreamCount = audioStreamCount;
      info.IsEncrypted = access_controlled == 1;
      info.Languages = languages;

      // According to the standard, the program_number would be set to zero if the
      // service is "inactive". We checked for that above. If we get to here then
      // the standing assumption is that the service is running, so this is just a
      // formality.
      // Again according to the standard, hidden services are either not running or
      // feeds/tests. hide_guide is usually set when the channel is a test. Therefore
      // services that are not running are those that are hidden but not excluded from
      // the guide, and running services are either not hidden or excluded from the
      // guide [if they are tests].
      info.IsRunning = (hidden == 0) || (hide_guide == 1);
      info.IsOtherMux = transport_stream_id != channel_tsid;

      if (m_pCallBack != NULL)
      {
        m_pCallBack->OnLvctReceived(info);
      }
    }

    // additional descriptors...
    int additional_descriptors_length = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
    pointer += 2;
    int endOfDescriptors = pointer + additional_descriptors_length;
    LogDebug("LvctParser: pointer = %d, additional descriptors length = %d, end of descriptors = %d", pointer, additional_descriptors_length, endOfDescriptors);
    while (pointer + 1 < endOfDescriptors)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      //LogDebug("LvctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, pointer + length);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug("LvctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptors = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfDescriptors, endOfSection, section_length);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("LvctParser: section parsing error");
    }
    else
    {
      m_mSeenSections[key] = true;
    }
  }
  catch (...)
  {
    LogDebug("LvctParser: unhandled exception in OnNewSection()");
  }
}

void CLvctParser::DecodeServiceLocationDescriptor(byte* b, int length, int* videoStreamCount, int* audioStreamCount, vector<unsigned int>* languages)
{
  if (length < 3)
  {
    LogDebug("LvctParser: invalid service location descriptor length = %d", length);
    *videoStreamCount = 0;
    *audioStreamCount = 0;
    return;
  }
  try
  {
    int pcr_pid = ((b[0] & 0x1f) << 8) + b[1];
    int number_elements = b[2];
    //LogDebug("LvctParser: PCR PID = 0x%x, number of elements = %d", pcr_pid, number_elements);
    int pointer = 3;
    for (int i = 0; i < number_elements && pointer + 5 < length; i++)
    {
      int stream_type = b[pointer++];
      int elementary_pid = ((b[pointer] & 0x1f) << 8) + b[pointer + 1];
      pointer += 2;
      unsigned int iso_639_language_code = b[pointer] + (b[pointer + 1] << 8) + (b[pointer + 2] << 16);
      if (iso_639_language_code != 0)
      {
        languages->push_back(iso_639_language_code);
      }
      pointer += 3;
      //LogDebug("LvctParser: stream type = 0x%x, elementary PID = 0x%x", stream_type, elementary_pid);

      if (stream_type == STREAM_TYPE_VIDEO_MPEG1 ||
          stream_type == STREAM_TYPE_VIDEO_MPEG2 ||
          stream_type == STREAM_TYPE_VIDEO_MPEG4 ||
          stream_type == STREAM_TYPE_VIDEO_H264 ||
          stream_type == STREAM_TYPE_VIDEO_MPEG2_DCII)
      {
        *videoStreamCount++;
      }
      else if (stream_type == STREAM_TYPE_AUDIO_MPEG1 ||
          stream_type == STREAM_TYPE_AUDIO_MPEG2 ||
          stream_type == STREAM_TYPE_AUDIO_AAC ||
          stream_type == STREAM_TYPE_AUDIO_LATM_AAC ||
          stream_type == STREAM_TYPE_AUDIO_AC3 ||
          stream_type == STREAM_TYPE_AUDIO_E_AC3_ATSC)
      {
        *audioStreamCount++;
      }
    }
    //LogDebug("LvctParser: video stream count = %d, audio stream count = %d", *videoStreamCount, *audioStreamCount);
  }
  catch (...)
  {
    LogDebug("LvctParser: unhandled exception in DecodeServiceLocationDescriptor()");
    *videoStreamCount = 0;
    *audioStreamCount = 0;
    languages->clear();
  }
}

void CLvctParser::DecodeMultipleStrings(byte* b, int length, vector<char*>* strings)
{
  if (length < 1)
  {
    LogDebug("LvctParser: invalid multiple strings structure length = %d", length);
    return;
  }
  try
  {
    int number_of_strings = b[0];

    //LogDebug("LvctParser: parse multiple strings, number of strings = %d", number_of_strings);  
    int pointer = 1;
    for (int i = 0; i < number_of_strings && pointer + 3 < length; i++)
    {
      unsigned int iso_639_language_code = b[pointer] + (b[pointer + 1] << 8) + (b[pointer + 2] << 16);
      pointer += 3;
      int number_of_segments = b[pointer++];
      //LogDebug("LvctParser: string %d, number of segments = %d", i, number_of_segments);
      for (int j = 0; j < number_of_segments && pointer + 2 < length; j++)
      {
        int compression_type = b[pointer++];
        int mode = b[pointer++];
        int number_bytes = b[pointer++];
        //LogDebug("LvctParser: segment %d, compression type = 0x%x, mode = 0x%x, number of bytes = %d", j, compression_type, mode, number_bytes);
        if (pointer + number_bytes >= length)
        {
          LogDebug("LvctParser: invalid string length %d in multiple string structure, pointer = %d, structure length = %d", number_bytes, pointer, length);
          return;
        }

        char* string = NULL;
        DecodeString(b, compression_type, mode, number_bytes, &string);
        if (string != NULL)
        {
          strings->push_back(string);
        }

        pointer += number_bytes;
      }
    }
  }
  catch (...)
  {
    LogDebug("LvctParser: unhandled exception in ParseMultipleStrings()");
  }
}

void CLvctParser::DecodeString(byte* b, int compression_type, int mode, int number_bytes, char** string)
{
  //LogDebug("LvctParser: decode string, compression type = 0x%x, mode = 0x%x, number of bytes = %d", compression_type, mode, number_bytes);
  if (compression_type == 0 && mode == 0)
  {
    *string = new char[number_bytes + 1];
    if (*string == NULL)
    {
      LogDebug("LvctParser: failed to allocate %d bytes in DecodeString()", number_bytes + 1);
      return;
    }
    memcpy(*string, b, number_bytes);
    (*string)[number_bytes] = 0;  // NULL terminate
    return;
  }
  LogDebug("LvctParser: unsupported compression type or mode in DecodeString(), compression type = 0x%x, mode = 0x%x", compression_type, mode);
  for (int i = 0; i < number_bytes; i++)
  {
    LogDebug("  %d: 0x%x", b[i]);
  }
}
