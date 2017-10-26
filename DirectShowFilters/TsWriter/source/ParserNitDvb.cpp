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
#include "ParserNitDvb.h"
#include <algorithm>  // find()
#include <cstddef>    // NULL
#include <cstring>    // strcmp(), strlen(), strncmp(), strncpy()
#include <cwchar>     // wcscmp(), wcsncpy()
#include <sstream>
#include <string>
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"
#include "OriginalNetworkIds.h"
#include "PrivateDataSpecifiers.h"
#include "TextUtil.h"
#include "Utils.h"


#define LANG_UND 0x646e75


extern void LogDebug(const wchar_t* fmt, ...);

CParserNitDvb::CParserNitDvb()
  : m_recordsService(600000), m_recordsTransmitter(600000)
{
  CParserNitDvb::SetPid(PID_NIT_DVB);
  m_tableIds.push_back(TABLE_ID_NIT_DVB_ACTUAL);
  m_tableIds.push_back(TABLE_ID_NIT_DVB_OTHER);
  SetCallBack(NULL);

  m_isOtherReady = false;
  m_otherCompleteTime = 0;
  m_enableCrcCheck = true;
  m_useCompatibilityMode = false;
  m_networkId = 0;
}

CParserNitDvb::~CParserNitDvb()
{
  CEnterCriticalSection lock(m_section);
  m_callBack = NULL;
  CleanUp();
}

void CParserNitDvb::SetPid(unsigned short pid)
{
  CEnterCriticalSection lock(m_section);
  wstringstream ss;
  ss << L"NIT DVB " << pid;
  wstring s = ss.str();
  size_t characterCount = sizeof(m_name) / sizeof(m_name[0]);
  wcsncpy(m_name, s.c_str(), characterCount);
  m_name[characterCount - 1] = NULL;
  CSectionDecoder::SetPid(pid);
  CSectionDecoder::Reset();
}

void CParserNitDvb::Reset(bool enableCrcCheck)
{
  LogDebug(L"%s: reset", m_name);
  CEnterCriticalSection lock(m_section);
  CleanUp();
  m_tableKeys.clear();
  m_nextTableKey = 1;
  m_enableCrcCheck = enableCrcCheck;
  EnableCrcCheck(enableCrcCheck);
  CSectionDecoder::Reset();
  m_seenSectionsActual.clear();
  m_unseenSectionsActual.clear();
  m_seenSectionsOther.clear();
  m_unseenSectionsOther.clear();
  m_isOtherReady = false;
  m_useCompatibilityMode = false;
  m_networkId = 0;
  LogDebug(L"%s: reset done", m_name);
}

void CParserNitDvb::SetCallBack(ICallBackNitDvb* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserNitDvb::OnNewSection(const CSection& section)
{
  try
  {
    if (!section.SectionSyntaxIndicator || !section.CurrentNextIndicator)
    {
      return;
    }
    vector<unsigned char>::const_iterator tableIt = find(m_tableIds.begin(),
                                                          m_tableIds.end(),
                                                          section.TableId);
    if (tableIt == m_tableIds.end())
    {
      return;
    }

    // Don't check the section length upper bound. Some providers ignore the
    // specification and use the top two bits which should be set to zero.
    if (section.SectionLength < 13)
    {
      LogDebug(L"%s: invalid section, length = %hu, table ID = 0x%hhx",
                m_name, section.SectionLength, section.TableId);
      return;
    }

    const unsigned char* data = section.Data;
    unsigned short extensionDescriptorsLength = ((data[8] & 0xf) << 8) | data[9];   // network or bouquet descriptors length
    //LogDebug(L"%s: table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section length = %hu, section number = %hhu, last section number = %hhu, extension descriptors length = %hu",
    //          m_name, section.TableId, section.TableIdExtension,
    //          section.VersionNumber, section.SectionLength,
    //          section.SectionNumber, section.LastSectionNumber,
    //          extensionDescriptorsLength);

    unsigned char effectiveTableId = section.TableId;
    CEnterCriticalSection lock(m_section);
    if (effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL)
    {
      if (m_useCompatibilityMode)
      {
        effectiveTableId = TABLE_ID_NIT_DVB_OTHER;
      }
      else if (m_networkId == 0)
      {
        m_networkId = section.TableIdExtension;
      }
      else if (m_networkId != section.TableIdExtension)
      {
        // We've detected multiple network definitions in NIT actual. This
        // stream is not DVB-compliant (!!!), and our table change/complete
        // logic can't handle it. Compatibility mode is a simple workaround in
        // which we parse NIT actual as if it is NIT other. As of June 2015,
        // this is required for Virgin Media (UK DVB-C)... and who knows how
        // many other cheeky DVB-C providers.
        LogDebug(L"%s: switching to compatibility mode", m_name);
        Reset(m_enableCrcCheck);
        m_useCompatibilityMode = true;
        m_networkId = section.TableIdExtension;
        return;
      }
    }

    vector<unsigned long long>* seenSections;
    vector<unsigned long long>* unseenSections;
    if (effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL)
    {
      seenSections = &m_seenSectionsActual;
      unseenSections = &m_unseenSectionsActual;
    }
    else
    {
      seenSections = &m_seenSectionsOther;
      unseenSections = &m_unseenSectionsOther;
    }

    unsigned long long sectionKey = ((unsigned long long)effectiveTableId << 32) | ((unsigned long long)section.VersionNumber << 24) | ((unsigned long long)section.TableIdExtension << 8) | section.SectionNumber;
    unsigned long long sectionGroupMask = 0xffffffff00ffff00;
    unsigned long long sectionGroupKey = sectionKey & sectionGroupMask;
    vector<unsigned long long>::const_iterator sectionIt = find(seenSections->begin(),
                                                                seenSections->end(),
                                                                sectionKey);
    if (sectionIt != seenSections->end())
    {
      // Yes. We might be ready!
      //LogDebug(L"%s: previously seen section, table ID = 0x%hhx, extension ID = %hu, section number = %hhu",
      //          m_name, effectiveTableId, section.TableIdExtension,
      //          section.SectionNumber);
      if (m_isOtherReady || m_unseenSectionsOther.size() != 0)
      {
        return;
      }

      // TS 101 211 section 4.4 recommends minimum repetition rates:
      // NIT actual = 10 seconds
      // NIT other = 10 seconds
      // BAT = 10 seconds
      // This code only handles other and BAT time out. Actual should only have
      // one NID, which makes completion deterministic.
      if (CTimeUtils::ElapsedMillis(m_otherCompleteTime) >= 5000)
      {
        if (m_unseenSectionsActual.size() == 0)
        {
          // The object passed as call-back delegate must be of the expected
          // type. Removing the cast will cause a crash, so don't do it!
          m_recordsService.RemoveExpiredRecords((ICallBackNitDvb*)this);
          m_recordsTransmitter.RemoveExpiredRecords(m_callBack);
        }

        m_isOtherReady = true;
        if (
          effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL ||
          effectiveTableId == TABLE_ID_NIT_DVB_OTHER
        )
        {
          if (m_seenSectionsOther.size() == 0)
          {
            LogDebug(L"%s: other not available", m_name);
          }
          else
          {
            LogDebug(L"%s: other ready, sections parsed = %llu, service count = %lu, transmitter count = %lu",
                      m_name, (unsigned long long)m_seenSectionsOther.size(),
                      m_recordsService.GetRecordCount(),
                      m_recordsTransmitter.GetRecordCount());
            if (m_callBack != NULL)
            {
              m_callBack->OnTableComplete(TABLE_ID_NIT_DVB_OTHER);
            }
          }
        }
        else
        {
          LogDebug(L"%s: ready, sections parsed = %llu, service count = %lu",
                    m_name, (unsigned long long)m_seenSectionsOther.size(),
                    m_recordsService.GetRecordCount());
          if (m_callBack != NULL)
          {
            m_callBack->OnTableComplete(effectiveTableId);
          }
        }
      }
      return;
    }

     // Were we expecting this section?
    sectionIt = find(unseenSections->begin(), unseenSections->end(), sectionKey);
    if (sectionIt == unseenSections->end())
    {
      // No. Is this a change/update, or just a new section group?
      bool isChange = false;
      if (effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL)
      {
        isChange = m_seenSectionsActual.size() != 0;
      }
      else
      {
        isChange = m_isOtherReady;
        vector<unsigned long long>::const_iterator tempSectionIt = seenSections->begin();
        while (tempSectionIt != seenSections->end())
        {
          if ((*tempSectionIt & sectionGroupMask) == sectionGroupKey)
          {
            isChange = true;
            tempSectionIt = seenSections->erase(tempSectionIt);
          }
          else
          {
            tempSectionIt++;
          }
        }

        tempSectionIt = unseenSections->begin();
        while (tempSectionIt != unseenSections->end())
        {
          if ((*tempSectionIt & sectionGroupMask) == sectionGroupKey)
          {
            isChange = true;
            tempSectionIt = unseenSections->erase(tempSectionIt);
          }
          else
          {
            tempSectionIt++;
          }
        }
      }

      if (isChange)
      {
        LogDebug(L"%s: changed, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu",
                  m_name, effectiveTableId, section.TableIdExtension,
                  section.VersionNumber, section.SectionNumber,
                  section.LastSectionNumber);
        m_recordsService.MarkExpiredRecords((effectiveTableId << 16) | section.TableIdExtension);
        m_recordsTransmitter.MarkExpiredRecords((effectiveTableId << 16) | section.TableIdExtension);

        if (effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL)
        {
          isChange = unseenSections->size() == 0;
          seenSections->clear();
          unseenSections->clear();
          if (isChange && m_callBack != NULL)
          {
            m_callBack->OnTableChange(TABLE_ID_NIT_DVB_ACTUAL);
          }
        }
        else if (m_isOtherReady)
        {
          m_isOtherReady = false;
          if (m_callBack != NULL)
          {
            m_callBack->OnTableChange(effectiveTableId);
          }
        }
      }
      else
      {
        LogDebug(L"%s: received, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu",
                  m_name, effectiveTableId, section.TableIdExtension,
                  section.VersionNumber, section.SectionNumber,
                  section.LastSectionNumber);
        if (
          m_callBack != NULL &&
          (
            (effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL && m_seenSectionsActual.size() == 0) ||
            (effectiveTableId != TABLE_ID_NIT_DVB_ACTUAL && m_seenSectionsOther.size() == 0)
          )
        )
        {
          m_callBack->OnTableSeen(effectiveTableId);
        }
      }

      unsigned long long baseKey = sectionKey & 0xffffffffffffff00;
      for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
      {
        unseenSections->push_back(baseKey + s);
      }
      sectionIt = find(unseenSections->begin(), unseenSections->end(), sectionKey);
    }
    else
    {
      //LogDebug(L"%s: new section, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu",
      //            m_name, effectiveTableId, section.TableIdExtension,
      //            section.VersionNumber, section.SectionNumber);
    }

    unsigned short pointer = 10;                              // points to the first byte in the extension descriptor loop
    unsigned short endOfSection = section.SectionLength - 1;  // points to the first byte in the CRC
    unsigned short endOfExtensionDescriptors = pointer + extensionDescriptorsLength;
    if (endOfExtensionDescriptors > endOfSection - 2)         // - 2 for the transport stream loop length bytes
    {
      LogDebug(L"%s: invalid section, extension descriptors length = %hu, pointer = %hu, end of section = %hu, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu",
                m_name, extensionDescriptorsLength, endOfSection,
                effectiveTableId, section.TableIdExtension,
                section.VersionNumber, section.SectionNumber);
      return;
    }

    map<unsigned long, char*> groupNames;                                         // language -> name
    vector<unsigned long> availableInCountries;
    vector<unsigned long> unavailableInCountries;
    vector<unsigned long> homeTransmitterKeys;
    unsigned long groupPrivateDataSpecifier;
    char* groupDefaultAuthority = NULL;
    vector<unsigned long long> groupTargetRegionIds;
    map<unsigned long long, map<unsigned long, char*>*> targetRegionNames;        // region ID -> [language -> name]
    map<unsigned char, char*> cyfrowyPolsatChannelCategoryNames;                  // category ID -> name
    map<unsigned short, map<unsigned long, char*>*> freesatRegionNames;           // region ID -> [language -> name]
    map<unsigned short, vector<unsigned short>*> freesatChannelCategoryIds;       // channel ID -> [category ID]
    map<unsigned short, map<unsigned long, char*>*> freesatChannelCategoryNames;  // category ID -> [language -> name]
    if (!DecodeExtensionDescriptors(data,
                                    pointer,
                                    endOfExtensionDescriptors,
                                    groupNames,
                                    availableInCountries,
                                    unavailableInCountries,
                                    homeTransmitterKeys,
                                    groupPrivateDataSpecifier,
                                    &groupDefaultAuthority,
                                    groupTargetRegionIds,
                                    targetRegionNames,
                                    cyfrowyPolsatChannelCategoryNames,
                                    freesatRegionNames,
                                    freesatChannelCategoryIds,
                                    freesatChannelCategoryNames))
    {
      LogDebug(L"%s: invalid section, table ID = 0x%hhx, extension ID = %hu, end of section = %hu",
                m_name, effectiveTableId, section.TableIdExtension,
                endOfSection);
      return;
    }

    vector<unsigned short> bouquetFreesatRegionIds;
    map<unsigned short, map<unsigned long, char*>*>::const_iterator regionIdIt = freesatRegionNames.begin();
    for ( ; regionIdIt != freesatRegionNames.end(); regionIdIt++)
    {
      bouquetFreesatRegionIds.push_back(regionIdIt->first);
    }

    AddGroupNames(NetworkOrBouquet, section.TableIdExtension, groupNames);
    AddGroupNameSets(TargetRegion, targetRegionNames);
    AddGroupNameSets(FreesatRegion, freesatRegionNames);
    AddGroupNameSets(FreesatChannelCategory, freesatChannelCategoryNames);

    map<unsigned char, char*>::const_iterator nameIt = cyfrowyPolsatChannelCategoryNames.begin();
    for ( ; nameIt != cyfrowyPolsatChannelCategoryNames.end(); nameIt++)
    {
      map<unsigned long, char*> temp;
      temp[LANG_UND] = nameIt->second;
      AddGroupNames(CyfrowyPolsatChannelCategory, nameIt->first, temp);
    }

    unsigned short transportStreamLoopLength = ((data[pointer] & 0xf) << 8) + data[pointer + 1];
    pointer += 2;
    //LogDebug(L"%s: transport stream loop length = %hu, pointer = %hu",
    //          m_name, transportStreamLoopLength, pointer);
    if (pointer + transportStreamLoopLength != endOfSection)
    {
      LogDebug(L"%s: invalid section, transport stream loop length = %hu, pointer = %hu, end of section = %hu, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu",
                m_name, transportStreamLoopLength, pointer, endOfSection,
                effectiveTableId, section.TableIdExtension,
                section.VersionNumber, section.SectionNumber);
      if (groupDefaultAuthority != NULL)
      {
        delete[] groupDefaultAuthority;
        groupDefaultAuthority = NULL;
      }
      CleanUpMapOfVectors(freesatChannelCategoryIds);
      return;
    }

    // Note: this following code relies on the assumption that each inner
    // descriptor loop will only contain one delivery system descriptor.
    while (pointer + 5 < endOfSection)
    {
      unsigned short transportStreamId = (data[pointer] << 8) + data[pointer + 1];
      pointer += 2;
      unsigned short originalNetworkId = (data[pointer] << 8) + data[pointer + 1];
      pointer += 2;

      unsigned short transportDescriptorsLength = ((data[pointer] & 0xf) << 8) + data[pointer + 1];
      pointer += 2;
      //LogDebug(L"%s: TSID = %hu, ONID = %hu, transport descriptors length = %hu, pointer = %hu",
      //          m_name, transportStreamId, originalNetworkId,
      //          transportDescriptorsLength, pointer);

      unsigned short endOfTransportDescriptors = pointer + transportDescriptorsLength;
      if (endOfTransportDescriptors > endOfSection)
      {
        LogDebug(L"%s: invalid section, transport descriptors length = %hu, pointer = %hu, end of section = %hu, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu, TSID = %hu, ONID = %hu",
                  m_name, transportDescriptorsLength, pointer, endOfSection,
                  effectiveTableId, section.TableIdExtension,
                  section.VersionNumber, section.SectionNumber,
                  transportStreamId, originalNetworkId);
        if (groupDefaultAuthority != NULL)
        {
          delete[] groupDefaultAuthority;
          groupDefaultAuthority = NULL;
        }
        CleanUpMapOfVectors(freesatChannelCategoryIds);
        return;
      }

      vector<unsigned short> serviceIds;
      map<unsigned short, map<unsigned long, unsigned short>*> logicalChannelNumbers;   // service ID -> [region ID -> logical channel number]
      map<unsigned short, bool> visibleInGuideFlags;                                    // service ID -> visible flag
      map<unsigned short, vector<unsigned char>*> norDigChannelListIds;                 // service ID -> [channel list ID]
      map<unsigned char, char*> norDigChannelListNames;                                 // channel list ID -> name
      map<unsigned short, unsigned short> openTvChannelIds;                             // service ID -> channel ID
      map<unsigned short, vector<unsigned short>*> openTvRegionIds;                     // service ID -> [region ID]
      map<unsigned short, unsigned short> freesatChannelIds;                            // service ID -> channel ID
      map<unsigned short, vector<unsigned short>*> freesatRegionIds;                    // service ID -> [region ID]
      vector<unsigned long long> transportStreamTargetRegionIds;
      char* transportStreamDefaultAuthority = NULL;
      vector<unsigned long> frequencies;
      map<unsigned long, unsigned long> cellFrequencies;                                // cell ID [16 bits] | cell ID extension [8 bits] => frequency
      CRecordNitTransmitterCable recordCable;
      CRecordNitTransmitterSatellite recordSatellite;
      CRecordNitTransmitterTerrestrial recordTerrestrial;
      if (!DecodeTransportStreamDescriptors(data,
                                            pointer,
                                            endOfTransportDescriptors,
                                            groupPrivateDataSpecifier,
                                            bouquetFreesatRegionIds,
                                            serviceIds,
                                            logicalChannelNumbers,
                                            visibleInGuideFlags,
                                            norDigChannelListIds,
                                            norDigChannelListNames,
                                            openTvChannelIds,
                                            openTvRegionIds,
                                            freesatChannelIds,
                                            freesatRegionIds,
                                            transportStreamTargetRegionIds,
                                            &transportStreamDefaultAuthority,
                                            frequencies,
                                            cellFrequencies,
                                            recordCable,
                                            recordSatellite,
                                            recordTerrestrial))
      {
        LogDebug(L"%s: invalid section, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu, TSID = %hu, ONID = %hu, end of section = %hu",
                  m_name, effectiveTableId, section.TableIdExtension,
                  section.VersionNumber, section.SectionNumber,
                  transportStreamId, originalNetworkId, endOfSection);
        if (groupDefaultAuthority != NULL)
        {
          delete[] groupDefaultAuthority;
          groupDefaultAuthority = NULL;
        }
        CleanUpMapOfVectors(freesatChannelCategoryIds);
        return;
      }

      nameIt = norDigChannelListNames.begin();
      for ( ; nameIt != norDigChannelListNames.end(); nameIt++)
      {
        map<unsigned long, char*> temp;
        temp[LANG_UND] = nameIt->second;
        AddGroupNames(NorDigChannelList, nameIt->first, temp);
      }

      // We now have a bunch of network/bouquet and transport stream details
      // that have to be recorded per-service.
      AddServices(effectiveTableId,
                  section.TableIdExtension,
                  section.SectionNumber,
                  originalNetworkId,
                  transportStreamId,
                  serviceIds,
                  logicalChannelNumbers,
                  visibleInGuideFlags,
                  (transportStreamDefaultAuthority != NULL ? transportStreamDefaultAuthority : groupDefaultAuthority),
                  freesatChannelIds,
                  freesatRegionIds,
                  freesatChannelCategoryIds,
                  openTvChannelIds,
                  openTvRegionIds,
                  norDigChannelListIds,
                  cellFrequencies,
                  ((transportStreamTargetRegionIds.size() > 0) ? transportStreamTargetRegionIds : groupTargetRegionIds),
                  availableInCountries,
                  unavailableInCountries);
      CleanUpMapOfMaps(logicalChannelNumbers);
      CleanUpMapOfVectors(freesatRegionIds);
      CleanUpMapOfVectors(openTvRegionIds);
      CleanUpMapOfVectors(norDigChannelListIds);
      if (transportStreamDefaultAuthority != NULL)
      {
        delete[] transportStreamDefaultAuthority;
        transportStreamDefaultAuthority = NULL;
      }

      // We also have transmitter details and frequencies that have to be combined.
      if (find(homeTransmitterKeys.begin(),
                homeTransmitterKeys.end(),
                GetLinkageKey(originalNetworkId, transportStreamId)) != homeTransmitterKeys.end())
      {
        recordCable.IsHomeTransmitter = true;
        recordSatellite.IsHomeTransmitter = true;
        recordTerrestrial.IsHomeTransmitter = true;
      }
      AddTransmitters(effectiveTableId,
                      section.TableIdExtension,
                      originalNetworkId,
                      transportStreamId,
                      recordCable,
                      recordSatellite,
                      recordTerrestrial,
                      cellFrequencies,
                      frequencies);
    }
    if (groupDefaultAuthority != NULL)
    {
      delete[] groupDefaultAuthority;
      groupDefaultAuthority = NULL;
    }
    CleanUpMapOfVectors(freesatChannelCategoryIds);

    if (pointer != endOfSection)
    {
      LogDebug(L"%s: section parsing error, pointer = %hu, end of section = %hu, table ID = 0x%hhx, extension ID = %hu, version number = %hhu, section number = %hhu",
                m_name, pointer, endOfSection, effectiveTableId,
                section.TableIdExtension, section.VersionNumber,
                section.SectionNumber);
      return;
    }

    seenSections->push_back(sectionKey);
    unseenSections->erase(sectionIt);
    if (unseenSections->size() == 0)
    {
      if (effectiveTableId == TABLE_ID_NIT_DVB_ACTUAL)
      {
        if (m_isOtherReady)
        {
          // The object passed as call-back delegate must be of the expected
          // type. Removing the cast will cause a crash, so don't do it!
          m_recordsService.RemoveExpiredRecords((ICallBackNitDvb*)this);
          m_recordsTransmitter.RemoveExpiredRecords(m_callBack);
        }
        LogDebug(L"%s: actual ready, sections parsed = %llu, service count = %lu, transmitter count = %lu",
                  m_name, (unsigned long long)m_seenSectionsActual.size(),
                  m_recordsService.GetRecordCount(),
                  m_recordsTransmitter.GetRecordCount());
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_NIT_DVB_ACTUAL);
        }
      }
      else
      {
        // We can't assume that we've seen all sections yet, because sections
        // for another network/bouquet may not have been received.
        m_otherCompleteTime = clock();
      }
    }
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in OnNewSection()", m_name);
  }
}

