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
#include <Windows.h>
#include <algorithm>
#include <bdatypes.h>
#include "AtscNitParser.h"

#define CARRIER_DEFINITION_STID 1
#define MODULATION_MODE_STID 2
#define SATELLITE_INFO_STID 3
#define TRANSPONDER_DATA_STID 4
#define MIN_STID CARRIER_DEFINITION_STID
#define MAX_STID TRANSPONDER_DATA_STID

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CAtscNitParser::CAtscNitParser(void)
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
}

CAtscNitParser::~CAtscNitParser(void)
{
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    delete m_mUnseenSections[t];
  }
}

bool CAtscNitParser::IsReady()
{
  return m_bIsReady;
}

void CAtscNitParser::Reset()
{
  LogDebug("AtscNitParser: reset");
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    m_mCurrentVersions[t] = -1;
    m_mUnseenSections[t]->clear();
  }
  m_bIsReady = false;
  CSectionDecoder::Reset();
  LogDebug("AtscNitParser: reset done");
}

int CAtscNitParser::GetCarrierDefinition(int reference)
{
  map<int, int>::iterator it = m_mCarrierDefinitions.find(reference);
  if (it == m_mCarrierDefinitions.end())
  {
    return -1;
  }
  return it->second;
}

int CAtscNitParser::GetModulationMode(int reference)
{
  map<int, int>::iterator it = m_mModulationModes.find(reference);
  if (it == m_mModulationModes.end())
  {
    return BDA_MOD_NOT_SET;
  }
  return it->second;
}

