/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include "OverlayRenderer.h"
#include <overlay.h>
#include <streams.h>
#include <D3d9.h>
#include "LibBlurayWrapper.h"

// TODO move to utils.h (utils.cpp)
#define CONVERT_90KHz_DS(x) (REFERENCE_TIME)(x * 111 + x / 9)
#define CONVERT_DS_90KHz(x) (REFERENCE_TIME)(x / 100 - x / 1000)

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

//#define LOG_DRAWING

extern void LogDebug(const char *fmt, ...);

COverlayRenderer::COverlayRenderer(CLibBlurayWrapper* pLib) :
  m_pLib(pLib),
  m_pD3DDevice(NULL)
{
  m_pPlanes[BD_OVERLAY_PG] = NULL;
  m_pPlanes[BD_OVERLAY_IG] = NULL;
  
  m_pPlanesBackbuffer[BD_OVERLAY_PG] = NULL;
  m_pPlanesBackbuffer[BD_OVERLAY_IG] = NULL;

  m_pARGBTextures[BD_OVERLAY_PG] = NULL;
  m_pARGBTextures[BD_OVERLAY_IG] = NULL;

  ZeroMemory((void*)&m_ARGBBuffer, sizeof(BD_ARGB_BUFFER_EX));

  m_hStopThreadEvent = CreateEvent(0, TRUE, FALSE, 0);
  m_hNewOverlayAvailable = CreateEvent(0, TRUE, FALSE, 0);

  m_hThread = CreateThread(0, 0, COverlayRenderer::ScheduleThreadEntryPoint, (LPVOID)this, 0, NULL);

  m_hOverlayTimerIG = CreateWaitableTimer(NULL, false, NULL);
  m_hOverlayTimerPG = CreateWaitableTimer(NULL, false, NULL);
}

COverlayRenderer::~COverlayRenderer()
{
  CAutoLock renderLock(&m_csRenderLock);
  CAutoLock queueLock(&m_csOverlayQueue);

  FreeOverlayQueue(BD_OVERLAY_IG);
  FreeOverlayQueue(BD_OVERLAY_PG);

  if (m_hOverlayTimerIG)
  {
    CloseHandle(m_hOverlayTimerIG);
    m_hOverlayTimerIG = NULL;
  }

  if (m_hOverlayTimerPG)
  {
    CloseHandle(m_hOverlayTimerPG);
    m_hOverlayTimerPG = NULL;
  }

  if (m_hNewOverlayAvailable)
    CloseHandle(m_hNewOverlayAvailable);

  if (m_hStopThreadEvent)
  {
    SetEvent(m_hStopThreadEvent);
    CloseHandle(m_hStopThreadEvent);
    WaitForSingleObject(m_hThread, INFINITE);
  }

  if (m_hThread)
    CloseHandle(m_hThread);

  CloseOverlay(BD_OVERLAY_PG);
  CloseOverlay(BD_OVERLAY_IG);
}

void COverlayRenderer::CancelTimers()
{
  if (CancelWaitableTimer(m_hOverlayTimerIG) == 0)
  {
    DWORD error = GetLastError();
    LogDebug("COverlayRenderer::CancelTimers - CancelWaitableTimer failed: %d", error);
  }

  if (CancelWaitableTimer(m_hOverlayTimerPG) == 0)
  {
    DWORD error = GetLastError();
    LogDebug("COverlayRenderer::CancelTimers - CancelWaitableTimer failed: %d", error);
  }
}

DWORD WINAPI COverlayRenderer::ScheduleThreadEntryPoint(LPVOID lpParameter)
{
  return ((COverlayRenderer *)lpParameter)->ScheduleThread();
}

