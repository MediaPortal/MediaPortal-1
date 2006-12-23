//------------------------------------------------------------------------------
// File: Dump.h
//
// Desc: DirectShow sample code - definitions for dump renderer.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
#ifndef __MPSA_
#define __MPSA_
#pragma warning(disable: 4511 4512 4995)
#undef INITGUID

#include "Section.h"
#include "SplitterSetup.h"
#include "atscparser.h"

class CStreamAnalyzerSectionsPin;
class CStreamAnalyzer;
class CStreamAnalyzerFilter;
class CMHWInputPin1;
class CMHWInputPin2;
class CEPGInputPin;
#define EC_PROGRAM_CHANGED EC_USER + 100
#define EC_GUIDE_CHANGED EC_USER + 101



DECLARE_INTERFACE_(IPMTCallback, IUnknown)
{
	STDMETHOD(OnPMTReceived)()PURE;
};

DECLARE_INTERFACE_(IHardwarePidFiltering, IUnknown)
{
	STDMETHOD(FilterPids)  (int count, int* pids)PURE;
};

// {B4F1F9BF-9ECA-41b8-883B-9C7FC0DD7047}
DEFINE_GUID(CLSID_StreamAnalyzerPropPage, 0xb4f1f9bf, 0x9eca, 0x41b8, 0x88, 0x3b, 0x9c, 0x7f, 0xc0, 0xdd, 0x70, 0x47);

// {BAAC8911-1BA2-4ec2-96BA-6FFE42B62F72}
DEFINE_GUID(CLSID_MPDSA, 0xbaac8911, 0x1ba2, 0x4ec2, 0x96, 0xba, 0x6f, 0xfe, 0x42, 0xb6, 0x2f, 0x72);

// {FB1EF498-2C7D-4fed-B2AA-B8F9E199F074}
DEFINE_GUID(IID_IStreamAnalyzer, 0xfb1ef498, 0x2c7d, 0x4fed, 0xb2, 0xaa, 0xb8, 0xf9, 0xe1, 0x99, 0xf0, 0x74);
// interface
DECLARE_INTERFACE_(IStreamAnalyzer, IUnknown)
{
	STDMETHOD(put_MediaType) (THIS_
    				  CMediaType *pmt       /* [in] */	// the media type selected
				 ) PURE;

    STDMETHOD(get_MediaType) (THIS_
    				  CMediaType **pmt      /* [out] */	// the media type selected
				 ) PURE;

    STDMETHOD(get_IPin) (THIS_
    				  IPin **pPin          /* [out] */	// the source pin
				 ) PURE;


    STDMETHOD(get_State) (THIS_
    				  FILTER_STATE *state  /* [out] */	// the filter state
				 ) PURE;

    STDMETHOD(GetChannelCount) (THIS_
    	WORD *count  /* [out] */	// count of channels
				 ) PURE;
    
	STDMETHOD(GetChannel) (THIS_
		WORD n, /* [in] number of channel */
		BYTE *ch  /* [out] */	// count of channels
				 ) PURE;
	
	
	STDMETHOD(GetCISize) (THIS_
		WORD *size
				 ) PURE;

	STDMETHOD(ResetParser)()PURE;
	STDMETHOD(ResetPids)()PURE;

	STDMETHOD(SetPMTProgramNumber) (THIS_ ULONG prgNum)PURE;
	STDMETHOD(GetPMTData) (THIS_ BYTE *pmtData)PURE;
	STDMETHOD(IsChannelReady) (THIS_ ULONG chNum)PURE;

	STDMETHOD(UseATSC) (THIS_ BOOL yesNo)PURE;
	STDMETHOD(IsATSCUsed) (THIS_ BOOL* yesNo)PURE;

	STDMETHOD(GetLCN) (THIS_ WORD channel, WORD* networkId, WORD* transportId, WORD* serviceID, WORD* LCN)PURE;
	STDMETHOD(SetPidFilterCallback) (THIS_ IHardwarePidFiltering* callback);

	STDMETHOD(Scanning) (THIS_ BOOL yesNo)PURE;
	STDMETHOD(SetPMTCallback) (THIS_ IPMTCallback* callback)PURE;
};

