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
#include "mhwinputpin2.h"


extern void Log(const char *fmt, ...) ;
CMHWInputPin2::CMHWInputPin2(CStreamAnalyzer *pDump,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CMHWInputPin2"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"InputD3"),                 // Pin name
    m_pReceiveLock(pReceiveLock),
    m_pDump(pDump),
    m_tLast(0)
{
	ResetPids();
	m_bGrabMHW=false;
}

//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CMHWInputPin2::CheckMediaType(const CMediaType *pmt)
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
HRESULT CMHWInputPin2::BreakConnect()
{
//	Log("mhwpin2:BreakConnect()");
    return CRenderedInputPin::BreakConnect();
}

HRESULT CMHWInputPin2::CompleteConnect(IPin *pPin)
{
//	Log("mhwpin2:CompleteConnect()");
	HRESULT hr=CBasePin::CompleteConnect(pPin);
	m_pDump->OnConnectMHW2();

	return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CMHWInputPin2::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMHWInputPin2::Receive(IMediaSample *pSample)
{
	if (m_bReset)
	{
		Log("mhw2:reset");
		m_bReset=false;
		m_bParsed=false;
		m_MHWParser.Reset();

		m_tableGrabber90.Reset();
		m_tableGrabber90.SetTableId(0xd3,0x90);
		
		m_tableGrabber91.Reset();
		m_tableGrabber91.SetTableId(0xd3,0x91);
		
		m_tableGrabber92.Reset();
		m_tableGrabber92.SetTableId(0xd3,0x92);
	}
	if (!m_bGrabMHW) return S_OK;
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
        return hr;
    }
	
	lDataLen=pSample->GetActualDataLength();
	// decode
	// decode
	if(lDataLen>5)
	{

		//Log("mhw2:OnPacket()");
		m_tableGrabber90.OnPacket(pbData,lDataLen);
		m_tableGrabber91.OnPacket(pbData,lDataLen);
		m_tableGrabber92.OnPacket(pbData,lDataLen);
		//Log("mhw2:OnPacket() done");
	}

    return S_OK;
}

void CMHWInputPin2::ResetPids()
{
	m_bReset=true;
	m_bGrabMHW=false;
}

//
// EndOfStream
//
STDMETHODIMP CMHWInputPin2::EndOfStream(void)
{
//	Log("mhwpin2:EndOfStream()");    
	CAutoLock lock(m_pReceiveLock);
	ResetPids();
    return CRenderedInputPin::EndOfStream();

} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CMHWInputPin2::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
    m_tLast = 0;
    return S_OK;

} // NewSegment


bool CMHWInputPin2::IsReady()
{
	if (m_tableGrabber90.IsSectionGrabbed() && 
		m_tableGrabber91.IsSectionGrabbed() && 
		m_tableGrabber92.IsSectionGrabbed() ) 
	{
		Parse();
		return true;
	}
//	Log("mhwpin2: t90:%d t91:%d t92:%d",m_tableGrabber90.IsSectionGrabbed(),m_tableGrabber91.IsSectionGrabbed(),m_tableGrabber92.IsSectionGrabbed());
	return false;
}

bool CMHWInputPin2::IsParsed()
{
	return m_bParsed;
}
void CMHWInputPin2::Parse()
{
	if (m_bParsed) return;
	//Log("mhwpin2: parse()");
	CAutoLock lock(&m_Lock);
	m_bParsed=true;
	m_bGrabMHW=false;
	//parse summaries
	//Log("MHW2: parse summaries:%d",m_tableGrabber90.Count());
	for (int i=0; i < m_tableGrabber90.Count();++i)
	{
		try
		{
			m_MHWParser.ParseSummaries(m_tableGrabber90.GetTable(i), m_tableGrabber90.GetTableLen(i));
		}
		catch(...)
		{
			Log("MHW:exception MHW2 ParseSummaries table:%d", i);
		}
	}

	//parse channels
	//Log("MHW2: parse channels:%d",m_tableGrabber91.Count());
	for (int i=0; i < m_tableGrabber91.Count();++i)
	{
		try
		{
			m_MHWParser.ParseChannels(m_tableGrabber91.GetTable(i), m_tableGrabber91.GetTableLen(i));
		}
		catch(...)
		{
			Log("MHW:exception MHW2 ParseChannels table:%d", i);
		}
	}

	//parse themes
	//Log("MHW2: parse themes:%d",m_tableGrabber92.Count());
	for (int i=0; i < m_tableGrabber92.Count();++i)
	{
		try
		{
			m_MHWParser.ParseThemes(m_tableGrabber92.GetTable(i), m_tableGrabber92.GetTableLen(i));
		}
		catch(...)
		{
			Log("MHW:exception MHW2 ParseThemes table:%d", i);
		}
	}
	//Log("MHW2:parse done()");
}

void CMHWInputPin2::GrabMHW()
{
	m_bGrabMHW=true;
	ResetPids();
}