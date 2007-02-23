/* 
 *	Copyright (C) 2006 Team MediaPortal
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
    m_pKNC=NULL;
}

//**************************************************************************************************
//* dtor
//* close device
//**************************************************************************************************
CKnc::~CKnc(void)
{
  if (m_bIsKNC)
  {
    if (m_pKNC)
    {
      m_pKNC->KNCBDA_CI_Disable();
    }
  }
  if (m_pKNC!=NULL)
  {
    delete m_pKNC;
  }
  m_pKNC=NULL;
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
  LogDebug("KNCBDA_CI_Enable");
  m_pKNC = new CKNCBDACI();
  if (m_pKNC->KNCBDA_CI_Enable(tunerFilter,this))
  {
    LogDebug("knc card detected");
    m_bIsKNC=true;
    if (m_pKNC->KNCBDA_CI_IsAvailable())
    {
      LogDebug("knc card detected with CAM");
      m_pKNC->KNCBDA_CI_HW_Enable(TRUE);
    }
  }
  return S_OK;
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
    if (m_pKNC->KNCBDA_CI_IsAvailable() ==FALSE)
    {
      *yesNo=TRUE;
      return S_OK;
    }
    if (m_pKNC->KNCBDA_CI_IsReady())
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
    BOOL result=m_pKNC->KNCBDA_CI_SendPMTCommand(pmt, PMTLength);
    LogDebug("KNCBDA_CI_SendPMTCommand %d",result);
    *succeeded = (result==TRUE);
  }
  else
  {
    *succeeded=true;
  }
  return S_OK;
}


void CKnc::OnKncCiState(UCHAR slot,int State, LPCTSTR lpszMessage) 
{
  LogDebug("OnKncCiState slot:%d state:%d msg:%s", slot,State,lpszMessage);
}
void CKnc::OnKncCiOpenDisplay(UCHAR slot) 
{
  LogDebug("OnKncCiOpenDisplay slot:%d", slot);
}
void CKnc::OnKncCiMenu(UCHAR slot,LPCTSTR lpszTitle, LPCTSTR lpszSubTitle, LPCTSTR lpszBottom, UINT nNumChoices) 
{
  LogDebug("OnKncCiMenu slot:%d", slot);
  LogDebug("OnKncCiMenu title:%s", lpszTitle);
  LogDebug("OnKncCiMenu subtitle:%s", lpszSubTitle);
  LogDebug("OnKncCiMenu bottom:%s", lpszSubTitle);
  LogDebug("OnKncCiMenu lpszBottom:%s", lpszSubTitle);
  LogDebug("OnKncCiMenu nNumChoices:%d", nNumChoices);
}
void CKnc::OnKncCiMenuChoice(UCHAR slot,UINT nChoice, LPCTSTR lpszText) 
{
  LogDebug("OnKncCiMenuChoice slot:%d choice:%d text:%s", slot,nChoice,lpszText);
}
void CKnc::OnKncCiRequest(UCHAR slot,BOOL bBlind, UINT nAnswerLength, LPCTSTR lpszText) 
{
  LogDebug("OnKncCiMenuChoice slot:%d bBlind:%d nAnswerLength:%d text:%s", slot,bBlind, nAnswerLength, lpszText);
}
void CKnc::OnKncCiCloseDisplay(UCHAR slot,UINT nDelay) 
{
  LogDebug("OnKncCiCloseDisplay slot:%d nDelay:%d", slot,nDelay);
}