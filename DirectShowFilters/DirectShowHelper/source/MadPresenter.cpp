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
//#include "..\..\alloctracing.h"
#include "StdString.h"
#include "threads/SystemClock.h"
#include "gdiplus.h"

static HWND g_hWnd;
static CComPtr<IGraphBuilder> mediaControlGraph;
bool StopEvent = false;

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

int GetEncoderClsid(const WCHAR* format, CLSID* pClsid)
{
  UINT  num = 0;          // number of image encoders
  UINT  size = 0;         // size of the image encoder array in bytes

  Gdiplus::ImageCodecInfo* pImageCodecInfo = NULL;

  Gdiplus::GetImageEncodersSize(&num, &size);
  if (size == 0)
    return -1;  // Failure

  pImageCodecInfo = static_cast<Gdiplus::ImageCodecInfo*>(malloc(size));
  if (pImageCodecInfo == NULL)
    return -1;  // Failure

  GetImageEncoders(num, size, pImageCodecInfo);

  for (UINT j = 0; j < num; ++j)
  {
    if (wcscmp(pImageCodecInfo[j].MimeType, format) == 0)
    {
      *pClsid = pImageCodecInfo[j].Clsid;
      free(pImageCodecInfo);
      return j;  // Success
    }
  }

  free(pImageCodecInfo);
  return -1;  // Failure
}

MPMadPresenter::MPMadPresenter(IVMR9Callback* pCallback, int xposition, int yposition, int width, int height, OAHWND parent, IDirect3DDevice9* pDevice, IGraphBuilder* pGraphbuilder) :
  CUnknown(NAME("MPMadPresenter"), nullptr),
  m_pCallback(pCallback),
  m_dwGUIWidth(width),
  m_dwGUIHeight(height),
  m_hParent(parent),
  m_pDevice(static_cast<IDirect3DDevice9Ex*>(pDevice)),
  m_pGraphbuilder(pGraphbuilder)
{
  //Set to true to use the Kodi windows creation or false if not
  m_pKodiWindowUse = false;
  Log("MPMadPresenter::Constructor() - instance 0x%x", this);
  m_pKodiWindowUse ? m_Xposition = 0 : m_Xposition = xposition;
  m_pKodiWindowUse ? m_Yposition = 0 : m_Yposition = yposition;
  m_pShutdown = false;
  m_pDevice->GetRenderTarget(0, &m_pSurfaceDevice);
  // Store device surface MP GUI for later
  m_pCallback->RestoreDeviceSurface(m_pSurfaceDevice);
  m_pInitMadVRWindowPositionDone = false;
  mediaControlGraph = m_pGraphbuilder;
  StopEvent = false;
  Log("MPMadPresenter::Constructor() Store Device Surface");
}

bool isFullscreen(HWND window)
{
  RECT a, b;
  GetWindowRect(window, &a);
  GetWindowRect(GetDesktopWindow(), &b);
  return (a.left == b.left  &&
    a.top == b.top   &&
    a.right == b.right &&
    a.bottom == b.bottom);
}

