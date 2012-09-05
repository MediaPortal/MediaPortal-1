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
#include <streams.h>
#include <bdatypes.h>
#include "PMTParser.h"
#include "VctParser.h"

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CVctParser::CVctParser(void)
{
  SetPid(PID_VCT);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset();
  m_pCallBack = NULL;
}

CVctParser::~CVctParser(void)
{
}

void CVctParser::SetCallBack(IVctCallBack* callBack)
{
  m_pCallBack = callBack;
}

bool CVctParser::IsReady()
{
  return m_bIsReady;
}

void CVctParser::Reset()
{
  LogDebug("VctParser: reset");
  CSectionDecoder::Reset();
  m_mSeenSections.clear();
  m_bIsReady = false;
  LogDebug("VctParser: reset done");
}

void CVctParser::OnNewSection(CSection& sections)
{
  // 0xc8 = terrestrial virtual channel table
  // 0xc9 = cable virtual channel table
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
      LogDebug("VctParser: invalid section length = %d", section_length);
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

    int endOfSection = section_length - 1;
    //LogDebug("VctParser: TSID = 0x%x, table ID = 0x%x, section number = %d, version = %d, last section number = %d, section length = %d, end of section = %d",
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
              //LogDebug("VctParser: not yet seen %x", it->first);
              ready = false;
              break;
            }
          }
          m_bIsReady = ready;
          if (ready)
          {
            LogDebug("VctParser: ready, sections parsed = %d", m_mSeenSections.size());
          }
        }
        return;
      }
    }
    else
    {
      //LogDebug("VctParser: new section %x", key);
      m_bIsReady = false;
      unsigned int k = (sections.table_id << 24) + (transport_stream_id << 8);
      while ((int)(k & 0xff) <= last_section_number)
      {
        if (m_mSeenSections.find(k) == m_mSeenSections.end())
        {
          //LogDebug("VctParser: add section %x", k);
          m_mSeenSections[k] = false;
        }
        k++;
      }
    }

    int protocol_version = section[8];
    int num_channels_in_section = section[9];
    //LogDebug("VctParser: protocol version = %d, number of channels = %d", protocol_version, num_channels_in_section);
    int pointer = 10;
    for (int i = 0; i < num_channels_in_section && pointer + 31 < endOfSection; i++)
    {
      // short_name = 7*16 bits (14 bytes), UTF-16 encoding
      static char short_name[CHANNEL_INFO_MAX_STRING_LENGTH];
      for (int count = 0; count < 7; count++)
      {
        pointer++;
        if (count < CHANNEL_INFO_MAX_STRING_LENGTH)
        {
          short_name[count] = section[pointer++];  // take every second byte as a rough ASCII conversion
          short_name[count + 1] = 0; // NULL terminate
        }
        else
        {
          pointer++;
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
      LogDebug("VctParser: short name = %s, major channel = %d, minor channel = %d, modulation = %d, carrier frequency = %d, channel TSID = 0x%x, program number = 0x%x, ETM location = %d, access controlled = %d, hidden = %d, path select = %d, out of band = %d, hide guide = %d, service type = 0x%x, source ID = 0x%x",
                  short_name, major_channel, minor_channel, modulation_mode, carrier_frequency, channel_tsid, program_number,
                  etm_location, access_controlled, hidden, path_select, out_of_band, hide_guide, service_type, source_id);

      // descriptors...
      int descriptors_length = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfDescriptors = pointer + descriptors_length;
      LogDebug("VctParser: pointer = %d, descriptors length = %d, end of descriptors = %d", pointer, descriptors_length, endOfDescriptors);

      vector<char*> extendedNames;
      int hasVideo = 0;
      int hasAudio = 0;
      while (pointer + 1 < endOfDescriptors)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        //LogDebug("VctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, pointer + length);
        if (pointer + length > endOfDescriptors)
        {
          LogDebug("VctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptors = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfDescriptors, endOfSection, section_length);
          return;
        }

        if (tag == 0xa0)  // extended channel name descriptor
        {
          DecodeMultipleStrings(&section[pointer], length, &extendedNames);
        }
        else if (tag == 0xa1) // service location descriptor
        {
          DecodeServiceLocationDescriptor(&section[pointer], length, &hasVideo, &hasAudio);
        }
        pointer += length;
      }

      if (program_number == 0)
      {
        // The service is inactive. program_number is the equivalent of the DVB service ID. There is
        // no way we can properly handle channels with service ID not set.
        continue;
      }
      if (out_of_band == 1)
      {
        // The service is delivered via some out-of-band mechanism that we don't have access to.
        continue;
      }

      CChannelInfo info;
      info.TransportStreamId = channel_tsid;
      info.ServiceId = program_number;
      if (extendedNames.size() == 0)
      {
        strcpy(info.ServiceName, short_name);
      }
      else
      {
        // Find the first extended name that can fit on the end of the short name.
        unsigned int maxNameLength = CHANNEL_INFO_MAX_STRING_LENGTH - 7 - 3 - 1; // - 7 for the length of the short name, - 3 for " (...)", - 1 for NULL termination
        for (vector<char*>::iterator it = extendedNames.begin(); it != extendedNames.end(); it++)
        {
          if (strlen(*it) <= maxNameLength)
          {
            strcat(short_name, " (");
            strcat(short_name, *it);
            strcat(short_name, ")");
            break;
          }
        }
      }
      if (sections.table_id == 0xc8)
      {
        strcpy(info.ProviderName, "ATSC");
      }
      else
      {
        strcpy(info.ProviderName, "Cable");
      }

      // The LCN can be two part or one part for cable channels. For terrestrial channels
      // the LCN is meant to be two part with major 1..99 and minor 0..999. Cable extends
      // the major channel limit to 1..999.
      if (CHANNEL_INFO_MAX_STRING_LENGTH > 10)
      {
        static char lcn[CHANNEL_INFO_MAX_STRING_LENGTH];
        if (((major_channel >> 4) & 0x3f) == 0x3f)
        {
          sprintf(lcn, "%d", ((major_channel & 0xf) << 10) + minor_channel);
        }
        else
        {
          sprintf(lcn, "%d.%d", major_channel, minor_channel);
        }
        strcpy((char*)info.LogicalChannelNumber, lcn);
      }

      info.ServiceType = service_type;
      info.HasVideo = hasVideo;
      info.HasAudio = hasAudio;
      info.IsEncrypted = access_controlled == 1;

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
        m_pCallBack->OnVctReceived(info);
      }
    }

    // additional descriptors...
    int additional_descriptors_length = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
    pointer += 2;
    int endOfDescriptors = pointer + additional_descriptors_length;
    LogDebug("VctParser: pointer = %d, additional descriptors length = %d, end of descriptors = %d", pointer, additional_descriptors_length, endOfDescriptors);
    while (pointer + 1 < endOfDescriptors)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      //LogDebug("VctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, pointer + length);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug("VctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptors = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfDescriptors, endOfSection, section_length);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("VctParser: section parsing error");
    }
    else
    {
      m_mSeenSections[key] = true;
    }
  }
  catch (...)
  {
    LogDebug("VctParser: unhandled exception in OnNewSection()");
  }
}

