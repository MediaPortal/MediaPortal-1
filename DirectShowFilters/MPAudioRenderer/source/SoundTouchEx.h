// Copyright (C) 2005-2010 Team MediaPortal
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

#include <dsound.h>
#include <ks.h>
#include <ksmedia.h>
#include <vector>
#include <deque>

#include <MMReg.h>  //must be before other Wasapi headers

#include "../SoundTouch/Include/SoundTouch.h"

//#undef INTEGER_SAMPLES

// Uncomment following line to enable dithering when 
// converting samples to lower bit depth
//#define DITHER_SAMPLES

class CSoundTouchEx : public soundtouch::SoundTouch
{
public:
  CSoundTouchEx();
  virtual ~CSoundTouchEx();

  bool StartResampling();
  bool StopResampling();

  virtual void putBuffer(const BYTE *pInBuffer, int numSamples);
  virtual void putMediaSample(IMediaSample *pMediaSample);

  virtual int getBuffer(BYTE *pOutBuffer, int maxSamples);

  bool SetInputFormat(int frameSize, int bytesPerSample);
  void SetInputChannels(int leftOffset, int rightOffset = 0);
  bool SetOutputFormat(int frameSize, int bytesPerSample);
  void SetOutputChannels(int leftOffset, int rightOffset = 0);
  bool SetupFormats();

  uint numChannels()  { return channels; };

protected:
  typedef void (CSoundTouchEx::*DeInterleaveFunc)(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count);
  typedef void (CSoundTouchEx::*InterleaveFunc)(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count);

  // Input buffer layout
  int m_nInFrameSize;       // Bytes in a frame. A frame contains a sample for each channel
  int m_nInBytesPerSample;  // Bytes in a sample. Can be 1 to 4. This is the "container" size
  int m_nInLeftOffset;      // The offset in bytes from the start of a frame of the channel sample to map to the Left channel
  int m_nInRightOffset;     // The offset in bytes from the start of a frame of the channel sample to map to the Right channel
  DeInterleaveFunc m_pfnDeInterleave;

  // Input queue
  std::vector<CComPtr<IMediaSample>> m_InSampleQueue;
  CCritSec m_InSampleQueueLock;

  // Output buffer layout
  int m_nOutFrameSize;      // Bytes in a frame. A frame contains a sample for each channel
  int m_nOutBytesPerSample; // Bytes in a sample. Can be 1 to 4. This is the "container" size
  int m_nOutLeftOffset;     // The offset in bytes from the start of a frame of the channel sample to map to the Left channel
  int m_nOutRightOffset;    // The offset in bytes from the start of a frame of the channel sample to map to the Right channel
  InterleaveFunc m_pfnInterleave;

  // Output queue
  std::vector<CComPtr<IMediaSample>> m_OutSampleQueue;
  CCritSec m_OutSampleQueueLock;

  // Temporary input buffer
  static const uint BATCH_LEN = 0x4000;
  soundtouch::SAMPLETYPE m_tempBuffer[2*BATCH_LEN];

  // internal functions to separate/merge streams out of sample buffers
  virtual void StereoDeInterleave(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count);
  virtual void StereoInterleave(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count);
  virtual void MonoDeInterleave(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count);
  virtual void MonoInterleave(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count);
};

