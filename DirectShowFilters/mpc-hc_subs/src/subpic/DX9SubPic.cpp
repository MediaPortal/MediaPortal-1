/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2014 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#include "stdafx.h"
#include "DX9SubPic.h"
#include <vmr9.h>
#include <algorithm>

//
// CDX9SubPic
//

CDX9SubPic::CDX9SubPic(IDirect3DSurface9* pSurface, CDX9SubPicAllocator* pAllocator, bool bExternalRenderer)
    : m_pSurface(pSurface)
    , m_pAllocator(pAllocator)
    , m_bExternalRenderer(bExternalRenderer)
{
    D3DSURFACE_DESC desc;
    ZeroMemory(&desc, sizeof(desc));
    if (SUCCEEDED(m_pSurface->GetDesc(&desc))) {
        m_maxsize.SetSize(desc.Width, desc.Height);
        m_rcDirty.SetRect(0, 0, desc.Width, desc.Height);
    }
}

CDX9SubPic::~CDX9SubPic()
{
    CAutoLock lock(&CDX9SubPicAllocator::ms_surfaceQueueLock);
    // Add surface to cache
    if (m_pAllocator) {
        for (POSITION pos = m_pAllocator->m_allocatedSurfaces.GetHeadPosition(); pos;) {
            POSITION thisPos = pos;
            CDX9SubPic* pSubPic = m_pAllocator->m_allocatedSurfaces.GetNext(pos);
            if (pSubPic == this) {
                m_pAllocator->m_allocatedSurfaces.RemoveAt(thisPos);
                break;
            }
        }
        m_pAllocator->m_freeSurfaces.AddTail(m_pSurface);
    }
}


// ISubPic

STDMETHODIMP_(void*) CDX9SubPic::GetObject()
{
    CComPtr<IDirect3DTexture9> pTexture;
    if (SUCCEEDED(m_pSurface->GetContainer(IID_PPV_ARGS(&pTexture)))) {
        return (IDirect3DTexture9*)pTexture;
    }

    return nullptr;
}

STDMETHODIMP CDX9SubPic::GetDesc(SubPicDesc& spd)
{
    D3DSURFACE_DESC desc;
    ZeroMemory(&desc, sizeof(desc));
    if (FAILED(m_pSurface->GetDesc(&desc))) {
        return E_FAIL;
    }

    spd.type = 0;
    spd.w = m_size.cx;
    spd.h = m_size.cy;
    spd.bpp =
        desc.Format == D3DFMT_A8R8G8B8 ? 32 :
        desc.Format == D3DFMT_A4R4G4B4 ? 16 : 0;
    spd.pitch = 0;
    spd.bits = nullptr;
    spd.vidrect = m_vidrect;

    return S_OK;
}

STDMETHODIMP CDX9SubPic::CopyTo(ISubPic* pSubPic)
{
    HRESULT hr;
    if (FAILED(hr = __super::CopyTo(pSubPic))) {
        return hr;
    }

    if (m_rcDirty.IsRectEmpty()) {
        return S_FALSE;
    }

    CComPtr<IDirect3DDevice9> pD3DDev;
    if (!m_pSurface || FAILED(m_pSurface->GetDevice(&pD3DDev)) || !pD3DDev) {
        return E_FAIL;
    }

    IDirect3DTexture9* pSrcTex = (IDirect3DTexture9*)GetObject();
    CComPtr<IDirect3DSurface9> pSrcSurf;
    pSrcTex->GetSurfaceLevel(0, &pSrcSurf);
    if (!pSrcSurf) {
        return E_FAIL;
    }
    D3DSURFACE_DESC srcDesc;
    pSrcSurf->GetDesc(&srcDesc);

    IDirect3DTexture9* pDstTex = (IDirect3DTexture9*)pSubPic->GetObject();
    CComPtr<IDirect3DSurface9> pDstSurf;
    pDstTex->GetSurfaceLevel(0, &pDstSurf);
    D3DSURFACE_DESC dstDesc;
    pDstSurf->GetDesc(&dstDesc);

    RECT r;
    SetRect(&r, 0, 0, std::min(srcDesc.Width, dstDesc.Width), std::min(srcDesc.Height, dstDesc.Height));
    POINT p = { 0, 0 };
    hr = pD3DDev->UpdateSurface(pSrcSurf, &r, pDstSurf, &p);

    return SUCCEEDED(hr) ? S_OK : E_FAIL;
}

