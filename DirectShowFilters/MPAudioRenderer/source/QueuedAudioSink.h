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
#include <queue>
#include "BaseAudioSink.h"

#define MPAR_S_THREAD_STOPPING ((HRESULT)0x00040200)

class CQueuedAudioSink :
  public CBaseAudioSink
{
public:
  CQueuedAudioSink(void);
  virtual ~CQueuedAudioSink(void);

// IAudioSink implementation
public:
  // Control
  virtual HRESULT Start();
  virtual HRESULT BeginStop();
  virtual HRESULT EndStop();

  // Processing
  virtual HRESULT PutSample(IMediaSample *pSample);
  virtual HRESULT EndOfStream();
  virtual HRESULT BeginFlush();
  //virtual HRESULT EndFlush();

// Queue services
protected:
  virtual HRESULT WaitForSample(DWORD dwTimeout);
  virtual HRESULT GetNextSample(IMediaSample **pSample, DWORD dwTimeout);

  __inline HANDLE &StopThreadEvent() { return m_hEvents[0]; };
  __inline HANDLE &InputSamplesAvailableEvent() { return m_hEvents[1]; };

// Internal implementation
protected:
  static DWORD WINAPI ThreadEntryPoint(LPVOID lpParameter);
  virtual DWORD ThreadProc() = 0;

protected:
  HANDLE m_hThread;
  HANDLE m_hEvents[2];
  //HANDLE m_hStopThreadEvent;
  //HANDLE m_hInputQueueHasSamplesEvent;
  //HANDLE m_hInputQueueEmptyEvent;

  CCritSec m_InputQueueLock;
  std::queue<CComPtr<IMediaSample>> m_InputQueue;
};
