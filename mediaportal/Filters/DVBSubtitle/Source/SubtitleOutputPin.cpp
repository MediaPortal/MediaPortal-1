/* 
 *	Copyright (C) 2006 Team MediaPortal
 *  Author: tourettes
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
#include <commdlg.h>
#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include <commctrl.h>
#include <initguid.h>

//#include "MPDVBSub.h"
//#include "proppage.h"
#include "SubtitleOutputPin.h"

extern void Log( const char *fmt, ... );

CSubtitleOutputPin::CSubtitleOutputPin(   
                      CDVBSub *pDVBSub,
                      CBaseFilter *pFilter,
                      CCritSec *pLock,
                      HRESULT *phr ) :

    CBaseOutputPin(NAME( "CSubtitleOutputPin" ),
					pFilter,            // Filter
					pLock,              // Locking
					phr,                // Return code
					L"Out" ),           // Pin name
					m_pDVBSub( pDVBSub )
{
	Reset();
	Log( "SubtitleOutputPin: Output pin created" );
}

CSubtitleOutputPin::~CSubtitleOutputPin()
{
}
//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CSubtitleOutputPin::CheckMediaType( const CMediaType *pmt )
{
	Log("SubtitleOutputPin: CheckMediaType()");
	if( pmt->majortype == MEDIATYPE_DVD_ENCRYPTED_PACK )
  {
    Log("SubtitleOutputPin: CheckMediaType() - found MEDIATYPE_DVD_ENCRYPTED_PACK");
    if( pmt->subtype == MEDIASUBTYPE_DVD_SUBPICTURE )
  	{
		  Log("SubtitleOutputPin: CheckMediaType() - found MEDIASUBTYPE_DVD_SUBPICTURE");
      return S_OK;
    }
	}
	return S_FALSE;
}


HRESULT CSubtitleOutputPin::CompleteConnect( IPin *pReceivePin )
{
  ASSERT(pReceivePin);

  HRESULT hr  = CBaseOutputPin::CompleteConnect( pReceivePin );
  if(FAILED(hr))
  {
      return hr;
  }

  return hr;
}
HRESULT CSubtitleOutputPin::GetMediaType( int iPosition, CMediaType* pmt )
{
	if( iPosition < 0 )
  {
    return E_INVALIDARG;
  }
	if( iPosition > 0 ) 
  {
    return VFW_S_NO_MORE_ITEMS;
  }

	pmt->InitMediaType();
	pmt->majortype  = MEDIATYPE_DVD_ENCRYPTED_PACK;
	pmt->subtype    = MEDIASUBTYPE_DVD_SUBPICTURE;
	pmt->formattype = FORMAT_None;

	return S_OK;
}

/*HRESULT CSubtitleOutputPin::GetDeliveryBuffer(
    IMediaSample **ppSample,
    REFERENCE_TIME *pStartTime,
    REFERENCE_TIME *pEndTime,
    DWORD dwFlags )
{
 return S_OK;
}
*/

HRESULT CSubtitleOutputPin::CheckConnect( IPin *pPin )
{
  return CBaseOutputPin::CheckConnect( pPin );
}

HRESULT CSubtitleOutputPin::Deliver( IMediaSample *pSample )
{
 return S_OK;
}

HRESULT CSubtitleOutputPin::DecideBufferSize( IMemAllocator *pAlloc,
                                              ALLOCATOR_PROPERTIES *ppropInputRequest )
{
	ppropInputRequest->cBuffers = 1;
	ppropInputRequest->cbBuffer = 4096; // what size do we need?
	ppropInputRequest->cbAlign  = 1;
	ppropInputRequest->cbPrefix = 0;

  ALLOCATOR_PROPERTIES Actual;
  HRESULT hr = pAlloc->SetProperties( ppropInputRequest, &Actual );
	
  if(FAILED(hr))
  {
    return hr;
  }

  if( ppropInputRequest->cBuffers > Actual.cBuffers || ppropInputRequest->cbBuffer > Actual.cbBuffer )
  {
    return E_FAIL;
  }
  else
  {
    return NOERROR;
  }
}


void CSubtitleOutputPin::Reset()
{
	m_bReset = true;
}

STDMETHODIMP CSubtitleOutputPin::BeginFlush( void )
{
//	Reset();
	return CBaseOutputPin::BeginFlush();
}
STDMETHODIMP CSubtitleOutputPin::EndFlush( void )
{
//	Reset();
	return CBaseOutputPin::EndFlush();
}

//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CSubtitleOutputPin::NewSegment( REFERENCE_TIME tStart,
											REFERENCE_TIME tStop,
											double dRate )
{
    m_tLast = 0;
    return S_OK;
}
