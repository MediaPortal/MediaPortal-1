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

#pragma once

#ifndef __FLASH_WINDOW_DEFINED
#define __FLASH_WINDOW_DEFINED

#include "OleContainerWindow.h"
#include "Flash10zh.tlh"

class CFlashWindow :
	public COleContainerWindow<ShockwaveFlashObjects::IShockwaveFlash>,
	public ShockwaveFlashObjects::_IShockwaveFlashEvents,
	public ShockwaveFlashObjects::IServiceProvider
{
public:
  CFlashWindow(HRESULT *result, const wchar_t *swfFilePath);
  virtual ~CFlashWindow(void);

  virtual HRESULT Initialize(void);
	virtual	HRESULT OnBeforeShowingContent();
	virtual	HRESULT OnAfterShowingContent();

  // gets result from query to flash
  // @param query : query in flash format
  // @return : result or NULL if error
  virtual const wchar_t *GetResult(const wchar_t *query);

  // IUnknown interface
	HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void ** ppvObject);
	ULONG STDMETHODCALLTYPE AddRef();
	ULONG STDMETHODCALLTYPE Release();

	// IDispatch interface
  virtual HRESULT STDMETHODCALLTYPE GetTypeInfoCount( 
    /* [out] */ UINT __RPC_FAR *pctinfo);
  virtual HRESULT STDMETHODCALLTYPE GetTypeInfo( 
    /* [in] */ UINT iTInfo,
    /* [in] */ LCID lcid,
    /* [out] */ ITypeInfo __RPC_FAR *__RPC_FAR *ppTInfo);
  virtual HRESULT STDMETHODCALLTYPE GetIDsOfNames( 
    /* [in] */ REFIID riid,
    /* [size_is][in] */ LPOLESTR __RPC_FAR *rgszNames,
    /* [in] */ UINT cNames,
    /* [in] */ LCID lcid,
    /* [size_is][out] */ DISPID __RPC_FAR *rgDispId);
  virtual /* [local] */ HRESULT STDMETHODCALLTYPE Invoke( 
    /* [in] */ DISPID dispIdMember,
    /* [in] */ REFIID riid,
    /* [in] */ LCID lcid,
    /* [in] */ WORD wFlags,
    /* [out][in] */ DISPPARAMS __RPC_FAR *pDispParams,
    /* [out] */ VARIANT __RPC_FAR *pVarResult,
    /* [out] */ EXCEPINFO __RPC_FAR *pExcepInfo,
    /* [out] */ UINT __RPC_FAR *puArgErr);

  // _IShockwaveFlashEvents interface
  HRESULT STDMETHODCALLTYPE OnReadyStateChange(long newState);
  HRESULT STDMETHODCALLTYPE OnProgress(long percentDone);
  HRESULT STDMETHODCALLTYPE FSCommand(_bstr_t command, _bstr_t args);
  HRESULT STDMETHODCALLTYPE CFlashWindow::FlashCall(_bstr_t request);

  // IServiceProvider interface

  HRESULT __stdcall raw_RemoteQueryService (
    GUID * guidService,
    GUID * riid,
    IUnknown **ppvObject);

protected:
	long m_lVersion;

  // holds swf file path
  wchar_t *swfFilePath;
  // holds internal copy of query result
  // flash instance get only reference to this variable (can't free memory!)
  wchar_t *queryResultInternal;
};

#endif