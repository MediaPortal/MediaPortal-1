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
#include "..\shared\EnterCriticalSection.h"

using namespace MediaPortal;


CEnterCriticalSection::CEnterCriticalSection(CCriticalSection& cs)
  : m_cs(cs), m_isOwner(false)
{
  Enter();
}

CEnterCriticalSection::CEnterCriticalSection(const CCriticalSection& cs)
  : m_cs(const_cast<CCriticalSection&>(cs)), m_isOwner(false)
{
  Enter();
}

CEnterCriticalSection::~CEnterCriticalSection()
{
  Leave();
}

bool CEnterCriticalSection::IsOwner() const
{
  return m_isOwner;
}

bool CEnterCriticalSection::Enter()
{
  if (m_isOwner)
  {
    return true;
  }

  // blocking call
  ::EnterCriticalSection(m_cs);
  m_isOwner = true;
  return m_isOwner;
}

void CEnterCriticalSection::Leave()
{
  if (!m_isOwner)
  {
    return;
  }

  ::LeaveCriticalSection(m_cs);
  m_isOwner = false;
}