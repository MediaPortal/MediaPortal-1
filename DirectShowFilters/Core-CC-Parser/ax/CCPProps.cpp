//------------------------------------------------------------------------------
// File: CcPProp.cpp
//
// Desc: DirectShow sample code - implementation of CCcParserProperties class.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------


#include "StdAfx.h"

//#include <commctrl.h>
#include <olectl.h>
#include <memory.h>

#include "resource.h"
#include "iCcParser.h"
#include "CCPProps.h"


//
// CreateInstance
//
// Override CClassFactory method.
// Set lpUnk to point to an IUnknown interface on a new CCcParserProperties object
//
CUnknown * WINAPI CCcParserProperties::CreateInstance(LPUNKNOWN lpunk, HRESULT *phr)
{
    ASSERT(phr);
    
    CUnknown *punk = new CCcParserProperties(lpunk, phr);
    if (punk == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return punk;

} // CreateInstance


//
// CCcParserProperties constructor
//
CCcParserProperties::CCcParserProperties(LPUNKNOWN lpunk, HRESULT *phr)
    : CBasePropertyPage(NAME("CcParser Property Page"), lpunk, 
                        IDD_CCPPROPS, IDS_NAME)
    , m_idSink(0), m_timerUpdateCCType(0), m_hwndDlg(NULL)
{

}

CCcParserProperties::~CCcParserProperties()
{
	ASSERT( !IsFilterSet());
}

STDMETHODIMP CCcParserProperties::NonDelegatingQueryInterface(REFIID riid,void **ppv)
{
    if (riid == IID_ICcDataSink)
        return GetInterface( static_cast<ICcDataSink*>(this),ppv);

	return CBasePropertyPage::NonDelegatingQueryInterface(riid,ppv);
}

/////////////////////////////////////////////////////////////////////////////////////////////
// OnReceiveMessage

INT_PTR CCcParserProperties::OnReceiveMessage(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(lParam);

    switch (uMsg) 
    {
        case WM_INITDIALOG:
			ASSERT( NULL == m_hwndDlg );
			m_hwndDlg = hwnd;

            return OnInitDialog();

        case WM_TIMER:
			OnTimer( wParam );
            return TRUE;

        case WM_COMMAND:
		{
            if( LOWORD(wParam) == IDC_CHANNEL && 
				HIWORD(wParam) == CBN_SELCHANGE 
			  )
			{
				OnSelchangeChannel();
			}
            else if( LOWORD(wParam) == IDC_CLEAR && 
				     HIWORD(wParam) == BN_CLICKED
				   ) 
            {
				OnClear();
			}

            else if( LOWORD(wParam) == IDC_A53 && 
				     HIWORD(wParam) == BN_CLICKED
				   ) 
            {
				OnA53();
			}

			return TRUE;
		}

        case WM_DESTROY:

			ResetFilter();
			m_hwndDlg = NULL;
            return TRUE;

        default:

            return FALSE;

    } // switch

} // OnReceiveMessage


//
// OnConnect
//

HRESULT CCcParserProperties::OnConnect(IUnknown * punk)
{
    if (punk == NULL) {
        DbgBreak("You can't call OnConnect with a NULL pointer!!");
        return(E_POINTER);
    }

    ASSERT( !m_pifCcParser );

	HRESULT hr = punk->QueryInterface(IID_ICcParser, (void **)m_pifCcParser.AcceptHere());
    if (FAILED(hr)) {
        DbgBreak("Can't get ICcParser interface!");
        return hr;
    }

	if( !m_pifCcParser )
	{
		ASSERT(0);
		return E_FAIL;
	}

	return NOERROR;

}

HRESULT CCcParserProperties::OnDisconnect()
{
    if( !m_pifCcParser )
        return E_UNEXPECTED;

	ResetFilter();

    m_pifCcParser = NULL;

    return(NOERROR);

}

HRESULT CCcParserProperties::OnDeactivate()
{
	ResetFilter();

    return NOERROR;
}

HRESULT CCcParserProperties::OnActivate()
{
	HWND hwndChannel = GetDlgItem( m_hwndDlg, IDC_CHANNEL );
	ASSERT( hwndChannel );

    if( !m_pifCcParser )
		ASSERT(0);
	else
	{
		int iChannel = 0;
		VERIFY( SUCCEEDED( m_pifCcParser->get_Channel( &iChannel )));
		
		SendMessage( hwndChannel, CB_SETCURSEL, iChannel, 0);

		ICcParser_CCTYPE typeXform;
		VERIFY( SUCCEEDED( m_pifCcParser->get_XformType( &typeXform )));
			
		CheckDlgButton( m_hwndDlg, IDC_A53, typeXform == ICcParser_CCTYPE_ATSC_A53 );
	}
	
	OnSelchangeChannel();
	SetFilter();

    return NOERROR;
}

BOOL CCcParserProperties::OnInitDialog() 
{
	HWND hwndChannel = GetDlgItem( m_hwndDlg, IDC_CHANNEL );
	ASSERT( hwndChannel );

	SendMessage( hwndChannel, CB_INSERTSTRING, 0, (LPARAM)_T("CC 1"));
	SendMessage( hwndChannel, CB_INSERTSTRING, 1, (LPARAM)_T("CC 2"));
	SendMessage( hwndChannel, CB_INSERTSTRING, 2, (LPARAM)_T("CC 3"));
	SendMessage( hwndChannel, CB_INSERTSTRING, 3, (LPARAM)_T("CC 4"));

	OnClear();
	
	return TRUE;  // return TRUE unless you set the focus to a control
}

void CCcParserProperties::OnClear() 
{
	SetDlgItemText( m_hwndDlg, IDC_EXTRA, _T(""));
}

void CCcParserProperties::OnA53() 
{
	m_pifCcParser->put_XformType( IsDlgButtonChecked( m_hwndDlg, IDC_A53 ) 
								  ? ICcParser_CCTYPE_ATSC_A53
								  : ICcParser_CCTYPE_None
								);
}

void CCcParserProperties::SetFilter() 
{
	ASSERT( m_pifCcParser );
	
	if( IsFilterSet())
		return;
	
    if( SUCCEEDED( m_pifCcParser->AddDataSink( this, &m_idSink )))
		ASSERT( IsFilterSet());
	else
		::MessageBox( m_hwndDlg, _T("Could not AddDataSink!"), _T("CC Filter Test"), MB_ICONEXCLAMATION );

	//::CheckDlgButton( m_hwndDlg, IDC_LOG, IsFilterSet());

	m_iCCType = m_iShownCCType = (UINT)-1;
	m_timerUpdateCCType = ::SetTimer( m_hwndDlg, eventUpdateCCType, 250, NULL );
}


void CCcParserProperties::ResetFilter() 
{
	ASSERT( m_pifCcParser );

	if( !IsFilterSet())
		return;

	m_pifCcParser->RemoveDataSink( m_idSink ); //TODO VERIFY( SUCCEEDED( 
	m_idSink = 0;

//	::CheckDlgButton( m_hwndDlg, IDC_LOG, false );

	if( m_timerUpdateCCType )
		::KillTimer( m_hwndDlg, m_timerUpdateCCType );

	::SetDlgItemText( m_hwndDlg, IDC_TYPE, _T(""));
}

STDMETHODIMP CCcParserProperties::OnCc( int nType, int iField, WORD ccField )
{
	m_iCCType = nType;

	return CCcTextParser::OnCc( nType, iField, ccField ) ? S_OK : S_FALSE;
}

bool CCcParserProperties::OnCode( CCWORD cc )
{
	if( IsDlgButtonChecked( m_hwndDlg, IDC_SHOW ))
	{
		HWND hwndStatus = GetDlgItem( m_hwndDlg, IDC_EXTRA );
		ASSERT( hwndStatus );

		switch( cc.b1() & 0x07 )
		{
			case 0x04:  //misc
			case 0x05:  //misc + F
				switch (cc.b2())
				{
					case 0x20: // start pop on captioning
					case 0x25: //2 row caption
					case 0x26: //3 row caption
					case 0x27: //4 row caption
					case 0x29: //resume direct caption
					case 0x2d: // CR
					{
						//m_bValidText = true;
						SendMessage( hwndStatus, EM_REPLACESEL, FALSE, (LPARAM)("\r\n"));
					}
					break;
				}
			break;
		}
	}

	return CCcTextParser::OnCode( cc );
}

bool CCcParserProperties::OnText( CCWORD cc )
{
	if( IsDlgButtonChecked( m_hwndDlg, IDC_SHOW ))
	{
		HWND hwndStatus = GetDlgItem( m_hwndDlg, IDC_EXTRA );
		ASSERT( hwndStatus );

		TCHAR szCC[3]; cc.GetString( szCC );
		
		if( szCC[0])
			SendMessage( hwndStatus, EM_REPLACESEL, FALSE, (LPARAM)szCC );
		else
			ASSERT(1); // Breakpoing placeholder
	}

	return CCcTextParser::OnText( cc );
}

void CCcParserProperties::OnSelchangeChannel() 
{
	HWND hwndChannel = GetDlgItem( m_hwndDlg, IDC_CHANNEL );
	ASSERT( hwndChannel );

	int iChannel = SendMessage( hwndChannel, CB_GETCURSEL, 0,0 );
	ASSERT( iChannel >= 0 );

	VERIFY( SUCCEEDED( m_pifCcParser->put_Channel( iChannel )));
	CCcTextParser::Reset( iChannel );
}

void CCcParserProperties::OnTimer(UINT idEvent) 
{
	if( eventUpdateCCType == idEvent )
	{
		m_iShownCCType = 
			m_iShownCCType == (UINT)-1
			? m_iCCType
		    : (UINT)-1;
		
		m_iCCType = (UINT)-1;

		switch( m_iShownCCType )	
		{
			case CCcParser::cctypeATSC_A53:
				SetDlgItemText( m_hwndDlg, IDC_TYPE, _T("A53"));
				break;

			case CCcParser::cctypeEchostar:
				SetDlgItemText(m_hwndDlg, IDC_TYPE, _T("Echo"));
				break;

			case CCcParser::cctypeNone:
				SetDlgItemText(m_hwndDlg, IDC_TYPE, _T("???"));
				break;

			default:
				SetDlgItemText(m_hwndDlg, IDC_TYPE, _T(""));
				break;
		}

	}
}



