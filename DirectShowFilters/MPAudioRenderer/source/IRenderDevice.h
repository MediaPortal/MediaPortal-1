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

#include <BaseTsd.h>

#include <dsound.h>

#include <MMReg.h>  //must be before other Wasapi headers
#include <strsafe.h>
#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>
#include <Endpointvolume.h>

#include "Settings.h"

class IRenderDevice
{
public:

  IRenderDevice(){};
  virtual ~IRenderDevice(){};

  virtual HRESULT CheckFormat(WAVEFORMATEX* pwfx) = 0;
  virtual HRESULT SetMediaType(WAVEFORMATEX* pwfx) = 0;
  virtual HRESULT CompleteConnect(IPin *pReceivePin) = 0;

  virtual HRESULT DoRenderSample(IMediaSample *pMediaSample, LONGLONG pSampleCounter) = 0;
  virtual void    OnReceiveFirstSample(IMediaSample *pMediaSample) = 0;

  virtual HRESULT EndOfStream() = 0;
  virtual HRESULT BeginFlush() = 0;
  virtual HRESULT EndFlush() = 0;

  virtual HRESULT Run(REFERENCE_TIME tStart) = 0;
  virtual HRESULT Stop(FILTER_STATE pState) = 0;
  virtual HRESULT Pause(FILTER_STATE pState) = 0;

  virtual HRESULT StopRendererThread() = 0;

  virtual HRESULT SetRate(double dRate) = 0;

  virtual HRESULT AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc) = 0;

  virtual REFERENCE_TIME Latency() = 0;
};
