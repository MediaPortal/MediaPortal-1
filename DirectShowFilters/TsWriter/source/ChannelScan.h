/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#include "patparser.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "nitdecoder.h"

using namespace Mediaportal;

#pragma once



// {1663DC42-D169-41da-BCE2-EEEC482CB9FB}
DEFINE_GUID(IID_ITSChannelScan, 0x1663dc42, 0xd169, 0x41da, 0xbc, 0xe2, 0xee, 0xec, 0x48, 0x2c, 0xb9, 0xfb);

DECLARE_INTERFACE_(ITSChannelScan, IUnknown)
{
	STDMETHOD(Start)(THIS_ bool waitForVCT)PURE;
	STDMETHOD(Stop)(THIS_)PURE;
	STDMETHOD(GetCount)(THIS_ int* channelCount)PURE;
	STDMETHOD(IsReady)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetChannel)(THIS_ int index,
										 long* networkId,
										 long* transportId,
										 long* serviceId,
										 int* majorChannel,
										 int* minorChannel,
										 int* frequency,
										 int* lcn,
										 int* freeCAMode,
										 int* serviceType,
										 int* modulation,
										 char** providerName,
										 char** serviceName,
										 int* pmtPid,
										 int* hasVideo,
										 int* hasAudio)PURE;
	STDMETHOD(SetCallBack)(THIS_ IChannelScanCallback* callback)PURE;

	STDMETHOD(ScanNIT)(THIS_)PURE;
	STDMETHOD(StopNIT)(THIS_)PURE;
	STDMETHOD(GetNITCount)(THIS_ int* transponderCount)PURE;
	STDMETHOD(GetNITChannel)(THIS_ int channel,int* type,int* frequency,int *polarisation, int* modulation, int* symbolrate, int* bandwidth, int* fecInner, int* rollOff, char** networkName)PURE;

};

class CMpTsFilter;

class CChannelScan: public CUnknown, public ITSChannelScan
{
public:
	CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter);
	~CChannelScan(void);
	
  DECLARE_IUNKNOWN
	
	STDMETHODIMP Start(bool waitForVCT);
	STDMETHODIMP Stop();
	STDMETHODIMP GetCount(int* channelCount);
	STDMETHODIMP IsReady( BOOL* yesNo);
	STDMETHODIMP GetChannel(int index,
										 long* networkId,
										 long* transportId,
										 long* serviceId,
										 int* majorChannel,
										 int* minorChannel,
										 int* frequency,
										 int* lcn,
										 int* freeCAMode,
										 int* serviceType,
										 int* modulation,
										 char** providerName,
										 char** serviceName,
										 int* pmtPid,
										 int* hasVideo,
										 int* hasAudio);
	STDMETHODIMP SetCallBack(IChannelScanCallback* callback);

	STDMETHODIMP ScanNIT();
	STDMETHODIMP StopNIT();
	STDMETHODIMP GetNITCount(int* transponderCount);
	STDMETHODIMP GetNITChannel(int channel,int* type, int* frequency,int *polarisation, int* modulation, int* symbolrate, int* bandwidth, int* fecInner, int* rollOff, char** networkName);

	void OnTsPacket(byte* tsPacket);
private:
	CPatParser m_patParser;
	bool m_bIsParsing;
	bool m_bIsParsingNIT;
	CMpTsFilter* m_pFilter;
	CCriticalSection m_section;
	IChannelScanCallback* m_pCallback;
  CNITDecoder m_nit;
};
