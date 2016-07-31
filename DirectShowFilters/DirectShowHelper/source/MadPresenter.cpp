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
#include "StdString.h"
#include "../../mpc-hc_subs/src/dsutil/DSUtil.h"

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

MPMadPresenter::MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, OAHWND parent, IDirect3DDevice9* pDevice, IMediaControl* pMediaControl) :
  CUnknown(NAME("MPMadPresenter"), NULL),
  m_pCallback(pCallback),
  m_dwGUIWidth(width),
  m_dwGUIHeight(height),
  m_hParent(parent),
  m_pDevice((IDirect3DDevice9Ex*)pDevice),
  m_pMediaControl(pMediaControl)
{
  Log("MPMadPresenter::Constructor() - instance 0x%x", this);
  m_pShutdown = false;
}

MPMadPresenter::~MPMadPresenter()
{
  CAutoLock cAutoLock(this);
  SAFE_DELETE(m_pMadD3DDev);
  SAFE_DELETE(m_pCallback);

  Log("MPMadPresenter::Destructor() - instance 0x%x", this);
}

void MPMadPresenter::InitializeOSD()
{
  {
    CAutoLock cAutoLock(this);

    CComQIPtr<IMadVROsdServices> pOsdServices = m_pMad;

    if (pOsdServices && !m_pInitOSD)
    {
      pOsdServices->OsdSetRenderCallback("MP-GUI", this, nullptr);
      Log("MPMadPresenter::OsdSetRenderCallback InitializeOSD for device 0x:%x", m_pMadD3DDev);
      m_pInitOSD = true;
    }
  }
}

void MPMadPresenter::InitializeOSDClear()
{
  {
    CAutoLock cAutoLock(this);

    CComQIPtr<IMadVROsdServices> pOsdServices = m_pMad;

    if (pOsdServices && !m_pShutdown)
    {
      pOsdServices->OsdSetRenderCallback("MP-GUI", nullptr, nullptr);
      AddRef();
      Log("MPMadPresenter::OsdSetRenderCallback InitializeOSD Clearing");
    }
  }
}

void MPMadPresenter::SetOSDCallback()
{
  {
    CAutoLock cAutoLock(this);
    InitializeOSD();
  }
}

IBaseFilter* MPMadPresenter::Initialize()
{
  CAutoLock cAutoLock(this);

  HRESULT hr = CoCreateInstance(CLSID_madVR, NULL, CLSCTX_INPROC_SERVER, __uuidof(IMadVRDirect3D9Manager), (void**)&m_pMad);

  if (FAILED(hr))
    return NULL;

  CComQIPtr<IBaseFilter> baseFilter = m_pMad;
  CComQIPtr<IMadVROsdServices> pOsdServices = m_pMad;
  CComQIPtr<IMadVRDirect3D9Manager> manager = m_pMad;
  CComQIPtr<IMadVRSubclassReplacement> pSubclassReplacement = m_pMad;
  CComQIPtr<ISubRender> pSubRender = m_pMad;
  CComQIPtr<IVideoWindow> pWindow = m_pMad;

  m_pMad->QueryInterface(&m_pCommand);

  if (!baseFilter || !pOsdServices || !manager || !pSubclassReplacement || !pSubRender || !m_pCommand || !pWindow)
    return NULL;

  //pOsdServices->OsdSetRenderCallback("MP-GUI", this); // Init OSD from later to avoid failed start on 3D (when D3D device is changed from madVR)
  manager->ConfigureDisplayModeChanger(false, true);

  pSubRender->SetCallback(m_subProxy);

  m_pCommand->SendCommandBool("disableSeekbar", true);

  // MPC-HC
  CWnd* m_pVideoWnd = CWnd::FromHandle(reinterpret_cast<HWND>(m_hParent));
  pWindow->put_Owner((OAHWND)m_pVideoWnd->m_hWnd);
  pWindow->put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN);
  pWindow->put_MessageDrain((OAHWND)m_pVideoWnd->m_hWnd);
  pWindow->SetWindowPosition(0, 0, m_dwGUIWidth, m_dwGUIHeight);

  for (CWnd* pWnd = m_pVideoWnd->GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetNextWindow()) {
    // 1. lets WM_SETCURSOR through (not needed as of now)
    // 2. allows CMouse::CursorOnWindow() to work with m_pVideoWnd
    pWnd->EnableWindow(FALSE);
  }

  // TODO implement IMadVRSubclassReplacement
  //pSubclassReplacement->DisableSubclassing();

  return baseFilter;
}

