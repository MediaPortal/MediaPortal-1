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
#include <Windows.h>
#include <bdatypes.h>
#include "BaseScteParser.h"

extern void LogDebug(const char* fmt, ...);

CBaseScteParser::CBaseScteParser(void)
  : m_clvctParser(PID_SCTE_BASE_PID), m_tlvctParser(PID_SCTE_BASE_PID)
{
  SetPid(PID_SCTE_BASE_PID);
}

CBaseScteParser::~CBaseScteParser(void)
{
  CleanUp();
}

void CBaseScteParser::Reset()
{
  CleanUp();
  m_mServicesWithoutNames.clear();
  m_nitParser.Reset();
  m_clvctParser.Reset();
  m_clvctParser.SetCallBack(this);
  m_nttParser.Reset();
  m_nttParser.SetCallBack(this);
  m_svctParser.Reset();
  m_svctParser.SetCallBack(this);
  m_tlvctParser.Reset();
  m_tlvctParser.SetCallBack(this);
  m_bIsReady = false;
}

void CBaseScteParser::CleanUp()
{
  map<int, CChannelInfo*>::iterator it = m_mServices.begin();
  while (it != m_mServices.end())
  {
    CChannelInfo* info = it->second;
    if (info != NULL)
    {
      delete info;
      info = NULL;
    }
    it++;
  }
  m_mServices.clear();
}

bool CBaseScteParser::IsReady()
{
  return m_bIsReady;
}

int CBaseScteParser::GetChannelCount()
{
  return m_mServices.size();
}

bool CBaseScteParser::GetChannel(int index, CChannelInfo** info)
{
  if (index < 0 || index >= (int)m_mServices.size()) 
  {
    return false;
  }

  int originalIndex = index;
  map<int, CChannelInfo*>::iterator it = m_mServices.begin();
  while (index)
  {
    it++;
    index--;
  }
  CChannelInfo* toReturn = it->second;

  // Add NIT details.
  if (toReturn != NULL && toReturn->Frequency > 0 && toReturn->Frequency < 256)
  {
    toReturn->Frequency = m_nitParser.GetCarrierDefinition(toReturn->Frequency);
    toReturn->Modulation = m_nitParser.GetModulationMode(toReturn->Modulation);
  }

  *info = toReturn;
  return true;
}

void CBaseScteParser::OnNewSection(CSection& section)
{
  switch (section.table_id)
  {
    case 0xc2:
      m_nitParser.OnNewSection(section);
      break;
    case 0xc3:
      m_nttParser.OnNewSection(section);
      break;
    case 0xc4:
      m_svctParser.OnNewSection(section);
      break;
    //case 0xc7: MGT not supported
    case 0xc8:
      m_tlvctParser.OnNewSection(section);
      break;
    case 0xc9:
      m_clvctParser.OnNewSection(section);
      break;
    default:
      LogDebug("BaseScteParser: other table ID 0x%x", section.table_id);
      break;
  }

  // Update ready status.
  if (m_clvctParser.IsReady() ||
      (m_nitParser.IsReady() &&
        (m_svctParser.IsReady() || m_clvctParser.IsReady() || m_tlvctParser.IsReady()) &&
        (m_nttParser.IsReady() || m_mServicesWithoutNames.size() == 0)))
  {
    m_bIsReady = true;
  }
}

