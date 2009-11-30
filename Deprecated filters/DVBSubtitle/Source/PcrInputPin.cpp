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
#include <commdlg.h>
#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include <commctrl.h>
#include <initguid.h>

#include "PcrInputPin.h"
#include "PatParser\TsHeader.h"

extern void LogDebug( const char *fmt, ... );

const int TSPacketSize = 188;

//
// Constructor
//
CPcrInputPin::CPcrInputPin( CDVBSub *pSubFilter,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
								HRESULT *phr ) :

    CBaseInputPin(NAME( "CPcrInputPin" ),
					pFilter,						// Filter
					pLock,							// Locking
					phr,							  // Return code
					L"Pcr" ),					  // Pin name
					m_pReceiveLock( pReceiveLock ),
					m_pFilter( pSubFilter ),
          m_pcrPid( -1 ),
          m_pDemuxerPin( NULL )
{
	Reset();
	LogDebug( "Pcr: Pin created" );
}


//
// Destructor
//
CPcrInputPin::~CPcrInputPin()
{
}


//
// CheckMediaType
//
HRESULT CPcrInputPin::CheckMediaType( const CMediaType *pmt )
{
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		LogDebug("PCR pin: CheckMediaType() - found MEDIASUBTYPE_MPEG2_TRANSPORT");
    return S_OK;
	}
	return S_FALSE;
}


//
// CompleteConnect
//
HRESULT CPcrInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  m_pDemuxerPin = pPin;
  if( m_pcrPid == -1 )
    return hr;  // PID is mapped later when we have it

  hr = m_pFilter->SetPid( this, m_pcrPid, MEDIA_TRANSPORT_PACKET );

  return hr;
}


//
// GetDemuxerPin
//
IPin* CPcrInputPin::GetDemuxerPin()
{
  return m_pDemuxerPin;
}


//
// ReceiveCanBlock
//
STDMETHODIMP CPcrInputPin::ReceiveCanBlock()
{
  return S_OK;
}


//
// Receive
//
STDMETHODIMP CPcrInputPin::Receive( IMediaSample *pSample )
{
  CAutoLock lock( m_pReceiveLock );
//  LogDebug( "CPcrInputPin::Receive" );

  HRESULT hr = CBaseInputPin::Receive( pSample );
  if( hr != S_OK ) 
  {
    LogDebug( "CPcrInputPin::Receive - BaseInputPin ignored the sample!" ); 
    return hr;
  }

  if( m_pcrPid == -1 )
    return S_OK;  // Nothing to be done yet

	CheckPointer( pSample, E_POINTER );

	PBYTE pbData = NULL;
	long lDataLen = 0;

	hr = pSample->GetPointer( &pbData );
	if( FAILED(hr) )
	{
		LogDebug( "Pcr pin: Receive() err = %d", hr );
		return hr;
	}
	lDataLen = pSample->GetActualDataLength();

	OnRawData( pbData, lDataLen );

//  LogDebug( "CPcrInputPin::Receive - done" );
  return S_OK;
}


//
// Reset
//
void CPcrInputPin::Reset()
{
  LogDebug( "CPcrInputPin::Reset" );
  m_currentPTS = 0;
  m_pcrPid = -1;
  LogDebug( "CPcrInputPin::Reset - done" );
}


//
// SetPcrPid
//
void CPcrInputPin::SetPcrPid( LONG pPid )
{
  LogDebug( "CPcrInputPin::SetPcrPid" );
  m_pcrPid = pPid;

  if( m_pDemuxerPin != NULL )
  {
    m_pFilter->SetPid( this, m_pcrPid, MEDIA_TRANSPORT_PACKET );
  }
  LogDebug( "CPcrInputPin::SetPcrPid - done" );
}


//
// SetPcrPid
//
STDMETHODIMP CPcrInputPin::BeginFlush( void )
{
  CAutoLock lock_it( m_pReceiveLock );
	LogDebug( "CPcrInputPin::BeginFlush" );
  HRESULT hr = CBaseInputPin::BeginFlush();
  LogDebug( "CPcrInputPin::BeginFlush - done" );
  return hr;
}


//
// EndFlush
//
STDMETHODIMP CPcrInputPin::EndFlush( void )
{
  LogDebug( "CPcrInputPin::EndFlush" );
  CAutoLock lock_it( m_pReceiveLock );
  Reset();
  HRESULT hr = CBaseInputPin::EndFlush();
  LogDebug( "CPcrInputPin::EndFlush - done" );
  return hr;
}


//
// GetCurrentPCR
//
ULONGLONG CPcrInputPin::GetCurrentPCR()
{
	return m_currentPTS;
}


//
// OnTsPacket
//
void CPcrInputPin::OnTsPacket( byte* tsPacket )
{
    if (m_pcrPid==-1) return;
    CTsHeader header(tsPacket);
    if (header.Pid != m_pcrPid) return;

    if (header.PayLoadOnly()) return;
    if (tsPacket[4]<7) return; //adaptation field length
    if (tsPacket[5]!=0x10) return;
    // There's a PCR.  Get it
    UINT64 pcrBaseHigh=0LL;
    UINT64 k=tsPacket[6]; k<<=25LL;pcrBaseHigh+=k;
    k=tsPacket[7]; k<<=17LL;pcrBaseHigh+=k;
    k=tsPacket[8]; k<<=9LL;pcrBaseHigh+=k;
    k=tsPacket[9]; k<<=1LL;pcrBaseHigh+=k;
    k=((tsPacket[10]>>7)&0x1); pcrBaseHigh +=k;
    m_currentPTS = pcrBaseHigh;

    m_pFilter->SetPcr( m_currentPTS );
}

