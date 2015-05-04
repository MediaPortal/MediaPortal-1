//------------------------------------------------------------------------------
// File: CcPProp.h
//
// Desc: DirectShow sample code - definition of CCcParserProperties class,
//       providing a properties page derived from the property page base
//       class for minimum effort.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------


#ifndef __CCPPROPS_H__
#define __CCPPROPS_H__

#include "ICcParser.h" // ICcDataSink
#include "../Parsing/CCParser.h"


class CCcParserProperties 
	: public CBasePropertyPage
	, private ICcDataSink
	, private CCcTextParser
{
public:
    DECLARE_IUNKNOWN

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN lpunk, HRESULT *phr);

    // Overrides from CBasePropertyPage
    HRESULT OnConnect(IUnknown * punk);
    HRESULT OnDisconnect(void);

    virtual HRESULT OnActivate();
    virtual HRESULT OnDeactivate();

    CCcParserProperties(LPUNKNOWN lpunk, HRESULT *phr);
	~CCcParserProperties();

	bool IsFilterSet() const { return 0 != m_idSink; }
	void SetFilter();
	void ResetFilter();

private:
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid,void **ppv);

// ICcDataSink
	STDMETHOD(OnCc)( int nType, int iField, WORD ccField );
	STDMETHOD(OnProgress)( int nPercent ) { return S_OK; }

// CCcTextParser
	virtual bool OnText( CCWORD ccText );
	virtual bool OnCode( CCWORD ccText );

	INT_PTR OnReceiveMessage(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

    HWND m_hwndDlg;

    auto_pif<ICcParser> m_pifCcParser;
	DWORD m_idSink;

	UINT m_iCCType;
	UINT m_iShownCCType;
	
	enum{ eventUpdateCCType = 1 };
	UINT m_timerUpdateCCType;

	BOOL OnInitDialog();
	void OnSelchangeChannel();
	void OnTimer(UINT nIDEvent);
	void OnClear();
	void OnA53();

};

#endif // __CCPPROPS_H__
