/**
*  TSParserSinkFilter.cpp
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  authors can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include "stdafx.h"
#include "TSParserSink.h"

//////////////////////////////////////////////////////////////////////
// CTSParserSinkFilter
//////////////////////////////////////////////////////////////////////
CTSParserSinkFilter::CTSParserSinkFilter(CTSParserSink *pTSParserSink, IUnknown *pUnk, CCritSec *pLock, HRESULT *phr) :
	CBaseFilter(NAME("CTSParserSinkFilter"), pUnk, pLock, CLSID_TSParserSink),
	m_pTSParserSink(pTSParserSink)
{
}

CTSParserSinkFilter::~CTSParserSinkFilter()
{
}

CBasePin * CTSParserSinkFilter::GetPin(int n)
{
    if (n == 0) {
        return m_pTSParserSink->m_pPin;
    } else {
        return NULL;
    }
}

int CTSParserSinkFilter::GetPinCount()
{
    return 1;
}

STDMETHODIMP CTSParserSinkFilter::Stop()
{
    CAutoLock cObjectLock(m_pLock);

	m_pTSParserSink->m_pPin->StopThread(100);

    if (m_pTSParserSink)
        m_pTSParserSink->CloseFile();
    
    return CBaseFilter::Stop();
}

STDMETHODIMP CTSParserSinkFilter::Pause()
{
    CAutoLock cObjectLock(m_pLock);

    return CBaseFilter::Pause();
}

STDMETHODIMP CTSParserSinkFilter::Run(REFERENCE_TIME tStart)
{
    CAutoLock cObjectLock(m_pLock);

    if (m_pTSParserSink)
    	m_pTSParserSink->OpenFile();

    return CBaseFilter::Run(tStart);
}