STDMETHODIMP CDX9SubPic::ClearDirtyRect()
{
    if (m_rcDirty.IsRectEmpty()) {
        return S_FALSE;
    }

    CComPtr<IDirect3DDevice9> pD3DDev;
    if (!m_pSurface || FAILED(m_pSurface->GetDevice(&pD3DDev)) || !pD3DDev) {
        return E_FAIL;
    }

    m_rcDirty.IntersectRect(m_rcDirty, CRect(0, 0, m_maxsize.cx, m_maxsize.cy));

    SubPicDesc spd;
    if (SUCCEEDED(Lock(spd))) {
        int h = m_rcDirty.Height();
        BYTE* ptr = spd.bits + spd.pitch * m_rcDirty.top + (m_rcDirty.left * spd.bpp >> 3);

        if (spd.bpp == 16) {
            const unsigned short color = m_bInvAlpha ? 0x00000000 : 0xFF000000;
            const int w2 = m_rcDirty.Width() * 2;
            while (h-- > 0) {
                memsetw(ptr, color, w2);
                ptr += spd.pitch;
            }
        } else if (spd.bpp == 32) {
            const DWORD color = m_bInvAlpha ? 0x00000000 : 0xFF000000;
            const int w4 = m_rcDirty.Width() * 4;
            while (h-- > 0) {
                memsetd(ptr, color, w4);
                ptr += spd.pitch;
            }
        }
        Unlock(nullptr);
    }

    m_rcDirty.SetRectEmpty();

    return S_OK;
}

STDMETHODIMP CDX9SubPic::Lock(SubPicDesc& spd)
{
    D3DSURFACE_DESC desc;
    ZeroMemory(&desc, sizeof(desc));
    if (FAILED(m_pSurface->GetDesc(&desc))) {
        return E_FAIL;
    }

    D3DLOCKED_RECT LockedRect;
    ZeroMemory(&LockedRect, sizeof(LockedRect));
    if (FAILED(m_pSurface->LockRect(&LockedRect, nullptr, D3DLOCK_NO_DIRTY_UPDATE | D3DLOCK_NOSYSLOCK))) {
        return E_FAIL;
    }

    spd.type = 0;
    spd.w = m_size.cx;
    spd.h = m_size.cy;
    spd.bpp =
        desc.Format == D3DFMT_A8R8G8B8 ? 32 :
        desc.Format == D3DFMT_A4R4G4B4 ? 16 : 0;
    spd.pitch = LockedRect.Pitch;
    spd.bits = (BYTE*)LockedRect.pBits;
    spd.vidrect = m_vidrect;

    return S_OK;
}

STDMETHODIMP CDX9SubPic::Unlock(RECT* pDirtyRect)
{
    m_pSurface->UnlockRect();

    if (pDirtyRect) {
        m_rcDirty = pDirtyRect;
        if (!m_rcDirty.IsRectEmpty()) {
            m_rcDirty.InflateRect(1, 1);
            m_rcDirty.IntersectRect(m_rcDirty, CRect(0, 0, m_size.cx, m_size.cy));

            CComPtr<IDirect3DTexture9> pTexture = (IDirect3DTexture9*)GetObject();
            if (pTexture) {
                pTexture->AddDirtyRect(&m_rcDirty);
            }
        }
    } else {
        m_rcDirty = CRect(CPoint(0, 0), m_size);
    }

    return S_OK;
}

