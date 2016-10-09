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

#include "StdAfx.h"

#include <initguid.h>
#include <streams.h>
#include <d3dx9.h>

#include "madpresenter.h"
#include "dshowhelper.h"
#include "mvrInterfaces.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"
#include "StdString.h"
#include "../../mpc-hc_subs/src/dsutil/DSUtil.h"
#include <afxwin.h>
#include "threads/SystemClock.h"

const DWORD D3DFVF_VID_FRAME_VERTEX = D3DFVF_XYZRHW | D3DFVF_TEX1;

struct VID_FRAME_VERTEX
{
  float x;
  float y;
  float z;
  float rhw;
  float u;
  float v;
};

MPMadPresenter::MPMadPresenter(IVMR9Callback* pCallback, DWORD width, DWORD height, OAHWND parent, IDirect3DDevice9* pDevice, IMediaControl* pMediaControl) :
  CUnknown(NAME("MPMadPresenter"), NULL),
  m_pCallback(pCallback),
  m_dwGUIWidth(width),
  m_dwGUIHeight(height),
  m_hParent(parent),
  m_pDevice((IDirect3DDevice9Ex*)pDevice),
  m_pMediaControl(pMediaControl)
{
  Log("MPMadPresenter::Constructor() - instance 0x%x", this);
  m_pShutdown = false;
  m_pDevice->GetRenderTarget(0, &m_pSurfaceDevice);
}

MPMadPresenter::~MPMadPresenter()
{
  {
    CAutoLock cAutoLock(this);

    if (m_pSRCB)
    {
      // nasty, but we have to let it know about our death somehow
      static_cast<CSubRenderCallback*>(static_cast<ISubRenderCallback*>(m_pSRCB))->SetDXRAPSUB(nullptr);
      Log("MPMadPresenter::Destructor() - m_pSRCB");
    }

    if (m_pORCB)
    {
      // nasty, but we have to let it know about our death somehow
      static_cast<COsdRenderCallback*>(static_cast<IOsdRenderCallback*>(m_pORCB))->SetDXRAP(nullptr);
      Log("MPMadPresenter::Destructor() - m_pORCB");
    }

    //// Unregister madVR Exclusive Callback
    //if (Com::SmartQIPtr<IMadVRExclusiveModeCallback> pEXL = m_pDXR)
    //  pEXL->Unregister(m_exclusiveCallback, this);

    Log("MPMadPresenter::Destructor() - m_pSRCB release 1");
    if (m_pSRCB)
      m_pSRCB.Release();
    Log("MPMadPresenter::Destructor() - m_pSRCB release 2");

    Log("MPMadPresenter::Destructor() - m_pORCB release 1");
    if (m_pORCB)
      m_pORCB.Release();
    Log("MPMadPresenter::Destructor() - m_pORCB release 2");

    Log("MPMadPresenter::Destructor() - m_pMad release 1");
    if (m_pMad)
    {
      // Let's madVR restore original display mode (when adjust refresh it's handled by madVR)
      if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
      {
        pMadVrCmd->SendCommand("restoreDisplayModeNow");
        pMadVrCmd.Release();
        Log("MPMadPresenter::Destructor() - restoreDisplayModeNow");
      }
      m_pMad.Release();
    }
    Log("MPMadPresenter::Destructor() - m_pMad release 2");

    // Detroy create madVR window and need to be here to avoid some crash
    DeInitMadvrWindow();

    Log("MPMadPresenter::Destructor() - instance 0x%x", this);
  }
}

void MPMadPresenter::InitializeOSD()
{
  // IOsdRenderCallback
  Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
  if (!pOR)
  {
    m_pMad = nullptr;
    return;
  }

  m_pORCB = new COsdRenderCallback(this);
  if (FAILED(pOR->OsdSetRenderCallback("MP-GUI", m_pORCB)))
  {
    m_pMad = nullptr;
  }
}

void MPMadPresenter::SetMadVrPaused()
{
  CAutoLock cAutoLock(this);

  int counter = 0;
  OAFilterState state = -1;
  m_pMediaControl->GetState(100, &state);
  if (state != State_Paused)
  {
    m_pPaused = false;
  }

  if (m_pMediaControl && !m_pPaused)
  {
    m_pMediaControl->Pause();
    m_pPaused = true;
    Log("MPMadPresenter:::SetMadVrPaused() pause");
  }
}