void CVctParser::DecodeServiceLocationDescriptor(byte* b, int length, int* hasVideo, int* hasAudio)
{
  if (length < 3)
  {
    LogDebug("VctParser: invalid service location descriptor length = %d", length);
    *hasVideo = 0;
    *hasAudio = 0;
    return;
  }
  try
  {
    int pcr_pid = ((b[0] & 0x1f) << 8) + b[1];
    int number_elements = b[2];
    //LogDebug("VctParser: PCR PID = 0x%x, number of elements = %d", pcr_pid, number_elements);
    int pointer = 3;
    for (int i = 0; i < number_elements && pointer + 5 < length; i++)
    {
      int stream_type = b[pointer++];
      int elementary_pid = ((b[pointer] & 0x1f) << 8) + b[pointer + 1];
      pointer += 2;
      int iso_639_language_code = (b[pointer] << 16) + (b[pointer + 1] << 8) + b[pointer + 2];
      pointer += 3;
      //LogDebug("VctParser: stream type = 0x%x, elementary PID = 0x%x", stream_type, elementary_pid);

      if (stream_type == STREAM_TYPE_VIDEO_MPEG1 ||
          stream_type == STREAM_TYPE_VIDEO_MPEG2 ||
          stream_type == STREAM_TYPE_VIDEO_MPEG4 ||
          stream_type == STREAM_TYPE_VIDEO_H264 ||
          stream_type == STREAM_TYPE_VIDEO_MPEG2_DCII)
      {
        *hasVideo++;
      }
      else if (stream_type == STREAM_TYPE_AUDIO_MPEG1 ||
          stream_type == STREAM_TYPE_AUDIO_MPEG2 ||
          stream_type == STREAM_TYPE_AUDIO_AAC ||
          stream_type == STREAM_TYPE_AUDIO_LATM_AAC ||
          stream_type == STREAM_TYPE_AUDIO_AC3 ||
          stream_type == STREAM_TYPE_AUDIO_E_AC3_ATSC)
      {
        *hasAudio++;
      }
    }
    //LogDebug("VctParser: has video = %d, has audio = %d", *hasVideo, *hasAudio);
  }
  catch (...)
  {
    LogDebug("VctParser: unhandled exception in DecodeServiceLocationDescriptor()");
    *hasVideo = 0;
    *hasAudio = 0;
  }
}

