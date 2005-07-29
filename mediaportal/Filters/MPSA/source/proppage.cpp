//------------------------------------------------------------------------------
// File: NullProp.cpp
//
// Desc: DirectShow sample code - implementation of MPDSTProperties class.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------


// Eliminate two expected level 4 warnings from the Microsoft compiler.
// The class does not have an assignment or copy operator, and so cannot
// be passed by value.  This is normal.  This file compiles clean at the
// highest (most picky) warning level (-W4).
#pragma warning(disable: 4511 4512)

#include <streams.h>
#include <windowsx.h>
#include <commctrl.h>
#include <olectl.h>
#include <memory.h>
#include <stdlib.h>
#include <stdio.h>
#include <tchar.h>

#include "resource.h"    // ids used in the dialog
#include "mpsa.h"    // public guids
#include "proppage.h"    // our own class


//
// CreateInstance
//
// Override CClassFactory method.
// Set lpUnk to point to an IUnknown interface on a new MPDSTProperties object
// Part of the COM object instantiation mechanism
//
CUnknown * WINAPI MPDSTProperties::CreateInstance(LPUNKNOWN lpunk, HRESULT *phr)
{
    ASSERT(phr);
    
    CUnknown *punk = new MPDSTProperties(lpunk, phr);

    if (punk == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return punk;
}


//
// MPDSTProperties::Constructor
//
// Constructs and initialises a MPDSTProperties object
//
MPDSTProperties::MPDSTProperties(LPUNKNOWN pUnk, HRESULT *phr)
: CBasePropertyPage(NAME("Stream Analyzer Property Page\0"),pUnk, IDD_DIALOG1, IDS_TITLE)
    , m_pPin(NULL)
    , m_nIndex(0)
    , m_pIMPDST(NULL)
{
    ASSERT(phr);

} // (constructor) MPDSTProperties


//
// SetDirty
//
// Sets m_hrDirtyFlag and notifies the property page site of the change
//
void MPDSTProperties::SetDirty()
{
    m_bDirty = TRUE;

    if (m_pPageSite)
        m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);

} // SetDirty


//
// OnReceiveMessage
//
// Override CBasePropertyPage method.
// Handles the messages for our property window
//
BOOL MPDSTProperties::OnReceiveMessage(HWND hwnd,
                                        UINT uMsg,
                                        WPARAM wParam,
                                        LPARAM lParam)
{
    switch (uMsg)
    {
        case WM_INITDIALOG:
        {
            // get the hWnd of the list box
            m_hwndLB = GetDlgItem (hwnd, IDC_MEDIALIST) ;
            m_editA = GetDlgItem (hwnd, IDC_EDIT1) ;
            m_editV = GetDlgItem (hwnd, IDC_EDIT2) ;
            m_editT = GetDlgItem (hwnd, IDC_EDIT3) ;
			m_editTSID= GetDlgItem (hwnd, IDC_EDIT4) ;
			m_editONID= GetDlgItem (hwnd, IDC_EDIT5) ;
			m_editPCR= GetDlgItem (hwnd, IDC_EDIT6) ;
			m_editPMT= GetDlgItem (hwnd, IDC_EDIT7) ;
			m_editPROV= GetDlgItem (hwnd, IDC_EDIT8) ;
			m_checkATSC=GetDlgItem(hwnd,IDC_CHECK1);

            FillListBox();

            return (LRESULT) 1;
        }

        case WM_COMMAND:
        {
           DWORD hiparam=HIWORD(wParam);
		   DWORD loparam=LOWORD(wParam);

		   
			if (HIWORD(wParam) == LBN_SELCHANGE)
                SetDirty();

			if(loparam==IDC_CHECK1)
			{
				int res=SendMessage(m_checkATSC,BM_GETCHECK,0,0);
				if(res)
					m_pIMPDST->UseATSC(TRUE);
				else
					m_pIMPDST->UseATSC(FALSE);
			}

			if(loparam==IDC_MEDIALIST)
			{
				OnApplyChanges();
			}
			if(uMsg==273 && wParam==1002)
			{
				ASSERT(m_pIMPDST);
				m_pIMPDST->ResetParser();
				FillListBox();
			}
            return (LRESULT) 1;
        }

    }

    return CBasePropertyPage::OnReceiveMessage(hwnd,uMsg,wParam,lParam);

} // OnReceiveMessage


//
// OnConnect
//
// Override CBasePropertyPage method.
// Notification of which object this property page should display.
// We query the object for the IMPDST interface.
//
// If cObjects == 0 then we must release the interface.
// Set the member variable m_pPin to the upstream output pin connected
// to our input pin (or NULL if not connected).
//
HRESULT MPDSTProperties::OnConnect(IUnknown *pUnknown)
{
    ASSERT(m_pIMPDST == NULL);
    CheckPointer(pUnknown,E_POINTER);

    HRESULT hr = pUnknown->QueryInterface(IID_IStreamAnalyzer, (void **) &m_pIMPDST);
    if (FAILED(hr))
    {
        return E_NOINTERFACE;
    }

    ASSERT(m_pIMPDST);
    ASSERT(!m_pPin);

    return (m_pIMPDST->get_IPin(&m_pPin));

} // OnConnect


