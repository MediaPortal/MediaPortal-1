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

extern void LogDebug(const char *fmt, ...);

COverlayRenderer::COverlayRenderer(CLibBlurayWrapper* pLib) :
  m_pLib(pLib),
  m_pD3DDevice(NULL)
{
  m_pPlanes[BD_OVERLAY_PG] = NULL;
  m_pPlanes[BD_OVERLAY_IG] = NULL;
}

COverlayRenderer::~COverlayRenderer()
{
  if (m_pPlanes[BD_OVERLAY_PG] && m_pPlanes[BD_OVERLAY_IG]->texture)
    m_pPlanes[BD_OVERLAY_PG]->texture->Release();

  if (m_pPlanes[BD_OVERLAY_IG] && m_pPlanes[BD_OVERLAY_IG]->texture)
    m_pPlanes[BD_OVERLAY_IG]->texture->Release();
}

void COverlayRenderer::SetD3DDevice(IDirect3DDevice9* device)
{
  m_pD3DDevice = device;

  BD_OVERLAY ov;
  ov.x = 0;
  ov.y = 0;
  ov.w = 1920;
  ov.h = 1080;
  ov.plane = BD_OVERLAY_PG;

  OpenOverlay(&ov);
  ClearArea(m_pPlanes[BD_OVERLAY_PG], &ov);

  ov.plane = BD_OVERLAY_IG;

  OpenOverlay(&ov);
  ClearArea(m_pPlanes[BD_OVERLAY_IG], &ov);
}

void COverlayRenderer::OverlayProc(const BD_OVERLAY* ov)
{
  if (!ov)
  {
    CloseOverlay(-1);
    return;
  }
  else

  if (ov->plane > BD_OVERLAY_IG) 
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

  OSDTexture* plane = m_pPlanes[ov->plane];

  if (!plane)
    return; 

  switch (ov->cmd) 
  {
    case BD_OVERLAY_DRAW:
      DrawBitmap(plane, ov);
      break;

    case BD_OVERLAY_WIPE:
      ClearArea(plane, ov);
      break;

    case BD_OVERLAY_CLEAR:
      ClearOverlay();
      break;

    case BD_OVERLAY_FLUSH:
      m_pLib->HandleOSDUpdate(*plane);

      if (ov->plane == BD_OVERLAY_IG) 
        m_pLib->HandleMenuStateChange(true);
      
      break;

    default:
      break;
  }
}

void COverlayRenderer::OpenOverlay(const BD_OVERLAY* pOv)
{
  if (!m_pPlanes[pOv->plane])
  {
    OSDTexture* osdTexture = new OSDTexture;
    osdTexture->height = pOv->h;
    osdTexture->width = pOv->w;
    osdTexture->x = pOv->x;
    osdTexture->y = pOv->y;
    osdTexture->texture = NULL;

    HRESULT hr = m_pD3DDevice->CreateTexture(osdTexture->width, osdTexture->height, 1, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, 
                                              D3DPOOL_DEFAULT, &osdTexture->texture, NULL);

    if (hr == S_OK)
      m_pPlanes[pOv->plane] = osdTexture;
    else
      LogDebug("ovr: OpenOverlay - CreateTexture 0x%08x", hr);
  }
}

void COverlayRenderer::CloseOverlay(const int pPlane)
{
  if (pPlane < 0) 
  {
    CloseOverlay(0);
    CloseOverlay(1);
    return;
  }

  if (pPlane < 2 && m_pPlanes[pPlane]) 
  {
    // TODO: clear specific plane
    ClearOverlay();

    if (pPlane == 1) 
      m_pLib->HandleMenuStateChange(false);
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
    }
    else
      LogDebug("ovr: ClearArea - GetSurfaceLevel failed: 0x%08x");
  }
}

void COverlayRenderer::ClearOverlay()
{
  OSDTexture nullTexture;
  nullTexture.height = 0;
  nullTexture.width = 0;
  nullTexture.x = 0;
  nullTexture.y = 0;
  nullTexture.texture = NULL;

  m_pLib->HandleOSDUpdate(nullTexture);
}

void COverlayRenderer::DrawBitmap(OSDTexture* pPlane, const BD_OVERLAY* pOv)
{
  if (!pPlane)
    return;

  if (pOv->palette)
    DecodePalette(pOv);

  if (pOv->img)
  {
    IDirect3DTexture9* texture = NULL;

    if (m_pD3DDevice)
    {
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

        RECT sourceRect;
        sourceRect.left = 0;
        sourceRect.top = 0;
        sourceRect.right = pOv->w;
        sourceRect.bottom = pOv->h;

        RECT dstRect;
        dstRect.left = pOv->x;
        dstRect.top = pOv->y;
        dstRect.right = pOv->x + pOv->w;
        dstRect.bottom = pOv->y + pOv->h;

        IDirect3DSurface9* sourceSurface = NULL;
        IDirect3DSurface9* dstSurface = NULL;

        texture->GetSurfaceLevel(0, &sourceSurface);
        pPlane->texture->GetSurfaceLevel(0, &dstSurface);

        m_pD3DDevice->StretchRect(sourceSurface, &sourceRect, dstSurface, &dstRect, D3DTEXF_NONE);

        sourceSurface->Release();
        dstSurface->Release();
        texture->Release();
      }
      else
        LogDebug("ovr: DrawBitmap - CreateTexture2 0x%08x", hr);
    }
  }
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

void COverlayRenderer::LogCommand(const BD_OVERLAY* ov)
{
  LogDebug("ovr: %s x: %4d y: %4d w: %4d h: %4d plane: %1d pts: %d", CommandAsString(ov->cmd),
    ov->x, ov->y, ov->w, ov->h, ov->plane, ov->pts);
}

LPCTSTR COverlayRenderer::CommandAsString(int pCmd)
{
	switch (pCmd)
	{
    case BD_OVERLAY_INIT:
      return _T("BD_OVERLAY_INIT ");
    case BD_OVERLAY_CLOSE:
      return _T("BD_OVERLAY_CLOSE");    
    case BD_OVERLAY_DRAW:
      return _T("BD_OVERLAY_DRAW ");
    case BD_OVERLAY_WIPE:
      return _T("BD_OVERLAY_WIPE ");
    case BD_OVERLAY_CLEAR:
      return _T("BD_OVERLAY_CLEAR");
    case BD_OVERLAY_FLUSH:
      return _T("BD_OVERLAY_FLUSH");
  	default:
	  	return _T("UNKNOWN");
	}
}