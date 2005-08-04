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
#include "mhwinputpin1.h"

extern void Log(const char *fmt, ...) ;
CMHWInputPin1::CMHWInputPin1(CStreamAnalyzer *pDump,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CMHWInputPin1"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"InputD2"),                 // Pin name
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
HRESULT CMHWInputPin1::CheckMediaType(const CMediaType *pmt)
{
	if(pmt->majortype==MEDIATYPE_MPEG2_SECTIONS)
		return S_OK;
	return S_FALSE;
}

//
// BreakConnect
//
// Break a connection
//
HRESULT CMHWInputPin1::BreakConnect()
{
	Log("mhwpin1:CompleteConnect()");
    return CRenderedInputPin::BreakConnect();
}

HRESULT CMHWInputPin1::CompleteConnect(IPin *pPin)
{
	Log("mhwpin1:CompleteConnect()");
	HRESULT hr=CBasePin::CompleteConnect(pPin);
	m_pDump->OnConnectMHW1();
	return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CMHWInputPin1::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMHWInputPin1::Receive(IMediaSample *pSample)
{
    CheckPointer(pSample,E_POINTER);

    //CAutoLock lock(m_pReceiveLock);
    PBYTE pbData;

    // Has the filter been stopped yet?

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    m_tLast = tStart;
	long lDataLen=0;

    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) {
        return hr;
    }
	
	lDataLen=pSample->GetActualDataLength();

	// decode
	if(lDataLen>5)
	{
	//	Log("mhwpin1: packet:(%x)", pbData[0]);
		m_tableGrabber.OnPacket(pbData,lDataLen);
	}
    return S_OK;
}


void CMHWInputPin1::ResetPids()
{
//	Log("mhwpin1: ResetPids()");
//	Parse();//test
	m_bParsed=false;
	m_MHWParser.Reset();
	m_tableGrabber.Reset();
	m_tableGrabber.SetTableId(0xd2,0x90);
}

//
// EndOfStream
//
STDMETHODIMP CMHWInputPin1::EndOfStream(void)
{
	Log("mhwpin1: EndOfStream()");
    CAutoLock lock(m_pReceiveLock);
	ResetPids();
    return CRenderedInputPin::EndOfStream();

} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CMHWInputPin1::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
    m_tLast = 0;
    return S_OK;

} // NewSegment

bool CMHWInputPin1::IsParsed()
{
	return m_bParsed;
}
bool CMHWInputPin1::IsReady()
{
	if (m_tableGrabber.IsSectionGrabbed() ) 
	{
		Parse();
		return true;
	}
	//Log("mhwpin1: IsReady():%d",m_tableGrabber.IsSectionGrabbed());
	return false;
}
void CMHWInputPin1::Parse()
{
	if (m_bParsed) return;
		
	Log("mhwpin1: parse()");
	m_bParsed=true;
	//parse summaries
	Log("MHW2: parse summaries:%d",m_tableGrabber.Count());
	for (int i=0; i < m_tableGrabber.Count();++i)
	{
		try
		{
			m_MHWParser.ParseTitles(m_tableGrabber.GetTable(i), m_tableGrabber.GetTableLen(i));
		}
		catch(...)
		{
			Log("MHW:exception MHW1 parse title table:%d", i);
		}
	}
}