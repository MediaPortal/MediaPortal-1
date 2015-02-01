#include "stdafx.h"

#include "CCParser.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CCcParser

inline TCHAR UpdateExcepionChar( char c )
{
	switch( c )
	{
		case 0x2A: return _T('Á');
		case 0x5C: return _T('É');
		case 0x5E: return _T('Í');
		case 0x5F: return _T('Ó');
		case 0x60: return _T('Ú');
		case 0x7B: return _T('Ç');
		case 0x7C: return _T('÷');
		case 0x7D: return _T('Ñ');
		case 0x7E: return _T('Ñ');
	};

	return (TCHAR)c;
}

void CCWORD::GetString( TCHAR szString[3] ) const
{
	if( !IsText())
	{
		szString[0] = 0; 
		
		return;
	}
	
	szString[1] = 0;

	if( IsExtChar())
	{
		switch( b1() & 0x13 )
		{
			case 0x12:
			{
				switch( b2())
				{
					case 0x20: szString[0] = _T('Á'); return;
					case 0x21: szString[0] = _T('É'); return;
					case 0x22: szString[0] = _T('Ó'); return;
					case 0x23: szString[0] = _T('Ú'); return;
					case 0x24: szString[0] = _T('Ü'); return;
					case 0x25: szString[0] = _T('ü'); return;
					case 0x26: szString[0] = _T('\''); return;
					case 0x27: szString[0] = _T('¡'); return;
					case 0x28: szString[0] = _T('*'); return; // actual asterisk
					case 0x29: szString[0] = _T('\''); return;
					case 0x2a: szString[0] = _T('-'); return;
					case 0x2b: szString[0] = _T('©'); return;
					case 0x2c: szString[0] = _T('*'); return; // {sm}
					case 0x2d: szString[0] = _T('·'); return;
					case 0x2e: szString[0] = _T('"'); return;
					case 0x2f: szString[0] = _T('"'); return;
					case 0x30: szString[0] = _T('À'); return;
					case 0x31: szString[0] = _T('Â'); return;
					case 0x32: szString[0] = _T('Ç'); return;
					case 0x33: szString[0] = _T('È'); return;
					case 0x34: szString[0] = _T('Ê'); return;
					case 0x35: szString[0] = _T('Ë'); return;
					case 0x36: szString[0] = _T('ë'); return;
					case 0x37: szString[0] = _T('Î'); return;
					case 0x38: szString[0] = _T('Ï'); return;
					case 0x39: szString[0] = _T('ï'); return;
					case 0x3a: szString[0] = _T('Ô'); return;
					case 0x3b: szString[0] = _T('Ù'); return;
					case 0x3c: szString[0] = _T('ù'); return;
					case 0x3d: szString[0] = _T('Û'); return;
					case 0x3e: szString[0] = _T('«'); return;
					case 0x3f: szString[0] = _T('»'); return;
				};
			}
			break;

			case 0x13:
			{
				switch( b2())
				{
					case 0x20: szString[0] = _T('Ã'); return;
					case 0x21: szString[0] = _T('ã'); return;
					case 0x22: szString[0] = _T('Í'); return;
					case 0x23: szString[0] = _T('Ì'); return;
					case 0x24: szString[0] = _T('ì'); return;
					case 0x25: szString[0] = _T('Ò'); return;
					case 0x26: szString[0] = _T('ò'); return;
					case 0x27: szString[0] = _T('Õ'); return;
					case 0x28: szString[0] = _T('õ'); return;
					case 0x29: szString[0] = _T('{'); return;
					case 0x2a: szString[0] = _T('}'); return;
					case 0x2b: szString[0] = _T('\\'); return;
					case 0x2c: szString[0] = _T('^'); return;
					case 0x2d: szString[0] = _T('_'); return;
					case 0x2e: szString[0] = _T('|'); return;
					case 0x2f: szString[0] = _T('~'); return;
					case 0x30: szString[0] = _T('Ä'); return;
					case 0x31: szString[0] = _T('ä'); return;
					case 0x32: szString[0] = _T('Ö'); return;
					case 0x33: szString[0] = _T('ö'); return;
					case 0x34: szString[0] = _T('ß'); return;
					case 0x35: szString[0] = _T('¥'); return;
					case 0x36: szString[0] = _T('¤'); return;
					case 0x37: szString[0] = _T('|'); return;
					case 0x38: szString[0] = _T('Å'); return;
					case 0x39: szString[0] = _T('å'); return;
					case 0x3a: szString[0] = _T('Ø'); return;
					case 0x3b: szString[0] = _T('ø'); return;
					case 0x3c: szString[0] = _T('*'); return; // {ul}
					case 0x3d: szString[0] = _T('*'); return; // {ur}
					case 0x3e: szString[0] = _T('*'); return; // {ll}
					case 0x3f: szString[0] = _T('*'); return; // {lr}
				};
			}
			break;

			default:
				ASSERT(0);
		}
	}
	

	szString[0] = UpdateExcepionChar( reinterpret_cast<const char*>( &m_w )[0]& 0x7F );
	szString[1] = UpdateExcepionChar( reinterpret_cast<const char*>( &m_w )[1]& 0x7F );
	szString[2] = 0;
}

