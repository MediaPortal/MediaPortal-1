// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "StdAfx.h"

#include <initguid.h>
#include <streams.h>
#include <d3dx9.h>

#include "madpresenter.h"
#include "dshowhelper.h"
#include "mvrInterfaces.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"
#include <vector>
#include "StdString.h"
#include <afxwin.h>

class CWnd;
const DWORD D3DFVF_VID_FRAME_VERTEX = D3DFVF_XYZRHW | D3DFVF_TEX1;

struct VID_FRAME_VERTEX
{
  float x;
  float y;
  float z;
  float rhw;
  float u;
  float v;
};

MPMadPresenter::MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, HWND parent, IDirect3DDevice9* pDevice, IMediaControl* pMediaControl, IGraphBuilder* mGraphbuilder) :
  CUnknown(NAME("MPMadPresenter"), nullptr),
  m_pCallback(pCallback),
  m_dwGUIWidth(width),
  m_dwGUIHeight(height),
  m_hParent(parent),
  m_pDevice(static_cast<IDirect3DDevice9Ex*>(pDevice)),
  m_pMediaControl(pMediaControl),
  m_pGraphBuilder(mGraphbuilder)
{
  Log("MPMadPresenter::Constructor() - instance 0x%x", this);
  m_subProxy = new MadSubtitleProxy(pCallback, m_pMediaControl, this);
  if (m_subProxy)
    m_subProxy->AddRef();
  m_pShutdown = false;
  m_pInitOSDClear = false;
  //m_exclusiveCallback = MPMadPresenterExclusive::ExclusiveCallback;
}

MPMadPresenter::~MPMadPresenter()
{
  Log("MPMadPresenter::Destructor() - instance 0x%x", this);
  CAutoLock cAutoLock(this);
}

void MPMadPresenter::InitializeOSD()
{
  {
    if (!m_pCallback)
    {
      Log("InitializeOSD m_pCallback : (0x%x)", m_pCallback);
      return;
    }

    CAutoLock cAutoLock(this);

    if (m_pOsdServices)// && m_pMadD3DDevBackup != m_pMadD3DDev || !m_pMadD3DDevBackup)
    {
      InitializeOSDClear();
      m_pOsdServices->OsdSetRenderCallback("MP-GUI", this, nullptr);
      Log("MPMadPresenter::OsdSetRenderCallback InitializeOSD");
    }
  }
}

void MPMadPresenter::InitializeOSDClear()
{
  {
    if (!m_pCallback)
    {
      Log("InitializeOSDClear m_pCallback : (0x%x)", m_pCallback);
      return;
    }

    CAutoLock cAutoLock(this);

    if (m_pOsdServices && !m_pShutdown)
    {
      Log("MPMadPresenter::OsdSetRenderCallback InitializeOSDClear");
      m_pOsdServices->OsdSetRenderCallback("MP-GUI", nullptr, nullptr);
      m_pInitOSDClear = true;
      AddRef();
    }
  }
}

//void MPMadPresenter::ForceMessage()
//{
//  {
//    if (!m_pCallback)
//    {
//      Log("ForceMessage m_pCallback : (0x%x)", m_pCallback);
//      return;
//    }
//
//    //CAutoLock cAutoLock(this);
//
//    //bool madVRInExclusiveMode = m_pInfo->GetBool("exclusiveModeActive", &m_ExclusiveMode);
//    m_pSettings2->SettingsGetBoolean(L"enableExclusive", &m_ExclusiveMode);
//    if (m_ExclusiveMode)
//    {
//      Log("ForceMessage trick 1 : (0x%x)", this);
//      m_pWindow->put_Owner(reinterpret_cast<OAHWND>(nullptr));
//      m_pWindow->put_Visible(false);
//      m_pWindow->put_Visible(true);
//      /*m_pWindow->put_Owner(m_hParent);*/
//      //m_pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hParent));
//      m_pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hWnd));
//      Log("ForceMessage trick 2 : (0x%x)", this);
//    }
//  }
//}

