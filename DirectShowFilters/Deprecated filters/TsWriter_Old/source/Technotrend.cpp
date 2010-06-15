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
#include "Technotrend.h"
#include "bdaapi_cimsg.h"

// Technotrend device names
#define LBDG2_NAME                       L"TechnoTrend BDA/DVB Capture"
#define LBDG2_NAME_C_TUNER               L"TechnoTrend BDA/DVB-C Tuner"
#define LBDG2_NAME_S_TUNER               L"TechnoTrend BDA/DVB-S Tuner"
#define LBDG2_NAME_T_TUNER               L"TechnoTrend BDA/DVB-T Tuner"
#define LBDG2_NAME_NEW                   L"ttBudget2 BDA DVB Capture"
#define LBDG2_NAME_C_TUNER_NEW           L"ttBudget2 BDA DVB-C Tuner"
#define LBDG2_NAME_S_TUNER_NEW           L"ttBudget2 BDA DVB-S Tuner"
#define LBDG2_NAME_T_TUNER_NEW           L"ttBudget2 BDA DVB-T Tuner"
#define LBUDGET3NAME                     L"TTHybridTV BDA Digital Capture"
#define LBUDGET3NAME_TUNER               L"TTHybridTV BDA DVBT Tuner"
#define LBUDGET3NAME_ATSC_TUNER          L"TTHybridTV BDA ATSC Tuner"
#define LBUDGET3NAME_TUNER_ANLG          L"TTHybridTV BDA Analog TV Tuner"
#define LBUDGET3NAME_ANLG                L"TTHybridTV BDA Analog Capture"
#define LUSB2BDA_DVB_NAME                L"USB 2.0 BDA DVB Capture"
#define LUSB2BDA_DSS_NAME                L"USB 2.0 BDA DSS Capture"
#define LUSB2BDA_DSS_NAME_TUNER          L"USB 2.0 BDA DSS Tuner"
#define LUSB2BDA_DVB_NAME_C_TUNER        L"USB 2.0 BDA DVB-C Tuner"
#define LUSB2BDA_DVB_NAME_S_TUNER        L"USB 2.0 BDA DVB-S Tuner"
#define LUSB2BDA_DVB_NAME_S_TUNER_FAKE   L"USB 2.0 BDA (DVB-T Fake) DVB-T Tuner"
#define LUSB2BDA_DVB_NAME_T_TUNER        L"USB 2.0 BDA DVB-T Tuner"
#define LUSB2BDA_DVBS_NAME_PIN           L"Pinnacle PCTV 4XXe Capture"
#define LUSB2BDA_DVBS_NAME_PIN_TUNER     L"Pinnacle PCTV 4XXe Tuner"

//Slot Status
/// Common interface slot is empty.
#define	CI_SLOT_EMPTY               0
/// A CAM is inserted into the common interface.
#define	CI_SLOT_MODULE_INSERTED     1
/// CAM initialisation ready.
#define	CI_SLOT_MODULE_OK           2
/// CAM initialisation ready.
#define CI_SLOT_CA_OK               3
/// CAM initialisation ready.
#define	CI_SLOT_DBG_MSG             4
/// Slot state could not be determined.
#define	CI_SLOT_UNKNOWN_STATE       0xFF

//SendCIMessage Tags
#define	CI_MSG_NONE                 0
#define	CI_MSG_CI_INFO              1
#define	CI_MSG_MENU                 2
#define	CI_MSG_LIST                 3
#define	CI_MSG_TEXT                 4
#define	CI_MSG_REQUEST_INPUT        5
#define	CI_MSG_INPUT_COMPLETE       6
#define	CI_MSG_LIST_MORE            7
#define	CI_MSG_MENU_MORE            8
#define	CI_MSG_CLOSE_MMI_IMM        9
#define	CI_MSG_SECTION_REQUEST      0xA
#define	CI_MSG_CLOSE_FILTER         0xB
#define	CI_PSI_COMPLETE             0xC
#define	CI_MODULE_READY             0xD
#define	CI_SWITCH_PRG_REPLY         0xE
#define	CI_MSG_TEXT_MORE            0xF