DWORD COverlayRenderer::ScheduleThread()
{
  const DWORD eventArraySize = 4;

  HANDLE handles[eventArraySize];
  handles[0] = m_hStopThreadEvent;
  handles[1] = m_hOverlayTimerIG;
  handles[2] = m_hOverlayTimerPG;
  handles[3] = m_hNewOverlayAvailable;

  DWORD stopThread = WAIT_OBJECT_0;
  DWORD processOverlayIG = WAIT_OBJECT_0 + 1;
  DWORD processOverlayPG = WAIT_OBJECT_0 + 2;
  DWORD newOverlayAvailable = WAIT_OBJECT_0 + 3;

  while (true)
  {
    ScheduleOverlays();

    DWORD result = WaitForMultipleObjects(eventArraySize, handles, false, INFINITE);

    if (result == WAIT_FAILED)
      return 0;
    else if (result == stopThread)
      return 0;
    else if(result == newOverlayAvailable)
    {
      CAutoLock queueLock(&m_csOverlayQueue);
      ResetEvent(m_hNewOverlayAvailable);
#ifdef LOG_DRAWING
      LogDebug("newOverlayAvailable");
#endif
    }
    else if (result == processOverlayIG || result == processOverlayPG)
    {
      UINT8 plane = result == processOverlayIG ? BD_OVERLAY_IG : BD_OVERLAY_PG;

      CAutoLock queueLock(&m_csOverlayQueue);

      if (!m_overlayQueue[plane].empty())
      {
        ivecOverlayQueue it = m_overlayQueue[plane].begin();

        if ((*it))
        {
  #ifdef LOG_DRAWING
          LogDebug("RENDERING PTS: %6.3f", (CONVERT_90KHz_DS((*it)->pts) + m_rtOffset) / 10000000.0);
  #endif

          // close frees all overlays
          bool freeOverlay = (*it)->cmd != BD_OVERLAY_CLOSE;

          ProcessOverlay((*it));

          if (freeOverlay)
            FreeOverlay(it);
        }
        else
          FreeOverlay(it);
      }
    }
  }
  
  return 0;
}

ivecOverlayQueue COverlayRenderer::FreeOverlay(ivecOverlayQueue overlay)
{
  ASSERT((*overlay));

  if ((*overlay)->img)
    m_pLib->DecreaseRefCount((*overlay)->img);
      
  if ((*overlay)->palette)
    delete (*overlay)->palette;

  UINT8 plane = (*overlay)->plane;

  delete (*overlay);
  (*overlay) = NULL;

  return m_overlayQueue[plane].erase(overlay);
}

void COverlayRenderer::FreeOverlayQueue(const uint8_t plane)
{
  CAutoLock queueLock(&m_csOverlayQueue);
  ivecOverlayQueue it = m_overlayQueue[plane].begin();
  while (it != m_overlayQueue[plane].end())
  {
    it = FreeOverlay(it);
  }
}

 bool COverlayRenderer::NextScheduleTime(REFERENCE_TIME& rtPts, UINT8 plane)
{
  if (!m_overlayQueue[plane].empty())
  {
    BD_OVERLAY_EX* overlay = *m_overlayQueue[plane].begin();
    if (overlay->scheduled)
      return false;

    if (overlay->pts <= 0)
    {
      rtPts = 0;
#ifdef LOG_DRAWING
      LogDebug("INSTANT HANDLING");
#endif
    }
    else
    {
      REFERENCE_TIME pts = CONVERT_90KHz_DS(overlay->pts);
      rtPts = pts - m_rtPlaybackPosition + m_rtOffset;

      //ASSERT(rtPts > 0);
      if (rtPts < 0)
      {
        rtPts = 0;
        LogDebug("SCHEDULING --- was negative -> 0");
      }

#ifdef LOG_DRAWING
      LogDebug("SCHEDULING PTS: %6.3f wait: %6.3f", pts / 10000000.0, rtPts / 10000000.0);
#endif
    }

    return true;
  }

  rtPts = -1;
  return false;
}

void COverlayRenderer::ScheduleOverlays()
{
  CAutoLock queueLock(&m_csOverlayQueue);

  REFERENCE_TIME rtDue = 0;
  if (NextScheduleTime(rtDue, BD_OVERLAY_IG))
  {
    LARGE_INTEGER liDueTime;
    liDueTime.QuadPart = -rtDue;

    LogDebug("           liDueTime: %6.3f", liDueTime.QuadPart / 10000000.0);

    if (SetWaitableTimer(m_hOverlayTimerIG, &liDueTime, 0, NULL, NULL, 0) == 0)
    {
#ifdef LOG_DRAWING      
      DWORD error = GetLastError();
      LogDebug("COverlayRenderer::ScheduleOverlay - SetWaitableTimer failed: %d", error);
#endif
    }
  }

  if (NextScheduleTime(rtDue, BD_OVERLAY_PG))
  {
    LARGE_INTEGER liDueTime;
    liDueTime.QuadPart = -rtDue;

    LogDebug("           liDueTime: %6.3f", liDueTime.QuadPart / 10000000.0);

    if (SetWaitableTimer(m_hOverlayTimerPG, &liDueTime, 0, NULL, NULL, 0) == 0)
    {
#ifdef LOG_DRAWING
      DWORD error = GetLastError();
      LogDebug("COverlayRenderer::ScheduleOverlay - SetWaitableTimer failed: %d", error);
#endif
    }
  }
}

