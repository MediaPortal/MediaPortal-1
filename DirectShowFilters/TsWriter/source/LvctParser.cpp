/*
* Copyright (C) 2006-2008 Team MediaPortal
* http://www.team-mediaportal.com
*
* This Program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2, or (at your option)
* any later version.
*
* This Program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with GNU Make; see the file COPYING. If not, write to
* the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
* http://www.gnu.org/copyleft/gpl.html
*
*/
#pragma warning(disable : 4995)
#include <windows.h>
#include <bdatypes.h>
#include "..\..\shared\BasePmtParser.h"
#include "LvctParser.h"

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CLvctParser::CLvctParser(int pid)
{
  SetPid(pid);
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
  m_iCurrentVersion = -1;
  m_mUnseenSections.clear();
  m_bIsReady = false;
  LogDebug("LvctParser: reset done");
}

void CLvctParser::OnNewSection(CSection& sections)
{
  // 0xc8 = terrestrial virtual channel table
  // 0xc9 = cable virtual channel table
  if ((sections.table_id != 0xc8 && sections.table_id != 0xc9) || m_bIsReady || m_pCallBack == NULL)
  {
    return;
  }
  byte* section = sections.Data;

  try
  {
    int sectionSyntaxIndicator = section[1] & 0x80;
    int sectionLength = ((section[1] & 0xf) << 8) + section[2];
    if ((sections.table_id == 0xc8 && sectionLength > 1021) || sectionLength > 4093 || sectionLength < 13)
    {
      LogDebug("LvctParser: invalid section length = %d", sectionLength);
      return;
    }
    int transportStreamId = (section[3] << 8) + section[4];
    int versionNumber = (section[5] >> 1) & 0x1f;
    int currentNextIndicator = section[5] & 1;
    if (currentNextIndicator == 0)
    {
      // Details do not apply yet...
      return;
    }
    int sectionNumber = section[6];
    int lastSectionNumber = section[7];

    int endOfSection = sectionLength - 1;
    LogDebug("LvctParser: table ID = 0x%x, transport stream ID = 0x%x, version = %d, section number = %d, last section number = %d, section length = %d, end of section = %d",
      sections.table_id, transportStreamId, versionNumber, sectionNumber, lastSectionNumber, sectionLength, endOfSection);

    if (versionNumber > m_iCurrentVersion || (versionNumber == MAX_TABLE_VERSION_NUMBER && versionNumber < m_iCurrentVersion))
    {
      LogDebug("LvctParser: new table version, table ID = 0x%x, version = %d, section number = %d, last section number = %d", sections.table_id, versionNumber, sectionNumber, lastSectionNumber);
      m_iCurrentVersion = versionNumber;
      m_mUnseenSections.clear();
      for (int s = 0; s <= lastSectionNumber; s++)
      {
        m_mUnseenSections.push_back(s);
      }
    }
    vector<int>::iterator sectionIt = find(m_mUnseenSections.begin(), m_mUnseenSections.end(), sectionNumber);
    if (sectionIt == m_mUnseenSections.end())
    {
      return;
    }

    int protocolVersion = section[8];
    int numChannelsInSection = section[9];
    //LogDebug("LvctParser: protocol version = %d, number of channels = %d", protocolVersion, numChannelsInSection);
    int pointer = 10;
    for (int i = 0; i < numChannelsInSection && pointer + 31 < endOfSection; i++)
    {
      // shortName = 7*16 bits (14 bytes), UTF-16 encoding
      char* name = new char[8];
      if (name == NULL)
      {
        LogDebug("LvctParser: failed to allocate 8 bytes for the short name");
      }
      else
      {
        for (int count = 0; count < 7; count++)
        {
          // Take every second byte as a rough ASCII conversion.
          pointer++; // skip the UTF-16 high byte
          name[count] = section[pointer++];
          name[count + 1] = 0; // NULL terminate
        }
        LogDebug("LvctParser: short name = %s", name);
      }

      // SCTE cable supports both one-part and two-part channel numbers where
      // the major and minor channel number range is 0..999.
      // ATSC supports only two-part channel numbers where the major range is
      // 1..99 and the minor range is 0..999.
      // When the minor channel number is 0 it indicates an analog channel.
      int majorChannel = ((section[pointer] & 0xf) << 6) + (section[pointer + 1] >> 2);
      pointer++;
      int minorChannel = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
      pointer += 2;
      if (((majorChannel >> 4) & 0x3f) == 0x3f)
      {
        majorChannel = ((majorChannel & 0xf) << 10) + minorChannel;
        minorChannel = 0;
      }

      // Frequency and modulation might be useful as part of an ATSC network
      // scan... except that the standard says that the frequency is deprecated
      // and should be set to zero. There is no other known ATSC NIT standard.
      int modulationMode = section[pointer++];
      unsigned int carrierFrequency = section[pointer++] << 24;  // Hz
      carrierFrequency += (section[pointer++] << 16);
      carrierFrequency += (section[pointer++] << 8);
      carrierFrequency += section[pointer++];

      int channelTsid = ((section[pointer]) << 8) + section[pointer + 1];
      pointer += 2;
      int programNumber = ((section[pointer]) << 8) + section[pointer + 1];
      pointer += 2;
      int etmLocation = (section[pointer] >> 6);
      bool accessControlled = ((section[pointer] >> 5) & 0x1);
      bool hidden = ((section[pointer] >> 4) & 0x1);

      // cable only
      int pathSelect = 0;
      bool outOfBand = false;
      if (sections.table_id == 0xc9)
      {
        pathSelect = ((section[pointer] >> 3) & 0x1);
        outOfBand = ((section[pointer] >> 2) & 0x1);
      }

      bool hideGuide = ((section[pointer++] >> 1) & 0x1);
      int serviceType = (section[pointer++] & 0x3f);
      int sourceId = ((section[pointer]) << 8) + section[pointer + 1];
      pointer += 2;
      LogDebug("LvctParser: major channel = %d, minor channel = %d, modulation = %d, carrier frequency = %d, channel TSID = 0x%x, program number = 0x%x, ETM location = %d, access controlled = %d, hidden = %d, path select = %d, out of band = %d, hide guide = %d, service type = 0x%x, source ID = 0x%x",
                  majorChannel, minorChannel, modulationMode, carrierFrequency, channelTsid, programNumber,
                  etmLocation, accessControlled, hidden, pathSelect, outOfBand, hideGuide, serviceType, sourceId);

      // descriptors...
      int descriptorsLength = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfDescriptors = pointer + descriptorsLength;
      LogDebug("LvctParser: pointer = %d, descriptors length = %d, end of descriptors = %d", pointer, descriptorsLength, endOfDescriptors);

      int videoStreamCount = 0;
      int audioStreamCount = 0;
      vector<unsigned int> languages;
      while (pointer + 1 < endOfDescriptors)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        LogDebug("LvctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, pointer + length);
        if (pointer + length > endOfDescriptors)
        {
          LogDebug("LvctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptors = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfDescriptors, endOfSection, sectionLength);
          if (name != NULL)
          {
            delete[] name;
            name = NULL;
          }
          return;
        }

        if (tag == 0xa0) // extended channel name descriptor
        {
          vector<char*> extendedNames;
          DecodeMultipleStrings(&section[pointer], length, &extendedNames);
          // Combine the first different extended name with the short name... carefully!
          int shortNameLength = 0;
          if (name != NULL)
          {
            shortNameLength = strlen(name);
          }
          for (size_t i = 0; i < extendedNames.size(); i++)
          {
            int extendedNameLength = strlen(extendedNames[i]);
            if (extendedNameLength == shortNameLength && strcmp(name, extendedNames[0]) == 0)
            {
              continue;
            }
            int nameBufferSize = 0;
            if (name != NULL)
            {
              nameBufferSize += strlen(" ()") + shortNameLength;
            }
            nameBufferSize += extendedNameLength + 1; // + 1 for NULL termination
            char* newName = new char[nameBufferSize];
            if (newName == NULL)
            {
              LogDebug("LvctParser: failed to allocate %d bytes for the extended name", nameBufferSize);
            }
            else
            {
              strcpy(newName, extendedNames[i]);
              if (name != NULL)
              {
                strcat(newName, " (");
                strcat(newName, name);
                strcat(newName, ")");
                delete[] name;
              }
              name = newName;
            }
            for (vector<char*>::iterator it = extendedNames.begin(); it != extendedNames.end(); it++)
            {
              delete[] *it;
            }
          }
        }
        else if (tag == 0xa1) // service location descriptor
        {
          DecodeServiceLocationDescriptor(&section[pointer], length, &videoStreamCount, &audioStreamCount, &languages);
        }
        pointer += length;
      }

      if (m_pCallBack != NULL)
      {
        m_pCallBack->OnLvctReceived(sections.table_id, name, majorChannel, minorChannel, modulationMode, carrierFrequency,
          channelTsid, programNumber, etmLocation, accessControlled, hidden, pathSelect, outOfBand, hideGuide, serviceType,
          sourceId, videoStreamCount, audioStreamCount, languages);
      }
      if (name != NULL)
      {
        delete[] name;
      }
    }

    // additional descriptors...
    int additionalDescriptorsLength = ((section[pointer] & 0x3) << 8) + section[pointer + 1];
    pointer += 2;
    int endOfDescriptors = pointer + additionalDescriptorsLength;
    LogDebug("LvctParser: pointer = %d, additional descriptors length = %d, end of descriptors = %d", pointer, additionalDescriptorsLength, endOfDescriptors);
    while (pointer + 1 < endOfDescriptors)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      LogDebug("LvctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", tag, length, pointer, pointer + length);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug("LvctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of descriptors = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfDescriptors, endOfSection, sectionLength);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("LvctParser: section parsing error");
      return;
    }

    m_mUnseenSections.erase(sectionIt);
    if (m_mUnseenSections.size() == 0)
    {
      m_bIsReady = true;
      m_pCallBack = NULL;
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
    int pcrPid = ((b[0] & 0x1f) << 8) + b[1];
    int numberElements = b[2];
    //LogDebug("LvctParser: PCR PID = 0x%x, number of elements = %d", pcrPid, numberElements);
    int pointer = 3;
    for (int i = 0; i < numberElements && pointer + 5 < length; i++)
    {
      int streamType = b[pointer++];
      int elementaryPid = ((b[pointer] & 0x1f) << 8) + b[pointer + 1];
      pointer += 2;
      unsigned int iso639LanguageCode = b[pointer] + (b[pointer + 1] << 8) + (b[pointer + 2] << 16);
      if (iso639LanguageCode != 0)
      {
        languages->push_back(iso639LanguageCode);
      }
      pointer += 3;
      //LogDebug("LvctParser: stream type = 0x%x, elementary PID = 0x%x", streamType, elementaryPid);

      if (streamType == SERVICE_TYPE_VIDEO_MPEG1 ||
          streamType == SERVICE_TYPE_VIDEO_MPEG2 ||
          streamType == SERVICE_TYPE_VIDEO_MPEG4 ||
          streamType == SERVICE_TYPE_VIDEO_H264 ||
          streamType == SERVICE_TYPE_VIDEO_HEVC ||
          streamType == SERVICE_TYPE_VIDEO_MPEG2_DCII)
      {
        (*videoStreamCount)++;
      }
      else if (streamType == SERVICE_TYPE_AUDIO_MPEG1 ||
          streamType == SERVICE_TYPE_AUDIO_MPEG2 ||
          streamType == SERVICE_TYPE_AUDIO_AAC ||
          streamType == SERVICE_TYPE_AUDIO_LATM_AAC ||
          streamType == SERVICE_TYPE_AUDIO_AC3 ||
          streamType == SERVICE_TYPE_AUDIO_E_AC3)
      {
        (*audioStreamCount)++;
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
    int numberStrings = b[0];

    //LogDebug("LvctParser: parse multiple strings, number of strings = %d", numberStrings);
    int pointer = 1;
    for (int i = 0; i < numberStrings && pointer + 3 < length; i++)
    {
      unsigned int iso639LanguageCode = b[pointer] + (b[pointer + 1] << 8) + (b[pointer + 2] << 16);
      pointer += 3;
      int numberSegments = b[pointer++];
      //LogDebug("LvctParser: string %d, number of segments = %d", i, numberSegments);
      for (int j = 0; j < numberSegments && pointer + 2 < length; j++)
      {
        int compressionType = b[pointer++];
        int mode = b[pointer++];
        int numberBytes = b[pointer++];
        //LogDebug("LvctParser: segment %d, compression type = 0x%x, mode = 0x%x, number of bytes = %d", j, compressionType, mode, numberBytes);
        if (pointer + numberBytes > length)
        {
          LogDebug("LvctParser: invalid string length %d in multiple string structure, pointer = %d, number of bytes = %d, structure length = %d", pointer, numberBytes, length);
          return;
        }

        char* string = NULL;
        DecodeString(&b[pointer], compressionType, mode, numberBytes, &string);
        if (string != NULL)
        {
          strings->push_back(string);
        }

        pointer += numberBytes;
      }
    }
  }
  catch (...)
  {
    LogDebug("LvctParser: unhandled exception in ParseMultipleStrings()");
  }
}

void CLvctParser::DecodeString(byte* b, int compressionType, int mode, int numberBytes, char** string)
{
  //LogDebug("LvctParser: decode string, compression type = 0x%x, mode = 0x%x, number of bytes = %d", compressionType, mode, numberBytes);
  if (compressionType == 0 && mode == 0)
  {
    *string = new char[numberBytes + 1];
    if (*string == NULL)
    {
      LogDebug("LvctParser: failed to allocate %d bytes in DecodeString()", numberBytes + 1);
      return;
    }
    memcpy(*string, b, numberBytes);
    (*string)[numberBytes] = 0; // NULL terminate
    return;
  }
  LogDebug("LvctParser: unsupported compression type or mode in DecodeString(), compression type = 0x%x, mode = 0x%x", compressionType, mode);
  for (int i = 0; i < numberBytes; i++)
  {
    LogDebug(" %d: 0x%x", i, b[i]);
  }
}