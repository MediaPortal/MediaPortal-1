#define _WIN32_WINNT 0x403
#ifdef _DEBUG
	#define ASSERT(_x_) do{ if (!(_x_)) DbgAssert(TEXT(#_x_),TEXT(__FILE__),__LINE__); } while(0)
	#define VERIFY(_x_) ASSERT(_x_)
#else
	#define ASSERT(_x_) ((void)0)
	#define VERIFY(_x_) ((void)(_x_))
#endif

#define RETURN_FAILED(exp) do{ HRESULT _highly_unlikely_named_hr_var_ = (exp); if( FAILED( _highly_unlikely_named_hr_var_ )) return _highly_unlikely_named_hr_var_; }while(0)

#ifndef _countof
#define _countof(array) (sizeof(array)/sizeof(array[0]))
#endif

#include <AtlColl.h>

#define __AFX_H__
#include <streams.h>
#undef __AFX_H__

#include <tchar.h>

/////////////////////////////////////////////////////////////////////////////
// auto_pif
//

template<class Interface = IUnknown>
struct auto_pif
{
	auto_pif( const auto_pif& apif )
	:	m_pif( apif.m_pif )
	{
		if( m_pif )
			m_pif->AddRef();
	}

	auto_pif( Interface* pif = NULL, bool bAlreadyAddRefed = false )
	:	m_pif( pif )
	{
		if( m_pif && !bAlreadyAddRefed )
			m_pif->AddRef();
	}

	~auto_pif(){ Release(); }
	
	void Release()
	{
		if( m_pif )
		{
			m_pif->Release();
			m_pif = NULL;
		}
	}

	HRESULT AcceptInterface( IUnknown* pif, REFIID iid )
	{
		if( !pif )
			return E_POINTER;
		
		Interface* pifNative;
		HRESULT hRes = pif->QueryInterface( iid, (void**)&pifNative );
		
		if( SUCCEEDED( hRes ))
			*this = pifNative;
		
		return hRes;
	}

	Interface** AcceptHere()
	{
		Release();

		return &m_pif;
	}

	const auto_pif& operator=( const auto_pif& apif )
		{ return operator=( apif.m_pif ); }

	const auto_pif& operator=( Interface* pif )
	{
		if( pif )
			pif->AddRef();

		Release();

		m_pif = pif;

		return *this;
	}
	
	operator Interface*() { return m_pif; }
	Interface* operator->(){ return m_pif; }
	const Interface* operator->() const { return m_pif; }

	operator auto_pif<IUnknown>&() 
	{ 
		ASSERT( static_cast<IUnknown*>( m_pif ) || !m_pif );
		
		return *(auto_pif<IUnknown>*)this; 
	}

	operator bool() const { return m_pif != NULL ;}
	bool operator ==( const Interface* pif ) const { return m_pif == pif; }

private:
	Interface* m_pif;	
};

