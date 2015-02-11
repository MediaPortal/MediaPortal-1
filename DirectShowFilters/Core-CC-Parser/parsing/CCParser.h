#ifndef __CCPARSER_H
#define __CCPARSER_H

#include "CcParseH264.h"

/////////////////////////////////////////////////////////////////////////////
// CCcParser
//

inline int bit( BYTE b, int nBit )
{
	return ( b & (1 << nBit )) >> nBit;
}

struct CCWORD
{
private:
	WORD m_w;

public:
	static bool IsValid( BYTE b )
	{
		return( 0 != ( bit(b,0)+bit(b,1)+bit(b,2)+bit(b,3)+bit(b,4)+bit(b,5)+bit(b,6)+bit(b,7))%2 );
	}

	CCWORD() : m_w( 0 ){}
	bool IsEmpty() const { return m_w == 0; }

	CCWORD( WORD w ) : m_w( w & 0x7F7F ) {}
	operator WORD() const 
	{ 
		WORD wRes = m_w;
		BYTE& b1 = reinterpret_cast<BYTE*>( &wRes )[0];
		if( !CCWORD::IsValid( b1 ))
		{
			ASSERT( (b1 & 0x80) == 0 );
			b1 |= 0x80;
			ASSERT( CCWORD::IsValid( b1 ));
		}

		BYTE& b2 = reinterpret_cast<BYTE*>( &wRes )[1];
		if( !CCWORD::IsValid( b2 ))
		{
			ASSERT( (b2 & 0x80) == 0 );
			b2 |= 0x80;
			ASSERT( CCWORD::IsValid( b2 ));
		}

		return wRes; 
	}

	const CCWORD& operator= (WORD w) { m_w = w & 0x7F7F; return *this; }

	CCWORD( const BYTE* pB ) : m_w( *reinterpret_cast<const UNALIGNED WORD*>(pB) & 0x7F7F ){}
	//CCWORD( BYTE b1, BYTE b2 ) : m_w( MAKEWORD( b1 & 0x7F, b2 & 0x7F )){}
	CCWORD( const CCWORD& cc ) : m_w( cc.m_w ){}

	BYTE b1() const { return LOBYTE(m_w); }
	BYTE b2() const { return HIBYTE(m_w); }
	
	bool IsStartXDS() const { return b1() >= 0x01 && b1() < 0x0F; } 
	bool IsEndXDS() const { return b1() == 0x0F; } 
	bool IsCode() const { return b1() >= 0x10 && b1() <= 0x1F; }
	
	bool IsExtChar() const { return ( b1() & 0xF6 )== 0x12/*0x12, 0x12,3 0x1A, 0x1B*/ && ( b2() & 0xE0 ) == 0x20 /* 20..3f*/; } ;
	bool IsText() const { return 0 != ( b1() & 0x60 ) || IsExtChar(); }

	void GetString( TCHAR szString[3]) const;

};

class CCcParser
{
protected:
	struct CCWORDSET
	{
		enum{ cSavedFields = 2 };
		
		CCWORD m_ccField1[cSavedFields];
		CCWORD m_ccField2[cSavedFields];
		
		REFERENCE_TIME m_timeStamp;

		bool IsEmpty() const { return m_ccField1[0].IsEmpty() && m_ccField2[0].IsEmpty();}
		void Empty() { m_ccField1[0] = CCWORD(); m_ccField2[0] = CCWORD(); m_timeStamp=0; ASSERT( IsEmpty());}
	};

public:
	CCcParser();
  virtual ~CCcParser();

	enum
	{
		fieldOdd,
		fieldEven
	};

	enum
	{
		cctypeNone,
		cctypeEchostar,
		cctypeATSC_A53
	};

// Implementation
public:
	void Reset();
	virtual bool OnDataArrivalMPEG( const BYTE* pData, UINT cbData, REFERENCE_TIME sourceTime );
	virtual bool OnDataArrivalAVC1( const BYTE* pData, UINT cbData, DWORD m_dwFlags, REFERENCE_TIME sourceTime );

protected:
	virtual bool OnCc( int nType, int iField, CCWORD ccField ) = 0;
	virtual bool OnProgress( int nPercent ) { return true; }

	virtual const BYTE* OnUserData( bool bPorI, const BYTE* p, const BYTE* pStop, REFERENCE_TIME sourceTime, bool bIsSubtypeAVC1 ); // All return the last recognized byte

	virtual bool OnCCSet( bool bPorI, int nType, const CCWORDSET& ccSet, bool bIsSubtypeAVC1 );
	virtual bool SendCCSet( int nType, const CCWORDSET& ccSet );

	const BYTE* SkipUserData( const BYTE* p, const BYTE* pStop );

private:
	inline const BYTE* Parse_DVD     ( bool bPorI, const BYTE* p, const BYTE* pStop );
	inline const BYTE* Parse_ATSC_A53( bool bPorI, const BYTE* p, const BYTE* pStop, REFERENCE_TIME sourceTime, bool bIsSubtypeAVC1 );
	inline const BYTE* Parse_Echostar( bool bPorI, const BYTE* p, const BYTE* pStop );
	const BYTE* Parse_Unknown        ( bool bPorI, const BYTE* p, const BYTE* pStop );

	CCWORDSET m_ccsetLastPorI;
	
	CCWORDSET m_ccsetH264[20]; //Sized to allow for maximum of 20 H.264 b-frames + ref frames
	int m_ccsetH264WrIdx = 0;
	
  CcParseH264 *m_CcParserH264;
};

class CCcTextParser
{
public:
	struct CCPROFILE
	{
		enum 
		{ 
			modeNone, modeCC, modeText, modeXDS,
			cModes,
			cMaxChannels = 4
		};

		UINT m_idChannel; // 0..cMaxChannels-1
		UINT m_idMode;

		bool IsFieldOK( int idField ) const
		{ 
			return m_idMode == modeXDS 
				   ? ( idField == CCcParser::fieldEven )
				   : ( idField == CCcParser::fieldOdd ) == ( m_idChannel < 2 );
		}

		void UpdateChannelFlag( BYTE b1 )
		{
			m_idChannel &= ~1;
			m_idChannel |= ( b1>>3 )&1 ;
		}

		bool operator==( const CCPROFILE& c ) const
		{
			return m_idChannel == c.m_idChannel && 
				   m_idMode    == c.m_idMode;
		}
	};

	void Reset( UINT idChannel, UINT idMode = CCPROFILE::modeCC );

	virtual bool OnText( CCWORD ccText ){ return true; }
	virtual bool OnCode( CCWORD ccText ){ return true; }

protected:
	CCPROFILE m_ccproCurrent;
	CCPROFILE m_ccproLookedFor;

	CCWORD m_ccLastCode;

	virtual bool OnCc( int nType, int iField, CCWORD ccField );
};

#endif ndef __CCPARSER_H