/////////////////////////////////////////////////////////////////////////////
// CCcParser

CCcParser::CCcParser()
{
}

CCcParser::~CCcParser()
{
}

void CCcParser::Reset()
{
	m_ccsetLastPorI.Empty();
//	m_picture_coding_type = typeNone;
}

enum
{ 
	mask_temporal_reference  = 0xC0FF, // 11111111 11000000
	mask_picture_coding_type = 0x3800, // 00000000 00111000
	typeNone				 = 0,
	typeI 					 = 0x0800, // 00000000 00001000	
	typeP 					 = 0x1000, // 00000000 00010000	
	typeB 					 = 0x1800, // 00000000 00011000	
};

inline bool RecognizePictureHeader( WORD& picture_coding_type, const BYTE* p, int iRollBack = 0 )
{
	ASSERT( iRollBack <= 4 );
	
	if(( *reinterpret_cast<const UNALIGNED DWORD*>(p) << 8*iRollBack ) != ( (DWORD)0x00010000 & ( (DWORD)0xFFFFFFFF << 8*iRollBack )))
		return false;

	WORD test_picture_coding_type = 
		(WORD)( *reinterpret_cast<const UNALIGNED WORD*>(p+4-iRollBack) & mask_picture_coding_type ); 

	if( test_picture_coding_type == typeI ||
		test_picture_coding_type == typeP ||
		test_picture_coding_type == typeB
	  )
	{
		picture_coding_type = test_picture_coding_type;

		return true;
	}

	return false;
}

bool CCcParser::OnDataArrivalMPEG( const BYTE* pData, UINT cbData )
{
	const BYTE* pStop = pData + cbData;

	ASSERT( 4 == sizeof(DWORD));
	ASSERT( 2 == sizeof(WORD));

	WORD picture_coding_type = typeNone;
	WORD temporal_reference = 0;

	for( const BYTE* p = pData; p < pStop - sizeof(DWORD); ++p )
	{
		if(( *reinterpret_cast<const UNALIGNED DWORD*>(p) & 0x00FFFFFF ) == 0x00010000 ) // 00000100 : start code prefix
		{
			switch( p[3])
			{
				case 0x00:   // picture_start_code
				{
					VERIFY( RecognizePictureHeader ( picture_coding_type, p ));
					temporal_reference = MAKEWORD( p[5] & 0xC0, p[4] ) >> 6;
				}
				break;

				case 0xb2:
				{
					if( picture_coding_type == typeNone )
					{
						// Sometimes pData starts a byte or so into the picture header.
						//	That's the "start code prefix" is not recognized.
						
						for( int iRollBack = 1; iRollBack <= 4; iRollBack++ )
							if( RecognizePictureHeader( picture_coding_type, pData, iRollBack ))
								break; // for
					}
					
					bool bPorI = ( picture_coding_type == typeP || picture_coding_type == typeI );
					ASSERT( bPorI || picture_coding_type == typeB );
					
					p = OnUserData( bPorI, p + 4, pStop );
					if( !p )
						return false;

					picture_coding_type = typeNone;
				}
				break;
			}
		}
		else
		{
			enum{ cReportInterval = 1000 };
			if( (p - pData) % cReportInterval == 0 )
			{
				if( !OnProgress( (p - pData) * 100 / cbData ))
					return false;
			}

			continue;
		}
	}

	return true;
}