void COverlayRenderer::SetD3DDevice(IDirect3DDevice9* device)
{
  CAutoLock lock(&m_csRenderLock);
  m_pD3DDevice = device;
}

void COverlayRenderer::SetScr(INT64 pts, INT64 offset)
{
  m_rtPlaybackPosition = CONVERT_90KHz_DS(pts);
  m_rtOffset = CONVERT_90KHz_DS(offset);
}

bool COverlayRenderer:: CreateARGBBuffers(bd_argb_buffer_s** pBuffer)
{
  // only BD_OVERLAY_IG layer is used at the moment, 
  // so no texture is created for BD_OVERLAY_PG layer
  
  *pBuffer = NULL;

  HRESULT hr = m_pD3DDevice->CreateTexture(OVERLAY_WIDTH, OVERLAY_HEIGHT, 1, D3DUSAGE_DYNAMIC, D3DFMT_A8R8G8B8, 
                                          D3DPOOL_DEFAULT, &m_pARGBTextures[BD_OVERLAY_IG], NULL);

  if (FAILED(hr))
  {
    LogDebug("COverlayRenderer::CreateARGBBuffers - failed to create PG texture (0x%08x)", hr);
    return false;
  }

  hr = m_pD3DDevice->CreateTexture(OVERLAY_WIDTH, OVERLAY_HEIGHT, 1, D3DUSAGE_DYNAMIC, D3DFMT_A8R8G8B8, 
                                          D3DPOOL_DEFAULT, &m_pARGBTextures[BD_OVERLAY_IG], NULL);

  if (FAILED(hr))
  {
    LogDebug("COverlayRenderer::CreateARGBBuffers - failed to create IG texture (0x%08x)", hr);
    return false;
  }
 
  BD_ARGB_BUFFER* buf = (BD_ARGB_BUFFER*)&m_ARGBBuffer;

  buf->buf[BD_OVERLAY_PG] = NULL;
  buf->buf[BD_OVERLAY_IG] = NULL;
  buf->width = OVERLAY_WIDTH;
  buf->height = OVERLAY_HEIGHT;
  buf->lock = ARBGLock;
  buf->unlock = ARBGUnlock;
  m_ARGBBuffer.render = this;

  *pBuffer = (BD_ARGB_BUFFER*)&m_ARGBBuffer;

  return true;
}

void ARBGLock(BD_ARGB_BUFFER* buffer)
{
  if (!buffer)
    return;

  BD_ARGB_BUFFER_EX* bufferEx = static_cast<BD_ARGB_BUFFER_EX*>(buffer);
  COverlayRenderer* renderer = static_cast<COverlayRenderer*>(bufferEx->render);

  renderer->LockARGBSurface(bufferEx);
}

void COverlayRenderer::LockARGBSurface(BD_ARGB_BUFFER_EX* buffer)
{
  RECT area = {};
  area.left = buffer->dirty[BD_OVERLAY_IG].x0;
  area.top = buffer->dirty[BD_OVERLAY_IG].y0;
  area.right = buffer->dirty[BD_OVERLAY_IG].x1;
  area.bottom = buffer->dirty[BD_OVERLAY_IG].y1;

  D3DLOCKED_RECT lockedRect = {};

  HRESULT hr = m_pARGBTextures[BD_OVERLAY_IG]->LockRect(0, &lockedRect, &area, 0);
  if (FAILED(hr))
  {
    LogDebug("COverlayRenderer::ARBGLock - failed to lock rect (0x%08x)", hr);
    return;
  }

  int width =  area.right - area.left;
  int height = area.bottom - area.top;

  m_ARGBBuffer.buf[BD_OVERLAY_IG] = (uint32_t*)lockedRect.pBits;
  m_ARGBBuffer.width = lockedRect.Pitch / 4;
  m_ARGBBuffer.height = height + 1;

  AdjustDirtyRect(BD_OVERLAY_IG, area.left, area.top, width, height);
}

