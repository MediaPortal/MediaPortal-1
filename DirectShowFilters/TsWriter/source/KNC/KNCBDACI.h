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
typedef BOOL __stdcall TKNCBDA_CI_Enable(int,IUnknown*,PVOID);
typedef BOOL __stdcall TKNCBDA_CI_Disable(int);
typedef BOOL __stdcall TKNCBDA_CI_IsAvailable(int);
typedef BOOL __stdcall TKNCBDA_CI_IsReady(int);
typedef BOOL __stdcall TKNCBDA_CI_HW_Enable(int,BOOL);
typedef BOOL __stdcall TKNCBDA_CI_GetName(int,LPTSTR,UINT);
typedef BOOL __stdcall TKNCBDA_CI_SendPMTCommand(int,PBYTE,int);
typedef BOOL __stdcall TKNCBDA_CI_EnterMenu(int,UCHAR);
typedef BOOL __stdcall TKNCBDA_CI_SelectMenu(int,UCHAR,UCHAR);
typedef BOOL __stdcall TKNCBDA_CI_CloseMenu(int,UCHAR);
typedef BOOL __stdcall TKNCBDA_CI_SendMenuAnswer(int,UCHAR,BOOL,LPCTSTR);


