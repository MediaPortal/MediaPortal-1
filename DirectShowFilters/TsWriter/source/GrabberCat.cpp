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
#include "GrabberCat.h"
#include <cstddef>      // NULL
#include <cstring>      // memcpy()
#include "..\..\shared\EnterCriticalSection.h"

using namespace std;


#define VERSION_NOT_SET 0xff


extern void LogDebug(const wchar_t* fmt, ...);

CGrabberCat::CGrabberCat(ISectionDispatcher* sectionDispatcher)
  : CSectionDecoder(sectionDispatcher)
{
  m_isReady = false;
  m_version = VERSION_NOT_SET;

  SetPid(PID_CAT);
  SetCallBack(NULL);
}

CGrabberCat::~CGrabberCat()
{
  SetCallBack(NULL);
}

void CGrabberCat::Reset()
{
  LogDebug(L"CAT: reset");
  CEnterCriticalSection lock(m_section);
  CSectionDecoder::Reset();
  m_isReady = false;
  m_version = VERSION_NOT_SET;
  m_catSection.Reset();
  LogDebug(L"CAT: reset done");
}

void CGrabberCat::SetCallBack(ICallBackCat* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CGrabberCat::OnNewSection(const CSection& section)
{
  try
  {
    if (
      section.TableId != TABLE_ID_CAT ||
      !section.SectionSyntaxIndicator ||
      section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.SectionNumber != 0 || section.LastSectionNumber != 0)
    {
      LogDebug(L"CAT: unsupported multi-section table, version number = %hhu, section number = %hhu, last section number = %hhu",
                section.VersionNumber, section.SectionNumber,
                section.LastSectionNumber);
      return;
    }
    if (section.SectionLength > 1021 || section.SectionLength < 9)
    {
      LogDebug(L"CAT: invalid section, length = %hu", section.SectionLength);
      return;
    }

    CEnterCriticalSection lock(m_section);
    if (m_version == section.VersionNumber)
    {
      return;
    }

    //LogDebug(L"CAT: version number = %hhu, section length = %hu",
    //          section.VersionNumber, section.SectionLength);

    m_isReady = true;
    if (m_version != VERSION_NOT_SET)
    {
      LogDebug(L"CAT: changed, version number = %hhu, prev. version number = %hhu",
                section.VersionNumber, m_version);
      if (m_callBack != NULL)
      {
        m_callBack->OnCatChanged(section.Data, section.SectionLength + 3);    // + 3 for table ID and section length bytes
      }
    }
    else
    {
      LogDebug(L"CAT: received, version number = %hhu", section.VersionNumber);
      if (m_callBack != NULL)
      {
        m_callBack->OnCatReceived(section.Data, section.SectionLength + 3);   // + 3 for table ID and section length bytes
      }
    }
    m_version = section.VersionNumber;
    m_catSection = section;
  }
  catch (...)
  {
    LogDebug(L"CAT: unhandled exception in OnNewSection()");
  }
}

bool CGrabberCat::IsReady() const
{
  return m_isReady;
}

bool CGrabberCat::GetTable(unsigned char* table, unsigned short& tableBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  if (!m_isReady)
  {
    LogDebug(L"CAT: not yet received");
    tableBufferSize = 0;
    return false;
  }
  unsigned short requiredBufferSize = m_catSection.SectionLength + 3;   // + 3 for table ID and section length bytes
  if (table == NULL || tableBufferSize < requiredBufferSize)
  {
    LogDebug(L"CAT: insufficient buffer size, required = %d, actual = %hu",
              requiredBufferSize, tableBufferSize);
    return false;
  }
  memcpy(table, m_catSection.Data, requiredBufferSize);
  tableBufferSize = requiredBufferSize;
  return true;
}