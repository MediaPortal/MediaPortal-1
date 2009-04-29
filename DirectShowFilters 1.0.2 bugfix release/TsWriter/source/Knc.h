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
#include "knc/KNCBDACI.h"

// {C71E2EFA-2439-4dbe-A1F7-935ADC37A4EC}
DEFINE_GUID(IID_IKNC, 0xc71e2efa, 0x2439, 0x4dbe, 0xa1, 0xf7, 0x93, 0x5a, 0xdc, 0x37, 0xa4, 0xec);

DECLARE_INTERFACE_(IKNC, IUnknown)
{
  STDMETHOD(SetTunerFilter)(THIS_ IBaseFilter* tunerFilter, int iDeviceIndex)PURE;
  STDMETHOD(IsKNC)(THIS_ BOOL* yesNo)PURE;
  STDMETHOD(IsCamReady)(THIS_ BOOL* yesNo)PURE;
  STDMETHOD(IsCIAvailable)(THIS_ BOOL* yesNo)PURE;
  STDMETHOD(SetDisEqc)(THIS_ UCHAR* pBuffer, ULONG nLen, ULONG nRepeatCount)PURE;
  STDMETHOD(DescrambleService)(THIS_ BYTE* PMT, int PMTLength,BOOL* succeeded)PURE;
};

// KNC1 device names
#define KNC1DVBSTuner                    L"KNC BDA DVB-S"
#define KNC1DVBS2Tuner                   L"KNC BDA DVB-S2"
#define KNC1DVBCTuner                    L"KNC BDA DVB-C"
#define KNC1DVBTTuner                    L"KNC BDA DVB-T"

class CKnc: public CUnknown, public IKNC
{
public:
  CKnc(LPUNKNOWN pUnk, HRESULT *phr);
  ~CKnc(void);
  DECLARE_IUNKNOWN

  STDMETHODIMP SetTunerFilter(IBaseFilter* tunerFilter, int iDeviceIndex);
  STDMETHODIMP IsKNC( BOOL* yesNo);
  STDMETHODIMP IsCamReady( BOOL* yesNo);
  STDMETHODIMP IsCIAvailable( BOOL* yesNo);
  STDMETHODIMP SetDisEqc( UCHAR* pBuffer, ULONG nLen, ULONG nRepeatCount);
  STDMETHODIMP DescrambleService( BYTE* PMT, int PMTLength,BOOL* succeeded);
  STDMETHODIMP DescrambleMultiple(WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded);

  char* LookupCIState(int State);
  void m_OnKncCiState(int State, LPCTSTR lpszMessage); /* callback Handler */

private: 
  TKNCBDA_CI_Enable			    *KNCBDA_CI_Enable;
  TKNCBDA_CI_Disable			  *KNCBDA_CI_Disable;
  TKNCBDA_CI_IsAvailable	  *KNCBDA_CI_IsAvailable;
  TKNCBDA_CI_IsReady			  *KNCBDA_CI_IsReady;
  TKNCBDA_CI_HW_Enable		  *KNCBDA_CI_HW_Enable;
  TKNCBDA_CI_GetName			  *KNCBDA_CI_GetName;
  TKNCBDA_CI_SendPMTCommand	*KNCBDA_CI_SendPMTCommand;
  TKNCBDA_CI_EnterMenu		  *KNCBDA_CI_EnterMenu;
  TKNCBDA_CI_SelectMenu		  *KNCBDA_CI_SelectMenu;
  TKNCBDA_CI_CloseMenu		  *KNCBDA_CI_CloseMenu;
  TKNCBDA_CI_SendMenuAnswer	*KNCBDA_CI_SendMenuAnswer;
  TKNCBDA_HW_Enable         *KNCBDA_HW_Enable;
  TKNCBDA_HW_DiSEqCWrite    *KNCBDA_HW_DiSEqCWrite;
  TKNCBDACICallback         m_callback;
  STDMETHODIMP              FreeKNCLibrary();
  bool                      m_bIsKNC;
  HINSTANCE                 m_hMod;
  UINT                      m_slot; /* default 0, hw-index for multiple cards, 0,1,2... how to find out???. */
  bool                      m_CAM_present;
};