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

#ifndef BDAAPITYPEDEFS_H
#define BDAAPITYPEDEFS_H

#include "bdaapi_TypSlotinfo.h" // typ_SlotInfo

#define BDG2_NAME                       TEXT("TechnoTrend BDA/DVB Capture")
#define BDG2_NAME_C_TUNER               TEXT("TechnoTrend BDA/DVB-C Tuner")
#define BDG2_NAME_S_TUNER               TEXT("TechnoTrend BDA/DVB-S Tuner")
#define BDG2_NAME_T_TUNER               TEXT("TechnoTrend BDA/DVB-T Tuner")
#define BDG2_NAME_NEW                   TEXT("ttBudget2 BDA DVB Capture")
#define BDG2_NAME_C_TUNER_NEW           TEXT("ttBudget2 BDA DVB-C Tuner")
#define BDG2_NAME_S_TUNER_NEW           TEXT("ttBudget2 BDA DVB-S Tuner")
#define BDG2_NAME_T_TUNER_NEW           TEXT("ttBudget2 BDA DVB-T Tuner")
#define BUDGET3NAME                     TEXT("TTHybridTV BDA Digital Capture")
#define BUDGET3NAME_TUNER               TEXT("TTHybridTV BDA DVBT Tuner")
#define BUDGET3NAME_ATSC_TUNER          TEXT("TTHybridTV BDA ATSC Tuner")
#define BUDGET3NAME_TUNER_ANLG          TEXT("TTHybridTV BDA Analog TV Tuner")
#define BUDGET3NAME_ANLG                TEXT("TTHybridTV BDA Analog Capture")
#define USB2BDA_DVB_NAME                TEXT("USB 2.0 BDA DVB Capture")
#define USB2BDA_DSS_NAME                TEXT("USB 2.0 BDA DSS Capture")
#define USB2BDA_DSS_NAME_TUNER          TEXT("USB 2.0 BDA DSS Tuner")
#define USB2BDA_DVB_NAME_C_TUNER        TEXT("USB 2.0 BDA DVB-C Tuner")
#define USB2BDA_DVB_NAME_S_TUNER        TEXT("USB 2.0 BDA DVB-S Tuner")
#define USB2BDA_DVB_NAME_S_TUNER_FAKE   TEXT("USB 2.0 BDA (DVB-T Fake) DVB-T Tuner")
#define USB2BDA_DVB_NAME_T_TUNER        TEXT("USB 2.0 BDA DVB-T Tuner")
#define USB2BDA_DVBS_NAME_PIN           TEXT("Pinnacle PCTV 4XXe Capture")
#define USB2BDA_DVBS_NAME_PIN_TUNER     TEXT("Pinnacle PCTV 4XXe Tuner")

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     Lists all possible and supported BDA devices. This enumerator is used
///     by the caller of bdaapiOpen() to open the correct interface to the
///     driver.
/////////////////////////////////////////////////////////////////////////////
typedef enum
{
    /// not set
    UNKNOWN = 0,
    /// Budget 2
    BUDGET_2,
    /// Budget 3 aka TT-budget T-3000
    BUDGET_3,
    /// USB 2.0
    USB_2,
    /// USB 2.0 Pinnacle
    USB_2_PINNACLE,
    /// USB 2.0 DSS
    USB_2_DSS,
    /// Budget 2 - new driver with only one sys file
    //BUDGET_2_NEW
} DEVICE_CAT;

/////////////////////////////////////////////////////////////////////////////
/// Lists all possible frontend types.
/////////////////////////////////////////////////////////////////////////////
typedef enum
{
    /// not set
    TYPE_FE_UNKNOWN = 0,
    /// DVB-C
    TYPE_FE_DVB_C,
    /// DVB-S
    TYPE_FE_DVB_S,
    /// DVB-S2
    TYPE_FE_DVB_S2,
    /// DVB-T
    TYPE_FE_DVB_T,
	/// ATSC
    TYPE_FE_ATSC,
    /// DSS
    TYPE_FE_DSS,
    /// DVB-C and DVB-T
    TYPE_FE_DVB_CT
} TYPE_FRONT_END;