extern void LogDebug(const char *fmt, ...) ;

//*** type defs for GetProcAddress
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPIOPENCI)(HANDLE hOpen,TS_CiCbFcnPointer CbFuncPointer);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPIGETDRVVERSION)(HANDLE hOpen, BYTE *v1, BYTE *v2,BYTE *v3, BYTE *v4);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPICLOSECI)(HANDLE   hOpen);
typedef void         (FAR PASCAL *BDAAPICLOSE)(HANDLE      hOpen);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPISETDVBTANTPWR)(HANDLE hOpen,BOOL   bAntPwrOnOff);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPISETDISEQCMSG)(HANDLE        hOpen,BYTE         *pData,BYTE          Bytes,BYTE          Repeat,BYTE          Toneburst,Polarisation  ePolarity);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPICIGETSLOTSTATUS)(HANDLE hOpen, BYTE nSlot);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPICIREADPSIFASTWITHPMT)(HANDLE hOpen,BYTE   *pPMT,WORD   wLength);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPICIREADPSIFASTDRVDEMUX)(HANDLE hOpen,WORD   PNR);
typedef HANDLE       (FAR PASCAL *BDAAPIOPENHWIDX) (DEVICE_CAT DevType,UINT        uiDevID);
typedef TYPE_RET_VAL (FAR PASCAL *BDAAPICIMULTIDECODE)(HANDLE hOpen,WORD  *PNR,int    NrOfPnrs);

//**************************************************************************************************
//* ctor
//**************************************************************************************************
CTechnotrend::CTechnotrend(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("MpTsTechnoTrend"), pUnk)
{
  m_hBdaApi = INVALID_HANDLE_VALUE;
  m_deviceType=UNKNOWN;
  m_ciStatus=-1;
  m_dll=NULL;
}

//**************************************************************************************************
//* dtor
//* close device
//**************************************************************************************************
CTechnotrend::~CTechnotrend(void)
{
  if (m_dll!=NULL && m_hBdaApi != INVALID_HANDLE_VALUE)
  {
    BDAAPICLOSECI closeCI=(BDAAPICLOSECI)GetProcAddress(m_dll,"_bdaapiCloseCI@4");
    if (closeCI!=NULL)
    {
      closeCI(m_hBdaApi);
    }
    else
    {
      LogDebug("Technotrend: unable to get proc adress of bdaapiCloseCI");
    }

    BDAAPICLOSE closeapi=(BDAAPICLOSE)GetProcAddress(m_dll,"_bdaapiClose@4");
    if (closeapi!=NULL)
    {
      closeapi(m_hBdaApi);
    }
    else
    {
      LogDebug("Technotrend: unable to get proc adress of bdaapiClose");
    }
  }
  if (m_dll!=NULL)
  {
    FreeLibrary(m_dll);
    m_dll=NULL;
  }
}


