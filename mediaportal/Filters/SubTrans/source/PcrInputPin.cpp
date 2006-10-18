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

#include "PcrInputPin.h"

extern void Log( const char *fmt, ... );

const int TSPacketSize = 188;

CPcrInputPin::CPcrInputPin( CSubTransform *m_pTransform,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
								HRESULT *phr ) :

    CRenderedInputPin(NAME( "CPcrInputPin" ),
					pFilter,						// Filter
					pLock,							// Locking
					phr,							  // Return code
					L"Pcr" ),					// Pin name
					m_pReceiveLock( pReceiveLock ),
					m_pTransform( m_pTransform )
{
	Reset();
	Log( "Pcr: Pin created" );
}

CPcrInputPin::~CPcrInputPin()
{
}


//
// CheckMediaType
//
HRESULT CPcrInputPin::CheckMediaType( const CMediaType *pmt )
{
	Log("Audio pin: CheckMediaType()");
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}


HRESULT CPcrInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  m_pDemuxerPin = pPin;
  if( m_pcrPid == -1 )
    return hr;  // PID is mapped later when we have it

  hr = MapPidToDemuxer( m_pcrPid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );

  return hr;
}


//
// Receive
//
STDMETHODIMP CPcrInputPin::Receive( IMediaSample *pSample )
{
	if( m_pcrPid == -1 )
    return S_OK;  // Nothing to be done yet

  if ( m_bReset )
	{
		Log( "Audio pin: reset" );
		m_bReset = false;
	}
	CheckPointer( pSample, E_POINTER );

	CAutoLock lock(m_pReceiveLock);
	PBYTE pbData = NULL;

	long lDataLen = 0;

	HRESULT hr = pSample->GetPointer( &pbData );
	if( FAILED(hr) )
	{
		Log( "Audio pin: Receive() err = %d", hr );
		return hr;
	}
	lDataLen = pSample->GetActualDataLength();

	OnRawData( pbData, lDataLen );
	//OnTsPacket(pbData);
	/*
	if( lDataLen > TSPacketSize )
	{
		ULONGLONG pts( 0 );
		int streamType( 0 ); // not used

		for( int pos( 0 ) ; pos < lDataLen - TSPacketSize*2 ; pos++ )
		{
			if( pbData[pos] == 0x47 && pbData[pos+TSPacketSize] == 0x47 &&
				pbData[pos+TSPacketSize*2] == 0x47 )
			{
				// Payload start?
				if( ( pbData[pos+1] & 0x40 ) > 0 )
				{
					if( S_OK == CurrentPTS( &pbData[pos], &pts, &streamType ) )
					{
						m_currentPTS = pts;
						Log("Audio pin: Receive - pcr = %lld - ", pts );
					}
					else
					{
						Log("Audio pin: Receive - pcr FAILED!");
					}
				}
			}
		}
	}
	*/

  return S_OK;
}


void CPcrInputPin::Reset()
{
  m_bReset = true;
  m_currentPTS = 0;
}

void CPcrInputPin::SetPcrPid( LONG pPid )
{
	m_pcrPid = pPid;
  MapPidToDemuxer( m_pcrPid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );
}


STDMETHODIMP CPcrInputPin::BeginFlush(void)
{
	Reset();
	return CRenderedInputPin::BeginFlush();
}


STDMETHODIMP CPcrInputPin::EndFlush(void)
{
	Reset();
	return CRenderedInputPin::EndFlush();
}


ULONGLONG CPcrInputPin::GetCurrentPTS()
{
	return m_currentPTS;
}

void CPcrInputPin::OnTsPacket( byte* tsPacket )
{
	ULONGLONG pts( 0 );
	int streamType( 0 ); // not used
	int PCR_FLAG;
	PTSTime ptstime;
	int adaptation_field_control = (tsPacket[3] & 0x30) >> 4;

	if (adaptation_field_control == 2 || adaptation_field_control == 3) // adaptation field exists
	{
		PCR_FLAG = (tsPacket[5] & 0x10) >> 4;
		if (tsPacket[4]>=7 && PCR_FLAG == 1)  // adaptation field is long enough and PCR_FLAG is set on.
		{
			UINT64 pcrBaseHigh=0LL;
			UINT64 k=tsPacket[6]; k<<=25LL;pcrBaseHigh+=k;
			k=tsPacket[7]; k<<=17LL;pcrBaseHigh+=k;
			k=tsPacket[8]; k<<=9LL;pcrBaseHigh+=k;
			k=tsPacket[9]; k<<=1LL;pcrBaseHigh+=k;
			k=((tsPacket[10]>>7)&0x1); pcrBaseHigh +=k;
			m_currentPTS = pcrBaseHigh;
			m_pTransform->PTSToPTSTime(m_currentPTS,&ptstime);
			Log("Pcr pin: Receive - pcr = %lld - %d:%02d:%02d:%03d", m_currentPTS,ptstime.h,ptstime.m,ptstime.s,ptstime.u );
		}
		else
		{
			Log("TS packet did not contain PCR value: PCR_FLAG = 0 or adaptation_field_length<7");
		}
	}
	else
	{
		Log("TS packet did not contain adaptation field. afc: %d", adaptation_field_control);
	}

/*	if( ( tsPacket[1] & 0x40 ) > 0 )
	{
		if( S_OK == CurrentPTS( &tsPacket[0], &pts, &streamType ) )
		{
			m_currentPTS = pts;
			Log("Pcr pin: Receive - pcr = %lld - ", pts );
		}
		else
		{
			Log("Pcr pin: Receive - pcr FAILED!");
		}
	}
*/
}

