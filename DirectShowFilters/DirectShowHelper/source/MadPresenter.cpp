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

MPMadPresenter::MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, OAHWND parent, IDirect3DDevice9* pDevice) :
  CUnknown(NAME("MPMadPresenter"), nullptr),
  m_pCallback(pCallback),
  m_dwGUIWidth(width),
  m_dwGUIHeight(height),
  m_hParent(parent),
  m_pDevice(static_cast<IDirect3DDevice9Ex*>(pDevice))
{
  Log("MPMadPresenter::Constructor() - instance 0x%x", this);
  m_subProxy = new MadSubtitleProxy(pCallback);
  if (m_subProxy)
    m_subProxy->AddRef();
  Log("MPMadPresenter::MPMadPresenter() - instance 0x%x");
}

MPMadPresenter::~MPMadPresenter()
{
  Log("MPMadPresenter::Destructor() - instance 0x%x", this);

  CAutoLock cAutoLock(this);

  Log("MPMadPresenter::Destructor() 2 ");

  if (m_pCallback)
  {
    m_pCallback->Release();
    m_pCallback = nullptr;
  }

  Log("MPMadPresenter::Destructor() 3 ");

  if (m_pSubRender)
    m_pSubRender->SetCallback(nullptr);

  Log("MPMadPresenter::Destructor() 4 ");

  if (m_subProxy)
  {
    m_subProxy->Release();
    m_subProxy = nullptr;
  }

  Log("MPMadPresenter::Destructor() 5 ");
}

void MPMadPresenter::InitializeOSD()
{
  CAutoLock cAutoLock(this);

  if (m_pOsdServices)
  {
    m_pOsdServices->OsdSetRenderCallback("MP-GUI", this, nullptr);
    Log("MPMadPresenter::InitializeOSD");
  }
}

IBaseFilter* MPMadPresenter::Initialize()
{
  CAutoLock cAutoLock(this);

  Log("MPMadPresenter::Init 1()");
  HRESULT hr = CoCreateInstance(CLSID_madVR, nullptr, CLSCTX_INPROC_SERVER, __uuidof(IMadVRDirect3D9Manager), reinterpret_cast<void**>(&m_pMad));

  Log("MPMadPresenter::Init 2()");
  if (FAILED(hr))
    return nullptr;

  m_pMad->QueryInterface(&m_pBaseFilter);
  m_pMad->QueryInterface(&m_pOsdServices);
  m_pMad->QueryInterface(&m_pManager);
  m_pMad->QueryInterface(&m_pSubclassReplacement);
  m_pMad->QueryInterface(&m_pSubRender);
  m_pMad->QueryInterface(&m_pWindow);
  m_pMad->QueryInterface(&m_pCommand);

  Log("MPMadPresenter::Init 3()");

  if (!m_pBaseFilter || !m_pOsdServices || !m_pManager || !m_pSubclassReplacement || !m_pSubRender || !m_pCommand || !m_pWindow)
    return nullptr;
  Log("MPMadPresenter::Init 4()");

  m_pManager->ConfigureDisplayModeChanger(true, true);
  Log("MPMadPresenter::Init 5()");

  m_pSubRender->SetCallback(m_subProxy);
  Log("MPMadPresenter::Init 6()");

  m_pCommand->SendCommandBool("disableSeekbar", true);
  Log("MPMadPresenter::Init 7()");

  m_pWindow->put_Owner(m_hParent);
  Log("MPMadPresenter::Init 8()");

  m_pWindow->SetWindowForeground(true);
  Log("MPMadPresenter::Init 9()");

  m_pWindow->put_MessageDrain(m_hParent);
  Log("MPMadPresenter::Init 10()");

  // TODO implement IMadVRSubclassReplacement
  //pSubclassReplacement->DisableSubclassing();

  return m_pBaseFilter;
}

HRESULT MPMadPresenter::Shutdown()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    Log("MPMadPresenter::Shutdown() - instance 0x%x");

    CAutoLock lock(this);

    if (m_pCallback)
    {
      m_pCallback->Release();
      m_pCallback = nullptr;
    }
  } // Scope for autolock

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

    if (m_pOsdServices)
    {
      Log("MPMadPresenter::ReleaseOSD() 1");
      m_pOsdServices->OsdSetRenderCallback("MP-GUI", nullptr, nullptr);
      Log("MPMadPresenter::ReleaseOSD() 2");
    }
  }

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

  if (!m_pCallback)
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

  if (!m_pCallback)
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

    vertices[0].x = static_cast<float>(rDest.left) - 0.5f;
    vertices[0].y = static_cast<float>(rDest.top) - 0.5f;
    vertices[0].z = 0.0f;
    vertices[0].rhw = 1.0f;
    vertices[0].u = 0.0f;
    vertices[0].v = 0.0f;

    vertices[1].x = static_cast<float>(rDest.right) - 0.5f;
    vertices[1].y = static_cast<float>(rDest.top) - 0.5f;
    vertices[1].z = 0.0f;
    vertices[1].rhw = 1.0f;
    vertices[1].u = 1.0f;
    vertices[1].v = 0.0f;

    vertices[2].x = static_cast<float>(rDest.right) - 0.5f;
    vertices[2].y = static_cast<float>(rDest.bottom) - 0.5f;
    vertices[2].z = 0.0f;
    vertices[2].rhw = 1.0f;
    vertices[2].u = 1.0f;
    vertices[2].v = 1.0f;

    vertices[3].x = static_cast<float>(rDest.left) - 0.5f;
    vertices[3].y = static_cast<float>(rDest.bottom) - 0.5f;
    vertices[3].z = 0.0f;
    vertices[3].rhw = 1.0f;
    vertices[3].u = 0.0f;
    vertices[3].v = 1.0f;

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

  CAutoLock cAutoLock(this);

  if (!pD3DDev)
    return S_OK;

  if (!m_pCallback)
    return S_OK;

  Log("MPMadPresenter::SetDevice() pD3DDev 0x:%x", pD3DDev);

  m_pMadD3DDev = static_cast<IDirect3DDevice9Ex*>(pD3DDev);
  m_deviceState.SetDevice(pD3DDev);

  //if (m_pCallback && pD3DDev)
  //  m_pCallback->SetSubtitleDevice((DWORD)pD3DDev);

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
