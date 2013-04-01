/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2012 see Authors.txt
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

#pragma once

#include "SubPicImpl.h"
#include "SubPicAllocatorPresenterImpl.h"
#include <d3d9.h>

// CDX9SubPic

class CVirtualLock
{
public:
    virtual void Lock() = 0;
    virtual void Unlock() = 0;
};

typedef void (FLock)(void* _pLock);

class CScopeLock
{
    void* m_pLock;
    FLock* m_pUnlockFunc;
public:
    CScopeLock(): m_pLock(NULL), m_pUnlockFunc(NULL) {};

    template <typename t_Lock>
    class TCLocker
    {
    public:
        static void fs_Locker(void* _pLock) {
            ((t_Lock*)_pLock)->Unlock();
        }
    };

    template <typename t_Lock>
    CScopeLock(t_Lock& _Lock) {
        _Lock.Lock();
        m_pLock = &_Lock;
        m_pUnlockFunc = TCLocker<t_Lock>::fs_Locker;
    }

    ~CScopeLock() {
        m_pUnlockFunc(m_pLock);
    }
};


class CDX9SubPicAllocator;
class CDX9SubPic : public CSubPicImpl
{
    CComPtr<IDirect3DSurface9> m_pSurface;

protected:
    STDMETHODIMP_(void*) GetObject(); // returns IDirect3DTexture9*

public:
    CDX9SubPicAllocator* m_pAllocator;
    bool m_bExternalRenderer;
    CDX9SubPic(IDirect3DSurface9* pSurface, CDX9SubPicAllocator* pAllocator, bool bExternalRenderer);
    ~CDX9SubPic();

    // ISubPic
    STDMETHODIMP GetDesc(SubPicDesc& spd);
    STDMETHODIMP CopyTo(ISubPic* pSubPic);
    STDMETHODIMP ClearDirtyRect(DWORD color);
    STDMETHODIMP Lock(SubPicDesc& spd);
    STDMETHODIMP Unlock(RECT* pDirtyRect);
    STDMETHODIMP AlphaBlt(RECT* pSrc, RECT* pDst, SubPicDesc* pTarget);
};

// CDX9SubPicAllocator

class CDX9SubPicAllocator : public CSubPicAllocatorImpl, public CCritSec
{
    CComPtr<IDirect3DDevice9> m_pD3DDev;
    CSize m_maxsize;
    bool m_bExternalRenderer;

    bool Alloc(bool fStatic, ISubPic** ppSubPic);

public:
    static CCritSec ms_SurfaceQueueLock;
    CAtlList<CComPtr<IDirect3DSurface9> > m_FreeSurfaces;
    CAtlList<CDX9SubPic*> m_AllocatedSurfaces;

    void GetStats(int& _nFree, int& _nAlloc);

    CDX9SubPicAllocator(IDirect3DDevice9* pD3DDev, SIZE maxsize, bool fPow2Textures, bool bExternalRenderer);
    ~CDX9SubPicAllocator();
    void ClearCache();

    // ISubPicAllocator
    STDMETHODIMP ChangeDevice(IUnknown* pDev);
    STDMETHODIMP SetMaxTextureSize(SIZE MaxTextureSize);
};