void MPMadPresenter::RepeatFrame()
{
  if (m_pShutdown)
  {
    Log("MPMadPresenter::ClearBackground() shutdown");
    return;
  }

  CAutoLock cAutoLock(this);

  // Render frame to try to fix HD4XXX GPU flickering issue
  Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
  pOR->OsdRedrawFrame();
}

IBaseFilter* MPMadPresenter::Initialize()
{
  CAutoLock cAutoLock(this);

  if (Com::SmartQIPtr<IBaseFilter> baseFilter = m_pMad)
  {
    if (Com::SmartQIPtr<IVideoWindow> pWindow = m_pMad)
    {
      // Create a madVR Window
      if (InitMadvrWindow(m_hWnd))
      {
        Log("%s : Create DSPlayer window - hWnd: %i", __FUNCTION__, m_hWnd);
        pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hWnd));
        pWindow->put_Visible(reinterpret_cast<OAHWND>(m_hWnd));
      }
    }
    return baseFilter;
  }

  return nullptr;
}

STDMETHODIMP MPMadPresenter::CreateRenderer(IUnknown** ppRenderer)
{
  CheckPointer(ppRenderer, E_POINTER);

  if (m_pMad)
  {
    return E_UNEXPECTED;
  }

  m_pMad.CoCreateInstance(CLSID_madVR, GetOwner());
  if (!m_pMad)
  {
    return E_FAIL;
  }

  Com::SmartQIPtr<ISubRender> pSR = m_pMad;
  if (!pSR)
  {
    m_pMad = nullptr;
    return E_FAIL;
  }

  m_pSRCB = new CSubRenderCallback(this);
  if (FAILED(pSR->SetCallback(m_pSRCB)))
  {
    m_pMad = nullptr;
    return E_FAIL;
  }

  // IOsdRenderCallback
  Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
  if (!pOR)
  {
    m_pMad = nullptr;
    return E_FAIL;
  }

  m_pORCB = new COsdRenderCallback(this);
  if (FAILED(pOR->OsdSetRenderCallback("MP-GUI", m_pORCB)))
  {
    m_pMad = nullptr;
    return E_FAIL;
  }

  // Configure initial Madvr Settings
  ConfigureMadvr();

  //CDSRendererCallback::Get()->Register(this);

  (*ppRenderer = reinterpret_cast<IUnknown*>(static_cast<INonDelegatingUnknown*>(this)))->AddRef();

  return S_OK;
}

void MPMadPresenter::EnableExclusive(bool bEnable)
{
  if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
    pMadVrCmd->SendCommandBool("disableExclusiveMode", !bEnable);
};

void MPMadPresenter::ConfigureMadvr()
{
  if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
    pMadVrCmd->SendCommandBool("disableSeekbar", true);

  if (Com::SmartQIPtr<IMadVRDirect3D9Manager> manager = m_pMad)
    manager->ConfigureDisplayModeChanger(false, true);

  // TODO implement IMadVRSubclassReplacement
  //if (Com::SmartQIPtr<IMadVRSubclassReplacement> pSubclassReplacement = m_pMad)  { }

  //if (Com::SmartQIPtr<IVideoWindow> pWindow = m_pMad)
  //{
  //  pWindow->SetWindowPosition(0, 0, m_dwGUIWidth, m_dwGUIHeight);
  //  pWindow->put_Owner(m_hParent);
  //}

  if (Com::SmartQIPtr<IMadVRSettings> m_pSettings = m_pMad)
  {
    // Read exclusive settings
    m_pSettings->SettingsGetBoolean(L"enableExclusive", &m_ExclusiveMode);

    if (m_ExclusiveMode)
    {
      m_pSettings->SettingsSetBoolean(L"exclusiveDelay", true);
      m_pSettings->SettingsSetBoolean(L"enableExclusive", true);
    }
    //else if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
    //{
    //  MPMadPresenter::EnableExclusive(false);
    //}
  }
}