void CVctParser::DecodeMultipleStrings(byte* b, int length, vector<char*>* strings)
{
  if (length < 1)
  {
    LogDebug("VctParser: invalid multiple strings structure length = %d", length);
    return;
  }
  try
  {
    int number_of_strings = b[0];

    //LogDebug("VctParser: parse multiple strings, number of strings = %d", number_of_strings);  
    int pointer = 1;
    for (int i = 0; i < number_of_strings && pointer + 3 < length; i++)
    {
      int iso_639_language_code = (b[pointer] << 16) + (b[pointer + 1] << 8) + b[pointer + 2];
      pointer += 3;
      int number_of_segments = b[pointer++];
      //LogDebug("VctParser: string %d, number of segments = %d", i, number_of_segments);
      for (int j = 0; j < number_of_segments && pointer + 2 < length; j++)
      {
        int compression_type = b[pointer++];
        int mode = b[pointer++];
        int number_bytes = b[pointer++];
        //LogDebug("VctParser: segment %d, compression type = 0x%x, mode = 0x%x, number of bytes = %d", j, compression_type, mode, number_bytes);
        if (pointer + number_bytes >= length)
        {
          LogDebug("VctParser: invalid string length %d in multiple string structure, pointer = %d, structure length = %d", number_bytes, pointer, length);
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
    LogDebug("VctParser: unhandled exception in ParseMultipleStrings()");
  }
}

void CVctParser::DecodeString(byte* b, int compression_type, int mode, int number_bytes, char** string)
{
  //LogDebug("VctParser: decode string, compression type = 0x%x, mode = 0x%x, number of bytes = %d", compression_type, mode, number_bytes);
  if (compression_type == 0 && mode == 0)
  {
    *string = new char[DESCRIPTOR_MAX_STRING_LENGTH + 1];
    int stringLength = min(number_bytes, DESCRIPTOR_MAX_STRING_LENGTH);
    memcpy(*string, b, stringLength);
    (*string)[stringLength] = 0;  // NULL terminate
    return;
  }
  LogDebug("VctParser: unsupported compression type or mode in DecodeString(), compression type = 0x%x, mode = 0x%x", compression_type, mode);
  for (int i = 0; i < number_bytes; i++)
  {
    LogDebug("  %d: 0x%x", b[i]);
  }
}
