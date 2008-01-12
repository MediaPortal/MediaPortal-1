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

#include "USB2CIIFace.h"

// {3B687F98-41DD-4b40-A7A3-FD6A08799D5B}
DEFINE_GUID(IID_IWinTvUsbCI, 0x3b687f98, 0x41dd, 0x4b40,  0xa7, 0xa3, 0xfd, 0x6a, 0x8, 0x79, 0x9d, 0x5b);

DECLARE_INTERFACE_(IWinTvUsbCI, IUnknown)
{
  STDMETHOD(SetFilter)(THIS_ IBaseFilter* tunerFilter)PURE;
	STDMETHOD(IsModuleInstalled)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(IsCAMInstalled)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(DescrambleService)(THIS_ BYTE* PMT, int PMTLength,BOOL* succeeded)PURE;
};


class CWinTvUsbCI: public CUnknown, public IWinTvUsbCI
{
public:
  CWinTvUsbCI(LPUNKNOWN pUnk, HRESULT *phr);
	~CWinTvUsbCI(void);
  DECLARE_IUNKNOWN
  
  STDMETHODIMP SetFilter(IBaseFilter* tunerFilter);
	STDMETHODIMP IsModuleInstalled( BOOL* yesNo);
	STDMETHODIMP IsCAMInstalled( BOOL* yesNo);
	STDMETHODIMP DescrambleService( BYTE* PMT, int PMTLength,BOOL* succeeded);

private: 
  IUSB2CIBDAConfig* m_pIUSB2CIPLugin;
};