HRESULT MPMadPresenter::Shutdown()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    CAutoLock lock(this);

    Log("MPMadPresenter::Shutdown() start");

    if (m_pCallback)
    {
      m_pCallback->SetSubtitleDevice(reinterpret_cast<DWORD>(nullptr));
      Log("MPMadPresenter::Shutdown() reset subtitle device");
      m_pCallback->RestoreDeviceSurface(reinterpret_cast<DWORD>(m_pSurfaceDevice));
      Log("MPMadPresenter::Shutdown() RestoreDeviceSurface");
      m_pCallback->Release();
      Log("MPMadPresenter::Shutdown() m_pCallback release");
    }

    Log("MPMadPresenter::Shutdown() stop");
    return S_OK;
  } // Scope for autolock
}

void MPMadPresenter::DeInitMadvrWindow()
{
  // remove ourself as user data to ensure we're not called anymore
  SetWindowLongPtr(m_hWnd, GWL_USERDATA, 0);

  // destroy the hidden window
  DestroyWindow(m_hWnd);

  Log("%s : DestroyWindow window - hWnd: %i", __FUNCTION__, m_hWnd);

  // unregister the window class
  UnregisterClass(m_className.c_str(), m_hInstance);

  // reset the hWnd
  m_hWnd = nullptr;

  Log("%s : DestroyWindow window - done", __FUNCTION__);
}

bool MPMadPresenter::InitMadvrWindow(HWND &hWnd)
{
  m_hInstance = static_cast<HINSTANCE>(GetModuleHandle(nullptr));
  if (m_hInstance == nullptr)
    Log("%s : GetModuleHandle failed with %d", __FUNCTION__, GetLastError());

  int nWidth = m_dwGUIWidth;
  int nHeight = m_dwGUIHeight;
  m_className = "MediaPortal:DSPlayer";

  // Register the windows class
  WNDCLASS wndClass;

  wndClass.style = CS_HREDRAW | CS_VREDRAW | CS_NOCLOSE;
  wndClass.lpfnWndProc = MPMadPresenter::WndProc;
  wndClass.cbClsExtra = 0;
  wndClass.cbWndExtra = 0;
  wndClass.hInstance = m_hInstance;
  wndClass.hIcon = nullptr;
  wndClass.hCursor = nullptr;
  wndClass.hbrBackground = static_cast<HBRUSH>(GetStockObject(BLACK_BRUSH));
  wndClass.lpszMenuName = nullptr;
  wndClass.lpszClassName = m_className.c_str();

  if (!RegisterClass(&wndClass))
  {
    //Log("%s : RegisterClass failed with %d", __FUNCTION__, GetLastError());
    //return false;
  }
  hWnd = CreateWindow(m_className.c_str(), m_className.c_str(),
    WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
    0, 0, nWidth, nHeight,
    reinterpret_cast<HWND>(m_hParent), NULL, m_hInstance, NULL);
  if (hWnd == nullptr)
  {
    Log("%s : CreateWindow failed with %d", __FUNCTION__, GetLastError());
    return false;
  }

  if (hWnd)
    SetWindowLongPtr(hWnd, GWL_USERDATA, NPT_POINTER_TO_LONG(this));

  return true;
}

LRESULT CALLBACK MPMadPresenter::WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
  HWND g_hWnd = nullptr;
  switch (uMsg)
  {
  case WM_MOUSEMOVE:
  case WM_LBUTTONDOWN:
  case WM_MBUTTONDOWN:
  case WM_RBUTTONDOWN:
  case WM_LBUTTONUP:
  case WM_MBUTTONUP:
  case WM_RBUTTONUP:
  case WM_MOUSEWHEEL:
    ::PostMessage(g_hWnd, uMsg, wParam, lParam);
    return(0);
  case WM_SIZE:
    SetWindowPos(hWnd, 0, 0, 0, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam), SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOZORDER);
    return(0);
  }
  return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

void MPMadPresenter::SetDsWndVisible(bool bVisible)
{
  int cmd;
  bVisible ? cmd = SW_SHOW : cmd = SW_HIDE;
  ShowWindow(m_hWnd, cmd);
  UpdateWindow(m_hWnd);
  Log("%s : Set DSPlayer window - Visible: %i", __FUNCTION__, cmd);
}