HRESULT MPMadPresenter::Shutdown()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    CAutoLock lock(this);

    Log("MPMadPresenter::Shutdown() scope start");

    m_pShutdown = true;

    if (m_pCallback)
    {
      m_pCallback->Release();
      m_pCallback = nullptr;
    }

    //// Delay for 2 seconds on init to clear all pending garbage from C#
    //Sleep(2000); // TODO Test for 3D

    Log("MPMadPresenter::Shutdown() scope done ");
  } // Scope for autolock

  m_pCallback = nullptr;

  if (m_pMad)
  {
    CComQIPtr<IVideoWindow> pWindow = m_pMad;
    CComQIPtr<IMadVROsdServices> pOsdServices = m_pMad;

    if (pWindow)
    {
      pWindow->put_Owner(reinterpret_cast<OAHWND>(nullptr));
      pWindow->put_Visible(false);
      pWindow.Release();
    }

    if (m_pCommand)
    {
      m_pCommand->SendCommandBool("disableExclusiveMode", true);
      m_pCommand->SendCommand("restoreDisplayModeNow");
      m_pCommand->Release();
    }

    if (m_pMadD3DDev) m_pMadD3DDev->Release();
    if (m_pDevice) m_pDevice->Release();
    if (m_pMadGuiVertexBuffer) m_pMadGuiVertexBuffer.Release();
    if (m_pMadOsdVertexBuffer) m_pMadOsdVertexBuffer.Release();
    if (m_pMadOsdVertexBuffer) m_pMadOsdVertexBuffer.Release();
    if (m_pRenderTextureOsd) m_pRenderTextureOsd.Release();
    if (m_pMPTextureGui) m_pMPTextureGui.Release();
    if (m_pMPTextureOsd) m_pMPTextureOsd.Release();
    if (m_pMad) m_pMad->Release();
    if (pOsdServices) pOsdServices->OsdSetRenderCallback("MP-GUI", nullptr, nullptr);
  }

  Log("MPMadPresenter::Shutdown()");

  return S_OK;
}

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
  if (ppvObject == NULL)
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

  WORD videoHeight = (WORD)activeVideoRect->bottom - (WORD)activeVideoRect->top;
  WORD videoWidth = (WORD)activeVideoRect->right - (WORD)activeVideoRect->left;

  bool uiVisible = false;

  CAutoLock cAutoLock(this);

  if (!m_pMPTextureGui || !m_pMadGuiVertexBuffer || !m_pRenderTextureGui || !m_pCallback)
    return CALLBACK_EMPTY;

  m_dwHeight = (WORD)fullOutputRect->bottom - (WORD)fullOutputRect->top;
  m_dwWidth = (WORD)fullOutputRect->right - (WORD)fullOutputRect->left;

  if (FAILED(hr = RenderToTexture(m_pMPTextureGui)))
    return hr;

  if (FAILED(hr = m_deviceState.Store()))
    return hr;

  if (FAILED(hr = m_pCallback->RenderGui(videoWidth, videoHeight, videoWidth, videoHeight)))
    return hr;

  uiVisible = hr == S_OK ? true : false;

  if (FAILED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
    return hr;

  if (FAILED(hr = SetupMadDeviceState()))
    return hr;

  if (FAILED(hr = SetupOSDVertex(m_pMadGuiVertexBuffer)))
    return hr;

  // Draw MP texture on madVR device's side
  if (FAILED(hr = RenderTexture(m_pMadGuiVertexBuffer, m_pRenderTextureGui)))
    return hr;

  if (FAILED(hr = m_deviceState.Restore()))
    return hr;

  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_EMPTY;
}

