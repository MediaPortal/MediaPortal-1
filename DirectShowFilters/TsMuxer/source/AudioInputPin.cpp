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
#include "AudioInputPin.h"
extern void LogDebug(const char *fmt, ...) ;

//
//  Definition of CTsMuxerAudioInputPin
//
CTsMuxerAudioInputPin::CTsMuxerAudioInputPin(IPacketReceiver *pTsMuxer,
											 LPUNKNOWN pUnk,
											 CBaseFilter *pFilter,
											 CCritSec *pLock,
											 CCritSec *pReceiveLock,
											 HRESULT *phr) :

CRenderedInputPin(NAME("CTsMuxerAudioInputPin"),
				  pFilter,                   // Filter
				  pLock,                     // Locking
				  phr,                       // Return code
				  L"Audio"),		         // Pin name
				  m_pReceiveLock(pReceiveLock),
				  m_pTsMuxer(pTsMuxer)
{
	LogDebug("CTsMuxerAudioInputPin:ctor");

	m_bIsReceiving=FALSE;

}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CTsMuxerAudioInputPin::CheckMediaType(const CMediaType *pType)
{
	if(MEDIATYPE_Audio == pType->majortype && MEDIASUBTYPE_MPEG2_AUDIO == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Audio == pType->majortype && MEDIASUBTYPE_MPEG1Payload == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Audio == pType->majortype && MEDIASUBTYPE_MPEG1AudioPayload == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Audio == pType->majortype && MEDIASUBTYPE_DVD_LPCM_AUDIO == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Audio == pType->majortype && MEDIASUBTYPE_DOLBY_AC3 == pType->subtype){
		return S_OK;
	}
	if(MEDIATYPE_Stream == pType->majortype && MEDIASUBTYPE_MPEG1Audio == pType->subtype){
		return S_OK;
	}
	return S_FALSE;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CTsMuxerAudioInputPin::BreakConnect()
{

	return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CTsMuxerAudioInputPin::ReceiveCanBlock()
{
	return S_FALSE;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CTsMuxerAudioInputPin::Receive(IMediaSample *pSample)
{
	try
	{
		if (pSample==NULL) 
		{
			LogDebug("Audio: receive sample=null");
			return S_OK;
		}

		//		CheckPointer(pSample,E_POINTER);
		//		CAutoLock lock(m_pReceiveLock);
		PBYTE pbData=NULL;

		long sampleLen=pSample->GetActualDataLength();
		if (sampleLen<=0)
		{

			LogDebug("Audio: receive samplelen:%d",sampleLen);
			return S_OK;
		}

		HRESULT hr = pSample->GetPointer(&pbData);
		if (FAILED(hr)) 
		{
			LogDebug("Audio: receive cannot get samplepointer");
			return S_OK;
		}
		if (sampleLen>0)
		{
			if (FALSE==m_bIsReceiving)
			{
				LogDebug("Audio: got signal...");
			}
			m_bIsReceiving=TRUE;
			m_lTickCount=GetTickCount();
		}
		m_pTsMuxer->WriteAudio(pbData,sampleLen);
	}
	catch(...)
	{
		LogDebug("Audio: receive exception");
	}
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CTsMuxerAudioInputPin::EndOfStream(void)
{
	CAutoLock lock(m_pReceiveLock);
	return CRenderedInputPin::EndOfStream();

} // EndOfStream

void CTsMuxerAudioInputPin::Reset()
{
	LogDebug("Audio: Reset()...");
	m_bIsReceiving=FALSE;
	m_lTickCount=0;
}
BOOL CTsMuxerAudioInputPin::IsReceiving()
{
	DWORD msecs=GetTickCount()-m_lTickCount;
	if (msecs>=1000)
	{
		if (m_bIsReceiving)
		{
			LogDebug("Audio: lost signal...");
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
STDMETHODIMP CTsMuxerAudioInputPin::NewSegment(REFERENCE_TIME tStart,
											   REFERENCE_TIME tStop,
											   double dRate)
{
	return S_OK;

} // NewSegment

HRESULT CTsMuxerAudioInputPin::GetMediaType(int iPosition,CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pLock);
	if(iPosition < 0) return E_INVALIDARG;
	if(iPosition > 5) return VFW_S_NO_MORE_ITEMS;

	pmt->ResetFormatBuffer();
	pmt->InitMediaType();
	pmt->formattype = FORMAT_None;

	if(iPosition == 0){
		pmt->majortype = MEDIATYPE_Audio;
		pmt->subtype = MEDIASUBTYPE_MPEG2_AUDIO;
	}
	if(iPosition == 1){
		pmt->majortype = MEDIATYPE_Audio;
		pmt->subtype = MEDIASUBTYPE_MPEG1Payload;
	}
	if(iPosition == 2){
		pmt->majortype = MEDIATYPE_Audio;
		pmt->subtype = MEDIASUBTYPE_MPEG1AudioPayload;
	}
	if(iPosition == 3){
		pmt->majortype = MEDIATYPE_Audio;
		pmt->subtype = MEDIASUBTYPE_DVD_LPCM_AUDIO;
	}
	if(iPosition == 4){
		pmt->majortype = MEDIATYPE_Audio;
		pmt->subtype = MEDIASUBTYPE_DOLBY_AC3;
	}
	if(iPosition == 5){
		pmt->majortype = MEDIATYPE_Stream;
		pmt->subtype = MEDIASUBTYPE_MPEG1Audio;
	}
	return S_OK;
}




