/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include "OutputPin.h"

COutputPin::COutputPin(LPUNKNOWN pUnk, CRtspSourceFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinOut"), phr, pFilter, L"Out"),
  CSourceSeeking(NAME("pinOut"),pUnk,phr,section),
	m_pFilter(pFilter),
	m_section(section)
{
}

COutputPin::~COutputPin(void)
{
}

STDMETHODIMP COutputPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	if (riid == IID_IAsyncReader)
  {
		int x=1;
	}
  if (riid == IID_IMediaSeeking)
  {
    return CSourceSeeking::NonDelegatingQueryInterface(riid, ppv);
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT COutputPin::GetMediaType(CMediaType *pmt)
{

	pmt->InitMediaType();
  pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);
  pmt->SetFormatType(&FORMAT_None);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();

	return S_OK;
}
HRESULT COutputPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;


	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
			pRequest->cBuffers = 2;
	}

	pRequest->cbBuffer = 5264*3;


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
HRESULT COutputPin::CompleteConnect(IPin *pReceivePin)
{
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
	}
	else
	{
	}
	return hr;
}

HRESULT COutputPin::FillBuffer(IMediaSample *pSample)
{
  BYTE* pBuffer;
  pSample->GetPointer(&pBuffer);
	long lDataLength = 5264*3;//pSample->GetActualDataLength();
  DWORD bytesRead=m_pFilter->GetData(pBuffer,lDataLength);
  pSample->SetActualDataLength(bytesRead);
  return S_OK;
}
HRESULT COutputPin::ChangeStart()
{
  return S_OK;
}
HRESULT COutputPin::ChangeStop()
{
	return S_OK;
}
HRESULT COutputPin::ChangeRate()
{
	return S_OK;
}