//**************************************************************************************************
//* callback from driver when the CI slot state changes
//**************************************************************************************************
static void __stdcall OnSlotStatusCallback(PVOID Context,BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
{
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
  technoTrend->OnSlotChange( nSlot, nStatus, csInfo);
}

//**************************************************************************************************
//* callback from driver when the CA state changes
//**************************************************************************************************
static void __stdcall OnCaStatusCallback(PVOID Context,BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus)
{
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
  technoTrend->OnCaChange(nSlot,nReplyTag,wStatus);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnDisplayString(PVOID Context,BYTE  nSlot,char* pString,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayString slot:%d %s", nSlot,pString);
}

//**************************************************************************************************
//* callback from driver to display the CI menu
//**************************************************************************************************
static void __stdcall OnDisplayMenu(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayMenu slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
/// \param Context      Can be used for a context pointer in the calling
///                     application. This parameter can be NULL.
/// \param nSlot        Is the Slot ID.
/// \param wItems       Number of Items in the List
/// \param pStringArray Contains all strings of the list.
/// \param wLength      Length of the string array.
//**************************************************************************************************
static void __stdcall OnDisplayList(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayList slot:%d items:%d len:%d", nSlot, wItems, wLength);
  char* szBuf = new char[wLength+1];
  int c=0;
  for (int i=0; i < wLength;++i)
  {
    szBuf[c++] = pStringArray[i];
    if (pStringArray[i]==0)
    {
      szBuf[c++] =0;
      LogDebug("technotrend  %s",szBuf);
      c=0;
    }
  }
  delete[] szBuf;

}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnSwitchOsdOff(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:CI_OnSwitchOsdOff slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnInputRequest(PVOID Context,BYTE  nSlot,BOOL  bBlindAnswer,BYTE  nExpectedLength, DWORD dwKeyMask)
{
  LogDebug("TechnoTrend:CI_OnInputRequest slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscSetDescriptor(PVOID Context,BYTE  nSlot,TYPE_CONNECT_DESCR* pDescriptor)
{
  LogDebug("TechnoTrend:OnLscSetDescriptor slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscConnect(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscConnect slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscDisconnect(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscDisconnect slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscSetParams(PVOID Context,BYTE  nSlot,BYTE  BufferSize,BYTE  Timeout10Ms)
{
  LogDebug("TechnoTrend:OnLscSetParams slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscEnquireStatus(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscEnquireStatus slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscGetNextBuffer(PVOID Context,BYTE  nSlot,BYTE  PhaseID)
{
  LogDebug("TechnoTrend:OnLscGetNextBuffer slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void __stdcall OnLscTransmitBuffer(PVOID Context,BYTE  nSlot,BYTE  PhaseID,BYTE* pData,WORD  nLength)
{
  LogDebug("TechnoTrend:OnLscTransmitBuffer slot:%d", nSlot);
}

//**************************************************************************************************
//* SetTunerFilter()
//* Called by application to set the tuner filter used
//* method checks if this tuner filter is a techno-trend device
//* and ifso opens the Technotrend driver and CI for further usage
//**************************************************************************************************
STDMETHODIMP CTechnotrend::SetTunerFilter(IBaseFilter* tunerFilter)
{
  if (m_dll==NULL)
  {
    m_hBdaApi = INVALID_HANDLE_VALUE;
    m_dll=LoadLibrary("ttBdaDrvApi_Dll.dll");
    if (m_dll==NULL) 
    {
      LogDebug("TechnoTrend:unable to load ttBdaDrvApi_Dll.dll:%d",GetLastError());
      return S_OK;
    }
    LogDebug("TechnoTrend:loaded ttBdaDrvApi_Dll.dll");
  }

  FILTER_INFO info;
  if (!SUCCEEDED(tunerFilter->QueryFilterInfo(&info))) return S_OK;
  if (wcscmp(info.achName,LBDG2_NAME_C_TUNER)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_S_TUNER)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_T_TUNER)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_C_TUNER_NEW)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_S_TUNER_NEW)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_T_TUNER_NEW)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBUDGET3NAME_TUNER)==0) m_deviceType=BUDGET_3;
  if (wcscmp(info.achName,LBUDGET3NAME_ATSC_TUNER)==0) m_deviceType=BUDGET_3;
  if (wcscmp(info.achName,LBUDGET3NAME_TUNER_ANLG)==0) m_deviceType=BUDGET_3;
  if (wcscmp(info.achName,LUSB2BDA_DVB_NAME_C_TUNER)==0) m_deviceType=USB_2;
  if (wcscmp(info.achName,LUSB2BDA_DVB_NAME_S_TUNER)==0) m_deviceType=USB_2;
  if (wcscmp(info.achName,LUSB2BDA_DVB_NAME_T_TUNER)==0) m_deviceType=USB_2;
  if (wcscmp(info.achName,LUSB2BDA_DVBS_NAME_PIN_TUNER)==0) m_deviceType=USB_2_PINNACLE;
  if (m_deviceType==UNKNOWN) return S_OK;

  LogDebug("Technotrend: card detected type:%d",m_deviceType);
  UINT deviceId;
  if (!GetDeviceID(tunerFilter, deviceId) )
  {
    LogDebug("Technotrend: unable to determine the device id");
    m_deviceType=UNKNOWN;  
    return S_OK;
  }
  BDAAPIOPENHWIDX openHwIdx= (BDAAPIOPENHWIDX)GetProcAddress(m_dll,"_bdaapiOpenHWIdx@8");
  if (openHwIdx!=NULL)
  {
    m_hBdaApi = openHwIdx(m_deviceType, deviceId);
    if (m_hBdaApi == INVALID_HANDLE_VALUE) 
    {
      LogDebug("Technotrend: unable to open the device");
      return S_OK;
    }
    LogDebug("Technotrend: OpenHWIdx succeeded");
  }
  else
  {
    LogDebug("Technotrend: unable to get proc adress of bdaapiOpenHWIdx");
    return S_OK;
  }

  memset(&m_technoTrendStructure,0,sizeof(m_technoTrendStructure));
  m_technoTrendStructure.p01Context=m_technoTrendStructure.p02Context=m_technoTrendStructure.p03Context=m_technoTrendStructure.p04Context=this;
  m_technoTrendStructure.p05Context=m_technoTrendStructure.p06Context=m_technoTrendStructure.p07Context=m_technoTrendStructure.p08Context=this;
  m_technoTrendStructure.p09Context=m_technoTrendStructure.p10Context=m_technoTrendStructure.p11Context=m_technoTrendStructure.p12Context=this;
  m_technoTrendStructure.p13Context=m_technoTrendStructure.p14Context=this;
  m_technoTrendStructure.p01=OnSlotStatusCallback;
  m_technoTrendStructure.p02=OnCaStatusCallback;
  m_technoTrendStructure.p03=OnDisplayString;
  m_technoTrendStructure.p04=OnDisplayMenu;
  m_technoTrendStructure.p05=OnDisplayList;
  m_technoTrendStructure.p06=OnSwitchOsdOff;
  m_technoTrendStructure.p07=OnInputRequest;
  m_technoTrendStructure.p08=OnLscSetDescriptor;
  m_technoTrendStructure.p09=OnLscConnect;
  m_technoTrendStructure.p10=OnLscDisconnect;
  m_technoTrendStructure.p11=OnLscSetParams;
  m_technoTrendStructure.p12=OnLscEnquireStatus;
  m_technoTrendStructure.p13=OnLscGetNextBuffer;
  m_technoTrendStructure.p14=OnLscTransmitBuffer;

  BDAAPIOPENCI openCI;
  TYPE_RET_VAL result;
  openCI=(BDAAPIOPENCI)GetProcAddress(m_dll,"_bdaapiOpenCI@116");
  if (openCI!=NULL)
  {
    result=openCI(m_hBdaApi, m_technoTrendStructure);
    if (result != RET_SUCCESS)
    {
      LogDebug("Technotrend: unable to open the CI:%d",result);
      m_deviceType=UNKNOWN;  
      return S_OK;
    }
    LogDebug("Technotrend: bdaapiOpenCI succeeded");

    BYTE v1,v2,v3,v4;
    BDAAPIGETDRVVERSION getDrvVersion=(BDAAPIGETDRVVERSION)GetProcAddress(m_dll,"_bdaapiGetDrvVersion@20");
    if (getDrvVersion!=NULL)
    {
      result=getDrvVersion(m_hBdaApi,&v1,&v2,&v3,&v4);
      if (result != RET_SUCCESS)
      {
        LogDebug("Technotrend: bdaapiGetDrvVersion failed %d",result);
      }
      else
      {
        LogDebug("Technotrend: initalized id:%x, driver version:%d.%d.%d.%d",deviceId,v1,v2,v3,v4);
      }
    }
    else
    {
      LogDebug("Technotrend: unable to get proc adress of bdaapiGetDrvVersion");
    }
  }
  else
  {
    LogDebug("Technotrend: unable to get proc adress of bdaapiOpenCI");
  }


  return S_OK;
}

//**************************************************************************************************
//* IsTechnoTrend()
//* Returns whether the tuner is a technotrend device or not
//**************************************************************************************************
STDMETHODIMP CTechnotrend::IsTechnoTrend( BOOL* yesNo)
{
  *yesNo= (m_hBdaApi != INVALID_HANDLE_VALUE);
  return S_OK;
}

//**************************************************************************************************
//* IsCamReady()
//* Returns whether the CAM is ready
//**************************************************************************************************
STDMETHODIMP CTechnotrend::IsCamReady( BOOL* yesNo)
{
  *yesNo=FALSE;
  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK||m_slotStatus==CI_SLOT_DBG_MSG)
  {
    *yesNo=TRUE;
  }
  return S_OK;
}

//**************************************************************************************************
//* SetAntennaPower()
//* Turn on/off the DVB-T antenna on USB devices
//**************************************************************************************************
STDMETHODIMP CTechnotrend::SetAntennaPower( BOOL onOff)
{
  if (m_deviceType==USB_2_PINNACLE || m_deviceType==USB_2)
  {
    BDAAPISETDVBTANTPWR setPower=(BDAAPISETDVBTANTPWR)GetProcAddress(m_dll,"_bdaapiSetDVBTAntPwr@8");
    if (setPower!=NULL)
    {
      HRESULT hr=setPower(m_hBdaApi,onOff);
      LogDebug("Technotrend: enable antenna power:%d %x",onOff,hr);
    }
    else
    {
      LogDebug("Technotrend: unable to get proc adress of bdaapiSetDVBTAntPwr");
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
STDMETHODIMP CTechnotrend::SetDisEqc(BYTE* diseqc, BYTE len, BYTE Repeat,BYTE Toneburst,int ePolarity)
{
  char buffer[129];
  strcpy(buffer,"");
  for (int i=0; i < (int)len;++i)
  {
    char tmp[30];
    sprintf(tmp,"0x%02.2x ", (int)diseqc[i]);
    strcat(buffer,tmp);
  }
  LogDebug("TechnoTrend:SetDiseqc:%s repeat:%d tone:%d pol:%d", buffer,Repeat,Toneburst,ePolarity);
  BDAAPISETDISEQCMSG setDisEqc=(BDAAPISETDISEQCMSG)GetProcAddress(m_dll,"_bdaapiSetDiSEqCMsg@24");
  if (setDisEqc!=NULL)
  {
    TYPE_RET_VAL result=setDisEqc(m_hBdaApi,diseqc,len,Repeat,Toneburst,(Polarisation)ePolarity);
    LogDebug("TechnoTrend:SetDiseqc:%d", result);
  }
  else
  {
    LogDebug("Technotrend: unable to get proc adress of bdaapiSetDiSEqCMsg");
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
STDMETHODIMP CTechnotrend::DescrambleMultiple(WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded)
{
  TYPE_RET_VAL hr;
  *succeeded=FALSE;
  BOOL enabled=FALSE;
  m_ciStatus=-1;
  LogDebug("TechnoTrend: Get CI Slot State");
  BDAAPICIGETSLOTSTATUS getSlotState=(BDAAPICIGETSLOTSTATUS)GetProcAddress(m_dll,"_bdaapiCIGetSlotStatus@8");
  if (getSlotState!=NULL)
  {
    hr=getSlotState(m_hBdaApi,0);
    if (hr!=RET_SUCCESS)
    {
      LogDebug("Technotrend: bdaapiCIGetSlotStatus failed:%d", hr);
    }
  }
  else
  {
    LogDebug("Technotrend: unable to get proc adress of bdaapiCIGetSlotStatus");
  }
  LogDebug("TechnoTrend: DescrambleMultiple:(%d)",NrOfOfPrograms);
  for (int i=0; i < NrOfOfPrograms;++i)
  {
    LogDebug("TechnoTrend: DescrambleMultiple: serviceId:%d", pNrs[i]);
  }
  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK||m_slotStatus==CI_SLOT_DBG_MSG )
  {

    BDAAPICIMULTIDECODE readPSI=(BDAAPICIMULTIDECODE)GetProcAddress(m_dll,"_bdaapiCIMultiDecode@12");
    if (readPSI!=NULL)
    {
      hr = readPSI(m_hBdaApi, pNrs,NrOfOfPrograms);
      if (hr==RET_SUCCESS)
      {
        if (m_ciStatus==1)
        {
          *succeeded=TRUE;
          LogDebug("TechnoTrend: services decoded:%x %d",hr,m_ciStatus);

        }
        else
        {
          *succeeded=TRUE;
          LogDebug("TechnoTrend: services decoded:%x %d",hr,m_ciStatus);
        }
      }
      else
      {
        LogDebug("TechnoTrend: services not decoded:%x",hr);
      }    
    }
    else
    {
      LogDebug("Technotrend: unable to get proc adress of bdaapiCIMultiDecode");
    }
  }
  else if (m_slotStatus==CI_SLOT_UNKNOWN_STATE || m_slotStatus==CI_SLOT_EMPTY)
  {
    //no CAM inserted
    LogDebug("TechnoTrend: no cam detected:%d",m_slotStatus);
    *succeeded=TRUE;
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
STDMETHODIMP CTechnotrend::DescrambleService( BYTE* pmt, int PMTLength,BOOL* succeeded)
{
  TYPE_RET_VAL hr;
  *succeeded=FALSE;
  BOOL enabled=FALSE;
  m_ciStatus=-1;
  LogDebug("TechnoTrend: Get CI Slot State");
  BDAAPICIGETSLOTSTATUS getSlotState=(BDAAPICIGETSLOTSTATUS)GetProcAddress(m_dll,"_bdaapiCIGetSlotStatus@8");
  if (getSlotState!=NULL)
  {
    hr=getSlotState(m_hBdaApi,0);
    if (hr!=RET_SUCCESS)
    {
      LogDebug("Technotrend: bdaapiCIGetSlotStatus failed:%d", hr);
    }
  }
  else
  {
    LogDebug("Technotrend: unable to get proc adress of bdaapiCIGetSlotStatus");
  }
  LogDebug("TechnoTrend: DescrambleService:(%d)",m_slotStatus);
  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK||m_slotStatus==CI_SLOT_DBG_MSG )
  {
    BDAAPICIREADPSIFASTWITHPMT readPSI=(BDAAPICIREADPSIFASTWITHPMT)GetProcAddress(m_dll,"_bdaapiCIReadPSIFastWithPMT@12");
    //BDAAPICIREADPSIFASTDRVDEMUX readPSI=(BDAAPICIREADPSIFASTDRVDEMUX)GetProcAddress(m_dll,"_bdaapiCIReadPSIFastDrvDemux@8");
    if (readPSI!=NULL)
    {
      hr = readPSI(m_hBdaApi, pmt,PMTLength);
      if (hr==RET_SUCCESS)
      {
        if (m_ciStatus==1)
        {
          *succeeded=TRUE;
          LogDebug("TechnoTrend: service decoded:%x %d",hr,m_ciStatus);

        }
        else
        {
          *succeeded=TRUE;
          LogDebug("TechnoTrend: service decoded:%x %d",hr,m_ciStatus);
        }
      }
      else
      {
        LogDebug("TechnoTrend: service not decoded:%x",hr);
      }    
    }
    else
    {
      LogDebug("Technotrend: unable to get proc adress of bdaapiCIReadPSIFastDrvDemux");
    }
  }
  else if (m_slotStatus==CI_SLOT_UNKNOWN_STATE || m_slotStatus==CI_SLOT_EMPTY)
  {
    //no CAM inserted
    LogDebug("TechnoTrend: no cam detected:%d",m_slotStatus);
    *succeeded=TRUE;
  }

  return S_OK;
}
//**************************************************************************************************
//* GetDeviceID()
//* Finds the technotrend device Id for the tuner filter
//* returns true if device id is found otherwise false
//**************************************************************************************************
bool CTechnotrend::GetDeviceID(IBaseFilter* tunerFilter, UINT& deviceId)
{
  bool success=false;
  IEnumPins* pEnumPins;
  tunerFilter->EnumPins(&pEnumPins);
  ULONG fetched;
  IPin* pins[2];
  while (SUCCEEDED( pEnumPins->Next(1,&pins[0],&fetched)))
  {
    if (fetched!=1) break;
    PIN_DIRECTION pinDirection;
    pins[0]->QueryDirection(&pinDirection);
    if (pinDirection!= PINDIR_OUTPUT) 
    {
      pins[0]->Release();
      continue;
    }

    IKsPin* pKsPin;
    if (SUCCEEDED( pins[0]->QueryInterface(IID_IKsPin, (void **)&pKsPin)))
    {    
      KSMULTIPLE_ITEM *pmi;
      HRESULT hr=pKsPin->KsQueryMediums(&pmi);
      pKsPin->Release();
      if (SUCCEEDED(hr))
      {
        // Use pointer arithmetic to reference the first medium structure.
        REGPINMEDIUM *pTemp = (REGPINMEDIUM*)(pmi + 1);
        for (ULONG i = 0; i < pmi->Count; i++, pTemp++) 
        {
          success=true;
          deviceId=pTemp->dw1;
          break;
        }
        CoTaskMemFree(pmi);
      }
    }
    pins[0]->Release();
  }
  pEnumPins->Release();
  return success;

}

//**************************************************************************************************
//* OnCaChange() callback from driver when CA state changes
//**************************************************************************************************
void CTechnotrend::OnCaChange(BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus)
{
  try
  {
    LogDebug("$ OnCaChange slot:%d reply:%d status:%d",nSlot,nReplyTag,wStatus);
    switch(nReplyTag)	
    {
    case CI_PSI_COMPLETE:
      LogDebug("$ CI: ### Number of programs : %04d",wStatus);
      break;

    case CI_MODULE_READY:
      LogDebug("$ CI: CI_MODULE_READY in OnCAStatus not supported");
      break;
    case CI_SWITCH_PRG_REPLY:
      {
        switch(wStatus)
        {
        case ERR_INVALID_DATA:
          LogDebug("$ CI: ERROR::SetProgram failed !!! (invalid PNR)");
          break;
        case ERR_NO_CA_RESOURCE:
          LogDebug("$ CI: ERROR::SetProgram failed !!! (no CA resource available)");
          break;
        case ERR_NONE:
          LogDebug("$ CI:    SetProgram OK");
          m_ciStatus=1;
          break;
        default:
          break;
        }
      }
      break;
    default:
      break;
    }
  }
  catch(...)
  {
    LogDebug("Technotrend: OnCaChange() exception");  
  }
}

//**************************************************************************************************
//* OnSlotChange() callback from driver when CI state changes
//**************************************************************************************************
void CTechnotrend::OnSlotChange(BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
{
  try
  {
    if (nStatus==0) LogDebug("Technotrend: slot:%d empty",nSlot);  
    else if (nStatus==1) LogDebug("Technotrend: slot:%d module inserted",nSlot);
    else if (nStatus==2) LogDebug("Technotrend: slot:%d module ok",nSlot);
    else if (nStatus==3) LogDebug("Technotrend: slot:%d ca ok",nSlot);
    else if (nStatus==4) LogDebug("Technotrend: slot:%d dbg msg",nSlot);
    else  LogDebug("Technotrend: slot:%d unknown state:%x",nSlot,nStatus);
    m_slotStatus=nStatus;

    if (csInfo!=NULL)
    {
      LogDebug("Technotrend:    CI status:%d ",csInfo->nStatus);
      if (csInfo->pMenuTitleString!=NULL)
        LogDebug("Technotrend:    CI text  :%s ",csInfo->pMenuTitleString);
      for (int i=0; i < csInfo->wNoOfCaSystemIDs;++i)
      {
        LogDebug("Technotrend:      ca system id  :%x ",csInfo->pCaSystemIDs[i]);
      }
    }
  }
  catch(...)
  {
    LogDebug("Technotrend: OnSlotChange() exception");  
  }
}
