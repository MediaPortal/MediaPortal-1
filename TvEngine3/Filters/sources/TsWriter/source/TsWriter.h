/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include "packetsync.h"
#include "multiplexer.h"
#include "videoanalyzer.h"
#include "channelscan.h"
#include "epgscanner.h"
#include "pmtgrabber.h"
#include "recorder.h"
#include "timeshifting.h"
#include "teletextgrabber.h"
#include "cagrabber.h"
#include "technotrend.h"
#include "tschannel.h"

#include <map>
#include <vector>
using namespace std;

class CMpTsFilterPin;
class CMpTs;
class CMpTsFilter;

DEFINE_GUID(CLSID_MpTsFilter, 0xfc50bed6, 0xfe38, 0x42d3, 0xb8, 0x31, 0x77, 0x16, 0x90, 0x9, 0x1a, 0x6e);

// {5EB9F392-E7FD-4071-8E44-3590E5E767BA}
DEFINE_GUID(IID_TSFilter, 0x5eb9f392, 0xe7fd, 0x4071, 0x8e, 0x44, 0x35, 0x90, 0xe5, 0xe7, 0x67, 0xba);

DECLARE_INTERFACE_(ITSFilter, IUnknown)
{
  STDMETHOD(AddChannel)(THIS_ ITSChannel** instance)PURE;
  STDMETHOD(DeleteChannel)(THIS_ ITSChannel* instance)PURE;
  STDMETHOD(GetChannel)(THIS_ int index, ITSChannel** instance)PURE;
  STDMETHOD(GetChannelCount)(THIS_ int* count)PURE;
  STDMETHOD(DeleteAllChannels)()PURE;
};

// Main filter object

class CMpTsFilter : public CBaseFilter
{
    CMpTs * const m_pWriterFilter;

public:

    // Constructor
    CMpTsFilter(CMpTs *pDump,LPUNKNOWN pUnk,CCritSec *pLock,HRESULT *phr);

    // Pin enumeration
    CBasePin * GetPin(int n);
    int GetPinCount();

    // Open and close the file as necessary
    STDMETHODIMP Run(REFERENCE_TIME tStart);
    STDMETHODIMP Pause();
    STDMETHODIMP Stop();
};


//  Pin object

class CMpTsFilterPin : public CRenderedInputPin,public CPacketSync
{
    CMpTs*	const	m_pWriterFilter;   // Main renderer object
    CCritSec*		const	m_pReceiveLock;    // Sample critical section
public:

    CMpTsFilterPin(CMpTs *pDump,LPUNKNOWN pUnk,CBaseFilter *pFilter,CCritSec *pLock,CCritSec *pReceiveLock,HRESULT *phr);

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

		//CPacketSync overrides
		void OnTsPacket(byte* tsPacket);
private:
	CCritSec		m_section;
};


//  CMpTs object which has filter and pin members

class CMpTs : public CUnknown, public ITSFilter
{

    friend class CMpTsFilter;
    friend class CMpTsFilterPin;
    CMpTsFilter*	m_pFilter;       // Methods for filter interfaces
    CMpTsFilterPin*	m_pPin;          // A simple rendered input pin
    CCritSec 		m_Lock;                // Main renderer critical section
    CCritSec 		m_ReceiveLock;         // Sublock for received samples
public:
    DECLARE_IUNKNOWN

    STDMETHODIMP AddChannel( ITSChannel** instance);
    STDMETHODIMP DeleteChannel( ITSChannel* instance);
    STDMETHODIMP GetChannel( int index, ITSChannel** instance);
    STDMETHODIMP GetChannelCount( int* count);
    STDMETHODIMP DeleteAllChannels();

    CMpTs(LPUNKNOWN pUnk, HRESULT *phr);
    ~CMpTs();
    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);
		void AnalyzeTsPacket(byte* tsPacket);

private:
    // Overriden to say what interfaces we support where
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
		CChannelScan*   m_pChannelScanner;
		CEpgScanner*		m_pEpgScanner;
    CTechnotrend*   m_pTechnoTrend;
    vector<CTsChannel*> m_vecChannels;
    typedef vector<CTsChannel*>::iterator ivecChannels;
};
