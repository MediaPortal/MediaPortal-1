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
#include "bdreader.h"

#define MAX_BUFFER_SIZE 0x40000

#define CHANNEL_LAYOUTS 4
#define CHANNEL_LAYOUT_NORMAL 0
#define CHANNEL_LAYOUT_5POINT1 1
#define CHANNEL_LAYOUT_7POINT0 2
#define CHANNEL_LAYOUT_7POINT1 3
#define CHANNELS_IN_LAYOUTS 8

// Channel mapping for the different channel layouts - most are the first but
// 5.1, 7.0 and 7.1 are different
const int CHANNEL_MAP[CHANNEL_LAYOUTS][CHANNELS_IN_LAYOUTS]=
{
  {0,1,2,3,4,5,6,7},
  {0,1,2,4,5,3,6,7},
  {0,1,2,5,3,4,6,7},
  {0,1,2,6,4,5,7,3}
};

const int channel_map_layouts[16] = 
{0, CHANNEL_LAYOUT_NORMAL, 0, CHANNEL_LAYOUT_NORMAL, CHANNEL_LAYOUT_NORMAL,
  CHANNEL_LAYOUT_NORMAL, CHANNEL_LAYOUT_NORMAL, CHANNEL_LAYOUT_NORMAL, CHANNEL_LAYOUT_NORMAL,
  CHANNEL_LAYOUT_5POINT1, CHANNEL_LAYOUT_7POINT0, CHANNEL_LAYOUT_7POINT1, 0, 0, 0, 0 };


class CAudioPin : public CSourceStream, public CSourceSeeking
{
public:
  CAudioPin(LPUNKNOWN pUnk, CBDReaderFilter *pFilter, HRESULT* phr, CCritSec* section);
  ~CAudioPin();

  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // CSourceStream
  HRESULT GetMediaType(CMediaType* pMediaType);
  HRESULT DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pRequest);
  HRESULT CompleteConnect(IPin* pReceivePin);
  HRESULT CheckConnect(IPin* pReceivePin);
  HRESULT FillBuffer(IMediaSample* pSample);
  HRESULT BreakConnect();

  // CSourceSeeking
  HRESULT ChangeStart();
  HRESULT ChangeStop();
  HRESULT ChangeRate();
  STDMETHODIMP SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags);
  STDMETHODIMP GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest);
  STDMETHODIMP GetDuration(LONGLONG* pDuration);
  STDMETHODIMP GetCurrentPosition(LONGLONG* pCurrent);
  STDMETHODIMP Notify(IBaseFilter* pSender, Quality q);

  HRESULT OnThreadStartPlay();
  void SetStart(CRefTime rtStartTime);
  bool IsConnected();
  void SetDiscontinuity(bool onOff);

  void SetInitialMediaType(const CMediaType* pmt);

protected:
  void      UpdateFromSeek();
  void      JoinAudioBuffers(Packet* pBuffer, CDeMultiplexer* pDemuxer);
  void      ProcessAudioSample(Packet* pBuffer, IMediaSample* pSample);
  void      CreateEmptySample(IMediaSample* pSample);

  inline void ConvertLPCMFromBE(BYTE* src, void* dest, int channels, int nSamples, int sampleSize, int channelMap);
  
  CBDReaderFilter* const m_pFilter;
  bool      m_bConnected;
  BOOL      m_bDiscontinuity;
  CCritSec* m_section;
  bool      m_bPresentSample;

  IPinConnection* m_pPinConnection;
  IPin* m_pReceiver;

  CMediaType m_mtInitial;

  int prevPl;
  CRefTime m_rtPrevSample;

  Packet* m_pCachedBuffer;
};

#endif
