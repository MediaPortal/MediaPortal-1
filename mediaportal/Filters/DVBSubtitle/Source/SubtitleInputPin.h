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
#pragma warning(disable: 4511 4512 4995)

#include "DVBSub.h"
#include "dvbsubs\dvbsubdecoder.h"
#include <streams.h>


class CSubtitleInputPin : public CRenderedInputPin
{
private:
  CDVBSub* const    m_pDVBSub;				// Main renderer object
  CCritSec * const	m_pReceiveLock;	  // Sample critical section
  REFERENCE_TIME		m_tLast;				  // Last sample receive time
	bool				      m_bReset;

public:

  CSubtitleInputPin(  CDVBSub *pDump,
                      LPUNKNOWN pUnk,
                      CBaseFilter *pFilter,
                      CCritSec *pLock,
                      CCritSec *pReceiveLock,
			                CDVBSubDecoder* pSubDecoder,
                      HRESULT *phr );

  ~CSubtitleInputPin();

  // Do something with this media sample
  STDMETHODIMP Receive(IMediaSample *pSample);
  STDMETHODIMP EndOfStream(void);
  STDMETHODIMP ReceiveCanBlock();

  STDMETHODIMP BeginFlush(void);
  STDMETHODIMP EndFlush(void);

  // Check if the pin can support this specific proposed type and format
  HRESULT CheckMediaType( const CMediaType * );
  HRESULT CompleteConnect( IPin *pPin );
  HRESULT BreakConnect();
  STDMETHODIMP NewSegment( REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate );

	void Reset();
	void SetSubtitlePID( ULONG pPID );

private:

	CDVBSubDecoder*		m_pSubDecoder;
	unsigned char*		m_PESdata;
	int					      m_Position;
	int					      m_PESlenght;
	ULONG				      m_SubtitlePID;
};
