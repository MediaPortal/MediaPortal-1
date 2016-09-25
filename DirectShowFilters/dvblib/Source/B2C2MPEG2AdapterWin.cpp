/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: Agree
 *	http://www.team-mediaportal.com
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
#include "stdafx.h"
#include <stdio.h>

#include "Include\b2c2_defs.h"

#include <tchar.h>
#include <Dshow.h>
#include <initguid.h>
#include "Include\B2C2_Guids.h"

#if defined _B2C2_USE_DEVICE_NOTIFICATION

	#if WINVER < 0x0500
	#error WINVER >= 0x0500 must be defined at project settings in combination with _B2C2_USE_DEVICE_NOTIFICATION
	#endif // WINVER < 0x0500

	#include <Winuser.h>
	#include <ndisguid.h>

	#ifdef UNICODE
	#define	REGISTERDEVICENOTIFY	TEXT ("RegisterDeviceNotificationW")
	#else
	#define	REGISTERDEVICENOTIFY	"RegisterDeviceNotificationA"
	#endif

	#define	UNREGISTERDEVICENOTIFY	"UnregisterDeviceNotification"

	#define TRACE_NOTIFICATION		printf

#endif //defined _B2C2_USE_DEVICE_NOTIFICATION

#include "Include\b2c2mpeg2adapter.h"

#include "Include\ib2c2mpeg2tunerctrl.h"
#include "Include\ib2c2mpeg2datactrl.h"
#include "Include\ib2c2mpeg2avctrl.h"

B2C2MPEG2Adapter::B2C2MPEG2Adapter (const TCHAR *pszAdapterName )
{
	memset (&m_szLastErrorText, 0, sizeof (m_szLastErrorText));
	m_dwLastErrorCode = 0;

	// Initialize COM.
	CoInitialize(NULL);

	m_pFilterGraph = NULL;
	m_pFilter = NULL;

	m_pPinOutAudio = NULL;
	m_pPinOutVideo = NULL;

	m_pMediaControl = NULL;
	m_pMediaEvent= NULL;

	m_pIB2C2MPEG2TunerCtrl = NULL;
	m_pIB2C2MPEG2DataCtrl = NULL;
	m_pIB2C2MPEG2AvCtrl = NULL;

	for (int iCnt = 0; iCnt < B2C2_FILTER_MAX_TS_PINS; iCnt++)
	{
		m_pTsPinFilter[iCnt] = NULL;
		m_pTsPinInterfaceFilter[iCnt] = NULL;
		m_pTsOutPin[iCnt] = NULL;
		m_pTsFilterInPin[iCnt] = NULL;
	}

#if defined _B2C2_USE_DEVICE_NOTIFICATION
	m_hDevNotify = NULL;			// Because RegisterDeviceNotification returns NULL as error
	m_hUser32Dll = NULL;			// Because LoadLibrary returns NULL as error
#endif //defined _B2C2_USE_DEVICE_NOTIFICATION
}

B2C2MPEG2Adapter::~B2C2MPEG2Adapter ()
{
	Release ();
	
	// Tear down COM.

	CoUninitialize ();
}

