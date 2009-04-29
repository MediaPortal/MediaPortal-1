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
// CriticalSection.h: interface for the CCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)
#define AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef _WINDOWS_
#include <Windows.h>
#endif /* _WINDOWS_ */


namespace Mediaportal
{
  // Wrapper for critical section struct
  class CCriticalSection
  {
  public:
    // Constructor
    // Initializes the critical section struct
	  CCriticalSection();
    // Destructor
	  virtual ~CCriticalSection();

    // Conversion operator
    operator LPCRITICAL_SECTION();

  private:
    // Copy constructor is disabled
    CCriticalSection(const CCriticalSection& src);
    // operator= is disabled
    CCriticalSection& operator=(const CCriticalSection& src);

    CRITICAL_SECTION    m_cs;
  };
}

#endif // !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)