//void MPMadPresenter::ForceMessageExclusive()
//{
//  {
//    if (!m_pCallback)
//    {
//      Log("ForceMessageExclusive m_pCallback : (0x%x)", m_pCallback);
//      return;
//    }
//
//    CAutoLock cAutoLock(this);
//
//    if (!m_pShutdown)
//    {
//      Log("ForceMessageExclusive trick 1 : (0x%x)", this);
//      m_pWindow->put_Owner(reinterpret_cast<OAHWND>(nullptr));
//      m_pWindow->put_Visible(false);
//      m_pWindow->put_Visible(true);
//      //m_pWindow->put_Owner(m_hParent);
//      m_pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hParent));
//      //m_pMediaControl->Run();
//      Log("ForceMessageExclusive trick 2 : (0x%x)", this);
//    }
//  }
//}

IBaseFilter* MPMadPresenter::Initialize()
{
  CAutoLock cAutoLock(this);

  //Log("MPMadPresenter::Init 1()");
  HRESULT hr = CoCreateInstance(CLSID_madVR, nullptr, CLSCTX_INPROC_SERVER, __uuidof(IMadVRDirect3D9Manager), reinterpret_cast<void**>(&m_pMad));

  //Log("MPMadPresenter::Init 2()");
  if (FAILED(hr))
    return nullptr;

  m_pMad->QueryInterface(&m_pBaseFilter);
  m_pMad->QueryInterface(&m_pOsdServices);
  m_pMad->QueryInterface(&m_pExclusiveModeCallback);
  m_pMad->QueryInterface(&m_pManager);
  m_pMad->QueryInterface(&m_pSubclassReplacement);
  m_pMad->QueryInterface(&m_pSubRender);
  m_pMad->QueryInterface(&m_pWindow);
  m_pMad->QueryInterface(&m_pCommand);
  //m_pMad->QueryInterface(&m_pInfo);
  //m_pMad->QueryInterface(&m_pSettings2);

  //Log("MPMadPresenter::Init 3()");

  if (!m_pBaseFilter || !m_pOsdServices || !m_pManager || !m_pSubclassReplacement || !m_pSubRender || !m_pCommand || !m_pWindow)
    return nullptr;
  //Log("MPMadPresenter::Init 4()");

  // Adding madVR to the graph (from C++) or comment out and adding it from C#.
  //hr = m_pGraphBuilder->AddFilter(m_pBaseFilter, L"madVR");

  // Create a madVR Window
  //if (InitMadvrWindow(m_hWndKodi))
  //  Log("%s : Create DSPlayer window - hWnd: %i", __FUNCTION__, m_hWndKodi);

  m_pManager->ConfigureDisplayModeChanger(true, true);
  //Log("MPMadPresenter::Init 5()");

  m_pSubRender->SetCallback(m_subProxy);
  //Log("MPMadPresenter::Init 6()");

  m_pCommand->SendCommandBool("disableSeekbar", true);
  //Log("MPMadPresenter::Init 7()");

  //if (m_pWindow)
  //{
  //  //m_pWindow->put_Owner(m_hParent);
  //  m_pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hWndKodi));
  //  m_pWindow->put_MessageDrain(reinterpret_cast<OAHWND>(m_hParent));
  //  m_pWindow->SetWindowPosition(0, 0, m_dwGUIWidth, m_dwGUIHeight);
  //  SetDsWndVisible(true);
  //}

  // MPC-HC
  CWnd* m_pVideoWnd = CWnd::FromHandle(m_hParent);
  m_pWindow->put_Owner(reinterpret_cast<OAHWND>(m_pVideoWnd->m_hWnd));
  m_pWindow->put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN);
  m_pWindow->put_MessageDrain(reinterpret_cast<OAHWND>(m_pVideoWnd->m_hWnd));

  for (CWnd* pWnd = m_pVideoWnd->GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetNextWindow()) {
    // 1. lets WM_SETCURSOR through (not needed as of now)
    // 2. allows CMouse::CursorOnWindow() to work with m_pVideoWnd
    pWnd->EnableWindow(FALSE);
  }

  // TODO implement IMadVRSubclassReplacement
  //m_pSubclassReplacement->DisableSubclassing();

  //m_pExclusiveModeCallback->Register(m_exclusiveCallback, this);

  return m_pBaseFilter;
}

