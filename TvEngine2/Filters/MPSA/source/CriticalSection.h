// CriticalSection.h: interface for the CCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_)
#define AFX_CRITICALSECTION_H__3B3A15BD_92D5_4044_8D69_5E1B8F15F369__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#if defined( _MSC_VER )
#pragma message( "Including " __FILE__ ) 
#endif // defined( _MSC_VER )

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
