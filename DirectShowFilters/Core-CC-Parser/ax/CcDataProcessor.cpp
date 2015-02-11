#include "StdAfx.h"
#include "ICcParser.h"

#include <Il21dec.h>

#include "CcDataProcessor.h"

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	CCcDataProcessor

CCcDataProcessor::CCcDataProcessor()
:	m_pSrcData(NULL), m_pCCData(NULL), m_pDataToTransform(NULL)
,	m_iTransformType(ICcParser_CCTYPE_None) 
,	m_nRecentlyRecognizedType(ICcParser_CCTYPE_None)
{
	m_profile.m_idChannel = 0;
	m_profile.m_idMode = CCcTextParser::CCPROFILE::modeCC;
}

CCcDataProcessor::~CCcDataProcessor()
{
}


void CCcDataProcessor::Reset()
{
  CCcParser::Reset();
}

enum{ idTheOnlySink = 1 }; // TODO: remove when multiple sinks are implemented

STDMETHODIMP CCcDataProcessor::AddDataSink( ICcDataSink* pSink, DWORD* pidSink )
{
    CheckPointer(pSink,E_POINTER);
    CheckPointer(pidSink,E_POINTER);

	if( m_pifDataSink )
		return E_FAIL;

    m_pifDataSink = pSink;

	*pidSink = idTheOnlySink;

    return NOERROR;
}

STDMETHODIMP CCcDataProcessor::RemoveDataSink( DWORD idSink )
{
    if( idSink != idTheOnlySink || !m_pifDataSink )
		return E_UNEXPECTED;
	
	m_pifDataSink = NULL;

    return NOERROR;
}

STDMETHODIMP CCcDataProcessor::get_Channel( int* piChannel )
{
    CheckPointer(piChannel,E_POINTER);

	*piChannel = m_profile.m_idChannel;

	return S_OK;
}

STDMETHODIMP CCcDataProcessor::put_Channel( int iChannel )
{
	if( iChannel < 0 || iChannel >= CCcTextParser::CCPROFILE::cMaxChannels )
		return E_INVALIDARG;

	m_profile.m_idChannel = iChannel;

	return S_OK;
}

STDMETHODIMP CCcDataProcessor::get_Service( AM_LINE21_CCSERVICE* piService )
{
    CheckPointer(piService,E_POINTER);

	switch( m_profile.m_idMode )
	{
	case CCcTextParser::CCPROFILE::modeNone:
		*piService = AM_L21_CCSERVICE_None;
		break;

	case CCcTextParser::CCPROFILE::modeCC:
		*piService = ( m_profile.m_idChannel % 2 == 0 ) 
					 ? AM_L21_CCSERVICE_Caption1
					 : AM_L21_CCSERVICE_Caption2;
		break;

	case CCcTextParser::CCPROFILE::modeText:
		*piService = ( m_profile.m_idChannel % 2 == 0 ) 
					 ? AM_L21_CCSERVICE_Text1
					 : AM_L21_CCSERVICE_Text2;
		break;

	case CCcTextParser::CCPROFILE::modeXDS:
		*piService = AM_L21_CCSERVICE_XDS;
		break;

	default:
		ASSERT(0);
		return E_UNEXPECTED;

	}

	return S_OK;
}

STDMETHODIMP CCcDataProcessor::put_Service( AM_LINE21_CCSERVICE iService )
{
	int iMode = CCcTextParser::CCPROFILE::modeNone;
	
	switch( iService )
	{
	case AM_L21_CCSERVICE_None:
		return S_OK;

	case AM_L21_CCSERVICE_Caption1:
	case AM_L21_CCSERVICE_Caption2:
		iMode = CCcTextParser::CCPROFILE::modeCC;
		break;

	case AM_L21_CCSERVICE_Text1:
    case AM_L21_CCSERVICE_Text2:
		iMode = CCcTextParser::CCPROFILE::modeText;
		break;

	case AM_L21_CCSERVICE_XDS:
		iMode = CCcTextParser::CCPROFILE::modeXDS;
		break;

	default:
		return E_NOTIMPL;
	}
	
	ASSERT( CCcTextParser::CCPROFILE::modeNone < iMode && iMode < CCcTextParser::CCPROFILE::cModes );

	m_profile.m_idMode = iMode;

	return S_OK;
}

STDMETHODIMP CCcDataProcessor::get_XformType( ICcParser_CCTYPE* piTransformType )
{ 
    CheckPointer(piTransformType,E_POINTER);

	*piTransformType = m_iTransformType;

	return S_OK;
}

STDMETHODIMP CCcDataProcessor::put_XformType( ICcParser_CCTYPE iTransformType )
{ 
	if( iTransformType < 0 || iTransformType >= __ICcParser_cTypesSupported )
		return E_INVALIDARG;

	if( iTransformType != ICcParser_CCTYPE_None &&
		iTransformType != ICcParser_CCTYPE_ATSC_A53
	  )
		return E_NOTIMPL;  //TODO
	
	m_iTransformType = iTransformType;

	return S_OK;
}

