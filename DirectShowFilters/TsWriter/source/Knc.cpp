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
#include <stdio.h>
#include "knc.h"

extern void LogDebug(const char *fmt, ...) ;

//**************************************************************************************************
//* ctor
//**************************************************************************************************
CKnc::CKnc(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("MpTsKNC"), pUnk)
{
  m_bIsKNC                  =false;
  KNCBDA_CI_Enable		      =NULL;
  KNCBDA_CI_Disable		      =NULL;
  KNCBDA_CI_IsAvailable	    =NULL;
  KNCBDA_CI_IsReady		      =NULL;
  KNCBDA_CI_HW_Enable		    =NULL;
  KNCBDA_CI_GetName		      =NULL;
  KNCBDA_CI_SendPMTCommand  =NULL;
  KNCBDA_CI_EnterMenu		    =NULL;
  KNCBDA_CI_SelectMenu	    =NULL;
  KNCBDA_CI_CloseMenu		    =NULL;
  KNCBDA_CI_SendMenuAnswer  =NULL;
  KNCBDA_HW_Enable          =NULL;
  KNCBDA_HW_DiSEqCWrite     =NULL;
  m_hMod					          =NULL;
  m_slot                    =0;
  m_verboseLogging          =false;
}

//**************************************************************************************************
//* dtor
//* close device
//**************************************************************************************************
CKnc::~CKnc(void)
{
  FreeKNCLibrary();
}

//**************************************************************************************************
//* FreeKNCLibrary()
//* close device and free library; default "exit" if device not detected or api dll functions missing
//**************************************************************************************************
STDMETHODIMP CKnc::FreeKNCLibrary()
{
  if (m_hMod!=NULL)
  {
    // Always call CI_Disable to free the library. No matter if the card is a knc one or not
    KNCBDA_CI_Disable(m_slot);
    FreeLibrary(m_hMod);
  }
  m_hMod=NULL;
  m_bIsKNC=false;
  return S_OK;
}

//**************************************************************************************************
//* IsKNC()
//* Returns whether the tuner is a KNC device or not
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
    if (KNCBDA_CI_IsAvailable(m_slot) == FALSE)
    {
      *yesNo=FALSE;
    }
    if (KNCBDA_CI_IsReady(m_slot))
    {
      *yesNo=TRUE;
    }
  }
  LogDebug("KNCBDA_CI_IsReady %d",*yesNo);
  return S_OK;
}

//**************************************************************************************************
//* IsCIAvailable()
//* Returns whether the CI clot is available
//**************************************************************************************************
STDMETHODIMP CKnc::IsCIAvailable( BOOL* yesNo)
{
  *yesNo=FALSE;
  if (m_bIsKNC)
  {
    if (KNCBDA_CI_IsAvailable(m_slot))
    {
      *yesNo=TRUE;
    }
  }
  LogDebug("KNCBDA_CI_IsAvailable %d",*yesNo);
  return S_OK;
}

//**************************************************************************************************
//* SetDisEqc()
//* Sends a DisEqc message to the LNB (DVB-S)
//* diseqcType: specifies the diseqc type (simple A,B, level 1 A/A etc..)
//* highband  : specifies if we are tuned to highband(1) or lowband(0)
//* vertical  : specifies if we are using vertical (1) polarisation or horizontal(0)
//**************************************************************************************************
STDMETHODIMP CKnc::SetDisEqc(UCHAR* pBuffer, ULONG nLen, ULONG nRepeatCount)
{	
  if (m_bIsKNC)
  {
    char buffer[129];
    strcpy(buffer,"");
    for (int i=0; i < (int)nLen;++i)
    {
      char tmp[30];
      sprintf(tmp,"%02.2x",(int)pBuffer[i]);
      strcat(buffer,tmp);
    }
    LogDebug("KNC: SetDiseqc:%s length:%d repeat:%d",buffer,nLen,nRepeatCount);
    BOOL result = KNCBDA_HW_DiSEqCWrite(m_slot,pBuffer,nLen,nRepeatCount);
    LogDebug("KNCBDA_HW_DiSEqCWrite: result:%d",result);
  }
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
    BOOL result=KNCBDA_CI_SendPMTCommand(m_slot,pmt, PMTLength);
    LogDebug("KNCBDA_CI_SendPMTCommand #1 PMTLength:%d, result:%d",PMTLength,result);
    *succeeded = (result==TRUE);
  }
  else
  {
    *succeeded=true;
  }
  return S_OK;
}