/* Destructor */
MPMadPresenter::~MPMadPresenter()
{
  {
    // TODO need to be commented to avoid deadlock.
    //CAutoLock cAutoLock(this);

    Log("MPMadPresenter::Destructor() - m_pGraphbuilder release");
    if (m_pGraphbuilder)
    {
      m_pGraphbuilder = nullptr;
    }

    Log("MPMadPresenter::Destructor() - mediaControlGraph release");
    if (mediaControlGraph)
    {
      mediaControlGraph = nullptr;
    }

    if (m_pMad)
    {
      m_pMad = nullptr;
    }

    // Detroy create madVR window and need to be here to avoid some crash
    if (m_pKodiWindowUse)
    {
      DeInitMadvrWindow();
    }

    DestroyWindow(reinterpret_cast<HWND>(pWnd));
    DestroyWindow(reinterpret_cast<HWND>(m_pVideoWnd));

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

  pOR.Release(); // WIP release
  Log("%s", __FUNCTION__);
}

void MPMadPresenter::SetMadVrPaused(bool paused)
{
  if (!m_pPausedDone && !m_pRunDone)
  {
    IMediaControl *m_pControl = nullptr;
    if ((mediaControlGraph) && (SUCCEEDED(mediaControlGraph->QueryInterface(__uuidof(IMediaControl), reinterpret_cast<LPVOID*>(&m_pControl)))) && (m_pControl))
    {
      if (m_pControl)
      {
        if (paused)
        {
          OAFilterState state;
          for (int i1 = 0; i1 < 200; i1++)
          {
            m_pControl->GetState(1000, &state);
            if (state != State_Paused)
            {
              m_pControl->Pause();
              m_pPausedDone = true;
              Log("MPMadPresenter:::SetMadVrPaused() pause");
              Sleep(10);
            }
            else if (state == State_Paused && m_pPausedCount > 1000)
            {
              m_pPausedDone = true;
            }
          }
        }
        if (!paused)
        {
          m_pControl->Run();
          m_pRunDone = true;
          Log("MPMadPresenter:::SetMadVrPaused() run");
        }
        m_pControl->Release();
        m_pControl = nullptr;
      }
    }
    m_pPausedCount++;
  }
}

void MPMadPresenter::RepeatFrame()
{
  if (m_pShutdown)
  {
    Log("MPMadPresenter::RepeatFrame() shutdown");
    return;
  }

  //CAutoLock cAutoLock(this);

  // Render frame to try to fix HD4XXX GPU flickering issue
  Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
  pOR->OsdRedrawFrame();
  pOR.Release(); // WIP release
}

void MPMadPresenter::GrabScreenshot()
{
  if (!m_pInitMadVRWindowPositionDone || m_pShutdown)
  {
    if (m_pCallback)
    {
      m_pCallback->GrabMadVrScreenshot(nullptr);
    }
    return;
  }

  try
  {
    if (m_pMad && m_pCallback)
    {
      if (Com::SmartQIPtr<IBasicVideo> m_pBV = m_pMad)
      {
        LONG nBufferSize = 0;
        HRESULT hr = E_NOTIMPL;
        hr = m_pBV->GetCurrentImage(&nBufferSize, NULL);
        if (hr != S_OK)
        {
          return;
        }
        long* ppData = static_cast<long *>(malloc(nBufferSize));
        hr = m_pBV->GetCurrentImage(&nBufferSize, ppData);
        if (hr != S_OK || !ppData)
        {
          free(ppData);
          return;
        }
        if (ppData)
        {
          PBITMAPINFO bi = PBITMAPINFO(ppData);
          PBITMAPINFOHEADER bih = &bi->bmiHeader;
          int bpp = bih->biBitCount;
          if (bpp != 16 && bpp != 24 && bpp != 32)
          {
            free(ppData);
            return;
          }
          m_pCallback->GrabMadVrScreenshot(LPVOID(ppData));
          free(ppData);
        }
        m_pBV.Release(); // WIP release
      }
    }
  }
  catch (...)
  {
  }
}

void MPMadPresenter::GrabFrame()
{
  {
    //CAutoLock cAutoLock(this);
    if (!m_pInitMadVRWindowPositionDone || m_pShutdown)
    {
      if (m_pCallback)
      {
        m_pCallback->GrabMadVrFrame(nullptr);
      }
      return;
    }

    if (m_pCallback && m_pMad)
    {
      if (Com::SmartQIPtr<IMadVRFrameGrabber> pMadVrFrame = m_pMad)
      {
        LPVOID dibImageBuffer = nullptr;
        pMadVrFrame->GrabFrame(ZOOM_ENCODED_SIZE, FLAGS_NO_SUBTITLES | FLAGS_NO_ARTIFACT_REMOVAL | FLAGS_NO_IMAGE_ENHANCEMENTS | FLAGS_NO_UPSCALING_REFINEMENTS | FLAGS_NO_HDR_SDR_CONVERSION,
          CHROMA_UPSCALING_BILINEAR, IMAGE_DOWNSCALING_BILINEAR, IMAGE_UPSCALING_BILINEAR, 0, &dibImageBuffer, nullptr);

        // Send the DIB to C#
        m_pCallback->GrabMadVrFrame(dibImageBuffer);
        LocalFree(dibImageBuffer);
        pMadVrFrame.Release(); // WIP release
        return;

        try
        {
          Gdiplus::GdiplusStartupInput gdiplusStartupInput;
          ULONG_PTR gdiplusToken;
          GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, nullptr);

          Gdiplus::Bitmap* bm = Gdiplus::Bitmap::FromBITMAPINFO(reinterpret_cast<BITMAPINFO*>(dibImageBuffer), dibImageBuffer);

          // Get the encoder clsid
          CLSID encoderClsid;
          GetEncoderClsid(L"image/png", &encoderClsid);

          // Send the BMP to C#
          //m_pCallback->GrabMadVrScreenshot(bm);

          //// Save the image
          //bm->Save(L"master.png", &encoderClsid, nullptr);

          // All GDI+ objects must be destroyed before GdiplusShutdown is called
          delete bm;
          LocalFree(dibImageBuffer);
          Gdiplus::GdiplusShutdown(gdiplusToken);
          Log("GrabFrame() hr");
        }
        catch (...)
        {
        }
      }
    }
  }
}

void MPMadPresenter::GrabCurrentFrame()
{
  {
    //CAutoLock cAutoLock(this);
    try
    {
      if (!m_pInitMadVRWindowPositionDone || m_pShutdown)
      {
        if (m_pCallback)
        {
          m_pCallback->GrabMadVrCurrentFrame(nullptr);
        }
        return;
      }
      if (m_pCallback && m_pMad)
      {
        if (Com::SmartQIPtr<IMadVRFrameGrabber> pMadVrFrame = m_pMad)
        {
          LPVOID dibImageBuffer = nullptr;
          pMadVrFrame->GrabFrame(ZOOM_50_PERCENT, FLAGS_NO_SUBTITLES | FLAGS_NO_ARTIFACT_REMOVAL | FLAGS_NO_IMAGE_ENHANCEMENTS | FLAGS_NO_UPSCALING_REFINEMENTS | FLAGS_NO_HDR_SDR_CONVERSION,
            CHROMA_UPSCALING_NGU_AA, IMAGE_DOWNSCALING_SSIM1D100, IMAGE_UPSCALING_NGU_SHARP_GRAIN, 0, &dibImageBuffer, nullptr);

          // Send the DIB to C#
          m_pCallback->GrabMadVrCurrentFrame(dibImageBuffer);
          LocalFree(dibImageBuffer);
          dibImageBuffer = nullptr;
          pMadVrFrame.Release(); // WIP release
          //Log("GrabFrame() hr");
        }
      }
    }
    catch (...)
    {
    }
  }
}

void MPMadPresenter::InitMadVRWindowPosition()
{
  if (m_pShutdown)
  {
    Log("MPMadPresenter::InitMadVRWindowPosition() shutdown");
    return;
  }

  if (m_pKodiWindowUse)
  {
    SetDsWndVisible(true);
  }

  // Init created madVR window instance.
  m_pReInitOSD = true;
  m_pInitMadVRWindowPositionDone = true;
}

void MPMadPresenter::MadVr3DSizeRight(int x, int y, int width, int height)
{
  if (m_pMadD3DDev)
  {
    m_dwLeft = x;
    m_dwTop = y;
    m_dwWidth = width;
    m_dwHeight = height;
    Log("%s : init ok for Auto D3D : %d x %d", __FUNCTION__, width, height);
  }
}

void MPMadPresenter::MadVr3DSizeLeft(int x, int y, int width, int height)
{
  if (m_pMadD3DDev)
  {
    m_dwLeftLeft = x;
    m_dwTopLeft = y;
    m_dwWidthLeft = width;
    m_dwHeightLeft = height;
    Log("%s : init ok for Auto D3D : %d x %d", __FUNCTION__, width, height);
  }
}

