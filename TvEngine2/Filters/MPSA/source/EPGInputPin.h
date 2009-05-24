#pragma once
#pragma warning(disable: 4511 4512 4995)
#include "Section.h"
#include "SplitterSetup.h"
#include "EPGParser.h"

class CEPGInputPin :
	public CRenderedInputPin
{
private:
    CStreamAnalyzer* const m_pDump;           // Main renderer object
    CCritSec * const	m_pReceiveLock;    // Sample critical section
    REFERENCE_TIME		m_tLast;             // Last sample receive time
	bool				m_bReset;
public:

    CEPGInputPin(CStreamAnalyzer *pDump,
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

public:
	void GrabEPG();
	bool isGrabbing();
	bool	IsEPGReady();
	ULONG	GetEPGChannelCount( );
	ULONG	GetEPGEventCount( ULONG channel);
	void	GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	void	GetEPGEvent( ULONG channel,  ULONG event,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre    );
	void    GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription    );

private:
	HRESULT ProcessEPG(BYTE *pbData,long len);
	CEPGParser m_parser;
};