/* callback Handlers */
void OnKncCiState(UCHAR slot,int State, LPCTSTR lpszMessage,PVOID pParam) 
{
  LogDebug("OnKncCiState slot:%d state:%d msg:%s", slot,State,lpszMessage);
  CKnc* knc = (CKnc*)pParam;
  knc->m_OnKncCiState(State, lpszMessage);
}
void CKnc::m_OnKncCiState(int State, LPCTSTR lpszMessage)
{
  LogDebug("m_OnKncCiState m_slot:%d state:%d msg:%s", m_slot,State,lpszMessage);
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
  LogDebug("OnKncCiRequest slot:%d bBlind:%d nAnswerLength:%d text:%s", slot,bBlind, nAnswerLength, lpszText);
}
void OnKncCiCloseDisplay(UCHAR slot,UINT nDelay,PVOID pParam) 
{
  LogDebug("OnKncCiCloseDisplay slot:%d nDelay:%d", slot,nDelay);
}

//**************************************************************************************************
//* SetTunerFilter()
//* Called by application to set the tuner filter used
//* method checks if this tuner filter is a KNC device
//* and if so opens the KNC driver and CI for further usage
//**************************************************************************************************
STDMETHODIMP CKnc::SetTunerFilter(IBaseFilter* tunerFilter)
{
  BOOL result;
  m_bIsKNC=false;
  m_hMod=LoadLibrary("KNCBDACTRL.dll");
  if (m_hMod!=NULL)
  {
    KNCBDA_CI_Enable=(TKNCBDA_CI_Enable*)GetProcAddress(m_hMod,"KNCBDA_CI_Enable");
    if(KNCBDA_CI_Enable==NULL)
    {
      LogDebug("KNCBDA_CI_Enable not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_Disable=(TKNCBDA_CI_Disable*)GetProcAddress(m_hMod,"KNCBDA_CI_Disable");
    if(KNCBDA_CI_Disable==NULL)
    {
      LogDebug("KNCBDA_CI_Disable not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_IsAvailable=(TKNCBDA_CI_IsAvailable*)GetProcAddress(m_hMod,"KNCBDA_CI_IsAvailable");
    if(KNCBDA_CI_IsAvailable==NULL)
    {
      LogDebug("KNCBDA_CI_IsAvailable not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_IsReady=(TKNCBDA_CI_IsReady*)GetProcAddress(m_hMod,"KNCBDA_CI_IsReady");
    if(KNCBDA_CI_IsReady==NULL)
    {
      LogDebug("KNCBDA_CI_IsReady not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_HW_Enable=(TKNCBDA_CI_HW_Enable*)GetProcAddress(m_hMod,"KNCBDA_CI_HW_Enable");
    if(KNCBDA_CI_HW_Enable==NULL)
    {
      LogDebug("KNCBDA_CI_HW_Enable not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_GetName=(TKNCBDA_CI_GetName*)GetProcAddress(m_hMod,"KNCBDA_CI_GetName");
    if(KNCBDA_CI_GetName==NULL)
    {
      LogDebug("KNCBDA_CI_GetName not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_SendPMTCommand=(TKNCBDA_CI_SendPMTCommand*)GetProcAddress(m_hMod,"KNCBDA_CI_SendPMTCommand");
    if(KNCBDA_CI_SendPMTCommand==NULL)
    {
      LogDebug("KNCBDA_CI_SendPMTCommand not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_EnterMenu=(TKNCBDA_CI_EnterMenu*)GetProcAddress(m_hMod,"KNCBDA_CI_EnterMenu");
    if(KNCBDA_CI_EnterMenu==NULL)
    {
      LogDebug("KNCBDA_CI_EnterMenu not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_SelectMenu=(TKNCBDA_CI_SelectMenu*)GetProcAddress(m_hMod,"KNCBDA_CI_SelectMenu");
    if(KNCBDA_CI_SelectMenu==NULL)
    {
      LogDebug("KNCBDA_CI_SelectMenu not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_CloseMenu=(TKNCBDA_CI_CloseMenu*)GetProcAddress(m_hMod,"KNCBDA_CI_CloseMenu");
    if(KNCBDA_CI_CloseMenu==NULL)
    {
      LogDebug("KNCBDA_CI_CloseMenu not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_CI_SendMenuAnswer=(TKNCBDA_CI_SendMenuAnswer*)GetProcAddress(m_hMod,"KNCBDA_CI_SendMenuAnswer");
    if(KNCBDA_CI_SendMenuAnswer==NULL)
    {
      LogDebug("KNCBDA_CI_SendMenuAnswer not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_HW_Enable=(TKNCBDA_HW_Enable*)GetProcAddress(m_hMod,"KNCBDA_HW_Enable");
    if(KNCBDA_HW_Enable==NULL)
    {
      LogDebug("KNCBDA_HW_Enable not found in dll");
      return FreeKNCLibrary();
    }

    KNCBDA_HW_DiSEqCWrite=(TKNCBDA_HW_DiSEqCWrite*)GetProcAddress(m_hMod,"KNCBDA_HW_DiSEqCWrite");
    if(KNCBDA_HW_DiSEqCWrite==NULL)
    {
      LogDebug("KNCBDA_HW_DiSEqCWrite not found in dll");
      return FreeKNCLibrary();
    }
    LogDebug("KNCBDA_CI_MemSet");
    memset(&m_callback,0,sizeof(m_callback));
    m_callback.pParam               = this;
    /* CI functions are already pointers */
    m_callback.OnKncCiState         =OnKncCiState;
    m_callback.OnKncCiOpenDisplay   =OnKncCiOpenDisplay;
    m_callback.OnKncCiMenu          =OnKncCiMenu;
    m_callback.OnKncCiMenuChoice    =OnKncCiMenuChoice;
    m_callback.OnKncCiRequest       =OnKncCiRequest;
    m_callback.OnKncCiCloseDisplay  =OnKncCiCloseDisplay;
    if (m_verboseLogging)
    {
      LogDebug("Callback struct:");
      LogDebug("m_callback.pParam:             %x",             m_callback.pParam);
      LogDebug("m_callback.OnKncCiState:       %x",       m_callback.OnKncCiState);
      LogDebug("m_callback.OnKncCiOpenDisplay: %x", m_callback.OnKncCiOpenDisplay);
      LogDebug("m_callback.OnKncCiMenu:        %x",        m_callback.OnKncCiMenu);
      LogDebug("m_callback.OnKncCiMenuChoice:  %x",  m_callback.OnKncCiMenuChoice);
      LogDebug("m_callback.OnKncCiRequest:     %x",     m_callback.OnKncCiRequest);
      LogDebug("m_callback.OnKncCiCloseDisplay:%x",m_callback.OnKncCiCloseDisplay);
    }
    LogDebug("KNCBDA_HW_Enable");
    if (KNCBDA_HW_Enable(m_slot,tunerFilter))
    {
      LogDebug("KNC card HW Enabled");
      m_bIsKNC=true;
    }
    LogDebug("KNCBDA_CI_Enable");
    if (KNCBDA_CI_Enable(m_slot,tunerFilter,&m_callback))
    {
      LogDebug("KNC CI enabled successfully");
      if (KNCBDA_CI_IsReady(m_slot))
      {
        m_bIsKNC=true;
        LogDebug("KNCBDA_CI_HW_Enable");
        result = KNCBDA_CI_HW_Enable(m_slot,TRUE);
        LogDebug("KNC card detected with CAM; CI_HW_Enable result:%d", result);
        try
        {
          LogDebug("KNCBDA_CI_GetName");
          char nameBuffer[100];
          result = KNCBDA_CI_GetName(m_slot, nameBuffer, sizeof(nameBuffer));
          LogDebug("CAM Type: %s", nameBuffer);
        }
        catch(...)
        {
          LogDebug("KNCBDA_CI_GetName failed.");
        }
      }
      else
      {
        LogDebug("KNC card detected without CAM");
        m_bIsKNC=true;
      }
    }
    if (m_bIsKNC == false)
    {
      LogDebug("KNC not detected. releasing library");
      return FreeKNCLibrary();
    }
  }
  else
  {
    char buffer[2048];
    GetCurrentDirectory(sizeof(buffer),buffer);
    LogDebug("KNC unable to load KNCBDACTRL.dll:%d",GetLastError());
    //LogDebug("%s",buffer);
  }
  return S_OK;
}