void MPMadPresenter::MadVrScreenResize(int x, int y, int width, int height, bool displayChange)
{
  // Set window video position when screen change.
  Log("%s : SetWindowPos : %d x %d", __FUNCTION__, width, height);
  if (m_pKodiWindowUse)
  {
    SetWindowPos(m_hWnd, nullptr, 0, 0, width, height, SWP_ASYNCWINDOWPOS);
  }
  else
  {
    SetWindowPos(reinterpret_cast<HWND>(m_hParent), m_hWnd, x, y, width, height, SWP_ASYNCWINDOWPOS);
  }

  // Needed to update OSD/GUI when changing directx present parameter on resolution change.
  if (displayChange)
  {
    if (m_pMadD3DDev)
    {
      // Needed to be set to true only if madVR device is ready
      m_pReInitOSD = true;
    }
    m_dwGUIWidth = width;
    m_dwGUIHeight = height;
    Log("%s : done : %d x %d", __FUNCTION__, width, height);
  }
}

void MPMadPresenter::MadVr3D(bool Enable)
{
  m_madVr3DEnable = Enable;
}

IBaseFilter* MPMadPresenter::Initialize()
{
  if (m_pMad)
  {
    if (Com::SmartQIPtr<IBaseFilter> baseFilter = m_pMad)
    {
      HRESULT hr = mediaControlGraph->AddFilter(baseFilter, _T("madVR"));
      if (FAILED(hr))
      {
        return nullptr;
      }
      return baseFilter;
    }
  }
  return nullptr;
}

STDMETHODIMP MPMadPresenter::CreateRenderer(IUnknown** ppRenderer)
{
  {
    CAutoLock lock(this);
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

    (*ppRenderer = reinterpret_cast<IUnknown*>(static_cast<INonDelegatingUnknown*>(this)))->AddRef();

    return S_OK;
  }
}

STDMETHODIMP MPMadPresenter::SetGrabEvent(HANDLE pGrabEvent)
{
  // ISubRenderCallback
  if (m_pMad)
  {
    Com::SmartQIPtr<ISubRender> pSR = m_pMad;
    if (!pSR)
    {
      m_pMad = nullptr;
      return E_FAIL;
    }

    m_pSRCB = new CSubRenderCallback(this);
    if (pSR && FAILED(pSR->SetCallback(m_pSRCB)))
    {
      m_pMad = nullptr;
      return E_FAIL;
    }
    m_pSRCB->AddRef();
    pSR.Release(); // WIP release
  }

  // IOsdRenderCallback
  if (m_pMad)
  {
    Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
    if (!pOR)
    {
      m_pMad = nullptr;
      return E_FAIL;
    }

    m_pORCB = new COsdRenderCallback(this);
    if (pOR && FAILED(pOR->OsdSetRenderCallback("MP-GUI", m_pORCB)))
    {
      m_pMad = nullptr;
      return E_FAIL;
    }
    m_pORCB->AddRef();
    pOR.Release(); // WIP release
  }

  // Create a madVR Window
  m_pKodiWindowUse ? g_hWnd = reinterpret_cast<HWND>(m_hParent) : g_hWnd = nullptr;
  m_pKodiWindowUse ? InitMadvrWindow(m_hWnd) : m_hWnd = reinterpret_cast<HWND>(m_hParent);
  m_pVideoWnd = CWnd::FromHandle(m_hWnd);
  IVideoWindow *m_pControl = nullptr;
  if (mediaControlGraph && (SUCCEEDED(mediaControlGraph->QueryInterface(__uuidof(IVideoWindow), reinterpret_cast<LPVOID*>(&m_pControl)))) && (m_pControl))
  {
    if (m_pControl)
    {
      VERIFY(SUCCEEDED(m_pControl->put_Owner(reinterpret_cast<OAHWND>(m_hWnd))));
      VERIFY(SUCCEEDED(m_pControl->put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN)));
      VERIFY(SUCCEEDED(m_pControl->put_MessageDrain(reinterpret_cast<OAHWND>(m_hWnd))));
    }
  }

  // release m_pControl
  m_pControl->Release();

  for (pWnd = m_pVideoWnd->GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetNextWindow()) {
    // 1. lets WM_SETCURSOR through (not needed as of now)
    // 2. allows CMouse::CursorOnWindow() to work with m_pVideoWnd
    pWnd->EnableWindow(FALSE);
  }

  if (m_pKodiWindowUse)
  {
    if (m_pCallback)
    {
      m_pCallback->DestroyHWnd(m_hWnd);
      Log("%s : Create and sended DSPlayer window - hWnd: %i", __FUNCTION__, m_hWnd);
    }
  }

  ////if (Com::SmartQIPtr<IVideoWindow> pWindow = m_pMad)
  ////{
  ////  // Create a madVR Window
  ////  if (!m_pKodiWindowUse) // no Kodi window
  ////  {
  ////    m_hWnd = reinterpret_cast<HWND>(m_hParent);
  ////    m_pVideoWnd = CWnd::FromHandle(m_hWnd);
  ////    IVideoWindow *m_pControl = nullptr;
  ////    if ((mediaControlGraph) && (SUCCEEDED(mediaControlGraph->QueryInterface(__uuidof(IVideoWindow), reinterpret_cast<LPVOID*>(&m_pControl)))) && (m_pControl))
  ////    {
  ////      if (m_pControl)
  ////      {
  ////        VERIFY(SUCCEEDED(m_pControl->put_Owner(reinterpret_cast<OAHWND>(m_hWnd))));
  ////        VERIFY(SUCCEEDED(m_pControl->put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN)));
  ////        VERIFY(SUCCEEDED(m_pControl->put_MessageDrain(reinterpret_cast<OAHWND>(m_hWnd))));
  ////        //VERIFY(SUCCEEDED(pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hWnd))));
  ////        //VERIFY(SUCCEEDED(pWindow->put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN)));
  ////        //VERIFY(SUCCEEDED(pWindow->put_MessageDrain(reinterpret_cast<OAHWND>(m_hWnd))));
  ////      }
  ////    }

  ////    // release m_pControl
  ////    m_pControl->Release();

  ////    for (pWnd = m_pVideoWnd->GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetNextWindow()) {
  ////      // 1. lets WM_SETCURSOR through (not needed as of now)
  ////      // 2. allows CMouse::CursorOnWindow() to work with m_pVideoWnd
  ////      pWnd->EnableWindow(FALSE);
  ////    }
  ////  }
  ////  else if (InitMadvrWindow(m_hWnd) && m_pKodiWindowUse) // Kodi window
  ////  {
  ////    m_pCallback->DestroyHWnd(m_hWnd);
  ////    m_pKodiWindowUse ? g_hWnd = reinterpret_cast<HWND>(m_hParent) : g_hWnd = nullptr;
  ////    VERIFY(SUCCEEDED(pWindow->put_Owner(reinterpret_cast<OAHWND>(m_hWnd))));
  ////    VERIFY(SUCCEEDED(pWindow->put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN)));
  ////    VERIFY(SUCCEEDED(pWindow->put_MessageDrain(reinterpret_cast<OAHWND>(m_hWnd))));
  ////    Log("%s : Create DSPlayer window - hWnd: %i", __FUNCTION__, m_hWnd);
  ////    Log("MPMadPresenter::Initialize() send DestroyHWnd value on C# side");
  ////  }
  ////  pWindow.Release(); // WIP release
  ////}

  // Configure initial Madvr Settings
  ConfigureMadvr();

  m_pGrabEvent = pGrabEvent;
  return S_OK;
}

