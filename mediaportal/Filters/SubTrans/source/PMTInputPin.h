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

#pragma once
#pragma warning( disable: 4995 )

#include "SubTransform.h"
#include <streams.h>

class CPMTInputPin : public CBaseInputPin
{
private:

    CSubTransform* const	m_pTransform;		  	// Main renderer object
    CCritSec * const		  m_pReceiveLock;			// Sample critical section
	  bool				      	  m_bReset;

public:

    CPMTInputPin( CSubTransform *m_pTransform,
					LPUNKNOWN pUnk,
					CBaseFilter *pFilter,
					CCritSec *pLock,
					CCritSec *pReceiveLock,
					HRESULT *phr );

	  ~CPMTInputPin();

    STDMETHODIMP Receive(IMediaSample *pSample);
//    STDMETHODIMP BeginFlush(void);
//    STDMETHODIMP EndFlush(void);

    HRESULT CheckMediaType( const CMediaType * );
    HRESULT CompleteConnect( IPin *pPin );

private:

private:
  
  HRESULT Process( BYTE *pbData, long len );

};
