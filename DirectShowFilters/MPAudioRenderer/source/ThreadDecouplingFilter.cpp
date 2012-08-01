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

#include "stdafx.h"
#include "ThreadDecouplingFilter.h"
#include "Globals.h"

#include "alloctracing.h"

HRESULT CThreadDecouplingFilter::EndOfStream()
{
  // next filter does not know it runs on a separate thread
  // we should queue an EOS marker so that it gets processed in 
  // the same thread as the audio data.
  PutSample(NULL);
  // wait until input queue is empty
  //if(m_hInputQueueEmptyEvent)
  //  WaitForSingleObject(m_hInputQueueEmptyEvent, END_OF_STREAM_FLUSH_TIMEOUT); // TODO make this depend on the amount of data in the queue
  return S_OK;
}


DWORD CThreadDecouplingFilter::ThreadProc()
{
  CComPtr<IMediaSample> pSample;
  HRESULT hr = S_FALSE;

  while(true)
  {
    hr = GetNextSampleOrCommand(NULL, &pSample, INFINITE, NULL, NULL);
    if (hr == MPAR_S_THREAD_STOPPING)
      return 0;
    if (m_pNextSink)
    {
      if (pSample)
        m_pNextSink->PutSample(pSample);
      else
        m_pNextSink->EndOfStream();
    }
  }
}
