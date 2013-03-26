// Copyright (C) 2005-2010 Team MediaPortal
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

#pragma warning(disable: 4244)

#include <streams.h>
#include <atlbase.h>
#include <d3dx9.h>
#include <vmr9.h>

#include "dx9allocatorpresenter.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

void Log(const char *fmt, ...) ;

CVMR9AllocatorPresenter::CVMR9AllocatorPresenter(IDirect3DDevice9* direct3dDevice, IVMR9Callback* callback, HMONITOR monitor)
: m_refCount(1)
{
  Log("----------v0.4---------------------------");
  m_hMonitor = monitor;
  m_pD3DDev = direct3dDevice;
  m_pCallback = callback;
  m_surfaceCount = 0;
  m_UseOffScreenSurface = false;
  m_pSurfaces = NULL;
}

CVMR9AllocatorPresenter::~CVMR9AllocatorPresenter()
{
  Log("CVMR9AllocatorPresenter dtor");	
  m_pIVMRSurfAllocNotify = NULL;
  m_pD3DDev = NULL;
  Log("CVMR9AllocatorPresenter dtor exit");	
}

void CVMR9AllocatorPresenter::UseOffScreenSurface(bool yesNo)
{
  m_UseOffScreenSurface=yesNo;
}

// IUnknown
HRESULT CVMR9AllocatorPresenter::QueryInterface( 
  REFIID riid,
  void** ppvObject)
{
  HRESULT hr = E_NOINTERFACE;

  if( ppvObject == NULL ) 
  {
    hr = E_POINTER;
  } 
  else if (riid == IID_IVMRSurfaceAllocator9) 
  {
    *ppvObject = static_cast<IVMRSurfaceAllocator9*>(this);
    AddRef();
    hr = S_OK;
  } 
  else if (riid == IID_IVMRImagePresenter9) 
  {
    *ppvObject = static_cast<IVMRImagePresenter9*>(this);
    AddRef();
    hr = S_OK;
  } 
  else if (riid == IID_IVMRWindowlessControl9) 
  {
    *ppvObject = static_cast<IVMRWindowlessControl9*>(this);
    AddRef();
    hr = S_OK;
  } 
  else if (riid == IID_IUnknown) 
  {
    *ppvObject = static_cast<IUnknown*>(static_cast<IVMRSurfaceAllocator9*>(this));
    AddRef();
    hr = S_OK;    
  }

  return hr;
}

ULONG CVMR9AllocatorPresenter::AddRef()
{
  return InterlockedIncrement(&m_refCount);
}

ULONG CVMR9AllocatorPresenter::Release()
{
  Log("CVMR9AllocatorPresenter::Release()");
  ULONG ret = InterlockedDecrement(&m_refCount);
  if( ret == 0 )
  {
    Log("CVMR9AllocatorPresenter::Cleanup()");
    delete this;
  }
  return ret;
}

STDMETHODIMP CVMR9AllocatorPresenter::InitializeDevice(DWORD_PTR dwUserID, VMR9AllocationInfo* lpAllocInfo, DWORD* lpNumBuffers)
{
  previousEndFrame=0;
  if(!lpAllocInfo || !lpNumBuffers)
    return E_POINTER;

  if(!m_pIVMRSurfAllocNotify)
    return E_FAIL;

  Log("vmr9:InitializeDevice() %dx%d AR %d:%d flags:%d buffers:%d  fmt:(%x) %c%c%c%c", 
    lpAllocInfo->dwWidth,lpAllocInfo->dwHeight, 
    lpAllocInfo->szAspectRatio.cx,lpAllocInfo->szAspectRatio.cy,
    lpAllocInfo->dwFlags,
    *lpNumBuffers,
    lpAllocInfo->Format,
    ((char)lpAllocInfo->Format&0xff),
    ((char)(lpAllocInfo->Format>>8)&0xff),
    ((char)(lpAllocInfo->Format>>16)&0xff),
    ((char)(lpAllocInfo->Format>>24)&0xff));
  if (m_UseOffScreenSurface)
    lpAllocInfo->dwFlags =VMR9AllocFlag_OffscreenSurface;

  m_surfaceCount=*lpNumBuffers;

  HRESULT hr;

  m_pSurfaces = new IDirect3DSurface9* [m_surfaceCount];


  DWORD dwFlags=lpAllocInfo->dwFlags;
  Log("vmr9:flags:");
  if (dwFlags & VMR9AllocFlag_3DRenderTarget)   Log("vmr9:  3drendertarget");
  if (dwFlags & VMR9AllocFlag_DXVATarget)		    Log("vmr9:  DXVATarget");
  if (dwFlags & VMR9AllocFlag_OffscreenSurface) Log("vmr9:  OffscreenSurface");
  if (dwFlags & VMR9AllocFlag_RGBDynamicSwitch) Log("vmr9:  RGBDynamicSwitch");
  if (dwFlags & VMR9AllocFlag_TextureSurface)   Log("vmr9:  TextureSurface");

  lpAllocInfo->dwFlags = VMR9AllocFlag_TextureSurface | VMR9AllocFlag_3DRenderTarget;
  lpAllocInfo->Format = D3DFMT_UNKNOWN;

  hr = m_pIVMRSurfAllocNotify->AllocateSurfaceHelper(lpAllocInfo, lpNumBuffers, m_pSurfaces);
  if(FAILED(hr))
  {
    Log("vmr9:InitializeDevice()   AllocateSurfaceHelper returned:0x%x",hr);
    return hr;
  }
  m_iVideoWidth=lpAllocInfo->dwWidth;
  m_iVideoHeight=lpAllocInfo->dwHeight;
  m_iARX=lpAllocInfo->szAspectRatio.cx;
  m_iARY=lpAllocInfo->szAspectRatio.cy;

  Log("vmr9:InitializeDevice() done()");
  return hr;
}


