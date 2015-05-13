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

MPMadPresenter::MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, IDirect3DDevice9* pDevice) :
  CUnknown(NAME("MPMadPresenter"), NULL),
  m_pCallback(pCallback),
  m_dwWidth(width),
  m_dwHeight(height),
  m_pDevice((IDirect3DDevice9Ex*)pDevice)
{
}

MPMadPresenter::~MPMadPresenter()
{
  CAutoLock cAutoLock(this);
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

  if (!baseFilter || !pOsdServices || !manager || !pSubclassReplacement)
    return NULL;

  pOsdServices->OsdSetRenderCallback("MP-GUI", this);
  manager->ConfigureDisplayModeChanger(false, false);

  // TODO implement IMadVRSubclassReplacement
  //pSubclassReplacement->DisableSubclassing();

  return baseFilter;
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

  WORD height = (WORD)fullOutputRect->bottom - (WORD)fullOutputRect->top;
  WORD width = (WORD)fullOutputRect->right - (WORD)fullOutputRect->left;

  CAutoLock cAutoLock(this);

  if (FAILED(hr = RenderToTexture(m_pMPTextureGui, m_pMPSurfaceGui)))
    return hr;

  if (FAILED(hr = StoreMadDeviceState()))
    return hr;

  if (FAILED(hr = m_pCallback->RenderGui(width, height, width, height)))
    return hr;

  if (FAILED(hr = SetupMadDeviceState()))
    return hr;
  
  if (FAILED(hr = SetupOSDVertex(m_pMadGuiVertexBuffer)))
    return hr;

  // Draw MP texture on madVR device's side
  if (FAILED(hr = RenderTexture(m_pMadGuiVertexBuffer, m_pRenderTextureGui)))
    return hr;

  if (FAILED(hr = RestoreMadDeviceState()))
    return hr;

  return hr;
}

HRESULT MPMadPresenter::RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT* fullOutputRect, RECT* activeVideoRect)
{
  HRESULT hr = E_UNEXPECTED;

  CAutoLock cAutoLock(this);

  WORD height = (WORD)fullOutputRect->bottom - (WORD)fullOutputRect->top;
  WORD width = (WORD)fullOutputRect->right - (WORD)fullOutputRect->left;

  if (FAILED(hr = RenderToTexture(m_pMPTextureOsd, m_pMPSurfaceOsd)))
    return hr;

  if (FAILED(hr = StoreMadDeviceState()))
    return hr;

  if (FAILED(hr = m_pCallback->RenderOverlay(width, height, width, height)))
    return hr;

  if (FAILED(hr = SetupMadDeviceState()))
    return hr;

  if (FAILED(hr = SetupOSDVertex(m_pMadOsdVertexBuffer)))
    return hr;

  // Draw MP texture on madVR device's side
  if (FAILED(hr = RenderTexture(m_pMadOsdVertexBuffer, m_pRenderTextureOsd)))
    return hr;

  if (FAILED(hr = RestoreMadDeviceState()))
    return hr;

  return hr;
}

HRESULT MPMadPresenter::RenderToTexture(IDirect3DTexture9* pTexture, IDirect3DSurface9* pSurface)
{
  HRESULT hr = E_UNEXPECTED;

  if (FAILED(hr = pTexture->GetSurfaceLevel(0, &pSurface)))
    return hr;

  if (FAILED(m_pCallback->SetRenderTarget((DWORD)pSurface)))
    return hr;

  if (FAILED(m_pDevice->Clear(0, NULL, D3DCLEAR_TARGET, D3DXCOLOR(0, 0, 0, 0), 1.0f, 0)))
    return hr;

  return hr;
}

HRESULT MPMadPresenter::RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture)
{
  HRESULT hr = m_pMadD3DDev->SetStreamSource(0, pVertexBuf, 0, sizeof(VID_FRAME_VERTEX));
  if (FAILED(hr))
    return hr;

  hr = m_pMadD3DDev->SetTexture(0, pTexture);
  if (FAILED(hr))
    return hr;

  hr = m_pMadD3DDev->DrawPrimitive(D3DPT_TRIANGLEFAN, 0, 2);
  if (FAILED(hr))
    return hr;

  return S_OK;
}

