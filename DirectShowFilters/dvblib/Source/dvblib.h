// dvblib.h : Hauptheaderdatei für die dvblib DLL
//
#include "stdafx.h"
//

#pragma once

#ifndef __AFXWIN_H__
	#error 'stdafx.h' muss vor dieser Datei in PCH eingeschlossen werden.
#endif

#include "resource.h"		// Hauptsymbole

//
// CdvblibApp
// Siehe dvblib.cpp für die Implementierung dieser Klasse
//
typedef struct tagVIDEOINFOHEADER2 {
    RECT                rcSource;
    RECT                rcTarget;
    DWORD               dwBitRate;
    DWORD               dwBitErrorRate;
    REFERENCE_TIME      AvgTimePerFrame;
    DWORD               dwInterlaceFlags;   // use AMINTERLACE_* defines. Reject connection if undefined bits are not 0
    DWORD               dwCopyProtectFlags; // use AMCOPYPROTECT_* defines. Reject connection if undefined bits are not 0
    DWORD               dwPictAspectRatioX; // X dimension of picture aspect ratio, e.g. 16 for 16x9 display
    DWORD               dwPictAspectRatioY; // Y dimension of picture aspect ratio, e.g.  9 for 16x9 display
    union {
        DWORD dwControlFlags;               // use AMCONTROL_* defines, use this from now on
        DWORD dwReserved1;                  // for backward compatiblity (was "must be 0";  connection rejected otherwise)
    };
    DWORD               dwReserved2;        // must be 0; reject connection otherwise
    BITMAPINFOHEADER    bmiHeader;
} VIDEOINFOHEADER2;

typedef struct tagMPEG2VIDEOINFO {
    VIDEOINFOHEADER2    hdr;
    DWORD               dwStartTimeCode;        //  ?? not used for DVD ??
    DWORD               cbSequenceHeader;       // is 0 for DVD (no sequence header)
    DWORD               dwProfile;              // use enum MPEG2Profile
    DWORD               dwLevel;                // use enum MPEG2Level
    DWORD               dwFlags;                // use AMMPEG2_* defines.  Reject connection if undefined bits are not 0
    DWORD               dwSequenceHeader[1];    // DWORD instead of Byte for alignment purposes
                                                //   For MPEG-2, if a sequence_header is included, the sequence_extension
                                                //   should also be included
} MPEG2VIDEOINFO;
typedef struct
{
    WORD SectionLength          : 12;
    WORD Reserved               :  2;
    WORD PrivateIndicator       :  1;
    WORD SectionSyntaxIndicator :  1;
} MPEG_HEADER_BITS, *PMPEG_HEADER_BITS;

typedef HRESULT (__stdcall *DeliverSectionData)(BYTE *dataPointer,long sectionLength,WORD header);

typedef HRESULT (*SAMPLECALLBACK) (
    IMediaSample * pSample, 
    REFERENCE_TIME * StartTime, 
    REFERENCE_TIME * StopTime,
    BOOL TypeChanged );


// We define the interface the app can use to program us
MIDL_INTERFACE("6B652FFF-11FE-4FCE-92AD-0266B5D7C78F")
IGrabberSample : public IUnknown
{
    public:
        
        virtual HRESULT STDMETHODCALLTYPE SetAcceptedMediaType( 
            const CMediaType *pType) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetConnectedMediaType( 
            CMediaType *pType) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetCallback( 
            SAMPLECALLBACK Callback) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetDeliveryBuffer( 
            ALLOCATOR_PROPERTIES props,
            BYTE *pBuffer) = 0;
};

class CdvblibApp : public CWinApp
{
public:
	CdvblibApp();

// Überschreibungen
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
