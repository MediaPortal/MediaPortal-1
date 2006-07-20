//------------------------------------------------------------------------------
// File: Dump.h
//
// Desc: DirectShow sample code - definitions for dump renderer.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
#include <map>
#include "filewriter.h"
#include "multifilewriter.h"
using namespace std;

class CDumpInputPin;
class CDump;
class CDumpFilter;

DEFINE_GUID(CLSID_MPFileWriter, 0xdb35f5ed, 0x26b2, 0x4a2a,  0x92, 0xd3, 0x85, 0x2e, 0x14, 0x5b, 0xf3, 0x2d );

DEFINE_GUID(IID_IMPFileRecord,0xd5ff805e, 0xa98b, 0x4d56,  0xbe, 0xde, 0x3f, 0x1b, 0x8e, 0xf7, 0x25, 0x33);

// interface
DECLARE_INTERFACE_(IMPFileRecord, IUnknown)
{
    STDMETHOD(SetRecordingFileName)(THIS_ char* pszFileName)PURE;
    STDMETHOD(StartRecord)(THIS_ )PURE;
    STDMETHOD(StopRecord)(THIS_ )PURE;
    STDMETHOD(IsReceiving)(THIS_ BOOL* yesNo)PURE;
    STDMETHOD(Reset)(THIS_)PURE;
    STDMETHOD(SetTimeShiftFileName)(THIS_ char* pszFileName)PURE;
    STDMETHOD(StartTimeShifting)(THIS_ )PURE;
    STDMETHOD(StopTimeShifting)(THIS_ )PURE;
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
	BOOL				m_bIsReceiving;
	DWORD				m_lTickCount;
	CCritSec		m_section;
};


//  CDump object which has filter and pin members

class CDump : public CUnknown, public IMPFileRecord
{
    friend class CDumpFilter;
    friend class CDumpInputPin;
    CDumpFilter*	m_pFilter;       // Methods for filter interfaces
    CDumpInputPin*	m_pPin;          // A simple rendered input pin
    CCritSec 		m_Lock;                // Main renderer critical section
    CCritSec 		m_ReceiveLock;         // Sublock for received samples
		FileWriter* m_pRecordFile;
		MultiFileWriter* m_pTimeShiftFile;
public:
    DECLARE_IUNKNOWN

    CDump(LPUNKNOWN pUnk, HRESULT *phr);
    ~CDump();

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

		HRESULT 		 Write(PBYTE pbData, LONG lDataLength);
		STDMETHODIMP SetRecordingFileName(char* pszFileName);
		STDMETHODIMP StartRecord();
		STDMETHODIMP StopRecord();

		STDMETHODIMP IsReceiving( BOOL* yesNo);
		STDMETHODIMP Reset();
		
    STDMETHODIMP SetTimeShiftFileName(char* pszFileName);
    STDMETHODIMP StartTimeShifting();
    STDMETHODIMP StopTimeShifting();
private:

    // Overriden to say what interfaces we support where
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

	char	m_strRecordingFileName[1024];
	char	m_strTimeShiftFileName[1024];
};