STDMETHODIMP MPMadPresenter::SetStopEvent()
{
  if (m_pSRCB)
  {
    // nasty, but we have to let it know about our death somehow
    static_cast<CSubRenderCallback*>(static_cast<ISubRenderCallback*>(m_pSRCB))->SetShutdownSub(true);
    Log("MPMadPresenter::SetStopEvent() m_pSRCB");
  }

  if (m_pORCB)
  {
    // nasty, but we have to let it know about our death somehow
    static_cast<COsdRenderCallback*>(static_cast<IOsdRenderCallback*>(m_pORCB))->SetShutdownOsd(true);
    Log("MPMadPresenter::SetStopEvent() m_pORCB");
  }
  return S_OK;
}

void MPMadPresenter::EnableExclusive(bool bEnable)
{
  if (m_pMad)
  {
    if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
    {
      pMadVrCmd->SendCommandBool("disableExclusiveMode", !bEnable);
      pMadVrCmd.Release(); // WIP release
    }
  }
};

void MPMadPresenter::ConfigureMadvr()
{
  if (m_pMad)
  {
    if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
    {
      pMadVrCmd->SendCommandBool("disableSeekbar", true);
      pMadVrCmd.Release(); // WIP release
    }

    if (Com::SmartQIPtr<IMadVRDirect3D9Manager> manager = m_pMad)
    {
      manager->ConfigureDisplayModeChanger(false, true);
      manager.Release(); // WIP release
    }

    //// TODO implement IMadVRSubclassReplacement (if enable, it 's breaking mouse event on FSE for MVC)
    //if (Com::SmartQIPtr<IMadVRSubclassReplacement> pSubclassReplacement = m_pMad)
    //{
    //  pSubclassReplacement->DisableSubclassing();
    //  pSubclassReplacement.Release(); // WIP release
    //}

    //if (Com::SmartQIPtr<IVideoWindow> pWindow = m_pMad) // Fix DXVA for FSE
    //{
    //  pWindow->SetWindowPosition(m_Xposition, m_Yposition, m_dwGUIWidth, m_dwGUIHeight);
    //  //pWindow->put_Owner(m_hParent);
    //  pWindow.Release(); // WIP release
    //}

    if (Com::SmartQIPtr<IMadVRSettings> m_pSettings = m_pMad)
    {
      // Read exclusive settings
      m_pSettings->SettingsGetBoolean(L"enableExclusive", &m_ExclusiveMode);

      if (m_ExclusiveMode)
      {
        //m_pSettings->SettingsSetBoolean(L"exclusiveDelay", true);
        m_pSettings->SettingsSetBoolean(L"enableExclusive", true);
      }
      m_pSettings.Release(); // WIP release
    }

    //SetWindowPos(m_hWnd, m_hWnd, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
  }
}

HRESULT MPMadPresenter::Shutdown()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    //CAutoLock lock(this);

    Log("MPMadPresenter::Shutdown() start");

    if (m_pCallback)
    {
      m_pCallback->SetSubtitleDevice(reinterpret_cast<LONG>(nullptr));
      Log("MPMadPresenter::Shutdown() reset subtitle device");
      m_pCallback->RestoreDeviceSurface(m_pSurfaceDevice);
      Log("MPMadPresenter::Shutdown() RestoreDeviceSurface");
      if (m_pKodiWindowUse)
      {
        // for using no Kodi madVR window way comment out this line
        m_pCallback->DestroyHWnd(m_hWnd);
      }
      Log("MPMadPresenter::Shutdown() send DestroyHWnd on C# side");
      m_pCallback->Release();
      m_pCallback = nullptr;
      Log("MPMadPresenter::Shutdown() m_pCallback release");
    }

    // Restore windowed overlay settings
    if (m_pMad)
    {
      if (Com::SmartQIPtr<IMadVRSettings> m_pSettings = m_pMad)
      {
        if (m_enableOverlay)
        {
          m_pSettings->SettingsSetBoolean(L"enableOverlay", true);
          m_pSettings.Release(); // WIP release
        }
      }
    }

    if (m_pDevice != nullptr)
    {
      m_pDevice = nullptr;
    }

    if (m_pSurfaceDevice != nullptr)
    {
      m_pSurfaceDevice = nullptr;
    }

    if (m_hParent)
    {
      m_hParent = NULL;
    }

    Log("MPMadPresenter::Shutdown() stop");
    return S_OK;
  } // Scope for autolock
}

