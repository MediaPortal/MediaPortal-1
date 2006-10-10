/*
 *	Copyright (C) 2005-2006 Team MediaPortal
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

#include "PMTInputPin.h"

extern void Log( const char *fmt, ... );

const int   TSPacketSize = 188;
ULONG       PMT_PID = 0x0;

CPMTInputPin::CPMTInputPin( CSubTransform *m_pTransform,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
								HRESULT *phr ) :

    CBaseInputPin( NAME( "CPMTInputPin" ),
					      pFilter,						// Filter
					      pLock,							// Locking
					      phr,							  // Return code
					      L"PMT" ),						// Pin name
					      m_pReceiveLock( pReceiveLock ),
					      m_pTransform( m_pTransform )
{
	Log( "PMT: Pin created" );
}

CPMTInputPin::~CPMTInputPin()
{
}
//
// CheckMediaType
//
HRESULT CPMTInputPin::CheckMediaType( const CMediaType *pmt )
{
	Log("PMT pin: CheckMediaType()");
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}

HRESULT CPMTInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr=CBasePin::CompleteConnect( pPin );

	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	PID_MAP			  pm;
	ULONG			    count;
	ULONG			    umPid;

	hr=pPin->QueryInterface( IID_IMPEG2PIDMap,(void**)&pMap );
	if( SUCCEEDED(hr) && pMap!=NULL )
	{
		hr=pMap->EnumPIDMap( &pPidEnum );
		if( SUCCEEDED(hr) && pPidEnum!=NULL )
		{
			while( pPidEnum->Next( 1, &pm, &count ) == S_OK )
			{
				if ( count != 1 )
				{
					break;
				}

				umPid = pm.ulPID;
				hr = pMap->UnmapPID( 1, &umPid );
				if( FAILED(hr) )
				{
					break;
				}
			}
			hr = pMap->MapPID( 1, &PMT_PID, MEDIA_TRANSPORT_PACKET );

			pPidEnum->Release();
		}
		pMap->Release();
	}
	return hr;
}


//
// Receive
//
STDMETHODIMP CPMTInputPin::Receive( IMediaSample *pSample )
{
  CheckPointer( pSample, E_POINTER );

  //CAutoLock lock(m_pReceiveLock);
  PBYTE pbData=NULL;

// Has the filter been stopped yet?
//  REFERENCE_TIME tStart, tStop;
//  pSample->GetTime(&tStart, &tStop);
//  m_tLast = tStart;
	long lDataLen=0;

  HRESULT hr = pSample->GetPointer( &pbData );
  if (FAILED(hr)) {
      return hr;
  }
	
	lDataLen = pSample->GetActualDataLength();
	// decode
	if( lDataLen > 5 )
		Process( pbData, lDataLen );

  return S_OK;
}

HRESULT CPMTInputPin::Process( BYTE *pbData, long len )
{
	return S_OK;
}


/*void CPMTInputPin::Reset()
{
	m_bReset = true;
}
*/
/*
STDMETHODIMP CPMTInputPin::BeginFlush(void)
{
	Reset();
	return CRenderedInputPin::BeginFlush();
}
STDMETHODIMP CPMTInputPin::EndFlush(void)
{
	Reset();
	return CRenderedInputPin::EndFlush();
}
*/

