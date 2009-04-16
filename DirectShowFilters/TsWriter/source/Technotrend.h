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
#include "ttBdaDrvApi.h"

#pragma once

// {B0AB5587-DCEC-49f4-B1AA-06EF58DBF1D3}
DEFINE_GUID(IID_ITechnoTrend, 0xb0ab5587, 0xdcec, 0x49f4, 0xb1, 0xaa, 0x6, 0xef, 0x58, 0xdb, 0xf1, 0xd3);

DECLARE_INTERFACE_(ITechnoTrend, IUnknown)
{
  STDMETHOD(SetTunerFilter)(THIS_ IBaseFilter* tunerFilter)PURE;
	STDMETHOD(IsTechnoTrend)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(IsCamPresent)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(IsCamReady)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(SetAntennaPower)(THIS_ BOOL onOff)PURE;
	STDMETHOD(SetDisEqc)(THIS_ BYTE* diseqc, BYTE len, BYTE Repeat,BYTE Toneburst,int ePolarity)PURE;
	STDMETHOD(DescrambleService)(THIS_ BYTE* PMT, int PMTLength,BOOL* succeeded)PURE;
	STDMETHOD(DescrambleMultiple)(THIS_ WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded)PURE;
};

class CTechnotrend: public CUnknown, public ITechnoTrend
{
public:
  CTechnotrend(LPUNKNOWN pUnk, HRESULT *phr);
	~CTechnotrend(void);
  DECLARE_IUNKNOWN
  
  STDMETHODIMP SetTunerFilter(IBaseFilter* tunerFilter);
	STDMETHODIMP IsTechnoTrend( BOOL* yesNo);
	STDMETHODIMP GetCISlotStatus();
  STDMETHODIMP IsCamPresent( BOOL* yesNo);
	STDMETHODIMP IsCamReady( BOOL* yesNo);
	STDMETHODIMP SetAntennaPower( BOOL onOff);
	STDMETHODIMP SetDisEqc( BYTE* diseqc, BYTE len, BYTE Repeat,BYTE Toneburst,int ePolarity);
	STDMETHODIMP DescrambleService( BYTE* PMT, int PMTLength,BOOL* succeeded);
	STDMETHODIMP DescrambleMultiple(WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded);

	// Info functions
	STDMETHODIMP GetDevNameAndFEType();
	STDMETHODIMP GetProductSellerID(); 
	STDMETHODIMP GetDrvVersion(UINT deviceId);

	// CI Menu functions
	STDMETHODIMP EnterModuleMenu();
	STDMETHODIMP SendMenuAnswer  (BYTE Selection);
	void OnDisplayMenu(BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength);

  void OnCaChange(BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus);
  void OnSlotChange(BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo);

private:
  bool        GetDeviceID(IBaseFilter* tunerFilter, UINT& deviceId);
  void		  FreeTechnotrendLibrary();
  HANDLE      m_hBdaApi;
  int         m_slotStatus;
  DEVICE_CAT  m_deviceType;
  int         m_ciStatus;
  HMODULE     m_dll;
  int         m_waitTimeout;
	int					m_ciSlotAvailable;
  bool        m_verboseLogging;
  TS_CiCbFcnPointer m_technoTrendStructure;
};
