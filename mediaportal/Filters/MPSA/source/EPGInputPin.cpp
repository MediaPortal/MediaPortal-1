/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Agree
 *	http://mediaportal.sourceforge.net
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

#include <windows.h>
#include <commdlg.h>
#include <streams.h>
#include <initguid.h>
#include "Section.h"
#include "MPSA.h"
#include "SplitterSetup.h"
#include "proppage.h"
#include "epginputpin.h"

extern void Log(const char *fmt, ...) ;

CEPGInputPin::CEPGInputPin(CStreamAnalyzer *pDump,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CEPGInputPin"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"EPG"),                 // Pin name
    m_pReceiveLock(pReceiveLock),
    m_pDump(pDump),
    m_tLast(0)
{
	ResetPids();
}

//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CEPGInputPin::CheckMediaType(const CMediaType *pmt)
{
//	Log("epg:CheckMediaType()");
	if(pmt->majortype==MEDIATYPE_MPEG2_SECTIONS  )
		return S_OK;
	return S_FALSE;
}

//
// BreakConnect
//
// Break a connection
//
HRESULT CEPGInputPin::BreakConnect()
{
//	Log("epg:BreakConnect()");
    return CRenderedInputPin::BreakConnect();
}

HRESULT CEPGInputPin::CompleteConnect(IPin *pPin)
{
//	Log("epg:CompleteConnect()");
	HRESULT hr=CBasePin::CompleteConnect(pPin);
	m_pDump->OnConnectEPG();
	return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CEPGInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CEPGInputPin::Receive(IMediaSample *pSample)
{
	if (m_bReset)
	{
		Log("epg:reset");
		m_bReset=false;
		m_pDump->m_pSections->ResetEPG();
	}
	//Log("epg:Receive()");
    CheckPointer(pSample,E_POINTER);

//    CAutoLock lock(m_pReceiveLock);
    PBYTE pbData=NULL;

    // Has the filter been stopped yet?

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    m_tLast = tStart;
	long lDataLen=0;

    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) {
		Log("epg:Receive() err");
        return hr;
    }
	
	lDataLen=pSample->GetActualDataLength();
	// decode
	if(lDataLen>5)
		m_pDump->ProcessEPG(pbData,lDataLen);

	//Log("epgpin:Receive() done");
    return S_OK;
}

void CEPGInputPin::ResetPids()
{
	m_bReset=true;
}

//
// EndOfStream
//
STDMETHODIMP CEPGInputPin::EndOfStream(void)
{
//	Log("epg:EndOfStream()");
    CAutoLock lock(m_pReceiveLock);
    return CRenderedInputPin::EndOfStream();

} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CEPGInputPin::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
//	Log("epg:NewSegment()");
    m_tLast = 0;
    return S_OK;

} // NewSegment

