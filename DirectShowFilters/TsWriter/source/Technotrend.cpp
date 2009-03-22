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
typedef TYPE_RET_VAL (*BDAAPIOPENCI)(HANDLE hOpen,TS_CiCbFcnPointer CbFuncPointer);
typedef TYPE_RET_VAL (*BDAAPIGETDRVVERSION)(HANDLE hOpen, BYTE *v1, BYTE *v2,BYTE *v3, BYTE *v4);
typedef TYPE_RET_VAL (*BDAAPICLOSECI)(HANDLE   hOpen);
typedef void         (*BDAAPICLOSE)(HANDLE      hOpen);
typedef TYPE_RET_VAL (*BDAAPISETDVBTANTPWR)(HANDLE hOpen,BOOL   bAntPwrOnOff);
typedef TYPE_RET_VAL (*BDAAPISETDISEQCMSG)(HANDLE        hOpen,BYTE         *pData,BYTE          Bytes,BYTE          Repeat,BYTE          Toneburst,Polarisation  ePolarity);
typedef TYPE_RET_VAL (*BDAAPICIGETSLOTSTATUS)(HANDLE hOpen, BYTE nSlot);
typedef TYPE_RET_VAL (*BDAAPICIREADPSIFASTWITHPMT)(HANDLE hOpen,BYTE   *pPMT,WORD   wLength);
typedef TYPE_RET_VAL (*BDAAPICIREADPSIFASTDRVDEMUX)(HANDLE hOpen,WORD   PNR);
typedef HANDLE       (*BDAAPIOPENHWIDX) (DEVICE_CAT DevType,UINT        uiDevID);
typedef TYPE_RET_VAL (*BDAAPICIMULTIDECODE)(HANDLE hOpen,WORD  *PNR,int    NrOfPnrs);
// Info functions
typedef TYPE_RET_VAL (*BDAAPIGETDEVNAMEANDFETYPE) (HANDLE hOpen, pTS_FilterNames FilterNames);
typedef TYPE_RET_VAL (*BDAAPIGETPRODUCTSELLERID) (HANDLE hOpen, PRODUCT_SELLER &ps);
//CI menu functions
typedef TYPE_RET_VAL (*BDAAPICIENTERMODULEMENU) (HANDLE hOpen, BYTE nSlot);
typedef TYPE_RET_VAL (*BDAAPICIMENUANSWER) (HANDLE hOpen, BYTE nSlot, BYTE Selection);

//**************************************************************************************************
//* ctor
//**************************************************************************************************
CTechnotrend::CTechnotrend(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("MpTsTechnoTrend"), pUnk)
{
  m_hBdaApi         = INVALID_HANDLE_VALUE;
  m_deviceType      = UNKNOWN;
	m_ciSlotAvailable = 0;
  m_ciStatus        = -1;
	m_waitTimeout     = 0;
  m_dll             = NULL;
  m_verboseLogging  = false; // Log extended information?
}

//**************************************************************************************************
//* dtor
//* close device
//**************************************************************************************************
CTechnotrend::~CTechnotrend(void)
{
  if (m_dll!=NULL && m_hBdaApi != INVALID_HANDLE_VALUE)
  {
    BDAAPICLOSECI closeCI=(BDAAPICLOSECI)GetProcAddress(m_dll,"bdaapiCloseCI");
    if (closeCI!=NULL)
    {
      closeCI(m_hBdaApi);
    }
    else
    {
      LogDebug("TechnoTrend: unable to get proc adress of bdaapiCloseCI");
    }

    BDAAPICLOSE closeapi=(BDAAPICLOSE)GetProcAddress(m_dll,"bdaapiClose");
    if (closeapi!=NULL)
    {
      closeapi(m_hBdaApi);
    }
    else
    {
      LogDebug("TechnoTrend: unable to get proc adress of bdaapiClose");
    }
  }
  if (m_dll!=NULL)
  {
    FreeTechnotrendLibrary();
  }
}

void CTechnotrend::FreeTechnotrendLibrary(){
  if (m_verboseLogging) LogDebug("Releasing TechnoTrend library");
  FreeLibrary(m_dll);
  m_dll=NULL;
}

