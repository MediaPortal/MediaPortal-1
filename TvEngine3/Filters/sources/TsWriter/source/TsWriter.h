//------------------------------------------------------------------------------
// File: Dump.h
//
// Desc: DirectShow sample code - definitions for dump renderer.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
#include <map>
using namespace std;

class CDumpInputPin;
class CDump;
class CDumpFilter;

DEFINE_GUID(CLSID_TsWriter, 0xfc50bed6, 0xfe38, 0x42d3, 0xb8, 0x31, 0x77, 0x16, 0x90, 0x9, 0x1a, 0x6e);

DEFINE_GUID(IID_IMPFileRecord,0x59f8d617, 0x92fd, 0x48d5, 0x8f, 0x6d, 0xa9, 0x7b, 0xfd, 0x95, 0xc4, 0x48);

// interface
DECLARE_INTERFACE_(ITsWriter, IUnknown)
{
	STDMETHOD(SetVideoPid)(THIS_ int videoPid)PURE;
	STDMETHOD(GetVideoPid)(THIS_ int* videoPid)PURE;
	
	STDMETHOD(SetAudioPid)(THIS_ int audioPid)PURE;
	STDMETHOD(GetAudioPid)(THIS_ int* audioPid)PURE;
	
	STDMETHOD(IsVideoEncrypted)(THIS_ int* yesNo)PURE;
	STDMETHOD(IsAudioEncrypted)(THIS_ int* yesNo)PURE;

	STDMETHOD(ResetAnalyer)(THIS_)PURE;
};
// Main filter object

class CDumpFilter : public CBaseFilter
{
    CDump * const m_pDump;

public:

    // Constructor
    CDumpFilter(CDump *pDump,
                LPUNKNOWN pUnk,
                CCritSec *pLock,
                HRESULT *phr);

    // Pin enumeration
    CBasePin * GetPin(int n);
    int GetPinCount();

    // Open and close the file as necessary
    STDMETHODIMP Run(REFERENCE_TIME tStart);
    STDMETHODIMP Pause();
    STDMETHODIMP Stop();
};


//  Pin object

class CDumpInputPin : public CRenderedInputPin
{
    CDump    * const	m_pDump;           // Main renderer object
    CCritSec * const	m_pReceiveLock;    // Sample critical section
public:

    CDumpInputPin(CDump *pDump,
                  LPUNKNOWN pUnk,
                  CBaseFilter *pFilter,
                  CCritSec *pLock,
                  CCritSec *pReceiveLock,
                  HRESULT *phr);

    // Do something with this media sample
    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream(void);
    STDMETHODIMP ReceiveCanBlock();

    // Write detailed information about this sample to a file
//    HRESULT WriteStringInfo(IMediaSample *pSample);

    // Check if the pin can support this specific proposed type and format
    HRESULT		CheckMediaType(const CMediaType *);
    // Break connection
    HRESULT		BreakConnect();
		BOOL			IsReceiving();
		void			Reset();
    // Track NewSegment
    STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);
private:
	CCritSec		m_section;
};


//  CDump object which has filter and pin members

class CDump : public CUnknown, public ITsWriter
{
	typedef struct TSHeader
	{
		BYTE SyncByte			;
		bool TransportError		;
		bool PayloadUnitStart	;
		bool TransportPriority	;
		unsigned short Pid		;
		BYTE TScrambling		;
		BYTE AdaptionControl	;
		BYTE ContinuityCounter	;
	};

    friend class CDumpFilter;
    friend class CDumpInputPin;
    CDumpFilter*	m_pFilter;       // Methods for filter interfaces
    CDumpInputPin*	m_pPin;          // A simple rendered input pin
    CCritSec 		m_Lock;                // Main renderer critical section
    CCritSec 		m_ReceiveLock;         // Sublock for received samples
public:
    DECLARE_IUNKNOWN

    CDump(LPUNKNOWN pUnk, HRESULT *phr);
    ~CDump();

		STDMETHODIMP SetVideoPid( int videoPid);
		STDMETHODIMP GetVideoPid( int* videoPid);
		
		STDMETHODIMP SetAudioPid( int audioPid);
		STDMETHODIMP GetAudioPid( int* audioPid);
		
		STDMETHODIMP IsVideoEncrypted( int* yesNo);
		STDMETHODIMP IsAudioEncrypted( int* yesNo);
		STDMETHODIMP ResetAnalyer();
    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

		void Analyze(BYTE* pbData, int nLen);
		int FindOffset(BYTE* pbData, int nLen);
		void LogHeader(TSHeader& header);
private:
		HRESULT GetTSHeader(BYTE *data,TSHeader *header);
		int m_videoPid;
		int m_audioPid;
		BOOL m_bAudioEncrypted;
		BOOL m_bVideoEncrypted;
		DWORD m_audioTimer;
		DWORD m_videoTimer;
		BOOL m_bInitAudio;
		BOOL m_bInitVideo;
    // Overriden to say what interfaces we support where
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

};