//void MPMadPresenter::Initialize2()
//{
//  CAutoLock cAutoLock(this);
//  if (m_pWindow)
//  {
//    if (m_pMediaControl)
//    {
//      OAFilterState _fs = -1;
//      if (m_pMediaControl) m_pMediaControl->GetState(1000, &_fs);
//      if (_fs == State_Running)
//        m_pMediaControl->Run();
//    }
//    Log("MPMadPresenter::Initialize2()");
//  }
//}

void MPMadPresenter::DeInitMadvrWindow()
{
  // remove ourself as user data to ensure we're not called anymore

  SetWindowLongPtr(m_hWndKodi, GWL_USERDATA, 0);

  // destroy the hidden window
  DestroyWindow(m_hWndKodi);

  // unregister the window class
  //UnregisterClass(m_className.c_str(), m_hInstance);

  // reset the hWnd
  m_hWndKodi = nullptr;
}


bool MPMadPresenter::InitMadvrWindow(HWND &hWnd)
{
  m_hInstance = static_cast<HINSTANCE>(GetModuleHandle(nullptr));
  if (m_hInstance == nullptr)
    Log("%s : GetModuleHandle failed with %d", __FUNCTION__, GetLastError());

  int nWidth = m_dwGUIWidth;
  int nHeight = m_dwGUIHeight;
  CStdString m_className = "MediaPortal:DSPlayer";

  // Register the windows class
  WNDCLASS wndClass;

  wndClass.style = CS_HREDRAW | CS_VREDRAW | CS_NOCLOSE;
  wndClass.lpfnWndProc = MPMadPresenter::WndProc;
  wndClass.cbClsExtra = 0;
  wndClass.cbWndExtra = 0;
  wndClass.hInstance = m_hInstance;
  wndClass.hIcon = nullptr;
  wndClass.hCursor = nullptr;
  wndClass.hbrBackground = static_cast<HBRUSH>(GetStockObject(BLACK_BRUSH));
  wndClass.lpszMenuName = nullptr;
  wndClass.lpszClassName = m_className.c_str();

  if (!RegisterClass(&wndClass))
  {
    //Log("%s : RegisterClass failed with %d", __FUNCTION__, GetLastError());
    //return false;
  }
  hWnd = CreateWindow(m_className.c_str(), m_className.c_str(),
    WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
    0, 0, nWidth, nHeight,
    m_hParent, NULL, m_hInstance, NULL);
  if (hWnd == nullptr)
  {
   Log("%s : CreateWindow failed with %d", __FUNCTION__, GetLastError());
    return false;
  }

  if (hWnd)
    SetWindowLongPtr(hWnd, GWL_USERDATA, NPT_POINTER_TO_LONG(this));

  return true;
}

LRESULT CALLBACK MPMadPresenter::WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
  HWND g_hWnd = nullptr;
  switch (uMsg)
  {
  case WM_MOUSEMOVE:
  case WM_LBUTTONDOWN:
  case WM_MBUTTONDOWN:
  case WM_RBUTTONDOWN:
  case WM_LBUTTONUP:
  case WM_MBUTTONUP:
  case WM_RBUTTONUP:
  case WM_MOUSEWHEEL:
    ::PostMessage(g_hWnd, uMsg, wParam, lParam);
    return(0);
  case WM_SIZE:
    SetWindowPos(hWnd, 0, 0, 0, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam), SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOZORDER);
    return(0);
  }
  return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

void MPMadPresenter::SetDsWndVisible(bool bVisible)
{
  int cmd;
  bVisible ? cmd = SW_SHOW : cmd = SW_HIDE;
  ShowWindow(m_hWndKodi, cmd);
  UpdateWindow(m_hWndKodi);
}


