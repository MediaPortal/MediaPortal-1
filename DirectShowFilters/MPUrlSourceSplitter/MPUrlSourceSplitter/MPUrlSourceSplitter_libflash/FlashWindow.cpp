/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "FlashWindow.h"

#include <wchar.h>

#define QUERY_RESULT_STRING_START                                             L"<string>"
#define QUERY_RESULT_STRING_START_LENGTH                                      8

#define QUERY_RESULT_STRING_END                                               L"</string>"
#define QUERY_RESULT_STRING_END_LENGTH                                        9

CFlashWindow::CFlashWindow(HRESULT *result, const wchar_t *swfFilePath)
  : COleContainerWindow(result)
{
  m_lVersion = 0;
  this->swfFilePath = NULL;
  this->queryResultInternal = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, swfFilePath);

    if (SUCCEEDED(*result))
    {
      this->swfFilePath = Duplicate(swfFilePath);

      CHECK_CONDITION_HRESULT(*result, this->swfFilePath, *result, E_OUTOFMEMORY);
    }
  }
}

CFlashWindow::~CFlashWindow(void)
{
  FREE_MEM(this->swfFilePath);
  FREE_MEM(this->queryResultInternal);
}

HRESULT CFlashWindow::Initialize(void)
{
  return __super::Initialize();
}

const wchar_t *CFlashWindow::GetResult(const wchar_t *query)
{
  wchar_t *result = NULL;
  if (this->m_lpControl != NULL)
  {
    _bstr_t queryResult = this->m_lpControl->CallFunction(query);

    wchar_t *tempResult = Duplicate((const wchar_t *)queryResult);
    unsigned int length = wcslen(tempResult) - QUERY_RESULT_STRING_START_LENGTH - QUERY_RESULT_STRING_END_LENGTH;

    FREE_MEM(this->queryResultInternal);
    this->queryResultInternal = ALLOC_MEM_SET(this->queryResultInternal, wchar_t, (length + 1), 0);
    if ((this->queryResultInternal != NULL) && (length != 0))
    {
      wmemcpy(this->queryResultInternal, tempResult + QUERY_RESULT_STRING_START_LENGTH, length);
    }

    FREE_MEM(tempResult);
    result = this->queryResultInternal;
  }
  return result;
}

//DShockwaveFlashEvents
HRESULT STDMETHODCALLTYPE CFlashWindow::OnReadyStateChange(long newState)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CFlashWindow::OnProgress(long percentDone)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CFlashWindow::FSCommand(_bstr_t command, _bstr_t args)
{
	return S_OK;
}

//
// Handle a call from Flash ActionScript (in .swf file)
//
// Flash marshalls function calls to XML, for example:
// request: <invoke name="addNumbers" returntype="xml"><arguments><number>0</number><number>1</number></arguments></invoke>
//
HRESULT STDMETHODCALLTYPE CFlashWindow::FlashCall(_bstr_t request)
{
	return S_FALSE;
}

HRESULT CFlashWindow::OnBeforeShowingContent()
{
	m_lVersion = m_lpControl->FlashVersion();

	if ((m_lVersion & 0x00FF0000) == 0x00080000)
  {
		m_bFixTransparency = TRUE;
  }
	else
  {
		m_bFixTransparency = FALSE;
  }
	
	HRESULT result = m_lpControl->QueryInterface(IID_IConnectionPointContainer, (void**)&m_lpConCont);
  if (SUCCEEDED(result))
  {
    result = m_lpConCont->FindConnectionPoint(ShockwaveFlashObjects::DIID__IShockwaveFlashEvents, &m_lpConPoint);
  }
  if (SUCCEEDED(result))
  {
    result = m_lpConPoint->Advise((ShockwaveFlashObjects::_IShockwaveFlashEvents *)this, &m_dwConPointID);
  }
  if (SUCCEEDED(result))
  {
    if (m_bTransparent)
    {
      m_lpControl->PutWMode(L"transparent");
    }

    m_lpControl->PutScale(L"showAll");
  }

	return result;
}

HRESULT CFlashWindow::OnAfterShowingContent()
{
  m_lpControl->PutEmbedMovie(TRUE);
  HRESULT result = m_lpControl->LoadMovie(0, this->swfFilePath);
  if (SUCCEEDED(result))
  {
    result = m_lpControl->Play();
  }

  return result;
}

