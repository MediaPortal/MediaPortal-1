//////////////////////////////////////////////////////////////////////////////
//
//                          (C) TechnoTrend AG 2005
//  All rights are reserved. Reproduction in whole or in part is prohibited
//  without the written consent of the copyright owner.
//
//  TechnoTrend reserves the right to make changes without notice at any time.
//  TechnoTrend makes no warranty, expressed, implied or statutory, including
//  but not limited to any implied warranty of merchantability or fitness for
//  any particular purpose, or that the use will not infringe any third party
//  patent, copyright or trademark. TechnoTrend must not be liable for any
//  loss or damage arising from its use.
//
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//
// Modification History:
//
//  Date     By      Description
//  -------  ------  ---------------------------------------------------------
//  18Mai05  HS      created
//  20Mai05  HS      bdaapiOpen() finished
//  23Mai05  HS      bdaapiGetMAC() added
//  23Mai05  HS      bdaapiGetDrvVersion() added
//  23Mai05  HS      bdaapiGetDeviceIDs() added
//  24Mai05  HS      prototype bdaapiI2CCombined() added
//  24Mai05  HS      prototype bdaapiSetDiSEqCMsg() added
//  24Mai05  HS      bdaapiEnumerate() added
//  13Jun05  HS      typedef's moved to ..\Include\bdaapiTypedefs.h
//
//////////////////////////////////////////////////////////////////////////////

#ifndef TTBDADRVAPI_H
#define TTBDADRVAPI_H

#ifdef TTBDADRVAPI_EXPORTS
#ifdef __cplusplus 
#define TTBDADRVAPI extern "C" __declspec(dllexport)
#else
#define TTBDADRVAPI __declspec(dllexport)
#endif
#else
#ifdef __cplusplus 
#define TTBDADRVAPI 
#define TTBDADRVAPI extern "C" __declspec(dllimport) 
#else
#define TTBDADRVAPI __declspec(dllimport)
#endif
#endif

// You must define TTUSBIR_STATIC_LIBRARY in your project if you use
// the static (not DLL) version of the library!
#ifdef TTBDADRVAPI_STATIC_LIBRARY
#undef TTBDADRVAPI
#define TTBDADRVAPI
#endif

#include <Mpeg2data.h>          // for the bdaapiReadPSIFast()
#include <atlbase.h>            // for the bdaapiReadPSIFast()
#include <windows.h>
#include "bdaapi_Typedefs.h"

/////////////////////////////////////////////////////////////////////////////
// open and close functions

TTBDADRVAPI UINT     bdaapiEnumerate (DEVICE_CAT DevType);
TTBDADRVAPI HANDLE   bdaapiOpen      (DEVICE_CAT DevType,
                                      UINT        uiDevID);
TTBDADRVAPI HANDLE   bdaapiOpenHWIdx (DEVICE_CAT DevType,
                                      UINT        uiDevID);
TTBDADRVAPI void     bdaapiClose     (HANDLE      hOpen);
TTBDADRVAPI TYPE_RET_VAL bdaapiOpenIR    (HANDLE      hOpen,
                                      PIRCBFCN    CallbackFcn = NULL,
                                      PVOID       Context = NULL);
TTBDADRVAPI TYPE_RET_VAL bdaapiCloseIR   (HANDLE      hOpen);
TTBDADRVAPI TYPE_RET_VAL bdaapiOpenCI(HANDLE            hOpen,
                                      TS_CiCbFcnPointer CbFuncPointer);
TTBDADRVAPI TYPE_RET_VAL bdaapiOpenCISlim(HANDLE                hOpen,
                                          TS_CiCbFcnPointerSlim CbFuncPointer);
TTBDADRVAPI TYPE_RET_VAL bdaapiOpenCIWithoutPointer(HANDLE hOpen);
TTBDADRVAPI TYPE_RET_VAL bdaapiCloseCI   (HANDLE   hOpen);
TTBDADRVAPI TYPE_RET_VAL bdaapiInstallDemuxReadEvent(HANDLE   hOpen,
                                                 PIRCBFCN CallbackFcn = NULL,
                                                 PVOID    Context = NULL);
TTBDADRVAPI TYPE_RET_VAL bdaapiUninstallDemuxReadEvent(HANDLE   hOpen);
//
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// functions to set something in the driver