HRESULT MPMadPresenter::Shutdown()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    CAutoLock lock(this);

    Log("MPMadPresenter::Shutdown() start");

    m_pShutdown = true;

    if (m_pInitOSDClear)
    {
      m_pRefCount = Release();
      m_pRefCount = m_pRefCount - 1;
      Log("MPMadPresenter::Shutdown() reference counter to be released for OSD : (%d)", m_pRefCount);
      for (int i = 0; i < m_pRefCount; ++i)
      {
        Release();
      }
    }

    if (m_pCallback)
    {
      m_pCallback->Release();
      m_pCallback = nullptr;
    }

    Log("MPMadPresenter::Shutdown() Scope 2 ");

    if (m_pSubRender)
      m_pSubRender->SetCallback(nullptr);

    Log("MPMadPresenter::Shutdown() Scope 3 ");

    if (m_subProxy)
    {
      m_subProxy->Release();
      m_subProxy = nullptr;
    }

    DeInitMadvrWindow();

    //// Delay for 2 seconds on init to clear all pending garbage from C#
    //Sleep(2000);

    Log("MPMadPresenter::Shutdown() done ");
  } // Scope for autolock

  //Log("MPMadPresenter::Shutdown() start OSD");
  //if (m_pOsdServices)
  //{
  //  m_pOsdServices->OsdSetRenderCallback("MP-GUI", nullptr, nullptr);
  //}
  //Log("MPMadPresenter::Shutdown() done OSD");

  if (m_pMad)
  {
    Log("MPMadPresenter::Shutdown() 1");

    if (m_pWindow)
    {
      Log("MPMadPresenter::Shutdown() 2");
      m_pWindow->put_Owner(reinterpret_cast<OAHWND>(nullptr));
      m_pWindow->put_Visible(false);
      Log("MPMadPresenter::Shutdown() 3");
    }

    if (m_pCommand)
    {
      Log("MPMadPresenter::Shutdown() 4");
      m_pCommand->SendCommandBool("disableExclusiveMode", true);
      m_pCommand->SendCommand("restoreDisplayModeNow");
      Log("MPMadPresenter::Shutdown() 5");
    }

    //if (m_pExclusiveModeCallback)
    //  m_pExclusiveModeCallback->Unregister(m_exclusiveCallback, this);

    Log("MPMadPresenter::Shutdown() start OSD");
    if (m_pOsdServices)
    {
      m_pOsdServices->OsdSetRenderCallback("MP-GUI", nullptr, nullptr);
    }
    Log("MPMadPresenter::Shutdown() done OSD");
  }

  return S_OK;
}

//void MPMadPresenterExclusive::ExclusiveCallback(LPVOID context, int event)
//{
//  MPMadPresenter *pThis = static_cast<MPMadPresenter*>(context);
//
//  vector<std::string> strEvent = { "IsAboutToBeEntered", "WasJustEntered", "IsAboutToBeLeft", "WasJustLeft" };
//
//  if (event == ExclusiveModeIsAboutToBeEntered || event == ExclusiveModeIsAboutToBeLeft)
//  {
//    pThis->m_isEnteringExclusive = true;
//    //pThis->ForceMessageExclusive();
//  }
//
//  if (event == ExclusiveModeWasJustEntered || event == ExclusiveModeWasJustLeft)
//  {
//    pThis->m_isEnteringExclusive = false;
//    //pThis->ForceMessageExclusive();
//  }
//
//  Log("%s madVR %s in Fullscreen Exclusive-Mode", __FUNCTION__, strEvent[event - 1].c_str());
//}

HRESULT MPMadPresenter::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  if (riid == __uuidof(IUnknown))
    return __super::NonDelegatingQueryInterface(riid, ppv);

  HRESULT hr = QueryInterface(riid, ppv);
  return SUCCEEDED(hr) ? hr : __super::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT MPMadPresenter::QueryInterface(REFIID riid, void** ppvObject)
{
  HRESULT hr = E_NOINTERFACE;
  if (ppvObject == nullptr)
    hr = E_POINTER;
  else if (riid == __uuidof(IOsdRenderCallback))
  {
    *ppvObject = static_cast<IOsdRenderCallback*>(this);
    AddRef();
    hr = S_OK;
  }
  else if (riid == __uuidof(ISubRender))
  {
    if (m_subProxy)
    {
      *ppvObject = static_cast<ISubRenderCallback*>(m_subProxy);
      AddRef();
      hr = S_OK;
    }
  }

  return hr;
}

ULONG MPMadPresenter::AddRef()
{
  return NonDelegatingAddRef();
}

ULONG MPMadPresenter::Release()
{
  return NonDelegatingRelease();
}

ULONG MPMadPresenter::NonDelegatingRelease()
{
  return __super::NonDelegatingRelease();
}

ULONG MPMadPresenter::NonDelegatingAddRef()
{
  return __super::NonDelegatingAddRef();
}