STDMETHODIMP CVMR9AllocatorPresenter::TerminateDevice(DWORD_PTR dwUserID)
{
  Log("vmr9:TerminateDevice()");
  DeleteSurfaces();
  return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetSurface(DWORD_PTR dwUserID, DWORD SurfaceIndex, DWORD SurfaceFlags, IDirect3DSurface9** lplpSurface)
{
  ULONG refcnt;	
  if (!lplpSurface)
  {
    Log("vmr9:GetSurface() invalid pointer");
    return E_POINTER;
  }

  if (SurfaceIndex >= m_surfaceCount ) 
  {
    Log("vmr9:GetSurface() invalid SurfaceIndex:%d",SurfaceIndex);
    return E_FAIL;
  }
  *lplpSurface = m_pSurfaces[SurfaceIndex];
  refcnt = m_pSurfaces[SurfaceIndex]->AddRef();
  return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::AdviseNotify(IVMRSurfaceAllocatorNotify9* lpIVMRSurfAllocNotify)
{
  //CAutoLock cAutoLock(this);

  Log("vmr9:AdviseNotify()");
  m_pIVMRSurfAllocNotify = lpIVMRSurfAllocNotify;

  HRESULT hr;
  if(FAILED(hr = m_pIVMRSurfAllocNotify->SetD3DDevice(m_pD3DDev, m_hMonitor)))
  {
    Log("vmr9:AdviseNotify() failed to set d3d device:%x",hr);
    return hr;
  }
  return S_OK;
}

// IVMRImagePresenter9

STDMETHODIMP CVMR9AllocatorPresenter::StartPresenting(DWORD_PTR dwUserID)
{
  //CAutoLock cAutoLock(this);
  //ASSERT(m_pD3DDev);

  Log("vmr9:StartPresenting()");
  return m_pD3DDev ? S_OK : E_FAIL;
}

STDMETHODIMP CVMR9AllocatorPresenter::StopPresenting(DWORD_PTR dwUserID)
{
  Log("vmr9:StopPresenting()");
  return S_OK;
}


STDMETHODIMP CVMR9AllocatorPresenter::PresentImage(DWORD_PTR dwUserID, VMR9PresentationInfo* lpPresInfo)
{
  HRESULT hr;
  static long frameCounter = 0;
  frameCounter++;

  if (!m_pIVMRSurfAllocNotify)
  {
    Log("vmr9:PresentImage() allocNotify not set");
    return E_FAIL;
  }
  if (!lpPresInfo || !lpPresInfo->lpSurf)
  {
    Log("vmr9:PresentImage() no surface");
    return E_POINTER;
  }

  try
  {
    /*
    if (lpPresInfo->rtStart>previousEndFrame)
    {
    Log("vmr9:Paint() begin:%d", lpPresInfo->rtStart);
    Log("             end:  %d",lpPresInfo->rtEnd);
    Log("             prev: %d",previousEndFrame);
    }*/
    previousEndFrame=lpPresInfo->rtEnd;
    //CAutoLock cAutoLock(this);

    m_fps = 10000000.0 / (lpPresInfo->rtEnd - lpPresInfo->rtStart);

    m_iARX=lpPresInfo->szAspectRatio.cx;
    m_iARY=lpPresInfo->szAspectRatio.cy;
    Paint(lpPresInfo->lpSurf, lpPresInfo->szAspectRatio);
  }
  catch(...)
  {
    Log("vmr9:PresentImage() exception");
  }
  hr = S_OK;

  return hr;
}

void CVMR9AllocatorPresenter::ReleaseCallBack()
{
  m_pCallback=NULL;
}
void CVMR9AllocatorPresenter::DeleteSurfaces()
{
  Log("vmr9:DeleteSurfaces()");

  if (m_pCallback!=NULL)
  {
    m_pCallback->PresentImage(0,0,0,0,0,0);
  }
  if (m_pSurfaces != NULL)
  {
    Log("vmr9:DeleteSurfaces() m_surfaceCount %d",m_surfaceCount);
    for(ULONG i = 0; i < m_surfaceCount; ++i ) 
    {
      if (m_pSurfaces[i] != NULL)
      {
        ULONG refcnt = m_pSurfaces[i]->Release();
        Log("vmr9:DeleteSurfaces #%d->%d",i,refcnt);
        m_pSurfaces[i] = NULL;
      }
    }
    delete m_pSurfaces;
    m_pSurfaces = NULL;
  }
  CAutoLock cAutoLock(this);

}

void CVMR9AllocatorPresenter::Paint(IDirect3DSurface9* pSurface, SIZE szAspectRatio)
{
  try
  {
    if (m_pCallback==NULL || pSurface == NULL)
      return;

    CComPtr<IDirect3DTexture9> pTexture = NULL;
    pSurface->GetContainer(IID_IDirect3DTexture9, (void**)&pTexture);
    m_pCallback->PresentImage(m_iVideoWidth, m_iVideoHeight, m_iARX,m_iARY, (DWORD)(IDirect3DTexture9*)pTexture, (DWORD)(IDirect3DSurface9*)pSurface);
  }
  catch(...)
  {
    Log("vmr9:Paint() invalid exception");
  }
}

STDMETHODIMP CVMR9AllocatorPresenter::GetNativeVideoSize( 
  /* [out] */ LONG *lpWidth,
  /* [out] */ LONG *lpHeight,
  /* [out] */ LONG *lpARWidth,
  /* [out] */ LONG *lpARHeight) 
{
  *lpWidth=m_iVideoWidth;
  *lpHeight=m_iVideoHeight;
  *lpARWidth=m_iARX;
  *lpARHeight=m_iARY;
  return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetMinIdealVideoSize( 
  /* [out] */ LONG *lpWidth,
  /* [out] */ LONG *lpHeight)
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetMaxIdealVideoSize( 
  /* [out] */ LONG *lpWidth,
  /* [out] */ LONG *lpHeight)
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::SetVideoPosition( 
  /* [in] */ const LPRECT lpSRCRect,
  /* [in] */ const LPRECT lpDSTRect) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetVideoPosition( 
  /* [out] */ LPRECT lpSRCRect,
  /* [out] */ LPRECT lpDSTRect) 
{
  lpSRCRect->left=0;
  lpSRCRect->top=0;
  lpSRCRect->right=m_iVideoWidth;
  lpSRCRect->bottom=m_iVideoHeight;

  lpDSTRect->left=0;
  lpDSTRect->top=0;
  lpDSTRect->right=m_iVideoWidth;
  lpDSTRect->bottom=m_iVideoHeight;

  return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetAspectRatioMode( 
  /* [out] */ DWORD *lpAspectRatioMode) 
{
  *lpAspectRatioMode=VMR_ARMODE_NONE;
  return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::SetAspectRatioMode( 
  /* [in] */ DWORD AspectRatioMode) 
{
  return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::SetVideoClippingWindow( 
  /* [in] */ HWND hwnd) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::RepaintVideo( 
  /* [in] */ HWND hwnd,
  /* [in] */ HDC hdc) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::DisplayModeChanged( void) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetCurrentImage( 
  /* [out] */ BYTE **lpDib) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::SetBorderColor( 
  /* [in] */ COLORREF Clr) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetBorderColor( 
  /* [out] */ COLORREF *lpClr) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::SetColorKey( 
  /* [in] */ COLORREF Clr) 
{
  return E_NOTIMPL;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetColorKey( 
  /* [out] */ COLORREF *lpClr) 
{
  if(lpClr) *lpClr = 0;
  return S_OK;
}
