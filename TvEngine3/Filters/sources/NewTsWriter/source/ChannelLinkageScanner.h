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
#pragma once
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "TsHeader.h"
#include "ChannelLinkageParser.h"

using namespace Mediaportal;

// {26DF395C-3D13-4f3e-9EC1-453FAAFFB13E}
DEFINE_GUID(IID_ITsChannelLinkageScanner, 0x26df395c, 0x3d13, 0x4f3e, 0x9e, 0xc1, 0x45, 0x3f, 0xaa, 0xff, 0xb1, 0x3e);

// {F8A86679-C80A-42fd-A148-20D681A67024}
DEFINE_GUID(IID_IChannelLinkageCallback, 0xf8a86679, 0xc80a, 0x42fd, 0xa1, 0x48, 0x20, 0xd6, 0x81, 0xa6, 0x70, 0x24);

DECLARE_INTERFACE_(IChannelLinkageCallback, IUnknown)
{
	STDMETHOD(OnLinkageReceived)()PURE;
};


// video anayzer interface
DECLARE_INTERFACE_(ITsChannelLinkageScanner, IUnknown)
{
	STDMETHOD(Start)(THIS_) PURE;
	STDMETHOD(Reset)(THIS_)PURE;

	STDMETHOD(GetChannelCount) (THIS_ ULONG* channelCount)PURE;
	STDMETHOD(GetChannel) (THIS_ ULONG channelIndex, WORD* network_id, WORD* transport_id,WORD* service_id  )PURE;
	STDMETHOD(GetLinkedChannelsCount) (THIS_ ULONG channelIndex, ULONG* linkedChannelsCount)PURE;
	STDMETHOD(GetLinkedChannel) (THIS_ ULONG channelIndex,ULONG linkIndex, WORD* network_id, WORD* transport_id,WORD* service_id, char** channelName  )PURE;
  
	STDMETHOD(SetCallBack)(THIS_ IChannelLinkageCallback* callback)PURE;
};

class CChannelLinkageScanner: public CUnknown, public ITsChannelLinkageScanner
{
public:
	CChannelLinkageScanner(LPUNKNOWN pUnk, HRESULT *phr);
	~CChannelLinkageScanner(void);

  DECLARE_IUNKNOWN

	STDMETHODIMP Start();
	STDMETHODIMP Reset();

	STDMETHODIMP GetChannelCount (THIS_ ULONG* channelCount);
	STDMETHODIMP GetChannel (THIS_ ULONG channelIndex, WORD* network_id, WORD* transport_id,WORD* service_id  );
	STDMETHODIMP GetLinkedChannelsCount (THIS_ ULONG channelIndex, ULONG* linkedChannelsCount);
	STDMETHODIMP GetLinkedChannel (THIS_ ULONG channel,ULONG linkIndex, WORD* network_id, WORD* transport_id,WORD* service_id, char** channelName  );

	STDMETHODIMP SetCallBack(IChannelLinkageCallback* callback);

	void OnTsPacket(byte* tsPacket);
protected:
	IChannelLinkageCallback* m_pCallBack;
	CChannelLinkageParser m_ChannelLinkageParser;
private:
	bool m_bScanning;
	CCriticalSection m_section;
	CTsHeader m_header;
};