HRESULT MPMadPresenter::ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT* fullOutputRect, RECT* activeVideoRect)
{
  HRESULT hr = E_UNEXPECTED;

  WORD videoHeight = static_cast<WORD>(activeVideoRect->bottom) - static_cast<WORD>(activeVideoRect->top);
  WORD videoWidth = static_cast<WORD>(activeVideoRect->right) - static_cast<WORD>(activeVideoRect->left);

  bool uiVisible = false;

  CAutoLock cAutoLock(this);

  if (!m_pMPTextureGui || !m_pMadGuiVertexBuffer || !m_pRenderTextureGui || !m_pCallback)
    return CALLBACK_EMPTY;

  m_dwHeight = static_cast<WORD>(fullOutputRect->bottom) - static_cast<WORD>(fullOutputRect->top);
  m_dwWidth = static_cast<WORD>(fullOutputRect->right) - static_cast<WORD>(fullOutputRect->left);

  RenderToTexture(m_pMPTextureGui, videoWidth, videoHeight, videoWidth, videoHeight);

  if (FAILED(hr = m_deviceState.Store()))
  {
    Log("ClearBackground hr1: 0x%08x", hr);
    return hr;
  }

  if (FAILED(hr = m_pCallback->RenderGui(videoWidth, videoHeight, videoWidth, videoHeight)))
  {
    Log("ClearBackground hr2: 0x%08x", hr);
    return hr;
  }

  uiVisible = hr == S_OK ? true : false;

  if (FAILED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
  {
    Log("ClearBackground hr3: 0x%08x", hr);
    return hr;
  }

  if (FAILED(hr = SetupMadDeviceState()))
  {
    Log("ClearBackground hr4: 0x%08x", hr);
    return hr;
  }

  if (FAILED(hr = SetupOSDVertex(m_pMadGuiVertexBuffer)))
  {
    Log("ClearBackground hr5: 0x%08x", hr);
    return hr;
  }

  // Draw MP texture on madVR device's side
  RenderTexture(m_pMadGuiVertexBuffer, m_pRenderTextureGui);

  if (FAILED(hr = m_deviceState.Restore()))
  {
    Log("ClearBackground hr6: 0x%08x", hr);
    return hr;
  }

  //Log("ClearBackground hr: 0x%08x", hr);
  //Log("ClearBackground uiVisible: 0x%08x", uiVisible);
  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_EMPTY;
}

HRESULT MPMadPresenter::RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT* fullOutputRect, RECT* activeVideoRect)
{
  HRESULT hr = E_UNEXPECTED;

  WORD videoHeight = static_cast<WORD>(activeVideoRect->bottom) - static_cast<WORD>(activeVideoRect->top);
  WORD videoWidth = static_cast<WORD>(activeVideoRect->right) - static_cast<WORD>(activeVideoRect->left);

  bool uiVisible = false;

  CAutoLock cAutoLock(this);

  if (!m_pMPTextureOsd || !m_pMadOsdVertexBuffer || !m_pRenderTextureOsd || !m_pCallback)
    return CALLBACK_EMPTY;

  IDirect3DSurface9* SurfaceMadVr = nullptr; // This will be released by C# side

  m_dwHeight = static_cast<WORD>(fullOutputRect->bottom) - static_cast<WORD>(fullOutputRect->top);
  m_dwWidth = static_cast<WORD>(fullOutputRect->right) - static_cast<WORD>(fullOutputRect->left);

  // Handle GetBackBuffer to be done only 2 frames
  countFrame++;
  if (countFrame == firstFrame || countFrame == secondFrame)
  {
    if (SUCCEEDED(hr = m_pMadD3DDev->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &SurfaceMadVr)))
    {
      if (SUCCEEDED(hr = m_pCallback->RenderFrame(videoWidth, videoHeight, videoWidth, videoHeight, reinterpret_cast<DWORD>(SurfaceMadVr))))
      {
        SurfaceMadVr->Release();
      }
      if (countFrame == secondFrame)
      {
        countFrame = resetFrame;
      }
    }
  }

  RenderToTexture(m_pMPTextureOsd, videoWidth, videoHeight, videoWidth, videoHeight);

  if (FAILED(hr = m_deviceState.Store()))
  {
    Log("RenderOsd hr1: 0x%08x", hr);
    return hr;
  }

  if (FAILED(hr = m_pCallback->RenderOverlay(videoWidth, videoHeight, videoWidth, videoHeight)))
  {
    Log("RenderOsd hr2: 0x%08x", hr);
    return hr;
  }

  uiVisible = hr == S_OK ? true : false;

  if (FAILED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
  {
    Log("RenderOsd hr3: 0x%08x", hr);
    return hr;
  }

  if (FAILED(hr = SetupMadDeviceState()))
  {
    Log("RenderOsd hr4: 0x%08x", hr);
    return hr;
  }

  if (FAILED(hr = SetupOSDVertex(m_pMadOsdVertexBuffer)))
  {
    Log("RenderOsd hr5: 0x%08x", hr);
    return hr;
  }

  // Draw MP texture on madVR device's side
  RenderTexture(m_pMadOsdVertexBuffer, m_pRenderTextureOsd);

  if (FAILED(hr = m_deviceState.Restore()))
  {
    Log("RenderOsd hr6: 0x%08x", hr);
    return hr;
  }

  //Log("RenderOsd hr: 0x%08x", hr);
  //Log("RenderOsd uiVisible: 0x%08x", uiVisible);
  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_EMPTY;
}

