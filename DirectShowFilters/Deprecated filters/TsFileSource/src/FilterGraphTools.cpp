/**
 *	FilterGraphTools.cpp
 *	Copyright (C) 2003-2004 Nate
 *  Copyright (C) 2004 Bionic Donkey
 *
 *	This file is part of DigitalWatch, a free DTV watching and recording
 *	program for the VisionPlus DVB-T.
 *
 *	DigitalWatch is free software; you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation; either version 2 of the License, or
 *	(at your option) any later version.
 *
 *	DigitalWatch is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with DigitalWatch; if not, write to the Free Software
 *	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#include "StdAfx.h"
#include "FilterGraphTools.h"
#include "GlobalFunctions.h"
#include <stdio.h>
#include <winerror.h>
#include <vector>
#include <ks.h>
#include <ksmedia.h>
#include <bdamedia.h>
#include "TSFileSinkGuids.h"
#include "Winsock.h"
#include "MediaFormats.h"
#include <stdio.h>
#include <string.h>
#include <time.h>
#include <sys/types.h>
#include <sys/timeb.h>

HRESULT FilterGraphTools::AddFilter(IGraphBuilder* piGraphBuilder, REFCLSID rclsid, IBaseFilter **ppiFilter, LPCWSTR pName, BOOL bSilent)
{
	HRESULT hr;
	if FAILED(hr = CoCreateInstance(rclsid, NULL, CLSCTX_INPROC_SERVER, IID_IBaseFilter, reinterpret_cast<void**>(ppiFilter)))
	{
		if (!bSilent)
		{
			if (hr == REGDB_E_CLASSNOTREG)
				(log << "Failed to load filter: " << pName << "  error number " << hr << "  The filter is not registered.\n").Write();
			else
				(log << "Failed to load filter: " << pName << "  error number " << hr << "\n").Write();
		}
		return hr;
	}

	if FAILED(hr = piGraphBuilder->AddFilter(*ppiFilter, pName))
	{
		(*ppiFilter)->Release();
		if (!bSilent)
		{
			(log << "Failed to add filter: " << pName << "\n").Write();
		}
		return hr;
	}
	return S_OK;
}

HRESULT FilterGraphTools::AddFilterByName(IGraphBuilder* piGraphBuilder, IBaseFilter **ppiFilter, CLSID clsidDeviceClass, LPCWSTR friendlyName)
{
	HRESULT hr = S_OK;
	CComPtr <IEnumMoniker> pEnum;
	CComPtr <ICreateDevEnum> pSysDevEnum;
	CComPtr <IMoniker> pMoniker;

	if FAILED(hr = pSysDevEnum.CoCreateInstance(CLSID_SystemDeviceEnum))
	{
        (log << "AddFilterByName: Cannot CoCreate ICreateDevEnum\n").Write();
        return E_FAIL;
    }

	BOOL FilterAdded = FALSE;
	hr = pSysDevEnum->CreateClassEnumerator(clsidDeviceClass, &pEnum, 0);
	switch(hr)
	{
	case S_OK:
		while (pMoniker.Release(), pEnum->Next(1, &pMoniker, 0) == S_OK)
		{
			//Get friendly name
			CComVariant varBSTR;
			//Get filter PropertyBag
			CComPtr <IPropertyBag> pBag;
			if FAILED(hr = pMoniker->BindToStorage(NULL, NULL, IID_IPropertyBag, reinterpret_cast<void**>(&pBag)))
			{
				(log << "AddFilterByName: Cannot BindToStorage\n").Write();
				break;
			}

		    if FAILED(hr = pBag->Read(L"FriendlyName", &varBSTR, NULL))
			{
				(log << "AddFilterByName: Failed to get name of filter\n").Write();
				continue;
			}

			LPOLESTR pStr;
			IBindCtx *pBindCtx;
			hr = CreateBindCtx(0, &pBindCtx);

			hr = pMoniker->GetDisplayName(pBindCtx, NULL, &pStr);

			DWORD hash = 0;
			hr = pMoniker->Hash(&hash);

			//Compare names
			CComVariant tmp = friendlyName;
			if(varBSTR.operator !=(tmp))
			{
				continue;
			}
	
			//Load filter
			if FAILED(hr = pMoniker->BindToObject(NULL, NULL, IID_IBaseFilter, reinterpret_cast<void**>(ppiFilter)))
			{
				(log << "AddFilterByName: Cannot BindToObject\n").Write();
				break;
			}

			//Add filter to graph
			if FAILED(hr = piGraphBuilder->AddFilter(*ppiFilter, varBSTR.bstrVal))
			{
				(log << "AddFilterByName: Failed to add Filter\n").Write();
				(*ppiFilter)->Release();
				return E_FAIL;
			}

			return S_OK;
		}
		break;
	case S_FALSE:
		(log << "AddFilterByName: Failed to create System Device Enumerator\n").Write();
		return E_FAIL;
		break;
	case E_OUTOFMEMORY:
		(log << "AddFilterByName: There is not enough memory available to create a class enumerator.\n").Write();
		return E_FAIL;
		break;
	case E_POINTER:
		(log << "AddFilterByName: Class Enumerator, NULL pointer argument\n").Write();
	  	return E_FAIL;
		break;
	}

	(log << "AddFilterByName: Failed to find matching filter.\n").Write();
	return E_FAIL;
}

HRESULT FilterGraphTools::AddFilterByDevicePath(IGraphBuilder* piGraphBuilder, IBaseFilter **ppiFilter, LPCWSTR pDevicePath, LPCWSTR pName)
{
	HRESULT hr;
	CComPtr <IBindCtx> pBindCtx;
	CComPtr <IMoniker> pMoniker;
	DWORD dwEaten;

	if FAILED(hr = CreateBindCtx(0, &pBindCtx))
	{
		(log << "AddFilterByDevicePath: Could not create bind context\n").Write();
		return hr;
	}

	if (FAILED(hr = MkParseDisplayName(pBindCtx, pDevicePath, &dwEaten, &pMoniker)) || (pMoniker == NULL))
	{
		(log << "AddFilterByDevicePath: Could not create moniker from device path " << pDevicePath << "  (" << pName << ")\n").Write();
		return hr;
	}

	LPWSTR pGraphName = NULL;
	if (pName != NULL)
	{
		strCopy(pGraphName, pName);
	}
	else
	{
		strCopy(pGraphName, pDevicePath);
	}

	if FAILED(hr = pMoniker->BindToObject(0, 0, IID_IBaseFilter, reinterpret_cast<void**>(ppiFilter)))
	{
		(log << "Could Not Create Filter: " << pGraphName << "\n").Write();
		delete[] pGraphName;
		return hr;
	}

	if FAILED(hr = piGraphBuilder->AddFilter(*ppiFilter, pGraphName))
	{
		(*ppiFilter)->Release();
		(log << "Failed to add filter: " << pGraphName << "\n").Write();
		delete[] pGraphName;
		return hr;
	}
	delete[] pGraphName;
	return S_OK;
} 


HRESULT FilterGraphTools::EnumPins(IBaseFilter* piSource)
{
	HRESULT hr;
	CComPtr <IEnumPins> piEnumPins;

	if SUCCEEDED(hr = piSource->EnumPins( &piEnumPins ))
	{
		char* string = (char*)malloc(2048);
		LPOLESTR str = (LPOLESTR)malloc(128);

		CComPtr <IPin> piPin;
		while (piPin.Release(), piEnumPins->Next(1, &piPin, 0) == NOERROR )
		{
			string[0] = '\0';

			PIN_INFO pinInfo;
			piPin->QueryPinInfo(&pinInfo);
			if (pinInfo.pFilter)
				pinInfo.pFilter->Release();	//QueryPinInfo adds a reference to the filter.

			if (pinInfo.dir == PINDIR_INPUT)
				sprintf(string, "Input Pin Name: %S\n", pinInfo.achName);
			else if (pinInfo.dir == PINDIR_OUTPUT)
				sprintf(string, "Output Pin Name: %S\n", pinInfo.achName); 
			else
				sprintf(string, "Pin Name: %S\n", pinInfo.achName); 


			CComPtr <IEnumMediaTypes> piMediaTypes;
			hr = piPin->EnumMediaTypes(&piMediaTypes);
			if (hr == S_OK)
			{
				AM_MEDIA_TYPE *mediaType;
				while (piMediaTypes->Next(1, &mediaType, 0) == NOERROR)
				{
					StringFromGUID2(mediaType->majortype, str, 127);
					sprintf(string, "%s  MajorType: %S\n", string, str);

					StringFromGUID2(mediaType->subtype, str, 127);
					sprintf(string, "%s  SubType: %S\n", string, str);
					if (mediaType->bFixedSizeSamples)
						sprintf(string, "%s  Fixed Sized Samples\n", string);
					else
						sprintf(string, "%s  Not Fixed Sized Samples\n", string);

					if (mediaType->bTemporalCompression)
						sprintf(string, "%s  Temporal Compression\n", string);
					else
						sprintf(string, "%s  Not Temporal Compression\n", string);
					StringFromGUID2(mediaType->formattype, str, 127);
					sprintf(string, "%s  Format Type: %S\n\n", string, str);
				}
			}
			(log << string << "\n").Write();
		}
	}
	return hr;
}

HRESULT FilterGraphTools::FindPin(IBaseFilter* piSource, LPCWSTR Id, IPin **ppiPin, REQUESTED_PIN_DIRECTION eRequestedPinDir)
{
	*ppiPin = NULL;

	if (piSource == NULL)
		return E_FAIL;
	HRESULT hr;
	CComPtr <IEnumPins> piEnumPins;
	
	if SUCCEEDED(hr = piSource->EnumPins( &piEnumPins ))
	{
		CComPtr <IPin> piPins;
		while (piPins.Release(), piEnumPins->Next(1, &piPins, 0) == NOERROR )
		{
			PIN_INFO pinInfo;
			hr = piPins->QueryPinInfo(&pinInfo);
			if (pinInfo.pFilter)
				pinInfo.pFilter->Release();	//QueryPinInfo adds a reference to the filter.
			if (wcscmp(Id, pinInfo.achName) == 0)
			{
				if ((eRequestedPinDir == REQUESTED_PINDIR_ANY) || (eRequestedPinDir == pinInfo.dir))
				{
					*ppiPin = piPins;
					(*ppiPin)->AddRef();
					return S_OK;
				}
			}
		}
	}
	if (hr == S_OK)
		return E_FAIL;
	return hr;
}

HRESULT FilterGraphTools::FindAnyPin(IBaseFilter* piSource, LPCWSTR Id, IPin **ppiPin, REQUESTED_PIN_DIRECTION eRequestedPinDir)
{
	*ppiPin = NULL;

	if (piSource == NULL)
		return E_FAIL;

	HRESULT hr;
	CComPtr <IEnumPins> piEnumPins;
	
	if SUCCEEDED(hr = piSource->EnumPins( &piEnumPins ))
	{
		CComPtr <IPin> piPins;
		while (piPins.Release(), piEnumPins->Next(1, &piPins, 0) == NOERROR )
		{
			PIN_INFO pinInfo;
			hr = piPins->QueryPinInfo(&pinInfo);
			if (pinInfo.pFilter)
				pinInfo.pFilter->Release();	//QueryPinInfo adds a reference to the filter.

			if (!Id || (Id && wcsstr(Id, pinInfo.achName) != NULL))
			{
				if ((eRequestedPinDir == REQUESTED_PINDIR_ANY) || (eRequestedPinDir == pinInfo.dir))
				{
					*ppiPin = piPins;
					(*ppiPin)->AddRef();
					return S_OK;
				}
			}
		}
	}
	if (hr == S_OK)
		return E_FAIL;
	return hr;
}

BOOL FilterGraphTools::IsPinActive(IBaseFilter* piSource, LPCWSTR Id, REQUESTED_PIN_DIRECTION eRequestedPinDir)
{
	if (piSource == NULL)
		return FALSE;

	HRESULT hr;
	CComPtr <IEnumPins> piEnumPins;
	
	if SUCCEEDED(hr = piSource->EnumPins( &piEnumPins ))
	{
		CComPtr <IPin> piPins;
		while (piPins.Release(), piEnumPins->Next(1, &piPins, 0) == NOERROR )
		{
			PIN_INFO pinInfo;
			hr = piPins->QueryPinInfo(&pinInfo);
			if (pinInfo.pFilter)
				pinInfo.pFilter->Release();	//QueryPinInfo adds a reference to the filter.

			if (!Id || (Id && wcsstr(Id, pinInfo.achName) != NULL))
			{
				if ((eRequestedPinDir == REQUESTED_PINDIR_ANY) || (eRequestedPinDir == pinInfo.dir))
				{
					return TRUE;
				}
			}
		}
	}
	if (hr == S_OK)
		return FALSE;
	return FALSE;
}

HRESULT FilterGraphTools::FindPinByMediaType(IBaseFilter* piSource, GUID majortype, GUID subtype, IPin **ppiPin, REQUESTED_PIN_DIRECTION eRequestedPinDir)
{
	*ppiPin = NULL;

	if (piSource == NULL)
		return E_FAIL;
	HRESULT hr;
	CComPtr <IEnumPins> piEnumPins;

	if SUCCEEDED(hr = piSource->EnumPins( &piEnumPins ))
	{
		CComPtr <IPin> piPins;
		while (piPins.Release(), piEnumPins->Next(1, &piPins, 0) == NOERROR )
		{
			PIN_INFO pinInfo;
			hr = piPins->QueryPinInfo(&pinInfo);
			if (pinInfo.pFilter)
			{
				pinInfo.pFilter->Release();	//QueryPinInfo adds a reference to the filter.
			}
			if ((eRequestedPinDir == REQUESTED_PINDIR_ANY) || (eRequestedPinDir == pinInfo.dir))
			{
				CComPtr <IEnumMediaTypes> piMediaTypes;
				if SUCCEEDED(hr = piPins->EnumMediaTypes(&piMediaTypes))
				{
					AM_MEDIA_TYPE *mediaType;
					while (piMediaTypes->Next(1, &mediaType, 0) == NOERROR)
					{
						if ((mediaType->majortype == majortype) &&
							(mediaType->subtype == subtype))
						{
							*ppiPin = piPins;
							(*ppiPin)->AddRef();
							return S_OK;
						}
					}
				}
			}
		}
	}
	if (hr == S_OK)
		return E_FAIL;
	return hr;
}

HRESULT FilterGraphTools::FindFirstFreePin(IBaseFilter* piSource, IPin **ppiPin, PIN_DIRECTION pinDirection)
{
	*ppiPin = NULL;

	if (piSource == NULL)
		return E_FAIL;
	HRESULT hr;
	CComPtr <IEnumPins> piEnumPins;

	if SUCCEEDED(hr = piSource->EnumPins( &piEnumPins ))
	{
		CComPtr <IPin> piPins;
		while (piPins.Release(), piEnumPins->Next(1, &piPins, 0) == NOERROR )
		{
			PIN_INFO pinInfo;
			hr = piPins->QueryPinInfo(&pinInfo);
			if (pinInfo.pFilter)
				pinInfo.pFilter->Release();	//QueryPinInfo adds a reference to the filter.
			if (pinInfo.dir == pinDirection)
			{
				//Check if pin is already connected
				CComPtr <IPin> pOtherPin;
				hr = piPins->ConnectedTo(&pOtherPin);
				if (FAILED(hr) && (hr != VFW_E_NOT_CONNECTED))
				{
					(log << "Failed to Determin if Pin is already connected\n").Write();
					return E_FAIL;
				}
				if (pOtherPin == NULL)
				{
					*ppiPin = piPins;
					(*ppiPin)->AddRef();
					return S_OK;
				}
			}
		}
	}
	if (hr == S_OK)
		return E_FAIL;
	return hr;
}

HRESULT FilterGraphTools::FindFilter(IGraphBuilder* piGraphBuilder, LPCWSTR Id, IBaseFilter **ppiFilter)
{
	*ppiFilter = NULL;
	if (piGraphBuilder == NULL)
		return E_FAIL;
	HRESULT hr;
	CComPtr <IEnumFilters> piEnumFilters;
	
	if SUCCEEDED(hr = piGraphBuilder->EnumFilters(&piEnumFilters))
	{
		CComPtr <IBaseFilter> piFilter;
		while (piFilter.Release(), piEnumFilters->Next(1, &piFilter, 0) == NOERROR )
		{
			FILTER_INFO filterInfo;
			hr = piFilter->QueryFilterInfo(&filterInfo);
			if (filterInfo.pGraph)
				filterInfo.pGraph->Release();

			if (wcscmp(Id, filterInfo.achName) == 0)
			{
				*ppiFilter = piFilter;
				(*ppiFilter)->AddRef();
				return S_OK;
			}
		}
	}
	if (hr == S_OK)
		return E_FAIL;
	return hr;
}

HRESULT FilterGraphTools::FindFilterByCLSID(IGraphBuilder* piGraphBuilder, CLSID rclsid, IBaseFilter **ppiFilter)
{
	*ppiFilter = NULL;
	if (piGraphBuilder == NULL)
		return E_FAIL;
	HRESULT hr;

	CComPtr <IEnumFilters> piEnumFilters;
	
	if SUCCEEDED(hr = piGraphBuilder->EnumFilters(&piEnumFilters))
	{
		CComPtr <IBaseFilter> piFilter;
		while (piFilter.Release(), piEnumFilters->Next(1, &piFilter, 0) == NOERROR )
		{
			CLSID clsid = GUID_NULL;
			piFilter->GetClassID(&clsid);

			if (IsEqualCLSID(rclsid, clsid))
			{
				*ppiFilter = piFilter;
				(*ppiFilter)->AddRef();
				return S_OK;
			}
		}
	}
	if (hr == S_OK)
		return E_FAIL;
	return hr;
}

HRESULT FilterGraphTools::ConnectFilters(IGraphBuilder* piGraphBuilder, IBaseFilter* piFilterUpstream, LPCWSTR sourcePinName, IBaseFilter* piFilterDownstream, LPCWSTR destPinName)
{
	if (piFilterUpstream == NULL)
	{
		(log << "ConnectPins: piFilterUpstream pointer is null\n").Write();
		return E_FAIL;
	}
	if (piFilterDownstream == NULL)
	{
		(log << "ConnectPins: piFilterDownstream pointer is null\n").Write();
		return E_FAIL;
	}
	HRESULT hr;
	CComPtr <IPin> piOutput;
	CComPtr <IPin> piInput;

	if (S_OK != (hr = FindPin(piFilterUpstream, sourcePinName, &piOutput, REQUESTED_PINDIR_OUTPUT)))
	{
		(log << "ConnectPins: Failed to find output pin named " << sourcePinName << "\n").Write();
		return E_FAIL;
	}

	if (S_OK != (hr = FindPin(piFilterDownstream, destPinName, &piInput, REQUESTED_PINDIR_INPUT)))
	{
		(log << "ConnectPins: Failed to find input pin named " << destPinName << "\n").Write();
		return E_FAIL;
	}

	hr = ConnectFilters(piGraphBuilder, piOutput, piInput);

	return hr;
}

HRESULT FilterGraphTools::ConnectFilters(IGraphBuilder* piGraphBuilder, IBaseFilter* piFilterUpstream, IBaseFilter* piFilterDownstream)
{
    HRESULT hr = S_OK;

    CComPtr <IEnumPins> piEnumPinsUpstream;
    if FAILED(hr = piFilterUpstream->EnumPins(&piEnumPinsUpstream))
	{
        return (log << "Cannot Enumerate Pins on Upstream Filter\n").Write(E_FAIL);
    }

    CComPtr <IPin> piPinUpstream;
    while (piPinUpstream.Release(), piEnumPinsUpstream->Next(1, &piPinUpstream, 0) == S_OK )
	{
	    PIN_INFO PinInfoUpstream;
        if FAILED(hr = piPinUpstream->QueryPinInfo (&PinInfoUpstream))
		{
            return (log << "Cannot Obtain Pin Info for Upstream Filter\n").Write(E_FAIL);
        }
		//QueryPinInfo increases the reference count on pFilter, so release it.
		if (PinInfoUpstream.pFilter != NULL)
			PinInfoUpstream.pFilter->Release();

		//Check if pin is an Output pin
		if (PinInfoUpstream.dir != PINDIR_OUTPUT)
		{
			continue;
		}

		//Check if pin is already connected
        CComPtr <IPin> pPinDown;
		hr = piPinUpstream->ConnectedTo(&pPinDown);
		if (FAILED(hr) && (hr != VFW_E_NOT_CONNECTED))
		{
			return (log << "Failed to Determin if Upstream Ouput Pin is already connected\n").Write(E_FAIL);
		}
		if (pPinDown != NULL)
		{
			continue;
		}


		CComPtr <IEnumPins> pIEnumPinsDownstream;
		if FAILED(hr = piFilterDownstream->EnumPins(&pIEnumPinsDownstream))
		{
			(log << "Cannot enumerate pins on downstream filter!\n").Write();
			return E_FAIL;
		}

		CComPtr <IPin> piPinDownstream;
		while (piPinDownstream.Release(), pIEnumPinsDownstream->Next (1, &piPinDownstream, 0) == S_OK )
		{
		    PIN_INFO PinInfoDownstream;
			if FAILED(hr = piPinDownstream->QueryPinInfo(&PinInfoDownstream))
			{
				return (log << "Cannot Obtain Pin Info for Downstream Filter\n").Write(E_FAIL);
			}
			//QueryPinInfo increases the reference count on pFilter, so release it.
			if (PinInfoDownstream.pFilter != NULL)
				PinInfoDownstream.pFilter->Release();

			//Check if pin is an Input pin
			if (PinInfoDownstream.dir != PINDIR_INPUT)
			{
				continue;
			}

			//Check if pin is already connected
			CComPtr <IPin>  pPinUp;
			hr = piPinDownstream->ConnectedTo(&pPinUp);
			if (FAILED(hr) && (hr != VFW_E_NOT_CONNECTED))
			{
				return (log << "Failed to Find if Downstream Input Pin is already connected\n").Write(E_FAIL);
			}
			if (pPinUp != NULL)
			{
				continue;
			}

			//Connect pins
			if SUCCEEDED(hr = ConnectFilters(piGraphBuilder, piPinUpstream, piPinDownstream))
			{
				return S_OK;
			}
		}
    }
	if (hr == S_OK)
		return E_FAIL;
    return hr;
}

HRESULT FilterGraphTools::ConnectFilters(IGraphBuilder* piGraphBuilder, IBaseFilter* piFilterUpstream, IPin* piPinDownstream)
{
    HRESULT hr = S_OK;

    CComPtr <IEnumPins> piEnumPinsUpstream;
    if FAILED(hr = piFilterUpstream->EnumPins(&piEnumPinsUpstream))
	{
        return (log << "Cannot Enumerate Pins on Upstream Filter\n").Write(E_FAIL);
    }

    CComPtr <IPin> piPinUpstream;
    while (piPinUpstream.Release(), piEnumPinsUpstream->Next(1, &piPinUpstream, 0) == S_OK)
	{
	    PIN_INFO PinInfoUpstream;
        if FAILED(hr = piPinUpstream->QueryPinInfo (&PinInfoUpstream))
		{
            return (log << "Cannot Obtain Pin Info for Upstream Filter\n").Write(E_FAIL);
        }
		//QueryPinInfo increases the reference count on pFilter, so release it.
		if (PinInfoUpstream.pFilter != NULL)
			PinInfoUpstream.pFilter->Release();

		//Check if pin is an Output pin
		if (PinInfoUpstream.dir != PINDIR_OUTPUT)
		{
			continue;
		}

		//Check if pin is already connected
        CComPtr <IPin> pPinDown;
		hr = piPinUpstream->ConnectedTo(&pPinDown);
		if (FAILED(hr) && (hr != VFW_E_NOT_CONNECTED))
		{
			return (log << "Failed to Determin if Upstream Ouput Pin is already connected\n").Write(E_FAIL);
		}
		if (pPinDown != NULL)
		{
			continue;
		}

		if SUCCEEDED(hr = ConnectFilters(piGraphBuilder, piPinUpstream, piPinDownstream))
		{
			return S_OK;
		}

		piPinUpstream.Release();
    }
	if (hr == S_OK)
		return E_FAIL;
    return hr;
}

HRESULT FilterGraphTools::ConnectFilters(IGraphBuilder* piGraphBuilder, IPin* piPinUpstream, IBaseFilter* piFilterDownstream)
{
    HRESULT hr = S_OK;

	CComPtr <IEnumPins> pIEnumPinsDownstream;
	if FAILED(hr = piFilterDownstream->EnumPins(&pIEnumPinsDownstream))
	{
		(log << "Cannot enumerate pins on downstream filter!\n").Write();
		return E_FAIL;
	}

	CComPtr <IPin> piPinDownstream;
	while (piPinDownstream.Release(), pIEnumPinsDownstream->Next (1, &piPinDownstream, 0) == S_OK)
	{
	    PIN_INFO PinInfoDownstream;
		if FAILED(hr = piPinDownstream->QueryPinInfo(&PinInfoDownstream))
		{
			return (log << "Cannot Obtain Pin Info for Downstream Filter\n").Write(E_FAIL);
		}
		//QueryPinInfo increases the reference count on pFilter, so release it.
		if (PinInfoDownstream.pFilter != NULL)
			PinInfoDownstream.pFilter->Release();

		//Check if pin is an Input pin
		if (PinInfoDownstream.dir != PINDIR_INPUT)
		{
			continue;
		}

		//Check if pin is already connected
		CComPtr <IPin>  pPinUp;
		hr = piPinDownstream->ConnectedTo(&pPinUp);
		if (FAILED(hr) && (hr != VFW_E_NOT_CONNECTED))
		{
			return (log << "Failed to Find if Downstream Input Pin is already connected\n").Write(E_FAIL);
		}
		if (pPinUp != NULL)
		{
			continue;
		}

		//Connect pins
		if SUCCEEDED(hr = ConnectFilters(piGraphBuilder, piPinUpstream, piPinDownstream))
		{
			return S_OK;
		}
		piPinDownstream.Release();
	}

	if (hr == S_OK)
		return E_FAIL;
    return hr;
}

HRESULT FilterGraphTools::ConnectFilters(IGraphBuilder* piGraphBuilder, IPin* piPinUpstream, IPin* piPinDownstream)
{
	HRESULT hr = piGraphBuilder->ConnectDirect(piPinUpstream, piPinDownstream, NULL);

	if SUCCEEDED(hr)
	{
		::OutputDebugString("Pin connection - SUCCEEDED\n");
	}
	else
	{
		::OutputDebugString("Pin connection - FAILED !!!\n");
	}

	return hr;
}

HRESULT FilterGraphTools::RenderPin(IGraphBuilder* piGraphBuilder, IBaseFilter* piSource, LPCWSTR pinName)
{
	HRESULT hr = S_OK;

	CComPtr <IPin> piPin;
	if (S_OK != (hr = FindPin(piSource, pinName, &piPin, REQUESTED_PINDIR_OUTPUT)))
		return hr;

	return piGraphBuilder->Render(piPin);
}


HRESULT FilterGraphTools::DisconnectAllPins(IGraphBuilder* piGraphBuilder)
{
	if (piGraphBuilder == NULL)
		return E_FAIL;
	HRESULT hr;
	CComPtr <IEnumFilters> piEnumFilters;
	hr = piGraphBuilder->EnumFilters(&piEnumFilters);
	if SUCCEEDED(hr)
	{
		CComPtr <IBaseFilter> piFilter;
		while (piFilter.Release(), piEnumFilters->Next(1, &piFilter, 0) == NOERROR )
		{
			CComPtr <IEnumPins> piEnumPins;
			hr = piFilter->EnumPins( &piEnumPins );
			if SUCCEEDED(hr)
			{
				CComPtr <IPin> piPin;
				while (piPin.Release(), piEnumPins->Next(1, &piPin, 0) == NOERROR )
				{
					hr = piPin->Disconnect();
					if (hr == VFW_E_NOT_STOPPED)
						(log << "Could not disconnect pin. The filter is active\n").Write();
				}
			}
		}
	}
	return hr;
}

HRESULT FilterGraphTools::RemoveAllFilters(IGraphBuilder* piGraphBuilder)
{
	if (piGraphBuilder == NULL)
		return E_FAIL;
	HRESULT hr;

	CComPtr <IEnumFilters> piEnumFilters;
	hr = piGraphBuilder->EnumFilters(&piEnumFilters);
	if ((hr != S_OK) || (piEnumFilters == NULL))
	{
		(log << "Error removing filters. Can't enumerate graph\n").Write();
		return hr;
	}

	IBaseFilter *piFilter = NULL;
	std::vector<IBaseFilter *> filterArray;

	piEnumFilters->Reset();
	while ((hr == S_OK) && ( piEnumFilters->Next(1, &piFilter, 0) == S_OK ) && (piFilter != NULL))
	{
		filterArray.push_back(piFilter);
	}

	std::vector<IBaseFilter *>::iterator it = filterArray.begin();
	for ( ; it < filterArray.end() ; it++ )
	{
		piFilter = *it;
		piGraphBuilder->RemoveFilter(piFilter);
		piFilter->Release();
	}

	return hr;
}

HRESULT FilterGraphTools::GetOverlayMixer(IGraphBuilder* piGraphBuilder, IBaseFilter **ppiFilter)
{
	HRESULT hr;

	*ppiFilter = NULL;
	hr = FindFilterByCLSID(piGraphBuilder, CLSID_OverlayMixer, ppiFilter);
	if (hr != S_OK)
	{
		//Overlay Mixer 2
		CLSID clsid = GUID_NULL;
		CLSIDFromString(L"{A0025E90-E45B-11D1-ABE9-00A0C905F375}", &clsid);
		hr = FindFilterByCLSID(piGraphBuilder, clsid, ppiFilter);
	}

	return hr;
}

HRESULT FilterGraphTools::GetOverlayMixerInputPin(IGraphBuilder* piGraphBuilder, LPCWSTR pinName, IPin **ppiPin)
{
	HRESULT hr;
	CComPtr <IBaseFilter> pfOverlayMixer;

	hr = GetOverlayMixer(piGraphBuilder, &pfOverlayMixer);
	if (hr == S_OK)
	{
		hr = FindPin(pfOverlayMixer, pinName, ppiPin, REQUESTED_PINDIR_INPUT);
		if (hr != S_OK)
			(log << "Error: Could not find Input pin on Overlay Mixer\n").Write();
	}
	return hr;
}

HRESULT FilterGraphTools::AddToRot(IUnknown *pUnkGraph, DWORD *pdwRegister) 
{
    CComPtr <IMoniker> pMoniker;
    CComPtr <IRunningObjectTable> pROT;
    if FAILED(GetRunningObjectTable(0, &pROT))
	{
        return E_FAIL;
    }
	WCHAR *wsz = (WCHAR *)malloc(256);
	swprintf(wsz, L"FilterGraph %08x pid %08x", (DWORD)pUnkGraph, GetCurrentProcessId());

    HRESULT hr = CreateItemMoniker(L"!", wsz, &pMoniker);
    if SUCCEEDED(hr)
	{
        hr = pROT->Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, pUnkGraph, pMoniker, pdwRegister);
    }
	free(wsz);
    return hr;
}

void FilterGraphTools::RemoveFromRot(DWORD pdwRegister)
{
    CComPtr <IRunningObjectTable> pROT;
    if SUCCEEDED(GetRunningObjectTable(0, &pROT))
	{
        pROT->Revoke(pdwRegister);
    }
	pdwRegister = 0;
}

HRESULT FilterGraphTools::SetReferenceClock(IBaseFilter *pFilter)
{
	HRESULT hr = S_OK;

	FILTER_INFO info;
	if FAILED(hr = pFilter->QueryFilterInfo(&info))
		return (log << "Failed to Query Filter Info from IBaseFilter filter: " << hr << "\n").Write(hr);

	CComQIPtr<IGraphBuilder> piGraphBuilder(info.pGraph);
	if (!piGraphBuilder)
		return (log << "Failed to get IGraphBuilder interface from IBaseFilter filter: " << hr << "\n").Write(hr);

	info.pGraph->Release();

	//Set reference clock
	CComQIPtr<IReferenceClock> piRefClock(pFilter);
	if (!piRefClock)
		return (log << "Failed to get reference clock interface on IBaseFilter filter: " << hr << "\n").Write(hr);

	CComQIPtr<IMediaFilter> piMediaFilter(piGraphBuilder);
	if (!piMediaFilter)
		return (log << "Failed to get IMediaFilter interface from graph: " << hr << "\n").Write(hr);

	if FAILED(hr = piMediaFilter->SetSyncSource(piRefClock))
		return (log << "Failed to set reference clock: " << hr << "\n").Write(hr);

	return hr;
}

/////////////////////////////////////////////////////////////////////////////
//BDA functions
/////////////////////////////////////////////////////////////////////////////

HRESULT FilterGraphTools::InitDVBTTuningSpace(CComPtr <ITuningSpace> &piTuningSpace)
{
	HRESULT hr = S_OK;

	if FAILED(hr = piTuningSpace.CoCreateInstance(CLSID_DVBTuningSpace))
	{
		(log << "Could not create DVBTuningSpace\n").Write();
		return hr;
	}

	CComQIPtr <IDVBTuningSpace2> piDVBTuningSpace(piTuningSpace);
	if (!piDVBTuningSpace)
	{
		piTuningSpace.Release();
		(log << "Could not QI TuningSpace\n").Write();
		return E_FAIL;
	}
	if FAILED(hr = piDVBTuningSpace->put_SystemType(DVB_Terrestrial))
	{
		piDVBTuningSpace.Release();
		piTuningSpace.Release();
		(log << "Could not put SystemType\n").Write();
		return hr;
	}
	CComBSTR bstrNetworkType = "{216C62DF-6D7F-4E9A-8571-05F14EDB766A}";
	if FAILED(hr = piDVBTuningSpace->put_NetworkType(bstrNetworkType))
	{
		piDVBTuningSpace.Release();
		piTuningSpace.Release();
		(log << "Could not put NetworkType\n").Write();
		return hr;
	}
	piDVBTuningSpace.Release();

	return S_OK;
}

HRESULT FilterGraphTools::CreateDVBTTuneRequest(CComPtr <ITuningSpace> piTuningSpace, CComPtr <ITuneRequest> &pExTuneRequest, long frequency, long bandwidth)
{
	HRESULT hr = S_OK;

	if (!piTuningSpace)
	{
		(log << "Tuning Space is NULL\n").Write();
		return E_FAIL;
	}

	//Get an interface to the TuningSpace
	CComQIPtr <IDVBTuningSpace2> piDVBTuningSpace(piTuningSpace);
    if (!piDVBTuningSpace)
	{
        (log << "Can't Query Interface for an IDVBTuningSpace2\n").Write();
		return E_FAIL;
	}

	//Get new TuneRequest from tuning space
	if FAILED(hr = piDVBTuningSpace->CreateTuneRequest(&pExTuneRequest))
	{
		(log << "Failed to create Tune Request\n").Write();
		return hr;
	}

	//Get interface to the TuneRequest
	CComQIPtr <IDVBTuneRequest> piDVBTuneRequest(pExTuneRequest);
	if (!piDVBTuneRequest)
	{
		pExTuneRequest.Release();
        (log << "Can't Query Interface for an IDVBTuneRequest.\n").Write();
		return E_FAIL;
	}

	//
	// Start
	//
	CComPtr <IDVBTLocator> pDVBTLocator;
	hr = pDVBTLocator.CoCreateInstance(CLSID_DVBTLocator);
	switch (hr)
	{ 
	case REGDB_E_CLASSNOTREG:
		pExTuneRequest.Release();
		piDVBTuneRequest.Release();
		(log << "The DVBTLocator class isn't registered in the registration database.\n").Write();
		return hr;

	case CLASS_E_NOAGGREGATION:
		pExTuneRequest.Release();
		piDVBTuneRequest.Release();
		(log << "The DVBTLocator class can't be created as part of an aggregate.\n").Write();
		return hr;
	}

	if FAILED(hr = pDVBTLocator->put_CarrierFrequency(frequency))
	{
		pDVBTLocator.Release();
		pExTuneRequest.Release();
		piDVBTuneRequest.Release();
		(log << "Can't set Frequency on Locator.\n").Write();
		return hr;
	}
	if FAILED(hr = pDVBTLocator->put_Bandwidth(bandwidth))
	{
		pDVBTLocator.Release();
		pExTuneRequest.Release();
		piDVBTuneRequest.Release();
		(log << "Can't set Bandwidth on Locator.\n").Write();
		return hr;
	}
	//
	//End
	//

	// Bind the locator to the tune request.
	
    if FAILED(hr = piDVBTuneRequest->put_Locator(pDVBTLocator))
	{
		pDVBTLocator.Release();
		pExTuneRequest.Release();
		piDVBTuneRequest.Release();
        (log << "Cannot put the locator on DVB Tune Request\n").Write();
		return hr;
    }

	pDVBTLocator.Release();
	piDVBTuneRequest.Release();

	return hr;
}

HRESULT FilterGraphTools::ClearDemuxPids(CComPtr<IBaseFilter>& pFilter)
{
	HRESULT hr = E_INVALIDARG;

	if(pFilter == NULL)
		return hr;

	CComPtr<IPin> pOPin;
	PIN_DIRECTION  direction;
	// Enumerate the Demux pins
	CComPtr<IEnumPins> pIEnumPins;
	if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins)))
	{
		ULONG pinfetch(0);
		while(pIEnumPins->Next(1, &pOPin, &pinfetch) == S_OK)
		{
			pOPin->QueryDirection(&direction);
			if(direction == PINDIR_OUTPUT)
			{
				WCHAR *pinName;
				pOPin->QueryId(&pinName);
				if FAILED(hr = ClearDemuxPins(pOPin))
					(log << "Failed to Clear demux Pin" << pinName << " pin : " << hr << "\n").Write();
			}
			pOPin.Release();
			pOPin = NULL;
		}
	}
	return S_OK;
}

HRESULT FilterGraphTools::ClearDemuxPins(IPin *pIPin)
{
	HRESULT hr = E_INVALIDARG;

	if(pIPin == NULL)
		return hr;

	IMPEG2PIDMap* muxMapPid;
	if(SUCCEEDED(pIPin->QueryInterface (&muxMapPid))){

		IEnumPIDMap *pIEnumPIDMap;
		if (SUCCEEDED(muxMapPid->EnumPIDMap(&pIEnumPIDMap))){
			ULONG pNumb = 0;
			PID_MAP pPidMap;
			while(pIEnumPIDMap->Next(1, &pPidMap, &pNumb) == S_OK){
				ULONG pid = pPidMap.ulPID;
				hr = muxMapPid->UnmapPID(1, &pid);
			}
		}
		muxMapPid->Release();
	}
	else {

		IMPEG2StreamIdMap* muxStreamMap;
		if(SUCCEEDED(pIPin->QueryInterface (&muxStreamMap))){

			IEnumStreamIdMap *pIEnumStreamMap;
			if (SUCCEEDED(muxStreamMap->EnumStreamIdMap(&pIEnumStreamMap))){
				ULONG pNumb = 0;
				STREAM_ID_MAP pStreamIdMap;
				while(pIEnumStreamMap->Next(1, &pStreamIdMap, &pNumb) == S_OK){
					ULONG pid = pStreamIdMap.stream_id;
					hr = muxStreamMap->UnmapStreamId(1, &pid);
				}
			}
			muxStreamMap->Release();
		}
	}
	return S_OK;
}

HRESULT FilterGraphTools::VetDemuxPin(IPin* pIPin, ULONG pid)
{
	HRESULT hr = E_INVALIDARG;

	if(pIPin == NULL)
		return hr;

	ULONG pids = 0;
	IMPEG2PIDMap* muxMapPid;
	if(SUCCEEDED(pIPin->QueryInterface (&muxMapPid))){

		IEnumPIDMap *pIEnumPIDMap;
		if (SUCCEEDED(muxMapPid->EnumPIDMap(&pIEnumPIDMap))){
			ULONG pNumb = 0;
			PID_MAP pPidMap;
			while(pIEnumPIDMap->Next(1, &pPidMap, &pNumb) == S_OK){
				if (pid != pPidMap.ulPID)
				{
					pids = pPidMap.ulPID;
					hr = muxMapPid->UnmapPID(1, &pids);
				}
			}
		}
		muxMapPid->Release();
	}
	else {

		IMPEG2StreamIdMap* muxStreamMap;
		if(SUCCEEDED(pIPin->QueryInterface (&muxStreamMap))){

			IEnumStreamIdMap *pIEnumStreamMap;
			if (SUCCEEDED(muxStreamMap->EnumStreamIdMap(&pIEnumStreamMap))){
				ULONG pNumb = 0;
				STREAM_ID_MAP pStreamIdMap;
				while(pIEnumStreamMap->Next(1, &pStreamIdMap, &pNumb) == S_OK){
					if (pid != pStreamIdMap.stream_id)
					{
						pids = pStreamIdMap.stream_id;
						hr = muxStreamMap->UnmapStreamId(1, &pids);
					}
				}
			}
			muxStreamMap->Release();
		}
	}
	return S_OK;
}

HRESULT FilterGraphTools::AddDemuxPins(DVBTChannels_Service* pService, CComPtr<IBaseFilter>& pFilter, int intPinType, BOOL bForceConnect)
{
	if (pService == NULL)
	{
		(log << "Skipping Demux Pins. No service passed.\n").Write();
		return E_INVALIDARG;
	}

	if (pFilter == NULL)
	{
		(log << "Skipping Demux Pins. No Demultiplexer passed.\n").Write();
		return E_INVALIDARG;
	}

	(log << "Adding Sink Demux Pins\n").Write();
	LogMessageIndent indent(&log);

	HRESULT hr;

	if (m_piMpeg2Demux)
		m_piMpeg2Demux.Release();

	if FAILED(hr = pFilter->QueryInterface(&m_piMpeg2Demux))
	{
		(log << "Failed to get the IMeg2Demultiplexer Interface on the Sink Demux.\n").Write();
		return E_FAIL;
	}

	long videoStreamsRendered = 0;
	long audioStreamsRendered = 0;
	long teletextStreamsRendered = 0;
	long subtitleStreamsRendered = 0;
	long tsStreamsRendered = 0;

	if (intPinType)
	{
		// render TS
		hr = AddDemuxPinsTS(pService, &tsStreamsRendered);
	}
	else
	{
		// render video
		hr = AddDemuxPinsVideo(pService, &videoStreamsRendered);
		if(FAILED(hr) && bForceConnect)
			return hr;

		// render h264 video if no mpeg2 video was rendered
		if (videoStreamsRendered == 0)
		{
			hr = AddDemuxPinsH264(pService, &videoStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render mpeg4 video if no mpeg2 or h264 video was rendered
		if (videoStreamsRendered == 0)
		{
			hr = AddDemuxPinsMpeg4(pService, &videoStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render teletext if video was rendered
		if (videoStreamsRendered > 0)
		{
			hr = AddDemuxPinsTeletext(pService, &teletextStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render Subtitles if video was rendered
		if (videoStreamsRendered > 0)
		{
			hr = AddDemuxPinsSubtitle(pService, &subtitleStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render mp1 audio
		hr = AddDemuxPinsMp1(pService, &audioStreamsRendered);
		if(FAILED(hr) && bForceConnect)
			return hr;

		// render mp2 audio if no mp1 was rendered
		if (audioStreamsRendered == 0)
		{
			hr = AddDemuxPinsMp2(pService, &audioStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render ac3 audio if no mp1/2 was rendered
		if (audioStreamsRendered == 0)
		{
			hr = AddDemuxPinsAC3(pService, &audioStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render aac audio if no ac3 or mp1/2 was rendered
		if (audioStreamsRendered == 0)
		{
			hr = AddDemuxPinsAAC(pService, &audioStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}

		// render dts audio if no ac3 or mp1/2 or aac was rendered
		if (audioStreamsRendered == 0)
		{
			hr = AddDemuxPinsDTS(pService, &audioStreamsRendered);
			if(FAILED(hr) && bForceConnect)
				return hr;
		}
	}

	if (m_piMpeg2Demux)
		m_piMpeg2Demux.Release();

	indent.Release();
	(log << "Finished Adding Demux Pins\n").Write();

	return S_OK;
}

HRESULT FilterGraphTools::AddDemuxPins(DVBTChannels_Service* pService, DVBTChannels_Service_PID_Types streamType, LPWSTR pPinName, AM_MEDIA_TYPE *pMediaType, long *streamsRendered)
{
	if (pService == NULL)
		return E_INVALIDARG;

	HRESULT hr = S_OK;

	long renderedStreams = 0;

//	if (!wcsicmp(pPinName, L"TS"))
	if (!_wcsicmp(pPinName, L"TS"))
	{
		long count = pService->GetStreamCount();

		wchar_t text[32];
		swprintf((wchar_t*)&text, pPinName);

		CComPtr <IPin> piPin;

		// Get the Pin
		CComPtr<IBaseFilter>pFilter;
		if SUCCEEDED(hr = m_piMpeg2Demux->QueryInterface(&pFilter))
		{
			if FAILED(hr = pFilter->FindPin(pPinName, &piPin))
			{
				// Create the Pin
				if (S_OK != (hr = m_piMpeg2Demux->CreateOutputPin(pMediaType, (wchar_t*)&text, &piPin)))
				{
					(log << "Failed to create demux " << pPinName << " pin : " << hr << "\n").Write();
					return hr;
				}
			}
		}
		else
		{
			// Create the Pin
			if (S_OK != (hr = m_piMpeg2Demux->CreateOutputPin(pMediaType, (wchar_t*)&text, &piPin)))
			{
				(log << "Failed to create demux " << pPinName << " pin : " << hr << "\n").Write();
				return hr;
			}
		}

		// Map the PID.
		CComPtr <IMPEG2PIDMap> piPidMap;
		if FAILED(hr = piPin.QueryInterface(&piPidMap))
		{
			(log << "Failed to query demux " << pPinName << " pin : " << hr << "\n").Write();
			return hr;	//it's safe to not piPin.Release() because it'll go out of scope
		}

		for ( long currentStream=0 ; currentStream<count ; currentStream++ )
		{
			ULONG Pid = pService->GetStreamPID(currentStream);
			if FAILED(hr = piPidMap->MapPID(1, &Pid, MEDIA_TRANSPORT_PACKET))
			{
				(log << "Failed to map demux " << pPinName << " pin : " << hr << "\n").Write();
				continue;	//it's safe to not piPidMap.Release() because it'll go out of scope
			}
			renderedStreams++;
		}

		ULONG Pidarray[6] = {0x00, 0x10, 0x11, 0x12, 0x13, 0x14};
		if FAILED(hr = piPidMap->MapPID(6, &Pidarray[0], MEDIA_TRANSPORT_PACKET))
		{
			(log << "Failed to map demux " << pPinName << " pin Fixed Pids : " << hr << "\n").Write();
		}

		renderedStreams++;
	}
	else
	{
		long count = pService->GetStreamCount(streamType);
		BOOL bMultipleStreams = (pService->GetStreamCount(streamType) > 1) ? 1 : 0;

		for ( long currentStream=0 ; currentStream<count ; currentStream++ )
		{
			ULONG Pid = pService->GetStreamPID(streamType, currentStream);

			wchar_t text[32];
			swprintf((wchar_t*)&text, pPinName);
			if (bMultipleStreams && currentStream > 0)
				swprintf((wchar_t*)&text, L"%s %i", pPinName, currentStream+1);

			CComPtr <IPin> piPin;

			// Get the Pin
			CComPtr<IBaseFilter>pFilter;
			if SUCCEEDED(hr = m_piMpeg2Demux->QueryInterface(&pFilter))
			{
				if FAILED(hr = pFilter->FindPin(pPinName, &piPin))
				{
					// Create the Pin
					(log << "Creating pin: PID=" << (long)Pid << "   Name=\"" << (LPWSTR)&text << "\"\n").Write();
					LogMessageIndent indent(&log);

					if (S_OK != (hr = m_piMpeg2Demux->CreateOutputPin(pMediaType, (wchar_t*)&text, &piPin)))
					{
						(log << "Failed to create demux " << pPinName << " pin : " << hr << "\n").Write();
						return hr;
					}
					indent.Release();
				}
			}
			else
			{
				(log << "Creating pin: PID=" << (long)Pid << "   Name=\"" << (LPWSTR)&text << "\"\n").Write();
				LogMessageIndent indent(&log);

				// Create the Pin
				if (S_OK != (hr = m_piMpeg2Demux->CreateOutputPin(pMediaType, (wchar_t*)&text, &piPin)))
				{
					(log << "Failed to create demux " << pPinName << " pin : " << hr << "\n").Write();
					return hr;
				}
				indent.Release();
			}

			// Map the PID.
			CComPtr <IMPEG2PIDMap> piPidMap;
			if FAILED(hr = piPin.QueryInterface(&piPidMap))
			{
				(log << "Failed to query demux " << pPinName << " pin : " << hr << "\n").Write();
				continue;	//it's safe to not piPin.Release() because it'll go out of scope
			}

			if FAILED(hr = VetDemuxPin(piPin, Pid))
			{
				(log << "Failed to unmap demux " << pPinName << " pin : " << hr << "\n").Write();
				continue;	//it's safe to not piPidMap.Release() because it'll go out of scope
			}

			if(Pid)
			{
				if(pMediaType->majortype == KSDATAFORMAT_TYPE_MPEG2_SECTIONS)
				{
					if FAILED(hr = piPidMap->MapPID(1, &Pid, MEDIA_TRANSPORT_PACKET))
					{
						(log << "Failed to map demux " << pPinName << " pin : " << hr << "\n").Write();
						continue;	//it's safe to not piPidMap.Release() because it'll go out of scope
					}
				}
				else if FAILED(hr = piPidMap->MapPID(1, &Pid, MEDIA_ELEMENTARY_STREAM))
				{
					(log << "Failed to map demux " << pPinName << " pin : " << hr << "\n").Write();
					continue;	//it's safe to not piPidMap.Release() because it'll go out of scope
				}
			}

			if (renderedStreams != 0)
				continue;

			renderedStreams++;
		}
	}

	if (streamsRendered)
		*streamsRendered = renderedStreams;

	return hr;
}

HRESULT FilterGraphTools::AddDemuxPinsVideo(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetVideoMedia(&mediaType);
	return AddDemuxPins(pService, video, L"Video", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsH264(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetH264Media(&mediaType);
	return AddDemuxPins(pService, h264, L"Video", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsMpeg4(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetMpeg4Media(&mediaType);
	return AddDemuxPins(pService, mpeg4, L"Video", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsMp1(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetMP1Media(&mediaType);
	return AddDemuxPins(pService, mp1, L"Audio", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsMp2(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetMP2Media(&mediaType);
	return AddDemuxPins(pService, mp2, L"Audio", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsAC3(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetAC3Media(&mediaType);
//	return AddDemuxPins(pService, ac3, L"AC3", &mediaType, streamsRendered);
	return AddDemuxPins(pService, ac3, L"Audio", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsAAC(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetAACMedia(&mediaType);
	return AddDemuxPins(pService, aac, L"Audio", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsDTS(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetDTSMedia(&mediaType);
	return AddDemuxPins(pService, dts, L"Audio", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsTeletext(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	ZeroMemory(&mediaType, sizeof(AM_MEDIA_TYPE));
	GetTelexMedia(&mediaType);
	return AddDemuxPins(pService, teletext, L"Teletext", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsSubtitle(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	ZeroMemory(&mediaType, sizeof(AM_MEDIA_TYPE));
	GetSubtitleMedia(&mediaType);
	return AddDemuxPins(pService, subtitle, L"Subtitle", &mediaType, streamsRendered);
}

HRESULT FilterGraphTools::AddDemuxPinsTS(DVBTChannels_Service* pService, long *streamsRendered)
{
	AM_MEDIA_TYPE mediaType;
	GetTSMedia(&mediaType);
	return AddDemuxPins(pService, unknown, L"TS", &mediaType, streamsRendered);
}


HRESULT FilterGraphTools::GetAC3Media(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_DOLBY_AC3;
	pintype->cbFormat = sizeof(MPEG1AudioFormat);//sizeof(AC3AudioFormat); //
	pintype->pbFormat = MPEG1AudioFormat;//AC3AudioFormat; //
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->formattype = FORMAT_WaveFormatEx;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT FilterGraphTools::GetMP2Media(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_MPEG2_AUDIO; 
	pintype->formattype = FORMAT_WaveFormatEx; 
	pintype->cbFormat = sizeof(MPEG2AudioFormat);
	pintype->pbFormat = MPEG2AudioFormat; 
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT FilterGraphTools::GetMP1Media(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_MPEG1Payload;
	pintype->formattype = FORMAT_WaveFormatEx; 
	pintype->cbFormat = sizeof(MPEG1AudioFormat);
	pintype->pbFormat = MPEG1AudioFormat;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT FilterGraphTools::GetAACMedia(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_AAC;
	pintype->formattype = FORMAT_WaveFormatEx; 
	pintype->cbFormat = sizeof(AACAudioFormat);
	pintype->pbFormat = AACAudioFormat;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT FilterGraphTools::GetDTSMedia(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_DTS;
	pintype->formattype = FORMAT_WaveFormatEx; 
	pintype->cbFormat = sizeof(DTSAudioFormat);
	pintype->pbFormat = DTSAudioFormat;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT FilterGraphTools::GetVideoMedia(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = KSDATAFORMAT_TYPE_VIDEO;
	pintype->subtype = MEDIASUBTYPE_MPEG2_VIDEO;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = FALSE;
	pintype->lSampleSize = 1;
	pintype->formattype = FORMAT_MPEG2Video;
	pintype->pUnk = NULL;
//	pintype->cbFormat = sizeof(Mpeg2ProgramVideo);
//	pintype->pbFormat = Mpeg2ProgramVideo;
	pintype->cbFormat = sizeof(g_Mpeg2ProgramVideo);
	pintype->pbFormat = g_Mpeg2ProgramVideo;


	return S_OK;
}

HRESULT FilterGraphTools::GetH264Media(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;
	if(pintype == NULL)
		return hr;
	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Video;
	pintype->subtype = H264_SubType;
	pintype->bFixedSizeSamples = FALSE;
	pintype->bTemporalCompression = TRUE;
	pintype->lSampleSize = 1;
	//pintype->formattype = FORMAT_VideoInfo;
	pintype->formattype = FORMAT_MPEG2Video;
	pintype->pUnk = NULL;
	//pintype->cbFormat = sizeof(H264VideoFormat); //This should be determined by the stream for CoreAVC. Other codecs are fine.
	//pintype->pbFormat = H264VideoFormat;
	pintype->cbFormat = sizeof(g_Mpeg2ProgramVideo);
	pintype->pbFormat = g_Mpeg2ProgramVideo;
	return S_OK;
}

HRESULT FilterGraphTools::GetMpeg4Media(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Video;
	pintype->subtype = FOURCCMap(MAKEFOURCC('A','V','C','1'));
	pintype->bFixedSizeSamples = FALSE;
	pintype->bTemporalCompression = TRUE;
	pintype->lSampleSize = 1;
	pintype->formattype = FORMAT_MPEG2Video;
	pintype->pUnk = NULL;
	pintype->cbFormat = sizeof(g_Mpeg2ProgramVideo);
	pintype->pbFormat = g_Mpeg2ProgramVideo;

	return S_OK;
}

HRESULT FilterGraphTools::GetTIFMedia(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = KSDATAFORMAT_TYPE_MPEG2_SECTIONS;
	pintype->subtype = MEDIASUBTYPE_DVB_SI; 
	pintype->formattype = KSDATAFORMAT_SPECIFIER_NONE;

	return S_OK;
}

HRESULT FilterGraphTools::GetTelexMedia(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = KSDATAFORMAT_TYPE_MPEG2_SECTIONS;
	pintype->subtype = KSDATAFORMAT_SUBTYPE_NONE; 
	pintype->formattype = KSDATAFORMAT_SPECIFIER_NONE; 

	return S_OK;
}

HRESULT FilterGraphTools::GetSubtitleMedia(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = KSDATAFORMAT_TYPE_VIDEO;
	pintype->subtype = MEDIASUBTYPE_DVD_SUBPICTURE;
	pintype->formattype = FORMAT_None;

	return S_OK;
}

HRESULT FilterGraphTools::GetTSMedia(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL)
		return hr;

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Stream;
	pintype->subtype = KSDATAFORMAT_SUBTYPE_BDA_MPEG2_TRANSPORT; 
	pintype->formattype = FORMAT_None; 

	return S_OK;
}

HRESULT FilterGraphTools::DisconnectOutputPins(IBaseFilter *pFilter)
{
	CComPtr<IPin> pOPin;
	PIN_DIRECTION  direction;
	// Enumerate the Demux pins
	CComPtr<IEnumPins> pIEnumPins;
	if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins)))
	{

		ULONG pinfetch(0);
		while(pIEnumPins->Next(1, &pOPin, &pinfetch) == S_OK)
		{

			pOPin->QueryDirection(&direction);
			if(direction == PINDIR_OUTPUT)
			{
				// Get an instance of the Demux control interface
				CComPtr<IMpeg2Demultiplexer> muxInterface;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
					LPWSTR pinName = L"";
					pOPin->QueryId(&pinName);
					muxInterface->DeleteOutputPin(pinName);
					muxInterface.Release();
				}
				else
				{
					IPin *pIPin = NULL;
					pOPin->ConnectedTo(&pIPin);
					if (pIPin)
					{
						pOPin->Disconnect();
						pIPin->Disconnect();
						pIPin->Release();
					}
				}

			}
			pOPin.Release();
			pOPin = NULL;
		}
	}
	return S_OK;
}

HRESULT FilterGraphTools::DisconnectInputPins(IBaseFilter *pFilter)
{
	CComPtr<IPin> pIPin;
	PIN_DIRECTION  direction;
	// Enumerate the Demux pins
	CComPtr<IEnumPins> pIEnumPins;
	if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins)))
	{

		ULONG pinfetch(0);
		while(pIEnumPins->Next(1, &pIPin, &pinfetch) == S_OK)
		{

			pIPin->QueryDirection(&direction);
			if(direction == PINDIR_INPUT)
			{
				IPin *pOPin = NULL;
				pIPin->ConnectedTo(&pOPin);
				if (pOPin)
				{
					pOPin->Disconnect();
					pIPin->Disconnect();
					pOPin->Release();
				}
			}
			pIPin.Release();
			pIPin = NULL;
		}
	}
	return S_OK;
}

HRESULT FilterGraphTools::DeleteOutputPins(IBaseFilter *pFilter)
{
    HRESULT hr = S_OK;
	
    PIN_DIRECTION  direction;
	CComPtr <IPin> pIPin;
	AM_MEDIA_TYPE *type;
	
	// Get an instance of the Demux control interface
	CComPtr <IMpeg2Demultiplexer> muxInterface;
	hr = pFilter->QueryInterface (&muxInterface);
	
	// Enumerate the Demux pins
    CComPtr <IEnumPins> pIEnumPins;
    hr = pFilter->EnumPins (&pIEnumPins);
	
    if (FAILED (hr))
    {
		(log << "Cannot get enumpins on Demux filter: " << hr << "\n").Write(hr);
        return hr;
    }

    while(pIEnumPins->Next(1, &pIPin, 0) == S_OK)
    {
        hr = pIPin->QueryDirection(&direction);
		
        if(direction == PINDIR_OUTPUT)
        {
			CComPtr <IPin> pDownstreamPin;
            pIPin->ConnectedTo(&pDownstreamPin);
			
            if(pDownstreamPin == NULL)
            {
				PIN_INFO pinInfo;
				if (FAILED(pIPin->QueryPinInfo(&pinInfo)))
				{
					(log << "Cannot Get Demux Output Pin Info on Demux filter: " << hr << "\n").Write(hr);
					return hr;
				}

				if(pinInfo.pFilter)
					pinInfo.pFilter->Release();

//				if FAILED(hr = ClearDemuxPins(pIPin))
//					(log << "Failed to Clear demux Pin" << pinInfo.achName << " pin : " << hr << "\n").Write();

				CComPtr <IEnumMediaTypes> ppEnum;
				if (SUCCEEDED (pIPin->EnumMediaTypes(&ppEnum)))
				{
					while(ppEnum->Next(1, &type, 0) == S_OK)
					{
						muxInterface->DeleteOutputPin(pinInfo.achName);
					}
				}
				ppEnum.Release();
            }
        }
        pIPin.Release();
    }
	
	return hr;
}

HRESULT FilterGraphTools::GetGraphBuilder(IPin *pIPin, CComPtr<IGraphBuilder>& piGraphBuilder)
{
	if (!pIPin)
		return E_INVALIDARG;

	HRESULT hr = E_FAIL;

	PIN_INFO PinInfo;
	if (SUCCEEDED(pIPin->QueryPinInfo(&PinInfo)))
	{
		FILTER_INFO FilterInfo;
		if (SUCCEEDED(PinInfo.pFilter->QueryFilterInfo(&FilterInfo)))
		{
			if (SUCCEEDED(FilterInfo.pGraph->QueryInterface(&piGraphBuilder)))
			{
				if (FilterInfo.pGraph)
					FilterInfo.pGraph->Release();

				if (PinInfo.pFilter)
					PinInfo.pFilter->Release();

				return S_OK;
			}
			if (FilterInfo.pGraph)
				FilterInfo.pGraph->Release();
		}
		if (PinInfo.pFilter)
			PinInfo.pFilter->Release();
	}
	return hr;
}