HRESULT MPMadPresenter::RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT* fullOutputRect, RECT* activeVideoRect)
{
  HRESULT hr = E_UNEXPECTED;

  WORD videoHeight = (WORD)activeVideoRect->bottom - (WORD)activeVideoRect->top;
  WORD videoWidth = (WORD)activeVideoRect->right - (WORD)activeVideoRect->left;

  bool uiVisible = false;

  CAutoLock cAutoLock(this);

  if (!m_pMPTextureOsd || !m_pMadOsdVertexBuffer || !m_pRenderTextureOsd || !m_pCallback)
    return CALLBACK_EMPTY;

  IDirect3DSurface9* SurfaceMadVr = nullptr; // This will be released by C# side

  m_dwHeight = (WORD)fullOutputRect->bottom - (WORD)fullOutputRect->top;
  m_dwWidth = (WORD)fullOutputRect->right - (WORD)fullOutputRect->left;

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

  if (FAILED(hr = RenderToTexture(m_pMPTextureOsd)))
    return hr;

  if (FAILED(hr = m_deviceState.Store()))
    return hr;

  if (FAILED(hr = m_pCallback->RenderOverlay(videoWidth, videoHeight, videoWidth, videoHeight)))
    return hr;

  uiVisible = hr == S_OK ? true : false;

  if (FAILED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
    return hr;

  if (FAILED(hr = SetupMadDeviceState()))
    return hr;

  if (FAILED(hr = SetupOSDVertex(m_pMadOsdVertexBuffer)))
    return hr;

  // Draw MP texture on madVR device's side
  if (FAILED(hr = RenderTexture(m_pMadOsdVertexBuffer, m_pRenderTextureOsd)))
    return hr;

  if (FAILED(hr = m_deviceState.Restore()))
    return hr;

  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_EMPTY;
}

HRESULT MPMadPresenter::RenderToTexture(IDirect3DTexture9* pTexture)
{
  HRESULT hr = E_UNEXPECTED;
  IDirect3DSurface9* pSurface = nullptr; // This will be relased by C# side

  if (FAILED(hr = pTexture->GetSurfaceLevel(0, &pSurface)))
    return hr;

  if (FAILED(hr = m_pCallback->SetRenderTarget((DWORD)pSurface)))
    return hr;

  if (FAILED(hr = m_pDevice->Clear(0, NULL, D3DCLEAR_TARGET, D3DXCOLOR(0, 0, 0, 0), 1.0f, 0)))
    return hr;

  return hr;
}

HRESULT MPMadPresenter::RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture)
{
  if (!m_pMadD3DDev)
    return S_OK;

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

  return S_OK;
  //Log("RenderTexture hr: 0x%08x", hr);
}

HRESULT MPMadPresenter::SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf)
{
  VID_FRAME_VERTEX* vertices = nullptr;

  // Lock the vertex buffer
  HRESULT hr = pVertextBuf->Lock(0, 0, (void**)&vertices, D3DLOCK_DISCARD);

  if (SUCCEEDED(hr))
  {
    RECT rDest;
    rDest.bottom = m_dwHeight;
    rDest.left = 0;
    rDest.right = m_dwWidth;
    rDest.top = 0;

    vertices[0].x = (float)rDest.left - 0.5f;
    vertices[0].y = (float)rDest.top - 0.5f;
    vertices[0].z = 0.0f;
    vertices[0].rhw = 1.0f;
    vertices[0].u = 0.0f;
    vertices[0].v = 0.0f;

    vertices[1].x = (float)rDest.right - 0.5f;
    vertices[1].y = (float)rDest.top - 0.5f;
    vertices[1].z = 0.0f;
    vertices[1].rhw = 1.0f;
    vertices[1].u = 1.0f;
    vertices[1].v = 0.0f;

    vertices[2].x = (float)rDest.right - 0.5f;
    vertices[2].y = (float)rDest.bottom - 0.5f;
    vertices[2].z = 0.0f;
    vertices[2].rhw = 1.0f;
    vertices[2].u = 1.0f;
    vertices[2].v = 1.0f;

    vertices[3].x = (float)rDest.left - 0.5f;
    vertices[3].y = (float)rDest.bottom - 0.5f;
    vertices[3].z = 0.0f;
    vertices[3].rhw = 1.0f;
    vertices[3].u = 0.0f;
    vertices[3].v = 1.0f;

    hr = pVertextBuf->Unlock();
    if (FAILED(hr))
      return hr;
  }

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

  return hr;
}

HRESULT MPMadPresenter::SetDevice(IDirect3DDevice9* pD3DDev)
{
  HRESULT hr = S_FALSE;

  CAutoLock cAutoLock(this);

  Log("MPMadPresenter::SetDevice() device 0x:%x", pD3DDev);

  m_pMadD3DDev = (IDirect3DDevice9Ex*)pD3DDev;

  if (m_pCallback && pD3DDev)
  {
    m_deviceState.SetDevice(pD3DDev);
    m_pCallback->SetSubtitleDevice((DWORD)pD3DDev);
    Log("MPMadPresenter::SetDevice() SetSubtitleDevice for D3D : 0x:%x", m_pMadD3DDev);
    //if (m_pMediaControl)
    //{
    //  OAFilterState _fs = -1;
    //  if (m_pMediaControl) m_pMediaControl->GetState(1000, &_fs);
    //  if (_fs == State_Paused)
    //    m_pMediaControl->Run();
    //  Log("MPMadPresenter::SetDevice() m_pMediaControl : 0x:%x", _fs);
    //}
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

  return hr;
}
