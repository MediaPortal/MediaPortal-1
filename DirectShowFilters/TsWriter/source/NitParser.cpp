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
#include <windows.h>
#include <bdatypes.h>
#include "NitParser.h"

extern bool DisableCRCCheck();

CNitParser::CNitParser(void)
{
  m_sName = "NitParser";
  SetPid(PID_NIT);
  m_vTableIds.push_back(0x40);  // 0x40 = actual (current) network
  m_vTableIds.push_back(0x41);  // 0x40 = other networks
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset();
}

CNitParser::~CNitParser(void)
{
  CleanUp();
}

void CNitParser::CleanUp()
{
  map<int, map<unsigned int, char*>*>::iterator groupNameSetIt = m_mGroupNames.begin();
  while (groupNameSetIt != m_mGroupNames.end())
  {
    map<unsigned int, char*>* nameSet = groupNameSetIt->second;
    if (nameSet != NULL)
    {
      map<unsigned int, char*>::iterator nameIt = nameSet->begin();
      while (nameIt != nameSet->end())
      {
        char* name = nameIt->second;
        delete[] name;
        name = NULL;
        nameIt++;
      }
      nameSet->clear();

      delete nameSet;
      nameSet = NULL;
    }
    groupNameSetIt++;
  }
  m_mGroupNames.clear();

  map<__int64, map<unsigned int, char*>*>::iterator regionNameIt = m_mTargetRegionNames.begin();
  while (regionNameIt != m_mTargetRegionNames.end())
  {
    map<unsigned int, char*>* nameSet = regionNameIt->second;
    if (nameSet != NULL)
    {
      map<unsigned int, char*>::iterator nameIt = nameSet->begin();
      while (nameIt != nameSet->end())
      {
        char* name = nameIt->second;
        delete[] name;
        name = NULL;
        nameIt++;
      }
      nameSet->clear();

      delete nameSet;
      nameSet = NULL;
    }
    regionNameIt++;
  }
  m_mTargetRegionNames.clear();

  map<__int64, map<int, bool>*>::iterator groupIdIt = m_mGroupIds.begin();
  while (groupIdIt != m_mGroupIds.end())
  {
    map<int, bool>* list = groupIdIt->second;
    delete list;
    list = NULL;
    groupIdIt++;
  }
  m_mGroupIds.clear();

  map<__int64, map<int, bool>*>::iterator cellIt = m_mAvailableInCells.begin();
  while (cellIt != m_mAvailableInCells.end())
  {
    map<int, bool>* list = cellIt->second;
    delete list;
    list = NULL;
    cellIt++;
  }
  m_mAvailableInCells.clear();

  map<__int64, map<__int64, bool>*>::iterator regionIt = m_mTargetRegions.begin();
  while (regionIt != m_mTargetRegions.end())
  {
    map<__int64, bool>* list = regionIt->second;
    delete list;
    list = NULL;
    regionIt++;
  }
  m_mTargetRegions.clear();

  map<__int64, map<unsigned int, bool>*>::iterator countryIt = m_mAvailableInCountries.begin();
  while (countryIt != m_mAvailableInCountries.end())
  {
    map<unsigned int, bool>* list = countryIt->second;
    delete list;
    list = NULL;
    countryIt++;
  }
  m_mAvailableInCountries.clear();

  countryIt = m_mUnavailableInCountries.begin();
  while (countryIt != m_mUnavailableInCountries.end())
  {
    map<unsigned int, bool>* list = countryIt->second;
    delete list;
    list = NULL;
    countryIt++;
  }
  m_mUnavailableInCountries.clear();

  vector<NitCableMultiplexDetail*>::iterator cableMuxIt = m_vCableMuxes.begin();
  while (cableMuxIt != m_vCableMuxes.end())
  {
    NitCableMultiplexDetail* cableMux = *cableMuxIt;
    delete cableMux;
    cableMux = NULL;
    cableMuxIt++;
  }
  m_vCableMuxes.clear();

  vector<NitSatelliteMultiplexDetail*>::iterator satMuxIt = m_vSatelliteMuxes.begin();
  while (satMuxIt != m_vSatelliteMuxes.end())
  {
    NitSatelliteMultiplexDetail* satMux = *satMuxIt;
    delete satMux;
    satMux = NULL;
    satMuxIt++;
  }
  m_vSatelliteMuxes.clear();

  vector<NitTerrestrialMultiplexDetail*>::iterator terrMuxIt = m_vTerrestrialMuxes.begin();
  while (terrMuxIt != m_vTerrestrialMuxes.end())
  {
    NitTerrestrialMultiplexDetail* terrMux = *terrMuxIt;
    delete terrMux;
    terrMux = NULL;
    terrMuxIt++;
  }
  m_vTerrestrialMuxes.clear();
}

