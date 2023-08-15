/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2016 see Authors.txt
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
#include "SubPicImpl.h"
#include "../DSUtil/DSUtil.h"

//
// CSubPicImpl
//

CSubPicImpl::CSubPicImpl()
    : CUnknown(NAME("CSubPicImpl"), nullptr)
    , m_rtStart(0)
    , m_rtStop(0)
    , m_rtSegmentStart(0)
    , m_rtSegmentStop(0)
    , m_rcDirty(0, 0, 0, 0)
    , m_maxsize(0, 0)
    , m_size(0, 0)
    , m_vidrect(0, 0, 0, 0)
    , m_virtualTextureSize(0, 0)
    , m_virtualTextureTopLeft(0, 0)
    , m_bInvAlpha(false)
    , m_relativeTo(WINDOW)
{
}

STDMETHODIMP CSubPicImpl::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
    return
        QI(ISubPic)
        __super::NonDelegatingQueryInterface(riid, ppv);
}

// ISubPic

STDMETHODIMP_(REFERENCE_TIME) CSubPicImpl::GetStart() const
{
    return m_rtStart;
}

STDMETHODIMP_(REFERENCE_TIME) CSubPicImpl::GetStop() const
{
    return m_rtStop;
}

STDMETHODIMP_(REFERENCE_TIME) CSubPicImpl::GetSegmentStart() const
{
    return m_rtSegmentStart >= 0 ? m_rtSegmentStart : m_rtStart;
}

STDMETHODIMP_(REFERENCE_TIME) CSubPicImpl::GetSegmentStop() const
{
    return m_rtSegmentStop >= 0 ? m_rtSegmentStop : m_rtStop;
}

STDMETHODIMP_(void) CSubPicImpl::SetSegmentStart(REFERENCE_TIME rtStart)
{
    m_rtSegmentStart = rtStart;
}

STDMETHODIMP_(void) CSubPicImpl::SetSegmentStop(REFERENCE_TIME rtStop)
{
    m_rtSegmentStop = rtStop;
}

STDMETHODIMP_(void) CSubPicImpl::SetStart(REFERENCE_TIME rtStart)
{
    m_rtStart = rtStart;
}

STDMETHODIMP_(void) CSubPicImpl::SetStop(REFERENCE_TIME rtStop)
{
    m_rtStop = rtStop;
}

STDMETHODIMP CSubPicImpl::CopyTo(ISubPic* pSubPic)
{
    CheckPointer(pSubPic, E_POINTER);

    pSubPic->SetStart(m_rtStart);
    pSubPic->SetStop(m_rtStop);
    pSubPic->SetSegmentStart(m_rtSegmentStart);
    pSubPic->SetSegmentStop(m_rtSegmentStop);
    pSubPic->SetDirtyRect(m_rcDirty);
    pSubPic->SetSize(m_size, m_vidrect);
    pSubPic->SetVirtualTextureSize(m_virtualTextureSize, m_virtualTextureTopLeft);
    pSubPic->SetInverseAlpha(m_bInvAlpha);

    return S_OK;
}

STDMETHODIMP CSubPicImpl::GetDirtyRect(RECT* pDirtyRect) const
{
    CheckPointer(pDirtyRect, E_POINTER);

    *pDirtyRect = m_rcDirty;

    return S_OK;
}