/////////////////////////////////////////////////////////////////////////////
/// Lists the seller of the product.
/////////////////////////////////////////////////////////////////////////////
typedef enum
{
    /// not set
    PS_UNKNOWN = 0,
    /// TechnoTrend
    PS_TECHNOTREND,
    /// Technisat
    PS_TECHNISAT
} PRODUCT_SELLER;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     Lists all possible return values of bda api functions. 
/////////////////////////////////////////////////////////////////////////////
typedef enum
{
    /// operation finished successful
    RET_SUCCESS = 0,
    /// operation is not implemented for the opened handle
    RET_NOT_IMPL,
    /// operation is not supported for the opened handle
    RET_NOT_SUPPORTED,
    /// the given HANDLE seems not to be correct
    RET_ERROR_HANDLE,
    /// the internal IOCTL subsystem has no device handle
    RET_IOCTL_NO_DEV_HANDLE,
    /// the internal IOCTL failed
    RET_IOCTL_FAILED,
    /// the infrared interface is already initialised
    RET_IR_ALREADY_OPENED,
    /// the infrared interface is not initialised
    RET_IR_NOT_OPENED,
    /// length exceeds maximum in EEPROM-Userspace operation
    RET_TO_MANY_BYTES,
    /// common interface hardware error
    RET_CI_ERROR_HARDWARE,
    /// common interface already opened
    RET_CI_ALREADY_OPENED,
    /// operation finished with timeout
    RET_TIMEOUT,
    /// read psi failed
    RET_READ_PSI_FAILED,
    /// not set
    RET_NOT_SET,
    /// operation finished with general error
    RET_ERROR,
	/// operation finished with ilegal pointer
    RET_ERROR_POINTER
} TYPE_RET_VAL;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     Lists all possible LED states of USB bicolor LEDs.
/////////////////////////////////////////////////////////////////////////////
typedef enum
{
    TYPE_LED_RED = 0,
    TYPE_LED_GREEN
} TYPE_LED_COLOR;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     
/////////////////////////////////////////////////////////////////////////////
typedef enum
{
    TYPE_NO_FILTER_TYPE,
    TYPE_STREAMING_FILTER,
    TYPE_PIPING_FILTER,
    TYPE_PES_FILTER,
    TYPE_ES_FILTER,
    TYPE_SECTION_FILTER,
    TYPE_MPE_SECTION_FILTER,
    TYPE_PID_FILTER,
    TYPE_MULTI_PID_FILTER,
    TYPE_TS_FILTER,
    TYPE_MULTI_MPE_FILTER
} TYPE_FILTER;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     Is the connection type. Only needed for common interface.
/////////////////////////////////////////////////////////////////////////////
typedef enum _typ_ConnectionType
{
    /// phone
	LSC_PHONE    = 1,
    /// cable
	LSC_CABLE    = 2,
    /// internet
	LSC_INTERNET = 3,
    /// seriel
	LSC_SERIALL
} TYPE_CONNECTION;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     Is the connection description. Only needed for common interface.
/////////////////////////////////////////////////////////////////////////////
typedef struct _typ_ConnectionDesc
{
    /// type of connection
	TYPE_CONNECTION	ConnectionType;
    /// phone number / channel ID
	char			DialIn[MAX_PATH];
    /// IP of the client
	char			ClientIP[MAX_PATH];
    /// IP of the server
	char			ServerIP[MAX_PATH];
    /// TCP port number
	char			TcpPort[MAX_PATH];
    /// connection authentification ID
	char			ConnectAutID[MAX_PATH];
    /// username
	char			LogonUsername[MAX_PATH];
    /// password
	char			LogonPassword[MAX_PATH];
    /// retry count
	BYTE			RetryCount;
    /// timeout
	BYTE			Timeout10Ms;
} TYPE_CONNECT_DESCR;

/////////////////////////////////////////////////////////////////////////////
// Callback function pointers ///////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 Infrared callback function.
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param Buf      Contains the remote code. If RC5 then the low word is
///                 used. If RC6 then the whole DWORD is used.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PIRCBFCN) (PVOID  Context,
                          DWORD  *Buf);

/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 This callback funtion is called if the state of the 
///                 common interface is changed. E.g. a CAM is inserted into
///                 the common interface or removed.
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID. Should ever be '0' for pc products.
/// \param nStatus  Slot status. Defines are given by CI_SLOT_* in 
///                 bdaapi_CIMsg.h.
/// \param csInfo   Detailed slot informations.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnSlotStatus)(PVOID          Context,
                                        BYTE           nSlot,
                                        BYTE           nStatus,
                                        TYP_SLOT_INFO* csInfo);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                  This callback function gives the status of an CAM