void ARBGUnlock(BD_ARGB_BUFFER* buffer)
{
  if (!buffer)
    return;

  BD_ARGB_BUFFER_EX* bufferEx = static_cast<BD_ARGB_BUFFER_EX*>(buffer);
  COverlayRenderer* renderer = static_cast<COverlayRenderer*>(bufferEx->render);

  renderer->UnlockARGBSurface(bufferEx);
}

void COverlayRenderer::UnlockARGBSurface(BD_ARGB_BUFFER_EX* buffer)
{
  HRESULT hr = m_pARGBTextures[BD_OVERLAY_IG]->UnlockRect(0);
  if (FAILED(hr))
  {
    LogDebug("COverlayRenderer::ARBGLock - failed to unlock rect (0x%08x)", hr);
    return;
  }

  CopyToFrontBuffer(BD_OVERLAY_IG, true);

  OSDTexture* plane = m_pPlanes[BD_OVERLAY_IG];
  m_pLib->HandleOSDUpdate(*plane);
}

void COverlayRenderer::OverlayProc(const BD_OVERLAY* ov)
{
  if (!ov)
    return;
  
  if (ov->cmd == BD_OVERLAY_CLOSE || ov->cmd == BD_OVERLAY_INIT)
  {
    ProcessOverlay(ov);
    return;
  }

  BD_OVERLAY_EX *copy = (BD_OVERLAY_EX*)malloc(sizeof(BD_OVERLAY_EX));
  memcpy(copy, ov, sizeof(*ov));
  copy->scheduled = false;

  if (ov->palette) 
  {
    copy->palette = (BD_PG_PALETTE_ENTRY*)malloc(PALETTE_SIZE * sizeof(BD_PG_PALETTE_ENTRY));
    memcpy((void*)copy->palette, ov->palette, PALETTE_SIZE * sizeof(BD_PG_PALETTE_ENTRY));
  }

  if (copy->img)
    m_pLib->IncreaseRefCount(copy->img);

  {
#ifdef LOG_DRAWING
    LogDebug("new arriva PTS: %6.3f wait: %6.3f", (CONVERT_90KHz_DS(copy->pts)) / 10000000.0);
#endif

    CAutoLock queueLock(&m_csOverlayQueue);

    m_overlayQueue[copy->plane].push_back(copy);
    SetEvent(m_hNewOverlayAvailable);
  }
}

void COverlayRenderer::ProcessOverlay(const BD_OVERLAY* pOv)
{
  if (!pOv)
  {
    CloseOverlay(BD_OVERLAY_IG);
    CloseOverlay(BD_OVERLAY_PG);
    return;
  }
  else if (pOv->plane > BD_OVERLAY_IG)
    return;

  LogCommand(pOv);

  switch (pOv->cmd)
  {
    case BD_OVERLAY_INIT:
      OpenOverlay(pOv);
      return;
    case BD_OVERLAY_CLOSE:
      CloseOverlay(pOv->plane);
      FreeOverlayQueue(pOv->plane);
      return;
  }

  OSDTexture* plane = m_pPlanesBackbuffer[pOv->plane];

  // Workaround for some BDs that wont issue BD_OVERLAY_INIT after BD_OVERLAY_CLOSE
  if (!plane) 
  {
    OpenOverlay(pOv);
    plane = m_pPlanesBackbuffer[pOv->plane];

    if (!plane)
      return;
  }

  switch (pOv->cmd) 
  {
    case BD_OVERLAY_DRAW:
      DrawBitmap(plane, pOv);
      break;

    case BD_OVERLAY_WIPE:
      ClearArea(plane, pOv);
      break;

    case BD_OVERLAY_CLEAR:
      ClearOverlay(pOv->plane);
      break;

    case BD_OVERLAY_HIDE: // TODO
      break;

    case BD_OVERLAY_FLUSH:
    {
      CopyToFrontBuffer(pOv->plane);
      
      OSDTexture* plane = m_pPlanes[pOv->plane];
      m_pLib->HandleOSDUpdate(*plane);
      break;
    }

    default:
      break;
  }
}

void COverlayRenderer::ARGBOverlayProc(const BD_ARGB_OVERLAY* ov)
{
  if (!ov)
  {
    CloseOverlay(BD_OVERLAY_IG);
    return;
  }
  else if (ov->plane > BD_OVERLAY_IG)
    return;

  LogARGBCommand(ov);

  switch (ov->cmd)
  {
    case BD_ARGB_OVERLAY_INIT:
      OpenOverlay(ov);
      return;
    case BD_ARGB_OVERLAY_CLOSE:
      CloseOverlay(ov->plane);
      return;
  }

  OSDTexture* pOsdTexture = m_pPlanesBackbuffer[ov->plane];

  switch (ov->cmd)
  {
    case BD_ARGB_OVERLAY_DRAW:
    case BD_ARGB_OVERLAY_FLUSH:
      break;
    default:
      ASSERT(false);
  }
}

