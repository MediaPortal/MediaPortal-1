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
#include "stdafx.h"
#include <MMReg.h>  //must be before other Wasapi headers
#include <ks.h>
#include <ksmedia.h>
#include "../AC3_encoder/ac3enc.h"
#include "BaseAudioSink.h"

class CAC3EncoderFilter :
  public CBaseAudioSink
{
public:
  CAC3EncoderFilter(void);
  virtual ~CAC3EncoderFilter(void);

// IAudioSink implementation
public:
  // Initialization
  virtual HRESULT Init();
  virtual HRESULT Cleanup();

  // Format negotiation
  virtual HRESULT NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth);

  // Processing
  virtual HRESULT PutSample(IMediaSample *pSample);
  virtual HRESULT EndOfStream();
  virtual HRESULT BeginFlush();
  virtual HRESULT EndFlush();

protected:
  bool FormatsEqual(const WAVEFORMATEX *pwfx1, const WAVEFORMATEX *pwfx2);
  WAVEFORMATEX *CreateAC3Format(int nSamplesPerSec, int nAC3BitRate);

  // AC3 Encoding
  HRESULT OpenAC3Encoder(unsigned int bitrate, unsigned int channels, unsigned int sampleRate);
  HRESULT CloseAC3Encoder();
  long CreateAC3Bitstream(void *buf, size_t size, BYTE *pDataOut);


protected:
  bool m_bPassThrough;
  WAVEFORMATEX *m_pInputFormat;
  WAVEFORMATEX *m_pOutputFormat;

  CComPtr<IMediaSample> m_pLastSample;
  AC3CodecContext* m_pEncoder;
};
