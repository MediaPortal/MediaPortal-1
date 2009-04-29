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

using namespace Mediaportal;

// {9A9E7592-A178-4a63-A210-910FD7FFEC8C}
DEFINE_GUID(IID_ITeletextGrabber, 0x9a9e7592, 0xa178, 0x4a63, 0xa2, 0x10, 0x91, 0xf, 0xd7, 0xff, 0xec, 0x8c);

DECLARE_INTERFACE_(ITeletextCallBack, IUnknown)
{
	STDMETHOD(OnTeletextReceived)(THIS_ BYTE* data, int packetCounts)PURE;
};


DECLARE_INTERFACE_(ITeletextGrabber, IUnknown)
{
	STDMETHOD(Start)(THIS_)PURE;
	STDMETHOD(Stop)(THIS_ )PURE;
	STDMETHOD(SetTeletextPid)(THIS_ int teletextPid)PURE;
	STDMETHOD(SetCallBack)(THIS_ ITeletextCallBack* callback)PURE;
};

class CTeletextGrabber: public CUnknown,  public ITeletextGrabber
{
public:
	CTeletextGrabber(LPUNKNOWN pUnk, HRESULT *phr);
	~CTeletextGrabber(void);

  DECLARE_IUNKNOWN
	STDMETHODIMP Start( );
	STDMETHODIMP Stop( );
	STDMETHODIMP SetTeletextPid( int teletextPid);
	STDMETHODIMP SetCallBack( ITeletextCallBack* callback);

	void OnTsPacket(byte* tsPacket);
private:
	ITeletextCallBack* m_pCallback;
	int			m_iTeletextPid;
	bool		m_bRunning;
	byte*   m_pBuffer;
	int		  m_iPacketCounter;
	CCriticalSection m_section;
};
