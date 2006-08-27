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
#pragma warning(disable: 4511 4512 4995)
#include "DVBSub.h"
#include <streams.h>

class CSubtitleOutputPin : public CBaseOutputPin
{
public:

  CSubtitleOutputPin( CDVBSub *pDVBSub,
                      CBaseFilter *pFilter,
                      CCritSec *pLock,
                      HRESULT *phr );

   ~CSubtitleOutputPin();

  HRESULT DecideBufferSize( IMemAllocator *pAlloc,
                            ALLOCATOR_PROPERTIES *ppropInputRequest );

  STDMETHODIMP BeginFlush( void );
  STDMETHODIMP EndFlush( void );

  HRESULT CheckMediaType( const CMediaType * );
  HRESULT CompleteConnect( IPin *pReceivePin );
  STDMETHODIMP NewSegment( REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate );

  HRESULT GetMediaType( int iPosition, CMediaType* pmt );

  HRESULT GetDeliveryBuffer(
    IMediaSample **ppSample,
    REFERENCE_TIME *pStartTime,
    REFERENCE_TIME *pEndTime,
    DWORD dwFlags );

  HRESULT CheckConnect( IPin *pPin );
  HRESULT Deliver( IMediaSample *pSample );

	void Reset();

private:
  CDVBSub* const    m_pDVBSub;				// Main renderer object
  REFERENCE_TIME		m_tLast;				// Last sample receive time
	bool				      m_bReset;
};
