// EnterCriticalSection.h: interface for the CEnterCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_ENTERCRITICALSECTION_H__AFDC94FA_28D1_47FA_BB4F_F2F852C1B660__INCLUDED_)
#define AFX_ENTERCRITICALSECTION_H__AFDC94FA_28D1_47FA_BB4F_F2F852C1B660__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#if defined( _MSC_VER )
#pragma message( "Including " __FILE__ ) 
#endif // defined( _MSC_VER )

//#ifndef _WINDOWS_
//// Minimum system required: Windows NT 4.0
//#define _WIN32_WINNT  0x0400 
//#define WINVER        0x0400
//#include <Windows.h>
//#endif /* _WINDOWS_ */

#if !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)
#include "CriticalSection.h"
#endif // !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)


namespace Mediaportal
{
  // Use to enter a critical section
  // Can only be used in blocking fashion
  // Critical section ends with object scope
  class CEnterCriticalSection
  {
  public:
    // Constructor
    // Obtain ownership of the cricital section
	  CEnterCriticalSection(CCriticalSection& cs);
    // Constructor
    // Obtain ownership of the cricital section
    // The const attribute will be removed with the const_cast operator
    // This enables the use of critical sections in const members
    CEnterCriticalSection(const CCriticalSection& cs);
    // Destructor
    // Leaves the critical section
	  virtual ~CEnterCriticalSection();

    // Test critical section ownership
    // Returns true if ownership was granted
    bool IsOwner() const;
    // Obtain ownership (may block)
    // Returns true when ownership was granted
    bool Enter();
    // Leave the critical section
    void Leave();

  private:
    CEnterCriticalSection(const CEnterCriticalSection& src);
    CEnterCriticalSection& operator=(const CEnterCriticalSection& src);

    // Reference to critical section object
    CCriticalSection&   m_cs;
    // Ownership flag
    bool                m_bIsOwner;
  };
}

#endif // !defined(AFX_ENTERCRITICALSECTION_H__AFDC94FA_28D1_47FA_BB4F_F2F852C1B660__INCLUDED_)
