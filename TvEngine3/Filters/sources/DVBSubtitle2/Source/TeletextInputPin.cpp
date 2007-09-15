/*
*	Copyright (C) 2006-2007 Team MediaPortal
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
#include <initguid.h>
#include <sstream>
using namespace std;
#include "TeletextInputPin.h"
#include "Hamming.h"
#include "TeletextConversion.h"

extern void LogDebug( const char *fmt, ... );

//
// Constructor
//
CTeletextInputPin::CTeletextInputPin( CDVBSub *pDVBSub,
									 LPUNKNOWN pUnk,
									 CBaseFilter *pFilter,
									 CCritSec *pLock,
									 CCritSec *pReceiveLock,
									 HRESULT *phr ) :
// start super ctor
CRenderedInputPin(NAME( "CTeletextInputPin" ),
				  pFilter,						    // Filter
				  pLock,							    // Locking
				  phr,							      // Return code
				  L"TeletextIn" ),				      	// Pin name
				  // end super constructor
				  m_pReceiveLock( pReceiveLock ),
				  m_pFilter( pDVBSub ),
				  m_teletextPid( -1 ),
				  m_pPin( NULL )
{
	m_pesDecoder = new CPesDecoder( this );
	decoder = new TeletextDecoder( pDVBSub ); 
	Reset();
	LogDebug( "Teletext: Input pin constructor!" );
	//LogCharacterTable();
}

//
// CheckMediaType
//
HRESULT CTeletextInputPin::CheckMediaType( const CMediaType *pmt )
{
	LogDebug("TeletextInputPin: CheckMediaType");
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		LogDebug("Teletext: CTeletextInputPin::CheckMediaType() - found MEDIASUBTYPE_MPEG2_TRANSPORT");
		return S_OK;
	}
	else{
		LogDebug("Teletext: CTeletextInputPin::CheckMediaType() - DID NOT FIND MEDIASUBTYPE_MPEG2_TRANSPORT!");
	}
	return S_FALSE;
}

//
// SetTeletextPid
//
void CTeletextInputPin::SetTeletextPid( LONG pPid )
{
	LogDebug( "TeletextPin: PID is %d",pPid);
	m_teletextPid = pPid;
	//MapPidToDemuxer( m_teletextPid, m_pPin, MEDIA_TRANSPORT_PACKET ); no longer needed
	m_pesDecoder->SetPid( m_teletextPid );
}

//
// Reset
//
void CTeletextInputPin::Reset()
{
	LogDebug("TeletextInputPin: Reset");
	m_bReset = true;
	m_pesDecoder->Reset();
	// TODO: Reset teletext decoder
}

//
// Destructor
//
CTeletextInputPin::~CTeletextInputPin()
{
	LogDebug("TeletextInputPin: DTOR");
	delete m_pesDecoder;
	delete decoder;
}

//
// CompleteConnect
//
HRESULT CTeletextInputPin::CompleteConnect( IPin *pPin )
{
	LogDebug( "TeletextPin: Complete Connect");
	HRESULT hr = CBasePin::CompleteConnect( pPin );
	m_pPin = pPin;

	if( m_teletextPid == -1 )
		return hr;  // PID is mapped later when we have it

	m_pesDecoder->SetPid( m_teletextPid );

	return hr;
}


void CTeletextInputPin::NotifySubPageInfo(int page, char lang[3]){
	LogDebug("CTeletextInputPin::NotifySubPageInfo");
	
	CAutoLock lock(m_pReceiveLock);

	decoder->NotifySubPageInfo(page,lang);
}
//
// Receive
//
STDMETHODIMP CTeletextInputPin::Receive( IMediaSample *pSample )
{
	//LogDebug( "TeletextPin: Receive");
	//return S_OK;

	CAutoLock lock(m_pReceiveLock);

	if( m_teletextPid == -1 )
	{
		LogDebug( "TeletextPin: Receive no PID yet");
		return S_OK;  // Nothing to be done yet
	}

	if ( m_bReset )
	{
		LogDebug( "TeletextPin: reset" );
		m_bReset = false;
	}
	CheckPointer( pSample, E_POINTER );

	PBYTE pbData = NULL;

	REFERENCE_TIME tStart, tStop;
	pSample->GetTime( &tStart, &tStop);
	long lDataLen = 0;
	HRESULT hr = pSample->GetPointer( &pbData );

  if( FAILED( hr ) )
	{
		LogDebug( "Teletext: Receive() err" );
		return hr;
	}
	lDataLen = pSample->GetActualDataLength();

  OnRawData( pbData, lDataLen );

  //LogDebug( "TeletextPin: Receive END");
  return S_OK;
}

//
// OnTsPacket
//
void CTeletextInputPin::OnTsPacket( byte* tsPacket )
{
  //LogDebug("CTeletextInputPin::OnTsPacket");
  m_pesDecoder->OnTsPacket( tsPacket );
}

//
// OnNewPesPacket
// (called from m_pesDecoder::OnTsPacket)
// See ETSI EN 300 472 (especially section 4.2)
//
int CTeletextInputPin::OnNewPesPacket( int streamid, byte* header, int headerlen,
                                       byte* data, int datalen, bool isStart )
{
	//LogDebug( "\nSTART PES PACKET : TeletextPin: PES: streamid %i, headerlen %i, datalen %i, isStart %i", streamid,headerlen,datlen, isStart);
	
	// header must start with 0x00 0x00 0x01
	assert(header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01);
	assert(headerlen == 45); // header must be 45 bytes

	byte stream_id = header[3];
	assert(stream_id == 0xBD); // must be private stream 1

	int PES_PACKET_LEN = (header[4] << 8 | header[5]);
	//LogDebug("PES_PACKET_LEN %i", PES_PACKET_LEN);
	
	bool data_alignment_indicator = (bool)((header[6] & 0x04) >> 2);
	// alignment indicator must be set for teletext
	assert(data_alignment_indicator);

	assert ( (header[6] & 0xC0) == 0x80 ); // the first two bits of the 6th header byte MUST be 10

	assert( (header[7] & 0xC0) != 0x40); // the PTS DTS bits are forbidden to be 01

	byte PES_HEADER_DATA_LENGTH = header[8];
	assert(PES_HEADER_DATA_LENGTH == 0x24);
		
	assert( (PES_PACKET_LEN + 6) % 184 == 0);	

	int dataBlockLen = PES_PACKET_LEN + 6 - headerlen;
	assert(dataBlockLen == datalen);

	// PES_PACKET_LEN is number of bytes AFTER PES_PACKET_LEN field.
	// header length is the total number of bytes in the header
	// so the data block at the end must be the PES_PACKET_LEN plus
	// the bytes up to PES_PACKET_LEN minus the header bytes

	//LogDebug("Data block length seems to be : %i", dataBlockLen);
	//return 0;
	// see ETSI EN 300 472
	byte data_identifier = data[0];
	if(!(data_identifier >= 0x10 && data_identifier <= 0x1F)){
		LogDebug("Data identifier not as expected %X", data_identifier);
	}
	// assert(data_identifier >= 0x10 && data_identifier <= 0x1F);

	// see Table 1 in section 4.3
	int size = 46; // data_unit_id + data_unit_length + data_field()
	int dataLeft = dataBlockLen - 1; // subtract 1 for data_identifier

	int offset = -1;

	for(int i = 0; dataLeft >= size; i++){
		offset = 1 + i * size; 

		byte data_unit_id = data[offset];	

		if(!(data_unit_id == 0xFF || data_unit_id == 0x02 || data_unit_id == 0x03)){
			LogDebug("Data unit id incorrect: %X", data_unit_id);
			if(data_unit_id == 0x2C && data[offset+2] == 0xE4 && data_identifier == 0x02){
				LogDebug("Data starts without data_identifier! data_identifier has value of data_unit_id, data_unit_id of data_unit_length etc..!!!");
			}
			
			assert(data_unit_id == 0xFF || data_unit_id == 0x02 || data_unit_id == 0x03);
			return 0;
		}

		if(data_unit_id == 0x03){ //EBU Teletext subtitle data
			//LogDebug("EBU Teletext subtitle data"); 

			byte data_unit_length = data[offset+1];
			assert(data_unit_length == 0x2C); // always the same length for teletext data (see section 4.4)

			byte* teletextPacketData = &data[offset+2]; // skip past data_unit_id and data_unit_length

			decoder->OnTeletextPacket(teletextPacketData);
		}
		else if(data_unit_id == 0x02){ //EBU teletext non-subtitle data
			byte data_unit_length = data[offset+1];
			assert(data_unit_length == 0x2C);
		}
		dataLeft -= size;
	}

	assert(dataLeft == 0);

  //LogDebug("FIN PES PACKET");
  return 0;
}


//
// EndOfStream
//
STDMETHODIMP CTeletextInputPin::EndOfStream( void )
{
  CAutoLock lock( m_pReceiveLock );
  return CRenderedInputPin::EndOfStream();
}

//
// ReceiveCanBlock
//
STDMETHODIMP CTeletextInputPin::ReceiveCanBlock()
{
  return S_OK;
}


//
// BeginFlush
//
STDMETHODIMP CTeletextInputPin::BeginFlush( void )
{
	return CRenderedInputPin::BeginFlush();
}


//
// EndFlush
//
STDMETHODIMP CTeletextInputPin::EndFlush( void )
{
	m_pFilter->NotifySeeking();
	return CRenderedInputPin::EndFlush();
}

//
// NewSegment
//
STDMETHODIMP CTeletextInputPin::NewSegment( REFERENCE_TIME tStart,
											REFERENCE_TIME tStop,
											double dRate )
{
  return CRenderedInputPin::NewSegment( tStart, tStop, dRate );
}

//
// BreakConnect
//
HRESULT CTeletextInputPin::BreakConnect()
{
  return CRenderedInputPin::BreakConnect();
}