//**************************************************************************************************
//* callback from driver when the CI slot state changes
//**************************************************************************************************
static void OnSlotStatusCallback(PVOID Context,BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
{
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
  technoTrend->OnSlotChange( nSlot, nStatus, csInfo);
}

//**************************************************************************************************
//* callback from driver when the CA state changes
//**************************************************************************************************
static void OnCaStatusCallback(PVOID Context,BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus)
{
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
  technoTrend->OnCaChange(nSlot,nReplyTag,wStatus);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnDisplayString(PVOID Context,BYTE  nSlot,char* pString,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayString slot:%d %s", nSlot,pString);
}

//**************************************************************************************************
//* callback from driver to display the CI menu
//**************************************************************************************************
/*static void OnDisplayMenu(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayMenu slot:%d", nSlot);
}*/

//**************************************************************************************************
//* callback from driver to display the CI menu
//**************************************************************************************************
static void OnDisplayMenuCallback(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayMenu slot:%d", nSlot);
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
	technoTrend->OnDisplayMenu(nSlot,wItems,pStringArray,wLength);
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
static void OnDisplayList(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
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
      LogDebug("TechnoTrend  %s",szBuf);
      c=0;
    }
  }
  delete[] szBuf;

}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnSwitchOsdOff(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:CI_OnSwitchOsdOff slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnInputRequest(PVOID Context,BYTE  nSlot,BOOL  bBlindAnswer,BYTE  nExpectedLength, DWORD dwKeyMask)
{
  LogDebug("TechnoTrend:CI_OnInputRequest slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscSetDescriptor(PVOID Context,BYTE  nSlot,TYPE_CONNECT_DESCR* pDescriptor)
{
  LogDebug("TechnoTrend:OnLscSetDescriptor slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscConnect(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscConnect slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscDisconnect(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscDisconnect slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscSetParams(PVOID Context,BYTE  nSlot,BYTE  BufferSize,BYTE  Timeout10Ms)
{
  LogDebug("TechnoTrend:OnLscSetParams slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscEnquireStatus(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscEnquireStatus slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscGetNextBuffer(PVOID Context,BYTE  nSlot,BYTE  PhaseID)
{
  LogDebug("TechnoTrend:OnLscGetNextBuffer slot:%d", nSlot);
}

//**************************************************************************************************
//* callback from driver 
//**************************************************************************************************
static void OnLscTransmitBuffer(PVOID Context,BYTE  nSlot,BYTE  PhaseID,BYTE* pData,WORD  nLength)
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
    if (m_verboseLogging) LogDebug("TechnoTrend:loaded ttBdaDrvApi_Dll.dll");
  }

  FILTER_INFO info;
  if (!SUCCEEDED(tunerFilter->QueryFilterInfo(&info))) {
	  FreeTechnotrendLibrary();
	  return S_OK;
  }
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
  if (m_deviceType==UNKNOWN) {
	  FreeTechnotrendLibrary();
	  return S_OK;
  }

  if (m_verboseLogging) LogDebug("TechnoTrend: card detected type:%d",m_deviceType);
  UINT deviceId;
  if (!GetDeviceID(tunerFilter, deviceId) )
  {
    if (m_verboseLogging) LogDebug("TechnoTrend: unable to determine the device id");
    m_deviceType=UNKNOWN;  
	  FreeTechnotrendLibrary();
    return S_OK;
  }
  BDAAPIOPENHWIDX openHwIdx= (BDAAPIOPENHWIDX)GetProcAddress(m_dll,"bdaapiOpenHWIdx");
  if (openHwIdx!=NULL)
  {
    m_hBdaApi = openHwIdx(m_deviceType, deviceId);
    if (m_hBdaApi == INVALID_HANDLE_VALUE) 
    {
      LogDebug("TechnoTrend: unable to open the device");
	    FreeTechnotrendLibrary();
      return S_OK;
    }
    if (m_verboseLogging) LogDebug("TechnoTrend: OpenHWIdx succeeded");
  }
  else
  {
    if (m_verboseLogging) LogDebug("TechnoTrend: unable to get proc adress of bdaapiOpenHWIdx");
	  FreeTechnotrendLibrary();
    return S_OK;
  }

	GetDrvVersion(deviceId);	// query some information from driver before CI open
	GetDevNameAndFEType(); 
	if (m_verboseLogging) GetProductSellerID();


  memset(&m_technoTrendStructure,0,sizeof(m_technoTrendStructure));
  m_technoTrendStructure.p01Context=m_technoTrendStructure.p02Context=m_technoTrendStructure.p03Context=m_technoTrendStructure.p04Context=this;
  m_technoTrendStructure.p05Context=m_technoTrendStructure.p06Context=m_technoTrendStructure.p07Context=m_technoTrendStructure.p08Context=this;
  m_technoTrendStructure.p09Context=m_technoTrendStructure.p10Context=m_technoTrendStructure.p11Context=m_technoTrendStructure.p12Context=this;
  m_technoTrendStructure.p13Context=m_technoTrendStructure.p14Context=this;
  m_technoTrendStructure.p01=OnSlotStatusCallback;
  m_technoTrendStructure.p02=OnCaStatusCallback;
  m_technoTrendStructure.p03=OnDisplayString;
  m_technoTrendStructure.p04=OnDisplayMenuCallback;
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
  openCI=(BDAAPIOPENCI)GetProcAddress(m_dll,"bdaapiOpenCI");
  if (openCI!=NULL)
  {
    result=openCI(m_hBdaApi, m_technoTrendStructure);
    if (result != RET_SUCCESS)
    {
      LogDebug("TechnoTrend: no CI detected: %d",result);
      m_ciSlotAvailable=0;
			// don't set to unknown; this would lead to never execute SetAntennaPower or other functions checking m_deviceType
			//m_deviceType=UNKNOWN;  
      return S_OK;
    }
		m_ciSlotAvailable=1;
    LogDebug("TechnoTrend: OpenCI succeeded");
		// query status on init; try to avoid first time init problem
		GetCISlotStatus();
  }
  else
  {
    if (m_verboseLogging) LogDebug("TechnoTrend: unable to get proc adress of bdaapiOpenCI");
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
  if (m_ciSlotAvailable == 1 && 
      (
        (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK) || // usual internal card/ci
        (m_deviceType==USB_2 && m_slotStatus==CI_SLOT_MODULE_INSERTED)      // usb box TT 3650 CT only tells "module inserted"
       )
     )
  {
    *yesNo=TRUE;
  }
	if (m_verboseLogging) LogDebug("TechnoTrend: IsCamReady: %d",*yesNo);
  return S_OK;
}

//**************************************************************************************************
//* IsCamPresent()
//* Returns whether the CAM is inserted
//**************************************************************************************************
STDMETHODIMP CTechnotrend::IsCamPresent( BOOL* yesNo)
{
  *yesNo=TRUE;
  if (m_ciSlotAvailable == 0 || m_slotStatus==CI_SLOT_EMPTY || m_slotStatus==CI_SLOT_UNKNOWN_STATE)
  {
    *yesNo=FALSE;
  }
	if (m_verboseLogging) LogDebug("TechnoTrend: IsCamPresent: %d",*yesNo);
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
    BDAAPISETDVBTANTPWR setPower=(BDAAPISETDVBTANTPWR)GetProcAddress(m_dll,"bdaapiSetDVBTAntPwr");
    if (setPower!=NULL)
    {
      HRESULT hr=setPower(m_hBdaApi,onOff);
      LogDebug("TechnoTrend: enable antenna power:%d %x",onOff,hr);
    }
    else
    {
      LogDebug("TechnoTrend: unable to get proc adress of bdaapiSetDVBTAntPwr");
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
  BDAAPISETDISEQCMSG setDisEqc=(BDAAPISETDISEQCMSG)GetProcAddress(m_dll,"bdaapiSetDiSEqCMsg");
  if (setDisEqc!=NULL)
  {
    TYPE_RET_VAL result=setDisEqc(m_hBdaApi,diseqc,len,Repeat,Toneburst,(Polarisation)ePolarity);
    LogDebug("TechnoTrend:SetDiseqc:%d", result);
  }
  else
  {
    LogDebug("TechnoTrend: unable to get proc adress of bdaapiSetDiSEqCMsg");
  }

  return S_OK;
}

//**************************************************************************************************
//* GetCISlotStatus()
//* Query state of CI slot;
//* result is set asynchron in callback function from driver; time gap can be 0.2 sec
//**************************************************************************************************
STDMETHODIMP CTechnotrend::GetCISlotStatus()
{
  TYPE_RET_VAL hr;
  m_ciStatus=-1;
  if (m_verboseLogging) LogDebug("TechnoTrend: Get CI Slot State");
  BDAAPICIGETSLOTSTATUS getSlotState=(BDAAPICIGETSLOTSTATUS)GetProcAddress(m_dll,"bdaapiCIGetSlotStatus");
  if (getSlotState!=NULL)
  {
    hr=getSlotState(m_hBdaApi,0);
    if (hr!=RET_SUCCESS)
    {
      LogDebug("TechnoTrend: bdaapiCIGetSlotStatus failed:%d", hr);
    }
  }
  else
  {
    LogDebug("TechnoTrend: unable to get proc adress of bdaapiCIGetSlotStatus");
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

	// if OpenCI failed, there's no CI ergo no CAM
	if (m_ciSlotAvailable==0) {
		if (m_verboseLogging) LogDebug("TechnoTrend: DescrambleMultiple: no CI present");
		*succeeded=TRUE;
		return S_OK;
	}

	GetCISlotStatus();

	LogDebug("TechnoTrend: DescrambleMultiple:(%d)",NrOfOfPrograms);
  for (int i=0; i < NrOfOfPrograms;++i)
  {
    LogDebug("TechnoTrend: DescrambleMultiple: serviceId:%d", pNrs[i]);
  }

	// Workaround for first time initialisation of ci slot
	// Problem: may cause slow down of channel change, when no CI is available
	int iRetryLoop=0;
	while (m_waitTimeout==0 && m_slotStatus==CI_SLOT_UNKNOWN_STATE && iRetryLoop<=10) {
		//unknown status
		if (m_verboseLogging) LogDebug("TechnoTrend: Wait for CI slot status, current slot status: %x; try %d; Sleep 100", m_slotStatus, iRetryLoop);
		iRetryLoop++; 
		Sleep(100);
	}	// loop

  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK || (m_deviceType==USB_2 && m_slotStatus==CI_SLOT_MODULE_INSERTED)) // || m_slotStatus==CI_SLOT_DBG_MSG)
  {
    BDAAPICIMULTIDECODE readPSI=(BDAAPICIMULTIDECODE)GetProcAddress(m_dll,"bdaapiCIMultiDecode");
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
          //*succeeded=TRUE;
				  *succeeded=FALSE;
					LogDebug("TechnoTrend: services not decoded:%x ciStatus: %d",hr,m_ciStatus);
        }
      }
      else
      {
        LogDebug("TechnoTrend: services not decoded:%x",hr);
      }    
    }
    else
    {
      LogDebug("TechnoTrend: unable to get proc adress of bdaapiCIMultiDecode");
    }
  }
  else if (m_slotStatus==CI_SLOT_UNKNOWN_STATE || m_slotStatus==CI_SLOT_EMPTY)
  {
		//still no valid state? don't try next time!
		m_waitTimeout=1;
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

		// if OpenCI failed, there's no CI ergo no CAM
	if (m_ciSlotAvailable==0) {
		if (m_verboseLogging) LogDebug("TechnoTrend: DescrambleService: no CI present");
		*succeeded=TRUE;
		return S_OK;
	}

	GetCISlotStatus();

	LogDebug("TechnoTrend: DescrambleService:(%d)",m_slotStatus);
  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK) // || m_slotStatus==CI_SLOT_DBG_MSG)
  {
    BDAAPICIREADPSIFASTWITHPMT readPSI=(BDAAPICIREADPSIFASTWITHPMT)GetProcAddress(m_dll,"bdaapiCIReadPSIFastWithPMT");
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
          *succeeded=FALSE;
          LogDebug("TechnoTrend: service not decoded:%x %d",hr,m_ciStatus);
        }
      }
      else
      {
        LogDebug("TechnoTrend: service not decoded:%x",hr);
      }    
    }
    else
    {
      LogDebug("TechnoTrend: unable to get proc adress of bdaapiCIReadPSIFastDrvDemux");
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
          m_ciStatus=-1; // not ready
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
    LogDebug("TechnoTrend: OnCaChange() exception");  
  }
}

//**************************************************************************************************
//* OnSlotChange() callback from driver when CI state changes
//**************************************************************************************************
void CTechnotrend::OnSlotChange(BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
{
  try
  {
    if (nStatus==CI_SLOT_EMPTY) LogDebug("TechnoTrend: slot:%d empty",nSlot);  
		else if (nStatus==CI_SLOT_MODULE_INSERTED) LogDebug("TechnoTrend: slot:%d module inserted",nSlot);
		else if (nStatus==CI_SLOT_MODULE_OK) LogDebug("TechnoTrend: slot:%d module ok",nSlot);
		else if (nStatus==CI_SLOT_CA_OK) LogDebug("TechnoTrend: slot:%d ca ok",nSlot);
		else if (nStatus==CI_SLOT_DBG_MSG) LogDebug("TechnoTrend: slot:%d dbg msg",nSlot);
    else  LogDebug("TechnoTrend: slot:%d unknown state:%x",nSlot,nStatus);
    m_slotStatus=nStatus;

    if (csInfo!=NULL)
    {
      LogDebug("TechnoTrend:    CI status:%d ",csInfo->nStatus);
      if (csInfo->pMenuTitleString!=NULL)
        LogDebug("TechnoTrend:    CI text  :%s ",csInfo->pMenuTitleString);
      for (int i=0; i < csInfo->wNoOfCaSystemIDs;++i)
      {
        LogDebug("TechnoTrend:      ca system id  :%x ",csInfo->pCaSystemIDs[i]);
      }
    }
  }
  catch(...)
  {
    LogDebug("TechnoTrend: OnSlotChange() exception");  
  }
}

//**************************************************************************************************
//* GetDrvVersion() [INFO] Shows the device driver version i.e. "5.0.0.12"
//**************************************************************************************************
STDMETHODIMP CTechnotrend::GetDrvVersion(UINT deviceId) {
  TYPE_RET_VAL hr;
  BYTE v1,v2,v3,v4;

  BDAAPIGETDRVVERSION getDrvVersion=(BDAAPIGETDRVVERSION)GetProcAddress(m_dll,"bdaapiGetDrvVersion");
  if (getDrvVersion!=NULL)
  {
    hr=getDrvVersion(m_hBdaApi,&v1,&v2,&v3,&v4);
    if (hr != RET_SUCCESS)
    {
      LogDebug("TechnoTrend: bdaapiGetDrvVersion failed %d",hr);
    }
    else
    {
      LogDebug("TechnoTrend: initalized id:%x, driver version:%d.%d.%d.%d",deviceId,v1,v2,v3,v4);
    }
  }
  else
  {
    LogDebug("TechnoTrend: unable to get proc adress of bdaapiGetDrvVersion");
  }
	return S_OK;
}



//**************************************************************************************************
//* GetDevNameAndFEType() [INFO] Shows filter names and frontend-type
//**************************************************************************************************
STDMETHODIMP CTechnotrend::GetDevNameAndFEType() {
	TS_FilterNames tsFilters;
  TYPE_RET_VAL hr;

    BDAAPIGETDEVNAMEANDFETYPE getDevName=(BDAAPIGETDEVNAMEANDFETYPE)GetProcAddress(m_dll,"bdaapiGetDevNameAndFEType");
    if (getDevName!=NULL)
    {
				hr=getDevName(m_hBdaApi, &tsFilters);
				if (hr!=RET_SUCCESS)
				{
					LogDebug("TechnoTrend: bdaapiGetDevNameAndFEType failed:%d", hr);
				} else {
					if (tsFilters.szProductName)                              LogDebug("TechnoTrend: ProductName: %s", tsFilters.szProductName);
					if (m_verboseLogging && tsFilters.szTunerFilterName)      LogDebug("TechnoTrend: TunerFilterName: %s", tsFilters.szTunerFilterName);
					if (m_verboseLogging && tsFilters.szTunerFilterName2)     LogDebug("TechnoTrend: TunerFilterName2: %s", tsFilters.szTunerFilterName2);
					if (m_verboseLogging && tsFilters.szCaptureFilterName)    LogDebug("TechnoTrend: CaptureFilterName: %s", tsFilters.szCaptureFilterName);
					if (m_verboseLogging && tsFilters.szAnlgTunerFilterName)  LogDebug("TechnoTrend: AnlgTunerFilterName: %s", tsFilters.szAnlgTunerFilterName);
					if (m_verboseLogging && tsFilters.szAnlgCaptureFilterName)LogDebug("TechnoTrend: AnlgCaptureFilterName: %s", tsFilters.szAnlgCaptureFilterName);
					if (m_verboseLogging) 
            switch ( tsFilters.FeType )
					  {
							case TYPE_FE_UNKNOWN:
									LogDebug("TechnoTrend: Frontend type unknown");
									break;
							case TYPE_FE_DVB_C:
									LogDebug("TechnoTrend: Frontend type DVB-C");
									break;
							case TYPE_FE_DVB_S:
									LogDebug("TechnoTrend: Frontend type DVB-S");
									break;
							case TYPE_FE_DVB_S2:
									LogDebug("TechnoTrend: Frontend type DVB-S2");
									break;
							case TYPE_FE_DVB_T:
									LogDebug("TechnoTrend: Frontend type DVB-T");
									break;
							case TYPE_FE_DVB_CT:
									LogDebug(tsFilters.szTunerFilterName2);
									LogDebug("TechnoTrend: Frontend type DVB-C / DVB-T (Hybrid)");
									break;
							default:
									LogDebug("TechnoTrend: Frontend type unknown.");
									break;
            }
				}
		}
  return S_OK;
}

//**************************************************************************************************
//* GetProductSellerID() [INFO] Shows wheter card is from Technotrend, Technisat 
//**************************************************************************************************
STDMETHODIMP CTechnotrend::GetProductSellerID() {
	PRODUCT_SELLER eProductSeller;
  TYPE_RET_VAL hr;

  BDAAPIGETPRODUCTSELLERID getProductSeller=(BDAAPIGETPRODUCTSELLERID)GetProcAddress(m_dll,"bdaapiGetProductSellerID");
    if (getProductSeller!=NULL)
    {
				hr=getProductSeller(m_hBdaApi, eProductSeller);
				if (hr!=RET_SUCCESS)
				{
					LogDebug("TechnoTrend: bdaapiGetProductSellerID failed:%d", hr);
				} else {
					//LogDebug("TechnoTrend: bdaapiGetProductSellerID ok.");
					switch (eProductSeller) {
						case PS_UNKNOWN: 
							LogDebug("TechnoTrend: Unknown seller");
							break;
						case PS_TECHNOTREND: 
							LogDebug("TechnoTrend: ProductSeller Technotrend");
							break;
						case PS_TECHNISAT: 
							LogDebug("TechnoTrend: ProductSeller Technisat");
							break;
					}
				}
		}
  return S_OK;
}


//**************************************************************************************************
//* EnterModuleMenu() [CI MENU] Enter the CI Menu 
//**************************************************************************************************
STDMETHODIMP CTechnotrend::EnterModuleMenu () {
  TYPE_RET_VAL hr;

  BDAAPICIENTERMODULEMENU enterModuleMenu=(BDAAPICIENTERMODULEMENU)GetProcAddress(m_dll,"bdaapiCIEnterModuleMenu");
  if (enterModuleMenu!=NULL)
  {
			hr=enterModuleMenu(m_hBdaApi, 0);
			if (hr!=RET_SUCCESS)
			{
				LogDebug("TechnoTrend: bdaapiCIEnterModuleMenu failed:%d", hr);
			} else {
				LogDebug("TechnoTrend: bdaapiCIEnterModuleMenu ok.");
			}
	}
	else
  {
    LogDebug("TechnoTrend: unable to get proc adress of bdaapiCIEnterModuleMenu");
  }
  return S_OK;
}

//**************************************************************************************************
//* OnDisplayMenu() [CI MENU] callback from driver when entering the CI Menu
//**************************************************************************************************
void CTechnotrend::OnDisplayMenu(BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  try
  {
    LogDebug("TechnoTrend: OnDisplayMenu ");

    if (pStringArray!=NULL)
    {
      for (int i=0; i < wItems; ++i)
      {
				LogDebug("TechnoTrend: %d: %s ",i, pStringArray[i]);
      }
    }
  }
  catch(...)
  {
    LogDebug("TechnoTrend: OnDisplayMenu() exception");  
  }
}

//**************************************************************************************************
//* SendMenuAnswer() [CI MENU] Send answer to CI Menu 
//**************************************************************************************************
STDMETHODIMP CTechnotrend::SendMenuAnswer  (BYTE Selection)
{
  TYPE_RET_VAL hr;
  BDAAPICIMENUANSWER sendMenuAnswer=(BDAAPICIMENUANSWER)GetProcAddress(m_dll,"bdaapiCIMenuAnswer");
  if (sendMenuAnswer!=NULL)
  {
			hr=sendMenuAnswer(m_hBdaApi, 0, Selection);
			if (hr!=RET_SUCCESS)
			{
				LogDebug("TechnoTrend: bdaapiCIMenuAnswer  failed:%d", hr);
			} else {
				LogDebug("TechnoTrend: bdaapiCIMenuAnswer  ok.");
			}
	}
	else
  {
    LogDebug("TechnoTrend: unable to get proc adress of bdaapiCIMenuAnswer");
  }
  return S_OK;
}