// {6301D1B8-6C92-4c6e-8CC2-CD1B05C6B545}
DEFINE_GUID(IID_IEPGGrabber, 0x6301d1b8, 0x6c92, 0x4c6e, 0x8c, 0xc2, 0xcd, 0x1b, 0x5, 0xc6, 0xb5, 0x45);
DECLARE_INTERFACE_(IEPGGrabber, IUnknown)
{
	STDMETHOD(GrabEPG)()PURE;
	STDMETHOD(IsEPGReady) (THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetEPGChannelCount) (THIS_ ULONG* channelCount)PURE;
	STDMETHOD(GetEPGEventCount) (THIS_ ULONG channel, ULONG* eventCount)PURE;
	STDMETHOD(GetEPGChannel) (THIS_ ULONG channel, WORD* networkId, WORD* transportid,WORD* service_id  )PURE;
	STDMETHOD(GetEPGEvent) (THIS_ ULONG channel, ULONG eventid,ULONG* language,ULONG* dateMJD,ULONG* timeUTC,ULONG* duration,char** genre    )PURE;
	STDMETHOD(GetEPGLanguage) (THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )PURE;
};

// {6F78D59C-1066-4e1b-8258-717F33C51F67}
DEFINE_GUID(IID_IMHWGrabber, 0x6f78d59c, 0x1066, 0x4e1b, 0x82, 0x58, 0x71, 0x7f, 0x33, 0xc5, 0x1f, 0x67);
DECLARE_INTERFACE_(IMHWGrabber, IUnknown)
{
	STDMETHOD(GrabMHW)()PURE;
	STDMETHOD(IsMHWReady) (THIS_ BOOL* yesNo)PURE;

	STDMETHOD(GetMHWTitleCount)(THIS_ WORD* count)PURE;
	STDMETHOD(GetMHWTitle)(THIS_ WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)PURE;
	STDMETHOD(GetMHWChannel)(THIS_ WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName)PURE;
	STDMETHOD(GetMHWSummary)(THIS_ WORD programId, char** summary)PURE;
	STDMETHOD(GetMHWTheme)(THIS_ WORD themeId, char** theme)PURE;

};

// {3921427B-72AC-4e4d-AF4F-518AFE1D0780}
DEFINE_GUID(IID_ATSCGrabber,0x3921427b, 0x72ac, 0x4e4d, 0xaf, 0x4f, 0x51, 0x8a, 0xfe, 0x1d, 0x7, 0x80);
DECLARE_INTERFACE_(IATSCGrabber, IUnknown)
{
	STDMETHOD(GrabATSC)()PURE;
	STDMETHOD(IsATSCReady) (THIS_ BOOL* yesNo)PURE;

	STDMETHOD(GetATSCTitleCount)(THIS_ WORD* count)PURE;
	STDMETHOD(GetATSCTitle)(THIS_ WORD no, WORD* source_id, ULONG* starttime, WORD* length_in_secs, char** title, char** description)PURE;
};

// Main filter object

class CStreamAnalyzerFilter : public CBaseFilter
{
    CStreamAnalyzer * const m_pDump;

public:

