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

#include "DeviceState.h"

DeviceState::DeviceState()
{
}

DeviceState::~DeviceState()
{
}

void DeviceState::SetDevice(IDirect3DDevice9* pDevice)
{
  CAutoLock cAutoLock(this);
  m_pD3DDev = pDevice;
}

void DeviceState::Store_Surface(IDirect3DSurface9* pSurface)
{
  m_pSurface = pSurface;
}

HRESULT DeviceState::Store()
{
  HRESULT hr = E_UNEXPECTED;

  CAutoLock cAutoLock(this);

  if (!m_pD3DDev)
    return hr;

  if (FAILED(hr = m_pD3DDev->GetScissorRect(&m_ScissorRect)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetVertexShader(&m_pVS)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetFVF(&m_dwFVF)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetTexture(0, &m_pTexture)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetStreamSource(0, &m_pStreamData, &m_OffsetInBytes, &m_Stride)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetRenderState(D3DRS_CULLMODE, &m_D3DRS_CULLMODE)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetRenderState(D3DRS_LIGHTING, &m_D3DRS_LIGHTING)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetRenderState(D3DRS_ZENABLE, &m_D3DRS_ZENABLE)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetRenderState(D3DRS_ALPHABLENDENABLE, &m_D3DRS_ALPHABLENDENABLE)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetRenderState(D3DRS_SRCBLEND, &m_D3DRS_SRCBLEND)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetRenderState(D3DRS_DESTBLEND, &m_D3DRS_DESTBLEND)))
    return hr;

  if (FAILED(hr = m_pD3DDev->GetPixelShader(&m_pPix)))
    return hr;

  return hr;
}

HRESULT DeviceState::Restore()
{
  HRESULT hr = S_FALSE;

  CAutoLock cAutoLock(this);

  if (!m_pD3DDev)
    return hr;

  if (FAILED(hr = m_pD3DDev->SetScissorRect(&m_ScissorRect)))
    return hr;

  hr = m_pD3DDev->SetTexture(0, m_pTexture);

  if (m_pTexture)
    m_pTexture->Release();

  if (FAILED(hr))
    return hr;

  hr = m_pD3DDev->SetVertexShader(m_pVS);

  if (m_pVS)
    m_pVS->Release();

  if (FAILED(hr))
    return hr;

  hr = m_pD3DDev->SetPixelShader(m_pPix);

  if (m_pPix)
    m_pPix->Release();

  if (FAILED(hr))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetFVF(m_dwFVF)))
    return hr;

  hr = m_pD3DDev->SetStreamSource(0, m_pStreamData, m_OffsetInBytes, m_Stride);

  if (m_pStreamData)
    m_pStreamData->Release();

  if (m_pSurface)
    m_pSurface->Release();

  if (FAILED(hr))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetRenderState(D3DRS_CULLMODE, m_D3DRS_CULLMODE)))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetRenderState(D3DRS_LIGHTING, m_D3DRS_LIGHTING)))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetRenderState(D3DRS_ZENABLE, m_D3DRS_ZENABLE)))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, m_D3DRS_ALPHABLENDENABLE)))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetRenderState(D3DRS_SRCBLEND, m_D3DRS_SRCBLEND)))
    return hr;

  if (FAILED(hr = m_pD3DDev->SetRenderState(D3DRS_DESTBLEND, m_D3DRS_DESTBLEND)))
    return hr;

  return hr;
}