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
#ifndef __AudioPin_H
#define __AudioPin_H
#include "tsreader.h"
#include "mediaseeking.h"

#define NB_AFTSIZE 300

class CAudioPin : public CSourceStream, public CSourceSeeking
{
public:
  CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section);
  ~CAudioPin();

  STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

  //CSourceStream
  HRESULT CheckMediaType(const CMediaType* pmt);
  HRESULT GetMediaType(int iPosition, CMediaType *pMediaType);
  HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
  HRESULT CompleteConnect(IPin *pReceivePin);
  HRESULT CheckConnect(IPin *pReceivePin);
  HRESULT FillBuffer(IMediaSample *pSample);
  HRESULT BreakConnect();
  HRESULT DoBufferProcessingLoop(void);

  // CSourceSeeking
  HRESULT ChangeStart();
  HRESULT ChangeStop();
  HRESULT ChangeRate();
  STDMETHODIMP SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags);
  STDMETHODIMP GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest );
  STDMETHODIMP GetDuration(LONGLONG *pDuration);
  STDMETHODIMP GetCurrentPosition(LONGLONG *pCurrent);
  STDMETHODIMP Notify(IBaseFilter * pSender, Quality q);

  HRESULT OnThreadStartPlay();
  HRESULT DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);
  void SetStart(CRefTime rtStartTime);
  bool IsConnected();
  bool IsInFillBuffer();
  bool HasDeliveredSample();
  void SetDiscontinuity(bool onOff);
  void SetAddPMT();
  double GetAudToPresMeanDelta();
  DWORD  m_FillBuffSleepTime;
  double GetAudioPresToRefDiff();
  int GetPMTiPosition();
  bool IsThreadRunning(CRefTime *pRtStartTime);

protected:
  HRESULT   UpdateFromSeek();
  void      CreateEmptySample(IMediaSample *pSample);
  
  CTsReaderFilter * const m_pTsReaderFilter;
  bool      m_bConnected;
  BOOL      m_bDiscontinuity;
  CCritSec* m_section;
  bool      m_bPresentSample;
  bool      m_bInFillBuffer;
  bool      m_bDownstreamFlush;

  void     ClearAverageFtime();
  void     CalcAverageFtime(double ftime);

  double  m_pllAFT [NB_AFTSIZE];   // buffer for average Audio ftime calculation
  int     m_nNextAFT;
	double  m_fAFTMean;
	double  m_llAFTSumAvg;	
	double  m_fAFTMeanRef;
	int     m_nMaxAFT;
    
  DWORD m_LastFillBuffTime;
  int   m_sampleCount;
  bool  m_bPinNoAddPMT;
  bool  m_bAddPMT;
  bool  m_bDisableSlowPlayDiscontinuity;
  int   m_iPosition;
  bool  m_bThreadRunning;
  
};

#endif
