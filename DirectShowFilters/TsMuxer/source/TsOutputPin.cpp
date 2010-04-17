/*
*  Copyright (C) 2005 Team MediaPortal
*  http://www.team-mediaportal.com
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

#include "TsOutPutPin.h"

extern void LogDebug(const char *fmt, ...) ;

CTsMuxerTsOutputPin::CTsMuxerTsOutputPin(LPUNKNOWN pUnk, CBaseFilter *pFilter, CCritSec* pLock,HRESULT *phr) :
CBaseOutputPin(NAME("CTsMuxerTsOutPin"), pFilter, pLock, phr, L"TS Output"),
m_pCritSection(pLock)
{
	m_bConnected = false;
}

CTsMuxerTsOutputPin::~CTsMuxerTsOutputPin()
{
	LogDebug("pin:dtor()");
}


HRESULT CTsMuxerTsOutputPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;
	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
		pRequest->cBuffers = 30;
	}

	pRequest->cbBuffer = 256000;

	ALLOCATOR_PROPERTIES Actual;
	hr = pAlloc->SetProperties(pRequest, &Actual);
	if (FAILED(hr))
	{
		return hr;
	}

	if (Actual.cbBuffer < pRequest->cbBuffer)
	{
		return E_FAIL;
	}

	return S_OK;
}

HRESULT CTsMuxerTsOutputPin::DeliverEndOfStream()
{
	return S_OK;
}

HRESULT CTsMuxerTsOutputPin::GetMediaType(int iPosition,CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pLock);
	if(iPosition < 0) return E_INVALIDARG;
	if(iPosition > 0) return VFW_S_NO_MORE_ITEMS;

	pmt->ResetFormatBuffer();
	pmt->InitMediaType();
	pmt->majortype = MEDIATYPE_Stream;
	pmt->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;
	pmt->formattype = FORMAT_None;

	return S_OK;
}

HRESULT CTsMuxerTsOutputPin::CheckMediaType(const CMediaType* pmt)
{
	return S_OK;/*
				return pmt->majortype == MEDIATYPE_Stream && pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT
				? S_OK
				: E_INVALIDARG;*/
}

HRESULT CTsMuxerTsOutputPin::CheckConnect(IPin *pReceivePin)
{
	return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CTsMuxerTsOutputPin::CompleteConnect(IPin *pReceivePin)
{
	m_bConnected = true;
	return CBaseOutputPin::CompleteConnect(pReceivePin);;
}

HRESULT CTsMuxerTsOutputPin::BreakConnect()
{
	m_bConnected = false;
	return CBaseOutputPin::BreakConnect();
}

bool CTsMuxerTsOutputPin::IsConnected()
{
	return m_bConnected;
}


HRESULT CTsMuxerTsOutputPin::Deliver(IMediaSample* pSample){
	LogDebug("CTsMuxerTsOutputPin - Deliver");
	return CBaseOutputPin::Deliver(pSample);
}
