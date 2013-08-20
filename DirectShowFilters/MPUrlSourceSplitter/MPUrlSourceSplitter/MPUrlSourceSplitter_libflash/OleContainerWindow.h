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

#ifndef __OLE_CONTAINER_WINDOW_DEFINED
#define __OLE_CONTAINER_WINDOW_DEFINED

#include <comdef.h>
#include <assert.h>

#define OLECONTAINER_DEF                                                        template <class TObj>
#define OLECONTAINER_DEF2                                                       TObj

#define OLECONTAINER_CONSTRUCT                                                  OLECONTAINER_DEF COleContainerWindow<OLECONTAINER_DEF2>
#define OLECONTAINER(type)                                                      OLECONTAINER_DEF type COleContainerWindow<OLECONTAINER_DEF2>

OLECONTAINER_DEF class COleContainerWindow : 
  public IOleClientSite,
	public IOleInPlaceSiteWindowless,
	public IOleInPlaceFrame,
	public IStorage
{
public:
  COleContainerWindow(void);
  virtual ~COleContainerWindow(void);

  static LRESULT CALLBACK WndProcStatic(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
	LRESULT WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
	HWND GetHWND() { assert(m_hWnd); return m_hWnd; }
	HWND GetParentWindow() { assert(m_hWndParent); return m_hWndParent; }
	HWND GetInstance() { assert(m_hInst); return m_hInst; }

  virtual HRESULT Initialize(void);
  virtual HRESULT Create(GUID clsid, DWORD dwExStyle, DWORD dwStyle, HWND hWndParent, HINSTANCE hInst, const wchar_t *className);
	virtual void Draw(HDC hdcDraw, const RECT *rcDraw, BOOL bErase);

	//ole container events
	virtual	HRESULT OnBeforeShowingContent();
	virtual	HRESULT OnAfterShowingContent();

  // IUnknown interface
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void ** ppvObject);
  ULONG STDMETHODCALLTYPE AddRef();
  ULONG STDMETHODCALLTYPE Release();

  // IOleClientSite interface
  virtual HRESULT STDMETHODCALLTYPE SaveObject();
  virtual HRESULT STDMETHODCALLTYPE GetMoniker(DWORD dwAssign, DWORD dwWhichMoniker, IMoniker ** ppmk);
  virtual HRESULT STDMETHODCALLTYPE GetContainer(LPOLECONTAINER FAR* ppContainer);
  virtual HRESULT STDMETHODCALLTYPE ShowObject();
  virtual HRESULT STDMETHODCALLTYPE OnShowWindow(BOOL fShow);
  virtual HRESULT STDMETHODCALLTYPE RequestNewObjectLayout();

  // IOleInPlaceSite interface
  virtual HRESULT STDMETHODCALLTYPE GetWindow(HWND FAR* lphwnd);
  virtual HRESULT STDMETHODCALLTYPE ContextSensitiveHelp(BOOL fEnterMode);
  virtual HRESULT STDMETHODCALLTYPE CanInPlaceActivate();
  virtual HRESULT STDMETHODCALLTYPE OnInPlaceActivate();
  virtual HRESULT STDMETHODCALLTYPE OnUIActivate();
  virtual HRESULT STDMETHODCALLTYPE GetWindowContext(LPOLEINPLACEFRAME FAR* lplpFrame,LPOLEINPLACEUIWINDOW FAR* lplpDoc,LPRECT lprcPosRect,LPRECT lprcClipRect,LPOLEINPLACEFRAMEINFO lpFrameInfo);
  virtual HRESULT STDMETHODCALLTYPE Scroll(SIZE scrollExtent);
  virtual HRESULT STDMETHODCALLTYPE OnUIDeactivate(BOOL fUndoable);
  virtual HRESULT STDMETHODCALLTYPE OnInPlaceDeactivate();
  virtual HRESULT STDMETHODCALLTYPE DiscardUndoState();
  virtual HRESULT STDMETHODCALLTYPE DeactivateAndUndo();
  virtual HRESULT STDMETHODCALLTYPE OnPosRectChange(LPCRECT lprcPosRect);

  // IOleInPlaceSiteEx interface
  virtual HRESULT STDMETHODCALLTYPE OnInPlaceActivateEx(BOOL __RPC_FAR *pfNoRedraw, DWORD dwFlags);
  virtual HRESULT STDMETHODCALLTYPE OnInPlaceDeactivateEx(BOOL fNoRedraw);
  virtual HRESULT STDMETHODCALLTYPE RequestUIActivate(void);

  // IOleInPlaceSiteWindowless interface
  virtual HRESULT STDMETHODCALLTYPE CanWindowlessActivate( void);
  virtual HRESULT STDMETHODCALLTYPE GetCapture( void);
  virtual HRESULT STDMETHODCALLTYPE SetCapture( 
    /* [in] */ BOOL fCapture);
  virtual HRESULT STDMETHODCALLTYPE GetFocus( void);
  virtual HRESULT STDMETHODCALLTYPE SetFocus( 
    /* [in] */ BOOL fFocus);
  virtual HRESULT STDMETHODCALLTYPE GetDC( 
    /* [in] */ LPCRECT pRect,
    /* [in] */ DWORD grfFlags,
    /* [out] */ HDC __RPC_FAR *phDC);
  virtual HRESULT STDMETHODCALLTYPE ReleaseDC( 
    /* [in] */ HDC hDC);
  virtual HRESULT STDMETHODCALLTYPE InvalidateRect( 
    /* [in] */ LPCRECT pRect,
    /* [in] */ BOOL fErase);
  virtual HRESULT STDMETHODCALLTYPE InvalidateRgn( 
    /* [in] */ HRGN hRGN,
    /* [in] */ BOOL fErase);
  virtual HRESULT STDMETHODCALLTYPE ScrollRect( 
    /* [in] */ INT dx,
    /* [in] */ INT dy,
    /* [in] */ LPCRECT pRectScroll,
    /* [in] */ LPCRECT pRectClip);
  virtual HRESULT STDMETHODCALLTYPE AdjustRect( 
    /* [out][in] */ LPRECT prc);
  virtual HRESULT STDMETHODCALLTYPE OnDefWindowMessage( 
    /* [in] */ UINT msg,
    /* [in] */ WPARAM wParam,
    /* [in] */ LPARAM lParam,
    /* [out] */ LRESULT __RPC_FAR *plResult);

  // IOleInPlaceFrame interface
  //	virtual HRESULT STDMETHODCALLTYPE GetWindow(HWND FAR* lphwnd);
  //	virtual HRESULT STDMETHODCALLTYPE ContextSensitiveHelp(BOOL fEnterMode);
  virtual HRESULT STDMETHODCALLTYPE GetBorder(LPRECT lprectBorder);
  virtual HRESULT STDMETHODCALLTYPE RequestBorderSpace(LPCBORDERWIDTHS pborderwidths);
  virtual HRESULT STDMETHODCALLTYPE SetBorderSpace(LPCBORDERWIDTHS pborderwidths);
  virtual HRESULT STDMETHODCALLTYPE SetActiveObject(IOleInPlaceActiveObject *pActiveObject, LPCOLESTR pszObjName);
  virtual HRESULT STDMETHODCALLTYPE InsertMenus(HMENU hmenuShared, LPOLEMENUGROUPWIDTHS lpMenuWidths);
  virtual HRESULT STDMETHODCALLTYPE SetMenu(HMENU hmenuShared, HOLEMENU holemenu, HWND hwndActiveObject);
  virtual HRESULT STDMETHODCALLTYPE RemoveMenus(HMENU hmenuShared);
  virtual HRESULT STDMETHODCALLTYPE SetStatusText(LPCOLESTR pszStatusText);
  virtual HRESULT STDMETHODCALLTYPE EnableModeless(BOOL fEnable);
  virtual HRESULT STDMETHODCALLTYPE TranslateAccelerator(LPMSG lpmsg, WORD wID);

  // IStorage interface
  virtual HRESULT STDMETHODCALLTYPE CreateStream(const WCHAR *pwcsName, DWORD grfMode, DWORD reserved1, DWORD reserved2, IStream **ppstm);
  virtual HRESULT STDMETHODCALLTYPE OpenStream(const WCHAR * pwcsName, void *reserved1, DWORD grfMode, DWORD reserved2, IStream **ppstm);
  virtual HRESULT STDMETHODCALLTYPE CreateStorage(const WCHAR *pwcsName, DWORD grfMode, DWORD reserved1, DWORD reserved2, IStorage **ppstg);
  virtual HRESULT STDMETHODCALLTYPE OpenStorage(const WCHAR * pwcsName, IStorage * pstgPriority, DWORD grfMode, SNB snbExclude, DWORD reserved, IStorage **ppstg);
  virtual HRESULT STDMETHODCALLTYPE CopyTo(DWORD ciidExclude, IID const *rgiidExclude, SNB snbExclude,IStorage *pstgDest);
  virtual HRESULT STDMETHODCALLTYPE MoveElementTo(const OLECHAR *pwcsName,IStorage * pstgDest, const OLECHAR *pwcsNewName, DWORD grfFlags);
  virtual HRESULT STDMETHODCALLTYPE Commit(DWORD grfCommitFlags);
  virtual HRESULT STDMETHODCALLTYPE Revert();
  virtual HRESULT STDMETHODCALLTYPE EnumElements(DWORD reserved1, void * reserved2, DWORD reserved3, IEnumSTATSTG ** ppenum);
  virtual HRESULT STDMETHODCALLTYPE DestroyElement(const OLECHAR *pwcsName);
  virtual HRESULT STDMETHODCALLTYPE RenameElement(const WCHAR *pwcsOldName, const WCHAR *pwcsNewName);
  virtual HRESULT STDMETHODCALLTYPE SetElementTimes(const WCHAR *pwcsName, FILETIME const *pctime, FILETIME const *patime, FILETIME const *pmtime);
  virtual HRESULT STDMETHODCALLTYPE SetClass(REFCLSID clsid);
  virtual HRESULT STDMETHODCALLTYPE SetStateBits(DWORD grfStateBits, DWORD grfMask);
  virtual HRESULT STDMETHODCALLTYPE Stat(STATSTG * pstatstg, DWORD grfStatFlag);

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

protected:
  GUID m_CLSID;
	TObj *m_lpControl;
	IOleObject *m_lpO;
	IViewObjectEx *m_lpViewObject;
	IViewObjectEx *m_lpViewObjectEx;
	IOleInPlaceObject *m_lpInPlaceObj;
	IOleInPlaceObjectWindowless *m_lpInPlaceObjWindowless;
	IConnectionPointContainer *m_lpConCont;
	IConnectionPoint *m_lpConPoint;

	HWND m_hWnd, m_hWndParent;
	HINSTANCE m_hInst;

	BOOL m_bChild;
	BOOL m_bTransparent;
	BOOL m_bFixTransparency;
	DWORD m_dwConPointID;
	HDC m_hdcBack;
	HBITMAP m_bmpBack;
	RECT m_rcBounds;
	BYTE *m_lpBitsOnly;
	int m_iBPP;
	HDC m_hdcBackW;
	HBITMAP m_bmpBackW;
	BYTE *m_lpBitsOnlyW;

  int m_iRef;

  wchar_t *className;
  wchar_t *classWindowName;
};

