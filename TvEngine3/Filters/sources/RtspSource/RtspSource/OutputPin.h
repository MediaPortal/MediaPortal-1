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
#pragma once

#include "rtspSourceFilter.h"
class COutputPin: public CSourceStream, public CSourceSeeking
{
public:
  COutputPin(LPUNKNOWN pUnk, CRtspSourceFilter *pFilter, HRESULT *phr,CCritSec* section);
  ~COutputPin(void);

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

	//CSourceStream
	HRESULT GetMediaType(CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT FillBuffer(IMediaSample *pSample);
	HRESULT CheckConnect(IPin *pReceivePin);
	HRESULT BreakConnect();

	// CSourceSeeking
	HRESULT Run(REFERENCE_TIME tStart);
	HRESULT ChangeStart();
	HRESULT ChangeStop();
	HRESULT ChangeRate();
  STDMETHODIMP GetDuration(LONGLONG *pDuration);
	STDMETHODIMP SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags);

	virtual HRESULT OnThreadStartPlay(void) ;
	void SetDuration(CRefTime& duration);
  void IsTimeShifting(bool onOff);
protected:
  HRESULT DisconnectOutputPins(IBaseFilter *pFilter);
  HRESULT DisconnectDemux();
  HRESULT SetDemuxClock(IReferenceClock *pClock);
  HRESULT SetAccuratePos(REFERENCE_TIME seektime);
	CRtspSourceFilter *	const m_pFilter;
	CCritSec* m_section;
  CCritSec m_SeekLock;
  CCritSec m_FillLock;
  bool m_DemuxLock;
	bool m_bRunning;
  bool m_bSeeking;
  bool m_biMpegDemux;
  bool m_bIsTimeShifting;
  bool m_bDisContinuity;
  IBaseFilter* m_mpegDemuxerFilter;
};
