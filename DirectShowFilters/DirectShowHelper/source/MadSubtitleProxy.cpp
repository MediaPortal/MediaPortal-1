// Copyright (C) 2005-2015 Team MediaPortal
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

#include "MadSubtitleProxy.h"
#include "dshowhelper.h"
#include "madpresenter.h"

MadSubtitleProxy::MadSubtitleProxy(IVMR9Callback* pCallback, MPMadPresenter* pPresenter) :
  CUnknown(NAME("MadSubtitleProxy"), NULL),
  m_pCallback(pCallback),
  m_pPresenter(pPresenter)
{
  Log("MadSubtitleProxy::Constructor() - instance 0x%x", this);
  CAutoLock cAutoLock(this);
  m_pPresenter->m_subProxy = this;
  m_pPresenter->m_subProxy->AddRef();
}

MadSubtitleProxy::~MadSubtitleProxy()
{
  CAutoLock cAutoLock(this);
  Log("MadSubtitleProxy::Destructor() - instance 0x%x", this);
}

void MadSubtitleProxy::Shutdown()
{
  {
    Log("MadSubtitleProxy::Shutdown()");
    CAutoLock Lock(this);

    if (m_pCallback)
    {
      m_pCallback->Release();
      m_pCallback = nullptr;
    }
  }
}

HRESULT MadSubtitleProxy::SetDevice(IDirect3DDevice9* device)
{
  if (m_pCallback)
  {
    CAutoLock cAutoLock(this);

    Log("MadSubtitleProxy::SetDevice() device 0x:%x", device);

    if (!m_pPresenter->m_pShutdown)
    {
      m_pMadD3DDev = device;

      if (!m_pMadD3DDev)
      {
        m_pInitOSDRender = false;
        return S_FALSE;
      }

      if (m_pMadD3DDev)
      {
        m_deviceState.SetDevice(device);
        m_pMadD3DDev = device;
      }
    }
  }
  return S_OK;
}

HRESULT MadSubtitleProxy::Render(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height)
{
  if (m_pCallback)
  {
    CAutoLock cAutoLock(this);

    if (!m_pInitOSDRender)
    {
      m_pInitOSDRender = true;
      m_pPresenter->m_pCallback->ForceOsdUpdate(true);
      Log("MadSubtitleProxy::Render() ForceOsdUpdate");
    }
    m_deviceState.Store();
    SetupMadDeviceState();

    m_pCallback->RenderSubtitle(frameStart, left, top, right, bottom, width, height);

    m_deviceState.Restore();
  }

  return S_OK;
}

HRESULT MadSubtitleProxy::SetupMadDeviceState()
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

  if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_SRCALPHA)))
    return hr;

  return hr;
}