OLECONTAINER_CONSTRUCT::COleContainerWindow()
{
	m_lpControl = NULL;
	m_lpO = NULL;
	m_lpViewObjectEx = NULL;
	m_lpViewObject = NULL;
	m_lpInPlaceObj = NULL;
	m_lpInPlaceObjWindowless = NULL;
	m_lpConCont = NULL;
	m_lpConPoint = NULL;

	m_hdcBack = NULL;
	m_bmpBack = NULL;
	m_hdcBackW = NULL;
	m_bmpBackW = NULL;
	m_rcBounds.left = m_rcBounds.top = m_rcBounds.right = m_rcBounds.bottom = 0;
	m_lpBitsOnly = NULL;
	m_lpBitsOnlyW = NULL;
	m_iBPP = 0;

	m_dwConPointID = 0;
	m_bTransparent = FALSE;
	m_bFixTransparency = FALSE;
	m_iRef = 0;

  this->className = NULL;
  this->classWindowName = NULL;
} 

OLECONTAINER_CONSTRUCT::~COleContainerWindow()
{
	if (m_lpControl)
	{
		if (m_lpConPoint)
		{
			if (m_dwConPointID)
				m_lpConPoint->Unadvise(m_dwConPointID);
			m_lpConPoint->Release();
		}

		if (m_lpConCont)
			m_lpConCont->Release();

		m_lpO->Close(OLECLOSE_NOSAVE);

		if (m_lpViewObjectEx)
			m_lpViewObjectEx->Release();

		if (m_lpViewObject)
			m_lpViewObject->Release();

		if (m_lpInPlaceObjWindowless)
			m_lpInPlaceObjWindowless->Release();

		if (m_lpInPlaceObj)
			m_lpInPlaceObj->Release();

		if (m_lpO)
			m_lpO->Release();

		m_lpControl->Release();
	}
	if (m_hdcBack)
		::DeleteDC(m_hdcBack);
	if (m_bmpBack)
		::DeleteObject(m_bmpBack);
	if (m_hdcBackW)
		::DeleteDC(m_hdcBackW);
	if (m_bmpBackW)
		::DeleteObject(m_bmpBackW);

  FREE_MEM(this->className);
  FREE_MEM(this->classWindowName);
}