void CAtscNitParser::OnNewSection(CSection& sections)
{
  if (sections.table_id != 0xc2 || m_bIsReady)
  {
    return;
  }
  if (sections.section_length < 8)
  {
    LogDebug("AtscNitParser: invalid section size %d, expected at least 11 bytes", sections.section_length);
    return;
  }
  byte* section = sections.Data;

  try
  {
    int sectionLength = ((section[1] & 0xf) << 8) + section[2];
    if (sections.section_length != sectionLength)
    {
      LogDebug("AtscNitParser: invalid section length = %d, byte count = %d", sectionLength, sections.section_length);
      return;
    }
    int protocolVersion = (section[3] & 0x1f);
    byte firstIndex = section[4];
    byte numberOfRecords = section[5];
    int transmissionMedium = (section[6] >> 4);
    int tableSubtype = (section[6] & 0x0f);

    int pointer = 7;
    int endOfSection = sectionLength - 1;

    byte satelliteId = 0;
    if (tableSubtype == TRANSPONDER_DATA_STID)
    {
      if (pointer >= endOfSection)
      {
        LogDebug("AtscNitParser: invalid section length at satellite ID, pointer = %d, end of section = %d", pointer, endOfSection);
        return;
      }
      satelliteId = section[pointer++];
    }
    LogDebug("AtscNitParser: section length = %d, protocol version = %d, first index = %d, number of records = %d, transmission medium = %d, table subtype = %d, satellite ID = %d",
      sectionLength, protocolVersion, firstIndex, numberOfRecords, transmissionMedium, tableSubtype, satelliteId);

    for (byte i = 0; i < numberOfRecords; i++)
    {
      switch (tableSubtype)
      {
        case CARRIER_DEFINITION_STID:
          DecodeCarrierDefinition(section, pointer, endOfSection, firstIndex, transmissionMedium);
          break;
        case MODULATION_MODE_STID:
          DecodeModulationMode(section, pointer, endOfSection, firstIndex, transmissionMedium);
          break;
        case SATELLITE_INFO_STID:
          DecodeSatelliteInformation(section, pointer, endOfSection);
          break;
        case TRANSPONDER_DATA_STID:
          DecodeTransponderData(section, pointer, endOfSection);
          break;
        default:
          LogDebug("AtscNitParser: unsupported table subtype %d", tableSubtype);
          return;
      }

      // table descriptors
      if (pointer >= endOfSection)
      {
        LogDebug("AtscNitParser: invalid section length at table descriptor count, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
        return;
      }
      byte descriptorCount = section[pointer++];
      for (byte d = 0; d < descriptorCount; d++)
      {
        if (pointer + 2 > endOfSection)
        {
          LogDebug("AtscNitParser: detected table descriptor count %d is invalid, pointer = %d, end of section = %d, loop = %d, inner loop = %d", descriptorCount, pointer, endOfSection, i, d);
          return;
        }
        byte tag = section[pointer++];
        byte length = section[pointer++];
        LogDebug("AtscNitParser: table descriptor, tag = 0x%x, length = %d", tag, length);
        if (pointer + length > endOfSection)
        {
          LogDebug("AtscNitParser: invalid table descriptor length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", length, pointer, endOfSection, i, d);
          return;
        }
        pointer += length;
      }
    }

    bool seenRevisionDescriptor = false;
    while (pointer + 1 < endOfSection)
    {
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("AtscNitParser: descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("AtscNitParser: invalid descriptor length %d, pointer = %d, end of section = %d", length, pointer, endOfSection);
        return;
      }

      if (tag == 0x93)
      {
        DecodeRevisionDetectionDescriptor(&section[pointer], length, tableSubtype);
        seenRevisionDescriptor = true;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("AtscNitParser: corruption detected at end of section, pointer = %d, end of section = %d", pointer, endOfSection);
      return;
    }

    // Second method of detecting complete tables, for when revision detection
    // descriptors are not used.
    if (!seenRevisionDescriptor && m_mCarrierDefinitions.size() != 0 && m_mModulationModes.size() != 0)
    {
      LogDebug("AtscNitParser: ready, seen carrier definitions and modulation modes");
      m_bIsReady = true;
    }
  }
  catch (...)
  {
    LogDebug("AtscNitParser: unhandled exception in OnNewSection()");
  }
}

bool CAtscNitParser::DecodeCarrierDefinition(byte* section, int& pointer, int endOfSection, byte& firstIndex, int transmissionMedium)
{
  if (pointer + 5 > endOfSection)
  {
    LogDebug("AtscNitParser: corruption detected at carrier definition, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }
  byte numberOfCarriers = section[pointer++];
  int spacingUnit = 10;   // kHz
  if ((section[pointer] & 0x80) != 0)
  {
    spacingUnit = 125;    // kHz
  }
  int frequencySpacing = spacingUnit * (((section[pointer] & 0x3f) << 8) + section[pointer + 1]);   // kHz
  pointer += 2;

  int frequencyUnit = 10; // kHz
  if ((section[pointer] & 0x80) != 0)
  {
    frequencyUnit = 125;  // kHz
  }
  int firstCarrierFrequency = frequencyUnit * (((section[pointer] & 0x7f) << 8) + section[pointer + 1]);  // kHz
  pointer += 2;
  LogDebug("AtscNitParser: carrier definition, number of carriers = %d, spacing unit = %d kHz, frequency spacing = %d kHz, frequency unit = %d kHz, first carrier frequency = %d kHz",
    numberOfCarriers, spacingUnit, frequencySpacing, frequencyUnit, firstCarrierFrequency);

  int carrierFrequency = firstCarrierFrequency;
  for (byte f = 0; f < numberOfCarriers; f++)
  {
    m_mCarrierDefinitions[firstIndex++] = carrierFrequency;
    carrierFrequency += frequencySpacing;
  }
  return true;
}

bool CAtscNitParser::DecodeModulationMode(byte* section, int& pointer, int endOfSection, byte& firstIndex, int transmissionMedium)
{
  if (pointer + 6 > endOfSection)
  {
    LogDebug("AtscNitParser: corruption detected at modulation mode, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }
  int transmissionSystem = (section[pointer] >> 4);
  int innerCodingMode = BDA_BCC_RATE_NOT_DEFINED;
  switch (section[pointer] & 0x0f)
  {
    case 0:
      innerCodingMode = BDA_BCC_RATE_5_11;
      break;
    case 1:
      innerCodingMode = BDA_BCC_RATE_1_2;
      break;
    case 3:
      innerCodingMode = BDA_BCC_RATE_3_5;
      break;
    case 5:
      innerCodingMode = BDA_BCC_RATE_2_3;
      break;
    case 7:
      innerCodingMode = BDA_BCC_RATE_3_4;
      break;
    case 8:
      innerCodingMode = BDA_BCC_RATE_4_5;
      break;
    case 9:
      innerCodingMode = BDA_BCC_RATE_5_6;
      break;
    case 11:
      innerCodingMode = BDA_BCC_RATE_7_8;
      break;
    case 15:
      // concatenated coding not used
      innerCodingMode = BDA_BCC_RATE_NOT_SET;
      break;
  }
  pointer++;

  bool isSplitBitstreamMode = ((section[pointer] & 0x80) != 0);
  ModulationType modulationFormat = BDA_MOD_NOT_SET;
  switch (section[pointer] & 0x1f)
  {
    case 1:
      modulationFormat = BDA_MOD_QPSK;
      break;
    case 2:
      modulationFormat = BDA_MOD_BPSK;
      break;
    case 3:
      modulationFormat = BDA_MOD_OQPSK;
      break;
    case 4:
      modulationFormat = BDA_MOD_8VSB;
      break;
    case 5:
      modulationFormat = BDA_MOD_16VSB;
      break;
    case 6:
      modulationFormat = BDA_MOD_16QAM;
      break;
    case 7:
      modulationFormat = BDA_MOD_32QAM;
      break;
    case 8:
      modulationFormat = BDA_MOD_64QAM;
      break;
    case 9:
      modulationFormat = BDA_MOD_80QAM;
      break;
    case 10:
      modulationFormat = BDA_MOD_96QAM;
      break;
    case 11:
      modulationFormat = BDA_MOD_112QAM;
      break;
    case 12:
      modulationFormat = BDA_MOD_128QAM;
      break;
    case 13:
      modulationFormat = BDA_MOD_160QAM;
      break;
    case 14:
      modulationFormat = BDA_MOD_192QAM;
      break;
    case 15:
      modulationFormat = BDA_MOD_224QAM;
      break;
    case 16:
      modulationFormat = BDA_MOD_256QAM;
      break;
    case 17:
      modulationFormat = BDA_MOD_320QAM;
      break;
    case 18:
      modulationFormat = BDA_MOD_384QAM;
      break;
    case 19:
      modulationFormat = BDA_MOD_448QAM;
      break;
    case 20:
      modulationFormat = BDA_MOD_512QAM;
      break;
    case 21:
      modulationFormat = BDA_MOD_640QAM;
      break;
    case 22:
      modulationFormat = BDA_MOD_768QAM;
      break;
    case 23:
      modulationFormat = BDA_MOD_896QAM;
      break;
    case 24:
      modulationFormat = BDA_MOD_1024QAM;
      break;
  }
  pointer++;

  // s/s
  int symbolRate = ((section[pointer] & 0x0f) << 24) + (section[pointer + 1] << 16) + (section[pointer + 2] << 8) + section[pointer + 3];
  pointer += 4;
  LogDebug("AtscNitParser: modulation mode, transmission system = %d, inner coding mode = %d, is split bitstream mode = %d, modulation format = %d, symbol rate = %d s/s",
    transmissionSystem, innerCodingMode, isSplitBitstreamMode, modulationFormat, symbolRate);

  m_mModulationModes[firstIndex++] = modulationFormat;
  return true;
}

bool CAtscNitParser::DecodeSatelliteInformation(byte* section, int&pointer, int endOfSection)
{
  if (pointer + 4 > endOfSection)
  {
    LogDebug("AtscNitParser: corruption detected at satellite information, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }
  byte satelliteId = section[pointer++];
  bool youAreHere = ((section[pointer] & 0x80) != 0);
  int frequencyBand = ((section[pointer] >> 5) & 0x03);
  bool outOfService = ((section[pointer] & 0x10) != 0);
  bool isEasternHemisphere = ((section[pointer] & 0x08) != 0);
  int orbitalPosition = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
  pointer += 2;
  bool isCircularPolarisation = ((section[pointer] & 0x80) != 0);
  int numberOfTransponders = (section[pointer++] & 0x3f) + 1;
  LogDebug("AtscNitParser: satellite information, satellite ID = %d, you are here = %d, frequency band = %d, out of service = %d, is Eastern hemisphere = %d, orbital position = %d, is circular polarisation = %d, number of transponders = %d",
    satelliteId, youAreHere, frequencyBand, outOfService, isEasternHemisphere, orbitalPosition, isCircularPolarisation, numberOfTransponders);
  return true;
}

bool CAtscNitParser::DecodeTransponderData(byte* section, int& pointer, int endOfSection)
{
  if (pointer + 6 > endOfSection)
  {
    LogDebug("AtscNitParser: corruption detected at transponder data, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }
  bool isMpeg2Transport = ((section[pointer] & 0x80) == 0);
  bool isVerticalRightPolarisation = ((section[pointer] & 0x40) != 0);
  int transponderNumber = (section[pointer++] & 0x3f);
  byte cdsReference = section[pointer++];
  LogDebug("AtscNitParser: transponder data, is MPEG 2 transport = %d, is vertical/right polarisation = %d, transponder number = %d, CDS reference = %d",
    isMpeg2Transport, isVerticalRightPolarisation, transponderNumber, cdsReference);
  if (isMpeg2Transport)
  {
    byte mmsReference = section[pointer++];
    int vctId = (section[pointer] << 8) + section[pointer + 1];
    pointer += 2;
    bool isRootTransponder = ((section[pointer++] & 0x80) != 0);
    LogDebug("AtscNitParser: MPEG 2 transponder data, MMS reference = %d, VCT ID = 0x%x, is root transponder = %d", mmsReference, vctId, isRootTransponder);
  }
  else
  {
    bool isWideBandwidthVideo = ((section[pointer] & 0x80) != 0);
    int waveformStandard = (section[pointer++] & 0x1f);
    bool isWideBandwidthAudio = ((section[pointer] & 0x80) != 0);
    bool isCompandedAudio = ((section[pointer] & 0x40) != 0);
    int matrixMode = ((section[pointer] >> 4) & 0x03);
    int subcarrier2Offset = 10 * (((section[pointer] & 0x0f) << 6) + (section[pointer + 1] >> 2));  // kHz
    pointer++;
    int subcarrier1Offset = 10 * (((section[pointer] & 0x03) << 8) + section[pointer + 1]);
    pointer += 2;
    LogDebug("AtscNitParser: non-MPEG 2 transponder data, is WB video = %d, waveform standard = %d, is WB audio = %d, is companded audio = %d, matrix mode = %d, subcarrier 2 offset = %d kHz, subcarrier 1 offset = %d kHz",
      isWideBandwidthVideo, waveformStandard, isWideBandwidthAudio, isCompandedAudio, matrixMode, subcarrier2Offset, subcarrier1Offset);
  }
  return true;
}

void CAtscNitParser::DecodeRevisionDetectionDescriptor(byte* b, int length, int tableSubtype)
{
  if (length != 3)
  {
    LogDebug("AtscNitParser: invalid revision detection descriptor length %d", length);
    return;
  }

  int currentVersion = m_mCurrentVersions[tableSubtype];
  vector<int>* unseenSections = m_mUnseenSections[tableSubtype];

  int versionNumber = (b[0] & 0x1f);
  int sectionNumber = b[1];
  int lastSectionNumber = b[2];
  LogDebug("AtscNitParser: revision detection descriptor, table subtype = %d, version = %d, section number = %d, last section number = %d", tableSubtype, versionNumber, sectionNumber, lastSectionNumber);
  if (versionNumber > currentVersion || (versionNumber == MAX_TABLE_VERSION_NUMBER && versionNumber < currentVersion))
  {
    LogDebug("AtscNitParser: new table version");
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
    LogDebug("AtscNitParser: existing table version, unseen section count = %d", unseenSections->size());
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
        LogDebug("AtscNitParser: ready, revision detection");
        m_bIsReady = isReady;
      }
    }
  }
}
