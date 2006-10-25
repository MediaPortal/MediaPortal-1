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

extern void LogDebug(const char *fmt, ...) ;

CTechnotrend::CTechnotrend(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("MpTsTechnoTrend"), pUnk)
{
  m_hBdaApi = INVALID_HANDLE_VALUE;
  m_deviceType=UNKNOWN;
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
  m_technoTrendStructure.p01=OnSlotStatusCallback;
  m_technoTrendStructure.p01Context=this;
  m_technoTrendStructure.p02=OnCaStatusCallback;
  m_technoTrendStructure.p02Context=this;

  if (!SUCCEEDED(bdaapiOpenCISlim(m_hBdaApi, m_technoTrendStructure)))
  {
      LogDebug("Technotrend: unable to open the CI");
      m_deviceType=UNKNOWN;  
      return S_OK;
  }
  BYTE v1,v2,v3,v4;
  bdaapiGetDrvVersion(m_hBdaApi,&v1,&v2,&v3,&v4);
  LogDebug("Technotrend: initalized, driver version:%d.%d.%d.%d",v1,v2,v3,v4);
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

STDMETHODIMP CTechnotrend::SetDisEqc(int diseqcType, BOOL hiband, BOOL vertical)
{
  int position=0;
  int option=0;
  switch (diseqcType)
  {
    case 0:
    case 1://simple A
      position = 0;
      option = 0;
      break;
    case 2://simple B
      position = 0;
      option = 0;
      break;
    case 3://Level 1 A/A
      position = 0;
      option = 0;
      break;
    case 4://Level 1 B/A
      position = 1;
      option = 0;
      break;
    case 5://Level 1 A/B
      position = 0;
      option = 1;
      break;
    case 6://Level 1 B/B
      position = 1;
      option = 1;
      break;
  }
  UINT diseqc = 0xE01038F0;
  if (hiband)                 // high band
    diseqc |= 0x00000001;
  else                        // low band
    diseqc &= 0xFFFFFFFE;

  if (vertical)             // vertikal
    diseqc &= 0xFFFFFFFD;
  else                        // horizontal
    diseqc |= 0x00000002;

  if (position != 0)             // Sat B
    diseqc |= 0x00000004;
  else                        // Sat A
    diseqc &= 0xFFFFFFFB;

  if (option != 0)               // option B
    diseqc |= 0x00000008;
  else                        // option A
    diseqc &= 0xFFFFFFF7;
        
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
  return S_OK;
}

STDMETHODIMP CTechnotrend::DescrambleService( int serviceId,BOOL* succeeded)
{
  *succeeded=FALSE;
  if (m_slotStatus==CI_SLOT_CA_OK || m_slotStatus==CI_SLOT_MODULE_OK)
  {
    LogDebug("TechnoTrend:DescrambleService:%x", serviceId);
    HRESULT hr = bdaapiCIReadPSIFastDrvDemux(m_hBdaApi, serviceId);
    if (SUCCEEDED(hr))
    {
      *succeeded=TRUE;
      LogDebug("TechnoTrend: service decoded");
    }
    else
    {
      LogDebug("TechnoTrend: service not decoded:%x",hr);
    }
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
  LogDebug("Technotrend: ca change. slot:%d replytag:%d statys:%d",nSlot,nReplyTag,wStatus);
}

void CTechnotrend::OnSlotChange(BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo)
{
  LogDebug("Technotrend: slot change. slot:%d status:%d",nSlot,nStatus);
  m_slotStatus=nStatus;
  if (csInfo!=NULL)
  {
    LogDebug("Technotrend: CI status:%d ",csInfo->nStatus);
    if (csInfo->pMenuTitleString!=NULL)
      LogDebug("Technotrend: CI text  :%s ",csInfo->pMenuTitleString);
    for (int i=0; i < csInfo->wNoOfCaSystemIDs;++i)
    {
      LogDebug("Technotrend: ca system id  :%x ",csInfo->pCaSystemIDs[i]);
    }
  }
}