HRESULT MPMadPresenter::Stopping()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    CAutoLock lock(this);

    if (m_pSRCB)
    {
      // nasty, but we have to let it know about our death somehow
      static_cast<CSubRenderCallback*>(static_cast<ISubRenderCallback*>(m_pSRCB))->SetDXRAPSUB(nullptr);
      Log("MPMadPresenter::Stopping() m_pSRCB");
    }

    if (m_pORCB)
    {
      // nasty, but we have to let it know about our death somehow
      static_cast<COsdRenderCallback*>(static_cast<IOsdRenderCallback*>(m_pORCB))->SetDXRAP(nullptr);
      Log("MPMadPresenter::Stopping() m_pORCB");
    }

    Log("MPMadPresenter::Stopping() m_pSRCB release 1");
    if (m_pSRCB)
      m_pSRCB.Release();
    Log("MPMadPresenter::Stopping() m_pSRCB release 2");

    Log("MPMadPresenter::Stopping() m_pORCB release 1");
    if (m_pORCB)
      m_pORCB.Release();
    Log("MPMadPresenter::Stopping() m_pORCB release 2");

    if (m_pMediaControl)
    {
      Log("MPMadPresenter::Stopping() m_pMediaControl stop 1");
      int counter = 0;
      OAFilterState state = -1;
      m_pMediaControl->Stop();
      m_pMediaControl->GetState(100, &state);
      while (state != State_Stopped)
      {
        Log("MPMadPresenter::Stopping() m_pMediaControl: graph still running");
        Sleep(100);
        m_pMediaControl->GetState(10, &state);
        counter++;
        if (counter >= 30)
        {
          if (state != State_Stopped)
          {
            Log("MPMadPresenter::Stopping() m_pMediaControl: graph still running");
          }
          break;
        }
      }
      m_pMediaControl = nullptr;
      Log("MPMadPresenter::Stopping() m_pMediaControl stop 2");
    }

    // Disable exclusive mode
    if (m_ExclusiveMode)
    {
      MPMadPresenter::EnableExclusive(false);
      Log("MPMadPresenter::Stopping() disable exclusive mode");
    }

    Log("MPMadPresenter::Stopping() stopped");
    return S_OK;
  } // Scope for autolock
}

STDMETHODIMP MPMadPresenter::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  if (riid != IID_IUnknown && m_pMad)
  {
    if (SUCCEEDED(m_pMad->QueryInterface(riid, ppv)))
    {
      return S_OK;
    }
  }

  return __super::NonDelegatingQueryInterface(riid, ppv);
}

void CRenderWait::Wait(int ms)
{
  m_renderState = RENDERFRAME_LOCK;
  XbmcThreads::EndTime timeout(ms);
  CSingleLock lock(m_presentlock);
  while (m_renderState == RENDERFRAME_LOCK && !timeout.IsTimePast())
    m_presentevent.wait(lock, timeout.MillisLeft());
}

void CRenderWait::Unlock()
{
  {
    CSingleLock lock(m_presentlock);
    m_renderState = RENDERFRAME_UNLOCK;
  }
  m_presentevent.notifyAll();
}