HRESULT B2C2MPEG2Adapter::Initialize ()
{
 	HRESULT			hr;

	// **********************************************************************
	// *** Set up the Filter Graph
	// **********************************************************************

	hr = CoCreateInstance (CLSID_FilterGraph,
						   NULL,
						   CLSCTX_INPROC,
						   IID_IGraphBuilder,
						   (void**) &m_pFilterGraph);

	if (FAILED (hr)) 
	{
		SetLastError (TEXT ("Create Filter Graph"), hr);
		return hr;
	}

	// Create B2C2 Filter, which is the upstream source filter.

	hr = CoCreateInstance (CLSID_B2C2MPEG2Filter,
						   NULL,
						   CLSCTX_INPROC,
						   IID_IBaseFilter,
						   (void **)&m_pFilter);
	
	if (FAILED (hr))
	{
		SetLastError (TEXT ("Create B2C2 MPEG2 Filter"), hr);
		return hr;
	}

	// Add B2C2 Filter to Filter Graph:

	hr = m_pFilterGraph->AddFilter (m_pFilter, L"B2C2-Filter");

	if (FAILED (hr))
	{
		SetLastError (TEXT ("Add B2C2 Filter to Filter Graph"), hr);
		return hr;
	}

	// Get Filter Data Control interface.

	hr = m_pFilter->QueryInterface (IID_IB2C2MPEG2DataCtrl3, (VOID **)&m_pIB2C2MPEG2DataCtrl);
	
	if (FAILED (hr))
	{
		SetLastError (TEXT ("Query Interface IID_IB2C2MPEG2DataCtrl3"), hr);
		return hr;
	}

	// **********************************************************************
	// *** Configure tuner
	// **********************************************************************

	// Get B2C2 Filter Tuner Control interface in preparation for tuning.

	hr = m_pFilter->QueryInterface (IID_IB2C2MPEG2TunerCtrl2, (VOID **)&m_pIB2C2MPEG2TunerCtrl);
	
	if (FAILED (hr))
	{
		SetLastError (TEXT ("Query Interface IID_IB2C2MPEG2TunerCtrl2"), hr);
		return hr;
	}

	// Initialize the tuner

	hr = m_pIB2C2MPEG2TunerCtrl->Initialize ();

	if (FAILED (hr))
	{
		SetLastError (TEXT ("Initialize Tuner Control"), hr);
		return hr;
	}

	// Get Filter Audio/Video Control interface.

	hr = m_pFilter->QueryInterface (IID_IB2C2MPEG2AVCtrl, (VOID **)&m_pIB2C2MPEG2AvCtrl);

	if (FAILED (hr))
	{
		SetLastError (TEXT ("Query Interface IID_IB2C2MPEG2AVCtrl"), hr);
		return hr;
	}

	// **********************************************************************
	// *** Validate member creation
	// **********************************************************************

	if (   m_pIB2C2MPEG2TunerCtrl == NULL
		|| m_pIB2C2MPEG2DataCtrl == NULL
		|| m_pIB2C2MPEG2AvCtrl == NULL)
	{
		SetLastError (TEXT ("Internal Error, initialize"), B2C2_SDK_E_CREATE_INTERFACE);
		return B2C2_SDK_E_CREATE_INTERFACE;
	}

	// This must be here; to read tuner settings.

    m_pIB2C2MPEG2TunerCtrl->CheckLock ();

	/*
	BOOL bLock = !FAILED (m_pIB2C2MPEG2TunerCtrl->CheckLock());
	TRACE (TEXT ("Broadband4PC is %s lock!\n"), bLock ? TEXT ("in") : TEXT ("out of"));
	*/

	return S_OK;
}

// This function will enumerate all pins at once, 
// therefore need only to be called once per filter lifetime
// Pins will be released at Release ().

