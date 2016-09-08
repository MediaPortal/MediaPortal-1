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
// EnterCriticalSection.h: interface for the CEnterCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_ENTERCRITICALSECTION_H__AFDC94FA_28D1_47FA_BB4F_F2F852C1B660__INCLUDED_)
#define AFX_ENTERCRITICALSECTION_H__AFDC94FA_28D1_47FA_BB4F_F2F852C1B660__INCLUDED_

#pragma once

#if !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)
#include "..\shared\CriticalSection.h"
#endif // !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)


namespace MediaPortal
{
  // Use to enter a critical section.
  // Can only be used in blocking fashion.
  // Critical section ends with object scope.
  class CEnterCriticalSection
  {
    public:
      CEnterCriticalSection(CCriticalSection& cs);
      CEnterCriticalSection(const CCriticalSection& cs);
      virtual ~CEnterCriticalSection();

      bool IsOwner() const;
      bool Enter();
      void Leave();

    private:
      CEnterCriticalSection(const CEnterCriticalSection& src);
      CEnterCriticalSection& operator=(const CEnterCriticalSection& src);

      CCriticalSection& m_cs;
      bool m_isOwner;
    };
}

#endif // !defined(AFX_ENTERCRITICALSECTION_H__AFDC94FA_28D1_47FA_BB4F_F2F852C1B660__INCLUDED_)