void MPMadPresenter::DeInitMadvrWindow()
{
  if (m_hWnd)
  {
    // remove ourself as user data to ensure we're not called anymore
    SetWindowLongPtr(m_hWnd, GWL_USERDATA, 0);

    // destroy the hidden window
    DestroyWindow(m_hWnd);

    Log("%s : DestroyWindow window - hWnd: %i", __FUNCTION__, m_hWnd);

    // unregister the window class
    UnregisterClass(m_className.c_str(), m_hInstance);
  }

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
  int nX = m_Xposition;
  int nY = m_Yposition;
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
    nX, nY, nWidth, nHeight,
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
  switch (uMsg)
  {
  case WM_MOUSEMOVE:
  case WM_LBUTTONDOWN:
  case WM_MBUTTONDOWN:
  case WM_RBUTTONDOWN:
  case WM_LBUTTONUP:
  case WM_MBUTTONUP:
  case WM_RBUTTONUP:
  case WM_LBUTTONDBLCLK :
  case WM_RBUTTONDBLCLK :
  case WM_MOUSEWHEEL:
  case WM_KEYFIRST :
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

UINT CALLBACK MediaControlThreadProc()
{
  CComPtr<IMediaControl> m_pControl = nullptr;
  if ((mediaControlGraph) && (SUCCEEDED(mediaControlGraph->QueryInterface(__uuidof(IMediaControl), reinterpret_cast<LPVOID*>(&m_pControl)))) && (m_pControl))
  if (m_pControl)
  {
    m_pControl->Pause();
    m_pControl->GetState(1000, nullptr);

    m_pControl->Stop();
    m_pControl->GetState(1000, nullptr);

    OAFilterState state;
    for (int i1 = 0; i1 < 200; i1++)
    {
      m_pControl->GetState(INFINITE, &state);
      if (state == State_Stopped)
        break;
      Sleep(10);
    }
    m_pControl = nullptr;
  }
  StopEvent = true;
  return 0;
}

void MediaControlStopThread()
{
  DWORD tid;
  CloseHandle(CreateThread(nullptr, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(MediaControlThreadProc), nullptr, 0, &tid));
}

HRESULT MPMadPresenter::Stopping()
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    //CAutoLock lock(this);
    StopEvent = false;

    if (!m_pMad)
    {
      return E_FAIL;
    }

    if (m_pORCB)
    {
      // IOsdRenderCallback
      Com::SmartQIPtr<IMadVROsdServices> pOR = m_pMad;
      if (!pOR)
      {
        m_pMad = nullptr;
        return E_FAIL;
      }

      if (pOR && FAILED(pOR->OsdSetRenderCallback("MP-GUI", nullptr)))
      {
        m_pMad = nullptr;
        return E_FAIL;
      }
      pOR.Release(); // WIP release

      if (m_pORCB)
      {
        // nasty, but we have to let it know about our death somehow
        static_cast<COsdRenderCallback*>(static_cast<IOsdRenderCallback*>(m_pORCB))->SetShutdownOsd(true);
        static_cast<COsdRenderCallback*>(static_cast<IOsdRenderCallback*>(m_pORCB))->SetDXRAP(nullptr);
        m_pORCB->Release();
        m_pORCB = nullptr;
        Log("MPMadPresenter::Stopping() m_pORCB");
      }
    }

    if (m_pSRCB)
    {
      Com::SmartQIPtr<ISubRender> pSR = m_pMad;
      if (!pSR)
      {
        m_pMad = nullptr;
        return E_FAIL;
      }

      if (pSR && (FAILED(pSR->SetCallback(nullptr))))
      {
        m_pMad = nullptr;
        return E_FAIL;
      }
      pSR.Release(); // WIP release

      if (m_pSRCB)
      {
        // nasty, but we have to let it know about our death somehow
        static_cast<CSubRenderCallback*>(static_cast<ISubRenderCallback*>(m_pSRCB))->SetShutdownSub(true);
        static_cast<CSubRenderCallback*>(static_cast<ISubRenderCallback*>(m_pSRCB))->SetDXRAPSUB(nullptr);
        m_pSRCB->Release();
        m_pSRCB = nullptr;
        Log("MPMadPresenter::Stopping() m_pSRCB");
      }
    }

    Log("MPMadPresenter::Stopping() start to stop instance - 1");

    //// Start mediacontrol stop in a thread
    //MediaControlStopThread();

    //while (!StopEvent)
    //{
    //  Sleep(10);
    //  // Wait that the stop event is finished
    //}

    // Stop in current thread
    CComPtr<IMediaControl> m_pControl = nullptr;
    if ((mediaControlGraph) && (SUCCEEDED(mediaControlGraph->QueryInterface(__uuidof(IMediaControl), reinterpret_cast<LPVOID*>(&m_pControl)))) && (m_pControl))
    {
      if (m_pControl)
      {
        m_pControl->Pause();
        m_pControl->GetState(1000, nullptr);

        m_pControl->Stop();
        m_pControl->GetState(1000, nullptr);

        OAFilterState state;
        for (int i1 = 0; i1 < 200; i1++)
        {
          m_pControl->GetState(INFINITE, &state);
          if (state == State_Stopped)
            break;
          Sleep(10);
        }
        m_pControl = nullptr;
      }
    }

    if (Com::SmartQIPtr<IMadVRSettings> m_pSettings = m_pMad)
    {
      Log("MPMadPresenter::Stopping() start to stop instance - 2");
      // Read enableOverlay settings
      m_pSettings->SettingsGetBoolean(L"enableOverlay", &m_enableOverlay);
      Log("MPMadPresenter::Stopping() start to stop instance - 3");

      if (m_enableOverlay)
      {
        m_pSettings->SettingsSetBoolean(L"enableOverlay", false);
        Log("MPMadPresenter::Stopping() disable windowed overlay mode");
      }
      m_pSettings.Release(); // WIP release
    }

    //// Disable exclusive mode
    //// WIP testing, don't disable exclusive mode instead let madVR doing it.
    //if (m_ExclusiveMode)
    //{
    //  MPMadPresenter::EnableExclusive(false);
    //  Log("MPMadPresenter::Stopping() disable exclusive mode");
    //}

    if (m_pMad)
    {
      // Let's madVR restore original display mode (when adjust refresh it's handled by madVR)
      if (Com::SmartQIPtr<IMadVRCommand> pMadVrCmd = m_pMad)
      {
        pMadVrCmd->SendCommand("restoreDisplayModeNow");
        pMadVrCmd.Release();
        Log("MPMadPresenter::Stopping() restoreDisplayModeNow");
      }
    }

    if (m_pKodiWindowUse)
    {
      SetDsWndVisible(false);
    }

    Log("MPMadPresenter::Stopping() m_pSRCB release 1");
    if (m_pSRCB)
      m_pSRCB->Release();
    Log("MPMadPresenter::Stopping() m_pSRCB release 2");

    Log("MPMadPresenter::Stopping() m_pORCB release 1");
    if (m_pORCB)
      m_pORCB->Release();
    Log("MPMadPresenter::Stopping() m_pORCB release 2");

    // Redo check if filter are stopped
    if ((mediaControlGraph) && (SUCCEEDED(mediaControlGraph->QueryInterface(__uuidof(IMediaControl), reinterpret_cast<LPVOID*>(&m_pControl)))) && (m_pControl))
    {
      if (m_pControl)
      {
        Log("MPMadPresenter::Stopping() m_pMediaControl stop 1");
        int counter = 0;
        OAFilterState state = -1;
        m_pControl->Stop();
        m_pControl->GetState(100, &state);
        while (state != State_Stopped)
        {
          Log("MPMadPresenter::Stopping() m_pMediaControl: graph still running");
          Sleep(100);
          m_pControl->GetState(10, &state);
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
        m_pControl.Release();
        m_pControl = nullptr;
        Log("MPMadPresenter::Stopping() m_pMediaControl stop 2");
      }
    }

    if (m_pMadD3DDev != nullptr)
    {
      m_pMadD3DDev = nullptr;
      Log("MPMadPresenter::Stopping() release m_pMadD3DDev");

      // Release devicestate
      m_deviceState.Lock();
      m_deviceState.Shutdown();
      m_deviceState.Unlock();
    }

    if (m_pCallback)
    {
      m_pCallback->SetSubtitleDevice(reinterpret_cast<LONG>(nullptr));
      Log("MPMadPresenter::SetDeviceOsd() reset C# subtitle device");
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
    Log("MPMadPresenter::ClearBackground() shutdown or init OSD");
    return hr;
  }

  // Lock madVR thread while Shutdown()
  //CAutoLock lock(&m_dsLock);

  DWORD videoHeight = activeVideoRect->bottom - activeVideoRect->top;
  DWORD videoWidth = activeVideoRect->right - activeVideoRect->left;

  //CAutoLock cAutoLock(this);

  ReinitOSD(true);

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

  m_dwHeight = fullOutputRect->bottom - fullOutputRect->top; // added back
  m_dwWidth = fullOutputRect->right - fullOutputRect->left;

  RenderToTexture(m_pMPTextureGui);

  if (SUCCEEDED(hr = m_deviceState.Store()))
  {
    hr = m_pCallback->RenderGui(m_dwWidth, m_dwHeight, videoWidth, videoHeight);
    if (m_pCallback->IsUiVisible())
    {
      for (int x = 0; x < m_pMadVRFrameCount; ++x) // need to let in a loop to slow down why ???
      {
        if (x <= 3)
        {
          // commented out (it slown down video on GPU Nvidia)
          m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE);
          //Log("MPMadPresenter::ClearBackground() IsUiVisible");
        }
      }
    }
  }

  uiVisible = hr == S_OK ? true : false;

  //Log("ClearBackground() hr: 0x%08x - 2", hr);

  if (SUCCEEDED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
    if (SUCCEEDED(hr = SetupMadDeviceState()))
      if (SUCCEEDED(hr = SetupOSDVertex(m_pMadGuiVertexBuffer)))
        // Draw MP texture on madVR device's side
        RenderTexture(m_pMadGuiVertexBuffer, m_pRenderTextureGui);

  // For 3D
  if (m_madVr3DEnable)
  {
    if (SUCCEEDED(hr = SetupOSDVertex3D(m_pMadGuiVertexBuffer)))
      // Draw MP texture on madVR device's side
      RenderTexture(m_pMadGuiVertexBuffer, m_pRenderTextureGui);
  }

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
  //CAutoLock lock(&m_dsLock);

  DWORD videoHeight = activeVideoRect->bottom - activeVideoRect->top;
  DWORD videoWidth = activeVideoRect->right - activeVideoRect->left;

  //Log("%s : log activeVideoRect bottom x top : %d x %d", __FUNCTION__, (WORD)activeVideoRect->bottom, (WORD)activeVideoRect->top);
  //Log("%s : log activeVideoRect right x left : %d x %d", __FUNCTION__, (WORD)activeVideoRect->right, (WORD)activeVideoRect->left);
  //Log("%s : log for : %d x %d", __FUNCTION__, m_dwHeight, m_dwWidth);

  //CAutoLock cAutoLock(this);

  ReinitOSD(false);

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

  //Log("%s : log fullOutputRect bottom x top : %d x %d", __FUNCTION__, (WORD)fullOutputRect->bottom, (WORD)fullOutputRect->top);
  //Log("%s : log fullOutputRect right x left : %d x %d", __FUNCTION__, (WORD)fullOutputRect->right, (WORD)fullOutputRect->left);
  //Log("%s : log for : %d x %d", __FUNCTION__, m_dwHeight, m_dwWidth);

  // Handle GetBackBuffer to be done only 2 frames
  //countFrame++;
  //if (countFrame == firstFrame || countFrame == secondFrame)
  {
    // For ambilight system but only working for D3D9
    if (SUCCEEDED(hr = m_pMadD3DDev->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &SurfaceMadVr)))
    {
      if (SUCCEEDED(hr = m_pCallback->RenderFrame(m_dwWidth, m_dwHeight, m_dwWidth, m_dwHeight, reinterpret_cast<LONG>(SurfaceMadVr))))
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
    hr = m_pCallback->RenderOverlay(m_dwWidth, m_dwHeight, videoWidth, videoHeight);
    if (m_pCallback->IsUiVisible())
    {
      for (int x = 0; x < m_pMadVRFrameCount; ++x) // need to let in a loop to slow down why ???
      {
        if (x <= 3)
        {
          // commented out (it slown down video on GPU Nvidia)
          m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE);
          //Log("MPMadPresenter::RenderOsd() IsUiVisible");
        }
      }
    }
  }

  uiVisible = hr == S_OK ? true : false;

  //Log("RenderOsd() hr: 0x%08x - 2", hr);

  if (SUCCEEDED(hr = m_pDevice->PresentEx(nullptr, nullptr, nullptr, nullptr, D3DPRESENT_FORCEIMMEDIATE)))
    if (SUCCEEDED(hr = SetupMadDeviceState()))
      if (SUCCEEDED(hr = SetupOSDVertex(m_pMadOsdVertexBuffer)))
        // Draw MP texture on madVR device's side
        RenderTexture(m_pMadOsdVertexBuffer, m_pRenderTextureOsd);

  // For 3D
  if (m_madVr3DEnable)
  {
    if (SUCCEEDED(hr = SetupOSDVertex3D(m_pMadOsdVertexBuffer)))
      // Draw MP texture on madVR device's side
      RenderTexture(m_pMadOsdVertexBuffer, m_pRenderTextureOsd);
  }

  m_deviceState.Restore();

  //// if we don't unlock, OSD will be slow because it will reach the timeout set in SetOSDCallback()
  //m_mpWait.Unlock();
  //m_dsLock.Unlock();

  if (m_pInitMadVRWindowPositionDone)
  {
    SetEvent(m_pGrabEvent);
  }

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
    if (SUCCEEDED(hr = m_pCallback->SetRenderTarget(reinterpret_cast<LONG>(pSurface))))
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
    rDest.left = m_dwLeft;
    rDest.right = m_dwWidth;
    rDest.top = m_dwTop;

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

HRESULT MPMadPresenter::SetupOSDVertex3D(IDirect3DVertexBuffer9* pVertextBuf)
{
  VID_FRAME_VERTEX* vertices = nullptr;

  // Lock the vertex buffer
  HRESULT hr = pVertextBuf->Lock(0, 0, (void**)&vertices, D3DLOCK_DISCARD);

  if (SUCCEEDED(hr))
  {
    RECT rDest;
    rDest.bottom = m_dwHeightLeft;
    rDest.left = m_dwLeftLeft;
    rDest.right = m_dwWidthLeft;
    rDest.top = m_dwTopLeft;

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

void MPMadPresenter::ReinitOSD(bool type)
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    //CAutoLock cAutoLock(this);

    // Needed to update OSD/GUI when changing directx present parameter on resolution change.
    if (m_pReInitOSD)
    {
      if (type)
      {
        Log("%s : ReinitOSD from : ClearBackground", __FUNCTION__);
      }
      else
      {
        Log("%s : ReinitOSD from : RenderOsd", __FUNCTION__);
      }
      m_pReInitOSD = false;
      m_pMPTextureGui = nullptr;
      m_pMPTextureOsd = nullptr;
      m_pMadGuiVertexBuffer = nullptr;
      m_pMadOsdVertexBuffer = nullptr;
      m_pRenderTextureGui = nullptr;
      m_pRenderTextureOsd = nullptr;
      m_hSharedGuiHandle = nullptr;
      m_hSharedOsdHandle = nullptr;
      m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureGui.p, &m_hSharedGuiHandle);
      m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureOsd.p, &m_hSharedOsdHandle);
      if (m_pMadD3DDev)
      {
        m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadGuiVertexBuffer.p, NULL);
        m_pMadD3DDev->CreateVertexBuffer(sizeof(VID_FRAME_VERTEX) * 4, D3DUSAGE_WRITEONLY, D3DFVF_VID_FRAME_VERTEX, D3DPOOL_DEFAULT, &m_pMadOsdVertexBuffer.p, NULL);
        m_pMadD3DDev->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureGui.p, &m_hSharedGuiHandle);
        m_pMadD3DDev->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pRenderTextureOsd.p, &m_hSharedOsdHandle);
      }
      Log("%s : ReinitOSD for : %d x %d", __FUNCTION__, m_dwGUIWidth, m_dwGUIHeight);
    }
  }
}

