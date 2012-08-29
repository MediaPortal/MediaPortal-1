/* 
 *	Copyright (C) 2006-2010 Team MediaPortal
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
#pragma warning(disable : 4995)
#include <windows.h>
#include "PatParser.h"


void LogDebug(const char *fmt, ...) ;

CPatParser::CPatParser(void)
{
  SetPid(PID_PAT);
  Reset();
  m_pCallBack = NULL;
}

CPatParser::~CPatParser(void)
{
}

void CPatParser::Reset()
{
  LogDebug("PatParser: reset");
  CSectionDecoder::Reset();
  m_mSeenSections.clear();
  m_mServices.clear();
  m_bIsReady = false;
  LogDebug("PatParser: reset done");
}

void CPatParser::SetCallBack(IPatCallBack* callBack)
{
  m_pCallBack = callBack;
}

bool CPatParser::IsReady()
{
  return m_bIsReady;
}

int CPatParser::GetServiceCount()
{
  return (int)m_mServices.size();
}

int CPatParser::GetService(int idx, int* serviceId, int* pmtPid)
{
  if (idx < 0)
  {
    return S_FALSE;
  }
  int i = 0;
  for (map<int, int>::iterator it = m_mServices.begin(); it != m_mServices.end(); it++)
  {
    if (i == idx)
    {
      *serviceId = it->first;
      *pmtPid = it->second;
      return S_OK;
    }
    i++;
  }
  return S_FALSE;
}

int CPatParser::GetPmtPid(int serviceId, int* pmtPid)
{
  map<int, int>::iterator it = m_mServices.find(serviceId);
  if (it == m_mServices.end())
  {
    return S_FALSE;
  }
  *pmtPid = it->second;
  return S_OK;
}

void CPatParser::OnNewSection(CSection& sections)
{
	if (sections.table_id != 0)
  {
    return;
  }
  if (m_pCallBack == NULL)
  {
    return;
  }
  byte* section = sections.Data;

  try
  {
    int section_syntax_indicator = section[1] & 0x80;
    int section_length = ((section[1] & 0xf) << 8) + section[2];
    if (section_length > 1021 || section_length < 12)
    {
      LogDebug("PatParser: invalid section length = %d", section_length);
      return;
    }
    int transport_stream_id = (section[3] << 8) + section[4];
    int version_number = (section[5] >> 1) & 0x1f;
    int current_next_indicator = section[5] & 1;
    if (current_next_indicator == 0)
    {
      // Details do not yet apply...
      return;
    }
    int section_number = section[6];
    int last_section_number = section[7];

    int endOfSection = section_length - 1;
    //LogDebug("PatParser: section number = %d, version number = %d, last section number = %d, section length = %d, end of section = %d",
    //          section_number, version_number, last_section_number, section_length, endOfSection);

    int key = section_number;
    map<int, bool>::iterator it = m_mSeenSections.find(key);
    if (it != m_mSeenSections.end())
    {
      // We know about this key. Have we seen it before?
      if (it->second)
      {
        // We've seen this section before. Have we seen all the sections that we're expecting to see?
        //LogDebug("PatParser: previously seen section %x", key);
        if (!m_bIsReady)
        {
          bool ready = true;
          for (it = m_mSeenSections.begin(); it != m_mSeenSections.end(); it++)
          {
            if (!it->second)
            {
              //LogDebug("PatParser: not yet seen %x", it->first);
              ready = false;
              break;
            }
          }
          m_bIsReady = ready;
          if (ready)
          {
            LogDebug("PatParser: ready, sections parsed = %d", m_mSeenSections.size());
          }
        }
        return;
      }
    }
    else
    {
      //LogDebug("PatParser: new section %x", key);
      m_bIsReady = false;
      int k = 0;
      while (k <= last_section_number)
      {
        if (m_mSeenSections.find(k) == m_mSeenSections.end())
        {
          //LogDebug("PatParser: add section %x", k);
          m_mSeenSections[k] = false;
        }
        k++;
      }
    }

    int pointer = 8;  // points to the first byte in the service loop
    while (pointer + 3 < endOfSection)
    {
      int service_id = (section[pointer] << 8) + section[pointer + 1];
      pointer += 2;
      int pmt_pid = ((section[pointer] & 0x1f) << 8) + section[pointer + 1];
      pointer += 2;
      //LogDebug("PatParser: service ID = 0x%x, PMT PID = 0x%x", service_id, pmt_pid);

      // There used to be more extensive checks here. Since we *always* have the CRC
      // checking enabled for this parser, we should be able to trust that the PIDs
      // are correct. Adding checks as were here before (eg. pmtPid > 0x12) can cause
      // problems for non-DVB streams. For the given example, there would be a problem
      // for ATSC streams which sometimes use PMT PID 0x10 for the first program.
      if (service_id > 0)  // Ignore the network information table PID entry...
      {
        map<int, int>::iterator it = m_mServices.find(service_id);
        if ((it == m_mServices.end() || it->second != pmt_pid) && m_pCallBack != NULL)
        {
          m_mServices[service_id] = pmt_pid;
          m_pCallBack->OnPatReceived(service_id, pmt_pid);
        }
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug("PatParser: section parsing error");
    }
    else
    {
      m_mSeenSections[key] = true;
    }
  }
  catch(...)
  {
    LogDebug("PatParser: unhandled exception in OnNewSection()");
  }
}
