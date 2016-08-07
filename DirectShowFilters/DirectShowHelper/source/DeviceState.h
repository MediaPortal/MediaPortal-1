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

#pragma once

#include "stdafx.h"
#include <streams.h>
#include <d3dx9.h>

class DeviceState : public CCritSec
{
  public:
    DeviceState();
    virtual ~DeviceState();

    void SetDevice(IDirect3DDevice9* pDevice);

    HRESULT Store();
    HRESULT Restore();
    void Store_Surface(IDirect3DSurface9* pSurface);
    void Shutdown();

  private:
    IDirect3DVertexShader9* m_pVS = nullptr;
    IDirect3DVertexBuffer9* m_pStreamData = nullptr;
    IDirect3DBaseTexture9* m_pTexture = nullptr;
    IDirect3DSurface9* m_pSurface = nullptr;

    DWORD m_dwFVF = 0;
    UINT  m_OffsetInBytes = 0;
    UINT  m_Stride = 0;
    RECT  m_ScissorRect;

    DWORD m_D3DRS_CULLMODE = 0;
    DWORD m_D3DRS_LIGHTING = 0;
    DWORD m_D3DRS_ZENABLE = 0;
    DWORD m_D3DRS_ALPHABLENDENABLE = 0;
    DWORD m_D3DRS_SRCBLEND = 0;
    DWORD m_D3DRS_DESTBLEND = 0;

    IDirect3DPixelShader9* m_pPix = nullptr;

    IDirect3DDevice9* m_pD3DDev = nullptr;
};

