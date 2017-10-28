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
#include "..\shared\SectionDispatcher.h"


CSectionDispatcher::CSectionDispatcher()
{
  m_nextProcessor = 0;
}

CSectionDispatcher::~CSectionDispatcher()
{
  for (unsigned char p = 0; p < PROCESSOR_COUNT; p++)
  {
    LogDebug(L"section dispatcher: processor, ID = %hhu, maximum queue length = %lu",
              p, m_processors[p].MaximumQueueLength());
  }
}

void CSectionDispatcher::DequeueSections(ISectionCallback& sectionDelegate)
{
  for (unsigned char p = 0; p < PROCESSOR_COUNT; p++)
  {
    m_processors[p].DequeueSections(sectionDelegate);
  }
}

void CSectionDispatcher::EnqueueSection(unsigned short pid,
                                        unsigned char tableId,
                                        const CSection& section,
                                        ISectionCallback& sectionDelegate)
{
  CSection* s = new CSection(section);
  if (s == NULL)
  {
    LogDebug(L"section dispatcher: failed to allocate section copy for dispatch, PID = %hu, table ID = 0x%hhx",
              pid, tableId);
    return;
  }

  CQueuedSection* qs = new CQueuedSection(pid, tableId, s, sectionDelegate);
  if (qs == NULL)
  {
    LogDebug(L"section dispatcher: failed to allocate queued section, PID = %hu, table ID = 0x%hhx",
              pid, tableId);
    delete s;
    return;
  }

  CEnterCriticalSection lock(m_section);
  m_processors[m_nextProcessor++].EnqueueSection(qs);
  m_nextProcessor = m_nextProcessor % PROCESSOR_COUNT;
}