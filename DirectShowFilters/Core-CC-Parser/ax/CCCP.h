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

#ifndef __CCCP_H
#define __CCCP_H

#include "ICcParser.h"
#include "CcDataProcessor.h"

using namespace std;

// Put out the name of a function and instance on the debugger.
// Invoke this at the start of functions to allow a trace.
#define DbgFunc(a) DbgLog(( LOG_TRACE                        \
                          , 2                                \
                          , TEXT("CCcFilter(Instance %d)::%s") \
                          , m_nThisInstance                  \
                          , TEXT(a)                          \
                         ));

//------------------------------------------------------------------------
// CCcFilter - the gargle filter class
//------------------------------------------------------------------------

class CCcFilter;

class CLine21OutputPin : public CBaseOutputPin
{
public:
    CLine21OutputPin( CCcFilter* pFilter );

	HRESULT UpdateCurrentService( IPin* pReceivePin = NULL );

	inline CCcFilter* GetFilter();
	
	// Overrides
	virtual HRESULT GetMediaType( int iPosition, CMediaType *pMediaType );
	virtual HRESULT CheckMediaType(const CMediaType* pmtOut);
	
	virtual HRESULT DecideBufferSize(IMemAllocator *,ALLOCATOR_PROPERTIES *);
	virtual HRESULT CompleteConnect(IPin *pReceivePin);

    // IQualityControl via CBasePin -- to prevern ASSERT
    virtual STDMETHODIMP Notify(IBaseFilter * pSender, Quality q);

	static const GUID m_guidMediaMajor;
	static const GUID m_guidMediaSubtype;
	static const WCHAR m_szName[];
};

class CCcFilterPassThroughPin : public CTransInPlaceOutputPin
{
public:
    CCcFilterPassThroughPin( CCcFilter* pFilter );

    virtual STDMETHODIMP QueryId(LPWSTR* Id) { return AMGetWideString(Name(), Id); }
};

class CCcFilterInputPin : public CTransInPlaceInputPin
{
public:
    CCcFilterInputPin( CCcFilter* pFilter );

    virtual STDMETHODIMP QueryId(LPWSTR* Id) { return AMGetWideString(Name(), Id); }

	virtual STDMETHODIMP BeginFlush();
	virtual STDMETHODIMP EndFlush();
	virtual HRESULT CheckStreaming();
};

class CCcFilter
    // Inherited classes
    : public CTransInPlaceFilter 
    , public ISpecifyPropertyPages
    , public ICcParser     
	, public CPersistStream
{

public:
    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

    DECLARE_IUNKNOWN;

    // Basic COM - used here to reveal our property interface.
    virtual STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    // CPersistStream overrides
    virtual HRESULT WriteToStream(IStream *pStream);
    virtual HRESULT ReadFromStream(IStream *pStream);
    virtual int SizeMax();
	virtual DWORD GetSoftwareVersion();
    virtual STDMETHODIMP GetClassID(CLSID *pClsid);

    // ISpecifyPropertyPages 
    virtual STDMETHODIMP GetPages(CAUUID * pPages);

    // CBaseFilter Overrides --
    virtual int GetPinCount();
    virtual CBasePin* GetPin(int n);
    virtual STDMETHODIMP FindPin(LPCWSTR Id, IPin **ppPin);

    // CTransInPlaceFilter Overrides --
    virtual HRESULT CheckInputType(const CMediaType *mtIn);

    // ICcParser - private interface to put/set properties
	STDMETHODIMP AddDataSink( ICcDataSink* pSink, DWORD* pidSink ){ CAutoLock foo(&m_CcParserLock); return m_proc.AddDataSink( pSink, pidSink ); }
    STDMETHODIMP RemoveDataSink( DWORD idSink ){ CAutoLock foo(&m_CcParserLock); return m_proc.RemoveDataSink( idSink ); }

	STDMETHODIMP get_Channel( int* piChannel ){ CAutoLock foo(&m_CcParserLock); return m_proc.get_Channel( piChannel ); }
    STDMETHODIMP put_Channel( int iChannel )
	{
		CAutoLock foo(&m_CcParserLock); 

		RETURN_FAILED( m_proc.put_Channel( iChannel )); 
		m_outpinLine21.UpdateCurrentService();

		CPersistStream::SetDirty(TRUE);

		return S_OK;
	}
        
    STDMETHODIMP get_Service( AM_LINE21_CCSERVICE* piService ){ CAutoLock foo(&m_CcParserLock); return m_proc.get_Service( piService ); }
    STDMETHODIMP put_Service( AM_LINE21_CCSERVICE iService )
	{ 
		CAutoLock foo(&m_CcParserLock); 
		
		RETURN_FAILED( m_proc.put_Service( iService )); 
		m_outpinLine21.UpdateCurrentService();

		CPersistStream::SetDirty(TRUE);
		return S_OK;
	}

    STDMETHODIMP get_XformType( ICcParser_CCTYPE* piType ){ CAutoLock foo(&m_CcParserLock); return m_proc.get_XformType( piType ); }
    STDMETHODIMP put_XformType( ICcParser_CCTYPE iType )
	{ 
		CAutoLock foo(&m_CcParserLock); 
	
		RETURN_FAILED( m_proc.put_XformType( iType )); 

		CPersistStream::SetDirty(TRUE);
		return S_OK;
	}

	// Pin-related constants
	static const GUID m_guidPassThroughMediaMajor;

	static const WCHAR m_szInput[];
	static const WCHAR m_szPassThrough[];

private:
	friend class CLine21OutputPin;

    // Constructor
    CCcFilter(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr);
	~CCcFilter();

    virtual HRESULT Receive(IMediaSample *pSample);
	HRESULT InitializeOutputSample( CBaseOutputPin* pPin, IMediaSample *pSample, IMediaSample **ppOutSample );

	// CTransInPlaceFilter 
	virtual HRESULT Transform(IMediaSample *pSample){ ASSERT(0); return S_OK;} // We don't use it but have to override pure virtual

	virtual STDMETHODIMP Stop();
	virtual STDMETHODIMP Pause();

    // Pass through calls downstream
	virtual HRESULT EndOfStream();
    virtual HRESULT BeginFlush();
    virtual HRESULT EndFlush();
    virtual HRESULT NewSegment( REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);

	// If there are multiple instances of this filter active, it's
    // useful for debug messages etc. to know which one this is.
    // This variable has no other purpose.
    static int m_nInstanceCount;                   // total instances
    int m_nThisInstance;

    CCritSec m_CcParserLock;          // To serialise access.

	CCcFilterInputPin			 m_inpinInput;
	CCcFilterPassThroughPin		 m_outpinPassThrough;
	CLine21OutputPin			 m_outpinLine21;

	COutputQueue* m_pPassThroughQueue;
	COutputQueue* m_pLine21Queue;

	CCcDataProcessor m_proc;
	CAtlArray<WORD> m_rgCCData;
	
  GUID m_guidPassThroughMediaSubtype;
  
  bool m_bIsSubtypeAVC1;
  
  DWORD m_dwFlags;

};

inline CCcFilter* CLine21OutputPin::GetFilter() 
{ 
	return static_cast<CCcFilter*>( m_pFilter ); 
}

#endif // ndef __CCCP_H