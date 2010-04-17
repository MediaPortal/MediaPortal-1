//////////////////////////////////////////////////////////////////////// 
// Lock.h: interface for the Lock class.
//


#if !defined(Lock_H)
#define Lock_H

#include <string>
using namespace std;

/////////////////////////////////////////////////
// Lock

class Lock
{
public:
	HANDLE		m_hMutex;			// used to lock/unlock object access
	bool		m_attached;			// shows attached to existing lock


	//////////////////////////////////////////////////////////////////////
	// Construction/Destruction
	//////////////////////////////////////////////////////////////////////

	Lock( string & lockName ) :
		m_hMutex(NULL),
		m_attached(false)
	{
		createLock(lockName);
	}

	Lock() :
		m_hMutex(NULL),
		m_attached(false)
	{
		createLock();
	}

	Lock( HANDLE hMutex ) :
		m_hMutex(NULL),
		m_attached(false)
	{
		if ( hMutex != NULL )
		{
			m_hMutex  = hMutex;
			m_attached = true;
		}
	}


	virtual ~Lock()
	{
		destroyLock();
	}


	/////////////////////////////////////////////////////////////
	// access object fxns

	bool lock	()
	{
		if ( m_hMutex == NULL )
			return false;

		WaitForSingleObject( m_hMutex, INFINITE );

		return true;
	}


	void unlock ()
	{
		ReleaseMutex(m_hMutex);
	}

	bool createLock ( string & lockName )
	{
		// return object mutex
		m_hMutex = ::CreateMutex( NULL, false, lockName.c_str() );
		if ( m_hMutex == 0 )
			return false;

		return true;
	}

	bool createLock ()
	{
		// return object mutex
		m_hMutex = ::CreateMutex( NULL, false, 0 );
		if ( m_hMutex == 0 )
			return false;

		return true;
	}

	static bool createLock ( HANDLE & hMutex )
	{
		// return object mutex
		hMutex = ::CreateMutex( NULL, false, 0 );
		if ( hMutex == 0 )
			return false;

		return true;
	}


	void destroyLock ()
	{
		if ( !m_attached )
		{
			if ( m_hMutex != NULL )
			{
				::CloseHandle( m_hMutex );
			}
		}

		m_attached = false;
		m_hMutex   = NULL;
	}

};












#endif