TTBDADRVAPI TYPE_RET_VAL bdaapiSetDVBTAutoOffsetMode(HANDLE hOpen, BOOL bAutoOnOff);
TTBDADRVAPI TYPE_RET_VAL bdaapiSetDiSEqCMsg(HANDLE        hOpen,
                                        BYTE         *pData,
                                        BYTE          Bytes,
                                        BYTE          Repeat,
                                        BYTE          Toneburst,
                                        Polarisation  ePolarity);
TTBDADRVAPI TYPE_RET_VAL bdaapiSetVideoport(HANDLE hOpen, BOOL bCIMode,
                                                      BOOL *bCIOut);
TTBDADRVAPI TYPE_RET_VAL bdaapiSetDVBTAntPwr(HANDLE hOpen,
                                             BOOL   bAntPwrOnOff);
TTBDADRVAPI TYPE_RET_VAL bdaapiSetDrvDemuxFilter(HANDLE      hOpen,
                                                 TYPE_FILTER FilterType,
                                                 WORD        wPID,
                                                 BYTE*       pbFilterData,
                                                 BYTE*       pbFilterMask,
                                                 BYTE        bLength,
                                                 BYTE       &FilterID);
TTBDADRVAPI TYPE_RET_VAL bdaapiDelDrvDemuxFilter(HANDLE hOpen,
                                                 BYTE   FilterID);
//
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// functions to get something from the driver

TTBDADRVAPI TYPE_RET_VAL bdaapiGetDrvVersion(HANDLE hOpen, BYTE *v1, BYTE *v2,
                                                       BYTE *v3, BYTE *v4);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetMAC(HANDLE hOpen, DWORD *dwHigh, DWORD *dwLow);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetDeviceIDs(HANDLE hOpen, WORD *wVendor,
                                                      WORD *wSubVendor,
                                                      WORD *wDevice,
                                                      WORD *wSubDevice);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetUSBHighspeedMode(HANDLE hOpen,
                                                   BOOL   *bDevIsHighspeed);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetDVBTAutoOffsetMode(HANDLE hOpen, BOOL *bAutoOnOff);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetDVBTAntPwr(HANDLE hOpen,
                                             DWORD  *dwAntPwr);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetDevNameAndFEType(HANDLE          hOpen,
                                                   pTS_FilterNames FilterNames);
TTBDADRVAPI TYPE_RET_VAL bdaapiGetHwIdx(HANDLE  hOpen,
                                        DWORD  &dwHwIdx);


TTBDADRVAPI TYPE_RET_VAL bdaapiCIReadPSIFast(HANDLE               hOpen,
                                             WORD                 PNR,
                                             CComPtr <IMpeg2Data> pIMpeg2Data);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIReadPSIFastDrvDemux(HANDLE hOpen,
                                                     WORD   PNR);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIReadPSIFastWithPMT(HANDLE hOpen,
                                                    BYTE   *pPMT,
                                                    WORD   wLength);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIReadDrvDemuxFilterData(HANDLE hOpen,
                                                        BYTE   FilterID,
                                                        BYTE   *pData,
                                                        WORD   &wLength,
                                                        DWORD  dwTimeout = 5000);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIMultiDecode(HANDLE hOpen,
                                             WORD  *PNR,
                                             int    NrOfPnrs);

//
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// menu functions for common interface

TTBDADRVAPI TYPE_RET_VAL bdaapiCIEnterModuleMenu(HANDLE hOpen, BYTE nSlot);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIGetSlotStatus(HANDLE hOpen, BYTE nSlot);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIAnswer(HANDLE hOpen,
                                        BYTE   nSlot,
                                        char*  pKeyBuffer,
                                        BYTE   nLength);
TTBDADRVAPI TYPE_RET_VAL bdaapiCIMenuAnswer(HANDLE hOpen,
                                            BYTE   nSlot,
                                            BYTE   Selection);
TTBDADRVAPI int	     bdaapiCIConvertCharBuf(HANDLE hOpen,
                                            char   *string,
                                            int    len);
//
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// EEPROM access

TTBDADRVAPI TYPE_RET_VAL bdaapiUserEEPROM_Write(HANDLE hOpen,
                                                BYTE   Offset,
                                                BYTE   *pData,
                                                BYTE   Length);
TTBDADRVAPI TYPE_RET_VAL bdaapiUserEEPROM_Read (HANDLE hOpen,
                                                BYTE   Offset,
                                                BYTE   *pData,
                                                BYTE   Length);
//
/////////////////////////////////////////////////////////////////////////////

#endif // #ifndef TTBDADRVAPI_H

// eof