void MPMadPresenter::RenderToTexture(IDirect3DTexture9* pTexture, WORD cx, WORD cy, WORD arx, WORD ary)
{
  if (!m_pDevice)
    return;

  //if (!m_pCallback)
  //{
  //  Log("RenderToTexture m_pCallback : (0x%x)", m_pCallback);
  //  return;
  //}

  HRESULT hr = E_UNEXPECTED;
  IDirect3DSurface9* pSurface = nullptr; // This will be released by C# side
  if (SUCCEEDED(hr = pTexture->GetSurfaceLevel(0, &pSurface)))
  {
    //Log("RenderToTexture GetSurfaceLevel hr: 0x%08x", hr);
    if (SUCCEEDED(hr = m_pCallback->SetRenderTarget(reinterpret_cast<DWORD>(pSurface))))
    {
      //Log("RenderToTexture SetRenderTarget hr: 0x%08x", hr);
      hr = m_pDevice->Clear(0, nullptr, D3DCLEAR_TARGET, D3DXCOLOR(0, 0, 0, 0), 1.0f, 0);
      //Log("RenderToTexture SetRenderTarget Clear hr: 0x%08x", hr);
    }
  }
  //Log("RenderToTexture hr: 0x%08x", hr);
}

void MPMadPresenter::RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture)
{
  if (!m_pMadD3DDev)
    return;

  HRESULT hr = E_UNEXPECTED;

  if (SUCCEEDED(hr = m_pMadD3DDev->SetStreamSource(0, pVertexBuf, 0, sizeof(VID_FRAME_VERTEX))))
  {
    //Log("RenderTexture SetStreamSource hr: 0x%08x", hr);
    if (SUCCEEDED(hr = m_pMadD3DDev->SetTexture(0, pTexture)))
    {
      //Log("RenderTexture SetTexture hr: 0x%08x", hr);
      hr = m_pMadD3DDev->DrawPrimitive(D3DPT_TRIANGLEFAN, 0, 2);
      //Log("RenderTexture DrawPrimitive hr: 0x%08x", hr);
    }
  }
  //Log("RenderTexture hr: 0x%08x", hr);
}