///                  operation. This means for e.g. service description
///                  successful or not.
/// \param Context   Can be used for a context pointer in the calling
///                  application. This parameter can be NULL.
/// \param nSlot     Is the Slot ID.
/// \param nReplyTag Reply tag.
/// \param wStatus   Status.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnCAStatus)(PVOID Context,
                                      BYTE  nSlot,
                                      BYTE  nReplyTag,
                                      WORD  wStatus);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnDisplayString
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/// \param pString  String to display.
/// \param wLength  Length of the string to display.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnDisplayString)(PVOID Context,
                                                    BYTE  nSlot,
                                                    char* pString,
                                                    WORD  wLength);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                     PCBFCN_CI_OnDisplayMenu
/// \param Context      Can be used for a context pointer in the calling
///                     application. This parameter can be NULL.
/// \param nSlot        Is the Slot ID.
/// \param wItems       Number of menu items.
/// \param pStringArray Contains all strings of the menu.
/// \param wLength      Length of the string array.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnDisplayMenu)(PVOID Context,
                                                  BYTE  nSlot,
                                                  WORD  wItems,
                                                  char* pStringArray,
                                                  WORD  wLength);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                     PCBFCN_CI_OnDisplayList
/// \param Context      Can be used for a context pointer in the calling
///                     application. This parameter can be NULL.
/// \param nSlot        Is the Slot ID.
/// \param wItems       Number of Items in the List
/// \param pStringArray Contains all strings of the list.
/// \param wLength      Length of the string array.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnDisplayList)(PVOID Context,
                                                  BYTE  nSlot,
                                                  WORD  wItems,
                                                  char* pStringArray,
                                                  WORD  wLength);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnSwitchOsdOff
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnSwitchOsdOff)(PVOID Context,
                                                   BYTE  nSlot);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                        PCBFCN_CI_OnInputRequest
/// \param Context         Can be used for a context pointer in the calling
///                        application. This parameter can be NULL.
/// \param nSlot           Is the Slot ID.
/// \param bBlindAnswer    Is it a blind answer.
/// \param nExpectedLength Expected length.
/// \param dwKeyMask       Key mask.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnInputRequest)(PVOID Context,
                                                   BYTE  nSlot,
                                                   BOOL  bBlindAnswer,
                                                   BYTE  nExpectedLength, 
                                                   DWORD dwKeyMask);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                     PCBFCN_CI_OnLscSetDescriptor
/// \param Context      Can be used for a context pointer in the calling
///                     application. This parameter can be NULL.
/// \param nSlot        Is the Slot ID.
/// \param pDescriptor  Detailed connection description.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscSetDescriptor)(PVOID Context,
                                                       BYTE  nSlot,
                                                       TYPE_CONNECT_DESCR* pDescriptor);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnLscConnect
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscConnect)(PVOID Context,
                                                 BYTE  nSlot);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnLscDisconnect
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscDisconnect)(PVOID Context,
                                                    BYTE  nSlot);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                     PCBFCN_CI_OnLscSetParams
/// \param Context      Can be used for a context pointer in the calling
///                     application. This parameter can be NULL.
/// \param nSlot        Is the Slot ID.
/// \param BufferSize   Size of the buffer in bytes.
/// \param Timeout10Ms  Timeout in 10 ms steps.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscSetParams)(PVOID Context,
                                                   BYTE  nSlot,
                                                   BYTE  BufferSize,
                                                   BYTE  Timeout10Ms);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnLscEnquireStatus
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscEnquireStatus)(PVOID Context,
                                                       BYTE  nSlot);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnLscGetNextBuffer
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/// \param PhaseID  Phase ID.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscGetNextBuffer)(PVOID Context,
                                                       BYTE  nSlot,
                                                       BYTE  PhaseID);