// IUnknown interface
HRESULT STDMETHODCALLTYPE CFlashWindow::QueryInterface(REFIID riid, void ** ppvObject)
{
	HRESULT hr = COleContainerWindow<ShockwaveFlashObjects::IShockwaveFlash>::QueryInterface(riid, ppvObject);
	if (hr != E_NOINTERFACE)
		return hr;

	if (IsEqualGUID(riid, ShockwaveFlashObjects::DIID__IShockwaveFlashEvents))
	{
		*ppvObject = (void*)dynamic_cast<ShockwaveFlashObjects::_IShockwaveFlashEvents *>(this);
	}
	else if (IsEqualGUID(riid, ShockwaveFlashObjects::IID_IServiceProvider))
	{
		*ppvObject = (void*)dynamic_cast<ShockwaveFlashObjects::IServiceProvider *>(this);
	}
	else
	{
		*ppvObject = 0;
		return E_NOINTERFACE;
	}
	if (!(*ppvObject))
		return E_NOINTERFACE; //if dynamic_cast returned 0

	this->m_iRef++;
	return S_OK;
}

ULONG STDMETHODCALLTYPE CFlashWindow::AddRef()
{
	this->m_iRef++;
	return this->m_iRef;
}

ULONG STDMETHODCALLTYPE CFlashWindow::Release()
{
	this->m_iRef--;
	return this->m_iRef;
}

// IDispatch interface
HRESULT STDMETHODCALLTYPE CFlashWindow::GetTypeInfoCount(UINT __RPC_FAR *pctinfo)
{
	return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CFlashWindow::GetTypeInfo( 
    /* [in] */ UINT iTInfo,
    /* [in] */ LCID lcid,
    /* [out] */ ITypeInfo __RPC_FAR *__RPC_FAR *ppTInfo)
{
	return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CFlashWindow::GetIDsOfNames( 
    /* [in] */ REFIID riid,
    /* [size_is][in] */ LPOLESTR __RPC_FAR *rgszNames,
    /* [in] */ UINT cNames,
    /* [in] */ LCID lcid,
    /* [size_is][out] */ DISPID __RPC_FAR *rgDispId)
{
	return E_NOTIMPL;
}

/*!
	\brief Callback handler for interface '_IShockwaveFlashEvents' (AKA 'Sink' in COM/Windows lingo)
*/
HRESULT STDMETHODCALLTYPE CFlashWindow::Invoke( 
    /* [in] */ DISPID dispIdMember,
    /* [in] */ REFIID riid,
    /* [in] */ LCID lcid,
    /* [in] */ WORD wFlags,
    /* [out][in] */ DISPPARAMS __RPC_FAR *pDispParams,
    /* [out] */ VARIANT __RPC_FAR *pVarResult,
    /* [out] */ EXCEPINFO __RPC_FAR *pExcepInfo,
    /* [out] */ UINT __RPC_FAR *puArgErr)
{
	if (wFlags == DISPATCH_METHOD)
	{
		switch (dispIdMember)          
		{          
		case 0xc5: // FlashCall (from ActionScript)
			if (pDispParams->cArgs != 1 || pDispParams->rgvarg[0].vt != VT_BSTR) 
				return E_INVALIDARG;
			return this->FlashCall(pDispParams->rgvarg[0].bstrVal);
			break;

		case 0x96: // FSCommand (from ActionScript)
			if (pDispParams->cArgs != 2 || pDispParams->rgvarg[0].vt != VT_BSTR || pDispParams->rgvarg[1].vt != VT_BSTR) 
				return E_INVALIDARG;
			return this->FSCommand(pDispParams->rgvarg[1].bstrVal, pDispParams->rgvarg[0].bstrVal);
			break;

		case 0x7a6: // OnProgress                  
			return OnProgress(pDispParams->rgvarg[0].intVal);
			break;

		case DISPID_READYSTATECHANGE:                  
			return E_NOTIMPL;
			break;
		}
	}

	return E_NOTIMPL;
}

HRESULT __stdcall CFlashWindow::raw_RemoteQueryService (
       GUID * guidService,
       GUID * riid,
       IUnknown * * ppvObject )
{
	return E_NOINTERFACE;
}