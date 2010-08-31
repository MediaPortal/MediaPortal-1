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
  IAudioSink *m_pNextSink;

};