bool CCcParser::OnDataArrivalAVC1( const BYTE* pData, UINT cbData )
{
	const BYTE* pStop = pData + cbData;

	ASSERT( 4 == sizeof(DWORD));
	ASSERT( 2 == sizeof(WORD));

	WORD picture_coding_type = typeNone;
	WORD temporal_reference = 0;

	for( const BYTE* p = pData; p < pStop - sizeof(DWORD); ++p )
	{
		if(( *reinterpret_cast<const UNALIGNED DWORD*>(p) & 0x00FFFFFF ) == 0x00010000 ) // 00000100 : start code prefix
		{
			switch( p[3])
			{
				case 0x00:   // picture_start_code
				{
					VERIFY( RecognizePictureHeader ( picture_coding_type, p ));
					temporal_reference = MAKEWORD( p[5] & 0xC0, p[4] ) >> 6;
				}
				break;

				case 0xb2:
				{
					if( picture_coding_type == typeNone )
					{
						// Sometimes pData starts a byte or so into the picture header.
						//	That's the "start code prefix" is not recognized.
						
						for( int iRollBack = 1; iRollBack <= 4; iRollBack++ )
							if( RecognizePictureHeader( picture_coding_type, pData, iRollBack ))
								break; // for
					}
					
					bool bPorI = ( picture_coding_type == typeP || picture_coding_type == typeI );
					ASSERT( bPorI || picture_coding_type == typeB );
					
					p = OnUserData( bPorI, p + 4, pStop );
					if( !p )
						return false;

					picture_coding_type = typeNone;
				}
				break;
			}
		}
		else
		{
			enum{ cReportInterval = 1000 };
			if( (p - pData) % cReportInterval == 0 )
			{
				if( !OnProgress( (p - pData) * 100 / cbData ))
					return false;
			}

			continue;
		}
	}

	return true;
}


const BYTE* CCcParser::OnUserData( bool bPorI, const BYTE* p, const BYTE* pStop )
{
	if( *reinterpret_cast<const UNALIGNED DWORD*>(p) == 0x34394147 ) // 47413934
		return Parse_ATSC_A53( bPorI, p + 4, pStop );
	
	if( *reinterpret_cast<const UNALIGNED WORD*>(p) == 0x0205 ) // 0502
		return Parse_Echostar( bPorI, p, pStop );
	
	return Parse_Unknown( bPorI, p, pStop );
}

inline const BYTE* CCcParser::Parse_ATSC_A53( bool bPorI, const BYTE* p, const BYTE* pStop )
{
	CCWORDSET cc;
	int iField1 = 0;
	int iField2 = 0;
	
	enum{ typeCCData = 0x03, typeBarData = 0x06 };
	if( *p == typeCCData ) //
	{
		p++;
		
		enum{ process_cc_data_flag = 0x40, mask_cc_count = 0x1F };
		if( p < pStop && ( p[0] & process_cc_data_flag ))
		{
			const int cCC_count = p[0] & mask_cc_count;
			if( cCC_count > 0 )
			{
				p++; // flags
				p++; // reserved 0xff

				enum{ cbToken = 3 };
				
				for( int iCC = 0; iCC < cCC_count && p+cbToken < pStop; iCC++ )
				{
					enum
					{ 
						flag_cc_valid = 0x04, mask_cc_type = 0x03, 
						typeField1 = 0x00, typeField2 = 0x01,  
					};

					if( p[0] & flag_cc_valid )
					{
						switch( p[0] & mask_cc_type )
						{
							case typeField1:
							{
								if( iField1 < _countof( cc.m_ccField1 ))
								{
									cc.m_ccField1[iField1] = CCWORD( &p[1] );
									iField1++;
								}
								else
									ASSERT(0);
							}
							break;

							case typeField2:
							{
								if( iField2 < _countof( cc.m_ccField2 ))
								{
									cc.m_ccField2[iField2] = CCWORD( &p[1] );
									iField2++;
								}
								else
									ASSERT(0);
							}
							break;
						}
					}

					p += cbToken;
				}
			}
		}
	}
	
	if( !OnCCSet( bPorI, cctypeATSC_A53, cc ))
		return false; 

	return SkipUserData( p, pStop ); //TODO
}

