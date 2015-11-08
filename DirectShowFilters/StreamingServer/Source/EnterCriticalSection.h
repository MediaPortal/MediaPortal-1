// EnterCriticalSection.h: interface for the CEnterCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#pragma once
#include "CriticalSection.h"

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