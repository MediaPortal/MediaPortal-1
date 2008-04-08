/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: Agree
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
#pragma warning(disable: 4786)

#include <windows.h>
#include <commdlg.h>
#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include <commctrl.h>
#include <time.h>

#include "Section.h"
#include "MPSA.h"
#include "SplitterSetup.h"
#include "proppage.h"
#include "mhwinputpin1.h"

extern void Log(const char *fmt, ...) ;
extern void Dump(const char *fmt, ...) ;
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
		
	timeoutTimer=time(NULL);

	ResetPids();
		
	m_bGrabMHW=false;
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
//	Log("mhwpin1:CompleteConnect()");
    return CRenderedInputPin::BreakConnect();
}

HRESULT CMHWInputPin1::CompleteConnect(IPin *pPin)
{
//	Log("mhwpin1:CompleteConnect()");
	HRESULT hr=CBasePin::CompleteConnect(pPin);

	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;

	//setup demuxer to map pid 0xd2
	hr=pPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(SUCCEEDED(hr) && pMap!=NULL)
	{
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(SUCCEEDED(hr) && pPidEnum!=NULL)
		{
			while(pPidEnum->Next(1,&pm,&count)== S_OK)
			{
				if (count!=1) break;
					
				umPid=pm.ulPID;
				hr=pMap->UnmapPID(1,&umPid);
				if(FAILED(hr))
				{	
					break;
				}
			}
			pid = (ULONG)0xd2;// EIT
			hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv

			pPidEnum->Release();
		}
		pMap->Release();
	}
	return hr;
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CMHWInputPin1::ReceiveCanBlock()
{
    return S_OK;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMHWInputPin1::Receive(IMediaSample *pSample)
{
	try
	{
		if (m_bReset)
		{
			Log("mhw1:reset");
			m_bReset=false;
			m_bParsed=false;
			m_MHWParser.Reset();
			timeoutTimer=time(NULL);
		}
		if (!m_bGrabMHW) return S_OK; //test
		CheckPointer(pSample,E_POINTER);

		//CAutoLock lock(m_pReceiveLock);
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
		if(lDataLen>11)
		{
			if (pbData[0]==0x90 && (pbData[1] >=0x70 && pbData[1] <=0x7f) )
			{
				if ( m_MHWParser.ParseTitles(pbData,lDataLen))
				{
					timeoutTimer=time(NULL);
				}
			}
		}
		
		int passed=time(NULL)-timeoutTimer;
		if (passed>30)
		{
			Parse();
		}
	}
	catch(...)
	{
		Dump("mhw1pin:--- UNHANDLED EXCEPTION ---");
	}
    return S_OK;
}


void CMHWInputPin1::ResetPids()
{
	m_bReset=true;
}

//
// EndOfStream
//
STDMETHODIMP CMHWInputPin1::EndOfStream(void)
{
//	Log("mhwpin1: EndOfStream()");
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

bool CMHWInputPin1::IsReady()
{
	int passed=time(NULL)-timeoutTimer;
	if (passed>30)
	{
		Parse();
	}
	return m_bParsed;
}
void CMHWInputPin1::Parse()
{
	Log("MHW1:timeout detected");
	m_bParsed=true;
	m_bGrabMHW=false;
}
void CMHWInputPin1::GrabMHW()
{
	Log("MHW1:Grab");
	m_bGrabMHW=true;
	ResetPids();
	timeoutTimer=time(NULL);
}
bool CMHWInputPin1::isGrabbing()
{
	return m_bGrabMHW;
}