void CCcDataProcessor::ProcessData( int cbData, const BYTE* pSrc, BYTE* pToTransform, CAtlArray<WORD>* pCCData, bool bIsSubtypeAVC1, DWORD dwFlags, REFERENCE_TIME sourceTime )
{
  ASSERT( !m_pSrcData );
  ASSERT( !m_pCCData );
	ASSERT( !m_pDataToTransform );

	m_pSrcData = pSrc;
	m_pCCData = pCCData;
	m_pDataToTransform = pToTransform;

	if (bIsSubtypeAVC1)
	{
		CCcParser::OnDataArrivalAVC1( pSrc, cbData, dwFlags, sourceTime );
	}
	else
	{
		CCcParser::OnDataArrivalMPEG( pSrc, cbData, sourceTime );
	}
	
	m_pSrcData = NULL;
	m_pCCData = NULL;
	m_pDataToTransform = NULL;

}

bool CCcDataProcessor::OnCc( int nType, int iField, CCWORD ccField )
{
	if( nType != 0 && m_pCCData )
	{	
		if( m_profile.IsFieldOK( iField ))
		{
			m_pCCData->Add( ccField );
		}
	}

	if( m_pifDataSink )
		m_pifDataSink->OnCc( nType, iField, ccField );

	return true;
}

bool CCcDataProcessor::OnCCSet( bool bPorI, int nType, const CCWORDSET& ccSet, bool bIsSubtypeAVC1 )
{
	m_nRecentlyRecognizedType = (ICcParser_CCTYPE)nType;
	m_ccRecentlyRecognizedSet = ccSet;

	return CCcParser::OnCCSet( bPorI, nType, ccSet, bIsSubtypeAVC1 );
}

const BYTE* CCcDataProcessor::OnUserData( bool bPorI, const BYTE* pUserData, const BYTE* pAllStop, REFERENCE_TIME sourceTime, bool bIsSubtypeAVC1 )
{
	ASSERT( 0 == m_nRecentlyRecognizedType && m_ccRecentlyRecognizedSet.IsEmpty());

	const BYTE* pUserDataStop = CCcParser::OnUserData( bPorI, pUserData, pAllStop, sourceTime, bIsSubtypeAVC1 );
	const int cbUserData = pUserDataStop - pUserData;

	ASSERT( m_pSrcData <= pUserData    && pUserData < pAllStop );
	ASSERT( m_pSrcData < pUserDataStop && pUserDataStop <= pAllStop );

	if( m_pDataToTransform && 
		m_iTransformType && m_nRecentlyRecognizedType && m_iTransformType != m_nRecentlyRecognizedType )
	{
		switch( m_iTransformType )
		{
			case ICcParser_CCTYPE_ATSC_A53:
			{
				const DWORD ATSC_identifier = 0x34394147; ASSERT( 4 == sizeof( ATSC_identifier ));
				const BYTE user_data_type_code_CC = 0x03; ASSERT( 1 == sizeof( user_data_type_code_CC ));
				const BYTE process_cc_data_flag   = 0x40; ASSERT( 1 == sizeof( process_cc_data_flag ));
				const BYTE marker_bits_cc_valid_1 = 0xfc; ASSERT( 1 == sizeof( marker_bits_cc_valid_1 ));
				const BYTE marker_bits_cc_valid_2 = 0xfd; ASSERT( 1 == sizeof( marker_bits_cc_valid_2 ));
				enum{ cbCCHeader = 7, cbCCWord = 3 };

				if( cbUserData >= cbCCHeader )
				{
					PBYTE p = pUserData - m_pSrcData + m_pDataToTransform;
					const PBYTE pStop = p + cbUserData;

					*reinterpret_cast<UNALIGNED DWORD*>
					(p) = ATSC_identifier;       p += sizeof( ATSC_identifier );
					*p = user_data_type_code_CC; p += sizeof( user_data_type_code_CC );
			
					BYTE* pCCInfo = p; 
					*p = 0x80; /*reserved*/		 p+= 1;
					*p = 0xFF   /*reserved*/;    p+= 1;

					BYTE cCC = 0;
					for( int i = 0; i < m_ccRecentlyRecognizedSet.cSavedFields; ++i )
					{
						if( p >= pStop || m_ccRecentlyRecognizedSet.m_ccField1[i].IsEmpty())
							break;

						*p = marker_bits_cc_valid_1;				   p += sizeof( marker_bits_cc_valid_1 );
						*reinterpret_cast<UNALIGNED WORD*>
						(p) = m_ccRecentlyRecognizedSet.m_ccField1[i]; p += 2;
						
						cCC++;
					}

					for( int i = 0; i < m_ccRecentlyRecognizedSet.cSavedFields; ++i )
					{
						if( p >= pStop || m_ccRecentlyRecognizedSet.m_ccField2[i].IsEmpty())
							break;

						*p = marker_bits_cc_valid_2;				   p += sizeof( marker_bits_cc_valid_2 );
						*reinterpret_cast<UNALIGNED WORD*>
						(p) = m_ccRecentlyRecognizedSet.m_ccField2[i]; p += 2;
						
						cCC++;
					}

					if( cCC > 0 )
					{
						*pCCInfo |= process_cc_data_flag;
						*pCCInfo |= cCC;
					}
				}
				else
					ASSERT(0);
			}
			break;
		}
	}

	m_nRecentlyRecognizedType = ICcParser_CCTYPE_None;
	m_ccRecentlyRecognizedSet.Empty();

	return pUserDataStop;
}

