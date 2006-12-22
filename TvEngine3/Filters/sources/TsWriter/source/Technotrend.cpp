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
#include "Technotrend.h"
#include "bdaapi_cimsg.h"


#define LBDG2_NAME_C_TUNER           L"TechnoTrend BDA/DVB-C Tuner"
#define LBDG2_NAME_S_TUNER           L"TechnoTrend BDA/DVB-S Tuner"
#define LBDG2_NAME_T_TUNER           L"TechnoTrend BDA/DVB-T Tuner"
#define LBUDGET3NAME_TUNER           L"TTHybridTV BDA DVBT Tuner"
#define LBUDGET3NAME_TUNER_ANLG      L"TTHybridTV BDA Analog TV Tuner"
#define LUSB2BDA_DVB_NAME_C_TUNER    L"USB 2.0 BDA DVB-C Tuner"
#define LUSB2BDA_DVB_NAME_S_TUNER    L"USB 2.0 BDA DVB-S Tuner"
#define LUSB2BDA_DVB_NAME_T_TUNER    L"USB 2.0 BDA DVB-T Tuner"
#define LUSB2BDA_DVBS_NAME_PIN_TUNER L"Pinnacle PCTV 400e Tuner"


//slot status
#define	CI_SLOT_EMPTY							0
#define	CI_SLOT_MODULE_INSERTED		1
#define	CI_SLOT_MODULE_OK					2
#define CI_SLOT_CA_OK							3
#define	CI_SLOT_DBG_MSG						4
#define	CI_SLOT_UNKNOWN_STATE			0xFF

//SendCIMessage Tags
#define	CI_PSI_COMPLETE					0xC
#define	CI_MODULE_READY					0xD
#define	CI_SWITCH_PRG_REPLY			0xE

extern void LogDebug(const char *fmt, ...) ;

CTechnotrend::CTechnotrend(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("MpTsTechnoTrend"), pUnk)
{
  m_hBdaApi = INVALID_HANDLE_VALUE;
  m_deviceType=UNKNOWN;
  m_ciStatus=-1;
}
CTechnotrend::~CTechnotrend(void)
{
  if (m_hBdaApi != INVALID_HANDLE_VALUE)
  {
    bdaapiCloseCI(m_hBdaApi);
    bdaapiClose(m_hBdaApi);
  }
}


static void OnSlotStatusCallback(PVOID Context,BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
{
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
  technoTrend->OnSlotChange( nSlot, nStatus, csInfo);
}
static void OnCaStatusCallback(PVOID Context,BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus)
{
  CTechnotrend* technoTrend=(CTechnotrend*)Context;
  technoTrend->OnCaChange(nSlot,nReplyTag,wStatus);
}

static void OnDisplayString(PVOID Context,BYTE  nSlot,char* pString,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayString slot:%d %s", nSlot,pString);
}
static void OnDisplayMenu(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayMenu slot:%d", nSlot);
}
static void OnDisplayList(PVOID Context,BYTE  nSlot,WORD  wItems,char* pStringArray,WORD  wLength)
{
  LogDebug("TechnoTrend:OnDisplayList slot:%d", nSlot);
}
static void OnSwitchOsdOff(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:CI_OnSwitchOsdOff slot:%d", nSlot);
}
static void OnInputRequest(PVOID Context,BYTE  nSlot,BOOL  bBlindAnswer,BYTE  nExpectedLength, DWORD dwKeyMask)
{
  LogDebug("TechnoTrend:CI_OnInputRequest slot:%d", nSlot);
}
static void OnLscSetDescriptor(PVOID Context,BYTE  nSlot,TYPE_CONNECT_DESCR* pDescriptor)
{
  LogDebug("TechnoTrend:OnLscSetDescriptor slot:%d", nSlot);
}
static void OnLscConnect(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscConnect slot:%d", nSlot);
}
static void OnLscDisconnect(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscDisconnect slot:%d", nSlot);
}
static void OnLscSetParams(PVOID Context,BYTE  nSlot,BYTE  BufferSize,BYTE  Timeout10Ms)
{
  LogDebug("TechnoTrend:OnLscSetParams slot:%d", nSlot);
}
static void OnLscEnquireStatus(PVOID Context,BYTE  nSlot)
{
  LogDebug("TechnoTrend:OnLscEnquireStatus slot:%d", nSlot);
}
static void OnLscGetNextBuffer(PVOID Context,BYTE  nSlot,BYTE  PhaseID)
{
  LogDebug("TechnoTrend:OnLscGetNextBuffer slot:%d", nSlot);
}
static void OnLscTransmitBuffer(PVOID Context,BYTE  nSlot,BYTE  PhaseID,BYTE* pData,WORD  nLength)
{
  LogDebug("TechnoTrend:OnLscTransmitBuffer slot:%d", nSlot);
}

STDMETHODIMP CTechnotrend::SetTunerFilter(IBaseFilter* tunerFilter)
{
  FILTER_INFO info;
  if (!SUCCEEDED(tunerFilter->QueryFilterInfo(&info))) return S_OK;
  if (wcscmp(info.achName,LBDG2_NAME_C_TUNER)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_S_TUNER)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBDG2_NAME_T_TUNER)==0) m_deviceType=BUDGET_2;
  if (wcscmp(info.achName,LBUDGET3NAME_TUNER)==0) m_deviceType=BUDGET_3;
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
  m_hBdaApi = bdaapiOpenHWIdx(m_deviceType, deviceId);
  if (m_hBdaApi == INVALID_HANDLE_VALUE) 
  {
    LogDebug("Technotrend: unable to open the device");
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

  if (!SUCCEEDED(bdaapiOpenCI(m_hBdaApi, m_technoTrendStructure)))
  {
      LogDebug("Technotrend: unable to open the CI");
      m_deviceType=UNKNOWN;  
      return S_OK;
  }
  BYTE v1,v2,v3,v4;
  bdaapiGetDrvVersion(m_hBdaApi,&v1,&v2,&v3,&v4);
  LogDebug("Technotrend: initalized id:%x, driver version:%d.%d.%d.%d",deviceId,v1,v2,v3,v4);
	

  return S_OK;
}

