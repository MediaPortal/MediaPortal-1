/**
 *	FilterGraphTools.h
 *	Copyright (C) 2003-2004 Nate
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

#ifndef FILTERGRAPHTOOLS_H
#define FILTERGRAPHTOOLS_H

//#pragma warning (disable : 4312)
//#pragma warning (disable : 4995)

//#include <dshow.h>
#include <bdatif.h>
#include "LogMessage.h"
#include "DVBTChannels.h"

typedef enum _RequestedPinDirection
{
	REQUESTED_PINDIR_INPUT    = PINDIR_INPUT,
	REQUESTED_PINDIR_OUTPUT   = PINDIR_OUTPUT,
	REQUESTED_PINDIR_ANY      = PINDIR_OUTPUT + 1
} REQUESTED_PIN_DIRECTION;

class FilterGraphTools : public LogMessageCaller
{
public:
	HRESULT AddFilter(IGraphBuilder* piGraphBuilder, REFCLSID rclsid, IBaseFilter **ppiFilter, LPCWSTR pName, BOOL bSilent = FALSE);
	HRESULT AddFilterByName(IGraphBuilder* piGraphBuilder, IBaseFilter **ppiFilter, CLSID clsidDeviceClass, LPCWSTR friendlyName);
	HRESULT AddFilterByDevicePath(IGraphBuilder* piGraphBuilder, IBaseFilter **piFilter, LPCWSTR pDevicePath, LPCWSTR pName);

	HRESULT EnumPins(IBaseFilter* piSource);

	HRESULT FindPin(IBaseFilter* piSource, LPCWSTR Id, IPin **ppiPin, REQUESTED_PIN_DIRECTION eRequestedPinDir = REQUESTED_PINDIR_ANY);
	HRESULT FindAnyPin(IBaseFilter* piSource, LPCWSTR Id, IPin **ppiPin, REQUESTED_PIN_DIRECTION eRequestedPinDir = REQUESTED_PINDIR_ANY);
	BOOL	IsPinActive(IBaseFilter* piSource, LPCWSTR Id, REQUESTED_PIN_DIRECTION eRequestedPinDir = REQUESTED_PINDIR_ANY);
	HRESULT FindPinByMediaType(IBaseFilter* piSource, GUID majortype, GUID subtype, IPin **ppiPin, REQUESTED_PIN_DIRECTION eRequestedPinDir = REQUESTED_PINDIR_ANY);
	HRESULT FindFirstFreePin(IBaseFilter* piSource, IPin **ppiPin, PIN_DIRECTION pinDirection);

	HRESULT FindFilter(IGraphBuilder* piGraphBuilder, LPCWSTR Id, IBaseFilter **ppiFilter);
	HRESULT FindFilterByCLSID(IGraphBuilder* piGraphBuilder, CLSID rclsid, IBaseFilter **ppiFilter);

	HRESULT ConnectFilters(IGraphBuilder* piGraphBuilder, IBaseFilter* piFilterUpstream, LPCWSTR sourcePinName, IBaseFilter* piFilterDownstream, LPCWSTR destPinName);
	HRESULT ConnectFilters(IGraphBuilder* piGraphBuilder, IBaseFilter* piFilterUpstream, IBaseFilter* piFilterDownstream);
	HRESULT ConnectFilters(IGraphBuilder* piGraphBuilder, IBaseFilter* piFilterUpstream, IPin* piPinDownstream);
	HRESULT ConnectFilters(IGraphBuilder* piGraphBuilder, IPin* piPinUpstream, IBaseFilter* piFilterDownstream);
	HRESULT ConnectFilters(IGraphBuilder* piGraphBuilder, IPin* piPinUpstream, IPin* piPinDownstream);
	HRESULT RenderPin     (IGraphBuilder* piGraphBuilder, IBaseFilter* piSource, LPCWSTR pinName);

	HRESULT DisconnectAllPins(IGraphBuilder* piGraphBuilder);
	HRESULT RemoveAllFilters(IGraphBuilder* piGraphBuilder);

	HRESULT GetOverlayMixer(IGraphBuilder* piGraphBuilder, IBaseFilter **ppiFilter);
	HRESULT GetOverlayMixerInputPin(IGraphBuilder* piGraphBuilder, LPCWSTR pinName, IPin **ppiPin);

	HRESULT AddToRot(IUnknown *pUnkGraph, DWORD *pdwRegister);
	void RemoveFromRot(DWORD pdwRegister);

	HRESULT SetReferenceClock(IBaseFilter *pFilter);

	//BDA functions
	HRESULT InitDVBTTuningSpace(CComPtr <ITuningSpace> &piTuningSpace);
	HRESULT CreateDVBTTuneRequest(CComPtr <ITuningSpace> piTuningSpace, CComPtr <ITuneRequest> &pExTuneRequest, long frequency, long bandwidth);
	HRESULT ClearDemuxPids(CComPtr<IBaseFilter>& pFilter);
	HRESULT ClearDemuxPins(IPin *pIPin);
	HRESULT VetDemuxPin(IPin* pIPin, ULONG pid);
	HRESULT AddDemuxPins(DVBTChannels_Service* pService, CComPtr<IBaseFilter>& pFilter, int intPinType = 0, BOOL bForceConnect = FALSE);
	HRESULT AddDemuxPins(DVBTChannels_Service* pService, DVBTChannels_Service_PID_Types streamType, LPWSTR pPinName, AM_MEDIA_TYPE *pMediaType, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsVideo(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsH264(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsMpeg4(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsMp1(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsMp2(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsAC3(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsAAC(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsDTS(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsTeletext(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsSubtitle(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT AddDemuxPinsTS(DVBTChannels_Service* pService, long *streamsRendered = NULL);
	HRESULT GetAC3Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetMP2Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetMP1Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetAACMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetDTSMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetVideoMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetH264Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetMpeg4Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetTIFMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetTelexMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetSubtitleMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetTSMedia(AM_MEDIA_TYPE *pintype);
	HRESULT DisconnectOutputPins(IBaseFilter *pFilter);
	HRESULT DisconnectInputPins(IBaseFilter *pFilter);
	HRESULT DeleteOutputPins(IBaseFilter *pFilter);
	HRESULT GetGraphBuilder(IPin *pIPin, CComPtr<IGraphBuilder>& piGraphBuilder);

private:
		CComPtr <IMpeg2Demultiplexer> m_piMpeg2Demux;

};

#endif