HRESULT MPMadPresenter::ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT* fullOutputRect, RECT* activeVideoRect)
{
  HRESULT hr = E_UNEXPECTED;

  if (m_pShutdown)
  {
    Log("MPMadPresenter::ClearBackground() shutdown");
    return hr;
  }

  // Lock madVR thread while Shutdown()
  CAutoLock lock(&m_dsLock);

  WORD videoHeight = (WORD)activeVideoRect->bottom - (WORD)activeVideoRect->top;
  WORD videoWidth = (WORD)activeVideoRect->right - (WORD)activeVideoRect->left;

  //CAutoLock cAutoLock(this);

  //// Ugly hack to avoid flickering (most occurs on Intel GPU)
  //bool isFullScreen = m_pCallback->IsFullScreen();
  //bool isUiVisible = m_pCallback->IsUiVisible();
  //if (isFullScreen)
  //{
  //  if (isUiVisible)
  //  {
  //    //int pRefreshrate = static_cast<int>(m_pRefreshrate);
  //    //Sleep(100 / m_pRefreshrate);
  //    int CountPass = uiVisible ? 1 : 3;
  //    //Log("MPMadPresenter::ClearBackground() uiVisible %x", CountPass);
  //    for (int x = 0; x < CountPass; ++x) // need to let in a loop to slow down why ???
  //    {
  //      // commented out (it slown down video on GPU Nvidia)
  //      //m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE);
  //    }
  //  }
  //  //m_mpWait.Unlock();
  //  //m_dsLock.Unlock();
  //  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_INFO_DISPLAY;
  //}

  uiVisible = false;

  //Log("MPMadPresenter::ClearBackground()");

  if (!m_pMPTextureGui || !m_pMadGuiVertexBuffer || !m_pRenderTextureGui || !m_pCallback)
    return CALLBACK_INFO_DISPLAY;

  m_dwHeight = (WORD)fullOutputRect->bottom - (WORD)fullOutputRect->top; // added back
  m_dwWidth = (WORD)fullOutputRect->right - (WORD)fullOutputRect->left;

  RenderToTexture(m_pMPTextureGui);

  if (SUCCEEDED(hr = m_deviceState.Store()))
  {
    if (SUCCEEDED(hr = m_pCallback->RenderGui(videoWidth, videoHeight, videoWidth, videoHeight)))
    {
      //Log("ClearBackground() hr: 0x%08x - 1", hr);
      if (m_pCallback->IsUiVisible())
      {
        for (int x = 0; x < m_pMadVRFrameCount; ++x) // need to let in a loop to slow down why ???
        {
          if (x <= 3)
          {
            // commented out (it slown down video on GPU Nvidia)
            m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE);
            //Log("MPMadPresenter::ClearBackground() IsUiVisible");
            // Render frame to try to fix HD4XXX GPU flickering issue
            //Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
            //pOR->OsdRedrawFrame();
          }
        }
      }
      uiVisible = hr == S_OK ? true : false;
    }
    else
    {
      return E_UNEXPECTED;
    }
  }

  //Log("ClearBackground() hr: 0x%08x - 2", hr);

  if (SUCCEEDED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
    if (SUCCEEDED(hr = SetupMadDeviceState()))
      if (SUCCEEDED(hr = SetupOSDVertex(m_pMadGuiVertexBuffer)))
        // Draw MP texture on madVR device's side
        RenderTexture(m_pMadGuiVertexBuffer, m_pRenderTextureGui);

  m_deviceState.Restore();

  //// if we don't unlock, OSD will be slow because it will reach the timeout set in SetOSDCallback()
  //m_mpWait.Unlock();
  //m_dsLock.Unlock();

  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_INFO_DISPLAY;
}