STDMETHODIMP CTechnotrend::IsTechnoTrend( BOOL* yesNo)
{
  *yesNo= (m_hBdaApi != INVALID_HANDLE_VALUE);
  return S_OK;
}

STDMETHODIMP CTechnotrend::IsCamReady( BOOL* yesNo)
{
  *yesNo=FALSE;
  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK)
  {
    *yesNo=TRUE;
  }
  return S_OK;
}

STDMETHODIMP CTechnotrend::SetAntennaPower( BOOL onOff)
{
  if (m_deviceType==USB_2_PINNACLE || m_deviceType==USB_2)
  {
    HRESULT hr=bdaapiSetDVBTAntPwr(m_hBdaApi,onOff);
    LogDebug("Technotrend: enable antenna power:%d %x",onOff,hr);
  }
  return S_OK;
}

STDMETHODIMP CTechnotrend::SetDisEqc(int diseqcType, int hiband, int vertical)
{
	LogDebug("Technotrend: SetDisEqc antenna :%d hiband:%d vertical:%d",diseqcType,hiband, vertical);
  int antennaNr=1;
  int option=0;
  switch (diseqcType)
  {
    case 0:
    case 1://simple A
      antennaNr = 1;
      break;
    case 2://simple B
      antennaNr = 2;
      break;
    case 3://Level 1 A/A
      antennaNr = 1;
      break;
    case 4://Level 1 B/A
      antennaNr = 2;
      break;
    case 5://Level 1 A/B
      antennaNr = 3;
      break;
    case 6://Level 1 B/B
      antennaNr = 4;
      break;
  }
  ULONG diseqc = 0xE01038F0;
  if (hiband!=0)              // high band
    diseqc |= 0x00000001;

  if (vertical==0)            // horizontal
    diseqc |= 0x00000002;

  diseqc |=  (byte)((antennaNr - 1) << 2);

  Polarisation polarity;
  if (vertical)
    polarity = BDA_POLARISATION_LINEAR_V;
  else
    polarity = BDA_POLARISATION_LINEAR_H;

  BYTE data[4];
  data[0]=(BYTE)((diseqc >> 24) & 0xff);
  data[1]=(BYTE)((diseqc >> 16) & 0xff);
  data[2]=(BYTE)((diseqc >> 8) & 0xff);
  data[3]=(BYTE)((diseqc ) & 0xff);

  HRESULT hr=bdaapiSetDiSEqCMsg(m_hBdaApi,&data[0],4,0,0,polarity);
  LogDebug("TechnoTrend:SetDiseqc:%x %x", diseqc,hr);
	bdaapiCIGetSlotStatus(m_hBdaApi,0);
	
  return S_OK;
}

STDMETHODIMP CTechnotrend::DescrambleService( int serviceId,BOOL* succeeded)
{
  HRESULT hr;
  *succeeded=FALSE;
  BOOL enabled=FALSE;
  m_ciStatus=-1;
	bdaapiCIGetSlotStatus(m_hBdaApi,0);
	LogDebug("TechnoTrend: DescrambleService:0x%x (%d) (%d)",serviceId,serviceId,m_slotStatus);
	if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK )
  {
    hr = bdaapiCIReadPSIFastDrvDemux(m_hBdaApi, (WORD)serviceId);
    if (hr==RET_SUCCESS)
    {
      if (m_ciStatus==1)
      {
        *succeeded=TRUE;
        LogDebug("TechnoTrend: service decoded:%x %d",hr,m_ciStatus);
    
      }
      else
      {
        LogDebug("TechnoTrend: service decoded:%x %d",hr,m_ciStatus);
      }
    }
    else
    {
      LogDebug("TechnoTrend: service not decoded:%x",hr);
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


void CTechnotrend::OnCaChange(BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus)
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

void CTechnotrend::OnSlotChange(BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
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