OLECONTAINER(HRESULT)::Initialize()
{
  return S_OK;
}

OLECONTAINER(HRESULT)::Create(GUID clsid, DWORD dwExStyle, DWORD dwStyle, HWND hWndParent, HINSTANCE hInst, const wchar_t *className)
{
  HRESULT result = S_OK;
	m_hWndParent = hWndParent;
	m_hInst = hInst;

	m_CLSID = clsid;
	m_bTransparent = dwExStyle & WS_EX_LAYERED;
	m_bChild = dwStyle & WS_CHILD;

  this->className = Duplicate(className);
  this->classWindowName = FormatString(L"%sWindow", this->className);

  result = ((this->className != NULL) && (this->classWindowName != NULL)) ? S_OK : E_OUTOFMEMORY;

  RECT r;
  if (SUCCEEDED(result) && (hWndParent != NULL))
  {
    if (m_bChild)
    {
      result = (::GetClientRect(hWndParent, &r) != 0) ? S_OK : HRESULT_FROM_WIN32(GetLastError());
    }
    else
    {
      result = (::GetWindowRect(hWndParent, &r) != 0) ? S_OK : HRESULT_FROM_WIN32(GetLastError());
    }
  }
  else
  {
    r.bottom = 0;
    r.left = 0;
    r.right = 0;
    r.top = 0;
  }

  if (SUCCEEDED(result))
  {
    m_hWnd = CreateWindowEx(dwExStyle,
      this->className, this->classWindowName,
      dwStyle,
      r.left, r.top, r.right-r.left, r.bottom-r.top, hWndParent, NULL, hInst, (void *)this);

    result = (m_hWnd != NULL) ? S_OK : HRESULT_FROM_WIN32(GetLastError());
  }

  if (SUCCEEDED(result))
  {
    result = (::SetWindowPos(GetHWND(), HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE) != 0) ? S_OK : HRESULT_FROM_WIN32(GetLastError());
  }
	
  if (SUCCEEDED(result))
  {
    result = OleCreate(m_CLSID, IID_IOleObject, OLERENDER_DRAW,
      0, (IOleClientSite *)this, (IStorage *)this, (void **)&m_lpO);
  }

  if (SUCCEEDED(result))
  {
    result = OleSetContainedObject(m_lpO, TRUE);
  }

  if (SUCCEEDED(result))
  {
    result = m_lpO->QueryInterface(__uuidof(TObj), (void **)&m_lpControl);
  }

  if (SUCCEEDED(result))
  {
    result = m_lpO->QueryInterface(IID_IViewObjectEx, (void **)&m_lpViewObjectEx);

    if (FAILED(result))
    {
      m_lpViewObjectEx = NULL;
      result = m_lpO->QueryInterface(IID_IViewObject, (void **)&m_lpViewObject);
    }
  }

  if (SUCCEEDED(result))
  {
    if (m_bTransparent)
    {
      result = m_lpO->QueryInterface(IID_IOleInPlaceObjectWindowless, (void **)&m_lpInPlaceObjWindowless);
      if (FAILED(result))
      {
        result = m_lpO->QueryInterface(IID_IOleInPlaceObject, (void **)&m_lpInPlaceObj);
        
        if (SUCCEEDED(result))
        {
          m_bTransparent = FALSE;
        }
      }
    }
    else
    {
      result = m_lpO->QueryInterface(IID_IOleInPlaceObject, (void **)&m_lpInPlaceObj);
    }
  }

  if (SUCCEEDED(result))
  {
    result = OnBeforeShowingContent();
  }

  if (SUCCEEDED(result))
  {
    m_lpControl->DisableLocalSecurity();
    m_lpControl->PutEmbedMovie(TRUE);
    m_lpControl->PutAllowScriptAccess(L"always");

    // instruct the object to show itself for editing or viewing
    result = m_lpO->DoVerb(OLEIVERB_SHOW, NULL, (IOleClientSite *)this, 0, NULL, NULL);
  }

  if (SUCCEEDED(result))
  {
    result = OnAfterShowingContent();
  }

  return result;
}