HRESULT MPMadPresenter::RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT* fullOutputRect, RECT* activeVideoRect)
{
  HRESULT hr = E_UNEXPECTED;

  if (m_pShutdown)
  {
    Log("MPMadPresenter::RenderOsd() shutdown");
    return hr;
  }

  // Lock madVR thread while Shutdown()
  CAutoLock lock(&m_dsLock);

  WORD videoHeight = (WORD)activeVideoRect->bottom - (WORD)activeVideoRect->top;
  WORD videoWidth = (WORD)activeVideoRect->right - (WORD)activeVideoRect->left;

  //CAutoLock cAutoLock(this);

  //// Ugly hack to avoid flickering (most occurs on Intel GPU)
  //bool isFullScreen = m_pCallback->IsFullScreen();
  //bool isUiVisible = m_pCallback->IsUiVisible();
  //if (isUiVisible)
  //{
  //  // Disabled for now (see http://forum.kodi.tv/showthread.php?tid=154534&pid=1964715#pid1964715)
  //  // Present frame in advance option lead to GUI lag and/or stuttering for Intel GPU
  //  //int pRefreshrate = static_cast<int>(m_pRefreshrate);
  //  //Sleep(100 / m_pRefreshrate);
  //  int CountPass = uiVisible ? 3 : 6;
  //  //Log("MPMadPresenter::RenderOsd() uiVisible %x", CountPass);
  //  for (int x = 0; x < CountPass; ++x) // need to let in a loop to slow down why ???
  //  {
  //    // commented out (it slown down video on GPU Nvidia)
  //    //m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE);
  //  }
  //  //m_mpWait.Unlock();
  //  //m_dsLock.Unlock();
  //  //return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_INFO_DISPLAY;
  //}

  uiVisible = false;

  //Log("MPMadPresenter::RenderOsd()");

  if (!m_pMPTextureOsd || !m_pMadOsdVertexBuffer || !m_pRenderTextureOsd || !m_pCallback)
    return CALLBACK_INFO_DISPLAY;

  IDirect3DSurface9* SurfaceMadVr = nullptr; // This will be released by C# side

  m_dwHeight = (WORD)fullOutputRect->bottom - (WORD)fullOutputRect->top;
  m_dwWidth = (WORD)fullOutputRect->right - (WORD)fullOutputRect->left;

  // Handle GetBackBuffer to be done only 2 frames
  //countFrame++;
  //if (countFrame == firstFrame || countFrame == secondFrame)
  {
    if (SUCCEEDED(hr = m_pMadD3DDev->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &SurfaceMadVr)))
    {
      if (SUCCEEDED(hr = m_pCallback->RenderFrame(videoWidth, videoHeight, videoWidth, videoHeight, reinterpret_cast<DWORD>(SurfaceMadVr))))
      {
        SurfaceMadVr->Release();
      }
      //if (countFrame == secondFrame)
      //{
      //  countFrame = resetFrame;
      //}
    }
  }

  RenderToTexture(m_pMPTextureOsd);

  if (SUCCEEDED(hr = m_deviceState.Store()))
  {
    if (SUCCEEDED(hr = m_pCallback->RenderOverlay(videoWidth, videoHeight, videoWidth, videoHeight)))
    {
      //Log("RenderOsd() hr: 0x%08x - 1", hr);
      if (m_pCallback->IsUiVisible())
      {
        for (int x = 0; x < m_pMadVRFrameCount; ++x) // need to let in a loop to slow down why ???
        {
          if (x <= 3)
          {
            // commented out (it slown down video on GPU Nvidia)
            m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE);
            //Log("MPMadPresenter::RenderOsd() IsUiVisible");
            // Render frame to try to fix HD4XXX GPU flickering issue
            //Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
            //pOR->OsdRedrawFrame();
          }
        }
      }
      uiVisible = hr == S_OK ? true : false;
    }
    else
    {
      return E_UNEXPECTED;
    }
  }

  //Log("RenderOsd() hr: 0x%08x - 2", hr);

  if (SUCCEEDED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
    if (SUCCEEDED(hr = SetupMadDeviceState()))
      if (SUCCEEDED(hr = SetupOSDVertex(m_pMadOsdVertexBuffer)))
        // Draw MP texture on madVR device's side
        RenderTexture(m_pMadOsdVertexBuffer, m_pRenderTextureOsd);

  m_deviceState.Restore();

  //// if we don't unlock, OSD will be slow because it will reach the timeout set in SetOSDCallback()
  //m_mpWait.Unlock();
  //m_dsLock.Unlock();

  return uiVisible ? CALLBACK_USER_INTERFACE : CALLBACK_INFO_DISPLAY;
}

void MPMadPresenter::RenderToTexture(IDirect3DTexture9* pTexture)
{
  if (!m_pDevice)
    return;
  HRESULT hr = E_UNEXPECTED;
  IDirect3DSurface9* pSurface = nullptr; // This will be released by C# side
  if (SUCCEEDED(hr = pTexture->GetSurfaceLevel(0, &pSurface)))
  {
    if (SUCCEEDED(hr = m_pCallback->SetRenderTarget(reinterpret_cast<DWORD>(pSurface))))
    {
      // TODO is it needed ?
      hr = m_pDevice->Clear(0, nullptr, D3DCLEAR_TARGET, D3DXCOLOR(0, 0, 0, 0), 1.0f, 0);
    }
  }
  //Log("RenderToTexture hr: 0x%08x", hr);
}

void MPMadPresenter::RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture)
{
  if (!m_pMadD3DDev)
    return;

  HRESULT hr = E_UNEXPECTED;

  if (SUCCEEDED(hr = m_pMadD3DDev->SetStreamSource(0, pVertexBuf, 0, sizeof(VID_FRAME_VERTEX))))
  {
    if (SUCCEEDED(hr = m_pMadD3DDev->SetTexture(0, pTexture)))
    {
      hr = m_pMadD3DDev->DrawPrimitive(D3DPT_TRIANGLEFAN, 0, 2);
    }
  }
  //Log("RenderTexture hr: 0x%08x", hr);
}

