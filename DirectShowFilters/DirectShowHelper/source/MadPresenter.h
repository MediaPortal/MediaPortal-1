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
#include "StdString.h"

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
      //CAutoLock cAutoLock(this); // TODO need to be commented to avoid deadlock.
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

  class CSubRenderCallback : public CUnknown, public ISubRenderCallback4, public CCritSec
  {
    MPMadPresenter* m_pDXRAPSUB;

    public: CSubRenderCallback(MPMadPresenter* pDXRAPSUB) : CUnknown(_T("CSubRender"), NULL) , m_pDXRAPSUB(pDXRAPSUB) {}

    DECLARE_IUNKNOWN
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv)
    {
      return
        QI(ISubRenderCallback4)
        __super::NonDelegatingQueryInterface(riid, ppv);
    }

    void SetDXRAPSUB(MPMadPresenter* pDXRAPSUB)
    {
      //CAutoLock cAutoLock(this); // TODO need to be commented to avoid deadlock.
      m_pDXRAPSUB = pDXRAPSUB;
    }

    // ISubRenderCallback

    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAPSUB ? m_pDXRAPSUB->SetDevice(pD3DDev) : E_UNEXPECTED;
    }

    STDMETHODIMP Render(REFERENCE_TIME rtStart, int left, int top, int right, int bottom, int width, int height)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAPSUB ? m_pDXRAPSUB->Render(rtStart, left, top, right, bottom, width, height) : E_UNEXPECTED;
    }

    STDMETHODIMP RenderEx(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, int left, int top, int right, int bottom, int width, int height)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAPSUB ? m_pDXRAPSUB->Render(frameStart, left, top, right, bottom, width, height) : E_UNEXPECTED;
    }

    STDMETHODIMP RenderEx2(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, RECT croppedVideoRect, RECT originalVideoRect, RECT viewportRect, const double videoStretchFactor = 1.0)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAPSUB ? m_pDXRAPSUB->Render(frameStart, croppedVideoRect.left, croppedVideoRect.top, croppedVideoRect.right, croppedVideoRect.bottom, viewportRect.top, viewportRect.right) : E_UNEXPECTED;
    }

    STDMETHODIMP RenderEx3(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, RECT croppedVideoRect, RECT originalVideoRect, RECT viewportRect, const double videoStretchFactor = 1.0, int xOffsetInPixels = 0, DWORD flags = 0)
    {
      CAutoLock cAutoLock(this);
      return m_pDXRAPSUB ? m_pDXRAPSUB->RenderEx3(std::move(frameStart), std::move(frameStop), std::move(avgTimePerFrame), std::move(croppedVideoRect), std::move(originalVideoRect), std::move(viewportRect), std::move(videoStretchFactor), xOffsetInPixels) : E_UNEXPECTED;
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
    void SetMadVrPaused(bool paused);
    void RepeatFrame();
    void MadVr3DSizeRight(uint16_t x, uint16_t y, DWORD width, DWORD height);
    void MadVr3DSizeLeft(uint16_t x, uint16_t y, DWORD width, DWORD height);
    void MadVrScreenResize(uint16_t x, uint16_t y, DWORD width, DWORD height, bool displayChange);
    void MadVr3D(bool Enable);
    HRESULT Shutdown();
    HRESULT Stopping();

    //madVR Window
    CStdString m_className;
    void SetDsWndVisible(bool);
    //virtual OAHWND HWnDMadVR() { return reinterpret_cast<OAHWND>(m_hWnd); }
    static LRESULT CALLBACK WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
    HWND m_hWnd;
    bool InitMadvrWindow(HWND &hWnd);
    void DeInitMadvrWindow();
    HINSTANCE m_hInstance;
    #if !defined(NPT_POINTER_TO_LONG)
    #define NPT_POINTER_TO_LONG(_p) ((long)(_p))
    #endif

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
    STDMETHODIMP ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev);
    STDMETHODIMP SetDeviceOsd(IDirect3DDevice9* pD3DDev);
    // ISubRenderCallback
    STDMETHOD(Render)(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height);
    // ISubRenderCallback2
    STDMETHOD(RenderEx)(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, int left, int top, int right, int bottom, int width, int height);
    // ISubRenderCallback3
    STDMETHOD(RenderEx2)(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, RECT croppedVideoRect, RECT originalVideoRect, RECT viewportRect, const double videoStretchFactor = 1.0);
    // ISubRenderCallback4
    STDMETHOD(RenderEx3)(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, RECT croppedVideoRect, RECT originalVideoRect, RECT viewportRect, const double videoStretchFactor = 1.0, int xOffsetInPixels = 0, DWORD flags = 0);

    virtual void EnableExclusive(bool bEnable);

    bool m_pShutdown = false;
    bool m_pInitOSD = false;
    bool m_pReInitOSD = false;
    IVMR9Callback* m_pCallback = nullptr;
    CCritSec m_dsLock;

  private:
    void RenderToTexture(IDirect3DTexture9* pTexture);
    void RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture);

    HRESULT SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf);
    HRESULT SetupOSDVertex3D(IDirect3DVertexBuffer9* pVertextBuf);
    void ReinitOSD();
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
    DWORD m_dwLeft = 0;
    DWORD m_dwTop = 0;

    DWORD m_dwWidthLeft = 0;
    DWORD m_dwHeightLeft = 0;
    DWORD m_dwLeftLeft = 0;
    DWORD m_dwTopLeft = 0;

    bool  m_madVr3DEnable = false;

    DeviceState m_deviceState;

    int countFrame = 0;
    int firstFrame = 1;
    int secondFrame = 3;
    int resetFrame = -1;
    int m_pRefCount = 0;
    int m_pMadVRFrameCount = 0;

    Com::SmartPtr<IOsdRenderCallback> m_pORCB;
    Com::SmartPtr<ISubRenderCallback> m_pSRCB;

    bool m_pInitOSDRender = false;
    int m_ExclusiveMode = 0;
    int m_enableOverlay = 0;

    CRenderWait m_mpWait;

    bool uiVisible = false;

    IDirect3DSurface9* m_pSurfaceDevice = nullptr;

    double m_pRefreshrate = 0;
    bool m_pPaused = false;
};

