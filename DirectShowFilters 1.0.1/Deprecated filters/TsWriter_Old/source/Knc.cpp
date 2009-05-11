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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <ks.h>
#include <ksproxy.h>
#include "knc.h"

extern void LogDebug(const char *fmt, ...) ;

//**************************************************************************************************
//* ctor
//**************************************************************************************************
CKnc::CKnc(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("MpTsKNC"), pUnk)
{
  m_bIsKNC=false; 
	KNCBDA_CI_Enable		=NULL;
	KNCBDA_CI_Disable		=NULL;
	KNCBDA_CI_IsAvailable	=NULL;
	KNCBDA_CI_IsReady		=NULL;
	KNCBDA_CI_HW_Enable		=NULL;
	KNCBDA_CI_GetName		=NULL;
	KNCBDA_CI_SendPMTCommand=NULL;
	KNCBDA_CI_EnterMenu		=NULL;
	KNCBDA_CI_SelectMenu	=NULL;
	KNCBDA_CI_CloseMenu		=NULL;
	KNCBDA_CI_SendMenuAnswer=NULL;
	m_hMod					=NULL;
}

//**************************************************************************************************
//* dtor
//* close device
//**************************************************************************************************
CKnc::~CKnc(void)
{
  if (m_bIsKNC)
  {
    KNCBDA_CI_Disable();
  }
  if (m_hMod!=NULL)
  {
    FreeLibrary(m_hMod);
    m_hMod=NULL;
  }
  m_hMod=NULL;
  m_bIsKNC=false;
}

//**************************************************************************************************
//* IsTechnoTrend()
//* Returns whether the tuner is a technotrend device or not
//**************************************************************************************************
STDMETHODIMP CKnc::IsKNC( BOOL* yesNo)
{
  *yesNo= (m_bIsKNC==true);
  return S_OK;
}

//**************************************************************************************************
//* IsCamReady()
//* Returns whether the CAM is ready
//**************************************************************************************************
STDMETHODIMP CKnc::IsCamReady( BOOL* yesNo)
{
  *yesNo=FALSE;
  if (m_bIsKNC)
  {
    if (KNCBDA_CI_IsAvailable() ==FALSE)
    {
      *yesNo=TRUE;
      return S_OK;
    }
    if (KNCBDA_CI_IsReady())
    {
      *yesNo=TRUE;
    }
  }
  return S_OK;
}

//**************************************************************************************************
//* SetDisEqc()
//* Sends a DisEqc message to the LNB (DVB-S)
//* diseqcType: specifies the diseqc type (simple A,B, level 1 A/A etc..)
//* highband  : specifies if we are tuned to highband(1) or lowband(0)
//* vertical  : specifies if we are using vertical (1) polarisation or horizontal(0)
//**************************************************************************************************
STDMETHODIMP CKnc::SetDisEqc(int diseqcType, int hiband, int vertical)
{	
  return S_OK;
}

//**************************************************************************************************
//* DescrambleMultiple()
//* Descrambles one or more programs
//* pNrs           : array containing service ids
//* NrOfOfPrograms : number of service ids in pNrs
//* succeeded      : on return specifies if decoding succeeded or not
//**************************************************************************************************
STDMETHODIMP CKnc::DescrambleMultiple(WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded)
{ 
  return S_OK;
}
//**************************************************************************************************
//* DescrambleService()
//* Sends the PMT to the CAM so it can start decoding the channel
//* PMT       : PMT table
//* PMTLength : length of PMT table
//* succeeded : on return specifies if decoding succeeded or not
//**************************************************************************************************
STDMETHODIMP CKnc::DescrambleService( BYTE* pmt, int PMTLength,BOOL* succeeded)
{
  if (m_bIsKNC)
  {
    BOOL result=KNCBDA_CI_SendPMTCommand(pmt, PMTLength);
    LogDebug("KNCBDA_CI_SendPMTCommand %d",result);
    *succeeded = (result==TRUE);
    //*succeeded=true;
  }
  else
  {
    *succeeded=true;
  }
  return S_OK;
}


void OnKncCiState(UCHAR slot,int State, LPCTSTR lpszMessage,PVOID pParam) 
{
  LogDebug("OnKncCiState slot:%d state:%d msg:%s", slot,State,lpszMessage);
}
void OnKncCiOpenDisplay(UCHAR slot,PVOID pParam) 
{
  LogDebug("OnKncCiOpenDisplay slot:%d", slot);
}
void OnKncCiMenu(UCHAR slot,LPCTSTR lpszTitle, LPCTSTR lpszSubTitle, LPCTSTR lpszBottom, UINT nNumChoices,PVOID pParam) 
{
  LogDebug("OnKncCiMenu slot:%d", slot);
  LogDebug("OnKncCiMenu title:%s", lpszTitle);
  LogDebug("OnKncCiMenu subtitle:%s", lpszSubTitle);
  LogDebug("OnKncCiMenu bottom:%s", lpszSubTitle);
  LogDebug("OnKncCiMenu lpszBottom:%s", lpszSubTitle);
  LogDebug("OnKncCiMenu nNumChoices:%d", nNumChoices);
}
void OnKncCiMenuChoice(UCHAR slot,UINT nChoice, LPCTSTR lpszText,PVOID pParam) 
{
  LogDebug("OnKncCiMenuChoice slot:%d choice:%d text:%s", slot,nChoice,lpszText);
}
void OnKncCiRequest(UCHAR slot,BOOL bBlind, UINT nAnswerLength, LPCTSTR lpszText,PVOID pParam) 
{
  LogDebug("OnKncCiMenuChoice slot:%d bBlind:%d nAnswerLength:%d text:%s", slot,bBlind, nAnswerLength, lpszText);
}
void OnKncCiCloseDisplay(UCHAR slot,UINT nDelay,PVOID pParam) 
{
  LogDebug("OnKncCiCloseDisplay slot:%d nDelay:%d", slot,nDelay);
}

