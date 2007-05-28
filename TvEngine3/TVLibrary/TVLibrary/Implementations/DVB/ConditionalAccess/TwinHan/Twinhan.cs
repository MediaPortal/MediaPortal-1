/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Summary description for Twinhan.
  /// </summary>

  #region Twinhan Sample Code
  /*#ifndef THBDA_IOCTL_H
#define THBDA_IOCTL_H


#define FILE_ANY_ACCESS     0
#define METHOD_BUFFERED     0

// {255E0082-2017-4b03-90F8-856A62CB3D67}
static const GUID GUID_THBDA_CMD = 
{ 0x255e0082, 0x2017, 0x4b03, { 0x90, 0xf8, 0x85, 0x6a, 0x62, 0xcb, 0x3d, 0x67 } };


//{E5644CC4-17A1-4eed-BD90-74FDA1D65423}
static GUID GUID_THBDA_TUNER = 
{ 0xE5644CC4, 0x17A1, 0x4eed, { 0xBD, 0x90, 0x74, 0xFD, 0xA1, 0xD6, 0x54, 0x23 } };

#define CTL_CODE( DeviceType, Function, Method, Access ) (                 \
    ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method) \
)

#define THBDA_IO_INDEX    0xAA00


//*******************************************************************************************************
//Functionality : Set turner power
//InBuffer      : Tuner_Power_ON | Tuner_Power_OFF
//InBufferSize  : 1 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SET_TUNER_POWER             CTL_CODE(THBDA_IO_INDEX, 100, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get turner power status
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : Tuner_Power_ON | Tuner_Power_OFF
//OutBufferSize : 1 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_TUNER_POWER             CTL_CODE(THBDA_IO_INDEX, 101, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//Obsolete now
//*******************************************************************************************************
//Functionality : Set LNB configuration
//InBuffer      : struct LNB_DATA
//InBufferSize  : sizeof(LNB_DATA) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
//#define THBDA_IOCTL_SET_LNB                     CTL_CODE(THBDA_IO_INDEX, 102, METHOD_BUFFERED, FILE_ANY_ACCESS) 
//
//Obsolete now
//*******************************************************************************************************
//Functionality : Get LNB configuration
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct LNB_DATA
//OutBufferSize : sizeof(LNB_DATA) bytes
//*******************************************************************************************************
//#define THBDA_IOCTL_GET_LNB                     CTL_CODE(THBDA_IO_INDEX, 103, METHOD_BUFFERED, FILE_ANY_ACCESS) 



//*******************************************************************************************************
//Functionality : Set LNB parameters
//InBuffer      : struct LNB_DATA
//InBufferSize  : sizeof(LNB_DATA) bytes
//OutBuffer     : 0
//OutBufferSize : 0
//*******************************************************************************************************
#define THBDA_IOCTL_SET_LNB_DATA            CTL_CODE(THBDA_IO_INDEX, 128, METHOD_BUFFERED, FILE_ANY_ACCESS) 

//*******************************************************************************************************
//Functionality : GET LNB parameters
//InBuffer      : NULL
//InBufferSize  : 0
//OutBuffer     : struct LNB_DATA
//OutBufferSize : sizeof(LNB_DATA) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_LNB_DATA            CTL_CODE(THBDA_IO_INDEX, 129, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Send DiSEqC command
//InBuffer      : struct DiSEqC_DATA
//InBufferSize  : sizeof(DiSEqC_DATA) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SET_DiSEqC                  CTL_CODE(THBDA_IO_INDEX, 104, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get DiSEqC command
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct DiSEqC_DATA
//OutBufferSize : sizeof(DiSEqC_DATA) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_DiSEqC                  CTL_CODE(THBDA_IO_INDEX, 105, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Set turner frequency and symbol rate
//InBuffer      : struct TURNER_VALUE
//InBufferSize  : sizeof(TURNER_VALUE) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_LOCK_TUNER                  CTL_CODE(THBDA_IO_INDEX, 106, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get turner frequency and symbol rate
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct TURNER_VALUE
//OutBufferSize : sizeof(TURNER_VALUE) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_TUNER_VALUE            CTL_CODE(THBDA_IO_INDEX, 107, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get signal quality & strength
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct SIGNAL_DATA
//OutBufferSize : sizeof(SIGNAL_DATA) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_SIGNAL_Q_S              CTL_CODE(THBDA_IO_INDEX, 108, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : START TS capture (from Tuner to driver Ring buffer)
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_START_CAPTURE               CTL_CODE(THBDA_IO_INDEX, 109, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Stop TS capture
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_STOP_CAPTURE                CTL_CODE(THBDA_IO_INDEX, 110, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get Driver ring buffer status
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct RING_BUF_STATUS 
//OutBufferSize : sizeof(RING_BUF_STATUS) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_RINGBUFFER_STATUS      CTL_CODE(THBDA_IO_INDEX, 111, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get TS from driver's ring buffer to local  buffer 
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct CAPTURE_DATA
//OutBufferSize : sizeof(CAPTURE_DATA) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_CAPTURE_DATA            CTL_CODE(THBDA_IO_INDEX, 112, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Set PID filter mode and Pids to PID filter
//InBuffer      : struct PID_FILTER_INFO
//InBufferSize  : sizeof(PID_FILTER_INFO) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SET_PID_FILTER_INFO         CTL_CODE(THBDA_IO_INDEX, 113, METHOD_BUFFERED, FILE_ANY_ACCESS)  


//*******************************************************************************************************
//Functionality : Get Pids, PLD mode and available max number Pids
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct PID_FILTER_INFO
//OutBufferSize : sizeof(PID_FILTER_INFO) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_PID_FILTER_INFO         CTL_CODE(THBDA_IO_INDEX, 114, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Start RC(Remote Controller receiving) thread
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_START_REMOTE_CONTROL        CTL_CODE(THBDA_IO_INDEX, 115, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Stop RC thread, and remove all RC event
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_STOP_REMOTE_CONTROL         CTL_CODE(THBDA_IO_INDEX, 116, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Add RC_Event to driver
//InBuffer      : REMOTE_EVENT
//InBufferSize  : sizeof(REMOTE_EVENT) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_ADD_RC_EVENT                CTL_CODE(THBDA_IO_INDEX, 117, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Remove RC_Event 
//InBuffer      : REMOTE_EVENT
//InBufferSize  : sizeof(REMOTE_EVENT) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_REMOVE_RC_EVENT             CTL_CODE(THBDA_IO_INDEX, 118, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get Remote Controller key
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : BYTE
//OutBufferSize : 1 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_REMOTE_CONTROL_VALUE    CTL_CODE(THBDA_IO_INDEX, 119, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//********************************************************************
//Functionality :Get number of  TV tuner RF in connector
//InBuffer        :  NULL
//InBufferSize  : 0
//OutBuffer     : DWORD , 1-n
//OutBufferSize : sizeof(DWORD) bytes
#define THBDA_IOCTL_GET_NUM_D_TUNER_RF_IN      CTL_CODE(THBDA_IO_INDEX, 130, METHOD_BUFFERED, FILE_ANY_ACCESS) 
//********************************************************************
//Functionality : Set DTV tuner RF in connector
//InBuffer        :  DWORD,    (For 3250, 0:the upper RF in, 1:the lower RF in)
//                   		For 6090, 0: master 1:slave 
//InBufferSize  : sizeof(DWORD) bytes
//OutBuffer     : 0
//OutBufferSize : 0
#define THBDA_IOCTL_SET_D_TUNER_RF_IN          CTL_CODE(THBDA_IO_INDEX, 131, METHOD_BUFFERED, FILE_ANY_ACCESS) 
//***************************************************************
//Functionality : Get DTV tuner RF in connector
//InBuffer     : 0
//InBufferSize : 0
//OutBuffer        :  DWORD,    (For 3250, 0:the upper RF in, 1:the lower RF in)
//                   		For 6090, 0: master 1:slave 
//OutBufferSize  : sizeof(DWORD) bytes
#define THBDA_IOCTL_GET_D_TUNER_RF_IN          CTL_CODE(THBDA_IO_INDEX, 132, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Set Remote control HID function enable or disable-For 2.60x winmanager to disable HID.
//InBuffer      : 1 0 for OFF,others for ON.
//InBufferSize  : 1 bytes
//OutBuffer     : 0 registers value
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_HID_RC_ENABLE             CTL_CODE(THBDA_IO_INDEX, 152, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Set HID Remote control configuration
//InBuffer      : RC_CONFIG
//InBufferSize  : sizeof(RC_CONFIG) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SET_HID_RC_CONFIG             CTL_CODE(THBDA_IO_INDEX, 153, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : GET HID Remote control configuration
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : RC_CONFIG
//OutBufferSize : sizeof(RC_CONFIG) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_HID_RC_CONFIG             CTL_CODE(THBDA_IO_INDEX, 154, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : For HID driver to get IR value- This is called by old HID driver, obsolete now
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : RC_Buffer
//OutBufferSize : 3 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_RCCODE                  CTL_CODE(THBDA_IO_INDEX, 300, METHOD_NEITHER, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Reset USB or PCI controller
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_RESET_DEVICE                CTL_CODE(THBDA_IO_INDEX, 120, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Check BDA driver if support IOCTL interface
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CHECK_INTERFACE             CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Set Twinhan BDA driver configuration
//InBuffer      : struct THBDAREGPARAMS
//InBufferSize  : sizeof(THBDAREGPARAMS) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SET_REG_PARAMS              CTL_CODE(THBDA_IO_INDEX, 122, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Get Twinhan BDA driver configuration
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct THBDAREGPARAMS
//OutBufferSize : struct THBDAREGPARAMS
//*******************************************************************************************************
#define THBDA_IOCTL_GET_REG_PARAMS              CTL_CODE(THBDA_IO_INDEX, 123, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Get device info
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct DEVICE_INFO
//OutBufferSize : sizeof(DEVICE_INFO) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_DEVICE_INFO             CTL_CODE(THBDA_IO_INDEX, 124, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Get driver info
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct DriverInfo
//OutBufferSize : sizeof(DriverInfo) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_DRIVER_INFO             CTL_CODE(THBDA_IO_INDEX, 125, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Write EEPROM value
//InBuffer      : struct EE_IO_DATA
//InBufferSize  : sizeof(EE_IO_DATA) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SET_EE_VAL                  CTL_CODE(THBDA_IO_INDEX, 126, METHOD_BUFFERED, FILE_ANY_ACCESS) 
                  

//*******************************************************************************************************
//Functionality : Read EEPROM value      
//InBuffer      : struct EE_IO_DATA
//InBufferSize  : sizeof(EE_IO_DATA) bytes
//OutBuffer     : struct EE_IO_DATA
//OutBufferSize : sizeof(EE_IO_DATA) bytes
//*******************************************************************************************************                          
#define THBDA_IOCTL_GET_EE_VAL                  CTL_CODE(THBDA_IO_INDEX, 127, METHOD_BUFFERED, FILE_ANY_ACCESS) 

//*******************************************************************************************************
//Functionality : Enable MCE DVBT TS translation (-S NIT -> -T NIT, and -T Freq/bandwidth -> -S Freq/SRate)
//InBuffer      : 1 0 for OFF,others for ON.
//InBufferSize  : 1 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_ENABLE_MCE_DVBT         CTL_CODE(THBDA_IO_INDEX, 304, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Reset (Clear) DVB-S Transponder mapping table entry for virtual DVB-T interface
//InBuffer      : NULL
//InBufferSize  : 0
//OutBuffer     : NULL
//OutBufferSize : 0
//*******************************************************************************************************
#define THBDA_IOCTL_RESET_T2S_MAPPING            CTL_CODE(THBDA_IO_INDEX, 301, METHOD_BUFFERED, FILE_ANY_ACCESS) 

//*******************************************************************************************************
//Functionality : Set DVB-S Transponder mapping table entry for virtual DVB-T interface
//InBuffer      : struct DVB-T2S_MAPPING_ENTRY
//InBufferSize  : sizeof(struct DVB-T2S_MAPPING_ENTRY) bytes
//OutBuffer     : NULL
//OutBufferSize : 0
//*******************************************************************************************************
#define THBDA_IOCTL_SET_T2S_MAPPING            CTL_CODE(THBDA_IO_INDEX, 302, METHOD_BUFFERED, FILE_ANY_ACCESS) 

//*******************************************************************************************************
//Functionality : GET DVB-S Transponder mapping table entry
//InBuffer      : &(Table_Index)
//InBufferSize  : sizeof(ULONG)
//OutBuffer     : struct DVB-T2S_MAPPING_ENTRY
//OutBufferSize : sizeof(struct DVB-T2S_MAPPING_ENTRY) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_T2S_MAPPING            CTL_CODE(THBDA_IO_INDEX, 303, METHOD_BUFFERED, FILE_ANY_ACCESS) 

//*******************************************************************************************************
//Functionality : Get locked frequency change status
//InBuffer      : NULL
//InBufferSize  : 0
//OutBuffer     : DWORD dwStatus: 1: Frequency change, 0: No change
//OutBufferSize : 1 DWORD
//*******************************************************************************************************
#define THBDA_IOCTL_GET_FREQ_CHANGE_STATUS     CTL_CODE(THBDA_IO_INDEX, 305, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Set DVB-S Transponder mapping table entry for virtual DVB-T interface DVBS2
//InBuffer      : struct DVB-T2S_MAPPING_ENTRY_S2
//InBufferSize  : sizeof(struct DVB-T2S_MAPPING_ENTRY_S2) bytes
//OutBuffer     : NULL
//OutBufferSize : 0
//*******************************************************************************************************
#define THBDA_IOCTL_SET_T2S_MAPPING_S2            CTL_CODE(THBDA_IO_INDEX, 306, METHOD_BUFFERED, FILE_ANY_ACCESS) 

//*******************************************************************************************************
//Functionality : GET DVB-S Transponder mapping table entry-DVBS2
//InBuffer      : &(Table_Index)
//InBufferSize  : sizeof(ULONG)
//OutBuffer     : struct DVB-T2S_MAPPING_ENTRY_S2
//OutBufferSize : sizeof(struct DVB-T2S_MAPPING_ENTRY_S2) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_T2S_MAPPING_S2            CTL_CODE(THBDA_IO_INDEX, 307, METHOD_BUFFERED, FILE_ANY_ACCESS) 


//*******************************************************************************************************
//Functionality : Enable/Register BDA interfaces for TS generator simulator filters
//InBuffer      : 1  0:Disabled, 1:DVB-S, 2:DVB-T, 4:DVB-C, 8:ATSC 16:MCE DVB-T
//InBufferSize  : 1  BYTE
//OutBuffer     : NULL
//OutBufferSize : 0 byte
//*******************************************************************************************************
#define THBDA_IOCTL_REGISTER_BDA_IF          CTL_CODE(THBDA_IO_INDEX, 400, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Simulation TS Start
//InBuffer      : struct tag_SIMU_PARAM
//InBufferSize  : sizeof(struct tag_SIMU_PARAM) bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SIMU_TS_START                CTL_CODE(THBDA_IO_INDEX, 401, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Simulation TS Stop
//InBuffer      : NULL
//InBufferSize  : 0
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SIMU_TS_STOP                CTL_CODE(THBDA_IO_INDEX, 402, METHOD_BUFFERED, FILE_ANY_ACCESS)
 
//*******************************************************************************************************
//Functionality : Simulation Set Property
//InBuffer      : struct tag_SIMU_PROP
//InBufferSize  : sizeof(struct tag_SIMU_PROP)
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SIMU_SET_PROP                CTL_CODE(THBDA_IO_INDEX, 403, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Simulation Set Property
//InBuffer      : NULL
//InBufferSize  : 0
//OutBuffer     : struct tag_SIMU_PROP
//OutBufferSize : sizeof(struct tag_SIMU_PROP) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_SIMU_GET_PROP                CTL_CODE(THBDA_IO_INDEX, 404, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Download tuner firmware, 704C
//InBuffer      : 1 byte buffer,  0:Downlaod analog TV firmware, 1:download DVB-T firmware
//InBufferSize  : 1:byte
//OutBuffer     :1 byte buffer,  0-99: download percentage, 100:download complete, 255:Fail 
//OutBufferSize : 1 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_DOWNLOAD_TUNER_FIRMWARE    CTL_CODE(THBDA_IO_INDEX, 410, METHOD_BUFFERED, FILE_ANY_ACCESS) 
//*******************************************************************************************************
//Functionality : Get tuner firmware download progress,704C
//InBuffer      : NULL
//InBufferSize  : 0:byte
//OutBuffer     :1 byte buffer,  0-99: download percentage, 100:download complete, 255:Fail 
//OutBufferSize : 1 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_DOWNLOAD_TUNER_FIRMWARE_STAUS CTL_CODE(THBDA_IO_INDEX, 411, METHOD_BUFFERED, FILE_ANY_ACCESS)
//*******************************************************************************************************
//Functionality : Get tuner firmware type
//InBuffer      : NULL
//InBufferSize  : 0:byte
//OutBuffer     : 1 byte buffer,  0:Analog TV firmware 1 DVB-T firmware
//OutBufferSize : 1 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_GET_TUNER_FIRMWARE_TYPE CTL_CODE(THBDA_IO_INDEX, 412, METHOD_BUFFERED, FILE_ANY_ACCESS) 





//*******************************************************************************************************
//Functionality : Get CI state
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct THCIState
//OutBufferSize : sizeof(THCIState) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_GET_STATE                CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Get APP info.
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct THAppInfo
//OutBufferSize : sizeof(THAppInfo) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_GET_APP_INFO                CTL_CODE(THBDA_IO_INDEX, 201, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Init MMI
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_INIT_MMI                    CTL_CODE(THBDA_IO_INDEX, 202, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Get MMI
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct THMMIInfo
//OutBufferSize : sizeof(THMMIInfo) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_GET_MMI                     CTL_CODE(THBDA_IO_INDEX, 203, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Answer
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : struct THMMIInfo
//OutBufferSize : sizeof(THMMIInfo) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_ANSWER                      CTL_CODE(THBDA_IO_INDEX, 204, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Close MMI
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_CLOSE_MMI                   CTL_CODE(THBDA_IO_INDEX, 205, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Send PMT
//InBuffer      : PMT data buffer
//InBufferSize  : PMT data buffer size bytes
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//Comment       : CA_PMT data format
                    //1: ca pmt list management;(8 bit);
                    //2: program number (16 bit);
                    //3: reserved (2 bit);
                    //4: version number (5 bit);
                    //5: current next indicator (I bit);
                    //6: reserved (4 bit);
                    //7: program information length (12 bit);
                    //8: if (7!=0)
                    //	    ca pmt command id (program level); (8 bit);
                    //	    ca descriptor at program level; (n * 8bit);
                    //9:  stream type (8 bit);
                    //10: reserved (3 bit);
                    //11: elementary stream PID (bit 13);
                    //12: reserved (4 bit);
                    //13: ES information length (12 bit);
                    //14: if (ES information length ! =0)
                    //       ca pmt command id ( elementary stream level) (8 bit);
                    //	     ca descriptor at elementary stream level; ( n * 8bit)
                    //* more detail, please refer to EN 50221 (8,4,3,4 CA_PMT); 
//*******************************************************************************************************
#define THBDA_IOCTL_CI_SEND_PMT                    CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS)


//This function is only available for MANTIS, CI_Support==SUPPORT_CI_NEW
//*******************************************************************************************************
//Functionality : Create CI event
//InBuffer      : hEventHandle (The event handle that is created by AP)
//InBufferSize  : sizeof(HANDLE)
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_EVENT_CREATE             CTL_CODE(THBDA_IO_INDEX, 208, METHOD_BUFFERED, FILE_ANY_ACCESS)

//This function is only available for MANTIS, CI_Support==SUPPORT_CI_NEW
//*******************************************************************************************************
//Functionality : Close CI event
//InBuffer      : hEventHandle (The event handle that is sended by create CI event)
//InBufferSize  : sizeof(HANDLE)
//OutBuffer     : NULL
//OutBufferSize : 0 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_EVENT_CLOSE              CTL_CODE(THBDA_IO_INDEX, 209, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Get PMT Reply
//InBuffer      : NULL
//InBufferSize  : 0 bytes
//OutBuffer     : PMT Reply Buffer
//OutBufferSize : sizeof(PMT Reply Buffer) bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_GET_PMT_REPLY            CTL_CODE(THBDA_IO_INDEX, 210, METHOD_BUFFERED, FILE_ANY_ACCESS)


//*******************************************************************************************************
//Functionality : Send CI raw command
//InBuffer      : RAW_CMD_INFO
//InBufferSize  : sizeof(RAW_CMD_INFO) bytes
//OutBuffer     : NULL
//OutBufferSize : 0
//*******************************************************************************************************
#define THBDA_IOCTL_CI_SEND_RAW_CMD            CTL_CODE(THBDA_IO_INDEX, 211, METHOD_BUFFERED, FILE_ANY_ACCESS)

//*******************************************************************************************************
//Functionality : Get CI raw command data
//InBuffer      : NULL
//InBufferSize  : 0
//OutBuffer     : Raw command data buffer
//OutBufferSize : Max 1024 bytes
//*******************************************************************************************************
#define THBDA_IOCTL_CI_GET_RAW_CMD_DATA        CTL_CODE(THBDA_IO_INDEX, 212, METHOD_BUFFERED, FILE_ANY_ACCESS)

#define MAX_PMT_REPLY_SIZE	1024

enum ca_pmt_cmd_id
{
	ok_decrambleing = 1,
	ok_mmi,
	query,
	not_selected
};
#define CA_PMT_CMDID_Decrambleing 1
#define CA_PMT_CMDID_MMI          2
#define CA_PMT_CMDID_Query        3 
#define CA_PMT_CMDID_NSelected    4

#define CA_PMT_LIST_MGT_MORE   0
#define CA_PMT_LIST_MGT_FIRST  1
#define CA_PMT_LIST_MGT_LAST   2
#define CA_PMT_LIST_MGT_ONLY   3
#define CA_PMT_LIST_MGT_ADD    4
#define CA_PMT_LIST_MGT_UPDATE 5


typedef enum _CAM_TYPE_ENUM
{
    CAM_DEFAULT = 1, //Viaccess
    TH_CAM_ASTON = 2,
    TH_CAM_CONAX = 3,
    TH_CAM_CRYPTOWORKS = 4    
}  CAM_TYPE_ENUM;




#define RC_NO_DATA                          0x44

#define Tuner_Power_ON                      1
#define Tuner_Power_OFF                     0


#define LNB_POWER_OFF                       0
#define LNB_POWER_ON                        1

#define LNB_TYPE_NORMAL                     0
#define LNB_TYPE_UNIVERSAL                  1
#define LNB_TYPE_CUSTOM                     2

#define Data_Burst_OFF                      0
#define Data_Burst_ON                       1

#define Tone_Burst_OFF                      0
#define Tone_Burst_ON                       1

//Note:it's reversed as compared to Satellite.dll.
#define POLARITY_H                          0
#define POLARITY_V                          1

#define DiSEqC_NULL                         0
#define DiSEqC_A                            1
#define DiSEqC_B                            2
#define DiSEqC_C                            3
#define DiSEqC_D                            4

#define F22K_Output_HiLo 0
#define F22K_Output_Off  1 
#define F22K_Output_On   2 


typedef struct {
    GUID    CmdGUID;            // Private Command GUID
    DWORD   dwIoControlCode;    // operation
    LPVOID  lpInBuffer;         // input data buffer
    DWORD   nInBufferSize;      // size of input data buffer
    LPVOID  lpOutBuffer;        // output data buffer
    DWORD   nOutBufferSize;     // size of output data buffer
    LPDWORD lpBytesReturned;    // byte count
} THBDACMD, *PTHBDACMD;

//Obsolete now
//typedef struct _LNB_DATA
//{
//    UCHAR           LNB_POWER;              // LNB_POWER_ON | LNB_POWER_OFF
//    UCHAR           POLARITY;               // POLARITY_H | POLARITY_V
//    UCHAR           HZ_22K;                 // HZ_22K_OFF | HZ_22K_ON
//    UCHAR           Tone_Data_Burst;        // Data_Burst_ON | Tone_Burst_ON |Tone_Data_Disable
//
//    ULONG           ulLNBLOFLowBand;        // LOF Low Band, in MHz
//    ULONG           ulLNBLOFHighBand;       // LOF High Band, in MHz
//    ULONG           ulLNBLOFHiLoSW;         // LOF High/Low Band Switch Freq, in MHz
//
//    UCHAR           HighBand_22K_Output;    // HighBand_22K_Output_Enable: output 22K if HighBand Freq, no output 22K if LowBand Freq
//                                            // HighBand_22K_Output_Disable: output 22K according to "HZ_22K" setting
//
//    UCHAR           DiSEqC_Port;            // DiSEqC_NULL | DiSEqC_A | DiSEqC_B | DiSEqC_C | DiSEqC_D
//} LNB_DATA, *P_LNB_DATA;

typedef struct _RC_CONFIG
{
    ULONG       IR_Standard;                // 0:RC-5, 1:NEC
    ULONG       IRSYSCODECHECK1;            // RC system code 1: 0x00ff (Twinhan's RC)
    ULONG       IRSYSCODECHECK2;            // RC system code 2: 0x01aa,(optional,3'rd party IR...)
                                            // IRSYSCODECHECKx --> 0:accept all , 0xffff:reject all
    ULONG       RC_Configuration ;	    // RC Mapping table
					    //; 0: DTV-DVB remote, 1: CyberLink, 2: IVI, 3: MCE, 4: DTV-DVB (WM_INPUT), 5: Custom,
 					    //  0x10:DNTV, 0xFFFF: Disable RC
} RC_CONFIG, *P_RC_CONFIG;



#define MAX_T2S_TABLE_SIZE 180
typedef struct _DVB_T2S_MAPPING_ENTRY
{
    ULONG       ulIndex;                // Mapping Table index from 0-MAX_T2S_TABLE_SIZE
    ULONG       T_Frequency;            // DVB-T in Khz 
    ULONG       S_Frequency;            // DVB-S in Khz, Original Frequency, (not offseted by LOF)
    ULONG       SymbolRate;		// in Ksps  
    ULONG       Polarity;		// POLARITY_H=0|POLARITY_V=1, Note:it's reversed as compared to Satellite.dll.
    ULONG       LNB_POWER;              // LNB_POWER_ON | LNB_POWER_OFF
    ULONG       Tone_Data_Burst;        // Tone_Data_OFF | Tone_Burst_ON | Data_Burst_ON 
    ULONG       ulLNBLOFLowBand;        // LOF Low Band, in KHz
    ULONG       ulLNBLOFHighBand;       // LOF High Band, in KHz
    ULONG       ulLNBLOFHiLoSW;         // LOF High/Low Band Switch Freq, in KHz
    ULONG       f22K_Output;	    	// 22KHz tone Control: F22K_Output_HiLo, F22K_Output_Off, F22K_Output_On
    ULONG       DiSEqC_Port;            // DiSEqC_NULL | DiSEqC_A | DiSEqC_B | DiSEqC_C | DiSEqC_D
} DVB_T2S_MAPPING_ENTRY, *P_DVB_T2S_MAPPING_ENTRY;

//DVBS/S2 Modulation
#define SAT_MOD_UNKNOWN   0       //(default, for DVB-S)
#define SAT_MOD_QPSK    20
#define SAT_MOD_8PSK    23
#define SAT_MOD_16APSK  24
#define SAT_MOD_32APSK  22

typedef struct _DVB_T2S_MAPPING_ENTRY_S2
{
    ULONG       ulIndex;                // Mapping Table index from 0-MAX_T2S_TABLE_SIZE
    ULONG       T_Frequency;            // DVB-T in Khz 
    ULONG       S_Frequency;            // DVB-S in Khz, Original Frequency, (not offseted by LOF)
    ULONG       SymbolRate;		// in Ksps  
    ULONG       Polarity;		// POLARITY_H=0|POLARITY_V=1, Note:it's reversed as compared to Satellite.dll.
    ULONG       LNB_POWER;              // LNB_POWER_ON | LNB_POWER_OFF
    ULONG       Tone_Data_Burst;        // Tone_Data_OFF | Tone_Burst_ON | Data_Burst_ON 
    ULONG       ulLNBLOFLowBand;        // LOF Low Band, in KHz
    ULONG       ulLNBLOFHighBand;       // LOF High Band, in KHz
    ULONG       ulLNBLOFHiLoSW;         // LOF High/Low Band Switch Freq, in KHz
    ULONG       f22K_Output;	    	// 22KHz tone Control: F22K_Output_HiLo, F22K_Output_Off, F22K_Output_On
    ULONG       DiSEqC_Port;            // DiSEqC_NULL | DiSEqC_A | DiSEqC_B | DiSEqC_C | DiSEqC_D
    ULONG       Modulation;            
    ULONG       Reserved[8];            
} DVB_T2S_MAPPING_ENTRY_S2, *P_DVB_T2S_MAPPING_ENTRY_S2;


typedef struct _LNB_DATA
{
    UCHAR           LNB_POWER;              // LNB_POWER_ON | LNB_POWER_OFF

    UCHAR           Tone_Data_Burst;        // Tone_Data_OFF | Tone_Burst_ON | Data_Burst_ON 

    ULONG           ulLNBLOFLowBand;        // LOF Low Band, in MHz, Note:BDA standard I/F in KHz.
    ULONG           ulLNBLOFHighBand;       // LOF High Band, in MHz
    ULONG           ulLNBLOFHiLoSW;         // LOF High/Low Band Switch Freq, in MHz

    UCHAR           f22K_Output;	    // 22KHz tone Control: F22K_Output_HiLo, F22K_Output_Off, F22K_Output_On

    UCHAR           DiSEqC_Port;            // DiSEqC_NULL | DiSEqC_A | DiSEqC_B | DiSEqC_C | DiSEqC_D
} LNB_DATA, *P_LNB_DATA;



typedef struct _DiSEqC_DATA
{
    INT             command_len;           // 3, 4, 5
    UCHAR           command[12];           // DiSEqC command 3, 4, 5
} DiSEqC_DATA, *P_DiSEqC_DATA;
    

#define LOCK_NOT_WAIT_RESULT                0
#define LOCK_WAIT_RESULT                    1

typedef struct _TURNER_VALUE
{
	ULONG           Frequency;              // DVB-S in Khz, Original Frequency, (not offseted by LOF)
	                                        // DVB-T, DVB-C 50000-860000Khz
	union {
        ULONG       SymbolRate;		        // in Ksps  
        ULONG       Bandwidth;			// 6000/7000/8000 in KHz  
    };
    ULONG           QAMSize;                 // 0/8/16/32/64/128/256
	UCHAR           WaitStatus;             // LOCK_NOT_WAIT_RESULT | LOCK_WAIT_RESULT
} TURNER_VALUE, *P_TURNER_VALUE;


#define Tuner_Lock                          1
#define Tuner_UnLock                        0

typedef struct _SIGNAL_DATA
{
    UCHAR           Quality;                // 0-99
    UCHAR           Strength;               // 0-99
    UCHAR           Lock_Status;            // Tuner_Lock | Tuner_UnLock
    UCHAR           Lock_Status_2;          // reserved, by products
}   SIGNAL_DATA, *P_SIGNAL_DATA;

typedef struct _RING_BUF_STATUS
{
    ULONG           Ring_Buf_Size;          // The TS Ring Buffer size in driver 
    ULONG           Ring_Buf_Head;          // The TS Ring Buffer Head, 0-(Ring_Buf_Size-1)
    ULONG           Ring_Buf_Tail;          // The TS Ring Buffer End,  0-(Ring_Buf_Size-1)
    ULONG           OverFlowCnt;            // # of times for buffer overflow 
    ULONG           OverFlowSize;           // TS data discarded
    UCHAR           Reset_Flag;             // 1:Reset overflowcnt/size after call, 0:no reset
} RING_BUF_STATUS, *P_RING_BUF_STATUS;

typedef struct _CAPTURE_DATA
{
    PUCHAR          TS_Buf;                 // in User space
    ULONG           TS_Buf_Size;            // 
    ULONG           Return_TS_Size;         // Actually TS data returned from driver to user 
} CAPTURE_DATA, *P_CAPTURE_DATA;



typedef struct _REMOTE_EVENT
{
    HANDLE          hEvent;
} REMOTE_EVENT, *PREMOTE_EVENT;


typedef struct _THBDAREGPARAMS
{
    ULONG           ulreserved1;
    ULONG           ulreserved2; 
    ULONG           ulDisableOffFreqScan;   // 0:Normal, 
                                            // 1:Disable off center-frequncy scan (+-167KHz/+-125KHz)
    ULONG           ulRelockMonitor;        // Relock Monitor enable flag
    ULONG           ulreserved3; 
    ULONG           ulreserved4;
    ULONG           ulreserved5; 
    ULONG           ulreserved6; 
    ULONG           ulreserved7;
    ULONG           ulreserved8;          
    ULONG           ulATSCFreqShift;        // ATSC frequency shift
} THBDAREGPARAMS, *PTHBDAREGPARAMS;


typedef struct _EE_IO_DATA
{
    DWORD           Address;
    DWORD           Value;
} EE_IO_DATA, *P_EE_IO_DATA;


typedef struct _DriverInfo 
{
    UCHAR           Version_Major;           // in BCD Ex., 3.2    =====> 0x32
    UCHAR           Version_Minor;           // 2.1    =====> 0x21
    UCHAR           FW_Version_Major;        // in BCD Ex., 10
    UCHAR           FW_Version_Minor;        //             05   =====> 1.0b05
    CHAR            Date_Time[22];           // Ex.,"2004-12-20 18:30:00" or  "DEC 20 2004 10:22:10"  with compiler __DATE__ and __TIME__  definition s
    CHAR            Company[8];              // Ex.,"DTV-DVB" 
    CHAR            SupportHWInfo[32];       // Ex.,"PCI DVB CX-878 with MCU series", "PCI ATSC CX-878 with MCU series", "7020/7021 USB-Sat", , "7045/7046 USB-Ter",.....................
    CHAR           CI_MMI_Flag;             //Bit 0:  0:No event mode support 1:Event mode supported
    CHAR           CI_MMI_Flag_Reserved;    //
    DWORD           SimuType;                // 0: Physical, 1: Virtual DVB-S, 2: Virtual DVB-T, 4: Virtual DVB-C, 8: Virtual ATSC 16:MCE Virtual DVB-T
    CHAR            Reserved[184];
} DriverInfo, *P_DriverInfo;

#define DEVICE_TYPE_DVBS                    0x00000001
#define DEVICE_TYPE_DVBT                    0x00000002
#define DEVICE_TYPE_DVBC                    0x00000004
#define DEVICE_TYPE_ATSC                    0x00000008
#define DEVICE_TYPE_ANNEX_C                 0x00000010       //US OpenCable
#define DEVICE_TYPE_ISDB_T                  0x00000020
#define DEVICE_TYPE_ISDB_S                  0x00000040

#define DEVICE_TYPE_PAL                     0x00000100
#define DEVICE_TYPE_NTSC                    0x00000200
#define DEVICE_TYPE_SECAM                   0x00000400
#define DEVICE_TYPE_SVIDEO                  0x00000800
#define DEVICE_TYPE_COMPOSITE               0x00001000
#define DEVICE_TYPE_FM                      0x00002000
#define DEVICE_TYPE_RC                      0x80000000     //Support Remote controller


#define Device_Speed_PCI                    0xff
#define Device_Speed_PCI_e                  0xfe  //PCI Express
#define Device_Speed_LOW                    0   //USB 1.1 low
#define Device_Speed_FULL                   1   //USB 1.1 full
#define Device_Speed_HIGH                   2   //USB 2.0 high

#define UNSUPPORT_CI                        0
#define SUPPORT_CI                          1   //PCI 878 MCU
#define SUPPORT_CI_NEW                      2   //Mantis


typedef struct _DEVICE_INFO
{
    CHAR            Device_Name[32];        // Ex., 1020, 3020C, 7045...
    ULONG           Device_TYPE;            // DEVICE_TYPE_DVBS, DEVICE_TYPE_DVBT, DEVICE_TYPE_DVBC...
    UCHAR           Device_Speed;           // Device_Speed_PCI, Device_Speed_FULL, Device_Speed_HIGH
    UCHAR           MAC_ADDRESS[6];
    UCHAR           CI_Support;              //  UNSUPPORT_CI | SUPPORT_CI |SUPPORT_CI_NEW 
    INT             TS_Packet_Len;          // 188 | 204    
    CHAR            PID_Filter;             //0:No pid filter   1:Pidfilter
    CHAR            PID_Filter_Bypass;      //0:No Bypass mode  1:Bypass mode supported
    CHAR            Reserved[188];
} DEVICE_INFO, *P_DEVICE_INFO;

#define MAX_PIDS_TABLE_NUM 32
enum {
    PID_FILTER_MODE_PASS=0,                 // only set PIDs pass through
    PID_FILTER_MODE_DISABLE,                // Disable PLD, let all TS pass through
    PID_FILTER_MODE_FILTER                  // only set PIDs can't pass through
};

typedef struct _PID_FILTER_INFO 
{
    BYTE            PIDFilterMode;          // PID_FILTER_MODE_PASS | PID_FILTER_MODE_DISABLE | PID_FILTER_MODE_FILTER
    BYTE            MaxPidsNum;             // Pid max number that HW & Fw afford!
    ULONG           CurPidValidMap;         // Pid valid index, ex CurPidValidMap = 0x00000003 means Pid value index 0, 1 are valid
    WORD            PidValue[MAX_PIDS_TABLE_NUM];     // Pid value table
} PID_FILTER_INFO, *P_PID_FILTER_INFO;


// Old CI messages, used in 878 MCU series, CI_Support==SUPPORT_CI
#define ME0						1
#define ME1						2
#define MMI0					3
#define MMI1					4
#define MMI0_ClOSE				5
#define MMI1_CLOSE				6
#define NON_CI_INFO				0

//used in MANTIS, CI_Support==SUPPORT_CI_NEW
enum CIMessage
{
// Old CI messages
	CI_STATUS_EMPTY_OLD = 0,		// NON_CI_INFO      0
	CI_STATUS_CAM_OK1_OLD,			// ME0				1
	CI_STATUS_CAM_OK2_OLD,			// ME1				2
	
	MMI_STATUS_GET_MENU_OK1_OLD,	// MMI0				3
	MMI_STATUS_GET_MENU_OK2_OLD,	// MMI1				4
	MMI_STATUS_GET_MENU_CLOSE1_OLD,	// MMI0_ClOSE		5
	MMI_STATUS_GET_MENU_CLOSE2_OLD,	// MMI1_ClOSE		6

// New CI messages
	CI_STATUS_EMPTY = 10,		// No CAM inserted
	CI_STATUS_INSERTED,			// CAM is inserted
	CI_STATUS_CAM_OK,			// Initila CAM OK
	CI_STATUS_CAM_UNKNOW,		// Unkonw CAM type

	MMI_STATUS_ANSWER_SEND,		// Communicating with CAM 
	MMI_STATUS_GET_MENU_OK,		// Get information from CAM
	MMI_STATUS_GET_MENU_FAIL,	// Fail to get information from CAM
	MMI_STATUS_GET_MENU_INIT,   // Init MMI
	MMI_STATUS_GET_MENU_CLOSE,  // Close MMI
	MMI_STATUS_GET_MENU_CLOSED, // MMI Closed
};


// Get CI info. IOCTL cmd structure, ,CI_Support==SUPPORT_CI_NEW
typedef struct {
	ULONG ulCIState;		// CI/CAM status
    ULONG ulMMIState;		// MMI status
	ULONG ulPMTState;
	ULONG ulEventMessage;	// Current event status
	ULONG Reserved[8];
} THCIState, *PTHCIState;

// Get CI info.(old) IOCTL cmd structure, 878 MCU,CI_Support==SUPPORT_CI
typedef struct
{
    ULONG ulCIState;		// CI/CAM status
    ULONG ulMMIState;		// MMI status
} THCIStateOld, *PTHCIStateOld;


typedef struct AppInfoStruct
{
	unsigned int app_type;
	unsigned int application_manufacture;
	unsigned int manufacture_code;
	char application_info[64];
} THAppInfo, *PTHAppInfo;

typedef struct MMIInfoStruct
{
	char Header[256];
	char SubHeader[256];
	char ButtomLine[256];
	char MenuItem[9][42];
	int  ItemCount;

	BOOL EnqFlag;

	BOOL Blind_Answer;
	int  Answer_Text_Length;
	char Prompt[256];

	int  Answer;
	char AnswerStr[256];

    int  Type;
} THMMIInfo, *PTHMMIInfo;



typedef struct tag_SIMU_PARAM {
 UCHAR      TSSrcFileName[256];
 ULONG      BitRate;
 ULONG      Repeat;
} SIMU_PARAM,*PSIMU_PARAM;
 
typedef struct tag_SIMU_PROP {
 ULONG      Percent;
 ULONG      BitRate;
 ULONG      Repeat;  // 0: Do not Repeat, !0: Repeat
} SIMU_PROP,*PSIMU_PROP;

enum RAWCMDSSTYPE
{
    SESSION_TYPE_PROFILE = 0,   // Profile session command
    SESSION_TYPE_APP,           // Application session command
    SESSION_TYPE_CA,            // CA session command
    SESSION_TYPE_MMI            // MMI sesstion command
};

typedef struct _RAW_CMD_INFO
{
	DWORD   dwSessionType;      // Raw command session type
    PVOID   pRawCmdBuff;        // Raw command buffer
    DWORD   dwRawCmdBuffSize;   // Raw command buffer size
    DWORD   Reserved[5];
} RAW_CMD_INFO, *PRAW_CMD_INFO;

#endif //THBDA_IOCTL_H*/
  #endregion

  public class Twinhan : IDiSEqCController
  {

    #region guids
    readonly Guid THBDA_TUNER = new Guid("E5644CC4-17A1-4eed-BD90-74FDA1D65423");
    readonly Guid GUID_THBDA_CMD = new Guid("255E0082-2017-4b03-90F8-856A62CB3D67");
    readonly uint THBDA_IOCTL_CI_SEND_PMT = 0xaa000338;     //CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CHECK_INTERFACE = 0xaa0001e4; //CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CI_GET_STATE = 0xaa000320;    //CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CI_GET_PMT_REPLY = 0xaa000348;//CTL_CODE(THBDA_IO_INDEX, 210, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_SET_DiSEqC = 0xaa0001a0;//CTL_CODE(THBDA_IO_INDEX, 104, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    readonly uint THBDA_IOCTL_GET_DiSEqC = 0xaa0001a4;//CTL_CODE(THBDA_IO_INDEX, 105, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    readonly uint THBDA_IOCTL_SET_LNB_DATA = 0xaa000200;//CTL_CODE(THBDA_IO_INDEX, 128, METHOD_BUFFERED, FILE_ANY_ACCESS) 

    #endregion

    #region variables
    bool _initialized;
    bool _isTwinHanCard;
    bool _camPresent;
    IBaseFilter _captureFilter;
    IntPtr _ptrPmt;
    IntPtr _ptrDiseqc;
    IntPtr _ptrDwBytesReturned;
    IntPtr _thbdaBuf;
    IntPtr _ptrOutBuffer;
    IntPtr _ptrOutBuffer2;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Twinhan"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public Twinhan(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      _ptrPmt = Marshal.AllocCoTaskMem(8192);
      _ptrDwBytesReturned = Marshal.AllocCoTaskMem(20);
      _thbdaBuf = Marshal.AllocCoTaskMem(8192);
      _ptrOutBuffer = Marshal.AllocCoTaskMem(8192);
      _ptrOutBuffer2 = Marshal.AllocCoTaskMem(8192);
      _ptrDiseqc = Marshal.AllocCoTaskMem(8192);

      _captureFilter = tunerFilter;
      _initialized = false;
      _camPresent = false;
      _isTwinHanCard = false;
      if (_captureFilter != null)
      {
        _isTwinHanCard = IsTwinhan;
        if (_isTwinHanCard)
        {
          _camPresent = IsCamPresent();
          Log.Log.WriteFile("Cam detected:{0}", _camPresent);
        }
      }
      _initialized = true;
    }
    /// <summary>
    /// Reutns if the tuner specified in the constructor supports twinhan CI/CAM handling
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is twinhan compatible; otherwise, <c>false</c>.
    /// </value>
    public bool IsTwinhan
    {
      get
      {
        if (_initialized) return _isTwinHanCard;

        bool result = IsTwinhanCard();
        if (result)
        {
          if (IsCamPresent())
          {
            Log.Log.WriteFile("twinhan: CAM inserted");
          }
          //SetLnbData(false, 9750, 10600, 11700, 1, 0);
          //System.Threading.Thread.Sleep(100);
          //SetLnbData(true, 9750, 10600, 11700, 1, 0);
        }
        return result;
      }
    }

    /// <summary>
    /// Gets the status of the CAM and CI.
    /// </summary>
    /// <param name="CIState">State of the CI.</param>
    /// <param name="MMIState">State of the MMI.</param>
    public void GetCAMStatus(out uint CIState, out uint MMIState)
    {
      CIState = 0;
      MMIState = 0;
      /*
       typedef struct {
        ULONG ulCIState;
        ULONG ulMMIState;
      } THCIState, *PTHCIState;
      */
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        DirectShowLib.IKsPropertySet propertySet = pin as DirectShowLib.IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          try
          {
            int thbdaLen = 0x28;
            Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
            Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
            Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
            Marshal.WriteByte(_thbdaBuf, 8, 0x90);
            Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
            Marshal.WriteByte(_thbdaBuf, 10, 0x85);
            Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
            Marshal.WriteByte(_thbdaBuf, 12, 0x62);
            Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
            Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
            Marshal.WriteByte(_thbdaBuf, 15, 0x67);
            Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CI_GET_STATE);//control code
            Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero); //LPVOID inbuffer
            Marshal.WriteInt32(_thbdaBuf, 24, 0);                //DWORD inbuffersize
            Marshal.WriteInt32(_thbdaBuf, 28, _ptrOutBuffer.ToInt32()); //LPVOID outbuffer
            Marshal.WriteInt32(_thbdaBuf, 32, 4096);                //DWORD outbuffersize
            Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//LPVOID bytesreturned

            int hr = propertySet.Set(propertyGuid, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
            if (hr == 0)
            {
              int bytesReturned = Marshal.ReadInt32(_ptrDwBytesReturned);
              CIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 0);
              MMIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 4);
              Log.Log.WriteFile("twinhan:CI State:{0:X} MMI State:{1:X}", CIState, MMIState);
            }
            else
            {
              Log.Log.WriteFile("twinhan: unable to get CI State hr:{0:X}", hr);
            }
          }
          finally
          {
          }
        }
        //Marshal.ReleaseComObject(pin);
      }
    }
    /// <summary>
    /// Determines whether a cam is present or not
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if cam is present; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      if (_initialized) return _camPresent;
      uint CIState;
      uint MMIState;
      GetCAMStatus(out CIState, out MMIState);
      if (CIState != 0) return true;
      return false;
    }
    /// <summary>
    /// Determines whether the cam is ready
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if cam is ready; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      return true;
    }

    /// <summary>
    /// Determines whether this card is twinhan compatible
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if card is twinhan compatible; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTwinhanCard()
    {
      if (_initialized) return _isTwinHanCard;
      Log.Log.WriteFile("Twinhan: check for twinhan driver");

      bool success = false;

      try
      {
        int thbdaLen = 0x28;
        try
        {
          Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
          Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
          Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
          Marshal.WriteByte(_thbdaBuf, 8, 0x90);
          Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
          Marshal.WriteByte(_thbdaBuf, 10, 0x85);
          Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
          Marshal.WriteByte(_thbdaBuf, 12, 0x62);
          Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
          Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
          Marshal.WriteByte(_thbdaBuf, 15, 0x67);
          Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CHECK_INTERFACE);//control code
          Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero);
          Marshal.WriteInt32(_thbdaBuf, 24, 0);
          Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);
          Marshal.WriteInt32(_thbdaBuf, 32, 0);
          Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);

          IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
          if (pin != null)
          {
            DirectShowLib.IKsPropertySet propertySet = pin as DirectShowLib.IKsPropertySet;
            if (propertySet != null)
            {
              Guid propertyGuid = THBDA_TUNER;

              int hr = propertySet.Set(propertyGuid, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
              if (hr == 0)
              {
                Log.Log.WriteFile("twinhan card detected");
                success = true;
              }
              //Marshal.ReleaseComObject(propertySet);
            }
            //Marshal.ReleaseComObject(pin);
          }
        }
        finally
        {
        }
      }
      finally
      {

      }
      return success;
    }


    /// <summary>
    /// Gets the answer from the CAM after sending the PMT .
    /// </summary>
    /// <returns>string containing the CAM answer</returns>
    public string GetPmtReply()
    {
      if (IsCamPresent() == false)
      {
        return "";
      }

      for (int i = 0; i < 1024; ++i)
      {
        Marshal.WriteByte(_ptrPmt, i, 0);
      }
      int thbdaLen = 0x28;
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CI_GET_PMT_REPLY);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero);//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, 0);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, _ptrPmt.ToInt32());//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 1024);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned

      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          int back = Marshal.ReadInt32(_ptrDwBytesReturned);
          if (hr != 0)
          {
            Log.Log.WriteFile("GetPmtReply() failed 0x{0:X}", hr);
          }

          Log.Log.WriteFile("GetPmtReply() returned {0} bytes", back);
          Marshal.ReleaseComObject(propertySet);
          try
          {
            System.IO.File.Delete("c:\\pmtreply.dat");
          }
          catch (Exception)
          {
          }
          using (System.IO.FileStream stream = new System.IO.FileStream("c:\\pmtreply.dat", System.IO.FileMode.OpenOrCreate))
          {
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
            {
              for (int i = 0; i < 1024; ++i)
              {
                byte k = Marshal.ReadByte(_ptrPmt, i);
                writer.Write(k);
              }
              writer.Flush();
            }
          }

        }
        Marshal.ReleaseComObject(pin);
      }
      return "";
    }

    /// <summary>
    /// Sends the PMT to the CAM/CI module
    /// </summary>
    /// <param name="camType">Type of the cam.</param>
    /// <param name="videoPid">The video pid.</param>
    /// <param name="audioPid">The audio pid.</param>
    /// <param name="caPMT">The caPMT structure.</param>
    /// <param name="caPMTLen">The caPMT lenght</param>
    public void SendPMT(CamType camType, uint videoPid, uint audioPid, byte[] caPMT, int caPMTLen)
    {
      if (IsCamPresent() == false) return;
      int camNumber = (int)camType;


      Log.Log.WriteFile("Twinhan: send PMT cam:{0} len:{1} video:0x{2:X} audio:0x{3:X}", camType, caPMTLen, videoPid, audioPid);

      if (caPMT.Length == 0)
        return;
      string line = "";
      for (int i = 0; i < caPMTLen; ++i)
      {
        string tmp = String.Format("{0:X} ", caPMT[i]);
        line += tmp;
      }
      Log.Log.WriteFile("capmt:{0}", line);
      Marshal.Copy(caPMT, 0, _ptrPmt, caPMTLen);
      int thbdaLen = 0x28;
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CI_SEND_PMT);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, (int)_ptrPmt.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, caPMTLen);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 0);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned

      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          int back = Marshal.ReadInt32(_ptrDwBytesReturned);

          if (hr != 0)
          {
            Log.Log.WriteFile("Twinhan: cam failed 0x{0:X}", hr);
          }
          else
            Log.Log.WriteFile("Twinhan: cam returned ok 0x{0:X}", hr);
          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }

      //System.Threading.Thread.Sleep(1000);
      //GetPmtReply();

    }
    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      byte disEqcPort = (byte)BandTypeConverter.GetAntennaNr(channel);

      byte turnon22Khz = 0;
      Int32 LNBLOFLowBand = 9750;
      Int32 LNBLOFHighBand = 10600;
      Int32 LNBLOFHiLoSW = 11700;

      BandTypeConverter.GetDefaultLnbSetup(parameters, channel.BandType, out LNBLOFLowBand, out LNBLOFHighBand, out LNBLOFHiLoSW);
      if (LNBLOFHiLoSW == 0) LNBLOFHiLoSW = 18000; // dont use lo/hi switch...

      if (BandTypeConverter.IsHiBand(channel, parameters))
      {
        turnon22Khz = 2;
      }
      else
      {
        turnon22Khz = 1;
      }

      SetLnbData(true, LNBLOFLowBand, LNBLOFHighBand, LNBLOFHiLoSW, turnon22Khz, disEqcPort);
      SendDiseqcCommandTest(parameters, channel);
    }

    void SetLnbData(bool lnbPower, int LNBLOFLowBand, int LNBLOFHighBand, int LNBLOFHiLoSW, int turnon22Khz, int disEqcPort)
    {
      Log.Log.WriteFile("Twinhan: SetLnb diseqc port:{0} 22khz:{1} low:{2} hi:{3} switch:{4} power:{5}", disEqcPort, turnon22Khz, LNBLOFLowBand, LNBLOFHighBand, LNBLOFHiLoSW, lnbPower);
      int thbdaLen = 0x28;
      int disEqcLen = 20;
      Marshal.WriteByte(_ptrDiseqc, 0, (byte)(lnbPower ? 1 : 0));              // 0: LNB_POWER
      Marshal.WriteByte(_ptrDiseqc, 1, 0);              // 1: Tone_Data_Burst (Tone_Data_OFF:0 | Tone_Burst_ON:1 | Data_Burst_ON:2)
      Marshal.WriteByte(_ptrDiseqc, 2, 0);
      Marshal.WriteByte(_ptrDiseqc, 3, 0);
      Marshal.WriteInt32(_ptrDiseqc, 4, LNBLOFLowBand); // 4: ulLNBLOFLowBand   LNBLOF LowBand MHz
      Marshal.WriteInt32(_ptrDiseqc, 8, LNBLOFHighBand);// 8: ulLNBLOFHighBand  LNBLOF HighBand MHz
      Marshal.WriteInt32(_ptrDiseqc, 12, LNBLOFHiLoSW); //12: ulLNBLOFHiLoSW   LNBLOF HiLoSW MHz
      Marshal.WriteByte(_ptrDiseqc, 16, (byte)turnon22Khz);   //16: f22K_Output (F22K_Output_HiLo:0 | F22K_Output_Off:1 | F22K_Output_On:2
      Marshal.WriteByte(_ptrDiseqc, 17, (byte)disEqcPort);    //17: DiSEqC_Port
      Marshal.WriteByte(_ptrDiseqc, 18, 0);
      Marshal.WriteByte(_ptrDiseqc, 19, 0);

      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_SET_LNB_DATA);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, (int)_ptrDiseqc.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, disEqcLen);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 0);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned

      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          if (hr != 0)
          {
            Log.Log.WriteFile("TwinHan SetLNB failed 0x{0:X}", hr);
          }
          else
            Log.Log.WriteFile("TwinHan SetLNB ok 0x{0:X}", hr);
          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }
    }

    #region IDiSEqCController Members

    public void SendDiseqcCommandTest(ScanParameters parameters, DVBSChannel channel)
    {
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	


      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);

      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);

      byte[] diseqc = new byte[4];
      diseqc[0] = 0xe0;
      diseqc[1] = 0x10;
      diseqc[2] = 0x38;
      diseqc[3] = cmd;
      SendDiSEqCCommand(diseqc);
    }

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      int thbdaLen = 0x28;
      int disEqcLen = 16;
      for (int i = 0; i < 12; ++i)
        Marshal.WriteByte(_ptrDiseqc, 4 + i, 0);

      Marshal.WriteInt32(_ptrDiseqc, 0, (int)diSEqC.Length);//command len
      for (int i = 0; i < diSEqC.Length; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, 4 + i, diSEqC[i]);
      }

      string line = "";
      for (int i = 0; i < disEqcLen; ++i)
      {
        byte k = Marshal.ReadByte(_ptrDiseqc, i);
        line += String.Format("{0:X} ", k);
      }
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_SET_DiSEqC);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, (int)_ptrDiseqc.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, disEqcLen);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 0);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned

      bool success = false;
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          if (hr != 0)
          {
            Log.Log.WriteFile("TwinHan DiSEqC cmd:{0} failed 0x{1:X}", line, hr);
          }
          else
          {
            Log.Log.WriteFile("TwinHan DiSEqC cmd:{0} succeeded", line);
            success = true;
          }
          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }
      return success;
    }

    /// <summary>
    /// Sends a diseqc command and reads a reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = new byte[1];
      reply[0] = 0;
      int thbdaLen = 0x28;
      int disEqcLen = 16;
      for (int i = 0; i < 16; ++i)
        Marshal.WriteByte(_ptrDiseqc, i, 0);

      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_GET_DiSEqC);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, 0);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)_ptrDiseqc.ToInt32());//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, disEqcLen);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned

      bool success = false;
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          if (hr != 0)
          {
            Log.Log.WriteFile("TwinHan get DiSEqC failed 0x{0:X}", hr);
          }
          else
          {
            Log.Log.WriteFile("TwinHan get DiSEqC ok 0x{0:X}", hr);
            success = true;
          }

          string line = "";
          for (int i = 0; i < 16; ++i)
          {
            byte k = Marshal.ReadByte(_ptrDiseqc, i);
            line += String.Format("{0:X} ", k);
          }
          Log.Log.Write("reply:{0}", line);

          success = true;

          int bytesReturned = Marshal.ReadInt32(_ptrDiseqc);
          if (bytesReturned > 0)
          {
            reply = new byte[bytesReturned];
            for (int i = 0; i < bytesReturned; ++i)
            {
              reply[i] = Marshal.ReadByte(_ptrDiseqc, 4 + i);
            }
          }

          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }
      return success;
    }

    #endregion
  }
}