void CNitParser::OnNewSection(CSection& sections)
{
  bool isValidTableId = false;
  for (unsigned int i = 0; i < m_vTableIds.size(); i++)
  {
    if (m_vTableIds[i] == sections.table_id)
    {
      isValidTableId = true;
      break;
    }
  }
  if (!isValidTableId)
  {
    return;
  }

  byte* section = sections.Data;

  try
  {
    int section_syntax_indicator = section[1] & 0x80;
    int section_length = ((section[1] & 0xf) << 8) + section[2];
    if (section_length > 1021 || section_length < 13)
    {
      LogDebug("%s: invalid section length = %d", m_sName, section_length);
      return;
    }
    int extension_id = (section[3] << 8) + section[4];    // network_id or bouquet_id
    int version_number = (section[5] >> 1) & 0x1f;
    int current_next_indicator = section[5] & 1;
    if (current_next_indicator == 0)
    {
      // Details do not apply yet...
      return;
    }
    int section_number = section[6];
    int last_section_number = section[7];
    int extension_descriptors_length = ((section[8] & 0xf) << 8) + section[9];  // network_descriptors_length or bouquet_descriptors_length

    int endOfSection = section_length - 1;
    //LogDebug("%s: extension ID = 0x%x, table ID = 0x%x, section number = %d, version = %d, last section number = %d, section length = %d, end of section = %d",
    //          m_sName, extension_id, sections.table_id, section_number, version_number, last_section_number, section_length, endOfSection);

    unsigned int key = (sections.table_id << 24) + (extension_id << 8) + section_number;
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
              //LogDebug("%s: not yet seen %x", m_sName, it->first);
              ready = false;
              break;
            }
          }
          m_bIsReady = ready;
          if (ready)
          {
            LogDebug("%s: ready, sections parsed = %d", m_sName, m_mSeenSections.size());
          }
        }
        return;
      }
    }
    else
    {
      //LogDebug("%s: new section %x", m_sName, key);
      m_bIsReady = false;
      unsigned int k = (sections.table_id << 24) + (extension_id << 8);
      while ((int)(k & 0xff) <= last_section_number)
      {
        if (m_mSeenSections.find(k) == m_mSeenSections.end())
        {
          //LogDebug("%s: add section %x", m_sName, k);
          m_mSeenSections[k] = false;
        }
        k++;
      }
    }

    int endOfExtensionDescriptorLoop = 10 + extension_descriptors_length;
    //LogDebug("%s: extension descriptors length = %d, end of extension descriptor loop = %d", m_sName, extension_descriptors_length, endOfExtensionDescriptorLoop);
    if (endOfExtensionDescriptorLoop > endOfSection)
    {
      LogDebug("%s: invalid extension descriptor loop length = %d, end of section = %d, section length = %d", m_sName, extension_descriptors_length, endOfSection, section_length);
      return;
    }
    int pointer = 10; // points to the first byte in the extension descriptor loop
    vector<unsigned int> availableInCountries;
    vector<unsigned int> unavailableInCountries;
    vector<__int64> groupTargetRegions;
    while (pointer + 1 < endOfExtensionDescriptorLoop)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      //LogDebug("%s: extension descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", m_sName, tag, length, pointer, pointer + length);
      if (pointer + length > endOfExtensionDescriptorLoop)
      {
        LogDebug("%s: invalid descriptor length = %d, pointer = %d, end of extension descriptors = %d, end of section = %d, section length = %d", m_sName, length, pointer, endOfExtensionDescriptorLoop, endOfSection, section_length);
        return;
      }

      if (tag == 0x40 || tag == 0x47) // network name descriptor, bouquet name descriptor
      {
        char* name = NULL;
        DecodeNameDescriptor(&section[pointer], length, &name);
        map<unsigned int, char*> tempNames;
        tempNames[0] = name;
        AddGroupNames(extension_id, &tempNames);
      }
      else if (tag == 0x49) // country availability descriptor
      {
        DecodeCountryAvailabilityDescriptor(&section[pointer], length, &availableInCountries, &unavailableInCountries);
      }
      else if (tag == 0x5b || tag == 0x5c)  // multilingual network name descriptor, multilingual bouquet name descriptor
      {
        map<unsigned int, char*> tempNames;
        DecodeMultilingualNameDescriptor(&section[pointer], length, &tempNames);
        AddGroupNames(extension_id, &tempNames);
      }
      // Extended descriptors...
      else if (tag == 0x7f)
      {
        if (length < 1)
        {
          LogDebug("%s: invalid extended descriptor length = %d, pointer = %d, end of extension descriptors = %d, end of section = %d, section length = %d", m_sName, length, pointer, endOfExtensionDescriptorLoop, endOfSection, section_length);
          return;
        }

        int tag_extension = section[pointer];
        if (tag_extension == 0x09)  // target region descriptor
        {
          DecodeTargetRegionDescriptor(&section[pointer], length, &groupTargetRegions);
        }
        else if (tag_extension == 0x0a) // target region name descriptor
        {
          map<__int64, char*> targetRegionNames;
          unsigned int language;
          DecodeTargetRegionNameDescriptor(&section[pointer], length, &targetRegionNames, &language);
          AddTargetRegionNames(&targetRegionNames, language);
        }
      }

      pointer += length;
    }
    pointer = endOfExtensionDescriptorLoop;

    int transport_stream_loop_length = ((section[pointer] & 0xf) << 8) + section[pointer + 1];
    pointer += 2;
    //LogDebug("%s: transport stream loop length = %d, end of transport stream loop = %d", m_sName, transport_stream_loop_length, pointer + transport_stream_loop_length);
    if (pointer + transport_stream_loop_length != endOfSection)
    {
      LogDebug("%s: invalid transport stream loop length = %d, pointer = %d, end of section = %d, section length = %d", m_sName, transport_stream_loop_length, pointer, endOfSection, section_length);
      return;
    }
    // Note: this following code relies on the assumption that each inner descriptor loop will only
    // contain the details for one multiplex/transponder.
    while (pointer + 5 < endOfSection)
    {
      int transport_stream_id = (section[pointer] << 8) + section[pointer + 1];
      pointer += 2;
      int original_network_id = (section[pointer] << 8) + section[pointer + 1];
      pointer += 2;
      int transport_descriptors_length = ((section[pointer] & 0xf) << 8) + section[pointer + 1];
      pointer += 2;
      int endOfTransportDescriptors = pointer + transport_descriptors_length;
      //LogDebug("%s: TSID = 0x%x, ONID = 0x%x, transport descriptors length = %d, pointer = %d, end of transport descriptors = %d",
      //          m_sName, transport_stream_id, original_network_id, transport_descriptors_length, pointer, endOfTransportDescriptors);
      if (endOfTransportDescriptors > endOfSection)
      {
        LogDebug("%s: invalid transport descriptors length = %d, pointer = %d, end of transport descriptors = %d, end of section = %d, section length = %d", m_sName, transport_descriptors_length, pointer, endOfTransportDescriptors, endOfSection, section_length);
        return;
      }

      NitSatelliteMultiplexDetail satelliteMux;
      NitCableMultiplexDetail cableMux;
      NitTerrestrialMultiplexDetail terrestrialMux;
      map<int, int> lcns;
      vector<int> frequencies;
      map<int, int> cellFrequencies;  // cell ID | cell ID extension => frequency
      vector<int> serviceIds;
      vector<__int64> transportStreamTargetRegions;
      while (pointer + 1 < endOfTransportDescriptors)
      {
        int tag = section[pointer++];
        int length = section[pointer++];
        //LogDebug("%s: transport descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d", m_sName, tag, length, pointer, pointer + length);
        if (pointer + length > endOfTransportDescriptors)
        {
          LogDebug("%s: invalid descriptor length = %d, pointer = %d, end of transport descriptors = %d, end of section = %d, section length = %d", m_sName, length, pointer, endOfTransportDescriptors, endOfSection, section_length);
          return;
        }

        if (tag == 0x41)  // service list descriptor
        {
          DecodeServiceListDescriptor(&section[pointer], length, &serviceIds);
        }
        else if (tag == 0x43) // satellite delivery system descriptor
        {
          if (DecodeSatelliteDeliverySystemDescriptor(&section[pointer], length, &satelliteMux))
          {
            satelliteMux.OriginalNetworkId = original_network_id;
            satelliteMux.TransportStreamId = transport_stream_id;
          }
        }
        else if (tag == 0x44) // cable delivery system descriptor
        {
          if (DecodeCableDeliverySystemDescriptor(&section[pointer], length, &cableMux))
          {
            cableMux.OriginalNetworkId = original_network_id;
            cableMux.TransportStreamId = transport_stream_id;
          }
        }
        else if (tag == 0x5a) // terrestrial delivery system descriptor
        {
          if (DecodeTerrestrialDeliverySystemDescriptor(&section[pointer], length, &terrestrialMux))
          {
            terrestrialMux.OriginalNetworkId = original_network_id;
            terrestrialMux.TransportStreamId = transport_stream_id;
          }
        }
        else if (tag == 0x62) // frequency list descriptor
        {
          DecodeFrequencyListDescriptor(&section[pointer], length, &frequencies);
        }
        else if (tag == 0x6d) // cell frequency link descriptor
        {
          DecodeCellFrequencyLinkDescriptor(&section[pointer], length, &cellFrequencies);
        }
        else if (tag == 0x79) // S2 satellite delivery system descriptor
        {
          if (DecodeS2SatelliteDeliverySystemDescriptor(&section[pointer], length, &satelliteMux))
          {
            satelliteMux.OriginalNetworkId = original_network_id;
            satelliteMux.TransportStreamId = transport_stream_id;
          }
        }
        else if (tag == 0x83) // logical channel number descriptor
        {
          DecodeLogicalChannelNumberDescriptor(&section[pointer], length, &lcns);
          AddLogicalChannelNumbers(original_network_id, transport_stream_id, &lcns);
        }
        else if (tag == 0xe2) // Sky NZ logical channel number descriptor
        {
          DecodeLogicalChannelNumberDescriptor(&section[pointer], length, &lcns);
          AddLogicalChannelNumbers(original_network_id, transport_stream_id, &lcns);
        }
        // Extended descriptors...
        else if (tag == 0x7f)
        {
          if (length < 1)
          {
            LogDebug("%s: invalid extended descriptor length = %d, pointer = %d, end of transport descriptors = %d, end of section = %d, section length = %d", m_sName, length, pointer, endOfTransportDescriptors, endOfSection, section_length);
            return;
          }

          int tag_extension = section[pointer];
          if (tag_extension == 0x04)  // T2 delivery system descriptor
          {
            if (DecodeT2TerrestrialDeliverySystemDescriptor(&section[pointer], length, &terrestrialMux, &cellFrequencies))
            {
              terrestrialMux.OriginalNetworkId = original_network_id;
              terrestrialMux.TransportStreamId = transport_stream_id;
            }
          }
          else if (tag_extension == 0x09) // target region descriptor
          {
            DecodeTargetRegionDescriptor(&section[pointer], length, &transportStreamTargetRegions);
          }
        }

        pointer += length;
      }

      // We now have a bunch of network, bouquet and transport stream details that have to be
      // recorded per-service.
      if (serviceIds.size() == 0)
      {
        serviceIds.push_back(0);  // For when properties apply to all services in a transport stream...
      }
      AddServiceDetails(extension_id, original_network_id, transport_stream_id, &serviceIds,
                        &cellFrequencies, ((transportStreamTargetRegions.size() > 0) ? &transportStreamTargetRegions : &groupTargetRegions),
                        &availableInCountries, &unavailableInCountries);

      // We also have multiplex tuning details and frequencies that have to be combined.
      AddMultiplexDetails(&cableMux, &satelliteMux, &terrestrialMux, &cellFrequencies, &frequencies);
    }

    if (pointer != endOfSection)
    {
      LogDebug("%s: section parsing error", m_sName);
    }
    else
    {
      m_mSeenSections[key] = true;
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in OnNewSection()", m_sName);
  }
}

