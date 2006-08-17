/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#ifndef __VideoPin_H
#define __VideoPin_H
#include "tsreader.h"

class CVideoPin : public CSourceStream, public IMediaSeeking
{
public:
	CVideoPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section);
	~CVideoPin();

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

	//CSourceStream
	HRESULT GetMediaType(CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT FillBuffer(IMediaSample *pSample);
	

	// IMediaSeeking
	DECLARE_IUNKNOWN
  HRESULT STDMETHODCALLTYPE GetCapabilities(DWORD *pCapabilities) ;
  HRESULT STDMETHODCALLTYPE CheckCapabilities(DWORD *pCapabilities) ;
  HRESULT STDMETHODCALLTYPE IsFormatSupported(const GUID *pFormat) ;
  HRESULT STDMETHODCALLTYPE QueryPreferredFormat(GUID *pFormat) ;
  HRESULT STDMETHODCALLTYPE GetTimeFormat(GUID *pFormat) ;
  HRESULT STDMETHODCALLTYPE IsUsingTimeFormat(const GUID *pFormat) ;
  HRESULT STDMETHODCALLTYPE SetTimeFormat(const GUID *pFormat) ;
  HRESULT STDMETHODCALLTYPE GetDuration(LONGLONG *pDuration) ;
  HRESULT STDMETHODCALLTYPE GetStopPosition(LONGLONG *pStop) ;
  HRESULT STDMETHODCALLTYPE GetCurrentPosition(LONGLONG *pCurrent) ;
  HRESULT STDMETHODCALLTYPE ConvertTimeFormat(LONGLONG *pTarget,const GUID *pTargetFormat,LONGLONG Source,const GUID *pSourceFormat) ;
  HRESULT STDMETHODCALLTYPE SetPositions( /* [out][in] */ LONGLONG *pCurrent,DWORD dwCurrentFlags,/* [out][in] */ LONGLONG *pStop,DWORD dwStopFlags) ;
  HRESULT STDMETHODCALLTYPE GetPositions(LONGLONG *pCurrent,LONGLONG *pStop) ;
  HRESULT STDMETHODCALLTYPE GetAvailable(LONGLONG *pEarliest,LONGLONG *pLatest) ;
  HRESULT STDMETHODCALLTYPE SetRate( double dRate) ;
  HRESULT STDMETHODCALLTYPE GetRate(double *pdRate) ;
  HRESULT STDMETHODCALLTYPE GetPreroll(LONGLONG *pllPreroll) ;


	HRESULT OnThreadStartPlay();
	void SetStart(CRefTime rtStartTime);
	void FlushStart();
	void FlushStop();

protected:
	BOOL m_bDiscontinuity;
	CTsReaderFilter *	const m_pTsReaderFilter;
	CCritSec* m_section;
	CRefTime m_rtStart;
  bool m_bDropPackets;
};

#endif