void COverlayRenderer::OpenOverlay(const BD_OVERLAY* pOv)
{
  ResetDirtyRect(pOv->plane, pOv->w, pOv->h);

  if ((m_pPlanes[pOv->plane] && pOv) &&
      (m_pPlanes[pOv->plane]->height != pOv->h ||
      m_pPlanes[pOv->plane]->width != pOv->w ||
      m_pPlanes[pOv->plane]->x != pOv->x ||
      m_pPlanes[pOv->plane]->y != pOv->y))
  {
    CloseOverlay(pOv->plane);
    CreateFrontAndBackBuffers(pOv->plane, pOv->x, pOv->y, pOv->w, pOv->h);
  }

  if (!m_pPlanes[pOv->plane])
    CreateFrontAndBackBuffers(pOv->plane, pOv->x, pOv->y, pOv->w, pOv->h);
}

void COverlayRenderer::OpenOverlay(const BD_ARGB_OVERLAY* pOv)
{
  const int plane = pOv->plane;

  ResetDirtyRect(plane, pOv->w, pOv->h);

  if ((m_pPlanes[plane] && pOv) &&
      (m_pPlanes[plane]->height != pOv->h ||
      m_pPlanes[plane]->width != pOv->w ||
      m_pPlanes[plane]->x != pOv->x ||
      m_pPlanes[plane]->y != pOv->y))
  {
    CloseOverlay(plane);
    CreateFrontAndBackBuffers(plane, pOv->x, pOv->y, pOv->w, pOv->h);
  }

  if (!m_pPlanes[plane])
    CreateFrontAndBackBuffers(plane, pOv->x, pOv->y, pOv->w, pOv->h);
}

void COverlayRenderer::CreateFrontAndBackBuffers(uint8_t plane, uint16_t x, uint16_t y, uint16_t w, uint16_t h)
{
  CAutoLock lock(&m_csRenderLock);
  
  if (!m_pD3DDevice)
    return;

  // Create front and back buffer textures
  for (int i = 0; i < 2; i++)
  {
    OSDTexture** ppOsdTexture = NULL;

    if (i == 0)
      ppOsdTexture = &m_pPlanes[plane];
    else
      ppOsdTexture = &m_pPlanesBackbuffer[plane];

    OSDTexture* osdTexture = new OSDTexture;
    osdTexture->height = h; 
    osdTexture->width = w;
    osdTexture->x = x;
    osdTexture->y = y;
    osdTexture->plane = plane;
    osdTexture->texture = NULL;

    HRESULT hr = m_pD3DDevice->CreateTexture(osdTexture->width, osdTexture->height, 1, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, 
                                              D3DPOOL_DEFAULT, &osdTexture->texture, NULL);

    if (SUCCEEDED(hr))
    {
      if (*ppOsdTexture)
        delete *ppOsdTexture;
      (*ppOsdTexture) = osdTexture;
    }
    else
    {
      delete osdTexture;
      osdTexture = NULL;
      LogDebug("ovr: OpenOverlay - CreateTexture 0x%08x", hr);
    }
  }
}

void COverlayRenderer::CloseOverlay(const uint8_t plane)
{
  CAutoLock lock(&m_csOverlayQueue);

  if (m_pPlanes[plane])
  {
    ClearOverlay(plane);
    CopyToFrontBuffer(plane);

    OSDTexture nullTexture = {0};
    nullTexture.plane = plane;
    m_pLib->HandleOSDUpdate(nullTexture);

    // C# side will release the front buffer
    if (m_pPlanes[plane])
    {
      delete m_pPlanes[plane];
      m_pPlanes[plane] = NULL;
    }

    if (m_pPlanesBackbuffer[plane])
    {
      if (m_pPlanesBackbuffer[plane]->texture)
        m_pPlanesBackbuffer[plane]->texture->Release();

      delete m_pPlanes[plane];
      m_pPlanesBackbuffer[plane] = NULL;
    }
  }

  FreeOverlayQueue(plane);
  CancelTimers();
}

