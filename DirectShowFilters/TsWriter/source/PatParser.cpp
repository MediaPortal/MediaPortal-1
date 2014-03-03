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
#pragma warning(disable : 4995)
#include <algorithm>
#include <windows.h>
#include "PatParser.h"


void LogDebug(const char *fmt, ...) ;

CPatParser::CPatParser(void)
{
  SetPid(PID_PAT);
  Reset();
  m_callBack = NULL;
}

CPatParser::~CPatParser(void)
{
  CleanUp();
}

void CPatParser::CleanUp()
{
  map<int, ProgramDetail*>::iterator it = m_programs.begin();
  while (it != m_programs.end())
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
    it++;
  }
  m_programs.clear();
}

void CPatParser::Reset()
{
  LogDebug("PAT: reset");
  CSectionDecoder::Reset();
  m_currentVersionNumber = -1;
  m_unseenSections.clear();
  CleanUp();
  m_isReady = false;
  LogDebug("PAT: reset done");
}

void CPatParser::SetCallBack(IPatCallBack* callBack)
{
  m_callBack = callBack;
}

bool CPatParser::IsReady()
{
  return m_isReady;
}

int CPatParser::GetProgramCount()
{
  return (int)m_programs.size();
}

int CPatParser::GetProgram(int idx, int* programNumber, int* pmtPid)
{
  if (idx < 0)
  {
    return S_FALSE;
  }
  int i = 0;
  for (map<int, ProgramDetail*>::iterator it = m_programs.begin(); it != m_programs.end(); it++)
  {
    if (i == idx)
    {
      *programNumber = it->first;
      *pmtPid = it->second->Pid;
      return S_OK;
    }
    i++;
  }
  return S_FALSE;
}

int CPatParser::GetPmtPid(int programNumber, int* pmtPid)
{
  map<int, ProgramDetail*>::iterator it = m_programs.find(programNumber);
  if (it == m_programs.end())
  {
    return S_FALSE;
  }
  *pmtPid = it->second->Pid;
  return S_OK;
}

void CPatParser::OnNewSection(CSection& sections)
{
  // 0x00 = standard PAT table ID
  if (sections.table_id != 0)
  {
    return;
  }
  if (m_callBack == NULL)
  {
    return;
  }
  byte* section = sections.Data;

  try
  {
    int sectionSyntaxIndicator = section[1] & 0x80;
    int sectionLength = ((section[1] & 0xf) << 8) + section[2];
    if (sectionLength > 1021 || sectionLength < 9)
    {
      LogDebug("PAT: invalid section length = %d", sectionLength);
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
    //LogDebug("PAT: section number = %d, version number = %d, last section number = %d, section length = %d, end of section = %d",
    //          sectionNumber, versionNumber, lastSectionNumber, sectionLength, endOfSection);

    if (versionNumber > m_currentVersionNumber || (versionNumber == MAX_TABLE_VERSION_NUMBER && versionNumber < m_currentVersionNumber))
    {
      LogDebug("PAT: new table version, version = %d, section number = %d, last section number = %d", sections.table_id, versionNumber, sectionNumber, lastSectionNumber);
      m_isReady = false;
      if (m_currentVersionNumber != -1)
      {
        for (map<int, ProgramDetail*>::iterator it = m_programs.begin(); it != m_programs.end(); it++)
        {
          it->second->IsCurrent = false;
        }
      }
      m_currentVersionNumber = versionNumber;
      m_unseenSections.clear();
      for (int s = 0; s <= lastSectionNumber; s++)
      {
        m_unseenSections.push_back(s);
      }
    }
    vector<int>::iterator sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionNumber);
    if (sectionIt == m_unseenSections.end())
    {
      //LogDebug("PAT: previously seen section %d", sectionNumber);
      return;
    }
    else
    {
      //LogDebug("PAT: new section %d", sectionNumber);
    }

    int pointer = 8;  // points to the first byte in the program loop
    while (pointer + 3 < endOfSection)
    {
      int programNumber = (section[pointer] << 8) + section[pointer + 1];
      pointer += 2;
      int pmtPid = ((section[pointer] & 0x1f) << 8) + section[pointer + 1];
      pointer += 2;
      //LogDebug("PAT: program number = %d, PMT PID = %d", programNumber, pmtPid);

      // There used to be more extensive checks here. Since we *always* have the CRC
      // checking enabled for this parser, we should be able to trust that the PIDs
      // are correct. Adding checks as were here before (eg. pmtPid > 0x12) can cause
      // problems for non-DVB streams. For the given example, there would be a problem
      // for ATSC streams which sometimes use PMT PID 0x10 for the first program.
      if (programNumber > 0)  // Ignore the network information table PID entry...
      {
        ProgramDetail* detail;
        map<int, ProgramDetail*>::iterator it = m_programs.find(programNumber);
        if (it == m_programs.end())
        {
          detail = new ProgramDetail();
          detail->ProgramNumber = programNumber;
          m_programs[programNumber] = detail;
          LogDebug("PAT: PMT PID for program number %d is %d", programNumber, pmtPid);
          if (m_callBack != NULL)
          {
            m_callBack->OnPatReceived(programNumber, pmtPid);
          }
        }
        else
        {
          detail = it->second;
          if (detail->Pid != pmtPid)
          {
            LogDebug("PAT: PMT PID for program number %d changed from %d to %d", programNumber, detail->Pid, pmtPid);
            if (m_callBack != NULL)
            {
              m_callBack->OnPatChanged(programNumber, detail->Pid, pmtPid);
            }
          }
        }
        detail->Pid = pmtPid;
        detail->IsCurrent = true;
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug("PAT: section parsing error");
      return;
    }

    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      map<int, ProgramDetail*>::iterator it = m_programs.begin();
      while (it != m_programs.end())
      {
        ProgramDetail* detail = it->second;
        if (!detail->IsCurrent)
        {
          LogDebug("PAT: program number %d has been removed", detail->ProgramNumber);
          if (m_callBack != NULL)
          {
            m_callBack->OnPatRemoved(detail->ProgramNumber, detail->Pid);
          }
          delete it->second;
          it->second = NULL;
          m_programs.erase(it++);
        }
        else
        {
          it++;
        }
      }
      LogDebug("PAT: ready");
      m_isReady = true;
    }
  }
  catch (...)
  {
    LogDebug("PAT: unhandled exception in OnNewSection()");
  }
}