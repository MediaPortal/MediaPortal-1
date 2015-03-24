//    -----------------------------------------
//    Notes for original code version (2.0.0.6)
//    -----------------------------------------
//    CCCP (Core Closed Captioning Parser) is a DirectShow filter 
//    that extracts Closed Captioning data from MPEG2 video. 
//    The data is normally used by the downstream filter to 
//    render and mix the CC with main video to aid hearing
//    or language-impaired users
//
//    Closed Captioning MPEG2 Parser
//    Original author: Zodiak
//    Copyright (C) 2004 zodiak@dvbn
//    -----------------------------------------

/*
 *  Modified to add H.264 Closed Caption parsing 
 *  and converted to Unicode.
 *
 *  Copyright (C) 2015 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include "../Parsing/CCParser.h"

interface ICcDataSink;

class CCcDataProcessor	: public CCcParser
{
public:
	CCcDataProcessor();
	~CCcDataProcessor();
	
	STDMETHODIMP AddDataSink( ICcDataSink* pSink, DWORD* pidSink );
    STDMETHODIMP RemoveDataSink( DWORD idSink );
    
	STDMETHODIMP get_Channel( int* iChannel );
    STDMETHODIMP put_Channel( int iChannel );
        
    STDMETHODIMP get_Service( AM_LINE21_CCSERVICE* piService );
    STDMETHODIMP put_Service( AM_LINE21_CCSERVICE iService );

    STDMETHODIMP get_XformType( ICcParser_CCTYPE* piType );
    STDMETHODIMP put_XformType( ICcParser_CCTYPE piType );

	void ProcessData( int cbData, const BYTE* pSrc, BYTE* pToTransform, CAtlArray<WORD>* pCCData, bool bIsSubtypeAVC1, DWORD dwFlags, REFERENCE_TIME sourceTime );

	//	CCcParser
	virtual bool OnCc( int nType, int iField, CCWORD ccField );
	virtual const BYTE* OnUserData( bool bPorI, const BYTE* p, const BYTE* pStop, REFERENCE_TIME sourceTime, bool bIsSubtypeAVC1 );
	virtual bool OnCCSet( bool bPorI, int nType, const CCWORDSET& ccSet, bool bIsSubtypeAVC1 );
	virtual void Reset();

private:
	CCcTextParser::CCPROFILE m_profile;
	ICcParser_CCTYPE m_iTransformType;

    auto_pif<ICcDataSink> m_pifDataSink; //TODO: multiple

	const BYTE* m_pSrcData;
	CAtlArray<WORD>* m_pCCData;
	BYTE* m_pDataToTransform;

	ICcParser_CCTYPE m_nRecentlyRecognizedType;
	CCWORDSET m_ccRecentlyRecognizedSet;
};

