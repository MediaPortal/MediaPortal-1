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

#include "IAudioSink.h"

#define DEFAULT_OUT_BUFFER_COUNT  (4)
#define DEFAULT_OUT_BUFFER_SIZE   (0x1000)

typedef class CBaseAudioSink CNullAudioFilter;

class CBaseAudioSink :
  public IAudioSink
{
public:
  CBaseAudioSink(void);
  virtual ~CBaseAudioSink(void);

// IAudioSink implementation
// Provide default implementations
public:
  // Initialization
  virtual HRESULT ConnectTo(IAudioSink *pSink);
  virtual HRESULT Disconnect();
  virtual HRESULT DisconnectAll();

  virtual HRESULT Init();
  virtual HRESULT Cleanup();

  // Control
  virtual HRESULT Start();
  virtual HRESULT BeginStop();
  virtual HRESULT EndStop();

  // Format negotiation
  virtual HRESULT NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth);

  // Processing
  virtual HRESULT PutSample(IMediaSample *pSample);
  virtual HRESULT EndOfStream();
  virtual HRESULT BeginFlush();
  virtual HRESULT EndFlush();

protected:
  // Helpers
  static bool FormatsEqual(const WAVEFORMATEX *pwfx1, const WAVEFORMATEX *pwfx2);

  // Initialization
  HRESULT InitAllocator();
  virtual HRESULT OnInitAllocatorProperties(ALLOCATOR_PROPERTIES *properties);

  HRESULT SetInputFormat(const WAVEFORMATEX *pwfx);
  HRESULT SetInputFormat(WAVEFORMATEX *pwfx, bool bAssumeOwnerShip = FALSE);
  HRESULT SetOutputFormat(const WAVEFORMATEX *pwfx);
  HRESULT SetOutputFormat(WAVEFORMATEX *pwfx, bool bAssumeOwnerShip = FALSE);

  // Output handling
  HRESULT RequestNextOutBuffer(REFERENCE_TIME rtStart);
  HRESULT OutputNextSample();

protected:
  IAudioSink *m_pNextSink;
  WAVEFORMATEX *m_pInputFormat;
  WAVEFORMATEX *m_pOutputFormat;
  bool m_bOutFormatChanged;

  // Output buffer support
  CComQIPtr<IMemAllocator> m_pMemAllocator;
  CComPtr<IMediaSample> m_pNextOutSample;


};
