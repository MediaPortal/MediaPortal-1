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
#include <streams.h>
#include <D3d9.h>
#include "LibBlurayWrapper.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define LOG_OVERLAY_COMMANDS

extern void LogDebug(const char *fmt, ...);

COverlayRenderer::COverlayRenderer(CLibBlurayWrapper* pLib) :
  m_pLib(pLib),
  m_pD3DDevice(NULL)
{
  m_pPlanes[BD_OVERLAY_PG] = NULL;
  m_pPlanes[BD_OVERLAY_IG] = NULL;
  
  m_pPlanesBackbuffer[BD_OVERLAY_PG] = NULL;
  m_pPlanesBackbuffer[BD_OVERLAY_IG] = NULL;
}

COverlayRenderer::~COverlayRenderer()
{
  CloseOverlay(BD_OVERLAY_PG);
  CloseOverlay(BD_OVERLAY_IG);
}

void COverlayRenderer::SetD3DDevice(IDirect3DDevice9* device)
{
  m_pD3DDevice = device;
}

void COverlayRenderer::OverlayProc(const BD_OVERLAY* ov)
{
  if (!ov)
  {
    CloseOverlay(BD_OVERLAY_IG);
    return;
  }
  else if (ov->plane > BD_OVERLAY_IG)
    return;

  LogCommand(ov);

  switch (ov->cmd) 
  {
    case BD_OVERLAY_INIT:
      OpenOverlay(ov);
      return;
    case BD_OVERLAY_CLOSE:
      CloseOverlay(ov->plane);
      return;
  }

  OSDTexture* plane = m_pPlanesBackbuffer[ov->plane];

  // Workaround for some BDs that wont issue BD_OVERLAY_INIT after BD_OVERLAY_CLOSE
  if (!plane) 
  {
    OpenOverlay(ov);
    plane = m_pPlanesBackbuffer[ov->plane];

    if (!plane)
      return;
  }

  switch (ov->cmd) 
  {
    case BD_OVERLAY_DRAW:
      DrawBitmap(plane, ov);
      break;

    case BD_OVERLAY_WIPE:
      ClearArea(plane, ov);
      break;

    case BD_OVERLAY_CLEAR:
      ClearOverlay(ov->plane);
      break;

    case BD_OVERLAY_FLUSH:
    {
      CopyToFrontBuffer(ov->plane);
      
      OSDTexture* plane = m_pPlanes[ov->plane];
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
      DrawARGBBitmap(pOsdTexture, ov);
      break;      
    case BD_ARGB_OVERLAY_FLUSH:
      {
        CopyToFrontBuffer(ov->plane);
      
        OSDTexture* plane = m_pPlanes[ov->plane];
        m_pLib->HandleOSDUpdate(*plane);
      }
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
  if (m_pPlanes[plane])
  {
    ClearOverlay(plane);
    CopyToFrontBuffer(plane);

    OSDTexture nullTexture = {0};
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
}

void COverlayRenderer::ClearArea(OSDTexture* pPlane, const BD_OVERLAY* pOv)
{
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

void COverlayRenderer::CopyToFrontBuffer(uint8_t plane)
{
  if (!m_pD3DDevice || !m_pPlanes[plane])
    return;

  IDirect3DSurface9* sourceSurface = NULL;
  IDirect3DSurface9* dstSurface = NULL;

  HRESULT hr = S_FALSE;

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

  m_pD3DDevice->StretchRect(sourceSurface, &m_dirtyRect[plane], dstSurface, &m_dirtyRect[plane], D3DTEXF_NONE);

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
#ifdef LOG_OVERLAY_COMMANDS
  LogDebug("ovr: %s x: %4d y: %4d w: %4d h: %4d plane: %1d pts: %d", CommandAsString(ov->cmd),
    ov->x, ov->y, ov->w, ov->h, ov->plane, ov->pts);
#endif 
}

void COverlayRenderer::LogARGBCommand(const BD_ARGB_OVERLAY* ov)
{
#ifdef LOG_OVERLAY_COMMANDS
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