bool CParserNitDvb::IsSeenActual() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSectionsActual.size() != 0;
}

bool CParserNitDvb::IsSeenOther() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSectionsOther.size() != 0;
}

bool CParserNitDvb::IsReadyActual() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSectionsActual.size() != 0 && m_unseenSectionsActual.size() == 0;
}

bool CParserNitDvb::IsReadyOther() const
{
  CEnterCriticalSection lock(m_section);
  return m_isOtherReady && IsSeenOther();
}

bool CParserNitDvb::GetService(unsigned short originalNetworkId,
                                unsigned short transportStreamId,
                                unsigned short serviceId,
                                unsigned short& freesatChannelId,
                                unsigned short& openTvChannelId,
                                unsigned long long* logicalChannelNumbers,
                                unsigned short& logicalChannelNumberCount,
                                bool& visibleInGuide,
                                unsigned short* groupIds,
                                unsigned char& groupIdCount,
                                unsigned long* availableInCells,
                                unsigned char& availableInCellCount,
                                unsigned long long* targetRegionIds,
                                unsigned char& targetRegionIdCount,
                                unsigned long* freesatRegionIds,
                                unsigned char& freesatRegionIdCount,
                                unsigned long* openTvRegionIds,
                                unsigned char& openTvRegionIdCount,
                                unsigned short* freesatChannelCategoryIds,
                                unsigned char& freesatChannelCategoryIdCount,
                                unsigned char* norDigChannelListIds,
                                unsigned char& norDigChannelListIdCount,
                                unsigned long* availableInCountries,
                                unsigned char& availableInCountryCount,
                                unsigned long* unavailableInCountries,
                                unsigned char& unavailableInCountryCount) const
{
  CEnterCriticalSection lock(m_section);

  unsigned long long dvbId = ((unsigned long long)originalNetworkId << 32) | ((unsigned long long)transportStreamId << 16) | serviceId;
  map<unsigned long long, vector<CRecordNitService*>*>::const_iterator servicesByDvbIdIt = m_cacheServiceRecordsByDvbId.find(dvbId);
  if (servicesByDvbIdIt == m_cacheServiceRecordsByDvbId.end() || servicesByDvbIdIt->second->size() == 0)
  {
    // Are there any common details for the entire transport stream?
    dvbId &= 0xffffffffffff0000;
    servicesByDvbIdIt = m_cacheServiceRecordsByDvbId.find(dvbId);
    if (servicesByDvbIdIt == m_cacheServiceRecordsByDvbId.end() || servicesByDvbIdIt->second->size() == 0)
    {
      // Not an error.
      return false;
    }
  }

  vector<unsigned long> expandedRegionIds;
  if (servicesByDvbIdIt->second->size() == 1)
  {
    CRecordNitService* record = servicesByDvbIdIt->second->at(0);
    freesatChannelId = record->FreesatChannelId;
    openTvChannelId = record->OpenTvChannelId;

    if (logicalChannelNumbers == NULL)
    {
      logicalChannelNumberCount = 0;
    }
    else
    {
      unsigned short requiredLcnCount;
      vector<unsigned long long> tempLcns;
      map<unsigned long, unsigned short>::const_iterator lcnIt = record->LogicalChannelNumbers.begin();
      for ( ; lcnIt != record->LogicalChannelNumbers.end(); lcnIt++)
      {
        tempLcns.push_back(((unsigned long long)record->TableId << 56) | ((unsigned long long)record->TableIdExtension << 40) | ((unsigned long long)lcnIt->first << 16) | lcnIt->second);
      }
      if (!CUtils::CopyVectorToArray(tempLcns,
                                      logicalChannelNumbers,
                                      logicalChannelNumberCount,
                                      requiredLcnCount))
      {
        LogDebug(L"%s: insufficient logical channel number array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hu, actual size = %hu",
                  m_name, originalNetworkId, transportStreamId, serviceId,
                  requiredLcnCount, logicalChannelNumberCount);
      }
    }

    visibleInGuide = record->VisibleInGuide;

    if (groupIds == NULL || groupIdCount == 0)
    {
      groupIdCount = 0;
      if (groupIds != NULL)
      {
        LogDebug(L"%s: insufficient group ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = 1, actual size = 0",
                  m_name, originalNetworkId, transportStreamId, serviceId);
      }
    }
    else
    {
      groupIds[0] = record->TableIdExtension;
      groupIdCount = 1;
    }

    unsigned char requiredCount;
    if (!CUtils::CopyVectorToArray(record->AvailableInCells,
                                    availableInCells,
                                    availableInCellCount,
                                    requiredCount) && availableInCells != NULL)
    {
      LogDebug(L"%s: insufficient available in cell array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, availableInCellCount);
    }
    if (!CUtils::CopyVectorToArray(record->TargetRegionIds,
                                    targetRegionIds,
                                    targetRegionIdCount,
                                    requiredCount) && targetRegionIds != NULL)
    {
      LogDebug(L"%s: insufficient target region ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, targetRegionIdCount);
    }
    ExpandRegionIds(record->FreesatRegionIds, record->TableIdExtension, expandedRegionIds);
    if (!CUtils::CopyVectorToArray(expandedRegionIds,
                                    freesatRegionIds,
                                    freesatRegionIdCount,
                                    requiredCount) && freesatRegionIds != NULL)
    {
      LogDebug(L"%s: insufficient Freesat region ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, freesatRegionIdCount);
    }
    ExpandRegionIds(record->OpenTvRegionIds, record->TableIdExtension, expandedRegionIds);
    if (!CUtils::CopyVectorToArray(expandedRegionIds,
                                    openTvRegionIds,
                                    openTvRegionIdCount,
                                    requiredCount) && openTvRegionIds != NULL)
    {
      LogDebug(L"%s: insufficient OpenTV region ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, openTvRegionIdCount);
    }
    if (!CUtils::CopyVectorToArray(record->FreesatChannelCategoryIds,
                                    freesatChannelCategoryIds,
                                    freesatChannelCategoryIdCount,
                                    requiredCount) && freesatChannelCategoryIds != NULL)
    {
      LogDebug(L"%s: insufficient Freesat channel category ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, freesatChannelCategoryIdCount);
    }
    if (!CUtils::CopyVectorToArray(record->NorDigChannelListIds,
                                    norDigChannelListIds,
                                    norDigChannelListIdCount,
                                    requiredCount) && norDigChannelListIds != NULL)
    {
      LogDebug(L"%s: insufficient NorDig channel list ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, norDigChannelListIdCount);
    }
    if (!CUtils::CopyVectorToArray(record->AvailableInCountries,
                                    availableInCountries,
                                    availableInCountryCount,
                                    requiredCount) && availableInCountries != NULL)
    {
      LogDebug(L"%s: insufficient available in country array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, availableInCountryCount);
    }
    if (!CUtils::CopyVectorToArray(record->UnavailableInCountries,
                                    unavailableInCountries,
                                    unavailableInCountryCount,
                                    requiredCount) && unavailableInCountries != NULL)
    {
      LogDebug(L"%s: insufficient unavailable in country array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                m_name, originalNetworkId, transportStreamId, serviceId,
                requiredCount, unavailableInCountryCount);
    }
    return true;
  }

  // Combine the information for all network/bouquet service records. Build
  // temporary maps to generate distinct results.
  freesatChannelId = 0;
  openTvChannelId = 0;
  map<unsigned long long, bool> tempLcns;
  visibleInGuide = false;
  map<unsigned short, bool> tempGroupIds;
  map<unsigned long, bool> tempAvailableInCells;
  map<unsigned long long, bool> tempTargetRegionIds;
  map<unsigned long, bool> tempFreesatRegionIds;
  map<unsigned long, bool> tempOpenTvRegionIds;
  map<unsigned short, bool> tempFreesatChannelCategoryIds;
  map<unsigned char, bool> tempNorDigChannelListIds;
  map<unsigned long, bool> tempAvailableInCountries;
  map<unsigned long, bool> tempUnavailableInCountries;
  vector<CRecordNitService*>::const_iterator it = servicesByDvbIdIt->second->begin();
  for ( ; it != servicesByDvbIdIt->second->end(); it++)
  {
    CRecordNitService* record = *it;
    if (record == NULL)
    {
      continue;
    }

    if (record->FreesatChannelId != 0)
    {
      if (freesatChannelId == 0)
      {
        freesatChannelId = record->FreesatChannelId;
      }
      else if (freesatChannelId != record->FreesatChannelId)
      {
        LogDebug(L"%s: Freesat channel ID conflict, ONID = %hu, TSID = %hu, service ID = %hu, Freesat CID = %hu, alternative Freesat CID = %hu",
                  m_name, originalNetworkId, transportStreamId, serviceId,
                  freesatChannelId, record->FreesatChannelId);
      }
    }

    if (record->OpenTvChannelId != 0)
    {
      if (openTvChannelId == 0)
      {
        openTvChannelId = record->OpenTvChannelId;
      }
      else if (openTvChannelId != record->OpenTvChannelId)
      {
        LogDebug(L"%s: OpenTV channel ID conflict, ONID = %hu, TSID = %hu, service ID = %hu, OpenTV CID = %hu, alternative OpenTV CID = %hu",
                  m_name, originalNetworkId, transportStreamId, serviceId,
                  openTvChannelId, record->OpenTvChannelId);
      }
    }

    map<unsigned long, unsigned short>::const_iterator lcnIt = record->LogicalChannelNumbers.begin();
    for ( ; lcnIt != record->LogicalChannelNumbers.end(); lcnIt++)
    {
      tempLcns[((unsigned long long)record->TableId << 56) | ((unsigned long long)record->TableIdExtension << 40) | ((unsigned long long)lcnIt->first << 16) | lcnIt->second] = true;
    }

    visibleInGuide |= record->VisibleInGuide;

    tempGroupIds[record->TableIdExtension] = true;
    AggregateSet(record->AvailableInCells, tempAvailableInCells);
    AggregateSet(record->TargetRegionIds, tempTargetRegionIds);
    ExpandRegionIds(record->FreesatRegionIds, record->TableIdExtension, expandedRegionIds);
    AggregateSet(expandedRegionIds, tempFreesatRegionIds);
    ExpandRegionIds(record->OpenTvRegionIds, record->TableIdExtension, expandedRegionIds);
    AggregateSet(expandedRegionIds, tempOpenTvRegionIds);
    AggregateSet(record->FreesatChannelCategoryIds, tempFreesatChannelCategoryIds);
    AggregateSet(record->NorDigChannelListIds, tempNorDigChannelListIds);
    AggregateSet(record->AvailableInCountries, tempAvailableInCountries);
    AggregateSet(record->UnavailableInCountries, tempUnavailableInCountries);
  }

  if (
    !GetSetValues(tempLcns, logicalChannelNumbers, logicalChannelNumberCount) &&
    logicalChannelNumbers != NULL
  )
  {
    LogDebug(L"%s: insufficient logical channel number array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempLcns.size(), groupIdCount);
  }
  if (
    !GetSetValues(tempGroupIds, groupIds, groupIdCount) &&
    groupIds != NULL
  )
  {
    LogDebug(L"%s: insufficient group ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempGroupIds.size(), groupIdCount);
  }
  if (
    !GetSetValues(tempAvailableInCells, availableInCells, availableInCellCount) &&
    availableInCells != NULL
  )
  {
    LogDebug(L"%s: insufficient available in cell array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempAvailableInCells.size(), availableInCellCount);
  }
  if (
    !GetSetValues(tempTargetRegionIds, targetRegionIds, targetRegionIdCount) &&
    targetRegionIds != NULL
  )
  {
    LogDebug(L"%s: insufficient target region ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempTargetRegionIds.size(), targetRegionIdCount);
  }
  if (
    !GetSetValues(tempFreesatRegionIds, freesatRegionIds, freesatRegionIdCount) &&
    freesatRegionIds != NULL
  )
  {
    LogDebug(L"%s: insufficient Freesat region ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempFreesatRegionIds.size(), freesatRegionIdCount);
  }
  if (
    !GetSetValues(tempOpenTvRegionIds, openTvRegionIds, openTvRegionIdCount) &&
    openTvRegionIds != NULL
  )
  {
    LogDebug(L"%s: insufficient OpenTV region ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempOpenTvRegionIds.size(), openTvRegionIdCount);
  }
  if (
    !GetSetValues(tempFreesatChannelCategoryIds,
                  freesatChannelCategoryIds,
                  freesatChannelCategoryIdCount) &&
    freesatChannelCategoryIds != NULL
  )
  {
    LogDebug(L"%s: insufficient Freesat channel category ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempFreesatChannelCategoryIds.size(),
              freesatChannelCategoryIdCount);
  }
  if (
    !GetSetValues(tempNorDigChannelListIds, norDigChannelListIds, norDigChannelListIdCount) &&
    norDigChannelListIds != NULL
  )
  {
    LogDebug(L"%s: insufficient NorDig channel list ID array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempNorDigChannelListIds.size(), norDigChannelListIdCount);
  }
  if (
    !GetSetValues(tempAvailableInCountries, availableInCountries, availableInCountryCount) &&
    availableInCountries != NULL
  )
  {
    LogDebug(L"%s: insufficient available in country array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempAvailableInCountries.size(), availableInCountryCount);
  }
  if (
    !GetSetValues(tempUnavailableInCountries, unavailableInCountries, unavailableInCountryCount) &&
    unavailableInCountries != NULL
  )
  {
    LogDebug(L"%s: insufficient unavailable in country array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %llu, actual size = %hhu",
              m_name, originalNetworkId, transportStreamId, serviceId,
              tempUnavailableInCountries.size(), unavailableInCountryCount);
  }
  return true;
}

unsigned char CParserNitDvb::GetNetworkNameCount(unsigned short networkId) const
{
  return GetNameCount(NetworkOrBouquet, networkId);
}

bool CParserNitDvb::GetNetworkNameByIndex(unsigned short networkId,
                                          unsigned char index,
                                          unsigned long& language,
                                          char* name,
                                          unsigned short& nameBufferSize) const
{
  return GetNameByIndex(NetworkOrBouquet, networkId, index, language, name, nameBufferSize);
}

bool CParserNitDvb::GetNetworkNameByLanguage(unsigned short networkId,
                                              unsigned long language,
                                              char* name,
                                              unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(NetworkOrBouquet, networkId, language, name, nameBufferSize);
}

unsigned char CParserNitDvb::GetTargetRegionNameCount(unsigned long long regionId) const
{
  return GetNameCount(TargetRegion, regionId);
}

bool CParserNitDvb::GetTargetRegionNameByIndex(unsigned long long regionId,
                                                unsigned char index,
                                                unsigned long& language,
                                                char* name,
                                                unsigned short& nameBufferSize) const
{
  return GetNameByIndex(TargetRegion, regionId, index, language, name, nameBufferSize);
}

bool CParserNitDvb::GetTargetRegionNameByLanguage(unsigned long long regionId,
                                                  unsigned long language,
                                                  char* name,
                                                  unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(TargetRegion, regionId, language, name, nameBufferSize);
}

unsigned char CParserNitDvb::GetCyfrowyPolsatChannelCategoryNameCount(unsigned char categoryId) const
{
  return GetNameCount(CyfrowyPolsatChannelCategory, categoryId);
}

bool CParserNitDvb::GetCyfrowyPolsatChannelCategoryNameByIndex(unsigned char categoryId,
                                                                unsigned char index,
                                                                unsigned long& language,
                                                                char* name,
                                                                unsigned short& nameBufferSize) const
{
  return GetNameByIndex(CyfrowyPolsatChannelCategory,
                        categoryId,
                        index,
                        language,
                        name,
                        nameBufferSize);
}

bool CParserNitDvb::GetCyfrowyPolsatChannelCategoryNameByLanguage(unsigned char categoryId,
                                                                  unsigned long language,
                                                                  char* name,
                                                                  unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(CyfrowyPolsatChannelCategory,
                            categoryId,
                            language,
                            name,
                            nameBufferSize);
}

unsigned char CParserNitDvb::GetFreesatRegionNameCount(unsigned short regionId) const
{
  return GetNameCount(FreesatRegion, regionId);
}

bool CParserNitDvb::GetFreesatRegionNameByIndex(unsigned short regionId,
                                                unsigned char index,
                                                unsigned long& language,
                                                char* name,
                                                unsigned short& nameBufferSize) const
{
  return GetNameByIndex(FreesatRegion, regionId, index, language, name, nameBufferSize);
}

bool CParserNitDvb::GetFreesatRegionNameByLanguage(unsigned short regionId,
                                                    unsigned long language,
                                                    char* name,
                                                    unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(FreesatRegion, regionId, language, name, nameBufferSize);
}

unsigned char CParserNitDvb::GetFreesatChannelCategoryNameCount(unsigned short categoryId) const
{
  return GetNameCount(FreesatChannelCategory, categoryId);
}

bool CParserNitDvb::GetFreesatChannelCategoryNameByIndex(unsigned short categoryId,
                                                          unsigned char index,
                                                          unsigned long& language,
                                                          char* name,
                                                          unsigned short& nameBufferSize) const
{
  return GetNameByIndex(FreesatChannelCategory,
                        categoryId,
                        index,
                        language,
                        name,
                        nameBufferSize);
}

bool CParserNitDvb::GetFreesatChannelCategoryNameByLanguage(unsigned short categoryId,
                                                            unsigned long language,
                                                            char* name,
                                                            unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(FreesatChannelCategory, categoryId, language, name, nameBufferSize);
}

unsigned char CParserNitDvb::GetNorDigChannelListNameCount(unsigned char channelListId) const
{
  return GetNameCount(NorDigChannelList, channelListId);
}

bool CParserNitDvb::GetNorDigChannelListNameByIndex(unsigned char channelListId,
                                                    unsigned char index,
                                                    unsigned long& language,
                                                    char* name,
                                                    unsigned short& nameBufferSize) const
{
  return GetNameByIndex(NorDigChannelList,
                        channelListId,
                        index,
                        language,
                        name,
                        nameBufferSize);
}

bool CParserNitDvb::GetNorDigChannelListNameByLanguage(unsigned char channelListId,
                                                        unsigned long language,
                                                        char* name,
                                                        unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(NorDigChannelList, channelListId, language, name, nameBufferSize);
}

bool CParserNitDvb::GetDefaultAuthority(unsigned short originalNetworkId,
                                        unsigned short transportStreamId,
                                        unsigned short serviceId,
                                        char* defaultAuthority,
                                        unsigned short& defaultAuthorityBufferSize) const
{
  CEnterCriticalSection lock(m_section);

  unsigned long long dvbId = ((unsigned long long)originalNetworkId << 32) | ((unsigned long long)transportStreamId << 16) | serviceId;
  map<unsigned long long, vector<CRecordNitService*>*>::const_iterator servicesByDvbIdIt = m_cacheServiceRecordsByDvbId.find(dvbId);
  if (servicesByDvbIdIt == m_cacheServiceRecordsByDvbId.end() || servicesByDvbIdIt->second->size() == 0)
  {
    // Is the default authority common for the entire transport stream?
    dvbId &= 0xffffffffffff0000;
    servicesByDvbIdIt = m_cacheServiceRecordsByDvbId.find(dvbId);
    if (servicesByDvbIdIt == m_cacheServiceRecordsByDvbId.end() || servicesByDvbIdIt->second->size() == 0)
    {
      // Not an error. Just means "default authority not found".
      return false;
    }
  }

  char* preferredDefaultAuthority = servicesByDvbIdIt->second->at(0)->DefaultAuthority;
  if (preferredDefaultAuthority == NULL)
  {
    // Not an error. Just means "default authority not available from here".
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(preferredDefaultAuthority,
                                  defaultAuthority,
                                  defaultAuthorityBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"%s: insufficient default authority buffer size, ONID = %hu, TSID = %hu, required size = %hu, actual size = %hu",
              m_name, originalNetworkId, transportStreamId, requiredBufferSize,
              defaultAuthorityBufferSize);
  }
  return true;
}

unsigned short CParserNitDvb::GetTransmitterCount() const
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_recordsTransmitter.GetRecordCount();
}

bool CParserNitDvb::GetTransmitter(unsigned short index,
                                    unsigned char& tableId,
                                    unsigned short& networkId,
                                    unsigned short& originalNetworkId,
                                    unsigned short& transportStreamId,
                                    bool& isHomeTransmitter,
                                    unsigned long& broadcastStandard,
                                    unsigned long* frequencies,
                                    unsigned char& frequencyCount,
                                    unsigned char& polarisation,
                                    unsigned char& modulation,
                                    unsigned long& symbolRate,
                                    unsigned short& bandwidth,
                                    unsigned char& innerFecRate,
                                    unsigned char& rollOffFactor,
                                    short& longitude,
                                    unsigned short& cellId,
                                    unsigned char& cellIdExtension,
                                    bool& isMultipleInputStream,
                                    unsigned char& plpId) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsTransmitter.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"%s: invalid transmitter index, index = %hu, record count = %lu",
              m_name, index, m_recordsTransmitter.GetRecordCount());
    return false;
  }

  CRecordNitTransmitter* recordTransmitter = dynamic_cast<CRecordNitTransmitter*>(record);
  if (recordTransmitter == NULL)
  {
    LogDebug(L"%s: invalid transmitter record, index = %hu", m_name, index);
    return false;
  }

  tableId = recordTransmitter->TableId;
  networkId = recordTransmitter->NetworkId;
  originalNetworkId = recordTransmitter->OriginalNetworkId;
  transportStreamId = recordTransmitter->TransportStreamId;
  isHomeTransmitter = recordTransmitter->IsHomeTransmitter;

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(recordTransmitter->Frequencies,
                                  frequencies,
                                  frequencyCount,
                                  requiredCount))
  {
    LogDebug(L"%s: insufficient frequency array size, index = %hu, table ID = 0x%hhx, NID = %hu, ONID = %hu, TSID = %hu, required size = %hhu, actual size = %hhu",
              m_name, tableId, networkId, originalNetworkId, transportStreamId,
              requiredCount, frequencyCount);
  }

  CRecordNitTransmitterCable* recordCable = dynamic_cast<CRecordNitTransmitterCable*>(recordTransmitter);
  if (recordCable != NULL)
  {
    if (recordCable->IsC2)
    {
      broadcastStandard = 0x0020;   // This is as-per the TV Server database BroadcastStandard.DvbC2 value.
      if (recordCable->ActiveOfdmSymbolDuration == 0)
      {
        bandwidth = 8000;
      }
      else if (recordCable->ActiveOfdmSymbolDuration == 1)
      {
        bandwidth = 6000;
      }
      else
      {
        LogDebug(L"%s: unhandled DVB-C2 active OFDM symbol duration, index = %hu, table ID = 0x%hhx, NID = %hu, ONID = %hu, TSID = %hu, duration = %hhu",
                  m_name, index, tableId, networkId, originalNetworkId,
                  transportStreamId, recordCable->ActiveOfdmSymbolDuration);
        bandwidth = 6000;
      }
      isMultipleInputStream = true;
      plpId = recordCable->PlpId;
    }
    else
    {
      broadcastStandard = 0x0010;   // This is as-per the TV Server database BroadcastStandard.DvbC value.
      modulation = recordCable->Modulation;
      symbolRate = recordCable->SymbolRate;
      innerFecRate = recordCable->InnerFecRate;
      isMultipleInputStream = false;
    }

    polarisation = 0;
    rollOffFactor = 0;
    longitude = 0;
    cellId = 0;
    cellIdExtension = 0;
    return true;
  }

  CRecordNitTransmitterSatellite* recordSatellite = dynamic_cast<CRecordNitTransmitterSatellite*>(recordTransmitter);
  if (recordSatellite != NULL)
  {
    polarisation = recordSatellite->Polarisation;
    modulation = recordSatellite->Modulation;
    symbolRate = recordSatellite->SymbolRate;
    innerFecRate = recordSatellite->InnerFecRate;
    rollOffFactor = recordSatellite->RollOff;
    longitude = ((recordSatellite->WestEastFlag ? 1 : -1) * recordSatellite->OrbitalPosition);
    if (recordSatellite->IsS2)
    {
      broadcastStandard = 0x0200;   // This is as-per the TV Server database BroadcastStandard.DvbS2 value.
      isMultipleInputStream = recordSatellite->MultipleInputStreamFlag;
      plpId = recordSatellite->InputStreamIdentifier;
    }
    else
    {
      broadcastStandard = 0x0100;   // This is as-per the TV Server database BroadcastStandard.DvbS value.
      isMultipleInputStream = false;
    }

    bandwidth = 0;
    cellId = 0;
    cellIdExtension = 0;
    return true;
  }

  CRecordNitTransmitterTerrestrial* recordTerrestrial = dynamic_cast<CRecordNitTransmitterTerrestrial*>(recordTransmitter);
  if (recordTerrestrial != NULL)
  {
    bandwidth = recordTerrestrial->Bandwidth;
    cellId = recordTerrestrial->CellId;
    cellIdExtension = recordTerrestrial->CellIdExtension;
    if (recordTerrestrial->IsT2)
    {
      broadcastStandard = 0x2000;   // This is as-per the TV Server database BroadcastStandard.DvbT2 value.
      isMultipleInputStream = recordTerrestrial->MultipleInputStreamFlag;
      plpId = recordTerrestrial->PlpId;
    }
    else
    {
      broadcastStandard = 0x1000;   // This is as-per the TV Server database BroadcastStandard.DvbT value.
      isMultipleInputStream = false;
    }

    polarisation = 0;
    modulation = 0;
    symbolRate = 0;
    innerFecRate = 0;
    rollOffFactor = 0;
    longitude = 0;
    return true;
  }

  LogDebug(L"%s: unhandled transmitter record type, index = %hu, table ID = 0x%hhx, NID = %hu, ONID = %hu, TSID = %hu",
            m_name, index, tableId, networkId, originalNetworkId,
            transportStreamId);
  return false;
}

void CParserNitDvb::CleanUp()
{
  // Caller should have already acquired the lock.
  CleanUpNames(m_groupNames);
  m_recordsService.RemoveAllRecords();
  m_recordsTransmitter.RemoveAllRecords();
  CleanUpMapOfVectors(m_cacheServiceRecordsByDvbId);
}

template<class T> void CParserNitDvb::CleanUpNames(map<T, map<unsigned long, char*>*>& names)
{
  map<T, map<unsigned long, char*>*>::iterator nameSetIt = names.begin();
  for ( ; nameSetIt != names.end(); nameSetIt++)
  {
    map<unsigned long, char*>* nameSet = nameSetIt->second;
    if (nameSet != NULL)
    {
      CUtils::CleanUpStringSet(*nameSet);
      delete nameSet;
      nameSetIt->second = NULL;
    }
  }
  names.clear();
}

template<class T1, class T2> void CParserNitDvb::CleanUpMapOfVectors(map<T1, vector<T2>*>& mapOfVectors)
{
  map<T1, vector<T2>*>::iterator mapIt = mapOfVectors.begin();
  for ( ; mapIt != mapOfVectors.end(); mapIt++)
  {
    vector<T2>* v = mapIt->second;
    if (v != NULL)
    {
      delete v;
      mapIt->second = NULL;
    }
  }
  mapOfVectors.clear();
}

void CParserNitDvb::CleanUpMapOfMaps(map<unsigned short, map<unsigned long, unsigned short>*>& mapOfMaps)
{
  map<unsigned short, map<unsigned long, unsigned short>*>::iterator mapIt = mapOfMaps.begin();
  for ( ; mapIt != mapOfMaps.end(); mapIt++)
  {
    if (mapIt->second != NULL)
    {
      delete mapIt->second;
      mapIt->second = NULL;
    }
  }
  mapOfMaps.clear();
}

void CParserNitDvb::ExpandRegionIds(const vector<unsigned short>& regionIds,
                                    unsigned short tableIdExtension,
                                    vector<unsigned long>& expandedRegionIds)
{
  expandedRegionIds.clear();
  vector<unsigned short>::const_iterator it = regionIds.begin();
  for ( ; it != regionIds.end(); it++)
  {
    expandedRegionIds.push_back(((unsigned long)tableIdExtension << 16) | *it);
  }
}

template<class T> void CParserNitDvb::AggregateSet(const vector<T>& values, map<T, bool>& set)
{
  vector<T>::const_iterator it = values.begin();
  for ( ; it != values.end(); it++)
  {
    set[*it] = true;
  }
}

template<class T1, class T2> bool CParserNitDvb::GetSetValues(const map<T1, bool>& set,
                                                              T1* keys,
                                                              T2& keyCount)
{
  unsigned char requiredCount = set.size();
  if (requiredCount == 0)
  {
    keyCount = 0;
    return true;
  }
  if (keys == NULL)
  {
    keyCount = 0;
    return false;
  }
  if (keyCount > requiredCount)
  {
    keyCount = requiredCount;
  }
  map<T1, bool>::const_iterator it = set.begin();
  for (unsigned char i = 0; i < keyCount; i++, it++)
  {
    keys[i] = it->first;
  }
  return keyCount == requiredCount;
}

void CParserNitDvb::AddGroupNames(unsigned char nameTypeId,
                                  unsigned long long groupId,
                                  map<unsigned long, char*>& names)
{
  map<unsigned long, char*>* nameSet = new map<unsigned long, char*>();
  if (nameSet == NULL)
  {
    LogDebug(L"%s: failed to allocate group name set map");
  }

  map<unsigned long, char*>::iterator nameIt = names.begin();
  for ( ; nameIt != names.end(); nameIt++)
  {
    if (nameIt->second != NULL)
    {
      if (nameSet == NULL)
      {
        delete[] nameIt->second;
        nameIt->second = NULL;
      }
      else
      {
        (*nameSet)[nameIt->first] = nameIt->second;
      }
    }
  }
  names.clear();

  if (nameSet != NULL)
  {
    map<unsigned long long, map<unsigned long, char*>*> tempNames;
    tempNames[groupId] = nameSet;
    AddGroupNameSets(nameTypeId, tempNames);
  }
}

template<class T> void CParserNitDvb::AddGroupNameSets(unsigned char nameTypeId,
                                                        map<T, map<unsigned long, char*>*>& names)
{
  map<T, map<unsigned long, char*>*>::iterator nameSetIt = names.begin();
  map<unsigned long, char*>::iterator nameIt;
  for ( ; nameSetIt != names.end(); nameSetIt++)
  {
    unsigned long long groupId = nameSetIt->first;
    unsigned long long key = ((unsigned long long)nameTypeId << 56) | groupId;

    map<unsigned long, char*>* existingNameSet = m_groupNames[key];
    if (existingNameSet == NULL)
    {
      existingNameSet = nameSetIt->second;
      for (nameIt = existingNameSet->begin(); nameIt != existingNameSet->end(); nameIt++)
      {
        //LogDebug(L"%s: group name, type ID = %hhu, name ID = %llu, language = %S, name = %S",
        //          m_name, nameTypeId, groupId, (char*)&(nameIt->first),
        //          nameIt->second);
      }
      m_groupNames[key] = nameSetIt->second;
      nameSetIt->second = NULL;
      continue;
    }

    map<unsigned long, char*>* newNameSet = nameSetIt->second;
    for (nameIt = newNameSet->begin(); nameIt != newNameSet->end(); nameIt++)
    {
      unsigned long language = nameIt->first;
      char* existingName = (*existingNameSet)[language];
      if (existingName == NULL)
      {
        //LogDebug(L"%s: group name, type ID = %hhu, name ID = %llu, language = %S, name = %S",
        //          m_name, nameTypeId, groupId, (char*)&language, nameIt->second);
        (*existingNameSet)[language] = nameIt->second;
      }
      else if (strcmp(existingName, nameIt->second) == 0)
      {
        delete[] nameIt->second;
        nameIt->second = NULL;
      }
      else
      {
        LogDebug(L"%s: replacing existing group name, type ID = %hhu, name ID = %llu, language = %S, old name = %S, new name = %S",
                  m_name, nameTypeId, groupId, (char*)&language, existingName,
                  nameIt->second);
        delete[] existingName;
        (*existingNameSet)[language] = nameIt->second;
        nameIt->second = NULL;
      }
    }

    newNameSet->clear();
    delete newNameSet;
    nameSetIt->second = NULL;
  }
  names.clear();
}

unsigned char CParserNitDvb::GetNameCount(NameType nameType, unsigned long long nameId) const
{
  unsigned long long key = ((unsigned long long)nameType << 56) | nameId;
  CEnterCriticalSection lock(m_section);
  map<unsigned long long, map<unsigned long, char*>*>::const_iterator it = m_groupNames.find(key);
  if (it == m_groupNames.end())
  {
    LogDebug(L"%s: invalid name identifiers, name type = %lu, name ID = %llu",
              m_name, nameType, nameId);
    return 0;
  }
  map<unsigned long, char*>* names = it->second;
  if (names == NULL)
  {
    return 0;
  }
  return names->size();
}

bool CParserNitDvb::GetNameByIndex(NameType nameType,
                                    unsigned long long nameId,
                                    unsigned char index,
                                    unsigned long& language,
                                    char* name,
                                    unsigned short& nameBufferSize) const
{
  unsigned long long key = ((unsigned long long)nameType << 56) | nameId;
  CEnterCriticalSection lock(m_section);
  map<unsigned long long, map<unsigned long, char*>*>::const_iterator it = m_groupNames.find(key);
  if (it == m_groupNames.end() || it->second == NULL)
  {
    LogDebug(L"%s: invalid name identifiers, name type = %lu, name ID = %llu",
              m_name, nameType, nameId);
    return false;
  }
  map<unsigned long, char*>* names = it->second;
  if (index >= names->size())
  {
    LogDebug(L"%s: invalid name index, name type = %lu, name ID = %llu, index = %hhu, count = %llu",
              m_name, nameType, nameId, index,
              (unsigned long long)names->size());
    return false;
  }

  unsigned char i = 0;
  map<unsigned long, char*>::const_iterator nameIt = names->begin();
  for ( ; nameIt != names->end(); nameIt++)
  {
    if (i++ == index)
    {
      language = nameIt->first;
      unsigned short requiredBufferSize = 0;
      if (!CUtils::CopyStringToBuffer(nameIt->second, name, nameBufferSize, requiredBufferSize))
      {
        LogDebug(L"%s: insufficient name buffer size, name type = %lu, name ID = %llu, index = %hhu, language = %S, required size = %hu, actual size = %hu",
                  m_name, nameType, nameId, index, (char*)&language,
                  requiredBufferSize, nameBufferSize);
      }
      return true;
    }
  }
  return false;
}

bool CParserNitDvb::GetNameByLanguage(NameType nameType,
                                      unsigned long long nameId,
                                      unsigned long language,
                                      char* name,
                                      unsigned short& nameBufferSize) const
{
  unsigned long long key = ((unsigned long long)nameType << 56) | nameId;
  CEnterCriticalSection lock(m_section);
  map<unsigned long long, map<unsigned long, char*>*>::const_iterator it = m_groupNames.find(key);
  if (it == m_groupNames.end() || it->second == NULL)
  {
    LogDebug(L"%s: invalid name identifiers, name type = %lu, name ID = %llu",
              m_name, nameType, nameId);
    return false;
  }

  map<unsigned long, char*>* names = it->second;
  map<unsigned long, char*>::const_iterator nameIt = names->find(language);
  if (nameIt == names->end())
  {
    LogDebug(L"%s: invalid name language, name type = %lu, name ID = %llu, language = %S",
              m_name, nameType, nameId, (char*)&language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(nameIt->second, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"%s: insufficient name buffer size, name type = %lu, name ID = %llu, language = %S, required size = %hu, actual size = %hu",
              m_name, nameType, nameId, (char*)&language, requiredBufferSize,
              nameBufferSize);
  }
  return true;
}

void CParserNitDvb::AddServices(unsigned char tableId,
                                unsigned short groupId,
                                unsigned char sectionNumber,
                                unsigned short originalNetworkId,
                                unsigned short transportStreamId,
                                vector<unsigned short>& serviceIds,
                                map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                map<unsigned short, bool>& visibleInGuideFlags,
                                char* defaultAuthority,
                                map<unsigned short, unsigned short>& freesatChannelIds,
                                map<unsigned short, vector<unsigned short>*>& freesatRegionIds,
                                map<unsigned short, vector<unsigned short>*>& freesatChannelCategoryIds,
                                map<unsigned short, unsigned short>& openTvChannelIds,
                                map<unsigned short, vector<unsigned short>*>& openTvRegionIds,
                                map<unsigned short, vector<unsigned char>*>& norDigChannelListIds,
                                map<unsigned long, unsigned long>& cellFrequencies,
                                vector<unsigned long long>& targetRegionIds,
                                vector<unsigned long>& availableInCountries,
                                vector<unsigned long>& unavailableInCountries)
{
  // Pull the keys (cell IDs) from the cell frequencies map.
  vector<unsigned long> cellIds;
  map<unsigned long, unsigned long>::const_iterator cellIt = cellFrequencies.begin();
  for ( ; cellIt != cellFrequencies.end(); cellIt++)
  {
    cellIds.push_back(cellIt->first);
  }

  // Fallback for properties that apply to all services in a transport stream...
  serviceIds.push_back(0);

  // Ensure all known service IDs are referenced in logical channel numbers.
  vector<unsigned short>::const_iterator serviceIdIt = serviceIds.begin();
  for ( ; serviceIdIt != serviceIds.end(); serviceIdIt++)
  {
    map<unsigned long, unsigned short>* lcns = logicalChannelNumbers[*serviceIdIt];   // (Inserts a key with value NULL if not already present.)
  }

  // Get or create the table key.
  unsigned long tableKeyLookup = (tableId << 24) | (groupId << 8) | sectionNumber;
  unsigned short tableKey = m_tableKeys[tableKeyLookup];
  if (tableKey == 0)
  {
    tableKey = m_nextTableKey;
    m_tableKeys[tableKeyLookup] = m_nextTableKey;

    m_nextTableKey = (m_nextTableKey + 1) % 0xffff;
    if (m_nextTableKey == 0)
    {
      // This should never happen, because it would mean that the table
      // contains 65535 [table ID + extension ID] combinations. That would be
      // extreme!
      LogDebug(L"%s: code logic failure, table key wrapped around", m_name);
      m_nextTableKey++;
    }
  }

  map<unsigned short, map<unsigned long, unsigned short>*>::const_iterator serviceIt = logicalChannelNumbers.begin();
  for ( ; serviceIt != logicalChannelNumbers.end(); serviceIt++)
  {
    CRecordNitService* record = new CRecordNitService(m_name);
    if (record == NULL)
    {
      LogDebug(L"%s: failed to allocate service record, table ID = 0x%hhx, extension ID = %hu, ONID = %hu, TSID = %hu, service ID = %hu",
                m_name, tableId, groupId, originalNetworkId, transportStreamId,
                serviceIt->first);
      continue;
    }

    record->TableKey = tableKey;
    record->TableId = tableId;
    record->TableIdExtension = groupId;
    record->OriginalNetworkId = originalNetworkId;
    record->TransportStreamId = transportStreamId;
    record->ServiceId = serviceIt->first;
    record->FreesatChannelId = freesatChannelIds[serviceIt->first];
    record->OpenTvChannelId = openTvChannelIds[serviceIt->first];

    // Default visible flag should be true. Be careful!
    map<unsigned short, bool>::const_iterator flagIt = visibleInGuideFlags.find(serviceIt->first);
    if (flagIt == visibleInGuideFlags.end())
    {
      record->VisibleInGuide = true;
    }
    else
    {
      record->VisibleInGuide = flagIt->second;
    }

    if (serviceIt->second != NULL)
    {
      record->LogicalChannelNumbers = *(serviceIt->second);   // copy contents
    }

    if (defaultAuthority != NULL)
    {
      unsigned short byteCount = strlen(defaultAuthority) + 1;
      record->DefaultAuthority = new char[byteCount];
      if (record->DefaultAuthority == NULL)
      {
        LogDebug(L"%s: failed to allocate %hu byte(s) for a service's default authority, table ID = 0x%hhx, extension ID = %hu, ONID = %hu, TSID = %hu, service ID = %hu",
                  m_name, byteCount, tableId, groupId, originalNetworkId,
                  transportStreamId, serviceIt->first);
      }
      else
      {
        strncpy(record->DefaultAuthority, defaultAuthority, byteCount);
      }
    }

    record->AvailableInCells = cellIds;
    record->TargetRegionIds = targetRegionIds;

    vector<unsigned short>* groupIds = freesatRegionIds[serviceIt->first];
    if (groupIds != NULL)
    {
      record->FreesatRegionIds = *groupIds;   // copy contents
    }

    groupIds = openTvRegionIds[serviceIt->first];
    if (groupIds != NULL)
    {
      record->OpenTvRegionIds = *groupIds;    // copy contents
    }

    groupIds = freesatChannelCategoryIds[record->FreesatChannelId];
    if (groupIds != NULL)
    {
      record->FreesatChannelCategoryIds = *groupIds;  // copy contents
    }

    vector<unsigned char>* listIds = norDigChannelListIds[serviceIt->first];
    if (listIds != NULL)
    {
      record->NorDigChannelListIds = *listIds;  // copy contents
    }

    record->AvailableInCountries = availableInCountries;
    record->UnavailableInCountries = unavailableInCountries;

    if (serviceIt->first == 0)
    {
      // Avoid false-positive duplicate detection.
      IRecord* r = NULL;
      if (m_recordsService.GetRecordByKey(record->GetKey(), &r) && r != NULL)
      {
        delete record;
        continue;
      }
    }
    if (!m_recordsService.AddOrUpdateRecord((IRecord**)&record, m_callBack))
    {
      continue;
    }

    // Add new service records to the cache.
    unsigned long long dvbId = ((unsigned long long)originalNetworkId << 32) | ((unsigned long long)transportStreamId << 16) | serviceIt->first;
    vector<CRecordNitService*>* servicesByDvbId = NULL;
    map<unsigned long long, vector<CRecordNitService*>*>::iterator servicesByDvbIdIt = m_cacheServiceRecordsByDvbId.find(dvbId);
    if (servicesByDvbIdIt != m_cacheServiceRecordsByDvbId.end() && servicesByDvbIdIt->second != NULL)
    {
      servicesByDvbId = servicesByDvbIdIt->second;
    }
    else
    {
      servicesByDvbId = new vector<CRecordNitService*>();
      if (servicesByDvbId == NULL)
      {
        LogDebug(L"%s: failed to allocate a services-by-DVB-ID cache vector, table ID = 0x%hhx, extension ID = %hu, ONID = %hu, TSID = %hu, service ID = %hu",
                  m_name, tableId, groupId, originalNetworkId,
                  transportStreamId, serviceIt->first);
        continue;
      }
      m_cacheServiceRecordsByDvbId[dvbId] = servicesByDvbId;
    }
    servicesByDvbId->push_back(record);
  }
}

void CParserNitDvb::AddTransmitters(unsigned char tableId,
                                    unsigned short groupId,
                                    unsigned short originalNetworkId,
                                    unsigned short transportStreamId,
                                    CRecordNitTransmitterCable& recordCable,
                                    CRecordNitTransmitterSatellite& recordSatellite,
                                    CRecordNitTransmitterTerrestrial& recordTerrestrial,
                                    map<unsigned long, unsigned long>& cellFrequencies,
                                    vector<unsigned long>& frequencies)
{
  CRecordNitTransmitter* record = NULL;
  if (recordCable.SymbolRate > 0)
  {
    record = &recordCable;
  }
  else if (recordSatellite.SymbolRate > 0)
  {
    record = &recordSatellite;
  }
  else if (recordTerrestrial.Bandwidth > 0)
  {
    if (cellFrequencies.size() > 0)
    {
      // The cell frequency map is populated from cell frequency link and T2
      // delivery system descriptors. Both of those descriptors only apply for
      // terrestrial networks.
      recordTerrestrial.TableId = tableId;
      recordTerrestrial.NetworkId = groupId;
      recordTerrestrial.OriginalNetworkId = originalNetworkId;
      recordTerrestrial.TransportStreamId = transportStreamId;

      map<unsigned long, unsigned long>::const_iterator cellIt = cellFrequencies.begin();
      for ( ; cellIt != cellFrequencies.end(); cellIt++)
      {
        recordTerrestrial.CellId = (unsigned short)(cellIt->first >> 8);
        recordTerrestrial.CellIdExtension = (cellIt->first & 0xff);
        recordTerrestrial.Frequencies.clear();
        recordTerrestrial.Frequencies.push_back(cellIt->second);
        AddTransmitter(&recordTerrestrial);
      }
      return;
    }

    record = &recordTerrestrial;
  }

  if (record != NULL)
  {
    record->TableId = tableId;
    record->NetworkId = groupId;
    record->OriginalNetworkId = originalNetworkId;
    record->TransportStreamId = transportStreamId;
    if (frequencies.size() > 0)
    {
      // We have seen a frequency list descriptor. Technically that could be a
      // list of frequencies for cable, satellite or terrestrial transmitters.
      vector<unsigned long>::const_iterator frequencyIt = frequencies.begin();
      for ( ; frequencyIt != frequencies.end(); frequencyIt++)
      {
        record->Frequencies.push_back(*frequencyIt);
      }
    }
    AddTransmitter(record);
  }
}

void CParserNitDvb::AddTransmitter(CRecordNitTransmitter* record)
{
  CRecordNitTransmitter* clone = record->Clone();
  if (clone == NULL)
  {
    LogDebug(L"%s: failed to allocate transmitter record", m_name);
    return;
  }

  // Assign the table key.
  unsigned long tableKeyLookup = (clone->TableId << 24) | (clone->NetworkId << 8);
  clone->TableKey = m_tableKeys[tableKeyLookup];
  if (clone->TableKey == 0)
  {
    clone->TableKey = m_nextTableKey;
    m_tableKeys[tableKeyLookup] = m_nextTableKey;

    m_nextTableKey = (m_nextTableKey + 1) % 0xffff;
    if (m_nextTableKey == 0)
    {
      // This should never happen, because it would mean that the table
      // contains 65535 [table ID + extension ID] combinations. That would be
      // extreme!
      LogDebug(L"%s: code logic failure, table key wrapped around", m_name);
      m_nextTableKey++;
    }
  }

  m_recordsTransmitter.AddOrUpdateRecord((IRecord**)&clone, m_callBack);
}

void CParserNitDvb::AddLogicalChannelNumber(unsigned short serviceId,
                                            unsigned char descriptorTag,
                                            unsigned short regionId,
                                            unsigned short logicalChannelNumber,
                                            const wchar_t* lcnType,
                                            map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const
{
  unsigned long lcnKey = ((unsigned long)descriptorTag << 16) | regionId;
  map<unsigned long, unsigned short>* serviceLcns = NULL;
  map<unsigned short, map<unsigned long, unsigned short>*>::iterator it = logicalChannelNumbers.find(serviceId);
  if (it == logicalChannelNumbers.end())
  {
    serviceLcns = new map<unsigned long, unsigned short>();
    if (serviceLcns == NULL)
    {
      LogDebug(L"%s: failed to allocate map for service %hu's logical channel numbers",
                m_name, serviceId);
      return;
    }
    logicalChannelNumbers[serviceId] = serviceLcns;
    (*serviceLcns)[lcnKey] = logicalChannelNumber;
    return;
  }

  serviceLcns = it->second;
  unsigned short currentLcn = (*serviceLcns)[lcnKey];
  if (currentLcn == 0 || currentLcn == 0xfff || currentLcn == 0xffff)
  {
    (*serviceLcns)[lcnKey] = logicalChannelNumber;
  }
  else if (
    logicalChannelNumber != 0 &&
    logicalChannelNumber != 0xfff &&
    logicalChannelNumber != 0xffff &&
    currentLcn != logicalChannelNumber
  )
  {
    if (logicalChannelNumber < currentLcn)
    {
      (*serviceLcns)[lcnKey] = logicalChannelNumber;
      unsigned short temp = currentLcn;
      currentLcn = logicalChannelNumber;
      logicalChannelNumber = temp;
    }
    LogDebug(L"%s: %s logical channel number conflict, service ID = %hu, descriptor tag = 0x%hhx, region ID = %hu, LCN = %hu, alternative LCN = %hu",
              m_name, lcnType, serviceId, descriptorTag, regionId, currentLcn,
              logicalChannelNumber);
  }
}

bool CParserNitDvb::DecodeExtensionDescriptors(const unsigned char* sectionData,
                                                unsigned short& pointer,
                                                unsigned short endOfExtensionDescriptors,
                                                map<unsigned long, char*>& names,
                                                vector<unsigned long>& availableInCountries,
                                                vector<unsigned long>& unavailableInCountries,
                                                vector<unsigned long>& homeTransmitterKeys,
                                                unsigned long& privateDataSpecifier,
                                                char** defaultAuthority,
                                                vector<unsigned long long>& targetRegionIds,
                                                map<unsigned long long, map<unsigned long, char*>*>& targetRegionNames,
                                                map<unsigned char, char*>& cyfrowyPolsatChannelCategoryNames,
                                                map<unsigned short, map<unsigned long, char*>*>& freesatRegionNames,
                                                map<unsigned short, vector<unsigned short>*>& freesatChannelCategories,
                                                map<unsigned short, map<unsigned long, char*>*>& freesatChannelCategoryNames) const
{
  try
  {
    unsigned char tableId = sectionData[0];
    unsigned short extensionId = (sectionData[3] << 8) | sectionData[4];

    bool result = true;
    while (pointer + 1 < endOfExtensionDescriptors)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      //LogDebug(L"%s: extension descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, private data specifier = %lu",
      //          m_name, tag, length, pointer, privateDataSpecifier);
      if (pointer + length > endOfExtensionDescriptors)
      {
        LogDebug(L"%s: invalid section, extension descriptor length = %hhu, pointer = %hu, end of extension descriptors = %hu, private data specifier = %lu",
                  m_name, length, pointer, endOfExtensionDescriptors,
                  privateDataSpecifier);
        result = false;
        break;
      }

      if (tag == 0x40 || tag == 0x47) // network name descriptor, bouquet name descriptor
      {
        char* name = NULL;
        result = DecodeNameDescriptor(&sectionData[pointer], length, &name);
        if (result && name != NULL)
        {
          char* existingName = names[LANG_UND];
          if (existingName != NULL)
          {
            if (strcmp(existingName, name) != 0)
            {
              LogDebug(L"%s: name conflict, table ID = 0x%hhx, extension ID = %hu, name = %S, alternative name = %S",
                        m_name, tableId, extensionId, existingName, name);
            }
            delete[] name;
          }
          else
          {
            names[LANG_UND] = name;
          }
        }
      }
      else if (tag == 0x49) // country availability descriptor
      {
        result = DecodeCountryAvailabilityDescriptor(&sectionData[pointer],
                                                      length,
                                                      availableInCountries,
                                                      unavailableInCountries);
      }
      else if (tag == 0x4a) // linkage descriptor
      {
        result = DecodeLinkageDescriptor(&sectionData[pointer], length, homeTransmitterKeys);
      }
      else if (tag == 0x5b || tag == 0x5c)  // multilingual network name descriptor, multilingual bouquet name descriptor
      {
        result = DecodeMultilingualNameDescriptor(&sectionData[pointer], length, names);
      }
      else if (tag == 0x5f) // private data specifier descriptor
      {
        result = DecodePrivateDataSpecifierDescriptor(&sectionData[pointer],
                                                      length,
                                                      privateDataSpecifier);
      }
      else if (tag == 0x73) // default authority descriptor
      {
        char* da = NULL;
        result = DecodeDefaultAuthorityDescriptor(&sectionData[pointer], length, &da);
        if (result && da != NULL)
        {
          if (*defaultAuthority != NULL)
          {
            if (strcmp(*defaultAuthority, da) != 0)
            {
              LogDebug(L"%s: extension default authority conflict, default authority = %S, alternative = %S",
                        m_name, *defaultAuthority, da);
            }
            delete[] da;
          }
          else
          {
            *defaultAuthority = da;
          }
        }
      }
      else if (tag == 0x7f) // DVB extended descriptors
      {
        unsigned char tagExtension = 0;
        if (length > 0)
        {
          tagExtension = sectionData[pointer];
        }
        else if (privateDataSpecifier != PRIVATE_DATA_SPECIFIER_SKY_NETWORK_NZ)
        {
          LogDebug(L"%s: invalid section, extension extended descriptor length = %hhu, pointer = %hu, end of extension descriptors = %hu",
                    m_name, length, pointer, endOfExtensionDescriptors);
          result = false;
          break;
        }

        if (tagExtension == 0x09)       // target region descriptor
        {
          result = DecodeTargetRegionDescriptor(&sectionData[pointer], length, targetRegionIds);
        }
        else if (tagExtension == 0x0a)  // target region name descriptor
        {
          map<unsigned long long, char*> tempNames;
          unsigned long language;
          result = DecodeTargetRegionNameDescriptor(&sectionData[pointer],
                                                    length,
                                                    tempNames,
                                                    language);
          if (result)
          {
            map<unsigned long long, char*>::iterator nameIt = tempNames.begin();
            for ( ; nameIt != tempNames.end(); nameIt++)
            {
              map<unsigned long, char*>* existingNames = targetRegionNames[nameIt->first];
              if (existingNames == NULL)
              {
                existingNames = new map<unsigned long, char*>();
                if (existingNames == NULL)
                {
                  LogDebug(L"%s: failed to allocate map for extension descriptors target region names",
                            m_name);
                  targetRegionNames.erase(nameIt->first);
                  if (nameIt->second != NULL)
                  {
                    delete[] nameIt->second;
                    nameIt->second = NULL;
                  }
                  continue;
                }

                (*existingNames)[language] = nameIt->second;
                targetRegionNames[nameIt->first] = existingNames;
              }
              else
              {
                char* existingName = (*existingNames)[language];
                if (existingName != NULL)
                {
                  if (strcmp(existingName, nameIt->second) != 0)
                  {
                    LogDebug(L"%s: target region name conflict, table ID = 0x%hhx, extension ID = %hu, target region ID = %llu, language = %S, name = %S, alternative name = %S",
                              m_name, tableId, extensionId, nameIt->first,
                              (char*)&language, existingName, nameIt->second);
                  }
                  delete[] nameIt->second;
                  nameIt->second = NULL;
                }
                else
                {
                  (*existingNames)[language] = nameIt->second;
                }
              }
            }
          }
        }
      }
      else if (privateDataSpecifier == PRIVATE_DATA_SPECIFIER_BBC)
      {
        if (tag == 0xd4) // Freesat region name list descriptor
        {
          result = DecodeFreesatRegionNameListDescriptor(&sectionData[pointer],
                                                          length,
                                                          freesatRegionNames);
        }
        else if (tag == 0xd5) // Freesat channel category mapping descriptor
        {
          result = DecodeFreesatChannelCategoryMappingDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  freesatChannelCategories);
        }
        else if (tag == 0xd8) // Freesat channel category name list descriptor
        {
          result = DecodeFreesatChannelCategoryNameListDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  freesatChannelCategoryNames);
        }
      }
      else if (tag == 0xe3 && extensionId == ORIGINAL_NETWORK_ID_POLSAT_CYFRA_NC)
      {
        result = DecodeCyfrowyPolsatChannelCategoryNameListDescriptor(&sectionData[pointer],
                                                                      length,
                                                                      cyfrowyPolsatChannelCategoryNames);
      }

      if (!result)
      {
        LogDebug(L"%s: invalid extension descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of extension descriptors = %hu, private data specifier = %lu",
                  m_name, tag, length, pointer, endOfExtensionDescriptors,
                  privateDataSpecifier);
        break;
      }

      pointer += length;
    }

    if (!result)
    {
      CUtils::CleanUpStringSet(names);
      if (*defaultAuthority != NULL)
      {
        delete[] *defaultAuthority;
        *defaultAuthority = NULL;
      }
      CleanUpNames(targetRegionNames);
      CleanUpNames(freesatRegionNames);
      CleanUpMapOfVectors(freesatChannelCategories);
      CleanUpNames(freesatChannelCategoryNames);
    }

    pointer = endOfExtensionDescriptors;
    return result;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeExtensionDescriptors()", m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeTransportStreamDescriptors(const unsigned char* sectionData,
                                                      unsigned short& pointer,
                                                      unsigned short endOfTransportDescriptors,
                                                      unsigned long groupPrivateDataSpecifier,
                                                      const vector<unsigned short>& bouquetFreesatRegionIds,
                                                      vector<unsigned short>& serviceIds,
                                                      map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                                      map<unsigned short, bool>& visibleInGuideFlags,
                                                      map<unsigned short, vector<unsigned char>*>& norDigChannelListIds,
                                                      map<unsigned char, char*>& norDigChannelListNames,
                                                      map<unsigned short, unsigned short>& openTvChannelIds,
                                                      map<unsigned short, vector<unsigned short>*>& openTvRegionIds,
                                                      map<unsigned short, unsigned short>& freesatChannelIds,
                                                      map<unsigned short, vector<unsigned short>*>& freesatRegionIds,
                                                      vector<unsigned long long>& targetRegionIds,
                                                      char** defaultAuthority,
                                                      vector<unsigned long>& frequencies,
                                                      map<unsigned long, unsigned long>& cellFrequencies,
                                                      CRecordNitTransmitterCable& recordCable,
                                                      CRecordNitTransmitterSatellite& recordSatellite,
                                                      CRecordNitTransmitterTerrestrial& recordTerrestrial) const
{
  try
  {
    unsigned char tableId = sectionData[0];
    unsigned short groupId = (sectionData[3] << 8) | sectionData[4];
    unsigned long privateDataSpecifier = groupPrivateDataSpecifier;
    bool result = true;
    while (pointer + 1 < endOfTransportDescriptors)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      //LogDebug(L"%s: transport descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, private data specifier = %lu",
      //          m_name, tag, length, pointer, privateDataSpecifier);
      if (pointer + length > endOfTransportDescriptors)
      {
        LogDebug(L"%s: invalid section, transport descriptor length = %hhu, pointer = %hu, end of transport descriptors = %hu, private data specifier = %lu",
                  m_name, length, pointer, endOfTransportDescriptors,
                  privateDataSpecifier);
        result = false;
        break;
      }

      if (tag == 0x41)  // service list descriptor
      {
        result = DecodeServiceListDescriptor(&sectionData[pointer], length, serviceIds);
      }
      else if (tag == 0x43) // satellite delivery system descriptor
      {
        result = DecodeSatelliteDeliverySystemDescriptor(&sectionData[pointer],
                                                          length,
                                                          recordSatellite);
      }
      else if (tag == 0x44) // cable delivery system descriptor
      {
        result = DecodeCableDeliverySystemDescriptor(&sectionData[pointer], length, recordCable);
      }
      else if (tag == 0x5a) // terrestrial delivery system descriptor
      {
        result = DecodeTerrestrialDeliverySystemDescriptor(&sectionData[pointer],
                                                            length,
                                                            recordTerrestrial);
      }
      else if (tag == 0x5f) // private data specifier descriptor
      {
        result = DecodePrivateDataSpecifierDescriptor(&sectionData[pointer],
                                                      length,
                                                      privateDataSpecifier);
      }
      else if (tag == 0x62) // frequency list descriptor
      {
        result = DecodeFrequencyListDescriptor(&sectionData[pointer], length, frequencies);
      }
      else if (tag == 0x6d) // cell frequency link descriptor
      {
        result = DecodeCellFrequencyLinkDescriptor(&sectionData[pointer], length, cellFrequencies);
      }
      else if (tag == 0x73) // default authority descriptor
      {
        char* da = NULL;
        result = DecodeDefaultAuthorityDescriptor(&sectionData[pointer], length, &da);
        if (result && da != NULL)
        {
          if (*defaultAuthority != NULL)
          {
            if (strcmp(*defaultAuthority, da) != 0)
            {
              LogDebug(L"%s: transport stream default authority conflict, default authority = %S, alternative = %S",
                        m_name, *defaultAuthority, da);
            }
            delete[] da;
          }
          else
          {
            *defaultAuthority = da;
          }
        }
      }
      else if (tag == 0x79) // S2 satellite delivery system descriptor
      {
        result = DecodeS2SatelliteDeliverySystemDescriptor(&sectionData[pointer],
                                                            length,
                                                            recordSatellite);
      }
      else if (tag == 0x7f) // DVB extended descriptors
      {
        if (length < 1)
        {
          LogDebug(L"%s: invalid section, transport extended descriptor length = %hhu, pointer = %hu, end of transport descriptors = %hu",
                    m_name, length, pointer, endOfTransportDescriptors);
          result = false;
          break;
        }

        unsigned char tagExtension = sectionData[pointer];
        if (tagExtension == 0x04)  // T2 delivery system descriptor
        {
          result = DecodeT2TerrestrialDeliverySystemDescriptor(&sectionData[pointer],
                                                                length,
                                                                recordTerrestrial,
                                                                cellFrequencies);
        }
        else if (tagExtension == 0x09) // target region descriptor
        {
          result = DecodeTargetRegionDescriptor(&sectionData[pointer], length, targetRegionIds);
        }
        else if (tagExtension == 0x0d) // C2 delivery system descriptor
        {
          result = DecodeC2CableDeliverySystemDescriptor(&sectionData[pointer],
                                                          length,
                                                          recordCable);
        }
      }
      else if (
        (
          tag == 0x82 &&
          (
            privateDataSpecifier == PRIVATE_DATA_SPECIFIER_TELEDENMARK ||   // Sagem logical channel number descriptor
            privateDataSpecifier == PRIVATE_DATA_SPECIFIER_POLSAT ||
            privateDataSpecifier == PRIVATE_DATA_SPECIFIER_STARHUB_SG ||    // StarHub TV (formerly Singapore Cable Vision) - Singapore DVB-C
            (                                                               // Sogecable (Movistar+) - Astra 19.2E eg. 10729V - don't include a PDS
              tableId != TABLE_ID_NIT_DVB_ACTUAL &&
              tableId != TABLE_ID_NIT_DVB_OTHER &&
              groupId >= 0x20 &&
              groupId <= 0x22
            )
          )
        ) ||
        (tag == 0x93 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_IRDETO) ||       // Irdeto logical channel number descriptor (Austar Australia)
        (tag == 0xe2 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_NEWS_DATACOM_1)  // News Data Com [NDC] logical channel number descriptor (Sky NZ)
      )
      {
        result = DecodeAlternativeLogicalChannelNumberDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  tag,
                                                                  visibleInGuideFlags,
                                                                  logicalChannelNumbers);
      }
      else if (
        tag == 0x83 ||      // NorDig logical channel number descriptor - used too widely and loosely to apply PDS/PDI filtering
        (
          tag == 0x88 &&    // HD simulcast logical channel number descriptor
          (
            privateDataSpecifier == PRIVATE_DATA_SPECIFIER_EACEM ||         // IEC/CENELEC 62 216 standard
            privateDataSpecifier == PRIVATE_DATA_SPECIFIER_IDPNDNT_TV_COM   // Freeview UK
          )
        )
      )
      {
        result = DecodeLogicalChannelNumberDescriptor(&sectionData[pointer],
                                                      length,
                                                      tag,
                                                      privateDataSpecifier,
                                                      visibleInGuideFlags,
                                                      logicalChannelNumbers);
      }
      else if (tag == 0x86 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_IDPNDNT_TV_COM)  // service attribute descriptor (Freeview UK)
      {
        result = DecodeServiceAttributeDescriptor(&sectionData[pointer],
                                                  length,
                                                  visibleInGuideFlags);
      }
      else if (tag == 0x87 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_NORDIG)    // NorDig logical channel descriptor version 2
      {
        result = DecodeNorDigLogicalChannelDescriptorVersion2(&sectionData[pointer],
                                                              length,
                                                              norDigChannelListNames,
                                                              norDigChannelListIds,
                                                              logicalChannelNumbers,
                                                              visibleInGuideFlags);
      }
      else if (tag == 0xb1 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_BSKYB_1)   // OpenTV channel descriptor
      {
        result = DecodeOpenTvChannelDescriptor(&sectionData[pointer],
                                                length,
                                                openTvRegionIds,
                                                openTvChannelIds,
                                                logicalChannelNumbers);
      }
      else if (tag == 0xd3 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_BBC)       // Freesat channel descriptor
      {
        result = DecodeFreesatChannelDescriptor(&sectionData[pointer],
                                                length,
                                                bouquetFreesatRegionIds,
                                                visibleInGuideFlags,
                                                freesatChannelIds,
                                                logicalChannelNumbers,
                                                freesatRegionIds);
      }

      if (!result)
      {
        LogDebug(L"%s: invalid transport stream descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of transport descriptors = %hu, private data specifier = %lu",
                  m_name, tag, length, pointer, endOfTransportDescriptors,
                  privateDataSpecifier);
        break;
      }
      pointer += length;
    }

    if (!result)
    {
      CleanUpMapOfMaps(logicalChannelNumbers);
      CleanUpMapOfVectors(norDigChannelListIds);
      CUtils::CleanUpStringSet(norDigChannelListNames);
      CleanUpMapOfVectors(openTvRegionIds);
      CleanUpMapOfVectors(freesatRegionIds);
      if (*defaultAuthority != NULL)
      {
        delete[] *defaultAuthority;
        *defaultAuthority = NULL;
      }
    }
    return result;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeTransportStreamDescriptors()", m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeNameDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          char** name) const
{
  if (dataLength == 0)
  {
    // Allow zero-length name descriptors.
    /*LogDebug(L"%s: invalid name descriptor, length = %hhu",
              m_name, dataLength);
    return false;*/
    *name = NULL;
    return true;
  }
  try
  {
    if (!CTextUtil::DvbTextToString(data, dataLength, name))
    {
      LogDebug(L"%s: invalid name descriptor, length = %hhu",
                m_name, dataLength);
      return false;
    }
    if (*name == NULL)
    {
      LogDebug(L"%s: failed to allocate a name", m_name);
    }
    else
    {
      //LogDebug(L"%s: name descriptor, name = %S", m_name, *name);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeNameDescriptor()", m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeCountryAvailabilityDescriptor(const unsigned char* data,
                                                        unsigned char dataLength,
                                                        vector<unsigned long>& availableInCountries,
                                                        vector<unsigned long>& unavailableInCountries) const
{
  if (dataLength == 0 || (dataLength - 1) % 3 != 0)
  {
    LogDebug(L"%s: invalid country availability descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    bool countryAvailabilityFlag = (data[pointer++] & 0x80) != 0;
    //LogDebug(L"%s: country availability descriptor, availability flag = %d",
    //          m_name, countryAvailabilityFlag);
    while (pointer + 2 < dataLength)
    {
      unsigned long countryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      //LogDebug(L"  %S", (char*)&countryCode);
      if (countryAvailabilityFlag)
      {
        availableInCountries.push_back(countryCode);
      }
      else
      {
        unavailableInCountries.push_back(countryCode);
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeCountryAvailabilityDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeLinkageDescriptor(const unsigned char* data,
                                            unsigned char dataLength,
                                            vector<unsigned long>& homeTransmitterKeys) const
{
  if (dataLength < 7)
  {
    LogDebug(L"%s: invalid linkage descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short transportStreamId = (data[0] << 8) | data[1];
    unsigned short originalNetworkId = (data[2] << 8) | data[3];
    unsigned short serviceId = (data[4] << 8) | data[5];
    unsigned char linkageType = data[6];
    //LogDebug(L"%s: linkage descriptor, TSID = %hu, ONID = %hu, service ID = %hu, linkage type = %hhu, descriptor length = %hhu",
    //          m_name, transportStreamId, originalNetworkId, serviceId,
    //          linkageType, dataLength);
    if (linkageType == 4)   // TS containing complete Network/Bouquet SI
    {
      homeTransmitterKeys.push_back(GetLinkageKey(originalNetworkId, transportStreamId));
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeLinkageDescriptor()", m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeMultilingualNameDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      map<unsigned long, char*>& names) const
{
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid multilingual name descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 3 < dataLength)
    {
      unsigned long iso639LanguageCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      unsigned char nameLength = data[pointer++];
      if (nameLength == 0)
      {
        //LogDebug(L"%s: multilingual name descriptor, language = %S",
        //          m_name, (char*)&iso639LanguageCode);
        continue;
      }

      char* name = NULL;
      if (
        pointer + nameLength > dataLength ||
        !CTextUtil::DvbTextToString(&data[pointer], nameLength, &name)
      )
      {
        LogDebug(L"%s: invalid multilingual name descriptor, descriptor length = %hhu, pointer = %hu, name length = %hhu, language = %S",
                  m_name, dataLength, pointer, nameLength,
                  (char*)&iso639LanguageCode);
        CUtils::CleanUpStringSet(names);
        return false;
      }
      if (name == NULL)
      {
        LogDebug(L"%s: failed to allocate the %S multilingual name",
                  m_name, (char*)&iso639LanguageCode);
        pointer += nameLength;
        continue;
      }

      char* existingName = names[iso639LanguageCode];
      if (existingName != NULL)
      {
        if (strcmp(existingName, name) != 0)
        {
          LogDebug(L"%s: multilingual name conflict, language = %S, name = %S, alternative name = %S",
                    m_name, (char*)&iso639LanguageCode, existingName, name);
        }
        delete[] name;
      }
      else
      {
        //LogDebug(L"%s: multilingual name descriptor, language = %S, name = %S",
        //          m_name, (char*)&iso639LanguageCode, name);
        names[iso639LanguageCode] = name;
      }

      pointer += nameLength;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeMultilingualNameDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodePrivateDataSpecifierDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          unsigned long& privateDataSpecifier) const
{
  if (dataLength != 4)
  {
    LogDebug(L"%s: invalid private data specifier descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    privateDataSpecifier = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
    //LogDebug(L"%s: private data specifier descriptor, specifier = %lu",
    //          m_name, privateDataSpecifier);
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodePrivateDataSpecifierDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeDefaultAuthorityDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      char** defaultAuthority) const
{
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid default authority descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    if (!CTextUtil::DvbTextToString(data, dataLength, defaultAuthority))
    {
      LogDebug(L"%s: invalid default authority descriptor, descriptor length = %hhu",
                m_name, dataLength);
      return false;
    }
    if (*defaultAuthority == NULL)
    {
      LogDebug(L"%s: failed to allocate a default authority", m_name);
    }
    else if (
      strncmp(*defaultAuthority, "crid://", 7) != 0 &&
      strncmp(*defaultAuthority, "CRID://", 7) != 0
    )
    {
      // Prepend the "crid://" part if necessary.
      unsigned char byteCount = 7 + strlen(*defaultAuthority) + 1;
      char* temp = new char[7 + strlen(*defaultAuthority) + 1];
      if (temp == NULL)
      {
        LogDebug(L"%s: failed to allocate %hhu bytes for a fully qualified default authority",
                  m_name, byteCount);
      }
      else
      {
        strncpy(temp, "crid://", 8);
        strncpy(&temp[7], *defaultAuthority, byteCount - 7);
      }
      delete[] *defaultAuthority;
      *defaultAuthority = temp;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeDefaultAuthorityDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeFreesatRegionNameListDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          map<unsigned short, map<unsigned long, char*>*>& names) const
{
  // <loop>
  //   region ID - 2 bytes
  //   ISO 639 language code - 3 bytes
  //   region name length - 1 byte
  //   region name - [region name length] bytes
  // </loop>
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid Freesat region name list descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 5 < dataLength)
    {
      unsigned short regionId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned long iso639LanguageCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      unsigned char nameLength = data[pointer++];
      if (nameLength == 0)
      {
        //LogDebug(L"%s: Freesat region name list descriptor, region ID = %hu, language = %S",
        //          m_name, regionId, (char*)&iso639LanguageCode);
        continue;
      }

      char* name = NULL;
      if (
        pointer + nameLength > dataLength ||
        !CTextUtil::DvbTextToString(&data[pointer], nameLength, &name)
      )
      {
        LogDebug(L"%s: invalid Freesat region name list descriptor, descriptor length = %hhu, pointer = %hu, name length = %hhu, region ID = %hu, language = %S",
                  m_name, dataLength, pointer, nameLength, regionId,
                  (char*)&iso639LanguageCode);
        CleanUpNames(names);
        return false;
      }
      if (name == NULL)
      {
        LogDebug(L"%s: failed to allocate Freesat region %hu's %S name",
                  m_name, regionId, (char*)&iso639LanguageCode);
        pointer += nameLength;
        continue;
      }

      map<unsigned long, char*>* regionNames = names[regionId];
      if (regionNames == NULL)
      {
        regionNames = new map<unsigned long, char*>();
        if (regionNames == NULL)
        {
          LogDebug(L"%s: failed to allocate map for Freesat region %hu's name list",
                    m_name, regionId);
          names.erase(regionId);
          delete[] name;
          pointer += nameLength;
          continue;
        }
        names[regionId] = regionNames;
      }

      char* existingName = (*regionNames)[iso639LanguageCode];
      if (existingName != NULL)
      {
        if (strcmp(existingName, name) != 0)
        {
          LogDebug(L"%s: Freesat region name conflict, region ID = %hu, language = %S, name = %S, alternative name = %S",
                    m_name, regionId, (char*)&iso639LanguageCode,
                    existingName, name);
        }
        delete[] name;
      }
      else
      {
        //LogDebug(L"%s: Freesat region name list descriptor, region ID = %hu, language = %S, name = %S",
        //          m_name, regionId, (char*)&iso639LanguageCode, name);
        (*regionNames)[iso639LanguageCode] = name;
      }

      pointer += nameLength;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeFreesatRegionNameListDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeFreesatChannelCategoryMappingDescriptor(const unsigned char* data,
                                                                  unsigned char dataLength,
                                                                  map<unsigned short, vector<unsigned short>*>& channels) const
{
  // <loop>
  //   flags - 5 bits
  //   category ID - 11 bits
  //   channel loop length - 1 byte
  //   <loop>
  //     reserved - 1 bit, always 0
  //     channel ID - 15 bits
  //   </loop>
  //   ??? - 1 byte, only present for the Content Control category (flags 0x04), always 0xc0
  // </loop>
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid Freesat channel category mapping descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 2 < dataLength)
    {
      unsigned char flags = data[pointer] >> 3;
      unsigned short categoryId = ((data[pointer] & 0x7) << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char channelLoopLength = data[pointer++];
      //LogDebug(L"%s: Freesat channel category mapping descriptor, flags = 0x%hhx, category ID = %hu, channel loop length = %hhu",
      //          m_name, flags, categoryId, channelLoopLength);

      unsigned short endOfChannelLoop = pointer + channelLoopLength;
      unsigned short endOfCategory = endOfChannelLoop;
      if ((flags & 0x4) != 0)
      {
        endOfCategory++;
      }
      if (endOfCategory > dataLength || channelLoopLength % 2 != 0)
      {
        LogDebug(L"%s: invalid Freesat channel category mapping descriptor, descriptor length = %hhu, pointer = %hu, channel loop length = %hhu, flags = 0x%hhx, category ID = %hu",
                  dataLength, pointer, channelLoopLength, flags, categoryId);
        CleanUpMapOfVectors(channels);
        return false;
      }

      while (pointer + 1 < endOfChannelLoop)
      {
        unsigned short channelId = ((data[pointer] & 0x7f) << 8) | data[pointer + 1];
        pointer += 2;
        //LogDebug(L"  %hu", channelId);

        vector<unsigned short>* categorySet = channels[channelId];
        if (categorySet == NULL)
        {
          categorySet = new vector<unsigned short>();
          if (categorySet == NULL)
          {
            LogDebug(L"%s: failed to allocate vector for Freesat channel %hu's category list",
                      m_name, channelId);
            channels.erase(channelId);
            continue;
          }
          channels[channelId] = categorySet;
        }
        categorySet->push_back(categoryId);
      }

      if ((flags & 0x4) != 0)
      {
        unsigned char extraByte = data[pointer++];
        //LogDebug(L"  extra byte = %hhu", extraByte);
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeFreesatChannelCategoryMappingDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeFreesatChannelCategoryNameListDescriptor(const unsigned char* data,
                                                                    unsigned char dataLength,
                                                                    map<unsigned short, map<unsigned long, char*>*>& names) const
{
  // <loop>
  //   flags - 5 bits
  //   category ID - 11 bits (minimum 7 bits for uniqueness, maximum 11 bits to match category mapping)
  //   category name loop length - 1 byte
  //   <loop>
  //     ISO 639 language code - 3 bytes
  //     category name length - 1 byte
  //     category name - [category name length] bytes
  //   </loop>
  // </loop>
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid Freesat channel category name list descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 2 < dataLength)
    {
      unsigned short categoryId = ((data[pointer] & 0x7) << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char categoryNameLoopLength = data[pointer++];
      //LogDebug(L"%s: Freesat channel category name list descriptor, category ID = %hu, name loop length = %hhu",
      //          m_name, categoryId, categoryNameLoopLength);

      unsigned short endOfCategoryNameLoop = pointer + categoryNameLoopLength;
      if (endOfCategoryNameLoop > dataLength)
      {
        LogDebug(L"%s: invalid Freesat channel category name list descriptor, descriptor length = %hhu, pointer = %hu, category name loop length = %hhu, category ID = %hu",
                  m_name, dataLength, pointer, categoryNameLoopLength,
                  categoryId);
        CleanUpNames(names);
        return false;
      }

      while (pointer + 3 < endOfCategoryNameLoop)
      {
        unsigned long iso639LanguageCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
        pointer += 3;
        unsigned char nameLength = data[pointer++];
        if (nameLength == 0)
        {
          //LogDebug(L"  language = %S", (char*)&iso639LanguageCode);
          continue;
        }

        char* name = NULL;
        if (
          pointer + nameLength > endOfCategoryNameLoop ||
          !CTextUtil::DvbTextToString(&data[pointer], nameLength, &name)
        )
        {
          LogDebug(L"%s: invalid Freesat channel category name list descriptor, descriptor length = %hhu, pointer = %hu, name length = %hhu, end of category name loop = %hu, category ID = %hu, language = %S",
                    m_name, dataLength, pointer, nameLength,
                    endOfCategoryNameLoop, categoryId,
                    (char*)&iso639LanguageCode);
          CleanUpNames(names);
          return false;
        }
        if (name == NULL)
        {
          LogDebug(L"%s: failed to allocate Freesat channel category %hu's %S name",
                    m_name, categoryId, (char*)&iso639LanguageCode);
          pointer += nameLength;
          continue;
        }

        map<unsigned long, char*>* categoryNames = names[categoryId];
        if (categoryNames == NULL)
        {
          categoryNames = new map<unsigned long, char*>();
          if (categoryNames == NULL)
          {
            LogDebug(L"%s: failed to allocate map for Freesat channel category %hu's name list",
                      m_name, categoryId);
            names.erase(categoryId);
            delete[] name;
            pointer += nameLength;
            continue;
          }
          names[categoryId] = categoryNames;
        }

        char* existingName = (*categoryNames)[iso639LanguageCode];
        if (existingName != NULL)
        {
          if (strcmp(existingName, name) != 0)
          {
            LogDebug(L"%s: Freesat channel category name conflict, category ID = %hu, language = %S, name = %S, alternative name = %S",
                      m_name, categoryId, (char*)&iso639LanguageCode,
                      existingName, name);
          }
          delete[] name;
        }
        else
        {
          //LogDebug(L"  language = %S, name = %S",
          //          (char*)&iso639LanguageCode, name);
          (*categoryNames)[iso639LanguageCode] = name;
        }

        pointer += nameLength;
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeFreesatChannelCategoryNameListDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeCyfrowyPolsatChannelCategoryNameListDescriptor(const unsigned char* data,
                                                                          unsigned char dataLength,
                                                                          map<unsigned char, char*>& names) const
{
  // <loop>
  //   category ID - 1 byte
  //   category name loop length - 1 byte
  //   category name length - 1 byte
  //   category name - [category name length] bytes
  // </loop>
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid Cyfrowy Polsat channel category name list descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    // Encoding indicator bytes are not used (=> ISO/IEC 6937-1), however it's
    // obvious that the actual encoding is something else. Probably ISO/IEC
    // 8859-2 or Windows code page 1250. Assume ISO/IEC 8859-2 (because it's
    // directly DVB-compatible).
    unsigned char encodingConversionBuffer[255];
    encodingConversionBuffer[0] = 0x10;
    encodingConversionBuffer[1] = 0x00;
    encodingConversionBuffer[2] = 0x02;

    //LogDebug(L"%s: Cyfrowy Polsat channel category name list descriptor, descriptor length = %hhu",
    //          m_name, dataLength);
    unsigned short pointer = 0;
    while (pointer + 2 < dataLength)
    {
      unsigned char categoryId = data[pointer++];
      unsigned char nameLength = data[pointer++];

      char* name = NULL;
      bool isInvalid = false;
      if (pointer + nameLength > dataLength)
      {
        isInvalid = true;
      }
      else if (data[pointer] < 0x20)
      {
        // Trust encoding indicator bytes if they're present. Not expected.
        isInvalid = !CTextUtil::DvbTextToString(&data[pointer], nameLength, &name);
      }
      else
      {
        strncpy((char*)&encodingConversionBuffer[3], (char*)&data[pointer], nameLength);
        isInvalid = !CTextUtil::DvbTextToString(encodingConversionBuffer, nameLength + 3, &name);
      }
      if (isInvalid)
      {
        LogDebug(L"%s: invalid Cyfrowy Polsat channel category name list descriptor, descriptor length = %hhu, pointer = %hu, name length = %hhu, category ID = %hhu",
                  m_name, dataLength, pointer, nameLength, categoryId);
        CUtils::CleanUpStringSet(names);
        return false;
      }

      if (name == NULL)
      {
        LogDebug(L"%s: failed to allocate Cyfrowy Polsat channel category name, category ID = %hhu",
                  m_name, categoryId);
      }
      else
      {
        char* existingName = names[categoryId];
        if (existingName != NULL)
        {
          if (strcmp(existingName, name) != 0)
          {
            LogDebug(L"%s: Cyfrowy Polsat channel category name conflict, category ID = %hhu, name = %S, alternative name = %S",
                      m_name, categoryId, existingName, name);
          }
          delete[] name;
        }
        else
        {
          //LogDebug(L"  category ID = %hhu, name = %S", categoryId, name);
          names[categoryId] = name;
        }
      }

      pointer += nameLength;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeCyfrowyPolsatChannelCategoryNameListDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeTargetRegionDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  vector<unsigned long long>& targetRegionIds) const
{
  if (dataLength < 4)
  {
    LogDebug(L"%s: invalid target region descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned long countryCode = data[1] | (data[2] << 8) | (data[3] << 16);
    if (dataLength == 4)
    {
      //LogDebug(L"%s: target region descriptor, country code = %S",
      //          m_name, (char*)&countryCode);
      targetRegionIds.push_back((unsigned long long)countryCode << 32);
      return true;
    }

    unsigned short pointer = 4;
    while (pointer < dataLength)
    {
      bool countryCodeFlag = (data[pointer] & 0x04) != 0;
      unsigned char regionDepth = data[pointer++] & 0x03;

      // How many bytes are we expecting in this loop?
      unsigned char byteCount = 0;
      if (countryCodeFlag)
      {
        byteCount += 3;
      }
      byteCount += regionDepth;
      if (regionDepth == 3)
      {
        byteCount++;
      }

      if (pointer + byteCount > dataLength)
      {
        LogDebug(L"%s: invalid target region descriptor, length = %hhu, pointer = %hu, country code flag = %d, region depth = %hhu, country code = %S",
                  m_name, dataLength, pointer, countryCodeFlag, regionDepth,
                  (char*)&countryCode);
        return false;
      }

      if (countryCodeFlag)
      {
        countryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
        pointer += 3;
      }

      unsigned long long targetRegionId = (unsigned long long)countryCode << 32;
      if (regionDepth > 0)
      {
        targetRegionId |= (data[pointer++] << 24);
        if (regionDepth > 1)
        {
          targetRegionId |= (data[pointer++] << 16);
          if (regionDepth > 2)
          {
            targetRegionId |= (data[pointer] << 8) | data[pointer + 1];
            pointer += 2;
          }
        }
      }

      //LogDebug(L"%s: target region descriptor, country code = %S, ID = %llu",
      //          m_name, (char*)&countryCode, targetRegionId);
      targetRegionIds.push_back(targetRegionId);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeTargetRegionDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeTargetRegionNameDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      map<unsigned long long, char*>& names,
                                                      unsigned long& language) const
{
  if (dataLength < 7)
  {
    LogDebug(L"%s: invalid target region name descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned long countryCode = data[1] | (data[2] << 8) | (data[3] << 16);
    language = data[4] + (data[5] << 8) + (data[6] << 16);
    //LogDebug(L"%s: target region name descriptor, country code = %S, language = %S",
    //          m_name, (char*)&countryCode, (char*)&language);

    unsigned short pointer = 7;
    while (pointer + 1 < dataLength)
    {
      unsigned char regionDepth = data[pointer] >> 6;
      unsigned char regionNameLength = data[pointer++] & 0x3f;

      // How many bytes are we expecting in this loop?
      unsigned short byteCount = regionNameLength + regionDepth;
      if (regionDepth == 3)
      {
        byteCount++;
      }

      char* name = NULL;
      if (
        pointer + byteCount > dataLength ||
        (
          regionNameLength > 0 &&
          !CTextUtil::DvbTextToString(&data[pointer], regionNameLength, &name)
        )
      )
      {
        LogDebug(L"%s: invalid target region name descriptor, descriptor length = %hhu, pointer = %hu, region depth = %hhu, region name length = %hhu, country code = %S",
                  m_name, dataLength, pointer, regionDepth, regionNameLength,
                  (char*)&countryCode);
        CUtils::CleanUpStringSet(names);
        return false;
      }
      if (regionNameLength > 0 && name == NULL)
      {
        LogDebug(L"%s: failed to allocate a target region name", m_name);
        pointer += byteCount;
        continue;
      }
      pointer += regionNameLength;

      unsigned long long targetRegionId = ((unsigned long long)countryCode << 32) | (data[pointer++] << 24);
      if (regionDepth > 1)
      {
        targetRegionId |= (data[pointer++] << 16);
        if (regionDepth > 2)
        {
          targetRegionId |= (data[pointer] << 8) | data[pointer + 1];
          pointer += 2;
        }
      }

      //LogDebug(L"  region ID = %llu, name = %S", targetRegionId, name);
      names[targetRegionId] = name;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeTargetRegionNameDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeServiceListDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned short>& serviceIds) const
{
  // Note zero-length descriptor is not an error. The Sky BAT on Astra 28.2 E
  // contains zero length descriptors.
  if (dataLength % 3 != 0)
  {
    LogDebug(L"%s: invalid service list descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    //LogDebug(L"%s: service list descriptor", m_name);
    unsigned short pointer = 0;
    while (pointer + 2 < dataLength)
    {
      unsigned short serviceId = (data[pointer] << 8) + data[pointer + 1];
      pointer += 2;
      unsigned char serviceType = data[pointer++];
      serviceIds.push_back(serviceId);
      //LogDebug(L"  ID = %hu, type = %hhu", serviceId, serviceType);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeServiceListDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeSatelliteDeliverySystemDescriptor(const unsigned char* data,
                                                            unsigned char dataLength,
                                                            CRecordNitTransmitterSatellite& record) const
{
  if (dataLength != 11)
  {
    LogDebug(L"%s: invalid satellite delivery system descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned long frequency = DecodeSatelliteFrequency(data);
    record.Frequencies.push_back(frequency);

    // Position in degrees is encoded with BCD digits. The DP is after the 3rd
    // digit.
    record.OrbitalPosition += (1000 * ((data[4] >> 4) & 0xf));
    record.OrbitalPosition += (100 * (data[4] & 0xf));
    record.OrbitalPosition += (10 * ((data[5] >> 4) & 0xf));
    record.OrbitalPosition += (data[5] & 0xf);

    record.WestEastFlag = (data[6] & 0x80) != 0;
    record.Polarisation = (data[6] & 0x60) >> 5;
    record.RollOff = (data[6] & 0x18) >> 3;
    record.IsS2 = (data[6] & 0x4) != 0;
    record.Modulation = data[6] & 0x3;

    // Symbol rate in Ms/s is encoded with BCD digits. The DP is after the 3rd
    // digit. We want the symbol rate in ks/s.
    record.SymbolRate = (100000 * ((data[7] >> 4) & 0xf));
    record.SymbolRate += (10000 * (data[7] & 0xf));
    record.SymbolRate += (1000 * ((data[8] >> 4) & 0xf));
    record.SymbolRate += (100 * (data[8] & 0xf));
    record.SymbolRate += (10 * ((data[9] >> 4) & 0xf));
    record.SymbolRate += (data[9] & 0xf);

    record.InnerFecRate = data[10] & 0xf;

    //LogDebug(L"%s: satellite delivery system descriptor, frequency = %lu kHz, orbital position = %hu, West/East flag = %d, polarisation = %hhu, roll off = %hhu, is S2 = %d, modulation = %hhu, symbol rate = %lu ks/s, inner FEC rate = %hhu",
    //          m_name, frequency, record.OrbitalPosition, record.WestEastFlag,
    //          record.Polarisation, record.RollOff, record.IsS2,
    //          record.Modulation, record.SymbolRate, record.InnerFecRate);
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeSatelliteDeliverySystemDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeCableDeliverySystemDescriptor(const unsigned char* data,
                                                        unsigned char dataLength,
                                                        CRecordNitTransmitterCable& record) const
{
  if (dataLength != 11)
  {
    LogDebug(L"%s: invalid cable delivery system descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned long frequency = DecodeCableFrequency(data);
    record.Frequencies.push_back(frequency);

    record.OuterFecMethod = data[5] & 0xf;
    record.Modulation = data[6];

    // Symbol rate in Ms/s is encoded with BCD digits. The DP is after the 3rd
    // digit. We want the symbol rate in ks/s.
    record.SymbolRate = (100000 * ((data[7] >> 4) & 0xf));
    record.SymbolRate += (10000 * (data[7] & 0xf));
    record.SymbolRate += (1000 * ((data[8] >> 4) & 0xf));
    record.SymbolRate += (100 * (data[8] & 0xf));
    record.SymbolRate += (10 * ((data[9] >> 4) & 0xf));
    record.SymbolRate += (data[9] & 0xf);

    record.InnerFecRate = data[10] & 0xf;

    //LogDebug(L"%s: cable delivery system descriptor, frequency = %lu kHz, outer FEC method = %hhu, modulation = %hhu, symbol rate = %lu ks/s, inner FEC rate = %hhu",
    //          m_name, frequency, record.OuterFecMethod, record.Modulation,
    //          record.SymbolRate, record.InnerFecRate);
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeCableDeliverySystemDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeTerrestrialDeliverySystemDescriptor(const unsigned char* data,
                                                              unsigned char dataLength,
                                                              CRecordNitTransmitterTerrestrial& record) const
{
  if (dataLength != 11)
  {
    LogDebug(L"%s: invalid terrestrial delivery system descriptor length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned long frequency = DecodeTerrestrialFrequency(data);
    record.Frequencies.push_back(frequency);

    record.Bandwidth = data[4] >> 5;
    switch (record.Bandwidth)
    {
      case 1:
        record.Bandwidth = 7000;
        break;
      case 2:
        record.Bandwidth = 6000;
        break;
      case 3:
        record.Bandwidth = 5000;
        break;
      default:
        record.Bandwidth = 8000;
        break;
    }

    record.IsHighPriority = (data[4] & 0x10) != 0;
    record.TimeSlicingIndicator = (data[4] & 0x08) != 0;
    record.MpeFecIndicator = (data[4] & 0x04) != 0;
    record.Constellation = data[5] >> 6;
    record.IndepthInterleaverUsed = (data[5] & 0x20) != 0;
    record.HierarchyAlpha = (data[5] >> 3) & 3;
    record.CodeRateHpStream = data[5] & 7;
    record.CodeRateLpStream = data[6] >> 5;
    record.GuardInterval = (data[6] >> 3) & 3;
    record.TransmissionMode = (data[6] >> 1) & 3;
    record.OtherFrequencyFlag = (data[6] & 1) != 0;

    //LogDebug(L"%s: terrestrial delivery system descriptor, frequency = %lu kHz, bandwidth = %hu kHz, is high priority = %d, time slicing indicator = %d, MPE FEC indicator = %d, constellation = %hhu, indepth interleaver used = %d, hierarchy alpha = %hhu, code rate HP stream = %hhu, code rate LP stream = %hhu, guard interval = %hhu, transmission mode = %hhu, other frequency flag = %d",
    //          m_name, frequency, record.Bandwidth, record.IsHighPriority,
    //          record.TimeSlicingIndicator, record.MpeFecIndicator,
    //          record.Constellation, record.IndepthInterleaverUsed,
    //          record.HierarchyAlpha, record.CodeRateHpStream,
    //          record.CodeRateLpStream, record.GuardInterval,
    //          record.TransmissionMode, record.OtherFrequencyFlag);
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeTerrestrialDeliverySystemDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeFrequencyListDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  vector<unsigned long>& frequencies) const
{
  if (dataLength == 0 || (dataLength - 1) % 4 != 0)
  {
    LogDebug(L"%s: invalid frequency list descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned char codingType = data[0] & 0x3;
    if (codingType != 1 && codingType != 2 && codingType != 3)
    {
      LogDebug(L"%s: unsupported frequency list descriptor coding type, type = %hhu",
                m_name, codingType);
      return false;
    }
    //LogDebug(L"%s: frequency list descriptor, coding type = %hhu",
    //          m_name, codingType);

    unsigned short pointer = 1;
    while (pointer + 3 < dataLength)
    {
      unsigned long frequency = 0;
      if (codingType == 1)
      {
        frequency = DecodeSatelliteFrequency(&data[pointer]);
      }
      else if (codingType == 2)
      {
        frequency = DecodeCableFrequency(&data[pointer]);
      }
      else
      {
        frequency = DecodeTerrestrialFrequency(&data[pointer]);
      }
      //LogDebug(L"  %lu kHz", frequency);
      frequencies.push_back(frequency);
      pointer += 4;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeFrequencyListDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeCellFrequencyLinkDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      map<unsigned long, unsigned long>& frequencies) const
{
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid cell frequency link descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 6 < dataLength)
    {
      unsigned long cellId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned long frequency = DecodeTerrestrialFrequency(&data[pointer]);
      pointer += 4;
      unsigned char subcellInfoLoopLength = data[pointer++];
      //LogDebug(L"%s: cell frequency link descriptor, cell ID = %lu, frequency = %lu kHz, sub-cell info loop length = %hhu",
      //          m_name, cellId, frequency, subcellInfoLoopLength);

      frequencies[cellId << 8] = frequency;
      if (subcellInfoLoopLength > 0)
      {
        short endOfSubCellInfoLoop = pointer + subcellInfoLoopLength;
        if (endOfSubCellInfoLoop > dataLength || subcellInfoLoopLength % 5 != 0)
        {
          LogDebug(L"%s: invalid cell frequency link descriptor, descriptor length = %hhu, pointer = %hu, sub-cell info loop length = %hhu, cell ID = %lu, frequency = %lu kHz",
                    m_name, dataLength, pointer, subcellInfoLoopLength, cellId,
                    frequency);
          return false;
        }
        while (pointer + 4 < endOfSubCellInfoLoop)
        {
          unsigned char cellIdExtension = data[pointer++];
          frequency = DecodeTerrestrialFrequency(&data[pointer]);
          pointer += 4;
          //LogDebug(L"  cell ID extension = %hhu, frequency = %lu kHz",
          //          cellIdExtension, frequency);

          frequencies[(cellId << 8) | cellIdExtension] = frequency;
        }
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeCellFrequencyLinkDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeS2SatelliteDeliverySystemDescriptor(const unsigned char* data,
                                                              unsigned char dataLength,
                                                              CRecordNitTransmitterSatellite& record) const
{
  // Satlink (Hotbird 13E 10853H) transport streams contain zero-length descriptors.
  if (dataLength == 0)
  {
    record.IsS2 = true;
    record.MultipleInputStreamFlag = false;
    record.BackwardsCompatibilityIndicator = false;
    record.ScramblingSequenceIndex = 0;
    record.InputStreamIdentifier = 0;
    return true;
  }

  if (dataLength > 5)
  {
    LogDebug(L"%s: invalid S2 satellite delivery system descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    bool scramblingSequenceSelector = (data[0] & 0x80) != 0;
    if (!scramblingSequenceSelector)
    {
      record.ScramblingSequenceIndex = 0;
    }
    else
    {
      if (dataLength < 4)
      {
        LogDebug(L"%s: invalid S2 satellite delivery system descriptor, length = %hhu, scrambling sequence selector = %d",
                  m_name, dataLength, scramblingSequenceSelector);
        return false;
      }
      record.ScramblingSequenceIndex = ((data[1] & 0x3) << 16) | (data[2] << 8) | data[3];
    }

    record.MultipleInputStreamFlag = (data[0] & 0x40) != 0;
    record.BackwardsCompatibilityIndicator = (data[0] & 0x20) != 0;

    if (!record.MultipleInputStreamFlag)
    {
      record.InputStreamIdentifier = 0;
    }
    else if ((scramblingSequenceSelector && dataLength != 5) || dataLength < 2)
    {
      LogDebug(L"%s: invalid S2 satellite delivery system descriptor, length = %hhu, scrambling sequence selector = %d, multiple input stream flag = %d",
                m_name, dataLength, scramblingSequenceSelector,
                record.MultipleInputStreamFlag);
      return false;
    }
    else if (scramblingSequenceSelector)
    {
      record.InputStreamIdentifier = data[4];
    }
    else
    {
      record.InputStreamIdentifier = data[1];
    }

    //LogDebug(L"%s: S2 satellite delivery system descriptor, multiple input stream flag = %d, backwards compatibility indicator = %d, scrambling sequence index = %lu, input stream identifier = %hhu",
    //          m_name, record.MultipleInputStreamFlag,
    //          record.BackwardsCompatibilityIndicator,
    //          record.ScramblingSequenceIndex, record.InputStreamIdentifier);
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeS2SatelliteDeliverySystemDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeAlternativeLogicalChannelNumberDescriptor(const unsigned char* data,
                                                                    unsigned char dataLength,
                                                                    unsigned char tag,
                                                                    map<unsigned short, bool>& visibleInGuideFlags,
                                                                    map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const
{
  // Found in Yousee Denmark specifications as the "channel descriptor" (Sagem
  // proprietary).
  // http://yousee.dk/~/media/pdf/cpe/rules_operation.ashx
  // <loop>
  //   service ID - 2 bytes
  //   logical channel number - 2 bytes
  // </loop>
  //
  // Channels that have an LCN are visible in the guide; those that don't are
  // not meant to be visible.
  // This format is also used by other providers such as Sky New Zealand and
  // Austar Australia.
  //
  // Note: Cyfrowy Polsat transport streams contain zero-length descriptors.
  if (/*dataLength == 0 ||*/ dataLength % 4 != 0)
  {
    LogDebug(L"%s: invalid alternative logical channel number descriptor, tag = 0x%hhx, length = %hhu",
              m_name, tag, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 3 < dataLength)
    {
      unsigned short serviceId = (data[pointer] << 8) | data[pointer + 1];
      unsigned short logicalChannelNumber = (data[pointer + 2] << 8) | data[pointer + 3];
      pointer += 4;
      //LogDebug(L"%s: alternative logical channel number descriptor, service ID = %hu, LCN = %hu",
      //          m_name, serviceId, logicalChannelNumber);

      visibleInGuideFlags[serviceId] = true;
      AddLogicalChannelNumber(serviceId,
                              tag,
                              0,
                              logicalChannelNumber,
                              L"alternative",
                              logicalChannelNumbers);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeAlternativeLogicalChannelNumberDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeLogicalChannelNumberDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          unsigned char tag,
                                                          unsigned long privateDataSpecifier,
                                                          map<unsigned short, bool>& visibleInGuideFlags,
                                                          map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const
{
  // De-facto standard logical channel number descriptor:
  // <loop>
  //   service ID - 2 bytes
  //   visible service flag - 1 bit
  //   reserved - 5 bits
  //   logical channel number - 10 bits
  // </loop>
  //
  // Some variations do not define the visible service flag. Others define the
  // logical channel number with more bits.
  //
  // Private data specifiers (http://www.dvbservices.com/identifiers/private_data_spec_id?page=1):
  // - Italy, Portugal DTT (EACEM - IEC/CENELEC 62 216 standard) = 0x28
  // - NorDig = 0x29
  // - Freeview NZ = 0x37
  // - Canal+ Cyfrowy = 0xa5
  // - ITI Neovision (Hotbird 13E 10834V) = 0xb0
  // - Freeview UK = 0x233a
  // - Freeview AU = 0x3200 - 0x320f
  //
  // Note: ITI Neovision transport streams contain zero-length descriptors.
  if (/*dataLength == 0 ||*/ dataLength % 4 != 0)
  {
    LogDebug(L"%s: invalid logical channel number descriptor, private data specifier = %lu, tag = 0x%hhx, length = %hhu",
              m_name, privateDataSpecifier, tag, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 3 < dataLength)
    {
      unsigned short serviceId = (data[pointer] << 8) | data[pointer + 1];
      bool visibleServiceFlag = (data[pointer + 2] & 0x80) != 0;
      unsigned char reserved = (data[pointer + 2] & 0x7c) >> 2;
      unsigned short logicalChannelNumber = ((data[pointer + 2] & 0x3) << 8) | data[pointer + 3];
      pointer += 4;

      // The NorDig LCN descriptor specifies 14 bits for the LCN.
      if (tag == 0x83 && privateDataSpecifier == PRIVATE_DATA_SPECIFIER_NORDIG)
      {
        logicalChannelNumber |= ((reserved & 0xf) << 10);
      }

      //LogDebug(L"%s: logical channel number descriptor, tag = 0x%hhx, PDS = %lu, service ID = %hu, visible service flag = %d, reserved = %hhu, LCN = %hu",
      //          m_name, tag, privateDataSpecifier, serviceId,
      //          visibleServiceFlag, reserved, logicalChannelNumber);

      // In the UK the standard LCN descriptor doesn't have the visible service
      // flag. The HD simulcast LCN descriptor does.
      if (tag != 0x83 || privateDataSpecifier != PRIVATE_DATA_SPECIFIER_IDPNDNT_TV_COM)
      {
        // Assuming that the receiver is HD-capable, the visible in guide flag
        // in the HD simulcast LCN descriptor overrides other visibility flags.
        if (tag == 0x88 || visibleInGuideFlags.find(serviceId) == visibleInGuideFlags.end())
        {
          visibleInGuideFlags[serviceId] = visibleServiceFlag;
        }
        else
        {
          visibleInGuideFlags[serviceId] |= visibleServiceFlag;
        }
      }

      AddLogicalChannelNumber(serviceId,
                              tag,
                              0,
                              logicalChannelNumber,
                              L"de-facto",
                              logicalChannelNumbers);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeLogicalChannelNumberDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeServiceAttributeDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      map<unsigned short, bool>& visibleInGuideFlags) const
{
  if (dataLength == 0 || dataLength % 3 != 0)
  {
    LogDebug(L"%s: invalid service attribute descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 2 < dataLength)
    {
      unsigned short serviceId = (data[pointer] << 8) | data[pointer + 1];
      bool numericSelectionFlag = (data[pointer + 2] & 0x2) != 0;
      bool visibleServiceFlag = (data[pointer + 2] & 0x1) != 0;
      pointer += 3;

      //LogDebug(L"%s: service attribute descriptor, service ID = %hu, numeric selection flag = %d, visible service flag = %d",
      //          m_name, serviceId, numericSelectionFlag, visibleServiceFlag);

      // Assume we are HD-capable. Therefore the visible in guide flag in this
      // descriptor is overriden by the one in the HD simulcast LCN descriptor.
      if (visibleInGuideFlags.find(serviceId) == visibleInGuideFlags.end())
      {
        visibleInGuideFlags[serviceId] = visibleServiceFlag;
      }
      else
      {
        visibleInGuideFlags[serviceId] |= visibleServiceFlag;
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeServiceAttributeDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeNorDigLogicalChannelDescriptorVersion2(const unsigned char* data,
                                                                  unsigned char dataLength,
                                                                  map<unsigned char, char*>& channelListNames,
                                                                  map<unsigned short, vector<unsigned char>*>& channelListIds,
                                                                  map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                                                  map<unsigned short, bool>& visibleInGuideFlags) const
{
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid NorDig logical channel descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 5 < dataLength)
    {
      unsigned char channelListId = data[pointer++];
      unsigned char channelListNameLength = data[pointer++];
      char* name = NULL;
      if (channelListNameLength > 0)
      {
        if (
          pointer + channelListNameLength + 4 > dataLength ||
          !CTextUtil::DvbTextToString(&data[pointer], channelListNameLength, &name)
        )
        {
          LogDebug(L"%s: invalid NorDig logical channel descriptor, descriptor length = %hhu, pointer = %hu, channel list name length = %hhu, channel list ID = %hhu",
                    m_name, dataLength, pointer, channelListNameLength,
                    channelListId);
          CUtils::CleanUpStringSet(channelListNames);
          CleanUpMapOfVectors(channelListIds);
          return false;
        }

        if (name == NULL)
        {
          LogDebug(L"%s: failed to allocate NorDig channel list %hhu's name",
                    m_name, channelListId);
        }
        else
        {
          char* existingName = channelListNames[channelListId];
          if (existingName != NULL)
          {
            if (strcmp(existingName, name) != 0)
            {
              LogDebug(L"%s: NorDig channel list name conflict, list ID = %hhu, name = %S, alternative name = %S",
                        m_name, channelListId, existingName, name);
            }
            delete[] name;
          }
          else
          {
            channelListNames[channelListId] = name;
          }
        }
        pointer += channelListNameLength;
      }

      unsigned long countryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      unsigned char serviceListLength = data[pointer++];
      //LogDebug(L"%s: NorDig logical channel descriptor, channel list ID = %hhu, name = %S, country code = %S, service list length = %hhu",
      //          m_name, channelListId, name == NULL ? "" : name,
      //          (char*)&countryCode, serviceListLength);

      unsigned short endOfServiceList = pointer + serviceListLength;
      if (endOfServiceList > dataLength || serviceListLength % 4 != 0)
      {
        LogDebug(L"%s: invalid NorDig logical channel descriptor, descriptor length = %hhu, pointer = %hu, service list length = %hhu, channel list ID = %hhu, country code = %S",
                  m_name, dataLength, pointer, serviceListLength,
                  channelListId, (char*)&countryCode);
        CUtils::CleanUpStringSet(channelListNames);
        CleanUpMapOfVectors(channelListIds);
        return false;
      }

      while (pointer + 3 < endOfServiceList)
      {
        unsigned short serviceId = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        bool visibleServiceFlag = (data[pointer] & 0x80) != 0;
        unsigned short logicalChannelNumber = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
        pointer += 2;
        //LogDebug(L"  service ID = %hu, visible service flag = %d, LCN = %hu",
        //          serviceId, visibleServiceFlag, logicalChannelNumber);

        if (visibleInGuideFlags.find(serviceId) == visibleInGuideFlags.end())
        {
          visibleInGuideFlags[serviceId] = visibleServiceFlag;
        }
        else
        {
          visibleInGuideFlags[serviceId] |= visibleServiceFlag;
        }

        AddLogicalChannelNumber(serviceId,
                                0x87,
                                0,
                                logicalChannelNumber,
                                L"NorDig",
                                logicalChannelNumbers);

        vector<unsigned char>* listIds = channelListIds[serviceId];
        if (listIds == NULL)
        {
          listIds = new vector<unsigned char>();
          if (listIds == NULL)
          {
            LogDebug(L"%s: failed to allocate vector for service %hu's NorDig channel list IDs",
                      m_name, serviceId);
            channelListIds.erase(serviceId);
            continue;
          }
          channelListIds[serviceId] = listIds;
        }

        listIds->push_back(channelListId);
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeNorDigLogicalChannelDescriptorVersion2()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeOpenTvChannelDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  map<unsigned short, vector<unsigned short>*>& regionIds,
                                                  map<unsigned short, unsigned short>& channelIds,
                                                  map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const
{
  // region ID - 2 bytes
  // <loop>
  //   service ID - 2 bytes
  //   service type - 1 byte; DVB-compatible
  //   channel ID - 2 bytes
  //   logical channel number - 2 bytes
  //   logical channel number 2 - 12 bits
  //   flags - 4 bits
  // </loop>
  if (dataLength < 2 || (dataLength - 2) % 9 != 0)
  {
    LogDebug(L"%s: invalid OpenTV channel descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short regionId = (data[0] << 8) | data[1];
    //LogDebug(L"%s: OpenTV channel descriptor, region ID = %hu",
    //          m_name, regionId);

    unsigned short pointer = 2;
    while (pointer + 8 < dataLength)
    {
      unsigned short serviceId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char serviceType = data[pointer++];
      unsigned short channelId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short logicalChannelNumber = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short logicalChannelNumber2 = (data[pointer] << 4) | (data[pointer + 1] >> 4);
      pointer++;
      unsigned char flags = data[pointer++] & 0xf;
      //LogDebug(L"  service ID = %hu, service type = %hhu, channel ID = %hu, LCN = %hu, LCN 2 = %hu, flags = 0x%hhx",
      //          serviceId, serviceType, channelId, logicalChannelNumber,
      //          logicalChannelNumber2, flags);

      channelIds[serviceId] = channelId;

      AddLogicalChannelNumber(serviceId,
                              0xb1,
                              regionId,
                              logicalChannelNumber,
                              L"OpenTV",
                              logicalChannelNumbers);

      vector<unsigned short>* channelRegionIds = regionIds[serviceId];
      if (channelRegionIds == NULL)
      {
        channelRegionIds = new vector<unsigned short>();
        if (channelRegionIds == NULL)
        {
          LogDebug(L"%s: failed to allocate vector for service %hu's OpenTV region IDs",
                    m_name, serviceId);
          regionIds.erase(serviceId);
          continue;
        }
        regionIds[serviceId] = channelRegionIds;
      }
      channelRegionIds->push_back(regionId);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeOpenTvChannelDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeFreesatChannelDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    const vector<unsigned short> bouquetRegionIds,
                                                    map<unsigned short, bool>& visibleInGuideFlags,
                                                    map<unsigned short, unsigned short>& channelIds,
                                                    map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                                    map<unsigned short, vector<unsigned short>*>& regionIds) const
{
  // <loop>
  //   service ID - 2 bytes
  //   reserved - 1 bit, always 1
  //   channel ID - 15 bits (minimum 14 bits for uniqueness, 15th bit varies without pattern and matching category mapping)
  //   LCN loop byte count - 1 byte
  //   <loop>
  //     flags - 4 bits
  //     logical channel number - 12 bits
  //     region ID - 2 bytes
  //   </loop>
  // </loop>
  if (dataLength == 0)
  {
    LogDebug(L"%s: invalid Freesat channel descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 4 < dataLength)
    {
      unsigned short serviceId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short channelId = ((data[pointer] & 0x7f) << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char lcnLoopLength = data[pointer++];
      //LogDebug(L"%s: Freesat channel descriptor, service ID = %hu, channel ID = %hu, LCN loop length = %hhu",
      //          m_name, serviceId, channelId, lcnLoopLength);

      unsigned short endOfLcnLoop = pointer + lcnLoopLength;
      if (endOfLcnLoop > dataLength || lcnLoopLength % 4 != 0)
      {
        LogDebug(L"%s: invalid Freesat channel descriptor, descriptor length = %hhu, pointer = %hu, LCN loop length = %hhu, service ID = %hu, channel ID = %hu",
                  m_name, dataLength, pointer, lcnLoopLength, serviceId,
                  channelId);
        CleanUpMapOfVectors(regionIds);
        return false;
      }

      channelIds[serviceId] = channelId;

      if (lcnLoopLength == 0)
      {
        if (visibleInGuideFlags.find(serviceId) == visibleInGuideFlags.end())
        {
          visibleInGuideFlags[serviceId] = false;
        }
        continue;
      }

      while (pointer + 3 < endOfLcnLoop)
      {
        // b0 (MSB) = ???
        // b1 = visible in guide
        // b2 = ??? (only used for certain BBC One/Two entries; perhaps related to regional or HD/SD LCN swaps)
        // b3 (LSB) = ???
        unsigned char flags = data[pointer] >> 4;   // values = 0x1, 0x9, 0xd, 0xf
        unsigned short logicalChannelNumber = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
        pointer += 2;
        unsigned short regionId = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        //LogDebug(L"  flags = 0x%hhx, LCN = %hu, region ID = %hu",
        //          flags, logicalChannelNumber, regionId);

        if (
          regionId == 0 ||      // "no region"
          regionId == 0x64 ||   // "bogus region"
          (flags & 4) == 0      // "invisible channel number"
        )
        {
          continue;
        }

        visibleInGuideFlags[serviceId] = true;

        vector<unsigned short>* channelRegionIds = regionIds[serviceId];
        if (channelRegionIds == NULL)
        {
          channelRegionIds = new vector<unsigned short>();
          if (channelRegionIds == NULL)
          {
            LogDebug(L"%s: failed to allocate vector for service %hu's Freesat region IDs",
                      m_name, serviceId);
            regionIds.erase(serviceId);
          }
          else
          {
            regionIds[serviceId] = channelRegionIds;
          }
        }

        if (regionId == 0xffff) // "all regions within bouquet"
        {
          vector<unsigned short>::const_iterator regionIdIt = bouquetRegionIds.begin();
          for ( ; regionIdIt != bouquetRegionIds.end(); regionIdIt++)
          {
            unsigned bouquetRegionId = *regionIdIt;
            AddLogicalChannelNumber(serviceId,
                                    0xd3,
                                    bouquetRegionId,
                                    logicalChannelNumber,
                                    L"Freesat",
                                    logicalChannelNumbers);

            if (channelRegionIds != NULL && find(channelRegionIds->begin(), channelRegionIds->end(), bouquetRegionId) == channelRegionIds->end())
            {
              channelRegionIds->push_back(*regionIdIt);
            }
          }
        }
        else
        {
          AddLogicalChannelNumber(serviceId,
                                  0xd3,
                                  regionId,
                                  logicalChannelNumber,
                                  L"Freesat",
                                  logicalChannelNumbers);
          if (channelRegionIds != NULL && find(channelRegionIds->begin(), channelRegionIds->end(), regionId) == channelRegionIds->end())
          {
            channelRegionIds->push_back(regionId);
          }
        }
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeFreesatChannelDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeT2TerrestrialDeliverySystemDescriptor(const unsigned char* data,
                                                                unsigned char dataLength,
                                                                CRecordNitTransmitterTerrestrial& record,
                                                                map<unsigned long, unsigned long>& frequencies) const
{
  if (dataLength < 4)
  {
    LogDebug(L"%s: invalid T2 terrestrial delivery system descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    record.IsT2 = true;
    record.PlpId = data[1];
    record.T2SystemId = (data[2] << 8) | data[3];
    if (dataLength == 4)
    {
      //LogDebug(L"%s: T2 terrestrial delivery system descriptor, PLP ID = %hhu, T2 system ID = %hu",
      //          m_name, record.PlpId, record.T2SystemId);
      return true;
    }
    if (dataLength < 6)
    {
      LogDebug(L"%s: invalid T2 terrestrial delivery system descriptor, length = %hhu, PLP ID = %hhu, T2 system ID = %hu",
                m_name, dataLength, record.PlpId, record.T2SystemId);
      return false;
    }

    unsigned char sisoMiso = data[4] >> 6;
    record.MultipleInputStreamFlag = sisoMiso != 0;

    record.Bandwidth = (data[4] & 0x3c) >> 2;
    switch (record.Bandwidth)
    {
      case 1:
        record.Bandwidth = 7000;
        break;
      case 2:
        record.Bandwidth = 6000;
        break;
      case 3:
        record.Bandwidth = 5000;
        break;
      case 4:
        record.Bandwidth = 10000;
        break;
      case 5:
        record.Bandwidth = 1712;
        break;
      default:
        record.Bandwidth = 8000;
        break;
    }

    record.GuardInterval = data[5] >> 5;
    record.TransmissionMode = (data[5] >> 2) & 0x7;
    record.OtherFrequencyFlag = (data[5] & 0x2) != 0;
    record.TimeFrequencySlicingFlag = (data[5] & 1) != 0;
    //LogDebug(L"%s: T2 terrestrial delivery system descriptor, PLP ID = %hhu, T2 system ID = %hu, SISO/MISO = %hhu, bandwidth = %hu kHz, guard interval = %hhu, transmission mode = %hhu, other frequency flag = %d, time-frequency slicing flag = %d",
    //          m_name, record.PlpId, record.T2SystemId, sisoMiso,
    //          record.Bandwidth, record.GuardInterval, record.TransmissionMode,
    //          record.OtherFrequencyFlag, record.TimeFrequencySlicingFlag);

    unsigned short pointer = 6;
    while (pointer + 3 < dataLength)
    {
      unsigned long cellId = (data[pointer] << 8) + data[pointer + 1];
      pointer += 2;

      unsigned long frequency = 0;
      if (record.TimeFrequencySlicingFlag)
      {
        unsigned char frequencyLoopLength = data[pointer++];
        //LogDebug(L"  cell ID = %lu, frequency loop length = %hhu",
        //          cellId, frequencyLoopLength);
        unsigned short endOfFrequencyLoop = pointer + frequencyLoopLength;
        if (endOfFrequencyLoop + 1 > dataLength || frequencyLoopLength % 4 != 0)
        {
          LogDebug(L"%s: invalid T2 terrestrial delivery system descriptor, descriptor length = %hhu, pointer = %hu, frequency loop length = %hhu, PLP ID = %hhu, T2 system ID = %hu, cell ID = %lu",
                    m_name, dataLength, pointer, frequencyLoopLength,
                    record.PlpId, record.T2SystemId, cellId);
          return false;
        }
        while (pointer + 3 < endOfFrequencyLoop)
        {
          frequency = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
          pointer += 4;
          //LogDebug(L"    frequency = %lu kHz", frequency);

          // TFS is not supported by the wider TV Server at this time. We'll
          // end up recording the last frequency in the set.
          frequencies[cellId << 8] = frequency;
        }
      }
      else
      {
        if (pointer + 4 > dataLength)
        {
          LogDebug(L"%s: invalid T2 terrestrial delivery system descriptor, descriptor length = %hhu, pointer = %hu, PLP ID = %hhu, T2 system ID = %hu, cell ID = %lu",
                    m_name, dataLength, pointer, record.PlpId,
                    record.T2SystemId, cellId);
          return false;
        }
        frequency = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;

        frequencies[cellId << 8] = frequency;
      }

      unsigned char subcellInfoLoopLength = data[pointer++];
      //LogDebug(L"  cell ID = %lu, frequency = %lu kHz, sub-cell info loop length = %hhu",
      //          cellId, frequency, subcellInfoLoopLength);
      unsigned short endOfSubCellInfoLoop = pointer + subcellInfoLoopLength;
      if (endOfSubCellInfoLoop > dataLength || subcellInfoLoopLength % 5 != 0)
      {
        LogDebug(L"%s: invalid T2 terrestrial delivery system descriptor, descriptor length = %hhu, pointer = %hu, sub-cell info loop length = %hhu, PLP ID = %hhu, T2 system ID = %hu, cell ID = %lu",
                  m_name, dataLength, pointer, subcellInfoLoopLength,
                  record.PlpId, record.T2SystemId, cellId);
        return false;
      }
      while (pointer + 4 < endOfSubCellInfoLoop)
      {
        unsigned char cellIdExtension = data[pointer++];
        frequency = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        //LogDebug(L"    cell ID extension = %hhu, frequency = %lu kHz",
        //          cellIdExtension, frequency);

        frequencies[(cellId << 8) + cellIdExtension] = frequency;
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeT2TerrestrialDeliverySystemDescriptor()",
              m_name);
  }
  return false;
}

bool CParserNitDvb::DecodeC2CableDeliverySystemDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          CRecordNitTransmitterCable& record) const
{
  if (dataLength != 8)
  {
    LogDebug(L"%s: invalid C2 cable delivery system descriptor, length = %hhu",
              m_name, dataLength);
    return false;
  }
  try
  {
    record.IsC2 = true;
    record.PlpId = data[1];
    record.DataSliceId = data[2];

    // Frequency is specified in Hz. We want the value in kHz.
    unsigned long frequency = ((data[3] << 24) | (data[4] << 16) | (data[5] << 8) | data[6]) / 1000;
    record.Frequencies.push_back(frequency);

    record.FrequencyType = data[7] >> 6;
    record.ActiveOfdmSymbolDuration = (data[7] & 0x38) >> 3;
    record.GuardInterval = data[7] & 0x7;
    //LogDebug(L"%s: C2 cable delivery system descriptor, PLP ID = %hhu, data slice ID = %hhu, frequency = %lu kHz, frequency type = %hhu, active OFDM symbol duration = %hhu, guard interval = %hhu",
    //          record.PlpId, record.DataSliceId, frequency,
    //          record.FrequencyType, record.ActiveOfdmSymbolDuration,
    //          record.GuardInterval);
    return true;
  }
  catch (...)
  {
    LogDebug(L"%s: unhandled exception in DecodeC2CableDeliverySystemDescriptor()",
              m_name);
  }
  return false;
}

unsigned long CParserNitDvb::DecodeCableFrequency(const unsigned char* data)
{
  // Frequency in MHz is encoded with BCD digits. The DP is after the 4th
  // digit. We want the frequency in kHz.
  unsigned long frequency = (1000000 * ((data[0] >> 4) & 0xf));
  frequency += (100000 * (data[0] & 0xf));
  frequency += (10000 * ((data[1] >> 4) & 0xf));
  frequency += (1000 * (data[1] & 0xf));
  frequency += (100 * ((data[2] >> 4) & 0xf));
  frequency += (10 * (data[2] & 0xf));
  frequency += ((data[3] >> 4) & 0xf);
  return frequency;
}

unsigned long CParserNitDvb::DecodeSatelliteFrequency(const unsigned char* data)
{
  // Frequency in GHz is encoded with BCD digits. The DP is after the 3rd
  // digit. We want the frequency in kHz.
  unsigned long frequency = (100000000 * ((data[0] >> 4) & 0xf));
  frequency += (10000000 * (data[0] & 0xf));
  frequency += (1000000 * ((data[1] >> 4) & 0xf));
  frequency += (100000 * (data[1] & 0xf));
  frequency += (10000 * ((data[2] >> 4) & 0xf));
  frequency += (1000 * (data[2] & 0xf));
  frequency += (100 * ((data[3] >> 4) & 0xf));
  frequency += (10 * (data[3] & 0xf));
  return frequency;
}

unsigned long CParserNitDvb::DecodeTerrestrialFrequency(const unsigned char* data)
{
  // Frequency is specified in units of 10 Hz. We want the value in kHz.
  return ((data[0] << 24) + (data[1] << 16) + (data[2] << 8) + data[3]) / 100;
}

unsigned long CParserNitDvb::GetLinkageKey(unsigned short originalNetworkId,
                                            unsigned short transportStreamId)
{
  return (originalNetworkId << 16) | transportStreamId;
}

void CParserNitDvb::OnTableComplete(unsigned char tableId)
{
}

void CParserNitDvb::OnNitServiceRemoved(unsigned char tableId,
                                        unsigned short tableIdExtension,
                                        unsigned short originalNetworkId,
                                        unsigned short transportStreamId,
                                        unsigned short serviceId,
                                        unsigned short freesatChannelId,
                                        unsigned short openTvChannelId,
                                        bool visibleInGuide,
                                        const map<unsigned long, unsigned short>& logicalChannelNumbers,
                                        const char* defaultAuthority,
                                        const vector<unsigned long>& availableInCells,
                                        const vector<unsigned long long>& targetRegionIds,
                                        const vector<unsigned short>& freesatRegionIds,
                                        const vector<unsigned short>& openTvRegionIds,
                                        const vector<unsigned short>& freesatChannelCategoryIds,
                                        const vector<unsigned char>& norDigChannelListIds,
                                        const vector<unsigned long>& availableInCountries,
                                        const vector<unsigned long>& unavailableInCountries)
{
  if (m_callBack != NULL)
  {
    m_callBack->OnNitServiceRemoved(tableId,
                                    tableIdExtension,
                                    originalNetworkId,
                                    transportStreamId,
                                    serviceId,
                                    freesatChannelId,
                                    openTvChannelId,
                                    visibleInGuide,
                                    logicalChannelNumbers,
                                    defaultAuthority,
                                    availableInCells,
                                    targetRegionIds,
                                    freesatRegionIds,
                                    openTvRegionIds,
                                    freesatChannelCategoryIds,
                                    norDigChannelListIds,
                                    availableInCountries,
                                    unavailableInCountries);
  }

  unsigned long long dvbId = ((unsigned long long)originalNetworkId << 32) | ((unsigned long long)transportStreamId << 16) | serviceId;
  map<unsigned long long, vector<CRecordNitService*>*>::iterator servicesByDvbIdIt = m_cacheServiceRecordsByDvbId.find(dvbId);
  if (servicesByDvbIdIt == m_cacheServiceRecordsByDvbId.end())
  {
    return;
  }

  vector<CRecordNitService*>* servicesByDvbId = servicesByDvbIdIt->second;
  if (servicesByDvbId != NULL)
  {
    vector<CRecordNitService*>::iterator serviceIt = servicesByDvbId->begin();
    while (serviceIt != servicesByDvbId->end())
    {
      CRecordNitService* recordService = *serviceIt;
      if (recordService == NULL)
      {
        serviceIt = servicesByDvbId->erase(serviceIt);
        continue;
      }
      if (
        recordService->TableId == tableId &&
        recordService->TableIdExtension == tableIdExtension
      )
      {
        serviceIt = servicesByDvbId->erase(serviceIt);
        break;
      }
      serviceIt++;
    }

    if (servicesByDvbId->size() != 0)
    {
      return;
    }

    delete servicesByDvbId;
    servicesByDvbIdIt->second = NULL;
  }

  m_cacheServiceRecordsByDvbId.erase(servicesByDvbIdIt);
}