STDMETHODIMP CSubPicImpl::GetSourceAndDest(RECT rcWindow, RECT rcVideo,
                                           RECT* pRcSource, RECT* pRcDest,
                                           const double videoStretchFactor /*= 1.0*/,
                                           int xOffsetInPixels /*= 0*/, int yOffsetInPixels /*= 0*/) const
{
    CheckPointer(pRcSource, E_POINTER);
    CheckPointer(pRcDest,   E_POINTER);

    if (m_size.cx > 0 && m_size.cy > 0 && m_rcDirty.Height() > 0) {
        CRect videoRect(rcVideo);
        CRect windowRect(rcWindow);

        CRect originalDirtyRect = m_rcDirty;
        *pRcSource = originalDirtyRect;
        originalDirtyRect.OffsetRect(m_virtualTextureTopLeft);

        CRect targetDirtyRect;

        // check if scaling is needed
        if (videoRect.Size() != windowRect.Size() || videoRect.Size() != m_virtualTextureSize) {
            if (m_relativeTo == BEST_FIT && m_virtualTextureSize.cx > 720 && videoStretchFactor == 1.0) {
                CRect visibleRect;
                visibleRect.top    = videoRect.top    > windowRect.top    ? (videoRect.top    > windowRect.bottom ? windowRect.bottom : videoRect.top)    : windowRect.top;
                visibleRect.bottom = videoRect.bottom < windowRect.bottom ? (videoRect.bottom < windowRect.top    ? windowRect.top    : videoRect.bottom) : windowRect.bottom;
                visibleRect.left   = videoRect.left   > windowRect.left   ? (videoRect.left   > windowRect.right  ? windowRect.right  : videoRect.left)   : windowRect.left;
                visibleRect.right  = videoRect.right  < windowRect.right  ? (videoRect.right  < windowRect.left   ? windowRect.left   : videoRect.right)  : windowRect.right;
                if (visibleRect.Width() <= 0 || visibleRect.Height() <= 0) {
                    visibleRect = windowRect;
                    ASSERT(false);
                }
                CPoint offset(0, 0);
                double scaleFactor;
                double subtitleAR = double(m_virtualTextureSize.cx) / m_virtualTextureSize.cy;
                double visibleAR  = double(visibleRect.Width()) / visibleRect.Height();
                double vertical_stretch = 1.0;
                if (visibleAR * 2 - subtitleAR < 0.01) {
                    // some PGS can be encoded with resolution at half height
                    vertical_stretch = 2.0;
                    subtitleAR /= 2.0;
                }

                if (visibleAR == subtitleAR) {
                    // exact same AR
                    scaleFactor = double(visibleRect.Width()) / m_virtualTextureSize.cx;
                    targetDirtyRect = CRect(lround(originalDirtyRect.left * scaleFactor), lround(originalDirtyRect.top * scaleFactor * vertical_stretch), lround(originalDirtyRect.right * scaleFactor), lround(originalDirtyRect.bottom * scaleFactor * vertical_stretch));
                    targetDirtyRect.OffsetRect(visibleRect.TopLeft());
                } else if (visibleAR > subtitleAR) {
                    // video is cropped in height
                    scaleFactor = double(visibleRect.Width()) / m_virtualTextureSize.cx;
                    int extraheight = m_virtualTextureSize.cy * scaleFactor * vertical_stretch - visibleRect.Height();
                    CRect expandedRect = visibleRect;
                    expandedRect.top    -= extraheight / 2;
                    expandedRect.bottom += extraheight - extraheight / 2;
                    offset.x = expandedRect.left;
                    offset.y = expandedRect.top;

                    targetDirtyRect = CRect(lround(originalDirtyRect.left * scaleFactor), lround(originalDirtyRect.top * scaleFactor * vertical_stretch), lround(originalDirtyRect.right * scaleFactor), lround(originalDirtyRect.bottom * scaleFactor * vertical_stretch));
                    targetDirtyRect.OffsetRect(offset);

                    if (expandedRect.left >= windowRect.left && expandedRect.top >= windowRect.top && expandedRect.right <= windowRect.right && expandedRect.bottom <= windowRect.bottom) {
                        // expanded fits in window
                    } else {
                        if (targetDirtyRect.left >= windowRect.left && targetDirtyRect.top >= windowRect.top && targetDirtyRect.right <= windowRect.right && targetDirtyRect.bottom <= windowRect.bottom) {
                            // dirty rect fits in window
                        } else {
                            // does not fit yet, rescale based on available window height
                            scaleFactor = double(windowRect.Height()) / m_virtualTextureSize.cy / vertical_stretch;
                            offset.x = lround((windowRect.Width() - scaleFactor * m_virtualTextureSize.cx) / 2.0);
                            offset.y = 0;

                            targetDirtyRect = CRect(lround(originalDirtyRect.left * scaleFactor), lround(originalDirtyRect.top * scaleFactor * vertical_stretch), lround(originalDirtyRect.right * scaleFactor), lround(originalDirtyRect.bottom * scaleFactor * vertical_stretch));
                            targetDirtyRect.OffsetRect(offset);
                        }
                    }
                } else {
                    // video is cropped in width
                    scaleFactor = double(visibleRect.Height()) / m_virtualTextureSize.cy / vertical_stretch;
                    int extrawidth = m_virtualTextureSize.cx * scaleFactor - visibleRect.Width();
                    CRect expandedRect = visibleRect;
                    expandedRect.left  -= extrawidth / 2;
                    expandedRect.right += extrawidth - extrawidth / 2;
                    offset.x = expandedRect.left;
                    offset.y = expandedRect.top;

                    targetDirtyRect = CRect(lround(originalDirtyRect.left * scaleFactor), lround(originalDirtyRect.top * scaleFactor * vertical_stretch), lround(originalDirtyRect.right * scaleFactor), lround(originalDirtyRect.bottom * scaleFactor * vertical_stretch));
                    targetDirtyRect.OffsetRect(offset);

                    if (expandedRect.left >= windowRect.left && expandedRect.top >= windowRect.top && expandedRect.right <= windowRect.right && expandedRect.bottom <= windowRect.bottom) {
                        // expanded fits in window
                    } else {
                        if (targetDirtyRect.left >= windowRect.left && targetDirtyRect.top >= windowRect.top && targetDirtyRect.right <= windowRect.right && targetDirtyRect.bottom <= windowRect.bottom) {
                            // dirty rect fits in window
                        } else {
                            // does not fit yet, rescale based on available window width
                            scaleFactor = double(windowRect.Width()) / m_virtualTextureSize.cx;
                            offset.x = 0;
                            offset.y = lround((windowRect.Height() - scaleFactor * m_virtualTextureSize.cy * vertical_stretch) / 2.0);

                            targetDirtyRect = CRect(lround(originalDirtyRect.left * scaleFactor), lround(originalDirtyRect.top * scaleFactor * vertical_stretch), lround(originalDirtyRect.right * scaleFactor), lround(originalDirtyRect.bottom * scaleFactor * vertical_stretch));
                            targetDirtyRect.OffsetRect(offset);
                        }
                    }
                }
            } else {
                CRect rcTarget = (m_relativeTo == WINDOW) ? windowRect : videoRect;
                CSize szTarget = rcTarget.Size();
                double scaleX = double(szTarget.cx) / m_virtualTextureSize.cx;
                double scaleY = double(szTarget.cy) / m_virtualTextureSize.cy;

                targetDirtyRect = CRect(lround(originalDirtyRect.left * scaleX), lround(originalDirtyRect.top * scaleY), lround(originalDirtyRect.right * scaleX), lround(originalDirtyRect.bottom * scaleY));
                targetDirtyRect.OffsetRect(rcTarget.TopLeft());
            }
        } else {
            // no scaling needed
            targetDirtyRect = originalDirtyRect;
        }

        if (videoStretchFactor != 1.0) {
            ASSERT(FALSE);
            // FIXME: when is videoStretchFactor not equal to 1.0? Test that situation. Only madvr might possibly use it. Our own renderers do not.
            LONG stretch = lround(targetDirtyRect.Width() * (1.0 - 1.0 / videoStretchFactor) / 2.0);
            targetDirtyRect.left += stretch;
            targetDirtyRect.right -= stretch;
        }

        targetDirtyRect.OffsetRect(CPoint(xOffsetInPixels, yOffsetInPixels));

        *pRcDest = targetDirtyRect;
        return S_OK;
    }

    return E_INVALIDARG;
}