//
// Helper methods from MPSA/Sections.cpp
//

HRESULT CPcrInputPin::GetPESHeader( BYTE *data, PESHeader *header )
{
	header->Reserved = ( data[0] & 0xC0 ) >> 6;
	header->ScramblingControl = ( data[0] & 0x30 ) >> 4;
	header->Priority = ( data[0] & 0x08 ) >> 3;
	header->dataAlignmentIndicator = ( data[0] & 0x04 ) >> 2;
	header->Copyright = ( data[0] & 0x02 ) >> 1;
	header->Original = data[0] & 0x01;
	header->PTSFlags = ( data[1] & 0xC0 ) >> 6;
	header->ESCRFlag = ( data[1] & 0x20 ) >> 5;
	header->ESRateFlag = ( data[1] & 0x10 ) >> 4;
	header->DSMTrickModeFlag=(data[1] & 0x08 ) >> 3;
	header->AdditionalCopyInfoFlag = ( data[1] & 0x04 ) >> 2;
	header->PESCRCFlag = ( data[1] & 0x02 ) >> 1;
	header->PESExtensionFlag = data[1] & 0x01;
	header->PESHeaderDataLength = data[2];
	return S_OK;
}
void CPcrInputPin::GetPTS( BYTE *data, ULONGLONG *pts )
{
	//*pts= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));

	uint64_t p0, p1, p2, p3, p4;

	// PTS is in bytes 9,10,11,12,13
	p0 = ( data[4] & 0xfe ) >> 1 | ( ( data[3] & 1 ) << 7 );
	p1 = ( data[3] & 0xfe ) >> 1 | ( ( data[2] & 2 ) << 6 );
	p2 = ( data[2] & 0xfc ) >> 2 | ( ( data[1] & 3 ) << 6 );
	p3 = ( data[1] & 0xfc ) >> 2 | ( ( data[0] & 6 ) << 5 );
	p4 = ( data[0] & 0x08 ) >> 3;

	*pts = p0 | ( p1 << 8 ) | ( p2 << 16 ) | ( p3 << 24 ) | ( p4 << 32 );
}
HRESULT CPcrInputPin::CurrentPTS( BYTE *pData, ULONGLONG *ptsValue, int *streamType )
{
	HRESULT hr=S_FALSE;
	*ptsValue=-1;
	TSHeader header;
	PESHeader pes;
	GetTSHeader(pData,&header);
	int offset=4;
	bool found=false;

	if( header.AdaptionControl == 1 || header.AdaptionControl == 3 )
		offset += pData[4];

	if( offset >= 188 )
		return S_FALSE;

	if( header.SyncByte==0x47 && pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1 )
	{
		*streamType=(int)( ( pData[offset+3] >> 5 ) & 0x07 );
		WORD pesLen=( pData[offset+4] << 8 ) + pData[offset+5];
		GetPESHeader( &pData[offset+6], &pes );
		BYTE pesHeaderLen = pData[offset+8];
		if( header.Pid ) // valid header
		{
			if( pes.PTSFlags == 0x02 )
			{
				GetPTS( &pData[offset+9], ptsValue );
				hr = S_OK;
			}
		}
	}
	return hr;
}

HRESULT CPcrInputPin::GetTSHeader( BYTE *data, TSHeader *header )
{
	header->SyncByte = data[0];
	header->TransportError = ( data[1] & 0x80) > 0 ? true:false;
	header->PayloadUnitStart = ( data[1] & 0x40 ) > 0 ? true:false;
	header->TransportPriority = ( data[1] & 0x20 ) > 0 ? true:false;
	header->Pid = ( ( data[1] & 0x1F ) << 8 ) + data[2];
	header->TScrambling = data[3] & 0xC0;
	header->AdaptionControl=( data[3] >> 4 ) & 0x3;
	header->ContinuityCounter = data[3] & 0x0F;
	return S_OK;
}