//
// OnDisconnect
//
// Override CBasePropertyPage method.
// Release the private interface, release the upstream pin.
//
HRESULT MPDSTProperties::OnDisconnect()
{
    // Release of Interface

    if (m_pIMPDST == NULL)
        return E_UNEXPECTED;

    m_pIMPDST->Release();
    m_pIMPDST = NULL;

    //
    // Release the pin interface that we are holding.
    //
    if (m_pPin)
    {
        m_pPin->Release() ;
        m_pPin = NULL ;
    }
    return NOERROR;

} // OnDisconnect


//
// Activate
//
// We are being activated
//
HRESULT MPDSTProperties::OnActivate()
{
    SendMessage (m_hwndLB, LB_SETCURSEL, m_nIndex, 0);
    return NOERROR;

} // Activate


//
// OnApplyChanges
//
// Changes made should be kept.
//
HRESULT MPDSTProperties::OnApplyChanges()
{

    ASSERT(m_pIMPDST);

    //
    // get the current selection of the media type
    //
    int iIndex = SendMessage (m_hwndLB, LB_GETCURSEL, 0, 0) ;
    if (iIndex <= 0)
        iIndex =0 ;

	Sections::ChannelInfo ch;

	HRESULT hr;
	hr=m_pIMPDST->GetChannel(iIndex,(BYTE*)&ch);
	if(hr==S_OK)
	{
		m_bDirty = FALSE;            // the page is now clean
		m_nIndex = iIndex;

		char buffer[255];


	// audio
	_i64toa((__int64)ch.Pids.AudioPid1,buffer,10);
	SendMessage( m_editA, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editA, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	// video
	_i64toa((__int64)ch.Pids.VideoPid,buffer,10);
	SendMessage( m_editV, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editV, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	//ttxt
	_i64toa((__int64)ch.Pids.Teletext,buffer,10);
	SendMessage( m_editT, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editT, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	//ttxt
	_i64toa((__int64)ch.TransportStreamID,buffer,10);
	SendMessage( m_editTSID, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editTSID, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	//ttxt
	_i64toa((__int64)ch.ProgrammNumber,buffer,10);
	SendMessage( m_editONID, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editONID, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	//ttxt
	_i64toa((__int64)ch.PCRPid,buffer,10);
	SendMessage( m_editPCR, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editPCR, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	//ttxt
	_i64toa((__int64)ch.ProgrammPMTPID,buffer,10);
	SendMessage( m_editPMT, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editPMT, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	//ttxt
	strcpy(buffer,(char*)ch.ProviderName);
	SendMessage( m_editPROV, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
	SendMessage( m_editPROV, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	}


    return NOERROR;

} // OnApplyChanges


//
// FillListBox
//
// Fill the list box with an enumeration of the media type
//
void MPDSTProperties::FillListBox()
{
    TCHAR szBuffer[255];
    int Loop = 0, wextent = 0;
    SIZE extent={0};

    //
    // get the current media type
    //

	m_nIndex = 0 ;

    //
    // Fill the first entry of the list box with a choice to select any media.
    //
    LoadString(g_hInst, IDS_ANYTYPE, szBuffer, 255);

    //
    // if the filter is not connected on the input, nothing more to fill
    // also return if we haven't gotten any pin interface
    //
    HDC hdc = GetDC (m_hwndLB) ;
	WORD count;
	Sections::ChannelInfo ch;
	HRESULT hr=m_pIMPDST->GetChannelCount(&count);
	WORD len;
	m_pIMPDST->GetCISize(&len);
	BYTE *chh=new BYTE[len];
    for(int n=0;n<count-1;n++)
	{
			if(m_pIMPDST->GetChannel(n,chh)==S_OK)
			{
				
				memcpy(&ch,chh,len);
				strcpy(szBuffer,(char*)&ch.ServiceName[0]);
				GetTextExtentPoint (hdc, szBuffer, lstrlen(szBuffer), &extent) ;
				if (extent.cx > wextent)
					wextent = extent.cx ;
				SendMessage (m_hwndLB, LB_ADDSTRING, 0, (LPARAM)szBuffer) ;
			}
	}

    SendMessage (m_hwndLB, LB_SETHORIZONTALEXTENT, wextent, 0) ;
    SendMessage (m_hwndLB, LB_SETCURSEL, m_nIndex, 0) ;
    ASSERT(m_pIMPDST);

	BOOL bYesNo;
	m_pIMPDST->IsATSCUsed(&bYesNo);
	if (bYesNo)
		SendMessage(m_checkATSC,BM_SETCHECK,BST_CHECKED,0);
	else
		SendMessage(m_checkATSC,BM_SETCHECK,BST_UNCHECKED,0);
    //
    // if the filter is in a running state, disable the list box and allow
    // no input.
    //

} // FillListBox