STDMETHODIMP CSubPicImpl::SetDirtyRect(const RECT* pDirtyRect)
{
    CheckPointer(pDirtyRect, E_POINTER);

    m_rcDirty = *pDirtyRect;

    return S_OK;
}

STDMETHODIMP CSubPicImpl::GetMaxSize(SIZE* pMaxSize) const
{
    CheckPointer(pMaxSize, E_POINTER);

    *pMaxSize = m_maxsize;

    return S_OK;
}

STDMETHODIMP CSubPicImpl::SetSize(SIZE size, RECT vidrect)
{
    m_size = size;
    m_vidrect = vidrect;

    if (m_size.cx > m_maxsize.cx) {
        m_size.cy = MulDiv(m_size.cy, m_maxsize.cx, m_size.cx);
        m_size.cx = m_maxsize.cx;
    }

    if (m_size.cy > m_maxsize.cy) {
        m_size.cx = MulDiv(m_size.cx, m_maxsize.cy, m_size.cy);
        m_size.cy = m_maxsize.cy;
    }

    if (m_size.cx != size.cx || m_size.cy != size.cy) {
        m_vidrect.top    = MulDiv(m_vidrect.top,    m_size.cx, size.cx);
        m_vidrect.bottom = MulDiv(m_vidrect.bottom, m_size.cx, size.cx);
        m_vidrect.left   = MulDiv(m_vidrect.left,   m_size.cy, size.cy);
        m_vidrect.right  = MulDiv(m_vidrect.right,  m_size.cy, size.cy);
    }
    m_virtualTextureSize = m_size;

    return S_OK;
}

