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
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <streams.h>
#include "TeletextGrabber.h"

#pragma once

// {9C9B9E27-A9EA-4ac9-B2FB-FC9FCACECA82}
DEFINE_GUID(IID_IAnalogChannelScanCallback, 0x9c9b9e27, 0xa9ea, 0x4ac9, 0xb2, 0xfb, 0xfc, 0x9f, 0xca, 0xce, 0xca, 0x82);

DECLARE_INTERFACE_(IAnalogChannelScanCallback, IUnknown)
{
	STDMETHOD(OnScannerDone)()PURE;
};


// {D44ABA24-57B2-44de-8D56-7B95CBF8527A}
DEFINE_GUID(IID_IAnalogChanelScan, 0xd44aba24, 0x57b2, 0x44de, 0x8d, 0x56, 0x7b, 0x95, 0xcb, 0xf8, 0x52, 0x7a);

DECLARE_INTERFACE_(IAnalogChanelScan, IUnknown)
{
	STDMETHOD(Start)(THIS_)PURE;
	STDMETHOD(Stop)(THIS_)PURE;
	STDMETHOD(IsReady)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetChannel)(THIS_ char** serviceName)PURE;
	STDMETHOD(SetCallBack)(THIS_ IAnalogChannelScanCallback* callback)PURE;
};


class CChannelScan: public CUnknown, public IAnalogChanelScan
{
public:
	CChannelScan(LPUNKNOWN pUnk, HRESULT *phr);
	~CChannelScan(void);
	
	DECLARE_IUNKNOWN
	
	STDMETHODIMP Start();
	STDMETHODIMP Stop();
	STDMETHODIMP IsReady( BOOL* yesNo);
	STDMETHODIMP GetChannel(char** serviceName);
	STDMETHODIMP SetCallBack(IAnalogChannelScanCallback* callback);

	void OnTeletextData(byte* sampleData, int sampleLen);
private:
	byte*						m_pBuffer;
	byte*						m_pBufferTemp;
	int							m_iBufferPos;
	bool						m_bIsScanning;
	BOOL						m_bChannelFound;
	BOOL						m_bScanningPossible;
	char						m_sServiceName[128];
	IAnalogChannelScanCallback*	m_pCallback;
};
