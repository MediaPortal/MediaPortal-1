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

#pragma once
#pragma warning( disable: 4511 4512 4995 )

#include "SubTransform.h"
#include <streams.h>

struct tsheader
{
	BYTE SyncByte			;
	bool TransportError		;
	bool PayloadUnitStart	;
	bool TransportPriority	;
	unsigned short Pid		;
	BYTE TScrambling		;
	BYTE AdaptionControl	;
	BYTE ContinuityCounter	;
};
typedef tsheader TSHeader;
 
struct pesheader
{
	BYTE     Reserved				;
	BYTE     ScramblingControl		;
	BYTE     Priority 				;
	BYTE     dataAlignmentIndicator	;
	BYTE     Copyright				;
	BYTE     Original				;
	BYTE     PTSFlags				;
	BYTE     ESCRFlag				;
	BYTE     ESRateFlag				;
	BYTE     DSMTrickModeFlag		;
	BYTE     AdditionalCopyInfoFlag	;
	BYTE     PESCRCFlag				;
	BYTE     PESExtensionFlag		;
	BYTE     PESHeaderDataLength	;
};
typedef pesheader PESHeader;

struct timedata
{
	int h;
	int m;
	int s;
	int u;
};
typedef timedata PTSTime;

class CAudioInputPin : public CRenderedInputPin
{
private:

    CSubTransform* const	m_pTransform;		// Main renderer object
    CCritSec * const		m_pReceiveLock;		// Sample critical section
	bool					m_bReset;

public:

    CAudioInputPin( CSubTransform *m_pTransform,
					LPUNKNOWN pUnk,
					CBaseFilter *pFilter,
					CCritSec *pLock,
					CCritSec *pReceiveLock,
					HRESULT *phr );

	~CAudioInputPin();

    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP BeginFlush(void);
    STDMETHODIMP EndFlush(void);

    // Check if the pin can support this specific proposed type and format
    HRESULT CheckMediaType( const CMediaType * );

	void Reset();
		
	ULONGLONG GetCurrentPTS();

private:

	// Helper methods from MPSA/Sections.h
	HRESULT GetTSHeader( BYTE *data,TSHeader *header );
	HRESULT GetPESHeader( BYTE *data, PESHeader *header );
	HRESULT CurrentPTS( BYTE *pData, ULONGLONG *ptsValue, int *streamType );
	void GetPTS( BYTE *data, ULONGLONG *pts );

private:

	ULONGLONG m_currentPTS;
};
