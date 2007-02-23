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

//CI MENU CALLBACK CLASS
class __declspec(dllimport)  CKNCBDACICallback
{
public:
	virtual void OnKncCiState(UCHAR slot,int State, LPCTSTR lpszMessage) = 0;
	virtual void OnKncCiOpenDisplay(UCHAR slot) = 0;
	virtual void OnKncCiMenu(UCHAR slot,LPCTSTR lpszTitle, LPCTSTR lpszSubTitle, LPCTSTR lpszBottom, UINT nNumChoices) = 0;
	virtual void OnKncCiMenuChoice(UCHAR slot,UINT nChoice, LPCTSTR lpszText) = 0;
	virtual void OnKncCiRequest(UCHAR slot,BOOL bBlind, UINT nAnswerLength, LPCTSTR lpszText) = 0;
	virtual void OnKncCiCloseDisplay(UCHAR slot,UINT nDelay) = 0;
};

//CI MAIN CONTROL
class __declspec(dllimport) CKNCBDACI
{
public:
	CKNCBDACI(void);
	~CKNCBDACI(void);

	//SHOULD BE CALLED FIRST (USE BDA TUNER FILTER)
	BOOL	KNCBDA_CI_Enable(IBaseFilter *pTunFilt,CKNCBDACICallback *pCallback);

	//SHOULD BE CALLED AT RELEASE
	BOOL    KNCBDA_CI_Disable();

	BOOL	KNCBDA_CI_IsAvailable();
	BOOL	KNCBDA_CI_IsReady();

	BOOL	KNCBDA_CI_HW_Enable(BOOL bDoIt);
	BOOL	KNCBDA_CI_GetName(LPTSTR lpszName, UINT cchMax);

	BOOL	KNCBDA_CI_SendPMTCommand(PBYTE pPmt, int nLen);

	BOOL	KNCBDA_CI_EnterMenu(UCHAR slot);
	BOOL	KNCBDA_CI_SelectMenu(UCHAR slot, UCHAR nSelection);
	BOOL	KNCBDA_CI_CloseMenu(UCHAR slot);
	BOOL	KNCBDA_CI_SendMenuAnswer(UCHAR slot, BOOL bCancel, LPCTSTR lpszText);
};
