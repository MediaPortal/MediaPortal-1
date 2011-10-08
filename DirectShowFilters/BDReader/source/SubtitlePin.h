/*
 *  Copyright (C) 2005-2011 Team MediaPortal
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

#include "StdAfx.h"
#include "bdreader.h"

class CSubtitlePin: public CSourceStream, public CSourceSeeking
{
public:
  CSubtitlePin(LPUNKNOWN pUnk, CBDReaderFilter* pFilter, HRESULT* phr, CCritSec* section);
  ~CSubtitlePin();

  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // CSourceStream
  HRESULT GetMediaType(CMediaType* pMediaType);
  HRESULT DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pRequest);
  HRESULT CompleteConnect(IPin* pReceivePin);
  HRESULT CheckConnect(IPin* pReceivePin);
  HRESULT FillBuffer(IMediaSample* pSample);
  HRESULT BreakConnect();
  HRESULT OnThreadStartPlay();

  // CSourceSeeking
  HRESULT ChangeStart();
  HRESULT ChangeStop();
  HRESULT ChangeRate();
  STDMETHODIMP GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest);
  STDMETHODIMP SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags);
  STDMETHODIMP GetDuration(LONGLONG* pDuration);
  STDMETHODIMP GetCurrentPosition(LONGLONG* pCurrent);
  HRESULT DeliverBeginFlush();
  HRESULT DeliverEndFlush();
  HRESULT DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);

	void SetRunningStatus(bool onOff);
  bool IsConnected();

protected:
  DWORD ThreadProc();

  void CreateEmptySample(IMediaSample *pSample);

  CBDReaderFilter* const m_pFilter;
  bool      m_bConnected;
  BOOL      m_bDiscontinuity;
  CCritSec* m_section;
  bool      m_bSeekDone;
  bool      m_bFlushing;
  DWORD     m_seekTimer;
  CRefTime  m_lastSeek;
  bool      m_bRunning;
};