    // Constructor
    CStreamAnalyzerFilter(CStreamAnalyzer *pDump,
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

class CStreamAnalyzerSectionsPin : public CRenderedInputPin
{
    CStreamAnalyzer    * const m_pDump;           // Main renderer object
    CCritSec * const m_pReceiveLock;    // Sample critical section
    REFERENCE_TIME m_tLast;             // Last sample receive time
public:

    CStreamAnalyzerSectionsPin(CStreamAnalyzer *pDump,
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
    HRESULT CheckMediaType(const CMediaType *);
	HRESULT CompleteConnect(IPin *pPin);
    // Break connection
    HRESULT BreakConnect();

    // Track NewSegment
    STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);
	void ResetPids();

};


//  CStreamAnalyzer object which has filter and pin members

class CStreamAnalyzer : public CUnknown, public IStreamAnalyzer,public ISpecifyPropertyPages, public IEPGGrabber, public IMHWGrabber, public IATSCGrabber
{
    friend class CStreamAnalyzerFilter;
    friend class CStreamAnalyzerSectionsPin;

    CStreamAnalyzerFilter*		m_pFilter;       // Methods for filter interfaces
    CStreamAnalyzerSectionsPin*	m_pPin;          // A simple rendered input pin
	CMHWInputPin1*				m_pMHWPin1;          // A simple rendered input pin
	CMHWInputPin2*				m_pMHWPin2;
	CEPGInputPin*				m_pEPGPin;	
    CCritSec					m_Lock;                // Main renderer critical section
    CCritSec					m_ReceiveLock;         // Sublock for received samples

public:
    DECLARE_IUNKNOWN
    CStreamAnalyzer(LPUNKNOWN pUnk, HRESULT *phr);
    ~CStreamAnalyzer();
    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

	HRESULT 	 Process(BYTE *pbData,long len);
	HRESULT 	 ProcessEPG(BYTE *pbData,long len);
	STDMETHODIMP ResetPids();
    STDMETHODIMP get_IPin (IPin **ppPin) ;
    STDMETHODIMP put_MediaType(CMediaType *pmt);
    STDMETHODIMP get_MediaType(CMediaType **pmt);
	STDMETHODIMP get_State(FILTER_STATE *state);
	STDMETHODIMP GetChannelCount(WORD *count);
	STDMETHODIMP GetChannel(WORD channel,BYTE *ch);
	STDMETHODIMP ResetParser();
	STDMETHODIMP GetCISize(WORD *size);
	STDMETHODIMP SetPMTProgramNumber(ULONG prgNum);
	STDMETHODIMP GetPMTData(BYTE *pmtData);
	STDMETHODIMP GetPages(CAUUID *pPages);
	STDMETHODIMP IsChannelReady(ULONG channel);
	STDMETHODIMP GetLCN(WORD channel,WORD* networkId, WORD* transportId, WORD* serviceID, WORD* LCN);
	STDMETHODIMP SetPidFilterCallback(IHardwarePidFiltering* callback);
	STDMETHODIMP SetPMTCallback (IPMTCallback* callback);
	STDMETHODIMP Scanning(BOOL yesNo);
	//ATSC
	STDMETHODIMP UseATSC(BOOL yesNo);
	STDMETHODIMP IsATSCUsed(BOOL* yesNo);

	//EPG
	STDMETHODIMP GrabEPG();
	STDMETHODIMP IsEPGReady(BOOL* yesNo);
	STDMETHODIMP GetEPGChannelCount( ULONG* channelCount);
	STDMETHODIMP GetEPGEventCount( ULONG channel,  ULONG* eventCount);
	STDMETHODIMP GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	STDMETHODIMP GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    );
	STDMETHODIMP GetEPGLanguage(THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    );
	//MHW
	STDMETHODIMP GrabMHW();
	STDMETHODIMP IsMHWReady(BOOL* yesNo);

	STDMETHODIMP GetMHWTitleCount(WORD* count);
	STDMETHODIMP GetMHWTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	STDMETHODIMP GetMHWChannel(WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName);
	STDMETHODIMP GetMHWSummary(WORD programId, char** summary);
	STDMETHODIMP GetMHWTheme(WORD themeId, char** theme);

	//ATSC
	STDMETHODIMP GrabATSC();
	STDMETHODIMP IsATSCReady (BOOL* yesNo);
	STDMETHODIMP GetATSCTitleCount(WORD* count);
	STDMETHODIMP GetATSCTitle(WORD no, WORD* source_id, ULONG* starttime, WORD* length_in_secs, char** title, char** description);

public:
	HRESULT NotifyFinished(int EVENT);
	Sections*				m_pSections;
	SplitterSetup*			m_pDemuxer;
	ChannelInfo				m_patTable[512];
	int						m_patChannelsCount;
	ULONG					m_pmtGrabProgNum;
	BYTE					m_pmtGrabData[4096];
	long					m_currentPMTLen;
	BOOL					m_bDecodeATSC;
	ATSCParser*				m_pAtscParser;
	IHardwarePidFiltering*  m_pCallback;
	IPMTCallback* m_pPMTCallback;
private:
    // Overriden to say what interfaces we support where
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
	HRESULT		   MapSectionPids();
	HRESULT		   MapPMTPids(int count, int* pids);
	bool		   m_bReset;
	BOOL m_bScanning;

    // Open and write to the file
};
#endif