STDMETHODIMP CDX9SubPic::AlphaBlt(RECT* pSrc, RECT* pDst, SubPicDesc* pTarget)
{
    ASSERT(pTarget == nullptr);

    if (!pSrc || !pDst) {
        return E_POINTER;
    }

    CRect src(*pSrc), dst(*pDst);

    CComPtr<IDirect3DDevice9> pD3DDev;
    CComPtr<IDirect3DTexture9> pTexture = (IDirect3DTexture9*)GetObject();
    if (!pTexture || FAILED(pTexture->GetDevice(&pD3DDev)) || !pD3DDev) {
        return E_NOINTERFACE;
    }

    D3DSURFACE_DESC desc;
    ZeroMemory(&desc, sizeof(desc));
    if (FAILED(pTexture->GetLevelDesc(0, &desc)) /*|| desc.Type != D3DRTYPE_TEXTURE*/) {
        return E_FAIL;
    }

    float w = (float)desc.Width;
    float h = (float)desc.Height;

    // Be careful with the code that follows. Some compilers (e.g. Visual Studio 2012) used to miscompile
    // it in some cases (namely x64 with optimizations /O2 /Ot). This bug led pVertices not to be correctly
    // initialized and thus the subtitles weren't shown.
    struct {
        float x, y, z, rhw;
        float tu, tv;
    } pVertices[] = {
        {float(dst.left),  float(dst.top),    0.5f, 2.0f, float(src.left)  / w, float(src.top) / h},
        {float(dst.right), float(dst.top),    0.5f, 2.0f, float(src.right) / w, float(src.top) / h},
        {float(dst.left),  float(dst.bottom), 0.5f, 2.0f, float(src.left)  / w, float(src.bottom) / h},
        {float(dst.right), float(dst.bottom), 0.5f, 2.0f, float(src.right) / w, float(src.bottom) / h},
    };

    for (size_t i = 0; i < _countof(pVertices); i++) {
        pVertices[i].x -= 0.5f;
        pVertices[i].y -= 0.5f;
    }

    pD3DDev->SetTexture(0, pTexture);

    // GetRenderState fails for devices created with D3DCREATE_PUREDEVICE
    // so we need to provide default values in case GetRenderState fails
    DWORD abe, sb, db;
    if (FAILED(pD3DDev->GetRenderState(D3DRS_ALPHABLENDENABLE, &abe))) {
        abe = FALSE;
    }
    if (FAILED(pD3DDev->GetRenderState(D3DRS_SRCBLEND, &sb))) {
        sb = D3DBLEND_ONE;
    }
    if (FAILED(pD3DDev->GetRenderState(D3DRS_DESTBLEND, &db))) {
        db = D3DBLEND_ZERO;
    }

    pD3DDev->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
    pD3DDev->SetRenderState(D3DRS_LIGHTING, FALSE);
    pD3DDev->SetRenderState(D3DRS_ZENABLE, FALSE);
    pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
    pD3DDev->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_ONE); // pre-multiplied src and ...
    pD3DDev->SetRenderState(D3DRS_DESTBLEND, m_bInvAlpha ? D3DBLEND_INVSRCALPHA : D3DBLEND_SRCALPHA); // ... inverse alpha channel for dst

    pD3DDev->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_SELECTARG1);
    pD3DDev->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    pD3DDev->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);

    if (src == dst) {
        pD3DDev->SetSamplerState(0, D3DSAMP_MAGFILTER, D3DTEXF_POINT);
        pD3DDev->SetSamplerState(0, D3DSAMP_MINFILTER, D3DTEXF_POINT);
    } else {
        pD3DDev->SetSamplerState(0, D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);
        pD3DDev->SetSamplerState(0, D3DSAMP_MINFILTER, D3DTEXF_LINEAR);
    }
    pD3DDev->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_NONE);

    pD3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, D3DTADDRESS_BORDER);
    pD3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, D3DTADDRESS_BORDER);
    pD3DDev->SetSamplerState(0, D3DSAMP_BORDERCOLOR, m_bInvAlpha ? 0x00000000 : 0xFF000000);

    /*//
    D3DCAPS9 d3dcaps9;
    pD3DDev->GetDeviceCaps(&d3dcaps9);
    if (d3dcaps9.AlphaCmpCaps & D3DPCMPCAPS_LESS) {
        pD3DDev->SetRenderState(D3DRS_ALPHAREF, (DWORD)0x000000FE);
        pD3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, TRUE);
        pD3DDev->SetRenderState(D3DRS_ALPHAFUNC, D3DPCMPCAPS_LESS);
    }
    *///

    pD3DDev->SetPixelShader(nullptr);

    if (m_bExternalRenderer && FAILED(pD3DDev->BeginScene())) {
        return E_FAIL;
    }

    pD3DDev->SetFVF(D3DFVF_XYZRHW | D3DFVF_TEX1);
    pD3DDev->DrawPrimitiveUP(D3DPT_TRIANGLESTRIP, 2, pVertices, sizeof(pVertices[0]));

    if (m_bExternalRenderer) {
        pD3DDev->EndScene();
    }

    pD3DDev->SetTexture(0, nullptr);

    pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, abe);
    pD3DDev->SetRenderState(D3DRS_SRCBLEND, sb);
    pD3DDev->SetRenderState(D3DRS_DESTBLEND, db);

    return S_OK;
}

//
// CDX9SubPicAllocator
//

CDX9SubPicAllocator::CDX9SubPicAllocator(IDirect3DDevice9* pD3DDev, SIZE maxsize, bool bExternalRenderer)
    : CSubPicAllocatorImpl(maxsize, true)
    , m_pD3DDev(pD3DDev)
    , m_maxsize(maxsize)
    , m_bExternalRenderer(bExternalRenderer)
{
}

CCritSec CDX9SubPicAllocator::ms_surfaceQueueLock;

CDX9SubPicAllocator::~CDX9SubPicAllocator()
{
    ClearCache();
}