bool CNitParser::IsReady()
{
  return m_bIsReady;
}

void CNitParser::Reset()
{
  LogDebug("%s: reset", m_sName);
  CSectionDecoder::Reset();
  CleanUp();
  m_mLogicalChannelNumbers.clear();
  m_mSeenSections.clear();
  m_bIsReady = false;
  LogDebug("%s: reset done", m_sName);
}

int CNitParser::GetMultiplexCount()
{
  return m_vCableMuxes.size() + m_vSatelliteMuxes.size() + m_vTerrestrialMuxes.size();
}

NitMultiplexDetail* CNitParser::GetMultiplexDetail(int idx)
{
  if (idx < 0)
  {
    return NULL;
  }
  if (idx < (int)m_vCableMuxes.size())
  {
    return m_vCableMuxes[idx];
  }
  idx -= m_vCableMuxes.size();
  if (idx < (int)m_vSatelliteMuxes.size())
  {
    return m_vSatelliteMuxes[idx];
  }
  idx -= m_vSatelliteMuxes.size();
  if (idx < (int)m_vTerrestrialMuxes.size())
  {
    return m_vTerrestrialMuxes[idx];
  }
  return NULL;
}

int CNitParser::GetLogicialChannelNumber(int originalNetworkId, int transportStreamId, int serviceId)
{
  __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + serviceId;
  return m_mLogicalChannelNumbers[serviceKey];
}

void CNitParser::GetNetworkIds(int originalNetworkId, int transportStreamId, int serviceId, vector<int>* networkIds)
{
  if (networkIds == NULL)
  {
    return;
  }
  __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + serviceId;
  map<int, bool>* serviceNetworkIds = m_mGroupIds[serviceKey];
  if (serviceNetworkIds == NULL)
  {
    serviceNetworkIds = m_mGroupIds[serviceKey - serviceId];
  }
  if (serviceNetworkIds == NULL)
  {
    return;
  }
  map<int, bool>::iterator it = serviceNetworkIds->begin();
  while (it != serviceNetworkIds->end())
  {
    networkIds->push_back(it->first);
    it++;
  }
}

void CNitParser::GetAvailableInCells(int originalNetworkId, int transportStreamId, int serviceId, vector<int>* cellIds)
{
  if (cellIds == NULL)
  {
    return;
  }
  __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + serviceId;
  map<int, bool>* serviceCellIds = m_mAvailableInCells[serviceKey];
  if (serviceCellIds == NULL)
  {
    serviceCellIds = m_mAvailableInCells[serviceKey - serviceId];
  }
  if (serviceCellIds == NULL)
  {
    return;
  }
  map<int, bool>::iterator it = serviceCellIds->begin();
  while (it != serviceCellIds->end())
  {
    cellIds->push_back(it->first);
    it++;
  }
}

void CNitParser::GetTargetRegionIds(int originalNetworkId, int transportStreamId, int serviceId, vector<__int64>* targetRegionIds)
{
  if (targetRegionIds == NULL)
  {
    return;
  }
  __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + serviceId;
  map<__int64, bool>* serviceRegionIds = m_mTargetRegions[serviceKey];
  if (serviceRegionIds == NULL)
  {
    serviceRegionIds = m_mTargetRegions[serviceKey - serviceId];
  }
  if (serviceRegionIds == NULL)
  {
    return;
  }
  map<__int64, bool>::iterator it = serviceRegionIds->begin();
  while (it != serviceRegionIds->end())
  {
    targetRegionIds->push_back(it->first);
    it++;
  }
}

void CNitParser::GetAvailableInCountries(int originalNetworkId, int transportStreamId, int serviceId, vector<unsigned int>* availableInCountries)
{
  if (availableInCountries == NULL)
  {
    return;
  }
  __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + serviceId;
  map<unsigned int, bool>* serviceAvailableInCountries = m_mAvailableInCountries[serviceKey];
  if (serviceAvailableInCountries == NULL)
  {
    serviceAvailableInCountries = m_mAvailableInCountries[serviceKey - serviceId];
  }
  if (serviceAvailableInCountries == NULL)
  {
    return;
  }
  map<unsigned int, bool>::iterator it = serviceAvailableInCountries->begin();
  while (it != serviceAvailableInCountries->end())
  {
    availableInCountries->push_back(it->first);
    it++;
  }
}

void CNitParser::GetUnavailableInCountries(int originalNetworkId, int transportStreamId, int serviceId, vector<unsigned int>* unavailableInCountries)
{
  if (unavailableInCountries == NULL)
  {
    return;
  }
  __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + serviceId;
  map<unsigned int, bool>* serviceUnavailableInCountries = m_mUnavailableInCountries[serviceKey];
  if (serviceUnavailableInCountries == NULL)
  {
    serviceUnavailableInCountries = m_mUnavailableInCountries[serviceKey - serviceId];
  }
  if (serviceUnavailableInCountries == NULL)
  {
    return;
  }
  map<unsigned int, bool>::iterator it = serviceUnavailableInCountries->begin();
  while (it != serviceUnavailableInCountries->end())
  {
    unavailableInCountries->push_back(it->first);
    it++;
  }
}

int CNitParser::GetNetworkNameCount(int networkId)
{
  map<unsigned int, char*>* networkNames = m_mGroupNames[networkId];
  if (networkNames != NULL)
  {
    return networkNames->size();
  }
  return 0;
}

void CNitParser::GetNetworkName(int networkId, int index, unsigned int* language, char** name)
{
  *language = 0;
  *name = NULL;
  int count = 0;
  map<unsigned int, char*>* networkNames = m_mGroupNames[networkId];
  if (networkNames == NULL)
  {
    return;
  }
  map<unsigned int, char*>::iterator nameIt = networkNames->begin();
  while (nameIt != networkNames->end())
  {
    if (count == index)
    {
      *language = nameIt->first;
      *name = nameIt->second;
      return;
    }
    count++;
    nameIt++;
  }
}

int CNitParser::GetTargetRegionNameCount(__int64 regionId)
{
  int count = 0;
  map<unsigned int, char*>* targetRegionNames = m_mTargetRegionNames[regionId];
  if (targetRegionNames != NULL)
  {
    count = targetRegionNames->size();
  }
  return count;
}

void CNitParser::GetTargetRegionName(__int64 regionId, int index, unsigned int* language, char** name)
{
  *language = 0;
  *name = NULL;
  int count = 0;
  map<unsigned int, char*>* targetRegionNames = m_mTargetRegionNames[regionId];
  if (targetRegionNames == NULL)
  {
    return;
  }
  map<unsigned int, char*>::iterator nameIt = targetRegionNames->begin();
  while (nameIt != targetRegionNames->end())
  {
    if (count == index)
    {
      *language = nameIt->first;
      *name = nameIt->second;
      return;
    }
    count++;
    nameIt++;
  }
}