HRESULT B2C2MPEG2Adapter::EnumerateFilterPins(BOOL bAudoPin /*= TRUE*/, 
											  BOOL bVideoPin /*= TRUE*/, 
											  BOOL bTsPins /*= TRUE*/)
{
	HRESULT		hr;

	IEnumPins	*pEnum;
	PIN_INFO	pin_Info;
	IPin *		pPinOut = NULL;

	char		szPinName[128];	// from SDK, strmif.h; PIN_INFO::achName[ 128 ]
	char		szTsPinName[10];

	// Check if already initialized; got filter

	if (m_pFilter == NULL)
	{
		SetLastError (TEXT ("Not Initialize, enumerate pins"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Locate B2C2 Filter Audio and Video pins:

	// 1) Get list of filter's pins.
	
	hr = m_pFilter->EnumPins(&pEnum);

	if(FAILED (hr))
	{
		SetLastError (TEXT ("Enumerate B2C2 Filter pins"), hr);
		return hr;								// *** FUNCTION EXIT POINT
	}

	// 2) Locate Audio and Video pins.
	
	while(pEnum->Next(1, &pPinOut, NULL) == S_OK)
	{
		hr = pPinOut->QueryPinInfo(&pin_Info);

		if (pin_Info.pFilter != NULL)
		{
			pin_Info.pFilter->Release();
		}

		if(FAILED (hr))
		{	
			continue;
		}

		if(pin_Info.dir != PINDIR_OUTPUT)
		{
			continue;
		}

		wcstombs (szPinName, pin_Info.achName, sizeof (szPinName));

		BOOL bPinUsed = FALSE;

		// Check for TS pins first

		if (bTsPins)
		{
			for (int iCnt = 0; iCnt < B2C2_FILTER_MAX_TS_PINS; iCnt++)
			{
				sprintf (szTsPinName,"Data %d",iCnt);

				if (  m_pTsOutPin[iCnt] == NULL 
				   && bTsPins
				   && strstr (szPinName, szTsPinName) != NULL)
				{
					m_pTsOutPin[iCnt] = pPinOut;
					bPinUsed = TRUE;
					break;	// Don't need to go to next pin
				}
			}
		}

		if (bPinUsed)
		{
		}
		else
		// Check if Audio PIN; if still 'missing'
		if (  m_pPinOutAudio == NULL
			    && bAudoPin 
				&& strstr (szPinName, "Audio") != NULL)
		{
			m_pPinOutAudio = pPinOut;
		}
		else
		// Check if Video PIN; if still 'missing'
		if (  m_pPinOutVideo == NULL 
		   && bVideoPin
		   && strstr (szPinName, "Video") != NULL)
		{
			m_pPinOutVideo = pPinOut;
		}
		else
		{
			// No PIN of interest, or don't need this reference, 
			// so decrease the counter
			pPinOut->Release();
		}

		// See if we got all we need; for AV pins only
		if (   !bTsPins
			&& bVideoPin && m_pPinOutVideo
			&& bAudoPin && m_pPinOutAudio)
		{
			break;	// Got both pins.
		}
	}

	pEnum->Release();

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::GetAudioVideoOutPins(IPin **ppPinOutAudio, IPin **ppPinOutVideo)
{
	HRESULT		hr;

	// Check if already initialized; got filter

	if (m_pFilter == NULL)
	{
		SetLastError (TEXT ("Not Initialize, enumerate B2C2 Filter AV pins"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Do we already have all pins requested?

	if (   (ppPinOutVideo && m_pPinOutVideo == NULL)
		|| (ppPinOutAudio && m_pPinOutAudio == NULL))
	{
		// Locate B2C2 Filter Audio and Video pins:

		hr = EnumerateFilterPins (ppPinOutAudio != NULL, ppPinOutVideo != NULL, FALSE);

		if (FAILED (hr))
		{
			// Last error has already been set at EnumerateFilterPins ()
			return hr;							// *** FUNCTION EXIT POINT
		}
	}

	// Return the pins.

	if (ppPinOutVideo)
	{
		*ppPinOutVideo = m_pPinOutVideo;
	}

	if (ppPinOutAudio)
	{
		*ppPinOutAudio = m_pPinOutAudio;
	}

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::GetMediaControl(IMediaControl **ppMediaControl)
{
	HRESULT		hr;

	// Check if already initialized; got FilterGraph

	if (m_pFilterGraph == NULL)
	{
		SetLastError (TEXT ("Not Initialize, query Media Control"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Check if Media Control was already queried

	if (!m_pMediaControl)
	{
		hr = m_pFilterGraph->QueryInterface (IID_IMediaControl, (void **)&m_pMediaControl);

		if (FAILED (hr))
		{
			SetLastError (TEXT ("Query Interface IID_IMediaControl"), hr);
			return hr;							// *** FUNCTION EXIT POINT
		}
	}

	// Return the Media Control

	if (ppMediaControl)
	{
		*ppMediaControl = m_pMediaControl;
	}

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::GetMediaEvent(IMediaEvent **ppMediaEvent)
{
	HRESULT		hr;

	// Check if already initialized; got FilterGraph

	if (m_pFilterGraph == NULL)
	{
		SetLastError (TEXT ("Not Initialize; query Media Event"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Check if Media Event was already queried

	if (!m_pMediaEvent)
	{
		hr = m_pFilterGraph->QueryInterface(IID_IMediaEvent, (void **)&m_pMediaEvent);

		if (FAILED (hr))
		{
			SetLastError (TEXT ("Query Interface IID_IMediaEvent"), hr);
			return hr;							// *** FUNCTION EXIT POINT
		}
	}

	// Return the Media Event

	if (ppMediaEvent)
	{
		*ppMediaEvent = m_pMediaEvent;
	}

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::CreateTsFilter (int nPin, 
										  REFCLSID refCLSID, 
										  IBaseFilter **ppCustomFilter /* = NULL*/)
{
	HRESULT		hr; 

	// Check if already initialized; got FilterGraph

	if (m_pFilterGraph == NULL)
	{
		SetLastError (TEXT ("Not Initialize, create custom TS PIN filter"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Check if valid PIN number

	if (nPin < 0 || nPin >= B2C2_FILTER_MAX_TS_PINS)
	{
		SetLastError (TEXT ("Invalid PIN number, create custom TS PIN filter"), B2C2_SDK_E_INVALID_PIN);
		return B2C2_SDK_E_INVALID_PIN;			// *** FUNCTION EXIT POINT
	}

	// Check if filter already created

	if (!m_pTsPinFilter[nPin])
	{
		// 1) Create Custom Filter

		hr = CoCreateInstance ((REFCLSID)refCLSID,
							   NULL,
							   CLSCTX_INPROC,
							   (REFIID)IID_IBaseFilter,
							   (void **)&m_pTsPinFilter[nPin]);
	
		if (FAILED (hr))
		{
			SetLastError (TEXT ("Create custom TS PIN filter"), hr);
			return hr;							// *** FUNCTION EXIT POINT
		}

		// 2) Add to Filter Graph.

		hr = m_pFilterGraph->AddFilter (m_pTsPinFilter[nPin], NULL);

		if(FAILED (hr))
		{
			SetLastError (TEXT ("Add custom TS PIN filter to Filter Graph"), hr);
			return hr;							// *** FUNCTION EXIT POINT
		}
	}

	// Return the filter

	if (ppCustomFilter)
	{
		*ppCustomFilter = m_pTsPinFilter[nPin];
	}

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::GetTsInterfaceFilter (int nPin, const IID& iid, IUnknown ** ppInterfaceFilter)
{
	HRESULT		hr;

	// Check if already initialized; got FilterGraph

	if (m_pFilterGraph == NULL)
	{
		SetLastError (TEXT ("Not Initialize"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Check if valid PIN number

	if (nPin < 0 || nPin >= B2C2_FILTER_MAX_TS_PINS)
	{
		SetLastError (TEXT ("Invalid PIN number, query Interface Filter"), B2C2_SDK_E_INVALID_PIN);
		return B2C2_SDK_E_INVALID_PIN;			// *** FUNCTION EXIT POINT
	}

	// Check if custom filter is already created; call CreateTsFilter ()

	if (!m_pTsPinFilter[nPin])
	{
		SetLastError (TEXT ("TS filter not initialize, query Interface Filter"), B2C2_SDK_E_NO_TS_FILTER);
		return B2C2_SDK_E_NO_TS_FILTER;			// *** FUNCTION EXIT POINT
	}

	// Check if interface already queried

	if (!m_pTsPinInterfaceFilter[nPin])
	{
		hr = m_pTsPinFilter[nPin]->QueryInterface (iid, 
												   (VOID **)&m_pTsPinInterfaceFilter[nPin]);

		if(FAILED (hr))
		{
			SetLastError (TEXT ("Query Interface Filter on TS Pin Filter"), hr);
			return hr;							// *** FUNCTION EXIT POINT
		}
	}

	// Return the interface

	if (ppInterfaceFilter)
	{
		*ppInterfaceFilter = m_pTsPinInterfaceFilter[nPin];
	}

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::GetTsOutPin (int nPin, IPin **ppTsOutPin)
{
	HRESULT		hr;

	// Check if already initialized; got Filter

	if (m_pFilter == NULL)
	{
		SetLastError (TEXT ("Not Initialize, enumerate B2C2 Filter Data pins"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;		// *** FUNCTION EXIT POINT
	}

	// Check if valid PIN number

	if (nPin < 0 || nPin >= B2C2_FILTER_MAX_TS_PINS)
	{
		SetLastError (TEXT ("Invalid PIN number, query Interface Filter"), B2C2_SDK_E_INVALID_PIN);
		return B2C2_SDK_E_INVALID_PIN;			// *** FUNCTION EXIT POINT
	}

	// Do we already have this pin?

	if (m_pTsOutPin[nPin] == NULL)
	{
		// Locate B2C2 Filter 'Data n' pins:

		hr = EnumerateFilterPins (FALSE, FALSE, TRUE);

		if(FAILED (hr))
		{
			// Last error has already been set at EnumerateFilterPins ()
			return hr;							// *** FUNCTION EXIT POINT
		}
	}

	// Return the PIN 

	if (ppTsOutPin)
	{
		*ppTsOutPin = m_pTsOutPin[nPin];
	}

	return S_OK;
}

HRESULT B2C2MPEG2Adapter::ConnectTsFilterInToTsOutPin (int nPin, const TCHAR * szInPinName /*= NULL*/)
{
	HRESULT		hr;
	IEnumPins	*pEnum;
	PIN_INFO	pin_Info;
	IPin *		pPinIn = NULL;
	BOOL		bUsePin;

	// Check if already initialized; got FilterGraph

	if (   m_pFilterGraph == NULL
		|| m_pFilter == NULL)
	{
		SetLastError (TEXT ("Not Initialize, enumerate B2C2 Filter Data pins"), B2C2_SDK_E_NOT_INITIALIZED);
		return B2C2_SDK_E_NOT_INITIALIZED;			// *** FUNCTION EXIT POINT
	}

	// Check if valid PIN number

	if (nPin < 0 || nPin >= B2C2_FILTER_MAX_TS_PINS)
	{
		SetLastError (TEXT ("Invalid PIN number, query Interface Filter"), B2C2_SDK_E_INVALID_PIN);
		return B2C2_SDK_E_INVALID_PIN;				// *** FUNCTION EXIT POINT
	}

	// Check if we got a filter for this PIN

	if (m_pTsPinFilter[nPin] == NULL)
	{
		SetLastError (TEXT ("TS filter not initialize, connect B2C2 Filter Data pins"), B2C2_SDK_E_NO_TS_FILTER);
		return B2C2_SDK_E_NO_TS_FILTER;				// *** FUNCTION EXIT POINT
	}

	// Do we already have this Output pin?

	if (m_pTsOutPin[nPin] == NULL)
	{
		// Locate B2C2 Filter 'Data n' pins:

		hr = EnumerateFilterPins (FALSE, FALSE, TRUE);

		if (FAILED (hr))
		{
			// Last error has already been set at EnumerateFilterPins ()
			return hr;								// *** FUNCTION EXIT POINT
		}
	}

	// Double check

	if (m_pTsOutPin[nPin] == NULL)
	{
		SetLastError (TEXT ("Internal Error, connect B2C2 Filter Data pins"), E_FAIL);
		return E_FAIL;
	}

	// Do we already have this Output pin?

	if (m_pTsFilterInPin[nPin] != NULL)
	{
		//There is already a pin connected!
		SetLastError (TEXT ("PIN already connected, connect B2C2 Filter Data pins"), B2C2_SDK_E_PIN_ALREADY_CONNECTED);
		return B2C2_SDK_E_PIN_ALREADY_CONNECTED;	// *** FUNCTION EXIT POINT
	}

	// Locate Dump Filter Generic Input pin:

	// 1) Get list of filter's pins.

	hr = m_pTsPinFilter[nPin]->EnumPins(&pEnum);

	if (FAILED (hr))
	{
		SetLastError (TEXT ("Enumerat TS filter pins"), hr);
		return hr;									// *** FUNCTION EXIT POINT
	}

	// 2) Locate the correct pin.
		
	while (pEnum->Next(1, &pPinIn, NULL) == S_OK)
	{
		bUsePin = TRUE;

		hr = pPinIn->QueryPinInfo(&pin_Info);

		if (pin_Info.pFilter != NULL)
		{
			pin_Info.pFilter->Release();
		}

		if (FAILED (hr))
		{	
			continue;
		}

		if (pin_Info.dir != PINDIR_INPUT)
		{
			// Wrong direction
			bUsePin = FALSE;
		}

		// check name if defined
		if (bUsePin && szInPinName)
		{
			char szPinName[128];	// from SDK, strmif.h; PIN_INFO::achName[ 128 ]
			wcstombs (szPinName, pin_Info.achName, sizeof (szPinName));

			if (strstr (szPinName, szInPinName) == NULL)
			{
				// Not this PIN
				bUsePin = FALSE;
			}
		}

		if (bUsePin)
		{
			// This is our PIN
			m_pTsFilterInPin[nPin] = pPinIn;

			// Don't need to look for more
			break;
		}
		else 
		{
			// We don't need this pin
			pPinIn->Release ();
			pPinIn = NULL;
		}
	}

	pEnum->Release ();

	// Check if pin is found 

	if (m_pTsFilterInPin[nPin] == NULL)
	{
		SetLastError (TEXT ("No Input PIN found, connect B2C2 Filter Data pins"), B2C2_SDK_E_NO_INPUT_PIN);
		return B2C2_SDK_E_NO_INPUT_PIN;
	}

	// Directly connect B2C2 Filter data output pin n to rendered/custom filter input pin.

	hr = m_pFilterGraph->ConnectDirect (m_pTsOutPin[nPin], m_pTsFilterInPin[nPin], NULL);

	if (FAILED (hr))
	{
		SetLastError (TEXT ("Filter Graph ConnectDirect method"), hr);
		return hr;									// *** FUNCTION EXIT POINT
	}

	return S_OK;
}

void B2C2MPEG2Adapter::Release()
{
	// **********************************************************************
	// *** Tear down all filters used, Filter Graph and COM
	// **********************************************************************

	for (int iCnt = 0; iCnt < B2C2_FILTER_MAX_TS_PINS; iCnt++)
	{
		if (m_pTsPinFilter[iCnt])
		{
			m_pTsPinFilter[iCnt]->Release();
			m_pTsPinFilter[iCnt] = NULL;
		}
		if (m_pTsPinInterfaceFilter[iCnt])
		{
			m_pTsPinInterfaceFilter[iCnt]->Release();
			m_pTsPinInterfaceFilter[iCnt] = NULL;
		}
		if (m_pTsOutPin[iCnt])
		{
			m_pTsOutPin[iCnt]->Release();
			m_pTsOutPin[iCnt] = NULL;
		}
		if (m_pTsFilterInPin[iCnt])
		{
			m_pTsFilterInPin[iCnt]->Release();
			m_pTsFilterInPin[iCnt] = NULL;
		}
	}

	if (m_pMediaControl)
	{
		m_pMediaControl->Release();
		m_pMediaControl = NULL;
	}

	if (m_pMediaEvent)
	{
		m_pMediaEvent->Release();
		m_pMediaEvent = NULL;
	}

	if (m_pPinOutAudio)
	{
		m_pPinOutAudio->Release();
		m_pPinOutAudio = NULL;
	}

	if (m_pPinOutVideo)
	{
		m_pPinOutVideo->Release();
		m_pPinOutVideo = NULL;
	}

	if (m_pIB2C2MPEG2AvCtrl)
	{
		m_pIB2C2MPEG2AvCtrl->Release();
		m_pIB2C2MPEG2AvCtrl = NULL;
	}

	if (m_pIB2C2MPEG2TunerCtrl)
	{
		m_pIB2C2MPEG2TunerCtrl->Release();
		m_pIB2C2MPEG2TunerCtrl = NULL;
	}

	if (m_pIB2C2MPEG2DataCtrl)
	{
		m_pIB2C2MPEG2DataCtrl->Release();
		m_pIB2C2MPEG2DataCtrl = NULL;
	}

	if (m_pFilter)
	{
		m_pFilter->Release();
		m_pFilter = NULL;
	}

	if (m_pFilterGraph)
	{
		m_pFilterGraph->Release();
		m_pFilterGraph = NULL;
	}
}

#if defined _B2C2_USE_DEVICE_NOTIFICATION

// This function is only included if _B2C2_USE_DEVICE_NOTIFICATION is defined

BOOL B2C2MPEG2Adapter::RegisterDeviceNotification(HANDLE hRecipient)
{
	HDEVNOTIFY (WINAPI *lpRegisterDeviceNotification) ( 
							IN HANDLE hRecipient,
							IN LPVOID NotificationFilter,
							IN DWORD Flags);

	if (m_hDevNotify != NULL)
	{
		// Notification receiver already registered
		return FALSE;
	}

	// Try to load User32.dll; not availabe on 98 or Me
	if(m_hUser32Dll == NULL)
	{
		// Load the library.
		m_hUser32Dll = LoadLibrary(TEXT ("User32.dll"));
	}

	if(m_hUser32Dll == NULL)
	{
		return FALSE;
	}

	lpRegisterDeviceNotification = (HDEVNOTIFY (WINAPI *)(HANDLE, LPVOID, DWORD))GetProcAddress((HMODULE)m_hUser32Dll, REGISTERDEVICENOTIFY);

	if (lpRegisterDeviceNotification)
	{
		DEV_BROADCAST_DEVICEINTERFACE filterData;
 
		ZeroMemory(&filterData, sizeof(DEV_BROADCAST_DEVICEINTERFACE));
 
		filterData.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
		filterData.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
		filterData.dbcc_classguid  = GUID_NDIS_LAN_CLASS; 

		m_hDevNotify = (*lpRegisterDeviceNotification)(hRecipient, 
													   &filterData, 
													   DEVICE_NOTIFY_WINDOW_HANDLE);

	}

	return (m_hDevNotify != NULL);
}

// This function is only included if _B2C2_USE_DEVICE_NOTIFICATION is defined

BOOL B2C2MPEG2Adapter::UnregisterDeviceNotification()
{
	BOOL (WINAPI *lpUnregisterDeviceNotification) (IN HDEVNOTIFY Handle);
	
	BOOL blRet = FALSE;

	if (m_hDevNotify == NULL)
	{
		// No device notification registers
		SetLastError (TEXT ("No Notification registered"), (DWORD) (-1));
		return blRet;
	}

	// Library already loaded by register
	if (m_hUser32Dll == NULL)
	{
		SetLastError (TEXT ("Library not loaded"), (DWORD) (-1));
		return blRet;
	}

	lpUnregisterDeviceNotification = (BOOL (WINAPI *)(HANDLE))GetProcAddress((HMODULE)m_hUser32Dll, UNREGISTERDEVICENOTIFY);
	
	if(lpUnregisterDeviceNotification)
	{
		blRet = (*lpUnregisterDeviceNotification)(m_hDevNotify);

		if (!blRet)
		{
			SetLastError (TEXT ("Unregister Device Notification"), ::GetLastError());
		}
	}
	
	FreeLibrary((HMODULE)m_hUser32Dll);
	m_hUser32Dll = NULL;

	m_hDevNotify = NULL;

	return blRet;
}

// This function is only included if _B2C2_USE_DEVICE_NOTIFICATION is defined

B2C2MPEG2Adapter::E_B2C2_DEVICE B2C2MPEG2Adapter::GetB2C2DeviceType (PDEV_BROADCAST_HDR pDevBcHdr)
{
	E_B2C2_DEVICE	eDevice = EDEV_NON_B2C2;
	TCHAR			*pDeviceName = NULL;

	// For Windows 2000 and later
	if (pDevBcHdr->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
	{
		PDEV_BROADCAST_DEVICEINTERFACE pBcDevIf = (PDEV_BROADCAST_DEVICEINTERFACE) pDevBcHdr;
		pDeviceName = pBcDevIf->dbcc_name;
	}
	// For Windows 98 and later
	else if(pDevBcHdr->dbch_devicetype == DBT_DEVTYP_DEVNODE)
	{
		PDEV_BROADCAST_DEVNODE pBcDevNode = (PDEV_BROADCAST_DEVNODE) pDevBcHdr;
		CM_Get_Device_ID(pBcDevNode->dbcd_devnode, m_szDeviceId, MAX_DEVICE_ID_LEN, 0);
		pDeviceName = m_szDeviceId;
	}

	if (pDeviceName)
	{
		// No Network Device, so check if B2C2 Device
		size_t 	nLen = _tcslen (pDeviceName);

		DWORD i;

		nLen = min(nLen, B2C2_MAX_DEVICE_NAME_LEN - sizeof(TCHAR));
		for(i = 0; i < nLen; i++)
		{
			m_szTmpStr[i] = tolower(pDeviceName[i]);
		}
		m_szTmpStr[nLen] = 0;//terminate the string

		// Check for USB device
		if (_tcsstr(m_szTmpStr, B2C2_USB_DEVICE_ID))	// "vid_0af7"
		{
			eDevice = EDEV_B2C2_USB;
		} else 
		// Might be PCI device
		if (_tcsstr(m_szTmpStr, B2C2_USB_DEVICE_ID))	// "13d0"
		{
			eDevice = EDEV_B2C2_PCI;
		}
	}
	return eDevice;
}

// This function is only included if _B2C2_USE_DEVICE_NOTIFICATION is defined

B2C2MPEG2Adapter::E_B2C2_DEVICE B2C2MPEG2Adapter::IsDeviceBroadcastEvent(UINT uiEvent, WPARAM wChangeEvent, LPARAM lData)
{
	E_B2C2_DEVICE eDevice = EDEV_NON_B2C2;

	if (wChangeEvent == uiEvent)
	{
		eDevice = GetB2C2DeviceType ((PDEV_BROADCAST_HDR) lData);
	}

	return eDevice;
}

// This function is only included if _B2C2_USE_DEVICE_NOTIFICATION is defined

int B2C2MPEG2Adapter::IsDeviceArrival(WPARAM wChangeEvent, LPARAM lData)
{
	E_B2C2_DEVICE eDevice = IsDeviceBroadcastEvent (DBT_DEVICEARRIVAL, wChangeEvent, lData);

	if (eDevice > EDEV_NON_B2C2)
	{
		TRACE_NOTIFICATION (_T("Got WM_DEVICECHANGE (DBT_DEVICEARRIVAL, %ld)\n"), lData);
	}

	return (int) eDevice;
}

// This function is only included if _B2C2_USE_DEVICE_NOTIFICATION is defined

int B2C2MPEG2Adapter::IsDeviceRemoveComplete(WPARAM wChangeEvent, LPARAM lData)
{
	E_B2C2_DEVICE eDevice = IsDeviceBroadcastEvent (DBT_DEVICEREMOVECOMPLETE, wChangeEvent, lData);

	if (eDevice > EDEV_NON_B2C2)
	{
		TRACE_NOTIFICATION (_T("Got WM_DEVICECHANGE (DBT_DEVICEREMOVECOMPLETE, %ld)\n"), lData);
	}

	return (int) eDevice;
}

#endif //defined _B2C2_USE_DEVICE_NOTIFICATION
