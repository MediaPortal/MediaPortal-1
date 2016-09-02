/*
 *  Copyright (C) 2005-2013 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#pragma once
#include <DShow.h>    // REFERENCE_TIME
#include <streams.h>  // CAutoLock, CBaseFilter, CBasePin, CCritSec, CUnknown (IUnknown, LPUNKNOWN)
#include <InitGuid.h> // DEFINE_GUID()
#include <WinError.h> // HRESULT
#include <sstream>
#include "..\shared\Thread.h"
#include "InputPinOobSi.h"
#include "InputPinTs.h"
#include "ITsAnalyser.h"

using namespace std;


#define STREAM_IDLE_TIMEOUT 1000


// {fc50bed6-fe38-42d3-b831-771690091a6e}
DEFINE_GUID(CLSID_TS_WRITER, 0xfc50bed6, 0xfe38, 0x42d3, 0xb8, 0x31, 0x77, 0x16, 0x90, 0x09, 0x1a, 0x6e);

class CTsWriterFilter : public CBaseFilter
{
  public:
    CTsWriterFilter(ITsAnalyser* analyser,
                    const wchar_t* debugPath,
                    LPUNKNOWN unk,
                    CCritSec* filterLock,
                    CCritSec& receiveLock,
                    HRESULT* hr);
    virtual ~CTsWriterFilter();

    CBasePin* GetPin(int n);
    int GetPinCount();

    STDMETHODIMP Pause();
    STDMETHODIMP Run(REFERENCE_TIME startTime);
    STDMETHODIMP Stop();

    STDMETHODIMP SetDumpFilePath(wchar_t* path);
    STDMETHODIMP DumpInput(bool enableTs, bool enableOobSi);
    void CheckSectionCrcs(bool enable);

  private:
    class CThreadContext
    {
      public:
        CTsWriterFilter* m_filter;
        bool m_isReceivingOobSi;
        bool m_isReceivingTs;
    };

    static bool __cdecl StreamingMonitorThreadFunction(void* arg);

    ITsAnalyser* m_analyser;
    CInputPinOobSi* m_inputPinOobSi;    // SCTE 65 out-of-band service information
    CInputPinTs* m_inputPinTs;          // MPEG 2 transport stream
    CCritSec& m_receiveLock;            // sample receive lock

    CThread m_streamingMonitorThread;
    CThreadContext m_streamingMonitorThreadContext;

    wstringstream m_debugPath;
    bool m_isDebugEnabledOobSi;
    bool m_isDebugEnabledTs;
};