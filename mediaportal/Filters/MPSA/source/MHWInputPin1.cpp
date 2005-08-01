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
	m_tableGrabber.Reset();
	m_tableGrabber.SetTableId(0xd2,0x90);

	m_tableGrabber90.Reset();
	m_tableGrabber90.SetTableId(0xd3,0x90);
	
	m_tableGrabber91.Reset();
	m_tableGrabber91.SetTableId(0xd3,0x91);
	
	m_tableGrabber92.Reset();
	m_tableGrabber92.SetTableId(0xd3,0x92);
}

//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CMHWInputPin1::CheckMediaType(const CMediaType *pmt)
{
	if(pmt->majortype==MEDIATYPE_Stream)
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

    CAutoLock lock(m_pReceiveLock);
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
		if (!m_tableGrabber.IsSectionGrabbed())
		{
			m_tableGrabber.OnPacket(pbData,lDataLen);
			
			if (m_tableGrabber.IsSectionGrabbed())
			{
				//parse titles
				for (int i=0; i < m_tableGrabber.Count();++i)
				{
					m_MHWParser.ParseTitles(m_tableGrabber.GetTable(i), m_tableGrabber.GetTableLen(i));
				}
			}
		}
		if (!m_tableGrabber90.IsSectionGrabbed())
		{
			m_tableGrabber90.OnPacket(pbData,lDataLen);
			
			if (m_tableGrabber90.IsSectionGrabbed())
			{
				//parse summaries
				for (int i=0; i < m_tableGrabber90.Count();++i)
				{
					m_MHWParser.ParseSummaries(m_tableGrabber90.GetTable(i), m_tableGrabber90.GetTableLen(i));
				}
			}
		}
		if (!m_tableGrabber91.IsSectionGrabbed())
		{
			m_tableGrabber91.OnPacket(pbData,lDataLen);
			
			if (m_tableGrabber91.IsSectionGrabbed())
			{	
				//parse channels
				for (int i=0; i < m_tableGrabber91.Count();++i)
				{
					m_MHWParser.ParseChannels(m_tableGrabber91.GetTable(i), m_tableGrabber91.GetTableLen(i));
				}
			}
		}
		if (!m_tableGrabber92.IsSectionGrabbed())
		{
			m_tableGrabber92.OnPacket(pbData,lDataLen);
			
			if (m_tableGrabber92.IsSectionGrabbed())
			{
				//parse themes
				for (int i=0; i < m_tableGrabber92.Count();++i)
				{
					m_MHWParser.ParseThemes(m_tableGrabber92.GetTable(i), m_tableGrabber92.GetTableLen(i));
				}
			}
		}
	}
    return NOERROR;
}
void CMHWInputPin1::ResetPids()
{
	m_tableGrabber.Reset();
	m_tableGrabber.SetTableId(0xd2,0x90);

	m_tableGrabber90.Reset();
	m_tableGrabber90.SetTableId(0xd3,0x90);
	
	m_tableGrabber91.Reset();
	m_tableGrabber91.SetTableId(0xd3,0x91);
	
	m_tableGrabber92.Reset();
	m_tableGrabber92.SetTableId(0xd3,0x92);
}

//
// EndOfStream
//
STDMETHODIMP CMHWInputPin1::EndOfStream(void)
{
    CAutoLock lock(m_pReceiveLock);
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

