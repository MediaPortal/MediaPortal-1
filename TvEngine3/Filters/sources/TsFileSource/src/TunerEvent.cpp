/**
*  TunerEvent.cpp
*  Copyright (C) 2004-2006 bear
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
*  bear can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include "stdafx.h"
#include "TunerEvent.h"
//#include "ITSFileSource.h"

TunerEvent::TunerEvent(CTSFileSourceFilter *pFilter) : m_dwBroadcastEventCookie(0), m_nRefCount(1)
{
	m_pTSFileSourceFilter = pFilter;
	m_spBroadcastEvent = NULL;
}

TunerEvent::~TunerEvent()
{
}

// IUnknown methods
ULONG TunerEvent::AddRef()
{
	return InterlockedIncrement(&m_nRefCount);
}

STDMETHODIMP TunerEvent::QueryInterface(REFIID riid, void **ppvObject)
{
	if (NULL == ppvObject)
		return E_POINTER;
	if (riid == __uuidof(IUnknown))
		*ppvObject = static_cast<IUnknown*>(this);
	else if (riid == __uuidof(IBroadcastEvent))
		*ppvObject = static_cast<IBroadcastEvent*>(this);
	else 
		return E_NOINTERFACE;

	AddRef();
	return S_OK;
}

ULONG TunerEvent::Release()
{
	_ASSERT(m_nRefCount >= 0);
	ULONG uCount = InterlockedDecrement(&m_nRefCount);
	if (uCount == 0)
	{
		delete this;
	}
	// Return the temporary variable, not the member
	// variable, for thread safety.
	return uCount;
}

// Query the Filter Graph Manager for the broadcast event service.
// If not found, create it and register it.
HRESULT TunerEvent::HookupGraphEventService(IFilterGraph *pGraph)
{
	
    HRESULT hr = S_OK;
    if (!m_spBroadcastEvent)
    {
        CComQIPtr<IServiceProvider> spServiceProvider(pGraph);
        if (!spServiceProvider)
        {
            return E_NOINTERFACE;
        }
        hr = spServiceProvider->QueryService(SID_SBroadcastEventService, 
            IID_IBroadcastEvent, 
            reinterpret_cast<void**>(&m_spBroadcastEvent));
        if (FAILED(hr))
        {
            // Create the Broadcast Event Service object.
            hr = m_spBroadcastEvent.CoCreateInstance(
                CLSID_BroadcastEventService,
                NULL, CLSCTX_INPROC_SERVER);
            if (FAILED(hr))
            {
                return hr; 
            }
            
            CComQIPtr<IRegisterServiceProvider> spRegService(pGraph);
            if (!spRegService)
            {
                return E_NOINTERFACE;
            }
            
            // Register the Broadcast Event Service object as a service.
            hr = spRegService->RegisterService(
                SID_SBroadcastEventService,
                m_spBroadcastEvent);
        }
    }
    return hr;
}

// Establish the connection point to receive events.
HRESULT TunerEvent::RegisterForTunerEvents()
{

	EventBusyFlag = false; //Make sure that we have Reset

	if (!m_spBroadcastEvent)
    {
        return E_FAIL;  // Forgot to call HookupGraphEventService.
    }

    if(m_dwBroadcastEventCookie)
    {
        return S_FALSE;  // There is already a connection; nothing to do.
    }

    CComQIPtr<IConnectionPoint> spConnectionPoint(m_spBroadcastEvent);
    if(!spConnectionPoint)
    {
        return E_NOINTERFACE;
    }

    return spConnectionPoint->Advise(static_cast<IBroadcastEvent*>(this),
        &m_dwBroadcastEventCookie);
}

// Unregister for events.
HRESULT TunerEvent::UnRegisterForTunerEvents()
{
    if (!m_spBroadcastEvent)
    {
        return S_OK; // Forgot to call HookupGraphEventService.
    }

    HRESULT hr = S_OK;
    if(!m_dwBroadcastEventCookie)
    {
        return S_OK; // Not registered for events; nothing to do.
    }

    CComQIPtr<IConnectionPoint> spConnectionPoint(m_spBroadcastEvent);
    if(!spConnectionPoint)
    {
        return E_NOINTERFACE;
    }
    
    // Release the connection point.
    hr = spConnectionPoint->Unadvise(m_dwBroadcastEventCookie);
    if (FAILED(hr))
    {
        // Error: Unable to unadvise the connection point.
        return hr;
    }
    m_dwBroadcastEventCookie = 0;
    m_spBroadcastEvent.Release();
    return S_OK;
}

// Send an event (for source objects).
HRESULT TunerEvent::Fire_Event(GUID eventID)
{
    if (!m_spBroadcastEvent)
    {
        return E_FAIL; // Forgot to call HookupGraphEventService.
    }
    return m_spBroadcastEvent->Fire(eventID);

}

HRESULT TunerEvent::DoChannelChange()
{
	HRESULT hr = S_FALSE;

//	ITSFileSource   *pProgram;    // Pointer to the filter's custom interface.
//	hr = m_pTSFileSourceFilter->QueryInterface(IID_ITSFileSource, (void**)(&pProgram));
//	if(SUCCEEDED(hr))
	if(m_pTSFileSourceFilter && m_pTSFileSourceFilter->m_pDemux)
	{
		m_bNPControlSave = m_pTSFileSourceFilter->m_pDemux->get_NPControl(); //Save NP Control mode
		m_pTSFileSourceFilter->m_pDemux->set_NPControl(false); //Turn off NP Control else we will loop

		m_bNPSlaveSave = m_pTSFileSourceFilter->m_pDemux->get_NPSlave(); //Save NP Slave mode
		m_pTSFileSourceFilter->m_pDemux->set_NPSlave(true); //Turn on NP Slave mode to change SID to set Demux control

		USHORT pgmNumb;
		m_pTSFileSourceFilter->GetPgmNumb(&pgmNumb);
		m_pTSFileSourceFilter->SetPgmNumb(pgmNumb);
//		pProgram->GetPgmNumb(&pgmNumb);
//		pProgram->SetPgmNumb(pgmNumb);
		m_pTSFileSourceFilter->m_pDemux->set_NPControl(m_bNPControlSave); //Restore NP Control mode
		m_pTSFileSourceFilter->m_pDemux->set_NPSlave(m_bNPSlaveSave); //Restore NP Control mode

//		pProgram->Release();
	}
	return hr;
}

void TunerEvent::SetEventFlag(bool flag)
{
	EventBusyFlag = flag;
}

// The one IBroadcastEvent method.
HRESULT TunerEvent::Fire(GUID eventID)
{
	// The tuner changed stations or channels.
	if (eventID == EVENTID_TuningChanged && !EventBusyFlag)
	{
		EventBusyFlag = true; //set Event flag to prevent looping
		DoChannelChange(); //Change The Channel
		EventBusyFlag = false; //Reset set Event flag to prevent looping
	}
	return S_OK;
}