HRESULT MPMadPresenter::SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf)
{
  VID_FRAME_VERTEX* vertices = nullptr;

  // Lock the vertex buffer
  HRESULT hr = pVertextBuf->Lock(0, 0, reinterpret_cast<void**>(&vertices), D3DLOCK_DISCARD);

  if (SUCCEEDED(hr))
  {
    RECT rDest;
    rDest.bottom = m_dwHeight;
    rDest.left = 0;
    rDest.right = m_dwWidth;
    rDest.top = 0;

    vertices[0].x = static_cast<float>(rDest.left);
    vertices[0].y = static_cast<float>(rDest.top);
    vertices[0].z = 0.0f;
    vertices[0].rhw = 1.0f;
    vertices[0].u = 0.0f;
    vertices[0].v = 0.0f;

    vertices[1].x = static_cast<float>(rDest.right);
    vertices[1].y = static_cast<float>(rDest.top);
    vertices[1].z = 0.0f;
    vertices[1].rhw = 1.0f;
    vertices[1].u = 1.0f;
    vertices[1].v = 0.0f;

    vertices[2].x = static_cast<float>(rDest.right);
    vertices[2].y = static_cast<float>(rDest.bottom);
    vertices[2].z = 0.0f;
    vertices[2].rhw = 1.0f;
    vertices[2].u = 1.0f;
    vertices[2].v = 1.0f;

    vertices[3].x = static_cast<float>(rDest.left);
    vertices[3].y = static_cast<float>(rDest.bottom);
    vertices[3].z = 0.0f;
    vertices[3].rhw = 1.0f;
    vertices[3].u = 0.0f;
    vertices[3].v = 1.0f;

    // Update vertices to compensate texel/pixel coordinate origins (top left of pixel vs. center of texel)
    // See https://msdn.microsoft.com/en-us/library/bb219690(VS.85).aspx
    for (int i = 0; i < 4; i++)
    {
      vertices[i].x -= 0.5f;
      vertices[i].y -= 0.5f;
    }

    hr = pVertextBuf->Unlock();
    if (FAILED(hr))
      return hr;
  }

  //Log("SetupOSDVertex hr: 0x%08x", hr);
  return hr;
}

HRESULT MPMadPresenter::SetupMadDeviceState()
{
  HRESULT hr = E_UNEXPECTED;

  RECT newScissorRect;
  newScissorRect.bottom = m_dwHeight;
  newScissorRect.top = 0;
  newScissorRect.left = 0;
  newScissorRect.right = m_dwWidth;

  if (FAILED(hr = m_pMadD3DDev->SetScissorRect(&newScissorRect)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetVertexShader(NULL)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetFVF(D3DFVF_VID_FRAME_VERTEX)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetPixelShader(NULL)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_LIGHTING, FALSE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ZENABLE, FALSE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_ONE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA)))
    return hr;

  //Log("SetupMadDeviceState hr: 0x%08x", hr);
  return hr;
}

HRESULT MPMadPresenter::SetDevice(IDirect3DDevice9* pD3DDev)
{
  HRESULT hr = S_FALSE;

  Log("MPMadPresenter::SetDevice() pD3DDev : 0x:%x", pD3DDev);

  //if (!m_pCallback)
  //{
  //  Log("MPMadPresenter::SetDevice() m_pCallback : (0x%x)", m_pCallback);
  //  return S_OK;
  //}

  CAutoLock cAutoLock(this);

  if (!pD3DDev)
    return S_OK;

  if (!m_pCallback)
    return S_OK;

  m_pMadD3DDev = static_cast<IDirect3DDevice9Ex*>(pD3DDev);
  m_deviceState.SetDevice(pD3DDev);

  // Backup device 
  //m_pMadD3DDevBackup = m_pMadD3DDev;

  if (m_pCallback && pD3DDev)
  {
    m_pCallback->SetSubtitleDevice((DWORD)pD3DDev);
    Log("MPMadPresenter::SetDevice() SetSubtitleDevice for D3D : 0x:%x", m_pMadD3DDev);
    if (m_pMediaControl)
    {
      OAFilterState _fs = -1;
      if (m_pMediaControl) m_pMediaControl->GetState(1000, &_fs);
      if (_fs == State_Paused)
        m_pMediaControl->Run();
      Log("MPMadPresenter::SetDevice() m_pMediaControl : 0x:%x", _fs);
    }
  }

  if (m_pMadD3DDev)
  {
    if (FAILED(hr = m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadGuiVertexBuffer.p, NULL)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadOsdVertexBuffer.p, NULL)))
      return hr;

    if (FAILED(hr = m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureGui.p, &m_hSharedGuiHandle)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureGui.p, &m_hSharedGuiHandle)))
      return hr;

    if (FAILED(hr = m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureOsd.p, &m_hSharedOsdHandle)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureOsd.p, &m_hSharedOsdHandle)))
      return hr;
  }
  else
    m_pMadD3DDev = nullptr;

  //Log("SetDevice hr: 0x%08x", hr);
  return hr;
}