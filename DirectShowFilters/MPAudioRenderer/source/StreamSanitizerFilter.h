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
#include "BaseAudioSink.h"
#include "Settings.h"
#include "..\libresample\src\samplerate.h"

class CStreamSanitizer : public CBaseAudioSink
{
public:
  CStreamSanitizer(AudioRendererSettings* pSettings);
  virtual ~CStreamSanitizer();

public:
  HRESULT Init();
  HRESULT Cleanup();

  HRESULT NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder);
  HRESULT PutSample(IMediaSample* pSample);

protected:
  void OutputSample();

protected:

  REFERENCE_TIME m_rtNextIncomingSampleTime;
  REFERENCE_TIME m_rtInSampleTime;

  REFERENCE_TIME m_rtUncorrectedError;

  AudioRendererSettings* m_pSettings;
};

