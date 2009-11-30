/* 
*	Copyright (C) 2006-2008 Team MediaPortal
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

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include "TeletextInputPin.h"

extern void LogDebug(const char *fmt, ...) ;

//
//  Definition of CTsMuxerTeletextInputPin
//
CTsMuxerTeletextInputPin::CTsMuxerTeletextInputPin(IPacketReceiver *pTsMuxer,
															 LPUNKNOWN pUnk,
															 CBaseFilter *pFilter,
															 CCritSec *pLock,
															 CCritSec *pReceiveLock,
															 HRESULT *phr) :

CRenderedInputPin(NAME("CTsMuxerTeletextInputPin"),
				  pFilter,                   // Filter
				  pLock,                     // Locking
				  phr,                       // Return code
				  L"Teletext Input"),        // Pin name
				  m_pReceiveLock(pReceiveLock),
				  m_pTsMuxer(pTsMuxer)
{
	LogDebug("CTsMuxerTeletextInputPin:ctor");

	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CTsMuxerTeletextInputPin::CheckMediaType(const CMediaType *pType)
{
	if(MEDIATYPE_VBI == pType->majortype && MEDIASUBTYPE_TELETEXT == pType->subtype){
		return S_OK;
	}
	return S_FALSE;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CTsMuxerTeletextInputPin::BreakConnect()
{

	return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CTsMuxerTeletextInputPin::ReceiveCanBlock()
{
	return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CTsMuxerTeletextInputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("TELETEXT: receive sample=null");
			return S_OK;
		}

		//		CheckPointer(pSample,E_POINTER);
		//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{
			return S_OK;
		}

		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("TELETEXT: receive cannot get samplepointer");
			return S_OK;
		}
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("TELETEXT: got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}

		//m_pTsMuxer->WriteTeletext(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("TELETEXT: receive exception");
	}
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CTsMuxerTeletextInputPin::EndOfStream(void)
{
	CAutoLock lock(m_pReceiveLock);
	return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CTsMuxerTeletextInputPin::Reset()
{
	LogDebug("TELETEXT: Reset()...");
	m_bIsReceiving=FALSE;
	m_lTickCount=0;
}
BOOL CTsMuxerTeletextInputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("TELETEXT: lost signal...");
		}
		m_bIsReceiving=FALSE;
	}
	return m_bIsReceiving;
}
//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CTsMuxerTeletextInputPin::NewSegment(REFERENCE_TIME tStart,
													   REFERENCE_TIME tStop,
													   double dRate)
{
	return S_OK;

} // NewSegment

HRESULT CTsMuxerTeletextInputPin::GetMediaType(int iPosition,CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pLock);
	if(iPosition < 0) return E_INVALIDARG;
	if(iPosition > 0) return VFW_S_NO_MORE_ITEMS;

	pmt->ResetFormatBuffer();
	pmt->InitMediaType();
	pmt->majortype = MEDIATYPE_VBI;
	pmt->subtype = MEDIASUBTYPE_TELETEXT;
	pmt->formattype = FORMAT_None;

	return S_OK;
}

