/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#include <streams.h>
#include "MPTSFilter.h"
#include "FilterAudioOutPin.h"

extern void LogDebug(const char *fmt, ...) ;

//
CFilterAudioPin::CFilterAudioPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, Sections *pSections, HRESULT *phr) :
	CBaseOutputPin(NAME("AudioPin"),pFilter,&m_cSharedState,phr,L"Audio (PES)")
{
	m_pSections=pSections;
}
HRESULT CFilterAudioPin::Deliver(IMediaSample *ms)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(ms,E_POINTER);
	CBaseOutputPin::Deliver(ms);
	return S_OK;

}
void CFilterAudioPin::Process(BYTE *audioBuffer,int audioSampleLen,REFERENCE_TIME& start,REFERENCE_TIME& stop)
{
	if (FALSE==CBaseOutputPin::IsConnected()) return;
	CAutoLock cAutoLock(&m_cSharedState);
	if (audioSampleLen>0)
	{
		IMediaSample *audioSample;
		if(GetDeliveryBuffer(&audioSample,NULL,NULL,0)==S_OK)
		{
			BYTE *data;
			audioSample->GetPointer(&data);
			CopyMemory(data,audioBuffer,audioSampleLen);
			audioSample->SetActualDataLength(audioSampleLen);
			if (start>0)
				audioSample->SetTime(&start,&stop);
			HRESULT hr=Deliver(audioSample);
			audioSample->Release();
			if(hr==S_FALSE)
				DeliverEndOfStream();
		}
		else
		{
			//no buffer!
			int x=1;
		}
	}
}

CFilterAudioPin::~CFilterAudioPin()
{
}

//

//
STDMETHODIMP CFilterAudioPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(ppv,E_POINTER);
	if(riid==IID_IUnknown)
	{
		return CUnknown::NonDelegatingQueryInterface(riid, ppv);
	}
	return CBaseOutputPin::NonDelegatingQueryInterface(riid,ppv);
}


HRESULT	CFilterAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *ppropInputRequest)
{
	HRESULT hr;
	CAutoLock lock(CBaseOutputPin::m_pLock);
    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(ppropInputRequest, E_POINTER);

	if (ppropInputRequest->cBuffers == 0)
    {
        ppropInputRequest->cBuffers = 12;
    }

	ppropInputRequest->cbBuffer = 18800;
	
    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(ppropInputRequest, &Actual);
    if (FAILED(hr))
    {
        return hr;
    }

    if (Actual.cbBuffer < ppropInputRequest->cbBuffer)
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CFilterAudioPin::CompleteConnect(IPin *pPin)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(pPin,E_POINTER);
	return CBaseOutputPin::CompleteConnect(pPin);
}
HRESULT CFilterAudioPin::CheckMediaType(const CMediaType *cmt)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(cmt,E_POINTER);
	if(cmt->majortype==MEDIATYPE_Audio &&
		cmt->subtype==MEDIASUBTYPE_MPEG2_AUDIO)
	{
		return S_OK;
	}
	return S_FALSE;
}
STDMETHODIMP CFilterAudioPin::ConnectionMediaType(AM_MEDIA_TYPE *pmt)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(pmt,E_POINTER);

	pmt->majortype=MEDIATYPE_Audio;
	pmt->subtype=MEDIASUBTYPE_MPEG2_AUDIO;
	pmt->formattype=FORMAT_MPEG2Audio;
	return S_OK;
}

HRESULT	CFilterAudioPin::GetMediaType(int iPosition,CMediaType *pMediaType)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
    CheckPointer(pMediaType,E_POINTER); 
	
	if(iPosition != 0)
    {
        return E_INVALIDARG;
    }

	pMediaType->InitMediaType();
	pMediaType->SetType(&MEDIATYPE_Audio);
	pMediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_AUDIO);
	pMediaType->SetFormatType(&FORMAT_MPEG2Audio);
	pMediaType->SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
	//int len=sizeof(Mpeg2ProgramVideo);
	return S_OK;
}
