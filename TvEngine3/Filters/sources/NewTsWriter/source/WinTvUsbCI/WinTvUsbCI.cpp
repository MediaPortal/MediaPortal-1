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
#include "WinTvUsbCI.h"


extern void LogDebug(const char *fmt, ...) ;

typedef struct _VERSION_TAB 
{
  UCHAR PlugginVersion[12];
  UCHAR BDAVersion[12];
  UCHAR USBVersion[12];
  UCHAR FwVersion[4];
  UCHAR FPGAVersion[3];
} VERSION_TAB;
typedef VERSION_TAB *PVERSION_TAB ;

typedef struct _CAM_INFO{
  UCHAR App_Type; // 01 : CA 02: EPG
  USHORT App_Manuf; // Manufacturer
  USHORT Manuf_Code; //
  UCHAR SizeOfMess; // Size
  char Mess[1024]; // ManufName
} CAM_INFO ;
typedef CAM_INFO *PCAM_INFO ;

enum {
  STATUS_DEVICE_UNPLUGGED=0,
  STATUS_MODULE_NOT_INSERTED,
  STATUS_MODULE_INSERTED
} USB2CI_STATUS;

typedef struct _EvtCallBack
{
  PVOID Context;
  HRESULT (*Callback_Status)(PVOID Context, long Status);
  HRESULT (*Callback_CamInfo)(PVOID Context, PCAM_INFO CamInfo);
  HRESULT (*Callback_CloseMMI)(PVOID Context );
  HRESULT (*Callback_APDUFromCAM)(PVOID Context, long SizeofAPDU, char* APDU);
}EvtCallBack,*pEvtCallBack;

static EvtCallBack _callback;
static int _status=STATUS_DEVICE_UNPLUGGED;

HRESULT Callback_Status(PVOID Context, long Status)
{
  LogDebug("WinTVCI:Callback_Status %x %x", Context,Status);
  return S_OK;
}
HRESULT Callback_CamInfo(PVOID Context, PCAM_INFO CamInfo)
{
  LogDebug("WinTVCI:Callback_CamInfo %x %x", Context,CamInfo);
  return S_OK;
}
HRESULT Callback_CloseMMI(PVOID Context )
{
  LogDebug("WinTVCI:Callback_CloseMMI %x", Context);
  return S_OK;
}

HRESULT Callback_APDUFromCAM(PVOID Context, long SizeofAPDU, char* APDU)
{
  LogDebug("WinTVCI:Callback_APDUFromCAM %x", Context);
  return S_OK;
}

CWinTvUsbCI::CWinTvUsbCI(LPUNKNOWN pUnk, HRESULT *phr)
:CUnknown( NAME ("CWinTvUsbCI"), pUnk)
{
  m_pIUSB2CIPLugin=NULL;
}

CWinTvUsbCI::~CWinTvUsbCI()
{
  if (m_pIUSB2CIPLugin!=NULL)
  {
    m_pIUSB2CIPLugin->Release();
  }
}

STDMETHODIMP CWinTvUsbCI::SetFilter(IBaseFilter* tunerFilter)
{
  tunerFilter->QueryInterface(IID_IUSB2CIBDAConfig,(void**)&m_pIUSB2CIPLugin);
  if (m_pIUSB2CIPLugin==NULL) return S_OK;
  _callback.Context=(PVOID)this;
  _callback.Callback_Status=Callback_Status;
  _callback.Callback_CamInfo=Callback_CamInfo;
  _callback.Callback_CloseMMI=Callback_CloseMMI;
  _callback.Callback_APDUFromCAM=Callback_APDUFromCAM;
  HRESULT hr=m_pIUSB2CIPLugin->USB2CI_Init(&_callback);
  if (!SUCCEEDED(hr))
  {
    LogDebug("Failed to initialize WinTvCI USB");
    m_pIUSB2CIPLugin->Release();
    m_pIUSB2CIPLugin=NULL;
    return S_OK;
  }
  LogDebug("WinTvCI USB:Initialized");

  
  VERSION_TAB version;
  if (!SUCCEEDED(m_pIUSB2CIPLugin->USB2CI_GetVersion(&version)))
  {
    LogDebug("Failed to get WinTvCI USB version info");
    return S_OK;
  }
  LogDebug("WinTV: pluggin version:%s", version.PlugginVersion);
  LogDebug("WinTV: bda     version:%s", version.BDAVersion);
  LogDebug("WinTV: usb     version:%s", version.USBVersion);
  LogDebug("WinTV: f/w     version:%s", version.FwVersion);
  LogDebug("WinTV: FPGA    version:%s", version.FPGAVersion);
  return S_OK;
}

STDMETHODIMP CWinTvUsbCI::IsModuleInstalled( BOOL* yesNo)
{
  *yesNo = (_status != STATUS_DEVICE_UNPLUGGED);
  return S_OK;
}

STDMETHODIMP CWinTvUsbCI::IsCAMInstalled( BOOL* yesNo)
{
  *yesNo = (_status == STATUS_MODULE_INSERTED);
  return S_OK;
}

STDMETHODIMP CWinTvUsbCI::DescrambleService( BYTE* PMT, int PMTLength,BOOL* succeeded)
{
  *succeeded=FALSE;
  HRESULT hr=m_pIUSB2CIPLugin->USB2CI_GuiSendPMT(PMT, (short)PMTLength);
  if (SUCCEEDED(hr))
  {
    *succeeded=TRUE;
  }
  return S_OK;
}
