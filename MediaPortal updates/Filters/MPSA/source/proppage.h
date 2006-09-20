//------------------------------------------------------------------------------
// File: proppage.h
//
// Desc: DirectShow sample code - definition of MPDSTProperties class.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

#ifndef __PROPPAGE_
#define __PROPPAGE_
#pragma warning(disable: 4511 4512 4995)

#include "mpsa.h"

class MPDSTProperties : public CBasePropertyPage
{

public:

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN lpunk, HRESULT *phr);
    DECLARE_IUNKNOWN;

private:

    BOOL OnReceiveMessage(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam);

    HRESULT OnConnect(IUnknown *pUnknown);
    HRESULT OnDisconnect();
    HRESULT OnActivate();
    HRESULT OnApplyChanges();

    void SetDirty();
    void FillListBox();

    MPDSTProperties(LPUNKNOWN lpunk, HRESULT *phr);

    HWND        m_hwndLB ;       // Handle of the list box
	HWND		m_editA;
	HWND		m_editAC3;
	HWND		m_editV;
	HWND		m_editT;
	HWND		m_editTSID;
	HWND		m_editONID;
	HWND		m_editPCR;
	HWND		m_editPMT;
	HWND		m_editPROV;
	HWND		m_checkATSC;
	HWND		m_editMajor;
	HWND		m_editMinor;
	HWND        m_editProg;
    int         m_nIndex ;       // Index of the selected media type
    IPin        *m_pPin ;        // The upstream output pin connected to us
    IStreamAnalyzer   *m_pIMPDST;    // Null In Place property interface

};  // class MPDSTProperties

#endif