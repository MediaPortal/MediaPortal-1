/************************************************************************************
*																					*
*	BDA CI CONTROL CLASS															*
*																					*
*************************************************************************************
*																					*
*	(c) 2007 by ODSoft multimedia													*
*																					*
*	Homepage: http://www.odsoft.org	Mail: service@odsoft.org						*
*																					*
************************************************************************************/

#pragma once

#include <strmif.h>

//CI STATES
#define KNC_BDA_CI_STATE_INITIALIZING			0
#define KNC_BDA_CI_STATE_TRANSPORT				1
#define KNC_BDA_CI_STATE_RESOURCE				2
#define KNC_BDA_CI_STATE_APPLICATION			3
#define KNC_BDA_CI_STATE_CONDITIONAL_ACCESS		4
#define KNC_BDA_CI_STATE_READY					5
#define KNC_BDA_CI_STATE_OPEN_SERVICE			6
#define KNC_BDA_CI_STATE_RELEASING				7
#define KNC_BDA_CI_STATE_CLOSE_MMI				8
#define KNC_BDA_CI_STATE_REQUEST				9
#define KNC_BDA_CI_STATE_MENU					10
#define KNC_BDA_CI_STATE_MENU_CHOICE			11
#define KNC_BDA_CI_STATE_OPEN_DISPLAY			12
#define KNC_BDA_CI_STATE_CLOSE_DISPLAY			13
#define KNC_BDA_CI_STATE_NONE					99

//CI MENU CALLBACK STRUCT
typedef struct TKNCBDACICallback
{
	PVOID pParam;
	void (*OnKncCiState)(UCHAR slot,int State,LPCTSTR lpszMessage,PVOID pParam);
	void (*OnKncCiOpenDisplay)(UCHAR slot,PVOID pParam);
	void (*OnKncCiMenu)(UCHAR slot,LPCTSTR lpszTitle,LPCTSTR lpszSubTitle,LPCTSTR lpszBottom,UINT nNumChoices,PVOID pParam);
	void (*OnKncCiMenuChoice)(UCHAR slot,UINT nChoice,LPCTSTR lpszText,PVOID pParam);
	void (*OnKncCiRequest)(UCHAR slot,BOOL bBlind,UINT nAnswerLength,LPCTSTR lpszText,PVOID pParam);
	void (*OnKncCiCloseDisplay)(UCHAR slot,UINT nDelay,PVOID pParam);	
}TKNCBDACICallback, *PKNCBDACICallback;

//FUNCTION TEMPLATES
typedef BOOL __stdcall TKNCBDA_CI_Enable(IUnknown*,PVOID);
typedef BOOL __stdcall TKNCBDA_CI_Disable();
typedef BOOL __stdcall TKNCBDA_CI_IsAvailable();
typedef BOOL __stdcall TKNCBDA_CI_IsReady();
typedef BOOL __stdcall TKNCBDA_CI_HW_Enable(BOOL);
typedef BOOL __stdcall TKNCBDA_CI_GetName(LPTSTR,UINT);
typedef BOOL __stdcall TKNCBDA_CI_SendPMTCommand(PBYTE,int);
typedef BOOL __stdcall TKNCBDA_CI_EnterMenu(UCHAR);
typedef BOOL __stdcall TKNCBDA_CI_SelectMenu(UCHAR,UCHAR);
typedef BOOL __stdcall TKNCBDA_CI_CloseMenu(UCHAR);
typedef BOOL __stdcall TKNCBDA_CI_SendMenuAnswer(UCHAR,BOOL,LPCTSTR);

