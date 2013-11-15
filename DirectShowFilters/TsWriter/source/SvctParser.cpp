/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include <algorithm>
#include "SvctParser.h"

#define VCM_STID 0
#define DCM_STID 1
#define ICM_STID 2
#define MIN_STID VCM_STID
#define MAX_STID ICM_STID

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CSvctParser::CSvctParser(void)
{
  SetPid(0x1ffc);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    m_mCurrentVersions[t] = -1;
    m_mUnseenSections[t] = new vector<int>();
  }
  Reset();
  m_pCallBack = NULL;
}

CSvctParser::~CSvctParser(void)
{
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    delete m_mUnseenSections[t];
  }
}

void CSvctParser::SetCallBack(ISvctCallBack* callBack)
{
  m_pCallBack = callBack;
}

bool CSvctParser::IsReady()
{
  return m_bIsReady;
}

void CSvctParser::Reset()
{
  LogDebug("SvctParser: reset");
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    m_mCurrentVersions[t] = -1;
    m_mUnseenSections[t]->clear();
  }
  m_mSeenVirtualChannels.clear();
  m_mDefinedChannelMap.clear();
  m_bIsReady = false;
  CSectionDecoder::Reset();
  LogDebug("SvctParser: reset done");
}

void CSvctParser::OnNewSection(CSection& sections)
{
  if (sections.table_id != 0xc4 || m_bIsReady || m_pCallBack == NULL)
  {
    return;
  }
  if (sections.section_length < 8)
  {
    LogDebug("SvctParser: invalid section size %d, expected at least 11 bytes", sections.section_length);
  }
  byte* section = sections.Data;

  try
  {
    int sectionLength = ((section[1] & 0x0f) << 8) + section[2];
    if (sections.section_length != sectionLength)
    {
      LogDebug("SvctParser: invalid section length = %d, byte count = %d", sectionLength, sections.section_length);
      return;
    }
    int protocolVersion = (section[3] & 0x1f);
    int transmissionMedium = (section[4] >> 4);
    int tableSubtype = (section[4] & 0xf);
    int vctId = (section[5] << 8) + section[6];

    LogDebug("SvctParser: section length = %d, protocol version = %d, transmission medium = %d, table subtype = %d, VCT ID = 0x%x", sectionLength, protocolVersion, transmissionMedium, tableSubtype, vctId);

    int pointer = 7;
    int subtableLength = 0;
    int endOfSection = sectionLength - 1;
    bool success = true;
    switch (tableSubtype)
    {
      case VCM_STID:
        success = DecodeVirtualChannelMap(section, pointer, endOfSection, transmissionMedium, vctId);
        break;
      case DCM_STID:
        success = DecodeDefinedChannelMap(section, pointer, endOfSection);
        break;
      case ICM_STID:
        success = DecodeInverseChannelMap(section, pointer, endOfSection);
        break;
      default:
        LogDebug("SvctParser: unsupported table subtype %d", tableSubtype);
        return;   // attempting to parse descriptors after skipping the table would be bad!
    }
    if (!success)
    {
      return;
    }

    bool seenRevisionDescriptor = false;
    while (pointer + 1 < endOfSection)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      LogDebug("SvctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", tag, length, pointer, pointer + length, endOfSection, sectionLength);
      if (pointer + length > endOfSection)
      {
        LogDebug("SvctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfSection, sectionLength);
        return;
      }

      if (tag == 0x93)  // revision detection descriptor
      {
        DecodeRevisionDetectionDescriptor(&section[pointer], length, tableSubtype);
        seenRevisionDescriptor = true;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("SvctParser: section parsing error");
    }

    // Second method of detecting complete tables, for when revision detection
    // descriptors are not used.
    if (!seenRevisionDescriptor && m_mSeenVirtualChannels.size() != 0 && m_mDefinedChannelMap.size() == m_mSeenVirtualChannels.size())
    {
      LogDebug("SvctParser: ready, virtual channel count = defined channel count");
      m_bIsReady = true;
      m_pCallBack = NULL;
    }
  }
  catch (...)
  {
    LogDebug("SvctParser: unhandled exception in OnNewSection()");
  }
}

bool CSvctParser::DecodeVirtualChannelMap(byte* section, int& pointer, int endOfSection, int transmissionMedium, int vctId)
{
  if (pointer + 7 > endOfSection)
  {
    LogDebug("SvctParser: corruption detected at virtual channel map, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  bool freqSpecIncluded = ((section[pointer] & 0x80) != 0);
  bool symbolRateIncluded = ((section[pointer] & 0x40) != 0);
  bool descriptorsIncluded = ((section[pointer++] & 0x20) != 0);
  bool splice = ((section[pointer++] & 0x80) != 0);
  unsigned int activationTime = 0;
  for (byte b = 0; b < 4; b++)
  {
    activationTime = activationTime << 8;
    activationTime = section[pointer++];
  }
  byte numberOfVcRecords = section[pointer++];
  LogDebug("SvctParser: virtual channel map, transmission medium = %d, freq. spec. included = %d, symbol rate included = %d, descriptors included = %d, splice = %d, activation time = %d, number of VC records = %d",
    transmissionMedium, freqSpecIncluded, symbolRateIncluded, descriptorsIncluded, splice, activationTime, numberOfVcRecords);

  for (byte i = 0; i < numberOfVcRecords; i++)
  {
    if (pointer + 9 > endOfSection)
    {
      LogDebug("SvctParser: detected number of virtual channel records %d is invalid, pointer = %d, end of section = %d, loop = %d", numberOfVcRecords, pointer, endOfSection, i);
      return false;
    }

    int virtualChannelNumber = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
    pointer += 2;
    m_mSeenVirtualChannels[virtualChannelNumber] = true;
    bool applicationVirtualChannel = ((section[pointer] & 0x80) != 0);
    int bitstreamSelect = ((section[pointer] & 0x40) >> 6);   // broadcast reserved
    int pathSelect = ((section[pointer] & 0x20) >> 5);        // satellite, SMATV, broadcast reserved
    int transportType = ((section[pointer] & 0x10) >> 4);
    int channelType = (section[pointer++] & 0x0f);
    int sourceId = (section[pointer] << 8) + section[pointer + 1];
    pointer += 2;
    LogDebug("SvctParser: virtual channel number = %d, application virtual channel = %d, bitstream select = %d, path select = %d, transport type = %d, channel type = %d, source ID = 0x%x",
      virtualChannelNumber, applicationVirtualChannel, bitstreamSelect, pathSelect, transportType, channelType, sourceId);

    if (channelType == 3) // NVOD access
    {
      int nvodChannelBase = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      pointer += 2;
      if (transmissionMedium == 3)  // SMATV
      {
        pointer += 3;
      }
      else if (transmissionMedium != 4) // not over the air
      {
        pointer += 2;
      }
      LogDebug("SvctParser: NVOD channel base = 0x%x", nvodChannelBase);
    }
    else
    {
      switch (transmissionMedium)
      {
        case 1: // satellite
          if (transportType == 0) // MPEG 2
          {
            byte satellite = section[pointer++];
            int transponder = (section[pointer++] & 0x3f);
            int programNumber = (section[pointer] << 8) + section[pointer + 1];
            pointer += 2;
            LogDebug("SvctParser: satellite = %d, transponder = %d, program number = 0x%x", satellite, transponder, programNumber);
          }
          else
          {
            byte satellite = section[pointer++];
            int transponder = (section[pointer++] & 0x3f);
            pointer += 2;
            LogDebug("SvctParser: satellite = %d, transponder = %d", satellite, transponder);
          }
          break;
        case 3: // SMATV
          if (transportType == 0) // MPEG 2
          {
            byte cdsReference = section[pointer++];
            int programNumber = (section[pointer] << 8) + section[pointer + 1];
            pointer += 2;
            byte mmsReference = section[pointer++];
            pointer++;
            LogDebug("SvctParser: CDS reference = %d, program number = 0x%x, MMS reference = %d", cdsReference, programNumber, mmsReference);
          }
          else
          {
            byte cdsReference = section[pointer++];
            bool scrambled = ((section[pointer] & 0x80) != 0);
            int videoStandard = (section[pointer++] & 0x0f);
            bool isWideBandwidthVideo = ((section[pointer] & 0x80) != 0);
            int waveformStandard = (section[pointer++] & 0x1f);
            bool isWideBandwidthAudio = ((section[pointer] & 0x80) != 0);
            bool isCompandedAudio = ((section[pointer] & 0x40) != 0);
            int matrixMode = ((section[pointer] >> 4) & 0x03);
            int subcarrier2Offset = 10 * (((section[pointer] & 0x0f) << 6) + (section[pointer + 1] >> 2));  // kHz
            pointer++;
            int subcarrier1Offset = 10 * (((section[pointer] & 0x03) << 8) + section[pointer + 1]);
            pointer += 2;
            LogDebug("SvctParser: CDS reference = %d, scrambled = %d, video standard = %d, is WB video = %d, waveform standard = %d, is WB audio = %d, is companded audio = %d, matrix mode = %d, subcarrier 2 offset = %d kHz, subcarrier 1 offset = %d kHz",
              cdsReference, scrambled, videoStandard, isWideBandwidthVideo, waveformStandard, isWideBandwidthAudio,
              isCompandedAudio, matrixMode, subcarrier2Offset, subcarrier1Offset);
          }
          break;
        case 4: // over the air
          if (transportType == 0) // MPEG 2
          {
            int programNumber = (section[pointer] << 8) + section[pointer + 1];
            pointer += 2;
            LogDebug("SvctParser: program number = 0x%x", programNumber);
          }
          else
          {
            bool scrambled = ((section[pointer] & 0x80) != 0);
            int videoStandard = (section[pointer++] & 0x0f);
            pointer++;
            LogDebug("SvctParser: scrambled = %d, video standard = %d", scrambled, videoStandard);
          }
          break;
        case 0: // cable
        case 2: // MMDS
          if (transportType == 0) // MPEG 2
          {
            byte cdsReference = section[pointer++];
            int programNumber = (section[pointer] << 8) + section[pointer + 1];
            pointer += 2;
            byte mmsReference = section[pointer++];
            LogDebug("SvctParser: CDS reference = %d, program number = 0x%x, MMS reference = %d", cdsReference, programNumber, mmsReference);
            if (m_pCallBack != NULL)
            {
              m_pCallBack->OnSvctReceived(transmissionMedium, vctId, virtualChannelNumber, applicationVirtualChannel, bitstreamSelect,
                pathSelect, channelType, sourceId, cdsReference, programNumber, mmsReference);
            }
          }
          else
          {
            byte cdsReference = section[pointer++];
            bool scrambled = ((section[pointer] & 0x80) != 0);
            int videoStandard = (section[pointer++] & 0x0f);
            pointer += 2;
            LogDebug("SvctParser: CDS reference = %d, scrambled = %d, video standard = %d", cdsReference, scrambled, videoStandard);
          }
          break;
        default:
          LogDebug("SvctParser: unsupported transmission medium %d", transmissionMedium);
          return false;
      }
    }

    if (freqSpecIncluded || transmissionMedium == 4)  // over the air
    {
      int frequencyUnit = 10; // kHz
      if ((section[pointer] & 0x80) != 0)
      {
        frequencyUnit = 125;  // kHz
      }
      int carrierFrequency = frequencyUnit * (((section[pointer] & 0x7f) << 8) + section[pointer + 1]);  // kHz
      pointer += 2;
      LogDebug("SvctParser: frequency, unit = %d kHz, carrier = %d kHz", frequencyUnit, carrierFrequency);
    }
    if (symbolRateIncluded && transmissionMedium != 4)  // not over the air
    {
      // s/s
      int symbolRate = ((section[pointer] & 0x0f) << 24) + (section[pointer + 1] << 16) + (section[pointer + 2] << 8) + section[pointer + 3];
      pointer += 4;
      LogDebug("SvctParser: symbol rate = %d s/s", symbolRate);
    }
    if (descriptorsIncluded)
    {
      if (pointer >= endOfSection)
      {
        LogDebug("SvctParser: invalid section length at virtual channel map descriptor count, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
        return false;
      }
      byte descriptorCount = section[pointer++];
      for (byte d = 0; d < descriptorCount; d++)
      {
        if (pointer + 2 > endOfSection)
        {
          LogDebug("SvctParser: detected virtual channel map descriptor count %d is invalid, pointer = %d, end of section = %d, loop = %d, inner loop = %d", descriptorCount, pointer, endOfSection, i, d);
          return false;
        }
        byte tag = section[pointer++];
        byte length = section[pointer++];
        LogDebug("SvctParser: virtual channel map descriptor, tag = 0x%x, length = %d", tag, length);
        if (pointer + length > endOfSection)
        {
          LogDebug("SvctParser: invalid virtual channel map descriptor length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", length, pointer, endOfSection, i, d);
          return false;
        }
        pointer += length;
      }
    }
  }
  return true;
}

bool CSvctParser::DecodeDefinedChannelMap(byte* section, int& pointer, int endOfSection)
{
  if (pointer + 3 > endOfSection)
  {
    LogDebug("SvctParser: corruption detected at defined channel map, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  int firstVirtualChannel = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
  pointer += 2;
  int dcmDataLength = (section[pointer++] & 0x7f);
  if (pointer + dcmDataLength > endOfSection)
  {
    LogDebug("SvctParser: invalid defined channel map data length %d, pointer = %d, end of section = %d", dcmDataLength, pointer, endOfSection);
    return false;
  }
  LogDebug("SvctParser: defined channel map, first virtual channel = %d, DCM data length = %d", firstVirtualChannel, dcmDataLength);
  int currentChannel = firstVirtualChannel;
  for (int i = 0; i < dcmDataLength; i++)
  {
    bool rangeDefined = ((section[pointer] & 0x80) != 0);
    int channelsCount = (section[pointer++] & 0x7f);
    LogDebug("SvctParser: range defined = %d, channels count = %d", rangeDefined, channelsCount);
    if (rangeDefined)
    {
      for (int c = 0; c < channelsCount; c++)
      {
        m_mDefinedChannelMap[currentChannel++] = true;
      }
    }
    else
    {
      currentChannel += channelsCount;
    }
  }
  return true;
}

bool CSvctParser::DecodeInverseChannelMap(byte* section, int& pointer, int endOfSection)
{
  if (pointer + 3 > endOfSection)
  {
    LogDebug("SvctParser: corruption detected at inverse channel map, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  int firstMapIndex = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
  pointer += 2;
  int recordCount = (section[pointer++] & 0x7f);
  if (pointer + (recordCount * 4) > endOfSection)
  {
    LogDebug("SvctParser: invalid inverse channel map record count %d, pointer = %d, end of section = %d", recordCount, pointer, endOfSection);
    return false;
  }
  LogDebug("SvctParser: inverse channel map, first map index = %d, record count = %d", firstMapIndex, recordCount);
  for (int i = 0; i < recordCount; i++)
  {
    int sourceId = (section[pointer] << 8) + section[pointer + 1];
    pointer += 2;
    int virtualChannelNumber = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
    pointer += 2;
    LogDebug("SvctParser: source ID = 0x%x, virtual channel number = %d", sourceId, virtualChannelNumber);
  }
  return true;
}

void CSvctParser::DecodeRevisionDetectionDescriptor(byte* b, int length, int tableSubtype)
{
  if (length != 3)
  {
    LogDebug("SvctParser: invalid revision detection descriptor length %d", length);
    return;
  }

  int currentVersion = m_mCurrentVersions[tableSubtype];
  vector<int>* unseenSections = m_mUnseenSections[tableSubtype];

  int versionNumber = (b[0] & 0x1f);
  int sectionNumber = b[1];
  int lastSectionNumber = b[2];
  LogDebug("SvctParser: revision detection descriptor, table subtype = %d, version = %d, section number = %d, last section number = %d", tableSubtype, versionNumber, sectionNumber, lastSectionNumber);
  if (versionNumber > currentVersion || (versionNumber == MAX_TABLE_VERSION_NUMBER && versionNumber < currentVersion))
  {
    LogDebug("SvctParser: new table version");
    m_bIsReady = false;
    m_mCurrentVersions[tableSubtype] = versionNumber;
    unseenSections->clear();
    for (int s = 0; s <= lastSectionNumber; s++)
    {
      unseenSections->push_back(s);
    }
  }
  else
  {
    LogDebug("SvctParser: existing table version, unseen section count = %d", unseenSections->size());
  }

  vector<int>::iterator it = find(unseenSections->begin(), unseenSections->end(), sectionNumber);
  if (it != unseenSections->end())
  {
    unseenSections->erase(it);
    if (unseenSections->size() == 0)
    {
      bool isReady = true;
      for (int t = MIN_STID; t <= MAX_STID; t++)
      {
        if (m_mUnseenSections[t]->size() != 0)
        {
          isReady = false;
          break;
        }
      }
      if (isReady)
      {
        LogDebug("SvctParser: ready, revision detection");
        m_bIsReady = isReady;
        m_pCallBack = NULL;
      }
    }
  }
}