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
#include "mhwinputpin2.h"


extern void Log(const char *fmt, ...) ;
extern void Dump(const char *fmt, ...) ;
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
	timeoutTimer=time(NULL);

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

	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;

	//setup demuxer to map pid 0xd3
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
			pid = (ULONG)0xd3;// EIT
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
STDMETHODIMP CMHWInputPin2::ReceiveCanBlock()
{
    return S_OK;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CMHWInputPin2::Receive(IMediaSample *pSample)
{
	try
	{
		if (m_bReset)
		{
			Log("mhw2:reset");
			m_bReset=false;
			m_bParsed=false;
			m_MHWParser.Reset();
			timeoutTimer=time(NULL);
		}
		if (!m_bGrabMHW) return S_OK; //test
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
			if (pbData[0]==0x90)
			{
				if (m_MHWParser.ParseSummaries(pbData,lDataLen))
				{
					timeoutTimer=time(NULL);
				}
			}
			if (pbData[0]==0x91)
			{
				if (m_MHWParser.ParseChannels(pbData,lDataLen))
				{
					timeoutTimer=time(NULL);
				}
			}
			if (pbData[0]==0x92)
			{
				if (m_MHWParser.ParseThemes(pbData,lDataLen))
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
		Dump("mhw2pin:--- UNHANDLED EXCEPTION ---");
	}
    return S_OK;
}

void CMHWInputPin2::ResetPids()
{
	m_bReset=true;
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
	int passed=time(NULL)-timeoutTimer;
	if (passed>30)
	{
		Parse();
	}
	return m_bParsed;
}

void CMHWInputPin2::Parse()
{
	Log("MHW2:timeout detected");
	m_bParsed=true;
	m_bGrabMHW=false;
}

void CMHWInputPin2::GrabMHW()
{
	Log("MHW2:Grab");
	m_bGrabMHW=true;
	ResetPids();
	timeoutTimer=time(NULL);
}
bool CMHWInputPin2::isGrabbing()
{
	return m_bGrabMHW;
}
