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
#include "VideoInputPin.h"

extern void LogDebug(const char *fmt, ...) ;

//
//  Definition of CTsMuxerVideoInputPin
//
CTsMuxerVideoInputPin::CTsMuxerVideoInputPin(IPacketReceiver *pTsMuxer,
													   LPUNKNOWN pUnk,
													   CBaseFilter *pFilter,
													   CCritSec *pLock,
													   CCritSec *pReceiveLock,
													   HRESULT *phr) :

CRenderedInputPin(NAME("CTsMuxerVideoInputPin"),
				  pFilter,                   // Filter
				  pLock,                     // Locking
				  phr,                       // Return code
				  L"Video"),           // Pin name
				  m_pReceiveLock(pReceiveLock),
				  m_pTsMuxer(pTsMuxer)
{
	LogDebug("CTsMuxerVideoInputPin:ctor");

	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CTsMuxerVideoInputPin::CheckMediaType(const CMediaType *pType)
{
	if(MEDIATYPE_Video == pType->majortype && MEDIASUBTYPE_MPEG2_VIDEO == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Video == pType->majortype && MEDIASUBTYPE_MPEG1Payload == pType->subtype){
		return S_OK;
	}

	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG2_VIDEO == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG1Video == pType->subtype){
		return S_OK;
	}
	return S_FALSE;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CTsMuxerVideoInputPin::BreakConnect()
{

	return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CTsMuxerVideoInputPin::ReceiveCanBlock()
{
	return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CTsMuxerVideoInputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("Video: receive sample=null");
			return S_OK;
		}

		//		CheckPointer(pSample,E_POINTER);
		//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{

			LogDebug("Video: receive samplelen:%d",sampleLen);
			return S_OK;
		}

		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("Video: receive cannot get samplepointer");
			return S_OK;
		}
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("Video: got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}
		m_pTsMuxer->WriteVideo(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("Video: receive exception");
	}
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CTsMuxerVideoInputPin::EndOfStream(void)
{
	CAutoLock lock(m_pReceiveLock);
	return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CTsMuxerVideoInputPin::Reset()
{
	LogDebug("Video: Reset()...");
	m_bIsReceiving=FALSE;
	m_lTickCount=0;
}
BOOL CTsMuxerVideoInputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("Video: lost signal...");
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
STDMETHODIMP CTsMuxerVideoInputPin::NewSegment(REFERENCE_TIME tStart,
													REFERENCE_TIME tStop,
													double dRate)
{
	return S_OK;

} // NewSegment

HRESULT CTsMuxerVideoInputPin::GetMediaType(int iPosition,CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pLock);
	if(iPosition < 0) return E_INVALIDARG;
	if(iPosition > 3) return VFW_S_NO_MORE_ITEMS;

	pmt->ResetFormatBuffer();
	pmt->InitMediaType();
	pmt->formattype = FORMAT_None;

	if(iPosition == 0){
		pmt->majortype = MEDIATYPE_Video;
		pmt->subtype = MEDIASUBTYPE_MPEG2_VIDEO;
	}
	if(iPosition == 1){
		pmt->majortype = MEDIATYPE_Video;
		pmt->subtype = MEDIASUBTYPE_MPEG1Payload;
	}
	if(iPosition == 2){
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG2_VIDEO;
	}
	if(iPosition == 3){
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG1Video;
	}
	return S_OK;
}




