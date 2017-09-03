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
#include "ParserMgt.h"
#include <algorithm>    // find()
#include "..\..\shared\EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserMgt::CParserMgt(unsigned short pid) : m_records(600000)
{
  m_pid = pid;
  SetCallBack(NULL);
}

CParserMgt::~CParserMgt()
{
  SetCallBack(NULL);
}

void CParserMgt::Reset()
{
  LogDebug(L"MGT %hu: reset", m_pid);
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  m_seenSections.clear();
  m_unseenSections.clear();
  LogDebug(L"MGT %hu: reset done", m_pid);
}

void CParserMgt::SetCallBack(ICallBackMgt* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserMgt::OnNewSection(const CSection& section)
{
  try
  {
    if (
      section.TableId != TABLE_ID_MGT ||
      !section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.SectionLength > 4093 || section.SectionLength < 14)
    {
      LogDebug(L"MGT %hu: invalid section, length = %hu",
                m_pid, section.SectionLength);
      return;
    }
    unsigned char protocolVersion = section.Data[8];
    if (protocolVersion != 0)
    {
      LogDebug(L"MGT %hu: unsupported protocol version, protocol version = %hhu",
                m_pid, protocolVersion);
      return;
    }

    const unsigned char* data = section.Data;
    unsigned short mapId = section.TableIdExtension;    // SCTE 65
    unsigned short tablesDefined = (data[9] << 8) | data[10];
    //LogDebug(L"MGT %hu: map ID = %hu, protocol version = %hhu, version number = %hhu, section length = %hu, section number = %hhu, last section number = %hhu, tables defined = %hu",
    //          m_pid, mapId, protocolVersion, section.VersionNumber,
    //          section.SectionLength, section.SectionNumber,
    //          section.LastSectionNumber, tablesDefined);

    // Have we seen this section before?
    unsigned short sectionKey = (section.VersionNumber << 8) | section.SectionNumber;
    CEnterCriticalSection lock(m_section);
    vector<unsigned short>::const_iterator sectionIt = find(m_seenSections.begin(), m_seenSections.end(), sectionKey);
    if (sectionIt != m_seenSections.end())
    {
      //LogDebug(L"MGT %hu: previously seen section, map ID = %hu, protocol version = %hhu, section number = %hhu",
      //          m_pid, mapId, protocolVersion, section.SectionNumber);
      return;
    }

    // Were we expecting this section?
    sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
    if (sectionIt == m_unseenSections.end())
    {
      // No. Is this a change/update, or the first section?
      if (m_unseenSections.size() != 0 || m_seenSections.size() != 0)
      {
        LogDebug(L"MGT %hu: changed, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                  m_pid, mapId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, section.LastSectionNumber);
        m_records.MarkExpiredRecords(0);
        m_seenSections.clear();
        m_unseenSections.clear();
        if (m_callBack != NULL)
        {
          m_callBack->OnTableChange(TABLE_ID_MGT);
        }
      }
      else
      {
        LogDebug(L"MGT %hu: received, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                  m_pid, mapId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, section.LastSectionNumber);
        if (m_callBack != NULL)
        {
          m_callBack->OnTableSeen(TABLE_ID_MGT);
        }
      }

      unsigned short baseKey = sectionKey & 0xff00;
      for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
      {
        m_unseenSections.push_back(baseKey + s);
      }
      sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
    }
    else
    {
      //LogDebug(L"MGT %hu: new section, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu",
      //          m_pid, mapId, protocolVersion, section.VersionNumber,
      //          section.SectionNumber);
    }

    unsigned short pointer = 11;                              // points to the first byte in the table loop
    unsigned short endOfSection = section.SectionLength - 1;  // points to the first byte in the CRC
    while (pointer + 10 < endOfSection - 2)
    {
      CRecordMgt* record = new CRecordMgt();
      if (record == NULL)
      {
        LogDebug(L"MGT %hu: failed to allocate record, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu",
                  m_pid, mapId, protocolVersion, section.VersionNumber,
                  section.SectionNumber);
        return;
      }

      if (!DecodeTableRecord(data, pointer, endOfSection, *record))
      {
        LogDebug(L"MGT %hu: invalid section, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, table type = %hu, PID = %hu",
                  m_pid, mapId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, record->TableType, record->Pid);
        delete record;
        return;
      }
      m_records.AddOrUpdateRecord((IRecord**)&record, m_callBack);
    }

    unsigned short descriptorsLength = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
    pointer += 2;
    //LogDebug(L"MGT %hu: descriptors length = %hu, pointer = %hu",
    //          m_pid, descriptorsLength, pointer);
    unsigned short endOfDescriptors = pointer + descriptorsLength;
    if (endOfDescriptors != endOfSection)
    {
      LogDebug(L"MGT %hu: invalid section, descriptors length = %hu, pointer = %hu, end of section = %hu, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu",
                m_pid, descriptorsLength, pointer, endOfSection, mapId,
                protocolVersion, section.VersionNumber,
                section.SectionNumber);
      return;
    }

    while (pointer + 1 < endOfDescriptors)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      //LogDebug(L"MGT %hu: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          m_pid, tag, length, pointer);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug(L"MGT %hu: invalid section, descriptor length = %hhu, pointer = %hu, end of descriptors = %hu, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, tag = 0x%hhx, end of section = %hu",
                  m_pid, length, pointer, endOfDescriptors, mapId, protocolVersion,
                  section.VersionNumber, section.SectionNumber, tag,
                  endOfSection);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"MGT %hu: section parsing error, pointer = %hu, end of section = %hu, map ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu",
                m_pid, pointer, endOfSection, mapId, protocolVersion,
                section.VersionNumber, section.SectionNumber);
      return;
    }

    m_seenSections.push_back(sectionKey);
    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      m_records.RemoveExpiredRecords(m_callBack);
      LogDebug(L"MGT %hu: ready, sections parsed = %llu, record count = %lu",
                m_pid, (unsigned long long)m_seenSections.size(),
                m_records.GetRecordCount());
      if (m_callBack != NULL)
      {
        m_callBack->OnTableComplete(TABLE_ID_MGT);
      }
    }
  }
  catch (...)
  {
    LogDebug(L"MGT %hu: unhandled exception in OnNewSection()", m_pid);
  }
}

