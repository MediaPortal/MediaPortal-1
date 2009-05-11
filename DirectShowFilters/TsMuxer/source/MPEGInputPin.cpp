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

#include <windows.h>
#include "MPEGInputPin.h"

extern void LogDebug(const char *fmt, ...) ;

//
//  Definition of CTsMuxerMPEGInputPin
//
CTsMuxerMPEGInputPin::CTsMuxerMPEGInputPin(IPacketReceiver *pTsMuxer,
													   LPUNKNOWN pUnk,
													   CBaseFilter *pFilter,
													   CCritSec *pLock,
													   CCritSec *pReceiveLock,
													   HRESULT *phr) :

CRenderedInputPin(NAME("CTsMuxerMPEGInputPin"),
				  pFilter,                   // Filter
				  pLock,                     // Locking
				  phr,                       // Return code
				  L"MPEG"),           // Pin name
				  m_pReceiveLock(pReceiveLock),
				  m_pTsMuxer(pTsMuxer)
{
	LogDebug("CTsMuxerMPEGInputPin:ctor");

	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CTsMuxerMPEGInputPin::CheckMediaType(const CMediaType *pType)
{
	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG1System == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG2_PROGRAM == pType->subtype){
		return S_OK;
	}
	return S_FALSE;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CTsMuxerMPEGInputPin::BreakConnect()
{

	return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CTsMuxerMPEGInputPin::ReceiveCanBlock()
{
	return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CTsMuxerMPEGInputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("MPEG2: receive sample=null");
			return S_OK;
		}

		//		CheckPointer(pSample,E_POINTER);
		//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{

			LogDebug("MPEG2: receive samplelen:%d",sampleLen);
			return S_OK;
		}

		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("MPEG2: receive cannot get samplepointer");
			return S_OK;
		}
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("MPEG2: got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}
		m_pTsMuxer->WriteProgram(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("MPEG2: receive exception");
	}
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CTsMuxerMPEGInputPin::EndOfStream(void)
{
	CAutoLock lock(m_pReceiveLock);
	return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CTsMuxerMPEGInputPin::Reset()
{
	LogDebug("MPEG2: Reset()...");
	m_bIsReceiving=FALSE;
	m_lTickCount=0;
}
BOOL CTsMuxerMPEGInputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("MPEG2: lost signal...");
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
STDMETHODIMP CTsMuxerMPEGInputPin::NewSegment(REFERENCE_TIME tStart,
													REFERENCE_TIME tStop,
													double dRate)
{
	return S_OK;

} // NewSegment

HRESULT CTsMuxerMPEGInputPin::GetMediaType(int iPosition,CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pLock);
	if(iPosition < 0) return E_INVALIDARG;
	if(iPosition > 1) return VFW_S_NO_MORE_ITEMS;

	pmt->ResetFormatBuffer();
	pmt->InitMediaType();
	pmt->formattype = FORMAT_None;

	if(iPosition == 0){
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG1System;
	}
	if(iPosition == 1){
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG2_PROGRAM;
	}
	return S_OK;
}




