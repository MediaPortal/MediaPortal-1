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

#include "stdafx.h"
#include "callback.h"
#include "mvrInterfaces.h"
#include "DeviceState.h"
#include "helpers/smartptr.h"
#include <tchar.h>
#include "../../mpc-hc_subs/src/DSUtil/DSUtil.h"
#include "threads/Condition.h"
#include "threads/CriticalSection.h"

using namespace std;

  enum SHAREDRENDER_STATE
  {
    RENDERFRAME_LOCK,
    RENDERFRAME_UNLOCK
  };

  class CRenderWait
  {
  public:
    void Wait(int ms);
    void Unlock();
    XbmcThreads::ConditionVariable m_presentevent;
    CCriticalSection m_presentlock;
    SHAREDRENDER_STATE m_renderState;
  };

class MPMadPresenter : public CUnknown, public CCritSec
{
  class COsdRenderCallback : public CUnknown, public IOsdRenderCallback, public CCritSec
  {
    MPMadPresenter* m_pDXRAP;

  public: COsdRenderCallback(MPMadPresenter* pDXRAP) : CUnknown(_T("COsdRender"), NULL) , m_pDXRAP(pDXRAP) {}

    DECLARE_IUNKNOWN
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv)
    {
      return
        QI(IOsdRenderCallback)
        __super::NonDelegatingQueryInterface(riid, ppv);
    }

    void SetDXRAP(MPMadPresenter* pDXRAP)
    {
      CAutoLock cAutoLock(this); // TODO need to be commented to avoid deadlock.
      m_pDXRAP = pDXRAP;
    }

    // IOsdRenderCallback

    STDMETHODIMP ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAP ? m_pDXRAP->ClearBackground(name, frameStart, fullOutputRect, activeVideoRect) : E_UNEXPECTED;
    }

    STDMETHODIMP RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAP ? m_pDXRAP->RenderOsd(name, frameStart, fullOutputRect, activeVideoRect) : E_UNEXPECTED;
    }

    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAP ? m_pDXRAP->SetDeviceOsd(pD3DDev) : E_UNEXPECTED;
    }
  };

  class CSubRenderCallback : public CUnknown, public ISubRenderCallback, public CCritSec
  {
    MPMadPresenter* m_pDXRAP;

  public:
    CSubRenderCallback(MPMadPresenter* pDXRAP)
      : CUnknown(_T("CSubRender"), NULL)
      , m_pDXRAP(pDXRAP) {
    }

    DECLARE_IUNKNOWN
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv)
    {
      return
        QI(ISubRenderCallback)
        __super::NonDelegatingQueryInterface(riid, ppv);
    }

    void SetDXRAP(MPMadPresenter* pDXRAP)
    {
      CAutoLock cAutoLock(this); // TODO need to be commented to avoid deadlock.
      m_pDXRAP = pDXRAP;
    }

    // ISubRenderCallback

    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAP ? m_pDXRAP->SetDevice(pD3DDev) : E_UNEXPECTED;
    }

    STDMETHODIMP Render(REFERENCE_TIME rtStart, int left, int top, int right, int bottom, int width, int height)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAP ? m_pDXRAP->Render(rtStart, left, top, right, bottom, width, height) : E_UNEXPECTED;
    }
  };

  public:

    MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, OAHWND parent, IDirect3DDevice9* pDevice, IMediaControl* pMediaControl);
    ~MPMadPresenter();

    // XBMC
    STDMETHODIMP CreateRenderer(IUnknown** ppRenderer);
    void ConfigureMadvr();

    IBaseFilter* Initialize();
    void InitializeOSD();
    void SetOSDCallback();
    HRESULT Shutdown();

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
    STDMETHODIMP ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev);
    STDMETHODIMP SetDeviceOsd(IDirect3DDevice9* pD3DDev);
    STDMETHOD(Render)(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height);

    virtual void EnableExclusive(bool bEnable);

    bool m_pShutdown = false;
    bool m_pInitOSD = false;
    IVMR9Callback* m_pCallback = nullptr;
    CCritSec m_dsLock;

  private:
    void RenderToTexture(IDirect3DTexture9* pTexture);
    void RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture);

    HRESULT SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf);
    HRESULT SetupMadDeviceState();

    OAHWND m_hParent = reinterpret_cast<OAHWND>(nullptr);

    IDirect3DDevice9Ex* m_pDevice = nullptr;
    IDirect3DDevice9Ex* m_pMadD3DDev = nullptr;

    IMediaControl* m_pMediaControl = nullptr;

    Com::SmartPtr<IUnknown> m_pMad;

    CComQIPtr<IDirect3DTexture9> m_pRenderTextureGui = nullptr;
    CComQIPtr<IDirect3DTexture9> m_pRenderTextureOsd = nullptr;

    CComQIPtr<IDirect3DTexture9> m_pMPTextureGui = nullptr;
    CComQIPtr<IDirect3DTexture9> m_pMPTextureOsd = nullptr;

    CComQIPtr<IDirect3DVertexBuffer9> m_pMadGuiVertexBuffer = nullptr;
    CComQIPtr<IDirect3DVertexBuffer9> m_pMadOsdVertexBuffer = nullptr;

    HANDLE m_hSharedGuiHandle = nullptr;
    HANDLE m_hSharedOsdHandle = nullptr;

    DWORD m_dwGUIWidth = 0;
    DWORD m_dwGUIHeight = 0;

    DWORD m_dwWidth = 0;
    DWORD m_dwHeight = 0;

    DeviceState m_deviceState;

    int countFrame = 0;
    int firstFrame = 1;
    int secondFrame = 3;
    int resetFrame = -1;
    int m_pRefCount = 0;

    Com::SmartPtr<IOsdRenderCallback> m_pORCB;
    Com::SmartPtr<ISubRenderCallback> m_pSRCB;

    bool m_pInitOSDRender = false;
    int m_ExclusiveMode = 0;

    CRenderWait m_mpWait;

    bool uiVisible = false;

    IDirect3DSurface9* m_pSurfaceDevice = nullptr;

    double m_pRefreshrate = 0;
};