/////////////////////////////////////////////////////////////////////////////
/// \brief
///                 PCBFCN_CI_OnLscTransmitBuffer
/// \param Context  Can be used for a context pointer in the calling
///                 application. This parameter can be NULL.
/// \param nSlot    Is the Slot ID.
/// \param PhaseID  Phase ID.
/// \param pData    Byte pointer to the data.
/// \param nLength  Length of data in bytes.
/////////////////////////////////////////////////////////////////////////////
typedef void (FAR PASCAL *PCBFCN_CI_OnLscTransmitBuffer)(PVOID Context,
                                                        BYTE  nSlot,
                                                        BYTE  PhaseID,
                                                        BYTE* pData,
                                                        WORD  nLength);

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     This structure contains all the possible callback function pointers
///     that the calling application can use. All function pointers are
///     optional. The context elements can be used for context pointer or
///     something else.
/////////////////////////////////////////////////////////////////////////////
typedef struct
{
    /// PCBFCN_CI_OnSlotStatus
    PCBFCN_CI_OnSlotStatus          p01;
    /// Context pointer for PCBFCN_CI_OnSlotStatus
    PVOID                           p01Context;
    /// PCBFCN_CI_OnCAStatus
    PCBFCN_CI_OnCAStatus            p02;
    /// Context pointer for PCBFCN_CI_OnCAStatus
    PVOID                           p02Context;
    /// PCBFCN_CI_OnDisplayString
    PCBFCN_CI_OnDisplayString       p03;
    /// Context pointer for PCBFCN_CI_OnDisplayString
    PVOID                           p03Context;
    /// PCBFCN_CI_OnDisplayMenu
    PCBFCN_CI_OnDisplayMenu         p04;
    /// Context pointer for PCBFCN_CI_OnDisplayMenu
    PVOID                           p04Context;
    /// PCBFCN_CI_OnDisplayList
    PCBFCN_CI_OnDisplayList         p05;
    /// Context pointer for PCBFCN_CI_OnDisplayList
    PVOID                           p05Context;
    /// PCBFCN_CI_OnSwitchOsdOff
    PCBFCN_CI_OnSwitchOsdOff        p06;
    /// Context pointer for PCBFCN_CI_OnSwitchOsdOff
    PVOID                           p06Context;
    /// PCBFCN_CI_OnInputRequest
    PCBFCN_CI_OnInputRequest        p07;
    /// Context pointer for PCBFCN_CI_OnInputRequest
    PVOID                           p07Context;
    /// PCBFCN_CI_OnLscSetDescriptor
    PCBFCN_CI_OnLscSetDescriptor    p08;
    /// Context pointer for PCBFCN_CI_OnLscSetDescriptor
    PVOID                           p08Context;
    /// PCBFCN_CI_OnLscConnect
    PCBFCN_CI_OnLscConnect          p09;
    /// Context pointer for PCBFCN_CI_OnLscConnect
    PVOID                           p09Context;
    /// PCBFCN_CI_OnLscDisconnect
    PCBFCN_CI_OnLscDisconnect       p10;
    /// Context pointer for PCBFCN_CI_OnLscDisconnect
    PVOID                           p10Context;
    /// PCBFCN_CI_OnLscSetParams
    PCBFCN_CI_OnLscSetParams        p11;
    /// Context pointer for PCBFCN_CI_OnLscSetParams
    PVOID                           p11Context;
    /// PCBFCN_CI_OnLscEnquireStatus
    PCBFCN_CI_OnLscEnquireStatus    p12;
    /// Context pointer for PCBFCN_CI_OnLscEnquireStatus
    PVOID                           p12Context;
    /// PCBFCN_CI_OnLscGetNextBuffer
    PCBFCN_CI_OnLscGetNextBuffer    p13;
    /// Context pointer for PCBFCN_CI_OnLscGetNextBuffer
    PVOID                           p13Context;
    /// PCBFCN_CI_OnLscTransmitBuffer
    PCBFCN_CI_OnLscTransmitBuffer   p14;
    /// Context pointer for PCBFCN_CI_OnLscTransmitBuffer
    PVOID                           p14Context;
} TS_CiCbFcnPointer, *pTS_CiCbFcnPointer;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     This structure contains all the possible callback function pointers
///     that the calling application can use. All function pointers are
///     optional. The context elements can be used for context pointer or
///     something else.
/////////////////////////////////////////////////////////////////////////////
typedef struct
{
    /// PCBFCN_CI_OnSlotStatus
    PCBFCN_CI_OnSlotStatus          p01;
    /// Context pointer for PCBFCN_CI_OnSlotStatus
    PVOID                           p01Context;
    /// PCBFCN_CI_OnCAStatus
    PCBFCN_CI_OnCAStatus            p02;
    /// Context pointer for PCBFCN_CI_OnCAStatus
    PVOID                           p02Context;
} TS_CiCbFcnPointerSlim, *pTS_CiCbFcnPointerSlim;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     Lists the bda filternames.
/////////////////////////////////////////////////////////////////////////////
typedef struct
{
    char            szTunerFilterName[MAX_PATH];
    char            szTunerFilterName2[MAX_PATH];
    char            szCaptureFilterName[MAX_PATH];
    char            szAnlgTunerFilterName[MAX_PATH];
    char            szAnlgCaptureFilterName[MAX_PATH];
    char            szProductName[MAX_PATH];
    TYPE_FRONT_END  FeType;
} TS_FilterNames,  *pTS_FilterNames;

#endif // #ifndef BDAAPITYPEDEFS_H

// eof
