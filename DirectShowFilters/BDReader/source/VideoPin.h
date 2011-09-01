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

#include "bdreader.h"
#include <initguid.h>

// D979F77B-DBEA-4BF6-9E6D-1D7E57FBAD53
DEFINE_GUID(MEDIASUBTYPE_WVC1_CYBERLINK,
      0xD979F77B, 0xDBEA, 0x4BF6, 0x9E, 0x6D, 0x1D, 0x7E, 0x57, 0xFB, 0xAD, 0x53);

// 629B40AD-AD74-4EF4-A985-F0C8D92E5ECA
DEFINE_GUID(MEDIASUBTYPE_WVC1_ARCSOFT,
      0x629B40AD, 0xAD74, 0x4EF4, 0xA9, 0x85, 0xF0, 0xC8, 0xD9, 0x2E, 0x5E, 0xCA);

class CVideoPin : public CSourceStream, public CSourceSeeking
{
public:

  enum VIDEO_DECODER
  {
    general = 0,
    Arcsoft,
    Cyberlink
  };

  CVideoPin(LPUNKNOWN pUnk, CBDReaderFilter *pFilter, HRESULT *phr,CCritSec* section);
  ~CVideoPin();

  STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

  // CSourceStream
  HRESULT GetMediaType(CMediaType *pMediaType);
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
  void SetStart(CRefTime rtStartTime);
  bool IsConnected();

  void SetInitialMediaType(const CMediaType* pmt);

protected:
  void DetectVideoDecoder();
  void UpdateFromSeek();

  void CreateEmptySample(IMediaSample* pSample);
  
  CBDReaderFilter * const m_pFilter;
  bool      m_bConnected;
  CCritSec* m_section;

  VIDEO_DECODER m_decoderType;

  IPinConnection* m_pPinConnection;
  IPin* m_pReceiver;

  CMediaType m_mtInitial;

  int       m_nPrevPl;

  Packet* m_pCachedBuffer;
};