void COverlayRenderer::ClearArea(OSDTexture* pPlane, const BD_OVERLAY* pOv)
{
  CAutoLock lock(&m_csRenderLock);

  if (m_pD3DDevice)
  {
    HRESULT hr = S_FALSE;
    
    RECT dstRect;
    dstRect.left = pOv->x;
    dstRect.top = pOv->y;
    dstRect.right = pOv->x + pOv->w;
    dstRect.bottom = pOv->y + pOv->h;

    IDirect3DSurface9* dstSurface = NULL;
    hr = pPlane->texture->GetSurfaceLevel(0, &dstSurface);

    if (SUCCEEDED(hr))
    {
      hr = m_pD3DDevice->ColorFill(dstSurface, &dstRect, 0x00000000);
      if (FAILED(hr))
        LogDebug("ovr: ClearArea - ColorFill failed: 0x%08x");
      else
        AdjustDirtyRect(pOv->plane, pOv->x, pOv->y, pOv->w, pOv->h);
    }
    else
      LogDebug("ovr: ClearArea - GetSurfaceLevel failed: 0x%08x");
  }
}

void COverlayRenderer::ClearOverlay(const uint8_t plane)
{
  if (m_pPlanes[plane])
  {
    BD_OVERLAY ov;
    ov.x = 0;
    ov.y = 0;
    ov.w = m_pPlanes[plane]->width;
    ov.h = m_pPlanes[plane]->height;
    ov.plane = plane;

    ClearArea(m_pPlanesBackbuffer[plane], &ov);
  
    m_dirtyRect[plane].left = 0;
    m_dirtyRect[plane].top = 0;
    m_dirtyRect[plane].right = m_pPlanes[plane]->width;
    m_dirtyRect[plane].bottom = m_pPlanes[plane]->height;
  }
}

void COverlayRenderer::DrawBitmap(OSDTexture* pOsdTexture, const BD_OVERLAY* pOv)
{
  CAutoLock lock(&m_csRenderLock);
  
  if (!pOsdTexture || !m_pD3DDevice)
    return;

  if (pOv->palette)
    DecodePalette(pOv);

  if (pOv->img)
  {
    IDirect3DTexture9* texture = NULL;

    if (m_pD3DDevice)
    {
      AdjustDirtyRect(pOv->plane, pOv->x, pOv->y, pOv->w, pOv->h);
      
      D3DLOCKED_RECT lockedRect;
    
      HRESULT hr = m_pD3DDevice->CreateTexture(pOv->w, pOv->h, 1, D3DUSAGE_DYNAMIC, D3DFMT_A8R8G8B8, 
                                                D3DPOOL_DEFAULT, &texture, NULL);
    
      if (SUCCEEDED(hr))
      {
        hr = texture->LockRect(0, &lockedRect, NULL, 0);
        if (SUCCEEDED(hr))
        {
          UINT32* dst = (UINT32*)lockedRect.pBits;
          const BD_PG_RLE_ELEM* rlep = pOv->img;
          unsigned pixels = pOv->w * pOv->h;

          // Copy image data to the texture
          if (pOv->img)
          {
            for (unsigned int i = 0; i < pixels; rlep++)
            {
              for (unsigned int j = rlep->len; j > 0; j--)
              {
                if (i > 0 && i % pOv->w == 0)
                  dst += lockedRect.Pitch / 4 - pOv->w;

                *dst = m_palette[rlep->color];
                dst++;
                i++;
              }
            }
          }
          else 
            LogDebug("ovr: DrawBitmap - pOv->img is NULL");

          texture->UnlockRect(0);
        }
        else
          LogDebug("ovr: DrawBitmap LockRect 0x%08x", hr);

        DrawToTexture(pOsdTexture, texture, pOv->x, pOv->y, pOv->w, pOv->h);
      }
      else
        LogDebug("ovr: DrawBitmap - CreateTexture2 0x%08x", hr);
    }
  }
}