void CNitParser::DecodeLogicalChannelNumberDescriptor(byte* b, int length, map<int, int>* lcns)
{
  // De-facto standard LCN descriptor:
  // <loop>
  //   service_id 16
  //   visible_service_flag 1
  //   reserved 5
  //   logical_channel_number 10
  // </loop>
  try
  {
    int pointer = 0;
    while (pointer + 3 < length)
    {
      int serviceId = (b[pointer] << 8) + b[pointer + 1];
      int lcn = ((b[pointer + 2] & 0x3) << 8) + b[pointer + 3];
      (*lcns)[serviceId] = lcn;
      pointer += 4;
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeLogicalChannelNumberDescriptor()", m_sName);
  }
}

bool CNitParser::DecodeCableDeliverySystemDescriptor(byte* b, int length, NitCableMultiplexDetail* mux)
{
  if (length != 11)
  {
    LogDebug("%s: invalid cable delivery system descriptor length = %d", m_sName, length);
    return false;
  }
  try
  {
    mux->Frequency = DecodeCableFrequency(b);

    mux->OuterFecMethod = (b[5] & 0xf);
    switch (mux->OuterFecMethod)
    {
      case 0:
        mux->OuterFecMethod = BDA_FEC_METHOD_NOT_SET;
        break;
      case 1:
        mux->OuterFecMethod = BDA_FEC_METHOD_NOT_DEFINED;
        break;
      case 2:
        mux->OuterFecMethod = BDA_FEC_RS_204_188;
        break;
      default:
        mux->OuterFecMethod = BDA_FEC_METHOD_NOT_SET;
        break;
    }

    mux->Modulation = b[6];
    switch (mux->Modulation)
    {
      case 0:
        mux->Modulation = BDA_MOD_NOT_DEFINED;
        break;
      case 1:
        mux->Modulation = BDA_MOD_16QAM;
        break;
      case 2:
        mux->Modulation = BDA_MOD_32QAM;
        break;
      case 3:
        mux->Modulation = BDA_MOD_64QAM;
        break;
      case 4:
        mux->Modulation = BDA_MOD_128QAM;
        break;
      case 5:
        mux->Modulation = BDA_MOD_256QAM;
        break;
      default:
        mux->Modulation = BDA_MOD_NOT_SET;
        break;
    }

    // Symbol rate in Ms/s is encoded with BCD digits. The DP is after the 3rd digit. We want the symbol rate in ks/s.
    mux->SymbolRate = (100000 * ((b[7] >> 4) & 0xf));
    mux->SymbolRate += (10000 * ((b[7] & 0xf)));
    mux->SymbolRate += (1000 * ((b[8] >> 4) & 0xf));
    mux->SymbolRate += (100 * ((b[8] & 0xf)));
    mux->SymbolRate += (10 * ((b[9] >> 4) & 0xf));
    mux->SymbolRate += ((b[9] & 0xf));

    mux->InnerFecRate = (b[10] & 0xf);
    switch (mux->InnerFecRate)
    {
      case 0:
        mux->InnerFecRate = BDA_BCC_RATE_NOT_DEFINED;
        break;
      case 1:
        mux->InnerFecRate = BDA_BCC_RATE_1_2;
        break;
      case 2:
        mux->InnerFecRate = BDA_BCC_RATE_2_3;
        break;
      case 3:
        mux->InnerFecRate = BDA_BCC_RATE_3_4;
        break;
      case 4:
        mux->InnerFecRate = BDA_BCC_RATE_5_6;
        break;
      case 5:
        mux->InnerFecRate = BDA_BCC_RATE_7_8;
        break;
      case 6:
        mux->InnerFecRate = BDA_BCC_RATE_8_9;
        break;
      case 7:
        mux->InnerFecRate = BDA_BCC_RATE_3_5;
        break;
      case 8:
        mux->InnerFecRate = BDA_BCC_RATE_4_5;
        break;
      case 9:
        mux->InnerFecRate = BDA_BCC_RATE_9_10;
        break;
      case 15:
        mux->InnerFecRate = BDA_BCC_RATE_NOT_DEFINED;
        break;
      default:
        mux->InnerFecRate = BDA_BCC_RATE_NOT_SET;
        break;
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeCableDeliverySystemDescriptor()", m_sName);
    return false;
  }
  return true;
}

bool CNitParser::DecodeSatelliteDeliverySystemDescriptor(byte* b, int length, NitSatelliteMultiplexDetail* mux)
{
  if (length != 11)
  {
    LogDebug("%s: invalid satellite delivery system descriptor length = %d", m_sName, length);
    return false;
  }
  try
  {
    mux->Frequency = DecodeSatelliteFrequency(b);

    // Position in degrees is encoded with BCD digits. The DP is after the 3rd digit.
    mux->OrbitalPosition += (1000 * ((b[4] >> 4) & 0xf));
    mux->OrbitalPosition += (100 * ((b[4] & 0xf)));
    mux->OrbitalPosition += (10 * ((b[5] >> 4) & 0xf));
    mux->OrbitalPosition += (b[5] & 0xf);

    mux->WestEastFlag = (b[6] & 0x80) != 0;

    mux->Polarisation = (b[6] & 0x60) >> 5;
    switch (mux->Polarisation)
    {
      case 0:
        mux->Polarisation = BDA_POLARISATION_LINEAR_H;
        break;
      case 1:
        mux->Polarisation = BDA_POLARISATION_LINEAR_V;
        break;
      case 2:
        mux->Polarisation = BDA_POLARISATION_CIRCULAR_L;
        break;
      case 3:
        mux->Polarisation = BDA_POLARISATION_CIRCULAR_R;
        break;
      default:
        mux->Polarisation = BDA_POLARISATION_NOT_SET;
        break;
    }

    mux->IsS2 = (b[6] & 0x4) != 0;
    if (mux->IsS2)
    {
      mux->RollOff = (b[6] & 0x18) >> 3;
      switch (mux->RollOff)
      {
        case 0:
          mux->RollOff = BDA_ROLL_OFF_35;
          break;
        case 1:
          mux->RollOff = BDA_ROLL_OFF_25;
          break;
        case 2:
          mux->RollOff = BDA_ROLL_OFF_20;
          break;
      }
    }
    else
    {
      mux->RollOff = BDA_ROLL_OFF_NOT_SET;
    }

    mux->Modulation = (b[6] & 0x3);

    // Frequency in Ms/s is encoded with BCD digits. The DP is after the 3rd digit. We want the symbol rate in ks/s.
    mux->SymbolRate = (100000 * ((b[7] >> 4) & 0xf));
    mux->SymbolRate += (10000 * ((b[7] & 0xf)));
    mux->SymbolRate += (1000 * ((b[8] >> 4) & 0xf));
    mux->SymbolRate += (100 * ((b[8] & 0xf)));
    mux->SymbolRate += (10 * ((b[9] >> 4) & 0xf));
    mux->SymbolRate += ((b[9] & 0xf));

    mux->InnerFecRate = (b[10] & 0xf);
    switch (mux->InnerFecRate)
    {
      default:
      case 0:
        mux->InnerFecRate = BDA_BCC_RATE_NOT_DEFINED;
        break;
      case 1:
        mux->InnerFecRate = BDA_BCC_RATE_1_2;
        break;
      case 2:
        mux->InnerFecRate = BDA_BCC_RATE_2_3;
        break;
      case 3:
        mux->InnerFecRate = BDA_BCC_RATE_3_4;
        break;
      case 4:
        mux->InnerFecRate = BDA_BCC_RATE_5_6;
        break;
      case 5:
        mux->InnerFecRate = BDA_BCC_RATE_7_8;
        break;
      case 6:
        mux->InnerFecRate = BDA_BCC_RATE_8_9;
        break;
      case 7:
        mux->InnerFecRate = BDA_BCC_RATE_3_5;
        break;
      case 8:
        mux->InnerFecRate = BDA_BCC_RATE_4_5;
        break;
      case 9:
        mux->InnerFecRate = BDA_BCC_RATE_9_10;
        break;
    }

    // DVB-S2 properties, available from the S2 satellite delivery descriptor.
    mux->MultipleInputStreamFlag = false;
    mux->BackwardsCompatibilityIndicator = false;
    mux->ScramblingSequenceIndex = 0;
    mux->InputStreamIdentifier = 0;
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeSatelliteDeliverySystemDescriptor()", m_sName);
    return false;
  }
  return true;
}

bool CNitParser::DecodeS2SatelliteDeliverySystemDescriptor(byte* b, int length, NitSatelliteMultiplexDetail* mux)
{
  if (length < 1 || length > 5)
  {
    LogDebug("%s: invalid S2 satellite delivery system descriptor length = %d", m_sName, length);
    return false;
  }
  try
  {
    bool scrambling_sequence_selector = (b[0] & 0x80) != 0;
    if (scrambling_sequence_selector)
    {
      if (length < 4)
      {
        LogDebug("%s: invalid S2 satellite delivery system descriptor length = %d", m_sName, length);
        return false;
      }
      mux->ScramblingSequenceIndex = ((b[1] & 0x3) << 16) + (b[2] << 8) + b[3];
    }
    else
    {
      mux->ScramblingSequenceIndex = 0;
    }

    mux->MultipleInputStreamFlag = (b[0] & 0x40) != 0;
    mux->BackwardsCompatibilityIndicator = (b[0] & 0x20) != 0;

    if (mux->MultipleInputStreamFlag)
    {
      if (scrambling_sequence_selector)
      {
        if (length != 5)
        {
          LogDebug("%s: invalid S2 satellite delivery system descriptor length = %d", m_sName, length);
          return false;
        }
        mux->InputStreamIdentifier = b[4];
      }
      else
      {
        if (length < 2)
        {
          LogDebug("%s: invalid S2 satellite delivery system descriptor length = %d", m_sName, length);
          return false;
        }
        mux->InputStreamIdentifier = b[2];
      }
    }
    else
    {
      mux->InputStreamIdentifier = 0;
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeS2SatelliteDeliverySystemDescriptor()", m_sName);
    return false;
  }
  return true;
}

bool CNitParser::DecodeTerrestrialDeliverySystemDescriptor(byte* b, int length, NitTerrestrialMultiplexDetail* mux)
{
  if (length != 11)
  {
    LogDebug("%s: invalid terrestrial delivery system descriptor length = %d", m_sName, length);
    return false;
  }
  try
  {
    mux->CentreFrequency = DecodeTerrestrialFrequency(b);

    mux->Bandwidth = (b[4] >> 5);
    switch (mux->Bandwidth)
    {
      case 1:
        mux->Bandwidth = 7000;
        break;
      case 2:
        mux->Bandwidth = 6000;
        break;
      case 3:
        mux->Bandwidth = 5000;
        break;
      default:
        mux->Bandwidth = 8000;
        break;
    }

    mux->IsHighPriority = ((b[4] & 0x10) != 0);
    mux->TimeSlicingIndicator = ((b[4] & 0x08) != 0);
    mux->MpeFecIndicator = ((b[4] & 0x04) != 0);

    mux->Constellation = (b[5] >> 6);
    switch (mux->Bandwidth)
    {
      case 0:
        mux->Constellation = BDA_MOD_QPSK;
        break;
      case 1:
        mux->Constellation = BDA_MOD_16QAM;
        break;
      case 2:
        mux->Constellation = BDA_MOD_64QAM;
        break;
      default:
        mux->Constellation = BDA_MOD_NOT_SET;
        break;
    }

    mux->IndepthInterleaverUsed = (b[5] >> 5) & 1;

    mux->HierarchyInformation = (b[5] >> 3) & 3;
    switch (mux->HierarchyInformation)
    {
      case 0:
        mux->HierarchyInformation = BDA_HALPHA_NOT_DEFINED; // non-hierarchial
        break;
      case 1:
        mux->HierarchyInformation = BDA_HALPHA_1;
        break;
      case 2:
        mux->HierarchyInformation = BDA_HALPHA_2;
        break;
      case 3:
        mux->HierarchyInformation = BDA_HALPHA_4;
        break;
      default:
        mux->HierarchyInformation = BDA_HALPHA_NOT_SET;
        break;
    }

    mux->CoderateHpStream = (b[5] & 7);
    switch (mux->CoderateHpStream)
    {
      case 0:
        mux->CoderateHpStream = BDA_BCC_RATE_1_2;
        break;
      case 1:
        mux->CoderateHpStream = BDA_BCC_RATE_2_3;
        break;
      case 2:
        mux->CoderateHpStream = BDA_BCC_RATE_3_4;
        break;
      case 3:
        mux->CoderateHpStream = BDA_BCC_RATE_5_6;
        break;
      case 4:
        mux->CoderateHpStream = BDA_BCC_RATE_7_8;
        break;
      default:
        mux->CoderateHpStream = BDA_BCC_RATE_NOT_SET;
        break;
    }

    mux->CoderateLpStream = (b[6] >> 5);
    switch (mux->CoderateLpStream)
    {
      case 0:
        mux->CoderateLpStream = BDA_BCC_RATE_1_2;
        break;
      case 1:
        mux->CoderateLpStream = BDA_BCC_RATE_2_3;
        break;
      case 2:
        mux->CoderateLpStream = BDA_BCC_RATE_3_4;
        break;
      case 3:
        mux->CoderateLpStream = BDA_BCC_RATE_5_6;
        break;
      case 4:
        mux->CoderateLpStream = BDA_BCC_RATE_7_8;
        break;
      default:
        mux->CoderateLpStream = BDA_BCC_RATE_NOT_SET;
        break;
    }

    mux->GuardInterval = (b[6] >> 3) & 3;
    switch (mux->GuardInterval)
    {
      case 0:
        mux->GuardInterval = BDA_GUARD_1_32;
        break;
      case 1:
        mux->GuardInterval = BDA_GUARD_1_16;
        break;
      case 2:
        mux->GuardInterval = BDA_GUARD_1_8;
        break;
      case 3:
        mux->GuardInterval = BDA_GUARD_1_4;
        break;
      default:
        mux->GuardInterval = BDA_GUARD_NOT_SET;
        break;
    }

    mux->TransmissionMode = (b[6] >> 1) & 3;
    switch (mux->TransmissionMode)
    {
      case 0:
        mux->TransmissionMode = BDA_XMIT_MODE_2K;
        break;
      case 1:
        mux->TransmissionMode = BDA_XMIT_MODE_8K;
        break;
      case 2:
        mux->TransmissionMode = BDA_XMIT_MODE_4K;
        break;
      default:
        mux->TransmissionMode = BDA_XMIT_MODE_NOT_SET;
        break;
    }
      
    mux->OtherFrequencyFlag = (b[6] & 1) != 0;

    // DVB-T2 properties, not applicable for DVB-T.
    mux->MultipleInputStreamFlag = false;
    mux->TimeFrequencySlicingFlag = false;
    mux->PlpId = 0;
    mux->T2SystemId = 0;
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeTerrestrialDeliverySystemDescriptor()", m_sName);
    return false;
  }
  return true;
}

bool CNitParser::DecodeT2TerrestrialDeliverySystemDescriptor(byte* b, int length, NitTerrestrialMultiplexDetail* mux, map<int, int>* frequencies)
{
  if (length == 4)
  {
    LogDebug("%s: ignoring short form T2 terrestrial delivery system descriptor", m_sName);
    return false;
  }
  if (length < 6)
  {
    LogDebug("%s: invalid T2 terrestrial delivery system descriptor length = %d", m_sName, length);
    return false;
  }
  try
  {
    mux->PlpId = b[1];
    mux->T2SystemId = (b[2] << 8) + b[3];
    mux->MultipleInputStreamFlag = (b[4] & 0xc0) != 0;

    mux->Bandwidth = (b[4] & 0x3c) >> 2;
    switch (mux->Bandwidth)
    {
      case 1:
        mux->Bandwidth = 7000;
        break;
      case 2:
        mux->Bandwidth = 6000;
        break;
      case 3:
        mux->Bandwidth = 5000;
        break;
      case 4:
        mux->Bandwidth = 10000;
        break;
      case 5:
        mux->Bandwidth = 1712;
        break;
      default:
        mux->Bandwidth = 8000;
        break;
    }

    mux->GuardInterval = (b[5] >> 5);
    switch (mux->GuardInterval)
    {
      case 0:
        mux->GuardInterval = BDA_GUARD_1_32;
        break;
      case 1:
        mux->GuardInterval = BDA_GUARD_1_16;
        break;
      case 2:
        mux->GuardInterval = BDA_GUARD_1_8;
        break;
      case 3:
        mux->GuardInterval = BDA_GUARD_1_4;
        break;
      case 4:
        mux->GuardInterval = BDA_GUARD_1_128;
        break;
      case 5:
        mux->GuardInterval = BDA_GUARD_19_128;
        break;
      case 6:
        mux->GuardInterval = BDA_GUARD_19_256;
        break;
      default:
        mux->GuardInterval = BDA_GUARD_NOT_SET;
        break;
    }

    mux->TransmissionMode = (b[5] >> 2) & 0x7;
    switch (mux->TransmissionMode)
    {
      case 0:
        mux->TransmissionMode = BDA_XMIT_MODE_2K;
        break;
      case 1:
        mux->TransmissionMode = BDA_XMIT_MODE_8K;
        break;
      case 2:
        mux->TransmissionMode = BDA_XMIT_MODE_4K;
        break;
      case 3:
        mux->TransmissionMode = BDA_XMIT_MODE_1K;
        break;
      case 4:
        mux->TransmissionMode = BDA_XMIT_MODE_16K;
        break;
      case 5:
        mux->TransmissionMode = BDA_XMIT_MODE_32K;
        break;
      default:
        mux->TransmissionMode = BDA_XMIT_MODE_NOT_SET;
        break;
    }

    mux->OtherFrequencyFlag = (b[5] & 0x02) != 0;
    mux->TimeSlicingIndicator = (b[5] & 1) != 0;

    int pointer = 6;
    while (pointer + 3 < length)
    {
      int cell_id = (b[pointer] << 8) + b[pointer + 1];
      pointer += 2;

      int frequency;
      if (mux->TimeSlicingIndicator)
      {
        // TFS is not supported by the wider TV Server at this time.
        LogDebug("%s: warning, unsupported time-frequency slicing frequency set found in T2 terrestrial delivery system descriptor", m_sName);
        int frequency_loop_length = b[pointer++];
        int endOfFrequencyLoop = pointer + frequency_loop_length;
        if (endOfFrequencyLoop + 1 > length)
        {
          LogDebug("%s: invalid frequency loop length in T2 terrestrial delivery system descriptor, length = %d, pointer = %d, end of frequency loop = %d, descriptor length = %d", m_sName, frequency_loop_length, pointer, endOfFrequencyLoop, length);
          return false;
        }
        while (pointer + 3 < endOfFrequencyLoop)
        {
          frequency = (b[pointer] << 24) + (b[pointer + 1] << 16) + (b[pointer + 2] << 8) + b[pointer + 3];
          pointer += 4;

          // We'll end up recording the last frequency in the set.
          (*frequencies)[(cell_id << 8)] = frequency;
        }
      }
      else
      {
        if (pointer + 3 + 1 > length)
        {
          LogDebug("%s: invalid T2 terrestrial delivery system descriptor detected in cell loop - not enough bytes left, pointer = %d, descriptor length = %d", m_sName, pointer, length);
          return false;
        }
        frequency = (b[pointer] << 24) + (b[pointer + 1] << 16) + (b[pointer + 2] << 8) + b[pointer + 3];
        pointer += 4;

        (*frequencies)[(cell_id << 8)] = frequency;
      }

      int subcell_info_loop_length = b[pointer++];
      int endOfSubCellInfoLoop = pointer + subcell_info_loop_length;
      if (endOfSubCellInfoLoop > length)
      {
        LogDebug("%s: invalid sub-cell info loop length in T2 terrestrial delivery system descriptor, length = %d, pointer = %d, end of sub-cell info loop = %d, descriptor length = %d", m_sName, subcell_info_loop_length, pointer, endOfSubCellInfoLoop, length);
        return false;
      }
      while (pointer + 4 < endOfSubCellInfoLoop)
      {
        int cell_id_extension = b[pointer++];
        frequency = (b[pointer] << 24) + (b[pointer + 1] << 16) + (b[pointer + 2] << 8) + b[pointer + 3];
        pointer += 4;

        (*frequencies)[(cell_id << 8) + cell_id_extension] = frequency;
      }
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeT2TerrestrialDeliverySystemDescriptor()", m_sName);
    return false;
  }
  return true;
}

void CNitParser::DecodeFrequencyListDescriptor(byte* b, int length, vector<int>* frequencies)
{
  if (length < 1)
  {
    LogDebug("%s: invalid frequency list descriptor length = %d", m_sName, length);
    return;
  }
  try
  {
    int coding_type = b[0] & 0x3;
    if (coding_type != 1 && coding_type != 2 && coding_type != 3)
    {
      LogDebug("%s: unsupported coding type %d in DecodeFrequencyListDescriptor()", m_sName, coding_type);
      return;
    }
    int pointer = 1;
    while (pointer + 3 < length)
    {
      int frequency = 0;
      if (coding_type == 1)
      {
        frequency = DecodeSatelliteFrequency(&b[pointer]);
      }
      else if (coding_type == 2)
      {
        frequency = DecodeCableFrequency(&b[pointer]);
      }
      else
      {
        frequency = DecodeTerrestrialFrequency(&b[pointer]);
      }
      bool alreadyAdded = false;
      for (unsigned int i = 0; i < frequencies->size(); i++)
      {
        if (frequency == (*frequencies)[i])
        {
          alreadyAdded = true;
          break;
        }
      }
      if (!alreadyAdded && frequency > 0)
      {
        frequencies->push_back(frequency);
      }
      pointer += 4;
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeFrequencyListDescriptor()", m_sName);
  }
}

void CNitParser::DecodeServiceListDescriptor(byte* b, int length, vector<int>* services)
{
  int pointer = 0;
  while (pointer + 2 < length)
  {
    int service_id = (b[pointer] << 8) + b[pointer + 1];
    pointer += 2;
    int service_type = b[pointer++];
    services->push_back(service_id);
  }
}

void CNitParser::DecodeNameDescriptor(byte* b, int length, char** name)
{
  if (length <= 0)
  {
    return;
  }
  *name = new char[length + 1];
  if (*name == NULL)
  {
    LogDebug("%s: failed to allocate memory in DecodeNameDescriptor()", m_sName);
    return;
  }
  getString468A(b, length, *name, length + 1);
}

void CNitParser::DecodeMultilingualNameDescriptor(byte* b, int length, map<unsigned int, char*>* names)
{
  int pointer = 0;
  while (pointer + 3 < length)
  {
    unsigned int iso_639_language_code = b[pointer] + (b[pointer + 1] << 8) + (b[pointer + 2] << 16);
    pointer += 3;
    int network_name_length = b[pointer++];
    if (network_name_length > 0)
    {
      char* name = new char[network_name_length + 1];
      if (name == NULL)
      {
        LogDebug("%s: failed to allocate memory in DecodeMultilingualNameDescriptor()", m_sName);
        return;
      }
      getString468A(&b[pointer], network_name_length, name, network_name_length + 1);
      (*names)[iso_639_language_code] = name;

      pointer += network_name_length;
    }
  }
}

void CNitParser::DecodeCellFrequencyLinkDescriptor(byte* b, int length, map<int, int>* frequencies)
{
  try
  {
    int pointer = 0;
    while (pointer + 6 < length)
    {
      int cell_id = (b[pointer] << 8) + b[pointer + 1];
      pointer += 2;
      int frequency = DecodeTerrestrialFrequency(&b[pointer]);
      pointer += 4;

      (*frequencies)[cell_id << 8] = frequency;

      int subcell_info_loop_length = b[pointer++];
      if (subcell_info_loop_length > 0)
      {
        int endOfSubCellInfoLoop = pointer + subcell_info_loop_length;
        if (endOfSubCellInfoLoop > length)
        {
          LogDebug("%s: invalid sub-cell info loop length %d, pointer = %d, descriptor length = %d", m_sName, subcell_info_loop_length, pointer, length);
          return;
        }
        while (pointer + 4 < endOfSubCellInfoLoop)
        {
          int cell_id_extension = b[pointer++];
          frequency = DecodeTerrestrialFrequency(&b[pointer]);
          pointer += 4;

          (*frequencies)[(cell_id << 8) + cell_id_extension] = frequency;
        }
      }
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeCellFrequencyLinkDescriptor()", m_sName);
  }
}

void CNitParser::DecodeCountryAvailabilityDescriptor(byte* b, int length, vector<unsigned int>* availableInCountries, vector<unsigned int>* unavailableInCountries)
{
  if (length < 1)
  {
    LogDebug("%s: invalid country availability descriptor length = %d", m_sName, length);
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
    LogDebug("%s: unhandled exception in DecodeCountryAvailabilityDescriptor()", m_sName);
  }
}

void CNitParser::DecodeTargetRegionDescriptor(byte* b, int length, vector<__int64>* targetRegions)
{
  if (length < 4)
  {
    LogDebug("%s: invalid target region descriptor length = %d", m_sName, length);
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
        LogDebug("%s: invalid target region descriptor length = %d, pointer = %d, country code flag = %d, region depth = %d", m_sName, length, pointer, country_code_flag, region_depth);
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
    LogDebug("%s: unhandled exception in DecodeTargetRegionDescriptor()", m_sName);
  }
}

void CNitParser::DecodeTargetRegionNameDescriptor(byte* b, int length, map<__int64, char*>* names, unsigned int* language)
{
  if (length < 7)
  {
    LogDebug("%s: invalid target region name descriptor length = %d", m_sName, length);
    return;
  }
  try
  {
    int country_code = (b[1] << 16) + (b[2] << 8) + b[3];
    unsigned int iso_639_language_code = b[4] + (b[5] << 8) + (b[6] << 16);
    *language = iso_639_language_code;
    int pointer = 7;
    while (pointer + 1 < length)
    {
      int region_depth = b[pointer] >> 6;
      int region_name_length = (b[pointer++] & 0x3);

      // How many bytes are we expecting in this loop?
      int byteCount = region_name_length + region_depth;
      if (region_depth == 3)
      {
        byteCount++;
      }

      if (pointer + byteCount > length)
      {
        LogDebug("%s: invalid target region name descriptor length = %d, pointer = %d, region depth = %d, region name length = %d", m_sName, length, pointer, region_depth, region_name_length);
        return;
      }

      if (region_name_length > 0)
      {
        char* targetRegionName = new char[region_name_length + 1];
        if (*targetRegionName == NULL)
        {
          LogDebug("%s: failed to allocate memory in DecodeTargetRegionNameDescriptor()", m_sName);
          return;
        }
        getString468A(&b[pointer], region_name_length, targetRegionName, region_name_length + 1);
        pointer += region_name_length;

        __int64 targetRegionId = ((__int64)country_code << 32) + (b[pointer++] << 24);
        if (region_depth > 1)
        {
          targetRegionId += (b[pointer++] << 16);
          if (region_depth > 2)
          {
            targetRegionId += (b[pointer] << 8) + b[pointer + 1];
            pointer += 2;
          }
        }

        (*names)[targetRegionId] = targetRegionName;
      }
    }
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeTargetRegionNameDescriptor()", m_sName);
  }
}

int CNitParser::DecodeCableFrequency(byte* b)
{
  // Frequency in MHz is encoded with BCD digits. The DP is after the 4th digit. We want the frequency in kHz.
  int frequency = (1000000 * ((b[0] >> 4) & 0xf));
  frequency += (100000 * ((b[0] & 0xf)));
  frequency += (10000 * ((b[1] >> 4) & 0xf));
  frequency += (1000 * ((b[1] & 0xf)));
  frequency += (100 * ((b[2] >> 4) & 0xf));
  frequency += (10 * ((b[2] & 0xf)));
  frequency += ((b[3] >> 4) & 0xf);
  return frequency;
}

int CNitParser::DecodeSatelliteFrequency(byte* b)
{
  // Frequency in GHz is encoded with BCD digits. The DP is after the 3rd digit. We want the frequency in kHz.
  int frequency = (100000000 * ((b[0] >> 4) & 0xf));
  frequency += (10000000 * ((b[0] & 0xf)));
  frequency += (1000000 * ((b[1] >> 4) & 0xf));
  frequency += (100000 * ((b[1] & 0xf)));
  frequency += (10000 * ((b[2] >> 4) & 0xf));
  frequency += (1000 * ((b[2] & 0xf)));
  frequency += (100 * ((b[3] >> 4) & 0xf));
  frequency += (10 * b[3] & 0xf);
  return frequency;
}

int CNitParser::DecodeTerrestrialFrequency(byte* b)
{
  // Frequency is specified in units of 10 Hz. We want the value in kHz.
  return ((b[0] << 24) + (b[1] << 16) + (b[2] << 8) + b[3]) / 100;
}

void CNitParser::AddLogicalChannelNumbers(int originalNetworkId, int transportStreamId, map<int, int>* lcns)
{
  if (lcns != NULL)
  {
    map<int, int>::iterator it = lcns->begin();
    while (it != lcns->end())
    {
      __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + it->first;
      if (m_mLogicalChannelNumbers[serviceKey] != 0)
      {
        LogDebug("%s: replacing existing logical channel number, ONID = 0x%x, TSID = 0x%x, SID = 0x%x, LCN = %d", m_sName, originalNetworkId, transportStreamId, it->first, it->second);
      }
      else
      {
        LogDebug("%s: logical channel number, ONID = 0x%x, TSID = 0x%x, SID = 0x%x, LCN = %d", m_sName, originalNetworkId, transportStreamId, it->first, it->second);
      }
      m_mLogicalChannelNumbers[serviceKey] = it->second;
      it++;
    }
  }
}

void CNitParser::AddGroupNames(int groupId, map<unsigned int, char*>* names)
{
  map<unsigned int, char*>* existingNames = m_mGroupNames[groupId];
  if (existingNames == NULL)
  {
    existingNames = new map<unsigned int, char*>();
    m_mGroupNames[groupId] = existingNames;
  }
  map<unsigned int, char*>::iterator nameIt = names->begin();
  while (nameIt != names->end())
  {
    unsigned int language = nameIt->first;
    if ((*existingNames)[language] != NULL)
    {
      LogDebug("%s: replacing existing group name, group ID = 0x%x, language = %d, name = %s", m_sName, groupId, language, nameIt->second);
      delete[] (*existingNames)[language];
    }
    else
    {
      LogDebug("%s: group name, group ID = 0x%x, language = %d, name = %s", m_sName, groupId, language, nameIt->second);
    }
    (*existingNames)[language] = nameIt->second;
    nameIt++;
  }
}

void CNitParser::AddTargetRegionNames(map<__int64, char*>* names, unsigned int language)
{
  map<__int64, char*>::iterator nameIt = names->begin();
  while (nameIt != names->end())
  {
    map<unsigned int, char*>* existingNames = m_mTargetRegionNames[nameIt->first];
    if (existingNames == NULL)
    {
      existingNames = new map<unsigned int, char*>();
      m_mTargetRegionNames[nameIt->first] = existingNames;
    }
    else if ((*existingNames)[language] != NULL)
    {
      LogDebug("%s: replacing existing target region name, region ID = 0x%x, language = %d, name = %s", m_sName, nameIt->first, language, nameIt->second);
      delete[] (*existingNames)[language];
    }
    else
    {
      LogDebug("%s: target region name, region ID = 0x%x, language = %d, name = %s", m_sName, nameIt->first, language, nameIt->second);
    }
    (*existingNames)[language] = nameIt->second;
    nameIt++;
  }
}

void CNitParser::AddServiceDetails(int groupId, int originalNetworkId, int transportStreamId, vector<int>* serviceIds,
                        map<int, int>* cellFrequencies, vector<__int64>* targetRegions,
                        vector<unsigned int>* availableInCountries, vector<unsigned int>* unavailableInCountries)
{
  vector<int>::iterator serviceIt = serviceIds->begin();
  while (serviceIt != serviceIds->end())
  {
    __int64 serviceKey = ((__int64)originalNetworkId << 32) + (transportStreamId << 16) + *serviceIt;

    map<int, bool>* serviceGroups = m_mGroupIds[serviceKey];
    if (serviceGroups == NULL)
    {
      serviceGroups = new map<int, bool>();
      m_mGroupIds[serviceKey] = serviceGroups;
    }
    (*serviceGroups)[groupId] = true;

    map<int, bool>* serviceAvailableInCells = m_mAvailableInCells[serviceKey];
    if (serviceAvailableInCells == NULL)
    {
      serviceAvailableInCells = new map<int, bool>();
      m_mAvailableInCells[serviceKey] = serviceAvailableInCells;
    }
    map<int, int>::iterator cellIt = cellFrequencies->begin();
    while (cellIt != cellFrequencies->end())
    {
      (*serviceAvailableInCells)[cellIt->first] = true;
      cellIt++;
    }

    map<__int64, bool>* serviceTargetRegions = m_mTargetRegions[serviceKey];
    if (serviceTargetRegions == NULL)
    {
      serviceTargetRegions = new map<__int64, bool>();
      m_mTargetRegions[serviceKey] = serviceTargetRegions;
    }
    vector<__int64>::iterator regionIt = targetRegions->begin();
    while (regionIt != targetRegions->end())
    {
      (*serviceTargetRegions)[*regionIt] = true;
      regionIt++;
    }

    map<unsigned int, bool>* serviceAvailableInCountries = m_mAvailableInCountries[serviceKey];
    if (serviceAvailableInCountries == NULL)
    {
      serviceAvailableInCountries = new map<unsigned int, bool>();
      m_mAvailableInCountries[serviceKey] = serviceAvailableInCountries;
    }
    vector<unsigned int>::iterator countryIt = availableInCountries->begin();
    while (countryIt != availableInCountries->end())
    {
      (*serviceAvailableInCountries)[*countryIt] = true;
      countryIt++;
    }

    map<unsigned int, bool>* serviceUnavailableInCountries = m_mUnavailableInCountries[serviceKey];
    if (serviceUnavailableInCountries == NULL)
    {
      serviceUnavailableInCountries = new map<unsigned int, bool>();
      m_mUnavailableInCountries[serviceKey] = serviceUnavailableInCountries;
    }
    countryIt = unavailableInCountries->begin();
    while (countryIt != unavailableInCountries->end())
    {
      (*serviceUnavailableInCountries)[*countryIt] = true;
      countryIt++;
    }

    serviceIt++;
  }
}

void CNitParser::AddMultiplexDetails(NitCableMultiplexDetail* cableMux, NitSatelliteMultiplexDetail* satelliteMux,
                          NitTerrestrialMultiplexDetail* terrestrialMux,
                          map<int, int>* cellFrequencies, vector<int>* frequencies)
{
  if (terrestrialMux != NULL && cellFrequencies != NULL && cellFrequencies->size() > 0)
  {
    // The cell frequency map is populated from cell frequency link and T2 delivery
    // system descriptors. Both of those descriptors only apply for terrestrial networks.
    map<int, int>::iterator cellIt = cellFrequencies->begin();
    while (cellIt != cellFrequencies->end())
    {
      terrestrialMux->CellId = (cellIt->first >> 8);
      terrestrialMux->CellIdExtension = (cellIt->first & 0xff);
      terrestrialMux->CentreFrequency = cellIt->second;
      AddTerrestrialMux(terrestrialMux);

      cellIt++;
    }
  }
  else if (frequencies != NULL && frequencies->size() > 0)
  {
    // We have seen a frequency list descriptor. Technically that could be a list of frequencies
    // for cable, satellite or terrestrial multiplexes.
    vector<int>::iterator frequencyIt = frequencies->begin();
    if (cableMux != NULL && cableMux->SymbolRate != 0)
    {
      while (frequencyIt != frequencies->end())
      {
        cableMux->Frequency = *frequencyIt;
        AddCableMux(cableMux);

        frequencyIt++;
      }
    }
    else if (satelliteMux != NULL && satelliteMux->SymbolRate != 0)
    {
      while (frequencyIt != frequencies->end())
      {
        satelliteMux->Frequency = *frequencyIt;
        AddSatelliteMux(satelliteMux);

        frequencyIt++;
      }
    }
    else if (terrestrialMux != NULL && terrestrialMux->Bandwidth != 0)
    {
      while (frequencyIt != frequencies->end())
      {
        terrestrialMux->CentreFrequency = *frequencyIt;
        AddTerrestrialMux(terrestrialMux);

        frequencyIt++;
      }
    }
  }
  else
  {
    if (cableMux != NULL && cableMux->SymbolRate != 0)
    {
      AddCableMux(cableMux);
    }
    else if (satelliteMux != NULL && satelliteMux->SymbolRate != 0)
    {
      AddSatelliteMux(satelliteMux);
    }
    else if (terrestrialMux != NULL && terrestrialMux->Bandwidth != 0)
    {
      AddTerrestrialMux(terrestrialMux);
    }
  }
}

void CNitParser::AddCableMux(NitCableMultiplexDetail* mux)
{
  // Do we actually have the multiplex tuning details?
  if (mux == NULL || mux->SymbolRate == 0)
  {
    return;
  }

  if (mux->Frequency > MIN_CABLE_FREQUENCY_KHZ && mux->Frequency < MAX_CABLE_FREQUENCY_KHZ)
  {
    bool alreadyAdded = false;
    for (vector<NitCableMultiplexDetail*>::iterator it = m_vCableMuxes.begin(); it != m_vCableMuxes.end(); it++)
    {
      if ((*it)->Equals(mux))
      {
        alreadyAdded = true;
        break;
      }
    }
    if (!alreadyAdded)
    {
      NitCableMultiplexDetail* cableMux = new NitCableMultiplexDetail();
      if (cableMux == NULL)
      {
        LogDebug("%s: failed to allocate memory in AddCableMux()", m_sName);
        return;
      }
      mux->Clone(cableMux);
      m_vCableMuxes.push_back(cableMux);
      LogDebug("%s: cable multiplex, ONID = 0x%x, TSID = 0x%x, frequency = %d kHz, modulation = %d, symbol rate = %d ks/s",
                m_sName, mux->OriginalNetworkId, mux->TransportStreamId, mux->Frequency, mux->Modulation, mux->SymbolRate);
    }
  }
}

void CNitParser::AddSatelliteMux(NitSatelliteMultiplexDetail* mux)
{
  // Do we actually have the multiplex tuning details?
  if (mux == NULL || mux->SymbolRate == 0)
  {
    return;
  }

  if (mux->Frequency > MIN_SATELLITE_FREQUENCY_KHZ && mux->Frequency < MAX_SATELLITE_FREQUENCY_KHZ)
  {
    bool alreadyAdded = false;
    for (vector<NitSatelliteMultiplexDetail*>::iterator it = m_vSatelliteMuxes.begin(); it != m_vSatelliteMuxes.end(); it++)
    {
      if ((*it)->Equals(mux))
      {
        alreadyAdded = true;
        break;
      }
    }
    if (!alreadyAdded)
    {
      NitSatelliteMultiplexDetail* satMux = new NitSatelliteMultiplexDetail();
      if (satMux == NULL)
      {
        LogDebug("%s: failed to allocate memory in AddSatelliteMux()", m_sName);
        return;
      }
      mux->Clone(satMux);
      m_vSatelliteMuxes.push_back(satMux);
      LogDebug("%s: satellite multiplex, ONID = 0x%x, TSID = 0x%x, frequency = %d kHz, polarisation = %d, modulation = %d, symbol rate = %d ks/s, inner FEC rate = %d, orbital position = %f, is east = %d, is DVB-S2 = %d, roll-off = %d, ISI = %d",
                m_sName, mux->OriginalNetworkId, mux->TransportStreamId, mux->Frequency, mux->Polarisation,
                mux->Modulation, mux->SymbolRate, mux->InnerFecRate, mux->OrbitalPosition,
                mux->WestEastFlag, mux->IsS2, mux->RollOff, mux->InputStreamIdentifier);
    }
  }
}

void CNitParser::AddTerrestrialMux(NitTerrestrialMultiplexDetail* mux)
{
  // Do we actually have the multiplex tuning details?
  if (mux == NULL || mux->Bandwidth == 0)
  {
    return;
  }

  if (mux->CentreFrequency > MIN_TERRESTRIAL_FREQUENCY_KHZ && mux->CentreFrequency < MAX_TERRESTRIAL_FREQUENCY_KHZ)
  {
    bool alreadyAdded = false;
    for (vector<NitTerrestrialMultiplexDetail*>::iterator it = m_vTerrestrialMuxes.begin(); it != m_vTerrestrialMuxes.end(); it++)
    {
      if ((*it)->Equals(mux))
      {
        alreadyAdded = true;
        break;
      }
    }
    if (!alreadyAdded)
    {
      NitTerrestrialMultiplexDetail* terrMux = new NitTerrestrialMultiplexDetail();
      if (terrMux == NULL)
      {
        LogDebug("%s: failed to allocate memory in AddTerrestrialMux()", m_sName);
        return;
      }
      mux->Clone(terrMux);
      m_vTerrestrialMuxes.push_back(terrMux);
      LogDebug("%s: terrestrial multiplex, ONID = 0x%x, TSID = 0x%x, frequency = %d kHz, bandwidth = %d kHz, cell ID = 0x%x, cell ID extension = 0x%x, PLP ID = %d",
                m_sName, mux->OriginalNetworkId, mux->TransportStreamId, mux->CentreFrequency, mux->Bandwidth,
                mux->CellId, mux->CellIdExtension, mux->PlpId);
    }
  }
}