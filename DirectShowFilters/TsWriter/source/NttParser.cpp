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
#include <algorithm>
#include "NttParser.h"

#define TRANSPONDER_NAME_STID 1
#define SATELLITE_TEXT_STID 2
#define RATINGS_TEXT_STID 3
#define RATING_SYSTEM_STID 4
#define CURRENCY_SYSTEM_STID 5
#define SOURCE_NAME_STID 6
#define MAP_NAME_STID 7
#define MIN_STID TRANSPONDER_NAME_STID
#define MAX_STID MAP_NAME_STID

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CNttParser::CNttParser(void)
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

CNttParser::~CNttParser(void)
{
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    delete m_mUnseenSections[t];
  }
}

void CNttParser::SetCallBack(INttCallBack* callBack)
{
  m_pCallBack = callBack;
}

bool CNttParser::IsReady()
{
  return m_bIsReady;
}

void CNttParser::Reset()
{
  LogDebug("NttParser: reset");
  for (int t = MIN_STID; t <= MAX_STID; t++)
  {
    m_mCurrentVersions[t] = -1;
    m_mUnseenSections[t]->clear();
  }
  m_bIsReady = false;
  CSectionDecoder::Reset();
  LogDebug("NttParser: reset done");
}

void CNttParser::OnNewSection(CSection& sections)
{
  if (sections.table_id != 0xc3 || m_bIsReady || m_pCallBack == NULL)
  {
    return;
  }
  if (sections.section_length < 9)
  {
    LogDebug("NttParser: invalid section size %d, expected at least 12 bytes", sections.section_length);
  }
  byte* section = sections.Data;

  try
  {
    int sectionLength = ((section[1] & 0x0f) << 8) + section[2];
    if (sections.section_length != sectionLength)
    {
      LogDebug("NttParser: invalid section length = %d, byte count = %d", sectionLength, sections.section_length);
      return;
    }
    int protocolVersion = (section[3] & 0x1f);
    unsigned int iso639LanguageCode = section[4] + (section[5] << 8) + (section[6] << 16);
    int transmissionMedium = (section[7] >> 4);
    int tableSubtype = (section[7] & 0xf);

    LogDebug("NttParser: section length = %d, protocol version = %d, ISO 639 language = %s, transmission medium = %d, table subtype = %d", sectionLength, protocolVersion, &iso639LanguageCode, transmissionMedium, tableSubtype);

    int pointer = 8;
    int endOfSection = sectionLength - 1;
    bool success = true;
    switch (tableSubtype)
    {
      case TRANSPONDER_NAME_STID:
        success = DecodeTransponderName(section, pointer, endOfSection, iso639LanguageCode);
        break;
      case SATELLITE_TEXT_STID:
        success = DecodeSatelliteText(section, pointer, endOfSection, iso639LanguageCode);
        break;
      case RATINGS_TEXT_STID:
        success = DecodeRatingsText(section, pointer, endOfSection, iso639LanguageCode);
        break;
      case RATING_SYSTEM_STID:
        success = DecodeRatingSystem(section, pointer, endOfSection, iso639LanguageCode);
        break;
      case CURRENCY_SYSTEM_STID:
        success = DecodeCurrencySystem(section, pointer, endOfSection, iso639LanguageCode);
        break;
      case SOURCE_NAME_STID:
        success = DecodeSourceName(section, pointer, endOfSection, iso639LanguageCode);
        break;
      case MAP_NAME_STID:
        success = DecodeMapName(section, pointer, endOfSection, iso639LanguageCode);
        break;
      default:
        LogDebug("NttParser: unsupported table subtype %d", tableSubtype);
        return;
    }
    if (!success)
    {
      return;
    }

    while (pointer + 1 < endOfSection)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      LogDebug("NttParser: descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid descriptor length %d, pointer = %d, end of section = %d", length, pointer, endOfSection);
        return;
      }

      if (tag == 0x93)  // revision detection descriptor
      {
        DecodeRevisionDetectionDescriptor(&section[pointer], length, tableSubtype);
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("NttParser: corruption detected at end of section, pointer = %d, end of section = %d", pointer, endOfSection);
    }
  }
  catch (...)
  {
    LogDebug("NttParser: unhandled exception in OnNewSection()");
  }
}

