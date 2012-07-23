// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#pragma once
#include "stdafx.h"
#include <MMReg.h>  //must be before other Wasapi headers
#include <ks.h>
#include <ksmedia.h>
#include "../AC3_encoder/ac3enc.h"
#include "BaseAudioSink.h"
#include "Settings.h"

#define AC3_FRAME_LENGTH    (1536)
#define AC3_MAX_BITRATE     (640000)
#define AC3_MIN_SAMPLE_RATE (32000)

#define AC3_MAX_COMP_FRAME_SIZE (AC3_MAX_BITRATE * AC3_FRAME_LENGTH / AC3_MIN_SAMPLE_RATE / 8)
#define AC3_BITSTREAM_OVERHEAD  (8*sizeof(WORD))
// Each data burst should have the length of the equivalent 16 bit stereo burst of the same number of samples
#define AC3_DATA_BURST_LENGTH   (4*AC3_FRAME_LENGTH)

class CAC3EncoderFilter :
  public CBaseAudioSink
{
public:
  CAC3EncoderFilter(AudioRendererSettings* pSettings);
  virtual ~CAC3EncoderFilter();

// IAudioSink implementation
public:
  // Initialization
  virtual HRESULT Init();
  virtual HRESULT Cleanup();

  // Format negotiation
  virtual HRESULT NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder);

  // Processing
  virtual HRESULT PutSample(IMediaSample* pSample);
  virtual HRESULT EndOfStream();
  virtual HRESULT BeginFlush();
  virtual HRESULT EndFlush();

protected:
  // Initialization
  HRESULT OnInitAllocatorProperties(ALLOCATOR_PROPERTIES* properties);

  WAVEFORMATEXTENSIBLE* CreateAC3Format(int nSamplesPerSec, int nAC3BitRate);

  // AC3 Encoding
  HRESULT OpenAC3Encoder(unsigned int bitrate, unsigned int channels, unsigned int sampleRate);
  HRESULT CloseAC3Encoder();
  long CreateAC3Bitstream(void* buf, size_t size, BYTE* pDataOut);

  // Processing
  //HRESULT ProcessPassThroughData(const BYTE *pData, long cbData, long *pcbDataProcessed);
  HRESULT ProcessAC3Data(const BYTE* pData, long cbData, long* pcbDataProcessed);
  //__inline HRESULT ProcessData(const BYTE *pData, long cbData, long *pcbDataProcessed)
  //  { return m_bPassThrough? ProcessPassThroughData(pData, cbData, pcbDataProcessed) : ProcessAC3Data(pData, cbData, pcbDataProcessed); };
  HRESULT ProcessAC3Frame(const BYTE* pData);

protected:
  bool m_bPassThrough;
  BYTE* m_pRemainingInput;  // buffer for data left over from previous PutSample() call
  int m_cbRemainingInput;   // valid byte count in above buffer
  int m_nFrameSize;         // uncompressed size in bytes, based on input format
  
  REFERENCE_TIME m_rtInSampleTime; 
  REFERENCE_TIME m_rtNextIncomingSampleTime;

  int m_nBitRate;
  AC3CodecContext* m_pEncoder;
  int m_nMaxCompressedAC3FrameSize; // based on output format; should always be even

  AudioRendererSettings* m_pSettings;
};
