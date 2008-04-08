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

#include "Section.h"
#include "MPSA.h"
#include "SplitterSetup.h"
#include "proppage.h"
#include "epginputpin.h"

extern void Log(const char *fmt, ...) ;
extern void Dump(const char *fmt, ...) ;
#define S_FINISHED (S_OK+1)

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
	
	//m_pDump->OnConnectEPG();
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	

	//setup demuxer to map pid 12
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
			pid = (ULONG)0x12;// EIT
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
STDMETHODIMP CEPGInputPin::ReceiveCanBlock()
{
    return S_OK;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CEPGInputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (m_bReset)
		{
			Log("epgpin:reset");
			m_bReset=false;
			m_parser.ResetEPG();
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
			Log("epgpin:Receive() err");
			return hr;
		}
		
		lDataLen=pSample->GetActualDataLength();
		// decode
		if(lDataLen>5)
		{
			ProcessEPG(pbData,lDataLen);
		}
		//Log("epgpin:Receive() done");
	}
	catch(...)
	{
		Dump("epgpin:--- UNHANDLED EXCEPTION ---");
	}
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


HRESULT CEPGInputPin::ProcessEPG(BYTE *pbData,long len)
{
	if (pbData==NULL) return S_OK;
	if (len <=3) return S_OK;
	try
	{
		if(pbData[0]==0x00 && pbData[1]==0x00 && pbData[2]==0x01)
		{
			//PES PACKET
			return S_OK;
		}
		
		if (pbData[0]==0x4e || pbData[0]==0x4f || (pbData[0] >= 0x50  && pbData[0] <= 0x6f)) //EPG
		{
			//Log("Got one");
			if (m_parser.IsEPGGrabbing())
			{
				//Log("mpsa::decode EPG");
				HRESULT hr;
				hr = m_parser.DecodeEPG(pbData,len);

				if(hr == S_FINISHED){
					//Log("mpsa::send_event");
					m_pDump->NotifyFinished(EC_GUIDE_CHANGED);
				}
				//Log("mpsa::decode EPG done");
			}
		}
	}
	catch(...)
	{
		Log("epgpin:--- PROCESSEPG UNHANDLED EXCEPTION ---");
	}
	return S_OK;
}



bool CEPGInputPin::isGrabbing()
{
	return m_parser.IsEPGGrabbing();
}

void CEPGInputPin::GrabEPG()
{
	m_parser.GrabEPG();
}
ULONG CEPGInputPin::GetEPGChannelCount( )
{
	return m_parser.GetEPGChannelCount( );
}
ULONG CEPGInputPin::GetEPGEventCount( ULONG channel)
{	
	return m_parser.GetEPGEventCount( channel);
}
void CEPGInputPin::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	 m_parser.GetEPGChannel( channel,  networkId,  transportid, service_id  );
}
void CEPGInputPin::GetEPGEvent( ULONG channel,  ULONG event,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre    )
{
	m_parser.GetEPGEvent( channel,  event,language, dateMJD, timeUTC, duration, strgenre    );
}
void CEPGInputPin::GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription    )
{
	m_parser.GetEPGLanguage(channel, eventid,languageIndex,language, eventText, eventDescription    );
}

bool CEPGInputPin::IsEPGReady()
{
	return m_parser.IsEPGReady();
}