//**************************************************************************************************
//* SetTunerFilter()
//* Called by application to set the tuner filter used
//* method checks if this tuner filter is a techno-trend device
//* and ifso opens the Technotrend driver and CI for further usage
//**************************************************************************************************
STDMETHODIMP CKnc::SetTunerFilter(IBaseFilter* tunerFilter)
{
  m_bIsKNC=false;
	m_hMod=LoadLibrary("KNCBDACTRL.dll");
  if (m_hMod!=NULL)
  {
    KNCBDA_CI_Enable=(TKNCBDA_CI_Enable*)GetProcAddress(m_hMod,"KNCBDA_CI_Enable");
		if(KNCBDA_CI_Enable==NULL)
		{
      LogDebug("KNCBDA_CI_Enable not found in dll");
			return S_OK;
		}

		KNCBDA_CI_Disable=(TKNCBDA_CI_Disable*)GetProcAddress(m_hMod,"KNCBDA_CI_Disable");
		if(KNCBDA_CI_Disable==NULL)
		{
      LogDebug("KNCBDA_CI_Disable not found in dll");
			return S_OK;
		}

		KNCBDA_CI_IsAvailable=(TKNCBDA_CI_IsAvailable*)GetProcAddress(m_hMod,"KNCBDA_CI_IsAvailable");
		if(KNCBDA_CI_IsAvailable==NULL)
		{
      LogDebug("KNCBDA_CI_IsAvailable not found in dll");
			return S_OK;
		}

		KNCBDA_CI_IsReady=(TKNCBDA_CI_IsReady*)GetProcAddress(m_hMod,"KNCBDA_CI_IsReady");
		if(KNCBDA_CI_IsReady==NULL)
		{
      LogDebug("KNCBDA_CI_IsReady not found in dll");
			return S_OK;
		}

		KNCBDA_CI_HW_Enable=(TKNCBDA_CI_HW_Enable*)GetProcAddress(m_hMod,"KNCBDA_CI_HW_Enable");
		if(KNCBDA_CI_HW_Enable==NULL)
		{
      LogDebug("KNCBDA_CI_HW_Enable not found in dll");
			return S_OK;
		}

		KNCBDA_CI_GetName=(TKNCBDA_CI_GetName*)GetProcAddress(m_hMod,"KNCBDA_CI_GetName");
		if(KNCBDA_CI_GetName==NULL)
		{
      LogDebug("KNCBDA_CI_GetName not found in dll");
			return S_OK;
		}

		KNCBDA_CI_SendPMTCommand=(TKNCBDA_CI_SendPMTCommand*)GetProcAddress(m_hMod,"KNCBDA_CI_SendPMTCommand");
		if(KNCBDA_CI_SendPMTCommand==NULL)
		{
      LogDebug("KNCBDA_CI_SendPMTCommand not found in dll");
			return S_OK;
		}

		KNCBDA_CI_EnterMenu=(TKNCBDA_CI_EnterMenu*)GetProcAddress(m_hMod,"KNCBDA_CI_EnterMenu");
		if(KNCBDA_CI_EnterMenu==NULL)
		{
      LogDebug("KNCBDA_CI_EnterMenu not found in dll");
			return S_OK;
		}

		KNCBDA_CI_SelectMenu=(TKNCBDA_CI_SelectMenu*)GetProcAddress(m_hMod,"KNCBDA_CI_SelectMenu");
		if(KNCBDA_CI_SelectMenu==NULL)
		{
      LogDebug("KNCBDA_CI_SelectMenu not found in dll");
			return S_OK;
		}

		KNCBDA_CI_CloseMenu=(TKNCBDA_CI_CloseMenu*)GetProcAddress(m_hMod,"KNCBDA_CI_CloseMenu");
		if(KNCBDA_CI_CloseMenu==NULL)
		{
      LogDebug("KNCBDA_CI_CloseMenu not found in dll");
			return S_OK; 
		}

		KNCBDA_CI_SendMenuAnswer=(TKNCBDA_CI_SendMenuAnswer*)GetProcAddress(m_hMod,"KNCBDA_CI_SendMenuAnswer");
		if(KNCBDA_CI_SendMenuAnswer==NULL)
		{
      LogDebug("KNCBDA_CI_SendMenuAnswer not found in dll");
			return S_OK;  
		}
    LogDebug("KNCBDA_CI_Enable");
    m_callback.pParam=this;
    m_callback.OnKncCiState=&OnKncCiState;
    m_callback.OnKncCiOpenDisplay=&OnKncCiOpenDisplay;
    m_callback.OnKncCiMenu=&OnKncCiMenu;
    m_callback.OnKncCiMenuChoice=&OnKncCiMenuChoice;
    m_callback.OnKncCiRequest=&OnKncCiRequest;
    m_callback.OnKncCiCloseDisplay=&OnKncCiCloseDisplay;
    if ( KNCBDA_CI_Enable(tunerFilter,&m_callback))
    {
      if (KNCBDA_CI_IsReady())
      {
				m_bIsKNC=true;
        LogDebug("knc card detected with CAM");
        KNCBDA_CI_HW_Enable(TRUE);
      }
      else
      {
        LogDebug("knc card detected without CAM");
      }
    }
  }
  else
  {
    char buffer[2048];
    GetCurrentDirectory(sizeof(buffer),buffer);
    LogDebug("knc unable to load KNCBDACTRL.dll:%d",GetLastError());
    //LogDebug("%s",buffer);
  }
  return S_OK;
}