void MPMadPresenter::ReinitD3DDevice()
{
  // Needed to release D3D device for resetting a new device from madVR
  try
  {
    if (m_pMPTextureGui)
    {
      m_pMPTextureGui.Release();
      m_pMPTextureGui = nullptr;
    }
    if (m_pMPTextureOsd)
    {
      m_pMPTextureOsd.Release();
      m_pMPTextureOsd = nullptr;
    }
    if (m_pMadGuiVertexBuffer)
    {
      m_pMadGuiVertexBuffer.Release();
      m_pMadGuiVertexBuffer = nullptr;
    }
    if (m_pMadOsdVertexBuffer)
    {
      m_pMadOsdVertexBuffer.Release();
      m_pMadOsdVertexBuffer = nullptr;
    }
    if (m_pRenderTextureGui)
    {
      m_pRenderTextureGui.Release();
      m_pRenderTextureGui = nullptr;
    }
    if (m_pRenderTextureOsd)
    {
      m_pRenderTextureOsd.Release();
      m_pRenderTextureOsd = nullptr;
    }
    m_hSharedGuiHandle = nullptr;
    m_hSharedOsdHandle = nullptr;
    CloseHandle(m_hSharedGuiHandle);
    CloseHandle(m_hSharedOsdHandle);
  }
  catch (...)
  {
    Log("%s : ReinitOSDDevice catch exception");
  }
  Log("%s : ReinitOSDDevice for : %d x %d", __FUNCTION__, m_dwGUIWidth, m_dwGUIHeight);
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
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    HRESULT hr = S_FALSE;

    if (m_pShutdown)
    {
      Log("MPMadPresenter::SetDeviceOsd() shutdown");
      return hr;
    }

    if (!pD3DDev)
    {
      if (m_pMadD3DDev != nullptr)
      {
        m_pMadD3DDev = nullptr;
        Log("MPMadPresenter::SetDeviceOsd() release m_pMadD3DDev");
      }
      return S_OK;
    }

    // Change madVR rendering D3D Device
    // if commented -> deadlock
    ChangeDevice(pD3DDev);

    if (m_pMadD3DDev && m_pCallback)
    {
      m_deviceState.SetDevice(m_pMadD3DDev);
      if (SUCCEEDED(hr = m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureGui.p, &m_hSharedGuiHandle)))
        if (SUCCEEDED(hr = m_pDevice->CreateTexture(m_dwGUIWidth, m_dwGUIHeight, 0, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &m_pMPTextureOsd.p, &m_hSharedOsdHandle)))
        {
          //=============================================
          // TODO disable OSD delay for now (used to force IVideoWindow on C# side)
          m_pCallback->ForceOsdUpdate(true);
          Log("%s : ForceOsdUpdate", __FUNCTION__);

          m_pMadVRFrameCount = m_pCallback->ReduceMadvrFrame();
          Log("%s : reduce madVR frame to : %i", __FUNCTION__, m_pMadVRFrameCount);
          //=============================================
        }
      // Authorize OSD placement
      m_pReInitOSD = true;
      return hr;
    }
    Log("MPMadPresenter::SetDeviceOsd() init madVR Window");
    return S_OK;
  }
}