bool CParserMgt::IsSeen() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSections.size() != 0;
}

bool CParserMgt::IsReady() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSections.size() != 0 && m_unseenSections.size() == 0;
}

unsigned short CParserMgt::GetTableCount() const
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_records.GetRecordCount();
}

bool CParserMgt::GetTable(unsigned short index,
                          unsigned short& tableType,
                          unsigned short& pid,
                          unsigned char& versionNumber,
                          unsigned long& numberBytes) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_records.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"MGT %hu: invalid table index, index = %hu, record count = %lu",
              m_pid, index, m_records.GetRecordCount());
    return false;
  }

  CRecordMgt* recordMgt = dynamic_cast<CRecordMgt*>(record);
  if (recordMgt == NULL)
  {
    LogDebug(L"MGT %hu: invalid table record, index = %hu", m_pid, index);
    return false;
  }
  tableType = recordMgt->TableType;
  pid = recordMgt->Pid;
  versionNumber = recordMgt->VersionNumber;
  numberBytes = recordMgt->NumberBytes;
  return true;
}

bool CParserMgt::DecodeTableRecord(const unsigned char* sectionData,
                                    unsigned short& pointer,
                                    unsigned short endOfSection,
                                    CRecordMgt& record)
{
  if (pointer + 11 > endOfSection - 2)
  {
    LogDebug(L"MGT: invalid table record, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.TableType = (sectionData[pointer] << 8) | sectionData[pointer + 1];
    pointer += 2;
    record.Pid = ((sectionData[pointer] & 0x1f) << 8) | sectionData[pointer + 1];
    pointer += 2;
    record.VersionNumber = sectionData[pointer++] & 0x1f;
    record.NumberBytes = (sectionData[pointer] << 24) | (sectionData[pointer + 1] << 16) | (sectionData[pointer + 2] << 8) | sectionData[pointer + 3];
    pointer += 4;
    unsigned short tableTypeDescriptorsLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;

    //LogDebug(L"MGT: table type = %hu, PID = %hu, version number = %hhu, number bytes = %lu, table type descriptors length = %hu",
    //          record.TableType, record.Pid, record.VersionNumber,
    //          record.NumberBytes, tableTypeDescriptorsLength);

    unsigned short endOfTableTypeDescriptors = pointer + tableTypeDescriptorsLength;
    if (endOfTableTypeDescriptors > endOfSection - 2)   // - 2 for section descriptors length
    {
      LogDebug(L"MGT: invalid table record, table type descriptors loop length = %hu, pointer = %hu, end of section = %hu",
                tableTypeDescriptorsLength, pointer, endOfSection);
      return false;
    }

    while (pointer + 1 < endOfTableTypeDescriptors)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"MGT: table type descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfTableTypeDescriptors)
      {
        LogDebug(L"MGT: invalid table record, table type descriptor length = %hhu, pointer = %hu, end of table type descriptors = %hu, tag = 0x%hhx, end of section = %hu",
                  length, pointer, endOfTableTypeDescriptors, tag,
                  endOfSection);
        return false;
      }

      pointer = endOfDescriptor;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"MGT: unhandled exception in DecodeTableRecord()");
  }
  return false;
}