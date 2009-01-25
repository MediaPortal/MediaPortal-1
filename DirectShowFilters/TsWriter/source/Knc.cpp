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
#include <atlconv.h>
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
  m_CAM_present             =false;
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
    if (KNCBDA_CI_IsReady(m_slot))
    {
      *yesNo=TRUE;
    }
  }
  m_CAM_present = *yesNo;
  LogDebug("KNC: card %d CI IsReady %d", m_slot, *yesNo);
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
  LogDebug("KNC: card %d CI IsAvailable %d", m_slot, *yesNo);
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
    LogDebug("KNC: card %d SetDiseqc:%s length:%d repeat:%d", m_slot, buffer,nLen,nRepeatCount);
    BOOL result = KNCBDA_HW_DiSEqCWrite(m_slot,pBuffer,nLen,nRepeatCount);
    LogDebug("KNC: card %d HW DiSEqCWrite: result:%d", m_slot, result);
  }
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
  if (m_bIsKNC && m_CAM_present)
  {
    char buffer[500];
    strcpy(buffer,"");
    for (int i=0; i < PMTLength;++i)
    {
      char tmp[30];
      sprintf(tmp,"%02.2x ", *(pmt+i));
      strcat(buffer,tmp);
    }
    LogDebug("KNC: PMT to decode: %s",buffer);
    BOOL result=KNCBDA_CI_SendPMTCommand(m_slot,pmt, PMTLength);
    LogDebug("KNC: card %d CI SendPMTCommand length:%d, result:%d", m_slot, PMTLength, result);
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
  CKnc* knc = (CKnc*)pParam;
  knc->m_OnKncCiState(State, lpszMessage);
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

/* translate CI state to message */
char* CKnc::LookupCIState(int State)
{
  char* CIStateText;
  switch (State)
  {
  case KNC_BDA_CI_STATE_INITIALIZING: 
    CIStateText = "Initializing";
    break;
  case KNC_BDA_CI_STATE_TRANSPORT: 
    CIStateText = "Transport";
    break;
  case KNC_BDA_CI_STATE_RESOURCE: 
    CIStateText = "Resource";
    break;
  case KNC_BDA_CI_STATE_APPLICATION: 
    CIStateText = "Application";
    break;
  case KNC_BDA_CI_STATE_CONDITIONAL_ACCESS: 
    CIStateText = "Conditional Access";
    break;
  case KNC_BDA_CI_STATE_READY: 
    CIStateText = "Ready";
    break;
  case KNC_BDA_CI_STATE_OPEN_SERVICE: 
    CIStateText = "Open Service";
    break;
  case KNC_BDA_CI_STATE_RELEASING: 
    CIStateText = "Releasing";
    break;
  case KNC_BDA_CI_STATE_CLOSE_MMI: 
    CIStateText = "Close MMI";
    break;
  case KNC_BDA_CI_STATE_REQUEST: 
    CIStateText = "Request";
    break;
  case KNC_BDA_CI_STATE_MENU: 
    CIStateText = "Menu";
    break;
  case KNC_BDA_CI_STATE_MENU_CHOICE: 
    CIStateText = "Menu Choice";
    break;
  case KNC_BDA_CI_STATE_OPEN_DISPLAY: 
    CIStateText = "Open Display";
    break;
  case KNC_BDA_CI_STATE_CLOSE_DISPLAY: 
    CIStateText = "Close Display";
    break;
  case KNC_BDA_CI_STATE_NONE: 
  default:
    CIStateText = "None";
    break;
  }
  return CIStateText;
}

/* Class member for one current card; Display Message Callback from API */
void CKnc::m_OnKncCiState(int State, LPCTSTR lpszMessage)
{
  BOOL result;
  LogDebug("KNC: card %d CI State: %s: %s", m_slot, lpszMessage, LookupCIState(State));
  IsCamReady(&result); // Call to check if CAM is inserted
}

//**************************************************************************************************
//* SetTunerFilter()
//* Called by application to set the tuner filter used
//* method checks if this tuner filter is a KNC device
//* and if so opens the KNC driver and CI for further usage
//**************************************************************************************************
STDMETHODIMP CKnc::SetTunerFilter(IBaseFilter* tunerFilter, int iDeviceIndex)
{
  BOOL result =false;
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
    memset(&m_callback,0,sizeof(m_callback));
    m_callback.pParam               = this;
    /* CI functions are already pointers */
    m_callback.OnKncCiState         =OnKncCiState;
    m_callback.OnKncCiOpenDisplay   =OnKncCiOpenDisplay;
    m_callback.OnKncCiMenu          =OnKncCiMenu;
    m_callback.OnKncCiMenuChoice    =OnKncCiMenuChoice;
    m_callback.OnKncCiRequest       =OnKncCiRequest;
    m_callback.OnKncCiCloseDisplay  =OnKncCiCloseDisplay;
    // Detect if passed filter is a KNC1 source. 
    FILTER_INFO info;
    if (!SUCCEEDED(tunerFilter->QueryFilterInfo(&info))) {
      LogDebug("KNC QueryFilterInfo failed.");
      return FreeKNCLibrary();
    }
    USES_CONVERSION; // for logging WCHAR 
    if (wcscmp(info.achName,KNC1DVBSTuner)  ==0|| wcscmp(info.achName,KNC1DVBS2Tuner) ==0||
      wcscmp(info.achName,KNC1DVBCTuner)  ==0|| wcscmp(info.achName,KNC1DVBTTuner)  ==0)
    {
      m_bIsKNC=true;
    }
    if (m_bIsKNC==false) {
      LogDebug("KNC not detected");
      return FreeKNCLibrary();
    }
    // iDeviceIndex passed by TvLibrary ! Enumerated by DevicePath
    m_slot = iDeviceIndex;
    LogDebug("KNC: card %d detected: %s", m_slot, W2A(info.achName));
    if (KNCBDA_HW_Enable(m_slot, tunerFilter))
    {
      LogDebug("KNC: card %d HW Enabled", m_slot);
    }
    if (KNCBDA_CI_Enable(m_slot, tunerFilter, &m_callback))
    {
      LogDebug("KNC: card %d CI slot enabled successfully", m_slot);
      if (KNCBDA_CI_HW_Enable(m_slot, TRUE)) 
        LogDebug("KNC: card %d CI HW enabled successfully", m_slot);
      else 
        LogDebug("KNC: card %d CI HW enable FAILED!", m_slot);
      if (KNCBDA_CI_IsReady(m_slot))
      {
        // remember CAM is present
        m_CAM_present = true;
        char nameBuffer[100];
        if (KNCBDA_CI_GetName(m_slot, nameBuffer, sizeof(nameBuffer)))
        {
          LogDebug("KNC: card %d CAM Type: %s", m_slot, nameBuffer);
        }
        else
        {
          LogDebug("KNC: CI_GetName failed.");
        }
      }
      else
      {
        LogDebug("KNC: card %d detected without CAM", m_slot);
        //m_bIsKNC=true;
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