void CDX9SubPicAllocator::GetStats(int& nFree, int& nAlloc) const
{
    CAutoLock autoLock(&ms_surfaceQueueLock);
    nFree = (int)m_freeSurfaces.GetCount();
    nAlloc = (int)m_allocatedSurfaces.GetCount();
}

void CDX9SubPicAllocator::ClearCache()
{
    TRACE(_T("CDX9SubPicAllocator::ClearCache\n"));
    // Clear the allocator of any remaining subpics
    CAutoLock autoLock(&ms_surfaceQueueLock);
    for (POSITION pos = m_allocatedSurfaces.GetHeadPosition(); pos;) {
        CDX9SubPic* pSubPic = m_allocatedSurfaces.GetNext(pos);
        pSubPic->m_pAllocator = nullptr;
    }
    m_allocatedSurfaces.RemoveAll();
    m_freeSurfaces.RemoveAll();
}

// ISubPicAllocator

STDMETHODIMP CDX9SubPicAllocator::ChangeDevice(IUnknown* pDev)
{
    CComQIPtr<IDirect3DDevice9> pD3DDev = pDev;
    CheckPointer(pD3DDev, E_NOINTERFACE);

    CAutoLock cAutoLock(this);
    HRESULT hr = S_FALSE;
    if (m_pD3DDev != pD3DDev) {
        ClearCache();
        m_pD3DDev = pD3DDev;
        hr = __super::ChangeDevice(pDev);
    }

    return hr;
}

STDMETHODIMP CDX9SubPicAllocator::SetMaxTextureSize(SIZE maxTextureSize)
{
    CAutoLock cAutoLock(this);
    if (maxTextureSize.cx > 0 && maxTextureSize.cy > 0 && m_maxsize != maxTextureSize) {
        ClearCache();
        m_maxsize = maxTextureSize;
        TRACE(_T("CDX9SubPicAllocator::SetMaxTextureSize %dx%d\n"), m_maxsize.cx, m_maxsize.cy);
    }

    return S_OK;
}

// ISubPicAllocatorImpl

bool CDX9SubPicAllocator::Alloc(bool fStatic, ISubPic** ppSubPic)
{
    if (!ppSubPic) {
        return false;
    }

    if (m_maxsize.cx <= 0 || m_maxsize.cy <= 0) {
        TRACE(_T("CDX9SubPicAllocator::Alloc -> maxsize is zero\n"));
        return false;
    }

    CAutoLock cAutoLock(this);

    *ppSubPic = nullptr;

    CComPtr<IDirect3DSurface9> pSurface;

    if (!fStatic) {
        CAutoLock cAutoLock2(&ms_surfaceQueueLock);
        if (!m_freeSurfaces.IsEmpty()) {
            pSurface = m_freeSurfaces.RemoveHead();
        }
    }

    if (!pSurface) {
        CComPtr<IDirect3DTexture9> pTexture;
        HRESULT hr = m_pD3DDev->CreateTexture(m_maxsize.cx, m_maxsize.cy, 1, 0, D3DFMT_A8R8G8B8, fStatic ? D3DPOOL_SYSTEMMEM : D3DPOOL_DEFAULT, &pTexture, nullptr);
        if (FAILED(hr)) {
            TRACE(_T("CDX9SubPicAllocator::Alloc -> CreateTexture failed (%dx%d), hr=%x\n"), m_maxsize.cx, m_maxsize.cy, hr);
            return false;
        }

        hr = pTexture->GetSurfaceLevel(0, &pSurface);
        if (FAILED(hr)) {
            TRACE(_T("CDX9SubPicAllocator::Alloc -> GetSurfaceLevel failed, hr=%x\n"), hr);
            return false;
        }

        TRACE(_T("CDX9SubPicAllocator::Alloc -> Surface allocated (%dx%d)\n"), m_maxsize.cx, m_maxsize.cy);
    }

    try {
        *ppSubPic = DEBUG_NEW CDX9SubPic(pSurface, fStatic ? nullptr : this, m_bExternalRenderer);
    } catch (CMemoryException* e) {
        e->Delete();
        TRACE(_T("CDX9SubPicAllocator::Alloc -> CDX9SubPic gave memory exception\n"));
        return false;
    }

    (*ppSubPic)->AddRef();
    (*ppSubPic)->SetInverseAlpha(m_bInvAlpha);

    if (!fStatic) {
        CAutoLock cAutoLock2(&ms_surfaceQueueLock);
        m_allocatedSurfaces.AddHead((CDX9SubPic*)*ppSubPic);
    }

    return true;
}
