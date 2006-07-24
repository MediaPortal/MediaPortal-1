/*
 *	Copyright (C) 2005 Team MediaPortal
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

#include "AudioInputPin.h"

extern void Log( const char *fmt, ... );

const int TSPacketSize = 188;

CAudioInputPin::CAudioInputPin( CSubTransform *m_pTransform,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
								HRESULT *phr ) :

    CRenderedInputPin(NAME( "CAudioInputPin" ),
					pFilter,						// Filter
					pLock,							// Locking
					phr,							// Return code
					L"Audio" ),						// Pin name
					m_pReceiveLock( pReceiveLock ),
					m_pTransform( m_pTransform )
{
	Reset();
	Log( "Audio: Pin created" );
}

CAudioInputPin::~CAudioInputPin()
{
}
//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CAudioInputPin::CheckMediaType( const CMediaType *pmt )
{
	Log("Audio pin: CheckMediaType()");
//	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}
/*
HRESULT CAudioInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr=CBasePin::CompleteConnect( pPin );

	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;

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
			pid = 0xAUDIO;	// THIS IS A TEST PID ONLY
			hr = pMap->MapPID( 1, &pid, MEDIA_TRANSPORT_PAYLOAD ); //MEDIA_ELEMENTARY_STREAM ); //

			pPidEnum->Release();
		}
		pMap->Release();
	}
	return hr;
}
*/

//
// Receive
//
STDMETHODIMP CAudioInputPin::Receive( IMediaSample *pSample )
{
	try
	{
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

		if( lDataLen > TSPacketSize )
		{
			ULONGLONG pts( 0 );
			int streamType( 0 ); // not used
			
			for( int pos( 0 ) ; pos < lDataLen - TSPacketSize*2 ; pos++ )
			{
				if( pbData[pos] == 0x47 && pbData[pos+TSPacketSize] == 0x47 && 
					pbData[pos+TSPacketSize*2] == 0x47 )
				{
					//Log( "Audio pin: Receive - found TS packet pos = %d", pos );
					// Payload start?
					if( ( pbData[pos+1] & 0x40 ) > 0 )
					{
						if( S_OK == CurrentPTS( &pbData[pos], &pts, &streamType ) )
						{
							m_currentPTS = pts;
							Log("PTS = %lld - Audio pin: Receive - Current", pts );
						}
						else
						{
							Log("Audio pin: Receive - CurrentPTS FAILED!!!");
						}
					}
				}
			}
		}
	}

	catch(...)
	{
		Log( "Audio pin: --- UNHANDLED EXCEPTION ---" );
	}

    return S_OK;
}

void CAudioInputPin::Reset()
{
	m_bReset = true;
}

/*void CAudioInputPin::SetAudioPID( ULONG pPID )
{
	m_AudioPID = pPID;
}
*/

STDMETHODIMP CAudioInputPin::BeginFlush(void)
{
	Reset();
	return CRenderedInputPin::BeginFlush();
}
STDMETHODIMP CAudioInputPin::EndFlush(void)
{
	Reset();
	return CRenderedInputPin::EndFlush();
}

ULONGLONG CAudioInputPin::GetCurrentPTS()
{
	return m_currentPTS;
}

//
// Helper methods from MPSA/Sections.cpp
//

HRESULT CAudioInputPin::GetPESHeader( BYTE *data, PESHeader *header )
{
	header->Reserved=(data[0] & 0xC0)>>6;
	header->ScramblingControl=(data[0] &0x30)>>4;
	header->Priority=(data[0] & 0x08)>>3;
	header->dataAlignmentIndicator=(data[0] & 0x04)>>2;
	header->Copyright=(data[0] & 0x02)>>1;
	header->Original=data[0] & 0x01;
	header->PTSFlags=(data[1] & 0xC0)>>6;
	header->ESCRFlag=(data[1] & 0x20)>>5;
	header->ESRateFlag=(data[1] & 0x10)>>4;
	header->DSMTrickModeFlag=(data[1] & 0x08)>>3;
	header->AdditionalCopyInfoFlag=(data[1] & 0x04)>>2;
	header->PESCRCFlag=(data[1] & 0x02)>>1;
	header->PESExtensionFlag=data[1] & 0x01;
	header->PESHeaderDataLength=data[2];
	return S_OK;
}
void CAudioInputPin::GetPTS( BYTE *data, ULONGLONG *pts )
{	
	*pts= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));
}
HRESULT CAudioInputPin::CurrentPTS( BYTE *pData, ULONGLONG *ptsValue,int *streamType )
{
	HRESULT hr=S_FALSE;
	*ptsValue=-1;
	TSHeader header;
	PESHeader pes;
	GetTSHeader(pData,&header);
	int offset=4;
	bool found=false;

	if(header.AdaptionControl==1 || header.AdaptionControl==3)
		offset+=pData[4];

	if( offset >= 188 ) 
		return S_FALSE;
	
	if(header.SyncByte==0x47 && pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1)
	{
		*streamType=(int)((pData[offset+3]>>5) & 0x07);
		WORD pesLen=(pData[offset+4]<<8)+pData[offset+5];
		GetPESHeader(&pData[offset+6],&pes);
		BYTE pesHeaderLen=pData[offset+8];
		if(header.Pid) // valid header
		{
			if(pes.PTSFlags==0x02)
			{
				// audio pes found
				GetPTS(&pData[offset+9],ptsValue);
				hr = S_OK;
			}
		}	
	}
	return hr;
}

HRESULT CAudioInputPin::GetTSHeader( BYTE *data,TSHeader *header )
{
	header->SyncByte=data[0];
	header->TransportError=(data[1] & 0x80)>0?true:false;
	header->PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	header->TransportPriority=(data[1] & 0x20)>0?true:false;
	header->Pid=((data[1] & 0x1F) <<8)+data[2];
	header->TScrambling=data[3] & 0xC0;
	header->AdaptionControl=(data[3]>>4) & 0x3;
	header->ContinuityCounter=data[3] & 0x0F;
	return S_OK;
}
