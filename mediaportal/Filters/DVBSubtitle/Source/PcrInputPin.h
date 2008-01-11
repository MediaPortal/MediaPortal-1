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

#pragma once
#pragma warning( disable: 4511 4512 4995 )

#include "PidObserver.h"
#include "DvbSub.h"
#include "PatParser\PacketSync.h"
#include <streams.h>

class CPcrInputPin : public CBaseInputPin, CPacketSync, MPCRPidObserver
{
public:

  CPcrInputPin( CDVBSub *pSubFilter,
				          LPUNKNOWN pUnk,
				          CBaseFilter *pFilter,
				          CCritSec *pLock,
				          CCritSec *pReceiveLock,
				          HRESULT *phr );

	~CPcrInputPin();

  STDMETHODIMP Receive( IMediaSample *pSample );
  STDMETHODIMP BeginFlush( void );
  STDMETHODIMP EndFlush( void );
  STDMETHODIMP ReceiveCanBlock();

  HRESULT CheckMediaType( const CMediaType * );
  HRESULT CompleteConnect( IPin *pPin );

  void SetPcrPid( LONG pPid );
  IPin* GetDemuxerPin();
	void Reset();

	ULONGLONG GetCurrentPCR();

  // From CPacketSync
  void OnTsPacket( byte* tsPacket );

private:

  CDVBSub *const	m_pFilter;		    // Main renderer object
  CCritSec *const	m_pReceiveLock;		// Sample critical section

	ULONGLONG m_currentPTS;
  LONG      m_pcrPid;

  IPin      *m_pDemuxerPin;
};