STDMETHODIMP CSubPicImpl::SetVirtualTextureSize(const SIZE pSize, const POINT pTopLeft)
{
    m_virtualTextureSize.SetSize(pSize.cx, pSize.cy);
    m_virtualTextureTopLeft.SetPoint(pTopLeft.x, pTopLeft.y);

    return S_OK;
}

STDMETHODIMP_(void) CSubPicImpl::SetInverseAlpha(bool bInverted)
{
    m_bInvAlpha = bInverted;
}

STDMETHODIMP CSubPicImpl::GetRelativeTo(RelativeTo* pRelativeTo) const
{
    CheckPointer(pRelativeTo, E_POINTER);

    *pRelativeTo = m_relativeTo;

    return S_OK;
}

STDMETHODIMP CSubPicImpl::SetRelativeTo(RelativeTo relativeTo)
{
    m_relativeTo = relativeTo;

    return S_OK;
}

//
// ISubPicAllocatorImpl
//

CSubPicAllocatorImpl::CSubPicAllocatorImpl(SIZE cursize, bool fDynamicWriteOnly)
    : CUnknown(NAME("ISubPicAllocatorImpl"), nullptr)
    , m_cursize(cursize)
    , m_fDynamicWriteOnly(fDynamicWriteOnly)
{
    m_curvidrect = CRect(CPoint(0, 0), m_cursize);
}

STDMETHODIMP CSubPicAllocatorImpl::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
    return
        QI(ISubPicAllocator)
        __super::NonDelegatingQueryInterface(riid, ppv);
}

// ISubPicAllocator

STDMETHODIMP CSubPicAllocatorImpl::SetCurSize(SIZE cursize)
{
    if (m_cursize != cursize) {
        TRACE(_T("CSubPicAllocatorImpl::SetCurSize: %dx%d\n"), cursize.cx, cursize.cy);
        m_cursize = cursize;
        FreeStatic();
    }
    return S_OK;
}

STDMETHODIMP CSubPicAllocatorImpl::SetCurVidRect(RECT curvidrect)
{
    m_curvidrect = curvidrect;
    return S_OK;
}

STDMETHODIMP CSubPicAllocatorImpl::GetStatic(ISubPic** ppSubPic)
{
    CheckPointer(ppSubPic, E_POINTER);

    {
        CAutoLock cAutoLock(&m_staticLock);

        if (!m_pStatic) {
            if (!Alloc(true, &m_pStatic) || !m_pStatic) {
                TRACE(_T("CSubPicAllocatorImpl::GetStatic failed\n"));
                return E_OUTOFMEMORY;
            }
        }

        *ppSubPic = m_pStatic;
    }

    (*ppSubPic)->AddRef();
    (*ppSubPic)->SetSize(m_cursize, m_curvidrect);

    return S_OK;
}

STDMETHODIMP CSubPicAllocatorImpl::AllocDynamic(ISubPic** ppSubPic)
{
    CheckPointer(ppSubPic, E_POINTER);

    if (!Alloc(false, ppSubPic) || !*ppSubPic) {
        return E_OUTOFMEMORY;
    }

    (*ppSubPic)->SetSize(m_cursize, m_curvidrect);

    return S_OK;
}

STDMETHODIMP_(bool) CSubPicAllocatorImpl::IsDynamicWriteOnly() const
{
    return m_fDynamicWriteOnly;
}

STDMETHODIMP CSubPicAllocatorImpl::ChangeDevice(IUnknown* pDev)
{
    return FreeStatic();
}

STDMETHODIMP CSubPicAllocatorImpl::FreeStatic()
{
    CAutoLock cAutoLock(&m_staticLock);
    if (m_pStatic) {
        m_pStatic.Release();
    }
    return S_OK;
}

STDMETHODIMP_(void) CSubPicAllocatorImpl::SetInverseAlpha(bool bInverted)
{
    m_bInvAlpha = bInverted;
}