STDMETHODIMP MPMadPresenter::ChangeDevice(IUnknown* pDev)
{
  CComQIPtr<IDirect3DDevice9Ex> pD3DDev = pDev;
  CheckPointer(pD3DDev, E_NOINTERFACE);

  //CAutoLock cAutoLock(this);
  HRESULT hr = S_FALSE;
  if (m_pMadD3DDev != pD3DDev)
  {
    m_pMadD3DDev = pD3DDev;
    return S_OK;
  }
  return hr;
}

HRESULT MPMadPresenter::SetDeviceSub(IDirect3DDevice9* pD3DDev)
{
  { // Scope for autolock for the local variable (lock, which when deleted releases the lock)
    HRESULT hr = S_FALSE;

    if (m_pShutdown)
    {
      Log("MPMadPresenter::SetDeviceSub() shutdown");
      return hr;
    }

    // init or update madVR rendering D3D Device
    ChangeDevice(pD3DDev);

    if (m_pCallback)
    {
      m_pCallback->SetSubtitleDevice(reinterpret_cast<LONG>(pD3DDev));
      Log("MPMadPresenter::SetDeviceSub() send subtitle device to C# 0x:%x", pD3DDev);
      return S_OK;
    }
    return hr;
  }
}

HRESULT MPMadPresenter::Render(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height)
{
  return RenderEx(frameStart, 0, 0, left, top, right, bottom, width, height);
}

