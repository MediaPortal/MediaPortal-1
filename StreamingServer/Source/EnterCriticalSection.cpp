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