void COverlayRenderer::DrawARGBBitmap(OSDTexture* pOsdTexture, const BD_ARGB_OVERLAY* pOv)
{
  CAutoLock lock(&m_csRenderLock);
  
  if (!pOsdTexture || !m_pD3DDevice || !pOv)
    return;

  if (pOv->argb)
  {
    IDirect3DTexture9* texture = NULL;

    if (m_pD3DDevice)
    {
      AdjustDirtyRect(pOv->plane, pOv->x, pOv->y, pOv->w, pOv->h);
      
      D3DLOCKED_RECT lockedRect;
    
      HRESULT hr = m_pD3DDevice->CreateTexture(pOv->w, pOv->h, 1, D3DUSAGE_DYNAMIC, D3DFMT_A8R8G8B8, 
                                                D3DPOOL_DEFAULT, &texture, NULL);

      if (SUCCEEDED(hr))
      {
        hr = texture->LockRect(0, &lockedRect, NULL, 0);
        if (SUCCEEDED(hr))
        {
          if (pOv->argb)
          {
            for(INT i = 0; i < pOv->h; i++)
            {
              DWORD *pDst = (DWORD*)lockedRect.pBits + (lockedRect.Pitch / 4) * i;
              DWORD *pSrc = (DWORD*)pOv->argb + i * pOv->stride;
              memcpy(pDst, pSrc, pOv->w * 4);
            }
          }
          else 
            LogDebug("ovr: DrawBitmap - pOv->argb is NULL");

          texture->UnlockRect(0);
        }
        else
          LogDebug("ovr: DrawBitmap LockRect 0x%08x", hr);

        DrawToTexture(pOsdTexture, texture, pOv->x, pOv->y, pOv->w, pOv->h);
      }
      else
        LogDebug("ovr: DrawBitmap - CreateTexture2 0x%08x", hr);
    }
  }
}

void COverlayRenderer::DrawToTexture(OSDTexture* pOsdTexture, IDirect3DTexture9* pTexture, uint16_t x, uint16_t y, uint16_t w, uint16_t h)
{
  if (!pOsdTexture || !pTexture || !m_pD3DDevice)
    return;

  RECT sourceRect;
  sourceRect.left = 0;
  sourceRect.top = 0;
  sourceRect.right = w;
  sourceRect.bottom = h;

  RECT dstRect;
  dstRect.left = x;
  dstRect.top = y;
  dstRect.right = x + w;
  dstRect.bottom = y + h;

  IDirect3DSurface9* sourceSurface = NULL;
  IDirect3DSurface9* dstSurface = NULL;

  pTexture->GetSurfaceLevel(0, &sourceSurface);
  pOsdTexture->texture->GetSurfaceLevel(0, &dstSurface);

  m_pD3DDevice->StretchRect(sourceSurface, &sourceRect, dstSurface, &dstRect, D3DTEXF_NONE);

  sourceSurface->Release();
  dstSurface->Release();
  pTexture->Release();
}

void COverlayRenderer::DecodePalette(const BD_OVERLAY* ov)
{
  for (unsigned int i = 0; i < PALETTE_SIZE; i++) 
  {
    // Convert the palette to use RGB colors instead of YUV as converting palette is
    // much faster than converting the whole bitmap.
    int T = ov->palette[i].T;
    int B = (int)(1.164 * (ov->palette[i].Y - 16) + 2.018 * (ov->palette[i].Cb - 128));
    int G = (int)(1.164 * (ov->palette[i].Y - 16) - 0.813 * (ov->palette[i].Cr - 128) - 0.391 * (ov->palette[i].Cb - 128));
    int R = (int)(1.164 * (ov->palette[i].Y - 16) + 1.596 * (ov->palette[i].Cr - 128));
    if (B < 0) B = 0; if (B > 255) B = 255;
    if (G < 0) G = 0; if (G > 255) G = 255;
    if (R < 0) R = 0; if (R > 255) R = 255; 
  
    R = (R * T) / 255;
    G = (G * T) / 255;
    B = (B * T) / 255;
    m_palette[i] = R << 16 | G << 8 | B | T << 24;
  }
}

