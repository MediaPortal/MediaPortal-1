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

#ifndef BDAAPI_CIMSG_H
#define BDAAPI_CIMSG_H

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

//////////////////////////////////////////
// slot status ///////////////////////////
//////////////////////////////////////////
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

//////////////////////////////////////////
// error codes ///////////////////////////
//////////////////////////////////////////
/// No error.
#define	ERR_NONE                    0
#define	ERR_WRONG_FLT_INDEX         1
#define	ERR_SET_FLT                 2
#define	ERR_CLOSE_FLT               3
#define	ERR_INVALID_DATA            4
#define	ERR_NO_CA_RESOURCE          5

#endif // #ifndef BDAAPI_CIMSG_H

// eof
