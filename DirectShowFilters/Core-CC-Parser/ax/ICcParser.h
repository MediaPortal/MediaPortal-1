//------------------------------------------------------------------------------
// File: ICcParser.h
//
// Desc: DirectShow sample code - custom interface that allows the user
//       to adjust the modulation rate.  It defines the interface between
//       the user interface component (the property sheet) and the filter
//       itself.  This interface is exported by the code in CcParser.cpp and
//       is used by the code in GargProp.cpp.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------


#ifndef __ICCPARSER__
#define __ICCPARSER__

#ifdef __cplusplus
extern "C" {
#endif

#include <Il21dec.h>


//
// ICcParser's GUID
//
// {602A08B2-ECD4-4920-9E44-DAD5D60B8A0A}
DEFINE_GUID(IID_ICcParser,
0x602a08b2, 0xecd4, 0x4920, 0x9e, 0x44, 0xda, 0xd5, 0xd6, 0xb, 0x8a, 0xa);

interface ICcDataSink;

enum ICcParser_CCTYPE
{
	ICcParser_CCTYPE_None,
	ICcParser_CCTYPE_Echostar,
	ICcParser_CCTYPE_ATSC_A53,

	__ICcParser_cTypesSupported
};

//
// ICcParser
//
interface ICcParser : public IUnknown 
{
    STDMETHOD(get_Channel)( int* piChannel ) PURE; // [out]
    STDMETHOD(put_Channel)( int iChannel ) PURE; // [in]
        
    STDMETHOD(get_Service)( AM_LINE21_CCSERVICE* piService ) PURE; // [out]
    STDMETHOD(put_Service)( AM_LINE21_CCSERVICE iService ) PURE; // [in]
        
	STDMETHOD(AddDataSink)( ICcDataSink* pSink, DWORD* idSink ) PURE;
    STDMETHOD(RemoveDataSink)( DWORD idSink ) PURE;

    STDMETHOD(get_XformType)( ICcParser_CCTYPE* piType ) PURE; // [out]
    STDMETHOD(put_XformType)( ICcParser_CCTYPE piType ) PURE; // [in]
};

//
// ICcDataSink
//

// {82B23C07-C68F-4bce-AE08-F2620E384114}
DEFINE_GUID(IID_ICcDataSink,
0x82b23c07, 0xc68f, 0x4bce, 0xae, 0x8, 0xf2, 0x62, 0xe, 0x38, 0x41, 0x14);

interface ICcDataSink : public IUnknown 
{
	STDMETHOD(OnCc)( int nType, int iField, WORD ccField ) PURE;
	STDMETHOD(OnProgress)( int nPercent ) PURE;
};

#ifdef __cplusplus
}
#endif

#endif // __ICCPARSER__
