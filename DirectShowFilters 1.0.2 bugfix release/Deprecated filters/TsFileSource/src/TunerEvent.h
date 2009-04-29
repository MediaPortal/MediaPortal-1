/**
*  TunerEvent.h
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

#ifndef TUNEREVENT_H
#define TUNEREVENT_H

#include "tuner.h"
#include <commctrl.h>
#include <atlbase.h>
#include "KS.H"
#include "KSMEDIA.H"
#include "bdamedia.h"

#include "TSFileSource.h"
//#include "Demux.h"

/**********************************************
 *
 *  TunerEvent Class
 *
 **********************************************/

class TunerEvent : public IBroadcastEvent
{

protected:
    long m_nRefCount; // Holds the reference count.

	CTSFileSourceFilter *m_pTSFileSourceFilter;
	
public: //private

	TunerEvent(CTSFileSourceFilter *pFilter);
	virtual ~TunerEvent();

	STDMETHODIMP_(ULONG) AddRef();
	STDMETHODIMP_(ULONG) Release();
	STDMETHODIMP QueryInterface(REFIID riid, void **ppvObject);
	STDMETHOD(Fire)(GUID eventID);
	void SetEventFlag(bool flag);

    HRESULT HookupGraphEventService(IFilterGraph *pGraph);
    HRESULT RegisterForTunerEvents();
    HRESULT UnRegisterForTunerEvents();
    HRESULT Fire_Event(GUID eventID);

protected:
	HRESULT DoChannelChange(void);


protected:

    CComPtr <IBroadcastEvent> m_spBroadcastEvent; 
    DWORD m_dwBroadcastEventCookie;
	bool EventBusyFlag;
	int m_bNPSlaveSave;
	int m_bNPControlSave;
};

#endif
