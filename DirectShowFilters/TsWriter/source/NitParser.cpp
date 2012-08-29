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
}

void CNitParser::OnNewSection(CSection& sections)
{
  bool isValidTableId = false;
  for (int i = 0; i < (int)m_vTableIds.size(); i++)
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
      // Details do not yet apply...
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
    vector<char*> names;
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

      if (tag == 0x40)  // network name descriptor
      {
        DecodeNameDescriptor(&section[pointer], length, &names);
      }
      else if (tag == 0x47) // bouquet name descriptor
      {
        DecodeNameDescriptor(&section[pointer], length, &names);
      }
      else if (tag == 0x5b) // multilingual network name descriptor
      {
        DecodeMultilingualNameDescriptor(&section[pointer], length, &names);
      }
      else if (tag == 0x5c) // multilingual bouquet name descriptor
      {
        DecodeMultilingualNameDescriptor(&section[pointer], length, &names);
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
          vector<int> serviceIds;
          DecodeServiceListDescriptor(&section[pointer], length, &serviceIds);
          for (vector<int>::iterator it = serviceIds.begin(); it != serviceIds.end(); it++)
          {
            AddGroupNames(original_network_id, transport_stream_id, *it, &names);
          }
        }
        else if (tag == 0x43) // satellite delivery system descriptor
        {
          DecodeSatelliteDeliverySystemDescriptor(&section[pointer], length, &satelliteMux);
          AddSatelliteMux(&satelliteMux);
        }
        else if (tag == 0x44) // cable delivery system descriptor
        {
          DecodeCableDeliverySystemDescriptor(&section[pointer], length, &cableMux);
          AddCableMux(&cableMux, &frequencies);
        }
        else if (tag == 0x5a) // terrestrial delivery system descriptor
        {
          DecodeTerrestrialDeliverySystemDescriptor(&section[pointer], length, &terrestrialMux);
          AddTerrestrialMux(&terrestrialMux, &frequencies);
        }
        else if (tag == 0x62) // frequency list descriptor
        {
          DecodeFrequencyListDescriptor(&section[pointer], length, &frequencies);
          AddTerrestrialMux(&terrestrialMux, &frequencies);
        }
        else if (tag == 0x83) // logical channel number descriptor
        {
          DecodeLogicalChannelNumberDescriptor(&section[pointer], length, &lcns);
          AddLogicalChannelNumbers(original_network_id, transport_stream_id, &lcns);
        }
        pointer += length;
      }
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
  catch(...)
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
  m_mSeenSections.clear();
  m_vSatelliteMuxes.clear();
  m_vCableMuxes.clear();
  m_vTerrestrialMuxes.clear();
  m_vLcns.clear();
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

int CNitParser::GetLogicialChannelNumber(int networkId, int transportId, int serviceId)
{
  for (unsigned int i = 0; i < m_vLcns.size(); i++)
  {
    NitLcn& lcn = *m_vLcns[i];
    if (lcn.NetworkId == networkId &&
        lcn.TransportStreamId == transportId &&
        lcn.ServiceId == serviceId)
    {
      return lcn.Lcn;
    }
  }
  return 10000;
}

vector<char*>* CNitParser::GetGroupNames(int networkId, int transportId, int serviceId)
{
  for (unsigned int i = 0; i < m_vGroupNames.size(); i++)
  {
    NitNameSet& nameSet = *m_vGroupNames[i];
    if (nameSet.NetworkId == networkId &&
        nameSet.TransportStreamId == transportId &&
        nameSet.ServiceId == serviceId)
    {
      return &nameSet.Names;
    }
  }
  return NULL;
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
    lcns = NULL;
  }
}

void CNitParser::DecodeCableDeliverySystemDescriptor(byte* b, int length, NitCableMultiplexDetail* mux)
{
  if (length != 11)
  {
    LogDebug("%s: invalid cable delivery system descriptor length = %d", m_sName, length);
    mux = NULL;
    return;
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
    mux = NULL;
  }
}

void CNitParser::DecodeSatelliteDeliverySystemDescriptor(byte* b, int length, NitSatelliteMultiplexDetail* mux)
{
  if (length != 11)
  {
    LogDebug("%s: invalid satellite delivery system descriptor length = %d", m_sName, length);
    mux = NULL;
    return;
  }
  try
  {
    mux->Frequency = DecodeSatelliteFrequency(b);

    // Position in degrees is encoded with BCD digits. The DP is after the 3rd digit.
    mux->OrbitalPosition += (1000 * ((b[4] >> 4) & 0xf));
    mux->OrbitalPosition += (100 * ((b[4] & 0xf)));
    mux->OrbitalPosition += (10 * ((b[5] >> 4) & 0xf));
    mux->OrbitalPosition += (b[5] & 0xf);

    mux->WestEastFlag = (b[6] & 0x80) >> 7;

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

    mux->IsS2 = (b[6] & 0x4) >> 2;
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
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeSatelliteDeliverySystemDescriptor()", m_sName);
    mux = NULL;
  }
}