//interface methods

//IUnknown

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::QueryInterface(REFIID riid, void ** ppvObject)
{
	if (IsEqualGUID(riid, IID_IUnknown))
		*ppvObject = (void*)(this);
	else if (IsEqualGUID(riid, IID_IOleInPlaceSite))
		*ppvObject = (void*)dynamic_cast<IOleInPlaceSite *>(this);
	else if (IsEqualGUID(riid, IID_IOleInPlaceSiteEx))
		*ppvObject = (void*)dynamic_cast<IOleInPlaceSiteEx *>(this);
	else if (IsEqualGUID(riid, IID_IOleInPlaceSiteWindowless))
		*ppvObject = (void*)dynamic_cast<IOleInPlaceSiteWindowless *>(this);
	else if (IsEqualGUID(riid, IID_IOleInPlaceFrame))
		*ppvObject = (void*)dynamic_cast<IOleInPlaceFrame *>(this);
	else if (IsEqualGUID(riid, IID_IStorage))
		*ppvObject = (void*)dynamic_cast<IStorage *>(this);
	else
	{
		*ppvObject = 0;
		return E_NOINTERFACE;
	}
	if (!(*ppvObject))
		return E_NOINTERFACE; //if dynamic_cast returned 0
	m_iRef++;
	return S_OK;
}

