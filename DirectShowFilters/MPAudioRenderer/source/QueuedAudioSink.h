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
#include <queue>
#include <vector>
#include "BaseAudioSink.h"

using namespace std;

class CQueuedAudioSink : public CBaseAudioSink
{
public:
  CQueuedAudioSink();
  virtual ~CQueuedAudioSink();

// IAudioSink implementation
public:
  // Control
  virtual HRESULT Start(REFERENCE_TIME rtStart);
  virtual HRESULT Run(REFERENCE_TIME rtStart);
  virtual HRESULT Pause();
  virtual HRESULT BeginStop();
  virtual HRESULT EndStop();

  // Processing
  virtual HRESULT PutSample(IMediaSample *pSample);
  virtual HRESULT EndOfStream();
  virtual HRESULT BeginFlush();
  virtual HRESULT EndFlush();

// Queue services
protected:
  enum AudioSinkCommand;

  virtual HRESULT PutCommand(AudioSinkCommand nCommand);
  virtual HRESULT PutOOBCommand(AudioSinkCommand nCommand);

  virtual HRESULT WaitForEvents(DWORD dwTimeout, vector<HANDLE>* handles, vector<DWORD>* dwWaitObjects);
  virtual HRESULT GetNextSampleOrCommand(AudioSinkCommand* pCommand, IMediaSample** pSample, DWORD dwTimeout, vector<HANDLE>* pHandles, vector<DWORD>* pWaitObjects, bool handleOOBOnly = false);

  //__inline HANDLE &StopThreadEvent() { return m_hEvents[0]; };
  //__inline HANDLE &InputSamplesAvailableEvent() { return m_hEvents[1]; };

// Internal implementation
protected:
  enum AudioSinkCommand
  {
    ASC_Nop = 0,
    ASC_PutSample,
    ASC_Flush,
    ASC_Stop,
    ASC_Pause,
    ASC_Resume
  };

  struct TQueueEntry 
  {
    AudioSinkCommand Command;
    CComPtr<IMediaSample> Sample;
    TQueueEntry(AudioSinkCommand nCommand) : Command(nCommand), Sample(NULL) {};
    TQueueEntry(IMediaSample *pSample) : Command(ASC_PutSample), Sample(pSample) {};
  };

protected:
  static DWORD WINAPI ThreadEntryPoint(LPVOID lpParameter);
  virtual DWORD ThreadProc() = 0;

  virtual HRESULT CloseThread();
  virtual HRESULT StartThread();

protected:
  DWORD  m_ThreadId;
  HANDLE m_hThread;
  HANDLE m_hStopThreadEvent;
  HANDLE m_hInputAvailableEvent;
  HANDLE m_hOOBCommandAvailableEvent;
  //HANDLE m_hInputQueueEmptyEvent;
  HANDLE m_hCurrentSampleReleased;

  vector<HANDLE> m_hEvents;
  vector<DWORD> m_dwWaitObjects;

  CCritSec m_inputQueueLock;
  vector<TQueueEntry> m_inputQueue;

  CCritSec m_OOBInputQueueLock;
  queue<TQueueEntry> m_OOBInputQueue;

  // Lock against this when dealing with resources that are used from the worker thread
  CCritSec  m_csResources;

  CComPtr<IMediaSample> m_pCurrentSample;
};
