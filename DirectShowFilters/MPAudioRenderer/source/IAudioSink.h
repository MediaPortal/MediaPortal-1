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


enum ChannelOrder
{
  DS_ORDER,
  AC3_ORDER
};

//typedef interface IAudioSink IAudioSink;

struct  IAudioSink
{
public:
  // Initialization
  virtual HRESULT ConnectTo(IAudioSink *pSink) = 0;
  virtual HRESULT Disconnect() = 0;
  virtual HRESULT DisconnectAll() = 0;

  virtual HRESULT Init() = 0;
  virtual HRESULT Cleanup() = 0;

  // Control
  virtual HRESULT Start(REFERENCE_TIME rtStart) = 0;
  virtual HRESULT Run(REFERENCE_TIME rtStart) = 0;
  virtual HRESULT Pause() = 0;
  virtual HRESULT BeginStop() = 0;
  virtual HRESULT EndStop() = 0;

  // Format negotiation
  virtual HRESULT NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder) = 0;

  // Buffer negotiation
  // -- TODO --
  // It should be possible for a chain of filters to negotiate buffer sizes
  // when some filters intend to reuse the IMediaSample buffers passed in PutSample()

  // Processing
  virtual HRESULT PutSample(IMediaSample *pSample) = 0;
  virtual HRESULT EndOfStream() = 0;
  virtual HRESULT BeginFlush() = 0;
  virtual HRESULT EndFlush() = 0;

};