HRESULT MPMadPresenter::RenderEx(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, int left, int top, int right, int bottom, int width, int height)
{
  return RenderEx2(frameStart, frameStop, avgTimePerFrame, { left, top, right, bottom }, { left, top, right, bottom }, { 0, 0, width, height });
}

HRESULT MPMadPresenter::RenderEx2(REFERENCE_TIME frameStart, REFERENCE_TIME frameStop, REFERENCE_TIME avgTimePerFrame, RECT croppedVideoRect, RECT originalVideoRect, RECT viewportRect, const double videoStretchFactor /*= 1.0*/)
{
  return RenderEx3(std::move(frameStart), std::move(frameStop), std::move(avgTimePerFrame), std::move(croppedVideoRect), std::move(originalVideoRect), std::move(viewportRect), std::move(videoStretchFactor));
}

HRESULT MPMadPresenter::RenderEx3(REFERENCE_TIME rtStart, REFERENCE_TIME rtStop, REFERENCE_TIME atpf, RECT croppedVideoRect, RECT originalVideoRect, RECT viewportRect, const double videoStretchFactor /*= 1.0*/, int xOffsetInPixels /*= 0*/, DWORD flags /*= 0*/)
{
  if (m_pCallback)
  {
    if (m_pShutdown)
    {
      Log("%s : shutdown", __FUNCTION__);
      return S_FALSE;
    }

    m_deviceState.Store();
    SetupMadDeviceState();

    m_pCallback->RenderSubtitleEx(rtStart, viewportRect, croppedVideoRect, xOffsetInPixels);

    // Commented out but useful for testing
    //Log("%s : RenderSubtitle : rtStart: %i, croppedVideoRect.left: %d, croppedVideoRect.top: %d, croppedVideoRect.right: %d, croppedVideoRect.bottom: %d", __FUNCTION__, rtStart, croppedVideoRect.left, croppedVideoRect.top, croppedVideoRect.right, croppedVideoRect.bottom);
    //Log("%s : RenderSubtitle : viewportRect.right : %i, viewportRect.bottom : %i, xOffsetInPixels : %i", __FUNCTION__, viewportRect.right, viewportRect.bottom, xOffsetInPixels);

    m_deviceState.Restore();
  }

  return S_OK;
}