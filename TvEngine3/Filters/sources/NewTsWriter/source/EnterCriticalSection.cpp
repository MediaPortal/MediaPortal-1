/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
// EnterCriticalSection.cpp: implementation of the CEnterCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#include "EnterCriticalSection.h"

using namespace Mediaportal;

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CEnterCriticalSection::CEnterCriticalSection(CCriticalSection& cs)
: m_cs( cs )
, m_bIsOwner( false )
{
  Enter();
}

CEnterCriticalSection::CEnterCriticalSection(const CCriticalSection& cs)
: m_cs( const_cast<CCriticalSection&>(cs) )
, m_bIsOwner( false )
{
  Enter();
}

CEnterCriticalSection::~CEnterCriticalSection()
{
  Leave();
}

bool CEnterCriticalSection::IsOwner() const
{
  return m_bIsOwner;
}

bool CEnterCriticalSection::Enter()
{
  // Test if we already own the critical section
  if ( true == m_bIsOwner )
  {
    return true;
  }

  // Blocking call
  ::EnterCriticalSection( m_cs );
  m_bIsOwner = true;

  return m_bIsOwner;
}

void CEnterCriticalSection::Leave()
{
  if ( false == m_bIsOwner )
  {
    return;
  }

  ::LeaveCriticalSection( m_cs );
  m_bIsOwner = false;
}
