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
#include "MadSubtitleProxy.h"
#include "DeviceState.h"

using namespace std;

class MPMadPresenter : public CUnknown, public IOsdRenderCallback, public CCritSec
{
  public:
    MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, HWND parent, IDirect3DDevice9* pDevice, IMediaControl* pMediaControl, IGraphBuilder* mGraphbuilder);
    ~MPMadPresenter();

    IBaseFilter* Initialize();
    void Initialize2();
    void InitializeOSD();
    void InitializeOSDClear();
    //void ForceMessage();
    //void ForceMessageExclusive();
    void SetDsWndVisible(bool);
    virtual OAHWND HWnDMadVR() { return reinterpret_cast<OAHWND>(m_hWndKodi); }

    HRESULT Shutdown();

    //madVR Window
    static LRESULT CALLBACK WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
    OAHWND* m_hWndReturn;
    HWND m_hWndKodi;
    bool InitMadvrWindow(HWND &hWnd);
    void DeInitMadvrWindow();
    HINSTANCE m_hInstance;
    bool m_isMadvr;

#if !defined(NPT_POINTER_TO_LONG)
#define NPT_POINTER_TO_LONG(_p) ((long)(_p))
#endif

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObject);

    ULONG STDMETHODCALLTYPE AddRef();
    ULONG STDMETHODCALLTYPE Release();
    ULONG STDMETHODCALLTYPE NonDelegatingAddRef();
    ULONG STDMETHODCALLTYPE NonDelegatingRelease();

    STDMETHODIMP ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev);

    EXCLUSIVEMODECALLBACK m_exclusiveCallback;
    bool m_isEnteringExclusive;
    virtual bool IsEnteringExclusive() { return m_isEnteringExclusive; }

  private:
    void RenderToTexture(IDirect3DTexture9* pTexture, WORD cx, WORD cy, WORD arx, WORD ary);
    void RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture);

    HRESULT SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf);
    HRESULT SetupMadDeviceState();

    MadSubtitleProxy* m_subProxy = nullptr;

    HWND m_hParent = nullptr;
    //OAHWND m_hParent = reinterpret_cast<OAHWND>(nullptr);

    IDirect3DDevice9Ex* m_pDevice = nullptr;
    IDirect3DDevice9Ex* m_pMadD3DDev = nullptr;
    IDirect3DDevice9Ex* m_pMadD3DDevBackup = nullptr;

    IVMR9Callback* m_pCallback = nullptr;

    IMediaControl* m_pMediaControl = nullptr;

    IGraphBuilder* m_pGraphBuilder = nullptr;
    
    CComPtr<IUnknown> m_pMad = nullptr;

    CComQIPtr<IBaseFilter> m_pBaseFilter = nullptr;
    CComQIPtr<IMadVROsdServices> m_pOsdServices = nullptr;
    CComQIPtr<IMadVRExclusiveModeCallback> m_pExclusiveModeCallback = nullptr;
    CComQIPtr<IMadVRDirect3D9Manager> m_pManager = nullptr;
    CComQIPtr<IMadVRSubclassReplacement> m_pSubclassReplacement = nullptr;
    CComQIPtr<ISubRender> m_pSubRender = nullptr;
    CComQIPtr<IVideoWindow> m_pWindow = nullptr;
    CComQIPtr<IMadVRCommand> m_pCommand = nullptr;
    CComQIPtr<IMadVRInfo> m_pInfo = nullptr;
    CComQIPtr<IMadVRSettings2> m_pSettings2 = nullptr;

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

    bool m_pShutdown = false;
    bool m_pInitOSDClear = false;
    int m_ExclusiveMode;

    int countFrame = 0;
    int firstFrame = 1;
    int secondFrame = 3;
    int resetFrame = -1;
    int m_pRefCount = 0;
};

//class MPMadPresenterExclusive : public IMadVRExclusiveModeCallback
//{
//public : 
//  static void __stdcall ExclusiveCallback(LPVOID context, int event);
//};