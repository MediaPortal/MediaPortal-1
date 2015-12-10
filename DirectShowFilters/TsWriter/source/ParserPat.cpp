/*
 *  Copyright (C) 2006-2010 Team MediaPortal
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
#include "ParserPat.h"
#include <algorithm>
#include "EnterCriticalSection.h"


#define VERSION_NOT_SET 0xff


extern void LogDebug(const wchar_t* fmt, ...);

CParserPat::CParserPat(void) : m_records(600000)
{
  m_isReady = false;
  m_version = VERSION_NOT_SET;
  m_transportStreamId = 0;
  m_networkPid = 0xffff;

  SetPid(PID_PAT);
  SetCallBack(NULL);
}

CParserPat::~CParserPat(void)
{
  SetCallBack(NULL);
}

void CParserPat::Reset()
{
  LogDebug(L"PAT: reset");
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  CSectionDecoder::Reset();
  m_isReady = false;
  m_version = VERSION_NOT_SET;
  m_unseenSections.clear();
  m_transportStreamId = 0;
  m_networkPid = 0;
  LogDebug(L"PAT: reset done");
}

void CParserPat::SetCallBack(ICallBackPat* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserPat::OnNewSection(CSection& section)
{
  try
  {
    if (
      section.table_id != TABLE_ID_PAT ||
      !section.SectionSyntaxIndicator ||
      section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.section_length > 1021 || section.section_length < 9)
    {
      LogDebug(L"PAT: invalid section, length = %d", section.section_length);
      return;
    }

    //LogDebug(L"PAT: TSID = %d, version number = %d, section length = %d, section number = %d, last section number = %d",
    //          section.table_id_extension, section.version_number,
    //          section.section_length, section.SectionNumber,
    //          section.LastSectionNumber);

    CEnterCriticalSection lock(m_section);
    unsigned short oldTransportStreamId = m_transportStreamId;
    if (section.table_id_extension != m_transportStreamId || section.version_number != m_version)
    {
      m_isReady = false;

      if (m_version != VERSION_NOT_SET)
      {
        LogDebug(L"PAT: changed, TSID = %d, version number = %d, prev. TSID = %hu, prev. version number = %hhu, section number = %d, last section number = %d",
                  section.table_id_extension, section.version_number,
                  m_transportStreamId, m_version, section.SectionNumber,
                  section.LastSectionNumber);
        if (m_callBack != NULL)
        {
          m_callBack->OnTableChange(TABLE_ID_PAT);
          if (section.table_id_extension != m_transportStreamId)
          {
            m_callBack->OnPatTsidChanged(m_transportStreamId, section.table_id_extension);
          }
        }
        m_records.MarkExpiredRecords(0);
      }
      else
      {
        LogDebug(L"PAT: received, TSID = %d, version number = %d, section number = %d, last section number = %d",
                  section.table_id_extension, section.version_number,
                  section.SectionNumber, section.LastSectionNumber);
        if (m_callBack != NULL)
        {
          m_callBack->OnTableSeen(TABLE_ID_PAT);
        }
      }

      m_unseenSections.clear();
      for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
      {
        m_unseenSections.push_back(s);
      }
      m_version = section.version_number;
      m_transportStreamId = section.table_id_extension;
    }

    vector<unsigned char>::const_iterator sectionIt = find(m_unseenSections.begin(),
                                                            m_unseenSections.end(),
                                                            section.SectionNumber);
    if (m_isReady || sectionIt == m_unseenSections.end())
    {
      //LogDebug(L"PAT: previously seen section %d", section.SectionNumber);
      return;
    }
    else
    {
      //LogDebug(L"PAT: new section %d", section.SectionNumber);
    }

    bool seenNetworkPid = false;
    unsigned char* data = section.Data;
    unsigned short pointer = 8;                               // points to the first byte in the program loop
    unsigned short endOfSection = section.section_length - 1; // points to the first byte in the CRC
    while (pointer + 3 < endOfSection)
    {
      unsigned short programNumber = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short pmtPid = ((data[pointer] & 0x1f) << 8) | data[pointer + 1];
      pointer += 2;
      if (programNumber == 0)
      {
        LogDebug(L"PAT: network PID = %hu", pmtPid);
        seenNetworkPid = true;
        if (m_networkPid != pmtPid)
        {
          if (m_callBack != NULL)
          {
            m_callBack->OnPatNetworkPidChanged(m_networkPid, pmtPid);
          }
          m_networkPid = pmtPid;
        }
        continue;
      }
      //LogDebug(L"PAT: program number = %hu, PMT PID = %hu", programNumber, pmtPid);

      // There used to be more extensive checks here. Since we *always* have the CRC
      // checking enabled for this parser, we should be able to trust that the PIDs
      // are correct. Adding checks as were here before (eg. pmtPid > 0x12) can cause
      // problems for non-DVB streams. For the given example, there would be a problem
      // for ATSC streams which sometimes use PMT PID 0x10 for the first program.
      CRecordPat* record = new CRecordPat();
      if (record == NULL)
      {
        LogDebug(L"PAT: failed to allocate record, version number = %d, section number = %d, program number = %hu, PMT PID = %hu",
                  section.version_number, section.SectionNumber,
                  programNumber, pmtPid);
        continue;
      }
      record->ProgramNumber = programNumber;
      record->Pid = pmtPid;
      m_records.AddOrUpdateRecord((IRecord**)&record, m_callBack);
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"PAT: section parsing error, pointer = %hu, end of section = %hu, version number = %d, section number = %d",
                pointer, endOfSection, section.version_number,
                section.SectionNumber);
      return;
    }

    if (!seenNetworkPid)
    {
      if (m_callBack != NULL)
      {
        m_callBack->OnPatNetworkPidChanged(m_networkPid, 0);
      }
      m_networkPid = 0;
    }

    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      m_records.RemoveExpiredRecords(m_callBack);
      LogDebug(L"PAT: ready, program count = %lu", m_records.GetRecordCount());
      m_isReady = true;
      if (m_callBack != NULL)
      {
        m_callBack->OnTableComplete(TABLE_ID_PAT);
      }
    }
  }
  catch (...)
  {
    LogDebug(L"PAT: unhandled exception in OnNewSection()");
  }
}

bool CParserPat::IsReady() const
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

void CParserPat::GetTransportStreamDetail(unsigned short& transportStreamId,
                                          unsigned short& networkPid,
                                          unsigned short& programCount) const
{
  CEnterCriticalSection lock(m_section);
  transportStreamId = m_transportStreamId;
  networkPid = m_networkPid;
  programCount = (unsigned short)m_records.GetRecordCount();
}

bool CParserPat::GetProgram(unsigned short index,
                            unsigned short& programNumber,
                            unsigned short& pmtPid) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_records.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"PAT: invalid program index, index = %hu, record count = %lu",
              index, m_records.GetRecordCount());
    return false;
  }

  CRecordPat* recordPat = dynamic_cast<CRecordPat*>(record);
  if (recordPat == NULL)
  {
    LogDebug(L"PAT: invalid program record, index = %hu", index);
    return false;
  }
  programNumber = recordPat->ProgramNumber;
  pmtPid = recordPat->Pid;
  return true;
}

bool CParserPat::GetPmtPid(unsigned short programNumber, unsigned short& pmtPid) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_records.GetRecordByKey(programNumber, &record) || record == NULL)
  {
    LogDebug(L"PAT: invalid program number, program number = %hu",
              programNumber);
    return false;
  }

  CRecordPat* recordPat = dynamic_cast<CRecordPat*>(record);
  if (recordPat == NULL)
  {
    LogDebug(L"PAT: invalid program record, program number = %hu", programNumber);
    return false;
  }
  pmtPid = recordPat->Pid;
  return true;
}