void CBaseScteParser::OnLvctReceived(int tableId, char* name, int majorChannelNumber, int minorChannelNumber, int modulationMode, 
                                      unsigned int carrierFrequency, int channelTsid, int programNumber, int etmLocation,
                                      bool accessControlled, bool hidden, int pathSelect, bool outOfBand, bool hideGuide,
                                      int serviceType, int sourceId, int videoStreamCount, int audioStreamCount,
                                      vector<unsigned int>& languages)
{
  if (programNumber == 0 || outOfBand)
  {
    // Not tunable/supported.
    return;
  }

  // Source ID may not be set when scanning for cable channels with a clear QAM
  // tuner (ie. in-band L-VCT). Also, source ID is almost certainly not unique
  // across transport streams and may not be unique within a transport stream
  // when scanning for over-the-air digital terrestrial channels.
  // These two possibilities are both problems because our service map is keyed
  // on source ID. What we do is make up an alternate service map key that we
  // hope will be unique.
  int originalSourceId = sourceId;
  CChannelInfo* info = NULL;
  if (sourceId != 0)
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(sourceId);
    if (it != m_mServices.end())
    {
      if ((it->second)->ServiceId == programNumber)
      {
        LogDebug("BaseScteParser: warning, found existing channel info for L-VCT source 0x%x (service ID = 0x%x)", sourceId, programNumber);
        info = it->second;
        m_mServicesWithoutNames.erase(sourceId);
      }
      else
      {
        sourceId = 0;
      }
    }
  }
  if (sourceId == 0)
  {
    sourceId = m_mServices.size() + 1;
    LogDebug("BaseScteParser: using fake source ID 0x%x to store service 0x%x", sourceId, programNumber);
  }
  if (info == NULL)
  {
    info = new CChannelInfo();
    m_mServices[sourceId] = info;
  }

  if (info != NULL)
  {
    if (tableId == 0xc8)
    {
      strcpy(info->ProviderName, "Terrestrial");
    }
    else
    {
      strcpy(info->ProviderName, "Cable");
    }
    if (name != NULL)
    {
      strcpy(info->ServiceName, name);
    }
    info->MajorChannel = majorChannelNumber;
    info->MinorChannel = minorChannelNumber;
    switch (modulationMode)
    {
      case 2:
        info->Modulation = BDA_MOD_64QAM;
        break;
      case 3:
        info->Modulation = BDA_MOD_256QAM;
        break;
      case 4:
        info->Modulation = BDA_MOD_8VSB;
        break;
      case 5:
        info->Modulation = BDA_MOD_16VSB;
        break;
      default:
        info->Modulation = BDA_MOD_NOT_SET;
        break;
    }
    info->Frequency = carrierFrequency / 1000;  // Hz => kHz
    info->TransportId = channelTsid;
    info->ServiceId = programNumber;
    if (accessControlled)
    {
      info->FreeCAMode = 1;
    }
    else
    {
      info->FreeCAMode = 0;
    }
    info->ServiceType = serviceType;
    info->NetworkId = originalSourceId;
    info->hasVideo = videoStreamCount;
    info->hasAudio = audioStreamCount;
  }
}

void CBaseScteParser::OnNttReceived(int sourceId, bool applicationType, char* name, unsigned int lang)
{
  if (applicationType)
  {
    // Not supported - an application/data service.
    return;
  }

  CChannelInfo* info = NULL;
  map<int, CChannelInfo*>::iterator it = m_mServices.find(sourceId);
  if (it == m_mServices.end())
  {
    info = new CChannelInfo();
    m_mServices[sourceId] = info;
  }
  else
  {
    info = it->second;
    m_mServicesWithoutNames.erase(sourceId);
  }

  if (info != NULL)
  {
    info->NetworkId = sourceId;
    if (name != NULL && strlen(name) < 254)
    {
      strcpy(info->ServiceName, name);
    }
  }
}

void CBaseScteParser::OnSvctReceived(int transmissionMedium, int vctId, int virtualChannelNumber, bool applicationVirtualChannel,
                                      int bitstreamSelect, int pathSelect, int channelType, int sourceId, byte cdsReference,
                                      int programNumber, byte mmsReference)
{
  if (applicationVirtualChannel || programNumber == 0 || sourceId == 0)
  {
    // Not tunable/supported.
    return;
  }

  CChannelInfo* info = NULL;
  map<int, CChannelInfo*>::iterator it = m_mServices.find(sourceId);
  if (it == m_mServices.end())
  {
    info = new CChannelInfo();
    m_mServices[sourceId] = info;
    m_mServicesWithoutNames[sourceId] = true;
  }
  else
  {
    info = it->second;
  }

  if (info != NULL)
  {
    info->NetworkId = sourceId;
    info->MajorChannel = virtualChannelNumber;
    info->ServiceId = programNumber;
    info->Frequency = cdsReference;
    info->Modulation = mmsReference;
    info->ServiceType = 2;  // default: ATSC digital television
    info->FreeCAMode = 1;   // default: encrypted
    switch (transmissionMedium)
    {
      case 0:
        strcpy(info->ProviderName, "Cable");
        break;
      case 1:
        strcpy(info->ProviderName, "Satellite");
        break;
      case 2:
        strcpy(info->ProviderName, "MMDS");
        break;
      case 3:
        strcpy(info->ProviderName, "SMATV");
        break;
      case 4:
        strcpy(info->ProviderName, "Terrestrial");
        break;
      default:
        strcpy(info->ProviderName, "Unknown");
        break;
    }
  }
}