OLECONTAINER(ULONG STDMETHODCALLTYPE)::AddRef()
{
	m_iRef++;
	return m_iRef;
}

OLECONTAINER(ULONG STDMETHODCALLTYPE)::Release()
{
	m_iRef--;
	return m_iRef;
}

//IOleClientSite

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SaveObject()
{ 
	return E_NOTIMPL; 
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetMoniker(DWORD dwAssign, DWORD dwWhichMoniker, IMoniker ** ppmk)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetContainer(LPOLECONTAINER FAR* ppContainer)
{
	*ppContainer = 0;
  	return E_NOINTERFACE;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::ShowObject() 
{
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnShowWindow(BOOL fShow)
{ 
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::RequestNewObjectLayout() 
{ 
	return E_NOTIMPL; 
}

//IOleInPlaceSite

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetWindow(HWND FAR* lphwnd)
{
	*lphwnd = GetHWND();
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::ContextSensitiveHelp(BOOL fEnterMode) 
{ 
	return E_NOTIMPL; 
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::CanInPlaceActivate()
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnInPlaceActivate()
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnUIActivate()
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetWindowContext(LPOLEINPLACEFRAME FAR* lplpFrame, LPOLEINPLACEUIWINDOW FAR* lplpDoc, LPRECT lprcPosRect, LPRECT lprcClipRect, LPOLEINPLACEFRAMEINFO lpFrameInfo)
{
	*lplpFrame = (LPOLEINPLACEFRAME)this;

	*lplpDoc = 0;

	lpFrameInfo->fMDIApp = FALSE;
	lpFrameInfo->hwndFrame = GetHWND();
	lpFrameInfo->haccel = 0;
	lpFrameInfo->cAccelEntries = 0;
	
	RECT r;
	::GetClientRect(GetHWND(), &r);
	*lprcPosRect = r;
	*lprcClipRect = r;
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::Scroll(SIZE scrollExtent)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnUIDeactivate(BOOL fUndoable)
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnInPlaceDeactivate()
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::DiscardUndoState()
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::DeactivateAndUndo()
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnPosRectChange(LPCRECT lprcPosRect)
{
	return(S_OK);
}


//IOleInPlaceSiteEx

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnInPlaceActivateEx(BOOL __RPC_FAR *pfNoRedraw, DWORD dwFlags)
{
	if (pfNoRedraw)
		*pfNoRedraw = FALSE;
	return S_OK;
}
OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnInPlaceDeactivateEx(BOOL fNoRedraw)
{
	return S_FALSE;
}
OLECONTAINER(HRESULT STDMETHODCALLTYPE)::RequestUIActivate(void)
{
	return S_FALSE;
}


//IOleInPlaceSiteWindowless

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::CanWindowlessActivate( void)
{
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetCapture( void)
{
	return S_FALSE;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetCapture( 
    /* [in] */ BOOL fCapture)
{
	return S_FALSE;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetFocus( void)
{
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetFocus( 
    /* [in] */ BOOL fFocus)
{
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetDC( 
    /* [in] */ LPCRECT pRect,
    /* [in] */ DWORD grfFlags,
    /* [out] */ HDC __RPC_FAR *phDC)
{
	return S_FALSE;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::ReleaseDC( 
    /* [in] */ HDC hDC)
{
	return S_FALSE;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::InvalidateRect( 
    /* [in] */ LPCRECT pRect,
    /* [in] */ BOOL fErase)
{
	Draw(NULL, pRect, fErase);
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::InvalidateRgn( 
    /* [in] */ HRGN hRGN,
    /* [in] */ BOOL fErase)
{
	return S_OK;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::ScrollRect( 
    /* [in] */ INT dx,
    /* [in] */ INT dy,
    /* [in] */ LPCRECT pRectScroll,
    /* [in] */ LPCRECT pRectClip)
{
	return E_NOTIMPL;
} 

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::AdjustRect( 
    /* [out][in] */ LPRECT prc)
{
	return S_FALSE;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OnDefWindowMessage( 
    /* [in] */ UINT msg,
    /* [in] */ WPARAM wParam,
    /* [in] */ LPARAM lParam,
    /* [out] */ LRESULT __RPC_FAR *plResult)
{
	return S_FALSE;
} 

OLECONTAINER(void)::Draw(HDC hdcDraw, const RECT *rcDraw, BOOL bErase)
{
	HWND hwnd = GetHWND();
	HRESULT hr;
	RECT r;

	IOleObject *lpO = m_lpO;
	IViewObject *lpV = m_lpViewObjectEx ? (IViewObject *)m_lpViewObjectEx : m_lpViewObject;

	if (!m_bTransparent)
	{
		RECT rTotal;
			::GetClientRect(hwnd, &rTotal);
		if (lpV)
		{
			if (!hdcDraw)
			{
				hdcDraw = ::GetDC(hwnd);
				hr = OleDraw(lpV, DVASPECT_CONTENT, hdcDraw, &rTotal);
				::ReleaseDC(hwnd, hdcDraw);
			}
			else
			{
				hr = OleDraw(lpV, DVASPECT_CONTENT, hdcDraw, &rTotal);
			}
		}
		return;
	}

	::GetWindowRect(hwnd, &r);
	if (!m_hdcBack || !EqualRect(&r, &m_rcBounds))
	{
		if (m_hdcBack)
			::DeleteDC(m_hdcBack);
		if (m_bmpBack)
			::DeleteObject(m_bmpBack);
		if (m_hdcBackW)
			::DeleteDC(m_hdcBackW);
		if (m_bmpBackW)
			::DeleteObject(m_bmpBackW);
		m_rcBounds = r;
		HDC hdc = ::GetDC(hwnd);
		BITMAPINFOHEADER bih = {0};
		bih.biSize = sizeof(BITMAPINFOHEADER);
		bih.biBitCount = 32;
		bih.biCompression = BI_RGB;
		bih.biPlanes = 1;
		bih.biWidth = r.right - r.left;
		bih.biHeight = -(r.bottom - r.top);
		m_hdcBack = CreateCompatibleDC(hdc);
		m_bmpBack = CreateDIBSection(hdc, (BITMAPINFO *)&bih, DIB_RGB_COLORS, (void **)&m_lpBitsOnly, NULL, 0x0);
		SelectObject(m_hdcBack, m_bmpBack);
		if (m_bFixTransparency)
		{
			m_hdcBackW = CreateCompatibleDC(hdc);
			m_bmpBackW = CreateDIBSection(hdc, (BITMAPINFO *)&bih, DIB_RGB_COLORS, (void **)&m_lpBitsOnlyW, NULL, 0x0);
			SelectObject(m_hdcBackW, m_bmpBackW);
		}
		::ReleaseDC(hwnd, hdc);
		if (m_iBPP == 0)
			m_iBPP = GetDeviceCaps(m_hdcBack, BITSPIXEL);
	}
	POINT p = {r.left, r.top};
	POINT p2 = {0, 0};
	SIZE sz = {r.right-r.left, r.bottom-r.top};

	if (lpO && lpV)
	{
		RECT rTotal;
		::GetClientRect(hwnd, &rTotal);
		RECTL rcBounds = {rTotal.left, rTotal.top, rTotal.right, rTotal.bottom};
		BYTE *dst = m_lpBitsOnly, *dstW;
		if (m_iBPP == 32)
		{
			if (!m_bFixTransparency) //if flash player version is other than 8, do usual painting
			{
				memset(m_lpBitsOnly, 0, sz.cx * sz.cy * 4);
				hr = OleDraw(lpV, DVASPECT_TRANSPARENT, m_hdcBack, &rTotal);
			}
			else //if player version is 8, we need to fix flash player 8 control transparency bug
			{
				memset(m_lpBitsOnly, 0, sz.cx * sz.cy * 4);
				memset(m_lpBitsOnlyW, 255, sz.cx * sz.cy * 4);
				hr = OleDraw(lpV, DVASPECT_TRANSPARENT, m_hdcBack, &rTotal);
				hr = OleDraw(lpV, DVASPECT_TRANSPARENT, m_hdcBackW, &rTotal);
				dst = m_lpBitsOnly;
				dstW = m_lpBitsOnlyW;
				BYTE r, g, b, a, rw, gw, bw, aw, alpha_r, alpha_g, alpha_b, alpha;
				for (int y = 0; y < sz.cy; y++)
				{
					for (int x = 0; x < sz.cx; x++)
					{
						//the idea is that we draw the same data onto black and white DC's
						//and then calculate per pixel alpha based on difference, produced by alpha blending
						r = *dst++;
						g = *dst++;
						b = *dst++;
						a = *dst++;
						rw = *dstW++;
						gw = *dstW++;
						bw = *dstW++;
						aw = *dstW++;
						alpha_r = rw-r;
						alpha_g = gw-g;
						alpha_b = bw-b;
						//division by 3 is for accuracy and can be replaced by
						//alpha = alpha_g; for example
						alpha = (alpha_r + alpha_g + alpha_b) / 3;
						*(dst - 1) = 255 - alpha;
						//this algorithm should be optimized for MMX to achieve best performance
					}
				} 
			}
		}
		else //in 8/16/24 bit screen depth UpdateLayeredWindow produces wrong results - we use underlaying DC to paint to
		{
			HWND hwndParent = ::GetParent(hwnd);
			HDC hdcParent = ::GetWindowDC(hwndParent);
			BOOL bRet = BitBlt(m_hdcBack, 0, 0, rTotal.right, rTotal.bottom, hdcParent, 0, 0, SRCCOPY);
			::ReleaseDC(hwndParent, hdcParent);
			hr = OleDraw(lpV, DVASPECT_TRANSPARENT, m_hdcBack, &rTotal);
			dst = m_lpBitsOnly;
		}
	}

	BLENDFUNCTION bf;
	bf.BlendOp = AC_SRC_OVER;
	bf.AlphaFormat = AC_SRC_ALPHA;
	bf.BlendFlags = 0;
	bf.SourceConstantAlpha = 255;
	BOOL bRet = UpdateLayeredWindow(hwnd, NULL, &p, &sz, m_hdcBack, &p2, 0, &bf, m_iBPP == 32 ? ULW_ALPHA : ULW_OPAQUE);
}


//IStorage

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::CreateStream(const WCHAR *pwcsName, DWORD grfMode, DWORD reserved1, DWORD reserved2, IStream **ppstm)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OpenStream(const WCHAR * pwcsName, void *reserved1, DWORD grfMode, DWORD reserved2, IStream **ppstm)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::CreateStorage(const WCHAR *pwcsName, DWORD grfMode, DWORD reserved1, DWORD reserved2, IStorage **ppstg)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::OpenStorage(const WCHAR * pwcsName, IStorage * pstgPriority, DWORD grfMode, SNB snbExclude, DWORD reserved, IStorage **ppstg)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::CopyTo(DWORD ciidExclude, IID const *rgiidExclude, SNB snbExclude,IStorage *pstgDest)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::MoveElementTo(const OLECHAR *pwcsName,IStorage * pstgDest, const OLECHAR *pwcsNewName, DWORD grfFlags)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::Commit(DWORD grfCommitFlags)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::Revert()
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::EnumElements(DWORD reserved1, void * reserved2, DWORD reserved3, IEnumSTATSTG ** ppenum)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::DestroyElement(const OLECHAR *pwcsName)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::RenameElement(const WCHAR *pwcsOldName, const WCHAR *pwcsNewName)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetElementTimes(const WCHAR *pwcsName, FILETIME const *pctime, FILETIME const *patime, FILETIME const *pmtime)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetClass(REFCLSID clsid)
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetStateBits(DWORD grfStateBits, DWORD grfMask)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::Stat(STATSTG * pstatstg, DWORD grfStatFlag)
{
	return E_NOTIMPL;
}


//IOleInPlaceFrame

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetBorder(LPRECT lprectBorder)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::RequestBorderSpace(LPCBORDERWIDTHS pborderwidths)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetBorderSpace(LPCBORDERWIDTHS pborderwidths)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetActiveObject(IOleInPlaceActiveObject *pActiveObject, LPCOLESTR pszObjName)
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::InsertMenus(HMENU hmenuShared, LPOLEMENUGROUPWIDTHS lpMenuWidths)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetMenu(HMENU hmenuShared, HOLEMENU holemenu, HWND hwndActiveObject)
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::RemoveMenus(HMENU hmenuShared)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::SetStatusText(LPCOLESTR pszStatusText)
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::EnableModeless(BOOL fEnable)
{
	return(S_OK);
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::TranslateAccelerator(LPMSG lpmsg, WORD wID)
{
	return E_NOTIMPL;
}


//IDispatch
OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetTypeInfoCount(UINT __RPC_FAR *pctinfo)
{
	return E_NOTIMPL;
}
OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetTypeInfo( 
    /* [in] */ UINT iTInfo,
    /* [in] */ LCID lcid,
    /* [out] */ ITypeInfo __RPC_FAR *__RPC_FAR *ppTInfo)
{
	return E_NOTIMPL;
}
OLECONTAINER(HRESULT STDMETHODCALLTYPE)::GetIDsOfNames( 
    /* [in] */ REFIID riid,
    /* [size_is][in] */ LPOLESTR __RPC_FAR *rgszNames,
    /* [in] */ UINT cNames,
    /* [in] */ LCID lcid,
    /* [size_is][out] */ DISPID __RPC_FAR *rgDispId)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT STDMETHODCALLTYPE)::Invoke( 
    /* [in] */ DISPID dispIdMember,
    /* [in] */ REFIID riid,
    /* [in] */ LCID lcid,
    /* [in] */ WORD wFlags,
    /* [out][in] */ DISPPARAMS __RPC_FAR *pDispParams,
    /* [out] */ VARIANT __RPC_FAR *pVarResult,
    /* [out] */ EXCEPINFO __RPC_FAR *pExcepInfo,
    /* [out] */ UINT __RPC_FAR *puArgErr)
{
	return E_NOTIMPL;
}

OLECONTAINER(HRESULT)::OnBeforeShowingContent()
{
	return S_OK;
}

OLECONTAINER(HRESULT)::OnAfterShowingContent()
{
	return S_OK;
}

OLECONTAINER(LRESULT CALLBACK)::WndProcStatic(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	if (uMsg == WM_CREATE)
	{
		LPCREATESTRUCT lpcs = (LPCREATESTRUCT)lParam;
		SetWindowLong(hWnd, GWL_USERDATA, (long)lpcs->lpCreateParams);
		return 0;
	}
	COleContainerWindow<TObj> *lpWnd = (COleContainerWindow<TObj> *)GetWindowLong(hWnd, GWL_USERDATA);
	if (lpWnd)
		return lpWnd->WndProc(hWnd, uMsg, wParam, lParam);
	else
		return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

OLECONTAINER(LRESULT)::WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_CREATE:
		{
			LPCREATESTRUCT lpcs = (LPCREATESTRUCT)lParam;
			SetWindowLong(hWnd, GWL_USERDATA, (long)lpcs->lpCreateParams);
			return 0;
		}
		break;

	case WM_PAINT:
		{
			if (!m_bTransparent)
			{
				PAINTSTRUCT ps;
				HDC hdc = ::BeginPaint(GetHWND(), &ps);
				Draw(hdc, &ps.rcPaint, ps.fErase);
				::EndPaint(GetHWND(), &ps);
				return 0;
			}
		}
		break;

	case WM_NCHITTEST:
		{
			int x = LOWORD(lParam), y = HIWORD(lParam);
			if (m_lpO && m_lpViewObjectEx)
			{
				IViewObjectEx *lpV = m_lpViewObjectEx;
				POINT p = {x, y};
				DWORD dwRes;
				RECT rTotal;
				GetWindowRect(GetHWND(), &rTotal);
				HRESULT hr = lpV->QueryHitPoint(DVASPECT_CONTENT, &rTotal, p, 1, &dwRes);
				if (hr == S_OK)
				{
					if (dwRes == HITRESULT_OUTSIDE)
						return HTTRANSPARENT;
					else
						return HTCLIENT;
				}
			}
		}
		break;

	case WM_SIZE:
		{
			HRESULT hr;
			RECT rPos;
			GetClientRect(GetHWND(), &rPos);
			RECT rClip = rPos;
			if (m_lpInPlaceObjWindowless)
				hr = m_lpInPlaceObjWindowless->SetObjectRects(&rPos, &rClip);
			else if (m_lpInPlaceObj)
				hr = m_lpInPlaceObj->SetObjectRects(&rPos, &rClip);
			return 0;
		}
		break;
	}

	if (m_lpInPlaceObjWindowless)
	{
		if (uMsg == WM_MOUSEMOVE || uMsg == WM_LBUTTONDOWN || uMsg == WM_LBUTTONUP || uMsg == WM_LBUTTONDBLCLK
			|| uMsg == WM_RBUTTONDOWN || uMsg == WM_RBUTTONUP || uMsg == WM_RBUTTONDBLCLK
			|| uMsg == WM_MBUTTONDOWN || uMsg == WM_MBUTTONUP || uMsg == WM_MBUTTONDBLCLK
			|| uMsg == WM_MOUSEWHEEL 
			|| uMsg == WM_KEYDOWN || uMsg == WM_KEYUP || uMsg == WM_CHAR
			|| uMsg == WM_SETCURSOR
			)
		{
			HRESULT hr;
			LRESULT res;
			hr = m_lpInPlaceObjWindowless->OnWindowMessage(uMsg, wParam, lParam, &res);
			if (hr == S_OK)
				return res;
		}
	}

	return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

#endif