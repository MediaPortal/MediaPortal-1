/* 
*  Copyright (C) 2005 Team MediaPortal
*  http://www.team-mediaportal.com
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
#include <streams.h>


class CTsMuxerTsOutputPin : public CBaseOutputPin
{
	CCritSec* const	m_pCritSection;		    // Sample critical section

public:		
	CTsMuxerTsOutputPin(LPUNKNOWN pUnk, CBaseFilter *pFilter, CCritSec* pLock, HRESULT *phr);
	~CTsMuxerTsOutputPin();

	//CSourceStream
	HRESULT GetMediaType(int iPosition,CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT CheckConnect(IPin *pReceivePin);
	HRESULT BreakConnect();

	HRESULT CheckMediaType(const CMediaType* pmt);
    
    HRESULT DeliverEndOfStream();
    virtual HRESULT Deliver(IMediaSample* pSample);

	bool IsConnected();

protected:
	bool      m_bConnected;

};

