// CriticalSection.cpp: implementation of the CCriticalSection class.
//
//////////////////////////////////////////////////////////////////////

#include "CriticalSection.h"

using namespace Mediaportal;

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CCriticalSection::CCriticalSection()
{
  ::InitializeCriticalSection( &m_cs );
}

CCriticalSection::~CCriticalSection()
{
  ::DeleteCriticalSection( &m_cs );
}

CCriticalSection::operator LPCRITICAL_SECTION()
{
  return &m_cs;
}