inline const BYTE* CCcParser::Parse_Echostar( bool bPorI, const BYTE* pData, const BYTE* pStop )
{
	CCWORDSET cc;
	
	//bPorI = false;
	bool bError = false;
	
	enum{ cTokenHeader = 2 };
	for( const BYTE* p = pData; 
	     !bError && p < pStop - cTokenHeader - p[0]; 
		 p += ( cTokenHeader + p[0])
	   )
	{
		switch( p[0] )
		{
			case 0x05:
			{
				if( p[1] == 0x04 )
				{
					ASSERT( bPorI );
					//bPorI = true;
					
				}
				else if( p[1] == 0x02 )
					; // Do nothing
				else
					bError = true;

			}
			break;
			
			case 0x02:
			{
				if( p[1] == 0x09 )
				{
					cc.m_ccField1[0] = CCWORD( &p[2]);
				}
				else if( p[1] == 0x0a )
				{
					cc.m_ccField2[0] = CCWORD( &p[2]);
				}
				else
					bError = true;
			}
			break;

			case 0x1b:
			{
				if( p[1] == 0x09 )
				{
					cc.m_ccField1[0] = CCWORD( &p[2]);
				}
				else if( p[1] == 0x0a )
				{
					cc.m_ccField2[0] = CCWORD( &p[2]);
				}
				else
					bError = true;
			}
			break;
			
			case 0x04:
			{
				if( p[1] == 0x09 )
				{
					cc.m_ccField1[0] = CCWORD( &p[2]);
					cc.m_ccField1[1] = CCWORD( &p[4]);
				}
				else if( p[1] == 0x0a )
				{
					cc.m_ccField2[0] = CCWORD( &p[2]);
					cc.m_ccField2[1] = CCWORD( &p[4]);
				}
				else
					bError = true;
			}
			break;
			
			case 0x00:
			{
				if( !OnCCSet( bPorI, cctypeEchostar, cc ))
					return NULL;

				return SkipUserData( p, pStop ); 
			};
			break;


			default:
				bError = true;
		}
	}
	
	return Parse_Unknown( bPorI, pData, pStop );
}

const BYTE* CCcParser::Parse_Unknown ( bool bPorI, const BYTE* p, const BYTE* pStop )
{
	return SkipUserData( p, pStop ); //TODO
}

const BYTE* CCcParser::SkipUserData( const BYTE* p, const BYTE* pStop )
{
	while( p < pStop - 3 && !( p[0] == 0x00 && p[1] == 0x00 && p[2] == 0x01 ))
	{
		p++;
	}

	return p + 2;
}

bool CCcParser::OnCCSet( bool bPorI, int nType, const CCWORDSET& cc )
{
	if( bPorI )
	{
		if( !m_ccsetLastPorI.IsEmpty())
		{
			if( !SendCCSet( nType, m_ccsetLastPorI ))
				return NULL;
		}

		m_ccsetLastPorI = cc;

		return true;
	}

	return SendCCSet( nType, cc );
}

bool CCcParser::SendCCSet( int nType, const CCWORDSET& cc )
{
	if( !cc.m_ccField1[0].IsEmpty())
	{
		if( !OnCc( nType, fieldOdd, cc.m_ccField1[0] ))
			return false;

		if( !cc.m_ccField1[1].IsEmpty())
		{
			if( !OnCc( nType, fieldOdd, cc.m_ccField1[1] ))
				return false;
		}
	}

	if( !cc.m_ccField2[0].IsEmpty())
	{
		if( !OnCc( nType, fieldEven, cc.m_ccField2[0] ))
			return false;

		if( !cc.m_ccField1[1].IsEmpty())
		{
			if( !OnCc( nType, fieldEven, cc.m_ccField2[1] ))
				return false;
		}
	}

	return true;
}

void CCcTextParser::Reset( UINT idChannel, UINT idMode )
{
	ASSERT( idChannel <= 3 );
	m_ccproLookedFor.m_idChannel = idChannel;

	ASSERT( idChannel < CCPROFILE::cModes );
	m_ccproLookedFor.m_idMode = idMode;

	m_ccproCurrent.m_idMode = CCPROFILE::modeNone;
	m_ccproCurrent.m_idChannel = 0;

	m_ccLastCode = 0;
}

bool CCcTextParser::OnCc( int nType, int iField, CCWORD cc )
{
	if( nType != 0 && !cc.IsEmpty() && m_ccproLookedFor.IsFieldOK( iField ))
	{
		if( cc.IsStartXDS()) 
		{
			ASSERT( iField == CCcParser::fieldEven );
			m_ccproCurrent.m_idMode = CCPROFILE::modeXDS;
		}
		else if( cc.IsEndXDS())
		{
			ASSERT( iField == CCcParser::fieldEven );
			m_ccproCurrent.m_idMode = CCPROFILE::modeCC;
		}
		else if( cc.IsCode() && !cc.IsExtChar())
		{
			m_ccproCurrent.m_idMode = CCPROFILE::modeCC;

			if( cc == m_ccLastCode )
			{
				m_ccLastCode = 0;
				return true;
			}

			m_ccLastCode = cc;
				
			m_ccproCurrent.UpdateChannelFlag( cc.b1() );
			if( m_ccproCurrent == m_ccproLookedFor )
			{
				if( !OnCode( cc ))
					return false;
			}
		}
		else if( m_ccproCurrent == m_ccproLookedFor )
		{
			ASSERT( cc.IsText());
			
			if( !OnText( cc ))
				return false;
		}
	}

	

	return true;
}

