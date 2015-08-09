// CriticalSection.h: interface for the CCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#pragma once
#include <Windows.h>


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