HRESULT MPMadPresenter::SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf)
{
  VID_FRAME_VERTEX* vertices = nullptr;

  // Lock the vertex buffer
  HRESULT hr = pVertextBuf->Lock(0, 0, (void**)&vertices, D3DLOCK_DISCARD);

  if (SUCCEEDED(hr))
  {
    RECT rDest;
    rDest.bottom = m_dwHeight;
    rDest.left = 0;
    rDest.right = m_dwWidth;
    rDest.top = 0;

    vertices[0].x = (float)rDest.left - 0.5f;
    vertices[0].y = (float)rDest.top - 0.5f;
    vertices[0].z = 0.0f;
    vertices[0].rhw = 1.0f;
    vertices[0].u = 0.0f;
    vertices[0].v = 0.0f;

    vertices[1].x = (float)rDest.right - 0.5f;
    vertices[1].y = (float)rDest.top - 0.5f;
    vertices[1].z = 0.0f;
    vertices[1].rhw = 1.0f;
    vertices[1].u = 1.0f;
    vertices[1].v = 0.0f;

    vertices[2].x = (float)rDest.right - 0.5f;
    vertices[2].y = (float)rDest.bottom - 0.5f;
    vertices[2].z = 0.0f;
    vertices[2].rhw = 1.0f;
    vertices[2].u = 1.0f;
    vertices[2].v = 1.0f;

    vertices[3].x = (float)rDest.left - 0.5f;
    vertices[3].y = (float)rDest.bottom - 0.5f;
    vertices[3].z = 0.0f;
    vertices[3].rhw = 1.0f;
    vertices[3].u = 0.0f;
    vertices[3].v = 1.0f;

    hr = pVertextBuf->Unlock();
    if (FAILED(hr))
      return hr;
  }

  return hr;
}

HRESULT MPMadPresenter::SetupMadDeviceState()
{
  HRESULT hr = E_UNEXPECTED;

  RECT newScissorRect;
  newScissorRect.bottom = m_dwHeight;
  newScissorRect.top = 0;
  newScissorRect.left = 0;
  newScissorRect.right = m_dwWidth;

  if (SUCCEEDED(hr = m_pMadD3DDev->SetScissorRect(&newScissorRect)))
    if (SUCCEEDED(hr = m_pMadD3DDev->SetVertexShader(NULL)))
      if (SUCCEEDED(hr = m_pMadD3DDev->SetFVF(D3DFVF_VID_FRAME_VERTEX)))
        if (SUCCEEDED(hr = m_pMadD3DDev->SetPixelShader(NULL)))
          if (SUCCEEDED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE)))
            if (SUCCEEDED(hr = m_pMadD3DDev->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE)))
              if (SUCCEEDED(hr = m_pMadD3DDev->SetRenderState(D3DRS_LIGHTING, FALSE)))
                if (SUCCEEDED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ZENABLE, FALSE)))
                  if (SUCCEEDED(hr = m_pMadD3DDev->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_ONE)))
                    if (SUCCEEDED(hr = m_pMadD3DDev->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA)))
                      return hr;
  return hr;
}

HRESULT MPMadPresenter::SetDeviceOsd(IDirect3DDevice9* pD3DDev)
{
  if (m_pShutdown)
  {
    Log("MPMadPresenter::SetDeviceOsd shutdown");
    return S_OK;
  }

  // Lock madVR thread while Shutdown()
  CAutoLock lock(&m_dsLock);

  //CAutoLock cAutoLock(this);
  if (!pD3DDev)
  {
    // release all resources
    //m_pSubPicQueue = nullptr;
    //m_pAllocator = nullptr;
    if (m_pCallback)
      m_pCallback->SetSubtitleDevice(reinterpret_cast<DWORD>(pD3DDev));
  }
  return S_OK;
}