HRESULT MPMadPresenter::SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf)
{
  VID_FRAME_VERTEX* vertices = nullptr;

  // Lock the vertex buffer
  HRESULT hr = pVertextBuf->Lock(0, 0, (void**)&vertices, NULL);

  if (SUCCEEDED(hr))
  {
    RECT rDest;
    rDest.bottom = 1080;
    rDest.left = 0;
    rDest.right = 1920;
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

HRESULT MPMadPresenter::StoreMadDeviceState()
{
  HRESULT hr = E_UNEXPECTED;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_ALPHABLENDENABLE, &m_dwOldALPHABLENDENABLE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_SRCBLEND, &m_dwOldSRCALPHA)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_DESTBLEND, &m_dwOldINVSRCALPHA)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetScissorRect(&m_oldScissorRect)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetVertexShader(&m_pOldVS)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetFVF(&m_dwOldFVF)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetTexture(0, &m_pOldTexture)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetStreamSource(0, &m_pOldStreamData, &m_nOldOffsetInBytes, &m_nOldStride)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_CULLMODE, &mD3DRS_CULLMODE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_LIGHTING, &mD3DRS_LIGHTING)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_ZENABLE, &mD3DRS_ZENABLE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_ALPHABLENDENABLE, &mD3DRS_ALPHABLENDENABLE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_SRCBLEND, &mD3DRS_SRCBLEND)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetRenderState(D3DRS_DESTBLEND, &mD3DRS_DESTBLEND)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->GetPixelShader(&mPix)))
    return hr;

  return hr;
}

HRESULT MPMadPresenter::SetupMadDeviceState()
{
  HRESULT hr = E_UNEXPECTED;

  RECT newScissorRect;
  newScissorRect.bottom = 1080;
  newScissorRect.top = 0;
  newScissorRect.left = 0;
  newScissorRect.right = 1920;

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

HRESULT MPMadPresenter::RestoreMadDeviceState()
{
  HRESULT hr = S_FALSE;

  if (FAILED(hr = m_pMadD3DDev->SetScissorRect(&m_oldScissorRect)))
    return hr;

  hr = m_pMadD3DDev->SetTexture(0, m_pOldTexture);

  if (m_pOldTexture)
    m_pOldTexture->Release();

  if (FAILED(hr))
    return hr;

  hr = m_pMadD3DDev->SetVertexShader(m_pOldVS);

  if (m_pOldVS)
    m_pOldVS->Release();

  if (FAILED(hr))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetFVF(m_dwOldFVF)))
    return hr;

  hr = m_pMadD3DDev->SetStreamSource(0, m_pOldStreamData, m_nOldOffsetInBytes, m_nOldStride);

  if (m_pOldStreamData)
    m_pOldStreamData->Release();

  if (FAILED(hr))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, m_dwOldALPHABLENDENABLE)))
    return hr;
  
  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_SRCBLEND, m_dwOldSRCALPHA)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_CULLMODE, mD3DRS_CULLMODE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_LIGHTING, mD3DRS_LIGHTING)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ZENABLE, mD3DRS_ZENABLE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, mD3DRS_ALPHABLENDENABLE)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_SRCBLEND, mD3DRS_SRCBLEND)))
    return hr;

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_DESTBLEND, mD3DRS_DESTBLEND)))
    return hr;

  return hr;
}

HRESULT MPMadPresenter::SetDevice(IDirect3DDevice9* pD3DDev)
{
  HRESULT hr = S_FALSE;

  CAutoLock cAutoLock(this);
  m_pMadD3DDev = (IDirect3DDevice9Ex*)pD3DDev;

  if (m_pMadD3DDev)
  {
    m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadGuiVertexBuffer, NULL);

    if (FAILED(hr = m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadOsdVertexBuffer, NULL)))
      return hr;

    if (FAILED(hr = m_pDevice->CreateTexture(m_dwWidth, m_dwHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureGui, &m_hSharedGuiHandle)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->CreateTexture(m_dwWidth, m_dwHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureGui, &m_hSharedGuiHandle)))
      return hr;

    if (FAILED(hr = m_pDevice->CreateTexture(m_dwWidth, m_dwHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureOsd, &m_hSharedOsdHandle)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->CreateTexture(m_dwWidth, m_dwHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureOsd, &m_hSharedOsdHandle)))
      return hr;
  }
  else
  {
    m_pDevice = nullptr;
    m_pMadD3DDev = nullptr;
  }

  return hr;
}