void CNitParser::DecodeTerrestrialDeliverySystemDescriptor(byte* b, int length, NitTerrestrialMultiplexDetail* mux)
{
  if (length != 11)
  {
    LogDebug("%s: invalid terrestrial delivery system descriptor length = %d", m_sName, length);
    mux = NULL;
    return;
  }
  try
  {
    mux->CentreFrequency = DecodeTerrestrialFrequency(b);

    mux->Bandwidth = (b[4] >> 5);
    switch (mux->Bandwidth)
    {
      case 1:
        mux->Bandwidth = 7;
        break;
      case 2:
        mux->Bandwidth = 6;
        break;
      case 3:
        mux->Bandwidth = 5;
        break;
      default:
        mux->Bandwidth = 8;
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
      
    mux->OtherFrequencyFlag = (b[6] & 1);
  }
  catch (...)
  {
    LogDebug("%s: unhandled exception in DecodeTerrestrialDeliverySystemDescriptor()", m_sName);
    mux = NULL;
  }
}

void CNitParser::DecodeFrequencyListDescriptor(byte* b, int length, vector<int>* frequencies)
{
  if (length < 1)
  {
    LogDebug("%s: invalid frequency list descriptor length = %d", m_sName, length);
    frequencies = NULL;
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
    else if (coding_type != 2 && coding_type != 3)
    {
      // We only expect cable or terrestrial frequencies.
      LogDebug("%s: unexpected coding type %d in DecodeFrequencyListDescriptor()", m_sName, coding_type);
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
      for (int i = 0; i < (int)frequencies->size(); i++)
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
    frequencies = NULL;
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

void CNitParser::DecodeNameDescriptor(byte* b, int length, vector<char*>* strings)
{
  char string[DESCRIPTOR_MAX_STRING_LENGTH + 1];
  int stringLength = min(length, DESCRIPTOR_MAX_STRING_LENGTH);
  memcpy(&string, b, stringLength);
  string[stringLength] = 0;  // NULL terminate
  strings->push_back((char*)&string);
}

void CNitParser::DecodeMultilingualNameDescriptor(byte* b, int length, vector<char*>* strings)
{
  int pointer = 0;
  while (pointer + 3 < length)
  {
    int iso_639_language_code = (b[pointer] << 16) + (b[pointer + 1] << 8) + b[pointer + 2];
    pointer += 3;
    int network_name_length = b[pointer++];

    char string[DESCRIPTOR_MAX_STRING_LENGTH + 1];
    int stringLength = min(network_name_length, DESCRIPTOR_MAX_STRING_LENGTH);
    memcpy(string, b, stringLength);
    string[stringLength] = 0;  // NULL terminate
    strings->push_back((char*)&string);

    pointer += network_name_length;
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

void CNitParser::AddLogicalChannelNumbers(int nid, int tsid, map<int, int>* lcns)
{
  if (&lcns != NULL)
  {
    for (map<int, int>::iterator it = lcns->begin(); it != lcns->end(); it++)
    {
			NitLcn lcn;
      lcn.Lcn = it->second;
			lcn.NetworkId = nid;
			lcn.TransportStreamId = tsid;
      lcn.ServiceId = it->first;
      m_vLcns.push_back(&lcn);
      LogDebug("%s: logical channel number, NID = 0x%x, TSID = 0x%x, SID = 0x%x, LCN = %d", m_sName, nid, tsid, it->first, it->second);
    }
  }
}

void CNitParser::AddGroupNames(int nid, int tsid, int sid, vector<char*>* names)
{
  if (names == NULL)
  {
    return;
  }
  for (vector<NitNameSet*>::iterator it = m_vGroupNames.begin(); it != m_vGroupNames.end(); it++)
  {
    NitNameSet* nameSet = *it;
    if (nid == nameSet->NetworkId && tsid == nameSet->TransportStreamId && sid == nameSet->ServiceId)
    {
      for (vector<char*>::iterator it2 = names->begin(); it2 != names->end(); it2++)
      {
        nameSet->Names.push_back(*it2);
      }
      return;
    }
  }
  NitNameSet n;
  n.NetworkId = nid;
  n.TransportStreamId = tsid;
  n.ServiceId = sid;
  // Copy the char* pointers. We don't copy the vector<char*>* pointer as more names
  // could be added later, and that could change the name list for multiple services
  // unintentionally.
  for (vector<char*>::iterator it = names->begin(); it != names->end(); it++)
  {
    n.Names.push_back(*it);
  }
  m_vGroupNames.push_back(&n);
}

void CNitParser::AddCableMux(NitCableMultiplexDetail* mux, vector<int>* frequencies)
{
  if (&mux != NULL && mux->SymbolRate != 0)
  {
    // Is the frequency specified in the cable delivery system descriptor, or should we expect
    // a frequency list descriptor to give us the list of all possible frequencies?
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
        m_vCableMuxes.push_back(mux);
        LogDebug("%s: cable multiplex, frequency = %d kHz, modulation = %d, symbol rate = %d ks/s",
                  m_sName, mux->Frequency, mux->Modulation, mux->SymbolRate);
		  }
    }
    // Do we have the required frequency list yet?
    else if (frequencies != NULL && frequencies->size() > 0)
    {
      for (unsigned int i = 0; i < frequencies->size(); i++)
      {
        NitCableMultiplexDetail clone;
        clone.Frequency = (*frequencies)[i];
        clone.OuterFecMethod = mux->OuterFecMethod;
        clone.Modulation = mux->Modulation;
        clone.SymbolRate = mux->SymbolRate;
        clone.InnerFecRate = mux->InnerFecRate;

			  bool alreadyAdded = false;
        for (vector<NitCableMultiplexDetail*>::iterator it = m_vCableMuxes.begin(); it != m_vCableMuxes.end(); it++)
			  {
          if ((*it)->Equals(&clone))
			    {
				    alreadyAdded = true;
				    break;
			    }
			  }
		    if (!alreadyAdded)
		    {
          m_vCableMuxes.push_back(&clone);
          LogDebug("%s: cable multiplex, frequency = %d kHz, modulation = %d, symbol rate = %d ks/s",
                    m_sName, clone.Frequency, clone.Modulation, clone.SymbolRate);
		    }
      }
    }
  }
}

void CNitParser::AddSatelliteMux(NitSatelliteMultiplexDetail* mux)
{
  if (&mux != NULL)
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
      m_vSatelliteMuxes.push_back(mux);
      LogDebug("%s: satellite multiplex, frequency = %d kHz, polarisation = %d, modulation = %d, symbol rate = %d ks/s, inner FEC rate = %d, is DVB-S2 = %d, roll-off = %d",
                m_sName, mux->Frequency, mux->Polarisation, mux->Modulation, mux->SymbolRate,
                mux->InnerFecRate, mux->IsS2, mux->RollOff);
		}
  }
}

void CNitParser::AddTerrestrialMux(NitTerrestrialMultiplexDetail* mux, vector<int>* frequencies)
{
  if (&mux != NULL && mux->Bandwidth != 0)
  {
    // Is the frequency specified in the terrestrial delivery system descriptor, or should we expect
    // a frequency list descriptor to give us the list of all possible frequencies?
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
        m_vTerrestrialMuxes.push_back(mux);
        LogDebug("%s: terrestrial multiplex, frequency = %d kHz, bandwidth = %d MHz",
                  m_sName, mux->CentreFrequency, mux->Bandwidth);
			}
    }
    // Do we have the required frequency list yet?
    else if (frequencies != NULL && frequencies->size() > 0)
    {
      for (unsigned int i = 0; i < frequencies->size(); i++)
      {
        NitTerrestrialMultiplexDetail clone;
        clone.CentreFrequency = (*frequencies)[i];
        clone.Bandwidth = mux->Bandwidth;
        clone.IsHighPriority = mux->IsHighPriority;
        clone.TimeSlicingIndicator = mux->TimeSlicingIndicator;
        clone.MpeFecIndicator = mux->MpeFecIndicator;
        clone.Constellation = mux->Constellation;
        clone.IndepthInterleaverUsed = mux->IndepthInterleaverUsed;
        clone.HierarchyInformation = mux->HierarchyInformation;
        clone.CoderateHpStream = mux->CoderateHpStream;
        clone.CoderateLpStream = mux->CoderateLpStream;
        clone.GuardInterval = mux->GuardInterval;
        clone.TransmissionMode = mux->TransmissionMode;
        clone.OtherFrequencyFlag = mux->OtherFrequencyFlag;

			  bool alreadyAdded = false;
        for (vector<NitTerrestrialMultiplexDetail*>::iterator it = m_vTerrestrialMuxes.begin(); it != m_vTerrestrialMuxes.end(); it++)
			  {
          if ((*it)->Equals(&clone))
			    {
				    alreadyAdded = true;
				    break;
			    }
			  }
			  if (!alreadyAdded)
			  {
          m_vTerrestrialMuxes.push_back(&clone);
          LogDebug("%s: terrestrial multiplex, frequency = %d kHz, bandwidth = %d MHz",
                    m_sName, clone.CentreFrequency, clone.Bandwidth);
			  }
      }
    }
  }
}