bool CNttParser::DecodeTransponderName(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer + 3 > endOfSection)
  {
    LogDebug("NttParser: corruption detected at transponder name, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte satelliteId = section[pointer++];
  byte firstIndex = section[pointer++];
  byte numberOfTntRecords = section[pointer++];
  LogDebug("NttParser: transponder name, satellite ID = %d, first index = %d, number of TNT records = %d", satelliteId, firstIndex, numberOfTntRecords);

  for (byte i = 0; i < numberOfTntRecords; i++)
  {
    if (pointer + 3 > endOfSection)
    {
      LogDebug("NttParser: detected transponder name table number of TNT records %d is invalid, pointer = %d, end of section = %d", numberOfTntRecords, pointer, endOfSection, i);
      return false;
    }
    int transponderNumber = (section[pointer++] & 0x3f);
    int transponderNameLength = (section[pointer++] & 0x1f);
    if (pointer + transponderNameLength > endOfSection)
    {
      LogDebug("NttParser: invalid transponder name table transponder name length %d, pointer = %d, end of section = %d, loop = %d", transponderNameLength, pointer, endOfSection, i);
      return false;
    }
    char* transponderName = NULL;
    DecodeMultilingualText(&section[pointer], transponderNameLength, &transponderName);
    if (transponderName != NULL)
    {
      LogDebug("NttParser: transponder name, number = %d, name = %s", transponderNumber, transponderName);
      delete[] transponderName;
      transponderName = NULL;
    }
    else
    {
      LogDebug("NttParser: transponder name, number = %d", transponderNumber);
    }
    pointer += transponderNameLength;

    // table descriptors
    if (pointer >= endOfSection)
    {
      LogDebug("NttParser: invalid section length at transponder name table descriptor count, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
      return false;
    }
    byte descriptorCount = section[pointer++];
    for (byte d = 0; d < descriptorCount; d++)
    {
      if (pointer + 2 > endOfSection)
      {
        LogDebug("NttParser: detected transponder name table descriptor count %d is invalid, pointer = %d, end of section = %d, loop = %d, inner loop = %d", descriptorCount, pointer, endOfSection, i, d);
        return false;
      }
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("NttParser: transponder name table descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid transponder name table descriptor length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", length, pointer, endOfSection, i, d);
        return false;
      }
      pointer += length;
    }
  }
  return true;
}

bool CNttParser::DecodeSatelliteText(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer + 2 > endOfSection)
  {
    LogDebug("NttParser: corruption detected at satellite text, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte firstIndex = section[pointer++];
  byte numberOfSttRecords = section[pointer++];
  LogDebug("NttParser: satellite text, first index = %d, number of STT records = %d", firstIndex, numberOfSttRecords);

  for (byte i = 0; i < numberOfSttRecords; i++)
  {
    if (pointer + 4 > endOfSection)
    {
      LogDebug("NttParser: detected satellite text table number of STT records %d is invalid, pointer = %d, end of section = %d, loop = %d", numberOfSttRecords, pointer, endOfSection, i);
      return false;
    }
    byte satelliteId = section[pointer++];
    int satelliteReferenceNameLength = (section[pointer++] & 0x0f);
    if (pointer + satelliteReferenceNameLength > endOfSection)
    {
      LogDebug("NttParser: invalid satellite text table satellite reference name length %d, pointer = %d, end of section = %d, loop = %d", satelliteReferenceNameLength, pointer, endOfSection, i);
      return false;
    }
    char* satelliteReferenceName = NULL;
    DecodeMultilingualText(&section[pointer], satelliteReferenceNameLength, &satelliteReferenceName);
    if (satelliteReferenceName != NULL)
    {
      LogDebug("NttParser: satellite text, satellite ID = %d, reference name = %s", satelliteId, satelliteReferenceName);
      delete[] satelliteReferenceName;
      satelliteReferenceName = NULL;
    }
    else
    {
      LogDebug("NttParser: satellite text, satellite ID = %d", satelliteId);
    }
    pointer += satelliteReferenceNameLength;

    if (pointer >= endOfSection)
    {
      LogDebug("NttParser: corruption detected at satellite text table full satellite name, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
      return false;
    }
    int fullSatelliteNameLength = (section[pointer++] & 0x1f);
    if (pointer + fullSatelliteNameLength > endOfSection)
    {
      LogDebug("NttParser: invalid satellite text table full satellite name length %d, pointer = %d, end of section = %d, loop = %d", fullSatelliteNameLength, pointer, endOfSection, i);
      return false;
    }
    char* fullSatelliteName = NULL;
    DecodeMultilingualText(&section[pointer], fullSatelliteNameLength, &fullSatelliteName);
    if (fullSatelliteName != NULL)
    {
      LogDebug("NttParser: satellite text, satellite ID = %d, full name = %s", satelliteId, fullSatelliteName);
      delete[] fullSatelliteName;
      fullSatelliteName = NULL;
    }
    else
    {
      LogDebug("NttParser: satellite text, satellite ID = %d", satelliteId);
    }
    pointer += fullSatelliteNameLength;

    // table descriptors
    if (pointer >= endOfSection)
    {
      LogDebug("NttParser: invalid section length at satellite text table descriptor count, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
      return false;
    }
    byte descriptorCount = section[pointer++];
    for (byte d = 0; d < descriptorCount; d++)
    {
      if (pointer + 2 > endOfSection)
      {
        LogDebug("NttParser: detected satellite text table descriptor count %d is invalid, pointer = %d, end of section = %d, loop = %d, inner loop = %d", descriptorCount, pointer, endOfSection, i, d);
        return false;
      }
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("NttParser: satellite text table descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid satellite text table descriptor length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", length, pointer, endOfSection, i, d);
        return false;
      }
      pointer += length;
    }
  }
  return true;
}

bool CNttParser::DecodeRatingsText(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer >= endOfSection)
  {
    LogDebug("NttParser: corruption detected at ratings text, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte ratingRegion = section[pointer++];
  for (byte i = 0; i < 6; i++)
  {
    if (pointer >= endOfSection)
    {
      LogDebug("NttParser: corruption detected at ratings text table levels defined, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
      return false;
    }
    byte levelsDefined = section[pointer++];
    if (levelsDefined > 0)
    {
      if (pointer >= endOfSection)
      {
        LogDebug("NttParser: corruption detected at ratings text table dimension name length, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
        return false;
      }
      byte dimensionNameLength = section[pointer++];
      if (pointer + dimensionNameLength > endOfSection)
      {
        LogDebug("NttParser: invalid ratings text table dimension name length %d, pointer = %d, end of section = %d, loop = %d", dimensionNameLength, pointer, endOfSection, i);
        return false;
      }
      char* dimensionName = NULL;
      DecodeMultilingualText(&section[pointer], dimensionNameLength, &dimensionName);
      if (dimensionName != NULL)
      {
        LogDebug("NttParser: ratings text, dimension name = %s, levels defined = %d", dimensionName, levelsDefined);
        delete[] dimensionName;
        dimensionName = NULL;
      }
      else
      {
        LogDebug("NttParser: ratings text, levels defined = %d", levelsDefined);
      }
      pointer += dimensionNameLength;

      for (byte l = 0; l < levelsDefined; l++)
      {
        byte ratingNameLength = section[pointer++];
        if (pointer + ratingNameLength > endOfSection)
        {
          LogDebug("NttParser: invalid ratings text table rating name length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", ratingNameLength, pointer, endOfSection, i, l);
          return false;
        }
        char* ratingName = NULL;
        DecodeMultilingualText(&section[pointer], ratingNameLength, &ratingName);
        if (ratingName != NULL)
        {
          LogDebug("NttParser: rating name = %s", ratingName);
          delete[] ratingName;
          ratingName = NULL;
        }
        pointer += ratingNameLength;
      }
    }
  }
  return true;
}

bool CNttParser::DecodeRatingSystem(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer >= endOfSection)
  {
    LogDebug("NttParser: corruption detected at rating system, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte regionsDefined = section[pointer++];
  for (byte i = 0; i < regionsDefined; i++)
  {
    if (pointer + 3 > endOfSection)
    {
      LogDebug("NttParser: detected rating system table regions defined %d is invalid, pointer = %d, end of section = %d, loop = %d", regionsDefined, pointer, endOfSection, i);
      return false;
    }
    byte dataLength = section[pointer++];
    int endOfData = pointer + dataLength;
    if (endOfData > endOfSection)
    {
      LogDebug("NttParser: invalid rating system table data length %d, pointer = %d, end of section = %d, loop = %d", dataLength, pointer, endOfSection, i);
      return false;
    }
    byte ratingRegion = section[pointer++];
    byte stringLength = section[pointer++];
    if (pointer + stringLength > endOfSection)
    {
      LogDebug("NttParser: invalid rating system table string length %d, pointer = %d, end of section = %d, loop = %d", stringLength, pointer, endOfSection, i);
      return false;
    }
    char* ratingSystem = NULL;
    DecodeMultilingualText(&section[pointer], stringLength, &ratingSystem);
    if (ratingSystem != NULL)
    {
      LogDebug("NttParser: rating system, region = %d, system = %s", ratingRegion, ratingSystem);
      delete[] ratingSystem;
      ratingSystem = NULL;
    }
    else
    {
      LogDebug("NttParser: rating system, region = %d", ratingRegion);
    }
    pointer += stringLength;

    // table descriptors
    while (pointer + 1 < endOfData)
    {
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("NttParser: rating system table descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid rating system table descriptor length %d, pointer = %d, end of section = %d, loop = %d", length, pointer, endOfSection, i);
        return false;
      }
      pointer += length;
    }
    if (pointer != endOfData)
    {
      LogDebug("NttParser: corruption detected at end of rating system data, pointer = %d, end of section = %d, end of data = %d, loop = %d", pointer, endOfSection, endOfData, i);
      return false;
    }
  }
  return true;
}

bool CNttParser::DecodeCurrencySystem(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer >= endOfSection)
  {
    LogDebug("NttParser: corruption detected at currency system, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte regionsDefined = section[pointer++];
  for (byte i = 0; i < regionsDefined; i++)
  {
    if (pointer + 3 > endOfSection)
    {
      LogDebug("NttParser: detected currency system table regions defined %d is invalid, pointer = %d, end of section = %d, loop = %d", regionsDefined, pointer, endOfSection, i);
      return false;
    }
    byte dataLength = section[pointer++];
    int endOfData = pointer + dataLength;
    if (endOfData > endOfSection)
    {
      LogDebug("NttParser: invalid currency system table data length %d, pointer = %d, end of section = %d, loop = %d", dataLength, pointer, endOfSection, i);
      return false;
    }
    byte currencyRegion = section[pointer++];
    byte stringLength = section[pointer++];
    if (pointer + stringLength > endOfSection)
    {
      LogDebug("NttParser: invalid currency system table string length %d, pointer = %d, end of section = %d, loop = %d", stringLength, pointer, endOfSection, i);
      return false;
    }
    char* currencySystem = NULL;
    DecodeMultilingualText(&section[pointer], stringLength, &currencySystem);
    if (currencySystem != NULL)
    {
      LogDebug("NttParser: currency system, region = %d, system = %s", currencyRegion, currencySystem);
      delete[] currencySystem;
      currencySystem = NULL;
    }
    else
    {
      LogDebug("NttParser: currency system, region = %d", currencyRegion);
    }
    pointer += stringLength;

    // table descriptors
    while (pointer + 1 < endOfData)
    {
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("NttParser: currency system table descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid currency system table descriptor length %d, pointer = %d, end of section = %d, loop = %d", length, pointer, endOfSection, i);
        return false;
      }
      pointer += length;
    }
    if (pointer != endOfData)
    {
      LogDebug("NttParser: corruption detected at end of currency system data, pointer = %d, end of section = %d, end of data = %d, loop = %d", pointer, endOfSection, endOfData, i);
      return false;
    }
  }
  return true;
}

bool CNttParser::DecodeSourceName(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer >= endOfSection)
  {
    LogDebug("NttParser: corruption detected at source name, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte numberOfSntRecords = section[pointer++];
  for (byte i = 0; i < numberOfSntRecords; i++)
  {
    if (pointer + 5 > endOfSection)
    {
      LogDebug("NttParser: detected source name table number of SNT records %d is invalid, pointer = %d, end of section = %d, loop = %d", numberOfSntRecords, pointer, endOfSection, i);
      return false;
    }
    bool applicationType = ((section[pointer++] & 0x80) != 0);
    int sourceId = (section[pointer] << 8) + section[pointer + 1];
    pointer += 2;
    byte nameLength = section[pointer++];
    if (pointer + nameLength > endOfSection)
    {
      LogDebug("NttParser: invalid source name table string length %d, pointer = %d, end of section = %d, loop = %d", nameLength, pointer, endOfSection, i);
      return false;
    }
    char* sourceName = NULL;
    DecodeMultilingualText(&section[pointer], nameLength, &sourceName);
    if (m_pCallBack != NULL)
    {
      m_pCallBack->OnNttReceived(sourceId, applicationType, sourceName, languageCode);
    }
    if (sourceName != NULL)
    {
      LogDebug("NttParser: source name, source ID = 0x%x, name = %s, application type = %d", sourceId, sourceName, applicationType);
      delete[] sourceName;
      sourceName = NULL;
    }
    else
    {
      LogDebug("NttParser: source name, source ID = 0x%x, application type = %d", sourceId, applicationType);
    }
    pointer += nameLength;

    // table descriptors
    if (pointer >= endOfSection)
    {
      LogDebug("NttParser: invalid section length at source name table descriptor count, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
      return false;
    }
    byte descriptorCount = section[pointer++];
    for (byte d = 0; d < descriptorCount; d++)
    {
      if (pointer + 2 > endOfSection)
      {
        LogDebug("NttParser: detected source name table descriptor count %d is invalid, pointer = %d, end of section = %d, loop = %d, inner loop = %d", descriptorCount, pointer, endOfSection, i, d);
        return false;
      }
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("NttParser: source name table descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid source name table descriptor length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", length, pointer, endOfSection, i, d);
        return false;
      }
      pointer += length;
    }
  }
  return true;
}

bool CNttParser::DecodeMapName(byte* section, int& pointer, int endOfSection, unsigned int languageCode)
{
  if (pointer >= endOfSection)
  {
    LogDebug("NttParser: corruption detected at map name, pointer = %d, end of section = %d", pointer, endOfSection);
    return false;
  }

  byte numberOfMntRecords = section[pointer++];
  for (byte i = 0; i < numberOfMntRecords; i++)
  {
    if (pointer + 4 > endOfSection)
    {
      LogDebug("NttParser: detected map name table number of MNT records %d is invalid, pointer = %d, end of section = %d, loop = %d", numberOfMntRecords, pointer, endOfSection, i);
      return false;
    }
    int vctId = (section[pointer] << 8) + section[pointer + 1];
    pointer += 2;
    byte mapNameLength = section[pointer++];
    if (pointer + mapNameLength > endOfSection)
    {
      LogDebug("NttParser: invalid map name table map name length %d, pointer = %d, end of section = %d, loop = %d", mapNameLength, pointer, endOfSection, i);
      return false;
    }
    char* mapName = NULL;
    DecodeMultilingualText(&section[pointer], mapNameLength, &mapName);
    if (mapName != NULL)
    {
      LogDebug("NttParser: map name, VCT ID = 0x%x, name = %s", vctId, mapName);
      delete[] mapName;
      mapName = NULL;
    }
    else
    {
      LogDebug("NttParser: map name, VCT ID = 0x%x", vctId);
    }
    pointer += mapNameLength;

    // table descriptors
    if (pointer >= endOfSection)
    {
      LogDebug("NttParser: invalid section length at map name table descriptor count, pointer = %d, end of section = %d, loop = %d", pointer, endOfSection, i);
      return false;
    }
    byte descriptorCount = section[pointer++];
    for (byte d = 0; d < descriptorCount; d++)
    {
      if (pointer + 2 > endOfSection)
      {
        LogDebug("NttParser: detected map name table descriptor count %d is invalid, pointer = %d, end of section = %d, loop = %d, inner loop = %d", descriptorCount, pointer, endOfSection, i, d);
        return false;
      }
      byte tag = section[pointer++];
      byte length = section[pointer++];
      LogDebug("NttParser: map name table descriptor, tag = 0x%x, length = %d", tag, length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid map name table descriptor length %d, pointer = %d, end of section = %d, loop = %d, inner loop = %d", length, pointer, endOfSection, i, d);
        return false;
      }
      pointer += length;
    }
  }
  return true;
}

void CNttParser::DecodeMultilingualText(byte* b, int length, char** string)
{
  if (length < 2)
  {
    LogDebug("NttParser: invalid multilingual text length = %d", length);
    return;
  }
  *string = new char[length + 1];
  if (*string == NULL)
  {
    LogDebug("NttParser: failed to allocate memory in DecodeMultilingualText()");
    return;
  }
  int stringOffset = 0;

  int pointer = 0;
  while (pointer + 1 < length)
  {
    int mode = b[pointer++];
    int segment_length = b[pointer++];
    if (pointer + segment_length > length)
    {
      LogDebug("NttParser: invalid multilingual text segment length = %d, pointer = %d, string offset = %d, mode = 0x%x, text length = %d", segment_length, pointer, stringOffset, mode, length);
      delete[] *string;
      *string = NULL;
      return;
    }

    if (mode == 0)
    {
      // We only support ASCII encoding at this time.
      memcpy(*string, &b[pointer], segment_length);
      stringOffset += segment_length;
      pointer += segment_length;
      (*string)[stringOffset] = 0;  // NULL terminate
    }
    else
    {
      LogDebug("NttParser: unsupported segment mode in DecodeMultilingualText(), mode = 0x%x", mode);
      for (int i = 0; i < segment_length; i++)
      {
        LogDebug("  %d: 0x%x", i, b[pointer++]);
      }
    }
  }
}

void CNttParser::DecodeRevisionDetectionDescriptor(byte* b, int length, int tableSubtype)
{
  if (length != 3)
  {
    LogDebug("NttParser: invalid revision detection descriptor length %d", length);
    return;
  }

  int currentVersion = m_mCurrentVersions[tableSubtype];
  vector<int>* unseenSections = m_mUnseenSections[tableSubtype];

  int versionNumber = (b[0] & 0x1f);
  int sectionNumber = b[1];
  int lastSectionNumber = b[2];
  LogDebug("NttParser: revision detection descriptor, table subtype = %d, version = %d, section number = %d, last section number = %d", tableSubtype, versionNumber, sectionNumber, lastSectionNumber);
  if (versionNumber > currentVersion || (versionNumber == MAX_TABLE_VERSION_NUMBER && versionNumber < currentVersion))
  {
    LogDebug("NttParser: new table version");
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
    LogDebug("NttParser: existing table version, unseen section count = %d", unseenSections->size());
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
        LogDebug("NttParser: ready, revision detection");
        m_bIsReady = isReady;
        m_pCallBack = NULL;
      }
    }
  }
}