HRESULT MPMadPresenter::SetDevice(IDirect3DDevice9* pD3DDev)
{
  HRESULT hr = S_FALSE;

  if (m_pShutdown)
  {
    Log("MPMadPresenter::SetDevice() shutdown");
    return hr;
  }

  // Lock madVR thread while Shutdown()
  CAutoLock lock(&m_dsLock);

  //CAutoLock cAutoLock(this);

  Log("MPMadPresenter::SetDevice() device 0x:%x", pD3DDev);

  if (!pD3DDev)
  {
    if (m_pMadD3DDev) m_pMadD3DDev->Release();
    m_pMadD3DDev = nullptr;
  }

  m_pMadD3DDev = static_cast<IDirect3DDevice9Ex*>(pD3DDev);

  if (m_pMadD3DDev)
  {
    m_deviceState.SetDevice(m_pMadD3DDev);

    if (SUCCEEDED(hr = m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureGui.p, &m_hSharedGuiHandle)))
      if (SUCCEEDED(hr = m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureOsd.p, &m_hSharedOsdHandle)))
      {
        hr = S_OK;
        Log("MPMadPresenter::SetDevice() init ok for D3D : 0x:%x", m_pMadD3DDev);
      }

    // Get refresh rate information
    if (Com::SmartQIPtr<IMadVRInfo> m_pInfos = m_pMad)
    {
      m_pInfos->GetDouble("RefreshRate", &m_pRefreshrate);
    }

    m_pInitOSDRender = false;
  }
  else
  {
    if (m_pCallback)
    {
      m_pCallback->SetSubtitleDevice((DWORD)m_pMadD3DDev);
      Log("MPMadPresenter::SetDevice() reset subtitle device");
    }
    Log("MPMadPresenter::SetDevice() Shutdown() 1");
    m_deviceState.Shutdown();
    Log("MPMadPresenter::SetDevice() Shutdown() 2");
  }

  // Init created madVR window instance.
  //SetDsWndVisible(true);
  //Log("MPMadPresenter::SetDevice() init madVR Window");

  return hr;
}

HRESULT MPMadPresenter::Render(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height)
{
  if (m_pCallback)
  {
    if (m_pShutdown)
    {
      Log("MPMadPresenter::Render() shutdown");
      return S_FALSE;
    }

    // Lock madVR thread while Shutdown()
    CAutoLock lock(&m_dsLock);

    CAutoLock cAutoLock(this);

    HRESULT hr = S_FALSE;

    if (!m_pInitOSDRender)
    {
      if (SUCCEEDED(hr = m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadGuiVertexBuffer.p, NULL)))
        if (SUCCEEDED(hr = m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadOsdVertexBuffer.p, NULL)))
          if (SUCCEEDED(hr = m_pMadD3DDev->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureGui.p, &m_hSharedGuiHandle)))
            if (SUCCEEDED(hr = m_pMadD3DDev->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureOsd.p, &m_hSharedOsdHandle)))
            {
              hr = S_OK;
              Log("MPMadPresenter::Render() init ok for D3D : 0x:%x", m_pMadD3DDev);
            }
      if (m_pCallback)
      {
        m_pCallback->SetSubtitleDevice((DWORD)m_pMadD3DDev);
        Log("MPMadPresenter::SetDevice() SetSubtitleDevice for D3D : 0x:%x", m_pMadD3DDev);
      }
      m_pInitOSDRender = true;

      //if (m_pMediaControl)
      //{
      //  OAFilterState _fs = -1;
      //  if (m_pMediaControl) m_pMediaControl->GetState(1000, &_fs);
      //  if (_fs == State_Paused)
      //    m_pMediaControl->Run();
      //  Log("MPMadPresenter::Render() m_pMediaControl : 0x:%x", _fs);
      //}

      // TODO disable OSD delay for now (used to force IVideoWindow on C# side)
      m_pCallback->ForceOsdUpdate(true);
      Log("MPMadPresenter::Render() ForceOsdUpdate");

      m_pMadVRFrameCount = m_pCallback->ReduceMadvrFrame();
      Log("%s : reduce madVR frame to : %i", __FUNCTION__, m_pMadVRFrameCount);

      // Init created madVR window instance.
      SetDsWndVisible(true);
      Log("MPMadPresenter::SetDevice() init madVR Window");
    }
    m_deviceState.Store();
    SetupMadDeviceState();

    m_pCallback->RenderSubtitle(frameStart, left, top, right, bottom, width, height);

    m_deviceState.Restore();
  }

  return S_OK;
}