void COverlayRenderer::CopyToFrontBuffer(uint8_t plane, bool ARGB)
{
  if (!m_pD3DDevice || !m_pPlanes[plane])
    return;

  IDirect3DSurface9* sourceSurface = NULL;
  IDirect3DSurface9* dstSurface = NULL;

  HRESULT hr = S_FALSE;

  if (ARGB)
    hr = m_pARGBTextures[BD_OVERLAY_IG]->GetSurfaceLevel(0, &sourceSurface);
  else
    hr = m_pPlanesBackbuffer[plane]->texture->GetSurfaceLevel(0, &sourceSurface);

  if (FAILED(hr))
  {
    LogDebug("ovr: CopyToFrontBuffer - GetSurfaceLevel(1): 0x%08x");
    return;
  }
    
  hr = m_pPlanes[plane]->texture->GetSurfaceLevel(0, &dstSurface);

  if (FAILED(hr))
  {
    sourceSurface->Release();
    LogDebug("ovr: CopyToFrontBuffer - GetSurfaceLevel(2): 0x%08x");
    return;
  }

  hr = m_pD3DDevice->StretchRect(sourceSurface, &m_dirtyRect[plane], dstSurface, &m_dirtyRect[plane], D3DTEXF_NONE);

  if (FAILED(hr))
    LogDebug("ovr: CopyToFrontBuffer - StretchRect: 0x%08x");

  sourceSurface->Release();
  dstSurface->Release();

  ResetDirtyRect(plane);
}

void COverlayRenderer::ResetDirtyRect(const uint8_t plane, uint16_t h, uint16_t w)
{
  m_dirtyRect[plane].bottom = 0;
  m_dirtyRect[plane].top = h;
  m_dirtyRect[plane].left = w;
  m_dirtyRect[plane].right = 0;
}

void COverlayRenderer::ResetDirtyRect(const uint8_t plane)
{
  if (!m_pPlanes[plane])
  {
    LogDebug("ovr: ResetDirtyRect - no plane: %d", plane);
    return;
  }

  m_dirtyRect[plane].bottom = 0;
  m_dirtyRect[plane].top = m_pPlanes[plane]->height;
  m_dirtyRect[plane].left = m_pPlanes[plane]->width;
  m_dirtyRect[plane].right = 0;
}

void COverlayRenderer::AdjustDirtyRect(const uint8_t plane, uint16_t x, uint16_t y, uint16_t w, uint16_t h)
{
  if (m_dirtyRect[plane].left > x)
    m_dirtyRect[plane].left = x;
  
  if (m_dirtyRect[plane].right < x + w)
    m_dirtyRect[plane].right = x + w;
  
  if (m_dirtyRect[plane].top > y)
    m_dirtyRect[plane].top = y;

  if (m_dirtyRect[plane].bottom < y + h)
    m_dirtyRect[plane].bottom = y + h;
}

void COverlayRenderer::LogCommand(const BD_OVERLAY* ov)
{
#ifdef LOG_DRAWING
  LogDebug("ovr: %s x: %4d y: %4d w: %4d h: %4d plane: %1d pts: %d", CommandAsString(ov->cmd),
    ov->x, ov->y, ov->w, ov->h, ov->plane, ov->pts);
#endif 
}

void COverlayRenderer::LogARGBCommand(const BD_ARGB_OVERLAY* ov)
{
#ifdef LOG_DRAWING
  LogDebug("ovr: %s x: %4d y: %4d w: %4d h: %4d stride: %4d plane: %i pts: %i", ARGBCommandAsString(ov->cmd),
    ov->x, ov->y, ov->w, ov->h, ov->stride, ov->plane, ov->pts);
#endif 
}

char* COverlayRenderer::CommandAsString(int cmd)
{
  switch (cmd)
  {
    case BD_OVERLAY_INIT:
      return "BD_OVERLAY_INIT ";
    case BD_OVERLAY_CLOSE:
      return "BD_OVERLAY_CLOSE";
    case BD_OVERLAY_DRAW:
      return "BD_OVERLAY_DRAW ";
    case BD_OVERLAY_WIPE:
      return "BD_OVERLAY_WIPE ";
    case BD_OVERLAY_CLEAR:
      return "BD_OVERLAY_CLEAR";
    case BD_OVERLAY_FLUSH:
      return "BD_OVERLAY_FLUSH";
    case BD_OVERLAY_HIDE:
     return "BD_OVERLAY_HIDE";
    default:
      return "UNKNOWN         ";
  }
}

char* COverlayRenderer::ARGBCommandAsString(int cmd)
{
  switch (cmd)
  {
    case BD_ARGB_OVERLAY_INIT:
      return "BD_OVERLAY_INIT ";
    case BD_ARGB_OVERLAY_CLOSE:
      return "BD_OVERLAY_CLOSE";
    case BD_ARGB_OVERLAY_DRAW:
      return "BD_OVERLAY_DRAW ";
    case BD_ARGB_OVERLAY_FLUSH:
      return "BD_OVERLAY_FLUSH";
    default:
      return "UNKNOWN         ";
  }
}
